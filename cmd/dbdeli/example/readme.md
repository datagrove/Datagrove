
create fresh sql server
```
docker run --name sql1 -v sql1data:/var/opt/mssql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Winter2023_" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

get a backup file
```
wget -O example/v10.bak https://github.com/Microsoft/sql-server-samples/releases/download/wide-world-importers-v1.0/WideWorldImporters-Full.bak

```

copy the backup file into the container (automate this?)
```
docker exec -it sql1 mkdir /var/opt/mssql/backup
sudo docker cp example/v10.bak sql1:/var/opt/mssql/backup/v10.bak
docker exec -it sql1 ls /var/opt/mssql/backup
```




```

{
    "test": {
        "v10": {
            "limit": 3,
            "backup": "wwi.bak",
            "database": "test",
            "db": "mssql"
        }
    }
}

```


