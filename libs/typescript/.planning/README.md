# [TYPESCRIPT_PLANNING]

Rasm TypeScript owns the web UI: the co-hosted SPA, the evidence and benchmark dashboards, and the companion control panels. The four .NET packages ship the complete wire surface; TS consumes every contract as settled vocabulary and never re-designs a wire shape. This charter is the instruction set a cold session executes in order — four stages create the TS lib branch as foundationally sound lib-grade code, not a loose collection of folders and files. The corpus is planning and instruction only; versions appear in exactly one location, the ADMISSIONS_RECORD.

## [1]-[PAGE_INDEX]

Deep pages land at stage [2.4] and register here as rows.

| [INDEX] | [PAGE]                                          | [OWNS]                                                            | [STATE] |
| :-----: | :---------------------------------------------- | :---------------------------------------------------------------- | :------ |
|   [1]   | [wire-consumption](wire-consumption.md)         | Contract inventory, codegen tooling, codec posture, tolerance law  | planned |
|   [2]   | [architecture-posture](architecture-posture.md) | Effect app shape, state layer, host topology                       | planned |

## [2]-[EXECUTION_STAGES]

Each stage's output is the next stage's sole input; a stage closes only when its gate holds.

| [INDEX] | [STAGE]                     | [OUTPUT]                                                      | [GATE]                                                        |
| :-----: | :-------------------------- | :------------------------------------------------------------ | :------------------------------------------------------------ |
|   [A]   | root infra finalization     | refreshed catalog, regenerated lockfile, closed root drift     | registry probes match; one install resolves; knip green        |
|   [B]   | lib scaffolding             | one real workspace package under `libs/typescript/`            | `tsc -b` green; workspace row links; zero C# coupling          |
|   [C]   | api extraction              | per-dependency surface catalogues; executed wire admissions    | every admitted dependency owns a catalogue page                |
|   [D]   | planning corpus completion  | deep pages, TS region ledger, finalized PAGE_INDEX rows        | cold-grader all-PASS sweeps; zero accepted findings            |

### [2.A]-[ROOT_INFRA_FINALIZATION]

The catalog refresh moves every consumed row in `pnpm-workspace.yaml` to newest stable in one pass: the `typescript` row to the TypeScript 7 line, the effect stack (`effect`, `@effect/platform`, `@effect/platform-browser`), the react line, `vite` and its plugin rows, and the tooling rows the workspace consumes. The verification route is three proofs:

- Registry probe: `pnpm view <package> version` per refreshed row; the catalog cell equals the probe output exactly under the `saveExact` law.
- One install: a single `pnpm install` resolves the entire refreshed catalog under `catalogMode: strict`.
- Typecheck proof: `pnpm exec tsc --version` resolves the bumped compiler line; the stage [2.B] gate `pnpm exec tsc -b` is the full typecheck discharge.

Root infra drift closes in the same pass:

- Knip truth: `pnpm exec knip` runs green; every `knip.ignoreDependencies` pattern in the root `package.json` re-justifies against a real consumer or is deleted; a row whose package leaves the catalog is deleted with it.
- Lockfile law: every catalog or override edit regenerates `pnpm-lock.yaml` in the same change; `pnpm.overrides` and `peerDependencyRules` rows re-validate against the refreshed catalog and obsolete rows are deleted.
- Engine pins: `engines.node` and `packageManager` stay exact pins and bump to newest stable in the same pass.

### [2.B]-[LIB_SCAFFOLDING_LAW]

The lib branch is one real pnpm workspace package. The package name is decided at scaffolding and registered in the region ledger.

- `pnpm-workspace.yaml` `packages` gains the `libs/typescript/*` row; the package lives at `libs/typescript/<pkg>` with its own `package.json` carrying `catalog:` pins only, never literal versions.
- The package owns its `tsconfig.json`, extending the root `tsconfig.base.json` and registering as a `references` row in the root solution `tsconfig.json`.
- Zero coupling to the C# tree: no import, path mapping, or build edge resolves into `libs/csharp`; integration crosses only the wire contracts of the CONSUMPTION_LAW.
- Single entry point: one `exports` map with one public root; internal modules never publish.

The Effect-first doctrine is binding on every module:

- One Effect rail per concern: `Effect<A, E, R>` carries effects, `Either` carries pure branching, `Stream` carries server-streams; thrown exceptions never cross a module boundary.
- Typed failures: `Data.TaggedError` families, one fault family per rail; `FaultDetailWire` reconstructs the .NET faults as one tagged family.
- Admission once: `Schema` decode at the wire edge; the interior never re-validates; `Option` carries absence and `null` exists only at the JSON boundary.
- One owner per axis: `Effect.Service` classes; a new capability lands as a method or row on an existing owner, never as a sibling service.
- One composition root: one `Layer` graph per host surface; one runtime.
- Density law: no thin wrappers over admitted packages, no helper or utility files, no parallel types for one concept; polymorphic collapse precedes any new entrypoint; fewer deep surfaces over many loose ones.

### [2.C]-[API_EXTRACTION]

Catalogue truth for every TS dependency — the TS analog of the C# per-package `.api` catalogues.

- Admission-pending rows land in the catalog here; the connect line moves together in one resolve with exact peer pins: `@connectrpc/connect`, `@connectrpc/connect-web`, `@bufbuild/protobuf`, `@bufbuild/protoc-gen-es`, `@bufbuild/buf`, plus `@msgpack/msgpack`.
- Extraction route: per admitted package, the surface catalogue derives from installed package source — the `node_modules/<pkg>` `exports` map resolved to its types entries, with extraction probes over the published `.d.ts` rollups. One catalogue page per package lands at `libs/typescript/<pkg>/.api/`.
- Resolution law: every API member written into a later planning fence resolves against a catalogue page; an unresolvable member becomes a RESEARCH row with an executable probe route, never prose.
- The ADMISSIONS_RECORD discharges every PIN cell at this stage and stage [2.A]; the testing stack is admitted and pinned here as well, derived from catalogue truth rather than carried forward.

### [2.D]-[PLANNING_CORPUS_COMPLETION]

- Deep pages author by the campaign method at `libs/csharp/.planning/campaign-method.md` to the review-law bar of the suite planning standard at `libs/csharp/.planning/README.md` — the suite review law with TS overlays: `ts contract` fences are the signature law; package resolution routes to the stage [2.C] catalogues; the comment law, hedge law, and version law apply unchanged.
- The TS region ledger `libs/typescript/.planning/region-map/` is created when deep-page authoring starts, mirroring the suite ledger protocol: provisional rows before authoring, an owner-symbol registry, FINAL flips on the cold all-PASS sweep.
- Ideation-first refinement: per-page blueprint decisions — owner, axis, store, and layer assignments — precede authoring; the two wire pages of this corpus are the settled vocabulary every deep page composes.
- Review-repair waves: a cold grader sweeps each page; the page repairs and re-sweeps until zero accepted findings; only an all-PASS sweep flips a PAGE_INDEX row to finalized.

## [3]-[CONSUMPTION_LAW]

The eleven TS_PROJECTION clusters across the four .NET planning corpora are the complete contract surface; the wire-consumption inventory transcribes them. Seven binding rows fix how TS consumes the wire: each row names the producing page, the producer's TS_PROJECTION fence is the contract, and TS adds nothing beside it.

| [INDEX] | [SURFACE]              | [PRODUCER]                                | [TS_CONSUMPTION]                                                                           |
| :-----: | :--------------------- | :---------------------------------------- | :------------------------------------------------------------------------------------------ |
|   [1]   | proto vocabulary       | Compute/remote-lane                        | connect-es codegen over the app-root-emitted descriptor set                                  |
|   [2]   | snapshot blobs         | Persistence/snapshot-codecs                | @msgpack/msgpack decode over the SnapshotCodec rows                                          |
|   [3]   | JSON runtime records   | AppHost/runtime-ports merged contexts      | STJ Strict camelCase records; `Schema.Class` decode drift-gated by the emitted schema set    |
|   [4]   | geometry JSON          | Persistence/snapshot-codecs GeoJSON rail   | GeoJSON features on map and geo dashboard series; projection law at Compute/remote-lane      |
|   [5]   | telemetry signals      | AppHost OTLP exporter at app roots         | collector ingestion; dashboards read the collector, never a bespoke wire                     |
|   [6]   | command deep links     | AppUi/commands-availability                | CommandIntent string keys drive remote invocation and deep links                             |
|   [7]   | evidence dashboard     | AppUi/diagnostics-evidence                 | EvidenceReceipt timeline ingestion through the receipt envelope wire                         |

## [4]-[TOPOLOGY_LAW]

- The web-service app root co-hosts the built SPA: static assets and the grpc-web endpoint share one origin, so CORS is zero day-one and the CORS header row is designed-only growth for a cross-origin deployment.
- Bearer is the browser credential case on the Compute `CredentialPolicy` axis; mTLS client certificates and UDS peer-credential are structurally absent in the browser.
- grpc-web carries unary and server-stream only; client-stream and bidi are structurally absent on the browser row, ArtifactSync bidi is excluded there, and the browser collaboration decomposition (server-stream down, unary up) is designed-only growth of rpc rows on the .NET side.
- The view layer renders through the React line in the workspace catalog; Effect owns every concern below the component boundary, and components subscribe at the edge.

## [5]-[ADMISSIONS_RECORD]

The only planning location where versions are written. Catalog rows carry the current pin and re-pin at stage [2.A]; admission-pending rows pin at stage [2.C]. Every row discharges its PIN cell at catalogue truth.

| [PACKAGE]                | [VERSION]                       | [PAGE]               | [CATALOGUE]       | [PIN]                  |
| :----------------------- | :------------------------------ | :------------------- | :---------------- | :--------------------- |
| typescript               | 6.0.3                           | architecture-posture | catalog           | pin at catalogue truth |
| effect                   | 3.21.2                          | architecture-posture | catalog           | pin at catalogue truth |
| @effect/platform         | 0.96.1                          | architecture-posture | catalog           | pin at catalogue truth |
| @effect/platform-browser | 0.76.0                          | architecture-posture | catalog           | pin at catalogue truth |
| react                    | 19.3.0-canary-f4e0d4ed-20260429 | architecture-posture | catalog           | pin at catalogue truth |
| react-dom                | 19.3.0-canary-f4e0d4ed-20260429 | architecture-posture | catalog           | pin at catalogue truth |
| vite                     | 8.0.10                          | architecture-posture | catalog           | pin at catalogue truth |
| @connectrpc/connect      | 2.1.1                           | wire-consumption     | admission pending | pin at catalogue truth |
| @connectrpc/connect-web  | 2.1.1                           | wire-consumption     | admission pending | pin at catalogue truth |
| @bufbuild/protobuf       | 2.12.0                          | wire-consumption     | admission pending | pin at catalogue truth |
| @bufbuild/protoc-gen-es  | 2.12.0                          | wire-consumption     | admission pending | pin at catalogue truth |
| @bufbuild/buf            | unpinned                        | wire-consumption     | admission pending | pin at catalogue truth |
| @msgpack/msgpack         | 3.1.3                           | wire-consumption     | admission pending | pin at catalogue truth |
