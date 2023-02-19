package dbcheckout

import (
	"github.com/datagrove/datagrove/pkg/web"
	"github.com/spf13/cobra"
)

// each app is going to have some shared state and some client state, and potentially some cluster/consensus state

type CheckoutClient struct {
	// what do I need to
	browser web.WebAppClient
}

// Close implements Peer
func (*CheckoutClient) Close() error {
	return nil
}

// Notify implements Peer
func (*CheckoutClient) Notify(method string, params []byte) {

}

// Rpc implements Peer
func (*CheckoutClient) Rpc(method string, params []byte) ([]byte, error) {
	return nil, nil
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
			web.Run(NewCheckoutClient, opt)
		},
	}
	// c := color.New(color.FgCyan).Add(color.Underline)
	// c.Printf("dgreserve")

}
