# [PY_RUNTIME_IDEAS]

The forward pool of higher-order concepts for `runtime`, grounded in the folder's domain and the monorepo purpose. Each open idea is a card — a bracketed slug, the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more `TASKLOG.md` tasks. A finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Five concepts realized in the design pages — streaming/bundle identity, the obstore fast-path, the apscheduler trigger union, the structlog/OTel bridge, and the xxhash admission — are closed at the design bar; their remaining work is the downstream source-transcription mode, outside the planning pool.

## [1]-[OPEN]

[OTEL_METRICS_SPINE]:
- Build the planned `observability/metrics` sub-domain: an async-observable instrument set (companion request-duration histogram, lane drain counters folded from `DrainReceipt`, `psutil` process RSS/CPU gauges) registered against one `MeterProvider` carrying the process/runtime semantic-conventions resource, exported over the same OTLP exporter the logs and traces use.
- Unlocks one measured-execution signal stream the host scrapes without per-package metric reinvention; lane saturation, retry exhaustion, and companion latency become first-class observable metrics rather than log fields.
- The `observability/receipts` page emits logs, trace context, and ad-hoc `psutil` facts but mints no metrics; the companion server and lane spine produce exactly the count/duration/gauge signals OTel metrics own, currently lost or stuffed into log fields.

[INTERPRETER_OFFLOAD_LANE]:
- Add a CPU-bound lane variant to `concurrency/lanes` over `anyio.to_interpreter.run_sync` that the geometry siblings hand a callable to — the lane offloads any caller-supplied CPU kernel (the open3d/trimesh numeric loops the siblings own) into per-subinterpreter execution under the same `CapacityLimiter` and `DrainReceipt`, never importing the kernel itself, so heavy geometry work does not stall the companion event loop or the GIL.
- Unlocks true-parallel CPU geometry on the normal-GIL floor through per-subinterpreter isolation (each subinterpreter carries its own GIL under PEP 734) without a process-pool serialization tax on large meshes, keeping one lane spine for I/O-bound and CPU-bound work alike.
- `LanePolicy.run` fans out coroutines under one event loop, and the only CPU escape today is `to_process.run_sync` with its pickle round-trip; PEP 734 subinterpreters and `anyio.to_interpreter` give a lower-overhead isolation seam the lane has not yet absorbed.

[CONTENT_ADDRESSED_LANE_CACHE]:
- Compose `identity/content-identity` with `concurrency/lanes` so a unit of work admitted as a `(ContentKey, Work[T])` pair short-circuits when the key — folding input bytes and `IdentityPolicy` — already carries a result, the `DrainReceipt` distinguishing a cache hit from a fresh completion and the cache itself an `expression` `Map[ContentKey, T]` the lane threads immutably across one session.
- Unlocks by-reference reuse of the most expensive offline work — re-tessellation, IFC evaluation, scan registration — across a companion session, turning the content-identity seed from an interchange key into an execution-elision key without a second cache owner.
- The two owners exist independently; identity keys artifacts and lanes drain work, but nothing folds the key into the lane admission decision, so identical work re-runs at full cost inside one session.

[STRUCTURAL_DRIFT_GUARD]:
- Extend `evidence/evidence` `Structural` from a span-returning query into a cross-language drift detector: one tree-sitter query family that locates a shared wire-projection name (a second mint of a canonical concept) across the Python and TypeScript sources, feeding the `assay code` rail the named cross-language drift defect the topology law forbids.
- Unlocks an automated guard on the one-name-one-owner law the wire contract depends on, catching a re-minted identity seed, receipt rail, or capability descriptor before it lands rather than at integration.
- `Structural.query` returns raw byte spans with no semantic over them; the drift defect is defined in the architecture law but has no detector, and tree-sitter S-expression queries over both grammars already in the manifest are the exact surface to express it.

[API_CATALOGUE_CAPTURE]:
- Capture the `.api/` catalogue members by resolvability — `obstore`, `apscheduler`, and `keyring` resolve on the cp315 core, so their page-named members graduate from a `RESEARCH` seam under a uv-sync reflection pass through `evidence/evidence` `ApiPackage.reflect`, while `xxhash` stays install-gated until its wheel syncs.
- Unlocks zero-guess fence transcription for the `resources`, `concurrency`, and `server` owners that presently name core-resolvable members, and isolates the `identity` xxhash digest-endianness confirmation against the C# hashing seam as the single install-gated link.
- The three core-resolvable spellings carry no reflection evidence only because no uv-sync pass has run, whereas `xxhash` is manifest-declared yet absent from the present host install, so the capture splits a pending reflection pass from an install gate rather than treating all four uniformly.

## [2]-[CLOSED]

- [INCREMENTAL_CONTENT_IDENTITY]: closed — `identity/content-identity` folds the whole-payload, streaming `xxh3_128`, and child-`ContentKey` Merkle modalities into one input-discriminated `ContentIdentity.of` under the one settings-folded seed; the streaming and Merkle-bundle design is complete, the `[XXHASH_PARITY]` capture seam tracking the uncaptured `xxhash` surface.
- [OBSTORE_TRANSPORT_FAST_PATH]: closed — `resources/roots` `ResourceRoot.reader` dispatches `OBJECT_STORE_SCHEMES` to the `obstore` async API and every other scheme to `UPath`, one polymorphic reader; the fast-path design is complete.
- [APSCHEDULER_TRIGGER_UNION]: closed — `concurrency/lanes` carries the `CronTrigger`/`IntervalTrigger`/`DateTrigger` union and the `fired` feeder over `Trigger.get_next_fire_time`, draining through the one `DrainReceipt`; the stale `aiocron` spelling is removed.
- [STRUCTLOG_OTEL_TRACE_BRIDGE]: closed — `observability/receipts` carries the `trace_context` processor reading `get_current_span` and the `merge_contextvars`/JSON chain with OTLP log egress; the processor-chain design is complete.
- [XXHASH_ADMISSION]: closed — `xxhash` is admitted in the root manifest (`xxhash>=3.7.0`, abi3 wheel) and `identity/content-identity` consumes `xxh3_128_intdigest`/`xxh3_64_intdigest`/streaming `xxh3_128` directly; the catalogue capture and the C# digest-endianness parity remain the `[XXHASH_PARITY]` research seam.
