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
            ExecuteWithinTransaction((db) =>
            {
                db.Execute(@"create table if not exists LocalizationCulture(
Id integer,
Name text,
Description text
)");
                db.Execute(@"create table if not exists LocalizationOriginalText(
Id integer,
OriginalText text
)");
                db.Execute(@"create table if not exists LocalizationTranslation(
Id integer,
LocalizationCultureId integer,
LocalizationOriginalTextId integer,
TranslatedText text
)");
            });
        }


        public override NPoco.Database GetDatabase()
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
    }

}
