# DOSSIER — aec-egress lane (drawing / specification / delivery / export / exchange / media / package)

Deep-read on disk of all 27 lane pages + `ARCHITECTURE.md` seam ledger. Register rows re-verified with file:line. Cross-checked phantom members against foundation pages (`graphic/raster/io`, `graphic/raster/measure`, `typography/shape`, `core/receipt`). Anchors are `libs/python/artifacts/.planning/`-relative.

---

## [00] HEADLINE VERDICTS

- The drawing plane has been **substantially rebuilt vs the brief's register snapshot** — E3/E6/E8 anchors drifted, some improved (HatchFill regime, kiwisolver read-back, typography composition in prose). But the **structural defects V2/V3/V5 persist and are in places argued-for by the current pages** (annotate `[TERMINATOR_MARKER]` explicitly declines V5 unification). Dense confident RESEARCH tails are the prime suspect: they claim capability the fences do not deliver.
- **V5/E6 terminator triad is worse than the register states**: three renderers using **three incompatible mechanisms** (dimension raw f-string SVG / annotate `drawsvg.Marker` / symbol `schemdraw arrow=`), plus symbol hand-duplicates every proportion magic between SVG and DXF arms.
- **V2/E4 pattern generator does not exist**: no `StrokeFamily`/`PatternSpec` owner corpus-wide; `_HATCH` is still eleven borrowed ACAD names (two double-booked), the SVG side hand-rolls crosshatch trig.
- **E5 verified-broken paths confirmed**: schedule honestly gates its blocked polars `Stub(group=)` fence (:748/:251); register composes the identical blocked form (:756) with **no block recorded**; media/analysis imports **three phantom functions** (`render_png`, `montage`, `frame_similarity`).
- **E10 package cycle is real and reaches private symbols** (codec eager-imports archive/delta private workers `_archive_in_process`/`_delta_unpack`).
- **NEW cross-cutting (beyond register)**: the entire media plane (6 pages) + `exchange/detect` mint a **folder-local `stamina.AsyncRetryingCaller` (`_WORKER_RETRY`)** — the exact anti-pattern the brief's `[04]`/E13 flags for `compose.md:159`, the runtime `offload(retry=RetryClass.OCCT)` rail unused.
- **Sound surfaces confirmed**: `specification/*` (9), `exchange/*` (8-9), `media/{container,audio,timeline,synthesis,filtergraph}` (8-9), `package/{archive,delta}` (9), `export/{layered,indesign}` (8, the E1 convergence proof). `delivery/transmittal` is the single E13-cleanest lane page.

---

## [01] REGISTER VERDICTS (my assigned rows, on-disk)

### E3 — Dual text engine (drawing half + media/subtitle + standard)
- **HOLD** `drawing/dimension.md:663-676` (ziafont outline) + `:679-693` (ziamath tolerance). Dimension is the **purest E3 offender**: `_annotation_layer` calls `ziafont.Font(...).text(...)` directly (:668,673); the fence imports **zero typography** (imports :42-57 carry no `typography/*`), yet prose (:16) claims "typography/shape#SHAPE owns the full shaped run" — clean prose-vs-fence drift, ISO 3098 text never sees itemization/fallback/shaping.
- **DRIFT** `drawing/annotate.md` — brief `:221,374-392` → on disk the ziamath usage is `_text_span:224` (`getsize()` measure) and the "discards shaped positions" defect moved to **`_prose_group:388-393`**: `:388` decodes `PositionedGlyphRun`, but uses it **only** for cluster→source-index slicing (`run[line.start][1]`), then re-outlines `source[lo:hi]` through ziafont `_outlined` (:393) — the shaped x_advance/x_offset **positions are discarded and re-shaped**, exactly the E3 defect, anchor moved.
- **HOLD** `drawing/standard.md:295-313` (`FontMetric.bind` reads `TTFont["OS/2"].sCapHeight`/`["head"].unitsPerEm`) + `:492-501` (`_read_metric` opens `TTFont`) — raw fontTools metric leak (brief `:292-322` covers the region). V3 wants a typography metrics owner.
- **HOLD** `media/subtitle.md:50` imports `shaped_rgba` + `:299` uses it — **phantom**: `typography/shape.md` defines no `shaped_rgba` (rg zero). Ironic: subtitle:48 self-flags a *different* phantom (`pysubs2.substation`) while importing this one.

### E4 — Hatch instances (standard + schedule)
- **HOLD** `drawing/standard.md:529-541` `_HATCH` eleven ACAD names; `ANSI31` double-booked STEEL `:530`/GLASS `:540` **exact**; **plus an unnamed extra: `ANSI37` double-booked** TIMBER_END `:535`/LIQUID `:539`. Magic scale/angle (`0.04`,`0.02`,`4.0`,`135.0`). `:506-524` `_LINETYPE` hand-computed dash arrays; `:640` (`[LINETYPE_FULL_CARDINALITY]`) states the page's own dash convention. Partial improvement: `HatchFill` regime (PATTERN/SOLID/GRADIENT) added, STEEL→SOLID, EARTH→GRADIENT — but **no `StrokeFamily`/`PatternSpec` generator; V2 unmet**.
- **HOLD** `drawing/schedule.md:355-356` invented hex `#7a5c33`/`#c6a86a` (GRADIENT swatch) **exact**; `:358-361` hand-rolled crosshatch trig (`math.cos(math.radians(spec.angle))`) **exact**; `_hatch_swatch`/`_color_swatch` `:339-364`. All swatch ink `#111111` hardcoded (:335,342,353,358) — E8/V4 literal-hex beyond the E4-named ones. **No SVG-side fill owner corpus-wide** confirmed.

### E5 — Broken paths (schedule + register + media/analysis)
- **HOLD** `drawing/schedule.md:748` `[GREAT_TABLES_POLARS_RENDER_BLOCK] [BLOCKED]` **exact** (records `fmt_number`/`tab_stub(groupname_col=)` raise `OutOfBoundsError`/`IndexError` on any polars-backed GT); `:251` subtotal-fence gate **exact**. The block reveals a **corpus-wide `visualization/table` render defect** — every `TableOp.Fmt` consumer (schedule/chart/report) is polars-blocked; escalation: schedule's *own* non-subtotal default path (`_render:209` `TablePlan(frame=shaped_polars).build()` with `Fmt(NUMBER)` ops) is **also** gated on the unlanded table.md `to_pandas()` fix, so schedule's "render-safe" claim at :748 is itself contingent.
- **HOLD** `delivery/register.md:756` `TableOp.Stub(rowname="reference", group="discipline")` **exact** — composes the identical blocked `group=` form on a polars `register.frame` (:437 `row()`→`pl.from_dicts`), with **no block recorded anywhere** in register.md. Plus `:760` `TableOp.Color(columns=["state"])` — also polars-blocked per the schedule block. The asymmetry (schedule honest, register silent) is the exact E5 defect.
- **HOLD** `media/analysis.md:48-49` — **all three phantom**: `render_png` (rg zero in `io.md`), `montage` (io has `Montage` factory `:388`/`_montage` private `:708`, not the lowercase `montage(tiles)→rgba` analysis calls at `:398`), `frame_similarity` (rg zero in `measure.md`). Usages `:259`,`:333`,`:398` all broken. Dense prose (:3,:14,:15) references them as if real.

### E6 — Duplication (terminator triad / symbol-table writers / register↔classify)
- **HOLD** terminator triad — three renderers, **three incompatible mechanisms**:
  - `drawing/dimension.md:520-546` `_terminator_kind`/`_terminator_mark` (raw f-string SVG `<path d>`) **exact**.
  - `drawing/annotate.md:268-285` `_marker` (`drawsvg.Marker(orient='auto')`) — brief `:269-286`, **DRIFT −1 line**.
  - `drawing/symbol.md:285` `arrow=_ARROW[style.terminator]` + `_ARROW` table `:567-572` (`schemdraw arrow=` codes) **exact**.
  - Escalation: annotate `[TERMINATOR_MARKER]:629` **explicitly declines V5** ("no shared drawsvg-marker owner to extract") — the corpus argues *against* the unified owner.
- **DRIFT** symbol-table writers — brief `export/dxf.md:731-742` → on disk `:731-742` is the **hatch-fill dispatch**; the actual dxf table writer moved to **`_table_entry:778`** + `TableEntry:352`/factories `:360-373`. Duplication real: dxf `_table_entry` (`doc.<table>.add`) ↔ `standard.md:413-430` `Standard.seed`. dxf `New` mints its own `ezdxf.new` doc, never composing `Standard.seed` (V11 seam unreal). (Note: dxf `Layer:360` factory name collides with `export/layered.Layer` and `standard.LayerName` — three `Layer` concepts.)
- **HOLD** `delivery/register.md:91-96` `class ClassificationSystem(StrEnum)` (5 members incl. Uniclass 2015/ISO 12006-2) parallel to `specification/classify.md:41-44` `class ClassSystem(StrEnum)` (3: MF/UF/OC). register prose (:5,:91,:338) *acknowledges* classify owns the code tables yet declares its own system enum. Paired with E9 `Classification.code: str` — register has two classify-duplication defects.
- (Out-of-lane cross-ref) AdaptMethod derive↔managed is `graphic/color` — another surveyor's row.

### E7 — Unwired seams (annotate / transmittal / filtergraph)
- **HOLD** `drawing/annotate.md:49` `from artifacts.visualization.chart.spec import Palette, hex_ramp` — cross-domain import with **no ledger edge** (ARCH annotate rows 133-140 list no chart/spec edge). Doubles as E2 color-orphan.
- **HOLD** `delivery/transmittal.md:58` `from artifacts.package.codec import Bundle, CodecProfile, CompressionAlgo` — and `:572` its own `[ARCHITECTURE_DELIVERY_TRANSMITTAL]` seam recital lists `package/archive` but **omits `package/codec`**; ARCHITECTURE.md (rows 210-215) also has no transmittal→codec edge. Doubly unledgered. (Also: prose :15,:16 attribute `Bundle` to `package/archive`, but the import + packages line :21 correctly place it in `package/codec` — internal ownership confusion; `Bundle` lives in codec.)
- **REFUTED** `media/filtergraph.md:14` — disk falsifies. filtergraph imports av/PIL/numpy only (imports :19-37, **no derive/graphic.color**); `:14` is `_grade_args` routing prose that merely *names* `graphic/color/derive#DERIVE` as a future routing target. The derive coupling the register cites does not exist; the rebuild removed it.

### E8 — Hardcoding sweep (standard / dimension / annotate)
- **HOLD** `drawing/standard.md` DIM/mleader/transparency magic (`_DIMSTYLE` rows :589-593, `_STATUS_TRANSPARENCY` :570-579, mleader `:429-430`).
- **HOLD** `drawing/dimension.md:84` `_ARC_SPAN=30.0` **exact**; `:534-542` terminator proportion multipliers (`*size*0.3`,`*0.5`,`*0.25`) **exact**; `:656` `dimasz`/`dimtxt*0.1`/`0.13` width magic **exact**. Plus `_svg` default `pen="#1a1a1a"` (:492) literal ink.
- **DRIFT** `drawing/annotate.md` — brief `:219,392` → the raw magic is `1.4` leading multiplier at `:222` and `:393`, plus named `_SHOULDER=2.0`/`_BUBBLE=1.4`/`_MIN_SEP=8.0` (:70-72). Partial remediation (magic lifted to module constants), but still un-parameterized geometry.

### E9 — Dead/stringly (detail / register / dxf)
- **DRIFT** `drawing/detail.md` — brief `:79,84` → the stringly sheet ref is `DetailRef.sheet: str` at **`:80`** (`designator: str` :79, `.cite()` :84); should be owned `SheetId` (standard.md). Mixes owned (`discipline: Discipline` :81) and stringly. `Callout.host: str` (:116) same pattern.
- **HOLD** `delivery/register.md:340` `code: str` (should be classify `ClassCode`) **exact**; `:352` `discipline: str` (should be standard `Discipline`) **exact**. Whole `InformationContainer` BS 1192 naming is stringly (:347-353).
- **HOLD** `export/dxf.md:111-118` `DxfVersion` by-VALUE (`"AC1032"`) + `:121-128` `DxfUnits` by-VALUE ints — the E9/V11 by-VALUE codes. **Internal inconsistency**: sibling policy enums `:196-240` are by-NAME (`getattr(<policy>, member.name)`), `LeaderSide:184-188` by-name; only version/units by-value.

### E10 — Ledger drift (package cycle) + core/receipt inversion
- **HOLD** `package/codec.md:37` eager `from artifacts.package.archive import ..._archive_in_process, _archive_unpack`; `:38` eager `...delta import ..._delta_in_process, _delta_unpack`; `package/archive.md:92` `lazy from ...codec import (...)` back-edge (comment :96 admits "cyclic owner"); `package/delta.md:30` same. **All exact.** Cycle broken only by `lazy`, and codec reaches **private cross-page workers** — strong coupling. V8/seam-law wants a neutral shared-vocabulary site.
- **HOLD** (brief `[05]` inversion (a), E10-adjacent) `core/receipt.md:33` `from artifacts.exchange.conformance import ConformanceVerdict`; `:84` `verdict: tuple[ContentKey, ConformanceVerdict]`; `:141` `Verdict` mint. Wave-1 (core) composes wave-3 (exchange/conformance). conformance.md:7 frames it as intentional one-directional (no reciprocal `ArtifactReceipt` import), but it is still a foundations-first violation. The DECISION must freeze-the-shape or re-home per `[05]`.

### E13 — Track-law debt (corpus-wide, my lane census)
- **HOLD** `from builtins import frozendict`: **26 of 27** lane pages (all but `transmittal.md`). Every such import raises (not a CPython builtin).
- **HOLD** `rasm.runtime.content_identity` full path: **21 lane pages** (absent only where the page mints no key — standard/classify/section-page/filtergraph substrates + transmittal carries it at :34).
- **HOLD** `[RESEARCH]` tails: **26 of 27** (all but `transmittal.md`; `indesign.md` carries 2).
- Folder-minted retry: see cross-cutting [02.6]. `transmittal.md` is the single cleanest lane page (no frozendict, no [RESEARCH]) but still carries `content_identity` at :34.

### E1 — Entry realization (layered / indesign convergence proof) — HOLD (sound)
- **HOLD** `export/layered.md:256` `async def emit(self) -> RuntimeRail[ArtifactReceipt]` **exact**, comment :257-260 "no module-level batch entry"; `export/indesign.md` `emit` per-instance-no-batch confirmed in prose (:5,:9,:16). These are the landed V6-target shape — the positive control the brief cites, verified on disk.

---

## [02] CROSS-CUTTING FINDINGS (beyond the register)

### [02.1] V5 mark-geometry: three mechanisms + SVG/DXF proportion duplication
Beyond the terminator triad (E6), `drawing/symbol.md` **hand-duplicates every proportion magic** between the SVG `_*_element` and DXF `_dxf_block` arms:
- North: SVG `_north_element:335-337` (`radius*0.42`,`*0.55`,`*0.25`) ↔ DXF `_dxf_block:528-530` (identical `0.42`/`0.55`/`0.25`).
- Datum: SVG `_datum_element:395-396` (`size*0.5`,`*0.8`,`*0.6`) ↔ DXF `:536-538` (identical).
- Revision: SVG `_revision_element:370-373` (`size*0.87`,`*0.5`,`*0.05`) ↔ DXF `:540-541` (identical).
No proportion-row owner exists; V5's "proportion rows feed BOTH lowerings" is unmet. This is the COVERAGE-naivety twin of a generator: the next symbol is two hand-edited magic sets, not one row.

### [02.2] V2 pattern plane absent
No `graphic/pattern`/`StrokeFamily`/`PatternSpec` owner corpus-wide. `HatchSpec.definition()` (standard.md:270-274) uses `ezdxf.tools.pattern.load` (DXF/ACAD-name-only); the SVG side (`schedule._hatch_swatch:358-361`) hand-rolls crosshatch with `math.cos/sin`; `dxf._build_entity` hatch arm (:738-741) also defers to ACAD pattern names. Three unrelated fill mechanisms, zero shared repeating-fill geometry owner.

### [02.3] Naivity axis — enumerated rosters where generators belong
- `drawing/standard.md` `_HATCH` (:529-541), `_DISCIPLINE` (:548-567) are seed rows over *no* generative shape for the pattern side (the ACAD names are terminal magic, not a tile algebra). The linetype dash arrays (:506-524) are hand-computed per row rather than derived from the stated `24/12/6/3/0` convention.
- `drawing/schedule.md` `_TEMPLATE` (:375-660, 15 rich rows) is the *correct* generator shape (one row → full TableOp sequence via `_schedule_ops`) — a positive exemplar the drawing plane's mark geometry should match.

### [02.4] Prose-vs-fence drift (dense confident pages hiding gaps)
- `drawing/detail.md`: prose `:3,:15,:18,:20` claim `graphic/color/derive#DERIVE` for the palette; fence `:46` imports `Palette,hex_ramp` from `visualization/chart/spec`; packages line `:21` correctly names chart/spec — internally contradictory (E2). V4 rehoming unrealized across the whole drawing plane (annotate:49/detail:46/schedule:44/symbol:45 all import the chart/spec alias).
- `drawing/dimension.md`: prose claims typography ownership of the shaped run; fence hand-outlines through ziafont with zero typography import.
- `media/analysis.md`: prose (:3,:14,:15) treats three phantom functions as real.

### [02.5] E5 render defect is corpus-wide, not schedule-local
`schedule.md:748` records the polars-backed-GT render block as "a `visualization/table#TABLE` + manifest residual"; the fix is one line (`GT(shaped.to_pandas())`). **Every** lane consumer of `visualization/table` on a polars frame is affected: `schedule` (its own default `Fmt` path), `delivery/register` (`_index_ops` Stub+Color), and any drawing legend. register ships it silently. This is a wave-2 `visualization/table` blocker that the AEC/egress cold pass inherits — the DECISION must land the pandas materialization in `visualization/table` (leg 2) so leg-3 pages verify rather than re-discover.

### [02.6] Folder-minted worker-death retry (media plane + detect) — SHARED-TIER LAW violation
`media/container.md:3` defines `_WORKER_RETRY = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)`; **all six media pages** (`container`,`analysis`,`audio`,`subtitle`,`synthesis`,`timeline`) use/import it, and `exchange/detect.md:11` mints its own `stamina.AsyncRetryingCaller.on(BrokenWorkerProcess)`. This is the exact anti-pattern the brief `[04]` SHARED-TIER LAW and E13 name for `compose.md:159`/`imposition:150`/`sheet:151`: worker-death retry must ride runtime **`offload(retry=RetryClass.OCCT)`** (the runtime `[V5]` landed target spanning `anyio.BrokenWorkerProcess`), never a folder-local caller. The lane *correctly* uses the runtime `WORKER_BAND` lane owner (`from rasm.runtime.lanes import WORKER_BAND`) but bolts a folder-minted retry on top. The DECISION must fold these onto the runtime retry export in leg-3's cold reconcile (media/exchange are cold-verify per `[05]`, so this rewire is the leg-1 corpus-wide reconcile obligation, not a leg-3 rewire).

### [02.7] V14 LayerPlan tree absent
`export/layered.md:169-199` `class Layer` is a flat `Struct` with `group: str` (:180) — a single label, not a nested semantic tree. No `LayerPlan` owner exists. The six upstream projectors (`composition/compose`,`imposition`,`sheet`,`visualization/diagram/draw`,`graphic/marks/encode`, drawing plane) import `export/layered.Layer` directly (the brief `[05]` inversion (c) upward fan). V14's foundational `LayerPlan` (ISO 13567-aligned semantic tree, nested `/Order`, PSD group folders) is unrealized; layered still *owns* the vocabulary its consumers reach up for.

### [02.8] Wave-1-composes-wave-3 inversions in my lane ([05] a + d)
- (a) `core/receipt.md:33` ← `exchange/conformance.ConformanceVerdict` (verified above).
- (d) `graphic/raster/io.md:49` ← `exchange/detect` `Detect`/`DetectEngine`/`DetectIdentity`/`Source` — io (foundations) composes detect (aec-egress). `detect.md:3` designs detect as the ingest-boundary format-ID substrate io consumes. Both inversions are one-directional (no cycle) but violate foundations-first; the DECISION re-homes the detect vocabulary below raster or re-waves, and freezes/re-homes the verdict shape.

---

## [03] INTEGRATION HOMES FOR ORPHAN-LOOKING ADMISSIONS (targets, never removals)

- **`zlib-ng`** (data-campaign strike candidate): **home CONFIRMED** — `package/codec.md:17` GZIP arm consumes `gzip_ng`/`gzip_ng_threaded`/`zlib_ng`/`crc32`/`crc32_combine` as the SIMD gzip substrate; the archive stored-ZIP CRC (`archive.md:18`) reuses the same `zlib_ng.crc32`. The brief's re-entry condition ("a `package/`-plane consuming fence admits it beside the live lz4/brotli/zstandard band") is met. Keep; do not strike.
- **`pyexiv2`** (removal-default): **survival condition UNMET** — `metadata.md:46` retains it as a capability-gated in-process arm behind `to_process`, but its capability (EXIF+IPTC+XMP+ICC) is a **subset** of the standing `pyexiftool` arm (which adds GPS+maker-notes, `metadata.md:11`). The brief requires "consumed capability pyexiftool cannot reach"; none exists. Integration target: either remove (pyexiftool owns the carrier) OR the DECISION must name a unique pyexiv2 capability. As-is it is a redundant second RASTER arm. (`iptcinfo3`/`python-xmp-toolkit` correctly superseded — prose-only at `metadata.md:7`, not imported.)
- **`detools`** (2023-stale, cycle-tail): fully integrated — `package/delta.md` mines the full `create_patch` surface (firmware `FirmwareLayout.of_elf` via pyelftools, `patch_info` header peek, three apply kernels). The brief's carried `parent-keyed detools delta row` is realized. Home: `package/delta`. (Its coupling into codec is the E10 cycle, not an orphan.)
- **`schemdraw`** (V10 schematic owner): only the `Segment*`/`ElementCompound` geometry spine is used (`symbol.md`); the 226-element catalog is unmined. Integration target is `visualization/diagram/schematic.md` (mid-plane, out of my lane) — noted for the mid-plane surveyor.
- **`drawsvg`**: `Pattern` is unmined (V2's SVG lowering) — the pattern-plane owner is its home. `Group`/`Path`/`Marker` well-used across annotate/symbol/detail/schedule; dimension is the outlier (raw f-string + ElementTree, `_svg`/`_polyline`/`_canvas` :488-517) — integration target: route dimension's SVG emission through drawsvg like its siblings.
- **`stamina`**: currently folder-minted across media/detect (see [02.6]) — integration target is the runtime `offload(retry=)`/`guarded` rail, not a package removal.

---

## [04] PER-PAGE VERDICTS

| Page | Grade-now | Entry / receipt | Key defects (anchors) |
|---|---|---|---|
| drawing/standard | 6.5 | substrate, no receipt/plan | E4 `_HATCH` borrowed names + ANSI31/ANSI37 double-book (:530/540, :535/539); fontTools leak (:295-313); no V2 generator; `Standard.seed` table writer (:413-444) duplicated by dxf `_table_entry` |
| drawing/schedule | 7 | `resolve→RuntimeRail`, `ArtifactReceipt.Schedule` | E5 blocked-Stub honestly gated (:748/251) but own default `Fmt` path also gated; invented swatch hex (:355-356); crosshatch trig (:358-361) |
| drawing/dimension | 6.5 | `render→RuntimeRail`, `Drawing`/`Pdf` | E3 purest offender (:663-693, zero typography import); E6 terminator (:520-546); E8 (:84,534-542,656); raw f-string+ElementTree SVG (outlier) |
| drawing/annotate | 6.5 | `render→RuntimeRail`, `Drawing` | E3 discards shaped positions (:388-393); E2/E7 chart/spec import unledgered (:49); uses `"Standard"` mleader (:452,479) ignoring standard's seeded `ISO-3098-MLEADER`; E6 `_marker` (:268-285) |
| drawing/symbol | 6.5 | `render→RuntimeRail`, `Drawing` | E6 `_ARROW` schemdraw-code terminators (:285,567-572); SVG/DXF proportion magic duplication ([02.1]); E2 chart/spec (:45) |
| drawing/detail | 6.5 | `resolve→RuntimeRail`, `Drawing` | E9 stringly `DetailRef.sheet: str` (:80), `host: str` (:116); E2 prose-vs-fence (prose derive, fence chart/spec :46) |
| specification/classify | 9 | pure substrate, no receipt | Sound. E13 (frozendict :33, [RESEARCH] tail). `ClassCode` cross-system value object — the model register's `Classification` should compose |
| specification/section | 9 | `of→RuntimeRail[ContentKey]`, `Spec` | Sound. Lowers into `document/model` tree. E13 only |
| delivery/register | 7.5 | `emit→RuntimeRail[ArtifactReceipt]`, `Register` | E5 silent blocked-Stub (:756); E6 parallel `ClassificationSystem` (:91-96); E9 `code`/`discipline` str (:340,352) |
| delivery/transmittal | 8.5 | `emit`/`close→RuntimeRail`, `Transmittal` | E7 codec edge unledgered (:58/:572); Bundle ownership prose-confusion (:15 vs :21). Cleanest E13 (no frozendict/[RESEARCH]) |
| export/dxf | 7 | `of`/`contribute→RuntimeRail`, `Cad` | E9 by-VALUE version/units (:111-128, inconsistent w/ by-name siblings); E6 `_table_entry` (:778) ↔ standard.seed; `Dxf.New` mints own doc (V11 seam unreal) |
| export/layered | 8 | `emit→RuntimeRail[ArtifactReceipt]` (E1 proof :256) | V14 flat `Layer`/`group: str` (:169-199); no `LayerPlan`; upward-fan importers |
| export/indesign | 8 | `emit→RuntimeRail` (E1 proof) | Sound. IDML step fold. E13 (2× [RESEARCH]) |
| exchange/conformance | 8-9 | `close→RuntimeRail[(ContentKey, ConformanceVerdict)]` | Sound. Owns [05](a) inversion target (receipt imports its Verdict). `stamina.retry` here is the *sanctioned* crypto-network weave (distinct from worker-death) |
| exchange/credential | 8-9 | `close→RuntimeRail[(ContentKey, CredentialEvidence)]` | Sound. c2pa in-process, `stamina.retry` transient-network only (sanctioned) |
| exchange/detect | 8-9 | `of`, no receipt (substrate) | Sound dual-engine (puremagic default + libmagic fallback per brief). Folder-minted `_WORKER_RETRY` ([02.6]); composed by io ([05](d) inversion) |
| exchange/metadata | 8-9 | `read`/`write`, `Metadata` receipt | Sound. `pyexiv2` redundant retention ([03]); iptcinfo3/xmp-toolkit correctly superseded |
| media/analysis | 6 (fences broken) | `produce→RuntimeRail`, `Media` | E5 THREE phantom imports (:48-49); folder-minted `_WORKER_RETRY` |
| media/subtitle | 7 | `produce→RuntimeRail`, `Media` | E3 `shaped_rgba` phantom (:50,299); folder-minted `_WORKER_RETRY` |
| media/container | 8-9 | `encode→RuntimeRail`, `Media` (spine) | Sound spine. Defines `_WORKER_RETRY` folder-minted stamina ([02.6]) |
| media/audio | 8-9 | worker over container, `Media` | Sound. Composes container/filtergraph |
| media/timeline | 8-9 | `render→RuntimeRail`, `Media` | Sound. Strongest ArtifactPipeline/CPM exemplar (multi-parent clip DAG) |
| media/synthesis | 8-9 | `produce→RuntimeRail`, `Media` | Sound. numpy oscillator bank + audio encode |
| media/filtergraph | 8-9 | `build_graph`, no receipt (substrate) | Sound. E7 REFUTED (no derive coupling); uses `@dataclass` for `WiredGraph` (Callable payload) — minor |
| package/codec | 8-9 | `pack`/`unpack`, `Bundle` (spine) | E10 cycle head + private reach (:37-38); zlib-ng home confirmed |
| package/archive | 9 | worker over codec, `Bundle` | Sound. E10 lazy back-edge (:92) |
| package/delta | 9 | worker over codec, `Bundle` | Sound. E10 lazy back-edge (:30); detools fully mined |

---

## [05] CHARTER-AS-IT-SHOULD-BE (highest-leverage lane rewrites)

1. **`drawing/` mark & fill geometry** must consume a foundational **`graphic/pattern` (StrokeFamily/PatternSpec, V2)** and a **`drawing/geometry`/symbol mark owner (V5, proportion rows)**: one terminator geometry owner feeding all three lowerings (SVG/DXF/schemdraw), one repeating-fill owner feeding ezdxf `set_pattern_fill` + drawsvg `<pattern>` + pathops-clipped geometry + legend swatches. Kills the three-mechanism triad, the SVG/DXF magic duplication, the eleven borrowed ACAD names, and the invented swatch hex.
2. **`drawing/` text** routes every glyph through `typography` (V3): dimension/annotate/symbol stop importing ziafont/ziamath directly; `_prose_group` consumes `PositionedGlyphRun` *positions* (not just cluster indices); a `typography` metrics owner replaces standard's fontTools leak; the `SHAPE_QA` golden oracle proves equivalence.
3. **`drawing/` color** rehomes to `graphic/color/derive` (V4): the four chart/spec importers (annotate:49/detail:46/schedule:44/symbol:45) rewire; detail's prose-vs-fence resolves; literal ink (`#111111`, `#1a1a1a`, invented material hex) becomes derive contrast/palette picks.
4. **`visualization/table` pandas materialization (leg 2)** so `schedule` and `register` render (E5); then register's `_index_ops` Stub+Color verify in leg-3 cold, and register's `Classification.code`/`discipline` compose classify `ClassCode`/standard `Discipline` (E9), killing the parallel `ClassificationSystem` (E6).
5. **Corpus-wide `ArtifactWork` entry + runtime retry rewire (leg-1 reconcile)**: media/detect fold `_WORKER_RETRY` onto runtime `offload(retry=RetryClass.OCCT)`; the folder-minted `stamina.AsyncRetryingCaller` dies. The E1 convergence (layered/indesign `emit`) generalizes to every producer.
6. **`LayerPlan` foundational owner (V14)** re-homes `export/layered.Layer` as a semantic tree below its six projectors; `export/layered` composes, never owns.
7. **Package cycle break (V8/E10)**: `Bundle`/`CompressionAlgo`/`CodecProfile` move to a neutral shared-vocabulary site; codec stops eager-importing archive/delta private workers; transmittal→codec edge is ledgered.
8. **`exchange`/`core` inversions ([05] a/d)**: freeze `ConformanceVerdict` as a wave-1-consumed contract (or re-home below receipt); re-home `Detect`/`DetectIdentity` below raster (or re-wave). `pyexiv2` removed or given a named unique capability.
