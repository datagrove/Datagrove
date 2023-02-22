package dbdeli

import (
	"encoding/json"
	"sync"
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
