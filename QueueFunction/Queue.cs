using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace QueueFunction
{
    public class Queue
    {
        [FunctionName("getMessages")]
        //con el return insertamos en un storage tipo table llamado orders que usa el connection string del appsetings.json
        [return: Table("Orders", Connection = "connectionString")]
        public TableOrder Run([QueueTrigger("appqueue", Connection = "connectionString")]Order order, ILogger log)
        {
            TableOrder tableOrder = new TableOrder()
            {
                PartitionKey = order.OrderID,
                RowKey = order.Quantity.ToString()
            };



            log.LogInformation($"Order Information has been written to the table");
            return tableOrder;
        }

        //otra forma
        [FunctionName("GetMessages")]
        public void Run([QueueTrigger("appqueue", Connection = "connectionString")] Order order, ILogger log, [Table("Orders", Connection = "connectionString")] ICollector<TableOrder> tableorder)
        {
            TableOrder tableOrder = new TableOrder()
            {
                PartitionKey = order.OrderID,
                RowKey = order.Quantity.ToString()
            };
            tableorder.Add(tableOrder);
            log.LogInformation("Order written to table");

        }
    }
}
