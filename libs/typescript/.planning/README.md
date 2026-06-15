# [TYPESCRIPT_PLANNING]

Rasm TypeScript owns the web UI: the co-hosted SPA, the evidence and benchmark dashboards, and the companion control panels. The four .NET packages ship the complete wire surface; TS consumes every contract as settled vocabulary and never re-designs a wire shape. Four ordered stages build the lib branch as foundationally sound lib-grade code.

## [1]-[PAGE_INDEX]

The two wire pages plus the eight-domain deep corpus are authored; the deep pages land at stage [2.D] and the TS region ledger registers them.

| [INDEX] | [PAGE]                                          | [OWNS]                                                                 | [STATE]   |
| :-----: | :---------------------------------------------- | :--------------------------------------------------------------------- | :-------- |
|   [1]   | [wire-consumption](wire-consumption.md)         | Contract inventory, codegen tooling, codec posture, tolerance law      | authored  |
|   [2]   | [architecture-posture](architecture-posture.md) | Effect app shape, state layer, host topology                           | authored  |
|   [3]   | [wire-contracts](wire-contracts.md)             | Byte-to-typed boundary; transport, five decode rails, quarantine fold  | finalized |
|   [4]   | [state-stores](state-stores.md)                 | Key-discriminated folds, envelope carrier, staleness and availability  | finalized |
|   [5]   | [view-surfaces](view-surfaces.md)               | Atom-bound leaf render and read-only observation routes                | finalized |
|   [6]   | [control-edge](control-edge.md)                 | The write edge; command gateway and intent registry                    | finalized |
|   [7]   | [runtime-host](runtime-host.md)                 | SPA root, Layer graph, platform, self-telemetry, build, worker pool    | finalized |
|   [8]   | [node-tier](node-tier.md)                       | Node-only deploy and durable-work surfaces; observability provisioning | partial   |

## [2]-[EXECUTION_STAGES]

Each stage's output is the next stage's sole input; a stage closes only when its gate holds.

| [INDEX] | [STAGE]                    | [OUTPUT]                                                    | [GATE]                                                          |
| :-----: | :------------------------- | :---------------------------------------------------------- | :-------------------------------------------------------------- |
|   [A]   | root infra finalization    | refreshed catalog, regenerated lockfile, closed root drift  | registry probes match; one install resolves; tsgo proof green   |
|   [B]   | lib scaffolding            | one real workspace package under `libs/typescript/`         | package tsgo proof green; workspace row links; zero C# coupling |
|   [C]   | api extraction             | per-dependency surface catalogues; executed wire admissions | every admitted dependency owns a catalogue page                 |
|   [D]   | planning corpus completion | deep pages, TS region ledger, finalized PAGE_INDEX rows     | cold-grader all-PASS sweeps; zero accepted findings             |

### [2.A]-[ROOT_INFRA_FINALIZATION]

The catalog refresh moves every consumed row in `pnpm-workspace.yaml` to newest stable in one pass. The stage gate is three proofs:

- Registry probe: `pnpm view <package> dist-tags.latest` per refreshed row; the catalog cell equals the probe output exactly under the `saveExact` law.
- One install: a single `pnpm install` resolves the entire refreshed catalog under `catalogMode: strict`.
- Typecheck proof: `pnpm exec tsgo --noEmit -p tsconfig.json` is the canonical compiler proof; `pnpm exec tsc --noEmit -p tsconfig.json` is a peer-bridge smoke.

Root infra drift closes in the same pass:

- Dependency-usage truth: every root package entry re-justifies against a real consumer or is deleted; a row whose package leaves the catalog is deleted with it.
- Lockfile law: every catalog or override edit regenerates `pnpm-lock.yaml` in the same change; workspace-owned overrides and `peerDependencyRules` rows re-validate against the refreshed catalog and obsolete rows are deleted.
- Engine alignment: `engines.node` and `packageManager` stay exact and bump to newest stable in the same pass.

### [2.B]-[LIB_SCAFFOLDING_LAW]

The lib branch is one real pnpm workspace package. The package name is decided at scaffolding and registered in the region ledger.

- `pnpm-workspace.yaml` `packages` gains the `libs/typescript/*` row; the package lives at `libs/typescript/<pkg>` with its own `package.json` carrying `catalog:` references only.
- The package owns its `tsconfig.json`, extending the root `tsconfig.base.json` and registering as a `references` row in the root solution `tsconfig.json`.
- Zero coupling to the C# tree: no import, path mapping, or build edge resolves into `libs/csharp`; integration crosses only the contracts inventoried at [wire-consumption](wire-consumption.md).
- Single entry point: one `exports` map with one public root; internal modules never publish.

Every module scaffolds under the Effect-first doctrine that [architecture-posture](architecture-posture.md) owns: one TS form per altitude, one owner per axis, admission once at the wire edge.

### [2.C]-[API_EXTRACTION]

Catalogue truth for every TS dependency — the TS analog of the C# per-package `.api` catalogues.

- The admission-pending rows of the ADMISSIONS_RECORD land in the catalog here.
- Extraction route: per admitted package, the surface catalogue derives from installed package source — the `node_modules/<pkg>` `exports` map resolved to its types entries, with extraction probes over the published `.d.ts` rollups. One catalogue page per package lands at `libs/typescript/<pkg>/.api/`.
- Resolution law: every API member written into a later planning fence resolves against a catalogue page; an unresolvable member becomes a RESEARCH row with an executable probe route, never prose.
- The ADMISSIONS_RECORD discharges every row at this stage and stage [2.A]; the testing stack is admitted here as well, derived from catalogue truth rather than carried forward.

### [2.D]-[PLANNING_CORPUS_COMPLETION]

- Deep pages author by the campaign method at `libs/csharp/.planning/campaign-method.md` to the review-law bar of the suite planning standard at `libs/csharp/.planning/README.md` — the suite review law with TS overlays: `ts contract` fences are the signature law; package resolution routes to the stage [2.C] catalogues; the comment law, hedge law, and catalogue law apply unchanged.
- The TS region ledger `libs/typescript/.planning/region-map/` is created when deep-page authoring starts, mirroring the suite ledger protocol: provisional rows before authoring, an owner-symbol registry, FINAL flips on the cold all-PASS sweep.
- Ideation-first refinement: per-page blueprint decisions — owner, axis, store, and layer assignments — precede authoring; the two wire pages of this corpus are the settled vocabulary every deep page composes.
- Review-repair waves: a cold grader sweeps each page; the page repairs and re-sweeps until zero accepted findings; only an all-PASS sweep flips a PAGE_INDEX row to finalized.

## [3]-[ADMISSIONS_RECORD]

The admission record owns package posture and catalogue state. Concrete package coordinates live in the workspace catalog, not in planning prose. The net-edit ledger holds one flagged-pending deletion, two comment-only security-line annotations, and zero version bumps; `pnpm-lock.yaml` is untouched.

| [PACKAGE]                  | [PAGE]               | [CATALOGUE]                                                                                                                                                                                     |
| :------------------------- | :------------------- | :---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| typescript                 | architecture-posture | catalog                                                                                                                                                                                         |
| effect                     | architecture-posture | catalog; core pin confirmed past the async-local-storage context-loss patched line; v3 held, v4 a designed-only growth row with unstable-import-path risk; consumers node-tier and runtime-host |
| @effect/platform           | architecture-posture | catalog                                                                                                                                                                                         |
| @effect/platform-browser   | architecture-posture | catalog                                                                                                                                                                                         |
| @effect/platform-node      | node-tier            | catalog                                                                                                                                                                                         |
| @effect/rpc                | node-tier            | catalog; RPC pin confirmed past the async-local-storage context-loss patched line; consumers node-tier and runtime-host                                                                         |
| @effect/cluster            | node-tier            | catalog; successor present at frontier                                                                                                                                                          |
| @effect/workflow           | node-tier            | catalog; successor present at frontier                                                                                                                                                          |
| @effect/cluster-workflow   | node-tier            | flagged-pending; superseded predecessor, zero root consumers, deletion deferred to stage [2.C] consumer sweep                                                                                   |
| @effect/sql                | node-tier            | catalog                                                                                                                                                                                         |
| @effect/sql-pg             | node-tier            | catalog                                                                                                                                                                                         |
| @effect/opentelemetry      | runtime-host         | catalog                                                                                                                                                                                         |
| @effect-atom/atom          | state-stores         | catalog                                                                                                                                                                                         |
| @effect-atom/atom-react    | view-surfaces        | catalog                                                                                                                                                                                         |
| react                      | architecture-posture | catalog                                                                                                                                                                                         |
| react-dom                  | architecture-posture | catalog                                                                                                                                                                                         |
| vite                       | architecture-posture | catalog                                                                                                                                                                                         |
| @typescript/native-preview | architecture-posture | research; registry latest advanced past the catalog dev-nightly pin at re-probe; row demoted pending stage-[2.A] catalog refresh                                                                |
| @connectrpc/connect        | wire-consumption     | admission pending                                                                                                                                                                               |
| @connectrpc/connect-web    | wire-consumption     | admission pending                                                                                                                                                                               |
| @bufbuild/protobuf         | wire-consumption     | admission pending                                                                                                                                                                               |
| @bufbuild/protoc-gen-es    | wire-consumption     | admission pending                                                                                                                                                                               |
| @bufbuild/buf              | wire-consumption     | admission pending                                                                                                                                                                               |
| @msgpack/msgpack           | wire-consumption     | admission pending                                                                                                                                                                               |
