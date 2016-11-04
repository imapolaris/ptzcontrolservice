using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer.Util
{
    public class RequestInfo
    {
        public string Method { get; set; }
        public string Params { get; set; }
        public string Id { get; set; }
        public string Version { get; set; }

        public string ToContent()
        {
            //return "{"+$"\"method\":{Method}, \"params\":{Params},\"id\":{Id},\"version\":{Version}"+"}";

            return "{"+
                "\"method\":" + "\"" +$"{Method}" + "\"," +
                "\"params\":[" + $"{Params}" + "]," +
                "\"id\":" + $"{Id}," +
                "\"version\":" + "\"" + $"{Version}" + "\"" +
                "}";

        }
    }
}
