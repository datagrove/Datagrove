package main

import (
	"fmt"
	"log"
	"os"

	"github.com/spf13/cobra"
)

// we need to copy the database file into the dock, then recover from the database.
// func (d *DbDeli) IntoContainer() {
// 	backup := path.Join(d.Home, x.Backup)
// 	db := fmt.Sprintf("%s_%d", name, i)
// }

func (d *DbDeli) Build() {
	for name, x := range d.State.Sku {
		drv, ok := d.Drivers[x.Db]
		if !ok {
			log.Fatalf("Unknown dbms %s", x.Db)
		}
		for i := 0; i < x.Limit; i++ {
			db := fmt.Sprintf("%s_%d", name, i)
			drv.Create("/var/opt/mssql/"+name+".bak", db, "/var/opt/mssql")
		}
	}
}

// note that the arguments here are 0 = first positional argument
func build() *cobra.Command {
	r := &cobra.Command{
		Use: "build [dir]",
		Run: func(cmd *cobra.Command, args []string) {
			mydir, _ := os.Getwd()
			if len(args) > 0 {
				mydir = args[0]
			}
			app, e := NewDbDeli(mydir)
			if e != nil {
				log.Fatal(e)
			}
			app.Build()
		},
	}
	return r
}
