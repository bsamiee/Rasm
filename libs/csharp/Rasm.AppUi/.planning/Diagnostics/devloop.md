# [APPUI_DIAGNOSTICS_DEVLOOP]

Rasm.AppUi dev loop is the Debug-profile working surface: hot-reload knob rows and the manual-reload intent edge, the ProDiagnostics visual-tree/property/event/layout inspector under one attach-config row, the user-facing performance HUD sample feed, the flamegraph fold, the solve time-travel scrub, cross-machine replay-verify, the in-app REPL, and remote evidence ingestion, then gives each instrument its face: a zoomable lane-grouped flame view, a scrub folded by the one transport grammar, a typed block stream over the virtualization fabric, and HUD chip rows on the chrome family. Every measure reads the settled receipt envelopes the `Diagnostics/evidence.md` timeline ingests and every surface is a projection over those measures — the loop mints no second meter, no second codec, no second command-execution path, and no second presentation stack.

## [01]-[INDEX]

- [02]-[DEV_LOOP]: Hot-reload knob rows; dispatcher starvation probe; HUD with its overlay vocabulary, flamegraph with the host profile-sample join, scrub, REPL, ingest; the collab pre-commit tap and JSON op-window export; wire-context-preserving ingest.
- [03]-[INSPECTOR]: ProDiagnostics attach-config row; the screenshot and property-edit handler bodies; live property commits; control-snapshot lane.
- [04]-[LOOP_SURFACES]: The flame view with lane grouping, hover, and zoom-to-span; the scrub bound to the transport grammar; the typed block stream over the virtualization fabric; the diagnostics HUD chip rows.

## [02]-[DEV_LOOP]

- Owner: `DevLoop` — the Debug loop surface with the hot-reload knob rows, the manual-reload intent edge, the remote-evidence ingest edge, the performance-HUD sample feed, the flamegraph fold with the host profile-sample join, the solve time-travel scrub, the cross-machine replay-verify, the in-app REPL, and the collab pre-commit tap and JSON op-window export; `HudSample`, `HudOverlay`, `OverdrawRamp`, `FlameNode`, `ProfileSampleSource`, `PreCommitFact`, `SolveScrub`, `Repl` the user-facing debug owners.
- Entry: `DispatcherLag` carries an admitted timeout, `TimeProvider`, and cancellation token so a starved dispatcher fails through `DevLoopFault.DispatcherTimeout` instead of leaving the probe pending forever; `Reload` routes the three injected hot-reload effects; `Ingest` decodes and re-emits a canonical `ReceiptEnvelope` without changing the origin HLC stamp; `FlameNode.Of` requests the frame correlation from `ProfileSampleSource` and folds every matching AppHost `ProfileSample` into the frame tree; `CollabPreCommit` binds the sync-owner pre-commit tap onto the evidence stream and `CollabJson` names the readable op-window export.
- Auto: the lag sink binds `EvidenceOps.DispatcherLag(<boundary>, elapsed).Seal` at composition, so starvation evidence rides the same envelope stream the dashboards ingest as a case of the one union, and the probe itself names no evidence shape; threshold evaluation stays with the health fold, so the probe carries zero literals; the `decode` column binds the AppHost envelope wire decode at composition so a companion node's receipt frames fold into the same envelope stream as local evidence with no second codec; `Reload` binds the three injected operations at composition under the master gate so the manual-reload intent is a command-table verb on Debug profiles and a structurally-absent route on Release closures where the injected source is stripped.
- Packages: HotAvalonia, Avalonia.Markup.Xaml.Loader (transitive floor, Debug pin), SkiaSharp, System.Reactive, LoroCs (companion, `VersionVector` in the JSON-export delegate signature only), Rasm.AppHost (project, seam types), LanguageExt.Core, NodaTime, BCL inbox
- Growth: one knob row retunes the reload gate, one `ReloadIntent` case absorbs a new manual-reload verb, one probe row absorbs a new loop measure, one `HudSample` field absorbs a new HUD metric, one `HudOverlay` case absorbs a new diagnostic overlay, a new eval outcome is one `CommandReceipt` projection on the one deck route, a new host-profile sample is one AppHost `ProfileSample` value under the profile subtree, and a new collab forensics verb is one member reading the sync owner; zero new surface.
- Boundary: HotAvalonia is a Debug-gated build asset whose injected `UseHotReload`, `EnableHotReload`, `DisableHotReload`, and `TriggerHotReload` extensions on `AppBuilder`/`Application` are the only callable surface — the Release strip is driven by `HotAvaloniaExcludeReferences`, whose default list names `HotAvalonia`, `HotAvalonia.Core`, and `HotAvalonia.Fody` and adds `Avalonia.Markup.Xaml.Loader` when `HotAvaloniaIncludeXamlLoader` is false, while `HotAvaloniaProcessReferences` (default false) governs only whether referenced PROJECTS join the weave scope, and the explicit `Avalonia.Markup.Xaml.Loader` markup-loader pin with its transitive floor lands in the charter admissions so the Debug XAML-compile path resolves and the Release closure carries none of it — the markup loader is the HotAvalonia weaver's Debug-only re-patch dependency, never a managed `AvaloniaXamlLoader.Load` runtime-materialize surface DevLoop exposes, so `TriggerHotReload` re-patches compiled-XAML methods in place while a DevLoop-raised runtime `AvaloniaRuntimeXamlLoader` inflation is the rejected form whose structural fault is `Surfaces.RejectRuntimeInflation`; the manual-reload intent rides composition-bound delegates so DevLoop names no injected symbol directly and the deleted form is a DevLoop-internal reload bootstrap beside the injected extensions; the performance HUD is the `HudSample` feed — frame-elapsed, GPU-elapsed, VRAM bytes, triangle count from the viewport `FrameReceipt`, and the per-node solve elapsed from the Compute solve receipts fold into one HUD sample stream the overlay renders, so the HUD reads the same receipt envelopes the timeline ingests and both a HUD-local meter and a HUD envelope of its own are deleted forms — every column is already sealed evidence, so re-sealing the sample would re-bill the same GPU duration at the usage fold, and an overlay-render failure recovers through the composition-bound `faults` route before the subscription edge's one terminal collapse so no failure is discarded; the diagnostic overlay is the `HudOverlay` vocabulary the same subscription carries — `Plain` renders the sample rows alone and `Overdraw` binds the Skia overdraw colour filter the render composes into its paint, so a draw-pressure heatmap and a plain HUD are one subscription under one row value and a second overlay surface is the deleted form; the ramp is `OverdrawRamp`, a six-band admitted value whose bands come from the `Theme/tokens.md` ramp like every other visual literal, so the native six-colour arity is structural rather than an argument refusal at the draw and a colour array spelled at the call site cannot exist; the filter mints ONCE per subscription and dies with it, so the returned handle disposes the overlay's native lease beside the sample subscription; flamegraphs are the `FlameNode` fold — the per-node solve and per-pass render durations nest into one self-and-total tree the overlay flattens by depth so a profiling flamegraph is a fold over the existing receipt durations, never a second profiler, and `ProfileSampleSource` is the read projection over the AppHost-owned `UiSchedulerPort.ProfileSamples` registration row: AppHost owns the `ProfileSample` shape, its `ProfileFrameForm` posture, and the feed seats, while AppUi filters by correlation and prefix-merges published samples into a `cpu-profile` subtree beside the `cpu` solve and `gpu` pass children, lane-grouped by emitting thread so unrelated stacks never share a prefix and the symbolization posture reaches the reader on the lane name; no duplicate profile record, profiler reference, or second registration port enters this folder; solve time-travel is the `SolveScrub` — each solve frame records its node id and state json keyed by ordinal so a user scrubs the solve history backward and forward and `Diff` surfaces the per-node state delta between two frames, the time-travel debugger over the journal the replay lane already records; cross-machine replay-verify is `ReplayVerify` — a journal replays through the one `ProofEngine.Replay` route and each receipt's payload digest compares to the baseline machine's digest so a cross-machine divergence surfaces as the exact journal index that diverged, the determinism check the headless lanes already prove extended across machines, with replay failure riding the one `IO` carrier to the caller — an `IO<Fin<T>>` public rail that leaves callers re-deciding failure below the boundary is the deleted form; the in-app REPL is `Repl` — a typed line parses into an intent key and payload and evaluates through the one `CommandProjections.Invoke` route so the REPL is the command table's interactive face and a second command-execution path is the rejected form, the eval result the same `CommandReceipt` every invocation route seals — a parallel eval-result union with an unproducible case is the deleted form; remote evidence ingestion decodes frames through the canonical AppHost `ReceiptEnvelope` JSON wire (`AppHostWireContext`) via a composition-bound decode delegate, so a devloop-local envelope codec is the rejected form and a frame decodes exactly as the envelope serialized — the W3C-derived correlation and tenant ride the envelope's own correlation/tenant/HLC slots, so `Ingest` re-emits them UNCHANGED and needs no second wire-context field (the collab-frame carrier is the `Collab/sync.md` `[04]-[LIVE_WIRE]` broadcast/merge concern, not the ingest edge); the collab forensics verbs read the settled sync owner — `CollabPreCommit` binds the composition-supplied `LiveWire.TapPreCommit` installer, seals each `PreCommitFact` through its own `Diagnostics/evidence#RECEIPT_UNION` `EvidenceReceipt.PreCommit` case onto the one `ReceiptSinkPort` the dashboards ingest, and leaves the pending commit's `ChangeModifier` untouched — every fact this loop seals on the AppUi package key is a case of that union, because the usage fold decodes each envelope there and a page-local kind const would refuse a whole chargeback window, while `CollabJson` names the readable op-window export the REPL and support bundle consume through the composition-supplied `LiveWire.ExportJson` delegate, so a second op codec or a second command-execution path is the rejected form; the HARFS remote-server knobs and the runtime timeout and hotkey knobs ride the same MSBuild gate as the master row and carry no managed surface.

```csharp signature
[Union]
public abstract partial record DevLoopFault : Expected {
    private DevLoopFault(string detail, int code) : base(detail, code, None) { }
    public sealed record FrameAbsent : DevLoopFault { public FrameAbsent(string detail) : base(detail, AppUiFaultBand.DevLoop.Code(0)) { } }
    public sealed record DispatcherTimeout : DevLoopFault { public DispatcherTimeout(string detail) : base(detail, AppUiFaultBand.DevLoop.Code(1)) { } }
    public sealed record Stream : DevLoopFault { public Stream(string detail) : base(detail, AppUiFaultBand.DevLoop.Code(2)) { } }
}

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
    public static HudSample Of(FrameReceipt frame, GpuTimeline gpu, long vramBytes, Seq<Duration> solveNodes) =>
        new(frame.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed), gpu.MeasuredGpu, vramBytes, frame.Triangles,
            solveNodes.Fold(Duration.Zero, static (total, elapsed) => total + elapsed), solveNodes.Count);
}

// Overdraw ramps carry SIX BANDS because Skia's overdraw filter admits exactly six colours — one per
// accumulated draw count with the last saturating — so band count is this value's shape rather than an
// argument the native entry refuses, and each band is a Theme token like every other visual literal.
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

// Diagnostic overlays are ONE vocabulary the single HUD subscription carries: a case needing no paint
// state carries none, and a case that does carries its own admitted ramp — overlay IS the value, so a
// `bool overdraw` knob beside the sample feed and a second overlay subscription are both unspellable.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record HudOverlay {
    private HudOverlay() { }
    public sealed record Plain : HudOverlay;
    public sealed record Overdraw(OverdrawRamp Ramp) : HudOverlay;

    public Option<SKColorFilter> Filter() => Switch(
        plain: static _ => Option<SKColorFilter>.None,
        overdraw: static heat => Some(heat.Ramp.Filter()));
}

// Read projection over the AppHost UiSchedulerPort.ProfileSamples feed. Composition buffers the
// AppHost-owned ProfileSample values by correlation; this delegate only reads one correlation window.
// Frames arrive root-first and pre-bounded by the producer's frame cap, so the fold never reverses or trims.
public delegate Seq<ProfileSample> ProfileSampleSource(CorrelationId correlation);

// The pre-commit observation the sync-owner tap seals — Loro ChangeMeta (lamport, wall time, message, op
// length) plus the change origin and the session correlation, so a merge dispute reads as an inspectable
// operation record on the evidence stream, never an opaque byte blob.
public readonly record struct PreCommitFact(string DocumentKey, uint Lamport, long Timestamp, Option<string> Message, uint Len, string Origin, CorrelationId Correlation);

public sealed record FlameNode(string Frame, Duration Self, Seq<FlameNode> Children) {
    public Duration Total => Self + Children.Fold(Duration.Zero, static (acc, child) => acc + child.Total);

    public Seq<(string Frame, Duration Total, int Depth)> Flatten(int depth = 0) =>
        Seq((Frame, Total, depth)) + Children.Bind(child => child.Flatten(depth + 1));

    // The frame tree joins three sources under one root: the cpu-solve residual, the gpu-pass durations,
    // and the OPTIONAL host profile subtree — matched ProfileSample frame paths fold into a cpu-profile
    // child so a hot native stack renders beside the receipt-derived durations; absent a sample the
    // profile child never appears, so the flamegraph degrades to the receipt fold with no gap.
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

    // Prefix-merge the samples into one profile subtree, grouped by the emitting thread so unrelated
    // stacks never share a prefix. Inside a lane shared frame prefixes collapse to shared nodes and each
    // sample's weight accrues at its own leaf, so a hot path renders as a wide bar. A sample is grafted
    // frame-by-frame; a frame already present at a level recurses into it, otherwise a fresh child grows.
    // Lane accumulation rides `Map`, not `HashMap`: key order IS the operation here, so lane roots emit in
    // lane-name order and the same sample set renders one tree — hash-ordered `Values` reorders siblings
    // per run and breaks the render-hash baseline the proof lane compares against.
    static FlameNode FromSamples(string root, Seq<ProfileSample> samples) =>
        new(root, Duration.Zero,
            samples.Fold(Map<string, FlameNode>(), static (lanes, sample) =>
                lanes.AddOrUpdate(Lane(sample),
                    node => Graft(node, toSeq(sample.Frames), Duration.FromMilliseconds(sample.WeightMillis)),
                    () => Graft(new FlameNode(Lane(sample), Duration.Zero, Seq<FlameNode>()),
                        toSeq(sample.Frames), Duration.FromMilliseconds(sample.WeightMillis))))
                .Values.ToSeq().Strict());

    // Lane names carry the AppHost symbolization posture, so an address-form tree never renders as
    // resolved call frames; AppHost stamps ProfileFrameForm.Address whenever no symbol source was bound.
    static string Lane(ProfileSample sample) =>
        string.Create(CultureInfo.InvariantCulture, $"thread {sample.ThreadId} ({sample.Form.Key})");

    static FlameNode Graft(FlameNode node, Seq<string> frames, Duration self) =>
        frames.Head.Match(
            None: () => node with { Self = node.Self + self },
            Some: head => node.Children.Exists(child => child.Frame == head)
                ? node with { Children = node.Children.Map(child => child.Frame == head ? Graft(child, frames.Tail, self) : child) }
                : node with { Children = node.Children.Add(Graft(new FlameNode(head, Duration.Zero, Seq<FlameNode>()), frames.Tail, self)) });
}

public sealed record SolveFrame(int Ordinal, string NodeId, JsonElement State, Instant At);
public sealed record SolveDelta(string NodeId, JsonElement From, JsonElement To);

public sealed record SolveScrub(Seq<SolveFrame> Frames) {
    // The ordinal IS the journal position, stamped at record rather than carried in by a producer: the
    // transport bounds its playhead on `Frames.Count` and reads the frame back by that index, so an ordinal
    // the producer chose would be a second coordinate the two ends disagree on in silence — a journal opened
    // mid-solve or recorded sparsely resolves nothing at every position the transport reports live, and
    // `Restore` refuses the frame the scrub is standing on. One coordinate makes the read O(1) besides.
    public Option<SolveFrame> At(int ordinal) =>
        ordinal >= 0 && ordinal < Frames.Count ? Some(Frames[ordinal]) : None;

    public SolveScrub Record(string nodeId, JsonElement state, Instant at) =>
        this with { Frames = Frames.Add(new SolveFrame(Frames.Count, nodeId, state, at)) };

    public Option<(SolveFrame From, SolveFrame To)> Window(int from, int to) =>
        (At(from), At(to)) switch {
            ({ IsSome: true, Case: SolveFrame a }, { IsSome: true, Case: SolveFrame b }) => Some((a, b)),
            _ => None,
        };

    public IO<Unit> Restore(int ordinal, Func<SolveFrame, IO<Unit>> apply) =>
        At(ordinal).Match(
            Some: apply,
            None: () => IO.fail<Unit>(new DevLoopFault.FrameAbsent($"solve frame {ordinal} is absent")));

    public Option<SolveDelta> Diff(int from, int to) =>
        Window(from, to).Bind(pair => JsonElement.DeepEquals(pair.From.State, pair.To.State)
            ? None
            : Some(new SolveDelta(pair.To.NodeId, pair.From.State, pair.To.State)));
}

// Every eval outcome IS a CommandReceipt from the one deck route — a parallel eval-result union whose
// second case no path produces is the illusory form this owner deletes; a parse refusal stays on IO.
public sealed record Repl(CommandDeck Deck, Func<string, Fin<(string Key, JsonElement Payload)>> Parse) {
    public IO<CommandReceipt> Eval(string line) =>
        Parse(line).Match(
            Succ: parsed => Deck.Invoke(parsed.Key, parsed.Payload),
            Fail: static error => IO.fail<CommandReceipt>(error));
}

public static class DevLoop {
    // The marshal callback is pure — one TrySetResult write; the elapsed value re-enters the rail
    // through the gate, so the lag sink sequences on the one carrier and its failure reaches the caller.
    public static IO<Unit> DispatcherLag(SurfaceScheduler boundary, TimeProvider time, Duration timeout, CancellationToken cancellation, Func<Duration, IO<Unit>> sink) =>
        IO.lift(() => (Mark: time.GetTimestamp(), Gate: new TaskCompletionSource<Duration>(TaskCreationOptions.RunContinuationsAsynchronously)))
            .Bind(state => (boundary.Marshal(() => state.Gate.TrySetResult(Duration.FromTimeSpan(time.GetElapsedTime(state.Mark))))
                .Bind(_ => IO.liftAsync(async () => await state.Gate.Task
                    .WaitAsync(timeout.ToTimeSpan(), time, cancellation)
                    .ConfigureAwait(false)))
                | @catch<IO, Duration>(static _ => true, static error => IO.fail<Duration>(new DevLoopFault.DispatcherTimeout(error.Message)))))
            .Bind(sink);

    // Callers hold the subscription as their lifetime handle: disposing it detaches the overlay AND
    // releases its native filter, so repeated Hud runs never stack duplicate render callbacks on one
    // sample feed nor leak a colour filter per run. Filters mint once per subscription rather than per
    // sample, Synchronize serializes concurrent sample emissions, and the Rx callback is the named
    // terminal edge — recovery composes before the one Run, so no failure is discarded.
    public static IO<IDisposable> Hud(
        IObservable<HudSample> samples,
        IScheduler scheduler,
        HudOverlay overlay,
        Func<HudSample, Option<SKColorFilter>, IO<Unit>> render,
        Func<Error, IO<Unit>> faults) =>
        IO.lift(() => overlay.Filter()).Map(filter => (IDisposable)new CompositeDisposable(
            samples.Synchronize().ObserveOn(scheduler).Subscribe(
                sample => ignore((render(sample, filter) | @catch<IO, Unit>(static _ => true, error => faults(error))).As().Run()),
                error => ignore(faults(new DevLoopFault.Stream(error.Message)).Run())),
            Disposable.Create(filter, static held => held.Iter(static lease => lease.Dispose()))));

    // A length mismatch IS a divergence: indices past the shorter side report as mismatches, so a
    // dropped or extra tail receipt never hides behind pairwise truncation.
    public static IO<Seq<int>> ReplayVerify(
        Seq<(string Key, JsonElement Payload)> journal,
        CommandDeck deck,
        Func<IO<Unit>> restore,
        Seq<string> baseline) =>
        ProofEngine.Replay(deck, journal, restore)
            .Map(replayed => toSeq(Enumerable.Range(0, Math.Max(replayed.Count, baseline.Count))
                .Where(index => index >= replayed.Count || index >= baseline.Count || replayed[index].PayloadDigest != baseline[index])));

    public static IO<Unit> Reload(ReloadIntent intent, Func<IO<Unit>> trigger, Func<IO<Unit>> enable, Func<IO<Unit>> disable) =>
        intent.Switch(
            state: (Trigger: trigger, Enable: enable, Disable: disable),
            trigger: static (ops, _) => ops.Trigger(),
            enable: static (ops, _) => ops.Enable(),
            disable: static (ops, _) => ops.Disable());

    public static IO<Unit> Ingest(ReceiptSinkPort sink, Func<ReadOnlyMemory<byte>, Fin<ReceiptEnvelope>> decode, ReadOnlyMemory<byte> frame) =>
        decode(frame).Match(
            Succ: sink.Emit,
            Fail: static error => IO.fail<Unit>(error));

    // The pre-commit tap is a composition-bound subscription over the sync owner: `install` is
    // LiveWire.TapPreCommit and each fact seals as its own evidence case onto the same ReceiptSinkPort the
    // dashboards ingest — no second collab surface, no second command-execution path, and no envelope minted
    // beside the sink's own HLC advance. The fact carries the session correlation it was observed under, so
    // only the ambient tenant threads in. The returned IDisposable is the caller's tap lifetime.
    public static IO<IDisposable> CollabPreCommit(
        Func<Func<PreCommitFact, IO<Unit>>, Func<Error, IO<Unit>>, IDisposable> install,
        ReceiptSinkPort sink,
        TenantContext tenant,
        Func<Error, IO<Unit>> faults) =>
        IO.lift(() => install(
            fact => EvidenceOps.PreCommit(fact).Seal(sink, fact.Correlation, tenant).Map(static _ => unit),
            faults));

    // The readable op-window export the REPL and support bundle consume: `export` is LiveWire.ExportJson,
    // so the JSON codec stays the sync owner's and this verb only names the devloop's readable-op edge.
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
- Entry: `public static IO<Unit> Attach(Application app, InspectorAttach row)` — one Debug-composition call; the attach is the only imperative edge, and the two handler bodies are composition-bound values the row carries into `DevToolsOptions`.
- Auto: `DevToolsOptions` carries the default `F12` gesture, `LaunchView : DevToolsViewKind`, `HotKeys : HotKeyConfiguration`, `ScreenshotHandler : IScreenshotHandler`, and `PropertyEditHandler : IDevToolsPropertyEditHandler?` — one config row, every knob a field; `PropertyValueEditorService` owns live property commits from the inspector so an edit lands through the service, never an ad-hoc reflection write; `VisualTreeDebug` owns the layout/renderer overlays; `VisualExtensions.RenderTo(Control, Stream, double)` is the control-snapshot lane `InspectorCapture` composes — its stream feeds the same capture encode fold `proof.md` owns, so an inspector screenshot is a `CaptureRow` sibling, never a second pixel path; `IDevToolsPropertyEditHandler.OnPropertyEdited(DevToolsPropertyEdit)` hands `InspectorEdits` the full edit record — inspected object, target, property and XAML names, declaring and property types, old and new values with their rendered text, the attached and Avalonia-property discriminants, and the resource-reference kind with its key — so the sealed receipt carries the whole commit rather than a re-derived summary.
- Packages: ProDiagnostics (Debug-gated, `PrivateAssets="all"`), Avalonia, LanguageExt.Core
- Growth: a new inspector knob is one `DevToolsOptions` field on the row; a new snapshot destination is one delivery delegate the capture row already carries; zero new surface.
- Boundary: ProDiagnostics is Debug-gated `PrivateAssets="all"` in the csproj `Dev Loop` group beside HotAvalonia and is absent from the Release surface — a Release-profile attach is structurally unrepresentable; the `ProDataGrid`/`ProCharts` siblings are NOT admitted; `Conventions.DefaultScreenshotHandler` is `internal`, so the package's own file-picker default is unreachable by name and `InspectorCapture` is the only handler this folder can bind — a consumer-side reference to that default is unspellable, never a policy choice; both handler seams collapse a typed rail at a host signature that carries none — `Take` returns a bare `Task` and `OnPropertyEdited` returns `void` — so each parks its typed failure on the composition-bound evidence route BEFORE the host value returns, and a handler that maps its failure to a swallowed exception is the deleted form; the inspector composes the SAME evidence spine as every loop measure — a property commit through `PropertyValueEditorService` projects onto the `EvidenceReceipt.Edit` case and stamps that case's own literal, so inspector mutations are attributable on the timeline exactly as the dispatcher-lag and pre-commit measures are, and the loop mints no kind of its own at any of the three; both first-party alternates failed the admission gate (`Avalonia.Diagnostics` feed-dead at 11.3.x with no Avalonia-12 asset; the Accelerate DevTools pay-tiered, license-gate rejected) — the record stands, never re-proposed.

```csharp signature
public sealed record InspectorAttach(DevToolsOptions Options);

// RenderTo writes each control snapshot at an admitted dpi into a capsule-owned sink, bytes detach
// before that sink closes, and delivery rides the composition-bound capture fold, so an inspector
// snapshot encodes exactly as every other CaptureRow. Take is the host's own async edge, so this rail
// collapses HERE — recovery runs before the awaited Task returns.
public sealed record InspectorCapture(
    double Dpi,
    Func<string, ReadOnlyMemory<byte>, IO<Unit>> Deliver,
    Func<Error, IO<Unit>> Faults) : IScreenshotHandler {

    public Task Take(Control control) =>
        (Snapshot(control, Dpi).Bind(bytes => Deliver(control.GetType().Name, bytes))
            | @catch<IO, Unit>(static _ => true, error => Faults(error))).As().RunAsync().AsTask();

    static IO<ReadOnlyMemory<byte>> Snapshot(Control control, double dpi) =>
        IO.lift(() => {
            // Exemption: RenderTo writes into a stream, so the capsule owns the sink and hands out
            // detached bytes — the stream never leaves this frame.
            using MemoryStream sink = new();
            control.RenderTo(sink, dpi);
            return (ReadOnlyMemory<byte>)sink.ToArray();
        });
}

// Every live inspector edit seals as an Edit-case EvidenceReceipt on the same stream the dashboards
// ingest, so a mutation made through the inspector is attributable exactly as one made through the
// command deck and stamps the union's own `edit` literal — an inspector-private kind beside it would
// name a second producer for one fact. Composition binds the projection alone; the seal is the union's,
// so the envelope carries the sink's own HLC advance rather than a hand-minted stamp. OnPropertyEdited
// returns void, so a typed refusal parks on the composition-bound faults route rather than dissolving
// at the host signature.
public sealed record InspectorEdits(
    Func<DevToolsPropertyEdit, EvidenceReceipt> Project,
    ReceiptSinkPort Sink,
    CorrelationId Correlation,
    TenantContext Tenant,
    Func<Error, IO<Unit>> Faults) : IDevToolsPropertyEditHandler {

    public void OnPropertyEdited(DevToolsPropertyEdit edit) =>
        ignore((Project(edit).Seal(Sink, Correlation, Tenant).Map(static _ => unit)
            | @catch<IO, Unit>(static _ => true, error => Faults(error))).As().Run());
}

public static class Inspector {
    public static IO<Unit> Attach(Application app, InspectorAttach row) =>
        IO.lift(() => ignore(app.AttachDevTools(row.Options)));
}
```

## [04]-[LOOP_SURFACES]

- Owner: `FlameSpan` with `FlameView` — the laid-out flame projection carrying lane grouping, the zoom-to-span re-root, and the hover hit; `ScrubTransport` — the solve scrub bound to the ONE transport grammar; `ReplBlock` `[Union]` with `BlockStream` — the typed, height-indexed, filterable block stream the REPL and the log share; `DiagnosticsChrome` — the HUD chip rows the perf sample and the quality readout bind.
- Cases: `ReplBlock` = Command | Refused | Log | Timeline — a typed line and its sealed receipt, a parse or admission refusal, a captured line burst, and a correlated evidence timeline, every case carrying the ordinal, the instant, and the one `Query` column the filter reads.
- Entry: `public Seq<FlameSpan> Spans` and `public Option<FlameSpan> Hit(double fraction, int depth)` on `FlameView`; `public static Fin<ScrubTransport> Of(SolveScrub scrub)` and `public ScrubTransport Raise(TransportVerb verb)`; `public Seq<(ReplBlock Block, double Offset, double Extent)> Index(double row)` on `BlockStream`; `public static Seq<ChromeRow> Rows()` on `DiagnosticsChrome`.
- Auto: the flame layout is the `Charts/custom#SKIA_KINDS` `CustomVisuals.WedgeSpans` parent-share nesting — the ONE fold the sunburst ring, the flame row, and this hit-test all read — so the rectangle a pointer lands on is the rectangle the plane drew and the flame renders as one `CustomVisual.Flame` row over the existing `VisualPayload.Wedge`, exactly as the gantt and timeline rows are two readings of one plan; zoom-to-span RE-ROOTS to the focused node so a hot leaf becomes the whole width and its own children become readable, while a stale focus path after a re-fold widens back to the root rather than rendering an empty view; the lane a span carries is its depth-one ancestor's frame, so a profile subtree's thread lanes group naturally and zooming into that subtree promotes the threads to lanes with no special case; the scrub folds the settled `Render/animation#TIMELINE_EDITOR` `TransportVerb` roster over the shared `TransportState`, so play, pause, stop, both steps, both jumps, loop, and speed reach the solve journal with no verb spelled here; the block stream declares each case's extent off the arity it already holds, so the height index is a running prefix rather than a measure pass and a filtered stream re-indexes without realizing a block; per-block copy and bookmark are command-table intent keys, so a block affordance is a deck verb like every other and the bookmark set is stream state rather than a per-block flag; the HUD chips are ordinary `Shell/navigation#SHELL_CHROME` `ChromeRow` rows on `ChromeSlot.Hud`, so a diagnostics chip takes the same slot admission, materialization, corner placement, and numeric typography role every viewport chip takes.
- Receipt: the surfaces seal nothing — every fact they render is already on the envelope stream (`HudSample` folds sealed receipts, the scrub reads the recorded journal, a command block carries the deck's own `CommandReceipt`, and the quality chips read the governor cell), so a presentation-layer seal would re-bill a duration the usage fold already accrued.
- Packages: Avalonia, DynamicData, System.Reactive, SkiaSharp, Thinktecture.Runtime.Extensions, LanguageExt.Core, NodaTime, BCL inbox
- Growth: a new flame lane is one grouping level in the tree the fold already walks; a new transport verb is one `TransportVerb` row consumed here with zero edit; a new block family is one `ReplBlock` case carrying its own extent and render arms; a new HUD readout is one `ChromeRow` row naming a fact key its owner declares; zero new surface.
- Boundary: these are PROJECTIONS and mint no instrument, no clock, and no second command path — the flame reads the `FlameNode` fold, the scrub reads `SolveScrub`, the REPL evaluates through the one `Repl.Eval` route onto `CommandProjections.Invoke`, and the chips read `HudSample` and `GovernorReadout`; the flame's DRAW rides the one custom-visual plane and a devloop-local Skia surface is the deleted form, because a flame graph is chart semantics the plane already owns once its layout row lands, while the zoom, lane, and hover questions stay HERE because they are questions about the frame tree rather than about pixels; a scrub-local next/previous pair beside the transport grammar is the rejected form — the two would disagree the first time looping or a speed change landed on one of them — and the scrub mints no playhead of its own beyond the ordinal-rate one its journal implies; the REPL and the log are ONE stream because they are one thing, an operator typing a line and the rich view of what it answered belonging directly under that line, so two panes are the deleted form and a scrollback buffer beside the block list is the second store this index retires; the stream binds the `Shell/virtualization#WINDOW_OWNER` fabric through `VirtualWindowSpec.Measured` with each case's declared extent as its seed, so a session of ten thousand blocks realizes a constant window and a devloop-local list virtualizer is that page's named rejected form; the filter reads each block's own `Query` column so no case is invisible to a search it should answer, and a per-case filter predicate is the shape that would let a log body match while a timeline correlation silently could not; bookmarks live on the STREAM keyed by ordinal rather than as a flag on a block, because a block is an immutable record of what happened and a bookmark is a reader's own annotation over it.

```csharp signature
// One laid-out flame rectangle. Geometry is FRACTIONS rather than pixels because the mount owns the extent —
// a span carrying pixels re-lays out on every resize — and the durations ride along so a hover detail reads
// self, total, and share off the value the pointer already resolved.
public readonly record struct FlameSpan(string Frame, string Lane, int Depth, double Start, double Width, Duration Self, Duration Total) {
    public double Share => Width;
}

// The indexed flatten row: one node's contribution carrying the lane it inherited, the parent index the wedge
// admission requires, and the durations the span projects.
public readonly record struct FlameRow(string Frame, string Lane, Duration Self, Duration Total, int Depth, int Parent);

public sealed record FlameView(FlameNode Root, Seq<string> Focus) {
    public static FlameView Of(FlameNode root) => new(root, Seq<string>());

    // Zoom-to-span RE-ROOTS rather than scaling a viewport: the focused node becomes the whole width, so a
    // two-percent leaf fills the view and its own children become legible. Scaling instead keeps every sibling
    // in the layout at sub-pixel width, which is exactly what makes a hot leaf unreachable to begin with.
    public FlameView Zoom(Seq<string> path) => this with { Focus = path };

    public FlameView Reset() => this with { Focus = Seq<string>() };

    // A focus path that no longer names a live node widens back to the node it reached, so a re-fold under a
    // held zoom degrades to a wider view rather than to an empty one an operator would read as a quiet frame.
    public FlameNode Focused =>
        Focus.Fold(Root, static (node, frame) => node.Children.Find(child => child.Frame == frame).IfNone(node));

    // The flatten threads the lane DOWN from each depth-one child, so a profile subtree's thread roots become
    // the lanes and zooming into that subtree promotes them with no special case. A zero-total subtree drops
    // WHOLE — its children are necessarily zero too, since a total is self plus children — so a frame that
    // measured nothing renders nothing and the wedge admission never sees the zero value it refuses.
    public Seq<FlameRow> Rows => Grow(Focused, Focused.Frame, depth: 0, parent: -1, Seq<FlameRow>());

    static Seq<FlameRow> Grow(FlameNode node, string lane, int depth, int parent, Seq<FlameRow> held) =>
        node.Total <= Duration.Zero
            ? held
            : held.Add(new FlameRow(node.Frame, lane, node.Self, node.Total, depth, parent)) switch {
                var seeded => node.Children.Fold(seeded, (rows, child) =>
                    Grow(child, depth == 0 ? child.Frame : lane, depth + 1, seeded.Count - 1, rows)),
            };

    // The payload the `CustomVisual.Flame` row folds. Value is the node's TOTAL, because the nesting is a
    // parent-share fold and a self-weighted tree would make every parent narrower than its own children.
    public VisualPayload.Wedge Payload =>
        new(Rows.Map(static row => (row.Frame, row.Total.ToTimeSpan().TotalNanoseconds, row.Depth, row.Parent)));

    // ONE nesting arithmetic serves the draw and the hit-test: `CustomVisuals.WedgeSpans` is the shared
    // parent-share fold the sunburst ring and the flame row both lay out from, so a page-local start-and-width
    // fold beside it is the deleted form that would drift on the first nesting repair and leave the pointer
    // resolving a rectangle the plane never drew.
    public Seq<FlameSpan> Spans =>
        Rows switch {
            var rows => CustomVisuals.WedgeSpans(rows.Map(static row => (row.Frame, row.Total.ToTimeSpan().TotalNanoseconds, row.Depth, row.Parent)))
                .Map(span => new FlameSpan(
                    rows[span.Index].Frame, rows[span.Index].Lane, span.Depth,
                    span.Start, span.Span, rows[span.Index].Self, rows[span.Index].Total))
                .Strict(),
        };

    public int Depth => Rows.Fold(0, static (deepest, row) => Math.Max(deepest, row.Depth));

    // The lane roster in emission order, so a legend and the lane bands read one sequence.
    public Seq<string> Lanes => Rows.Map(static row => row.Lane).Distinct().Strict();

    // Hover detail: the span covering the pointer at its own depth row. Depth arrives resolved from the mount's
    // row height, so this owner converts no pixels and the two ends cannot disagree about a row boundary.
    public Option<FlameSpan> Hit(double fraction, int depth) =>
        Spans.Find(span => span.Depth == depth && fraction >= span.Start && fraction < span.Start + span.Width);
}

// The solve scrub IS a playback surface, so it reads the ONE transport grammar the timeline editor declares
// rather than minting verbs of its own — a scrub-local next/previous pair would be a second grammar over one
// motion, and the two would disagree the first time looping or a speed change landed on one of them.
public sealed record ScrubTransport(SolveScrub Scrub, TransportState Transport) {
    // A solve journal carries no wall-clock rate: its ORDINALS are its frames, so the playhead advances one
    // ordinal per unit and `Playhead.Position` is never read here. Binding the recorded instants to the rate
    // instead makes a step skip ordinals wherever the solve ran unevenly — precisely where an operator steps.
    public const double OrdinalRate = 1d;

    public static Fin<ScrubTransport> Of(SolveScrub scrub) =>
        scrub.Frames.IsEmpty
            ? Fin.Fail<ScrubTransport>(new DevLoopFault.FrameAbsent("the solve journal recorded no frame"))
            : Fin.Succ(new ScrubTransport(scrub, TransportState.Of(
                Playhead.At(OrdinalRate, Duration.FromSeconds(scrub.Frames.Count - 1), PlaybackMode.Once))));

    public ScrubTransport Raise(TransportVerb verb) => this with { Transport = verb.Fold(Transport, Transport.Head) };

    public ScrubTransport Tick() => this with { Transport = Transport.Advanced() };

    // The frame the head is over, so a scrub read and a transport fold can never name different ordinals.
    public Option<SolveFrame> Current => Scrub.At(checked((int)Transport.Head.Index));

    // Restore rides the settled scrub rail at the head's OWN ordinal, so a caller cannot apply a frame the
    // transport is not on and the time-travel apply keeps one entry.
    public IO<Unit> Restore(Func<SolveFrame, IO<Unit>> apply) => Scrub.Restore(checked((int)Transport.Head.Index), apply);

    // The comparison window is the transport's own range, so a scrub-local from/to pair is unspellable: an
    // operator sets the in point with `jump-in` and the delta reads from there to wherever the head landed.
    public Option<SolveDelta> Delta => Scrub.Diff(checked((int)Transport.Head.First), checked((int)Transport.Head.Index));
}

// The REPL and the log are ONE ordered stream of typed blocks, because they are one thing: an operator types a
// line, the deck seals a receipt, and the rich view of that receipt belongs directly under the line that
// produced it. Two surfaces put the answer in a pane the question is not in. `Query` is the one column the
// filter reads, declared on the base so every case is reachable by a search that should answer it.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplBlock(long Ordinal, Instant At, string Query) {
    public sealed record Command(long Ordinal, Instant At, string Line, CommandReceipt Receipt) : ReplBlock(Ordinal, At, Line);
    public sealed record Refused(long Ordinal, Instant At, string Line, Error Fault) : ReplBlock(Ordinal, At, Line);
    public sealed record Log(long Ordinal, Instant At, string Boundary, Seq<string> Lines) : ReplBlock(Ordinal, At, string.Join('\n', Lines));
    public sealed record Timeline(long Ordinal, Instant At, EvidenceTimeline Value) : ReplBlock(Ordinal, At, Value.Correlation.ToString());

    // Extent is DECLARED off the arity the block already holds rather than measured, because a measured window
    // must realize a block to learn its height and would therefore realize the whole stream to scroll to its
    // end — the exact cost the virtualization fabric exists to delete. The declared value is the ledger's SEED,
    // so a block whose rendered height differs re-measures once and the scrollbar settles without a jump.
    // The threaded row height is spelled `height`, never `unit` — `unit` is the Prelude's own `Unit` singleton
    // every rail in this folder returns, and a `double` wearing that name reads as the empty value at exactly
    // the sites that multiply by it.
    public double Extent(double row) => Switch(
        state:   row,
        command: static (height, block) => height * 2d,
        refused: static (height, block) => height * 2d,
        log:     static (height, block) => height * Math.Max(1, block.Lines.Count),
        timeline: static (height, block) => height * (block.Value.Rows.Count + 1));

    // Every block materializes through the ONE control factory, so a command echo, a refusal, a log burst, and
    // an evidence table are four arms of one vocabulary and the block stream mints no control of its own. The
    // timeline arm reads the SAME `EvidenceReport.Blocks` table the export plane paginates, so the in-app view
    // and the exported report can never name different columns.
    public ControlIntent Render(double row) => Switch(
        state:   row,
        command: static (height, block) => (ControlIntent)new ControlIntent.Panel(
            $"{DevLoopSurfaces.BlockKey}.{block.Ordinal}",
            Seq<ControlIntent>(
                new ControlIntent.Label($"{DevLoopSurfaces.BlockKey}.{block.Ordinal}.line", block.Line, TypographyRole.Code, IntentBinding.Of(PaintRole.Text)),
                new ControlIntent.Label($"{DevLoopSurfaces.BlockKey}.{block.Ordinal}.outcome", block.Receipt.Outcome.Kind, TypographyRole.Numeric, IntentBinding.Of(PaintRole.TextMuted))),
            DevLoopSurfaces.BlockProgram, IntentBinding.Of(PaintRole.Surface)),
        refused: static (height, block) => (ControlIntent)new ControlIntent.Panel(
            $"{DevLoopSurfaces.BlockKey}.{block.Ordinal}",
            Seq<ControlIntent>(
                new ControlIntent.Label($"{DevLoopSurfaces.BlockKey}.{block.Ordinal}.line", block.Line, TypographyRole.Code, IntentBinding.Of(PaintRole.Text)),
                new ControlIntent.Label($"{DevLoopSurfaces.BlockKey}.{block.Ordinal}.fault", block.Fault.Message, TypographyRole.Code, IntentBinding.Of(PaintRole.ErrorText))),
            DevLoopSurfaces.BlockProgram, IntentBinding.Of(PaintRole.Surface)),
        log:     static (height, block) => (ControlIntent)new ControlIntent.Panel(
            $"{DevLoopSurfaces.BlockKey}.{block.Ordinal}",
            block.Lines.Map((line, index) => (ControlIntent)new ControlIntent.Label(
                $"{DevLoopSurfaces.BlockKey}.{block.Ordinal}.{index}", line, TypographyRole.Code, IntentBinding.Of(PaintRole.TextMuted))),
            DevLoopSurfaces.BlockProgram, IntentBinding.Of(PaintRole.Surface)),
        timeline: static (height, block) => (ControlIntent)new ControlIntent.Grid(
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
            VirtualWindowSpec.FixedRow(height * (block.Value.Rows.Count + 1)),
            IntentBinding.Of(PaintRole.Surface)));
}

// The block stream: one ordered list, one height index, one filter, one bookmark set. Bookmarks live HERE
// keyed by ordinal rather than as a flag on a block, because a block is an immutable record of what happened
// and a bookmark is a reader's own annotation over it — a mutable flag would make the record editable to make
// the annotation possible.
public sealed record BlockStream(Seq<ReplBlock> Blocks, Set<long> Bookmarks, Option<string> Filter) {
    public static BlockStream Open() => new(Seq<ReplBlock>(), Set<long>(), None);

    public BlockStream Append(ReplBlock block) => this with { Blocks = Blocks.Add(block) };

    // Bookmarking TOGGLES, because the affordance an operator reaches for is one verb over a block whose state
    // they can already see — a separate clear verb doubles the deck rows for one bit.
    public BlockStream Bookmark(long ordinal) =>
        this with { Bookmarks = Bookmarks.Contains(ordinal) ? Bookmarks.Remove(ordinal) : Bookmarks.Add(ordinal) };

    public BlockStream Narrow(Option<string> query) => this with { Filter = query };

    // Filtering reads each block's OWN `Query` column, so a typed line, a log body, and a timeline correlation
    // all match one predicate; a per-case predicate is the shape that lets a log body match while a correlation
    // silently cannot. A bookmarked block survives every filter, because a reader who marked it asked for it.
    public Seq<ReplBlock> Visible =>
        Filter.Match(
            Some: query => Blocks.Filter(block =>
                Bookmarks.Contains(block.Ordinal) || block.Query.Contains(query, StringComparison.OrdinalIgnoreCase)),
            None: () => Blocks);

    // The height index is a running prefix over the visible blocks, so seeking an ordinal is a lookup rather
    // than a walk and a filter change re-indexes without measuring anything.
    public Seq<(ReplBlock Block, double Offset, double Extent)> Index(double row) =>
        Visible.Fold((Rows: Seq<(ReplBlock, double, double)>(), Offset: 0d), (state, block) =>
            block.Extent(row) switch {
                var extent => (state.Rows.Add((block, state.Offset, extent)), state.Offset + extent),
            }).Rows;

    public double Height(double row) => Index(row).Fold(0d, static (sum, entry) => sum + entry.Extent);

    // The block a scroll offset lands INSIDE, so a pointer between two blocks resolves to the one containing
    // it rather than to a boundary no block owns.
    public Option<ReplBlock> At(double row, double offset) =>
        Index(row).Find(entry => offset >= entry.Offset && offset < entry.Offset + entry.Extent)
            .Map(static entry => entry.Block);

    // The window request rides the one fabric with each case's declared extent as the ledger seed, so ten
    // thousand blocks realize a constant window and a devloop-local list virtualizer is the fabric's own
    // rejected form.
    public static VirtualWindowSpec Window(double viewportExtent, double row) =>
        VirtualWindowSpec.Measured(viewportExtent, row);
}

public static class DevLoopSurfaces {
    public const string BlockKey = "devloop.block";
    public const string BlockProgram = "devloop.block.stack";

    // Per-block affordances are COMMAND KEYS, so copy and bookmark carry gestures, availability, and receipts
    // off the one intent table exactly as every other verb does; a block-local button is the deleted form.
    public const string CopyVerb = "devloop.block.copy";
    public const string BookmarkVerb = "devloop.block.bookmark";
    public const string FilterVerb = "devloop.block.filter";
    public const string FlameZoomVerb = "devloop.flame.zoom";
    public const string FlameResetVerb = "devloop.flame.reset";

    // Chip fact keys this loop's own readouts answer; the governor's keys stay at the governor.
    public const string FrameFact = "devloop.frame";
    public const string GpuFact = "devloop.gpu";
    public const string VramFact = "devloop.vram";
    public const string TrianglesFact = "devloop.triangles";
    public const string SolveFact = "devloop.solve";
}

// The diagnostics HUD is the SAME chrome family every other HUD reads: chips are `ChromeRow` rows on the HUD
// slot placed by corner, materialized through the one `ShellChrome.Materialize` fold into numeric labels whose
// fixed advance width keeps a live figure from jittering on every digit change. A diagnostics-local overlay
// panel beside the chrome family is the deleted form — it would carry its own placement, its own visibility
// matrix, and its own typography, and would disagree with the viewport chips on all three.
public static class DiagnosticsChrome {
    // Rank orders within the corner; the corner is the placement value the proportional canvas reads. Perf
    // chips lead the trailing corner and the governor readout follows, so a degrade reads next to the numbers
    // that caused it rather than across the surface from them.
    static readonly Seq<(string Fact, CornerPosition Corner, int Rank)> Chips = Seq(
        (DevLoopSurfaces.FrameFact, CornerPosition.TopRight, 0),
        (DevLoopSurfaces.GpuFact, CornerPosition.TopRight, 1),
        (DevLoopSurfaces.VramFact, CornerPosition.TopRight, 2),
        (DevLoopSurfaces.TrianglesFact, CornerPosition.TopRight, 3),
        (DevLoopSurfaces.SolveFact, CornerPosition.TopRight, 4),
        (GovernorReadout.TierFact, CornerPosition.TopRight, 5),
        (GovernorReadout.BreachFact, CornerPosition.TopRight, 6),
        (GovernorReadout.HeadroomFact, CornerPosition.TopRight, 7),
        (GovernorReadout.HistoryFact, CornerPosition.TopRight, 8));

    // Every chip names the SAME intent key its fact is addressed by, so the deck row that toggles a readout and
    // the row that renders it are one key and a chip can never render a fact no verb governs.
    public static Seq<ChromeRow> Rows() =>
        Chips.Map(static chip => new ChromeRow(
            IntentKey: chip.Fact,
            Slot: ChromeSlot.Hud,
            Path: DevLoopSurfaces.BlockKey,
            Rank: chip.Rank,
            Visible: static _ => true,
            Content: new ChromeContent.Chip(chip.Corner, chip.Fact)))
            .Strict();

    // The fact values one HUD sample and one governor readout answer, so the chip stream is a projection of two
    // settled values rather than nine subscriptions — a chip that sampled its own instrument would render a
    // governor state no verdict ever produced.
    public static Seq<(string Fact, string Value)> Facts(HudSample hud, GovernorReadout quality) =>
        Seq(
            (DevLoopSurfaces.FrameFact, hud.FrameElapsed.ToString()),
            (DevLoopSurfaces.GpuFact, hud.GpuElapsed.ToString()),
            (DevLoopSurfaces.VramFact, hud.VramBytes.ToString(CultureInfo.InvariantCulture)),
            (DevLoopSurfaces.TrianglesFact, hud.Triangles.ToString(CultureInfo.InvariantCulture)),
            (DevLoopSurfaces.SolveFact, hud.SolveElapsed.ToString()),
            (GovernorReadout.TierFact, quality.Tier.Key),
            // An unbreached tier reads the TIGHTEST axis rather than a blank, because "nothing is breaching"
            // and "I have no idea what is closest" are different answers and only one of them is useful.
            (GovernorReadout.BreachFact, quality.Breach.Map(static axis => axis.Key).IfNone(quality.Tightest.Key)),
            (GovernorReadout.HeadroomFact, quality.Headroom.ToString("P0", CultureInfo.InvariantCulture)),
            (GovernorReadout.HistoryFact, quality.Recent.Count.ToString(CultureInfo.InvariantCulture)));
}
```

## [05]-[RESEARCH]

(none)
