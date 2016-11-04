using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.Base
{
    public class SnapshotHistoryInfo
    {
        public List<SnapshotInfo> History = new List<SnapshotInfo>();
        public List<TargetMux> Mux = new List<TargetMux>();

        public bool AddMux(string id1, string id2, DateTime time)
        {
            if(id1 != id2 && !Mux.Any(_=>(_.Id1== id1 && _.Id2 == id2)|| (_.Id1 == id2 && _.Id2 == id1)))
            {
                Mux.Add(new TargetMux() { Id1 = id1, Id2 = id2, MuxTime = time });
                return true;
            }
            return false;
        }
    }
}
