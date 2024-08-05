namespace WebApiOTLP_Example;

public class Pedido
{
    public long Id { get; set; }
    public string Codigo { get; set; }
    public string NomeCliente { get; set; }
    public List<Produto> Produtos { get; set; }
    public decimal Total => Produtos?.Sum(x => x.Quantidade * x.Valor) ?? 0;
}

public class Produto
{
    public long Id { get; set;}
    public string Nome { get; set;}
    public decimal Valor { get; set; }
    public int Quantidade { get; set; }
}

