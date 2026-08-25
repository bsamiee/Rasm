# [MATERIALS_BENCHMARKS]

KERNEL benchmark identity is a `BenchKernel` row with a `BenchInput` pin and its resolved content key. `Suite` derives as `rasm.materials.<kernel>`, and `Case` carries both the pin token and content key, so catalogue or library content changes fork claim lineage without requiring a new row spelling.

Settled composition: Materials owns workload vocabulary and content-bound identity. `Benchmark`, `BenchMeasurement`, `BenchmarkFault`, `GatePolicy`, and `BenchmarkGate` arrive settled from `Rasm.AppHost/Observability/benchmarks#BENCHMARK` under the branch benchmark-peer up-reference ruling, `BenchMeasurement.Of` already folding the harness sample into one `Distribution<Elapsed>`; `InstrumentSet` arrives from the kernel signal capsule.

## [01]-[INDEX]

- [02]-[WORKLOAD_ROWS]: `BenchKernel` vocabulary over the section, capacity, appearance, and texture-plane kernels, the `BenchInput` pin union with its extent case over the closed `ProgramPin`, the `BenchPin` resolvers every pin names, and the `BenchWorkload` corpus.
- [03]-[GATE_COMPOSITION]: content-bound corpus, fresh-benchmark projection, and corpus gate pass.

## [02]-[WORKLOAD_ROWS]

- Owner: `BenchKernel` `[SmartEnum<string>]` — the measured-kernel vocabulary whose `Suite` column derives the benchmark suite; `BenchInput` `[Union]` — the pinned-input shapes; `ProgramPin` `[Union]` — the four closed vocabularies an extent workload names a program from; `BenchPin` — the fallible pin resolvers; `BenchWorkload` — one kernel bound to one pin.
- Cases: `CatalogueLeast` binds the least-designation `Sectioned` row of the named family, so a reseed shifts the pin deterministically; `LibraryRow` binds one registered `MaterialLibrary` key; `Synthetic` derives a deterministic sample grid from its seed through the owning kernel, so the input carries no fixture file and no RNG state outside the seed; `Roster` pins a closed sweep whose size is the vocabulary's own; `Extent` binds a texel square to a named program, so the measured magnitude and the measured program both enter the case token.
- Entry: `MaterialsBench.Corpus(contentKey)` resolves every pin through one injected content-key function; `MaterialsBench.CaseOf` derives the benchmark case token from the pin and resolved key through the generated total `Switch`.
- Auto: a pin edit or resolved-content edit changes the case token, so claim lineage forks visibly instead of silently comparing different programs; `Magnitude` is a declared column on every pin, so a throughput read divides the measurement count into the harness duration rather than re-parsing a display token a grammar owns.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured kernel is one `BenchKernel` row and one `Corpus` row; a new program vocabulary is one `ProgramPin` case breaking `Key`; a new pin shape is one `BenchInput` case breaking BOTH `CaseOf` and `Magnitude` at compile time — `Extent` and `Roster` each landed through exactly that break.
- Law: one kernel takes as many `Corpus` rows as it has genuinely distinct cost classes, and a pair either SEPARATES cost classes or STRADDLES a routing constant — never one program at two sizes. `PlaneCodec` and `TileSynth` separate; `HeightSolve` straddles `filter#HEIGHT_FIELD`'s own direct-versus-Krylov ceiling, so the constant that routes the solve is measured rather than asserted.
- Law: a program is a CLOSED `ProgramPin` case reading its own vocabulary's key, never a bare token — a misspelled container, a retired strategy, or an unresolvable material key each produced a legal-looking case token measuring nothing, discovered only when the harness refused mid-run. Every pin carries its RESOLVER on `BenchPin` and each rails, since an empty family or a refused synthetic grid measures the harness's own empty-input path and grades as the fastest row in the corpus.
- Boundary: workload rows pin inputs and derive identity — kernel bodies stay on their owning pages and a workload never re-implements the kernel it measures. `PressGpuParity` is the one workload whose INTEREST is not its own duration: it presses one plan on both lanes and COMPOSES `press#PRESS_PRODUCT` `PressProduct.Parity`, which the gate reads as evidence and NEVER as a content input — persisted plane bytes are CPU-minted by structure, so grading that divergence against a tolerance proposes exactly the equivalence the content-identity veto denies.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using LanguageExt;
using Rasm.AppHost.Observability;
using Rasm.Domain;
using Rasm.Materials.Appearance;
using Rasm.Materials.Component;
using Rasm.Materials.Raster;
using Thinktecture;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [TYPES] ---------------------------------------------------------------------------
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
    public static readonly BenchKernel OracleProve = new("gpu.oracle-prove");
    public static readonly BenchKernel HeightSolve = new("texture.height-solve");
    public static readonly BenchKernel PlaneCodec = new("texture.codec");
    public static readonly BenchKernel IblPrefilter = new("environment.prefilter");

    public string Suite => $"rasm.materials.{Key}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record ProgramPin {
    private ProgramPin() { }

    public sealed record Library(string MaterialKey) : ProgramPin;
    public sealed record Container(RasterFormat Format) : ProgramPin;
    public sealed record Tiling(TileStrategy Strategy) : ProgramPin;
    public sealed record Height(HeightSolver Solver) : ProgramPin;

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

    public sealed record Roster : BenchInput;

    public sealed record Extent(int Width, int Height, ProgramPin Program) : BenchInput;

    public long Magnitude =>
        Switch(
            catalogueLeast: static _ => 1L,
            libraryRow: static _ => 1L,
            roster: static _ => 1L,
            synthetic: static s => s.Count,
            extent: static e => (long)e.Width * e.Height);
}

// --- [MODELS] --------------------------------------------------------------------------
public sealed record BenchWorkload(BenchKernel Kernel, BenchInput Input, UInt128 ContentKey);

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class BenchPin {
    public static Fin<ComponentRow> CatalogueLeast(ComponentCatalogue catalogue, string familyKey, Op key) =>
        ComponentFamily.TryGet(familyKey, out ComponentFamily? family)
            ? toSeq(catalogue.Rows.Filter(row => row.Item.Family == family! && row.Sectioned)
                    .OrderBy(static row => row.Item.Designation.Value, StringComparer.Ordinal))
                .Head
                .ToFin(new ProjectionFault.Unresolved(key, $"<bench-catalogue-least-empty:{familyKey}>"))
            : new ProjectionFault.Unresolved(key, $"<bench-family-unknown:{familyKey}>");

    public static Fin<Seq<BrdfSample>> SyntheticGrid(BenchInput.Synthetic pin, Op key) =>
        Acquisition.SyntheticGrid(pin.Seed, pin.Count, key);

    public static Fin<Seq<OracleVector>> Oracle(Op key) =>
        Raster.Oracle.All is { IsEmpty: false } fixtures
            ? Fin.Succ(fixtures)
            : new ProjectionFault.Unresolved(key, "<bench-oracle-roster-empty>");

    public static Fin<ProgramPin> Program(ProgramPin program, Func<string, Op, Fin<Unit>> library, Op key) =>
        program.Switch(
            library:   p => library(p.MaterialKey, key).Map(_ => program),
            container: _ => Fin.Succ(program),
            tiling:    _ => Fin.Succ(program),
            height:    _ => Fin.Succ(program));
}
```

## [03]-[GATE_COMPOSITION]

- Owner: `MaterialsBench` — the content-bound corpus roster, case identity, and the gate pass over that corpus; AppHost owns benchmark minting, host fingerprinting, tracing, judging, and the gate's instrument writes.
- Entry: `Corpus(contentKey)` resolves every logical pin to current content, `CaseOf(workload)` emits the logical token with its fixed-width content key, `Fresh(workload, measured, stamps)` projects one workload and its harness columns through `Benchmark.Of`, and `Gate(...)` runs the whole corpus through `BenchmarkGate.Gate` and returns every accumulating verdict rail.
- Auto: catalogue reseeds and library edits change `ContentKey` even when their designation or material key is stable, so the gate re-baselines structurally rather than comparing two different programs; a regressed workload rides its own accumulating `Validation` and never aborts the corpus pass, so one pass grades every kernel and each names every reason it failed.
- Law: identity columns are this folder's and measurement columns are the harness's — the host fingerprint, verdict, and artifact key belong to the AppHost mint, and spelling any of them here forks the gate's own truth. Materials claims no relative lane, so `Reference` stays absent and `GatePolicy.SpeedupFloor` stays `None`.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new corpus entry is one logical pin row; a new measured benchmark axis remains an AppHost owner change threading `BenchMeasurement`; harness residence and claim residence arrive as functions, so the bench project moves either without touching this page.
- Boundary: this page composes the gate and never opens a measurement session, a durable claim store, or an `ActivitySource`. No statistical fold lands here: `BenchMeasurement.Of` already admits the harness sample into exact order statistics over one `Distribution<Elapsed>`, so a folder-local moment mint states a second answer to a measurement the AppHost carrier owns.

```csharp
// --- [IMPORTS] -------------------------------------------------------------------------
using System;
using System.Collections.Frozen;
using LanguageExt;
using Rasm.AppHost.Observability;
using Rasm.Domain;
using Rasm.Materials.Raster;
using static LanguageExt.Prelude;

namespace Rasm.Materials.Projection;

// --- [OPERATIONS] ----------------------------------------------------------------------
public static class MaterialsBench {
    public static Seq<BenchWorkload> Corpus(Func<BenchInput, UInt128> contentKey) =>
        Seq<(BenchKernel Kernel, BenchInput Input)>(
            (BenchKernel.SectionSolve, new BenchInput.CatalogueLeast("steel")),
            (BenchKernel.InteractionSweep, new BenchInput.CatalogueLeast("reinforcement")),
            (BenchKernel.GgxFit, new BenchInput.Synthetic(Seed: 7, Count: 4096)),
            (BenchKernel.GraphCompile, new BenchInput.LibraryRow("paint.car-metallic")),
            (BenchKernel.GraphEval, new BenchInput.LibraryRow("paint.car-metallic")),
            (BenchKernel.SpectralUpsample, new BenchInput.LibraryRow("wood.oak")),
            (BenchKernel.TextureSample, new BenchInput.Extent(256, 256, CarMetallic)),
            (BenchKernel.KubelkaMunkMix, new BenchInput.LibraryRow("paint.clearcoat")),
            (BenchKernel.ShadeSpan, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.TexturePress, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.PressGpuParity, new BenchInput.Extent(2048, 2048, CarMetallic)),
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, new ProgramPin.Container(RasterFormat.Exr))),
            (BenchKernel.PlaneCodec, new BenchInput.Extent(1024, 1024, new ProgramPin.Container(RasterFormat.Ktx2))),
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.GraphCut))),
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.OffsetHeal))),
            (BenchKernel.TileSynth, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.HistogramBlend))),
            (BenchKernel.TileGrade, new BenchInput.Extent(4096, 4096, new ProgramPin.Tiling(TileStrategy.OffsetHeal))),
            (BenchKernel.Convolve, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.ConvolveSquare, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.MipFold, new BenchInput.Extent(4096, 4096, CarMetallic)),
            (BenchKernel.OracleProve, new BenchInput.Roster()),
            (BenchKernel.HeightSolve, new BenchInput.Extent(2048, 2048, new ProgramPin.Height(HeightSolver.Poisson))),
            (BenchKernel.HeightSolve, new BenchInput.Extent(4096, 4096, new ProgramPin.Height(HeightSolver.Poisson))),
            (BenchKernel.IblPrefilter, new BenchInput.Extent(2048, 1024, CarMetallic)))
        .Map(pin => new BenchWorkload(pin.Kernel, pin.Input, contentKey(pin.Input)));

    static readonly ProgramPin CarMetallic = new ProgramPin.Library("paint.car-metallic");

    public static string CaseOf(BenchWorkload workload) => $"{workload.Input.Switch(
        catalogueLeast: static c => $"catalogue:{c.FamilyKey}",
        libraryRow: static l => $"library:{l.MaterialKey}",
        roster: static _ => "roster",
        synthetic: static s => $"synthetic:{s.Seed}x{s.Count}",
        extent: static e => $"extent:{e.Width}x{e.Height}:{e.Program.Key}")}@{workload.ContentKey:x32}";

    public static Benchmark Fresh(BenchWorkload workload, BenchMeasurement measured, FrozenDictionary<string, string> stamps) =>
        Benchmark.Of(suite: workload.Kernel.Suite, @case: CaseOf(workload),
            corpus: Some(workload.ContentKey), measured: measured, stamps: stamps);

    public static Fin<PressRun> Parity(PressProduct.Minted minted, PressProduct.Preview preview, Op key) =>
        PressProduct.Parity(minted, preview, key);

    public static IO<Seq<Validation<Error, Benchmark>>> Gate(
        InstrumentSet signals,
        Func<BenchInput, UInt128> contentKey,
        Func<BenchWorkload, BenchMeasurement> harness,
        Func<BenchWorkload, Option<Benchmark>> claim,
        FrozenDictionary<string, string> stamps,
        GatePolicy policy,
        Op key) =>
        Corpus(contentKey)
            .Traverse(workload => BenchmarkGate.Gate(
                signals, Fresh(workload, harness(workload), stamps), claim(workload), policy, key))
            .As();
}
```

## [04]-[RESEARCH]

(none)
