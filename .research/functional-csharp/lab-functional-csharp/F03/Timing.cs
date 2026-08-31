namespace Lab.F03;

internal static class Timing {
    public static Unit Time(string operation, Action action) => Time<Unit>(operation, fun(action));

    public static T Time<T>(string operation, Func<T> body) {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        T result = body();
        stopwatch.Stop();
        Console.WriteLine(string.Create(CultureInfo.InvariantCulture, $"{operation} took {stopwatch.ElapsedMilliseconds}ms"));
        return result;
    }
}
