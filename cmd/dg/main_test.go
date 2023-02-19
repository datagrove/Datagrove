package main

import (
	"os"
	"testing"
)

func Test_reserve(t *testing.T) {
	os.Args = []string{"dgtest", "reserve", "/Users/jim/dev/asi/asi1/packages/asitest/dist"}
	main()
}
