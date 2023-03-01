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
	// we need to optionally back up the golden copies here
	// we need to support more than one

	for name, x := range d.State.Sku {
		drv, ok := d.Drivers[x.Dbms]
		if !ok {
			log.Fatalf("Unknown dbms %s", x.Dbms)
		}
		e := drv.Backup(name) //ßbackupDir + name+".bak", db, "/var/opt/mssql")
		if e != nil {
			log.Fatal(e)
		}
	}

	for name, x := range d.State.Sku {
		drv, ok := d.Drivers[x.Dbms]
		if !ok {
			log.Fatalf("Unknown dbms %s", x.Dbms)
		}

		for i := 0; i < x.Limit; i++ {
			db := fmt.Sprintf("%s_%d", name, i)
			e := drv.Create(db)
			if e != nil {
				log.Fatal(e)
			}
		}
	}
}

//backup+name+".bak",
// , "/var/opt/mssql"

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
	driver := s.Drivers[cf.Dbms]
	return driver.Restore(db)
}

// restores a snapshot
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

// this is just a utility, not required. load some backup to the the "golden" copy.
func load() *cobra.Command {
	r := &cobra.Command{
		Use: "load package backupfile sku",
		Run: func(cmd *cobra.Command, args []string) {

			app, e := NewDbDeli(args[0])
			if e != nil {
				log.Fatal(e)
			}
			backupfile := args[1]
			sku := args[2]

			sk, ok := app.State.Sku[sku]
			if !ok {
				log.Fatalf("No sku %s", sku)
			}
			drv, ok := app.Drivers[sk.Dbms]
			if !ok {
				log.Fatalf("No driver %s", sk.Database)
			}
			drv.BackupToDatabase(backupfile, sku)

		},
	}
	return r
}
