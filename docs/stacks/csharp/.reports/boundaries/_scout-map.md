# Universal Boundary Concern Map

Pressure statements for any codebase integrating a native host, a UI toolkit, background compute, and a persistence layer. Each statement names a force at a seam, not an implementation.

## [HOST_COMPOSITION_SEAMS]

- The application core never calls into the native host directly; host adapters compose the core and forward completion signals. Any path where the core reaches into the host runtime inverts the seam.
- Reference direction is one-way per layer: lower layers consume runtime policy injected from above and emit receipts back; the layer that owns dispatch and drain never takes a reference downward.
- Companion-process bootstrap concerns — dependency injection containers, configuration binding, health endpoints, telemetry exporters, transport resilience — are legal only at out-of-process roots and test hosts; none may enter the in-process plugin path.
- In-process observability is write-only through platform-built-in primitives; exporters are passive listeners at companion roots. No exporter type crosses inward, and no inbound callback flows from exporters into core state.
- The persistence layer is a passive consumer of fully resolved identity and path values; resolution logic lives above it, and a partial or absent profile must be rejected before the open transition is entered.
- Shutdown is exclusively event-driven: the host raises unload, the adapter marshals, and only then does the global drain begin. The core observing "complete" before the marshal has happened is a seam violation.

## [HOST_EMBEDDING_AND_RENDERING]

- Toolkit surfaces embed as child nodes inside a long-lived host tree whose nodes are borrowed, never owned. Handle ordering during show and hide is a correctness requirement; mis-order corrupts z-order and compositor layering.
- Toolkit platform options must be set before any surface initializes — including any static constructor that touches the toolkit. Initialization order is a correctness law with no recovery path once the platform delegate is claimed.
- Only the embeddable-surface entry point is valid for retained panels; the standalone-window entry breaks native parenting, compositor layer ownership, and GPU surface tracking.
- Native 3D scene content composites above toolkit layers; overlays route through the host's own display conduit exclusively. Toolkit drawing placed over the scene violates the layering contract and fails unpredictably per platform.
- GPU and software rendering paths have distinct correctness surfaces; retained panels, chart renders, and offscreen draws must coexist with the host's frame pacing without starving it.
- Focus is a two-sided contract: the consumer owns resign/restore calls while the host owns the native focus chain. Mis-sequencing focus against attach/detach produces platform focus-routing failures.
- Self-carried native rendering assets risk identity conflicts when another in-process module maps a different version family of the same library first. Loaded-identity fingerprinting at runtime is the detection mechanism; asset copy and restore ordering is the (often unaddressed) prevention mechanism.

## [NATIVE_RESOURCE_LIFETIME]

- Drain order is strict and global: fence new work, signal cancellation, drain compute, flush persistence, complete UI, dispose handles last. A handle disposed before its consumer quiesces is a protocol violation, not a race.
- Drain deadlines are cancellation windows derived from an injectable clock, not unconditional disposal; every participant must honor the window and either complete or cancel cleanly within it.
- Native library load is a two-phase acquire: probe with evidence, then construct only on probe success. Probe and construction must be atomic from the caller's perspective — a stale probe must never authorize construction.
- Native runtime sessions are registry-owned and cache-keyed by content hash plus full configuration; disposal belongs to the registry owner, never to call sites; ephemeral per-call sessions are forbidden.
- Pooled buffers and streams return to the pool at phase boundaries and never leak into downstream materialization; pooled-memory vocabulary must never appear in public signatures, or callers become coupled to pool lifetime.
- Child surfaces detach without disposal on hide and reattach symmetrically on show; at teardown, child-surface close events complete before host containers dispose — inversion corrupts native handles still held by the child.
- Connections for raw maintenance operations (backup, integrity verification, compaction) are acquired and disposed within their owning lifecycle state, never held across transitions; an interruption mid-way through connection-scoped configuration directives must quarantine the connection rather than leave it ambiguous.
- Data-access contexts are operation-scoped: acquired and disposed inside the owning operation, never ambient, never singleton, never captured across async continuations.

## [LIFECYCLE_STATE_MACHINES]

- Every boundary layer owns an ordered lifecycle machine with explicit fault and drain states; unsupported transitions reject as typed receipts carrying boundary, correlation, and timestamp — never thrown, never silently ignored.
- The draining state fences new work and new subscriptions, then awaits in-flight completion. The contract must state whether a mid-flight fold or operation completes before close or is abandoned; leaving this implicit risks either lost writes or post-fence mutation.
- Fault states must remain escapable into the drain-and-dispose path; treating fault as fully terminal leaks subscriptions and live surfaces.
- A corruption state fences all normal read paths; only export and close transitions remain valid. Transparent fallback reads from a corrupt store are forbidden.
- Re-boot paths (disposed back to uninitialized) collide with process-static toolkit initialization that is not re-entrant within a process. The design must choose suspended-not-destroyed retention or accept process restart as the only true teardown; this is the highest-risk lifecycle seam.
- Partially applied schema migrations and abandoned migration locks surface as operator-visible maintenance receipts; silently retrying a possibly half-applied change is forbidden, and no automatic rollback may be assumed.
- A store whose schema version exceeds the compiled model is rejected as a typed failure; there is no best-effort open on a newer schema.
- Failure-and-cancel ownership at teardown is cooperative: lower layers honor the cancellation signal and complete or cancel in-flight states cleanly; only the root decides re-open cadence.

## [ABSENCE_AND_SENTINELS]

- Capability absence is typed state (unavailable, degraded), never null, ambient singletons, or hidden locators; every consumer handles the degraded branch explicitly.
- Degraded outcomes carry a usable result plus explicit degradation evidence as a first-class outcome — never a null, never an exception swallowed at the call site.
- Unknown progress is declared, not encoded as zero or a magic negative; the progress fraction is a discriminated value with a monotone non-decreasing law.
- Availability derived from host state (such as undo depth) must emit a definite disabled value when the host has none — not an unresolved optional that leaves controls indeterminate.
- Unvalidated screens carry a defined initial validation state so the UI never transiently renders as valid before rules initialize.
- Missing asset keys resolve to a defined fallback, never an unhandled lookup failure inside the resource pipeline.
- Native-library unavailability is a terminal typed receipt: not retried internally, not forked into a fallback path.
- Health is derived from typed capability states and explicit instants — never status strings or untyped booleans; a contributor that constructs a status string violates the projection law.
- Platform sentinel values (epoch minima, empty identifiers, origin defaults) project to typed absence at the boundary and never propagate inward — including into the persisted entity graph and snapshot payloads.

## [EVENTS_AND_SUBSCRIPTIONS]

- Every subscription is detachable and owned by a lifecycle: detach happens on drain entry, strictly before handle disposal. Failure to detach before disposal is a leak by definition.
- Activation-scoped subscription symmetry: every subscription opened during screen activation joins the activation disposable set; anything opened outside it accumulates silently across reactivation cycles.
- Host-owned event buses (canvas, paint, chrome, repaint) can detach and reattach independently of the consumer's lifecycle; adapters must re-query availability defensively rather than assume subscription continuity across hide/show transitions.
- Host notification subscriptions (theme, scale, activation) are lifecycle-owned and disposed through the drain path, never left as process-static handlers.
- Live collection change subscriptions tear down at drain before dispose; a subscriber that completes asynchronously after disposal begins risks mutating a disposed source.
- Degradation and health changes are polled via typed state, never pushed via callbacks into callers.
- The projection stream never carries error notifications; faults travel on a separate receipt channel. Consumers expecting observable error semantics are silently isolated unless they also subscribe to the receipt surface.
- Gated progress observables must specify late-subscription semantics — replay-last, snapshot-plus-delta, or rejection. Silence here yields inconsistent consumer behavior, since a subscriber arriving mid-execution misses early phases.
- Committed-change delivery to the fold worker rides a queue or channel, not a raw event that can be missed under contention.

## [THREADS_AND_MARSHALING]

- One UI scheduler port, established exactly once, through which all UI-bound work crosses; the core never dispatches to the UI thread directly. A port captured off the UI thread yields silent deadlock or cross-thread faults.
- The UI thread is one that all mutation must marshal onto with captured failure: the platform's invoke primitive can swallow throws into a silent report channel, so marshaled work must run inside a failure-capturing effect.
- Multiple scheduler participants (toolkit dispatcher, reactive scheduler, host thread affinity) align through the single boundary; all live projections and progress observations marshal through it before binding.
- Producers never schedule for consumers: background compute emits without observe-on calls; the subscriber owns all marshaling and the producer assumes nothing about subscriber threads.
- Solve and render hot paths never block on compute or storage: they submit typed intent and consume cached receipts; any synchronous join is a threading-law violation.
- A single named serial worker folds committed changes into read-only projected state; parallelism inside the fold breaks projection consistency; the worker's lifetime is bound to the store lifecycle and to a rendezvous with the global drain.
- Rate limiting belongs to the observer: the projection emits at fold rate; the UI applies sampling and throttling at its own scheduler boundary; the producer never throttles internally.
- Internal thread pools of embedded native runtimes are capped below the host's display and solve thread budgets, with the cap recorded as evidence at construction.
- Scale and DPI change notifications arrive on the main thread and must propagate to layout, font metrics, and canvas resolution before the next render frame; asynchronous dispatch risks one stale-scale frame.

## [STATE_CELLS_AND_SINGLETONS]

- One root cancellation token per process, owned at the host root; all work derives child scopes from it; it is never replicated, forked independently, or held beyond drain.
- One runtime spine record per process (cancellation, time, ports, configuration); a re-boot creates a new record, never a second concurrent instance; multiple in-process consumers share one scheduler identity rather than establishing competing spines.
- Process-static native initialization is token-gated and once-only; concurrent open transitions racing the initializer require a gate cell, since the initializer carries no idempotency guarantee.
- Memoization and cache keys must be structurally complete — content hash plus full configuration; partial hashing produces stale-hit false positives that present as nondeterministic correctness bugs.
- Generated catalogs and registries are process-static with stable keys that survive provider-set regeneration; identity must hold across rebuilds without public API change.
- Correlation identifiers for capture windows are finite-lifetime, owned at the root, and propagated unchanged into every artifact receipt. Fault re-entry must define replace-versus-reuse semantics: a new identifier breaks downstream joins on the old window; a reused one collides artifacts from two fault windows.
- Loaded native library identity is a process-level singleton check: if a different version family is already mapped, the conflict must surface rather than be silently accepted.

## [WIRE_CONTRACTS_AND_RECEIPTS]

- One polymorphic receipt family per layer: every transition emits a typed receipt carrying timestamp, correlation, discriminant, policy, elapsed duration, and source boundary. Generic ledger abstractions and parallel per-lane result systems are rejected; extension adds a receipt case, never a new service or result type.
- External input crosses exactly one validation boundary: raw shape, boundary validator, typed validation rail, domain model. Folding an unvalidated external shape into a domain model skips the law.
- Exactly one retry owner per outbound hop, held at the root; a lower layer detecting a second owner emits conflict evidence rather than stacking a local loop — and the root must assert no-conflict at drain, or the evidence is inert and the invariant unenforceable.
- One wire codec per remote lane; secondary serialization formats are rejected on that lane. Schema-derived contracts are compile-time proven within generator scope, not runtime-reflected; any payload type outside generator coverage must be projected to an attributed payload record at the boundary or it becomes a latent runtime failure.
- Snapshot envelopes carry schema version, codec discriminant, explicit checksum, and payload; the discriminant is preserved so decoding selects the correct path; content-derived keys are by construction unstable across payload change and consumers must not assume otherwise.
- Temporal values persist through one explicit temporal vocabulary with registered serialization; platform date sentinels never enter persisted or audited shapes.
- Physical-unit conversion happens at the external boundary into typed intent and measurement fields; raw scalars never model units internally.
- Storage naming and convention transforms are internal; public code addresses entity kinds and query shapes, never physical column or table names.
- Optional engine capabilities (full-text indexing, structured-document functions) gate on a probe receipt, never on assumed presence in the loaded native bundle.
- Receipt and projection shapes are cross-layer breaking-change surfaces: a shape change in a lower rail breaks every observer above it; live projections and command consumers bind to shape stability, not to re-query.
- Toolkit and provider types never appear on public surfaces; only read-only snapshots and observable projections cross the boundary; mutable subjects, changesets, and internal collection sources stay sealed.
- Support and diagnostic bundles are composite wire shapes with split ownership: the root owns trigger, correlation, window, and size cap; each layer owns its evidence items, classification, and redaction; artifacts leave the local store only after redaction.

## [LANE_PRESSURE]

- **resource-capsules** — borrowed host-tree nodes versus owned drain slots; two-phase probe-then-construct atomic acquisition; registry-owned session disposal with cache-keyed reuse; operation-scoped data contexts with no ambient capture; pooled buffers returned at phase boundaries and absent from public signatures; maintenance connections scoped to single lifecycle states; child-before-container disposal ordering; quarantine on interrupted connection-scoped configuration.
- **absence-and-sentinels** — typed unavailable/degraded capability states with mandatory degraded-branch handling; declared-unknown progress as a discriminated value; definite disabled emission on absent host state; defined initial validation state; asset-key fallbacks; sentinel-to-typed-absence projection at every boundary including persisted shapes; corruption, downgrade, migration-lock, and native-unavailability as terminal typed receipts.
- **events-and-threads** — single once-captured scheduler port with failure-capturing marshal; activation-scoped subscription symmetry; defensive re-query of host buses that detach independently; serial fold worker with drain rendezvous and observer-owned rate limiting; producer-side scheduling abstinence; hot-path non-blocking law; native thread-pool caps below host budgets; pre-frame scale propagation; subscription fencing at drain entry before any disposal.
- **state-cells** — single root cancellation token with derived child scopes; one runtime spine per process with defined re-boot semantics; token-gated once-only native initialization; structurally complete cache keys; finite-lifetime correlation cells with explicit fault re-entry replace-versus-reuse semantics; stable registry keys across regeneration; loaded-native-identity conflict surfacing.
- **wire-contracts** — one receipt family per layer with typed cases for every transition and failure; single validation boundary for external input; single retry owner per hop with root-enforced conflict assertion; one codec per lane with compile-time generator coverage and boundary payload projection; versioned, checksummed, codec-discriminated envelopes; one temporal vocabulary excluding date sentinels; probe-gated optional capabilities; receipt-shape stability as the cross-layer breaking-change contract; sealed internal mutables with read-only projections as the only public crossing.
