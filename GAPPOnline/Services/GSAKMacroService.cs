using GAPPOnline.Hubs;
using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GAPPOnline.Services
{
    public partial class GSAKMacroService
    {
        private static GSAKMacroService _uniqueInstance = null;
        private static object _lockObject = new object();
        private Dictionary<string, Macro> _runningMacros;

        private GSAKMacroService()
        {
            _runningMacros = new Dictionary<string, Macro>();
        }

        public static GSAKMacroService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new GSAKMacroService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        protected string GetGSAKMacroFolder(long userId, bool createIfNotExists = false)
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
            result = Path.Combine(result, "GSAKMacros");
            if (createIfNotExists && !Directory.Exists(result))
            {
                Directory.CreateDirectory(result);
            }
            return result;
        }

        public GSAKMacroViewModel GetGSAKMacros(Models.Settings.User user, int page, int pageSize, long? userId = null, string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("GSAKMacro.*")
                .Append(", User.Name as UserName")
                .Append(", 1 as CanDelete")
                .Append(", 1 as CanEdit")
                .Append(", 1 as CanClone")
                .From("GSAKMacro")
                .InnerJoin("User").On("GSAKMacro.UserId=User.Id")
                .Where("1=1");
            if (userId != null && userId > 0)
            {
                sql = sql.Append("and User.Id=@0", userId);
            }
            return SettingsDatabaseService.Instance.GetPage<GSAKMacroViewModel, GSAKMacroViewModelItem>(page, pageSize, sortOn, sortAsc, "FileName", sql);
        }

        public void InstallMacro(Models.Settings.User user, string macroFile, string originalFileName)
        {
            SettingsDatabaseService.Instance.ExecuteWithinTransaction((db) =>
            {
                var m = db.FirstOrDefault<GSAKMacro>("where UserId=@0 and FileName=@1 collate nocase", user.Id, originalFileName);
                if (m == null)
                {
                    m = new GSAKMacro();
                    m.UserId = user.Id;
                    m.FileName = originalFileName;
                    m.UserData = "";
                    m.Version = "";
                    m.Url = "";
                    m.Author = "";
                    m.Description = "";
                }
                m.FileDate = DateTime.UtcNow;
                var allLines = File.ReadAllLines(macroFile);
                foreach (var line in allLines)
                {
                    var tl = line.Trim();
                    if (tl.StartsWith("#") && tl.Length>1)
                    {
                        var parts = tl.Substring(1).Split(new char[] { '=' }, 2);
                        if (parts.Length > 1)
                        {
                            switch (parts[0].Trim().ToLower())
                            {
                                case "macversion":
                                    m.Version = parts[1].Trim();
                                    break;
                                case "macdescription":
                                    m.Description = parts[1].Trim();
                                    break;
                                case "macauthor":
                                    m.Author = parts[1].Trim();
                                    break;
                                case "macurl":
                                    m.Url = parts[1].Trim();
                                    break;
                            }
                        }
                    }
                }
                var tf = Path.Combine(GetGSAKMacroFolder(user.Id, true), originalFileName);
                if (File.Exists(tf))
                {
                    File.Delete(tf);
                }
                File.Copy(macroFile, tf);
                db.Save(m);
                DataChangedHub.ReportChangeToClients(user, "GSAKMacro");
            });
        }

        public void RunMacro(string pconnectionId, string puserGuid, string pfilename)
        {
            string connectionId = pconnectionId;
            string userGuid = puserGuid;
            string filename = pfilename;
            Task.Factory.StartNew(() =>
            {
                var usr = AccountService.Instance.GetUserByUserGuid(userGuid);
                if (usr != null)
                {
                    var macro = new Macro(usr, filename);
                    Macro currentMacro;
                    lock (_runningMacros)
                    {
                        _runningMacros.TryGetValue(connectionId, out currentMacro);
                    }
                    if (currentMacro != null)
                    {
                        currentMacro.Stop();
                        _runningMacros.Remove(connectionId);
                    }
                    lock (_runningMacros)
                    {
                        _runningMacros.Add(connectionId, macro);
                    }
                    var conId = connectionId;
                    GSAKMacroHub.MacroIsStarted(conId);
                    try
                    {
                        macro.Run(conId, null, 0);
                    }
                    catch (Exception e)
                    {
                        NotificationService.Instance.AddErrorMessage(e.Message);
                    }
                    lock (_runningMacros)
                    {
                        _runningMacros.Remove(connectionId);
                    }
                    macro.Dispose();
                    GSAKMacroHub.MacroIsFinished(conId);
                }
            });
        }

        public void MsgOKResult(string connectionId)
        {

        }
    }
}
