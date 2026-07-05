# [PY_RUNTIME]

`runtime` is the host-free execution foundation every `libs/python` sibling composes. It mints shared value shapes once and references no sibling: the single content-identity owner reproducing the C# `XxHash128` seed bit-identically, the one boundary-fault and `Result`/`Option` rail, the one resilience policy, caller-owned context and settings admission, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime decoding the C#-owned protobuf wire, the `msgspec.msgpack` op-log delta, the capability-descriptor SDK, external-API and structural-parsing evidence, the cross-runtime seed-parity corpus binding (`identity < receipts < reproduction`), and the private daemon entrypoint grammar. The companion owns no wire vocabulary and decodes only C#-minted shapes (single-mint invariant). `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[RECEIPTS](.planning/observability/receipts.md)
- [02]-[METRICS](.planning/observability/metrics.md)
- [03]-[TELEMETRY](.planning/observability/telemetry.md)
- [04]-[FAULTS](.planning/reliability/faults.md)
- [05]-[RESILIENCE](.planning/reliability/resilience.md)
- [06]-[ROOTS](.planning/transport/roots.md)
- [07]-[SERVE](.planning/transport/serve.md)
- [08]-[WIRE](.planning/transport/wire.md)
- [09]-[ADMISSION](.planning/execution/admission.md)
- [10]-[LANES](.planning/execution/lanes.md)
- [11]-[RECIPE](.planning/execution/recipe.md)
- [12]-[IDENTITY](.planning/evidence/identity.md)
- [13]-[REPRODUCTION](.planning/evidence/reproduction.md)
- [14]-[EVIDENCE](.planning/evidence/evidence.md)
- [15]-[CLOCK](.planning/clock/clock.md)

## [02]-[DOMAIN_PACKAGES]

Every domain library this folder owns directly, planned or implemented. Versions are centralized in the one root manifest; this list carries no pin.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager`

[TRANSPORT]:
- `httpx`
- `asyncssh`
- `watchfiles`
- `stamina`
- `opentelemetry-instrumentation-grpc`
- `grpcio-health-checking`
- `obstore`
- `fsspec`
- `universal-pathlib`

[SCHEDULING]:
- `apscheduler`

[RECIPE]:
- `queenbee`
- `lbt-recipes`
- `pollination-handlers`

[PARSING]:
- `cyclopts`
- `tree-sitter`
- `tree-sitter-python`
- `tree-sitter-typescript`

[COMPRESSION]:
- `lz4`

## [03]-[SUBSTRATE_PACKAGES]

Branch-wide substrate packages this folder consumes; the canonical registry and API evidence live at `libs/python/.planning/README.md` and `libs/python/.api/`.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[OBSERVABILITY]:
- `structlog`
- `opentelemetry-api`
- `opentelemetry-sdk`
- `opentelemetry-exporter-otlp-proto-http`
- `psutil`

[IDENTITY]:
- `xxhash`

[WIRE_CODEGEN]:
- `protobuf`
- `grpcio`
- `grpcio-tools`
