using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Services.Database
{
    public class SettingsDatabaseService : BaseDatabaseService
    {
        private static SettingsDatabaseService _uniqueInstance = null;
        private static object _lockObject = new object();

        private NPoco.Database _settingsDatabase;

        private SettingsDatabaseService()
        {
            InitDatabase();
        }

        public static SettingsDatabaseService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new SettingsDatabaseService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void InitDatabase()
        {
            _settingsDatabase = GetDatabase();
            var jm = _settingsDatabase.ExecuteScalar<string>("PRAGMA journal_mode");
            if (string.Compare(jm, "wal", true) != 0)
            {
                _settingsDatabase.Execute("PRAGMA journal_mode = WAL");
            }
            ExecuteWithinTransaction(_settingsDatabase, (db) =>
            {
                db.Execute(@"create table if not exists User(
Id integer PRIMARY KEY,
Name text,
Password text,
PreferredLanguage text,
MemberTypeId integer,
IsAdmin bit
)");
                db.Execute(@"create table if not exists GCComAccount(
Id integer PRIMARY KEY,
UserId integer,
AvatarUrl text,
MemberTypeId integer,
PublicGuid text,
GCComName text,
Token text,
TokenFromDate datetime
)");
                db.Execute(@"create table if not exists GSAKDatabase(
Id integer PRIMARY KEY,
UserId integer,
Name text,
Description text,
CreatedAt datetime
)");
            });
        }

        protected override NPoco.Database GetDatabase()
        {
            var fn = Path.Combine(Startup.HostingEnvironment.ContentRootPath, "App_Data");
            if (!Directory.Exists(fn))
            {
                Directory.CreateDirectory(fn);
            }
            fn = Path.Combine(fn, "settings.db3");
            var con = new SqliteConnection(string.Format("data source={0}", fn));
            con.Open();
            //con.CreateCollation("nocase", new Comparison<string>((a, b) => { return string.Compare(a, b, true); }));
            return new GAPPOnlineDatabase(this, con);
        }

        public override void Execute(Action<NPoco.Database> action)
        {
            Execute(_settingsDatabase, action);
        }

        public override T GetPage<T, T2>(int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql)
        {
            T result;
            result = GetPage<T, T2>(_settingsDatabase, page, pageSize, sortOn, sortAsc, defaultSort, sql);
            return result;
        }

        public override bool ExecuteWithinTransaction(Action<NPoco.Database> action)
        {
            bool result = false;
            result = ExecuteWithinTransaction(_settingsDatabase, action);
            return result;
        }


    }

}
