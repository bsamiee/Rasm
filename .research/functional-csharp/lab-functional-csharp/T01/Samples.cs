namespace Lab.T01;

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
internal sealed partial class CurrencyCode {
    public static readonly CurrencyCode Eur = new("EUR");
    public static readonly CurrencyCode Usd = new("USD");
}

internal static class StatusJson {
    public static string Write(OrderStatus status) => System.Text.Json.JsonSerializer.Serialize(status);

    public static Fin<OrderStatus> Read(string json) =>
        System.Text.Json.JsonSerializer.Deserialize<OrderStatus>(json) is { } status ? status : Error.New("missing status");
}

internal static class Escalation {
    public static bool Urgent(Priority priority) => priority > 1;

    public static string Padded(Priority priority) => priority.ToString("000", CultureInfo.InvariantCulture);
}

internal sealed class CaptureSink : ILogEventSink {
    private readonly List<string> _lines = [];

    public IReadOnlyList<string> Lines => _lines;

    public void Emit(Serilog.Events.LogEvent logEvent) => _lines.Add(logEvent.RenderMessage(CultureInfo.InvariantCulture));
}

internal static class Samples {
    public static Fin<Unit> Run() =>
        DeclarationSample()
            .Bind(static _ => SurfaceSample())
            .Bind(static _ => MatchingSample())
            .Bind(static _ => BehaviorSample())
            .Bind(static _ => ComparersSample())
            .Bind(static _ => OperatorsSample())
            .Bind(static _ => GenericSample())
            .Bind(static _ => JsonSample())
            .Bind(static _ => LoggingSample());

    private static Fin<Unit> DeclarationSample() =>
        Check(
            nameof(DeclarationSample),
            ("key normalized", string.Equals(Country.Germany.Key, "DE", StringComparison.Ordinal)),
            ("base argument", Country.Germany.NumericCode == 276),
            ("own member", string.Equals(Country.France.Name, "France", StringComparison.Ordinal)),
            ("items in declaration order", Country.Items.SequenceEqual([Country.Germany, Country.France])));

    private static Fin<Unit> SurfaceSample() =>
        Check(
            nameof(SurfaceSample),
            ("Find hit", Lookup.Find<OrderStatus, string>("PENDING") == Some(OrderStatus.Pending)),
            ("Find miss", Lookup.Find<OrderStatus, string>("nope").IsNone),
            ("Admit hit", Lookup.Admit<Tier, string, UnknownTier>("plus") == Pure(Tier.Plus)),
            ("Admit miss", Lookup.Admit<Tier, string, UnknownTier>("nope").Match(Succ: static _ => false, Fail: static e => e.IsType<UnknownTier>() && e.HasCode(1))),
            ("Admit message", Try.lift(static () => Tier.Get("nope").Key).Run().Match(Succ: static _ => false, Fail: static e => e.Message.Contains("'nope'", StringComparison.Ordinal))),
            ("Get null returns null", OrderStatus.Get(key: null) is null),
            ("Validate null reports unknown", OrderStatus.Validate(key: null, provider: null, out _) is not null),
            ("implicit to key", string.Equals(OrderStatus.Shipped, "Shipped", StringComparison.Ordinal)),
            ("explicit from key", (OrderStatus)"Shipped" == OrderStatus.Shipped),
            ("Parse", OrderStatus.Parse("Delivered", provider: null) == OrderStatus.Delivered),
            ("TryParse span", OrderStatus.TryParse("Delivered".AsSpan(), provider: null, out OrderStatus? delivered) && delivered == OrderStatus.Delivered),
            ("Validate span", OrderStatus.Validate("Shipped".AsSpan(), provider: null, out OrderStatus? shipped) is null && shipped == OrderStatus.Shipped),
            ("hash from key comparer", OrderStatus.Pending.GetHashCode() == StringComparer.OrdinalIgnoreCase.GetHashCode("Pending")));

    private static Fin<Unit> MatchingSample() {
        List<string> log = [];
        Matching.RecordColdChain(ProductType.Housewares, log);
        int unhandled = log.Count;
        Matching.RecordAll(ProductType.Housewares, log);
        Matching.RecordAll(ProductType.Groceries, log);
        return Check(
            nameof(MatchingSample),
            ("Switch state", string.Equals(Matching.Label(ProductType.Groceries, 2.5m), "cold chain, 2.5 kg", StringComparison.Ordinal)),
            ("MapPartially default", string.Equals(Matching.Handling(ProductType.Housewares), "standard", StringComparison.Ordinal)),
            ("SwitchPartially without default is a no-op", unhandled == 0),
            ("SwitchPartially default receives the item", log.SequenceEqual(["Housewares", "cold chain"], StringComparer.Ordinal)));
    }

    private static Fin<Unit> BehaviorSample() =>
        Check(
            nameof(BehaviorSample),
            ("per-item data", ShippingMethod.Express.Price(2m) == 17.49m),
            ("delegate Up", MoneyRoundingStrategy.Up.Round(1.005m) == 1.01m),
            ("delegate Nearest", MoneyRoundingStrategy.Nearest.Round(1.005m) == 1.00m),
            ("keyless items", MoneyRoundingStrategy.Items.Count == 2),
            ("keyless ToString", string.Equals(MoneyRoundingStrategy.Up.ToString(), "Up", StringComparison.Ordinal)),
            ("lazy transition allowed", OrderStatus.Pending.CanTransitionTo(OrderStatus.Shipped)),
            ("lazy transition denied", !OrderStatus.Pending.CanTransitionTo(OrderStatus.Delivered)),
            ("derived payload type", NotificationChannel.Email.PayloadType == typeof(string) && NotificationChannel.Sms.PayloadType == typeof(int)),
            ("derived items", NotificationChannel.Items.Select(static c => c.Key).SequenceEqual(["email", "sms"], StringComparer.Ordinal)),
            ("derived lookup", NotificationChannel.Get("SMS") == NotificationChannel.Sms));

    private static Fin<Unit> ComparersSample() =>
        Check(
            nameof(ComparersSample),
            ("ordinal miss", !CurrencyCode.TryGet("eur", out _)),
            ("ordinal hit", CurrencyCode.TryGet("EUR", out _)),
            ("ordinal order", CurrencyCode.Eur < CurrencyCode.Usd),
            ("custom comparer trims", Ticker.TryGet(" MSFT ", out Ticker? msft) && msft == Ticker.Msft),
            ("custom comparer is case sensitive", !Ticker.TryGet("msft", out _)),
            ("span lookup through alternate comparer", Ticker.TryGet(" AAPL".AsSpan(), out Ticker? aapl) && aapl == Ticker.Aapl),
            ("custom order", Ticker.Aapl < Ticker.Msft));

    private static Fin<Unit> OperatorsSample() =>
        Check(
            nameof(OperatorsSample),
            ("key type overload", Escalation.Urgent(Priority.High)),
            ("key type overload low", !Escalation.Urgent(Priority.Low)),
            ("item comparison", Priority.High > Priority.Low),
            ("key equality", Priority.High == 3),
            ("IFormattable", string.Equals(Escalation.Padded(Priority.Low), "001", StringComparison.Ordinal)),
            ("CompareTo", Priority.Low.CompareTo(Priority.High) < 0));

    private static Fin<Unit> GenericSample() =>
        Check(
            nameof(GenericSample),
            ("int key", Metric<int>.Parse("1", provider: null) == Metric<int>.Humidity),
            ("double key", Metric<double>.Get(0.0) == Metric<double>.Temperature),
            ("items", Metric<decimal>.Items.Count == 2));

    private static Fin<Unit> JsonSample() =>
        Check(
            nameof(JsonSample),
            ("write key", string.Equals(StatusJson.Write(OrderStatus.Pending), "\"Pending\"", StringComparison.Ordinal)),
            ("read key ignoring case", StatusJson.Read("\"shipped\"") == Pure(OrderStatus.Shipped)),
            ("discriminator reads circle", ReadShape(ShapeKind.Circle, "{\"Radius\":2}") == new Circle(2)),
            ("discriminator reads square", ReadShape(ShapeKind.Get("Square"), "{\"Side\":3}") == new Square(3)));

    private static Shape? ReadShape(ShapeKind kind, string json) {
        System.Text.Json.Utf8JsonReader reader = new(System.Text.Encoding.UTF8.GetBytes(json));
        return kind.Read(ref reader, System.Text.Json.JsonSerializerOptions.Default);
    }

    private static Fin<Unit> LoggingSample() {
        CaptureSink sink = new();
        Logging.Emit(OrderStatus.Pending, sink);
        return sink.Lines.Count == 1 && string.Equals(sink.Lines[0], "status \"Pending\"", StringComparison.Ordinal)
            ? unit
            : Error.New($"{nameof(LoggingSample)}: {string.Join(" | ", sink.Lines)}");
    }

    private static Fin<Unit> Check(string sample, params ReadOnlySpan<(string Name, bool Passed)> checks) {
        foreach ((string name, bool passed) in checks)
            if (!passed) return Error.New($"{sample}: {name}");
        return unit;
    }
}
