# [MATERIALS_BENCHMARKS]

THE KERNEL BENCHMARK CORPUS. One `BenchKernel` vocabulary names every measured materials kernel and one `BenchWorkload` row pins each kernel to a content-stable input, so a run is reproducible by construction and a speed claim anywhere in this folder resolves to a `BenchmarkReceipt` the AppHost gate stamped. `Suite` derives as `rasm.materials.<kernel>` and `Case` as the pinned-input token, so the receipt family carries kernel identity and input key inside columns it already owns, and the host-fingerprint slot is the `HostEvidence` the receipt stamps.

Settled composition: `BenchmarkReceipt`, `BenchmarkVerdict`, `BenchmarkGate`, `GatePolicy`, `BenchmarkRun`, `HostEvidence`, `ReceiptSinkPort`, and `CorrelationId` arrive from the `Rasm.AppHost` benchmark rail; `MaterialLibrary` keys pin the appearance inputs and the component catalogue pins the section inputs. BenchmarkDotNet binds in the branch bench project per the test-stack manifest tier, never this package's csproj — the bench edge folds each raw run to one receipt, and the durable claim the gate compares against resolves by content fingerprint from the Persistence reuse index.

## [01]-[INDEX]

- [02]-[WORKLOAD_ROWS]: the `BenchKernel` vocabulary, the `BenchInput` pin union, and the `BenchWorkload` corpus.
- [03]-[GATE_COMPOSITION]: the `MaterialsBench` receipt mint and the span-wrapped gated run.

## [02]-[WORKLOAD_ROWS]

- Owner: `BenchKernel` `[SmartEnum<string>]` — the measured-kernel vocabulary whose `Suite` column derives the receipt suite; `BenchInput` `[Union]` — the pinned-input shapes; `BenchWorkload` — one kernel bound to one pin.
- Cases: `BenchInput.CatalogueLeast` binds the least-designation `Sectioned` row of the named family at composition, so a catalogue reseed shifts the pin deterministically; `BenchInput.LibraryRow` binds one registered `MaterialLibrary` key; `BenchInput.Synthetic` derives a deterministic sample grid from its seed through the owning kernel, so the input carries no fixture file.
- Entry: `MaterialsBench.Corpus` is the workload roster the bench project enumerates; `MaterialsBench.CaseOf` derives the receipt case token from the pin through the generated total `Switch`.
- Auto: a pinned-input edit changes the case token, so claim lineage forks visibly instead of silently comparing across inputs; the interaction sweep pins the reinforcement family because the hull builds from the RC section, and the two graph kernels share one library pin so compile and eval measure one program.
- Packages: Rasm.AppHost (project), Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured kernel is one `BenchKernel` row and one `Corpus` row; a new pin shape is one `BenchInput` case breaking `CaseOf` at compile time.
- Boundary: workload rows pin inputs and derive identity — kernel bodies stay on their owning pages, and a workload never re-implements the kernel it measures.

```csharp signature
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

    public string Suite => $"rasm.materials.{Key}";
}

[Union(ConversionFromValue = ConversionOperatorsGeneration.None)]
public abstract partial record BenchInput {
    private BenchInput() { }

    public sealed record CatalogueLeast(string FamilyKey) : BenchInput;
    public sealed record LibraryRow(string MaterialKey) : BenchInput;
    public sealed record Synthetic(int Seed, int Count) : BenchInput;
}

public sealed record BenchWorkload(BenchKernel Kernel, BenchInput Input);
```

## [03]-[GATE_COMPOSITION]

- Owner: `MaterialsBench` — the corpus roster, the receipt mint, and the gated-run composition over the AppHost benchmark rail.
- Entry: `Fresh` mints the `Unjudged` receipt at the bench edge from the workload's derived suite and case with the current `HostEvidence`; `Gated` runs one workload inside the minted span through `BenchmarkRun.Traced`, judges it through `BenchmarkGate.Gate` under `GatePolicy.Canonical`, and fans the judged receipt through the sink under the AppHost benchmark kind.
- Auto: the gate is receipt-driven — claim resolution rides the host digest and the suite/case identity the receipt carries, so no per-workload registry exists on either side; a regressed run still fans, so the AppHost benchmark projection arm counts duration and regressions off every verdict; the span carries suite and case tags at start, so the continuous-profiling linkage lands the flame graph one click from the regressed receipt.
- Packages: Rasm.AppHost (project), LanguageExt.Core, NodaTime, BCL inbox.
- Growth: a new measured axis is one receipt field at the AppHost owner breaking the gate fold; a new corpus entry is one `Corpus` row here.
- Boundary: profile-span correlation composes at the service root — a desktop bench run without a profiler endpoint executes the identical span with the linkage dormant; the raw BenchmarkDotNet artifacts stay at the bench-project edge, and the one artifact key crosses on the receipt's own column.

```csharp signature
public static class MaterialsBench {
    public static readonly Seq<BenchWorkload> Corpus = Seq(
        new BenchWorkload(BenchKernel.SectionSolve, new BenchInput.CatalogueLeast("steel")),
        new BenchWorkload(BenchKernel.InteractionSweep, new BenchInput.CatalogueLeast("reinforcement")),
        new BenchWorkload(BenchKernel.GgxFit, new BenchInput.Synthetic(Seed: 7, Count: 4096)),
        new BenchWorkload(BenchKernel.GraphCompile, new BenchInput.LibraryRow("paint.car-metallic")),
        new BenchWorkload(BenchKernel.GraphEval, new BenchInput.LibraryRow("paint.car-metallic")),
        new BenchWorkload(BenchKernel.SpectralUpsample, new BenchInput.LibraryRow("wood.oak")),
        new BenchWorkload(BenchKernel.TextureSample, new BenchInput.Synthetic(Seed: 11, Count: 65536)),
        new BenchWorkload(BenchKernel.KubelkaMunkMix, new BenchInput.LibraryRow("paint.clearcoat")));

    public static string CaseOf(BenchInput input) => input.Switch(
        catalogueLeast: static c => $"catalogue:{c.FamilyKey}",
        libraryRow: static l => $"library:{l.MaterialKey}",
        synthetic: static s => $"synthetic:{s.Seed}x{s.Count}");

    public static BenchmarkReceipt Fresh(
        BenchWorkload workload, Duration median, Duration p95, long allocatedBytes, long operations, CorrelationId correlation) =>
        new(workload.Kernel.Suite, CaseOf(workload.Input), HostEvidence.Current(), median, p95, allocatedBytes, operations,
            BenchmarkVerdict.Unjudged, Option<string>.None, correlation);

    public static IO<Fin<BenchmarkReceipt>> Gated(
        ReceiptSinkPort sink, ActivitySource source, BenchWorkload workload, Option<BenchmarkReceipt> claim, Func<Fin<BenchmarkReceipt>> run) =>
        IO.lift(() => BenchmarkRun.Traced(source, workload.Kernel.Suite, CaseOf(workload.Input), run))
            .Bind(fresh => fresh.Match(
                Succ: receipt => BenchmarkGate.Gate(sink, receipt, claim, GatePolicy.Canonical),
                Fail: static error => IO.pure(Fin.Fail<BenchmarkReceipt>(error))));
}
```

## [04]-[RESEARCH]

- [SYNTHETIC_GRID_LAW]-[OPEN]: `GgxFit`'s synthetic pin derives its BRDF sample grid from the seed through the microfacet kernel — pin the exact grid derivation (zenith/azimuth stratification over `Seed`/`Count`) at the acquisition owner before the bench project enumerates the corpus; route: the bsdf microfacet fence and the acquisition `BrdfSample` shape.
