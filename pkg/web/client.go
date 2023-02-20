package web

import (
	"encoding/json"
	"io"

	"github.com/gorilla/websocket"
)

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
	id       string
	conn     *websocket.Conn
	open     []string
	writable []uint8     // 1 = writable, 2=subscribed
	send     chan []byte // update a batch of logs
}

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

var _ Peer = (*Client)(nil)

func (c *Client) Close() error {
	// close the websocket, its probably closed already though
	c.conn.Close()
	return nil
}
