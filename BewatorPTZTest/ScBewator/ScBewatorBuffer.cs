using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BewatorPTZTest.ScBewator
{
    class ScBewatorBuffer
    {
        protected List<byte> _buffer = new List<byte>();
        protected byte[] StartPosition;
        public ScBewatorBuffer()
        {
            StartPosition = new byte[] { 0xFF, 0xEE };
        }

        public void Add(IEnumerable<byte> collection)
        {
            _buffer.AddRange(collection);
        }

        public virtual bool ParseMessage(out ScBewatorMessage msg)
        {
            msg = new ScBewatorMessage(); 
            removeInvalidStart();
            int length = getLength();
            if (length > 0 && _buffer.Count >= length)
            {
                loadMsg(msg, length);
                return checkCodeAndRemoved(length);//校验和验证并移除该段消息
            }
            return false;
        }
        
        protected void removeInvalidStart()
        {
            int count = 0;
            while (_buffer.Count > 0 && !StartPosition.Any(_ => _ == _buffer[0]))
            {
                _buffer.RemoveAt(0);
                count++;
            }
        }

        /// <summary>校验和验证并移除该段消息</summary>
        /// <param name="codeLength">总长度</param>
        /// <returns>验证是否通过</returns>
        protected bool checkCodeAndRemoved(int codeLength)
        {
            byte realChecksum = 0;
            for (int i = 1; i < codeLength - 1; i++)
                realChecksum += _buffer[i];
            byte msgChecksum = _buffer[codeLength - 1];
            _buffer.RemoveRange(0, codeLength);
            //if(realChecksum != msgChecksum)
            //    Console.WriteLine("drop {0} bytes.", codeLength);
            return realChecksum == msgChecksum;
        }

        protected ScBewatorMessage newMessage()
        {
            return new ScBewatorMessage();
        }

        protected int getLength()
        {
            int length = 7;
            if (_buffer.Count >= 7 && _buffer[0] == 0xEE)
                length = 9;
            return length;
        }

        protected void loadMsg(ScBewatorMessage msg, int length)
        {
            (msg as ScBewatorMessage).SynchByte = _buffer[0];
            msg.AddressId = _buffer[1];
            msg.Command = ((_buffer[2] << 8) + _buffer[3]) & 0xFFFF;
            msg.Params = _buffer.GetRange(4, length - 5).ToArray();
             Prompt.Instance.OnReceived(string.Format("Receive {0}\t{1}", BitConverter.ToString(_buffer.Take(length).ToArray()), DateTime.Now.TimeOfDay));
            //Console.WriteLine("Receive Buffer: {0}\t{1}", BitConverter.ToString(_buffer.Take(length).ToArray()), DateTime.Now.TimeOfDay);
        }
    }
}
