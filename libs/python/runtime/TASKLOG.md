# [PY_RUNTIME_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[FLOOR_AND_SERVER_PROBES]

Probes gated on the sub-3.15 companion environment and the live server boot; each is named in its page RESEARCH cluster.

| [INDEX] | [ITEM]                                                                                                 | [PAGE#CLUSTER]    | [STATUS] |
| :-----: | :----------------------------------------------------------------------------------------------------- | :---------------- | :------: |
|   [1]   | `grpc.aio` server boots against the inbound companion descriptors under the anyio runner               | server-host#SERVE |  SPIKE   |
|   [2]   | Credential intake (token/keyring/insecure-loopback) gates the connecting peer on the inbound serve     | server-host#SERVE |  SPIKE   |
|   [3]   | `grpcio`/`grpcio-tools`/`protobuf` reflect on the cp312 companion floor; first-class admission lands at the lock-scope decision | server-host#SERVE |  BLOCKED |

## [2]-[CATALOGUE_AND_IDENTITY_GATES]

Manifest and `.api` gaps and the identity-seed proof; no live server required.

| [INDEX] | [ITEM]                                                                                          | [PAGE#CLUSTER]            | [STATUS] |
| :-----: | :--------------------------------------------------------------------------------------------- | :----------------------- | :------: |
|   [1]   | The companion server wire (`grpcio`/`grpcio-tools`/`protobuf`) resolves to `.api` rows once the floor/lock-scope decision admits the sub-3.15 environment | server-host#SERVE         | BLOCKED  |
|   [2]   | `ContentIdentity` seed proven byte-identical against the cross-boundary content key at the seam | content-identity#IDENTITY | SPIKE    |

## [3]-[TRANSCRIPTION]

The implementation sequence is the `ARCHITECTURE.md` `[SOURCE_TREE]` build order (vocabulary owners before consumers, `faults.py` through `evidence.py`); each file transcribes its page clusters verbatim and resolves the RESEARCH rows those pages carry. Production source is absent.

| [INDEX] | [ITEM]                                                                          | [PAGE#CLUSTER]         | [STATUS] |
| :-----: | :----------------------------------------------------------------------------- | :--------------------- | :------: |
|   [1]   | Transcribe the BUILD_ORDER files per `ARCHITECTURE.md` `[SOURCE_TREE]`         | rails-resilience#FAULT | QUEUED   |
