# these are not expected to be correct, use with caution. A community memory, feel free to offer changes.

If you have a database somewhere, here's a one liner to grab a backup. Keep in mind this is going to create a database on the database server, so you will need to copy it from there.
```
SQLCMDPASSWORD=Winter2023_ SQLCMD -U sa -E -S touch -Q "BACKUP DATABASE test TO DISK='test.bak'"
```


# random sql notes
```
sudo docker exec -it sql1 /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'Winter2023_'  -Q 'RESTORE FILELISTONLY FROM DISK = "/var/opt/mssql/backup/wwi.bak"' | tr -s ' ' | cut -d ' ' -f 1-2

sudo docker exec -it sql1 /opt/mssql-tools/bin/sqlcmd \
   -S localhost -U SA -P '<YourNewStrong!Passw0rd>' \
   -Q 'RESTORE DATABASE WideWorldImporters FROM DISK = "/var/opt/mssql/backup/wwi.bak" WITH 
     MOVE "WWI_Primary" TO "/var/opt/mssql/data/WWI_Primary.mdf", 
     MOVE "WWI_UserData" TO "/var/opt/mssql/data/WideWorldImporters_userdata.ndf",
     MOVE "WWI_Log" TO "/var/opt/mssql/data/WideWorldImporters.ldf", 
     MOVE "WWI_InMemory_Data_1" TO "/var/opt/mssql/data/WideWorldImporters_InMemory_Data_1"'

```
