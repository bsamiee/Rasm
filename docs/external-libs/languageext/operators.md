# [H1][LANGUAGEEXT_OPERATORS]
>**Dictum:** *Operators encode algebra on a carrier; identify the carrier before composing.*

<br>

[IMPORTANT] Pin **`LanguageExt.Core` `5.0.0-beta-77`**. Verify overload semantics in local package XML.

Hand-written domain `operator +`/`|` on application types are **not** LanguageExt operators.

---
## [1][KLEISLI_AND_APPLICATIVE]
>**Dictum:** *Bind sequences dependent steps; star and triple-angle sequence independent applicative steps.*

<br>

| [INDEX] | [SYMBOL]               | [CARRIER]             | [SEMANTICS]                              |
| :-----: | ---------------------- | --------------------- | ---------------------------------------- |
|   [1]   | `>>`                   | `K<F,A>` monads       | Kleisli bind / discard-first sequence    |
|   [2]   | `>>>`                  | `K<F,A>` applicatives | Applicative sequence (independent steps) |
|   [3]   | `*`                    | `K<F,A>`              | Functor map / applicative apply          |
|   [4]   | `>> lower` / unary `+` | `K<F,A>`              | Downcast via `Prelude.lower`             |
|   [5]   | `&`                    | `Validation<E,A>`     | Applicative product — accumulate errors  |
|   [6]   | `+`                    | `Error` / monoid `E`  | Append errors in validation folds        |

Prefer tuple `.Apply(f)` with explicit lowering, or `Validation` `&`, when independent fields compose.

`Validation<string,T>` is **not supported** — error type needs `Semigroup`/`Monoid` (use `StringM` or `Error`). Rasm forbids `Validation<Seq<Error>,T>` (`CSP0703`); see `rasm.md`.

---
## [2][CHOICE_CATCH_FINALLY]
>**Dictum:** *Pipe meaning depends on carrier — not a single alternative algebra.*

<br>

| [INDEX] | [CARRIER]               | [SYMBOL] | [SEMANTICS]                           |
| :-----: | ----------------------- | -------- | ------------------------------------- |
|   [1]   | `K<Validation<E,A>>`    | `\|`     | Choice — first success wins           |
|   [2]   | `Fallible` + `CatchM`   | `\|`     | Catch when predicate matches          |
|   [3]   | `Eff<RT,A>` + `Finally` | `\|`     | Run finally after main effect         |
|   [4]   | `[Flags]` enum          | `\|`     | Bitwise OR — unrelated to LanguageExt |

**Not documented as bare `Option<T> | Option<T>` alternative** in pinned XML — use `Choose`, `IfNone`, `Match`, or `Alternative` trait methods.

Eff recovery: **`Prelude.catch(...)`**, **`IfFailEff`**, **`IfFail`** — not unconstrained `eff1 | eff2`.

---
## [3][SCHEDULE_ALGEBRA]
>**Dictum:** *Retry policy composes with union, intersect, pipe, and transformer chain.*

<br>

| [INDEX] | [EXPRESSION]                                                                 | [MEANING]                                               |
| :-----: | ---------------------------------------------------------------------------- | ------------------------------------------------------- |
|   [1]   | `Schedule.a \| Schedule.b`                                                   | Schedule union (documented in Schedule type remarks)    |
|   [2]   | `union(scheduleA, scheduleB)` / `intersect(policy, Schedule.upto(duration))` | Prelude combine                                         |
|   [3]   | `transformerA + transformerB`                                                | Chain `ScheduleTransformer` instances                   |
|   [4]   | `append`, `interleave`, `take`, `skip`, `tail`, `map`, `filter`, `bind`      | Additional Prelude schedule transforms                  |
|   [5]   | `200 * LanguageExt.UnitsOfMeasure.ms`                                        | Duration literals — import `LanguageExt.UnitsOfMeasure` |

Schedule intersect uses **`intersect(...)`** or documented fullwidth intersect glyph in XML examples — **not** ASCII `&` on `Schedule`.

Pair with `IO<T>.Retry(Schedule)`, `Prelude.retry` / `repeat`, or `@catch` at effect boundaries.

---
## [4][NOT_IN_PINNED_XML]
>**Dictum:** *Absence in XML is absence in API.*

<br>

| [INDEX] | [CLAIM]                                              | [STATUS]                         |
| :-----: | ---------------------------------------------------- | -------------------------------- |
|   [1]   | LanguageExt `Decision` type                          | Absent                           |
|   [2]   | `\|>` pipeline operator                              | Absent                           |
|   [3]   | `ComposeK`, `HyloM`, `FoldArrows` as shipped Prelude | Absent — schematic patterns only |

Use LINQ `from..in..select` for monadic composition. **`Next.Loop`** exists for trampolining.

---
## [5][RULES]
>**Dictum:** *Document carrier before symbol.*

<br>

- Verify every operator claim against pinned `LanguageExt.Core.xml`.
- Disambiguate hand domain operators from LanguageExt and from `[Flags]`.
