# Carrier-Polymorphic Admission Traversal

[KINDED_ARROW_LAW]:
- An admission arrow typed `Func<TRaw, K<F, TOwner>>` — not a concrete `Func<TRaw, Validation<Error, TOwner>>` — is carrier-agnostic: one arrow drives accumulating, short-circuiting, effectful, and transformer-stacked admission depending solely on which `F` the caller supplies. The concrete-typed bridge is the special case obtained by fixing `F`; the kinded form is the primary.
- The trait the composition demands selects exactly the semantics it is allowed to use: `Applicative<F>` permits combining independent admissions but forbids inspecting one to decide the next; `Monad<F>` adds sequential dependency at the cost of parallel accumulation; `Choice<F>`/`Alternative<F>` add ordered fallback; `Fallible<TFail, F>` adds carrier-agnostic rejection. The weakest sufficient trait is the correct constraint — demanding `Monad` forecloses ever specializing the same composition to an accumulating carrier.
- Bare `Applicative<F>` cannot inject a fault — it exposes only `Pure`, never a failure constructor — so a polymorphic arrow that must reject constrains on the conjunction with `Fallible<TFail, F>`: success is `F.Pure`, rejection is `F.Fail`, and the pair is the precise capability set that lets one arrow both succeed in any applicative and fail in any fallible carrier.
- The applicative contract carries two `Apply` overloads — an eager `K<F, A>` right operand and a lazy `Memo<F, A>` right operand forced only after the function carrier is known. First-success grammars compose through `Choose(K<F, A>, Memo<F, A>)`, which returns the left untouched when it is already a success and forces the memoized right only otherwise — deferred fallback admission is a trait-level `Memo` overload, never a hand-rolled control-flow trick.
- Carrier migration is a named arrow, not a re-admission: `Natural<TFrom, TTo>` instances expose `static abstract K<G, A> Transform<A>(K<F, A>)`, so an admitted value moves between sequential, lazy-effect, optional, and either carriers by transformation rather than unwrap-and-rewrap — carrier choice at one seam never forecloses carrier choice at the next.

[TRAVERSE_IS_THE_BATCH_BRIDGE]:
- Batch admission of a homogeneous collection is one traversal: `raws.Traverse(Owner.Admit)` lifts `Seq<TRaw>` and the kinded arrow to `K<F, Seq<TOwner>>`. `Traverse` and `TraverseM` share one fold skeleton and differ only in the combiner — `Traverse` threads state through the applicative lift (independence: every element evaluated, all faults surfaced under an accumulating `F`), `TraverseM` threads through `Bind` (dependence: first failure aborts the remainder). All-faults-versus-first-fault is the choice of `F` and the verb, derived from one fold, never two code paths.
- The accumulation guarantee lives in the failure type's algebra, not in the traversal verb: applicative traversal over a fallible carrier accumulates only when the failure type carries a semigroup; with a non-monoidal failure it still type-checks and silently degenerates to first-error semantics. A traversal that must accumulate constrains the failure type's algebra, not merely the verb.
- `Sequence` is `Traverse` of identity — the transpose `K<T, K<F, A>> -> K<F, K<T, A>>` collapses a sequence of already-launched admissions into one carrier holding the sequence — and default traversal derives `Traverse` as `Sequence` composed with `Map`, so a traversable instance authors one direction and inherits the other.

```csharp
public static K<F, Seq<TOwner>> AdmitAll<F, TOwner, TRaw>(Seq<TRaw> raws)
    where F : Applicative<F>, Fallible<Fault, F>
    where TOwner : class, IObjectFactory<TOwner, TRaw, Fault>
    where TRaw : notnull, allows ref struct =>
    raws.Traverse(raw => TOwner.Validate(raw, null, out var owned) is { } fault
        ? F.Fail<TOwner>(fault)   // kinded fault lift — accumulates under the accumulating F, halts under the sequential F
        : F.Pure(owned!))
        .As();
```

[SEMIGROUP_RESOLUTION_MECHANISM]:
- The accumulating carrier's `Apply` resolves the failure semigroup by reflecting for the closed trait instance — `MakeGenericType(typeof(TFail))` then a static `Instance` property read, wrapped as an option of the instance — so the lookup is exact in the static failure type and invisible to the constraint solver. A composition that never names the identity operations (alternative-empty, monoid-unit) never trips the loud unsupported-operation path and therefore never learns its semigroup was missing: the first multi-failure input, not the call site, is where the degradation surfaces.

[TRANSFORMER_THREADS_THE_MONOID]:
- The validation transformer carries `Func<MonoidInstance<TFail>, K<M, Validation<TFail, A>>>` — the run function takes the monoid as a parameter instead of reflecting for it. Stacked admission therefore cannot degrade to first-fault silently: the monoid is in scope by construction at every apply, and the obligation moves to the single seam where the transformer is run, not to every apply site.

```csharp
// transformer Apply: M sequences, monoid accumulates — both in one expression
static K<ValidationT<F, M>, B> Apply<A, B>(K<ValidationT<F, M>, Func<A, B>> mf, K<ValidationT<F, M>, A> ma) =>
    new ValidationT<F, M, B>(monoid =>
        from ff in mf.As().Run(monoid)
        from fa in ma.As().Run(monoid)
        select ff.ApplyI(fa, monoid).As());
```

- The transformer's lift moves an inner-monad effect into the admission layer without admitting: an effectful source — a read that may fail in `M` — feeds the admission applicative as a peer field beside pure field admissions. The inner monad owns acquisition failure, the validation layer owns admission failure, and both surface through the one run.
- The transformer's alternative-empty manufactures the monoid identity as a failure lifted into the inner monad — an evidence-free rejection at the bottom of a stack is a well-typed inner-monad value, so triage that gates on aggregate emptiness must reach through `M` to read it.

[NESTED_CARRIER_FLATTENING]:
- Two-phase admission yields a nested carrier — leaf fields accumulate applicatively into `K<F, TLeaf>`, the composite admits monadically over admitted leaves, producing `K<F, K<F, TComposite>>` whose join is the phase boundary. Flattening between phases is the carrier's own join, never a manual unwrap, and the four traversable entry points — `Traverse`/`Sequence` for an applicative inner carrier, `TraverseM`/`SequenceM` for a monadic one — select the fault semantics of a nested-collection admission purely by which strength the inner carrier offers.
