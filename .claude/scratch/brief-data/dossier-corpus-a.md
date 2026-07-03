# DOSSIER — data/.planning corpus-a (graph, gridded, impact)

Lane: corpus-a. Scope read FULLY: `graph/graph.md`, `gridded/store.md`, `gridded/field.md`, `gridded/virtual.md`, `gridded/ragged.md`, `impact/impact.md`; `data/ARCHITECTURE.md`, `README.md`, `TASKLOG.md`, `IDEAS.md`; upstream `RASM-RUNTIME-BRIEF.md` (full); ref `libs/.planning/architecture.md`; `RASM-COMPONENT-PARADIGM-DECISION.md` `[AMENDMENTS]`. Anchors are `libs/python/data/.planning/<page>` unless noted.

Stance note: the gridded quartet (store/virtual/ragged/field) is genuinely world-class in density and ADT collapse; the two systemic defects below are NOT illusory-depth on those pages — they are corpus-wide paradigm/seam defects that survive because no page owns the folder-level rail choice. impact.md IS illusory depth. graph.md is strong but carries stale governance + hardcoded weights.

---

## PER-PAGE VERDICTS

### graph/graph.md — 8/10
Single-owner `GraphPayload`/`GraphAlgorithm`/`GraphResult` over one `_run_rx` kernel, `_as_rx` coercion, igraph community split, node-keyed `frame` seam to tabular. ~40 algorithm cases; `RX_CENTRALITY`/`IG_COMMUNITY`/`_EGRESS` use the doctrinal `expression.Map.of_seq`. This page IS the dispatch-rail exemplar.
Defects:
- `graph.md:36` — stale import `from rasm.runtime.content_identity import ...`; runtime `[V4]` renames the module to `rasm.runtime.identity` (`content_identity` is the drift). Import-breaking post-runtime.
- `graph.md:27` — module-top `import igraph` (GPL). Prose (`:3`,`:18`,`:467`) asserts igraph is "GPL-confined to the community split," but an unconditional module-top import makes GPL transitive for ANY consumer importing `rasm.data.graph`. Confinement is prose-only; the structural GPL boundary is the whole module.
- Hardcoded weights vs prose-claimed selectors: `_run_rx` hardcodes `weight_fn=float` / `lambda _: 1.0` / `lambda _: 0.0` at `:329,:332,:335,:337,:343,:367,:381,:383,:385`. Prose `:12` claims shortest-path "carries the path policy" and min_cut "carries the weight selector," but the case payloads (`:96-100` bare `tuple[NodeId,NodeId]`, `:113` min_cut `None`) carry no weight/policy field. Prose-vs-fence gap + hardcoding: a graph whose edges are not float payloads cannot be weighted.
- Stale governance: `TASKLOG` GRAPH_DEEPEN + GRAPH_COMMUNITY are `[QUEUED]` (`TASKLOG.md:48,:55`) yet the fence already realizes both (40 cases incl. leiden/louvain/infomap, `IG_COMMUNITY`, backend-dispatched centrality table). Cards desynced from realized fence.
Charter as it SHOULD be: keep as the folder's dispatch-rail reference; parameterize the edge-weight selector into the shortest-path/min-cut/spanning case payloads (a `WeightSelector` policy value), gate igraph behind a lazy import or an explicit optional-extra so GPL is not unconditional, and close the two realized cards.

### gridded/store.md — 8/10
`TensorStore` two-engine axis (`ZARR`/`TENSORSTORE`), codec pipeline single-owned by `TensorChunking.shards`, arity-normalized `write_region` (`Write | Iterable[Write]`), typed empty-write `Error`, `cubed` plan `[03]-[PLAN]` with `MemoryProbe`/`PlanReceipt`, `@beartype(conf=FAULT_CONF)` admission. Very strong.
Defects:
- `store.md:32` — stale `content_identity` import.
- `store.md:19` — `from builtins import frozendict`; 12 uses (`_COMPRESSOR`,`_FILTER`,`_ZARR_WRITE/READ`,`_KVSTORE_DRIVER`,`_CREATE/_WRITE/_WRITE_MANY/_READ`, plan `_LINALG`). Not a CPython builtin (PEP 416 rejected) → `ImportError`; and it is the exact form `graph.md:14,:18` brands a deleted form.
- RESEARCH-gated density: `:655-659` defer `nbytes_stored` return shape, `TaskEndEvent.peak_measured_mem_end` field name, `tensorstore` kvstore driver names, `Transaction(atomic=True)` spelling — several fence paths are unconfirmed API assumptions, not settled.
Charter: correct; only rail unification (frozendict→Map) + identity rename + closing the four RESEARCH items.

### gridded/field.md — 7/10 (OVER-STUFFED)
745 LOC across FOUR sections (FIELD/SELECT/VIRTUAL/EGRESS) and 9+ major types: `FieldDataset`,`FieldEncoding`,`FieldEngine`,`FieldSelection`,`ReductionPolicy`,`FieldVirtual`,`VirtualParser`,`ManifestExport`,`CFDtype`,`FieldReceipt`. The `[02]-[FIELD]`/`[03]-[SELECT]` flox reduction machinery and the `[04]-[VIRTUAL]` manifest machinery are individually excellent (the flox superset `Reduction`, `_NAN_BASE`/`_SCAN_BASE` comprehension-derived fallback, the `CFDtype` resolve/inspect inverse). But the page carries two nearly-independent domains.
Defects:
- `field.md:29,:375,:673` — stale `content_identity`.
- `field.md:147` — `from builtins import frozendict` (5 uses: `_NAN_BASE`,`_SCAN_BASE`,`_SKIPNA_BASE` frozenset, `_FALLBACK_CALL`).
- `field.md:600` — `repr(var.data.manifest.dict()).encode()` as the content-key byte source. `repr(dict)` is the exact deleted form `graph.md:18` forbids ("a `repr(dict)` content-key byte stream where the canonical wire keys it") and is non-canonical/non-reproducible cross-runtime — undercuts the count-prefix canonical-bytes law (`RASM-COMPONENT-PARADIGM-DECISION.md:63`). Diverges from the sibling `virtual.md:209-210` which keys off `snapshot.encode()`/`"\n".join(refs).encode()`.
- Split pressure: `FieldVirtual`/`VirtualParser`/`ManifestExport`/`CFDtype` (~lines 346-644, ~300 LOC) is composed by the SEPARATE `gridded/virtual.md` (`virtual.md:28` imports `FieldVirtual, ManifestExport`), forcing a `virtual.py → field.py` module dependency for a single "virtual reference" concern deliberately split (TASKLOG `TENSOR_SPLIT`/`FIELD_VIRTUAL`). The manifest construction (field) and the icechunk registration (virtual) are two halves of one concern living on two pages.
Charter: `field.py` should own CF read/select/reduce/egress; the virtual-cube manifest sub-domain (`FieldVirtual`+parsers+CFDtype+ManifestExport) is a merge candidate INTO `gridded/virtual.md` (co-locating manifest construction with icechunk registration), which would dissolve the cross-page import and shrink field to a focused CF owner. Replace `repr(dict)` with a canonical manifest wire.

### gridded/virtual.md — 9/10
Exceptional icechunk owner: `VersionOp` (`aggregate`/`stamp`/`diff`/`reclaim`/`checkout`) one `apply` dispatch, nested `Reclaim`/`ReadAt`/`ManifestWrite` sub-axes, `IceStorage` 8-factory collapse, Merkle `ContentIdentity.of` over (snapshot_key, refs_key), `VirtualOutcome` union, composes `FieldVirtual` (no duplication), `ConflictSolver` auto-rebase. Correct C# Persistence seam framing (snapshot identity = Persistence wire concern, no data-side version engine).
Defects:
- `virtual.md:29` — stale `content_identity`.
- `virtual.md:23` — `from builtins import frozendict` (`_STORAGE`, 6 uses). `virtual.md:18` EXPLICITLY endorses `Final[frozendict[...]]` as "the sibling `gridded/store#STORE` convention" — the entrenchment of the wrong rail against `graph.md`'s deleted-form ruling.
- `virtual.md:28` — module-top `from rasm.data.gridded.field import FieldVirtual, ManifestExport`, but prose `:14`,`:299` claims field is "imported function-local at the accessor case." Fence-vs-prose; also transitively pulls field's module-top `virtualizarr` (`field.md:360`).
- `virtual.md:300` — cross-runtime `XxHash128` seed reproduction of the icechunk snapshot key vs `Rasm.Persistence/Version/Snapshots` flagged RESEARCH (unconfirmed the seed derivation matches C#).
Charter: correct as the icechunk version-control owner; absorb `FieldVirtual` (see field split) so the page owns the full virtual-reference concern end to end.

### gridded/ragged.md — 9/10
Exceptional awkward owner: one `RaggedSource` admission union, `RaggedOp` transform axis with the `_AXIS_OP` seven-arm collapse + one `_FOLD` closure table unifying 17 members (12 reducers/3 paired/2 order) via `_reduce`/`_paired`/`_ordered` closures keyed by `_WEIGHTED`/`_SAMPLE` sets, form-algebra round-trip, `RaggedSink` egress, validity/parameters evidence.
Defects:
- `ragged.md:29` — stale `content_identity`.
- `ragged.md:19` — `from builtins import frozendict` (`_AXIS_OP`,`_REDUCE`,`_FOLD`,`_WEIGHTED`,`_SAMPLE`). `ragged.md:16` and `:366` EXPLICITLY defend frozendict as "the sibling convention" — second page entrenching the wrong rail.
- Unwired forward seam: `ragged.md:28` imports `ArrowCStream` and `_c_stream` composes `ArrowCStream.of` (`:218-219`,`:310-311`). Per `TASKLOG.md:20-25` INTEROP_STREAM `[QUEUED]`, `ArrowCStream.of` is a PENDING promotion of interop's private `_export_c_stream` — ragged's `c_stream` composes a public classmethod the interop owner has not yet exposed. Consumer ahead of producer.
- `ragged.md:23` — module-top `import nanoarrow` while `pyarrow` is deferred; inconsistent import discipline (nanoarrow is light, minor).
Charter: correct; unify the rail; the `ArrowCStream.of` dependency resolves when INTEROP_STREAM lands (corpus-b interop.md).

### impact/impact.md — 5/10 (ILLUSORY DEPTH — weakest corpus-a page)
Prose claims a five-provider EN-15804 normalization owner (`MaterialImpact` over `ImpactSource` openepd/ilcd_epd/brightway/openlca/premise → one indicator×stage cell stream). The fence does NOT deliver it.
Defects:
- `impact.md:195,:202,:209` — `_from_openepd`, `_from_score`, `_from_olca` bodies are `...` STUBS. Only `_from_epdx` (`:173`) is realized. So 3 of 4 `_normalize` arms are unimplemented → 4 of 5 sources (openepd, brightway, premise_background, openlca) route to placeholder bodies. The headline "one `_normalize` fold over five providers" is illusory.
- `impact.md:178` — `unit=""` hardcoded empty for EVERY epdx cell (the only realized arm) → `ImpactCell.unit` is a dead field on the sole working path.
- `impact.md:28` — stale `content_identity`.
- TransportResource claimed in prose (`:16`,`:247` — EC3/openLCA "under the runtime TransportResource") but ABSENT from fence imports (`:28-30`) and unreachable (the consuming arms are stubs). Seam claimed, not wired.
- NO governance: `TASKLOG.md`/`IDEAS.md` carry ZERO impact cards (open or closed). impact/ was modified 2 days ago (vs 5 days for siblings) — recently added, its stub state untracked by any card; the `impact → Rasm.Materials/Rasm.Compute` and `impact ⇄ Rasm.Persistence` seams (`ARCHITECTURE.md:56-61`) rest on an unbuilt owner.
Charter as it SHOULD be: realize `_from_openepd` (`Impacts.get_impact_set(method).get_scopeset_by_name`), `_from_score` (bw2calc `LCA(...).score` aggregate + `MultiLCA` spread), `_from_olca` (`get_total_impacts()` rows), thread the real declared unit per cell, wire `TransportResource` into the EC3/openLCA fetch legs, and author the impact IDEAS/TASKLOG cards. Until then the page is a spec, not a design.

---

## CROSS-CUTTING FINDINGS

### C1 — Stale identity module name (import-breaking, uniform)
`from rasm.runtime.content_identity import ...` on ALL 15 identity-consuming data pages (22 occ): corpus-a graph/store/field/virtual/ragged/impact + corpus-b catalog/geospatial/mesh/columnar/contract/egress/interop/lakehouse/profile. Runtime `[V4_LEDGER_AND_CODEMAP_TRUTH]` renames the canonical module to `rasm.runtime.identity` (ruled default; `content_identity` is "the drift," rewired at runtime's own `lanes.md:49`/`identity.md:226`). The moment runtime lands, every data page's identity import breaks. Mechanical but folder-wide and blocking.

### C2 — Dispatch-table rail bifurcation (duplicate mechanism + non-importable form)
Two rails coexist for the identical concern (a `Final` callable/behavior lookup):
- `expression.Map.of_seq` — graph (9), contract (8), profile (12), egress (1), interop (1), geospatial (2), mesh (2). The doctrinal rail (runtime GENERATOR_LAW mandates the `expression.Map` rail; `graph.md:14,:18` brands frozendict a deleted form).
- `from builtins import frozendict` — store (12), field (5), ragged (7), virtual (6), lakehouse (11), columnar (4), query (4), catalog (3), + partial geospatial/mesh. `from builtins import frozendict` raises `ImportError` (no CPython `builtins.frozendict`; PEP 416 rejected) unless a forbidden shim injects it.
The split is entrenched and self-justifying in OPPOSITE directions: `virtual.md:18`, `ragged.md:16,:366` cite `Final[frozendict[...]]` as "the sibling `gridded/store#STORE` convention"; `graph.md` cites `Map.of_seq` as the convention and frozendict as the deleted form. geospatial/mesh mix BOTH within one page. No page owns the folder-level rail choice.

### C3 — Non-canonical content-key byte sources
`field.md:600` keys off `repr(var.data.manifest.dict()).encode()`; `graph.md:18` forbids `repr(dict)` byte streams; `virtual.md:209-210` keys off `snapshot.encode()`. Inconsistent + `repr()` is non-reproducible cross-runtime, undercutting the count-prefix canonical-bytes law (`RASM-COMPONENT-PARADIGM-DECISION.md:63`: content keys over PropertySet/QuantitySet-style collections need self-delimiting count-prefixed canonical bytes; PY_WIRE_ALIGNMENT builds against it). Data correctly DELEGATES the count-prefix fold to runtime `ContentIdentity.of` (Merkle over `tuple[ContentKey,...]`), but the INPUT bytes must be canonical, not `repr()`.

### C4 — Unmined upstream capability (named opportunities from runtime brief)
Runtime `[04]-[PACKAGE_PRESSURE]` explicitly delegates to the DATA folder to mine:
- `obstore` WRITE/conditional/`PutMode` surfaces (runtime keeps only the read-slice — brief `:177`). Data egress owns write.
- `fsspec` transaction/`cat_ranges`/block-cache/`FSMap`/`rsync` (brief `:181`, "stay the data folder's to mine"). Verify corpus-b `egress.md`/`columnar.md` mine these; a gap is unmined admitted capability.
Consumer surfaces data correctly composes (confirm at seam, never re-mint): `ContentIdentity`/`ContentKey` (all pages), `ReceiptContributor` (all receipts contribute via it), `TransportResource`/`ResourceRef` (DSN/credential owner — impact EC3/openLCA, columnar/lakehouse/egress remote), `LanePolicy.cached` elision (REUSE_WIRE consumer idea). The `evidence/reproduction` `ParityReceipt` (runtime `[V7]`) is where data's cross-runtime content-key parity should be proven.

### C5 — Governance freshness
Stale open cards: GRAPH_DEEPEN + GRAPH_COMMUNITY `[QUEUED]` yet realized in graph.md. Missing cards: impact has none while impact.md ships stubs. TASKLOG `INTEROP_STREAM` (interop `ArrowCStream.of` promotion) is a live blocker for the already-composed ragged `c_stream`.

### C6 — Band-gating pattern (consistent, correctly flagged)
Companion `<3.15` gates: impact whole cluster (`impact.md:3`), field flox arm (`field.md:3`), icechunk (`virtual.md`/`field.md`). Heavy scientific capability is cp315-gated companion-only until wheels ship — a real capability boundary the folder handles consistently via function-local import + `policy.vectorizable`/band probes.

### C7 — Cross-page compose coupling (mostly correct, one unwired)
`virtual.py → field.py` (FieldVirtual) and `ragged.py → tabular/interop` (ArrowCStream.of) are same-language intra-data seams. field←virtual is the split-concern coupling (C at field verdict). ragged→interop is unwired until INTEROP_STREAM promotes the classmethod.

---

## VERDICT CANDIDATES (campaign-defining, evidence-first)

VC1 — RENAME identity import folder-wide. `rasm.runtime.content_identity` → `rasm.runtime.identity` on all 15 pages (22 occ). Evidence: runtime `[V4]` ruling; grep confirms uniform stale spelling. Import-breaking the moment runtime lands; the single highest-confidence migration pressure.

VC2 — UNIFY the dispatch-table rail on `expression.Map.of_seq`; delete every `from builtins import frozendict`. Evidence: non-importable (no CPython builtin); `graph.md:14,:18` deleted-form ruling; runtime GENERATOR_LAW `expression.Map` mandate; the split is entrenched with `virtual.md:18`/`ragged.md:16,:366` citing frozendict as canon vs graph citing Map. Two camps citing "the sibling" for opposite rails is the definition of an unresolved folder-level ruling.

VC3 — impact.md is a STUB, not a design. Evidence: `impact.md:195,:202,:209` `...` bodies (3 of 4 `_normalize` arms); `:178` `unit=""`; TransportResource claimed-not-wired; zero governance cards. Either realize the four provider folds + author cards, or the impact plane's Rasm.Materials/Rasm.Compute/Rasm.Persistence seams are illusory.

VC4 — field.md over-stuffed; virtual-cube concern split across field.md#VIRTUAL and gridded/virtual.md. Evidence: 745 LOC / 4 sections / 9+ types; `virtual.md:28` imports `FieldVirtual`. Rule the ownership: merge the manifest sub-domain (FieldVirtual/VirtualParser/ManifestExport/CFDtype) into gridded/virtual.md so manifest construction co-locates with icechunk registration and the cross-page import dissolves; field.py narrows to CF read/select/reduce/egress.

VC5 — Canonical content-key bytes at every Persistence-bound key. Evidence: `field.md:600` `repr(dict)` vs `graph.md:18` prohibition vs `virtual.md:209` canonical bytes; count-prefix law `RASM-COMPONENT-PARADIGM-DECISION.md:63`. Rule one canonical manifest-wire byte source; keep the count-prefix fold in runtime `ContentIdentity.of` (data feeds canonical bytes, never re-mints).

VC6 — GPL igraph boundary. Evidence: `graph.md:27` unconditional module-top `import igraph` vs `:3,:18,:467` "confined to the community split." An unconditional import makes GPL transitive for every `rasm.data.graph` consumer. Rule the distribution model: lazy/optional-extra igraph, or accept the whole graph module as the GPL boundary and state it honestly.

VC7 — Parameterize graph edge weights. Evidence: hardcoded `weight_fn=float`/unit lambdas `graph.md:329-385`; prose `:12` claims carried path-policy/weight-selector the case payloads (`:96-100,:113`) lack. A `WeightSelector` policy value on the weighted cases closes the prose-vs-fence gap.

VC8 — Reconcile governance to realized fences. Evidence: GRAPH_DEEPEN/GRAPH_COMMUNITY `[QUEUED]` but realized; impact cards missing; INTEROP_STREAM blocks ragged's already-composed `c_stream`. Close realized cards, author impact cards, sequence INTEROP_STREAM ahead of ragged's promotion dependency.

VC9 — Mine runtime-delegated write surfaces in data. Evidence: runtime brief `:177,:181` delegates obstore WRITE/PutMode + fsspec transaction/cat_ranges/FSMap/rsync to data. Confirm corpus-b egress/columnar mine them; a gap is unmined admitted capability the upstream explicitly handed over.
