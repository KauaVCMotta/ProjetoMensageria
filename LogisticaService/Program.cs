using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using LogisticaService;

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "pagamentos_exchange", type: ExchangeType.Fanout);

var queueDeclareResult = await channel.QueueDeclareAsync();
var queueName = queueDeclareResult.QueueName;
await channel.QueueBindAsync(queue: queueName, exchange: "pagamentos_exchange", routingKey: string.Empty);

Console.WriteLine(" [LOGÍSTICA] Aguardando liberação de carga...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var evento = JsonSerializer.Deserialize<PagamentoCriadoEvent>(json);

    if (evento != null)
    {
        Console.WriteLine($"\n[ESTOQUE] Pedido #{evento.PedidoId} APROVADO.");
        Console.WriteLine($"[ESTOQUE] Gerando o codigo de envio e separando produtos...");
        Console.WriteLine("---------------------------------------------------");
    }
    await Task.Yield();
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
Console.ReadLine();