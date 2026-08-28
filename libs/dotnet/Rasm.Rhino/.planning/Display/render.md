# [RASM_RHINO_DISPLAY_RENDER]

`RenderJob` owns batch execution and scoped framebuffer operations, detached onto an engine thread through `JobAsync` when an `AsyncProgram` rides the open; `RealtimeEngine` owns progressive viewport participation, `LightAuthorities` owns engine-side custom-light authority over the host light manager, and `SceneQueue` owns live scene-change delivery beside them over the host `ChangeQueue`. `Effects.Configure` owns settings mutation, and `TextureBake` resolves content identities inside a document demand. Every settled host outcome is a VALUE — a `RenderOutcome` carrying the host's own return-code row with its retriability classification — and every registry claim is TOKENED, so a registrant retires only the seat it proved.

`RenderPipeline`, `RenderWindow`, `RenderTexture`, and every `ChangeQueue` payload remain internal host handles. Consumers receive `RenderYield`, `RealtimeStart`, `EffectRoster`, detached texture results, or detached `SceneDelta` batches off a channel reader. The engines mount at plug-in load: `HostUi/shell.md`'s composition capsule registers `RenderJob` capability, the realtime engine plans, and the scene queue as `ShellMount.Engines`, so this page's owners are reached from the one in-package load root rather than waiting for an `apps/` shell.

## [01]-[INDEX]

- [02]-[BATCH_SESSION]: `RenderRun`, `RenderRequest`, `RenderChannel`, `ChannelOrder`, `ChannelFact`, `PixelBlock`, `GammaValue`, `RenderOutcome` with its `RenderCode` roster, `WindowOp` with its `WindowYield` egress family, `RenderJob` with its job-lifetime `PostEffectGate`, and the `AsyncProgram`/`JobAsync` detached-thread modality over `AsyncRenderContext`.
- [03]-[REALTIME]: `RealtimeProgram` hooks, `RealtimePassPolicy`, the host-constructed `RealtimeEngine`/`RealtimeEngineInfo` adapter pair over `RealtimeDisplayMode` with the token-claimed `RealtimeEngines` registry, and the `LightAuthorities` custom-light authority over `LightManagerSupport`.
- [04]-[POST_AND_TEXTURE]: `PostEffectOp` configuration rows over `RenderSettings.PostEffects`, `BuiltinEffect` identity, the `EffectProgram`/`EffectHost`/`EffectPass` authoring half with its `ChannelView` and `GpuHandle` ports, `PostEffectGate` execution control, and the `TextureBake` evaluation rows.
- [05]-[CHANGEQUEUE]: `SceneDelta`, `SceneBatch`, `QueuePolicy`, and the `SceneQueue` adapter over the host change queue with the channel hand-off boundary.
- [06]-[SURFACE_LEDGER]: owner-to-ingress-to-pipeline-to-egress roster across the batch job, the realtime engine pair, the effect and texture half, and the scene queue.

## [02]-[BATCH_SESSION]

- Owner: `RenderRun` closes full-frame and region execution; `FramebufferScope` closes pipeline, viewport-target, and detached window acquisition; `WindowOp` closes framebuffer widening, channel census, pixel writes, gamma/dither adjustment, component reads, raster snapshot, and file egress, each answering one `WindowYield` case; `RenderOutcome` closes the settled host verdict over the `RenderCode` roster, so `Render() == Ok` — a bool collapsing eleven distinct host failures — is the deleted form and a caller reads WHICH code halted the run beside its retriability classification.
- Entry: `RenderJob.Open(DocumentSession, PlugIn, Size2i, CapabilitySet<RenderChannel>, RenderProgram, Option<AsyncProgram>, Option<Func<EffectId, Fin<bool>>>) : Fin<RenderJob>` admits the plan; every `Configure` re-resolves the document, proves request-owned needs, and binds the matching pipeline inside that demand.
- Law: the host outcome is DATA, never a result failure — a run that settled `Cancel` or `EmptyScene` is a measured verdict `RenderYield.Ran` carries as `RenderOutcome.Halted(code)`, while the result fails only on a crossing fault the host raised. `RenderCode` keys on the host's own `RenderReturnCode` (all twelve members, `api-rhinocommon-render.md [ENUM_ROSTERS]`) and each row CLASSIFIES its retriability as a kernel `Retriability` value — `EnterModalLoop` and `ErrorStartingRender` are transient-shaped, a user `Cancel` is terminal by definition — so a root-bound executor reads the classification off the row and this library executes no retry of its own (branch RULINGS `[02]`).
- Law: one lifecycle gate excludes disposal across a complete configuration demand; a document-serial change retires the prior pipeline before the current demand mints its replacement. The release latch is the kernel `MountPhase` row stepped through `Cell.Step` — a second release reads a REFUSED transition rather than no-opping — while the demand-versus-dispose exclusion keeps its lock, because a host demand cannot ride a CAS body.
- Law: every `GetRenderWindow*` call remains inside private `WithWindow`; `WindowOp` is the only public operation vocabulary.
- Law: batch and realtime never merge — a `RenderJob` produces a finished window, a `RealtimeEngine` participates per frame; one owner claiming both is the collapsed form the host API's own split forecloses.
- Law: the framebuffer roster is a job-level fact carried as ONE `CapabilitySet<RenderChannel>` — `RenderChannel` realizes `ICapability`, so admission, membership, wire text, and the host flag word all ride the kernel set algebra and the former `ChannelSet` carrier deletes whole. `Add` widens the roster through `AddChannel` with a `bool`-confirmed outcome and `Census` reads availability, roster visibility, and post-effect demand back as `ChannelFact` rows whose `CapabilitySet<FramebufferState>` admits through `FramebufferState.Law` — a shown-but-unavailable channel is an ILLEGAL corner the census refuses typed rather than publishing. Per-pixel write is `PixelBlock` through `SetRGBAChannelColors`; per-pixel READ is `Read`, whose `OpenChannel` cursor lives exactly one arm and fills a caller-owned component buffer through `GetValues`, so a raw buffer pointer stays unrepresentable and no native pixel port outlives the borrow. Inside a post-effect execute body the reader is `[04]`'s `ChannelView` instead, and the framebuffer publishes no GPU-channel opener at all.
- Law: framebuffer egress is a `WindowOp` arm over an ALREADY-SETTLED path — `Snapshot` answers the host bitmap as a `CaptureArtifact` under the same custody a viewport capture takes, and `SaveAs` takes the `DocumentPath` its caller settled and writes it directly, because both host file writers dispatch on the destination extension. Path settlement is the publish leg's: `Exchange/publish.md`'s `Landing.Save` arm resolves `OutputPolicy` BEFORE building the op, so this page keeps the sink-free write capability and the Exchange-owned settle never re-enters a Display fence (E-R8 arms-up).
- Law: `DitherMethod` is `Rasm.Rhino.Render`'s, composed by name — the framebuffer adjust and the document dither state name one owner, so a mode admitted for one is the mode the other reads.
- Law: the post-effect gate is a JOB-lifetime policy, never a window operation. Registration transfers the native control to the window and leaves the host holding only a weak reference, so the job registers once per pipeline against the session-lifetime framebuffer, roots the instance for its own lifetime, and releases by dropping that root — a gate registered inside a per-batch window borrow is consulted by no render, and an unrooted one silently refuses every effect.
- Law: `Adjust` is read-modify-write because `RenderWindow.ImageAdjust` carries an internal constructor — the arm reads `GetAdjust`, writes both settable axes onto that instance, and returns it through `SetAdjust`; a freshly constructed adjust value is unspellable, and a partial write leaves the unwritten axis at whatever the prior render left.
- Law: the detached-thread modality is `Option<AsyncProgram>` on `Open`, never a sibling job type — when present, `JobPipeline` binds one `JobAsync : AsyncRenderContext` at construction, `OnRenderBegin` launches the engine thread after the program's `Begin`, and a failed launch fails the begin so no orphan thread survives the failure. The detached body's settle CLOSES the async protocol: `JobAsync` answers the host `EndAsyncRender(RenderSuccessCode)` with `Completed` on a settled body and `Failed` on a refused one, so the host window learns the outcome the thread measured rather than inferring one from silence.
- Law: the detached body writes through the same `RealtimePort` pixel carrier the realtime engine owns and halts on the token `StopRendering` trips; the host stop joins the thread, closes the port, and runs the program's `Stopped` hook before the base cancel flag sets.
- Law: pause is a CAPABILITY the program's own shape declares — `RenderProgram.Pause` is `Option`-shaped and `SupportsPause` answers its presence, so the host is never promised a pause the program cannot honour; the pause verbs cross as `RunGate` rows and the modal poll answers a `ContinueVerdict` row, so a user cancel is distinguishable from a fault at the one arm that reads it. Scene population is the same posture: `RenderProgram.Scene` carries the six host scene-population virtuals as one optional program — the document tables the engine walks as a `CapabilitySet<SceneTable>`, the empty-scene admission as its one named bit, the exclusion probe, and the mesh and light intake folds — and an absent program leaves the host's own defaults standing.
- Boundary: `ViewportTarget` enters through `ViewportLease`; disposable `ViewportInfo` values exist only inside its borrow and never cross the render contract.
- Boundary: the framebuffer is LINEAR scene-referred radiometry, so the bulk carriers — `PixelBlock`, the `ValuesCase` component block — keep the host `Color4f` quad and never convert per texel; the one admitted correspondence for a VALUE crossing is the kernel pair `PerceptualColor.OfHost(Color4f, RgbTransfer.Linear)` / `ToColor4f(GamutPolicy.Unbounded, RgbTransfer.Linear)`, which `[04]`'s `ChannelView` publishes beside its raw fast lane so a consumer wanting perceptual math composes the kernel and a hot loop pays nothing.
- Packages: `api-rhinocommon-render.md` (`RenderPipeline`, `RenderWindow`, `RenderWindow.Channel`, `ImageAdjust`, `StandardChannels`, `ComponentOrders`, `RenderReturnCode`, `RenderSuccessCode`, the scene-population virtuals); `api-rhinocommon-render-realtime.md` (`AsyncRenderContext`); `api-languageext.md` (carriers, `Seq`, `Atom`); `api-thinktecture-runtime-extensions.md` (unions, rows); kernel `Domain/results` (`Retriability`, `Transition`/`Cell`), `Domain/hooks` (`Ring<T>`), `Domain/validation` (`CapabilitySet`, `CapabilityLaw`, `FactoryBridge.Row`), `Interaction/chrome` (`MountPhase`).

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System.Collections.Frozen;
using System.Globalization;
using System.Linq;
using System.Threading.Channels;
using Cq = Rhino.Render.ChangeQueue;
using NodaTime;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Interaction;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rasm.Rhino.Exchange;
using Rasm.Rhino.HostUi;
using Rasm.Rhino.Render;
using Rasm.Rhino.Viewport;
using Rasm.Spatial;
using Rhino;
using Rhino.Display;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.PlugIns;
using Rhino.Render;
using Riok.Mapperly.Abstractions;
using Thinktecture;

namespace Rasm.Rhino.Display;

// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RenderChannel : ICapability<RenderChannel> {
    public static readonly RenderChannel Red = new(key: "red", native: RenderWindow.StandardChannels.Red, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Green = new(key: "green", native: RenderWindow.StandardChannels.Green, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Blue = new(key: "blue", native: RenderWindow.StandardChannels.Blue, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Alpha = new(key: "alpha", native: RenderWindow.StandardChannels.Alpha, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Rgba = new(key: "rgba", native: RenderWindow.StandardChannels.RGBA, order: static () => ChannelOrder.Rgba);
    public static readonly RenderChannel Rgb = new(key: "rgb", native: RenderWindow.StandardChannels.RGB, order: static () => ChannelOrder.Rgb);
    public static readonly RenderChannel Distance = new(key: "distance", native: RenderWindow.StandardChannels.DistanceFromCamera, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel NormalX = new(key: "normal-x", native: RenderWindow.StandardChannels.NormalX, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel NormalY = new(key: "normal-y", native: RenderWindow.StandardChannels.NormalY, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel NormalZ = new(key: "normal-z", native: RenderWindow.StandardChannels.NormalZ, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Normal = new(key: "normal", native: RenderWindow.StandardChannels.NormalXYZ, order: static () => ChannelOrder.Rgb);
    public static readonly RenderChannel LuminanceRed = new(key: "luminance-red", native: RenderWindow.StandardChannels.LuminanceRed, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel LuminanceGreen = new(key: "luminance-green", native: RenderWindow.StandardChannels.LuminanceGreen, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel LuminanceBlue = new(key: "luminance-blue", native: RenderWindow.StandardChannels.LuminanceBlue, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel BackgroundLuminanceRed = new(key: "background-luminance-red", native: RenderWindow.StandardChannels.BackgroundLuminanceRed, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel BackgroundLuminanceGreen = new(key: "background-luminance-green", native: RenderWindow.StandardChannels.BackgroundLuminanceGreen, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel BackgroundLuminanceBlue = new(key: "background-luminance-blue", native: RenderWindow.StandardChannels.BackgroundLuminanceBlue, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel MaterialIds = new(key: "material-ids", native: RenderWindow.StandardChannels.MaterialIds, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel ObjectIds = new(key: "object-ids", native: RenderWindow.StandardChannels.ObjectIds, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Wireframe = new(key: "wireframe", native: RenderWindow.StandardChannels.Wireframe, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel AlbedoRed = new(key: "albedo-red", native: RenderWindow.StandardChannels.AlbedoRed, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel AlbedoGreen = new(key: "albedo-green", native: RenderWindow.StandardChannels.AlbedoGreen, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel AlbedoBlue = new(key: "albedo-blue", native: RenderWindow.StandardChannels.AlbedoBlue, order: static () => ChannelOrder.Single);
    public static readonly RenderChannel Albedo = new(key: "albedo", native: RenderWindow.StandardChannels.AlbedoRGB, order: static () => ChannelOrder.Rgb);
    public static readonly RenderChannel WireframePoints = new(key: "wireframe-points", native: RenderWindow.StandardChannels.WireframePointsRGBA, order: static () => ChannelOrder.Rgba);
    public static readonly RenderChannel WireframeIsocurves = new(key: "wireframe-isocurves", native: RenderWindow.StandardChannels.WireframeIsocurvesRGBA, order: static () => ChannelOrder.Rgba);
    public static readonly RenderChannel WireframeCurves = new(key: "wireframe-curves", native: RenderWindow.StandardChannels.WireframeCurvesRGBA, order: static () => ChannelOrder.Rgba);
    public static readonly RenderChannel WireframeAnnotations = new(key: "wireframe-annotations", native: RenderWindow.StandardChannels.WireframeAnnotationsRGBA, order: static () => ChannelOrder.Rgba);

    internal RenderWindow.StandardChannels Native { get; }
    [UseDelegateFromConstructor] internal partial ChannelOrder Order();

    internal Guid Id => RenderWindow.ChannelId(ch: Native);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class FramebufferState : ICapability<FramebufferState> {
    public static readonly FramebufferState Available = new(key: "available");
    public static readonly FramebufferState Shown = new(key: "shown");
    public static readonly FramebufferState Requested = new(key: "requested");

    public static CapabilityLaw<FramebufferState> Law => law.Value;
    private static readonly Lazy<CapabilityLaw<FramebufferState>> law = new(static () =>
        new CapabilityLaw<FramebufferState>(Legal: Seq(
            CapabilitySet<FramebufferState>.None,
            CapabilitySet<FramebufferState>.Of(Available),
            CapabilitySet<FramebufferState>.Of(Available, Shown),
            CapabilitySet<FramebufferState>.Of(Available, Requested),
            CapabilitySet<FramebufferState>.All)));
}

[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[ValidationError]
public readonly partial struct EffectId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(string.Join(" | ", new object?[] { nameof(EffectId) })) : null;
}

[ValueObject<float>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[ValidationError]
public readonly partial struct GammaValue {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref float value) =>
        validationError = ValidityClaim.All(ValidityClaim.Finite(value: value), value > 0.0f)
            ? null
            : new ValidationError(string.Join(" | ", new object?[] { nameof(GammaValue), "a positive finite framebuffer gamma" }));
}

[SmartEnum<int>]
public sealed partial class ChannelOrder {
    public static readonly ChannelOrder Single = new(0, ComponentOrders.Irrelevant, 1);
    public static readonly ChannelOrder Rgba = new(1, ComponentOrders.RGBA, 4);
    public static readonly ChannelOrder Argb = new(2, ComponentOrders.ARGB, 4);
    public static readonly ChannelOrder Rgb = new(3, ComponentOrders.RGB, 3);
    public static readonly ChannelOrder Bgr = new(4, ComponentOrders.BGR, 3);
    public static readonly ChannelOrder Abgr = new(5, ComponentOrders.ABGR, 4);
    public static readonly ChannelOrder Bgra = new(6, ComponentOrders.BGRA, 4);

    internal ComponentOrders Native { get; }
    internal int Components { get; }
}

[SmartEnum<int>]
public sealed partial class ImageEgress {
    public static readonly ImageEgress Dib = new(
        key: 0,
        write: static (window, path) => HostEdge.Side(() => window.SaveDibAsBitmap(filename: path.Value)));
    public static readonly ImageEgress Opaque = new(
        key: 1,
        write: static (window, path) => HostEdge.Side(() => window.SaveRenderImageAs(filename: path.Value, saveAlpha: false)));
    public static readonly ImageEgress Alpha = new(
        key: 2,
        write: static (window, path) => HostEdge.Side(() => window.SaveRenderImageAs(filename: path.Value, saveAlpha: true)));

    [UseDelegateFromConstructor]
    internal partial Unit Write(RenderWindow window, DocumentPath path);
}

[SmartEnum<RenderPipeline.RenderReturnCode>]
public sealed partial class RenderCode {
    public static readonly RenderCode Ok = new(key: RenderPipeline.RenderReturnCode.Ok, retriability: Retriability.Terminal);
    public static readonly RenderCode EmptyScene = new(key: RenderPipeline.RenderReturnCode.EmptyScene, retriability: Retriability.Terminal);
    public static readonly RenderCode Cancel = new(key: RenderPipeline.RenderReturnCode.Cancel, retriability: Retriability.Terminal);
    public static readonly RenderCode NoActiveView = new(key: RenderPipeline.RenderReturnCode.NoActiveView, retriability: Retriability.Terminal);
    public static readonly RenderCode OnPreCreateWindow = new(key: RenderPipeline.RenderReturnCode.OnPreCreateWindow, retriability: Retriability.Terminal);
    public static readonly RenderCode NoFrameWndPointer = new(key: RenderPipeline.RenderReturnCode.NoFrameWndPointer, retriability: Retriability.Terminal);
    public static readonly RenderCode ErrorCreatingWindow = new(key: RenderPipeline.RenderReturnCode.ErrorCreatingWindow, retriability: Retriability.Terminal);
    public static readonly RenderCode ErrorStartingRender = new(key: RenderPipeline.RenderReturnCode.ErrorStartingRender, retriability: Retriability.Transient);
    public static readonly RenderCode EnterModalLoop = new(key: RenderPipeline.RenderReturnCode.EnterModalLoop, retriability: Retriability.Transient);
    public static readonly RenderCode ExitModalLoop = new(key: RenderPipeline.RenderReturnCode.ExitModalLoop, retriability: Retriability.Terminal);
    public static readonly RenderCode ExitRhino = new(key: RenderPipeline.RenderReturnCode.ExitRhino, retriability: Retriability.Terminal);
    public static readonly RenderCode InternalError = new(key: RenderPipeline.RenderReturnCode.InternalError, retriability: Retriability.Terminal);

    public Retriability Retriability { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderOutcome {
    private RenderOutcome() { }
    public sealed record Finished : RenderOutcome;
    public sealed record Halted(RenderCode Code) : RenderOutcome;

    internal static Fin<RenderOutcome> Of(RenderPipeline.RenderReturnCode code) =>
        FactoryBridge.Row<RenderPipeline.RenderReturnCode, RenderCode>(candidate: code).Map(static row =>
            row == RenderCode.Ok ? (RenderOutcome)new Finished() : new Halted(Code: row));

    public bool IsValid => ValidityClaim.All(this is Finished);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderRun : IValidityEvidence {
    private RenderRun() { }
    public sealed record Frame : RenderRun;
    public sealed record Region(ViewportTarget Target, Offset2i Origin, Size2i Extent, HostRow<bool> Placement) : RenderRun;

    public bool IsValid => Switch(
        frame: static _ => true,
        region: static row => ValidityClaim.All(row.Extent.Width > 0, row.Extent.Height > 0));
}

public readonly record struct ChannelFact(RenderChannel Channel, CapabilitySet<FramebufferState> States);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowYield {
    private WindowYield() { }
    public sealed record SilentCase : WindowYield;
    public sealed record ChannelsCase(Seq<ChannelFact> Rows) : WindowYield;
    public sealed record ValuesCase(RenderChannel Channel, Offset2i Origin, Size2i Extent, ChannelOrder Order, ReadOnlyMemory<float> Values) : WindowYield;
    public sealed record RasterCase(CaptureArtifact Artifact) : WindowYield;
    public sealed record SavedCase(DocumentPath Target, ImageEgress Egress) : WindowYield;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowOp : IValidityEvidence {
    private WindowOp() { }
    public sealed record Add(CapabilitySet<RenderChannel> Channels) : WindowOp;
    public sealed record Census(CapabilitySet<RenderChannel> Channels) : WindowOp;
    public sealed record Write(PixelBlock Block) : WindowOp;
    public sealed record Adjust(GammaValue Gamma, DitherMethod Dither) : WindowOp;
    public sealed record Read(RenderChannel Channel, Offset2i Origin, Size2i Extent, ChannelOrder Order) : WindowOp;
    public sealed record Snapshot : WindowOp;
    public sealed record SaveAs(DocumentPath Target, ImageEgress Egress) : WindowOp;

    public bool IsValid => Switch(
        add: static row => !row.Channels.Held.Count.Equals(0),
        census: static row => !row.Channels.Held.Count.Equals(0),
        write: static row => row.Block.IsValid,
        adjust: static _ => true,
        read: static row => ValidityClaim.All(row.Extent.Width > 0, row.Extent.Height > 0),
        snapshot: static _ => true,
        saveAs: static _ => true);

    internal bool Mutates => this is Add or Write or Adjust;

    internal Fin<WindowYield> Apply(RenderWindow window) => Switch(
        window,
        add: static (ctx, row) => Channels.AddTo(row.Channels, ctx).Map(static _ => (WindowYield)new WindowYield.SilentCase()),
        census: static (ctx, row) => Channels.CensusOn(row.Channels, ctx).Map(static rows => (WindowYield)new WindowYield.ChannelsCase(rows)),
        write: static (ctx, row) => row.Block.Blit(ctx).Map(static _ => (WindowYield)new WindowYield.SilentCase()),
        adjust: static (ctx, row) => Try.lift(() =>
            Optional(ctx.GetAdjust()).ToFin(new KernelFault.InvalidResult()).Map(held => {
                held.Gamma = row.Gamma.Value;
                held.Dither = row.Dither.Native;
                ctx.SetAdjust(imageAdjust: held);
                return (WindowYield)new WindowYield.SilentCase();
            })).Run().Bind(static inner => inner),
        read: static (ctx, row) => Try.lift(() => {
            using RenderWindow.Channel channel = ctx.OpenChannel(id: row.Channel.Native);
            return Optional(channel).ToFin(Fail: new KernelFault.InvalidResult()).Bind(open => Try.lift(() => {
                float[] values = new float[checked(row.Extent.Width * row.Extent.Height * row.Order.Components)];
                open.GetValues(
                    rectangle: row.Origin(extent: row.Extent),
                    stride: row.Extent.Width,
                    componentOrder: row.Order.Native,
                    values: ref values);
                return Fin.Succ<WindowYield>(new WindowYield.ValuesCase(row.Channel, row.Origin, row.Extent, row.Order, values));
            }).Run().Bind(static inner => inner));
        }).Run().Bind(static inner => inner),
        snapshot: static (ctx, _) => Try.lift(() => Optional(ctx.GetBitmap()).ToFin(Fail: new KernelFault.InvalidResult())).Run().Bind(static inner => inner)
            .Bind(bitmap => CaptureArtifact.Raster(bitmap: bitmap))
            .Map(static artifact => (WindowYield)new WindowYield.RasterCase(artifact)),
        saveAs: static (ctx, row) => Try.lift(() => Fin.Succ((row.Egress.Write(ctx, row.Target), row.Target).Item2)).Run().Bind(static inner => inner)
            .Map(settled => (WindowYield)new WindowYield.SavedCase(settled, row.Egress)));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderRequest : IValidityEvidence {
    private RenderRequest() { }
    public sealed record Run(RenderRun Scope) : RenderRequest;
    public sealed record Gate(RunGate Posture) : RenderRequest;
    public sealed record Window(FramebufferScope Scope, Seq<WindowOp> Operations) : RenderRequest;

    public bool IsValid => Switch(
        run: static row => row.Scope.IsValid,
        gate: static _ => true,
        window: static row => ValidityClaim.All(
            row.Scope.IsValid,
            !row.Operations.IsEmpty,
            row.Operations.ForAll(static operation => operation.IsValid)));

    internal Seq<SessionNeed> Needs => Switch(
        run: static _ => Seq(SessionNeed.Read),
        gate: static _ => Seq(SessionNeed.Read),
        window: static row => row.Operations.Exists(static operation => operation.Mutates)
            ? Seq(SessionNeed.Read, SessionNeed.Mutate)
            : Seq(SessionNeed.Read));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderYield : IDetachedDocumentResult {
    private RenderYield() { }
    public sealed record Ran(RenderRun Scope, RenderOutcome Outcome) : RenderYield;
    public sealed record Gated(RunGate Posture) : RenderYield;
    public sealed record Windowed(int Operations, Seq<WindowYield> Yields) : RenderYield;

    public Option<RunOutcome> Summary(HostText label) => Switch(
        state: label,
        ran: static (text, row) => row.Outcome is RenderOutcome.Finished
            ? Some<RunOutcome>(new RunOutcome.Completed(Label: text, Scale: Scaled(row.Scope)))
            : None,
        gated: static (_, _) => None,
        windowed: static (text, row) => Some<RunOutcome>(new RunOutcome.Completed(
            Label: text,
            Scale: new Dictionary<string, string>(StringComparer.Ordinal) {
                [nameof(RenderYield.Windowed.Operations)] = row.Operations.ToString(CultureInfo.InvariantCulture),
                [nameof(RenderYield.Windowed.Yields)] = row.Yields.Count.ToString(CultureInfo.InvariantCulture),
            }.ToFrozenDictionary(StringComparer.Ordinal))));

    private static FrozenDictionary<string, string> Scaled(RenderRun scope) => scope.Switch(
        frame: static _ => FrozenDictionary<string, string>.Empty,
        region: static row => new Dictionary<string, string>(StringComparer.Ordinal) {
            [nameof(RenderRun.Region.Extent)] = $"{row.Extent.Width}x{row.Extent.Height}",
        }.ToFrozenDictionary(StringComparer.Ordinal));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record FramebufferScope : IValidityEvidence {
    private FramebufferScope() { }
    public sealed record SessionCase(HostRow<bool> Wireframe, HostRow<bool> Source) : FramebufferScope;
    public sealed record ViewportCase(ViewportTarget Target, HostRow<bool> Source, Offset2i Origin, Size2i Extent) : FramebufferScope;
    public sealed record DetachedCase(ViewportTarget Target, Size2i Extent) : FramebufferScope;

    public bool IsValid => Switch(
        sessionCase: static _ => true,
        viewportCase: static row => ValidityClaim.All(row.Extent.Width > 0, row.Extent.Height > 0),
        detachedCase: static row => ValidityClaim.All(row.Extent.Width > 0, row.Extent.Height > 0));
}

internal static class FramebufferRow {
    internal static readonly HostRow<bool> Offscreen = new(Key: "offscreen", Native: false);
    internal static readonly HostRow<bool> InWindow = new(Key: "in-window", Native: true);
    internal static readonly HostRow<bool> OmitWireframe = new(Key: "omit-wireframe", Native: false);
    internal static readonly HostRow<bool> WithWireframe = new(Key: "with-wireframe", Native: true);
    internal static readonly HostRow<bool> PipelineSource = new(Key: "pipeline-source", Native: false);
    internal static readonly HostRow<bool> RenderSource = new(Key: "render-source", Native: true);
}

[SmartEnum<int>]
public sealed partial class RunGate {
    public static readonly RunGate Running = new(key: 0, paused: false);
    public static readonly RunGate Paused = new(key: 1, paused: true);
    internal bool PausedState { get; }
}

[SmartEnum<int>]
public sealed partial class ContinueVerdict {
    public static readonly ContinueVerdict Proceed = new(key: 0, continues: true);
    public static readonly ContinueVerdict Halt = new(key: 1, continues: false);
    internal bool Continues { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SceneTable : ICapability<SceneTable> {
    public static readonly SceneTable Geometry = new(key: "geometry");
    public static readonly SceneTable Lights = new(key: "lights");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record SceneMesh(RhinoObject Subject, Material Material, Mesh Geometry);

public sealed record SceneProgram(
    CapabilitySet<SceneTable> Tables,
    bool EmptySceneRenders,
    Option<Func<RhinoObject, bool>> Exclude,
    Func<SceneMesh, Fin<Unit>> Mesh,
    Func<LightObject, Fin<Unit>> Light);

public sealed record RenderProgram(
    Func<Fin<Unit>> Begin,
    Func<System.Drawing.Rectangle, Fin<Unit>> BeginRegion,
    Func<Fin<Unit>> End,
    Func<Fin<ContinueVerdict>> Continue,
    Option<Func<RunGate, Fin<Unit>>> Pause = default,
    Option<Func<Size2i, Fin<Unit>>> BeginQuiet = default,
    Option<SceneProgram> Scene = default);

public sealed record AsyncProgram(
    Func<RealtimePort, CancellationToken, Fin<Unit>> Render,
    string ThreadName,
    Option<Func<Fin<Unit>>> Stopped = default);

public sealed record PixelBlock {
    private PixelBlock(Offset2i origin, Size2i extent, ReadOnlyMemory<Color4f> pixels) =>
        (Origin, Extent, Pixels) = (origin, extent, pixels);
    public Offset2i Origin { get; }
    public Size2i Extent { get; }
    public ReadOnlyMemory<Color4f> Pixels { get; }

    public bool IsValid => ValidityClaim.All(
        Extent.Width > 0,
        Extent.Height > 0,
        (long)Pixels.Length == (long)Extent.Width * Extent.Height);

    public static Fin<PixelBlock> Of(Offset2i origin, Size2i extent, ReadOnlyMemory<Color4f> pixels) {
        PixelBlock candidate = new(origin, extent, pixels);
        return candidate.IsValid ? Fin.Succ(candidate) : Fin.Fail<PixelBlock>(new KernelFault.InvalidInput());
    }

    internal Fin<Unit> Blit(RenderWindow window) {
        PixelBlock self = this;
        return Try.lift(() => {
            System.Drawing.Rectangle region = self.Origin.Window(extent: self.Extent);
            window.SetRGBAChannelColors(rectangle: region, colors: self.Pixels.ToArray());
            window.InvalidateArea(region);
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner);
    }
}

// --- [OPERATIONS] ----------------------------------------------------------------------
internal static class Channels {
    internal static CapabilitySet<RenderChannel> Rgba => rgba.Value;
    private static readonly Lazy<CapabilitySet<RenderChannel>> rgba =
        new(static () => CapabilitySet<RenderChannel>.Of(RenderChannel.Rgba));

    internal static RenderWindow.StandardChannels Flags(CapabilitySet<RenderChannel> channels) =>
        (RenderWindow.StandardChannels)channels.Mask(static row => (int)row.Native);

    internal static Fin<Unit> AddTo(CapabilitySet<RenderChannel> channels, RenderWindow window) =>
        toSeq(channels.Held).TraverseM(row => Try.lift(() =>
            Admit.Confirm(success: window.AddChannel(channel: row.Native))).Run().Bind(static inner => inner)).As().Map(static _ => unit);

    internal static Fin<Seq<ChannelFact>> CensusOn(CapabilitySet<RenderChannel> channels, RenderWindow window) => Try.lift(() => {
        FrozenSet<Guid> requested = window.GetRequestedRenderChannels().ToFrozenSet();
        return toSeq(channels.Held).TraverseM(row => {
            CapabilitySet<FramebufferState> measured = CapabilitySet<FramebufferState>.None;
            measured = window.IsChannelAvailable(id: row.Id) ? measured.With(FramebufferState.Available) : measured;
            measured = window.IsChannelShown(id: row.Id) ? measured.With(FramebufferState.Shown) : measured;
            measured = requested.Contains(row.Id) ? measured.With(FramebufferState.Requested) : measured;
            return FramebufferState.Law.Admit(held: measured).Map(states => new ChannelFact(Channel: row, States: states));
        }).As().Map(static rows => rows.Strict());
    }).Run().Bind(static inner => inner);
}

// --- [SERVICES] ------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record JobAsyncLifecycle {
    private JobAsyncLifecycle() { }
    internal sealed record Idle : JobAsyncLifecycle;
    internal sealed record Running(RealtimePort Port) : JobAsyncLifecycle;
    internal sealed record Stopped : JobAsyncLifecycle;
}

internal sealed class JobAsync : AsyncRenderContext {
    private readonly AsyncProgram program;
    private readonly CancellationTokenSource halt = new();
    private readonly Func<Error, Unit> record;
    private readonly Lock lifecycleGate = new();
    private JobAsyncLifecycle lifecycle = new JobAsyncLifecycle.Idle();

    internal JobAsync(AsyncProgram program, Func<Error, Unit> record) =>
        (this.program, this.record, this.key) = (program, record);

    internal Fin<Unit> Launch() {
        lock (lifecycleGate) {
            return from _ in guard(lifecycle is JobAsyncLifecycle.Idle, new KernelFault.InvalidContext()).ToFin()
                   from window in Optional(RenderWindow).ToFin(Fail: new KernelFault.MissingContext())
                   let opened = new RealtimePort(window, key)
                   from installed in Fin.Succ(HostEdge.Side(() => lifecycle = new JobAsyncLifecycle.Running(opened)))
                   from started in Try.lift(() => Admit.Confirm(success: StartRenderThread(
                           threadStart: () => Settled(window: window, port: opened),
                           threadName: program.ThreadName))).Run().Bind(static inner => inner)
                       .MapFail(failure => (
                           HostEdge.Side(() => lifecycle = new JobAsyncLifecycle.Idle()),
                           opened.Close(),
                           failure).Item3)
                   select unit;
        }
    }

    private void Settled(RenderWindow window, RealtimePort port) {
        Fin<Unit> outcome = Try.lift(() => program.Render(port, halt.Token)).Run().Bind(static inner => inner);
        _ = outcome.IfFail(record);
        _ = Try.lift(() => Fin.Succ(HostEdge.Side(() => window.EndAsyncRender(successCode: outcome.IsSucc
            ? RenderWindow.RenderSuccessCode.Completed
            : RenderWindow.RenderSuccessCode.Failed)))).Run().Bind(static inner => inner).IfFail(record);
    }

    public override void StopRendering() {
        lock (lifecycleGate) {
            if (lifecycle is JobAsyncLifecycle.Stopped) { return; }
            JobAsyncLifecycle prior = lifecycle;
            lifecycle = new JobAsyncLifecycle.Stopped();
            _ = Custody.Release(
                    releases: Seq<Func<Fin<Unit>>>(
                        () => Try.lift(halt.Cancel).Run().Bind(static inner => inner),
                        () => Try.lift(JoinRenderThread).Run().Bind(static inner => inner),
                        () => Try.lift(() => Fin.Succ(value: prior.Switch(
                            idle: static _ => unit,
                            running: static row => row.Port.Close(),
                            stopped: static _ => unit))).Run().Bind(static inner => inner),
                        () => program.Stopped.TraverseM(hook => Try.lift(hook).Run().Bind(static inner => inner)).As().Map(static _ => unit),
                        () => Try.lift(() => base.StopRendering()).Run().Bind(static inner => inner)))
                .IfFail(record);
        }
    }

    protected override void Dispose(bool isDisposing) {
        Seq<Func<Fin<Unit>>> releases = isDisposing
            ? Seq<Func<Fin<Unit>>>(() => Try.lift(halt.Dispose).Run().Bind(static inner => inner), () => Try.lift(() => base.Dispose(isDisposing)).Run().Bind(static inner => inner))
            : Seq<Func<Fin<Unit>>>(() => Try.lift(() => base.Dispose(isDisposing)).Run().Bind(static inner => inner));
        _ = Custody.Release(releases: releases).IfFail(record);
    }
}

internal sealed class JobPipeline : RenderPipeline {
    private readonly RenderProgram program;
    private readonly Option<JobAsync> detached;
    private readonly Ring<Error> faults;

    internal JobPipeline(
        RhinoDoc document,
        RunMode mode,
        PlugIn plugin,
        Size2i extent,
        CapabilitySet<RenderChannel> channels,
        RenderProgram program,
        Option<AsyncProgram> render,
        Ring<Error> faults)
        : base(document, mode, plugin, extent.Native, plugin.Name, Channels.Flags(channels), reuseRenderWindow: false, clearLastRendering: true) {
        (this.program, this.faults, this.key) = (program, faults);
        detached = render.Map(plan => new JobAsync(plan, record: failure => Record(failure)));
        _ = detached.Iter(context => {
            AsyncRenderContext bound = context;
            SetAsyncRenderContext(ref bound);
        });
    }

    internal Unit Halt() => ignore(detached.Iter(static context => {
        context.StopRendering();
        context.Dispose();
    }));

    protected override bool OnRenderBegin() =>
        Accept(Try.lift(program.Begin).Run().Bind(static inner => inner)) && detached.Match(
            Some: context => Accept(context.Launch()),
            None: static () => true);

    protected override bool OnRenderBeginQuiet(System.Drawing.Size imageSize) => program.BeginQuiet.Match(
        Some: begin => Accept(Size2i.Of(width: imageSize.Width, height: imageSize.Height)
            .Bind(extent => Try.lift(() => begin(extent)).Run().Bind(static inner => inner))),
        None: () => base.OnRenderBeginQuiet(imageSize));

    protected override bool OnRenderWindowBegin(RhinoView view, System.Drawing.Rectangle rect) =>
        Accept(Try.lift(() => program.BeginRegion(rect)).Run().Bind(static inner => inner));

    protected override void OnRenderEnd(RenderEndEventArgs e) => ignore(Accept(Try.lift(program.End).Run().Bind(static inner => inner)));

    public override bool SupportsPause() => program.Pause.IsSome;

    public override void PauseRendering() => ignore(Accept(Seat(RunGate.Paused)));

    public override void ResumeRendering() => ignore(Accept(Seat(RunGate.Running)));

    protected override bool ContinueModal() => Try.lift(program.Continue).Run().Bind(static inner => inner).Match(
        Succ: static verdict => verdict.Continues,
        Fail: failure => { Record(failure); return false; });

    protected override bool NeedToProcessGeometryTable() =>
        program.Scene.Map(static scene => scene.Tables.Admits(SceneTable.Geometry)).IfNone(base.NeedToProcessGeometryTable());

    protected override bool NeedToProcessLightTable() =>
        program.Scene.Map(static scene => scene.Tables.Admits(SceneTable.Lights)).IfNone(base.NeedToProcessLightTable());

    protected override bool RenderSceneWithNoMeshes() =>
        program.Scene.Map(static scene => scene.EmptySceneRenders).IfNone(base.RenderSceneWithNoMeshes());

    protected override bool IgnoreRhinoObject(RhinoObject obj) =>
        program.Scene.Bind(static scene => scene.Exclude).Match(
            Some: exclude => Try.lift(() => Fin.Succ(exclude(obj))).Run().Bind(static inner => inner).Match(
                Succ: static excluded => excluded,
                Fail: failure => { Record(failure); return false; }),
            None: () => base.IgnoreRhinoObject(obj));

    protected override bool AddRenderMeshToScene(RhinoObject obj, Material material, Mesh mesh) =>
        program.Scene.Match(
            Some: scene => Accept(Try.lift(() => scene.Mesh(new SceneMesh(Subject: obj, Material: material, Geometry: mesh))).Run().Bind(static inner => inner)),
            None: () => base.AddRenderMeshToScene(obj, material, mesh));

    protected override bool AddLightToScene(LightObject light) =>
        program.Scene.Match(
            Some: scene => Accept(Try.lift(() => scene.Light(light)).Run().Bind(static inner => inner)),
            None: () => base.AddLightToScene(light));

    internal Fin<Unit> Seat(RunGate posture) => program.Pause.Match(
        Some: pause => Try.lift(() => pause(posture)).Run().Bind(static inner => inner),
        None: () => Fin.Fail<Unit>(new KernelFault.InvalidContext()));

    internal Unit Record(Error failure) => ignore(faults.Park(item: failure));

    private bool Accept(Fin<Unit> result) => result.Match(
        Succ: static _ => true,
        Fail: failure => { Record(failure); return false; });
}

public sealed class RenderJob : IDisposable, IDetachedDocumentResult {
    private readonly DocumentSession session;
    private readonly PlugIn owner;
    private readonly Size2i extent;
    private readonly CapabilitySet<RenderChannel> channels;
    private readonly RenderProgram program;
    private readonly Option<AsyncProgram> render;
    private readonly Option<Func<EffectId, Fin<bool>>> decide;
    private readonly Ring<Error> faults = new(cap: DisplayFaults.Cap);
    private readonly Lock lifecycle = new();
    private readonly Atom<MountPhase> phase = Atom(MountPhase.Open);
    private JobPipeline? pipeline;
    private PostEffectGate? gate;
    private uint documentSerial;

    private RenderJob(DocumentSession session, PlugIn owner, Size2i extent, CapabilitySet<RenderChannel> channels, RenderProgram program, Option<AsyncProgram> render, Option<Func<EffectId, Fin<bool>>> decide) =>
        (this.session, this.owner, this.extent, this.channels, this.program, this.render, this.decide, this.key) =
        (session, owner, extent, channels, program, render, decide);

    public static Fin<RenderJob> Open(DocumentSession session, PlugIn owner, Size2i extent, CapabilitySet<RenderChannel> channels, RenderProgram program, Option<AsyncProgram> render = default, Option<Func<EffectId, Fin<bool>>> gate = default) {
        return from documentSession in Optional(session).ToFin(Fail: new KernelFault.MissingContext())
               from plugin in Admit.Need(owner)
               from plan in Admit.Need(program)
               from _ in guard(!channels.Held.Count.Equals(0), new KernelFault.InvalidInput(Axis: Some("channels")))
               from __ in guard(extent.Width > 0 && extent.Height > 0, new KernelFault.InvalidInput(Axis: Some("extent")))
               from ___ in guard(render.Match(
                   Some: static detached => detached is { Render: not null } && !string.IsNullOrWhiteSpace(detached.ThreadName),
                   None: static () => true), new KernelFault.InvalidInput(Axis: Some("render")))
               from ____ in guard(gate.Match(Some: static value => value is not null, None: static () => true), new KernelFault.InvalidInput(Axis: Some("gate")))
               select new RenderJob(documentSession, plugin, extent, channels, plan, render, gate);
    }

    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public Fin<RenderYield> Configure(RenderRequest request) {
        lock (lifecycle) {
            return guard(!phase.Value.Closes, new KernelFault.InvalidContext()).ToFin()
                .Bind(_ => guard(request is not null && request.IsValid, new KernelFault.InvalidInput()).ToFin())
                .Bind(_ => Admit.Demand(
                    use: document => Current(document).Bind(current => Apply(current, request)),
                    needs: request.Needs.ToArray()));
        }
    }

    private Fin<RenderYield> Apply(JobPipeline current, RenderRequest request) => request.Switch(
        (Job: this, Pipeline: current),
        run: static (ctx, row) => ctx.Job.Run(ctx.Pipeline, row.Scope)
            .Map(outcome => (RenderYield)new RenderYield.Ran(row.Scope, outcome)),
        gate: static (ctx, row) => ctx.Pipeline.Seat(posture: row.Posture)
            .Map(_ => (RenderYield)new RenderYield.Gated(row.Posture)),
        window: static (ctx, row) => ctx.Job.WithWindow(ctx.Pipeline, row.Scope, window => row.Operations
            .TraverseM(operation => operation.Apply(window)).As()
            .Map(done => (RenderYield)new RenderYield.Windowed(Operations: done.Count, Yields: done.Strict()))));

    private Fin<JobPipeline> Current(RhinoDoc document) =>
        pipeline is { } current && documentSerial == document.RuntimeSerialNumber
            ? Fin.Succ(current)
            : Retire().Bind(_ => Try.lift(() => {
                JobPipeline replacement = new(
                    document: document,
                    mode: session.Mode.Switch(
                        interactive: static () => RunMode.Interactive,
                        scripted: static () => RunMode.Scripted,
                        headless: static () => RunMode.Scripted),
                    plugin: owner,
                    extent: extent,
                    channels: channels,
                    program: program,
                    render: render,
                    faults: faults);
                (pipeline, documentSerial) = (replacement, document.RuntimeSerialNumber);
                return Fin.Succ(replacement);
            }).Run().Bind(static inner => inner).Bind(replacement => Arm(replacement).Map(_ => replacement)));

    private Fin<Unit> Arm(JobPipeline current) => decide.TraverseM(predicate => WithWindow(
            current,
            new FramebufferScope.SessionCase(Wireframe: FramebufferRow.OmitWireframe, Source: FramebufferRow.PipelineSource),
            window => Try.lift(() => {
                PostEffectGate armed = new(predicate, current.Record, op);
                window.RegisterPostEffectExecutionControl(ec: armed);
                gate = armed;
                return Fin.Succ(value: unit);
            }).Run().Bind(static inner => inner))).As().Map(static _ => unit);

    private Fin<RenderOutcome> Run(JobPipeline current, RenderRun scope) {
        RenderJob self = this;
        return scope.Switch(
            state: (Job: self, Pipeline: current),
            frame: static (ctx, _) => Try.lift(() => RenderOutcome.Of(code: ctx.Pipeline.Render())).Run().Bind(static inner => inner),
            region: static (ctx, request) =>
                from lease in ViewportLease.Of(session: ctx.Job.session, target: request.Target)
                from outcome in lease.Use(borrow: row => Try.lift(() => RenderOutcome.Of(
                    code: ctx.Pipeline.RenderWindow(
                        view: row.View,
                        rect: request.Origin.Window(extent: request.Extent),
                        inWindow: request.Placement.Native))).Run().Bind(static inner => inner))
                select outcome);
    }

    private Fin<TOut> WithWindow<TOut>(JobPipeline current, FramebufferScope scope, Func<RenderWindow, Fin<TOut>> borrow) {
        return scope.Switch(
            state: (Job: this, Pipeline: current, Borrow: borrow),
            sessionCase: static (ctx, request) => ctx.Job.BorrowWindow(
                mint: () => ctx.Pipeline.GetRenderWindow(request.Wireframe.Native, request.Source.Native),
                borrow: ctx.Borrow),
            viewportCase: static (ctx, request) =>
                from lease in ViewportLease.Of(ctx.Job.session, request.Target)
                from result in lease.Use(row => row.Info(info => ctx.Job.BorrowWindow(
                    mint: () => ctx.Pipeline.GetRenderWindow(info, request.Source.Native, request.Origin.Window(request.Extent)),
                    borrow: ctx.Borrow)))
                select result,
            detachedCase: static (ctx, request) =>
                from lease in ViewportLease.Of(ctx.Job.session, request.Target)
                from result in lease.Use(row => row.Info(info => ctx.Job.BorrowWindow(
                    mint: () => {
                        RenderWindow window = RenderWindow.Create(request.Extent.Native);
                        window.SetView(info);
                        return window;
                    },
                    borrow: ctx.Borrow)))
                select result);
    }

    private Fin<TOut> BorrowWindow<TOut>(Func<RenderWindow> mint, Func<RenderWindow, Fin<TOut>> borrow) =>
        Try.lift(() => {
            using RenderWindow window = mint();
            return Optional(window).ToFin(Fail: new KernelFault.InvalidResult()).Bind(borrow);
        }).Run().Bind(static inner => inner);

    private Fin<Unit> Retire() {
        JobPipeline? current = pipeline;
        (pipeline, gate, documentSerial) = (null, null, 0u);
        if (current is null) { return Fin.Succ(unit); }
        return Custody.Release(
            releases: Seq<Func<Fin<Unit>>>(
                () => Try.lift(() => Fin.Succ(current.Halt())).Run().Bind(static inner => inner),
                () => Try.lift(() => { current.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner)));
    }

    public void Dispose() {
        lock (lifecycle) {
            _ = HostEdge.SideWhen(
                Cell.Step(phase, static held => held.Closes ? None : Some(MountPhase.Released), new KernelFault.InvalidContext()) is Transition<MountPhase>.Committed,
                () => ignore(Retire().IfFail(failure => ignore(faults.Park(item: failure)))));
        }
    }
}

```

## [03]-[REALTIME]

- Owner: `RealtimeProgram` closes the engine body an author writes; `RealtimePassPolicy` closes pass budget, feature admission, and the GPU technology a descriptor demands; `RealtimeChrome` closes the HUD's declared surface; `RealtimeEnginePlan` binds those three with the session timeline into the ONE value `RealtimeEngines` keys by engine identity. `RealtimeEngine`/`RealtimeEngineInfo` are the host-constructed adapter pair, `RealtimeLifecycle` is their single progressive-state authority, and `LightAuthorities` owns engine-side custom-light authority over `LightManagerSupport` with `LightAuthorityHost` as its host adapter.
- Owner: `SeatRegistry<TSeat>` is the ONE process-static claim table both registries instantiate — `Claim` seats the payload, drives the host installation, and rolls the seat back on refusal, handing the caller a `SeatToken` beside the installation's own proof; `Retire` proves the token before dropping the seat. `RealtimeEngines` and `LightAuthorities` are its two facades, so the claim body, the rollback, and the bounded fault ring exist once.
- Entry: `RealtimeEngines.Register(Guid, RealtimeEnginePlan, PlugIn) : Fin<(SeatToken Token, Seq<Guid> Installed)>` seats the plan then drives the host scan; `LightAuthorities.Register(Guid, LightAuthorityProgram, PlugIn) : Fin<SeatToken>` seats the program then drives the manager registration. `HostUi/shell.md`'s composition capsule calls both at plug-in load and holds the tokens as `ShellMount.Engines`, so retirement at unload names the seats it proved and nothing else.
- Law: both registered host families are HOST-CONSTRUCTED — `RegisterDisplayModes` and `RegisterLightManager` reflect over the plug-in's exported types and activate each through a public parameterless constructor — so an adapter takes no program at construction and resolves it from the registry instead: the engine at `PostConstruct`, the light manager at `RenderEngineId`, and a seated plan is the prerequisite the registration entry claims first. The two process-static tables are FORCED by that activation contract and are the only statics this page declares.
- Law: every claim is TOKENED and retires WHOLE. `Claim` answers the token its own seat carries, `Retire` refuses a token that no longer holds the claim, and `LightAuthorities.Retire` drops the program seat and the `RenderEngineId`-seated host row together, because a surviving host answers `Notify` for a retired program and a surviving program answers host callbacks for a retired engine. The seat transition is the kernel `Cell.Claim` verdict — `Ceded` IS the occupied answer, so the `Find`/`ReferenceEquals` re-derivation the claim used to run has no spelling left.
- Law: a descriptor never restates its engine — `RealtimeEngineInfo` overrides all five host columns and every one derives from the seated `RealtimeEnginePlan`: `Name` off the chrome, `DrawOpenGl` and `DontRegisterAttributesOnStart` off the policy's `CapabilitySet<RealtimeFeature>`, `RequiredDisplayTechnology` off the policy's `GpuTechnology` row. A class-info and the engine it names cannot disagree, and `RealtimeDisplayModeType` stays abstract because only the concrete descriptor knows which engine type it registers.
- Law: progressive state is ONE cell. `RealtimeLifecycle` closes idle, primed, live, and settled as cases stepped through `Cell.Step`, so `IsRendererStarted`, `IsFrameBufferAvailable`, `IsCompleted`, `LastRenderedPass`, and `HudLastRenderedPass` all READ one authority — the `Atom<bool>` framebuffer latch, the two program-polled predicates, and the `ReferenceEquals(lifecycle, owned)` probe that stood in for a transition verdict are the deleted forms. `Priming` carries no pass ordinal because no pass has rendered; that absence is the discriminant, not a forged zero.
- Law: the engine ANSWERS the host and the program DRIVES the engine — `RealtimeSignal` is the back-channel a progressive body holds: `Pass` steps the ordinal, `Settle` closes convergence, `Redraw` requests the host repaint. `SignalRedraw` was a public engine member no program reaches, so the redraw capability stood declared and unreachable.
- Law: render extent is adapter state, not a program poll — `StartRenderer` seeds it and `OnRenderSizeChanged` steps it, so `GetRenderSize` reads what the host itself last supplied and a program answering a stale extent is unspellable.
- Law: the HUD binds the roster the host publishes — nine controls carrying a press and three click gestures, less the two post-effect toggles that publish no press — so `HudSignal.TouchCase` carries control and gesture as columns and `Wire` subscribes one row per real event. A control's pressability is the PRESENCE of its press subscription in the row table, never a mirrored column. `HudRendererPaused` and `HudRendererLocked` answer the host's own `Paused`/`Locked` properties and `HudMaximumPasses` answers the policy's budget, so those three are derived reads, not chrome capabilities: `HudFeature` carries the five the chrome declares.
- Law: framebuffer and middleground hooks project `Seq<DisplayMark>` and complete through `Marks.Paint` over `Canvas.Pipeline` — the adapter alone receives `DisplayPipeline`, reads viewport identity off the pipeline's own live viewport, stamps the boundary's honest `ConduitPhase` row, and parks the `DrawTally` refusal rows. Each projection is GAUGED on `DispatchLane.Paced` through the plan's injected `MonotonicTimeline`, so an over-budget frame reads as a breached span instead of as nothing at all.
- Law: `RealtimePort` is the engine's pixel and progress carrier — `Write` blits a `PixelBlock`, `Progress` reports the host's own caption-and-fraction pair, `Rendering` seats the host's in-flight flag, and `Invalidate` requests the repaint of a written region. It stays live for the progressive session, closes before `Shutdown`, and never exports `RenderWindow`; `[02]`'s detached batch body writes through this same carrier.
- Law: the registered authority is the one source of truth for engine-owned lights — document lights stay on the Objects lights pipeline, a parallel light registry beside the authority is rejected, and every authority hook receives the detached `DocKey`, never the live document. A refusal is TYPED: the three host `bool` returns that once conflated "answered no" with "the operation refused" now answer `Fin<Unit>`, `Fin<SwitchState>`, and a `LightRetirement` row, so the host's scalar is projected from a verdict rather than standing in for one.
- Law: every long-lived ledger on this page is the bounded kernel `Ring<Error>` under the ONE declared `DisplayFaults.Cap` (`Display/conduit.md`) — the registry, an activated engine, and a light authority all outlive any single render, so each sheds under a countable `Shed` rather than growing one `Error` per faulted frame.
- Boundary: `RenderFault` is this page's admission family on `FaultBand.HostRender 4950/4` — the folder ruling seats one fault family per band row at the band's owner page; generated value admissions cross through the kernel validation bridge, while semantic seat and host refusals code here.
- Boundary: callback failures park on `Faults`; no event handler swallows a failed result, and `RealtimeDisplayMode`/`RealtimeDisplayModeClassInfo`/`LightManagerSupport`/`LightArray` never cross a public signature.
- Packages: `api-rhinocommon-render-realtime.md` (`RealtimeDisplayMode` and its ~30 members, `RealtimeDisplayModeClassInfo`, `LightManagerSupport`, `LightArray`, `LightMangerSupportCustomEvent`); `api-rhinocommon-render.md` (`RenderWindow.SetProgress`/`SetIsRendering`/`InvalidateArea`); `api-rhinocommon-display.md` (`DisplayPipeline`, `DisplayTechnology`); kernel `Domain/results` (`FaultBand`, `Cell`/`Transition`), `Domain/hooks` (`Ring<T>`), `Domain/validation` (`CapabilitySet`, `ICapability`, `FactoryBridge.Row`), `Parametric/projections` (`MonotonicTimeline`, `GaugedSpan`), `Interaction/dispatch` (`DispatchLane`); `Display/draw.md` (`Canvas`, `DisplayMark`, `Marks.Paint`, `SpriteSheet`), `Display/conduit.md` (`ConduitFrame`, `ConduitPhase`, `SwitchState`, `DisplayFaults`), `Display/modes.md` (`Appearance`, `HostRow<TNative>`); NodaTime (`Instant`); LanguageExt.Core; Thinktecture.Runtime.Extensions.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class HudControl {
    public static readonly HudControl Play = new(key: 0);
    public static readonly HudControl Pause = new(key: 1);
    public static readonly HudControl Lock = new(key: 2);
    public static readonly HudControl Unlock = new(key: 3);
    public static readonly HudControl ProductName = new(key: 4);
    public static readonly HudControl StatusText = new(key: 5);
    public static readonly HudControl Time = new(key: 6);
    public static readonly HudControl PostEffectsOn = new(key: 7);
    public static readonly HudControl PostEffectsOff = new(key: 8);
}

[SmartEnum<int>]
public sealed partial class HudGesture {
    public static readonly HudGesture Pressed = new(key: 0);
    public static readonly HudGesture LeftClicked = new(key: 1);
    public static readonly HudGesture RightClicked = new(key: 2);
    public static readonly HudGesture DoubleClicked = new(key: 3);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HudSignal {
    private HudSignal() { }
    public sealed record TouchCase(HudControl Control, HudGesture Gesture) : HudSignal;
    public sealed record MaxPassesCase(Rasm.Numerics.Dimension Passes) : HudSignal;
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class RealtimeFeature : ICapability<RealtimeFeature> {
    public static readonly RealtimeFeature PostEffects = new(key: "post-effects");
    public static readonly RealtimeFeature OpenGl = new(key: "open-gl");
    public static readonly RealtimeFeature FastDraw = new(key: "fast-draw");
    public static readonly RealtimeFeature DeferAttributes = new(key: "defer-attributes");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class HudFeature : ICapability<HudFeature> {
    public static readonly HudFeature Show = new(key: "show");
    public static readonly HudFeature Controls = new(key: "controls");
    public static readonly HudFeature Passes = new(key: "passes");
    public static readonly HudFeature MaxPasses = new(key: "max-passes");
    public static readonly HudFeature EditMaxPasses = new(key: "edit-max-passes");
}

[SmartEnum<bool>]
public sealed partial class RenderIntent {
    public static readonly RenderIntent Viewport = new(key: false);
    public static readonly RenderIntent Capture = new(key: true);
}

[SmartEnum<LightMangerSupportCustomEvent>]
public sealed partial class LightChange {
    public static readonly LightChange Added = new(key: LightMangerSupportCustomEvent.light_added);
    public static readonly LightChange Deleted = new(key: LightMangerSupportCustomEvent.light_deleted);
    public static readonly LightChange Undeleted = new(key: LightMangerSupportCustomEvent.light_undeleted);
    public static readonly LightChange Modified = new(key: LightMangerSupportCustomEvent.light_modified);
    public static readonly LightChange Sorted = new(key: LightMangerSupportCustomEvent.light_sorted);
}

[SmartEnum<bool>]
public sealed partial class LightRetirement {
    public static readonly LightRetirement Retire = new(key: false);
    public static readonly LightRetirement Restore = new(key: true);
}

[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[ValidationError]
public readonly partial struct SeatToken {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(string.Join(" | ", new object?[] { nameof(SeatToken) })) : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record RealtimeLifecycle {
    private RealtimeLifecycle() { }
    internal sealed record Idle : RealtimeLifecycle;
    internal sealed record Priming(SpriteSheet Sprites, RealtimePort Pixels) : RealtimeLifecycle;
    internal sealed record Live(SpriteSheet Sprites, RealtimePort Pixels, Rasm.Numerics.Dimension Pass) : RealtimeLifecycle;
    internal sealed record Settled(Rasm.Numerics.Dimension Pass) : RealtimeLifecycle;

    internal Option<(SpriteSheet Sprites, RealtimePort Pixels)> Held => Switch(
        idle: static _ => Option<(SpriteSheet, RealtimePort)>.None,
        priming: static row => Some((row.Sprites, row.Pixels)),
        live: static row => Some((row.Sprites, row.Pixels)),
        settled: static _ => Option<(SpriteSheet, RealtimePort)>.None);

    internal Rasm.Numerics.Dimension Pass => Switch(
        idle: static _ => Rasm.Numerics.Dimension.Create(value: 0),
        priming: static _ => Rasm.Numerics.Dimension.Create(value: 0),
        live: static row => row.Pass,
        settled: static row => row.Pass);
}

// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.HostRender;
    private RenderFault() { }

    [FaultCase(0)] public sealed partial record SeatTaken(Guid Engine) : RenderFault;
    [FaultCase(1)] public sealed partial record SeatAbsent(Guid Engine) : RenderFault;
    [FaultCase(2)] public sealed partial record Unbound(string Member) : RenderFault;
    [FaultCase(3)] public sealed partial record HostRefused(string Member, string Detail) : RenderFault;

    public sealed override string Message => Switch(
        seatTaken: static fault => $"Render engine '{fault.Engine}' already has a seat for '{fault.Key}'.",
        seatAbsent: static fault => $"Render engine '{fault.Engine}' has no seat for '{fault.Key}'.",
        unbound: static fault => $"Render member '{fault.Member}' is unbound for '{fault.Key}'.",
        hostRefused: static fault => $"Render host member '{fault.Member}' refused '{fault.Key}': {fault.Detail}");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record RealtimePassPolicy {
    private RealtimePassPolicy(Rasm.Numerics.Dimension maxPasses, CapabilitySet<RealtimeFeature> features, GpuTechnology technology) =>
        (MaxPasses, Features, Technology) = (maxPasses, features, technology);

    public Rasm.Numerics.Dimension MaxPasses { get; }
    public CapabilitySet<RealtimeFeature> Features { get; }
    public GpuTechnology Technology { get; }

    public static Fin<RealtimePassPolicy> Of(
        Rasm.Numerics.Dimension maxPasses,
        CapabilitySet<RealtimeFeature> features,
        Option<GpuTechnology> technology = default) {
        return guard(maxPasses.Value > 0, new KernelFault.InvalidInput(Axis: Some(nameof(maxPasses)))).ToFin()
            .Map(_ => new RealtimePassPolicy(maxPasses, features, technology.IfNone(GpuTechnology.Absent)));
    }
}

public sealed record RealtimeChrome {
    private RealtimeChrome(HostText productName, CapabilitySet<HudFeature> features, Option<Func<Fin<HostText>>> status, Option<Instant> started) =>
        (ProductName, Features, Status, Started) = (productName, features, status, started);

    public HostText ProductName { get; }
    public CapabilitySet<HudFeature> Features { get; }
    public Option<Func<Fin<HostText>>> Status { get; }
    public Option<Instant> Started { get; }

    public static Fin<RealtimeChrome> Of(
        HostText productName,
        CapabilitySet<HudFeature> features,
        Option<Func<Fin<HostText>>> status = default,
        Option<Instant> started = default) {
        return guard(
                !features.Admits(HudFeature.EditMaxPasses) || features.Admits(HudFeature.MaxPasses),
                (Error)new KernelFault.InvalidValue(nameof(RealtimeChrome), "an editable max-pass field the HUD also displays")).ToFin()
            .Bind(_ => Admit.Need(productName))
            .Map(name => new RealtimeChrome(name, features, status, started));
    }
}

public readonly record struct RealtimeStart(
    Size2i Extent, DocKey Document, RenderIntent Intent, RealtimePort Pixels, RealtimeSignal Signal);

public readonly record struct RealtimeWorld(DocKey Document, Guid View, Seq<Appearance> Appearance);

public sealed record RealtimeProgram(
    Func<RealtimeStart, Fin<Unit>> Start,
    Func<Fin<Unit>> Shutdown,
    Func<Size2i, Fin<Unit>> Resized,
    Func<ConduitFrame, Fin<Seq<DisplayMark>>> InitFramebuffer,
    Func<ConduitFrame, Fin<Seq<DisplayMark>>> DrawMiddleground,
    Func<Seq<Appearance>, Fin<Unit>> SettingsChanged,
    Option<Func<RealtimeWorld, Fin<Unit>>> World = default,
    Option<Func<ViewFrame, Fin<Unit>>> Viewed = default,
    Option<Func<Fin<UnitInterval>>> CaptureProgress = default,
    Option<Func<HudSignal, Fin<Unit>>> Hud = default);

public readonly record struct ViewFrame(Guid View, uint Crc);

public sealed record RealtimeEnginePlan(
    RealtimeProgram Program,
    RealtimePassPolicy Policy,
    RealtimeChrome Chrome,
    MonotonicTimeline Clock) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Program is not null, Policy is not null, Chrome is not null, Clock is not null);
}

public sealed record LightSolo(
    Func<DocKey, Guid, SwitchState, Fin<Unit>> Set,
    Func<DocKey, Guid, Fin<SwitchState>> Get,
    Func<DocKey, Fin<Rasm.Numerics.Dimension>> Count);

public sealed record LightAuthorityProgram(
    Func<DocKey, Fin<Seq<Light>>> Roster,
    Func<DocKey, Guid, Fin<Option<Light>>> Resolve,
    Func<DocKey, Light, Fin<Unit>> Amend,
    Func<DocKey, Light, LightRetirement, Fin<Unit>> Retire,
    Func<DocKey, Light, Fin<int>> Serial,
    Func<DocKey, Light, Fin<HostText>> Describe,
    Func<DocKey, Seq<Light>, Fin<Unit>> Edit,
    Func<DocKey, Seq<Light>, Fin<Unit>> Group,
    Func<DocKey, Seq<Light>, Fin<Unit>> Ungroup,
    Option<LightSolo> Solo = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Roster is not null, Resolve is not null, Amend is not null, Retire is not null, Serial is not null,
        Describe is not null, Edit is not null, Group is not null, Ungroup is not null);
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class RealtimePort {
    private readonly Atom<Option<RenderWindow>> window;

    internal RealtimePort(RenderWindow window) =>
        (this.window, this.key) = (Atom(Some(window)));

    public Fin<Unit> Write(PixelBlock block) => Held().Bind(target => block.Blit(target, key));

    public Fin<Unit> Progress(HostText caption, UnitInterval fraction) => Held().Bind(target =>
        Try.lift(() => Fin.Succ(HostEdge.Side(() => target.SetProgress(text: caption.Resolve(), progress: (float)fraction.Value)))).Run().Bind(static inner => inner));

    public Fin<Unit> Rendering(SwitchState state) => Held().Bind(target =>
        Try.lift(() => Fin.Succ(HostEdge.Side(() => target.SetIsRendering(is_rendering: state.Enabled)))).Run().Bind(static inner => inner));

    public Fin<Unit> Invalidate(Option<(Offset2i Origin, Size2i Extent)> region = default) => Held().Bind(target =>
        Try.lift(() => Fin.Succ(region.Match(
            Some: row => HostEdge.Side(() => target.InvalidateArea(row.Origin.Window(extent: row.Extent))),
            None: () => HostEdge.Side(target.Invalidate)))).Run().Bind(static inner => inner));

    internal Unit Close() => ignore(Cell.Take(window));

    private Fin<RenderWindow> Held() => window.Value.ToFin(Fail: new KernelFault.InvalidContext());
}

public sealed class RealtimeSignal {
    private readonly Func<Fin<Unit>> redraw;
    private readonly Func<Option<Rasm.Numerics.Dimension>, Transition<RealtimeLifecycle>> step;

    internal RealtimeSignal(Func<Fin<Unit>> redraw, Func<Option<Rasm.Numerics.Dimension>, Transition<RealtimeLifecycle>> step) =>
        (this.redraw, this.step) = (redraw, step);

    public Fin<Unit> Redraw() => redraw();

    public Transition<RealtimeLifecycle> Pass(Rasm.Numerics.Dimension ordinal) => step(Some(ordinal));

    public Transition<RealtimeLifecycle> Settle() => step(Option<Rasm.Numerics.Dimension>.None);
}

internal sealed class SeatRegistry<TSeat> where TSeat : notnull {
    private readonly Atom<HashMap<Guid, (SeatToken Token, TSeat Seat)>> seats =
        Atom(HashMap<Guid, (SeatToken Token, TSeat Seat)>());
    private readonly Ring<Error> faults = new(cap: DisplayFaults.Cap);

    internal SeatRegistry() => this.key = key;

    internal Seq<Error> Faults => faults.Parked;
    internal long Shed => faults.Shed;

    internal Fin<(SeatToken Token, TProof Proof)> Claim<TProof>(Guid engine, TSeat seat, Func<Fin<TProof>> install) {
        SeatToken token = SeatToken.Create(Guid.NewGuid());
        return from _ in guard(engine != Guid.Empty, (Error)new KernelFault.InvalidValue(nameof(engine), "a non-empty render engine identity")).ToFin()
               from claimed in Cell.Claim(seats, engine, () => (token, seat)) is Transition<HashMap<Guid, (SeatToken, TSeat)>>.Committed
                   ? Fin.Succ(token)
                   : Fin.Fail<SeatToken>(new RenderFault.SeatTaken(Engine: engine))
               from proof in Try.lift(install).Run().Bind(static inner => inner).Rollback(() => Fin.Succ(Release(engine, claimed)))
               select (claimed, proof);
    }

    internal Fin<Unit> Retire(Guid engine, SeatToken token, Func<Fin<Unit>> uninstall) =>
        from held in seats.Value.Find(engine).ToFin(Fail: new RenderFault.SeatAbsent(Engine: engine))
        from owned in guard(held.Token == token, (Error)new RenderFault.SeatTaken(Engine: engine))
        from _ in Custody.Release(Seq<Func<Fin<Unit>>>(uninstall, () => Fin.Succ(Release(engine, token))))
        select unit;

    internal Option<TSeat> Find(Guid engine) => seats.Value.Find(engine).Map(static row => row.Seat);

    internal Unit Observe(Error failure) => ignore(faults.Park(item: failure));

    private Unit Release(Guid engine, SeatToken token) => ignore(seats.Swap(rows => rows.Find(engine)
        .Filter(row => row.Token == token)
        .Map(_ => rows.Remove(engine))
        .IfNone(rows)));
}

public static class RealtimeEngines {
    private static readonly SeatRegistry<RealtimeEnginePlan> Seats = new();

    public static Seq<Error> Faults => Seats.Faults;
    public static long Shed => Seats.Shed;

    public static Fin<(SeatToken Token, Seq<Guid> Installed)> Register(
        Guid engine, RealtimeEnginePlan plan, PlugIn owner) {
        return from admitted in guard(plan is { IsValid: true }, new KernelFault.InvalidInput(Axis: Some(nameof(plan)))).ToFin()
               from plugin in Admit.Need(owner)
               from claimed in Seats.Claim(engine, plan, () =>
                   from rows in Try.lift(() => Optional(RealtimeDisplayMode.RegisterDisplayModes(plugin: plugin))
                       .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(RealtimeDisplayMode.RegisterDisplayModes), Detail: "no descriptor set"))).Run().Bind(static inner => inner)
                   let installed = toSeq(rows).Map(static row => row.GUID).Strict()
                   from matched in guard(
                       installed.Exists(row => row == engine),
                       (Error)new RenderFault.SeatAbsent(Engine: engine))
                   select installed)
               select claimed;
    }

    public static Fin<Unit> Unregister(Guid engine, SeatToken token, PlugIn owner) {
        return Admit.Need(owner).Bind(plugin => Seats.Retire(
            engine, token, () => Try.lift(() => RealtimeDisplayMode.UnregisterDisplayModes(plugin: plugin)).Run().Bind(static inner => inner)));
    }

    internal static Option<RealtimeEnginePlan> Plan(Guid engine) => Seats.Find(engine);

    internal static Unit Observe(Error failure) => Seats.Observe(failure);

}

public abstract class RealtimeEngineInfo : RealtimeDisplayModeClassInfo {
    protected abstract Guid Engine { get; }

    public sealed override Guid GUID => Engine;

    public sealed override string Name => Chrome(static chrome => chrome.ProductName.Resolve(), nameof(RealtimeEngine));

    public sealed override bool DrawOpenGl => Policy(static policy => policy.Features.Admits(RealtimeFeature.OpenGl), false);

    public sealed override bool DontRegisterAttributesOnStart =>
        Policy(static policy => policy.Features.Admits(RealtimeFeature.DeferAttributes), false);

    public sealed override Rhino.Display.DisplayTechnology RequiredDisplayTechnology =>
        Policy(static policy => policy.Technology.Key, Rhino.Display.DisplayTechnology.None);

    private TOut Policy<TOut>(Func<RealtimePassPolicy, TOut> read, TOut fallback) =>
        Seated(plan => read(plan.Policy), fallback, member: nameof(Policy));

    private TOut Chrome<TOut>(Func<RealtimeChrome, TOut> read, TOut fallback) =>
        Seated(plan => read(plan.Chrome), fallback, member: nameof(Chrome));

    private TOut Seated<TOut>(Func<RealtimeEnginePlan, TOut> read, TOut fallback, string member) =>
        RealtimeEngines.Plan(Engine).Match(
            Some: read,
            None: () => (RealtimeEngines.Observe(new RenderFault.Unbound(Key: RealtimeEngines.Anchor, Member: member)), fallback).Item2);
}

public abstract class RealtimeEngine : RealtimeDisplayMode {
    private readonly Atom<RealtimeLifecycle> lifecycle = Atom<RealtimeLifecycle>(new RealtimeLifecycle.Idle());
    private readonly Atom<Size2i> extent = Atom(Size2i.Create(width: 1, height: 1));
    private readonly Atom<Option<ViewFrame>> viewed = Atom(Option<ViewFrame>.None);
    private readonly Ring<Error> faults = new(cap: DisplayFaults.Cap);
    private readonly Lock lifecycleGate = new();
    private Option<RealtimeEnginePlan> bound;

    protected abstract Guid Engine { get; }

    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public sealed override void PostConstruct() {
        base.PostConstruct();
        bound = RealtimeEngines.Plan(Engine);
        _ = bound.Match(
            Some: Wire,
            None: () => Observe(new RenderFault.Unbound(Member: nameof(PostConstruct))));
    }

    private Unit Wire(RealtimeEnginePlan plan) {
        MaxPasses = plan.Policy.MaxPasses.Value;
        PostEffectsOn = plan.Policy.Features.Admits(RealtimeFeature.PostEffects);
        SetUseDrawOpenGl(plan.Policy.Features.Admits(RealtimeFeature.OpenGl));
        OnInitFramebuffer += (_, e) => ignore(Project(plan, e.Pipeline, ConduitPhase.Framebuffer, plan.Program.InitFramebuffer)
            .Iter(_ => ignore(Step(static held => held is RealtimeLifecycle.Priming row
                ? Some<RealtimeLifecycle>(new RealtimeLifecycle.Live(row.Sprites, row.Pixels, Rasm.Numerics.Dimension.Create(value: 0)))
                : Option<RealtimeLifecycle>.None))));
        OnDrawMiddleground += (_, e) => ignore(Project(plan, e.Pipeline, ConduitPhase.Middleground, plan.Program.DrawMiddleground));
        OnDisplayPipelineSettingsChanged += (_, e) => ignore(Observe(
            Appearance.Of(e.Attributes).Bind(concerns => Try.lift(() => plan.Program.SettingsChanged(concerns)).Run().Bind(static inner => inner))));
        return ignore(plan.Program.Hud.Iter(WireHud));
    }

    private Unit WireHud(Func<HudSignal, Fin<Unit>> signal) {
        Unit Bind(HudControl control, HudGesture gesture, Action<EventHandler> subscribe) => HostEdge.Side(() => subscribe(
            (_, _) => ignore(Observe(Try.lift(() => signal(new HudSignal.TouchCase(control, gesture))).Run().Bind(static inner => inner)))));

        Unit Control(
            HudControl control,
            Option<Action<EventHandler>> pressed,
            Action<EventHandler> left,
            Action<EventHandler> right,
            Action<EventHandler> doubled) {
            _ = pressed.Iter(subscribe => Bind(control, HudGesture.Pressed, subscribe));
            _ = Bind(control, HudGesture.LeftClicked, left);
            _ = Bind(control, HudGesture.RightClicked, right);
            return Bind(control, HudGesture.DoubleClicked, doubled);
        }

        _ = Control(HudControl.Play, Some<Action<EventHandler>>(h => HudPlayButtonPressed += h),
            h => HudPlayButtonLeftClicked += h, h => HudPlayButtonRightClicked += h, h => HudPlayButtonDoubleClicked += h);
        _ = Control(HudControl.Pause, Some<Action<EventHandler>>(h => HudPauseButtonPressed += h),
            h => HudPauseButtonLeftClicked += h, h => HudPauseButtonRightClicked += h, h => HudPauseButtonDoubleClicked += h);
        _ = Control(HudControl.Lock, Some<Action<EventHandler>>(h => HudLockButtonPressed += h),
            h => HudLockButtonLeftClicked += h, h => HudLockButtonRightClicked += h, h => HudLockButtonDoubleClicked += h);
        _ = Control(HudControl.Unlock, Some<Action<EventHandler>>(h => HudUnlockButtonPressed += h),
            h => HudUnlockButtonLeftClicked += h, h => HudUnlockButtonRightClicked += h, h => HudUnlockButtonDoubleClicked += h);
        _ = Control(HudControl.ProductName, Some<Action<EventHandler>>(h => HudProductNamePressed += h),
            h => HudProductNameLeftClicked += h, h => HudProductNameRightClicked += h, h => HudProductNameDoubleClicked += h);
        _ = Control(HudControl.StatusText, Some<Action<EventHandler>>(h => HudStatusTextPressed += h),
            h => HudStatusTextLeftClicked += h, h => HudStatusTextRightClicked += h, h => HudStatusTextDoubleClicked += h);
        _ = Control(HudControl.Time, Some<Action<EventHandler>>(h => HudTimePressed += h),
            h => HudTimeLeftClicked += h, h => HudTimeRightClicked += h, h => HudTimeDoubleClicked += h);
        _ = Control(HudControl.PostEffectsOn, None,
            h => HudPostEffectsOnButtonLeftClicked += h, h => HudPostEffectsOnButtonRightClicked += h, h => HudPostEffectsOnButtonDoubleClicked += h);
        _ = Control(HudControl.PostEffectsOff, None,
            h => HudPostEffectsOffButtonLeftClicked += h, h => HudPostEffectsOffButtonRightClicked += h, h => HudPostEffectsOffButtonDoubleClicked += h);
        MaxPassesChanged += (_, e) => ignore(Observe(Try.lift(() =>
            signal(new HudSignal.MaxPassesCase(Passes: Rasm.Numerics.Dimension.Create(value: e.MaxPasses)))).Run().Bind(static inner => inner)));
        return unit;
    }

    // --- [ENGINE_LIFECYCLE]
    public override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) {
        lock (lifecycleGate) {
            Fin<RealtimeLifecycle.Priming> opened =
                from _ in guard(lifecycle.Value is RealtimeLifecycle.Idle, new KernelFault.InvalidContext()).ToFin()
                from size in Size2i.Of(width: w, height: h)
                from document in DocKey.Of(doc, key)
                from intent in FactoryBridge.Row<bool, RenderIntent>(candidate: forCapture)
                from window in Admit.Need(renderWindow)
                let primed = new RealtimeLifecycle.Priming(new SpriteSheet(), new RealtimePort(window, key))
                from started in Bound(program => program.Start(new RealtimeStart(
                        Extent: size, Document: document, Intent: intent, Pixels: primed.Pixels, Signal: Signal())))
                    .Rollback(() => Release(primed), key)
                from seated in Fin.Succ(ignore(extent.Swap(_ => size)))
                from stepped in Step(_ => Some<RealtimeLifecycle>(primed)) is Transition<RealtimeLifecycle>.Committed
                    ? Fin.Succ(primed)
                    : Fin.Fail<RealtimeLifecycle.Priming>(new KernelFault.InvalidContext())
                select stepped;
            return Observe(opened).IsSucc;
        }
    }

    public override void ShutdownRenderer() {
        lock (lifecycleGate) {
            RealtimeLifecycle prior = lifecycle.Value;
            _ = Step(static held => held is RealtimeLifecycle.Idle ? None : Some<RealtimeLifecycle>(new RealtimeLifecycle.Idle()))
                is Transition<RealtimeLifecycle>.Committed
                ? ignore(Observe(Custody.Release(
                    Seq<Func<Fin<Unit>>>(() => Bound(static program => program.Shutdown()), () => Released(prior)), key)))
                : unit;
        }
    }

    public override void GetRenderSize(out int width, out int height) =>
        (width, height) = (extent.Value.Width, extent.Value.Height);

    public override bool OnRenderSizeChanged(int width, int height) => Observe(
        from size in Size2i.Of(width: width, height: height)
        from _ in Fin.Succ(ignore(extent.Swap(_ => size)))
        from resized in Bound(program => program.Resized(size))
        select resized).IsSucc;

    public override void CreateWorld(RhinoDoc doc, ViewInfo viewInfo, DisplayPipelineAttributes displayPipelineAttributes) =>
        ignore(bound.Bind(static plan => plan.Program.World).Match(
            Some: build => Observe(
                from document in DocKey.Of(doc, key)
                from concerns in Appearance.Of(displayPipelineAttributes, key)
                from built in Try.lift(() => build(new RealtimeWorld(document, viewInfo.Viewport.Id, concerns))).Run().Bind(static inner => inner)
                select built),
            None: () => { base.CreateWorld(doc, viewInfo, displayPipelineAttributes); return Fin.Succ(unit); }));

    public override bool IsRendererStarted() => lifecycle.Value is not RealtimeLifecycle.Idle;

    public override bool IsCompleted() => lifecycle.Value is RealtimeLifecycle.Settled;

    public override bool IsFrameBufferAvailable(ViewInfo view) =>
        lifecycle.Value is RealtimeLifecycle.Live or RealtimeLifecycle.Settled;

    public override int LastRenderedPass() => lifecycle.Value.Pass.Value;

    // --- [ENGINE_PARTICIPATION]
    public override bool UseFastDraw() => Policy(
        static policy => policy.Features.Admits(RealtimeFeature.FastDraw), base.UseFastDraw());

    public override int OpenGlVersion() => Policy(
        static policy => policy.Features.Admits(RealtimeFeature.OpenGl) ? base.OpenGlVersion() : 0, 0);

    public override bool ShowCaptureProgress() => bound.Bind(static plan => plan.Program.CaptureProgress).IsSome;

    public override double CaptureProgress() => bound.Bind(static plan => plan.Program.CaptureProgress).Match(
        Some: read => Observe(Try.lift(read).Run().Bind(static inner => inner)).Match(Succ: static value => value.Value, Fail: static _ => 0d),
        None: base.CaptureProgress);

    public override void SetView(ViewInfo view) {
        base.SetView(view);
        _ = Observe(
            from frame in Try.lift(() => Fin.Succ(new ViewFrame(View: view.Viewport.Id, Crc: ComputeViewportCrc(view)))).Run().Bind(static inner => inner)
            from _ in Fin.Succ(ignore(viewed.Swap(_ => Some(frame))))
            from bound in bound.Bind(static plan => plan.Program.Viewed)
                .TraverseM(seat => Try.lift(() => seat(frame)).Run().Bind(static inner => inner)).As().Map(static _ => unit)
            select bound);
    }

    // --- [HUD]
    public override string HudProductName() => Chrome(static chrome => chrome.ProductName.Resolve(), string.Empty);

    public override bool HudShow() => HudFlag(HudFeature.Show);

    public override bool HudShowControls() => HudFlag(HudFeature.Controls);

    public override bool HudShowPasses() => HudFlag(HudFeature.Passes);

    public override bool HudShowMaxPasses() => HudFlag(HudFeature.MaxPasses);

    public override bool HudAllowEditMaxPasses() => HudFlag(HudFeature.EditMaxPasses);

    public override bool HudShowCustomStatusText() => Chrome(static chrome => chrome.Status.IsSome, false);

    public override string HudCustomStatusText() => Chrome(
        chrome => chrome.Status.Match(
            Some: status => Observe(Try.lift(status).Run().Bind(static inner => inner)).Match(Succ: static text => text.Resolve(), Fail: static _ => string.Empty),
            None: static () => string.Empty),
        string.Empty);

    public override int HudLastRenderedPass() => LastRenderedPass();

    public override bool HudRendererPaused() => Paused;

    public override bool HudRendererLocked() => Locked;

    public override int HudMaximumPasses() => Policy(static policy => policy.MaxPasses.Value, base.HudMaximumPasses());

    public override DateTime HudStartTime() => Chrome(static chrome => chrome.Started, Option<Instant>.None)
        .Match(Some: static started => started.ToDateTimeUtc(), None: base.HudStartTime);

    private bool HudFlag(HudFeature feature) => Chrome(chrome => chrome.Features.Admits(feature), false);

    // --- [ENGINE_INTERNALS]
    private Fin<TOut> Bound<TOut>(Func<RealtimeProgram, Fin<TOut>> use) =>
        bound.ToFin(Fail: new RenderFault.Unbound(Member: nameof(Bound)))
            .Bind(plan => Try.lift(() => use(plan.Program)).Run().Bind(static inner => inner));

    private TOut Policy<TOut>(Func<RealtimePassPolicy, TOut> read, TOut fallback) =>
        Seated(plan => read(plan.Policy), fallback, member: nameof(Policy));

    private TOut Chrome<TOut>(Func<RealtimeChrome, TOut> read, TOut fallback) =>
        Seated(plan => read(plan.Chrome), fallback, member: nameof(Chrome));

    private TOut Seated<TOut>(Func<RealtimeEnginePlan, TOut> read, TOut fallback, string member) => bound.Match(
        Some: read,
        None: () => (Observe(new RenderFault.Unbound(Member: member)), fallback).Item2);

    private Fin<DrawTally> Project(
        RealtimeEnginePlan plan, DisplayPipeline pipeline, ConduitPhase phase, Func<ConduitFrame, Fin<Seq<DisplayMark>>> project) {
        RealtimeEngine self = this;
        return Observe(plan.Clock.Gauged<DrawTally, DispatchLane>(
                lane: DispatchLane.Paced,
                body: () => self.lifecycle.Value.Held.ToFin(Fail: new KernelFault.InvalidContext()).Bind(held =>
                    from frame in Fin.Succ(ConduitFrame.Of(pipeline, pipeline.Viewport, phase))
                    from marks in Try.lift(() => project(frame)).Run().Bind(static inner => inner)
                    from outcome in Marks.Paint(new Canvas.Pipeline(frame, held.Sprites), marks, self.key)
                    select outcome)))
            .Bind(measured => (
                HostEdge.SideWhen(measured.Span.Breached, () => ignore(self.Observe(new RenderFault.HostRefused(
                    Key: self.key, Member: nameof(Project), Detail: $"{phase.Key} overran {measured.Span.Overrun}")))),
                measured.Value).Item2)
            .Map(outcome => (outcome.Refused.Iter(cause => ignore(Observe(cause))), outcome).Item2);
    }

    private RealtimeSignal Signal() {
        RealtimeEngine self = this;
        return new RealtimeSignal(
            redraw: () => self.Observe(Try.lift(self.SignalRedraw).Run().Bind(static inner => inner)),
            step: ordinal => self.Step(held => ordinal.Match(
                Some: pass => held is RealtimeLifecycle.Live row
                    ? Some<RealtimeLifecycle>(row with { Pass = pass })
                    : Option<RealtimeLifecycle>.None,
                None: () => held is RealtimeLifecycle.Live settled
                    ? Some<RealtimeLifecycle>(new RealtimeLifecycle.Settled(settled.Pass))
                    : Option<RealtimeLifecycle>.None)));
    }

    private Transition<RealtimeLifecycle> Step(Func<RealtimeLifecycle, Option<RealtimeLifecycle>> step) =>
        Cell.Step(lifecycle, step, new KernelFault.InvalidContext());

    private Fin<Unit> Released(RealtimeLifecycle prior) => prior.Held.TraverseM(held => Custody.Release(
            Seq<Func<Fin<Unit>>>(
                () => Fin.Succ(held.Pixels.Close()),
                () => held.Sprites.Release()))).As().Map(static _ => unit);

    private Fin<Unit> Release(RealtimeLifecycle.Priming primed) => Custody.Release(
        Seq<Func<Fin<Unit>>>(
            () => Fin.Succ(primed.Pixels.Close()),
            () => primed.Sprites.Release()));

    private Fin<T> Observe<T>(Fin<T> result) {
        _ = result.IfFail(failure => ignore(faults.Park(item: failure)));
        return result;
    }

    private Unit Observe(Error failure) => ignore(faults.Park(item: failure));
}

public static class LightAuthorities {
    private static readonly SeatRegistry<LightAuthorityProgram> Seats = new();
    private static readonly AtomHashMap<Guid, LightAuthorityHost> Hosts = AtomHashMap(HashMap<Guid, LightAuthorityHost>());

    public static Seq<Error> Faults => Seats.Faults;
    public static long Shed => Seats.Shed;

    internal static Unit Seat(Guid engine, LightAuthorityHost host) =>
        Hosts.AddOrUpdate(engine, host);

    public static Fin<SeatToken> Register(Guid engine, LightAuthorityProgram program, PlugIn owner) {
        return from admitted in guard(program is { IsValid: true }, new KernelFault.InvalidInput(Axis: Some(nameof(program)))).ToFin()
               from plugin in Admit.Need(owner)
               from claimed in Seats.Claim(engine, program, () => Try.lift(() => Fin.Succ(
                   HostEdge.Side(() => LightManagerSupport.RegisterLightManager(plugin)))).Run().Bind(static inner => inner))
               select claimed.Token;
    }

    public static Fin<Unit> Unregister(Guid engine, SeatToken token) {
        return Seats.Retire(engine, token, () => Fin.Succ(Hosts.Remove(engine)));
    }

    public static Fin<Unit> Notify(DocumentSession session, Guid engine, LightChange change) {
        return from source in Optional(session).ToFin(Fail: new KernelFault.MissingContext())
               from move in Admit.Need(change)
               from host in Hosts.Find(engine).ToFin(Fail: new RenderFault.SeatAbsent(Engine: engine))
               from _ in Admit.Demand(
                   use: document => Try.lift(() => {
                       Light unread = default;
                       host.OnCustomLightEvent(document, move.Key, ref unread);
                       return Fin.Succ(value: unit);
                   }).Run().Bind(static inner => inner),
                   needs: [SessionNeed.Read])
               select unit;
    }

    internal static TOut Answer<TOut>(Guid engine, RhinoDoc document, Func<LightAuthorityProgram, DocKey, Fin<TOut>> body, TOut fallback) =>
        (from program in Seats.Find(engine).ToFin(Fail: new RenderFault.SeatAbsent(Key: Seats.Anchor, Engine: engine))
         from key in DocKey.Of(document, Seats.Anchor)
         from result in Try.lift(() => body(program)).Run().Bind(static inner => inner)
         select result).Match(
            Succ: static value => value,
            Fail: failure => (Seats.Observe(failure), fallback).Item2);
}

public abstract class LightAuthorityHost : LightManagerSupport {

    protected abstract Guid Plugin { get; }
    protected abstract Guid Engine { get; }

    public sealed override Guid PluginId() => Plugin;

    public sealed override Guid RenderEngineId() {
        _ = LightAuthorities.Seat(engine: Engine, host: this);
        return Engine;
    }

    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) {
        LightArray target = light_array;
        _ = LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Roster().Map(rows => rows.Fold(unit, (_, row) => {
                target.Append(row);
                return unit;
            })),
            fallback: unit);
    }

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) => HostEdge.Settle(
        slot: ref light,
        outcome: LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Resolve(uuid).Bind(found => found.ToFin(Fail: new KernelFault.InvalidResult())),
            fallback: Fin.Fail<Light>(new KernelFault.InvalidResult())));

    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Amend(light), fallback: unit);

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) => LightAuthorities.Answer(
        Engine, doc,
        (program, key) => FactoryBridge.Row<bool, LightRetirement>(candidate: bUndelete)
            .Bind(verb => program.Retire(key, light, verb))
            .Map(static _ => true),
        fallback: false);

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) {
        Light carrier = light;
        return LightAuthorities.Answer(Engine, doc, (program, key) => program.Serial(carrier), fallback: -1);
    }

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) {
        Light carrier = light;
        return LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Describe(carrier).Map(static text => text.Resolve()), fallback: string.Empty);
    }

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) {
        Seq<Light> edited = Drained(light_array);
        return LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Edit(edited).Map(static _ => true), fallback: false);
    }

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) {
        Seq<Light> grouped = Drained(light_array);
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Group(grouped), fallback: unit);
    }

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) {
        Seq<Light> grouped = Drained(light_array);
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Ungroup(grouped), fallback: unit);
    }

    public override bool SetLightSolo(RhinoDoc doc, Guid uuid_light, bool bSolo) => LightAuthorities.Answer(
        Engine, doc,
        (program, key) => program.Solo.Match(
            Some: solo => FactoryBridge.Row<bool, SwitchState>(candidate: bSolo)
                .Bind(state => solo.Set(key, uuid_light, state))
                .Map(static _ => true),
            None: () => Fin.Succ(base.SetLightSolo(doc, uuid_light, bSolo))),
        fallback: false);

    public override bool GetLightSolo(RhinoDoc doc, Guid uuid_light) => LightAuthorities.Answer(
        Engine, doc,
        (program, key) => program.Solo.Match(
            Some: solo => solo.Get(key, uuid_light).Map(static state => state.Enabled),
            None: () => Fin.Succ(base.GetLightSolo(doc, uuid_light))),
        fallback: false);

    public override int LightsInSoloStorage(RhinoDoc doc) => LightAuthorities.Answer(
        Engine, doc,
        (program, key) => program.Solo.Match(
            Some: solo => solo.Count(key).Map(static count => count.Value),
            None: () => Fin.Succ(base.LightsInSoloStorage(doc))),
        fallback: 0);

    private static Seq<Light> Drained(LightArray rows) =>
        toSeq(Enumerable.Range(0, rows.Count()).Select(rows.ElementAt));
}
```

## [04]-[POST_AND_TEXTURE]

- Owner: `PostEffectOp` closes settings edits over `RenderSettings.PostEffects`; `BuiltinEffect` closes built-in identity; `EffectProgram` closes the authored effect body and `EffectHost` adapts it onto the host abstract; `EffectPass` closes framebuffer access during one execution with `ChannelView` and `GpuHandle` as its two borrow ports; `PostEffectGate` closes per-render execution policy; `TextureBake` closes live-evaluator and baked-simulation modes over `ContentRef` and object identity.
- Entry: `Effects.Configure(DocumentSession, Seq<PostEffectOp>) : Fin<EffectRoster>` owns the document-scoped settings batch and `Effects.Register(EffectSource) : Fin<EffectRegistry>` the process-global one-shot admission; the argument shape alone discriminates them, and registration never opens a document demand because the host installs TYPES, not rows. `TextureBake.Evaluate` is the third entry, resolving its content inside its own demand.
- Law: configuration and execution never merge — `PostEffectOp` writes the settings-side rows the pipeline reads at render time, `PostEffectGate` decides per-render execution on a window, and the two host catalogs draw the same line. A mutating batch demands `SessionNeed.Mutate`, a census-only batch `SessionNeed.Read`, and the batch's own case shapes derive the need set, never a caller flag.
- Law: the collection owns every row. `PostEffectCollection` is the disposable native scoped to the demand window, while a `PostEffectData` is a NON-OWNING cursor whose pointer re-resolves through the collection on every member read and whose disposal is inert — so an arm holds rows for the whole window, disposes none, and a `using` over one asserts custody the type does not carry. Admission is the cursor's first member read, which throws on an unknown id, never a null-probe on a factory that cannot answer null.
- Law: an effect row's display state is ONE `CapabilitySet<EffectDisplay>` column carrying the two axes the settings cursor publishes. All four corners are LEGAL by host truth — an effect shown but off is a roster entry a user can enable, an effect on but hidden is a forced pass — so the set carries no `CapabilityLaw` and the absence is stated rather than filled with a vacuous roster. Stage SELECTION is a roster fact, not a row fact: the host answers it per stage through `GetSelectedPostEffect`, so it rides `EffectRoster.Selected` and never a third bit on a row.
- Law: a parameter name is `EffectField` and a parameter value is `EffectValue`, never a bare `string` and never `object`. Host truth bounds the vocabulary: `GetParameter` answers `IConvertible`, which carries no colour, so a colour parameter rides its packed ARGB word and `PerceptualColor` is the one owner on both legs — the erased `object` the old surface threaded past its boundary had no reader naming what it held.
- Law: `EffectHost` is the whole authoring adapter and every host hook forwards to the program — parameter read and write, state IO, UI-section seating, reset, and help all reach `EffectProgram` rows, so a hard-coded `false` is the deleted form. A parameter WRITE and a factory reset both bracket the host content-change protocol (`BeginChange`/`EndChange` around the mutation, `Changed` after it), because an effect that mutates without announcing leaves every dependent content stale.
- Law: the gate is armed by the DECLARATION, not the registration — only an effect whose `[CustomPostEffect]` carries `UseExecutionControl` hands its per-frame decision to a registered control, so `EffectHost.Timing` reads the host's own `ExecuteWhileRenderingOption` back as an `EffectTiming` row and a consumer proves which effects a gate decides for. Every other timing runs on the host's own cadence.
- Law: `BuiltinEffect` is the built-in identity vocabulary and `EffectId` seeds from its rows, never from a bare `Guid` literal — each row defers its `PostEffectUuids` read behind `[UseDelegateFromConstructor]` because those members are get-only properties over a native id table, so an eager field reference resolves them at type init before the host RDK is up. The roster is READ on every census row: `EffectFact.Builtin` resolves through `Named`, so a consumer distinguishes a shipped effect from a plug-in's own without a second table.
- Law: a channel borrow is a `ChannelLease` case, never a write-plus-commit bool pair — the illegal `(write: false, commit: true)` corner is unrepresentable, and `Commit` alone replaces the written channel's id in the chain, so a failed body leaves the prior channel standing rather than silently vanishing from the chain.
- Law: `ChannelView` is EXTENT-BOUND. Host truth: `GetValues` validates its rectangle against the framebuffer and FAILS when it does not fit, while `GetValue`/`SetValue`/`AddValue` validate NEITHER coordinate — so every per-pixel member gates the offset against the view's own extent first and the read runs through the validating member over a one-pixel rectangle. The pass's `ChannelOrder` sizes every buffer, so the zero-length array once handed to a host `ref`-fill and the hard-coded `RGBA` order beside a seven-row vocabulary both delete.
- Law: the colour crossing is PUBLISHED here. `Read` answers the framebuffer's own linear quad and `Sample` answers `PerceptualColor.OfHost(quad, RgbTransfer.Linear)`; `Write` and `Accumulate` overload on the value they take, the perceptual arm quantizing through `ToColor4f(GamutPolicy.Unbounded, RgbTransfer.Linear)`. Two names, two regimes — a hot loop pays nothing and a consumer wanting perceptual math composes the kernel instead of hand-rolling opponent math over floats.
- Law: the GPU frame buffer is post-effect territory alone — `PostEffectChannel.GPU()` is the one managed producer of a texture handle, so `EffectPass.Handle` reads it inside the execute window under `GpuAllowed`, closes it on that window, and `CopyDown` is the only route from a texture back to per-pixel values. `Advance` reports row progress and a refused report is the user's cancel, which halts the pixel loop through the carrier.
- Law: live-versus-baked is the union's discriminant, selected by the texture's own capability — a consumer asks for live first and falls to the baked case on refusal, and the fallback is a case transition, never a silent quality change. A live evaluator is INITIALIZED before the body sees it, because the host publishes `Initialize` as a separate verdict and an uninitialized evaluator answers colours no sampler measured; the bake takes the host's RETURN-shaped `SimulatedTexture` sibling, so the `ref`-fill and its `null!` seed have no spelling left.
- Boundary: `PostEffectPipeline`, `PostEffectChannel`, `RenderWindow.Channel`, and `RenderWindow.ChannelGPU` stay inside `EffectPass`; an authored body BORROWS a `ChannelView` or `GpuHandle` for the length of the host bracket and only its own computed value crosses out — a texture id or pixel port that outlives the bracket names freed native memory, so neither port is a detached result. `TextureBake.Evaluate` disposes evaluator or simulation before detached egress.
- Packages: `api-rhinocommon-render.md` (`PostEffect`, `PostEffectPipeline`, `PostEffectChannel`, `PostEffectState`, `PostEffectUI`, `CustomPostEffectAttribute`, `PostEffectUuids`, `PostEffectExecutionControl`, `RenderTexture`, `TextureEvaluator`, `RenderWindow.Channel`/`ChannelGPU`); `api-rhinocommon-rendersettings.md` (`PostEffectCollection`, `PostEffectData` cursor semantics); `api-rhinocommon-rendercontent.md` (`SimulatedTexture`, `RenderContent.ChangeContexts`); `api-rhinocommon-display.md` (`DisplayTechnology`); kernel `Numerics/atoms` (`PerceptualColor`, `RgbTransfer`, `GamutPolicy`, `UnitInterval`, `Dimension`), `Domain/validation` (`CapabilitySet`, `FactoryBridge.Row`, `HostEdge.Settle`), `Domain/results` (`Lease`); `Render/content.md` (`ContentRef`, `ChangeReason`); kernel `Domain/results` (`Custody`); NodaTime (`Duration`).

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<PostEffects.PostEffectType>]
public sealed partial class EffectStage {
    public static readonly EffectStage Early = new(key: PostEffects.PostEffectType.Early);
    public static readonly EffectStage ToneMapping = new(key: PostEffects.PostEffectType.ToneMapping);
    public static readonly EffectStage Late = new(key: PostEffects.PostEffectType.Late);
}

[SmartEnum<PostEffects.PostEffectExecuteWhileRenderingOptions>]
public sealed partial class EffectTiming {
    public static readonly EffectTiming Never = new(key: PostEffects.PostEffectExecuteWhileRenderingOptions.Never, gated: false);
    public static readonly EffectTiming Always = new(key: PostEffects.PostEffectExecuteWhileRenderingOptions.Always, gated: false);
    public static readonly EffectTiming Delayed = new(key: PostEffects.PostEffectExecuteWhileRenderingOptions.UseDelay, gated: false);
    public static readonly EffectTiming Controlled = new(key: PostEffects.PostEffectExecuteWhileRenderingOptions.UseExecutionControl, gated: true);

    public bool Gated { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class EffectDisplay : ICapability<EffectDisplay> {
    public static readonly EffectDisplay On = new(key: "on");
    public static readonly EffectDisplay Shown = new(key: "shown");
}

[SmartEnum<Rhino.Display.DisplayTechnology>]
public sealed partial class GpuTechnology {
    public static readonly GpuTechnology Absent = new(key: Rhino.Display.DisplayTechnology.None);
    public static readonly GpuTechnology OpenGl = new(key: Rhino.Display.DisplayTechnology.OpenGL);
    public static readonly GpuTechnology Metal = new(key: Rhino.Display.DisplayTechnology.Metal);
    public static readonly GpuTechnology DirectX = new(key: Rhino.Display.DisplayTechnology.DirectX);
    public static readonly GpuTechnology Software = new(key: Rhino.Display.DisplayTechnology.Software);
    public static readonly GpuTechnology Vulkan = new(key: Rhino.Display.DisplayTechnology.Vulkan);
}

[SmartEnum<int>]
public sealed partial class BuiltinEffect {
    public static readonly BuiltinEffect Glare = new(key: 0, static () => PostEffects.PostEffectUuids.Glare);
    public static readonly BuiltinEffect Bloom = new(key: 1, static () => PostEffects.PostEffectUuids.Bloom);
    public static readonly BuiltinEffect Glow = new(key: 2, static () => PostEffects.PostEffectUuids.Glow);
    public static readonly BuiltinEffect Fog = new(key: 3, static () => PostEffects.PostEffectUuids.Fog);
    public static readonly BuiltinEffect DepthOfField = new(key: 4, static () => PostEffects.PostEffectUuids.DepthOfField);
    public static readonly BuiltinEffect Multiplier = new(key: 5, static () => PostEffects.PostEffectUuids.Multiplier);
    public static readonly BuiltinEffect Noise = new(key: 6, static () => PostEffects.PostEffectUuids.Noise);
    public static readonly BuiltinEffect GaussianBlur = new(key: 7, static () => PostEffects.PostEffectUuids.GaussianBlur);
    public static readonly BuiltinEffect WireframePoints = new(key: 8, static () => PostEffects.PostEffectUuids.WireframePointsRGBA);
    public static readonly BuiltinEffect WireframeCurves = new(key: 9, static () => PostEffects.PostEffectUuids.WireframeCurvesRGBA);
    public static readonly BuiltinEffect WireframeIsocurves = new(key: 10, static () => PostEffects.PostEffectUuids.WireframeIsocurvesRGBA);
    public static readonly BuiltinEffect WireframeAnnotations = new(key: 11, static () => PostEffects.PostEffectUuids.WireframeAnnotationsRGBA);
    public static readonly BuiltinEffect ClampTone = new(key: 12, static () => PostEffects.PostEffectUuids.ToneMapper_Clamp);
    public static readonly BuiltinEffect BlackWhitePointTone = new(key: 13, static () => PostEffects.PostEffectUuids.ToneMapper_BlackWhitePoint);
    public static readonly BuiltinEffect LogarithmicTone = new(key: 14, static () => PostEffects.PostEffectUuids.ToneMapper_Logarithmic);
    public static readonly BuiltinEffect FalseColorTone = new(key: 15, static () => PostEffects.PostEffectUuids.ToneMapper_FalseColor);
    public static readonly BuiltinEffect FilmicTone = new(key: 16, static () => PostEffects.PostEffectUuids.ToneMapper_Filmic);
    public static readonly BuiltinEffect Gamma = new(key: 17, static () => PostEffects.PostEffectUuids.Gamma);
    public static readonly BuiltinEffect Dithering = new(key: 18, static () => PostEffects.PostEffectUuids.Dithering);
    public static readonly BuiltinEffect Watermark = new(key: 19, static () => PostEffects.PostEffectUuids.Watermark);
    public static readonly BuiltinEffect HueSatLum = new(key: 20, static () => PostEffects.PostEffectUuids.HueSatLum);
    public static readonly BuiltinEffect BrightnessContrast = new(key: 21, static () => PostEffects.PostEffectUuids.BriCon);

    [UseDelegateFromConstructor]
    internal partial Guid Uuid();

    public Fin<EffectId> Address() => Try.lift(() => Fin.Succ(EffectId.Create(Uuid()))).Run().Bind(static inner => inner);

    internal static Option<BuiltinEffect> Named(EffectId effect) =>
        toSeq(Items).Find(row => row.Uuid() == effect.Value);
}

[ValueObject<string>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[ValidationError]
public readonly partial struct EffectField {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref string value) {
        validationError = string.IsNullOrWhiteSpace(value) ? new ValidationError(string.Join(" | ", new object?[] { nameof(EffectField) })) : null;
        value = value?.Trim() ?? string.Empty;
    }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EffectValue {
    private EffectValue() { }
    public sealed record Number(double Value) : EffectValue;
    public sealed record Whole(int Value) : EffectValue;
    public sealed record Flag(SwitchState Value) : EffectValue;
    public sealed record Text(HostText Value) : EffectValue;
    public sealed record Colour(PerceptualColor Value) : EffectValue;

    internal Fin<object> Native() => Switch(
        number: static row => Fin.Succ<object>(row.Value),
        whole: static row => Fin.Succ<object>(row.Value),
        flag: static row => Fin.Succ<object>(row.Value.Enabled),
        text: static row => Fin.Succ<object>(row.Value.Resolve()),
        colour: static (row) => row.Value.ToArgb().Map(static packed => (object)packed));

    internal static Fin<EffectValue> Of(IConvertible held) => held switch {
        bool flag => FactoryBridge.Row<bool, SwitchState>(candidate: flag).Map(static row => (EffectValue)new Flag(row)),
        int whole => Fin.Succ<EffectValue>(new Whole(whole)),
        double number => Fin.Succ<EffectValue>(new Number(number)),
        float number => Fin.Succ<EffectValue>(new Number(number)),
        string text => Acceptance.Text(value: text).Bind(admitted => Try.lift(() =>
            Fin.Succ<EffectValue>(new Text(HostText.Create(english: admitted, context: 0)))).Run().Bind(static inner => inner)),
        _ => Fin.Fail<EffectValue>(new RenderFault.HostRefused(Member: nameof(PostEffects.PostEffectData.GetParameter), Detail: held.GetType().Name)),
    };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record GpuHandle {
    private GpuHandle() { }
    public sealed record OpenGlCase(uint Texture, Size2i Extent, Rasm.Numerics.Dimension PixelSize) : GpuHandle;
    public sealed record MetalCase(nint Texture, Size2i Extent, Rasm.Numerics.Dimension PixelSize) : GpuHandle;
    public sealed record UnbackedCase(GpuTechnology Technology) : GpuHandle;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record ChannelLease {
    private ChannelLease() { }
    internal sealed record Reading : ChannelLease;
    internal sealed record Writing : ChannelLease;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record EffectSource {
    private EffectSource() { }
    public sealed record PlugInCase(PlugIn Owner) : EffectSource;
    public sealed record AssemblyCase(System.Reflection.Assembly Source, Guid PlugInId) : EffectSource;

    internal Fin<Seq<Type>> Install() => Switch(
        plugInCase: static (row) => Try.lift(() =>
            Optional(PostEffects.PostEffect.RegisterPostEffect(plugin: row.Owner))
                .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(PostEffects.PostEffect.RegisterPostEffect), Detail: "no type set"))
                .Map(static types => toSeq(types))).Run().Bind(static inner => inner),
        assemblyCase: static (row) =>
            from _ in guard(row.PlugInId != Guid.Empty, (Error)new KernelFault.InvalidValue(nameof(row.PlugInId), "a non-empty plug-in identity")).ToFin()
            from types in Try.lift(() =>
                Optional(PostEffects.PostEffect.RegisterPostEffect(assembly: row.Source, pluginId: row.PlugInId))
                    .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(PostEffects.PostEffect.RegisterPostEffect), Detail: "no type set"))
                    .Map(static registered => toSeq(registered))).Run().Bind(static inner => inner)
            select types);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PostEffectOp : IValidityEvidence {
    private PostEffectOp() { }
    public sealed record CensusCase : PostEffectOp;
    public sealed record DisplayCase(EffectId Effect, CapabilitySet<EffectDisplay> State) : PostEffectOp;
    public sealed record ReorderCase(EffectId Move, Option<EffectId> Before) : PostEffectOp;
    public sealed record SelectCase(EffectStage Stage, EffectId Effect) : PostEffectOp;
    public sealed record TuneCase(EffectId Effect, EffectField Field, EffectValue Value) : PostEffectOp;

    public bool IsValid => Switch(
        censusCase: static _ => true,
        displayCase: static _ => true,
        reorderCase: static row => ValidityClaim.All(row.Before.Map(before => before != row.Move).IfNone(true)),
        selectCase: static _ => true,
        tuneCase: static row => ValidityClaim.All(row.Value is not null));

    internal bool Mutates => this is not CensusCase;

    internal Fin<Unit> Apply(PostEffects.PostEffectCollection collection) => Switch(
        state: collection,
        censusCase: static (_, _) => Fin.Succ(value: unit),
        displayCase: static (ctx, row) => Data(ctx, row.Effect).Bind(data => Try.lift(() => {
            data.On = row.State.Admits(EffectDisplay.On);
            data.Shown = row.State.Admits(EffectDisplay.Shown);
            return Fin.Succ(value: unit);
        }).Run().Bind(static inner => inner)),
        reorderCase: static (ctx, row) => Try.lift(() => Admit.Confirm(success: ctx.MovePostEffectBefore(
            id_move: row.Move.Value,
            id_before: row.Before.Map(static before => before.Value).IfNone(Guid.Empty)))).Run().Bind(static inner => inner),
        selectCase: static (ctx, row) => Try.lift(() =>
            ctx.SetSelectedPostEffect(type: row.Stage.Key, id: row.Effect.Value)).Run().Bind(static inner => inner),
        tuneCase: static (ctx, row) => from data in Data(ctx, row.Effect)
                                       from native in row.Value.Native()
                                       from written in Try.lift(() => Admit.Confirm(
                                           success: data.SetParameter(param_name: row.Field.Value, param_value: native))).Run().Bind(static inner => inner)
                                       select written);

    private static Fin<PostEffects.PostEffectData> Data(PostEffects.PostEffectCollection collection, EffectId effect) =>
        Try.lift(() => {
            PostEffects.PostEffectData cursor = collection.PostEffectDataFromId(id: effect.Value);
            return Fin.Succ((ignore(cursor.Id), cursor).Item2);
        }).Run().Bind(static inner => inner);
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class TextureEvaluation : ICapability<TextureEvaluation> {
    public static readonly TextureEvaluation Filtering = new(key: "filtering", native: RenderTexture.TextureEvaluatorFlags.DisableFiltering);
    public static readonly TextureEvaluation LocalMapping = new(key: "local-mapping", native: RenderTexture.TextureEvaluatorFlags.DisableLocalMapping);
    public static readonly TextureEvaluation Adjustment = new(key: "adjustment", native: RenderTexture.TextureEvaluatorFlags.DisableAdjustment);
    public static readonly TextureEvaluation ProjectionChange = new(key: "projection-change", native: RenderTexture.TextureEvaluatorFlags.DisableProjectionChange);

    internal RenderTexture.TextureEvaluatorFlags Native { get; }
}

[SmartEnum<RenderTexture.TextureGeneration>]
public sealed partial class TextureGenerationUse {
    public static readonly TextureGenerationUse Allow = new(key: RenderTexture.TextureGeneration.Allow);
    public static readonly TextureGenerationUse Disallow = new(key: RenderTexture.TextureGeneration.Disallow);
    public static readonly TextureGenerationUse Skip = new(key: RenderTexture.TextureGeneration.Skip);
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct EffectFact(
    EffectId Id,
    EffectStage Stage,
    HostText Name,
    CapabilitySet<EffectDisplay> Display,
    Option<BuiltinEffect> Builtin,
    uint Digest);

public sealed record EffectRoster(Seq<EffectFact> Rows, HashMap<EffectStage, EffectId> Selected) : IDetachedDocumentResult;

public sealed record EffectRegistry(Seq<Type> Registered) : IDetachedDocumentResult;

public sealed record EffectProgram(
    Func<EffectPass, Fin<Unit>> Execute,
    Func<EffectStateBag, Fin<Unit>> Read,
    Func<EffectStateBag, Fin<Unit>> Write,
    Func<Fin<Unit>> Reset,
    CapabilitySet<RenderChannel> Required,
    Func<EffectField, Fin<Option<EffectValue>>> Param,
    Func<EffectField, EffectValue, Fin<Unit>> Tune,
    Option<Func<EffectPass, Fin<bool>>> Admits = default,
    Option<Func<PostEffects.PostEffectUI, Fin<Unit>>> Sections = default,
    Option<Func<Fin<Unit>>> Help = default) : IValidityEvidence {
    public bool IsValid => ValidityClaim.All(
        Execute is not null, Read is not null, Write is not null, Reset is not null,
        Param is not null, Tune is not null, !Required.Held.Count.Equals(0));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureBake : IValidityEvidence {
    private TextureBake() { }
    public sealed record LiveCase(ContentRef Texture, CapabilitySet<TextureEvaluation> Suppressed) : TextureBake;
    public sealed record BakedCase(ContentRef Texture, TextureGenerationUse Generation, Rasm.Numerics.Dimension Size, Guid Subject) : TextureBake;

    public bool IsValid => Switch(
        liveCase: static row => ValidityClaim.All(row.Texture is not null),
        bakedCase: static row => ValidityClaim.All(row.Texture is not null, row.Size.Value > 0, row.Subject != Guid.Empty));

    public Fin<TOut> Evaluate<TOut>(
        DocumentSession session,
        Func<TextureEvaluator, Fin<TOut>> live,
        Func<SimulatedTexture, Fin<TOut>> baked)
        where TOut : IDetachedDocumentResult {
        TextureBake self = this;
        return guard(session is not null && live is not null && baked is not null && IsValid, new KernelFault.InvalidInput()).ToFin()
            .Bind(_ => Admit.Demand(
                use: document => self.Switch(
                    state: (Document: document, Live: live, Baked: baked),
                    liveCase: static (ctx, bake) =>
                        from texture in Texture(bake.Texture, ctx.Document)
                        from result in Try.lift(() => {
                            RenderTexture.TextureEvaluatorFlags flags = toSeq(bake.Suppressed.Held).Fold(
                                RenderTexture.TextureEvaluatorFlags.Normal,
                                static (word, row) => word | row.Native);
                            using TextureEvaluator evaluator = texture.CreateEvaluator(flags);
                            return Optional(evaluator)
                                .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(RenderTexture.CreateEvaluator), Detail: bake.Suppressed.Wire))
                                .Bind(held => Admit.Confirm(success: held.Initialize()).Map(_ => held))
                                .Bind(ctx.Live);
                        }).Run().Bind(static inner => inner)
                        select result,
                    bakedCase: static (ctx, bake) =>
                        from texture in Texture(bake.Texture, ctx.Document)
                        from subject in Optional(ctx.Document.Objects.FindId(bake.Subject)).ToFin(new KernelFault.InvalidInput())
                        from result in Try.lift(() => {
                            using SimulatedTexture? simulated = texture.SimulatedTexture(
                                tg: bake.Generation.Key, size: bake.Size.Value, obj: subject);
                            return Optional(simulated)
                                .ToFin(Fail: new RenderFault.HostRefused(Member: nameof(RenderTexture.SimulatedTexture), Detail: bake.Generation.Key.ToString()))
                                .Bind(ctx.Baked);
                        }).Run().Bind(static inner => inner)
                        select result),
                needs: [SessionNeed.Read]));
    }

    private static Fin<RenderTexture> Texture(ContentRef address, RhinoDoc document) =>
        from content in address.Resolve(document)
        from texture in content is RenderTexture value
            ? Fin.Succ(value)
            : Fin.Fail<RenderTexture>(new RenderFault.HostRefused(Member: nameof(ContentRef.Resolve), Detail: content.GetType().Name))
        select texture;
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class ChannelView {
    private readonly RenderWindow.Channel channel;

    internal ChannelView(RenderWindow.Channel channel, Size2i extent, ChannelOrder order) =>
        (this.channel, Extent, Order, this.key) = (channel, extent, order);

    public Size2i Extent { get; }
    public ChannelOrder Order { get; }

    public Fin<Color4f> Read(Offset2i at) =>
        Block(origin: at, extent: Size2i.Create(width: 1, height: 1)).Bind(values => values.Span switch {
            [float r, float g, float b, float a] => Fin.Succ(new Color4f(r, g, b, a)),
            [float grey] => Fin.Succ(new Color4f(grey, grey, grey, 1.0f)),
            [float r, float g, float b] => Fin.Succ(new Color4f(r, g, b, 1.0f)),
            _ => Fin.Fail<Color4f>(new RenderFault.HostRefused(Member: nameof(RenderWindow.Channel.GetValues), Detail: Order.Key.ToString())),
        });

    public Fin<PerceptualColor> Sample(Offset2i at) =>
        Read(at).Bind(quad => PerceptualColor.OfHost(host: quad, transfer: RgbTransfer.Linear));

    public Fin<Unit> Write(Offset2i at, Color4f value) =>
        Within(at).Bind(_ => Try.lift(() => Fin.Succ(HostEdge.Side(() => channel.SetValue(x: at.X, y: at.Y, value: value)))).Run().Bind(static inner => inner));

    public Fin<Unit> Write(Offset2i at, PerceptualColor value) => Lowered(value).Bind(quad => Write(at, quad));

    public Fin<Unit> Accumulate(Offset2i at, Color4f value) =>
        Within(at).Bind(_ => Try.lift(() => Fin.Succ(HostEdge.Side(() => channel.AddValue(x: at.X, y: at.Y, value: value)))).Run().Bind(static inner => inner));

    public Fin<Unit> Accumulate(Offset2i at, PerceptualColor value) => Lowered(value).Bind(quad => Accumulate(at, quad));

    public Fin<ReadOnlyMemory<float>> Block(Offset2i origin, Size2i extent) =>
        from _ in Within(origin)
        from __ in guard(
            origin.X + extent.Width <= Extent.Width && origin.Y + extent.Height <= Extent.Height,
            new KernelFault.InvalidInput(Axis: Some(nameof(extent))))
        from values in Try.lift(() => {
            float[] buffer = new float[checked(extent.Width * extent.Height * Order.Components)];
            channel.GetValues(
                rectangle: origin.Window(extent: extent),
                stride: extent.Width,
                componentOrder: Order.Native,
                values: ref buffer);
            return Fin.Succ<ReadOnlyMemory<float>>(buffer);
        }).Run().Bind(static inner => inner)
        select values;

    public Fin<(float Low, float High)> Span => Try.lift(() => {
        channel.GetMinMaxValues(min: out float low, max: out float high);
        return Fin.Succ(value: (Low: low, High: high));
    }).Run().Bind(static inner => inner);

    private Fin<Unit> Within(Offset2i at) => guard(
        at.X < Extent.Width && at.Y < Extent.Height,
        new KernelFault.InvalidInput(Axis: Some(nameof(at)))).ToFin();

    private Fin<Color4f> Lowered(PerceptualColor value) =>
        value.ToColor4f(gamut: GamutPolicy.Unbounded, transfer: RgbTransfer.Linear);
}

public sealed class EffectStateBag {
    private readonly PostEffects.PostEffectState state;

    internal EffectStateBag(PostEffects.PostEffectState state) => (this.state, this.key) = (state);

    public Fin<Option<T>> Read<T>(EffectField field) => Try.lift(() =>
        Fin.Succ(Admit.Probe<T>((out T held) => state.TryGetValue(name: field.Value, vValue: out held)))).Run().Bind(static inner => inner);

    public Fin<Unit> Write<T>(EffectField field, T value) =>
        Try.lift(() => Admit.Confirm(success: state.SetValue(name: field.Value, vValue: value))).Run().Bind(static inner => inner);
}

public sealed class EffectPass {
    private readonly PostEffects.PostEffectPipeline pipeline;

    internal EffectPass(PostEffects.PostEffectPipeline pipeline, Offset2i origin, Size2i extent) =>
        (this.pipeline, Origin, Extent, this.key) = (pipeline, origin, extent);

    public Offset2i Origin { get; }
    public Size2i Extent { get; }
    public Fin<Size2i> Frame => Try.lift(() =>
        pipeline.Dimensions() is var frame ? Size2i.Of(width: frame.Width, height: frame.Height) : Fin.Fail<Size2i>(new KernelFault.InvalidResult())).Run().Bind(static inner => inner);
    public bool GpuAllowed => pipeline.GPUAllowed;
    public bool Rendering => pipeline.IsRendering;
    public Fin<Guid> Session => Try.lift(() => Fin.Succ(pipeline.RenderingId)).Run().Bind(static inner => inner);
    public Fin<float> PeakLuminance => Try.lift(() => Fin.Succ(value: pipeline.GetMaxLuminance())).Run().Bind(static inner => inner);
    public Fin<Seq<EffectId>> Order => Try.lift(() => Fin.Succ(value: toSeq(pipeline.ExecutionOrder()).Map(EffectId.Create))).Run().Bind(static inner => inner);

    public Fin<Duration> Elapsed => Try.lift(() => Fin.Succ(Duration.FromMilliseconds(
        (double)pipeline.GetEndTimeInMilliseconds() - pipeline.GetStartTimeInMilliseconds()))).Run().Bind(static inner => inner);

    public Fin<Unit> Restart(Duration at) {
        return guard(at >= Duration.Zero, new KernelFault.InvalidInput(Axis: Some(nameof(at)))).ToFin().Bind(_ => Try.lift(() =>
            Fin.Succ(HostEdge.Side(() => pipeline.SetStartTimeInMilliseconds(
                checked((ulong)at.TotalMilliseconds))))).Run().Bind(static inner => inner));
    }

    public Fin<TOut> Read<TOut>(RenderChannel channel, Func<ChannelView, Fin<TOut>> borrow) =>
        Borrowed(channel, new ChannelLease.Reading(), borrow, key ?? this.key);

    public Fin<Unit> Write(RenderChannel channel, Func<ChannelView, Fin<Unit>> body) =>
        Borrowed(channel, new ChannelLease.Writing(), body, key ?? this.key);

    public Fin<TOut> Handle<TOut>(RenderChannel channel, Func<GpuHandle, Fin<TOut>> borrow) {
        return guard(GpuAllowed, new KernelFault.InvalidContext()).ToFin().Bind(_ => Try.lift(() => {
            using PostEffects.PostEffectChannel port = pipeline.GetChannelForRead(id: channel.Id);
            return Optional(port).ToFin(Fail: new KernelFault.MissingContext()).Bind(active => {
                using RenderWindow.ChannelGPU? texture = active.GPU();
                return Optional(texture).ToFin(Fail: new KernelFault.InvalidResult()).Bind(held =>
                    from technology in FactoryBridge.Row<Rhino.Display.DisplayTechnology, GpuTechnology>(candidate: held.DisplayTechnology)
                    from extent in Size2i.Of(width: held.Width(), height: held.Height())
                    let size = Rasm.Numerics.Dimension.Create(value: checked((int)held.PixelSize()))
                    from taken in borrow(technology.Key switch {
                        Rhino.Display.DisplayTechnology.OpenGL =>
                            (GpuHandle)new GpuHandle.OpenGlCase(Texture: held.TextureHandleOpenGL(), Extent: extent, PixelSize: size),
                        Rhino.Display.DisplayTechnology.Metal =>
                            new GpuHandle.MetalCase(Texture: held.TextureHandleMetal(), Extent: extent, PixelSize: size),
                        _ => new GpuHandle.UnbackedCase(Technology: technology),
                    })
                    select taken);
            });
        }).Run().Bind(static inner => inner));
    }

    public Fin<Unit> CopyDown(RenderChannel channel) {
        return guard(GpuAllowed, new KernelFault.InvalidContext()).ToFin().Bind(_ => Try.lift(() => {
            using PostEffects.PostEffectChannel source = pipeline.GetChannelForRead(id: channel.Id);
            using PostEffects.PostEffectChannel sink = pipeline.GetChannelForWrite(id: channel.Id);
            return from live in Optional(source).ToFin(Fail: new KernelFault.MissingContext())
                   from target in Optional(sink).ToFin(Fail: new KernelFault.MissingContext())
                   from texture in Optional(live.GPU()).ToFin(Fail: new KernelFault.InvalidResult())
                   from pixels in Optional(target.CPU()).ToFin(Fail: new KernelFault.InvalidResult())
                   from _copied in Try.lift(() => HostEdge.Side(() => { using (texture) using (pixels) { texture.CopyTo(channel: pixels); } })).Run().Bind(static inner => inner)
                   from _committed in Try.lift(() => HostEdge.Side(target.Commit)).Run().Bind(static inner => inner)
                   select unit;
        }).Run().Bind(static inner => inner));
    }

    public Fin<Unit> Advance(Rasm.Numerics.Dimension rows) {
        return Try.lift(() => HostEdge.Side(() => ((IProgress<int>)pipeline).Report(value: rows.Value))).Run().Bind(static inner => inner);
    }

    private Fin<TOut> Borrowed<TOut>(RenderChannel channel, ChannelLease lease, Func<ChannelView, Fin<TOut>> borrow) =>
        Frame.Bind(frame => Try.lift(() => {
            using PostEffects.PostEffectChannel port = lease.Switch(
                state: (Pipeline: pipeline, Channel: channel),
                reading: static (ctx, _) => ctx.Pipeline.GetChannelForRead(id: ctx.Channel.Id),
                writing: static (ctx, _) => ctx.Pipeline.GetChannelForWrite(id: ctx.Channel.Id));
            return Optional(port).ToFin(Fail: new KernelFault.MissingContext()).Bind(active => {
                using RenderWindow.Channel? pixels = active.CPU();
                return Optional(pixels).ToFin(Fail: new KernelFault.InvalidResult())
                    .Bind(view => borrow(new ChannelView(channel: view, extent: frame, order: channel.Order())))
                    .Bind(done => lease is ChannelLease.Writing
                        ? Try.lift(() => HostEdge.Side(active.Commit)).Run().Bind(static inner => inner).Map(_ => done)
                        : Fin.Succ(value: done));
            });
        }).Run().Bind(static inner => inner));
}

public abstract class EffectHost : PostEffects.PostEffect {
    private readonly EffectProgram program;
    private readonly Ring<Error> faults = new(cap: DisplayFaults.Cap);

    protected EffectHost(EffectProgram program) => this.program = program;

    public Seq<Error> Faults => faults.Parked;
    public long Shed => faults.Shed;

    public EffectTiming Timing => Observe(
        FactoryBridge.Row<PostEffects.PostEffectExecuteWhileRenderingOptions, EffectTiming>(candidate: ExecuteWhileRenderingOption),
        EffectTiming.Always);

    public override Guid[] RequiredChannels => toSeq(program.Required.Held).Map(static row => row.Id).ToArray();

    public override bool CanExecute(PostEffects.PostEffectPipeline pipeline) => program.Admits.Match(
        Some: admits => Accept(Frame(pipeline).Bind(rect => Pass(pipeline, rect)).Bind(admits)),
        None: () => true);

    public override bool Execute(PostEffects.PostEffectPipeline pipeline, System.Drawing.Rectangle rect) =>
        Accept(Pass(pipeline, rect).Bind(pass => program.Execute(pass).Map(static _ => true)));

    public override bool ReadState(PostEffects.PostEffectState state) =>
        Accept(Try.lift(() => program.Read(new EffectStateBag(state: state))).Run().Bind(static inner => inner).Map(static _ => true));

    public override bool WriteState(ref PostEffects.PostEffectState state) {
        PostEffects.PostEffectState held = state;
        return Accept(Try.lift(() => program.Write(new EffectStateBag(state: held))).Run().Bind(static inner => inner).Map(static _ => true));
    }

    public override void ResetToFactoryDefaults() =>
        ignore(Accept(Amended(ChangeReason.Program, () => Try.lift(program.Reset).Run().Bind(static inner => inner)).Map(static _ => true)));

    public override bool GetParam(string param, ref object v) {
        Fin<object> resolved =
            from field in FactoryBridge.Accept<EffectField>(candidate: param)
            from held in Try.lift(() => program.Param(field)).Run().Bind(static inner => inner)
            from value in held.ToFin(Fail: new KernelFault.InvalidResult(Detail: Some(param)))
            from native in value.Native()
            select native;
        _ = resolved.IfFail(cause => ignore(faults.Park(item: cause)));
        return HostEdge.Settle(slot: ref v, outcome: resolved);
    }

    public override bool SetParam(string param, object v) => Accept(
        from field in FactoryBridge.Accept<EffectField>(candidate: param)
        from held in v is IConvertible convertible
            ? EffectValue.Of(convertible, key)
            : Fin.Fail<EffectValue>(new RenderFault.HostRefused(Member: nameof(SetParam), Detail: param))
        from written in Amended(ChangeReason.Program, () => Try.lift(() => program.Tune(field, held)).Run().Bind(static inner => inner))
        select true);

    public override void AddUISections(PostEffects.PostEffectUI ui) =>
        ignore(program.Sections.Iter(seat => ignore(Accept(Try.lift(() => seat(ui)).Run().Bind(static inner => inner).Map(static _ => true)))));

    public override bool DisplayHelp() => program.Help.Match(
        Some: show => Accept(Try.lift(show).Run().Bind(static inner => inner).Map(static _ => true)),
        None: static () => false);

    private Fin<Unit> Amended(ChangeReason reason, Func<Fin<Unit>> body) =>
        from _ in Try.lift(() => HostEdge.Side(() => BeginChange(changeContext: reason.Native))).Run().Bind(static inner => inner)
        from settled in body().Rollback(() => Try.lift(() => HostEdge.Side(() => ignore(EndChange()))).Run().Bind(static inner => inner))
        from closed in Custody.Release(
            Seq<Func<Fin<Unit>>>(
                () => Try.lift(() => HostEdge.Side(() => ignore(EndChange()))).Run().Bind(static inner => inner),
                () => Try.lift(() => HostEdge.Side(Changed)).Run().Bind(static inner => inner)))
        select closed;

    private Fin<System.Drawing.Rectangle> Frame(PostEffects.PostEffectPipeline pipeline) =>
        Optional(pipeline).ToFin(Fail: new KernelFault.MissingContext()).Bind(active => Try.lift(() =>
            active.Dimensions() is var frame && frame.Width > 0 && frame.Height > 0
                ? Fin.Succ(new System.Drawing.Rectangle(0, 0, frame.Width, frame.Height))
                : Fin.Fail<System.Drawing.Rectangle>(error: new KernelFault.InvalidResult())).Run().Bind(static inner => inner));

    private Fin<EffectPass> Pass(PostEffects.PostEffectPipeline pipeline, System.Drawing.Rectangle rect) =>
        from active in Optional(pipeline).ToFin(Fail: new KernelFault.MissingContext())
        from origin in Offset2i.Of(x: rect.X, y: rect.Y)
        from extent in Size2i.Of(width: rect.Width, height: rect.Height)
        select new EffectPass(pipeline: active, origin: origin, extent: extent);

    private bool Accept<T>(Fin<T> result) => Observe(result.Map(static _ => true), false);

    private T Observe<T>(Fin<T> result, T fallback) => result.Match(
        Succ: static value => value,
        Fail: failure => (ignore(faults.Park(item: failure)), fallback).Item2);
}

internal sealed class PostEffectGate : PostEffects.PostEffectExecutionControl {
    private readonly Func<EffectId, Fin<bool>> decide;
    private readonly Func<Error, Unit> reject;

    internal PostEffectGate(Func<EffectId, Fin<bool>> decide, Func<Error, Unit> reject) =>
        (this.decide, this.reject, this.key) = (decide, reject);

    public override bool ReadyToExecutePostEffect(Guid postEffectId) =>
        Try.lift(() => decide(EffectId.Create(postEffectId))).Run().Bind(static inner => inner).Match(
            Succ: static value => value,
            Fail: failure => (reject(failure), false).Item2);
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Effects {
    public static Fin<EffectRegistry> Register(EffectSource source) {
        return from active in Admit.Need(source)
               from types in active.Install()
               select new EffectRegistry(Registered: types.Strict());
    }

    public static Fin<EffectRoster> Configure(DocumentSession session, Seq<PostEffectOp> ops) {
        return from source in Optional(session).ToFin(Fail: new KernelFault.MissingContext())
               from admitted in guard(
                   !ops.IsEmpty && ops.ForAll(static row => row is { IsValid: true }), new KernelFault.InvalidInput(Axis: Some(nameof(ops))))
               from roster in Admit.Demand(
                   use: document => Try.lift(() => {
                       using PostEffects.PostEffectCollection collection = document.RenderSettings.PostEffects;
                       return ops.TraverseM(row => row.Apply(collection: collection)).As()
                           .Bind(_ => Roster(collection: collection));
                   }).Run().Bind(static inner => inner),
                   needs: ops.Exists(static row => row.Mutates)
                       ? [SessionNeed.Read, SessionNeed.Mutate]
                       : [SessionNeed.Read])
               select roster;
    }

    private static Fin<EffectRoster> Roster(PostEffects.PostEffectCollection collection) =>
        from rows in toSeq(collection).TraverseM(data => Detached(data: data)).As()
        from selected in Try.lift(() => Fin.Succ(value: toSeq(EffectStage.Items)
            .Choose(stage => Admit.Probe<Guid>((out Guid chosen) => collection.GetSelectedPostEffect(type: stage.Key, id: out chosen))
                .Map(chosen => (stage, EffectId.Create(chosen))))
            .ToHashMap())).Run().Bind(static inner => inner)
        select new EffectRoster(Rows: rows.Strict(), Selected: selected);

    private static Fin<EffectFact> Detached(PostEffects.PostEffectData data) => Try.lift(() =>
        from stage in FactoryBridge.Row<PostEffects.PostEffectType, EffectStage>(candidate: data.Type)
}

[PostEffects.CustomPostEffect(
    postEffectType: PostEffects.PostEffectType.Late,
    name: "<effect-name>",
    styles: PostEffects.PostEffectStyles.ExecuteForProductionRendering
        | PostEffects.PostEffectStyles.ExecuteForRealtimeRendering
        | PostEffects.PostEffectStyles.DefaultOn,
    executeWhileRenderingOption: PostEffects.PostEffectExecuteWhileRenderingOptions.UseExecutionControl,
    canDisplayHelp: false)]
public sealed class ChannelEffect() : EffectHost(program: Program) {
    private static readonly EffectField Gain = EffectField.Create("<field-gain>");

    private static EffectProgram Program { get; } = new(
        Execute: static pass => pass.Read(RenderChannel.Rgba, view => pass.Write(RenderChannel.Rgba, sink =>
            Iterable.createRange(Enumerable.Range(0, view.Extent.Height))
                .Traverse(row => Iterable.createRange(Enumerable.Range(0, view.Extent.Width))
                    .Traverse(column => Offset2i.Of(column, row)
                        .Bind(at => view.Read(at).Bind(pixel => sink.Write(at, pixel))))
                    .As()
                    .Bind(_ => pass.Advance(rows: Rasm.Numerics.Dimension.Create(value: 1))))
                .As()
                .Map(static _ => unit))),
        Read: static bag => bag.Read<double>(Gain).Map(static _ => unit),
        Write: static bag => bag.Write(Gain, value: 1.0),
        Reset: static () => Fin.Succ(unit),
        Required: CapabilitySet<RenderChannel>.Of(RenderChannel.Rgba),
        Param: static field => Fin.Succ(field == Gain ? Some<EffectValue>(new EffectValue.Number(1.0)) : Option<EffectValue>.None),
        Tune: static (field, value) => field == Gain && value is EffectValue.Number
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new RenderFault.HostRefused(Member: nameof(EffectProgram.Tune), Detail: field.Value)),
        Admits: static pass => pass.Frame.Map(static frame => frame.Width > 0 && frame.Height > 0));
}
```

## [05]-[CHANGEQUEUE]

- Owner: `SceneDelta` closes the scene-change family as detached value cases; `SceneBatch` is the idempotent custody capsule carrying its deltas between the host's own open and seal stamps; `QueuePolicy` closes capacity, bake axes, residency, the three host participation switches, and the four optional host-policy programs; `SceneQueue` adapts the host `ChangeQueue` and is the sole payload source.
- Entry: `SceneQueue.Of(QueueSource, PlugIn, QueuePolicy, ModelUnit, Context, TimeProvider) : Fin<SceneQueue>` opens public document sources or internal preview sources; `Drive` folds world build, flush, one-shot, and material refresh; `Pull` answers census reads as detached pulses; `Drain` transfers whole batches under kernel `Env` cancellation, gauged on `DispatchLane.Deferred`.
- Law: the queue is a MOUNT-FACING value. `HostUi/shell.md`'s composition capsule opens it at plug-in load and publishes its `Deltas` reader as part of `ShellMount.Engines`, so the batch hand-off has one declared writer and one declared reader rather than waiting for an `apps/` shell to invent both.
- Law: geometry identity composes the kernel reconciliation chain — `MeshSpace.Of` admits the duplicated patch, `EncodeForm.Of` canonicalizes, `Reconciliation.Apply` digests, and `GeometryHash` is minted only through that chain, never a second hash; GPU residency rides `Encode.Apply` into `EncodedGeometry` when the policy carries a `PackPolicy` row.
- Law: duplicated geometry enters owned custody before admission, reconciliation, or residency work; any downstream refusal releases the duplicate through `Custody.Rollback`, while a successful patch transfers the lease into the batch.
- Law: staging, sealing, and closing are ONE cell of three cases stepped through `Cell.Step`, and every payload a caller needs rides the transition's own POST-STATE — `Sealing` carries the batch it just cut beside the deltas that arrived during the cut, and `Closed` carries the stranded set — so the three captured-local writes inside replayable swap bodies (each of which re-ran on every CAS retry) have no spelling left. A delta staged into a closed cell reads `Refused` and releases immediately; a second close reads `Refused` and strands nothing twice.
- Law: hooks mint detached deltas inside the host grant and seal them at `NotifyEndUpdates` into one bounded channel write — the host callback never runs a consumer continuation inline, the reader is the only egress, and a refused or evicted batch releases its leases and lands as a typed `QueueLoss` row on a BOUNDED ring. `NotifyBeginUpdates` stamps the batch's open, so `SceneBatch` carries the host's own build span rather than one seal instant a consumer cannot difference.
- Law: each host change list converts atomically — a failed member records its fault, releases every detached predecessor in reverse custody through `Custody.Release`, and stages no partial delta.
- Law: the drain runs EVERY batch it read. A refused apply releases its own batch and its cause accumulates, so a residue sweep sees every outcome instead of halting on the first — a first-failure return left later batches in the reader with no owner. A direct environment poll carries `Errors.Cancelled`; only `Try.lift` mints a cause-bearing cancellation from a caught exception.
- Law: `ProvideOriginalObject`, `bNotifyChanges`, and `bRespectDisplayPipelineAttributes` are ONE `CapabilitySet<QueueTrait>` column on the policy — three independent host switches the queue's constructors and one override read, so `QueuePolicy.Live` and `QueuePolicy.Preview` are the canonical rows and the `= true`/`= false` literals that once encoded those decisions at a default-argument site are gone. `OriginalObjects` is what makes `Mesh.Object`/`Attributes` readable at all, so hardcoding it off stranded two host columns.
- Law: every host policy override the queue used to leave at its default is one OPTIONAL policy program, absent meaning the host's own default stands: `BakeSize` sizes a texture bake, `ContentDigest` composes `Render/content.md`'s `HashProbe` for the content hash, and `Baking` names the bake axes. A program that is absent is a decision deferred to the host, which is a different fact from a program that answered.
- Boundary: `AreViewsEqual` keeps the host's own redraw-trigger test. Comparing two `ViewInfo` values needs a DETACHED camera carrier no owner on this branch publishes, and the queue reaches no `ComputeViewportCrc` — that member is the realtime engine's — so a viewport-id hash standing in for a camera comparison would read equal across every camera move.
- Law: the two settings hooks are TWO cases. `ApplyRenderSettingsChanges(RenderSettings)` carries a fully readable payload projected through `Render/settings.md`'s own `RenderConfig.Of`, so the configuration a render must match crosses whole; `ApplyRenderSettingsChanges(DisplayRenderSettings)` stays payload-free because its three getters throw. Folding both into one payload-free case discarded the discriminant AND the readable half.
- Law: the two clip hooks are TWO cases — a dynamic clip change is a drag-time signal a consumer answers with a cheaper path, and folding it into the batch case with an empty removal list erased which of the two the host raised.
- Law: an `Enabled` column that gates its own payload is PRESENCE — a disabled sky's shadow intensity and a disabled ground's altitude are values no producer measured, so `SkylightCase` and `GroundCase` carry `Option`, and the ground's two remaining switches ride one `CapabilitySet<GroundTrait>` column beside the `Crc` change key the payload's own identity depends on. Every other host `bool` on a detached payload crosses as a `SwitchState` row, so a swapped argument at a construction site cannot invert a meaning silently.
- Law: a `SceneBatch` is a PUBLIC detached result, so it carries the `ModelUnit` regime its world-space geometry, altitudes, and bounds were measured in (branch RULINGS `[02]`) — the queue takes the regime at `Of` because the host's preview constructor reaches no document to read one from, and a consumer rescaling without it relabels rather than converts.
- Law: `SceneBatch.Use` excludes release across patch projection and drawing; `Drain` transfers the whole idempotent capsule, and its consumer releases only after the borrowed render pipeline settles. `SceneMarks.Render` projects flushed geometry through `Marks.Paint` while that custody is held — never a private draw path — and batch versus realtime stays split: `SceneQueue` feeds a `RealtimeProgram`, never a `RenderJob`.
- Boundary: content resolves as identity, never as graph — a material touch carries its CRC and original-instance ids for the Render content pipeline; block ancestry and decals resolve through the Blocks and content owners.
- Boundary: custom render meshes enter the live scene through the Objects authoring owner — a `Rhino.Render.CustomRenderMeshes.RenderMeshProvider` registered through `RegisterProvider` under this page's viewport and pipeline-attributes context — and reach this queue as ordinary mesh deltas; the provider adapter itself stays the authoring page's owner.
- Packages: `api-rhinocommon-render-realtime.md` (`ChangeQueue` and its 17 `Apply*` hooks, the notify bracket, the policy overrides, the `GetQueue*` pulls, the CRC resolvers, every payload column); `api-bcl-channels.md` (`CreateBounded`, `DropOldest`, `SingleReader`, the drop observer); kernel `Meshing/mesh` (`MeshSpace`), `Spatial/reconciliation` (`Reconciliation`, `ReconcileOp`, `EncodeForm`, `GeometryHash`), `Drawing/pack` (`Encode`, `PackOp`, `PackPolicy`, `EncodedGeometry`), `Domain/results` (`Lease`, `Cell`, `Transition`), `Domain/hooks` (`Ring<T>`), `Domain/context` (`ModelUnit`, `Context`), `Analysis/query` (`Env`), `Interaction/dispatch` (`UiFault`, `DispatchLane`), `Parametric/projections` (`MonotonicTimeline`, `MonotonicStamp`); `Render/settings.md` (`RenderConfig`, `WorkflowEvidence`, `EnvironmentRole`), `Render/content.md` (`ContentRef`, `HashProbe`), kernel `Domain/results` (`Custody`), `Display/draw.md` (`Canvas`, `DisplayMark`, `Marks.Paint`, `ShadedMaterial`, `SpriteSheet`), `Display/modes.md` (`Appearance`, `HostRow<TNative>`); Riok.Mapperly for `QueueMap`.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BakeAxis : ICapability<BakeAxis> {
    public static readonly BakeAxis Decals = new(key: "decals", bit: Cq.ChangeQueue.BakingFunctions.Decals);
    public static readonly BakeAxis ProceduralTextures = new(key: "procedural-textures", bit: Cq.ChangeQueue.BakingFunctions.ProceduralTextures);
    public static readonly BakeAxis CustomObjectMappings = new(key: "custom-object-mappings", bit: Cq.ChangeQueue.BakingFunctions.CustomObjectMappings);
    public static readonly BakeAxis WcsMappings = new(key: "wcs-mappings", bit: Cq.ChangeQueue.BakingFunctions.WcsBasedMappings);
    public static readonly BakeAxis MultipleMappingChannels = new(key: "multiple-mapping-channels", bit: Cq.ChangeQueue.BakingFunctions.MultipleMappingChannels);
    public static readonly BakeAxis NoRepeatTextures = new(key: "no-repeat-textures", bit: Cq.ChangeQueue.BakingFunctions.NoRepeatTextures);

    internal Cq.ChangeQueue.BakingFunctions Bit { get; }
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class QueueTrait : ICapability<QueueTrait> {
    public static readonly QueueTrait NotifyChanges = new(key: "notify-changes");
    public static readonly QueueTrait RespectDisplayAttributes = new(key: "respect-display-attributes");
    public static readonly QueueTrait OriginalObjects = new(key: "original-objects");
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class GroundTrait : ICapability<GroundTrait> {
    public static readonly GroundTrait ShadowOnly = new(key: "shadow-only");
    public static readonly GroundTrait Underside = new(key: "underside");
}

[SmartEnum<Cq.Light.Event>]
public sealed partial class LightMotion {
    public static readonly LightMotion Added = new(key: Cq.Light.Event.Added);
    public static readonly LightMotion Deleted = new(key: Cq.Light.Event.Deleted);
    public static readonly LightMotion Undeleted = new(key: Cq.Light.Event.Undeleted);
    public static readonly LightMotion Modified = new(key: Cq.Light.Event.Modified);
    public static readonly LightMotion Sorted = new(key: Cq.Light.Event.Sorted);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueueSource {
    private QueueSource() { }
    public sealed record LiveCase(DocumentSession Session, ViewportTarget Target) : QueueSource;
    internal sealed record PreviewCase(CreatePreviewEventArgs Args) : QueueSource;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record QueueDrive {
    private QueueDrive() { }
    public sealed record WorldCase(SwitchState FlushWhenReady) : QueueDrive;
    public sealed record FlushCase : QueueDrive;
    public sealed record OneShotCase : QueueDrive;
    public sealed record MaterialsCase : QueueDrive;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScenePull {
    private ScenePull() { }
    public sealed record ViewCase : ScenePull;
    public sealed record ConfigCase : ScenePull;
    public sealed record BoundsCase : ScenePull;
    public sealed record SunCase : ScenePull;
    public sealed record SkylightCase : ScenePull;
    public sealed record GroundCase : ScenePull;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScenePulse : IDetachedDocumentResult {
    private ScenePulse() { }
    public sealed record ViewPulse(Guid View) : ScenePulse;
    public sealed record ConfigPulse(RenderConfig Config) : ScenePulse;
    public sealed record BoundsPulse(BoundingBox Bounds, ModelUnit Units) : ScenePulse;
    public sealed record SunPulse(Lease<Light> Sun) : ScenePulse;
    public sealed record SkylightPulse(Option<SkyDelta> State) : ScenePulse;
    public sealed record GroundPulse(Option<GroundDelta> State) : ScenePulse;
}

// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct BakeDemand(Guid Subject, uint Material, HostRow<TextureType> Slot);

public readonly record struct ContentDigest(ContentRef Content, HashProbe Probe, WorkflowEvidence Workflow);

public sealed record QueuePolicy {
    private QueuePolicy(
        Rasm.Numerics.Dimension capacity,
        CapabilitySet<BakeAxis> baking,
        CapabilitySet<QueueTrait> traits,
        Option<PackPolicy> residency,
        Option<Func<BakeDemand, Fin<Rasm.Numerics.Dimension>>> bakeSize,
        Option<Func<ContentDigest, Fin<uint>>> contentDigest) =>
        (Capacity, Baking, Traits, Residency, BakeSize, ContentDigest) =
        (capacity, baking, traits, residency, bakeSize, contentDigest);

    public Rasm.Numerics.Dimension Capacity { get; }
    public CapabilitySet<BakeAxis> Baking { get; }
    public CapabilitySet<QueueTrait> Traits { get; }
    public Option<PackPolicy> Residency { get; }
    public Option<Func<BakeDemand, Fin<Rasm.Numerics.Dimension>>> BakeSize { get; }
    public Option<Func<ContentDigest, Fin<uint>>> ContentDigest { get; }

    public static QueuePolicy Live(Rasm.Numerics.Dimension capacity) => new(
        capacity,
        CapabilitySet<BakeAxis>.None,
        CapabilitySet<QueueTrait>.Of(QueueTrait.NotifyChanges, QueueTrait.OriginalObjects),
        Option<PackPolicy>.None, default, default);

    public static QueuePolicy Preview(Rasm.Numerics.Dimension capacity) => new(
        capacity, CapabilitySet<BakeAxis>.None, CapabilitySet<QueueTrait>.None,
        Option<PackPolicy>.None, default, default);

    public static Fin<QueuePolicy> Of(
        Rasm.Numerics.Dimension capacity,
        CapabilitySet<BakeAxis> baking,
        CapabilitySet<QueueTrait> traits,
        Option<PackPolicy> residency = default,
        Option<Func<BakeDemand, Fin<Rasm.Numerics.Dimension>>> bakeSize = default,
        Option<Func<ContentDigest, Fin<uint>>> contentDigest = default) =>
        guard(capacity.Value > 0, new KernelFault.InvalidInput(Axis: Some(nameof(capacity)))).ToFin()
            .Map(_ => new QueuePolicy(capacity, baking, traits, residency, bakeSize, contentDigest));

    internal Cq.ChangeQueue.BakingFunctions Bake =>
        (Cq.ChangeQueue.BakingFunctions)Baking.Mask(static row => (int)row.Bit);
}

public readonly record struct MappingSlot(int Channel, Transform Local, Option<TextureMapping> Mapping);



public sealed record MeshPatch(GeometryHash Content, Lease<Mesh> Geometry, Option<EncodedGeometry> Residency);

public sealed record MeshDelta(Guid Id, Transform Ocs, Seq<MappingSlot> Mappings, Seq<MeshPatch> Patches);

public readonly record struct InstanceMotion(uint Instance, Transform Motion);

public sealed record InstanceDelta(uint Instance, Guid Root, Guid Parent, Guid Mesh, MaterialTouch Material, Transform Placement);

public sealed record MaterialTouch(uint Material, uint MeshInstance, Seq<Guid> Origins);

public sealed record LightDelta(Guid Id, uint Crc, LightMotion Change, Lease<Light> Data);

public readonly record struct SkyDelta(SwitchState CustomEnvironment, double ShadowIntensity);

public sealed record GroundDelta(
    CapabilitySet<GroundTrait> Traits,
    double Altitude,
    uint Material,
    Vector2d TextureScale,
    Vector2d TextureOffset,
    double TextureRotation,
    uint Crc);

public sealed record ClipDelta(Guid Id, Plane Plane, SwitchState Enabled, Seq<Guid> Views, Seq<Guid> ClipViewports);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record SceneDelta {
    private SceneDelta() { }
    public sealed record ViewCase(Guid View) : SceneDelta;
    public sealed record GeometryCase(Seq<Guid> Removed, Seq<MeshDelta> Added) : SceneDelta;
    public sealed record InstanceCase(Seq<uint> Removed, Seq<InstanceDelta> Upserted) : SceneDelta;
    public sealed record MotionCase(Seq<InstanceMotion> Moves) : SceneDelta;
    public sealed record LightCase(Seq<LightDelta> Lights) : SceneDelta;
    public sealed record DynamicLightCase(Seq<Lease<Light>> Lights) : SceneDelta;
    public sealed record SunCase(Lease<Light> Sun) : SceneDelta;
    public sealed record MaterialCase(Seq<MaterialTouch> Touches) : SceneDelta;
    public sealed record SettingsCase(RenderConfig Config) : SceneDelta;
    public sealed record DisplaySettingsCase : SceneDelta;
    public sealed record EnvironmentCase(Seq<EnvironmentRole> Roles) : SceneDelta;
    public sealed record SkylightCase(Option<SkyDelta> State) : SceneDelta;
    public sealed record GroundCase(Option<GroundDelta> State) : SceneDelta;
    public sealed record ClipCase(Seq<Guid> Removed, Seq<ClipDelta> Upserted) : SceneDelta;
    public sealed record DynamicClipCase(Seq<ClipDelta> Changed) : SceneDelta;
    public sealed record WorkflowCase(WorkflowEvidence Evidence) : SceneDelta;
    public sealed record AttributesCase(Seq<Appearance> Concerns) : SceneDelta;
    public sealed record DynamicReadyCase : SceneDelta;

    internal Fin<Unit> Release() => Switch(
        viewCase: static _ => Fin.Succ(unit),
        geometryCase: static (row) => Custody.Release(
            row.Added.Bind(static delta => delta.Patches).Strict(), static patch => Fin.Succ(patch.Geometry.Dispose())),
        instanceCase: static _ => Fin.Succ(unit),
        motionCase: static _ => Fin.Succ(unit),
        lightCase: static (row) => Custody.Release(row.Lights, static delta => Fin.Succ(delta.Data.Dispose())),
        dynamicLightCase: static (row) => Custody.Release(row.Lights, static lease => Fin.Succ(lease.Dispose())),
        sunCase: static (row) => Try.lift(() => Fin.Succ(row.Sun.Dispose())).Run().Bind(static inner => inner),
        materialCase: static _ => Fin.Succ(unit),
        settingsCase: static _ => Fin.Succ(unit),
        displaySettingsCase: static _ => Fin.Succ(unit),
        environmentCase: static _ => Fin.Succ(unit),
        skylightCase: static _ => Fin.Succ(unit),
        groundCase: static _ => Fin.Succ(unit),
        clipCase: static _ => Fin.Succ(unit),
        dynamicClipCase: static _ => Fin.Succ(unit),
        workflowCase: static _ => Fin.Succ(unit),
        attributesCase: static _ => Fin.Succ(unit),
        dynamicReadyCase: static _ => Fin.Succ(unit));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record QueueCell {
    private QueueCell() { }
    internal sealed record Open(Seq<SceneDelta> Staged) : QueueCell;
    internal sealed record Sealing(Seq<SceneDelta> Batch, Seq<SceneDelta> Staged) : QueueCell;
    internal sealed record Closed(Seq<SceneDelta> Stranded) : QueueCell;
}

public sealed class SceneBatch : IDisposable {
    private readonly Seq<SceneDelta> deltas;
    private readonly Atom<MountPhase> phase = Atom(MountPhase.Open);

    internal SceneBatch(Seq<SceneDelta> deltas, Option<MonotonicStamp> opened, Option<MonotonicStamp> sealedAt, ModelUnit units) =>
        (this.deltas, Opened, Sealed, Units, this.key) = (deltas, opened, sealedAt, units);

    public Option<MonotonicStamp> Opened { get; }
    public Option<MonotonicStamp> Sealed { get; }
    public ModelUnit Units { get; }
    public Rasm.Numerics.Dimension Count => Rasm.Numerics.Dimension.Create(value: deltas.Count);

    internal Fin<TResult> Use<TResult>(Func<Seq<SceneDelta>, Fin<TResult>> use) =>
        from _ in guard(!phase.Value.Closes, new KernelFault.InvalidContext()).ToFin()
        from body in Admit.Need(use)
        from result in Try.lift(() => body(deltas)).Run().Bind(static inner => inner)
        select result;

    public Fin<Unit> Release() =>
        Cell.Step(phase, static held => held.Closes ? None : Some(MountPhase.Released), new KernelFault.InvalidContext())
            is Transition<MountPhase>.Committed
            ? Custody.Release(deltas, delta => delta.Release())
            : Fin.Succ(unit);

    public void Dispose() => ignore(Release());
}

public readonly record struct QueueLoss(Option<MonotonicStamp> At, Rasm.Numerics.Dimension Deltas);

// --- [OPERATIONS] ----------------------------------------------------------------------
[Mapper]
internal static partial class QueueMap {
    [MapProperty(nameof(Cq.Skylight.UsesCustomEnvironment), nameof(SkyDelta.CustomEnvironment))]
    internal static partial SkyDelta Detach(Cq.Skylight payload);

    [MapProperty(nameof(Cq.DynamicObjectTransform.MeshInstanceId), nameof(InstanceMotion.Instance))]
    [MapProperty(nameof(Cq.DynamicObjectTransform.Transform), nameof(InstanceMotion.Motion))]
    internal static partial InstanceMotion Detach(Cq.DynamicObjectTransform payload);

    [MapProperty(nameof(Cq.ClippingPlane.IsEnabled), nameof(ClipDelta.Enabled))]
    [MapProperty(nameof(Cq.ClippingPlane.ViewIds), nameof(ClipDelta.Views))]
    internal static partial ClipDelta Detach(Cq.ClippingPlane payload);

    internal static partial MappingSlot Detach(Cq.MappingChannel payload);

    internal static GroundDelta Detach(Cq.GroundPlane payload) => new(
        Traits: (payload.IsShadowOnly ? CapabilitySet<GroundTrait>.Of(GroundTrait.ShadowOnly) : CapabilitySet<GroundTrait>.None)
            is var held && payload.ShowUnderside ? held.With(GroundTrait.Underside) : held,
        Altitude: payload.Altitude,
        Material: payload.MaterialId,
        TextureScale: payload.TextureScale,
        TextureOffset: payload.TextureOffset,
        TextureRotation: payload.TextureRotation,
        Crc: payload.Crc);

    [UserMapping(Default = true)]
    private static SwitchState State(bool value) => value ? SwitchState.On : SwitchState.Off;

    [UserMapping(Default = true)]
    private static Option<TextureMapping> Held(TextureMapping value) => Optional(value);

    [UserMapping(Default = true)]
    private static Seq<Guid> Rows(Guid[] values) => toSeq(values).Strict();
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed class SceneQueue : Cq.ChangeQueue {
    private static readonly Seq<(RenderEnvironment.Usage Bit, EnvironmentRole Role)> EnvironmentPolicy = [
        (RenderEnvironment.Usage.Background, EnvironmentRole.Background),
        (RenderEnvironment.Usage.ReflectionAndRefraction, EnvironmentRole.Reflection),
        (RenderEnvironment.Usage.Skylighting, EnvironmentRole.Skylighting),
    ];
    private static readonly RenderEnvironment.Usage EnvironmentMask = EnvironmentPolicy.Fold(
        RenderEnvironment.Usage.None,
        static (mask, row) => mask | row.Bit);
    private readonly Channel<SceneBatch> lane;
    private readonly Atom<QueueCell> cell = Atom<QueueCell>(new QueueCell.Open(Seq<SceneDelta>()));
    private readonly Atom<Option<MonotonicStamp>> opened = Atom(Option<MonotonicStamp>.None);
    private readonly Ring<QueueLoss> losses = new(cap: DisplayFaults.Cap);
    private readonly Ring<Error> faults = new(cap: DisplayFaults.Cap);
    private readonly QueuePolicy policy;
    private readonly MonotonicTimeline timeline;
    private readonly ModelUnit units;
    private readonly Context context;

    private SceneQueue(Guid plugin, uint document, ViewInfo view, QueuePolicy policy, MonotonicTimeline timeline, ModelUnit units, Context context)
        : base(plugin, document, view, null,
            bRespectDisplayPipelineAttributes: policy.Traits.Admits(QueueTrait.RespectDisplayAttributes),
            bNotifyChanges: policy.Traits.Admits(QueueTrait.NotifyChanges)) =>
        (this.policy, this.timeline, this.units, this.context, this.key, lane) =
        (policy, timeline, units, context, Open(policy, timeline, losses));

    private SceneQueue(Guid plugin, CreatePreviewEventArgs preview, QueuePolicy policy, MonotonicTimeline timeline, ModelUnit units, Context context)
        : base(plugin, preview) =>
        (this.policy, this.timeline, this.units, this.context, this.key, lane) =
        (policy, timeline, units, context, Open(policy, timeline, losses));

    public ChannelReader<SceneBatch> Deltas => lane.Reader;

    public Seq<Error> Faults => faults.Parked;
    public Seq<QueueLoss> Losses => losses.Parked;
    public long Shed => faults.Shed + losses.Shed;

    public static Fin<SceneQueue> Of(
        QueueSource source,
        PlugIn owner,
        QueuePolicy policy,
        ModelUnit units,
        Context context,
        TimeProvider clock) {
        return from shape in Admit.Need(source)
               from plugin in Admit.Need(owner)
               from plan in Admit.Need(policy)
               from regime in Admit.Need(units)
               from ambient in Admit.Need(context)
               from ticks in Admit.Need(clock)
               from timeline in MonotonicTimeline.Of(provider: ticks)
               from queue in shape.Switch(
                   (Plugin: plugin, Plan: plan, Timeline: timeline, Units: regime, Context: ambient),
                   liveCase: static (held, row) =>
                       from lease in ViewportLease.Of(session: row.Session, target: row.Target)
                       from opened in lease.Use(borrow: seat => seat.Info(view => Try.lift(() => Fin.Succ(new SceneQueue(
                           plugin: held.Plugin.Id,
                           document: row.Session.Key,
                           view: view,
                           policy: held.Plan,
                           timeline: held.Timeline,
                           units: held.Units,
                           context: held.Context))).Run().Bind(static inner => inner)))
                       select opened,
                   previewCase: static (held, row) => Try.lift(() => Fin.Succ(new SceneQueue(
                       plugin: held.Plugin.Id,
                       preview: row.Args,
                       policy: held.Plan,
                       timeline: held.Timeline,
                       units: held.Units,
                       context: held.Context))).Run().Bind(static inner => inner))
               select queue;
    }

    public Fin<Unit> Drive(QueueDrive drive) {
        return from plan in Admit.Need(drive)
               from _ in Live()
               from done in plan.Switch(
                   this,
                   worldCase: static (held, row) => Try.lift(() =>
                       Fin.Succ(HostEdge.Side(() => held.CreateWorld(bFlushWhenReady: row.FlushWhenReady.Enabled)))).Run().Bind(static inner => inner),
                   flushCase: static (held, _) => Try.lift(() => Fin.Succ(HostEdge.Side(held.Flush))).Run().Bind(static inner => inner),
                   oneShotCase: static (held, _) => Try.lift(() => Fin.Succ(HostEdge.Side(held.OneShot))).Run().Bind(static inner => inner),
                   materialsCase: static (held, _) => Try.lift(() => Fin.Succ(HostEdge.Side(held.RefreshMaterials))).Run().Bind(static inner => inner))
               select done;
    }

    public Fin<ScenePulse> Pull(ScenePull pull) {
        return from ask in Admit.Need(pull)
               from _ in Live()
               from pulse in ask.Switch(
                   this,
                   viewCase: static (held, _) => Try.lift(() =>
                       Optional(held.GetQueueView()).ToFin(Fail: new KernelFault.MissingContext())
                           .Map(static view => (ScenePulse)new ScenePulse.ViewPulse(View: view.Viewport.Id))).Run().Bind(static inner => inner),
                   configCase: static (held, _) => Try.lift(() =>
                       Optional(held.GetQueueRenderSettings()).ToFin(Fail: new KernelFault.MissingContext())
                           .Bind(settings => RenderConfig.Of(settings))
                           .Map(static config => (ScenePulse)new ScenePulse.ConfigPulse(config))).Run().Bind(static inner => inner),
                   boundsCase: static (held, _) => Try.lift(() => Fin.Succ<ScenePulse>(
                       new ScenePulse.BoundsPulse(Bounds: held.GetQueueSceneBoundingBox(), Units: held.units))).Run().Bind(static inner => inner),
                   sunCase: static (held, _) => Try.lift(() =>
                       Optional(held.GetQueueSun()).ToFin(Fail: new KernelFault.MissingContext())
                           .Map(static sun => (ScenePulse)new ScenePulse.SunPulse(
                               Sun: new Lease<Light>.Owned(Value: (Light)sun.Duplicate())))).Run().Bind(static inner => inner),
                   skylightCase: static (held, _) => Try.lift(() =>
                       Optional(held.GetQueueSkylight()).ToFin(Fail: new KernelFault.MissingContext())
                           .Map(static sky => (ScenePulse)new ScenePulse.SkylightPulse(State: Sky(sky)))).Run().Bind(static inner => inner),
                   groundCase: static (held, _) => Try.lift(() =>
                       Optional(held.GetQueueGroundPlane()).ToFin(Fail: new KernelFault.MissingContext())
                           .Map(static ground => (ScenePulse)new ScenePulse.GroundPulse(State: Ground(ground)))).Run().Bind(static inner => inner))
               select pulse;
    }

    public Fin<Rasm.Numerics.Dimension> Drain(Func<SceneBatch, Fin<Unit>> take, Option<Env> env = default) {
        SceneQueue self = this;
        return from body in Admit.Need(take)
               from _ in Live()
               from measured in timeline.Gauged<Rasm.Numerics.Dimension, DispatchLane>(
                   lane: DispatchLane.Deferred,
                   work: op,
                   body: () => self.Apply(self.Taken(), body, env))
               from applied in (HostEdge.SideWhen(measured.Span.Breached, () => ignore(self.faults.Park(
                       item: new RenderFault.HostRefused(Member: nameof(Drain), Detail: measured.Span.Overrun.ToString())))),
                   measured.Value).Item2
               select applied;
    }

    public Fin<Unit> Close() {
        SceneQueue self = this;
        return Cell.Step(
                cell,
                static held => held switch {
                    QueueCell.Open row => Some<QueueCell>(new QueueCell.Closed(row.Staged)),
                    QueueCell.Sealing row => Some<QueueCell>(new QueueCell.Closed(row.Batch + row.Staged)),
                    _ => Option<QueueCell>.None,
                },
                new KernelFault.InvalidContext()) switch {
            Transition<QueueCell>.Committed { State: QueueCell.Closed closed } =>
                Custody.Release(
                    Seq<Func<Fin<Unit>>>(
                        () => self.Stranded(closed.Stranded),
                        () => Try.lift(() => Fin.Succ(HostEdge.Side(() => ignore(self.lane.Writer.TryComplete())))).Run().Bind(static inner => inner),
                        () => self.Sweep(),
                        () => Try.lift(() => { self.Dispose(); return Fin.Succ(unit); }).Run().Bind(static inner => inner))),
            _ => Fin.Succ(unit),
        };
    }

    // --- [APPLY_HOOKS]
    protected override void ApplyViewChange(ViewInfo viewInfo) =>
        ignore(Stage(new SceneDelta.ViewCase(View: viewInfo.Viewport.Id)));

    protected override void ApplyMeshChanges(Guid[] deleted, List<Cq.Mesh> added) => StageBatch(
        source: toSeq(added),
        detach: Detach,
        release: delta => Custody.Release(delta.Patches, static patch => Fin.Succ(patch.Geometry.Dispose()), key),
        project: rows => new SceneDelta.GeometryCase(Removed: toSeq(deleted).Strict(), Added: rows));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<Cq.MeshInstance> addedOrChanged) => StageBatch(
        source: toSeq(addedOrChanged),
        detach: payload => Try.lift(() => Fin.Succ(new InstanceDelta(
            Instance: payload.InstanceId,
            Root: payload.RootId,
            Parent: payload.ParentId,
            Mesh: payload.MeshId,
            Material: Touch(material: payload.MaterialId, instance: payload.InstanceId),
            Placement: payload.Transform))).Run().Bind(static inner => inner),
        release: static _ => Fin.Succ(unit),
        project: rows => new SceneDelta.InstanceCase(Removed: toSeq(deleted).Strict(), Upserted: rows));

    protected override void ApplyDynamicObjectTransforms(List<Cq.DynamicObjectTransform> dynamicObjectTransforms) =>
        ignore(Stage(new SceneDelta.MotionCase(Moves: toSeq(dynamicObjectTransforms).Map(QueueMap.Detach).Strict())));

    protected override void ApplyLightChanges(List<Cq.Light> lightChanges) => StageBatch(
        source: toSeq(lightChanges),
        detach: payload =>
            from change in FactoryBridge.Row<Cq.Light.Event, LightMotion>(candidate: payload.ChangeType)
            from data in Try.lift(() => Fin.Succ(new Lease<Light>.Owned(Value: (Light)payload.Data.Duplicate()))).Run().Bind(static inner => inner)
            select new LightDelta(Id: payload.Id, Crc: payload.IdCrc, Change: change, Data: data),
        release: static delta => Fin.Succ(delta.Data.Dispose()),
        project: static rows => new SceneDelta.LightCase(Lights: rows));

    protected override void ApplyDynamicLightChanges(List<Light> dynamicLightChanges) => StageBatch(
        source: toSeq(dynamicLightChanges),
        detach: payload => Try.lift(() => Fin.Succ<Lease<Light>>(new Lease<Light>.Owned(Value: (Light)payload.Duplicate()))).Run().Bind(static inner => inner),
        release: static lease => Fin.Succ(lease.Dispose()),
        project: static rows => new SceneDelta.DynamicLightCase(Lights: rows));

    protected override void ApplySunChanges(Light sun) => ignore(Observe(Try.lift(() => Fin.Succ(
        Stage(new SceneDelta.SunCase(Sun: new Lease<Light>.Owned(Value: (Light)sun.Duplicate()))))).Run().Bind(static inner => inner)));

    protected override void ApplyMaterialChanges(List<Cq.Material> mats) => ignore(Stage(new SceneDelta.MaterialCase(
        Touches: toSeq(mats).Map(payload => Touch(material: payload.Id, instance: payload.MeshInstanceId)).Strict())));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) => ignore(Observe(
        RenderConfig.Of(rs, key).Map(config => Stage(new SceneDelta.SettingsCase(Config: config)))));

    protected override void ApplyRenderSettingsChanges(Cq.DisplayRenderSettings settings) =>
        ignore(Stage(new SceneDelta.DisplaySettingsCase()));

    protected override void ApplyEnvironmentChanges(RenderEnvironment.Usage usage) => ignore(Observe(
        usage != RenderEnvironment.Usage.None && (usage | EnvironmentMask) == EnvironmentMask
            ? Fin.Succ(EnvironmentPolicy.Choose(row => (usage & row.Bit) != 0 ? Some(row.Role) : None).Strict())
            : Fin.Fail<Seq<EnvironmentRole>>(new RenderFault.HostRefused(Member: nameof(ApplyEnvironmentChanges), Detail: usage.ToString()))
        ).Map(roles => Stage(new SceneDelta.EnvironmentCase(Roles: roles))));

    protected override void ApplySkylightChanges(Cq.Skylight skylight) =>
        ignore(Stage(new SceneDelta.SkylightCase(State: Sky(skylight))));

    protected override void ApplyGroundPlaneChanges(Cq.GroundPlane gp) =>
        ignore(Stage(new SceneDelta.GroundCase(State: Ground(gp))));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) => ignore(Observe(Try.lift(() =>
        Fin.Succ(Stage(new SceneDelta.WorkflowCase(Evidence: WorkflowEvidence.Of(lw))))).Run().Bind(static inner => inner)));

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<Cq.ClippingPlane> addedOrModified) =>
        ignore(Stage(new SceneDelta.ClipCase(
            Removed: toSeq(deleted).Strict(),
            Upserted: toSeq(addedOrModified).Map(QueueMap.Detach).Strict())));

    protected override void ApplyDynamicClippingPlaneChanges(List<Cq.ClippingPlane> changed) =>
        ignore(Stage(new SceneDelta.DynamicClipCase(Changed: toSeq(changed).Map(QueueMap.Detach).Strict())));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        ignore(Observe(Appearance.Of(displayPipelineAttributes, key)
            .Map(concerns => Stage(new SceneDelta.AttributesCase(Concerns: concerns)))));

    // --- [NOTIFY_BRACKET]
    protected override void NotifyBeginUpdates() {
        base.NotifyBeginUpdates();
        _ = opened.Swap(_ => Error.New(key: key.Message).ToOption());
    }

    protected override void NotifyEndUpdates() {
        base.NotifyEndUpdates();
        _ = Cell.Step(
                cell,
                static held => held is QueueCell.Open row && !row.Staged.IsEmpty
                    ? Some<QueueCell>(new QueueCell.Sealing(Batch: row.Staged, Staged: Seq<SceneDelta>()))
                    : Option<QueueCell>.None,
                new KernelFault.InvalidContext()) is Transition<QueueCell>.Committed { State: QueueCell.Sealing cut }
            ? Publish(cut.Batch)
            : unit;
    }

    protected override void NotifyDynamicUpdatesAreAvailable() {
        base.NotifyDynamicUpdatesAreAvailable();
        _ = Stage(new SceneDelta.DynamicReadyCase());
    }

    // --- [POLICY_OVERRIDES]
    protected override bool ProvideOriginalObject() => policy.Traits.Admits(QueueTrait.OriginalObjects);

    protected override Cq.ChangeQueue.BakingFunctions BakeFor() => policy.Bake;

    protected override int BakingSize(RhinoObject obj, RenderMaterial material, TextureType type) => policy.BakeSize.Match(
        Some: size => Observe(
                from slot in HostRow<TextureType>.Of(native: type)
                from measured in Try.lift(() => size(new BakeDemand(Subject: obj.Id, Material: material.RenderHash, Slot: slot))).Run().Bind(static inner => inner)
                select measured)
            .Match(Succ: static value => value.Value, Fail: _ => base.BakingSize(obj, material, type)),
        None: () => base.BakingSize(obj, material, type));

    protected override uint ContentRenderHash(RenderContent content, CrcRenderHashFlags flags, string excluded, LinearWorkflow lw) =>
        policy.ContentDigest.Match(
            Some: digest => Observe(
                    from probe in HostEdge.Text(excluded).Match(
                        Some: named => HashProbe.Excluding(flags, named),
                        None: () => HashProbe.Excluding(flags))
                    from address in ContentRef.Of(content.Id)
                    from hashed in Try.lift(() => digest(new ContentDigest(
                        Content: address, Probe: probe, Workflow: WorkflowEvidence.Of(lw)))).Run().Bind(static inner => inner)
                    select hashed)
                .Match(Succ: static value => value, Fail: _ => base.ContentRenderHash(content, flags, excluded, lw)),
            None: () => base.ContentRenderHash(content, flags, excluded, lw));

    // --- [QUEUE_INTERNALS]
    private Fin<Unit> Live() => guard(cell.Value is not QueueCell.Closed, new KernelFault.InvalidContext()).ToFin();

    private static Channel<SceneBatch> Open(QueuePolicy policy, MonotonicTimeline timeline, Ring<QueueLoss> losses) =>
        Channel.CreateBounded<SceneBatch>(
            new BoundedChannelOptions(policy.Capacity.Value) {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                AllowSynchronousContinuations = false,
            },
            dropped => {
                _ = dropped.Release();
                _ = losses.Park(item: new QueueLoss(At: Error.New(key: key.Message).ToOption(), Deltas: dropped.Count));
            });

    private Unit Publish(Seq<SceneDelta> cut) {
        SceneBatch batch = new(
            deltas: cut,
            opened: opened.Value,
            sealedAt: Error.New(key: key.Message).ToOption(),
            units: units);
        _ = HostEdge.SideWhen(!lane.Writer.TryWrite(batch), () => {
            _ = batch.Release();
            _ = losses.Park(item: new QueueLoss(At: batch.Sealed, Deltas: batch.Count));
        });
        return ignore(Cell.Step(
            cell,
            static held => held is QueueCell.Sealing row ? Some<QueueCell>(new QueueCell.Open(row.Staged)) : None,
            new KernelFault.InvalidContext()));
    }

    private Transition<QueueCell> Stage(SceneDelta delta) {
        Transition<QueueCell> staged = Cell.Step(
            cell,
            held => held switch {
                QueueCell.Open row => Some<QueueCell>(new QueueCell.Open(row.Staged.Add(delta))),
                QueueCell.Sealing row => Some<QueueCell>(row with { Staged = row.Staged.Add(delta) }),
                _ => Option<QueueCell>.None,
            },
            new KernelFault.InvalidContext());
        return staged is Transition<QueueCell>.Refused
            ? (ignore(Observe(delta.Release())), staged).Item2
            : staged;
    }

    private void StageBatch<TIn, TOut>(
        Seq<TIn> source,
        Func<TIn, Fin<TOut>> detach,
        Func<TOut, Fin<Unit>> release,
        Func<Seq<TOut>, SceneDelta> project) =>
        ignore(Observe(DetachAll(source, detach, release)).Map(rows => Stage(project(rows.Strict()))));

    private Fin<Seq<TOut>> DetachAll<TIn, TOut>(Seq<TIn> source, Func<TIn, Fin<TOut>> detach, Func<TOut, Fin<Unit>> release) =>
        source.Fold(
            Fin.Succ(Seq<TOut>()),
            (accepted, input) => accepted.Bind(done => Try.lift(() => detach(input)).Run().Bind(static inner => inner)
                .Map(done.Add)
                .Rollback(done, release, key)));

    private Fin<Rasm.Numerics.Dimension> Apply(Seq<SceneBatch> batches, Func<SceneBatch, Fin<Unit>> take, Option<Env> env) {
        (Seq<Error> refused, Seq<Rasm.Numerics.Dimension> applied) = batches
            .Map(batch => env.Map(static held => held.Cancellation.IsCancellationRequested).IfNone(false)
                ? batch.Release().Bind(_ => Fin.Fail<Rasm.Numerics.Dimension>(Errors.Cancelled))
                : Try.lift(() => take(batch)).Run().Bind(static inner => inner).Match(
                    Succ: _ => Fin.Succ(batch.Count),
                    Fail: cause => batch.Release().Match(
                        Succ: _ => Fin.Fail<Rasm.Numerics.Dimension>(cause),
                        Fail: cleanup => Fin.Fail<Rasm.Numerics.Dimension>(cause + cleanup))))
            .Partition();
        return refused.IsEmpty
            ? Fin.Succ(Rasm.Numerics.Dimension.Create(value: applied.Sum(static row => row.Value)))
            : Fin.Fail<Rasm.Numerics.Dimension>(Error.Many(refused));
    }

    private Seq<SceneBatch> Taken() {
        Seq<SceneBatch> held = Seq<SceneBatch>();
        while (lane.Reader.TryRead(out SceneBatch? batch)) {
            held = held.Add(batch);
        }
        return held.Strict();
    }

    private Fin<Unit> Sweep() {
        Seq<SceneBatch> residue = Taken();
        return Custody.Release(residue, batch => (
            losses.Park(item: new QueueLoss(At: Error.New(key: op.Message).ToOption(), Deltas: batch.Count)),
            batch.Release()).Item2);
    }

    private Fin<Unit> Stranded(Seq<SceneDelta> rows) => rows.IsEmpty
        ? Fin.Succ(unit)
        : (losses.Park(item: new QueueLoss(At: Error.New(key: op.Message).ToOption(), Deltas: Rasm.Numerics.Dimension.Create(value: rows.Count))),
           Custody.Release(rows, delta => delta.Release())).Item2;

    private MaterialTouch Touch(uint material, uint instance) => new(
        Material: material,
        MeshInstance: instance,
        Origins: toSeq(OriginalInstanceIdsFromMaterialId(material)).Strict());

    private Fin<MeshDelta> Detach(Cq.Mesh payload) =>
        from patches in DetachAll(
            source: toSeq(payload.GetMeshes()),
            detach: Patch,
            release: static patch => Fin.Succ(patch.Geometry.Dispose()))
        from delta in Try.lift(() => Fin.Succ(new MeshDelta(
            Id: payload.Id(),
            Ocs: payload.OcsTransform,
            Mappings: toSeq(payload.Mappings).Map(QueueMap.Detach).Strict(),
            Patches: patches.Strict()))).Run().Bind(static inner => inner)
        select delta;

    private Fin<MeshPatch> Patch(Mesh native) =>
        from geometry in Try.lift(() => Optional(native.Duplicate() as Mesh).ToFin(new KernelFault.InvalidResult())
            .Map(static duplicate => (Lease<Mesh>)new Lease<Mesh>.Owned(Value: duplicate))).Run().Bind(static inner => inner)
        from patch in (from space in MeshSpace.Of(native: geometry.Resource, context: context)
                       from answer in Reconciliation.Apply(new ReconcileOp.Encode(EncodeForm.Of(space)))
                       from digest in answer.Switch(
                           digest: static row => Fin.Succ(row.Value),
                           reconciled: static (op) => Fin.Fail<GeometryHash>(new KernelFault.InvalidResult(Detail: Some(nameof(ReconcileAnswer.Reconciled)))))
                       from residency in policy.Residency
                           .TraverseM(pack => Encode.Apply(new PackOp.MeshPatch(Source: space, Policy: pack)))
                           .As()
                       select new MeshPatch(Content: digest, Geometry: geometry, Residency: residency))
            .Rollback(() => Fin.Succ(geometry.Dispose()))
        select patch;

    private static Option<SkyDelta> Sky(Cq.Skylight payload) =>
        payload.Enabled ? Some(QueueMap.Detach(payload)) : Option<SkyDelta>.None;

    private static Option<GroundDelta> Ground(Cq.GroundPlane payload) =>
        payload.Enabled ? Some(QueueMap.Detach(payload)) : Option<GroundDelta>.None;

    private Fin<T> Observe<T>(Fin<T> outcome) {
        _ = outcome.IfFail(cause => ignore(faults.Park(item: cause)));
        return outcome;
    }
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class SceneMarks {
    public static Fin<DrawTally> Render(
        SceneBatch batch,
        Canvas canvas,
        Func<MeshPatch, ShadedMaterial> material) {
        return from source in Optional(batch).ToFin(new KernelFault.InvalidInput())
               from project in Optional(material).ToFin(new KernelFault.InvalidInput())
               from rendered in source.Use(deltas => Marks.Paint(canvas, Project(deltas, project)))
               select rendered;
    }

    private static Seq<DisplayMark> Project(Seq<SceneDelta> deltas, Func<MeshPatch, ShadedMaterial> material) => deltas
        .Bind(static delta => delta is SceneDelta.GeometryCase geometry ? geometry.Added : Seq<MeshDelta>())
        .Bind(static delta => delta.Patches)
        .Map(patch => (DisplayMark)new DisplayMark.World(
            Value: new WorldMark.MeshShaded(Value: patch.Geometry.Resource, Material: material(patch))))
        .Strict();
}
```

## [06]-[SURFACE_LEDGER]

| [INDEX] | [OWNER]            | [INGRESS]                            | [PIPELINE]                            | [EGRESS]                    |
| :-----: | :----------------- | :----------------------------------- | :------------------------------------ | :-------------------------- |
|  [01]   | `RenderJob`        | `RenderRequest` · `FramebufferScope` | demand · `WindowOp.Apply`             | `RenderYield`               |
|  [02]   | `JobAsync`         | `AsyncProgram`                       | engine thread · `EndAsyncRender`      | `RealtimePort` writes       |
|  [03]   | `RealtimeEngines`  | `RealtimeEnginePlan`                 | `SeatRegistry.Claim` · host scan      | `SeatToken` · descriptors   |
|  [04]   | `RealtimeEngine`   | host activation · `PostConstruct`    | lifecycle steps · gauged paint        | framebuffer · HUD answers   |
|  [05]   | `LightAuthorities` | `LightAuthorityProgram`              | `SeatRegistry.Claim` · `Answer`       | `SeatToken` · host CRUD     |
|  [06]   | `Effects`          | `PostEffectOp` · `EffectSource`      | settings demand · host registration   | `EffectRoster`              |
|  [07]   | `EffectHost`       | `EffectProgram`                      | change bracket · `EffectPass` borrows | host effect answers         |
|  [08]   | `TextureBake`      | `ContentRef` · mode row              | evaluator or bake inside a demand     | caller-shaped detached row  |
|  [09]   | `SceneQueue`       | host `Apply*` hooks · `QueueDrive`   | `QueueCell` steps · bounded channel   | `SceneBatch` off the reader |
|  [10]   | `SceneMarks`       | `SceneBatch` · `Canvas`              | held custody · `Marks.Paint`          | `DrawTally`                 |

## [07]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
