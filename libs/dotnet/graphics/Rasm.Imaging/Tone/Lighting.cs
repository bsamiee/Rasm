using System.Drawing;
using System.Globalization;
using System.Numerics;
using System.Runtime.InteropServices;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Wacton.Unicolour;

namespace Rasm.Imaging.Tone;

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum]
public sealed partial class LightingMood {
    public static readonly LightingMood HighKey = new();
    public static readonly LightingMood Neutral = new();
    public static readonly LightingMood LowKey = new();
}

public sealed record PaletteEntry(Vector3 Color, FramePosition Position, float Share) {
    public double Chroma =>
        float.Max(Color.X, float.Max(Color.Y, Color.Z)) switch {
            > 0f and var peak => ((double)peak - float.Min(Color.X, float.Min(Color.Y, Color.Z))) / peak,
            _ => 0f,
        };
}

public sealed record LightingReading(
    Gamut Gamut, Vector3 KeyColor, Vector3 FillColor, Vector3 AmbientColor,
    float Ratio, float Range, float MeteredExposure, double MeanLuminance,
    Vector2 Direction, float Hardness, float Split, bool Backlit,
    Option<(ColorTemperature Temperature, Tint Tint)> KeyWhite, Seq<PaletteEntry> Palette) {
    private static readonly (float HighKeyMean, float HighKeyRatio, float LowKeyMean, float LowKeyRatio, float LowKeySplit) Thresholds =
        (Linear(0.45f), float.Log2(4f), Linear(0.22f), float.Log2(8f), 0.42f);

    public LightingMood Mood =>
        MeanLuminance > Thresholds.HighKeyMean && Ratio < Thresholds.HighKeyRatio ? LightingMood.HighKey
        : MeanLuminance < Thresholds.LowKeyMean || Ratio > Thresholds.LowKeyRatio || Split > Thresholds.LowKeySplit ? LightingMood.LowKey
        : LightingMood.Neutral;

    public Option<(PaletteEntry Key, PaletteEntry Accent)> Gels =>
        Palette.Take(2).Filter(static entry => entry.Chroma >= 0.18f) switch {
            [var first, var second] => Some(Vector2.Dot(new Vector2(first.Position.X - second.Position.X, second.Position.Y - first.Position.Y), Direction) >= 0f ? (first, second) : (second, first)),
            _ => None,
        };

    private static float Linear(float encodedMean) {
        Vector4 mean = new(new Vector3(encodedMean), 1f);
        TransferCurve.Srgb.Decode(new Span<Vector4>(ref mean), Nits.ReferenceWhite);
        return mean.X;
    }
}

public sealed record DistantLight(Vector3 Direction, Vector3 Color, double Stops);

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

        internal bool Chance(double probability) => Bernoulli.Sample(random, probability) is 1;

        internal T Pick<T>(IReadOnlyList<T> items) => items[random.Next(0, items.Count)];
    }

    public static Fin<LightingReading> Read(PixelFrame frame, Gamut gamut, int paletteSize) {
        PixelFrame analysis = frame.Downscaled(PixelExtent.Analysis);
        (int width, int height) = (analysis.Size.Width, analysis.Size.Height);
        (double X, double Y, double Z) luminance = gamut.Luminance;
        double Luma(double red, double green, double blue) => double.Max(LuminanceSamples.Luminance(red, green, blue, luminance), LuminanceSamples.Floor);
        (int Left, int Top, int Right, int Bottom) Inset(double fraction) =>
            ((int)double.Floor(width * fraction), (int)double.Floor(height * fraction)) switch {
                var (x, y) => (x, y, width - x, height - y),
            };
        bool Inside(int index, (int Left, int Top, int Right, int Bottom) box) =>
            int.DivRem(index, width) switch {
                var (y, x) => x >= box.Left && x < box.Right && y >= box.Top && y < box.Bottom,
            };
        (int Left, int Top, int Right, int Bottom) subject = Inset(0.12);
        (long Left, long Top, long Right, long Bottom) window = (
            (long)analysis.Origin.X + subject.Left, (long)analysis.Origin.Y + subject.Top,
            (long)analysis.Origin.X + subject.Right, (long)analysis.Origin.Y + subject.Bottom);
        return paletteSize <= 0 ? Error.New(ErrorCodes.ValidationFailed, string.Create(CultureInfo.InvariantCulture, $"palette size {paletteSize} must be positive"))
            : from all in LuminanceSamples.Sorted(analysis, analysis.Window, rgb => Luma(rgb.X, rgb.Y, rgb.Z),
                (key, pixel) => (Key: key, Index: ((int)(pixel.Y - analysis.Origin.Y) * width) + (int)(pixel.X - analysis.Origin.X)))
              let samples = all.Where(sample => Inside(sample.Index, subject)).ToArray()
              from stops in Metering.Default.Metered([.. samples.Select(static sample => (sample.Key, Weight: 1f))], window)
              select Measured(samples, all, stops);

        LightingReading Measured((double Key, int Index)[] samples, (double Key, int Index)[] all, float stops) {
            Vector3 Color(int index) => MemoryMarshal.Cast<float, Vector4>(analysis.Block)[index].AsVector3();
            Vector2 Point(int index) => int.DivRem(index, width) switch {
                var (row, column) => Position(column, row, width, height),
            };
            (double R, double G, double B) Mean(IEnumerable<int> indices) =>
                indices.Aggregate((R: 0d, G: 0d, B: 0d, Count: 0), (sum, index) => Color(index) switch {
                    var color => (sum.R + color.X, sum.G + color.Y, sum.B + color.Z, sum.Count + 1),
                }) switch {
                    var (r, g, b, count) => (r / count, g / count, b / count),
                };
            Vector3 Rgb((double R, double G, double B) color) => new((float)color.R, (float)color.G, (float)color.B);
            IEnumerable<(double Key, int Index)> Band(float low, float high) =>
                new ArraySegment<(double Key, int Index)>(samples)[LuminanceSamples.Slice(samples.Length, AxisFraction.Create(low), AxisFraction.Create(high))];
            Vector2 Center(IEnumerable<(double Key, int Index)> band) =>
                band.Aggregate((X: 0d, Y: 0d, Count: 0), (sum, sample) => Point(sample.Index) switch {
                    var point => (sum.X + point.X, sum.Y + point.Y, sum.Count + 1),
                }) switch {
                    var (x, y, count) => new((float)(x / count), (float)(y / count)),
                };
            LuminanceSamples ranks = new([.. samples.Select(static sample => sample.Key)]);
            double At(float rank) => ranks.At(AxisFraction.Create(rank));
            float Asymmetry(Func<Vector2, float> axis) =>
                samples.GroupBy(sample => float.Sign(axis(Point(sample.Index))))
                    .ToDictionary(static half => half.Key, static half => half.Average(static sample => sample.Key)) switch {
                        var halves when halves.TryGetValue(1, out double positive) && halves.TryGetValue(-1, out double negative) => (float)((positive - negative) / (positive + negative)),
                        _ => 0f,
                    };
            Vector2 asymmetry = Saturated(new(Asymmetry(static point => point.X), Asymmetry(static point => point.Y)));
            (double Key, int Index)[] highlights = [.. Band(0.55f, 1f)];
            (double momentX, double momentY, double momentWeight) = highlights.Aggregate((X: 0d, Y: 0d, Weight: 0d), (sum, sample) => (Point(sample.Index), Weight: sample.Key - highlights[0].Key) switch {
                var (point, weight) => ((weight * point.X) + sum.X, (weight * point.Y) + sum.Y, weight + sum.Weight),
            });
            Dictionary<int, double> plane = samples.ToDictionary(static sample => sample.Index, static sample => sample.Key);
            (double X, double Y) Slope(int index) {
                (int row, int column) = int.DivRem(index, width);
                (int left, int right, int top, int bottom) = (
                    column > 0 && plane.ContainsKey(index - 1) ? index - 1 : index,
                    column + 1 < width && plane.ContainsKey(index + 1) ? index + 1 : index,
                    row > 0 && plane.ContainsKey(index - width) ? index - width : index,
                    row + 1 < height && plane.ContainsKey(index + width) ? index + width : index);
                return (right == left ? 0d : (plane[right] - plane[left]) / (right - left),
                    bottom == top ? 0d : (plane[top] - plane[bottom]) / ((bottom - top) / width));
            }
            (double gradientX, double gradientY, double gradientWeight, double steepest) = samples.Aggregate((X: 0d, Y: 0d, Weight: 0d, Steepest: 0d), (sum, sample) => Slope(sample.Index) switch {
                var (x, y) => double.Hypot(x, y) switch {
                    var magnitude => (sum.X + (magnitude * x), sum.Y + (magnitude * y), sum.Weight + (magnitude * magnitude), double.Max(sum.Steepest, double.Max(double.Abs(x), double.Abs(y)))),
                },
            });
            Vector2 direction = (0.42f * asymmetry)
                + (0.28f * Saturated(1.25f * (Center(Band(0.78f, 1f)) - Center(Band(0f, 0.22f)))))
                + (0.18f * (momentWeight > 0d ? Saturated(new((float)(momentX / momentWeight), (float)(momentY / momentWeight))) : Vector2.Zero))
                + (0.12f * (gradientWeight > 0d ? new Vector2((float)(gradientX / gradientWeight), (float)(gradientY / gradientWeight)) : Vector2.Zero));
            (int Left, int Top, int Right, int Bottom) centerBox = Inset(0.25);
            (int Left, int Top, int Right, int Bottom) rimBox = Inset(0.18);
            double overall = all.Average(static sample => sample.Key);
            double core = samples.Average(static sample => sample.Key);
            Option<double> Average(IEnumerable<(double Key, int Index)> values) => values.AsIterableNE().Map(static band => Enumerable.Average(band, static sample => sample.Key));
            bool backlit = (from center in Average(all.Where(sample => Inside(sample.Index, centerBox)))
                            from border in Average(all.Where(sample => !Inside(sample.Index, centerBox)))
                            select border > center * 1.35 && center <= overall * 1.08).Exists(static holds => holds)
                || Average(samples.Where(sample => !Inside(sample.Index, rimBox))).Exists(rim => rim > (core * 1.25) && core < (overall * 1.05));
            int count = int.Min(paletteSize, samples.Length);
            (double R, double G, double B)[] centers = [.. Enumerable.Range(0, count).Select(index =>
                Mean(new ArraySegment<(double Key, int Index)>(samples)[
                    (int)((long)index * samples.Length / count)..(int)((long)(index + 1) * samples.Length / count)]
                    .Select(static sample => sample.Index)))];
            int[] Assigned() => [.. samples.Select(sample => Color(sample.Index) switch {
                var color => centers.Select((center, index) => (
                    Distance: double.Pow(center.R - color.X, 2d) + double.Pow(center.G - color.Y, 2d) + double.Pow(center.B - color.Z, 2d), Index: index)).Min().Index,
            })];
            int[] labels = Assigned();
            IGrouping<int, int>[] groups = [.. Enumerable.Range(0, samples.Length).GroupBy(index => labels[index])];
            while (true) {
                foreach (IGrouping<int, int> group in groups)
                    centers[group.Key] = Mean(group.Select(index => samples[index].Index));
                int[] next = Assigned();
                if (labels.SequenceEqual(next))
                    break;
                labels = next;
                groups = [.. Enumerable.Range(0, samples.Length).GroupBy(index => labels[index])];
            }
            List<(PaletteEntry Entry, Unicolour Color)> palette = [];
            foreach (IGrouping<int, int> group in groups.OrderByDescending(static group => group.Count()).ThenBy(static group => group.Key)) {
                Vector3 color = Rgb(centers[group.Key]);
                Vector2 point = Center(group.Select(index => samples[index]));
                PaletteEntry entry = new(color, new(FrameAxis.Create((point.X + 1f) / 2f), FrameAxis.Create((1f - point.Y) / 2f)), (float)group.Count() / samples.Length);
                double level = Luma(color.X, color.Y, color.Z);
                Unicolour perceptual = new Unicolour(gamut.Configuration, ColourSpace.RgbLinear, color.X / level, color.Y / level, color.Z / level).ConvertToConfiguration(Configuration.Default);
                Option<(double Distance, int Index)> nearest = palette.Select((kept, index) => (Distance: perceptual.Difference(kept.Color, DeltaE.Ok), Index: index)).AsIterableNE().Map(static distances => Enumerable.Min(distances));
                if (nearest.Case is ValueTuple<double, int> { Item1: < 0.08, Item2: var index })
                    palette[index] = (palette[index].Entry with { Share = palette[index].Entry.Share + entry.Share }, palette[index].Color);
                else
                    palette.Add((entry, perceptual));
            }
            (double R, double G, double B) key = Mean(Band(0.85f, 1f).Select(static sample => sample.Index));
            (double R, double G, double B) fill = Mean(Band(0.35f, 0.6f).Select(static sample => sample.Index));
            float range = (float)(double.Log2(At(0.95f)) - double.Log2(At(0.05f)));
            double hardnessScale = steepest + (At(0.9f) - At(0.1f));
            return new LightingReading(
                gamut, Rgb(key), Rgb(fill), Rgb(Mean(Band(0f, 0.15f).Select(static sample => sample.Index))),
                (float)(double.Log2(Luma(key.R, key.G, key.B)) - double.Log2(Luma(fill.R, fill.G, fill.B))), range, stops, overall,
                direction, hardnessScale > 0d ? (float)(steepest / hardnessScale) : 0f,
                asymmetry.Length() * float.Min(1f, range / float.Log2(6f)), backlit,
                Adaptation.Correlate(gamut, Rgb(key)).ToOption(), toSeq(palette.Select(static kept => kept.Entry)));
        }
    }

    public static PixelFrame Sphere(Seq<DistantLight> lights, PixelExtent size) {
        (Vector3 Direction, double R, double G, double B)[] sources = [.. lights.Map(static light => double.Exp2(light.Stops) switch {
            var gain => (light.Direction, gain * light.Color.X, gain * light.Color.Y, gain * light.Color.Z),
        })];
        float radius = size.ShortSide / 2f;
        return new PixelFrame(Point.Empty, size, size, block => {
            Span<Vector4> pixels = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < pixels.Length; index++) {
                (int row, int column) = int.DivRem(index, size.Width);
                Vector2 disc = new((column + 0.5f - (size.Width / 2f)) / radius, ((size.Height / 2f) - row - 0.5f) / radius);
                float distance = disc.Length();
                Vector2 rim = distance > 1f ? disc / distance : disc;
                Vector3 normal = new(rim, float.Sqrt(float.Max(1f - rim.LengthSquared(), 0f)));
                (double red, double green, double blue) = (0d, 0d, 0d);
                foreach ((Vector3 direction, double r, double g, double b) in sources) {
                    double cosine = double.Max(((double)normal.X * direction.X) + ((double)normal.Y * direction.Y) + ((double)normal.Z * direction.Z), 0d);
                    (red, green, blue) = (red + (r * cosine), green + (g * cosine), blue + (b * cosine));
                }
                float coverage = float.Clamp(((1f - distance) * radius) + 0.5f, 0f, 1f);
                pixels[index] = coverage > 0f ? new((float)red, (float)green, (float)blue, coverage) : Vector4.Zero;
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
}
