SELECT DISTINCT a.[name]
FROM sysobjects a
INNER JOIN syscomments b on a.id = b.id
WHERE a.name LIKE '%terminal%';

SELECT c.name AS ColName, t.name AS TableName
FROM sys.columns c
    JOIN sys.tables t ON c.object_id = t.object_id
WHERE c.name LIKE '%terminal%';