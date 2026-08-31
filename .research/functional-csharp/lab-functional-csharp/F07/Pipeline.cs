namespace Lab.F07;

internal static class PipeExtensions {
    public static R Pipe<T, R>(this T value, Func<T, R> function) => function(value);
}

internal static class Temperature {
    private static readonly Func<decimal, decimal, decimal> SubtractBase = static (fixedValue, input) => input - fixedValue;
    private static readonly Func<decimal, decimal, decimal> MultiplyBase = static (fixedValue, input) => input * fixedValue;
    private static readonly Func<decimal, decimal, decimal> DivideBase = static (fixedValue, input) => input / fixedValue;

    public static decimal FahrenheitToCelsius(decimal value) => value.Pipe(par(SubtractBase, 32m)).Pipe(par(MultiplyBase, 5m)).Pipe(par(DivideBase, 9m));
}
