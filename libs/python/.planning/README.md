# [PYTHON_BRANCH]

Python is the platform's offline evaluation and production half — host-free science, compute, data, geometry, and IFC capability across peer packages held to the cross-language density bar. `runtime` mints the shared value shapes; `data` moves every dataset the platform touches; `geometry` tessellates, verifies, and evaluates as an independent peer producer; `compute` graduates scientific evidence; `artifacts` lands publication-grade output — and every result crosses to C# as content-keyed wire data, requested and re-imported, never imported as code.

One root `pyproject.toml` owns interpreter admission and dependency groups; this branch registry owns the substrate tiers every folder composes.

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

Every folder composes this cross-domain foundation. One root manifest owns versions; each tier carries one catalogue at the branch `libs/python/.api/<dist>.md`, and folder overlays carry only local admission law.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`
- `trio` — Structured-concurrency backend `anyio` runs on; a runtime selection, never a code change.
- `cloudpickle` — Ships closures, lambdas, and module-local kernels across the worker process and subinterpreter seams stdlib pickle refuses.
- `tblib` — Carries worker-side traceback frames across the pickle seam, so a crossed exception re-raises with its true origin.
- `loky` — Owns the warm reusable crash-respawning process pool behind the worker fabric's `process` kind.
- `pebble` — Owns terminal deadline enforcement: a wall-clock timeout kills the worker mid-kernel and reclaims the slot.

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
