namespace Lab.F11;

internal static class Calculator {
    public static Fin<double> Calculate(double x, double y) =>
        y == 0 ? Error.New("y cannot be 0")
        : x != 0 && Math.Sign(x) != Math.Sign(y) ? Error.New("x / y cannot be negative")
        : Math.Sqrt(x / y);
}
