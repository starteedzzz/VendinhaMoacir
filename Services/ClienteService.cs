using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Vendinha.Core.Data;
using Vendinha.Core.Models;

namespace Vendinha.Core.Services
{
    public class ClienteService
    {
        private readonly VendinhaDbContext context;

        public ClienteService(VendinhaDbContext context)
        {
            this.context = context;
        }

        public bool Validar(Cliente cliente, out List<ValidationResult> erros)
        {
            cliente.Cpf = LimparCpf(cliente.Cpf);
            cliente.NomeCompleto = cliente.NomeCompleto?.Trim();
            cliente.Email = string.IsNullOrWhiteSpace(cliente.Email) ? null : cliente.Email.Trim();

            var validation = new ValidationContext(cliente);
            erros = new List<ValidationResult>();
            Validator.TryValidateObject(cliente, validation, erros, true);

            if (!CpfValido(cliente.Cpf))
            {
                erros.Add(new ValidationResult("CPF inválido"));
            }

            if (cliente.DataNascimento.Date >= DateTime.Today)
            {
                erros.Add(new ValidationResult("Data de nascimento deve ser menor que a data atual"));
            }

            return erros.Count == 0;
        }

        public bool Criar(Cliente cliente, out List<ValidationResult> erros)
        {
            if (!Validar(cliente, out erros))
            {
                return false;
            }

            if (context.Clientes.Any(e => e.Cpf == cliente.Cpf))
            {
                erros.Add(new ValidationResult("Já existe cliente com esse CPF"));
                return false;
            }

            context.Clientes.Add(cliente);
            context.SaveChanges();
            return true;
        }

        public List<Cliente> Listar(string busca = "", int pagina = 1)
        {
            if (pagina < 1)
            {
                pagina = 1;
            }

            var clientes = context.Clientes
                .Include(e => e.Dividas)
                .AsEnumerable();

            if (!string.IsNullOrWhiteSpace(busca))
            {
                clientes = clientes.Where(e => e.NomeCompleto.ToLower().Contains(busca.ToLower()));
            }

            return clientes
                .OrderByDescending(e => e.TotalDividas)
                .ThenBy(e => e.NomeCompleto)
                .Skip((pagina - 1) * 10)
                .Take(10)
                .ToList();
        }

        public Cliente? BuscarPorId(int id)
        {
            return context.Clientes
                .Include(e => e.Dividas)
                .FirstOrDefault(e => e.Id == id);
        }

        public bool Atualizar(Cliente cliente, out List<ValidationResult> erros)
        {
            if (!Validar(cliente, out erros))
            {
                return false;
            }

            var existe = context.Clientes.Any(e => e.Id == cliente.Id);
            if (!existe)
            {
                erros.Add(new ValidationResult("Cliente não encontrado"));
                return false;
            }

            var cpfDuplicado = context.Clientes.Any(e => e.Cpf == cliente.Cpf && e.Id != cliente.Id);
            if (cpfDuplicado)
            {
                erros.Add(new ValidationResult("Já existe cliente com esse CPF"));
                return false;
            }

            context.Clientes.Update(cliente);
            context.SaveChanges();
            return true;
        }

        public bool Excluir(int id)
        {
            var cliente = context.Clientes.FirstOrDefault(e => e.Id == id);

            if (cliente == null)
            {
                return false;
            }

            context.Clientes.Remove(cliente);
            context.SaveChanges();
            return true;
        }

        private string LimparCpf(string cpf)
        {
            if (cpf == null)
            {
                return "";
            }

            return new string(cpf.Where(char.IsDigit).ToArray());
        }

        private bool CpfValido(string cpf)
        {
            if (cpf.Length != 11)
            {
                return false;
            }

            if (cpf.Distinct().Count() == 1)
            {
                return false;
            }

            var soma = 0;
            for (var i = 0; i < 9; i++)
            {
                soma += (cpf[i] - '0') * (10 - i);
            }

            var resto = soma % 11;
            var digito1 = resto < 2 ? 0 : 11 - resto;

            soma = 0;
            for (var i = 0; i < 10; i++)
            {
                soma += (cpf[i] - '0') * (11 - i);
            }

            resto = soma % 11;
            var digito2 = resto < 2 ? 0 : 11 - resto;

            return cpf[9] - '0' == digito1 && cpf[10] - '0' == digito2;
        }
    }
}
