package main

import (
	"fmt"
	"os"

	"github.com/datagrove/datagrove/pkg/dbdeli"
	"github.com/joho/godotenv"
	"github.com/spf13/cobra"
)

func main() {
	godotenv.Load()
	var rootCmd = &cobra.Command{
		Use: "dg [sub]",
	}

	rootCmd.AddCommand(dbdeli.New())

	if err := rootCmd.Execute(); err != nil {
		fmt.Println(err)
		os.Exit(1)
	}
}
