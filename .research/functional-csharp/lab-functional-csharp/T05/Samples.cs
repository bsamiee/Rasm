using Serilog.Events;
using Serilog.Formatting.Display;

namespace Lab.T05;

internal sealed class Capture : ILogEventSink {
    private static readonly MessageTemplateTextFormatter Formatter = new("{Message:j}");
    private readonly List<string> _lines = [];

    public Seq<string> Lines => toSeq(_lines);

    public void Emit(LogEvent logEvent) {
        using StringWriter writer = new(CultureInfo.InvariantCulture);
        Formatter.Format(logEvent, writer);
        _lines.Add(writer.ToString());
    }

    public Logger Logger(TypesToRenderAsString renderAsString, int maximumDepth) =>
        new LoggerConfiguration()
            .WriteTo.Sink(this)
            .Destructure.UsingThinktectureRuntimeExtensions(renderAsString)
            .Destructure.ToMaximumDepth(maximumDepth)
            .CreateLogger();
}

internal static class Samples {
    public static Fin<Unit> Run() =>
        RegistrationSample()
            .Bind(static _ => FamiliesSample())
            .Bind(static _ => DepthSample())
            .Bind(static _ => RenderingSample())
            .Bind(static _ => CaveatsSample());

    private static Fin<Unit> RegistrationSample() {
        using Logger logger = Logging.Create(TypesToRenderAsString.None);
        using Logger bounded = Logging.CreateBounded(3);
        Families.Log(logger);
        Rendering.Log(bounded);
        return Check(
            nameof(RegistrationSample),
            ("flags", (int)Enum.Parse<TypesToRenderAsString>("All") == 7),
            ("flags attribute", typeof(TypesToRenderAsString).IsDefined(typeof(FlagsAttribute), inherit: false)));
    }

    private static Fin<Unit> FamiliesSample() {
        Capture capture = new();
        using (Logger logger = capture.Logger(TypesToRenderAsString.None, 10))
            Families.Log(logger);
        return Check(
            nameof(FamiliesSample),
            ("lines", capture.Lines == Seq(
                "keyed smart enum: \"Paid\"",
                "simple value object: 99.95",
                "union holding string: \"pending\"",
                "union holding value object: 99.95",
                "union holding smart enum: \"Paid\"",
                "union holding complex value object: {\"Lower\":1,\"Upper\":10,\"$type\":\"Boundary\"}",
                "record with members: {\"Status\":\"Paid\",\"Total\":99.95,\"$type\":\"Order\"}",
                "complex value object: {\"Lower\":1,\"Upper\":10,\"$type\":\"Boundary\"}",
                "keyless smart enum: {\"Name\":\"email\",\"$type\":\"Channel\"}",
                "regular union: {\"Radius\":2.5,\"$type\":\"Circle\"}")));
    }

    private static Fin<Unit> DepthSample() {
        Capture one = new();
        Capture two = new();
        Capture three = new();
        using (Logger logger = one.Logger(TypesToRenderAsString.None, 1))
            Families.Log(logger);
        using (Logger logger = two.Logger(TypesToRenderAsString.None, 2))
            Families.Log(logger);
        using (Logger logger = three.Logger(TypesToRenderAsString.None, 3))
            Families.Log(logger);
        return Check(
            nameof(DepthSample),
            ("depth 1 value object", one.Lines.At(1) == Some("simple value object: null")),
            ("depth 1 record", one.Lines.At(6) == Some("record with members: {\"Status\":null,\"Total\":null,\"$type\":\"Order\"}")),
            ("depth 2 value object", two.Lines.At(1) == Some("simple value object: 99.95")),
            ("depth 2 union", two.Lines.At(3) == Some("union holding value object: null")),
            ("depth 2 record", two.Lines.At(6) == Some("record with members: {\"Status\":null,\"Total\":null,\"$type\":\"Order\"}")),
            ("depth 3 union", three.Lines.At(3) == Some("union holding value object: 99.95")),
            ("depth 3 record", three.Lines.At(6) == Some("record with members: {\"Status\":\"Paid\",\"Total\":99.95,\"$type\":\"Order\"}")));
    }

    private static Fin<Unit> RenderingSample() {
        Capture none = new();
        Capture all = new();
        Capture unions = new();
        using (Logger logger = none.Logger(TypesToRenderAsString.None, 10))
            Rendering.Log(logger);
        using (Logger logger = all.Logger(TypesToRenderAsString.All, 10))
            Rendering.Log(logger);
        using (Logger logger = unions.Logger(TypesToRenderAsString.AdHocUnions, 10))
            Rendering.Log(logger);
        return Check(
            nameof(RenderingSample),
            ("none", none.Lines == Seq(
                "keyed smart enum: \"Paid\"",
                "simple value object: 99.95",
                "union holding value object: 99.95",
                "union holding complex value object: {\"Lower\":1,\"Upper\":10,\"$type\":\"Boundary\"}",
                "value object with SkipToString: 3")),
            ("all", all.Lines == Seq(
                "keyed smart enum: \"Paid\"",
                "simple value object: \"99.95\"",
                "union holding value object: \"99.95\"",
                "union holding complex value object: \"{ Lower = 1, Upper = 10 }\"",
                "value object with SkipToString: \"Lab.T05.Quantity\"")),
            ("unions", unions.Lines == Seq(
                "keyed smart enum: \"Paid\"",
                "simple value object: 99.95",
                "union holding value object: \"99.95\"",
                "union holding complex value object: \"{ Lower = 1, Upper = 10 }\"",
                "value object with SkipToString: 3")));
    }

    private static Fin<Unit> CaveatsSample() {
        Capture capture = new();
        using (Logger logger = capture.Logger(TypesToRenderAsString.None, 10))
            Caveats.Log(logger);
        return Check(
            nameof(CaveatsSample),
            ("lines", capture.Lines == Seq(
                "value object with object factory: 42",
                "default struct union: \"Capturing the property value threw an exception: InvalidOperationException\"",
                "smart enum without @: \"Paid\"",
                "value object without @: \"99.95\"",
                "record without @: \"Order { Status = Paid, Total = 99.95 }\"")),
            ("ToValue", string.Equals(Percentage.Create(42).ToValue(), "42%", StringComparison.Ordinal)),
            ("SkipToString", typeof(Quantity).GetMethod(nameof(ToString), Type.EmptyTypes)?.DeclaringType != typeof(Quantity)));
    }

    private static Fin<Unit> Check(string sample, params (string Name, bool Ok)[] checks) {
        Seq<string> failed = toSeq(checks).Choose(static check => check.Ok ? Option<string>.None : Some(check.Name));
        return guard(failed.IsEmpty, Error.New($"{sample}: {string.Join(" | ", failed)}")).ToFin();
    }
}
