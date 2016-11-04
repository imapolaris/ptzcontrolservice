using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class CanNotControlExpection: Exception
    {
        public CanNotControlExpection(string message)
            :base(message)
        {
        }

        public CanNotControlExpection()
            :this("云镜控制失败")
        {
        }
    }
}
