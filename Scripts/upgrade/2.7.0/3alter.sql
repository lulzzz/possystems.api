ALTER TABLE [dbo].[pos_terminal]
ALTER COLUMN [ip_address] [varchar](50) NOT null;

ALTER TABLE [dbo].[user]
ADD barcode nvarchar(20) null;

CREATE UNIQUE NONCLUSTERED INDEX [IX_user] ON [dbo].[user]
(
	[user_name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

ALTER TABLE [dbo].[role_claim]
ALTER COLUMN [claim_value] nvarchar(20); 

ALTER TABLE [dbo].[role_claim]
ALTER COLUMN [claim_type] nvarchar(20); 

TRUNCATE TABLE [dbo].[role_claim]

CREATE UNIQUE NONCLUSTERED INDEX [IX_role_claim] ON [dbo].[role_claim]
(
	[role_id] ASC,
	[claim_value] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
