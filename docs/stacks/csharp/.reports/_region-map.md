# [REGION_MAP] — csharp

Region ledger for the C# stack corpus: one owner per region, snippet entries before code, page entries at finalization, repairs route to the owner.

## [1]-[PAGE_REGIONS]

- language.md: language-form placement law — extension surfaces; pattern dispatch grammar; immutable carriers; collection composition; generic algebra; text literals; stack kernels; the canonical modern-spelling chooser.
- rails-and-effects.md: result and effect-flow law — carrier choice, representation, and identity; boundary conversion and exception capture; traversal sequencing policy; failure-combinator algebra; effect runtime, schedule policy with schedule-driven iteration, resource brackets, and catch-policy recovery; boundary state cells and the stream-versus-typed receipt split; carrier-polymorphic composition and transformer-stack capability order.
- system-apis.md: System.* replacement law — modern BCL surfaces deleting hand-rolled data-shape, runtime-primitive, and system-seam patterns; the unified smell table.
- testing/: proof rails — unit law matrices and property baselines; evidence rails; runtime verification scenarios; specialized proof tooling.

## [2]-[SNIPPET_REGIONS]

[LANGUAGE]:
- L-01 EXTENSION_SURFACE_SITE: extension blocks on a foreign BCL receiver — instance property and method; static property; non-conversion static operator.
- L-02 PATTERN_DISPATCH_SITE: switch-expression pattern grammar — list/slice, positional-in-list, property/relational/logical patterns over `ReadOnlySpan<T>`; constant-string and char list patterns over `ReadOnlySpan<char>`.
- L-03 IMMUTABLE_CARRIER_SITE: inert carrier surface — `required` + `init`; `field` accessor invariant; nested `with`-through-struct update across a sealed record and a readonly record struct.
- L-04 COLLECTION_COMPOSITION_SITE: construction shape — collection-expression spread; `params ReadOnlySpan<T>` and `params IReadOnlyList<T>` element shapes; implicit `^` index assignment inside object initializers.
- L-05 GENERIC_ALGEBRA_SITE: type-level algebra — static abstract members; static virtual default-body fold through the self-constraint; void-returning instance compound assignment.
- L-06 TEXT_LITERAL_SITE: literal forms — `u8` span constants including escape-bearing; `\e` terminal escapes in processed interpolation; `$$` raw-string interpolation with expression holes.
- L-07 STACK_KERNEL_SITE: stack-only kernel — `readonly ref struct` primary constructor; `ref` field with explicit-constructor ref-assign; `ref struct` interface implementation; `scoped ref` + `allows ref struct` step; confined fold loop.

[SHAPES]:
- S-01 FAULT_FAMILIES: union-shaped dual-contract fault family — `Expected` base with private-constructor closure; `IValidationError` static `Create` as the string-bearing tier beside structured cases; `Semigroup` aggregate-case `Combine` fold.
- S-02 RAIL_BRIDGE: generic constrained extension-block admission bridge — covariant fault-base factory-contract constraint; `allows ref struct` raw parameter; one-expression property-pattern projection with implicit rail lifts; three-valued option projection for null-yield owners.
- S-03 OPERATOR_ALGEBRA: operator-axis grant set as declared algebra — per-axis `OperatorsGeneration` declaration; hand-written cross-dimension operator re-entering admission; exact-binary-family generic kernel with admitted seed; key-typed relational threshold gate.
- S-04 DISPATCH_AND_ROWS: smart-enum behavior-row table — composite-returning `[UseDelegateFromConstructor]` delegate column beside a data column; generated constructor row injection; row invocation through the generated method surface.
- S-05 RAIL_ARMS: carrier-polymorphic catamorphism over a recursive generated union — trait-constrained `K<F,B>` fold; tuple-state static arms; applicative tuple combine in the recursive arm.
- S-06 KEY_AND_IDENTITY_POLICY: value-object comparer accessor policy — one accessor type swings equality, hashing, ordering, relational generation, and `CompareTo`; string policy is declared once per layer.
- S-07 AD_HOC_FORM: ad-hoc union parameter absorber — named slots, stateless member storage, mixed collection ingress, and generated probes replace overload families and payload bags.

[RAILS_AND_EFFECTS]:
- R-01 EXCEPTION_CAPTURE: exception capture — `Try.lift<Fin<T>>` + `Run` + `MapFail` + self-flattening `Bind` collapsing the double rail.
- R-02 RAIL_TRAVERSAL: rail traversal sequencing — indexed `Map`; chained `TraverseM(identity).As()` abort path; `Strict()` before boundary transfer.
- R-03 VALIDATION_APPLICATIVE: validation applicative — tuple `Apply` accumulation over `ToValidation` projections with monadic flatten.
- R-04 RUNTIME_RECORD: runtime-as-data — one multi-capability runtime record (policy, clock, state cell, cancellation) with dual `Eff.runtime<RT>().Map(...)` capability projections and `Eff.local` inner-runtime narrowing.
- R-05 RESOURCE_BOUNDARY: drive-polarity resource contract — fail-only `Retry` over the acquiring effect plus `IO.Bracket(Use, Catch, Fin)`.
- R-06 SCHEDULE_POLICY: schedule operator algebra — union magnitude-flooring and intersect gap-flooring over a bounded `Backoff` curve applied through `IO<T>.Retry`.
- R-07 ATOM_STATE: atom memoization — point read; `{ IsSome: true, Case: }` projection; contention-safe idempotent `Swap`; `IfNone` fallback.
- R-08 RECEIPTS: receipt monoid — record-struct fact carrier with identity element, `operator +`, and the `Fold(Empty, +)` collapse of N receipts into one.
- R-09 CAPABILITY_BY_STACK_ORDER: transformer-stack capability — carrier-polymorphic comprehension over `Stateful`+`Readable`+`MonadUnliftIO`+`Monad` with `mapIO` exposing the IO floor through the stack.
- R-10 CATCH_POLICY_CASCADE: recovery-as-data — factory-returned `CatchM` policy value composed first-match-wins through the carrier-polymorphic `K<M,A> | CatchM` ladder over a `Fallible<Error,M>, Monad<M>` body, facet predicates (`IsExceptional`, `expected`) selecting the failure class.
- R-11 CONVERGE: schedule-driven iteration — `RepeatWhile(Schedule, predicate)` over an `Atom`-advanced `IO.lift` effect, `recurs(n) & spaced` bound-meets-cadence and the atom state-advance as supporting material.

[SYSTEM_APIS]:
- SA-01 READ_MOSTLY_LOOKUP: span-keyed alternate lookup — frozen ordinal-comparer construction; cached `AlternateLookup` probe field; non-throwing `TryGetAlternateLookup` acquisition with span-keyed `TryAdd` mutation.
- SA-02 FORMAT_AND_PARSE: UTF-8 format-parse pipeline — `Utf8.TryWrite` invariant-culture interpolation; generic numeric round-trip through constraint subsumption; `u8` header constant and span slicing as supporting material.

[ALGORITHMS]:
- A-01 DENSE_ROUTE_DISPATCH: non-uniform factorization route — closed `FactorRoute` union binding five MathNet decompositions (`Cholesky`/`LU`/`QR`/`Evd`/`Svd`) to one shared `ISolver<double>` arrow through generated `Switch`, the rank-revealing case forcing `vectors:true` so absent-vector failure surfaces at admission, one `Bind` serving every case.
- A-02 ITERATIVE_WITNESS_GATE: Krylov solve witness — `TrySolveIterative` over an ordered `Iterator` criterion stack (`Failure`→`Divergence`→`Residual`→`Count`, size-scaled `8·dim` ceiling), the recomputed true-relative-residual against the original operator as the only admission test surviving preconditioning and breakdown substitution.
- A-03 SCHUR_PAIR_DECODE: real-Schur eigenvector lift — `Math.Sign(λ.Imaginary)` strided adjacent-column `Zip` gather over `Enumerate()` reconstructing the complex modal matrix, the eigenvalue vector as the sole pairing discriminant.
- A-04 SPECTRAL_SYMBOL_DRIVE: spectral operator apply — pointwise wavenumber-symbol drive between in-place `Fourier.Forward`/`Inverse` under `AsymmetricScaling`, the split-spectrum `WaveAxis` owner deriving wavenumbers via `Generate.LinearRangeMap`, even-length Nyquist odd-symbol zeroing.
- A-05 SPARSE_EDIT_ALGEBRA: structural-edit cost class — `Edit` union over three primitives dispatched by guarded `switch` with `when op.Kind is FactorKind.Spd`, a `false` rank-1 result folding to `Rebuild` (discard-never-retry) and a `var` catch-all routing every pattern change to reconstruction.
- A-06 CONDITIONING_FALLBACK_CONVERGENCE: primary→secondary route rebind on the concrete Fin rail — `.Bind(Gate)` then `.BindFail(_ => secondary…Bind(Gate))` — both conditioning routes converging on the ONE post-solve true-relative-residual gate, the taken path recorded via `nameof` into the failure; distinct from R-10 (no CatchM policy value, no `|` ladder, no facet predicates) and from A-01/A-02 (route dispatch and witness grammar reused as supporting material only).

[SURFACES_AND_DISPATCH]:
- SD-01 SEALED_ENTRYPOINT: regular-`[Union]` reachability seal — private owner constructor, one hand-written factory ingress over a span-`Validate` verb vocabulary with state-threaded construction `Switch`, extension-block dispatch egress.
- SD-02 MODAL_ARITY_ONE_ARM: one carrier-polymorphic `Lift<F>` arm consumed at two call shapes — `TraverseM` (abort) and `Traverse` over `Validation<Error>` (accumulate) with a monoid fold tail — the carrier alone switching the policy.
- SD-03 CARRIER_POLYMORPHIC_JOIN: three independent `Switch` dispatches joined once through the tuple `.Apply`, carrier-orthogonal accumulate-versus-abort; `Recur` stays prose-only.
- SD-04 TYPE_LEVEL_ADMIT: self-constrained `IObjectFactory<TOwner,TValue,TError>` boundary admitting every owner, span-keyed `allows ref struct` instantiation, owner-chosen `TError.Create`.
- SD-05 STACKING_ORDER: the acquire-once-versus-reacquire bracket/retry pair as two policies from two operator orders.

Entries under [RAILS_AND_EFFECTS] are the finalized snippet regions; R-03 was relabelled from RECOVERY to its proven card, R-05 replaced the statement-exemption capsule with the drive-polarity bracket, and R-09 opened the transformer-stack composition region. The complex-project hardening pass added R-10 (catch-policy cascade under EFFECT_RECOVERY) and R-11 (schedule-driven converge under SCHEDULE_POLICY), folded EFFECT_LIFTING's layer-vs-effect-lift law into CAPABILITY_BY_STACK_ORDER, merged OPERATIONAL_RECEIPTS and ALGORITHM_RECEIPTS into one RECEIPTS card (R-08 relabelled from ALGORITHM_RECEIPTS), and extended R-04's spotlight to the multi-capability runtime record.

## [3]-[KNOWN_OVERLAPS]

The L-07 / S-04 ref-struct fold collision was repaired at the shapes.md snippet assignment: the incumbent manual-owner fold snippet was retired and the replacement set demonstrates no ref-struct kernel.

A-01 and A-05 both compose a `[Union]` over a dispatch, but the spotlights are disjoint and scoped apart: A-01 demonstrates the MathNet solver-arrow binding under generated `.Switch`, A-05 the sparse edit-cost-class under a language `switch` expression with a cross-field `when` guard and reconstruction catch-all — no shared demonstration. A-03's three-arm `Math.Sign` relational `switch` is scoped distinct from L-02 (which owns `ReadOnlySpan<T>` pattern grammar): A-03's arms are a numeric discriminant whose payload is the strided eigenvector gather. A-02's `Try.lift` is scoped distinct from R-01 (which owns the `Try.lift`+`MapFail` exception-capture grammar): A-02's capture is minimal plumbing under the numeric residual-witness gate.

SD-01 is scoped distinct from S-04 (no delegate-row table — a construction `Switch`, not behavior rows). SD-02 is scoped distinct from R-02/R-03 (spotlight is one-arm-many-modalities; traversal verbs are supporting material). SD-03 is scoped distinct from S-05/R-09 (independent non-recursive join, no catamorphism, no transformer stack). SD-04 is scoped distinct from S-02 (open-owner-family inversion, not the per-owner rail-lift bridge). SD-05 is scoped distinct from R-05/R-06 (ORDER-as-policy pair; bracket and schedule mechanics are supporting material).
