# [REGION_MAP] — csharp

Region ledger for the C# stack corpus: one owner per region, snippet entries before code, page entries at finalization, repairs route to the owner.

## [1]-[PAGE_REGIONS]

- language.md: language-form placement law — extension surfaces; pattern dispatch grammar; immutable carriers; collection composition; generic algebra; text literals; stack kernels; the canonical modern-spelling chooser.
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

[RAILS_AND_EFFECTS]:
- R-01 EXCEPTION_CAPTURE: exception capture — `Try.lift` + `Run` + `MapFail` + self-flattening `Bind`.
- R-02 RAIL_TRAVERSAL: rail traversal — indexed `Map`; `TraverseM(identity).As()`; chained traversal; `Strict()` before boundary transfer.
- R-03 RECOVERY: validation applicative — tuple `Apply` over `ToValidation` projections with monadic flatten.
- R-04 EFFECT_LIFTING: effect runtime ask — runtime record plus `Eff.runtime<RT>()` capability projection.
- R-05 RESOURCE_BOUNDARY: resource capsule — `using` acquisition inside a boundary capsule returning a rail; the named statement exemption.
- R-06 SCHEDULE_POLICY: schedule retry — `IO<T>.Retry` with exponential-union-recurs schedule and units-of-measure duration.
- R-07 ATOM_STATE: atom memoization — point read; `{ IsSome: true, Case: }` projection; contention-safe `Swap` with idempotent transition; `IfNone` fallback.
- R-08 ALGORITHM_RECEIPTS: receipt monoid — record-struct fact carrier with identity element and `operator +` fold algebra.

[SYSTEM_APIS]:
- SA-01 READ_MOSTLY_LOOKUP: span-keyed alternate lookup — frozen ordinal-comparer construction; cached `AlternateLookup` probe field; non-throwing `TryGetAlternateLookup` acquisition with span-keyed `TryAdd` mutation.
- SA-02 FORMAT_AND_PARSE: UTF-8 format-parse pipeline — `Utf8.TryWrite` invariant-culture interpolation; generic numeric round-trip through constraint subsumption; `u8` header constant and span slicing as supporting material.

Entries under [RAILS_AND_EFFECTS] describe an incumbent partial page; the rebuild retires or replaces its set at snippet assignment.

## [3]-[KNOWN_OVERLAPS]

None recorded. The L-07 / S-04 ref-struct fold collision was repaired at the shapes.md snippet assignment: the incumbent manual-owner fold snippet was retired and the replacement set demonstrates no ref-struct kernel.
