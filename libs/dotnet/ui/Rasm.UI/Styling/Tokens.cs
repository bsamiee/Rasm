using System.Text.Json.Serialization;
using SkiaSharp;
using Thinktecture.Text.Json.Serialization;

namespace Rasm.UI.Styling;

// --- [MODELS] --------------------------------------------------------------------------
public sealed record Metrics(float Unit, float Hit, float Icon, float Stroke);

[SmartEnum<string>]
public sealed partial class Space {
    public static readonly Space Zero = new("zero", 0f);
    public static readonly Space Hairline = new("hairline", 0.25f);
    public static readonly Space Tight = new("tight", 0.5f);
    public static readonly Space Small = new("small", 1f);
    public static readonly Space Medium = new("medium", 2f);
    public static readonly Space Large = new("large", 3f);
    public static readonly Space Loose = new("loose", 4f);

    public float Units { get; }

    public float Of(Metrics metrics) => Units * metrics.Unit;
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Radius {
    public static readonly Radius Zero = new("zero", 0f);
    public static readonly Radius Small = new("small", 0.75f);
    public static readonly Radius Medium = new("medium", 1.5f);
    public static readonly Radius Large = new("large", 3f);
    public static readonly Radius Capsule = new("capsule", float.PositiveInfinity);

    public float Units { get; }

    public float Of(Metrics metrics, SKSize size) => float.Min(Units * metrics.Unit, float.Min(size.Width, size.Height) / 2f);
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Elevation {
    public static readonly Elevation Flat = new("flat", 0f, 0f);
    public static readonly Elevation Raised = new("raised", 0.25f, 1f);
    public static readonly Elevation Floating = new("floating", 1f, 3f);
    public static readonly Elevation Overlay = new("overlay", 2f, 6f);

    public float Offset { get; }
    public float Blur { get; }

    public float Extent(Metrics metrics) => ((3f * Blur) + Offset) * metrics.Unit;
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class TypeRole {
    public static readonly TypeRole Body = new("body", 1f, 400, []);
    public static readonly TypeRole Caption = new("caption", 0.85f, 400, []);
    public static readonly TypeRole Heading = new("heading", 1.15f, 600, []);
    public static readonly TypeRole Numeric = new("numeric", 1f, 400, ["tnum"]);

    public float Scale { get; }
    public int Weight { get; }
    public ImmutableArray<string> Features { get; }
}

[SmartEnum<string>(SerializationFrameworks = SerializationFrameworks.None)]
[JsonConverter(typeof(ThinktectureJsonConverterFactory))]
public sealed partial class Duration {
    public static readonly Duration Abrupt = new("abrupt", TimeSpan.Zero);
    public static readonly Duration Brief = new("brief", TimeSpan.FromMilliseconds(50));
    public static readonly Duration Fast = new("fast", TimeSpan.FromMilliseconds(150));
    public static readonly Duration Normal = new("normal", TimeSpan.FromMilliseconds(300));
    public static readonly Duration Slow = new("slow", TimeSpan.FromMilliseconds(500));
    public static readonly Duration Tedious = new("tedious", TimeSpan.FromMilliseconds(1000));
    public static readonly Duration Torpid = new("torpid", TimeSpan.FromMilliseconds(1500));

    public TimeSpan Span { get; }
}
