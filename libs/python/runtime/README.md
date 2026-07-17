# [PY_RUNTIME]

`runtime` is stratum zero of the Python branch — the host-free execution foundation every sibling composes, where foundation-grade proof outranks feature reach. Every shared vocabulary — content identity, the boundary-fault and resilience rails, caller-owned admission, execution lanes, logical time, receipts and telemetry, resource transport, the companion gRPC server — is minted exactly once and consumed by symbol, and the module set stands alone with zero sibling packages installed. Its bar is multiplied trust: every sibling fold returns through the fault rail, every retry in the branch rides the one policy table, every scheduled pipeline crosses the lane owner, and the C# host drives the served wire — a defect here multiplies across the branch, so nothing lands as convention that can land as proof.

One polymorphic owner per sub-domain mints the shared value shapes once, and every sibling returns through them rather than re-minting its own. Companion decode admits only C#-minted wire shapes and owns no wire vocabulary, so identity crosses the runtime boundary through a single mint.

## [01]-[ROUTER]

[OBSERVABILITY]:
- [01]-[RECEIPTS](.planning/observability/receipts.md): Receipt union, drain taxonomy, and contributor-fold port.
- [02]-[METRICS](.planning/observability/metrics.md): One `MeterProvider`'s instruments and the record mapping.
- [03]-[TELEMETRY](.planning/observability/telemetry.md): Profile-gated OTLP install owner.

[RELIABILITY]:
- [04]-[FAULTS](.planning/reliability/faults.md): Boundary-fault union and its exception-to-fault projector.
- [05]-[RESILIENCE](.planning/reliability/resilience.md): Retry policy table, one row per retryable class.

[TRANSPORT]:
- [06]-[ROOTS](.planning/transport/roots.md): Resource roots and refs over fsspec and the remote transports.
- [07]-[SERVE](.planning/transport/serve.md): gRPC server lifecycle, route roster, and credential admit.
- [08]-[SHAPES](.planning/transport/shapes.md): Proto vocabulary and its descriptor drift gate.
- [09]-[WIRE](.planning/transport/wire.md): Protobuf transcode, frame legs, and the CRDT-op codec.

[EXECUTION]:
- [10]-[ADMISSION](.planning/execution/admission.md): Runtime context, causal frames, and settings admission.
- [11]-[LANES](.planning/execution/lanes.md): Lane-policy task groups and the stage-plan DAG.
- [12]-[WORKERS](.planning/execution/workers.md): Worker fabric — kind family, kernel crossing, warm pools, remote/device/guest arms, and supervision.
- [13]-[RECIPE](.planning/execution/recipe.md): Content-keyed recipe execution on the thread lane.

[EVIDENCE]:
- [14]-[IDENTITY](.planning/evidence/identity.md): Content identity and key reproducing the C# seed bit-identically.
- [15]-[REPRODUCTION](.planning/evidence/reproduction.md): Seed-reproduction corpus and its parity fold.
- [16]-[EVIDENCE](.planning/evidence/evidence.md): Evidence union, catalogue member facts, and grammar registry.

[CLOCK]:
- [17]-[CLOCK](.planning/clock/clock.md): HLC stamp, element id, tenant, and causal frame.

## [02]-[DOMAIN_PACKAGES]

Domain libraries this folder admits directly; versions centralize in the one root manifest.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager` — cloud secret-manager read arm behind settings admission.
- `google-crc32c` — secret-payload transport-integrity digest.

[TRANSPORT]:
- `httpx`
- `asyncssh`
- `watchfiles`
- `stamina`
- `opentelemetry-instrumentation-grpc`
- `grpcio-health-checking`
- `universal-pathlib`

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

Branch-wide substrate this folder consumes; the canonical registry and API evidence live at `libs/python/.planning/README.md` and `libs/python/.api/`.

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

[WIRE]:
- `protobuf`
- `grpcio`
- `grpcio-tools`
- `lz4`
