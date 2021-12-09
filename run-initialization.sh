# Wait to be sure that SQL Server came up
sleep 90s

# Run the restore script to create the DB with schema & data
/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'yourStrong@Password' -Q 'RESTORE DATABASE POSSystemsDBV2 FROM DISK = "/var/opt/mssql/backup/POSSystemsDBV2.bak" WITH MOVE "POSSystemsDB" TO "/var/opt/mssql/data/POSSystemsDB.mdf", MOVE "POSSystemsDB_log" TO "/var/opt/mssql/data/POSSystemsDB_log.ldf"'