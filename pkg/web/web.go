package web

// opinionated web core for dg projects.
import (
	"encoding/base32"
	"encoding/json"

	"fmt"
	"io"
	"log"
	"mime"
	"net/http"
	"os"
	"path/filepath"
	"strings"
	"time"

	"github.com/gorilla/mux"
	"github.com/gorilla/securecookie"
	"github.com/gorilla/sessions"
	"github.com/gorilla/websocket"
	"github.com/pkg/browser"
	"github.com/prometheus/client_golang/prometheus/promhttp"
	"github.com/rs/cors"
)

// each app is going to have some shared state and some client state, and potentially some cluster/consensus state

type WebAppOptions struct {
	Home      string
	WriteHome string
	Port      string
	CertPem   string
	KeyPem    string
}

// it might be best to generate rpc's?
type WebApp interface {
	Connect(cl WebAppClient) Peer
}
type WebAppClient interface {
	Peer
}

type SecretCookie = string
type SessionId = string

var errFetch = fmt.Errorf("fetch error")

//var sessCount = expvar.NewInt("sessions")

func foo() {
	http.Handle("/metrics", promhttp.Handler())
}

func randomString(sz int) string {
	return strings.TrimRight(base32.StdEncoding.EncodeToString(
		securecookie.GenerateRandomKey(sz)), "=")
}

const (
	// Time allowed to write a message to the peer.
	writeWait = 10 * time.Second

	// Time allowed to read the next pong message from the peer.
	pongWait = 60 * time.Second

	// Send pings to peer with this period. Must be less than pongWait.
	pingPeriod = (pongWait * 9) / 10

	// Maximum message size allowed from peer.
	maxMessageSize = 512
)

var SECURE_KEY = securecookie.GenerateRandomKey(32)

var store = sessions.NewCookieStore(SECURE_KEY)

// allows us to serve SPA
type spaHandler struct {
	staticPath string
	indexPath  string
}

var ErrSsh = fmt.Errorf("ssh error")

// a static file handler for SPA
func (h spaHandler) ServeHTTP(w http.ResponseWriter, r *http.Request) {
	session, _ := store.Get(r, "_")
	if session.ID == "" {
		session.ID = strings.TrimRight(base32.StdEncoding.EncodeToString(
			securecookie.GenerateRandomKey(32)), "=")
		err := store.Save(r, w, session)
		if err != nil {
			log.Println(err)
			return
		}
	}

	path := filepath.Join(h.staticPath, r.URL.Path)
	// this is a waste if the last character is / since it will never exist as a file
	_, err := os.Stat(path)
	if os.IsNotExist(err) {
		// this serves the index file if the file we are looking for
		// doesn't existads
		http.ServeFile(w, r, filepath.Join(h.staticPath, h.indexPath))
		return
	} else if err != nil {
		//log.Printf("\nfile missing %s,%s", path, err.Error())
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	if filepath.Ext(path) == ".js" {
		w.Header().Add("Content-type", "application/javascript")
	}
	// this serves the file if it does exist.
	//log.Printf("\nserving file %s", h.staticPath)
	http.FileServer(http.Dir(h.staticPath)).ServeHTTP(w, r)
}

type Config struct {
	Websocket string `json:"websocket,omitempty"`
	Drop      string `json:"drop,omitempty"`
}

type NewWebClient = func(w WebAppClient) (Peer, error)

func DefaultOptions() *WebAppOptions {
	mydir, _ := os.Getwd()
	return &WebAppOptions{
		Home: mydir,
		Port: ":5174",
	}
}

var newWebClient NewWebClient

func Run(new NewWebClient, s ...*WebAppOptions) {
	newWebClient = new
	var opt *WebAppOptions
	if len(s) > 0 {
		opt = s[0]
	} else {
		opt = DefaultOptions()
	}
	mime.AddExtensionType(".js", "application/javascript")
	r := mux.NewRouter()
	r.Handle("/debug/vars", http.DefaultServeMux)
	r.Use(func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			log.Println(r.RequestURI)
			next.ServeHTTP(w, r)
		})
	})

	// register other functions here
	r.HandleFunc("/ws", func(w http.ResponseWriter, r *http.Request) {
		serveWs(w, r)
	})
	r.HandleFunc("/fetch", func(w http.ResponseWriter, r *http.Request) {
		serveFetch(w, r)
	})
	r.HandleFunc("/write", func(w http.ResponseWriter, r *http.Request) {
		serveWrite(w, r)
	})

	spa := spaHandler{staticPath: opt.Home, indexPath: filepath.Join(opt.Home, "index.html")}
	r.PathPrefix("/").Handler(spa)
	c := cors.New(cors.Options{
		AllowedOrigins:   []string{"*"},
		AllowCredentials: true,
	})
	r2 := c.Handler(r)

	err := browser.OpenURL("http://localhost" + opt.Port)
	if err != nil {
		log.Print(err)
	}
	if len(opt.CertPem) > 0 {
		log.Fatal(http.ListenAndServeTLS(opt.Port, opt.CertPem, opt.KeyPem, r2))
	} else {
		log.Fatal(http.ListenAndServe(opt.Port, r2))
	}
}

func Json(a any) []byte {
	b, _ := json.Marshal(a)
	return b
}

var upgrader2 = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
}

type Rpc struct {
	Method  string          `json:"method,omitempty"`
	Id      int64           `json:"id,omitempty"`
	Channel uint64          `json:"channel,omitempty"`
	Params  json.RawMessage `json:"params,omitempty"`
}
type RpcReply struct {
	Id     int64           `json:"id,omitempty"`
	Result json.RawMessage `json:"result,omitempty"`
	Error  string          `json:"error,omitempty"`
}

// upload a file
func serveWrite(w http.ResponseWriter, r *http.Request) {
	var err error = errFetch

	vars := r.URL.Query()
	file, ok1 := vars["f"]
	sess, ok2 := vars["s"]
	if !ok2 || sess[0] == "" || file[0] == "" || !ok1 {
		http.Error(w, err.Error(), http.StatusInternalServerError)
		return
	}

	r.ParseMultipartForm(32 << 20)
	fileup, handler, err := r.FormFile("file")
	if err != nil {
		log.Println(err)
		return
	}
	nm := handler.Filename + "." + randomString(8)
	name := file[0] + "/" + nm

	b, e := io.ReadAll(fileup)
	if e != nil {
		return
	}
	log.Printf("Upload %s", name)
	_ = b
	if e != nil {
		log.Println(e)
		http.Error(w, e.Error(), http.StatusInternalServerError)
	}
	w.WriteHeader(http.StatusOK)
	w.Write([]byte(nm))
}

// requests for files from the client look like this.
func serveFetch(w http.ResponseWriter, r *http.Request) {
	r.URL.Query()
	mb := []byte{}

	w.WriteHeader(http.StatusOK)
	w.Write(mb)

}

// serveWs handles websocket requests from the peer.
func serveWs(w http.ResponseWriter, r *http.Request) {
	var err error = errFetch
	r.URL.Query()

	conn, err := upgrader2.Upgrade(w, r, nil)
	if err != nil {
		log.Println(err)
		return
	}
	randomid := func() string {
		return strings.TrimRight(base32.StdEncoding.EncodeToString(
			securecookie.GenerateRandomKey(32)), "=")
	}
	c := &Client{
		id:       randomid(),
		conn:     conn,
		open:     []string{},
		writable: []uint8{},
		send:     make(chan []byte, 256),
	}

	c2, _ := newWebClient(c)
	go func() {
		// this defer will make sure that the ssh is closed when the websocket
		// is closed
		defer func() {
			c.Close()
		}()
		c.conn.SetReadLimit(maxMessageSize)
		c.conn.SetReadDeadline(time.Now().Add(pongWait))
		c.conn.SetPongHandler(func(string) error { c.conn.SetReadDeadline(time.Now().Add(pongWait)); return nil })

		// return true if connection is good, false if websocket is closed.
		var m Rpc
		again := func() bool {
			_, message, err := c.conn.ReadMessage()
			if err != nil {
				if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
					log.Printf("error: %v", err)
				}
				return false // breaks out of loop
			}
			json.Unmarshal(message, &m)
			if m.Id > 0 {
				a, _, e := c2.Rpc(m.Method, m.Params, nil)
				if e != nil {
					mb, _ := json.Marshal(&RpcReply{
						Id:    m.Id,
						Error: e.Error(),
					})
					c.send <- mb
					return true
				} else {
					mbx, _ := json.Marshal(a)
					mb, _ := json.Marshal(&RpcReply{
						Id:     m.Id,
						Result: mbx, // returns a channel, not used currently
					})
					c.send <- mb
				}
			} else {
				c2.Notify(m.Method, m.Params, nil)
			}
			return true
		}
		for again() {
		}

	}()
	go func() {
		for {
			ticker := time.NewTicker(pingPeriod)
			defer func() {
				ticker.Stop()
				c.conn.Close()
			}()
			for {
				select {
				case s, ok := <-c.send:
					c.conn.SetWriteDeadline(time.Now().Add(writeWait))
					if !ok {
						c.conn.WriteMessage(websocket.CloseMessage, []byte{})
						return
					}
					w, err := c.conn.NextWriter(websocket.TextMessage)
					if err != nil {
						return
					}
					w.Write(s)
					if err := w.Close(); err != nil {
						return
					}
				case <-ticker.C:
					c.conn.SetWriteDeadline(time.Now().Add(writeWait))
					if err := c.conn.WriteMessage(websocket.PingMessage, nil); err != nil {
						return
					}
				}
			}

		}
	}()

}
