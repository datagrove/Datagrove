package main

import (
	"fmt"
	"log"
	"os"
	"strconv"

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
			e := drv.Create("/var/opt/mssql/backup/"+name+".bak", db, "/var/opt/mssql")
			if e != nil {
				log.Fatal(e)
			}
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
func (s *DbDeli) Restore(sku string, tag int) error {
	cf, ok := s.State.Sku[sku]
	if !ok || tag >= cf.Limit {
		return fmt.Errorf("bad sku %s,%d", sku, tag)
	}
	db := fmt.Sprintf("%s_%d", sku, tag)
	driver := s.Drivers[cf.Db]
	return driver.Restore(db)
}
func restore() *cobra.Command {
	r := &cobra.Command{
		Use: "restore package sku tag",
		Run: func(cmd *cobra.Command, args []string) {

			app, e := NewDbDeli(args[0])
			if e != nil {
				log.Fatal(e)
			}
			i, _ := strconv.Atoi(args[2])
			app.Restore(args[1], i)
		},
	}
	return r
}
