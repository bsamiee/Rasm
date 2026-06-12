# [TYPESCRIPT_PLANNING]

Rasm TypeScript owns the web UI: the co-hosted SPA, the evidence and benchmark dashboards, and the companion control panels. The four .NET packages ship the complete wire surface; TS consumes every contract as settled vocabulary and never re-designs a wire shape. The stack is the Effect ecosystem inside the pnpm workspace, landing after the workspace TypeScript line bumps to 7. Pages are decision-complete blueprints the implementation session transcribes; the suite planning standard at `libs/csharp/.planning/README.md` governs deep-page authoring with TS overlays, and a TS region ledger is created when deep-page authoring starts.

## [1]-[PAGE_INDEX]

| [INDEX] | [PAGE]                                            | [OWNS]                                                          | [STATE] |
| :-----: | :------------------------------------------------ | :--------------------------------------------------------------- | :------ |
|   [1]   | [wire-consumption](wire-consumption.md)           | Contract inventory, codegen tooling, codec posture, tolerance law | planned |
|   [2]   | [architecture-posture](architecture-posture.md)   | Effect app shape, state layer, host topology, testing law        | planned |

## [2]-[CONSUMPTION_LAW]

Seven binding rows fix how TS consumes the .NET wire. Each row names the producing page; the producer's TS_PROJECTION fence is the contract and TS adds nothing beside it.

| [INDEX] | [SURFACE]              | [PRODUCER]                              | [TS_CONSUMPTION]                                                        |
| :-----: | :--------------------- | :--------------------------------------- | :----------------------------------------------------------------------- |
|   [1]   | proto vocabulary       | Compute/remote-lane                      | connect-es codegen over the app-root-emitted descriptor set              |
|   [2]   | snapshot blobs         | Persistence/snapshot-codecs              | @msgpack/msgpack decode over the SnapshotCodec rows                       |
|   [3]   | JSON runtime records   | AppHost/runtime-ports merged contexts    | STJ Strict camelCase records; `Schema.Class` decode drift-gated by the emitted schema set |
|   [4]   | geometry JSON          | Persistence/snapshot-codecs GeoJSON rail | GeoJSON features on map and geo dashboard series; projection law at Compute/remote-lane |
|   [5]   | telemetry signals      | AppHost OTLP exporter at app roots       | collector ingestion; dashboards read the collector, never a bespoke wire  |
|   [6]   | command deep links     | AppUi/commands-availability              | CommandIntent string keys drive remote invocation and deep links          |
|   [7]   | evidence dashboard     | AppUi/diagnostics-evidence               | EvidenceReceipt timeline ingestion through the receipt envelope wire      |

## [3]-[TOPOLOGY_LAW]

- The web-service app root co-hosts the built SPA: static assets and the grpc-web endpoint share one origin, so CORS is zero day-one and the CORS header row is designed-only growth for a cross-origin deployment.
- Bearer is the browser credential case on the Compute `CredentialPolicy` axis; mTLS client certificates and UDS peer-credential are structurally absent in the browser.
- grpc-web carries unary and server-stream only; client-stream and bidi are structurally absent on the browser row, ArtifactSync bidi is excluded there, and the browser collaboration decomposition (server-stream down, unary up) is designed-only growth of rpc rows on the .NET side.
- The view layer renders through the React line in the workspace catalog; Effect owns every concern below the component boundary, and components subscribe at the edge.

## [4]-[WORK_PLAN]

A later campaign session executes four stages with zero guessing, mirroring the .NET campaign's method. Each stage's output is the next stage's sole input.

| [INDEX] | [STAGE]                | [INPUT]                                                          | [OUTPUT]                                              | [GATE]                                            |
| :-----: | :--------------------- | :---------------------------------------------------------------- | :------------------------------------------------------ | :-------------------------------------------------- |
|   [1]   | catalogue truth        | pnpm catalog, emitted descriptor set, eleven TS_PROJECTION clusters | executed admissions; generated `*_pb.ts` surface catalogue | `pnpm install` resolves; `buf generate` emits        |
|   [2]   | refinement blueprints  | wire-consumption, architecture-posture, `coding-ts` skill          | per-page owner, service, layer, and store decisions      | every owner maps to exactly one axis                 |
|   [3]   | page authoring         | blueprints                                                         | deep pages under the full machine-check grammar          | cold-grader all-PASS sweep with review-repair        |
|   [4]   | charters               | finalized pages                                                    | build order, file process, proof gates per app           | PAGE_INDEX rows flip finalized                       |

- Stage 1 pins the unversioned admission rows (`@bufbuild/buf`, the Stryker line) into the catalog and verifies the exact peer pins across the connect line in one resolve.
- Stage 3 adapts the suite page grammar to TS: `ts contract` fences are the signature law, owner symbols register in the TS ledger, and the wire pages of this corpus are the settled vocabulary every deep page composes.
- Stage 4 routes quality through the `testing-ts` rail recorded on the architecture-posture page.

## [5]-[ADMISSIONS_RECORD]

The only planning location where versions are written. Catalog rows exist in `pnpm-workspace.yaml` today; admission-pending rows land at the catalogue-truth stage.

| [PACKAGE]                  | [VERSION]                          | [PAGE]               | [CATALOGUE]        |
| :------------------------- | :--------------------------------- | :------------------- | :----------------- |
| typescript                 | 6.0.3                              | architecture-posture | catalog            |
| effect                     | 3.21.2                             | architecture-posture | catalog            |
| @effect/platform           | 0.96.1                             | architecture-posture | catalog            |
| @effect/platform-browser   | 0.76.0                             | architecture-posture | catalog            |
| react                      | 19.3.0-canary-f4e0d4ed-20260429    | architecture-posture | catalog            |
| react-dom                  | 19.3.0-canary-f4e0d4ed-20260429    | architecture-posture | catalog            |
| vite                       | 8.0.10                             | architecture-posture | catalog            |
| vitest                     | 4.1.5                              | architecture-posture | catalog            |
| @effect/vitest             | 0.29.0                             | architecture-posture | catalog            |
| @vitest/coverage-v8        | 4.1.5                              | architecture-posture | catalog            |
| fast-check                 | 4.7.0                              | architecture-posture | catalog            |
| @connectrpc/connect        | 2.1.1                              | wire-consumption     | admission pending  |
| @connectrpc/connect-web    | 2.1.1                              | wire-consumption     | admission pending  |
| @bufbuild/protobuf         | 2.12.0                             | wire-consumption     | admission pending  |
| @bufbuild/protoc-gen-es    | 2.12.0                             | wire-consumption     | admission pending  |
| @bufbuild/buf              | pin at catalogue truth             | wire-consumption     | admission pending  |
| @msgpack/msgpack           | 3.1.3                              | wire-consumption     | admission pending  |
| @stryker-mutator/core      | pin at catalogue truth             | architecture-posture | admission pending  |
