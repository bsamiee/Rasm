# [RASM_USAGE]

Cross-stack implementation surface for new work. The owner ladder selects the package, platform, and API rail that owns each capability.

## [1]-[OWNER_LADDER]

| [INDEX] | [OWNER]            | [CAPABILITY]                                                           |
| :-----: | ------------------ | ---------------------------------------------------------------------- |
|   [1]   | RhinoCommon        | Geometry validity, tolerances, units, transforms, topology             |
|   [2]   | GH2                | `IDataAccess`, trees, paths, coverage, diagnostics                     |
|   [3]   | MathNet            | Linear algebra, solvers, fitting, optimization, statistics, symbolics  |
|   [4]   | BCL/System         | Spans, regex, frozen lookup, generic math, SIMD, time, IO, diagnostics |
|   [5]   | LanguageExt        | `Fin`, `Validation`, `Eff`, `IO`, `Schedule`, `Seq`, `K<F,A>`          |
|   [6]   | Thinktecture       | Value objects, smart enums, unions, generated dispatch                 |
|   [7]   | Platform libraries | `Rasm.AppUi`, `Rasm.AppHost`, `Rasm.Persistence`, `Rasm.Compute`       |
|   [8]   | Composition root   | Generic Host, Scrutor, health, telemetry, HTTP resilience; bootstrap only |

Choose the first owner that directly owns the capability. Local platform surfaces integrate or compose approved libraries through their owner rails; they do not replace package-owned behavior with wrapper vocabulary.

## [2]-[FLOW]

1. Receive raw input through Rhino/GH2/System boundary policy.
2. Encode domain meaning with Thinktecture generated shapes.
3. Carry failure through LanguageExt `Fin`, `Validation`, or `Eff`.
4. Execute MathNet only for algorithmic numeric work; symbolic work requires a numeric capability gate.
5. Project output back through Rhino validity or GH2 tree/diagnostic rules.
6. Route app UI, runtime, persistence, and compute concerns through the platform rail that owns the capability before adding packages or public surfaces.
7. Keep provider APIs inside the owner rail that owns them; public vocabulary stays provider-neutral.
8. Build platform rails from the complete feature set: host, companion, sidecar, downstream app, provider, asset, storage, compute, telemetry, support, and proof variants are parameterized cases inside existing shapes before new public surfaces appear.

## [3]-[PATTERNS]

| [INDEX] | [PATTERN]        | [GUIDANCE]                                                                                    |
| :-----: | ---------------- | --------------------------------------------------------------------------------------------- |
|   [1]   | Domain rail      | Thinktecture shapes values; LanguageExt carries validation and effects.                       |
|   [2]   | Rhino numeric    | Rhino validates geometry; MathNet solves selected numeric projection; Rhino validates output. |
|   [3]   | Symbolic GH2     | GH2 reads formula text; Symbolics parses/evaluates; diagnostics report exact failed stage.    |
|   [4]   | System primitive | BCL handles grammar, lookup, spans, timing, and diagnostics it owns directly.                 |

## [4]-[REJECTIONS]

- Do not wrap library APIs without domain value.
- Do not add packages as planning markers.
- Do not treat build-contract package records as runtime proof.
- Do not let MathNet override Rhino geometry semantics.
- Do not flatten GH2 tree/path behavior into lists.
- Do not treat public docs as compile truth when local WIP XML differs.
- Do not use current early implementation symbols as doctrine.
