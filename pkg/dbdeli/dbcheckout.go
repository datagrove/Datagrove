package dbdeli

import (
	"encoding/json"
	"sync"

	"github.com/datagrove/datagrove/pkg/web"
	"github.com/spf13/cobra"
)

// global state
type DbDeliOptions struct {
}

// the server can stream a []Reservation list on a websocket for monitoring
type Reservation struct {
	Id          int    `json:"id"`
	Description string `json:"description"`
}

type DbDeli struct {
	opt       *DbDeliOptions
	mu        sync.Mutex
	configure Configure
	sku       map[string]Sku
}

var deli DbDeli

// we can load and then watch the configuration file for changes
// should we use cobra for this?
func NewDbDeli() *DbDeli {
	return &DbDeli{
		opt:       nil,
		mu:        sync.Mutex{},
		configure: Configure{},
		sku:       map[string]Sku{},
	}
}
func (d *DbDeli) Configure(m []byte) error {
	var opt DbDeliOptions
	json.Unmarshal(m, &opt)
	d.mu.Lock()
	d.opt = &opt
	d.mu.Unlock()
	return nil
}

type Sku struct {
	sku   ConfigureSku
	avail []int
	lease map[int]string
}

type ConfigureSku struct {
	Limit int `json:"limit"`
}
type Configure struct {
	Sku map[string]*ConfigureSku `json:"sku,omitempty"`
}

// load the configuration from the opt.home, watch and reconfigure if the file changes.
// the web server can separately broadcast these out if configured to.

// move back to cmd? if composing command app belongs here though
func New() *cobra.Command {
	return &cobra.Command{
		Use: "start [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			app := NewDbDeli()

			// called on each socket connection
			// called once to create a guest connection
			NewCheckoutClient := func(m web.Server, browser web.Peer) (web.Peer, error) {
				return &CheckoutClient{
					deli:    app,
					server:  m,
					browser: browser,
				}, nil
			}

			// configure can be  called outside the context of a client
			// for example on startup and when a watched configuration file changes.
			// the configuration is automatically published to "all"
			configure := func(m []byte) error {
				return app.Configure(m)
			}
			web.Run(NewCheckoutClient, cmd, args, configure)
		},
	}
}
