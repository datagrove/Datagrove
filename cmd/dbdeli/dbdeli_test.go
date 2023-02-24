package main

import (
	"os"
	"testing"
)

func Test_build(t *testing.T) {
	os.Args = []string{"one", "build", "example"}
	main()
}

func Test_start(t *testing.T) {
	os.Args = []string{"one", "start", "example"}
	main()
}

func Test_restore(t *testing.T) {
	os.Args = []string{"", "restore", "example", "v10", "0"}
	main()
}
