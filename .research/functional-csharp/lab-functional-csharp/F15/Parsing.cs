namespace Lab.F15;

internal static partial class Parsing {
    public static Try<System.Text.Json.JsonDocument> Parse(string json) => Try.lift(() => System.Text.Json.JsonDocument.Parse(json));

    public static Try<Uri> CreateUri(string value) => Try.lift(() => new Uri(value));
}

internal static partial class Parsing {
    public static Try<Uri> ExtractUri(string json) =>
        from document in Parse(json)
        let uriString = document.RootElement.GetProperty("Uri").ToString()
        from uri in CreateUri(uriString)
        select uri;
}
