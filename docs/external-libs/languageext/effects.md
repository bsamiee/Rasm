# [H1][LANGUAGEEXT_EFFECTS]

[IMPORTANT] Pin **`LanguageExt.Core`** at the version pinned in `Directory.Packages.props`. Runtime effects use concrete runtime records and `Eff.runtime<RT>()`. Boundary adapters may use statement control flow only with an explicit boundary marker in code.

## [1][RAILS]

| [INDEX] | [RAIL]                | [QUESTION]                         | [USE]                                                     |
| :-----: | --------------------- | ---------------------------------- | --------------------------------------------------------- |
|   [1]   | `Fin<T>`              | Did one operation succeed or fail? | Native calls, value admission, projection.                |
|   [2]   | `Validation<Error,T>` | Which inputs failed?               | Multi-input forms, symbols, requirement sets.             |
|   [3]   | `Eff<RT,T>`           | Which runtime context is needed?   | Document, filesystem, clock, bridge.                      |
|   [4]   | `IO<T>`               | Which side-effect is deferred?     | Resource, file, process, and host execution descriptions. |

## [2][RUNTIME_RECORD]

Use `Eff.runtime<RT>()` to read host capability records, then project with `Map`, `Bind`, `MapFail`, `BiMap`, or LINQ query syntax. Keep runtime records small and concrete. Do not introduce service locators, decorator containers, or unused package posture when a runtime record owns the dependency.

## [3][SCHEDULE]

Schedule builders: `recurs`, `spaced`, `linear`, `exponential`, `fibonacci`, `upto`, `jitter`, `maxDelay`.
Composition: `Schedule.a | Schedule.b`, `union`, `intersect`, `transformerA + transformerB` — full operator table in the same package doc set under operators §3.
Eff recovery: `Prelude.catch(...)`, `Prelude.retry(schedule, eff)`, `IfFailEff` — not generic `eff1 | eff2`.
IO: `IO<T>.Retry(Schedule)`, `Finally`, `Bracket`.

Pure domain `Fin`/`Validation` transforms avoid blocking delay schedules.

## [4][STATE]

| [INDEX] | [SURFACE]                           | [USE]                                                      |
| :-----: | ----------------------------------- | ---------------------------------------------------------- |
|   [1]   | `Atom<T>`                           | UI/session state with validated transitions.               |
|   [2]   | `AtomHashMap`, `AtomSeq`, `AtomQue` | Host-owned concurrent collections.                         |
|   [3]   | `Ref<T>` and STM                    | Coordinated mutable references under explicit transaction. |

Never use LanguageExt state to hide native object lifetime, host tree mutation, or ordinary domain accumulation.

## [5][RECOVERY]

- Convert native exceptions at boundary adapters into `Error` once.
- Prefer `MapFail`, `BiMap`, fallback alternatives, and verified catch combinators from local XML.
- Keep terminal collapse at command/component/tool edges.
- Preserve original operation, host object, tolerance, and input name in diagnostics.

## [6][V5_DELTAS]

`Directory.Packages.props` pins the `LanguageExt.Core` version. Verify deltas from v4 surface with `uv run python -m tools.quality api query LanguageExt.Core <symbol>`:

| [INDEX] | [V4_SURFACE]                                 | [V5_REPLACEMENT]                                                            |
| :-----: | -------------------------------------------- | --------------------------------------------------------------------------- |
|   [1]   | `Aff<T>` / `Aff<RT, T>`                      | `Eff<RT, T>` + `IO.liftAsync` inside the async leg.                         |
|   [2]   | `Resource<T>` top-level type                 | `IO<T>.Bracket`/`BracketFail`/`Finally`, `Prelude.use`.                     |
|   [3]   | `HasX<RT>` runtime constraints               | Concrete runtime records consumed via `Eff.runtime<RT>()`.                  |
|   [4]   | `LanguageExt.Pipes` (Producer/Consumer/Pipe) | Removed. `Atom<T>` + paint, `Channel<T>`, or process cache.                 |
|   [5]   | `StreamT<M, A>`                              | Removed. Same alternatives as Pipes.                                        |
|   [6]   | `Sequence(...)` extension                    | `seq.Traverse(identity) >> lower` or unary `+`; use `.As()` for `Eff`/`IO`. |
|   [7]   | Sequential `.Traverse`                       | `TraverseM` → `K<F, Seq<B>>`; lower via `>> lower`, unary `+`, or `.As()`.  |

**Validation monoid requirement (v5):** `Validation<E, A>` requires `E : Monoid<E>`. `Validation<Error, T>` works (`Error.Combine`); `Validation<string, T>` does NOT compile — use `StringM` or `Error`. Rasm forbids `Validation<Seq<Error>,T>` (`CSP0703`); GH UI uses `Validation<Seq<UiFault>,T>` — see `rasm.md`.
**Atom surface (v5):** `Atom<T>.Swap`/`SwapMaybe`/`SwapIO`/`SwapMaybeIO`. **No `Subscribe`/`OnChange`/`Reset`** — react-to-state patterns use the host paint loop or polling. `Atom<TMetadata, T>` two-arg form threads metadata through swap functions without closure allocation.
**Pattern matching:** Record-case patterns are faster than `.Match` and prefer-able for hot paths: `fin switch { Fin.Succ<int>(var x) => x, Fin.Fail<int>(var e) => 0 }`.
