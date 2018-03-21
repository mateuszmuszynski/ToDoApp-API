using Microsoft.WindowsAzure.Storage.Table;

namespace ToDoApp
{
    public class ToDoItem : TableEntity
    {
        public string Name { get; set; }
    }
}
