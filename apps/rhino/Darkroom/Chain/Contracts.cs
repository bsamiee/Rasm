using System.Numerics;
using System.Runtime.InteropServices;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rasm.UI;
using Rhino.Render;

namespace Darkroom.Chain;

// --- [TYPES] ---------------------------------------------------------------------------
public interface IPixelStage<TSelf> : IStateRecord<TSelf> where TSelf : IPixelStage<TSelf> {
    public static abstract Option<PixelPass> Pass(TSelf state, PassContext context);

    public static virtual Seq<GuideChannel> Channels(TSelf state) => [];

    public static virtual PassContext Continued(TSelf state, Fraction mix, PassContext context) => context;
}

public interface IFrameJob<TSelf, TState> : IDisposable
    where TSelf : IFrameJob<TSelf, TState>
    where TState : IStateRecord<TState> {
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

public readonly record struct PassContext(
    PixelExtent Extent, Gamut Working, DisplayDevice Display, Option<OutputDepth> Depth, Transfer Signal, double Exposure,
    Option<Camera> Camera, HashMap<GuideChannel, PixelFrame> Guides, Option<TimeSpan> Time, Option<SceneLights> Lights, Derivations Derivations, bool Rendering) {
    public bool Formed => Signal is not Transfer.Linear;
}
