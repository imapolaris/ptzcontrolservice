using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.Protocol.ScBewator
{
    class ScBewatorBuffer : Buffer
    {
        public ScBewatorBuffer()
        {
            StartPosition = new byte[] { 0xFF, 0xEE };
        }

        protected override Message newMessage()
        {
            return new ScBewatorMessage();
        }

        protected override int getLength()
        {
            int length = 7;
            if (_buffer.Count >= 7 && _buffer[0] == 0xEE)
                length = 9;
            return length;
        }

        protected override void loadMsg(Message msg, int length)
        {
            (msg as ScBewatorMessage).SynchByte = _buffer[0];
            msg.AddressId = _buffer[1];
            msg.Command = ((_buffer[2] << 8) + _buffer[3]) & 0xFFFF;
            msg.Params = _buffer.GetRange(4, length - 5).ToArray();
            //Console.WriteLine("Receive Buffer: {0}\t{1}", BitConverter.ToString(_buffer.Take(length).ToArray()), DateTime.Now.TimeOfDay);
        }
    }
}
