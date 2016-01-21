using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace LogReceiverSyphonConsole
{
    class Program
    {
        private const string queueName = "logs";

        private const string primaryQueueSharedAccessKeyName = "RootManageSharedAccessKey";

        // This is the bad key
        //private const string primaryQueueSharedAccessKey = "blah";

        // This is the good key.
        private const string primaryQueueSharedAccessKey = "iOqJY7JnqzKzeEJ9sYh7v+ZWJE4IgOnyORXqNaqRDW4=";

        private const string secondaryQueueSharedAccessKeyName = "RootManageSharedAccessKey";
        private const string secondaryQueueSharedAccessKey = "p2UEGMOsxeKUPxzLh7xhhFGCqHcIi/YKSang7jZPnqk=";

        private static Uri primaryServiceBusAddressUri;
        private static TokenProvider primaryTokenProvider;

        private static Uri secondaryServiceBusAddressUri;
        private static TokenProvider secondaryTokenProvider;

        private static NamespaceManager primaryNamespaceManager;
        private static NamespaceManager secondaryNamespaceManager;

        private static MessagingFactory primaryMessagingFactory;
        private static MessagingFactory secondaryMessagingFactory;

        static void Main(string[] args)
        {
            primaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", "collier", string.Empty);
            primaryTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(primaryQueueSharedAccessKeyName, primaryQueueSharedAccessKey);

            secondaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", "collier-secondary", string.Empty);
            secondaryTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(secondaryQueueSharedAccessKeyName, secondaryQueueSharedAccessKey);

            primaryNamespaceManager = new NamespaceManager(primaryServiceBusAddressUri, primaryTokenProvider);
            secondaryNamespaceManager = new NamespaceManager(secondaryServiceBusAddressUri, secondaryTokenProvider);

            primaryMessagingFactory = MessagingFactory.Create(primaryServiceBusAddressUri,
                new MessagingFactorySettings { TokenProvider = primaryTokenProvider });

            secondaryMessagingFactory = MessagingFactory.Create(secondaryServiceBusAddressUri,
                new MessagingFactorySettings { TokenProvider = secondaryTokenProvider });

            try
            {
                SendAvailabilityPairedNamespaceOptions sendAvailabilityOptions =
                    new SendAvailabilityPairedNamespaceOptions(secondaryNamespaceManager, secondaryMessagingFactory,
                    backlogQueueCount: 10,
                    failoverInterval: TimeSpan.Zero,
                    enableSyphon: true);


                primaryMessagingFactory.PairNamespaceAsync(sendAvailabilityOptions).Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            while (true)
            {
                Console.WriteLine();

                Thread.Sleep(TimeSpan.FromSeconds(2));
            }
        }
    }
}
