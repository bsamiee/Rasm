# [PY_RUNTIME_TASKLOG]

Open and closed work for `runtime`, distilled from `IDEAS.md`. Each task card carries a status marker on its leader — `[QUEUED]`, `[ACTIVE]`, or `[BLOCKED]` when open; `[COMPLETE]` or `[DROPPED]` when closed — and three to four bullets: the capability or file to build, the external packages to integrate, the integration points and boundaries/wires, and the key considerations. One idea spawns one or more tasks. A design-complete idea whose only remaining work is source transcription closes here and routes to `[TRANSCRIBE_OWNERS]`.

## [1]-[OPEN]

[QUEUED] [OTEL_METRICS_PAGE]:
- Author the planned `observability/metrics` page: an OTel async-observable instrument set (companion request-duration histogram, lane drain counters folded from `DrainReceipt`, `psutil` RSS/CPU gauges) against one `MeterProvider` with process/runtime semantic-conventions resource attributes.
- Integrate `opentelemetry-api`, `opentelemetry-sdk` (`MeterProvider`/`ObservableGauge`/`ObservableCounter`), `opentelemetry-exporter-otlp-proto-http`, `psutil`.
- Internal to `observability/`; the OTLP exporter is the same one the `receipts` log/trace egress uses, and the drain-counter source is the `DrainReceipt` from `concurrency/lanes#LANE`.
- Async-observable callbacks must read live `DrainReceipt`/`psutil` state without holding a lane lock; the metric stream is local-evidence only — product telemetry export and health stay AppHost-owned.

[QUEUED] [INTERPRETER_OFFLOAD_LANE]:
- Extend `concurrency/lanes` `LanePolicy` with a CPU-bound offload variant over `anyio.to_interpreter.run_sync`, routing geometry/scan kernels into per-subinterpreter execution under the same `CapacityLimiter` and emitting the same `DrainReceipt`.
- Integrate `anyio` (`to_interpreter.run_sync`, `to_process.run_sync` as the fallback when subinterpreters are unavailable).
- Internal to `concurrency/`; the offloaded kernels are the offline scan/tessellation work the `libs/python` geometry siblings own — the lane offloads, it never imports the kernel; the receipt drains through the one `DrainReceipt`.
- Subinterpreter availability is runtime-gated (PEP 734); the variant falls back to `to_process.run_sync` when `to_interpreter` is absent, and the offloaded callable must be picklable for the process fallback path.

[QUEUED] [CONTENT_ADDRESSED_LANE_CACHE]:
- Extend `concurrency/lanes` `LanePolicy.run` to accept work as `Sequence[tuple[ContentKey, Work[T]]]`, threading a `frozendict[ContentKey, T]` cache so a unit whose key already carries a result short-circuits without invoking the coroutine, the `DrainReceipt` gaining a `hit` count distinct from `completed`.
- Integrate `xxhash` (via `identity`), `expression` (`frozendict`/`Option`/`Result`), `msgspec`.
- Internal to `concurrency/` reading `identity/content-identity#IDENTITY` for the `ContentKey` shape; the cache is session-local in-memory, never a durable store (durable identity federation stays the C# `Rasm.Persistence` owner consumed at the wire).
- The key folds the `IdentityPolicy` so a settings change misses correctly; eviction is bounded by an LRU cap; a hit reproduces the exact `RuntimeRail` value, never a stale partial; only an `Ok` outcome caches, an `Error` re-runs.

[QUEUED] [STRUCTURAL_DRIFT_GUARD]:
- Extend `evidence/evidence` `Structural` with a cross-language drift-query family locating a re-minted canonical concept (a second wire-projection name) across Python and TypeScript sources, returning the offending spans for the `assay code` rail.
- Integrate `tree-sitter`, `tree-sitter-python`, `tree-sitter-typescript`.
- Internal to `evidence/`; the output feeds the `assay code` rail and the cross-`libs/` drift law — it consumes the shared canonical-name set, never minting a parallel registry.
- The query family is the named cross-language drift detector; a false positive on a legitimately distinct same-named concept in a distinct namespace must be distinguishable by the query's namespace scope, never a blanket name match.

[BLOCKED] [COMPANION_FLOOR]:
- Boot the `grpc.aio` `ServerHost` against the C# `ComputeService`/`ArtifactSync` descriptors under the `anyio` runner and gate the connecting peer on the `Credential` intake, resolving the PEM-bundle encoding, in `server/serve#SERVE`.
- Integrate `grpcio`, `grpcio-tools`, `protobuf`, `keyring` (the `Keyring` credential case), `anyio`.
- The wire is the existing C# `ComputeService`/`ArtifactSync` proto consumed at the seam — the runtime mints no second wire vocabulary; the companion serves over the UDS/InProcess leg.
- Blocked on the sub-3.15 companion floor/lock-scope decision: `grpcio`/`grpcio-tools`/`protobuf` ride `python_version<'3.13'` and are pruned from the lock until the companion environment is admitted; `keyring` admission and the `CREDENTIAL_PEM` resolution land with this task.

[BLOCKED] [CRDT_OPLOG_DECODE]:
- Decode the op-log CRDT-op union (the breaking wire amendment, LWW surviving only as the register arm) in `server/serve#SERVE` as one more case on the existing C# companion proto — the companion never authors an op kind the wire does not carry.
- Integrate `grpcio`/`protobuf` (the companion proto), `msgspec`; no new wire vocabulary.
- The producer is `csharp:Rasm.Persistence/versioning#CRDT_ALGEBRA` + `sync/collaboration#TS_PROJECTION`; the companion decodes the amended payload at the same seam the base services cross, never a second decoder. The cross-`libs/` `CRDT_OPLOG_WIRE_AMENDMENT` task carries the tri-language scope.
- Blocked on `[COMPANION_FLOOR]` (the companion-floor admission and the base proto) and on the upstream op vocabulary landing on the wire.

[BLOCKED] [CAPABILITY_SDK_CONSUME]:
- Consume the C#-generated capability-descriptor SDK in `server/serve#SERVE`, deriving the companion command surface from the generated descriptor — one polymorphic invoke keyed by descriptor id, the `inputSchema` the generated per-descriptor JSON Schema, never a hand-written per-service client.
- Integrate `grpcio`/`protobuf` (the generated SDK), `msgspec`; no per-service hand client.
- The producer is `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN`; the companion reads the generated descriptor and never re-authors a capability shape. The cross-`libs/` `CAPABILITY_SDK_CODEGEN` task carries the tri-language scope.
- Blocked on `[COMPANION_FLOOR]` and on the upstream descriptor source landing.

## [2]-[CLOSED]

- [STREAMING_IDENTITY] [COMPLETE]: `identity/content-identity` carries `ContentIdentity.stream` and `ContentIdentity.fold` under the one settings-folded seed; design landed; source transcription is the separate mode.
- [OBSTORE_FAST_PATH] [COMPLETE]: `resources/roots` `ResourceRoot.reader` dispatches `OBJECT_STORE_SCHEMES` to the `obstore` async API and every other scheme to `UPath`; design landed, the `OBSTORE_READER` fence-spelling row resolving at transcription.
- [APSCHEDULER_SOURCE] [COMPLETE]: `concurrency/lanes` carries the `apscheduler` `Trigger` union and the `fired` feeder; the stale `aiocron` spelling is removed.
- [STRUCTLOG_CHAIN] [COMPLETE]: `observability/receipts` `Signals.configure` carries `merge_contextvars`, the `trace_context` processor, the timestamper, and the JSON renderer, with the OTLP log egress; design landed.
- [XXHASH_ADMISSION] [COMPLETE]: `xxhash` admitted in the root manifest; `identity/content-identity` consumes `xxh3_128_intdigest`/`xxh3_64_intdigest`, closing the seed-binding RESEARCH gap.
