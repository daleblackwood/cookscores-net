# cookscores

Recommended only for small game demos, not tested at production scale.

A small .NET 9 Minimal API + PostgreSQL high-score API for games. Version: see `VERSION`.

**Install:** with the .NET 9 SDK and a PostgreSQL database, run `./install.sh`. It writes `appsettings.json` and creates the tables. You can re-run it to upgrade. Then `dotnet run`. The connection string can also come from `ConnectionStrings__CookScores`.

**Endpoints** (`/api/<method>`): `heartbeat`, `score` (POST JSON), `scoreboard`, `replay`, `name` (POST JSON), `stats`. Responses are lowerCamelCase JSON with nulls omitted; errors are `{ "error": "..." }`.

**Test** (local only): `dotnet run -- test http://localhost:5000/`

**Docker (testing):** `docker build -f Dockerfile.test -t cookscores-test . && docker run --rm cookscores-test` runs the tests. Use `docker run --rm -p 8080:8080 cookscores-test serve` to run the API for manual testing.
