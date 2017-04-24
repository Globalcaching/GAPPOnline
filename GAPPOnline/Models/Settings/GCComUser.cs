using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("GCComAccount")]
    public class GCComUser: AutoIdModel
    {
        public long UserId { get; set; }

        public long MemberId { get; set; }
        public string AvatarUrl { get; set; }
        public long MemberTypeId { get; set; }
        public Guid PublicGuid { get; set; }
        public string GCComName { get; set; }
        public string Token { get; set; }
        public DateTime TokenFromDate { get; set; }
    }
}
