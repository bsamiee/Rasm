# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — the wire seams that span two or more of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`; this node carries only the seam each branch consumes at the boundary, never a re-aggregation of branch work. Each task names its producer and consumer touchpoints in `lang:pkg/page#CLUSTER` notation plus the considerations that scope it; a closed task compacts to one or two lines.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with. `Atomic` flags a minor-scope task so a later session sizes its turn correctly and does not overscope a batch of small items.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
- Atomic: <present only on a minor-scope task; one short phrase naming the small unit so a later session does not overscope its turn>.
-->

[TS_CONSUMES_CSHARP_WIRE]-[QUEUED]: decode C# wire contracts in the TypeScript interchange tier.
- Capability: the TS interchange tier consumes only C#-minted wire contracts: suite proto method shapes, tenant context, W3C trace context, grpc-web transport, and the upstream capability tuple.
- Shape: producer `csharp:Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` feeds consumer `typescript:interchange/Transport/transport#PROTOCOL_SELECTION_AXIS`, with host-local C# surfaces excluded from the consumed set.
- Unlocks: TypeScript web and edge clients gain a live transport leg without coupling to C# interiors or inventing branch-local wire facts.
- Anchors: the C#-owns-the-wire law, the grpc-web transport leg, and `csharp:Rasm.AppHost/Observability/telemetry` as the four-signal telemetry lake the browser reads as one decoded feed.

[PYTHON_COMPANION_SERVES_WIRE]-[BLOCKED]: serve the C# compute wire through the Python companion.
- Capability: the Python `runtime/transport/serve` companion serves existing C# `ComputeService` and `ArtifactSync` gRPC over the UDS/InProcess leg, reproduces `ContentIdentity`, and carries data/artifact bundles plus graduation evidence across the offline seam.
- Shape: producer `csharp:Rasm.Compute/Runtime/channels#PROTO_VOCABULARY` feeds consumers `python:runtime/transport/serve#SERVE` and `python:runtime/evidence/identity#IDENTITY`.
- Unlocks: the Python companion becomes a peer decoder and offline producer for the C# wire without reaching into C# interiors or minting a second vocabulary.
- Anchors: the one C# `XxHash128` seed, the companion server-host contract, the Python branch `SERVE_C_WIRE_CONSUME_GENERATED` task, and `[CREDENTIAL_PEM]` `\n--SEP--\n` placeholder handling.
- Tension: blocked on the upstream descriptor source, the CRDT op landing on the wire, and the `[CREDENTIAL_PEM]` `\n--SEP--\n` placeholder carried on `SERVE_HOST`.

[TESSELLATION_RAIL_TRILANG]-[QUEUED]: route the shared IFC and CAD tessellation rail through Python-native GLB output.
- Capability: the Python-native two-hop `IFC → IfcOpenShell → GLB` rail and the AP242 CAD-STEP `STEP → OCCT → GLB` companion produce the one content-keyed GLB every runtime consumes.
- Shape: producers `python:geometry/mesh/daemon#TESSELLATE` and `csharp:Rasm.Bim/exchange/interchange#TESSELLATION_REQUEST` feed consumers `csharp:Rasm.Compute/Runtime/codecs#TILE_PARTITION` and `typescript:ui/render/glb#GLB_VIEWPORT` through `typescript:interchange/Codec/frame#CONTENT_HASHING`.
- Unlocks: C# builds and content-addresses tessellation requests, Python serves geometry evaluation, and TypeScript renders GLB by the same content key without a duplicate mesh pipeline per runtime.
- Anchors: the one-owner-per-runtime geometry law, the `python:runtime/transport/serve#SERVE` companion contract, the per-tile `EXT_structural_metadata`/`EXT_mesh_features` schema, the `SourceFormat`-discriminated request, and the one `ContentIdentity` seed.
- Tension: the GLB arm is queued; the AP242 STEP arm lands through the geometry companion's OpenCascade admission, and the TS mesh-shape promotion stays carried on the TS `ui` `TASKLOG.md`.

[CONTENT_IDENTITY_PARITY]-[BLOCKED]: prove the C# content-address seed reproduces across all three runtimes.
- Capability: the `XxHash128` content-address seed, seed-zero policy, and HLC two-64-bit-half order reproduce byte-identically in C#, Python, and TypeScript.
- Shape: producer `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` feeds consumers `python:runtime/evidence/identity#IDENTITY` and `typescript:interchange/Codec/frame#CONTENT_HASHING`.
- Unlocks: off-thread digest reuse, convergence presence folds, causal ordering, and cross-runtime artifact reuse can trust one key instead of reconciling per-runtime hashes.
- Anchors: the C#-owned seed, Python `xxh3_128_intdigest`, a TS 128-bit wasm hash path, and the multi-runtime fixture proving half order.
- Tension: blocked on TS `hash-wasm` admission replacing 128-bit-incapable `xxhash-wasm`; the Python leg is a root-manifest substrate plus fixture-proof task under `python:runtime/evidence/identity`.

[CRDT_OPLOG_WIRE_AMENDMENT]-[QUEUED]: op-log CRDT-op union is a breaking amendment to the one wire vocabulary.
- Capability: the op-log CRDT-op union amends the one wire vocabulary, with LWW surviving only as the register arm and no consumer-authored op kind outside the producer vocabulary.
- Shape: producers `csharp:Rasm.Persistence/Version/commits#CRDT_WIRE`, `#CRDT_ALGEBRA`, `#TS_PROJECTION`, and `csharp:Rasm.Persistence/Sync/collaboration#TS_PROJECTION` feed `typescript:interchange/Codec/codec#CRDT_OP_DECODE`, `typescript:projection/causality/vector#CRDT_SEMILATTICE`, `typescript:ui/overlay/presence#PRESENCE_OVERLAY`, and `python:runtime/transport/serve#CRDT_DECODE`.
- Unlocks: TS web, TS projection, TS UI, and Python companion legs decode one CRDT payload, including the `set`/`write`/`add`/`remove`/`increment`/`insertAfter`/`delete`/`maintain`/`beat`/`leave` arms and the `EphemeralMap` presence delta.
- Anchors: `OpLogEntryWire`, `CrdtOpWire`, `CommitNodeWire`, `VersionVectorWire`, `HlcWire`, `causality/vector#CRDT_SEMILATTICE`, `#ORIGIN_CURSOR`, the msgpack delta decode, and `TRI_LANGUAGE_WIRE_PARITY`.
- Tension: residual producer-gated gaps are Python `Lz4BlockArray` envelope decompression (`[CRDT_OPLOG_LZ4]`) and UI `Awareness` beat `state` byte-encoding (`[BEAT_PAYLOAD]`); the producer must choose `MessagePackCompression.None` for the companion lane or publish one envelope spec for the msgpack-csharp ext block-array framing.

[CAPABILITY_SDK_CODEGEN]-[QUEUED]: one capability-descriptor source emits the C#/TS/Python SDKs and the MCP projection.
- Capability: one AppHost capability-descriptor source emits C#, TypeScript, and Python SDKs plus the MCP projection, with no per-service hand-written client.
- Shape: producer `csharp:Rasm.AppHost/Agent/capability#SDK_CODEGEN` feeds consumers `typescript:interchange/Transport/transport#CODEGEN_TOOLING` and `python:runtime/transport/serve#SERVE`.
- Unlocks: a new operation becomes a descriptor row and one generated `invoke` path keyed by descriptor id, with generated per-descriptor JSON Schema as the input contract.
- Anchors: the AppHost descriptor owner, the polymorphic `invoke` method, descriptor `inputSchema`, the TS codegen tooling consumer, and the Python companion server-host.
- Tension: the TS consumer is blocked on the upstream descriptor source; Python SDK-codegen consumption rides the companion server-host.

[FAULT_WIRE_ROUNDTRIP]-[QUEUED]: prove the C# fault triple reconstructs losslessly in TypeScript.
- Capability: C# packs `(package, code, case)` with package-exclusive code bands and TypeScript reconstructs the same triple losslessly.
- Shape: producer `csharp:Rasm.Compute/Runtime/channels#FAULT_PROJECTION` `WireFault`/`FaultDetail` pack site feeds consumer `typescript:interchange/Ingress/fault` `faultTagOf`/`FAULT_CTOR`; Python mints `FaultDetail` outbound but is not a package-keyed decoder.
- Unlocks: a typed fault rail crosses C# to TypeScript without collapsing to status code plus string or treating either `package` or `code` as a total key.
- Anchors: ComputeFault `2200`, HopFault `4500`, WireFault `4520-4532`, store/config app-root bands, producer code-in-band assertions, TS `case` threading, `Quarantine` fallback for unmapped packages, and the round-trip parity property in both suites.

[CAUSAL_TENANT_IDENTITY_WIRE]-[BLOCKED]: read the C# causal stamp and tenant context in Python and TypeScript.
- Capability: the C#-minted HLC two-half causal stamp and `TenantContext` partition are read by Python inbound receipt propagation and TypeScript skew-aware ordering without a peer re-mint.
- Shape: producer `csharp:Rasm.AppHost/Runtime/ports#PORT_RECORDS` feeds consumers `python:runtime/transport/serve#SERVE` and `typescript:projection/convergence#SKEW_ORDERING`.
- Unlocks: every runtime orders and attributes work by one causal/tenant frame, with TypeScript marking concurrent-uncertain rows from the HLC band instead of inventing a render-only ordering leaf.
- Anchors: `ReceiptEnvelope`, `hlc_physical`, `hlc_logical`, `tenant`, the same multi-runtime fixture as content-seed parity, and the C#-owns-the-wire law that makes a second causal stamp or tenant scheme a drift defect.
- Tension: inherits the content-seed parity proof: TS `hash-wasm` admission and Python `runtime/evidence/identity` fixture parity.

[GRADUATION_EVIDENCE_INWARD]-[QUEUED]: python graduation rail is the single content-keyed contract every offline result crosses outward on.
- Capability: Python graduation evidence is the single content-keyed outward contract for offline results consumed by the C# determinism closure.
- Shape: producer `python:compute/graduation#GRADUATION` feeds consumer `csharp:Rasm.AppHost/Runtime/determinism#EVENT_LOG`.
- Unlocks: C# re-imports offline science into the hash-chained event log by one `ContentIdentity` key and one `HandoffAxis` rail instead of a family of per-result handoffs.
- Anchors: the seven-arm `HandoffAxis` `Literal` union (`solver`, `symbolic`, `model-asset`, `array-layout`, `unit-law`, `uncertainty-law`, `geometry`), `GeometrySubject`, content-keyed ONNX `model-asset`, `ModelAssetManifest`, landed `T-DDG-ADJOINT-ROWS`, and `Rasm.Compute` `DesignProblem.OperatorRows`.
- Tension: the C# determinism consumer's remaining open leaf is the offline graduation-evidence handoff itself, not the adjoint surface.

[CONTENT_REUSE_FEDERATION_WIRE]-[QUEUED]: resolve a companion cross-session content-keyed reuse miss against the C#-owned durable federation ledger.
- Capability: the Python companion's session-local content-keyed lane cache federates by-reference over the wire so an in-session miss resolves a prior `Ok` result against the C# durable reuse ledger, the `ContentKey` the single reuse name and the C# `Query/Federation` the sole durable-identity owner.
- Shape: producer `csharp:Rasm.Persistence/Query/federation#FEDERATED_PLAN` feeds consumer `python:runtime/transport/serve#SERVE`, with `python:runtime/transport/wire#WIRE_PROTO_CODEC` transcoding the `ReuseFrame` `(ContentKey, ContentDescriptor)` lookup and `python:runtime/evidence/identity#IDENTITY` reproducing the seed; the durable row keys on the `#ENTITY_GRAPH` `XxHash128` identity over the `Version/commits` content-addressed commit-DAG substrate.
- Unlocks: cross-session, cross-package, and cross-runtime by-reference reuse where `compute`/`data`/`geometry`/`artifacts` outputs hit one durable ledger keyed by the same identity the in-session `Map[ContentKey, T]` keys, the in-session cache the hot path and the durable federation the cold-resolve tier.
- Anchors: the one `XxHash128` seed, `LanePolicy.cached`/`Keyed[T]`/`Map[ContentKey, T]`, `WireProtoCodec`, `transport/serve` invoke, `FEDERATED_PLAN`/`ENTITY_GRAPH`, the `Version/commits` content-addressed commit-DAG, and the `CONTENT_IDENTITY_PARITY` digest-parity gate the key trusts.
- Tension: the participant is by-reference resolution only — never a durable Python store — and the lookup misses correctly on a settings-drift key as the in-session cache does; this is distinct from the `csharp:Rasm.Persistence` `[REUSE_WIRE]` data-lane reuse-ledger pairing, and inherits `CONTENT_IDENTITY_PARITY` plus the `PYTHON_COMPANION_SERVES_WIRE` `SERVE` leg.
- Ripple: `python:runtime` `[CONTENT_REUSE_FEDERATION_WIRE]` and `csharp:Rasm.Persistence` `[REUSE_WIRE]` — the runtime card owns the by-reference wire participant and disclaims durable storage; this seam binds it to the C# durable federation owner, distinct from the data-lane reuse pairing.

[ONE_WIRE_FIXTURE_CORPUS]-[QUEUED]: one content-addressed golden-fixture corpus every runtime's parity harness reads.
- Capability: one content-addressed golden-fixture corpus carries the frozen byte facts every runtime parity harness reads.
- Shape: producers `csharp:Rasm.Compute/Runtime/codecs#CONTENT_ADDRESSING` and `csharp:Rasm/Geometry/Spatial/reconciliation#CANONICAL_BYTE_IDENTITY` feed consumers `python:runtime/evidence/identity#IDENTITY`, `typescript:interchange/Codec/frame#CONTENT_HASHING`, `typescript:testing/`, and the C# shared test corpus.
- Unlocks: `CONTENT_IDENTITY_PARITY`, `FAULT_WIRE_ROUNDTRIP`, and `TRI_LANGUAGE_WIRE_PARITY` share one corpus instead of re-deriving fixtures per runtime.
- Anchors: the one `XxHash128` seed, the eight-primitive clash bytes, 52-byte `0x9462A71A5DD13DCFA3B1D6D225FCBE70` adjacency digest, `FaultDetail` triples, CRDT op-set, GLB-by-key, HLC two-half stamps, and corpus index `csharp:Rasm/Geometry/Spatial/reconciliation#ONE_WIRE_FIXTURE_CORPUS`.
- Tension: `CANONICAL_BYTE_IDENTITY` is host-validated REAL and frozen; `CLASH_GOLDEN`, `FAULT_TRIPLES`, `CRDT_OP_SET`, `GLB_BY_KEY`, and `HLC_TWO_HALF` stay DESIGN-PIN until each producer pins its byte-deriving input, with `csharp:Rasm/Geometry/Spatial/index#CLASH_GOLDEN` as the open pin.

[WEB_GEOMETRY_RESIDENCY_WIRE]-[BLOCKED]: bind the WebGPU residency manifest as the second geometry wire shape.
- Capability: the content-keyed splat/meshlet residency manifest is the second geometry wire shape for WebGPU raster paths beyond triangulated GLB-per-tile.
- Shape: producer `csharp:Rasm.AppUi/Render/viewport#TS_PROJECTION` feeds consumers `typescript:platform/Transport/decode#RESIDENCY_DECODE` and `typescript:ui/render/glb#GLB_VIEWPORT`, with the Python SPZ/SOG splat-decode companion bound as the payload source.
- Unlocks: AppUi mints the manifest once, the TS worker decodes it, and `ui/glb-viewport` consumes by reference while the single-hash-mint invariant holds at `platform/worker/`.
- Anchors: manifest mint+decode is complete for `:x32` content keys, `[x,y,z,r]` bounds tuple, grouped `ViewpointWire` camera, `uint?` color, `ContentKeyHex` `Schema.transform`, and the `interchange` `ContentKey` brand; the splat leg inherits `ONE_CONTENT_IDENTITY` parity.
- Tension: residuals stay on splat-payload decode (`[PYTHON_PAYLOAD_DECODE: SOG/PLY/LAZ]`) and WebGPU cluster-LOD upload (`[HOST_PROBE_DEFERRED: live WebGPU device]`).

[ONE_DISTRIBUTED_TRACE]-[QUEUED]: propagate one W3C trace-context frame through browser, C#, and Python work.
- Capability: one W3C trace-context frame is injected and extracted across browser, C#, and Python work so one trace spans a browser interaction, C# solve, and Python tessellation.
- Shape: producer `csharp:Rasm.AppHost/Observability/telemetry#CORRELATION_SPINE` feeds consumers `python:runtime/observability/receipts#RECEIPT` and `typescript:platform/Observability/telemetry#METRIC_REGISTRY`.
- Unlocks: collector traces read end-to-end instead of as three disconnected process roots; the same trace also carries GenAI/MCP spans.
- Anchors: `traceparent`, `tracestate`, `TraceContextPropagator`, `BaggagePropagator`, `Propagators.DefaultTextMapPropagator`, inbound gRPC metadata extraction, grpc-web response-header continuation, `gen_ai.*`, and MCP tool span conventions.
- Tension: peers inject their active span today but do not extract the C# parent across the wire; this seam is independent of the `xxhash` parity gate because propagation context, not content seed, is the carrier.

[ONE_HEALTH_DEGRADATION_WIRE]-[QUEUED]: consume the C# health-degradation wire in TypeScript availability gates.
- Capability: the C#-minted `HealthSnapshot`, `DegradationLevel`, and alert wire become the one capability-health frame TypeScript reads to gate UI exposure and command dispatch.
- Shape: producer `csharp:Rasm.AppHost/Observability/health#TS_PROJECTION` feeds consumers `typescript:projection/evidence/availability#AVAILABILITY_GATE` and `typescript:ui/render/dashboard#LIVE_BINDING_DASHBOARD`.
- Unlocks: a degraded capability can keep serving while browser commands, node routing, and operator dashboards read one retained-capability and degradation-level fact.
- Anchors: `grpc.health.v1`, C# `WireHealth`, usable-failure degradation law, dial-time `isEnabled` at the one `interchange:CommandGateway` egress, and the existing TS availability gate.
- Tension: the health wire is minted C#-side with a TS projection, but the master seam still must bind it as the TS availability gate's upstream input; a second health model or TS-side level derivation is drift.

[ONE_AGENT_TOOL_PROJECTION]-[QUEUED]: project C# capability descriptors into one MCP tool catalog.
- Capability: C# capability descriptors project to one MCP tool catalog with effect hints, cost preview, progress, and resume token decoded by the TS agent runtime and served by the Python companion.
- Shape: producer `csharp:Rasm.AppHost/Agent/mcp#TS_PROJECTION` feeds consumer `typescript:services/agent/mcp#MCP_TRANSPORT`, with `python:runtime/transport/serve#SERVE` serving the same tools.
- Unlocks: the host JSON Schema remains the validating authority, TS lifts decoded tools into `@effect/ai` `Tool.make` with one host `payload`, cost/progress/resume decode once, and no branch mints a static parameter mirror.
- Anchors: `CommandAIFunction : AIFunction`, `McpServerTool.Create(AIFunction, McpServerToolCreateOptions)`, dry-run cost, SSE progress, `Last-Event-ID`, `ToolResult`, `ReceiptEnvelopeWire<ToolResultWire>`, `McpToolWire`, `CostPreviewWire`, `ProgressNotificationWire`, `ResumeTokenWire`, `ReceiptEnvelopeCarrier`, and shared `CAPABILITY_SDK_CODEGEN` descriptor source.
- Tension: the C# producer mints SDK-native `tools/list`/cost/progress projection while TS names decoded shapes; the agent-tool wire is distinct from bare SDK codegen and must stay one decode surface.

[TRI_LANGUAGE_WIRE_PARITY]-[BLOCKED]: live tri-language decode parity closes the finalized C# wire across TS web and Python companion consumers.
- Capability: TS web and the Python companion decode the finalized C# wire field-for-field against multi-runtime receipts, with content and causal seeds reproduced bit-identically.
- Shape: the parity seam spans every shared owner: CRDT amendment, generated SDK, agent-tool catalog, distributed-trace context, health-degradation wire, content identity, and causal identity.
- Unlocks: this is the final cross-language close-out before implementation because every branch proves it decodes the same wire rather than an inferred local copy.
- Anchors: every branch and folder task that finalizes a shared owner, plus `CONTENT_IDENTITY_PARITY`, `CAUSAL_TENANT_IDENTITY_WIRE`, `CRDT_OPLOG_WIRE_AMENDMENT`, `CAPABILITY_SDK_CODEGEN`, `ONE_AGENT_TOOL_PROJECTION`, `ONE_DISTRIBUTED_TRACE`, and `ONE_HEALTH_DEGRADATION_WIRE`.
- Tension: blocked until all branch and folder producer/consumer tasks land.

[TOOLCHAIN_EVIDENCE_AUDIT]-[QUEUED]: cross-libs core records per-folder native capability and extension evidence against the one manifest law and Forge extension catalog.
- Capability: the cross-libs core records current per-folder native capability evidence: `data` owns `flox` and the `substrait` DuckDB community extension, `artifacts` consumes Forge-provided `libvips`/`leptonica`/`tesseract`/`ghostscript` for `pyvips`/`ocrmypdf`, and `pdal` stays the `geometry` point-cloud filter-graph owner rather than a data admission.
- Shape: an audit row in `planning-targets.md` records per-folder native capability evidence and the Forge DuckDB-extensions `substrait` catalog row, keeping centralization absolute as the one Python manifest carries every dependency and `zstandard` stays the `tools.assay` dependency rather than a data-corpus admission.
- Anchors: `libs/.planning/planning-targets.md`, the one Python manifest band law, the Forge DuckDB-extensions catalog substrait row, `scientific-tools.nix`, and the `pdal`-stays-geometry boundary.
- Ripple: `data` `[SUBSTRAIT_PORTABILITY]` and `[FLOX_ADMIT]` — the data folder already admits `flox` and the duckdb-substrait extension; this core audit row mirrors those landed admissions plus the `artifacts` Forge native-capability paragraph so cross-libs evidence matches the per-folder owner law.
- Atomic: one `planning-targets.md` audit row recording per-folder native capability evidence and the substrait extension row.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

(none)
