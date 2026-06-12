# parallelism-budget — bedrock

## the unified budget record

- One frozen budget record per process owns every concurrency axis: cpu workers, io degree, partition count, permit limit, queue limit, lane capacities, batch size, soft and hard drain budgets, heartbeat period, staleness threshold.
- Every degree, permit, capacity, and window in the process derives arithmetically from the record.
- A literal degree at a call site is the defect — it makes the concurrency posture unrecoverable from declarations and unrebalanceable without a code hunt.
- Platform defaults the record must pin rather than inherit: `Parallel.ForEachAsync` and `ForAsync` default to `Environment.ProcessorCount` when unconfigured.
- A negative declared degree on the async loop family also resolves to processor count — negative is "default", never "unbounded".
- PLINQ defaults to `min(ProcessorCount, 512)` and hard-caps any declared degree at 512.
- Two subsystems silently defaulting to full processor count on one machine is 2× oversubscription that no declaration shows — the reason the record pins both explicitly.
- Cross-axis inequalities live in the record and are checked at construction, not discovered in production.
- Inequality: lane capacity ≥ workers × batch size, or workers starve on hand-off.
- Inequality: permit limit ≥ workers wherever permits gate lane writes, or the limiter, not the budget, silently sets the real degree.
- Inequality: staleness threshold > soft drain + hard drain + heartbeat period — the cross-process lease inequality, derived below.
- A budget record whose fields cannot prove its own inequalities is a bag of numbers, not a budget.
- One waiting room per hop: a limiter queue (`QueueLimit`) in front of a bounded lane is two buffers with two policies on one hop — pressure evidence splits, drain order turns ambiguous, latency bounds become the sum of two opaque waits; the record assigns each hop exactly one waiting axis and zeroes the other.
- Cpu workers and io degree never share one number: cpu degree is sized by local cores, io degree by the remote system's capacity — collapsing them couples a remote slowdown to local starvation and a local burst to remote overload.
- Partition count ≥ workers, always: fewer partitions than workers idles the surplus; range partitioning serves cache locality on uniform work, chunk partitioning serves load balance on skewed work — the partition row carries which.
- A `QueueLimit` of zero degrades the awaited acquire to fail-fast — the declared no-waiting row of the limiter family, chosen when the caller owns its own retry cadence and a queue would hide it.

## parallel admission law

- `Parallel.ForEachAsync` is the default parallel iteration for async and IO-bound work.
- The overload family spans `IEnumerable<T>` and `IAsyncEnumerable<T>`, each with bare, token, and `ParallelOptions` forms; the body is `Func<T, CancellationToken, ValueTask>`.
- The token handed to the body is the loop's internal token: it trips when any sibling iteration throws or the external token fires — cooperative stop is structural, not disciplinary.
- The first exception stops further launches; all recorded exceptions aggregate on the returned task — no failure is masked by the first.
- Source enumeration is serialized under an internal lock: sources need not be thread-safe, and exactly one move-next runs at a time.
- Consequence of the serialized pull: a slow producer serializes the whole loop — the decoupled spelling is producer → bounded lane → `ForEachAsync` over the lane's async stream.
- `Parallel.ForAsync<T>` spans `IBinaryInteger<T>`: index-range parallelism with no materialized range and the same token and exception structure — the index-kernel row of the same law.
- `ParallelOptions` is the shared knob record for the family: { `MaxDegreeOfParallelism` (default -1 → processor count for the async family), `TaskScheduler`, `CancellationToken` }; the scheduler row routes workers onto an owned scheduler, isolating the loop's affinity and its share of the pool.
- The budget therefore pins three things per loop, not one: degree, scheduler, and token — the option record exists so all three travel together as one policy value.
- PLINQ earns admission only for CPU-bound, side-effect-free, associative computation.
- The required spelling is total: `AsParallel().WithDegreeOfParallelism(budget).WithCancellation(token)` plus an explicit merge decision.
- A bare `AsParallel()` is the rejected form on every axis at once — unpinned degree, unstoppable, unmeasured merge.
- Merge policy is a declared row: `ParallelMergeOptions.NotBuffered` when a consumer streams results; `FullyBuffered` for terminal sinks; `ForAll` for sink-without-merge — no merge stage exists at all.
- `AsOrdered` buys order with a reordering stage's latency and memory — admitted only when order is contractual.
- `WithExecutionMode(ParallelExecutionMode.ForceParallelism)` overrides the cost model's sequential fallback when small operator shapes hide expensive bodies.
- `ForAll` consumes results on the worker threads themselves — its sink must be thread-safe by construction, and the canonical thread-safe sink is a lane write, which re-enters the receipted world.
- Order is scoped, not global: `AsOrdered` opens an ordered section and `AsUnordered` closes it — confining the ordered span to the operators that contractually need it confines the reordering buffer's cost window.
- `AsSequential()` exits parallel space mid-query: the canonical shape is sequential head, parallel middle, sequential tail — parallelizing cheap projection tails buys merge cost for nothing.
- Partitioning is a declared input shape: `Partitioner.Create(0, n, rangeSize)` sizes index ranges to cache or work granularity.
- `EnumerablePartitionerOptions.NoBuffering` serves slow or latency-sensitive producers — default chunk buffering trades arrival latency for amortized throughput and holds items invisible to the consumer.
- Synchronous `Parallel.For`/`ForEach` survive only inside measured CPU kernels under the named kernel exemption; the async pair is the default everywhere else.
- Task-group fan-out is admissible only when structurally bounded by the budget — every spawn rides a derived degree; unbounded fan-out via task spawning per item is the canonical rejected form.
- Exception convergence: `ForEachAsync` faults with the aggregate of all iteration failures; PLINQ throws an aggregate at terminal operators; both convert at exactly one boundary into the typed rail — per-iteration catch blocks that swallow or re-raise piecemeal forfeit the aggregation the primitives already perform.

## rate limiting: limiter rows

- Four limiters are four policy rows over one acquisition contract.
- `ConcurrencyLimiter` { `PermitLimit`, `QueueLimit`, `QueueProcessingOrder` }: the in-flight gate; permits return on lease disposal.
- The token-bucket pair prices two distinct quantities in one row: sustained rate = `TokensPerPeriod` ÷ `ReplenishmentPeriod`, burst ceiling = `TokenLimit` — declaring one without deriving the other is half a policy.
- Acquisition verb is itself a policy: `AttemptAcquire` at the edge is shed-with-receipt (the verdict is immediate and the caller owns retry); `AcquireAsync` is queue-and-wait (the limiter owns ordering) — choosing the verb chooses who holds the waiting room.
- `TokenBucketRateLimiter` { `TokenLimit`, `TokensPerPeriod`, `ReplenishmentPeriod`, `QueueLimit`, order, `AutoReplenishment` }: sustained rate with burst capacity up to the bucket.
- `FixedWindowRateLimiter` { `PermitLimit`, `Window`, `QueueLimit`, order, `AutoReplenishment` }: quota aligned to window boundaries.
- `SlidingWindowRateLimiter` { `PermitLimit`, `Window`, `SegmentsPerWindow`, `QueueLimit`, order, `AutoReplenishment` }: quota without the boundary-burst artifact — a fixed window admits up to 2× the rate across a boundary; segments amortize it.
- `QueueProcessingOrder` defaults to `OldestFirst` on all four; `AutoReplenishment` defaults to `true` on the three replenishing rows.
- The replenishing rows share the `ReplenishingRateLimiter` base: `ReplenishmentPeriod`, `IsAutoReplenishing`, and `TryReplenish` — manual replenishment is a first-class mode for callers that own cadence.

## rate limiting: lease shape and queue semantics

- The lease is a structural solve-path guard: the surface offers no blocking acquire, so thread-blocking on permits is unrepresentable in the type system.
- `AttemptAcquire` is synchronous fail-fast; `AcquireAsync` is an awaited queue — the only two acquisition postures that exist.
- `RateLimitLease.IsAcquired` carries the verdict; disposal is the release on the concurrency row — the handle is the permit.
- Permit leaks are object leaks: a widening gap between `TotalSuccessfulLeases` and disposals is the detection signal.
- Zero-permit calls are first-class probes, not degenerate requests: `AttemptAcquire(0)` succeeds iff permits remain — the exhaustion probe; `AcquireAsync(0)` parks until the next replenishment edge without consuming — a structural wait-for-capacity-edge; both delete polling loops over statistics.
- A `permitCount` above `PermitLimit` throws `ArgumentOutOfRangeException` synchronously on every limiter — an unsatisfiable request is a construction defect, never a queue entry.
- Under `OldestFirst`, freed permits are reserved for the queue head: fresh arrivals cannot barge even when permits are momentarily free.
- Under `OldestFirst`, queue overflow fails the incoming request — the newcomer is the loser.
- Under `NewestFirst`, fresh arrivals take available permits immediately — barging is the policy, not a bug.
- Under `NewestFirst`, queue overflow evicts the oldest queued request — failing a lease that was already being awaited; the failed lease is the staleness receipt.
- Decision rule: interactive intent → `NewestFirst` — latest intent wins and stale intents receipt out; fairness-bound throughput → `OldestFirst`.
- Failed leases carry typed metadata: `MetadataName.RetryAfter` (`TimeSpan`, from replenishing rows) and `MetadataName.ReasonPhrase`.
- `TryGetMetadata(MetadataName<T>)` extracts typed values; `GetAllMetadata` enumerates; `MetadataName.Create<T>(name)` mints custom typed names for bespoke lease evidence.
- `RetryAfter` is the bridge from limiter verdicts into schedule-driven retry on the effect rail: the limiter names the earliest useful retry instant.
- A fixed backoff beside a `RetryAfter`-bearing lease re-derives what the lease already states — the lease's instant wins.
- `GetStatistics()` returns { `CurrentAvailablePermits`, `CurrentQueuedCount`, `TotalSuccessfulLeases`, `TotalFailedLeases` } — the budget's live gauges; the statistics type is an init-only snapshot record, never a live view.
- Probe noise trap: a successful zero-permit probe increments `TotalSuccessfulLeases` — gauge consumers either subtract probe counts or accept that the success counter measures verdicts, not permits consumed.
- Derived latency gauge: expected queue wait ≈ queued permit count ÷ replenish rate — the two statistics fields plus the row's replenishment arithmetic price the queue without instrumenting it.
- Concurrency-lease disposal services queued waiters in queue order as permits return — release latency is the waiter's wake latency, with no polling anywhere.
- `IdleDuration` is non-null only when the limiter is fully idle — the idle-eviction signal partition managers key on.
- Disposal of any limiter completes all queued acquisitions with failed leases.
- Drain integration follows from that: limiters dispose in the forced phase, after lanes complete, so cooperative-phase work never observes synthetic limiter failures.

## rate limiting: partitioned and chained composition

- `PartitionedRateLimiter.Create(partitioner, comparer?)` evaluates the partitioner on every acquisition; the per-key limiter is created once and cached against the partition key.
- The partitioner function is therefore on the hot path of every acquire — it stays allocation-light and never touches IO.
- An internal heartbeat at 100 ms cadence both replenishes every replenishing partition limiter and disposes partitions idle beyond 10 s.
- The heartbeat consequence: replenishment granularity inside a partitioned limiter is the heartbeat's cadence, regardless of a row's declared replenishment period shorter than it.
- The typed partition factories — `RateLimitPartition.GetConcurrencyLimiter`, `GetTokenBucketLimiter`, `GetFixedWindowLimiter`, `GetSlidingWindowLimiter` — force `AutoReplenishment = false` so all partitions share the one heartbeat.
- A custom `RateLimitPartition.Get` whose factory leaves auto-replenishment on pays one timer per partition — the silent per-key timer leak the typed factories exist to prevent.
- `GetNoLimiter(key)` is the declared exempt row: exemption lives in the partition table, not in call-site branching.
- The exempt row reports a null `IdleDuration` permanently — exempt partitions are never idle-evicted and reside for the limiter's lifetime; a high-cardinality exempt key space is a permanent-resident leak by construction.
- `RateLimitPartition.Get(key, factory)` is the open row for custom limiter types; the factory must return a fresh limiter per call — returning a shared instance double-disposes it on idle eviction.
- Partition disposal consequence: a partition's limiter can be disposed underneath a holder after idle — never cache the inner limiter or its statistics reference.
- The safe read posture is through the partitioned surface per resource, which re-resolves the partition on every call.
- `RateLimiter.CreateChained` and `PartitionedRateLimiter.CreateChained` acquire in declared order.
- On a later failure or throw inside a chain, every earlier acquired lease is disposed and the failure propagates — partial holds cannot leak.
- The combined lease aggregates metadata first-wins per name.
- Combined statistics report the minimum available permits, the innermost limiter's successful-lease count, and aggregates for the rest — the chain's gauges are a fold, not a list.
- Chain order law: coarsest first, finest last — a global rejection then costs zero per-key bookkeeping, and the global row's `RetryAfter` dominates correctly.
- `WithTranslatedKey(keyAdapter, leaveOpen)` re-keys an existing partitioned limiter at a boundary: one limiter, two key vocabularies, no second permit pool; the adapter runs on every acquisition path concurrently and must be thread-safe; `leaveOpen` decides whether disposing the translation disposes the inner limiter.
- The translation applies to every verb — attempt, acquire, and statistics all route through the adapted key — so per-resource gauges remain truthful across the re-keyed surface.

## single-flight coalesce gate

- Law: one active-owner cell per work identity; the first entrant publishes its task handle into the cell by compare-and-set and owns execution.
- A concurrent second entrant loses the CAS, awaits the published task, and receives a coalesced receipt carrying the owner's identity and a shared marker — duplicate work becomes shared evidence instead of duplicate spend.
- The cell carries an interlocked dedup counter alongside the handle; the receipt reports how many entrants one execution served — the gate's effectiveness gauge, read from the receipt stream rather than inferred.
- The owner clears the cell on its completion edge by compare-exchanging its own handle out — never a blind clear: a successor may already have registered a new flight, and a blind clear tears the successor's publication.
- Cancellation split: the owner runs under its own token; coalesced waiters bound their waits individually around the shared task — a waiter's cancellation abandons its wait, never the work; owner cancellation fails all waiters with one terminal evidence.
- Linking waiter tokens into the owner's execution is the rejected form: it hands every coalesced caller a kill switch over shared work.
- Placement: the gate sits between intent and `AcquireAsync` — a coalesced entrant consumes zero permits and zero lane capacity; single-flight is budget protection before budget spend.
- Generalization: keyed single-flight is the partitioned family's degenerate row — a keyed cache of active task handles with the same per-key creation and the same idle-eviction need, holding a receipt where the limiter holds a lease.
- One keyed gate per identity vocabulary; per-call-site ad-hoc gates fragment the identity space and coalesce nothing.
- The work identity is value-shaped, never call-site-shaped: the key derives from the canonical request value (its admitted identity), so two textually distant callers issuing one logical request coalesce — keys minted from caller context defeat the gate exactly where it pays.

## cross-process lease ownership

- The lease record is { owner id, claim stamp, heartbeat stamp } at a shared medium.
- Owner identity is process-unique across restarts — process id alone recycles; the id pairs the process identifier with a boot-instant stamp so a reborn process never satisfies its predecessor's lease.
- Claim is the medium's own atomic create-if-absent — atomic file create, unique-key insert — never read-then-write; claim atomicity is inherited from the medium or it does not exist.
- Release-on-drain is a cooperative-phase step ordered before the owner's lanes force-drain.
- The ordering guarantees successors observe release while the owner is still alive to complete it.
- A release scheduled after forced teardown races process death and reproduces exactly the stale-lease window the protocol exists to prevent.
- Crash reclaim: a contender steals only when observed heartbeat staleness exceeds the staleness threshold.
- Staleness is observed, never assumed: the contender reads the heartbeat stamp from the medium at decision time, and the read and the steal bracket one adjudication.
- The threshold derives from the budget record under staleness > soft drain + hard drain + heartbeat period — a draining-but-alive owner is structurally unstealable because it either heartbeats or releases before the threshold can elapse.
- Across hosts the inequality gains an additive clock-skew bound: staleness > drains + heartbeat + skew; the stamp-envelope mechanics that measure skew are owned elsewhere — this lane owns only the term's presence in the inequality.
- The steal is itself an atomic conditional swap on the observed stale stamp value — a CAS at the medium — so two contenders cannot both reclaim; the winner emits a takeover receipt naming the previous owner and the observed staleness.
- Heartbeat cadence is a budget row executed on the process's owned schedule; this lane owns the stamp algebra and the inequality, not the cadence machinery.
- The in-process token-gated cell and the cross-process lease are one lifecycle law at two altitudes: the lease adds only staleness adjudication, because absent shared memory, time is the only liveness evidence.

## divergent

- Budget-derivation closure: the record's derivation table is total — cpu workers → iteration and query degrees; io degree → outbound hop permits; partitions → partitioner range size; permit limit → limiter rows; lane capacities → channel rows; batch size → flush rows; drain budgets → two-phase deadlines; heartbeat and staleness → lease rows.
- Each consumer axis names its formula, so rebalancing is one record edit and zero call-site edits.
- Rejected row: per-call-site literal degrees — unrebalanceable by construction.
- Rejected row: a semaphore as limiter — no queue order, no eviction receipts, no metadata, no statistics: four evidence channels deleted at once.
- Rejected row: timer-per-partition replenishment — the heartbeat owns cadence.
- Rejected row: double waiting rooms per hop — two buffers, two policies, no latency bound.
- Permit-weight pricing: `AcquireAsync(weight)` prices heterogeneous work on one limiter — weight in cost units, cumulative queued weight counted against `QueueLimit`.
- One weighted limiter replaces N per-class limiters whose relative rates would otherwise be hand-derived; the weight function is a policy value beside the row.
- The over-limit throw makes impossible weights a construction failure rather than a permanent queue entry.
- Admission chooser closure (work shape → primitive): async-io unordered → `ForEachAsync` over a lane; cpu associative aggregate → PLINQ in its full spelling; cpu index kernel → `ForAsync` or the kernel exemption; ordered parallel transform → the block row admitted at the lane page; identical concurrent intents → single-flight; cross-process exclusivity → lease.
- The chooser is total over work shapes — a workload matching no row is a missing budget axis, not a bespoke primitive.
- Verdict-family unification: a failed lease, a full-lane refusal, and a coalesced receipt are one closed verdict family — { admitted, queued, coalesced, shed(retry-after), evicted } — each carrying its evidence (`RetryAfter`, drop receipt, owner id); backoff, telemetry, and drain logic dispatch on verdict shape instead of on which subsystem said no, and the budget record is what closes the family.
- The budget is observable, and observation closes the loop: limiter statistics and lane receipts fold into periodic budget evidence, and sustained queue growth over a window is the undersized-axis signal — rebalancing is then one record edit, executed on the process's owned cadence; a budget without its evidence fold is set once and wrong forever.
- Lease scoping composes the resource law: a permit lease is acquire/release with a typed handle, so it rides the same bracket grammar every owned resource rides — the lease is never stored in a field and released by convention; the bracket's release is the disposal.
