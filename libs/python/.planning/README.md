# [PYTHON_BRANCH]

The Python branch is a first-class host-free science/compute/data/geometry/IFC library of five peer packages, held to the cross-language density bar in idiomatic modern Python. The single root `pyproject.toml` owns the `python>=3.15` project and all dependency groups. The Forge companion lane owns the `<'3.13'` native geometry/IFC cores and the `grpcio-tools` codegen compiler outside that manifest, while the `grpc.aio` runtime (`grpcio` and `protobuf`) resolves transitively on the cp315 core through `specklepy`. This file routes the branch index docs and registers the packages shared across folders.

## [01]-[ROUTER]

- [01]-[ARCHITECTURE](ARCHITECTURE.md): five-package domain map, dependency direction (runtime mints shared shapes; the four consumers compose them and never re-mint), and the interpreter floor.
- [02]-[IDEAS](IDEAS.md): cross-package Python concert — ideas coupling the packages to each other, distilled from the per-folder ideas.
- [03]-[TASKLOG](TASKLOG.md): cross-package open work, including the companion-environment floor gate.
- [04]-[RUNTIME](../runtime/README.md)
- [05]-[COMPUTE](../compute/README.md)
- [06]-[DATA](../data/README.md)
- [07]-[GEOMETRY](../geometry/README.md)
- [08]-[ARTIFACTS](../artifacts/README.md)

## [02]-[SUBSTRATE_PACKAGES]

The cross-domain Python foundation every folder builds on: typing/rails, concurrency, observability, the numeric substrate, content identity, byte/array compression, the wire-codegen toolchain, and the test stack. Root-compatible package versions live in the root manifest; companion-floor rows carry no pin here. Folder READMEs list these under their own `## [3]-[SUBSTRATE_PACKAGES]` section rather than duplicating the registry here.

The typing/rails, concurrency, observability, numeric-substrate, identity, compression, and companion-wire tiers each carry one catalogue at the branch `libs/python/.api/<dist>.md`, authored once and never duplicated into a folder `.api/`. Domain-owned packages (geometry, data/geometry seams, runtime/data transport) keep their catalogues in the consuming folder, not here.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`
- `trio` (the alternate structured-concurrency backend `anyio` runs on; a runtime selection, never a code change)

[OBSERVABILITY]:
- `structlog`
- `opentelemetry-api`
- `opentelemetry-sdk`
- `opentelemetry-exporter-otlp-proto-http`
- `psutil`

[NUMERIC_SUBSTRATE]:
- `numpy`

[COMPRESSION]:
- `zlib-ng` (drop-in accelerated `zlib`/`gzip`/`deflate`/`crc32`)
- `numcodecs` (chunked-array compression + filter codec registry)

[IDENTITY]:
- `xxhash`

[WIRE_CODEGEN]:
- `grpcio` (`grpc.aio` runtime — resolves transitively on the cp315 core through `specklepy`)
- `grpcio-tools` (`protoc` codegen compiler — companion-lane-only on the Forge `<'3.13'` interpreter; only proto codegen assumes the companion interpreter)
- `protobuf`

[TEST_SUBSTRATE]:
- `pytest` (+ its plugins)
- `hypothesis`
- `inline-snapshot`
- `coverage`
- `mutmut`
