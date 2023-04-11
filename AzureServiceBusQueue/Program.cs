//1.Install Azure.Messaging.ServiceBus

using Azure.Messaging.ServiceBus;
using AzureServiceBusQueue;
using Newtonsoft.Json;

string connectionString = "";
string queueName = "appqueue";
string[] Importance = new string[] { "High", "Medium", "Low" };

List<Order> orders = new List<Order>()
{
    new Order(){ OrderID="O4", Quantity = 140, UnitPrice =1.99f},
    new Order(){ OrderID="O5", Quantity = 250, UnitPrice =19.99f},
    new Order(){ OrderID="O6", Quantity = 360, UnitPrice =149.99f}
};



async Task SendMessage(List<Order> orders)
{
    ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
    var serviceBusSender = serviceBusClient.CreateSender(queueName);

    var serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();
    int i = 0;
    foreach (var item in orders)
    {
        ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(item));
        serviceBusMessage.ContentType="application/json";
        //añadir custom props
        serviceBusMessage.ApplicationProperties.Add("Importance", Importance[i]);
        //tiempo que vive el mensaje en la cola
        serviceBusMessage.TimeToLive = TimeSpan.FromSeconds(30);
        i++;
        if (!serviceBusMessageBatch.TryAddMessage(serviceBusMessage))
        {
            throw new Exception("Error ocurred");
        }
    };
    Console.WriteLine("Sending Messages");
    await serviceBusSender.SendMessagesAsync(serviceBusMessageBatch);
}

async Task PeekMessages()
{
    ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
    var serviceBusReceiver = serviceBusClient.CreateReceiver(queueName,
        new ServiceBusReceiverOptions() { ReceiveMode= ServiceBusReceiveMode.PeekLock });

    var messages = serviceBusReceiver.ReceiveMessagesAsync();

    await foreach (var item in messages)
    {
        Order order = JsonConvert.DeserializeObject<Order>(item.Body.ToString());

        Console.WriteLine("Order Id {0} and {1}",order.OrderID,order.Quantity);
    }

    serviceBusReceiver
};

async Task ReceiveMessages()
{
    ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
    var serviceBusReceiver = serviceBusClient.CreateReceiver(queueName,
        new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.ReceiveAndDelete });

    IAsyncEnumerable<ServiceBusReceivedMessage> messages = serviceBusReceiver.ReceiveMessagesAsync();

    await foreach (var item in messages)
    {
        Order order = JsonConvert.DeserializeObject<Order>(item.Body.ToString());

        Console.WriteLine("Order Id {0} and {1}", order.OrderID, order.Quantity);
    }
}

async Task GetProperties()
{
    ServiceBusClient serviceBusClient = new ServiceBusClient(connectionString);
    var serviceBusReceiver = serviceBusClient.CreateReceiver(queueName,
        new ServiceBusReceiverOptions() { ReceiveMode = ServiceBusReceiveMode.PeekLock });

    IAsyncEnumerable<ServiceBusReceivedMessage> messages = serviceBusReceiver.ReceiveMessagesAsync();

    await foreach (var item in messages)
    {
        Console.WriteLine("Message Id {0} and sequence number {1})", item.MessageId, item.SequenceNumber);
        Console.WriteLine("Importance {0}",item.ApplicationProperties["Importance"]);
        //Console.WriteLine("Message importance {0}");

    }
    await serviceBusReceiver.DisposeAsync();

}

await GetProperties();
//await PeekMessages();
//await SendMessage(orders);