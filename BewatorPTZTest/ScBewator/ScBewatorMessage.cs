using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BewatorPTZTest.ScBewator
{
    class ScBewatorMessage
    {
        public byte SynchByte;
        public int AddressId;
        public int Command;
        public byte[] Params;
    }
}
