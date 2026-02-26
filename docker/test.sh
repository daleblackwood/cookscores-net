#!/bin/bash
set -e

service postgresql start > /dev/null
su postgres -c "psql -q" <<'SQL'
CREATE USER cookscores PASSWORD 'cookscores';
CREATE DATABASE cookscores OWNER cookscores;
SQL

export ConnectionStrings__CookScores="Host=127.0.0.1;Database=cookscores;Username=cookscores;Password=cookscores"
cd /app
dotnet CookScores.dll install

if [ "$1" = "serve" ]; then
    exec dotnet CookScores.dll
fi

dotnet CookScores.dll > /dev/null &
until (exec 3<> /dev/tcp/127.0.0.1/8080) 2> /dev/null; do sleep 0.5; done
dotnet CookScores.dll test http://localhost:8080/
