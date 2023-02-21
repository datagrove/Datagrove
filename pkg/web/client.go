package web

import (
	"encoding/json"
	"io"
	"sync"

	"github.com/gorilla/websocket"
)

// can configuration be a standard topic then?
// do we publish to it to set it?
// channels can have different behaviors for newcomers: get summary, get all, get new. Most common would be get summary.
// maybe each topic should be a file in a directory (separate? what if we are running from readonly? readonly starter config?)
// there are also more complex behaviors like update by character then commit.
type Server interface {
	Publish(topic string, data []byte, more []byte)
}
type NewWebClient = func(monitor Server, peer Peer) (Peer, error)
type webServer struct {
	mu        sync.Mutex
	channel   map[string]*WebChannel
	onConnect NewWebClient
	Home      string
	WriteHome string
	Url       string
	CertPem   string
	KeyPem    string
}

var _ Server = (*webServer)(nil)

// send implements Server
func (s *webServer) Publish(topic string, data []byte, more []byte) {
	s.mu.Lock()
	defer s.mu.Unlock()
	ch, ok := s.channel[topic]
	if !ok {
		return
	}
	for k := range ch.listen {
		k.Notify(topic, data, more)
	}
}

// more generally this may be a pub sub channel  with a default of all
type WebChannel struct {
	listen map[Peer]bool
}

type Rpc struct {
	Method string          `json:"method,omitempty"`
	Id     int64           `json:"id,omitempty"`
	Params json.RawMessage `json:"params,omitempty"`
}
type RpcReply struct {
	Id     int64           `json:"id,omitempty"`
	Result json.RawMessage `json:"result,omitempty"`
	Error  string          `json:"error,omitempty"`
}
type Peer interface {
	// primary need is for a way to read/write rpc's
	// json, cbor, arrow
	// {json rpc}\0binary
	// if first character is 0 then begin with cbor
	io.Closer
	Rpc(method string, params []byte, data []byte) (any, []byte, error)
	Notify(method string, params []byte, data []byte)
}

// wrapper for websocket, should we make generic?
type Client struct {
	id        string
	conn      *websocket.Conn
	open      []string
	writable  []uint8     // 1 = writable, 2=subscribed
	send      chan []byte // update a batch of logs
	subscribe map[string]bool
}

var _ Peer = (*Client)(nil)

// Notify implements Peer
func (c *Client) Notify(method string, params []byte, data []byte) {
	b, _ := json.Marshal(&Rpc{
		Method: method,
		Id:     0,
		Params: params,
	})
	c.send <- b
}

// Rpc implements Peer
func (c *Client) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	b, _ := json.Marshal(&Rpc{
		Method: method,
		Id:     0,
		Params: params,
	})
	c.send <- b
	return nil, nil, nil
}

func (c *Client) Close() error {
	// close the websocket, its probably closed already though
	// remove any subscriptions associated with it.
	server.mu.Lock()
	defer server.mu.Unlock()
	for k := range c.subscribe {
		m := server.channel[k]
		delete(m.listen, c)
	}

	c.conn.Close()
	return nil
}

// global state.
var server = &webServer{
	channel: map[string]*WebChannel{},
}
