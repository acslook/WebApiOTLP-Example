using System.Collections.Concurrent;
using Bogus;

namespace WebApiOTLP_Example;

public class GeradorPedido
{
    private int _produtoId = 1;
    private int _pedidoId = 1;
    private readonly ConcurrentBag<Pedido> _pedidos = new ConcurrentBag<Pedido>();

    public Pedido GerarNovoPedido()
    {
        var fakeProduto = new Faker<Produto>()
                .RuleFor(p => p.Id, f => _produtoId++)
                .RuleFor(p => p.Nome, f => f.Commerce.ProductName())
                .RuleFor(p => p.Quantidade, f => f.Random.Int(1, 5))
                .RuleFor(p => p.Valor, f => f.Random.Decimal(10, 5000));

        var fakePedido = new Faker<Pedido>()
            .RuleFor(p => p.Id, f => _pedidoId++)
            .RuleFor(p => p.Codigo, f => f.Random.AlphaNumeric(10))
            .RuleFor(p => p.NomeCliente, f => f.Person.UserName)
            .RuleFor(p => p.Produtos, f => fakeProduto.Generate(3));

        var pedido = fakePedido.Generate();
        _pedidos.Add(pedido);

        return pedido;
    }

    public List<Pedido> GetPedidos()
    {
        return _pedidos.ToList();
    }
}
