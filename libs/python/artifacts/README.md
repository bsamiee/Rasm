# [PY_ARTIFACTS]

`artifacts` is the host-free, self-contained durable-artifact engine: it turns data, compute, and geometry outputs — and any structured payload — into controllable, layer-clean files keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. It reads as eleven domains: a paginated `document` tree (emit/extract inverses, finishing, PDF/UA tagging, reproducible reports), a `visualization` data-to-visual axis (charts, publication tables, AEC diagrams), the `graphic` 2D primitive toolkit (raster, vector, marks, color), `typography` (font binary, shaping, line-layout), `composition` (figure placement, sheet-sets, imposition), editable `export` (named-layer SVG/PDF/TIFF + IDML), the `exchange` boundary (metadata, credentials, conformance, format detection), temporal `media`, the 3D `scene` plane (render/export/USD), the `core` production spine (plan/format/receipt), and the `package` content-addressed compression close. The library is a general science/data surface valid wherever the monorepo needs a file emitted; it owns no UI, no durable store, no IFC/GLB geometry, and no columnar/mesh interchange. This file routes the design pages and registers the external packages.

## [01]-[ROUTER]

The engine reads as eleven domains; each row is one authored design page under `.planning/<domain>/`, grouped in `ARCHITECTURE` `[01]-[DOMAIN_MAP]` order.

[document] — paginated structured documents over the one `DocumentNode` tree:
- [MODEL](.planning/document/model.md): The single interior `DocumentNode` `msgspec` tagged-union tree (page/section/block/run/table/figure/field/annotation/structure-element) plus the `DocumentDelta` diff/merge algebra keyed by content key — the representation emission lowers FROM and extraction recovers TO, making the two inverses over one node algebra.
- [EMIT](.planning/document/emit.md): `DocumentPlan` document-mode emission axis — every `reportlab`/`weasyprint`/`typst` PDF, `pymupdf`/`pypdfium2` render, `pypdf` assembly, `pikepdf` repair, Office, and structured-text backend is a lowering arm folding FROM the `DocumentNode` tree, never a parallel rail; production lowers and `LENS` recovers.
- [LENS](.planning/document/lens.md): `DocumentLens` recover-TO inverse rebuilding a `DocumentNode` tree out of an emitted PDF, scanned raster, or workbook — text/image/word/region/table extract, full-text search, OCR, outline/embedded-file harvest, form-widget recovery, redaction scrub, `Story` re-layout, and ODF ingest over a `_CORE_ARMS`/`_GATED_ARMS` band dispatch into the runtime columnar corpus lane.
- [EGRESS](.planning/document/egress.md): `DocumentEgress` security-and-navigation finishing close over an emitted PDF/Office container — `EgressStep` `Finisher`-row encryption (RC4/AES with a `Permissions` policy), outline authoring, overlay/underlay watermarking, attachment binding, qpdf content-stream rewriting (incl. OCG-layer edit), MuPDF redaction burn-in, and Office decrypt; finishes an artifact, never authors one.
- [TAGGED](.planning/document/tagged.md): `Access` PDF/UA tagged-content owner authoring AND auditing the marked-content structure tree — `TAG` folds the `model#NODE` `StructureNode`/`StructEltKind` family and `FigureNode.alt` into the `pikepdf` `/StructTreeRoot`, `AUDIT` walks it into a typed `StructureAudit`, threading its `conformant` verdict into `exchange/conformance#SIGN`.
- [REPORT](.planning/document/report.md): `ReportPlan` reproducible-report composition over a `COMPOSE_ARMS` async dispatch producing the `DocumentNode` tree — `Section` value-object composition, `jinja2` sandboxed templating, `papermill`/`nbclient` parameterized-notebook execution with `jupytext`/`nbconvert` lowering, and `pymupdf.Story` HTML-to-PDF reflow; binds figures keyed by content key, re-renders nothing.

[visualization] — data -> visual artifact:
- [CHART_SPEC](.planning/visualization/chart/spec.md): `ChartSpec` chart-authoring tagged union over the host-free 2D engines — `altair` Vega-Lite, `lets-plot` grammar-of-graphics, `matplotlib` publication — each case palette-threaded from `graphic/color/derive#DERIVE`, discriminated by one `ChartEngineTag` literal, handing the themed result to `CHART_EXPORT`.
- [CHART_EXPORT](.planning/visualization/chart/export.md): `ChartExport` host-free render/format dispatch folding the chart case, `RenderPolicy`, and target `ExportFormat` to bytes over `vl-convert-python` (Rust V8, zero browser) and in-process `lets-plot`, threading the `CHART_TRANSFORM` server-side data pre-pass into one self-contained spec; the flat-SVG/raster handoff consumed by `composition/compose#COMPOSE`.
- [TABLE](.planning/visualization/table.md): `TablePlan` publication-quality `great-tables` owner (BYO-DataFrame, polars first-class) producing journal tables — spanners, scientific value formatting, uncertainty merges, inline nanoplots, data-driven coloring, `opt_*` theme — exported HTML/LaTeX/PDF, the third artifact pillar beside documents and charts.
- [DIAGRAM_LAYOUT](.planning/visualization/diagram/layout.md): `DiagramLayout` data-driven AEC-diagram coordinate assignment folding a `data/tabular#GRAPH` adjacency frame into positioned `glyphset#GLYPHSET` marks over `rustworkx` `Pos2DMapping`, `grandalf` Sugiyama hierarchical layers, and deterministic AEC projection (sun-path/circulation/stacking/program/site); coordinates only.
- [DIAGRAM_DRAW](.planning/visualization/diagram/draw.md): `DiagramDraw` `drawsvg` named-layer SVG emission folding the positioned `DiagramGlyph` sequence into a `Drawing`, bucketing each mark into its named `Group` by `GlyphStyle.layer` so the diagram emits as named layers `export/layered#LAYERED` binds; palette-indexed, composites nothing.
- [DIAGRAM_GLYPHSET](.planning/visualization/diagram/glyphset.md): `DiagramGlyph` bounded AEC diagram-primitive vocabulary — one closed `tagged_union` over `Node`/`Edge`/`Swimlane`/`Annotation`/`Marker` carrying typed geometry-and-`GlyphStyle` payload keyed by the layout node/edge index — the shared grammar the layout and draw owners compose; emits no SVG, computes no coordinates.

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
- [IMPOSITION](.planning/composition/imposition.md): `Imposition` n-up/booklet/signature press-imposition owner over the closed-payload `ImposeOp` union — the `Scheme` `NUP`/`BOOKLET`/`SIGNATURE` `Plan`-row page-order and `Geometry`/`Imposed` value family over `pymupdf.show_pdf_page` and `pdfimpose`; the dedicated multi-sheet engine distinct from the `egress#IMPOSE` finishing step.

[export] — editable layered hand-off for Illustrator/InDesign:
- [LAYERED](.planning/export/layered.md): `LayeredExport` editable-export owner over SVG named layers, PDF optional-content groups, OpenRaster `ORA` packages, and Photoshop-compatible layered TIFF through `psdtags`/`tifffile`, binding placed sources as named editable layers rather than flattened path soup.
- [INDESIGN](.planning/export/indesign.md): `Idml` SimpleIDML template-mutation hand-off over the closed `IdmlStep` family, mutating one InDesign-exported `.idml` template through its named XML structure and contributing `ArtifactReceipt.Office`.

[exchange] — metadata / provenance / format identification at the boundary:
- [METADATA](.planning/exchange/metadata.md): `Metadata` descriptive-metadata read/write owner over the closed `MetaStandard` axis — cross-format `EXIF`/`IPTC`/`XMP` over `pyexiftool` (the exiftool binary boundary), `XMP` over `pikepdf` `open_metadata()` / `python-xmp-toolkit`, `IPTC` over `iptcinfo3`, optional in-process `pyexiv2` — discriminating `MetaOp` read/write by carrier-and-direction.
- [CREDENTIAL](.planning/exchange/credential.md): `Provenance` C2PA content-credential owner over `c2pa-python` — `Sign`/`Read`/`Verify`/`Ingredient` binding a tamper-evident manifest into the image/BMFF/audio signable set, the `SignerSpec` `CertKey`/`Callback` HSM seam over a `C2paSigningAlg` row, keying the signed buffer into the `csharp:Rasm.Persistence` store; PDF stays the `conformance` rail's.
- [CONFORMANCE](.planning/exchange/conformance.md): `Conformance` PDF cryptographic-conformance close over `pyhanko` — `ConformStep` PAdES B-B/B-T/B-LT/B-LTA signing under `PadesLevel`/`CertifyPerm` with archival-timestamp-chain refresh, the audit folding a `ConformanceVerdict` consuming the `typography/font#FONT` embed-audit and `document/tagged#ACCESS` structural result; contributes `ArtifactReceipt.Verdict`.
- [DETECT](.planning/exchange/detect.md): `Detect` media-type/format-identification gate over `python-magic` libmagic — `DetectOp` `Mime`/`Identity`/`Describe` into a typed `DetectIdentity`, the ingest-boundary format-ID gate the document/raster/Office owners read before per-format dispatch, on the gated subprocess band; contributes no receipt.

[media] — temporal media:
- [AUDIO](.planning/media/audio.md): `_encode_audio` temporal-artifact AUDIO-stream encode arm over `av` — `AudioFrame.from_ndarray`/`AudioFifo` rebuffer to the codec frame size, the `MediaProfile.voiced` stream-configure projection — composing the `media/video#MEDIA` container/profile/evidence family, re-owning no vocabulary; contributes the same `ArtifactReceipt.Media` case.

[scene] — 3D / spatial visualization:
- [EXPORT](.planning/scene/export.md): `SceneTarget` 3D scene-file export owner — the closed `PNG`/`GLTF`/`VRML`/`OBJ`/`HTML`/`USD`/`USDZ` target keyed inside `render#SCENE` `SceneOp.Export`, the `render_export` worker dispatching over the offscreen plotter (`USD`/`USDZ` delegated to `stage`); holds the `geometry/mesh` boundary — scene-file serialization, not raw mesh codec.

[core] — production spine:
- [PLAN](.planning/core/plan.md): `ArtifactPipeline` content-keyed sub-graph-elision plan folding each producer's `ContentIdentity` into `runtime/execution#LANE` `Admit.keyed = (ContentKey, Work)` units over `graphlib.TopologicalSorter`, the third reuse-fabric consumer of the `core/receipt#RECEIPT` `contribute` fold; owns no cache/store/scheduler/drain.
- [FORMAT](.planning/core/format.md): `TemplatePipeline` declarative input-to-output template/format owner binding one `TemplateContext` through the EXISTING sibling binding surfaces — DOCX/PDF/PPTX delegated to `document/emit#DOCUMENT`, HTML to `document/report#REPORT`, and the directly-owned `odfpy` ODF write arm — for one-context-many-format spec-sheet production.
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
- `svgelements` - Pure-Python SVG geometry/transform/parse — `SVG.parse`/`Path`/`Matrix`/`bbox` — owning the `figures/compose` SVG scale-to-fit/n-up/crop/bounds composition over the SVG the chart/QR/nanoplot owners emit.
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
- `uharfbuzz` - OpenType text shaping — the `Blob`/`Face`/`Font`/`Buffer`/`shape`/`GlyphInfo`/`GlyphPosition` pipeline, COLRv1 `PaintFuncs`, `Font.set_variations`, and the `draw_glyph_with_pen` fontTools outline bridge
- `blackrenderer` - COLRv1 color-glyph raster/SVG rendering over the uharfbuzz/fontTools paint chain
- `python-bidi` - UAX#9 bidirectional reorder pass before HarfBuzz shaping for mixed-direction Perso-Arabic runs, feeding `typography/shape`
- `uniseg` - Unicode line, grapheme, and word segmentation for line-break opportunities and paragraph layout.
- `pyphen` - Language-aware soft-hyphenation dictionaries for tight measures and Knuth-Plass penalty rows.
- `opentype-feature-freezer` - freeze OpenType features into the default glyph set for `typography/font`.
- `vharfbuzz` - HarfBuzz shaping QA and buffer-diff beside `uharfbuzz` for `typography/shape`.
- `PyICU` - ICU locale line-break/bidi/collation (gated `python_version<'3.15'`; sdist-only, needs ICU native).

[Composition]:
- `pdfimpose` - Saddle-stitch, wire, cards, cut/fold, and signature page-order computation normalized into `composition/imposition` placement and `ImposedPlan` facts.

[Editable export]:
- `simpleidml` - IDML package mutation and page/layer/template composition for the InDesign export owner.
- `PhotoshopAPI` - native PSD/PSB layered-document writer, the PSD authority for `export/layered` (gated `python_version<'3.15'`; no cp315 wheel/sdist, source-build is a Forge follow-up).
- `psd-tools` - PSD read/inspect plus pixel author beside the `PhotoshopAPI` writer.
- `imagecodecs` - PackBits/ZIP channel codecs for layered raster egress.

[CAD exchange]:
- `ezdxf` - DXF document/entity model, read/write, and render backend for `export/dxf`.

[Diagrams]:
- `grandalf` - Sugiyama hierarchy layout, edge routing, and layered graph coordinates for diagram layout beside `rustworkx` (fallback until `fast-sugiyama` parity lands).
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
- `pyexiftool` - richest cross-format descriptive-metadata read/write over the exiftool binary; the `exchange/metadata` cross-format owner, replaces the abandoned `exif`.
- `pyexiv2` - in-process EXIF/IPTC/XMP read/write (gated `python_version<'3.15'`; wheels-only, no cp315 wheel yet).
- `iptcinfo3` - IPTC raster metadata read/write through `exchange/metadata` carrier facts.
- `python-xmp-toolkit` - XMP namespace/property packet handling behind the metadata owner.

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
- `av` (PyAV in-process container/codec read/write/filtergraph for the 7-page `media/` plane; capability-detection filter routing — native `drawtext`/`subtitles`/`ass`/`eq` when the build exposes them, verified `overlay`/`colorbalance`/`curves`/`nlmeans` substitutes otherwise. Source-build against the machine ffmpeg-full is a Forge follow-up.)
- `pysubs2` - subtitle parse/convert/retime/restyle model for `media/subtitle`.

[Unlocked admitted surfaces]: capabilities the new pages reach through existing package rows:
- `pikepdf` XMP — `Pdf.open_metadata()` scalar read/write, the in-process `exchange/metadata` PDF/XMP path beside the `/OCProperties` object-model surgery the `export/layered` `PdfSurgery` arm uses.
- `pyvips` ICC — the fused-libvips decode/downscale/ICC/smartcrop streaming pipeline the `graphic/raster/io` high-throughput provider reaches through the Forge `libvips` runtime.

[Admitted artifact-local overlays]: these packages have root manifest rows and folder `.api` catalogues; production pages consume them only through their owning planning files.
- `grandalf` — `visualization/diagram/layout` hierarchy coordinate assignment and edge routing beside the admitted `rustworkx` graph substrate.
- `uniseg` + `pyphen` — `typography/layout` Unicode line-break segmentation plus language-aware hyphenation feeding the paragraph-fit owner.
- `simpleidml` — `export/indesign` IDML package mutation over template, layer, spread, page, XML, and imported asset operations.
- `psdtags` + `tifffile` — `export/layered` Photoshop-compatible layered TIFF output and TIFF extratag authoring.
- `pyexiftool` + `pyexiv2` + `iptcinfo3` + `python-xmp-toolkit` — `exchange/metadata` cross-format and raster/PDF descriptive-metadata carrier facts.
- `pdfimpose` — `composition/imposition` saddle/wire/card/cut/fold/signature page-order computation normalized into local placement facts.
- `polars` + `rustworkx` + `zlib-ng` — artifact-local overlays for admitted substrate rows consumed directly by table/diagram/package owners.

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
