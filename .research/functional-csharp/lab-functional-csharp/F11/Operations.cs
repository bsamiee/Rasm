namespace Lab.F11;

internal static class Operations {
    public static Fin<string> Describe(double x, double y) =>
        Calculator.Calculate(x, y).Map(static root => string.Create(CultureInfo.InvariantCulture, $"{root}"));

    public static Fin<double> FourthRoot(double x, double y) =>
        Calculator.Calculate(x, y).Bind(static root => Calculator.Calculate(root, 1));

    public static Fin<double> Root(double value) =>
        from v in Pure(value).ToFin()
        from _ in guard(v >= 0, Error.New("value cannot be negative"))
        select Math.Sqrt(v);
}
