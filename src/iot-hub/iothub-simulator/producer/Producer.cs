using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading;
using shared.core;
using shared.Messages;

namespace producer
{
    class Producer
    {
        static void Main(string[] args)
        {
            var cmd = new CommandArgs(args);

            if (!cmd.ContainsKey("token") || !cmd.ContainsKey("name"))
            {
                Console.WriteLine("Usage: -name ABC -token ALPHA");
                Console.ReadLine();
                return;
            }

            var deviceName = cmd.GetValue("name", "default");
            var deviceKey = cmd.GetValue("token", null);

            Console.WriteLine("Press Ctrl-C to stop the sender process");
            Console.WriteLine("Press Enter to start now");
            Console.ReadLine();

             SendingRandomMessages(deviceName, deviceKey);
        }

        static void SendingRandomMessages(string deviceName, string token)
        {
            // No need of SAS token for producer
            var deviceClient = DeviceClient.Create("iot-hub.azure-devices.net", 
                        new DeviceAuthenticationWithRegistrySymmetricKey(deviceName, token));

            Console.ForegroundColor = ConsoleColor.Cyan;

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

                    using (var eventData = new Message(Encoding.UTF8.GetBytes(dataString)))
                    {
                        // Set user properties if needed
                        eventData.Properties.Add("Type", "Telemetry_" + DateTime.Now.ToLongTimeString());

                        // PartitionKey does not exist in IoT Hub
                        // eventData.PartitionKey = deviceName;

                        // Can send max 250KB in single message or 500 in a batch
                        deviceClient.SendEventAsync(eventData).Wait();
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
