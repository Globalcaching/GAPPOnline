using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("GSAKDatabase")]
    [NPoco.PrimaryKey("Id")]
    public class GSAKDatabase
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
