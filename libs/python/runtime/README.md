# [PY_RUNTIME]

`runtime` is the Python-local execution foundation every `libs/python` sibling consumes. It owns caller-owned context and settings admission, the single boundary-fault and Result/Option rail, the one resilience policy, the one content-identity owner, resource roots and bounded structured-concurrency lanes, local receipts and the contributor port, the inbound companion gRPC server-runtime and credential axis, external-API and structural-parsing evidence, and the private daemon entrypoint grammar. It has zero consumers today and implementation is full-capability. Owner state and the axis registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`. The design pages in `.planning/` are decision-complete blueprints an implementation agent transcribes; the package catalogues in `.api/` carry the external-surface evidence each page consumes.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                            | [OWNS]                                                          |
| :-----: | :------------------------------------------------ | :------------------------------------------------------------- |
|   [1]   | [context-settings](.planning/context-settings.md) | profile, correlation, deadline, context and settings admission |
|   [2]   | [rails-resilience](.planning/rails-resilience.md) | the boundary-fault tagged union, Result/Option rail, one Retry |
|   [3]   | [content-identity](.planning/content-identity.md) | the single XxHash128 content-identity owner                     |
|   [4]   | [resources-lanes](.planning/resources-lanes.md)   | resource roots, transport resources, anyio lanes, stage DAG    |
|   [5]   | [observability](.planning/observability.md)       | local receipts, the contributor port, redaction, signals       |
|   [6]   | [server-host](.planning/server-host.md)           | the inbound companion gRPC server lifecycle and credential axis |
|   [7]   | [evidence](.planning/evidence.md)                 | API and structural-parsing evidence, private entrypoint grammar |

## [2]-[ADMISSIONS_RECORD]

The executed admissions ledger maps each package to its consuming page, `.api` catalogue, and admission status. Versions live in the root manifest; this table never carries a pin. `[STATUS]` is one of `admitted`, `catalogue-pending`. The companion server wire rides the `python_version<'3.13'` floor and carries `catalogue-pending` until the floor/lock-scope decision admits the sub-3.15 environment.

| [INDEX] | [PACKAGE]                                                | [PAGE]           | [CATALOGUE]                                                                                                                          | [STATUS]          |
| :-----: | :------------------------------------------------------- | :--------------- | :---------------------------------------------------------------------------------------------------------------------------------- | :---------------- |
|   [1]   | expression                                              | rails-resilience | api-expression.md                                                                                                                  | admitted          |
|   [2]   | stamina                                                 | rails-resilience | api-stamina.md                                                                                                                     | admitted          |
|   [3]   | pydantic, pydantic-settings, msgspec                    | context-settings | api-pydantic.md, api-pydantic-settings.md, api-msgspec.md                                                                          | admitted          |
|   [4]   | beartype                                               | rails-resilience | api-beartype.md                                                                                                                    | admitted          |
|   [5]   | fsspec, s3fs, gcsfs, universal-pathlib                 | resources-lanes  | api-fsspec.md, api-s3fs.md, api-gcsfs.md, api-universal-pathlib.md                                                                 | admitted          |
|   [6]   | httpx, asyncssh, specklepy                             | resources-lanes  | api-httpx.md, api-asyncssh.md, api-specklepy.md                                                                                    | admitted          |
|   [7]   | anyio, watchfiles, aiocron                             | resources-lanes  | api-anyio.md, api-watchfiles.md, api-aiocron.md                                                                                    | admitted          |
|   [8]   | structlog, opentelemetry-api, opentelemetry-sdk, opentelemetry-exporter-otlp-proto-http, psutil | observability | api-structlog.md, api-opentelemetry-api.md, api-opentelemetry-sdk.md, api-opentelemetry-exporter-otlp-proto-http.md, api-psutil.md | admitted          |
|   [9]   | grpcio, grpcio-tools, protobuf                         | server-host      | api-grpcio.md, api-grpcio-tools.md, api-protobuf.md                                                                                | catalogue-pending |
|  [10]   | cyclopts                                               | evidence         | api-cyclopts.md                                                                                                                    | admitted          |
|  [11]   | tree-sitter, tree-sitter-python, tree-sitter-typescript | evidence         | api-tree-sitter.md, api-tree-sitter-python.md, api-tree-sitter-typescript.md                                                       | admitted          |

## [3]-[PROOF_GATES]

Proof runs at the planned phase gate, not after each edit. `[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]                | [RAIL]      | [EVIDENCE]                                          |
| :-----: | :-------------------- | :---------- | :------------------------------------------------- |
|  [G1]   | locked restore        | uv          | runtime pins resolve against the root manifest      |
|  [G2]   | API catalogue resolve | assay api   | every fence member resolves to an `.api` row        |
|  [G3]   | type check            | ty          | typed-signature transcription resolves clean        |
|  [G4]   | lint and format       | ruff        | routed closure, zero diagnostics                    |
|  [G5]   | spec law-matrix       | pytest      | runtime law-matrix specs pass                       |
|  [G6]   | companion floor       | uv          | `grpcio`/`grpcio-tools`/`protobuf` reflect on cp312 |
|  [G7]   | page diagram render   | mermaid-cli | page diagrams render through the local renderer      |
