#!/bin/bash

# 啟動 SQL Server，並放在背景執行
/opt/mssql/bin/sqlservr &
PID=$!

echo "Waiting for SQL Server to start..."

# 根據 SQL Server 版本，尋找 sqlcmd 的路徑
SQLCMD="/opt/mssql-tools18/bin/sqlcmd"
if [ ! -f "$SQLCMD" ]; then
    SQLCMD="/opt/mssql-tools/bin/sqlcmd"
fi

# 等待 SQL Server 啟動 (使用 -C 來信任憑證)
until $SQLCMD -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -C -Q "SELECT 1" &> /dev/null
do
    echo "SQL Server is starting up..."
    sleep 2
done

echo "SQL Server is up - running initialization script..."

# 執行 DB 初始化腳本
$SQLCMD -S localhost -U sa -P "${MSSQL_SA_PASSWORD}" -C -i /docker-entrypoint-initdb.d/init.sql

echo "Initialization finished."

# 等待背景的 SQL Server 程序，確保容器不會提早結束
wait $PID
