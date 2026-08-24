# [PY_ARTIFACTS]

`artifacts` is a publication and print-production engine carrying the AEC documentation plane on top. It folds data, compute, geometry, and any structured payload into layer-clean files, each returning one kind-discriminated `ArtifactReceipt`.

## [01]-[ROUTER]

[DOCUMENT]:
- [01]-[MODEL](.planning/document/model.md): `DocumentNode` tagged-union tree and its content-keyed diff/merge algebra.
- [02]-[EMIT](.planning/document/emit.md): Emission axis every PDF/Office/text backend lowers from the `DocumentNode` tree.
- [03]-[LENS](.planning/document/lens.md): `DocumentLens` recover-to inverse from emitted container to node tree and the examination ops.
- [04]-[EGRESS](.planning/document/egress.md): `DocumentEgress` encryption, outline, watermark, and redaction finishing over an emitted container.
- [05]-[TAGGED](.planning/document/tagged.md): `Access` PDF/UA structure, PDF/X preflight, and PDF/A archival close over one `AccessOp` union.
- [06]-[REPORT](.planning/document/report.md): `ReportPlan` reproducible-report composition into the node tree from sections and notebooks.

[VISUALIZATION]:
- [07]-[CHART_SPEC](.planning/visualization/chart/spec.md): `ChartSpec` chart-authoring union over the host-free 2D engines, palette-threaded.
- [08]-[CHART_EXPORT](.planning/visualization/chart/export.md): `ChartExport` host-free render/format dispatch with the vegafusion pre-pass.
- [09]-[TABLE](.planning/visualization/table.md): `TablePlan` great-tables publication-table owner exporting HTML/LaTeX/PDF.
- [10]-[DASHBOARD](.planning/visualization/dashboard.md): `DashboardPlan` offline single-file HTML deck over one shared Vega runtime.
- [11]-[DIAGRAM_LAYOUT](.planning/visualization/diagram/layout.md): `DiagramLayout` coordinate assignment emitting the diagram-kind vocabulary.
- [12]-[DIAGRAM_DRAW](.planning/visualization/diagram/draw.md): `DiagramDraw` named-layer SVG and editable .drawio emission over one draw target.
- [13]-[DIAGRAM_GLYPHSET](.planning/visualization/diagram/glyphset.md): `DiagramGlyph` bounded diagram-primitive vocabulary both owners compose.
- [14]-[DIAGRAM_SCHEMATIC](.planning/visualization/diagram/schematic.md): `Schematic` named-symbol producer the mark vocabulary cannot express.
- [15]-[DIAGRAM_SOLAR](.planning/visualization/diagram/solar.md): pvlib SPA solar-ephemeris and generated sun-path furniture owner.

[DRAWING]:
- [16]-[REGIME](.planning/drawing/regime.md): Closed drafting vocabulary and BIND substrate every drawing consumer reads; mints no receipt.
- [17]-[STANDARD](.planning/drawing/standard.md): `Standard` ezdxf symbol-table lowering of the regime onto a DXF document.
- [18]-[DIMENSION](.planning/drawing/dimension.md): `Dimension` ISO 129-1 + ISO 1101 GD&T dimensioning producer dual-lowered per target.
- [19]-[SYMBOL](.planning/drawing/symbol.md): `Symbol` AEC drawing-symbol owner dual-lowered to drawsvg groups and ezdxf blocks.
- [20]-[ANNOTATE](.planning/drawing/annotate.md): `Annotate` ISO 128-2 leader, keynote, note, and revision-cloud owner, dual-lowered.
- [21]-[DETAIL](.planning/drawing/detail.md): `Detail` detail-callout owner over a content-keyed block store and the cross-reference DAG.
- [22]-[SCHEDULE](.planning/drawing/schedule.md): `Schedule` AEC-schedule and BIM QTO owner lowering into the publication-table builder.

[SPECIFICATION]:
- [23]-[SECTION](.planning/specification/section.md): `Spec` CSI SectionFormat 3-part producer authored into the `DocumentNode` tree.
- [24]-[CLASSIFY](.planning/specification/classify.md): `ClassCode` MasterFormat/UniFormat/OmniClass owner and drawing-to-spec resolver.

[DELIVERY]:
- [25]-[REGISTER](.planning/delivery/register.md): `Register` ISO 19650 container-register, sheet-index, and container-metadata owner.
- [26]-[GATE](.planning/delivery/gate.md): `QualityGate` per-kind threshold fold grading every producer verdict a transmittal ships on.
- [27]-[TRANSMITTAL](.planning/delivery/transmittal.md): `Transmittal` ISO 19650 issue-for-construction close folding one `TransmittalEvidence`.
- [28]-[NOTICE](.planning/delivery/notice.md): `TransmittalNotice` projection preserving operation, content, source, and confidentiality semantics.

[GRAPHIC]:
- [29]-[RASTER_IO](.planning/graphic/raster/io.md): `Raster` host-free pixel IO/convert/working-surface owner over pillow and pyvips.
- [30]-[RASTER_PROCESS](.planning/graphic/raster/process.md): Raster vocabulary owner and produced-raster engine over pillow and scikit-image.
- [31]-[RASTER_MEASURE](.planning/graphic/raster/measure.md): scikit-image measured-score half producing perceptual and feature scalars.
- [32]-[TEXTURE_PLANE](.planning/graphic/texture/plane.md): `Plane` deep-pixel substrate and the codec rows lifting the estate 8-bit ceiling.
- [33]-[TEXTURE_DERIVE](.planning/graphic/texture/derive.md): `DeriveOp` channel-derivation kernels over one separable resampler.
- [34]-[TEXTURE_INGEST](.planning/graphic/texture/ingest.md): `TextureRole`/`IblProduct` slot vocabulary and its total loose-file classifier.
- [35]-[TEXTURE_SET](.planning/graphic/texture/set.md): `TextureSet` producer minting the generated `appearance.v1.Set` behind a merkle set key.
- [36]-[TEXTURE_IBL](.planning/graphic/texture/ibl.md): `Ibl` environment-radiance prefilter minting the products an `hdri` manifest names.
- [37]-[VECTOR_PATH](.planning/graphic/vector/path.md): `Path` svgelements metric substrate — point-at-distance, decimation, one tolerance policy.
- [38]-[VECTOR_REGION](.planning/graphic/vector/region.md): `Region` boolean, offset, and stroke-to-outline owner with metric text-on-path.
- [39]-[VECTOR_PATTERN](.planning/graphic/vector/pattern.md): `PatternSpec` repeating-fill and hatch generator over typed motif-aware lowerings.
- [40]-[MARKS_MARK](.planning/graphic/marks/mark.md): `Symbology` shared machine-readable-mark vocabulary both codec halves import.
- [41]-[MARKS_ENCODE](.planning/graphic/marks/encode.md): `Mark` machine-readable-mark owner composing generation with decode and verify inverses.
- [42]-[MARKS_DECODE](.planning/graphic/marks/decode.md): `DecodeScope.scan` decode substrate the generation arms cannot express, mark-rail-composed.
- [43]-[COLOR_DERIVE](.planning/graphic/color/derive.md): `Colorimetry` upstream color source — CIE/CAM16/spectral, gamut, CVD, harmony, WCAG.
- [44]-[COLOR_MANAGED](.planning/graphic/color/managed.md): `ColorManaged` downstream ICC/LUT/CCTF color-managed raster egress.
- [45]-[STYLE](.planning/graphic/style.md): `Theme` theme-as-data owner carrying type, stroke, palette, ground, and sheet-family rows.
- [46]-[LAYER](.planning/graphic/layer.md): `LayerPlan` semantic layer tree every layered producer projects into and exporter composes.

[TYPOGRAPHY]:
- [47]-[FONT](.planning/typography/font.md): `FontEngineering` font subset, instance, synthesis, and embed-audit owner.
- [48]-[SHAPE](.planning/typography/shape.md): `Shaping` uharfbuzz text-shaping, bidi reorder, and COLRv1 glyph-render owner.
- [49]-[MATH](.planning/typography/math.md): `Formula` one ziamath mathematical-typesetting owner every formula consumer routes through.
- [50]-[LAYOUT](.planning/typography/layout.md): `LineLayout` line-break, hyphenation, and Knuth-Plass paragraph-fit owner.

[COMPOSITION]:
- [51]-[COMPOSE](.planning/composition/compose.md): `Figure` post-render figure placement owner emitting flat SVG.
- [52]-[SHEET](.planning/composition/sheet.md): `Sheet` single-sheet title-block/frame owner and the `SheetSet` register-ready set owner.
- [53]-[IMPOSITION](.planning/composition/imposition.md): `Imposition` n-up, booklet, and signature press-imposition owner.

[EXPORT]:
- [54]-[LAYERED](.planning/export/layered.md): `LayeredExport` editable layered-export owner over every layered container target.
- [55]-[INDESIGN](.planning/export/indesign.md): `Idml` SimpleIDML template-mutation hand-off; contributes the Office receipt.
- [56]-[DXF](.planning/export/dxf.md): `Dxf` ezdxf CAD-exchange owner over the DXF-op family and the geospatial bridge.

[EXCHANGE]:
- [57]-[METADATA](.planning/exchange/metadata.md): `MetaCarrier` descriptive EXIF/IPTC/XMP/ICC read/write axis over raster, PDF, and media.
- [58]-[CREDENTIAL](.planning/exchange/credential.md): `Provenance` content-credential sign/read/embed/ingredient-archive owner keyed by content.
- [59]-[CONFORMANCE](.planning/exchange/conformance.md): `Conformance` pyhanko PAdES sign/stamp/augment/reserve/audit owner folding one verdict.
- [60]-[DETECT](.planning/exchange/detect.md): `Detect` format-identification gate over puremagic with a python-magic fallback.

[MEDIA]:
- [61]-[CONTAINER](.planning/media/container.md): `Media` container and codec spine from demux through adaptive-streaming egress.
- [62]-[FILTERGRAPH](.planning/media/filtergraph.md): `FilterNode` capability-detected native-vs-substitute filter-routing core.
- [63]-[AUDIO](.planning/media/audio.md): `Pcm` block audio arm — decode, encode, resample, layout, and mix over the av floor.
- [64]-[TIMELINE](.planning/media/timeline.md): `Timeline` non-linear editing over the container and filtergraph spine.
- [65]-[SUBTITLE](.planning/media/subtitle.md): `Subtitle` pysubs2 parse/convert/retime/restyle, passthrough-mux, and burn-in owner.
- [66]-[ANALYSIS](.planning/media/analysis.md): `Analysis` read-side media measurement and thumbnail owner over the container spine.
- [67]-[SYNTHESIS](.planning/media/synthesis.md): `Synthesis` generated audio and video test-signal producer feeding the encode arms.

[SCENE]:
- [68]-[SPEC](.planning/scene/spec.md): `SceneGrid` parse-floor seam vocabulary and the `WORKER_MODULE` floor anchor.
- [69]-[RENDER](.planning/scene/render.md): `Scene3d` offscreen 3D render producer and rgb24 frame-egress owner on the worker lane.
- [70]-[RENDER_WORKER](.planning/scene/render_worker.md): `_KERNELS` shipped worker kernel bodies the process lane resolves on the worker floor.
- [71]-[EXPORT](.planning/scene/export.md): `ExportRow` correspondence over every scene export target with deterministic capture.
- [72]-[STAGE](.planning/scene/stage.md): `StageOp` usd-core USD/USDZ stage-authoring owner.

[CORE]:
- [73]-[PLAN](.planning/core/plan.md): `ArtifactPipeline` content-keyed sub-graph-elision plan over the runtime session lane.
- [74]-[ISSUE](.planning/core/issue.md): `ArtifactIssue` constructing owner folding producer emit sets into the pipeline and the lane drive.
- [75]-[RECEIPT](.planning/core/receipt.md): `ArtifactReceipt` one receipt union every producer contributes one case to.
- [76]-[HOOKS](.planning/core/hooks.md): `ArtifactsLeg` raise-leg roster and the `ArtifactHook` production-fact point table.
- [77]-[BENCH](.planning/core/bench.md): `CORPUS` producer benchmark entries and their deterministic-input recipes over the runtime bench tier.

[PACKAGE]:
- [78]-[BUNDLE](.planning/package/bundle.md): `Bundle` shared package-plane vocabulary and port floor; mints no receipt.
- [79]-[CODEC](.planning/package/codec.md): `Codec` single-blob ZSTD/LZ4/BROTLI/GZIP compression producer composing the bundle.
- [80]-[ARCHIVE](.planning/package/archive.md): `Archive` multi-file 7z/ZIP archive half and the reproducible-ZIP owner.
- [81]-[DELTA](.planning/package/delta.md): `Delta` detools binary diff/patch arm over parent-keyed delta nodes.

## [02]-[DOMAIN_PACKAGES]

Domain-specific libraries admitted by this folder; admission rows ride the workspace manifests as bare names, `uv.lock` fixes every version, and this folder's `.api/` corroborates.

[DOCUMENTS]:
- `reportlab`
- `weasyprint` — HTML-to-PDF with outline tree.
- `typst` — PDF/A compile with data binding.
- `pymupdf`
- `pypdfium2`
- `pdf-oxide` — Rust PDF extract/render/create/forms.
- `pypdf` — Assembly and outline/transform egress.
- `pikepdf` — Repair, encrypt, overlay, and structure-tree authoring.
- `python-docx`
- `python-pptx`
- `openpyxl`
- `xlsxwriter` — Write-only XLSX with charts and formats.
- `python-calamine` — Fast read-only XLSX/XLS/ODS ingest.
- `odfpy` — OpenDocument read/write.
- `docxtpl` — DOCX template render over jinja2.
- `msoffcrypto-tool` — Encrypted Office decrypt at ingest.
- `pdfplumber` — Page text/table/word geometry extraction.
- `ocrmypdf` — OCR text layer over scanned PDF.
- `lxml`
- `ruamel-yaml`
- `tomlkit`
- `jinja2`
- `papermill`
- `nbclient`
- `nbconvert` — Notebook export to HTML/PDF/script.
- `jupytext` — Notebook/text round-trip.

[VISUALIZATION]:
- `altair`
- `matplotlib`
- `lets-plot` — FLOOR-GATED with no wheel, sdist, or nix route; second host-free chart engine whose `lets_plot` arms rail `<engine-unavailable>`.
- `vl-convert-python` — Primary host-free chart export.
- `vegafusion` — Chart export transform pre-pass.
- `great-tables` — Publication-table producer.
- `polars` — First-class table and frame substrate.

[DIAGRAMS]:
- `rustworkx` — Graph layout, detail DAG, and plan producer graph.
- `grandalf` — Second Sugiyama layered-layout engine.
- `pyelk` — ELK layered/orthogonal/ports/nesting layout.
- `fast-sugiyama` — Rust Sugiyama layered placement.
- `kiwisolver` — Cassowary constraint-layout solver.
- `ziafont` — Glyph text-to-SVG-path outlining.
- `ziamath` — Math-to-SVG rendering.
- `latex2mathml` — LaTeX-to-MathML front-end ziamath drives; composed directly at the `commands.FUNCTIONS` operator registry.
- `schemdraw` — Native-SVG schematic diagrams.
- `drawpyo` — Editable draw.io export.
- `pvlib` — NREL solar-position ephemeris.

[IMAGING]:
- `pillow` — Raster IO/transform/ICC, annotation, metadata.
- `scikit-image` — Measured scores, transforms, and registration; builds at the floor under the manifest's pythran metadata override.
- `pyvips` — Fused libvips decode/downscale/ICC/smartcrop.
- `resvg-py` — SVG-to-raster render.
- `tifffile` — TIFF container IO and layered-TIFF writer.
- `psdtags` — Photoshop TIFF image resources.
- `imagecodecs` — Deep-pixel file and channel-byte codec rails, display-container array writes, ICC transform, BCn decode.
- `openexr` — Named-channel, multi-part, and tiled EXR documents.
- `pyktx` — In-process KTX2 container with Basis/ASTC encode and block transcode.

[IMAGING_TOOLS]: Host binaries the texture producers spawn.
- `ktx` — Unified KTX-Software CLI holding the KTX2 encode floor the python and C# branches spawn; TS consumes the produced bytes.

[VECTOR_CAD]:
- `svgelements` — Pure-Python SVG geometry and parse.
- `skia-pathops` — Boolean/offset/stroke-to-outline.
- `drawsvg` — Hierarchical named-layer SVG authoring.
- `ezdxf` — DXF model, render backend, block store, symbol-table lowering.

[MARKS]:
- `segno` — QR/Micro-QR.
- `python-barcode` — Linear 1D symbologies.
- `zxing-cpp` — 2D-matrix symbology encode/decode.

[COLOR]:
- `colour-science`
- `coloraide` — CSS-space parse/interpolate/gamut-map.
- `colour-cxf` — CxF3 spot/spectral color exchange.
- `opencolorio` — Config-driven transform graph, CPU/GPU processors, and the scene-linear working-space role.

[TYPOGRAPHY]:
- `fonttools`
- `uharfbuzz` — OpenType shaping and outline bridge.
- `blackrenderer` — COLRv1 color-glyph render.
- `python-bidi` — UAX#9 bidirectional reorder.
- `uniseg` — Unicode line/grapheme/word segmentation.
- `pyphen` — Language-aware soft-hyphenation.
- `opentype-feature-freezer` — Freezes OpenType features into the default set.
- `vharfbuzz` — HarfBuzz shaping QA and buffer-diff.
- `PyICU` — ICU line-break, bidi, and collation power path.

[EXCHANGE]:
- `pyhanko` — PAdES PDF signing and conformance.
- `c2pa-python` — C2PA content-credential sign/verify.
- `puremagic` — Pure-Python format sniffer, default detect path.
- `python-magic` — Format-ID power path over libmagic.
- `pyexiftool` — Cross-format descriptive-metadata read/write.

[EDITABLE_EXPORT]:
- `simpleidml` — IDML package and template mutation.
- `psd-tools` — Sole native PSD/PSB owner: layered author, read/inspect, composite, and structural readback.
- `pdfimpose` — Saddle/wire/card/cut/fold/signature page-order.

[MEDIA]:
- `av` — PyAV container/codec/filtergraph.
- `pysubs2` — Subtitle parse/convert/retime/restyle.

[SCENE]:
- `pyvista` — OVERLAY; the `scene/render_worker#WORKER` plotter surface.
- `vtk` — OVERLAY; `vtkmodules` exporters at `scene/export#EXPORT` and `scene/stage#STAGE`.
- `usd-core` — OVERLAY; `pxr` stage authoring at `scene/stage#STAGE`.

[COMPRESSION]:
- `zstandard`
- `brotli`
- `zlib-ng` — Accelerated gzip/zlib behind the GZIP codec.
- `py7zr`
- `stream-zip` — Streaming ZIP emit.
- `stream-unzip` — Streaming ZIP ingest.
- `detools` — Binary diff/patch for delta bundles.

## [03]-[SUBSTRATE_PACKAGES]

Shared substrate consumed from the Python registry, whose charters own the full contracts; `libs/python/.api/` holds the shared API evidence.

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

[IDENTITY]:
- `xxhash` — XXH3 digests behind the bundle, archive, and transmittal content preimages.

[COMPRESSION]:
- `lz4` — `lz4.frame` codec row behind the compression producer and delta patch store.

[WIRE_CODEGEN]:
- `rasm.contracts` — `appearance_pb` plane-set classes `graphic/texture/set` emits; `fabrication_pb.FeatureControl` `drawing/dimension` decodes.
- `protovalidate` — Descriptor-owned standard and CEL admission over completed generated appearance documents, retaining structured violations whole.
