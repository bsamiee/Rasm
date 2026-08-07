# [MATERIALS_BENCHMARKS]

KERNEL benchmark identity is a `BenchKernel` row with a `BenchInput` pin and its resolved content key. `Suite` derives as `rasm.materials.<kernel>`, and `Case` carries both the pin token and content key, so catalogue or library content changes fork claim lineage without requiring a new row spelling.

Settled composition: Materials owns workload vocabulary and content-bound identity. `BenchmarkReceipt`, `BenchMeasurement`, `GatePolicy`, and `BenchmarkGate` arrive settled from `Rasm.AppHost/Observability/benchmarks#BENCHMARK_RECEIPT` under the branch benchmark-peer up-reference ruling; `ReceiptSinkPort` and `CorrelationId` arrive from the kernel signal capsule. BenchmarkDotNet binds in the branch bench project and never this package's csproj.

## [01]-[INDEX]

- [02]-[WORKLOAD_ROWS]: `BenchKernel` vocabulary over the section, capacity, appearance, and texture-plane kernels, the `BenchInput` pin union with its extent case over the closed `ProgramPin`, the `BenchPin` resolvers every pin names, and the `BenchWorkload` corpus.
- [03]-[GATE_COMPOSITION]: content-bound corpus, fresh-receipt projection, and corpus gate pass.

## [02]-[WORKLOAD_ROWS]

- Owner: `BenchKernel` `[SmartEnum<string>]` — the measured-kernel vocabulary whose `Suite` column derives the receipt suite; `BenchInput` `[Union]` — the pinned-input shapes; `ProgramPin` `[Union]` — the four closed vocabularies an extent workload names a program from; `BenchPin` — the fallible pin resolvers; `BenchWorkload` — one kernel bound to one pin.
- Cases: `BenchInput.CatalogueLeast` binds the least-designation `Sectioned` row of the named family at composition, so a catalogue reseed shifts the pin deterministically; `BenchInput.LibraryRow` binds one registered `MaterialLibrary` key; `BenchInput.Synthetic` derives a deterministic sample grid from its seed through the owning kernel — the `GgxFit` pin is `Acquisition.SyntheticGrid(seed, count, key)` — `Fin<Seq<BrdfSample>>`, the workload binding its rail at composition so a refused grid fails the pin loud — the stratified goniophotometer capture whose reflectance the microfacet forward model evaluates at seed-derived ground-truth alphas, so the input carries no fixture file and no RNG state outside the seed; `BenchInput.Roster` pins a closed sweep whose size is the vocabulary's own, the golden-fixture prove being the landed instance; `BenchInput.Extent` binds a texel square to a named program, the pair a plane workload sizes by — a library row for a shade or a press, a `RasterFormat` container key for an encode, a sky model for a prefilter, a `TileStrategy` key for a tiling run, a `HeightSolver` key for a height solve — so the measured magnitude and the measured program both enter the case token. Every pin answers `Magnitude`, the count of measurement units it represents, so a throughput read divides that column into the harness duration rather than re-parsing the display token.
- Entry: `MaterialsBench.Corpus(contentKey)` resolves every pin through one injected content-key function; `MaterialsBench.CaseOf` derives the receipt case token from the pin and resolved key through the generated total `Switch`.
- Auto: a pin edit or resolved-content edit changes the case token, so claim lineage forks visibly instead of silently comparing different programs; the interaction sweep pins the reinforcement family because the hull builds from the RC section, the two graph kernels share one library pin so compile and eval measure one program, and the batched-shade and press rows share ONE extent and ONE program so the span rail's cost and the whole bake's cost are two readings of one workload rather than two workloads. The parity row holds that same program and departs on EXTENT ALONE, because its device floor bounds the square it can dispatch — a shared extent it cannot run measures nothing, and the lane comparison the row exists for survives the departure while a refused dispatch does not.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured kernel is one `BenchKernel` row and one `Corpus` row; a new program vocabulary is one `ProgramPin` case breaking `Key`; a new pin shape is one `BenchInput` case breaking BOTH `CaseOf` and `Magnitude` at compile time — `Extent` and `Roster` each landed through exactly that break — and one kernel takes as many `Corpus` rows as it has genuinely distinct cost classes.
- Boundary: a pair of rows on one kernel either SEPARATES cost classes or STRADDLES a routing constant, never repeats one program at two sizes: the `PlaneCodec` and `TileSynth` pairs separate — a managed float container against a spawned block-compression tool, a maximum-flow band solve against a per-texel field fold — while the `HeightSolve` pair straddles `filter#HEIGHT_FIELD`'s own direct-versus-Krylov ceiling by construction, so the constant that routes the solve is the one number this corpus measures rather than asserts. Workload rows pin inputs and derive identity — kernel bodies stay on their owning pages, and a workload never re-implements the kernel it measures. `Magnitude` is the one throughput denominator: the plane rows exist to be read as texels per second at four thousand square, and deriving a rate by splitting the case token binds a reader to a display grammar that owns no numbers. A program is a CLOSED `ProgramPin` case reading its own vocabulary's key, never a bare token: a misspelled container, a retired strategy, or an unresolvable material key each produced a legal-looking case token measuring nothing, discovered only when the harness refused mid-run — and the token change that lands with the typed pin is a deliberate baseline reset, since a claim held under an unvalidated token was never proven to measure the program it named. Every pin carries its RESOLVER on `BenchPin` and each rails: an empty family or a refused synthetic grid measures the harness's own empty-input path and grades as the fastest row in the corpus. `PressGpuParity` is the one workload whose INTEREST is not its own duration: it presses one plan on both lanes and COMPOSES `press#PRESS_RECEIPT` `PressProduct.Parity` for the CPU-versus-GPU channel divergence rather than folding a second measure here, which the gate reads as evidence and NEVER as a content input, because persisted plane bytes are CPU-minted by structure and a GPU-keyed plane forks the content key at its preimage. Grading that divergence against a tolerance proposes exactly the equivalence the estate refused.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.AppHost.Observability;   // BenchmarkReceipt, BenchMeasurement, BenchmarkGate, GatePolicy
using Rasm.Domain;                  // CorrelationId, ReceiptSinkPort, Op
using Rasm.Materials.Appearance;    // Acquisition, BrdfSample — the synthetic-grid producer the fit pin binds
using Rasm.Materials.Component;     // ComponentCatalogue, ComponentRow, ComponentFamily — the catalogue pin's roster
using Rasm.Materials.Raster;        // RasterFormat, TileStrategy, HeightSolver, PressProduct, PressReceipt,
                                    // Golden/GoldenVector — the WGSL fixture roster the prove workload sweeps
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] --------------------------------------------------------------------------------
[SmartEnum<string>]
[KeyMemberEqualityComparer<ComparerAccessors.StringOrdinal, string>]
[KeyMemberComparer<ComparerAccessors.StringOrdinal, string>]
public sealed partial class BenchKernel {
    public static readonly BenchKernel SectionSolve = new("section.solve");
    public static readonly BenchKernel InteractionSweep = new("interaction.sweep");
    public static readonly BenchKernel GgxFit = new("acquisition.fit");
    public static readonly BenchKernel GraphCompile = new("graph.compile");
    public static readonly BenchKernel GraphEval = new("graph.eval");
    public static readonly BenchKernel SpectralUpsample = new("spectral.upsample");
    public static readonly BenchKernel TextureSample = new("texture.sample");
    public static readonly BenchKernel KubelkaMunkMix = new("finish.mix");
    public static readonly BenchKernel ShadeSpan = new("graph.shade-span");
    public static readonly BenchKernel TexturePress = new("texture.press");
    public static readonly BenchKernel PressGpuParity = new("texture.press.gpu-parity");
    public static readonly BenchKernel TileSynth = new("texture.tile");
    public static readonly BenchKernel TileGrade = new("texture.grade");
    public static readonly BenchKernel Convolve = new("texture.convolve.separable");
    public static readonly BenchKernel ConvolveSquare = new("texture.convolve.square");
    public static readonly BenchKernel MipFold = new("texture.mip-fold");
    public static readonly BenchKernel GoldenProve = new("gpu.golden-prove");
    public static readonly BenchKernel HeightSolve = new("texture.height-solve");
    public static readonly BenchKernel PlaneCodec = new("texture.codec");
    public static readonly BenchKernel IblPrefilter = new("environment.prefilter");

    public string Suite => $"rasm.materials.{Key}";
}

// The program an extent workload measures, as a CLOSED union over the four vocabularies that actually name one — a
// library row, a raster container, a tile strategy, a height solver. The bare string it replaces let any token seat
// itself as a program: a misspelled container key, a retired strategy, or a material key that no longer resolves all
// produced a legal-looking case token measuring nothing, and the corpus discovered it only when the harness refused
// mid-run. Each case reads its OWN key, so the token derives from the vocabulary that owns it and a strategy rename
// re-spells the case exactly where the roster moved. It is a SIBLING of BenchInput rather than a nested type: every
// composition site spells `ProgramPin.Container` bare, which a nested declaration renders unresolvable.
[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramPin {
    private ProgramPin() { }

    public sealed record Library(string MaterialKey) : ProgramPin;
    public sealed record Container(RasterFormat Format) : ProgramPin;
    public sealed record Tiling(TileStrategy Strategy) : ProgramPin;
    public sealed record Height(HeightSolver Solver) : ProgramPin;

    // Each case reads the KEY its own vocabulary publishes — never a second spelling minted here.
    public string Key => Switch(
        library:   static p => p.MaterialKey,
        container: static p => p.Format.Key,
        tiling:    static p => p.Strategy.Key,
        height:    static p => p.Solver.Key);
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BenchInput {
    private BenchInput() { }

    public sealed record CatalogueLeast(string FamilyKey) : BenchInput;
    public sealed record LibraryRow(string MaterialKey) : BenchInput;
    public sealed record Synthetic(int Seed, int Count) : BenchInput;

    // Roster pins a CLOSED SWEEP — a workload whose subject is every row a vocabulary declares, sized by that
    // vocabulary rather than by a caller's number. The golden-fixture prove is the landed instance: its subject is
    // every fixture every WGSL kernel carries, so a seed-and-count pin would name a grid the workload never builds
    // and would grade the sweep against a magnitude nothing in it measures. The case carries NO count column — the
    // population is the roster's own, and copying it here would freeze a number the kernel table moves.
    public sealed record Roster : BenchInput;

    // Extent pins the PLANE workloads: a bake, a batched shade, an encode, and a prefilter all size by an EXTENT over a
    // named program, which no seed-and-count expresses — a 4096-square press of one library row and a 4096-square press of
    // another are different programs at the same magnitude, and folding them onto one synthetic token lets a held claim
    // judge one against the other. The extent enters the case token, so a re-measurement at a different square is a
    // visible lineage fork rather than a silent comparison across two workloads.
    public sealed record Extent(int Width, int Height, ProgramPin Program) : BenchInput;

    // How many measurement units the pin represents — texels for a plane, samples for a synthetic grid, one whole
    // program for a row pin. THROUGHPUT is the interest of the plane workloads, and a rate is measured over this count
    // divided into the harness duration. It is a COLUMN because the alternative is re-parsing the case token a display
    // grammar owns — a reader splitting an extent token to recover a magnitude binds to a string form the token is free
    // to re-spell, and the first row whose program key carries a colon reads back a wrong number.
    public long Magnitude =>
        Switch(
            catalogueLeast: static _ => 1L,
            libraryRow: static _ => 1L,
            roster: static _ => 1L,
            synthetic: static s => s.Count,
            extent: static e => (long)e.Width * e.Height);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BenchWorkload(BenchKernel Kernel, BenchInput Input, UInt128 ContentKey);

// --- [OPERATIONS] ---------------------------------------------------------------------------
// The pin RESOLVERS — the producers every pin case names and none carried. A pin is a DECLARATION until
// something turns it into the subject a harness measures, and each resolution is fallible for its own reason:
// a catalogue reseed can leave a family with no Sectioned row, a synthetic grid can refuse its own admission.
// Both rail rather than answer a default, because a benchmark measuring a substituted subject reports a number
// under a case token naming the subject it did not measure.
public static class BenchPin {
    // The LEAST designation of the named family's Sectioned rows, in the family's own ordering — deterministic
    // by construction, so a reseed shifts the pin visibly through the content key rather than by luck of
    // enumeration order. An unknown family, or a family the reseed left with no Sectioned row, RAILS: an empty
    // family silently resolving to nothing measures the harness's own empty-input path.
    public static Fin<ComponentRow> CatalogueLeast(ComponentCatalogue catalogue, string familyKey, Op key) =>
        ComponentFamily.TryGet(familyKey, out ComponentFamily? family)
            ? toSeq(catalogue.Rows.Filter(row => row.Item.Family == family! && row.Sectioned)
                    .OrderBy(static row => row.Item.Designation.Value, StringComparer.Ordinal))
                .Head
                .ToFin(ProjectionFault.Unresolved(key, $"<bench-catalogue-least-empty:{familyKey}>"))
            : ProjectionFault.Unresolved(key, $"<bench-family-unknown:{familyKey}>");

    // The synthetic grid FAILS LOUD: Acquisition.SyntheticGrid rails on a refused seed or count, and a workload
    // binding a silently-empty grid measures a zero-sample fit that grades as the fastest row in the corpus. It
    // serves the FIT pin alone — the sampling and prove workloads that once shared this producer named subjects it
    // does not build, and each now pins the shape it actually measures.
    public static Fin<Seq<BrdfSample>> SyntheticGrid(BenchInput.Synthetic pin, Op key) =>
        Acquisition.SyntheticGrid(pin.Seed, pin.Count, key);

    // The roster sweep RAILS ON EMPTY for the reason every sibling resolver does: a golden roster resolving to
    // nothing measures the harness's own empty-sweep path and grades as the fastest row in the corpus. Reading the
    // roster here rather than pinning its size is what keeps a fixture the kernel table adds inside the measurement.
    public static Fin<Seq<GoldenVector>> Golden(Op key) =>
        Raster.Golden.All is { IsEmpty: false } fixtures
            ? Fin.Succ(fixtures)
            : ProjectionFault.Unresolved(key, "<bench-golden-roster-empty>");

    // The extent pin's PROGRAM resolves before the harness sizes anything. Three ProgramPin cases carry a typed
    // vocabulary row and are live by construction — a container, a strategy, and a solver cannot dangle, which is
    // the whole reason the closed union replaced the bare token. `Library` still carries a material key a library
    // edit can retire, so it alone is proved, through an injected admission rather than a mint this page would
    // otherwise have to spell: the corpus already takes its content key, harness, and claim as functions, and the
    // library probe joins them rather than binding this roster to a catalogue surface it does not own.
    public static Fin<ProgramPin> Program(ProgramPin program, Func<string, Op, Fin<Unit>> library, Op key) =>
        program.Switch(
            library:   p => library(p.MaterialKey, key).Map(_ => program),
            container: _ => Fin.Succ(program),
            tiling:    _ => Fin.Succ(program),
            height:    _ => Fin.Succ(program));
}
```

## [03]-[GATE_COMPOSITION]

- Owner: `MaterialsBench` — the content-bound corpus roster, case identity, and the gate pass over that corpus; AppHost owns receipt minting, host evidence, tracing, judging, and sink fan.
- Entry: `Corpus(contentKey)` resolves every logical pin to current content, `CaseOf(workload)` emits the logical token with its fixed-width content key, `Fresh(workload, measured, correlation)` projects one workload and its harness columns through `BenchmarkReceipt.Of`, and `Gate(...)` runs the whole corpus through `BenchmarkGate.Gate` and returns every verdict rail.
- Auto: catalogue reseeds and library edits change `ContentKey` even when their designation or material key is stable, so the gate re-baselines structurally rather than comparing two different programs; a regressed workload rides its own `Fin` and never aborts the corpus pass, so one pass grades every kernel.
- Law: identity columns are this folder's and measurement columns are the harness's — host evidence, verdict, artifact key, and correlation belong to the AppHost mint, and spelling any of them here forks the gate's own truth. Materials claims no relative lane, so `Reference` stays absent and `GatePolicy.SpeedupFloor` stays `None`.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new corpus entry is one logical pin row; a new measured receipt axis remains an AppHost owner change threading `BenchMeasurement`; harness residence and claim residence arrive as functions, so the bench project moves either without touching this page.
- Boundary: raw BenchmarkDotNet artifacts stay at the bench-project edge, which supplies `harness` and `claim` — this page composes the gate and never opens a measurement session, a durable claim store, or an `ActivitySource`.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.AppHost.Observability;   // BenchmarkReceipt, BenchMeasurement, BenchmarkGate, GatePolicy
using Rasm.Domain;                  // CorrelationId, ReceiptSinkPort, Op
using Rasm.Materials.Raster;        // RasterFormat, TileStrategy, HeightSolver, PressProduct, PressReceipt
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] ---------------------------------------------------------------------------
public static class MaterialsBench {
    public static Seq<BenchWorkload> Corpus(Func<BenchInput, UInt128> contentKey) =>
        Seq<(BenchKernel Kernel, BenchInput Input)>(
            (BenchKernel.SectionSolve, new BenchInput.CatalogueLeast("steel")),
            (BenchKernel.InteractionSweep, new BenchInput.CatalogueLeast("reinforcement")),
            (BenchKernel.GgxFit, new BenchInput.Synthetic(Seed: 7, Count: 4096)),
            (BenchKernel.GraphCompile, new BenchInput.LibraryRow("paint.car-metallic")),
            (BenchKernel.GraphEval, new BenchInput.LibraryRow("paint.car-metallic")),
            (BenchKernel.SpectralUpsample, new BenchInput.LibraryRow("wood.oak")),
            // TextureSample sizes by EXTENT over a program, never by a seed-and-count: the sampling fold runs per
            // texel across a named source, so the synthetic pin it once carried resolved through the acquisition
            // grid — a stratified goniophotometer capture the texture fold never touches — and graded a subject
            // this row does not measure. The square holds the same magnitude the count pinned, so the reading is
            // comparable across the correction while the case token forks visibly.
            (BenchKernel.TextureSample, new BenchInput.Extent(256, 256, CarMetallic)),
            (BenchKernel.KubelkaMunkMix, new BenchInput.LibraryRow("paint.clearcoat")),
            // ShadeSpan pins the FOUR-THOUSAND-SQUARE row the batched evaluator exists for: the whole square through the
            // frozen compiled order against one caller-rented scratch. It shares its program with the press rows, so the
            // span rail's own cost separates from the plane write and the channel fold rather than hiding inside a press
            // number, and a regression in either is attributable.
            (BenchKernel.ShadeSpan, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.TexturePress, new BenchInput.Extent(4096, 4096, CarMetallic)),
            // PressGpuParity is EXTENT-BOUND BY THE DEVICE rather than by the corpus: the `gpu#PRESS_DEVICE` storage-buffer
            // floor over a sixteen-byte texel admits 8388608 texels, so the four-thousand square its CPU siblings measure
            // asks the accelerator for twice the largest buffer the floor guarantees and the row refuses at dispatch instead
            // of measuring. It holds the SHARED PROGRAM and takes the largest square under that floor, so the lane
            // comparison runs at a legal extent rather than standing as a corpus row that never produces a number. The
            // CPU-versus-GPU divergence its press receipt carries stays TELEMETRY the gate never reads, because a GPU-keyed
            // plane forks the content key and the estate's answer is a structural veto rather than a tolerance.
            (BenchKernel.PressGpuParity, new BenchInput.Extent(2048, 2048, CarMetallic)),
            // PlaneCodec pins its CONTAINER, never a channel: the measured cost is the coder's own path, and the roster
            // spans two cost classes a single row averages into a number describing neither — a managed float container
            // encodes in-process, while the block-compressed container's floor is a spawned tool whose process cost
            // dominates its coder entirely. A smaller square holds the pin, because a 4k plane prices the arena walk
            // rather than the coder.
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, new ProgramPin.Container(RasterFormat.Exr))),
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, new ProgramPin.Container(RasterFormat.Ktx2))),
            // TileSynth pins THREE strategy cost classes at the default 32-texel overlap. The graphCut/offsetHeal
            // pair is the one `tile#TILE_SYNTH` names as the routing decision this corpus exists to settle: both
            // walk the same `lines × (2·Overlap + 1)` band, but the cut builds a flow graph over it and runs an
            // augmenting search superlinear in vertices and arcs, while the heal is exactly that many
            // dynamic-programming cost evaluations with no graph at all. A caller whose extent makes the cut
            // dominate takes the heal, so the crossing between them is a MEASURED number a later edit moves on
            // evidence rather than a preference asserted in a comment — and the pair only reads as a crossing when
            // both rows run at ONE extent and ONE overlap. The histogram blend is the third class, a per-texel
            // field fold that shares neither cost structure, so one averaged row would describe none of them.
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.GraphCut))),
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.OffsetHeal))),
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.HistogramBlend))),
            // TileGrade is the gate's OWN cost, separate from every synthesis row: an ingested third-party set is
            // graded without ever being synthesized, so the population that pays grading alone has no row inside a
            // synthesis measurement. Both signals run per grade — a streamed base-level seam fold and a spectral
            // transform at the policy's grading edge — so the row measures the pair the verdict actually needs.
            (BenchKernel.TileGrade, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.OffsetHeal))),
            // Convolve separates its TWO cost classes by construction rather than by size: a separable pass is
            // O(2r) per texel where a square window is O(r-squared), which at the three-sigma radius is 98 taps
            // against 2401 — one averaged row would describe neither, and the ratio between them is the whole
            // reason the separability column exists.
            (BenchKernel.Convolve, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.ConvolveSquare, new BenchInput.Extent(4096, 4096, CarMetallic)),
            // MipFold pins the KAISER row rather than the box floor: the box fold is a four-tap average and the
            // Kaiser is the windowed sinc every colour channel takes, so the measured chain cost is the one every
            // press, ingest, and decode actually pays per channel.
            (BenchKernel.MipFold, new BenchInput.Extent(4096, 4096, CarMetallic)),
            // GoldenProve measures the WGSL fixture dispatch itself — the roster sweep every kernel's own golden
            // vectors drive — because the proof estate runs it on every device change and its cost is the gate
            // between a fast re-prove and one nobody runs. It pins the ROSTER: the sweep's size is the fixture
            // population the kernel table declares, so the seed-and-count it once carried both named an acquisition
            // grid this workload never builds and froze a fixture count the roster is free to grow past.
            (BenchKernel.GoldenProve, new BenchInput.Roster()),
            // HeightSolve straddles filter#HEIGHT_FIELD's own DirectCeiling by construction: 2048-square seats under it
            // on the exact Cholesky factor and 4096-square above it on the preconditioned Krylov lane, so the constant
            // that routes the solve becomes a measured crossing a later edit moves on evidence rather than a number
            // asserted in a comment.
            (BenchKernel.HeightSolve, new BenchInput.Extent(2048, 2048, new ProgramPin.Height(HeightSolver.Poisson))),
            (BenchKernel.HeightSolve, new BenchInput.Extent(4096, 4096, new ProgramPin.Height(HeightSolver.Poisson))),
            // IblPrefilter rides a 2:1 equirect by construction, and its cost is the specular level set's own sweep.
            (BenchKernel.IblPrefilter, new BenchInput.Extent(2048, 1024, CarMetallic)))
        .Map(pin => new BenchWorkload(pin.Kernel, pin.Input, contentKey(pin.Input)));

    // The ONE program every plane row that measures a bake shares, spelled once: the span rail's cost, the whole
    // bake's cost, the accelerator lane's cost, and the two filter costs are readings of ONE workload, and a
    // second spelling of the key is how two of them silently become two workloads.
    static readonly ProgramPin CarMetallic = new ProgramPin.Library("paint.car-metallic");

    public static string CaseOf(BenchWorkload workload) => $"{workload.Input.Switch(
        catalogueLeast: static c => $"catalogue:{c.FamilyKey}",
        libraryRow: static l => $"library:{l.MaterialKey}",
        roster: static _ => "roster",
        synthetic: static s => $"synthetic:{s.Seed}x{s.Count}",
        extent: static e => $"extent:{e.Width}x{e.Height}:{e.Program.Key}")}@{workload.ContentKey:x32}";

    // Corpus identity is bound, so Corpus is Some on every row — a workload with no corpus key would
    // let a held claim judge a different program under the same case token.
    public static BenchmarkReceipt Fresh(BenchWorkload workload, BenchMeasurement measured, CorrelationId correlation) =>
        BenchmarkReceipt.Of(suite: workload.Kernel.Suite, @case: CaseOf(workload),
            corpus: Some(workload.ContentKey), measured: measured, correlation: correlation);

    // The PressGpuParity workload's own measurement, COMPOSED from the press page's producer rather than folded
    // here: PressProduct.Parity is the one owner of the CPU-versus-GPU per-channel maximum and it stamps the
    // minted receipt's GpuDeltaMax. This workload presses ONE plan on both lanes and hands the pair to that
    // producer, so the divergence a receipt carries and the divergence a benchmark reports are one number. The
    // delta stays TELEMETRY and never reaches the gate: persisted plane bytes are CPU-minted by structure, so a
    // tolerance over this number would propose exactly the equivalence the content-identity veto denies.
    public static Fin<PressReceipt> Parity(PressProduct.Minted minted, PressProduct.Preview preview, Op key) =>
        PressProduct.Parity(minted, preview, key);

    // Applicative Traverse, never TraverseM: a regressed kernel rides its own Fin, and short-circuiting
    // on first regression leaves every later kernel ungraded and unfanned.
    public static IO<Seq<Fin<BenchmarkReceipt>>> Gate(
        ReceiptSinkPort sink,
        Func<BenchInput, UInt128> contentKey,
        Func<BenchWorkload, BenchMeasurement> harness,
        Func<BenchWorkload, Option<BenchmarkReceipt>> claim,
        CorrelationId correlation,
        GatePolicy policy) =>
        Corpus(contentKey)
            .Traverse(workload => BenchmarkGate.Gate(sink, Fresh(workload, harness(workload), correlation), claim(workload), policy))
            .As();
}
```

## [04]-[RESEARCH]

(none)
