package dbcheckout

import (
	"bytes"
	"encoding/json"
	"html/template"
	"log"
	"os"
	"os/exec"
	"path/filepath"
	"sync"

	"github.com/datagrove/datagrove/pkg/web"
	"github.com/spf13/cobra"
)

var opt *web.WebAppOptions
var mu sync.Mutex
var avail = []int{}
var lease = map[int]string{}
var listen = map[web.Peer]bool{}

type Configure struct {
	Nworker   int    `json:"nworker"`
	Host      string `json:"host"`
	Sqlserver string `json:"sqlserver"`
	Db        string `json:"db"`
	Bak       string `json:"bak"`
	Datadir   string `json:"datadir"`
}
type V struct {
	*Configure
	I int
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

type Reservation struct {
	Id          int    `json:"id"`
	Description string `json:"description"`
}

func update() {
	a := []Reservation{}
	for k, v := range lease {
		a = append(a, Reservation{
			Id:          k,
			Description: v,
		})
	}
	b, _ := json.Marshal(a)
	for w := range listen {
		w.Notify("update", b, nil)
	}
}
func reconfig() {
	b, _ := json.Marshal(configure)
	for w := range listen {
		w.Notify("config", b, nil)
	}
}
func load() {
	b, e := os.ReadFile(filepath.Join(opt.Home, "config.json"))
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
	reconfig()
}

const restartSql = `
SET @DatabaseName = N'{{.Database}}_{{.I}}'
DECLARE @SQL varchar(max)
SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
FROM MASTER..SysProcesses
WHERE DBId = DB_ID(@DatabaseName) AND SPId <> @@SPId
EXEC(@SQL)
GO

RESTORE DATABASE @DatabaseName from DATABASE_SNAPSHOT = '{{.Database}}_ss
GO
`

func refreshDb(n int) {
	var v = &V{
		Configure: &configure,
		I:         n,
	}
	t, err := template.New("todos").Parse(restartSql)
	if err != nil {
		panic(err)
	}
	var tpl bytes.Buffer
	err = t.Execute(&tpl, v)
	if err != nil {
		panic(err)
	}
	exec.Command(tpl.String())
}

// Rpc implements Peer
func (s *CheckoutClient) Rpc(method string, params []byte, data []byte) (any, []byte, error) {
	mu.Lock()
	defer mu.Unlock()
	switch method {

	case "configure":
		v := ""
		json.Unmarshal(params, &v)
		if len(v) > 0 {
			os.WriteFile(filepath.Join(opt.Home, "config.json"), []byte(v), os.ModePerm)
			load()
		}
	case "reserve":
		v := ""
		json.Unmarshal(params, &v)
		n := -1
		if len(avail) > 0 {
			n := avail[len(avail)-1]
			refreshDb(n)
			// we need to restore the database to a snapshot here.
			// we need to kill all the users, unclear if the web will then just restart
			avail = avail[0 : len(avail)-1]
			lease[n] = v
			// no arguments, just get a database
			update()
		}
		return &n, nil, nil
	case "release":
		v := 0
		json.Unmarshal(params, &v)
		avail = append(avail, v)
		log.Printf("Release %d", v)
		delete(lease, v)
		update()
	case "releaseAll":
		load()
	}
	return nil, nil, nil
}

var _ web.Peer = (*CheckoutClient)(nil)

func New() *cobra.Command {
	return &cobra.Command{
		Use: "reserve [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			opt = web.DefaultOptions()
			if len(args) > 0 {
				opt.Home = args[0]
			}
			load()
			NewCheckoutClient := func(browser web.WebAppClient) (web.Peer, error) {
				mu.Lock()
				defer mu.Unlock()
				listen[browser] = true
				reconfig()
				return &CheckoutClient{
					browser: browser,
				}, nil
			}
			web.Run(NewCheckoutClient, opt)
		},
	}
}
