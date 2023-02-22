package main

import (
	"embed"
	"fmt"
	"os"

	"github.com/datagrove/datagrove/pkg/dbdeli"
	"github.com/datagrove/datagrove/pkg/web"
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
			app := dbdeli.NewDbDeli()

			// called on each socket connection
			// called once to create a guest connection
			NewCheckoutClient := func(m web.Server, browser web.Peer) (web.Peer, error) {
				return &dbdeli.CheckoutClient{
					Deli:    app,
					Server:  m,
					Browser: browser,
				}, nil
			}

			// configure can be  called outside the context of a client
			// for example on startup and when a watched configuration file changes.
			// the configuration is automatically published to "all"
			configure := func(m []byte) error {
				return app.Configure(m)
			}
			mydir, _ := os.Getwd()
			if len(args) > 1 {
				mydir = args[1]
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
