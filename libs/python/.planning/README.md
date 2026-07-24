# [PYTHON_BRANCH]

Python is the platform's offline evaluation and production half — host-free science, compute, data, geometry, and IFC capability across peer packages held to the cross-language density bar. Every result crosses to C# as content-keyed wire data, requested and re-imported, never imported as code.

One root `pyproject.toml` owns interpreter admission and dependency groups; this branch registry owns the substrate tiers every folder composes.

## [01]-[ROUTER]

- [01]-[RUNTIME](../runtime/README.md): shared-rail minting foundation.
- [02]-[COMPUTE](../compute/README.md): scientific-evidence graduation.
- [03]-[DATA](../data/README.md): dataset movement and interchange.
- [04]-[GEOMETRY](../geometry/README.md): geometry and IFC evidence production.
- [05]-[ARTIFACTS](../artifacts/README.md): publication-grade output.

## [02]-[SUBSTRATE_PACKAGES]

Every folder composes this cross-domain foundation. Each runtime-composable package carries one catalogue at the branch `libs/python/.api/<dist>.md`, folder overlays carry only local admission law, and the test tier is manifest-owned with no branch catalogue. Branch tier admits only the vendor-neutral surface every folder imports; composition-root machinery — the `opentelemetry-instrumentation-*` train, the `pyroscope-otel` push — homes folder-local to `runtime`.

[TYPING_RAILS]:
- `expression` — `Result`/`Option` carriers, do-notation builders, `pipe`/`compose`, and `Block`/`Map` immutable traversal.
- `msgspec` — `Struct` wire codecs, `Meta` constraints, and the `convert` rename projection.
- `beartype` — runtime boundary contracts, `vale` refinements, and `door` predicates.
- `pydantic` — untrusted-ingress admission models and the `TypeAdapter` payload gate.

[CONCURRENCY]:
- `anyio` — Structured-concurrency surface: task groups, cancel scopes, offload arms, memory streams, portal bridge.
- `trio` — Backend `anyio` runs on and the deterministic test kit; a runtime selection, never a code change.
- `cloudpickle` — Ships closures, lambdas, and module-local kernels across the worker process and subinterpreter seams stdlib pickle refuses.
- `tblib` — Carries worker-side traceback frames across the pickle seam, so a crossed exception re-raises with its true origin.
- `loky` — Owns the warm reusable crash-respawning process pool behind the worker fabric's `process` kind.
- `pebble` — Owns terminal deadline enforcement: a wall-clock timeout kills the worker mid-kernel and reclaims the slot.

[OBSERVABILITY]:
- `structlog` — Processor-chain structured logging; the branch's in-process log face.
- `opentelemetry-api` — Vendor-neutral tracer/meter/propagation surface; the only OTel import a library makes.
- `opentelemetry-sdk` — Provider, `Resource`, processor, and reader wiring; composition roots alone touch it.
- `opentelemetry-exporter-otlp-proto-http` — OTLP HTTP+protobuf egress; the estate default transport.
- `psutil` — Whole-process accounting batched through one `Process.oneshot` collection.

[NUMERIC_SUBSTRATE]:
- `numpy` — Dense `float64` array substrate every numeric route factors through.
- `xarray` — Labelled N-D array algebra over `numpy`; gridded datasets and dimensioned reductions ride it.

[GRAPH_SUBSTRATE]:
- `networkx` — Graph payload classes, conversion bridges, and algorithm families over directed, undirected, and multi-edge graphs.

[IDENTITY]:
- `xxhash` — Content-key hashing beneath the runtime `ContentKey` minting.

[TRANSPORT]:
- `fsspec` — Filesystem abstraction every remote and local byte access resolves through.
- `obstore` — Rust object-store client: S3/GCS/Azure byte-range reads, puts, and listing.
- `universal-pathlib` — `UPath` path objects over every fsspec backend; one cross-store path currency.

[MESH_INTERCHANGE]:
- `meshio` — Neutral mesh read/write across solver formats.

[COMPRESSION]:
- `lz4` — Frame and block compression for wire and cache payloads.

[WIRE_CODEGEN]:
- `grpcio` — Channel and server runtime beneath the serve rail.
- `grpcio-tools` — `protoc` invocation surface minting the generated stubs.
- `protobuf` — Message runtime beneath the stubs; well-known types and `json_format`.

[TEST_SUBSTRATE]:
- `pytest` — Spec runner; its plugin roster rides the root manifest.
- `hypothesis` — Property-based generation and shrinking.
- `inline-snapshot` — Inline expected-value snapshots updated in place.
- `coverage` — Branch coverage measurement.
- `mutmut` — Mutation testing over the spec suite.
