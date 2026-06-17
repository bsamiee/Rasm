# [SERVICES_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[PROBES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Durable harness: exactly-once execution, durable-replay, and RLS-scope isolation against testcontainers Postgres + Redis | durable-execution#DURABLE_EXECUTION | SPIKE |
| [2] | Search harness: fused weighted-rank ordering against testcontainers Postgres with pg_trgm + HNSW | hybrid-search#HYBRID_SEARCH | SPIKE |
| [3] | Runner-restart harness: backplane recovery and exactly-one-runner pin survival across restart | internal-rpc#RUNNER_AND_SCHEDULING | SPIKE |
| [4] | Subpath split probe: the `@pulumi/*` deploy-time closure stays unloaded on the runtime path under the `./provisioning` subpath | provisioning#PROVISIONING | SPIKE |

## [2]-[ADMISSION_GATES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | GraphFork CRDT op vocabulary the durable hub peer would fold waits on the upstream op-log merge-law amendment routed through the Tier-0 seam ledger | internal-rpc#RUNNER_AND_SCHEDULING | BLOCKED |
| [2] | Implementation-time root/folder `package.json` `./node` + `./provisioning` subpath `exports` authored at transcription | provisioning#PROVISIONING | QUEUED |

## [3]-[TRANSCRIPTION]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order modules per `ARCHITECTURE.md` `[SOURCE_TREE]` (`persistence.ts` through `node.ts` then `index.ts`); each module transcribes its page clusters verbatim | persistence#STORE_BOUNDARY | QUEUED |
