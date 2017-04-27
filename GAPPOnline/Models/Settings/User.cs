using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("User")]
    [NPoco.PrimaryKey("Id")]
    public class User
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string PreferredLanguage { get; set; }
        public long MemberTypeId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
