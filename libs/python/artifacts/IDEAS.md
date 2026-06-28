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

[CONTENT_KEYED_REUSE_AND_SIGNAL]-[QUEUED]: artifact receipts become the ARTIFACT_PIPELINE sub-graph-elision spine and the measurement edge.
- Capability: artifact production contributes cache-hit and measured-output facts through the existing receipt fold instead of minting an artifacts cache or metrics owner, and supplies the elision substrate the `ARTIFACT_PIPELINE` plan folds into whole-chain reuse.
- Shape: every producer threads `(ContentKey, Work)` into the runtime session lane admission port and reports duration, bytes, compression ratio, and hit/miss through `receipt/receipt#RECEIPT` `contribute`; this card owns the keying spine, not a durable cache or store.
- Unlocks: expensive PDFs, notebooks, publication tables, bundles, and offscreen scenes become content-keyed elision targets and observable render outputs that `ARTIFACT_PIPELINE` composes into sub-graph elision with no new artifacts surface.
- Anchors: `ContentIdentity.of(...)`, `ArtifactReceipt`, `receipt/receipt#RECEIPT`, runtime `execution/lanes` `Keyed[T] = (ContentKey, Work[T])`, branch `CONTENT_ADDRESSED_REUSE_FABRIC`, branch `ONE_MEASURED_SIGNAL_STREAM`, and runtime `observability/metrics`.
- Tension: runtime session-lane admission elision and the `MeterProvider` instrument set must land before artifacts can consume them; durable content-addressed identity stays the C# `Rasm.Persistence` owner and is never minted here.
- Ripple: branch `ONE_MEASURED_SIGNAL_STREAM` — the measurement edge consumes the one runtime metric stream whose realization is the in-pass `runtime/observability/metrics.md` `MeterProvider` rewrite, never a separate runtime card; artifacts contribute through `ArtifactReceipt.contribute` and mint no metric owner.

[OUTWARD_FIGURE_HANDOFF_AND_DRIFT]-[QUEUED]: figures leave through one handoff axis and one drift guard.
- Capability: outward artifact figures share the Python graduation rail and structural drift detector rather than creating an artifacts-specific handoff or canonical-name guard.
- Shape: figure, table, chart, and scene outputs cross outward only as the `compute/graduation` `HandoffAxis` `model-asset` case keyed by `ContentIdentity`.
- Unlocks: artifacts can export sibling-consumable visual assets while proving zero private handoff rails and zero canonical-concept re-mints.
- Anchors: `ContentIdentity`, `RuntimeRail`, `Receipt`, `ReceiptContributor`, `ArtifactReceipt`, `compute/graduation#GRADUATION`, branch `ONE_GRADUATION_RAIL_OUTWARD`, and branch `CROSS_PACKAGE_DRIFT_GUARD`.
- Tension: upstream `compute/graduation` `HandoffAxis` and runtime `Structural.drift` must land before artifacts can verify the live drift query.

[ACCESSIBILITY]-[QUEUED]: an `accessibility/` sub-domain authors the PDF/UA tagged-structure tree into the PDF and self-verifies it, the screen-reader, alt-text, reading-order, and language-tagging floor enterprise document pipelines legally require.
- Capability: the `StructureNode` tree becomes marked-content structure authored into the PDF, plus an `AUDIT` producing an `AccessVerdict` over tag coverage, alt-text presence, reading-order validity, and language tagging self-verified against the authored tree.
- Shape: a new `accessibility/tagged.py` owner discriminates `AccessOp` (`TAG`/`AUDIT`) folding the `StructureNode`/`StructEltKind` tree into the PDF marked-content structure tree over `pikepdf` `StructTreeRoot`/`StructElem` object-model writes.
- Unlocks: legally required accessibility for the PDF pipeline, closing the structural verdict `typography/sign#SIGN` honestly disclaims.
- Anchors: `documents/model#NODE` `StructureNode` plus the `STRUCT_ELT_KIND` collapse, `pikepdf` `StructTreeRoot`/`StructElem` over `pikepdf.Object`, `typography/sign#SIGN`, and the settled `receipt/receipt#RECEIPT` `ArtifactReceipt.Egress`/`Pdf` reuse target.
- Tension: the `accessibility/tagged.md` page-authoring work-item has not landed (no `.planning` page exists yet), so the owner stays QUEUED until the `Access`/`AccessOp` `Tag`/`Audit` owner is authored; `AUDIT` self-verifies the authored marked-content structure tree against the source-of-truth `StructureNode` because artifacts authored the tree, and the `pikepdf` `StructTreeRoot`/`StructElem` object-model writes author the tree as a large object-model spike with no external grade and no JVM.

[MEDIA]-[QUEUED]: a `media/` sub-domain encodes a frame sequence — chart-over-time, rotating 3D scene, simulation playback — to MP4/WebM/GIF, the science-suite gap where a temporal study has no video artifact path.
- Capability: video and audio encode and mux over PyAV with bundled FFmpeg, folding a `tuple[bytes, ...]` PNG frame sequence from scene or chart through a streaming encoder.
- Shape: a new `media/encode.py` owner discriminates `MediaOp` (`ENCODE_VIDEO`/`ENCODE_AUDIO`/`MUX`) over `av.open`, `add_stream`, `VideoFrame.from_ndarray`, and `stream.encode`, contributing a new `ArtifactReceipt.Media` case over codec, frame count, duration, and byte count.
- Unlocks: temporal artifacts for animated charts and rotating scenes, closing the greenfield video gap.
- Anchors: `av.open(sink, mode='w')`, `add_stream`, `VideoFrame.from_ndarray`, `AudioFrame`, `stream.encode`, `OutputContainer.mux`, `figures/scene#SCENE` `SceneOp.Frames` rgb24 frame source, the settled `receipt/receipt#RECEIPT` `ArtifactReceipt.Media` case, and `.api/av.md`.

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

[VIEWPOINT]-[COMPLETE]: the `CameraPose` viewpoint value plus the `RenderSpec` scalar-bar/axes/lighting/background overlay flags and the `viewed` camera fold realize the deterministic scene-presentation sub-axis on `figures/scene/scene.md`; the `Plotter.camera`/`show_axes`/`add_scalar_bar`/`Plotter(lighting=)` member spellings carry the marked `[SCENE_VIEW]` `.api/pyvista.md` RESEARCH deepen.
[SYMBOLOGY]-[COMPLETE]: the full encode-decode encoded-mark owner landed as `figures/marks/marks.md` `Mark` over the `Symbology` axis — segno QR/Micro-QR/sequence, the python-barcode `get_barcode_class` linear-1D registry, and zxing-cpp `create_barcode`/`Barcode.to_svg` 2D-matrix DataMatrix/PDF417/Aztec/MaxiCode — with the `MarkOp.Decode` `read_barcodes` round-trip inverse the generation arms cannot express, all dependency-free SVG on the runtime and only the Pillow-opening `Decode` raster-intake crossing the gated subprocess seam; the separate-2D-matrix-owner routing note is resolved. The higher-order parent of `ENCODED_MARK_OWNER`.
[ENCODED_MARK_OWNER]-[COMPLETE]: subsumed by `SYMBOLOGY` — the one `Symbology` axis spanning QR/Micro-QR (segno), linear 1D (python-barcode), and 2D-matrix (zxing-cpp) plus the encode-decode round-trip lands on `figures/marks/marks.md`, not a QR-only special case or per-symbology class set.
[PROVENANCE]-[COMPLETE]: the `provenance/credential.md` `Provenance` owner discriminating `ProvenanceOp` (`Sign`/`Read`/`Verify`/`Ingredient`) over c2pa-python `Builder`/`Reader` with the per-case `SignerSpec` cert/callback union and the `C2paSigningAlg` ES/PS/ED25519 axis landed, contributing the `receipt/receipt#RECEIPT` `Credential` case and the `csharp:Rasm.Persistence` content-key binding; cross-format content-authenticity orthogonal to the `typography/sign#SIGN` PAdES close (PDF read-only here), over the present `.api/c2pa-python.md`.
[ARTIFACT_PIPELINE]-[COMPLETE]: the `pipeline/plan.md` `ArtifactPipeline` owner folds the `ArtifactWork` producer-node graph (each node its `ContentIdentity.of(...)` `ContentKey` plus its `Work[ArtifactReceipt]` thunk and parent-key edges) through `graphlib.TopologicalSorter` into front-ordered `runtime/execution#LANE` `Admit.keyed` units, projecting the per-artifact `receipt/receipt#RECEIPT` `contribute` folds into one sub-graph-elision `PipelinePlan` and owning no cache/store/scheduler/drain/DAG re-mint — the third reuse-fabric consumer beside the lane elision and the `MeterProvider` stream.
[INGEST]-[COMPLETE]: the recover-TO inverse widened past PDF on `documents/lens.md` — the `DOCX_READ` (python-docx form-and-content), `YAML_READ`/`TOML_READ` (ruamel-yaml/tomlkit core), and `XML_READ` (lxml gated) arms fold through the one `_node`/`_value_node` constructor into `DocumentNode`/`to_corpus_row` beside the realized `ODS_READ` (odfpy core) and `XLSX_READ` (python-calamine gated) arms, the `.api/python-calamine.md` cite clearing the `OFFICE_INGEST` RESEARCH marker; uniform corpus ingest from every admitted format. Ripple `data` `[CORPUS_INGEST]`.
