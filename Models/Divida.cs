using System.ComponentModel.DataAnnotations;

namespace Vendinha.Core.Models
{
    public class Divida
    {
        public int Id { get; set; }

        [Required]
        public int ClienteId { get; set; }

        public Cliente Cliente { get; set; } = null!;

        [Range(0.01, 999999)]
        public decimal Valor { get; set; }

        public bool Paga { get; set; }

        public DateTime DataCriacao { get; set; }

        public DateTime? DataPagamento { get; set; }
    }
}
