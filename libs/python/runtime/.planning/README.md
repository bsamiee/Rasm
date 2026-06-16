# [PY_RUNTIME_PLANNING]

`runtime` is the Python-local execution foundation every sibling package consumes. It has zero consumers today; implementation is full-capability with no holding back. These pages are decision-complete blueprints an implementation agent transcribes — never re-designed downstream. The package owns caller-owned context/settings admission, the single boundary-fault + Result/Option rail, the one resilience policy, the one content-identity owner, resource roots, bounded concurrency lanes, local receipts + the contributor port, the inbound companion gRPC server-runtime + credential axis, external-API + structural-parsing evidence, and the private daemon/CLI entrypoint grammar.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                          | [OWNS]                                                        | [STATE]   |
| :-----: | :---------------------------------------------- | :----------------------------------------------------------- | :-------- |
|   [1]   | [context-settings](context-settings.md)         | profile, correlation, deadline, context + settings admission | finalized   |
|   [2]   | [rails-resilience](rails-resilience.md)         | the boundary-fault tagged union, Result/Option rail, one Retry | finalized |
|   [3]   | [content-identity](content-identity.md)         | the single XxHash128 content-identity owner                   | finalized   |
|   [4]   | [resources-lanes](resources-lanes.md)           | resource roots, transport resources, anyio lanes, stage DAG  | finalized   |
|   [5]   | [observability](observability.md)               | local receipts, the contributor port, redaction, signals     | finalized   |
|   [6]   | [server-host](server-host.md)                   | the inbound companion gRPC server lifecycle + credential axis | finalized  |
|   [7]   | [evidence](evidence.md)                         | API + structural-parsing evidence, private entrypoint grammar | finalized   |

## [2]-[CATALOGUE_PENDING]

- The companion server wire (`grpcio`, `grpcio-tools`, `protobuf`) is catalogued under runtime but rides the `python_version<'3.13'` companion floor; first-class admission lands when the floor/lock-scope decision admits the sub-3.15 environment (suite TASKLOG).

## [3]-[DENSITY_BAR]

Implementation collapses to one owner per axis and one entrypoint family per rail. A new feature is a row, a case, or a policy value, never a new surface. `[STATE]` carries `FINALIZED` where the owner is a transcription-complete fence with no open gate and `SPIKE` where the fence is complete but its proof carries a residual native/floor/live-server probe named in the page RESEARCH cluster.

| [INDEX] | [AXIS/CONCERN]        | [OWNER]              | [KIND]                | [CASES]                          |  [STATE]  |
| :-----: | :-------------------- | :------------------ | :-------------------- | :------------------------------- | :-------: |
|   [1]   | Context admission     | `RuntimeContext`    | frozen Struct + factory | profile/correlation/deadline/class | FINALIZED |
|   [2]   | Profile vocabulary    | `RuntimeProfile`    | StrEnum + policy table | tool/sidecar/package/test        | FINALIZED |
|   [3]   | Settings admission    | `SettingsAdmission` | pydantic-settings owner | mapping/env/payload sources      | FINALIZED |
|   [4]   | Fault family          | `BoundaryFault`     | tagged union          | config/resource/deadline/api/import/wire/boundary | FINALIZED |
|   [5]   | Rail carrier          | `RuntimeRail`       | Result/Option alias   | abort/accumulate algebras        | FINALIZED |
|   [6]   | Resilience policy     | `Retry`             | frozen policy table   | row per retryable class          | FINALIZED |
|   [7]   | Content identity      | `ContentIdentity`   | static surface        | `key`/`seed`/`fold`              | FINALIZED |
|   [8]   | Resource roots        | `ResourceRoot`      | frozen owner + `ResourceRef` | file/object-store/scratch  | FINALIZED |
|   [9]   | Transport resource    | `TransportResource` | tagged union          | http/ssh/speckle                 | FINALIZED |
|  [10]   | Concurrency lanes     | `LanePolicy`        | frozen owner + `DrainReceipt` | capacity/cancellation/drain | FINALIZED |
|  [11]   | Stage DAG             | `StagePlan`         | frozen owner          | stage edges, per-stage retry     | FINALIZED |
|  [12]   | Receipt evidence      | `Receipt`           | tagged union          | admitted/planned/emitted/rejected/drained | FINALIZED |
|  [13]   | Contributor port      | `ReceiptContributor`| Protocol              | one `contribute` method          | FINALIZED |
|  [14]   | Server host           | `ServerHost`        | boundary capsule      | `serve`/`drain` over `grpc.aio`  |   SPIKE   |
|  [15]   | Credential axis       | `Credential`        | tagged union          | token/keyring/insecure-loopback  |   SPIKE   |
|  [16]   | API evidence          | `ApiPackage`        | record + `ApiMember`  | distribution/import/owner/surface | FINALIZED |
|  [17]   | Entrypoint grammar     | `Entrypoint`        | cyclopts app          | companion private entry only      | FINALIZED |

## [4]-[BUILD_ORDER]

Vocabulary owners first, then shapes, rails, dispatch surfaces, boundaries, composition. The single root manifest admits every distribution; package source lands directly under `libs/python/runtime`.

| [INDEX] | [FILE]              | [TRANSCRIBES]                          | [GATE]            |
| :-----: | :------------------ | :------------------------------------- | :---------------- |
|   [1]   | `faults.py`         | rails-resilience#FAULT, #RESILIENCE    | static            |
|   [2]   | `context.py`        | context-settings#CONTEXT, #SETTINGS    | static            |
|   [3]   | `identity.py`       | content-identity#IDENTITY              | static            |
|   [4]   | `resources.py`      | resources-lanes#RESOURCE               | static            |
|   [5]   | `lanes.py`          | resources-lanes#LANE                   | static            |
|   [6]   | `observability.py`  | observability#RECEIPT                  | static            |
|   [7]   | `server.py`         | server-host#SERVE                      | static + floor    |
|   [8]   | `evidence.py`       | evidence#API, #ENTRY                   | static            |

## [5]-[PROOF_GATES]

| [GATE] | [RAIL]                       | [EVIDENCE]                                          |
| :----: | :--------------------------- | :------------------------------------------------- |
|  [G1]  | `uv lock --check`            | runtime pins resolve against the root manifest     |
|  [G2]  | `.api` catalogue             | every fence member resolves to an `.api` row       |
|  [G3]  | companion floor              | `grpcio`/`grpcio-tools`/`protobuf` reflect on cp312 |

## [6]-[PROHIBITIONS]

- [NEVER] add a public surface beside the budgeted owners; a new capability is a row or case.
- [NEVER] mint a second content-identity, receipt, retry, correlation, or wire vocabulary owner.
- [NEVER] add a public CLI command; `Entrypoint` is the companion's PRIVATE entry only.
- [NEVER] discover the host, start product services, own host lifecycle, derive product roots, or own the global clock.
- [NEVER] raise inside domain flow, propagate `None`-as-failure or sentinels, or use stringly-typed dispatch.
- [NEVER] read process environment directly after context admission; `SettingsAdmission` is the one source-order owner.

## [7]-[ADMISSIONS_RECORD]

| [INDEX] | [PACKAGE]                                | [PAGE]              | [CATALOGUE]                        | [STATUS]        |
| :-----: | :--------------------------------------- | :------------------ | :--------------------------------- | :-------------- |
|   [1]   | expression                               | rails-resilience    | api-expression.md                  | admitted        |
|   [2]   | stamina                                  | rails-resilience    | api-stamina.md                     | admitted        |
|   [3]   | pydantic, pydantic-settings, msgspec     | context-settings    | api-pydantic*.md, api-msgspec.md   | admitted        |
|   [4]   | beartype                                 | rails-resilience    | api-beartype.md                    | admitted        |
|   [5]   | fsspec, s3fs, gcsfs, universal-pathlib   | resources-lanes     | api-fsspec.md ...                  | admitted        |
|   [6]   | httpx, asyncssh, specklepy               | resources-lanes     | api-httpx.md, api-asyncssh.md, api-specklepy.md | admitted |
|   [7]   | anyio, watchfiles, aiocron               | resources-lanes     | api-anyio.md ...                   | admitted        |
|   [8]   | structlog, opentelemetry-*, psutil       | observability       | api-structlog.md ...               | admitted        |
|   [9]   | grpcio, grpcio-tools, protobuf           | server-host         | api-grpcio.md ...                  | catalogue-pending |
|  [10]   | cyclopts                                 | evidence            | api-cyclopts.md                    | admitted        |
|  [11]   | tree-sitter, tree-sitter-python/typescript | evidence          | api-tree-sitter*.md                | admitted        |

## [8]-[REFINEMENT_HORIZON]

Entry for the next deepening session: `libs/python/.planning/campaign-method.md`, then the suite `TASKLOG.md`, then this charter. The two `SPIKE` owners (`ServerHost`, `Credential`) finalize once the floor/lock-scope decision admits the sub-3.15 companion environment and the `grpcio.aio` server boots against the C# `ComputeService`/`ArtifactSync` descriptors. The `ContentIdentity` seed is proven byte-identical against the C# `InterchangeIdentity` XxHash128 output. The bar: every sibling composes runtime ports without a second owner anywhere.
