# [RASM_RHINO_DISPLAY_RENDER]

`RenderJob` owns batch execution and scoped window operations, detached onto an engine thread through `JobAsync` when an `AsyncProgram` rides the open; `RealtimeEngine` owns progressive viewport participation, `LightAuthorities` owns engine-side custom-light authority over the host light manager, and `SceneQueue` owns live scene-change delivery beside them over the host `ChangeQueue`. `Effects.Configure` owns settings mutation, and `TextureBake` resolves content identities inside a document demand.

`RenderPipeline`, `RenderWindow`, `RenderTexture`, and every `ChangeQueue` payload remain internal host handles. Consumers receive `RenderReceipt`, `RealtimeStart`, `EffectRoster`, detached texture results, or detached `SceneDelta` batches off a channel reader.

## [01]-[INDEX]

- [02]-[BATCH_SESSION]: `RenderRun`, `RenderRequest`, `RenderChannel`, `PixelBlock`, `RenderJob`, and the `AsyncProgram`/`JobAsync` detached-thread modality over `AsyncRenderContext`.
- [03]-[REALTIME]: `RealtimeProgram` hooks, `RealtimePassPolicy`, the `RealtimeEngine` adapter over `RealtimeDisplayMode`, and the `LightAuthorities` custom-light authority over `LightManagerSupport`.
- [04]-[POST_AND_TEXTURE]: `PostEffectOp` configuration rows over `RenderSettings.PostEffects`, `PostEffectGate` execution control, and the `TextureBake` evaluation rows.
- [05]-[CHANGEQUEUE]: `SceneDelta`, `SceneBatch`, `QueuePolicy`, and the `SceneQueue` adapter over the host change queue with the channel hand-off seam.

## [02]-[BATCH_SESSION]

- Owner: `RenderRun` closes full-frame and region execution; `WindowScope` closes pipeline, viewport-target, and detached window acquisition; `WindowOp` closes channel opening, pixel writes, and post-effect gating.
- Entry: `RenderJob.Open(DocumentSession, PlugIn, Size2i, ChannelSet, RenderProgram, Op?) : Fin<RenderJob>` admits the plan; every `Configure` re-resolves the document, proves request-owned needs, and binds the matching pipeline inside that demand.
- Law: one lifecycle gate excludes disposal across a complete configuration demand; a document-serial change retires the prior pipeline before the current demand mints its replacement.
- Law: every `GetRenderWindow*` call remains inside private `WithWindow`; `WindowOp` is the only public operation vocabulary.
- Law: batch and realtime never merge — a `RenderJob` produces a finished window, a `RealtimeEngine` participates per frame; one owner claiming both is the collapsed form the host API's own split forecloses.
- Law: the opened channel is the only per-pixel path — a raw buffer pointer beside `OpenChannel`/`SetRGBAChannelColors` is unrepresentable because the block is the sole write carrier.
- Law: the detached-thread modality is `Option<AsyncProgram>` on `Open`, never a sibling job type — when present, `JobPipeline` binds one `JobAsync : AsyncRenderContext` at construction, `OnRenderBegin` launches the engine thread after the program's `Begin`, and a failed launch fails the begin so no orphan thread survives the rail.
- Law: the detached body writes through the same `RealtimePort` pixel carrier the realtime engine owns and halts on the token `StopRendering` trips; the host stop joins the thread, closes the port, and runs the program's `Stopped` hook before the base cancel flag sets.
- Law: `RenderProgram.SetPaused` owns pause state; host pause callbacks and `RenderRequest` pause rows enter the same caught rail, and `JobPipeline.SupportsPause` advertises only that admitted capability.
- Boundary: `ViewportTarget` enters through `ViewportLease`; disposable `ViewportInfo` values exist only inside its borrow and never cross the render contract.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System.Threading.Channels;
using Rasm.Analysis;
using Rasm.Domain;
using Rasm.Drawing;
using Rasm.Meshing;
using Rasm.Numerics;
using Rasm.Parametric;
using Rasm.Rhino.Document;
using Rasm.Rhino.Render;
using Rasm.Rhino.Viewport;
using Rasm.Spatial;
using Rhino.Geometry;
using Rhino.Render;

namespace Rasm.Rhino.Display;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class RenderChannel {
    public static readonly RenderChannel Red = new(0, RenderWindow.StandardChannels.Red);
    public static readonly RenderChannel Green = new(1, RenderWindow.StandardChannels.Green);
    public static readonly RenderChannel Blue = new(2, RenderWindow.StandardChannels.Blue);
    public static readonly RenderChannel Alpha = new(3, RenderWindow.StandardChannels.Alpha);
    public static readonly RenderChannel Rgba = new(4, RenderWindow.StandardChannels.RGBA);
    public static readonly RenderChannel Rgb = new(5, RenderWindow.StandardChannels.RGB);
    public static readonly RenderChannel Distance = new(6, RenderWindow.StandardChannels.DistanceFromCamera);
    public static readonly RenderChannel NormalX = new(7, RenderWindow.StandardChannels.NormalX);
    public static readonly RenderChannel NormalY = new(8, RenderWindow.StandardChannels.NormalY);
    public static readonly RenderChannel NormalZ = new(9, RenderWindow.StandardChannels.NormalZ);
    public static readonly RenderChannel Normal = new(10, RenderWindow.StandardChannels.NormalXYZ);
    public static readonly RenderChannel LuminanceRed = new(11, RenderWindow.StandardChannels.LuminanceRed);
    public static readonly RenderChannel LuminanceGreen = new(12, RenderWindow.StandardChannels.LuminanceGreen);
    public static readonly RenderChannel LuminanceBlue = new(13, RenderWindow.StandardChannels.LuminanceBlue);
    public static readonly RenderChannel BackgroundLuminanceRed = new(14, RenderWindow.StandardChannels.BackgroundLuminanceRed);
    public static readonly RenderChannel BackgroundLuminanceGreen = new(15, RenderWindow.StandardChannels.BackgroundLuminanceGreen);
    public static readonly RenderChannel BackgroundLuminanceBlue = new(16, RenderWindow.StandardChannels.BackgroundLuminanceBlue);
    public static readonly RenderChannel MaterialIds = new(17, RenderWindow.StandardChannels.MaterialIds);
    public static readonly RenderChannel ObjectIds = new(18, RenderWindow.StandardChannels.ObjectIds);
    public static readonly RenderChannel Wireframe = new(19, RenderWindow.StandardChannels.Wireframe);
    public static readonly RenderChannel AlbedoRed = new(20, RenderWindow.StandardChannels.AlbedoRed);
    public static readonly RenderChannel AlbedoGreen = new(21, RenderWindow.StandardChannels.AlbedoGreen);
    public static readonly RenderChannel AlbedoBlue = new(22, RenderWindow.StandardChannels.AlbedoBlue);
    public static readonly RenderChannel Albedo = new(23, RenderWindow.StandardChannels.AlbedoRGB);
    public static readonly RenderChannel WireframePoints = new(24, RenderWindow.StandardChannels.WireframePointsRGBA);
    public static readonly RenderChannel WireframeIsocurves = new(25, RenderWindow.StandardChannels.WireframeIsocurvesRGBA);
    public static readonly RenderChannel WireframeCurves = new(26, RenderWindow.StandardChannels.WireframeCurvesRGBA);
    public static readonly RenderChannel WireframeAnnotations = new(27, RenderWindow.StandardChannels.WireframeAnnotationsRGBA);

    internal RenderWindow.StandardChannels Native { get; }
}

[SmartEnum<int>]
public sealed partial class RegionPlacement {
    public static readonly RegionPlacement Offscreen = new(0, false);
    public static readonly RegionPlacement Window = new(1, true);
    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class WireframeChannel {
    public static readonly WireframeChannel Omit = new(0, false);
    public static readonly WireframeChannel Include = new(1, true);
    internal bool Native { get; }
}

[SmartEnum<int>]
public sealed partial class RenderViewSource {
    public static readonly RenderViewSource Pipeline = new(0, false);
    public static readonly RenderViewSource RenderView = new(1, true);
    internal bool Native { get; }
}

[ValueObject<Guid>(ConversionToKeyMemberType = ConversionOperatorsGeneration.Implicit)]
public readonly partial struct EffectId {
    static partial void ValidateFactoryArguments(ref ValidationError? validationError, ref Guid value) =>
        validationError = value == Guid.Empty ? new ValidationError(message: "Post-effect identity is empty.") : null;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderRun {
    private RenderRun() { }
    public sealed record Frame : RenderRun;
    public sealed record Region(ViewportTarget Target, Offset2i Origin, Size2i Extent, RegionPlacement Placement) : RenderRun;

    internal bool Valid => Switch(
        frame: static _ => true,
        region: static row => row.Target is not null
            && row.Placement is not null
            && row.Extent.Width > 0
            && row.Extent.Height > 0);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowOp {
    private WindowOp() { }
    public sealed record Open(RenderChannel Channel) : WindowOp;
    public sealed record Write(PixelBlock Block) : WindowOp;
    public sealed record Gate(Func<EffectId, Fin<bool>> Decide) : WindowOp;

    internal bool Valid => Switch(
        open: static row => row.Channel is not null,
        write: static row => row.Block is not null,
        gate: static row => row.Decide is not null);

    internal Fin<Unit> Apply(RenderWindow window, Action<Error> reject, Op key) => Switch(
        (Window: window, Reject: reject, Op: key),
        open: static (ctx, row) => ctx.Op.Catch(() => {
            using RenderWindow.Channel channel = ctx.Window.OpenChannel(row.Channel.Native);
            return Optional(channel).ToFin(ctx.Op.InvalidResult()).Map(static _ => unit);
        }),
        write: static (ctx, row) => row.Block.Blit(ctx.Window, ctx.Op),
        gate: static (ctx, row) => PostEffectGate.Register(ctx.Window, row.Decide, ctx.Reject, ctx.Op));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderRequest {
    private RenderRequest() { }
    public sealed record Run(RenderRun Scope) : RenderRequest;
    public sealed record Pause : RenderRequest;
    public sealed record Resume : RenderRequest;
    public sealed record Window(WindowScope Scope, Seq<WindowOp> Operations) : RenderRequest;

    internal bool Valid => Switch(
        run: static row => row.Scope is not null && row.Scope.Valid,
        pause: static _ => true,
        resume: static _ => true,
        window: static row => row.Scope is not null
            && row.Scope.Valid
            && !row.Operations.IsEmpty
            && row.Operations.ForAll(static operation => operation is not null && operation.Valid));

    internal Seq<SessionNeed> Needs => Seq(SessionNeed.Read);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record RenderReceipt : IDetachedDocumentResult {
    private RenderReceipt() { }
    public sealed record Ran(RenderRun Scope) : RenderReceipt;
    public sealed record Paused : RenderReceipt;
    public sealed record Resumed : RenderReceipt;
    public sealed record Windowed(int Operations) : RenderReceipt;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record WindowScope {
    private WindowScope() { }
    public sealed record SessionCase(WireframeChannel Wireframe, RenderViewSource Source) : WindowScope;
    public sealed record ViewportCase(ViewportTarget Target, RenderViewSource Source, Offset2i Origin, Size2i Extent) : WindowScope;
    public sealed record DetachedCase(ViewportTarget Target, Size2i Extent) : WindowScope;

    internal bool Valid => Switch(
        sessionCase: static row => row.Wireframe is not null && row.Source is not null,
        viewportCase: static row => row.Target is not null
            && row.Source is not null
            && row.Extent.Width > 0
            && row.Extent.Height > 0,
        detachedCase: static row => row.Target is not null && row.Extent.Width > 0 && row.Extent.Height > 0);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ChannelSet {
    private ChannelSet(Seq<RenderChannel> rows) => Rows = rows;
    public Seq<RenderChannel> Rows { get; }
    public static ChannelSet Rgba { get; } = new([RenderChannel.Rgba]);

    public static Fin<ChannelSet> Of(Seq<RenderChannel> rows, Op? key = null) =>
        guard(!rows.IsEmpty && rows.ForAll(static row => row is not null), key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new ChannelSet(toSeq(rows.AsEnumerable().Distinct())));

    internal RenderWindow.StandardChannels Flags =>
        Rows.Fold(default(RenderWindow.StandardChannels), static (mask, row) => mask | row.Native);

    internal Fin<Unit> OpenOn(RenderWindow window, Op? key = null) {
        Op op = key.OrDefault();
        return Rows.TraverseM(row => op.Catch(() => {
            using RenderWindow.Channel channel = window.OpenChannel(row.Native);
            return Optional(channel).ToFin(Fail: op.InvalidResult(detail: row.ToString())).Map(static _ => unit);
        })).As().Map(static _ => unit);
    }
}

public sealed record PixelBlock {
    private PixelBlock(Offset2i origin, Size2i extent, ReadOnlyMemory<Color4f> pixels) =>
        (Origin, Extent, Pixels) = (origin, extent, pixels);
    public Offset2i Origin { get; }
    public Size2i Extent { get; }
    public ReadOnlyMemory<Color4f> Pixels { get; }

    public static Fin<PixelBlock> Of(Offset2i origin, Size2i extent, ReadOnlyMemory<Color4f> pixels, Op? key = null) =>
        guard(extent.Width > 0
            && extent.Height > 0
            && (long)pixels.Length == (long)extent.Width * extent.Height, key.OrDefault().InvalidInput()).ToFin()
            .Map(_ => new PixelBlock(origin, extent, pixels));

    internal Fin<Unit> Blit(RenderWindow window, Op key) {
        PixelBlock self = this;
        return key.Catch(() => {
            System.Drawing.Rectangle region = self.Origin.Window(extent: self.Extent);
            window.SetRGBAChannelColors(rectangle: region, colors: self.Pixels.ToArray());
            window.InvalidateArea(region);
            return Fin.Succ(value: unit);
        });
    }
}

public sealed record RenderProgram(
    Func<Fin<Unit>> Begin,
    Func<System.Drawing.Rectangle, Fin<Unit>> BeginRegion,
    Func<Fin<Unit>> End,
    Func<Fin<bool>> Continue,
    Func<bool, Fin<Unit>> SetPaused) {
    internal bool Valid => Begin is not null
        && BeginRegion is not null
        && End is not null
        && Continue is not null
        && SetPaused is not null;
}

public sealed record AsyncProgram(
    Func<RealtimePort, CancellationToken, Fin<Unit>> Render,
    string ThreadName,
    Option<Func<Fin<Unit>>> Stopped = default);

// --- [SERVICES] -----------------------------------------------------------------------------
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
    private readonly Action<Error> record;
    private readonly Lock lifecycleGate = new();
    private readonly Op key;
    private JobAsyncLifecycle lifecycle = new JobAsyncLifecycle.Idle();

    internal JobAsync(AsyncProgram program, Action<Error> record, Op key) =>
        (this.program, this.record, this.key) = (program, record, key);

    internal Fin<Unit> Launch() {
        lock (lifecycleGate) {
            return from _ in guard(lifecycle is JobAsyncLifecycle.Idle, key.InvalidContext()).ToFin()
                   from window in Optional(RenderWindow).ToFin(Fail: key.MissingContext())
                   let opened = new RealtimePort(window, key)
                   from installed in Fin.Succ(Op.Side(() => lifecycle = new JobAsyncLifecycle.Running(opened)))
                   from started in key.Catch(() => key.Confirm(success: StartRenderThread(
                           threadStart: () => ignore(key.Catch(() => program.Render(opened, halt.Token)).IfFail(record)),
                           threadName: program.ThreadName)))
                       .BindFail(failure => (
                           Op.Side(() => lifecycle = new JobAsyncLifecycle.Idle()),
                           opened.Close(),
                           Fin.Fail<Unit>(failure)).Item3)
                   select unit;
        }
    }

    public override void StopRendering() {
        lock (lifecycleGate) {
            if (lifecycle is JobAsyncLifecycle.Stopped) { return; }
            JobAsyncLifecycle prior = lifecycle;
            lifecycle = new JobAsyncLifecycle.Stopped();
            halt.Cancel();
            JoinRenderThread();
            _ = prior.Switch(
                idle: static _ => unit,
                running: static row => row.Port.Close(),
                stopped: static _ => unit);
            _ = program.Stopped.Iter(hook => ignore(key.Catch(hook).IfFail(record)));
            base.StopRendering();
        }
    }

    protected override void Dispose(bool isDisposing) {
        _ = Op.SideWhen(isDisposing, halt.Dispose);
        base.Dispose(isDisposing);
    }
}

internal sealed class JobPipeline : RenderPipeline {
    private readonly RenderProgram program;
    private readonly Option<JobAsync> detached;
    private readonly Atom<Seq<Error>> faults;
    private readonly Op key;

    internal JobPipeline(
        RhinoDoc document,
        RunMode mode,
        PlugIn plugin,
        Size2i extent,
        ChannelSet channels,
        RenderProgram program,
        Option<AsyncProgram> render,
        Atom<Seq<Error>> faults,
        Op key)
        : base(document, mode, plugin, extent.Native, plugin.Name, channels.Flags, reuseRenderWindow: false, clearLastRendering: true) {
        (this.program, this.faults, this.key) = (program, faults, key);
        detached = render.Map(plan => new JobAsync(plan, record: failure => Record(failure), key: key));
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
        Accept(key.Catch(program.Begin)) && detached.Match(
            Some: context => Accept(context.Launch()),
            None: static () => true);

    protected override bool OnRenderWindowBegin(RhinoView view, System.Drawing.Rectangle rect) =>
        Accept(key.Catch(() => program.BeginRegion(rect)));

    protected override void OnRenderEnd(RenderEndEventArgs e) => ignore(Accept(key.Catch(program.End)));

    public override bool SupportsPause() => true;

    public override void PauseRendering() => ignore(Accept(SetPaused(paused: true)));

    public override void ResumeRendering() => ignore(Accept(SetPaused(paused: false)));

    protected override bool ContinueModal() => key.Catch(program.Continue).Match(
        Succ: static value => value,
        Fail: failure => { Record(failure); return false; });

    internal Fin<Unit> SetPaused(bool paused) => key.Catch(() => program.SetPaused(paused));

    internal Unit Record(Error failure) => ignore(faults.Swap(rows => rows.Add(failure)));

    private bool Accept(Fin<Unit> result) => result.Match(
        Succ: static _ => true,
        Fail: failure => { Record(failure); return false; });
}

public sealed class RenderJob : IDisposable, IDetachedDocumentResult {
    private readonly DocumentSession session;
    private readonly PlugIn owner;
    private readonly Size2i extent;
    private readonly ChannelSet channels;
    private readonly RenderProgram program;
    private readonly Option<AsyncProgram> render;
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly Lock lifecycle = new();
    private readonly Op key;
    private JobPipeline? pipeline;
    private uint documentSerial;
    private int released;

    private RenderJob(DocumentSession session, PlugIn owner, Size2i extent, ChannelSet channels, RenderProgram program, Option<AsyncProgram> render, Op key) =>
        (this.session, this.owner, this.extent, this.channels, this.program, this.render, this.key) =
        (session, owner, extent, channels, program, render, key);

    public static Fin<RenderJob> Open(DocumentSession session, PlugIn owner, Size2i extent, ChannelSet channels, RenderProgram program, Option<AsyncProgram> render = default, Op? key = null) {
        Op op = key.OrDefault();
        return from documentSession in Optional(session).ToFin(Fail: op.MissingContext())
               from plugin in Optional(owner).ToFin(Fail: op.InvalidInput())
               from channelSet in Optional(channels).ToFin(Fail: op.InvalidInput())
               from plan in Optional(program).Filter(static value => value.Valid).ToFin(Fail: op.InvalidInput())
               from _ in guard(extent.Width > 0 && extent.Height > 0, op.InvalidInput())
               from __ in guard(render.Match(
                   Some: static detached => detached is { Render: not null } && !string.IsNullOrWhiteSpace(detached.ThreadName),
                   None: static () => true), op.InvalidInput())
               select new RenderJob(documentSession, plugin, extent, channelSet, plan, render, op);
    }

    public Seq<Error> Faults => faults.Value;

    public Fin<RenderReceipt> Configure(RenderRequest request, Op? key = null) {
        Op op = key.OrDefault();
        lock (lifecycle) {
            return guard(Volatile.Read(ref released) == 0, op.InvalidContext()).ToFin()
                .Bind(_ => guard(request is not null && request.Valid, op.InvalidInput()).ToFin())
                .Bind(_ => session.Demand(
                    use: document => Current(document, op).Bind(current => Apply(current, request, op)),
                    key: op,
                    needs: request.Needs.ToArray()));
        }
    }

    private Fin<RenderReceipt> Apply(JobPipeline current, RenderRequest request, Op key) => request.Switch(
        (Job: this, Pipeline: current, Op: key),
        run: static (ctx, row) => ctx.Job.Run(ctx.Pipeline, row.Scope, ctx.Op)
            .Map(_ => (RenderReceipt)new RenderReceipt.Ran(row.Scope)),
        pause: static (ctx, _) => ctx.Pipeline.SetPaused(paused: true)
            .Map(static _ => (RenderReceipt)new RenderReceipt.Paused()),
        resume: static (ctx, _) => ctx.Pipeline.SetPaused(paused: false)
            .Map(static _ => (RenderReceipt)new RenderReceipt.Resumed()),
        window: static (ctx, row) => ctx.Job.WithWindow(ctx.Pipeline, row.Scope, window => row.Operations
            .TraverseM(operation => operation.Apply(window, ctx.Pipeline.Record, ctx.Op)).As()
            .Map(done => (RenderReceipt)new RenderReceipt.Windowed(done.Count)), ctx.Op));

    private Fin<JobPipeline> Current(RhinoDoc document, Op op) =>
        pipeline is { } current && documentSerial == document.RuntimeSerialNumber
            ? Fin.Succ(current)
            : Retire(op).Bind(_ => op.Catch(() => {
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
                    faults: faults,
                    key: key);
                (pipeline, documentSerial) = (replacement, document.RuntimeSerialNumber);
                return Fin.Succ(replacement);
            }));

    private Fin<Unit> Run(JobPipeline current, RenderRun scope, Op key) {
        Op op = key.OrDefault();
        RenderJob self = this;
        return scope.Switch(
            state: (Job: self, Pipeline: current, Op: op),
            frame: static (ctx, _) => ctx.Op.Catch(() =>
                ctx.Op.Confirm(success: ctx.Pipeline.Render() == RenderPipeline.RenderReturnCode.Ok)),
            region: static (ctx, request) =>
                from lease in ViewportLease.Of(session: ctx.Job.session, target: request.Target, key: ctx.Op)
                from _ in lease.Use(borrow: row => ctx.Op.Catch(() => ctx.Op.Confirm(
                    success: ctx.Pipeline.RenderWindow(
                        view: row.View,
                        rect: request.Origin.Window(extent: request.Extent),
                        inWindow: request.Placement.Native) == RenderPipeline.RenderReturnCode.Ok)), key: ctx.Op)
                select unit);
    }

    private Fin<TOut> WithWindow<TOut>(JobPipeline current, WindowScope scope, Func<RenderWindow, Fin<TOut>> borrow, Op? key = null) {
        Op op = key.OrDefault();
        return scope.Switch(
            state: (Job: this, Pipeline: current, Borrow: borrow, Op: op),
            sessionCase: static (ctx, request) => ctx.Job.BorrowWindow(
                mint: () => ctx.Pipeline.GetRenderWindow(request.Wireframe.Native, request.Source.Native),
                borrow: ctx.Borrow,
                key: ctx.Op),
            viewportCase: static (ctx, request) =>
                from lease in ViewportLease.Of(ctx.Job.session, request.Target, ctx.Op)
                from result in lease.Use(row => row.Info(info => ctx.Job.BorrowWindow(
                    mint: () => ctx.Pipeline.GetRenderWindow(info, request.Source.Native, request.Origin.Window(request.Extent)),
                    borrow: ctx.Borrow,
                    key: ctx.Op), ctx.Op), ctx.Op)
                select result,
            detachedCase: static (ctx, request) =>
                from lease in ViewportLease.Of(ctx.Job.session, request.Target, ctx.Op)
                from result in lease.Use(row => row.Info(info => ctx.Job.BorrowWindow(
                    mint: () => {
                        RenderWindow window = RenderWindow.Create(request.Extent.Native);
                        window.SetView(info);
                        return window;
                    },
                    borrow: ctx.Borrow,
                    key: ctx.Op), ctx.Op), ctx.Op)
                select result);
    }

    private Fin<TOut> BorrowWindow<TOut>(Func<RenderWindow> mint, Func<RenderWindow, Fin<TOut>> borrow, Op key) =>
        key.Catch(() => {
            using RenderWindow window = mint();
            return Optional(window).ToFin(Fail: key.InvalidResult()).Bind(borrow);
        });

    private Fin<Unit> Retire(Op op) {
        JobPipeline? current = pipeline;
        (pipeline, documentSerial) = (null, 0u);
        if (current is null) { return Fin.Succ(unit); }
        Seq<Error> failures = Seq(
                op.Catch(() => Fin.Succ(current.Halt())),
                op.Catch(() => { current.Dispose(); return Fin.Succ(unit); }))
            .Choose(static result => result.Match(
                Succ: static _ => Option<Error>.None,
                Fail: static failure => Some(failure)));
        return failures.IsEmpty
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(failures.Fold(Errors.None, static (combined, failure) => combined + failure));
    }

    public void Dispose() {
        lock (lifecycle) {
            _ = Op.SideWhen(
                Interlocked.Exchange(location1: ref released, value: 1) is 0,
                () => ignore(Retire(key).IfFail(failure => ignore(faults.Swap(rows => rows.Add(failure))))));
        }
    }
}

```

## [03]-[REALTIME]

- Owner: `RealtimeProgram` carries detached lifecycle queries and mark projections; `RealtimePassPolicy` carries pass, post-effect, and OpenGL policy; `LightAuthorityProgram` carries the engine-owned custom-light CRUD hooks with `LightAuthorities` as the engine-keyed program registry and fault ledger.
- Entry: `RealtimeEngine` adapts the complete `RealtimeDisplayMode` abstract and event surface; `LightAuthorityHost` adapts the complete `LightManagerSupport` abstract surface, and because `RegisterLightManager` discovers and constructs the plugin's sealed subclass itself, that subclass carries only its two identity guids.
- Law: framebuffer and middleground hooks project `Seq<Mark>`; the adapter alone receives `DisplayPipeline` and renders through `Marks.Render`.
- Law: `RealtimePort` exposes only `PixelBlock.Write`, remains live for the progressive session, and closes before `Shutdown`; the engine alone controls its lifetime and never exports `RenderWindow`.
- Law: the registered authority is the one source of truth for engine-owned lights — document lights stay on the Objects lights rail, a parallel light registry beside the authority is rejected, and every authority hook receives the detached `DocKey`, never the live document.
- Law: engine registration claims one token-keyed slot atomically; any occupied engine refuses before host registration, and host-registration failure removes only its matching claim.
- Law: `LightChange` closes the host custom-event vocabulary and `LightAuthorities.Notify` is the one egress announcing an engine-light mutation back to the host light manager; solo behavior is an `Option<LightSolo>` row whose absence leaves the host's own solo storage standing.
- Boundary: callback failures accumulate in `Faults`; no event handler swallows a failed rail.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HudSignal {
    private HudSignal() { }
    public sealed record PlayCase : HudSignal;
    public sealed record PauseCase : HudSignal;
    public sealed record LockCase : HudSignal;
    public sealed record UnlockCase : HudSignal;
    public sealed record MaxPassesCase(int Passes) : HudSignal;
}

[SmartEnum<int>]
public sealed partial class RealtimeFeature {
    public static readonly RealtimeFeature PostEffects = new(0);
    public static readonly RealtimeFeature OpenGl = new(1);
}

[SmartEnum<int>]
public sealed partial class HudFeature {
    public static readonly HudFeature Show = new(0);
    public static readonly HudFeature Controls = new(1);
    public static readonly HudFeature Passes = new(2);
    public static readonly HudFeature MaxPasses = new(3);
    public static readonly HudFeature EditMaxPasses = new(4);
}

[SmartEnum<int>]
public sealed partial class RenderIntent {
    public static readonly RenderIntent Viewport = new(0);
    public static readonly RenderIntent Capture = new(1);
    internal static RenderIntent Of(bool capture) => capture ? Capture : Viewport;
}

[SmartEnum<LightMangerSupportCustomEvent>]
public sealed partial class LightChange {
    public static readonly LightChange Added = new(LightMangerSupportCustomEvent.light_added);
    public static readonly LightChange Deleted = new(LightMangerSupportCustomEvent.light_deleted);
    public static readonly LightChange Undeleted = new(LightMangerSupportCustomEvent.light_undeleted);
    public static readonly LightChange Modified = new(LightMangerSupportCustomEvent.light_modified);
    public static readonly LightChange Sorted = new(LightMangerSupportCustomEvent.light_sorted);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record RealtimePassPolicy {
    private RealtimePassPolicy(int maxPasses, Seq<RealtimeFeature> features) =>
        (MaxPasses, Features) = (maxPasses, features);
    public int MaxPasses { get; }
    public Seq<RealtimeFeature> Features { get; }
    public static Fin<RealtimePassPolicy> Of(int maxPasses, Seq<RealtimeFeature> features, Op? key = null) =>
        from value in key.OrDefault().Positive(maxPasses)
        from _ in guard(features.ForAll(static feature => feature is not null), key.OrDefault().InvalidInput())
        select new RealtimePassPolicy(value, toSeq(features.AsEnumerable().Distinct()));
}

public sealed record RealtimeChrome {
    private RealtimeChrome(string productName, Seq<HudFeature> features, Option<Func<string>> status, Option<DateTime> started) =>
        (ProductName, Features, Status, Started) = (productName, features, status, started);

    public string ProductName { get; }
    public Seq<HudFeature> Features { get; }
    public Option<Func<string>> Status { get; }
    public Option<DateTime> Started { get; }

    public static Fin<RealtimeChrome> Of(
        string productName,
        Seq<HudFeature> features,
        Option<Func<string>> status = default,
        Option<DateTime> started = default,
        Op? key = null) =>
        guard(!string.IsNullOrWhiteSpace(productName)
            && features.ForAll(static feature => feature is not null)
            && status.Match(Some: static value => value is not null, None: static () => true), key.OrDefault().InvalidInput())
            .ToFin()
            .Map(_ => new RealtimeChrome(productName, toSeq(features.AsEnumerable().Distinct()), status, started));
}

public readonly record struct RealtimeStart(Size2i Extent, DocKey Document, RenderIntent Intent, RealtimePort Pixels);

public sealed class RealtimePort {
    private RenderWindow? window;
    private readonly Op key;
    internal RealtimePort(RenderWindow window, Op key) => (this.window, this.key) = (window, key);
    public Fin<Unit> Write(PixelBlock block) =>
        from value in Optional(block).ToFin(key.InvalidInput())
        from target in Optional(Volatile.Read(ref window)).ToFin(key.InvalidContext())
        from written in value.Blit(target, key)
        select written;
    internal Unit Close() => ignore(Interlocked.Exchange(ref window, null));
}

public sealed record RealtimeProgram(
    Func<RealtimeStart, Fin<Unit>> Start,
    Func<Fin<Unit>> Shutdown,
    Func<bool> Started,
    Func<bool> Completed,
    Func<Size2i> RenderSize,
    Func<int> LastPass,
    Func<Size2i, Fin<Unit>> Resized,
    Func<ConduitFrame, Fin<Seq<Mark>>> InitFramebuffer,
    Func<ConduitFrame, Fin<Seq<Mark>>> DrawMiddleground,
    Func<Fin<Unit>> SettingsChanged,
    Option<Func<HudSignal, Fin<Unit>>> Hud = default);

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
internal abstract partial record RealtimeLifecycle {
    private RealtimeLifecycle() { }
    internal sealed record Idle : RealtimeLifecycle;
    internal sealed record Running(SpriteSheet Sprites, RealtimePort Pixels) : RealtimeLifecycle;
}

public sealed record LightSolo(
    Func<DocKey, Guid, bool, Fin<bool>> Set,
    Func<DocKey, Guid, Fin<bool>> Get,
    Func<DocKey, Fin<int>> Count);

public sealed record LightAuthorityProgram(
    Func<DocKey, Fin<Seq<Light>>> Roster,
    Func<DocKey, Guid, Fin<Option<Light>>> Resolve,
    Func<DocKey, Light, Fin<Unit>> Amend,
    Func<DocKey, Light, bool, Fin<bool>> Retire,
    Func<DocKey, Light, Fin<int>> Serial,
    Func<DocKey, Light, Fin<string>> Describe,
    Func<DocKey, Seq<Light>, Fin<bool>> Edit,
    Func<DocKey, Seq<Light>, Fin<Unit>> Group,
    Func<DocKey, Seq<Light>, Fin<Unit>> Ungroup,
    Option<LightSolo> Solo = default);

// --- [SERVICES] -----------------------------------------------------------------------------
public sealed class RealtimeEngine : RealtimeDisplayMode {
    private readonly RealtimeProgram program;
    private readonly RealtimeChrome chrome;
    private readonly Atom<bool> framebufferReady = Atom(false);
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly Lock lifecycleGate = new();
    private RealtimeLifecycle lifecycle = new RealtimeLifecycle.Idle();
    private readonly Op key;

    private RealtimeEngine(RealtimeProgram program, RealtimePassPolicy policy, RealtimeChrome chrome, Op key) {
        this.program = program;
        this.chrome = chrome;
        this.key = key;
        MaxPasses = policy.MaxPasses;
        PostEffectsOn = policy.Features.Exists(static feature => feature == RealtimeFeature.PostEffects);
        SetUseDrawOpenGl(policy.Features.Exists(static feature => feature == RealtimeFeature.OpenGl));
        OnInitFramebuffer += (_, e) => {
            ConduitFrame frame = Frame(e.Pipeline);
            Fin<RealtimeLifecycle.Running> initialized = Observe(RenderMarks(frame, program.InitFramebuffer));
            _ = CommitFramebuffer(initialized);
        };
        OnDrawMiddleground += (_, e) => {
            ConduitFrame frame = Frame(e.Pipeline);
            _ = Observe(RenderMarks(frame, program.DrawMiddleground));
        };
        OnDisplayPipelineSettingsChanged += (_, _) => _ = Observe(this.key.Catch(program.SettingsChanged));
        _ = program.Hud.Iter(signal => {
            HudPlayButtonPressed += (_, _) => ignore(Observe(this.key.Catch(() => signal(new HudSignal.PlayCase()))));
            HudPauseButtonPressed += (_, _) => ignore(Observe(this.key.Catch(() => signal(new HudSignal.PauseCase()))));
            HudLockButtonPressed += (_, _) => ignore(Observe(this.key.Catch(() => signal(new HudSignal.LockCase()))));
            HudUnlockButtonPressed += (_, _) => ignore(Observe(this.key.Catch(() => signal(new HudSignal.UnlockCase()))));
            MaxPassesChanged += (_, e) => ignore(Observe(this.key.Catch(() => signal(new HudSignal.MaxPassesCase(Passes: e.MaxPasses)))));
        });
    }

    public static Fin<RealtimeEngine> Of(RealtimeProgram program, RealtimePassPolicy policy, RealtimeChrome chrome, Op? key = null) {
        Op op = key.OrDefault();
        return guard(program is not null && policy is not null && chrome is not null, op.InvalidInput()).ToFin()
            .Map(_ => new RealtimeEngine(program, policy, chrome, op));
    }

    public Seq<Error> Faults => faults.Value;

    public override void GetRenderSize(out int width, out int height) {
        Size2i extent = Observe(key.Catch(() => Fin.Succ(program.RenderSize())), default(Size2i));
        (width, height) = (extent.Width, extent.Height);
    }

    public override int LastRenderedPass() => Observe(key.Catch(() => Fin.Succ(program.LastPass())), 0);

    public override string HudProductName() => chrome.ProductName;

    public override bool HudShow() => chrome.Features.Exists(static feature => feature == HudFeature.Show);

    public override bool HudShowControls() => chrome.Features.Exists(static feature => feature == HudFeature.Controls);

    public override bool HudShowPasses() => chrome.Features.Exists(static feature => feature == HudFeature.Passes);

    public override bool HudShowMaxPasses() => chrome.Features.Exists(static feature => feature == HudFeature.MaxPasses);

    public override bool HudAllowEditMaxPasses() => chrome.Features.Exists(static feature => feature == HudFeature.EditMaxPasses);

    public override bool HudShowCustomStatusText() => chrome.Status.IsSome;

    public override string HudCustomStatusText() => chrome.Status.Match(
        Some: status => Observe(key.Catch(() => Fin.Succ(status())), string.Empty),
        None: static () => string.Empty);

    public override int HudLastRenderedPass() => LastRenderedPass();

    public override DateTime HudStartTime() => chrome.Started.IfNone(() => base.HudStartTime());

    public override bool StartRenderer(int w, int h, RhinoDoc doc, ViewInfo view, ViewportInfo viewportInfo, bool forCapture, RenderWindow renderWindow) {
        lock (lifecycleGate) {
            if (lifecycle is not RealtimeLifecycle.Idle) { return false; }
            RealtimeLifecycle.Running owned = new(new SpriteSheet(), new RealtimePort(renderWindow, key));
            Fin<Unit> started = from extent in Size2i.Of(w, h, key)
                                from document in DocKey.Of(doc, key)
                                from _ in key.Catch(() => program.Start(new RealtimeStart(
                                    extent,
                                    document,
                                    RenderIntent.Of(forCapture),
                                    owned.Pixels)))
                                select unit;
            Fin<Unit> settled = started.IsSucc ? started : Release(owned, started);
            Fin<Unit> observed = Observe(settled);
            _ = Op.SideWhen(observed.IsSucc, () => lifecycle = owned);
            return observed.IsSucc;
        }
    }

    public override void ShutdownRenderer() {
        lock (lifecycleGate) {
            _ = framebufferReady.Swap(_ => false);
            RealtimeLifecycle prior = lifecycle;
            lifecycle = new RealtimeLifecycle.Idle();
            _ = prior.Switch(
                idle: static _ => unit,
                running: running => ignore(Observe(Release(running, key.Catch(program.Shutdown)))));
        }
    }

    public override bool IsRendererStarted() => Observe(key.Catch(() => Fin.Succ(program.Started())), false);

    public override bool IsCompleted() => Observe(key.Catch(() => Fin.Succ(program.Completed())), false);

    public override bool IsFrameBufferAvailable(ViewInfo view) => framebufferReady.Value;

    public override bool OnRenderSizeChanged(int width, int height) =>
        Observe(Size2i.Of(width: width, height: height, key: key)
            .Bind(extent => key.Catch(() => program.Resized(extent)))).IsSucc;

    public Fin<Unit> Redraw() => Observe(key.Catch(SignalRedraw));

    private Fin<RealtimeLifecycle.Running> RenderMarks(ConduitFrame frame, Func<ConduitFrame, Fin<Seq<Mark>>> project) =>
        WithSprites(sheet => key.Catch(() => project(frame))
            .Bind(marks => Marks.Render(new Canvas.Pipeline(frame), sheet, marks, key).Map(static _ => unit)));

    private Fin<RealtimeLifecycle.Running> WithSprites(Func<SpriteSheet, Fin<Unit>> use) {
        lock (lifecycleGate) {
            return lifecycle.Switch(
                idle: _ => Fin.Fail<RealtimeLifecycle.Running>(key.MissingContext()),
                running: row => use(row.Sprites).Map(_ => row));
        }
    }

    private Unit CommitFramebuffer(Fin<RealtimeLifecycle.Running> initialized) {
        lock (lifecycleGate) {
            bool ready = initialized.Match(
                Succ: owned => ReferenceEquals(lifecycle, owned),
                Fail: static _ => false);
            return ignore(framebufferReady.Swap(_ => ready));
        }
    }

    private Fin<Unit> Release(RealtimeLifecycle.Running owned, Fin<Unit> primary) {
        owned.Pixels.Close();
        Fin<Unit> cleanup = key.Catch(() => { owned.Sprites.Dispose(); return Fin.Succ(unit); });
        return primary.Match(
            Succ: _ => cleanup,
            Fail: failure => cleanup.Match(
                Succ: _ => Fin.Fail<Unit>(failure),
                Fail: secondary => Fin.Fail<Unit>(failure + secondary)));
    }

    private Fin<T> Observe<T>(Fin<T> result) {
        _ = result.IfFail(failure => ignore(faults.Swap(rows => rows.Add(failure))));
        return result;
    }

    private T Observe<T>(Fin<T> result, T fallback) => result.Match(
        Succ: static value => value,
        Fail: failure => { _ = faults.Swap(rows => rows.Add(failure)); return fallback; });

    private static ConduitFrame Frame(DisplayPipeline pipeline) =>
        ConduitFrame.Of(pipeline, pipeline.Viewport, ConduitPhase.PostObjects);
}

public static class LightAuthorities {
    private static readonly Atom<HashMap<Guid, (Guid Token, LightAuthorityProgram Program)>> Programs =
        Atom(HashMap<Guid, (Guid Token, LightAuthorityProgram Program)>());
    private static readonly Atom<Seq<Error>> Failures = Atom(Seq<Error>());
    private static readonly Op Key = Op.Of(name: nameof(LightAuthorities));

    public static Seq<Error> Faults => Failures.Value;

    public static Fin<Unit> Register(Guid engine, LightAuthorityProgram program, PlugIn owner, Op? key = null) {
        Op op = key.OrDefault();
        Guid token = Guid.NewGuid();
        return from _ in guard(engine != Guid.Empty && program is not null && owner is not null, op.InvalidInput()).ToFin()
               from claimed in guard(
                   Programs.Swap(rows => rows.TryAdd(engine, (token, program)))
                       .Find(engine)
                       .Map(row => row.Token == token)
                       .IfNone(false),
                   op.InvalidContext()).ToFin()
               from registered in op.Catch(() => {
                   LightManagerSupport.RegisterLightManager(owner);
                   return Fin.Succ(value: unit);
               }).BindFail(failure => (
                   Programs.Swap(rows => rows.Find(engine)
                       .Filter(row => row.Token == token)
                       .Map(_ => rows.Remove(engine))
                       .IfNone(rows)),
                   Fin.Fail<Unit>(failure)).Item2)
               select unit;
    }

    public static Fin<Unit> Notify(DocumentSession session, LightAuthorityHost authority, LightChange change, Light light, Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(session).ToFin(Fail: op.MissingContext())
               from host in Optional(authority).ToFin(Fail: op.InvalidInput())
               from move in Optional(change).ToFin(Fail: op.InvalidInput())
               from carrier in Optional(light).ToFin(Fail: op.InvalidInput())
               from _ in source.Demand(
                   use: document => op.Catch(() => {
                       Light moved = carrier;
                       host.OnCustomLightEvent(document, move.Key, ref moved);
                       return Fin.Succ(value: unit);
                   }),
                   key: op,
                   needs: [SessionNeed.Read])
               select unit;
    }

    internal static TOut Answer<TOut>(Guid engine, RhinoDoc document, Func<LightAuthorityProgram, DocKey, Fin<TOut>> body, TOut fallback) =>
        (from program in Programs.Value.Find(engine).Map(static claim => claim.Program).ToFin(Fail: Key.MissingContext())
         from key in DocKey.Of(document, Key)
         from result in Key.Catch(() => body(program, key))
         select result).Match(
            Succ: static value => value,
            Fail: failure => (Failures.Swap(rows => rows.Add(failure)), fallback).Item2);
}

public abstract class LightAuthorityHost : LightManagerSupport {
    protected abstract Guid Plugin { get; }
    protected abstract Guid Engine { get; }

    public sealed override Guid PluginId() => Plugin;

    public sealed override Guid RenderEngineId() => Engine;

    public sealed override void GetLights(RhinoDoc doc, ref LightArray light_array) {
        LightArray target = light_array;                                     // Exemption: host ref/out fill members are the platform-forced statement seam
        _ = LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Roster(key).Map(rows => rows.Fold(unit, (_, row) => {
                target.Append(row);
                return unit;
            })),
            fallback: unit);
    }

    public sealed override bool LightFromId(RhinoDoc doc, Guid uuid, ref Light light) {
        Option<Light> found = LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Resolve(key, uuid),
            fallback: Option<Light>.None);
        if (found.Case is Light resolved) {
            light = resolved;
            return true;
        }
        return false;
    }

    public sealed override void ModifyLight(RhinoDoc doc, Light light) =>
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Amend(key, light), fallback: unit);

    public sealed override bool DeleteLight(RhinoDoc doc, Light light, bool bUndelete) =>
        LightAuthorities.Answer(Engine, doc, (program, key) => program.Retire(key, light, bUndelete), fallback: false);

    public sealed override int ObjectSerialNumberFromLight(RhinoDoc doc, ref Light light) {
        Light carrier = light;
        return LightAuthorities.Answer(Engine, doc, (program, key) => program.Serial(key, carrier), fallback: -1);
    }

    public sealed override string LightDescription(RhinoDoc doc, ref Light light) {
        Light carrier = light;
        return LightAuthorities.Answer(Engine, doc, (program, key) => program.Describe(key, carrier), fallback: string.Empty);
    }

    public sealed override bool OnEditLight(RhinoDoc doc, ref LightArray light_array) =>
        LightAuthorities.Answer(Engine, doc, (program, key) => program.Edit(key, Drained(light_array)), fallback: false);

    public sealed override void GroupLights(RhinoDoc doc, ref LightArray light_array) =>
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Group(key, Drained(light_array)), fallback: unit);

    public sealed override void UnGroup(RhinoDoc doc, ref LightArray light_array) =>
        _ = LightAuthorities.Answer(Engine, doc, (program, key) => program.Ungroup(key, Drained(light_array)), fallback: unit);

    public override bool SetLightSolo(RhinoDoc doc, Guid uuid_light, bool bSolo) =>
        LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Solo.Match(
                Some: solo => solo.Set(key, uuid_light, bSolo),
                None: () => Fin.Succ(base.SetLightSolo(doc, uuid_light, bSolo))),
            fallback: false);

    public override bool GetLightSolo(RhinoDoc doc, Guid uuid_light) =>
        LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Solo.Match(
                Some: solo => solo.Get(key, uuid_light),
                None: () => Fin.Succ(base.GetLightSolo(doc, uuid_light))),
            fallback: false);

    public override int LightsInSoloStorage(RhinoDoc doc) =>
        LightAuthorities.Answer(Engine, doc,
            (program, key) => program.Solo.Match(
                Some: solo => solo.Count(key),
                None: () => Fin.Succ(base.LightsInSoloStorage(doc))),
            fallback: 0);

    private static Seq<Light> Drained(LightArray rows) =>
        toSeq(Enumerable.Range(0, rows.Count()).Select(rows.ElementAt));
}
```

## [04]-[POST_AND_TEXTURE]

- Owner: `PostEffectOp` closes settings edits; `PostEffectGate` closes execution policy; `TextureBake` closes live evaluator and simulated-texture modes over `ContentRef` and object identity.
- Law: configuration and execution never merge — `PostEffectOp` writes the settings-side rows the pipeline reads at render time, `PostEffectGate` decides per-render execution on a window; the render-settings page's edit rail points here and carries no eighth sub-owner record.
- Law: a mutating op batch demands `SessionNeed.Mutate`, a census-only batch `SessionNeed.Read` — the batch's own case shapes derive the need set, never a caller flag; `PostEffectCollection` and each `PostEffectData` are disposable host natives scoped to the demand window, and only `EffectRoster` crosses out.
- Law: live-versus-baked is the union's discriminant, selected by the texture's own capability — a consumer asks for live first and falls to the baked case on refusal, and the fallback is a case transition, never a silent quality change.
- Boundary: `TextureBake.Evaluate` is internal, resolves `RenderTexture` and `RhinoObject` inside `DocumentSession.Demand`, and disposes evaluator or simulation before detached egress.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<int>]
public sealed partial class EffectStage {
    public static readonly EffectStage Early = new(key: 0, native: PostEffects.PostEffectType.Early);
    public static readonly EffectStage ToneMapping = new(key: 1, native: PostEffects.PostEffectType.ToneMapping);
    public static readonly EffectStage Late = new(key: 2, native: PostEffects.PostEffectType.Late);

    internal PostEffects.PostEffectType Native { get; }

    internal static Fin<EffectStage> Stage(PostEffects.PostEffectType native, Op key) =>
        toSeq(Items).Find(row => row.Native == native).ToFin(Fail: key.InvalidResult(detail: native.ToString()));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record PostEffectOp {
    private PostEffectOp() { }
    public sealed record CensusCase : PostEffectOp;
    public sealed record ToggleCase(EffectId Effect, bool On, bool Shown) : PostEffectOp;
    public sealed record ReorderCase(EffectId Move, EffectId Before) : PostEffectOp;
    public sealed record SelectCase(EffectStage Stage, EffectId Effect) : PostEffectOp;
    public sealed record TuneCase(EffectId Effect, string Parameter, object Value) : PostEffectOp;

    internal bool Valid => Switch(
        censusCase: static _ => true,
        toggleCase: static row => row.Effect.Value != Guid.Empty,
        reorderCase: static row => row.Move.Value != Guid.Empty && row.Before.Value != Guid.Empty && row.Move.Value != row.Before.Value,
        selectCase: static row => row.Stage is not null && row.Effect.Value != Guid.Empty,
        tuneCase: static row => row.Effect.Value != Guid.Empty && !string.IsNullOrWhiteSpace(row.Parameter) && row.Value is not null);

    internal Fin<Unit> Apply(PostEffects.PostEffectCollection collection, Op key) => Switch(
        state: (Collection: collection, Op: key),
        censusCase: static (_, _) => Fin.Succ(value: unit),
        toggleCase: static (ctx, op) => Data(collection: ctx.Collection, effect: op.Effect, key: ctx.Op).Bind(data => ctx.Op.Catch(() => {
            using PostEffects.PostEffectData owned = data;
            owned.On = op.On;
            owned.Shown = op.Shown;
        })),
        reorderCase: static (ctx, op) => ctx.Op.Catch(() =>
            ctx.Op.Confirm(success: ctx.Collection.MovePostEffectBefore(id_move: op.Move.Value, id_before: op.Before.Value))),
        selectCase: static (ctx, op) => ctx.Op.Catch(() =>
            ctx.Collection.SetSelectedPostEffect(type: op.Stage.Native, id: op.Effect.Value)),
        tuneCase: static (ctx, op) => Data(collection: ctx.Collection, effect: op.Effect, key: ctx.Op).Bind(data => ctx.Op.Catch(() => {
            using PostEffects.PostEffectData owned = data;
            return ctx.Op.Confirm(success: owned.SetParameter(param_name: op.Parameter, param_value: op.Value));
        })));

    private static Fin<PostEffects.PostEffectData> Data(PostEffects.PostEffectCollection collection, EffectId effect, Op key) =>
        key.Catch(() => Optional(collection.PostEffectDataFromId(id: effect.Value)).ToFin(Fail: key.InvalidInput()));
}

// --- [MODELS] -------------------------------------------------------------------------------
public readonly record struct PostEffectState(EffectId Id, EffectStage Stage, string Name, bool On, bool Shown);

public sealed record EffectRoster(Seq<PostEffectState> Rows, HashMap<EffectStage, EffectId> Selected) : IDetachedDocumentResult;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Effects {
    public static Fin<EffectRoster> Configure(DocumentSession session, Seq<PostEffectOp> ops, Op? key = null) {
        Op op = key.OrDefault();
        Seq<SessionNeed> needs = ops.Exists(static row => row is not PostEffectOp.CensusCase)
            ? [SessionNeed.Read, SessionNeed.Mutate]
            : [SessionNeed.Read];
        return guard(session is not null && ops.ForAll(static row => row is not null && row.Valid), op.InvalidInput()).ToFin().Bind(_ => session.Demand(
            use: document => op.Catch(() => {
                using PostEffects.PostEffectCollection collection = document.RenderSettings.PostEffects;
                return ops.TraverseM(row => row.Apply(collection: collection, key: op)).As()
                    .Bind(_ => Roster(collection: collection, op: op));
            }),
            key: op,
            needs: needs.ToArray()));
    }

    private static Fin<EffectRoster> Roster(PostEffects.PostEffectCollection collection, Op op) =>
        from rows in toSeq(collection).TraverseM(data => Detached(data: data, op: op)).As()
        from selected in op.Catch(() => Fin.Succ(value: toSeq(EffectStage.Items)
            .Choose(stage => collection.GetSelectedPostEffect(type: stage.Native, id: out Guid chosen)
                ? Some((stage, EffectId.Create(chosen)))
                : None)
            .ToHashMap()))
        select new EffectRoster(Rows: rows.Strict(), Selected: selected);

    private static Fin<PostEffectState> Detached(PostEffects.PostEffectData data, Op op) => op.Catch(() => {
        using PostEffects.PostEffectData owned = data;
        return EffectStage.Stage(native: owned.Type, key: op)
            .Map(stage => new PostEffectState(Id: EffectId.Create(owned.Id), Stage: stage, Name: owned.LocalName, On: owned.On, Shown: owned.Shown));
    });
}

[SmartEnum<int>]
public sealed partial class TextureEvaluation {
    public static readonly TextureEvaluation DisableFiltering = new(0, RenderTexture.TextureEvaluatorFlags.DisableFiltering);
    public static readonly TextureEvaluation DisableLocalMapping = new(1, RenderTexture.TextureEvaluatorFlags.DisableLocalMapping);
    public static readonly TextureEvaluation DisableAdjustment = new(2, RenderTexture.TextureEvaluatorFlags.DisableAdjustment);
    public static readonly TextureEvaluation DisableProjectionChange = new(3, RenderTexture.TextureEvaluatorFlags.DisableProjectionChange);

    internal RenderTexture.TextureEvaluatorFlags Native { get; }
}

[SmartEnum<int>]
public sealed partial class TextureGenerationUse {
    public static readonly TextureGenerationUse Allow = new(0, RenderTexture.TextureGeneration.Allow);
    public static readonly TextureGenerationUse Disallow = new(1, RenderTexture.TextureGeneration.Disallow);
    public static readonly TextureGenerationUse Skip = new(2, RenderTexture.TextureGeneration.Skip);

    internal RenderTexture.TextureGeneration Native { get; }
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record TextureBake {
    private TextureBake() { }
    public sealed record LiveCase(ContentRef Texture, Seq<TextureEvaluation> Policy) : TextureBake;
    public sealed record BakedCase(ContentRef Texture, TextureGenerationUse Generation, int Size, Guid Subject) : TextureBake;

    internal bool Valid => Switch(
        liveCase: static row => row.Texture is not null && row.Policy.ForAll(static policy => policy is not null),
        bakedCase: static row => row.Texture is not null
            && row.Generation is not null
            && row.Size > 0
            && row.Subject != Guid.Empty);

    internal Fin<TOut> Evaluate<TOut>(DocumentSession session, Func<TextureEvaluator, Fin<TOut>> live, Func<SimulatedTexture, Fin<TOut>> baked, Op? key = null)
        where TOut : IDetachedDocumentResult {
        Op op = key.OrDefault();
        return guard(session is not null && live is not null && baked is not null && Valid, op.InvalidInput()).ToFin().Bind(_ => session.Demand(
            use: document => Switch(
                state: (Document: document, Live: live, Baked: baked, Op: op),
                liveCase: static (ctx, bake) => from content in bake.Texture.Resolve(ctx.Document, ctx.Op)
                                                   from texture in content is RenderTexture value
                                                       ? Fin.Succ(value)
                                                       : Fin.Fail<RenderTexture>(ctx.Op.InvalidInput())
                                                   from result in ctx.Op.Catch(() => {
                                                       RenderTexture.TextureEvaluatorFlags flags = bake.Policy.Fold(
                                                           RenderTexture.TextureEvaluatorFlags.Normal,
                                                           static (value, policy) => value | policy.Native);
                                                       using TextureEvaluator evaluator = texture.CreateEvaluator(flags);
                                                       return Optional(evaluator).ToFin(ctx.Op.InvalidResult()).Bind(ctx.Live);
                                                   })
                                                   select result,
                bakedCase: static (ctx, bake) => from content in bake.Texture.Resolve(ctx.Document, ctx.Op)
                                                    from texture in content is RenderTexture value
                                                        ? Fin.Succ(value)
                                                        : Fin.Fail<RenderTexture>(ctx.Op.InvalidInput())
                                                    from subject in Optional(ctx.Document.Objects.FindId(bake.Subject)).ToFin(ctx.Op.InvalidInput())
                                                    from result in ctx.Op.Catch(() => {
                                                        SimulatedTexture simulated = null!;
                                                        texture.SimulateTexture(ref simulated, bake.Generation.Native, bake.Size, subject);
                                                        using SimulatedTexture? owned = simulated;
                                                        return Optional(owned).ToFin(ctx.Op.InvalidResult()).Bind(ctx.Baked);
                                                    })
                                                    select result),
            key: op,
            needs: [SessionNeed.Read]));
    }
}

// --- [SERVICES] -----------------------------------------------------------------------------
internal sealed class PostEffectGate : PostEffects.PostEffectExecutionControl {
    private readonly Func<EffectId, Fin<bool>> decide;
    private readonly Action<Error> reject;
    private readonly Op key;

    private PostEffectGate(Func<EffectId, Fin<bool>> decide, Action<Error> reject, Op key) =>
        (this.decide, this.reject, this.key) = (decide, reject, key);

    internal static Fin<Unit> Register(RenderWindow window, Func<EffectId, Fin<bool>> decide, Action<Error> reject, Op? key = null) =>
        key.OrDefault().Catch(() => {
            window.RegisterPostEffectExecutionControl(ec: new PostEffectGate(decide, reject, key.OrDefault()));
            return Fin.Succ(value: unit);
        });

    public override bool ReadyToExecutePostEffect(Guid postEffectId) => key.Catch(() => decide(EffectId.Create(postEffectId))).Match(
        Succ: static value => value,
        Fail: failure => { reject(failure); return false; });
}
```

## [05]-[CHANGEQUEUE]

- Owner: `SceneDelta` closes the scene-change family as detached value cases; `SceneBatch` is the idempotent custody capsule carrying its deltas and monotonic seal stamp; `SceneQueue` adapts the host `ChangeQueue` and is the sole payload source.
- Entry: `SceneQueue.Of(QueueSource, PlugIn, QueuePolicy, Context, TimeProvider, Op?)` opens public document sources or internal preview sources; `Drive` folds world build, flush, one-shot, and material refresh; `Pull` answers census reads as detached pulses; `Drain` transfers whole batches under kernel `Env` cancellation.
- Law: geometry identity composes the kernel reconciliation chain — `MeshSpace.Of` admits the duplicated patch, `EncodeForm.Of` canonicalizes, `Reconciliation.Apply` digests, and `GeometryHash` is minted only through that chain, never a second hash; GPU residency rides `Encode.Apply` into `EncodedGeometry` when the policy carries a `PackPolicy` row.
- Law: duplicated geometry enters owned custody before admission, reconciliation, or residency work; any downstream refusal releases the duplicate, while a successful patch transfers the lease into the batch.
- Law: hooks mint detached deltas inside the host grant and seal them at `NotifyEndUpdates` into one bounded channel write — the host callback never runs a consumer continuation inline, the reader is the only egress, and a refused or evicted batch releases its leases and lands as a typed `QueueLoss` receipt.
- Law: each host change list converts atomically — a failed member records its fault, releases every detached predecessor in reverse custody, and stages no partial delta.
- Law: staging and closure are one atomic transition on the pending cell — a closed cell refuses the delta, which releases immediately, closure drains accepted deltas into a `QueueLoss` receipt, and every public operation refuses a released queue with context evidence.
- Law: `SceneMarks.Render` projects flushed geometry and completes `Marks.Render` while `SceneBatch` custody remains held on the realtime engine's conduit frame — never a private draw path; a redraw request rides `RedrawTarget`, and batch versus realtime stays split: `SceneQueue` feeds a `RealtimeProgram`, never a `RenderJob`.
- Law: `SceneBatch.Use` excludes release across patch projection and drawing; `Drain` transfers the whole idempotent capsule, and its consumer releases only after the borrowed render rail settles.
- Law: a consumer's heavy per-patch application — re-encode, LOD, analysis — rides the kernel `Operation.Apply` prepare-gated fold under the same `Env`; the drain itself only walks the reader and hands leases across.
- Boundary: content resolves as identity, never as graph — a material touch carries its CRC and original-instance ids for the Render content rail; block ancestry and decals resolve through the Blocks and content owners; the `DisplayRenderSettings` payload stays unprojected while its host getters remain unresolved, folding to the stateless `SettingsCase`.
- Boundary: custom render meshes enter the live scene through the Objects authoring owner — a `Rhino.Render.CustomRenderMeshes.RenderMeshProvider` registered through `RegisterProvider` under this page's viewport and pipeline-attributes context — and reach this queue as ordinary mesh deltas; the provider adapter itself stays the authoring page's owner.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<uint>]
public sealed partial class BakeAxis {
    public static readonly BakeAxis Decals = new(1u);
    public static readonly BakeAxis ProceduralTextures = new(2u);
    public static readonly BakeAxis CustomObjectMappings = new(4u);
    public static readonly BakeAxis WcsMappings = new(8u);
    public static readonly BakeAxis MultipleMappingChannels = new(0x10u);
    public static readonly BakeAxis NoRepeatTextures = new(0x20u);

    internal static Cq.ChangeQueue.BakingFunctions Fold(FrozenSet<BakeAxis> axes) =>
        toSeq(axes).Fold(
            Cq.ChangeQueue.BakingFunctions.None,
            static (mask, axis) => mask | (Cq.ChangeQueue.BakingFunctions)axis.Key);
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
    public sealed record WorldCase(bool FlushWhenReady) : QueueDrive;
    public sealed record FlushCase : QueueDrive;
    public sealed record OneShotCase : QueueDrive;
    public sealed record MaterialsCase : QueueDrive;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScenePull {
    private ScenePull() { }
    public sealed record ViewCase : ScenePull;
    public sealed record BoundsCase : ScenePull;
    public sealed record SunCase : ScenePull;
    public sealed record SkylightCase : ScenePull;
    public sealed record GroundCase : ScenePull;
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ScenePulse {
    private ScenePulse() { }
    public sealed record ViewPulse(Guid View) : ScenePulse;
    public sealed record BoundsPulse(BoundingBox Bounds) : ScenePulse;
    public sealed record SunPulse(Lease<Light> Sun) : ScenePulse;
    public sealed record SkylightPulse(SkyDelta State) : ScenePulse;
    public sealed record GroundPulse(GroundDelta State) : ScenePulse;
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record QueuePolicy {
    private QueuePolicy(int capacity, FrozenSet<BakeAxis> baking, Option<PackPolicy> residency, bool notifyChanges, bool respectDisplayAttributes) =>
        (Capacity, Baking, Residency, NotifyChanges, RespectDisplayAttributes) =
        (capacity, baking, residency, notifyChanges, respectDisplayAttributes);

    public int Capacity { get; }
    public FrozenSet<BakeAxis> Baking { get; }
    public Option<PackPolicy> Residency { get; }
    public bool NotifyChanges { get; }
    public bool RespectDisplayAttributes { get; }

    public static Fin<QueuePolicy> Of(
        int capacity,
        FrozenSet<BakeAxis> baking,
        Option<PackPolicy> residency = default,
        bool notifyChanges = true,
        bool respectDisplayAttributes = false,
        Op? key = null) =>
        from admitted in key.OrDefault().Positive(capacity)
        select new QueuePolicy(admitted, baking, residency, notifyChanges, respectDisplayAttributes);
}

public readonly record struct MappingSlot(int Channel, Transform Local);

public sealed record MeshPatch(GeometryHash Content, Lease<Mesh> Geometry, Option<EncodedGeometry> Residency);

public sealed record MeshDelta(Guid Id, Transform Ocs, Seq<MappingSlot> Mappings, Seq<MeshPatch> Patches);

public readonly record struct InstanceMotion(uint Instance, Transform Motion);

public sealed record InstanceDelta(uint Instance, Guid Root, Guid Parent, Guid Mesh, MaterialTouch Material, Transform Placement);

public sealed record MaterialTouch(uint Material, uint MeshInstance, Seq<Guid> Origins);

public sealed record LightDelta(Guid Id, uint Crc, LightMotion Change, Lease<Light> Data);

public readonly record struct SkyDelta(bool Enabled, bool CustomEnvironment, double ShadowIntensity);

public sealed record GroundDelta(
    bool Enabled,
    bool ShadowOnly,
    bool Underside,
    double Altitude,
    uint Material,
    Vector2d TextureScale,
    Vector2d TextureOffset,
    double TextureRotation);

public sealed record ClipDelta(Guid Id, Plane Plane, bool Enabled, Seq<Guid> Views);

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
    public sealed record SettingsCase : SceneDelta;
    public sealed record EnvironmentCase(Seq<EnvironmentRole> Roles) : SceneDelta;
    public sealed record SkylightCase(SkyDelta State) : SceneDelta;
    public sealed record GroundCase(GroundDelta State) : SceneDelta;
    public sealed record ClipCase(Seq<Guid> Removed, Seq<ClipDelta> Upserted) : SceneDelta;
    public sealed record WorkflowCase : SceneDelta;
    public sealed record AttributesCase : SceneDelta;

    internal Unit Release() => Switch(
        viewCase: static _ => unit,
        geometryCase: static row => ignore(row.Added.Iter(static delta => delta.Patches.Iter(static patch => ignore(patch.Geometry.Dispose())))),
        instanceCase: static _ => unit,
        motionCase: static _ => unit,
        lightCase: static row => ignore(row.Lights.Iter(static delta => ignore(delta.Data.Dispose()))),
        dynamicLightCase: static row => ignore(row.Lights.Iter(static lease => ignore(lease.Dispose()))),
        sunCase: static row => ignore(row.Sun.Dispose()),
        materialCase: static _ => unit,
        settingsCase: static _ => unit,
        environmentCase: static _ => unit,
        skylightCase: static _ => unit,
        groundCase: static _ => unit,
        clipCase: static _ => unit,
        workflowCase: static _ => unit,
        attributesCase: static _ => unit);
}

public sealed class SceneBatch : IDisposable {
    private readonly Seq<SceneDelta> deltas;
    private readonly Lock lifecycle = new();
    private int released;

    internal SceneBatch(Seq<SceneDelta> deltas, Option<MonotonicStamp> sealedAt) =>
        (this.deltas, Sealed) = (deltas, sealedAt);

    public Option<MonotonicStamp> Sealed { get; }
    internal int Count => deltas.Count;

    internal Fin<TResult> Use<TResult>(Func<Seq<SceneDelta>, Fin<TResult>> use, Op key) {
        lock (lifecycle) {
            return guard(released == 0, key.InvalidContext()).ToFin()
                .Bind(_ => guard(use is not null, key.InvalidInput()).ToFin())
                .Bind(_ => key.Catch(() => use(deltas)));
        }
    }

    public Unit Release() {
        lock (lifecycle) {
            if (released != 0) { return unit; }
            released = 1;
            return ignore(deltas.Iter(static delta => ignore(delta.Release())));
        }
    }

    public void Dispose() => _ = Release();
}

public readonly record struct QueueLoss(Option<MonotonicStamp> At, int Deltas);

// --- [SERVICES] -----------------------------------------------------------------------------
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
    private readonly Atom<Option<Seq<SceneDelta>>> pending = Atom(Some(Seq<SceneDelta>()));
    private readonly Atom<Seq<QueueLoss>> losses = Atom(Seq<QueueLoss>());
    private readonly Atom<Seq<Error>> faults = Atom(Seq<Error>());
    private readonly QueuePolicy policy;
    private readonly MonotonicTimeline timeline;
    private readonly Context context;
    private readonly Op key;
    private int released;

    private SceneQueue(Guid plugin, uint document, ViewInfo view, QueuePolicy policy, MonotonicTimeline timeline, Context context, Op key)
        : base(plugin, document, view, null, bRespectDisplayPipelineAttributes: policy.RespectDisplayAttributes, bNotifyChanges: policy.NotifyChanges) =>
        (this.policy, this.timeline, this.context, this.key, lane) = (policy, timeline, context, key, Open(policy, timeline, losses, key));

    private SceneQueue(Guid plugin, CreatePreviewEventArgs preview, QueuePolicy policy, MonotonicTimeline timeline, Context context, Op key)
        : base(plugin, preview) =>
        (this.policy, this.timeline, this.context, this.key, lane) = (policy, timeline, context, key, Open(policy, timeline, losses, key));

    public ChannelReader<SceneBatch> Deltas => lane.Reader;

    public Seq<Error> Faults => faults.Value;

    public Seq<QueueLoss> Losses => losses.Value;

    public static Fin<SceneQueue> Of(
        QueueSource source,
        PlugIn owner,
        QueuePolicy policy,
        Context context,
        TimeProvider clock,
        Op? key = null) {
        Op op = key.OrDefault();
        return from shape in Optional(source).ToFin(Fail: op.InvalidInput())
               from plugin in Optional(owner).ToFin(Fail: op.InvalidInput())
               from plan in Optional(policy).ToFin(Fail: op.InvalidInput())
               from ambient in Optional(context).ToFin(Fail: op.InvalidInput())
               from ticks in Optional(clock).ToFin(Fail: op.InvalidInput())
               from timeline in MonotonicTimeline.Of(provider: ticks, key: op)
               from queue in shape.Switch(
                   (Plugin: plugin, Plan: plan, Timeline: timeline, Context: ambient, Op: op),
                   liveCase: static (held, row) =>
                       from lease in ViewportLease.Of(session: row.Session, target: row.Target, key: held.Op)
                       from opened in lease.Use(borrow: seat => seat.Info(view => held.Op.Catch(() => Fin.Succ(new SceneQueue(
                           plugin: held.Plugin.Id,
                           document: row.Session.Key,
                           view: view,
                           policy: held.Plan,
                           timeline: held.Timeline,
                           context: held.Context,
                           key: held.Op))), held.Op), key: held.Op)
                       select opened,
                   previewCase: static (held, row) => held.Op.Catch(() => Fin.Succ(new SceneQueue(
                       plugin: held.Plugin.Id,
                       preview: row.Args,
                       policy: held.Plan,
                       timeline: held.Timeline,
                       context: held.Context,
                       key: held.Op))))
               select queue;
    }

    public Fin<Unit> Drive(QueueDrive drive, Op? key = null) {
        ArgumentNullException.ThrowIfNull(drive);
        Op op = key.OrDefault();
        return from _ in guard(flag: Volatile.Read(location: ref released) is 0, False: op.InvalidContext()).ToFin()
               from done in drive.Switch(
                   (Queue: this, Op: op),
                   worldCase: static (held, row) => held.Op.Catch(() => {
                       held.Queue.CreateWorld(bFlushWhenReady: row.FlushWhenReady);
                       return Fin.Succ(value: unit);
                   }),
                   flushCase: static (held, _) => held.Op.Catch(() => {
                       held.Queue.Flush();
                       return Fin.Succ(value: unit);
                   }),
                   oneShotCase: static (held, _) => held.Op.Catch(() => {
                       held.Queue.OneShot();
                       return Fin.Succ(value: unit);
                   }),
                   materialsCase: static (held, _) => held.Op.Catch(() => {
                       held.Queue.RefreshMaterials();
                       return Fin.Succ(value: unit);
                   }))
               select done;
    }

    public Fin<ScenePulse> Pull(ScenePull pull, Op? key = null) {
        ArgumentNullException.ThrowIfNull(pull);
        Op op = key.OrDefault();
        return guard(Volatile.Read(location: ref released) is 0, op.InvalidContext()).ToFin().Bind(_ => pull.Switch(
            (Queue: this, Op: op),
            viewCase: static (held, _) => Fin.Succ<ScenePulse>(value: new ScenePulse.ViewPulse(View: held.Queue.ViewId)),
            boundsCase: static (held, _) => held.Op.Catch(() =>
                Fin.Succ<ScenePulse>(value: new ScenePulse.BoundsPulse(Bounds: held.Queue.GetQueueSceneBoundingBox()))),
            sunCase: static (held, _) => held.Op.Catch(() =>
                Optional(held.Queue.GetQueueSun()).ToFin(Fail: held.Op.MissingContext())
                    .Map(static sun => (ScenePulse)new ScenePulse.SunPulse(Sun: new Lease<Light>.Owned(Value: (Light)sun.Duplicate())))),
            skylightCase: static (held, _) => held.Op.Catch(() =>
                Optional(held.Queue.GetQueueSkylight()).ToFin(Fail: held.Op.MissingContext())
                    .Map(static sky => (ScenePulse)new ScenePulse.SkylightPulse(State: Detach(sky)))),
            groundCase: static (held, _) => held.Op.Catch(() =>
                Optional(held.Queue.GetQueueGroundPlane()).ToFin(Fail: held.Op.MissingContext())
                    .Map(static ground => (ScenePulse)new ScenePulse.GroundPulse(State: Detach(ground))))));
    }

    public Fin<int> Drain(Func<SceneBatch, Fin<Unit>> take, Option<Env> env = default, Op? key = null) {
        ArgumentNullException.ThrowIfNull(take);
        Op op = key.OrDefault();
        return guard(Volatile.Read(location: ref released) is 0, op.InvalidContext()).ToFin().Bind(_ => op.Catch(() => {
            int applied = 0;
            while (lane.Reader.TryRead(out SceneBatch? batch)) {                       // Exemption: channel drain walks the reader's own terminal grammar
                if (env.Map(static held => held.Cancellation.IsCancellationRequested).IfNone(false)) {
                    _ = batch.Release();
                    return Fin.Fail<int>(error: new UiFault.Cancelled(Key: op));
                }
                Fin<Unit> outcome = op.Catch(() => take(batch));
                if (outcome.IsFail) {
                    _ = batch.Release();
                    return outcome.Map(_ => applied);
                }
                applied += batch.Count;
            }
            return Fin.Succ(value: applied);
        }));
    }

    public Fin<Unit> Close(Op? key = null) {
        Op op = key.OrDefault();
        if (Interlocked.Exchange(location1: ref released, value: 1) is not 0) return Fin.Succ(value: unit);
        Seq<SceneDelta> stranded = default;
        _ = pending.Swap(state => (state.Iter(rows => stranded = rows), Option<Seq<SceneDelta>>.None).Item2);
        _ = stranded.Iter(static delta => ignore(delta.Release()));
        _ = Op.SideWhen(!stranded.IsEmpty, () => ignore(losses.Swap(rows =>
            rows.Add(new QueueLoss(At: timeline.Capture(key: op).ToOption(), Deltas: stranded.Count)))));
        _ = lane.Writer.TryComplete();
        while (lane.Reader.TryRead(out SceneBatch? residue)) {                          // Exemption: forced residue sweep — every undelivered batch releases and receipts
            _ = residue.Release();
            _ = losses.Swap(rows => rows.Add(new QueueLoss(At: timeline.Capture(key: op).ToOption(), Deltas: residue.Count)));
        }
        return op.Catch(() => {
            Dispose();
            return Fin.Succ(value: unit);
        });
    }

    protected override void ApplyViewChange(ViewInfo viewInfo) => Stage(delta: new SceneDelta.ViewCase(View: ViewId));

    protected override void ApplyMeshChanges(Guid[] deleted, List<Cq.Mesh> added) =>
        StageBatch(
            source: toSeq(added),
            detach: Detach,
            release: static delta => ignore(delta.Patches.Iter(static patch => ignore(patch.Geometry.Dispose()))),
            project: rows => new SceneDelta.GeometryCase(Removed: toSeq(deleted).Strict(), Added: rows));

    protected override void ApplyMeshInstanceChanges(List<uint> deleted, List<Cq.MeshInstance> addedOrChanged) =>
        StageBatch(
            source: toSeq(addedOrChanged),
            detach: payload => key.Catch(() => Fin.Succ(new InstanceDelta(
                Instance: payload.InstanceId,
                Root: payload.RootId,
                Parent: payload.ParentId,
                Mesh: payload.MeshId,
                Material: new MaterialTouch(
                    Material: payload.MaterialId,
                    MeshInstance: payload.InstanceId,
                    Origins: toSeq(OriginalInstanceIdsFromMaterialId(payload.MaterialId)).Strict()),
                Placement: payload.Transform))),
            release: static _ => unit,
            project: rows => new SceneDelta.InstanceCase(Removed: toSeq(deleted).Strict(), Upserted: rows));

    protected override void ApplyDynamicObjectTransforms(List<Cq.DynamicObjectTransform> dynamicObjectTransforms) =>
        Stage(delta: new SceneDelta.MotionCase(Moves: toSeq(dynamicObjectTransforms)
            .Map(static payload => new InstanceMotion(Instance: payload.MeshInstanceId, Motion: payload.Transform)).Strict()));

    protected override void ApplyLightChanges(List<Cq.Light> lightChanges) =>
        StageBatch(
            source: toSeq(lightChanges),
            detach: payload =>
                from change in LightMotion.TryGet(payload.ChangeType, out LightMotion? motion) && motion is { } admitted
                    ? Fin.Succ(admitted)
                    : Fin.Fail<LightMotion>(error: key.InvalidResult())
                from data in key.Catch(() => Fin.Succ(new Lease<Light>.Owned(Value: (Light)payload.Data.Duplicate())))
                select new LightDelta(Id: payload.Id, Crc: payload.IdCrc, Change: change, Data: data),
            release: static delta => delta.Data.Dispose(),
            project: static rows => new SceneDelta.LightCase(Lights: rows));

    protected override void ApplyDynamicLightChanges(List<Light> dynamicLightChanges) =>
        StageBatch(
            source: toSeq(dynamicLightChanges),
            detach: payload => key.Catch(() => Fin.Succ<Lease<Light>>(new Lease<Light>.Owned(Value: (Light)payload.Duplicate()))),
            release: static lease => lease.Dispose(),
            project: static rows => new SceneDelta.DynamicLightCase(Lights: rows));

    protected override void ApplySunChanges(Light sun) =>
        ignore(Observe(key.Catch(() => Fin.Succ(Stage(delta: new SceneDelta.SunCase(
            Sun: new Lease<Light>.Owned(Value: (Light)sun.Duplicate())))))));

    protected override void ApplyMaterialChanges(List<Cq.Material> mats) =>
        Stage(delta: new SceneDelta.MaterialCase(Touches: toSeq(mats).Map(payload => new MaterialTouch(
            Material: payload.Id,
            MeshInstance: payload.MeshInstanceId,
            Origins: toSeq(OriginalInstanceIdsFromMaterialId(payload.Id)).Strict())).Strict()));

    protected override void ApplyRenderSettingsChanges(RenderSettings rs) => Stage(delta: new SceneDelta.SettingsCase());

    protected override void ApplyRenderSettingsChanges(Cq.DisplayRenderSettings settings) => Stage(delta: new SceneDelta.SettingsCase());

    protected override void ApplyEnvironmentChanges(RenderEnvironment.Usage usage) =>
        ignore(Observe(usage != RenderEnvironment.Usage.None && (usage | EnvironmentMask) == EnvironmentMask
                ? Fin.Succ(EnvironmentPolicy.Choose(row => (usage & row.Bit) != 0 ? Some(row.Role) : None))
                : Fin.Fail<Seq<EnvironmentRole>>(error: key.InvalidResult(detail: usage.ToString())))
            .Iter(roles => Stage(delta: new SceneDelta.EnvironmentCase(Roles: roles))));

    protected override void ApplySkylightChanges(Cq.Skylight skylight) =>
        Stage(delta: new SceneDelta.SkylightCase(State: Detach(skylight)));

    protected override void ApplyGroundPlaneChanges(Cq.GroundPlane gp) =>
        Stage(delta: new SceneDelta.GroundCase(State: Detach(gp)));

    protected override void ApplyLinearWorkflowChanges(LinearWorkflow lw) => Stage(delta: new SceneDelta.WorkflowCase());

    protected override void ApplyClippingPlaneChanges(Guid[] deleted, List<Cq.ClippingPlane> addedOrModified) =>
        Stage(delta: new SceneDelta.ClipCase(
            Removed: toSeq(deleted).Strict(),
            Upserted: toSeq(addedOrModified).Map(static payload => new ClipDelta(
                Id: payload.Id,
                Plane: payload.Plane,
                Enabled: payload.IsEnabled,
                Views: toSeq(payload.ViewIds).Strict())).Strict()));

    protected override void ApplyDynamicClippingPlaneChanges(List<Cq.ClippingPlane> changed) =>
        Stage(delta: new SceneDelta.ClipCase(
            Removed: Seq<Guid>(),
            Upserted: toSeq(changed).Map(static payload => new ClipDelta(
                Id: payload.Id,
                Plane: payload.Plane,
                Enabled: payload.IsEnabled,
                Views: toSeq(payload.ViewIds).Strict())).Strict()));

    protected override void ApplyDisplayPipelineAttributesChanges(DisplayPipelineAttributes displayPipelineAttributes) =>
        Stage(delta: new SceneDelta.AttributesCase());

    protected override void NotifyEndUpdates() {
        base.NotifyEndUpdates();
        Seq<SceneDelta> taken = default;
        _ = pending.Swap(state => state.Map(rows => (taken = rows, Seq<SceneDelta>()).Item2));
        if (taken.IsEmpty) return;
        SceneBatch batch = new(deltas: taken, sealedAt: timeline.Capture(key: key).ToOption());
        _ = Op.SideWhen(!lane.Writer.TryWrite(batch), () => {
            _ = batch.Release();
            _ = losses.Swap(rows => rows.Add(new QueueLoss(At: batch.Sealed, Deltas: batch.Count)));
        });
    }

    protected override bool ProvideOriginalObject() => false;

    protected override Cq.ChangeQueue.BakingFunctions BakeFor() => BakeAxis.Fold(axes: policy.Baking);

    private static Channel<SceneBatch> Open(QueuePolicy policy, MonotonicTimeline timeline, Atom<Seq<QueueLoss>> losses, Op key) =>
        Channel.CreateBounded<SceneBatch>(
            new BoundedChannelOptions(policy.Capacity) {
                FullMode = BoundedChannelFullMode.DropOldest,
                SingleReader = true,
                AllowSynchronousContinuations = false,
            },
            dropped => {
                _ = dropped.Release();
                _ = losses.Swap(rows => rows.Add(new QueueLoss(At: timeline.Capture(key: key).ToOption(), Deltas: dropped.Count)));
            });

    private static SkyDelta Detach(Cq.Skylight payload) =>
        new(Enabled: payload.Enabled, CustomEnvironment: payload.UsesCustomEnvironment, ShadowIntensity: payload.ShadowIntensity);

    private static GroundDelta Detach(Cq.GroundPlane payload) => new(
        Enabled: payload.Enabled,
        ShadowOnly: payload.IsShadowOnly,
        Underside: payload.ShowUnderside,
        Altitude: payload.Altitude,
        Material: payload.MaterialId,
        TextureScale: payload.TextureScale,
        TextureOffset: payload.TextureOffset,
        TextureRotation: payload.TextureRotation);

    private Fin<MeshDelta> Detach(Cq.Mesh payload) =>
        from patches in DetachAll(
            source: toSeq(payload.GetMeshes()),
            detach: Patch,
            release: static patch => patch.Geometry.Dispose())
        from delta in key.Catch(() => Fin.Succ(new MeshDelta(
            Id: payload.Id(),
            Ocs: payload.OcsTransform,
            Mappings: toSeq(payload.Mappings).Map(static channel => new MappingSlot(Channel: channel.Channel, Local: channel.Local)).Strict(),
            Patches: patches.Strict())))
        select delta;

    private Fin<MeshPatch> Patch(Mesh native) =>
        from geometry in key.Catch(() => Optional(native.Duplicate() as Mesh).ToFin(key.InvalidResult())
            .Map(static duplicate => (Lease<Mesh>)new Lease<Mesh>.Owned(Value: duplicate)))
        from patch in (from space in MeshSpace.Of(native: geometry.Resource, context: context, key: key)
                       from digest in Reconciliation.Apply(new ReconcileOp.Encode(EncodeForm.Of(space)), key).Bind(answer => answer.Switch(
                           digest: static row => Fin.Succ(row.Value),
                           reconciled: _ => Fin.Fail<GeometryHash>(error: key.InvalidResult()),
                           topology: _ => Fin.Fail<GeometryHash>(error: key.InvalidResult())))
                       from residency in policy.Residency.Match(
                           Some: pack => Encode.Apply(new PackOp.MeshPatch(Source: space, Policy: pack), key).Map(Some),
                           None: () => Fin.Succ(value: Option<EncodedGeometry>.None))
                       select new MeshPatch(Content: digest, Geometry: geometry, Residency: residency))
            .BindFail(failure => (geometry.Dispose(), Fin.Fail<MeshPatch>(failure)).Item2)
        select patch;

    private void StageBatch<TIn, TOut>(
        Seq<TIn> source,
        Func<TIn, Fin<TOut>> detach,
        Func<TOut, Unit> release,
        Func<Seq<TOut>, SceneDelta> project) {
        _ = Observe(DetachAll(source, detach, release)).Iter(rows => Stage(project(rows.Strict())));
    }

    private Fin<Seq<TOut>> DetachAll<TIn, TOut>(
        Seq<TIn> source,
        Func<TIn, Fin<TOut>> detach,
        Func<TOut, Unit> release) =>
        source.Fold(
            Fin.Succ(Seq<TOut>()),
            (accepted, input) => accepted.Bind(done => key.Catch(() => detach(input))
                .Map(done.Add)
                .BindFail(failure => (
                    toSeq(done.AsEnumerable().Reverse()).Iter(release),
                    Fin.Fail<Seq<TOut>>(failure)).Item2)));

    private void Stage(SceneDelta delta) {
        bool staged = false;
        _ = pending.Swap(state => state.Match(
            Some: rows => (staged = true, Some(rows.Add(delta))).Item2,
            None: () => (staged = false, Option<Seq<SceneDelta>>.None).Item2));
        _ = Op.SideWhen(!staged, () => ignore(delta.Release()));
    }

    private Option<T> Observe<T>(Fin<T> outcome) => outcome.Match(
        Succ: Some,
        Fail: failure => (faults.Swap(rows => rows.Add(failure)), Option<T>.None).Item2);
}

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SceneMarks {
    public static Fin<int> Render(
        SceneBatch batch,
        Canvas canvas,
        SpriteSheet sprites,
        Func<MeshPatch, DisplayMaterial> material,
        Op? key = null) {
        Op op = key.OrDefault();
        return from source in Optional(batch).ToFin(op.InvalidInput())
               from project in Optional(material).ToFin(op.InvalidInput())
               from rendered in source.Use(
                   deltas => Marks.Render(canvas, sprites, Project(deltas, project), op),
                   op)
               select rendered;
    }

    private static Seq<Mark> Project(Seq<SceneDelta> deltas, Func<MeshPatch, DisplayMaterial> material) => deltas
        .Bind(static delta => delta is SceneDelta.GeometryCase geometry ? geometry.Added : Seq<MeshDelta>())
        .Bind(static delta => delta.Patches)
        .Map(patch => (Mark)new Mark.World(Value: new WorldMark.MeshShaded(Value: patch.Geometry.Resource, Material: material(patch))))
        .Strict();
}
```
