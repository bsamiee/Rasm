# Recovery Policy Values And Catch-Frame Discipline

[POLICY_VALUE_RECOVERY]:
- A recovery is a reified predicate-handler pair, not control flow: the pair is ordinary data that can be named, returned from a policy factory, parameterized, and selected from a table, and it retargets its bound type by mapping the handler while reusing the predicate. The pair does not compose with its siblings directly — there is no pair-to-pair operator — so a recovery cascade is assembled at the protected effect, where each successive operator application wraps the composite to its left; the first matching predicate consumes the failure and later handlers are not consulted for it.

```csharp
static CatchM<Error, M, A> Transient<M, A>() where M : Fallible<Error, M>, Monad<M> =>
    @catch(TransientCode, e => Recompute<M, A>(e));

K<M, A> recovered =
    protectedEffect
  | Transient<M, A>()
  | expected(e => Fallback<M, A>(e))
  | @catch(_ => true, e => SurfaceFatal<M, A>(e));
```

- The `Fallible<Error, M>, Monad<M>` constraint pair on a policy factory is load-bearing as a unit: `expected` resolves against `Monad<M>`, and `pure` inside a handler body is admissible only because `Monad<M> : Applicative<M>` flows the lifting capability through. Relaxing the body to `Fallible<Error, M>` alone breaks both calls at once — the `Monad<M>` constraint is not decorative.
- The recovery matrix is two orthogonal axes chosen by combinator, never by branches inside a handler: which failures (all, by code, by identity, by leaf subtype, expected-only, exceptional-only) crossed with which multiplicity over a matching batch (the bind-fold variant recovers fail-fast across matching members; the monoid-fold variant visits every matching member and merges handler outcomes). A handler author never branches on "one error or many" — the predicate chooses membership, the combinator chooses multiplicity, and the single-failure and batch paths unify under the same predicate.

[FRAME_DISCIPLINE]:
- Catch is a stack discipline inside the interpreter: the protected operation pushes a handler frame and a pop node removes it on normal completion before the forward continuation runs. A handler structurally cannot catch a failure that arises after its protected region — handler scope is exact, not lexical-approximate.
- A recovered branch re-enters the saved forward continuation: recovery is not a terminal. The handler's success resumes the original pipeline at the point after the protected region, which is what makes mid-pipeline recovery composition-safe rather than a dead end that forces rail reconstruction.
- The handler frame lifts any escaped exception through the canonicalizing admission before testing the predicate: a cancellation thrown deep in foreign code arrives as the canonical cancelled value, an aggregate arrives pre-flattened, and a wrapped rail error arrives unwrapped to its original form. Predicates are therefore written against codes and facet partitions, never against exception types, and remain stable as the underlying throw site changes.
