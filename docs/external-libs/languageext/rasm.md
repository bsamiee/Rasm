# [H1][LANGUAGEEXT_RASM]
>**Dictum:** *Rasm code flows through one rail until a host boundary consumes it.*

<br>

[IMPORTANT] This file states repo posture only. API detail belongs in sibling reference files.

---
## [1][RAIL_POLICY]
>**Dictum:** *Rail choice is architecture, not syntax.*

<br>

| [INDEX] | [SCENARIO] | [RAIL] |
| :-----: | ---------- | ------ |
| [1] | Native value admission, Rhino validity, MathNet result projection. | `Fin<T>` |
| [2] | Independent input requirements, formula symbol sets, GH2 multi-port reads. | `Validation<Error,T>` |
| [3] | Rhino document, GH2 runtime, filesystem, bridge, clock, process. | `Eff<RT,T>` |
| [4] | Deferred resource or side-effect description. | `IO<T>` |

---
## [2][BOUNDARY_POLICY]
>**Dictum:** *Native hosts receive concrete values after rails preserve intent.*

<br>

- Collapse rails at Rhino command, GH2 component, CLI, or test boundary only.
- Preserve operation name, input nickname, native type, tolerance, and failure stage in errors.
- Convert Rhino mutable/disposable values into Rasm-owned projections before long-lived storage.
- Keep GH2 tree/path semantics at the GH2 boundary; do not flatten them for local convenience.

---
## [3][NOISE_REJECTION]
>**Dictum:** *LanguageExt removes ceremony when used directly.*

<br>

- Do not create helper wrappers around `Fin`, `Validation`, `Eff`, `Seq`, or `Schedule`.
- Do not add service-location or container doctrine to Rasm effect code.
- Do not make current implementation symbols canonical doc truth.
- Do not document unused LanguageExt package families without a pinned consumer.
