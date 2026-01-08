using System;

namespace LogisticaService
{
    public class PagamentoCriadoEvent
    {
        public int PedidoId { get; set; }
        public decimal Valor { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Data { get; set; }
    }
}
