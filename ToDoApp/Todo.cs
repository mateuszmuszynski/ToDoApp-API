using System;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            public bool IsDone { get; set; }
        }

        [FunctionName("GetTodos")]
        public static HttpResponseMessage GetTodo([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "todos")]HttpRequestMessage req, [Table("person", Connection = "Connection")]IQueryable<ToDoItem> table, TraceWriter log)
        {
            return req.CreateResponse(HttpStatusCode.OK, table.ToList());
        }

        [FunctionName("AddToDo")]
        public static HttpResponseMessage AddToDo([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "todos")]string name, HttpRequestMessage req, 
            [Table("todo", Connection = "Connection")]ICollector<ToDoItem> table, TraceWriter log)
        {
            table.Add(new ToDoItem { PartitionKey = "todoItem", RowKey = DateTime.UtcNow.Ticks.ToString(), Name = name });
            return req.CreateResponse(HttpStatusCode.OK);
        }

        public class UpdateToDoModel
        {
            public string RowKey { get; set; }
            public string Name { get; set; }
            public bool IsDone { get; set; }
        }

        [FunctionName("UpdateToDo")]
        public static HttpResponseMessage UpdateToDo([HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "todos")]UpdateToDoModel item, HttpRequestMessage req, 
            [Table("todo", Connection = "Connection")]CloudTable table, TraceWriter log)
        {
            var entity = new TableEntity("todoItem", item.RowKey)
            {
                ETag = "*" //Force override ("last-write-wins" strategy) 
            };

            var operation = TableOperation.Replace(entity);
            table.Execute(operation);
            return req.CreateResponse(HttpStatusCode.OK);
        }

        //[FunctionName("DeleteToDo")]
        //public static HttpResponseMessage DeleteToDo([HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "todos")]string rowKey, HttpRequestMessage req, 
        //    [Table("todo", "todoItem", "{httpTrigger}", Connection = "Connection")]ToDoItem item, [Table("todo", Connection = "Connection")]CloudTable cloudTable, TraceWriter log)
        //{
        //    //[Table("todo", Connection = "Connection")]IQueryable<ToDoItem> table
        //    //var item = (from todoItem in table
        //    //            where todoItem.PartitionKey == "todoItem" && todoItem.RowKey == rowKey
        //    //            select todoItem).FirstOrDefault();

        //    var operation = TableOperation.Delete(item);
        //    cloudTable.Execute(operation);
        //    return req.CreateResponse(HttpStatusCode.OK);
        //}
    }
}
