using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.Protocol.Bewator
{
    class BewatorBuffer : Buffer
    {
        public BewatorBuffer()
        {
            StartPosition = new byte[] { 0xA8 };
        }

        protected override int getLength()
        {
            int length = 7;
            if (_buffer.Count >= length)
            {
                if (_buffer[2] == 3 && (_buffer[3] & 0x01) == 1)
                    length = 9;
            }
            return length;
        }

        protected override void loadMsg(Message msg, int length)
        {
            msg.AddressId = _buffer[1];
            msg.Command = ((_buffer[2] << 8) + _buffer[3]) & 0xFFFF;
            msg.Params = _buffer.GetRange(4, length - 5).ToArray();
            //Console.WriteLine($"Recv {BitConverter.ToString(_buffer.Take(length).ToArray())}");
        }
    }
}
