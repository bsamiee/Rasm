# [TS_TASKLOG]

Branch-level open work for `libs/typescript`. Per-domain open work lives in each charter; this log carries the cross-domain finalization-and-deepening work-set. Closed rows live in neither this log nor the charters.

## [1]-[SPIKE_RESOLUTION]

- XxHash128 byte-identity harness (tier-2, highest-risk): prove `xxhash-wasm` `h128` over the assembled artifact blob is byte-identical to C# `System.IO.Hashing.XxHash128` digest with seed=0, fixed endianness, and two-64-bit-half byte order, in throwaway scaffolding against both runtimes; fold the verified spelling into `ArtifactFrameRail` and flip the interchange DENSITY_BAR `ArtifactFrameRail` SPIKE→FINALIZED. Until closed, the content-addressed cache is untrusted across C#, the TS browser worker pool, and the future Python companion.

## [2]-[CROSS_DOMAIN_FINALIZATION]

- Cold all-PASS sweep flipping every charter PAGE_INDEX `[STATE]` to finalized once the twelve domain pages and four branch pages read clean against the suite review law.
- Reconcile `architecture-tree.md` and `test-strategy.md` against the four-domain owner set each loop; re-derive the source tree from the charter BUILD_ORDER.
- Verify the RULE_ENFORCEMENT guards materialize inside the ONE package: the `projection/**` `no-restricted-imports` `@connectrpc/*` ban, the `./provisioning` subpath export isolating the `services/provisioning/**` closure, and the folder-scoped `browser`/`node`/`neutral` `no-restricted-imports` strata land as real ESLint flat-config + single-`package.json`-subpath-`exports` facts at implementation start.

## [3]-[CROSS_BRANCH_PRECONDITION_DAG]

- GlbViewport WebGL mesh render: waits on C# `remote-lane#TS_PROJECTION` promoting `GeometryPayload(mesh)`/`MeshTensor`, then the Python `libs/python/compute` IFC->GLB companion; TS is the last node. Recorded in region-map/seam-splits.md.
- GraphFork CRDT projection-fold row: waits on the C# `sync-collaboration#MERGE_LAW` op-log amendment before `projection` folds a CRDT op vocabulary.
- Capability-descriptor SDK + MCP client codegen row on `interchange`: waits on the C# `capability-registry#CAPABILITY_CATALOG`.
- BCF anchor-algebra `ui` render-surface + `interchange` anchor-codec: waits on the C# `annotation#ANCHOR_ALGEBRA`.

## [4]-[NEXT_LOOP_DEEPENING]

- Drive each domain's REFINEMENT_HORIZON: deeper codec streaming on interchange; the standing-query window vocabulary (tumbling/sliding/session + watermarks) on projection; deeper render GPU paths and the component-system role breadth on web; deeper durable saga composition, hybrid-search re-ranking, and IaC lifecycle on services.
- Grade every existing owner against the world-class bar each closeout; name surface-level owners and under-exploited admitted packages; push the densest owner deeper rather than adding parallel surfaces.
