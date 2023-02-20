package web

import (
	"io"

	"github.com/gorilla/websocket"
)

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
func (*Client) Notify(method string, params []byte, data []byte) {
	panic("unimplemented")
}

// Rpc implements Peer
func (*Client) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	panic("unimplemented")
}

var _ Peer = (*Client)(nil)

func (c *Client) Close() error {
	// close the websocket, its probably closed already though
	c.conn.Close()
	return nil
}
