# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — the wire seams that span two or more of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`; this node carries only the seam each branch consumes at the boundary, never a re-aggregation of branch work. Each task names its producer and consumer touchpoints in `lang:pkg/page#CLUSTER` notation plus the considerations that scope it; a closed task compacts to one or two lines.

## [1]-[OPEN]

[TS_CONSUMES_CSHARP_WIRE] [QUEUED]:
- The TS interchange tier decodes the C# wire contracts only — the suite proto method shapes, the tenant-context wire, and W3C trace context — with no C# interior coupling, selecting the grpc-web transport for the live legs.
- Producer `csharp:Rasm.Compute/remote/channels#PROTO_VOCABULARY` to consumer `typescript:interchange/transport#PROTOCOL_SELECTION_AXIS`.
- Considerations: the wire is the only coupling; a host-local C# surface never appears in the TS consumed set, and the capability tuple is read from the upstream wire, never invented branch-side. The four-signal telemetry lake stays `csharp:Rasm.AppHost/observability` owned across the wire; the browser reads the collector as one more decoded feed and never mints a second telemetry surface.

[PYTHON_COMPANION_SERVES_WIRE] [QUEUED]:
- The Python `runtime/server/serve` companion serves the existing C# `ComputeService`/`ArtifactSync` gRPC over the UDS/InProcess transport leg; `ContentIdentity` reproduces the C# `XxHash128` seed; data/artifact bundles and graduation evidence cross the offline seam.
- Producer `csharp:Rasm.Compute/remote/channels#PROTO_VOCABULARY` to consumers `python:runtime/server/serve#SERVE` and `python:runtime/identity/content-identity#IDENTITY`.
- Considerations: never reach a C# interior; reproduce the seed bit-identically; the companion mints no second wire vocabulary. Blocked downstream on the Python sub-3.15 companion-floor admission gate carried on the Python branch `TASKLOG.md`.

[TESSELLATION_RAIL_TRILANG] [QUEUED]:
- The two-hop `IFC → IfcOpenShell → GLB` (and the AP242 CAD-STEP `STEP → OCCT → GLB` companion) is Python-native and serves the one GLB every runtime consumes: C# builds the tessellation request and content-addresses the result, Python serves the daemon, TS renders the GLB by the same content key; meshing has one owner per runtime (Rasm-DEC, Py-scan, TS-render) and IFC one semantic owner per runtime, meeting only at the content-keyed GLB.
- Producers `python:geometry/tessellation/daemon#TESSELLATE` (serving over the `python:runtime/server/serve#SERVE` companion contract) and `csharp:Rasm.Bim/exchange/interchange#TESSELLATION_REQUEST` (request build) to consumers `csharp:Rasm.Compute/interchange/codecs#TILE_PARTITION` (content-address the result) and `typescript:ui/viewport/glb-viewport#GLB_VIEWPORT` via `typescript:interchange/artifacts/frame-reassembly#CONTENT_HASHING`.
- Considerations: one geometry-evaluation companion, no duplicate mesh pipeline per runtime; the per-tile `EXT_structural_metadata`/`EXT_mesh_features` schema carries the IFC class and field band on the same content key, so a re-tessellation at a new deflection re-keys geometry and metadata together. The result is keyed by the one `ContentIdentity` seed, so this seam trusts the content-identity parity precondition; the CAD-STEP hop rides the same `SourceFormat`-discriminated request, never a second daemon. Blocked downstream on the Python companion-floor admission gate carried on the Python branch `TASKLOG.md` and the TS mesh-shape promotion carried on the TS `ui` `TASKLOG.md`.

[CONTENT_IDENTITY_PARITY] [QUEUED]:
- Prove the `XxHash128` content-address seed reproduces byte-identically across the three runtimes against the one C#-owned seed, including the HLC two-64-bit-half order a logical off-by-one-half would corrupt.
- Producer `csharp:Rasm.Compute/interchange/codecs#CONTENT_ADDRESSING` to consumers `python:runtime/identity/content-identity#IDENTITY` and `typescript:interchange/artifacts/frame-reassembly#CONTENT_HASHING`.
- Considerations: the seed (seed zero, two-half order) is the single source; the TS leg resolves a 128-bit-capable wasm hash or downgrades the C#-owned seed to XXH64; the Python leg consumes `xxh3_128_intdigest`. The seam is the precondition the off-thread digest and the convergence presence fold both trust as cross-runtime.

[CRDT_OPLOG_WIRE_AMENDMENT] [QUEUED]:
- The op-log CRDT-op union is a breaking amendment to the one wire vocabulary (LWW survives only as the register arm); the TS-web codec leg and the Python companion decode the amended payload, never an additive parallel surface.
- Producer `csharp:Rasm.Persistence/versioning#CRDT_ALGEBRA` + `csharp:Rasm.Persistence/sync/collaboration#TS_PROJECTION` to consumers `typescript:projection/convergence/lww-merge#LWW_MERGE` and `python:runtime/server/serve#SERVE`.
- Considerations: the producer owns the op vocabulary; neither consumer authors an op kind the wire does not carry. The TS `projection` GraphFork arm is blocked on the upstream op landing on the wire; the Python decode rides the same companion proto, no second decoder.

[CAPABILITY_SDK_CODEGEN] [QUEUED]:
- One capability-descriptor source emits the C#/TS/Python SDKs and the MCP projection; the sibling branches consume the generated SDK, never a per-service hand-written client.
- Producer `csharp:Rasm.AppHost/capability/registry#SDK_CODEGEN` to consumers `typescript:interchange/transport#CODEGEN_TOOLING` and `python:runtime/server/serve#SERVE`.
- Considerations: the descriptor source is owned once in AppHost; `invoke` is one polymorphic method keyed by descriptor id, the `inputSchema` the generated per-descriptor JSON Schema. The TS consumer is blocked on the upstream descriptor source; the Python SDK-codegen consumption rides the companion server-host.

[FAULT_WIRE_ROUNDTRIP] [QUEUED]:
- The fault wire is a two-sided coherence contract: C# packs `(package, code, case)` with package-exclusive code bands (an enforced producer invariant) and TS reconstructs the wire triple losslessly. Bands are disjoint (ComputeFault 2200, HopFault 4500, WireFault 4520-4532, store/config at app roots), so package selects the family and code selects the arm; neither field alone is a total key.
- Producer `csharp:Rasm.Compute/remote/channels#FAULT_PROJECTION` `WireFault`/`FaultDetail` pack site to consumer `typescript:interchange/faults/fault-family` `faultTagOf`/`FAULT_CTOR`; NOT tri-language — Python mints `FaultDetail` outbound and is not a package-keyed decoder.
- Considerations: the C# pack site asserts code-in-band-of-package before emit so a misbanded code is a producer fault, never a wire ambiguity; the TS `FAULT_CTOR` already threads the `case` field and folds an unmapped package to `Quarantine`. The remaining cross-language obligation is the round-trip parity property asserting `(package, code, case)` reconstructs losslessly, added to both suites.

[CAUSAL_TENANT_IDENTITY_WIRE] [QUEUED]:
- The HLC two-half causal stamp and tenant context are minted once in C# and read by the peers: the Python receipt fan-in propagates the same causal/tenant frame inbound and the TS ordering fold reads the HLC band off the wire, neither re-minting a parallel stamp or tenant scheme.
- Producer `csharp:Rasm.AppHost/ports/runtime-ports#HLC_FANIN` to consumers `python:runtime/identity/content-identity#IDENTITY` (causal/tenant propagation on the receipt) and `typescript:projection/convergence#SKEW_ORDERING`.
- Considerations: the HLC two-64-bit-half order rides the same parity gate as the content seed, so the causal-identity parity is proven in the same multi-runtime fixture; the TS leg consumes the band as an ordering input (concurrent-uncertain rows), never a render-only leaf; a second causal stamp or tenant scheme in any peer is the named drift defect.

[GRADUATION_EVIDENCE_INWARD] [QUEUED]:
- The Python graduation rail is the single content-keyed contract every offline result crosses outward on; the C# determinism closure re-imports it into the hash-chained event log and references it by the one content key, never a per-result handoff.
- Producer `python:compute/graduation#HANDOFF` to consumer `csharp:Rasm.AppHost/determinism/determinism-and-replay#EVENT_LOG`.
- Considerations: the evidence is keyed by the one `ContentIdentity` seed (so this seam trusts the content-identity parity precondition); the surrogate fit crosses as a content-keyed ONNX artifact the C# side runs inference over, never an in-process training loop; Python holds the `HandoffAxis` rail singular so the C# owner consumes one rail, never a handoff family.

[TRI_LANGUAGE_WIRE_PARITY] [BLOCKED]:
- Live tri-language decode parity: the TS-web leg and the Python companion decode the finalized C# wire — including the CRDT amendment and the generated SDK — field-for-field against multi-runtime receipts, with the content and causal seeds reproduced bit-identically.
- Spans every shared owner; blocked on every branch and folder task finalizing first. This is the final cross-language close-out before implementation.

## [2]-[CLOSED]

[GEOMETRYCORE_PACKAGE_CANDIDATE] [DROPPED]:
- A standalone robust-geometry package was considered and dropped: the concern folds into the C# `Rasm` kernel as the `Geometry/` robust-core sub-domain, and the units-boundary interior-double contradiction is ratified there as a sanctioned filter-then-exact interior exception.
