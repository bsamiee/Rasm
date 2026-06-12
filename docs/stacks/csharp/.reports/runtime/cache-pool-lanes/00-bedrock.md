# cache-pool-lanes — bedrock

## hybrid cache surface

- The read-through entrypoint is one modality-polymorphic family: the primary overload carries `TState` so the factory is a static lambda over explicit state.
  - Zero closure capture on the hot path.
- The convenience overloads delegate into the `TState` form through a cached identity wrapper.
  - The `TState` form is the only real code path, so depth-of-overload never means duplicated machinery.
- Key overloads accept `string`, `ReadOnlySpan<char>`, and an interpolated-string handler.
- The span and handler key paths probe the cache before materializing a string.
  - Composed keys never allocate on hits, so key derivation can be interpolation-shaped without a hit-path cost.
- `SetAsync` is not a separate write path: it rides the same stampede machinery with the local-read and distributed-read flags forced.
- A set is therefore a read-through whose factory is the constant value.
  - One machine serves get, get-or-create, and set, and write-path behavior inherits every read-path guard.
- Batch removal verbs (`RemoveAsync(keys)`, `RemoveByTagAsync(tags)`) are folds over the singular forms — plural intake without plural machinery.
- The flags algebra closes the topology axis: effective flags = per-call entry flags OR options-level hard flags, falling back to options-level defaults.
- Three precedence layers resolve through one OR-fold — hard flags are non-negotiable profile law, defaults are profile posture, per-call flags are situational tightening only.
- Eight flag bits factor as three concerns × two directions plus two specials: local-cache read/write, distributed-cache read/write (with combined disable values per pair), `DisableUnderlyingData`, and per-entry `DisableCompression`.
- `DisableUnderlyingData` turns get-or-create into a pure probe: a miss returns the default value instead of invoking the factory.
  - The read-without-work modality, free from the same entrypoint.
- Every cache topology — L1-only, L2-only, write-through, read-probe, full bypass.
  - Is one flags value; no topology ever appears as a code branch, and a topology change is a policy-row edit.
- Entry options are two durations plus flags: `Expiration` (L2 and overall) and `LocalCacheExpiration` (L1).
- Effective L1 lifetime is clamped to the minimum of the two durations — L1 can never outlive L2 truth by configuration accident.
- Unset entry options inherit `DefaultEntryOptions`; absent those, both expirations default to five minutes.
- The local default derives from the overall default — setting only `Expiration` moves both, which is the intended single-knob case.
- Cache-wide guards are option values: maximum payload (one-MiB default), maximum key length (1024 default), `DisableCompression`, `ReportTagMetrics`.
- Key hygiene is structural: keys containing control characters are rejected outright — wire-unsafe keys cannot enter either layer.
- Oversize and malformed entries degrade to factory-direct execution rather than faulting the call.
  - Guard breaches cost performance, never correctness.
- L1 is the process's registered memory cache instance, resolved from composition.
  - Hybrid entries share that cache's size limits and compaction policy with every other memory-cache consumer, so L1 sizing is a process-level decision, not a hybrid-local one.
- L2 is the distributed cache when one is registered; `DistributedCacheServiceKey` selects it through keyed resolution.
  - The L2 binding is itself a keyed composition fact.
- Multiple cache profiles — different L2 backends, serializers, guards, defaults.
  - Are keyed registrations of the whole cache (`AddKeyedHybridCache`), one profile per key; consumers receive their profile by key and never see topology.
- Buffer-capable L2 implementations are feature-detected and driven through the buffer protocol.
  - No per-operation byte-array allocation against modern backends; the optimization is composition-derived, not call-site code.
- Degenerate-pair elision: when the registered L2 is the memory-backed distributed shim and L1 is the standard memory cache, the L2 is silently discarded.
- Consequence: a composition that "tests the L2 path" against the memory shim exercises no L2 path at all.
  - L2 behavior is provable only against a real out-of-process backend.
- Serializer seam: string and byte-array payloads have inbuilt serializers; other types resolve per-type serializer registrations first, then serializer factories, with a JSON fallback factory closing the chain.
- Payload codec is composition policy per profile (`AddSerializer<T>`, `AddSerializer<T, TImpl>`, `AddSerializerFactory`); call sites never name a codec.
- L1 storage discriminates payload immutability: types proven immutable cache as shared instances, while mutable types cache as serialized payloads re-deserialized per read.
  - Consumers can never alias a mutable cached object — and mutable payload types pay per-read deserialization in L1, so immutable records are the performant payload shape.
- A write carries tags like any populate: `SetAsync` accepts the same tag set, so tag families attach at every entry-creation path.
  - There is no untagged back door into the cache.
- Combined flag values are the OR of their pair — disable-local is local-read|local-write, disable-distributed is distributed-read|distributed-write.
  - Flag composition is transparent bit algebra; custom postures compose from the primitives.
- When the effective flags disable both cache layers, the call short-circuits to direct factory invocation without touching the flight machinery.
  - Full bypass costs nothing — a bypass profile is a safe instrument for A/B-ing cache value, not a degraded mode.

## stampede and tags

- Single-flight is keyed by (key, flags): concurrent callers on one key join one in-flight operation.
- The same key under different flags is a different flight — mixed-flag access to one key intentionally does not share work; key discipline implies flag discipline.
- The probe corollary: probe-flagged readers form their own flight class and never trigger or join factory work.
  - A two-tier read strategy (probe-first fast path, populate path converging to one factory run) falls out of the flags algebra with no code.
- Joiner cancellation is reference-counted: the factory invocation is cancelable only when every joiner is cancelable, and it aborts only when all joiners have canceled.
- One impatient caller detaches without killing the shared computation — caller-local timeouts are safe against shared work by construction.
- Receipt consequence: factory-invocation counts undercount request counts by design; cache observability must count joins, not factory runs.
- Flight scope is the cache instance, hence the process: single-flight dedupes within one process only.
  - A fleet-wide stampede is bounded to one factory run per process, and the first process to populate L2 serves the others' flights through the L2 read inside the flight — cross-process convergence is an L2 property, not a flight property.
- Per-entry receipt streams are the wrong instrument here — the cache is the receipt-free fast path; durable evidence belongs to the store the factory reads.
- Tag invalidation is timestamp shadowing, never eviction: entries carry a creation timestamp plus their tag set.
- Validity is creation-newer-than every tag's invalidation timestamp and the wildcard epoch — invalidation is a comparison, not a scan.
- Removing by tag records now() into the tag's slot locally at once and persists it through L2.
  - The invalidation is suite-wide without any key enumeration.
- Tag timestamps are lazily prefetched from L2 per tag on first use — cross-process invalidation costs one L2 read per tag per process, amortized.
- The wildcard tag `*` is the global epoch: removing by it invalidates everything older than now across both layers, and clears the local tag table when no L2 exists.
- Epoch bump is the cache-flush verb; nothing ever enumerates keys, so flush cost is constant regardless of population.
- Shadowed entries are not reclaimed — L1 memory is freed by expiration, not invalidation; memory sizing must assume full retention of invalidated entries until natural expiry.
- Tags are validity predicates, not indexes: there is no list-entries-by-tag, and modeling a lookup index as tags is the rejected use.
- Validity checking folds over the entry's tags on every hit — tags are paid for per read, the structural argument for a small closed tag vocabulary over fine-grained tagging.
- All validity timestamps come from the injected time abstraction — tag shadowing, expiry, and stampede timing are deterministic under the test clock; cache temporal behavior is provable without sleeps.
- Invalidation vocabulary is exactly three verbs — key removal, tag shadow, wildcard epoch. Any richer invalidation grammar (prefix scans, predicates) signals the data belongs in a store, not a cache.

## object pools

- The default pool is a CAS fast-slot plus a concurrent queue holding capacity-minus-one items: `Get` tries the slot, then the queue, then creates.
- `Get` never blocks and never throttles — pool exhaustion is invisible; the caller always receives an instance.
- Pools bound retention, not allocation: a burst beyond capacity allocates freely and the excess is dropped at return.
- A pool is a steady-state allocation damper, never a concurrency limiter.
  - Limiting belongs to the throughput owner, and a pool pressed into limiting duty fails silently by allocating.
- The return path is a single predicate: the policy's return decision vetoes reuse, and a vetoed object is dropped silently.
  - Disposed under the disposable pool variant.
- There is no error channel on return; the predicate is the entire protocol, which is why the predicate must carry the whole sanity policy.
- The default policy routes the return decision through `IResettable.TryReset`.
  - Reset is the one advice point that is simultaneously cleanup and sanity gate.
- Policy dispatch is two-layer: the pool consults the policy's return decision, and the default policy consults the resettable contract.
  - Custom policies replace the outer layer, resettable types extend the inner one, and the two compose without either knowing the other.
- A reset that detects corrupted, oversized, or stale state returns false and the instance leaves circulation.
  - Discard-on-false is the leak-safe default.
- "Repair and return true anyway" is the rejected form: it launders bad state back into circulation; the predicate's falsity is evidence, and evidence is not negotiable at the reset seam.
- Provider dispatch is type-driven: the default provider returns the disposable-aware pool whenever the element type is disposable.
  - Rejected and overflow returns are disposed, not leaked.
- The static pool factory routes through a fresh default provider, so disposal dispatch holds on every construction path.
  - There is no construction route that loses disposal.
- Default retained capacity is twice the processor count — a per-core steady-state assumption, not a workload cap; it is a provider policy value sized per lane, never accepted implicitly for high-fan-out lanes.
- The text-buffer policy is the canonical buffer-pool row: initial capacity 100, retention ceiling 4096.
- Buffer returns clear the builder; over-ceiling builders are discarded.
  - The discard-on-oversize rule prevents one pathological lease from permanently inflating pool memory.
- Every buffer-shaped pool policy copies the two-knob shape — construction size, retention ceiling.
  - The ceiling is the knob that matters: it is the pool's memory bound under adversarial lease sizes.
- Leak posture: the default pools track nothing about outstanding leases.
  - A leased-and-never-returned instance is ordinary garbage, invisible to the pool.
- Only the leak-tracking provider (finalizer-based) detects unreturned leases, and it is a diagnostic-posture composition swap, never a production resident.
- Production leak discipline is therefore structural: lease scopes own the return in a bracket shape so the return is unconditional on every exit path.
  - The pool's API cannot enforce this; the lease idiom must.
- Pooled instances never carry per-lease ambient state across returns; the reset predicate enforces that invariant mechanically.
  - A pooled type without a meaningful reset is a type that should not be pooled.
- Disposable-pool teardown is total: disposing the pool drains and disposes every retained instance, including the fast slot.
- After pool disposal, a lease attempt throws the disposed exception and a racing return disposes the incoming instance instead of retaining it.
  - Returns racing teardown cannot leak — the teardown protocol needs no external coordination with in-flight leases.
- Pool teardown rides provider disposal at host shutdown, which lands after the last drain band.
  - A pooled resource whose teardown must precede a drain band (a native handle a flush still needs) registers its own stop participation; relying on provider disposal ordering for mid-drain needs is the rejected form.
- Specialized pools are provider extensions, not new pool types: the text-buffer pool ships as an extension over the provider.
  - A new specialized pool is one extension method plus a policy row — the pool taxonomy never grows a parallel class hierarchy.

## divergent — hybridcache-depth

- The one cache law: every cached read in the system is the get-or-create entrypoint over (derived key, explicit state, static factory, options row, tags) against a profile.
  - Key from one derivation algebra with no free literals, options from a frozen policy table, tags naming invalidation families.
- There is no second cache idiom: get-then-set pairs are stampede-unsafe by construction; caller-side population locks duplicate the flight machinery; hand-rolled negative caching is a probe-flagged row with short expiry.
  - Each rejected form has a one-row spelling inside the law.
- Profile rows absorb topology growth: a profile = (service key, L2 selection, serializer set, guards, default entry options, hard flags).
- Adding a distributed L2, splitting hot/cold profiles, or an L1-only embedded posture lands as profile rows and flag values with zero call-site change.
  - Call sites name profiles and policy rows only.
- Tag taxonomy law: tags form a small closed vocabulary derived from domain identities — entity-family tags, suite epoch, per-tenant tags.
  - Declared beside the key derivation so key and tag algebras cannot drift apart.
- The L1/L2/flags/tags machinery composes into deployment-shape freedom: the same call sites serve a single-process embedded posture (L1-only profile), a multi-process suite (shared L2, tag invalidation as the cross-process coherence channel), and a read-replica posture (probe-flagged profiles against a populate-owning process) — coherence strategy is profile data.
- Stampede single-flight plus tag shadowing yields a free consistency contract: after a tag invalidation, at most one factory run per key repopulates, and every consumer observes either the pre-invalidation value or the repopulated one.
  - Never a torn intermediate. Stating this contract once beats re-deriving it at every consumer.
- Outcome evidence rides the built-in tag-metrics switch plus join-versus-run accounting at the profile level.
  - Cache health is a profile-level projection, never per-entry bookkeeping.

## divergent — objectpool-reset-law

- Pool policy unifies as frozen rows: lane → (create, reset predicate, retained capacity, oversize ceiling, disposal class).
- Every pooled lane in a process is one row in one table; the row is injected, never constructed at use sites.
- The table is the complete inventory of mutable-reuse surfaces — auditable in one read, which matters because pooled mutables are the highest-risk state class in the process: shared, mutable, and identity-reused by design.
- The reset predicate is compositional: ceiling checks, dirty-state detection, and invariant verification compose as predicate conjunction inside one reset.
  - The pool sees one boolean, the row carries the policy stack.
- Sanity gates that would otherwise scatter across lease sites collapse into the row — the reset predicate is where lease-site defensive code goes to die.
- Pool-versus-cache is one retention law with two value classes: pools retain identity-reused mutables under a reset predicate; the cache retains value-reused immutables under validity predicates (expiry, tag shadow).
- Choosing between them is choosing the value class — anything mutable-after-publication in a cache or identity-tracked in a pool has chosen wrong.
- Both lanes are siblings of one runtime concern: bounding rebuild cost without owning truth. Neither surface is ever a system of record; both lose content arbitrarily by design; any correctness dependency on retention is the disqualifying smell for both.
- Growth cases land as rows: a new pooled lane is one policy row; a new buffer class is the two-knob buffer row; a diagnostic leak hunt is the leak-tracking provider swapped in the diagnostic posture's composition row. No growth case touches consumption code.
- The reset law generalizes to every recycle-shaped resource the runtime owns: scratch buffers, reusable command objects, connection-adjacent state.
  - If it returns to a shared store, it passes a reset predicate first, and the predicate's false branch discards. One law, table-many instances.
