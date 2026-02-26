#!/bin/bash
set -e
cd "$(dirname "$0")"

CONF=appsettings.json

if ! command -v dotnet > /dev/null; then
    echo "The .NET 9 SDK is required." >&2
    exit 1
fi

ask() {
    if [ -z "${!1}" ]; then
        read -r -p "$2 [$3]: " value
        printf -v "$1" '%s' "${value:-$3}"
    fi
}

if [ ! -f "$CONF" ]; then
    ask DB_HOST "Database host" localhost
    ask DB_PORT "Database port" 5432
    ask DB_NAME "Database name" cookscores
    ask DB_USER "Database user" postgres
    if [ -z "$DB_PASS" ]; then
        read -r -s -p "Database password: " DB_PASS
        echo
    fi
    printf '{\n  "ConnectionStrings": {\n    "CookScores": "Host=%s;Port=%s;Database=%s;Username=%s;Password=%s"\n  }\n}\n' \
        "$DB_HOST" "$DB_PORT" "$DB_NAME" "$DB_USER" "$DB_PASS" > "$CONF"
    echo "Wrote $CONF"
fi

dotnet run -- install
