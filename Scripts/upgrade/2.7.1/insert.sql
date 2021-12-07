INSERT INTO [dbo].[configuration]
           ([config_code]
           ,[config_value]
           ,[status]
           ,[created_date]
           ,[created_by]
           ,[modified_date]
           ,[modified_by]
           ,[Description])
     VALUES
           ('printCopy'
           ,'1'
           ,'A'
           ,getdate()
           ,'Admin'
           ,getdate()
           ,'Admin'
           ,getdate())
GO


