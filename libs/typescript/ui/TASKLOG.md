# [UI_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[BLOCKED_SEAMS]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | `GlbViewport` mesh-DECODE seam: the upstream mesh wire type must be promoted out of the proto vocabulary into the projection fence (routed through the Tier-0 seam ledger) before `decodeMeshView` consumes the generated descriptor; the backend/draw/camera owners are authored against the in-memory `MeshView` | render-surfaces#GLB_VIEWPORT | BLOCKED |

## [2]-[ADMISSION_GATES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | The four WebGL packages are admitted-on-precondition; their catalog rows activate only once the mesh wire promotion lands | render-surfaces#GLB_VIEWPORT | BLOCKED |
| [2] | BCF anchor-algebra render surface waits on the upstream anchor-algebra fence routed through the Tier-0 seam ledger | render-surfaces#RENDER_SURFACES | BLOCKED |
| [3] | Implementation-time root/folder `package.json` `./ui` subpath `exports` authored at transcription | component-system#COMPONENT_SYSTEM | QUEUED |

## [3]-[TRANSCRIPTION]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order modules per `ARCHITECTURE.md` `[SOURCE_TREE]` (`binding.ts` before `component-system/` before `render-surfaces.ts` before `index.ts`); each module transcribes its page clusters verbatim | binding#BINDING | QUEUED |
