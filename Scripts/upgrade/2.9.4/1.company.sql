ALTER TABLE [dbo].[company] DROP COLUMN template; 
ALTER TABLE [dbo].[company] ADD notes nvarchar(MAX) null;
Go