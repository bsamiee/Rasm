using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using CommunityToolkit.HighPerformance;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using Rasm.Imaging.ColorManagement;
using Rasm.Imaging.Pixels;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LightingMood {
    public static readonly LightingMood HighKey = new();
    public static readonly LightingMood Neutral = new();
    public static readonly LightingMood LowKey = new();
}

public sealed record PaletteEntry(Vector3 Color, FramePosition Position, float Share);

public sealed record LightingReading(
    Vector3 KeyColor, Vector3 FillColor, Vector3 AmbientColor,
    float Ratio, float Range, float MeteredExposure,
    Vector2 Direction, Option<float> Hardness, float Split, Option<float> Backlight, Seq<PaletteEntry> Palette) {
    private static readonly (float HighKeyExposure, float HighKeyRatio, float LowKeyExposure, float LowKeyRatio) Thresholds =
        (Metered(0.45f), float.Log2(4f), Metered(0.22f), float.Log2(8f));

    public LightingMood Mood =>
        MeteredExposure < Thresholds.HighKeyExposure && Ratio < Thresholds.HighKeyRatio ? LightingMood.HighKey
        : MeteredExposure > Thresholds.LowKeyExposure || Ratio > Thresholds.LowKeyRatio ? LightingMood.LowKey
        : LightingMood.Neutral;

    public Option<(PaletteEntry Key, PaletteEntry Accent)> Gels =>
        Palette.Take(2) switch {
            [var first, var second] => Some(Vector2.Dot(new Vector2(first.Position.X - second.Position.X, second.Position.Y - first.Position.Y), Direction) >= 0f ? (first, second) : (second, first)),
            _ => None,
        };

    private static float Metered(float encodedMean) {
        Vector4 mean = new(new Vector3(encodedMean), 1f);
        TransferCurve.Srgb.Decode(new Span<Vector4>(ref mean), Nits.ReferenceWhite);
        return float.Log2(Exposure.MiddleGrey / mean.X);
    }
}

public sealed record DistantLight(Vector3 Direction, Vector3 Color, float Stops);

[SmartEnum]
public sealed partial class Gel {
    public static readonly Gel Warm = new(new Vector3(1f, 0.78f, 0.5f));
    public static readonly Gel DeepWarm = new(new Vector3(1f, 0.58f, 0.28f));
    public static readonly Gel Neutral = new(Vector3.One);
    public static readonly Gel Cool = new(new Vector3(0.6f, 0.72f, 1f));
    public static readonly Gel Sky = new(new Vector3(0.5f, 0.68f, 1f));
    public static readonly Gel Magenta = new(new Vector3(1f, 0.3f, 0.85f));
    public static readonly Gel Cyan = new(new Vector3(0.2f, 0.85f, 0.95f));
    public static readonly Gel Pink = new(new Vector3(1f, 0.45f, 0.7f));
    public static readonly Gel Blue = new(new Vector3(0.3f, 0.5f, 1f));
    public static readonly Gel Green = new(new Vector3(0.45f, 0.95f, 0.5f));
    public static readonly Gel Fire = new(new Vector3(1f, 0.5f, 0.2f));
    public static readonly Gel Teal = new(new Vector3(0.2f, 0.72f, 0.72f));

    public Vector3 Color { get; }
}

public sealed record Blob(Vector2 Center, float Sharpness, float Intensity, Vector3 Color);

public sealed record ReferenceScene {
    internal ReferenceScene() { }

    public float Base { get; internal init; } = 0.08f;
    public Vector2 Gradient { get; internal init; }
    public Vector3 Tint { get; internal init; } = Vector3.One;
    public Seq<Blob> Blobs { get; internal init; }
    public float Rim { get; internal init; }
    public Vector3 RimColor { get; internal init; } = Vector3.One;
    public float Vignette { get; internal init; }
    public float Stripes { get; internal init; }
    public int Dapples { get; internal init; }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReferenceFamily {
    public static readonly ReferenceFamily Sky = new("sky");
    public static readonly ReferenceFamily Studio = new("studio");
    public static readonly ReferenceFamily Portrait = new("portrait");
    public static readonly ReferenceFamily Cinematic = new("cinematic");
    public static readonly ReferenceFamily Environment = new("environment");
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReferencePreset {
    public static readonly ReferencePreset GoldenHour = new("golden-hour", ReferenceFamily.Sky, new() {
        Base = 0.2f, Gradient = new(0.9f, 0.1f), Tint = Gel.Warm.Color, Blobs = [new(new(0.7f, 0.3f), 1.2f, 0.7f, Gel.Warm.Color)],
    });
    public static readonly ReferencePreset Sunrise = new("sunrise", ReferenceFamily.Sky, new() {
        Base = 0.16f, Gradient = new(0.5f, 0.2f), Tint = new(1f, 0.7f, 0.45f), Blobs = [new(new(-0.6f, -0.2f), 1.4f, 0.8f, new(1f, 0.65f, 0.4f))],
    });
    public static readonly ReferencePreset Sunset = new("sunset", ReferenceFamily.Sky, new() {
        Base = 0.18f, Gradient = new(0.6f, 0f), Tint = Gel.DeepWarm.Color, Blobs = [new(new(0.6f, -0.25f), 1.3f, 0.9f, Gel.DeepWarm.Color)], Vignette = 0.3f,
    });
    public static readonly ReferencePreset BlueHour = new("blue-hour", ReferenceFamily.Sky, new() { Base = 0.12f, Gradient = new(0f, 0.6f), Tint = Gel.Cool.Color });
    public static readonly ReferencePreset Twilight = new("twilight", ReferenceFamily.Sky, new() {
        Base = 0.1f, Gradient = new(0f, 0.5f), Tint = new(0.7f, 0.6f, 1f), Blobs = [new(new(0f, 0.8f), 1f, 0.4f, Gel.Magenta.Color)],
    });
    public static readonly ReferencePreset Midday = new("midday", ReferenceFamily.Sky, new() {
        Base = 0.4f, Gradient = new(0f, 0.5f), Tint = Gel.Neutral.Color, Blobs = [new(new(0.1f, 0.8f), 1.4f, 0.6f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset HarshNoon = new("harsh-noon", ReferenceFamily.Sky, new() {
        Base = 0.25f, Blobs = [new(new(0f, 0.85f), 3f, 1.1f, Gel.Neutral.Color)], Vignette = 0.45f,
    });
    public static readonly ReferencePreset Overcast = new("overcast", ReferenceFamily.Sky, new() { Base = 0.55f, Gradient = new(0f, 0.25f), Tint = new(0.95f, 0.97f, 1f) });
    public static readonly ReferencePreset FoggyMorn = new("foggy-morn", ReferenceFamily.Sky, new() { Base = 0.5f, Gradient = new(0f, 0.3f), Tint = new(0.9f, 0.95f, 1f), Vignette = 0.2f });
    public static readonly ReferencePreset NightSky = new("night-sky", ReferenceFamily.Sky, new() {
        Base = 0.04f, Gradient = new(0f, 0.25f), Tint = Gel.Cool.Color, Blobs = [new(new(0.4f, 0.8f), 6f, 0.5f, new(0.8f, 0.9f, 1f))],
    });
    public static readonly ReferencePreset TopSky = new("top-sky", ReferenceFamily.Sky, new() { Base = 0.08f, Gradient = new(0f, 0.95f), Tint = new(0.85f, 0.92f, 1f) });
    public static readonly ReferencePreset Softbox = new("softbox", ReferenceFamily.Studio, new() { Base = 0.5f, Blobs = [new(new(0f, 0.4f), 0.6f, 0.5f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset SoftboxL = new("softbox-l", ReferenceFamily.Studio, new() { Base = 0.18f, Blobs = [new(new(-0.6f, 0.35f), 0.9f, 0.8f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset SoftboxR = new("softbox-r", ReferenceFamily.Studio, new() { Base = 0.18f, Blobs = [new(new(0.6f, 0.35f), 0.9f, 0.8f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset BeautyDish = new("beauty-dish", ReferenceFamily.Studio, new() {
        Base = 0.2f, Blobs = [new(new(0f, 0.45f), 1.6f, 0.9f, Gel.Neutral.Color)], Rim = 0.25f,
    });
    public static readonly ReferencePreset RingLight = new("ring-light", ReferenceFamily.Studio, new() {
        Base = 0.18f, Blobs = [new(new(0f, 0f), 0.8f, 0.7f, Gel.Neutral.Color)], Rim = 0.7f,
    });
    public static readonly ReferencePreset Clamshell = new("clamshell", ReferenceFamily.Studio, new() {
        Base = 0.25f, Blobs = [new(new(0f, 0.6f), 1.2f, 0.7f, Gel.Neutral.Color), new(new(0f, -0.6f), 1.2f, 0.45f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset Butterfly = new("butterfly", ReferenceFamily.Studio, new() { Base = 0.2f, Blobs = [new(new(0f, 0.55f), 1.4f, 0.85f, new(1f, 0.95f, 0.9f))] });
    public static readonly ReferencePreset BroadLight = new("broad-light", ReferenceFamily.Studio, new() { Base = 0.16f, Blobs = [new(new(-0.45f, 0.25f), 1f, 0.85f, new(1f, 0.96f, 0.9f))] });
    public static readonly ReferencePreset ShortLight = new("short-light", ReferenceFamily.Studio, new() {
        Base = 0.12f, Blobs = [new(new(0.55f, 0.3f), 1.3f, 0.85f, new(1f, 0.96f, 0.9f))], Vignette = 0.2f,
    });
    public static readonly ReferencePreset HighKey = new("high-key", ReferenceFamily.Studio, new() { Base = 0.7f, Blobs = [new(new(0f, 0.5f), 0.7f, 0.4f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset LowKey = new("low-key", ReferenceFamily.Studio, new() {
        Base = 0.03f, Blobs = [new(new(0.45f, 0.2f), 2.2f, 1f, new(1f, 0.97f, 0.92f))], Vignette = 0.5f,
    });
    public static readonly ReferencePreset Rembrandt = new("rembrandt", ReferenceFamily.Portrait, new() { Base = 0.06f, Blobs = [new(new(-0.5f, 0.45f), 1.6f, 1.1f, Gel.Warm.Color)] });
    public static readonly ReferencePreset RembrandtR = new("rembrandt-r", ReferenceFamily.Portrait, new() { Base = 0.06f, Blobs = [new(new(0.5f, 0.45f), 1.6f, 1.1f, Gel.Warm.Color)] });
    public static readonly ReferencePreset Loop = new("loop", ReferenceFamily.Portrait, new() { Base = 0.12f, Blobs = [new(new(0.35f, 0.3f), 1.4f, 0.95f, new(1f, 0.95f, 0.88f))] });
    public static readonly ReferencePreset Split = new("split", ReferenceFamily.Portrait, new() {
        Base = 0.05f, Blobs = [new(new(0.85f, 0.1f), 2f, 1.1f, new(1f, 0.97f, 0.92f))], Vignette = 0.35f,
    });
    public static readonly ReferencePreset UnderLight = new("under-light", ReferenceFamily.Portrait, new() { Base = 0.06f, Blobs = [new(new(0f, -0.7f), 1.6f, 1f, new(1f, 0.95f, 0.9f))] });
    public static readonly ReferencePreset HairLight = new("hair-light", ReferenceFamily.Portrait, new() {
        Base = 0.1f, Blobs = [new(new(0f, 0.5f), 1f, 0.5f, Gel.Neutral.Color)], Rim = 0.9f,
    });
    public static readonly ReferencePreset Noir = new("noir", ReferenceFamily.Cinematic, new() {
        Base = 0.02f, Blobs = [new(new(-0.8f, 0.35f), 2.4f, 1.2f, Gel.Neutral.Color)], Stripes = 6f, Vignette = 0.55f,
    });
    public static readonly ReferencePreset TealOrange = new("teal-orange", ReferenceFamily.Cinematic, new() {
        Base = 0.08f, Blobs = [new(new(0.6f, 0.25f), 1.2f, 0.9f, Gel.DeepWarm.Color), new(new(-0.6f, -0.1f), 1f, 0.6f, Gel.Teal.Color)],
    });
    public static readonly ReferencePreset NeonMc = new("neon-mc", ReferenceFamily.Cinematic, new() {
        Base = 0.04f, Blobs = [new(new(-0.55f, 0.3f), 1.2f, 0.9f, Gel.Magenta.Color), new(new(0.55f, -0.1f), 1.2f, 0.9f, Gel.Cyan.Color)],
    });
    public static readonly ReferencePreset NeonBp = new("neon-bp", ReferenceFamily.Cinematic, new() {
        Base = 0.04f, Blobs = [new(new(-0.5f, 0f), 1.2f, 0.9f, Gel.Blue.Color), new(new(0.55f, 0.3f), 1.2f, 0.9f, Gel.Pink.Color)],
    });
    public static readonly ReferencePreset Candlelight = new("candlelight", ReferenceFamily.Cinematic, new() {
        Base = 0.05f, Blobs = [new(new(0f, -0.1f), 1.8f, 1f, Gel.Fire.Color)], Vignette = 0.5f,
    });
    public static readonly ReferencePreset Firelight = new("firelight", ReferenceFamily.Cinematic, new() {
        Base = 0.06f, Gradient = new(0f, -0.1f), Tint = Gel.Fire.Color, Blobs = [new(new(0f, -0.5f), 1f, 0.9f, Gel.Fire.Color)], Vignette = 0.35f,
    });
    public static readonly ReferencePreset Moonlight = new("moonlight", ReferenceFamily.Cinematic, new() {
        Base = 0.05f, Blobs = [new(new(-0.4f, 0.5f), 1.4f, 0.7f, Gel.Sky.Color)], Vignette = 0.3f,
    });
    public static readonly ReferencePreset MoonlitWin = new("moonlit-win", ReferenceFamily.Cinematic, new() {
        Base = 0.04f, Blobs = [new(new(0.5f, 0.2f), 1f, 0.8f, Gel.Sky.Color)], Stripes = 5f, Vignette = 0.35f,
    });
    public static readonly ReferencePreset HorrorUp = new("horror-up", ReferenceFamily.Cinematic, new() {
        Base = 0.04f, Blobs = [new(new(0f, -0.7f), 1.6f, 1f, Gel.Green.Color)], Vignette = 0.45f,
    });
    public static readonly ReferencePreset Window = new("window", ReferenceFamily.Environment, new() { Base = 0.12f, Blobs = [new(new(0.55f, 0.35f), 0.8f, 0.85f, new(1f, 0.98f, 0.92f))] });
    public static readonly ReferencePreset WindowBlind = new("window-blind", ReferenceFamily.Environment, new() {
        Base = 0.1f, Blobs = [new(new(0.5f, 0.3f), 0.8f, 0.9f, Gel.Warm.Color)], Stripes = 7f,
    });
    public static readonly ReferencePreset Forest = new("forest", ReferenceFamily.Environment, new() { Base = 0.1f, Gradient = new(0f, 0.3f), Tint = Gel.Green.Color, Dapples = 14 });
    public static readonly ReferencePreset Underwater = new("underwater", ReferenceFamily.Environment, new() { Base = 0.12f, Gradient = new(0f, 0.4f), Tint = Gel.Teal.Color, Dapples = 12 });
    public static readonly ReferencePreset SnowBounce = new("snow-bounce", ReferenceFamily.Environment, new() {
        Base = 0.6f, Gradient = new(0f, 0.2f), Tint = new(0.95f, 0.98f, 1f), Blobs = [new(new(0f, -0.5f), 0.8f, 0.3f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset Desert = new("desert", ReferenceFamily.Environment, new() {
        Base = 0.35f, Gradient = new(0.2f, 0.4f), Tint = new(1f, 0.85f, 0.6f), Blobs = [new(new(0.3f, 0.7f), 1.4f, 0.6f, Gel.Warm.Color)],
    });
    public static readonly ReferencePreset StreetLamp = new("street-lamp", ReferenceFamily.Environment, new() {
        Base = 0.03f, Blobs = [new(new(0f, 0.4f), 2f, 1f, new(1f, 0.8f, 0.5f))], Vignette = 0.6f,
    });
    public static readonly ReferencePreset StageSpot = new("stage-spot", ReferenceFamily.Environment, new() {
        Base = 0.02f, Blobs = [new(new(0f, 0.6f), 2.6f, 1.2f, Gel.Neutral.Color)], Vignette = 0.6f,
    });
    public static readonly ReferencePreset ConcertRgb = new("concert-rgb", ReferenceFamily.Environment, new() {
        Base = 0.04f, Blobs = [new(new(-0.6f, 0.4f), 1.4f, 0.8f, Gel.Blue.Color), new(new(0f, 0.5f), 1.4f, 0.8f, Gel.Magenta.Color), new(new(0.6f, 0.4f), 1.4f, 0.8f, Gel.Cyan.Color)],
    });
    public static readonly ReferencePreset SciFi = new("sci-fi", ReferenceFamily.Environment, new() {
        Base = 0.06f, Blobs = [new(new(0.5f, 0f), 1f, 0.7f, Gel.Cyan.Color)], Stripes = 4f, Vignette = 0.3f,
    });

    public ReferenceFamily Family { get; }
    public ReferenceScene Scene { get; }
}

[SmartEnum<string>]
[ValidationError<InvalidToneValue>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ReferenceDistribution {
    public static readonly ReferenceDistribution Balanced = new("balanced", static random => new() {
        Blobs = [.. Enumerable.Range(0, random.Next(1, 4)).Select(_ => new Blob(
            new(random.Uniform(-0.8, 0.8), random.Uniform(-0.7, 0.85)), random.Uniform(0.7, 2.4), random.Uniform(0.5, 1.1), random.Pick(Gel.Items).Color))],
        Base = random.Uniform(0.03, 0.5),
        Gradient = new(random.Uniform(0, 0.6), random.Uniform(0, 0.6)),
        Tint = random.Pick(Gel.Items).Color,
        Rim = random.Chance(0.4) ? random.Uniform(0, 1.2) : 0f,
        Vignette = random.Chance(0.5) ? random.Uniform(0, 0.5) : 0f,
        Stripes = random.Pick([0f, 0f, 0f, 5f, 7f]),
        Dapples = random.Pick([0, 0, 0, 12]),
    });
    public static readonly ReferenceDistribution Vivid = new("vivid", static random => {
        (Vector3 Color, Vector2 Slot)[] gels = [
            (new(1.4f, 0.12f, 0.85f), new(-0.62f, 0.18f)), (new(0.15f, 0.75f, 1.35f), new(0.62f, 0.18f)), (new(1.35f, 0.55f, 0.05f), new(0f, 0.58f)),
            (new(0.25f, 1.05f, 0.35f), new(-0.38f, -0.42f)), (new(0.75f, 0.25f, 1.35f), new(0.38f, -0.42f))];
        return new() {
            Blobs = [.. random.Pick(random.Pick<int[][]>([[[0, 1, 3], [0, 1, 4], [0, 2, 4], [1, 2, 4]], [[0, 1, 2, 3], [0, 1, 3, 4], [0, 2, 3, 4]], [[0, 1, 2, 3, 4]]]))
                .Select(gel => new Blob(
                    gels[gel].Slot + new Vector2(random.Uniform(-0.08, 0.08), random.Uniform(-0.08, 0.08)), random.Uniform(1.3, 2.1), random.Uniform(0.85, 1.45), gels[gel].Color))],
            Base = random.Uniform(0.03, 0.1),
            Gradient = new(random.Uniform(0, 0.15), random.Uniform(0, 0.15)),
            Tint = new(0.42f, 0.46f, 0.52f),
            Rim = random.Chance(0.25) ? random.Uniform(0, 0.5) : 0f,
            Vignette = random.Uniform(0.15, 0.45),
        };
    });
    public static readonly ReferenceDistribution Soft = new("soft", static random => new() {
        Blobs = [.. Enumerable.Range(0, random.Next(1, 3)).Select(_ => new Blob(
            new(random.Uniform(-0.45, 0.45), random.Uniform(-0.35, 0.55)), random.Uniform(0.55, 1), random.Uniform(0.35, 0.65),
            random.Pick<Vector3>([new(1f, 0.96f, 0.92f), new(0.94f, 0.96f, 1f), new(0.98f, 0.95f, 0.9f)])))],
        Base = random.Uniform(0.32, 0.55),
        Gradient = new(random.Uniform(0, 0.25), random.Uniform(0, 0.25)),
        Tint = random.Pick<Vector3>([new(0.97f, 0.95f, 0.92f), new(0.93f, 0.95f, 0.98f)]),
        Vignette = random.Chance(0.3) ? random.Uniform(0, 0.2) : 0f,
    });

    public ReferenceScene Rolled(int seed) => Roll(new Xoshiro256StarStar(seed, threadSafe: false));

    [UseDelegateFromConstructor]
    private partial ReferenceScene Roll(Xoshiro256StarStar random);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Lighting {
    extension(Xoshiro256StarStar random) {
        internal float Uniform(double low, double high) => (float)ContinuousUniform.Sample(random, low, high);

        internal bool Chance(double probability) => Bernoulli.Sample(random, probability) == 1;

        internal T Pick<T>(IReadOnlyList<T> items) => items[random.Next(0, items.Count)];
    }

    public static Fin<LightingReading> Read(PixelFrame frame, Vector3 luminance) {
        PixelFrame analysis = frame.Downscaled(PixelExtent.Analysis);
        (int width, int height) = (analysis.Size.Width, analysis.Size.Height);
        float Luma(Vector3 color) => float.Max(Vector3.Dot(luminance, color), LuminanceSamples.Floor);
        return (LuminanceSamples.Sorted(analysis, analysis.Window, Luma, (key, pixel) => (Key: key, Index: (pixel.Y * width) + pixel.X)), Metering.Default.Metered(analysis, luminance))
            .Apply(Measured)
            .As();

        LightingReading Measured((float Key, int Index)[] samples, float stops) {
            const int sectors = 12;
            const float chromatic = 0.18f;
            LuminanceSamples ranks = new([.. samples.Select(static sample => sample.Key)]);
            float At(float rank) => ranks.At(Percentile.Create(rank));
            (float shadows, float low, float high, float highlights) = (At(0.15f), At(0.35f), At(0.6f), At(0.85f));
            float[] logs = [.. Enumerable.Repeat(float.NaN, width * height)];
            foreach ((float level, int index) in samples)
                logs[index] = float.Log2(level);
            ReadOnlySpan2D<float> plane = new(logs, height, width);
            ReadOnlySpan<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(analysis.Block);
            (Vector4 Light, Vector2 Moment)[] hues = new (Vector4, Vector2)[sectors];
            (Vector4 shadow, Vector4 middle, Vector4 bright, Vector4 halves, Vector4 halved) = (Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero, Vector4.Zero);
            (Vector2 moment, Vector2 rings, Vector2 ringed, float steepest) = (Vector2.Zero, Vector2.Zero, Vector2.Zero, float.NaN);
            foreach ((float level, int index) in samples) {
                (int row, int column) = int.DivRem(index, width);
                (Vector4 color, Vector2 point) = (new(pixels[index].AsVector3(), 1f), Position(column, row, width, height));
                Vector4 side = Vector4.ConditionalSelect(Vector4.GreaterThan(new Vector4(point.X, -point.X, point.Y, -point.Y), Vector4.Zero), Vector4.One, Vector4.Zero);
                Vector2 ring = Vector2.LessThanAll(Vector2.Abs(point), new Vector2(0.5f)) ? Vector2.UnitY : Vector2.UnitX;
                (float peak, float trough) = (float.Max(color.X, float.Max(color.Y, color.Z)), float.Min(color.X, float.Min(color.Y, color.Z)));
                int hue = ((int)float.Floor((float.Atan2(float.Sqrt(3f) * (color.Y - color.Z), (2f * color.X) - color.Y - color.Z) * sectors / float.Tau) + 0.5f) + sectors) % sectors;
                float weight = peak - trough > chromatic * peak ? level : 0f;
                (shadow, middle, bright) = (shadow + (level <= shadows ? color : Vector4.Zero), middle + (level >= low && level <= high ? color : Vector4.Zero), bright + (level >= highlights ? color : Vector4.Zero));
                (moment, halves, halved, rings, ringed) = (moment + (level * point), halves + (level * side), halved + side, rings + (level * ring), ringed + ring);
                (steepest, hues[hue]) = (float.MaxNumber(steepest, Slope(plane, column, row).Length()), (hues[hue].Light + (weight * color), hues[hue].Moment + (weight * point)));
            }
            (Vector3 key, Vector3 fill, float range, float mass) = (bright.AsVector3() / bright.W, middle.AsVector3() / middle.W, float.Log2(At(0.95f) / At(0.05f)), rings.X + rings.Y);
            (Vector4 halfMeans, Vector2 ringMeans) = (halves / halved, rings / ringed);
            return new LightingReading(
                key, fill, shadow.AsVector3() / shadow.W,
                float.Log2(Luma(key) / Luma(fill)), range, stops,
                moment / mass,
                Some(steepest * int.Max(width, height)).Filter(float.IsFinite),
                Saturated(new(Contrast(halfMeans.X, halfMeans.Y), Contrast(halfMeans.Z, halfMeans.W))).Length() * float.Min(1f, range / float.Log2(6f)),
                Vector2.GreaterThanAll(ringed, Vector2.Zero) ? Some(float.Log2(ringMeans.X / ringMeans.Y)) : None,
                toSeq(hues
                    .Where(static hue => hue.Light.W > 0f)
                    .Select(hue => (hue.Moment / hue.Light.W) switch {
                        var center => new PaletteEntry(
                            hue.Light.AsVector3() / hue.Light.W, new FramePosition(FrameAxis.Create((center.X + 1f) / 2f), FrameAxis.Create((1f - center.Y) / 2f)), hue.Light.W / mass),
                    })
                    .OrderByDescending(static entry => entry.Share)));
        }
    }

    public static PixelFrame Sphere(Seq<DistantLight> lights, PixelExtent size) {
        (Vector3 Direction, Vector3 Radiance)[] sources = [.. lights.Map(static light => (light.Direction, float.Exp2(light.Stops) * light.Color))];
        float radius = int.Min(size.Width, size.Height) / 2f;
        return new PixelFrame(Point.Empty, size, size, block => {
            Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < pixels.Length; index++) {
                (int row, int column) = int.DivRem(index, size.Width);
                Vector2 disc = new((column + 0.5f - (size.Width / 2f)) / radius, ((size.Height / 2f) - row - 0.5f) / radius);
                float distance = disc.Length();
                Vector2 rim = distance > 1f ? disc / distance : disc;
                Vector3 normal = new(rim, float.Sqrt(float.Max(1f - rim.LengthSquared(), 0f)));
                Vector3 light = Vector3.Zero;
                foreach ((Vector3 direction, Vector3 radiance) in sources)
                    light += radiance * float.Max(Vector3.Dot(normal, direction), 0f);
                float coverage = float.Clamp(((1f - distance) * radius) + 0.5f, 0f, 1f);
                pixels[index] = coverage > 0f ? new(light, coverage) : Vector4.Zero;
            }
        });
    }

    public static PixelFrame Reference(ReferenceScene scene, PixelExtent size, int seed) {
        Xoshiro256StarStar random = new(seed, threadSafe: false);
        Blob[] blobs = [.. scene.Blobs, .. Enumerable.Range(0, scene.Dapples).Select(_ =>
            new Blob(new(random.Uniform(-1, 1), random.Uniform(-1, 1)), random.Uniform(8, 22), random.Uniform(0.3, 0.8), scene.Tint))];
        return new PixelFrame(Point.Empty, size, size, block => {
            Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < pixels.Length; index++) {
                (int row, int column) = int.DivRem(index, size.Width);
                Vector2 point = Position(column, row, size.Width, size.Height);
                Vector3 light = (scene.Base + float.Max(((point.X + 1f) * scene.Gradient.X / 2f) + ((point.Y + 1f) * scene.Gradient.Y / 2f), 0f)) * scene.Tint;
                foreach (Blob blob in blobs)
                    light += float.Exp(-Vector2.DistanceSquared(point, blob.Center) * blob.Sharpness) * blob.Intensity * blob.Color;
                float banded = scene.Stripes > 0f && float.SinPi(point.Y * scene.Stripes) <= 0f ? 0.45f : 1f;
                Vector3 rimmed = (light * banded) + (float.Pow(float.Clamp(point.Length() - 0.5f, 0f, 1f), 1.4f) * scene.Rim * scene.RimColor);
                pixels[index] = new(rimmed * (1f - (float.Min(point.LengthSquared(), 1f) * scene.Vignette)), 1f);
            }
        });
    }

    private static Vector2 Position(int column, int row, int width, int height) =>
        new((((2f * column) + 1f) / width) - 1f, 1f - (((2f * row) + 1f) / height));

    private static Vector2 Saturated(Vector2 cue) => cue * float.Min(1f, 1f / cue.Length());

    private static float Contrast(float high, float low) => high + low is > 0f and var sum ? (high - low) / sum : 0f;

    private static Vector2 Slope(ReadOnlySpan2D<float> plane, int x, int y) =>
        (int.Max(x - 1, 0), int.Min(x + 1, plane.Width - 1), int.Max(y - 1, 0), int.Min(y + 1, plane.Height - 1)) switch {
            var (left, right, top, bottom) => new(
                right > left ? (plane[y, right] - plane[y, left]) / (right - left) : 0f,
                bottom > top ? (plane[top, x] - plane[bottom, x]) / (bottom - top) : 0f),
        };
}
