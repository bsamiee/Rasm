# [H1][LANGUAGEEXT_TRAITS_HKT]
>**Dictum:** *Traits let one algorithm target many computation shapes.*

<br>

[IMPORTANT] Use higher-kinded abstraction only when it removes real duplication across `Fin`, `Eff`, `Option`, `Validation`, or collection effects. A single caller does not justify a generic trait surface.

---
## [1][CORE_TYPES]
>**Dictum:** *`K<F,A>` is the carrier for effect-polymorphic values.*

<br>

| [INDEX] | [TYPE] | [ROLE] |
| :-----: | ------ | ------ |
| **[1]** | `K<F,A>` | Higher-kinded value encoded for C#. |
| **[2]** | `K<F,A,B>` | Higher-kinded value with two carried types. |
| **[3]** | `.As()` | Downcast from `K<F,A>` to the concrete carrier at a consumption boundary. |
| **[4]** | `pure<F,A>` | Lift a value into the abstract carrier. |
| **[5]** | `error<F,A>` | Lift failure into a fallible carrier. |

[CRITICAL] `.As()` belongs at the boundary where a concrete carrier is required. Do not scatter `.As()` through the middle of a generic algorithm.

---
## [2][TRAIT_SELECTION]
>**Dictum:** *Constraints describe the exact operations an algorithm may use.*

<br>

| [INDEX] | [CONSTRAINT] | [ALLOWS] | [USE_WHEN] |
| :-----: | ------------ | -------- | ---------- |
| **[1]** | `Functor<F>` | `Map`. | Success projection only. |
| **[2]** | `Applicative<F>` | Pure values and independent application. | Independent effects can combine. |
| **[3]** | `Monad<F>` | Dependent `Bind` sequencing. | Later steps depend on earlier values. |
| **[4]** | `Fallible<F>` | Failure construction and recovery. | Algorithm needs typed failure without choosing `Fin` or `Eff`. |
| **[5]** | `SemigroupK<F>` | Alternative combination. | Fallback between computations. |
| **[6]** | `MonoidK<F>` | Empty computation. | Folded effect sequences need identity. |
| **[7]** | `Alternative<F>` | Applicative plus choice. | Combine independent effects with fallback semantics. |
| **[8]** | `Choice<F>` | Branch-like choice as algebra. | Encode selection without statement branching. |
| **[9]** | `Bifunctor<F>` | Map two carried channels. | Preserve context while transforming both sides. |
| **[10]** | `Local<M,E>` | Local environment transformation. | Prefer direct runtime records unless environment adaptation repeats. |
| **[11]** | `MonadIO<M>` and `MonadUnliftIO<M>` | Lift, bracket, retry, repeat, and timeout IO from abstract carriers. | Infrastructure only; avoid exposing to GH2 components. |
| **[12]** | `Natural<F,G>` variants | Natural transformations between carriers. | Use when carrier conversion is a reusable law, not a one-off cast. |

---
## [3][KLEISLI]
>**Dictum:** *Function composition over effects is the advanced compression point.*

<br>

Use `A -> K<F,B>` arrows when:
- The same pipeline should run as `Fin<T>` for pure validation and `Eff<RT,T>` for host execution.
- A sequence of stages should fold into one computation.
- Failure policy belongs at the composition boundary, not inside every stage.

Canonical shapes:
- `Func<A, K<F,B>>` for one effectful stage.
- `Func<A, K<F,A>>` for homogeneous stage folds.
- `Seq<Func<A, K<F,A>>>` plus a fold over Kleisli identity for dynamic stage lists.

---
## [4][TRANSFORMERS]
>**Dictum:** *Transformers preserve nested effects without unwrapping by hand.*

<br>

| [INDEX] | [TYPE] | [USE] |
| :-----: | ------ | ----- |
| **[1]** | `OptionT<M,A>` | Optional result inside another monad. |
| **[2]** | `EitherT<L,M,R>` | Typed left/right result inside another monad. |
| **[3]** | `FinT<M,A>` | `Error` rail inside another monad. |
| **[4]** | `ValidationT<F,M,A>` | Accumulating validation inside another monad. |
| **[5]** | `ReaderT<E,M,A>` | Environment access inside another monad. |
| **[6]** | `StateT<S,M,A>` | State threading inside another monad. |
| **[7]** | `WriterT<W,M,A>` | Monoidal output inside another monad. |
| **[8]** | `TryT<M,A>` | Exception-capturing rail inside another monad. |
| **[9]** | `IdentityT<M,A>` | Pure carrier transformer. |

[IMPORTANT] Prefer direct `Eff<RT,T>` or `Fin<T>` until transformer use removes visible nested `Map` or `Bind` plumbing.

---
## [5][DOMAIN_TRAITS]
>**Dictum:** *Value traits add algebra to domain atoms.*

<br>

| [INDEX] | [TRAIT] | [MEANING] |
| :-----: | ------- | --------- |
| **[1]** | `LanguageExt.Traits.Domain.DomainType<SELF,REPR>` | Validated conversion between domain type and representation. |
| **[2]** | `LanguageExt.Traits.Domain.Identifier<SELF>` | Identity-like value. |
| **[3]** | `LanguageExt.Traits.Domain.Amount<SELF,SCALAR>` | Quantity with vector-space behavior. |
| **[4]** | `LanguageExt.Traits.Domain.Locus<SELF,DISTANCE,SCALAR>` | Position that subtracts to a distance. |
| **[5]** | `LanguageExt.Traits.Domain.VectorSpace<SELF,SCALAR>` | Scalable additive value. |

Use these only when generic math and domain algebra materially improve the implementation. For simple boundary wrappers, Thinktecture `[ValueObject<T>]` remains the smaller shape.

---
## [6][RULES]
>**Dictum:** *Advanced abstraction must buy visible compression.*

<br>

- Choose the narrowest trait constraint that admits the needed operations.
- Use `static` lambdas and tuple state when folding arrows.
- Keep `K<F,A>` APIs close to algorithmic code; do not expose them to GH2 component specs.
- Convert `Option<T>` to `Fin<T>` when absence becomes a user-facing fault.
- Convert `Fin<T>` to `Validation<Error,T>` only at accumulation boundaries.
