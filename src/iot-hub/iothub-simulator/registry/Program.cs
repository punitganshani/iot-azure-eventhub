using Microsoft.Azure.Devices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace registry
{
    class Program
    {
        static void Main(string[] args)
        {
            ViewAllDevices().Wait();
            Console.WriteLine("Press any key to exit..");
            Console.ReadLine();
        }

        private static async Task ViewAllDevices()
        {
            var connectionString = "HostName=iot-hub.azure-devices.net;SharedAccessKeyName=registryRead;SharedAccessKey=Fj6t3FuX+D+QIbR0RgLWp3zxNv1Et1R5RLRY7MRx0Ic=";
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            var devices = await registryManager.GetDevicesAsync(100);

            foreach (var device in devices)
            {
                Console.WriteLine(JsonConvert.SerializeObject(device, Formatting.Indented));
            }
        }
    }
}
