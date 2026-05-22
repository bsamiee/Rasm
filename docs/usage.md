# [H1][RASM_USAGE]
>**Dictum:** *Advanced Rasm code chooses the owner before choosing the API.*

<br>

[IMPORTANT] This guide stacks source-verified C# surfaces for new implementation work. It is not an API catalog; leaf docs own package and member detail.

---
## [1][OWNER_LADDER]
>**Dictum:** *The strongest implementation uses each layer for its native concern.*

<br>

| [INDEX] | [OWNER] | [CAPABILITY] | [READ] |
| :-----: | ------- | ------------ | ------ |
| [1] | RhinoCommon | Geometry validity, tolerances, units, transforms, topology, curves, meshes. | `external-libs/mathnet/rhino.md`, local RhinoWIP XML |
| [2] | GH2 | `IDataAccess`, trees, paths, coverage, diagnostics, user-visible numeric policy. | `external-libs/mathnet/gh2.md`, local GH2 XML |
| [3] | MathNet | Linear algebra, solvers, fitting, optimization, statistics, formulas. | `external-libs/mathnet/*.md` |
| [4] | BCL/System | Spans, generated regex, frozen lookup, generic math, runtime primitives, build metadata. | `system-api-map/*.md` |
| [5] | LanguageExt | `Fin`, `Validation`, `Eff`, `IO`, `Schedule`, `Seq`, `K<F,A>`. | `external-libs/languageext/*.md` |
| [6] | Thinktecture | Value objects, smart enums, unions, generated dispatch. | `external-libs/thinktecture/*.md` |

---
## [2][FLOW]
>**Dictum:** *Values move from native truth into rails, algorithms, and host output.*

<br>

1. Admit raw input through Rhino/GH2/System boundary policy.
2. Encode domain meaning with Thinktecture generated shapes.
3. Carry failure through LanguageExt `Fin`, `Validation`, or `Eff`.
4. Execute MathNet only for algorithmic numeric or symbolic work.
5. Project output back through Rhino validity or GH2 tree/diagnostic rules.

---
## [3][PATTERNS]
>**Dictum:** *Stacking power comes from explicit ownership, not wrappers.*

<br>

| [INDEX] | [PATTERN] | [GUIDANCE] |
| :-----: | --------- | ---------- |
| [1] | Domain rail | Thinktecture admits values; LanguageExt carries validation and effects. |
| [2] | Rhino numeric | Rhino validates geometry; MathNet solves selected numeric projection; Rhino validates output. |
| [3] | Symbolic GH2 | GH2 reads formula text; Symbolics parses/evaluates; diagnostics report exact failed stage. |
| [4] | System primitive | BCL handles grammar, lookup, spans, timing, or diagnostics only when it owns the concern. |

---
## [4][REJECTIONS]
>**Dictum:** *False sophistication creates weak code.*

<br>

- Do not wrap library APIs without domain value.
- Do not add packages as future intent.
- Do not let MathNet override Rhino geometry semantics.
- Do not flatten GH2 tree/path behavior into lists.
- Do not treat public docs as compile truth when local WIP XML differs.
- Do not use current early implementation symbols as doctrine.

---
## [5][PROOF]
>**Dictum:** *Every advanced claim carries a proof path.*

<br>

Use repo manifests for versions and references, local NuGet XML for package APIs, local RhinoWIP XML/decompile for host APIs, and official docs for current context. Performance and runtime-load claims require measured or load-context evidence before docs call them active.
