package dbdeli

import (
	"database/sql"
	"fmt"
	"log"

	_ "github.com/microsoft/go-mssqldb"
)

type Dbp interface {
	Create(backup string, db string, filedir string) error
	Restore(db string) error
}

type DbpBase struct {
}
type DbpMssql struct {
	*sql.DB
}

func (c *DbpMssql) Exec1(d string) error {
	_, err := c.Exec(d)
	if err != nil {
		log.Print(err)
		panic(err)
	}
	return err
}

// Create implements Dbp
func (c *DbpMssql) Create(backup string, db string, filedir string) error {
	c.Drop(db + "_ss")
	c.Drop(db)
	var dbfile = filedir + "\\" + db + ".mdf"
	var logfile = filedir + "\\" + db + ".ldf"
	c.Exec1(fmt.Sprintf("RESTORE DATABASE [%s] FROM  DISK = N'%s' WITH  FILE = 1,  MOVE N'iMISMain15' TO N'%s',  MOVE N'iMISMain15_log' TO N'%s',  NOUNLOAD,  STATS = 5", db, backup, dbfile, logfile))
	return c.Snapshot(db)
}

// Drop implements Dbp
func (c *DbpMssql) Drop(db string) error {
	return c.Exec1(fmt.Sprintf("drop database if exists %s", db))
}

// Snapshot implements Dbp
func (c *DbpMssql) Snapshot(db string) error {
	return c.Exec1(fmt.Sprintf("CREATE DATABASE %s_ss ON  ( NAME = iMISMain15, FILENAME = 'd:\\db\\%s.ss' ) AS SNAPSHOT OF %s", db, db, db))
}

var _ Dbp = (*DbpMssql)(nil)

func NewDbpMssql(server, user, password string, port int) (*DbpMssql, error) {
	connString := fmt.Sprintf("server=%s;user id=%s;password=%s;port=%d", server, user, password, port)
	conn, err := sql.Open("mssql", connString)
	if err != nil {
		return nil, err
	}
	return &DbpMssql{
		conn,
	}, nil
}

func (c *DbpMssql) Disconnect(db string) {
	var s = fmt.Sprintf(`DECLARE @SQL varchar(max)
	SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
			FROM MASTER..SysProcesses
			WHERE DBId = DB_ID('%s') AND SPId <> @@SPId
	EXEC(@SQL)`, db)
	c.Exec1(s)
}

func (c *DbpMssql) Restore(db string) error {
	c.Disconnect(db)
	return c.Exec1(fmt.Sprintf("RESTORE DATABASE %s from DATABASE_SNAPSHOT = '%s_ss'", db, db))
}

/*
const restartSql = `

GO


GO

	var v = &V{
		Configure: &configure,
		I:         n,
	}
	t, err := template.New("todos").Parse(restartSql)
	if err != nil {
		panic(err)
	}
	var tpl bytes.Buffer
	err = t.Execute(&tpl, v)
	if err != nil {
		panic(err)
	}
	exec.Command(tpl.String())
*/
