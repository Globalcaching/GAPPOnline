using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("GSAKDatabase")]
    public class GSAKDatabase: AutoIdModel
    {
        public long UserId { get; set; }

        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
