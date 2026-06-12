# tensor-simd — bedrock

## tensor ownership and the span axis

- `Tensor<T>` is the sole owning rank-N carrier: a sealed class over one flat backing array plus a `TensorShape`
  carrying lengths, strides, and start offset.
- `TensorSpan<T>` / `ReadOnlyTensorSpan<T>` are the two ref-struct view polarities; `TensorDimensionSpan<T>` /
  `ReadOnlyTensorDimensionSpan<T>` are the per-dimension cursors. One owner, two view polarities, one cursor —
  the complete shape vocabulary.
- A local NDArray/matrix wrapper re-derives shape math the shape struct already owns; it is the rejected form
  regardless of how thin it is.
- The admission lattice is implicit-conversion-shaped: `T[]` → `Tensor<T>`, `Tensor<T>` → `TensorSpan<T>` →
  `ReadOnlyTensorSpan<T>`.
- Kernel signatures take `in ReadOnlyTensorSpan<T>` and write `in TensorSpan<T>`; the owner appears only at
  allocation and egress, so every interior surface is projection-blind to where memory came from.
- Element indexers take `params ReadOnlySpan<nint>` or `params ReadOnlySpan<NIndex>`; range slicing takes
  `params ReadOnlySpan<NRange>` and returns a view sharing storage with adjusted strides — `Slice` never copies.
- `Lengths` and `Strides` are `ReadOnlySpan<nint>`; `FlattenedLength` is `nint`. Shape arithmetic plumbed
  through `int` silently truncates on large extents; `int`-bound consumers project explicitly at their own
  boundary.
- Dense-vs-strided is a queryable class, never an assumption: `IsDense` and `HasAnyDenseDimensions` report
  layout.
- `TryGetSpan(startIndexes, length, out Span<T>)` returns false when the requested window crosses a stride
  discontinuity — the dense fast path is gated by this probe, and a route that skips the probe is wrong on every
  sliced view.
- `FlattenTo` / `TryFlattenTo(Span<T>)` export linearized content regardless of layout; they are the
  strided-to-linear bridge when the probe fails.
- `ToDenseTensor()` returns `this` unchanged when the tensor is already dense — densify-if-needed, not defensive
  copy. Mutating the "copy" mutates the original whenever the source was dense; a guaranteed-independent buffer
  requires `CreateFromShape` plus `CopyTo`.
- `TensorSpan<T>.operator ==` is view identity — same root reference and same shape — never content equality.
- Content equality has two distinct laws: `Tensor.EqualsAll` / `EqualsAny` (elementwise under
  `IEqualityOperators<T,T,bool>`, including span-vs-scalar forms) and `SequenceEqual` (`IEquatable`, shape plus
  content as one predicate). Choosing `==` where `SequenceEqual` is meant compiles and silently compares the
  wrong thing.
- Pinned residency is a creation-time policy: `CreateFromShape(lengths, pinned: true)`; `IsPinned` reports it;
  `GetPinnedHandle()` yields a `MemoryHandle`; `GetPinnableReference()` serves `fixed`. Residency is decided at
  allocation and never retrofitted onto a live tensor.
- `CreateFromShapeUninitialized` skips zero-init. Its admission law: legal only when every element is provably
  written before first read — full-coverage fills or a complete `SetSlice`/`CopyTo` plan. A partially-covered
  uninitialized tensor leaks stale pool content into results.
- Random fills are vocabulary members, not local RNG loops: `FillGaussianNormalDistribution` and
  `FillUniformDistribution` own stochastic initialization.
- `GetDimensionSpan(int dimension)` yields rank-(N−1) sub-views — batch iteration over a leading dimension
  without index arithmetic.
- `GetSpan(startIndexes, length)` / `TryGetSpan` also exist on the owner directly, so dense interior windows are
  addressable without first projecting a tensor span.
- `TensorMarshal.CreateTensorSpan` / `CreateReadOnlyTensorSpan` / `GetReference` form the raw-memory gate:
  native interop memory, pooled planes, and model outputs enter the tensor algebra as views without copy. The
  one seam where lifetime is the caller's proof obligation — the view must not outlive its root.
- Broadcast materializes; there is no lazy stride-0 repeated view: `Tensor.Broadcast(source, lengths)` validates
  compatibility, allocates uninitialized, and copies; `BroadcastTo` / `TryBroadcastTo` write into a caller
  destination.
- Memory cost of a broadcast equals the broadcast shape. Scalar operands therefore ride the scalar-position
  kernel overloads, never a broadcast scalar tensor; broadcasting is reserved for genuine shape expansion into a
  consumed destination.
- The shape-edit vocabulary is closed: `Reshape` (view when layout permits), `Squeeze` / `SqueezeDimension` /
  `Unsqueeze`, `PermuteDimensions` / `Transpose`, `Concatenate` / `ConcatenateOnDimension`, `Stack` /
  `StackAlongDimension`, `Split`, `Reverse` / `ReverseDimension`, `Resize` / `ResizeTo`.
- Region writes are owned: `SetSlice(values, ranges)` writes a window; `FilteredUpdate(filter, value | values)`
  writes under a boolean mask tensor — masked update never needs an element loop.
- Every composition and remap member carries a destination overload (`in TensorSpan<T> destination`) beside its
  allocating form — allocation-free pipelines are first-class, not a special mode.
- `Tensor<T>.Empty` and `TensorSpan<T>.Empty` are the canonical empty rows; emptiness is `IsEmpty`, never a null
  owner — absence of payload is a shape state, not a reference state.
- Enumeration over a strided view walks logical index order via linear-offset advance — `foreach` over a
  transposed or sliced view yields elements in the view's order, not storage order, so order-sensitive folds are
  layout-safe by construction.
- `NIndex` and `NRange` carry from-end addressing at native width — the `^`/`..` grammar reaches rank-N shapes,
  and negative-index helper arithmetic at call sites is the foreclosed spelling.
- The non-generic float overload family coexists with the generic family as the same kernels; the writing law:
  the generic spelling is the only one authored — dtype changes then never touch call sites.

## primitives dispatch vocabulary

- `TensorPrimitives` is one flat static vocabulary over `ReadOnlySpan<T>` / `Span<T>` where the generic-math
  constraint IS the dispatch row's admission column.
- Constraint rows: `Add`/`Sum` require `IAdditionOperators` + `IAdditiveIdentity`; `Dot` requires both additive
  and multiplicative identities; `Exp`/`Sigmoid`/`SoftMax` require `IExponentialFunctions`;
  `Norm`/`StdDev`/`CosineSimilarity` require `IRootFunctions`; `FusedMultiplyAdd` requires
  `IFloatingPointIeee754`; `Clamp`/`IndexOfMax` require `INumber`; `Abs`/`Average`/conversions require
  `INumberBase`; rounding requires `IFloatingPoint`.
- Dtype admission is compile-time constraint satisfaction, never a runtime check — an inadmissible dtype-op pair
  does not exist as a callable, so the dispatch table cannot hold an invalid row.
- Arity is the only structural axis. Unary: span→span. Binary: span×span and span×scalar. Ternary:
  `MultiplyAdd`, `AddMultiply`, `FusedMultiplyAdd`, `Lerp`, `Clamp` — each enumerating every span/scalar
  placement, so no operand position ever forces materialization.
- Reductions span→T: `Sum`, `Product`, `Min`/`Max`, `MinNumber`/`MaxNumber`, `MinMagnitude`/`MaxMagnitude`
  (+`Number` variants), `Norm`, `SumOfSquares`, `SumOfMagnitudes`, `ProductOfSums`, `ProductOfDifferences`,
  `Average`, `StdDev`.
- Searches span→int: `IndexOfMax` / `IndexOfMin` / `IndexOfMaxMagnitude` / `IndexOfMinMagnitude`.
- Predicates span→bool: `IsFiniteAll` / `IsFiniteAny`, `IsNaNAll` / `IsNaNAny`, `IsInfinityAny`, sign probes
  (`IsPositive`, `IsNegative` families).
- Pair reductions: `Dot`, `Distance`, `CosineSimilarity`, `HammingDistance`. `HammingDistance` counts positions
  where default equality fails — it is dtype-agnostic (no numeric constraint) and serves byte-identity diffing
  as well as numeric vectors.
- Dual-output kernels: `SinCos`, `SinCosPi`, `DivRem` — two destinations whose result regions must be disjoint.
- NaN semantics are an explicit column, not folklore: `Min`/`Max` match IEEE 754:2019 `minimum`/`maximum`
  (NaN-propagating — one NaN poisons the reduction); `MinNumber`/`MaxNumber` match
  `minimumNumber`/`maximumNumber` (NaN-ignoring — NaN is treated as missing data). Choosing the wrong column
  silently changes what a pipeline reports on dirty data.
- The fused-multiply triad is a numeric-contract axis, not a speed knob: `MultiplyAdd` permits double rounding;
  `FusedMultiplyAdd` guarantees single rounding per element including the scalar tail; `MultiplyAddEstimate`
  licenses the platform to choose, so its results legitimately differ across machines.
- Any reproducibility claim must pin which of the three it covers; `MultiplyAddEstimate` is inadmissible
  wherever cross-machine bit agreement is part of the contract.
- The in-place law: destination may alias an input only when both start at the same memory location; partial
  overlap throws. `Exp(x, x)` is legal; `Exp(x, x.Slice(1))` throws.
- Windowed in-place pipelines are therefore forbidden by the library itself — shifting-window algorithms need a
  scratch span by construction, not by convention.
- The conversion triad is a policy column: `ConvertChecked` (throw on out-of-range), `ConvertSaturating`
  (clamp), `ConvertTruncating` (wrap). All require `INumberBase` on both ends; same-type conversion degrades to
  copy.
- Primitive widenings ride dedicated vector paths (byte→ushort/uint/float and peers) — a conversion-dtype table
  selects the verb as a column, and three sibling call sites choosing verbs by `if` is the collapse trigger.
- Integer-only families exist where float families do not: `BitwiseAnd` / `BitwiseOr` / `Xor`, `ShiftLeft`,
  `ShiftRightArithmetic`, `PopCount`, `TrailingZeroCount` — bit-plane work is vocabulary, not loops.
- Transcendental coverage is wide enough that a missing function is suspicious before it is real: `Pow`,
  `Log`/`Log2`, `Sqrt`/`Cbrt`, trig and hyperbolic families with `Pi` variants, `Atan2`/`Atan2Pi`,
  `Ieee754Remainder`, `DegreesToRadians`, `CopySign`, `Sign`, `Round` with digits and `MidpointRounding`
  overloads.
- Scalar broadcasting is free at every binary/ternary kernel via the scalar-position overloads; materializing a
  constant tensor to feed a span×span overload is the rejected spelling.
- Search-row NaN semantics: `IndexOfMax`/`IndexOfMin` return the index of the FIRST NaN when any NaN is present,
  and positive zero ranks above negative zero — index searches are NaN-poisoning by position, so a NaN-bearing
  payload returns a NaN's index, not the largest finite value's.
- C-runtime-backed transcendental rows carry a documented platform-variance remark: exact results may differ
  across operating systems and architectures — these rows are cross-platform-variant by contract, which assigns
  them a tolerance class no amount of pinning removes.
- Statistical reductions are the named analytical fold vocabulary: `Sum`, `Average`, `StdDev`, `Norm`,
  `Min`/`Max`, `IndexOfMax` are fold rows feeding consumers that render totals and never recompute them.
- The fold lives once at the vocabulary; a renderer, report, or receipt receives the folded value — two
  consumers can never disagree about a total because neither computes it.
- The library-owns-the-algorithm boundary: algorithmic reduction — downsampling (stride-slice views per bucket
  plus one reduction row per bucket), batch flatten (`FlattenTo`, dimension cursors), masked update
  (`FilteredUpdate`) — is owned once as named rows over this vocabulary.
- A consumer that loops elements to downsample or flatten re-derives a kernel the vocabulary already names;
  consumer verbs are row selection and rendering, nothing else.

## dtype rows and tolerance classes

- The dtype axis carries an acceleration class per row: `float`/`double` are full-width rows; binary integers
  are exact rows with the bitwise families.
- `Half` rides widen-to-float vector kernels — half lanes widen, compute in float, narrow back. `Half`
  throughput is float-class, and `Half` results equal compute-in-float-then-narrow by construction of the route.
- The model-boundary brain-float carrier is not a primitives dtype; it converts at the inference admission seam
  and never enters span kernels.
- Finite admission short-circuits by dtype class: on binary-integer and decimal element types `IsFiniteAll`
  returns true without scanning. The finite-gate column does not exist on integer rows; a dtype table that runs
  the gate uniformly wastes a pass per integer payload. The vectorized gate demonstration itself is settled root
  law; this lane owns only the dtype-row consequence.
- The empty edge inverts the gate: `IsFiniteAll` on an empty span returns false. Emptiness must be decided by
  its own arm before the finite gate, or an empty payload mis-codes as "non-finite" and corrupts the rejection
  taxonomy.
- Tolerance classes are a closed vocabulary keyed by (op family, dtype): exact — integer kernels, copies,
  conversions among exactly-representable values.
- ULP-banded — same-route float transcendentals; per-element relative bound.
- Accumulation-scaled — reductions: vectorized `Sum`/`Dot` reassociate additions across lanes, so scalar and
  vectorized totals legitimately differ within a bound scaling as N·ε·Σ|xᵢ|. A fixed epsilon on long or
  ill-conditioned series is structurally wrong.
- The cancellation ratio |Σxᵢ| / Σ|xᵢ| decides when even the scaled bound is meaningless — heavy cancellation
  makes any route-equivalence claim on the total vacuous, and the proof row records the ratio class it was
  proven under.
- Platform-variant — `MultiplyAddEstimate` and estimate-class rows; no cross-machine tolerance class exists for
  them at all.
- Cross-platform-variant — C-runtime-backed transcendentals: same machine reproducible, cross-OS only
  ULP-banded; a golden-vector suite pinned to exact bits on these rows fails on the next platform by design, so
  golden vectors for them are banded, never exact.
- The equivalence-proof law: every vectorized route in the dispatch vocabulary carries a scalar reference fold
  and a proof row of (op identity, dtype, arity, tolerance class, length classes).
- Mandatory length classes: empty, one, sub-vector-width, exact width multiples, and
  width-multiple-plus-remainder. The remainder tail of every vectorized kernel executes scalar code — only
  lengths straddling the width boundary exercise both paths in one call, and a proof sweep using only round
  lengths certifies half the kernel.
- Equivalence and speed are orthogonal gates on the same route row: this lane owns the correctness proof; the
  measured-claim gate is the dispatch lane's. A route failing its equivalence row is poisoned regardless of any
  speed claim and demotes to the scalar reference until re-proven.

## tensor-rank composition

- The static `Tensor` class mirrors the primitives at tensor rank — `Add` / `Subtract` / `Multiply` / `Divide` /
  `Exp` / `SoftMax` / `Sum` / `Dot` / `CosineSimilarity` over `ReadOnlyTensorSpan` with identical generic-math
  constraints — adding shape validation and stride-aware iteration.
- Every tensor-rank op has both an allocating form and a destination form; pipelines that already own their
  output buffers never allocate.
- Division covers the scalar-left arm (`Divide(T x, in ReadOnlyTensorSpan<T> y, …)`) — reciprocal-family
  expressions need no manufactured one-tensor.
- The altitude split: tensor-rank ops are the composition route (shape-checked, layout-tolerant); span-rank
  primitives are the measured route.
- A hot loop flattens dense operands through the `TryGetSpan` / `FlattenTo` probe and calls the span kernel
  directly; tensor-rank convenience inside a measured kernel is the rejected spelling because it re-validates
  shape per call.

## divergent — tensor-span-law

- Maximal unification: every memory class enters the algebra through exactly one of three gates — owner gate
  (`Tensor.Create*` over arrays and shapes), span gate (`TensorSpan` constructors over array, `Span`, or pointer
  with lengths and strides), marshal gate (`TensorMarshal` over ref-rooted foreign memory).
- Each gate row carries its lifetime class: GC-owned, caller-scoped, pinned-native. A pooled staging plane, a
  model output buffer, and a plain array become one view vocabulary; every downstream kernel is gate-blind.
- The growth proof: a new memory source is one gate-row decision, zero new types; a new rank is data (a lengths
  array), zero new code.
- The view/owner split is the state law made structural: views are call-graph material (ref structs — storing
  one in a field is compiler-rejected), owners are state. An API that retains tensor data retains the owner and
  re-projects per use; "caching a span" is the foreclosed defect, not a missing feature.
- Rejected-form catalogue: local tensor/matrix wrappers (shape-math re-derivation); broadcast-materialize for
  scalar operands (scalar-position overloads exist); defensive `ToDenseTensor` as copy (identity on dense
  input); shape-check helper functions (validation is built into every composing op); `==` for content (view
  identity only); `int`-typed shape plumbing (`nint` extents truncate).
- Cross-feature interaction: pinned creation + `GetPinnedHandle` + the marshal gate compose into a no-copy
  interop loop — allocate pinned once, hand the same root to native and managed kernels alternately. The
  staleness hazard is mutation ordering, which the owning capsule's borrow discipline already legislates at
  root.

## divergent — primitives-dispatch-vocabulary

- The whole surface collapses to one table: rows are semantic ops; columns are arity shape, constraint family,
  in-place legality (same-start only), NaN column, conversion/rounding policy, tolerance class, scalar
  reference.
- One generic kernel per arity binds the constraint once; everything else is row data. Because admission is the
  constraint, the type system is the table's integrity check.
- Row identity travels symbolically: a row names its kernel via method-group identity (`nameof`-stable). The
  proof row, the speed claim, and the receipt all key on one symbol — a renamed kernel breaks every stale
  reference loudly at compile time instead of serving stale claims.
- Fold rows as policy values: a named reduction row (op symbol + tolerance class + dtype) is a value passed to
  consumers; consumers selecting reductions by enum-and-switch re-derive what the row already carries.
- Multi-statistic requests over one payload batch their power-sum members: `Sum` + `SumOfSquares` derive mean
  and variance in two passes maximum instead of N independent sweeps; the derivation lives beside the rows so no
  consumer re-implements streaming accumulation the vocabulary's reductions already imply.
- Failure-mode taxonomy for the vocabulary: shape/length mismatch — throws at the kernel, unreachable in the
  interior over admitted intents; overlap violation — throws, repaired by scratch discipline; constraint absence
  — uncallable, not a runtime class at all; tolerance breach — equivalence-row failure, the only failure that is
  data rather than exception, and the only one with a demotion path instead of a fix-the-caller path.

## divergent — dtype-tolerance-equivalence

- Claim-identity fingerprint: an equivalence row is identified by (op symbol, dtype, arity, length class,
  vector-width class). Vector width varies by machine, so width lives inside the row identity and proofs re-run
  when the width class changes; the staleness and demotion algebra over those identities is the dispatch lane's
  gate, composed here only as the consumer of this identity shape.
- Cross-dtype proof economy: `Half` kernels equal float-kernel-then-narrow by route construction, so the float
  proof plus one narrowing-boundary check — values near the half-precision overflow threshold where narrowing
  saturates to infinity — certifies the half row. A full independent half sweep adds cost without adding
  coverage.
- Reassociation is the only legitimate scalar/vector divergence for arithmetic rows. A divergence on an
  elementwise (non-reduction) row at the same dtype is a defect, never a tolerance case: elementwise rows carry
  the exact or ULP class, and a sweep that "fixes" an elementwise mismatch by widening tolerance is masking a
  kernel bug.
- NaN-column equivalence is binary, not toleranced: `Min` vs `MinNumber` differ by definition on NaN-bearing
  inputs; a proof row for a NaN-propagating kernel must include NaN-bearing vectors or it certifies the kernel
  only on clean data — the dirty-data behavior is exactly what the column exists to pin.
- The dual-gate composition stated once: route admission = equivalence proof (this lane) ∧ measured claim
  (dispatch lane); either gate failing demotes to the scalar reference, the never-vetoed terminal row.
  Correctness is the floor; speed is the privilege.
