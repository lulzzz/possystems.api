

INSERT INTO [dbo].[role]
           ([role_name]
           ,[description]
           ,[created_date]
           ,[created_by]
           ,[modified_date]
           ,[modified_by]
           ,[status])
     VALUES
           ('Admin'
           ,null
           ,getdate()
           ,'Admin'
           ,getdate()
           ,'Admin'
           ,'A')
GO


