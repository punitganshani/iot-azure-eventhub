using Microsoft.ServiceBus.Messaging;
using shared;
using shared.core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace consumer
{
    class Consumer
    {
        static void Main(string[] args)
        {
            var cmd = new CommandArgs(args);

            string storageConnectionString = Constants.GetStorageConnectionString(Constants.storageAccountName, Constants.storageAccountKey);
            var serviceBus = cmd.GetValue("bus", Constants.DefaultBusName);
            var eventHubName = cmd.GetValue("hub", Constants.DefaultHubName);
            var host = RegisterEventProcessor<ConsoleEventProcessor>(serviceBus, eventHubName,
                                                                        Constants.consumerKeyName, Constants.consumerHash,
                                                                        storageConnectionString
                                                                        );

            Console.WriteLine("Receiving. Press enter key to stop worker.");
            Console.ReadLine();
            host.UnregisterEventProcessorAsync().Wait();
        }

        private static EventProcessorHost RegisterEventProcessor<T>(string serviceBus, string eventHubName,
                string consumerKeyName, string consumerHash,
                string storageConnectionString) where T : IEventProcessor
        {
            string serviceBusConnectionString = Constants.GetBusConnectionString(serviceBus, consumerKeyName, consumerHash);
            var eventClient = EventHubClient.CreateFromConnectionString(serviceBusConnectionString, eventHubName);
            var eventProcessorHost = new EventProcessorHost(Environment.MachineName, eventHubName,
                            eventClient.GetDefaultConsumerGroup().GroupName, serviceBusConnectionString, storageConnectionString);

            Console.WriteLine("Registering EventProcessor...");
            eventProcessorHost.RegisterEventProcessorAsync<ConsoleEventProcessor>().Wait();

            return eventProcessorHost;
        }
    }

    public class ConsoleEventProcessor : IEventProcessor
    {
        int counter = 0;
        async Task IEventProcessor.CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine("Processor Shutting Down. Partition '{0}', Reason: '{1}'.", context.Lease.PartitionId, reason);
            if (reason == CloseReason.Shutdown)
            {
                await context.CheckpointAsync();
            }
        }

        Task IEventProcessor.OpenAsync(PartitionContext context)
        {
            Console.WriteLine("ConsoleEventProcessor initialized.  Partition: '{0}', Offset: '{1}'", context.Lease.PartitionId, context.Lease.Offset);
            return Task.FromResult<object>(null);
        }

        async Task IEventProcessor.ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            foreach (EventData eventData in messages)
            {
                if (eventData.SystemProperties.ContainsKey("correlation-id"))
                {
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(string.Format("Response Received Partition: '{0}' {1}", context.Lease.PartitionId, (++counter)));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Data: " + data);
                }
                else
                {
                    string data = Encoding.UTF8.GetString(eventData.GetBytes());
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(string.Format("Message Received Partition: '{0}' {1}", context.Lease.PartitionId, (++counter)));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine("Data: " + data);
                }
            }

            Thread.Sleep(500);
            await context.CheckpointAsync();
        }
    }
}
