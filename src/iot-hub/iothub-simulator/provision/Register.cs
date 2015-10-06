using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Common.Exceptions;
using shared.core;
using shared;

namespace provision
{
    class Register
    {
        static void Main(string[] args)
        {
            var cmd = new CommandArgs(string.Join(" ", args));

            if (!cmd.ContainsKey("name"))
            {
                Console.WriteLine("Usage: -name ABC");
                // Console.ReadKey();
                return;
            }

            var deviceName = cmd.GetValue("name", "Default");
            var eventHubName = cmd.GetValue("hub", Constants.DefaultHubName);
            var original = Console.ForegroundColor;
            RegisterDevice(deviceName, eventHubName).Wait();
            Console.ForegroundColor = original;
            Console.ReadKey();
        }

        private static async Task RegisterDevice(string deviceName, string hubName)
        {
            var connectionString = "HostName=iot-hub.azure-devices.net;SharedAccessKeyName=iothubowner;SharedAccessKey=tweYUQyj3MN5qbTGB/qWtGnG5BA2G56mSzqz1V43gBc=";
            var registryManager = RegistryManager.CreateFromConnectionString(connectionString);

            Device device;
            try
            {
                device = await registryManager.AddDeviceAsync(new Device(deviceName));
            }
            catch (DeviceAlreadyExistsException)
            {
                device = await registryManager.GetDeviceAsync(deviceName);
            }

            Console.WriteLine("Generated device key: {0}", device.Authentication.SymmetricKey.PrimaryKey);
        }
    }
}
