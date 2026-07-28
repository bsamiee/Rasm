# [MATERIALS_BENCHMARKS]

KERNEL benchmark identity is a `BenchKernel` row with a `BenchInput` pin and its resolved content key. `Suite` derives as `rasm.materials.<kernel>`, and `Case` carries both the pin token and content key, so catalogue or library content changes fork claim lineage without requiring a new row spelling.

Settled composition: Materials owns workload vocabulary and content-bound identity. `BenchmarkReceipt`, `BenchMeasurement`, `GatePolicy`, and `BenchmarkGate` arrive settled from `Rasm.AppHost/Observability/benchmarks#BENCHMARK_RECEIPT` under the branch benchmark-peer up-reference ruling; `ReceiptSinkPort` and `CorrelationId` arrive from the kernel signal capsule. BenchmarkDotNet binds in the branch bench project and never this package's csproj.

## [01]-[INDEX]

- [02]-[WORKLOAD_ROWS]: `BenchKernel` vocabulary over the section, capacity, appearance, and texture-plane kernels, the `BenchInput` pin union with its extent-and-program case, and the `BenchWorkload` corpus.
- [03]-[GATE_COMPOSITION]: content-bound corpus, fresh-receipt projection, and corpus gate pass.

## [02]-[WORKLOAD_ROWS]

- Owner: `BenchKernel` `[SmartEnum<string>]` — the measured-kernel vocabulary whose `Suite` column derives the receipt suite; `BenchInput` `[Union]` — the pinned-input shapes; `BenchWorkload` — one kernel bound to one pin.
- Cases: `BenchInput.CatalogueLeast` binds the least-designation `Sectioned` row of the named family at composition, so a catalogue reseed shifts the pin deterministically; `BenchInput.LibraryRow` binds one registered `MaterialLibrary` key; `BenchInput.Synthetic` derives a deterministic sample grid from its seed through the owning kernel — the `GgxFit` pin is `Acquisition.SyntheticGrid(seed, count)`, the stratified goniophotometer capture whose reflectance the microfacet forward model evaluates at seed-derived ground-truth alphas, the `TextureSample` pin the texture fold's own seed grid — so the input carries no fixture file and no RNG state outside the seed; `BenchInput.Extent` binds a texel square to a named program, the pair a plane workload sizes by — a library row for a shade or a press, a `RasterFormat` container key for an encode, a sky model for a prefilter — so the measured magnitude and the measured program both enter the case token. Every pin answers `Magnitude`, the count of measurement units it represents, so a throughput read divides that column into the harness duration rather than re-parsing the display token.
- Entry: `MaterialsBench.Corpus(contentKey)` resolves every pin through one injected content-key function; `MaterialsBench.CaseOf` derives the receipt case token from the pin and resolved key through the generated total `Switch`.
- Auto: a pin edit or resolved-content edit changes the case token, so claim lineage forks visibly instead of silently comparing different programs; the interaction sweep pins the reinforcement family because the hull builds from the RC section, the two graph kernels share one library pin so compile and eval measure one program, and the batched-shade, press, and parity rows share ONE extent and ONE program so the span rail's cost, the whole bake's cost, and the accelerator lane's cost are three readings of one workload rather than three workloads.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured kernel is one `BenchKernel` row and one `Corpus` row; a new pin shape is one `BenchInput` case breaking BOTH `CaseOf` and `Magnitude` at compile time — the `Extent` case landed through exactly that break, and one kernel takes as many `Corpus` rows as it has genuinely distinct cost classes.
- Boundary: workload rows pin inputs and derive identity — kernel bodies stay on their owning pages, and a workload never re-implements the kernel it measures. `Magnitude` is the one throughput denominator: the plane rows exist to be read as texels per second at four thousand square, and deriving a rate by splitting the case token binds a reader to a display grammar that owns no numbers. `PressGpuParity` is the one workload whose INTEREST is not its own duration: it measures the GPU lane's throughput and its press receipt carries the CPU-versus-GPU channel divergence, which the gate reads as evidence and NEVER as a content input, because persisted plane bytes are CPU-minted by structure and a GPU-keyed plane forks the content key at its preimage. Grading that divergence against a tolerance proposes exactly the equivalence the estate refused.

```csharp signature
// --- [RUNTIME_PRELUDE] ----------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.AppHost.Observability;   // BenchmarkReceipt, BenchMeasurement, BenchmarkGate, GatePolicy
using Rasm.Domain;                  // CorrelationId, ReceiptSinkPort
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
    public static readonly BenchKernel PlaneCodec = new("texture.codec");
    public static readonly BenchKernel IblPrefilter = new("environment.prefilter");

    public string Suite => $"rasm.materials.{Key}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BenchInput {
    private BenchInput() { }

    public sealed record CatalogueLeast(string FamilyKey) : BenchInput;
    public sealed record LibraryRow(string MaterialKey) : BenchInput;
    public sealed record Synthetic(int Seed, int Count) : BenchInput;

    // Extent pins the PLANE workloads: a bake, a batched shade, an encode, and a prefilter all size by an EXTENT over a
    // named program, which no seed-and-count expresses — a 4096-square press of one library row and a 4096-square press of
    // another are different programs at the same magnitude, and folding them onto one synthetic token lets a held claim
    // judge one against the other. The extent enters the case token, so a re-measurement at a different square is a
    // visible lineage fork rather than a silent comparison across two workloads.
    public sealed record Extent(int Width, int Height, string ProgramKey) : BenchInput;

    // How many measurement units the pin represents — texels for a plane, samples for a synthetic grid, one whole
    // program for a row pin. THROUGHPUT is the interest of the plane workloads (texels per second at four thousand
    // square is what the batched evaluator was minted for), and a rate is measured over this count divided into the
    // harness duration. It is a COLUMN because the alternative is re-parsing the case token a display grammar owns
    // — a reader splitting "extent:4096x4096:paint.car-metallic" to recover a magnitude binds to a string form the
    // token is free to re-spell, and the first row whose program key carries a colon reads back a wrong number.
    public long Magnitude =>
        Switch(
            catalogueLeast: static _ => 1L,
            libraryRow: static _ => 1L,
            synthetic: static s => s.Count,
            extent: static e => (long)e.Width * e.Height);
}

// --- [MODELS] -------------------------------------------------------------------------------
public sealed record BenchWorkload(BenchKernel Kernel, BenchInput Input, UInt128 ContentKey);
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
using Rasm.Domain;                  // CorrelationId, ReceiptSinkPort
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
            (BenchKernel.TextureSample, new BenchInput.Synthetic(Seed: 11, Count: 65536)),
            (BenchKernel.KubelkaMunkMix, new BenchInput.LibraryRow("paint.clearcoat")),
            // ShadeSpan pins the FOUR-THOUSAND-SQUARE row the batched evaluator exists for: 16.7 million texels
            // through the frozen compiled order against one caller-rented scratch. It shares its program with the two
            // press rows, so the span rail's own cost separates from the plane write and the channel fold rather than
            // hiding inside a press number, and a regression in either is attributable.
            (BenchKernel.ShadeSpan, new BenchInput.Extent(4096, 4096, "paint.car-metallic")),
            (BenchKernel.TexturePress, new BenchInput.Extent(4096, 4096, "paint.car-metallic")),
            // PressGpuParity measures the GPU lane's own throughput at the same extent and program; the CPU-versus-GPU
            // divergence its press receipt carries is TELEMETRY the gate never reads, because a GPU-keyed plane forks the
            // content key and the estate's answer is a structural veto rather than a tolerance.
            (BenchKernel.PressGpuParity, new BenchInput.Extent(4096, 4096, "paint.car-metallic")),
            // PlaneCodec pins its CONTAINER, never a channel: the measured cost is the coder's own path, and the roster
            // spans two cost classes a single row averages into a number describing neither — a managed float container
            // encodes in-process, while the block-compressed container's floor is a spawned tool whose process cost
            // dominates its coder entirely. A smaller square holds the pin, because a 4k plane prices the arena walk
            // rather than the coder.
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, "exr")),
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, "ktx2")),
            // IblPrefilter rides a 2:1 equirect by construction, and its cost is the specular level set's own sweep.
            (BenchKernel.IblPrefilter, new BenchInput.Extent(2048, 1024, "hosek-wilkie")))
        .Map(pin => new BenchWorkload(pin.Kernel, pin.Input, contentKey(pin.Input)));

    public static string CaseOf(BenchWorkload workload) => $"{workload.Input.Switch(
        catalogueLeast: static c => $"catalogue:{c.FamilyKey}",
        libraryRow: static l => $"library:{l.MaterialKey}",
        synthetic: static s => $"synthetic:{s.Seed}x{s.Count}",
        extent: static e => $"extent:{e.Width}x{e.Height}:{e.ProgramKey}")}@{workload.ContentKey:x32}";

    // Corpus identity is bound, so Corpus is Some on every row — a workload with no corpus key would
    // let a held claim judge a different program under the same case token.
    public static BenchmarkReceipt Fresh(BenchWorkload workload, BenchMeasurement measured, CorrelationId correlation) =>
        BenchmarkReceipt.Of(suite: workload.Kernel.Suite, @case: CaseOf(workload),
            corpus: Some(workload.ContentKey), measured: measured, correlation: correlation);

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
