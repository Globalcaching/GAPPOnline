using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("UserSessionInfo")]
    [NPoco.PrimaryKey("Id")]
    public class UserSessionInfo
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        [NPoco.Ignore]
        public string UserGuid { get; set; }

        public long? SelectedGSAKDatabaseId { get; set; }
        public string ActiveGCCode { get; set; }
    }
}
