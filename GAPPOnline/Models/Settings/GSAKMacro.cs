using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GAPPOnline.Models.Settings
{
    [NPoco.TableName("GSAKMacro")]
    [NPoco.PrimaryKey("Id")]
    public class GSAKMacro
    {
        public long Id { get; set; }
        public long UserId { get; set; }

        public string FileName { get; set; }
        public string Description { get; set; }
        public DateTime FileDate { get; set; }
        public string Version { get; set; }
        public DateTime? LastRun { get; set; }
        public int RunCount { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string UserData { get; set; }
    }
}
