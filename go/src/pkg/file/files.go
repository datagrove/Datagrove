package file

import (
	"io/ioutil"
	"os"
	"path/filepath"
	"strings"
)

func CopyFiles(source, destination string) error {

	var err error = filepath.Walk(source, func(path string, info os.FileInfo, err error) error {
		var relPath string = strings.Replace(path, source, "", 1)
		if relPath == "" {
			return nil
		}
		if info.IsDir() {
			return os.Mkdir(filepath.Join(destination, relPath), 0755)
		} else {
			var data, err1 = ioutil.ReadFile(filepath.Join(source, relPath))
			if err1 != nil {
				return err1
			}
			err = ioutil.WriteFile(filepath.Join(destination, relPath), data, 0666)
			if err != nil {
				panic(err)
			}
			return nil
		}
	})
	if err != nil {
		panic(err)
	}
	return err
}

// delete and move
func MoveForce(from, to string) {
	os.RemoveAll(to)
	os.Rename(from, to)
}

func FileExists(path string) bool {
	if _, err := os.Stat(path); err == nil {
		return true
	} else {
		return false
	}
}
