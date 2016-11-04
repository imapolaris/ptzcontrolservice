using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BewatorPTZTest
{
    public class Prompt
    {
        public static readonly Prompt Instance = new Prompt();
        public Action<string> SendEvent { get; set; }
        public Action<string> ReceivedEvent { get; set; }
        public Action<string> StatusEvent { get; set; }

        public void OnSend(object obj)
        {
            Console.WriteLine(obj);
            var handle = SendEvent;
            if (handle != null)
                handle(obj.ToString());
        }

        public void OnReceived(object obj)
        {
            //Console.WriteLine(obj);
            var handle = ReceivedEvent;
            if (handle != null)
                handle(obj.ToString());
        }

        public void OnStatus(object obj)
        {
            obj = $"{DateTime.Now.TimeOfDay} {obj}";
            Console.WriteLine(obj);
            var handle = StatusEvent;
            if (handle != null)
                handle(obj.ToString()); 
        }
    }
}
