USE [POSSystemsDBV2]
GO

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
           ('confirmationToPrint'
           ,'False'
           ,'A'
           ,getdate()
           ,'Admin'
           ,getdate()
           ,'Admin'
           ,'Confirmation required for print')
GO


