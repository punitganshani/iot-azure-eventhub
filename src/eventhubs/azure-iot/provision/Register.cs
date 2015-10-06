using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using shared;
using shared.core;
using System;

namespace provision
{
    class Register
    {        
        static void Main(string[] args)
        {
            var cmd = new CommandArgs(string.Join(" ", args));

            if (!cmd.ContainsKey("name"))
            {
                Console.WriteLine("Usage: -name ABC (-hub inbox -bus iot-bus)");
                // Console.ReadKey();
                return;
            }

            var deviceName = cmd.GetValue("name", "Default");
            var eventHubName = cmd.GetValue("hub", Constants.DefaultHubName);
            var serviceBus = cmd.GetValue("bus", Constants.DefaultBusName);
            var original = Console.ForegroundColor;
            CreateHashForDevice(deviceName, eventHubName, serviceBus);
            Console.ForegroundColor = original;
            Console.ReadKey();
        }

        private static async void CreateHashForDevice(string deviceName, string eventHubName, string serviceBus)
        {
            string serviceBusConnectionString = Constants.GetBusConnectionString(serviceBus, "configure", Constants.ConsumerHash);
            var connectionString = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);
            var ehd = await namespaceManager.GetEventHubAsync(eventHubName);

            // Create a customer SAS rule with Manage permissions
            ehd.UserMetadata = deviceName;
            string ruleName = deviceName;
            string ruleKey = SharedAccessAuthorizationRule.GenerateRandomKey();
            ehd.Authorization.Add(new SharedAccessAuthorizationRule(ruleName, ruleKey, new AccessRights[] { AccessRights.Send }));
            namespaceManager.UpdateEventHubAsync(ehd).Wait();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(String.Format("{0} => {1}", deviceName, ruleKey));
        }
    }
}
