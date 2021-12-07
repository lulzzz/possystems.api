INSERT INTO [dbo].[role_claim]
           ([claim_type]
           ,[claim_value]
           ,[created_date]
           ,[created_by]
           ,[modified_date]
           ,[modified_by]
           ,[status]
           ,[role_id])
     VALUES
           ('Admin'
           ,'User'
           ,getdate()
           ,'Admin'
           ,getdate()
           ,'Admin'
           ,'A'
           ,(select role_id from [role] where [role_name] = 'Admin'))
GO


