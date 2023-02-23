
create fresh sql server
```
docker run --name sql1 -v sql1data:/var/opt/mssql -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Winter2023_" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
```

get a backup file
```
wget -O example/wwi.bak https://github.com/Microsoft/sql-server-samples/releases/download/wide-world-importers-v1.0/WideWorldImporters-Full.bak

```

copy the backup file into the container (automate this?)
```
docker exec -it sql1 mkdir /var/opt/mssql/backup
sudo docker cp example/wwi.bak sql1:/var/opt/mssql/backup/v10.bak
docker exec -it sql1 ls /var/opt/mssql/backup

sudo docker exec -it sql1 /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'Winter2023_'  -Q 'RESTORE FILELISTONLY FROM DISK = "/var/opt/mssql/backup/wwi.bak"' | tr -s ' ' | cut -d ' ' -f 1-2

sudo docker exec -it sql1 /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P '<YourNewStrong!Passw0rd>' \
   -Q 'RESTORE DATABASE WideWorldImporters FROM DISK = "/var/opt/mssql/backup/wwi.bak" WITH 
     MOVE "WWI_Primary" TO "/var/opt/mssql/data/WWI_Primary.mdf", 
     MOVE "WWI_UserData" TO "/var/opt/mssql/data/WideWorldImporters_userdata.ndf",
     MOVE "WWI_Log" TO "/var/opt/mssql/data/WideWorldImporters.ldf", 
     MOVE "WWI_InMemory_Data_1" TO "/var/opt/mssql/data/WideWorldImporters_InMemory_Data_1"'

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


