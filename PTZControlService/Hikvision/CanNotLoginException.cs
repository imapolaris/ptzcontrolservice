using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTZControlService.Hikvision
{
    public class CanNotLoginException : Exception
    {
        public CanNotLoginException()
            : base()
        {
        }

        public CanNotLoginException(string message)
            : base(message)
        {
        }
    }
}
