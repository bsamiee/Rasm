# [SERVICES_FEATURES]

The realized capability list for the node tier and its hosting infrastructure. Every feature is a row or case on a budgeted owner, never a new surface; mechanics live at the `.planning/` page#cluster anchor named on each row, and the owner's realization state is read from `ARCHITECTURE.md` `[OWNER_REGISTRY]`.

## [1]-[DURABLE_EXECUTION]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                      |
| :-----: | :----------------------------------------------------------------------------- | :--------------------------------- |
|   [1]   | Closed durable-unit family: workflow/activity/clock/deferred/queue/rate-limiter  | durable-execution#DURABLE_EXECUTION |
|   [2]   | Cluster engine: WorkflowEngine over Sharding + MessageStorage                     | durable-execution#DURABLE_EXECUTION |
|   [3]   | One AI provider literal axis collapsing the five provider satellites              | durable-execution#DURABLE_EXECUTION |
|   [4]   | Agent journal Model.Class: session/tool-call/checkpoint/complete                  | durable-execution#DURABLE_EXECUTION |
|   [5]   | Resilience primitives: circuit-breaker, retry, JSON-patch over the fault family   | durable-execution#DURABLE_EXECUTION |
|   [6]   | Durable saga compensation: forward/compensating pairs with reverse-order rollback  | durable-execution#DURABLE_EXECUTION |

## [2]-[PERSISTENCE_AND_SEARCH]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]               |
| :-----: | :----------------------------------------------------------------------------- | :-------------------------- |
|   [7]   | One SQL boundary: PgClient, Model.Class per entity, Migrator                      | persistence#STORE_BOUNDARY   |
|   [8]   | Entity registry of ~15 entities with projections via Model.fields/Schema.pick     | persistence#STORE_BOUNDARY   |
|   [9]   | Multi-tenant RLS: app.current_tenant GUC + lifecycle + purge family               | persistence#TENANCY          |
|  [10]   | Jobs/DLQ, event journal, and notification channel matrix as Model.Class rows       | persistence#WORK_AND_SIGNALS |
|  [11]   | Asset-transfer codec axis: csv/xlsx/pdf/archive export                             | persistence#WORK_AND_SIGNALS |
|  [12]   | Feature flags as 0-100 percentage-bucket rollout integers                          | persistence#WORK_AND_SIGNALS |
|  [13]   | Fused weighted-rank hybrid search: semantic + lexical + trigram + phonetic          | hybrid-search#HYBRID_SEARCH  |

## [3]-[RPC_AND_PROVISIONING]

| [INDEX] | [FEATURE]                                                                       | [PAGE#CLUSTER]                     |
| :-----: | :----------------------------------------------------------------------------- | :-------------------------------- |
|  [14]   | One internal RPC RpcGroup with WorkflowProxy derived over the same Schema         | internal-rpc#INTERNAL_RPC          |
|  [15]   | Four-row runner backplane: protocol/message-storage/runner-storage/runner-health  | internal-rpc#RUNNER_AND_SCHEDULING |
|  [16]   | Scheduled work: exactly-one-runner pin, shard-pinned cron                          | internal-rpc#RUNNER_AND_SCHEDULING |
|  [17]   | Two-mode IaC tier model: data/compute/observe × cloud/self-hosted                  | provisioning#PROVISIONING          |
|  [18]   | Deploy lifecycle driver: up/preview/refresh/destroy CLI verbs                       | provisioning#PROVISIONING          |
|  [19]   | Cross-stack topology outputs: DSN/object-store/redis/otlp/spa-origin                 | provisioning#PROVISIONING          |
|  [20]   | Secret + policy boundary: Doppler/ESC resolution with advisory/mandatory guard rules | provisioning#PROVISIONING          |
|  [21]   | Provisioned observability stack: Alloy OTLP -> Prometheus -> Grafana                 | provisioning#PROVISIONING          |
