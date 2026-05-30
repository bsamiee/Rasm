# [H1][RASM_USAGE]
>**Dictum:** *Advanced Rasm code chooses the owner before choosing the API.*

<br>

[IMPORTANT] Cross-stack implementation surface for new work. Leaf docs own package and member detail.

---
## [1][OWNER_LADDER]
>**Dictum:** *The strongest implementation uses each layer for its native concern.*

<br>

| [INDEX] | [OWNER] | [CAPABILITY] | [READ] |
| :-----: | ------- | ------------ | ------ |
| [1] | RhinoCommon | Geometry validity, tolerances, units, transforms, topology, curves, meshes. | `external-libs/mathnet/rhino.md`, local RhinoWIP XML |
| [2] | GH2 | `IDataAccess`, trees, paths, coverage, diagnostics, user-visible numeric policy. | `external-libs/mathnet/gh2.md`, local GH2 XML |
| [3] | MathNet | Linear algebra, solvers, fitting, optimization, statistics, symbolic formulas; CSparse at sparse direct factorization boundary. | `external-libs/mathnet/*.md` |
| [4] | BCL/System | Spans, generated regex, frozen lookup, generic math, SIMD/tensors, time, channels, IO/buffers, diagnostics. | `system-api-map/bcl.md`, `system-api-map/replacements.md`, `system-api-map/meta.md`, `system-api-map/packages.md` |
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

| [INDEX] | [SOURCE] | [OWNS] |
| :-----: | -------- | ------ |
| [1] | `Directory.Packages.props` | Pinned versions and central package state. |
| [2] | `Directory.Build.props` | Package references, global usings, RhinoWIP/GH2 host references. |
| [3] | Local NuGet XML / lockfiles | Exact API surface for pinned assets. |
| [4] | RhinoWIP XML / decompile | RhinoCommon, GH2, Rhino.UI, Eto compile truth. |
| [5] | `docs/system-api-map`, `docs/external-libs` | BCL and product-library policy after local proof. |
| [6] | Official docs | Current context only when local proof is silent. |

Performance and runtime-load claims require measured or load-context evidence before docs call them active.
