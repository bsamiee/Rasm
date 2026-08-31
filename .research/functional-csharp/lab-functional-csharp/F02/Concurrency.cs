namespace Lab.F02;

internal static class StringExt {
    public static string ToSentenceCase(this string value) =>
        char.ToUpperInvariant(value[0]) + string.Concat(value[1..].Select(char.ToLowerInvariant));
}

internal sealed class ListFormatter {
    private int counter;

    private string PrependCounter(string value) =>
        string.Create(CultureInfo.InvariantCulture, $"{++counter}. {value}");

    public Seq<string> Format(Seq<string> items) =>
        items
            .Map(StringExt.ToSentenceCase)
            .Map(PrependCounter);
}

internal static class Formatting {
    public static Seq<string> Format(Seq<string> items) =>
        items
            .Map(StringExt.ToSentenceCase)
            .Zip(toSeq(Range(1, items.Count)), static (item, index) => string.Create(CultureInfo.InvariantCulture, $"{index}. {item}"));
}
