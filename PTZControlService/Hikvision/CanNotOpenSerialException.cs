using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PTZControlService.Hikvision
{
    public class CanNotOpenSerialException : Exception
    {
        public CanNotOpenSerialException()
            : base()
        {
        }

        public CanNotOpenSerialException(string message)
            : base(message)
        {
        }
    }
}
