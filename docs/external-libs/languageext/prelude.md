# [LANGUAGEEXT_PRELUDE]

[IMPORTANT] Pin **`LanguageExt.Core`** at the version pinned in `Directory.Packages.props`. When global usings include `static LanguageExt.Prelude`, per-file `using static` is redundant.

Advanced Prelude surfaces — not basic `Map`/`Bind`.

## [1][OPTION_AND_FIN]

| [INDEX] | [MEMBER]                              | [ROLE]                                                               |
| :-----: | ------------------------------------- | -------------------------------------------------------------------- |
|   [1]   | `Some(x)` / `None` / `Option<T>.None` | Option constructors                                                  |
|   [2]   | `Optional(x)`                         | Lift nullable/reference; pair with `.ToFin(Fail:…)` at boundary      |
|   [3]   | `Fin.Succ` / `Fin.Fail`               | Primary `Fin` constructors                                           |
|   [4]   | `FinSucc` / `FinFail`                 | Alternate Prelude constructors — same semantics                      |
|   [5]   | `guard(condition, error)`             | Monadic chain precondition                                           |
|   [6]   | `guardnot(condition, error)`          | Negated guard                                                        |
|   [7]   | `validationA \| validationB`          | `Validation` choice on K-carrier — not bare `Option<T> \| Option<T>` |

## [2][COLLECTION_AND_IDENTITY]

| [INDEX] | [MEMBER]           | [ROLE]                                                                                  |
| :-----: | ------------------ | --------------------------------------------------------------------------------------- |
|   [1]   | `toSeq(...)`       | Normalize to `Seq<T>`                                                                   |
|   [2]   | `toHashMap(...)`   | Normalize to `HashMap<K,V>`                                                             |
|   [3]   | `unit`             | Unit value in FP pipelines                                                              |
|   [4]   | `identity`         | Identity function — `.Traverse(identity) >> lower` or unary `+`; `.As()` for `Eff`/`IO` |
|   [5]   | `Seq<T>.Cons(...)` | List cons — not a top-level Prelude function                                            |

## [3][SCHEDULE_AND_TIME]

| [INDEX] | [MEMBER]                                  | [NOTES]                                       |
| :-----: | ----------------------------------------- | --------------------------------------------- |
|   [1]   | `Prelude.use` / `IO<T>.Bracket`           | Resource scope                                |
|   [2]   | `LanguageExt.UnitsOfMeasure.ms`, `sec`, … | Duration literals — explicit namespace import |

Schedule builders and algebra (`|`, `union`, `intersect`, `+` on transformers): operators doc §3.

## [4][RECOVERY]

| [INDEX] | [MEMBER]                                                    | [ROLE]                                                           |
| :-----: | ----------------------------------------------------------- | ---------------------------------------------------------------- |
|   [1]   | `Prelude.catch(...)` / `@catch` / `catchOf` / `catchOfFold` | Effect recovery                                                  |
|   [2]   | `IfFailEff` / `IfFail`                                      | Instance methods on Eff / Validation / Try — not Prelude members |

## [5][RULES]

- Do not re-wrap Prelude members in application helpers.
- Prefer `Optional(x).ToFin(Fail:…)` over null checks in domain modules.
- Pair `identity` with `.Traverse(identity) >> lower` (or `.As()` on Eff/IO) when lowering `K<F, Seq<A>>`.
- Native exception boundaries: `Try.lift<Fin<T>>(…).Run()`.
