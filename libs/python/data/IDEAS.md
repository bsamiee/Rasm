# [PY_DATA_IDEAS]

The forward pool of higher-order concepts for `data`, grounded in the host-free interchange role. Each idea is a card — slug leader plus the capability, what it unlocks, and the gap or modern technique it draws on — and spawns one or more tasks in `TASKLOG.md`. `[1]-[OPEN]` holds live concepts; `[2]-[CLOSED]` records dispositions so an idea is never re-litigated.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[MULTIMODAL]-[QUEUED]: a multimodal AI-asset store sub-domain treats Lance blob-v2 plus ANN vector index (`IVF_PQ`/`IVF_HNSW_PQ`) plus BM25 full-text search as an indexed-retrieval concern, not a table-format concern; cramming `create_index`/`full_text_query`/`blob_field` as `LakeOp` cases pollutes the format-agnostic operation axis with Lance-only verbs every other arm must reject.
- Capability: an `AssetStore` owner over a `RetrievalOp` axis (vector-ANN, full-text, blob-fetch, hybrid) composing the Lakehouse Lance binding for storage and adding the retrieval axis Lance uniquely owns; blob-v2 columns (images, video, audio, embeddings) with multi-x blob reads and storage reduction.
- Shape: a new `data/tabular/multimodal.py` owner over a `RetrievalOp` axis composing the Lance storage binding, keyed by `ContentIdentity`.
- Unlocks: embedding-vector search over BIM/fabrication asset corpora; BM25 search over document-derived text columns; blob-streamed point-cloud/mesh-render payloads.
- Anchors: `pylance` `create_index`/`create_scalar_index`/`to_table(full_text_query=)`/`blob_field`/`take_blobs`/`write_dataset(data_storage_version="2.2")`, `tabular/lakehouse#LAKEHOUSE` Lance binding, and `ContentIdentity`.
- Tension: depends on the Lance maintenance task (`LAKEHOUSE_RESEARCH_SETTLE` flips `[LANCE_VERSION]` settled); the retrieval axis composes Lance storage, never re-mints the lakehouse owner.
- Ripple: `python:compute` `[DATA_STUDY_INPUT]` — `compute/experiments` reads the Lance vector index over embedding columns this `AssetStore` exposes, mirroring the `python:data/tabular/multimodal` study-input edge on both endpoints.

[COMMUNITY]-[QUEUED]: a graph community-detection/clustering capability axis fills a real gap — rustworkx 0.17 ships zero community detection and networkx Louvain is slow pure-Python; igraph's C-core Leiden/Louvain/Infomap is the canonical high-quality partition owner returning a node-to-community membership matching `GraphResult.partition`.
- Capability: a community-detection axis on the existing graph owner — a third `GraphBackend` row plus Leiden/Louvain/Infomap/LabelPropagation `GraphAlgorithm` cases folding to `GraphResult(partition=...)` via one `_run_ig` arm; modularity-scored partition discovery over the same `GraphPayload`, feeding tabular edge-list egress.
- Shape: a new capability axis on `graph.md#GRAPH` — backend row plus algorithm cases plus dispatch arm, not a new file.
- Unlocks: partition discovery over BIM/fabrication graph corpora; modularity-scored clustering; downstream cluster analysis through the existing edge-list egress.
- Anchors: `igraph` `community_leiden`/`community_multilevel`/`community_infomap`/`modularity`/`VertexClustering.membership`, `graph#GRAPH` `GraphResult(partition=...)`, and `Graph.from_networkx`/`Graph.DataFrame`.
- Tension: admits dist `igraph` (not deprecated python-igraph; import stays `import igraph`); backend recovery stays value-driven — community cases pin igraph by class, never a knob; igraph C-core wheel confirmed on cp315 by probe, gated below 3.15 only if it lags.

[COVERAGE]-[QUEUED]: a labelled-raster sub-domain unifies the bare-ndarray `CoverageResult`, the CF xarray field store, and the STAC cube under one georeferenced `DataArray` owner — makes raster a first-class labelled dimension, the convergence of geospatial -> gridded/field -> catalog.
- Capability: a `Coverage` owner over rioxarray plus odc-stac — rioxarray bridges the bare-ndarray `CoverageResult` to a CF `xarray.DataArray` and closes the raster-WRITE gap (COG output); `odc.stac.load` builds the lazy cube from discovered STAC items, replacing the type-incorrect `fold_virtual_cube` HDFParser-over-COG path.
- Shape: a new `[COVERAGE]` cluster on `geospatial.md` (and the catalog `ASSETS` arm), folding `RasterOp` dispatch on source shape into one polymorphic apply.
- Unlocks: raster as a labelled `DataArray` feeding the CF field store; COG write-back the `EgressFormat` lacks; correct COG cube load from STAC discovery.
- Anchors: `rioxarray` `open_rasterio`/`.rio.{reproject,clip,reproject_match,to_raster(driver="COG")}`, `odc-stac` `odc.stac.load(items, bands=, crs=, resolution=, patch_url=)`, `gridded/field` CF owner, and `geospatial#GEO` `RasterGeoClaim`.
- Tension: xarray banned module-level -> rio bodies bind `xr` function-local; rioxarray/odc-stac are cp315-clean pure-Python (no gate); replaces the `fold_virtual_cube` COG arm, virtualizarr stays scoped to HDF5/NetCDF.

[REUSE_WIRE]-[QUEUED]: the data-plane face of the cross-libs content-identity concert — every data producer stamps its output with the bit-identical `ContentKey` computed from the C#-owned XxHash128 seed and emits it on the existing runtime/transport wire, so a content key computed by any runtime is reusable by the others without re-derivation; a consumer concept, never a Python owner of derivation/versioning/cache.
- Capability: a binding consumer idea — data tabular/spatial/gridded producers fold the `ContentKey` from `evidence/identity#IDENTITY` and surface it on the existing seam so Rasm.Persistence federates the durable reuse ledger; the session-local elision is the runtime `LanePolicy.cached` consumer, not a data owner.
- Shape: a recorded consumer concept; one mirrored seam `tabular/* -> csharp:Rasm.Persistence` `[CONTENT_KEY]`; zero new data owner, zero durable Python store.
- Unlocks: true cross-language artifact reuse; reproducible study-input provenance chains; the data-plane consumption of the tri-language content-identity concert.
- Anchors: runtime `ContentIdentity` (the C#-owned XxHash128 seed), `evidence/identity#IDENTITY`, the existing runtime/transport wire, and the Rasm.Persistence Version/Query-Federation durable owner.
- Tension: stays a consumer — never a derivation DAG owner, never a durable Python cache (`lanes.md` forbids a durable lane cache; durable federation is C# Persistence); the `ContentKey` is reproduced from the one C#-owned seed, never re-minted.
- Ripple: `csharp:Rasm.Persistence` `[REUSE_WIRE]` — Persistence `Query/Federation` is the durable consumer that federates the reuse ledger over the C#-seed `ContentKey` the data producers stamp; XxHash128 is the one C#-owned identity both runtimes reproduce bit-identically.

[STREAM]-[QUEUED]: a distinct stream/ sub-domain owns changefeed-driven incremental and windowed pipeline execution.
- Capability: changefeed-driven incremental/windowed pipeline execution over the polars/daft streaming engines, owning window/watermark/state.
- Shape: a new `stream/incremental.py` owner consuming `tabular/columnar#MATERIALIZE` DerivedSnapshot deltas plus the `tabular/lakehouse` changefeed plus the `tabular/interop#INTEROP_STREAM` Arrow carrier; never re-mints CDC-materialization, the carrier, or versioning/cache.
- Unlocks: incremental/streaming materialization of derived frames without re-reading full snapshots, emitted on the existing runtime/transport wire.
- Anchors: `tabular/columnar#MATERIALIZE` DerivedSnapshot, `tabular/lakehouse` changefeed, `tabular/interop#INTEROP_STREAM`, polars streaming `collect(engine="streaming")`, daft streaming execution, and `python:runtime/transport`.
- Tension: the one-word page name is `stream/incremental.md`; any streaming-engine library admission beyond polars/daft is validated against current docs before settling.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[PROFILE]-[COMPLETE]: realized as the new `tabular/profile.py` `QualityProfile` owner over `pointblank.Validate`/`Thresholds`/`Actions` — the `ProbeStep` plan axis, the `Grade` severity sweep, the `ProfileReport` GT/wire axis emitting the `[SHAPE]` frame to artifacts, and the plan-content-keyed `ProfileReceipt`; the ARCHITECTURE codemap carries the profile node.

[COVENANT]-[COMPLETE]: realized as the `contract.md#COLLECTION` `FrameCovenant` cluster over `dataframely.Collection` — `RelationEdge`/`RelationCardinality`/`CovenantOp` (prove/consistent/restrict/extend/persist/sample), the Merkle-folded covenant `ContractClaim` composing columnar `PartitionBundle` and catalog item/asset member content-keys without re-minting them.

[LINEAGE]-[DROPPED]: a Python lineage/version.py plus provenance.py duplicates the Rasm.Persistence Version/Provenance owner (W3C-PROV causal DAG, tamper-evident ledger, op-log changefeed) that architecture law names the sole content-hash identity/federation owner per runtime consumed at the wire; the host-free slice survives as QUERY_PLAN_PROVENANCE and icechunk `set_virtual_ref` relocates to gridded/virtual.md.

[CACHE]-[DROPPED]: an obstore-backed CacheStore keyed by ContentKey is a second durable content-addressed store, the exact concern C# Rasm.Persistence owns, and consumes a reuse-fabric elision PORT that does not exist (lanes.md fixes the cache as a session-local in-memory `Map[ContentKey,T]` and lists a durable lane cache among the deleted forms); re-aimed as REUSE_WIRE, which consumes the C#-seed ContentKey with no durable Python store.

[DERIVATION]-[DROPPED]: defined as STREAM plus LINEAGE plus CACHE, the concept collapses to provenance narration once LINEAGE/CACHE are cut and STREAM is folded; its genuine value (a content key computed by any runtime is reusable by the others) is re-aimed to REUSE_WIRE as a thin consumer idea, not a fourth owner.
