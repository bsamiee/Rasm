# DOSSIER — lane=foundations (graphic/* + typography/* + core/*)

Scope read FULLY on disk: `README.md`, `ARCHITECTURE.md`, and the 14 lane pages —
`core/{plan,receipt,format}`, `graphic/vector`, `graphic/color/{derive,managed}`,
`graphic/raster/{io,measure,process}`, `graphic/marks/{encode,decode}`,
`typography/{font,shape,layout}`. Every assigned register row re-verified against disk with
`rg`/anchor checks. Member claims spot-checked against the fences and the `.api` citations the
pages carry.

## [00] HEADLINE — the register's "naive/illusory" verdict is STALE as CODE QUALITY, LIVE as STRUCTURE

The foundations corpus on disk has been substantially rebuilt SINCE the brief's `[00]-[VERDICT]`
baseline. As **page-internal code**, most foundations pages are already 8.5–9.5 grade
(`core/plan` = realized `ArtifactWork`/CPM engine; `typography/font` = the receipt-law exemplar;
`graphic/raster/{process,measure,io}` = a 139-member dispatch engine; `graphic/color/derive` =
773-line two-engine colorimetry; `typography/{shape,layout}` = uharfbuzz+Knuth-Plass at
publication grade). A per-page "this page is shallow" attack on these is REFUTED by disk.

What HOLDS — and is the whole job of this design pass — is the **STRUCTURAL/SEAM** rot the register
names, none of which a per-page cold pass touches:
1. **E1/V6 entry non-convergence** — 6 distinct producer carrier shapes in this lane ALONE, no
   `emit() -> ArtifactWork`, no page constructs `ArtifactPipeline`.
2. **E2 derive orphan** — a world-class color owner with ZERO importers + a parallel `ColorReceipt`
   rail; the palette that actually flows is a bare `NDArray` alias minted in `visualization/chart/spec`.
3. **E4 no graphic-plane fill/pattern owner** (V2 gap) — corpus-wide.
4. **E13 track-law debt** — frozendict/content_identity/[RESEARCH] present at the EXACT register counts;
   the rebuild that produced the strong pages did NOT apply the V16 track rebind.
5. Duplicate carriers (`RasterFact`, `AdaptMethod`), the marks encode⇄decode cycle, and missing
   new-owners (V2 pattern, V13 style/type-system, typography/math).

Design directive: for the strong pages, DO NOT re-litigate internal density; fix the seams, the entry
contract, the orphan wiring, and the track rebind. For the genuinely thin/absent surfaces (fill owner,
math owner, style/type-system, the plate-authoring lane) mint the owners.

---

## [01] REGISTER VERDICTS (assigned rows, re-verified on disk)

### E1 — Entry fiction (core/plan half) — **HOLD** (primary anchor exact; sub-anchors drifted)
- `core/plan.md:354` `class ArtifactPipeline(Struct, frozen=True)` — **EXACT**.
- "constructed by no page outside its own `of()` factory" — **HOLD**. `rg 'ArtifactPipeline'`
  corpus-wide returns ZERO construction sites; every hit outside `core/plan.md` is prose in a
  `[…] [RESOLVED]` block or a comment (`delivery/transmittal.md:573`, `specification/section.md:708`,
  `media/timeline.md:96,293`, `export/{layered:257,260,indesign:303}`). No constructing owner exists
  — the brief's SEAM_AND_ENTRY_LAW mandate ("the DECISION names the constructing owner whose fence
  builds `ArtifactPipeline` from producer `emit()` nodes") is unmet.
- `of()` factory: register `:448-451`; disk `of` staticmethod is **:441-450** → **DRIFT** (corrected
  anchor `core/plan.md:441-450`).
- `:89 Work[ArtifactReceipt] unsatisfiable`: line 89 is now `class ArtifactWork`; the `work:
  Work[ArtifactReceipt]` field moved to **:91** → **DRIFT** anchor, but the DEFECT is **VERIFIED
  stronger** on disk: `Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]` is satisfiable only by a
  producer returning `RuntimeRail[ArtifactReceipt]`, and the lane's producers return SIX incompatible
  shapes (census below). plan.md itself is fully rebuilt to the `ArtifactWork` node design, so the
  defect migrated from "unsatisfiable field" to "unconstructed pipeline + unconverged producer
  contract + missing constructing owner."
- `core/format.md:260-271 (_fanned)`: disk `_fanned` at **:271-278** — HOLD, but note it fans
  FORMATS of one context (a task-group + `traversed` fan-out), not a producer-DAG scheduler; it is a
  second scheduling PATH that bypasses `ArtifactPipeline`, consistent with V6's "`_fanned` dies with
  format's dissolution."

**E1/V6 CARRIER CENSUS (disk, this lane only) — the "six incompatible shapes" verified:**
| page | entry | return |
|---|---|---|
| `color/derive.md:473` | `derive()` | `RuntimeRail[ColorReceipt]` ← parallel rail (V4 violation) |
| `color/managed.md:255` | `apply()` | `RuntimeRail[ArtifactReceipt]` |
| `marks/encode.md:525` | `of()` | `RuntimeRail[Block[Result[ArtifactReceipt, MarkFault]]]` |
| `raster/io.md:447` | `of()` | `RuntimeRail[Block[Result[ArtifactReceipt, RasterFault]]]` |
| `typography/font.md:263` | `engineer()` | `RuntimeRail[tuple[ContentKey, ArtifactReceipt]]` |
| `typography/{shape:265,layout:153}` | `run()`/`lay()` | `RuntimeRail[ContentKey]` |
Method names ALSO diverge (`derive`/`apply`/`of`/`engineer`/`run`/`lay`) — no uniform `emit()`.
`graphic/vector.md:771` `of() -> RuntimeRail[Block[VectorResult]]` is correctly a substrate (mints no
receipt). The V6 rewire must land the `ArtifactWork` node contract across every one of these.

### E2 — Color orphan — **HOLD** (all anchors exact)
- `graphic/color/derive.md:3` claims "the one color source every visual plane pulls palettes from";
  `rg 'from artifacts.graphic.color.derive'` corpus-wide = **ZERO importers**.
- De-facto palette: `visualization/chart/spec.md:30` `type Palette = NDArray[np.float64]`, `:128`
  `def hex_ramp(...)` — **EXACT**. Imported by the four drawing pages
  `drawing/annotate.md:49`, `detail.md:46`, `schedule.md:44`, `symbol.md:45` — **EXACT**; plus
  `visualization/chart/export.md:36`, `visualization/diagram/draw.md:37`; plus `scene/render.md:45
  type Palette = (...)` the third alias — **EXACT**.
- Parallel receipt rail: `derive.md:432 class ColorReceipt` (frozen dataclass), `:458 class
  ColorReceiptWire`, `derive()` returns `RuntimeRail[ColorReceipt]` (:473) — **EXACT**. This is the
  brief `[00]`'s "one violation: derive's parallel `ColorReceipt` rail."
- Three-concerns-in-one-page (V4): a 28-member `Metric` StrEnum (`derive.md:223-251`), the `_METRIC`
  measurement table (`:282-315`), and the CxF3 `Spot` intake (`:629`, `read_cxf`) all co-reside with
  the derivation arms — confirmed.

### E3 — Dual text engine (typography half) — **HOLD**
- `typography/shape.md` is a near-9.5 shaping owner: 7 arms (NORMALIZE/BIDI/ITEMIZE/FALLBACK/SHAPE/
  RASTERIZE/QA), `PositionedGlyphRun` with GID/cluster/advance/offset/flags/extents/metrics/baseline/
  style, `on_path()`/`to_svg_path()`/`glyph_outlines` (the V3 outline surface IS here, `:216-222`),
  COLRv1/CBDT/sbix/SVG/CPU color-glyph raster (`:466-540`), CPAL flag palette select, synthetic
  bold/slant, vharfbuzz QA. It is UNDER-CONSUMED, not shallow.
- `shaped_rgba` PHANTOM — **HOLD**: `rg 'def shaped_rgba'` = never defined; referenced only at
  `media/subtitle.md:50` (import) and `:299` (call). shape.md has the RASTERIZE capability but exports
  no `shaped_rgba(fragment, style) -> bytes`. Unwired export gap.
- Dual-engine coexistence broader than the register's 4: `rg 'ziafont|ziamath'` = **7 pages**
  (`document/model`, `drawing/{annotate,dimension,standard,symbol}`, `visualization/diagram/{draw,
  glyphset}`), all beside the live `typography/shape` owner. `drawing/annotate.md:47-48` DOES import
  `LineBrokenRun`+`PositionedGlyphRun` yet still hand-builds ziafont — the "decodes then discards +
  re-shapes" pattern the brief names.
- `typography/math.md` owner MISSING — `ls typography/` = `{font,layout,shape}.md` only. The ziamath
  concern the 7 pages hand-build has no owner.
- The specific E3 anchors (`annotate.md:387-392`, `standard.md:292-322`, `dimension.md:663-693`) live
  in consumer pages outside this lane; the typography-SOURCE side (which supplies the surface) is
  verified sound-and-complete — the fix is consumer rewiring + a math owner + a `shaped_rgba` export.

### E4 — Hatch/fill "no SVG-side fill owner corpus-wide" — **HOLD**
- `rg 'class Pattern|PatternSpec|StrokeFamily|def hatch|HatchSpec|drawsvg.Pattern|set_pattern_fill'`
  over `graphic/ typography/ core/` = **ZERO**. No graphic-plane pattern/hatch/tiling owner exists;
  `drawsvg` is not imported anywhere in graphic. The V2 owner is a missing-owner gap. (The
  `drawing/standard._HATCH` instance table the row's other anchors name is outside this lane.)

### E5 — Broken paths (media-import half in raster lane) — **HOLD**
- `media/analysis.md:48` `lazy from artifacts.graphic.raster.io import render_png, montage`; `:49`
  `... measure import frame_similarity`. On disk: `io.md` exports `RasterOp.Montage` (:388) + private
  `_montage` (:708) but **no bare `montage`** and **no `render_png`**; `measure.md` has SSIM via
  `Transform.SSIM`/`_metrics` but **no `frame_similarity`**. All three imports are phantoms. The
  capabilities EXIST (montage grid, PNG re-encode, SSIM) but are not exposed as the bare surfaces
  analysis calls (`render_png(rgba)`, `montage(chosen)`, `frame_similarity(prev,cur)`).

### E6 — Duplication (AdaptMethod anchor in my pages) — **HOLD** (exact)
- `graphic/color/derive.md:94` and `graphic/color/managed.md:74` both `class AdaptMethod(StrEnum)`,
  member-for-member identical — **EXACT**. managed's own `[ADAPTATION]` note (`:444`) ADMITS the
  duplication ("aligned member-for-member with derive's AdaptMethod over the same registry").

### E7 — Unwired seams (marks cycle + typography-source anchors) — **HOLD**
- Marks encode⇄decode cycle CONFIRMED: `graphic/marks/encode.md:55,60` import from
  `artifacts.graphic.marks.decode` (`DecodeSource`/`MarkDecodeError`/`_zxing_decode` + more);
  `graphic/marks/decode.md:33` imports from `artifacts.graphic.marks.encode` (`RasterFact`,
  `Symbology`). Real bidirectional import cycle — the SEAM_AND_ENTRY_LAW's named "marks encode⇄decode"
  census cycle, live on disk.
- ARCH:153/156 (`emit←shape/layout`): the typography SOURCE side is real (shape emits
  `PositionedGlyphRun`, layout emits `LineBrokenRun` via `broken()`); the "composed nowhere" defect is
  on `document/emit` (mid-plane, outside this lane).

### E8 — Hardcoding (vector anchors) — **HOLD** (all four exact)
- `graphic/vector.md:267` `approximate_arcs_with_cubics(0.1)`; `:278` `convertConicsToQuads(0.25)`;
  `:450` `spine.point(min(frac + 1e-3, 1.0))`; `:498` `Length(length).value(ppi=96.0, ...)` —
  precisely the brief V1 magic set `0.1/0.25/ppi=96.0/1e-3`, all inline, none lifted to policy rows.

### E9 — Dead/stringly (receipt stringly-kind half) — **HOLD**
- `core/receipt.md:37` `type ArtifactKind = Literal[...]` — the stringly kind. The Literal now spans
  **:37-60** (22 members); register `:37-41` starts EXACT at 37 but undershoots the range → note
  corrected range `:37-60`.
- `core/receipt.md:314` `_KEYS: frozendict[ArtifactKind, tuple[str, ...]]` hand-synced field-name
  table — **EXACT**. `_facts` zips case tails against it under `strict=True` (`:300`); it must stay
  hand-synced with the 22 `case()` tuples. The brief's "stringly `ArtifactKind` Literal + hand-synced
  `_KEYS` becomes one derived owner" is unaddressed.

### E10 — Ledger drift (derive half) — **HOLD**
- `ARCHITECTURE.md:103-105` declare `graphic/color/derive → visualization` (`[DERIVE]:
  ColorReceipt.coords palette`), `→ scene`, `→ graphic/color/managed` DERIVE edges. Against **zero
  derive importers** (E2), all three ledger edges are uncomposed. `:103` even encodes the
  V4-condemned `ColorReceipt.coords` in the ledger itself. (model.md:15 ten-variants and the package
  codec-cycle parts of E10 are outside this lane.)

### E13 — Track-law debt — **HOLD** (counts EXACT)
- `content_identity`: 69 spellings / 44 files; 45 full `rasm.runtime.content_identity` paths — matches
  register exactly. `from builtins import frozendict`: 59 sites / 54 files — exact. `[RESEARCH]`
  headers: 54 files — exact.
- In this lane: `from rasm.runtime.content_identity` in 9 pages (`managed:32`, `io:39`, `encode:41`,
  `font:34`, `shape:37`, `layout:34`, `plan:33`, `receipt:30`, `format:37`); `from builtins import
  frozendict` in 12 pages (all but `font` and `plan`); `[03]-[RESEARCH]` header in 12 pages (all but
  `receipt` and `format`, which use `[03]-[SIGNALS]`). Every one of the 14 lane pages carries at least
  one debt marker. The V16 rebind was NOT applied by whatever rebuild produced the strong pages.

---

## [02] PER-PAGE VERDICTS + DEFECTS + CHARTER-AS-IT-SHOULD-BE

### core/plan.md (465 LOC) — 9/10 code, structurally UNFED
- STRONG: rustworkx `PyDiGraph` topology, `Schedule` CPM (forward/backward `Block.fold`, once-computed
  `finish`/`latest`), min-slack front ordering, `PipelinePlan` coverage evidence
  (`dangling`/`cyclic`/`collided`/`untargeted`/`elided` + `severed: Option[PlanFault]`), `to_thread`
  offload, `_emitted` planned-phase receipt. The `ArtifactWork` node contract IS the V6 target shape.
- DEFECT: constructed/scheduled by NOBODY (E1). No producer emits `ArtifactWork`; the `work` thunks the
  comment at `export/layered.md:257` promises (`work=export.emit`) are never wrapped into nodes.
- DEFECT (track-law): `:33 from rasm.runtime.content_identity`; `:453 [03]-[RESEARCH]` (11 [RESOLVED]
  items — fold into Packages/.api per V16c). Note: plan.md correctly carries NO frozendict.
- CHARTER: keep the engine verbatim; the DECISION must name the CONSTRUCTING owner(s) (the flagship
  sheet-set issue + editorial package) whose fence builds `ArtifactPipeline.of(<producer emit() nodes>)`,
  and land `emit() -> ArtifactWork | Iterable[ArtifactWork]` on every producer so the six carriers
  collapse. Rename `content_identity`→`identity`; purge `[RESEARCH]`.

### core/receipt.md (339 LOC) — 9/10 code, two live debts
- STRONG: 22-case `ArtifactReceipt` tagged_union, `(ContentKey, *scalars)` payloads, total `slot`/
  `_facts` matches closed by `assert_never`, `frozendict` evidence bands for `preview`/`media`/`cad`/
  `scene`, `contribute() -> Iterable[Receipt]` structural port. The receipt-family spine the brief
  preserves.
- DEFECT (E9): stringly `ArtifactKind` Literal (`:37-60`) + hand-synced `_KEYS` (`:314`) — the twin
  hand-maintained tables the brief wants collapsed to one derived owner.
- DEFECT (track-law): `:27 from builtins import frozendict` (import RAISES — frozendict is not a
  builtin), `:30 content_identity`. TENSION for the DECISION: the evidence bands are msgspec `case()`
  PAYLOAD fields (`scene: tuple[..., frozendict[str, float | str]]`, `:78`), not mere `Final` tables —
  the V16(b) `frozendict->Map` rebind is trivial for `_KEYS` but non-trivial for msgspec-encodable
  hashable payload bands (`expression.Map` is not msgspec-native). This band-vs-table split must be
  ruled explicitly, NOT hand-waved as "re-type `Final[frozendict]`→`Final[Map]`."
- CHARTER: derive `ArtifactKind` from the case roster (one owner, no hand-synced `_KEYS`); rule the
  band typing; rename import; keep the union case roster fixed for leg-1 (later legs consume).

### core/format.md (288 LOC) — 9/10 code, but a V6 DISSOLUTION candidate
- STRONG: `TemplatePipeline` bind-and-route over `TemplateTarget` StrEnum (14 targets), fused
  `DELEGATES` route table (mode+spec-builder in one row), `SheetProfile`/`QueryProfile` value objects,
  `TemplatePayload` TypedDict + `_PAYLOAD` TypeAdapter admission, `bound()` modal fan-out. Genuinely
  bind-and-route (owns no engine), NOT the "1:1 DocumentMode re-spelling" the register implies —
  though nearly every `TemplateTarget` maps 1:1 to a `DocumentMode` (only `HTML`→report differs).
- DEFECT (V6): the brief rules `core/format` DISSOLVES into `document/emit` (a cross-wave absorb pair,
  leg 2). The disk page is a real, working owner — the DECISION must rule the dissolution explicitly
  (re-home `TemplatePayload` admission + the `bound()` fan-out into emit; `_fanned` dies), NOT leave it
  as a strong incumbent. `_fanned` (`:271-278`) is the second scheduling path E1 names.
- DEFECT (track-law): `:32 frozendict`, `:37 content_identity`. `[ASSET_BAND] [BLOCKED]` (`:288`) is a
  live open item on `EmitSpec.assets` widening.
- CHARTER: condemn-but-intact until leg 2; the DECISION states the emit re-home target set.

### graphic/vector.md (885 LOC) — 9/10 code, the V1 folder-split KERNEL
- STRONG: `Vector` modal owner, `VectorOp`/`VectorResult`/`VectorFault` closed families, svgelements
  parse/measure/sample/flatten/subpaths + skia-pathops boolean/outline/warp/wind/region/contains + a
  single `svg()` emitter, `@lru_cache` parse memo, `to_process` WORKER_BAND offload, one pen-protocol
  geometry spine.
- DEFECT (E8): tolerance/DPI magic inline at `:267,278,450,498` — V1 wants these lifted to policy rows.
- DEFECT (V1 COVERAGE thin-slices): NO metric arc-length point-at-distance arm (dimension ticks need
  a point at "20mm along path", not a fraction `t∈[0,1]`; `sample` at `:537` uses parametric `npoint`);
  NO polyline decimation/simplify; NO area-weighted centroid (`region` at `:335` returns area/hull but
  no centroid — `drawing/detail` stands in with vertex-mean). Unit-egress DEFERRED explicitly
  (`:883 [GROWTH_AXES]`, `Length.to_mm` suffix-strip).
- DEFECT (V1/V2): raw f-string SVG at `:487,494,419,288` (`svg()`/`path()`/`_framed`/`clip`) — the
  brief wants the owned serialization OR `drawsvg`; `drawsvg` unimported. Prose-vs-fence drift: the
  `[01]`/Cases prose calls `Region` a 4-tuple `(area, bounds, count, convex)` but the fence
  (`:85`) is a 5-tuple with the control-hull bounds appended.
- DEFECT (track-law): `:35 frozendict` (`_FLATTEN`/`_PROJECT` tables), `:875 [RESEARCH]`.
- CHARTER: split into `graphic/vector/` folder (≥2 pages: the svgelements query/affine engine + the
  pathops boolean/offset/stroke/warp spine + serialization/raster egress + text-on-path — the DECISION
  draws the cut). Add metric point-at-distance, decimation, centroid, unit egress; lift tolerances to
  a policy row; admit `drawsvg` for the emitter. TIER: generate (primitive path algebra). Host the V2
  pattern owner as a sibling page in the folder.

### graphic/color/derive.md (773 LOC) — 9.5/10 code, ORPHANED (the #1 integration home)
- STRONG: two-engine colour-science + ColorAide, `ColorOp` (convert/adapt/gamut/filter/palette/
  compose/temperature/measure/correct/spot), `ColorModel` dual-name vocabulary, `Ramp` union,
  28-member `Metric` + `_METRIC` policy table, CxF3 `Spot` intake, refined `Annotated`+`Is` bounds.
  World-class colorimetry.
- DEFECT (E2/V4): ZERO importers; parallel `ColorReceipt`/`ColorReceiptWire` rail (`:432,458`),
  `derive() -> RuntimeRail[ColorReceipt]` (`:473`). Three concerns in one page (derivation /
  measurement / exchange).
- DEFECT (E6): `AdaptMethod` (`:94`) duplicated in managed (`:74`).
- DEFECT (track-law): `:27 frozendict`, `:762 [RESEARCH]`.
- CHARTER: rehome `Palette`/`hex_ramp` FROM `visualization/chart/spec` INTO derive (or a derive-owned
  palette surface); collapse `ColorReceipt`→`ArtifactReceipt`; split derivation/measurement/exchange
  per the DECISION; own `AdaptMethod` (managed composes). TIER: generate (color derivation). Rewire the
  4 drawing importers + chart/spec + scene to consume derive; compose the ARCH:103-105 edges as real
  fences (no literal hex anywhere).

### graphic/color/managed.md (446 LOC) — 9/10 code, correct receipt, plate-authoring gap
- STRONG: ICC/LUT/CCTF egress, `ManageOp` (Managed/Export), `GradeStep` chain fold, pyvips
  `icc_transform` in a `to_process` worker, lcms2 soft-proof (`_softproof`, out-of-gamut count), CxF3
  device-half `separations()` (SpotChannel), TAC ink preflight (`ManagedFact.INK`). Contributes
  `ArtifactReceipt.Preview` (`:293,304`) — correctly on the shared rail (unlike derive).
- DEFECT (E6): `AdaptMethod` (`:74`) duplicate — resolve by importing derive's.
- DEFECT (V4 plate): SPOT/CxF `ColorCmykplusN` is DECODE-ONLY (`:224` "libvips authors no named
  DeviceN channel, so the declaration is carried evidence"). No owner AUTHORS Separation/DeviceN
  colorspaces — the plate-authoring lane (pikepdf raw object model, per brief V4) is absent.
- DEFECT (track-law + V16c): `:26 frozendict`, `:32 content_identity`, `:441 [03]-[RESEARCH]` with a
  LIVE UNRESOLVED item `[ADAPTATION] [RESEARCH]` (`:444`) — convert to an in-body gated obligation.
  Prose drift: `[SOFT_PROOF_SEPARATIONS]` (`:446`) claims siblings carry a stale `receipt.receipt`
  import "corrected on their own pages" — verified STALE: `io.md:43` and `encode.md:46` already use
  the canonical `artifacts.core.receipt` path. Delete the note.
- CHARTER: compose derive's `AdaptMethod`; keep the egress owner; the DECISION sites the Separation/
  DeviceN plate-authoring lane (over pikepdf) that consumes managed's SpotChannel declaration. TIER:
  select/egress (consumes derive's provenance).

### graphic/raster/io.md (1026 LOC) — 9/10 code, V8 re-partition PARTIAL
- STRONG: `Raster` owner, 15 `RasterOp` cases (thumbnail/convert/crop/probe/montage/composite/
  transform/detect/smartcrop/pyramid/geometry/deframe/sequence/quantize/children), pillow+pyvips
  engine-polymorphic, delegated `exchange/detect` gate, `WORKER_BAND` `to_process`, `RasterFault`
  vocabulary, per-op `Result` batch. The 139-member `Transform` StrEnum vocabulary owner (`:152+`).
- DEFECT (V8): still 1026 LOC. The acceptor BODIES + rows moved to process/measure (good), but io
  retains the enum + RasterOp owner + dispatcher — the DECISION rules whether io splits further.
- DEFECT (E5): exports no bare `render_png`/`montage` — `media/analysis` phantom-imports them.
- DEFECT (V8 deferred): pyvips `block_untrusted_set` hardening + `Source`/`Target` streaming are
  DEFERRED growth axes (`:19`), not landed.
- DEFECT (dup carrier): `class RasterFact` declared here (`:321`) AND re-declared in
  `marks/encode.md:258` — io.md:7 admits it. Duplicate carrier.
- DEFECT (track-law): `:33 frozendict`, `:39 content_identity`, `:1013 [RESEARCH]`.
- CHARTER: export `render_png`/`montage` bare surfaces (E5 home); land pyvips hardening/streaming;
  consolidate `RasterFact` to the neutral site the marks-cycle break creates. TIER: N/A (working
  surface).

### graphic/raster/measure.md (279 LOC) — 9.5/10 code (award-grade), one phantom
- STRONG: 42-member measurement engine, member-derived families (blob trio/corner quintet/fit trio/
  flow pair/keypoint pair via one `getattr`), 3 acceptors, kwargs-policy columns, composes process
  substrate. NOT naive — the opposite.
- DEFECT (E5): exports no `frame_similarity` — SSIM exists (`Transform.SSIM`→`_metrics`,
  `structural_similarity`) but `media/analysis.md:49` phantom-imports the bare name.
- DEFECT (track-law + V16c): `:22 frozendict`, `:267 [03]-[RESEARCH]` (11 [RESOLVED] items — fold).
- CHARTER: export `frame_similarity(a,b) -> float` (a bare SSIM helper, the E5 media home); keep the
  engine; fold the [RESEARCH] tail. TIER: N/A.

### graphic/raster/process.md (483 LOC) — 9.5/10 code (award-grade)
- STRONG: 97-member produced-raster engine, 10 acceptors, `MorphKind`/`FilterChannel` dispositions,
  owns the shared `TransformInput`/`TransformArm`/`_save_array`/`_luminance`/`_channels` substrate.
  Clean produced-vs-measured axis split with measure. No naivety.
- DEFECT (track-law): `:28 frozendict`, `:473 [RESEARCH]`.
- CHARTER: keep verbatim; rename import; fold [RESEARCH]. TIER: N/A.

### graphic/marks/encode.md (602 LOC) — 8.5/10 code, the cycle + dup-carrier owner
- STRONG: `Mark` owner over `MarkOp` (Encode/Decode), segno/python-barcode/zxing-cpp, `Symbology`
  vocabulary, `MarkFault`, `MarkPayload` TypedDict, contributes `ArtifactReceipt.Preview`.
- DEFECT (E7/V8 cycle): imports decode (`:55,60`) while decode imports encode (`:33`) — the marks
  encode⇄decode cycle. `of() -> RuntimeRail[Block[Result[ArtifactReceipt, MarkFault]]]` — a 6th/7th
  distinct producer carrier.
- DEFECT (dup carrier): re-declares `RasterFact` (`:258`) beside io's canonical (`:321`).
- DEFECT (track-law): `:34 frozendict`, `:41 content_identity`, `:592 [RESEARCH]`.
- CHARTER: break the cycle via a neutral shared-vocabulary site (`RasterFact`/`Symbology`/
  `DecodeSource` to an owner both import, per V8); consolidate `RasterFact`; land the `ArtifactWork`
  emit. TIER: generate (mark geometry over vector).

### graphic/marks/decode.md (390 LOC) — 8.5/10 code, the cycle's other half
- STRONG: `_zxing_decode` inverse, `DecodeSource`, `@beartype` foreign-admission contract.
- DEFECT (E7/V8 cycle): imports `RasterFact`/`Symbology` from encode (`:33`).
- DEFECT (track-law): `:29 frozendict`, `:385 [RESEARCH]`.
- CHARTER: consume the neutral vocabulary site; keep the decode inverse. TIER: N/A.

### typography/font.md (412 LOC) — 9.5/10 code, THE RECEIPT-LAW EXEMPLAR
- STRONG: `FontEngineering`, per-mode `FontJob` closed union (subset/instance/axis_catalog/outline/
  embed_audit/merge/freeze/feature/compile), `engineer() -> RuntimeRail[tuple[ContentKey,
  ArtifactReceipt]]` deriving key+receipt from ONE `_render` payload, `_GATE`-bounded `to_thread`
  offload, `FontJob.receipt` twin match, `ScriptTags` seam for shape. The exemplar the brief names.
- DEFECT (track-law): `:34 content_identity`, `:406 [RESEARCH]`. (Correctly NO frozendict.)
- NOTE: even the exemplar returns `tuple[ContentKey, ArtifactReceipt]`, not `ArtifactWork` — the V6
  rewire touches font too.
- CHARTER: keep verbatim; the receipt-derivation shape is the reference siblings match; rename import;
  fold [RESEARCH]. TIER: N/A.

### typography/shape.md (589 LOC) — 9.5/10 code, UNDER-CONSUMED
- STRONG: see E3. The full text engine.
- DEFECT (E3): under-consumed by the drawing/diagram plane (dual ziafont/ziamath engine runs beside
  it); exports no `shaped_rgba` (subtitle phantom).
- DEFECT (track-law): `:32 frozendict`, `:37 content_identity`, `:582 [RESEARCH]`.
- CHARTER: export `shaped_rgba(fragment, style) -> bytes` (the RASTERIZE surface subtitle needs); be
  the sole text engine every glyph routes through (drawing/diagram rewire); a `typography/math` sibling
  owns ziamath, composing shape's outline. Add the font-metrics owner surface (cap-height/x-height via
  `hb_ot_metrics`) V3 names. TIER: generate (shaping/glyph).

### typography/layout.md (460 LOC) — 9.5/10 code, break-correctness only (V13 gap)
- STRONG: Knuth-Plass total-fit (badness/demerits/fitness-classes/active-node frontier), uniseg/PyICU
  engine-polymorphic break, pyphen hyphenation incl. the `DataInt.data` `(change,index,cut)`
  orthographic channel (`:239`), COLLATE, `broken() -> LineBrokenRun` projection + `lay() ->
  RuntimeRail[ContentKey]`. Reads shape's break-safety/tatweel flags.
- DEFECT (V13): owns break-correctness ONLY (brief-confirmed). The TYPE SYSTEM (type-scale ratios —
  zero corpus hits, per-role leading/tracking, variable-axis coords per role, OpenType feature presets)
  has NO owner — the V13 gap.
- NOTE (V12/pyphen): layout CARRIES the orthographic channel but no emit arm APPLIES it at a break
  (the consumer gap is on `document/emit`, mid-plane).
- DEFECT (track-law): `:28 frozendict`, `:34 content_identity`, `:455 [RESEARCH]`.
- CHARTER: keep break-correctness; the V13 style plane owns type-scale/leading/tracking/feature-preset
  rows (composing shape's `FeatureSpec` + font's `INSTANCE`/`FREEZE`). TIER: N/A (break correctness).

---

## [03] CROSS-CUTTING FINDINGS

1. **V6 entry non-convergence is the dominant foundations defect.** 6 producer carrier shapes in this
   lane (census in E1); no `emit() -> ArtifactWork`; no `ArtifactPipeline` constructor anywhere. The
   corpus-wide `ArtifactWork` rewire is leg-1's largest blast radius — the DECISION must (a) land
   `emit() -> ArtifactWork | Iterable[ArtifactWork]` on every producer, (b) name the constructing
   owner(s) whose fence builds the pipeline, (c) collapse `derive`'s `ColorReceipt` into the union.

2. **Track-law rebind (V16) is mechanical but corpus-wide and NOT applied.** Every lane page carries
   ≥1 of {`content_identity` import (9 pages), `from builtins import frozendict` (12 pages), `[03]-
   [RESEARCH]` (12 pages)}. Counts match the register exactly (69/45/59/54). The `frozendict->Map`
   rebind has a real subtlety: `receipt`/`managed`/`measure`/`io` use `frozendict` as msgspec `case()`
   PAYLOAD bands (`frozendict[str, float|str]`), not just `Final` dispatch tables — `expression.Map`
   is not msgspec-native, so the band-vs-table split must be ruled, not blanket-rebound.

3. **Duplicate carriers:** `RasterFact` (`io:321` + `encode:258`); `AdaptMethod` (`derive:94` +
   `managed:74`). Both collapse to one owner (io/neutral-site for RasterFact; derive for AdaptMethod).

4. **Marks encode⇄decode import cycle** (E7/V8 census) — break via a neutral shared-vocabulary site
   holding `RasterFact`/`Symbology`/`DecodeSource`.

5. **Missing new-owners the DECISION must site (all in the foundational plane):**
   - V2 pattern/hatch/tiling owner (`graphic/vector/` sibling) — the E4 fill gap; home for
     `drawsvg.Pattern`, ezdxf pattern-line lowering, pathops clip lowering.
   - V13 style/type-system owner (`graphic/style.md`) — the type-scale/leading/tracking rows layout
     doesn't own; the theme-as-data plane.
   - `typography/math.md` — the ziamath owner the 7 dual-engine pages hand-build.
   - Separation/DeviceN plate-authoring lane (over pikepdf) consuming managed's SpotChannel.

6. **Prose-vs-fence drift:** `vector.md` Region prose (4-tuple) vs fence (5-tuple, `:85`);
   `managed.md:446` stale sibling-import note (siblings already corrected).

---

## [04] INTEGRATION HOMES (orphan-looking admissions → TARGET, never removal)

Every admitted package in this lane is a strong, integration-shaped surface — closure is WIRING, never
pruning:
- **`colour-science` + `coloraide` + `colour-cxf`** (derive, 773 LOC): fully mined but the OWNER is
  orphaned. TARGET: wire the 4 drawing + chart/spec + scene consumers to derive; compose ARCH:103-105.
  managed already consumes the CxF device half — keep the two-reader split.
- **`drawsvg`**: NOT integrated in graphic (the only unwired graphic admission). TARGET: the V2 pattern
  page (`Pattern` tiles) + vector's f-string-SVG replacement (`Drawing`/`Group`/`Raw`).
- **`skia-pathops` + `svgelements` + `resvg-py`** (vector): fully integrated; add the V1 metric/
  decimation/centroid/unit arms over the existing surface.
- **`scikit-image`** (process 97 + measure 42): fully integrated; add `render_png`/`montage`/
  `frame_similarity` bare surfaces for the media seam.
- **`pyvips` + `pillow`** (io + managed): integrated; land the deferred `block_untrusted_set`/streaming.
- **`uharfbuzz` + `blackrenderer` + `python-bidi` + `uniseg` + `pyphen` + `vharfbuzz` + `PyICU` +
  `fonttools` + `opentype-feature-freezer`** (shape/layout/font): fully integrated; expose
  `shaped_rgba`; the math/style/metrics owners compose shape+font.
- **`segno` + `python-barcode` + `zxing-cpp`** (marks): integrated; break the cycle, land the emit.

No removal candidate exists in the foundations lane. The `[PYPROJECT_RECONCILIATION]` standing removals
(`iptcinfo3`/`python-xmp-toolkit`/`pyexiv2`/`grandalf`-on-parity/PyICU-ruling) are all outside this
lane's owning surfaces (exchange/metadata, diagram/layout) — `PyICU` is a live gated upgrade in
shape/layout (charter: `uniseg`+`python-bidi` stand while the wheel gate holds), an integration target,
never a removal.
