using Newtonsoft.Json;
using PTZControlService;
using SeeCool.GISFramework.AmqUtil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CCTVSnapshotServer
{
    public class ActiveMQDataProvider: IDisposable
    {
        private string _uri = string.Empty;
        private string _topic = string.Empty;
        private string _ruleType = string.Empty;
        private AmqConsumer _amqConsumerRecv = null;
        public Action<DynamicTarget> DynamicEvent;
        
        public ActiveMQDataProvider(string uri = "tcp://127.0.0.1:61616", string topic = "RITSV2.Event", string ruleType = "Enter_Region")
        {
            _uri = uri;
            _topic = topic;
            _ruleType = ruleType;
            startup();
        }

        void startup()
        {
            _amqConsumerRecv = new AmqConsumer();
            _amqConsumerRecv.OnTextMessage += onTextMessage;
            _amqConsumerRecv.InitTopic(_uri, _topic);
        }

        void shutdown()
        {
            if (_amqConsumerRecv != null)
            {
                _amqConsumerRecv.OnTextMessage -= onTextMessage;
                _amqConsumerRecv = null;
            }
        }
        void onTextMessage(string message)
        {
            try
            {
                EventData data = JsonConvert.DeserializeObject<EventData>(message);
                if (data.RuleType == _ruleType)
                {
                    onDynamicTarget(data.Ship.Track, data.CrossDirection);
                    Console.WriteLine("Id: " + data.Ship.Track.Id);
                }
            }
            catch (Exception err)
            {
                Console.WriteLine(err);
            }
        }

        private void onDynamicTarget(TrackData track, int crossDirection)
        {
            string id = track.Id;
            if (track.Geometry != null && track.Geometry.Substring(0,7) == "POINT (")
            {
                char[] separator = new char[] { ' ', ')' };
                var geoArray = track.Geometry.Substring(7).Split(separator, StringSplitOptions.RemoveEmptyEntries);
                double lon = double.Parse(geoArray[0]);
                double lat = double.Parse(geoArray[1]);
                onDynamic(new DynamicTarget(track.Id, track.MMSI, track.Name, lon, lat, track.Sog, track.Cog, track.Heading,
                    track.Length, track.Width, track.MeasureA,track.MeasureC, track.Time, crossDirection));
            }
        }

        private void onDynamic(DynamicTarget target)
        {
            Console.WriteLine($"Id: {target.Id}\t Time: {target.Time}  Length {target.Length}");
            var handler = DynamicEvent;
            if (handler != null)
                handler(target);
        }

        public void Dispose()
        {
            shutdown();
        }
    }
}
