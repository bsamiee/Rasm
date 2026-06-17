# [PY_RUNTIME_FEATURES]

The realized capability list for the execution foundation. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[CONTEXT_AND_RAILS]

The caller-owned context and settings admission and the single fault/rail/resilience spine every sibling returns through.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]              |
| :-----: | :------------------------------------------------------------------------ | :------------------------- |
|   [1]   | One immutable context carrying profile, correlation, deadline, class      | context-settings#CONTEXT   |
|   [2]   | Profile vocabulary with a per-profile policy table over four modalities   | context-settings#CONTEXT   |
|   [3]   | One settings-source-order admission over pydantic-settings                | context-settings#SETTINGS  |
|   [4]   | One boundary-fault tagged union every package raises through              | rails-resilience#FAULT     |
|   [5]   | Result/Option rail selecting abort versus accumulate by monadic algebra   | rails-resilience#FAULT     |
|   [6]   | Exception-to-fault conversion exactly once at the owning boundary         | rails-resilience#FAULT     |
|   [7]   | One stamina-backed Retry policy table with a row per retryable class      | rails-resilience#RESILIENCE |

## [2]-[IDENTITY_RESOURCES_LANES]

The single content key, the resource and transport roots, and the bounded structured-concurrency spine.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]            |
| :-----: | :------------------------------------------------------------------------ | :----------------------- |
|   [8]   | One XxHash128 content key with the settings-folded seed                   | content-identity#IDENTITY |
|   [9]   | Resource roots over fsspec/universal-pathlib with safe relative resolve   | resources-lanes#RESOURCE  |
|  [10]   | One transport-resource union over HTTP/SSH/Speckle acquisition           | resources-lanes#RESOURCE  |
|  [11]   | Bounded anyio task-group lanes with capacity, cancellation, drain receipt | resources-lanes#LANE      |
|  [12]   | Multi-stage DAG orchestration with per-stage retry over the lane spine    | resources-lanes#LANE      |

## [3]-[EVIDENCE_AND_SERVE]

Local evidence production, the contributor port, the inbound companion serve, and the structural-evidence and entrypoint grammar.

| [INDEX] | [FEATURE]                                                                  | [PAGE#CLUSTER]        |
| :-----: | :------------------------------------------------------------------------ | :-------------------- |
|  [13]   | One receipt tagged union with admitted/planned/emitted/rejected/drained   | observability#RECEIPT |
|  [14]   | One contributor port every sibling's typed receipt wires through          | observability#RECEIPT |
|  [15]   | Field redaction before emission; structlog/OTel/psutil signal feeds       | observability#RECEIPT |
|  [16]   | Inbound gRPC server lifecycle: bind, request lifecycle, graceful drain    | server-host#SERVE     |
|  [17]   | Credential axis over token, keyring, insecure-loopback                    | server-host#SERVE     |
|  [18]   | API evidence records feeding the structural-evidence rail                 | evidence#API          |
|  [19]   | tree-sitter structural-parsing evidence over polyglot source grammars      | evidence#API          |
|  [20]   | Type-hint-driven private companion entrypoint grammar over cyclopts       | evidence#ENTRY        |
