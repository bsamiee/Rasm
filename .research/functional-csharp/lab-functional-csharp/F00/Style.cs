namespace Lab.F00;

internal sealed record Person(string Name, int Age);

internal static class Style {
    public static Fin<Unit> Probe() {
        Seq<int> numbers = [1, 2, 3];
        Seq<int> doubled = numbers.Map(static x => x * 2);
        int total = doubled.Fold(0, static (acc, x) => acc + x);
        string rendered = string.Create(CultureInfo.InvariantCulture, $"total {total}");
        Option<int> query =
            from a in parseInt("40")
            from b in parseInt("3")
            select a + b;
        Option<int> oneLine = from a in parseInt("1") from b in parseInt("2") select a + b;
        Console.WriteLine(rendered);
        return Verify.Check(
            nameof(Style),
            ("total == 12", total == 12),
            ("query == Some(43)", query == Some(43)),
            ("oneLine == Some(3)", oneLine == Some(3)));
    }
}
