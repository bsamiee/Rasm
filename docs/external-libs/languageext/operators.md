# [H1][LANGUAGEEXT_OPERATORS]
>**Dictum:** *The same symbol can mean different algebras; disambiguate the carrier before composing.*

<br>

[IMPORTANT] Rasm pins `LanguageExt.Core` `5.0.0-beta-77`. Verify overload semantics in local XML before documenting new operator uses. Hand-written `operator +`/`|` on Rasm structs are **not** LanguageExt operators — see `../thinktecture/union-attributes.md`.

---
## [1][DISAMBIGUATION]
>**Dictum:** *Four `|` families and three `+` families coexist in Rasm C#.*

<br>

| [INDEX] | [CARRIER] | [SYMBOL] | [SEMANTICS] | [RASM_PRODUCTION] |
| :-----: | --------- | -------- | ----------- | ----------------- |
| [1] | `Option<T>` | `\|` | LanguageExt alternative (verify bias in pinned XML) | moderate |
| [2] | `[Flags]` enum | `\|` | Bitwise OR on capability masks | common |
| [3] | Rasm struct/record | `\|` | Hand-written absorption lattice (`RepaintRequest`, `Subscription`, `FileOverride<T>`) | heavy |
| [4] | `Schedule` / `ScheduleTransformer` | `\|` | Chain retry transformers via `op_Addition` | **unused in libs/** |
| [5] | `Validation<E,A>` | `&` | Applicative product (`op_BitwiseAnd` in XML) | **unused** — prefer tuple `.Apply().As()` |
| [6] | `Error` | `+` | Monoid append when folding faults (`Ui.cs` fault fold) | GH UI boundary |
| [7] | Rasm struct/record | `+` | Hand semigroup append (`Requirement`, `VectorField`, receipts, plans) | heavy |

[NEVER] Conflate Rasm lattice `\|` with LanguageExt `Option |` or `[Flags] |`.

---
## [2][SCHEDULE_ALGEBRA]
>**Dictum:** *Retry policy is LanguageExt surface; Rasm production does not use it yet.*

<br>

| [INDEX] | [EXPRESSION] | [MEANING] |
| :-----: | ------------ | --------- |
| [1] | `Schedule.exponential(...) \| Schedule.jitter(...) \| Schedule.recurs(...)` | Chain transformers via `\|` on `ScheduleTransformer` |
| [2] | `intersect(policy, Schedule.upto(duration))` | Bound intersection — **not** C# `&` on `Schedule` |
| [3] | `200 * LanguageExt.UnitsOfMeasure.ms` | Duration literals require `UnitsOfMeasure` import (not global in Rasm) |

Schedule operators belong at composition boundaries only. Domain transforms stay on `Fin` or `Validation`. Zero `@catch`, `.Retry(`, or `Schedule` usage in production `libs/csharp/` today.

Eff recovery in LanguageExt v5 uses **`Prelude.catch(...)`** / `IfFailEff`, not arbitrary `eff1 | eff2`. XML binds `Eff |` to `Finally` composition — see local `LanguageExt.Core.xml`.

---
## [3][OPTION_AND_VALIDATION]
>**Dictum:** *Product and alternative answer different failure questions.*

<br>

| [INDEX] | [FORM] | [USE] |
| :-----: | ------ | ----- |
| [1] | `optA \| optB` | Optional host projection merge |
| [2] | `(a,b,c).Apply(f).As()` | **Preferred** applicative product for independent fields |
| [3] | `v1 & v2` | Validation product — exists in LE; Rasm uses `.Apply()` instead |

`Validation<string,T>` does **not** compile in v5. Use `Validation<Error,T>`, `StringM`, or `Validation<Seq<UiFault>,T>` per `effects.md` and `rasm.md`.

---
## [4][NOT_USED_IN_RASM]
>**Dictum:** *Absence is architectural, not ignorance.*

<br>

| [INDEX] | [SURFACE] | [RASM_SUBSTITUTE] |
| :-----: | --------- | ----------------- |
| [1] | Kleisli `>>` / `<<` | LINQ `from..in..select`; trait arrows in skill `transforms.md` only |
| [2] | Mapster `.Adapt<T>()` | Thinktecture factories; one GH2 native `.Adapt` at boundary |
| [3] | `\|>` pipeline operator | Not in LanguageExt v5 Rasm graph |
| [4] | LanguageExt `Decision` type | **Does not exist** in beta-77 XML — use Rasm `OverlayDecision` or GH `Components.Decision` |

---
## [5][RULES]
>**Dictum:** *Operators encode algebra; document the carrier before the symbol.*

<br>

- Verify every LanguageExt operator claim against pinned `LanguageExt.Core.xml`.
- Keep hand `\|`/`+` on domain types explicit in the owning type.
- Cross-reference `combinators.md` for method-form equivalents.
- Cross-reference `../thinktecture/union-attributes.md` for Rasm `[SkipUnionOps]` / `[GenerateUnionOps]` (SelfOp emission — not Thinktecture union ops).
