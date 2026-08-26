# [APPUI_DIAGNOSTICS_DEVLOOP]

Rasm.AppUi dev loop is the Debug-profile working surface: hot-reload knob rows and the manual-reload intent edge, the ProDiagnostics visual-tree/property/event/layout inspector under one attach-config row, the user-facing performance HUD sample feed, the flamegraph fold, the solve time-travel scrub, cross-machine replay-verify, the in-app REPL, and remote event ingestion, then gives each instrument its face: a zoomable lane-grouped flame view, a scrub folded by the one transport grammar, a typed block stream over the virtualization fabric, and HUD chip rows on the chrome family. Every surface projects settled measures and events through the existing diagnostics owners.

## [01]-[INDEX]

- [02]-[DEV_LOOP]: Hot-reload knob rows; the scheduled dispatcher starvation probe; HUD channel with its overlay vocabulary, flamegraph with the host profile-sample join, scrub, REPL, ingest; the collab pre-commit tap and JSON op-window export.
- [03]-[INSPECTOR]: ProDiagnostics attach-config row; the screenshot and property-edit handler bodies over the one host-edge collapse; live property commits; control-snapshot lane.
- [04]-[LOOP_SURFACES]: The flame view with lane grouping, hover, and zoom-to-span; the scrub bound to the transport grammar; the typed block stream over the virtualization fabric; the diagnostics HUD chip roster.

## [02]-[DEV_LOOP]

- Owner: `DevLoopFault` — the direct generated `[Union]` with one `[FaultCase]` leaf per development-loop failure; `HostSink` — the ONE host-edge result collapse parking every refusal on the composition-minted kernel `FaultCell`; `DevLoop` — the loop verbs; `HudSample`, `HudOverlay`, `OverdrawRamp`, `FlameNode`, `ProfileSampleSource`, `PreCommitFact`, `SolveScrub`, `Repl` the user-facing debug owners.
- Entry: `DispatcherLag` probes the UI boundary on a declared `Schedule` cadence — a starvation probe that fired once and never again measures nothing — marking and reading elapsed through the kernel `MonotonicTimeline` and refusing a starved marshal as `DevLoopFault.DispatcherTimeout`; `Hud` conflates the sample feed through a bounded latest-wins `Channel<HudSample>` and leases the overlay's native filter for the subscription's lifetime; `Reload` routes the three injected hot-reload effects; `Ingest` admits a canonical `RasmEvent<Extensions>` and appends it unchanged; `FlameNode.Of` folds every matching AppHost `ProfileSample` into the frame tree; `CollabPreCommit` binds the sync-owner pre-commit tap onto the AppUi fact stream and `CollabJson` names the readable op-window export.
- Auto: the lag sink fires `AppUiFact.DispatcherLag` at `AppUiPoint.DispatcherLag`; the remote decode column binds the kernel CloudEvent admission at composition, so a companion node's events join the same stream; `Reload` binds the three injected operations at composition under the master gate, so the manual-reload intent is a command-table verb on Debug profiles and a structurally-absent route on Release closures; the HUD render delegate binds its own `SurfaceScheduler.Marshal` at composition, so thread affinity stays with the marshal owner and the pump stays one bounded reader.
- Packages: HotAvalonia, Avalonia.Markup.Xaml.Loader (transitive floor, Debug pin), SkiaSharp, System.Reactive, LoroCs (companion, `VersionVector` in the JSON-export delegate signature only), Rasm.AppHost (project, boundary types), Rasm (kernel `FaultBand`/`FaultCell`/`MonotonicTimeline`), LanguageExt.Core, NodaTime, BCL inbox (`System.Threading.Channels`)
- Growth: one knob row retunes the reload gate, one `ReloadIntent` case absorbs a new manual-reload verb, one `HudSample` field absorbs a new HUD metric, one `HudOverlay` case absorbs a new diagnostic overlay, a new eval outcome extends `DeckOutcome`, a new host-profile sample is one AppHost `ProfileSample` value under the profile subtree, and a new collab forensics verb is one member reading the sync owner; zero new surface.
- Law: HotAvalonia is a Debug-gated build asset whose injected `UseHotReload`/`EnableHotReload`/`DisableHotReload`/`TriggerHotReload` extensions are the only callable surface — the Release strip rides `HotAvaloniaExcludeReferences` (default `HotAvalonia`, `HotAvalonia.Core`, `HotAvalonia.Fody`, plus `Avalonia.Markup.Xaml.Loader` when `HotAvaloniaIncludeXamlLoader` is false), `HotAvaloniaProcessReferences` (default false) governs only whether referenced PROJECTS join the weave scope, and the markup loader is the weaver's Debug-only re-patch dependency — a DevLoop-raised runtime `AvaloniaRuntimeXamlLoader` inflation is the rejected form whose structural fault is `Surfaces.RejectRuntimeInflation`.
- Law: the HUD is the `HudSample` feed — every column is already sealed evidence, so re-sealing the sample would re-bill the same GPU duration at the usage fold; the overlay is the `HudOverlay` vocabulary on the same subscription, its `OverdrawRamp` bands `Theme/tokens.md` rows, and the filter mints ONCE per subscription and dies with it.
- Law: flamegraphs are the `FlameNode` fold over settled frame, solve, pass, and profile durations; `ProfileSampleSource` reads the AppHost-owned `UiSchedulerPort.ProfileSamples` registration row, AppUi filters by correlation and prefix-merges published samples into a `cpu-profile` subtree lane-grouped by emitting thread, and the symbolization posture reaches the reader on the lane name.
- Law: cross-machine replay-verify rides the one `ProofEngine.Replay` route and the one `ProofEngine.Divergent` index walk `Diagnostics/proof.md` owns; the REPL evaluates through the one `CommandProjections.Invoke` route and returns the same `DeckOutcome` as every invocation route.
- Law: remote event ingestion uses the kernel CloudEvent admission through a composition-bound decode delegate; `Ingest` appends the admitted event unchanged, and a decode refusal is terminal.
- Boundary: the collab forensics verbs read the settled sync owner — `CollabPreCommit` binds the composition-supplied `CollabWire.TapPreCommit` installer and fires each `PreCommitFact` as `AppUiFact.PreCommit`, leaving the pending commit's `ChangeModifier` untouched, while `CollabJson` names the readable op-window export the REPL and support bundle consume through the composition-supplied `CollabWire.ExportJson` delegate; the HARFS remote-server knobs and the runtime timeout and hotkey knobs ride the same MSBuild gate as the master row and carry no managed surface.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record DevLoopFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.DevLoop;
    private DevLoopFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => Detail;
    [FaultCase(0)]
    public sealed partial record FrameAbsent(string Detail)       : DevLoopFault(Detail);
    [FaultCase(1)]
    public sealed partial record DispatcherTimeout(string Detail, Error Cause) : DevLoopFault(Detail), ICausedFault;
    [FaultCase(2)]
    public sealed partial record Stream(string Detail)            : DevLoopFault(Detail);
}

// --- [MODELS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReloadIntent {
    private ReloadIntent() { }
    public sealed record Trigger : ReloadIntent;
    public sealed record Enable : ReloadIntent;
    public sealed record Disable : ReloadIntent;
}

public readonly record struct HudSample(
    Duration FrameElapsed,
    Duration GpuElapsed,
    long VramBytes,
    long Triangles,
    Duration SolveElapsed,
    int PerNodeCount) {
    public static HudSample Of(FrameRender frame, GpuTimeline gpu, long vramBytes, Seq<Duration> solveNodes) =>
        new(frame.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed), gpu.MeasuredGpu, vramBytes, frame.Triangles,
            solveNodes.Fold(Duration.Zero, static (total, elapsed) => total + elapsed), solveNodes.Count);
}

[ComplexValueObject]
public sealed partial class OverdrawRamp {
    public SKColor Clear { get; }
    public SKColor Once { get; }
    public SKColor Twice { get; }
    public SKColor Thrice { get; }
    public SKColor Quadruple { get; }
    public SKColor Saturated { get; }

    public SKColorFilter Filter() => SKColorFilter.CreateOverdraw([Clear, Once, Twice, Thrice, Quadruple, Saturated]);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HudOverlay {
    private HudOverlay() { }
    public sealed record Plain : HudOverlay;
    public sealed record Overdraw(OverdrawRamp Ramp) : HudOverlay;

    public Option<SKColorFilter> Filter() => Switch(
        plain: static _ => Option<SKColorFilter>.None,
        overdraw: static heat => Some(heat.Ramp.Filter()));
}

public delegate Seq<ProfileSample> ProfileSampleSource(CorrelationId correlation);

public readonly record struct PreCommitFact(string DocumentKey, ulong Lamport, Option<string> Message, ulong Ops, string Origin);

public sealed record FlameNode(string Frame, Duration Self, Seq<FlameNode> Children) {
    public Duration Total => Self + Children.Fold(Duration.Zero, static (acc, child) => acc + child.Total);

    public Seq<(string Frame, Duration Total, int Depth)> Flatten(int depth = 0) =>
        Seq((Frame, Total, depth)) + Children.Bind(child => child.Flatten(depth + 1));

    public static FlameNode Of(
        CorrelationId correlation,
        HudSample hud,
        Seq<(string Node, Duration Elapsed)> solves,
        Seq<PassTiming> passes,
        ProfileSampleSource profiles) =>
        new("frame", Duration.Zero,
            Seq(
                new FlameNode("cpu", hud.FrameElapsed > hud.SolveElapsed ? hud.FrameElapsed - hud.SolveElapsed : Duration.Zero,
                    solves.Map(static row => new FlameNode(row.Node, row.Elapsed, Seq<FlameNode>()))),
                new FlameNode("gpu", Duration.Zero, passes.Map(static row => new FlameNode(row.Pass, row.Resolved, Seq<FlameNode>()))))
            + (profiles(correlation) switch {
                { Count: 0 } => Seq<FlameNode>(),
                var samples => Seq(FromSamples("cpu-profile", samples)),
            }));

    static FlameNode FromSamples(string root, Seq<ProfileSample> samples) =>
        new(root, Duration.Zero,
            samples.Fold(Map<string, FlameTrie>(), static (lanes, sample) =>
                lanes.AddOrUpdate(Lane(sample),
                    lane => lane.Graft(toSeq(sample.Frames), Duration.FromMilliseconds(sample.WeightMillis)),
                    () => FlameTrie.Empty.Graft(toSeq(sample.Frames), Duration.FromMilliseconds(sample.WeightMillis))))
                .AsIterable().Map(static pair => pair.Value.Node(pair.Key)).ToSeq().Strict());

    static string Lane(ProfileSample sample) =>
        string.Create(CultureInfo.InvariantCulture, $"thread {sample.ThreadId} ({sample.Form.Key})");
}

internal readonly record struct FlameTrie(Duration Self, Map<string, FlameTrie> Children) {
    internal static readonly FlameTrie Empty = new(Duration.Zero, Map<string, FlameTrie>());

    internal FlameTrie Graft(Seq<string> frames, Duration self) =>
        frames.Head.Match(
            None: () => this with { Self = Self + self },
            Some: head => this with {
                Children = Children.AddOrUpdate(head,
                    child => child.Graft(frames.Tail, self),
                    () => Empty.Graft(frames.Tail, self)),
            });

    internal FlameNode Node(string frame) =>
        new(frame, Self, Children.AsIterable().Map(static pair => pair.Value.Node(pair.Key)).ToSeq().Strict());
}

public sealed record SolveFrame(int Ordinal, string NodeId, JsonElement State, Instant At);
public sealed record SolveDelta(string NodeId, JsonElement From, JsonElement To);

public sealed record SolveScrub(Seq<SolveFrame> Frames) {
    public Option<SolveFrame> At(int ordinal) =>
        ordinal >= 0 && ordinal < Frames.Count ? Some(Frames[ordinal]) : None;

    public SolveScrub Record(string nodeId, JsonElement state, Instant at) =>
        this with { Frames = Frames.Add(new SolveFrame(Frames.Count, nodeId, state, at)) };

    public Option<(SolveFrame From, SolveFrame To)> Window(int from, int to) =>
        (At(from), At(to)).Apply(static (a, b) => (a, b)).As();

    public IO<Unit> Restore(int ordinal, Func<SolveFrame, IO<Unit>> apply) =>
        At(ordinal).Match(
            Some: apply,
            None: () => IO.fail<Unit>(new DevLoopFault.FrameAbsent($"solve frame {ordinal} is absent")));

    public Option<SolveDelta> Diff(int from, int to) =>
        Window(from, to).Bind(static pair => JsonElement.DeepEquals(pair.From.State, pair.To.State)
            ? None
            : Some(new SolveDelta(pair.To.NodeId, pair.From.State, pair.To.State)));
}

public sealed record Repl(CommandDeck Deck, Func<string, Fin<(string Key, Rasm.Contracts.Ui.CommandPayloadWire Payload)>> Parse) {
    public IO<DeckOutcome> Eval(string line) =>
        Parse(line).Match(
            Succ: parsed => Deck.Invoke(parsed.Key, parsed.Payload, CallerModality.Operator),
            Fail: static error => IO.fail<DeckOutcome>(error));
}

// --- [SERVICES] ------------------------------------------------------------------------
public sealed record HostSink(FaultCell Faults, HookId Point) {
    public Unit Collapse(IO<Unit> body) =>
        ignore((body | @catch<IO, Unit>(static _ => true, error => IO.lift(() => ignore(Faults.Park(Point, error))))).As().Run());

    public Task CollapseAsync(IO<Unit> body) =>
        (body | @catch<IO, Unit>(static _ => true, error => IO.lift(() => ignore(Faults.Park(Point, error))))).As().RunAsync().AsTask();
}

public static class DevLoopPoints {
    public static readonly HookId Hud = HookId.Create(value: "rasm.appui.devloop.hud");
    public static readonly HookId Inspector = HookId.Create(value: "rasm.appui.devloop.inspector");
    public static readonly HookId Collab = HookId.Create(value: "rasm.appui.devloop.collab");
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class DevLoop {
    static readonly Op DispatcherOp = Op.Of(name: "appui.devloop.dispatcher-lag");

    public static IO<Unit> DispatcherLag(
        SurfaceScheduler boundary,
        MonotonicTimeline line,
        Duration timeout,
        Schedule cadence,
        CancellationToken cancellation,
        Func<Duration, IO<Unit>> sink) =>
        (from state in IO.lift(() => (
                Start: line.Capture(),
                Gate: new TaskCompletionSource<Fin<TimeSpan>>(TaskCreationOptions.RunContinuationsAsynchronously)))
         from _queued in boundary.Marshal(() => state.Gate.TrySetResult(
             state.Start.Bind(start => line.Capture().Bind(end => line.Elapsed(start, end)))))
         from elapsed in IO.liftVAsync(() => DispatcherOp.Catch(
                 async token => await state.Gate.Task
                     .WaitAsync(timeout.ToTimeSpan(), token).ConfigureAwait(false),
                 provider: error => error.HasException<TimeoutException>()
                     ? Some(new DevLoopFault.DispatcherTimeout(
                         $"dispatcher marshal exceeded {timeout}", error))
                     : None,
                 token: cancellation))
             .Bind(static settled => IO.lift(settled))
         from _sunk in sink(Duration.FromTimeSpan(elapsed))
         select unit)
        .RepeatUntil(cadence, _ => cancellation.IsCancellationRequested);

    public static IO<IDisposable> Hud(
        IObservable<HudSample> samples,
        HudOverlay overlay,
        Func<HudSample, Option<SKColorFilter>, IO<Unit>> render,
        HostSink sink,
        CancellationToken cancellation) =>
        IO.lift(() => {
            Option<SKColorFilter> filter = overlay.Filter();
            Channel<HudSample> feed = Channel.CreateBounded<HudSample>(
                new BoundedChannelOptions(1) { FullMode = BoundedChannelFullMode.DropOldest, SingleReader = true });
            IDisposable subscription = samples.Subscribe(
                sample => ignore(feed.Writer.TryWrite(sample)),
                error => ignore(sink.Collapse(IO.fail<Unit>(error))),
                () => ignore(feed.Writer.TryComplete()));
            Task pump = Task.Run(async () => {
                await foreach (HudSample sample in feed.Reader.ReadAllAsync(cancellation).ConfigureAwait(false)) {
                    await sink.CollapseAsync(render(sample, filter)).ConfigureAwait(false);
                }
            }, cancellation);
            ignore(pump.ContinueWith(
                _ => filter.Iter(static lease => lease.Dispose()),
                TaskScheduler.Default));
            return (IDisposable)Disposable.Create(() => {
                subscription.Dispose();
                ignore(feed.Writer.TryComplete());
            });
        });

    public static IO<Seq<int>> ReplayVerify(
        Seq<(string Key, Rasm.Contracts.Ui.CommandPayloadWire Payload, CallerModality Caller)> journal,
        CommandDeck deck,
        Func<IO<Unit>> restore,
        Seq<string> baseline) =>
        ProofEngine.Replay(deck, journal, restore)
            .Map(replayed => ProofEngine.Divergent(replayed.Map(static outcome => outcome.PayloadDigest), baseline));

    public static IO<Unit> Reload(ReloadIntent intent, Func<IO<Unit>> trigger, Func<IO<Unit>> enable, Func<IO<Unit>> disable) =>
        intent.Switch(
            state: (Trigger: trigger, Enable: enable, Disable: disable),
            trigger: static (ops, _) => ops.Trigger(),
            enable: static (ops, _) => ops.Enable(),
            disable: static (ops, _) => ops.Disable());

    public static Fin<Unit> Ingest(
        Func<ReadOnlyMemory<byte>, Fin<RasmEvent<Rasm.Contracts.Event.Extensions>>> decode,
        Func<RasmEvent<Rasm.Contracts.Event.Extensions>, Fin<Unit>> append,
        ReadOnlyMemory<byte> frame) =>
        decode(frame).Bind(append);

    public static IO<IDisposable> CollabPreCommit(
        Func<Func<PreCommitFact, IO<Unit>>, IDisposable> install,
        HookSet<AppUiPoint, AppUiFact, TelemetrySource> hooks) =>
        IO.lift(() => install(
            fact => IO.lift(() => hooks.Fire(
                AppUiPoint.PreCommit,
                new AppUiFact.PreCommit(fact.DocumentKey, fact.Lamport, fact.Ops, fact.Origin, fact.Message),
                Op.Of(name: "appui.devloop.pre-commit")))
                .Bind(static settled => IO.lift(settled))
                .Map(static _ => unit)));

    public static Fin<string> CollabJson(Func<VersionVector, VersionVector, Fin<string>> export, VersionVector from, VersionVector to) =>
        export(from, to);
}
```

| [INDEX] | [KNOB_ROW]                    | [VALUE]                    | [ROLE]                                  |
| :-----: | :---------------------------- | :------------------------- | :-------------------------------------- |
|  [01]   | HotAvalonia                   | Debug default              | master gate                             |
|  [02]   | HotAvaloniaIncludeExtensions  | exe default                | injects the UseHotReload source         |
|  [03]   | HotAvaloniaExcludeReferences  | HotAvalonia + Core + Fody  | Release closure strip list              |
|  [04]   | HotAvaloniaIncludeXamlLoader  | false adds loader to strip | markup-loader strip membership          |
|  [05]   | HotAvaloniaProcessReferences  | false default              | referenced-project weave scope          |
|  [06]   | HotAvaloniaAutoEnable         | build default              | reload enablement at boot               |
|  [07]   | HotAvaloniaRecompileResources | build default              | resource recompilation on reload        |
|  [08]   | markup-loader pin             | transitive floor           | `Avalonia.Markup.Xaml.Loader` Debug pin |
|  [09]   | HotAvaloniaRemote             | non-desktop opt-in         | remote reload route                     |
|  [10]   | HotAvaloniaTimeout            | runtime default            | reload timeout window                   |
|  [11]   | HotAvaloniaHotkey             | runtime default            | manual-reload key chord                 |
|  [12]   | HarfsAddress / HarfsPort      | remote endpoint            | HARFS file-server endpoint              |

## [03]-[INSPECTOR]

- Owner: `InspectorAttach` — the one ProDiagnostics attach row carrying the native `DevToolsOptions` policy object; `InspectorCapture` — the `IScreenshotHandler` body routing every inspector snapshot into the one capture encode fold; `InspectorEdits` — the `IDevToolsPropertyEditHandler` body sealing every live property commit onto the evidence stream; the package option surface remains the complete configuration owner.
- Entry: `Attach(Application app, InspectorAttach row)` — one Debug-composition call; the attach is the only imperative edge, and the two handler bodies are composition-bound values the row carries into `DevToolsOptions`.
- Auto: `DevToolsOptions` carries the default `F12` gesture, `LaunchView`, `HotKeys`, `ScreenshotHandler`, and `PropertyEditHandler` — one config row, every knob a field; `PropertyValueEditorService` owns live property commits so an edit lands through the service, never an ad-hoc reflection write; `VisualExtensions.RenderTo(Control, Stream, double)` is the control-snapshot lane `InspectorCapture` composes — its stream feeds the same capture encode fold `proof.md` owns, so an inspector screenshot is a `CaptureRow` sibling, never a second pixel path; `OnPropertyEdited(DevToolsPropertyEdit)` hands `InspectorEdits` the full edit record to the composition-bound settlement function.
- Packages: ProDiagnostics (Debug-gated, `PrivateAssets="all"`), Avalonia, Rasm (kernel `FaultCell`), LanguageExt.Core
- Growth: a new inspector knob is one `DevToolsOptions` field on the row; a new snapshot destination is one delivery delegate the capture row already carries; zero new surface.
- Law: both handler adapters collapse a typed result at a host signature that carries none — `Take` returns a bare `Task` and `OnPropertyEdited` returns `void` — so each rides the ONE `HostSink` collapse, parking its refusal on the kernel `FaultCell` BEFORE the host value returns; a handler mapping its failure to a swallowed exception is the deleted form.
- Law: a property commit fires `AppUiFact.Edit` at `AppUiPoint.Edit`, so inspector mutations and deck-routed edits join the same timeline.
- Boundary: ProDiagnostics is Debug-gated `PrivateAssets="all"` beside HotAvalonia and absent from the Release surface — a Release-profile attach is structurally unrepresentable; `Conventions.DefaultScreenshotHandler` is `internal`, so the package's file-picker default is unreachable by name and `InspectorCapture` is the only handler this folder can bind; the `ProDataGrid`/`ProCharts` siblings are NOT admitted; both first-party alternates failed the admission gate (`Avalonia.Diagnostics` feed-dead with no Avalonia-12 asset; the Accelerate DevTools pay-tiered, license-gate rejected) — the record stands, never re-proposed.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public sealed record InspectorAttach(DevToolsOptions Options);

public sealed record InspectorCapture(
    double Dpi,
    Func<string, ReadOnlyMemory<byte>, IO<Unit>> Deliver,
    HostSink Sink) : IScreenshotHandler {

    public Task Take(Control control) =>
        Sink.CollapseAsync(Snapshot(control, Dpi).Bind(bytes => Deliver(control.GetType().Name, bytes)));

    static IO<ReadOnlyMemory<byte>> Snapshot(Control control, double dpi) =>
        IO.lift(() => {
            using MemoryStream sink = new();
            control.RenderTo(sink, dpi);
            return (ReadOnlyMemory<byte>)sink.ToArray();
        });
}

public sealed record InspectorEdits(
    Func<DevToolsPropertyEdit, IO<Unit>> Settle,
    HostSink Faults) : IDevToolsPropertyEditHandler {

    public void OnPropertyEdited(DevToolsPropertyEdit edit) =>
        ignore(Faults.Collapse(Settle(edit)));
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class Inspector {
    public static IO<Unit> Attach(Application app, InspectorAttach row) =>
        IO.lift(() => ignore(app.AttachDevTools(row.Options)));
}
```

## [04]-[LOOP_SURFACES]

- Owner: `FlameSpan` with `FlameView` — the laid-out flame projection carrying lane grouping, the zoom-to-span re-root, and the hover hit; `ScrubTransport` — the solve scrub bound to the ONE transport grammar; `ReplBlock` `[Union]` with `BlockStream` — the typed, height-indexed, filterable block stream the REPL and the log share; `HudFact` — the ONE chip roster whose rows carry key, chrome rank, AND read column; `DiagnosticsChrome` — the chip rows projected off that roster.
- Cases: `ReplBlock` = Command | Log | Timeline — a typed line with its result-carried outcome, a captured line burst, and a correlated evidence timeline, every case carrying the ordinal, the instant, and the one `Query` column the filter reads.
- Entry: `Spans` and `Hit(double fraction, int depth)` on `FlameView`; `ScrubTransport.Of(SolveScrub)` and `Raise(TransportVerb)`; `Index(double row)` on `BlockStream`; `DiagnosticsChrome.Rows()` and `Facts(HudSample, GovernorReadout)`.
- Auto: the flame layout is the `Charts/custom#SKIA_KINDS` `CustomVisuals.WedgeSpans` parent-share nesting — the ONE fold the sunburst ring, the flame row, and this hit-test all read; zoom-to-span RE-ROOTS to the focused node while a stale focus path widens back to the node it reached; the lane a span carries is its depth-one ancestor's frame, so a profile subtree's thread lanes group naturally; the scrub folds the settled `Render/animation#TIMELINE_EDITOR` `TransportVerb` roster over the shared `TransportState`, so no verb is spelled here; the block stream declares each case's extent off the arity it already holds, so the height index is a running prefix and a filtered stream re-indexes without realizing a block; per-block copy and bookmark are command-table intent keys; the HUD chips are ordinary `Shell/navigation#SHELL_CHROME` `ChromeRow` rows on `ChromeSlot.Hud`, with display precedence carried explicitly by each `HudFact` row and one corner constant for the block.
- Outcome: the surfaces project facts already held by their owners.
- Packages: Avalonia, DynamicData, System.Reactive, SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new flame lane is one grouping level in the tree the fold already walks; a new transport verb is one `TransportVerb` row consumed here with zero edit; a new block family is one `ReplBlock` case carrying its own extent and render arms; a new HUD readout is one `HudFact` row — key, chrome rank, read column, and chip arrive together; zero new surface.
- Law: these are PROJECTIONS and mint no instrument, no clock, and no second command path; the flame's DRAW rides the one custom-visual plane, while zoom, lane, and hover stay HERE because they are questions about the frame tree rather than about pixels; the REPL and the log are ONE stream — an operator's line and the rich view of what it answered belong under one another — and the stream binds the `Shell/virtualization#WINDOW_OWNER` fabric through `VirtualWindowSpec.Measured` with each case's declared extent as its seed.
- Law: the filter reads each block's own `Query` column so no case is invisible to a search it should answer; bookmarks live on the STREAM keyed by ordinal, because a block is an immutable record and a bookmark is a reader's annotation over it.
- Boundary: the roster IS both chip tables — a `Facts` list beside a `Chips` list was two authorities for one roster, and the row's read column is the derivation that deletes the mirror.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
public readonly record struct FlameSpan(string Frame, string Lane, int Depth, double Start, double Width, Duration Self, Duration Total);

public readonly record struct FlameRow(string Frame, string Lane, Duration Self, Duration Total, int Depth, int Parent);

// --- [TABLES] --------------------------------------------------------------------------
[SmartEnum<string>]
public sealed partial class HudFact {
    public static readonly HudFact Frame = new("devloop.frame", 10, static (hud, _) => hud.FrameElapsed.ToString());
    public static readonly HudFact Gpu = new("devloop.gpu", 20, static (hud, _) => hud.GpuElapsed.ToString());
    public static readonly HudFact Vram = new("devloop.vram", 30, static (hud, _) => hud.VramBytes.ToString(CultureInfo.InvariantCulture));
    public static readonly HudFact Triangles = new("devloop.triangles", 40, static (hud, _) => hud.Triangles.ToString(CultureInfo.InvariantCulture));
    public static readonly HudFact Solve = new("devloop.solve", 50, static (hud, _) => hud.SolveElapsed.ToString());
    public static readonly HudFact Tier = new(GovernorReadout.TierFact, 60, static (_, quality) => quality.Tier.Key);
    public static readonly HudFact Breach = new(GovernorReadout.BreachFact, 70, static (_, quality) =>
        quality.Breach.Map(static axis => axis.Key)
            .IfNone(() => quality.Tightest.Map(static axis => axis.Key).IfNone("-")));
    public static readonly HudFact Headroom = new(GovernorReadout.HeadroomFact, 80, static (_, quality) =>
        quality.Headroom.Map(static room => room.ToString("P0", CultureInfo.InvariantCulture)).IfNone("-"));
    public static readonly HudFact History = new(GovernorReadout.HistoryFact, 90, static (_, quality) =>
        quality.Recent.Count.ToString(CultureInfo.InvariantCulture));

    public int ChromeRank { get; }
    [UseDelegateFromConstructor] public partial string Read(HudSample hud, GovernorReadout quality);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record FlameView(FlameNode Root, Seq<string> Focus) {
    public static FlameView Of(FlameNode root) => new(root, Seq<string>());

    public FlameView Zoom(Seq<string> path) => this with { Focus = path };

    public FlameView Reset() => this with { Focus = Seq<string>() };

    public FlameNode Focused =>
        Focus.Fold(Root, static (node, frame) => node.Children.Find(child => child.Frame == frame).IfNone(node));

    public Seq<FlameRow> Rows => Grow(Focused, Focused.Frame, depth: 0, parent: -1, Seq<FlameRow>());

    static Seq<FlameRow> Grow(FlameNode node, string lane, int depth, int parent, Seq<FlameRow> held) =>
        node.Total <= Duration.Zero
            ? held
            : held.Add(new FlameRow(node.Frame, lane, node.Self, node.Total, depth, parent)) switch {
                var seeded => node.Children.Fold(seeded, (rows, child) =>
                    Grow(child, depth == 0 ? child.Frame : lane, depth + 1, seeded.Count - 1, rows)),
            };

    public VisualPayload.Wedge Payload =>
        new(Rows.Map(static row => (row.Frame, row.Total.ToTimeSpan().TotalNanoseconds, row.Depth, row.Parent)));

    public Seq<FlameSpan> Spans =>
        Rows switch {
            var rows => CustomVisuals.WedgeSpans(rows.Map(static row => (row.Frame, row.Total.ToTimeSpan().TotalNanoseconds, row.Depth, row.Parent)))
                .Map(span => rows[span.Index] switch {
                    var row => new FlameSpan(row.Frame, row.Lane, span.Depth, span.Start, span.Span, row.Self, row.Total),
                })
                .Strict(),
        };

    public int Depth => Rows.Fold(0, static (deepest, row) => Math.Max(deepest, row.Depth));

    public Seq<string> Lanes => Rows.Map(static row => row.Lane).Distinct().Strict();

    public Option<FlameSpan> Hit(double fraction, int depth) =>
        Spans.Find(span => span.Depth == depth && fraction >= span.Start && fraction < span.Start + span.Width);
}

public sealed record ScrubTransport(SolveScrub Scrub, TransportState Transport) {
    public static Fin<ScrubTransport> Of(SolveScrub scrub) =>
        scrub.Frames.IsEmpty
            ? Fin.Fail<ScrubTransport>(new DevLoopFault.FrameAbsent("the solve journal recorded no frame"))
            : Fin.Succ(new ScrubTransport(scrub, TransportState.Of(
                Playhead.At(1d, Duration.FromSeconds(scrub.Frames.Count - 1), PlaybackMode.Once))));

    public ScrubTransport Raise(TransportVerb verb) => this with { Transport = verb.Fold(Transport, Transport.Head) };

    public ScrubTransport Tick() => this with { Transport = Transport.Advanced() };

    public Option<SolveFrame> Current => Scrub.At(checked((int)Transport.Head.Index));

    public IO<Unit> Restore(Func<SolveFrame, IO<Unit>> apply) => Scrub.Restore(checked((int)Transport.Head.Index), apply);

    public Option<SolveDelta> Delta => Scrub.Diff(checked((int)Transport.Head.First), checked((int)Transport.Head.Index));
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplBlock(long Ordinal, Instant At, string Query) {
    public sealed record Command(long Ordinal, Instant At, string Line, Fin<DeckOutcome> Outcome) : ReplBlock(Ordinal, At, Line);
    public sealed record Log(long Ordinal, Instant At, string Boundary, Seq<string> Lines) : ReplBlock(Ordinal, At, string.Join('\n', Lines));
    public sealed record Timeline(long Ordinal, Instant At, EvidenceTimeline Value) : ReplBlock(Ordinal, At, Value.Correlation.ToString());

    public double Extent(double height) => Switch(
        state:    height,
        command:  static (h, _) => h * 2d,
        log:      static (h, block) => h * Math.Max(1, block.Lines.Count),
        timeline: static (h, block) => h * (block.Value.Rows.Count + 1));

    public ControlIntent Render(double height) => Switch(
        state:   height,
        command: static (_, block) => Panel(block.Ordinal, Seq(
            Label(block.Ordinal, "line", block.Line, PaintRole.Text),
            block.Outcome.Match(
                Succ: outcome => Label(block.Ordinal, "outcome", outcome.Outcome.Kind, PaintRole.TextMuted),
                Fail: fault => Label(block.Ordinal, "fault", fault.Message, PaintRole.ErrorText)))),
        log:     static (_, block) => Panel(block.Ordinal,
            block.Lines.Map((line, index) => Label(block.Ordinal, index.ToString(CultureInfo.InvariantCulture), line, PaintRole.TextMuted)).ToSeq()),
        timeline: static (h, block) => new ControlIntent.Grid(
            $"{DevLoopSurfaces.BlockKey}.{block.Ordinal}",
            EvidenceReport.Blocks(block.Value)
                .Choose(static entry => entry is ReportBlock.Table table ? table.Rows.Head : None)
                .Head
                .Map(static header => header.Map(static column => new ColumnRow(
                    HeaderKey: column,
                    Cell: new ControlIntent.Label($"cell.{column}", column, TypographyRole.Numeric, IntentBinding.Of(PaintRole.Text) with { ValueKey = Some(column) }),
                    Editor: None,
                    Extent: DataGridLength.Auto,
                    SortKey: Some(column),
                    Align: HorizontalAlignment.Left)))
                .IfNone(Seq<ColumnRow>()),
            VirtualWindowSpec.FixedRow(h * (block.Value.Rows.Count + 1)),
            IntentBinding.Of(PaintRole.Surface)));

    static ControlIntent Label(long ordinal, string slot, string text, PaintRole role) =>
        new ControlIntent.Label($"{DevLoopSurfaces.BlockKey}.{ordinal}.{slot}",
            text, TypographyRole.Code, IntentBinding.Of(role));

    static ControlIntent Panel(long ordinal, Seq<ControlIntent> labels) =>
        new ControlIntent.Panel($"{DevLoopSurfaces.BlockKey}.{ordinal}", labels,
            DevLoopSurfaces.BlockProgram, IntentBinding.Of(PaintRole.Surface));
}

public sealed record BlockStream(Seq<ReplBlock> Blocks, Set<long> Bookmarks, Option<string> Filter) {
    public static BlockStream Open() => new(Seq<ReplBlock>(), Set<long>(), None);

    public BlockStream Append(ReplBlock block) => this with { Blocks = Blocks.Add(block) };

    public BlockStream Bookmark(long ordinal) =>
        this with { Bookmarks = Bookmarks.Contains(ordinal) ? Bookmarks.Remove(ordinal) : Bookmarks.Add(ordinal) };

    public BlockStream Narrow(Option<string> query) => this with { Filter = query };

    public Seq<ReplBlock> Visible =>
        Filter.Match(
            Some: query => Blocks.Filter(block =>
                Bookmarks.Contains(block.Ordinal) || block.Query.Contains(query, StringComparison.OrdinalIgnoreCase)),
            None: () => Blocks);

    public Seq<(ReplBlock Block, double Offset, double Extent)> Index(double height) =>
        Visible.Fold((Rows: Seq<(ReplBlock, double, double)>(), Offset: 0d), (state, block) =>
            block.Extent(height) switch {
                var extent => (state.Rows.Add((block, state.Offset, extent)), state.Offset + extent),
            }).Rows;

    public double Height(double height) => Index(height).Fold(0d, static (sum, entry) => sum + entry.Extent);

    public Option<ReplBlock> At(double height, double offset) =>
        Index(height).Find(entry => offset >= entry.Offset && offset < entry.Offset + entry.Extent)
            .Map(static entry => entry.Block);

    public static VirtualWindowSpec Window(double viewportExtent, double height) =>
        VirtualWindowSpec.Measured(viewportExtent, height);
}

// --- [COMPOSITION] ---------------------------------------------------------------------
public static class DevLoopSurfaces {
    public const string BlockKey = "devloop.block";
    public const string BlockProgram = "devloop.block.stack";

    public const string CopyVerb = "devloop.block.copy";
    public const string BookmarkVerb = "devloop.block.bookmark";
    public const string FilterVerb = "devloop.block.filter";
    public const string FlameZoomVerb = "devloop.flame.zoom";
    public const string FlameResetVerb = "devloop.flame.reset";
}

public static class DiagnosticsChrome {
    static readonly CornerPosition Corner = CornerPosition.TopRight;

    public static Seq<ChromeRow> Rows() =>
        toSeq(HudFact.Items).Map(fact => new ChromeRow(
            IntentKey: fact.Key,
            Slot: ChromeSlot.Hud,
            Path: DevLoopSurfaces.BlockKey,
            Rank: fact.ChromeRank,
            Visible: static _ => true,
            Content: new ChromeContent.Chip(Corner, fact.Key))).ToSeq().Strict();

    public static Seq<(string Fact, string Value)> Facts(HudSample hud, GovernorReadout quality) =>
        toSeq(HudFact.Items).Map(fact => (fact.Key, fact.Read(hud, quality))).Strict();
}
```

## [05]-[RESEARCH]

(none)
