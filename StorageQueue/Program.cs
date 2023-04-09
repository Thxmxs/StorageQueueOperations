//1 instalar azure.storage.queues

using Azure.Storage.Queues;
using Newtonsoft.Json;
using StorageQueue;

string connectionString = "";
string queueName = "appqueue";

//Agregar un mensaje a la cola
async Task SendMessage(string orderId, int quantity)
{
    QueueClient queueClient = new QueueClient(connectionString, queueName);

    if (queueClient.Exists())
    {
        Order order = new Order() { OrderID = orderId, Quantity= quantity };
        var jsonObject = JsonConvert.SerializeObject(order);
        //La informacion debe ser enviado en formato 6 por eso la linea 20 y 21 estan convirtiendo el json
        var bytes = System.Text.Encoding.UTF8.GetBytes(jsonObject.ToString());
        var message = System.Convert.ToBase64String(bytes);

        await queueClient.SendMessageAsync(message);
        Console.WriteLine("The message has been sent");
    }
}

//ver los mensajes de una cola no los elimina ni recibe
async Task PeekMessage()
{
    QueueClient queueClient = new QueueClient(connectionString, queueName);
    int maxMessages = 10;

    var messages = await queueClient.PeekMessagesAsync(maxMessages);

    foreach (var message in messages.Value)
    {
        var base64String = System.Convert.FromBase64String(message.Body.ToString());
        var stringData = System.Text.Encoding.UTF8.GetString(base64String);
        Order order = JsonConvert.DeserializeObject<Order>(stringData);
        Console.WriteLine("Order Id {0}, and the quantity {1}",order.OrderID,order.Quantity);
    }
}

//recibir los mensajes de la cola y luego eliminarlos
async Task ReceiveMessage()
{
    QueueClient queueClient = new QueueClient(connectionString, queueName);
    int maxMessages = 10;

    var messages = await queueClient.ReceiveMessagesAsync(maxMessages);

    foreach (var message in messages.Value)
    {
        Console.WriteLine(message.Body);
        queueClient.DeleteMessage(message.MessageId,message.PopReceipt);
    }
}

int GetQueueLength()
{
    QueueClient queueClient = new QueueClient(connectionString, queueName);

    if (queueClient.Exists())
    {
        var properties = queueClient.GetProperties();
        return properties.Value.ApproximateMessagesCount;
        
    }
    return 0;
}

//await PeekMessage();

await SendMessage("O1",200);
await SendMessage("O2", 2500);
/*

Console.WriteLine(GetQueueLength());


 
await ReceiveMessage();


await PeekMessage();

await SendMessage("First message");
await SendMessage("Second Message");
*/