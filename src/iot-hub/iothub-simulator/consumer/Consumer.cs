using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using Microsoft.Azure.Devices.Common;
using Microsoft.ServiceBus.Messaging;
using shared.core;
using shared;

namespace consumer
{
    class Consumer
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Receiving. Press enter key to stop worker.");
            ReceiveMessages().Wait();        

            Console.ReadLine();
        }

        private async static Task ReceiveMessages()
        {
            string connectionString = Constants.GetServiceString();
            string iotHubD2cEndpoint = "messages/events";
            Console.WriteLine("Receive messages\n");
            var eventHubClient = EventHubClient.CreateFromConnectionString(connectionString, iotHubD2cEndpoint);

            var d2cPartitions = eventHubClient.GetRuntimeInformation().PartitionIds;

            foreach (string partition in d2cPartitions)
            {
                ReceiveFromParition(eventHubClient, partition);
            }
        }

        private static async Task ReceiveFromParition(EventHubClient eventHubClient, string partition)
        {
            int counter = 0;
            var eventHubReceiver = eventHubClient.GetDefaultConsumerGroup().CreateReceiver(partition, DateTime.Now);
            while (true)
            {
                var eventData = await eventHubReceiver.ReceiveAsync();
                if (eventData == null) continue;

                string data = Encoding.UTF8.GetString(eventData.GetBytes());
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(string.Format("Message Received Partition: '{0}' {1}", partition, (++counter)));
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Data: " + data);
            }
        }
    }
}
