using GAPPOnline.Models;
using GAPPOnline.Models.Localization;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace GAPPOnline.Services
{
    public class LocalizationService : BaseService
    {
        private static LocalizationService _uniqueInstance = null;
        private static object _lockObject = new object();

        public class TranslationCache
        {
            public TranslationCache()
            {
                LocalizationOriginalText = new Dictionary<string, long>();
                LocalizationTranslations = new Dictionary<long, Dictionary<long, string>>();
            }

            public Dictionary<string, long> LocalizationOriginalText { get; set; }
            public Dictionary<long, Dictionary<long, string>> LocalizationTranslations { get; set; }

            public void Clear()
            {
                LocalizationOriginalText.Clear();
                LocalizationTranslations.Clear();
            }
        }

        private TranslationCache _cache = null;

        private LocalizationDatabaseService _databaseService;

        private LocalizationService()
        {
            _databaseService = LocalizationDatabaseService.Instance;
            _cache = new TranslationCache();
            InitializeCache();
        }

        public static LocalizationService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new LocalizationService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }

        private void InitializeCache()
        {
            lock (_cache)
            {
                _cache.Clear();
                using (var db = _databaseService.GetDatabase())
                {
                    var lc = db.FirstOrDefault<LocalizationCulture>("where Name = 'en-US' COLLATE NOCASE");
                    if (lc == null)
                    {
                        lc = new LocalizationCulture();
                        lc.Name = "en-US";
                        lc.Description = "en-US";
                        _databaseService.Save(db, lc);

                    }
                    _cache.LocalizationOriginalText = db.Fetch<LocalizationOriginalText>().ToDictionary(i => i.OriginalText, i => i.Id);
                    var cultures = db.Fetch<LocalizationCulture>();
                    foreach (var c in cultures)
                    {
                        _cache.LocalizationTranslations.Add(c.Id, db.Fetch<LocalizationTranslation>("where LocalizationCultureId = @0", c.Id).ToDictionary(i => i.LocalizationOriginalTextId, i => i.TranslatedText));
                    }
                }
            }
        }

        public void Initialize()
        {
            CultureInfo ci = CurrentCultureInfo;
            if (ci == null)
            {
                ci = CultureInfo.CurrentUICulture; //Thread.CurrentThread.CurrentUICulture;
                CurrentCultureInfo = ci;
                CurrentCulture = GetLocalizationCulture(ci.Name);
            }

            CultureInfo.CurrentCulture = ci;
            CultureInfo.CurrentUICulture = ci;
            //Thread.CurrentThread.CurrentCulture = ci;
            //Thread.CurrentThread.CurrentUICulture = ci;
        }

        //[IHttpContextAccessor contextAccessor]
        public LocalizationCulture CurrentCulture
        {
            get
            {
                var result = System.Web.HttpContext.Current.Session.GetObjectFromJson<LocalizationCulture>("LocalizationService.CurrentCulture");
                //var result = contextAccessor.HttpContext.Session["LocalizationService.CurrentCulture"] as LocalizationCulture;
                return result;
            }
            set
            {
                System.Web.HttpContext.Current.Session.SetObjectAsJson("LocalizationService.CurrentCulture", value);
                //HttpContext.Current.Session["LocalizationService.CurrentCulture"] = value;
            }
        }

        public CultureInfo CurrentCultureInfo
        {
            get
            {
                var name = System.Web.HttpContext.Current.Session.GetObjectFromJson<string>("LocalizationService.CurrentCultureInfo");
                if(!string.IsNullOrEmpty(name))
                    return new System.Globalization.CultureInfo(name);
                else
                    return null;
            }
            set
            {
                System.Web.HttpContext.Current.Session.SetObjectAsJson("LocalizationService.CurrentCultureInfo", value?.Name);
            }
        }

        public LocalizationCulture GetLocalizationCulture(string culture)
        {
            LocalizationCulture result = null;
            lock (this)
            {
                using (var db = _databaseService.GetDatabase())
                {
                    result = db.FirstOrDefault<LocalizationCulture>("where Name = @0 COLLATE NOCASE", culture);
                }
            }
            return result;
        }

        public string Translate(string text)
        {
            var result = text;
            if (CurrentCulture != null)
            {
                lock (_cache)
                {
                    Dictionary<long, string> table;
                    if (_cache.LocalizationTranslations.TryGetValue(CurrentCulture.Id, out table))
                    {
                        string translatedText = null;

                        long originalTextId = 0;
                        if (_cache.LocalizationOriginalText.TryGetValue(text, out originalTextId))
                        {
                            table.TryGetValue(originalTextId, out translatedText);
                        }
                        else
                        {
                            using (var db = _databaseService.GetDatabase())
                            {
                                var m = new LocalizationOriginalText();
                                m.OriginalText = text;
                                _databaseService.Save(db, m);
                                _cache.LocalizationOriginalText.Add(text, m.Id);
                            }
                        }

                        result = string.IsNullOrEmpty(translatedText) ? text : translatedText;
                    }
                }
            }
            return result;
        }

        public string this[string key]
        {
            get
            {
                return Translate(key);
            }
        }

        public void SaveLocalizationCulture(LocalizationCulture m)
        {
            _databaseService.ExecuteWithinTransaction((db) =>
            {
                _databaseService.Save(db, m);
                lock (_cache)
                {
                    Dictionary<long, string> table;
                    if (!_cache.LocalizationTranslations.TryGetValue(m.Id, out table))
                    {
                        _cache.LocalizationTranslations.Add(m.Id, new Dictionary<long, string>());
                    }
                }
            });
        }

        public void DeleteLocalizationCulture(LocalizationCulture m)
        {
            _databaseService.ExecuteWithinTransaction((db) =>
            {
                lock (_cache)
                {
                    Dictionary<long, string> table;
                    if (_cache.LocalizationTranslations.TryGetValue(m.Id, out table))
                    {
                        db.Execute("delete from LocalizationTranslation where LocalizationCultureId=@0", m.Id);
                        db.Delete(m);
                        _cache.LocalizationTranslations.Remove(m.Id);
                    }
                }
            });
        }

        public void SaveLocalizationTranslation(LocalizationTranslationViewModelItem item)
        {
            using (var db = _databaseService.GetDatabase())
            {
                lock (_cache)
                {
                    Dictionary<long, string> table;
                    if (_cache.LocalizationTranslations.TryGetValue(item.CultureId, out table) && db.FirstOrDefault<LocalizationOriginalText>("where Id=@0", item.OriginalTextId) !=null)
                    {
                        var m = db.FirstOrDefault<LocalizationTranslation>("where LocalizationCultureId=@0 and LocalizationOriginalTextId=@1", item.CultureId, item.OriginalTextId);
                        if (m == null)
                        {
                            m = new LocalizationTranslation();
                            m.LocalizationCultureId = item.CultureId;
                            m.LocalizationOriginalTextId = item.OriginalTextId;
                        }
                        table[item.OriginalTextId] = item.TranslatedText;
                        m.TranslatedText = item.TranslatedText;
                        _databaseService.Save(db, m);
                    }
                }
            }
        }

        public LocalizationTranslationViewModel GetLocalizationTranslations(int page, int pageSize, long cultureId, string filterOrg = "", string filterTrans = "", string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("LocalizationOriginalText.OriginalText")
                .Append(", TranslatedText = LocalizationTranslation.TranslatedText")
                .Append($", CultureId = {cultureId}")
                .Append(", OriginalTextId = LocalizationOriginalText.Id")
                .Append(", LocalizationCultureId = LocalizationCulture.Id")
                .Append(", LocalizationCultureName = LocalizationCulture.Name")
                .Append(", CanDelete = 0")
                .Append(", CanEdit = 1")
                .Append(", CanClone = 0")
                .Append(", CanSelect = 1")
                .From("LocalizationOriginalText")
                .LeftJoin("LocalizationTranslation").On("LocalizationOriginalText.Id = LocalizationTranslation.LocalizationOriginalTextId and LocalizationTranslation.LocalizationCultureId=@0", cultureId)
                .LeftJoin("LocalizationCulture").On("LocalizationTranslation.LocalizationCultureId = LocalizationCulture.Id")
                .Where("1=1");
            if (!string.IsNullOrEmpty(filterOrg))
            {
                sql = sql.Append("and LocalizationOriginalText.OriginalText like @0", string.Format("%{0}%", filterOrg));
            }
            if (!string.IsNullOrEmpty(filterTrans))
            {
                sql = sql.Append("and LocalizationTranslation.TranslatedText like @0", string.Format("%{0}%", filterTrans));
            }
            if (!string.IsNullOrEmpty(sortOn))
            {
                sortOn = string.Format("CAST({0} as NVarchar(1000))", sortOn);
            }
            var result = _databaseService.GetPage<LocalizationTranslationViewModel, LocalizationTranslationViewModelItem>(page, pageSize, sortOn, sortAsc, "CAST(OriginalText as NVarchar(1000))", sql);
            return result;
        }

        public LocalizationCultureViewModel GetLocalizationCultures(int page, int pageSize, string sortOn = "", bool sortAsc = true)
        {
            var sql = NPoco.Sql.Builder.Select("LocalizationCulture.*")
                .Append(", CanDelete = 1")
                .Append(", CanEdit = 1")
                .Append(", CanClone = 0")
                .Append(", CanSelect = 1")
                .From("LocalizationCulture");
            return _databaseService.GetPage<LocalizationCultureViewModel, LocalizationCultureViewModelItem>(page, pageSize, sortOn, sortAsc, "Name", sql);
        }

    }
}