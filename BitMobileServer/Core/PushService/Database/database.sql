CREATE TABLE [admin].UserPushTokens(
	[UserId] UNIQUEIDENTIFIER NOT NULL,
	[DeviceId] VARCHAR(250) NOT NULL,
	[Token] VARCHAR(250) NOT NULL
)
GO

ALTER TABLE [admin].UserPushTokens ADD CONSTRAINT UQ_UserPushTokens UNIQUE([UserId],[DeviceId],[Token])
GO

CREATE PROCEDURE [admin].[GetUserPushTokens] @UserId UNIQUEIDENTIFIER
AS
SELECT [Token] FROM [admin].UserPushTokens WHERE [UserId] = @UserId
GO

CREATE PROCEDURE [admin].[RegisterPushToken] @UserId UNIQUEIDENTIFIER, @DeviceId VARCHAR(250), @Token VARCHAR(250)
AS
IF NOT EXISTS(SELECT * FROM [admin].[UserPushTokens] WHERE [UserId] = @UserId AND DeviceId = @DeviceId AND Token = @Token)
	INSERT INTO [admin].UserPushTokens([UserId],[DeviceId],[Token]) VALUES(@UserId, @DeviceId, @Token)
GO
