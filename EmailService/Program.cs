using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using EmailService; // Certifique-se que o namespace no arquivo .cs está correto

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "pagamentos_exchange", type: ExchangeType.Fanout);

var queueDeclareResult = await channel.QueueDeclareAsync();
var queueName = queueDeclareResult.QueueName;
await channel.QueueBindAsync(queue: queueName, exchange: "pagamentos_exchange", routingKey: string.Empty);

Console.WriteLine(" [EMAIL] Aguardando para enviar comprovantes...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var json = Encoding.UTF8.GetString(ea.Body.ToArray());
    var evento = JsonSerializer.Deserialize<PagamentoCriadoEvent>(json);

    if (evento != null)
    {
        Console.WriteLine($"\n[E-MAIL] Enviando recibo do pedido #{evento.PedidoId} para o cliente...");
        Console.WriteLine($"[E-MAIL] Valor confirmado: R$ {evento.Valor}.");
        Console.WriteLine("---------------------------------------------------");
    }
    await Task.Yield();
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);
Console.ReadLine();