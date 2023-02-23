Node wrapper for pkg/dbcheckout and package/dbdeli_view

```
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=dsa" -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest

npx dbdeli [dir]
```