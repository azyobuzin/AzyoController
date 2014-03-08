using System.Data.SQLite;
using System.Threading.Tasks;
using Codeplex.Data;

namespace RemoteControlAdapter.Model.Databases
{
    public static class Database
    {
        public static DbExecutor Connect()
        {
            var exec = new DbExecutor(new SQLiteConnection(Settings.DatabaseName));
            exec.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS MyTweets (Id INTEGER PRIMARY KEY, Text TEXT, CreatedAt INTEGER, UserId INTEGER)");
            exec.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Words (UserId INTEGER, Word TEXT, Count INTEGER)");
            exec.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS Operations (Channel INTEGER, Start INTEGER, End INTEGER)");
            return exec;
        }

        public static Task<DbExecutor> ConnectAsync()
        {
            return Task.Run(() => Connect());
        }
    }
}
