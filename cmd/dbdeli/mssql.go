package main

import (
	"database/sql"
	"fmt"
	"io"
	"log"

	_ "github.com/microsoft/go-mssqldb"
)

type Dbp interface {
	io.Closer
	Create(backup string, db string, filedir string) error
	Restore(db string) error
}
type Driver struct {
	Server   string `json:"server,omitempty"`
	User     string `json:"user,omitempty"`
	Password string `json:"password,omitempty"`
	Port     int    `json:"port,omitempty"`
}
type DbpBase struct {
}
type DbpMssql struct {
	*Driver
	db *sql.DB
}

// Close implements Dbp
func (s *DbpMssql) Close() error {
	if s.db == nil {
		return nil
	}
	return s.db.Close()
}

var _ Dbp = (*DbpMssql)(nil)

func (c *DbpMssql) Query(d string) error {
	db, e := c.Connect()
	if e != nil {
		return e
	}

	rows, err := db.Query(d)
	var strrow string
	for rows.Next() {
		err = rows.Scan(&strrow)
	}

	return err
}
func (c *DbpMssql) Exec1(d string) error {
	log.Printf("Exec: %s", d)
	db, e := c.Connect()
	if e != nil {
		return e
	}

	_, err := db.Exec(d)
	if err != nil {
		log.Print(err)
		panic(err)
	}
	return err
}

func (c *DbpMssql) Backup(db string) error {
	cmd := fmt.Sprintf("BACKUP DATABASE %s TO DISK = '/var/opt/mssql/backup/%s.bak'", db, db)
	return c.Exec1(cmd)
}

type LogicalFile struct {
	name string
	kind string
}

func (c *DbpMssql) DescribeBackup(f string) ([]LogicalFile, error) {
	cmd := fmt.Sprintf("RESTORE FILELISTONLY FROM DISK = '%s'", f)
	db, e := c.Connect()
	if e != nil {
		return nil, e
	}
	rows, err := db.Query(cmd)
	if err != nil {
		return nil, err
	}

	var columns []string
	columns, err = rows.Columns()

	colNum := len(columns)

	cols := make([]interface{}, colNum)
	colp := make([]*string, colNum)
	for i := range colp {
		cols[i] = new(sql.NullString)
	}

	x := []LogicalFile{}
	for rows.Next() {
		err = rows.Scan(cols...)
		name := cols[0].(*sql.NullString)
		kind := (cols[2]).(*sql.NullString)
		x = append(x, LogicalFile{name: name.String, kind: kind.String})
	}

	return x, err
}

// Create implements Dbp
func (c *DbpMssql) Create(backup string, db string, filedir string) error {
	log.Printf("Create: %s,%s,%s", backup, db, filedir)
	lf, e := c.DescribeBackup(backup)
	if e != nil {
		return e
	}

	c.Drop(db + "_ss")
	c.Drop(db)

	ext := map[string]string{
		"S": "",
		"D": ".mdf",
		"L": ".log",
	}
	s := fmt.Sprintf("RESTORE DATABASE [%s] FROM DISK = N'%s' WITH", db, backup)
	for _, o := range lf {
		s += fmt.Sprintf(" Move N'%s' to N'%s/%s_%s%s',", o.name, filedir, db, o.name, ext[o.kind])
	}

	s += " STATS=5"
	//
	// c.Exec1(fmt.Sprintf(WITH  FILE = 1,  MOVE N'iMISMain15' TO N'%s',  MOVE N'iMISMain15_log' TO N'%s',  NOUNLOAD,  STATS = 5", db, backup, dbfile, logfile))
	c.Exec1(s)
	// Snapshot implements Dbp

	s = fmt.Sprintf("CREATE DATABASE %s_ss ON ", db)
	for _, o := range lf {
		if o.kind == "D" {
			s += fmt.Sprintf("(NAME = %s,  FILENAME = '%s/%s_%s.ss'),", o.name, filedir, db, o.name)
		}
	}
	s = s[0 : len(s)-1]
	s += fmt.Sprintf(" AS SNAPSHOT OF %s", db)
	return c.Exec1(s)

}

// Drop implements Dbp
func (c *DbpMssql) Drop(db string) error {
	return c.Exec1(fmt.Sprintf("drop database if exists %s", db))
}

var _ Dbp = (*DbpMssql)(nil)

func NewMsSql(d *Driver) *DbpMssql {
	if d == nil {
		d = &Driver{
			User: "sa", Password: "dsa", Server: "localhost", Port: 1433,
		}
	}
	return &DbpMssql{
		Driver: d,
		db:     nil,
	}
}

func (d *DbpMssql) Connect() (*sql.DB, error) {
	connString := fmt.Sprintf("server=%s;user id=%s;password=%s;port=%d", d.Server, d.User, d.Password, d.Port)
	log.Printf("Connect: %s", connString)
	var err error
	if d.db != nil {
		return d.db, nil
	}
	d.db, err = sql.Open("mssql", connString)

	if err != nil {
		return nil, err
	} else {
		return d.db, nil
	}
}

func (c *DbpMssql) Disconnect(db string) error {
	var s = fmt.Sprintf(`DECLARE @SQL varchar(max)
	SELECT @SQL = COALESCE(@SQL,'') + 'Kill ' + Convert(varchar, SPId) + ';'
			FROM MASTER..SysProcesses
			WHERE DBId = DB_ID('%s') AND SPId <> @@SPId
	EXEC(@SQL)`, db)
	return c.Exec1(s)
}

func (c *DbpMssql) Restore(db string) error {
	//c.Disconnect(db)
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
