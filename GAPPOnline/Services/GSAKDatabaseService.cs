using GAPPOnline.Hubs;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public class GSAKDatabaseService: BaseDatabaseService
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

        public GSAKDatabaseViewModel GetGSAKDatabases(Models.Settings.User user, int page, int pageSize, long? userId = null, string filterName = "", string filterUserName = "", string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("GSAKDatabase.*")
                .Append(", User.Name as UserName")
                .Append(", 1 as CanDelete")
                .Append(", 1 as CanEdit")
                .Append(", 1 as CanClone");
            if (user.SessionInfo?.SelectedGSAKDatabaseId == null)
            {
                sql = sql.Append(", 1 as CanSelect");
            }
            else
            {
                sql = sql.Append($", (GSAKDatabase.Id <> {user.SessionInfo.SelectedGSAKDatabaseId}) as CanSelect");
            }
                sql = sql.From("GSAKDatabase")
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
            if (!string.IsNullOrEmpty(filterUserName))
            {
                sql = sql.Append("and User.Name like @0", $"%{filterUserName}%");
            }
            return SettingsDatabaseService.Instance.GetPage<GSAKDatabaseViewModel, GSAKDatabaseViewModelItem>(page, pageSize, sortOn, sortAsc, "GSAKDatabase.Name", sql);
        }

        public void SelectGSAKDatabase(Models.Settings.User user, Models.Settings.GSAKDatabase item)
        {
            SettingsDatabaseService.Instance.Execute((db) =>
            {
                var gsakDb = db.FirstOrDefault<Models.Settings.GSAKDatabase>("where Id=@0 and UserId=@1", item.Id, user.Id);
                if (user.SessionInfo.SelectedGSAKDatabaseId != gsakDb?.Id)
                {
                    user.SessionInfo.SelectedGSAKDatabaseId = gsakDb?.Id;
                    user.SessionInfo.ActiveGCCode = null;
                    db.Save(user.SessionInfo);
                }
            });
            DataChangedHub.SendSessionInfoToClient(user);
        }

        public void SaveGSAKDatabase(Models.Settings.User user, Models.Settings.GSAKDatabase item)
        {
            SettingsDatabaseService.Instance.ExecuteWithinTransaction((db) =>
            {
                if (item.Id == 0)
                {
                    item.CreatedAt = DateTime.UtcNow;
                    item.UserId = user.Id;
                    db.Save(item);
                    GetGSAKDatabaseFile(user.Id, item.Id, true);
                    NotificationService.Instance.AddSuccessMessage(_T("GSAK database has been created."));
                }
                else
                {
                    var org = db.FirstOrDefault<Models.Settings.GSAKDatabase>("where Id=@0", item.Id);
                    org.Name = item.Name;
                    org.Description = item.Description;
                    db.Save(org);
                    NotificationService.Instance.AddSuccessMessage(_T("Changes have been saved."));
                }
            });
            DataChangedHub.SendDataChangedToClient(user, "GSAKDatabase");
        }

        public bool CheckGSAKDatabaseName(Models.Settings.User user, Models.Settings.GSAKDatabase item)
        {
            bool result = false;
            SettingsDatabaseService.Instance.Execute((db) =>
            {
                result = db.FirstOrDefault<Models.Settings.GSAKDatabase>("where Id<>@0 and UserId=@1 and Name like @2", item.Id, user.Id, item.Name) == null;
            });
            return result;
        }

        private string GetGSAKDatabaseFolder(long userId, long gsakDatabaseId, bool createIfNotExists = false)
        {
            var result = Path.Combine(Startup.HostingEnvironment.ContentRootPath, "App_Data");
            if (createIfNotExists && !Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            result = Path.Combine(result, $"User_{userId}");
            if (createIfNotExists && !Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            result = Path.Combine(result, $"DB_{gsakDatabaseId}");
            if (createIfNotExists && !Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        private string GetGSAKDatabaseFile(long userId, long gsakDatabaseId, bool createIfNotExists = false)
        {
            var result = GetGSAKDatabaseFolder(userId, gsakDatabaseId, createIfNotExists);
            result = Path.Combine(result, "sqlite.db3");
            if (createIfNotExists && !File.Exists(result))
            {
                File.Copy(Path.Combine(Startup.HostingEnvironment.WebRootPath, "files", "sqlite.db3"), result);
            }
            return result;
        }

        private string GetGSAKDatabaseFile(Models.Settings.User user)
        {
            return GetGSAKDatabaseFile(user.Id, (long)user.SessionInfo.SelectedGSAKDatabaseId);
        }

        public GSAKGeocacheViewModel GetGSAKGeocaches(Models.Settings.User user, int page, int pageSize
            , string filterCode = ""
            , string filterName = ""
            , string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("Caches.*")
                .Append(", 1 as CanDelete")
                .Append(", 1 as CanEdit")
                .Append(", 0 as CanClone");
            if (user.SessionInfo?.ActiveGCCode == null)
            {
                sql = sql.Append(", 1 as CanSelect");
            }
            else
            {
                sql = sql.Append($", (Caches.Code <> {user.SessionInfo.ActiveGCCode}) as CanSelect");
            }
            sql = sql.From("Caches")
            .Where("1=1");
            if (!string.IsNullOrEmpty(filterCode))
            {
                sql = sql.Append("and Caches.Code like @0", $"%{filterCode}%");
            }
            if (!string.IsNullOrEmpty(filterName))
            {
                sql = sql.Append("and Caches.Name like @0", $"%{filterName}%");
            }
            var result = GSAKDatabaseInstance.GetPage<GSAKGeocacheViewModel, GSAKGeocacheViewModelItem>(GetGSAKDatabaseFile(user), page, pageSize, sortOn, sortAsc, "Code", sql);
            foreach (var item in result.Items)
            {
                if (!string.IsNullOrEmpty(item.CacheType))
                {
                    item.GCComCacheType = GetCacheType(item.CacheType[0]);
                }
                if (!string.IsNullOrEmpty(item.Container))
                {
                    item.GCComContainer = GetContainer(item.Container[0]);
                }
            }
            return result;
        }


        private int GetCacheType(char type)
        {
            switch (type)
            {
                case 'T': return 2;
                case 'M': return 3;
                case 'V': return 4;
                case 'B': return 5;
                case 'E': return 6;
                case 'U': return 8;
                case 'A': return 9;
                case 'W': return 11;
                case 'L': return 12;
                case 'C': return 13;
                case 'R': return 137;
                case 'Z': return 453;
                case 'X': return 1304;
                case 'I': return 1858;
                case 'F': return 3653;
                case 'H': return 3773;
                case 'D': return 3774;
                default: return 0;
            }
        }

        private int GetContainer(char container)
        {
            switch (container)
            {
                case 'N': return 1;
                case 'M': return 2;
                case 'R': return 3;
                case 'L': return 4;
                case 'V': return 5;
                case 'O': return 6;
                case 'S': return 8;
                default: return 6;
            }
        }

    }
}
