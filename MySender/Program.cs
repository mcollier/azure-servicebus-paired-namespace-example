using System;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace MySender
{
    class Program
    {
        private static string queueName;

        private static NamespaceManager primaryNamespaceManager;
        private static NamespaceManager secondaryNamespaceManager;

        private static MessagingFactory primaryMessagingFactory;
        private static MessagingFactory secondaryMessagingFactory;

        static void Main(string[] args)
        {
            ConfigureServiceBusNamespace();

            //CreatePrimaryQueue().Wait();

            for (int i = 0; i < 50; i++)
            {
                SendMessage(string.Format("{0} - Hello Mike!", i));
            }

            Console.WriteLine("All done - press any key to exit.");
            Console.ReadLine();
        }

        static void ConfigureServiceBusNamespace()
        {
            queueName = ConfigurationManager.AppSettings["queuename"];

            string primaryAccessKeyName = ConfigurationManager.AppSettings["primarySBKeyName"];
            string primaryAccessKey = ConfigurationManager.AppSettings["primarySBKey"];
            string primaryNamespaceName = ConfigurationManager.AppSettings["primarySBNamespaceName"];

            string secondaryAccessKeyName = ConfigurationManager.AppSettings["secondarySBKeyName"];
            string secondaryAccessKey = ConfigurationManager.AppSettings["secondarySBKey"];
            string secondaryNamespaceName = ConfigurationManager.AppSettings["secondarySBNamespaceName"];

            // Build up the primary 
            var primaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", primaryNamespaceName, string.Empty);
            var primaryTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(primaryAccessKeyName, primaryAccessKey);

            primaryNamespaceManager = new NamespaceManager(primaryServiceBusAddressUri, primaryTokenProvider);
            primaryMessagingFactory = MessagingFactory.Create(primaryServiceBusAddressUri,
               new MessagingFactorySettings { TokenProvider = primaryTokenProvider });

            // Build up the secondary
            var secondaryServiceBusAddressUri = ServiceBusEnvironment.CreateServiceUri("sb", secondaryNamespaceName, string.Empty);
            var secondaryTokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(secondaryAccessKeyName, secondaryAccessKey);

            secondaryNamespaceManager = new NamespaceManager(secondaryServiceBusAddressUri, secondaryTokenProvider);
            secondaryMessagingFactory = MessagingFactory.Create(secondaryServiceBusAddressUri,
                new MessagingFactorySettings { TokenProvider = secondaryTokenProvider });

            // Set up the paired namespace details
            SendAvailabilityPairedNamespaceOptions sendAvailabilityOptions =
                new SendAvailabilityPairedNamespaceOptions(secondaryNamespaceManager, secondaryMessagingFactory,
                backlogQueueCount: 10,
                failoverInterval: TimeSpan.Zero,
                enableSyphon: false);

            primaryMessagingFactory.PairNamespaceAsync(sendAvailabilityOptions).Wait();

            if (sendAvailabilityOptions.BacklogQueueCount < 1)
            {
                Console.WriteLine("ERROR: No backlog queues created");
            }
        }

        static async Task CreatePrimaryQueue()
        {
            var exists = await primaryNamespaceManager.QueueExistsAsync(queueName);
            if (!exists)
            {
                await primaryNamespaceManager.CreateQueueAsync(queueName);
            }
        }

        static async Task SendMessageAsync(string msg)
        {
            QueueClient queueClient = primaryMessagingFactory.CreateQueueClient(queueName);

            BrokeredMessage bm = new BrokeredMessage(msg);

            Console.WriteLine("Sending message {0}", bm.MessageId);

            await queueClient.SendAsync(bm);
        }

        static void SendMessage(string msg)
        {
            QueueClient queueClient = primaryMessagingFactory.CreateQueueClient(queueName);

            BrokeredMessage bm = new BrokeredMessage(msg);

            Console.WriteLine("Sending message {0}", bm.MessageId);

            queueClient.Send(bm);
        }
    }
}
