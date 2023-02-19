package main

import (
	"encoding/json"
	"encoding/xml"
	"io/fs"
	"os"
	"path/filepath"
	"strings"

	f "github.com/datagrove/datagrove/pkg/file"
	trx "github.com/datagrove/datagrove/pkg/trx"
)

// describes json output
type UnitTest struct {
	TestName string `json:"test_name,omitempty"`
	Outcome  string `json:"outcome,omitempty"`
}
type UnitTestLong struct {
	Output string `json:"output,omitempty"`
	Error  string `json:"error,omitempty"`
	Stack  string `json:"stack,omitempty"`
}
type TestResults struct {
	Name string     `json:"name"`
	File []string   `json:"file"`
	Test []UnitTest `json:"test"`
}

// extensions split by |
func IndexFiles(input string, extension string) []string {
	extv := strings.Split(extension, "|")
	exts := map[string]bool{}
	for _, o := range extv {
		exts[o] = true
	}
	r := []string{}
	matchGo := func(p string, d fs.DirEntry, err error) error {
		ext := filepath.Ext(p)
		if exts[ext] {
			r = append(r, p)
		}
		return nil
	}
	fsys := os.DirFS(input)
	fs.WalkDir(fsys, ".", matchGo)
	return r
}

// this walks pth and writes the selected files (for now just jpg) to a json that is just string[]

// recursive copy

func Copy(from, to string) {
	os.RemoveAll(to)
	os.Mkdir(to, os.ModePerm)
	f.CopyFiles(from, to)
}
func TrxResults(path string) []trx.UnitTestResult {
	var t trx.TestRun
	b, e := os.ReadFile(path)
	if e != nil {
		panic(e)
	}
	e = xml.Unmarshal(b, &t)
	if e != nil {
		panic(e)
	}
	return t.Results.UnitTestResult
}

func Process(name string, dir string) {

	var t TestResults
	t.Name = name                    // can be anything, not used other than copied to json
	t.File = IndexFiles(dir, ".jpg") // get rid of this? should embed the list of images into the (static) output.
	if f.FileExists(dir + "/testResults.trx") {
		os.Rename(dir+"/testResults.trx", dir+"/test_results.trx")
	}
	for _, x := range TrxResults(dir + "/test_results.trx") {
		var o UnitTest
		o.TestName = x.TestName
		o.Outcome = x.Outcome
		var long UnitTestLong
		long.Output = x.Output.StdOut
		long.Error = x.Output.ErrorInfo.Message
		long.Stack = x.Output.ErrorInfo.StackTrace
		b, _ := json.Marshal(&long)
		os.WriteFile(dir+"/images/"+x.TestName+".json", b, 0666)

		t.Test = append(t.Test, o)
	}

	b, err := json.Marshal(&t)
	if err != nil {
		panic(err)
	}
	os.WriteFile(dir+"/../test_results.json", b, 0666)
}

type Build struct {
	TestDirectory string
}

/*
func Nightly(name string) {
	MoveForce("../../test/imis/TestResults", "../results/public/TestResults")
	Copy("../../test/imis/gherkin", "../results/public/gherkin")
	Process(name, "../results/public/TestResults")
}
func main() {
	if len(os.Args) > 1 {
		Nightly(os.Args[1])
	} else {
		Nightly("v10")
	}
}
*/
