# [MATERIALS_BENCHMARKS]

KERNEL benchmark identity is a `BenchKernel` row with a `BenchInput` pin and its resolved content key. `Suite` derives as `rasm.materials.<kernel>`, and `Case` carries both the pin token and content key, so catalogue or library content changes fork claim lineage without requiring a new row spelling.

Settled composition: Materials owns workload vocabulary and content-bound identity. AppHost receipt minting, tracing, gating, and sink fan stay outside settled code until their exact members enter an admitted API catalogue. BenchmarkDotNet binds in the branch bench project and never this package's csproj.

## [01]-[INDEX]

- [02]-[WORKLOAD_ROWS]: the `BenchKernel` vocabulary, the `BenchInput` pin union, and the `BenchWorkload` corpus.
- [03]-[GATE_COMPOSITION]: the content-bound corpus and blocked AppHost receipt composition.

## [02]-[WORKLOAD_ROWS]

- Owner: `BenchKernel` `[SmartEnum<string>]` — the measured-kernel vocabulary whose `Suite` column derives the receipt suite; `BenchInput` `[Union]` — the pinned-input shapes; `BenchWorkload` — one kernel bound to one pin.
- Cases: `BenchInput.CatalogueLeast` binds the least-designation `Sectioned` row of the named family at composition, so a catalogue reseed shifts the pin deterministically; `BenchInput.LibraryRow` binds one registered `MaterialLibrary` key; `BenchInput.Synthetic` derives a deterministic sample grid from its seed through the owning kernel — the `GgxFit` pin is `Acquisition.SyntheticGrid(seed, count)`, the stratified goniophotometer capture whose reflectance the microfacet forward model evaluates at seed-derived ground-truth alphas, the `TextureSample` pin the texture fold's own seed grid — so the input carries no fixture file and no RNG state outside the seed.
- Entry: `MaterialsBench.Corpus(contentKey)` resolves every pin through one injected content-key function; `MaterialsBench.CaseOf` derives the receipt case token from the pin and resolved key through the generated total `Switch`.
- Auto: a pin edit or resolved-content edit changes the case token, so claim lineage forks visibly instead of silently comparing different programs; the interaction sweep pins the reinforcement family because the hull builds from the RC section, and the two graph kernels share one library pin so compile and eval measure one program.
- Packages: Thinktecture.Runtime.Extensions, LanguageExt.Core.
- Growth: a new measured kernel is one `BenchKernel` row and one `Corpus` row; a new pin shape is one `BenchInput` case breaking `CaseOf` at compile time.
- Boundary: workload rows pin inputs and derive identity — kernel bodies stay on their owning pages, and a workload never re-implements the kernel it measures.

```csharp signature
[SmartEnum<string>]
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

[Union]
public abstract partial record BenchInput {
    private BenchInput() { }

    public sealed record CatalogueLeast(string FamilyKey) : BenchInput;
    public sealed record LibraryRow(string MaterialKey) : BenchInput;
    public sealed record Synthetic(int Seed, int Count) : BenchInput;
}

public sealed record BenchWorkload(BenchKernel Kernel, BenchInput Input, UInt128 ContentKey);
```

## [03]-[GATE_COMPOSITION]

- Owner: `MaterialsBench` — the content-bound corpus roster and case identity; AppHost owns receipt minting, tracing, gating, and sink fan.
- Entry: `Corpus(contentKey)` resolves every logical pin to current content, and `CaseOf(workload)` emits the logical token with its fixed-width content key.
- Auto: catalogue reseeds and library edits change `ContentKey` even when their designation or material key is stable.
- Packages: LanguageExt.Core, BCL inbox.
- Growth: a new corpus entry is one logical pin row; a new measured receipt axis remains an AppHost owner change.
- Boundary: `[APPHOST_BENCHMARK_CATALOG]` blocks receipt code. Raw BenchmarkDotNet artifacts stay at the bench-project edge.

```csharp signature
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
            (BenchKernel.KubelkaMunkMix, new BenchInput.LibraryRow("paint.clearcoat")))
        .Map(pin => new BenchWorkload(pin.Kernel, pin.Input, contentKey(pin.Input)));

    public static string CaseOf(BenchWorkload workload) => $"{workload.Input.Switch(
        catalogueLeast: static c => $"catalogue:{c.FamilyKey}",
        libraryRow: static l => $"library:{l.MaterialKey}",
        synthetic: static s => $"synthetic:{s.Seed}x{s.Count}")}@{workload.ContentKey:x32}";
}
```

## [04]-[RESEARCH]

- [APPHOST_BENCHMARK_CATALOG]-[BLOCKED]: which exact signatures admit the corpus-bearing receipt and gate rail? Route: `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.Materials/.api/api-rasm-apphost.md`, then `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/.api/api-rasm-apphost.md`, against `/Users/bardiasamiee/Documents/99.Github/Rasm/libs/csharp/Rasm.AppHost/.planning/Observability/benchmarks.md`.
