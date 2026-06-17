# [PROJECTION_TASKLOG]

Open work owned by this folder; closed items do not appear. `[STATUS]` is one of `QUEUED`, `ACTIVE`, `BLOCKED`, `SPIKE`; owner state is read at `ARCHITECTURE.md` `[OWNER_REGISTRY]`. Every `SPIKE` row names the probe that flips its owner registry cell to `FINALIZED`.

## [1]-[PROBES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Standing-query window fold: live-changefeed confirmation of tumbling/sliding/session bucketing and late-row signed-delta retract against a running op-log replay | fold-algebra#FOLD_ALGEBRA | SPIKE |
| [2] | LWW convergent fold: cross-peer strong-eventual-consistency harness proving two divergent-delivery folds reach a byte-identical state against the live adjudication | fold-algebra#FOLD_ALGEBRA | SPIKE |

## [2]-[ADMISSION_GATES]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | GraphFork CRDT op vocabulary the convergent fold would carry waits on the upstream op-log merge-law amendment routed through the Tier-0 seam ledger | fold-algebra#FOLD_ALGEBRA | BLOCKED |
| [2] | Implementation-time root/folder `package.json` subpath `exports` authored at transcription | envelope-and-evidence#ENVELOPE_AND_EVIDENCE | QUEUED |

## [3]-[TRANSCRIPTION]

| [INDEX] | [ITEM] | [PAGE#CLUSTER] | [STATUS] |
| :-----: | ------ | -------------- | :------: |
| [1] | Transcribe the build-order modules per `ARCHITECTURE.md` `[SOURCE_TREE]` (`fold-algebra.ts` before `envelope-and-evidence.ts` before `index.ts`); each module transcribes its page clusters verbatim | fold-algebra#FOLD_ALGEBRA | QUEUED |
