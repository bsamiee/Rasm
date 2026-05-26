# [H1][LANGUAGEEXT_EFFECTS]
>**Dictum:** *Effects preserve execution and failure meaning until the host consumes them.*

<br>

[IMPORTANT] Rasm runtime effects use runtime records and `Eff.runtime<RT>()`. Boundary adapters may use statement control flow only with an explicit boundary marker in code.

---
## [1][RAILS]
>**Dictum:** *One rail answers one failure question.*

<br>

| [INDEX] | [RAIL] | [QUESTION] | [USE] |
| :-----: | ------ | ---------- | ----- |
| [1] | `Fin<T>` | Did one operation succeed or fail? | Native calls, value admission, projection. |
| [2] | `Validation<Error,T>` | Which independent inputs failed? | Multi-input GH2, formula symbols, requirement sets. |
| [3] | `Eff<RT,T>` | Which runtime context is needed? | Rhino docs, GH runtime, filesystem, clock, bridge. |
| [4] | `IO<T>` | Which side-effect is deferred? | Resource, file, process, and host execution descriptions. |

---
## [2][RUNTIME_RECORD]
>**Dictum:** *Runtime records expose capabilities without service location.*

<br>

Use `Eff.runtime<RT>()` to read host capability records, then project with `Map`, `Bind`, `MapFail`, `BiMap`, or LINQ query syntax. Keep runtime records small and concrete. Do not introduce service locators, decorator containers, or unused package posture when a runtime record owns the dependency.

---
## [3][SCHEDULE]
>**Dictum:** *Retry policy is algebra, not exception plumbing.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `Schedule.recurs`, `spaced`, `linear`, `exponential`, `fibonacci` | Bounded retry/repeat cadence. |
| [2] | `upto`, `fixedInterval`, `windowed` | Time or count limits. |
| [3] | `maxDelay`, `maxCumulativeDelay`, `jitter`, `decorrelate`, `resetAfter` | Host-friendly backoff shaping. |
| [4] | `IO<T>.Retry`, `Repeat`, `Timeout`, `Fork`, `Finally`, `Bracket` | Resource and retry composition. |

Use schedule policy at composition boundaries only. Domain transforms stay pure and fallible through `Fin` or `Validation`.

---
## [4][STATE]
>**Dictum:** *Managed state belongs to hosts, not domain transforms.*

<br>

| [INDEX] | [SURFACE] | [USE] |
| :-----: | --------- | ----- |
| [1] | `Atom<T>` | UI/session state with validated transitions. |
| [2] | `AtomHashMap`, `AtomSeq`, `AtomQue` | Host-owned concurrent collections. |
| [3] | `Ref<T>` and STM | Coordinated mutable references under explicit transaction. |

Never use LanguageExt state to hide Rhino object lifetime, GH2 tree mutation, or ordinary domain accumulation.

---
## [5][RECOVERY]
>**Dictum:** *Recovery projects typed failures, not raw exceptions.*

<br>

- Convert native exceptions at boundary adapters into `Error` once.
- Prefer `MapFail`, `BiMap`, fallback alternatives, and verified catch combinators from local XML.
- Keep terminal collapse at command/component/tool edges.
- Preserve original operation, host object, tolerance, and input name in diagnostics.

---
## [6][V5_DELTAS]
>**Dictum:** *v5 reshaped the effect surface; pinned local XML is the only truth.*

<br>

`Directory.Packages.props` pins `LanguageExt.Core 5.0.0-beta-77`. Verified deltas from v4 surface (against `~/.nuget/packages/languageext.core/5.0.0-beta-77/lib/net10.0/LanguageExt.Core.xml`):

| [INDEX] | [V4_SURFACE] | [V5_REPLACEMENT] |
| :-----: | ------------ | ---------------- |
| [1] | `Aff<T>` / `Aff<RT, T>` | `Eff<RT, T>` + `IO.liftAsync` inside the async leg. |
| [2] | `Resource<T>` top-level type | `IO<T>.Bracket(use, finally)`, `IO<T>.BracketFail`, `IO<T>.Finally`, `Prelude.use`. |
| [3] | `HasX<RT>` runtime constraints | Concrete runtime records consumed via `Eff.runtime<RT>()`. |
| [4] | `LanguageExt.Pipes` (Producer/Consumer/Pipe) | Gone with no replacement. Use `Atom<T>` + paint hook, `Channel<T>`, or process-static cache per use case. |
| [5] | `StreamT<M, A>` | Gone with no replacement. Same alternatives as Pipes. |
| [6] | `Sequence(...)` extension | `seq.Traverse(identity).As()`. |
| [7] | Sequential `.Traverse` | `TraverseM` returns `K<F, Seq<B>>`; `.As()` lowering is MANDATORY to recover `Fin<Seq<B>>` / `Option<Seq<B>>`. |

**Validation monoid requirement (v5):** `Validation<E, A>` requires `E : Monoid<E>`. `Validation<Error, T>` works (`Error.Combine`); `Validation<string, T>` does NOT compile — use `StringM` or `Seq<Error>`.

**Atom surface (v5):** `Atom<T>.Swap`/`SwapMaybe`/`SwapIO`/`SwapMaybeIO`. **No `Subscribe`/`OnChange`/`Reset`** — react-to-state patterns use the host paint loop or polling. `Atom<TMetadata, T>` two-arg form threads metadata through swap functions without closure allocation.

**Pattern matching:** Record-case patterns are faster than `.Match` and prefer-able for hot paths: `fin switch { Fin.Succ<int>(var x) => x, Fin.Fail<int>(var e) => 0 }`.
