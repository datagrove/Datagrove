package dbdeli

import (
	"database/sql"
	"fmt"
	"log"
	"testing"

	f "github.com/datagrove/datagrove/pkg/file"

	_ "github.com/microsoft/go-mssqldb"
)

// this is basic functionality to connect to sql server.
func Test_sql(t *testing.T) {
	var server = "localhost"
	var user = "sa"
	var password = "dsa"
	var port = 1433

	connString := fmt.Sprintf("server=%s;user id=%s;password=%s;port=%d", server, user, password, port)
	fmt.Printf(" connString:%s\n", connString)
	conn, err := sql.Open("mssql", connString)
	if err != nil {
		panic(err)
	}
	defer conn.Close()
}

const COUNT = 3

func Test_create(t *testing.T) {
	s, err := NewDbpMssql("localhost", "sa", "dsa", 1433)
	if err != nil {
		panic(err)
	}
	defer s.Close()
	for x := 0; x < COUNT; x++ {
		var df = "d:\\db"
		db := fmt.Sprintf("iMISMain10_%d", x)
		s.Create(df+"\\v10.bak", db, df)
		s.Restore(db)
	}
}

func Test_copy(t *testing.T) {
	var dir = "C:/VSTS/master/deployment/v10/TenantData/Tenants/"

	for x := 0; x < COUNT; x++ {
		err := f.CopyDir(dir+"tenant_template", dir+fmt.Sprintf("test_tenant_%d", x))
		if err != nil {
			log.Fatal(err)
		}
	}
}

func Test_disconnect(t *testing.T) {
	s, err := NewDbpMssql("localhost", "sa", "dsa", 1433)
	if err != nil {
		panic(err)
	}
	s.Disconnect("iMISMain10_0")
	s.Restore("iMISMain10_0")
}
