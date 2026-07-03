# DOSSIER — CENSUS — libs/python/data/.planning

Lane: census (read-only survey). Corpus: 16 design pages, 6384 code LOC (loc). Governing docs read: `data/ARCHITECTURE.md`, `README.md`, `IDEAS.md`, `TASKLOG.md`, `libs/.planning/architecture.md`, `libs/python/.planning/ARCHITECTURE.md`, upstream `RASM-RUNTIME-BRIEF.md` (FULL), `RASM-COMPONENT-PARADIGM-DECISION.md`/`RASM-GENERATION-SPEC.md` (wire-law context), runtime `.planning` (seam verification). Every intra-data import + every runtime/csharp seam swept.

## [00]-[HEADLINE]

The `data` corpus is page-level excellent and graph-level fragile, with one genuinely unbuilt page. 13 of 16 pages are 8-10/10 dense, doctrine-fluent, deep-mined owners; the runtime FIT is otherwise clean (all data→runtime seams verified valid). Four structural defects define the campaign: (1) a **tabular import SCC** — `columnar⇄lakehouse` + `columnar⇄query` — caused by the `MATERIALIZE`/`DerivedSnapshot` composing-concern being mis-homed in `columnar`, the base every sibling imports; (2) a **`contract⇄interop` cycle** whose own `contract` page proves the authors know (ADMISSION `TYPE_CHECKING`-guards it) while COLLECTION breaks the guard eagerly, resolvable by re-homing `FieldShape`; (3) a **`catalog⇄geospatial` cycle** broken only by a lazy import, from cross-composed STAC/raster ownership; (4) **`impact.md` is a stub** — 3 of 5 `_normalize` arms are literal `...`, untracked by TASKLOG, illusory against its prose. The dominant upstream migration is the RUNTIME brief's `content_identity→identity` rename (V4/V7) hitting 15 data pages / 22 import sites. Governance surfaces are blind to the intra-data import topology (the SCC never appears in the seam ledger) and carry a 2-site `rasm.data.catalog` module-path drift against the codemap.

## [01]-[PER-PAGE VERDICTS]

### tabular/columnar.md — 7/10 (425 LOC; total 522 lines)
- **DEFECT (structural, campaign-defining):** `[04]-[MATERIALIZE]`/`DerivedSnapshot` composes `Lakehouse`+`QueryEngine` yet lives inside `columnar` — the LOWEST tabular module (it defines `DatasetKind`/`DatasetRef`/`QueryReceipt`/`predicate_count`/`ScanPlan`/`ColumnarEgress` that lakehouse+query import). Its module-top prelude imports `from rasm.data.tabular.lakehouse import Lakehouse, TableFormat` (columnar.md:429) and `from rasm.data.tabular.query import QueryEngine, QuerySpec` (columnar.md:430) — closing both cycles.
- Excellent otherwise: `ScanPlan` 8-case tagged union, the exported `predicate_count`/`_PREDICATE_NODES` fold shared with query (the correct single-owner pattern), `QueryReceipt.railed` content-key discipline, the Corpus/Excel wire arms.
- **Owner charter as it SHOULD be:** `columnar` = pure dataset-ref + scan + typed-egress + query-receipt BASE (no MATERIALIZE). Extract `DerivedSnapshot`/`PartitionBundle` to a new `tabular/materialize.py` (composes lakehouse+query+columnar downward) OR fold into `query`. Then columnar imports nothing intra-data; lakehouse/query/materialize import columnar cleanly → DAG.

### tabular/lakehouse.md — 8/10 (366 LOC)
- STRONG. `LakeOp` (11 cases) × `TableFormat` (Delta/Iceberg/Lance) dispatch, `_PORTABLE` reject-as-data table, `_receipt`/`_snapshot` single-polymorphic, `guarded_sync(RetryClass.LAKE_COMMIT)` delegation. `[03]-[RESEARCH]` catalogue-settled to the member level.
- Half the `columnar⇄lakehouse` cycle: `from rasm.data.tabular.columnar import DatasetKind, DatasetRef` (lakehouse.md:43, module-top) — this direction is LEGITIMATE (lakehouse needs the dataset owner); the cycle is columnar's fault (see above).

### tabular/query.md — 8/10 (452 LOC)
- STRONG. `QuerySpec` 6-case axis, `Transport` option-projection, `SqlGate`/`IrEmit` sqlglot/ibis plane, `_provenance` lineage fold, `guarded(RetryClass.REMOTE_DB/STREAMING)` delegation. Only page NOT importing `content_identity` directly (gets it via `columnar.QueryReceipt`).
- Half the `columnar⇄query` cycle: `from rasm.data.tabular.columnar import QueryReceipt, predicate_count` (query.md:42, module-top) — LEGITIMATE (shared receipt/fold).
- **GAP:** `datafusion` is admitted (README `[QUERY]`, pyproject) but query.md imports/uses NONE of it; `QuerySpec.Federated` unbuilt. TASKLOG `SUBSTRAIT_PORTABILITY` correctly names the datafusion leg as remaining open — but the package is admitted-unconsumed until it lands.

### tabular/contract.md — 7/10 (375 LOC)
- **DEFECT (cycle + inconsistency):** `[03]-[ADMISSION]` guards interop under `if TYPE_CHECKING: from rasm.data.tabular.interop import Backend, FrameInterop` (contract.md:181-182) + a function-local `from ...interop import Backend  # noqa: PLC0415` (contract.md:225), EXPLICITLY "since interop imports FieldShape back from this page and a module-top import would close the cycle." Yet `[04]-[COLLECTION]` imports the SAME symbols EAGERLY at module-top: `from rasm.data.tabular.interop import Backend, FrameInterop` (contract.md:262). One module, one guarded edge + one unguarded — a latent import-order-fragile cycle.
- **Root:** `FieldShape` (a pure structural value object interop MINTS in `_shapes`, interop.md:162) is mis-homed in contract `[03]`. Re-home `FieldShape` → interop; contract→interop becomes one-way; the ADMISSION `TYPE_CHECKING` guard becomes unnecessary.
- Otherwise excellent: `CheckKind` over 4 `Map` tables, `FrameCovenant`/dataframely `CovenantOp`, Merkle-keyed `ContractClaim`.

### tabular/interop.md — 8/10 (148 LOC)
- STRONG, dense. `Backend`×`_BACKEND` table (5 rows: POLARS/PANDAS/PYARROW/MODIN/DUCKDB — note TASKLOG `INTEROP_STREAM` calls it "3-row", now STALE), `ArrowCStream.of` carrier, null-mask-accurate `schema_of`.
- Back-edge of the cycle: `from rasm.data.tabular.contract import FieldShape` (interop.md:31, module-top). interop is the LOWER owner; FieldShape should live here.
- **Owner charter as it SHOULD be:** interop OWNS `FieldShape` (it constructs it); contract composes it.

### tabular/profile.md — 8/10 (295 LOC)
- STRONG, self-contained-ish. `pointblank` grade sweep, `ProbeStep` plan axis, `QualityProfile` GT-frame emit. `from rasm.data.tabular.contract import FieldShape` (profile.md:41) — one-way, clean.

### tabular/egress.md — 9/10 (238 LOC)
- MODEL page. `StoreOp` (12 cases) over one `_ROUTE`/`_Row` sync+async table, `guarded`/`guarded_sync(RetryClass.OBJECT_STORE)` delegation with the doubled-span/doubled-lift deleted-form law, `obstore.exceptions` boundary-catch, no-path/operation-bytes content-key discipline. Self-contained (runtime only).

### spatial/geospatial.md — 7/10 (890 LOC — LARGEST)
- STRONG but the biggest surface: 3 axes (`[02]-GEO` VectorGeoClaim/RasterGeoClaim/EgressFormat, `[03]-SPATIAL` DuckDB engine, `[04]-GRID` GridSystem) + `StacIngest`. `RasterOp` 14 cases, `_CONSTRUCT` table, `apply_remote` guarded envelope.
- Back-edge of `catalog⇄geospatial`: `from rasm.data.catalog import TableSink, TableSource, stac_table, stac_table_direct, stac_table_rehydrate  # noqa: PLC0415` (geospatial.md:578, LAZY — this is what breaks the cycle). Carries the `rasm.data.catalog` PATH DRIFT.
- Module-top: `from rasm.data.tabular.columnar import QueryReceipt` (geospatial.md:30, one-way).
- Size + tri-axis breadth is a split-pressure signal, but the axes share the CRS/claim/receipt spine — a split would fracture the claim owner; keep whole.

### spatial/catalog.md — 7/10 (607 LOC)
- STRONG. `StacQuery` union onto one `Client.search`, `Surface`/`SurfaceRow` routing, `Signing`/`SchemeRow`, `[03]-TABLE` stac-geoparquet encoder, `[04]-ASSETS` `AssetFold`.
- **DEFECT (cycle, eager):** `[04]-ASSETS` module-top `from rasm.data.spatial.geospatial import RasterGeoClaim, Resampling` (catalog.md:478) — the eager half of `catalog⇄geospatial`.
- **DEFECT (path drift):** `[04]-ASSETS` self-imports `from rasm.data.catalog import Signing, StacDiscovery, Surface` (catalog.md:475) — WRONG module path (`rasm.data.catalog` vs codemap `spatial/catalog.py` = `rasm.data.spatial.catalog`), while the same prelude correctly nests `rasm.data.gridded.field`/`rasm.data.spatial.geospatial`/`rasm.data.tabular.egress`.
- ASSETS composes 5 siblings (egress, gridded/field, gridded/virtual, spatial/geospatial, self) — heavy but each a settled seam.

### spatial/mesh.md — 8/10 (395 LOC)
- STRONG. `MeshBackend`×`_BACKEND` (meshio/trimesh/rhino3dm), `PointCloud` laspy/COPC, per-op `_TRACER` span, `guarded(RetryClass.HTTP)` remote-COPC. `from rasm.data.tabular.columnar import QueryReceipt` (mesh.md:35, one-way).
- Page-craft artifact: `[03]-POINTCLOUD` prelude self-imports `from rasm.data.spatial.mesh import _column` (mesh.md:332) — a same-module self-import (POINTCLOUD is `rasm.data.spatial.mesh`), harmless in assembled code, technically a self-reference. Signals POINTCLOUD could be its own module but the codemap deliberately co-locates it.

### gridded/store.md — 9/10 (525 LOC)
- MODEL page. `TensorBackend` (zarr/tensorstore) delegate axis, `TensorCodec`/`Serializer`/`_COMPRESSOR`/`_FILTER` tables, `_KVSTORE_DRIVER`, `write_region` `T|Iterable[T]` arity collapse, cubed `[03]-PLAN`/`PlanOp`/`MemoryProbe`. Self-contained (runtime only; imports `OBJECT_STORE_SCHEMES`).

### gridded/virtual.md — 9/10 (251 LOC)
- STRONG. icechunk `VirtualReference`/`VersionOp`/`IceStorage`/`ManifestWrite`, `@railed` commit fold, `ConflictSolver` auto-rebase. **COMPOSES** `gridded/field#VIRTUAL`: `from rasm.data.gridded.field import FieldVirtual, ManifestExport` (virtual.md:28, module-top, one-way) — a clean layering (field=virtualizarr manifest, virtual=icechunk store+version), NOT duplication.
- Naming smell: `ManifestExport` (field) vs `ManifestWrite` (virtual) — two near-identically-named manifest concepts on adjacent owners. Minor.
- Only page importing `railed` from faults (verified real: `faults.md:208 railed = effect.result[...]()`).

### gridded/ragged.md — 9/10 (303 LOC)
- MODEL page. `RaggedOp`/`FoldPolicy`/`_FOLD`/`_AXIS_OP` extreme collapse, `RaggedSource`/`RaggedSink` unions. `from rasm.data.tabular.interop import ArrowCStream` (ragged.md:28, module-top, one-way — interop does NOT import ragged, no cycle).

### gridded/field.md — 8/10 (611 LOC; total 745 lines)
- STRONG, base-of-gridded. 4 sections (FIELD/SELECT/VIRTUAL/EGRESS). `FieldEngine` delegate axis, `ReductionPolicy`/flox lowering, `FieldVirtual`/`VirtualParser`/`CFDtype`. Imports runtime only (base). Depended-on by virtual + catalog.
- Size + 4-concern breadth is a split-pressure signal but the sections share the CF-field spine.

### graph/graph.md — 9/10 (425 LOC)
- MODEL page. `GraphAlgorithm` (~42 algos, family-folded) → single `_run_rx` kernel via `_as_rx` coercion, `RX_CENTRALITY`/`IG_COMMUNITY` tables, `GraphResult.frame` node-keyed Arrow producer, `_run_ig` GPL-confined community. Self-contained (runtime only). Produces the `graph→columnar` pa.Table seam (no import — data-shape hand-off).
- **TASKLOG DRIFT:** fully realizes `[GRAPH_DEEPEN]` (full rustworkx suite) + `[GRAPH_COMMUNITY]` (igraph leiden/louvain/infomap) — both still marked `[QUEUED]` in TASKLOG. Done-but-open.

### impact/impact.md — 4/10 (205 LOC — WEAKEST, illusory-depth epicenter)
- **DEFECT (unbuilt):** 3 of 5 `_normalize` arms are literal `...` stubs: `_from_openepd` body `...` (impact.md:195), `_from_score` body `...` (impact.md:202), `_from_olca` body `...` (impact.md:209). Only `_from_epdx` (:173) + `_lower` egress (:212) are implemented.
- **Prose-vs-fence gap:** the header, `[02]-IMPACT` owner charter, and ARCHITECTURE codemap claim full 5-provider (openepd/ilcd_epd/brightway/openlca/premise) EN 15804 normalization; the fence delivers 1 provider + egress. `[03]-RESEARCH` mines the catalogs but the mining never reached the fence bodies.
- **Untracked:** NO TASKLOG card exists for impact (the 11 open tasks cover tabular/spatial/gridded/graph — impact is invisible to the work ledger).
- **Owner charter as it SHOULD be:** the charter is correct (one `MaterialImpact` normalizer, `ImpactSource` axis, `_normalize` fold); the page needs the 3 stub arms built + a TASKLOG card. Verdict is 4 for realization, not design.

## [02]-[CROSS-CUTTING]

### A. PAGE-LEVEL IMPORT CYCLES (4, one SCC + 3 two-cycles)
1. **columnar ⇄ lakehouse** — columnar.md:429 (MATERIALIZE prelude, module-top) → lakehouse; lakehouse.md:43 (module-top) → columnar. REAL.
2. **columnar ⇄ query** — columnar.md:430 (MATERIALIZE prelude, module-top) → query; query.md:42 (module-top) → columnar. REAL. {columnar, lakehouse, query} form one SCC via the shared `columnar` node.
   - Root cause of 1+2: `DerivedSnapshot`/`MATERIALIZE` (a HIGHER concern composing lakehouse+query) is mis-homed in `columnar` (the base). Fix = extract MATERIALIZE upward.
3. **contract ⇄ interop** — contract.md:262 (COLLECTION prelude, EAGER module-top) → interop; interop.md:31 (module-top) → contract. contract.md:181-182 (ADMISSION) `TYPE_CHECKING`-guards the SAME edge, self-contradicting within one module. Root = `FieldShape` mis-homed in contract.
4. **catalog ⇄ geospatial** — catalog.md:478 (ASSETS prelude, EAGER module-top) → geospatial; geospatial.md:578 (`# noqa: PLC0415`, LAZY) → catalog. Broken ONLY by geospatial's lazy import. Root = STAC-table/raster-claim ownership cross-composed across the two pages.

### B. MODULE-PATH DRIFT (governance/codemap-truth)
`rasm.data.catalog` (missing `.spatial.`) at exactly 2 sites: catalog.md:475 (self-import), geospatial.md:578. Codemap (`ARCHITECTURE.md:19`) maps `spatial/catalog.py` → `rasm.data.spatial.catalog`. Every OTHER intra-data import nests correctly (`rasm.data.tabular.*`, `rasm.data.gridded.*`, `rasm.data.spatial.geospatial`, `rasm.data.spatial.mesh`). This is the data analog of the RUNTIME brief's `content_identity` codemap drift.

### C. RUNTIME-SEAM FIT (verified against current runtime `.planning`)
ALL data→runtime seams VALID NOW except the module NAME:
- VALID + STABLE across the runtime rebuild: `FAULT_CONF` (faults.md:134), `boundary`/`async_boundary`/`traversed`/`trapped` (faults.md:160/164/188/171), `railed` (faults.md:208), `BoundaryFault`/`Disposition` (faults), `RetryClass` + members `OBJECT_STORE`/`HTTP`/`WIRE`/`SECRET`/`OCCT`/`RPC`/`LAKE_COMMIT`/`REMOTE_DB`/`STREAMING` (resilience.md:67-78), `guarded`/`guarded_sync` (resilience.md:212/221), `ResourceRef`/`ResourceRoot`/`TransportResource`/`OBJECT_STORE_SCHEMES` (roots.md:117/159/208/97), `Receipt`/`ReceiptContributor` (receipts), `ContentIdentity`/`ContentKey` symbols. Data hand-rolls NONE of these — it is already a MAXIMAL, clean runtime consumer (retry via guarded/guarded_sync, keying via ContentIdentity.of, faults via boundary/traversed, transport via ResourceRef/TransportResource). No "data reinvents runtime" opportunity exists.
- MIGRATION PRESSURE (single, dominant): `rasm.runtime.content_identity` → `rasm.runtime.identity`. The RUNTIME brief V4/V7 renames the module (the current runtime fences THEMSELVES carry the drift — identity.md:226 self-import, lanes.md:49 — the exact evidence the brief cites). Data blast radius: **15 pages, 22 import sites** (all pages except query.md, which gets it transitively via `columnar.QueryReceipt`). Per-page site counts: catalog 3, field 3, contract 2, columnar 2, mesh 2, {profile, lakehouse, interop, egress, geospatial, impact, virtual, store, ragged, graph} 1 each.

### D. DEAD / STUB CARRIERS
- impact `_from_openepd`/`_from_score`/`_from_olca` bodies = `...` (impact.md:195/202/209). 3 dead dispatch arms behind a complete `_normalize` match.

### E. SEAM-LEDGER DIFF vs ARCH [02]-[SEAMS] (both directions)
- **DECLARED-UNWIRED:** ARCH:43 `tabular ← python:artifacts/figures [WIRE]: color palette arrays / appearance correlates` — NO data fence consumes an artifacts/figures color palette. columnar's `Corpus` arm consumes artifacts/**documents** (`to_corpus_record`, ARCH:42, WIRED) — the figures/palette inbound seam has no data-side owner. Candidate phantom on the data ledger (or a purely-inbound hand-off that needs no data owner — decide + strike or wire).
- **WIRED-UNDECLARED:** the intra-data IMPORT topology is absent from the ledger EXCEPT one row (`tabular/columnar ← graph/graph`, ARCH:41, a pa.Table data-shape hand-off). The ~15 real import edges — the 4 cycles + virtual→field, ragged→interop, mesh→columnar, catalog→{field,virtual,egress}, geospatial→columnar, profile→contract, lakehouse→columnar, query→columnar — never surface, so the SCC is invisible to governance. Policy nuance: the ledger's intent appears to be cross-`libs/` seams + notable cross-sub-domain data-shape hand-offs (graph→columnar, impact→contract ARCH:58, impact→profile ARCH:59, profile→figures ARCH:52, mesh→geometry ARCH:46/47); under that policy import edges are legitimately omitted, but then the ledger gives zero visibility into the cyclic coupling — a governance blind spot the campaign should close (either declare intra-data seams or add a cycle-audit note).

### F. CSHARP SEAMS the pages carry (7 — FIT verdict: CLEAN, boundary-only)
1. `tabular/columnar → csharp:Rasm.Compute/Runtime` [SHAPE] DOE dataset / labelled-array study input (ARCH:37).
2. `tabular/* → csharp:Rasm.Persistence` [CONTENT_KEY] C#-seed `ContentKey` stamped on outputs, federated as durable reuse ledger (ARCH:49) — the `[REUSE_WIRE]` idea; every producer reproduces the C#-owned `XxHash128` seed via runtime `ContentIdentity`, never re-mints.
3. `tabular/query ⇄ csharp:Rasm.Persistence/Query/federation` [WIRE] Substrait binary plan + ibis `to_sql` portable SQL (ARCH:50) — query `PlanWire.SUBSTRAIT`/`IrEmit`; TASKLOG `SUBSTRAIT_PORTABILITY` ripples `Rasm.Persistence [SUBSTRAIT_FEDERATION_SEAM]`.
4. `gridded/virtual → csharp:Rasm.Persistence` [CONTENT_KEY] icechunk as-of snapshot identity reproduced from the `XxHash128` seed (ARCH:53) — virtual `Version/Snapshots` wire; `[PERSISTENCE_VERSION_WIRE]` names `ICECHUNK_ASOF_CONTENT_KEY`.
5. `spatial/geospatial → csharp:Rasm.Compute` [SHAPE] native GeoArrow buffers sharing the GLB wire layout (ARCH:54) — TASKLOG `GEOSPATIAL_INGRESS_DEEPEN` ripples `Rasm.Compute [DATA_GEOARROW_GLB]`.
6. `impact → csharp:Rasm.Materials` [ASSESSMENT] EN 15804 as `Discipline.Environmental` `Assessment` / `MaterialPropertySet.Environmental`, content-keyed; `Rasm.Compute` assessment-runner, `Rasm.Materials` projection (ARCH:60).
7. `impact ⇄ csharp:Rasm.Persistence` [CONTENT_KEY] EPD/LCA identity deduped in the durable reuse ledger (ARCH:61).
- **Fit assessment:** all 7 are content-keyed wire hand-offs (pa.Table / GeoArrow buffers / Substrait blob / icechunk snapshot id), decoded at the C# boundary, reproducing the single C#-owned `XxHash128` seed via runtime `ContentIdentity`, re-minting nothing. Aligns with `libs/.planning/architecture.md [07]-[CROSS_LANGUAGE_WIRE]` (one owner per shared concept, consumed at the boundary) and the `RASM-COMPONENT-PARADIGM-DECISION.md [AMENDMENTS]` count-prefixed canonical-bytes law (data carries the ContentKey obligation as a downstream CONSUMER, never authors the seam vocabulary). No coupling to any C# interior. This is the folder's strongest dimension.

### G. UPSTREAM CAPABILITY-CONSUMPTION / WATERFALL
- Data consumes the runtime surface deeply and correctly; NO capability the RUNTIME brief adds (recipe.md, transport/shapes.md, latched, drain-vocabulary re-home) is something data needs.
- **No waterfall EDITS to the runtime brief are required:** every data→runtime demand (guarded_sync sync-retry envelope, the 5 sibling-domain `RetryClass` rows, `ContentIdentity.of(fmt, source)`, `traversed`/`Disposition`, `ResourceRef`/`TransportResource`/`OBJECT_STORE_SCHEMES`) is satisfied by the runtime brief's ruled surface. The sibling POLICY rows data relies on are explicitly ruled to STAY (brief V5).
- The `content_identity→identity` rename is ALREADY ruled by RUNTIME V4/V7; data is the demanding downstream that MAKES the rename load-bearing at scale (15 pages) — a migration the DATA campaign absorbs, not a new capability. (If any edit were wanted it would be a one-line consumer note in RUNTIME V4 naming `data` as a 15-page downstream consumer of the rename; but the rename is already ruled, so it is recorded here as migration pressure, not an owner extension. Census lane makes no edits.)

### H. HARDCODING vs GENERATOR / OTHER
- Package-vs-fence gap: `datafusion` admitted (README `[QUERY]`, pyproject `DOMAIN_PACKAGES`) but zero query.md consumers (Federated leg unbuilt). Mild illusory-capability parallel to the runtime brief's recipe-rail finding.
- TASKLOG staleness: `[GRAPH_DEEPEN]`/`[GRAPH_COMMUNITY]` `[QUEUED]` but fully realized in graph.md; `[INTEROP_STREAM]` describes a "3-row Backend cap" now grown to 5 rows.
- No hardcoded-where-generator-belongs defects found in the fences read (the table-driven discipline is corpus-wide and strong: `_ROUTE`/`_BACKEND`/`_CMP`/`_FOLD`/`_AXIS_OP`/`_COMPRESSOR`/`_FILTER`/`RX_CENTRALITY`/`IG_COMMUNITY`/`_STORAGE`/`_SCHEME`/`_SURFACE` are all `Final[frozendict]`/`Map` rows).

## [03]-[VERDICT CANDIDATES] (campaign-defining structural rulings, evidence-first)

1. **Break the tabular SCC by re-homing MATERIALIZE out of columnar.** `DerivedSnapshot`/`PartitionBundle` (composes `Lakehouse`+`QueryEngine`) sits in the base `columnar` module, forcing `columnar⇄lakehouse` (columnar.md:429↔lakehouse.md:43) + `columnar⇄query` (columnar.md:430↔query.md:42). Extract to `tabular/materialize.py` (or fold into query); columnar becomes a pure base → the tabular graph is a DAG.
2. **Re-home `FieldShape` from contract to interop to kill the `contract⇄interop` cycle.** interop MINTS FieldShape (interop.md:162) yet imports it from contract (interop.md:31); contract COLLECTION eagerly imports interop (contract.md:262) while contract ADMISSION `TYPE_CHECKING`-guards the same edge "to avoid closing the cycle" (contract.md:181-182). FieldShape is a pure structural value object belonging to the lower owner; the move makes contract→interop one-way.
3. **Consolidate STAC-interchange/raster-claim ownership to break `catalog⇄geospatial`.** catalog ASSETS eagerly imports geospatial.RasterGeoClaim (catalog.md:478); geospatial lazily imports catalog.stac_table (geospatial.md:578, noqa) purely to dodge the cycle. The STAC-table encoder (catalog `[03]-TABLE`), its StacIngest wrapper (geospatial), and the AssetFold coverage arm that builds RasterGeoClaim (catalog `[04]-ASSETS`) are cross-composed. Move AssetFold coverage to geospatial (where RasterGeoClaim lives) OR re-home RasterGeoClaim to a shared base, so the dependency is one-way.
4. **impact.md is unbuilt and untracked — realize the 3 stub arms + admit a TASKLOG card.** `_from_openepd`/`_from_score`/`_from_olca` bodies are literal `...` (impact.md:195/202/209); prose + codemap claim full 5-provider EN 15804 normalization while the fence delivers `_from_epdx` + `_lower` only. The `[03]-RESEARCH` catalog mining exists but never reached the bodies. Lowest verdict (4/10) in the folder; the folder's illusory-depth epicenter.
5. **Migrate `content_identity→identity` across 15 data pages (22 sites) — the dominant RUNTIME-brief waterfall.** RUNTIME V4/V7 renames the module; every data page except query.md imports the old name. Verified: all OTHER data→runtime seams (FAULT_CONF, boundary/async_boundary/traversed/railed, RetryClass+members, guarded/guarded_sync, ResourceRef/ResourceRoot/TransportResource/OBJECT_STORE_SCHEMES, Receipt/ReceiptContributor, ContentIdentity/ContentKey) survive the rebuild unchanged — the FIT is otherwise clean, so this rename is the ONLY structural migration the runtime campaign imposes.
6. **Fix the `rasm.data.catalog` module-path drift (2 sites) against the codemap.** catalog.md:475 (self-import) + geospatial.md:578 drop `.spatial.`; the codemap maps `spatial/catalog.py` → `rasm.data.spatial.catalog`, and every other spatial import nests correctly. A concrete governance/codemap-truth defect.
7. **Give the seam ledger visibility into the intra-data import graph (and reconcile ARCH:43).** ARCH [02]-[SEAMS] declares one intra-data edge (graph→columnar, ARCH:41) but is blind to the ~15 import edges and all 4 cycles; ARCH:43 (`tabular ← artifacts/figures` color palette) is declared-unwired (no consuming fence). Either declare the intra-data topology or add a cycle-audit surface; strike or wire ARCH:43.

Secondary (lower-tier): (8) `datafusion` admitted-unconsumed pending `QuerySpec.Federated`; (9) TASKLOG staleness — GRAPH_DEEPEN/GRAPH_COMMUNITY done-but-QUEUED, INTEROP_STREAM's "3-row Backend" stale (now 5); (10) `ManifestExport` (field) vs `ManifestWrite` (virtual) near-identical manifest naming on adjacent owners.

CSHARP-FIT VERDICT: CLEAN. 7 content-keyed wire seams, all reproducing the one C#-owned XxHash128 seed via runtime ContentIdentity, decoded at the boundary, re-minting nothing; no C#-interior coupling. The folder's strongest structural dimension.
