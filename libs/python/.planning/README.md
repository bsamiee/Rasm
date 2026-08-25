# [PYTHON_BRANCH]

Python is an independently adoptable host-free platform for science, compute, data, geometry, IFC, and artifact production across peer packages held to the cross-language density bar. Python applications originate and operate these capabilities from the branch package graph alone; polyglot applications exchange contract-conforming artifacts at app roots, requested and re-imported as data, never imported as code.

Root `pyproject.toml` is the virtual workspace root — no `[project]` table — owning the Python 3.15 platform, the estate dependency group, and every gate table; each folder carries one member manifest declaring its distribution identity and bare-name edges, and `uv.lock` fixes every version. Native distributions whose wheels stop below the platform floor ride the Forge python-overlay `.pth`, and their marker row states that mechanism at the root manifest.

This branch registry owns the substrate tiers every folder composes. FLOOR-GATED marks a folder registry row whose `python_version` marker no supported interpreter satisfies, so admission stands while reach waits on a floor wheel or a floor move; OVERLAY marks the inverse, where the Forge python-overlay `.pth` supplies the module worker-side at the interpreter floor; rows naming host binaries carry no manifest row and state their provisioning lane.

## [01]-[ROUTER]

- [01]-[RUNTIME](../runtime/README.md): Shared-rail minting foundation.
- [02]-[COMPUTE](../compute/README.md): Scientific-evidence graduation.
- [03]-[DATA](../data/README.md): Dataset movement and interchange.
- [04]-[GEOMETRY](../geometry/README.md): Geometry and IFC evidence production.
- [05]-[ARTIFACTS](../artifacts/README.md): Publication-grade output.
- [06]-[CAD](../cad/README.md): Exact solid modeling and neutral CAD exchange behind generated `CadService`.

## [02]-[SUBSTRATE_PACKAGES]

Every folder composes this cross-domain foundation. Each runtime-composable package carries one catalogue at the branch `libs/python/.api/<dist>.md`, folder overlays carry only local admission law, and the test tier is manifest-owned with no branch catalogue. Branch tier admits only the vendor-neutral surface every folder imports; composition-root machinery, the `opentelemetry-instrumentation-*` train and the `pyroscope-otel` push, homes folder-local to `runtime`.

[TYPING_RAILS]:
- `expression` — `Result`/`Option` carriers, do-notation builders, `pipe`/`compose`, and `Block`/`Map` immutable traversal.
- `msgspec` — `Struct` wire codecs, `Meta` constraints, and the `convert` rename projection.
- `beartype` — Runtime boundary contracts, `vale` refinements, and `door` predicates.
- `pydantic` — Untrusted-ingress admission models and the `TypeAdapter` payload gate.

[CONCURRENCY]:
- `anyio` — Structured-concurrency surface: task groups, cancel scopes, offload arms, memory streams, portal bridge.
- `trio` — Backend `anyio` runs on and the deterministic test kit; a runtime selection, never a code change.
- `cloudpickle` — Ships closures, lambdas, and module-local kernels across the worker process and subinterpreter seams stdlib pickle refuses.
- `tblib` — Carries worker-side traceback frames across the pickle seam, so a crossed exception re-raises with its true origin.
- `loky` — Owns the warm reusable crash-respawning process pool behind the worker crossing's `process` kind.
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

[EVENT_FABRIC]:
- `cloudevents` — Specification attribute algebra, its validating event family, the JSON format, and the protocol bindings.
- `confluent-kafka` — librdkafka client, cluster administration, and the Schema Registry stack with its magic-byte framing.
- `nats-py` — NATS core and JetStream client: subject addressing, headers, streams, and the KV and object-store bucket families.
- `paho-mqtt` — MQTT protocol state machine, its 5.0 property vocabulary, reason codes, and topic-filter matching.
- `pika` — AMQP 0-9-1 protocol: the blocking channel, the content-header vocabulary, topology verbs, and publisher confirms.
- `fastavro` — Avro container and schemaless codecs, schema parse and fingerprinting, and logical-type dispatch.
- `jsonschema` — Foreign JSON Schema validation across six draft validators, the payload gate beneath the registry JSON serializer.

[MESH_INTERCHANGE]:
- `meshio` — Neutral mesh read/write across solver formats.

[COMPRESSION]:
- `lz4` — Frame and block compression for wire and cache payloads.

[WIRE_CODEGEN]:
- `protobuf-py` — Message runtime beneath the generated `_pb.py` bindings; binary and JSON codecs, descriptors, and the `wkt` carriers.
- `connectrpc` — Connect, gRPC, and gRPC-Web service protocols, ASGI applications, interceptors, and typed clients over the generated stubs.
- `protobuf` — Google message runtime beneath the Substrait plan IR and the ONNX model IR.

[TEST_SUBSTRATE]:
- `pytest` — Spec runner; its plugin roster rides the root manifest.
- `hypothesis` — Property-based generation and shrinking.
- `inline-snapshot` — Inline expected-value snapshots updated in place.
- `coverage` — Branch coverage measurement.
- `mutmut` — Mutation testing over the spec suite.
