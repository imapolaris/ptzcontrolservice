using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.Protocol.ICU03
{
    class ICU03Buffer : Buffer
    {
        public ICU03Buffer()
        {
            StartPosition = new byte[] { 0xFA };
        }

        protected override Message newMessage()
        {
            return new ICU03Message();
        }

        protected override int getLength()
        {
            int length = 0;
            if (_buffer.Count >= 6)
            {
                if (_buffer[1] >= 6)
                    length = _buffer[1];
                else
                    _buffer.RemoveAt(0);
            }
            return length;
        }

        protected override void loadMsg(Message msg, int length)
        {
            msg.AddressId = _buffer[2];
            (msg as ICU03Message).Destination = _buffer[3];
            msg.Command = _buffer[4];
            int paramLen = length - 6;
            msg.Params = _buffer.GetRange(5, paramLen).ToArray();
        }
    }
}
