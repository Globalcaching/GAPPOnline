using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Services.Database
{
    public class GSAKDatabaseInstance : BaseDatabaseService
    {
        private NPoco.Database _gsakDatabase;
        private string _fileName;

        private GSAKDatabaseInstance(string filename)
        {
            _fileName = filename;
            InitDatabase();
        }

        private void InitDatabase()
        {
            _gsakDatabase = GetDatabase();
        }

        protected override NPoco.Database GetDatabase()
        {
            var con = new SqliteConnection(string.Format("data source={0}", _fileName));
            con.Open();
            //con.CreateCollation("gsaknocase", new Comparison<string>((a, b) => { return string.Compare(a, b, true); }));
            return new GAPPOnlineDatabase(this, con);
        }

        public override void Execute(Action<NPoco.Database> action)
        {
            Execute(_gsakDatabase, action);
        }

        public override T GetPage<T, T2>(int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql)
        {
            T result;
            result = GetPage<T, T2>(_gsakDatabase, page, pageSize, sortOn, sortAsc, defaultSort, sql);
            return result;
        }

        public override bool ExecuteWithinTransaction(Action<NPoco.Database> action)
        {
            bool result = false;
            result = ExecuteWithinTransaction(_gsakDatabase, action);
            return result;
        }

        public override void Dispose()
        {
            if (_gsakDatabase != null)
            {
                _gsakDatabase.Dispose();
                _gsakDatabase = null;
            }
            base.Dispose();
        }


        public static void Execute(string filename, Action<NPoco.Database> action)
        {
            using (var instance = new GSAKDatabaseInstance(filename))
            {
                instance.Execute(action);
            }
        }

        public static T GetPage<T, T2>(string filename, int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql) 
            where T : new()
        {
            T result;
            using (var instance = new GSAKDatabaseInstance(filename))
            {
                result = instance.GetPage<T, T2>(page, pageSize, sortOn, sortAsc, defaultSort, sql);
            }
            return result;
        }

        public static bool ExecuteWithinTransaction(string filename, Action<NPoco.Database> action)
        {
            bool result = false;
            using (var instance = new GSAKDatabaseInstance(filename))
            {
                result = instance.ExecuteWithinTransaction(action);
            }
            return result;
        }

    }

}
