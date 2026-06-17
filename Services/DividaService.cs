using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Vendinha.Core.Data;
using Vendinha.Core.Models;

namespace Vendinha.Core.Services
{
    public class DividaService
    {
        private readonly VendinhaDbContext context;

        public DividaService(VendinhaDbContext context)
        {
            this.context = context;
        }

        public bool Validar(Divida divida, out List<ValidationResult> erros)
        {
            var validation = new ValidationContext(divida);
            erros = new List<ValidationResult>();
            Validator.TryValidateObject(divida, validation, erros, true);

            if (!context.Clientes.Any(e => e.Id == divida.ClienteId))
            {
                erros.Add(new ValidationResult("Cliente não encontrado"));
            }

            var jaTemDividaAberta = context.Dividas.Any(e =>
                e.ClienteId == divida.ClienteId &&
                !e.Paga &&
                e.Id != divida.Id
            );

            if (!divida.Paga && jaTemDividaAberta)
            {
                erros.Add(new ValidationResult("Cliente já possui dívida em aberto"));
            }

            return erros.Count == 0;
        }

        public bool Criar(Divida divida, out List<ValidationResult> erros)
        {
            divida.DataCriacao = DateTime.Now;
            divida.DataPagamento = divida.Paga ? DateTime.Now : null;

            if (!Validar(divida, out erros))
            {
                return false;
            }

            context.Dividas.Add(divida);
            context.SaveChanges();
            return true;
        }

        public List<Divida> ListarPorCliente(int clienteId)
        {
            return context.Dividas
                .Where(e => e.ClienteId == clienteId)
                .OrderBy(e => e.Paga)
                .ThenByDescending(e => e.DataCriacao)
                .ToList();
        }

        public Divida? BuscarPorId(int id)
        {
            return context.Dividas
                .Include(e => e.Cliente)
                .FirstOrDefault(e => e.Id == id);
        }

        public bool Atualizar(Divida divida, out List<ValidationResult> erros)
        {
            var atual = context.Dividas.FirstOrDefault(e => e.Id == divida.Id);

            if (atual == null)
            {
                erros = new List<ValidationResult>
                {
                    new ValidationResult("Dívida não encontrada")
                };
                return false;
            }

            atual.Valor = divida.Valor;
            atual.Paga = divida.Paga;
            atual.DataPagamento = divida.Paga ? DateTime.Now : null;

            if (!Validar(atual, out erros))
            {
                return false;
            }

            context.Dividas.Update(atual);
            context.SaveChanges();
            return true;
        }

        public bool Pagar(int id)
        {
            var divida = context.Dividas.FirstOrDefault(e => e.Id == id);

            if (divida == null)
            {
                return false;
            }

            divida.Paga = true;
            divida.DataPagamento = DateTime.Now;
            context.Dividas.Update(divida);
            context.SaveChanges();
            return true;
        }

        public bool Excluir(int id)
        {
            var divida = context.Dividas.FirstOrDefault(e => e.Id == id);

            if (divida == null)
            {
                return false;
            }

            context.Dividas.Remove(divida);
            context.SaveChanges();
            return true;
        }
    }
}
