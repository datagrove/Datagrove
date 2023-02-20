package dbcheckout

import (
	"encoding/json"
	"log"
	"os"
	"path/filepath"
	"sync"

	"github.com/datagrove/datagrove/pkg/web"
	"github.com/spf13/cobra"
)

var home string = ""
var mu sync.Mutex
var avail = []int{}
var lease = map[int]string{}
var listen map[web.Peer]bool

type Configure struct {
	Nworker   int    `json:"nworker"`
	Host      string `json:"host"`
	Sqlserver string `json:"sqlserver"`
	Db        string `json:"db"`
	Bak       string `json:"bak"`
	Datadir   string `json:"datadir"`
}

var configure Configure

type CheckoutClient struct {
	// what do I need to
	browser web.WebAppClient
}

// Close implements Peer
func (s *CheckoutClient) Close() error {
	mu.Lock()
	defer mu.Unlock()
	delete(listen, s.browser)
	return nil
}

// Notify implements Peer
func (*CheckoutClient) Notify(method string, params []byte, more []byte) {

}

func update() {
	b, _ := json.Marshal(lease)
	for w := range listen {
		w.Notify("update", b, nil)
	}
}
func load() {
	b, e := os.ReadFile(filepath.Join(home, "config.json"))
	if e != nil {
		return
	}

	json.Unmarshal(b, &configure)
	// load a configuration imediately releases all locks with prejudice
	avail = make([]int, configure.Nworker)
	for x := 0; x < configure.Nworker; x++ {
		avail[x] = x
	}
	lease = map[int]string{}
	update()
}

// Rpc implements Peer
func (s *CheckoutClient) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	mu.Lock()
	defer mu.Unlock()
	switch method {
	case "listen":
		listen[s.browser] = true
	case "configure":
		v := []string{}
		json.Unmarshal(params, &v)
		if len(v) > 0 {
			os.WriteFile(filepath.Join(home, "config.json"), []byte(v[0]), os.ModePerm)
			load()
		}
	case "reserve":
		v := ""
		json.Unmarshal(params, &v)
		n := avail[len(avail)-1]
		avail = avail[0 : len(avail)-1]
		lease[n] = v
		// no arguments, just get a database
		update()
		return &n, nil, nil
	case "release":
		v := 0
		json.Unmarshal(params, &v)
		avail = append(avail, v)
		log.Printf("Release %d", v)
	}
	return nil, nil, nil
}

var _ web.Peer = (*CheckoutClient)(nil)

func New() *cobra.Command {
	return &cobra.Command{
		Use: "reserve [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			opt := web.DefaultOptions()
			if len(args) > 0 {
				opt.Home = args[0]
			}
			NewCheckoutClient := func(browser web.WebAppClient) (web.Peer, error) {
				return &CheckoutClient{
					browser: browser,
				}, nil
			}
			home = opt.Home
			web.Run(NewCheckoutClient, opt)
		},
	}
	// c := color.New(color.FgCyan).Add(color.Underline)
	// c.Printf("dgreserve")

}
