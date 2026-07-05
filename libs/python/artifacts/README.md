# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained durable-artifact engine: it turns data, compute, and geometry outputs — and any structured payload — into controllable, layer-clean files keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. It reads as these domains: a paginated `document` tree (emit/extract inverses, finishing, PDF/UA tagging, reproducible reports), a `visualization` data-to-visual axis (charts, publication tables, AEC diagrams), the `drawing` AEC drawing-production plane (owned ISO/NCS drafting vocabularies + drawing symbols + detail cross-references), the `specification` CSI construction-specification plane (SectionFormat 3-part sections authored into the document tree + MasterFormat/UniFormat/OmniClass classification), the `delivery` ISO 19650 delivery plane (drawing register / sheet-index / container-metadata + issue-for-construction transmittal), the `graphic` 2D primitive toolkit (raster, vector, marks, color), `typography` (font binary, shaping, line-layout), `composition` (figure placement, sheet-sets, imposition), editable `export` (named-layer SVG/PDF/TIFF + IDML), the `exchange` boundary (metadata, credentials, conformance, format detection), temporal `media`, the 3D `scene` plane (render/export/USD), the `core` production spine (plan/format/receipt), and the `package` content-addressed compression close. The library is a general science/data surface valid wherever the monorepo needs a file emitted; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [01]-[ROUTER]

Each router row is one authored design page under `.planning/<domain>/`, grouped in `ARCHITECTURE` `[01]-[DOMAIN_MAP]` order.

[document] — paginated structured documents over the one `DocumentNode` tree:
- [MODEL](.planning/document/model.md): The single interior `DocumentNode` `msgspec` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure-element) plus the `DocumentDelta` diff/merge algebra keyed by content key — the representation emission lowers FROM and extraction recovers TO, making the two inverses over one node algebra.
- [EMIT](.planning/document/emit.md): `DocumentPlan` document-mode emission axis — every `reportlab`/`weasyprint`/`typst` PDF, `pymupdf`/`pypdfium2` render, `pypdf` assembly, `pikepdf` repair, Office, and structured-text backend is a lowering arm folding FROM the `DocumentNode` tree, never a parallel rail; production lowers and `LENS` recovers.
- [LENS](.planning/document/lens.md): `DocumentLens` recover-TO inverse rebuilding a `DocumentNode` tree out of an emitted PDF, scanned raster, or workbook — layout-aware text/word/region extract, per-page `STORY` tree recovery, recovered vector `PATHS` geometry, ruled-table grid, full-text search, in-process OCR, outline/embedded-file/annotation harvest, form-widget recovery with field flags, and ODF/`.docx`/YAML/TOML/XML ingest over the one `_ROUTES` `(arm, default_provider)` table — `LensProvider.PDFOXIDE` (MIT/Apache `pdf_oxide`) the commercial-safe layout-aware default, `LensProvider.band` keying the CORE `to_thread` / GATED `to_process` offload into the runtime columnar corpus lane.
- [EGRESS](.planning/document/egress.md): `DocumentEgress` security-and-navigation finishing close over an emitted PDF/Office container — `EgressStep` `Finisher`-row encryption (RC4/AES with a `Permissions` policy), outline authoring, overlay/underlay watermarking, attachment binding, qpdf content-stream rewriting (incl. OCG-layer edit), MuPDF redaction burn-in, and Office decrypt; finishes an artifact, never authors one.
- [TAGGED](.planning/document/tagged.md): `Access` PDF/UA tagged-content owner authoring AND auditing the marked-content structure tree — `TAG` folds the `model#NODE` `StructureNode`/`StructEltKind` family and `FigureNode.alt` into the `pikepdf` `/StructTreeRoot`, `AUDIT` walks it into a typed `StructureAudit`, threading its `conformant` verdict into `exchange/conformance#SIGN`.
- [REPORT](.planning/document/report.md): `ReportPlan` reproducible-report composition over a `COMPOSE_ARMS` async dispatch producing the `DocumentNode` tree — `Section` value-object composition, `jinja2` sandboxed templating, `papermill`/`nbclient` parameterized-notebook execution with `jupytext`/`nbconvert` lowering, and `pymupdf.Story` HTML-to-PDF reflow; binds figures keyed by content key, re-renders nothing.

[visualization] — data -> visual artifact:
- [CHART_SPEC](.planning/visualization/chart/spec.md): `ChartSpec` chart-authoring tagged union over the host-free 2D engines — `altair` Vega-Lite, `lets-plot` grammar-of-graphics, `matplotlib` publication — each case palette-threaded from `graphic/color/derive#DERIVE`, discriminated by one `ChartEngineTag` literal, handing the themed result to `CHART_EXPORT`.
- [CHART_EXPORT](.planning/visualization/chart/export.md): `ChartExport` host-free render/format dispatch folding the chart case, `RenderPolicy`, and target `ExportFormat` to bytes over `vl-convert-python` (Rust V8, zero browser) and in-process `lets-plot`, threading the `CHART_TRANSFORM` server-side data pre-pass into one self-contained spec; the flat-SVG/raster handoff consumed by `composition/compose#COMPOSE`.
- [TABLE](.planning/visualization/table.md): `TablePlan` publication-quality `great-tables` owner (BYO-DataFrame, polars first-class) producing journal tables — spanners, scientific value formatting, uncertainty merges, inline nanoplots, data-driven coloring, `opt_*` theme — exported HTML/LaTeX/PDF, the third artifact pillar beside documents and charts.
- [DIAGRAM_LAYOUT](.planning/visualization/diagram/layout.md): `DiagramLayout` data-driven diagram coordinate assignment folding a `data/tabular#GRAPH` adjacency frame into positioned `glyphset#GLYPHSET` marks over the closed `LayoutPolicy` `Force`/`Radial`/`Layered`/`Projected`/`Constrained` cases across five engines (`rustworkx` `Pos2DMapping` spring/topological, `fast-sugiyama` native-Rust layered placement superseding `grandalf`, `pyelk` ELK orthogonal+ports+nesting, `kiwisolver` Cassowary `Constrained` solve, `grandalf` parity/spline-route fallback), the `Radial` `CIRCULAR`/`SHELL` ring modes, and deterministic projection — emitting all ten `DiagramKind` (the AEC sun-path/circulation/stacking/program/site PLUS the general node-link/flowchart/entity-relation/Sankey/section-callout) threading the `NodeShape`/`Port`/`weight` topology; coordinates only.
- [DIAGRAM_DRAW](.planning/visualization/diagram/draw.md): `DiagramDraw` named-layer diagram emission folding the positioned `DiagramGlyph` sequence into the egress the `DrawTarget` selects — the `drawsvg` named-layer SVG arm (default, bucketing each mark into its named `Group` by `GlyphStyle.layer` for `export/layered#LAYERED`, with `ziafont`/`ziamath` `<path>` label/formula outlining) or the `drawpyo` editable-`.drawio` arm (a standalone diagrams.net file); palette-indexed, composites nothing.
- [DIAGRAM_GLYPHSET](.planning/visualization/diagram/glyphset.md): `DiagramGlyph` bounded data-driven diagram-primitive vocabulary — one closed `tagged_union` over the five marks `Node`/`Edge`/`Swimlane`/`Annotation`/`Marker` carrying typed geometry-topology-and-`GlyphStyle` payload (the `NodeShape`/`Port`/`weight` topology axes and the closed `TextAnchor` vocabulary) keyed by the layout node/edge index — the shared grammar the layout and draw owners compose across the general/technical AND AEC diagram classes; emits no SVG, computes no coordinates.

[drawing] — AEC drawing-production plane, the owned ISO/NCS drafting vocabularies + drawing symbols + detail cross-references + schedules on top of the pub/print substrate:
- [DRAWING_STANDARD](.planning/drawing/standard.md): `Standard` closed AEC-drafting owned-vocabulary substrate — ISO 128 line types/weights + ISO 128-50 hatch materials, ISO 5455 scales, the ISO 13567/AIA/NCS layer-name + NCS `SheetId` sheet-identification codecs, ISO 3098 lettering geometry, and ISO 129-1 dimension-style families — lowered onto the real `ezdxf` symbol tables through `seed`/`graphics`/`dimstyle`/`hatch`/`rgb`/`lettering`/`paper_factor`, the substrate every drawing producer reads; mints no receipt and no plan node, exactly as `glyphset`.
- [DRAWING_DIMENSION](.planning/drawing/dimension.md): `Dimension` ISO 129-1 dimensioning producer over the closed `DimOp` `tagged_union` (`Linear`/`Aligned`/`Angular2L`/`Angular3P`/`AngularCRA`/`Radius`/`Diameter`/`Ordinate`/`Arc3P`/`ArcCRA`/`Chain`/`Baseline`) dual-lowered over `DimTarget` into the `ezdxf` `add_*_dim().render()` native path (DXF/SVG/PDF, ISO tolerance as `dimtol`/`dimlim`/`MTextEditor.stack`) or the `LAYERED` decomposition (`ezdxf.math.Construction*` geometry + self-contained filled/stroked ISO 129-1 terminator marks + `ziafont` ISO 3098 text + `ziamath.Latex` tolerance math into named `export/layered#LAYERED` `Layer` rows, all penned by the discipline sRGB, the tapered-terminator `graphic/vector#VECTOR` `VectorOp.Outline` stroke-to-outline a pending growth axis), the `override=` DIM-variables from `drawing/standard#DIMSTYLE`, and the dimension-line offset stack solved by one `kiwisolver` `Solver`; contributes the shared `ArtifactReceipt.Drawing` case.
- [DRAWING_SYMBOL](.planning/drawing/symbol.md): `Symbol` AEC drawing-symbol owner over the closed `SymbolKind` `tagged_union` (`Section`/`Elevation`/`Detail`/`Grid`/`MatchLine`/`North`/`ScaleBar`/`Revision`/`KeyPlan`/`Datum`/`BreakLine`) dual-lowered over `SymbolTarget` into `drawsvg` named-layer groups (`schemdraw`-rendered, `ziafont`-outlined) or `ezdxf` reusable blocks, composing `schemdraw` `ElementCompound`/`Segment*` compound geometry over one total `_element` fold + one `kiwisolver` grid-bubble solve (endpoint-pinned, `value()` read back) + `drawing/standard#STANDARD`'s owned ISO `LayerName`/`LineWeight`/`TextHeight`/`Terminator`, its `glyph` PNG projection (`graphic/vector#VECTOR` `rasterize`) feeding the `composition/sheet#SHEET` `NorthArrow.glyph`/`KeyPlan.figure` cells; contributes the shared `ArtifactReceipt.Drawing` case.
- [DRAWING_ANNOTATE](.planning/drawing/annotate.md): `Annotate` ISO 128-2 annotation owner over the closed `AnnotateOp` `tagged_union` (`Leader`/`TextNote`/`RevisionCloud`) with nested `LeaderContent` (`Note`/`Keynote`/`Flag`) and `NoteBody` (`Prose`/`Math`) sub-families, dual-lowered over `SymbolTarget` into `drawsvg` named-layer leaders (auto-oriented `Marker` terminator, `ziafont`-outlined content, `ziamath` formula, `Path.A` revision-cloud scallops) or `ezdxf` `add_multileader_mtext`/`add_multileader_block` multileaders (`MTextEditor` content, `add_attdef` keynote block, `add_wipeout` mask, bulged `add_lwpolyline` cloud), composing `drawing/symbol#SYMBOL` `SymbolStyle` + `drawing/standard#STANDARD` `Terminator`/`LineWeight`/`TextHeight`/`LayerName`, `typography/layout#LAYOUT` `LineBrokenRun` Knuth-Plass general notes, `graphic/vector#VECTOR` `skia-pathops` scallop-band offset, and one `kiwisolver` keynote/flag-column solve; contributes the shared `ArtifactReceipt.Drawing` case.
- [DRAWING_DETAIL](.planning/drawing/detail.md): `Detail` detail-management owner over the `Callout` reference value (the `(host, ordinal)` structural discriminant + `target` `DetailRef` + `CalloutBoundary`), the content-keyed `DetailLibrary` `ezdxf`-block store (one detail authored once via `doc.blocks.new` + `add_attdef`, placed N times via `add_auto_blockref`, foreign details through `Importer`/`xref`), and the `rustworkx` `PyDAG(check_cycle=True)` sheet cross-reference DAG (`DAGWouldCycle` mutation-time rejection, `transitive_reduction` minimal graph, `ancestors` revision-impact closure, `node_link_json` content-key wire), projecting the `SymbolKind` bubble the sheet's `Symbol` draws; contributes the shared `ArtifactReceipt.Drawing` case.
- [DRAWING_SCHEDULE](.planning/drawing/schedule.md): `Schedule` AEC-scheduling owner over the closed `ScheduleContent` `tagged_union` (`tabular` a settled `polars.DataFrame` + `ScheduleKind` NCS/AIA template, `legend` a `LegendKind` + authored `LegendEntry` set) lowering into `visualization/table#TABLE` `TablePlan.build` — the `ScheduleKind` vocabulary (`DOOR`/`WINDOW`/`ROOM_FINISH`/`WALL_TYPE`/`PARTITION`/`EQUIPMENT`/`PLUMBING_FIXTURE`/`LIGHTING_FIXTURE`/`FINISH`/`HARDWARE_SET`/`PANEL`/`STRUCTURAL_COLUMN`/`STRUCTURAL_BEAM`/`FURNITURE`/`QUANTITY` the canonical `csharp:Rasm.Bim` QTO takeoff) keyed to a `ScheduleTemplate` whose `_schedule_ops` fold DERIVES the `TableOp` sequence (header/stub/spanners/`FmtKind`/missing-value/`hex_ramp` fire-rating coloring/grand-summary counting the key AND `pl.col().sum()`-totalling each `totals` measure/cost column, filtered to the shaped frame's real columns), and the `LegendKind` legends (`LINE_TYPE`/`HATCH_MATERIAL`/`DISCIPLINE_LAYER` derived from `drawing/standard#STANDARD` into `drawsvg` swatches + authored `ABBREVIATION`/`KEYNOTE`/`MATERIAL_FINISH`/`GENERAL_NOTE` legends); shapes the settled `data/tabular` QTO frame, never authors it; contributes the new `ArtifactReceipt.Schedule` case.

[specification] — CSI construction-specification plane, authored into the document tree on the pub/print substrate:
- [SECTION](.planning/specification/section.md): `Spec` CSI SectionFormat 3-part (General/Products/Execution) producer over the owned article vocabularies (the 15/11/15 published `_ARTICLES` rosters, the four `SpecMethod`s of specifying, the `SubmittalClass` regimes, the `ParagraphRole` content-vs-specifier-note editorial disposition) plus the `PageFormat` `NumberScheme` CSI/UFGS multi-level numbering, lowering the validated section INTO the `document/model#MODEL` `DocumentNode` tree (a `NOTE` paragraph to a decorative `BlockKind.ARTIFACT` the issue strips) and folding the specifier-note / unresolved-fill-in / reference-reconciliation / canonical-order `SpecVerdict`; composes `specification/classify#CLASSIFY` for the `ClassCode` section identity; contributes `ArtifactReceipt.Spec`.
- [CLASSIFY](.planning/specification/classify.md): `ClassCode` cross-system classification value object + `ReferenceIndex` drawing<->spec resolver — the MasterFormat (35 divisions/6 subgroups), UniFormat (A-G+Z groups + elements), and OmniClass (15 tables) owned vocabularies authored to exact published cardinality, the `DERIVED_LOGIC` crosswalk over one primary correspondence plus the OmniClass table-alignment invariant, and the bidirectional `resolve`/`coordinate` binding each spec section to the sheets that detail it with the specced-vs-drawn coverage reconciliation; pure substrate, mints no receipt exactly as `drawing/standard#STANDARD`.

[delivery] — ISO 19650 delivery plane, issuing the drawn and specified set:
- [REGISTER](.planning/delivery/register.md): `Register` ISO 19650 information-container register / drawing-index owner over the closed `RegisterOp` (`Index` great-tables sheet-index / `Container` lxml COBie·BS 1192 XML / `Audit` `RegisterFault.combined` coverage fold / `Render` xlsxwriter spreadsheet + openpyxl round-trip) — the exact NA Table NA.1 suitability (`S0`-`S7`/`A`-`B`/documented-project) + revision (`P`/`C`) + CDE-state owned vocabularies, the `InformationContainer` BS 1192 naming + `composition/sheet#SHEET` `TitleBlock` aggregation, and the one `polars` register `frame` the index and the document `TableNode` both lower from; contributes `ArtifactReceipt.Register`.
- [TRANSMITTAL](.planning/delivery/transmittal.md): `Transmittal` ISO 19650 issue-for-construction orchestrator over the closed `Assemble`/`Seal`/`Issue`/`Manifest` union — composing `composition/imposition#IMPOSE` press-form lay, `package/archive#ARCHIVE` content-addressed container, the CONCURRENT `exchange/conformance#CONFORMANCE` PAdES-LTA + `exchange/credential#CREDENTIAL` C2PA sheet-lineage sign over one `anyio` task-group failure boundary, and `delivery/register#REGISTER` as the issued index + `lxml` transmittal-record XML; contributes `ArtifactReceipt.Transmittal`.

[graphic] — 2D graphic-primitive toolkit every visual + document plane composes:
- [RASTER_PROCESS](.planning/graphic/raster/process.md): The `scikit-image` produced-raster transform half — restoration/exposure/segmentation/thresholding/morphology/geometric-transform/filters folded by the `TRANSFORMS` member-acceptor table — owning the shared `TransformInput`/`TransformArm` substrate the measure half composes, worker worker only.
- [RASTER_MEASURE](.planning/graphic/raster/measure.md): The `scikit-image` measured-score half producing scalars — `_measure` (contours/entropy/HOG/blobs/LBP/corners), `_register` (optical-flow/phase-correlation), and `_metrics` (the six perceptual-quality scalars) contributed as `MEASURE_TRANSFORMS`, stamping the `RasterFact.score` map, composing the `process#PROCESS` substrate.
- [MARKS_DECODE](.planning/graphic/marks/decode.md): `_zxing_decode` machine-readable-mark decode inverse the generation arms cannot express — `zxing-cpp` `read_barcodes` recovering text/format/validity/quad-position from a raster mark, the one `MarkOp` arm crossing the gated subprocess seam (because `read_barcodes` opens its raster through gated Pillow), folding the shared `RasterFact`.
- [COLOR_DERIVE](.planning/graphic/color/derive.md): `Colorimetry` upstream color-derivation owner over `colour-science` (CIE convert/spectral/CAM16/`delta_E`/chromatic-adaptation/CCT) + `ColorAide` (gamut-map/CVD/perceptual-palette/harmony/WCAG contrast), the one color source every visual plane pulls palettes from, folding `ColorReceipt`, feeding the `managed` ICC leg.
- [COLOR_MANAGED](.planning/graphic/color/managed.md): `ColorManaged` ICC/LUT/CCTF color-managed raster-egress owner — the `colour-science` grade chain plus pyvips/libvips `icc_transform` inside the worker seam, contributing `ArtifactReceipt.Preview`.

[typography] — font binary + glyph shaping + line-layout:
- [FONT](.planning/typography/font.md): `FontEngineering` font-binary owner over `fonttools` — `subset.Subsetter` footprint reduction, `varLib.instancer` partial-axis instancing under `AxisLimit`, `fvar`/`STAT` axis introspection into `AxisCatalog`, the `SVGPathPen` outline bridge, and embed-completeness audit; the subset->instance->embed-precondition chain consumed by `document/emit#DOCUMENT` `FONT_EMBED`.
- [SHAPE](.planning/typography/shape.md): `Shaping` text-shaping-and-glyph-render owner — `uharfbuzz` `Face`/`Font`/`Buffer`/`shape` producing `PositionedGlyphRun` on the core, the `python-bidi` UAX#9 reorder pass on the worker lane, and `blackrenderer` COLRv1/COLRv0 `drawGlyph` color-glyph raster; the run consumed by `document/emit`, `composition/compose`, and `layout`.
- [LAYOUT](.planning/typography/layout.md): `LineLayout` paragraph line-layout owner folding the shaped run and a measure into line-broken runs — `uniseg` UAX#14 break positions, `pyphen` soft-hyphenation, and a hand-rolled Knuth-Plass Box/Glue/Penalty total-fit dynamic program.

[composition] — assembling artifacts into pages/sheets:
- [COMPOSE](.planning/composition/compose.md): `Figure` post-render placement owner over the closed-payload `FigureOp` union — scale-to-fit, n-up tile, crop, rotate-place, registration-overlay over the `graphic/vector#VECTOR` `svgelements` primitive on the core, `resvg-py` raster floor, plus `pillow` draw/filter/metadata on the worker lane; emits FLAT-SVG, routes named-layer authoring to `export/layered`.
- [SHEET](.planning/composition/sheet.md): `Sheet` single-sheet title-block/frame/field owner over the closed `SheetOp` union (`Frame`/`Place`/`Fill`/`Stamp`/`Preview`) across `reportlab`/`typst`/`weasyprint` under one `Standard` profile and `pymupdf` `show_pdf_page`/`add_ocg`/`set_ocmd`/`insert_htmlbox` placement — the exact ISO 5457 Table 2 zone grid, the ISO 7200 `TitleBlockAudit`, `Viewport` ISO 5455 scale binding over `svgelements.Matrix`, and the `SheetSet` multi-sheet assembly numbering each `drawing/standard#STANDARD` `SheetId` and projecting `registered()` to `delivery/register#REGISTER` + `scheduled()` to `visualization/table#TABLE`.
- [IMPOSITION](.planning/composition/imposition.md): `Imposition` n-up/booklet/signature press-imposition owner over the closed-payload `ImposeOp` union — the `Scheme` `NUP`/`BOOKLET`/`SIGNATURE` `Plan`-row page-order and `Geometry`/`Imposed` value family over `pymupdf.show_pdf_page` and `pdfimpose`; the dedicated multi-sheet engine distinct from the `egress#IMPOSE` finishing step.

[export] — editable layered hand-off for Illustrator/InDesign + DXF CAD exchange:
- [LAYERED](.planning/export/layered.md): `LayeredExport` editable-export owner over the closed `ExportTarget` — SVG named layers, PDF optional-content groups, OpenRaster `ORA` packages (`pyvips`/`lxml`/`stream-zip`), native Photoshop `PSD`/`PSB` channel-stack documents (`psd-tools` standing author, `PhotoshopAPI` the gated native writer, `imagecodecs` channel codecs), and Photoshop-compatible layered TIFF through `psdtags`/`tifffile`, binding placed sources as named editable layers rather than flattened path soup.
- [INDESIGN](.planning/export/indesign.md): `Idml` SimpleIDML template-mutation hand-off over the closed `IdmlStep` family, mutating one InDesign-exported `.idml` template through its named XML structure and contributing `ArtifactReceipt.Office`.
- [DXF](.planning/export/dxf.md): `Dxf` CAD-exchange editable hand-off owner over the closed-payload `DxfOp` `tagged_union` (`New`/`Read`/`Recover`/`Render`/`Query`/`Bridge`) over `ezdxf` — fresh-document authoring, conforming read, damaged-file `recover` salvage, the seven-backend render lowering into `composition/sheet#SHEET` (`PyMuPdfBackend` PDF) and `graphic/vector#VECTOR` (`SVGBackend` SVG), EQL/spatial query, and the DXF↔SVG↔GeoJSON↔glyph `Bridge` (`make_path`/`flattening`/`from_vertices`/`render_lines`/`addons.geo`/`addons.text2path`) at the vertex/`d`-string wire; contributes `ArtifactReceipt.Cad`.

[exchange] — metadata / provenance / format identification at the boundary:
- [METADATA](.planning/exchange/metadata.md): `Metadata` descriptive-metadata read/write owner — a two-case `read`/`write` `tagged_union` over the `(MetaCarrier, payload)` shape, the closed `MetaCarrier` axis (`RASTER` over the categorical-best cross-format `pyexiftool` standing arm plus the optional cp-gated in-process `pyexiv2` unified arm and `pillow` `ImageCms` ICC detail, `PDF` over `pikepdf` `PdfMetadata`, `MEDIA` over `av` container tags) whose `_CARRIER` row selects the `(reader, writer, lane)` triple, folding EXIF/IPTC/XMP/ICC + GPS + container structure into ONE `MetaFacts` through the `_FIELD_KEYS` logical→standard correspondence; returns the `(ContentKey, MetaFacts, bytes)` triple the consumer projects onto `ArtifactReceipt.Metadata`, with `iptcinfo3`/`python-xmp-toolkit`/`pyvips` superseded on the raster carrier and flagged for the final `pyproject` reconciliation.
- [CREDENTIAL](.planning/exchange/credential.md): `Provenance` C2PA content-credential owner over `c2pa-python` — the closed `tagged_union` `Sign`/`Read`/`ReadFragment`/`Embed` (Verify folded into Read's `validation_state`/`validation_results` evidence, `ReadFragment` the fragmented-BMFF read, `Embed` the `format_embeddable` sidecar→embedded JUMBF rewrap) binding a tamper-evident manifest into the image/BMFF/audio signable set, the `SignerSpec` `cert_key`/`callback` union (with the `CallbackSigner.ed25519` in-process no-HSM digest-signer over `ed25519_sign`) over a `C2paSigningAlg` row, the full 18-member IPTC `DigitalSource` intent vocabulary, and the `resource_to_stream` thumbnail extraction into `CredentialEvidence.resources`; returns the `(ContentKey, CredentialEvidence)` pair the consumer projects onto the four-scalar `ArtifactReceipt.Credential`, keying the signed buffer into the `csharp:Rasm.Persistence` store; PDF/raw-camera stays the `conformance` rail's.
- [CONFORMANCE](.planning/exchange/conformance.md): `Conformance` PDF cryptographic-conformance close over `pyhanko` — the closed `tagged_union` `sign`/`stamp`/`augment`/`reserve`/`audit` (PAdES B-B/B-T/B-LT/B-LTA signing, the signer-free `/DocTimeStamp` proof-of-existence `stamp`, the LTV `augment` archival refresh, the seed-value `reserve` future-signer field-prep with `SigCertConstraints`, and the resilient multi-signature `audit`) under `PadesLevel`/`CertifyPerm`/`Digest`, the `SignerSource` `PemKey`/`Pkcs12Bundle`/`ExternalSig` credential union, the `Appearance` invisible/visible drawing-sheet seal (`TextStampStyle` + scan-to-verify `QRStampStyle`), and the `DssPolicy` write policy, folding a `ConformanceVerdict` that consumes the `typography/font#FONT` embed-audit and `document/tagged#ACCESS` structural result; contributes `ArtifactReceipt.Verdict`.
- [DETECT](.planning/exchange/detect.md): `Detect` dual-engine media-type/format-identification gate — the `DetectEngine` `PUREMAGIC`/`LIBMAGIC`/`LAYERED` axis (the `puremagic` in-process `to_thread` default over a confidence roster + `single_deep_scan` exact OOXML/CFBF subtype, the `python-magic` libmagic `to_process`/`WORKER_BAND` fallback under a `stamina` `BrokenWorkerProcess` retry, and the escalate-`UNKNOWN`-only layered composition) and the `DetectProfile` `MIME`/`DESCRIBE`/`IDENTITY` output vocabulary, folding one `Source` (`Buffer`/`File`, `@beartype(FAULT_CONF)` ingress) into a typed `DetectIdentity` (`MediaClass`/`Container`/`Trust`/`confidence`) admitted through `DetectSettings` env; the ingest-boundary format-ID gate the document/raster/Office owners read before per-format dispatch; contributes no receipt.

[media] — temporal media, the container/codec/filter/timeline/subtitle/analysis/synthesis plane over in-process `av` (PyAV) with capability-detection filter routing:
- [CONTAINER](.planning/media/container.md): `Media` container/codec SPINE and the shared `Media`/`MediaOp`/`MediaProfile`/`MediaEvidence`/`MediaFault`/`ContainerFormat`/`ColorProfile` family over the closed `EncodeVideo`/`EncodeAudio`/`Mux`/`Transcode`/`Remux` `MediaOp` — the mux/demux capsule, the `av.codecs_available`/`bitstream_filters_available` registry probes before minting, real HDR via the integer-`AVCOL`-code `ColorProfile` over `yuv420p10le`, `av.codec.hwaccel.HWAccel` VideoToolbox decode, HLS/DASH/fMP4 segmented `io_open` sinks to a `UPath` root, `VideoFrame.from_dlpack` device ingest, and the `_seek`/`_decode_video`/`_decode_window` read inverses; `Media.encode` IS the `core/plan#PLAN` `ArtifactWork.work` thunk, contributing the shared `ArtifactReceipt.Media` case (HDR tag + segment-count facts). Absorbs the former `video.py`.
- [FILTERGRAPH](.planning/media/filtergraph.md): the capability-detection filter-routing CORE — one closed `FilterNode` family (`Scale`/`Crop`/`Fps`/`Format`/`ColorGrade`/`Denoise`/`TextBurn`/`SubtitleBurn`/`Xfade`/`Concat`/`Amix`) and the `build_graph`/`link_clips`/`cross_dissolve` builders routing each op to its native `av.filter` when `av.filter.filters_available` (the 448-set probe) exposes it AND it wires in-process, else a verified substitute (`drawtext`→Pillow `ImageFont(RAQM)` RGBA numpy-composite, `eq`→`curves`+`hue`, `hqdn3d`→`nlmeans`, `xfade`→numpy cross-dissolve since the native filter fails in-process `configure()`), multi-input pads via `FilterContext.link_to`, explicit `abuffersink`; mints no receipt, its filter-node-count fact riding the composing producer's `Media` band.
- [TIMELINE](.planning/media/timeline.md): `Timeline` non-linear-editing layer over the container/filtergraph spine — the closed `TimelineOp` (`Trim`/`Concat`/`Segment`/`Xfade`) composing `media/container#CONTAINER` `_seek`/`_decode_window`/`_encode_video` and `media/filtergraph#FILTER` `cross_dissolve`/`link_clips`: `Trim` frame-accurate seek+zero-base, `Concat` the two DERIVED strategies (lossless `Packet` re-stamp vs re-encode by clip-param match), `Segment` the container `SEGMENT` segmented sink, `Xfade` the numpy dissolve; each `Clip` a parent `ContentKey` so a multi-clip timeline is the media plane's strongest `core/plan#PLAN` `ArtifactPipeline` DAG; contributes the shared `Media` case (clip/segment counts).
- [AUDIO](.planning/media/audio.md): `_encode_audio` temporal-artifact AUDIO-stream encode arm over `av` — `AudioFrame.from_ndarray`/`AudioResampler` reframe to the codec frame size, the `MediaProfile.voiced` stream-configure projection, the `Master`/`Stage` `loudnorm`/`alimiter` mastering chain, and the `_decode_audio` inverse the `analysis`/`synthesis` pages compose — composing the `media/container#CONTAINER` container/profile/evidence family, re-owning no vocabulary; contributes the same `ArtifactReceipt.Media` case.
- [SUBTITLE](.planning/media/subtitle.md): `Subtitle` timed-text owner over the closed `SubtitleOp` (`Convert`/`Retime`/`Restyle`/`Mux`/`BurnIn`) — `pysubs2` multi-dialect parse/convert/retime/restyle (the eleven `FORMAT_IDENTIFIERS` folded to one `SubtitleDialect`, `parse_tags` override-aware styled fragments, `load_from_whisper` ASR bridge), passthrough soft-subtitle mux into an `av` container, and `typography/shape#SHAPE` RGBA overlay burn-in (the `filters_available`-selected substitute when libass is absent); contributes the shared `ArtifactReceipt.Media` case with event/style counts in the facts band.
- [ANALYSIS](.planning/media/analysis.md): `Analysis` read-side measurement owner over the closed `AnalysisOp` (`Waveform`/`Spectrogram`/`Loudness`/`Silence`/`SceneDetect`/`Thumbnail`) — each capability-routed native-vs-substitute by the `media/filtergraph#FILTER` probe: `showwavespic`/`showspectrumpic` audio→image, `av.filter.loudnorm.stats` two-pass EBU R128, `select='gt(scene,t)'` cut-frame counting, `thumbnail` pick, else numpy envelope/STFT/RMS-threshold and `graphic/raster/measure#MEASURE` SSIM; composes `media/audio#MEDIA` `_decode_audio`; contributes the shared `Media` case with scene-cut/silence/LUFS facts.
- [SYNTHESIS](.planning/media/synthesis.md): `Synthesis` generator owner over the closed `SynthOp` (`Oscillator`/`Noise`/`Additive`/`Fm`/`Am`/`Sweep`/`Impulse`) — numpy-native `np.cumsum` phase-accumulation oscillators (band-limited harmonic sums), `np.random.Generator` white/pink/brown noise, FM/AM, `np.interp` ADSR, and `np.cumsum`-instantaneous-frequency chirp (no `compute` generation owner exists), encoded through `media/audio#MEDIA` `_encode_audio` reusing its `Pcm`/`Master` vocabulary; contributes the shared `Media` case with `fundamental_hz`/`waveform`/`duration` facts.

[scene] — 3D / spatial visualization:
- [EXPORT](.planning/scene/export.md): `SceneTarget` 3D scene-file export owner — the closed `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` target keyed inside `render#SCENE` `SceneOp.Export`, the `render_export` worker dispatching over the offscreen plotter (`USD`/`USDZ` delegated to `stage`); holds the `geometry/mesh` boundary — scene-file serialization, not raw mesh codec.

[core] — production spine:
- [PLAN](.planning/core/plan.md): `ArtifactPipeline` content-keyed sub-graph-elision plan folding each producer's `ContentIdentity` into `runtime/execution#LANE` `Admit.keyed = (ContentKey, Work)` units over one `rustworkx.PyDiGraph` (`topological_generations` fronts, `digraph_find_cycle` gate, `ancestors` target closure) with a Critical-Path-Method `Schedule` driving min-slack front order, the synchronous graph kernel offloaded onto `anyio.to_thread`; the third reuse-fabric consumer of the `core/receipt#RECEIPT` `contribute` fold; owns no cache/store/scheduler/drain.
- [FORMAT](.planning/core/format.md): `TemplatePipeline` declarative one-context-to-many-formats owner binding one `DocumentNode` context through the EXISTING sibling binding surfaces, owning no engine — the DOCX/PDF/PPTX/ODF document, ODS/XLSX spreadsheet-schedule, and XML/YAML/TOML structured-data forms delegated to `document/emit#DOCUMENT` and HTML to `document/report#REPORT` — so one bound QTO/schedule/spec context egresses across the whole publication AND AEC-documentation output set; every receipt is the delegated producer's `@receipted` weave, the pipeline threading only the head `ContentKey`.
- [RECEIPT](.planning/core/receipt.md): The one kind-discriminated `ArtifactReceipt` tagged union shared across every production sub-domain, keyed by the runtime content key and wired through the runtime `ReceiptContributor` port — every producer contributes one case, never a parallel receipt rail.

[package] — content-addressed compression / archive / delta:
- [CODEC](.planning/package/codec.md): `Bundle` content-addressed single-blob compression owner and the `CompressionAlgo` union spine — `zstandard`/`lz4`/`brotli`/`zlib-ng` gzip plus the composed archive/delta cases, the `CodecProfile` per-codec knob-struct union, and the `pack`/`unpack` modal inverse over a `BundleManifest` of `(name, ContentKey, algo, size)` rows.
- [ARCHIVE](.planning/package/archive.md): `Archive` multi-file archive-container half — the `py7zr` `SevenZipFile` 7z and `stream-zip`/`stream-unzip` bounded-memory ZIP arms folding a `*payloads` spread into one container, composing the `codec#CODEC` `Bundle`/`CodecProfile` scaffold, filling the `entries`/`verified` container slots single-blob codecs leave zero.
- [DELTA](.planning/package/delta.md): `Delta` binary diff/patch arm keying an incremental delta bundle by a parent content key — diffing one to-image against a from-image and storing only the compressed `detools` (`bsdiff`/`hdiffpatch`/`suffix_array`) patch, composing the `codec#CODEC` scaffold through the `DeltaKnobs` `CodecProfile` case.

## [02]-[DOMAIN_PACKAGES]

Every domain rendering library this folder uses. Versions are centralized in the one Python manifest; substrate packages live in `[3]-[SUBSTRATE_PACKAGES]` below. Native system dependencies stay outside this registry and are owned by the provisioning surface that supplies them.

[Documents]:
- `reportlab`
- `weasyprint` - `HTML.write_pdf`, `Document.make_bookmark_tree` outline
- `typst` - `PDF_TYPST` mode with PDF/A via `pdf_standards`, the reusable `Compiler` world, `query`/`eval` introspection, `sys_inputs` data binding
- `pymupdf`
- `pypdfium2`
- `pdf-oxide` - Rust PDF extract/render/create/forms (abi3), rationalized against the pdfplumber/pypdf/pypdfium2/pymupdf stack per concern
- `pypdf` - assembly plus `PdfWriter.encrypt`/outline/`Transformation`/`merge_page` egress finishing
- `pikepdf` - repair/linearize plus `Encryption`/`Permissions`/`Outline`/`add_overlay`/`add_underlay`/`as_form_xobject`/`AttachedFileSpec` egress finishing
- `python-docx`
- `python-pptx`
- `openpyxl`
- `xlsxwriter` - Write-only XLSX emit with charts/formats/conditional formatting
- `python-calamine` - Fast read-only XLSX/XLS/ODS ingest into the corpus lane
- `odfpy` - OpenDocument ODT/ODS/ODP read/write
- `docxtpl` - jinja2 DOCX template render over python-docx
- `msoffcrypto-tool` - Encrypted Office document decrypt at the ingest boundary
- `pdfplumber` - Page text/table/word geometry extraction over pdfminer
- `ocrmypdf` - OCR text layer over a scanned PDF on the tesseract/ghostscript Forge native floor
- `lxml`
- `ruamel-yaml`
- `tomlkit`

[Reporting]:
- `jinja2`
- `papermill`
- `nbclient`
- `nbconvert` - Notebook export to HTML/PDF/script over the executed notebook tree
- `jupytext` - Notebook/text round-trip for diffable report sources

[Charts]:
- `altair`
- `matplotlib`
- `lets-plot` - Second host-free chart engine beside the `vl-convert` Vega export path
- `vl-convert-python` - Primary host-free static chart export
- `vegafusion` - Chart `EXPORT` transform pre-pass

[Scene3d]:
- `pyvista`
- `vtk`
- `usd-core` - USD/USDA/USDC scene authoring and stage composition for 3D scene export

[Tables]:
- `great-tables`
- `polars` - First-class table/frame substrate for publication tables and diagram graph inputs

[Imaging]:
- `pillow` - Raster I/O/transform/`ImageCms` ICC profiles, plus `ImageDraw`/`ImageFont`/`ImageOps`/`ImageFilter`/`Image.Exif` figure annotation and metadata
- `scikit-image`
- `segno` - QR/Micro-QR, dependency-free serializers, replaces qrcode
- `python-barcode` - Linear 1D symbologies only — Code128/EAN/UPC/ITF/Code39/Codabar — beside the segno QR arm
- `zxing-cpp` - 2D-matrix symbology owner — DataMatrix/PDF417/Aztec/MaxiCode encode and `read_barcodes` decode round-trip — beside the python-barcode linear arm
- `pyvips` - Fast libvips raster pipeline — resize/thumbnail/format-convert/composite — on the libvips Forge native floor
- `resvg-py` - Resvg SVG-to-raster render with accurate font/filter support
- `svgelements` - Pure-Python SVG geometry/transform/parse owning the `figures/compose` scale-to-fit/n-up/crop/bounds composition over chart/QR/nanoplot SVG.
- `skia-pathops` - Skia boolean/offset/stroke-to-outline path operations for `graphic/vector`, beside the `shapely` planar set-op surface.
- `drawsvg` - Hierarchical named-layer SVG authoring — `Drawing`/`Group(id=...)`/`as_svg` — for the `export/layered` Illustrator/InDesign editable hand-off.
- `tifffile` - TIFF container IO, extratag authoring, ICC/metadata fields, and Photoshop-compatible layered TIFF writer boundary.
- `psdtags` - Photoshop TIFF image resources, layer records, and layer/mask source data encoded through `tifffile` extratags.
- `python-magic`
- `puremagic` - pure-Python format sniffer, the default `exchange/detect` path beside the libmagic power path

[Color]:
- `colour-science`
- `coloraide` - CSS-space color parse/interpolate/gamut-map for web-facing palette egress
- `colour-cxf` - CxF3 spot/spectral color exchange for the print/separations plane (device-link/proof transforms route through Pillow `ImageCms`)

[Typography]:
- `fonttools`
- `pyhanko`
- `uharfbuzz` - OpenType shaping pipeline, COLRv1 `PaintFuncs`, `Font.set_variations`, and the `draw_glyph_with_pen` fontTools outline bridge
- `blackrenderer` - COLRv1 color-glyph raster/SVG rendering over the uharfbuzz/fontTools paint chain
- `python-bidi` - UAX#9 bidirectional reorder pass before HarfBuzz shaping for mixed-direction Perso-Arabic runs, feeding `typography/shape`
- `uniseg` - Unicode line, grapheme, and word segmentation for line-break opportunities and paragraph layout.
- `pyphen` - Language-aware soft-hyphenation dictionaries for tight measures and Knuth-Plass penalty rows.
- `opentype-feature-freezer` - freeze OpenType features into the default glyph set for `typography/font`.
- `vharfbuzz` - HarfBuzz shaping QA and buffer-diff beside `uharfbuzz` for `typography/shape`.
- `PyICU` - ICU locale line-break/bidi/collation (gated `python_version<'3.15'`; sdist-only, needs ICU native).

[Composition]:
- `pdfimpose` - Saddle-stitch, wire, cards, cut/fold, and signature page-order computation normalized into `composition/imposition` `ImposedPlan` facts.

[Editable export]:
- `simpleidml` - IDML package mutation and page/layer/template composition for the InDesign export owner.
- `PhotoshopAPI` - native PSD/PSB layered writer, the PSD authority for `export/layered`; gated `python_version<'3.15'`, source-build a Forge follow-up.
- `psd-tools` - PSD read/inspect plus pixel author beside the `PhotoshopAPI` writer.
- `imagecodecs` - PackBits/ZIP channel codecs for layered raster egress.

[CAD exchange]:
- `ezdxf` - DXF document/entity model and render backend for `export/dxf`, the drawing-plane block store, and the `drawing/standard` symbol-table lowering.

[Diagrams]:
- `grandalf` - Sugiyama hierarchy layout and edge routing beside `rustworkx`, fallback until `fast-sugiyama` parity lands.
- `pyelk` - ELK layered/orthogonal/ports/nesting diagram layout for the node-link/ER/flowchart diagram pages.
- `fast-sugiyama` - fast Sugiyama layered placement (Rust/abi3) replacing `grandalf` placement.
- `kiwisolver` - Cassowary constraint-layout solver promoted to a direct surface.
- `ziafont` - font glyph text-to-SVG-path for label glyph runs.
- `ziamath` - math-to-SVG rendering for annotation math.
- `schemdraw` - native-SVG schematic diagrams.
- `drawpyo` - draw.io / diagrams.net export.

[Provenance]:
- `c2pa-python` - C2PA content-credential manifest sign/verify keyed by the content key, feeding `provenance/credential`

[Metadata]:
- `pyexiftool` - cross-format descriptive-metadata read/write over the exiftool binary; the `exchange/metadata` owner, replaces `exif`.
- `pyexiv2` - in-process EXIF/IPTC/XMP read/write (gated `python_version<'3.15'`; wheels-only, no cp315 wheel yet).
- `iptcinfo3` - superseded on the raster carrier by `pyexiftool`/`pyexiv2`; flagged for final `pyproject` reconciliation.
- `python-xmp-toolkit` - superseded by the `pyexiftool`/`pyexiv2` pass, `pikepdf` owning PDF/XMP; flagged for final `pyproject` reconciliation.

[Compression]:
- `zstandard`
- `lz4`
- `brotli`
- `zlib-ng` - Accelerated gzip/zlib container compression behind the `GZIP` codec row.
- `py7zr`
- `stream-zip` - Streaming ZIP archive emit without buffering the whole archive
- `stream-unzip` - Streaming ZIP archive ingest without buffering the whole archive
- `detools` - Binary-diff/patch generation for incremental artifact delta bundles

[Media]:
- `av` - PyAV container/codec/filtergraph for the `media/` plane; capability-detected native filters, verified substitutes; ffmpeg-full build a Forge follow-up.
- `pysubs2` - subtitle parse/convert/retime/restyle model for `media/subtitle`.

[UNLOCKED_ADMITTED_SURFACES]: capabilities the new pages reach through existing package rows:
- `pikepdf` XMP — `Pdf.open_metadata()` scalar read/write, the in-process PDF/XMP path beside the `/OCProperties` surgery the `PdfSurgery` arm uses.
- `pyvips` ICC — the fused decode/downscale/ICC/smartcrop streaming pipeline reached through the Forge `libvips` runtime.

[ADMITTED_LOCAL_OVERLAYS]: these packages have root manifest rows and folder `.api` catalogues; production pages consume them only through their owning planning files.
- `grandalf` — `visualization/diagram/layout` hierarchy coordinate assignment and edge routing beside the admitted `rustworkx` graph substrate.
- `uniseg` + `pyphen` — `typography/layout` Unicode line-break segmentation plus language-aware hyphenation feeding the paragraph-fit owner.
- `simpleidml` — `export/indesign` IDML package mutation over template, layer, spread, page, XML, and imported asset operations.
- `psdtags` + `tifffile` — `export/layered` Photoshop-compatible layered TIFF output and TIFF extratag authoring.
- `pyexiftool` + `pyexiv2` — `exchange/metadata` raster carrier, `pikepdf` the `PDF` and `av` the `MEDIA` carrier; `iptcinfo3`/`python-xmp-toolkit` superseded.
- `pdfimpose` — `composition/imposition` saddle/wire/card/cut/fold/signature page-order computation normalized into local placement facts.
- `polars` + `rustworkx` + `zlib-ng` — overlays for table/diagram/package owners; `rustworkx` also owns the detail `PyDAG` and `core/plan` producer graph.

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting substrate libraries this folder consumes; canonical registry lives at `libs/python/.planning/README.md` and branch `libs/python/.api/`.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[NUMERIC_SUBSTRATE]:
- `numpy`
