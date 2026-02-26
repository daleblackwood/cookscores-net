namespace CookScores;

public static class Install
{
    public static async Task Run(Db db)
    {
        await db.Exec(@"
            CREATE TABLE IF NOT EXISTS scores (
                sid SERIAL PRIMARY KEY,
                game VARCHAR(50) NOT NULL,
                pid VARCHAR(50) NOT NULL,
                score INT NOT NULL,
                level VARCHAR(32),
                seed INT DEFAULT 0,
                time INT DEFAULT 0,
                ts TIMESTAMPTZ NOT NULL DEFAULT now(),
                data JSONB
            );
            CREATE TABLE IF NOT EXISTS replays (
                sid INT PRIMARY KEY REFERENCES scores(sid) ON DELETE CASCADE,
                replay TEXT NOT NULL,
                data JSONB
            );
            CREATE TABLE IF NOT EXISTS players (
                pid VARCHAR(100) PRIMARY KEY,
                pname VARCHAR(100) NOT NULL,
                email VARCHAR(100),
                password VARCHAR(100),
                data JSONB
            );
            CREATE TABLE IF NOT EXISTS meta (
                k VARCHAR(50) PRIMARY KEY,
                v VARCHAR(255) NOT NULL
            );");
        await db.Exec(@"
            INSERT INTO meta (k, v) VALUES ('version', @version)
            ON CONFLICT (k) DO UPDATE SET v = excluded.v", new { version = Api.Version });
        Console.WriteLine($"cookscores {Api.Version} installed.");
    }
}
