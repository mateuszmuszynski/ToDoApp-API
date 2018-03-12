using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;

namespace ToDoApp
{
    public static class Todo
    {
        public class ToDoItem : TableEntity
        {
            public string Name { get; set; }
        }

        [FunctionName("GetTodos")]
        public static List<ToDoItem> GetTodos([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")]HttpRequestMessage req, [Table("todos")]IQueryable<ToDoItem> table, TraceWriter log)
        {
            return table.ToList();
        }

        [FunctionName("GetTodo")]
        public static ToDoItem GetTodo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos/{id}")]HttpRequestMessage req, string id, [Table("todos")]IQueryable<ToDoItem> query)
        {
            var entity = (from todoItem in query
                          where todoItem.PartitionKey == "todoItem" && todoItem.RowKey == id
                          select todoItem).FirstOrDefault();

            return entity;
        }

        [FunctionName("AddToDo")]
        public static async Task<HttpResponseMessage> AddToDo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")]string name, HttpRequestMessage req, [Table("todos")]ICollector<ToDoItem> table, TraceWriter log)
        {
            table.Add(new ToDoItem { PartitionKey = "todoItem", RowKey = DateTime.UtcNow.Ticks.ToString(), Name = name });
            return req.CreateResponse(HttpStatusCode.Created);
        }

        public class UpdateToDoModel
        {
            public string Name { get; set; }
        }

        [FunctionName("UpdateToDo")]
        public static async Task UpdateToDo([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos/{id}")]UpdateToDoModel item, string id, HttpRequestMessage req, [Table("todos")]IQueryable<ToDoItem> query, [Table("todos")]CloudTable table, TraceWriter log)
        {
            var entity = (from todoItem in query
                          where todoItem.PartitionKey == "todoItem" && todoItem.RowKey == id
                          select todoItem).FirstOrDefault();

            entity.Name = item.Name;

            var operation = TableOperation.Replace(entity);
            table.Execute(operation);
            return;
        }

        [FunctionName("DeleteToDo")]
        public static HttpResponseMessage DeleteToDo([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos/{id}")]HttpRequestMessage req, string id, [Table("todos")]IQueryable<ToDoItem> entity, [Table("todos")]CloudTable cloudTable, TraceWriter log)
        {
            var operation = TableOperation.Delete(new TableEntity { PartitionKey = "todoItem", RowKey = id, ETag = "*" });
            cloudTable.Execute(operation);
            return req.CreateResponse(HttpStatusCode.OK);
        }

        [FunctionName("QueueOutput")]
        [return: Queue("queue1")]
        public static string QueueOutput([HttpTrigger] dynamic input, TraceWriter log)
        {
            return input;
        }
    }
}
