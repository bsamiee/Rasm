# [PY_RUNTIME]

`runtime` is the host-free execution foundation every `libs/python` sibling composes. It mints the shared value shapes once and references no sibling: the single content-identity owner reproducing the C# `XxHash128` seed bit-identically, the one boundary-fault and Result/Option rail, the one resilience policy, caller-owned context and settings admission, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime decoding the C#-owned protobuf wire, the MessagePack op-log delta, and the capability-descriptor SDK, external-API and structural-parsing evidence, and the private daemon entrypoint grammar. The companion owns no wire vocabulary; it decodes the C#-minted shapes (single-mint invariant). This README routes the design pages and lists the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, `TASKLOG.md` the open work.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- identity: [content-identity](.planning/identity/content-identity.md)
- reliability: [faults](.planning/reliability/faults.md), [resilience](.planning/reliability/resilience.md)
- context: [admission](.planning/context/admission.md)
- resources: [roots](.planning/resources/roots.md)
- concurrency: [lanes](.planning/concurrency/lanes.md)
- observability: [receipts](.planning/observability/receipts.md), [metrics](.planning/observability/metrics.md)
- server: [serve](.planning/server/serve.md)
- evidence: [evidence](.planning/evidence/evidence.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions live in the one root manifest; this list carries no pin. The gRPC stack splits by provenance: the `grpc.aio` runtime leg (`grpcio`, `protobuf`) resolves transitively on the cp315 core through `specklepy`, so the `ServerHost` serve leg sits on the core; only `grpcio-tools` (the `protoc` codegen compiler) is companion-lane-only on the Forge `<'3.13'` interpreter (python312) and is NOT declared in the cp315 `pyproject.toml`. Two rows ride the `python_version<'3.15'` companion band with no cp315 wheel synced: `xxhash` (content-identity digest) and `lz4` (the op-log `Lz4BlockArray` decompression at `server/serve#CRDT_DECODE`); the MessagePack op-log decode through `msgspec.msgpack` is cp315-clean and the LZ4 decompression is the single install-gated leg. Every other row is a cp315-clean manifest dependency.

- pydantic-settings
- xxhash
- lz4
- stamina
- obstore
- httpx
- asyncssh
- watchfiles
- apscheduler
- cyclopts
- opentelemetry-instrumentation-grpc
- tree-sitter
- tree-sitter-python
- tree-sitter-typescript

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes. The shared-runtime registry lives at `libs/python/.api/`; the resource-root transport family is owned by `runtime` at `resources/roots#ResourceRoot` and catalogued in this folder's `.api/`.

[SHARED_RUNTIME]:
- expression
- beartype
- msgspec
- pydantic
- anyio
- structlog
- psutil
- opentelemetry-api
- opentelemetry-sdk
- opentelemetry-exporter-otlp-proto-http
- grpcio
- grpcio-tools
- protobuf

[RESOURCE_ROOTS]:
- fsspec
- s3fs
- gcsfs
- universal-pathlib
