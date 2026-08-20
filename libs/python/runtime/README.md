# [PY_RUNTIME]

`runtime` is stratum zero of the Python branch: the standalone host-free execution foundation every sibling composes, one polymorphic owner per sub-domain minting Python's shared value shapes once. Siblings return through those mints instead of re-minting their own, and the module set stands alone, so a Python application operates with no sibling installed and no foreign runtime or emitted bundle present.

## [01]-[ROUTER]

[OBSERVABILITY]:
- [01]-[RECEIPTS](.planning/observability/receipts.md): Receipt union, drain taxonomy, cost evidence, composition scope, and contributor port.
- [02]-[LOGGING](.planning/observability/logging.md): Structlog pipeline chain, OTLP wire projection, `LogShip` egress, and the terminal doors.
- [03]-[METRICS](.planning/observability/metrics.md): Metric spine: `INSTRUMENTS` census, `MEASURES` admission, views, occupancy, and the train.
- [04]-[HOOKS](.planning/observability/hooks.md): Scoped hook registry with modality rows and telemetry taps.
- [05]-[PROFILES](.planning/observability/profiles.md): Pyroscope push, benchmark receipts, and the offline-job message envelope.
- [06]-[TELEMETRY](.planning/observability/telemetry.md): Profile-gated OTLP install owner; every other observability surface assumes providers.
- [07]-[BUNDLE](.planning/observability/bundle.md): Support-bundle capsule — fenced collectors, content-keyed archive, and the diagnostic route.
- [08]-[JOURNAL](.planning/observability/journal.md): Durable fact stream, retention classes, exact-decimal rating, and crypto-shredded erasure.

[RELIABILITY]:
- [09]-[FAULTS](.planning/reliability/faults.md): Boundary-fault union, its exception-to-fault projector, and the versioned scope coordinate.
- [10]-[RESILIENCE](.planning/reliability/resilience.md): One `RetryClass` policy table ruling every branch retry, failure window, and admission rate.

[TRANSPORT]:
- [11]-[ROOTS](.planning/transport/roots.md): `ObjectStoreLane` one obstore dispatch surface with `RemoteEndpoint`/`HttpEndpoint` custody.
- [12]-[SERVE](.planning/transport/serve.md): gRPC server lifecycle, route roster, capability invoke, and the daemon entry.
- [13]-[SHAPES](.planning/transport/shapes.md): Proto vocabulary and its descriptor drift gate.
- [14]-[WIRE](.planning/transport/wire.md): Protobuf transcode, frame legs, and the CRDT-op codec.
- [15]-[EVENT](.planning/transport/event.md): CloudEvents message envelope owner — attribute grammar, extension roster, and the format contract.
- [16]-[BINDING](.planning/transport/binding.md): Protocol binding rows, the fact emitter, and the broker lane.
- [17]-[FILTER](.planning/transport/filter.md): CESQL expression owner, the closed `FilterDialect` family, and the subscription resource.

[EXECUTION]:
- [18]-[ADMISSION](.planning/execution/admission.md): Runtime context, causal frames, settings, and trust rows.
- [19]-[LANES](.planning/execution/lanes.md): Lane-policy task groups and the stage-plan DAG.
- [20]-[WORKERS](.planning/execution/workers.md): Worker crossing — closed kind family, kernel value, warm pools, the fenced work lease, supervision.
- [21]-[RECIPE](.planning/execution/recipe.md): `RecipeExecution` content-keyed recipe seat on the offload lane under the AGPL charter.

[EVIDENCE]:
- [22]-[CLOCK](.planning/evidence/clock.md): HLC stamp, element id, tenant, and causal frame.
- [23]-[IDENTITY](.planning/evidence/identity.md): Python content-key implementation proving the shared digest contract.
- [24]-[REPRODUCTION](.planning/evidence/reproduction.md): Seed-reproduction corpus and its parity fold.
- [25]-[EVIDENCE](.planning/evidence/evidence.md): Evidence union, catalogue member facts, and grammar registry.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; versions centralize in the root `pyproject.toml` and corroborate against this folder's `.api/`.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager` — GCP `SecretTier.cloud` read arm behind settings admission.
- `google-crc32c` — Secret-payload transport-integrity digest.
- `hvac` — HashiCorp Vault `SecretTier.cloud` read arm.
- `azure-keyvault-secrets` — Azure Key Vault `SecretTier.cloud` read arm.
- `azure-identity` — Managed/workload `TokenCredential` chain the Azure Key Vault read arm constructs.

[TRANSPORT]:
- `httpx`
- `hishel[httpx]` — RFC-9111 HTTP cache over the httpx transport rail.
- `asyncssh`
- `watchfiles`
- `stamina`
- `opentelemetry-instrumentation-grpc`
- `grpcio-health-checking`

[OBSERVABILITY]:
- `opentelemetry-exporter-otlp-proto-grpc` — Daemon-selectable OTLP gRPC egress row; proto-http stays the estate default.
- `opentelemetry-semantic-conventions` — Released schema-url roster the one branch schema-url pin reads; a semconv bump moves that pin.
- `opentelemetry-instrumentation` — `BaseInstrumentor` lifecycle, dependency gate, and suppression scopes every train row below implements.
- `opentelemetry-instrumentation-asyncio` — Coroutine and `to_thread` context propagation on the train.
- `opentelemetry-instrumentation-dbapi` — PEP-249 wrap seam: db-semconv spans for duckdb and ADBC drivers without a dedicated instrumentor.
- `opentelemetry-instrumentation-httpx` — Client spans on the httpx transport legs.
- `opentelemetry-instrumentation-jinja2` — Template render, compile, and load spans on the train.
- `opentelemetry-instrumentation-psycopg` — psycopg DBAPI spans the data query surfaces ride.
- `opentelemetry-instrumentation-sqlite3` — stdlib sqlite3 DBAPI spans.
- `opentelemetry-instrumentation-system-metrics` — System and interpreter-GC gauges under the train's slice.
- `opentelemetry-instrumentation-threading` — Cross-thread context propagation on the train.
- `opentelemetry-processor-baggage` — Baggage-to-span and baggage-to-log promotion pair behind the telemetry `PROMOTED_BAGGAGE` predicate.
- `opentelemetry-resource-detector-containerid` — `container.id` resource detector on the telemetry detector list.
- `pyroscope-io` — Native continuous-profiling push agent.
- `pyroscope-otel` — Continuous-profiling push and the root-span profile link.

[EVIDENCE]:
- `cryptography` — AEAD envelope and key-wrap primitives the journal's per-subject crypto-shredding seals under.

[EXECUTION]:
- `apscheduler` — One cron and interval scheduler owner.
- `wasmtime` — In-process guest sandbox for the WASM worker kind.
- `queenbee`
- `lbt-recipes`
- `pollination-handlers`

[PARSING]:
- `cyclopts` — Typed CLI entrypoint grammar for the daemon.
- `lark` — CESQL grammar engine behind the `sql` filter dialect; geometry registers the same substrate for its selector grammar.
- `tree-sitter`
- `tree-sitter-python`
- `tree-sitter-typescript`

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

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
- `numpy` — Shared-memory span reconstruction at the worker crossing, deferred behind the wire axis.

[IDENTITY]:
- `xxhash`

[TRANSPORT]:
- `fsspec`
- `obstore`
- `universal-pathlib`

[EVENT_FABRIC]:
- `cloudevents` — Attribute algebra behind the runtime-owned mint boundary and format contract.
- `confluent-kafka` — Kafka client behind the Kafka binding row and its registry serializers.
- `nats-py` — Client behind the subject-addressed binding row.
- `paho-mqtt` — MQTT client behind the unprefixed MQTT binding rows.
- `pika` — Blocking AMQP 0-9-1 client behind the RabbitMQ binding row.
- `fastavro` — Avro codec beneath the registry serializer and the branch-owned Avro format row.
- `jsonschema` — Payload gate beneath the registry JSON serializer and every published `dataschema`.

[COMPRESSION]:
- `lz4`

[WIRE_CODEGEN]:
- `grpcio`
- `grpcio-tools`
- `protobuf`
