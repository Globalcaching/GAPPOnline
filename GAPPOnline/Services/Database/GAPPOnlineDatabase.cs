using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Services.Database
{
    public class GAPPOnlineDatabase : NPoco.Database
    {
        private BaseDatabaseService _service;

        public GAPPOnlineDatabase(BaseDatabaseService service, DbConnection connection) : base(connection)
        {
            _service = service;
            KeepConnectionAlive = false;
        }

        protected override void OnExecutingCommand(DbCommand cmd)
        {
            base.OnExecutingCommand(cmd);
        }

        public override object Insert<T>(string tableName, string primaryKeyName, bool autoIncrement, T poco)
        {
            var result = base.Insert<T>(tableName, primaryKeyName, autoIncrement, poco);
            _service.OnRecordChanged(this, poco, LocalizationDatabaseService.RecordChange.Added);
            return result;
        }

        public override int Delete(string tableName, string primaryKeyName, object poco, object primaryKeyValue)
        {
            var result = base.Delete(tableName, primaryKeyName, poco, primaryKeyValue);
            _service.OnRecordChanged(this, poco, LocalizationDatabaseService.RecordChange.Deleted);
            return result;
        }

        public override int Update(string tableName, string primaryKeyName, object poco, object primaryKeyValue, IEnumerable<string> columns)
        {
            var result = base.Update(tableName, primaryKeyName, poco, primaryKeyValue, columns);
            _service.OnRecordChanged(this, poco, LocalizationDatabaseService.RecordChange.Updated);
            return result;
        }
    }
}
