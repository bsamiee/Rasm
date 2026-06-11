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
- S-01 ADMISSION_STACK: generated admission stack — union-shaped typed fault implementing the validation-error contract; key-membered scalar value object with comparer policy and normalizing validation partial; complex value object with typed-fault validation; try-create-to-`Fin` bridge.
- S-02 SMART_ENUM: smart-enum behavior rows — constructor delegates with generated delegate partial; keyed lookup projected into `Fin`.
- S-03 UNION: state-threaded generated `Switch` with static arms inside an `Eff` query expression composing traversal and fold.
- S-04 MANUAL_VARIANT_OWNER: manual variant owner — ref-struct fold owner with confined statement loop and finish projection.

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

Entries under [SHAPES] and [RAILS_AND_EFFECTS] describe incumbent partial pages; each rebuild retires or replaces its set at snippet assignment.

## [3]-[KNOWN_OVERLAPS]

- L-07 / S-04 — both demonstrate a ref-struct fold kernel with a confined statement loop. Owner: language.md. Repair lands at the shapes.md rebuild's snippet assignment: the manual-owner decision demonstrates through a non-loop region such as type-indexed projection.
