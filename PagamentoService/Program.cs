using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using PagamentoService; 

var factory = new ConnectionFactory { HostName = "localhost" };
using var connection = await factory.CreateConnectionAsync();
using var channel = await connection.CreateChannelAsync();

await channel.ExchangeDeclareAsync(exchange: "pagamentos_exchange", type: ExchangeType.Fanout);

var novoPagamento = new PagamentoCriadoEvent
{
    PedidoId = new Random().Next(1000, 9999),
    Valor = 250.00m,
    Status = "Aprovado",
    Data = DateTime.Now
};

var json = JsonSerializer.Serialize(novoPagamento);
var body = Encoding.UTF8.GetBytes(json);

await channel.BasicPublishAsync(exchange: "pagamentos_exchange",
                                routingKey: string.Empty,
                                body: body);

Console.WriteLine($" [v] PagamentoService: Evento do Pedido {novoPagamento.PedidoId} enviado com sucesso!");