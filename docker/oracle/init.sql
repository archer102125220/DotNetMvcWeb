-- 切換到預設的 Pluggable Database (FREEPDB1)
ALTER SESSION SET CONTAINER = FREEPDB1;

-- 如果需要重置，可以解除註解底下這行 (如果 user 已經存在會報錯，初次建立可忽略)
-- DROP USER dotnet_mvc_user CASCADE;

-- 建立應用程式專用的 User (Schema)，這在 Oracle 中等同於建立一個獨立的 DB 空間
CREATE USER dotnet_mvc_user IDENTIFIED BY "MvcOracleDb123!" DEFAULT TABLESPACE users QUOTA UNLIMITED ON users;

-- 賦予基本連線與資源權限
GRANT CONNECT, RESOURCE TO dotnet_mvc_user;
GRANT CREATE SESSION, CREATE TABLE, CREATE VIEW, CREATE SEQUENCE, CREATE PROCEDURE TO dotnet_mvc_user;
