# Newtype-Deriving Effect Runtime

[ISOMORPHISM_AS_DERIVATION_KEY]:
- A wrapper carrier earns its entire trait stack from one bidirectional structure isomorphism, not from re-implemented operations. `Deriving<Super, Sub>` is a pure alias for `NaturalIso<Super, Sub>`, which is exactly `Natural<Super, Sub>` plus `CoNatural<Super, Sub>`: the down-arrow `static abstract K<Sub, A> Transform<A>(K<Super, A>)` and the up-arrow `static abstract K<Super, A> CoTransform<A>(K<Sub, A>)`.
- Every derived operation is the same sandwich: lower the wrapper to its representation through `Transform`, run the representation's already-correct instance method, lift the result back through `CoTransform`. The wrapper declares structure once; behavior is computed, never copied.
- The two arrows are variance-annotated mirror images — `Natural<out F, in G>` and `CoNatural<in F, out G>` — so the same `Super`/`Sub` pair satisfies both directions without a second type parameter. The isomorphism is total at the structure level; bound values are untouched, which is what distinguishes a natural transformation from a `Map`.
- Because `Transform`/`CoTransform` are `static abstract`, the lowering and lifting are resolved at the constraint, devirtualized per concrete carrier, and allocate nothing — the wrapper is genuinely zero-overhead over its representation for the derived surface.

[STACKED_TRAIT_INHERITANCE]:
- The deriving interfaces form a subsumption lattice that mirrors the trait lattice exactly. `Monad<Super, Sub>` already carries `Applicative<Super, Sub>`, `Functor<Super, Sub>`, and the base `Monad<Super>`; `MonadIO<Super, Sub>` adds the IO-lift surface on top of `Monad<Super, Sub>`; `MonadUnliftIO<Super, Sub>` sits at the apex and transitively re-exports every lower derivation. Declaring the apex supertype hands the wrapper the full chain in one shot.
- The lattice is self-constraining: each deriving interface requires `Sub` to implement the corresponding base trait (`where Sub : MonadUnliftIO<Sub>, MonadIO<Sub>`) and requires `Super` to be the deriving interface plus the base trait. The compiler refuses a derivation whose representation cannot supply the operation being derived, so an under-powered representation is a compile error, not a runtime gap.
- A capability hole is expressible by stopping lower on the lattice: a wrapper that should be a monad but must never touch IO derives `Monad<Super, Sub>` and omits `MonadIO<Super, Sub>`, so `LiftIO` is simply not in scope on that carrier. Capability is a position in the inheritance chain, not a runtime feature flag.
- `MonadUnliftIO<Super, Sub>` carries optional-IO marker constraints (`Maybe.MonadUnliftIO<Super>`, `Maybe.MonadIO<Super>`) so a stack whose inner monad lacks an IO floor still type-checks; the derived IO operations then degrade to the absent-IO branch rather than failing to resolve.

[FULL_RUNTIME_SURFACE_FROM_ONE_DECLARATION]:
- The apex derivation materializes the complete effect-runtime vocabulary as default-interface members on the wrapper: `ToIO`, `MapIO`, `LocalIO`, `PostIO`, `TimeoutIO`, both `BracketIO` arities, `RepeatIO`/`RepeatWhileIO`/`RepeatUntilIO`, `RetryIO`/`RetryWhileIO`/`RetryUntilIO` — each with bare and `Schedule`-parameterized overloads. None of these is written for the wrapper; all are computed from the representation's `MonadUnliftIO` via the isomorphism.
- This is the source of carrier-polymorphism for the whole timeout/bracket/retry/repeat family: those free functions are stated once against `MonadUnliftIO<M>` over `MapIO`, and any wrapper that derives the trait picks them up for free. A new effect carrier inherits resource safety, cancellation scoping, schedule-driven retry, and synchronization-context marshaling without a single bespoke combinator.
- `MonadIO<Super, Sub>` supplies both `LiftIO(IO<A>)` and `LiftIO(K<IO, A>)`, the second delegating to the first — so the wrapper admits already-constructed IO effects with no per-carrier lift code. The bridge from the bottom carrier into any derived wrapper is itself derived.

[READER_DERIVATION_AND_ENVIRONMENT_PROJECTION]:
- A capability-reading carrier derives `Readable<Super, Env, Sub>`, which yields `Asks`, `Ask`, and `Local` over the wrapper from a representation that reads the same `Env`. The environment type travels as a third type parameter through the isomorphism, so the derived reader projects the wrapper's environment with zero reader-monad re-plumbing.
- The capability-free reader degenerate makes the bound type itself the environment: when the representation is `ReaderT<A, IO>` and the supertype reads `A`, `ask<A>()` returns the bound value, which is why a no-capability effect carrier still satisfies the reader interface without a capability record. The reader is derived; the environment is whatever the representation reads.
- `Stateful<Super, Sub, S>` is the symmetric derivation for state-threaded carriers — `get`, `put`, `modify`, `gets`, and scoped `local` over the wrapper from a representation that threads `S`. State and reader derivations share the identical `NaturalIso` machinery, differing only in the extra `Env`/`S` parameter.

[CAPABILITY_RESOLUTION_AS_DERIVED_LOOKUP]:
- Multiple capability slots in one runtime record create accessor ambiguity; the fix is not a dispatch table but a derived strongly-typed lookup. `Has<M, Env, VALUE>` (where `Env : Has<M, VALUE>`) exposes `public static readonly K<M, VALUE> ask = Env.Ask`, forcing qualification through the type parameters and caching the static-virtual `Ask` resolution into one field initialization.
- Reading a capability through the cached `Has<M, Env, VALUE>.ask` collapses per-bind static-virtual dispatch to a single readonly-field load. Adding a capability is adding a `Has<M, VALUE>` implementation to the runtime record; the accessor for it is derived from the type, never hand-written, and never collides with a sibling capability because the three-parameter lookup disambiguates by `VALUE`.
- `Local<M, InnerEnv> : Has<M, InnerEnv>` adds exactly one operation — `static abstract K<M, A> With<A>(Func<InnerEnv, InnerEnv>, K<M, A>)` — turning a readable capability into a scopable one. A runtime slot becomes locally-rebindable by implementing one method beside its `Has` membership; the scoped-projection embedding of a dependency-isolated sub-computation is this `With` plus the reader derivation, nothing more.

[CONSTRUCTING_A_DERIVED_CARRIER]:
- The full recipe for a new effect carrier over an existing representation is one record, two arrow methods, and one supertype list — the trait bodies are inherited:

```csharp
public readonly record struct App<A>(ReaderT<Caps, IO, A> Run) :
    K<App, A>,
    Deriving.MonadUnliftIO<App<A>, ReaderT<Caps, IO, A>>,
    Deriving.Readable<App<A>, Caps, ReaderT<Caps, IO, A>>
{
    public static K<ReaderT<Caps, IO, A>, B> Transform<B>(K<App<A>, B> fa) =>
        ((App<B>)fa).Run;                                   // lower to representation
    public static K<App<A>, B> CoTransform<B>(K<ReaderT<Caps, IO, A>, B> fa) =>
        new App<B>(fa.As());                                // lift back to wrapper
}

// every IO-runtime combinator now resolves on App<A> with no further code:
static K<App<A>, B> WithPolicy<B>(K<App<A>, B> ma, Schedule s) =>
    ma.RetryIO(s).TimeoutIO(30 * seconds).BracketIO();
```

- The wrapper supplies only the isomorphism; `RetryIO`, `TimeoutIO`, `BracketIO`, `LiftIO`, and the reader's `Asks`/`Local` arrive through the deriving supertypes. A second carrier with different capabilities is the same three lines against its own representation — the runtime surface never proliferates per carrier.
- Each derived operation costs one `Transform` down-cast and one `CoTransform` up-cast around the representation's own implementation; for a record-struct wrapper over a `ReaderT` both casts are field reads, so the derived carrier is allocation-neutral against running the representation directly.

[FAILURE_AND_EDGE_SURFACES]:
- The derivation is structure-only: `Transform`/`CoTransform` must not inspect, reorder, or drop bound values, or the derived `Monad`/`Applicative` laws break silently — the compiler enforces the type, never the naturality law. A `CoTransform` that re-wraps with altered state is a law violation the type system cannot catch.
- `Fallible<E, Super, Sub>` derives the error rail with a custom error type `E`; the two-parameter `Fallible<Super, Sub>` fixes `E` to the canonical error and is the common form. A carrier that derives `Monad` but omits the `Fallible` derivation has no `Catch`/`@catch` surface — recovery combinators are absent exactly where the fallible derivation is absent, so a non-failing carrier cannot accidentally swallow errors.
- `MonadT<Super, Sub, M>` derives a transformer wrapper that re-exports the inner monad `M`'s `lift`; the inner `M` is an `out` parameter, so the derived transformer is covariant in its base monad and a single transformer declaration serves every inner monad satisfying `Monad<M>`. The transformer's lift and the monad's bind both fall out of the one isomorphism plus the inner-monad constraint.
