# DRAFT — RASM-PY-ARTIFACTS structural blueprint — lens: PACKAGE-MINING-FIRST

Work OUTWARD from the `[04]` package-pressure obligations and the roster-closure table: every admitted package binds to ONE owning surface at operator depth, every census row rules on its condition, the shared-tier weave (`expression`/runtime lane/`identity`/`ArtifactReceipt`) composes in every fold — and the page-set is DERIVED as the housing for those bindings, never the reverse. A page exists because a package (or a shared-tier concern) needs an owner; a missing owner is a zero-consumer admission the page-set closes; a thin owner is rebuilt to operator depth, never pruned. Integration is the closure; removal is only the four `[PYPROJECT_RECONCILIATION]` census defaults, each ruled on its stated survival condition.

THESIS: the corpus rot is that admitted packages are consumed BELOW the depth their surface reaches AND at the WRONG owner — `colour-science`/`coloraide` orphaned behind a bare `NDArray` alias, `drawsvg.Pattern`/`schemdraw`-catalog/`pvlib` zero-consumer, `uharfbuzz` shaping run beside seven hand-built `ziafont`/`ziamath` sites, `pikepdf` plate-authoring absent while its decode half is cataloged, `tifffile` reaching only one extratag call. The package-mining pass rehomes each package to its canonical owner, mints the missing owners the orphan admissions demand, and threads the shared-tier weave (lane/retry/identity/receipt/Map) through every fold — the entry contract, seam acyclicity, and track rebind fall out as the structural consequence.

---

## [A] PACKAGE -> OWNER BINDING MAP (the roster closure; this lens's spine)

Every admitted artifacts package + the load-bearing shared-tier substrate, bound to its canonical owning surface at the depth its `.api` catalog proves. `(census)` = a `[PYPROJECT_RECONCILIATION]` dead-marker or removal row, ruled in `[F]`. `(NEW-owner)` = the admission is currently orphan/thin and the page-set mints/rebuilds its owner.

### SHARED-TIER WEAVE (composed in EVERY fold, never re-minted)
| substrate | owner surface | integration at operator depth |
|---|---|---|
| `expression` `Result`/`Option`/`Block`/`Map`/`@tagged_union` | every page | the ONE ADT + dispatch spine; `Map.of_seq` is THE dispatch-table rail (`[I]` completes the frozendict rebind); `traverse`/`sequence` thread producer batches |
| `rasm.runtime.identity` `ContentIdentity.of` / `IdentitySource.lift` / `CANONICAL_POLICY` | every producer | every ContentKey rides `of(fmt, source)` over canonical bytes under `CANONICAL_POLICY` (runtime `[V7]`); buffer/stream/merkle via `lift`; never `repr()`/`str()` bytes, never a local `IdentityPolicy()` allocation |
| runtime `lanes.offload(retry=RetryClass.OCCT)` on the isolation-modality axis | every native/subprocess/press/raster/render/media seam | the THREAD/PROCESS bands under runtime-owned bounds; worker-death retry rides `offload(retry=)` spanning `anyio.BrokenWorkerProcess`/`BrokenWorkerInterpreter`; ZERO artifacts-minted `CapacityLimiter`, ZERO folder `stamina.AsyncRetryingCaller` |
| runtime `guarded` over the one `POLICY` table (incl. the `ORACLE` row) | `exchange/conformance`, `document/lens`, `graphic/raster/io` | retry ONLY at flaky external oracles (veraPDF/JHOVE/`validate_jhove`) through the `ORACLE` policy row; never around pure transforms |
| runtime `ReceiptContributor` structural port | every producer's `contribute` fold | structural conformance, NO subclass (geometry `[V5]` no-subclass discipline); the 22-case `ArtifactReceipt` is the ONE rail |
| runtime metric recorder (table-keyed domain-histogram, runtime `[V5]`) | `core/receipt` `contribute` | production duration/byte-volume/compression-ratio facts thread into the ONE runtime metric stream (`[06]` gate #1, REALIZED — `[H]`) |
| `structlog`+`opentelemetry` span per fold | owned aspects | span per fold through the owned weave, never inline opens |
| `msgspec` | every ingress + wire projection | one-shot admission at ingress; `msgspec.Struct` wire projections; the hashable evidence bands (`[I]` band-vs-table ruling) |
| `numpy` | `raster/*`, `color/*`, `diagram/solar`, `media/synthesis`, `scene` | vectorize where a page loops rows |
| `pydantic`/`pydantic-settings` | boundary-only | provider ingress models + settings; never an interior owner |

### FOUNDATIONS PLANE (leg 1) — the primitive owners

| package(s) | owning page | mine-to-depth binding |
|---|---|---|
| `svgelements` | `graphic/vector/algebra.md` (NEW) | `SVG.parse`/`Path`/`Matrix`/`bbox`/`SVG.select`, `Point.distance_to`/`angle_to`/`polar_to`/`reflected_across`/`matrix_transform`, `Length.to_mm` unit egress; metric arc-length point-at-distance, polyline decimation, area-weighted centroid; tolerance/DPI magic (`0.1`/`0.25`/`ppi=96.0`/`1e-3`) lifted to `Map` policy rows |
| `skia-pathops` | `graphic/vector/boolean.md` (NEW) | `addPath`/verb-level build, boolean/offset/stroke-to-outline/warp/wind/region/contains; composes `algebra` |
| `drawsvg` + `resvg-py` | `graphic/vector/emit.md` (NEW) | `Drawing`/`Group`/`Raw`/`Path` replacing ALL f-string SVG, gradient primitives (V13 aesthetics); `resvg` SVG->PNG; `RenderPolicy`; text-on-path composing `typography/shape.on_path` |
| `drawsvg.Pattern` + `skia-pathops` + `ezdxf.tools.pattern` | `graphic/vector/pattern.md` (NEW, V2) | `StrokeFamily`/`PatternSpec`/`DensityLaw` generator; THREE lowerings from one spec — `set_pattern_fill(definition=)`, `drawsvg.Pattern` tiles, pathops clip against a region; `HatchMaterial` -> `PatternSpec` seed rows |
| `colour-science` (models/CAM16/spectral) + `coloraide` | `graphic/color/derive.md` (REBUILD) | CIE convert/CAM16/spectral + gamut-map/CVD/harmony/WCAG-contrast/perceptual-palette/`interpolate`/`mix`/`mask`; `write_LUT` LUT-authoring; `Palette`/`hex_ramp` REHOMED here (from `chart/spec`); `AdaptMethod` sole owner |
| `colour-science` (difference/appearance) + `colour-cxf` | `graphic/color/measure.md` (NEW, from derive split) | the 28-member `Metric` delta_E/appearance concern; `read_cxf`/`write_cxf` over `cxf3.CxF` — the CxF3 measurement/exchange split |
| `pyvips` (ICC) + `pikepdf` (plate) | `graphic/color/managed.md` (REBUILD) | `icc_transform` soft-proof egress; `Separation`/`DeviceN` PLATE-AUTHORING over pikepdf raw object model (consuming its own `SpotChannel` decode — the write surface absent from `.api/pikepdf.md:124`, authored with the verdict); TAC hardens to a policy gate + overprint/rich-black rows |
| `pillow` + `pyvips` + `tifffile` | `graphic/raster/io.md` (REBUILD) | pillow IO/ImageChops + pyvips `block_untrusted_set` hardening + `Source`/`Target` streaming + `Foreign*` enums; `tifffile` GeoTIFF tags, `aszarr`/`memmap`/`TiffSequence` lazy-tiled, `validate_jhove` oracle; exports bare `render_png`/`montage` (E5); owns consolidated dispatch, RasterOp modal |
| `scikit-image` (produced) + `pillow` (procedural) | `graphic/raster/process.md` (KEEP) `(census)` | 97-member produced-raster engine + ImageChops/ImageMath/gradients/noise/Color3DLUT; owns `TransformInput`/`TransformArm` substrate |
| `scikit-image` (measured) | `graphic/raster/measure.md` (KEEP) `(census)` | 42-member measured engine; exports bare `frame_similarity` (SSIM, E5); OWNS `RasterFact` (consolidated from io duplicate) |
| — | `graphic/marks/vocabulary.md` (NEW, cycle-break) | neutral site: `Symbology`/`DecodeSource`/`MarkOp`/`MarkFault`/`MarkPayload` — both encode and decode import DOWN (breaks the encode<->decode census cycle) |
| `segno` + `python-barcode` + `zxing-cpp` (encode) | `graphic/marks/encode.md` (REBUILD) | `QRCode`/`make_micro`, `get_barcode_class` SVG writers, `create_barcode`; imports vocabulary + `measure.RasterFact` down |
| `zxing-cpp` (decode) | `graphic/marks/decode.md` (KEEP+fix) | `read_barcodes` inverse; imports vocabulary down |
| `uharfbuzz` + `python-bidi` + `blackrenderer` + `vharfbuzz` + `fonttools.unicodedata` | `typography/shape.md` (REBUILD) | `Face`/`Font`/`Buffer`/`shape` + UAX#9 reorder + COLRv1 `BlackRendererFont`/`PaintFormat`/`Surface`; EXPORTS `shaped_rgba(fragment, style) -> bytes` (subtitle phantom); fallback-chain RESOLUTION with `fontTools.unicodedata.script()`/`script_extension()` ISO 15924 itemization; font-metrics surface (`hb_ot_metrics`/`get_font_extents`) — THE text engine every glyph routes through; `vharfbuzz.serialize_buf` SHAPE_QA rail |
| `fonttools` + `opentype-feature-freezer` + `uharfbuzz` (metrics) | `typography/font.md` (KEEP+extend) | subset/instance/`fvar`/`STAT`/`SVGPathPen`; `RemapByOTL` FREEZE arm; `ScriptTags` seam for shape; the receipt-law EXEMPLAR (sibling reference) |
| `uniseg` + `pyphen` + `PyICU`(gated) | `typography/layout.md` (KEEP+improve) | Knuth-Plass + UAX#14 break + `DataInt.data` `(change,index,cut)` orthographic channel (carried for emit to APPLY, V12) |
| `ziamath` + `uharfbuzz` (math) + `typography/shape` (outline) | `typography/math.md` (NEW, V3) | THE math owner the 7 pages hand-build; `ziamath` typeset composing shape's `to_svg_path` (ziafont collapses into shape's outline) backed by uharfbuzz `Face.has_math_data`/`get_math_constant(OTMathConstant)`/`get_math_glyph_variants`/`get_math_glyph_assembly` layout-quality tier |
| `colour-science` (via derive) + regime pens + shape/font (via) | `graphic/style.md` (NEW, V13) | style-as-DATA theme rows: color system through derive, stroke hierarchies from regime pens, grids/figure-ground, TYPE SYSTEM (type-scale/leading/tracking/variable-axis via font `INSTANCE`/`FREEZE` + shape `FeatureSpec` presets), SHEET FAMILY (title-block grid variants + page-master rows) |
| — | `graphic/layer.md` (NEW, V14) | `LayerPlan` semantic layer TREE (ISO 13567 names, discipline/kind/z-order rows, nested groups) — the foundational owner the six pre-egress projectors reach without an upward edge; `export/layered` COMPOSES, never owns |
| — (pure ISO/NCS data) | `drawing/regime.md` (NEW, `[05](b)` re-home) | the regime VOCABULARY slice: `Discipline`/`ScaleRatio`/`SheetId`/`Terminator`/`LineWeight`/`TextHeight`/`LayerName`/`LineType`/`HatchMaterial` + AIA/ISO 13567/NCS codecs + ISO 216/5455 + pen/lettering/hatch-material BIND rows; NO ezdxf (that stays `drawing/standard`, leg 3) |
| `puremagic` + `python-magic` | `exchange/detect.md` (KEEP, RE-WAVED leg 1, `[05](d)`) | dual-engine format-ID ingest gate; leaf ingress substrate re-waved below raster |
| `rustworkx` (CPM/DAG) | `core/plan.md` (KEEP, made REAL) | `PyDiGraph` topology + CPM `Schedule`; EXPORTS `ArtifactWork` node + `ArtifactPipeline.of(nodes)` construction API (`[C]`) |
| runtime `ReceiptContributor` + `msgspec` bands | `core/receipt.md` (KEEP+fix) | the ONE `ArtifactReceipt` 22-case union; `ColorReceipt` collapses IN; `ArtifactKind` DERIVED from the case roster (kills hand-synced `_KEYS`); RE-HOMES `ConformanceVerdict` (`[05](a)`); `contribute` threads gate-#1 metrics + the gate-#2 artifacts-origin `graduates()` producer |

### MID PLANE (leg 2) — the visual/document producers

| package(s) | owning page | mine-to-depth binding |
|---|---|---|
| `great-tables` + `polars` + `kiwisolver` | `visualization/table.md` (REBUILD) | `GT(frame.to_pandas())` SAFE FLOOR (polars `.style` demoted to probed), `tab_options`/`write_raw_html`/`with_locale`; pagination/continuation-across-sheets; kiwisolver column-width solve; units-sub-rows; column-discipline ingress (`data/tabular` self-describing frames) |
| `altair` + `matplotlib` | `visualization/chart/spec.md` (REBUILD) | typed `MARKS`/`CHANNELS`/`TRANSFORMS`/`COMPOSITION` grammar algebra (the `Vega(dict)` case DIES) — `@theme.register` typed `ThemeConfig`/`*Kwds`, `mark_geoshape`/`project`/`topo_feature`/`graticule`, `transform_regression`/`loess`/`density`/`quantile`, `expr` namespace; matplotlib publication `rcParams` bound by V13; imports `derive.Palette` (V4) |
| `vl-convert-python` + `lets-plot`(census) + `vegafusion` | `visualization/chart/export.md` (REBUILD, absorbs transform) | `svg_to_png/jpeg/pdf` the ONE chart rasterizer, `register_font_directory` font-identity loop with fonttools/uharfbuzz, `get_local_tz`/`format_locale` deterministic axes; lets-plot theme/flavor/element seam OR narrowed charter; `vegafusion.runtime.pre_transform_spec`/`pre_transform_extract` (transform.md's pre-pass absorbed) |
| — | `visualization/diagram/glyphset.md` (REBUILD) | dead-carrier purge (`EndCap`/`SubLayout`/`TextRun`/`Port.at`/`corner` consumed or deleted) + V15 area/polygon glyph case |
| `pyelk` + `fast-sugiyama` + `grandalf`(census) + `rustworkx` + `kiwisolver` | `visualization/diagram/layout.md` (REBUILD) | 5-engine coordinate assignment; typed ELK layout-option vocabulary over `.api/pyelk.md` `layoutOptions` keys (stringly literals die); V15 machinery — plan-geometry ingress (typed coordinate columns), area-proportion law, CIRCULATION/SECTION_CALLOUT spring-defaults die for plan-anchored `Constrained`; consumes `solar` |
| `drawsvg` + `drawpyo` | `visualization/diagram/draw.md` (REBUILD) | drawsvg named-layer + drawpyo `load_diagram`->mutate-by-`get_by_id`->`File.write` `.drawio` round-trip + `PageSize` presets; V3 rewire (route labels/math through `typography/math`+`shape`, NOT local ziafont/ziamath); 19-shape parity; `_INK` -> derive contrast pick |
| `schemdraw` | `visualization/diagram/schematic.md` (NEW, V10) | the 226-element `schemdraw.elements` catalog (logic/flow/dsp, Kmap/Timing/BitField, `svgconfig.text='path'`) — the named-symbol schematic class the five marks cannot express (the `Segment*`/`ElementCompound` spine STAYS with `drawing/symbol`) |
| `pvlib` (NEW admission) + owned closed-form kernel | `visualization/diagram/solar.md` (NEW, V15) | `solarposition` SPA azimuth/altitude, sunrise/transit/solstice, numpy-vectorized date sampling; sun-path furniture (horizon circle, altitude rings, compass ticks, labeled date arcs) as generated geometry over `ProjectionKind`; owned kernel is the declined-admission fallback; geometry's AGPL `Sunpath` stays OUT (geometry `[V1]` pin) |
| `pyvista` + `vtk` | `scene/render.md` (REBUILD) `(census)` | `view_xy`/`view_isometric` standard-view family; `vtkPolyDataSilhouette`/`vtkFeatureEdges`/`enable_hidden_line_removal` (catalog authored with verdict); section cut+poché feeding V2 patterns; `vtkGL2PSExporter` 3D->vector; date/latitude directional sun policy (beyond parameterless `enable_shadows`); typed mesh ingress (geometry `mesh`/data `MeshPayload`) |
| `vtk`/`pyvista` (export) | `scene/export.md` (KEEP+fix) `(census)` | `SceneTarget`/`Sink`/`_EXPORTER` + GL2PS target; render<->export cycle broken (takes plotter as an opaque capsule, imports NO render); zlib_ng clone DELETED (bundling routes to package/archive at the consumer) |
| `usd-core` | `scene/stage.md` (KEEP) `(census)` | USD/USDZ `PrimKind`/`PointInstancer`/`Camera`/`Material`/`UsdSemantics.LabelsAPI`; payloads/variants/purposes |
| `svgelements` + `resvg-py` + `pillow` | `composition/compose.md` (improve) | `Figure` placement composing `graphic/vector` one-hop; `_GATE`/`_TRANSIENT` -> runtime lane |
| `pdfimpose` + `pymupdf` | `composition/imposition.md` (REBUILD) | OWNS the ONE press fold (`_mint_groups`/`_draw_one`/`_oc_state`/`_configure_layers` — sheet+egress COMPOSE it, V7); `pdfimpose.schema.AbstractImpositor`/`impose`/`Margins` saddle/wire/cards/cut-fold/signature; completes the `frozendict`->`Map` migration |
| `reportlab`/`typst`/`weasyprint` + `pymupdf` | `composition/sheet.md` (REBUILD, FLAGSHIP constructor) | composes imposition's press fold; `_SIZES` from ISO 216 halving + `pymupdf.paper_rect`; 2D ISO 7200 title-block grid; `regime.lettering()`+shaped runs (kills Helvetica magic); cover-sheet/index/general-notes ISSUE completion; page-box law (MediaBox/TrimBox/BleedBox/CropBox); V13 sheet-family variants; imports `regime` (leg 1, not `standard`) |
| — | `document/model.md` (KEEP) | the 11-variant `DocumentNode` tree; fix "ten-variant" stale count |
| `reportlab`/`weasyprint`/`typst` + `python-docx`/`odfpy`/`ruamel-yaml`/`tomlkit`/`xlsxwriter` + `docxtpl`/`python-pptx` + `pyphen` + `pikepdf`/`pdf-oxide`/`pymupdf`(forms) | `document/emit.md` (REBUILD, FLAGSHIP constructor, absorbs format) | every backend lowers FROM the tree; composes typography seams (shaped runs/bidi/itemization, V3); ~48-field `EmitSpec` -> grouped value objects; weasyprint `@page`/`string-set`/`target-counter` running content + cross-arm TOC; pyphen orthographic APPLICATION at breaks; FORMS owner per-target (`pdf_oxide DocumentBuilder`/`pikepdf.add_field`/`pymupdf.add_widget`); `core/format` DISSOLVES here (`TemplatePayload` admission + `bound()` fan-out + office template-clone as emit rows); `typst.Compiler` world reuse/`query`/`eval` |
| `jinja2` + `papermill` + `nbclient` + `nbconvert` + `jupytext` | `document/report.md` (REBUILD) | `SandboxedEnvironment`; `execute_notebook`; `NotebookClient` + `CellExecutionError`/`CellTimeoutError`/`DeadKernelError` rail; `get_exporter`/`PDFExporter`; terminal PDF routes THROUGH emit (`Pdf.from_html` dup dies); page-numbered TOC |
| `pikepdf`/`pypdf`/`pymupdf` + `msoffcrypto-tool` | `document/egress.md` (improve) | finishing close; composes imposition's saddle fold (kills the egress clone + the false `[EGRESS_DISTINCT]`); `OfficeFile`/`load_key`/`decrypt` |
| `pdf-oxide`/`pymupdf`/`pdfplumber`/`python-calamine`/`ocrmypdf` | `document/lens.md` (KEEP+V12) | recover-TO inverse; OCR pre-flight gate (`pdfinfo.PdfInfo`/`PageInfo.has_text` SKIP on text-bearing, `is_tagged`/`has_acroform`/`has_signature` routing) + in-package PDF/A egress (`speculative_pdfa_conversion`/`file_claims_pdfa` feeding the veraPDF oracle) + `run_hocr_pipeline` two-phase hOCR; `CalamineWorkbook`/`iter_rows` |
| `pikepdf` + `pdf-oxide` | `document/tagged.md` (KEEP) | two-source PDF/UA StructTreeRoot author+audit reconciliation |

### AEC/EGRESS PLANE (leg 3) — the drawing/delivery/exchange/media/package producers

| package(s) | owning page | mine-to-depth binding |
|---|---|---|
| `ezdxf` (symbol tables) | `drawing/standard.md` (REBUILD, ezdxf lowering) | `seed`/`graphics`/`dimstyle`/`hatch`/`rgb`/`paper_factor` symbol-table lowering; composes `regime` (leg 1 vocabulary) + `pattern` (V2, kills the 11 borrowed ACAD names) + font metrics (kills fontTools leak); the standard->dxf seed seam (export/dxf composes it) |
| `ezdxf` (dims) + `drawsvg` + `kiwisolver` | `drawing/dimension.md` (REBUILD) | ISO 129-1 `add_*_dim`; V3 (text via `typography/math`+`shape`, NOT ziafont/ziamath); V5 (compose symbol's terminator geometry); ISO 286 fits + ISO 1101 GD&T frames + DIMALT |
| `schemdraw` (Segment* spine) + `ezdxf` (blocks) + `drawsvg` + `svgelements` | `drawing/symbol.md` (REBUILD, V5 mark-geometry OWNER) | the ONE parametric mark-geometry owner: ISO 129-1 terminators, north/scale-bar/grid-bubble/section-marker proportion rows feeding BOTH SVG+DXF lowerings, revision triangles/clouds; rotation via vector `Matrix`+`Point.polar_to`/`reflected_across` (hand-rolled trig dies) |
| `drawsvg` + `ezdxf` (multileader) + `kiwisolver` | `drawing/annotate.md` (REBUILD) | ISO 128-2 leaders/keynotes; V3 (consume PositionedGlyphRun POSITIONS not re-shape); V5 (compose symbol terminators); welding ISO 2553 + surface-texture ISO 1302 + datum symbols; palette from derive |
| `ezdxf` (block store) + `rustworkx` | `drawing/detail.md` (REBUILD) | `SheetId`-typed refs; `PyDAG` cross-reference DAG; section/elevation callouts re-scoped to imported figures (V9 boundary); palette from derive |
| `great-tables` (via table) + `polars` | `drawing/schedule.md` (REBUILD) | lowers into `visualization/table` (pandas floor lands leg 2; schedule cold-verifies); legend swatches consume `pattern` (V2, kills hand-rolled crosshatch); palette from derive (kills invented hex) |
| `document/model` (lowers into) | `specification/section.md` (cold+additive) | full §5.1.7 Schematron; composes classify `ClassCode` |
| `rustworkx` (crosswalk) | `specification/classify.md` (cold+additive) | Uniclass 2015 + OmniClass element rows (kills register's parallel enum); `Discipline` composed from `regime` |
| `great-tables`+`xlsxwriter`+`openpyxl`+`lxml`+`polars` + `rustworkx` | `delivery/register.md` (REBUILD) | fixes E5 silent blocked-Stub (pandas floor); composes classify `ClassCode` + regime `Discipline` (kills parallel `ClassificationSystem`); `TopologicalSorter`/`transitive_reduction`/`ancestors`/`descendants` DAG assembly; column-discipline ingress |
| `lxml` + `rustworkx` + `imposition`/`archive`/`conformance`/`credential`/`register` | `delivery/transmittal.md` (cold+additive) | distribution matrix + acknowledgement round-trip; cross-reference closure; ledgers the `package/codec` edge (E7) |
| `ezdxf` (full) | `export/dxf.md` (REBUILD, V11) | paperspace/viewport authoring; standard->dxf seed seam REAL (composes `standard.seed`, kills the `_table_entry` dup); ATTRIB/Image/MPolygon/underlay; `import_blocks` beside `import_entities`; by-NAME version/unit; `ezdxf.path`/`ezdxf.math` vector bridge; `of()`+`contribute()` converges to `emit()` |
| `drawsvg`+`pymupdf`+`pikepdf`+`psd-tools`+`PhotoshopAPI`(census)+`psdtags`+`tifffile`+`imagecodecs`+`pyvips`+`lxml`+`stream-zip` | `export/layered.md` (REBUILD, V14) | COMPOSES `LayerPlan` (leg 1) — no longer owns `Layer`; PSD group folders + 12 native blend modes (psd-tools standing author), PDF OCG `/Order` tree, structured SVG `<g>` (drawsvg Group), IDML layers, layered TIFF/ORA; `tifffile` GeoTIFF site-plan raster; `imagecodecs` `*_encode`/`*_decode`/`.available` |
| `simpleidml` | `export/indesign.md` (KEEP) | IDML step fold (E1 convergence proof) |
| `pyexiftool` + `pikepdf` + `av` + `pillow` | `exchange/metadata.md` (cold, pyexiv2 REMOVED) | `MetaCarrier` one-pass; `pyexiv2` REMOVED (`[F]` — subset of pyexiftool, GPL-3.0 in-process copyleft; carrier already owned out-of-process) |
| `c2pa-python` | `exchange/credential.md` (KEEP) | `Builder` sign/`Reader.get_validation_state`/`C2paSigningAlg` (V6 entry rewire only) |
| `pyhanko` | `exchange/conformance.md` (KEEP) | PAdES B-B/T/LT/LTA + `/DocTimeStamp`/`augment`/`reserve`/`audit`; composes `core/receipt.ConformanceVerdict` (re-homed, imports DOWN); veraPDF/JHOVE oracle rides `guarded` ORACLE row |
| `av` + `pysubs2` | `media/{container,audio,timeline,subtitle,analysis,synthesis,filtergraph}.md` (cold, 2 rebuilds) | the media spine (8-9 grade); `_WORKER_RETRY` folds onto `offload(retry=RetryClass.OCCT)` (leg-1 reconcile); `subtitle` gains real `shaped_rgba`; `analysis` REBUILD (E5 phantoms `render_png`/`montage`/`frame_similarity` now real io/measure surfaces) |
| — | `package/bundle.md` (NEW, cycle-break) | neutral vocabulary: `Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleManifest` + `PackWorker` Protocol — codec/archive/delta import DOWN (breaks the codec<->archive<->delta census cycle; codec stops eager-importing private workers) |
| `zstandard`+`lz4`+`brotli`+`zlib-ng` | `package/codec.md` (cold, zlib-ng re-entry) | single-blob compression; `zlib-ng` re-tag `[DATA]`->`[ARTIFACTS]` (its live GZIP-band consumer already meets the re-entry condition) |
| `py7zr`+`stream-zip`+`stream-unzip` | `package/archive.md` (cold+growth) | `SevenZipFile.writeall`/`extractall`/`test` + `stream_zip`/`stream_unzip`; the multi-entry `unpack`/`list`/`BundleManifest` inverse (carried growth); reproducible-ZIP owner scene bundling routes to |
| `detools` | `package/delta.md` (cold+growth) | `create_patch`/`apply_patch`/`patch_info`; the parent-keyed delta row (carried growth) |

---

## [B] PAGE-SET TABLE (per-page action + engine lowering + tier + entry + seams + wave)

Columns: `PATH` | `ACTION -> engine lowering` | `TIER` (generate/bind/select where the page touches the geometry/style triads, else N/A) | `ENTRY` (`emit()` receipt case for producers; NONE for substrate/vocabulary) | `KEY SEAMS` (in <- / out ->; one anchor per seam). Owner roster + shapes are in `[A]`. Engine lowering vocabulary: `new` | `rebuild` | `improve` | `deletePages[...]` | `absorb {into, from}`.

### LEG 1a — FOUNDATION OWNERS

- `graphic/vector/algebra.md` | SPLIT (from `vector.md`) -> `new` | TIER generate | ENTRY NONE (substrate) | in <- runtime `identity`; out -> `vector/boolean`,`vector/emit`,`vector/pattern`,`drawing/symbol`,`composition/compose`
- `graphic/vector/boolean.md` | SPLIT -> `new` | TIER generate | NONE | in <- `vector/algebra`; out -> `vector/pattern`,`drawing/annotate`(scallop offset),`export/dxf`
- `graphic/vector/emit.md` | SPLIT -> `new` + `absorb {into: vector/emit, from: vector.md}` | TIER generate | NONE | in <- `vector/algebra`,`vector/boolean`,`typography/shape`(on_path); out -> `composition/compose`,`drawing/*`
- `graphic/vector/pattern.md` | NEW (V2) -> `new` | TIER generate | NONE | in <- `vector/algebra`,`vector/boolean`,`drawing/regime`(HatchMaterial rows); out -> `drawing/standard`,`drawing/schedule`,`export/dxf`,`export/layered`,`scene/render`(poché)
- `graphic/color/derive.md` | REBUILD -> `rebuild` + `absorb {into: derive, from: chart/spec Palette/hex_ramp}` | TIER generate | NONE | in <- runtime `identity`; out -> `color/measure`,`color/managed`,`chart/spec`,`diagram/draw`,`scene/render`,`drawing/{standard,dimension,symbol,annotate,detail,schedule}`,`style`
- `graphic/color/measure.md` | SPLIT (from derive) -> `new` | TIER N/A | NONE | in <- `color/derive`; out -> `color/managed`(SpotChannel/CxF3)
- `graphic/color/managed.md` | REBUILD -> `rebuild` | TIER select | `emit() -> ArtifactReceipt.Preview` | in <- `color/derive`,`color/measure`; out -> `graphic/raster`(MANAGED),`document`(ICC raster),`export/layered`(plate)
- `graphic/raster/io.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> Iterable[ArtifactWork]` (Preview per op) + bare `render_png`/`montage` substrate exports | in <- `exchange/detect`(re-waved),`raster/measure`(RasterFact),`color/managed`; out -> `media/analysis`,`composition/compose`
- `graphic/raster/process.md` | KEEP -> `improve` | TIER N/A | NONE (worker substrate) | in <- (leaf); out -> `raster/io`,`raster/measure`
- `graphic/raster/measure.md` | KEEP -> `improve` (owns `RasterFact` consolidated) | TIER N/A | NONE | in <- `raster/process`; out -> `raster/io`,`marks/encode`,`marks/decode`,`media/analysis`(frame_similarity)
- `graphic/marks/vocabulary.md` | NEW (cycle-break) -> `new` | TIER N/A | NONE | in <- (leaf); out -> `marks/encode`,`marks/decode`
- `graphic/marks/encode.md` | REBUILD -> `rebuild` | TIER generate | `emit() -> Iterable[ArtifactWork]` (Preview per mark) | in <- `marks/vocabulary`,`raster/measure`; out -> `export/layered`(LAYERED)
- `graphic/marks/decode.md` | KEEP+fix -> `improve` | TIER N/A | NONE (recovery substrate) | in <- `marks/vocabulary`,`raster/measure`; out -> (consumers)
- `graphic/style.md` | NEW (V13) -> `new` | TIER select | NONE | in <- `color/derive`,`drawing/regime`,`typography/{shape,font}`; out -> `chart/spec`,`diagram/draw`,`table`,`composition/sheet`,`document/emit`,`drawing/*`
- `graphic/layer.md` | NEW (V14) -> `new` | TIER N/A | NONE | in <- (leaf); out -> `composition/{compose,imposition,sheet}`,`diagram/draw`,`marks/encode`,`export/layered`,`drawing/*`
- `typography/font.md` | KEEP+extend -> `improve` | TIER N/A | `emit() -> ArtifactWork` (Font-embed case) | in <- runtime `identity`; out -> `typography/{shape,math}`(ScriptTags/FREEZE),`document/emit`(FONT_EMBED),`style`(INSTANCE/FREEZE)
- `typography/shape.md` | REBUILD -> `rebuild` | TIER generate | NONE (produces `PositionedGlyphRun`; ContentKey memo cache, not an artifact) | in <- `typography/font`; out -> `typography/{layout,math}`,`vector/emit`(on_path),`document/emit`,`composition/compose`,`media/subtitle`(shaped_rgba),`drawing/{dimension,annotate,symbol}`,`diagram/draw`,`style`
- `typography/layout.md` | KEEP+improve -> `improve` | TIER N/A | NONE (produces `LineBrokenRun`) | in <- `typography/shape`; out -> `document/emit`(LAYOUT+pyphen channel),`drawing/annotate`
- `typography/math.md` | NEW (V3) -> `new` | TIER generate | NONE (produces math SVG paths) | in <- `typography/{shape,font}`; out -> `drawing/{dimension,annotate}`,`document/model`,`diagram/draw`
- `drawing/regime.md` | NEW (`[05](b)` re-home from `standard.md`) -> `new` | TIER bind | NONE | in <- `color/derive`(pens),`vector/pattern`(HatchMaterial),`typography/{shape,font}`(lettering metrics); out -> `drawing/standard`,`composition/sheet`,`drawing/detail`,`specification/classify`,`delivery/register`,`style`
- `exchange/detect.md` | KEEP (RE-WAVED leg 1, `[05](d)`) -> `improve` | TIER N/A | NONE (ingest gate) | in <- (leaf puremagic/python-magic); out -> `raster/io`,`document/lens`,`document/emit`

### LEG 1b — SPINE + CORPUS-WIDE ENTRY REWIRE

- `core/plan.md` | KEEP (made REAL) -> `improve` | TIER N/A | NONE (exports `ArtifactWork` + `ArtifactPipeline.of`) | in <- runtime `lanes`(Keyed/StagePlan),`core/receipt`(contribute); out -> `composition/sheet`,`document/emit`,`delivery/transmittal` (the constructing owners)
- `core/receipt.md` | KEEP+fix -> `improve` (absorbs `ColorReceipt`; RE-HOMES `ConformanceVerdict`; gate-1 contribute; gate-2 artifacts-origin producer) -> `absorb {into: receipt, from: color/derive ColorReceipt}` | TIER N/A | NONE | in <- runtime `ReceiptContributor`+metric recorder, `compute/graduation`(hub, gate 2); out -> EVERY producer, `exchange/conformance`(ConformanceVerdict down)
- `core/format.md` | DELETE -> `deletePages[core/format.md]` + `absorb {into: document/emit, from: core/format}` (executes in leg 2, condemned-but-intact until then) | — | — | —
- CORPUS-WIDE: leg-1b reconcile has whole-repository write authority; lands `emit() -> ArtifactWork | Iterable[ArtifactWork]` on EVERY producer (all legs), the `frozendict`->`Map` + `content_identity`->`identity` + `[RESEARCH]`-purge rebind, and the `_WORKER_RETRY`/`CapacityLimiter`->`offload` fold — leg-2/3 rebuilds INHERIT and cold-verify these, never rewire.

### LEG 2 — MID PLANE

- `visualization/table.md` | REBUILD -> `rebuild` | TIER select(style rows) | `emit() -> ArtifactReceipt.Preview` | in <- `data/tabular`(SHAPE column-discipline),`style`; out -> `drawing/schedule`,`delivery/register`,`composition/sheet`
- `visualization/chart/spec.md` | REBUILD -> `rebuild` + `absorb {from: chart/spec Palette/hex_ramp -> derive}` | TIER select | NONE (produces ChartSpec) | in <- `color/derive`(Palette),`style`; out -> `chart/export`
- `visualization/chart/export.md` | MERGE (transform in) -> `rebuild` + `absorb {into: export, from: chart/transform}` | TIER N/A | `emit() -> ArtifactReceipt.Preview` | in <- `chart/spec`,`typography/font`(register_font_directory); out -> `composition/compose`
- `visualization/chart/transform.md` | DELETE -> `deletePages[chart/transform.md]` (absorbed into export) | — | — | —
- `visualization/diagram/glyphset.md` | REBUILD -> `rebuild` | TIER N/A | NONE (vocabulary) | in <- (leaf); out -> `diagram/{layout,draw}`
- `visualization/diagram/layout.md` | REBUILD -> `rebuild` | TIER N/A | NONE (coordinates) | in <- `data/tabular`(GRAPH),`diagram/glyphset`,`diagram/solar`; out -> `diagram/draw`
- `visualization/diagram/draw.md` | REBUILD -> `rebuild` | TIER select | `emit() -> ArtifactReceipt.Preview` | in <- `diagram/layout`,`typography/{math,shape}`,`color/derive`,`layer`,`style`; out -> `export/layered`
- `visualization/diagram/schematic.md` | NEW (V10) -> `new` | TIER select | `emit() -> ArtifactReceipt.Preview` | in <- `typography/{math,shape}`,`layer`,`style`; out -> `export/layered`
- `visualization/diagram/solar.md` | NEW (V15) -> `new` | TIER generate | NONE (produces solar geometry) | in <- `vector/algebra`; out -> `diagram/layout`(SOLAR_ARC)
- `scene/render.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Scene` | in <- `scene/export`(SceneTarget),`geometry/mesh`+`data/MeshPayload`(typed ingress, DATA edge),`vector/pattern`(poché); out -> `media/container`(frames, DATA edge)
- `scene/export.md` | KEEP+fix (cycle break) -> `improve` | TIER N/A | NONE (export dispatch; plotter as capsule) | in <- (opaque plotter capsule); out -> `scene/render`
- `scene/stage.md` | KEEP -> `improve` | TIER N/A | NONE (USD authoring) | in <- (leaf usd-core); out -> `scene/{render,export}`
- `composition/compose.md` | KEEP+improve -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Preview` | in <- `vector/emit`,`layer`; out -> `export/layered`
- `composition/imposition.md` | REBUILD (press-fold OWNER) -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Egress` | in <- `layer`,`document`(PDF); out -> `composition/sheet`(press fold),`document/egress`(press fold),`delivery/transmittal`
- `composition/sheet.md` | REBUILD (FLAGSHIP constructor) -> `rebuild` | TIER select(sheet family) | `emit() -> Iterable[ArtifactWork]` (Drawing per sheet; SheetSet aggregate node whose parents are member keys) | in <- `drawing/regime`(ScaleRatio/SheetId — leg 1),`visualization/table`(scheduled bridge),`composition/imposition`(press fold),`style`,`layer`,`document/emit`(notes seam); out -> `delivery/register`(registered projection, DATA),`export/layered`
- `document/model.md` | KEEP -> `improve` | TIER N/A | NONE (DocumentNode tree) | in <- `data/tabular`(WIRE); out -> `document/{emit,tagged,report}`,`specification/section`
- `document/emit.md` | REBUILD (FLAGSHIP constructor; absorbs format) -> `rebuild` + `absorb {into: emit, from: core/format}` | TIER select(page-master) | `emit() -> Iterable[ArtifactWork]` (Pdf/Office/Egress per document; editorial package aggregate node) | in <- `document/model`,`typography/{shape,layout}`(V3 seams REAL),`color/managed`(ICC),`style`,`exchange/detect`; out -> `document/{report,egress,tagged}`,`exchange/conformance`,`composition/sheet`(notes)
- `document/report.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Pdf` | in <- `document/model`,`document/emit`(terminal PDF route); out -> (consumers)
- `document/egress.md` | KEEP+improve -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Egress` | in <- `composition/imposition`(saddle fold),`document/emit`; out -> (consumers)
- `document/lens.md` | KEEP+V12 -> `improve` | TIER N/A | `emit() -> ArtifactReceipt` (recovery case) | in <- `exchange/detect`; out -> `data/tabular`(corpus lane)
- `document/tagged.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Egress` | in <- `document/model`; out -> `exchange/conformance`(structural result, DATA)

### LEG 3 — AEC / EGRESS

- `drawing/standard.md` | REBUILD (ezdxf lowering; vocabulary moved to `regime`) -> `rebuild` + `absorb {into: drawing/regime, from: standard vocabulary}` | TIER bind | NONE (ezdxf symbol-table substrate) | in <- `drawing/regime`,`vector/pattern`,`typography/font`(metrics),`color/derive`; out -> `drawing/*`,`export/dxf`(seed seam)
- `drawing/dimension.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Drawing` | in <- `drawing/{standard,symbol}`,`typography/{math,shape}`,`vector/boolean`,`color/derive`; out -> `composition/sheet`,`export/layered`
- `drawing/symbol.md` | REBUILD (V5 mark-geometry OWNER) -> `rebuild` | TIER generate | `emit() -> ArtifactReceipt.Drawing` | in <- `drawing/{regime,standard}`,`vector/{algebra,boolean}`,`typography/shape`,`color/derive`; out -> `drawing/{dimension,annotate,detail}`,`composition/sheet`,`export/layered`
- `drawing/annotate.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Drawing` | in <- `drawing/{standard,symbol}`,`typography/{layout,shape,math}`,`vector/boolean`,`color/derive`; out -> `composition/sheet`,`export/layered`
- `drawing/detail.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Drawing` | in <- `drawing/{symbol,regime}`,`rustworkx`,`color/derive`; out -> `composition/sheet`
- `drawing/schedule.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Schedule` | in <- `data/tabular`(QTO),`drawing/{regime,standard}`,`visualization/table`,`vector/pattern`(swatches),`color/derive`; out -> `composition/sheet`
- `specification/section.md` | KEEP+additive -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Spec` | in <- `document/model`,`specification/classify`,`data/tabular`; out -> (document tree)
- `specification/classify.md` | KEEP+additive -> `improve` | TIER N/A | NONE (substrate) | in <- `drawing/regime`(Discipline); out -> `specification/section`,`delivery/register`
- `delivery/register.md` | REBUILD -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Register` | in <- `composition/sheet`(registered, DATA),`specification/classify`(ClassCode),`drawing/regime`(Discipline),`data/tabular`,`rustworkx`; out -> `visualization/table`,`delivery/transmittal`
- `delivery/transmittal.md` | KEEP+additive -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Transmittal` (aggregate node; parents = member keys) | in <- `composition/imposition`,`package/archive`,`exchange/{conformance,credential}`,`delivery/register`,`package/codec`(ledgered, E7),`rustworkx`; out -> (issue)
- `export/dxf.md` | REBUILD (V11) -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Cad` | in <- `drawing/standard`(seed seam),`vector`(bridge); out -> `composition/sheet`,`geospatial`
- `export/layered.md` | REBUILD (V14) -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Preview/Egress` (E1 proof) | in <- `graphic/layer`(LayerPlan tree),`composition`,`drawing/*`,`marks/encode`,`diagram/draw`; out -> (hand-off)
- `export/indesign.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Office` (E1 proof) | in <- `composition`; out -> (hand-off)
- `exchange/metadata.md` | KEEP (pyexiv2 REMOVED) -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Metadata` | in <- (carriers); out -> `core/receipt`
- `exchange/credential.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Credential` | in <- runtime `identity`; out -> `core/receipt`,`Rasm.Persistence`(key binding)
- `exchange/conformance.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Verdict` | in <- `document`,`document/tagged`,`core/receipt`(ConformanceVerdict down),`typography/font`(embed audit); out -> `core/receipt`
- `media/container.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Media` | in <- `scene`(frames, DATA),`media/filtergraph`; out -> `media/*`
- `media/filtergraph.md` | KEEP -> `improve` | TIER N/A | NONE (substrate; E7 REFUTED — no derive coupling) | in <- (leaf av); out -> `media/*`
- `media/audio.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Media` | in <- `media/{container,filtergraph}`; out -> `media/{analysis,synthesis,timeline}`
- `media/timeline.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Media` (strongest CPM DAG exemplar) | in <- `media/{container,filtergraph,audio}`; out -> (render)
- `media/subtitle.md` | KEEP+shaped_rgba -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Media` | in <- `media/{container,filtergraph}`,`typography/shape`(shaped_rgba — now REAL); out -> (mux/burn)
- `media/analysis.md` | REBUILD (E5 phantoms) -> `rebuild` | TIER N/A | `emit() -> ArtifactReceipt.Media` | in <- `media/{container,audio,filtergraph}`,`raster/{io,measure}`(render_png/montage/frame_similarity REAL),`compute/analysis/signal`; out -> `raster/io`(thumbnail)
- `media/synthesis.md` | KEEP -> `improve` | TIER N/A | `emit() -> ArtifactReceipt.Media` | in <- `media/audio`,`compute/analysis/signal`; out -> (encode)
- `package/bundle.md` | NEW (cycle-break) -> `new` | TIER N/A | NONE (vocabulary + PackWorker port) | in <- runtime `identity`; out -> `package/{codec,archive,delta}`
- `package/codec.md` | KEEP (zlib-ng re-tag) -> `improve` | TIER N/A | NONE (substrate/spine) | in <- `package/bundle`; out -> `package/{archive,delta}`(PackWorker), `delivery/transmittal`(ledgered)
- `package/archive.md` | KEEP+growth -> `improve` | TIER N/A | `emit() -> ArtifactReceipt` (Bundle case) | in <- `package/bundle`; out -> `delivery/transmittal`, scene bundling consumer
- `package/delta.md` | KEEP+growth -> `improve` | TIER N/A | `emit() -> ArtifactReceipt` (Bundle case) | in <- `package/bundle`; out -> (incremental)

PAGE-ROW COUNT: 71 rows (23 leg 1 [21 leg-1a owners + 2 core leg-1b] + 21 leg 2 + 27 leg 3), of which NEW=13 (`vector/{algebra,boolean,emit,pattern}`, `color/measure`, `marks/vocabulary`, `style`, `layer`, `typography/math`, `drawing/regime`, `diagram/schematic`, `diagram/solar`, `package/bundle`), DELETE=3 (`vector.md` absorbed into `vector/*`, `core/format` -> `document/emit`, `chart/transform` -> `chart/export`).

---

## [C] ArtifactWork ENTRY CONTRACT (as law) + the constructing owners

The six live carrier conventions collapse to ONE producer contract. `core/plan.md` owns the node and the constructor; every producer emits nodes; the two flagship constructors build the pipeline.

```python
# --- core/plan.md — the live node (never re-spelled thinner) ------------------------------
type Work[T] = Callable[[], Awaitable[RuntimeRail[T]]]                 # unchanged runtime lane thunk

@tagged_union(frozen=True)                                             # admission mode carried, not a knob
class Admission:
    tag: Literal["keyed", "bare", "retried"] = tag()
    keyed: None = case()                                              # (ContentKey, Work) -> lane elision (gate #3 wiring)
    bare: None = case()                                              # weave-only, no elision key
    retried: RetryClass = case()                                     # worker-death band via offload(retry=)

class ArtifactWork(Struct, frozen=True, gc=False):
    key: ContentKey                                                  # producer fills ALWAYS (receipt carries its own key)
    work: Work[ArtifactReceipt]                                      # producer fills ALWAYS
    parents: tuple[ContentKey, ...] = ()                             # composite -> member keys; else ()
    admission: Admission = Admission(keyed=None)                     # by production shape
    cost: float = 1.0                                               # min-slack CPM front order

# --- the ONE producer contract every producer page satisfies -----------------------------
def emit(self, /) -> ArtifactWork | Iterable[ArtifactWork]: ...       # ArtifactPipeline.of normalizes lone|iterable
```

- ONE producer contract: every producer page (`[B]` `emit()` rows) exposes `emit() -> ArtifactWork | Iterable[ArtifactWork]`; the six carriers (bare key+weave; in-band receipt; key+receipt tuple; key+Evidence/Verdict tuple; Result-wrapped media pair; Block-wrapped batch) collapse to this — `export/layered`/`export/indesign` are the LANDED convergence proof (`emit() -> RuntimeRail[ArtifactReceipt]`, no module batch entry). The parallel batch rails DIE: `document/emit#produced`, `document/report#rendered`, `core/format#_fanned`, and every per-instance `of()` batch.
- Multi-artifact ARITY is law: a sheet SET or diagram SUITE emits ONE node per member with per-member keys (re-issue re-renders only changed members); a composite (`Register`/`Transmittal`/the issued set) is ONE aggregate node whose `parents` are its member keys and whose receipt case carries the aggregate facts. Receipts are TERMINAL — incremental production is node granularity, never a partial receipt.
- `dxf`'s `of()+contribute()` converges to the offloaded `emit()` its export siblings landed (the `_composed` single-fold is already on disk; the non-converged entry shape is what the rewire closes).
- CONSTRUCTING OWNERS (the fence that builds `ArtifactPipeline.of(<producer emit() nodes>)` — claimed-but-unconstructed dies):
  - `composition/sheet.md` — `SheetSet.issue()` is FLAGSHIP construction #1: folds each sheet's `emit()` node + the aggregate SheetSet node into `ArtifactPipeline.of(...)`.
  - `document/emit.md` — the editorial-document-PACKAGE is FLAGSHIP construction #2: folds each component's `emit()` node into `ArtifactPipeline.of(...)`.
  - `delivery/transmittal.md` — the composite ISSUE constructor over `sheet`'s per-member nodes (the standing `Transmittal` aggregate proof).
- ONE receipt family: `ArtifactReceipt` stays the only rail; `derive`'s `ColorReceipt`/`ColorReceiptWire` collapse in; the stringly `ArtifactKind` Literal + hand-synced `_KEYS` becomes ONE derived owner (`ArtifactKind` derived from the case roster). Leg 1 fixes the roster; later legs consume, never re-open.

---

## [D] CORRECTED SEAM LEDGER (acyclic; every edge within-wave or earlier; 4+1 inversions + 3 cycles disposed)

The full page-level import graph is acyclic and topologically ordered by leg (1 < 2 < 3). A forward edge (earlier-leg page importing a later-leg page) is a partition defect; every one below is disposed. Cross-plane FIGURE flow is a DATA edge (parent ContentKey on the work graph), never an import — the mid-plane note.

### The four NAMED inversions (`[05]`) + the fifth found — DISPOSED

| # | inversion on disk | disposition (this blueprint) |
|---|---|---|
| (a) | `core/receipt.md:33` <- `exchange/conformance.ConformanceVerdict` (wave 1 <- wave 3) | RE-HOME `ConformanceVerdict`/`Verdict` vocabulary INTO `core/receipt.md` (leg 1); `exchange/conformance` (leg 3) imports it DOWN. The verdict SHAPE is a receipt-family concern; conformance PRODUCES it. Clean downward edge, no freeze needed. |
| (b) | `composition/sheet.md:48` <- `drawing/standard` ScaleRatio/SheetId (+ V3 lettering + V13 pens pull more regime rows up) | SPLIT `drawing/standard`: the pure VOCABULARY + BIND rows -> `drawing/regime.md` (NEW, leg 1); the ezdxf lowering stays `drawing/standard.md` (leg 3). `sheet`/`style`/`detail`/`classify`/`register` import `regime` (leg 1). |
| (c) | six-page `export/layered.Layer` upward fan (compose/imposition/sheet/diagram-draw/marks-encode + drawing) | RESOLVE via foundational `graphic/layer.md` `LayerPlan` (NEW, leg 1); every projector imports `layer` DOWN; `export/layered` (leg 3) COMPOSES the tree, never owns it. |
| (d) | `graphic/raster/io.md:49` <- `exchange/detect` Detect/DetectEngine/DetectIdentity/Source (wave 1 <- wave 3) | RE-WAVE `exchange/detect.md` to LEG 1 (ingress substrate, leaf puremagic/python-magic, no upward deps). `io` imports `detect` (leg 1). The rest of `exchange/` stays leg 3. |
| (e) NEW | `scene/export.md` -> `package/archive` (zlib_ng reproducible-ZIP clone; wave 2 -> wave 3) | REMOVE the edge: DELETE scene/export's `zlib_ng.compressobj` clone; scene/export produces scene FILES; the reproducible ZIP is `package/archive`'s (leg 3), composed at a leg-3 bundling consumer (or USDZ via `stage`/usd-core). zlib-ng's LIVE re-entry consumer is `package/codec`'s GZIP band (already met). package stays leg 3. |

### The three CYCLES (`[SEAM_AND_ENTRY_LAW]` census) — DISPOSED

| cycle | disposition |
|---|---|
| `graphic/marks` encode<->decode (`encode:55,60` <-> `decode:33`, RasterFact/Symbology) | NEW `graphic/marks/vocabulary.md` neutral site owns `Symbology`/`DecodeSource`/`MarkOp`/`MarkFault`/`MarkPayload`; `RasterFact` consolidates to `graphic/raster/measure.md` (its semantic owner, ARCH:114). encode+decode import BOTH down — no back-edge. |
| `scene` render<->export (`render:38` <-> `export:33`) | HOIST the plotter across the seam as an OPAQUE capsule (`boundaries.md` capsule owner): `scene/export` takes the plotter as a bare handle, imports NOTHING from `render`; `render` imports `export`'s `SceneTarget`. One direction. |
| `package` codec<->archive<->delta (`codec:37-38` eager private-worker reach <-> `archive:92`/`delta:30` lazy-back) | NEW `package/bundle.md` neutral vocabulary (`Bundle`/`CompressionAlgo`/`CodecProfile`/`BundleManifest` + `PackWorker` Protocol); codec/archive/delta import `bundle` DOWN and satisfy `PackWorker` structurally (wired at the composition root — codec stops eager-importing private workers). No back-edge. |

### Standing cross-domain edges corrected against disk (E7/E10 drift)
- `drawing/annotate.md:49` chart/spec import -> becomes `color/derive` import WITH a ledger edge (E7 unledgered import + E2 color-orphan, both closed by the V4 rehome).
- `delivery/transmittal.md:58` `package/codec` import -> LEDGERED (E7 — the `:572` seam recital + ARCH gain the codec edge).
- `filtergraph.md:14` "derive coupling" -> STRUCK from the ledger (E7 REFUTED — line 14 is `av.filter` routing; zero derive coupling on disk).
- `dimension.md` `VectorOp.Outline` -> the ARCH:122 "landed outline" parenthetical corrects (E7/E10 — the call was deleted as phantom; the seam is `vector/boolean` stroke-to-outline composed by symbol's terminator geometry).
- `model.md:15` "ten-variant" -> "eleven-variant" (E10 stale count).
- ARCH:103-105 derive `[DERIVE]` edges -> become REAL fences (V4 rehome wires the four drawing importers + chart/spec + scene to `color/derive`); ARCH:94's gate-#2 wording "model-asset case" -> "artifacts-origin case" (`[H]`).

---

## [E] LEG PARTITION (topological over `[D]`; each leg self-contained, whole-repo reconcile)

Legs are DEPENDENCY BARRIERS. The `[05]` three-wave law (FOUNDATIONS -> MID -> AEC/EGRESS) is the shape; the leg-1 1a/1b split is RULED (below). Each leg's in-run reconcile closes every confirmed residual with whole-repository write authority before the leg returns — NO post-leg residual steps, NO hard_residual channel, NO between-legs cleanup. Per-leg engine invocation targets are repo-relative `libs/python/artifacts/.planning/...`.

### LEG-1 1a/1b SPLIT — RULED YES

Leg 1 carries the campaign's largest blast radius (vector split + pattern + typography + color + V13 style + V14 layer + regime re-home + detect re-wave + marks cycle-break + the corpus-wide `ArtifactWork`/`Map`/`identity`/retry rewire). The 1a/1b split is ruled BECAUSE the entry-contract rewire touches producers in ALL legs and must land AFTER the owners are stable; 1b's reconcile is bounded to the entry contract alone once 1a's owners are residual-clean.

- **LEG 1a — foundation OWNERS** (no entry rewire; mint/rebuild the owners, land the packages, break the marks cycle, re-home the vocabulary):
  `Workflow(rebuild, {targets: ["libs/python/artifacts/.planning/graphic/vector", "libs/python/artifacts/.planning/graphic/color", "libs/python/artifacts/.planning/graphic/raster", "libs/python/artifacts/.planning/graphic/marks", "libs/python/artifacts/.planning/graphic/style.md", "libs/python/artifacts/.planning/graphic/layer.md", "libs/python/artifacts/.planning/typography", "libs/python/artifacts/.planning/drawing/regime.md", "libs/python/artifacts/.planning/exchange/detect.md"], brief: "RASM-PY-ARTIFACTS-DECISION.md"})`
  Reconcile closes: `pvlib` admission is NOT here (its owner `diagram/solar` is leg 2 — but the roster row + `.api/pvlib.md` land WITH leg 2, per `[F]`); the shared-tier weave threads every owner; the marks/vector/color/typography `.api` catalogs refresh; `README.md`/`ARCHITECTURE.md` domain-map + seam-ledger rows for the new owners close.
- **LEG 1b — SPINE + corpus-wide entry rewire** (gated on 1a landing residual-clean):
  `Workflow(rebuild, {targets: ["libs/python/artifacts/.planning/core/plan.md", "libs/python/artifacts/.planning/core/receipt.md"], brief: "RASM-PY-ARTIFACTS-DECISION.md"})`
  Reconcile (whole-repo write authority) closes CORPUS-WIDE: `emit() -> ArtifactWork | Iterable[ArtifactWork]` on EVERY producer (all legs); the `frozendict`->`Map` dispatch rebind + msgspec band exemption (`[I]`); `rasm.runtime.content_identity` -> `rasm.runtime.identity` (all 69 spellings / 44 pages); `[RESEARCH]` purge (all 54 pages); the folder-minted `CapacityLimiter`/`stamina.AsyncRetryingCaller` fold onto `lanes.offload(retry=RetryClass.OCCT)` (the ~38-site census, NOT the 3-4 the register names); gate #1 (`ArtifactReceipt.contribute` metrics) + gate #2 (artifacts-origin `graduates()` producer). `core/format.md` stays condemned-but-intact (its absorb executes in leg 2). Leg-2/3 rebuilds INHERIT these and cold-verify, never rewire.

### LEG 2 — MID PLANE
`Workflow(rebuild, {targets: ["libs/python/artifacts/.planning/visualization", "libs/python/artifacts/.planning/scene", "libs/python/artifacts/.planning/composition", "libs/python/artifacts/.planning/document"], brief: "RASM-PY-ARTIFACTS-DECISION.md"})`
Executes: the `core/format` -> `document/emit` cross-wave absorb (the ABSORBER's leg); `chart/transform` -> `chart/export` absorb; V15 machinery (`diagram/solar` NEW + `pvlib` admission + `.api/pvlib.md`; `diagram/layout` plan-geometry/area law; `diagram/schematic` NEW + schemdraw catalog); V9 scene drawing-bridge (silhouette/feature-edge/GL2PS catalog authorship in `.api/vtk.md`+`.api/pyvista.md`); V7 press-fold ownership; V3 typography seams composed in `emit`/`sheet`/`draw`; the `visualization/table` pandas floor (the E5 fix leg-3 pages verify). Reconcile closes the mid-plane `.api` + governance rows; placed figures arrive as parent ContentKeys (DATA edges), never imports.

### LEG 3 — AEC / EGRESS
`Workflow(rebuild, {targets: ["libs/python/artifacts/.planning/drawing", "libs/python/artifacts/.planning/specification", "libs/python/artifacts/.planning/delivery", "libs/python/artifacts/.planning/export", "libs/python/artifacts/.planning/exchange", "libs/python/artifacts/.planning/media", "libs/python/artifacts/.planning/package"], brief: "RASM-PY-ARTIFACTS-DECISION.md"})`
Executes: V2+V3+V5 composed in `drawing/*` (pattern/mark-geometry/typography now real to compose); V11 dxf depth; V14 `LayerPlan` tree across `export/layered`; the specification/delivery additive deltas + rustworkx DAG; `exchange/metadata` `pyexiv2` removal; `package` cycle-break (`bundle` NEW) + zlib-ng re-tag + the two carried growth realizations. `media`/`exchange` are cold-VERIFY (their `ArtifactWork` entry + retry fold landed in leg-1b's reconcile). Acceptance closes on the `[05]` flagship visual review (issued sheet-set slice, one portfolio diagram per AEC kind, an editorial spec-book section).

---

## [F] PACKAGE ROSTER DELTA (integration-first; every census row ruled on its condition; ADDITIVE where a gap demands)

Centralize in root `pyproject.toml` with `.api` stubs per admission. Closure is INTEGRATION at the owning surface (`[A]`), NEVER removal — the ONLY sanctioned removals are the `[PYPROJECT_RECONCILIATION]` rows, each ruled by its brief-stated survival condition on evidence.

### ADMISSION (additive — a real gap no admitted package serves; feed-verified)
| package | root row | owner | `.api` obligation | ruling |
|---|---|---|---|---|
| `pvlib` | `pyproject.toml` `[ARTIFACTS]` — BSD-3, NREL, pure-Python wheel (interpreter-agnostic, ZERO census exposure) | `visualization/diagram/solar.md` (NEW, V15) | AUTHOR `libs/python/artifacts/.api/pvlib.md` with the verified `solarposition` member set (`get_solarposition`/`sun_rise_set_transit_spa`/`nrel_earthsun_distance`, refraction-corrected azimuth/altitude, numpy date sampling) | ADMIT. Discharges the recorded V15 solar-ephemeris proof burden (capability a hand-rolled kernel re-derives); the owned closed-form kernel STANDS as the declined-admission fallback; lands WITH leg 2 (its owner's leg). |

### CENSUS RULINGS (the eight dead-marker rows + the four removal defaults)
| row | class | condition | ruling |
|---|---|---|---|
| `scikit-image` | wheel-lag (`python_version<'3.15'`) | cp315 wheel lands (disk comment "Cython codegen fails on 3.15" is more pessimistic than benign wheel-lag; verdict holds either way) | KEEP pinned; `raster/process`+`measure` capability claims carry the marker-drop blocker BY NAME (V8). Marker drops when the wheel lands. |
| `pyvista` | pure-Python wheel, gated only THROUGH `vtk` | `vtk` cp315 wheel lands | KEEP; `scene` V9 build carries the blocker by name (V9). |
| `vtk` | wheel-lag | cp315 wheel lands | KEEP pinned; V9. |
| `usd-core` | wheel-lag | cp315 wheel lands | KEEP pinned; `scene/stage` (V9). |
| `lets-plot` | wheel-lag | cp315 wheel lands | KEEP pinned; `chart/export` second-engine charter NARROWS to a consumed-capability row OR gains its theme/flavor/element seam — a parity claim without a consumed row dies (V10); blocker named. |
| `PhotoshopAPI` | structurally-dead (NO sdist, wheels stop cp314) | Forge source-build follow-up | KEEP pinned; `export/layered`'s native-PSD writer rides `psd-tools` (the STANDING author) until the Forge build lands; the 12 native blend modes land on psd-tools first (V14). |
| `PyICU` | structurally-dead (NEVER shipped a wheel; sdist + native ICU + compiler) | Forge native-build lane OR removal | KEEP pinned as a GATED upgrade; `uniseg`+`python-bidi` are the STANDING charter in `typography/{shape,layout}` while the gate holds (V3); an integration target, never a removal. |
| `pyexiv2` | removal-default (GPL-3.0 IN-PROCESS copyleft; `python_version<'3.15'`) | survives ONLY re-sited behind the process boundary WITH consumed capability pyexiftool cannot reach | REMOVE. Condition UNMET on disk (api-census: `metadata.md:46` in-process arm's capability is a SUBSET of the standing `pyexiftool` arm; no unique capability exists) AND the in-process form violates the copyleft posture. `pyexiftool` (out-of-process) owns the carrier. Drop the root row + `.api/pyexiv2.md`. |
| `iptcinfo3` | removal-default (SUPERSEDED, zero live consumers) | survives ONLY when the page-set lands a live consumer (a sidecar-XMP owner) | REMOVE — no sidecar-XMP owner is minted; `metadata.md` imports neither; the one-pass `pyexiftool`/`pyexiv2` fold superseded it (`.api/iptcinfo3.md:75`). Drop the root row. |
| `python-xmp-toolkit` | removal-default (SUPERSEDED, zero live consumers) | same sidecar-XMP condition | REMOVE — `pikepdf` owns the PDF/XMP path; no sidecar owner minted. Drop the root row. |
| `grandalf` | on-parity removal (GPL-2.0/EPL-1.0 dual copyleft in-process; no release since 2023) | `fast-sugiyama` parity lands AND the `_grandalf_router` SPLINES route re-homes | KEEP for now — a LIVE consumer (parity oracle + the SOLE spline router `layout.md:_grandalf_router`); removal has a real routing ripple, NOT a zero-consumer strike. `diagram/layout` (leg 2) proves fast-sugiyama parity + re-homes SPLINES; the removal lands only when BOTH conditions hold. |

### RE-ENTRY (data-campaign strike -> artifacts home)
| package | ruling |
|---|---|
| `zlib-ng` | RE-TAG `pyproject.toml:40` `[DATA]` -> `[ARTIFACTS]`; the LIVE consuming fence is `package/codec.md`'s GZIP band (`gzip_ng`/`gzip_ng_threaded`/`crc32`/`crc32_combine`) beside the `lz4`/`brotli`/`zstandard` band — the re-entry condition is ALREADY MET on disk (api-census). AUTHOR the folder `.api/zlib-ng.md` overlay (shared-tier only today). `scene/export`'s hand-reach clone DELETES (`[D]` inversion e). |

### PDF-ARM RATIONALIZATION (carried)
`pdf_oxide` (MIT-OR-Apache, abi3-cp315-forward) is the commercial-safe layout-aware default; `pdfplumber`/`pypdf`/`pypdfium2`/`pymupdf` split PER CONCERN (lens extract / assembly / render / repair+forms). `puremagic`-default / `python-magic`-fallback confirmed. No PDF-arm removal — the split is capability, each concern a live consumer.

### `.api` OBLIGATIONS authored WITH the verdict (absence-claims the register names)
- `.api/pikepdf.md` — ADD the Separation/DeviceN plate-AUTHORING write surface (only the read-side `is_separation`/`is_device_n` at `:124` today) + `add_field` form authoring (`:30,135`); `mediabox`/`trimbox`/`bleedbox` page-box attrs already at `:111,149`.
- `.api/pymupdf.md` — ADD `trimbox`/`bleedbox` (only `mediabox` at `:227` today), `add_widget` form authoring.
- `.api/weasyprint.md` — ADD `@page` margin-box running content, `string-set` section-aware heads, `target-counter` page references (absent today).
- `.api/vtk.md` + `.api/pyvista.md` — ADD `vtkPolyDataSilhouette`/`vtkFeatureEdges`/`vtkGL2PSExporter` + the `view_xy`/`view_isometric` view-preset family (absent from BOTH tiers today).
- `.api/pvlib.md` — NEW (above).
- `.api/zlib-ng.md` (folder overlay) — NEW (re-entry).
- License census carried: `grandalf` GPLv2|EPLv1 SUPERSEDED-PENDING-REMOVAL; `fast-sugiyama` MIT abi3-cp315; `pdf_oxide` MIT-OR-Apache abi3-cp315 — recorded in the owning README/`.api`, not in design prose.

---

## [I] V16 TRACK-REBIND RULINGS (the three corpus-wide laws; band-vs-table ruled explicitly)

The four upstream campaigns land three laws this folder inherits at the track's largest blast radius; leg-1b's reconcile executes them per touched page, never a separate pass.

- **(a) identity rename**: `rasm.runtime.content_identity` -> `rasm.runtime.identity` at ALL 69 `content_identity` spellings (45 full import paths) across 44 pages (runtime `[V4]`). Acceptance: `rg 'rasm.runtime.content_identity'` under `.planning/` returns zero.
- **(b) dispatch-rail rebind — RE-GROUNDED + band-vs-table RULED**: the api-census REFUTES the register premise "`from builtins import frozendict` is not a CPython builtin / every import raises" — PEP 814 (Final, 3.15) makes `frozendict` a builtin (`from builtins import frozendict` SUCCEEDS; MRO `(frozendict, object)`; immutable; hashable). So the rebind stands ONLY on `[04]` SHARED-TIER-LAW consistency grounds (the `expression.Map` ADT/dispatch spine the runtime `[V8]`/data `[V2]`/compute forced on every sibling), NOT as a bug fix. THE BAND-VS-TABLE SPLIT (the subtlety `receipt`/`managed`/`measure`/`io` carry — `frozendict` used as msgspec `case()` PAYLOAD bands, `frozendict[str, float | str]`, not just `Final` dispatch tables; `expression.Map` is NOT msgspec-native):
  - DISPATCH / POLICY tables (`Final[frozendict[...]]`, the `_FLATTEN`/`_PROJECT`/`_METRIC`/`_HATCH`/`VL_RENDER`/`_KIND_POLICY`/`_ROUTES` family) RE-TYPE `Final[Map[...]]` (`Map.of_seq`) — the SHARED-TIER rail.
  - msgspec `case()` PAYLOAD evidence bands (the `preview`/`media`/`cad`/`scene` `frozendict[str, float|str]` hashable maps on `ArtifactReceipt`, and the same on `managed`/`measure`/`io` receipts) STAY the CPython-3.15 builtin `frozendict` — msgspec-native, hashable, wire-encodable; `Map` cannot ride a msgspec struct field.
  - deleted-form prose names the anti-pattern by the `Map`-vs-`frozendict`-TABLE contrast (never "broken import"). Acceptance NARROWS: `rg 'Final\[frozendict'` in DISPATCH-table position returns zero (migrated to `Map`); the msgspec payload bands retain `frozendict` explicitly; the redundant `from builtins import frozendict` import drops where only a table used it, survives where a payload band uses the builtin.
- **(c) `[RESEARCH]` purge**: BANNED folder-wide — 54 pages carry tails. Load-bearing member confirmations fold into the owning `Packages` block or the `.api` catalog; version/freshness narration DELETES; unsettled items convert to explicit in-body GATED obligations. Acceptance: `rg '\[RESEARCH\]'` under `.planning/` returns zero headers.

---

## [H] GATED-OBLIGATION RULINGS ([06] — each re-ruled explicitly; realize or keep gated with the blocker named)

| gate | upstream blocker | ruling |
|---|---|---|
| #1 measured-signals contribution (production duration, byte-volume, compression-ratio facts entering the ONE runtime metric stream) | runtime `[V5]` table-keyed domain-histogram recorder — LANDED (track 1 before track 5) | **REALIZE.** `core/receipt.md`'s `contribute` fold threads the facts through `ArtifactReceipt.contribute` at contribution time into the runtime recorder (the `INSTRUMENTS`-derived domain-histogram) — never a local logging fief. artifacts is the recorder's NAMED demanding consumer. Lands leg 1b. |
| #2 outward figure hand-off (figure/table/chart/scene outputs crossing to sibling packages as the compute `graduation` `HandoffAxis` case keyed by `ContentIdentity`) | compute `[V1]/[V2]` graduation hub — LANDED (track 4 before track 5) | **REALIZE via a NEW artifacts-origin `HandoffAxis` case + its self-wired `graduates()` producer, NOT `model_asset`.** Compute `[V2]` is decisive: `model_asset` is a distinct compute-OWN case; artifacts figures cannot ride it; the artifacts-origin case ships WITH this folder's producer per the compute `[V2]` extension law (geometry-ripple-at-axis-scale). `core/receipt.md` hosts the producer: it imports the compute hub DOWN and calls `graduates(owner, HandoffAxis(artifacts=subject), key, measured, ceiling)`, projecting any `ArtifactReceipt` into the case; the DEFAULT ceiling is a governed policy row on artifacts' own policy carrier (compute `[V2]` ceiling law), the caller's tighter row the override. CORRECT the `[06]`/`ARCHITECTURE.md:94` wording "model-asset case" -> "artifacts-origin case". Lands leg 1b (the producer) + leg 2 (the wiring at figure/table/chart/scene producers). |
| #3 content-keyed output elision (each producer threading its `(ContentKey, Work)` into runtime lane admission so signed PDFs/notebooks/charts/tables/bundles/scenes short-circuit on a cache hit) | C# `Rasm.Persistence` reuse fabric — NOT a py upstream | **KEEP GATED, blocker named.** The WIRING lands (leg 1b): `ArtifactWork.admission = Admission(keyed=None)` threads the `(ContentKey, Work)` pair into lane admission at every producer. The ELISION itself (cache-hit short-circuit) stays gated on the C# Persistence reuse fabric; realize the wiring, keep the elision gated — no silent skip. |

---

## [G] DISPOSITION TABLES (every verdict, evidence row, and capability target ruled)

### V1-V16 verdict dispositions
| V | disposition (page-set consequence) |
|---|---|
| V1 vector folder | `graphic/vector.md` -> `graphic/vector/` folder: `algebra` (svgelements + metric point-at-distance/decimation/centroid/unit egress) / `boolean` (skia-pathops) / `emit` (drawsvg+resvg, f-string SVG dies) / `pattern` (V2). Tolerances -> `Map` policy rows. `vector.md` absorbed. |
| V2 pattern plane | NEW `graphic/vector/pattern.md` — `StrokeFamily`/`PatternSpec`/`DensityLaw`, THREE lowerings (ezdxf `set_pattern_fill`, `drawsvg.Pattern`, pathops clip); `HatchMaterial` seed rows; consumed by `drawing/{standard,schedule}`, `export/{dxf,layered}`. |
| V3 typography completion | NEW `typography/math.md` (ziamath owner); `shape` exports `shaped_rgba` + fallback-chain resolution (`fontTools.unicodedata` itemization) + font-metrics; `font` FREEZE arm; consumers rewire (`drawing/{dimension,annotate,symbol}`, `diagram/draw`, `document/emit`, `composition/sheet`); `SHAPE_QA` (vharfbuzz) parity oracle. |
| V4 color rehoming | `Palette`/`hex_ramp` -> `color/derive`; scene's 3rd alias dies; 4 drawing importers rewire; `ColorReceipt` -> `ArtifactReceipt`; derive/measure split (measurement+CxF3 -> `color/measure`); `AdaptMethod` -> derive; plate-authoring (pikepdf Separation/DeviceN) -> `managed`; TAC policy gate + overprint rows; NO literal hex corpus-wide. |
| V5 drafting-mark geometry | ONE owner in `drawing/symbol.md` — ISO 129-1 terminators + north/scale-bar/grid-bubble/section-marker proportion rows feeding BOTH SVG+DXF, revision triangles/clouds; rotation via vector `Matrix`+`Point.polar_to` (hand-rolled trig dies); dimension/annotate compose it. |
| V6 entry realization | `[C]` — `emit() -> ArtifactWork` corpus-wide; `core/plan` REAL (sheet + emit constructors); parallel batch rails die; `core/format` dissolves into `emit`. |
| V7 press-fold ownership | `composition/imposition` OWNS the Composed/OCG fold; `sheet`+`egress` compose it; `sheet` `_SIZES` from ISO 216 + `paper_rect`; 2D ISO 7200 grid; ISSUE completion (cover/index/general-notes); page-box law. |
| V8 raster+media seams | `raster/io` re-partition (Transform vocab on process/measure); `render_png`/`montage`/`frame_similarity` REAL surfaces; pyvips hardening/streaming; marks cycle break (`marks/vocabulary`); `scikit-image` census blocker named. |
| V9 scene-drawing bridge | `scene/render` BUILDS the bridge (standard views, silhouette/feature-edge, section+poché, GL2PS, directional sun, typed mesh ingress); catalog authored in `.api/vtk.md`+`.api/pyvista.md`; render<->export cycle broken; `detail` callouts + V15 massing consume scene-extracted vectors; `vtk`/`usd-core` census blockers named. |
| V10 visualization repairs | `table` pandas floor + pagination + kiwisolver + units rows; `chart/transform` merges into `export`; typed grammar (Vega dict dies); lets-plot charter narrows/seam (census blocker named); `diagram/draw` 19-shape parity + dead carriers purged; typed ELK vocab; NEW `diagram/schematic` (schemdraw 226-element). |
| V11 dxf depth | `export/dxf` paperspace/viewport + standard->dxf seed seam REAL (kills `_table_entry` dup) + entity completion + `import_blocks` + by-NAME version/unit + `ezdxf.path`/`math` bridge + `emit()` convergence. |
| V12 document plane | `emit` composes typography seams + weasyprint `@page`/`string-set`/`target-counter` + cross-arm TOC + pyphen orthographic application + FORMS owner; `model` folds re-home to emit; `report` routes terminal PDF through emit (dup dies); `lens` OCR pre-flight + PDF/A egress; running heads. |
| V13 design language | NEW `graphic/style.md` — theme rows (color via derive, stroke via regime pens, TYPE SYSTEM, SHEET FAMILY, page-master); every visual plane composes it; zero library-default output; the six AEC `DiagramKind`s reach portfolio grade as theme DATA. |
| V14 layer taxonomy | NEW `graphic/layer.md` — `LayerPlan` semantic tree (ISO 13567); every layered exporter (PSD/OCG/SVG/IDML/ORA) + every projector consumes it; `export/layered` composes not owns; Illustrator lane = layered-PDF + organized-SVG pair; 12 PSD blend modes on psd-tools (PhotoshopAPI census). |
| V15 diagram machinery | NEW `diagram/solar.md` (pvlib SPA + owned kernel fallback + sun-path furniture over `ProjectionKind`); `diagram/layout` plan-geometry ingress (typed coord columns) + area law (STACKING/PROGRAM/SITE area-proportional, CIRCULATION/SECTION_CALLOUT plan-anchored); `glyphset` area/polygon case; massing = V9 scene egress + diagram overlays (DATA edge). |
| V16 track rebind + page grammar | `[I]` — identity rename, `Map` rebind (band-vs-table ruled), `[RESEARCH]` purge; leg-1b reconcile, per touched page. |

### E1-E13 evidence dispositions
| E | disposition |
|---|---|
| E1 entry fiction | `[C]` — the constructing owners (sheet/emit) build `ArtifactPipeline`; six carriers collapse to `emit()`; batch rails die. |
| E2 color orphan | V4 rehome — `derive` gains the 4 drawing + chart/spec + scene importers; the bare `NDArray` alias + 3rd scene alias die; ARCH:103-105 become real fences; prose-vs-fence lie (detail:15 vs :46) closes. |
| E3 dual text engine | V3 — `typography/math` owns ziamath; shape owns shaping+outline+`shaped_rgba`; 7 hand-built sites rewire; standard's fontTools leak -> shape/font metrics; annotate consumes POSITIONS not re-shape. |
| E4 hatch instances | V2 pattern generator — the 11 borrowed ACAD names (ANSI31/ANSI37 double-books) + hand-rolled crosshatch trig + invented swatch hex all die for `PatternSpec` rows. |
| E5 broken paths | `table` pandas floor (leg 2) unblocks schedule's own default `Fmt` path + register's silent Stub (`register` verifies leg 3); `render_png`/`montage`/`frame_similarity` become REAL io/measure surfaces (media/analysis REBUILD). |
| E6 duplication | press fold -> imposition (V7); terminator triad -> symbol V5 owner; two symbol-table writers -> standard.seed (dxf composes); two `Pdf.from_html` -> emit (report routes through); egress imposition algebra -> imposition saddle fold; `ClassificationSystem` -> classify `ClassCode`; `AdaptMethod` -> derive. |
| E7 unwired seams | ARCH:122/137/153/156 composed in fences (V3/V1); annotate:49 ledgered (V4); transmittal:58 codec edge ledgered; filtergraph:14 STRUCK (REFUTED); VectorOp.Outline phantom corrected. |
| E8 hardcoding | tolerances/DPI/pt-tables/Helvetica/DIM/ink/font/ELK-strategy literals -> `Map` policy rows + regime + style + typed ELK vocab; `[04]` SHARED-TIER weave. |
| E9 dead/stringly | `EndCap`/`SubLayout`/`TextRun`/`Port.at`/`corner` consumed or deleted (glyphset); stringly sheet refs -> `SheetId` (detail); `code`/`discipline` -> `ClassCode`/`Discipline` (register); `ArtifactKind` Literal + `_KEYS` -> derived owner; dxf by-VALUE -> by-NAME. |
| E10 ledger drift | ARCH annotations synced to disk; model.md:15 "ten"->"eleven"; package codec/archive/delta cycle unledgered+cyclic -> `package/bundle` neutral site + ledgered. |
| E11 scene seams | render<->export cycle broken (plotter capsule); key+receipt carrier -> `emit()`; stage collapse-premise REFUTED (KEEP, 488-LOC USD owner); silhouette/feature-edge/GL2PS catalog authored. |
| E12 output-grade gaps | V13/V15/V9/V12 — kind-aware diagram dispatch + area law + solar; type scale + sheet families; standard views + sun policy + typed mesh; running heads + cross-arm TOC + live forms + page-box law + overprint. |
| E13 track-law debt | `[I]` — 69 identity / 59 frozendict (band-vs-table) / 54 [RESEARCH] / the ~38-site limiter+stamina census (NOT 3-4) fold onto `lanes.offload`; leg-1b reconcile. |

### [03] capability-escalation targets (each closed by the named page-set delta)
| plane | now->target | closing delta (page-set) |
|---|---|---|
| graphic/vector(+pattern) | 7->9.5 | folder split + `pattern` + metric/decimation/centroid/unit egress + policy-lifted tolerances |
| typography | 6->9.5 | `math` owner + shape outline/`shaped_rgba`/fallback-chain/metrics + font FREEZE + layout leading/tracking to style |
| graphic/color | 6->9 | derive/measure split + one receipt rail + `write_LUT`/spectral-resample + Separation/DeviceN plate + TAC gate |
| graphic/raster | 5->9 | vocabulary re-home + real media seams + pyvips hardening/streaming + pillow procedural |
| visualization/table | 6->9 | pandas floor + pagination + kiwisolver + units + nested/spanning + compose-ingress bridge |
| visualization/diagram | 5.5->9.5 | 19-shape + dead carriers + `schematic` (schemdraw) + typed ELK + V15 machinery + V13 themes |
| design language (V13) | ->9.5 | `graphic/style.md` + binding seam + sheet families + type-system + derive-backed color |
| export/layered+indesign | 8->9.5 | `graphic/layer.md` `LayerPlan` tree + Illustrator lane + native PSD blend + IDML mapping |
| visualization/chart | 7->9 | typed grammar (Vega dict dies) + transform merged + lets-plot seam/narrowed |
| scene | 5.5->9 | standard views + silhouette + section/poché + GL2PS + camera/sun policy + typed mesh ingress |
| core (plan/receipt) | 6->9.5 | entry REAL + derived kind/keys + format dissolved |
| drawing | 6.2->9.5 | V2+V3+V5 composed + ISO 286/1101 dims + welding/surface-texture/datum annotate + regime split |
| composition | 6.5->9.5 | one press fold + ISO 7200 grid + derived sizes + vector affines + ISSUE completion + page-box law |
| document | 7.5->9.5 | typography seams + knob-bag value objects + report-through-emit + weasyprint parity + running heads + TOC + forms + lens OCR/PDFA + orthographic hyphens |
| export/dxf | 7->9.5 | paperspace/viewports + realized standard seam + entity completion + blocks import + `emit()` convergence |
| specification/delivery | 8.5->9.5 | OmniClass/Uniclass rows + Schematron + `ClassCode`/`Discipline` composition + Archive-vs-Bundle + distribution/acknowledgement + rustworkx DAG |

Sound-surface PRESERVE (cold-pass + ONLY the additive deltas a `[03]`/V6 row names): `specification/*`, `document/tagged`, `exchange/*`, `media/*`, `package/*`, `export/indesign`, `typography/font` (the receipt-law exemplar its siblings match).
