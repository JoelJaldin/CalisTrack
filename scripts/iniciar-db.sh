#!/usr/bin/env bash
set -euo pipefail
calistrack_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
calistrack_tools="$(dirname "$calistrack_root")/.calistrack-tools"
calistrack_sql="$calistrack_tools/sqlserver/opt/mssql/bin/sqlservr"
if [[ ! -x "$calistrack_sql" || ! -f "$calistrack_root/.local/sqlserver/sa-password" ]]; then
    echo 'SQL Server local no está preparado. Seguí la guía de README.md para configurar una instancia.'
    exit 1
fi
export ACCEPT_EULA=Y
export MSSQL_PID=Developer
export MSSQL_SA_PASSWORD_FILE="$calistrack_root/.local/sqlserver/sa-password"
export MSSQL_DATA_DIR="/var/opt/mssql/data"
export MSSQL_LOG_DIR="/var/opt/mssql/data"
export MSSQL_MASTER_DATA_FILE="/var/opt/mssql/data/master.mdf"
export MSSQL_MASTER_LOG_FILE="/var/opt/mssql/data/mastlog.ldf"
export MSSQL_ERROR_LOG_FILE="/var/opt/mssql/log/errorlog"
export MSSQL_MACHINE_KEY_FILE="/var/opt/mssql/secrets/machine-key"
export MSSQL_MEMORY_LIMIT_MB=2048
export MSSQL_TCP_PORT=14333
export MSSQL_DISABLE_COREDUMP=1
cd "$calistrack_root/.local/sqlserver"
exec "$calistrack_sql"
