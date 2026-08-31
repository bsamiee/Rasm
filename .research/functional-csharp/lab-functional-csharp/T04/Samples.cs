namespace Lab.T04;

internal static class Samples {
    public static Fin<Unit> Run() =>
        ShippingMethodSample()
            .Bind(static _ => FileLocationSample())
            .Bind(static _ => DualSample())
            .Bind(static _ => RegionSample())
            .Bind(static _ => TextOrNumberSample())
            .Bind(static _ => SlugSample());

    private static Fin<Unit> ShippingMethodSample() =>
        Check(
            nameof(ShippingMethodSample),
            ("Wire uses the slug", string.Equals(ShippingMethods.Wire(ShippingMethod.Express), "\"express\"", StringComparison.Ordinal)),
            ("Read uses the slug", ReferenceEquals(ShippingMethods.Read("\"express\""), ShippingMethod.Express)),
            ("Read rejects a number token", Try.lift(static () => ShippingMethods.Read("2")).Run().Match(Succ: static _ => false, Fail: static e => e.IsType<Exceptional>())),
            ("Read rejects an unknown slug", Try.lift(static () => ShippingMethods.Read("\"bogus\"")).Run().IsFail),
            ("Read accepts null", ShippingMethods.Read("null") is null),
            ("Accepts known", ShippingMethods.Accepts("standard")),
            ("Rejects unknown", !ShippingMethods.Accepts("nope")),
            ("Parsed", ReferenceEquals(ShippingMethods.Parsed("express"), ShippingMethod.Express)),
            ("Parse throws FormatException", Try.lift(static () => ShippingMethods.Parsed("nope")).Run().Match(Succ: static _ => false, Fail: static e => e.Exception.Match(Some: static x => x is FormatException, None: static () => false))),
            ("ISpanParsable present", typeof(ISpanParsable<ShippingMethod>).IsAssignableFrom(typeof(ShippingMethod))),
            ("Span Parse reads the int key", ShippingMethod.TryParse("2".AsSpan(), provider: null, out ShippingMethod? bySpan) && ReferenceEquals(bySpan, ShippingMethod.Express)),
            ("Span Parse rejects the slug", Try.lift(static () => ShippingMethod.Parse("express".AsSpan(), provider: null)).Run().Match(Succ: static _ => false, Fail: static e => e.Exception.Match(Some: static x => x is FormatException, None: static () => false))));

    private static Fin<Unit> FileLocationSample() =>
        Check(
            nameof(FileLocationSample),
            ("Round trip trims", string.Equals(System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<FileLocation>("\" store :doc-1\"")), "\"store:doc-1\"", StringComparison.Ordinal)),
            ("Null passes through", System.Text.Json.JsonSerializer.Deserialize<FileLocation>("null") is null),
            ("Empty string rejected", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<FileLocation>("\"\"")).Run().IsFail),
            ("Missing store rejected", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<FileLocation>("\":x\"")).Run().IsFail),
            ("Parse empty throws FormatException", Try.lift(static () => FileLocation.Parse("", provider: null)).Run().Match(Succ: static _ => false, Fail: static e => e.Exception.Match(Some: static x => x is FormatException, None: static () => false))),
            ("Parse splits", string.Equals(FileLocation.Parse("s:u", provider: null).Path, "u", StringComparison.Ordinal)),
            ("ToValue joins", string.Equals(FileLocation.Create("s", "u").ToValue(), "s:u", StringComparison.Ordinal)),
            ("IParsable present", typeof(IParsable<FileLocation>).IsAssignableFrom(typeof(FileLocation))),
            ("ISpanParsable absent", !typeof(FileLocation).GetInterfaces().Any(static i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISpanParsable<>))));

    private static Fin<Unit> DualSample() =>
        Check(
            nameof(DualSample),
            ("JSON writes the string", string.Equals(System.Text.Json.JsonSerializer.Serialize(Dual.Two), "\"two\"", StringComparison.Ordinal)),
            ("JSON reads the string", ReferenceEquals(System.Text.Json.JsonSerializer.Deserialize<Dual>("\"one\""), Dual.One)),
            ("Explicit ToValue yields the char", ((IConvertible<char>)Dual.Two).ToValue() == '2'),
            ("Char Validate", Dual.Validate('1', provider: null, out Dual? fromChar) is null && ReferenceEquals(fromChar, Dual.One)),
            ("Char Validate rejects", Dual.Validate('9', provider: null, out Dual? unknown) is not null && unknown is null),
            ("Char factory interface", typeof(IObjectFactory<Dual, char, ValidationError>).IsAssignableFrom(typeof(Dual))));

    private static Fin<Unit> RegionSample() {
        const int Long = 300;
        string longValue = new('x', Long);
        return Check(
            nameof(RegionSample),
            ("Known value", string.Equals(System.Text.Json.JsonSerializer.Deserialize<Region>("\"eu\"").ToString(), "eu", StringComparison.Ordinal)),
            ("Round trip", string.Equals(System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<Region>("\"us\"")), "\"us\"", StringComparison.Ordinal)),
            ("Pool path above 128", System.Text.Json.JsonSerializer.Deserialize<Region>($"\"{longValue}\"").ToString().Length == Long),
            ("Escaped value", string.Equals(System.Text.Json.JsonSerializer.Deserialize<Region>("\"a\\u0062c\"").ToString(), "abc", StringComparison.Ordinal)),
            ("Number token rejected", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<Region>("42")).Run().IsFail),
            ("Span Parse", string.Equals(Region.Parse("eu".AsSpan(), provider: null).ToString(), "eu", StringComparison.Ordinal)),
            ("ISpanParsable present", typeof(ISpanParsable<Region>).IsAssignableFrom(typeof(Region))),
            ("Span factory interface", typeof(IObjectFactory<Region, ReadOnlySpan<char>, ValidationError>).IsAssignableFrom(typeof(Region))));
    }

    private static Fin<Unit> TextOrNumberSample() =>
        Check(
            nameof(TextOrNumberSample),
            ("Number case", string.Equals(System.Text.Json.JsonSerializer.Serialize((TextOrNumber)42), "\"number:42\"", StringComparison.Ordinal)),
            ("Text case", string.Equals(System.Text.Json.JsonSerializer.Serialize((TextOrNumber)"hi"), "\"text:hi\"", StringComparison.Ordinal)),
            ("Reads number", System.Text.Json.JsonSerializer.Deserialize<TextOrNumber>("\"number:7\"") is { IsNumber: true, AsNumber: 7 }),
            ("Rejects unknown prefix", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<TextOrNumber>("\"zzz\"")).Run().IsFail),
            ("Parse text", string.Equals(TextOrNumber.Parse("text:a", provider: null).AsText, "a", StringComparison.Ordinal)));

    private static Fin<Unit> SlugSample() =>
        Check(
            nameof(SlugSample),
            ("Round trip", string.Equals(System.Text.Json.JsonSerializer.Serialize(System.Text.Json.JsonSerializer.Deserialize<Slug>("\"Hello\"")), "\"Hello\"", StringComparison.Ordinal)),
            ("Null reference", System.Text.Json.JsonSerializer.Deserialize<Slug>("null") is null),
            ("Empty rejected", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<Slug>("\"\"")).Run().IsFail),
            ("Space rejected", Try.lift(static () => System.Text.Json.JsonSerializer.Deserialize<Slug>("\"has space\"")).Run().IsFail),
            ("IParsable present", typeof(IParsable<Slug>).IsAssignableFrom(typeof(Slug))),
            ("TryParse null", !Slug.TryParse(null, provider: null, out _)),
            ("Converter attribute", typeof(Slug).GetCustomAttributes(typeof(System.Text.Json.Serialization.JsonConverterAttribute), inherit: false).Length == 1));

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
