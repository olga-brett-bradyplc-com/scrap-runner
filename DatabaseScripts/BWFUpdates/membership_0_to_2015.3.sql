USE [scraprunnerhost]
GO

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[aspnet_membership]') AND type in (N'U'))
BEGIN
   PRINT 'ASP Membership must be installed before running this script'
   SET NOEXEC ON
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[fn_isdba]') AND type in (N'FN'))
   DROP FUNCTION [brady_membership].[fn_isdba]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_createsecurable]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_createsecurable]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_deletesecurable]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_deletesecurable]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_createrole]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_createrole]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_deleterole]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_deleterole]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_addrolesecurables]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_addrolesecurables]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[userrole]') AND type in (N'V'))
   DROP VIEW [brady_membership].[userrole]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[usersecurable]') AND type in (N'V'))
   DROP VIEW [brady_membership].[usersecurable]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[roleuser]') AND type in (N'U'))
   DROP TABLE [brady_membership].[roleuser]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[rolesecurable]') AND type in (N'U'))
   DROP TABLE [brady_membership].[rolesecurable]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[extendedrole]') AND type in (N'U'))
   DROP TABLE [brady_membership].[extendedrole]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[roletype]') AND type in (N'U'))
   DROP TABLE [brady_membership].[roletype]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[extendeduser]') AND type in (N'U'))
   DROP TABLE [brady_membership].[extendeduser]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[securableprivilege]') AND type in (N'U'))
   DROP TABLE [brady_membership].[securableprivilege]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[securable]') AND type in (N'U'))
   DROP TABLE [brady_membership].[securable]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[securabletypeprivilege]') AND type in (N'U'))
   DROP TABLE [brady_membership].[securabletypeprivilege]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[securabletype]') AND type in (N'U'))
   DROP TABLE [brady_membership].[securabletype]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[privilege]') AND type in (N'U'))
   DROP TABLE [brady_membership].[privilege]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[nexthigh]') AND type in (N'U'))
   DROP TABLE [brady_membership].[nexthigh]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[dbversion]') AND type in (N'U'))
   DROP TABLE [brady_membership].[dbversion]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_checkversion]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_checkversion]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_updateversion]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_updateversion]
GO

IF  EXISTS (SELECT * FROM sys.schemas WHERE name = N'brady_membership')
   DROP SCHEMA [brady_membership] 
GO

CREATE SCHEMA [brady_membership] AUTHORIZATION [dbo]
GO

DECLARE @RoleName sysname
set @RoleName = N'membership_admin'
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @RoleName AND type = 'R')
Begin
	DECLARE @RoleMemberName sysname
	DECLARE Member_Cursor CURSOR FOR
	select [name]
	from sys.database_principals 
	where principal_id in ( 
		select member_principal_id 
		from sys.database_role_members 
		where role_principal_id in (
			select principal_id
			FROM sys.database_principals where [name] = @RoleName  AND type = 'R' ))

	OPEN Member_Cursor;

	FETCH NEXT FROM Member_Cursor
	into @RoleMemberName

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec sp_droprolemember @rolename=@RoleName, @membername= @RoleMemberName

		FETCH NEXT FROM Member_Cursor
		into @RoleMemberName
	END;

	CLOSE Member_Cursor;
	DEALLOCATE Member_Cursor;
End

GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'membership_admin' AND type = 'R')
DROP ROLE [membership_admin]
GO

CREATE ROLE [membership_admin] AUTHORIZATION [dbo]
GO

DECLARE @RoleName sysname
set @RoleName = N'membership_read'
IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = @RoleName AND type = 'R')
Begin
	DECLARE @RoleMemberName sysname
	DECLARE Member_Cursor CURSOR FOR
	select [name]
	from sys.database_principals 
	where principal_id in ( 
		select member_principal_id 
		from sys.database_role_members 
		where role_principal_id in (
			select principal_id
			FROM sys.database_principals where [name] = @RoleName  AND type = 'R' ))

	OPEN Member_Cursor;

	FETCH NEXT FROM Member_Cursor
	into @RoleMemberName

	WHILE @@FETCH_STATUS = 0
	BEGIN

		exec sp_droprolemember @rolename=@RoleName, @membername= @RoleMemberName

		FETCH NEXT FROM Member_Cursor
		into @RoleMemberName
	END;

	CLOSE Member_Cursor;
	DEALLOCATE Member_Cursor;
End

GO

IF  EXISTS (SELECT * FROM sys.database_principals WHERE name = N'membership_read' AND type = 'R')
DROP ROLE [membership_read]
GO

CREATE ROLE [membership_read] AUTHORIZATION [dbo]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[dbversion](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[version] [nvarchar](50) NOT NULL,
	[major] [int] NOT NULL,
	[minor] [int] NOT NULL,
	[patch] [int] NOT NULL,
	[compile] [int] NOT NULL,
	[timestamp] AS GetDate()
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[dbversion]
   ADD CONSTRAINT [pk_dbversion]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO
      
GRANT SELECT ON [brady_membership].[dbversion] TO membership_read;
GO

CREATE PROCEDURE brady_membership.prc_checkversion
    ( @OldMajor    INT
    , @OldMinor    INT
    , @OldPatch    INT
    , @OldCompile  INT
    , @NewMajor    INT
    , @NewMinor    INT
    , @NewPatch    INT
    , @NewCompile  INT
   )
AS
BEGIN
   --SET NOCOUNT ON
   
   IF NOT EXISTS (SELECT version FROM brady_membership.dbversion WHERE major = @OldMajor AND minor = @OldMinor AND patch = @OldPatch AND compile = @OldCompile)
   BEGIN 
      RAISERROR('Cannot upgrade to %d.%d.%d.%d before database is upgraded to %d.%d.%d.%d', 16, 1, @NewMajor, @NewMinor, @NewPatch, @NewCompile, @OldMajor, @OldMinor, @OldPatch, @OldCompile)
   END 
   
   IF EXISTS (SELECT version FROM brady_membership.dbversion WHERE major = @NewMajor AND minor = @NewMinor AND patch = @NewPatch AND compile = @NewCompile)
   BEGIN
      RAISERROR('Cannot upgrade to %d.%d.%d.%d as database is already at this version or greater', 16, 1, @NewMajor, @NewMinor, @NewPatch, @NewCompile)
   END 
END
GO

CREATE PROCEDURE brady_membership.prc_updateversion
    ( @NewMajor    INT
    , @NewMinor    INT
    , @NewPatch    INT
    , @NewCompile  INT
   )
AS
BEGIN
   DECLARE @version NVARCHAR(43)
   SET @version = (CAST(@NewMajor AS NVARCHAR(10)) + '.' + CAST(@NewMinor AS NVARCHAR(10)) + '.' + CAST(@NewPatch AS NVARCHAR(10)) + '.' + CAST(@NewCompile AS NVARCHAR(10)))
   
   INSERT INTO brady_membership.dbversion (version, major, minor, patch, compile)
      VALUES (@version, @NewMajor, @NewMinor, @NewPatch, @NewCompile)
END
GO

CREATE TABLE [brady_membership].[nexthigh]
(
	[id] [int]    IDENTITY(1,1) NOT NULL,
	[nexthigh]    [bigint] NOT NULL,
	[entityname]  [nvarchar](30) NOT NULL
) ON [PRIMARY]
GO
 
ALTER TABLE [brady_membership].[nexthigh]
   ADD CONSTRAINT [pk_nexthigh] 
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[nexthigh]
   ADD CONSTRAINT [uk_membershipnexthigh1]
      UNIQUE (entityname)
GO

GRANT UPDATE ON [brady_membership].[nexthigh] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[nexthigh] TO [membership_admin]
GO

CREATE TABLE [brady_membership].[privilege](
	[id] [bigint] NOT NULL,
	[name] [nvarchar](64) NOT NULL,
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[privilege]
   ADD CONSTRAINT [pk_privilege]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[privilege]
   ADD CONSTRAINT [uk_privilege1]
      UNIQUE (lowername)
GO

GRANT INSERT ON [brady_membership].[privilege] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[privilege] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[privilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[privilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[privilege] TO [membership_read]
GO

CREATE TABLE [brady_membership].[securabletype](
	[id] [bigint] NOT NULL,
	[name] [nvarchar](64) NOT NULL,
	[creatable] int NOT NULL,
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securabletype]
   ADD CONSTRAINT [pk_securabletype]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securabletype]
   ADD CONSTRAINT [uk_securabletype1]
      UNIQUE (lowername)
GO

GRANT INSERT ON [brady_membership].[securabletype] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[securabletype] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[securabletype] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securabletype] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securabletype] TO [membership_read]
GO

CREATE TABLE [brady_membership].[securabletypeprivilege](
	[securabletypeid] [bigint] NOT NULL,
	[privilegeid] [bigint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securabletypeprivilege]
   ADD CONSTRAINT [pk_securabletypeprivilege]
      PRIMARY KEY CLUSTERED ( [securabletypeid],[privilegeid] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securabletypeprivilege]
   ADD CONSTRAINT [fk_securabletypepriv_sectype]
      FOREIGN KEY (securabletypeid)
         REFERENCES [brady_membership].[securabletype](id)
GO

ALTER TABLE [brady_membership].[securabletypeprivilege]
   ADD CONSTRAINT [fk_securabletypepriv_priv]
      FOREIGN KEY (privilegeid)
         REFERENCES [brady_membership].[privilege](id)
GO

GRANT INSERT ON [brady_membership].[securabletypeprivilege] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[securabletypeprivilege] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[securabletypeprivilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securabletypeprivilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securabletypeprivilege] TO [membership_read]
GO

CREATE TABLE [brady_membership].[securable](
	[id] [bigint] NOT NULL,
	[typeid] [bigint] NOT NULL,
	[name] [nvarchar](64) NOT NULL,
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securable]
   ADD CONSTRAINT [pk_securable]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securable]
   ADD CONSTRAINT [uk_securable1]
      UNIQUE (lowername)
GO

ALTER TABLE [brady_membership].[securable]
   ADD CONSTRAINT [fk_securable_type]
      FOREIGN KEY (typeid)
         REFERENCES [brady_membership].[securabletype](id)
GO

GRANT INSERT ON [brady_membership].[securable] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[securable] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[securable] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securable] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securable] TO [membership_read]
GO

CREATE TABLE [brady_membership].[securableprivilege](
	[securableid] [bigint] NOT NULL,
	[privilegeid] [bigint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securableprivilege]
   ADD CONSTRAINT [pk_securableprivilege]
      PRIMARY KEY CLUSTERED ( [securableid],[privilegeid] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[securableprivilege]
   ADD CONSTRAINT [fk_securableprivilege_securable]
      FOREIGN KEY (securableid)
         REFERENCES [brady_membership].[securable](id)
GO

ALTER TABLE [brady_membership].[securableprivilege]
   ADD CONSTRAINT [fk_securableprivilege_priv]
      FOREIGN KEY (privilegeid)
         REFERENCES [brady_membership].[privilege](id)
GO

GRANT INSERT ON [brady_membership].[securableprivilege] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[securableprivilege] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[securableprivilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securableprivilege] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[securableprivilege] TO [membership_read]
GO

CREATE TABLE [brady_membership].[roletype](
	[id] [bigint] NOT NULL,
	[name] [nvarchar](64) NOT NULL,
	[creatable] [int] NOT NULL,
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[roletype]
   ADD CONSTRAINT [pk_roletype]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[roletype]
   ADD CONSTRAINT [uk_roletype1]
      UNIQUE (lowername)
GO

INSERT INTO [brady_membership].[roletype]([id], [name], [creatable])
   VALUES (0, 'System', 0)
GO
INSERT INTO [brady_membership].[roletype]([id], [name], [creatable])
   VALUES (1, 'Group', 1)
GO
INSERT INTO [brady_membership].[roletype]([id], [name], [creatable])
   VALUES (2, 'User', 0)
GO

GRANT INSERT ON [brady_membership].[roletype] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[roletype] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[roletype] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[roletype] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[roletype] TO [membership_read]
GO

CREATE TABLE [brady_membership].[extendedrole](
	[id] [bigint] NOT NULL,
	[typeid] [bigint] DEFAULT 1 NOT NULL,
	[name] [nvarchar](256) NOT NULL,
	[description] [nvarchar](256),
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[extendedrole]
   ADD CONSTRAINT [pk_role]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[extendedrole]
   ADD CONSTRAINT [uk_role1]
      UNIQUE (lowername)
GO

ALTER TABLE [brady_membership].[extendedrole]
   ADD CONSTRAINT [fk_extendedrole_type]
      FOREIGN KEY (typeid)
         REFERENCES [brady_membership].[roletype](id)
GO

GRANT INSERT ON [brady_membership].[extendedrole] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[extendedrole] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[extendedrole] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[extendedrole] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[extendedrole] TO [membership_read]
GO

CREATE TABLE [brady_membership].[rolesecurable](
	[id] [bigint] NOT NULL,
	[roleid] [bigint] NOT NULL,
	[securableid] [bigint] NOT NULL,
	[privilegeid] [bigint] NOT NULL,
	[allow] [int] NOT NULL,
	[deny] [int] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[rolesecurable]
   ADD CONSTRAINT [pk_rolesecurable]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[rolesecurable]
   ADD CONSTRAINT [uk_rolesecurable1]
      UNIQUE ([roleid], [securableid], [privilegeid])
GO

ALTER TABLE [brady_membership].[rolesecurable]
   ADD CONSTRAINT [fk_rolesecurable_role]
      FOREIGN KEY ([roleid])
         REFERENCES [brady_membership].[extendedrole]([id])
            ON DELETE CASCADE
GO

ALTER TABLE [brady_membership].[rolesecurable]
   ADD CONSTRAINT [fk_rolesecurable_securableprivilege]
      FOREIGN KEY ([securableid], [privilegeid])
         REFERENCES [brady_membership].[securableprivilege]([securableid], [privilegeid])
            ON DELETE CASCADE
GO

GRANT INSERT ON [brady_membership].[rolesecurable] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[rolesecurable] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[rolesecurable] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[rolesecurable] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[rolesecurable] TO [membership_read]
GO

CREATE TABLE [brady_membership].[extendeduser](
	[id] [bigint] NOT NULL,
	[name] [nvarchar](256) NOT NULL,
	[firstname] [nvarchar](256),
	[lastname] [nvarchar](256),
	[lowername] AS LOWER([name])
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[extendeduser]
   ADD CONSTRAINT [pk_user]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[extendeduser]
   ADD CONSTRAINT [uk_user1]
      UNIQUE (lowername)
GO

GRANT INSERT ON [brady_membership].[extendeduser] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[extendeduser] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[extendeduser] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[extendeduser] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[extendeduser] TO [membership_read]
GO

CREATE TABLE [brady_membership].[roleuser](
	[id] [bigint] NOT NULL,
	[roleid] [bigint] NOT NULL,
	[userid] [bigint] NOT NULL
) ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[roleuser]
   ADD CONSTRAINT [pk_roleuser]
      PRIMARY KEY CLUSTERED ( [id] ) 
         ON [PRIMARY]
GO

ALTER TABLE [brady_membership].[roleuser]
   ADD CONSTRAINT [uk_roleuser1]
      UNIQUE ([roleid], [userid])
GO

ALTER TABLE [brady_membership].[roleuser]
   ADD CONSTRAINT [fk_roleuser_user]
      FOREIGN KEY ([userid])
         REFERENCES [brady_membership].[extendeduser]([id])
            ON DELETE CASCADE
GO

ALTER TABLE [brady_membership].[roleuser]
   ADD CONSTRAINT [fk_roleuser_role]
      FOREIGN KEY ([roleid])
         REFERENCES [brady_membership].[extendedrole]([id])
            ON DELETE CASCADE
GO

GRANT INSERT ON [brady_membership].[roleuser] TO [membership_admin]
GO

GRANT UPDATE ON [brady_membership].[roleuser] TO [membership_admin]
GO

GRANT DELETE ON [brady_membership].[roleuser] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[roleuser] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[roleuser] TO [membership_read]
GO

CREATE VIEW [brady_membership].[userrole]
AS
   SELECT 
       ru.id, 
       ru.userid,
       u.name username,
       ru.roleid,
       r.name rolename
   FROM 
   [brady_membership].[roleuser] ru
   JOIN
   [brady_membership].[extendedrole] r ON r.id = ru.roleid
   JOIN
   [brady_membership].[extendeduser] u ON u.id = ru.userid
GO   

GRANT SELECT ON [brady_membership].[userrole] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[userrole] TO [membership_read]
GO

CREATE VIEW [brady_membership].[usersecurable]
AS
SELECT 
	ru.userid		userid,
	u.name			username,
	rs.securableid	securableid,
	s.name			securablename,
	s.typeid        securabletypeid,
    st.name         securabletypename,
	rs.privilegeid	privilegeid,
	p.name			privilegename,
	allow = CASE SUM(rs.allow) 
				WHEN 0 THEN 0
				ELSE 
					CASE SUM(rs.[deny]) 
						WHEN 0 THEN 1
						ELSE 0 
					END
			END,
	[deny] = CASE SUM(rs.[deny]) 
				WHEN 0 THEN 0
				ELSE 1
			END 
FROM
	[brady_membership].[roleuser] ru
	JOIN
	[brady_membership].[rolesecurable] rs ON rs.roleid = ru.roleid
	JOIN
	[brady_membership].[extendeduser] u ON u.id = ru.userid
	JOIN
	[brady_membership].[securable] s ON s.id = rs.securableid
	JOIN
	[brady_membership].[securabletype] st ON st.id = s.typeid
	JOIN
	[brady_membership].[privilege] p ON p.id = rs.privilegeid
GROUP BY 
	ru.userid,
	u.name,
	rs.securableid,
	s.name,
	s.typeid,
    st.name,
	rs.privilegeid,
	p.name
GO

GRANT SELECT ON [brady_membership].[usersecurable] TO [membership_admin]
GO

GRANT SELECT ON [brady_membership].[usersecurable] TO [membership_read]
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

INSERT INTO [brady_membership].[nexthigh] ([nexthigh], [entityname])
   VALUES (1, 'ExtendedRole')
GO

INSERT INTO [brady_membership].[nexthigh] ([nexthigh], [entityname])
   VALUES (1, 'ExtendedUser')
GO

INSERT INTO [brady_membership].[nexthigh] ([nexthigh], [entityname])
   VALUES (1, 'RoleUser')
GO

INSERT INTO [brady_membership].[nexthigh] ([nexthigh], [entityname])
   VALUES (1, 'RoleSecurable')
GO

INSERT INTO [brady_membership].[nexthigh] ([nexthigh], [entityname])
   VALUES (1, 'Securable')
GO

CREATE FUNCTION [brady_membership].[fn_isdba](@username AS NVARCHAR(256)) RETURNS INT
AS
BEGIN
	RETURN IS_ROLEMEMBER('db_owner', @username)
END
GO

GRANT EXECUTE ON [brady_membership].[fn_isdba] TO [membership_admin]
GO

CREATE PROCEDURE [brady_membership].[prc_createsecurable](@securabletype AS NVARCHAR(64),
														  @name          AS NVARCHAR(64))
AS 
BEGIN
   DECLARE @id BIGINT
   DECLARE @typeid BIGINT
   DECLARE @creatable INT
   DECLARE @privid BIGINT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'Securable'
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for securable', 16, 1)
   END
   
   SELECT @typeid = st.id, 
	      @creatable = st.creatable 
   FROM brady_membership.securabletype st 
   WHERE st.lowername = LOWER(@securabletype)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid securable type: %s', 16, 1, @securabletype)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'Securable'
      INSERT INTO brady_membership.securable(id, typeid, name)
         VALUES(@id, @typeid, @name)
            
      DECLARE priv_csr CURSOR FOR SELECT stp.privilegeid FROM brady_membership.securabletypeprivilege stp WHERE stp.securabletypeid = @typeid
      OPEN priv_csr
      FETCH NEXT FROM priv_csr INTO @privid;
      WHILE @@FETCH_STATUS = 0
      BEGIN
         INSERT INTO brady_membership.securableprivilege(securableid, privilegeid)
            VALUES (@id, @privid)

   	     FETCH NEXT FROM priv_csr INTO @privid
      END;
      CLOSE priv_csr
      DEALLOCATE priv_csr
   END
   ELSE
   BEGIN
      RAISERROR('Cannot create securable of type: %s', 16, 1, @securabletype)
   END 
END
GO
  
GRANT EXECUTE ON [brady_membership].[prc_createsecurable] TO [membership_admin]
GO

CREATE PROCEDURE [brady_membership].[prc_deletesecurable](@name NVARCHAR(64))
AS
BEGIN
   DECLARE @creatable INT
   SELECT @creatable = st.creatable 
   FROM brady_membership.securable s
	    JOIN 
	    brady_membership.securabletype st ON st.id = s.typeid
   WHERE s.lowername = LOWER(@name)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid securable: %s', 16, 1, @name)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_memebrship.fn_isdba(USER)
   END 
   
   IF @creatable != 0 
   BEGIN
      DELETE FROM brady_membership.securable WHERE lowername = LOWER(@name)
   END
   ELSE
   BEGIN
      RAISERROR('Cannot delete securable: %s', 16, 1, @name)
   END
END
GO
       
GRANT EXECUTE ON [brady_membership].[prc_deletesecurable] TO [membership_admin]
GO

CREATE PROCEDURE brady_membership.prc_createrole(@roletype NVARCHAR(64),
                                                 @name     NVARCHAR(256))
AS
BEGIN
   DECLARE @id BIGINT
   DECLARE @typeid BIGINT
   DECLARE @creatable INT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'ExtendedRole'
 
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for role', 16, 1)
   END 
   
   SELECT @typeid = rt.id, 
	      @creatable = rt.creatable 
   FROM brady_membership.roletype rt 
   WHERE rt.lowername = LOWER(@roletype)  
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid role type: %s', 16, 1, @roletype)
   END
      
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'ExtendedRole'
      INSERT INTO brady_membership.extendedrole(id, typeid, name)
         VALUES(@id, @typeid, @name)
   END
   ELSE
   BEGIN
      RAISERROR('Cannot create role of type: %s', 16, 1, @roletype)
   END
END
GO
       
GRANT EXECUTE ON brady_membership.prc_createrole TO membership_admin
GO

CREATE PROCEDURE [brady_membership].[prc_deleterole](@name NVARCHAR(256))
AS
BEGIN
   DECLARE @creatable INT
   SELECT @creatable = rt.creatable 
   FROM brady_membership.extendedrole r 
        JOIN
        brady_membership.roletype rt ON rt.id = r.typeid
   WHERE r.lowername = LOWER(@name)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid role: %s', 16, 1, @name)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      DELETE FROM brady_membership.extendedrole WHERE lowername = LOWER(@name)
   END
   ELSE
   BEGIN
      RAISERROR('Cannot delete role: %s', 16, 1, @name)
   END 
END
GO
       
GRANT EXECUTE ON [brady_membership].[prc_deleterole] TO [membership_admin]
GO

CREATE PROCEDURE [brady_membership].[prc_addrolesecurables](@role NVARCHAR(256),
                                                            @securable NVARCHAR(64))
AS
BEGIN
   
   DECLARE @id      BIGINT
   DECLARE @roleid  BIGINT
   DECLARE @securableid BIGINT
   DECLARE @privilegeid BIGINT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'RoleSecurable'
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for role securable', 16, 1)
   END
   
   SELECT @roleid = r.id FROM brady_membership.extendedrole r WHERE r.lowername = LOWER(@role)
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid role', 16, 1)
   END
   
   DECLARE securables_csr CURSOR FOR SELECT s.id securableid, sp.privilegeid 
									 FROM brady_membership.securable s JOIN 
                                          brady_membership.securableprivilege sp ON sp.securableid = s.id
                                     WHERE s.lowername = LOWER(@securable) AND
                                           NOT EXISTS (SELECT 1 
                                                       FROM brady_membership.rolesecurable rs 
                                                       WHERE rs.roleid = @roleid AND
                                                             rs.securableid = s.id AND
                                                             rs.privilegeid = sp.privilegeid)
   OPEN securables_csr;
   FETCH NEXT FROM securables_csr INTO @securableid, @privilegeid
   WHILE @@FETCH_STATUS = 0
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'RoleSecurable';         
      INSERT INTO brady_membership.rolesecurable(id, roleid, securableid, privilegeid, allow, [deny])
         VALUES (@id, @roleid, @securableid, @privilegeid, -1, 0);
      SET @id = @id + 1;
      FETCH NEXT FROM securables_csr INTO @securableid, @privilegeid
   END
   CLOSE securables_csr
   DEALLOCATE securables_csr
END
GO  
       
GRANT EXECUTE ON [brady_membership].[prc_addrolesecurables] TO [membership_admin];
GO

EXEC sp_addrolemember [aspnet_membership_fullaccess], [membership_admin] 
GO

EXEC sp_addrolemember [aspnet_roles_fullaccess], [membership_admin]
GO

EXEC sp_addrolemember [aspnet_membership_basicaccess], [membership_read]
GO

EXEC sp_addrolemember [aspnet_roles_basicaccess], [membership_read]
GO

brady_membership.prc_updateversion 2013, 1, 0, 1
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2013, 1, 0, 1, 2013, 1, 0, 2
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

CREATE INDEX [ifk_securabletypeprivilege1]
   ON [brady_membership].[securabletypeprivilege](securabletypeid)
GO

CREATE INDEX [ifk_securabletypeprivilege2]
   ON [brady_membership].[securabletypeprivilege](privilegeid)
GO
      
CREATE INDEX [ifk_securable1]
   ON [brady_membership].[securable](typeid)
GO

CREATE INDEX [ifk_securableprivilege1]
   ON [brady_membership].[securableprivilege](securableid)
GO

CREATE INDEX [ifk_securableprivilege2]
   ON [brady_membership].[securableprivilege](privilegeid)
GO

CREATE INDEX [ifk_extendedrole1]
   ON [brady_membership].[extendedrole](typeid)
GO
   
CREATE INDEX [ifk_rolesecurable1]
   ON [brady_membership].[rolesecurable](roleid)
GO
      
CREATE INDEX [ifk_rolesecurable2]
   ON [brady_membership].[rolesecurable](securableid, privilegeid)
GO

CREATE INDEX [ifk_roleuser1]
   ON [brady_membership].[roleuser](userid)
GO

CREATE INDEX [ifk_roleuser2]
   ON [brady_membership].[roleuser](roleid)
GO

ALTER TABLE [brady_membership].[securable]
   DROP CONSTRAINT [uk_securable1]
GO

ALTER TABLE [brady_membership].[securable]
   ADD CONSTRAINT [uk_securable1]
      UNIQUE (typeid, lowername)
GO

ALTER TABLE [brady_membership].[securableprivilege]
   DROP CONSTRAINT [fk_securableprivilege_securable]
GO

ALTER TABLE [brady_membership].[securableprivilege]
   ADD CONSTRAINT [fk_securableprivilege_securable]
      FOREIGN KEY (securableid)
         REFERENCES [brady_membership].[securable](id)
            ON DELETE CASCADE
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_createsecurable]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_createsecurable]
GO

CREATE PROCEDURE [brady_membership].[prc_createsecurable](@securabletype AS NVARCHAR(64),
														  @name          AS NVARCHAR(64))
AS 
BEGIN
   DECLARE @id BIGINT
   DECLARE @typeid BIGINT
   DECLARE @creatable INT
   DECLARE @privid BIGINT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'Securable'
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for securable', 16, 1)
   END
   
   SELECT @typeid = st.id, 
	      @creatable = st.creatable 
   FROM brady_membership.securabletype st 
   WHERE st.lowername = LOWER(@securabletype)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid securable type: %s', 16, 1, @securabletype)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'Securable'
      INSERT INTO brady_membership.securable(id, typeid, name)
         VALUES(@id, @typeid, @name)
            
      DECLARE priv_csr CURSOR FOR SELECT stp.privilegeid FROM brady_membership.securabletypeprivilege stp WHERE stp.securabletypeid = @typeid
      OPEN priv_csr
      FETCH NEXT FROM priv_csr INTO @privid;
      WHILE @@FETCH_STATUS = 0
      BEGIN
         INSERT INTO brady_membership.securableprivilege(securableid, privilegeid)
            VALUES (@id, @privid)

   	     FETCH NEXT FROM priv_csr INTO @privid
      END;
      CLOSE priv_csr
      DEALLOCATE priv_csr
   END
   ELSE
   BEGIN
      RAISERROR('Cannot create securable of type: %s', 16, 1, @securabletype)
   END 
END
GO
  
GRANT EXECUTE ON [brady_membership].[prc_createsecurable] TO [membership_admin]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_deletesecurable]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_deletesecurable]
GO

CREATE PROCEDURE [brady_membership].[prc_deletesecurable](@securabletype NVARCHAR(64),
                                                          @securablename NVARCHAR(64))
AS
BEGIN
   DECLARE @creatable INT
   DECLARE @typeid    BIGINT
   
   SELECT @creatable = st.creatable, 
          @typeid = typeid
   FROM brady_membership.securable s
	    JOIN 
	    brady_membership.securabletype st ON st.id = s.typeid
   WHERE 
       st.lowername = LOWER(@securabletype) AND
       s.lowername = LOWER(@securablename)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid securable: %s', 16, 1, @securablename)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_memebrship.fn_isdba(USER)
   END 
   
   IF @creatable != 0 
   BEGIN
      DELETE FROM brady_membership.securable 
         WHERE typeid = @typeid AND
               lowername = LOWER(@securablename)
   END
   ELSE
   BEGIN
      RAISERROR('Cannot delete securable: %s', 16, 1, @securablename)
   END
END
GO
       
GRANT EXECUTE ON [brady_membership].[prc_deletesecurable] TO [membership_admin]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_createrole]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_createrole]
GO

CREATE PROCEDURE brady_membership.prc_createrole(@roletype NVARCHAR(64),
                                                 @name     NVARCHAR(256))
AS
BEGIN
   DECLARE @id BIGINT
   DECLARE @typeid BIGINT
   DECLARE @creatable INT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'ExtendedRole'
 
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for role', 16, 1)
   END 
   
   SELECT @typeid = rt.id, 
	      @creatable = rt.creatable 
   FROM brady_membership.roletype rt 
   WHERE rt.lowername = LOWER(@roletype)  
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid role type: %s', 16, 1, @roletype)
   END
      
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'ExtendedRole'
      INSERT INTO brady_membership.extendedrole(id, typeid, name)
         VALUES(@id, @typeid, @name)
      EXEC dbo.aspnet_roles_createrole 'BradyMembership', @name
   END
   ELSE
   BEGIN
      RAISERROR('Cannot create role of type: %s', 16, 1, @roletype)
   END
END
GO
       
GRANT EXECUTE ON brady_membership.prc_createrole TO membership_admin
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_deleterole]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_deleterole]
GO

CREATE PROCEDURE [brady_membership].[prc_deleterole](@name NVARCHAR(256))
AS
BEGIN
   DECLARE @creatable INT
   
   SELECT @creatable = rt.creatable 
   FROM brady_membership.extendedrole r 
        JOIN
        brady_membership.roletype rt ON rt.id = r.typeid
   WHERE r.lowername = LOWER(@name)  
   
   IF @@ROWCOUNT = 0 
   BEGIN
      RAISERROR('Invalid role: %s', 16, 1, @name)
   END 
   
   IF @creatable = 0 
   BEGIN
      SET @creatable = brady_membership.fn_isdba(USER)
   END 

   IF @creatable != 0 
   BEGIN
      DELETE FROM brady_membership.extendedrole WHERE lowername = LOWER(@name)
      EXEC dbo.aspnet_roles_deleterole 'BradyMembership', p_name, 0
   END
   ELSE
   BEGIN
      RAISERROR('Cannot delete role: %s', 16, 1, @name)
   END 
END
GO
       
GRANT EXECUTE ON [brady_membership].[prc_deleterole] TO [membership_admin]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_addrolesecurables]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_addrolesecurables]
GO

CREATE PROCEDURE [brady_membership].[prc_addrolesecurables](@role NVARCHAR(256),
                                                            @securabletype NVARCHAR(64),
                                                            @securable NVARCHAR(64))
AS
BEGIN
   
   DECLARE @id      BIGINT
   DECLARE @roleid  BIGINT
   DECLARE @securableid BIGINT
   DECLARE @privilegeid BIGINT
   
   SELECT @id = nh.nexthigh 
   FROM brady_membership.nexthigh nh 
   WHERE nh.entityname = 'RoleSecurable'
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Failed to read next high for role securable', 16, 1)
   END
   
   SELECT @roleid = r.id FROM brady_membership.extendedrole r WHERE r.lowername = LOWER(@role)
   
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid role', 16, 1)
   END
   
   DECLARE securables_csr CURSOR FOR SELECT s.id securableid, sp.privilegeid 
									 FROM brady_membership.securable s 
									      JOIN 
                                          brady_membership.securabletype st ON st.id = s.typeid  
                                          JOIN
                                          brady_membership.securableprivilege sp ON sp.securableid = s.id
                                     WHERE st.lowername = LOWER(@securabletype) AND
                                           s.lowername = LOWER(@securable) AND
                                           NOT EXISTS (SELECT 1 
                                                       FROM brady_membership.rolesecurable rs 
                                                       WHERE rs.roleid = @roleid AND
                                                             rs.securableid = s.id AND
                                                             rs.privilegeid = sp.privilegeid)
   OPEN securables_csr;
   FETCH NEXT FROM securables_csr INTO @securableid, @privilegeid
   WHILE @@FETCH_STATUS = 0
   BEGIN
      UPDATE brady_membership.nexthigh SET nexthigh = @id + 1 WHERE entityname = 'RoleSecurable';         
      INSERT INTO brady_membership.rolesecurable(id, roleid, securableid, privilegeid, allow, [deny])
         VALUES (@id, @roleid, @securableid, @privilegeid, -1, 0);
      SET @id = @id + 1;
      FETCH NEXT FROM securables_csr INTO @securableid, @privilegeid
   END
   CLOSE securables_csr
   DEALLOCATE securables_csr
END
GO  
       
GRANT EXECUTE ON [brady_membership].[prc_addrolesecurables] TO [membership_admin];
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_addsecurableprivilege]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_addsecurableprivilege]
GO

CREATE PROCEDURE [brady_membership].[prc_addsecurableprivilege](@securabletype AS NVARCHAR(64),
																@securable AS NVARCHAR(64),
																@privilege AS NVARCHAR(64))
AS
BEGIN
   DECLARE @securableid BIGINT
   DECLARE @privilegeid BIGINT
   
   SELECT @securableid = s.id FROM brady_membership.securable s
								   JOIN
								   brady_membership.securabletype st ON st.id = s.typeid
						      WHERE
						           st.lowername = LOWER(@securabletype) AND
						           s.lowername = LOWER(@securable)

   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid securable', 16, 1)
   END						      
        
   SELECT @privilegeid = p.id FROM brady_membership.privilege p WHERE p.lowername = LOWER(@privilege)
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid privilege', 16, 1)
   END
   
   IF NOT EXISTS(SELECT 1 FROM brady_membership.securableprivilege sp
						  WHERE sp.securableid = @securableid AND 
						        sp.privilegeid = @privilege)
   BEGIN
      INSERT INTO brady_membership.securableprivilege(securableid, privilegeid)
               VALUES (@securableid, @privilegeid)
   END
   						        
END
GO

GRANT EXECUTE ON [brady_membership].[prc_addsecurableprivilege] TO [membership_admin]
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_removesecurableprivilege]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_removesecurableprivilege]
GO

CREATE PROCEDURE [brady_membership].[prc_removesecurableprivilege](@securabletype AS NVARCHAR(64),
																@securable AS NVARCHAR(64),
																@privilege AS NVARCHAR(64))
AS
BEGIN
   DECLARE @securableid BIGINT
   DECLARE @privilegeid BIGINT
   
   SELECT @securableid = s.id FROM brady_membership.securable s
								   JOIN
								   brady_membership.securabletype st ON st.id = s.typeid
						      WHERE
						           st.lowername = LOWER(@securabletype) AND
						           s.lowername = LOWER(@securable)

   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid securable', 16, 1)
   END						      
        
   SELECT @privilegeid = p.id FROM brady_membership.privilege p WHERE p.lowername = LOWER(@privilege)
   IF @@ROWCOUNT = 0
   BEGIN
      RAISERROR('Invalid privilege', 16, 1)
   END
   
   IF EXISTS(SELECT 1 FROM brady_membership.securableprivilege sp
						  WHERE sp.securableid = @securableid AND 
						        sp.privilegeid = @privilegeid)
   BEGIN
      DELETE FROM brady_membership.securableprivilege
               WHERE securableid = @securableid AND
                     privilegeid = @privilegeid
   END
   						        
END
GO

GRANT EXECUTE ON [brady_membership].[prc_removesecurableprivilege] TO [membership_admin]
GO

ALTER TABLE [brady_membership].[dbversion] ALTER COLUMN [version] NVARCHAR(100)
GO
ALTER TABLE [brady_membership].[dbversion] ADD [product] NVARCHAR(50)
GO
ALTER TABLE [brady_membership].[dbversion] ADD [productrevision] INT
GO

ALTER TABLE [brady_membership].[dbversion]
   ADD CONSTRAINT [uk_dbversion1]
      UNIQUE ([major], [minor], [patch], [compile],[product], [productrevision])
GO    
  
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_checkproductversion]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_checkproductversion]
GO

CREATE PROCEDURE brady_membership.prc_checkproductversion
    ( @OldMajor    INT
    , @OldMinor    INT
    , @OldPatch    INT
    , @OldCompile  INT
    , @OldProduct  NVARCHAR(50)
    , @OldProductRevision INT
    , @NewMajor    INT
    , @NewMinor    INT
    , @NewPatch    INT
    , @NewCompile  INT
    , @NewProduct  NVARCHAR(50)
    , @NewProductRevision INT
   )
AS
BEGIN
   --SET NOCOUNT ON
   DECLARE @msg NVARCHAR(400)
   
   IF NOT EXISTS (SELECT version FROM brady_membership.dbversion 
                                 WHERE major = @OldMajor AND 
                                       minor = @OldMinor AND 
                                       patch = @OldPatch AND 
                                       compile = @OldCompile AND 
                                       ((product IS NULL AND @OldProduct IS NULL) OR product = @OldProduct) AND
                                       ((productrevision IS NULL AND @OldProductRevision IS NULL) OR productrevision = @OldProductRevision))
   BEGIN 
      SET @msg = 'Cannot upgrade to ' + CONVERT(NVARCHAR(10), @NewMajor) + '.' + CONVERT(NVARCHAR(10), @NewMinor) + '.' + CONVERT(NVARCHAR(10), @NewPatch) + '.' + CONVERT(NVARCHAR(10), @NewCompile)
      IF (@NewProduct IS NOT NULL) 
      BEGIN
         SET @msg = @msg + ' ' + @NewProduct + ' revision ' + CONVERT(NVARCHAR(10), @NewProductRevision)
      END
      
      SET @msg = @msg + ' before database is upgraded to ' + CONVERT(NVARCHAR(10), @OldMajor) + '.' + CONVERT(NVARCHAR(10), @OldMinor) + '.' + CONVERT(NVARCHAR(10), @OldPatch) + '.' + CONVERT(NVARCHAR(10), @OldCompile)
      IF (@OldProduct IS NOT NULL) 
      BEGIN
         SET @msg = @msg + ' ' + @OldProduct + ' revision ' + CONVERT(NVARCHAR(10), @OldProductRevision)           
      END
        
      RAISERROR(@msg, 16, 1)
   END 
   
   IF EXISTS (SELECT version FROM brady_membership.dbversion 
                             WHERE major = @NewMajor AND 
                                   minor = @NewMinor AND 
                                   patch = @NewPatch AND 
                                   compile = @NewCompile AND
                                   ((product IS NULL AND @NewProduct IS NULL) OR product = @NewProduct) AND
                                   ((productrevision IS NULL AND @NewProductRevision IS NULL) OR productrevision = @NewProductRevision))
   BEGIN
      SET @msg = 'Cannot upgrade to ' + CONVERT(NVARCHAR(10), @NewMajor) + '.' + CONVERT(NVARCHAR(10), @NewMinor) + '.' + CONVERT(NVARCHAR(10), @NewPatch) + '.' + CONVERT(NVARCHAR(10), @NewCompile)
      IF (@NewProduct IS NOT NULL) 
      BEGIN
         SET @msg = @msg + ' ' + @NewProduct + ' revision ' + CONVERT(NVARCHAR(10), @NewProductRevision)
      END
      SET @msg = @msg + ' as database is already at this version or greater'
      RAISERROR(@msg, 16, 1)
   END 
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[brady_membership].[prc_updateproductversion]') AND type in (N'P'))
   DROP PROCEDURE [brady_membership].[prc_updateproductversion]
GO

CREATE PROCEDURE brady_membership.prc_updateproductversion
    ( @NewMajor    INT
    , @NewMinor    INT
    , @NewPatch    INT
    , @NewCompile  INT
    , @NewProduct  NVARCHAR(50)
    , @NewProductRevision INT
   )
AS
BEGIN
   DECLARE @version NVARCHAR(400)
   SET @version = (CAST(@NewMajor AS NVARCHAR(10)) + '.' + CAST(@NewMinor AS NVARCHAR(10)) + '.' + CAST(@NewPatch AS NVARCHAR(10)) + '.' + CAST(@NewCompile AS NVARCHAR(10))) + ' ' + @NewProduct + ' revision ' + CAST(@NewProductRevision AS NVARCHAR(10))
   
   INSERT INTO brady_membership.dbversion (version, major, minor, patch, compile, product, productrevision)
      VALUES (@version, @NewMajor, @NewMinor, @NewPatch, @NewCompile, @NewProduct, @NewProductRevision)
END
GO
  
--insert core privileges
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (1, 'Create Any')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (2, 'Read Any')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (3, 'Update Any')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (4, 'Delete Any')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (5, 'Execute Any')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (101, 'Create')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (102, 'Read')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (103, 'Update')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (104, 'Delete')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (105, 'Execute')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (106, 'Import')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (107, 'Export')
GO
   
INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (108, 'Open')
GO

INSERT INTO [brady_membership].[privilege](id,name)
   VALUES (109, 'Use')
GO

--create system securable type   
INSERT INTO [brady_membership].[securabletype](id, name, creatable)
   VALUES (1, 'System', 0)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (1, 1)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (1, 2)
GO 

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (1, 3)
GO 
   
INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (1, 4)
GO 
   
INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (1, 5)
GO

--create membership securable type   
INSERT INTO [brady_membership].[securabletype](id, name, creatable)
   VALUES (2, 'Membership', 0)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (2, 101)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (2, 102)
GO 

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (2, 103)
GO 
   
INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (2, 104)
GO
 
--create table securable type
INSERT INTO [brady_membership].[securabletype](id, name, creatable)
   VALUES (3, 'Table', 1)
GO
   
INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (3, 101)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (3, 102)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (3, 103)
GO

INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (3, 104)
GO

--create function securable type
INSERT INTO [brady_membership].[securabletype](id, name, creatable)
   VALUES (4, 'Function', 1)
GO
   
INSERT INTO [brady_membership].[securabletypeprivilege](securabletypeid, privilegeid)
   VALUES (4, 105)
GO
     
--create system and membership securables
[brady_membership].[prc_createsecurable] 'System', 'System'
GO
[brady_membership].[prc_createsecurable] 'Membership', 'User'
GO
[brady_membership].[prc_createsecurable] 'Membership', 'Role'
GO

--create system roles
--administrator role
[brady_membership].[prc_createrole] 'System', 'Administrator'
GO
[brady_membership].[prc_addrolesecurables] 'Administrator', 'System', 'System'
GO

--public role
[brady_membership].[prc_createrole] 'System', 'Public'
GO

[brady_membership].[prc_updateversion] 2013,1,0,2
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2013, 1, 0, 2, 2013, 3, 0, 1
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[servicesetting](
	[Id] [bigint] NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[Value] [nvarchar](2056) NOT NULL,
 CONSTRAINT [PK_servicesetting] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_servicesetting_1] UNIQUE NONCLUSTERED 
(
	[Name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

ALTER TABLE [brady_membership].[extendeduser] ADD
	culture nvarchar(32) NOT NULL CONSTRAINT DF_extendeduser_culture DEFAULT N'en-GB'
GO

INSERT INTO [brady_membership].[nexthigh]
           ([nexthigh]
           ,[entityname])
     VALUES
           (0
           ,'ServiceSetting')
GO

[brady_membership].[prc_updateversion] 2013, 3, 0, 1
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2013, 3, 0, 1, 2013, 3, 0, 2
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[sessionterminated](
	[Id] [bigint] NOT NULL,
	[SessionId] [nvarchar](64) NOT NULL,
	[Username] [nvarchar](256) NOT NULL,
	[Timestamp] [datetime] NOT NULL,
	[Reason] [nvarchar](64) NOT NULL,
 CONSTRAINT [PK_sessiontimeout] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY],
 CONSTRAINT [UK_sessionterminated_sessionid] UNIQUE NONCLUSTERED 
(
	[SessionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [brady_membership].[nexthigh]
           ([nexthigh]
           ,[entityname])
     VALUES
           (0
           ,'SessionTerminated')
GO

[brady_membership].[prc_updateversion] 2013, 3, 0, 2
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2013, 3, 0, 2, 2014, 1, 0, 1
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION
GO
EXECUTE sp_rename N'brady_membership.extendeduser.culture', N'Tmp_languageculture_2', 'COLUMN' 
GO
EXECUTE sp_rename N'brady_membership.extendeduser.Tmp_languageculture_2', N'languageculture', 'COLUMN' 
GO
ALTER TABLE brady_membership.extendeduser ADD
	formattingculture nvarchar(32) NOT NULL CONSTRAINT DF_extendeduser_formattingculture DEFAULT (N'en-GB')
GO
ALTER TABLE brady_membership.extendeduser SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

[brady_membership].[prc_updateversion] 2014, 1, 0, 1
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2014, 1, 0, 1, 2014, 1, 0, 2
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

BEGIN TRANSACTION
GO
ALTER TABLE brady_membership.extendeduser ADD
	datetimedisplayformat nvarchar(32) NOT NULL CONSTRAINT DF_extendeduser_datetimedisplayformat DEFAULT (N'dd-MMM-yyyy HH:mm:ss'),
	datedisplayformat nvarchar(32) NOT NULL CONSTRAINT DF_extendeduser_datedisplayformat DEFAULT (N'dd-MMM-yyyy')
GO
ALTER TABLE brady_membership.extendeduser SET (LOCK_ESCALATION = TABLE)
GO
COMMIT
GO

[brady_membership].[prc_updateversion] 2014, 1, 0, 2
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2014, 1, 0, 2, 2014, 1, 0, 3
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

[brady_membership].[prc_updateversion] 2014, 1, 0, 3
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2014,1,0,3,2014,3,0,1
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[BwfDataLevelPermission]
(
	[Id] [bigint] NOT NULL,
	[EntityType] [nvarchar](256) NOT NULL,
	[EntityId] [bigint] NOT NULL,
	[PermissionId] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL,
	[PermissionName] [nvarchar](256) NOT NULL,
	[EntityDescription] [nvarchar](256) NOT NULL
CONSTRAINT [PK_BwfDataLevelPermission] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



GO

INSERT INTO [brady_membership].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'BwfDataLevelPermission')

GO

CREATE TABLE [brady_membership].[BwfServiceLevelPermission]
(
	[Id] [bigint] NOT NULL,
	[RoleId] [bigint] NOT NULL,
	[Type] [nvarchar](256) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
	[RoleName] [nvarchar](256) NOT NULL
CONSTRAINT [PK_BwfServiceLevelPermission] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]



GO

INSERT INTO [brady_membership].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'BwfServiceLevelPermission')

GO

CREATE TABLE [brady_membership].[bwfuser]
(
	[id] [bigint] NOT NULL,
	[username] [nvarchar](256) NOT NULL,
	[firstname] [nvarchar](256),
	[lastname] [nvarchar](256),
	[emailaddress] [nvarchar](256),
	[languageculture] [nvarchar](32) NOT NULL,
	[formattingculture] [nvarchar](32) NOT NULL,
	[datetimedisplayformat] [nvarchar](32) NOT NULL,
	[datedisplayformat] [nvarchar](32) NOT NULL,
	[isapproved] [bit] NOT NULL
CONSTRAINT [PK_bwfuser] PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [brady_membership].[bwfuser] ADD  CONSTRAINT [UK_bwfuser_0] UNIQUE NONCLUSTERED 
(
	[username] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

GO

INSERT INTO [brady_membership].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'User')

GO

CREATE TABLE [brady_membership].[bwfrole]
(
	[id] [bigint] NOT NULL,
	[name] [nvarchar](256) NOT NULL,
	[description] [nvarchar](1024),
	[isadministrator] [bit] NOT NULL
CONSTRAINT [PK_bwfrole] PRIMARY KEY CLUSTERED
(
	[id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

ALTER TABLE [brady_membership].[bwfrole] ADD  CONSTRAINT [UK_bwfrole_0] UNIQUE NONCLUSTERED 
(
	[name] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

GO

INSERT INTO [brady_membership].[NextHigh]
	([NextHigh],
	[EntityName])
VALUES
	(1,
	'Role')

GO

CREATE TABLE [brady_membership].[bwfuser_bwfrole]
(
	[userid] [bigint] NOT NULL,
	[roleid] [bigint] NOT NULL

) ON [PRIMARY]
ALTER TABLE [brady_membership].[bwfuser_bwfrole]  WITH CHECK ADD  CONSTRAINT [FK_bwfuser_bwfrole_bwfuser] FOREIGN KEY([userid])
REFERENCES [brady_membership].[bwfuser] ([id])
GO
ALTER TABLE [brady_membership].[bwfuser_bwfrole]  WITH CHECK ADD  CONSTRAINT [FK_bwfuser_bwfrole_bwfrole] FOREIGN KEY([roleid])
REFERENCES [brady_membership].[bwfrole] ([id])
GO
ALTER TABLE [brady_membership].[bwfuser_bwfrole] ADD  CONSTRAINT [UK_bwfuser_bwfrole_0] UNIQUE NONCLUSTERED 
(
	[userid] ASC,[roleid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]

GO

[brady_membership].[prc_updateversion] 2014,3,0,1
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2014, 3, 0, 1, 2015, 3, 0, 1
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER TABLE [brady_membership].[bwfuser]
    ADD [userpicture] [varbinary](MAX) NULL
GO

ALTER TABLE [brady_membership].[bwfuser]
    ADD [userpicturetype] [nvarchar](32)
GO

[brady_membership].[prc_updateversion] 2015, 3, 0, 1
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2015, 3, 0, 1, 2015, 3, 0, 2
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[resetpasswordrequest] 
(
	Id bigint NOT NULL,
	UserId bigint NOT NULL,
	UniqueKey nvarchar(1024) NOT NULL,
	RequestedAt datetime NOT NULL,
	HasBeenUsed bit NOT NULL
CONSTRAINT [PK_resetpasswordrequest] PRIMARY KEY CLUSTERED
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

GO

INSERT INTO [brady_membership].[nexthigh]
	([nexthigh],
	[entityname])
VALUES (0,'ResetPasswordRequest')

GO

[brady_membership].[prc_updateversion] 2015, 3, 0, 2
GO

SET NOEXEC OFF

BEGIN TRY
   EXEC [brady_membership].[prc_checkversion] 2015, 3, 0, 2, 2015, 3, 0, 3
END TRY
BEGIN CATCH
   PRINT ERROR_MESSAGE()
   SET NOEXEC ON
END CATCH
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [brady_membership].[bwfaudit]
(
	id bigint NOT NULL,
	action nvarchar(50) NOT NULL,
	timestamp datetime2(7) NOT NULL,
	username nvarchar(50) NULL
	CONSTRAINT [PK_bwfaudit] PRIMARY KEY CLUSTERED 
	(
		[id] ASC
	)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]

CREATE TABLE [brady_membership].[a_bwfuser]
(
	bwf_auditId bigint NOT NULL,
	bwf_actionId bigint NOT NULL,
	bwf_auditSummary nvarchar(1024) NOT NULL,
	id bigint NOT NULL,
	username nvarchar(256) NOT NULL,
	firstname nvarchar(256) NULL,
	lastname nvarchar(256) NULL,
	emailaddress nvarchar(256) NULL,
	languageculture nvarchar(32) NOT NULL,
	formattingculture nvarchar(32) NOT NULL,
	datetimedisplayformat nvarchar(32) NOT NULL,
	datedisplayformat nvarchar(32) NOT NULL,
	isapproved bit NOT NULL,
	userpicture varbinary(MAX) NULL,
	userpicturetype nvarchar(32) NULL
	CONSTRAINT [PK_a_bwfuser] PRIMARY KEY CLUSTERED
	(
		[bwf_auditId] ASC
	)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [brady_membership].[a_bwfrole] 
(
	bwf_auditId bigint NOT NULL,
	bwf_actionId bigint NOT NULL,
	bwf_auditSummary nvarchar(1024) NOT NULL,
	id bigint NOT NULL,
	name nvarchar(256) NOT NULL,
	description nvarchar(1024) NULL,
	isadministrator bit NOT NULL
	CONSTRAINT [PK_a_bwfrole] PRIMARY KEY CLUSTERED
	(
		[bwf_auditId] ASC
	)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

CREATE TABLE [brady_membership].[a_bwfuser_bwfrole] 
(
	bwf_auditId bigint NOT NULL,
	bwf_actionId bigint NOT NULL,
	bwf_auditSummary nvarchar(1024) NOT NULL,
	userid bigint NOT NULL,
	roleid bigint NOT NULL
	CONSTRAINT [PK_a_bwfuser_bwfrole] PRIMARY KEY CLUSTERED
	(
		[bwf_auditId] ASC
	)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'BwfAudit')
INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'RoleAudit')
INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'UserAudit')
INSERT INTO [brady_membership].[NextHigh] ([NextHigh], [EntityName]) VALUES (0, 'UserRoleAudit')
GO

[brady_membership].[prc_updateversion] 2015, 3, 0, 3
GO

SET NOEXEC OFF