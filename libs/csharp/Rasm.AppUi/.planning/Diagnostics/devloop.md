# [APPUI_DIAGNOSTICS_DEVLOOP]

Rasm.AppUi dev loop is the Debug-profile working surface: hot-reload knob rows and the manual-reload intent edge, the ProDiagnostics visual-tree/property/event/layout inspector under one attach-config row, the user-facing performance HUD sample feed, the flamegraph fold, the solve time-travel scrub, cross-machine replay-verify, the in-app REPL, and remote evidence ingestion. Every measure reads the settled receipt envelopes the `Diagnostics/evidence.md` timeline ingests — the loop mints no second meter, no second codec, and no second command-execution path.

## [01]-[INDEX]

- [02]-[DEV_LOOP]: Hot-reload knob rows; dispatcher starvation probe; HUD, flamegraph, scrub, REPL, ingest.
- [03]-[INSPECTOR]: The ProDiagnostics attach-config row; live property commits; control-snapshot lane.

## [02]-[DEV_LOOP]

- Owner: `DevLoop` — the Debug loop surface with the hot-reload knob rows, the manual-reload intent edge, the remote-evidence ingest edge, the performance-HUD sample feed, the flamegraph fold, the solve time-travel scrub, the cross-machine replay-verify, and the in-app REPL; `HudSample`, `FlameNode`, `SolveScrub`, `Repl` the user-facing debug owners.
- Entry: `DispatcherLag` carries an admitted timeout, `TimeProvider`, and cancellation token so a starved dispatcher fails through `DevLoopFault.DispatcherTimeout` instead of leaving the probe pending forever; `Reload` routes the three injected hot-reload effects; `Ingest` decodes and re-emits a canonical `ReceiptEnvelope` without changing the origin HLC stamp.
- Auto: the lag sink binds to `ReceiptSinkPort.Send` at composition under the `LagKind` row, so starvation evidence rides the same envelope stream the dashboards ingest; threshold evaluation stays with the health fold, so the probe carries zero literals; the `decode` column binds the AppHost envelope wire decode at composition so a companion node's receipt frames fold into the same envelope stream as local evidence with no second codec; `Reload` binds the three injected operations at composition under the master gate so the manual-reload intent is a command-table verb on Debug profiles and a structurally-absent route on Release closures where the injected source is stripped.
- Packages: HotAvalonia, Avalonia.Markup.Xaml.Loader (transitive floor, Debug pin), LanguageExt.Core, NodaTime, BCL inbox
- Growth: one knob row retunes the reload gate, one `ReloadIntent` case absorbs a new manual-reload verb, one probe row absorbs a new loop measure, one `HudSample` field absorbs a new HUD metric, and a new eval outcome is one `CommandReceipt` projection on the one deck route; zero new surface.
- Boundary: HotAvalonia is a Debug-gated build asset whose injected `UseHotReload`, `EnableHotReload`, `DisableHotReload`, and `TriggerHotReload` extensions on `AppBuilder`/`Application` are the only callable surface — the Release strip is driven by `HotAvaloniaExcludeReferences`, whose default list names `HotAvalonia`, `HotAvalonia.Core`, and `HotAvalonia.Fody` and adds `Avalonia.Markup.Xaml.Loader` when `HotAvaloniaIncludeXamlLoader` is false, while `HotAvaloniaProcessReferences` (default false) governs only whether referenced PROJECTS join the weave scope, and the explicit `Avalonia.Markup.Xaml.Loader` markup-loader pin with its transitive floor lands in the charter admissions so the Debug XAML-compile path resolves and the Release closure carries none of it — the markup loader is the HotAvalonia weaver's Debug-only re-patch dependency, never a managed `AvaloniaXamlLoader.Load` runtime-materialize surface DevLoop exposes, so `TriggerHotReload` re-patches compiled-XAML methods in place while a DevLoop-raised runtime `AvaloniaXamlLoader.Load` call is the rejected form whose structural fault is `Surfaces.RejectRuntimeXaml`; the manual-reload intent rides composition-bound delegates so DevLoop names no injected symbol directly and the deleted form is a DevLoop-internal reload bootstrap beside the injected extensions; the performance HUD is the `HudSample` feed — frame-elapsed, GPU-elapsed, VRAM bytes, triangle count from the viewport `FrameReceipt`, and the per-node solve elapsed from the Compute solve receipts fold into one HUD sample stream the overlay renders, so the HUD reads the same receipt envelopes the timeline ingests and a HUD-local meter is the deleted form, and an overlay-render failure recovers through the composition-bound `faults` route before the subscription edge's one terminal collapse so no failure is discarded; flamegraphs are the `FlameNode` fold — the per-node solve and per-pass render durations nest into one self-and-total tree the overlay flattens by depth so a profiling flamegraph is a fold over the existing receipt durations, never a second profiler; solve time-travel is the `SolveScrub` — each solve frame records its node id and state json keyed by ordinal so a user scrubs the solve history backward and forward and `Diff` surfaces the per-node state delta between two frames, the time-travel debugger over the journal the replay lane already records; cross-machine replay-verify is `ReplayVerify` — a journal replays through the one `ProofEngine.Replay` route and each receipt's payload digest compares to the baseline machine's digest so a cross-machine divergence surfaces as the exact journal index that diverged, the determinism check the headless lanes already prove extended across machines, with replay failure riding the one `IO` carrier to the caller — an `IO<Fin<T>>` public rail that leaves callers re-deciding failure below the boundary is the deleted form; the in-app REPL is `Repl` — a typed line parses into an intent key and payload and evaluates through the one `CommandProjections.Invoke` route so the REPL is the command table's interactive face and a second command-execution path is the rejected form, the eval result the same `CommandReceipt` every invocation route seals — a parallel eval-result union with an unproducible case is the deleted form; remote evidence ingestion decodes frames through the canonical AppHost `ReceiptEnvelope` JSON wire (`AppHostWireContext`) via a composition-bound decode delegate, so a devloop-local envelope codec is the rejected form and a frame decodes exactly as the envelope serialized; the HARFS remote-server knobs and the runtime timeout and hotkey knobs ride the same MSBuild gate as the master row and carry no managed surface.

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

public sealed record FlameNode(string Frame, Duration Self, Seq<FlameNode> Children) {
    public Duration Total => Self + Children.Fold(Duration.Zero, static (acc, child) => acc + child.Total);

    public Seq<(string Frame, Duration Total, int Depth)> Flatten(int depth = 0) =>
        Seq((Frame, Total, depth)) + Children.Bind(child => child.Flatten(depth + 1));

    public static FlameNode Of(HudSample hud, Seq<(string Node, Duration Elapsed)> solves, Seq<PassTiming> passes) =>
        new("frame", Duration.Zero,
            Seq(
                new FlameNode("cpu", hud.FrameElapsed > hud.SolveElapsed ? hud.FrameElapsed - hud.SolveElapsed : Duration.Zero,
                    solves.Map(static row => new FlameNode(row.Node, row.Elapsed, Seq<FlameNode>()))),
                new FlameNode("gpu", Duration.Zero, passes.Map(static row => new FlameNode(row.Pass, row.Resolved, Seq<FlameNode>())))));
}

public sealed record SolveFrame(int Ordinal, string NodeId, JsonElement State, Instant At);
public sealed record SolveDelta(string NodeId, JsonElement From, JsonElement To);

public sealed record SolveScrub(Seq<SolveFrame> Frames) {
    public Option<SolveFrame> At(int ordinal) => Frames.Find(frame => frame.Ordinal == ordinal);

    public SolveScrub Record(SolveFrame frame) => this with { Frames = Frames.Add(frame) };

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
    public const string LagKind = "dispatcher-lag";
    public const string HudKind = "perf-hud";

    // The marshal callback is pure — one TrySetResult write; the elapsed value re-enters the rail
    // through the gate, so the lag sink sequences on the one carrier and its failure reaches the caller.
    public static IO<Unit> DispatcherLag(SurfaceScheduler boundary, TimeProvider time, Duration timeout, CancellationToken cancellation, Func<Duration, IO<Unit>> sink) =>
        IO.lift(() => (Mark: time.GetTimestamp(), Gate: new TaskCompletionSource<Duration>(TaskCreationOptions.RunContinuationsAsynchronously)))
            .Bind(state => (boundary.Marshal(() => state.Gate.TrySetResult(Duration.FromTimeSpan(time.GetElapsedTime(state.Mark))))
                .Bind(_ => IO.liftAsync(async () => await state.Gate.Task
                    .WaitAsync(timeout.ToTimeSpan(), time, cancellation)
                    .ConfigureAwait(false))))
                | @catch<IO, Duration>(static _ => true, static error => IO.fail<Duration>(new DevLoopFault.DispatcherTimeout(error.Message)))))
            .Bind(sink);

    // The subscription is the caller's lifetime handle: disposing it detaches the overlay, so repeated
    // Hud runs never stack duplicate render callbacks on one sample feed. Synchronize serializes
    // concurrent sample emissions, and the Rx callback is the named terminal edge — recovery composes
    // before the one Run, so no failure is discarded.
    public static IO<IDisposable> Hud(IObservable<HudSample> samples, IScheduler scheduler, Func<HudSample, IO<Unit>> render, Func<Error, IO<Unit>> faults) =>
        IO.lift(() => samples.Synchronize().ObserveOn(scheduler).Subscribe(
            sample => ignore((render(sample) | @catch<IO, Unit>(static _ => true, error => faults(error))).As().Run()),
            error => ignore(faults(new DevLoopFault.Stream(error.Message)).Run())));

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

- Owner: `InspectorAttach` — the one ProDiagnostics attach row carrying the native `DevToolsOptions` policy object; the package option surface remains the complete configuration owner.
- Entry: `public static IO<Unit> Attach(Application app, InspectorAttach row)` — one Debug-composition call; the attach is the only imperative edge.
- Auto: `DevToolsOptions` carries the default `F12` gesture, `LaunchView : DevToolsViewKind`, `HotKeys : HotKeyConfiguration`, `ScreenshotHandler : IScreenshotHandler`, and `PropertyEditHandler : IDevToolsPropertyEditHandler` — one config row, every knob a field; `PropertyValueEditorService` owns live property commits from the inspector so an edit lands through the service, never an ad-hoc reflection write; `VisualTreeDebug` owns the layout/renderer overlays; `VisualExtensions.RenderTo(Control, Stream, double)` is the control-snapshot lane the screenshot handler composes — its stream feeds the same capture encode fold `proof.md` owns, so an inspector screenshot is a `CaptureRow` sibling, never a second pixel path.
- Packages: ProDiagnostics (Debug-gated, `PrivateAssets="all"`), LanguageExt.Core
- Growth: a new inspector knob is one `DevToolsOptions` field on the row; zero new surface.
- Boundary: ProDiagnostics is Debug-gated `PrivateAssets="all"` in the csproj `Dev Loop` group beside HotAvalonia and is absent from the Release surface — a Release-profile attach is structurally unrepresentable; the `ProDataGrid`/`ProCharts` siblings are NOT admitted; the inspector composes the SAME evidence spine as every loop measure — a property commit through `PropertyValueEditorService` seals an Edit-case `EvidenceReceipt` so inspector mutations are attributable on the timeline; both first-party alternates failed the admission gate (`Avalonia.Diagnostics` feed-dead at 11.3.x with no Avalonia-12 asset; the Accelerate DevTools pay-tiered, license-gate rejected) — the record stands, never re-proposed.

```csharp signature
public sealed record InspectorAttach(DevToolsOptions Options);

public static class Inspector {
    public static IO<Unit> Attach(Application app, InspectorAttach row) =>
        IO.lift(() => ignore(app.AttachDevTools(row.Options)));
}
```
