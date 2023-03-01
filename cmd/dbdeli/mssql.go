package main

import (
	"database/sql"
	"fmt"
	"io"
	"log"
	"os"
	"path"
	"strings"

	_ "github.com/microsoft/go-mssqldb"
)

type Dbp interface {
	io.Closer

	BackupToDatabase(backupPath, database string) error

	// backs up the golden database, then restores and snapshots.
	Backup(db string) error
	Create(db string, begin, end int) error
	Restore(db string) error
	Drop2(db string) error
}
type Driver struct {
	Server   string `json:"server,omitempty"`
	User     string `json:"user,omitempty"`
	Password string `json:"password,omitempty"`
	Port     int    `json:"port,omitempty"`
	Files    string `json:"files,omitempty"`
	Windows  bool   `json:"windows,omitempty"`
}
type DbpBase struct {
}
type DbpMssql struct {
	*Driver
	db *sql.DB
}

var _ Dbp = (*DbpMssql)(nil)

// Close implements Dbp
func (s *DbpMssql) Close() error {
	if s.db == nil {
		return nil
	}
	return s.db.Close()
}

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
	f := path.Join(c.Files, db+".bak")
	os.Remove(f)
	f = c.Join("", f)
	cmd := fmt.Sprintf("BACKUP DATABASE %s TO DISK = '%s'", db, f)
	return c.Exec1(cmd)
}

type LogicalFile struct {
	name string
	kind string
}

func (c *DbpMssql) DescribeBackup(f string) ([]LogicalFile, error) {
	f = c.Join("", f)
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

func (c *DbpMssql) BackupToDatabase2(backup, db string, lf []LogicalFile) error {
	c.Drop(db)
	ext := map[string]string{
		"S": "",
		"D": ".mdf",
		"L": ".log",
	}

	s := fmt.Sprintf("RESTORE DATABASE [%s] FROM DISK = N'%s' WITH", db, c.Join("", backup))
	for _, o := range lf {
		pf := c.Join(c.Files, db+"_"+o.name+ext[o.kind])
		s += fmt.Sprintf(" Move N'%s' to N'%s',", o.name, pf) //  c.Files, db, o.name, ext[o.kind]
	}
	s += " STATS=5"
	return c.Exec1(s)
}

// used (optionally) to load a gold database. Alternately you could create the gold copy with code, but this is option for convenienc.
func (c *DbpMssql) BackupToDatabase(backup, db string) error {
	// order is important here, we need to drop the snapshot before the database.
	lf, e := c.DescribeBackup(backup)
	if e != nil {
		return e
	}
	return c.BackupToDatabase2(backup, db, lf)
}

// Create implements Dbp
func (c *DbpMssql) Drop2(db string) error {
	c.Drop(db + "_ss")
	return c.Drop(db)
}

func (c *DbpMssql) Join(dir, file string) string {
	s := path.Join(dir, file)
	if c.Windows {
		return strings.ReplaceAll(s, "/", "\\")
	} else {
		return s
	}
}

// Create implements Dbp
func (c *DbpMssql) Create(name string, begin, end int) error {
	filedir := c.Files
	backup := path.Join(c.Driver.Files, name+".bak")
	lf, e := c.DescribeBackup(backup)
	if e != nil {
		return e
	}

	for tag := begin; tag < end; tag++ {
		db := fmt.Sprintf("%s_%d", name, tag)
		// order is important here, we need to drop the snapshot before the database.
		c.Drop(db + "_ss")
		e := c.BackupToDatabase2(backup, db, lf)
		if e != nil {
			return e
		}

		// Snapshot implements Dbpa
		s := fmt.Sprintf("CREATE DATABASE %s_ss ON ", db)
		for _, o := range lf {
			if o.kind == "D" {
				pf := c.Join(filedir, db+"_"+o.name+".ss")
				s += fmt.Sprintf("(NAME = %s,  FILENAME = '%s'),", o.name, pf)
			}
		}
		s = s[0 : len(s)-1]
		s += fmt.Sprintf(" AS SNAPSHOT OF %s", db)
		e = c.Exec1(s)
		if e != nil {
			return e
		}
	}
	return nil
}

// Drop implements Dbp
func (c *DbpMssql) Drop(db string) error {
	return c.Exec1(fmt.Sprintf("drop database if exists %s", db))
}

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
	e := c.Exec1(fmt.Sprintf("ALTER DATABASE [%s] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", db))
	if e != nil {
		return e
	}
	e = c.Exec1(fmt.Sprintf("RESTORE DATABASE %s from DATABASE_SNAPSHOT = '%s_ss'", db, db))
	if e != nil {
		return e
	}
	return c.Exec1(fmt.Sprintf("ALTER DATABASE [%s] SET MULTI_USER WITH ROLLBACK IMMEDIATE", db))
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
