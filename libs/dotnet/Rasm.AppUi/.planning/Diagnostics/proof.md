# [APPUI_DIAGNOSTICS_PROOF]

Rasm.AppUi proof derives capture, check, variant-density, benchmark, and replay cells from live catalogs. Capture rows prove pixels by content hash, benchmark rows gate headless frame cost against held `BenchmarkReceipt` claims, command journals replay under virtual time, and CsCheck with Verify seals the matrix. This page owns the row families, derivation engine, benchmark lane, render-hash law, the typed `ProofFault` rail, and the golden and skew registries as DATA.

## [01]-[INDEX]

- [02]-[CAPTURE_LANES]: Host-agnostic frame capture rows; render-hash regression proof with its attribution election.
- [03]-[HEADLESS_DERIVATION]: Catalog-derived proof matrix and benchmark lanes; deterministic command-journal replay; the one index-divergence walk.
- [04]-[PROOF_LAW]: Law-matrix fence — FrameHash equality, deterministic capture, replay determinism, the instrument fold, the frame-bench gate, the bundle-tree pin, the shipped-roster conformance fold.
- [05]-[GUARD_REGISTRY]: Committed-golden roster and the cross-package skew guards as constructed rows with their re-prove entries.

## [02]-[CAPTURE_LANES]

- Owner: `ProofFault` — the direct generated `[Union]` with one `[FaultCase]` leaf per proof failure and the ONE divergence-attribution election; `FrameGrab` — the one grab shape returning the rasterized frame beside its optional sealed record; `CaptureRow` — the admitted per-surface capture row carrying scale, gamut, text posture, and tick policy; `Captures` — the shot-and-regression surface.
- Entry: `Captures.Shot(VisualRuntime runtime, CaptureRow row)` — `IO` rail through the settled encode fold with one PNG artifact and one render receipt per shot; `CaptureRow.Of` — accumulating admission whose refusal names every offending column.
- Auto: `CaptureRow.Key` is the complete artifact-cell identity supplied by `RenderHashLane.Cell`, so `Captures.Shot` prefixes it once and never re-appends scale, gamut, or posture; the `Scale`, `Gamut`, and `Posture` columns enter the grab delegate together, pinning render scaling, the exact `VisualCodec.ColorPolicy` row, and the exact `Theme/typography#SHAPING_RAIL` `RenderPosture` row, so a golden reproduces on any machine rather than diffing on the panel it was taken over; the `Ticks` column enters `ProofEngine.Advance`, the one forced-frame operation capture and benchmark lanes share; the receipt's `FrameHash` rides the suite content-hash identity row (the kernel `ContentHash.Of` delegate the capture runtime binds), and a grab that recorded its ops hands the sealed `SKPicture` back so the encode owner folds its `Serialize` bytes onto the receipt's `DrawHash` column.
- Receipt: a regression divergence is a typed `ProofFault` whose CASE carries the attribution — `RasterDiverged` where draw ops held while pixels moved, `DrawDiverged` where the ops themselves moved, `HashDiverged` where either side carries no draw hash — elected by the ONE `ProofFault.Diverged` factory; a bare `Error.New` on this rail is the deleted form.
- Packages: SkiaSharp, Avalonia.Headless, Avalonia.Skia, Thinktecture.Runtime.Extensions, Rasm (kernel `FaultBand`/`Fault`), LanguageExt.Core
- Growth: one capture row absorbs a new surface lane; one `Scale` value absorbs a new DPI baseline; one `Posture` value absorbs a new surface-class text reading; one `ProofFault` case is one `[FaultCase]` leaf; a widened grab is one edit on `FrameGrab`; zero new surface.
- Law: grab delegates bind at composition per surface row — the headless lane rides `CaptureRenderedFrame`/`GetLastRenderedFrame` whose `WriteableBitmap` pixels enter the hash fold through `Lock()` over the `ILockedFramebuffer`, an un-shown top-level folds to an absent grab rather than a throw, `UseHeadlessDrawing` false selects the Skia backend on every hash lane, and `SetRenderScaling` pins the device scale before the grab (it throws on a non-positive scale, which the row's admitted `Scale` forecloses).
- Law: the custom-visual lane packs ONCE and replays twice — `CustomVisual.Record` seals the op list, `Materialize` replays it and HANDS THE SEALED RECORD BACK, `CustomVisual.RenderTwin` replays that same `VisualRecord`, and the caller owns the one release once both frames are sealed; a second layout run behind a parallel grab contract is the deleted duplicate; a wide-gamut custom tile hashes its float or ICC-tagged pixels and never a quantized sRGB shadow.
- Boundary: the rhino lane rides the settled host viewport capture port; the desktop in-tree lane renders through `RenderTargetBitmap.Render(Visual)` with `CopyPixels` as its pixel projection, or evaluates a live visual onto a leased Skia canvas through `DrawingContextHelper.RenderAsync`; `ForceRenderTimerTick` is the only frame-advance verb on the deterministic lane — a debounce that fails under forced ticks has smuggled wall time — and the tick count is a row column; `Regression` compares `FrameHash` values from the settled receipt family and reads `DrawHash` only to NAME the divergence, so pixel equality stays the single pass condition; a per-spec screenshot helper and a second baseline store beside the blob lane are the rejected forms.

```csharp signature


// --- [ERRORS] -------------------------------------------------------------------------------
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProofFault : Fault {
    private static readonly FaultBand FamilyBand = FaultBand.Proof;
    private ProofFault(string detail) { Detail = detail; }
    public string Detail { get; }
    public override string Message => Detail;

    [FaultCase(0)]
    public sealed partial record HashDiverged(string Cell, string Actual, string Baseline) : ProofFault($"proof/render-hash: {Cell} diverged");
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
    public sealed partial record ReplayShape(int First, int Second) : ProofFault($"proof/replay: receipt counts diverged {First} != {Second}");
    // Column-precise admission refusals: the accumulating gate mints ONE fault per offending column, so a row
    // with two bad columns reports two typed refusals rather than one message interpolating the tuple.
    [FaultCase(7)]
    public sealed partial record BudgetInvalid(string ScreenId, string Column, string Value) : ProofFault($"proof/frame-budget: {ScreenId} refused {Column}={Value}");
    [FaultCase(8)]
    public sealed partial record CaptureInvalid(string Cell, string Column, string Value) : ProofFault($"proof/capture-budget: {Cell} refused {Column}={Value}");
    [FaultCase(9)]
    public sealed partial record RasterDiverged(string Cell, string Actual, string Baseline) : ProofFault($"proof/raster: {Cell} draw ops held while pixels moved {Actual} != {Baseline} — a rasterizer or driver change");
    [FaultCase(10)]
    public sealed partial record DrawDiverged(string Cell, string Actual, string Baseline) : ProofFault($"proof/draw: {Cell} draw ops moved {Actual} != {Baseline} — a content change");
    // The WHOLE gap set rides typed — a truncated list in a message string loses every gap past the format cap.
    [FaultCase(11)]
    public sealed partial record DockCoverage(Seq<string> Gaps) : ProofFault($"proof/skew-dock: {Gaps.Count} controls resolve a control theme under some variants and not others");

    // The ONE divergence-attribution election: ops held while pixels moved is a rasterizer change, ops moved is
    // a content change, and an absent draw hash on either side leaves the break honestly unattributed.
    public static ProofFault Diverged(string cell, string actual, string baseline, Option<string> freshDraw, Option<string> heldDraw) =>
        freshDraw.Bind(fresh => heldDraw.Map(held => fresh == held)).Match(
            Some: same => same
                ? new RasterDiverged(cell, actual, baseline)
                : (ProofFault)new DrawDiverged(cell, freshDraw.IfNone(string.Empty), heldDraw.IfNone(string.Empty)),
            None: () => new HashDiverged(cell, actual, baseline));
}

// --- [MODELS] -------------------------------------------------------------------------------
// One capture-grab shape: a named delegate replaces the five-arity generic every lane, twin, and law entry
// otherwise re-spells, so widening the grab is one edit here.
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
    // Text is the one raster input a golden cannot leave to the host: subpixel coverage carries the panel's own
    // RGB stripe order. Posture is a COLUMN because the paged-export lane pins its own linear-metric reading,
    // and a golden proving screen text against page text proves neither.
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class Captures {
    public const string Kind = "capture";

    // The BRACKET releases the record on both exits: a release seated in the success `Map` never runs on the
    // encode's failure arm, and the regression lanes shoot the same cell repeatedly under a property generator —
    // one leaked picture becomes a leak per sample precisely on the runs that are already failing.
    public static IO<RenderReceipt> Shot(VisualRuntime runtime, CaptureRow row) =>
        row.Shoot().Bracket(
            frame => VisualCodec.Encode(runtime, frame.Image, VisualCodec.Png, Kind, $"captures/{row.Key}.png", frame.Record),
            frame => IO.lift(() => frame.Record.Iter(static record => record.Dispose())));

    // RenderHashLane.Cell is the custom-twin key@variant-density lane identity, never the family Kind constant —
    // same-family failures stay attributable to their exact cell.
    public static Fin<RenderReceipt> Regression(string cell, RenderReceipt actual, string baseline, Option<string> heldDraw) =>
        actual.FrameHash == baseline
            ? Fin.Succ(actual)
            : Fin.Fail<RenderReceipt>(ProofFault.Diverged(cell, actual.FrameHash, baseline, actual.DrawHash, heldDraw));
}
```

## [03]-[HEADLESS_DERIVATION]

- Owner: `ProofCheck` — the check vocabulary realizing kernel `ICapability<ProofCheck>`, so a screen row CARRIES the checks it admits as a `CapabilitySet<ProofCheck>` column instead of threading applicability predicates through every call; `ProofSpec` — the derived spec row; `ProofEngine` — the derivation, replay, and the ONE index-divergence walk; `RenderHashLane` — the scale-gamut-posture-keyed render-hash cell; `BenchLane` — the catalog-derived frame-benchmark cell with its warm-up column.
- Cases: activation, render-hash, focus-walk, variant-sweep, density-sweep, disposal-leak, pointer-walk, drag-drop, contrast-audit, semi-conformance, frame-cost — the two input-proof rows drive the headless synthetic-input verbs, the contrast-audit row sweeps the `Shell/accessibility.md` WCAG luminance gate over every variant-density cell, the semi-conformance row walks the shipped theme roster on the live application, and the frame-cost row proves per-pass render cost against the `FrameBudget`.
- Entry: `ProofEngine.Derive(catalog, grid, probe)` — each headless row crosses only the checks its own `Checks` capability column admits, then spans every variant-density cell; `ProofEngine.Bench(catalog, samples, ticks, warmup)` admits budgets before constructing any benchmark lane; `ProofEngine.Divergent(first, second)` — indices past the shorter side report as mismatches, so a dropped or extra tail receipt never hides behind pairwise truncation, and `Diagnostics/devloop.md`'s cross-machine verify composes this same walk.
- Auto: derived specs execute on the shared `HeadlessUnitTestSession` through `GetOrStartForAssembly` once per assembly and `Dispatch` per spec — a session that cannot start refuses as `ProofFault.SessionUnavailable` at the acquisition, never as an untyped throw; `FakeTimeProvider` time travel fills the headless row's virtual-time slot; `Replay` drives the journal through the one remote-invocation route on the frozen deck, so journal replay, deep links, and interactive execution seal the same receipt family; the snapshot store rehydrates screen state before the first journal entry, so replay is deterministic end to end; the pointer-walk and drag-drop checks drive synthetic input through `HeadlessWindowExtensions.MouseDown`/`MouseMove`/`MouseUp`/`MouseWheel` between `ForceRenderTimerTick` advances, the drag-drop check driving `DragDrop` in the load-bearing `DragEnter` → `DragOver` → `Drop` sequence (a `DragOver` without a prior `DragEnter` seeds no drop context), the resulting effect read from `DragEventArgs.DragEffects` inside the handler; benchmark lanes derive from the same `HeadlessLane` the proof matrix crosses, so a screen added to the catalog gains its frame benchmark with zero roster edit.
- Receipt: every executed spec seals its `EvidenceReceipt` through the `Diagnostics/evidence.md` union — disposal-leak audits ride the Disposal case and render checks ride the Render case.
- Packages: Avalonia.Headless, Avalonia.Headless.XUnit, Avalonia.Skia, Verify.XunitV3, CsCheck (testkit), Microsoft.Extensions.TimeProvider.Testing, Thinktecture.Runtime.Extensions, Rasm (kernel `CapabilitySet`), LanguageExt.Core, BCL inbox
- Growth: one check row sweeps every headless screen whose `Checks` column admits it, one grid cell sweeps every check, one `RenderHashLane` cell sweeps every key×scale×gamut combination, and one `BenchLane` cell sweeps every headless row at its budgets; zero new surface.
- Boundary: the derivation engine deletes hand-written per-screen smoke specs — a bespoke screen spec beside the engine is the named defect; every lane rides the ONE shared session, because `StartNew` composes the assembly's `[AvaloniaTestApplication]` entry point and only DEFAULTS the headless WINDOWING subsystem where that entry point selected none — the entry point's own `UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })` and `UseSkia` selections both survive into the session the render-hash lanes draw under; a second render-proof `AppBuilder` beside that session is the rejected form and the `Shell/hosts#HOST_AXIS` one-setup guard throws process-wide on the second admission; host-bound screens exit the matrix structurally through the catalog's headless lane, never through skipped specs.
- Law: `ScreenCatalogRow.Checks : CapabilitySet<ProofCheck>` is the applicability RELATION seated on the catalog row (`Shell/screens.md`), so which checks a screen admits reads off the roster a maintainer edits rather than off a predicate closure at every derivation call.

```csharp signature
// --- [TYPES] --------------------------------------------------------------------------------
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

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record ProofSpec(
    string ScreenId,
    ProofCheck Check,
    ThemeVariantRow Variant,
    DensityRow Density,
    Func<IO<EvidenceReceipt>> Run);

// One frame-benchmark cell per headless catalog row: samples, ticks, and the warm-up discard are lane data, so
// a multi-frame or animation-settled benchmark pins its budget as columns and never wall time. Warm-up rides
// the CASE key — a warmed and a cold protocol are two measurement contracts, so a lane adopting a warm-up
// re-baselines its held claim rather than comparing across protocols.
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

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class ProofEngine {
    public static Fin<Seq<BenchLane>> Bench(ScreenCatalog catalog, int samples, int ticks, int warmup) =>
        catalog.HeadlessLane.TraverseM(row => BenchLane.Of(row.Id, samples, ticks, warmup)).As();

    internal static IO<Unit> Advance(int ticks) =>
        IO.lift(() => AvaloniaHeadlessPlatform.ForceRenderTimerTick(ticks));

    // Each headless row crosses only the checks its OWN capability column admits — the relation lives on the
    // catalog row, so no applicability predicate threads through the call.
    public static Seq<ProofSpec> Derive(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        catalog.HeadlessLane.Bind(row =>
            grid.Bind(cell =>
                toSeq(ProofCheck.Items).Filter(check => row.Checks.Admits(check)).Map(check =>
                    new ProofSpec(row.Id, check, cell.Variant, cell.Density, probe(row, check, cell.Variant, cell.Density)))));

    // Session acquisition is the one producer of SessionUnavailable — a session that cannot start names itself
    // instead of throwing untyped out of the dispatcher hop; the spec body's own RunAsync throw stays the ProofFault-coded rail liftAsync re-admits.
    public static IO<EvidenceReceipt> Dispatch(ProofSpec spec) =>
        (IO.lift(() => HeadlessUnitTestSession.GetOrStartForAssembly(typeof(ProofEngine).Assembly))
            | @catch<IO, HeadlessUnitTestSession>(static _ => true,
                static error => IO.fail<HeadlessUnitTestSession>(error)))
        .Bind(session => IO.liftAsync(async () => await session
            .Dispatch(() => spec.Run().RunAsync().AsTask(), CancellationToken.None)
            .ConfigureAwait(false)));

    // The journal carries the RECORDED caller beside its key and payload: a replay that re-labelled every
    // captured operator gesture as its own modality would hand the suite mediation a caller nothing did,
    // and the AppHost event log would chain a provenance the original run never had.
    public static IO<Seq<DeckReceipt>> Replay(
        CommandDeck deck,
        Seq<(string Key, JsonElement Payload, CallerModality Caller)> journal,
        Func<IO<Unit>> restore) =>
        restore().Bind(_ => journal.TraverseM(entry => deck.Invoke(entry.Key, entry.Payload, entry.Caller)).As());

    // The ONE index-divergence walk the replay lanes and the devloop cross-machine verify share: indices past
    // the shorter side report as mismatches, so a dropped or extra tail never hides behind pairwise truncation.
    public static Seq<int> Divergent(Seq<string> first, Seq<string> second) =>
        toSeq(Enumerable.Range(0, Math.Max(first.Count, second.Count)))
            .Filter(index => index >= first.Count || index >= second.Count || first[index] != second[index])
            .Strict();
}
```

## [04]-[PROOF_LAW]

- Owner: `ProofLaw` — the law-matrix fence surface composing `ProofEngine` with CsCheck property generators, `Verify.XunitV3` FrameHash equality, the `MetricCollector<T>` instrument lane, the frame-bench gate minting estate `BenchmarkReceipt` evidence, the bundle-tree pin, the `Theme/tokens.md` shipped-roster conformance fold, and the suite-hygiene gates over the golden registry.
- Entry: `ProofMatrix(...)` — the one entrypoint owning the singular-cell and full-matrix run by input shape, so a per-spec screenshot helper is the deleted form.
- Auto: `RenderHashGrid` generates cells from the live headless catalog crossed with admitted scale, `VisualCodec.ColorPolicy`, and `RenderPosture` data, so a new screen, gamut, or surface-class text reading expands proof without a named roster edit; `FrameHashEquality` seals one generated cell through `Captures.Shot` then `Verifier.Verify`; `ReplayDeterminism` restores the same snapshot before each journal run, resets virtual time, rejects unequal receipt counts before pairing, and verifies the complete digest sequence; `FrameCost` requires a baseline for every pass and ACCUMULATES every regressed pass, so a second regression never hides behind the first; `InstrumentFold` mounts contributions, resolves the named handle off the mounted roster, and brackets the collector so writes and observable-gauge reads share one cell family and a refused measurement fails the fold's own rail rather than reading as a zero sum; `FrameBench` discards the lane's warm-up frames BEFORE the allocation bracket, samples between forced ticks, hands the elapsed spans to `BenchMeasurement.Of` — whose one `Distribution<Elapsed>` reads median, interquartile spread, and the p95 quantile off a single sort — mints one Unjudged `BenchmarkReceipt` under `HostFingerprint.Current`, and composes `BenchmarkGate.Gate` over the held claim and sink; `Divergence` buckets the fresh-versus-held median ratio under `Buckets.DivergenceRatio`; `BundleShape` pins the exported support archive as two goldens that BOTH report.
- Packages: Verify.XunitV3, CsCheck, Avalonia.Headless, Microsoft.Extensions.Diagnostics.Testing, Rasm.AppHost (project, seam types), Rasm (kernel `Custody`/`UnitInterval`), NodaTime, LanguageExt.Core
- Growth: one lane cell absorbs a new golden; one benchmark claim is one held `BenchmarkReceipt` value; zero new surface.
- Law: the `RenderHashGrid` FrameHash golden bytes derive under the `libs/contracts/manifest.json` `content-identity` framing and seed law and stay a .NET-tree snapshot no peer runtime binds — the render-hash lane is the one host golden producer; the property lanes take a DECLARED seed, so a red run replays byte-for-byte instead of reproducing by luck.
- Law: the proof fence is a terminal edge — `IO<A>.Run()`/`RunAsync()` THROW the typed `Error` on failure, so a failing disposition composes BEFORE the terminal, and the `@catch` recovery on a property lane narrows to the proof band so a bug outside it surfaces instead of reading as a clean `false` verdict.
- Law: the frame-bench lane composes the AppHost benchmark rail as settled vocabulary — `BenchMeasurement.Of`, `BenchmarkReceipt.Of`, `BenchmarkGate.Gate`, and `GatePolicy.Canonical` mint and judge over the spine's own `HostFingerprint`, the held claim arrives as a value off the Persistence reuse index, and the judged receipt fans through the sink under the AppHost benchmark kind; the allocation delta reads a PROCESS-WIDE counter, so bench lanes run serially and no parallel proof cell overlaps the bracket.
- Boundary: `VerifyZip`/`VerifyDirectory` pin support-bundle roster and tree completeness, and the extracted `manifest.json` carries the AppHost `SupportManifest.Entry` `ContentKey` column, so content identity pins in the same golden pair rather than a re-hash of the zip.

```csharp signature
// --- [MODELS] -------------------------------------------------------------------------------
// The cell key spells every raster-deciding input the golden was taken under, so a break names the axis it
// moved on and a Golden-posture screen cell and a Paged-posture export cell are two goldens rather than one file two lanes overwrite in turn.
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
    // Both hygiene gates and the registry soundness read on one entry; WhenAll lets a second failure report beside the first instead of hiding behind it.
    public static async Task SuiteHygiene() {
        GoldenLanes.Sound().ThrowIfFail();
        await Task.WhenAll(
            VerifyChecks.Run(),
            Task.Run(static () => DanglingSnapshots.Run()));
    }

    // The golden pins BOTH hashes, so the snapshot itself records whether a later break moved pixels alone or
    // moved draw ops; a recordless lane pins DrawHash as None and its golden stays honest about carrying no
    // attribution. IO.RunAsync throws the typed Error — the runner's loud failure IS the ProofFault-coded throw.
    public static async Task FrameHashEquality(VisualRuntime runtime, RenderHashLane lane, FrameGrab grab) {
        RenderReceipt receipt = await lane.Row(grab)
            .Match(Succ: row => Captures.Shot(runtime, row), Fail: IO.fail<RenderReceipt>)
            .RunAsync();
        await Verifier.Verify(new { lane.Cell, receipt.FrameHash, receipt.DrawHash, receipt.ColorSpace })
            .UniqueForTargetFramework()
            .UseTextForParameters(lane.Cell);
    }

    // The seed is DECLARED, so a red property replays byte-for-byte; the @catch narrows to the proof band, so a
    // bug outside it surfaces as itself rather than reading as a clean determinism failure.
    public static void DeterministicCapture(VisualRuntime runtime, Seq<RenderHashLane> lanes, FrameGrab grab, string seed) =>
        LaneGen(lanes).Sample(lane =>
            (lane.Row(grab).Match(
                Succ: row => Captures.Shot(runtime, row)
                    .Bind(first => Captures.Shot(runtime, row)
                        .Map(second => first.FrameHash == second.FrameHash && first.DrawHash == second.DrawHash)),
                Fail: IO.fail<bool>)
                | @catch<IO, bool>(static error => error is ProofFault, static _ => IO.pure(false))).As().Run(),
            seed: seed);

    public static IO<Seq<EvidenceReceipt>> ProofMatrix(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        ProofEngine.Derive(catalog, grid, probe).TraverseM(ProofEngine.Dispatch).As();

    // One composed IO: both replays, both virtual-time resets, and the shape gate ride the rail; the single
    // terminal RunAsync throws the ProofFault.ReplayShape-coded Error on divergence. FakeTimeProvider's
    // timestamps derive from its own clock, so the SetUtcNow reset covers the monotonic reads too.
    public static async Task ReplayDeterminism(
        CommandDeck deck,
        Seq<(string Key, JsonElement Payload, CallerModality Caller)> journal,
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
    // The mount precedes the collector, which binds the already-published Instrument<long> pulled off the
    // mounted roster; an empty envelope set and a name mounted under another measurement type both refuse by
    // name BEFORE the collector exists, because a vacuous sum and a long-typed read of a Real row both render as a passing zero.
    public static Fin<long> InstrumentFold(
        IMeterFactory factory, string version, CorrelationId root,
        LevelCells cells, Seq<TelemetryContributorPort> contributions,
        string instrument, Seq<ReceiptEnvelope> envelopes) =>
        envelopes.IsEmpty
            ? Fin.Fail<long>(new ProofFault.SessionUnavailable($"proof/instrument: {instrument} folded no envelopes"))
            : AppUiTelemetry.Mount(factory, version, root, cells, contributions).Bind(set =>
                set.Mounts.Find(seat => seat.Row.Name == instrument)
                    .Bind(static seat => seat.Handle is Instrument<long> whole ? Some(whole) : None)
                    .ToFin(Fail: new ProofFault.SessionUnavailable($"proof/instrument: {instrument} is no mounted whole-measure instrument"))
                    .Bind(mounted => EvidenceFan.Fan(set).Bind(fan =>
                        Custody.Bracket(
                            acquire: () => new MetricCollector<long>(mounted),
                            project: collector => envelopes.TraverseM(envelope => EvidenceFan.Project(fan, envelope)).As()
                                .Map(_ => collector.GetMeasurementSnapshot().Sum(static measurement => measurement.Value)),
                            key: Op.Of(name: "proof.instrument")))));

    // Slot conformance rides the SAME live application the accessibility sweep already stands up: Semi.Avalonia
    // compiles its dictionaries into XamlClosure bodies, so IL enumeration yields one opaque blob and the roster
    // is DERIVED per run off the live Application.Current.Styles chain — a scratch roster captured at authoring
    // time is palette-only AND freezes a vocabulary the next bump moves while reading as a pass.
    public static Fin<Unit> SemiConformance(ResolvedTheme resolved, Seq<IStyle> chain) =>
        SemiCorrespondence.SemiCovered(resolved, SemiRoster.Walk(chain));

    // Cost lane ACCUMULATES: every pass compares variance-aware against its baseline and the frame budget, and
    // every regressed pass reports — a traverse that stopped at the first regression would hide the second.
    public static Fin<Seq<(string Pass, Duration Elapsed)>> FrameCost(
        FrameReceipt receipt, HashMap<string, Duration> baseline, FrameBudget budget, UnitInterval variance) =>
        receipt.Passes.Traverse(pass =>
            baseline.Find(pass.Pass).Match(
                Some: known => pass.Elapsed <= budget.Frame && pass.Elapsed.TotalTicks <= known.TotalTicks * (1.0 + variance.Value)
                    ? Validation<Error, (string, Duration)>.Success(pass)
                    : Validation<Error, (string, Duration)>.Fail((Error)new ProofFault.CostRegressed(pass.Pass, known.ToString(), pass.Elapsed.ToString())),
                None: () => Validation<Error, (string, Duration)>.Fail((Error)new ProofFault.BaselineAbsent(pass.Pass))))
            .As().ToFin();

    // Divergence magnitude on the shared advice axis: the fresh-versus-held median ratio lands on the first
    // covering DivergenceRatio edge; a zero-length held median or non-regression folds to zero.
    public static double Divergence(BenchmarkReceipt fresh, BenchmarkReceipt held) =>
        held.Measured.Figures.Median.To() <= 0d ? 0d
            : (fresh.Measured.Figures.Median.To() / held.Measured.Figures.Median.To() - 1d) switch {
                <= 0d => 0d,
                var ratio => toSeq(Buckets.DivergenceRatio)
                    .Find(edge => ratio <= edge)
                    .IfNone(Buckets.DivergenceRatio[^1]),
            };

    // --- [BENCH]
    public const string BenchSuite = "rasm.appui.frame";

    // Warm-up frames run BEFORE the allocation bracket, so cold-JIT cost enters neither the spans nor the
    // delta. The counted traverse COLLECTS a receipt per sample — Schedule.recurs drives repetition of one
    // effect and discards its per-pass values, which is why the range fold stays.
    public static IO<BenchmarkReceipt> FrameBench(
        BenchLane lane,
        Func<IO<FrameReceipt>> frame,
        CorrelationId correlation,
        Option<UInt128> corpus,
        Option<BenchmarkReceipt> claim,
        ReceiptSinkPort sink) =>
        from _warm in toSeq(Enumerable.Range(0, lane.Warmup))
            .TraverseM(_ => ProofEngine.Advance(lane.Ticks).Bind(_ => frame()))
            .As()
        from before in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        from frames in toSeq(Enumerable.Range(0, lane.Samples))
            .TraverseM(_ => ProofEngine.Advance(lane.Ticks).Bind(_ => frame()))
            .As()
        from after in IO.lift(() => GC.GetTotalAllocatedBytes(precise: true))
        // One ascending nanosecond array feeds the sorted-array owner, so both order statistics read off the
        // same sort; QuantileCustom under R1 (EmpiricalInvCDF) is the exact ceiling nearest-rank order statistic the branch ruling pins.
        from measured in IO.lift(() => BenchMeasurement.Of(
            spans: frames.Map(static receipt => receipt.Passes.Fold(Duration.Zero, static (total, pass) => total + pass.Elapsed)),
            allocatedBytes: after - before,
            operations: frames.Count,
            key: Op.Of()))
        // Host identity is the spine's six intrinsic columns whole; the headless lane's own posture keys the
        // CASE, so a stamp naming it here would split one host across two fingerprints.
        let fresh = measured.Map(figures => BenchmarkReceipt.Of(
            suite: BenchSuite,
            @case: lane.Case,
            corpus: corpus,
            measured: figures,
            correlation: correlation,
            stamps: FrozenDictionary<string, string>.Empty))
        from gate in fresh.Match(
            Succ: row => BenchmarkGate.Gate(sink, row, claim, GatePolicy.Canonical, Op.Of()),
            Fail: fault => IO.pure(Validation<Error, BenchmarkReceipt>.Fail((BenchmarkFault)fault)))
        from judged in gate.Match(Succ: IO.pure, Fail: static faults => IO.fail<BenchmarkReceipt>(faults.Head))
        select judged;
}
```

## [05]-[GUARD_REGISTRY]

- Owner: `GoldenLane` with `GoldenLanes` — the committed-golden roster as CONSTRUCTED rows, each naming its pinned artifact beside the entry that writes it, with the `Sound` uniqueness fold the suite-hygiene gate reads; `SkewVerdict` `[SmartEnum<string>]` — the three-value guard disposition; `SkewGuard` with `SkewGuards` — the per-pair guard rows carrying witnessed verdict, resolution, and the entry that RE-PROVES on a bump, beside the two live re-provable folds.
- Cases: `SkewVerdict` = binds | covers | fails — `binds` is a cross-boundary type crossing that loaded and invoked, `covers` is a themable-surface roster that resolved whole, `fails` is a load-time refusal whose resolution is the DROP recorded beside it.
- Entry: `SkewGuards.DockTheming(host, controls, variants)` — refuses with the WHOLE asymmetric-gap set as a typed `ProofFault.DockCoverage`, so no gap hides past a message-format cap; `SkewGuards.SkiaBoundary(context)` — leases the Skia api feature at the live render boundary under `Custody.Bracket` and refuses each crossing BY NAME on an accumulating admission.
- Auto: a guard row is DATA — the pair, the mechanism, the witnessed verdict, and the resolution are columns, and a row whose `Guard` column names an entry re-proves on every bump of either side; the two provable guards ride the settled proof rails and mint nothing — dock coverage is a resource read over the live theme (the sweep passes each `ThemeVariant` explicitly), and the Skia boundary is one lease off the same `ISkiaSharpApiLeaseFeature` the `Vfx` material route composes; a dropped package carries no `Guard` entry because there is nothing left to bind.
- Receipt: a guard refusal is a typed `ProofFault` on the proof rail like every other lane, so a skew break reports beside a pixel drift rather than as a build-time surprise.
- Packages: Avalonia, Avalonia.Skia, SkiaSharp, Semi.Avalonia.Dock, Dock.Avalonia, Thinktecture.Runtime.Extensions, Rasm (kernel `Custody`), LanguageExt.Core
- Growth: a new golden is one `GoldenLanes` row naming its writer; a new cross-package pair is one `SkewGuards` row and, where the pair is bindable, one fold beside the two here; zero new surface.
- Law: the registry is the reverse index from a committed artifact to the entry that writes it — a golden with no row is an orphan file and a row with no writer is a lane nothing proves; `LayoutWireGolden.Canonical` lives in the AppUI test package because it asserts the deterministic `Shell/solver#TS_PROJECTION` canonical serialization, and this registry NAMES it rather than re-spelling it, since a second snapshot over the same wires would commit a second file that drifts silently from the first.
- Law: the three package rows record verdicts witnessed on the assay bind-and-invoke rail, and a verdict is never a version pin — the Skia pair is proven at the render boundary rather than pinned, because the framework's declared dependency is a FLOOR the loader satisfies with a higher major under a matching public key, so the guard is the re-run.
- Boundary: the validation package is DROPPED estate-wide rather than pinned back — the manifest and registry drop lands at the package owners and its row records the verdict and the resolution; the dock skin carries zero keys in its own theme dictionaries and inherits every light/dark decision from the base Semi dictionaries, so the standing obligation is VARIANT COHERENCE — the two Semi packages move to one variant vocabulary together while the dock skin may lag the dock CONTROL package freely (`Shell/navigation#DOCK_LAYOUTS` states the same obligation at the consuming boundary).

```csharp signature
// --- [TABLES] -------------------------------------------------------------------------------
public sealed record GoldenLane(string Lane, string Pins, string Writer);

// The roster is CONSTRUCTED data — writers derive from the entry names, so a renamed law entry breaks the row
// at compile time; Sound proves lane uniqueness and non-blank writers where the suite-hygiene gate reads it.
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

// A guard row is DATA and its `Guard` column is the RE-PROOF: a witnessed verdict answers what held at the
// version it was taken under, and only an entry that runs again answers what holds after the next bump. A
// dropped package carries `None` — the resolution IS the removal.
public sealed record SkewGuard(string Pair, string Mechanism, SkewVerdict Verdict, string Resolution, Option<string> Guard);

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class SkewGuards {
    public static readonly Seq<SkewGuard> Rows = Seq(
        new SkewGuard("validation rail over reactive major", "restore graph read, then bind-invoke", SkewVerdict.Fails,
            "dropped estate-wide; rail rebuilt owned", None),
        new SkewGuard("dock skin over dock controls", "control-theme resolution per variant", SkewVerdict.Covers,
            "re-proves on every bump of either side", Some(nameof(DockTheming))),
        new SkewGuard("framework Skia over pinned stack", "lease and invoke at render boundary", SkewVerdict.Binds,
            "re-proves on every bump of either side", Some(nameof(SkiaBoundary))));

    // Coverage answers per (type, variant) cell: the skin defines no variant-local values of its own, so a key
    // resolving under one variant and not the other is exactly the incoherence version proximity cannot see. A
    // control needing no theme carries none in EITHER variant and drops out, so only asymmetric gaps report — and the WHOLE gap set rides the typed case.
    public static Fin<Unit> DockTheming(IResourceHost host, Seq<Type> controls, Seq<ThemeVariant> variants) =>
        controls
            .Map(control => (Control: control,
                Resolved: variants.Filter(variant => host.TryFindResource(control, variant, out object? theme) && theme is ControlTheme)))
            .Filter(row => !row.Resolved.IsEmpty && row.Resolved.Count < variants.Count)
            .Map(static row => row.Control.Name) switch {
            { IsEmpty: true } => Fin.Succ(unit),
            var gaps => Fin.Fail<Unit>(new ProofFault.DockCoverage(gaps.Strict())),
        };

    // The declared graphics dependency is a FLOOR the loader satisfies with a higher major, so a green restore
    // proves nothing and the only honest guard is the crossing itself. Each probe refuses BY NAME, so a failure
    // says WHICH crossing broke; an absent feature IS the answer on a non-Skia backend.
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
