# channel-lanes — bedrock

## lane policy algebra

- A lane is one declared policy row over seven axes: capacity, full-mode, reader arity, writer arity, continuation inlining, priority comparer, drop receipt.
- One frozen row table per process; producers reference rows by name.
- Inline `new BoundedChannelOptions` at a call site is the rejected form: it makes the backpressure decision unrecoverable from declarations.
- `BoundedChannelOptions` validates `Capacity >= 0` and `FullMode` within the four-value range at assignment; an invalid row fails at construction, never at first write.
- `Channel<TWrite, TRead>` is the two-type-parameter base; `Channel<T>` collapses both; implicit conversions project a channel to its reader or writer, so a lane hands out exactly the half it grants.
- The single-type options classes split capability: `UnboundedChannelOptions` carries only the shared axes; `UnboundedPrioritizedChannelOptions<T>` adds `Comparer`; capacity and full-mode exist only on the bounded row.
- Every factory yields a symmetric channel (`Channel<T>` derives `Channel<T, T>`); the asymmetric `Channel<TWrite, TRead>` base is the extension seam for adapter channels whose write and read types differ — no built-in implementation uses it.
- Contention models differ by row: the plain unbounded lane rides a lock-free concurrent queue on the write path; every bounded-lane operation serializes on one internal monitor — a bounded lane is a serialization point, and its throughput ceiling is lock hand-off rate.
- Scale-out law from that ceiling: a hot bounded lane shards into N lanes selected by key hash, with the shard count derived from the parallelism budget — widening one lane's capacity raises burst absorption, never throughput.

## full-mode semantics: the mode × operation matrix

- The bare `CreateBounded(int)` factory is shorthand for the `Wait` row with asynchronous continuations — drop modes, inlining, and receipts all require the options form, so any receipted lane is necessarily declared through options.
- The bare `CreateUnbounded()` likewise defaults to asynchronous continuations; only the options form reaches the single-consumer fast path.
- `Wait` + `TryWrite` on full: returns `false` — the only mode in which `TryWrite` can fail on a live channel.
- `Wait` + `WriteAsync` on full: parks the writer in a blocked-writer queue; completion of the write proves buffer admission.
- `Wait` + `WaitToWriteAsync`: completes only when space frees — the genuine writer-side backpressure signal.
- `DropWrite` on full: the incoming item is discarded; the receipt callback receives the incoming item; the write reports success.
- `DropOldest` on full: the queue head — the oldest buffered item — is evicted to admit the new item; the receipt receives the evicted item.
- `DropNewest` on full: the queue tail — the most recently buffered item, never the incoming one — is evicted; the receipt receives the evicted item.
- Under all three drop modes `TryWrite` returns `true` on a live channel, and `WaitToWriteAsync` is immediately `true`: drop modes structurally delete writer-side backpressure.
- Consequence law: under drop modes, write success is not delivery evidence; the receipt is the only loss evidence.
- A drop-mode lane without an `itemDropped` callback is unreceipted loss and a rail rejection.
- The `itemDropped` callback runs synchronously on the writing thread, outside the channel's internal lock.
- Receipt-callback discipline: fold into a fact stream (counter, receipt cell); never log, allocate heavily, or block — the callback sits on every full-capacity write.
- Receipts fire only on capacity eviction; a write refused because the channel completed produces no receipt — post-completion refusals belong to drain residue accounting, and conflating the two double-counts loss.
- The default `WriteAsync` is `TryWrite` first, then a `WaitToWriteAsync`/`TryWrite` loop; when the channel completes mid-wait it throws `ChannelClosedException` — a writer that must not throw on shutdown polls `WaitToWriteAsync` and folds `false` to its own stop signal.

## rendezvous and hand-off

- Capacity zero is a rendezvous lane: no buffer exists; `TryWrite` succeeds only when a reader is already parked — direct hand-off.
- Both the `int`-capacity and options factories accept zero; negative capacity throws.
- At capacity zero all three drop modes collapse to one behavior — the only discriminant is mode ≠ `Wait`.
- A rendezvous unmatched write under any drop mode folds to drop-with-receipt and reports success; `Wait` parks the writer until a reader arrives.
- Rendezvous rows are the handshake primitive: completion of the write is proof of consumption, deleting completion-source ping-pong patterns.
- Direct hand-off is not rendezvous-only: a buffered bounded lane hands the item straight to a parked reader when the buffer is empty, bypassing the queue entirely.
- Sizing consequence: low-load latency is hand-off latency, not queue latency — a buffer buys burst absorption only, never latency; sizing capacity for latency is a category error.

## arity fast paths and counting evidence

- `SingleReader` on an unbounded lane selects a dedicated single-consumer implementation with pooled read operations — the only arity declaration that changes the implementation class.
- `SingleWriter` is accepted and consumed by nothing: no implementation branches on it; it documents intent and buys zero throughput — a design justified by a single-writer fast path is justified by nothing.
- Trap: the single-consumer unbounded fast path forfeits `Count` (`CanCount` is `false`; `TryPeek` survives) — a depth gauge must stay on the multi-consumer implementation or carry its own counter.
- Bounded and plain unbounded readers expose `CanCount`/`Count` and `CanPeek`/`TryPeek`; the base reader defaults both capabilities to `false`, `Count` throws where uncounted, and `Completion` defaults to a never-completing task — capability probes, never assumptions, gate generic lane instrumentation.
- The prioritized lane is unbounded with a least-first comparer; `Count` and `TryPeek` are supported.
- Arity flags are ignored entirely on the prioritized lane — no single-consumer variant exists, so declaring them there is inert.
- Equal-priority order in the prioritized lane is not FIFO-stable; a fair-within-class lane carries a composite (priority, monotonic sequence) comparand with the sequence minted at write time.
- `AllowSynchronousContinuations` lets the completing side run the parked side's continuation inline — a writer thread executes reader code on `TryWrite`, and the inlined continuation runs before the producer's own next line.
- Inlining is admissible only on same-affinity hot paths where the producer is the intended execution vehicle; never on a lane whose consumer marshals, takes locks, or re-enters the producer.

## completion algebra

- `TryComplete(error?)`: first call wins; later calls return `false`; `Complete` throws on the second call — idempotent shutdown paths spell `TryComplete`.
- A completion error becomes the `Completion` task's fault and rethrows from pending and future reads; `ChannelClosedException` carries the completion error as its inner exception, so the original fault survives the crossing.
- Terminal observation splits by verb: `WaitToReadAsync` folds clean completion to `false`; `ReadAsync` throws `ChannelClosedException` — loops key on the boolean verb, single-item reads accept the throw.
- Completion fails parked writers: a `Wait`-mode writer parked in `WriteAsync` observes completion as `ChannelClosedException`; a parked `WaitToWriteAsync` resolves `false` — the two writer postures see shutdown through their own verb's grammar.
- Writer-side `WaitToWriteAsync` returning `false` is the structural stop-producing signal; catching the closed exception as flow control is the rejected form.
- `ReadAsync` attempts a synchronous `TryRead` before parking — single-item reads on a hot lane are allocation-free in the fast path; the async machinery engages only on an empty buffer.
- `ReadAllAsync` greedily drains every available item between waits — an inner `TryRead` loop empties the buffer, then the outer `WaitToReadAsync` parks; cancellation lands only at the wait edge, so a continuously hot lane defers cancellation observation to the next empty point.
- All async verbs short-circuit a pre-cancelled token to an already-canceled result before touching channel state — cancellation checks are free to front-load.
- Parked-operation pooling is token-gated: the single-consumer and rendezvous implementations reuse pooled parked operations only when the wait's token cannot be canceled; a cancelable token allocates a fresh parked operation per wait.
- Hot-loop law from that mechanic: a dedicated consumer loop passes the default token on its read verbs and shuts down via completion — per-iteration cancelable tokens forfeit the pooled fast path on every wait.
- Parked readers are served in arrival order; forced-drain residue reads come out FIFO on plain lanes and in comparer order on the prioritized lane — residue receipts inherit the lane's own ordering semantics.

## backpressure as evidence, receipts, drain

- Every lane declares which side observes pressure: `Wait` rows put evidence on the writer (await durations, park counts); drop rows put evidence in receipts (counts by reason).
- The conservation identity is the audit: items written = items read + receipted losses + drained residue; a lane that cannot close the identity from its declared evidence is misconfigured by construction.
- Receipt shape: { lane id, reason ∈ evicted-oldest | evicted-newest | refused-write, item evidence }.
- Reasons are disjoint loss classes: evicted-oldest measures conflation lag; refused-write measures shed load — folding them into one counter destroys the only signal distinguishing consumer-slow from producer-hot.
- Cooperative-then-forced drain is a generic two-phase pair instantiated per lane.
- Phase one, cooperative: `TryComplete` the writer, then drain the reader to exhaustion within the soft budget.
- Phase two, forced: cancel parked reads, `TryRead`-loop the residue into residue receipts, count as forced-loss evidence.
- The (complete, await-budget, hard-stop, receipt-residue) grammar covers any completable resource; the channel is its purest instance because each phase is a single verb.
- Banded participation: each lane row carries a drain band; bands drain in declared order; residue receipts carry the band so the drain fold attributes loss per band.
- A prioritized lane participates within itself by comparer order — high-band residue receipts first.
- The state machine that sequences bands is settled elsewhere; the lane owns only its participation contract: stop-accepting on `TryComplete`, bounded flush, receipted residue.
- Partial-batch flush at drain: a batching consumer flushes its partial batch the moment `WaitToReadAsync` returns `false` — completion is the flush trigger; count-threshold-only batchers strand their tail, so every batch loop's exit edge is a flush edge.

## consumer-loop shapes and evidence cadence

- Three consumer shapes cover every lane row: the dedicated single-reader loop (iterate `ReadAllAsync`), the N-worker shared reader (each worker loops `WaitToReadAsync`/`TryRead`), and the batch loop (read-N-or-quiet with the flush law above); worker count derives from the parallelism budget, never a local literal.
- N workers on one lane distribute items by race — no per-worker fairness exists; a workload requiring per-consumer fairness shards into per-consumer lanes instead of fighting the race.
- Consumer loops shut down via completion, not cancellation: the cooperative phase ends the loop through the read verb's own terminal grammar; the token appears only in the forced phase.
- `ReadAllAsync` is also the composition seam upward: it surfaces the lane as an async stream directly consumable by the budgeted parallel iteration primitive — producer decoupling and bounded parallel consumption compose as lane row plus budget row with zero adapter code.
- Lane identity in receipts is the row's vocabulary symbol, not the channel instance — receipts keyed by row name survive lane re-creation across drain/restart cycles and aggregate across shards.
- Depth evidence rides the consumer's cadence: `Count` is sampled as a gauge where `CanCount` holds, never read per-write; write-path evidence is the receipt stream, read-path evidence is the sampled gauge — the two cadences never share an instrument.
- `Wait`-lane pressure evidence is measured at the writer: stamped await duration around `WriteAsync` and a parked-writer count — the duration distribution is the backpressure signal that drop-mode lanes express as receipt counts instead.

## dataflow admission

- The channel is the default lane; a block earns admission only on a capability channels lack, and there are exactly four.
- Capability 1 — linked-graph completion: `LinkTo` with `PropagateCompletion` flows completion and faults through a topology; channels have no link concept.
- Capability 2 — grouping: `BatchBlock` with `TriggerBatch` partial flush, non-greedy (`Greedy = false`) two-phase reservation across sources, and `MaxNumberOfGroups` self-completion after a quota.
- Capability 3 — broadcast-latest: `BroadcastBlock` holds one current value and re-offers it to late or slow targets — conflation as topology for N consumers; for one consumer the mailbox lane row is denser.
- Capability 4 — ordered parallel transform: `TransformBlock` with `MaxDegreeOfParallelism` and `EnsureOrdered` preserves input order across parallel workers.
- The knob record in full: `DataflowBlockOptions` { `BoundedCapacity` (default `Unbounded` = -1), `TaskScheduler`, `CancellationToken`, `MaxMessagesPerTask`, `EnsureOrdered` (default `true`), `NameFormat` }.
- `ExecutionDataflowBlockOptions` adds { `MaxDegreeOfParallelism`, `SingleProducerConstrained` }; `GroupingDataflowBlockOptions` adds { `Greedy`, `MaxNumberOfGroups` }; `DataflowLinkOptions` is { `PropagateCompletion`, `MaxMessages`, `Append` }.
- `MaxMessagesPerTask` is the fairness knob: it bounds how many messages one spawned task processes before yielding the scheduler — long-running graphs sharing a scheduler with latency-sensitive work cap it.
- `TransformBlock`'s single bound covers input and output together: an unconsumed output queue counts against `BoundedCapacity` and backpressures producers.
- With `EnsureOrdered`, one slow item head-of-line blocks output release behind it.
- The canonical deadlock: bounded transform, output never linked or drained, producer parked in `SendAsync` — permanently wedged with zero faults.
- The rule that prevents it: a bounded propagator's output is linked or pulled before the first `SendAsync`.
- `LinkTo` without `PropagateCompletion` never completes downstream — the silent-never-drains defect; completion and faults travel only along links that opt in.
- Selective non-propagating links are valid only for tee-offs whose lifetime an owner ends explicitly.
- A faulted block discards its buffered input — loss without receipts; the asymmetry against channels (complete-and-drain versus fault-and-discard) is the deepest reason channels stay the default lane.
- Where a block is admitted, its `Completion` fault folds into the drain receipt rail so discard becomes attributed loss.
- Offer verbs mirror channel verbs: `Post` is the `TryWrite` analog — immediate `false` on a full bounded block; `SendAsync` is the `WriteAsync` analog and buffers exactly one postponed message per call.
- A producer picks one offer verb per lane row; mixing them mixes two backpressure regimes on one hop.
- `DataflowBlock.NullTarget<T>()` is the declared absorb row — explicit, receiptless discard; admission requires a predicate-guarded link so only matched rejects flow to it, making discard a routing decision instead of a default.
- `TransformManyBlock` is the one-to-many expansion row — one input message yields zero or more outputs inside the same bounded, ordered regime; flattening outside the block re-opens the bound.
- `BroadcastBlock` takes a cloning function at construction: targets receive copies, so mutable payloads do not alias across consumers — a null cloner shares references and silently reintroduces aliasing.
- Greedy joins (the default) take messages as they arrive: under skewed sources one side buffers unboundedly while waiting for the other — the skew hazard; non-greedy reservation is the bounded form and the only atomic one.
- The reservation rail (reserve, consume, release over `DataflowMessageHeader`) is the family's only multi-source atomic take; non-greedy `JoinBlock` consumes it to take from N sources atomically or not at all.
- Hand-rolling cross-lane atomic consumption over channels marks a missing join row — multi-source atomicity belongs to the block family.
- `Encapsulate(target, source)` folds a graph segment into one propagator — the composition primitive that keeps a multi-block lane presentable as one row with one ingress and one egress.
- The block family has its own drain verbs mirroring the lane pair: `Complete` on the head block is the cooperative edge; `Completion` on the leaf (reached via propagating links) is the await point; `OutputAvailableAsync` is the reader-side readiness analog of `WaitToReadAsync`.
- Forced-phase block residue: `TryReceive`/`TryReceiveAll` on `IReceivableSourceBlock<T>` pull buffered output without a parked reader — `TryReceiveAll` is the block-side residue loop, and its yield is the block's drained-residue receipt.
- `Fault(exception)` is the forced-stop verb for a wedged block; because faulting discards input, it is ordered after residue extraction, never before — fault-then-receive reads an already-emptied queue.
- A block graph's two-phase drain is therefore: cooperative — head `Complete`, await leaf `Completion` under the soft budget; forced — `TryReceiveAll` every receivable block into residue receipts, then `Fault` the stragglers and fold their `Completion` faults into the receipt rail.
- Blocks bridge to observable streams via `AsObservable`/`AsObserver`; the operator law on the stream side belongs to the streams lane — the admission fact here is that the bridge preserves the block's bounded semantics on its side of the seam.

- Write-path cost is mode-asymmetric: drop-mode `TryWrite` never allocates — eviction, receipt, and return happen on the caller's frame; a `Wait`-mode `WriteAsync` that parks allocates a blocked-write operation per park (pooled only on the rendezvous fast path with a non-cancelable token). Shed-style lanes are allocation-free under overload by construction; wait-style lanes pay allocation exactly when pressured.
- Completion faults carry domain codes: the exception handed to `TryComplete(error)` is the one residue readers and `Completion` observers see (wrapped as the closed exception's inner) — completing with a coded typed fault makes every drain-side observation classifiable without string matching.
- The handshake row doubles as the determinism row: capacity-zero hand-off makes producer/consumer interleaving explicit — a write completes if and only if a read consumed it — which converts timing-dependent hand-off assertions into sequential ones.

## divergent

- The lane row is one record — { name, capacity, mode, reader arity, writer arity, inline, comparer, band, receipt sink } — consumed by one lane factory; the record is the unit of declaration, audit, and drain participation, and scattered per-call-site option construction is the form the record deletes.
- Lane-table closure: the entire channel surface folds to six canonical rows, and naming them closes the vocabulary.
  - mailbox — capacity 1, `DropOldest`: the conflating latest-value lane; receipt count equals conflation count; deletes hand-rolled latest-value locks.
  - ordered-work — bounded, `Wait`, single consumer loop: deletes semaphore-plus-queue pairs.
  - handshake — capacity 0, `Wait`: write completion proves consumption; deletes completion-source ping-pong.
  - shed-ingest — bounded, `DropWrite`: admission control at the edge; deletes try-lock admission scatter.
  - control — prioritized with a composite (priority, sequence) comparand: deletes parallel urgent/normal queue pairs.
  - firehose-diagnostics — unbounded, single reader, self-carried counter: deletes fire-and-forget task spawning per event.
- Every producer in a process maps to one row; a producer fitting no row is a missing row, not a bespoke channel.
- The mailbox row is the structural seat of cadence-gated conflation at the hand-off altitude: the producer writes at native rate, the consumer reads at its own cadence.
- Mailbox drop receipts are the declared coalescing evidence at this altitude — value-level coalescing operators live in the streams lane and hand off to a mailbox when crossing into pull-world.
- Single-reader as structural singleton: declaring `SingleReader` and materializing exactly one consumer loop is the throughput-altitude spelling of single-owner lifecycle — exclusivity is structural (only one loop exists) rather than guarded (a gate checks ownership); the lane-row table is therefore also an ownership table where every single-reader row names its one consumer.
- Loss-identity algebra pushed to closure: loss sites across the family are exactly { full-mode eviction, forced-drain residue, declared absorb, faulted-block discard }, each carrying a distinct receipt reason.
- Per-lane conservation identities sum to a per-process identity — a process that can state ingress = egress + Σ receipts from declarations alone has made queue loss a provable property.
- Admission table with rejected rows, each named: `BufferBlock` rejected — a channel row is strictly denser for plain hand-off; `WriteOnceBlock` rejected — a single-assignment cell owns publish-once; standalone `ActionBlock` rejected — channel plus reader loop separates buffering policy from execution policy and keeps drain receipted; `BroadcastBlock` admitted only for N-consumer latest-value — the one-consumer case is the mailbox row.
- Unified batching/streaming vocabulary: a batched hand-off row { batch size, flush cadence, partial-flush-at-drain } and a bounded stream hop row { capacity = window, single reader } are one row family at two granularities.
- The three substrate spellings of that family: `BatchBlock` + `TriggerBatch` is the block spelling; read-N-or-quiet over a bounded channel is the lane spelling; a windowed remote stream hop is the wire spelling.
- One vocabulary across three substrates means a throughput requirement moves between substrates by swapping the row, never by redesigning the producer.
