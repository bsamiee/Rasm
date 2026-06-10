# Carrier Deepening

[IMPLICIT_CONVERSION_SURFACE]:
- `Option<A>` carries `op_Implicit(A)` → `Option<A>`, `op_Implicit(Fail<Unit>)` → `None`, and `op_Implicit(Unit)` → `None`. The `A`-to-`Option<A>` implicit calls `Optional()`, which null-folds to `None` — `Option` is the null-safe boundary carrier. `Either<L,R>` carries implicits from both `L` and `R`. `Validation<F,A>` carries implicits from `A` and from `F`. `Fin<A>` carries no implicit conversions from `A` or `Error` — construction is always explicit through `Fin.Succ`, `Fin.Fail`, `Prelude.FinSucc`, or `Prelude.FinFail`.
- `Eff<A>` carries a direct implicit from `Fin<A>`: a `Fin<A>` value returned in any `Eff<A>` position is silently promoted. The inverse does not exist. Explicit `.Run()` or effect materialization is always required to recover `Fin<A>` from `Eff<A>`.
- The absence of a null guard on `Fin.Succ` construction means a `null` reference type value produces `Succ(null)`, not `Fail`. `Option`'s implicit from `A` is the correct carrier for nullable ingress precisely because `Optional()` null-folds; the two carriers are not interchangeable at nullable boundaries.

[EXPLICIT_CAST_TRAP]:
- `Validation<F,A>` exposes `explicit operator A` and `explicit operator F`. Both delegate to the internal `SuccessValue`/`FailValue` properties; a cast on the wrong case throws `InvalidCastException`. `Fin<A>.SuccValue` and `Fin<A>.FailValue` are public but marked unsafe — they do not guard and throw on the wrong case. The safe read path for both carriers is pattern matching: `case Fin<A>.Succ s` (where `s.Value` is public) or `{ IsSucc: true }` for `Fin`, and `{ IsSuccess: true, SuccessValue: var v }` for `Validation`.

[APPLY_SEMIGROUP_DEGRADATION]:
- `ValidationExtensions.Apply` operates over `K<Validation<F>,_>` and requires `F` to carry a semigroup instance for accumulation to work. When `F` implements `Semigroup<F>`, `F.Instance` (a static virtual property) resolves at JIT time and accumulation works. The `ApplyI` overload accepts an explicit `Option<SemigroupInstance<F>>` — passing `None` degrades silently to fail-first behavior without any exception. This means the accumulation guarantee is not enforced at the call site for `ApplyI`; the guarantee requires either a compile-time `F : Semigroup<F>` constraint (available in generic context) or an explicit `Some(F.Instance)` argument.
- `SelectMany` on `Validation<F,A>` carries no semigroup constraint and always operates sequentially (fail-first). Error accumulation requires `Apply` (or `Applicative.lift` / `CombineI`), never `SelectMany`. A LINQ query expression on `Validation` is always sequential regardless of `F`.

[VALIDATION_MAPFAIL_TARGET_CONSTRAINT]:
- `Validation<F,A>.MapFail<F1>(Func<F,F1> f)` produces `Validation<F1,A>`. The type signature imposes no compile-time monoid constraint on `F1`; however, subsequent `Apply` accumulation on the mapped result requires `F1 : Semigroup<F1>` at the `Apply` call site. `BiBind<F1,A1>` has no such constraint on `F1` at all because it replaces both rails with a new `Validation<F1,A1>` supplied by the caller — the library does not combine `F1` values internally. The pattern: use `BiBind` for structural type-change where accumulation semantics are not preserved; use `BindFail` when the result must remain law-abiding and `MapFail` when only the error value needs remapping within the same `F`.

[ERROR_HIERARCHY_MECHANICS]:
- `Error.Combine` (op_Addition) appends two errors. Combining two scalars produces `ManyErrors(Seq(this, other))`; combining two `ManyErrors` flattens their sequences; combining a scalar with a `ManyErrors` prepends or appends the scalar. `Errors.None` is the monoid identity — it is an error state without any error values, and `Combine(Errors.None, x) = x`. `Errors.None` carries `IsEmpty = true`, which distinguishes an absence-converted-to-failure from a real failure when `Option.ToFin()` (the zero-arg form) is used.
- `Error.Is(Error other)` determines identity by code (when `Code != 0`) or by message string equality (when `Code == 0`). It does not use reference equality or type identity. For `ManyErrors`, `Is` walks each member asking `member.Is(other)`. `HasCode(int)` also walks the full nested structure. `IsType<E>()` tests `this is E` — structural type, not semantic code — and `ManyErrors` overrides it to test each member. The `Filter<E>()` operator on `ManyErrors` runs `Choose` over members, keeping only those that `IsType<E>()`, and returns `Errors.None` when the set is empty.
- The live `Exception` in `Exceptional` is stored as `private readonly` and is not serialization-safe. `Error.Exception` returns the live value only when `IsExceptional` is true; for a deserialized `Exceptional`, `.ToException()` rebuilds from `Message`/`Code` only, discarding the stack trace. `HasException<E>()` returns `true` only when the live field holds an instance of `E` — it returns `false` for deserialized `Exceptional` instances regardless of type.

[CATCHM_ANATOMY]:
- `CatchM<E,M,A>` is a `readonly record struct` with two fields: `Func<E,bool> Match` and `Func<E,K<M,A>> Action`. Its `Run(E error, K<M,A> otherwise)` method: if `Match` returns false, returns `otherwise` unchanged (propagating the error); if true, calls `Action`. Chaining multiple named catches via `|` is ordered: each catch's predicate is evaluated left-to-right until one matches, and the original error propagates through all mismatches. There is no `CatchM` composition operator — only `Map<B>` to transform the recovery value. Multiple catches must be chained as separate `|` applications.
- The `|` operator on `Fin<A>` has two distinct overloads: `Fin<A> | Fin<A>` (alternative — returns left if `Succ`, right otherwise) and `K<Fin, A> | CatchM<Error, Fin, A>` (named recovery). Both `Validation<F,A>` and `Option<A>` carry the same `| CatchM` overload with their respective failure types (`F` and `Unit`). The named catch combinators (`@catch`, `expected`, `exceptional`, `catchOf<E>`, `expectedOf<E>`) each produce a `CatchM<Error,M,A>` composable with `|` against any `Fallible<Error,M>` carrier including `Fin`, `Eff`, `IO`, `Try`, and `Either<Error,_>`.

[FIN_NATURAL_TRANSFORMATION]:
- `Fin` participates in natural transformations: `Natural<Fin, Eff>`, `Natural<Fin, IO>`, and `Natural<Fin, Option>` are supported. The `Natural<Fin, Option>` direction discards the error: `Succ → Some(value)`, `Fail → None`. The `Natural<Option, Fin>` direction uses `ToFin()` (the zero-arg overload), which maps `None` to `Fail(Errors.None)` — a structurally valid but semantically empty failure. Two consequences: (1) `Fin` to `Option` and back is not round-trip-safe — the original error is replaced by `Errors.None`; (2) callers using `Natural.transform<Option, Fin, A>` need to be aware the `None` case produces a `Fail` with `IsEmpty = true`, not a carrier with meaningful failure evidence. Testing `error.IsEmpty` distinguishes absence-converted-to-failure from a real failure. `Natural<Fin, IO>` preserves the error: `Succ → IO.Pure`, `Fail → IO.fail<A>(error)`.

[GUARD_FORMS_CONTRASTED]:
- `guard(bool, Error)` produces `Guard<Error, Unit>` which carries the error payload and integrates into any `Fallible<Error,M>` via `SelectMany` extension overloads. `guard<F>(bool)` with `where F : MonoidK<F>, Applicative<F>` produces `K<F, Unit>` by calling `F.Empty<Unit>()` on false and `F.Pure(unit)` on true — no error payload, no `Guard` wrapper, just the applicative's empty or pure. The two forms are not interchangeable: the payload form is the correct choice inside `Eff`/`IO`/`Fin` pipelines; the applicative form is correct for `Option`/`Seq`/`Lst` where absence rather than failure is the semantic.
- `guardnot(bool, Error)` is a strict alias that negates the condition before calling `guard`. `iff<F,A>(bool, K<F,A>, K<F,A>)`, `when<F>(bool, K<F,Unit>)`, and `unless<F>(bool, K<F,Unit>)` are the complementary carrier-polymorphic branching forms: `iff` is a non-short-circuiting value selection; `when` runs the alternative on true; `unless` runs it on false. All three require `F : Applicative<F>` only — no `Fallible` constraint. `iff` also carries overloads accepting `bool` directly (rather than `K<F,bool>`) for eager-predicate sites.

[OPTION_AS_ALTERNATIVE]:
- `Option<A>` implements `Fallible<Unit, Option>` — its failure side is `Unit`. `@catch` against `Option<A>` requires `CatchM<Unit, Option, A>`; the named catch forms that dispatch on `Error` subtypes cannot be applied to `Option` without first projecting to `Fin` or `Eff`. The `|` on `K<Option,A>` also accepts `K<Option,A> | Pure<A>` (promotes the pure to `Some`) and `K<Option,A> | Unit` (no-op on the `None` path, returning `None`). These forms allow option-chain alternatives in compact expressions. The `guard<F>(bool)` applicative form with `F = Option` is the correct inline absence gate: `None` on false, `Some(())` on true, without constructing a `Guard<Error,Unit>` that would require a later `ToOption`.

[VALIDATION_COMBINEONLY_ERROR_ACCUMULATION]:
- Error accumulation in `Validation<Error,A>` is strictly applicative — `CombineI(lhs, rhs, F.Instance)` and `Apply` are the two public surfaces. `CombineI(Validation<F,A>, Validation<F,A>, Option<SemigroupInstance<F>>)` returns `Validation<F, Seq<A>>`: when both succeed, their values are collected into a `Seq`; when any fail, their errors are combined via `F.Instance.Combine`. `CombineFirst` shares the same signature but returns the first success value rather than a sequence — suitable for pairwise combining where only the first success is the goal. The public `CombineI(Validation<F, Seq<A>>, Validation<F, A>)` overload (without the `Option<SemigroupInstance<F>>` parameter) takes the already-accumulated sequence as `lhs` and appends to it, which is the fold-friendly form for accumulating over an `IEnumerable<Validation<F,A>>` via `Seq.fold`.

```csharp
// Applicative accumulation: all three checks run; all failures collected
var result =
    Validation.Succ<Error, int>(42)
        .Apply(Validation.Succ<Error, Func<int, int>>(x => x + 1))
        .CombineI(Validation.Fail<Error, int>(Error.New("b")), F.Instance);
// result: Fail(ManyErrors([Error "b"]))

// BiBind with type-changing failure side — target F1 need not satisfy Semigroup
Validation<string, int> coerced =
    someValidation.BiBind(
        f => Validation<string, int>.Fail(f.Message),
        v => Validation<string, int>.Success(v));
```
