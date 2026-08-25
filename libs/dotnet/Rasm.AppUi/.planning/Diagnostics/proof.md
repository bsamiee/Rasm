# [APPUI_DIAGNOSTICS_PROOF]

Rasm.AppUi proof derives capture, check, variant-density, benchmark, and replay cells from live catalogs. Capture rows prove pixels by content hash, benchmark rows gate headless frame cost against held `Benchmark` claims, command journals replay under virtual time, and CsCheck with Verify seals the matrix. This page owns the row families, derivation engine, benchmark lane, render-hash law, the typed `ProofFault` rail, and the golden and skew registries as DATA.

## [01]-[INDEX]

- [02]-[CAPTURE_LANES]: Host-agnostic frame capture rows; render-hash regression proof with its attribution election.
- [03]-[HEADLESS_DERIVATION]: Catalog-derived proof matrix and benchmark lanes; deterministic command-journal replay; the one index-divergence walk.
- [04]-[PROOF_LAW]: Law-matrix fence — FrameHash equality, deterministic capture, replay determinism, the instrument fold, the frame-bench gate, the bundle-tree pin, the shipped-roster conformance fold.
- [05]-[GUARD_REGISTRY]: Committed-golden roster and the cross-package skew guards as constructed rows with their re-prove entries.

## [02]-[CAPTURE_LANES]

- Owner: `ProofFault` — the direct generated `[Union]` with one `[FaultCase]` leaf per proof failure and the ONE divergence-attribution election; `FrameGrab` — the one grab shape returning the rasterized frame beside its optional sealed record; `CaptureRow` — the admitted per-surface capture row carrying scale, gamut, text posture, and tick policy; `Captures` — the shot-and-regression surface.
- Entry: `Captures.Shot(VisualRuntime runtime, CaptureRow row)` — `IO` rail through the settled encode fold with one PNG `VisualArtifact` per shot; `CaptureRow.Of` — accumulating admission whose refusal names every offending column.
- Auto: `CaptureRow.Key` is the complete artifact-cell identity supplied by `RenderHashLane.Cell`, so `Captures.Shot` prefixes it once and never re-appends scale, gamut, or posture; the `Scale`, `Gamut`, and `Posture` columns enter the grab delegate together, pinning render scaling, the exact `VisualCodec.ColorPolicy` row, and the exact `Theme/typography#SHAPING_RAIL` `RenderPosture` row, so a golden reproduces on any machine rather than diffing on the panel it was taken over; the `Ticks` column enters `ProofEngine.Advance`, the one forced-frame operation capture and benchmark lanes share; `VisualArtifact.FrameHash` rides the suite content-hash identity row, and a grab that recorded its ops hands the sealed `SKPicture` back so the encode owner folds its `Serialize` bytes onto `DrawHash`.
- Outcome: a regression divergence is a typed `ProofFault` whose CASE carries the attribution — `RasterDiverged` where draw ops held while pixels moved, `DrawDiverged` where the ops themselves moved, `HashDiverged` where either side carries no draw hash — elected by the ONE `ProofFault.Diverged` factory; a bare `Error.New` on this rail is the deleted form.
- Packages: SkiaSharp, Avalonia.Headless, Avalonia.Skia, Thinktecture.Runtime.Extensions, Rasm (kernel `FaultBand`/`Fault`), LanguageExt.Core
- Growth: one capture row absorbs a new surface lane; one `Scale` value absorbs a new DPI baseline; one `Posture` value absorbs a new surface-class text reading; one `ProofFault` case is one `[FaultCase]` leaf; a widened grab is one edit on `FrameGrab`; zero new surface.
- Law: grab delegates bind at composition per surface row — the headless lane rides `CaptureRenderedFrame`/`GetLastRenderedFrame` whose `WriteableBitmap` pixels enter the hash fold through `Lock()` over the `ILockedFramebuffer`, an un-shown top-level folds to an absent grab rather than a throw, `UseHeadlessDrawing` false selects the Skia backend on every hash lane, and `SetRenderScaling` pins the device scale before the grab (it throws on a non-positive scale, which the row's admitted `Scale` forecloses).
- Law: the custom-visual lane packs ONCE and replays twice — `CustomVisual.Record` seals the op list, `Materialize` replays it and HANDS THE SEALED RECORD BACK, `CustomVisual.RenderTwin` replays that same `VisualRecord`, and the caller owns the one release once both frames are sealed; a second layout run behind a parallel grab contract is the deleted duplicate; a wide-gamut custom tile hashes its float or ICC-tagged pixels and never a quantized sRGB shadow.
- Boundary: the rhino lane rides the settled host viewport capture port; the desktop in-tree lane renders through `RenderTargetBitmap.Render(Visual)` with `CopyPixels` as its pixel projection, or evaluates a live visual onto a leased Skia canvas through `DrawingContextHelper.RenderAsync`; `ForceRenderTimerTick` is the only frame-advance verb on the deterministic lane — a debounce that fails under forced ticks has smuggled wall time — and the tick count is a row column; `Regression` compares `VisualArtifact.FrameHash` and reads `DrawHash` only to name the divergence, so pixel equality stays the single pass condition; a per-spec screenshot helper and a second baseline store beside the blob lane are the rejected forms.

```csharp


// --- [ERRORS] --------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProofFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Proof;
    private ProofFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record HashDiverged(string Cell, UInt128 Actual, UInt128 Baseline) : ProofFault($"proof/render-hash: {Cell} diverged");
    [FaultCase(1)]
    public sealed partial record ReplayDiverged(int JournalIndex) : ProofFault($"proof/replay: journal index {JournalIndex} diverged");
    [FaultCase(2)]
    public sealed partial record GrabAbsent(string Cell) : ProofFault($"proof/capture: {Cell} produced no frame");
    [FaultCase(3)]
    public sealed partial record SessionUnavailable(string Value) : ProofFault($"proof/session: {Value}");
    [FaultCase(4)]
    public sealed partial record CostRegressed(string Pass, string Baseline, string Actual) : ProofFault($"proof/frame-cost: {Pass} regressed {Baseline} -> {Actual}");
    [FaultCase(5)]
    public sealed partial record BaselineAbsent(string Pass) : ProofFault($"proof/frame-cost: {Pass} has no admitted baseline");
    [FaultCase(6)]
    public sealed partial record ReplayShape(int First, int Second) : ProofFault($"proof/replay: outcome counts diverged {First} != {Second}");
    [FaultCase(7)]
    public sealed partial record BudgetInvalid(string ScreenId, string Column, string Value) : ProofFault($"proof/frame-budget: {ScreenId} refused {Column}={Value}");
    [FaultCase(8)]
    public sealed partial record CaptureInvalid(string Cell, string Column, string Value) : ProofFault($"proof/capture-budget: {Cell} refused {Column}={Value}");
    [FaultCase(9)]
    public sealed partial record RasterDiverged(string Cell, UInt128 Actual, UInt128 Baseline) : ProofFault($"proof/raster: {Cell} draw ops held while pixels moved {Actual} != {Baseline} — a rasterizer or driver change");
    [FaultCase(10)]
    public sealed partial record DrawDiverged(string Cell, UInt128 Actual, UInt128 Baseline) : ProofFault($"proof/draw: {Cell} draw ops moved {Actual} != {Baseline} — a content change");
    [FaultCase(11)]
    public sealed partial record DockCoverage(Seq<string> Gaps) : ProofFault($"proof/skew-dock: {Gaps.Count} controls resolve a control theme under some variants and not others");

    public static ProofFault Diverged(string cell, UInt128 actual, UInt128 baseline, Option<UInt128> freshDraw, Option<UInt128> heldDraw) =>
        freshDraw.Bind(fresh => heldDraw.Map(held => (Fresh: fresh, Held: held))).Match(
            Some: pair => pair.Fresh == pair.Held
                ? new RasterDiverged(cell, actual, baseline)
                : (ProofFault)new DrawDiverged(cell, pair.Fresh, pair.Held),
            None: () => new HashDiverged(cell, actual, baseline));
}

// --- [MODELS] --------------------------------------------------------------------------
public delegate IO<(SKImage Image, Option<SKPicture> Record)> FrameGrab(
    double scale,
    VisualCodec.ColorPolicy gamut,
    RenderPosture posture,
    Func<IO<Unit>> advance);

public sealed record CaptureRow {
    private CaptureRow(string key, double scale, VisualCodec.ColorPolicy gamut, RenderPosture posture, int ticks, FrameGrab grab) =>
        (Key, Scale, Gamut, Posture, Ticks, Grab) = (key, scale, gamut, posture, ticks, grab);

    public string Key { get; }
    public double Scale { get; }
    public VisualCodec.ColorPolicy Gamut { get; }
    public RenderPosture Posture { get; }
    public int Ticks { get; }
    public FrameGrab Grab { get; }

    public static Fin<CaptureRow> Of(string key, double scale, VisualCodec.ColorPolicy gamut, RenderPosture posture, int ticks, FrameGrab grab) =>
        (Slot(!string.IsNullOrWhiteSpace(key), key, "key", key),
         Slot(scale > 0d, key, "scale", scale.ToString(CultureInfo.InvariantCulture)),
         Slot(ticks > 0, key, "ticks", ticks.ToString(CultureInfo.InvariantCulture)))
            .Apply((_, _, _) => new CaptureRow(key, scale, gamut, posture, ticks, grab))
            .ToFin();

    static Validation<Error, Unit> Slot(bool holds, string cell, string column, string value) =>
        holds ? Validation<Error, Unit>.Success(unit)
              : Validation<Error, Unit>.Fail((Error)new ProofFault.CaptureInvalid(cell, column, value));

    public IO<(SKImage Image, Option<SKPicture> Record)> Shoot() => Grab(Scale, Gamut, Posture, () => ProofEngine.Advance(Ticks));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class Captures {
    public static readonly ArtifactKind Kind = ArtifactKind.Create("capture");

    public static IO<VisualArtifact> Shot(VisualRuntime runtime, CaptureRow row) =>
        row.Shoot().Bracket(
            frame => VisualCodec.Encode(runtime, frame.Image, VisualCodec.Png, Kind, $"captures/{row.Key}.png", frame.Record),
            frame => IO.lift(() => frame.Record.Iter(static record => record.Dispose())));

    public static Fin<VisualArtifact> Regression(string cell, VisualArtifact actual, UInt128 baseline, Option<UInt128> heldDraw) =>
        actual.FrameHash == baseline
            ? Fin.Succ(actual)
            : Fin.Fail<VisualArtifact>(ProofFault.Diverged(cell, actual.FrameHash, baseline, actual.DrawHash, heldDraw));
}
```

## [03]-[HEADLESS_DERIVATION]

- Owner: `ProofCheck` — the check vocabulary realizing kernel `ICapability<ProofCheck>`, so a screen row CARRIES the checks it admits as a `CapabilitySet<ProofCheck>` column instead of threading applicability predicates through every call; `ProofSpec` — the derived spec row; `ProofEngine` — the derivation, replay, and the ONE index-divergence walk; `RenderHashLane` — the scale-gamut-posture-keyed render-hash cell; `BenchLane` — the catalog-derived frame-benchmark cell with its warm-up column.
- Cases: activation, render-hash, focus-walk, variant-sweep, density-sweep, disposal-leak, pointer-walk, drag-drop, contrast-audit, semi-conformance, frame-cost — the two input-proof rows drive the headless synthetic-input verbs, the contrast-audit row sweeps the `Shell/accessibility.md` WCAG luminance gate over every variant-density cell, the semi-conformance row walks the shipped theme roster on the live application, and the frame-cost row proves per-pass render cost against the `FrameBudget`.
- Entry: `ProofEngine.Derive(catalog, grid, probe)` — each headless row crosses only the checks its own `Checks` capability column admits, then spans every variant-density cell; `ProofEngine.Bench(catalog, samples, ticks, warmup)` admits budgets before constructing any benchmark lane; `ProofEngine.Divergent(first, second)` — indices past the shorter side report as mismatches, so a dropped or extra tail outcome never hides behind pairwise truncation, and `Diagnostics/devloop.md`'s cross-machine verify composes this same walk.
- Auto: derived specs execute on the shared `HeadlessUnitTestSession` through `GetOrStartForAssembly` once per assembly and `Dispatch` per spec — a session that cannot start refuses as `ProofFault.SessionUnavailable` at the acquisition, never as an untyped throw; `FakeTimeProvider` time travel fills the headless row's virtual-time slot; `Replay` drives the journal through the one remote-invocation route on the frozen deck, so journal replay, deep links, and interactive execution return the same `DeckOutcome`; the snapshot store rehydrates screen state before the first journal entry, so replay is deterministic end to end; the pointer-walk and drag-drop checks drive synthetic input through `HeadlessWindowExtensions.MouseDown`/`MouseMove`/`MouseUp`/`MouseWheel` between `ForceRenderTimerTick` advances, the drag-drop check driving `DragDrop` in the load-bearing `DragEnter` → `DragOver` → `Drop` sequence (a `DragOver` without a prior `DragEnter` seeds no drop context), the resulting effect read from `DragEventArgs.DragEffects` inside the handler; benchmark lanes derive from the same `HeadlessLane` the proof matrix crosses, so a screen added to the catalog gains its frame benchmark with zero roster edit.
- Outcome: executed operations fire their own `AppUiFact` cases; each proof row returns `Unit` or its typed `ProofFault`.
- Packages: Avalonia.Headless, Avalonia.Headless.XUnit, Avalonia.Skia, Verify.XunitV3, CsCheck (testkit), Microsoft.Extensions.TimeProvider.Testing, Thinktecture.Runtime.Extensions, Rasm (kernel `CapabilitySet`), LanguageExt.Core, BCL inbox
- Growth: one check row sweeps every headless screen whose `Checks` column admits it, one grid cell sweeps every check, one `RenderHashLane` cell sweeps every key×scale×gamut combination, and one `BenchLane` cell sweeps every headless row at its budgets; zero new surface.
- Boundary: the derivation engine deletes hand-written per-screen smoke specs — a bespoke screen spec beside the engine is the named defect; every lane rides the ONE shared session, because `StartNew` composes the assembly's `[AvaloniaTestApplication]` entry point and only DEFAULTS the headless WINDOWING subsystem where that entry point selected none — the entry point's own `UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })` and `UseSkia` selections both survive into the session the render-hash lanes draw under; a second render-proof `AppBuilder` beside that session is the rejected form and the `Shell/hosts#HOST_AXIS` one-setup guard throws process-wide on the second admission; host-bound screens exit the matrix structurally through the catalog's headless lane, never through skipped specs.
- Law: `ScreenCatalogRow.Checks : CapabilitySet<ProofCheck>` is the applicability RELATION seated on the catalog row (`Shell/screens.md`), so which checks a screen admits reads off the roster a maintainer edits rather than off a predicate closure at every derivation call.

```csharp
// --- [TYPES] ---------------------------------------------------------------------------
[NoReorder]
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class ProofCheck : ICapability<ProofCheck> {
    public static readonly ProofCheck Activation = new("activation");
    public static readonly ProofCheck RenderHash = new("render-hash");
    public static readonly ProofCheck FocusWalk = new("focus-walk");
    public static readonly ProofCheck VariantSweep = new("variant-sweep");
    public static readonly ProofCheck DensitySweep = new("density-sweep");
    public static readonly ProofCheck DisposalLeak = new("disposal-leak");
    public static readonly ProofCheck PointerWalk = new("pointer-walk");
    public static readonly ProofCheck DragDrop = new("drag-drop");
    public static readonly ProofCheck ContrastAudit = new("contrast-audit");
    public static readonly ProofCheck SemiConformance = new("semi-conformance");
    public static readonly ProofCheck FrameCost = new("frame-cost");
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record ProofSpec(
    string ScreenId,
    ProofCheck Check,
    ThemeVariantRow Variant,
    DensityRow Density,
    Func<IO<Unit>> Run);

public sealed record BenchLane {
    private BenchLane(string screenId, int samples, int ticks, int warmup) =>
        (ScreenId, Samples, Ticks, Warmup) = (screenId, samples, ticks, warmup);

    public string ScreenId { get; }
    public int Samples { get; }
    public int Ticks { get; }
    public int Warmup { get; }
    public string Case => $"{ScreenId}@{Samples}x{Ticks}~w{Warmup}";

    public static Fin<BenchLane> Of(string screenId, int samples, int ticks, int warmup) =>
        (Slot(!string.IsNullOrWhiteSpace(screenId), screenId, "screen-id", screenId),
         Slot(samples > 0, screenId, "samples", samples.ToString(CultureInfo.InvariantCulture)),
         Slot(ticks > 0, screenId, "ticks", ticks.ToString(CultureInfo.InvariantCulture)),
         Slot(warmup >= 0, screenId, "warmup", warmup.ToString(CultureInfo.InvariantCulture)))
            .Apply((_, _, _, _) => new BenchLane(screenId, samples, ticks, warmup))
            .ToFin();

    static Validation<Error, Unit> Slot(bool holds, string screenId, string column, string value) =>
        holds ? Validation<Error, Unit>.Success(unit)
              : Validation<Error, Unit>.Fail((Error)new ProofFault.BudgetInvalid(screenId, column, value));
}

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class ProofEngine {
    public static Fin<Seq<BenchLane>> Bench(ScreenCatalog catalog, int samples, int ticks, int warmup) =>
        catalog.HeadlessLane.TraverseM(row => BenchLane.Of(row.Id, samples, ticks, warmup)).As();

    internal static IO<Unit> Advance(int ticks) =>
        IO.lift(() => AvaloniaHeadlessPlatform.ForceRenderTimerTick(ticks));

    public static Seq<ProofSpec> Derive(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<Unit>>> probe) =>
        catalog.HeadlessLane.Bind(row =>
            grid.Bind(cell =>
                toSeq(ProofCheck.Items).Filter(check => row.Checks.Admits(check)).Map(check =>
                    new ProofSpec(row.Id, check, cell.Variant, cell.Density, probe(row, check, cell.Variant, cell.Density)))));

    public static IO<Unit> Dispatch(ProofSpec spec) =>
        (IO.lift(() => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(ProofEngine).Assembly))
            | @catch<IO, HeadlessUnitTestSession>(static _ => true,
                static error => IO.fail<HeadlessUnitTestSession>(error)))
        .Bind(session => IO.liftAsync(async () => await session
            .Dispatch(() => spec.Run().RunAsync().AsTask(), CancellationToken.None)
            .ConfigureAwait(false)));

    public static IO<Seq<DeckOutcome>> Replay(
        CommandDeck deck,
        Seq<(string Key, Rasm.Contracts.Ui.CommandPayloadWire Payload, CallerModality Caller)> journal,
        Func<IO<Unit>> restore) =>
        restore().Bind(_ => journal.TraverseM(entry => deck.Invoke(entry.Key, entry.Payload, entry.Caller)).As());

    public static Seq<int> Divergent(Seq<string> first, Seq<string> second) =>
        toSeq(Enumerable.Range(0, Math.Max(first.Count, second.Count)))
            .Filter(index => index >= first.Count || index >= second.Count || first[index] != second[index])
            .Strict();
}
```

## [04]-[PROOF_LAW]

- Owner: `ProofLaw` — the law-matrix fence surface composing `ProofEngine` with CsCheck property generators, `Verify.XunitV3` FrameHash equality, the `MetricCollector<T>` instrument lane, the frame-bench gate returning the judged `Benchmark`, the bundle-tree pin, the `Theme/tokens.md` shipped-roster conformance fold, and the suite-hygiene gates over the golden registry.
- Entry: `ProofMatrix(...)` — the one entrypoint owning the singular-cell and full-matrix run by input shape, so a per-spec screenshot helper is the deleted form.
- Auto: `RenderHashGrid` generates cells from the live headless catalog crossed with admitted scale, `VisualCodec.ColorPolicy`, and `RenderPosture` data, so a new screen, gamut, or surface-class text reading expands proof without a named roster edit; `FrameHashEquality` seals one generated cell through `Captures.Shot` then `Verifier.Verify`; `ReplayDeterminism` restores the same snapshot before each journal run, resets virtual time, rejects unequal outcome counts before pairing, and verifies the complete digest sequence; `FrameCost` requires a baseline for every pass and ACCUMULATES every regressed pass, so a second regression never hides behind the first; `InstrumentFold` mounts contributions, resolves the named handle off the mounted roster, and brackets the collector around the supplied exercise so direct writes and observable-gauge reads share one cell family; `FrameBench` discards the lane's warm-up frames before the allocation bracket, samples between forced ticks, hands the elapsed spans to `BenchMeasurement.Of`, mints one unjudged `Benchmark`, and composes `BenchmarkGate.Gate` over the held claim and mounted instruments; `Divergence` buckets the fresh-versus-held median ratio under `Buckets.DivergenceRatio`; `BundleShape` pins the exported support archive as two goldens that both report.
- Packages: Verify.XunitV3, CsCheck, Avalonia.Headless, Microsoft.Extensions.Diagnostics.Testing, Rasm.AppHost (project, seam types), Rasm (kernel `Custody`/`UnitInterval`), NodaTime, LanguageExt.Core
- Growth: one lane cell absorbs a new golden; one benchmark claim is one held `Benchmark` value; zero new surface.
- Law: the `RenderHashGrid` FrameHash golden bytes derive under the `libs/contracts/manifest.json` `content-identity` framing and seed law and stay a .NET-tree snapshot no peer runtime binds — the render-hash lane is the one host golden producer; the property lanes take a DECLARED seed, so a red run replays byte-for-byte instead of reproducing by luck.
- Law: the proof fence is a terminal edge — `IO<A>.Run()`/`RunAsync()` THROW the typed `Error` on failure, so a failing disposition composes BEFORE the terminal, and the `@catch` recovery on a property lane narrows to the proof band so a bug outside it surfaces instead of reading as a clean `false` verdict.
- Law: the frame-bench lane composes the AppHost benchmark rail as settled vocabulary — `BenchMeasurement.Of`, `Benchmark.Of`, `BenchmarkGate.Gate`, and `GatePolicy.Canonical` mint and judge over `HostFingerprint`; the held claim arrives as a value off the Persistence reuse index, and the gate writes its mounted instruments directly; the allocation delta reads a process-wide counter, so bench lanes run serially and no parallel proof cell overlaps the bracket.
- Boundary: `VerifyZip`/`VerifyDirectory` pin support-bundle roster and tree completeness, and the extracted `manifest.json` carries the AppHost `SupportManifest.Entry` `ContentKey` column, so content identity pins in the same golden pair rather than a re-hash of the zip.

```csharp
// --- [MODELS] --------------------------------------------------------------------------
public readonly record struct RenderHashLane(string Key, double Scale, VisualCodec.ColorPolicy Gamut, RenderPosture Posture, int Ticks) {
    public string Cell => $"{Key}@{Scale}x{Gamut.Key}~{Posture.Key}";

    public Fin<CaptureRow> Row(FrameGrab grab) => CaptureRow.Of(Cell, Scale, Gamut, Posture, Ticks, grab);
}

public static class ProofLaw {
    // --- [GENERATORS]
    public static Seq<RenderHashLane> RenderHashGrid(
        ScreenCatalog catalog,
        Seq<double> scales,
        Seq<VisualCodec.ColorPolicy> gamuts,
        Seq<RenderPosture> postures,
        int ticks) =>
        catalog.HeadlessLane
            .Filter(static row => row.Checks.Admits(ProofCheck.RenderHash))
            .Bind(row => scales.Bind(scale => gamuts.Bind(gamut =>
                postures.Map(posture => new RenderHashLane(row.Id, scale, gamut, posture, ticks)))));

    public static Gen<RenderHashLane> LaneGen(Seq<RenderHashLane> lanes) => Gen.OneOfConst([.. lanes]);

    // --- [LAW_ENTRIES]
    public static async Task SuiteHygiene() {
        GoldenLanes.Sound().ThrowIfFail();
        await Task.WhenAll(
            VerifyChecks.Run(),
            Task.Run(static () => DanglingSnapshots.Run()));
    }

    public static async Task FrameHashEquality(VisualRuntime runtime, RenderHashLane lane, FrameGrab grab) {
        VisualArtifact artifact = await lane.Row(grab)
            .Match(Succ: row => Captures.Shot(runtime, row), Fail: IO.fail<VisualArtifact>)
            .RunAsync();
        await Verifier.Verify(new { lane.Cell, artifact.FrameHash, artifact.DrawHash, artifact.ColorSpace })
            .UniqueForTargetFramework()
            .UseTextForParameters(lane.Cell);
    }

    public static void DeterministicCapture(VisualRuntime runtime, Seq<RenderHashLane> lanes, FrameGrab grab, string seed) =>
        LaneGen(lanes).Sample(lane =>
            (lane.Row(grab).Match(
                Succ: row => Captures.Shot(runtime, row)
                    .Bind(first => Captures.Shot(runtime, row)
                        .Map(second => first.FrameHash == second.FrameHash && first.DrawHash == second.DrawHash)),
                Fail: IO.fail<bool>)
                | @catch<IO, bool>(static error => error is ProofFault, static _ => IO.pure(false))).As().Run(),
            seed: seed);

    public static IO<Unit> ProofMatrix(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<Unit>>> probe) =>
        ProofEngine.Derive(catalog, grid, probe).TraverseM(ProofEngine.Dispatch).As().Map(static _ => unit);

    public static async Task ReplayDeterminism(
        CommandDeck deck,
        Seq<(string Key, Rasm.Contracts.Ui.CommandPayloadWire Payload, CallerModality Caller)> journal,
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

    public static Task BundleShape(string bundlePath, string extractedRoot) =>
        Task.WhenAll(
            Task.Run(async () => await Verifier.VerifyZip(bundlePath)),
            Task.Run(async () => await Verifier.VerifyDirectory(extractedRoot)));

    // --- [FOLDS]
    public static Fin<long> InstrumentFold(
        IMeterFactory factory, string version, CorrelationId root,
        LevelCells cells, Seq<TelemetryContributorPort> contributions,
        string instrument, Func<InstrumentSet, Fin<Unit>> exercise) =>
        AppUiTelemetry.Mount(factory, version, root, cells, contributions).Bind(set =>
            set.Mounts.Find(seat => seat.Row.Name == instrument)
                .Bind(static seat => seat.Handle is Instrument<long> whole ? Some(whole) : None)
                .ToFin(Fail: new ProofFault.SessionUnavailable($"proof/instrument: {instrument} is no mounted whole-measure instrument"))
                .Bind(mounted => Custody.Bracket(
                    acquire: () => new MetricCollector<long>(mounted),
                    project: collector => exercise(set)
                        .Map(_ => collector.GetMeasurementSnapshot().Sum(static measurement => measurement.Value)),
                    key: Op.Of(name: "proof.instrument"))));

    public static Fin<Unit> SemiConformance(ResolvedTheme resolved, Seq<IStyle> chain) =>
        SemiCorrespondence.SemiCovered(resolved, SemiRoster.Walk(chain));

    public static Fin<Seq<(string Pass, Duration Elapsed)>> FrameCost(
        FrameRender frame, HashMap<string, Duration> baseline, FrameBudget budget, UnitInterval variance) =>
        frame.Passes.Traverse(pass =>
            baseline.Find(pass.Pass).Match(
                Some: known => pass.Elapsed <= budget.Frame && pass.Elapsed.TotalTicks <= known.TotalTicks * (1.0 + variance.Value)
                    ? Validation<Error, (string, Duration)>.Success(pass)
                    : Validation<Error, (string, Duration)>.Fail((Error)new ProofFault.CostRegressed(pass.Pass, known.ToString(), pass.Elapsed.ToString())),
                None: () => Validation<Error, (string, Duration)>.Fail((Error)new ProofFault.BaselineAbsent(pass.Pass))))
            .As().ToFin();

    public static double Divergence(Benchmark fresh, Benchmark held) =>
        held.Measured.Figures.Median.To() <= 0d ? 0d
            : (fresh.Measured.Figures.Median.To() / held.Measured.Figures.Median.To() - 1d) switch {
                <= 0d => 0d,
                var ratio => toSeq(Buckets.DivergenceRatio)
                    .Find(edge => ratio <= edge)
                    .IfNone(Buckets.DivergenceRatio[^1]),
            };

    // --- [BENCH]
    public const string BenchSuite = "rasm.appui.frame";

    public static IO<Benchmark> FrameBench(
        BenchLane lane,
        Func<IO<FrameRender>> frame,
        Option<UInt128> corpus,
        Option<Benchmark> claim,
        InstrumentSet signals) =>
        from _warm in toSeq(Enumerable.Range(0, lane.Warmup))
            .TraverseM(_ => ProofEngine.Advance(lane.Ticks).Bind(_ => frame()))
            .As()
        from before in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        from frames in toSeq(Enumerable.Range(0, lane.Samples))
            .TraverseM(_ => ProofEngine.Advance(lane.Ticks).Bind(_ => frame()))
            .As()
        from after in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        from measured in IO.lift(() => BenchMeasurement.Of(
            spans: frames.Map(static result => result.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed)),
            allocatedBytes: after - before,
            operations: frames.Count,
            key: Op.Of()))
        let fresh = measured.Map(figures => Benchmark.Of(
            suite: BenchSuite,
            @case: lane.Case,
            corpus: corpus,
            measured: figures,
            stamps: FrozenDictionary<string, string>.Empty))
        from gate in fresh.Match(
            Succ: row => BenchmarkGate.Gate(signals, row, claim, GatePolicy.Canonical, Op.Of()),
            Fail: fault => IO.pure(Validation<Error, Benchmark>.Fail((BenchmarkFault)fault)))
        from judged in gate.Match(Succ: IO.pure, Fail: static faults => IO.fail<Benchmark>(faults.Head))
        select judged;
}
```

## [05]-[GUARD_REGISTRY]

- Owner: `GoldenLane` with `GoldenLanes` — the committed-golden roster as CONSTRUCTED rows, each naming its pinned artifact beside the entry that writes it, with the `Sound` uniqueness fold the suite-hygiene gate reads; `SkewVerdict` `[SmartEnum<string>]` — the three-value guard disposition; `SkewGuard` with `SkewGuards` — the per-pair guard rows carrying witnessed verdict, resolution, and the entry that RE-PROVES on a bump, beside the two live re-provable folds.
- Cases: `SkewVerdict` = binds | covers | fails — `binds` is a cross-boundary type crossing that loaded and invoked, `covers` is a themable-surface roster that resolved whole, `fails` is a load-time refusal whose resolution is the DROP recorded beside it.
- Entry: `SkewGuards.DockTheming(host, controls, variants)` — refuses with the WHOLE asymmetric-gap set as a typed `ProofFault.DockCoverage`, so no gap hides past a message-format cap; `SkewGuards.SkiaBoundary(context)` — leases the Skia api feature at the live render boundary under `Custody.Bracket` and refuses each crossing BY NAME on an accumulating admission.
- Auto: a guard row is DATA — the pair, the mechanism, the witnessed verdict, and the resolution are columns, and a row whose `Guard` column names an entry re-proves on every bump of either side; the two provable guards ride the settled proof rails and mint nothing — dock coverage is a resource read over the live theme (the sweep passes each `ThemeVariant` explicitly), and the Skia boundary is one lease off the same `ISkiaSharpApiLeaseFeature` the `Vfx` material route composes; a dropped package carries no `Guard` entry because there is nothing left to bind.
- Outcome: a guard refusal is a typed `ProofFault` on the proof rail like every other lane, so a skew break reports beside a pixel drift rather than as a build-time surprise.
- Packages: Avalonia, Avalonia.Skia, SkiaSharp, Semi.Avalonia.Dock, Dock.Avalonia, Thinktecture.Runtime.Extensions, Rasm (kernel `Custody`), LanguageExt.Core
- Growth: a new golden is one `GoldenLanes` row naming its writer; a new cross-package pair is one `SkewGuards` row and, where the pair is bindable, one fold beside the two here; zero new surface.
- Law: the registry is the reverse index from a committed artifact to the entry that writes it — a golden with no row is an orphan file and a row with no writer is a lane nothing proves; `LayoutWireGolden.Canonical` lives in the AppUI test package because it asserts the deterministic `Shell/solver#TS_PROJECTION` canonical serialization, and this registry NAMES it rather than re-spelling it, since a second snapshot over the same wires would commit a second file that drifts silently from the first.
- Law: the three package rows record verdicts witnessed on the assay bind-and-invoke rail, and a verdict is never a version pin — the Skia pair is proven at the render boundary rather than pinned, because the framework's declared dependency is a FLOOR the loader satisfies with a higher major under a matching public key, so the guard is the re-run.
- Boundary: the validation package is DROPPED estate-wide rather than pinned back — the manifest and registry drop lands at the package owners and its row records the verdict and the resolution; the dock skin carries zero keys in its own theme dictionaries and inherits every light/dark decision from the base Semi dictionaries, so the standing obligation is VARIANT COHERENCE — the two Semi packages move to one variant vocabulary together while the dock skin may lag the dock CONTROL package freely (`Shell/navigation#DOCK_LAYOUTS` states the same obligation at the consuming boundary).

```csharp
// --- [TABLES] --------------------------------------------------------------------------
public sealed record GoldenLane(string Lane, string Pins, string Writer);

public static class GoldenLanes {
    public static readonly Seq<GoldenLane> Rows = Seq(
        new GoldenLane("render-hash", "frame hash, draw hash, colour space per capture cell", nameof(ProofLaw.FrameHashEquality)),
        new GoldenLane("replay-digests", "the whole ordered payload-digest sequence of two replays", nameof(ProofLaw.ReplayDeterminism)),
        new GoldenLane("bundle-roster", "support-archive zip entries", nameof(ProofLaw.BundleShape)),
        new GoldenLane("bundle-tree", "extracted tree beside each entry's ContentKey", nameof(ProofLaw.BundleShape)),
        new GoldenLane(
            "layout-protojson-golden",
            "ordered generated LayoutProgram canonical ProtoJSON",
            "Rasm.AppUi.Tests LayoutWireGolden.Canonical"));

    public static Fin<Unit> Sound() =>
        Rows.Map(static row => row.Lane).Distinct().Count == Rows.Count
        && Rows.ForAll(static row => !string.IsNullOrWhiteSpace(row.Writer))
            ? Fin.Succ(unit)
            : Fin.Fail<Unit>(new ProofFault.SessionUnavailable("proof/goldens: every lane is unique and names its writer"));
}

[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class SkewVerdict {
    public static readonly SkewVerdict Binds = new("binds");
    public static readonly SkewVerdict Covers = new("covers");
    public static readonly SkewVerdict Fails = new("fails");
}

public sealed record SkewGuard(string Pair, string Mechanism, SkewVerdict Verdict, string Resolution, Option<string> Guard);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class SkewGuards {
    public static readonly Seq<SkewGuard> Rows = Seq(
        new SkewGuard("validation rail over reactive major", "restore graph read, then bind-invoke", SkewVerdict.Fails,
            "dropped estate-wide; rail rebuilt owned", None),
        new SkewGuard("dock skin over dock controls", "control-theme resolution per variant", SkewVerdict.Covers,
            "re-proves on every bump of either side", Some(nameof(DockTheming))),
        new SkewGuard("framework Skia over pinned stack", "lease and invoke at render boundary", SkewVerdict.Binds,
            "re-proves on every bump of either side", Some(nameof(SkiaBoundary))));

    public static Fin<Unit> DockTheming(IResourceHost host, Seq<Type> controls, Seq<ThemeVariant> variants) =>
        controls
            .Map(control => (Control: control,
                Resolved: variants.Filter(variant => host.TryFindResource(control, variant, out object? theme) && theme is ControlTheme)))
            .Filter(row => !row.Resolved.IsEmpty && row.Resolved.Count < variants.Count)
            .Map(static row => row.Control.Name) switch {
            { IsEmpty: true } => Fin.Succ(unit),
            var gaps => Fin.Fail<Unit>(new ProofFault.DockCoverage(gaps.Strict())),
        };

    public static IO<Fin<Unit>> SkiaBoundary(ImmediateDrawingContext context) =>
        IO.lift(() => context.TryGetFeature<ISkiaSharpApiLeaseFeature>(out ISkiaSharpApiLeaseFeature? feature) && feature is not null
            ? Custody.Bracket(
                acquire: () => feature.Lease(),
                project: static lease =>
                    (Probe(lease.SkCanvas is not null, "proof/skew-skia: the leased canvas did not resolve"),
                     Probe(SkiaSharpExtensions.ToSKColor(Colors.Transparent).Alpha == 0, "proof/skew-skia: the colour crossing did not resolve"))
                        .Apply(static (_, _) => unit)
                        .ToFin(),
                key: Op.Of(name: "proof.skew.skia"))
            : Fin.Fail<Unit>(new ProofFault.SessionUnavailable("proof/skew-skia: the draw context hands out no Skia api lease")));

    static Validation<Error, Unit> Probe(bool holds, string detail) =>
        holds ? Validation<Error, Unit>.Success(unit) : Validation<Error, Unit>.Fail((Error)new ProofFault.SessionUnavailable(detail));
}
```

## [06]-[RESEARCH]

(none)
