using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vendinha.Core.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string NomeCompleto { get; set; } = string.Empty;

        [Required]
        [StringLength(11, MinimumLength = 11)]
        public string Cpf { get; set; } = string.Empty;

        [Required]
        public DateTime DataNascimento { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public List<Divida> Dividas { get; set; } = new List<Divida>();

        [NotMapped]
        public int Idade
        {
            get
            {
                var hoje = DateTime.Today;
                var idade = hoje.Year - DataNascimento.Year;
                if (DataNascimento.Date > hoje.AddYears(-idade))
                {
                    idade--;
                }
                return idade;
            }
        }

        [NotMapped]
        public decimal TotalDividas
        {
            get
            {
                return Dividas.Where(d => !d.Paga).Sum(d => d.Valor);
            }
        }
    }
}
