# [SERVICES]

`services` is the complete node services tier of the TypeScript branch AND the infrastructure that hosts it as one concern — durable execution, the typed SQL persistence boundary with its entity registry and multi-tenant RLS, the fused hybrid-search owner, the internal RPC surface and the runner/scheduling backplane, and the two-mode IaC provisioning tier. Zero consumers exist; implementation is full-capability with no holding back; `.planning/` pages are transcribed, never re-designed. It is the node publication entry (Nx tag `scope:node`), participates in the wire topology as one peer over the same proto vocabulary (remote/companion/sidecar/hub/service), dials the capture-event client-stream that is structurally non-dialable from the browser, and integrates with the rest of the system only through the wire contracts and the companion seams routed through the Tier-0 seam ledger. The `./provisioning` exports subpath keeps the deploy-time IaC closure off the durable runtime hot path. Owner-state and the rails/axes registry live in `ARCHITECTURE.md`; the realized capability list in `FEATURES.md`; open work in `TASKLOG.md`.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                                  | [OWNS]                                                                            |
| :-----: | :----------------------------------------------------- | :------------------------------------------------------------------------------- |
|   [1]   | [durable-execution](.planning/durable-execution.md)   | WorkflowOwner, ActivityOwner, ClusterEngine, the AiProvider axis, agent journal, resilience |
|   [2]   | [persistence](.planning/persistence.md)               | SqlBoundary, the entity-model registry, multi-tenant RLS, jobs/DLQ, events, notifications, assets, flags |
|   [3]   | [hybrid-search](.planning/hybrid-search.md)           | the semantic+lexical+trigram+phonetic fused weighted-rank search owner             |
|   [4]   | [internal-rpc](.planning/internal-rpc.md)             | InternalRpc RpcGroup, WorkflowProxy projection, RunnerBackplane, ScheduledWork     |
|   [5]   | [provisioning](.planning/provisioning.md)             | the tier model, two-mode dispatch, the ./provisioning subpath, StackOutputs, bootstrap |

## [2]-[ADMISSIONS_RECORD]

Each package maps to its consuming page, central catalogue at `libs/typescript/.api/`, and admission status. Concrete coordinates live in the workspace catalog (`pnpm-workspace.yaml` `catalog:`); this table never carries a pin.

| [INDEX] | [PACKAGE]                                               | [PAGE]                                | [CATALOGUE]                                        | [STATUS]                    |
| :-----: | :----------------------------------------------------- | :------------------------------------ | :------------------------------------------------ | :-------------------------- |
|   [1]   | @effect/cluster + @effect/workflow                     | durable-execution                     | `.api/api-effect-cluster.md` + `.api/api-effect-workflow.md` | admitted          |
|   [2]   | @effect/ai + the five provider satellites               | durable-execution                     | `.api/api-effect-ai.md` + provider pages          | admitted                    |
|   [3]   | @effect/experimental                                   | durable-execution, internal-rpc       | `.api/api-effect-experimental.md`                 | admitted                    |
|   [4]   | rfc6902                                                | durable-execution                     | `.api/api-infra-data.md`                          | admitted                    |
|   [5]   | @effect/sql + @effect/sql-pg                           | persistence, hybrid-search            | `.api/api-effect-sql.md` + `.api/api-effect-sql-pg.md` | admitted              |
|   [6]   | exceljs/papaparse/jspdf/jszip/sharp                    | persistence                           | `.api/api-infra-data.md`                          | admitted                    |
|   [7]   | @effect/rpc                                            | internal-rpc                          | `.api/api-effect-rpc.md`                          | admitted                    |
|   [8]   | ioredis                                                | internal-rpc                          | `.api/api-infra-data.md`                          | admitted                    |
|   [9]   | @effect/cli                                            | provisioning                          | `.api/api-effect-cli.md`                          | admitted                    |
|  [10]   | @pulumi/* + @dopplerhq/node-sdk + @effect-aws/client-s3 | provisioning, persistence            | `.api/api-infra-data.md`                          | admitted (optional/subpath) |
|  [11]   | @effect/opentelemetry + @opentelemetry/sdk-trace-node  | provisioning                          | `.api/api-effect-opentelemetry.md`               | admitted                    |
|  [12]   | effect                                                 | all pages                             | `.api/api-effect.md`                              | admitted                    |

## [3]-[PROOF_GATES]

`[RAIL]` names the owning rail; the executable command lives with that rail owner, never restated here.

| [INDEX] | [GATE]          | [RAIL]                            | [EVIDENCE]                                          |
| :-----: | :-------------- | :-------------------------------- | :------------------------------------------------- |
|  [G1]   | catalog resolve | `pnpm` install/restore            | `catalogMode` strict resolves `@rasm/ts`            |
|  [G2]   | typecheck       | `tsgo` typecheck                  | zero diagnostics; isolatedDeclarations emits `.d.ts` |
|  [G3]   | subpath split   | node subpath import probe          | the IaC closure not loaded on the runtime path      |
|  [G4]   | durable harness | testcontainers Postgres + Redis    | exactly-once, durable-replay, RLS-scope prove       |
|  [G5]   | search harness  | testcontainers Postgres + pg_trgm/HNSW | fused weighted-rank ranks correctly            |
|  [G6]   | page render     | local mermaid-cli                 | page diagrams render through the local renderer      |
