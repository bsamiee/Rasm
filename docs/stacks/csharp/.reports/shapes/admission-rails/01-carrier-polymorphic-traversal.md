# Carrier-Polymorphic Admission Traversal

[HIGHER_KINDED_ADMISSION_ARROW]:
- An admission function typed `Func<TRaw, K<F, TOwner>>` — not `Func<TRaw, Validation<Error, TOwner>>` — is carrier-agnostic: the same arrow drives accumulation, short-circuit, effectful, and transformer-stacked admission depending solely on which `F` the caller supplies. The bridge's natural return shape is the kinded one; the concrete-typed bridge is the special case obtained by fixing `F`.
- The applicative contract is `static abstract K<F, B> Apply<A, B>(K<F, Func<A, B>>, K<F, A>)` plus a second overload taking `Memo<F, A>` as the right operand. Every N-field admission collapses to repeated `Apply` over a curried constructor; arity is absorbed by `Applicative.lift` overloads up to the uncurried-function and curried-function forms at three-plus parameters, so one `lift` call admits all fields and applies the constructor in a single kinded expression with no intermediate carriers named.
- The lazy second operand is the entire deferral mechanism: `Memo<F, A>` wraps an unevaluated admission, and `Apply(mf, Memo<F,A> ma)` forces `ma.Value` only after `mf` is known. First-success grammars compose through `Choose(K<F,A>, Memo<F,A>)`, which returns the left untouched when it is already a success and forces the memoized right only otherwise — the lazy fallback grammar is a trait-level `Memo` overload, not a control-flow trick.
- Carrier migration is a named arrow, not a re-admission: the sequential rail declares `Natural<TFrom, TTo>` instances exposing `static abstract K<G, A> Transform<A>(K<F, A>)`, so an admitted value moves between the sequential carrier and the lazy-effect, optional, and either carriers by transformation, never by unwrap-and-rewrap. Carrier choice at one seam never forecloses carrier choice at the next.

[TRAVERSE_IS_THE_BATCH_BRIDGE]:
- Batch admission of a homogeneous collection is one traversal: `raws.Traverse(Owner.Admit)` lifts `Seq<TRaw>` and `Func<TRaw, K<F,TOwner>>` to `K<F, Seq<TOwner>>`. The collection's traversable instance folds the elements through `Applicative.lift((acc, x) => acc.Add(x), state, f(value))`, so when `F` is the accumulating carrier every element fault accumulates, and when `F` is the sequential carrier the first fault wins — the all-faults-versus-first-fault decision is the choice of `F`, derived from one fold, not two code paths.
- `Traverse` and `TraverseM` share the same fold skeleton and differ only in the combiner: `Traverse` threads state through `Applicative.lift` (independence — every element evaluated), `TraverseM` threads through `Bind` (dependence — short-circuit on the first failure of a monadic `F`). The applicative form is the right tool for "admit every row, report every bad row"; the monadic form is "admit until the first rejection, then stop." Selecting between them is a statement about whether later admissions depend on earlier ones.
- `Sequence` is `Traverse` of identity: `Seq<K<F,TOwner>>` collapses to `K<F, Seq<TOwner>>`, turning a sequence of already-launched admissions into one carrier holding the sequence. A pre-built batch of per-element admission results sequences into a single rail without naming the element carrier; the transpose `K<T, K<F, A>> -> K<F, K<T, A>>` is the whole operation.
- Bare `Applicative<F>` cannot inject a fault — it exposes only `Pure`, never a failure constructor — so a carrier-polymorphic admission arrow that must reject needs the stronger `Fallible<TFail, F>` capability, whose `Fail<A>(TFail)` is the kinded fault lift. This is the precise constraint that lets one arrow both succeed in any applicative and fail in any fallible carrier: success uses `F.Pure`, rejection uses `F.Fail`, and the batch bridge constrains on the conjunction.

```csharp
public static K<F, Seq<TOwner>> AdmitAll<F, TOwner, TRaw>(Seq<TRaw> raws)
    where F : Applicative<F>, Fallible<Fault, F>
    where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
    where TRaw : notnull, allows ref struct =>
    raws.Traverse(raw => TOwner.Validate(raw, null, out var owned) is { } fault
        ? F.Fail<TOwner>(fault)   // kinded fault lift — accumulates under Validation<F>, halts under Fin
        : F.Pure(owned!))
        .As();
```

[SEMIGROUP_RESOLUTION_PHYSICS]:
- The accumulating carrier's `Apply` resolves the failure semigroup at runtime by reflecting for `Semigroup<TFail>.Instance` through `MakeGenericType(typeof(TFail)).GetProperty("Instance")`, wrapped as `Option<SemigroupInstance<TFail>>`. The lookup is exact in the failure type — an inherited `Semigroup<ErrorBase>` is invisible when the static failure type is a derived fault, because the reflected generic is closed over the exact `TFail`.
- The resolution miss is silent and load-bearing: when the lookup is `None`, the apply body falls to a branch that keeps the function-carrier's fault and discards the right operand's fault entirely — no exception, no diagnostic, just first-fault reporting. A typed fault family intended for accumulating composition either carries `Error` as its currency (the many-errors monoid is always present) or declares its own `Semigroup<TFault>.Instance` with an aggregate case; absent both, every N-field bridge degrades to one-fault reporting that compiles and runs.
- Only the identity-demanding operations fail loud: the public construction surface and the alternative-empty and monoid-unit contexts require the full monoid at compile time and throw an unsupported-operation naming the missing monoid. The asymmetry is deliberate — accumulation degrades quietly, identity construction refuses. A bridge that never names empty or unit never trips the loud path and never learns its semigroup was missing.

[TRANSFORMER_THREADS_THE_MONOID_EXPLICITLY]:
- The validation transformer carries `Func<MonoidInstance<TFail>, K<M, Validation<TFail, A>>>` — the run function takes the monoid instance as a parameter rather than reflecting for it. Stacking admission over an inner monad both accumulates faults and sequences inner effects in one applicative: the transformer's `Apply` runs the function and argument carriers through the inner monad's bind, then combines their faults through the passed-in monoid.

```csharp
// transformer Apply: M sequences, monoid accumulates — both in one expression
static K<ValidationT<F, M>, B> Apply<A, B>(K<ValidationT<F, M>, Func<A, B>> mf, K<ValidationT<F, M>, A> ma) =>
    new ValidationT<F, M, B>(monoid =>
        from ff in mf.As().Run(monoid)
        from fa in ma.As().Run(monoid)
        select ff.ApplyI(fa, monoid).As());
```

- Because the transformer passes the monoid explicitly into `ApplyI`, the silent first-fault degradation of the bare carrier cannot occur inside the stack: the monoid is in scope by construction, so transformer-stacked admission accumulates correctly even for a typed family that lacks a reflectively-discoverable instance. The cost is that the family must satisfy the monoid at the seam where the transformer is run, not at every apply site.
- The transformer's lift moves an inner-monad effect into the admission layer without admitting (`Lift(K<M,A>)`), so an effectful source — a read that may fail in `M` — feeds the admission applicative as a peer field. Effectful field acquisition and pure field admission accumulate side by side in one carrier; the inner monad owns acquisition failure, the validation layer owns admission failure, and both surface through the one run.
- The transformer's alternative-empty manufactures `Fail(monoid.Empty)` lifted into the inner monad — the evidence-free rejection of the stacked carrier is the inner monad wrapping the monoid identity, so an empty aggregate at the bottom of a transformer stack is still a well-typed inner-monad value, and triage that gates on emptiness must reach through `M` to read it.

[NESTED_CARRIER_FLATTENING]:
- Two-phase admission yields a nested carrier — leaf fields accumulate applicatively into `K<F, TLeaf>` and the composite admits monadically over the leaves, producing `K<F, K<F, TComposite>>` whose join is the phase boundary. The applicative stage and the monadic stage are the same traverse skeleton with different combiners stacked; flattening between them is the carrier's own join, never a manual unwrap.
- The traversable contract supplies four entry points distinguished by the inner carrier's strength: `Traverse`/`Sequence` for an applicative inner carrier, `TraverseM`/`SequenceM` for a monadic one. A composite whose fields are independent uses the applicative pair and accumulates; a composite whose later fields read earlier admitted values uses the monadic pair and short-circuits. The same nested-collection admission expression changes fault semantics purely by which traverse strength the inner carrier offers.
- Default traversal derives `Traverse` from `Sequence` composed with `Map` (`sequence(Map(f, ta))`) — a traversable instance need only author one of the pair, and the carrier-polymorphic batch bridge inherits the other for free. The single authored direction is the transpose; everything else is derivation.

[CONSTRAINT_SHAPE_GATES_THE_POLYMORPHISM]:
- A carrier-polymorphic admission bridge constrains its carrier as `where F : Applicative<F>` and never narrower — constraining to a concrete carrier forfeits the entire point. The accumulation-versus-short-circuit choice then lives at the call site, which selects the carrier; the bridge body is one expression valid for all `F`.
- The raw-type parameter must mirror the factory contract's `allows ref struct` anti-constraint, because the admission interface itself declares its raw value as `notnull, allows ref struct`. A bridge whose raw parameter omits the anti-constraint silently excludes every span-keyed admission from the polymorphic path — the byref-like raw types are exactly the zero-allocation wire-buffer admissions, and dropping the anti-constraint amputates them without error.
- The factory contract is covariant in its error parameter, and constraint satisfaction flows through variant interface conversions even across the static-abstract members: an owner declaring the most precise fault case satisfies a bridge constrained on the fault base, and the static `Validate` dispatches to the owner's implementation viewed as the base. One base-constrained, carrier-polymorphic bridge therefore serves an entire fault lattice across every carrier — the owner picks the precise case, the carrier picks the composition semantics, and the single bridge declaration spans both axes.
- The receiver-extension shape is the only position from which the owner type infers in a zero-type-argument call, because generic method argument inference is all-or-nothing while extension-receiver inference binds the owner from the dotted receiver and the raw from the argument. A leading-type-parameter generic method cannot reach the same call ergonomics; the polymorphic bridge over the static-abstract factory must be a generic static extension on the owner, not a free function.
