truncate table [dbo].[role_user];
truncate table [dbo].[user];

ALTER TABLE [dbo].[user]
ALTER COLUMN [email] [nvarchar](50) NOT null;
ALTER TABLE [dbo].[user]
ADD [company] [int] null;

ALTER TABLE [dbo].[user]  WITH CHECK ADD  CONSTRAINT [FK_user_company] FOREIGN KEY([company_id])
REFERENCES [dbo].[company] ([id])
GO
ALTER TABLE [dbo].[user] CHECK CONSTRAINT [FK_user_company]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IX_user_1] ON [dbo].[user]
(
	[email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


