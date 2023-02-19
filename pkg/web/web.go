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

type WebAppOptions struct {
	Home      string
	WriteHome string
	Port      string
	CertPem   string
	KeyPem    string
}

type Peer interface {
	// primary need is for a way to read/write rpc's
	// json, cbor, arrow
	// {json rpc}\0binary
	// if first character is 0 then begin with cbor
	io.Closer
	Rpc(method string, params []byte) ([]byte, error)
	Notify(method string, params []byte)
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
func Run(new NewWebClient, s ...*WebAppOptions) {
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

func (c *Client) Close() {
	// close the websocket, its probably closed already though
	c.conn.Close()
}

func Json(a any) []byte {
	b, _ := json.Marshal(a)
	return b
}

// wrapper for websocket, should we make generic?
type Client struct {
	id       string
	conn     *websocket.Conn
	open     []string
	writable []uint8     // 1 = writable, 2=subscribed
	send     chan []byte // update a batch of logs
}

var upgrader2 = websocket.Upgrader{
	CheckOrigin: func(r *http.Request) bool {
		return true
	},
}

type Rpc struct {
	Method  string          `json:"method,omitempty"`
	Id      string          `json:"id,omitempty,string"`
	Channel uint64          `json:"channel,omitempty"`
	Params  json.RawMessage `json:"params,omitempty"`
}
type RpcReply struct {
	Id     string          `json:"id,omitempty,string"`
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
			log.Printf("%s,%v", string(message), err)
			if err != nil {
				if websocket.IsUnexpectedCloseError(err, websocket.CloseGoingAway, websocket.CloseAbnormalClosure) {
					log.Printf("error: %v", err)
				}

				return false
			}
			json.Unmarshal(message, &m)
			jsonErr := func(e error) bool {
				if e != nil {
					mb, _ := json.Marshal(&RpcReply{
						Id:    m.Id,
						Error: e.Error(),
					})
					c.send <- mb
					return true
				} else {
					return false
				}
			}
			jsonOk := func(a []byte) {
				mb, _ := json.Marshal(&RpcReply{
					Id:     m.Id,
					Result: a, // returns a channel, not used currently
				})
				c.send <- mb
			}
			jsonReturn := func(a any, e error) {
				if e != nil {
					jsonErr(e)
				} else {
					mb, _ := json.Marshal(a)
					jsonOk(mb)
				}
			}
			_ = jsonReturn

			switch m.Method {
			case "open":
				jsonOk([]byte("0"))
				return true
			case "close":
				// not used
				return true
			case "logout":
				jsonReturn(1, nil)
				return true
			case "login":
				// this is a login message
				// return the databases if successful
				// most recent is kept in localStorage
				var opt struct {
					Server   string `json:"server,omitempty"`
					User     string `json:"user,omitempty"`
					Password string `json:"password,omitempty"`
					Totp     string `json:"totp,omitempty"`
					Drop     string `json:"drop,omitempty"`
				}
				json.Unmarshal(m.Params, &opt)

				if err == nil {
					log.Printf("Login: %s %s\n", opt.User, c.id)
				} else {
					log.Printf("Login failed %s %s", opt.User, err.Error())
				}

				jsonReturn(c.id, err)
				return true
			}

			switch m.Method {

			case "hello":
				var opt struct {
					Db string `json:"db,omitempty"`
				}
				e := json.Unmarshal(m.Params, &opt)
				if jsonErr(e) {
					return false
				}
				if e != nil {
					jsonErr(e)
					return false
				}
				jsonReturn([]byte{}, nil)
				return true
			default:
				log.Printf("\nUnknown method %s\n", m.Method)
				return false
			}
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
