using System;
using System.Text;
using System.Threading;
using Microsoft.ServiceBus.Messaging;
using shared.Messages;
using shared.core;
using Newtonsoft.Json;
using Microsoft.ServiceBus;
using shared;

namespace device
{
    class Producer
    {
        static void Main(string[] args)
        {
            var cmd = new CommandArgs(args);

            if (!cmd.ContainsKey("hash") || !cmd.ContainsKey("name"))
            {
                Console.WriteLine("Usage: -name ABC -hash ALPHA");
                Console.ReadLine();
                return;
            }

            var deviceName = cmd.GetValue("name", "default");
            var hash = cmd.GetValue("hash", null);
            var eventHubName = cmd.GetValue("hub", Constants.DefaultHubName);
            var serviceBus = cmd.GetValue("bus", Constants.DefaultBusName);

            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();

            SendingRandomMessages(deviceName, hash, eventHubName, serviceBus);
        }

        static void SendingRandomMessages(string deviceName, string hash, string eventHubName, string serviceBus)
        {
            var serviceBusUri = new Uri(String.Format("sb://{0}.servicebus.windows.net/", serviceBus));
            var connStr = ServiceBusConnectionStringBuilder.CreateUsingSharedAccessKey(serviceBusUri, deviceName, hash);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Using connection string:" + connStr);
            var eventHubClient = EventHubClient.CreateFromConnectionString(connStr, eventHubName);
            eventHubClient.RetryPolicy = RetryPolicy.Default;

            var rand = new Random();
            int counter = 0;
            while (true)
            {
                try
                {
                    var sensor = new SensorData
                    {
                        DeviceName = deviceName,
                        RecordedAt = DateTime.UtcNow,
                        SensorType = "Temperature",
                        MeasuredValue = rand.Next(25, 27),
                        MessageId = Guid.NewGuid().ToString()
                    };

                    var dataString = JsonConvert.SerializeObject(sensor, Formatting.Indented);

                    using (var eventData = new EventData(Encoding.UTF8.GetBytes(dataString)))
                    {
                        // Set user properties if needed
                        eventData.Properties.Add("Type", "Telemetry_" + DateTime.Now.ToLongTimeString());
                        eventData.PartitionKey = deviceName;

                        eventHubClient.Send(eventData);
                    }

                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine("Sent Message to Hub:" + (++counter));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(dataString);

                    Thread.Sleep(500);
                }
                catch (Exception exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("{0} > Exception: {1}", DateTime.Now, exception.Message);
                    Console.ResetColor();
                }

                Thread.Sleep(200);
            }
        }
    }
}
