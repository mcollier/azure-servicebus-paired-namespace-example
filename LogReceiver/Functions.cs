using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.ServiceBus.Messaging;

namespace LogReceiver
{
    public class Functions
    {
        // This function will get triggered/executed when a new message is written 
        // on an Azure Queue called queue.
        //public static void ProcessQueueMessage([QueueTrigger("queue")] string message, TextWriter log)
        //{
        //    log.WriteLine(message);
        //}

        public static async Task ProcessQueueMessageAsync(
            [ServiceBusTrigger("logs")] BrokeredMessage message,
            TextWriter logger)
        {
            var b = message.GetBody<string>();
            await logger.WriteLineAsync(b);
            //Console.WriteLine(message);
        }
    }
}
