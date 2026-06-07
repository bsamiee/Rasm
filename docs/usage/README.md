# [RASM_USAGE]

Cross-stack implementation surface for new work. Leaf docs own package and member detail.

## [1][OWNER_LADDER]

| [INDEX] | [OWNER]            | [CAPABILITY]                                                           |
| :-----: | ------------------ | ---------------------------------------------------------------------- |
|   [1]   | RhinoCommon        | Geometry validity, tolerances, units, transforms, topology             |
|   [2]   | GH2                | `IDataAccess`, trees, paths, coverage, diagnostics                     |
|   [3]   | MathNet            | Linear algebra, solvers, fitting, optimization, statistics, symbolics  |
|   [4]   | BCL/System         | Spans, regex, frozen lookup, generic math, SIMD, time, IO, diagnostics |
|   [5]   | LanguageExt        | `Fin`, `Validation`, `Eff`, `IO`, `Schedule`, `Seq`, `K<F,A>`          |
|   [6]   | Thinktecture       | Value objects, smart enums, unions, generated dispatch                 |
|   [7]   | Platform libraries | `Rasm.AppUi`, `Rasm.AppHost`, `Rasm.Persistence`, `Rasm.Compute`       |
|   [8]   | Composition root   | Scrutor, EF, Serilog, OTel, Http.Resilience; bootstrap only            |

Choose the first owner that directly owns the capability. Local platform surfaces integrate or compose approved libraries through their owner rails; they do not replace admitted libraries by default.

[READ]
- [1] `../hosts/rhino/`; local RhinoWIP XML; `uv run python -m tools.assay api query --key rhino-common --symbol <symbol>`
- [2] `../hosts/grasshopper/`; local GH2 XML; `uv run python -m tools.assay api query --key gh2 --symbol <symbol>`
- [3] `../stacks/csharp/numeric-algorithms.md`; `../stacks/csharp/sparse-factorization.md`
- [4] `../stacks/csharp/platform/system-apis.md`, `../stacks/csharp/platform/build-and-packages.md`
- [5] `../stacks/csharp/rails-and-effects.md`
- [6] `../stacks/csharp/domain-shapes.md`
- [7] `../../libs/csharp/Rasm.AppUi/README.md`, `../../libs/csharp/Rasm.AppHost/README.md`, `../../libs/csharp/Rasm.Persistence/README.md`, `../../libs/csharp/Rasm.Compute/README.md`
- [8] `composition.md`

## [2][FLOW]

1. Admit raw input through Rhino/GH2/System boundary policy.
2. Encode domain meaning with Thinktecture generated shapes.
3. Carry failure through LanguageExt `Fin`, `Validation`, or `Eff`.
4. Execute MathNet only for algorithmic numeric work; symbolic work requires a numeric adoption gate.
5. Project output back through Rhino validity or GH2 tree/diagnostic rules.
6. Route app UI, runtime, persistence, and compute concerns through `composition.md` and owner-local platform manuals before adding packages or public rails.

## [3][PATTERNS]

| [INDEX] | [PATTERN]        | [GUIDANCE]                                                                                    |
| :-----: | ---------------- | --------------------------------------------------------------------------------------------- |
|   [1]   | Domain rail      | Thinktecture admits values; LanguageExt carries validation and effects.                       |
|   [2]   | Rhino numeric    | Rhino validates geometry; MathNet solves selected numeric projection; Rhino validates output. |
|   [3]   | Symbolic GH2     | GH2 reads formula text; Symbolics parses/evaluates; diagnostics report exact failed stage.    |
|   [4]   | System primitive | BCL handles grammar, lookup, spans, timing, or diagnostics only when it owns the concern.     |

## [4][REJECTIONS]

- Do not wrap library APIs without domain value.
- Do not add packages as future intent in `Directory.Packages.props`.
- Do not treat candidate package records as runtime proof.
- Do not let MathNet override Rhino geometry semantics.
- Do not flatten GH2 tree/path behavior into lists.
- Do not treat public docs as compile truth when local WIP XML differs.
- Do not use current early implementation symbols as doctrine.

## [5][PROOF]

| [INDEX] | [PROOF_OWNER]                                            | [OWNS]                                                           |
| :-----: | -------------------------------------------------------- | ---------------------------------------------------------------- |
|   [1]   | `Directory.Packages.props`                               | Central package versions and package state.                      |
|   [2]   | `Directory.Build.props`                                  | Package references, global usings, RhinoWIP/GH2 host references. |
|   [3]   | Local NuGet XML / lockfiles                              | Exact API surface for local package assets.                      |
|   [4]   | RhinoWIP XML / decompile                                 | RhinoCommon, GH2, Rhino.UI, Eto compile truth.                   |
|   [5]   | `docs/stacks/csharp/platform`, `docs/stacks/csharp/*.md` | BCL, package graph, and C# capability policy after local proof.  |
|   [6]   | `docs/stacks/csharp/testing`                             | Test-rail API surfaces.                                          |
|   [7]   | `docs/standards`                                         | Doc-type structure, technical style, proof, and retrieval shape. |
|   [8]   | Owner-local platform manuals                             | Future App UI, AppHost, Persistence, and Compute intent.         |
|   [9]   | Official docs                                            | Current context only when local proof is silent.                 |

Performance and runtime-load claims require measured or load-context evidence before docs call them active.
