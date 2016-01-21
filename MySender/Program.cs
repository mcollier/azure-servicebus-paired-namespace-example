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
        //private const string primaryQueueSharedAccessKey = "iOqJY7JnqzKzeEJ9sYh7v+ZWJE4IgOnyORXqNaqRDW4=";
        private const string primaryQueueSharedAccessKey = "HNdXanqLkWNLKha46KBpdg6CU4429D8fs6VPU9ImjaQ=";

        

        private const string secondaryQueueSharedAccessKeyName = "RootManageSharedAccessKey";

        //private const string secondaryQueueSharedAccessKey = "p2UEGMOsxeKUPxzLh7xhhFGCqHcIi/YKSang7jZPnqk=";
        private const string secondaryQueueSharedAccessKey = "BS4BBdCstSpQC4CCJj2SQxYpICJI9Y0phwbtERbzt24=";

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
            
            ConfigureServiceBusNamespace();

            CreatePrimaryQueue().Wait();

            for (int i = 0; i < 50; i++)
            {
                SendMessage(string.Format("{0} - Hello Mike!", i)).Wait();
            }

            Console.WriteLine("All done - press any key to exit.");
            Console.ReadLine();
            //SendMessage2("Hello Mike!!");
        }

        static void ConfigureServiceBusNamespace()
        {
            primaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", "b-collier", string.Empty);
            primaryTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(primaryQueueSharedAccessKeyName, primaryQueueSharedAccessKey);

            secondaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", "b-collier-secondary", string.Empty);
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
