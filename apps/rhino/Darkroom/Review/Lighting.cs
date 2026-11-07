using System.Numerics;
using Darkroom.Chain;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Tone;
using Rasm.Rhino.Document;
using Rasm.Rhino.Objects;
using Rasm.Rhino.Persistence;
using Rhino;
using Rhino.Display;
using UnitsNet;

namespace Darkroom.Review;

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum]
internal sealed partial class Gel {
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

internal sealed record Blob(Vector2 Center, float Sharpness, float Intensity, Vector3 Color);

internal sealed record ReferenceScene {
    public float Base { get; init; } = 0.08f;
    public Vector2 Gradient { get; init; }
    public Vector3 Tint { get; init; } = Vector3.One;
    public Seq<Blob> Blobs { get; init; }
    public float Rim { get; init; }
    public float Vignette { get; init; }
    public float Stripes { get; init; }
    public int Dapples { get; init; }

    public PixelFrame Rendered(PixelExtent size, int seed) {
        Xoshiro256StarStar random = new(seed, threadSafe: false);
        Blob[] blobs = [.. Blobs, .. toSeq(Range(0, Dapples)).Map(_ => new Blob(
            new Vector2(random.Uniform(-1d, 1d), random.Uniform(-1d, 1d)), random.Uniform(8d, 22d), random.Uniform(0.3d, 0.8d), Tint)).Strict()];
        PixelFrame frame = new(System.Drawing.Point.Empty, size, size);
        frame.Rows((line, _, row) => {
            for (int column = 0; column < line.Length; column++) {
                Vector2 point = new((((2f * column) + 1f) / size.Width) - 1f, 1f - (((2f * row) + 1f) / size.Height));
                Vector3 light = (Base + float.Max(((point.X + 1f) * Gradient.X / 2f) + ((point.Y + 1f) * Gradient.Y / 2f), 0f)) * Tint;
                foreach (Blob blob in blobs)
                    light += float.Exp(-Vector2.DistanceSquared(point, blob.Center) * blob.Sharpness) * blob.Intensity * blob.Color;
                float banded = Stripes > 0f && float.SinPi(point.Y * Stripes) <= 0f ? 0.45f : 1f;
                Vector3 rimmed = (light * banded) + new Vector3(float.Pow(float.Clamp(point.Length() - 0.5f, 0f, 1f), 1.4f) * Rim);
                line[column] = new Vector4(rimmed * (1f - (float.Min(point.LengthSquared(), 1f) * Vignette)), 1f);
            }
        });
        return frame;
    }
}

[SmartEnum<string>]
internal sealed partial class ReferencePreset {
    public static readonly ReferencePreset GoldenHour = new("golden-hour", new() {
        Base = 0.2f, Gradient = new(0.9f, 0.1f), Tint = Gel.Warm.Color, Blobs = [new(new(0.7f, 0.3f), 1.2f, 0.7f, Gel.Warm.Color)],
    });
    public static readonly ReferencePreset Sunrise = new("sunrise", new() {
        Base = 0.16f, Gradient = new(0.5f, 0.2f), Tint = new(1f, 0.7f, 0.45f), Blobs = [new(new(-0.6f, -0.2f), 1.4f, 0.8f, new(1f, 0.65f, 0.4f))],
    });
    public static readonly ReferencePreset Sunset = new("sunset", new() {
        Base = 0.18f, Gradient = new(0.6f, 0f), Tint = Gel.DeepWarm.Color, Blobs = [new(new(0.6f, -0.25f), 1.3f, 0.9f, Gel.DeepWarm.Color)], Vignette = 0.3f,
    });
    public static readonly ReferencePreset BlueHour = new("blue-hour", new() { Base = 0.12f, Gradient = new(0f, 0.6f), Tint = Gel.Cool.Color });
    public static readonly ReferencePreset Twilight = new("twilight", new() {
        Base = 0.1f, Gradient = new(0f, 0.5f), Tint = new(0.7f, 0.6f, 1f), Blobs = [new(new(0f, 0.8f), 1f, 0.4f, Gel.Magenta.Color)],
    });
    public static readonly ReferencePreset Midday = new("midday", new() {
        Base = 0.4f, Gradient = new(0f, 0.5f), Tint = Gel.Neutral.Color, Blobs = [new(new(0.1f, 0.8f), 1.4f, 0.6f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset HarshNoon = new("harsh-noon", new() {
        Base = 0.25f, Blobs = [new(new(0f, 0.85f), 3f, 1.1f, Gel.Neutral.Color)], Vignette = 0.45f,
    });
    public static readonly ReferencePreset Overcast = new("overcast", new() { Base = 0.55f, Gradient = new(0f, 0.25f), Tint = new(0.95f, 0.97f, 1f) });
    public static readonly ReferencePreset FoggyMorn = new("foggy-morn", new() { Base = 0.5f, Gradient = new(0f, 0.3f), Tint = new(0.9f, 0.95f, 1f), Vignette = 0.2f });
    public static readonly ReferencePreset NightSky = new("night-sky", new() {
        Base = 0.04f, Gradient = new(0f, 0.25f), Tint = Gel.Cool.Color, Blobs = [new(new(0.4f, 0.8f), 6f, 0.5f, new(0.8f, 0.9f, 1f))],
    });
    public static readonly ReferencePreset TopSky = new("top-sky", new() { Base = 0.08f, Gradient = new(0f, 0.95f), Tint = new(0.85f, 0.92f, 1f) });
    public static readonly ReferencePreset Softbox = new("softbox", new() { Base = 0.5f, Blobs = [new(new(0f, 0.4f), 0.6f, 0.5f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset SoftboxL = new("softbox-l", new() { Base = 0.18f, Blobs = [new(new(-0.6f, 0.35f), 0.9f, 0.8f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset SoftboxR = new("softbox-r", new() { Base = 0.18f, Blobs = [new(new(0.6f, 0.35f), 0.9f, 0.8f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset BeautyDish = new("beauty-dish", new() {
        Base = 0.2f, Blobs = [new(new(0f, 0.45f), 1.6f, 0.9f, Gel.Neutral.Color)], Rim = 0.25f,
    });
    public static readonly ReferencePreset RingLight = new("ring-light", new() {
        Base = 0.18f, Blobs = [new(new(0f, 0f), 0.8f, 0.7f, Gel.Neutral.Color)], Rim = 0.7f,
    });
    public static readonly ReferencePreset Clamshell = new("clamshell", new() {
        Base = 0.25f, Blobs = [new(new(0f, 0.6f), 1.2f, 0.7f, Gel.Neutral.Color), new(new(0f, -0.6f), 1.2f, 0.45f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset Butterfly = new("butterfly", new() { Base = 0.2f, Blobs = [new(new(0f, 0.55f), 1.4f, 0.85f, new(1f, 0.95f, 0.9f))] });
    public static readonly ReferencePreset BroadLight = new("broad-light", new() { Base = 0.16f, Blobs = [new(new(-0.45f, 0.25f), 1f, 0.85f, new(1f, 0.96f, 0.9f))] });
    public static readonly ReferencePreset ShortLight = new("short-light", new() {
        Base = 0.12f, Blobs = [new(new(0.55f, 0.3f), 1.3f, 0.85f, new(1f, 0.96f, 0.9f))], Vignette = 0.2f,
    });
    public static readonly ReferencePreset HighKey = new("high-key", new() { Base = 0.7f, Blobs = [new(new(0f, 0.5f), 0.7f, 0.4f, Gel.Neutral.Color)] });
    public static readonly ReferencePreset LowKey = new("low-key", new() {
        Base = 0.03f, Blobs = [new(new(0.45f, 0.2f), 2.2f, 1f, new(1f, 0.97f, 0.92f))], Vignette = 0.5f,
    });
    public static readonly ReferencePreset Rembrandt = new("rembrandt", new() { Base = 0.06f, Blobs = [new(new(-0.5f, 0.45f), 1.6f, 1.1f, Gel.Warm.Color)] });
    public static readonly ReferencePreset RembrandtR = new("rembrandt-r", new() { Base = 0.06f, Blobs = [new(new(0.5f, 0.45f), 1.6f, 1.1f, Gel.Warm.Color)] });
    public static readonly ReferencePreset Loop = new("loop", new() { Base = 0.12f, Blobs = [new(new(0.35f, 0.3f), 1.4f, 0.95f, new(1f, 0.95f, 0.88f))] });
    public static readonly ReferencePreset Split = new("split", new() {
        Base = 0.05f, Blobs = [new(new(0.85f, 0.1f), 2f, 1.1f, new(1f, 0.97f, 0.92f))], Vignette = 0.35f,
    });
    public static readonly ReferencePreset UnderLight = new("under-light", new() { Base = 0.06f, Blobs = [new(new(0f, -0.7f), 1.6f, 1f, new(1f, 0.95f, 0.9f))] });
    public static readonly ReferencePreset HairLight = new("hair-light", new() {
        Base = 0.1f, Blobs = [new(new(0f, 0.5f), 1f, 0.5f, Gel.Neutral.Color)], Rim = 0.9f,
    });
    public static readonly ReferencePreset Noir = new("noir", new() {
        Base = 0.02f, Blobs = [new(new(-0.8f, 0.35f), 2.4f, 1.2f, Gel.Neutral.Color)], Stripes = 6f, Vignette = 0.55f,
    });
    public static readonly ReferencePreset TealOrange = new("teal-orange", new() {
        Base = 0.08f, Blobs = [new(new(0.6f, 0.25f), 1.2f, 0.9f, Gel.DeepWarm.Color), new(new(-0.6f, -0.1f), 1f, 0.6f, Gel.Teal.Color)],
    });
    public static readonly ReferencePreset NeonMagentaCyan = new("neon-magenta-cyan", new() {
        Base = 0.04f, Blobs = [new(new(-0.55f, 0.3f), 1.2f, 0.9f, Gel.Magenta.Color), new(new(0.55f, -0.1f), 1.2f, 0.9f, Gel.Cyan.Color)],
    });
    public static readonly ReferencePreset NeonBluePink = new("neon-blue-pink", new() {
        Base = 0.04f, Blobs = [new(new(-0.5f, 0f), 1.2f, 0.9f, Gel.Blue.Color), new(new(0.55f, 0.3f), 1.2f, 0.9f, Gel.Pink.Color)],
    });
    public static readonly ReferencePreset Candlelight = new("candlelight", new() {
        Base = 0.05f, Blobs = [new(new(0f, -0.1f), 1.8f, 1f, Gel.Fire.Color)], Vignette = 0.5f,
    });
    public static readonly ReferencePreset Firelight = new("firelight", new() {
        Base = 0.06f, Gradient = new(0f, -0.1f), Tint = Gel.Fire.Color, Blobs = [new(new(0f, -0.5f), 1f, 0.9f, Gel.Fire.Color)], Vignette = 0.35f,
    });
    public static readonly ReferencePreset Moonlight = new("moonlight", new() {
        Base = 0.05f, Blobs = [new(new(-0.4f, 0.5f), 1.4f, 0.7f, Gel.Sky.Color)], Vignette = 0.3f,
    });
    public static readonly ReferencePreset MoonlitWindow = new("moonlit-window", new() {
        Base = 0.04f, Blobs = [new(new(0.5f, 0.2f), 1f, 0.8f, Gel.Sky.Color)], Stripes = 5f, Vignette = 0.35f,
    });
    public static readonly ReferencePreset HorrorUp = new("horror-up", new() {
        Base = 0.04f, Blobs = [new(new(0f, -0.7f), 1.6f, 1f, Gel.Green.Color)], Vignette = 0.45f,
    });
    public static readonly ReferencePreset Window = new("window", new() { Base = 0.12f, Blobs = [new(new(0.55f, 0.35f), 0.8f, 0.85f, new(1f, 0.98f, 0.92f))] });
    public static readonly ReferencePreset WindowBlind = new("window-blind", new() {
        Base = 0.1f, Blobs = [new(new(0.5f, 0.3f), 0.8f, 0.9f, Gel.Warm.Color)], Stripes = 7f,
    });
    public static readonly ReferencePreset Forest = new("forest", new() { Base = 0.1f, Gradient = new(0f, 0.3f), Tint = Gel.Green.Color, Dapples = 14 });
    public static readonly ReferencePreset Underwater = new("underwater", new() { Base = 0.12f, Gradient = new(0f, 0.4f), Tint = Gel.Teal.Color, Dapples = 12 });
    public static readonly ReferencePreset SnowBounce = new("snow-bounce", new() {
        Base = 0.6f, Gradient = new(0f, 0.2f), Tint = new(0.95f, 0.98f, 1f), Blobs = [new(new(0f, -0.5f), 0.8f, 0.3f, Gel.Neutral.Color)],
    });
    public static readonly ReferencePreset Desert = new("desert", new() {
        Base = 0.35f, Gradient = new(0.2f, 0.4f), Tint = new(1f, 0.85f, 0.6f), Blobs = [new(new(0.3f, 0.7f), 1.4f, 0.6f, Gel.Warm.Color)],
    });
    public static readonly ReferencePreset StreetLamp = new("street-lamp", new() {
        Base = 0.03f, Blobs = [new(new(0f, 0.4f), 2f, 1f, new(1f, 0.8f, 0.5f))], Vignette = 0.6f,
    });
    public static readonly ReferencePreset StageSpot = new("stage-spot", new() {
        Base = 0.02f, Blobs = [new(new(0f, 0.6f), 2.6f, 1.2f, Gel.Neutral.Color)], Vignette = 0.6f,
    });
    public static readonly ReferencePreset ConcertRgb = new("concert-rgb", new() {
        Base = 0.04f, Blobs = [new(new(-0.6f, 0.4f), 1.4f, 0.8f, Gel.Blue.Color), new(new(0f, 0.5f), 1.4f, 0.8f, Gel.Magenta.Color), new(new(0.6f, 0.4f), 1.4f, 0.8f, Gel.Cyan.Color)],
    });
    public static readonly ReferencePreset SciFi = new("sci-fi", new() {
        Base = 0.06f, Blobs = [new(new(0.5f, 0f), 1f, 0.7f, Gel.Cyan.Color)], Stripes = 4f, Vignette = 0.3f,
    });

    public ReferenceScene Scene { get; }
}

[SmartEnum<string>]
internal sealed partial class ReferenceDistribution {
    private static readonly Seq<(Vector3 Color, Vector2 Slot)> VividGels = [
        (new(1.4f, 0.12f, 0.85f), new(-0.62f, 0.18f)), (new(0.15f, 0.75f, 1.35f), new(0.62f, 0.18f)), (new(1.35f, 0.55f, 0.05f), new(0f, 0.58f)),
        (new(0.25f, 1.05f, 0.35f), new(-0.38f, -0.42f)), (new(0.75f, 0.25f, 1.35f), new(0.38f, -0.42f))];
    private static readonly Seq<Seq<Seq<int>>> VividSets = [[[0, 1, 3], [0, 1, 4], [0, 2, 4], [1, 2, 4]], [[0, 1, 2, 3], [0, 1, 3, 4], [0, 2, 3, 4]], [[0, 1, 2, 3, 4]]];

    public static readonly ReferenceDistribution Balanced = new("balanced", static random => new() {
        Blobs = toSeq(Range(0, random.Count(1, 3))).Map(_ => new Blob(
            new(random.Uniform(-0.8d, 0.8d), random.Uniform(-0.7d, 0.85d)), random.Uniform(0.7d, 2.4d), random.Uniform(0.5d, 1.1d), random.Pick(toSeq(Gel.Items)).Color)).Strict(),
        Base = random.Uniform(0.03d, 0.5d),
        Gradient = new(random.Uniform(0d, 0.6d), random.Uniform(0d, 0.6d)),
        Tint = random.Pick(toSeq(Gel.Items)).Color,
        Rim = random.Chance(0.4d) ? random.Uniform(0d, 1.2d) : 0f,
        Vignette = random.Chance(0.5d) ? random.Uniform(0d, 0.5d) : 0f,
        Stripes = random.Pick([0f, 0f, 0f, 5f, 7f]),
        Dapples = random.Pick([0, 0, 0, 12]),
    });
    public static readonly ReferenceDistribution Vivid = new("vivid", static random => new() {
        Blobs = random.Pick(random.Pick(VividSets)).Map(gel => new Blob(
            VividGels[gel].Slot + new Vector2(random.Uniform(-0.08d, 0.08d), random.Uniform(-0.08d, 0.08d)), random.Uniform(1.3d, 2.1d), random.Uniform(0.85d, 1.45d), VividGels[gel].Color))
            .Strict(),
        Base = random.Uniform(0.03d, 0.1d),
        Gradient = new(random.Uniform(0d, 0.15d), random.Uniform(0d, 0.15d)),
        Tint = new(0.42f, 0.46f, 0.52f),
        Rim = random.Chance(0.25d) ? random.Uniform(0d, 0.5d) : 0f,
        Vignette = random.Uniform(0.15d, 0.45d),
    });
    public static readonly ReferenceDistribution Soft = new("soft", static random => new() {
        Blobs = toSeq(Range(0, random.Count(1, 2))).Map(_ => new Blob(
            new(random.Uniform(-0.45d, 0.45d), random.Uniform(-0.35d, 0.55d)), random.Uniform(0.55d, 1d), random.Uniform(0.35d, 0.65d),
            random.Pick<Vector3>([new(1f, 0.96f, 0.92f), new(0.94f, 0.96f, 1f), new(0.98f, 0.95f, 0.9f)]))).Strict(),
        Base = random.Uniform(0.32d, 0.55d),
        Gradient = new(random.Uniform(0d, 0.25d), random.Uniform(0d, 0.25d)),
        Tint = random.Pick<Vector3>([new(0.97f, 0.95f, 0.92f), new(0.93f, 0.95f, 0.98f)]),
        Vignette = random.Chance(0.3d) ? random.Uniform(0d, 0.2d) : 0f,
    });

    public ReferenceScene Rolled(int seed) => Roll(new Xoshiro256StarStar(seed, threadSafe: false));

    [UseDelegateFromConstructor]
    private partial ReferenceScene Roll(Random random);
}

public sealed record RigReference(Vector3 KeyColor, Vector3 FillColor, Vector2 Direction, float Ratio, float Hardness) {
    public static RigReference Of(LightingReading reading) =>
        reading.Gels.Match(Some: static gels => (Key: gels.Key.Color, Fill: gels.Accent.Color), None: () => (Key: reading.KeyColor, Fill: reading.FillColor)) switch {
            var (key, fill) => reading.Gamut.MatrixTo(ChainScene.Working.Gamut) switch {
                var toRender => new RigReference(Vector3.Transform(key, toRender), Vector3.Transform(fill, toRender), reading.Direction, reading.Ratio, reading.Hardness),
            },
        };
}

public sealed record RigLayout(
    RigReference Reference, RigEmitter Key, double KeySoftness, Angle FillSize, Option<double> Fill, Option<double> Rim, ColorFilter Tint, double ColorSaturation, double Distance) {
    private static readonly Vector3d InverseSquared = new(0d, 0d, 1d);

    public Fin<Seq<RigLight>> Rigged =>
        (Azimuth: Angle.FromRadians(Reference.Direction.X * Math.PI / 2d),
         Elevation: Angle.FromRadians(Reference.Direction.Y * Math.PI / 2d),
         Size: Angle.FromRadians(Reference.Hardness > 0f ? Math.Atan2(KeySoftness * Reference.Ratio, Reference.Hardness) : Math.PI / 2d),
         Key: Peaked(Reference.KeyColor)) switch {
             var held => (Seq(
                     Row("key", Key, held.Azimuth, held.Elevation, held.Size, 0d, held.Key),
                     Row("fill", new RigEmitter.RectangularLight(), -held.Azimuth, Angle.Zero, FillSize, Fill.IfNone(-Reference.Ratio), Peaked(Reference.FillColor)))
                 + Rim.Map(rim => Row("rim", Key, Angle.FromRadians(Math.IEEERemainder(held.Azimuth.Radians.ToDouble() + Math.PI, Math.Tau)), held.Elevation, held.Size, rim, held.Key)).ToSeq())
                 .Traverse(static light => light)
                 .As(),
         };

    public IO<Seq<(string Slot, Guid Id, bool Created)>> Placed(RhinoDoc doc, KeyName name, RigTarget target, RigBasis basis, double level) =>
        IO.lift(Rigged)
            .Bind(lights => LightRigs.Reconcile(doc, new LightRig(name, target, basis, level, Angle.Zero, Angle.Zero, lights), (string)name, new RedrawPolicy.AllViews(Deferred: true)))
            .Map(static committed => committed.Value);

    private static Color4f Peaked(Vector3 color) => color == Vector3.Zero ? Color4f.White : Lights.Peaked(color.X, color.Y, color.Z).Light;

    private Fin<RigLight> Row(string slot, RigEmitter emitter, Angle azimuth, Angle elevation, Angle size, double stops, Color4f color) =>
        ColorFilter.Validated(
                Tint.Red * (1d + (ColorSaturation * (color.R - 1d))),
                Tint.Green * (1d + (ColorSaturation * (color.G - 1d))),
                Tint.Blue * (1d + (ColorSaturation * (color.B - 1d))))
            .Map(filter => new RigLight(slot, slot, emitter, azimuth, elevation, Distance, size, stops, new LightColor.Linear(filter), InverseSquared));
}

// --- [OPERATIONS] ----------------------------------------------------------------------

internal static class RandomDraws {
    extension(Random random) {
        public float Uniform(double low, double high) => (float)ContinuousUniform.Sample(random, low, high);

        public int Count(int low, int high) => DiscreteUniform.Sample(random, low, high);

        public bool Chance(double probability) => Bernoulli.Sample(random, probability) is 1;

        public T Pick<T>(Seq<T> items) => items[DiscreteUniform.Sample(random, 0, items.Count - 1)];
    }
}
