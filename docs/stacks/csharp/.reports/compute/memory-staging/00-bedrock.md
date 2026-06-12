# memory-staging — bedrock

## allocation-class axis

- The axis is one closed five-class vocabulary; every staging allocation passes one selection fold whose
  discriminant is recoverable from the payload itself: size knowledge × lifetime × exit seam.
- The five classes: stack (`stackalloc` — size known, small, synchronous, never escapes); scoped pooled
  (`SpanOwner<T>` — size known, method-local, synchronous, unbounded size); owned pooled (`MemoryOwner<T>` —
  crosses awaits, stored, sliced, handed off); append writer (`ArrayPoolBufferWriter<T>` — final size unknown,
  writer-shaped production); pooled stream (byte payloads crossing IO and codec seams).
- A sixth spelling — `new T[]` or a raw growable memory stream in a staging path — is the rejected row:
  unpooled, unreceipted GC and large-object pressure.
- `stackalloc` carries two hard bounds: a small fixed byte cap (a declared policy constant on the order of
  hundreds of bytes to one KiB, never a per-site judgment) and never-inside-a-loop — stack space releases at
  method exit, not scope exit, so loop iteration compounds frames toward overflow.
- Above the cap or under iteration, the fold selects `SpanOwner<T>`.
- `SpanOwner<T>` is rent-with-stack-discipline, not stackalloc-backed: `Allocate(int size)`, `Allocate(size,
  AllocationMode)`, `Allocate(size, ArrayPool<T>, AllocationMode)` always rent from a pool.
- The ref-struct shape makes escape a compile error and `Dispose` the guaranteed return. Its value over
  stackalloc is unbounded size at identical scoping; over `MemoryOwner` it is zero heap objects per rental.
- `MemoryOwner<T>` is the storable owner: same `Allocate` family, `Memory` / `Span` projections; `Length` is the
  requested size — the rented array is larger and the owner clips, so length never equals capacity.
- `MemoryOwner<T>.Slice(start, length)` is a CONSUMING transfer: it nulls the source owner's array and returns a
  new owner over the same rented buffer. The source is dead after `Slice` — further use throws the disposed
  rejection — and exactly one owner ever returns the array.
- Treating `Slice` as a borrowing view is the trap; the transfer-not-borrow shape is the same single-ownership
  polarity the root capsule law legislates, composed here for pooled buffers.
- `AllocationMode` is the reset column at rent time: pooled rents carry prior request content; `Clear` zeroes on
  rent, `Default` does not.
- The hygiene rule: payloads partially written before first read, or crossing a trust seam, rent `Clear`;
  fully-overwritten payloads rent `Default` and keep the bandwidth.
- Per-lane pool isolation is a constructor column, not new code: the `ArrayPool<T>` parameter on every
  `Allocate` admits dedicated pools per lane so one lane's size-class churn cannot evict another lane's buffers.
- `ArrayPoolBufferWriter<T>` owns growth-by-rent production: `GetSpan` / `GetMemory(sizeHint)` +
  `Advance(count)`; `WrittenSpan` / `WrittenMemory` / `WrittenCount` are the read projection; `Clear()` resets
  the written window for in-place reuse across cycles; `Capacity` / `FreeCapacity` serve budget probes.
- The writer is simultaneously `IBufferWriter<T>` and `IMemoryOwner<T>` — producers see the writer contract, the
  lifetime holder sees the owner contract, one object.
- `MemoryBufferWriter<T>` is the fixed-capacity sibling: constructed over caller-supplied `Memory<T>`, same
  writer verbs, no growth — the writer row for budget-exact protocols where exceeding the window must fail
  rather than rent.
- Raw rented arrays resize through `ArrayPoolExtensions.Resize` / `EnsureCapacity` — rent-copy-return as one
  verb; hand-rolled rent/copy/return triples at call sites are the collapse target.
- `DangerousGetArray()` / `DangerousGetReference()` are the zero-copy bridges to array-demanding and
  ref-demanding APIs; the legality column: the projection dies with the owner, and the segment must never be
  stored past the owner's return.
- `MemoryOwner<byte>.AsStream()` transfers disposal into the produced stream — the caller stops tracking the
  owner and closing the stream returns the buffer. Single-owner transfer, same polarity as `Slice`; holding both
  handles is the double-return defect.

## views and planes

- `Span2D<T>` / `ReadOnlySpan2D<T>` are the dense-plane views: `Height`, `Width`, `nint`-typed `Length`, row and
  column enumeration (`GetRow` / `GetColumn`), `GetRowSpan` for the contiguous row, six `Slice` overloads,
  `Fill` / `Clear` / `CopyTo` / `TryCopyTo`.
- `Memory2D<T>` / `ReadOnlyMemory2D<T>` are the storable counterparts with `Pin()` and `TryGetMemory`; planes
  over arrays, memory, and pointers unify two-dimensional staging without a local matrix type.
- Contiguity is a probe, not an assumption: `TryGetSpan(out Span<T>)` succeeds only when the plane is gap-free;
  a sliced plane carries row pitch ≠ width and fails.
- The probe is the plane-to-tensor seam: a dense plane's flat span enters the tensor gate directly; a pitched
  plane row-iterates via `GetRowSpan`. The probe-then-route shape mirrors the tensor lane's dense gate — one
  law, two surfaces.
- `Cast` / `AsBytes` reinterpret span bytes; the admission rule: byte reinterpretation demands explicit codec
  and endianness ownership at the calling rail — a reinterpreting projection without a named codec owner is the
  rejected form.
- Ref carriers (`Ref<T>`, `ReadOnlyRef<T>`, `NullableRef<T>`, `Box<T>`) and span tokenizers stay
  package-internal implementation material; they never become staging vocabulary.
- `StringPool` interns transient text without intermediate strings: `GetOrAdd(ReadOnlySpan<char>)` and
  `GetOrAdd(ReadOnlySpan<byte>, Encoding)` deduplicate straight from parse and wire buffers; `TryGet` probes
  without admission; `Reset` drops a generation.
- Pool capacity is fixed at construction with priority-bucket eviction; the shared instance serves casual use
  and sized instances serve lanes with known cardinality.
- Placement law: pooled text belongs to staging receipts and diagnostic labels, never to domain values — domain
  text is admitted through owners, not interned.
- `HashCode<T>.Combine(ReadOnlySpan<T>)` is the one-call content hash over value spans — claim keys, cache keys,
  and payload identity hash flattened spans through this verb instead of per-element accumulation loops; it is
  the staging-side input to every content-keyed seam.
- `BitHelper` owns bit-plane flag work over integer carriers (`SetFlag`/`HasFlag`, `SetRange`/`ExtractRange`,
  `HasZeroByte`/`HasByteEqualTo`) — packed capability masks and SWAR-class byte probes are vocabulary, not
  shift-and-mask expressions at call sites.
- `ObjectMarshal` reads object internals and is internal implementation material under the same law as the ref
  carriers — never staging vocabulary.
- The stream extension verbs `Read`/`Write` move span payloads across arbitrary streams at the IO seam; together
  with `AsStream` over memory, owners, and writers they are the only stream bridges — stream implementation
  types stay package-internal and never appear in compute vocabulary.
- A flat owner becomes a plane by projection, not reallocation: `AsMemory2D`/`AsSpan2D` over memory and arrays
  carry (height, width) onto rented buffers — 2-D staging is a view decision over the same pooled bytes.
- `Memory2D<T>.Pin()` yields the memory handle for native seams — plane-shaped interop pins through the plane,
  keeping pin scope congruent with use scope.

## one process stream pool

- Exactly one `RecyclableMemoryStreamManager` per process; `RecyclableMemoryStreamManager.Options` freezes
  policy at construction: `BlockSize`, `LargeBufferMultiple`, `MaximumBufferSize`, the two free-byte caps,
  `UseExponentialLargeBuffer`, and the hygiene toggles.
- Pool policy is one declared record co-located with staging policy; a second manager forks the block economy
  and doubles steady-state memory.
- The two-pool economy: the small pool serves fixed-size blocks chained into streams (default block 128 KiB);
  the large pool serves contiguous buffers in `LargeBufferMultiple` steps (default 1 MiB) up to
  `MaximumBufferSize` (default 128 MiB), linear by default, exponential under the growth-curve option.
- A stream lives entirely in chained blocks until something demands contiguity.
- `GetBuffer()` is the contiguity cliff: it migrates the whole stream from chained blocks into one large buffer
  — or an unpooled allocation past `MaximumBufferSize`.
- `GetReadOnlySequence()` reads the chain without migration; `TryGetBuffer` is the non-throwing contiguity
  probe.
- The stream is itself `IBufferWriter<byte>` (`GetSpan` / `GetMemory` / `Advance`) — codec seams write into the
  chain directly rather than through `Write` copies.
- The route law: sequence reads and writer-shaped writes are the default rows; `GetBuffer` is admitted only
  where a downstream contract demands one contiguous span.
- `ToArray` is the double-cost edge — contiguity migration plus a full copy; it requires an explicit receipt
  reason, and `ThrowExceptionOnToArray = true` converts the policy into a structural ban for lanes that must
  never copy out.
- Zero free-byte caps mean unbounded retention: with `MaximumSmallPoolFreeBytes` / `MaximumLargePoolFreeBytes`
  at their zero defaults the return path always pools, so the process holds its high-water mark forever. Bounded
  caps are the production posture.
- With caps set, returns past the cap discard with reason `EnoughFree` — healthy bounded behavior, not a defect
  signal.
- `MaximumStreamCapacity` rejects runaway streams; the breach surfaces as the over-capacity event plus an
  exception, so a runaway producer is stopped by policy rather than by the machine.
- `GetStream(id, tag, requiredSize, asContiguousBuffer)` pre-sizes and pre-routes;
  `GetStream(ReadOnlySpan<byte>)` copies in at admission for foreign payloads.
- `tag` is the provenance key for every event the stream later emits; the `Guid` id correlates one stream across
  its lifecycle events.
- Position-stateless concurrent reads exist as their own verbs: `SafeRead(buffer, ref long streamPosition)` and
  `SafeReadByte` thread the position explicitly, so concurrent readers never race the stream's internal cursor.
- The pool's evidence rides two rails from one source: CLR events on the manager for in-process folds, and an
  event-source writer for out-of-process tracing — the leak fold consumes the in-process rail; fleet tooling
  reads the same facts without code.
- `Capacity64` carries the long-range capacity beside the `int`-typed inherited `Capacity`; long-range consumers
  read the 64-bit member.
- Partial egress is owned: `WriteTo(stream, offset, count)` and `WriteTo(byte[], offset, count)` copy windows
  without slicing detours; `CopyTo` / `CopyToAsync` ride the standard seam.

## leak-evidence folds

- The manager's events are one fact stream, and leak detection is a fold over it, never a debugger session.
- The stream-lifecycle events: created (requested vs actual size), disposed (with lifetime), double-disposed
  (both dispose stacks), finalized (finalizer ran ⇒ a dispose was missed), converted-to-array (edge-copy audit),
  over-capacity.
- The buffer-economy events: block created, large buffer created (with `Pooled` flag — unpooled large rents are
  the oversize-payload signal), buffer discarded (`BufferType` × `DiscardReason`), stream length, usage report
  (four pool gauges).
- The conservation law gives the fold its predicate: per tag-window, created = disposed + live + finalized.
- A growing created−disposed gap with zero finalized is a held-stream leak — the tag names the lane; nonzero
  finalized is a missed-dispose leak with the allocation stack as evidence when stack capture is on.
- Discard-reason taxonomy routes the response: `EnoughFree` dominant ⇒ caps below workload — raise caps or
  accept churn; `TooLarge` dominant ⇒ payloads exceed `MaximumBufferSize` — raise the cap or split the payload.
- The two reasons demand opposite tuning moves; a fold counting discards without splitting by reason cannot
  steer.
- `GenerateCallStacks` and `ZeroOutBuffer` are diagnostics-row-only columns: per-stream stack capture allocates
  heavily and zeroing costs bandwidth. The production row keeps both false; the pool-policy record carries an
  explicit diagnostics variant that flips them — leak tracking lives only on the test and diagnostics row.
- Pool sizing arithmetic: small-pool free-byte cap ≈ block size × expected concurrent streams × mean blocks per
  stream; the large-pool cap pays off only when a contiguity-demanding consumer is on a hot path.
- Usage-report deltas trend residency against these targets; the gauges are the input to capacity reviews, not
  live alerting noise.

## pool reset law

- The page's pool-reset law, stated over the staging pools this lane owns: a pooled instance never carries
  request state across returns.
- The return path is one shape: reset fold → sanity predicate → return-or-discard.
- Reset columns: `AllocationMode.Clear` at rent for buffers; `ZeroOutBuffer` at cycle for stream blocks on the
  diagnostics row; `Clear()` for writer reuse.
- The sanity predicate is structural fitness: a stream past capacity, a buffer past the pool cap. A false
  predicate discards — receipted through the discard event — instead of returning a corrupt or oversize instance
  to the pool.
- Double-return is data, not a crash: the double-dispose event carries both stacks as a protocol-violation
  receipt; the responding posture poisons the offending lane's row until the violation is repaired — the
  sticky-failure posture the root boundary law already legislates, composed.
- The reset law generalizes across the axis: writer reuse rides `Clear()` (resets the window, keeps capacity);
  owner reuse does not exist — owners are rent-once return-once by construction, and "reuse" is a new rent, the
  cheaper and safer spelling because the pool IS the reuse mechanism.

## ParallelHelper behind measured claims

- The struct-generic command shape: `For<TAction>(Range | int,int [, in TAction] [, minimumActionsPerThread])
  where TAction : struct, IAction` with `Invoke(int)`.
- `For2D<TAction>` covers `Range`×`Range`, a rectangle, or explicit bounds with `IAction2D`; `ForEach<TItem,
  TAction>` covers `ReadOnlyMemory<TItem>` with `IInAction<TItem>` (readonly per-item ref) and ref-action
  variants for in-place mutation.
- The struct constraint devirtualizes and inlines the body; captured state is explicit fields on the action
  struct — the action IS a policy value, satisfying policy-as-values for parallel kernels.
- `minimumActionsPerThread` is the granularity gate: it floors per-thread work so small payloads never fan out.
- The floor and the degree both derive arithmetically from the settled parallelism budget record — this lane
  consumes the derived per-lane share and never declares thread counts; a literal degree beside the budget
  record is the foreclosed second-budget defect.
- Admission posture: the sequential fold is the default row; a parallel row exists only behind a measured claim
  for its (kernel, payload-class) pair.
- Fan-out cost — thread wake, cache contention, false sharing on adjacent partitions — makes small-payload
  parallelism a structural regression class, which is why the claim gate exists; the claim mechanics
  (fingerprints, staleness, demotion) are the dispatch lane's law, composed here as the gate this row stands
  behind.
- Ref-struct composition shape: views cannot be action-struct fields, so a plane-parallel kernel captures
  `Memory2D<T>` (or the owner) and projects `.Span` / `GetRowSpan` inside `Invoke` — the spelling that satisfies
  both the ref-struct rules and partition locality.
- Nesting rule: parallel rows do not nest. An inner kernel reached from a parallel row runs its sequential row
  unconditionally — encoded by deriving the inner cap as zero, never by runtime detection.

## divergent — allocation-class-axis

- Maximal law: one staging-class policy family — Stack / Scoped / Owned / Writer / Stream — where each row
  carries rent verb, return verb, escape legality (compile-enforced on the two ref-struct rows), reset column,
  size bound, and exit-seam set.
- A new payload kind is a row choice; a staging need fitting no row is a missing row in this family, never a new
  owner type beside it.
- Seam-typed exits are the axis's deep interaction: scoped pooled exits only into span-consuming kernels; owned
  pooled exits into tensor and marshal gates and async handoffs; writers exit into codec and serializer seams;
  streams exit into IO and wire substrates. An exit outside the row's seam set is the misuse the fold makes
  visible at review time.
- Rejected-form catalogue with what each forecloses: `new T[]` per request — forecloses pool economy and leak
  receipts; raw growable memory stream — forecloses block reuse, large-object discipline, and the event stream;
  GC-handle pinning for interop — pinned tensor allocation and `Memory2D.Pin` own that seam; segment-juggling
  helpers — the one `DangerousGetArray` bridge exists; borrow-shaped wrappers over `MemoryOwner` — ownership
  here is transfer-shaped, and borrow grammar belongs to the root capsule law.
- Cross-class interaction: the writer-then-owner handoff (`ArrayPoolBufferWriter` produces, `WrittenMemory` is
  consumed, the writer disposes after the consumer completes) is the one legal two-stage lifetime; copying
  written content into a fresh owner to "simplify" lifetimes is the double-allocation spelling the handoff
  deletes.

## divergent — stream-pool-leak-evidence

- The pool is a conserved resource with an audit identity, and the fold is the auditor: conservation per tag,
  discard taxonomy per reason, residency trend per gauge — three projections over one fact stream, all
  fold-derived, none requiring instrumentation beyond the manager's own events.
- The fact stream enters the same receipt collapse all compute evidence rides; the combination mechanics are
  settled rail law composed at the consumer.
- Tag discipline is what makes the fold actionable: tags name lanes — codec, model payload, export — not call
  sites. Per-call-site tag explosion destroys the conservation windows; one global tag destroys attribution. The
  tag set is a small closed vocabulary declared with the pool policy.
- Advanced cross-check: the converted-to-array event count against the receipt reasons recorded at `ToArray`
  call sites — a conversion event without a matching receipt reason is an unaudited edge copy, the exact defect
  the throw-on-copy option converts from audit finding into structural impossibility.
- Lifetime distribution from the disposed event's lifetime field is the early-warning fold: a fattening tail per
  tag predicts held-stream leaks before conservation breaks, because the gap shows in percentiles before it
  shows in counts.

## divergent — parallelhelper-claims

- Claim shape for a parallel row: kernel symbol, payload length class, budget share, measured speedup versus the
  sequential fold.
- Below the discovered length floor the sequential row wins by default; the floor is measured data stored as a
  claim — never a literal threshold in code — and re-validates when the budget record or hardware class changes.
- Demotion on a stale fingerprint is silent-to-correct: the kernel runs sequentially and the demotion receipt
  signals re-measurement; production never throws for being unproven.
- The oversubscription failure mode is foreclosed structurally, not detected: every parallel degree derives from
  one budget record and nested contexts derive zero — two budget-ignorant parallel layers multiplying threads
  cannot be expressed when degrees only come from the derivation.
- False sharing is the quiet claim-killer: partitioning columns of a row-major plane puts adjacent writes on one
  cache line across threads. The rows-then-`GetRowSpan` spelling partitions on the pitch axis and is the reason
  the measured claim holds.
- A claim measured on the row spelling does not transfer to a column spelling — the claim's kernel symbol pins
  the partition shape too, or the claim certifies the wrong kernel.
