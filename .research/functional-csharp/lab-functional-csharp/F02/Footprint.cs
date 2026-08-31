namespace Lab.F02;

internal static class Greeting {
    public static Eff<RT, Unit> Greet<RT>() where RT : Has<Eff<RT>, ConsoleIO> =>
        from _ in Console<RT>.writeLine("Enter your name:")
        from name in Console<RT>.readLine
        from __ in Console<RT>.writeLine(GreetingFor(name))
        select unit;

    public static string GreetingFor(string name) => $"Hello {name}";
}

internal sealed record Product(string Name, decimal Price);

internal sealed record OrderLine(Product Product, int Quantity);

internal sealed record Order(Seq<OrderLine> OrderLines);

internal static class Orders {
    public static (decimal Total, Seq<OrderLine> LinesToDelete) RecomputeTotal(Order order) =>
        (
            order.OrderLines.Fold(0m, static (total, line) => total + (line.Product.Price * line.Quantity)),
            order.OrderLines.Filter(static line => line.Quantity == 0)
        );
}
