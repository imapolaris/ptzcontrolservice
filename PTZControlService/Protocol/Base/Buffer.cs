using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTZControlService.Protocol
{
    class Message
    {
        public int AddressId;
        public int Command;
        public byte[] Params;
    }

    abstract class Buffer
    {
        protected List<byte> _buffer = new List<byte>();
        protected byte[] StartPosition;
        public void Add(IEnumerable<byte> collection)
        {
            _buffer.AddRange(collection);
        }

        public virtual bool ParseMessage(out Message msg)
        {
            msg = newMessage();
            removeInvalidStart();
            int length = getLength();
            if (length > 0 && _buffer.Count >= length)
            {
                loadMsg(msg, length);
                return checkCodeAndRemoved(length);//校验和验证并移除该段消息
            }
            return false;
        }

        protected abstract int getLength();

        protected abstract void loadMsg(Message msg, int length);

        protected void removeInvalidStart()
        {
            int count = 0;
            while (_buffer.Count > 0 && !StartPosition.Any(_=>_ == _buffer[0]))
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

        protected virtual Message newMessage()
        {
            return new Message();
        }
    }
}
