package dbdeli

import (
	"encoding/json"
	"fmt"

	"github.com/datagrove/datagrove/pkg/web"
)

// uses datagrove basic web app.

// based on an active websocket connection to the web front end
type CheckoutClient struct {
	// what do I need to
	Deli    *DbDeli
	Server  web.Server
	Browser web.Peer
}

var _ web.Peer = (*CheckoutClient)(nil)

// Close implements Peer
func (s *CheckoutClient) Close() error {
	return nil
}

func (*CheckoutClient) Notify(method string, params []byte, more []byte) {
	// we don't use any notifications, but this is part of the Peer interface
}
func (s *CheckoutClient) WebRpc(method string, params []byte, data []byte) (any, []byte, error) {

	// this needs to be a web api for the convenience of the test code
	return s.Rpc(method, params, data)
}

// Rpc implements Peer
func (s *CheckoutClient) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	deli.mu.Lock()
	defer deli.mu.Unlock()

	var v struct {
		Sku         string `json:"id,omitempty"`
		Description string `json:"description,omitempty"`
		Ticket      int
	}
	json.Unmarshal(params, &v)
	sk, ok := deli.sku[v.Sku]
	if !ok {
		return nil, nil, fmt.Errorf("unknown %s", v.Sku)
	}

	var err error
	result := -1

	release := func(ticket int) {
		sk.avail = append(sk.avail, ticket)
		delete(sk.lease, ticket)
	}
	switch method {

	case "reserve":
		if len(sk.avail) > 0 {
			n := sk.avail[len(sk.avail)-1]

			// we need to restore the database to a snapshot here.
			// we need to kill all the users, unclear if the web will then just restart
			sk.avail = sk.avail[0 : len(sk.avail)-1]
			sk.lease[n] = v.Description
			// no arguments, just get a database

		} else {
			// if we haven't reached our limit we can copy another one.
		}
	case "release":
		release(v.Ticket)
	case "releaseAll":
		for x := range sk.lease {
			release(x)
		}

	}
	a := []Reservation{}
	for k, v := range sk.lease {
		a = append(a, Reservation{
			Id:          k,
			Description: v,
		})
	}
	b, _ := json.Marshal(a)
	s.Server.Publish(v.Sku, b, nil)
	return &result, nil, err
}
