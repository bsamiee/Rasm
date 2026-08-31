namespace Lab.F06;

internal static class Adapters {
    public static readonly Func<decimal, decimal, decimal> Subtract = static (left, right) => left - right;

    public static readonly Func<decimal, decimal, decimal> SubtractFrom = flip(Subtract);
}

internal static class Factories {
    public static Func<int, bool> IsMod(int divisor) => value => value % divisor == 0;

    public static Seq<int> MultiplesOfThree => toSeq(Range(1, 20)).Filter(IsMod(3));
}
