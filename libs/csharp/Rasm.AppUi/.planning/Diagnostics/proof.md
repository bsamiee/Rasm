# [APPUI_DIAGNOSTICS_PROOF]

Rasm.AppUi proof derives capture, check, variant-density, benchmark, and replay cells from live catalogs. Capture rows prove pixels by content hash, benchmark rows gate headless frame cost against held `BenchmarkReceipt` claims, command journals replay under virtual time, and CsCheck with Verify seals the matrix. This page owns the row families, derivation engine, benchmark lane, render-hash law, and typed `ProofFault` rail.

## [01]-[INDEX]

- [02]-[CAPTURE_LANES]: Host-agnostic frame capture rows; render-hash regression proof.
- [03]-[HEADLESS_DERIVATION]: Catalog-derived proof matrix and benchmark lanes; deterministic command-journal replay.
- [04]-[PROOF_LAW]: Law-matrix fence — FrameHash equality, deterministic capture, replay determinism, the instrument fold, the frame-bench gate, the bundle-tree pin.

## [02]-[CAPTURE_LANES]

- Owner: `ProofFault` — the typed proof rail; `CaptureRow` — the admitted per-surface capture row carrying scale, gamut, and tick policy; `Captures` — the shot-and-regression surface.
- Entry: `public static IO<RenderReceipt> Shot(VisualRuntime runtime, CaptureRow row)` — `IO` rail through the settled encode fold with one PNG artifact and one render receipt per shot.
- Auto: `CaptureRow.Key` is the complete artifact-cell identity supplied by `RenderHashLane.Cell`, so `Captures.Shot` prefixes it once and never re-appends scale or gamut; the `Scale` and `Gamut` columns enter the grab delegate together, pinning render scaling through `SetRenderScaling` and color policy through the exact `VisualCodec.ColorPolicy` row; the `Ticks` column enters `ProofEngine.Advance`, the one forced-frame operation capture and benchmark lanes share, so a single-frame baseline pins `Ticks: 1` and an animation-settled or multi-frame capture pins its own count as data and never wall time; the receipt's `FrameHash` rides the suite content-hash identity row (the kernel `ContentHash.Of` delegate the capture runtime binds).
- Receipt: a regression divergence is a typed `ProofFault.HashDiverged` whose `Code` derives through `AppUiFaultBand.Proof`; a bare `Error.New` on this rail is the deleted form.
- Packages: SkiaSharp, Avalonia.Headless, Avalonia.Skia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one capture row absorbs a new surface lane; one `Scale` value on a row absorbs a new DPI baseline; one `ProofFault` case is one `detail` ordinal under the 6700 row; zero new surface.
- Boundary: grab delegates bind at composition per surface row and no capture member is named outside its own row — the headless lane rides the `WriteableBitmap? CaptureRenderedFrame(this TopLevel)` and `WriteableBitmap? GetLastRenderedFrame(this TopLevel)` extensions whose `WriteableBitmap` pixels enter the hash fold through `Lock()` over the `ILockedFramebuffer` (`Address`, `RowBytes`, `Size`, `Format`), an un-shown top-level returning a null frame folds to an absent grab rather than a throw, with `UseHeadlessDrawing` false selecting the Skia backend on every hash lane and `SetRenderScaling(this TopLevel, double)` pinning the device scale before the grab (it throws `ArgumentOutOfRangeException` on a non-positive scale, so the row `Scale` stays positive) so the render-hash is scale-attributable; the custom-visual lane is a RECEIPT lane, not a grab lane — `CustomVisual.Materialize(VisualRuntime, CustomVisualData, SKImageInfo, ColorSpaceAxis)` already returns `IO<Fin<RenderReceipt>>` with the encode and the `FrameHash` inside, so the custom-tile cell binds that rail straight into `Regression` keyed by `key@scale×gamut` under its `ColorSpaceAxis` cell (the keyed projection of `VisualCodec.ColorPolicy`), a second `CaptureRow.Grab` contract over the same cell is the deleted duplicate, a wide-gamut custom tile hashes its float or ICC-tagged pixels and never a quantized sRGB shadow, and the render-hash regression attributes a custom-tile pixel drift to the exact kind, scale, and gamut cell; the rhino lane rides the settled host viewport capture port; the desktop in-tree lane renders through `RenderTargetBitmap.Render(Visual)` with `CopyPixels(PixelRect, nint, int, int)` as its pixel projection, or evaluates a live visual onto a leased Skia canvas through `DrawingContextHelper.RenderAsync` where the in-tree row already holds a render lease so the capture composes the visual into the encode fold without a second offscreen surface; `ForceRenderTimerTick` is the only frame-advance verb on the deterministic lane — a debounce or animation that fails under forced ticks has smuggled wall time, and the tick count is a row column so a multi-frame capture is data; `Regression` compares `FrameHash` values from the settled receipt family, so a per-spec screenshot helper is the deleted form and a second baseline store beside the blob lane is the rejected form.

```csharp signature
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProofFault : Expected, IValidationError<ProofFault> {
    private ProofFault(string detail, int code) : base(detail, code) { }

    public static ProofFault Create(string message) => new Text(message);

    public sealed record Text(string Detail)
        : ProofFault(Detail, AppUiFaultBand.Proof.Code(0));
    public sealed record HashDiverged(string Cell, string Actual, string Baseline)
        : ProofFault($"proof/render-hash: {Cell} diverged", AppUiFaultBand.Proof.Code(1));
    public sealed record ReplayDiverged(int JournalIndex)
        : ProofFault($"proof/replay: journal index {JournalIndex} diverged", AppUiFaultBand.Proof.Code(2));
    public sealed record GrabAbsent(string Cell)
        : ProofFault($"proof/capture: {Cell} produced no frame", AppUiFaultBand.Proof.Code(3));
    public sealed record SessionUnavailable(string Detail)
        : ProofFault($"proof/session: {Detail}", AppUiFaultBand.Proof.Code(4));
    public sealed record CostRegressed(string Pass, string Baseline, string Actual)
        : ProofFault($"proof/frame-cost: {Pass} regressed {Baseline} -> {Actual}", AppUiFaultBand.Proof.Code(5));
    public sealed record BaselineAbsent(string Pass)
        : ProofFault($"proof/frame-cost: {Pass} has no admitted baseline", AppUiFaultBand.Proof.Code(6));
    public sealed record ReplayShape(int First, int Second)
        : ProofFault($"proof/replay: receipt counts diverged {First} != {Second}", AppUiFaultBand.Proof.Code(7));
    public sealed record BudgetInvalid(string ScreenId, int Samples, int Ticks)
        : ProofFault($"proof/frame-budget: a screen id and positive samples and ticks are required, received {ScreenId}:{Samples}x{Ticks}", AppUiFaultBand.Proof.Code(8));
    public sealed record CaptureInvalid(string Cell, double Scale, int Ticks)
        : ProofFault($"proof/capture-budget: a cell, gamut, grab, positive scale, and positive tick count are required, received {Cell}:{Scale}x{Ticks}", AppUiFaultBand.Proof.Code(9));
}

public sealed record CaptureRow {
    private CaptureRow(
        string key,
        double scale,
        VisualCodec.ColorPolicy gamut,
        int ticks,
        Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> grab) =>
        (Key, Scale, Gamut, Ticks, Grab) = (key, scale, gamut, ticks, grab);

    public string Key { get; }
    public double Scale { get; }
    public VisualCodec.ColorPolicy Gamut { get; }
    public int Ticks { get; }
    public Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> Grab { get; }

    public static Fin<CaptureRow> Of(
        string key,
        double scale,
        VisualCodec.ColorPolicy gamut,
        int ticks,
        Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> grab) =>
        !string.IsNullOrWhiteSpace(key) && scale > 0d && gamut is not null && ticks > 0 && grab is not null
            ? Fin.Succ(new CaptureRow(key, scale, gamut, ticks, grab))
            : Fin.Fail<CaptureRow>(new ProofFault.CaptureInvalid(key, scale, ticks));

    public IO<SKImage> Shoot() => Grab(Scale, Gamut, () => ProofEngine.Advance(Ticks));
}

public static class Captures {
    public const string Kind = "capture";

    public static IO<RenderReceipt> Shot(VisualRuntime runtime, CaptureRow row) =>
        row.Shoot().Bind(image =>
            VisualCodec.Encode(runtime, image, VisualCodec.Png, Kind, $"captures/{row.Key}.png"));

    // RenderHashLane.Cell is the custom-twin key@variant-density lane identity,
    // never the family Kind constant — same-family failures stay attributable to their exact cell.
    public static Fin<RenderReceipt> Regression(string cell, RenderReceipt actual, string baseline) =>
        actual.FrameHash == baseline
            ? Fin.Succ(actual)
            : Fin.Fail<RenderReceipt>(new ProofFault.HashDiverged(cell, actual.FrameHash, baseline));
}
```

## [03]-[HEADLESS_DERIVATION]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `ProofCheck` — the ten-row check vocabulary; `ProofSpec` — the derived spec row; `ProofEngine` — the derivation and replay surface; `RenderHashLane` — the `key@scale×gamut` render-hash cell; `BenchLane` — the catalog-derived frame-benchmark cell.
- Cases: activation, render-hash, focus-walk, variant-sweep, density-sweep, disposal-leak, pointer-walk, drag-drop, contrast-audit, frame-cost — the two input-proof rows drive the headless synthetic-input verbs, the contrast-audit row sweeps the `Shell/accessibility.md` WCAG luminance gate over every variant-density cell so an inaccessible pairing fails the matrix like a pixel drift, and the frame-cost row proves per-pass render cost against the `FrameBudget` so a pass that doubles its cost fails the matrix like a pass that changes its pixels.
- Entry: `public static Seq<ProofSpec> Derive(ScreenCatalog catalog, Seq<(ThemeVariantRow Variant, DensityRow Density)> grid, Func<ScreenCatalogRow, ProofCheck, bool> applies, Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe)` — each headless row crosses only the checks its composition-bound applicability relation admits, then spans every variant-density cell. `ProofEngine.Bench` admits positive sample and tick budgets before constructing any benchmark lane.
- Auto: derived specs execute on the shared `HeadlessUnitTestSession` through `GetOrStartForAssembly` once per assembly and `Dispatch` per spec, so every spec runs on the one UI thread without a per-spec session boot, and `[AvaloniaFact]` dispatch under the xunit.v3 MTP runner rides the same session; `FakeTimeProvider` time travel fills the headless row's virtual-time slot; `Replay` drives the journal through the one remote-invocation route on the frozen deck, so journal replay, deep links, and interactive execution seal the same receipt family; the snapshot store rehydrates screen state before the first journal entry, so replay is deterministic end to end; the pointer-walk and drag-drop checks drive synthetic input on the session top-level through `HeadlessWindowExtensions.MouseDown(this TopLevel, Point, MouseButton, RawInputModifiers)`/`MouseMove(this TopLevel, Point, RawInputModifiers)`/`MouseUp(this TopLevel, Point, MouseButton, RawInputModifiers)`/`MouseWheel(this TopLevel, Point, Vector, RawInputModifiers)` between `ForceRenderTimerTick` advances, the drag-drop check driving `DragDrop(this TopLevel, Point, RawDragEventType, IDataTransfer, DragDropEffects, RawInputModifiers)` (void return) in the load-bearing `DragEnter` → `DragOver` → `Drop` sequence (`RawDragEventType` from `Avalonia.Input.Raw`) because a `DragOver` without a prior `DragEnter` seeds no drop context and raises no routed handler, the resulting effect read from `DragEventArgs.DragEffects` inside the handler and never a return value, so a pointer-interaction or drop-target proof is a deterministic frame sequence and never wall-time hover timing; benchmark lanes derive from the same `HeadlessLane` the proof matrix crosses — `ProofEngine.Bench` traverses one `BenchLane.Of` admission per headless row, so a screen added to the catalog gains its frame benchmark with zero roster edit, invalid budgets refuse as `ProofFault.BudgetInvalid`, and `FrameBench` consumes admitted values without clamping.
- Receipt: every executed spec seals its `EvidenceReceipt` through the `Diagnostics/evidence.md` union — disposal-leak audits ride the Disposal case and render checks ride the Render case.
- Packages: Avalonia.Headless, Avalonia.Headless.XUnit, Avalonia.Skia, Verify.XunitV3, CsCheck (testkit), Microsoft.Extensions.TimeProvider.Testing (FakeTimeProvider), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one check row sweeps every headless screen, one grid cell sweeps every check, one `RenderHashLane` cell sweeps every key×scale×gamut combination, and one `BenchLane` cell sweeps every headless row at its sample and tick budget; zero new surface.
- Boundary: the derivation engine deletes hand-written per-screen smoke specs — a bespoke screen spec beside the engine is the named defect; the engine owns execution and capture while sibling audit folds declare their own row shapes over it; the render-hash and capture lanes run on the Skia render-proof builder the surface-hosts headless row binds (`UseSkia` then `UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })` then `SetupWithoutStarting`) because `HeadlessUnitTestSession.StartNew` force-applies `UseHeadlessDrawing = true` and never `UseSkia`, so a frame captured under the session alone is the stub-drawing form, while the activation, focus-walk, pointer-walk, drag-drop, and disposal-leak checks ride the shared `HeadlessUnitTestSession` where stub drawing is acceptable; host-bound screens exit the matrix structurally through the catalog's headless lane, never through skipped specs.

```csharp signature
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProofCheck {
    public static readonly ProofCheck Activation = new("activation");
    public static readonly ProofCheck RenderHash = new("render-hash");
    public static readonly ProofCheck FocusWalk = new("focus-walk");
    public static readonly ProofCheck VariantSweep = new("variant-sweep");
    public static readonly ProofCheck DensitySweep = new("density-sweep");
    public static readonly ProofCheck DisposalLeak = new("disposal-leak");
    public static readonly ProofCheck PointerWalk = new("pointer-walk");
    public static readonly ProofCheck DragDrop = new("drag-drop");
    public static readonly ProofCheck ContrastAudit = new("contrast-audit");
    public static readonly ProofCheck FrameCost = new("frame-cost");
}

public sealed record ProofSpec(
    string ScreenId,
    ProofCheck Check,
    ThemeVariantRow Variant,
    DensityRow Density,
    Func<IO<EvidenceReceipt>> Run);

// One frame-benchmark cell per headless catalog row: samples and ticks are lane data, so a
// multi-frame or animation-settled benchmark pins its budget as columns and never wall time.
public sealed record BenchLane {
    private BenchLane(string screenId, int samples, int ticks) =>
        (ScreenId, Samples, Ticks) = (screenId, samples, ticks);

    public string ScreenId { get; }
    public int Samples { get; }
    public int Ticks { get; }
    public string Case => $"{ScreenId}@{Samples}x{Ticks}";

    public static Fin<BenchLane> Of(string screenId, int samples, int ticks) =>
        !string.IsNullOrWhiteSpace(screenId) && samples > 0 && ticks > 0
            ? Fin.Succ(new BenchLane(screenId, samples, ticks))
            : Fin.Fail<BenchLane>(new ProofFault.BudgetInvalid(screenId, samples, ticks));
}

public static class ProofEngine {
    public static Fin<Seq<BenchLane>> Bench(ScreenCatalog catalog, int samples, int ticks) =>
        catalog.HeadlessLane.TraverseM(row => BenchLane.Of(row.Id, samples, ticks)).As();

    internal static IO<Unit> Advance(int ticks) =>
        IO.lift(() => AvaloniaHeadlessPlatform.ForceRenderTimerTick(ticks));

    public static Seq<ProofSpec> Derive(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, bool> applies,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        catalog.HeadlessLane.Bind(row =>
            grid.Bind(cell =>
                toSeq(ProofCheck.Items).Filter(check => applies(row, check)).Map(check =>
                    new ProofSpec(row.Id, check, cell.Variant, cell.Density, probe(row, check, cell.Variant, cell.Density)))));

    // RunAsync returns the value and THROWS the typed Error on failure; liftAsync re-admits that throw
    // onto the IO error channel, so the dispatcher hop preserves the ProofFault-coded rail end to end.
    public static IO<EvidenceReceipt> Dispatch(ProofSpec spec) =>
        IO.liftAsync(async () => await HeadlessUnitTestSession
            .GetOrStartForAssembly(typeof(ProofEngine).Assembly)
            .Dispatch(() => spec.Run().RunAsync().AsTask(), CancellationToken.None)
            .ConfigureAwait(false));

    public static IO<Seq<CommandReceipt>> Replay(
        CommandDeck deck,
        Seq<(string Key, JsonElement Payload)> journal,
        Func<IO<Unit>> restore) =>
        restore().Bind(_ => journal.TraverseM(entry => deck.Invoke(entry.Key, entry.Payload)).As());
}
```

## [04]-[PROOF_LAW]

- Owner: `ProofLaw` — the law-matrix fence surface composing `ProofEngine` with CsCheck property generators, `Verify.XunitV3` FrameHash equality, the `MetricCollector<T>` instrument lane, the frame-bench gate minting estate `BenchmarkReceipt` evidence, the bundle-tree pin, and the `VerifyChecks`/`DanglingSnapshots` suite-hygiene gates.
- Entry: `public static IO<Seq<EvidenceReceipt>> ProofMatrix(...)` — the one entrypoint that owns the singular-cell and full-matrix run by input shape so a per-spec screenshot helper is the deleted form.
- Auto: `RenderHashGrid` generates cells from the live headless catalog crossed with admitted scale and `VisualCodec.ColorPolicy` data, so a new screen or gamut expands proof without a named roster edit; `ProofLaw.FrameHashEquality` seals one generated cell through `Captures.Shot` then `Verifier.Verify`; `ProofLaw.ReplayDeterminism` restores the same snapshot before each journal run, resets virtual time, rejects unequal receipt counts before pairing, and verifies the complete digest sequence; `ProofLaw.FrameCost` requires a baseline for every pass and compares it against both variance and `FrameBudget`, so a missing baseline cannot bless a new pass implicitly; `ProofLaw.InstrumentFold` receives the composition `LevelCells`, mounts contributions whose level readers close over that instance, and passes the same instance to `EvidenceFan.Fan`, so writes and observable-gauge reads share one cell family; `ProofLaw.FrameBench` samples a `BenchLane` between forced ticks, folds the sorted elapsed distribution to the true median — the central value on an odd count, the two central values' mean on an even one — and nearest-rank p95 — admitted positive `Samples` keeps the distribution non-empty, so the rank index is total without a bounds guard — beside the precise allocation delta, mints one corpus-bearing Unjudged `BenchmarkReceipt` under `HostEvidence.Current()`, composes `BenchmarkGate.Gate` over the held claim and sink, and collapses its verdict onto the public `IO` rail; `ProofLaw.Divergence` buckets the fresh-versus-held median ratio under `Buckets.DivergenceRatio` so regression magnitude reads on the shared advice axis; `ProofLaw.BundleShape` pins the exported support archive as two goldens — the zip entry roster and the extracted tree.
- Packages: Verify.XunitV3, CsCheck, Avalonia.Headless, Microsoft.Extensions.Diagnostics.Testing, Rasm.AppHost (project, seam types), NodaTime, LanguageExt.Core
- Growth: one lane cell absorbs a new golden; one benchmark claim is one held `BenchmarkReceipt` value; zero new surface.
- Boundary: the `RenderHashGrid` FrameHash golden bytes are the C#-host-validated leg of the content-addressed kernel-hasher-keyed ONE_WIRE_FIXTURE_CORPUS — the render-hash lane is the host golden producer the cross-runtime consumers read, never a second golden store; the headless capture lanes are the parity oracle for every `[V6]` fence repair — a repaired fence proves itself here before the campaign closes; gamut cells key by `ColorPolicy` rows (the `Render/capture.md` one gamut/transfer family); the proof fence is a terminal edge with the CATALOG-TRUE collapse law — `IO<A>.Run()` returns `A` and `RunAsync()` returns `ValueTask<A>`, both THROWING the typed `Error` on failure (only `Eff.Run` lands `Fin`) — so a failing disposition composes BEFORE the terminal: the awaited `RunAsync` throw carries the registry-coded `ProofFault` the runner reports loudly, a property that must survive failure composes `| @catch` to its verdict value before its one `Run()`, and a `ThrowIfFail`/`IfFail` shim applied to the terminal result is the phantom spelling that does not compile; the frame-bench lane composes the AppHost benchmark rail as settled vocabulary — `BenchmarkReceipt`, `HostEvidence`, `BenchmarkGate.Gate`, and `GatePolicy.Canonical` mint and judge, the held claim arrives as a value off the Persistence reuse index, and the judged receipt fans through the sink under the AppHost benchmark kind, so the dashboards benchmark layout ingests it off the HLC-ordered envelope stream and an AppUi-local benchmark receipt family is the deleted form; `VerifyZip`/`VerifyDirectory` pin support-bundle roster and tree completeness, while content-hash manifest proof remains blocked until AppHost `SupportManifest.Entry` carries its post-redaction key.

```csharp signature
public readonly record struct RenderHashLane(string Key, double Scale, VisualCodec.ColorPolicy Gamut, int Ticks) {
    public string Cell => $"{Key}@{Scale}x{Gamut.Key}";

    public Fin<CaptureRow> Row(Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> grab) =>
        CaptureRow.Of(Cell, Scale, Gamut, Ticks, grab);
}

public static class ProofLaw {
    public static Seq<RenderHashLane> RenderHashGrid(
        ScreenCatalog catalog,
        Seq<double> scales,
        Seq<VisualCodec.ColorPolicy> gamuts,
        Func<ScreenCatalogRow, bool> captureable,
        int ticks) =>
        catalog.HeadlessLane
            .Filter(captureable)
            .Bind(row => scales.Bind(scale => gamuts.Map(gamut => new RenderHashLane(row.Id, scale, gamut, ticks))));

    public static async Task SuiteHygiene() {
        await VerifyChecks.Run();
        DanglingSnapshots.Run();
    }

    // IO.RunAsync returns the value and throws the typed Error — the runner's loud failure IS the
    // ProofFault-coded throw; no Fin shim exists on the IO terminal.
    public static async Task FrameHashEquality(
        VisualRuntime runtime,
        RenderHashLane lane,
        Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> grab) {
        RenderReceipt receipt = await lane.Row(grab)
            .Match(Succ: row => Captures.Shot(runtime, row), Fail: IO.fail<RenderReceipt>)
            .RunAsync();
        await Verifier.Verify(new { lane.Cell, receipt.FrameHash, receipt.ColorSpace })
            .UniqueForTargetFramework()
            .UseTextForParameters(lane.Cell);
    }

    public static Gen<RenderHashLane> LaneGen(Seq<RenderHashLane> lanes) => Gen.OneOfConst([.. lanes]);

    // DeterministicCapture composes the whole double-shot as ONE IO<bool> with @catch recovery to false, collapsed by a
    // single terminal Run() — a mid-property Fin shim over Run() is a phantom spelling.
    public static void DeterministicCapture(
        VisualRuntime runtime,
        Seq<RenderHashLane> lanes,
        Func<double, VisualCodec.ColorPolicy, Func<IO<Unit>>, IO<SKImage>> grab) =>
        LaneGen(lanes).Sample(lane =>
            (lane.Row(grab).Match(
                Succ: row => Captures.Shot(runtime, row)
                    .Bind(first => Captures.Shot(runtime, row).Map(second => first.FrameHash == second.FrameHash)),
                Fail: IO.fail<bool>)
                | @catch<IO, bool>(static _ => true, static _ => IO.pure(false))).As().Run());

    public static IO<Seq<EvidenceReceipt>> ProofMatrix(ScreenCatalog catalog, Seq<(ThemeVariantRow Variant, DensityRow Density)> grid, Func<ScreenCatalogRow, ProofCheck, bool> applies, Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        ProofEngine.Derive(catalog, grid, applies, probe).TraverseM(ProofEngine.Dispatch).As();

    // One composed IO: both replays, both virtual-time resets, and the shape gate ride the rail; the
    // single terminal RunAsync throws the ProofFault.ReplayShape-coded Error on divergence.
    public static async Task ReplayDeterminism(
        CommandDeck deck,
        Seq<(string Key, JsonElement Payload)> journal,
        Func<IO<Unit>> restore,
        FakeTimeProvider time) {
        Seq<(string First, string Second)> digests = await (
            from _first in IO.lift(() => { time.SetUtcNow(DateTimeOffset.UnixEpoch); return unit; })
            from first in ProofEngine.Replay(deck, journal, restore)
            from _second in IO.lift(() => { time.SetUtcNow(DateTimeOffset.UnixEpoch); return unit; })
            from second in ProofEngine.Replay(deck, journal, restore)
            from pairs in first.Count == second.Count
                ? IO.pure(first.Map(static r => r.PayloadDigest).Zip(second.Map(static r => r.PayloadDigest)).ToSeq())
                : IO.fail<Seq<(string, string)>>(new ProofFault.ReplayShape(first.Count, second.Count))
            select pairs).RunAsync();
        await Verifier.Verify(digests);
    }

    // Instrument lane: the mount precedes the collector, which binds the already-published
    // Instrument<long> pulled from the mounted set — no measurement rides late-instrument observation
    // semantics; the factory scope isolates parallel proof cells, and level rows assert through their
    // cell value instead.
    public static long InstrumentFold(
        IMeterFactory factory, string version, string schemaUrl, CorrelationId root,
        LevelCells cells, Seq<TelemetryContributorPort> contributions,
        string instrument, Seq<ReceiptEnvelope> envelopes) {
        InstrumentSet set = AppUiTelemetry.Mount(factory, version, schemaUrl, root, contributions);
        ReceiptFan fan = EvidenceFan.Fan(set, cells);
        using MetricCollector<long> collector = new((Instrument<long>)set.ByName[instrument]);
        ignore(envelopes.Iter(envelope => EvidenceFan.Project(fan, envelope)));
        return collector.GetMeasurementSnapshot().Sum(static measurement => measurement.Value);
    }

    public const string BenchSuite = "rasm.appui.frame";

    // Frame-bench mint: N forced-tick frame probes on the deterministic lane fold to one estate
    // BenchmarkReceipt — sorted-elapsed median and p95, precise allocation delta, Unjudged verdict —
    // and the AppHost BenchmarkGate.Gate judges it against the held claim under GatePolicy.Canonical,
    // fanning the judged receipt onto the envelope stream the dashboards benchmark layout ingests.
    public static IO<BenchmarkReceipt> FrameBench(
        BenchLane lane,
        Func<IO<FrameReceipt>> frame,
        CorrelationId correlation,
        Option<UInt128> corpus,
        Option<BenchmarkReceipt> claim,
        ReceiptSinkPort sink) =>
        from before in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        from frames in toSeq(Enumerable.Range(0, lane.Samples))
            .TraverseM(_ => ProofEngine.Advance(lane.Ticks).Bind(_ => frame()))
            .As()
        from after in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        let sorted = toSeq(frames.Map(static receipt => receipt.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed)).OrderBy(identity))
        let mid = sorted.Count / 2
        let fresh = new BenchmarkReceipt(
            Suite: BenchSuite,
            Case: lane.Case,
            Host: HostEvidence.Current(),
            Corpus: corpus,
            // True median: an odd count reads the central value, an even count averages the two
            // central values — the upper-of-two shortcut skews every even-sample bench upward.
            Median: (sorted.Count & 1) == 1
                ? sorted[mid]
                : Duration.FromNanoseconds((sorted[mid - 1] + sorted[mid]).TotalNanoseconds / 2d),
            P95: sorted[(int)Math.Ceiling(sorted.Count * 0.95d) - 1],
            AllocatedBytes: after - before,
            Operations: frames.Count,
            Reference: None,
            Verdict: BenchmarkVerdict.Unjudged,
            ArtifactKey: None,
            Correlation: correlation)
        from gate in BenchmarkGate.Gate(sink, fresh, claim, GatePolicy.Canonical)
        from judged in gate.Match(Succ: IO.pure, Fail: IO.fail<BenchmarkReceipt>)
        select judged;

    // Divergence magnitude on the shared advice axis: the fresh-versus-held median ratio lands on the
    // first covering DivergenceRatio edge, so a regression reads against the same buckets the
    // distribution histograms advertise; a zero-length held median or non-regression folds to zero.
    public static double Divergence(BenchmarkReceipt fresh, BenchmarkReceipt held) =>
        held.Median.TotalTicks <= 0d ? 0d
            : (fresh.Median.TotalTicks / held.Median.TotalTicks - 1d) switch {
                <= 0d => 0d,
                var ratio => toSeq(Buckets.DivergenceRatio)
                    .Find(edge => ratio <= edge)
                    .IfNone(Buckets.DivergenceRatio[^1]),
            };

    // BundleShape settles the exported support archive as two goldens — the zip entry roster and
    // extracted directory tree — so a member added to or dropped from the contributor roster
    // diverges the golden before any support session ships an incomplete bundle.
    public static async Task BundleShape(string bundlePath, string extractedRoot) {
        await Verifier.VerifyZip(bundlePath);
        await Verifier.VerifyDirectory(extractedRoot);
    }

    // Cost lane: baseline pass durations compare variance-aware, and every pass stays under the frame
    // budget — a pass past either bound folds its typed fault; a missing baseline is an admission fault.
    public static Fin<Seq<(string Pass, Duration Elapsed)>> FrameCost(
        FrameReceipt receipt, HashMap<string, Duration> baseline, FrameBudget budget, double variance) =>
        receipt.Passes.TraverseM(pass =>
            baseline.Find(pass.Pass).Match(
                Some: known => pass.Elapsed <= budget.Frame && pass.Elapsed.TotalTicks <= known.TotalTicks * (1.0 + variance)
                    ? Fin.Succ(pass)
                    : Fin.Fail<(string, Duration)>(new ProofFault.CostRegressed(pass.Pass, known.ToString(), pass.Elapsed.ToString())),
                None: () => Fin.Fail<(string, Duration)>(new ProofFault.BaselineAbsent(pass.Pass)))).As();
}
```

## [05]-[RESEARCH]

(none)
