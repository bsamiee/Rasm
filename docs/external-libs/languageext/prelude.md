# [H1][LANGUAGEEXT_PRELUDE]
>**Dictum:** *Prelude is globally imported; treat it as syntax, not ceremony.*

<br>

[IMPORTANT] Rasm injects `using static LanguageExt.Prelude` from `Directory.Build.props`. Per-file `using static` is redundant. This file catalogs **advanced** Prelude surfaces used in Rasm production — not basic `Map`/`Bind`.

---
## [1][OPTION_AND_FIN]
>**Dictum:** *Lift host sentinels before domain logic.*

<br>

| [INDEX] | [MEMBER] | [RASM_USE] |
| :-----: | -------- | ---------- |
| [1] | `Some(x)` / `None` / `Option<T>.None` | heavy |
| [2] | `Optional(x)` | heavy — pair with `.ToFin(Fail:…)` at boundary |
| [3] | `Fin.Succ` / `Fin.Fail` | heavy — primary `Fin` constructors |
| [4] | `guard(condition, error)` | heavy — `from` chain preconditions |
| [5] | `optA \| optB` | moderate — see `operators.md` for carrier disambiguation |

`FinSucc` / `FinFail` exist in Prelude XML; Rasm production uses **`Fin.Succ` / `Fin.Fail`** exclusively.

`guardnot` exists in Prelude XML; **zero** production usage in `libs/csharp/` today.

---
## [2][COLLECTION_AND_IDENTITY]
>**Dictum:** *Normalize shape once at the pipeline edge.*

<br>

| [INDEX] | [MEMBER] | [RASM_USE] |
| :-----: | -------- | ---------- |
| [1] | `toSeq(...)` | heavy |
| [2] | `toHashMap(...)` | moderate — e.g. layout grouping |
| [3] | `unit` | heavy — void in FP pipelines |
| [4] | `identity` | moderate — `.Traverse(identity).As()` |
| [5] | `Seq<T>.Cons(...)` | moderate — **not** a top-level Prelude function |

---
## [3][TIME_AND_RESOURCE]
>**Dictum:** *Duration and resource helpers are boundary concerns.*

<br>

| [INDEX] | [MEMBER] | [NOTES] |
| :-----: | -------- | ------- |
| [1] | `LanguageExt.UnitsOfMeasure.ms`, `sec`, … | **Not** global in Rasm; import explicitly for Schedule examples |
| [2] | `Prelude.use` / `IO<T>.Bracket` | Exist in v5 XML; **zero** production usage in `libs/csharp/` |

---
## [4][RULES]
>**Dictum:** *Prelude removes imports; it does not remove rail discipline.*

<br>

- Do not re-wrap Prelude members in repo helpers.
- Prefer `Optional(x).ToFin(Fail:…)` over null checks in domain modules.
- Pair `identity` with `.Traverse(identity).As()` when lowering v5 `K<F, Seq<A>>`.
- Native exception boundaries use `Try.lift<Fin<T>>(…).Run()` — see `combinators.md`.
