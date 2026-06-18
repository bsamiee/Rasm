# [PY_RUNTIME]

`runtime` is the host-free execution foundation every `libs/python` sibling composes. It mints the shared value shapes once and references no sibling: the single content-identity owner, the one boundary-fault and Result/Option rail, the one resilience policy, caller-owned context and settings admission, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime and credential axis, external-API and structural-parsing evidence, and the private daemon entrypoint grammar. This README routes the design pages and lists the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, `TASKLOG.md` the open work.

## [1]-[ROUTER]

The design pages under `.planning/`, grouped by sub-domain.

- identity: [content-identity](.planning/identity/content-identity.md)
- reliability: [faults](.planning/reliability/faults.md), [resilience](.planning/reliability/resilience.md)
- context: [admission](.planning/context/admission.md)
- resources: [roots](.planning/resources/roots.md)
- concurrency: [lanes](.planning/concurrency/lanes.md)
- observability: [receipts](.planning/observability/receipts.md)
- server: [serve](.planning/server/serve.md)
- evidence: [evidence](.planning/evidence/evidence.md)

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions live in the one root manifest; this list carries no pin. The gRPC stack splits by provenance: the `grpc.aio` runtime leg (`grpcio`, `protobuf`) resolves transitively on the cp315 core through `specklepy`, so the `ServerHost` serve leg sits on the core; only `grpcio-tools` (the `protoc` codegen compiler) is companion-lane-only on the Forge `<'3.13'` interpreter (python312) and is NOT declared in the cp315 `pyproject.toml`. Every other row is a cp315-clean manifest dependency.

- pydantic-settings
- xxhash
- stamina
- fsspec
- s3fs
- gcsfs
- obstore
- universal-pathlib
- httpx
- asyncssh
- specklepy
- watchfiles
- apscheduler
- cyclopts
- tree-sitter
- tree-sitter-python
- tree-sitter-typescript
- keyring

## [3]-[CROSS_CUTTING]

Branch-wide infrastructure packages this folder consumes; canonical registry lives at `libs/python/.api/`.

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
