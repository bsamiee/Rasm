# [PY_ARTIFACTS_IDEAS]

The folder's forward pool of higher-order concepts, each grounded in artifact production and the host-free companion charter. Open ideas are cards in `[1]-[OPEN]`; a finished or dropped idea moves to `[2]-[CLOSED]` with a one-line disposition. Each idea drives one or more `TASKLOG.md` tasks.

OPEN contains `ACTIVE` work and `QUEUED` next-up work in logical sequence; `BLOCKED` keeps open but non-actionable work; `CLOSED` separates finished `COMPLETE` items from unimplemented `DROPPED` items. `Ripple` names the origin or counterpart card a cross-folder entry pairs with.

## [01]-[OPEN]

<!-- source-only: open idea card template:
[ID]-[STATUS]: <ambitious concise thesis>.
- Capability: <higher-order concept, invariant, or owner capability>.
- Shape: <what the idea becomes as a system, product, owner, or feature set(s)>.
- Unlocks: <new branch, package, workflow, proof, user, or agent capability made possible>.
- Anchors: <owners, seams, packages, doctrines, or techniques that make the idea plausible>.
- Tension: <only when an unresolved constraint, boundary, bet, or dependency shapes the idea>.
- Ripple: <origin/counterpart card this entry pairs with across folders, as `pkg` `[SLUG]`; present only on a cross-folder ripple counterpart card>.
-->

[ENCODED_MARK_OWNER]-[QUEUED]: one encoded-mark axis covers QR/Micro-QR, linear 1D, and 2D-matrix marks over one round-trip.
- Capability: machine-readable mark production joins the preview owner as one encoded-mark family spanning QR, linear 1D, and 2D-matrix symbologies with encode-decode round-trip, not a QR-only special case or per-symbology class set.
- Shape: `figures/preview#PREVIEW` carries one `PreviewOp.MARK` row and one `Symbology` axis over `segno` QR/Micro-QR, `python-barcode` linear registry rows, and `zxing-cpp` 2D-matrix DataMatrix/PDF417/Aztec/MaxiCode rows.
- Unlocks: dependency-free SVG mark output across QR, Micro-QR, linear 1D, and 2D-matrix classes with generation-correctness proof from one decode pass.
- Anchors: `figures/preview#PREVIEW`, `PreviewOp.MARK`, `Symbology`, `segno`, `python-barcode` `get_barcode_class`, `zxingcpp.create_barcode`, `zxingcpp.read_barcodes`, `SVGWriter`, `PROVIDED_BARCODES`, and `.api/python-barcode.md`.
- Tension: the linear and 2D-matrix rows stay blocked on uv-sync plus `assay api` reflection; `zxing-cpp` lands on the companion subprocess seam while `segno`/`python-barcode` stay cp315-core.

[CONTENT_KEYED_REUSE_AND_SIGNAL]-[QUEUED]: artifact receipts become the ARTIFACT_PIPELINE sub-graph-elision spine and the measurement edge.
- Capability: artifact production contributes cache-hit and measured-output facts through the existing receipt fold instead of minting an artifacts cache or metrics owner, and supplies the elision substrate the `ARTIFACT_PIPELINE` plan folds into whole-chain reuse.
- Shape: every producer threads `(ContentKey, Work)` into the runtime session lane admission port and reports duration, bytes, compression ratio, and hit/miss through `receipt/receipt#RECEIPT` `contribute`; this card owns the keying spine, not a durable cache or store.
- Unlocks: expensive PDFs, notebooks, publication tables, bundles, and offscreen scenes become content-keyed elision targets and observable render outputs that `ARTIFACT_PIPELINE` composes into sub-graph elision with no new artifacts surface.
- Anchors: `ContentIdentity.of(...)`, `ArtifactReceipt`, `receipt/receipt#RECEIPT`, runtime `execution/lanes` `Keyed[T] = (ContentKey, Work[T])`, branch `CONTENT_ADDRESSED_REUSE_FABRIC`, branch `ONE_MEASURED_SIGNAL_STREAM`, and runtime `observability/metrics`.
- Tension: runtime session-lane admission elision and the `MeterProvider` instrument set must land before artifacts can consume them; durable content-addressed identity stays the C# `Rasm.Persistence` owner and is never minted here.

[OUTWARD_FIGURE_HANDOFF_AND_DRIFT]-[QUEUED]: figures leave through one handoff axis and one drift guard.
- Capability: outward artifact figures share the Python graduation rail and structural drift detector rather than creating an artifacts-specific handoff or canonical-name guard.
- Shape: figure, table, chart, and scene outputs cross outward only as the `compute/graduation` `HandoffAxis` `model-asset` case keyed by `ContentIdentity`.
- Unlocks: artifacts can export sibling-consumable visual assets while proving zero private handoff rails and zero canonical-concept re-mints.
- Anchors: `ContentIdentity`, `RuntimeRail`, `Receipt`, `ReceiptContributor`, `ArtifactReceipt`, `compute/graduation#GRADUATION`, branch `ONE_GRADUATION_RAIL_OUTWARD`, and branch `CROSS_PACKAGE_DRIFT_GUARD`.
- Tension: upstream `compute/graduation` `HandoffAxis` and runtime `Structural.drift` must land before artifacts can verify the live drift query.

[PROVENANCE]-[QUEUED]: a `provenance/` sub-domain signs every emitted artifact with a tamper-evident C2PA content-credentials manifest, the cross-format content-authenticity floor beyond conformance's PDF-only PAdES signing.
- Capability: cross-format content credentials thread every image, PDF, and video with a signed manifest carrying creator, action, and ingredient assertions plus the `ContentKey` as a hard binding, with sign, read, verify, and ingredient-chain operations.
- Shape: a new `provenance/credential.py` owner discriminates `ProvenanceOp` (`SIGN`/`READ`/`VERIFY`/`INGREDIENT`) over `c2pa-python` with one `Manifest` value object carrying assertions plus ingredients, contributing a new `ArtifactReceipt.Provenance` case over manifest validity, assertion count, and ingredient chain.
- Unlocks: a signed, verifiable provenance chain on every artifact MIME type, distinct from PAdES PDF-internal signatures, so graduating artifacts cross outward already content-authenticated.
- Anchors: `c2pa-python` `Builder.sign`, `Reader.from_file`, `Reader.validation_state`, `documents/egress#FINISH`, `receipt/receipt#RECEIPT`, and `ContentIdentity`.
- Tension: `c2pa-python` is a Rust-FFI wheel that execution provisions through pyproject plus uv sync, an `assay api` probe, an authored `.api/c2pa-python.md`, and result banding, staying QUEUED; it stays distinct from `conformance#SIGN` PAdES and never collapses into it.

[ACCESSIBILITY]-[QUEUED]: an `accessibility/` sub-domain authors the PDF/UA tagged-structure tree into the PDF and self-verifies it, the screen-reader, alt-text, reading-order, and language-tagging floor enterprise document pipelines legally require.
- Capability: the `StructureNode` tree becomes marked-content structure authored into the PDF, plus an `AUDIT` producing an `AccessVerdict` over tag coverage, alt-text presence, reading-order validity, and language tagging self-verified against the authored tree.
- Shape: a new `accessibility/tagged.py` owner discriminates `AccessOp` (`TAG`/`AUDIT`) folding the `StructureNode`/`StructEltKind` tree into the PDF marked-content structure tree over `pikepdf` `StructTreeRoot`/`StructElem` object-model writes.
- Unlocks: legally required accessibility for the PDF pipeline, closing the structural verdict `conformance#AUDIT` honestly disclaims.
- Anchors: `documents/model#NODE` `StructureNode` plus the `STRUCT_ELT_KIND` collapse, `pikepdf` `StructTreeRoot`/`StructElem` over `pikepdf.Object`, and `conformance#AUDIT`.
- Tension: `AUDIT` self-verifies the authored marked-content structure tree against the source-of-truth `StructureNode` because artifacts authored the tree, and the `pikepdf` `StructTreeRoot`/`StructElem` object-model writes author the tree as a large object-model spike with no external grade and no JVM.

[MEDIA]-[QUEUED]: a `media/` sub-domain encodes a frame sequence — chart-over-time, rotating 3D scene, simulation playback — to MP4/WebM/GIF, the science-suite gap where a temporal study has no video artifact path.
- Capability: video and audio encode and mux over PyAV with bundled FFmpeg, folding a `tuple[bytes, ...]` PNG frame sequence from scene or chart through a streaming encoder.
- Shape: a new `media/encode.py` owner discriminates `MediaOp` (`ENCODE_VIDEO`/`ENCODE_AUDIO`/`MUX`) over `av.open`, `add_stream`, `VideoFrame.from_ndarray`, and `stream.encode`, contributing a new `ArtifactReceipt.Media` case over codec, frame count, duration, and byte count.
- Unlocks: temporal artifacts for animated charts and rotating scenes, closing the greenfield video gap.
- Anchors: `av.open(sink, mode='w')`, `add_stream`, `VideoFrame`, `AudioFrame`, `stream.encode`, `figures/scene#SCENE` frame source via `SCENE_TIMESERIES_FRAMES`, and `receipt/receipt#RECEIPT`.
- Tension: `av`/PyAV is cp315-core with an abi3 wheel and bundled FFmpeg, so media encodes in-process synchronously inside the async boundary, never `anyio.to_process` and never a gated band; the frame source is `SCENE_TIMESERIES_FRAMES`, so `MEDIA` is no longer a sink without a producer.

[ARTIFACT_PIPELINE]-[QUEUED]: one declarative `ArtifactPlan` composes chart->compose->report->emit->egress->conform->provenance->bundle by producing the `(ContentKey, Work)` pairs the runtime session lane elides, so each unchanged stage is a cache hit without artifacts owning any cache, store, scheduler, or DAG.
- Capability: a single pipeline spine renders a report as one plan whose unchanged stages short-circuit on the runtime lane's session cache, turning the reuse-fabric consumer into the pipeline's elision face rather than a per-producer afterthought.
- Shape: an `ArtifactPlan` folds each owner's existing `ContentIdentity.of(...)` into a `Keyed[T] = (ContentKey, Work[T])` the runtime `LanePolicy.cached` admits, while `StagePlan` already owns the topological DAG via `graphlib.TopologicalSorter` so the pipeline never re-mints ordering; placement resolves to a new top-level `pipeline/` sub-domain.
- Unlocks: sub-graph elision across the whole production chain consumed from the runtime lane port, raising `CONTENT_KEYED_REUSE_AND_SIGNAL` from per-producer keying to whole-chain elision.
- Anchors: every `_emit` already returns `ContentIdentity.of(...)`, `receipt/receipt#RECEIPT` `contribute`, runtime `execution/lanes` `Keyed[T] = (ContentKey, Work[T])`, and `StagePlan.execute` `graphlib` DAG.
- Tension: the runtime lane cache is session-local in-memory and a durable lane cache is a deleted form, so durable identity federation stays the C# `Rasm.Persistence` owner; `ArtifactPlan` produces `Keyed` pairs and owns no cache, no store, no scheduler, and no DAG re-mint.

[INGEST]-[QUEUED]: the recover-to inverse for the non-PDF formats — the corpus thesis that emission and extraction are inverses over one `DocumentNode` algebra is PDF-only, while spreadsheet, `.docx`, ODF, and structured-text have no inverse owner anywhere.
- Capability: spreadsheet over `python-calamine`, `.docx` over `python-docx` read accessors, ODF over `python-calamine`/odfpy, and structured-text over `ruamel-yaml`/`tomlkit`/`lxml` all lower into `DocumentNode`/`to_corpus_row`, making every admitted format a true inverse.
- Shape: a deepen of `documents/lens.md` where the recovery concern widens from PDF-only to recover-from-any-format feeding the runtime columnar corpus lane uniformly, resolved at page altitude rather than a new sub-domain.
- Unlocks: the other half of the bidirectional seam the suite left open, with uniform corpus ingest from every format.
- Anchors: `documents/lens#LENS`, `documents/model#NODE` `to_corpus_row`, `python-calamine` `CalamineWorkbook.from_path`/`get_sheet_by_name`/`to_python`, `python-docx` read accessors, and `ruamel-yaml`/`tomlkit` load.
- Tension: `python-calamine` is companion `<3.15` with no cp315 wheel, so the spreadsheet and ODF read lands gated on the subprocess seam beside `lxml`, while the `.docx` and structured-text read arms are cp315-core; docxtpl authors templates and is not part of ingest.
- Ripple: `data` `[CORPUS_INGEST]`

[SYMBOLOGY]-[QUEUED]: elevate the MARK preview arm into a full encode-decode encoded-mark owner spanning `segno` QR/Micro-QR core, `python-barcode` linear 1D core, and `zxing-cpp` 2D-matrix DataMatrix/PDF417/Aztec/MaxiCode companion, round-trip verifiable.
- Capability: every symbology class becomes one polymorphic owner with generation-correctness proof, where `create_barcode` then `read_barcodes` yields a decode-confidence receipt fact and SVG output stays dependency-free for all classes.
- Shape: the QR-arm, 1D-arm, and 2D-owner trio fragmented across `preview.md` collapses into one `Symbology` axis with a `PreviewOp.DECODE` round-trip op, resolving the page's separate-2D-matrix-owner routing note.
- Unlocks: 2D-matrix codes — the explicitly dropped DataMatrix/PDF417 — plus round-trip verification, all in one owner.
- Anchors: `figures/preview#PREVIEW`, `PreviewOp.MARK`, `Symbology`, `segno.make`/`make_sequence`, `python-barcode` `get_barcode_class`, `zxingcpp.create_barcode`/`BarcodeFormat`/`Barcode.to_svg`/`read_barcodes`.
- Tension: `zxing-cpp` is companion `<3.15` on the subprocess seam while `python-barcode` stays cp315-core for 1D; this is the higher-order parent of the `ENCODED_MARK_OWNER` idea and refines that idea rather than duplicating it.
- Ripple: `data` `[CORPUS_INGEST]`

[FORMS]-[QUEUED]: a form-fill and form-recovery sub-domain — no current owner fills or harvests AcroForm fields, yet three admitted libs own the surface: `pikepdf` AcroForm/DefaultAppearanceStreamGenerator, `pymupdf` add_widget/widgets/bake, and `pypdfium2` PdfFormEnv/init_forms.
- Capability: a polymorphic fill, harvest, and flatten dispatch crosses emit, lens, and egress bidirectionally like documents, filling programmatically with appearance-stream generation, harvesting into `FieldNode`, and flattening via bake.
- Shape: a new form owner where fill authors AcroForm fields, harvest is the lens recovery into `FieldNode` since the model tree already carries a field node kind, and bake flatten is the egress close.
- Unlocks: programmatic Word and PDF form workflows — fill, extract, and flatten — a genuine new sub-domain crossing three existing owners.
- Anchors: `pikepdf` `AcroForm`/`DefaultAppearanceStreamGenerator`/`ExtendedAppearanceStreamGenerator`, `pymupdf` `add_widget`/`widgets`/`delete_widget`/`bake`, `pypdfium2` `PdfFormEnv`/`init_forms`, and `documents/model#NODE` `FieldNode`.
- Tension: the owner spans three sub-domains, so owner placement resolves between a new sub-domain and a deepen of model, lens, and egress before a settled fence; the `FieldNode` kind already exists, so harvest recovers into an existing variant.

[COMPOSITION]-[QUEUED]: a chart-composition algebra — layer, hconcat, vconcat, and facet small-multiples — as a `ChartSpec`-level fold, mirroring `figures/compose`'s SVG n-up but at the spec layer, since altair owns layer/hconcat/vconcat/FacetChart and lets-plot owns GGBunch/SupPlotsSpec, both unreachable through the owner today.
- Capability: multi-panel scientific figures author as one composite `ChartSpec` rather than externally tiled SVGs, spanning the chart and compose sub-domains at the spec layer.
- Shape: a composite-root fold on `ChartSpec` over the engine composition operators, where the engines already carry the capability verified in `.api/altair.md` composition rows, so the gap is purely owner-side.
- Unlocks: composite multi-panel figures without external tiling, a genuinely larger concern than a single task.
- Anchors: `altair` `layer`/`hconcat`/`vconcat`/`FacetChart`, `lets-plot` `GGBunch`/`SupPlotsSpec`, `figures/chart#CHART`, and `.api/altair.md` `[03]-[ENTRYPOINTS]`.
- Tension: the algebra is only reachable once `CHART_GRAMMAR_ALGEBRA` lands the build algebra, since composition is the next layer over the mark/encode algebra.

[DOCUMENT_REVIEW]-[QUEUED]: the already-built `DocumentDelta` diff/merge/invert algebra surfaces as a user-facing reviewable change product — redline/track-changes between versions and a content-keyed audit trail — not just an internal corpus-diff primitive.
- Capability: professional document-comparison, the redline feature every legal and publishing suite ships, comes for free from the existing algebra, plus an invert-backed undo stack, pairing with `REDACTION_BURN` for the irreversible-redaction close.
- Shape: a `DocumentReview` owner projects `tuple[DocumentDelta, ...]` into a visual redline via compose and emit plus an invert-backed undo stack, landing in a reports or documents page rather than a new sub-domain.
- Unlocks: redline and track-changes from the existing diff/merge/invert fold, with the reversible-until-burned redaction patch closing under `REDACTION_BURN`.
- Anchors: `documents/model#DELTA` diff/merge/invert, `documents/emit#DOCUMENT`, `figures/compose#COMPOSE`, and `REDACTION_BURN`.
- Tension: the owner is pure composition of existing owners, so page placement resolves before a settled fence rather than minting a new sub-domain.

## [02]-[CLOSED]

<!-- source-only: closed task card template:
[ID]-[COMPLETE|DROPPED]: <one-line disposition>; keep closed tasks collapsed unless a second retained fact changes future routing.
-->

[VIEWPOINT]-[COMPLETE]: the `CameraPose` viewpoint value plus the `RenderSpec` scalar-bar/axes/lighting/background overlay flags and the `viewed` camera fold realize the deterministic scene-presentation sub-axis on `figures/scene.md`; the `Plotter.camera`/`show_axes`/`add_scalar_bar`/`Plotter(lighting=)` member spellings carry the marked `[SCENE_VIEW]` `.api/pyvista.md` RESEARCH deepen.
