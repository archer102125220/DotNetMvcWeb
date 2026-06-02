# Oracle Database 常用指令與操作指南

本指南整理了在開發環境中，與 Oracle Database 互動最常使用的各類指令。涵蓋了 Docker 容器操作、連線工具 (`sqlplus`) 以及一些 Oracle 特有的 SQL 語法。

## 1. Docker 容器管理指令

我們在本地端是透過 Docker 運行 Oracle 23ai Free 版本。

*   **啟動現有的 Oracle 容器**
    ```bash
    docker start dot-net-mvc-web-oracle-free-db
    ```
*   **停止 Oracle 容器**
    ```bash
    docker stop dot-net-mvc-web-oracle-free-db
    ```
*   **查看 Oracle 容器運行狀態**
    ```bash
    docker ps -a | grep oracle
    ```

## 2. 進入資料庫 (使用 SQL*Plus)

`sqlplus` 是 Oracle 官方的終端機連線工具。您可以直接在 Docker 容器內執行它，進行資料庫操作。

*   **以一般使用者身份進入 SQL*Plus**
    ```bash
    docker exec -it dot-net-mvc-web-oracle-free-db sqlplus myuser/mypassword@//localhost:1521/FREEPDB1
    ```
*   **以系統管理員 (SYSDBA) 身份進入 SQL*Plus** (若需執行權限管理或建立使用者)
    ```bash
    docker exec -it dot-net-mvc-web-oracle-free-db sqlplus sys/myadminpassword@//localhost:1521/FREEPDB1 as sysdba
    ```

### SQL*Plus 內部常用快捷指令
當您成功進入 `SQL> ` 提示字元後，可以使用以下系統指令：
*   `exit` 或 `quit`：離開 SQL*Plus 回到一般終端機。
*   `clear screen` 或 `cl scr`：清除畫面。
*   `describe [TableName];` 或 `desc [TableName];`：查看某張資料表的結構與欄位設計。

---

## 3. Oracle 常用 SQL 指令

Oracle 的 SQL 語法與 MySQL 或 SQL Server 有些微不同，以下整理開發時最常用到的指令。

### 查看結構與中繼資料 (Metadata)
*   **列出目前使用者擁有的所有資料表 (Tables)**
    ```sql
    SELECT table_name FROM user_tables;
    ```
*   **列出所有的 Sequence (序列/流水號)**
    ```sql
    SELECT sequence_name FROM user_sequences;
    ```

### 基本 CRUD 操作
*   **查詢資料 (注意：EF Core 建立的表通常需加雙引號)**
    ```sql
    SELECT * FROM "OracleDemoItems";
    ```
*   **分頁查詢 (Oracle 12c+ 的新語法)**
    ```sql
    -- 略過前 10 筆，取接下來的 5 筆資料
    SELECT * FROM "OracleDemoItems"
    ORDER BY "Id"
    OFFSET 10 ROWS FETCH NEXT 5 ROWS ONLY;
    ```
*   **新增資料**
    ```sql
    INSERT INTO "OracleDemoItems" ("Name", "Description", "CreatedAt")
    VALUES ('測試名稱', '這是一段描述', SYSDATE);
    ```
    > 💡 **提示**：`SYSDATE` 或是 `CURRENT_TIMESTAMP` 是 Oracle 取得當前資料庫系統時間的內建變數。

### DUAL 虛擬表
Oracle 強制規定所有的 `SELECT` 語句都**必須**有 `FROM`。如果您只想計算簡單的算式或取得時間，必須從內建的 `DUAL` 虛擬表中查詢：
```sql
SELECT SYSDATE FROM DUAL;
SELECT 1 + 1 FROM DUAL;
```

---

## 4. 常見問題與踩坑 (Gotchas)

1.  **區分大小寫 (Case Sensitivity)**
    在 Oracle 中，如果您建表時沒有加雙引號，所有的名稱都會被轉成**全大寫**。但因為我們使用的是 EF Core，EF Core 預設會幫我們對欄位和表名加上雙引號，這會讓 Oracle 變成**嚴格區分大小寫**。
    所以下 SQL 指令時，請務必記得加上雙引號：
    ```sql
    -- ❌ 錯誤 (會報錯 Table or view does not exist)
    SELECT * FROM OracleDemoItems;
    
    -- ✅ 正確
    SELECT * FROM "OracleDemoItems";
    ```

2.  **IDENTITY 欄位與 EF Core 種子資料衝突 (ORA-00001)**
    如果您在 EF Core 的 `OnModelCreating` 裡面使用了 `HasData` 手動指定 Id (如 Id=1, 2, 3) 來塞入種子資料，Oracle 內部的 Identity 流水號計數器**不會**自動跟著跳過這些 Id。當您後續想要從應用程式 Insert 新資料時，資料庫可能會嘗試給它 Id=1，進而發生 Primary Key 重複 (`ORA-00001`) 的錯誤。
    *解決方法*：實務上，若要在 Oracle 使用 `HasData`，建議將固定種子資料的 Id 設為負數 (例如 `-1`, `-2`)，或者改用我們示範的 `DbInitializer.cs` 來動態新增種子資料。
