Node wrapper for pkg/dbcheckout and package/dbdeli_view

note that sql server will silently quit if password is not complex enough.
```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Winter2023_" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

sudo docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=<YourStrong!Passw0rd>' \
   --name 'sql1' -p 1401:1433 \
   -v sql1data:/var/opt/mssql \
   -d mcr.microsoft.com/mssql/server:2022-latest

sudo docker exec -it sql1 mkdir /var/opt/mssql/backup


npx dbdeli [dir]
```

https://learn.microsoft.com/en-us/sql/linux/tutorial-restore-backup-in-sql-server-container?view=sql-server-ver16

sudo docker exec -it sql1 mkdir /var/opt/mssql/backup
sudo docker cp wwi.bak sql1:/var/opt/mssql/backup