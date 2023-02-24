package main

import (
	"embed"
	"encoding/json"
	"fmt"
	"log"
	"os"
	"path"
	"sync"

	"github.com/datagrove/datagrove/pkg/web"
	"github.com/davecgh/go-spew/spew"
	"github.com/joho/godotenv"
	"github.com/spf13/cobra"
	//"github.com/xeipuuv/gojsonschema"
)

// compare jsonschema https://dev.to/vearutop/benchmarking-correctness-and-performance-of-go-json-schema-validators-3247
// https://github.com/santhosh-tekuri/jsonschema

var (
	//go:embed dist
	res embed.FS
)

// load the configuration from the opt.home, watch and reconfigure if the file changes.
// the web server can separately broadcast these out if configured to.

// move back to cmd? if composing command app belongs here though
// embed the
func start() *cobra.Command {
	port := 5174
	r := &cobra.Command{
		Use: "start [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			// default to current directory or first fixed position
			mydir, _ := os.Getwd()
			if len(args) > 0 {
				mydir = args[0]
			}

			app, e := NewDbDeli(mydir)
			if e != nil {
				log.Fatal(e)
			}

			// called on each socket connection
			// called once to create a guest connection
			NewCheckoutClient := func(m web.Server, browser web.Peer) (web.Peer, error) {
				r := &CheckoutClient{
					Deli:    app,
					Server:  m,
					Browser: browser,
				}
				app.Mu.Lock()
				defer app.Mu.Unlock()
				b, _ := json.Marshal(&app.State)
				// a nil browser is http guest
				if browser != nil {
					spew.Dump(b)
					browser.Rpc("update", b, nil, 0)
				}
				return r, nil
			}

			// configure can be  called outside the context of a client
			// for example on startup and when a watched configuration file changes.
			// the configuration is automatically published to "all"
			configure := func(m []byte) error {
				return app.Configure(m)
			}

			web.Run(&web.Options{
				New:       NewCheckoutClient,
				Port:      port,
				Configure: configure,
				Fs:        res,
				Home:      mydir,
			})
		},
	}
	r.Flags().IntVarP(&port, "port", "p", 5174, "port")
	return r
}

// state that is shared with the browser
type SharedState struct {
	Options     DbDeliOptions           `json:"options"`
	Sku         map[string]ConfigureSku `json:"sku"`
	Reservation map[string]Reservation  `json:"reservation"`
	Db          map[string]*Driver      `json:"db"`
}

// not used; global options (not sku options)
type DbDeliOptions struct {
}

// the server can stream a []Reservation list on a websocket for monitoring
type Reservation struct {
	Sku         string `json:"sku,omitempty"` // each database has a unique label
	Ticket      int    `json:"ticket,omitempty"`
	Description string `json:"description,omitempty"`
}

type ConfigureSku struct {
	Limit    int    `json:"limit,omitempty"`
	Database string `json:"database,omitempty"`
	Backup   string `json:"backup,omitempty"`
	Db       string `json:"db,omitempty"`
}

// server state.
type DbDeli struct {
	State   SharedState
	Mu      sync.Mutex
	Home    string
	Sku     map[string]*SkuState
	Drivers map[string]Dbp
}
type SkuState struct {
	waiting []Promise
}

// not used currently
func (d *DbDeli) Configure(m []byte) error {
	var opt DbDeliOptions
	json.Unmarshal(m, &opt)
	d.Mu.Lock()
	d.State.Options = opt
	d.Mu.Unlock()
	return nil
}

// we can load and then watch the configuration file for changes
// should we use cobra for this?
// main error here is no config file,
func NewDbDeli(home string) (*DbDeli, error) {
	// read the current shared state.
	var v SharedState
	b, e := os.ReadFile(path.Join(home, "shared.json"))
	if e != nil {
		return nil, e
	}
	v.Reservation = map[string]Reservation{}
	json.Unmarshal(b, &v)

	// we should syntax check our json here!
	// schemaLoader := gojsonschema.NewReferenceLoader("file:///home/me/schema.json")

	// result, err := gojsonschema.Validate(schemaLoader, documentLoader)
	// if err != nil {
	// 	panic(err.Error())
	// }

	// if result.Valid() {
	// 	fmt.Printf("The document is valid\n")
	// } else {
	// 	fmt.Printf("The document is not valid. see errors :\n")
	// 	for _, desc := range result.Errors() {
	// 		fmt.Printf("- %s\n", desc)
	// 	}
	// }

	r := &DbDeli{
		State: v,
		Mu:    sync.Mutex{},
		Home:  home,
		Sku:   map[string]*SkuState{},
		Drivers: map[string]Dbp{
			"mssql": NewMsSql(v.Db["mssql"]),
		},
	}
	for k := range v.Sku {
		r.Sku[k] = &SkuState{
			waiting: []Promise{},
		}
	}
	return r, nil
}

type Promise struct {
	peer *CheckoutClient
	tag  int64
	sku  string
	desc string
}

// uses datagrove basic web app.

// based on an active websocket connection to the web front end
type CheckoutClient struct {
	// what do I need to
	Deli    *DbDeli
	Server  web.Server
	Browser web.Peer
}

// not used, we  don't call the client.
func (*CheckoutClient) Reply(tag int64, result any, more []byte, err error) {
	panic("unimplemented")
}

var _ web.Peer = (*CheckoutClient)(nil)

func (s *CheckoutClient) publish() {
	b, _ := json.Marshal(&s.Deli.State)
	s.Server.Publish("update", b, nil)
}

func (s *DbDeli) reserve(sku, desc string, tag int) error {
	cf, ok := s.State.Sku[sku]
	if !ok || tag >= cf.Limit {
		return fmt.Errorf("bad sku %s,%d", sku, tag)
	}
	db := fmt.Sprintf("%s_%d", sku, tag)
	driver := s.Drivers[cf.Db]
	return driver.Restore(db)
}

func (s *CheckoutClient) reserve(sku, desc string, tag int64) bool {
	cf, ok := s.Deli.State.Sku[sku]
	if !ok {
		s.Browser.Reply(tag, nil, nil, fmt.Errorf("bad sku %s", sku))
		return true
	}
	for i := 0; i < cf.Limit; i++ {
		leaseKey := fmt.Sprintf("%s~%d", sku, i)
		if _, ok := s.Deli.State.Reservation[leaseKey]; !ok {
			s.Deli.State.Reservation[leaseKey] = Reservation{
				Sku:         sku,
				Ticket:      i,
				Description: desc,
			}
			// recover the snapshot
			db := fmt.Sprintf("%s_%d", sku, i)
			driver := s.Deli.Drivers[cf.Db]
			e := driver.Restore(db)
			if e != nil {
				log.Fatal(e)
			}
			s.Browser.Reply(tag, i, nil, nil)
			s.publish()
			return true
		}
	}
	return false // suspend
}

// when we block on the semaphore we can't hold the global locks.
// so return nil or a semaphore. if a semaphore, wait on it, then retry the reservation?
// if we return a bool to mean bl
func (s *CheckoutClient) Rpc(method string, params []byte, data []byte, tag int64) {
	//withlock := func() *semaphore.Weighted {
	s.Deli.Mu.Lock()
	defer s.Deli.Mu.Unlock()

	var v struct {
		Sku         string `json:"sku,omitempty"`
		Description string `json:"description,omitempty"`
		Ticket      int
	}
	json.Unmarshal(params, &v)

	// return false to suspend

	switch method {
	case "update":
		// nothing, just fall through to publish
	case "reserve":
		if !s.reserve(v.Sku, v.Description, tag) {
			// no available database, push a promise.
			cg := s.Deli.Sku[v.Sku]
			cg.waiting = append(cg.waiting, Promise{
				peer: s,
				tag:  tag,
				sku:  v.Sku,
				desc: v.Description,
			})
		}
	case "release":
		leaseKey := fmt.Sprintf("%s~%d", v.Sku, v.Ticket)
		if _, ok := s.Deli.State.Reservation[leaseKey]; ok {
			delete(s.Deli.State.Reservation, leaseKey)
			cg := s.Deli.Sku[v.Sku]
			if len(cg.waiting) > 0 {
				pr := cg.waiting[len(cg.waiting)-1]
				cg.waiting = cg.waiting[0 : len(cg.waiting)-1]
				pr.peer.reserve(pr.sku, pr.desc, pr.tag)
			}
		}
		s.publish()
	}

}

func main() {
	godotenv.Load()
	var rootCmd = &cobra.Command{
		Use: "dbdeli [sub]",
	}

	rootCmd.AddCommand(start())
	rootCmd.AddCommand(build())
	rootCmd.AddCommand(restore())

	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}
