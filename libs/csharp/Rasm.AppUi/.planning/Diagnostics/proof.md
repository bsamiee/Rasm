# [APPUI_DIAGNOSTICS_PROOF]

Rasm.AppUi proof is one derivation engine: host-agnostic capture rows prove pixels by content hash, a catalog-derived matrix crosses every headless screen with every check and every variant-density cell, command journals replay under virtual time, and the law-matrix fence seals every cell through CsCheck properties and Verify goldens. The page owns the capture and proof row families, the derivation and replay engine, the render-hash law surface, and the typed `ProofFault` rail — composing the `Diagnostics/evidence.md` union for every sealed receipt.

## [01]-[INDEX]

- [02]-[CAPTURE_LANES]: Host-agnostic frame capture rows; render-hash regression proof.
- [03]-[HEADLESS_DERIVATION]: Catalog-derived proof matrix; deterministic command-journal replay.
- [04]-[PROOF_LAW]: The law-matrix fence — FrameHash equality, deterministic capture, replay determinism.

## [02]-[CAPTURE_LANES]

- Owner: `ProofFault` — the typed proof rail; `CaptureRow` — the per-surface capture row carrying the DPI-scale column; `Captures` — the shot-and-regression surface.
- Entry: `public static IO<RenderReceipt> Shot(VisualRuntime runtime, CaptureRow row)` — `IO` rail through the settled encode fold; one PNG artifact plus one render receipt per shot.
- Auto: capture keys prefix into the per-run artifact scope behind the runtime blob delegate, so a shot never computes a path; the `Scale` column pins the headless render scaling through `SetRenderScaling` so a hi-DPI baseline keys distinctly from its standard-scale twin; the `Ticks` column folds that many `ForceRenderTimerTick` advances into one deterministic frame effect before the grab, so a single-frame baseline pins `Ticks: 1` and an animation-settled or multi-frame capture pins its own count as data and never wall time; the receipt's `FrameHash` rides the suite content-hash identity row (the kernel `ContentHash.Of` delegate the capture runtime binds).
- Receipt: a regression divergence is a typed `ProofFault.HashDiverged` whose `Code` derives through the `Diagnostics/evidence.md#FAULT_TABLES` `AppUiFaultBand.Proof` row (6700); a bare `Error.New` on this rail is the deleted form.
- Packages: SkiaSharp, Avalonia.Headless, Avalonia.Skia, Thinktecture.Runtime.Extensions, LanguageExt.Core
- Growth: one capture row absorbs a new surface lane; one `Scale` value on a row absorbs a new DPI baseline; one `ProofFault` case is one `detail` ordinal under the 6700 row; zero new surface.
- Boundary: grab delegates bind at composition per surface row and no capture member is named outside its own row — the headless lane rides the `WriteableBitmap? CaptureRenderedFrame(this TopLevel)` and `WriteableBitmap? GetLastRenderedFrame(this TopLevel)` extensions whose `WriteableBitmap` pixels enter the hash fold through `Lock()` over the `ILockedFramebuffer` (`Address`, `RowBytes`, `Size`, `Format`), an un-shown top-level returning a null frame folds to an absent grab rather than a throw, with `UseHeadlessDrawing` false selecting the Skia backend on every hash lane and `SetRenderScaling(this TopLevel, double)` pinning the device scale before the grab (it throws `ArgumentOutOfRangeException` on a non-positive scale, so the row `Scale` stays positive) so the render-hash is scale-attributable; the custom-visual lane is a RECEIPT lane, not a grab lane — `CustomVisual.Materialize(VisualRuntime, CustomVisualData, SKImageInfo, ColorSpaceAxis)` already returns `IO<RenderReceipt>` with the encode and the `FrameHash` inside, so the custom-tile cell feeds that receipt straight into `Regression` keyed by `key@scale×gamut` under its `ColorSpaceAxis` cell (the keyed projection of `VisualCodec.ColorPolicy`), a second `CaptureRow.Grab` contract over the same cell is the deleted duplicate, a wide-gamut custom tile hashes its float or ICC-tagged pixels and never a quantized sRGB shadow, and the render-hash regression attributes a custom-tile pixel drift to the exact kind, scale, and gamut cell; the rhino lane rides the settled host viewport capture port; the desktop in-tree lane renders through `RenderTargetBitmap.Render(Visual)` with `CopyPixels(PixelRect, nint, int, int)` as its pixel projection, or evaluates a live visual onto a leased Skia canvas through `DrawingContextHelper.RenderAsync` where the in-tree row already holds a render lease so the capture composes the visual into the encode fold without a second offscreen surface; `ForceRenderTimerTick` is the only frame-advance verb on the deterministic lane — a debounce or animation that fails under forced ticks has smuggled wall time, and the tick count is a row column so a multi-frame capture is data; `Regression` compares `FrameHash` values from the settled receipt family, so a per-spec screenshot helper is the deleted form and a second baseline store beside the blob lane is the rejected form.

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
}

public sealed record CaptureRow(string Key, double Scale, int Ticks, Func<double, Func<IO<Unit>>, IO<SKImage>> Grab) {
    public IO<SKImage> Shoot() =>
        Grab(Scale, () => Range(0, int.Max(Ticks, 1))
            .Fold(IO.pure(unit), static (rail, _) => rail.Bind(static _ => IO.lift(AvaloniaHeadlessPlatform.ForceRenderTimerTick))));
}

public static class Captures {
    public const string Kind = "capture";

    public static IO<RenderReceipt> Shot(VisualRuntime runtime, CaptureRow row) =>
        row.Shoot().Bind(image =>
            VisualCodec.Encode(runtime, image, VisualCodec.Png, Kind, $"captures/{row.Key}@{row.Scale}x.png"));

    public static Fin<RenderReceipt> Regression(RenderReceipt actual, string baseline) =>
        actual.FrameHash == baseline
            ? Fin.Succ(actual)
            : Fin.Fail<RenderReceipt>(new ProofFault.HashDiverged(actual.Kind, actual.FrameHash, baseline));
}
```

## [03]-[HEADLESS_DERIVATION]

- Owner: `ComparerAccessors.StringOrdinal` accessor; `ProofCheck` — the ten-row check vocabulary; `ProofSpec` — the derived spec row; `ProofEngine` — the derivation and replay surface; `RenderHashLane` — the `key@scale×gamut` render-hash cell.
- Cases: activation, render-hash, focus-walk, variant-sweep, density-sweep, disposal-leak, pointer-walk, drag-drop, contrast-audit, frame-cost — the two input-proof rows drive the headless synthetic-input verbs, the contrast-audit row sweeps the `Shell/accessibility.md` WCAG luminance gate over every variant-density cell so an inaccessible pairing fails the matrix like a pixel drift, and the frame-cost row proves per-pass render cost against the `FrameBudget` so a pass that doubles its cost fails the matrix like a pass that changes its pixels.
- Entry: `public static Seq<ProofSpec> Derive(ScreenCatalog catalog, Seq<(ThemeVariantRow Variant, DensityRow Density)> grid, Func<ScreenCatalogRow, ProofCheck, bool> applies, Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe)` — each headless row crosses only the checks its composition-bound applicability relation admits, then spans every variant-density cell.
- Auto: derived specs execute on the shared `HeadlessUnitTestSession` through `GetOrStartForAssembly` once per assembly and `Dispatch` per spec, so every spec runs on the one UI thread without a per-spec session boot, and `[AvaloniaFact]` dispatch under the xunit.v3 MTP runner rides the same session; `FakeTimeProvider` time travel fills the headless row's virtual-time slot; `Replay` drives the journal through the one remote-invocation route on the frozen deck, so journal replay, deep links, and interactive execution seal the same receipt family; the snapshot store rehydrates screen state before the first journal entry, so replay is deterministic end to end; the pointer-walk and drag-drop checks drive synthetic input on the session top-level through `HeadlessWindowExtensions.MouseDown(this TopLevel, Point, MouseButton, RawInputModifiers)`/`MouseMove(this TopLevel, Point, RawInputModifiers)`/`MouseUp(this TopLevel, Point, MouseButton, RawInputModifiers)`/`MouseWheel(this TopLevel, Point, Vector, RawInputModifiers)` between `ForceRenderTimerTick` advances, the drag-drop check driving `DragDrop(this TopLevel, Point, RawDragEventType, IDataTransfer, DragDropEffects, RawInputModifiers)` (void return) in the load-bearing `DragEnter` → `DragOver` → `Drop` sequence (`RawDragEventType` from `Avalonia.Input.Raw`) because a `DragOver` without a prior `DragEnter` seeds no drop context and raises no routed handler, the resulting effect read from `DragEventArgs.DragEffects` inside the handler and never a return value, so a pointer-interaction or drop-target proof is a deterministic frame sequence and never wall-time hover timing.
- Receipt: every executed spec seals its `EvidenceReceipt` through the `Diagnostics/evidence.md` union — disposal-leak audits ride the Disposal case and render checks ride the Render case.
- Packages: Avalonia.Headless, Avalonia.Headless.XUnit, Avalonia.Skia, Verify.XunitV3, CsCheck (testkit), Microsoft.Extensions.TimeProvider.Testing (FakeTimeProvider), Thinktecture.Runtime.Extensions, LanguageExt.Core, BCL inbox
- Growth: one check row sweeps every headless screen, one grid cell sweeps every check, and one `RenderHashLane` cell sweeps every key×scale×gamut combination; zero new surface.
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

public static class ProofEngine {
    public static Seq<ProofSpec> Derive(
        ScreenCatalog catalog,
        Seq<(ThemeVariantRow Variant, DensityRow Density)> grid,
        Func<ScreenCatalogRow, ProofCheck, bool> applies,
        Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        catalog.HeadlessLane.Bind(row =>
            grid.Bind(cell =>
                toSeq(ProofCheck.Items).Filter(check => applies(row, check)).Map(check =>
                    new ProofSpec(row.Id, check, cell.Variant, cell.Density, probe(row, check, cell.Variant, cell.Density)))));

    public static IO<EvidenceReceipt> Dispatch(ProofSpec spec) =>
        IO.liftAsync(async () => await HeadlessUnitTestSession
                .GetOrStartForAssembly(typeof(ProofEngine).Assembly)
                .Dispatch(() => spec.Run().RunAsync().AsTask(), CancellationToken.None)
                .ConfigureAwait(false))
            .Bind(static settled => settled.Match(Succ: IO.pure<EvidenceReceipt>, Fail: IO.fail<EvidenceReceipt>));

    public static IO<Seq<CommandReceipt>> Replay(CommandDeck deck, Seq<(string Key, JsonElement Payload)> journal) =>
        journal.TraverseM(entry => deck.Invoke(entry.Key, entry.Payload)).As();
}
```

## [04]-[PROOF_LAW]

- Owner: `ProofLaw` — the law-matrix fence surface composing `ProofEngine` with CsCheck property generators, `Verify.XunitV3` FrameHash equality, and the `VerifyChecks`/`DanglingSnapshots` suite-hygiene gates.
- Entry: `public static IO<Seq<EvidenceReceipt>> ProofMatrix(...)` — the one entrypoint that owns the singular-cell and full-matrix run by input shape so a per-spec screenshot helper is the deleted form.
- Auto: `ProofLaw.FrameHashEquality` seals one `key@scale×gamut` cell through `Captures.Shot` then `Verifier.Verify` so a render-hash regression attributes to the exact cell; `ProofLaw.DeterministicCapture` is the CsCheck property that two captures of one lane hash identically (a debounce or animation that smuggles wall time fails it), folding both terminal `Fin` results on the rail so a failed capture fails the property instead of vanishing; `ProofLaw.ReplayDeterminism` replays the journal twice under `FakeTimeProvider.SetUtcNow(UnixEpoch)` and `Verifier.Verify`-equals the two payload-digest seqs; `ProofLaw.FrameCost` compares each `FrameReceipt` pass duration against its baseline under a variance fraction and the `FrameBudget` ceiling, so a pass whose cost regressed past tolerance folds a typed `ProofFault.CostRegressed` — pixel goldens prove what a frame looks like, this lane proves what it costs; `ProofLaw.SuiteHygiene` awaits `VerifyChecks.Run()` once per suite and sweeps `DanglingSnapshots.Run()` in the CI cleanup pass, so a misconfigured verify pipeline and an orphaned `.verified.` golden are suite-gate failures, never silent corpus drift as the render-hash grid grows per catalog row.
- Packages: Verify.XunitV3, CsCheck, Avalonia.Headless, LanguageExt.Core
- Growth: one lane cell absorbs a new golden; zero new surface.
- Boundary: the `RenderHashGrid` FrameHash golden bytes are the C#-host-validated leg of the content-addressed kernel-hasher-keyed ONE_WIRE_FIXTURE_CORPUS — the render-hash lane is the host golden producer the cross-runtime consumers read, never a second golden store; the headless capture lanes are the parity oracle for every `[V6]` fence repair — a repaired fence proves itself here before the campaign closes; gamut cells key by `ColorPolicy` rows (the `Render/capture.md` one gamut/transfer family); the proof fence is a terminal edge, so every `Run`/`RunAsync` lands on `Fin` and its disposition is explicit — `ThrowIfFail` collapses a capture or replay failure into the loud typed `ProofFault`-coded error the runner reports, `IfFail(false)` fails the property, and an assignment reading the inner value straight off the terminal result is the rejected form that neither compiles nor represents failure.

```csharp signature
public readonly record struct RenderHashLane(string Key, double Scale, string Gamut, int Ticks) {
    public string Cell => $"{Key}@{Scale}x{Gamut}";

    public CaptureRow Row(Func<double, Func<IO<Unit>>, IO<SKImage>> grab) =>
        new(Cell, Scale, Ticks, grab);
}

public static class ProofLaw {
    public static readonly Seq<RenderHashLane> RenderHashGrid = toSeq(
        from key in Seq("viewport", "custom-tile", "drafting-sheet")
        from scale in Seq(1.0, 2.0)
        from gamut in Seq(VisualCodec.ColorPolicy.Display.Key, VisualCodec.ColorPolicy.DisplayP3.Key, VisualCodec.ColorPolicy.Rec2020.Key)
        select new RenderHashLane(key, scale, gamut, Ticks: 1));

    public static async Task SuiteHygiene() {
        await VerifyChecks.Run();
        DanglingSnapshots.Run();
    }

    public static async Task FrameHashEquality(VisualRuntime runtime, RenderHashLane lane, Func<double, Func<IO<Unit>>, IO<SKImage>> grab) {
        RenderReceipt receipt = (await Captures.Shot(runtime, lane.Row(grab)).RunAsync()).ThrowIfFail();
        await Verifier.Verify(new { lane.Cell, receipt.FrameHash, receipt.ColorSpace })
            .UniqueForTargetFramework()
            .UseTextForParameters(lane.Cell);
    }

    public static Gen<RenderHashLane> LaneGen => Gen.OneOfConst([.. RenderHashGrid]);

    public static void DeterministicCapture(VisualRuntime runtime, Func<double, Func<IO<Unit>>, IO<SKImage>> grab) =>
        LaneGen.Sample(lane =>
            Captures.Shot(runtime, lane.Row(grab)).Run()
                .Bind(first => Captures.Shot(runtime, lane.Row(grab)).Run()
                    .Map(second => first.FrameHash == second.FrameHash))
                .IfFail(static _ => false));

    public static IO<Seq<EvidenceReceipt>> ProofMatrix(ScreenCatalog catalog, Seq<(ThemeVariantRow Variant, DensityRow Density)> grid, Func<ScreenCatalogRow, ProofCheck, bool> applies, Func<ScreenCatalogRow, ProofCheck, ThemeVariantRow, DensityRow, Func<IO<EvidenceReceipt>>> probe) =>
        ProofEngine.Derive(catalog, grid, applies, probe).TraverseM(ProofEngine.Dispatch).As();

    public static async Task ReplayDeterminism(CommandDeck deck, Seq<(string Key, JsonElement Payload)> journal, FakeTimeProvider time) {
        time.SetUtcNow(DateTimeOffset.UnixEpoch);
        Seq<CommandReceipt> first = (await ProofEngine.Replay(deck, journal).RunAsync()).ThrowIfFail();
        time.SetUtcNow(DateTimeOffset.UnixEpoch);
        Seq<CommandReceipt> second = (await ProofEngine.Replay(deck, journal).RunAsync()).ThrowIfFail();
        await Verifier.Verify(first.Map(static r => r.PayloadDigest).Zip(second.Map(static r => r.PayloadDigest)));
    }

    // Cost lane: baseline pass durations compare variance-aware, and every pass stays under the frame
    // budget — a pass past either bound folds its typed fault; a pass with no baseline seeds one.
    public static Fin<Seq<(string Pass, Duration Elapsed)>> FrameCost(
        FrameReceipt receipt, HashMap<string, Duration> baseline, FrameBudget budget, double variance) =>
        receipt.Passes.TraverseM(pass =>
            baseline.Find(pass.Pass).Match(
                Some: known => pass.Elapsed <= budget.Frame && pass.Elapsed.TotalTicks <= known.TotalTicks * (1.0 + variance)
                    ? Fin.Succ(pass)
                    : Fin.Fail<(string, Duration)>(new ProofFault.CostRegressed(pass.Pass, known.ToString(), pass.Elapsed.ToString())),
                None: () => Fin.Succ(pass))).As();
}
```
