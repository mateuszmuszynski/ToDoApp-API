using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace ToDoApp
{
    public static class Queues
    {
        [FunctionName("Queue1")]
        [return: Queue("queue2")]
        public static QueueItem Queue1([QueueTrigger("queue1")]string myQueueItem, [Table("todos", "todoItem", "{queueTrigger}")]ToDoItem entity, TraceWriter log)
        {
            log.Info($"Queue1 called with message: {myQueueItem}");

            return new QueueItem
            {
                Body = myQueueItem
            };
        }

        [FunctionName("Queue2")]
        [return: Queue("queue4")]
        public static QueueItem Queue2([QueueTrigger("queue2")]QueueItem myQueueItem, [Queue("queue3")]ICollector<string> queue3, TraceWriter log)
        {
            var json = JsonConvert.SerializeObject(myQueueItem);

            log.Info($"Queue2 called with message: {json}");

            if (string.IsNullOrEmpty(myQueueItem.Body))
            {
                throw new Exception();
            }

            queue3.Add(json);
            return myQueueItem;
        }

        [FunctionName("Queue3")]
        public static void Queue3([QueueTrigger("queue3")]QueueItem myQueueItem, TraceWriter log)
        {
            log.Info($"Queue3 called with message: {myQueueItem.Body}");
        }

        [FunctionName("Queue4")]
        public static void Queue4([QueueTrigger("queue4")]string myQueueItem, TraceWriter log)
        {
            log.Info($"Queue4 called with message: {myQueueItem}");
        }
    }
}
