# [LANGUAGEEXT_TRAITS]

[IMPORTANT] Use traits only when they remove real duplication. Runtime-record `Eff<RT,T>` remains the default host effect shape.

## [1][CORE]

| [INDEX] | [SURFACE]                 | [USE]                                           |
| :-----: | ------------------------- | ----------------------------------------------- |
|   [1]   | `K<F,A>`                  | Effect-polymorphic value.                       |
|   [2]   | `.As()`                   | Projection back to concrete carrier.            |
|   [3]   | `pure<F,A>`, `error<F,A>` | Generic success/failure construction.           |
|   [4]   | `Natural`                 | Carrier transformation between effect families. |

## [2][CONSTRAINTS]

| [INDEX] | [TRAIT]                             | [MEANING]                                             |
| :-----: | ----------------------------------- | ----------------------------------------------------- |
|   [1]   | `Functor`, `Applicative`, `Monad`   | Mapping, independent application, sequential binding. |
|   [2]   | `Fallible`, `Choice`, `Alternative` | Failure and fallback.                                 |
|   [3]   | `Foldable`, `Traversable`           | Structure-aware reduction and traversal.              |
|   [4]   | `MonadIO`, `MonadUnliftIO`          | IO lifting and controlled host execution.             |
|   [5]   | `SemigroupK`, `MonoidK`             | Carrier combination.                                  |

## [3][TRANSFORMERS]

Use `OptionT`, `EitherT`, `FinT`, `ValidationT`, `ReaderT`, `StateT`, `WriterT`, `TryT`, `IdentityT`, `ChronicleT`, `RWST` only when nested effects are persistent structure. Prefer direct `Fin`, `Validation`, and `Eff` for normal code.

## [4][DOMAIN_TRAITS]

`DomainType`, `Identifier`, `Amount`, `Locus`, and `VectorSpace` are advanced value algebra surfaces. Use them for reusable scalar/vector-space algorithms. Prefer Thinktecture value objects for simple boundary admission.

## [5][RULES]

- Introduce trait-polymorphism only after repeated carrier-specific algorithms exist.
- Keep host/runtime ownership outside trait constraints; native semantics stay at boundary projections.
- Do not use trait vocabulary to hide runtime dependencies or host object lifetime.
