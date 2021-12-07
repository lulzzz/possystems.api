EXEC sp_rename 'user_info', 'user'
EXEC sp_rename 'role_info', 'role'
EXEC  sp_rename 'user.user_id', 'user_code', 'COLUMN';
EXEC  sp_rename 'user_role.user_id', 'user_code', 'COLUMN';
EXEC  sp_rename 'user_role.user_role_id', 'user_role_code', 'COLUMN';
GO
ALTER TABLE [dbo].[user] ADD user_id INT NOT NULL IDENTITY(1,1);
ALTER TABLE [dbo].[user_role] DROP CONSTRAINT FK_user_role_role_info;
ALTER TABLE [dbo].[user_role] DROP CONSTRAINT FK_user_role_user_info;
ALTER TABLE [dbo].[user] DROP CONSTRAINT PK_user_info;
ALTER TABLE [dbo].[user] ADD PRIMARY KEY ( user_id );
ALTER TABLE [dbo].[role] ADD role_id INT NOT NULL IDENTITY(1,1);
ALTER TABLE [dbo].[user_role] ADD role_id INT NOT NULL  DEFAULT(0);
ALTER TABLE [dbo].[user_role] ADD user_id INT NOT NULL  DEFAULT(0);
ALTER TABLE [dbo].[role_claim] ADD role_id INT NOT NULL  DEFAULT(0);

ALTER TABLE [dbo].[user_role] DROP CONSTRAINT PK_user_role; 

ALTER TABLE [dbo].[user_role] DROP COLUMN [user_code]; 
ALTER TABLE [dbo].[user_role] DROP COLUMN [role_code]; 
ALTER TABLE [dbo].[user_role] DROP COLUMN [user_role_code]; 

ALTER TABLE [dbo].[user_role] ADD user_role_id INT NOT NULL IDENTITY(1,1);
ALTER TABLE [dbo].[user_role] ADD PRIMARY KEY ( user_role_id );

ALTER TABLE [dbo].[role_claim] DROP CONSTRAINT FK_role_claim_role_info;
ALTER TABLE [dbo].[role] DROP CONSTRAINT PK_role_info; 
ALTER TABLE [dbo].[role] ADD PRIMARY KEY ( role_id );

ALTER TABLE [dbo].[role] DROP COLUMN [role_code]; 
ALTER TABLE [dbo].[user] DROP COLUMN [user_code]; 
GO

TRUNCATE TABLE [dbo].[user_role];

ALTER TABLE [dbo].[user_role]
   ADD CONSTRAINT FK_user_role_role 
   FOREIGN KEY (role_id)
      REFERENCES [dbo].[role] (role_id)
      ON DELETE CASCADE
      ON UPDATE CASCADE
;
GO

ALTER TABLE [dbo].[user_role]
   ADD CONSTRAINT FK_user_role_user FOREIGN KEY (user_id)
      REFERENCES [dbo].[user] (user_id)
      ON DELETE CASCADE
      ON UPDATE CASCADE
;
GO
