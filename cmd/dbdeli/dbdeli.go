package main

import (
	"embed"
	"encoding/json"
	"fmt"
	"os"
	"path"
	"sync"

	"github.com/datagrove/datagrove/pkg/web"
	"github.com/davecgh/go-spew/spew"
	"github.com/joho/godotenv"
	"github.com/spf13/cobra"
)

var (
	//go:embed dist
	res embed.FS
)

func main() {
	godotenv.Load()
	var rootCmd = &cobra.Command{
		Use: "dbdeli [sub]",
	}

	rootCmd.AddCommand(New())

	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}

// load the configuration from the opt.home, watch and reconfigure if the file changes.
// the web server can separately broadcast these out if configured to.

// move back to cmd? if composing command app belongs here though
// embed the
func New() *cobra.Command {
	port := 5174
	r := &cobra.Command{
		Use: "start [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			// default to current directory or first fixed position
			mydir, _ := os.Getwd()
			if len(args) > 1 {
				mydir = args[1]
			}

			app := NewDbDeli(mydir)

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
					browser.Rpc("update", b, nil)
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
	Drivers     []string                `json:"drivers"`
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
	Limit        int    `json:"limit,omitempty"`
	Database     string `json:"database,omitempty"`
	DatabaseType string `json:"database_type,omitempty"`
}

// server state.
type DbDeli struct {
	State SharedState
	Mu    sync.Mutex
	Home  string
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
func NewDbDeli(home string) *DbDeli {
	// read the current shared state.
	var v SharedState
	b, _ := os.ReadFile(path.Join(home, "shared.json"))
	v.Reservation = map[string]Reservation{}
	json.Unmarshal(b, &v)
	v.Drivers = []string{"mssql"}
	return &DbDeli{
		Home:  home,
		State: v,
		Mu:    sync.Mutex{},
	}
}

// uses datagrove basic web app.

// based on an active websocket connection to the web front end
type CheckoutClient struct {
	// what do I need to
	Deli    *DbDeli
	Server  web.Server
	Browser web.Peer
}

var _ web.Peer = (*CheckoutClient)(nil)

// Rpc implements Peer
func (s *CheckoutClient) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	s.Deli.Mu.Lock()
	defer s.Deli.Mu.Unlock()

	var v struct {
		Sku         string `json:"sku,omitempty"`
		Description string `json:"description,omitempty"`
		Ticket      int
	}
	json.Unmarshal(params, &v)
	var err error
	result := -1

	// do we want release all to be all databases? what is the use case
	release := func(sku string, ticket int) {
		leaseKey := fmt.Sprintf("%s~%d", v.Sku, v.Ticket)
		delete(s.Deli.State.Reservation, leaseKey)
	}
	reserve := func(sku string, desc string) int {
		cf := s.Deli.State.Sku[sku]
		for i := 0; i < cf.Limit; i++ {
			leaseKey := fmt.Sprintf("%s~%d", v.Sku, i)
			if _, ok := s.Deli.State.Reservation[leaseKey]; !ok {
				s.Deli.State.Reservation[leaseKey] = Reservation{
					Sku:         sku,
					Ticket:      i,
					Description: desc,
				}
				return i
			}
		}
		return -1
	}
	switch method {
	case "update":
		// nothing, just fall through to publish
	case "reserve":
		result = reserve(v.Sku, v.Description)
	case "release":
		release(v.Sku, v.Ticket)
	case "releaseAll":
		for _, x := range s.Deli.State.Reservation {
			release(x.Sku, x.Ticket)
		}
	}

	b, _ := json.Marshal(&s.Deli.State)
	s.Server.Publish("update", b, nil)
	return &result, nil, err
}
