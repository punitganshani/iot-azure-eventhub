using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace shared
{
    public class Constants
    {
        //const string ServiceBusConnectionStringFormat = @"Endpoint=sb://{0}.servicebus.windows.net/;SharedAccessKeyName={1};SharedAccessKey={2}";
        //const string StorageConnectingStringFormat = @"DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}";
        //public const string ConsumerHash = "OK8N2lDKZLYgAq2tBMg9I1Shan7SYPpUBeNDFcMxvzM=";
        //public const string storageAccountName = "iotdb";
        //public const string storageAccountKey = "C7tqYeoUH8WsW53UrMciUZ08LbG4uT9NBArwnUAf1Yqvuym1iL6JpmMFBIOLk/DDlQxCYx5dQOXOwwW7I/YilA==";
        //public const string consumerKeyName = @"consumer";
        //public const string consumerHash = "lxkPVRY/kVKi1NV/mz8q1YPVmOez5q9jlnOD2ELC28Y=";
        //public const string DefaultBusName = "iot-bus";
        public const string DefaultHubName = "iot-hub";

        public const string HubConnectionStringFormat= "HostName={0}.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey={1}";
        public const string SharedOwnerAccessKey = "tweYUQyj3MN5qbTGB/qWtGnG5BA2G56mSzqz1V43gBc=";
    }
}
