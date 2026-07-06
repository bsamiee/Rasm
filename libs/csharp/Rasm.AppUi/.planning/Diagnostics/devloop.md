# [APPUI_DIAGNOSTICS_DEVLOOP]

Rasm.AppUi dev loop is the Debug-profile working surface: hot-reload knob rows and the manual-reload intent edge, the ProDiagnostics visual-tree/property/event/layout inspector under one attach-config row, the user-facing performance HUD sample feed, the flamegraph fold, the solve time-travel scrub, cross-machine replay-verify, the in-app REPL, and remote evidence ingestion. Every measure reads the settled receipt envelopes the `Diagnostics/evidence.md` timeline ingests — the loop mints no second meter, no second codec, and no second command-execution path.

## [01]-[INDEX]

- [02]-[DEV_LOOP]: Hot-reload knob rows; dispatcher starvation probe; HUD, flamegraph, scrub, REPL, ingest.
- [03]-[INSPECTOR]: The ProDiagnostics attach-config row; live property commits; control-snapshot lane.

## [02]-[DEV_LOOP]

- Owner: `DevLoop` — the Debug loop surface with the hot-reload knob rows, the manual-reload intent edge, the remote-evidence ingest edge, the performance-HUD sample feed, the flamegraph fold, the solve time-travel scrub, the cross-machine replay-verify, and the in-app REPL; `HudSample`, `FlameNode`, `SolveScrub`, `Repl`, `ReplResult` the user-facing debug owners.
- Entry: `public static IO<Unit> DispatcherLag(SurfaceScheduler boundary, TimeProvider time, Func<Duration, IO<Unit>> sink)` — marshal round-trip lag into the composition-bound sink delegate; `public static IO<Unit> Reload(ReloadIntent intent, Func<IO<Unit>> trigger, Func<IO<Unit>> enable, Func<IO<Unit>> disable)` — the manual-reload verb routes the three injected `AvaloniaHotReloadExtensions` operations through composition-bound delegates so a palette-raised reload intent dispatches `TriggerHotReload`/`EnableHotReload`/`DisableHotReload` without DevLoop touching the injected surface; `public static IO<Unit> Ingest(ReceiptSinkPort sink, Func<ReadOnlyMemory<byte>, Fin<ReceiptEnvelope>> decode, ReadOnlyMemory<byte> frame)` — the composition-bound binary-wire `decode` column lifts a remote frame into a `ReceiptEnvelope` re-emitted through `sink.Emit`, preserving the origin node's HLC stamp so the correlation join reads each node's own clock.
- Auto: the lag sink binds to `ReceiptSinkPort.Send` at composition under the `LagKind` row, so starvation evidence rides the same envelope stream the dashboards ingest; threshold evaluation stays with the health fold, so the probe carries zero literals; the `decode` column binds the settled Persistence binary-wire decode at composition so a companion node's receipt frames fold into the same envelope stream as local evidence with no second codec; `Reload` binds the three injected operations at composition under the master gate so the manual-reload intent is a command-table verb on Debug profiles and a structurally-absent route on Release closures where the injected source is stripped.
- Packages: HotAvalonia, Avalonia.Markup.Xaml.Loader (transitive floor, Debug pin), LanguageExt.Core, NodaTime, BCL inbox
- Growth: one knob row retunes the reload gate, one `ReloadIntent` case absorbs a new manual-reload verb, one probe row absorbs a new loop measure, one `HudSample` field absorbs a new HUD metric, and one `ReplResult` case absorbs a new eval outcome; zero new surface.
- Boundary: HotAvalonia is a Debug-gated build asset whose injected `UseHotReload`, `EnableHotReload`, `DisableHotReload`, and `TriggerHotReload` extensions on `AppBuilder`/`Application` are the only callable surface — the master gate plus `HotAvaloniaProcessReferences` enabled strips `HotAvalonia.Core`, `HotAvalonia.Extensions`, and the `HotAvalonia.Fody` weaver from Release closures while `HotAvaloniaExcludeReferences` names the exact reference list the strip removes, and the explicit `Avalonia.Markup.Xaml.Loader` markup-loader pin with its transitive floor lands in the charter admissions so the Debug XAML-compile path resolves and the Release closure carries none of it — the markup loader is the HotAvalonia weaver's Debug-only re-patch dependency, never a managed `AvaloniaXamlLoader.Load` runtime-materialize surface DevLoop exposes, so `TriggerHotReload` re-patches compiled-XAML methods in place while a DevLoop-raised runtime `AvaloniaXamlLoader.Load` call is the rejected form whose structural fault is `Surfaces.RejectRuntimeXaml`; the manual-reload intent rides composition-bound delegates so DevLoop names no injected symbol directly and the deleted form is a DevLoop-internal reload bootstrap beside the injected extensions; the performance HUD is the `HudSample` feed — frame-elapsed, GPU-elapsed, VRAM bytes, triangle count from the viewport `FrameReceipt`, and the per-node solve elapsed from the Compute solve receipts fold into one HUD sample stream the overlay renders, so the HUD reads the same receipt envelopes the timeline ingests and a HUD-local meter is the deleted form; flamegraphs are the `FlameNode` fold — the per-node solve and per-pass render durations nest into one self-and-total tree the overlay flattens by depth so a profiling flamegraph is a fold over the existing receipt durations, never a second profiler; solve time-travel is the `SolveScrub` — each solve frame records its node id and state json keyed by ordinal so a user scrubs the solve history backward and forward and `Diff` surfaces the per-node state delta between two frames, the time-travel debugger over the journal the replay lane already records; cross-machine replay-verify is `ReplayVerify` — a journal replays through the one `ProofEngine.Replay` route and each receipt's payload digest compares to the baseline machine's digest so a cross-machine divergence surfaces as the exact journal index that diverged, the determinism check the headless lanes already prove extended across machines; the in-app REPL is `Repl` — a typed line parses into an intent key and payload and evaluates through the one `CommandProjections.Invoke` route so the REPL is the command table's interactive face and a second command-execution path is the rejected form, the eval result the typed `ReplResult` union; remote evidence ingestion packs envelopes through the settled Persistence binary wire row via a composition-bound codec delegate, so a second binary codec here is the rejected form; the HARFS remote-server knobs and the runtime timeout and hotkey knobs ride the same MSBuild gate as the master row and carry no managed surface.

```csharp signature
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
    int PerNodeCount);

public sealed record FlameNode(string Frame, Duration Self, Seq<FlameNode> Children) {
    public Duration Total => Self + Children.Fold(Duration.Zero, static (acc, child) => acc + child.Total);

    public Seq<(string Frame, Duration Total, int Depth)> Flatten(int depth = 0) =>
        Seq((Frame, Total, depth)) + Children.Bind(child => child.Flatten(depth + 1));
}

public sealed record SolveFrame(int Ordinal, string NodeId, JsonElement State, Instant At);

public sealed record SolveScrub(Seq<SolveFrame> Frames) {
    public Option<SolveFrame> At(int ordinal) => Frames.Find(frame => frame.Ordinal == ordinal);

    public SolveScrub Record(SolveFrame frame) => this with { Frames = Frames.Add(frame) };

    public Option<(SolveFrame From, SolveFrame To)> Diff(int from, int to) =>
        (At(from), At(to)) switch {
            ({ IsSome: true, Case: SolveFrame a }, { IsSome: true, Case: SolveFrame b }) => Some((a, b)),
            _ => None,
        };
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ReplResult {
    private ReplResult() { }
    public sealed record Value(JsonElement Json) : ReplResult;
    public sealed record Receipt(CommandReceipt Command) : ReplResult;
    public sealed record Failed(string Detail) : ReplResult;
}

public sealed record Repl(CommandDeck Deck, Func<string, Fin<(string Key, JsonElement Payload)>> Parse) {
    public IO<ReplResult> Eval(string line) =>
        Parse(line).Match(
            Succ: parsed => Deck.Invoke(parsed.Key, parsed.Payload).Map(static receipt => (ReplResult)new ReplResult.Receipt(receipt)),
            Fail: error => IO.pure<ReplResult>(new ReplResult.Failed(error.Message)));
}

public static class DevLoop {
    public const string LagKind = "dispatcher-lag";
    public const string HudKind = "perf-hud";

    public static IO<Unit> DispatcherLag(SurfaceScheduler boundary, TimeProvider time, Func<Duration, IO<Unit>> sink) =>
        IO.lift(time.GetTimestamp)
            .Bind(mark => boundary.Marshal(() =>
                ignore(sink(Duration.FromTimeSpan(time.GetElapsedTime(mark))).Run())));

    // The subscription is the caller's lifetime handle: disposing it detaches the overlay, so repeated
    // Hud runs never stack duplicate render callbacks on one sample feed.
    public static IO<IDisposable> Hud(IObservable<HudSample> samples, Func<HudSample, IO<Unit>> render) =>
        IO.lift(() => samples.Subscribe(sample => ignore(render(sample).Run())));

    // A length mismatch IS a divergence: indices past the shorter side report as mismatches, so a
    // dropped or extra tail receipt never hides behind pairwise truncation.
    public static IO<Fin<Seq<int>>> ReplayVerify(Seq<(string Key, JsonElement Payload)> journal, CommandDeck deck, Seq<string> baseline) =>
        ProofEngine.Replay(deck, journal)
            .Map(receipts => toSeq(receipts) switch {
                var replayed => Fin.Succ(toSeq(Enumerable.Range(0, Math.Max(replayed.Count, baseline.Count))
                    .Where(index => index >= replayed.Count || index >= baseline.Count || replayed[index].PayloadDigest != baseline[index]))),
            });

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

| [INDEX] | [KNOB_ROW]                   | [VALUE]            | [ROLE]                                  |
| :-----: | :--------------------------- | :----------------- | :-------------------------------------- |
|  [01]   | HotAvalonia                  | Debug default      | master gate                             |
|  [02]   | HotAvaloniaIncludeExtensions | exe default        | injects the UseHotReload source         |
|  [03]   | HotAvaloniaProcessReferences | enabled            | Release closure strip                   |
|  [04]   | HotAvaloniaExcludeReferences | weaver + core list | reference list the strip removes        |
|  [05]   | markup-loader pin            | transitive floor   | `Avalonia.Markup.Xaml.Loader` Debug pin |
|  [06]   | HotAvaloniaRemote            | non-desktop opt-in | remote reload route                     |
|  [07]   | HotAvaloniaTimeout           | runtime default    | reload timeout window                   |
|  [08]   | HotAvaloniaHotkey            | runtime default    | manual-reload key chord                 |
|  [09]   | HarfsAddress / HarfsPort     | remote endpoint    | HARFS file-server endpoint              |

## [03]-[INSPECTOR]

- Owner: `InspectorAttach` — the one ProDiagnostics attach-config row the Debug composition binds; the inspector itself is the package's `DevTools` surface under the ORIGINAL `Avalonia.Diagnostics` assembly and namespace, so `AttachDevTools(DevToolsOptions)` binds unchanged.
- Entry: `public static IO<Unit> Attach(Application app, InspectorAttach row)` — one Debug-composition call; the row is data, the attach is the only imperative edge.
- Auto: `DevToolsOptions` carries the default `F12` gesture, `LaunchView : DevToolsViewKind`, `HotKeys : HotKeyConfiguration`, `ScreenshotHandler : IScreenshotHandler`, and `PropertyEditHandler : IDevToolsPropertyEditHandler` — one config row, every knob a field; `PropertyValueEditorService` owns live property commits from the inspector so an edit lands through the service, never an ad-hoc reflection write; `VisualTreeDebug` owns the layout/renderer overlays; `VisualExtensions.RenderTo(Control, Stream, double)` is the control-snapshot lane the screenshot handler composes — its stream feeds the same capture encode fold `proof.md` owns, so an inspector screenshot is a `CaptureRow` sibling, never a second pixel path.
- Packages: ProDiagnostics (Debug-gated, `PrivateAssets="all"`), LanguageExt.Core
- Growth: a new inspector knob is one `DevToolsOptions` field on the row; zero new surface.
- Boundary: ProDiagnostics is Debug-gated `PrivateAssets="all"` in the csproj `Dev Loop` group beside HotAvalonia and is absent from the Release surface — a Release-profile attach is structurally unrepresentable; the `ProDataGrid`/`ProCharts` siblings are NOT admitted; the inspector composes the SAME evidence spine as every loop measure — a property commit through `PropertyValueEditorService` seals an Edit-case `EvidenceReceipt` so inspector mutations are attributable on the timeline; both first-party alternates failed the admission gate (`Avalonia.Diagnostics` feed-dead at 11.3.x with no Avalonia-12 asset; the Accelerate DevTools pay-tiered, license-gate rejected) — the record stands, never re-proposed.

```csharp signature
public sealed record InspectorAttach(
    KeyGesture Gesture,
    DevToolsViewKind LaunchView,
    Option<IScreenshotHandler> Screenshot,
    Option<IDevToolsPropertyEditHandler> PropertyEdit) {

    public DevToolsOptions ToOptions() => new() {
        Gesture = Gesture,
        LaunchView = LaunchView,
        ScreenshotHandler = Screenshot.Case as IScreenshotHandler,
        PropertyEditHandler = PropertyEdit.Case as IDevToolsPropertyEditHandler,
    };
}

public static class Inspector {
    public static IO<Unit> Attach(Application app, InspectorAttach row) =>
        IO.lift(() => ignore(app.AttachDevTools(row.ToOptions())));
}
```
