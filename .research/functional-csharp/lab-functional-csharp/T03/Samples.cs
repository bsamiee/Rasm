namespace Lab.T03;

[Union<string, int>]
internal readonly partial struct TextOrNumberStruct;

internal sealed class CapturingSink : ILogEventSink {
    private readonly List<Serilog.Events.LogEvent> _events = [];

    public IReadOnlyList<Serilog.Events.LogEvent> Events => _events;

    public void Emit(Serilog.Events.LogEvent logEvent) => _events.Add(logEvent);
}

internal static class Samples {
    public static Fin<Unit> Run() {
        Seq<(string Name, bool Ok)> checks = AdHoc() + Settings() + Stateless() + Generic() + Regular() + Matching() + UseCases() + Frameworks();
        Option<(string Name, bool Ok)> failed = checks.Find(static check => !check.Ok);
        return failed.Match(
            Some: static check => Fin.Fail<Unit>(Error.New($"check failed: {check.Name}")),
            None: static () => Fin.Succ(unit));
    }

    private static bool Same(string? left, string right) => string.Equals(left, right, StringComparison.Ordinal);

    private static bool Throws<TException>(Func<object?> action) where TException : Exception =>
        Try.lift(action).Run().Match(Succ: static _ => false, Fail: static error => error.Exception.Match(Some: static exception => exception is TException, None: static () => false));

    private static Seq<(string Name, bool Ok)> AdHoc() {
        TextNumberOrFlag text = "text";
        TextNumberOrFlag number = 42;
        return [
            ("IsString", text.IsString && !number.IsString),
            ("AsString", Same(text.AsString, "text")),
            ("Cast", (int)number == 42),
            ("Value", number.Value is 42),
            ("Same ignore case", text == (TextNumberOrFlag)"TEXT"),
            ("ToString", Same(number.ToString(), "42")),
            ("Hash", number.GetHashCode() == 42.GetHashCode()),
            ("AsString throws", Throws<InvalidOperationException>(() => number.AsString)),
            ("Cast throws", Throws<InvalidOperationException>(() => (string)number)),
        ];
    }

    private static Seq<(string Name, bool Ok)> Settings() {
        LabeledTextOrNumber labeled = new("seven", "label");
        NamedTextOrNumber named = "  HELLO  ";
        return [
            ("Labeled", Same(labeled.AsText, "seven") && Same(labeled.Label, "label") && !labeled.IsNumber),
            ("Normalize", Same(named.AsText, "hello") && Same(named.ToString(), "hello")),
        ];
    }

    private static Seq<(string Name, bool Ok)> Stateless() {
        ApiResponse notFound = new NotFoundError();
        ApiResponse success = new SuccessResponse("data");
        TextOrNumberStruct[] uninitializedPair = new TextOrNumberStruct[2];
        TextOrNumberStruct uninitialized = uninitializedPair[0];
        TextOrNumberStruct other = uninitializedPair[1];
        TextOrNumberStruct initialized = 1;
        MaybeInt absent = (new MaybeInt[1])[0];
        MaybeInt some = 42;
        return [
            ("Map", notFound.Map(success: 200, notFound: 404) == 404 && success.Map(success: 200, notFound: 404) == 200),
            ("Stateless equality", notFound == new ApiResponse(new NotFoundError()) && notFound != success),
            ("Stateless accessor", notFound.AsNotFound == default && notFound.Value is NotFoundError),
            ("Default Value throws", Throws<InvalidOperationException>(() => uninitialized.Value)),
            ("Default ToString throws", Throws<InvalidOperationException>(uninitialized.ToString)),
            ("Default GetHashCode throws", Throws<InvalidOperationException>(() => uninitialized.GetHashCode())),
            ("Default Switch throws", Throws<InvalidOperationException>(() => uninitialized.Switch<string>(@string: static s => s, int32: static i => i.ToString(CultureInfo.InvariantCulture)))),
            ("Default Map throws", Throws<InvalidOperationException>(() => uninitialized.Map(@string: "s", int32: "i"))),
            ("Default equality throws", Throws<InvalidOperationException>(() => uninitialized == other)),
            ("Default equality with initialized is false", uninitialized != initialized),
            ("Default IsString false", !uninitialized.IsString),
            ("MaybeInt default", absent.IsAbsent && !absent.IsInt32 && absent == MaybeInt.None && absent.GetHashCode() == MaybeInt.None.GetHashCode() && absent.Value is Absent),
            ("MaybeInt some", some.IsInt32 && some.AsInt32 == 42),
        ];
    }

    private static Seq<(string Name, bool Ok)> Generic() {
        Result<int> success = 42;
        Result<int> error = "boom";
        return [
            ("CreateT", success == Result<int>.CreateT(42) && success.IsT && success.AsT == 42),
            ("Ctor", new Result<int>(7).AsT == 7),
            ("CreateString", error == Result<int>.CreateString("boom") && error.IsString),
        ];
    }

    private static Seq<(string Name, bool Ok)> Regular() {
        OrderState processing = new OrderState.Processing(DateTime.UnixEpoch);
        OrderState fromString = "me";
        OrderState fromDate = DateTime.UnixEpoch;
        return [
            ("Ship", OrderTransitions.Ship(processing, new ShipRequest(DateTime.UnixEpoch, "TRK", CanShip: true)) is OrderState.Shipped),
            ("Ship denied", OrderTransitions.Ship(processing, new ShipRequest(DateTime.UnixEpoch, "TRK", CanShip: false)) is OrderState.Processing),
            ("Ship placed", OrderTransitions.Ship(new OrderState.Placed("me"), new ShipRequest(DateTime.UnixEpoch, "TRK", CanShip: true)) is OrderState.Placed),
            ("CanCancel", processing.CanCancel() && !new OrderState.Shipped(DateTime.UnixEpoch, "TRK").CanCancel()),
            ("Implicit string", fromString is OrderState.Placed && fromDate is OrderState.Processing),
            ("Nested names", RequestOutcomes.StatusCode(new RequestOutcome.Failure.NotFound()) == 404 && RequestOutcomes.StatusCode(new RequestOutcome.Success()) == 200),
            ("StopAt", Same(RequestOutcomes.Group(new RequestOutcome.Failure.Unauthorized()), "failed") && Same(RequestOutcomes.Group(new RequestOutcome.Success()), "ok")),
            ("Nested own names", new RequestOutcome.Failure.Unauthorized().Map(notFound: 404, unauthorized: 401) == 401),
        ];
    }

    private static Seq<(string Name, bool Ok)> Matching() {
        TextNumberOrFlag flag = true;
        TextNumberOrFlag text = "abc";
        List<string> sink = [];
        flag.SwitchPartially(sink, @string: static (list, s) => list.Add(s));
        int unhandled = sink.Count;
        text.SwitchPartially(sink, @string: static (list, s) => list.Add(s));
        return [
            ("MapPartially", Same(PartialMatching.Label(flag), "other") && Same(PartialMatching.Label(text), "text")),
            ("SwitchPartially", PartialMatching.Length(flag) == 0 && PartialMatching.Length(text) == 3),
            ("Void partial no-op", unhandled == 0 && sink.Count == 1),
        ];
    }

    private static Seq<(string Name, bool Ok)> UseCases() {
        PartiallyKnownDate exact = new DateOnly(2024, 3, 15);
        PartiallyKnownDate fromYear = 1980;
        string json = System.Text.Json.JsonSerializer.Serialize(exact);
        PartiallyKnownDate? back = System.Text.Json.JsonSerializer.Deserialize<PartiallyKnownDate>(json);
        Jurisdiction.Country de = Jurisdiction.Country.Create("de");
        Jurisdiction germany = de;
        Jurisdiction europe = Jurisdiction.Continent.Europe;
        return [
            ("Implicit int", fromYear is PartiallyKnownDate.YearOnly),
            ("Json", Same(json, """{"$type":"Date","Month":3,"Day":15,"Year":2024}""") && back == exact),
            ("Jurisdiction switch", Same(germany.Switch(country: static _ => "country", unknown: static _ => "unknown", continent: static _ => "continent"), "country")),
            ("Jurisdiction continent", Same(europe.Switch(country: static _ => "country", unknown: static _ => "unknown", continent: static c => c.Key), "Europe")),
            ("Unknown instance", Jurisdiction.Unknown.Instance.Switch(country: static _ => false, unknown: static _ => true, continent: static _ => false)),
            ("Country equality", de == Jurisdiction.Country.Create("DE")),
            ("YearMonth case", new PartiallyKnownDate.YearMonth(1980, 6).Year == 1980),
        ];
    }

    private static Seq<(string Name, bool Ok)> Frameworks() {
        TextOrNumberSerializable text = "hello";
        TextOrNumberSerializable number = 5;
        string json = System.Text.Json.JsonSerializer.Serialize(text);
        TextOrNumberSerializable? back = System.Text.Json.JsonSerializer.Deserialize<TextOrNumberSerializable>(json);
        CapturingSink sink = new();
        TextNumberOrFlag content = 42;
        TextOrNumberStruct uninitialized = (new TextOrNumberStruct[1])[0];
        using (Logger logger = new LoggerConfiguration().Destructure.UsingThinktectureRuntimeExtensions().WriteTo.Sink(sink).CreateLogger()) {
            logger.Information("Content {@Content} in state {@State}", content, new OrderState.Shipped(DateTime.UnixEpoch, "TRK"));
            logger.Information("Uninitialized {@Union}", uninitialized);
        }
        Serilog.Events.LogEvent logged = sink.Events[0];
        Serilog.Events.LogEvent captured = sink.Events[1];
        return [
            ("ToValue", Same(text.ToValue(), "Text|hello") && Same(number.ToValue(), "Number|5")),
            ("Json round trip", Same(json, "\"Text|hello\"") && back == text),
            ("Json invalid", Throws<System.Text.Json.JsonException>(static () => System.Text.Json.JsonSerializer.Deserialize<TextOrNumberSerializable>("\"Bogus\""))),
            ("Json null", System.Text.Json.JsonSerializer.Deserialize<TextOrNumberSerializable>("null") is null),
            ("Parsable", TextOrNumberSerializable.TryParse("Number|7", CultureInfo.InvariantCulture, out TextOrNumberSerializable? parsed) && parsed?.AsNumber == 7),
            ("TryParse null", !TextOrNumberSerializable.TryParse(null, CultureInfo.InvariantCulture, out _)),
            ("Parse invalid", Throws<FormatException>(static () => TextOrNumberSerializable.Parse("bad", CultureInfo.InvariantCulture))),
            ("Serilog ad hoc scalar", logged.Properties["Content"] is Serilog.Events.ScalarValue { Value: 42 }),
            ("Serilog regular structure", logged.Properties["State"] is Serilog.Events.StructureValue { TypeTag: string tag } && Same(tag, "Shipped")),
            ("Serilog uninitialized placeholder", captured.Properties["Union"] is Serilog.Events.ScalarValue { Value: string placeholder } && placeholder.StartsWith("Capturing the property value threw an exception: InvalidOperationException", StringComparison.Ordinal)),
        ];
    }
}
