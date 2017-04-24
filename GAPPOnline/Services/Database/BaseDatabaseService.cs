using GAPPOnline.Models;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GAPPOnline.Services.Database
{
    public class BaseDatabaseService : BaseService
    {
        private static Dictionary<Type, long> _cachedIds = new Dictionary<Type, long>();

        public enum RecordChange
        {
            Added,
            Updated,
            Deleted
        }

        public delegate void RecordChangedHandler(NPoco.Database db, object record, RecordChange action);
        public event RecordChangedHandler RecordChanged;

        public void OnRecordChanged(NPoco.Database db, object record, RecordChange action)
        {
            RecordChanged?.Invoke(db, record, action);
        }

        public virtual NPoco.Database GetDatabase()
        {
            return null;
        }

        public T Save<T>(NPoco.Database db, T item) where T: AutoIdModel
        {
            var pd = db.PocoDataFactory.ForType(typeof(T));
            if (item.Id == 0)
            {
                long newId;
                lock (_cachedIds)
                {
                    if (!_cachedIds.TryGetValue(typeof(T), out newId))
                    {
                        var curId = db.ExecuteScalar<long?>($"select max(Id) from {pd.TableInfo.TableName}");
                        newId = curId ?? 0;
                    }
                    newId++;
                    _cachedIds[typeof(T)] = newId;
                }
                item.Id = newId;
                db.Insert(pd.TableInfo.TableName, null, false, item);
            }
            else
            {
                db.Update(pd.TableInfo.TableName, "Id", item);
            }
            return item;
        }

        public bool ExecuteWithinTransaction(Action<NPoco.Database> action)
        {
            bool result = false;
            using (var db = GetDatabase())
            {
                result = ExecuteWithinTransaction(db, action);
            }
            return result;
        }

        public bool ExecuteWithinTransaction(NPoco.Database db, Action<NPoco.Database> action)
        {
            bool result = false;
            db.BeginTransaction(System.Data.IsolationLevel.Serializable);
            try
            {
                action(db);
                db.CompleteTransaction();
                result = true;
            }
            catch (Exception e)
            {
                try
                {
                    db.AbortTransaction();
                }
                catch
                {
                }
            }
            return result;
        }

        public T GetPage<T, T2>(int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql)
            where T : new()
        {
            var result = new T();
            using (var db = GetDatabase())
            {
                if (!string.IsNullOrWhiteSpace(sortOn))
                {
                    var sort = sortAsc ? "Asc" : "Desc";
                    sql = sql.OrderBy($"{sortOn} {sort}");
                }
                else if (!string.IsNullOrWhiteSpace(defaultSort))
                {
                    sql = sql.OrderBy($"{defaultSort} asc");
                }
                var items = db.Page<T2>(page, pageSize, sql);
                typeof(T).GetProperty("Items").SetValue(result, items.Items, null);
                typeof(T).GetProperty("CurrentPage").SetValue(result, items.CurrentPage, null);
                typeof(T).GetProperty("PageCount").SetValue(result, items.TotalPages, null);
                typeof(T).GetProperty("TotalCount").SetValue(result, items.TotalItems, null);
            }
            return result;
        }

        public PagedListViewModel GetPage(int page, int pageSize, string sortOn, bool sortAsc, string defaultSort, NPoco.Sql sql)
        {
            var result = new PagedListViewModel();
            using (var db = GetDatabase())
            {
                if (!string.IsNullOrWhiteSpace(sortOn))
                {
                    var sort = sortAsc ? "Asc" : "Desc";
                    sql = sql.OrderBy($"{sortOn} {sort}");
                }
                else if (!string.IsNullOrWhiteSpace(defaultSort))
                {
                    sql = sql.OrderBy($"{defaultSort} asc");
                }
                var dbResult = db.Page<dynamic>(page, pageSize, sql);
                result.Items = dbResult.Items;
                result.CurrentPage = dbResult.CurrentPage;
                result.PageCount = dbResult.TotalPages;
                result.TotalCount = dbResult.TotalItems;
            }
            return result;
        }

        public class PragmaTableInfo
        {
            public string name { get; set; }
        }
        protected bool ColumnExists(NPoco.Database db, string tableName, string columnName)
        {
            bool result = false;
            try
            {
                var columns = db.Fetch<PragmaTableInfo>($"pragma table_info({tableName}");
                result = (from a in columns where a.name == columnName select a).Any();
            }
            catch
            {
            }
            return result;
        }

        protected bool TableExists(NPoco.Database db, string tableName)
        {
            bool result = false;
            try
            {
                object o = db.ExecuteScalar<string>(string.Format("SELECT name FROM sqlite_master WHERE type='table' AND name=@0", tableName));
                if (o == null || o.GetType() == typeof(DBNull))
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }
            catch
            {
            }
            return result;
        }

        //protected static void BindFunction(SqliteConnection connection, SqliteFunction function)
        //{
        //    var attributes = function.GetType().GetCustomAttributes(typeof(SQLiteFunctionAttribute), true).Cast<SQLiteFunctionAttribute>().ToArray();
        //    if (attributes.Length == 0)
        //    {
        //        throw new InvalidOperationException("SQLiteFunction doesn't have SQLiteFunctionAttribute");
        //    }
        //    connection.BindFunction(attributes[0], function);
        //}

    }
}
