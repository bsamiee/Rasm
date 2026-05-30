# [H1][LANGUAGEEXT_PRELUDE]
>**Dictum:** *Prelude is globally imported when configured; treat it as syntax, not ceremony.*

<br>

[IMPORTANT] Pin **`LanguageExt.Core` `5.0.0-beta-77`**. When global usings include `static LanguageExt.Prelude`, per-file `using static` is redundant.

This file catalogs **advanced** Prelude surfaces — not basic `Map`/`Bind`.

---
## [1][OPTION_AND_FIN]
>**Dictum:** *Lift absence before domain logic.*

<br>

| [INDEX] | [MEMBER] | [ROLE] |
| :-----: | -------- | ------ |
| [1] | `Some(x)` / `None` / `Option<T>.None` | Option constructors |
| [2] | `Optional(x)` | Lift nullable/reference; pair with `.ToFin(Fail:…)` at boundary |
| [3] | `Fin.Succ` / `Fin.Fail` | Primary `Fin` constructors |
| [4] | `FinSucc` / `FinFail` | Alternate Prelude constructors — same semantics |
| [5] | `guard(condition, error)` | Monadic chain precondition |
| [6] | `guardnot(condition, error)` | Negated guard |
| [7] | `validationA \| validationB` | `Validation` choice on K-carrier — see `operators.md`; not bare `Option<T> \| Option<T>` |

---
## [2][COLLECTION_AND_IDENTITY]
>**Dictum:** *Normalize shape once at the pipeline edge.*

<br>

| [INDEX] | [MEMBER] | [ROLE] |
| :-----: | -------- | ------ |
| [1] | `toSeq(...)` | Normalize to `Seq<T>` |
| [2] | `toHashMap(...)` | Normalize to `HashMap<K,V>` |
| [3] | `unit` | Unit value in FP pipelines |
| [4] | `identity` | Identity function — `.Traverse(identity) >> lower` or unary `+`; `.As()` for `Eff`/`IO` |
| [5] | `Seq<T>.Cons(...)` | List cons — **not** a top-level Prelude function |

---
## [3][SCHEDULE_AND_TIME]
>**Dictum:** *Duration and retry helpers are boundary concerns.*

<br>

| [INDEX] | [MEMBER] | [NOTES] |
| :-----: | -------- | ------- |
| [1] | `Schedule.exponential`, `spaced`, `jitter`, `recurs`, `upto` | Retry policy building blocks |
| [2] | `union(schedule, schedule)` / `intersect(schedule, schedule)` | Schedule composition — Prelude functions |
| [3] | `LanguageExt.UnitsOfMeasure.ms`, `sec`, … | Duration literals — explicit namespace import |
| [4] | `Prelude.use` / `IO<T>.Bracket` | Resource scope |

See `operators.md` §3 for Schedule algebra.

---
## [4][RECOVERY]
>**Dictum:** *Catch and retry are explicit Prelude entrypoints.*

<br>

| [INDEX] | [MEMBER] | [ROLE] |
| :-----: | -------- | ------ |
| [1] | `Prelude.catch(...)` / `@catch` / `catchOf` / `catchOfFold` | Effect recovery |
| [2] | `IfFailEff` / `IfFail` | Instance methods on Eff / Validation / Try — not Prelude members |

---
## [5][RULES]
>**Dictum:** *Prelude removes imports; it does not remove rail discipline.*

<br>

- Do not re-wrap Prelude members in application helpers.
- Prefer `Optional(x).ToFin(Fail:…)` over null checks in domain modules.
- Pair `identity` with `.Traverse(identity) >> lower` (or `.As()` on Eff/IO) when lowering `K<F, Seq<A>>`.
- Native exception boundaries: `Try.lift<Fin<T>>(…).Run()` — see `combinators.md`.
