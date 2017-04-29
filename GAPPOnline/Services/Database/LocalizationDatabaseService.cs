using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Services.Database
{
    public class LocalizationDatabaseService : BaseDatabaseService
    {
        private static LocalizationDatabaseService _uniqueInstance = null;
        private static object _lockObject = new object();

        private NPoco.Database _localizationDatabase;

        private LocalizationDatabaseService()
        {
            InitDatabase();
        }

        public static LocalizationDatabaseService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new LocalizationDatabaseService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void InitDatabase()
        {
            _localizationDatabase = GetDatabase();
            var jm = _localizationDatabase.ExecuteScalar<string>("PRAGMA journal_mode");
            if (string.Compare(jm, "wal", true) != 0)
            {
                _localizationDatabase.Execute("PRAGMA journal_mode = WAL");
            }
            ExecuteWithinTransaction(_localizationDatabase, (db) =>
            {
                db.Execute(@"create table if not exists LocalizationCulture(
Id integer PRIMARY KEY,
Name text,
Description text
)");
                db.Execute(@"create table if not exists LocalizationOriginalText(
Id integer PRIMARY KEY,
OriginalText text
)");
                db.Execute(@"create table if not exists LocalizationTranslation(
Id integer PRIMARY KEY,
LocalizationCultureId integer not null,
LocalizationOriginalTextId integer not null,
TranslatedText text
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
            fn = Path.Combine(fn, "localization.db3");
            var con = new SqliteConnection(string.Format("data source={0}", fn));
            con.Open();
            return new GAPPOnlineDatabase(this, con);
        }

        public override void Execute(Action<NPoco.Database> action)
        {
            Execute(_localizationDatabase, action);
        }

        public override T GetPage<T, T2>(int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql)
        {
            T result;
            result = GetPage<T, T2>(_localizationDatabase, page, pageSize, sortOn, sortAsc, defaultSort, sql);
            return result;
        }

        public override bool ExecuteWithinTransaction(Action<NPoco.Database> action)
        {
            bool result = false;
            result = ExecuteWithinTransaction(_localizationDatabase, action);
            return result;
        }
    }

}
