using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public class GSAKDatabaseService
    {
        private static GSAKDatabaseService _uniqueInstance = null;
        private static object _lockObject = new object();

        private GSAKDatabaseService()
        {
        }

        public static GSAKDatabaseService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new GSAKDatabaseService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        public GSAKDatabaseViewModel GetGSAKDatabases(int page, int pageSize, long? userId = null, string filterName = "", string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("GSAKDatabase.*")
                .Append(", User.Name as UserName")
                .From("GSAKDatabase")
                .InnerJoin("User").On("GSAKDatabase.UserId=User.Id")
                .Where("1=1");
            if (userId != null && userId > 0)
            {
                sql = sql.Append("and User.Id=@0", userId);
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                sql = sql.Append("and GSAKDatabase.Name like @0", $"%{filterName}%");
            }
            return SettingsDatabaseService.Instance.GetPage<GSAKDatabaseViewModel, GSAKDatabaseViewModelItem>(page, pageSize, sortOn, sortAsc, "GSAKDatabase.Name", sql);
        }

    }
}
