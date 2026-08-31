namespace Lab.F07;

internal sealed record Book(string Title, string Author, string PublicationDate);

internal static class Parsing {
    public const bool SkipHeader = true;
    public const bool KeepHeader = false;
    public static readonly Func<bool, string, string, string, Seq<Book>> ParseBooks =
        static (skipHeader, lineBreak, fieldDelimiter, content) =>
            toSeq(content.Split(lineBreak))
                .Skip(skipHeader ? 1 : 0)
                .Map(line => line.Split(fieldDelimiter))
                .Map(static fields => new Book(fields[0], fields[1], fields[2]));
}

internal static class Partials {
    public static readonly Func<string, Seq<Book>> ParseLinuxComma = par(Parsing.ParseBooks, Parsing.KeepHeader, "\n", ",");
    public static readonly Func<string, string, Seq<Book>> ParseWindows = par(Parsing.ParseBooks, Parsing.SkipHeader, Environment.NewLine);
    public static readonly Func<string, Seq<Book>> ParseWindowsComma = par(ParseWindows, ",");
}

internal static class Families {
    public static readonly Func<bool, Func<string, Func<string, Func<string, Seq<Book>>>>> Curried = curry(Parsing.ParseBooks);
    public static readonly Func<string, Func<string, Func<string, Seq<Book>>>> ParseWithHeader = Curried(true);
    public static readonly Func<string, Func<string, Seq<Book>>> ParseWindowsWithHeader = ParseWithHeader(Environment.NewLine);
    public static readonly Func<string, Seq<Book>> ParseWindowsComma = ParseWindowsWithHeader(",");
    public static readonly Func<string, Seq<Book>> ParseWindowsPipe = ParseWindowsWithHeader("|");
}
