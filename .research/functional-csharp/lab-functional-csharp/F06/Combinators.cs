namespace Lab.F06;

internal static class Piping {
    public static TOut Pipe<TIn, TOut>(this TIn value, Func<TIn, TOut> transform) => transform(value);

    public static string Celsius(decimal fahrenheit) =>
        fahrenheit
            .Pipe(static x => x - 32).Pipe(static x => x * 5).Pipe(static x => x / 9)
            .Pipe(static x => Math.Round(x, 2, MidpointRounding.ToEven))
            .Pipe(static x => string.Create(CultureInfo.InvariantCulture, $"{x} degrees C"));
}

internal static class Forks {
    public static TOut Fork<TIn, TLeft, TRight, TOut>(this TIn value, Func<TIn, TLeft> left, Func<TIn, TRight> right, Func<TLeft, TRight, TOut> join) =>
        join(left(value), right(value));

    public static double Average(Seq<double> values) =>
        values.Fork(static s => s.Fold(0.0, static (total, x) => total + x), static s => s.Count, static (sum, count) => sum / count);

    public static TOut Fork<TIn, TPart, TOut>(this TIn value, Func<Seq<TPart>, TOut> join, Seq<Func<TIn, TPart>> prongs) =>
        join(prongs.Map(prong => prong(value)));
}

internal static class Conversions {
    public static readonly Func<decimal, decimal> FahrenheitToCelsius = static x => (x - 32) * 5 / 9;

    public static readonly Func<decimal, string> Format = static x => string.Create(CultureInfo.InvariantCulture, $"{Math.Round(x, 2, MidpointRounding.ToEven)} degrees");

    public static readonly Func<decimal, string> FormattedConversion = compose(FahrenheitToCelsius, Format);
}

internal static class Observers {
    public static Option<int> Logged(Option<int> value, Action<int> log) => value.Do(log);

    public static Seq<int> Traced(Seq<int> values, Action<int> log) => values.Do(log);
}

internal static class Guards {
    public static IO<Unit> WarnWhenEmpty(int stock, Action<string> notify) =>
        unless(stock > 0, IO.lift(() => notify("out of stock"))).As();

    public static IO<Unit> WarnWhenFull(int stock, Action<string> notify) =>
        when(stock > 100, IO.lift(() => notify("overstocked"))).As();
}
