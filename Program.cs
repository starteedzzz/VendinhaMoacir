using Vendinha.Core.Data;
using Vendinha.Core.Models;
using Vendinha.Core.Services;

Environment.SetEnvironmentVariable(
    "ConnectionStrings__Default",
    "Data Source=vendinha.db"
);

var db = new VendinhaDbContext();
db.Database.EnsureCreated();

var clienteService = new ClienteService(db);
var dividaService = new DividaService(db);

var opcao = "";

while (opcao != "0")
{
    Console.WriteLine("\n=== Vendinha Plena ===");
    Console.WriteLine("1 - Criar cliente");
    Console.WriteLine("2 - Listar clientes");
    Console.WriteLine("3 - Atualizar cliente");
    Console.WriteLine("4 - Excluir cliente");
    Console.WriteLine("5 - Criar dívida");
    Console.WriteLine("6 - Listar dívidas do cliente");
    Console.WriteLine("7 - Atualizar dívida");
    Console.WriteLine("8 - Pagar dívida");
    Console.WriteLine("9 - Excluir dívida");
    Console.WriteLine("0 - Sair");
    Console.Write("Escolha: ");
    opcao = Console.ReadLine();

    if (opcao == "1")
    {
        CriarCliente(clienteService);
    }
    else if (opcao == "2")
    {
        ListarClientes(clienteService);
    }
    else if (opcao == "3")
    {
        AtualizarCliente(clienteService);
    }
    else if (opcao == "4")
    {
        ExcluirCliente(clienteService);
    }
    else if (opcao == "5")
    {
        CriarDivida(dividaService);
    }
    else if (opcao == "6")
    {
        ListarDividas(dividaService);
    }
    else if (opcao == "7")
    {
        AtualizarDivida(dividaService);
    }
    else if (opcao == "8")
    {
        PagarDivida(dividaService);
    }
    else if (opcao == "9")
    {
        ExcluirDivida(dividaService);
    }
}

static void CriarCliente(ClienteService service)
{
    Console.Write("Nome completo: ");
    var nome = Console.ReadLine();

    Console.Write("CPF: ");
    var cpf = Console.ReadLine();

    Console.Write("Data de nascimento (yyyy-mm-dd): ");
    var data = DateTime.Parse(Console.ReadLine() ?? "");

    Console.Write("E-mail: ");
    var email = Console.ReadLine();

    var cliente = new Cliente
    {
        NomeCompleto = nome ?? "",
        Cpf = cpf ?? "",
        DataNascimento = data,
        Email = email
    };

    var sucesso = service.Criar(cliente, out var erros);

    if (sucesso)
    {
        Console.WriteLine("Cliente criado com id {0}", cliente.Id);
    }
    else
    {
        MostrarErros(erros);
    }
}

static void ListarClientes(ClienteService service)
{
    Console.Write("Busca por nome: ");
    var busca = Console.ReadLine();

    Console.Write("Página: ");
    var pagina = int.Parse(Console.ReadLine() ?? "");

    var clientes = service.Listar(busca, pagina);

    foreach (var cliente in clientes)
    {
        Console.WriteLine("#{0} - {1} - CPF: {2} - Idade: {3} - Total aberto: R$ {4}",
            cliente.Id,
            cliente.NomeCompleto,
            cliente.Cpf,
            cliente.Idade,
            cliente.TotalDividas);
    }
}

static void AtualizarCliente(ClienteService service)
{
    Console.Write("Id do cliente: ");
    var id = int.Parse(Console.ReadLine() ?? "");

    var cliente = service.BuscarPorId(id);

    if (cliente == null)
    {
        Console.WriteLine("Cliente não encontrado");
        return;
    }

    Console.Write("Nome completo: ");
    cliente.NomeCompleto = Console.ReadLine() ?? "";

    Console.Write("CPF: ");
    cliente.Cpf = Console.ReadLine() ?? "";

    Console.Write("Data de nascimento (yyyy-mm-dd): ");
    cliente.DataNascimento = DateTime.Parse(Console.ReadLine() ?? "");

    Console.Write("E-mail: ");
    cliente.Email = Console.ReadLine();

    var sucesso = service.Atualizar(cliente, out var erros);

    if (sucesso)
    {
        Console.WriteLine("Cliente atualizado");
    }
    else
    {
        MostrarErros(erros);
    }
}

static void ExcluirCliente(ClienteService service)
{
    Console.Write("Id do cliente: ");
    var id = int.Parse(Console.ReadLine() ?? "");

    if (service.Excluir(id))
    {
        Console.WriteLine("Cliente excluído");
    }
    else
    {
        Console.WriteLine("Cliente não encontrado");
    }
}

static void CriarDivida(DividaService service)
{
    Console.Write("Id do cliente: ");
    var clienteId = int.Parse(Console.ReadLine() ?? "");

    Console.Write("Valor: ");
    var valor = decimal.Parse(Console.ReadLine() ?? "");

    var divida = new Divida
    {
        ClienteId = clienteId,
        Valor = valor,
        Paga = false
    };

    var sucesso = service.Criar(divida, out var erros);

    if (sucesso)
    {
        Console.WriteLine("Dívida criada com id {0}", divida.Id);
    }
    else
    {
        MostrarErros(erros);
    }
}

static void ListarDividas(DividaService service)
{
    Console.Write("Id do cliente: ");
    var clienteId = int.Parse(Console.ReadLine() ?? "");

    var dividas = service.ListarPorCliente(clienteId);

    foreach (var divida in dividas)
    {
        Console.WriteLine("#{0} - Valor: R$ {1} - Paga: {2} - Criada em: {3} - Paga em: {4}",
            divida.Id,
            divida.Valor,
            divida.Paga ? "sim" : "não",
            divida.DataCriacao,
            divida.DataPagamento);
    }
}

static void AtualizarDivida(DividaService service)
{
    Console.Write("Id da dívida: ");
    var id = int.Parse(Console.ReadLine() ?? "");

    var divida = service.BuscarPorId(id);

    if (divida == null)
    {
        Console.WriteLine("Dívida não encontrada");
        return;
    }

    Console.Write("Valor: ");
    divida.Valor = decimal.Parse(Console.ReadLine() ?? "");

    Console.Write("Está paga? s/n: ");
    divida.Paga = (Console.ReadLine() ?? "").ToLower() == "s";

    var sucesso = service.Atualizar(divida, out var erros);

    if (sucesso)
    {
        Console.WriteLine("Dívida atualizada");
    }
    else
    {
        MostrarErros(erros);
    }
}

static void PagarDivida(DividaService service)
{
    Console.Write("Id da dívida: ");
    var id = int.Parse(Console.ReadLine() ?? "");

    if (service.Pagar(id))
    {
        Console.WriteLine("Dívida paga");
    }
    else
    {
        Console.WriteLine("Dívida não encontrada");
    }
}

static void ExcluirDivida(DividaService service)
{
    Console.Write("Id da dívida: ");
    var id = int.Parse(Console.ReadLine() ?? "");

    if (service.Excluir(id))
    {
        Console.WriteLine("Dívida excluída");
    }
    else
    {
        Console.WriteLine("Dívida não encontrada");
    }
}

static void MostrarErros(List<System.ComponentModel.DataAnnotations.ValidationResult> erros)
{
    Console.WriteLine("Erro:");
    foreach (var erro in erros)
    {
        Console.WriteLine("- {0}", erro.ErrorMessage);
    }
}
