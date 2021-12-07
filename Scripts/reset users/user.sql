
INSERT INTO [dbo].[user]
           ([user_name]
           ,[password]
           ,[display_name]
           ,[full_name]
           ,[designation]
           ,[contact_no]
           ,[email]
           ,[address]
           ,[created_date]
           ,[created_by]
           ,[modified_date]
           ,[modified_by]
           ,[status]
           ,[barcode])
     VALUES
           ('admin'
           ,''
           ,'Admin'
           ,null
           ,null
           ,''
           ,null
           ,null
           ,getdate()
           ,'admin'
           ,getdate()
           ,'admin'
           ,'A'
           ,null)
GO


