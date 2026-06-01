-- 檢查並建立資料庫
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'DotNetMvcDb')
BEGIN
    CREATE DATABASE [DotNetMvcDb];
END
GO

USE [DotNetMvcDb];
GO

-- 檢查並建立 Login
IF NOT EXISTS (SELECT * FROM sys.server_principals WHERE name = N'AppUser')
BEGIN
    CREATE LOGIN [AppUser] WITH PASSWORD = 'AppUser!123456789';
END
GO

-- 檢查並建立 User
IF NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = N'AppUser')
BEGIN
    CREATE USER [AppUser] FOR LOGIN [AppUser];
    -- 賦予 db_owner 權限給應用程式帳號
    ALTER ROLE db_owner ADD MEMBER [AppUser];
END
GO
