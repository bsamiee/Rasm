# [CROSS_LIBS_TASKLOG]

Open and closed cross-language tasks — work that names more than one of C# / Python / TypeScript. Per-language and per-folder work lives in the branch and folder `TASKLOG.md`. Each task card carries a status marker plus the packages, integration points, and considerations that scope it; one cross-`libs/` idea spawns one or more tasks.

## [1]-[OPEN]

[TS_CONSUMES_CSHARP_WIRE] [QUEUED]:
- TypeScript decodes the C# wire contracts only — the remote-lane TS-projection method shapes, the tenant-context wire, and W3C trace context — with no C# interior coupling.
- Integration: `csharp:Compute/remote-lane#TRANSPORT_AXIS` to `typescript:interchange/transport`.
- Considerations: the wire is the only coupling; a host-local C# surface never appears in the TS consumed set.

[PYTHON_COMPANION_SERVES_WIRE] [QUEUED]:
- The Python `runtime/server-host` companion serves the existing C# `ComputeService`/`ArtifactSync` gRPC over the UDS transport leg; `ContentIdentity` reproduces the C# `XxHash128` seed; data/artifact bundles and graduation evidence cross the offline seam.
- Integration: `csharp:Compute/remote-lane#TRANSPORT_AXIS` to `python:runtime/server-host` and `python:runtime/content-identity`.
- Considerations: never reach a C# interior; reproduce the seed bit-identically.

[CRDT_OPLOG_WIRE_AMENDMENT] [QUEUED]:
- The op-log CRDT-op union is a breaking amendment to the one wire vocabulary (LWW survives only as the register arm); the TS-web codec leg and the Python companion decode the amended payload.
- Integration: `csharp:AppHost/runtime-ports#WIRE_LAW` to `typescript:interchange/codec-rails` and `python:runtime/server-host`.

[CAPABILITY_SDK_CODEGEN] [QUEUED]:
- One capability-descriptor source emits the C#/TS/Python SDKs and the MCP projection; the sibling branches consume the generated SDK, never a per-service hand-written client.
- Integration: `csharp:AppHost/capability-registry#SDK_CODEGEN` to `typescript:interchange/transport` and `python:runtime/server-host`.

[TRI_LANGUAGE_WIRE_PARITY] [BLOCKED]:
- Live tri-language decode parity: the TS-web leg and the Python companion decode the finalized C# wire — including the CRDT amendment and the generated SDK — field-for-field against multi-runtime receipts, with the content seed reproduced bit-identically.
- Blocked on every branch and folder task finalizing first; this is the final cross-language close-out before implementation.

## [2]-[CLOSED]

[GEOMETRYCORE_PACKAGE_CANDIDATE] [DROPPED]:
- A standalone robust-geometry package was considered and dropped: the concern folds into the C# `Rasm` kernel as the `Geometry/` robust-core sub-domain, and the units-boundary interior-double contradiction is ratified there as a sanctioned filter-then-exact interior exception.
