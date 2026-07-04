# DRAFT — ENTRY-SEAM-FIRST structural blueprint (RASM-PY-ARTIFACTS)

Lens: work BACKWARD from the `ArtifactWork` entry contract and the corrected seam ledger to the page
structure that makes the one producer contract real. Thesis: `core/plan` already IS the CPM engine and
`ArtifactPipeline.of` already normalizes `ArtifactWork | Iterable[ArtifactWork]` — the defect is not the
engine, it is that (1) NO page constructs it, (2) 43 producers return 11 incompatible carrier shapes across
6 conventions, none `emit() -> ArtifactWork`, and (3) four wave-1←wave-3 import inversions plus three
cycles make the leg graph non-acyclic. Fix the contract and the graph first; the page structure follows
from "who emits a node, who constructs the pipeline, who owns the vocabulary a node's parents key on."

Disk truth this draft is built on (re-verified): `core/plan.md` `ArtifactWork(key, work, parents,
admission, cost)` is the target node UNCHANGED; `Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]`;
`ArtifactPipeline.of(works, *, lane, warm, targets)` normalizes lone/iterable at `:449`. E13 census EXACT
on disk: 69 `content_identity` / 45 full paths / 59 `from builtins import frozendict` / 54 `[RESEARCH]` /
43 `CapacityLimiter(` mints / 50 bare `stamina.` refs. `from builtins import frozendict` SUCCEEDS on the
target interpreter — `frozendict.__module__ == 'builtins'` (PEP 814, py3.15) — the register's "every import
raises" premise is REFUTED; the V16b action stands on SHARED-TIER `Map`-spine consistency, not as a bug fix.

---

## [A] THE `ArtifactWork` ENTRY CONTRACT AS LAW (V6)

### [A.1] The one producer contract

`core/plan`'s `ArtifactWork` node is authoritative and UNCHANGED. The SEAM_AND_ENTRY_LAW
`emit() -> ArtifactWork | Iterable[ArtifactWork]` is the binding producer signature; `core/plan`'s stale
prose calling the producer method `emit`/`_emit` "the work thunk" is corrected by this rewire. Every
producer page lands exactly this pair:

```python
# NEW uniform entry on every producer — returns the NODE(s), never a receipt, never a bare key, never a batch.
def emit(self, /) -> ArtifactWork | Iterable[ArtifactWork]:
    return ArtifactWork(
        key=self._key,                    # the ContentIdentity.of(...) key the producer already mints
        work=self._emit,                  # the RENAMED render thunk — Work[ArtifactReceipt], plan never invokes it
        parents=self._parents,            # upstream ContentKeys (bound-figure keys, source-artifact keys)
        admission=Admission(keyed=None),  # keyed default; retried only for a whole-producer transient
        cost=self._cost,                  # render-time weight the CPM forward pass sums
    )

async def _emit(self, /) -> RuntimeRail[ArtifactReceipt]:   # the producer's existing render, renamed private
    ...  # returns the in-band ArtifactReceipt whose .slot IS self._key; a failed production folds to the rail fault
```

`_emit` is a bound zero-arg coroutine — it satisfies `Work[ArtifactReceipt]` directly, no lambda wrapper.
The receipt carries its own key (`ArtifactReceipt.slot`), so `emit` never returns a `(key, receipt)` pair.
Batch producers return `Iterable[ArtifactWork]` — one node per member with per-member keys (MODAL_ARITY:
the batch is the iterable arity of `emit`, per-member elision stays per-member).

### [A.2] The 11-shape → 1-contract collapse (43 producers, disk-verified entries)

Every current entry, its convention, and its transcription. The `_emit` thunk column is the RENAMED
private method; the receipt case is the in-band `ArtifactReceipt` case its rail resolves to.

| Convention (brief's 6) | Current shape (disk) | Pages | Collapse |
|---|---|---|---|
| in-band receipt (converged half) | `RuntimeRail[ArtifactReceipt]` | `export/layered:256`, `indesign:302`, `color/managed:255`, `chart/export:247`, `table:664`, `register:666`, `transmittal.emit:321` | rename current `emit`→`_emit`; add `emit()->ArtifactWork`. The module-level batch driver was already killed (E1 proof); this lands the node builder over the surviving per-instance thunk. |
| bare key + weave | `RuntimeRail[ContentKey]` | `compose:387`, `imposition:509`, `sheet:531`, `egress:498`, `lens:249`, `tagged:258`, `dxf:1067`, `section:626`, `shape:265`, `layout:153` | `_emit` returns `RuntimeRail[ArtifactReceipt]` — the receipt whose `.slot` IS the key returned bare today. `shape`/`layout` mint the `Document` case (axis catalog / positioned run / broken stream); `egress`/`tagged` `Egress`; `lens` `Introspection`; `section` `Spec`; `dxf` `Cad`; `compose` `Preview`; `imposition` `Egress`; `sheet` `Egress`/`Pdf`/`Preview`. |
| key+receipt tuple | `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` | `scene/render:431`, `font.engineer:263` | drop the redundant leading key (receipt.slot carries it); `_emit -> RuntimeRail[ArtifactReceipt]` (`Scene`/`Document`). |
| key+Evidence/Verdict tuple | `RuntimeRail[tuple[ContentKey, X]]` | `conformance.close:598` (Verdict), `credential.close:522` (CredentialEvidence), `metadata.of:305` (MetaFacts,bytes), `transmittal.close:244` (TransmittalEvidence) | `_emit -> RuntimeRail[ArtifactReceipt]` minting the case that carries X's facts (`Verdict`-band/`Credential`/`Metadata`/`Transmittal`); `close`/`of` collapse into `_emit`, the caller reads facts off the receipt. |
| layers+receipt / bytes+receipt / artifact+receipt | `RuntimeRail[tuple[tuple[Layer,...] \| bytes \| DrawArtifact, ArtifactReceipt]]` | `annotate:167`, `detail:300`, `dimension:212`, `symbol:157` (layers); `schedule:196` (bytes); `draw:97` (DrawArtifact) | the leading payload projects INTO the V14 `LayerPlan` (drawing/diagram producers project layer trees, not return tuples); `_emit -> RuntimeRail[ArtifactReceipt]` (`Drawing`/`Schedule`/`Diagram`). |
| Result-wrapped media pair | `RuntimeRail[Result[tuple[ContentKey, ArtifactReceipt], MediaFault]]` | `container.encode:235`, `timeline.render:100`, `subtitle.produce:163`, `analysis.produce:154`, `synthesis.produce:163` | the inner `Result[..., MediaFault]` DIES — `Work[ArtifactReceipt]` forbids it; `MediaFault` folds into the `RuntimeRail` fault at `async_boundary` (a failed production is the runtime spine's `rejected` line, receipt.md:5). `_emit -> RuntimeRail[ArtifactReceipt]` (`Media`). |
| Block-wrapped batch | `RuntimeRail[Block[Result[ArtifactReceipt, Fault]]]` | `marks/encode.of:525`, `raster/io.of:447` | `emit -> Iterable[ArtifactWork]` — one node per member; each member's `_emit -> RuntimeRail[ArtifactReceipt]`, per-member `Fault` on its own rail. The `Block[Result]` batch DIES. |
| module-level batch rail | `RuntimeRail[Block[ContentKey]]` | `emit.produced:538`, `report.rendered:809` | `emit -> Iterable[ArtifactWork]` per plan; the module batch driver DIES; the pipeline CONSTRUCTION moves to the constructing owner (§A.3). |
| parallel receipt rail (V4 violation) | `RuntimeRail[ColorReceipt]` | `color/derive.derive:473` | DELETED — `derive` is a substrate (returns color values, no receipt); `ColorReceipt`/`ColorReceiptWire` (`derive:432,458`) removed, not collapsed-as-a-case. |
| substrate (correct) | `RuntimeRail[Block[VectorResult]]` | `vector.of:771` | unchanged in kind — `vector/*` mints no receipt; split into `graphic/vector/{path,region,pattern}` (V1/V2), all NONE-entry. |

Six legacy carriers collapsed: the batch driver (`produced`/`rendered`/`_fanned`), the `Block[Result]`
batch, the `tuple[ContentKey, X]` pairs, the `layers/bytes/artifact + receipt` tuples, the media
`Result` wrapper, and the `ColorReceipt` rail. Every survivor is `emit() -> ArtifactWork | Iterable`.

### [A.3] The constructing owners — `ArtifactPipeline` becomes REAL

`core/plan` is the ENGINE, never the constructor. THREE fences build `ArtifactPipeline.of(<emit nodes>)`;
each is a composite producer at the top of its dependency cone (imports members downward, no inversion):

1. **`composition/sheet` `SheetSet.issue()`** — the SHEET-SET flagship. Gathers each member sheet's
   `emit()` node + the drawing-index sheet + schedule + register nodes, `ArtifactPipeline.of(<nodes>,
   targets=<changed members>)`. Multi-artifact arity: one node per sheet with per-member keys, so a
   re-issued set re-renders only changed members. The `Egress`/`Pdf` sheet receipts are the members;
   the index sheet is a leaf.
2. **`document/emit` `DocumentPackage`** (absorbs `core/format` `TemplatePipeline`) — the EDITORIAL
   DOCUMENT-PACKAGE flagship. Binds one `DocumentNode` context to N format `emit()` nodes,
   `ArtifactPipeline.of(<format nodes>)`. Replaces the dead `produced`/`_fanned` batch rails: the
   one-context-many-format fan-out that `TemplatePipeline.bound()` did as a second scheduling path is now
   the pipeline's own fronts.
3. **`delivery/transmittal`** — the ISSUE flagship. Emits the aggregate `Transmittal` node whose `parents`
   are the member sheet keys (receipts are terminal — the aggregate carries facts, never a partial
   receipt); internally composes the sheet-set sub-pipeline + conformance + credential + archive nodes.

`core/plan`'s `[CONTENT_KEY_NODES]` producer list is refreshed to name these three as the constructing
owners; the plan imports no producer (the `Work` thunk carries the call), so no producer→plan→producer
cycle forms.

### [A.4] The limiter/stamina collapse rides the entry rewire (leg-1 corpus-wide)

The 43 `CapacityLimiter(` mints + 50 bare `stamina.` refs are the SAME leg-1 reconcile as the `emit()`
rewire, not a 3-4 site patch (upstream-law finding 1, api-census [2] — the register's 3-4 named anchors
are a thin slice of the corpus surface). Every folder-minted `CapacityLimiter`/`stamina.AsyncRetryingCaller`
dies; each `_emit` thunk's native/subprocess seam rides `lanes.offload(kernel, retry=RetryClass.OCCT)`
under the runtime-owned bound (the ORACLE row for exchange/conformance oracle verdicts). A `keyed` node
with internal offload-retry keeps `keyed` and rides `LanePolicy.offload(retry=...)` inside `_emit` (per
`core/plan [ADMISSION_LOWERING]`); `retried` admission is reserved for a whole-producer transient. The
media plane's `_WORKER_RETRY` (`container:3`, 6 pages) and `detect:11` fold onto the same export.

---

## [B] THE CORRECTED SEAM LEDGER (acyclic, 4 inversions disposed, 3 cycles broken)

The leg partition is a topological order over this ledger. Every cross-domain import points within-wave or
to an earlier wave; the four live inversions and three cycles are disposed below. Wave assignments in §C.

### [B.1] The four wave-1←wave-3 inversions — each RE-WAVED or RE-HOMED (never frozen-in-place)

- **(a) `core/receipt` ← `exchange/conformance.ConformanceVerdict`** (E10-adjacent, aec-egress [02.8]).
  RULING: **re-home below receipt via band collapse.** `ArtifactReceipt.verdict` becomes
  `tuple[ContentKey, frozendict[str, float | str]]` — a native-scalar evidence band exactly like
  `scene`/`media`/`cad`/`preview`. `core/receipt` imports ZERO producer modules (the union's sole producer
  import removed). `exchange/conformance` keeps `ConformanceVerdict` as its OWN typed value object and
  flattens `ConformanceVerdict.facts()` into the band at `ArtifactReceipt.Verdict(key, facts)` mint time.
  Consumers needing the typed verdict (`document/tagged`) read it from conformance directly (a wave-3
  intra-plane edge), never off the receipt. This ELIMINATES the edge (not merely freezes it): a wave-1
  page importing a wave-3 page is a foundations-first violation regardless of acyclicity, so freeze-in-place
  is rejected. Alternative (freeze the shape as a wave-1 contract) is the lighter fallback if type
  preservation at the receipt seam is later ruled load-bearing — but the band matches the union's own
  pattern and is strictly stronger on the partition axis.

- **(b) `composition/sheet` ← `drawing/standard` (`ScaleRatio`/`SheetId` + V3 lettering + V13 pens)**
  (mid-plane dossier, [05]b). RULING: **re-wave `drawing/standard` (whole substrate) to WAVE 1 / leg 1a.**
  `standard` mints no receipt and composes only wave-1 owners — `graphic/color/derive` (`rgb`),
  `graphic/vector/pattern` (`hatch`), `graphic/vector/path` (geometry), `typography` metrics (`lettering`)
  — so it is topologically foundational, wave-3 only by topical grouping. As a wave-1 substrate (exactly
  like `classify`/`glyphset`), sheet (w2), the drawing producers (w3), and `dxf` (w3) all import it
  downward; V13 `graphic/style` (w1) SELECTS its regime rows within-wave; V14 `LayerPlan` (w1) reads its
  layer-field vocabulary within-wave. `drawing/` splits across waves (standard w1, producers w3) — legal,
  waves are dependency barriers not folder groupings.

- **(c) six-page `export/layered.Layer` upward fan** (V14, aec-egress [02.7], [05]c). RULING: **re-home
  `LayerPlan` to a foundational owner via a folder split.** `export/layered.md` SPLITS into
  `export/layered/plan.md` (the `LayerPlan` semantic-tree vocabulary + ISO 13567 naming — WAVE 1) and
  `export/layered/write.md` (the PSD/OCG/SVG/IDML/ORA/layered-TIFF writers that COMPOSE the tree — WAVE 3).
  The six projectors (`compose`, `imposition`, `sheet`, `diagram/draw`, `marks/encode`, drawing plane)
  import `LayerPlan` from `plan` (w1) downward; the upward fan becomes six downward edges. `write` (w3)
  composes the tree, never owns it.

- **(d) `graphic/raster/io` ← `exchange/detect`** ([05]d, aec-egress [02.8]). RULING: **re-wave
  `exchange/detect` to WAVE 1 / leg 1a.** `detect` is the ingest-boundary format-ID substrate `io` consumes
  (io delegates format detection to detect); it mints no receipt and composes only `puremagic`/`python-magic`
  + runtime lanes. As a wave-1 substrate, `io` (w1) imports it within-wave, and the document/raster/Office
  consumers import it downward. `exchange/` splits across waves (detect w1, conformance/credential/metadata
  w3) — legal.

### [B.2] The three cycles — each broken via a neutral shared-vocabulary site

- **marks `encode`⇄`decode`** (E7, `encode:55,60`⇄`decode:33`). NEW `graphic/marks/mark.md` (w1) owns the
  shared vocabulary (`Symbology`, `DecodeSource`, `MarkFault`, `MarkPayload`); `encode` and `decode` both
  import `mark`, neither imports the other. The duplicate `RasterFact` (`io:321` + `encode:258`)
  consolidates to `graphic/raster/io`'s canonical (encode imports it from io, w1 within-wave).
- **scene `render`⇄`export`** (E11, `render:34-38` lazy ↔ `export` imports render). Hoist `SceneTarget`
  into `scene/render` (render owns the target vocabulary its `SceneOp.Export` keys); `export` composes
  render's `SceneTarget` downward, render no longer imports export. Cycle broken (leg 2).
- **package `codec`⇄`archive`⇄`delta`** (E10, `codec:37-38` eager private-worker reach ↔ `archive:92`/
  `delta:30` lazy-back). NEW `package/bundle.md` (w3) owns `Bundle`/`CompressionAlgo`/`CodecProfile`/
  `BundleManifest`; codec/archive/delta each compose `bundle`, the eager private-worker imports die, the
  cycle breaks (leg 3, landing with the two carried growth realizations).

### [B.3] The corrected ledger — every cross-domain edge, direction = IMPORT direction

Format: `consumer  ←/→  owner  # [TAG] anchor`. `→` = consumer imports owner. All edges point within-wave
or earlier. Cross-branch data edges (`data/tabular`, `compute/*`, `geometry/mesh`, runtime) are ingress, not
artifacts imports, and carry no wave constraint. New/changed edges vs `ARCHITECTURE.md:98-222` are flagged.

```
# --- WAVE 1 internal (foundations) ---
graphic/color/derive          →  graphic/color/derive          # measurement folded in (V4); no ColorReceipt rail
graphic/color/managed         →  graphic/color/derive          # [ADAPT] AdaptMethod owned by derive, managed composes (E6)
graphic/color/managed         →  graphic/color/derive          # [DERIVE] tone-curve/space provenance (ARCH:105, now real)
graphic/vector/region         →  graphic/vector/path           # boolean/offset over the path algebra
graphic/vector/pattern        →  graphic/vector/path           # [OWNER_BOUNDARIES] pattern composes vector, not marks (V2)
graphic/vector/pattern        →  graphic/vector/region         # pathops clip lowering for real clipped hatch geometry
graphic/marks/encode          →  graphic/marks/mark            # NEW neutral vocab (cycle break)
graphic/marks/decode          →  graphic/marks/mark            # NEW neutral vocab (cycle break)
graphic/marks/encode          →  graphic/raster/io             # RasterFact consolidated to io's canonical (E6 dup)
graphic/marks/encode          →  graphic/vector/path           # [GEOMETRY] mark geometry over vector (tier: generate)
graphic/raster/io             →  exchange/detect               # [DETECT] detect RE-WAVED to w1 — inversion (d) DISPOSED
graphic/raster/process        →  graphic/raster/io             # shared TransformInput substrate
graphic/raster/measure        →  graphic/raster/process        # produced-vs-measured axis
typography/shape              →  typography/font                # [SHAPE] face/variation + embed-audit precondition (ARCH:152)
typography/layout             →  typography/shape               # reads break-safety/tatweel flags
typography/math               →  typography/shape               # NEW ziamath owner composes shape's outline (V3)
typography/math               →  typography/font                # NEW math-font metrics
graphic/style                 →  graphic/color/derive           # NEW V13 — color system resolved through derive
graphic/style                 →  drawing/standard               # NEW V13 — SELECTS regime pen/lettering rows (standard now w1)
graphic/style                 →  typography/shape               # NEW V13 — feature presets over FeatureSpec
graphic/style                 →  typography/layout              # NEW V13 — leading/tracking over break correctness
graphic/style                 →  typography/font                # NEW V13 — variable-axis INSTANCE/FREEZE per role
drawing/standard              →  graphic/color/derive           # [DERIVE] discipline sRGB (ARCH:119, standard now w1)
drawing/standard              →  graphic/vector/pattern         # [HATCH] HatchMaterial -> PatternSpec (V2)
drawing/standard              →  graphic/vector/path            # ISO 3098 lettering geometry
drawing/standard              →  typography/shape               # [METRICS] cap/x-height via font-metrics (V3, kills fontTools leak)
export/layered/plan           →  drawing/standard               # NEW LayerPlan reads ISO 13567 layer-field vocab (V14)
core/plan                     →  core/receipt                   # ArtifactReceipt the Work payload (UNCHANGED)
core/receipt                  →  (nothing)                      # verdict->band; ConformanceVerdict edge DISPOSED — inversion (a)
# --- WAVE 2 internal + into WAVE 1 ---
composition/compose           →  graphic/vector/path            # [GEOMETRY] svgelements placement (ARCH:115)
composition/compose           →  export/layered/plan            # [LAYERED] projects into LayerPlan (V14, was flat Layer — inversion c)
composition/imposition        →  export/layered/plan            # [LAYERED] projects into LayerPlan (V14 — inversion c)
composition/sheet             →  drawing/standard               # [SCALE] ScaleRatio/SheetId (standard now w1 — inversion b DISPOSED)
composition/sheet             →  export/layered/plan            # [LAYERED] frame/figures as LayerPlan rows (V14 — inversion c)
composition/sheet             →  graphic/vector/path            # hand-built affines routed through vector (V7)
composition/sheet             →  graphic/style                  # [THEME] title-block grid VARIANTS = sheet-family rows (V13)
composition/sheet             →  typography/shape               # [SHAPE] lettering() + shaped runs, kills Helvetica magic (V3/E8)
composition/sheet             →  typography/layout              # general-notes line break
composition/sheet             →  visualization/table            # [TABLE] SheetSet.scheduled TablePlan (ARCH:159)
composition/sheet             →  document/emit                  # [NOTES] sheet<-emit general-notes typesetting seam (V7, was prose)
composition/sheet             →  core/plan                      # SheetSet.issue constructs ArtifactPipeline (§A.3 flagship)
visualization/table           →  graphic/style                  # [THEME] table style rows (V13)
visualization/chart/spec      →  graphic/color/derive           # [PALETTE] Palette/hex_ramp REHOMED from spec into derive (V4/E2)
visualization/chart/spec      →  graphic/style                  # [THEME] chart theme rows bind rcParams/ThemeConfig (V13)
visualization/chart/export    →  graphic/color/derive           # rewired from spec-alias to derive (V4/E2)
visualization/chart/export    →  visualization/chart/spec       # folds the chart case (transform MERGED in — V10)
visualization/diagram/draw    →  graphic/color/derive           # [INK] _INK becomes derive contrast pick (V4/E8)
visualization/diagram/draw    →  typography/math                # NEW — routes formula through math owner, not local ziamath (V3/E3)
visualization/diagram/draw    →  typography/shape               # NEW — label outlining through shape, not local ziafont (V3/E3)
visualization/diagram/draw    →  export/layered/plan            # [LAYERED] named-layer projection into LayerPlan (V14 — inversion c)
visualization/diagram/draw    →  graphic/style                  # [THEME] diagram aesthetics rows (V13)
visualization/diagram/layout  →  visualization/diagram/glyphset # positioned marks (adds area/polygon case — V15)
visualization/diagram/layout  →  visualization/diagram/solar    # NEW — sun-path furniture geometry (V15)
visualization/diagram/schematic→  visualization/diagram/glyphset # NEW schemdraw catalog beside the five marks (V10)
visualization/diagram/solar   →  graphic/vector/path            # NEW — sun-path arc geometry over vector (V15, tier generate)
scene/export                  →  scene/render                   # SceneTarget hoisted to render — cycle DISPOSED
scene/export                  →  package/bundle                 # composes bundle repro-ZIP not zlib_ng clone (E-zlib); bundle RE-WAVED to w2 so this is within-wave (else w2->w3 inversion)
document/emit                 →  document/model                 # lowers FROM the tree (ARCH:13)
document/emit                 →  typography/shape               # [SHAPE] E7 seam now COMPOSED (V3/E7, was zero arms)
document/emit                 →  typography/layout              # [LAYOUT] LineBrokenRun + pyphen orthographic emission (V3/V12)
document/emit                 →  typography/font                # [FONT] FONT_EMBED subset/instance (ARCH:151)
document/emit                 →  graphic/color/managed          # [MANAGED] ICC-profiled raster (ARCH:107)
document/emit                 →  graphic/style                  # [THEME] page-master rows + running heads (V13)
document/emit                 →  export/layered/plan            # OCG layer tree for layered PDF (V14)
document/emit                 →  core/plan                      # DocumentPackage constructs ArtifactPipeline (§A.3 flagship)
document/report               →  document/emit                  # terminal PDF routes through emit — Pdf.from_html dup dies (V12/E6)
document/report               →  document/model                 # composes into the tree
document/tagged               →  document/model                 # StructureNode tree (ARCH:178)
document/lens                 →  document/model                 # recovers TO the tree
document/egress               →  composition/imposition         # egress composes imposition's ONE press fold (V7/E6)
# --- WAVE 3 internal + into WAVE 1/2 ---
drawing/dimension             →  drawing/standard               # [DIMSTYLE] (ARCH:120, standard w1)
drawing/dimension             →  drawing/symbol                 # V5 mark geometry owner (terminator/proportion rows)
drawing/dimension             →  typography/math                # ziamath tolerance math via owner (V3/E3)
drawing/dimension             →  typography/shape               # ISO 3098 shaped run (V3/E3, kills local ziafont)
drawing/dimension             →  graphic/vector/path            # [VECTOR] metric point-at-distance for tick spacing (V1)
drawing/dimension             →  graphic/color/derive           # discipline pen (V4, kills literal ink)
drawing/dimension             →  export/layered/plan            # [LAYERED] dimension layer rows (V14)
drawing/annotate              →  drawing/symbol                 # [STYLE] SymbolStyle + V5 terminator geometry (ARCH:133)
drawing/annotate              →  drawing/standard               # [STANDARD] (ARCH:134)
drawing/annotate              →  typography/layout              # [LAYOUT] Knuth-Plass notes (ARCH:135, kept)
drawing/annotate              →  typography/shape               # [SHAPE] consumes POSITIONS not just clusters (V3/E3)
drawing/annotate              →  typography/math                # formula notes via owner (V3)
drawing/annotate              →  graphic/vector/region          # scallop-band offset (ARCH:137)
drawing/annotate              →  graphic/color/derive           # palette REWIRED spec->derive (V4/E2/E7, was unledgered :49)
drawing/symbol                →  graphic/vector/path            # [GEOMETRY] mark geometry (V5, replaces schemdraw arrow= + numpy trig)
drawing/symbol                →  graphic/vector/region          # rasterize for sheet-cell seam
drawing/symbol                →  drawing/standard               # owned ISO vocab
drawing/symbol                →  graphic/color/derive           # palette REWIRED spec->derive (V4/E2)
drawing/detail                →  drawing/symbol                 # [BUBBLE] (ARCH:129)
drawing/detail                →  drawing/standard               # [STANDARD] SheetId-typed refs (E9, was stringly :80)
drawing/detail                →  graphic/vector/path            # area-weighted centroid (V1, replaces vertex-mean)
drawing/detail                →  graphic/color/derive           # palette REWIRED spec->derive (V4/E2, resolves prose-vs-fence)
drawing/schedule              →  visualization/table            # [LOWER] TablePlan.build (ARCH:143, pandas floor lands leg2)
drawing/schedule              →  drawing/standard               # [STANDARD] legend swatches (ARCH:142)
drawing/schedule              →  graphic/vector/pattern         # legend swatch geometry (V2, kills crosshatch trig + invented hex)
drawing/schedule              →  graphic/color/derive           # swatch color (V4, kills invented hex :355)
specification/section         →  document/model                 # [MODEL] (ARCH:146)
specification/section         →  specification/classify         # [CLASSIFY] ClassCode (ARCH:147)
specification/section         →  visualization/table            # [TABLE] QTO citation frame (ARCH:148)
specification/classify        →  drawing/standard               # [DISCIPLINE] (ARCH:150, standard w1)
delivery/register             →  composition/sheet              # [SHEET] SheetEntry from_title_block (ARCH:207, downward w3<-w2)
delivery/register             →  specification/classify         # ClassCode composition kills parallel ClassificationSystem (E6/E9)
delivery/register             →  drawing/standard               # Discipline composition (E9, was bare-str :352)
delivery/register             →  visualization/table            # [TABLE] Index render (ARCH:208)
delivery/register             →  rustworkx                      # DAG assembly ordering (V-pkg, [04] rustworkx)
delivery/transmittal          →  composition/sheet              # sheet-set members (§A.3 issue flagship)
delivery/transmittal          →  composition/imposition         # [IMPOSE] (ARCH:210)
delivery/transmittal          →  package/archive                # [ARCHIVE] (ARCH:211)
delivery/transmittal          →  package/codec                  # [CODEC] Bundle — LEDGERED (E7, was unledgered :58)
delivery/transmittal          →  exchange/conformance           # [CONFORMANCE] (ARCH:212)
delivery/transmittal          →  exchange/credential            # [CREDENTIAL] (ARCH:213)
delivery/transmittal          →  delivery/register              # [REGISTER] (ARCH:214)
delivery/transmittal          →  core/plan                      # constructs the issue sub-pipeline (§A.3 flagship)
export/dxf                    →  drawing/standard               # [SEED] Dxf.New composes Standard.seed (V11, was own doc)
export/dxf                    →  graphic/vector/path            # [VECTOR] make_path/flatten bridge (ARCH:168, now one direction)
export/layered/write          →  export/layered/plan            # composes the LayerPlan tree (V14)
export/layered/write          →  composition/compose            # [COMPOSE] placed layout (ARCH:163)
export/indesign               →  export/layered/plan            # IDML layers = LayerPlan (V14)
export/indesign               →  composition/compose            # [TEMPLATE] placed layout (ARCH:165)
exchange/conformance          →  document/emit                  # [DOCUMENT] emitted PDF (ARCH:175, w3<-w2)
exchange/conformance          →  document/tagged                # [ACCESS] structural result (ARCH:176)
exchange/conformance          →  typography/font                # embed-audit
# ARCH:179 document/tagged->exchange/conformance [SIGN] is DATA-FLOW, not an import: the import is
# conformance->tagged above (w3<-w2), so no tagged->conformance edge exists — no w2->w3 inversion.
package/codec                 →  package/bundle                 # NEW neutral vocab (cycle break), bundle w2
package/archive               →  package/bundle                 # NEW neutral vocab (cycle break), bundle w2
package/delta                 →  package/bundle                 # NEW neutral vocab (cycle break), bundle w2
media/* (7)                   →  media/container|filtergraph    # spine composition (ARCH:183-206, cold)
media/subtitle                →  typography/shape               # [SHAPE] shaped_rgba EXPORTED (E3, was phantom :50)
media/analysis                →  graphic/raster/io              # render_png/montage EXPORTED bare (E5, was phantom)
media/analysis                →  graphic/raster/measure         # frame_similarity EXPORTED bare (E5, was phantom)
```

NOTE on the two data-flow-vs-import seams: (1) ARCH:179 `document/tagged → exchange/conformance [SIGN]` and
(2) ARCH:159-160 `composition/sheet → delivery/register [REGISTER]` are DATA-flow arrows, not imports. In
both, the OWNER imports the SOURCE downward — conformance (w3) imports tagged (w2); register (w3) imports
sheet's SheetSet (w2) via `from_title_block`. The producer projects plain tuples (`registered()`, the
structural result) with no upward import. So neither adds a w2→w3 edge; both are proper w3←w2 downward
imports already in the ledger. The ledger's `→` is IMPORT direction throughout; ARCH's mixed `→`/`←` are
data-flow annotations reconciled to import direction here.

The whole import graph is acyclic: every edge above points within-wave or to an earlier wave; the four
inversions are re-waved/re-homed, the three cycles broken at neutral sites.

---

## [C] THE FULL PAGE-SET (67 pages: 60 current +7 net; per-row schema)

Legend: Action+Lowering uses the engine vocabulary — KEEP→`improve`; REBUILD→`rebuild`; NEW→`new`;
SPLIT→N×`new` + `delete{into,from}`; MERGE→`improve`(absorber) + `delete{into,from}`; MOVE→`new`(dest) +
`delete{into,from}`; DELETE→`deletePages`. Tier per `[OWNER_BOUNDARIES]` (generate/bind/select or N/A).
Entry: `emit()` (receipt case) for producers, NONE for substrate/vocabulary. Seams: see §B (anchor per
edge). Wave/leg per §B waving.

### [C.1] LEG 1a — foundational OWNERS (vocabulary/substrate rebuilds; the emit rewire is 1b)

| Path | Action + lowering | Tier — charter / skeleton | Entry | Wave/leg |
|---|---|---|---|---|
| `graphic/vector/path.md` | NEW `new` (+ `delete{into:region, from:vector.md}` bulk) | generate — svgelements parse/query/affine/measure/sample + **metric point-at-distance** (arc-length, kills parametric-t tick spacing V1), **polyline decimation/simplify**, **area-weighted centroid** (V1, detail stops vertex-mean), **`Length.to_mm` unit egress** (V1), text-on-path. Owner `Path`; `PathOp`/`PathResult` closed families; tolerances lifted to `_TOLERANCE` policy row (E8 `0.1/0.25/ppi=96.0/1e-3`). | NONE (substrate) | 1/1a |
| `graphic/vector/region.md` | NEW `new` (absorbs `vector.md` bulk: `delete{into:region, from:graphic/vector.md}`) | generate — skia-pathops boolean/offset/stroke/warp/wind/region/contains + serialization (`drawsvg.Drawing/Group/Raw` replaces f-string SVG V1) + resvg raster egress. Owner `Region`; policy-lifted flatten tolerances. | NONE | 1/1a |
| `graphic/vector/pattern.md` | NEW `new` (V2) | generate — REPEATING fill geometry over path/region: `StrokeFamily`/`PatternSpec`/`DensityLaw` (brief shape), THREE lowerings (ezdxf `set_pattern_fill`, `drawsvg.Pattern`, pathops-clipped geometry). `HatchMaterial` = seed rows binding material→`PatternSpec` per ISO 128-50/ANSI/BS regime. | NONE | 1/1a |
| `graphic/color/derive.md` | REBUILD `rebuild` | generate — colour-science + ColorAide derivation + **measurement** (28-member `Metric` deltaE/contrast/CVD/WCAG kept, not split — one colour-science read owner) + **rehomed `Palette`/`hex_ramp`** (MOVE from chart/spec). Owns `AdaptMethod` (E6, managed composes). `ColorReceipt`/`ColorReceiptWire` DELETED (V4). | NONE (substrate) | 1/1a |
| `graphic/color/managed.md` | REBUILD `rebuild` | select/egress — ICC/LUT/CCTF egress (pyvips) + **CxF3 spot intake** (colour-cxf, from derive's exchange concern) + **Separation/DeviceN plate authoring** (pikepdf raw object model, consumes SpotChannel, V4) + **TAC policy gate** (limit row + overprint/rich-black rows) + LUT authoring (`write_LUT`). Composes derive's `AdaptMethod`. | `emit()` (`Preview` + plate band) | 1/1b |
| `graphic/raster/io.md` | REBUILD `rebuild` (V8) | N/A (working surface) — `Raster` owner, 15 `RasterOp`, 139-member `Transform` vocab; **exports bare `render_png`/`montage`** (E5 media home); **pyvips `block_untrusted_set` hardening + `Source`/`Target` streaming** (V8 deferred→landed); canonical `RasterFact` (marks consolidates here). tifffile GeoTIFF/`aszarr`/`memmap`/`validate_jhove` ([04]). | `emit()` per-member (`Preview`) | 1/1b |
| `graphic/raster/process.md` | KEEP `improve` | N/A — 97-member produced-raster engine; pillow ImageChops/ImageMath/gradients ([03]). | NONE (substrate) | 1/1a |
| `graphic/raster/measure.md` | KEEP `improve` | N/A — 42-member measure engine; **exports bare `frame_similarity`** (E5 SSIM helper). | `emit()` (`Preview`) | 1/1b |
| `graphic/marks/mark.md` | NEW `new` (cycle break) | N/A (vocabulary) — shared `Symbology`/`DecodeSource`/`MarkFault`/`MarkPayload` both encode+decode import. | NONE | 1/1a |
| `graphic/marks/encode.md` | REBUILD `rebuild` | generate — segno/python-barcode/zxing encode over mark geometry; imports `mark` + io's `RasterFact` (cycle+dup fixed E6/E7). | `emit()` per-member (`Preview`) | 1/1b |
| `graphic/marks/decode.md` | KEEP `improve` | N/A — zxing decode inverse; imports `mark` (cycle fixed). | `emit()` (`Preview`) | 1/1b |
| `graphic/style.md` | NEW `new` (V13) | select — theme-as-data owner: color system via derive, stroke hierarchies via standard pens, composition grids, per-plane bindings; **TYPE SYSTEM** rows (type-scale ratios, per-role leading/tracking, variable-axis coords via font INSTANCE/FREEZE, OpenType feature presets via shape FeatureSpec); **SHEET FAMILY** rows (title-block grid variants, margin/zone/page-master). One theme = one row set. | NONE (data) | 1/1a |
| `typography/font.md` | KEEP `improve` (receipt-law exemplar) | N/A — FontEngineering; add font-metrics owner surface (cap/x-height via `hb_ot_metrics`, V3, standard stops leaking fontTools). | `emit()` (`Document`) | 1/1b |
| `typography/shape.md` | REBUILD `rebuild` (V3) | generate — the SOLE text engine; **exports `shaped_rgba(fragment,style)->bytes`** (E3 subtitle phantom); fallback-chain RESOLUTION via `fontTools.unicodedata.script()` itemization; SHAPE_QA golden oracle for consumer rewire parity. | `emit()` (`Document`) | 1/1b |
| `typography/layout.md` | KEEP `improve` | N/A — Knuth-Plass break correctness; carries pyphen `(change,index,cut)` channel (emit applies it V12). | `emit()` (`Document`) | 1/1b |
| `typography/math.md` | NEW `new` (V3) | generate — ziamath owner (7 pages hand-build it) composing shape's outline + uharfbuzz `Face.has_math_data`/`get_math_constant`/`get_math_glyph_variants`/`get_math_glyph_assembly`; ziafont/ziamath enter typography manifest. | `emit()` (`Document`) | 1/1b |
| `drawing/standard.md` | REBUILD `rebuild` + RE-WAVE to 1 (inversion b) | bind — closed AEC drafting vocabulary (Discipline/ScaleRatio/SheetId/LineWeight/TextHeight/Terminator/LayerName/HatchMaterial/LetteringStyle) + regime BIND rows (material→pattern, discipline→pen/lettering) + ezdxf symbol-table lowering (`seed`/`graphics`/`dimstyle`/`hatch`). ISO 286 fits, ISO 13567 full field structure, NCS level-2, bounded AIA. Composes pattern/derive/vector/shape-metrics (all w1). | NONE (substrate) | 1/1a |
| `exchange/detect.md` | KEEP `improve` + RE-WAVE to 1 (inversion d) | N/A (substrate) — puremagic/python-magic format-ID gate; folder-minted `_WORKER_RETRY` folds to `lanes.offload(retry=OCCT)` (leg-1 reconcile). | NONE | 1/1a |
| `export/layered/plan.md` | NEW `new` (V14, SPLIT of layered) | bind — `LayerPlan` semantic layer TREE (bounded roster, discipline/kind/z-order rows, ISO 13567 names on AEC / stable editorial names elsewhere, meaning-nested groups). Every layered exporter + every drawing/diagram producer projects INTO it. | NONE (vocabulary) | 1/1a |
| `core/receipt.md` | REBUILD `rebuild` (E9/inversion a/V16) | N/A — `ArtifactReceipt` union (22 cases); **`verdict`→native band** (imports zero producers, inversion a); **`ArtifactKind` Literal + `_KEYS` derived from the case roster** (E9, one owner not two hand-synced tables); band fields STAY `frozendict` (V16b-C, msgspec-native), `_KEYS`→`Map` (V16b-B). | NONE (family) | 1/1b |
| `core/plan.md` | KEEP `improve` (E1/V6) | N/A — the CPM engine, verbatim; **refresh `[CONTENT_KEY_NODES]` to name the 3 constructing owners** (§A.3) and the corrected producer roster; `content_identity`→`identity`; purge `[RESEARCH]`. | NONE (engine) | 1/1b |
| `core/format.md` | DELETE `deletePages` + `delete{into:document/emit, from:core/format}` | — dissolves into document/emit (V6, cross-wave absorb executes in leg 2; condemned-intact until then). | — | (absorb leg 2) |

### [C.2] LEG 2 — MID PLANE

| Path | Action + lowering | Tier — charter / skeleton | Entry | Wave/leg |
|---|---|---|---|---|
| `visualization/table.md` | REBUILD `rebuild` (E5/V10) | N/A — `GT(frame.to_pandas())` DEFAULT floor (polars `.style` demoted to probed capability); pagination/continuation-across-sheets; kiwisolver column-width; units-sub-rows; upstream `data/tabular` self-describing frames (source/unit/id/key columns). | `emit()` (`Table`) | 2/2 |
| `visualization/chart/spec.md` | REBUILD `rebuild` + MOVE Palette out (V4) | N/A — altair Vega authoring; `Palette`/`hex_ramp` MOVED to derive; imports derive. | NONE (authoring→export) | 2/2 |
| `visualization/chart/export.md` | REBUILD `rebuild` + MERGE transform (`improve` + `delete{into:export, from:transform}`) | N/A — host-free render dispatch; absorbs vegafusion pre-pass; typed MARKS/CHANNELS/TRANSFORMS/COMPOSITION grammar (Vega dict case dies, V10); lets-plot theme/flavor seam or narrowed charter; vl-convert `register_font_directory` font-identity loop. | `emit()` (`Chart`) | 2/2 |
| `visualization/chart/transform.md` | DELETE `deletePages` + `delete{into:chart/export, from:chart/transform}` | — merges into export (V10); stray `</content>` :314 removed. | — | (absorb 2) |
| `visualization/diagram/glyphset.md` | REBUILD `rebuild` (E9/V15) | N/A (vocabulary) — consume-or-delete dead carriers (`EndCap`/`SubLayout`/`TextRun`/`Port.at`/`corner`); ADD area/polygon glyph case (V15 room/parcel/footprint, five-mark closure opens). | NONE | 2/2 |
| `visualization/diagram/layout.md` | REBUILD `rebuild` (V15/V10) | N/A — graph layout; typed ELK layout-option vocab over pyelk `layoutOptions` (stringly dies V10); plan-geometry ingress = typed coordinate columns; area law (STACKING/PROGRAM/SITE area-proportional, CIRCULATION/SECTION_CALLOUT plan-anchored not spring); composes solar. grandalf held (parity+splines) until fast-sugiyama parity. | NONE (coordinates→draw) | 2/2 |
| `visualization/diagram/draw.md` | REBUILD `rebuild` (V3/V10/V13) | N/A — renders all 19 NodeShapes (12 dead/type-error fixed); routes math→`typography/math`, labels→`typography/shape` (E3, local ziafont/ziamath dies); `_INK`→derive contrast pick; reads `GlyphStyle.text`; projects into LayerPlan. | `emit()` (`Diagram`) | 2/2 |
| `visualization/diagram/schematic.md` | NEW `new` (V10) | N/A — schemdraw 226-element catalog (logic/flow/dsp, Kmap/Timing/BitField) the five marks can't express; `svgconfig.text='path'`. | `emit()` (`Diagram`) | 2/2 |
| `visualization/diagram/solar.md` | NEW `new` (V15) | generate — solar-ephemeris: pvlib SPA azimuth/altitude from date/latitude, solstice/equinox/hour-line arc sampling, sun-path furniture (horizon circle, altitude rings, compass ticks, labeled date arcs) over `ProjectionKind`; owned closed-form kernel the fallback. | NONE (geometry→layout) | 2/2 |
| `scene/render.md` | REBUILD `rebuild` (V9) | N/A — pyvista owner; standard-view family (`view_xy`/`view_isometric` presets); silhouette/feature-edge/hidden-line `FieldFilter` case (`vtkPolyDataSilhouette`/`vtkFeatureEdges`); GL2PS 3D→vector target (`vtkGL2PSExporter`); directional sun policy (date/lat, replaces parameterless `enable_shadows`); typed mesh ingress (geometry `mesh`/data `MeshPayload`, not `grid:object`); hoists `SceneTarget` (cycle break). | `emit()` (`Scene`) | 2/2 |
| `scene/stage.md` | KEEP `improve` (collapse premise REFUTED) | N/A — USD authoring owner (488 LOC, deep). | `emit()` (`Scene`) | 2/2 |
| `scene/export.md` | KEEP `improve` | N/A — SceneTarget dispatch; composes `package/bundle` repro-ZIP (zlib_ng clone dies). | NONE (substrate) | 2/2 |
| `composition/compose.md` | KEEP `improve` (V6/E13) | N/A — figure placement; projects into LayerPlan; `_GATE`/stamina fold to lanes. | `emit()` (`Preview`) | 2/2 |
| `composition/imposition.md` | REBUILD `rebuild` (V7/E6) | N/A — the ONE press fold owner (`_mint_groups`/`_configure_layers` OCG mechanism); sheet+egress compose it. pdfimpose schemes; complete Map migration (mixed state). | `emit()` (`Egress`) | 2/2 |
| `composition/sheet.md` | REBUILD `rebuild` (V3/V7/V13, §A.3) | bind — sheet owner; composes imposition's fold (not clone); ISO 216-derived `_SIZES`; 2D ISO 7200 title-block grid; sheet-family VARIANTS via style; `standard.lettering()`+shaped runs (Helvetica magic dies); cover/index/general-notes ISSUE completion; MediaBox/TrimBox/BleedBox page-box law; **`SheetSet.issue()` constructs `ArtifactPipeline`** (flagship). | `emit()` (`Egress`/`Pdf`/`Preview`) + `SheetSet.issue()` | 2/2 |
| `document/model.md` | KEEP `improve` (E10) | N/A — the 11-variant DocumentNode tree; `:15` "ten-variant" stale count fixed. | NONE (tree) | 2/2 |
| `document/emit.md` | REBUILD `rebuild` (V6/V12/E1/E7, §A.3, absorbs format) | N/A — lowering axis composing typography seams (V3); `EmitSpec` knob bag→grouped value objects; weasyprint `@page`/`string-set` running heads; cross-arm page-numbered TOC; FORMS first-class; **`DocumentPackage` constructs `ArtifactPipeline`** (flagship, replaces `produced`/`_fanned`); absorbs `core/format` `TemplatePipeline` bind-and-route + office template-clone delegates. | `emit()` (`Pdf`/`Office`/`Report`/`Document`) + `DocumentPackage` | 2/2 |
| `document/report.md` | REBUILD `rebuild` (V12/E6) | N/A — report composition; terminal PDF routes through emit (`Pdf.from_html` dup dies); page-numbered TOC. | `emit()` (`Report`) | 2/2 |
| `document/egress.md` | KEEP `improve` (E6/V7) | N/A — finishing; composes imposition's fold ([EGRESS_DISTINCT] contradiction fixed). | `emit()` (`Egress`) | 2/2 |
| `document/lens.md` | KEEP `improve` (V12) | N/A — recover-TO owner; OCR pre-flight gate (`ocrmypdf.pdfinfo`) + in-package PDF/A egress. | `emit()` (`Introspection`) | 2/2 |
| `document/tagged.md` | KEEP `improve` | N/A — PD/UA structural author. | `emit()` (`Egress`) | 2/2 |

### [C.3] LEG 3 — AEC / EGRESS

| Path | Action + lowering | Tier — charter / skeleton | Entry | Wave/leg |
|---|---|---|---|---|
| `drawing/dimension.md` | REBUILD `rebuild` (V3/V5/E3/E8) | generate — ISO 129-1 producer; ISO 286 fits + ISO 1101 GD&T frames/datums + dual-unit DIMALT; composes symbol's V5 terminator geometry; typography shape+math (local ziafont/ziamath dies); vector metric point-at-distance; DIM policy from TextHeight ratios (magic dies). | `emit()` (`Drawing`) | 3/3 |
| `drawing/annotate.md` | REBUILD `rebuild` (V3/V5/E3/E7) | generate — ISO 128-2 annotation; welding (ISO 2553)+surface-texture (ISO 1302)+datum/level; consumes shaped POSITIONS (E3, not re-shape); composes symbol V5 terminator; palette rewired derive (E7 ledgered). | `emit()` (`Drawing`) | 3/3 |
| `drawing/symbol.md` | REBUILD `rebuild` (V5) | generate — the ONE parametric mark-geometry owner (ISO 129-1 terminators, north/scale-bar/grid-bubble/section proportions, revision triangles/clouds); proportion ROWS feed BOTH SVG+DXF lowerings; rotation composes vector Matrix (numpy trig dies); schemdraw `Segment*`/`ElementCompound` spine stays. | `emit()` (`Drawing`) | 3/3 |
| `drawing/detail.md` | REBUILD `rebuild` (E9/V1) | generate — SheetId-typed refs (stringly `sheet:str` dies E9); build-once DAG; vector area-weighted centroid; palette derive (prose-vs-fence fixed). | `emit()` (`Drawing`) | 3/3 |
| `drawing/schedule.md` | REBUILD `rebuild` (E5/V2/V10) | bind — schedule/legend; pandas-floor render (E5, leg2 table lands it); pattern-generator swatches (crosshatch trig + invented hex die); `Reshape`-lowered. | `emit()` (`Schedule`) | 3/3 |
| `specification/section.md` | KEEP `improve` | N/A — CSI SectionFormat producer. | `emit()` (`Spec`) | 3/3 |
| `specification/classify.md` | KEEP `improve` | N/A (substrate) — MasterFormat/UniFormat/OmniClass + OmniClass element rows ([03]); §5.1.7 Schematron. | NONE | 3/3 |
| `delivery/register.md` | REBUILD `rebuild` (E5/E6/E9) | N/A — ISO 19650 register; composes classify `ClassCode` + standard `Discipline` (parallel `ClassificationSystem` dies E6, bare-str dies E9); Uniclass 2015; silent blocked-Stub fixed (E5); rustworkx assembly ordering. | `emit()` (`Register`) | 3/3 |
| `delivery/transmittal.md` | REBUILD `rebuild` (E7, §A.3) | N/A — ISO 19650 issue orchestrator; codec edge LEDGERED (E7); distribution matrix + acknowledgement round-trip; rustworkx cross-reference closure; **constructs the issue sub-pipeline** (flagship). | `emit()` (`Transmittal`) | 3/3 |
| `export/dxf.md` | REBUILD `rebuild` (V11/E9) | N/A — paperspace/viewport authoring; `Dxf.New` composes `Standard.seed` (V11 seam real); ATTRIB/Image/MPolygon/underlay; `import_blocks`; by-NAME version/unit (E9 by-value dies); `ezdxf.path`/`math` vector bridge. | `emit()` (`Cad`) | 3/3 |
| `export/layered/write.md` | REBUILD `rebuild` (V14, SPLIT of layered) | N/A — PSD/OCG/SVG/IDML/ORA/layered-TIFF writers composing the `LayerPlan` tree (never owns it); 12 native PSD blend modes (psd-tools author first); Illustrator lane = layered-PDF + organized-SVG pair. | `emit()` (`Preview`/`Egress`) | 3/3 |
| `export/indesign.md` | KEEP `improve` | N/A — IDML template mutation; IDML layers = LayerPlan. | `emit()` (`Office`) | 3/3 |
| `exchange/conformance.md` | KEEP `improve` (E-oracle) | N/A — pyhanko PAdES close; `ConformanceVerdict` OWNED here, flattened to receipt band (inversion a); veraPDF/JHOVE oracle via runtime ORACLE row. | `emit()` (`Verdict`) | 3/3 |
| `exchange/credential.md` | KEEP `improve` | N/A — c2pa content-credential. | `emit()` (`Credential`) | 3/3 |
| `exchange/metadata.md` | REBUILD `rebuild` (E-pyexiv2) | N/A — descriptive metadata; **pyexiv2 in-process arm re-sited behind process boundary OR removed** ([04] ruling below). | `emit()` (`Metadata`) | 3/3 |
| `media/container.md` | KEEP `improve` | N/A — av spine; `_WORKER_RETRY` folds to lanes (leg-1 reconcile). | `emit()` (`Media`) | 3/3 |
| `media/filtergraph.md` | KEEP `improve` (E7 REFUTED) | N/A (substrate) — capability-routed filter core; no derive coupling (E7 refuted). | NONE | 3/3 |
| `media/audio.md` | KEEP `improve` | N/A — audio encode over container. | `emit()` (`Media`) | 3/3 |
| `media/timeline.md` | KEEP `improve` | N/A — non-linear editing; strongest multi-parent clip DAG (ArtifactPipeline exemplar). | `emit()` (`Media`) | 3/3 |
| `media/subtitle.md` | REBUILD `rebuild` (E3) | N/A — pysubs2 timed-text; `shaped_rgba` now real (E3). | `emit()` (`Media`) | 3/3 |
| `media/analysis.md` | REBUILD `rebuild` (E5) | N/A — read-side measurement; `render_png`/`montage`/`frame_similarity` now real (E5). | `emit()` (`Media`) | 3/3 |
| `media/synthesis.md` | KEEP `improve` | N/A — numpy oscillator bank. | `emit()` (`Media`) | 3/3 |
| `package/bundle.md` | NEW `new` (cycle break) + RE-WAVE to 2 | N/A (vocabulary) — shared `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleManifest` + the reproducible-ZIP kernel; codec/archive/delta compose it downward (cycle break); `scene/export` (w2) composes the repro-ZIP within-wave (resolves the scene→compression inversion). zlib-ng re-entry band beside lz4/brotli/zstandard. Sited w2 as the cross-plane compression substrate, not w3, so no w2→w3 edge. | NONE | 2/2 |
| `package/codec.md` | REBUILD `rebuild` (E10) | N/A — single-blob compression; composes bundle (eager private-worker reach dies, cycle broken); zlib-ng GZIP arm. | `emit()` (`Bundle`) | 3/3 |
| `package/archive.md` | KEEP `improve` (+ carried growth) | N/A — multi-file archive; **multi-entry `unpack`/`list`/`BundleManifest` inverse** ([04] carried growth). | `emit()` (`Bundle`) | 3/3 |
| `package/delta.md` | KEEP `improve` (+ carried growth) | N/A — detools delta; **parent-keyed delta row** ([04] carried growth). | `emit()` (`Bundle`) | 3/3 |

deletePages: `core/format.md` (→document/emit, leg 2), `visualization/chart/transform.md` (→chart/export,
leg 2), `graphic/vector.md` (→graphic/vector/region, leg 1a), `export/layered.md` (→export/layered/write,
leg 3, with the LayerPlan slice already extracted to plan.md). Net: 60 − 4 deleted + 11 new = 67.

---

## [D] PACKAGE ROSTER DELTA (integration-first; every reconciliation row RULED by its condition)

### [D.1] The pvlib admission (feed-verified, additive)

`pvlib` — BSD-3, NREL-backed, pure-Python wheel (interpreter-agnostic, NO census exposure). ADMIT. Owner:
`visualization/diagram/solar.md` (V15 solar-ephemeris). Integration: `solarposition` SPA family
(refraction-corrected azimuth/altitude), sunrise/transit/solstice solvers, numpy-vectorized date sampling.
`.api/pvlib.md` authored at admission with the verified `solarposition` member set. Discharges the recorded
proof burden (capability a hand-rolled kernel re-derives); the owned closed-form kernel stands as the
declined-admission fallback. Distinct from geometry's AGPL ladybug `Sunpath` (that stays behind the process
boundary, geometry track).

### [D.2] `[PYPROJECT_RECONCILIATION]` census rows — each ruled by its survival CONDITION (never zero-count)

| Package | Census row | RULING (condition-tested) |
|---|---|---|
| `grandalf` | copyleft GPL-2.0/EPL-1.0, no release since 2023 | KEEP (integration) — LIVE consumer: parity oracle + the SOLE splines router (`layout:_grandalf_router:525`). Removal gated on fast-sugiyama parity AND a SPLINES re-home (a real routing ripple, not a zero-consumer strike). Behind the process boundary until then (copyleft posture). |
| `pyexiv2` | GPL-3.0 in-process (`metadata:46`), cp-gated dead marker | RE-SITE or REMOVE — survival condition (consumed capability pyexiftool cannot reach) is UNMET: pyexiv2's EXIF+IPTC+XMP+ICC is a SUBSET of the standing pyexiftool arm (adds GPS+maker-notes). RULING: re-site behind the out-of-process boundary (the `pyexiftool` arm already owns the carrier) OR remove; the in-process GPL-3.0 arm at `metadata:46` violates the copyleft-only-behind-a-process-boundary posture and cannot stand as-is. Default: REMOVE (pyexiftool owns the carrier); the `.api` records SUPERSEDED. |
| `iptcinfo3`, `python-xmp-toolkit` | SUPERSEDED, zero live consumers (prose-only `metadata:7`, not imported) | REMOVE — no page-set row lands a sidecar-XMP owner consuming them; the one-pass pyexiftool/pyexiv2 fold owns the carrier. `.api` records SUPERSEDED. |
| `zlib-ng` | data-campaign strike; artifacts named its home | RE-ENTER — condition MET: `package/codec:17` GZIP arm consumes `gzip_ng`/`zlib_ng`/`crc32`; `scene/export` composes `package/bundle` repro-ZIP (the clone at `export:234` dies). Re-tag `[DATA]`→`[ARTIFACTS]`, author folder `.api` overlay beside lz4/brotli/zstandard. |
| `PyICU` | never shipped a wheel (sdist+native ICU+compiler), dead marker | GATED — `uniseg`+`python-bidi` are the standing charter (shape/layout). Resolution is the Forge native-build lane or removal; carried, blocker named. Not a removal (live gated upgrade target). |
| `scikit-image` | cp314 wheels stand, cp315 pending | GATED (wheel-lag) — process/measure capability carries the marker-drop blocker by name (V8). |
| `vtk`, `usd-core`, `lets-plot` | cp314 wheels stand, cp315 pending | GATED (wheel-lag) — V9/V10 capability carries the marker-drop blocker by name; `pyvista` pure-Python, gated only through vtk. |
| `PhotoshopAPI` | no sdist, wheels stop at cp314 (structurally dead) | GATED — `psd-tools` is the standing PSD author (V14 blend modes land on it first); PhotoshopAPI's native-writer lane rides the Forge source-build follow-up. Blocker named. |

### [D.3] Mine-to-depth `.api` obligations (folder tier stacks onto shared tier; folder-only is a defect)

New `.api` catalogs authored WITH the verdict (absent surfaces): `pvlib.md` (solarposition);
`weasyprint` `@page`/`string-set`/`target-counter` (V12, absent from catalog); `vtk`/`pyvista`
Silhouette/FeatureEdges/GL2PS (V9, absent from BOTH); `pymupdf` trimbox/bleedbox (V7, mediabox present
only); `pikepdf` Separation/DeviceN authoring surface (V4, read-side flags only cataloged). Deepen existing:
`drawsvg.Pattern` (V2), `schemdraw` 226-element catalog (V10), `uharfbuzz` math surface (V3), `tifffile`
GeoTIFF/aszarr/memmap/validate_jhove (V8), `great-tables` GT(pandas)/tab_options (V10/E5), `altair`
theme.ThemeConfig registry (V13), `vl-convert` register_font_directory (V13), `drawpyo` load_diagram
round-trip (V10), `rustworkx` DAG surface for delivery/spec (V-pkg), `ocrmypdf` pdfinfo/pdfa (V12),
`ezdxf.path`/paperspace (V11), `coloraide`/`colour-science` LUT/CAM16 (V4), `pyphen` DataInt.data channel (V12).

---

## [E] VERDICT DISPOSITION (V1-V16, every one ruled)

| V | Ruling (page/leg) |
|---|---|
| V1 | `graphic/vector.md` SPLITS → `graphic/vector/{path,region,pattern}` (3 pages, leg 1a). Metric point-at-distance, decimation, area-weighted centroid, `Length.to_mm` unit egress on `path`; f-string SVG→drawsvg on `region`; tolerances→policy rows. |
| V2 | NEW `graphic/vector/pattern.md` (leg 1a): `StrokeFamily`/`PatternSpec`/`DensityLaw`, 3 lowerings (ezdxf/drawsvg.Pattern/pathops-clip); `HatchMaterial` seed rows per ISO 128-50/ANSI/BS regime. schedule/dxf/layered consume it. |
| V3 | NEW `typography/math.md` (leg 1b) + `shape` exports `shaped_rgba` + fallback-chain resolution (fontTools.unicodedata) + font-metrics owner. Consumers (dimension/annotate/symbol/draw/emit/sheet) rewire; SHAPE_QA golden oracle proves parity. |
| V4 | `Palette`/`hex_ramp` MOVE chart/spec→derive (leg 1a); `ColorReceipt` DELETED (derive=substrate); measurement stays in derive, exchange (CxF3)+plate authoring consolidate into managed; `AdaptMethod` collapses to derive. No literal hex corpus-wide. TAC policy gate + overprint/rich-black rows in managed. |
| V5 | ONE mark-geometry owner in `drawing/symbol` (leg 3): ISO 129-1 terminators + north/scale-bar/grid-bubble/section proportions + revision triangles/clouds; proportion ROWS feed SVG+DXF; composes vector Matrix (numpy trig dies). dimension/annotate/symbol consume. `drawing/geometry.md` NOT minted (symbol ownership holds). |
| V6 | The `emit() -> ArtifactWork \| Iterable[ArtifactWork]` contract corpus-wide (§A); `core/plan` REAL via 3 constructing owners (§A.3); batch rails (`produced`/`rendered`/`_fanned`) + `Block[Result]` batch + `ColorReceipt` die; `core/format` dissolves into `document/emit`. Leg 1b reconcile. |
| V7 | ONE press fold in `composition/imposition` (leg 2); sheet+egress compose it; ISO 216 `_SIZES`; 2D ISO 7200 grid; cover/index/general-notes ISSUE completion; sheet←emit notes seam COMPOSED; MediaBox/TrimBox/BleedBox page-box law. |
| V8 | `graphic/raster/io` re-partition (leg 1b): bare `render_png`/`montage` exported, pyvips hardening/streaming, RasterFact consolidated; marks cycle broken via `graphic/marks/mark`. scikit-image marker-drop blocker named. |
| V9 | BUILD the scene drawing bridge (leg 2): standard-view family + silhouette/feature-edge FieldFilter case + GL2PS target + directional sun policy + typed mesh ingress on `scene/render`; render⇄export cycle broken (SceneTarget hoisted); `.api/vtk.md`+`.api/pyvista.md` authored. stage KEEPS. vtk/usd marker-drop blocker named. V15 massing suite consumes this egress. |
| V10 | table pandas-floor (E5) + pagination + kiwisolver + units rows; chart transform MERGES into export + typed grammar (Vega dict dies) + lets-plot narrowed/seam; diagram 19-shape parity + dead carriers consumed + NEW `schematic.md` + typed ELK vocab. lets-plot blocker named. |
| V11 | `export/dxf` (leg 3): paperspace/viewports, `Standard.seed` seam real, ATTRIB/Image/MPolygon/underlay, `import_blocks`, by-NAME version/unit, ezdxf.path bridge. |
| V12 | `document/emit` composes typography seams + knob-bag→value objects + weasyprint parity + section-aware running heads + cross-arm TOC + FORMS first-class; `document/report` routes terminal PDF through emit (dup dies); `document/lens` OCR pre-flight + PDF/A egress; pyphen orthographic emission. |
| V13 | NEW `graphic/style.md` (leg 1a): theme-as-data, TYPE SYSTEM rows + SHEET FAMILY rows; every visual plane composes the binding seam; six AEC DiagramKinds reach publish grade via theme data over V15 geometry. Library-default output = defect. |
| V14 | NEW `export/layered/plan.md` `LayerPlan` (leg 1a, SPLIT of layered): semantic layer TREE, ISO 13567 names; six projectors project INTO it (upward fan → downward, inversion c disposed); `export/layered/write.md` composes; 12 PSD blend modes on psd-tools. |
| V15 | NEW `visualization/diagram/solar.md` (pvlib + closed-form fallback, leg 2); plan-geometry typed-column ingress + area law in `layout`/`glyphset`; massing suite = V9 scene egress + diagram overlays. Ladybug Sunpath stays OUT (AGPL, process boundary). |
| V16 | (a) `content_identity`→`identity` (69 spellings/45 paths, leg-assigned per touched page); (b) frozendict band-vs-table split RULED in §K; (c) `[RESEARCH]` purge (54 pages), folds into Packages/`.api` or in-body gated obligations. `rg`-zero acceptance per §K. |

---

## [F] EVIDENCE DISPOSITION (E1-E13, every row) — anchors re-verified on disk

| E | Disposition |
|---|---|
| E1 | Entry fiction. `ArtifactPipeline` constructed by nobody → 3 constructing owners (§A.3); `Work[ArtifactReceipt]` satisfiable via `_emit` thunk; batch rails die. RESOLVED §A. |
| E2 | Color orphan. `Palette`/`hex_ramp`→derive; 4 drawing importers + chart/spec + chart/export + scene rewire to derive; scene's 3rd alias dies; ARCH:103-105 edges composed; prose-vs-fence (detail) resolved. RESOLVED §B/§C. |
| E3 | Dual text engine. NEW `typography/math`; `shaped_rgba` exported; annotate consumes POSITIONS; standard's fontTools leak → font-metrics owner; 7 hand-build pages rewire; SHAPE_QA parity. RESOLVED §C. |
| E4 | Hatch instances. NEW `graphic/vector/pattern` generator; 11 borrowed ACAD names + ANSI31/ANSI37 double-book + invented swatch hex die. RESOLVED (V2). |
| E5 | Broken paths. table `GT(to_pandas())` floor (schedule + register subtotal render); `render_png`/`montage`/`frame_similarity` exported bare (io/measure). Leg 2 lands table fix; leg 3 verifies. RESOLVED. |
| E6 | Duplication. press fold→imposition; `Pdf.from_html`→emit; egress imposition algebra→imposition; terminator triad→symbol V5; symbol-table writers→standard.seed (dxf composes); register `ClassificationSystem`→classify; `AdaptMethod`→derive. RESOLVED. |
| E7 | Unwired seams. ARCH:122/137/153/156 composed (dimension/annotate←vector, emit←shape/layout); `annotate:49` chart/spec import → derive, LEDGERED; `transmittal:58` codec edge LEDGERED; `filtergraph:14` derive coupling REFUTED (no edge exists). RESOLVED. |
| E8 | Hardcoding. vector tolerances→policy rows; sheet pt tables→ISO 216 + Helvetica magic→lettering(); standard DIM magic→TextHeight ratios; emit knob bag→value objects; draw `_INK`→derive; layout stringly ELK→typed vocab. RESOLVED across V1/V7/V12/V13. |
| E9 | Dead/stringly. glyphset dead carriers consumed/deleted; draw 7-of-19→19; detail `sheet:str`→`SheetId`; register `code`/`discipline`→`ClassCode`/`Discipline`; receipt stringly `ArtifactKind`+`_KEYS`→derived from case roster; dxf by-VALUE→by-NAME. RESOLVED. |
| E10 | Ledger drift. ARCH re-synced (§B); `model:15` "ten"→eleven; package codec⇄archive⇄delta cycle broken via `package/bundle`. RESOLVED. |
| E11 | Scene seams. render⇄export cycle broken (SceneTarget hoisted); stage collapse premise REFUTED (KEEP); silhouette/feature-edge/GL2PS `.api` authored + built (V9). RESOLVED. |
| E12 | Output-grade gaps. kind-blind dispatch + closed five-mark + spring defaults + no solar → V15 machinery; running head/TOC/forms/type-scale/overprint/TrimBox → V12/V13/V7. RESOLVED across verdicts. |
| E13 | Track-law debt. 69/45 content_identity, 59 frozendict imports, 54 [RESEARCH], 43 CapacityLimiter+50 stamina — RESOLVED §K (rebind) + §A.4 (limiter/stamina collapse rides emit rewire, leg-1 corpus-wide, full 43+50 surface not the 3-4 named slice). |

---

## [G] [03] CAPABILITY-TARGET DISPOSITION (every plane row homed)

Each `[03]` target is closed by a named page + leg: graphic/vector(+pattern) 7→9.5 (vector split + pattern,
1a); typography 6→9.5 (math/outline/fallback/metrics owners + shaped_rgba + V13 type-system surface, 1b);
graphic/color 6→9 (rehomed palette + one rail + measurement-in-derive/exchange-in-managed + LUT/spectral +
plate + TAC, 1a/1b); graphic/raster 5→9 (vocab re-home + media seams + pyvips hardening + pillow procedural,
1b); visualization/table 6→9 (pandas floor + pagination + kiwisolver + units + compose-ingress bridge, 2);
visualization/diagram 5.5→9.5 (19-shape + dead carriers + schematic + typed ELK + V15 machinery + V13, 2);
design language V13 —→9.5 (style owner + type-system + sheet-family + derive-backed, 1a); export/layered
8→9.5 (LayerPlan tree + Illustrator lane + PSD blend + IDML mapping, 1a/3); visualization/chart 7→9 (typed
grammar + transform merge + lets-plot, 2); scene 5.5→9 (standard views + silhouette + section/poché + GL2PS
+ sun + typed mesh, 2); core 6→9.5 (entry real + derived kind/keys + format dissolved, 1b); drawing 6.2→9.5
(V2+V3+V5 + ISO 286/1101/2553/1302 + SheetId + Reshape, 3); composition 6.5→9.5 (one fold + ISO 7200 grid +
derived sizes + ISSUE completion + page-box, 2); document 7.5→9.5 (typography seams + value objects + report
routing + running heads + TOC + forms + lens OCR, 2); export/dxf 7→9.5 (paperspace + seam + entities +
blocks, 3); specification/delivery 8.5→9.5 (OmniClass + Uniclass + Schematron + composition + matrix +
rustworkx DAG, 3).

---

## [H] [04] PACKAGE-ROW DISPOSITION (integration, never removal except the ruled reconciliation rows)

SHARED-TIER weave binds every production fold (expression ADT+dispatch, numpy vectorization, concurrency
ONLY via `lanes.offload`, retry ONLY via `guarded`/`offload(retry=)` over the POLICY/ORACLE table, structlog+
otel span, msgspec ingress, `ContentIdentity.of` keys, `ReceiptContributor` structural conformance) — §A.4
lands the concurrency/retry half corpus-wide. Every `[04]` package binds to its owner: drawsvg→pattern+
region+diagram (V2/V13); schemdraw→schematic (V10); uharfbuzz math+metrics→math+font (V3); pyphen channel→
emit (V12); skia-pathops/svgelements→vector (V1); pillow/pyvips→raster (V8); tifffile→raster/io (V8);
great-tables/kiwisolver→table (V10); altair/vegafusion/vl-convert→chart (V10/V13); drawpyo→draw (V10);
pyvista/vtk/usd-core→scene (V9); coloraide/colour-science/colour-cxf→color (V4); ezdxf→dxf+standard (V11);
PyICU/uniseg→shape/layout (gated); pikepdf→managed plate + emit forms + egress page-box (V4/V12/V7);
weasyprint→emit (V12); ocrmypdf→lens (V12); rustworkx→plan+detail+register+transmittal+classify (V-pkg);
matplotlib→chart/spec (V13); lxml→register+transmittal+layered; segno/barcode/zxing→marks; scikit-image→
raster (V8, gated); blackrenderer→shape (V3); pdfimpose→imposition (V7); psdtags/imagecodecs→layered (V14);
c2pa/av/pysubs2/py7zr/stream-zip/detools→exchange/media/package (cold, entry rewire only); colour-cxf→color
managed (V4). Carried growth folds: emit's 6 lowering folds (V12 leg2), chart grammar (V10 leg2),
archive unpack/BundleManifest inverse + detools parent-keyed row (leg3). pvlib ADMITTED (§D.1).

---

## [I] [06] GATED-OBLIGATION RULINGS (each realized or kept-gated with the blocker named)

- **Gate #1 measured-signals contribution** — UNBLOCKED (runtime track 1 landed before artifacts). Runtime
  `[V5]` ships the table-keyed domain-histogram recorder (`record_artifact(kind, byte_volume, ratio)`,
  INSTRUMENTS Block verified on runtime disk). RULING: **REALIZE** — the artifacts emit-harvest seam composes
  `record_artifact` off the `ArtifactReceipt._facts` scalars (byte-volume, compression-ratio) keyed by the
  carried `artifact` kind, exactly as `retry_hook` is resilience-composed. No local logging fief; the
  `[METRIC_SIGNALS] [RESOLVED]` seam on `core/receipt:338` is realized in leg 1b. Render duration is NOT an
  `ArtifactReceipt` fact (the `Metrics.measured` aspect owns it).

- **Gate #2 outward figure hand-off** — UNBLOCKED (compute track 4 landed). CORRECTION (upstream-law finding
  2): the artifacts [06] + `ARCHITECTURE.md:94` "model-asset case" wording is WRONG. Compute `[V2]` (the
  authority) requires a NEW **artifacts-origin `HandoffAxis` case** shipped WITH its self-wired producer;
  `model_asset` is a distinct compute-OWN case and figures cannot ride it (one-producer-per-case law).
  RULING: **REALIZE via a new artifacts-origin axis case + producer**, keyed by `ContentIdentity`, the
  runtime `evidence Structural.drift` query staying clean; CORRECT the "model-asset" wording in [06] and
  `ARCHITECTURE.md:94`. Realizing #2 via `model_asset` is a DEFECT. Sited with the figure producers (the
  outward hand-off travels off the `emit()` node's content key). Leg 2 (mid-plane figures) / leg 1b seam.

- **Gate #3 content-keyed output elision** — KEEP GATED. Blocker: the C# `Rasm.Persistence` reuse fabric
  (not a py upstream). Each producer threads its `(ContentKey, Work)` pair into lane admission (the `emit()`
  node already carries this), so the seam is READY; the durable cross-run short-circuit lands when
  Persistence does. Blocker named, no silent close.

---

## [J] LEG PARTITION — dependency barriers, self-contained, whole-repo in-run reconcile

Leg 1 splits 1a/1b (the largest blast radius warrants it): 1a settles the foundational OWNERS (vocabulary/
substrate/receipt-union/LayerPlan/style/standard-rewaved/detect-rewaved) so 1b's corpus-wide `emit()` rewire
has settled cases/plans/vocabularies to bind against. 1b gated on 1a landing residual-clean.

- **LEG 1a (FOUNDATIONS — owners)**: `graphic/vector/{path,region,pattern}`, `graphic/color/derive`,
  `graphic/raster/{process}`, `graphic/marks/mark`, `graphic/style`, `typography/{font,shape,layout,math}`
  (shape/math rebuilt as owners; the receipt-minting half of their entry lands 1b), `drawing/standard`
  (re-waved), `exchange/detect` (re-waved), `export/layered/plan`. In-run reconcile: `README`/`ARCHITECTURE`
  domain-map + seam-ledger rewrite for the vector split, LayerPlan siting, standard/detect re-wave; pyproject
  `.api` stubs for pvlib + zlib-ng re-tag; the V16 rebind on every touched 1a page.
- **LEG 1b (ENTRY-CONTRACT rewire — corpus-wide)**: `core/{receipt,plan}` finalized; the `emit() ->
  ArtifactWork` rewire on ALL 43 producers (§A.2); the limiter/stamina collapse (43+50 surface, §A.4); the
  Gate #1 recorder seam. In-run reconcile: closes the entry rewire on every producer across all three folder
  planes (AEC/media/exchange producers get their `emit()` here so leg 3 cold-verifies), the V16 rebind
  completion, `core/format` condemned-intact (dissolves leg 2).
- **LEG 2 (MID PLANE)**: `visualization/*` (V15 machinery + solar), `scene/*` (V9 + cycle break),
  `composition/*` (V7 fold), `document/*` (V12 + `core/format` absorb + DocumentPackage constructor), and
  `package/bundle` (the compression vocabulary + repro-ZIP kernel, re-waved to w2 so scene/export composes it
  within-wave). In-run reconcile: the sheet-set/document-package pipeline construction, table pandas-floor
  (leg-3 schedule/register depend on it), the Gate #2 artifacts-origin producer.
- **LEG 3 (AEC/EGRESS)**: `drawing/*`, `specification/*`, `delivery/*`, `export/{dxf,layered/write,indesign}`,
  `exchange/{conformance,credential,metadata}` (cold — entry landed leg 1b), `media/*` (cold),
  `package/{codec,archive,delta}` (cold except archive/delta carried growth; each composes the leg-2
  `package/bundle`, landing the codec⇄archive⇄delta cycle break). In-run reconcile: verifies the leg-1b
  entries, closes the carried growth, the pyexiv2 re-site/removal + iptcinfo3/xmp-toolkit removal.

Per-leg engine invocation rows (targets repo-relative `libs/python/artifacts/.planning/...`):
```
1a: rebuild {targets:[graphic/vector, graphic/color/derive, graphic/raster/process, graphic/marks/mark,
    graphic/style, typography, drawing/standard, exchange/detect, export/layered/plan], brief:DECISION}
1b: rebuild {targets:[core/receipt, core/plan, graphic/color/managed, graphic/raster/io,
    graphic/raster/measure, graphic/marks/encode, graphic/marks/decode], brief:DECISION}  # + corpus-wide
    emit()/limiter reconcile writes every producer plane
2:  rebuild {targets:[visualization, scene, composition, document, package/bundle], brief:DECISION}
3:  rebuild {targets:[drawing, specification, delivery, export/dxf, export/layered/write, export/indesign,
    exchange, media, package/codec, package/archive, package/delta], brief:DECISION}
```
No post-leg residual channel: each leg's in-run reconcile has whole-repository write authority and closes
every confirmed residual before it returns. No card governance (IDEAS/TASKLOG dead); findings land as page
rows, seam rows, or gated obligations.

---

## [K] V16 TRACK-REBIND RULING (the band-vs-table split, ruled explicitly)

The register's E13(b) premise "`from builtins import frozendict` is not a CPython builtin — every import
raises" is **REFUTED** on the target interpreter: `from builtins import frozendict` SUCCEEDS
(`frozendict.__module__ == 'builtins'`, PEP 814 Final, py3.15; verified `uv run python`). The V16b ACTION
still stands, re-grounded on `[04]` SHARED-TIER consistency (the `Map` ADT/dispatch spine runtime/data/
compute forced on every sibling), NOT as a bug fix. The rebind splits by ROLE (shapes.md `[OWNER_INDEX]`
row [08] admits BOTH `frozendict` and `Map` as immutable-evidence owners):

- **V16b-A (import deletion)** — DELETE every `from builtins import frozendict` line (59 sites). Redundant:
  `frozendict` is a builtin, referenced bare. This satisfies the `rg`-zero acceptance gate for `from builtins
  import frozendict` MECHANICALLY, independent of role.
- **V16b-B (TABLE role → Map)** — every `Final[frozendict[K,V]]` dispatch/policy table (92 `Final[frozendict`
  sites: `_KEYS`, `_METRIC`, `VL_RENDER`, `DELEGATES`, `PLANS`, `FINISHERS`, `_ROUTES`, …) re-types
  `Final[Map[K,V]]`, constructed `Map.of_seq(...)`, on the shared `Map` spine. `Map.try_find`/`Map.__getitem__`
  replace `frozendict` lookup.
- **V16b-C (BAND role → STAYS frozendict)** — `frozendict[...]` as a msgspec `case()` payload slot
  (`receipt:78,80,86,92` scene/preview/media/cad; `managed:278`; `marks/encode:262`/`decode:329` RasterFact
  band; `dxf:343,357`; `indesign:112`; `imposition:341`) or a `msgspec.Struct` field (`detail:227`) STAYS
  `frozendict` (bare builtin, no import). `expression.Map` is NOT msgspec-native — a `case()` payload must be
  msgspec-encodable and hashable; `frozendict` is the sanctioned immutable-evidence owner there. The
  deleted-form PROSE names the anti-pattern by the **Map-table-vs-frozendict-band contrast**, never "broken
  vs working import."

Cross-track record: runtime/data/compute siblings cite the same falsified premise; the DECISION corrects the
justification here (artifacts scope) and flags the cross-track re-grounding without re-planning siblings.

Acceptance (mechanical, per-page as legs touch): `rg` returns zero under `libs/python/artifacts/.planning/`
for `rasm.runtime.content_identity`, `from builtins import frozendict`, and `[RESEARCH]` headers. Bare
`frozendict[...]` band references survive (V16b-C); they carry no import line and are not the gated form.

---

## SELF-REPORT (exact counts)

- Page rows authored: **67** (60 current − 4 deleted + 11 new).
- Deleted pages: **4** (`core/format`, `visualization/chart/transform`, `graphic/vector.md`,
  `export/layered.md` — the last two absorbed into their new folders).
- New pages: **11** (`graphic/vector/{path,region,pattern}`, `graphic/style`, `graphic/marks/mark`,
  `typography/math`, `export/layered/{plan,write}`, `visualization/diagram/{schematic,solar}`,
  `package/bundle`).
- Verdicts disposed: **16** (V1-V16, §E).
- Evidence rows disposed: **13** (E1-E13, §F).
- Package rows ruled: **8 reconciliation census rows + 1 pvlib admission + ~28 mine-to-depth** (§D/§H).
- Inversions disposed: **4** (a/b/c/d, §B.1); cycles broken: **3** (marks/scene/package, §B.2).
- Gated obligations ruled: **3** (#1 realize, #2 realize-via-new-axis-case + wording fix, #3 keep-gated) (§I).
- Legs: **4** (1a/1b/2/3, §J); constructing owners named: **3** (sheet-set, document-package, transmittal).
- Carrier conventions collapsed: **6** (11 concrete shapes → 1 `emit() -> ArtifactWork | Iterable`, §A.2).
