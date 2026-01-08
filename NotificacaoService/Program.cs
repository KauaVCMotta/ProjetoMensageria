using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using NotificacaoService; // Garante que ele enxergue a classe no novo arquivo

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "pagamentos_exchange", type: ExchangeType.Fanout);

var queueDeclareResult = await channel.QueueDeclareAsync();
var queueName = queueDeclareResult.QueueName;

await channel.QueueBindAsync(queue: queueName, exchange: "pagamentos_exchange", routingKey: string.Empty);

Console.WriteLine(" [*] NotificacaoService: Aguardando eventos de pagamento...");

var consumer = new AsyncEventingBasicConsumer(channel);
consumer.ReceivedAsync += async (model, ea) =>
{
    var body = ea.Body.ToArray();
    var json = Encoding.UTF8.GetString(body);

    try
    {
        var evento = JsonSerializer.Deserialize<PagamentoCriadoEvent>(json);

        if (evento != null)
        {
            Console.WriteLine("\n========================================");
            Console.WriteLine($"   NOTIFICAÇÃO RECEBIDA - PEDIDO #{evento.PedidoId}");
            Console.WriteLine("========================================");
            Console.WriteLine($" VALOR: R$ {evento.Valor}");
            Console.WriteLine($" STATUS: {evento.Status}");
            Console.WriteLine($" DATA: {evento.Data:dd/MM/yyyy HH:mm:ss}");
            Console.WriteLine("========================================\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($" [!] Erro ao processar mensagem: {ex.Message}");
    }

    await Task.Yield();
};

await channel.BasicConsumeAsync(queue: queueName, autoAck: true, consumer: consumer);

Console.WriteLine(" Pressione [enter] para sair.");
Console.ReadLine();