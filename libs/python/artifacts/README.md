# [PY_ARTIFACTS]

`artifacts` is one body: a publication and print-production engine carrying a high-end AEC documentation engine on top. Its pub/print plane — color-managed, separations-aware, PDF/X-correct, typographically complete, layered-export-clean, provenance-sealed — composes under the AEC plane — sheet sets, drawing standards, dimensions, annotation, schedules, specifications, ISO 19650 delivery — and every owner grades against both planes at once.

Output bar is art-directed generation: editorial documents and forms at the InDesign-native grade, AEC sheet sets sharp and contemporary over CAD-vendor default, and architectural diagrams — massing, sun-path, circulation, stacking, program, site — at the grade top offices and schools publish. Artistic style is a graded axis of every visual owner, carried as theme data and never left to library defaults; hand-off files organize as a professional builds them — named, meaningfully grouped layers, never thousands of loose elements.

It folds data, compute, and geometry outputs — and any structured payload — into layer-clean files keyed by the runtime content key and carrying one kind-discriminated `ArtifactReceipt`. It owns no UI, no durable store, no IFC/GLB geometry, and no columnar or mesh interchange — those cross at the content-keyed wire, never by reference.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[MODEL](.planning/document/model.md): `DocumentNode` tagged-union tree and its content-keyed diff/merge algebra.
- [02]-[EMIT](.planning/document/emit.md): Emission axis every PDF/Office/text backend lowers from the `DocumentNode` tree.
- [03]-[LENS](.planning/document/lens.md): `DocumentLens` recover-to inverse from emitted container to node tree and the examination ops.
- [04]-[EGRESS](.planning/document/egress.md): `DocumentEgress` encryption, outline, watermark, and redaction finishing over an emitted container.
- [05]-[TAGGED](.planning/document/tagged.md): `Access` PDF/UA marked-content owner authoring and auditing the structure tree.
- [06]-[REPORT](.planning/document/report.md): `ReportPlan` reproducible-report composition into the node tree from sections and notebooks.

[VISUALIZATION]:
- [07]-[CHART_SPEC](.planning/visualization/chart/spec.md): `ChartSpec` chart-authoring union over the host-free 2D engines, palette-threaded.
- [08]-[CHART_EXPORT](.planning/visualization/chart/export.md): `ChartExport` host-free render/format dispatch with the vegafusion pre-pass.
- [09]-[TABLE](.planning/visualization/table.md): `TablePlan` great-tables publication-table owner exporting HTML/LaTeX/PDF.
- [10]-[DIAGRAM_LAYOUT](.planning/visualization/diagram/layout.md): `DiagramLayout` coordinate assignment emitting the diagram-kind vocabulary.
- [11]-[DIAGRAM_DRAW](.planning/visualization/diagram/draw.md): `DiagramDraw` named-layer SVG and editable .drawio emission over one draw target.
- [12]-[DIAGRAM_GLYPHSET](.planning/visualization/diagram/glyphset.md): `DiagramGlyph` bounded diagram-primitive vocabulary both owners compose.
- [13]-[DIAGRAM_SCHEMATIC](.planning/visualization/diagram/schematic.md): `Schematic` named-symbol producer the mark vocabulary cannot express.
- [14]-[DIAGRAM_SOLAR](.planning/visualization/diagram/solar.md): pvlib SPA solar-ephemeris and generated sun-path furniture owner.

[DRAWING]:
- [15]-[DRAWING_REGIME](.planning/drawing/regime.md): Closed drafting vocabulary and BIND substrate every drawing consumer reads; mints no receipt.
- [16]-[DRAWING_STANDARD](.planning/drawing/standard.md): `Standard` ezdxf symbol-table lowering of the regime onto a DXF document.
- [17]-[DRAWING_DIMENSION](.planning/drawing/dimension.md): `Dimension` ISO 129-1 + ISO 1101 GD&T dimensioning producer dual-lowered per target.
- [18]-[DRAWING_SYMBOL](.planning/drawing/symbol.md): `Symbol` AEC drawing-symbol owner dual-lowered to drawsvg groups and ezdxf blocks.
- [19]-[DRAWING_ANNOTATE](.planning/drawing/annotate.md): `Annotate` ISO 128-2 leader, keynote, note, and revision-cloud owner, dual-lowered.
- [20]-[DRAWING_DETAIL](.planning/drawing/detail.md): `Detail` detail-callout owner over a content-keyed block store and the cross-reference DAG.
- [21]-[DRAWING_SCHEDULE](.planning/drawing/schedule.md): `Schedule` AEC-schedule and BIM QTO owner lowering into the publication-table builder.

[SPECIFICATION]:
- [22]-[SECTION](.planning/specification/section.md): `Spec` CSI SectionFormat 3-part producer authored into the `DocumentNode` tree.
- [23]-[CLASSIFY](.planning/specification/classify.md): `ClassCode` MasterFormat/UniFormat/OmniClass owner and drawing-to-spec resolver.

[DELIVERY]:
- [24]-[REGISTER](.planning/delivery/register.md): `Register` ISO 19650 container-register, sheet-index, and container-metadata owner.
- [25]-[TRANSMITTAL](.planning/delivery/transmittal.md): `Transmittal` issue-for-construction orchestrator over imposition, archive, and sign.

[GRAPHIC]:
- [26]-[RASTER_IO](.planning/graphic/raster/io.md): `Raster` host-free pixel IO/convert/working-surface owner over pillow and pyvips.
- [27]-[RASTER_PROCESS](.planning/graphic/raster/process.md): Raster vocabulary owner and produced-raster engine over pillow and scikit-image.
- [28]-[RASTER_MEASURE](.planning/graphic/raster/measure.md): scikit-image measured-score half producing perceptual and feature scalars.
- [29]-[VECTOR_PATH](.planning/graphic/vector/path.md): `Path` svgelements metric substrate — point-at-distance, decimation, one tolerance policy.
- [30]-[VECTOR_REGION](.planning/graphic/vector/region.md): `Region` boolean, offset, and stroke-to-outline owner with metric text-on-path.
- [31]-[VECTOR_PATTERN](.planning/graphic/vector/pattern.md): `PatternSpec` repeating-fill and hatch generator over typed motif-aware lowerings.
- [32]-[MARKS_MARK](.planning/graphic/marks/mark.md): `Symbology` shared machine-readable-mark vocabulary both codec halves import.
- [33]-[MARKS_ENCODE](.planning/graphic/marks/encode.md): `Mark` machine-readable-mark owner composing generation with decode and verify inverses.
- [34]-[MARKS_DECODE](.planning/graphic/marks/decode.md): `DecodeScope.scan` decode substrate the generation arms cannot express, mark-rail-composed.
- [35]-[COLOR_DERIVE](.planning/graphic/color/derive.md): `Colorimetry` upstream color source — CIE/CAM16/spectral, gamut, CVD, harmony, WCAG.
- [36]-[COLOR_MANAGED](.planning/graphic/color/managed.md): `ColorManaged` downstream ICC/LUT/CCTF color-managed raster egress.
- [37]-[STYLE](.planning/graphic/style.md): `Theme` theme-as-data owner carrying type, stroke, palette, ground, and sheet-family rows.
- [38]-[LAYER](.planning/graphic/layer.md): `LayerPlan` semantic layer tree every layered producer projects into and exporter composes.

[TYPOGRAPHY]:
- [39]-[FONT](.planning/typography/font.md): `FontEngineering` font subset, instance, synthesis, and embed-audit owner.
- [40]-[SHAPE](.planning/typography/shape.md): `Shaping` uharfbuzz text-shaping, bidi reorder, and COLRv1 glyph-render owner.
- [41]-[MATH](.planning/typography/math.md): `Formula` one ziamath mathematical-typesetting owner every formula consumer routes through.
- [42]-[LAYOUT](.planning/typography/layout.md): `LineLayout` line-break, hyphenation, and Knuth-Plass paragraph-fit owner.

[COMPOSITION]:
- [43]-[COMPOSE](.planning/composition/compose.md): `Figure` post-render figure and section placement owner emitting flat SVG.
- [44]-[SHEET](.planning/composition/sheet.md): `Sheet` single-sheet title-block/frame owner and the `SheetSet` register-ready set owner.
- [45]-[IMPOSITION](.planning/composition/imposition.md): `Imposition` n-up, booklet, and signature press-imposition owner.

[EXPORT]:
- [46]-[LAYERED](.planning/export/layered.md): `LayeredExport` editable layered-export owner over every layered container target.
- [47]-[INDESIGN](.planning/export/indesign.md): `Idml` SimpleIDML template-mutation hand-off; contributes the Office receipt.
- [48]-[DXF](.planning/export/dxf.md): `Dxf` ezdxf CAD-exchange owner over the DXF-op family and the geospatial bridge.

[EXCHANGE]:
- [49]-[METADATA](.planning/exchange/metadata.md): `MetaCarrier` descriptive EXIF/IPTC/XMP/ICC read/write axis over raster, PDF, and media.
- [50]-[CREDENTIAL](.planning/exchange/credential.md): `Provenance` content-credential sign/read/embed/ingredient-archive owner keyed by content.
- [51]-[CONFORMANCE](.planning/exchange/conformance.md): `Conformance` pyhanko PAdES sign/stamp/augment/audit owner folding one verdict.
- [52]-[DETECT](.planning/exchange/detect.md): `Detect` format-identification gate over puremagic with a python-magic fallback.

[MEDIA]:
- [53]-[CONTAINER](.planning/media/container.md): `Media` container and codec spine from demux through adaptive-streaming egress.
- [54]-[FILTERGRAPH](.planning/media/filtergraph.md): `FilterNode` capability-detected native-vs-substitute filter-routing core.
- [55]-[AUDIO](.planning/media/audio.md): `_encode_audio` av audio-stream encode, resample, and master arm.
- [56]-[TIMELINE](.planning/media/timeline.md): `Timeline` non-linear editing over the container and filtergraph spine.
- [57]-[SUBTITLE](.planning/media/subtitle.md): `Subtitle` pysubs2 parse/convert/retime/restyle, passthrough-mux, and burn-in owner.
- [58]-[ANALYSIS](.planning/media/analysis.md): `Analysis` read-side media measurement and thumbnail owner over the container spine.
- [59]-[SYNTHESIS](.planning/media/synthesis.md): `Synthesis` generated audio and video test-signal producer feeding the encode arms.

[SCENE]:
- [60]-[SPEC](.planning/scene/spec.md): `SceneGrid` parse-floor seam vocabulary and the `WORKER_MODULE` floor anchor.
- [61]-[RENDER](.planning/scene/render.md): `Scene3d` offscreen 3D render producer and rgb24 frame-egress owner on the worker lane.
- [62]-[RENDER_WORKER](.planning/scene/render_worker.md): `_KERNELS` shipped worker kernel bodies the process lane resolves on the worker floor.
- [63]-[EXPORT](.planning/scene/export.md): `ExportRow` correspondence over every scene export target with deterministic capture.
- [64]-[STAGE](.planning/scene/stage.md): `StageOp` usd-core USD/USDZ stage-authoring owner.

[CORE]:
- [65]-[PLAN](.planning/core/plan.md): `ArtifactPipeline` content-keyed sub-graph-elision plan over the runtime session lane.
- [66]-[ISSUE](.planning/core/issue.md): `ArtifactIssue` constructing owner folding producer emit sets into the pipeline and drain.
- [67]-[RECEIPT](.planning/core/receipt.md): `ArtifactReceipt` one receipt union every producer contributes one case to.

[PACKAGE]:
- [68]-[BUNDLE](.planning/package/bundle.md): `Bundle` shared package-plane vocabulary and port floor; mints no receipt.
- [69]-[CODEC](.planning/package/codec.md): `Codec` single-blob ZSTD/LZ4/BROTLI/GZIP compression producer composing the bundle.
- [70]-[ARCHIVE](.planning/package/archive.md): `Archive` multi-file 7z/ZIP archive half and the reproducible-ZIP owner.
- [71]-[DELTA](.planning/package/delta.md): `Delta` detools binary diff/patch arm over parent-keyed delta nodes.

## [02]-[DOMAIN_PACKAGES]

Domain libraries admitted by this folder; versions centralize in the one Python manifest and corroborate against this folder's `.api/`. Native system dependencies stay outside the registry, owned by the provisioning surface that supplies them.

[DOCUMENTS]:
- `reportlab`
- `weasyprint` — HTML-to-PDF with outline tree
- `typst` — PDF/A compile with data binding
- `pymupdf`
- `pypdfium2`
- `pdf-oxide` — Rust PDF extract/render/create/forms
- `pypdf` — assembly and outline/transform egress
- `pikepdf` — repair, encrypt, overlay, and structure-tree authoring
- `python-docx`
- `python-pptx`
- `openpyxl`
- `xlsxwriter` — write-only XLSX with charts and formats
- `python-calamine` — fast read-only XLSX/XLS/ODS ingest
- `odfpy` — OpenDocument read/write
- `docxtpl` — jinja2 DOCX template render
- `msoffcrypto-tool` — encrypted Office decrypt at ingest
- `pdfplumber` — page text/table/word geometry extraction
- `ocrmypdf` — OCR text layer over scanned PDF
- `lxml`
- `ruamel-yaml`
- `tomlkit`
- `jinja2`
- `papermill`
- `nbclient`
- `nbconvert` — notebook export to HTML/PDF/script
- `jupytext` — notebook/text round-trip

[VISUALIZATION]:
- `altair`
- `matplotlib`
- `lets-plot` — second host-free chart engine
- `vl-convert-python` — primary host-free chart export
- `vegafusion` — chart export transform pre-pass
- `great-tables` — publication-table producer
- `polars` — first-class table and frame substrate

[DIAGRAMS]:
- `rustworkx` — graph layout, detail DAG, and plan producer graph
- `grandalf` — second Sugiyama layered-layout engine
- `pyelk` — ELK layered/orthogonal/ports/nesting layout
- `fast-sugiyama` — Rust Sugiyama layered placement
- `kiwisolver` — Cassowary constraint-layout solver
- `ziafont` — glyph text-to-SVG-path outlining
- `ziamath` — math-to-SVG rendering
- `schemdraw` — native-SVG schematic diagrams
- `drawpyo` — draw.io editable export
- `pvlib` — NREL solar-position ephemeris

[IMAGING]:
- `pillow` — raster IO/transform/ICC, annotation, metadata
- `scikit-image` — measured-score and transform arms
- `pyvips` — fused libvips decode/downscale/ICC/smartcrop
- `resvg-py` — SVG-to-raster render
- `tifffile` — TIFF container IO and layered-TIFF writer
- `psdtags` — Photoshop TIFF image resources
- `imagecodecs` — PackBits/ZIP channel codecs

[VECTOR_CAD]:
- `svgelements` — pure-Python SVG geometry and parse
- `skia-pathops` — boolean/offset/stroke-to-outline
- `drawsvg` — hierarchical named-layer SVG authoring
- `ezdxf` — DXF model, render backend, block store, symbol-table lowering

[MARKS]:
- `segno` — QR/Micro-QR
- `python-barcode` — linear 1D symbologies
- `zxing-cpp` — 2D-matrix symbology encode/decode

[COLOR]:
- `colour-science`
- `coloraide` — CSS-space parse/interpolate/gamut-map
- `colour-cxf` — CxF3 spot/spectral color exchange

[TYPOGRAPHY]:
- `fonttools`
- `uharfbuzz` — OpenType shaping and outline bridge
- `blackrenderer` — COLRv1 color-glyph render
- `python-bidi` — UAX#9 bidirectional reorder
- `uniseg` — Unicode line/grapheme/word segmentation
- `pyphen` — language-aware soft-hyphenation
- `opentype-feature-freezer` — freeze OpenType features into the default set
- `vharfbuzz` — HarfBuzz shaping QA and buffer-diff
- `PyICU` — ICU line-break, bidi, and collation power path

[EXCHANGE]:
- `pyhanko` — PAdES PDF signing and conformance
- `c2pa-python` — C2PA content-credential sign/verify
- `puremagic` — pure-Python format sniffer, default detect path
- `python-magic` — libmagic format-ID power path
- `pyexiftool` — cross-format descriptive-metadata read/write

[DELIVERY]:
- `cloudevents` — CloudEvents structured/binary envelope with distributed-tracing extension for the transmittal notice

[EDITABLE_EXPORT]:
- `simpleidml` — IDML package and template mutation
- `PhotoshopAPI` — native PSD/PSB layered writer
- `psd-tools` — PSD read/inspect and pixel author
- `pdfimpose` — saddle/wire/card/cut/fold/signature page-order

[MEDIA]:
- `av` — PyAV container/codec/filtergraph
- `pysubs2` — subtitle parse/convert/retime/restyle

[SCENE]:
- `pyvista`
- `vtk`
- `usd-core` — USD/USDA/USDC scene authoring

[COMPRESSION]:
- `zstandard`
- `lz4`
- `brotli`
- `zlib-ng` — accelerated gzip/zlib behind the GZIP codec
- `py7zr`
- `stream-zip` — streaming ZIP emit
- `stream-unzip` — streaming ZIP ingest
- `detools` — binary diff/patch for delta bundles

## [03]-[SUBSTRATE_PACKAGES]

Cross-cutting substrate consumed from the branch registry; the branch `libs/python/.planning` README and `libs/python/.api/` own the full contracts and API evidence.

[TYPING_RAILS]:
- `expression`
- `msgspec`
- `beartype`
- `pydantic`

[CONCURRENCY]:
- `anyio`

[OBSERVABILITY]:
- `opentelemetry-api`
- `structlog`

[NUMERIC_SUBSTRATE]:
- `numpy`
