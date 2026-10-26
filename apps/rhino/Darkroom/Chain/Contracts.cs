using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Procedural;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rhino.Render;
using Wacton.Unicolour;

namespace Darkroom.Chain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IEffectState<TSelf> where TSelf : IEffectState<TSelf> {
    public static abstract TSelf Default { get; }
}

public interface IPixelStage<TSelf> : IEffectState<TSelf> where TSelf : IPixelStage<TSelf> {
    public static abstract Option<PixelPass> Pass(TSelf state, PassContext context);

    public static virtual Seq<GuideChannel> Channels(TSelf state) => [];

    public static virtual PassContext Continued(TSelf state, float mix, PassContext context) => context;
}

public interface IFrameJob<TSelf, TState> : IDisposable
    where TSelf : IFrameJob<TSelf, TState>
    where TState : IEffectState<TState> {
    public static abstract Fin<TSelf> Open();

    public static abstract Seq<GuideChannel> Channels(TState state);

    public Fin<PixelFrame> Run(TState state, PixelFrame color, HashMap<GuideChannel, PixelFrame> guides, IProgress<int> rows);
}

// --- [MODELS] --------------------------------------------------------------------------
[SmartEnum<RenderWindow.StandardChannels>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
public sealed partial class GuideChannel {
    private const float Background = 1e10f;

    public static readonly GuideChannel Depth = new(RenderWindow.StandardChannels.DistanceFromCamera, static (frame, meters) =>
        new PixelFrame(frame.Origin, frame.Size, frame.Extent, block => {
            ReadOnlySpan<Vector4> depths = MemoryMarshal.Cast<float, Vector4>(frame.Block);
            Span<Vector4> metric = MemoryMarshal.Cast<float, Vector4>(block.AsSpan());
            for (int index = 0; index < depths.Length; index++)
                metric[index] = new Vector4(new Vector3(depths[index].X is Background ? float.PositiveInfinity : depths[index].X * meters), depths[index].W);
        }));
    public static readonly GuideChannel Normal = new(RenderWindow.StandardChannels.NormalXYZ, static (frame, _) => frame);
    public static readonly GuideChannel Albedo = new(RenderWindow.StandardChannels.AlbedoRGB, static (frame, _) => frame);
    public static readonly GuideChannel ObjectId = new(RenderWindow.StandardChannels.ObjectIds, static (frame, _) => frame);
    public static readonly GuideChannel MaterialId = new(RenderWindow.StandardChannels.MaterialIds, static (frame, _) => frame);

    [UseDelegateFromConstructor]
    public partial PixelFrame Measured(PixelFrame frame, float meters);
}

public sealed record SceneLights(Option<Vector3> Sun, HashMap<Guid, Vector3> Points, HashMap<Guid, Vector3> Directions);

public sealed record PassContext(
    PixelExtent Extent, Gamut Working, DisplayDevice Display, Option<OutputDepth> Depth, Transfer Signal, double Exposure,
    Option<Camera> Camera, HashMap<GuideChannel, PixelFrame> Guides, Option<TimeSpan> Time, Option<SceneLights> Lights, Derivations Derivations, bool Rendering) {
    public bool Formed => Signal is not Transfer.Linear;
}

public sealed record RampStop([property: JsonConverter(typeof(UnicolourConverter))] Unicolour Color, float Position) {
    public bool Eased { get; init; }

    public static Unicolour At(ImmutableArray<RampStop> ramp, float position) =>
        (toSeq(ramp).Filter(stop => stop.Position <= position).Last.IfNone(ramp[0]), toSeq(ramp).Find(stop => stop.Position >= position).IfNone(ramp[^1])) switch {
            var (low, high) when high.Position > low.Position => low.Color.Mix(high.Color, ColourSpace.RgbLinear, ((position - low.Position) / (high.Position - low.Position)) switch { var progress => high.Eased ? ShapeEdge.Smooth(progress) : progress }),
            var (low, _) => low.Color,
        };
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public sealed class UnicolourConverter : JsonConverter<Unicolour> {
    public override Unicolour Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        JsonSerializer.Deserialize<double[]>(ref reader, options) is [var red, var green, var blue, var alpha]
            ? new Unicolour(ColourSpace.RgbLinear, red, green, blue, alpha)
            : throw new JsonException("colour holds no linear red, green, blue and alpha");

    public override void Write(Utf8JsonWriter writer, Unicolour value, JsonSerializerOptions options) =>
        JsonSerializer.Serialize(writer, new[] { value.RgbLinear.R, value.RgbLinear.G, value.RgbLinear.B, value.Alpha.A }, options);
}
