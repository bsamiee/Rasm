# [LANGUAGEEXT_COMBINATORS]

[IMPORTANT] LanguageExt combinators use the active `LanguageExt.Core` surface.

## [1][LOWERING]

| [INDEX] | [CARRIER]    | [LOWER AFTER TRAVERSEM]               |
| :-----: | ------------ | ------------------------------------- |
|   [1]   | `Fin`        | `.TraverseM(f) >> lower` or unary `+` |
|   [2]   | `Validation` | `.TraverseM(f) >> lower`              |
|   [3]   | `Option`     | `.TraverseM(f) >> lower`              |
|   [4]   | `Eff` / `IO` | `.TraverseM(f).As()`                  |

`Prelude.lower` and `k >> lower` are documented downcast operators in package XML.

## [2][RAIL_TRANSFORMS]

| [INDEX] | [COMBINATOR]                                   | [CARRIERS]                        | [NOTES]             |
| :-----: | ---------------------------------------------- | --------------------------------- | ------------------- |
|   [1]   | `.MapFail(f)`                                  | `Fin`, `Validation`, `Eff`, `Try` | Map failure value   |
|   [2]   | `.BindFail(f)`                                 | `Fin`, `Validation`, `Try`        | Not on `Eff`        |
|   [3]   | `.IfFail(f)`                                   | `Validation`, `Try`, `Eff`        | Not on `Fin`        |
|   [4]   | `.BiBind(Succ:, Fail:)`                        | `Fin`, `Validation`               | Both branches       |
|   [5]   | `.BiMap(succ, fail)`                           | `Fin`, `Validation`, `Option`     | Bifunctor map       |
|   [6]   | `.ToFin()` / `.ToValidation()` / `.ToOption()` | cross-rail                        | Boundary projection |
|   [7]   | `.Match(Succ:, Fail:)`                         | `Fin`, `Option`, `Either`         | Terminal collapse   |

## [3][APPLICATIVE]

| [INDEX] | [FORM]                                  | [USE]                           |
| :-----: | --------------------------------------- | ------------------------------- |
|   [1]   | `ValidationExtensions.Apply` chains     | Multi-field validation          |
|   [2]   | `v1 & v2`                               | Validation product via operator |
|   [3]   | `Validation<E,T>` where `E : Monoid<E>` | Parallel error accumulation     |

`Validation<string,T>` is **not supported** — use `StringM` or a monoidal error type. Consumer validation error policies belong in the local posture file.

## [4][COLLECTION_TRAVERSAL]

| [INDEX] | [COMBINATOR]                   | [NOTES]                                    |
| :-----: | ------------------------------ | ------------------------------------------ |
|   [1]   | `.TraverseM(f) >> lower`       | Fin / Validation / Option                  |
|   [2]   | `.TraverseM(f).As()`           | Eff / IO                                   |
|   [3]   | `.Traverse(identity) >> lower` | v4 `Sequence` replacement                  |
|   [4]   | `.Choose(f)`                   | Filter-map to Option                       |
|   [5]   | `.Fold(init, f)`               | Immutable aggregation                      |
|   [6]   | `.Strict()`                    | Materialize lazy sequences before boundary |

`HashMap` algebra: `.AddOrUpdate`, `.Find`, `.Filter`.

## [5][EFFECT_AND_RECOVERY]

| [INDEX] | [COMBINATOR]                                           | [NOTES]                                    |
| :-----: | ------------------------------------------------------ | ------------------------------------------ |
|   [1]   | `Eff.runtime<RT>()`                                    | Materialize effect with runtime record     |
|   [2]   | `Reader.Asks` / `Ask<R,A>` / `Prelude.liftEff`         | Environment ask and pure lift              |
|   [3]   | `.Run` / `.RunAsync` / `.RunIO`                        | Collapse matrix — pick overload explicitly |
|   [4]   | `Try.lift<Fin<T>>(f).Run()`                            | Exception capsule at native boundary       |
|   [5]   | `Prelude.catch` / `@catch` / `catchOf` / `catchOfFold` | Effect recovery                            |
|   [6]   | `IO<T>.Retry(Schedule)` / `Prelude.retry` / `repeat`   | Schedule-driven retry                      |

## [6][STATE]

| [INDEX] | [COMBINATOR]                          | [NOTES]                      |
| :-----: | ------------------------------------- | ---------------------------- |
|   [1]   | `Atom<T>.Swap(f)`                     | Synchronous state transition |
|   [2]   | `Atom<T>.SwapMaybe(f)`                | Transition to `Option<T>`    |
|   [3]   | `Atom<T>.SwapIO` / `SwapMaybeIO`      | IO-backed swap               |
|   [4]   | `AtomHashMap` / `AtomSeq` / `AtomQue` | Collection-shaped atoms      |

## [7][PRELUDE_GUARDS]

| [INDEX] | [MEMBER]                     | [USE]                               |
| :-----: | ---------------------------- | ----------------------------------- |
|   [1]   | `guard(condition, error)`    | Fail chain when condition false     |
|   [2]   | `guardnot(condition, error)` | Negated guard                       |
|   [3]   | `Optional(x)`                | Lift nullable/reference to `Option` |
|   [4]   | `Some(x)` / `None`           | Option constructors                 |
|   [5]   | `identity`                   | `.Traverse(identity) >> lower`      |
|   [6]   | `toSeq` / `toHashMap`        | Collection normalization            |

## [8][TRAITS]

| [INDEX] | [SURFACE]                         | [NOTES]                      |
| :-----: | --------------------------------- | ---------------------------- |
|   [1]   | `Next.Loop` / `Next.Done`         | Trampolining                 |
|   [2]   | `K<F,A>` + traits                 | Higher-kinded algorithms     |
|   [3]   | `ComposeK`, `HyloM`, `FoldArrows` | Absent from local XML        |

Use file-local `from..in` unless measured duplication proves trait abstraction. See `traits.md`.

## [9][RULES]

- One fallible step → `Fin` + `Bind`/`Map`.
- Independent fields → `Validation` + `Apply` / `&`.
- Host context → `Eff<RT,T>` + concrete runtime record.
- Many fallible elements → `TraverseM` + carrier-appropriate lowering.
- Optional merge → `Choose` or validation `|`.
- Do not create helper wrappers around combinators listed here.
