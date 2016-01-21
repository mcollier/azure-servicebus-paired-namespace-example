using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace MySender
{
    class Program
    {
        private const string queueName = "logs";

        private const string primaryQueueSharedAccessKeyName = "RootManageSharedAccessKey";

        // This is the bad key
        //private const string primaryQueueSharedAccessKey = "blah";

        // This is the good key.
        private const string primaryQueueSharedAccessKey = "ixxxxxxxxxxxxxx"; // replace with legit good key

        private const string secondaryQueueSharedAccessKeyName = "RootManageSharedAccessKey";
        private const string secondaryQueueSharedAccessKey = "pxxxxxxxxxxx"; // replace with legit good key

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

            SendAvailabilityPairedNamespaceOptions sendAvailabilityOptions =
                new SendAvailabilityPairedNamespaceOptions(secondaryNamespaceManager, secondaryMessagingFactory,
                backlogQueueCount: 10,
                failoverInterval: TimeSpan.Zero,
                enableSyphon: false);


            primaryMessagingFactory.PairNamespaceAsync(sendAvailabilityOptions).Wait();

            //CreatePrimaryQueue().Wait();

            //SendMessage("Hello World!").Wait();
            SendMessage2("Hello Mike!!");
        }

        static async Task CreatePrimaryQueue()
        {
            var exists = await primaryNamespaceManager.QueueExistsAsync(queueName);
            if (!exists)
            {
                await primaryNamespaceManager.CreateQueueAsync(queueName);
            }
        }

        static async Task SendMessage(string msg)
        {
            QueueClient queueClient = primaryMessagingFactory.CreateQueueClient(queueName);

            BrokeredMessage bm = new BrokeredMessage(msg);

            await queueClient.SendAsync(bm);
        }

        static void SendMessage2(string msg)
        {
            QueueClient queueClient = primaryMessagingFactory.CreateQueueClient(queueName);

            BrokeredMessage bm = new BrokeredMessage(msg);

            queueClient.Send(bm);
        }
    }
}
