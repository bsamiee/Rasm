# SYNTHESIS — blueprint of record (RASM-PY-ARTIFACTS)

BASE: `draft-entry-seam-first.md` — the consensus winner (contract-reach + partition-elegance; sole
gate_acyclic passer beside package-mining-first on the law-gates read). Grafts applied, each named by two
or more judges or disk-verified as a single-judge survivor:

- **G1 (J1+J2+J3)** — ONE constructing owner: NEW `core/issue.md`, polymorphic `issue(IssueRequest)` over
  the closed `sheet_set | diagram_suite | document_package | single` union, WAVED TO LEG 3 (top of the
  dependency cone, importing terminal producers downward). Replaces the base's three distributed
  constructor fences (sheet/emit/transmittal — those pages stay PRODUCERS).
- **G2 (J1+J2+J3)** — KEY-OVER-INPUT: `ArtifactWork.key` mints over the producer's canonical INPUT spec
  ⊕ parent keys, pre-run, so keyed admission is a pre-production cache probe; the OUTPUT content-address
  is a separate evidence fact (Gate #3), never the elision key. Disk proof: `font.md:272-274` mints over
  produced bytes today — the rejected order.
- **G3 (J2+J3)** — fifth inversion by EDGE REMOVAL: `scene/export`'s `zlib_ng` reproducible-ZIP clone
  (`export.md:36,65,230`) DIES with no replacement import; `package/` stays fully cohesive in leg 3;
  scene bundling is `package/archive`'s emit over scene-file parent keys (a work-graph DATA edge).
- **G4 (J2+J3)** — the package→owner operator-depth binding map and the `.api`-obligations-authored-WITH-
  the-verdict table (package-mining [A]/[F]) grafted onto the roster sections.
- **G5 (J2+J3)** — `ConformanceVerdict` RE-HOMED INTO `core/receipt` (inversion a): the `Verdict` case
  already stores the typed value (`receipt.md:84`), so owning the shape makes the dependency internal and
  `exchange/conformance` imports it DOWN (w3→w1). The base's verdict→native-band collapse is REJECTED —
  it drops the typed verdict `document/tagged` consumes and its own aside forced a w2→w3 read (the J1
  wart); the re-home preserves the case signature unchanged and kills the wart.
- **G6 (J1+J3)** — regime BIND page: `drawing/standard` SPLITS into `drawing/regime.md` (NEW, leg 1a,
  BIND — the vocabulary + bind rows, no ezdxf) and `drawing/standard.md` (REBUILD, leg 3 — the ezdxf
  symbol-table lowering composing regime rows). Replaces the base's whole-page re-wave; the brief's own
  wording re-homes "that vocabulary slice", and the split is the crispest OWNER_BOUNDARIES tier reading.
- **G7 (J3, base-compatible)** — `LayerPlan` RE-SITED to `graphic/layer.md` (NEW, leg 1a); `export/layered`
  stays ONE page (REBUILD, leg 3) that composes the tree. Replaces the base's `export/layered/{plan,write}`
  split — same acyclicity, less folder fragmentation, and the disk fan is NINE importers, not six.
- **G8 (J1+J2)** — the three flagship construction chains carried as the acceptance derivation (§L),
  re-pointed at `core/issue`.
- **G9 (J3, disk-verified)** — the `Admission` selection rule stated as law (§A.1); the cases exist on
  disk (`plan.md:51-55`).
- **G10 (J1, disk-verified)** — NEW `ArtifactReceipt.Color` case RULED IN: the roster is 22 cases with no
  color-deliverable terminal (`receipt.md:73-94`), while V4 lands LUT authoring, plate authoring, and the
  TAC gate on `managed` — a `.icc`/`.cube`/swatch-book/plate-set IS an artifact whose facts a `Preview`
  band would flatten. Roster fixes at 23 in leg 1b.
- **G11 (J1)** — the color split arity RULED EXPLICITLY at 2 pages (§E V4): measurement stays a `derive`
  owner block, CxF3 intake sites in `managed` beside the `SpotChannel` plate lane it feeds. The 3-page
  (`measure`)/4-page (`measure`+`exchange`) alternatives are REJECTED as thin single-consumer shells
  (COLLAPSE_SCAN 06/07); capability is preserved whole — the 28-member `Metric` family and the
  `read_cxf`/`write_cxf` surface survive intact inside the two deep owners.

Everything below is the merged blueprint in full; where a section is unchanged from the base it is
restated, not referenced, so this file stands alone.

Disk truth carried: `core/plan.md` `ArtifactWork(key, work, parents, admission, cost)` is the target node
UNCHANGED; `Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]`; `ArtifactPipeline.of` normalizes
lone/iterable at `:449`. E13 census EXACT on disk: 69 `content_identity` / 45 full paths / 59
`from builtins import frozendict` / 54 `[RESEARCH]` / 43 `CapacityLimiter(` mints / 50 bare `stamina.`
refs. `from builtins import frozendict` SUCCEEDS on the target interpreter (PEP 814, py3.15) — the
register's "every import raises" premise is REFUTED; the V16b action stands on SHARED-TIER `Map`-spine
consistency, not as a bug fix (§K).

---

## [A] THE `ArtifactWork` ENTRY CONTRACT AS LAW (V6)

### [A.1] The one producer contract + the key law + the admission law

`core/plan`'s `ArtifactWork` node is authoritative and UNCHANGED in shape. The SEAM_AND_ENTRY_LAW
`emit() -> ArtifactWork | Iterable[ArtifactWork]` is the binding producer signature. Every producer page
lands exactly this pair:

```python
# NEW uniform entry on every producer — returns the NODE(s), never a receipt, never a bare key, never a batch.
def emit(self, /) -> ArtifactWork | Iterable[ArtifactWork]:
    return ArtifactWork(
        key=self._key,          # ContentIdentity.of(canonical(spec) ⊕ parent_keys, policy=CANONICAL_POLICY) — PRE-RUN
        work=self._emit,        # the RENAMED render thunk — Work[ArtifactReceipt]; the plan never invokes it
        parents=self._parents,  # upstream ContentKeys (bound-figure keys, member keys on a composite)
        admission=Admission(keyed=None),
        cost=self._cost,        # render-time weight the CPM forward pass sums
    )

async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:  # the producer's existing render, renamed private
    ...  # threads self._key into the terminal receipt so receipt.slot == node.key; a failed production folds to the rail fault
```

- **KEY-OVER-INPUT (G2, law).** `ArtifactWork.key` mints over the producer's frozen request/spec owner's
  canonical bytes ⊕ its parent keys — deterministic and computable BEFORE `work` runs — so `core/plan`'s
  `keyed` admission probes the threaded `cache_seed` and the runtime lane short-circuits BEFORE
  production. Today every producer mints over produced OUTPUT bytes (`font.engineer` over `_render`
  payload `font.md:274`; `layered._emit` over `LayerFact.data`), so a key exists only after work runs and
  elision is impossible. `_emit` threads the same key into its terminal receipt (`receipt.slot ==
  node.key` — the hit/miss match). A separate output content-address rides an evidence fact only where
  the store needs it (Gate #3, C#-gated); it is never the elision key.
- **Admission selection rule (G9, law).** `keyed` is the default (cache-eligible, the elision spine);
  `bare` is the forced-live non-addressable one-shot; `retried(RetryClass.OCCT)` is reserved for a
  whole-producer transient (a native render/press/raster crossing a worker that can die). The producer
  picks its case; `ArtifactWork.lowered` matches onto the lane's `Admit` arm. A `keyed` node with an
  internal offload-retry keeps `keyed` and rides `LanePolicy.offload(retry=...)` inside `_emit` (per
  `core/plan [ADMISSION_LOWERING]`).
- `_emit` is a bound zero-arg coroutine — it satisfies `Work[ArtifactReceipt]` directly, no lambda
  wrapper. The receipt carries its own key (`ArtifactReceipt.slot`), so `emit` never returns a
  `(key, receipt)` pair. Batch producers return `Iterable[ArtifactWork]` — one node per member with
  per-member keys (MODAL_ARITY: the batch is the iterable arity of `emit`; per-member elision stays
  per-member). A composite is ONE aggregate node whose `parents` are its member keys and whose receipt
  case carries the aggregate facts; receipts are terminal — incrementality is node granularity, never a
  partial receipt.

### [A.2] The 11-shape → 1-contract collapse (43 producers, disk-verified entries)

The leg-1b rewire checklist. The `_emit` thunk column is the RENAMED private method; the receipt case is
the in-band `ArtifactReceipt` case its rail resolves to.

| Convention (brief's 6) | Current shape (disk) | Pages | Collapse |
|---|---|---|---|
| in-band receipt (converged half) | `RuntimeRail[ArtifactReceipt]` | `export/layered:256`, `indesign:302`, `color/managed:255`, `chart/export:247`, `table:664`, `register:666`, `transmittal.emit:321` | rename current `emit`→`_emit`; add `emit()->ArtifactWork` with the pre-run input key (G2). The module-level batch driver was already killed (E1 proof); this lands the node builder over the surviving per-instance thunk. |
| bare key + weave | `RuntimeRail[ContentKey]` | `compose:387`, `imposition:509`, `sheet:531`, `egress:498`, `lens:249`, `tagged:258`, `dxf:1067`, `section:626`, `shape:265`, `layout:153` | `_emit` returns `RuntimeRail[ArtifactReceipt]` — the receipt whose `.slot` IS the key returned bare today. `shape`/`layout` mint `Document`; `egress`/`tagged` `Egress`; `lens` `Introspection`; `section` `Spec`; `dxf` `Cad`; `compose` `Preview`; `imposition` `Egress`; `sheet` `Egress`/`Pdf`/`Preview`. |
| key+receipt tuple | `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` | `scene/render:431`, `font.engineer:263` | drop the redundant leading key (receipt.slot carries it); `_emit -> RuntimeRail[ArtifactReceipt]` (`Scene`/`Document`); the key RE-MINTS over the INPUT spec (G2 — `font.md:274` is the key-over-output proof case). |
| key+Evidence/Verdict tuple | `RuntimeRail[tuple[ContentKey, X]]` | `conformance.close:598` (Verdict), `credential.close:522` (CredentialEvidence), `metadata.of:305` (MetaFacts,bytes), `transmittal.close:244` (TransmittalEvidence) | `_emit -> RuntimeRail[ArtifactReceipt]` minting the case that carries X's facts (`Verdict` typed via the re-homed shape / `Credential` / `Metadata` / `Transmittal`); `close`/`of` collapse into `_emit`; the caller reads facts off the receipt. |
| layers+receipt / bytes+receipt / artifact+receipt | `RuntimeRail[tuple[tuple[Layer,...] \| bytes \| DrawArtifact, ArtifactReceipt]]` | `annotate:167`, `detail:300`, `dimension:212`, `symbol:157` (layers); `schedule:196` (bytes); `draw:97` (DrawArtifact) | the leading payload projects INTO `graphic/layer.LayerPlan` (G7 — drawing/diagram producers project layer trees, never return tuples); `_emit -> RuntimeRail[ArtifactReceipt]` (`Drawing`/`Schedule`/`Diagram`). |
| Result-wrapped media pair | `RuntimeRail[Result[tuple[ContentKey, ArtifactReceipt], MediaFault]]` | `container.encode:235`, `timeline.render:100`, `subtitle.produce:163`, `analysis.produce:154`, `synthesis.produce:163` | the inner `Result[..., MediaFault]` DIES — `Work[ArtifactReceipt]` forbids it; `MediaFault` folds into the `RuntimeRail` fault at `async_boundary`. `_emit -> RuntimeRail[ArtifactReceipt]` (`Media`). |
| Block-wrapped batch | `RuntimeRail[Block[Result[ArtifactReceipt, Fault]]]` | `marks/encode.of:525`, `raster/io.of:447` | `emit -> Iterable[ArtifactWork]` — one node per member; each member's `_emit -> RuntimeRail[ArtifactReceipt]`, per-member `Fault` on its own rail. The `Block[Result]` batch DIES. |
| module-level batch rail | `RuntimeRail[Block[ContentKey]]` | `emit.produced:538`, `report.rendered:809` | `emit -> Iterable[ArtifactWork]` per producer; the module batch drivers DIE; pipeline CONSTRUCTION moves to `core/issue` (§A.3). |
| parallel receipt rail (V4 violation) | `RuntimeRail[ColorReceipt]` | `color/derive.derive:473` | DELETED — `derive` is a substrate (returns color values, no receipt); `ColorReceipt`/`ColorReceiptWire` (`derive:432,458`) removed, not collapsed-as-a-case. |
| substrate (correct) | `RuntimeRail[Block[VectorResult]]` | `vector.of:771` | unchanged in kind — `vector/*` mints no receipt; split into `graphic/vector/{path,region,pattern}` (V1/V2), all NONE-entry. |

Six legacy carriers collapsed: the batch driver (`produced`/`rendered`/`_fanned`), the `Block[Result]`
batch, the `tuple[ContentKey, X]` pairs, the `layers/bytes/artifact + receipt` tuples, the media `Result`
wrapper, and the `ColorReceipt` rail. Every survivor is `emit() -> ArtifactWork | Iterable[ArtifactWork]`.

### [A.3] THE constructing owner — NEW `core/issue.md` (G1), and `ArtifactPipeline` becomes REAL

`core/plan` is the ENGINE, never the constructor. ONE NEW page owns pipeline construction for every
modality — the arity law made a single entry — sited at LEG 3, the top of the dependency cone, importing
terminal producers downward (the output-first draft's leg-1b siting was the disqualifying mis-wave; this
placement is acyclic):

```python
# core/issue.md skeleton — the SOLE constructing owner (wave 3 / leg 3; nothing imports it)
@tagged_union(frozen=True)
class IssueRequest:  # the flagship discriminant — one closed vocabulary, one arity owns all modalities
    tag: Literal["sheet_set", "diagram_suite", "document_package", "single"] = tag()
    sheet_set: "TransmittalRecord" = case()            # terminal = delivery/transmittal (aggregate + members)
    diagram_suite: tuple["DiagramLayout", ...] = case()  # terminal = visualization/diagram/draw, one node per kind
    document_package: tuple["SpecSection", ...] = case() # terminal chain = section -> emit -> egress
    single: ArtifactWork = case()                      # any lone producer node

@dataclass(frozen=True, slots=True, kw_only=True)
class ArtifactIssue:
    lane: LanePolicy
    async def issue(self, request: IssueRequest, /) -> RuntimeRail[Block[ArtifactReceipt]]:
        nodes, targets = self._nodes(request)          # total match: terminal.emit() -> Iterable[ArtifactWork]
        plan = ArtifactPipeline.of(nodes, targets=targets).plan()   # core/plan schedules (CPM fronts)
        return await self.lane.driven(plan.fronts, plan.cache_seed) # runtime LanePolicy drains fronts
```

- `issue()` is the composition root, not a producer: it calls the terminal producers' `emit()`, folds the
  nodes into `ArtifactPipeline.of(...)`, `.plan()`s, drives the fronts through the runtime `LanePolicy`,
  and returns the terminal receipts. Its imports all point downward: `delivery/transmittal` (w3,
  within-wave), `composition/sheet`/`document/emit`/`document/egress`/`visualization/diagram/draw` (w2),
  `core/plan` (w1), the runtime lane. NOTHING in artifacts imports `core/issue` — external hosts and
  sibling packages invoke it.
- The base's three distributed constructor fences are SUPERSEDED: `composition/sheet.SheetSet`,
  `document/emit.DocumentPackage`, and `delivery/transmittal` stay PRODUCERS — `SheetSet.emit()` returns
  one node per sheet (cover, drawing-index, general-notes, drawing sheets) plus nothing aggregate;
  `transmittal.emit()` returns the member node set plus the ONE aggregate `Transmittal` node whose
  `parents` are the member keys; `document/emit` binds one `DocumentNode` context to N format nodes
  (absorbing `core/format`'s `TemplatePayload` admission and `bound()` fan-out as emit rows). The
  diagram-SUITE flagship — the constructor gap every distributed-constructor draft left open — is
  `issue(IssueRequest(diagram_suite=...))` over `draw.emit()` per kind.
- `core/plan`'s `[CONTENT_KEY_NODES]` producer list refreshes to name `core/issue` as THE constructing
  owner; the plan imports no producer (the `Work` thunk carries the call), so no producer→plan→producer
  cycle forms. `core/` spans waves (receipt+plan w1/leg 1b, issue w3/leg 3) — legal, waves are dependency
  barriers, not folder groupings; leg 1 still finishes receipt+plan and later legs never re-open them.

### [A.4] The limiter/stamina collapse rides the entry rewire (leg-1 corpus-wide)

The 43 `CapacityLimiter(` mints + 50 bare `stamina.` refs are the SAME leg-1b reconcile as the `emit()`
rewire, not a 3-4 site patch. Every folder-minted `CapacityLimiter`/`stamina.AsyncRetryingCaller` dies;
each `_emit` thunk's native/subprocess seam rides `lanes.offload(kernel, retry=RetryClass.OCCT)` under the
runtime-owned bound (the ORACLE row for exchange/conformance oracle verdicts; retry ONLY through runtime
`guarded` over the one `POLICY` table). The media plane's `_WORKER_RETRY` (`container:3`, 6 pages) and
`detect:11` fold onto the same export.

---

## [B] THE CORRECTED SEAM LEDGER (acyclic; 4+1 inversions disposed; 3 cycles broken)

The leg partition is a topological order over this ledger. Every cross-domain import points within-wave or
to an earlier wave. Ubiquitous spine edges are stated once: EVERY producer imports `core/plan`
(`ArtifactWork`) and `core/receipt` (its case mint) downward — elided from the per-page rows below.

### [B.1] The four named inversions + the fifth found — each DISPOSED

- **(a) `core/receipt:33` ← `exchange/conformance.ConformanceVerdict`** (w1 composing w3). RULING (G5):
  **RE-HOME `ConformanceVerdict` INTO `core/receipt`.** The `Verdict` case already stores the typed value
  (`receipt.md:84`); owning the shape makes the dependency internal, the case signature is UNCHANGED, and
  `exchange/conformance` (w3) imports the shape DOWN and MINTS it. `document/tagged` (w2) consumes the
  typed verdict off `ArtifactReceipt.Verdict` via `core/receipt` (w1) — no w2→w3 read exists anywhere.
  The base's verdict→native-band collapse is REJECTED: it strips the typed verdict from the receipt seam
  its own consumers need and forced the contradictory w2→w3 aside.
- **(b) `composition/sheet:48` ← `drawing/standard` (`ScaleRatio`/`SheetId` + V3 lettering + V13 pens)**.
  RULING (G6): **SPLIT `drawing/standard`.** The vocabulary + BIND slice — `Discipline`/`ScaleRatio`/
  `SheetId`/`Terminator`/`LineWeight`/`TextHeight`/`LayerName`/`LineType`/`HatchMaterial`/
  `LetteringStyle`, the AIA/ISO 13567/NCS codecs, ISO 216/5455 derivations, and the material→pattern /
  discipline→pen/lettering BIND rows — moves to NEW `drawing/regime.md` (w1/leg 1a, tier BIND); the ezdxf
  symbol-table lowering (`seed`/`graphics`/`dimstyle`/`hatch`/`rgb`/`paper_factor`) stays
  `drawing/standard.md` (w3/leg 3) composing regime rows. `sheet` (w2), `graphic/style` (w1, SELECT),
  `graphic/layer` (w1, field vocabulary), `classify`/`register`/the drawing producers (w3) all import
  `regime` downward or within-wave; `export/dxf` (w3) composes `standard`'s seed seam within-wave.
- **(c) the NINE-page `export/layered.Layer` upward fan** (disk: compose, imposition, sheet, annotate,
  detail, dimension, symbol, marks/encode, diagram/draw). RULING (G7): **RE-SITE `LayerPlan` to NEW
  `graphic/layer.md`** (w1/leg 1a) — the semantic layer TREE (bounded top-level roster,
  discipline/kind/z-order rows, ISO 13567-aligned names on AEC output, stable editorial names elsewhere,
  meaning-nested groups). Every projector imports `layer` downward; `export/layered` (w3) stays ONE page
  that COMPOSES the tree, never owns it. The upward fan becomes nine downward edges.
- **(d) `graphic/raster/io:49` ← `exchange/detect`** (w1 composing w3). RULING: **RE-WAVE
  `exchange/detect` to w1/leg 1a.** Detect is the ingest-boundary format-ID substrate (leaf
  puremagic/python-magic + runtime lanes, mints no receipt); `io` imports it within-wave; the
  document/raster/Office consumers import it downward. `exchange/` splits across waves (detect w1;
  conformance/credential/metadata w3) — legal.
- **(e) NEW — `scene/export` → the package compression plane** (w2 composing w3; the `zlib_ng` repro-ZIP
  clone at `export.md:36,65,230` byte-couples to `package/archive#ARCHIVE`). RULING (G3): **REMOVE THE
  EDGE.** The clone DIES; `scene/export` produces scene FILES; the reproducible ZIP is the package
  plane's (leg 3) — a scene bundle is `package/archive`'s emit whose `parents` are the scene-file content
  keys (a work-graph DATA edge, never an import); USDZ stays `scene/stage`'s own usd-core packaging.
  `package/bundle` therefore stays LEG 3 with its folder (the base's w2 re-wave is superseded), and the
  zlib-ng re-entry consumer is `package/codec`'s live GZIP band (§D.2).

### [B.2] The three cycles — each broken at a neutral shared-vocabulary site

- **marks `encode`⇄`decode`** (`encode:55,60`⇄`decode:33`). NEW `graphic/marks/mark.md` (w1) owns
  `Symbology`/`DecodeSource`/`MarkFault`/`MarkPayload`; encode and decode both import `mark`, neither
  imports the other. The duplicate `RasterFact` (`io:321` + `encode:258`) consolidates to
  `graphic/raster/io`'s canonical.
- **scene `render`⇄`export`** (`render:34-38` lazy ↔ export imports render). Hoist `SceneTarget` into
  `scene/render` (render owns the target vocabulary its `SceneOp.Export` keys); `export` composes
  render's `SceneTarget` downward; render no longer imports export. Broken in leg 2.
- **package `codec`⇄`archive`⇄`delta`** (`codec:37-38` eager private-worker reach ↔ `archive:92`/
  `delta:30` lazy-back). NEW `package/bundle.md` (w3) owns `Bundle`/`CompressionAlgo`/`CodecProfile`/
  `BundleManifest` + the `PackWorker` port; codec/archive/delta each compose `bundle` downward, the eager
  private-worker imports die. Lands leg 3 with the two carried growth realizations.

### [B.3] The corrected ledger — every cross-domain edge, direction = IMPORT direction

Format: `consumer → owner  # [TAG] anchor` (`→` = consumer imports owner). All edges point within-wave or
earlier. Cross-branch data edges (`data/tabular`, `compute/*`, `geometry/mesh`, runtime) are ingress, not
artifacts imports, and carry no wave constraint.

```
# --- WAVE 1 internal (foundations) ---
graphic/color/managed         →  graphic/color/derive          # [ADAPT] AdaptMethod owned by derive, managed composes (E6)
graphic/color/managed         →  graphic/color/derive          # [DERIVE] tone-curve/space provenance (ARCH:105, now real)
graphic/vector/region         →  graphic/vector/path           # boolean/offset over the path algebra
graphic/vector/pattern        →  graphic/vector/path           # [OWNER_BOUNDARIES] pattern composes vector, not marks (V2)
graphic/vector/pattern        →  graphic/vector/region         # pathops clip lowering for real clipped hatch geometry
graphic/marks/encode          →  graphic/marks/mark            # NEW neutral vocab (cycle break)
graphic/marks/decode          →  graphic/marks/mark            # NEW neutral vocab (cycle break)
graphic/marks/encode          →  graphic/raster/io             # RasterFact consolidated to io's canonical (E6 dup)
graphic/marks/encode          →  graphic/vector/path           # [GEOMETRY] mark geometry over vector (tier: generate)
graphic/marks/encode          →  graphic/layer                 # [LAYERED] mark rows project into LayerPlan (V14 — inversion c, fan edge 8/9)
graphic/raster/io             →  exchange/detect               # [DETECT] detect RE-WAVED to w1 — inversion (d) DISPOSED
graphic/raster/process        →  graphic/raster/io             # shared TransformInput substrate
graphic/raster/measure        →  graphic/raster/process        # produced-vs-measured axis
typography/shape              →  typography/font               # [SHAPE] face/variation + embed-audit precondition (ARCH:152)
typography/layout             →  typography/shape              # reads break-safety/tatweel flags
typography/math               →  typography/shape              # NEW ziamath owner composes shape's outline (V3)
typography/math               →  typography/font               # NEW math-font metrics
drawing/regime                →  graphic/color/derive          # [DERIVE] discipline pens resolved by value (V4/V13, tier bind)
drawing/regime                →  graphic/vector/pattern        # [HATCH] HatchMaterial -> PatternSpec bind rows (V2)
drawing/regime                →  typography/font               # [METRIC] lettering cap/x-height via font-metrics owner (V3, kills fontTools leak)
graphic/layer                 →  drawing/regime                # NEW LayerPlan reads ISO 13567 layer-field vocabulary (V14)
graphic/style                 →  graphic/color/derive          # NEW V13 — color system resolved through derive
graphic/style                 →  drawing/regime                # NEW V13 — SELECTS regime pen/lettering rows (tier select over bind)
graphic/style                 →  typography/shape              # NEW V13 — feature presets over FeatureSpec
graphic/style                 →  typography/layout             # NEW V13 — leading/tracking over break correctness
graphic/style                 →  typography/font               # NEW V13 — variable-axis INSTANCE/FREEZE per role
core/plan                     →  core/receipt                  # ArtifactReceipt the Work payload (UNCHANGED)
core/receipt                  →  (nothing)                     # ConformanceVerdict RE-HOMED IN — inversion (a) DISPOSED (G5)
# --- WAVE 2 internal + into WAVE 1 ---
composition/compose           →  graphic/vector/path           # [GEOMETRY] svgelements placement (ARCH:115)
composition/compose           →  graphic/layer                 # [LAYERED] projects into LayerPlan (V14 — inversion c)
composition/imposition        →  graphic/layer                 # [LAYERED] OCG groups project into LayerPlan (V14 — inversion c)
composition/sheet             →  drawing/regime                # [SCALE] ScaleRatio/SheetId (regime w1 — inversion b DISPOSED)
composition/sheet             →  graphic/layer                 # [LAYERED] frame/figures as LayerPlan rows (V14 — inversion c)
composition/sheet             →  graphic/vector/path           # hand-built affines routed through vector (V7)
composition/sheet             →  graphic/style                 # [THEME] title-block grid VARIANTS = sheet-family rows (V13)
composition/sheet             →  typography/shape              # [SHAPE] lettering() + shaped runs, kills Helvetica magic (V3/E8)
composition/sheet             →  typography/layout             # general-notes line break
composition/sheet             →  visualization/table           # [TABLE] SheetSet.scheduled TablePlan (ARCH:159)
composition/sheet             →  document/emit                 # [NOTES] sheet<-emit general-notes typesetting seam (V7, was prose)
composition/sheet             →  composition/imposition        # [IMPOSE] composes the ONE press fold (V7/E6)
visualization/table           →  graphic/style                 # [THEME] table style rows (V13)
visualization/chart/spec      →  graphic/color/derive          # [PALETTE] Palette/hex_ramp REHOMED from spec into derive (V4/E2)
visualization/chart/spec      →  graphic/style                 # [THEME] chart theme rows bind rcParams/ThemeConfig (V13)
visualization/chart/export    →  graphic/color/derive          # rewired from spec-alias to derive (V4/E2)
visualization/chart/export    →  visualization/chart/spec      # folds the chart case (transform MERGED in — V10)
visualization/diagram/draw    →  graphic/color/derive          # [INK] _INK becomes derive contrast pick (V4/E8)
visualization/diagram/draw    →  typography/math               # NEW — formula through math owner, not local ziamath (V3/E3)
visualization/diagram/draw    →  typography/shape              # NEW — label outlining through shape, not local ziafont (V3/E3)
visualization/diagram/draw    →  graphic/layer                 # [LAYERED] named-layer projection into LayerPlan (V14 — inversion c)
visualization/diagram/draw    →  graphic/style                 # [THEME] diagram aesthetics rows (V13)
visualization/diagram/layout  →  visualization/diagram/glyphset # positioned marks (adds area/polygon case — V15)
visualization/diagram/layout  →  visualization/diagram/solar   # NEW — sun-path furniture geometry (V15)
visualization/diagram/schematic → visualization/diagram/glyphset # NEW schemdraw catalog beside the five marks (V10)
visualization/diagram/solar   →  graphic/vector/path           # NEW — sun-path arc geometry over vector (V15, tier generate)
scene/export                  →  scene/render                  # SceneTarget hoisted to render — cycle DISPOSED
# scene/export -> package/*: NO EDGE — inversion (e) DISPOSED by edge removal (G3); scene bundling is a
# package/archive DATA edge (parents = scene-file keys); the zlib_ng clone at export.md:36,230 DIES.
document/emit                 →  document/model                # lowers FROM the tree (ARCH:13)
document/emit                 →  typography/shape              # [SHAPE] E7 seam now COMPOSED (V3/E7, was zero arms)
document/emit                 →  typography/layout             # [LAYOUT] LineBrokenRun + pyphen orthographic emission (V3/V12)
document/emit                 →  typography/font               # [FONT] FONT_EMBED subset/instance (ARCH:151)
document/emit                 →  graphic/color/managed         # [MANAGED] ICC-profiled raster (ARCH:107)
document/emit                 →  graphic/style                 # [THEME] page-master rows + running heads (V13)
document/emit                 →  graphic/layer                 # OCG layer tree for layered PDF (V14)
document/report               →  document/emit                 # terminal PDF routes through emit — Pdf.from_html dup dies (V12/E6)
document/report               →  document/model                # composes into the tree
document/tagged               →  document/model                # StructureNode tree (ARCH:178)
document/tagged               →  core/receipt                  # typed ConformanceVerdict payload read off ArtifactReceipt.Verdict (w2->w1, G5)
document/lens                 →  document/model                # recovers TO the tree
document/lens                 →  exchange/detect               # format-ID pre-flight (detect w1)
document/egress               →  composition/imposition        # egress composes imposition's ONE press fold (V7/E6)
# --- WAVE 3 internal + into WAVE 1/2 ---
core/issue                    →  delivery/transmittal          # sheet_set modality — terminal aggregate producer (G1, within-wave)
core/issue                    →  composition/sheet             # SheetSet member nodes (w3<-w2)
core/issue                    →  document/emit                 # document_package format nodes (w3<-w2)
core/issue                    →  document/egress               # document_package finishing terminal (w3<-w2)
core/issue                    →  visualization/diagram/draw    # diagram_suite per-kind nodes (w3<-w2)
core/issue                    →  core/plan                     # ArtifactPipeline.of construction (w3<-w1)
drawing/standard              →  drawing/regime                # ezdxf lowering composes the BIND rows (G6, w3<-w1)
drawing/standard              →  graphic/vector/pattern        # ezdxf pattern-line definitions lowering (V2)
drawing/dimension             →  drawing/regime                # [DIM] TextHeight-ratio DIM policy (E8, w3<-w1)
drawing/dimension             →  drawing/standard              # [DIMSTYLE] ezdxf dimstyle lowering (within-wave)
drawing/dimension             →  drawing/symbol                # V5 mark geometry owner (terminator/proportion rows)
drawing/dimension             →  typography/math               # ziamath tolerance math via owner (V3/E3)
drawing/dimension             →  typography/shape              # ISO 3098 shaped run (V3/E3, kills local ziafont)
drawing/dimension             →  graphic/vector/path           # [VECTOR] metric point-at-distance for tick spacing (V1)
drawing/dimension             →  graphic/color/derive          # discipline pen (V4, kills literal ink)
drawing/dimension             →  graphic/layer                 # [LAYERED] dimension layer rows (V14)
drawing/annotate              →  drawing/symbol                # [STYLE] SymbolStyle + V5 terminator geometry (ARCH:133)
drawing/annotate              →  drawing/regime                # [STANDARD] vocabulary rows (ARCH:134)
drawing/annotate              →  typography/layout             # [LAYOUT] Knuth-Plass notes (ARCH:135, kept)
drawing/annotate              →  typography/shape              # [SHAPE] consumes POSITIONS not just clusters (V3/E3)
drawing/annotate              →  typography/math               # formula notes via owner (V3)
drawing/annotate              →  graphic/vector/region         # scallop-band offset (ARCH:137)
drawing/annotate              →  graphic/color/derive          # palette REWIRED spec->derive (V4/E2/E7, was unledgered :49)
drawing/annotate              →  graphic/layer                 # [LAYERED] annotation layer rows (V14)
drawing/symbol                →  graphic/vector/path           # [GEOMETRY] mark geometry (V5, replaces numpy trig)
drawing/symbol                →  graphic/vector/region         # rasterize for sheet-cell seam
drawing/symbol                →  drawing/regime                # owned ISO vocab (w3<-w1)
drawing/symbol                →  graphic/color/derive          # palette REWIRED spec->derive (V4/E2)
drawing/symbol                →  graphic/layer                 # [LAYERED] symbol layer rows (V14)
drawing/detail                →  drawing/symbol                # [BUBBLE] (ARCH:129)
drawing/detail                →  drawing/regime                # [STANDARD] SheetId-typed refs (E9, was stringly :80)
drawing/detail                →  graphic/layer                 # [LAYERED] detail layer rows (V14 — inversion c, fan edge 9/9)
drawing/detail                →  graphic/vector/path           # area-weighted centroid (V1, replaces vertex-mean)
drawing/detail                →  graphic/color/derive          # palette REWIRED spec->derive (V4/E2, resolves prose-vs-fence)
drawing/schedule              →  visualization/table           # [LOWER] TablePlan.build (ARCH:143, pandas floor lands leg2)
drawing/schedule              →  drawing/regime                # [STANDARD] legend material bind rows (ARCH:142)
drawing/schedule              →  graphic/vector/pattern        # legend swatch geometry (V2, kills crosshatch trig + invented hex)
drawing/schedule              →  graphic/color/derive          # swatch color (V4, kills invented hex :355)
specification/section         →  document/model                # [MODEL] (ARCH:146)
specification/section         →  specification/classify        # [CLASSIFY] ClassCode (ARCH:147)
specification/section         →  visualization/table           # [TABLE] QTO citation frame (ARCH:148)
specification/classify        →  drawing/regime                # [DISCIPLINE] (ARCH:150, regime w1)
delivery/register             →  composition/sheet             # [SHEET] SheetEntry from_title_block (ARCH:207, w3<-w2)
delivery/register             →  specification/classify        # ClassCode composition kills parallel ClassificationSystem (E6/E9)
delivery/register             →  drawing/regime                # Discipline composition (E9, was bare-str :352)
delivery/register             →  visualization/table           # [TABLE] Index render (ARCH:208)
delivery/transmittal          →  composition/sheet             # sheet-set member nodes (w3<-w2)
delivery/transmittal          →  composition/imposition        # [IMPOSE] (ARCH:210)
delivery/transmittal          →  package/archive               # [ARCHIVE] (ARCH:211, within-wave)
delivery/transmittal          →  package/codec                 # [CODEC] Bundle — LEDGERED (E7, was unledgered :58)
delivery/transmittal          →  exchange/conformance          # [CONFORMANCE] (ARCH:212)
delivery/transmittal          →  exchange/credential           # [CREDENTIAL] (ARCH:213)
delivery/transmittal          →  delivery/register             # [REGISTER] (ARCH:214)
export/dxf                    →  drawing/standard              # [SEED] Dxf.New composes Standard.seed (V11, within-wave)
export/dxf                    →  drawing/regime                # by-NAME version/unit vocabulary (E9, w3<-w1)
export/dxf                    →  graphic/vector/path           # [VECTOR] make_path/flatten bridge (ARCH:168, one direction)
export/layered                →  graphic/layer                 # composes the LayerPlan tree (V14 — composes, never owns)
export/layered                →  composition/compose           # [COMPOSE] placed layout (ARCH:163)
export/indesign               →  graphic/layer                 # IDML layers = LayerPlan (V14)
export/indesign               →  composition/compose           # [TEMPLATE] placed layout (ARCH:165)
exchange/conformance          →  core/receipt                  # ConformanceVerdict re-homed — imports the shape DOWN (G5, w3<-w1)
exchange/conformance          →  document/emit                 # [DOCUMENT] emitted PDF (ARCH:175, w3<-w2)
exchange/conformance          →  document/tagged               # [ACCESS] structural result (ARCH:176, w3<-w2)
exchange/conformance          →  typography/font               # embed-audit
# ARCH:179 document/tagged->exchange/conformance [SIGN] is DATA-FLOW, not an import: the import is
# conformance->tagged above (w3<-w2); no tagged->conformance edge exists — no w2->w3 inversion.
package/codec                 →  package/bundle                # NEW neutral vocab (cycle break, within-wave w3)
package/archive               →  package/bundle                # NEW neutral vocab (cycle break, within-wave w3)
package/delta                 →  package/bundle                # NEW neutral vocab (cycle break, within-wave w3)
media/* (7)                   →  media/container|filtergraph   # spine composition (ARCH:183-206, cold)
media/subtitle                →  typography/shape              # [SHAPE] shaped_rgba EXPORTED (E3, was phantom :50)
media/analysis                →  graphic/raster/io             # render_png/montage EXPORTED bare (E5, was phantom)
media/analysis                →  graphic/raster/measure        # frame_similarity EXPORTED bare (E5, was phantom)
```

Data-flow-vs-import reconciliation (carried from the base, verified): (1) ARCH:179 `document/tagged →
exchange/conformance [SIGN]` and (2) ARCH:159-160 `composition/sheet → delivery/register [REGISTER]` are
DATA-flow arrows, not imports — in both, the OWNER imports the SOURCE downward (conformance w3 imports
tagged w2; register w3 imports sheet's `SheetSet` w2 via `from_title_block`), and the producer projects
plain tuples with no upward import. The ledger's `→` is IMPORT direction throughout; ARCH's mixed arrows
are reconciled to import direction here. The whole import graph is acyclic: every edge points within-wave
or to an earlier wave; the five inversions are re-homed/re-waved/removed, the three cycles broken at
neutral sites.

---

## [C] THE FULL PAGE-SET (69 live pages: 60 current − 3 deleted + 12 new; full row schema)

Legend: Action+Lowering uses the engine vocabulary — KEEP→`improve`; REBUILD→`rebuild`; NEW→`new`;
SPLIT→N×`new` + `delete{into,from}`; MERGE→`improve`(absorber) + `delete{into,from}`; MOVE→`new`(dest) +
`delete{into,from}` (a slice-move without source deletion = `new`(dest) + `rebuild`(source), the absorb
named in both charters). Tier per `[OWNER_BOUNDARIES]` (generate/bind/select or N/A). Entry: `emit()`
(receipt case) for producers, NONE for substrate/vocabulary. Seams: in/out top anchors per row; §B.3 is
the authoritative full ledger.

### [C.1] LEG 1a — foundational OWNERS (vocabulary/substrate rebuilds; the emit rewire is 1b)

| Path | Action + lowering | Tier — charter / skeleton | Entry | Seams (in ← / out →) | Wave/leg |
|---|---|---|---|---|---|
| `graphic/vector/path.md` | NEW `new` (SPLIT of `vector.md`) | generate — svgelements parse/query/affine/measure/sample + **metric point-at-distance** (arc-length, kills parametric-t tick spacing V1), **polyline decimation/simplify**, **area-weighted centroid** (V1), **`Length.to_mm` unit egress** (V1), text-on-path. Owner `Path`; `PathOp`/`PathResult` closed families; tolerances lifted to `_TOLERANCE` policy row (E8 `0.1/0.25/ppi=96.0/1e-3`). | NONE (substrate) | in: runtime identity; out: region, pattern, symbol, dimension, detail, compose, sheet, dxf, solar | 1/1a |
| `graphic/vector/region.md` | NEW `new` + `delete{into: graphic/vector/region, from: graphic/vector.md}` | generate — skia-pathops boolean/offset/stroke/warp/wind/region/contains + serialization (`drawsvg.Drawing/Group/Raw` replaces f-string SVG V1) + resvg raster egress. Owner `Region`; policy-lifted flatten tolerances. | NONE | in: path; out: pattern, annotate, symbol | 1/1a |
| `graphic/vector/pattern.md` | NEW `new` (V2) | generate — REPEATING fill geometry over path/region: `StrokeFamily`/`PatternSpec`/`DensityLaw` (brief shape), THREE lowerings (ezdxf `set_pattern_fill`, `drawsvg.Pattern`, pathops-clipped geometry). `HatchMaterial` = seed rows binding material→`PatternSpec` per ISO 128-50/ANSI/BS regime axis. | NONE | in: path, region; out: regime (bind rows), standard (lowering), schedule, layered | 1/1a |
| `graphic/color/derive.md` | REBUILD `rebuild` | generate — colour-science + ColorAide derivation + **measurement owner block** (28-member `Metric` deltaE/contrast/CVD/WCAG kept whole — G11 rules the 2-page arity) + **rehomed `Palette`/`hex_ramp`** (MOVE in from chart/spec). Owns `AdaptMethod` (E6, managed composes). `ColorReceipt`/`ColorReceiptWire` DELETED (V4). | NONE (substrate) | in: runtime identity; out: managed, style, regime, chart/spec+export, diagram/draw, scene/render, drawing/* | 1/1a |
| `graphic/raster/process.md` | KEEP `improve` | N/A — 97-member produced-raster engine; pillow ImageChops/ImageMath/gradients ([03]). scikit-image marker-drop blocker named (V8). | NONE (substrate) | in: io; out: measure | 1/1a |
| `graphic/marks/mark.md` | NEW `new` (cycle break) | N/A (vocabulary) — shared `Symbology`/`DecodeSource`/`MarkFault`/`MarkPayload` both encode+decode import. | NONE | out: encode, decode | 1/1a |
| `graphic/style.md` | NEW `new` (V13) | select — theme-as-data owner: color system via derive, stroke hierarchies via regime pens, composition grids, per-plane bindings (chart/diagram/sheet/table); **TYPE SYSTEM** rows (type-scale ratios, per-role leading/tracking, variable-axis coords via font INSTANCE/FREEZE, OpenType feature presets via shape FeatureSpec); **SHEET FAMILY** rows (title-block grid variants, margin/zone/page-master). A theme row references regime rows by key; switching office style = switching one row set. | NONE (data) | in: derive, regime, shape, layout, font; out: sheet, emit, table, chart/spec, diagram/draw | 1/1a |
| `graphic/layer.md` | NEW `new` (V14, G7 re-site) | bind — `LayerPlan` semantic layer TREE (bounded top-level roster, discipline/kind/z-order rows, ISO 13567 names on AEC / stable editorial names elsewhere, meaning-nested groups; `BlendMode`/`LayerIntent`). The NINE disk projectors + every layered exporter project INTO it; `export/layered` composes, never owns. | NONE (vocabulary) | in: regime (ISO 13567 fields); out: compose, imposition, sheet, diagram/draw, marks/encode, drawing/*, export/layered, export/indesign, document/emit | 1/1a |
| `typography/font.md` | KEEP `improve` (receipt-law exemplar) | N/A — FontEngineering; add the font-METRICS owner surface (cap/x-height via `hb_ot_metrics`, V3 — regime stops leaking fontTools); `engineer`→`_emit` with the G2 input-spec key re-mint. | `emit()` (`Document`) | in: —; out: shape, math, regime (metrics), emit (FONT_EMBED), style | 1/1b |
| `typography/shape.md` | REBUILD `rebuild` (V3) | generate — the SOLE text engine; **exports `shaped_rgba(fragment, style) -> bytes`** (E3 subtitle phantom); fallback-chain RESOLUTION via `fontTools.unicodedata.script()`/`script_extension()` itemization; COLRv1 blackrenderer raster; SHAPE_QA golden oracle (vharfbuzz `serialize_buf`) is the consumer-rewire parity proof. | `emit()` (`Document`) | in: font; out: layout, math, emit, sheet, dimension, annotate, symbol, draw, subtitle, region (text-on-path) | 1/1b |
| `typography/layout.md` | KEEP `improve` | N/A — Knuth-Plass break correctness + vertical (ttb) stacking; carries the pyphen `(change,index,cut)` channel (emit applies it V12). | `emit()` (`Document`) | in: shape; out: emit, annotate, sheet, style | 1/1b |
| `typography/math.md` | NEW `new` (V3) | generate — ziamath owner (7 pages hand-build it) composing shape's outline + uharfbuzz `Face.has_math_data`/`get_math_constant(OTMathConstant)`/`get_math_glyph_variants`/`get_math_glyph_assembly`; ziafont/ziamath enter typography's manifest. | `emit()` (`Document`) | in: shape, font; out: dimension, annotate, draw, model | 1/1b |
| `drawing/regime.md` | NEW `new` (G6, inversion b — vocabulary slice out of `standard`; absorb {into: drawing/regime, from: drawing/standard vocabulary}) | bind — the closed AEC drafting VOCABULARY + BIND rows: `Discipline`/`ScaleRatio`/`SheetId`/`LineWeight`/`TextHeight`/`Terminator`/`LayerName`/`LineType`/`HatchMaterial`/`LetteringStyle`; AIA/ISO 13567 full field structure/NCS level-2 codecs; ISO 216/5455 derivations; material→`PatternSpec`, discipline→pen/lettering bind rows (values resolved through derive/pattern/font-metrics). NO ezdxf — the lowering stays `drawing/standard` (leg 3). | NONE (substrate) | in: derive (pens), pattern (hatch), font (metrics); out: style, layer, sheet, classify, register, drawing/*, dxf, standard | 1/1a |
| `exchange/detect.md` | KEEP `improve` + RE-WAVE to 1 (inversion d) | N/A (substrate) — puremagic-default/python-magic-fallback format-ID gate; folder-minted `_WORKER_RETRY` folds to `lanes.offload(retry=OCCT)` (leg-1 reconcile). | NONE | in: —; out: raster/io, lens, emit | 1/1a |

### [C.2] LEG 1b — SPINE + corpus-wide entry rewire (gated on 1a residual-clean)

| Path | Action + lowering | Tier — charter / skeleton | Entry | Seams (in ← / out →) | Wave/leg |
|---|---|---|---|---|---|
| `core/receipt.md` | REBUILD `rebuild` (E9 / inversion a / V16 / G5 / G10) | N/A — the ONE `ArtifactReceipt` union, roster FIXED AT 23: the 22 disk cases + NEW **`Color`** case (G10 — LUT/plate/swatch terminals: `(key, space, intent, tac_peak, plates, frozendict band)`-shaped payload finalized at the roster freeze). **`ConformanceVerdict` RE-HOMED IN** (G5 — the `Verdict` case signature unchanged; conformance imports the shape down). `ArtifactKind` Literal + hand-synced `_KEYS` become ONE owner DERIVED from the case roster (E9). Band fields STAY `frozendict` (V16b-C, msgspec-native); `_KEYS` and every dispatch table → `Map` (V16b-B). Gate #1 `contribute` metrics + the Gate #2 artifacts-origin `graduates()` producer land here (§I). | NONE (family) | in: runtime ReceiptContributor + metric recorder, compute/graduation hub (Gate #2); out: every producer, conformance (shape down), tagged (typed verdict) | 1/1b |
| `core/plan.md` | KEEP `improve` (E1/V6) | N/A — the CPM engine, verbatim; **`[CONTENT_KEY_NODES]` refreshed to name `core/issue` as THE constructing owner** (§A.3) + the corrected producer roster + the G2 key-over-input law on the node contract prose; `content_identity`→`identity`; purge `[RESEARCH]`. | NONE (engine) | in: runtime lanes (Keyed/StagePlan), receipt; out: issue (construction), every producer (node type) | 1/1b |
| `graphic/color/managed.md` | REBUILD `rebuild` | select — ICC/LUT/CCTF egress (pyvips `icc_transform` on lane) + **CxF3 spot intake** (colour-cxf `read_cxf`/`write_cxf` over `cxf3.CxF` — G11 sites exchange here beside the plate lane it feeds) + **Separation/DeviceN plate authoring** (pikepdf raw object model, consumes own `SpotChannel` decode, V4) + **TAC policy gate** (limit row + overprint/rich-black rows) + LUT authoring (`write_LUT`). Composes derive's `AdaptMethod`. | `emit()` (`Preview` + NEW `Color` for LUT/plate/swatch terminals) | in: derive; out: raster/io, emit (ICC raster) | 1/1b |
| `graphic/raster/io.md` | REBUILD `rebuild` (V8) | N/A (working surface) — `Raster` owner, 15 `RasterOp`, 139-member `Transform` vocab re-partitioned to behavior owners; **exports bare `render_png`/`montage`** (E5 media home); **pyvips `block_untrusted_set` hardening + `Source`/`Target` streaming**; canonical `RasterFact` (marks consolidates here); tifffile GeoTIFF tags/`aszarr`/`memmap`/`TiffSequence`/`validate_jhove` oracle ([04]). | `emit()` per-member (`Preview`) | in: detect, process, managed; out: measure, marks/encode (RasterFact), media/analysis | 1/1b |
| `graphic/raster/measure.md` | KEEP `improve` | N/A — 42-member measure engine; **exports bare `frame_similarity`** (E5 SSIM). | `emit()` (`Preview`) | in: process; out: media/analysis | 1/1b |
| `graphic/marks/encode.md` | REBUILD `rebuild` | generate — segno/python-barcode/zxing encode over mark geometry; imports `mark` + io's `RasterFact` (cycle + E6 dup fixed). | `emit()` per-member (`Preview`) | in: mark, io, path; out: layered (projection) | 1/1b |
| `graphic/marks/decode.md` | KEEP `improve` | N/A — zxing `read_barcodes` inverse; imports `mark` (cycle fixed). | `emit()` (`Preview`) | in: mark, io | 1/1b |
| `core/format.md` | DELETE `deletePages` + `delete{into: document/emit, from: core/format}` | — dissolves into document/emit (V6); cross-wave absorb executes in the ABSORBER's leg (leg 2); condemned-intact until then. | — | — | (absorb leg 2) |

LEG-1b corpus-wide reconcile (whole-repository write authority): the `emit()` rewire on ALL 43 producers
(§A.2) with the G2 input-spec key re-mint; the limiter/stamina collapse (43+50 surface, §A.4); the V16
rebind (§K) on every touched page; the Gate #1 recorder seam + Gate #2 producer (§I). Leg-2/3 rebuilds
INHERIT and cold-verify these, never rewire.

### [C.3] LEG 2 — MID PLANE

| Path | Action + lowering | Tier — charter / skeleton | Entry | Seams (in ← / out →) | Wave/leg |
|---|---|---|---|---|---|
| `visualization/table.md` | REBUILD `rebuild` (E5/V10) | N/A — `GT(frame.to_pandas())` DEFAULT floor (polars `.style` demoted to probed capability); pagination/continuation-across-sheets; kiwisolver column-width solving; units-sub-rows; upstream ingress is named law: QTO/energy/impact/study evidence arrives as self-describing `data/tabular` frames (source/unit/identifier/content-key columns decoded by name, never re-derived). | `emit()` (`Table`) | in: data/tabular, style, derive; out: schedule, register, sheet, section | 2/2 |
| `visualization/chart/spec.md` | REBUILD `rebuild` + MOVE Palette out (V4) | N/A — altair typed MARKS/CHANNELS/TRANSFORMS/COMPOSITION grammar (raw `Vega(dict)` case DIES); `Palette`/`hex_ramp` MOVED to derive; `@theme.register` typed `ThemeConfig` rows are `graphic/style` lowerings; `mark_geoshape`/`project`/`topo_feature`; in-grammar `transform_regression`/`loess`/`density`; matplotlib publication `rcParams` bound by V13. | NONE (authoring→export) | in: derive, style; out: chart/export | 2/2 |
| `visualization/chart/export.md` | REBUILD `rebuild` + MERGE transform (`improve` + `delete{into: chart/export, from: chart/transform}`) | N/A — host-free render dispatch; absorbs the vegafusion `pre_transform_spec`/`pre_transform_extract` pre-pass; vl-convert `svg_to_png/jpeg/pdf` the ONE chart rasterizer + `register_font_directory` font-identity loop + `get_local_tz`/`format_locale` deterministic axes; lets-plot theme/flavor seam or narrowed charter (census blocker named). | `emit()` (`Chart`) | in: chart/spec, derive; out: compose (figures as data) | 2/2 |
| `visualization/chart/transform.md` | DELETE `deletePages` + `delete{into: chart/export, from: chart/transform}` | — merges into export (V10); stray `</content>` :314 removed. | — | — | (absorb 2) |
| `visualization/diagram/glyphset.md` | REBUILD `rebuild` (E9/V15) | N/A (vocabulary) — consume-or-delete dead carriers (`EndCap`/`SubLayout`/`TextRun`/`Port.at`/`corner`); ADD the AREA/polygon glyph case (V15 room/parcel/footprint — the five-mark closure opens by one case). | NONE | out: layout, draw, schematic | 2/2 |
| `visualization/diagram/layout.md` | REBUILD `rebuild` (V15/V10) | N/A — 5-engine coordinate assignment; typed ELK layout-option vocabulary over pyelk `layoutOptions` (stringly literals die V10); plan-geometry ingress = typed coordinate columns on the `data/tabular` adjacency frame (untyped `attrs` dies); AREA LAW (STACKING/PROGRAM/SITE area-proportional; CIRCULATION/SECTION_CALLOUT plan-anchored `Constrained`, spring defaults die); composes solar. grandalf held (parity oracle + sole `_grandalf_router` SPLINES route) until fast-sugiyama parity + spline re-home. | NONE (coordinates→draw) | in: data/tabular (GRAPH), glyphset, solar; out: draw | 2/2 |
| `visualization/diagram/draw.md` | REBUILD `rebuild` (V3/V10/V13/V15) | N/A — the diagram-suite TERMINAL: renders all 19 `NodeShape`s (12 dead/type-error fixed); routes math→`typography/math`, labels→`typography/shape` (E3); `_INK`→derive contrast pick; reads `GlyphStyle.text` via style; projects into `graphic/layer`; drawpyo `load_diagram`→`get_by_id`→`File.write` `.drawio` round-trip + `PageSize` presets; massing figures arrive as scene-egress parent keys (DATA edge). SUITE emits one node per kind. | `emit() -> Iterable[ArtifactWork]` (`Diagram`) | in: layout, math, shape, derive, layer, style; out: layered (projection); consumed by core/issue | 2/2 |
| `visualization/diagram/schematic.md` | NEW `new` (V10) | N/A — schemdraw 226-element catalog (logic/flow/dsp, Kmap/Timing/BitField) the five marks cannot express; `svgconfig.text='path'`; the `Segment*`/`ElementCompound` geometry spine stays with `drawing/symbol`. | `emit()` (`Diagram`) | in: glyphset, shape, style, layer | 2/2 |
| `visualization/diagram/solar.md` | NEW `new` (V15) | generate — solar-ephemeris: pvlib `solarposition` SPA (refraction-corrected azimuth/altitude from date/latitude), sunrise/transit/solstice solvers, numpy-vectorized date sampling; solstice/equinox/hour-line arc SAMPLING; sun-path FURNITURE (horizon circle, altitude rings, compass ticks, labeled date arcs) over `ProjectionKind` composing vector; the owned closed-form kernel is the declined-admission fallback; ladybug `Sunpath` stays OUT (AGPL, geometry track). | NONE (geometry→layout) | in: path; out: layout | 2/2 |
| `scene/render.md` | REBUILD `rebuild` (V9) | N/A — pyvista owner; standard-view family (`view_xy`/`view_isometric` plan/elevation/section/axo/iso presets); silhouette/feature-edge/hidden-line `FieldFilter` cases (`vtkPolyDataSilhouette`/`vtkFeatureEdges`/`enable_hidden_line_removal`); GL2PS 3D→vector target (`vtkGL2PSExporter`); directional sun policy (date/latitude, replaces parameterless `enable_shadows`); typed mesh ingress (geometry `mesh`/data `MeshPayload`, not `grid: object`); hoists `SceneTarget` (cycle break). Catalog authorship: `.api/vtk.md` + `.api/pyvista.md` (§D.3). vtk/usd marker-drop blockers named. | `emit()` (`Scene`) | in: geometry/mesh + data MeshPayload (DATA), derive, pattern (poché); out: export, draw + detail (linework as DATA parents) | 2/2 |
| `scene/stage.md` | KEEP `improve` (collapse premise REFUTED) | N/A — USD/USDZ authoring owner (488 LOC, deep); usd-core payloads/variants/purposes; USDZ packaging stays its own. | `emit()` (`Scene`) | in: render | 2/2 |
| `scene/export.md` | KEEP `improve` (G3) | N/A — `SceneTarget` dispatch over render's hoisted vocabulary; **the `zlib_ng` repro-ZIP clone DELETED with NO replacement import** (inversion e — scene emits FILES; bundling is `package/archive`'s emit over scene-file parent keys, a DATA edge). | NONE (substrate) | in: render, stage; out: — (files egress) | 2/2 |
| `composition/compose.md` | KEEP `improve` (V6/E13) | N/A — figure placement; projects into `graphic/layer`; `_GATE`/stamina fold to lanes; placed figures arrive as parent content keys (DATA edges), never imports. | `emit()` (`Preview`) | in: path, layer; out: layered, indesign | 2/2 |
| `composition/imposition.md` | REBUILD `rebuild` (V7/E6) | N/A — the ONE press fold owner (`_mint_groups`/`_configure_layers` OCG mechanism); sheet + egress compose it; pdfimpose `AbstractImpositor`/`impose`/`Margins` schemes; completes the Map migration. | `emit()` (`Egress`) | in: layer; out: sheet, egress, transmittal (composed) | 2/2 |
| `composition/sheet.md` | REBUILD `rebuild` (V3/V7/V13) | bind — the sheet PRODUCER (construction moved to core/issue, G1): composes imposition's fold (clone dies); ISO 216-derived `_SIZES` + `pymupdf.paper_rect`; 2D ISO 7200 title-block grid; sheet-family VARIANTS via style; `regime.lettering()` + shaped runs (Helvetica magic dies); **cover-sheet producer** (zero corpus concept) + drawing-INDEX sheet (`scheduled()`→compose→table format bridge made real) + general-notes sheet (REAL sheet←emit typesetting seam); MediaBox/TrimBox/BleedBox/CropBox page-box law. `SheetSet.emit()` returns ONE node per sheet, per-member keys. | `emit() -> Iterable[ArtifactWork]` (`Egress`/`Pdf`/`Preview`) | in: regime, style, layer, shape, layout, table, imposition, emit (notes); out: register (data), layered; consumed by core/issue + transmittal | 2/2 |
| `document/model.md` | KEEP `improve` (E10) | N/A — the 11-variant `DocumentNode` tree; `:15` "ten-variant" stale count fixed; `FormulaNode` composes typography/math. | NONE (tree) | in: math; out: emit, report, tagged, lens, section | 2/2 |
| `document/emit.md` | REBUILD `rebuild` (V6/V12/E1/E7; absorbs format) | N/A — the lowering axis composing typography seams IN its arms (V3 — ARCH:153/156 live, no reportlab re-derived metrics, bidi real); ~45-field `EmitSpec` → grouped value objects; weasyprint `@page`/`string-set` running heads + `target-counter`; cross-arm page-numbered TOC; FORMS first-class (`pdf_oxide DocumentBuilder`/`pikepdf.add_field`/`pymupdf.add_widget` per-target lowering; `pdf_forms` over real markup); pyphen orthographic substitution APPLIED at breaks; absorbs `core/format` (`TemplatePayload` admission + `bound()` fan-out + office template-clone delegates as emit rows); `produced` batch rail dies. The document-package NODE SET is emitted here; construction is core/issue's. | `emit() -> ArtifactWork \| Iterable[ArtifactWork]` (`Pdf`/`Office`/`Report`/`Document`) | in: model, shape, layout, font, managed, style, layer, detect; out: report, egress, tagged, conformance (data), sheet (notes); consumed by core/issue | 2/2 |
| `document/report.md` | REBUILD `rebuild` (V12/E6) | N/A — reproducible report composition (jinja2 `SandboxedEnvironment`, papermill/nbclient rails, `get_exporter`); terminal PDF routes through emit (`Pdf.from_html` dup dies); `_toc` gains page numbers; `rendered` batch rail dies. | `emit()` (`Report`) | in: model, emit | 2/2 |
| `document/egress.md` | KEEP `improve` (E6/V7) | N/A — finishing close (encrypt/outline/watermark/page-box; msoffcrypto `OfficeFile` protect/decrypt); composes imposition's saddle fold (`[EGRESS_DISTINCT]` contradiction fixed). | `emit()` (`Egress`) | in: imposition, emit; consumed by core/issue | 2/2 |
| `document/lens.md` | KEEP `improve` (V12) | N/A — recover-TO owner; OCR pre-flight gate (`ocrmypdf.pdfinfo.PdfInfo`/`PageInfo.has_text` SKIPS OCR on text-bearing pages; `is_tagged`/`has_acroform`/`has_signature` routing) + in-package PDF/A egress (`speculative_pdfa_conversion`/`file_claims_pdfa` feeding the veraPDF oracle) + `run_hocr_pipeline` two-phase hOCR; calamine XLSX_READ. | `emit()` (`Introspection`) | in: model, detect; out: data/tabular (corpus, DATA) | 2/2 |
| `document/tagged.md` | KEEP `improve` | N/A — PDF/UA structural author + audit reconciliation; consumes the typed `ConformanceVerdict` off `ArtifactReceipt.Verdict` (w2→w1, G5). | `emit()` (`Egress`) | in: model, receipt (typed verdict); out: conformance (structural result, DATA) | 2/2 |

### [C.4] LEG 3 — AEC / EGRESS

| Path | Action + lowering | Tier — charter / skeleton | Entry | Seams (in ← / out →) | Wave/leg |
|---|---|---|---|---|---|
| `core/issue.md` | NEW `new` (G1, V6/E1) | N/A — **THE sole constructing owner**: polymorphic `issue(IssueRequest)` over the closed `sheet_set \| diagram_suite \| document_package \| single` union; builds `ArtifactPipeline.of(terminal.emit(), targets=...)`, drives fronts through the runtime `LanePolicy`, returns terminal receipts (§A.3 skeleton). Top of the dependency cone; nothing imports it. | `issue(IssueRequest) -> RuntimeRail[Block[ArtifactReceipt]]` (composition root, not a producer — no receipt case) | in: transmittal, sheet, emit, egress, draw, plan, runtime lane; out: — | 3/3 |
| `drawing/standard.md` | REBUILD `rebuild` (G6 — vocabulary slice OUT to regime; absorb {into: drawing/regime, from: drawing/standard vocabulary}) | bind (lowering) — the ezdxf symbol-table LOWERING: `seed`/`graphics`/`dimstyle`/`hatch`/`rgb`/`paper_factor` authoring composing regime's BIND rows + pattern's `PatternSpec` definitions; the standard→dxf seed seam (V11 — dxf composes, `_table_entry` dup dies). | NONE (substrate) | in: regime, pattern; out: dxf (seed), dimension (dimstyle), annotate (mleader) | 3/3 |
| `drawing/dimension.md` | REBUILD `rebuild` (V3/V5/E3/E8) | generate — ISO 129-1 producer; ISO 286 fits + ISO 1101 GD&T frames/datums + dual-unit DIMALT; composes symbol's V5 terminator geometry; typography shape+math (local ziafont/ziamath dies); vector metric point-at-distance; DIM policy from regime `TextHeight` ratios (magic dies). | `emit()` (`Drawing`) | in: regime, standard, symbol, shape, math, path, derive, layer | 3/3 |
| `drawing/annotate.md` | REBUILD `rebuild` (V3/V5/E3/E7) | generate — ISO 128-2 annotation; welding (ISO 2553) + surface-texture (ISO 1302) + datum/level symbols; consumes shaped POSITIONS (E3, never re-shapes); composes symbol V5 terminators; scallop-band via region offset; palette rewired to derive (E7 ledgered). | `emit()` (`Drawing`) | in: regime, symbol, shape, layout, math, region, derive, layer | 3/3 |
| `drawing/symbol.md` | REBUILD `rebuild` (V5) | generate — the ONE parametric mark-geometry owner (ISO 129-1 terminators; north-arrow/scale-bar/grid-bubble/section-marker proportion ROWS; revision triangles/clouds); proportion rows feed BOTH SVG + DXF lowerings; rotation/reflection composes vector `Matrix`/`Point.polar_to`/`reflected_across` (numpy trig dies); schemdraw `Segment*`/`ElementCompound` spine stays. `drawing/geometry.md` NOT minted — symbol's ownership holds. | `emit()` (`Drawing`) | in: path, region, regime, derive, layer; out: dimension, annotate, detail (composed) | 3/3 |
| `drawing/detail.md` | REBUILD `rebuild` (E9/V1/V9) | generate — `SheetId`-typed refs (stringly `sheet: str` dies E9); build-once rustworkx DAG; vector area-weighted centroid (vertex-mean dies); section/elevation callouts re-scoped to scene-extracted vectors or upstream figures (V9 boundary — `block: bytes` gains a stated origin); palette via derive. | `emit()` (`Drawing`) | in: symbol, regime, path, derive, layer; scene linework as DATA parents | 3/3 |
| `drawing/schedule.md` | REBUILD `rebuild` (E5/V2/V10) | bind — schedule/legend producer; pandas-floor render (E5 — leg-2 table lands it, schedule verifies); pattern-generator swatches (crosshatch trig + invented hex die); `Reshape`-lowered; QTO rows as self-describing frames. | `emit()` (`Schedule`) | in: table, regime, pattern, derive, data/tabular | 3/3 |
| `specification/section.md` | KEEP `improve` | N/A — CSI SectionFormat producer; full §5.1.7 Schematron; composes classify `ClassCode`. | `emit()` (`Spec`) | in: model, classify, table | 3/3 |
| `specification/classify.md` | KEEP `improve` | N/A (substrate) — MasterFormat/UniFormat/OmniClass + OmniClass element rows + Uniclass 2015 (kills register's parallel enum); rustworkx `transitive_reduction` crosswalk closure. | NONE | in: regime (Discipline); out: section, register | 3/3 |
| `delivery/register.md` | REBUILD `rebuild` (E5/E6/E9) | N/A — ISO 19650 register; composes classify `ClassCode` + regime `Discipline` (parallel `ClassificationSystem` dies E6; bare-str dies E9); Uniclass 2015; silent blocked-Stub fixed (E5, verifies the leg-2 pandas floor); rustworkx `TopologicalSorter` assembly ordering. | `emit()` (`Register`) | in: sheet (data), classify, regime, table; out: transmittal (composed) | 3/3 |
| `delivery/transmittal.md` | REBUILD `rebuild` (E7) | N/A — ISO 19650 issue AGGREGATE PRODUCER (construction moved to core/issue, G1): `emit()` returns the member node set + ONE aggregate `Transmittal` node whose `parents` are member keys; codec edge LEDGERED (E7); distribution matrix + acknowledgement round-trip; rustworkx `ancestors`/`descendants` cross-reference closure. | `emit() -> Iterable[ArtifactWork]` (`Transmittal` aggregate) | in: sheet, imposition, archive, codec, conformance, credential, register; consumed by core/issue | 3/3 |
| `export/dxf.md` | REBUILD `rebuild` (V11/E9) | N/A — paperspace/viewport authoring; `Dxf.New` composes `Standard.seed` (V11 seam real); ATTRIB/Image/MPolygon/underlay entity completion; `import_blocks` beside `import_entities`; by-NAME version/unit (E9); `ezdxf.path`/`ezdxf.math` vector bridge; `of()+contribute()` converges to `emit()`. | `emit()` (`Cad`) | in: standard (seed), regime, path | 3/3 |
| `export/layered.md` | REBUILD `rebuild` (V14, G7 — stays ONE page) | N/A — the layered-export WRITERS composing `graphic/layer.LayerPlan` (never owns it): PSD group folders + 12 native blend modes (psd-tools standing author; PhotoshopAPI census blocker named), PDF OCG with the real `/Order` tree, structured SVG `<g>` (drawsvg Group), IDML layers, layered TIFF/ORA (psdtags/imagecodecs `*_encode`/`.available`); the ILLUSTRATOR lane explicit = layered-PDF + organized-SVG pair with the tree intact. | `emit()` (`Preview`/`Egress`) | in: layer, compose | 3/3 |
| `export/indesign.md` | KEEP `improve` | N/A — SimpleIDML template mutation; IDML layers = LayerPlan; the E1 convergence proof holds. | `emit()` (`Office`) | in: layer, compose | 3/3 |
| `exchange/conformance.md` | KEEP `improve` (G5) | N/A — pyhanko PAdES close; MINTS the re-homed `core/receipt.ConformanceVerdict` (imports the shape DOWN, inversion a); veraPDF/JHOVE oracle rides the runtime `guarded` ORACLE row. | `emit()` (`Verdict`) | in: receipt (shape), emit, tagged, font | 3/3 |
| `exchange/credential.md` | KEEP `improve` | N/A — c2pa `Builder` sign / `Reader.get_validation_state`. | `emit()` (`Credential`) | in: runtime identity; out: transmittal (composed) | 3/3 |
| `exchange/metadata.md` | REBUILD `rebuild` (census) | N/A — descriptive metadata over the `MetaCarrier` one-pass axis; **pyexiv2 REMOVED** (§D.2 — subset of the standing out-of-process pyexiftool arm; GPL-3.0 in-process violates the copyleft posture). | `emit()` (`Metadata`) | in: detect | 3/3 |
| `media/container.md` | KEEP `improve` | N/A — av spine; `_WORKER_RETRY` folds to lanes (leg-1 reconcile); scene frames arrive as DATA parents. | `emit()` (`Media`) | in: filtergraph | 3/3 |
| `media/filtergraph.md` | KEEP `improve` (E7 REFUTED) | N/A (substrate) — capability-routed filter core; no derive coupling (E7 refuted, edge struck). | NONE | out: media/* | 3/3 |
| `media/audio.md` | KEEP `improve` | N/A — audio encode over container. | `emit()` (`Media`) | in: container, filtergraph | 3/3 |
| `media/timeline.md` | KEEP `improve` | N/A — non-linear editing; strongest multi-parent clip DAG (ArtifactPipeline exemplar). | `emit()` (`Media`) | in: container, filtergraph, audio | 3/3 |
| `media/subtitle.md` | REBUILD `rebuild` (E3) | N/A — pysubs2 timed-text; `shaped_rgba` now real (E3). | `emit()` (`Media`) | in: container, filtergraph, shape | 3/3 |
| `media/analysis.md` | REBUILD `rebuild` (E5) | N/A — read-side measurement; `render_png`/`montage`/`frame_similarity` now real io/measure surfaces (E5). | `emit()` (`Media`) | in: container, audio, filtergraph, io, measure | 3/3 |
| `media/synthesis.md` | KEEP `improve` | N/A — numpy oscillator bank. | `emit()` (`Media`) | in: audio | 3/3 |
| `package/bundle.md` | NEW `new` (cycle break; LEG 3 — G3 supersedes the base's w2 re-wave) | N/A (vocabulary) — shared `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleManifest` + the `PackWorker` port; codec/archive/delta compose it downward (cycle break); zlib-ng band vocabulary beside lz4/brotli/zstandard. | NONE | out: codec, archive, delta | 3/3 |
| `package/codec.md` | REBUILD `rebuild` (E10) | N/A — single-blob compression; composes bundle (eager private-worker reach dies, cycle broken); the LIVE zlib-ng GZIP band (`gzip_ng`/`gzip_ng_threaded`/`crc32`/`crc32_combine`) — the re-entry consumer (§D.2). | `emit()` (`Bundle`) | in: bundle; out: transmittal (composed) | 3/3 |
| `package/archive.md` | KEEP `improve` (+ carried growth) | N/A — multi-file archive; **the reproducible-ZIP owner** (scene bundling routes here as DATA parents — inversion e); **multi-entry `unpack`/`list`/`BundleManifest` inverse** ([04] carried growth). | `emit()` (`Bundle`) | in: bundle; out: transmittal (composed) | 3/3 |
| `package/delta.md` | KEEP `improve` (+ carried growth) | N/A — detools delta; **parent-keyed delta row** ([04] carried growth). | `emit()` (`Bundle`) | in: bundle | 3/3 |

deletePages: `core/format.md` (→ document/emit, absorb leg 2), `visualization/chart/transform.md`
(→ chart/export, absorb leg 2), `graphic/vector.md` (→ graphic/vector/region, absorb leg 1a).
`drawing/standard.md` and `export/layered.md` are NOT deleted (G6/G7 — slice-move and compose-only
rebuilds). Net: 60 − 3 deleted + 12 new (`graphic/vector/{path,region,pattern}`, `graphic/style`,
`graphic/layer`, `graphic/marks/mark`, `typography/math`, `drawing/regime`,
`visualization/diagram/{schematic,solar}`, `package/bundle`, `core/issue`) = **69 live pages**.

---

## [D] PACKAGE ROSTER DELTA (integration-first; every reconciliation row RULED by its condition)

### [D.1] The pvlib admission (feed-verified, additive)

`pvlib` — BSD-3, NREL-backed, pure-Python wheel (interpreter-agnostic, NO census exposure). **ADMIT.**
Owner: `visualization/diagram/solar.md` (V15). Integration: the `solarposition` SPA family
(`get_solarposition`, `sun_rise_set_transit_spa`, `nrel_earthsun_distance` — refraction-corrected
azimuth/altitude), sunrise/transit/solstice solvers, numpy-vectorized date sampling.
`.api/pvlib.md` authored at admission with the verified `solarposition` member set; the root row + `.api`
land WITH leg 2 (the owner's leg). Discharges the recorded proof burden; the owned closed-form kernel
stands as the declined-admission fallback. Distinct from geometry's AGPL ladybug `Sunpath` (process
boundary, geometry track).

### [D.2] `[PYPROJECT_RECONCILIATION]` census rows — each ruled by its survival CONDITION (never zero-count)

| Package | Census row | RULING (condition-tested) |
|---|---|---|
| `grandalf` | copyleft GPL-2.0/EPL-1.0, no release since 2023 | KEEP (integration) — LIVE consumer: parity oracle + the SOLE splines router (`layout.md:_grandalf_router`). Removal gated on fast-sugiyama parity AND a SPLINES re-home (a real routing ripple, never a zero-consumer strike); behind the process boundary until then (copyleft posture). `diagram/layout` (leg 2) proves parity + re-homes splines; removal lands only when BOTH hold. |
| `pyexiv2` | GPL-3.0 in-process (`metadata:46`), cp-gated dead marker | **REMOVE** — survival condition (consumed capability pyexiftool cannot reach) is UNMET on disk: pyexiv2's EXIF+IPTC+XMP+ICC is a SUBSET of the standing out-of-process pyexiftool arm, AND the in-process GPL-3.0 form violates the copyleft-behind-a-process-boundary posture. Drop the root row; `.api/pyexiv2.md` records SUPERSEDED. Lands leg 3 (the owner's leg). |
| `iptcinfo3`, `python-xmp-toolkit` | SUPERSEDED, zero live consumers (prose-only `metadata:7`) | **REMOVE** — no page-set row lands a sidecar-XMP owner consuming them; the one-pass pyexiftool fold owns the carrier (pikepdf owns the PDF/XMP path). `.api` records SUPERSEDED. |
| `zlib-ng` | data-campaign strike; artifacts named its home | **RE-ENTER** — condition MET on disk: `package/codec:17`'s GZIP arm consumes `gzip_ng`/`zlib_ng`/`crc32` live. Re-tag `pyproject.toml` `[DATA]`→`[ARTIFACTS]`; author the folder `.api/zlib-ng.md` overlay beside lz4/brotli/zstandard. `scene/export`'s hand-reach clone DELETES (inversion e, G3) — the package plane is the sole consumer, exactly as the brief sites it. |
| `PyICU` | never shipped a wheel (sdist + native ICU + compiler), dead marker | GATED — `uniseg`+`python-bidi` are the standing charter (shape/layout). Resolution is the Forge native-build lane or removal; carried as an integration target with the blocker named, never a removal. |
| `scikit-image` | cp314 wheels stand, cp315 pending | GATED (wheel-lag) — process/measure capability carries the marker-drop blocker by name (V8). |
| `vtk`, `usd-core`, `lets-plot` | cp314 wheels stand, cp315 pending | GATED (wheel-lag) — V9/V10 capability carries the marker-drop blocker by name; `pyvista` pure-Python, gated only through vtk; lets-plot's second-engine charter narrows to a consumed-capability row or gains its theme/flavor/element seam (a parity claim without a consumed row dies). |
| `PhotoshopAPI` | no sdist, wheels stop at cp314 (structurally dead) | GATED — `psd-tools` is the standing PSD author (the V14 blend modes land on it first); PhotoshopAPI's native-writer lane rides the Forge source-build follow-up. Blocker named. |

PDF-arm rationalization (carried): `pdf_oxide` (MIT-OR-Apache, abi3 cp315-forward) is the layout-aware
default; `pdfplumber`/`pypdf`/`pypdfium2`/`pymupdf` split PER CONCERN (lens extract / assembly / render /
repair+forms) — each concern a live consumer, no PDF-arm removal. `puremagic`-default /
`python-magic`-fallback confirmed (detect, leg 1a).

### [D.3] Package → owner binding map (G4 — the roster-closure spine at operator depth)

SHARED-TIER WEAVE, composed in EVERY fold, never re-minted: `expression`
(`tagged_union`/`Result`/`Option`/`Block`/`Map` — the one ADT + dispatch spine; `Map.of_seq` the
dispatch-table rail per §K); runtime `identity` (`ContentIdentity.of` over canonical bytes under
`CANONICAL_POLICY`; `IdentitySource.lift` for buffer/stream/merkle; never `repr()`/`str()` bytes); runtime
`lanes.offload(retry=RetryClass.OCCT)` (every native/subprocess/press/raster/render/media seam; zero
artifacts-minted limiters); runtime `guarded` over the one `POLICY` table incl. the `ORACLE` row
(conformance/lens/io oracle verdicts only); runtime `ReceiptContributor` structural port (no subclass);
the runtime metric recorder (Gate #1, §I); `structlog`+`opentelemetry` span per fold via the owned weave;
`msgspec` one-shot ingress admission + wire projections + the hashable evidence bands; `numpy`
vectorization where a page loops rows; `pydantic`/`pydantic-settings` boundary-only.

| Package(s) | Owning page | Mine-to-depth binding |
|---|---|---|
| `svgelements` | `graphic/vector/path` | `SVG.parse`/`Path`/`Matrix`/`bbox`/`SVG.select`; `Point.distance_to`/`angle_to`/`polar_to`/`reflected_across`/`matrix_transform`; `Length.to_mm` unit egress; metric arc-length point-at-distance, decimation, area-weighted centroid; tolerance/DPI magic lifted to policy rows |
| `skia-pathops` + `drawsvg` + `resvg-py` | `graphic/vector/region` | `addPath`/verb-level build, boolean/offset/stroke-to-outline/warp/wind/contains; `Drawing`/`Group`/`Raw` replacing ALL f-string SVG; gradient primitives (V13); resvg raster; `RenderPolicy` |
| `drawsvg.Pattern` + `skia-pathops` + `ezdxf.tools.pattern` | `graphic/vector/pattern` | the V2 generator: `StrokeFamily`/`PatternSpec`/`DensityLaw`; THREE lowerings — `set_pattern_fill(definition=)`, `drawsvg.Pattern` tiles, pathops clip |
| `colour-science` + `coloraide` + `colour-cxf` + `pyvips`(ICC) + `pikepdf`(plate) | `graphic/color/{derive,managed}` | derive: CIE/CAM16/spectral + gamut/CVD/harmony/WCAG + `interpolate`/`mix`/`mask` + the 28-member `Metric` block + rehomed `Palette`/`hex_ramp` + `AdaptMethod`; managed: `icc_transform` egress, `write_LUT`/SpectralShape authoring, `read_cxf`/`write_cxf` CxF3 intake, Separation/DeviceN plate authoring over the pikepdf raw object model, TAC gate |
| `pillow` + `pyvips` + `tifffile` + `scikit-image` | `graphic/raster/{io,process,measure}` | pillow IO/ImageChops/ImageMath/Color3DLUT/gradients; pyvips `block_untrusted_set` + `Source`/`Target` + `Foreign*` enums; tifffile GeoTIFF tags, `aszarr`/`memmap`/`TiffSequence`, `validate_jhove` oracle; the 97/42-member scikit engines behind the acceptor tables (census-gated) |
| `segno` + `python-barcode` + `zxing-cpp` | `graphic/marks/{mark,encode,decode}` | `QRCode`/`make_micro` + payloads; `get_barcode_class` SVG writers; `create_barcode` + `read_barcodes` round-trip over the neutral vocabulary |
| `uharfbuzz` + `python-bidi` + `blackrenderer` + `vharfbuzz` + `fonttools` + `opentype-feature-freezer` + `uniseg` + `pyphen` (+ `PyICU` gated) | `typography/{font,shape,layout,math}` | shaping/bidi/COLRv1 (`BlackRendererFont`/32-member `PaintFormat`/`Surface`); `fontTools.unicodedata.script()` itemization; `hb_ot_metrics`/`get_font_extents` metrics; `Face.has_math_data`/`get_math_constant`/`get_math_glyph_variants`/`get_math_glyph_assembly` math tier; `RemapByOTL` FREEZE; `DataInt.data` orthographic channel; `vharfbuzz.serialize_buf` SHAPE_QA goldens |
| `great-tables` + `kiwisolver` + `polars` | `visualization/table` | `GT(frame.to_pandas())` floor, `tab_options`/`write_raw_html`/`with_locale`; kiwisolver width solving |
| `altair` + `matplotlib` + `vegafusion` + `vl-convert-python` + `lets-plot`(census) | `visualization/chart/{spec,export}` | typed `theme.ThemeConfig`/`*Kwds` registry (`@theme.register`); `mark_geoshape`/`project`/`topo_feature`/`graticule`; in-grammar `transform_regression`/`loess`/`density`/`quantile`; `expr` namespace; `runtime.pre_transform_spec`/`pre_transform_extract`; `svg_to_png/jpeg/pdf` the ONE rasterizer; `register_font_directory`; `get_local_tz`/`format_locale` |
| `pyelk` + `fast-sugiyama` + `grandalf`(census) + `rustworkx` + `schemdraw` + `drawpyo` + `pvlib`(NEW) | `visualization/diagram/{glyphset,layout,draw,schematic,solar}` | typed `layoutOptions` vocabulary; 5-engine assignment; the 226-element schemdraw catalog (`svgconfig.text='path'`); drawpyo `load_diagram`→`get_by_id`→`File.write` + `PageSize` presets + TOML style vocabularies as `GlyphStyle` targets; pvlib `solarposition` |
| `pyvista` + `vtk` + `usd-core` | `scene/{render,stage,export}` | view presets, `vtkPolyDataSilhouette`/`vtkFeatureEdges`/`enable_hidden_line_removal`, `vtkGL2PSExporter`; USD payloads/variants/purposes (census-gated) |
| `pdfimpose` + `pymupdf` + `reportlab`/`weasyprint`/`typst` | `composition/{imposition,sheet}` | `schema.AbstractImpositor`/per-schema `impose`/`Margins`; `paper_rect`; page-box law; the ONE press fold |
| `typst` + `python-docx` + `odfpy` + `ruamel-yaml` + `tomlkit` + `docxtpl` + `python-pptx` + `xlsxwriter` + `openpyxl` + `pikepdf`/`pdf_oxide`/`pymupdf`(forms) + `weasyprint` + `msoffcrypto-tool` + `python-calamine` + `ocrmypdf` + `jinja2`/`papermill`/`nbclient`/`nbconvert`/`jupytext` | `document/{emit,report,egress,lens,tagged}` | `Compiler` world reuse + `query`/`eval`; the `add_*`/`Styles` DOCX fold; ODT/ODS arms; round-trip YAML/TOML arms (the format dissolution re-homes the delegates); `DocumentBuilder`/`add_field`/`add_widget` forms; `@page`/`string-set`/`target-counter`; `OfficeFile` protect/decrypt; `CalamineWorkbook.iter_rows`; `pdfinfo`/`pdfa`/`run_hocr_pipeline`; `SandboxedEnvironment` + the nbclient fault rail + `get_exporter` |
| `ezdxf` | `drawing/standard` + `export/dxf` | symbol-table `seed`/`graphics`/`dimstyle`/`hatch`; paperspace/layout API; ATTRIB/Image/MPolygon/underlay; `import_blocks`; DIMSTYLE full-variable surface; `ezdxf.path`/`ezdxf.math` bridge |
| `lxml` + `rustworkx` | `delivery/{register,transmittal}` + `specification/classify` + `drawing/detail` | `etree._Element` authoring, `XPath`/`XSLT`/`iterparse`, `XMLSyntaxError` rail; `TopologicalSorter` staged release, `transitive_reduction`, `ancestors`/`descendants`, `PyDAG`/`DAGWouldCycle` |
| `psd-tools` + `psdtags` + `imagecodecs` + `simpleidml` (+ `PhotoshopAPI` gated) | `export/{layered,indesign}` | `TiffImageSourceData`→`PsdLayers`→`PsdChannel` + `fromtiff`; per-codec `*_encode`/`*_decode` + `.available` probes; IDML step fold |
| `pyhanko` + `c2pa-python` + `pyexiftool` | `exchange/{conformance,credential,metadata}` | PAdES B-B/T/LT/LTA + `/DocTimeStamp`/`augment`/`reserve`/`audit`; `Builder` sign / `Reader.get_validation_state` / `C2paSigningAlg`; the one-pass `MetaCarrier` |
| `av` + `pysubs2` | `media/*` | `InputContainer`/`OutputContainer` mux/demux, `av.filter.Graph`, `VideoFrame.from_ndarray`; `SSAFile`/`SSAEvent`/`make_time` |
| `py7zr` + `stream-zip` + `stream-unzip` + `detools` + `zstandard`/`lz4`/`brotli`/`zlib-ng`(re-entry) | `package/{bundle,codec,archive,delta}` | `SevenZipFile.writeall`/`extractall`/`test`; `stream_zip` `MemberFile`/`Method` rows + AES sentinels; `create_patch`/`apply_patch`/`patch_info` parent-keyed; the codec bands incl. `gzip_ng`/`crc32_combine`; the reproducible-ZIP kernel on archive |
| `rustworkx` (CPM) | `core/plan` | `PyDiGraph` topology + the CPM `Schedule` (verbatim engine) |

`.api` obligations authored WITH the verdict (absence-claims the register names): `.api/pvlib.md` NEW
(solarposition set); `.api/zlib-ng.md` folder overlay NEW (re-entry); `.api/pikepdf.md` ADD the
Separation/DeviceN plate-AUTHORING write surface + `add_field` (read-side flags only today, `:124,30,135`);
`.api/pymupdf.md` ADD `trimbox`/`bleedbox` + `add_widget` (only `mediabox` today); `.api/weasyprint.md`
ADD `@page` margin-box running content, `string-set`, `target-counter` (absent today); `.api/vtk.md` +
`.api/pyvista.md` ADD `vtkPolyDataSilhouette`/`vtkFeatureEdges`/`vtkGL2PSExporter` + the view-preset
method family (absent from BOTH tiers). Deepen existing: `drawsvg.Pattern` (V2); `schemdraw` element
catalog (V10); `uharfbuzz` math surface (V3); `tifffile` GeoTIFF/aszarr/memmap/validate_jhove (V8);
`great-tables` GT(pandas)/tab_options (V10/E5); `altair` ThemeConfig registry (V13); `vl-convert`
register_font_directory (V13); `drawpyo` load_diagram round-trip (V10); `rustworkx` DAG surface for
delivery/spec (E-pkg); `ocrmypdf` pdfinfo/pdfa (V12); `ezdxf.path`/paperspace (V11);
`coloraide`/`colour-science` LUT/CAM16 (V4); `pyphen` DataInt.data channel (V12). License census carried
in the owning README/`.api`, never design prose.

---

## [E] VERDICT DISPOSITION (V1-V16, every one ruled)

| V | Ruling (page/leg) |
|---|---|
| V1 | `graphic/vector.md` SPLITS → `graphic/vector/{path,region,pattern}` (3 pages, leg 1a). Metric point-at-distance, decimation, area-weighted centroid, `Length.to_mm` unit egress on `path`; f-string SVG→drawsvg on `region`; tolerances→policy rows. |
| V2 | NEW `graphic/vector/pattern.md` (leg 1a): `StrokeFamily`/`PatternSpec`/`DensityLaw`, 3 lowerings (ezdxf/drawsvg.Pattern/pathops-clip); `HatchMaterial` seed rows per ISO 128-50/ANSI/BS regime bind in `drawing/regime`. schedule/dxf/layered consume it. |
| V3 | NEW `typography/math.md` (leg 1b) + `shape` exports `shaped_rgba` + fallback-chain resolution (fontTools.unicodedata) + the font-METRICS owner on `font.md`. Consumers (dimension/annotate/symbol/draw/emit/sheet) rewire; SHAPE_QA golden oracle proves parity. |
| V4 | `Palette`/`hex_ramp` MOVE chart/spec→derive (leg 1a); `ColorReceipt` DELETED (derive = substrate); **arity RULED at 2 pages (G11)**: measurement (the 28-member `Metric` family) stays a derive owner block; CxF3 intake + plate authoring + TAC gate + LUT authoring consolidate into managed beside the `SpotChannel` lane they feed — the 3/4-page splits rejected as thin single-consumer shells, capability preserved whole. `AdaptMethod` collapses to derive. NEW `ArtifactReceipt.Color` case for the LUT/plate/swatch terminals (G10). No literal hex corpus-wide. |
| V5 | ONE mark-geometry owner in `drawing/symbol` (leg 3): ISO 129-1 terminators + north/scale-bar/grid-bubble/section proportions + revision triangles/clouds; proportion ROWS feed SVG+DXF; composes vector Matrix (numpy trig dies). `drawing/geometry.md` NOT minted (symbol ownership holds). |
| V6 | The `emit() -> ArtifactWork \| Iterable[ArtifactWork]` contract corpus-wide (§A) with the G2 key-over-input law and the G9 admission rule; `core/plan` REAL via THE ONE constructing owner `core/issue` (§A.3, leg 3); batch rails (`produced`/`rendered`/`_fanned`) + `Block[Result]` batch + `ColorReceipt` die; `core/format` dissolves into `document/emit`. Leg-1b reconcile. |
| V7 | ONE press fold in `composition/imposition` (leg 2); sheet + egress compose it; ISO 216 `_SIZES`; 2D ISO 7200 grid; cover/index/general-notes ISSUE completion; sheet←emit notes seam COMPOSED; MediaBox/TrimBox/BleedBox page-box law. |
| V8 | `graphic/raster/io` re-partition (leg 1b): bare `render_png`/`montage` exported, pyvips hardening/streaming, RasterFact consolidated; marks cycle broken via `graphic/marks/mark`. scikit-image marker-drop blocker named. |
| V9 | BUILD the scene drawing bridge (leg 2): standard-view family + silhouette/feature-edge `FieldFilter` cases + GL2PS target + directional sun policy + typed mesh ingress on `scene/render`; render⇄export cycle broken (SceneTarget hoisted); `.api/vtk.md`+`.api/pyvista.md` authored. stage KEEPS. vtk/usd marker-drop blockers named. V15's massing suite + detail's callouts consume this egress as DATA parents. |
| V10 | table pandas-floor (E5) + pagination + kiwisolver + units rows; chart transform MERGES into export + typed grammar (Vega dict dies) + lets-plot narrowed/seam; diagram 19-shape parity + dead carriers consumed + NEW `schematic.md` + typed ELK vocab. lets-plot blocker named. |
| V11 | `export/dxf` (leg 3): paperspace/viewports, `Standard.seed` seam real, ATTRIB/Image/MPolygon/underlay, `import_blocks`, by-NAME version/unit, ezdxf.path bridge. |
| V12 | `document/emit` composes typography seams + knob-bag→value objects + weasyprint parity + section-aware running heads + cross-arm TOC + FORMS first-class + pyphen orthographic emission; `document/report` routes terminal PDF through emit (dup dies); `document/lens` OCR pre-flight + PDF/A egress. |
| V13 | NEW `graphic/style.md` (leg 1a): theme-as-data, TYPE SYSTEM rows + SHEET FAMILY rows; every visual plane composes the binding seam; theme rows SELECT `drawing/regime` bind rows by key (the full generate/bind/select triad named: vector+pattern+symbol GENERATE, regime+layer BIND, style SELECTS); six AEC DiagramKinds reach publish grade via theme data over V15 geometry. Library-default output = defect. |
| V14 | NEW `graphic/layer.md` `LayerPlan` (leg 1a, G7): semantic layer TREE, ISO 13567 names; the NINE disk projectors project INTO it (upward fan → downward, inversion c disposed); `export/layered` stays ONE page composing the tree; 12 PSD blend modes on psd-tools; the Illustrator lane explicit. |
| V15 | NEW `visualization/diagram/solar.md` (pvlib + closed-form fallback, leg 2); plan-geometry typed-column ingress + area law in `layout`/`glyphset`; massing suite = V9 scene egress + diagram overlays (DATA parents). Ladybug Sunpath stays OUT (AGPL, process boundary). |
| V16 | (a) `content_identity`→`identity` (69 spellings/45 paths, per touched page); (b) frozendict band-vs-table split RULED in §K on the PEP-814 refutation; (c) `[RESEARCH]` purge (54 pages) folds into Packages/`.api` or in-body gated obligations. `rg`-zero acceptance per §K. |

---

## [F] EVIDENCE DISPOSITION (E1-E13, every row; anchors re-verified on disk)

| E | Disposition |
|---|---|
| E1 | Entry fiction. `ArtifactPipeline` constructed by nobody → THE constructing owner `core/issue` (§A.3); `Work[ArtifactReceipt]` satisfiable via the `_emit` thunk; the pre-run input key (G2) makes keyed admission a real cache probe; batch rails die. RESOLVED §A. |
| E2 | Color orphan. `Palette`/`hex_ramp`→derive; 4 drawing importers + chart/spec + chart/export + scene rewire to derive; scene's 3rd alias dies; ARCH:103-105 edges composed; the detail prose-vs-fence split resolved. RESOLVED §B/§C. |
| E3 | Dual text engine. NEW `typography/math`; `shaped_rgba` exported; annotate consumes POSITIONS; the fontTools metrics leak → the font-metrics owner; 7 hand-build pages rewire; SHAPE_QA parity. RESOLVED §C. |
| E4 | Hatch instances. NEW `graphic/vector/pattern` generator + `drawing/regime` bind rows; 11 borrowed ACAD names + ANSI31 double-booking + invented swatch hex die. RESOLVED (V2/G6). |
| E5 | Broken paths. table `GT(to_pandas())` floor (schedule + register subtotal render); `render_png`/`montage`/`frame_similarity` exported bare (io/measure). Leg 2 lands the table fix; leg 3 verifies. RESOLVED. |
| E6 | Duplication. press fold→imposition; `Pdf.from_html`→emit; egress imposition algebra→imposition; terminator triad→symbol V5; symbol-table writers→standard.seed (dxf composes); register `ClassificationSystem`→classify; `AdaptMethod`→derive. RESOLVED. |
| E7 | Unwired seams. ARCH:122/137/153/156 composed (dimension/annotate←vector, emit←shape/layout); `annotate:49` chart/spec import → derive, LEDGERED; `transmittal:58` codec edge LEDGERED; `filtergraph:14` derive coupling REFUTED (edge struck); the ARCH:122 "landed outline" stale parenthetical corrected (`dimension:746` records the phantom deletion; the real seam is region's stroke-to-outline composed via symbol). RESOLVED. |
| E8 | Hardcoding. vector tolerances→policy rows; sheet pt tables→ISO 216 + Helvetica magic→`regime.lettering()`+shaped runs; DIM magic→regime TextHeight ratios; emit knob bag→value objects; draw `_INK`→derive; layout stringly ELK→typed vocab. RESOLVED across V1/V7/V12/V13/G6. |
| E9 | Dead/stringly. glyphset dead carriers consumed/deleted; draw 7-of-19→19; detail `sheet:str`→`SheetId`; register `code`/`discipline`→`ClassCode`/`Discipline`; receipt stringly `ArtifactKind`+`_KEYS`→derived from the case roster; dxf by-VALUE→by-NAME. RESOLVED. |
| E10 | Ledger drift. ARCH re-synced (§B.3, incl. ARCH:94's "model-asset" wording → "artifacts-origin", §I); `model:15` "ten"→eleven; package codec⇄archive⇄delta cycle broken via `package/bundle`. RESOLVED. |
| E11 | Scene seams. render⇄export cycle broken (SceneTarget hoisted); stage collapse premise REFUTED (KEEP); silhouette/feature-edge/GL2PS `.api` authored + built (V9). RESOLVED. |
| E12 | Output-grade gaps. kind-blind dispatch + closed five-mark + spring defaults + no solar → V15 machinery; running head/TOC/forms/type-scale/overprint/TrimBox → V12/V13/V7. RESOLVED across verdicts. |
| E13 | Track-law debt. 69/45 content_identity, 59 frozendict imports, 54 [RESEARCH], 43 CapacityLimiter + 50 stamina — RESOLVED §K (rebind) + §A.4 (limiter/stamina collapse rides the emit rewire, leg-1b corpus-wide, the full 43+50 surface, not the 3-4 named slice). |

---

## [G] [03] CAPABILITY-TARGET DISPOSITION (every plane row homed)

graphic/vector(+pattern) 7→9.5 (vector split + pattern, 1a); typography 6→9.5 (math/outline/fallback/
metrics owners + shaped_rgba + the V13 type-system surface, 1b); graphic/color 6→9 (rehomed palette + one
rail + measurement-in-derive/exchange-in-managed + LUT/spectral + plate + TAC + the `Color` receipt case,
1a/1b); graphic/raster 5→9 (vocab re-home + real media seams + pyvips hardening + pillow procedural, 1b);
visualization/table 6→9 (pandas floor + pagination + kiwisolver + units + compose-ingress bridge, 2);
visualization/diagram 5.5→9.5 (19-shape + dead carriers + schematic + typed ELK + V15 machinery + V13, 2);
design language V13 —→9.5 (style owner + type-system + sheet-family + derive-backed color + the
regime-bind seam, 1a); export/layered+indesign 8→9.5 (LayerPlan tree + Illustrator lane + PSD blend + IDML
mapping, 1a/3); visualization/chart 7→9 (typed grammar + transform merge + lets-plot, 2); scene 5.5→9
(standard views + silhouette + section/poché + GL2PS + sun + typed mesh, 2); core 6→9.5 (entry real via
core/issue + the pre-run key + derived kind/keys + format dissolved, 1b/3); drawing 6.2→9.5 (V2+V3+V5 +
ISO 286/1101/2553/1302 + SheetId + Reshape + the regime/standard tier split, 1a/3); composition 6.5→9.5
(one fold + ISO 7200 grid + derived sizes + ISSUE completion + page-box, 2); document 7.5→9.5 (typography
seams + value objects + report routing + running heads + TOC + forms + lens OCR/PDF-A + orthographic
hyphens, 2); export/dxf 7→9.5 (paperspace + realized seam + entities + blocks + emit convergence, 3);
specification/delivery 8.5→9.5 (OmniClass + Uniclass + Schematron + ClassCode/Discipline composition +
distribution matrix + acknowledgement + rustworkx DAG, 3).

---

## [H] [04] PACKAGE-ROW DISPOSITION (integration, never removal except the ruled reconciliation rows)

The §D.3 binding map is the disposition: every `[04]` row binds to its named owner at the stated operator
depth; the SHARED-TIER weave binds every production fold; §A.4 lands the concurrency/retry half
corpus-wide. Carried growth folds land with their owner's leg: `document/emit`'s six lowering folds
(V12, leg 2); the V10 chart grammar algebra (leg 2); the `package/archive` `unpack`/`list`/
`BundleManifest` inverse + the parent-keyed `detools` delta row (leg 3, with the bundle cycle break).
pvlib ADMITTED (§D.1); the four removal defaults + zlib-ng re-entry RULED (§D.2); zero prune framing —
the only removals are the brief's own reconciliation rows, each condition-tested.

---

## [I] [06] GATED-OBLIGATION RULINGS (each realized or kept-gated with the blocker named)

- **Gate #1 measured-signals contribution** — UNBLOCKED (runtime track 1 landed). Runtime `[V5]` ships the
  table-keyed domain-histogram recorder. RULING: **REALIZE** — the emit-harvest seam composes the recorder
  off the `ArtifactReceipt._facts` scalars (byte-volume, compression-ratio) keyed by the carried kind at
  `contribute` time, never a local logging fief; the `[METRIC_SIGNALS]` seam on `core/receipt:338`
  realizes in leg 1b. Render duration is NOT a receipt fact (the runtime `Metrics.measured` aspect owns it).
- **Gate #2 outward figure hand-off** — UNBLOCKED (compute track 4 landed). CORRECTION: the artifacts [06]
  + `ARCHITECTURE.md:94` "model-asset case" wording is WRONG — compute `[V2]` (the authority) requires a
  NEW **artifacts-origin `HandoffAxis` case** shipped WITH its self-wired producer; `model_asset` is a
  compute-OWN case and figures cannot ride it (one-producer-per-case law). RULING: **REALIZE via the new
  axis case + producer** — `core/receipt` hosts the `graduates()` producer (imports the compute hub DOWN,
  projects any `ArtifactReceipt` into the case keyed by `ContentIdentity`; the default ceiling a governed
  policy row on artifacts' own carrier, the caller's tighter row the override); the runtime `evidence
  Structural.drift` query stays clean; the wording corrects in [06]/`ARCHITECTURE.md:94`. Producer lands
  leg 1b; the figure/table/chart/scene wiring lands leg 2. |
- **Gate #3 content-keyed output elision** — **KEEP GATED**, blocker named: the C# `Rasm.Persistence`
  reuse fabric (not a py upstream). The WIRING lands now (leg 1b): `Admission(keyed=None)` threads each
  producer's `(ContentKey, Work)` pair into lane admission, and the G2 pre-run input key makes the probe
  real in-session; the durable cross-run short-circuit lands when Persistence does. No silent close.

---

## [J] LEG PARTITION — dependency barriers, self-contained, whole-repo in-run reconcile

Legs are DEPENDENCY BARRIERS over §B.3; the [05] three-wave law is the shape; the ONLY extra split is the
brief-named leg-1 1a/1b ruling — RULED YES: leg 1 carries the campaign's largest blast radius (vector split
+ pattern + typography + color + V13 style + V14 layer + regime split + detect re-wave + marks cycle-break
+ the corpus-wide entry/limiter/V16 rewire), and 1b's corpus-wide `emit()` rewire must bind against settled
1a owners. 1b gated on 1a landing residual-clean. Four legs total; no other split.

- **LEG 1a (FOUNDATIONS — owners)**: `graphic/vector/{path,region,pattern}`, `graphic/color/derive`,
  `graphic/raster/process`, `graphic/marks/mark`, `graphic/style`, `graphic/layer`, `typography` (owner
  structure; the receipt-minting entries land 1b), `drawing/regime` (NEW, the vocabulary slice),
  `exchange/detect` (re-waved). In-run reconcile: `README`/`ARCHITECTURE` domain-map + seam-ledger rewrite
  for the vector split, the layer/regime sitings, the detect re-wave; the V16 rebind on every touched page.
- **LEG 1b (SPINE + ENTRY-CONTRACT rewire — corpus-wide)**: `core/receipt` (23-case roster fixed:
  `Color` in, `ConformanceVerdict` re-homed in, kind/keys derived), `core/plan` (constructing-owner
  refresh + key law), `graphic/color/managed`, `graphic/raster/{io,measure}`,
  `graphic/marks/{encode,decode}`, `typography/{font,shape,layout,math}` entry halves. In-run reconcile
  (whole-repo write authority): the `emit()` rewire on ALL 43 producers with the G2 key re-mint (§A.2);
  the limiter/stamina collapse (§A.4); the Gate #1 recorder seam + Gate #2 `graduates()` producer; the V16
  rebind completion; `core/format` condemned-intact (dissolves leg 2).
- **LEG 2 (MID PLANE)**: `visualization/*` (V15 machinery + solar + pvlib admission + `.api/pvlib.md`),
  `scene/*` (V9 + cycle break + the inversion-e clone deletion), `composition/*` (V7 fold),
  `document/*` (V12 + the `core/format` absorb). In-run reconcile: the table pandas-floor (leg-3
  schedule/register depend on it), the Gate #2 figure wiring, mid-plane `.api` + governance rows.
- **LEG 3 (AEC/EGRESS)**: `core/issue` (NEW — the constructing owner, top of the cone),
  `drawing/{regime-consumers}` = `drawing/{standard,dimension,annotate,symbol,detail,schedule}`,
  `specification/*`, `delivery/*`, `export/{dxf,layered,indesign}`,
  `exchange/{conformance,credential,metadata}` (cold — entries landed 1b; metadata lands the pyexiv2
  removal), `media/*` (cold), `package/{bundle,codec,archive,delta}` (bundle NEW; codec/archive/delta
  cold except the cycle break + zlib-ng re-tag + the two carried growth realizations). In-run reconcile:
  verifies the leg-1b entries, closes the carried growth, the iptcinfo3/xmp-toolkit removals, and the
  flagship review inputs.

Per-leg engine invocation rows (targets repo-relative `libs/python/artifacts/.planning/...`):
```
1a: rebuild {targets:[libs/python/artifacts/.planning/graphic/vector, libs/python/artifacts/.planning/graphic/color/derive.md,
    libs/python/artifacts/.planning/graphic/raster/process.md, libs/python/artifacts/.planning/graphic/marks/mark.md,
    libs/python/artifacts/.planning/graphic/style.md, libs/python/artifacts/.planning/graphic/layer.md,
    libs/python/artifacts/.planning/typography, libs/python/artifacts/.planning/drawing/regime.md,
    libs/python/artifacts/.planning/exchange/detect.md], brief: RASM-PY-ARTIFACTS-DECISION.md}
1b: rebuild {targets:[libs/python/artifacts/.planning/core/receipt.md, libs/python/artifacts/.planning/core/plan.md,
    libs/python/artifacts/.planning/graphic/color/managed.md, libs/python/artifacts/.planning/graphic/raster/io.md,
    libs/python/artifacts/.planning/graphic/raster/measure.md, libs/python/artifacts/.planning/graphic/marks/encode.md,
    libs/python/artifacts/.planning/graphic/marks/decode.md], brief: RASM-PY-ARTIFACTS-DECISION.md}
    # + the corpus-wide emit()/key/limiter/V16 reconcile writes every producer plane
2:  rebuild {targets:[libs/python/artifacts/.planning/visualization, libs/python/artifacts/.planning/scene,
    libs/python/artifacts/.planning/composition, libs/python/artifacts/.planning/document], brief: RASM-PY-ARTIFACTS-DECISION.md}
3:  rebuild {targets:[libs/python/artifacts/.planning/core/issue.md, libs/python/artifacts/.planning/drawing,
    libs/python/artifacts/.planning/specification, libs/python/artifacts/.planning/delivery,
    libs/python/artifacts/.planning/export, libs/python/artifacts/.planning/exchange,
    libs/python/artifacts/.planning/media, libs/python/artifacts/.planning/package], brief: RASM-PY-ARTIFACTS-DECISION.md}
```
No post-leg residual channel: each leg's in-run reconcile has whole-repository write authority and closes
every confirmed residual before it returns. No card governance (IDEAS/TASKLOG dead); findings land as page
rows, seam rows, or gated obligations. Campaign acceptance closes on the §L flagship visual review.

---

## [K] V16 TRACK-REBIND RULING (the band-vs-table split, ruled explicitly)

The register's E13(b) premise "`from builtins import frozendict` is not a CPython builtin — every import
raises" is **REFUTED** on the target interpreter: the import SUCCEEDS (`frozendict.__module__ ==
'builtins'`, PEP 814 Final, py3.15; verified `uv run python`). The V16b ACTION still stands, re-grounded
on `[04]` SHARED-TIER consistency (the `Map` ADT/dispatch spine runtime/data/compute forced on every
sibling), NOT as a bug fix. The rebind splits by ROLE (shapes doctrine OWNER_INDEX row [08] admits BOTH
`frozendict` and `Map` as immutable-evidence owners):

- **V16b-A (import deletion)** — DELETE every `from builtins import frozendict` line (59 sites);
  `frozendict` is a builtin, referenced bare. Satisfies the `rg`-zero gate MECHANICALLY, independent of role.
- **V16b-B (TABLE role → Map)** — every `Final[frozendict[K,V]]` dispatch/policy table (92 `Final[frozendict`
  sites: `_KEYS`, `_METRIC`, `VL_RENDER`, `DELEGATES`, `PLANS`, `FINISHERS`, `_ROUTES`, `_HATCH`,
  `_KIND_POLICY`, …) re-types `Final[Map[K,V]]`, constructed `Map.of_seq(...)`; `Map.try_find`/lookup
  replace `frozendict` access.
- **V16b-C (BAND role → STAYS frozendict)** — `frozendict[...]` as a msgspec `case()` payload slot
  (`receipt:78,80,86,92` scene/preview/media/cad + the NEW `Color` band; `managed:278`;
  `marks/encode:262`/`decode:329` RasterFact band; `dxf:343,357`; `indesign:112`; `imposition:341`) or a
  `msgspec.Struct` field (`detail:227`) STAYS `frozendict` (bare builtin, no import). `expression.Map` is
  NOT msgspec-native — a `case()` payload must be msgspec-encodable and hashable. Deleted-form prose names
  the anti-pattern by the **Map-table-vs-frozendict-band contrast**, never "broken vs working import."

Cross-track record: runtime/data/compute siblings cite the same falsified premise; this blueprint corrects
the justification in artifacts scope and FLAGS the cross-track re-grounding without re-planning siblings.

Acceptance (mechanical, per-page as legs touch): `rg` returns zero under `libs/python/artifacts/.planning/`
for `rasm.runtime.content_identity`, `from builtins import frozendict`, and `[RESEARCH]` headers. Bare
`frozendict[...]` band references survive (V16b-C); they carry no import line and are not the gated form.

---

## [L] ACCEPTANCE DERIVATION — the three flagship construction chains (G8)

Each flagship is a producer-`emit()` graph whose terminal `core/issue` folds into `ArtifactPipeline.of` and
drives through the runtime lane; the campaign closes on their visual review against the [TELOS] bar.

- **[L1] Issued sheet SET** — `issue(IssueRequest(sheet_set=...))`: regime/style/pattern/typography (w1
  substrate) → `drawing/*.emit()` per-drawing nodes (`Drawing`/`Schedule`) → `sheet.SheetSet.emit()` ONE
  node per sheet (cover + drawing-index via the scheduled()→compose→table bridge + general-notes via the
  sheet←emit seam + drawing sheets; parents = placed content keys; title-block grid = a style sheet-family
  row) → `register.emit()` (parents = sheet keys) → `transmittal.emit()` member nodes + ONE aggregate
  (`Transmittal`, parents = member keys) → `ArtifactPipeline.of(nodes, targets={transmittal.key})` → lane
  fronts → per-member elision on re-issue (only changed sheets re-render — the G2 key law's proof).
- **[L2] Portfolio AEC diagram SUITE** — `issue(IssueRequest(diagram_suite=...))`: solar furniture (pvlib
  SPA over `ProjectionKind`) + plan-polygon/area ingress (`data/tabular` typed columns) + scene massing
  egress (V9 silhouette/GL2PS, DATA parents) → `layout.emit-inputs` per kind (SUN_PATH real sun geometry;
  STACKING/PROGRAM/SITE area-proportional; CIRCULATION/SECTION_CALLOUT plan-anchored) → `draw.emit()` ONE
  node per `DiagramKind` (19 shapes, typography-shaped labels, LayerPlan projection, derive ink, style
  theme rows) → pipeline → `Block[ArtifactReceipt.Diagram]`. A node-link render wearing an AEC label fails
  the review — V15 is machinery, not labels.
- **[L3] Editorial document PACKAGE** — `issue(IssueRequest(document_package=...))`: style type-system rows
  + typography seams (w1) → `section.emit()` (`Spec`, authors into the model tree) → `emit.emit()` format
  nodes (typography composed IN arms, running heads via `@page`/`string-set`, cross-arm page-numbered TOC,
  forms, absorbed template-clone rows) → `egress.emit()` finishing terminal (imposition fold, page-box,
  PDF/A) → pipeline targeted at the egress key → a paginated, running-head, TOC'd, forms-capable spec book.

---

## SELF-REPORT (exact counts)

- Page rows authored: **69 live** (60 current − 3 deleted + 12 new) + 3 delete rows = 72 table rows.
- Deleted pages: **3** (`core/format`, `visualization/chart/transform`, `graphic/vector.md`).
- New pages: **12** (`graphic/vector/{path,region,pattern}`, `graphic/style`, `graphic/layer`,
  `graphic/marks/mark`, `typography/math`, `drawing/regime`, `visualization/diagram/{schematic,solar}`,
  `package/bundle`, `core/issue`).
- Grafts applied: **11** (G1-G11; every ≥2-judge graft + four disk-verified single-judge grafts).
- Verdicts disposed: **16** (V1-V16, §E). Evidence rows disposed: **13** (E1-E13, §F).
- Package rows ruled: **8 census rows + 1 pvlib admission + the full [04] binding map** (§D).
- Inversions disposed: **5** (a re-home-into-receipt / b regime split / c graphic/layer / d detect re-wave
  / e edge removal, §B.1); cycles broken: **3** (§B.2).
- Gated obligations ruled: **3** (#1 realize, #2 realize via the artifacts-origin axis case + wording fix,
  #3 wiring-realized/elision-gated with the blocker named) (§I).
- Legs: **4** (1a/1b/2/3, §J); constructing owner: **1** (`core/issue`, leg 3 — the receipt roster fixed
  at 23 with `Color` in and `ConformanceVerdict` re-homed).
- Carrier conventions collapsed: **6** (11 concrete shapes → 1 `emit() -> ArtifactWork | Iterable`, §A.2).
- Re-verified merge properties: disposition-complete (V/E/[03]/[04]/[06] all tabled), acyclic (§B.3 —
  every edge within-wave or earlier; core/issue at the cone top; no scene→package import), entry-complete
  (43 producers → one contract; the constructing owner named; receipt case per producer), tier-named
  (generate: vector/pattern/symbol/solar/marks-encode/shape/math; bind: regime/layer/sheet/schedule;
  select: style/managed), integration-first (zero prune framing; every census row condition-tested; every
  admission bound to an owner with its `.api` obligation).
