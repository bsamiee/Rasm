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
| [5] | GH UI parallel checks with batched faults. | `Validation<Seq<UiFault>,T>` |
| [6] | Thinktecture VO factory with custom fault type. | `[ValidationError<TFault>]` on generated `Validate` |

[NEVER] Use `Validation<string,T>` — v5 requires `E : Monoid<E>`; use `StringM` or `Validation<Error,T>`. [NEVER] Use `Validation<Seq<Error>,T>` in domain/application/shared (`CSP0703`). GH UI parallel faults: `Validation<Seq<UiFault>,T>`.

Global Prelude is injected from `Directory.Build.props`. See `prelude.md`.

Domain semigroup/lattice operators (`+`, `|` on receipts, repaint policy, fields) are **Rasm-defined**, not LanguageExt — see `operators.md` §1.

Decision-style monoids are **host types**, not LanguageExt: `OverlayDecision` (Rhino overlay), `Rasm.Grasshopper.Components.Decision` (component input routing).

---
## [2][BOUNDARY_POLICY]
>**Dictum:** *Native hosts receive concrete values after rails preserve intent.*

<br>

- Collapse rails at Rhino command, GH2 component, CLI, or test boundary only.
- Preserve operation name, input nickname, native type, tolerance, and failure stage in errors.
- Convert Rhino mutable/disposable values into Rasm-owned projections before long-lived storage.
- Keep GH2 tree/path semantics at the GH2 boundary; do not flatten them for local convenience.

---
## [3][BOUNDARY_PATTERNS]
>**Dictum:** *Two shapes recur at every host boundary; name them once.*

<br>

| [INDEX] | [PATTERN] | [SHAPE] |
| :-----: | --------- | ------- |
| [1] | Native sentinel projection | `DateTime.MinValue`, `Guid.Empty`, `Point3d.Origin`, `UnitSystem.None`, `RhinoMath.UnsetIntIndex`, and non-positive numerics project to `Option<T>.None` at the adapter; never propagate sentinels inward. |
| [2] | Event subscription disposal | Sealed `IDisposable` owns a `Seq<Action>` of detachers; one polymorphic `Attach<TArgs>` captures the wrapper delegate so `+=`/`-=` are symmetric; the factory returns `Fin<IDisposable>` so attach failure stays on the rail. |
| [3] | Native state bracketing | Native-script transitions (snapshot restore, named layer state, viewport mode) bracket the body with save-to-sentinel, restore-target, body, restore-sentinel, delete-sentinel; the sentinel name is `Guid`-based so the user's persisted table is left intact and the try/finally guarantees cleanup. |

- Apply sentinel projection at the same call site that crosses the host boundary; do not lift it into a separate helper module.
- Build subscription disposers as one sealed type per boundary owner; do not generalize across unrelated host surfaces.
- Build state brackets as a single private helper per boundary owner; do not expose the sentinel name in public API.

---
## [4][OP_BOUNDARY]
>**Dictum:** *Op is the branded admission capsule at every native edge.*

<br>

Defined in `libs/csharp/Rasm/Domain/Validation.cs`. Consumed across Rasm, Rhino, and Grasshopper.

| [INDEX] | [MEMBER] | [USE] |
| :-----: | -------- | ----- |
| [1] | `Op.Of(name)` / `key.OrDefault()` | Branded operation identity from caller context |
| [2] | `op.Need(value)` | Nullable/reference admission → `Fin` |
| [3] | `op.Catch(() => Fin<T>)` | Native exception → typed `Fin` once |
| [4] | `Op.Side` / `Op.SideWhen` | Host side-effect on success rail |
| [5] | `op.AcceptValidated<TVO>(value)` | Thinktecture VO factory → `Fin` |
| [6] | `op.ValidateParallel` / `AcceptAll` | GH UI parallel checks → `Validation<Seq<UiFault>,T>` |
| [7] | `[BoundaryAdapter]` | CSP analyzer marks adapter-only APIs |

GH extensions (`AcceptPoint`, `AcceptRect`, `Attempt`) live in `Rasm.Grasshopper/UI/Ui.cs` — same rail, UI coordinate vocabulary.

---
## [5][NOISE_REJECTION]
>**Dictum:** *LanguageExt removes ceremony when used directly.*

<br>

- Do not create helper wrappers around `Fin`, `Validation`, `Eff`, `Seq`, or `Schedule`.
- Do not add service-location or container doctrine to Rasm effect code.
- Do not make current implementation symbols canonical doc truth.
- Do not document unused LanguageExt package families without a pinned consumer.
