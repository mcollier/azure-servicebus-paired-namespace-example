using System;
using System.Configuration;
using System.Threading;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace LogReceiverSyphonConsole
{
    class Program
    {
        private static NamespaceManager primaryNamespaceManager;
        private static NamespaceManager secondaryNamespaceManager;

        private static MessagingFactory primaryMessagingFactory;
        private static MessagingFactory secondaryMessagingFactory;

        static void Main()
        {
            string primaryAccessKeyName = ConfigurationManager.AppSettings["primarySBKeyName"];
            string primaryAccessKey = ConfigurationManager.AppSettings["primarySBKey"];
            string primaryNamespaceName = ConfigurationManager.AppSettings["primarySBNamespaceName"];

            string secondaryAccessKeyName = ConfigurationManager.AppSettings["secondarySBKeyName"];
            string secondaryAccessKey = ConfigurationManager.AppSettings["secondarySBKey"];
            string secondaryNamespaceName = ConfigurationManager.AppSettings["secondarySBNamespaceName"];

            Uri primaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", primaryNamespaceName, string.Empty);
            TokenProvider primaryTokenProvider =
                TokenProvider.CreateSharedAccessSignatureTokenProvider(primaryAccessKeyName, primaryAccessKey);

            Uri secondaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", secondaryNamespaceName, string.Empty);
            TokenProvider secondaryTokenProvider =
                TokenProvider.CreateSharedAccessSignatureTokenProvider(secondaryAccessKeyName, secondaryAccessKey);

            primaryNamespaceManager = new NamespaceManager(primaryServiceBusAddressUri, primaryTokenProvider);
            secondaryNamespaceManager = new NamespaceManager(secondaryServiceBusAddressUri, secondaryTokenProvider);

            primaryMessagingFactory = MessagingFactory.Create(primaryServiceBusAddressUri,
                new MessagingFactorySettings { TokenProvider = primaryTokenProvider });

            secondaryMessagingFactory = MessagingFactory.Create(secondaryServiceBusAddressUri,
                new MessagingFactorySettings { TokenProvider = secondaryTokenProvider });


            SendAvailabilityPairedNamespaceOptions sendAvailabilityOptions =
                new SendAvailabilityPairedNamespaceOptions(secondaryNamespaceManager, secondaryMessagingFactory,
                    backlogQueueCount: 10,
                    failoverInterval: TimeSpan.Zero,
                    enableSyphon: true);


            primaryMessagingFactory.PairNamespaceAsync(sendAvailabilityOptions).Wait();

            while (true)
            {
                Console.WriteLine("Working . . . ");
                Thread.Sleep(TimeSpan.FromMilliseconds(100));
            }
        }
    }
}
