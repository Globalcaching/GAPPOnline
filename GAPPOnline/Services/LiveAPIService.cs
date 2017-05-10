using GAPPOnline.Models.Settings;
using GAPPOnline.Services.Database;
using GAPPOnline.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading.Tasks;
using System.Xml;

namespace GAPPOnline.Services
{
    public class LiveAPIService : BaseService
    {
        private static LiveAPIService _uniqueInstance = null;
        private static object _lockObject = new object();
        private static Groundspeak.LiveClient _client = null;

        private bool _testMode = false;

        private LiveAPIService()
        {
            _testMode = bool.Parse(Startup.Configuration["LiveAPI:TestMode"]);

        }

        public static LiveAPIService Instance
        {
            get
            {
                if (_uniqueInstance == null)
                {
                    lock (_lockObject)
                    {
                        if (_uniqueInstance == null)
                        {
                            _uniqueInstance = new LiveAPIService();
                        }
                    }
                }
                return _uniqueInstance;
            }
        }
     
        private Groundspeak.LiveClient Client
        {
            get
            {
                if (_client == null)
                {
                    lock (_lockObject)
                    {
                        if (_client == null)
                        {
                            BinaryMessageEncodingBindingElement binaryMessageEncoding = new BinaryMessageEncodingBindingElement()
                            {
                                ReaderQuotas = new XmlDictionaryReaderQuotas()
                                {
                                    MaxStringContentLength = int.MaxValue,
                                    MaxBytesPerRead = int.MaxValue,
                                    MaxDepth = int.MaxValue,
                                    MaxArrayLength = int.MaxValue
                                }
                            };

                            HttpTransportBindingElement httpTransport = new HttpsTransportBindingElement()
                            {
                                MaxBufferSize = int.MaxValue,
                                MaxReceivedMessageSize = int.MaxValue,
                                AllowCookies = false,
                            };

                            // add the binding elements into a Custom Binding
                            CustomBinding binding = new CustomBinding(binaryMessageEncoding, httpTransport);

                            EndpointAddress endPoint;
                            if (_testMode)
                            {
                                endPoint = new EndpointAddress("https://staging.api.groundspeak.com/Live/V6Beta/geocaching.svc/Silverlightsoap");
                            }
                            else
                            {
                                endPoint  = new EndpointAddress("https://api.groundspeak.com/LiveV6/Geocaching.svc/Silverlightsoap");
                            }

                            _client = new Groundspeak.LiveClient(binding, endPoint);
                        }
                    }
                }
                return _client;
            }
        }

        public async Task<LiveAPIResultViewModel> GetYourUserProfile(Models.Settings.User user, string token)
        {
            var result = new LiveAPIResultViewModel();
            var req = new Groundspeak.GetYourUserProfileRequest()
            {
                AccessToken = token,
                DeviceInfo = new Groundspeak.DeviceData()
                {
                    DeviceName = "GlobalcachingApplication",
                    DeviceUniqueId = "internal",
                    ApplicationSoftwareVersion = "V1.0.0.0"
                }
            };
            req.AccessToken = token;
            var res = await Client.GetYourUserProfileAsync(req);
            result.LiveAPIResult = res;
            if (res?.Status?.StatusCode != 0)
            {
                NotificationService.Instance.AddErrorMessage(_T("Live API call error:") + " GetYourUserProfile: " + res?.Status?.StatusMessage ?? "");
            }
            else
            {
                NotificationService.Instance.AddSuccessMessage(_T("Live API call success:") + " GetYourUserProfile");
                SettingsDatabaseService.Instance.ExecuteWithinTransaction((db) =>
                {
                    var gcComUser = db.FirstOrDefault<GCComUser>("where UserId=@0 and MemberId=@1", user.Id, res.Profile.User.Id);
                    if (gcComUser == null)
                    {
                        gcComUser = new GCComUser();
                        gcComUser.UserId = user.Id;
                        gcComUser.MemberId = res.Profile.User.Id ?? 0;
                    }
                    gcComUser.AvatarUrl = res.Profile.User.AvatarUrl;
                    gcComUser.GCComName = res.Profile.User.UserName;
                    gcComUser.MemberTypeId = res.Profile.User.MemberType.MemberTypeId;
                    gcComUser.PublicGuid = res.Profile.User.PublicGuid.ToString();
                    gcComUser.Token = token;
                    gcComUser.TokenFromDate = DateTime.UtcNow;
                    db.Save(gcComUser);
                });
            }
            return result;
        }

    }
}