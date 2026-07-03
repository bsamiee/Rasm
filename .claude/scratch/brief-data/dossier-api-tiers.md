# DOSSIER — API-TIERS lane — libs/python/data

Scope: BOTH catalog tiers judged mined-vs-unmined against the 16 owning design pages.
Branch tier `libs/python/.api/` = 26 stubs (3572 LOC). Folder tier `libs/python/data/.api/` = 64 stubs (6599 LOC). Owning corpus = 16 pages (6384 LOC): tabular{columnar,query,lakehouse,contract,interop,profile,egress}, spatial{geospatial,catalog,mesh}, gridded{store,virtual,ragged,field}, graph, impact.
Stance result: the corpus is world-class, not naive. Catalogs are integration-shaped (STACK/INTEGRATION/SIBLING blocks, not member dumps); the tabular/gridded/spatial planes are deeply mined against confirmed members with `.api`-anchored [RESEARCH] tails. Defects are STRUCTURAL (tier placement, one hollow plane, dead admissions), not per-member.

---

## [A] TIER-PLACEMENT — the dominant structural ruling

The data roster is SPLIT across two tiers. `libs/python/.api/` (the "branch" substrate tier) holds these `owner: data` / `[PY_DATA_API_*]` catalogs that per the README's own law belong in `libs/python/data/.api/`:

| stub (in branch tier) | owner tag | header | data consumer | verdict |
|---|---|---|---|---|
| `daft.md` | `owner: data` (L10) | PY_DATA_API_DAFT | `query.md` `_stream`/`_daft_scan` | MISPLACED — clearest; explicit owner:data |
| `networkx.md` | `owner: data` (L12) | PY_DATA_API_NETWORKX | `graph.md` codec/egress lane | MISPLACED — split from `rustworkx`/`igraph` siblings in folder tier |
| `numcodecs.md` | `owner: data` (L10) | PY_DATA_API_NUMCODECS | `store.md` `_COMPRESSOR`/`_FILTER` | MISPLACED — split from `zarr` sibling; ALSO README-absent |
| `xarray.md` | `owner: data` (L12) | PY_DATA_API_XARRAY | `field.md` CF cube | MISPLACED (defensible if `compute` co-consumes labelled arrays) |
| `arro3-core.md` | none | PY_DATA_API_ARRO3_CORE | `deltalake.load_cdf`, `egress` list count, `interop`, `virtual` | MISPLACED (defensible — Arrow substrate shared with compute/geometry) |
| `adbc-driver-manager.md` | none | PY_DATA_API_ADBC_DRIVER_MANAGER | `query.md` `_adbc`/`dbapi` | MISPLACED (defensible — runtime `resilience` `_named` refs it by-name) |
| `zlib-ng.md` | `owner: data` (L10) | PY_DATA_API_ZLIB_NG | NONE (0 pages) | MISPLACED + DEAD — see [C1] |

Correct branch-substrate (shared, stay): `anyio`, `beartype`, `expression`, `msgspec`, `numpy`, `pydantic`, `xxhash`(owner:runtime). Runtime-folder catalogs parked in the branch tier (`pydantic-settings`/`stamina`/`universal-pathlib` owner:runtime; `trio`/`structlog`/`opentelemetry-*`/`grpcio`/`grpcio-tools`/`protobuf`/`psutil`) are the RUNTIME campaign's cleanup, out of data scope.
The clean split cases — `networkx` (graph triad split: `networkx` branch, `rustworkx`+`igraph` folder), `numcodecs` (codec pair split: `numcodecs` branch, `zarr` folder), `daft` — should relocate to `libs/python/data/.api/`. `xarray`/`arro3-core`/`adbc-driver-manager` are the genuinely-shared boundary cases; keep in branch ONLY if a second Python folder (compute) verifiably consumes them, else relocate. The README `[02]-[DOMAIN_PACKAGES]` states "API evidence lives in the adjacent `.api/` folder" (i.e. `data/.api/`) for domain packages yet lists `arro3-core`/`daft`(LAKEHOUSE)/`networkx`(GRAPH)/`xarray`(GRIDDED) — self-contradiction between the roster and the placement.

Folder-tier metadata gap: 8 folder-tier stubs lack the `- owner: data` line every other stub carries — `ibis-framework`, `meshio`, `nanoarrow`, `narwhals`, `pandas`, `pandera`, `polars`, `pyarrow` (the older-authored set). Cosmetic but inconsistent.

---

## [B] PER-STUB JUDGMENT (mined vs unmined vs deferred vs dead)

MINED-AT-DEPTH (catalog surface consumed by fences, verified against design-page [Packages]/[RESEARCH]):
- Tabular core: `narwhals` (interop `_BACKEND`/`schema_of`/`_eager`), `pyarrow` (universal), `polars` (columnar scan/`register_io_source`/`collect(engine=)`), `duckdb` (columnar `RemoteGlob`/`Window`/relational + query `_duckdb`), `sqlglot` (query `SqlGate` transpile/`edges`/`lineage`, columnar `predicate_count`), `ibis-framework` (query `Ir`/`to_sql`/`parse_sql`/`to_pyarrow_batches`/`BaseBackend.con`), `deltalake` (lakehouse + columnar `load_cdf`/`QueryBuilder`), `pyiceberg` (lakehouse `Transaction`/`scan`/`UpdateSchema`/`expire_snapshots`), `pylance` (lakehouse write/read/merge/index-build/optimize/vacuum), `dataframely` (contract COLLECTION — near-total: `Collection.{filter,validate,is_valid,join,collect_all,common_primary_key,sample,write_parquet,scan_parquet,serialize,matches}`/`concat_collection_members`/`FailureInfo.{invalid,details,counts,cooccurrence_counts}`/`Config`), `pandera` (contract QUALITY `Check.*` full), `pointblank` (profile — deepest single-package mine: `col_vals_*`/`col_{avg,sum,sd}_{op}`/`Thresholds`/`Actions`/9-case report axis), `obstore` (egress full write/read/list/sign/handle surface), `nanoarrow` (interop CARRIER `ArrowCStream.of`, ragged IPC), `arro3-core` (deltalake CDF reader, egress list-count), `fastexcel` (columnar `Excel`), `connectorx` (query `_connectorx`), `adbc-driver-flightsql` (query `_flightsql`/`Transport.db_kwargs`), `adbc-driver-manager` (query `_adbc`/`dbapi`), `polars-st` (geospatial GRID_DGGS), `obspec-utils` (field `ObjectStoreRegistry` via `obspec_utils.registry`).
- Gridded: `zarr` (store `create_array` slots + selection), `numcodecs` (store `_FILTER`/`_COMPRESSOR` numcodecs.zarr3 rows), `tensorstore` (store `_ts_*` KvStore/Transaction), `cubed` (store PLAN linalg/`from_zarr`/`Callback`), `awkward` (ragged — near-total: `RaggedSource`/`RaggedOp`/`_FOLD`/`ArrowCStream` bridge), `icechunk` (virtual `VersionOp`/`IceStorage`/`ManifestWrite`/8 storage factories), `virtualizarr` (field VIRTUAL 8-parser + `to_kerchunk`/`to_icechunk`, virtual `to_icechunk` accessor), `netcdf4` (field write `createVariable(zlib=/quantize_mode=/…)` + `date2num`), `h5py` (field `h5netcdf` + native `build_virtual_dataset`/`VirtualLayout` + `CFDtype` special-types), `flox` (field SELECT `xarray_reduce`/`groupby_scan`), `xarray` (field engine), `pandas` (foreign-file wire row only, by design — README `[DATAFRAME]` note).
- Spatial: `geopandas`/`shapely`/`pyproj`/`pyogrio`/`rasterio`/`geoarrow-rust-compute`/`h3ronpy` (geospatial), `pystac`/`pystac-client`/`planetary-computer`/`stac-geoparquet` (catalog), `odc-stac` (catalog ASSETS `odc.stac.load`).
- Mesh: `meshio`/`trimesh`/`rhino3dm` (mesh `_BACKEND`), `laspy`/`lazrs`/`laszip` (mesh POINTCLOUD).
- Graph: `rustworkx` (graph `_run_rx` — ~40 arms), `igraph` (graph `_run_ig` community), `networkx` (graph codec/egress lane + `_as_rx` bridge).

DEFERRED-BY-CARD (admitted + catalogued at depth but fences unmined; legitimate — the IDEAS/TASKLOG card names it):
- `rioxarray` (0 pages) → `CATALOG_COVERAGE_ODCSTAC`/`COVERAGE` QUEUED (the `[COVERAGE]` DataArray cluster).
- `xarray-spatial` (0 pages) → `GEOSPATIAL_TERRAIN` BLOCKED.
- `ducklake` (0 pages) → `LAKEHOUSE_DUCKLAKE_FORMAT` QUEUED (README: "planned admission").
- `datafusion` PACKAGE (0 pages; the `datafusion` grep hits are deltalake's embedded DataFusion, NOT the `SessionContext` package) → `SUBSTRAIT_PORTABILITY` QUEUED `QuerySpec.Federated`. 187-LOC catalog for a not-yet-demanded federation engine.
- `duckdb-substrait` — mined via DuckDB extension (`query.md` `con.install_extension("substrait")`/`get_substrait`), not by module import; not a defect.
- `pylance` ANN/FTS/blob retrieval surface (`create_index nearest=`/`full_text_query=`/`blob_field`/`take_blobs`/`MergeInsertBuilder` conditional clauses) → `MULTIMODAL` BLOCKED. Lakehouse mines only the index-BUILD (`Index` op); the QUERY side is card-deferred.

UNMINED-TAIL inside otherwise-mined stubs (advanced surface no page demands; mostly reasonable deferral, catalog anchors for future demand):
- `sqlglot`: `diff`/`Edit` (query-version change detection — the catalog's own SIBLING_STACK names it feeding "provenance/migration owners"; no page uses it), `build_scope`/`traverse_scope` scope analysis, the typed builder DSL (`select`/`column`/`func` programmatic construction), JSONPath.
- `deltalake`: `add_constraint`/`drop_constraint` (CHECK constraints), `add_feature`/`TableFeatures` (protocol upgrades), `create_checkpoint`/`compact_logs`/`cleanup_metadata`/`repair`, `deletion_vectors()`, `convert_to_deltalake`, `PostCommitHookProperties`.
- `pyiceberg`: partition-spec/transform authoring (`PartitionSpec`/`BucketTransform`/`YearTransform`… — lakehouse `Write` DISCARDS `_partition_by` for Iceberg), `ManageSnapshots` branch/tag (`create_branch`/`create_tag`; rollback noted out-of-`_PORTABLE`), `add_files`, `dynamic_partition_overwrite`, `to_daft`/`to_bodo`/`to_ray` egress, `StaticTable.from_metadata`, view DDL.
- `polars`: Polars Cloud `remote()`, `GPUEngine`, `Catalog` (Unity/Iceberg REST), `LazyFrame.serialize/deserialize` (plan wire), `sink_iceberg`/`sink_delta`.
- `datafusion` (even once federation lands): UDF factories `udf`/`udaf`/`udwf`, `functions` namespace.
- `dataframely`: `to_sqlalchemy_columns`/`to_pydantic_model`, `read_parquet_metadata_*`, Collection delta IO.
- `rustworkx`: VF2 (sub)graph isomorphism, maximum bisimulation, `generators` submodule, DOT/Matrix-Market IO, group centrality, edge/Misra-Gries/bipartite coloring, shell/spiral/bipartite layouts (→ `GRAPH_DEEPEN` QUEUED; but see [C4] on the stale figure).
- `obstore`: `obstore.auth.*` credential providers + `obstore.fsspec` bridge (runtime `roots`/`TransportResource` owns credentials — correct non-mine).

---

## [C] CROSS-CUTTING FINDINGS

[C1] DEAD ADMISSIONS (`owner: data`, catalogued, zero design-page consumers, README-roster-absent):
- `zlib-ng.md` (branch tier, `owner: data`, `rail: compression`): 0 data-page consumers (the `zlib` grep hits are `numcodecs.Zlib` in `store.md` `_COMPRESSOR` and the `zlib: bool` netcdf4 flag in `field.md` `FieldEncoding` — NOT the `zlib_ng` module). Its own charter positions it "beside `zstandard`/`lz4`/`brotli`" and "the artifacts-rail codecs own transport/container payloads" — i.e. a `libs/python/artifacts` compression concern mistagged `owner: data`. NOT in README `[02]`/`[03]`. Verdict: expel from the data roster (strike pyproject row + `.api` stub, or re-home to artifacts).
- `csvkit.md` (folder tier, `owner: data`, `rail: tabular-cli`, 94 LOC): 0 design-page consumers. Its 14-CLI subprocess boundary (`in2csv`/`csvsql`/`csvstat`/…) is entirely unwired; its own charter admits it is "the boundary complement to the in-process Arrow readers (`fastexcel`) and query engine (`duckdb`/`polars`)" — i.e. redundant with the deeply-mined CSV paths (`polars.scan_csv`, `duckdb.read_csv`, `pyarrow.csv`, `columnar.md` `DatasetKind.CSV`). NOT in README roster. Verdict: expel unless a concrete subprocess-CLI seam is authored (foreign DBF/fixed-width→CSV via `in2csv` is the only non-redundant capability; if kept, it needs a `columnar.md` arm demanding it).

[C2] THE HOLLOW PLANE — `impact.md` illusory capability (largest prose-vs-fence split in the folder):
- 3 of 5 `_normalize` arms are literal `...` stubs: `_from_openepd` (`impact.md:195`), `_from_score` (`:202`), `_from_olca` (`:209`). Only `_from_epdx` (`:173-188`) and `_lower` (`:212-226`) are realized fences.
- Yet the backing EPD/LCA catalogs are richly authored at full depth and grep-confirmed referenced ONLY in `impact.md` `[Packages]`/`[RESEARCH]` prose: `openepd.md` (123 LOC — `Impacts`/`ImpactSet`/`ScopeSet`/`OpenEpdApiClientSync`/`DefaultBundleReader`), `bw2calc.md` (80 LOC — `LCA`/`MultiLCA`/`MethodConfig`/staged `lci`→`lcia`→`score`), `bw2data`, `bw2io`, `bw-processing` (0 fence hits — named in `[Packages]` prose only), `brightway2`, `olca-ipc`, `premise`. Eight catalogs + a full `[03]-[RESEARCH]` fold-plan describing capability the fences do not realize.
- Result: the impact plane CLAIMS OpenEPD/EC3 ingest, Brightway solve, openLCA IPC, and premise prospective-background scoring, but only ILCD+EPD (`epdx`) crosses to a fence. `MaterialImpact.of` returns a rail whose openepd/brightway/openlca arms produce `...`. This is the folder's one genuinely-illusory owner.
- Secondary impact defect: `bw2calc.md` names transitive deps `scipy`/`matrix_utils`/`stats_arrays`/`bw_graph_tools`/`pandas`/`xarray` — `scipy` is the numeric-trio the ARCHITECTURE boundary reserves to `compute`; the companion-gated (`<3.15`) impact plane pulls it transitively, acceptable only under the stated companion band.

[C3] SUBSTRATE-CATALOG DUPLICATION / anchor drift:
- `obstore.md` exists in BOTH the data folder tier (full 135-LOC catalog, `owner: data`) and (per `egress.md:285` cross-ref) `runtime/.api/obstore.md` (runtime read-slice stub per RUNTIME-BRIEF V13). The dual-tier split is intended (runtime = read-slice, data = write owner), BUT `egress.md:285`/`:286` cite `runtime/.api/obstore.md` for the exceptions taxonomy when the data-tier `obstore.md:145-157` carries the same 12-leaf `BaseError` set — a cross-tier anchor that should point at the folder's own catalog.
- The RUNTIME-BRIEF V13 routes fsspec's `transaction`/`cat_ranges`/block-cache/`FSMap`/`rsync` surfaces "to the DATA folder," yet `data/.api/` has NO `fsspec.md` (it lives in the branch/runtime tier). `columnar.md` `RemoteGlob` DOES touch fsspec (`register_filesystem(dataset.ref.path.fs)` via `UPath.fs`) — so the data folder consumes an fsspec surface with no folder-tier catalog. Missing `data/.api/fsspec.md` (data read-slice: the `.fs` accessor + `register_filesystem`).

[C4] STALE CARD-vs-CATALOG version/coverage drift:
- `rustworkx.md` catalogs `0.18.0`; `graph.md`/`GRAPH_DEEPEN`/`GRAPH_COMMUNITY` prose says "rustworkx 0.17" and "taps ~11 of 40+ algorithms." The realized `graph.md` `_run_rx` already implements ~40 arms (traversal/shortest-path/all-pairs/DAG/connectivity/cut/6-centrality/structure/layout/community) — the "~11 of 40" figure is a pre-rebuild artifact; the genuine remaining unmined rustworkx surface is isomorphism/bisimulation/generators/DOT-IO/group-centrality/edge-coloring (per [B]), not "29 of 40." The card's coverage claim understates the realized page.

[C5] CATALOG-QUALITY POSITIVES (preserve — not defects):
- The catalogs are integration-shaped, not member-dumps: every one carries `STACK`/`INTEGRATION_STACKING`/`SIBLING_STACK` blocks wiring cross-package rails (e.g. `datafusion.md` `[INTEGRATION_STACKING]` substrait spine ↔ `duckdb-substrait`; `deltalake.md` `to_pyarrow_dataset` → `datafusion`/`duckdb` federation; `narwhals.md` `arrow-intake stack` → `nanoarrow`).
- `.api`-anchored [RESEARCH] discipline: design pages cite exact catalog rows (`pointblank.md [03]-ENTRYPOINTS [01]-[19]`, `pyiceberg.md`/`deltalake.md`/`pylance.md` member+arity confirmations), and correction records (`obstore` `SignCapableStore`/`HTTP_METHOD` stub-only `TypeAlias`; `numcodecs.zarr3` deprecation → `zarr.codecs.numcodecs`) that prevent phantom members.
- Single-mint FIT held: every content key rides runtime `ContentIdentity` over real bytes; no data catalog re-mints identity/version/lineage (correctly deferred to C# `Rasm.Persistence` at the wire per every page's Boundary).

---

## [D] VERDICT CANDIDATES (campaign-defining, evidence-first)

V-API-1 — SPLIT-TIER ROSTER. Five data-owned catalogs sit in the branch substrate tier: `daft`(owner:data L10), `networkx`(owner:data L12), `numcodecs`(owner:data L10), `xarray`(owner:data L12), `zlib-ng`(owner:data L10), plus header-tagged `arro3-core`/`adbc-driver-manager`. The graph triad (`networkx` branch / `rustworkx`+`igraph` folder) and codec pair (`numcodecs` branch / `zarr` folder) are split across tiers, contradicting README `[02]`'s "API evidence lives in the adjacent `.api/` folder." RULING: relocate the data-only ones (`daft`,`networkx`,`numcodecs`) to `data/.api/`; keep `xarray`/`arro3-core`/`adbc-driver-manager` in branch ONLY if `compute` verifiably co-consumes, else relocate; add `- owner: data` to the 8 folder-tier stubs missing it.

V-API-2 — THE IMPACT PLANE IS HOLLOW. `impact.md:195/202/209` (`_from_openepd`/`_from_score`/`_from_olca`) are `...` stubs; only `_from_epdx`/`_lower` are realized. Eight richly-authored EPD/LCA catalogs (`openepd`,`bw2calc`,`bw2data`,`bw2io`,`bw-processing`,`brightway2`,`olca-ipc`,`premise`) + a full `[03]-[RESEARCH]` fold-plan back capability no fence realizes. RULING: either realize the four normalize arms against the confirmed catalog members (the catalogs are ready) or demote the unrealized `ImpactSource` cases to a documented `planned`-phase gate; the current state claims OpenEPD/Brightway/openLCA/premise ingest the fences do not deliver.

V-API-3 — DEAD ADMISSIONS. `zlib-ng.md` (owner:data, 0 consumers, README-absent, self-charters as an artifacts-tier compression codec) and `csvkit.md` (owner:data, 0 consumers, README-absent, self-charters as redundant with the mined polars/duckdb/pyarrow CSV paths). RULING: expel both from the data roster (strike pyproject rows + `.api` stubs), or re-home `zlib-ng` to `artifacts` and author a concrete `columnar.md` `in2csv` foreign-decode arm if `csvkit` is retained.

V-API-4 — datafusion IS A 187-LOC CATALOG FOR AN UNDEMANDED ENGINE. `datafusion.md` (fully authored, `owner: data`) is consumed by zero fences; the only `datafusion` design-page hits are deltalake's EMBEDDED DataFusion (`QueryBuilder`), not the `SessionContext` federation package. It is card-deferred (`SUBSTRAIT_PORTABILITY` QUEUED `QuerySpec.Federated`). RULING: acceptable as a QUEUED-card pre-catalog IF the card stays the anchor; otherwise it is the single largest unmined admitted surface and should carry an explicit `[BLOCKED-ON-CARD]` marker so it is not read as live capability.

V-API-5 — SUBSTRATE ANCHOR/CATALOG GAPS. (a) `egress.md:285-286` cites `runtime/.api/obstore.md` for the exceptions taxonomy the folder's own `data/.api/obstore.md:145-157` already carries. (b) RUNTIME-BRIEF V13 routes fsspec's transaction/`cat_ranges`/`FSMap` surfaces "to the DATA folder," but `data/.api/` has no `fsspec.md` while `columnar.md` `RemoteGlob` consumes `UPath.fs`+`register_filesystem`. RULING: retarget the egress exceptions anchor to the folder tier; author `data/.api/fsspec.md` (data read-slice) to receive the V13-delegated surfaces.

V-API-6 — TIER SEMANTICS ARE UNDECLARED. `libs/python/.api/` mixes branch-substrate (`numpy`/`xxhash`/`expression`), runtime-folder (`stamina`/`universal-pathlib`/`pydantic-settings` owner:runtime; `trio`/`grpcio`/`otel`/`psutil`), AND data-folder (`daft`/`networkx`/`numcodecs`/`xarray`/`zlib-ng`) catalogs with no stated inclusion rule. The three-tier reality (`libs/python/.api/` branch, `runtime/.api/`, `data/.api/`) has no charter fixing what promotes a folder package to the branch tier. RULING: declare the branch-tier admission law ("shared by ≥2 Python folders OR pure numeric/typing/hashing substrate") in `libs/python/.planning/README.md`, then re-file every folder-owned catalog that fails it.

V-API-7 — CARD/CATALOG COVERAGE DRIFT (`GRAPH_DEEPEN`). Card claims "~11 of 40" rustworkx algorithms and "rustworkx 0.17"; catalog is `0.18.0` and `graph.md` `_run_rx` already implements ~40 arms. RULING: refresh the card to the realized surface (genuine remaining unmined = isomorphism/bisimulation/generators/DOT-IO/group-centrality/edge-coloring) so the coverage figure stops understating the page.

---

## [E] DOMAIN GAPS (roster-addition candidates — named, not researched)

No concern is left OWNERLESS — the admitted roster covers the folder's declared domains densely. The gaps are narrow:
- Foreign-tabular decode (DBF / fixed-width / GeoJSON→CSV): the only non-redundant `csvkit` capability; if `csvkit` is expelled, `pyogrio`/`fastexcel`/`duckdb` cover most, but fixed-width/DBF has no admitted in-process owner. Candidate: keep `in2csv` narrowly OR admit a lightweight DBF/fixed-width reader.
- COG raster WRITE-back: `EgressFormat` lacks COG output; the `COVERAGE`/`CATALOG_COVERAGE_ODCSTAC` cards already name `rioxarray.rio.to_raster(driver="COG")` as the fill — admitted, catalogued, card-deferred (not a true gap).
- Native GeoArrow compute sharing the C# GLB wire: `geoarrow-rust-compute` is admitted+catalogued but `GEOSPATIAL_INGRESS_DEEPEN` (QUEUED) notes the `to_parquet` byte-roundtrip still stands in for the native buffer path — a mining gap, not a roster gap.
- fsspec data read-slice catalog (see V-API-5b) — a missing catalog, not a missing package.
- No gap in: dataframe/arrow/lakehouse/query/contract/graph/gridded/spatial/mesh — each concern has a mined owner.

---

## [F] UPSTREAM (RUNTIME-BRIEF) — consumer pressures + opportunities (NO brief edit made)

The RUNTIME-BRIEF already anticipates data as consumer and explicitly defers data-tier admission to this campaign ("write/conditional/PutMode surfaces remain the data folder's to mine"; "leave data-tier admission to the data campaign"). No genuine LACK verifiable from data's vantage warranted a surgical edit. Recorded pressures:
- MIGRATION (dominant): RUNTIME-BRIEF V4/V12 renames `rasm.runtime.content_identity` → `rasm.runtime.identity`. ALL 16 data pages import `from rasm.runtime.content_identity import ContentIdentity, ContentKey` — the exact drift spelling the brief deletes. The data campaign must rewire every page's identity import when runtime lands.
- MIGRATION: data consumes `RetryClass.OBJECT_STORE` (egress/roots), `RetryClass.HTTP` (catalog/mesh/geospatial/pointcloud), `RetryClass.REMOTE_DB`+`RetryClass.STREAMING` (query), `RetryClass.LAKE_COMMIT` (lakehouse) and the `guarded`/`guarded_sync` fused envelopes (`AsyncRetryingCaller`/`RetryingCaller`). RUNTIME-BRIEF V5 enumerates only "occt/rpc/lake-commit/remote-db/streaming" (partial, "258+"); OBJECT_STORE/HTTP are consumed by runtime `roots` itself so likely present — VERIFY these rows survive the resilience rebuild, and that `guarded_sync` remains an export (egress/lakehouse both bind it).
- MIGRATION: data binds `rasm.runtime.faults.{FAULT_CONF, RuntimeRail, boundary, async_boundary, traversed, Disposition, BoundaryFault, railed}` universally. The brief provisions FAULT_CONF (`faults.md:126,131` shared BeartypeConf), traversed (V8), boundary/async_boundary — but `railed` (used `virtual.md`, the `effect.result` yield-from builder) is not called out in the brief; confirm it survives as a faults export.
- OPPORTUNITY: data hand-rolls no transport/resilience/identity/receipt — it composes runtime `TransportResource`/`ResourceRef`/`ContentIdentity`/`ReceiptContributor` at every seam. The RUNTIME `shapes.md` (NEW, V2) wire vocabulary + count-prefix content-key law is the anchor `impact.md`'s `Discipline.Environmental` seam and every content-keyed egress build against — already consumed correctly as a boundary contract, never re-planned.
