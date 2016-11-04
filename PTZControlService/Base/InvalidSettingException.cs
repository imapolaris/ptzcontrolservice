using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService
{
    public class InvalidSettingException: Exception
    {
        public InvalidSettingException()
            : base("无效配置参数")
        { }

        public InvalidSettingException(string message)
            :base(message)
        {
        }
    }
}
