CREATE TABLE [dbo].[session](
	[session_id] [int] IDENTITY(1,1) NOT NULL,
	[user_id] [int] NOT NULL,
	[start_time] [datetime] NOT NULL,
	[end_time] [datetime] NULL,
	[created_date] [datetime] NOT NULL,
	[created_by] [nvarchar](50) NOT NULL,
	[modified_date] [datetime] NULL,
	[modified_by] [nvarchar](50) NULL,
	[status] [char](1) NOT NULL,
 CONSTRAINT [PK_session] PRIMARY KEY CLUSTERED 
(
	[session_id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[session]  WITH CHECK ADD  CONSTRAINT [FK_session_user] FOREIGN KEY([user_id])
REFERENCES [dbo].[user] ([user_id])
GO

ALTER TABLE [dbo].[session] CHECK CONSTRAINT [FK_session_user]
GO


