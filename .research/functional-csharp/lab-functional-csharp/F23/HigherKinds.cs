namespace Lab.F23;

internal sealed record Line(string Sku, decimal Price);

internal static class HigherKinds {
    public static K<F, decimal> Prices<F>(K<F, Line> lines) where F : Functor<F> =>
        lines.Map(static line => line.Price);

    public static Option<decimal> OptionPrice(Option<Line> line) => Prices(line).As();

    public static Seq<decimal> SeqPrices(Seq<Line> lines) => Prices(lines).As();
}
