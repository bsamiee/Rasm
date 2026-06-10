# Carrier Frontier

[FIN_APPLICATIVE_ERROR_ACCUMULATION]:
- `Fin<A>` has two structurally distinct combination behaviors. `Bind` is sequential and fail-fast: on `Fail`, the continuation is skipped and the error propagates unchanged — standard monadic short-circuit. `Apply` (the applicative surface) accumulates: when both `mf` and `ma` are `Fail`, the `Apply` implementation concatenates their errors via `value4 + value3` (i.e., `Error.Combine`), returning `Fail(mf_error + ma_error)`. A `Fin<Func<A,B>>.Fail` paired with a `Fin<A>.Fail` yields a `ManyErrors` aggregate, not just the left error. This makes `Fin` weakly applicative-accumulating without crossing to `Validation` for pairwise independent checks — the accumulation is bounded to exactly two sites per `Apply` call, not a fold over N values.
- Practical consequence: curried application chains `f * arg1 * arg2 * arg3` accumulate at each `*` step, meaning the final result may carry the pairwise combination of adjacent failures rather than the full cross-product of all failures. For full N-way accumulation, `Validation` is still required; `Fin.Apply` gives partial accumulation only.
- `SemigroupK<Fin>.Combine` (the `+` operator on `K<Fin,A>`) implements `Choice`-style fallback with error merging: when both sides are `Fail`, it combines their errors; when `lhs` is `Succ`, it returns `lhs` immediately. This is distinct from `Alternative.Choose` which short-circuits to `rhs` on `lhs` failure without combining the errors. `Combine` is the additive monoid; `Choose` is the right-biased selector.

```csharp
// Apply accumulates exactly two Fail errors — not a full fold
Fin<int> mf = Fin.Fail<Func<string, int>>(Error.New("fn-missing"));
Fin<string> ma = Fin.Fail<string>(Error.New("arg-missing"));
Fin<int> result = mf.Apply(ma);
// result: Fail(ManyErrors([Error "fn-missing", Error "arg-missing"]))

// SemigroupK.Combine: both Fail → merged error; left Succ → left wins
Fin<int> combined = Fin.Fail<int>(Error.New("a")) + Fin.Fail<int>(Error.New("b"));
// combined: Fail(ManyErrors([Error "a", Error "b"]))
```

[FIN_INTERNAL_VALUE_ACCESSOR_LAW]:
- `Fin<A>.SuccValue` and `Fin<A>.FailValue` are `internal abstract` properties on the base class; the `Succ` and `Fail` nested types override them as `internal override`. They are not accessible from consuming assemblies. The only public unsafe extraction is `ThrowIfFail()`, which calls `FailValue.Throw()` (entering exception flow) and returns `SuccValue` on success. `ThrowIfFail()` is the single legitimate "unwrap or throw" surface; it is restricted to host-boundary adapters, never used in domain logic. The `Fin.FailValue` name is visible in the library's own internals (e.g., `Eff.LiftIO` and the `Recur` tail-loop implementation access it via `fin.FailValue` inside `LanguageExt.Core` assembly scope), so it appears in stack traces and decompiles but is not callable from application code.
- Pattern matching is the only public read path: `{ IsSucc: true }` flat-match form uses `IsSucc: bool` (public abstract) and is safe; `case Fin<A>.Succ s` gives `s.Value` as the public positional record field. The nested `Succ` record exposes `Value` publicly; the nested `Fail` record exposes `Error` publicly — these are the safe named fields.

[VALIDATION_BIBIND_CONSTRAINT_ASYMMETRY]:
- `Validation<F,A>.BiBind<F1,A1>(Func<F, Validation<F1,A1>>, Func<A, Validation<F1,A1>>)` carries no compile-time constraint on `F1`. The target failure type may be any type, including non-monoid types. This is the structural escape hatch: `BiBind` bypasses the monoid requirement entirely because it replaces both rails with a new `Validation<F1,A1>` supplied by the caller — the library does not need to combine `F1` values internally. Contrast `BindFail<F1>` which does carry `where F1 : Monoid<F1>` because it delegates to `BiBind` with `Validation.SuccessI<F1,A>` as the success arm, and the generated `SuccessI` helper requires a monoid-admissible type to remain coherent for subsequent `Apply` accumulation. The pattern: `BiBind` for structural type-change where accumulation semantics are not preserved; `BindFail` when the result must remain a law-abiding `Validation`.

[CATCHOF_SEMANTICS_FOR_MANYERRORS]:
- `catchOf<E,M,A>` (requires `M : Monad<M>`) and `catchOfFold<E,M,A>` (requires `M : MonoidK<M>`) handle `ManyErrors` differently. `catchOf` calls `es.Filter<E>().ForAllM(Fail)`: `ForAllM` on `ManyErrors` is sequential-short-circuit — it folds left-to-right binding each atomic error into the next via `Bind`, stopping on first failure. `catchOfFold` calls `es.Filter<E>().FoldM(Fail)`: `FoldM` on `ManyErrors` starts with `M.Empty<A>()` and combines each result via `M.Combine` — when `M = Fin`, this accumulates errors; when `M = Eff`, this accumulates the last error. The `ForAllM`/`FoldM` distinction encodes the fail-fast vs. accumulate policy for multi-error `ManyErrors` payloads inside a single catch handler.
- For a single-error `Exceptional` or `Expected`, both forms call `Fail(e)` directly — the difference is invisible unless the caught error is a `ManyErrors` aggregate. Choosing `catchOfFold` over `catchOf` only matters when the error structure may carry `ManyErrors` of the target subtype `E`.

[OPTION_BIMAP_NULL_GUARD_ASYMMETRY]:
- `Option<A>.BiMap<B>(Func<A,B> Some, Func<Unit,B> None)` wraps its result in `Check.NullReturn`, which throws `ResultIsNullException` when the mapping function returns `null`. The second overload `BiMap<B>(Func<A,B> Some, Func<B> None)` does not call `Check.NullReturn` — it returns the result of `None()` directly without a null guard. This asymmetry means: the `Func<Unit,B>` overload is the defensive form; the `Func<B>` overload silently admits `null` from the `None` branch. When the `None` producer may return `null` (e.g., a nullable reference type value), the `Func<Unit,B>` overload's null guard fires at the mapping site rather than propagating a null `Option<B>` with `IsSome = true`.
- `Option<A>.BiBind<B>(Func<A, Option<B>> Some, Func<Option<B>> None)` has neither guard — both arms return `Option<B>` directly. The null-guard asymmetry is exclusive to `BiMap` with the `Func<Unit,B>` form.

[FIN_NATURAL_TRANSFORMATION_ASYMMETRY]:
- `Natural<Fin, Option>` discards the error: `Succ → Some(value)`, `Fail → None`. `Natural<Option, Fin>` uses `ToFin()` (the zero-arg overload), which calls `ToFin(Errors.None)` — the `None` case maps to `Fail(Errors.None)`, a structurally valid but semantically empty failure. Two consequences: (1) `Fin` to `Option` and back is not round-trip-safe — the `Fail` case survives both directions but with the original error replaced by `Errors.None`; (2) callers using `Natural.transform<Option, Fin, A>` (which is what `Eff.Lift` uses internally in some paths) need to be aware the `None` case produces a `Fail` with `IsEmpty = true`, not a carrier with a meaningful error. Testing `error.IsEmpty` distinguishes the absence-converted-to-failure from a real failure.
- `Natural<Fin, IO>` maps `Succ → IO.Pure`, `Fail → IO.fail<A>(error)` — the error is preserved for `IO` promotion, unlike the `Option` direction.

[VALIDATION_PARTITION_SURFACE]:
- For `Validation`, the batch partition surfaces are `Fails<F,S>(IEnumerable<Validation<F,S>>)` → `IEnumerable<F>` and `Fails<F,S>(Seq<Validation<F,S>>)` → `Seq<F>`, both in `ValidationExtensions`. These extract the failure values from a sequence of validations as a flat projection. There is no symmetric `Succs` on `Validation` through this surface — the success side is not partitioned in parallel. Contrast `Fin`: `K<F, Fin<A>>.Partition()` returns `(Seq<Error> Fails, Seq<A> Succs)` as a single tuple, `Fails()` returns just the error sequence, and `Succs()` returns just the success sequence — all three are defined on any `Foldable<F>` container of `Fin<A>`, not on `Fin<A>` itself. The Fin partition surface requires a wrapping foldable (e.g., `Seq<Fin<A>>` as `K<Seq, Fin<A>>`); there is no `Fin<A>.Partition()` instance method.

```csharp
// Fin partition — needs a foldable wrapper; returns both channels
Seq<Fin<int>> results = Seq(Fin.Succ(1), Fin.Fail<int>(Error.New("x")), Fin.Succ(2));
(Seq<Error> fails, Seq<int> succs) = results.Kind().Partition();
// fails: [Error "x"], succs: [1, 2]

// Validation partition — failure-side only, success-side omitted
Seq<Validation<Error, int>> vs = Seq(
    Validation.Success<Error, int>(1),
    Validation.Fail<Error, int>(Error.New("y")));
Seq<Error> errs = vs.Fails();  // [Error "y"]
```

[OPTION_NATURAL_CARRIERS]:
- `Option` implements `Natural<Option, Fin>`, `Natural<Option, Arr>`, `Natural<Option, Lst>`, `Natural<Option, Seq>`, `Natural<Option, Iterable>`, `Natural<Option, Eff>`, and `Natural<Option, OptionT<IO>>`. The `Eff` direction lifts `Some` to `Eff.Pure` and `None` to `Eff.Fail(Errors.None)` — the same `Errors.None` produced by `ToFin()`. Any carrier-polymorphic pipeline using `Natural.transform<Option, M, A>` will generate `Errors.None`-tagged failures from `None`, making subsequent `HasCode` or `Is` predicates behave incorrectly unless the caller explicitly checks `error.IsEmpty`. The correct ingress pattern when `None` must carry diagnostic identity is always `option.ToFin(Error.New(...))` or `option.ToFin(Errors.SomeMeaningfulCode)` rather than relying on the natural transformation.

[VALIDATION_BIFUNCTOR_VIA_BIBIND]:
- The `Validation` static class implements `Bifunctor<Validation>`. `BiMap<L,A,M,B>` (the first-argument map) is implemented by delegating to `BiBind`: the `Fail` arm calls `f(x).As2()` and the `Succ` arm calls `SuccessI<Y,A>` (the identity success cast). There is no independent `MapFail` on `Validation<F,A>` itself — the instance-level `MapFail<F1>` (with `where F1 : Monoid<F1>`) produces a new `Validation<F1,A>` by mapping the failure type. This means `MapFail` and the `Bifunctor.BiMap` first-map are not the same operation: `MapFail` enforces `F1 : Monoid<F1>` and only touches the failure side; `Bifunctor.BiMap` uses `BiBind` internally and carries no constraint, but operates through the higher-kinded `K<Validation, L, A>` surface rather than directly on `Validation<F,A>`.
