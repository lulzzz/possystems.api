INSERT INTO [dbo].[user_role]
           ([created_date]
           ,[created_by]
           ,[modified_date]
           ,[modified_by]
           ,[status]
           ,[role_id]
           ,[user_id])
     VALUES
           (getdate()
           ,'Admin'
           ,getdate()
           ,'Admin'
           ,'A'
           ,(select role_id from [role] where role_name = 'Admin')
           ,(select user_id	 from [user] where user_name = 'Admin'))
GO


