# [PYTHON_BRANCH]

The Python branch is a first-class host-free science, compute, data, geometry, and IFC library of five peer packages, held to the cross-language density bar in idiomatic modern Python. The root `pyproject.toml` owns package admission and dependency groups. This file routes the branch index docs and registers the packages shared across folders.

## [01]-[ROUTER]

- [01]-[ARCHITECTURE](ARCHITECTURE.md): Domain map
- [02]-[IDEAS](IDEAS.md): cross-package Python concert — ideas coupling the packages to each other, distilled from the per-folder ideas.
- [03]-[TASKLOG](TASKLOG.md): cross-package open work.
- [04]-[RUNTIME](../runtime/README.md)
- [05]-[COMPUTE](../compute/README.md)
- [06]-[DATA](../data/README.md)
- [07]-[GEOMETRY](../geometry/README.md)
- [08]-[ARTIFACTS](../artifacts/README.md)

## [02]-[SUBSTRATE_PACKAGES]

The cross-domain Python foundation every folder builds on: typing/rails, concurrency, observability, the numeric substrate, content identity, byte/array compression, the wire-codegen toolchain, and the test stack. Package versions live in the root manifest. Folder READMEs list these under their own `## [3]-[SUBSTRATE_PACKAGES]` section rather than duplicating the registry here.

The typing/rails, concurrency, observability, numeric-substrate, identity, transport, mesh-interchange, compression, and companion-wire tiers each carry one catalogue at the branch `libs/python/.api/<dist>.md`, authored once and never duplicated into a folder `.api/`. Folder overlays carry only local admission law.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`
- `trio` - Structured-concurrency backend `anyio` runs on; a runtime selection, never a code change.

[OBSERVABILITY]:
- `structlog`
- `opentelemetry-api`
- `opentelemetry-sdk`
- `opentelemetry-exporter-otlp-proto-http`
- `psutil`

[NUMERIC_SUBSTRATE]:
- `numpy`

[IDENTITY]:
- `xxhash`

[TRANSPORT]:
- `fsspec`
- `obstore`

[MESH_INTERCHANGE]:
- `meshio`

[COMPRESSION]:
- `lz4`

[WIRE_CODEGEN]:
- `grpcio`
- `grpcio-tools`
- `protobuf`

[TEST_SUBSTRATE]:
- `pytest` — with its plugin set
- `hypothesis`
- `inline-snapshot`
- `coverage`
- `mutmut`
