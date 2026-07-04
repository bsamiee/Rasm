# DOSSIER — Persistence lane=query-ingest

Scope: `Query/{lane,cypher,topology,columnar,cache}.md` + `Ingest/tabular.md`. Anchors are `.planning/`-relative unless prefixed. `ARCH:` = package `libs/csharp/Rasm.Persistence/ARCHITECTURE.md` (root, NOT `.planning/`, 117 LOC). Register rows re-verified on disk: assigned E4/E13/E14(columnar); cross-anchored E6/E2/E11. Member verification: `assay` present; load-bearing external members proven against the decompile-verified `.api` catalogs (assay-catalog agree where checked).

Verified line counts: lane 520 · cypher 449 · topology 322 · columnar 491 · cache 251 · tabular 307.

---

## [00] REGISTER VERDICTS (assigned + my-lane anchors)

| Row | Status | Corrected anchor / evidence |
|---|---|---|
| E4 band collisions | **DRIFT** | Collisions all REAL; anchors off-by-one; columnar band is **8350-8356** (lines 143-149), not "8350-8352 @144-146". See [01]. |
| E13 folder/page overload | **DRIFT** | Ingest one-file HOLD; lane is **520** LOC and the retrieval span is **:267-520** (not "521"/":267-521"); Sep prose-charter at **5,18,20,297** (not "3,5,18,298"). See [05],[03]. |
| E14 parameterization (columnar) | **DRIFT** | Raw-interp + 4 posture literals REAL; anchors **255 / 356-360 / 390** (not 256/356-361/391); `:271` is itself a raw-interp site (mis-labeled "bound"); Egress `{projection}` (raw SQL) + `{stamp}` are additional understated sites. See [04]. |
| E6 seam-ledger (my rows) | **HOLD** | All four confirmed: ARCH:55 topology←reconciliation declared-unwired/mis-targeted; ARCH:57 lane Substrait half phantom; VECTOR_CODEBOOK⇄Compute + cache-L2⇄AppHost wired-undeclared. See [X1]. |
| E2 orphan (my anchors) | **DRIFT** | cache.md:185 swap-row charter HOLD (exact); Ara3D DEBUG-IL at **columnar.md:410** (not 411). See [X2]. |
| E11 OfGraph hot path (topology) | **HOLD** | `topology.md:76` `TopologyView.Of` → `ContentAddress.OfGraph(graph)` per snapshot — exact. |

---

## [01] E4 BAND COLLISIONS — disk truth table (assigned)

Every collision the register asserts is REAL. The register understates the columnar band and drifts one line low on nearly every anchor. Full on-disk map of the 83xx space touched by my lane + its collision counterparts:

| Owner | ns | Codes | Disk lines | Collides with |
|---|---|---|---|---|
| `ColumnarFault` (columnar) | `.Query` | **8350-8356** | 143-149 | `ServerFault` on 8350-8352 |
| `ServerFault` (provisioning) | `.Store` | 8350-8352 | **219-221** | `ColumnarFault` on 8350-8352 |
| `GraphFault` (cypher) | `.Query` | 8360-8363 | 68-71 | lane codebook 8360-8363 |
| lane codebook (bare `Error.New`) | `.Query` | 8360-8363 | 432,435,437,441 | cypher `GraphFault` 8360-8363 |
| `TopologyFault` (topology) | `.Query` | 8370-8371 | 179-180 | tabular + provisioning-loose |
| `TabularFault` (tabular) | `Ingest` | 8370-8373 | 104-107 | topology 8370-8371 + provisioning-loose 8371-8373 |
| provisioning loose (`Error.New`) | `.Store` | 8371-8375, 8379, 8380 | 72-74, **292-293**, **320**, **325** | tabular/topology on 8371-8373 |
| `GraphFault` (graph, store rail) | `.Persistence` | 8300-8302 | **154** | simple-name ×2 with cypher `GraphFault` |

Findings:
1. **835x is a two-owner collision, wider than stated.** `ColumnarFault` owns the whole decade 8350-8356 (7 cases, columnar.md:143-149); `ServerFault` re-uses 8350-8352 (provisioning.md:219-221). Register anchor "columnar.md:144-146 ≡ provisioning.md:220-222" is off-by-one on both sides (8350 is at columnar:143 / provisioning:219). `Expected.Code` is the telemetry key → 8350/8351/8352 each resolve to two owners.
2. **836x collision is EXACT (HOLD).** cypher `GraphFault` 8360-8363 (cypher.md:68-71) ≡ lane codebook 8360-8363 (lane.md:432,435,437,441). The lane codebook is the **only un-banded owner in my lane** — bare `Error.New(8360..8363)`, the SEAM_AND_RAIL_LAW "deleted form" (a rail fault MUST be a `[Union] : Expected` case). This is the single hardest breach on my six pages.
3. **837x is a THREE-way collision, worse than stated.** `TopologyFault` {8370,8371} ∩ `TabularFault` {8370,8371,8372,8373} ∩ provisioning-loose {8371,8372,8373,...}. The register lists these as separate clusters; on disk 8371/8372/8373 are simultaneously tabular cases AND bare provisioning `FailureRank` receipt codes (provisioning.md:72-74). provisioning's loose anchors drift: 8374/8375 at **292-293** (register 293-294), 8379 at **320** (register 321), 8380 at **325** (register 326).
4. **Simple-name `GraphFault` ×2 (HOLD).** graph.md:154 (ns `Rasm.Persistence`, store rail, 8300-8302) vs cypher.md:60 (ns `Rasm.Persistence.Query`, 8360-8363). Two `GraphFault : Expected` unions; register graph anchor ":155" is off-by-one (decl at :154). V4 renames cypher's to `CypherFault`.
5. **Prose neighborhoods AGREE on decade labels but are collision-blind.** topology.md:166 ("837x … beside ColumnarFault 835x and GraphFault 836x"), columnar.md:128 ("835x … beside GraphFault 836x, TopologyFault 837x"), cypher.md:49 ("836x"). The register calls these "inconsistent" — precisely, they are mutually *consistent* on the decade map but each is blind to the collision inside its own decade (columnar unaware of ServerFault; topology unaware of tabular/provisioning). The defect is the absence of a type-enforced registry, not label disagreement. V4 (one `[SmartEnum<int>]` band registry, duplicate integer fails at type-init) is the correct resolution; per-page decade prose dies for one registry pointer.

**Charter-as-it-should-be (my lane's contribution to the V4 map):** Columnar keeps 835x (8350-8356); provisioning `ServerFault` + loose receipt codes re-band to a fresh decade. Cypher renames `GraphFault`→`CypherFault`, keeps 836x. lane codebook gains a typed **`RetrievalFault`** union (V5b's new `Query/retrieval.md` page) on a fresh band — killing the bare `Error.New`. Topology keeps 837x (8370-8371); Tabular re-bands off 837x. Every one of these unions derives `Code => Band + n` from the one registry.

---

## [02] Query/cypher.md — per-page verdict

Grade holds at the register's 8.5 (rename-only). **Genuinely strong; the injection-safe `format('%L')` composition is real and catalog-confirmed.**

- **SOUND, preserve:** The `pgr_TSP` matrix-SQL is composed server-side via `format('SELECT * FROM pgr_dijkstraCostMatrix(%L, %L::bigint[], directed => false)', @edges, @vids::text)` (cypher.md:381) — pgRouting re-parses the matrix text so an Npgsql parameter cannot bind inside it, and BOTH the Edges-SQL and the `bigint[]` vids are `%L`-quoted (the array re-cast `::bigint[]`). The reasoning at :367-374 is correct: a bare `%s` would inject the unquoted `{1,2,3}` array-output form. This is the strongest boundary in my lane.
- **SOUND:** The three result spaces never conflate — AGE→`ElementSet` (node-space), pgrouting NODE→`H3Cell` cell-space, `pgr_bridges` cut-EDGE→raw `Severed` `long`s (cypher.md:240,276,289). `CleaveKind.NodeValued` is a live decode discriminant (:420-427) keeping an edge id out of `CellOf`. This is award-grade modeling.
- **Member proof (all HOLD, no phantoms):** `ag_catalog.cypher(graph,$$..$$,params agtype) RETURNS SETOF record` (api-apache-age.md:74); `age_vle` engine for `Reach` (api-apache-age.md:118,128); `pgr_KSP`/`pgr_TSP`/`pgr_dijkstraCostMatrix`/`pgr_articulationPoints`/`pgr_bridges` (api-pgrouting.md:66,72,91,114,115). The version-specific claim cypher.md:375-376 ("4.0 pgr_TSP dropped the annealing knobs, only start_id/end_id remain") is catalog-confirmed (api-pgrouting.md:85). `NpgsqlDataSourceBuilder.UsePhysicalConnectionInitializer` (cypher.md:96-98) confirmed (api-npgsql.md:142, "disqualifies NoResetOnClose").
- **DEFECT (E4):** `GraphFault` (cypher.md:60) — rename to `CypherFault`, re-derive `Code` through the registry (V4). Band prose cypher.md:49 dies.
- **CROSS-LEG (E13/V5c):** cypher.md:3 consumes `Store/provisioning#SERVER_EXTENSIONS ServerExtension.CreateSql` on the identity-migration rail — a leg-3 page consuming a leg-4-owned vocabulary. HOLD; the frozen-contract ruling binds (leg 4 may not alter the `ServerExtension` row shape cypher froze).
- **LOW / dead-carrier:** `AgtypePath.Weight` (cypher.md:198) is always `0.0` — the Match/Reach decode (:310) emits `new AgtypePath(Seq(id), 0.0)`, one single-vertex path per row, so both `Vertices` (always singleton) and `Weight` (always 0.0) are under-utilized. The `AgtypePath` abstraction is heavier than the decode ever populates. Harden: either populate a real path weight from the AGE result or collapse the Match decode to a flat `Seq<NodeId>`.

---

## [03] Query/topology.md — per-page verdict

Grade 9 (register). **The one genuine correctness gap in my lane sits here, catalog-flagged.**

- **DEFECT (NEW, MEDIUM — beyond register):** `Lca` (topology.md:257-264) runs `OfflineLeastCommonAncestor` over the `Spatial` view with **no `IsDirectedAcyclicGraph` pre-gate**, while `Order` (:301-306) IS gated. api-quikgraph.md:26 explicitly flags Tarjan `OfflineLeastCommonAncestor` as **"unsound over the [cyclic graph] … `IsDirectedAcyclicGraph` pre-gated"**. topology.md itself models containment cycles as a real conflict (`Cycles` query :208; `Version/merge#STRUCTURAL_DIFF ContainmentCycle` :126). So a malformed Spatial view (a Contain+Aggregate cycle) makes `Ancestor`/`Lca` return an unsound `Common` rather than railing a typed fault. **Charter-as-it-should-be:** `Lca` pre-gates `tree.IsDirectedAcyclicGraph()` and rails `TopologyFault.Cyclic` (or a new `NonTree` case) on a cyclic Spatial view, symmetric with `Order`.
- **DEFECT (E4):** `TopologyFault` 8370-8371 (topology.md:179-180) re-derives through the V4 registry; keeps 837x. Band prose :166 dies.
- **CONFIRMED (E11):** `TopologyView.Of` (topology.md:76) computes `ContentAddress.OfGraph(graph).Value` per snapshot — the memo hot path. Per V12 this stays the Element-seam `OfGraph` (topology's memo is acceptable; the O(delta) concern is timetravel's Scrub, not mine). HOLD as anchor.
- **DEFECT (E6, my ARCH row):** `ARCH:55 Query/topology ← Rasm/Spatial/reconciliation [CONTENT_KEY] adjacency-derived GeometryHash` — topology.md has **zero** reconciliation references (no `GeometryHash`/`NamingHashOps`/`NameAddress`/`TopoName`; only `ContentAddress.OfGraph`). The upstream is LANDED (`Rasm/Spatial/reconciliation.md`, kernel counterpart `Rasm/ARCHITECTURE.md:79`). This ARCH row is **mis-targeted** — per V12 the adjacency-signature seam feeds `Version/merge`'s diff (`GraphNode.GeometryHash`), not topology. Correct: re-point ARCH:55 off `Query/topology` onto `Version/merge`; topology keeps only the OfGraph memo.
- **LOW / dead-carrier:** `TypedEdge.IsContainment` / `TypedEdge.Kind` (topology.md:66-67) — the traversals filter at *view-build* time via `EdgeFilter.Admit(rawRelationship)` (:91,103) and post-build read only `e.Source`/`e.Target` (OutEdges/InEdges). No fence reads `edge.Kind`/`edge.IsContainment` off a `TypedEdge`. The comment (:61-63) justifies them for "a kind-filtered traversal" but the realized traversals never need per-edge kind (the view is already kind-filtered). Candidate dead accessors; keep only if an external consumer reads them, else drop.
- **Member proof (HOLD):** every QuikGraph member confirmed (api-quikgraph.md): `TreeBreadthFirstSearch`→`TryFunc` (:105), `OfflineLeastCommonAncestor(root, IEnumerable<SEquatableEdge>)` (:117), `SourceFirstTopologicalSort` (:94), `IsDirectedAcyclicGraph` (:96), `Roots`/`Sinks` (:108), `Strongly`/`WeaklyConnectedComponents` (:118), `ShortestPathsDijkstra` (:129). api-quikgraph.md:149 already anchors its STACKING to `Query/topology#GRAPH_TOPOLOGY` — no phantom.

---

## [04] Query/columnar.md — per-page verdict (E14 assigned)

Grade 8.5→9.5 target. Deep, honest engine owner; the E14 trust-gate gap is real and **wider than the register credits**.

- **DEFECT (E14, HOLD substance / DRIFT anchors):** raw-interpolated identifiers & paths with NO typed trust gate, beside correctly `DuckDBParameter`-bound values:
  - `Mount` (columnar.md:**255**, register said 256): `$"ATTACH IF NOT EXISTS '{store}' AS {alias} (TYPE {typed.Key}, READ_ONLY)"` — `{store}` (path) and `{alias}` (identifier) raw.
  - `Secret` (columnar.md:**271**, register mis-labels this "bound"): `$"CREATE OR REPLACE SECRET {name}{into} (TYPE {scope.Key}, PROVIDER {scope.Provider}, …)"` — the secret `{name}` and persist alias `{into}` are RAW; only the config *values* bind (`DuckDBParameter` :270). The register classified :271 as a bound-value exemplar — on disk it is a mixed site with a raw secret name.
  - `Egress` (columnar.md:**356-360**): `$"COPY ({projection}) TO '{destination}' (… KV_METADATA {{ stamp: '{stamp}' }})"` — `{projection}` is an **entire caller-supplied SQL SELECT fragment** interpolated raw (most concerning), plus `{destination}` (path) and `{stamp}` (quote in stamp breaks out). The register omits `{projection}`/`{stamp}` — the surface is larger than "identifiers/paths".
  - `Generation` (columnar.md:**390**, register said 391): `read_parquet('{root}/**/*.parquet', …)` — `{root}` raw.
  - Properly-bound exemplar (`Query`, columnar.md:224-226): every `{i}` value hole rewrites to `$pi` and binds `DuckDBParameter($"p{i}", …)`. This is the correct pattern the identifier/path sites lack.
  - **Charter-as-it-should-be:** a typed `Identifier`/`StorePath`/`SecretName` value-object family (validated `[A-Za-z_][A-Za-z0-9_]*` / path allowlist) gates every interpolated identifier/path; `{projection}` becomes a composed `SetExpr`/typed projection, never a raw string. V10's "typed trust gate" is exactly this.
- **DEFECT (E14, HOLD):** four repeated posture literals (columnar.md:80-83) — every `ColumnarProfile` row hardcodes `"80%"` (memoryCap) and `"90%"` (spillCap). Collapse to one package constant (V10).
- **CONFIRMED (E2, DRIFT anchor):** Ara3D DEBUG-IL caveat at columnar.md:**410** (register said 411): "`Ara3D.BimOpenSchema[.IO]` assemblies are DEBUG-built". api-ara3d-bimopenschema.md:17 confirms `AssemblyConfiguration("Debug")`, `DisableOptimizations | EnableEditAndContinue`, release `1.0.1`. The E2 upstream-fix demand stands.
- **Member proof — DuckDB mapped-appender surface is REAL (NOT phantom):** api-duckdb.md confirms `DuckDBMappedAppender<T,TMap>` (:61), `DuckDBAppenderMap<T>` (:62), `CreateAppender<T,TMap>` (:119), `.AppendRecords`/`.Close` (:124-125), `DuckDB.NET.Data.Mapping` ns (:24), `Duplicate` (:101), `GetQueryProgress`→`DuckDBQueryProgress{double Percentage}` (:37,102), `UseStreamingMode` (:108). ADBC: `AdbcConnection.CreateStatement`/`AdbcStatement.SqlQuery` set/`QueryResult.Stream`/`Bind` (api-adbc-apache.md:86-87, api-arrow.md:232,136). ParquetSharp.Arrow ns confirmed (api-parquetsharp.md:12).
  - **LOW phantom-spelling:** columnar.md:289 uses `statement.ExecuteQueryAsync()`; the catalog documents sync `ExecuteQuery()` (api-adbc-apache.md:11,87). Verify the async variant exists on `AdbcStatement` or fall back to `ExecuteQuery()` wrapped in `IO.liftAsync`.
  - **LOW phantom-spelling:** columnar.md:3,467-468 call `BimData.WriteDuckDB` / `frames.WriteDuckDB(path)` as an instance method; the catalog lists `DuckDbUtils.WriteToDuckDB(IDataSet)` (:56) and `BimDataSerializer.WriteDuckDB`, with `BimData.ToDataSet()` (:85) producing the `IDataSet`. The exact receiver `BimData.WriteDuckDB` is unverified — real call is likely `frames.ToDataSet().WriteToDuckDB(path)` or `BimDataSerializer.WriteDuckDB(frames, path)`.
- **LOW seam-precision (E6-adjacent):** `ARCH:56 Query/columnar ← Rasm.Bim/Model [PROJECTION] BIM-typed BimOpenSchema FlatTableProjection (Bim-implemented seam)` — but columnar.md:435-448 **implements** `BimOpenSchemaProjection : FlatTableProjection` inside Persistence with a generic structural map (`Header.Schema.Key`/`Delta.NodeCount`). columnar.md:410 hedges both branches ("EAV-generic → Persistence owns; BIM-typed → Bim-implemented"). The realized fence is the Persistence-owned generic branch, tensioning the ARCH's "Bim-implemented" declaration. DECISION should rule which branch is law and align ARCH:56.
- **CROSS-CUT (E3):** api-duckdb.md:176 anchors its receipt row to the **deleted** `Store/profiles` page — a live catalog-drift instance touching my columnar package (register E3 lists `api-duckdb→Store/profiles`). Re-anchor onto `Query/columnar` in the leg-1 catalog motion.

---

## [05] Ingest/tabular.md — per-page verdict (E13 assigned)

Grade 8 (register, folder-move only). **The folder law and a real dead-carrier both sit here.**

- **CONFIRMED (E13):** `Ingest/` is a one-file folder (only `tabular.md` on disk) — the standing folder-architecture-law violation. V11: grow to a ≥2-page codec-ingress axis (`+schedule.md` if a durable-schedule consumer exists in the Bim seam, else `tabular` folds up to a top-level page — the folder never stays at one file).
- **CONFIRMED (E13, HOLD substance / DRIFT anchors):** `Sep` is chartered **only as prose** — named at tabular.md:**5,18,20,297** (register said "3,5,18,298"; line 3 says "delimited" without naming Sep, the table row is 297) and NEVER composed in any code fence. No `SepReader`/`SepReaderOptions`/`Parse<T>` appears in a fence; the realized codec is entirely MiniExcel. Sep members exist (api-sep.md:61-63: `SepReader`, `.Row`, `.Col.Parse<T>`, `IAsyncEnumerable<Row>`), so the charter is realizable but unrealized. **Charter-as-it-should-be:** V11 makes `Sep` explicit owner law — either a fenced `Sep` delimited-lane surface within tabular, or the delimited concern is genuinely MiniExcel-covered and the Sep charter/roster row drops.
- **DEFECT (NEW, MEDIUM — beyond register): `TabularWire.Wire` is a dead carrier / unwired seam.** `Wire(string column)` (tabular.md:272-273) builds a `DynamicExcelColumn` with a `CustomFormatter`, and the comment (:269) claims it is "registered on the spec's policy `DynamicColumns`". But `TabularFormat.Policy()` (:40-44) attaches **no** `DynamicColumns`, and **no fence calls `Wire(`** (grep: only the definition + comments). The Scan/Reader/Write "dynamic/reader/write legs" the comment cites never wire it either. So the `CustomFormatter` cell-projection path (table row :301, "wire-cell projection") is asserted in prose but absent from every realized read/write path — the typed read uses `Bind<T>` (STJ), which IS wired; `Wire`/`CustomFormatter` is orphaned. **Charter-as-it-should-be:** either compose `Wire`-built `DynamicExcelColumn`s into `Policy().DynamicColumns` on the dynamic/reader/write legs (making the CustomFormatter law real), or delete `Wire` and drop the CustomFormatter claim — the STJ `Bind<T>` factory is the single wire path.
- **DEFECT (E4):** `TabularFault` 8370-8373 (tabular.md:104-107) re-bands off 837x through the V4 registry (collides with topology 8370-8371 + provisioning-loose 8371-8373).
- **SOUND, preserve:** `TabularSpec` as the one modality discriminant (typed/dynamic/reader/probe/egress all dispatch on it, no `ReadTyped`/`ReadDynamic` family); `Origin [Union]` absorbing the path-vs-stream overload split; the `TabularFault.Lift` funnel folding all three MiniExcel exceptions on both read AND write legs; the per-column `RedactionPlan.Cell` that passes unclassified cells through as their typed `object?` (:149-150) rather than stringifying. These are genuinely dense.
- **Member proof (HOLD):** MiniExcel `Query`/`QueryRange`/`GetReader`/`SaveAs`/`Insert`/`SaveAsByTemplate`/`ConvertCsvToXlsx`/`MiniExcelDataReader`/`OpenXmlConfiguration`/`DynamicExcelColumn`/`DateOnlyConversionMode` — page correctly avoids the `where T:class,new()` `Query<T>` POCO binder (:16,20,299). `Redactor`/`IRedactorProvider`/`GetRedactor(DataClassificationSet)`/`Redact<T>` (api-redaction). `linq2db` `BulkCopy`/`BulkCopyAsync` are `IEnumerable<T>`-sourced (consumes the typed enumerable, not the reader — :20,303). Note: `linq2db.EntityFrameworkCore` is a brief-CONDITIONAL package — tabular.md:5,18,20,303 references it only in prose; per `[04]` it "becomes a composed fence or it leaves". Currently prose-only → leave-candidate unless V11 composes it.

---

## [06] Query/lane.md — per-page verdict (E4 + E13 + E6)

Grade 8.5→9.5 target. **Four concerns, the V5b split axis, and the worst E4 breach (the only un-banded owner).**

- **CONFIRMED (E13):** four concerns — `[02]READ_ROUTING` (12-125), `[03]ELEMENT_SET_ALGEBRA` (127-266), `[04]FUSION_AND_CACHE` (267-377), `[05]VECTOR_CODEBOOK` (379-520). File is **520** LOC (register "521"). V5b extracts `FUSION_AND_CACHE`+`VECTOR_CODEBOOK` (the coupled ANN subsystem) → `Query/retrieval.md`; the retrieval span is **:267-520** (register ":267-521" overshoots by one). The coupling is real and bidirectional: fusion's vector branch reads codebook rows (lane.md:275, "resolving through `#VECTOR_CODEBOOK VectorRow.Subject`-mapped ranked rows"), codebook feeds fusion (lane.md:388, "feeds `#FUSION_AND_CACHE FusionRank.Fuse` as one ranked branch") — one ANN owner, correctly extracted together. lane drops to routing + set-algebra.
- **DEFECT (E4, the hardest breach):** the codebook `Train` rails **bare `Error.New(8360..8363)`** (lane.md:432,435,437,441) — the ONLY un-banded fault owner in my six pages, the explicit SEAM_AND_RAIL_LAW "deleted form", colliding with cypher `GraphFault` 8360-8363. V4/V5b: becomes a typed **`RetrievalFault : Expected`** union on the new `Query/retrieval.md` page, band from the registry. **This is the single most load-bearing E4 fix in the lane.**
- **DEFECT (E6, my ARCH row, wired-undeclared):** the `VECTOR_CODEBOOK⇄Compute/Model/embedding` seam is WIRED in-fence (lane.md:381,384,388 — `VectorCodebook.Train` owns PQ training HERE, Compute encodes against `ProductCodebook` by project reference, dependency `Compute→Persistence`) and declared by Compute (`COMPUTE:99`), but the own `ARCHITECTURE.md` has **no** row (grep: zero `codebook`/`embedding`/`VECTOR_CODEBOOK`/`Compute/Model` seam rows). V12 adds `Query/lane (or retrieval) VECTOR_CODEBOOK ← csharp:Rasm.Compute` to the SEAMS.
- **DEFECT (E6, my ARCH row, phantom half):** `ARCH:57 Query/lane ⇄ python:data/tabular/query [WIRE] ElementSet receipt currency + Substrait portable plan`. The **ElementSet currency half is real** (lane.md:127-266 owns `ElementSet`/`SetExpr`/`Receipt`); the **Substrait portable-plan half is phantom** — no `FederatedPlan`/`SetExpr`-lowering/federation owner exists on disk. This is the V1 federation-disposition hinge: reintroduce `Query/federation.md` (lowering Substrait onto the existing `SetExpr`) or retire and re-scope ARCH:57 to the ElementSet half only.
- **SOUND, preserve (register-praised):** the `StalenessWatermark` sequence-gap measurement (lane.md:40-116) correctly refuses the recording-clock `ShardState.Timestamp` confound and rails a maximally-stale watermark for a never-projected shard (:107-115) — genuinely careful. The length-framed `ElementSet.Canonical` preimage (:218-230) freezes the `elementset` parity vector that `Version/commits#CRDT_WIRE ContentParityCorpus.Contribute` consumes (CONTRIBUTED by lane, never reverse-imported — correct direction; commits.md is the CRDT_WIRE owner, verified). The `Closure` bounded transitive fold (:246-256) threads the topology `Expand` as a port, keeping reachability with the graph owner.
- **Member proof (HOLD, no phantoms):** all six pgvector distance members `L2Distance`/`MaxInnerProduct`/`CosineDistance`/`L1Distance`/`HammingDistance`/`JaccardDistance` with ops `<->`/`<#>`/`<=>`/`<+>`/`<~>`/`<%>` confirmed exact (api-pgvector-ef.md:158-163), incl. the bit-arity→Hamming/Jaccard vs float→four-metrics split (lane.md:297 matches catalog arity columns). Marten watermark surface: `FetchEventStoreStatistics`/`AllProjectionProgress` (api-marten.md:58,148), `EventStoreStatistics` (sequence+high-water, :107), `ShardState`/`ShardName` (:106). `HybridCache.GetOrCreateAsync(key,state,factory,options,tags)` + `HybridCacheEntryOptions` (api-hybrid-cache.md). `TensorPrimitives.Distance/Add/Divide` (shared tier). **Seam-honesty flag (LOW):** lane.md:82-84 binds `store.WaitForNonStaleProjectionDataAsync(timeout)` which resolves through `Marten.Events.TestingExtensions` (api-marten.md:217 confirms the store/host overloads are TestingExtensions wrappers). lane.md:19 acknowledges this and argues it forwards to the production `IMartenDatabase.WaitForNonStaleProjectionDataAsync`. The member exists (not a phantom), but a production entry binding a `TestingExtensions` symbol is a latent smell — DECISION may prefer routing through `IProjectionDaemon`/`IMartenDatabase` directly.
- **AdcScan logic (SOUND):** the min-priority-queue max-heap keeping the nearest `top` (lane.md:459-470) is correct — priority `-distance`, peek yields the largest kept distance, `EnqueueDequeue` on `distance < worst`, extraction descending into ascending-by-distance order. No defect. (Minor: :463 iterates `codes.Length` without asserting `== codebook.Subspaces`; PQ codes are constructed to match — LOW.)

---

## [07] Query/cache.md — per-page verdict (E6 + E2)

Grade 9 (register, sound; "a batching note, never a rebuild"). **No E4/E13/E14 anchors; the L2 seam and swap-row charter both HOLD.**

- **CONFIRMED (E6, wired-undeclared):** the `cache#L2_CONTRIBUTION⇄AppHost CACHE_PORT` seam is WIRED (cache.md:191-232 implements `CacheL2Store : IBufferDistributedCache` + `CacheCodecFactory : IHybridCacheSerializerFactory` for the AppHost port) and declared by AppHost (`APPHOST:69`), but the own ARCHITECTURE.md has **no** `Query/cache ⇄ AppHost [CACHE_PORT]` row — ARCH:58 declares only the *Compute→cache index* seam (`Query/cache ← Rasm.Compute [INDEX]`), a different seam. V12 adds the L2⇄AppHost row.
- **CONFIRMED (E2, HOLD exact):** cache.md:185 states the swap-row charter — "a redis swap is the `Microsoft.Extensions.Caching.StackExchangeRedis RedisCache` (itself an `IBufferDistributedCache`, so the buffer-contract seam survives the swap)". This is the charter that makes the raw `StackExchange.Redis` direct ref (PROPS:314) prunable (V7). api-hybrid-cache.md:89 confirms `RedisCache` implements `IBufferDistributedCache`.
- **CONFIRMED (forward-consumer pressure):** cache.md:20 one-row growth law ("a new artifact family is one `ArtifactKind` row") is load-bearing for Compute's assessment `ResultBlob` (RASM-CS-COMPUTE-BRIEF `[V1]`) and Fabrication's durable rows (V12: programs/`.cli` stacks/3MF/NC1/travelers as `ArtifactKind` rows on the one artifact index). HOLD — survives any harden.
- **Member proof (HOLD):** `IBufferDistributedCache : IDistributedCache` with `TryGetAsync(IBufferWriter<byte>)` + `SetAsync(ReadOnlySequence<byte>)` (api-hybrid-cache.md:80,85,91); `IHybridCacheSerializerFactory.TryCreateSerializer<T>([NotNullWhen(true)] out …)` exact (api-hybrid-cache.md:76 — matches cache.md:223). `MessagePackSerializer.Serialize/Deserialize` with options. Marten session members standard.
- **LOW prose-precision:** cache.md:182 cites `HybridCachePayload.Write`/`TryParse` and `ExpiredByEntry`/`ExpiredByTag`/`ExpiredByWildcard` as the runtime's expiry-envelope owner — these are **internal** Microsoft.Extensions.Caching.Hybrid types, cited only in prose to justify why `CacheL2Store` is a pure storage leg (no read-time expiry gate). Fine as reasoning, but they are not public API the fence composes; keep the citation lightweight.
- **SOUND, preserve:** the sync-over-async bridges (cache.md:192,210-217) are the accepted `IDistributedCache` boundary shape (register). The `ModelResultKey.Content` length-framed preimage (cache.md:83-107) reuses the same `Canonical` framing law as lane's `ElementSet` — one collision-free discipline across both. The horizon gate folded INTO `Lookup` (cache.md:120-121, `Filter(row => Fresh(...))`) — no caller-applied bool.

---

## [X1] CROSS-CUTTING — E6 seam ledger (my lane, all four HOLD)

The own `ARCHITECTURE.md` (117 LOC, package root) SEAMS block, verified against fences:

| ARCH row | Kind | Verdict |
|---|---|---|
| ARCH:55 `Query/topology ← Rasm/Spatial/reconciliation` GeometryHash | declared-unwired | **Mis-targeted** — topology.md has zero reconciliation refs; upstream landed; real consumer is `Version/merge` (V12). Re-point off topology. |
| ARCH:57 `Query/lane ⇄ python:data/tabular/query` ElementSet + Substrait | half-phantom | ElementSet currency real (lane.md:127-266); Substrait-plan owner absent (V1 hinge). |
| VECTOR_CODEBOOK⇄Compute/Model/embedding | wired-undeclared | lane.md:381-388 wires; COMPUTE:99 declares; **no own ARCH row**. Add it. |
| cache#L2_CONTRIBUTION⇄AppHost CACHE_PORT | wired-undeclared | cache.md:191-232 wires; APPHOST:69 declares; **no own ARCH row** (ARCH:58 is the Compute→cache INDEX seam, different). Add it. |

Additional ARCH rows for my pages that ARE declared + wired (HOLD): ARCH:56 `Query/columnar ← Rasm.Bim/Model` (BimOpenSchema — but see the [04] Bim-implemented-vs-Persistence-implemented tension), ARCH:58 `Query/cache ← Rasm.Compute [INDEX]`, ARCH:61 `Ingest/tabular → Rasm.Element [WIRE] row-shape-only`, ARCH:62 `Query/columnar ⇄ python:data/tabular [WIRE] Arrow over ADBC`.

## [X2] CROSS-CUTTING — catalog drift touching my packages (E3)

- api-duckdb.md:176 → dead `Store/profiles` receipt anchor (columnar's package). Re-anchor `Query/columnar`.
- (Register E3 also lists `api-arrow→Query/rail+Sync/egress`, `api-sep→Query/lanes`, `api-h3/api-pgvectorscale/api-timescaledb→Query/lanes`, `api-apache-age/api-pgrouting/api-pg-graphql→Query/federation`, `api-pg-search→Store/server` — all dead anchors on catalogs my lane consumes; not re-verified line-by-line here, flagged for the leg-1 catalog re-anchor motion.)

## [X3] CROSS-CUTTING — naivety axes (beyond register)

**NA-1 (naive / insufficient):**
1. `topology.md:257-264` `Lca` — no DAG pre-gate on an operation the substrate (api-quikgraph.md:26) declares unsound over cyclic input, while the page models containment cycles as real. MEDIUM. [see 03]
2. `tabular.md:272-273` `TabularWire.Wire` — unwired CustomFormatter path; the "wire-cell projection" law (table row :301) is unrealized. MEDIUM. [see 05]
3. `columnar.md:255,271,356-360,390` — no typed identifier/path trust gate; `{projection}` interpolates a raw SQL fragment. MEDIUM (E14). [see 04]

**NA-2 (over-engineered / needless):** none material. `StalenessWatermark.Lag` (lane.md:40-47) is effectively bi-valued (Zero/MaxValue) — a Duration carrying a boolean — but is defensible for the `Stale(ceiling)` contract; LOW, not worth collapsing. cypher/topology's rich `GraphQuery`/`TopologyQuery` case rosters are brief-endorsed depth, not sprawl.

**Dead carriers (beyond register E9's blobstore/ledger/retention list):**
- `cypher.md:198` `AgtypePath.Weight` — always 0.0. LOW.
- `topology.md:66-67` `TypedEdge.IsContainment`/`Kind` — unread post-build. LOW.
- `tabular.md:272-273` `TabularWire.Wire` — unwired. MEDIUM.

## [X4] SUMMARY — the lane's real perimeter debt

The six fence interiors are strong-to-award-grade (cypher's `format('%L')`, lane's watermark honesty + parity contribution, topology's SCC-once collapse, columnar's posture-anchor session, cache's folded horizon gate, tabular's spec-discriminant). The debt is the PERIMETER, exactly as the brief's verdict predicts, plus a bounded set of in-fence gaps:

- **E4 is the dominant defect**: three collision decades + the lane's bare `Error.New` (the only un-banded owner) + the `GraphFault` simple-name ×2. One V4 registry resolves all of it; the register's anchors are systematically off-by-one and understate the columnar (8350-**8356**) and 837x (three-way) spans.
- **Two new in-fence gaps** the register does not carry: `topology.Lca` DAG-pre-gate (correctness) and `tabular.TabularWire.Wire` dead carrier (unwired seam).
- **All four E6 seam rows** touching my lane confirmed — two declared-unwired/phantom (ARCH:55 mis-targeted, ARCH:57 Substrait half), two wired-undeclared (codebook⇄Compute, L2⇄AppHost).
- **Two low phantom-spellings** to close before the columnar leg: `AdbcStatement.ExecuteQueryAsync` and `BimData.WriteDuckDB` receiver.
