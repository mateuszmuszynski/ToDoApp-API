using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace ToDoApp
{
    public static class Queues
    {
        public class QueueItem
        {
            public string Body { get; set; }
        }

        [FunctionName("Queue1")]
        [return: Queue("queue2")]
        public static QueueItem Queue1([QueueTrigger("queue1")]string myQueueItem, TraceWriter log)
        {
            log.Info($"Queue1 called with message: {myQueueItem}");

            return new QueueItem
            {
                Body = myQueueItem
            };
        }

        [FunctionName("Queue2")]
        public static void Queue2([QueueTrigger("queue2")]QueueItem myQueueItem, [Queue("queue3")]ICollector<string> queue, TraceWriter log)
        {
            var json = JsonConvert.SerializeObject(myQueueItem);

            log.Info($"Queue2 called with message: {json}");

            if (string.IsNullOrEmpty(myQueueItem.Body))
            {
                throw new Exception();
            }

            queue.Add(json);

            return;
        }

        [FunctionName("Queue3")]
        public static void Queue3([QueueTrigger("queue3")]QueueItem myQueueItem, TraceWriter log)
        {
            log.Info($"Queue3 called with message: {myQueueItem.Body}");
        }
    }
}
