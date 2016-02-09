CREATE TABLE dbo.Licenses(
	[Id] UNIQUEIDENTIFIER PRIMARY KEY,
	[Server] VARCHAR(250) NOT NULL,
	[Name] VARCHAR(250) NOT NULL,
	[Qty] INT NOT NULL,
	[ExpireDate] DATETIME NOT NULL,
	[Activated] BIT NOT NULL DEFAULT(0),
	[CreationDate] DATETIME NOT NULL,
	[ActivationDate] DATETIME NULL
)
GO

CREATE PROCEDURE dbo.GetLicenseInfo @Id UNIQUEIDENTIFIER
AS
SELECT
	[Id],
	[Server],
	[Name],
	[Qty],
	[ExpireDate],
	[Activated],
	[CreationDate],
	[ActivationDate]
FROM dbo.Licenses
WHERE [Id] = @Id
GO

CREATE PROCEDURE dbo.CreateLicense @Id UNIQUEIDENTIFIER, @Server VARCHAR(250), @Name VARCHAR(250), @Qty INT, @ExpireDate DATETIME
AS
SET NOCOUNT ON
INSERT INTO dbo.Licenses(
	[Id],
	[Server],
	[Name],
	[Qty],
	[ExpireDate],
	[Activated],
	[CreationDate],
	[ActivationDate]
)
VALUES(
	@Id,
	@Server,
	@Name,
	@Qty,
	@ExpireDate,
	0,
	GETDATE(),
	NULL
)
GO

CREATE PROCEDURE dbo.ActivateLicense @Id UNIQUEIDENTIFIER, @Server VARCHAR(250), @Name VARCHAR(250), @Qty INT, @ExpireDate DATETIME
AS
SET NOCOUNT ON
UPDATE dbo.Licenses SET 
	[Activated] = 1, 
	[ActivationDate] = GETDATE() 
WHERE 
	[Id] = @Id AND
	[Server] = @Server AND
	[Name] = @Name AND
	[Qty] = @Qty AND
	[ExpireDate] = @ExpireDate
GO


