using System.Data.SQLite;
using System.Threading.Tasks;
using Codeplex.Data;

namespace RemoteControlAdapter.Model.Databases
{
    public static class Database
    {
        public static Task<DbExecutor> Connect()
        {
            return Task.Run(() =>
            {
                var exec = new DbExecutor(new SQLiteConnection(Settings.DatabaseName));
                exec.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS MyTweets (Id INTEGER PRIMARY KEY, Text TEXT, CreatedAt INTEGER, UserId INTEGER)");
                exec.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Words (UserId INTEGER, Word TEXT, Count INTEGER)");
                return exec;
            });
        }
    }
}
