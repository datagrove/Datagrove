# these are not expected to be correct, use with caution. A community memory, feel free to offer changes.

If you have a database somewhere, here's a one liner to grab a backup. Keep in mind this is going to create a database on the database server, so you will need to copy it from there.
```
SQLCMDPASSWORD=Winter2023_ SQLCMD -U sa -E -S touch -Q "BACKUP DATABASE test TO DISK='test.bak'"
```
