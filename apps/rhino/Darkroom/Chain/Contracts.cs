using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MathNet.Numerics.Integration;
using Rasm.Imaging.Color;
using Rasm.Imaging.Pixels;
using Rasm.Imaging.Quantization;
using Rasm.Imaging.Tone;
using Rhino.Render;

namespace Darkroom.Chain;

// --- [TYPES] ---------------------------------------------------------------------------

internal interface IPixelStage<TSelf> where TSelf : IPixelStage<TSelf> {
    public static abstract TSelf Default { get; }

    public static abstract Fin<Option<PixelPass>> Pass(TSelf state, PassContext context);

    public static virtual Seq<(string Name, Func<TSelf, TSelf> Look)> Looks => [];

    public static virtual Seq<GuideChannel> Channels(TSelf state) => [];

    public static virtual PassContext Continued(TSelf state, float mix, PassContext context) => context;
}

// --- [MODELS] --------------------------------------------------------------------------

[SmartEnum<RenderWindow.StandardChannels>(SkipIComparable = true, SwitchMethods = SwitchMapMethodsGeneration.None, MapMethods = SwitchMapMethodsGeneration.None)]
internal sealed partial class GuideChannel {
    private const float Background = 1e10f;

    public static readonly GuideChannel Depth = new(RenderWindow.StandardChannels.DistanceFromCamera, static (frame, meters) =>
        new PixelFrame(frame.Origin, frame.Size, frame.Extent, block => {
            ReadOnlySpan<Vector4> depths = MemoryMarshal.Cast<float, Vector4>(frame.Block);
            Span<Vector4> metric = MemoryMarshal.Cast<float, Vector4>(block);
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

internal sealed record SceneLights(Option<Vector3> Sun, HashMap<Guid, Vector3> Points, HashMap<Guid, Vector3> Directions);

internal sealed record PassContext(
    PixelExtent Extent, Gamut Working, DisplayDevice Display, Option<OutputDepth> Depth, Transfer Signal, double Exposure,
    Option<Camera> Camera, HashMap<GuideChannel, PixelFrame> Guides, Option<Duration> Time, Option<SceneLights> Lights, Derivations Derivations, GaussLegendreRule Integration) {
    public bool Rendering { get; init; }
    public bool Formed => Signal is not Transfer.Linear;
}

// --- [ERRORS] --------------------------------------------------------------------------

[SmartEnum]
internal sealed partial class DarkroomReason {
    public static readonly DarkroomReason Stated = new("rejected input");
    public static readonly DarkroomReason Unlisted = new("not a listed effect kind");
    public static readonly DarkroomReason Unchained = new("absent from the chain");
    public static readonly DarkroomReason Staged = new("runs in another stage");
    public static readonly DarkroomReason Unmixed = new("takes no mix");
    public static readonly DarkroomReason Lights = new("no scene lights");
    public static readonly DarkroomReason Empty = new("no entry");
    public static readonly DarkroomReason Unviewed = new("no such view in the document");
    public static readonly DarkroomReason Overscan = new("overscan needs the active viewport");
    public static readonly DarkroomReason Unrenderable = new("cannot render");
    public static readonly DarkroomReason Space = new("exceeds the free space");
    public static readonly DarkroomReason Unreadable = new("unreadable");
    public static readonly DarkroomReason Missing = new("missing");
    public static readonly DarkroomReason Off = new("off");
    public static readonly DarkroomReason Running = new("already running");
    public static readonly DarkroomReason Edited = new("unsaved edits");
    public static readonly DarkroomReason Unattended = new("stops unattended renders");
    public static readonly DarkroomReason Stale = new("out of date");
    public static readonly DarkroomReason Cyclic = new("links each other");
    public static readonly DarkroomReason Units = new("mixed unit systems");
    public static readonly DarkroomReason Cpu = new("runs on CPU");
    public static readonly DarkroomReason Uncopied = new("outside the document copy");
    public static readonly DarkroomReason Invalid = new("invalid");
    public static readonly DarkroomReason Beyond = new("past the entry's frames");
    public static readonly DarkroomReason Refused = new("render job refused");
    public static readonly DarkroomReason Failed = new("failed");
    public static readonly DarkroomReason Unsaved = new("ended without a saved rendering");

    public string Message { get; }
}

internal sealed record DarkroomFault(object? Subject, DarkroomReason Reason, Option<int> Index = default, Option<Error> Inner = default, string Expression = "")
    : Expected(Reason.Message, 0, Inner),
    IValidationError<DarkroomFault> {
    public object? Subject { get; } = Subject;
    public DarkroomReason Reason { get; } = Reason;
    public string Expression { get; } = Expression;

    public override string Message => string.Create(CultureInfo.InvariantCulture,
        $"{Reason.Message}: {Expression}{(Expression.Length is 0 ? "" : " = ")}{Subject}{Index.Map(static at => string.Create(CultureInfo.InvariantCulture, $" at index {at}")).IfNone("")}");

    public static DarkroomFault Create(string message) => new(message, DarkroomReason.Stated);

    public static Fin<Unit> Unless<T>(bool flag, DarkroomReason reason, T rejected, Option<int> index = default, [CallerArgumentExpression(nameof(rejected))] string names = "") =>
        flag ? unit : new DarkroomFault(rejected, reason, index, Expression: names);
}
