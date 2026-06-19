# [RASM_USAGE]

Cross-stack implementation surface for new work. The owner ladder selects the package, platform, and API rail that owns each capability.

## [01]-[OWNER_LADDER]

| [INDEX] | [OWNER]            | [CAPABILITY]                                                              |
| :-----: | ------------------ | ------------------------------------------------------------------------- |
|  [01]   | RhinoCommon        | Geometry validity, tolerances, units, transforms, topology                |
|  [02]   | GH2                | `IDataAccess`, trees, paths, coverage, diagnostics                        |
|  [03]   | MathNet            | Linear algebra, solvers, fitting, optimization, statistics, symbolics     |
|  [04]   | BCL/System         | Spans, regex, frozen lookup, generic math, SIMD, time, IO, diagnostics    |
|  [05]   | LanguageExt        | `Fin`, `Validation`, `Eff`, `IO`, `Schedule`, `Seq`, `K<F,A>`             |
|  [06]   | Thinktecture       | Value objects, smart enums, unions, generated dispatch                    |
|  [07]   | Platform libraries | `Rasm.AppUi`, `Rasm.AppHost`, `Rasm.Persistence`, `Rasm.Compute`          |
|  [08]   | Composition root   | Generic Host, Scrutor, health, telemetry, HTTP resilience; bootstrap only |

Choose the first owner that directly owns the capability. Local platform surfaces integrate or compose approved libraries through their owner rails; they do not replace package-owned behavior with wrapper vocabulary.

## [02]-[FLOW]

1. Receive raw input through Rhino/GH2/System boundary policy.
2. Encode domain meaning with Thinktecture generated shapes.
3. Carry failure through LanguageExt `Fin`, `Validation`, or `Eff`.
4. Execute MathNet only for algorithmic numeric work; symbolic work requires a numeric capability gate.
5. Project output back through Rhino validity or GH2 tree/diagnostic rules.
6. Route app UI, runtime, persistence, and compute concerns through the platform rail that owns the capability before adding packages or public surfaces.
7. Keep provider APIs inside the owner rail that owns them; public vocabulary stays provider-neutral.
8. Build platform rails from the complete feature set: host, companion, sidecar, downstream app, provider, asset, storage, compute, telemetry, support, and confirmation variants are parameterized cases inside existing shapes before new public surfaces appear.

## [03]-[PATTERNS]

| [INDEX] | [PATTERN]        | [GUIDANCE]                                                                                    |
| :-----: | ---------------- | --------------------------------------------------------------------------------------------- |
|  [01]   | Domain rail      | Thinktecture shapes values; LanguageExt carries validation and effects.                       |
|  [02]   | Rhino numeric    | Rhino validates geometry; MathNet solves selected numeric projection; Rhino validates output. |
|  [03]   | Symbolic GH2     | GH2 reads formula text; Symbolics parses/evaluates; diagnostics report exact failed stage.    |
|  [04]   | System primitive | BCL handles grammar, lookup, spans, timing, and diagnostics it owns directly.                 |

## [04]-[BOUNDARIES]

- Library APIs stay direct unless the local owner adds domain value.
- Packages enter only when an owner consumes their capability.
- Build-contract package records are implementation inputs, not runtime confirmation.
- Rhino geometry semantics outrank MathNet numeric convenience at geometry boundaries.
- GH2 tree/path behavior stays tree-shaped.
- Local WIP XML and API lookup outrank public docs for compile behavior.
- Early implementation symbols become doctrine only after the owning surface admits them.
