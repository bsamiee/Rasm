# [PY_RUNTIME]

`runtime` is stratum zero of the Python branch: the host-free execution foundation every sibling composes, where proof outranks feature reach. Every shared vocabulary — content identity, fault and resilience rails, admission, lanes, logical time, receipts and telemetry, resource transport, the companion server — mints once, symbol-consumed; the module set stands alone, zero siblings installed. A defect here multiplies — every fold returns through the fault rail, every retry rides the one policy table, the C# host drives the served wire — so nothing lands as convention that can land as proof.

One polymorphic owner per sub-domain mints the shared value shapes once, and every sibling returns through them rather than re-minting its own. Companion decode admits only C#-minted wire shapes and owns no wire vocabulary, so identity crosses the runtime boundary through a single mint.

## [01]-[ROUTER]

[OBSERVABILITY]:
- [01]-[RECEIPTS](.planning/observability/receipts.md): Receipt union, drain taxonomy, cost evidence, and contributor-fold port.
- [02]-[LOGGING](.planning/observability/logging.md): Structlog pipeline, stdout ship law, and the log-ship policy.
- [03]-[METRICS](.planning/observability/metrics.md): One `MeterProvider`'s instruments, the record mapping, and the instrumentor train.
- [04]-[HOOKS](.planning/observability/hooks.md): Scoped hook registry with modality rows and telemetry taps.
- [05]-[PROFILES](.planning/observability/profiles.md): Pyroscope push, benchmark receipts, and the offline-job envelope.
- [06]-[TELEMETRY](.planning/observability/telemetry.md): Profile-gated OTLP install owner.
- [07]-[BUNDLE](.planning/observability/bundle.md): Support-bundle capsule — fenced collectors, content-keyed archive, and the diagnostic route.

[RELIABILITY]:
- [08]-[FAULTS](.planning/reliability/faults.md): Boundary-fault union and its exception-to-fault projector.
- [09]-[RESILIENCE](.planning/reliability/resilience.md): Retry policy table, one row per retryable class.

[TRANSPORT]:
- [10]-[ROOTS](.planning/transport/roots.md): Resource roots and refs over fsspec and the remote transports.
- [11]-[SERVE](.planning/transport/serve.md): gRPC server lifecycle, route roster, capability invoke, and the daemon entry.
- [12]-[SHAPES](.planning/transport/shapes.md): Proto vocabulary and its descriptor drift gate.
- [13]-[WIRE](.planning/transport/wire.md): Protobuf transcode, frame legs, and the CRDT-op codec.

[EXECUTION]:
- [14]-[ADMISSION](.planning/execution/admission.md): Runtime context, causal frames, and settings admission.
- [15]-[LANES](.planning/execution/lanes.md): Lane-policy task groups and the stage-plan DAG.
- [16]-[WORKERS](.planning/execution/workers.md): Worker fabric — kind family, kernel crossing, warm pools, remote/device/guest arms, the parented worker emitter, and supervision.
- [17]-[RECIPE](.planning/execution/recipe.md): Content-keyed recipe execution on the thread lane.

[EVIDENCE]:
- [18]-[IDENTITY](.planning/evidence/identity.md): Content identity and key reproducing the C# seed bit-identically.
- [19]-[REPRODUCTION](.planning/evidence/reproduction.md): Seed-reproduction corpus and its parity fold.
- [20]-[EVIDENCE](.planning/evidence/evidence.md): Evidence union, catalogue member facts, and grammar registry.

[CLOCK]:
- [21]-[CLOCK](.planning/clock/clock.md): HLC stamp, element id, tenant, and causal frame.

## [02]-[DOMAIN_PACKAGES]

Domain libraries this folder admits directly; versions centralize in the one root manifest.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager` — cloud secret-manager read arm behind settings admission.
- `google-crc32c` — secret-payload transport-integrity digest.
- `hvac` — HashiCorp Vault read arm closing the `SecretTier.cloud` provider family beside the GCP arm.
- `azure-keyvault-secrets` — Azure Key Vault read arm completing the `SecretTier.cloud` provider family beside the GCP and Vault arms.

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
- `opentelemetry-instrumentation-asyncio` — coroutine and `to_thread` context propagation on the train.
- `opentelemetry-instrumentation-dbapi` — generic PEP-249 wrap seam giving duckdb and ADBC DBAPI drivers db-semconv spans without a dedicated instrumentor.
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

[WIRE]:
- `protobuf`
- `grpcio`
- `grpcio-tools`
- `lz4`
