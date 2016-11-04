using PTZControlService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.Base
{
    public class SnapshotInfo
    {
        public DynamicTarget Ship { get; set; }
        public bool IsSucceeded { get; set; }
        public string FileName { get; set; }
        public DateTime SnapshotTime { get; set; }
        public string Prompt { get; set; }
    }
}
