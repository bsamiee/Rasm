# [PY_RUNTIME]

`runtime` is the host-free execution foundation every `libs/python` sibling composes. It mints the shared value shapes once and references no sibling: the single content-identity owner, the one boundary-fault and Result/Option rail, the one resilience policy, caller-owned context and settings admission, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime and credential axis, external-API and structural-parsing evidence, and the private daemon entrypoint grammar. This README routes the design pages and lists the external packages the folder uses; `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, `TASKLOG.md` the open work.

## [1]-[PAGE_ROUTER]

| [INDEX] | [SUB_DOMAIN]  | [PAGE]                                                      | [PAGE_OWNER]                       |
| :-----: | :------------ | :--------------------------------------------------------- | :--------------------------------- |
|   [1]   | identity      | [content-identity](.planning/identity/content-identity.md) | `ContentIdentity` / `ContentKey`   |
|   [2]   | reliability   | [faults](.planning/reliability/faults.md)                  | `BoundaryFault` / `RuntimeRail`    |
|   [3]   | reliability   | [resilience](.planning/reliability/resilience.md)          | `Retry`                            |
|   [4]   | context       | [admission](.planning/context/admission.md)                | `RuntimeContext` / `SettingsAdmission` |
|   [5]   | resources     | [roots](.planning/resources/roots.md)                      | `ResourceRoot` / `TransportResource`   |
|   [6]   | concurrency   | [lanes](.planning/concurrency/lanes.md)                    | `LanePolicy` / `StagePlan`         |
|   [7]   | observability | [receipts](.planning/observability/receipts.md)            | `Receipt` / `ReceiptContributor`   |
|   [8]   | server        | [serve](.planning/server/serve.md)                         | `ServerHost` / `Entrypoint`        |
|   [9]   | evidence      | [evidence](.planning/evidence/evidence.md)                 | `ApiPackage` / `Structural`        |

## [2]-[PACKAGES]

Every external library the folder uses, planned or implemented. Versions live in the one root manifest; this list carries no pin.

- expression
- beartype
- msgspec
- pydantic
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
- anyio
- watchfiles
- apscheduler
- structlog
- opentelemetry-api
- opentelemetry-sdk
- opentelemetry-exporter-otlp-proto-http
- psutil
- cyclopts
- tree-sitter
- tree-sitter-python
- tree-sitter-typescript
- grpcio
- grpcio-tools
- protobuf
- keyring
