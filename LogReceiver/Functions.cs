using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;

namespace LogReceiver
{
    public class Functions
    {
        public static async Task ProcessQueueMessageAsync(
            [ServiceBusTrigger("logs")] BrokeredMessage message,
            TextWriter logger)
        {
            await logger.WriteLineAsync(string.Format("Received message with ID [{0}].", message.MessageId));

            await logger.WriteLineAsync(message.GetBody<string>());
        }

        //public static void ProcessQueueMessage(
        //    [ServiceBusTrigger("logs")] BrokeredMessage message,
        //    TextWriter logger)
        //{
        //    logger.WriteLine("Received message with ID [{0}].", message.MessageId);

        //    logger.WriteLine(message.GetBody<string>());
        //}
    }
}
