# [PY_RUNTIME]

`runtime` is the host-free execution foundation every `libs/python` sibling composes. It mints shared value shapes once and references no sibling: the single content-identity owner reproducing the C# `XxHash128` seed bit-identically, the one boundary-fault and `Result`/`Option` rail, the one resilience policy (`POLICY` rows spanning the retry classes through `ORACLE` conformance and `OCC_NATIVE` geometry, every provider-discriminated target import-free, `install` returning the finalized hook tuple), caller-owned context and settings admission, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime — `Route` registration roster, served health protocol, `time_remaining` deadline lift, `FaultDetail` trailer round-trip, and the daemon composition root — decoding the C#-owned protobuf wire through the `PROTO_VOCABULARY` wire vocabulary with its descriptor drift gate and `grpcio-tools` codegen contract, the `msgspec.msgpack` op-log delta, the capability-descriptor SDK, external-API and structural-parsing evidence, the cross-runtime seed-parity corpus binding (`identity < receipts < reproduction`), and the private daemon entrypoint grammar. The companion owns no wire vocabulary and decodes only C#-minted shapes (single-mint invariant). `ARCHITECTURE.md` carries the domain map, `IDEAS.md` the forward pool, and `TASKLOG.md` the open work.

## [01]-[ROUTER]

- [01]-[RECEIPTS](.planning/observability/receipts.md)
- [02]-[METRICS](.planning/observability/metrics.md)
- [03]-[TELEMETRY](.planning/observability/telemetry.md)
- [04]-[FAULTS](.planning/reliability/faults.md)
- [05]-[RESILIENCE](.planning/reliability/resilience.md)
- [06]-[ROOTS](.planning/transport/roots.md)
- [07]-[SERVE](.planning/transport/serve.md)
- [08]-[SHAPES](.planning/transport/shapes.md)
- [09]-[WIRE](.planning/transport/wire.md)
- [10]-[ADMISSION](.planning/execution/admission.md)
- [11]-[LANES](.planning/execution/lanes.md)
- [12]-[RECIPE](.planning/execution/recipe.md)
- [13]-[IDENTITY](.planning/evidence/identity.md)
- [14]-[REPRODUCTION](.planning/evidence/reproduction.md)
- [15]-[EVIDENCE](.planning/evidence/evidence.md)
- [16]-[CLOCK](.planning/clock/clock.md)

## [02]-[DOMAIN_PACKAGES]

Every domain library this folder owns directly, planned or implemented. Versions are centralized in the one root manifest; this list carries no pin.

[SETTINGS_SECRETS]:
- `pydantic-settings`
- `keyring`
- `google-cloud-secret-manager` — consumed live by the `execution/admission` `[03]-[SETTINGS]` `_probe` cloud arm
- `google-crc32c` — `SecretPayload.data_crc32c` transport-integrity digest consumed by the `execution/admission` `[03]-[SETTINGS]` `cloud_read` fence

[TRANSPORT]:
- `httpx`
- `asyncssh`
- `watchfiles`
- `stamina`
- `opentelemetry-instrumentation-grpc`
- `grpcio-health-checking`
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

[TRANSPORT]:
- `fsspec`
- `obstore`

[COMPRESSION]:
- `lz4`

[WIRE_CODEGEN]:
- `protobuf`
- `grpcio`
- `grpcio-tools`
