# [PY_RUNTIME]

`runtime` is stratum zero of the Python branch — the standalone host-free execution foundation every sibling composes, one polymorphic owner per sub-domain minting Python's shared value shapes once. Siblings return through those mints instead of re-minting their own, and the module set stands alone: a Python application operates with no sibling installed and no foreign runtime or emitted bundle present.

## [01]-[ROUTER]

[OBSERVABILITY]:
- [01]-[RECEIPTS](.planning/observability/receipts.md): Receipt union, drain taxonomy, cost evidence, composition scope, and contributor port.
- [02]-[LOGGING](.planning/observability/logging.md): `LogPipeline` chain, OTLP wire projection, `LogShip` egress, and the terminal doors.
- [03]-[METRICS](.planning/observability/metrics.md): `Metrics` spine — `INSTRUMENTS` census, `MEASURES` admission, views, occupancy, and the train.
- [04]-[HOOKS](.planning/observability/hooks.md): Scoped hook registry with modality rows and telemetry taps.
- [05]-[PROFILES](.planning/observability/profiles.md): Pyroscope push, benchmark receipts, and the offline-job envelope.
- [06]-[TELEMETRY](.planning/observability/telemetry.md): Profile-gated OTLP install owner.
- [07]-[BUNDLE](.planning/observability/bundle.md): Support-bundle capsule — fenced collectors, content-keyed archive, and the diagnostic route.
- [08]-[JOURNAL](.planning/observability/journal.md): Durable fact stream, retention classes, exact-decimal rating, and crypto-shredded erasure.

[RELIABILITY]:
- [09]-[FAULTS](.planning/reliability/faults.md): Boundary-fault union, its exception-to-fault projector, and the versioned scope coordinate.
- [10]-[RESILIENCE](.planning/reliability/resilience.md): Retry policy table, one row per retryable class.

[TRANSPORT]:
- [11]-[ROOTS](.planning/transport/roots.md): Resource roots and refs over fsspec and the remote transports.
- [12]-[SERVE](.planning/transport/serve.md): gRPC server lifecycle, route roster, capability invoke, and the daemon entry.
- [13]-[SHAPES](.planning/transport/shapes.md): Proto vocabulary and its descriptor drift gate.
- [14]-[WIRE](.planning/transport/wire.md): Protobuf transcode, frame legs, and the CRDT-op codec.

[EXECUTION]:
- [15]-[ADMISSION](.planning/execution/admission.md): Runtime context, causal frames, and settings admission.
- [16]-[LANES](.planning/execution/lanes.md): Lane-policy task groups and the stage-plan DAG.
- [17]-[WORKERS](.planning/execution/workers.md): Worker crossing — closed kind family, kernel value, warm pools, the fenced work lease, supervision.
- [18]-[RECIPE](.planning/execution/recipe.md): Content-keyed recipe execution on the thread lane.

[EVIDENCE]:
- [19]-[CLOCK](.planning/evidence/clock.md): HLC stamp, element id, tenant, and causal frame.
- [20]-[IDENTITY](.planning/evidence/identity.md): Python content-key implementation proving the shared digest contract.
- [21]-[REPRODUCTION](.planning/evidence/reproduction.md): Seed-reproduction corpus and its parity fold.
- [22]-[EVIDENCE](.planning/evidence/evidence.md): Evidence union, catalogue member facts, and grammar registry.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager` — GCP `SecretTier.cloud` read arm behind settings admission.
- `google-crc32c` — secret-payload transport-integrity digest.
- `hvac` — HashiCorp Vault `SecretTier.cloud` read arm.
- `azure-keyvault-secrets` — Azure Key Vault `SecretTier.cloud` read arm.
- `azure-identity` — managed/workload `TokenCredential` chain the Azure Key Vault read arm constructs.

[TRANSPORT]:
- `httpx`
- `hishel[httpx]` — RFC-9111 HTTP cache over the httpx transport rail.
- `asyncssh`
- `watchfiles`
- `stamina`
- `opentelemetry-instrumentation-grpc`
- `grpcio-health-checking`

[OBSERVABILITY]:
- `opentelemetry-exporter-otlp-proto-grpc` — daemon-selectable OTLP gRPC egress row; proto-http stays the estate default.
- `opentelemetry-semantic-conventions` — released schema-url roster the one branch schema-url pin reads; a semconv bump moves that pin.
- `opentelemetry-instrumentation` — `BaseInstrumentor` lifecycle, dependency gate, and suppression scopes every train row below implements.
- `opentelemetry-instrumentation-asyncio` — coroutine and `to_thread` context propagation on the train.
- `opentelemetry-instrumentation-dbapi` — PEP-249 wrap seam: db-semconv spans for duckdb and ADBC drivers without a dedicated instrumentor.
- `opentelemetry-instrumentation-httpx` — client spans on the httpx transport legs.
- `opentelemetry-instrumentation-jinja2` — template render, compile, and load spans on the train.
- `opentelemetry-instrumentation-psycopg` — psycopg DBAPI spans the data query surfaces ride.
- `opentelemetry-instrumentation-sqlite3` — stdlib sqlite3 DBAPI spans.
- `opentelemetry-instrumentation-system-metrics` — system and interpreter-GC gauges under the train's slice.
- `opentelemetry-instrumentation-threading` — cross-thread context propagation on the train.
- `opentelemetry-processor-baggage` — baggage-to-span and baggage-to-log promotion pair behind the telemetry `PROMOTED_BAGGAGE` predicate.
- `opentelemetry-resource-detector-containerid` — `container.id` resource detector on the telemetry detector list.
- `pyroscope-io` — native continuous-profiling push agent.
- `pyroscope-otel` — continuous-profiling push and the root-span profile link.

[EVIDENCE]:
- `cryptography` — AEAD envelope and key-wrap primitives the journal's per-subject crypto-shredding seals under.

[EXECUTION]:
- `apscheduler` — one cron and interval scheduler owner.
- `wasmtime` — in-process guest sandbox for the WASM worker kind.
- `queenbee`
- `lbt-recipes`
- `pollination-handlers`

[PARSING]:
- `cyclopts` — typed CLI entrypoint grammar for the daemon.
- `tree-sitter`
- `tree-sitter-python`
- `tree-sitter-typescript`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Py registry; the registry and its charters own the full contracts, and `libs/python/.api/` holds the shared API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`
- `cloudpickle`
- `tblib`
- `loky`
- `pebble`

[OBSERVABILITY]:
- `structlog`
- `opentelemetry-api`
- `opentelemetry-sdk`
- `opentelemetry-exporter-otlp-proto-http`
- `psutil`

[NUMERIC_SUBSTRATE]:
- `numpy` — shared-memory span reconstruction at the worker crossing, deferred behind the wire axis.

[IDENTITY]:
- `xxhash`

[TRANSPORT]:
- `fsspec`
- `obstore`
- `universal-pathlib`

[COMPRESSION]:
- `lz4`

[WIRE_CODEGEN]:
- `grpcio`
- `grpcio-tools`
- `protobuf`
