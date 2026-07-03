# [PY_ARTIFACTS_IMPOSITION]

The press-imposition owner laying emitted sheets into a press-ready imposed form. `Imposition` is ONE owner over the imposition pipeline carrying a closed-payload `ImposeOp` `expression.tagged_union` whose every case carries its own typed payload — `Impose` the drawing case carrying the `source`, `Scheme`, `Geometry`, and `Marks` it discriminates on, `Plan` the compute-only pre-flight case, `Proof` the raster-proof case (a single imposed sheet or a multi-sheet contact strip carrying a `ProofPolicy` that selects the RGB-screen / CMYK-press-separations / GRAY-density ink model, the raster codec, a trim/bleed clip, a registration-tint / dot-gain gamma / film-negative finish, and an optional output-intent ICC `proof` profile that ICC-soft-proofs the imposed sheet through `ImageCms.buildProofTransform` + `Flags.SOFTPROOFING|GAMUTCHECK` with an out-of-press-gamut count — the ICC-managed separations preflight the raw device conversion cannot warn on, the profile arriving via the `graphic/color/managed#MANAGED` seam) — never a per-scheme `Nup`/`Booklet`/`Signature` draw family differing only by a literal scheme and a `leaves` knob, and never a `StrEnum` keyed against an erased `dict[str, object]` bag. It reads an already-emitted multi-page PDF (the sheet set from `composition/sheet#SHEET`, the document body from `document/emit#DOCUMENT`) and re-orders, scales, rotates, and crops each source page onto a larger imposed sheet — n-up grid placement, saddle-stitch booklet pagination, folded-signature ordering, the work-and-turn / work-and-tumble single-plate duplex schemes, cut-and-stack, and the come-and-go / perfect-bind / sheetwise impositions through the local `pymupdf.Page.show_pdf_page(...)` placement engine, plus the provider-native `wire` (wire/spiral-bound drawing-set), `hardcover` (sewn folded-signature book), `cards` (front/back sample/keynote cards), and `zine` (single-sheet 8-page fold) bindery forms whose fold geometry only the admitted `pdfimpose` schema wrappers own — the engine selected by `Geometry.engine` for the dual-capable schemes and forced to `pdfimpose` for the provider-native ones. `Scheme` is the closed `StrEnum` over every imposition scheme; a locally-placeable scheme is a `PLANS` policy-table row binding ONE `place(pages, geometry)` placement computation that fuses page order, recto/verso rotation, and per-sheet creep shift, while a provider-native scheme (`wire`/`hardcover`/`cards`/`zine`) is a `_PDFIMPOSE_SCHEMAS` row ONLY (the inverse of local-only `perfect-bind`, which the provider exposes no schema for), so a new scheme is one member plus one row and `scheme in PLANS` is the locally-placeable discriminant every consumer reads; the imposed-sheet count is DERIVED from that one placement `Block` (`_sheet_count` the `Block.fold` max over each `Placement.sheet` plus one), never a second parallel formula that drifts from the placement. `Placement` is the closed-payload value object each computed placement carries — source page index, target imposed-sheet index, target cell rect, the `Quarter`-refined verso-rotation axis, the fit-scale and over/under-draw flags, the SHARED signature-group name it is a member of, the group default-visibility and reader-lock state, the `Membership` OCMD visibility policy, and bleed clip — so the local imposition fold reads every placement field straight onto the `show_pdf_page` keyword set over the one live native page, binds each member to its shared signature-group OCG through a `Document.set_ocmd(ocgs=[group], policy=)` membership dictionary (the nested reader-layer tree the sibling `composition/sheet#SHEET` mints, a reader toggling the whole signature) rather than N flat duplicate `add_ocg` groups, then a single `Document.set_layer(0, on=, off=, locked=)` ui-config write drives each deduped signature group's reader-panel toggle and lock state exactly as `composition/sheet#SHEET` `_configure_layers` does, never a per-scheme mutable draw loop, never a hard-coded `rotate=0`, and never a minted OCG with no reader visibility/lock config behind it. `Geometry` is the row-policy value object the imposition carries — the imposed-sheet size and orientation, the `ImpositionEngine` provider row, the `Across`-refined n-across/n-down grid, the inner binding-edge gutter, the `omargin` outer/trim margin insetting the whole grid, the head/foot trim, the spine allowance, the `Leaves`-refined fold/signature leaf count, the per-signature creep extent and its push-out / shingle-in direction, the lay feed edge and its reserved gripper claw margin, the bleed/trim margin, the `last` back-cover pin (trailing pages held with blank-fill before them), and the `cards` `back` verso-order token — projecting once through `partition(shift)` to the binding-and-gripper-aware cell-rect grid every scheme's placement list reads, every field live in that projection, the load-bearing divisor invariants `Is`-refined so a non-positive grid count or negative outer margin rails as `api` at admission rather than dividing by zero in the fold. `Marks` is the press-finishing policy the imposed document carries — the LOCAL `press` printer's-mark set (`PressMark` crop / fold / registration / colour-bar drawn on the imposed sheet at the computed cell/margin boundaries through `pymupdf.Page.new_shape`/`draw_line`/`draw_rect`/`draw_circle`/`Shape.finish`/`commit`, DISTINCT from the `overlay` flag that routes a FIGURE-placement overlay SHAPE to `composition/compose#COMPOSE`, which sees figure placement not the press-form cell grid), the print-control marks the press needs, the per-signature imposition-map navigation outline, the cut-list / fold-map embedded press file, the interactive-layer bake, font subsetting, image recompression, hidden-content scrub, the optional-content layer config, the deterministic linearized save, and the press info/XMP metadata — so a press-ready imposed PDF is one finishing pass, never a parallel sheet re-author. One imposition surface discriminating the operation, not a per-scheme imposer family. `pymupdf` resolves on the runtime (native MuPDF cp310-native), so the local arm defers its engine through one module-scope `lazy import` and offloads the `_composed` fold off the event loop through a `CapacityLimiter`-bounded `to_thread.run_sync`; `pdfimpose` is also provider-contained, accepting `(io.BytesIO(source),)` and `io.BytesIO()` at the schema wrapper boundary and returning only imposed PDF bytes plus local `Composed`/`ImposedPlan` facts. The imposed sheets the fold produces are handed onward to `document` assembly (the imposed body is the document the assembler collates), so this owner computes the imposition and places the pages but assembles no document; it projects the imposed layout outward as `Imposition.layers -> tuple[Layer, ...]` (one row per OCG-bound placement) to `export/layered#LAYERED` exactly as the sibling `composition/sheet#SHEET` does, so the named-layer authoring lands once in the export owner and this owner projects the rows. This owner is the DEDICATED booklet/signature press-imposition engine distinct from the simple `document/egress#FINISH` `IMPOSE` step, which is the in-document `pypdf.Transformation` n-up grid fold over an already-finished PDF — egress imposes a finished PDF as one finishing step among ten, while this owner computes the saddle-stitch creep-compensated page order, the folded-signature ordering, and the work-and-turn duplexing the egress step never reaches. The fault rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(subject, thunk, catch=_FAULTS)` over the real engine raise tuple `_FAULTS = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)` — admitting `pymupdf.FileDataError`/`EmptyFileError` (`RuntimeError`-derived), `pdfimpose.UserError` (a direct `BaseException` subclass that must be named explicitly), a malformed `Rect(*cell)` extent or off-axis-rotation `ValueError`, a non-positive-grid / negative-extent `BeartypeCallHintViolation` from the `@_GUARD`-contracted `_admit` scalar seam `Geometry.partition` calls at the division site, a `PLANS`/`_PDFIMPOSE_SCHEMAS` key miss, an out-of-range `Proof` sheet-index `IndexError`, and the stream/source file fault as distinct `BoundaryFault` cases rather than the `Exception` catch-all the faults owner rejects and rather than a parallel `ImposeFault` `Literal` the boundary never reads; the interior raises nothing of its own, the live-native-page mutation the one platform-forced statement seam `boundaries.md` names — every opened `pymupdf` handle bracketed by `with` so it closes deterministically on each exit (success, fault, cancellation), never GC-reaped — cancellation excluded from `_FAULTS` and re-raised as the structured signal. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` through the receipt owner's named flat-scalar mints selected by the `Composed.kind` discriminant — the OCG-bearing `Impose` arm `ArtifactReceipt.Egress(key, bytes, sheets, 0, 0, overlays)` carrying the imposed byte count, the REAL `Document.page_count`, and the minted-OCG layer count on the `overlays` slot exactly as the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms do (a no-mark imposition the degenerate `ArtifactReceipt.Pdf(key, bytes, sheets)` form), the `Proof` arm `ArtifactReceipt.Preview(key, width, height)` carrying the pixmap extent, and the `Plan` arm none — its `ImposedPlan` pre-flight rides `Imposition.planned`, never a fabricated zero-sheet `Pdf` receipt over plan-JSON bytes.

## [01]-[INDEX]

- [01]-[IMPOSE]: n-up / booklet / signature / work-and-turn / cut-and-stack / come-and-go / perfect-bind / sheetwise locally-placeable plus wire / hardcover / cards / zine provider-native press-imposition owner over the closed-payload `ImposeOp` `tagged_union` (`Impose` the drawing case discriminating on the `Scheme` value, `Plan` the compute-only pre-flight case, `Proof` the `ProofPolicy`-driven RGB/CMYK/GRAY raster-proof case) folded once into a `Composed` evidence struct the `of`/`contribute`/`layers` projections share, rail-typed `RuntimeRail[ContentKey]` over `async_boundary(catch=_FAULTS)`, every imposition routing through the single `core/plan#PLAN` `ArtifactPipeline` production entry as a producer node, dispatched to `pymupdf` (`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, rotate, clip, oc)` cross-document source-page placement with head-to-head verso rotation, fit-scale, bleed clip, and OCG/OCMD layer binding; `add_ocg`+`set_ocmd`+`set_layer` shared-signature-group OCG mint, OCMD membership nesting, and reader visibility/lock config; `Page.new_shape`+`Shape.draw_line`/`draw_rect`/`draw_circle`+`Shape.finish`/`commit` the LOCAL press-form crop/fold/registration/colour-bar mark draw at the imposed-cell/sheet boundaries; `set_toc`/`embfile_add`/`bake`/`scrub`/`subset_fonts`/`rewrite_images`/`set_metadata`/`set_xml_metadata`/`tobytes` press finishing; `get_pixmap(colorspace=csRGB|csCMYK|csGRAY, clip=)` + `Pixmap.tint_with`/`gamma_with`/`invert_irect` + `Pixmap.tobytes`/`pil_tobytes`/`samples` press-faithful proof, bridged to a PIL Image for the `ImageCms.buildProofTransform` + `Flags.SOFTPROOFING|GAMUTCHECK` ICC soft-proof; core) and to the `pdfimpose` provider schema wrappers (`saddle`/`cutstackfold`/`copycutfold`/`hardcover`/`wire`/`cards`/`onepagezine`.impose, honoring only `mark=['crop']`); `Scheme`/`Place`/`Placement`/`Geometry`/`Marks`/`ImposedPlan`/`Artifact`/`ProofInk`/`ProofRaster`/`ProofPolicy`/`Membership`/`PressMark` the closed scheme/placement/geometry/finishing/proof/layer/mark value-object family; the `frozendict[Scheme, Place]` `PLANS` local-placement table and the `frozendict[Scheme, PdfImposeSchema]` `_PDFIMPOSE_SCHEMAS` provider table whose per-schema `accepts` frozenset filters the one candidate kwarg dict; the one `_composed` total `match` lowering the `Impose` arm to `_imposed` over the resolved placements (or `_pdfimposed` for the provider engine and every provider-native scheme), the `@_GUARD`-contracted `_admit` scalar seam `Geometry.partition` calls at the division site raising the grid/extent shape violation the `_FAULTS` tuple admits; projects `Imposition.layers` to `export/layered#LAYERED` `Layer` (empty for a provider imposition that mints no local OCG); threads `core/receipt#RECEIPT` through the receipt owner's named `ArtifactReceipt.Egress`/`Pdf`/`Preview` flat-scalar mints selected by the `Composed.kind` discriminant (the `Preview` `scores` band carrying the soft-proof out-of-gamut count). The output-intent/proof ICC profile and the CxF3 separations arrive via the `graphic/color/managed#MANAGED` seam, never a direct `colour-cxf` import.

## [02]-[IMPOSE]

- Owner: `Imposition` the one imposition owner discriminating the operation over the closed `ImposeOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; the verb family is three cases — `Impose` the drawing case carrying `(source, Scheme, Geometry, Marks)` discriminating on the `Scheme` value, `Plan` the compute-only pre-flight case, and `Proof` the `ProofPolicy`-driven RGB/CMYK/GRAY raster-proof case — never a `Nup`/`Booklet`/`Signature` sibling triple differing only by the literal scheme and a `leaves` knob the `Geometry` already carries. `Scheme` the closed `StrEnum` over the press schemes — the locally-placeable set (`NUP` raw n-across-by-n-down grid, `BOOKLET` saddle-stitch 2-up centerfold, `SIGNATURE` folded-signature multi-leaf, `WORK_AND_TURN` single-plate duplex flipped across the gripper edge, `WORK_AND_TUMBLE` single-plate duplex flipped head-to-foot down the lead edge, `CUT_AND_STACK` guillotine-cut sequential stack, `COME_AND_GO` two-up duplicate-set tumble, `PERFECT_BIND` n-up flat bind-edge stack, `SHEETWISE` recto/verso separate-plate grid) plus the provider-native set whose fold geometry only `pdfimpose` owns (`WIRE` wire/spiral-bound stacked pages, `HARDCOVER` sewn folded-signature book, `CARDS` front/back sample/keynote cards with `Geometry.back` verso order, `ZINE` single-sheet 8-page 2x4 fold); `PLANS` the one `frozendict[Scheme, Place]` policy table binding each locally-placeable scheme straight to its `place(pages, geometry)` placement function (a provider-native scheme carries no `PLANS` row and routes to `_pdfimposed` unconditionally), and `_PDFIMPOSE_SCHEMAS` the `frozendict[Scheme, PdfImposeSchema]` provider table each row's `accepts` frozenset filtering the one candidate kwarg dict so onepagezine never sees `signature`/`imargin`, cards never `last`, and wire never `bind` — page order, recto/verso `rotate`, fit-scale, and per-sheet creep shift fused into one sweep returning the full `Placement` list — the table the totality proof of one placement arm per scheme, never an `if scheme == ...` cascade and never a parallel imposer class; the imposed-sheet count is the derived `_sheet_count(placements)` over that one list, never a second `sheets` callable that drifts from the placement; `Placement` the closed-payload value object each placement carries (source page index, imposed-sheet index, cell rect, rotate axis, fit-scale flag, OCG layer name, the OCG default-visibility and reader-lock state, bleed clip rect) projecting every field straight onto the `show_pdf_page` keyword set and feeding the one `set_layer` reader-config write; `Geometry` the row-policy value object carrying the imposed-sheet size and orientation, the n-across/n-down grid, the inner binding-edge gutter, the `omargin` outer/trim margin, the head/foot trim, the spine allowance, the fold count and signature leaf count, the per-sheet creep shift, the bleed/trim, the `last` back-cover pin, and the `cards` `back` verso-order token, projecting once through `partition` to the binding-aware cell grid every locally-placeable scheme reads with every field live; `Marks` the press-finishing policy carrying the overlay route, the print-control mark set, the per-signature outline route, the cut-list/fold-map attachment, the interactive-layer bake flag, the font-subset flag, the image-recompress flag, the hidden-content scrub flag, the optional-content layer config, the deterministic linearized-save flag, and the press info/XMP metadata. `pymupdf` owns the `Page.show_pdf_page` cross-document source-page draw (every `keep_proportion`/`rotate`/`clip`/`oc` keyword a `Placement`/`Marks` field), the `open`/`new_page(width=, height=)` imposed-sheet construction at the `Geometry.oriented` press-sheet dimensions, the `add_ocg`-mint-then-`set_layer(0, on=, off=, locked=)`-config OCG-layer reader binding, the `set_toc`/`embfile_add`/`bake`/`subset_fonts`/`rewrite_images`/`scrub`/`set_metadata`/`set_xml_metadata` press finishing, the `get_pixmap(colorspace=, clip=)` + `tint_with`/`gamma_with`/`invert_irect` + `tobytes`/`pil_tobytes` press-faithful RGB/CMYK/GRAY proof raster, and the `tobytes(garbage=, deflate=, use_objstms=)` deterministic serialize; the per-scheme page-order, creep, verso-rotation, and tumble arithmetic is this owner's page-settled fold over that one cross-document placement floor, never a re-implemented PDF byte emitter.
- Cases: `ImposeOp` cases — `Impose(source, scheme, geometry, marks)` (the one drawing case — the `_composed` arm resolves `PLANS[scheme]` over the source page count and `Geometry`, opens the source through `pymupdf.open(stream=source)`, raises the `pymupdf.EmptyFileError` the `_FAULTS` tuple admits on a zero-page source, computes the `Placement` list and the derived `_sheet_count` once, allocates that many imposed pages at the `Geometry` imposed-sheet size on the live document, draws `_draw_one` over the placements where each `_draw_one` mints `add_ocg(p.name, on=p.visible)` for a `name`-bearing placement and draws `out[p.sheet].show_pdf_page(...)` on the live native page returning its `(xref, visible, locked)` mint row, `Block.choose(_oc_state)` keeps the rows that minted a real xref, one `_configure_layers` `set_layer(0, on=, off=, locked=)` write drives the reader visibility/lock config over the minted set, so `Marks.serialize` runs once on the all-placed document and `Composed.layers` carries the minted total, then runs the `Marks.finished` press finish — the saddle order, the head-to-head verso rotation `180·(cell // across) % 360`, the per-fold creep, the work-and-turn / work-and-tumble duplex mirror, the come-and-go duplicate-set, and the sheetwise recto/verso split all carried by the resolved `PLANS` row, the case unchanged) · `Plan(source, scheme, geometry)` (compute-only pre-flight — `_planned` resolves `PLANS[scheme]` over the source page count and `Geometry`, returning the `ImposedPlan` `msgspec.Struct` carrying the placement rows, the per-scheme metrics — signature count, fold depth, sheets, blank-pad count, per-sheet creep extent — and the derived sheet count without drawing, the pre-flight arm a press operator reads to validate the imposition before committing the draw) · `Proof(source, dpi, sheet, policy)` (raster proof — the `_composed` proof arm opens the already-imposed press-form PDF and discriminates the `sheet` selector on input shape: an `int` runs `_rasterized` — `get_pixmap(dpi=, colorspace=_colorspace(policy.ink), clip=)` in the `ProofPolicy` ink model (RGB screen, CMYK separations, GRAY density), the `tint_with`/`gamma_with`/`invert_irect` registration-tint / dot-gain gamma / film-negative finish, then `_proofed` — the raw `_encoded` policy raster (native `tobytes` PNG/PSD or the `pil_tobytes` WEBP/AVIF/TIFF bridge) when no output-intent profile is set, else the `_softproof` ICC-managed separations proof: bridge the pixmap to a PIL Image via `pil_tobytes`/`samples`, `ImageCms.buildProofTransform` the working sRGB reference against the `ProofPolicy.proof` press OUTPUT-INTENT under `Flags.SOFTPROOFING` and again under `SOFTPROOFING | GAMUTCHECK`, `applyTransform` both, and count the differing out-of-press-gamut pixels the gamut alarm paints onto the `Preview` receipt `scores` band — the ICC-managed CMYK-separations preflight the raw `get_pixmap(csCMYK)` device conversion cannot warn on, the output-intent profile arriving via the `graphic/color/managed#MANAGED` seam — over that one imposed sheet, a tuple drives `_contact` to lay the selected sheets — every sheet when empty — row-major into one montage page through the same `show_pdf_page` draw and rasterize the strip once through the SAME `_rasterized` press path, both keyed by the same `ContentKey` with the `Pixmap.width`/`height` riding the `Preview` receipt, so a bindery proofs ANY single imposed sheet — recto, verso, or an inner signature page — in ICC-managed CMYK separations with a gamut warning or a whole-signature contact strip, never only the first and never a re-imposition) — matched by one total `match`/`case`; never a per-scheme imposer sibling, never a parallel booklet-versus-signature draw method, never a per-page draw call family, never a hard-coded `rotate=0`. The `Marks` finish, the fit-scale, and the bleed `clip` ride the one drawing case through the shared `_draw_one` live-page sweep, so a new scheme is one `Scheme` member plus one `PLANS` row, never a new case.
- Auto: `_composed(op) -> Composed` is the ONE total `match` both `of` and `contribute` read — no second render recomputes the bytes. The `Impose` arm resolves `PLANS[scheme]` over the source page count and `Geometry` and hands the resolved `Placement` `Block` plus the `Geometry`/`Marks` to `_imposed`; the `@_GUARD`-contracted `_admit` scalar seam `Geometry.partition` calls at the division site has already deep-checked the `Is`-refined `Across`/`Leaves`/`Span` scalars (a `msgspec.Struct` field or a `Block` element is not deep-checked by beartype — only a direct scalar parameter is) and railed a non-positive grid count or negative extent as `api` BEFORE the placement resolution that feeds the native draw; `_imposed` computes the derived `_sheet_count` once, allocates that many imposed pages through `Document.new_page(width=, height=)` at the `Geometry` imposed-sheet size on the live `out` document, mints ONE shared signature-group OCG per unique placement `name` through `_mint_groups`, and folds `_draw_one` over the placement `Block` — each `_draw_one` binding a `name`-bearing placement to its shared group through a `set_ocmd(ocgs=[group], policy=p.policy)` OCMD membership dictionary (the `composition/sheet#SHEET` nested-layer pattern — a reader toggles the whole signature, never N flat duplicate `add_ocg` groups labelled `sig-N`) and drawing `out[p.sheet].show_pdf_page(Rect(*p.cell), src, pno=p.source, keep_proportion=p.fit, overlay=p.overlay, rotate=p.rotate, clip=Rect(*p.clip) if p.clip else None, oc=oc)` on the live native page (never a held `Page` list that outlives `out` and faults on draw, the live-handle mutation the `boundaries.md` platform-forced statement seam), returning its `(group, visible, locked)` row — drawing each source page into its computed cell at its computed rotation, fit-scale, over/under draw, and bleed clip; `Block.choose(_oc_state)` keeps the rows that bound a real group and one `_configure_layers` write drives the reader's per-signature-group visibility and lock panel over the DEDUPED shared groups exactly as the sibling `composition/sheet#SHEET` `_configure_layers` does, then runs the `Marks.finished(out, geometry, sheets)` press finish (the imposition-map `set_toc`, the cut-list `embfile_add`, the `bake`/`scrub`/`subset_fonts(fallback=True)`/`rewrite_images`/`set_metadata`/`set_xml_metadata` keyed off the `Marks` flags) and `_press_marks(out, geometry, marks.press)` — the LOCAL crop/fold/registration/colour-bar draw at the computed imposed-cell rects and sheet margins through `Page.new_shape`/`draw_line`/`draw_rect`/`draw_circle`/`Shape.finish`/`commit` (DISTINCT from the `Marks.overlay` figure-overlay route to `composition/compose#COMPOSE`, which sees figure placement not the press-form cell grid), then the `Marks.serialize(out)` deterministic `tobytes`, returning `Composed(data, sheets=out.page_count, layers=len(groups))` reading the REAL `Document.page_count` off the imposed document. The native-handle `Block.map`/`Block.choose` sweep is the platform-forced seam exactly as the sibling `composition/sheet#SHEET` `_composed` and `export/layered#LAYERED` `_pdf_ocg` arms run it — a `Block.fold`/`traverse` over a discarded mutated-document `Result[Document, Never]` is the index-threaded fold the `rails-and-effects.md` page rejects, not the dense form, because the engine raise the boundary converts replaces a per-element `Result` thread that can never carry an `Error`. The `_grid`/`_folded`/`_duplexed`/`_stacked`/`_paired`/`_split` placement functions are the `PLANS` rows (the `NUP` and `PERFECT_BIND` schemes both bind `_grid`, perfect-bind carrying its `spine` allowance on the `Geometry`; the `WORK_AND_TURN` and `WORK_AND_TUMBLE` schemes both bind `_duplexed`, differing only by the `on_across` mirror-axis policy value, two rows over one body never two functions); the `cell` for each placement comes from `partition` (binding-edge gutter, head/foot trim, spine allowance, the `shingle`-signed creep shift, bleed margin all applied there). The `Plan` arm `_planned` resolves the same placement function over the page count (or, for a provider-native scheme absent from `PLANS`, an empty local model carrying scheme/pages/sheet-size/engine) and projects the `ImposedPlan`, drawing no surface; the `Proof` arm opens the source and discriminates the `sheet` selector on input shape — an `int` runs `_rasterized(src[sheet], dpi, policy)`, a tuple drives `_contact` to montage the selected sheets (every sheet when empty) through `show_pdf_page` and rasterize the strip once through the same `_rasterized` press path — where `_rasterized` renders `get_pixmap(colorspace=_colorspace(policy.ink), clip=)` in the `ProofPolicy` ink model and applies the `tint_with`/`gamma_with`/`invert_irect` finish, and `_encoded(pixmap, policy.raster)` writes the native `tobytes` PNG/PSD or the `pil_tobytes` WEBP/AVIF/TIFF bytes, carrying the pixmap extent onto the `Composed.kind` raster discriminant. `of` maps the `Composed` to the content key through `ContentIdentity.key`; `contribute` reads the same `_composed` and routes on `Composed.kind` plus the `Composed.layers` count to the receipt owner's named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mint, so the byte count, page count, and OCG-layer count are the computed facts, never a recompute and never a fabricated zero-sheet `Pdf` receipt over non-PDF bytes.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the receipt owner's named flat-scalar mints — the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count select the mint once and each named static factory keyword-constructs its `case()`, so the imposition owner adds NO sibling factory and NO new receipt case. The OCG-bearing `Impose` op selects `ArtifactReceipt.Egress(key, len(data), composed.sheets, 0, 0, composed.layers)` carrying the imposed byte count, the REAL imposed-sheet page count, and the minted-OCG layer count on the `overlays` slot (zero encryption-R/outline-depth — imposition is neither a security nor a navigation close, exactly the slot the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms report their authored-layer count on), and a degenerate no-mark imposition (`composed.layers == 0`) selects `ArtifactReceipt.Pdf(key, len(data), composed.sheets)`; the `Proof` op selects `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], composed.scores)` carrying the pixmap extent plus the soft-proof out-of-gamut band (empty when no output-intent profile is set). `contribute` reads the SAME `_composed(op)` `of` drives (one fold), mints the key over `Composed.data` through the same `ContentIdentity.key` `of` uses (self-contained, no caller-passed key), and yields `receipt.contribute()` — never a per-kind facts `Struct` re-wrapping the scalars the named mint already takes positionally (the indirection the receipt owner rejects), and never a `dispatch(kind, *scalars)` bag. The `Plan` op contributes no receipt — its `ImposedPlan` projection is a pre-flight payload, not an imposed PDF, so a fake `0`-page `Pdf` receipt over plan-JSON bytes is the deleted form; a press operator reads the `ImposedPlan` directly through `Imposition.planned`.
- Growth: a new locally-placeable imposition scheme is one `Scheme` member plus one `Place` row in `PLANS` whose `place` computes its order, rotation, fit-scale, creep, and count, and a new provider-native scheme is one `Scheme` member plus one `_PDFIMPOSE_SCHEMAS` row carrying its `impose` function and its `accepts` kwarg frozenset — never a parallel imposer class, never an `if scheme == ...` branch, never a new `ImposeOp` case; a new geometry knob (a variable gutter, a crossover bleed, a per-signature creep curve, a lay-edge jog, an outer trim, a back-cover pin) is one `Geometry` field read in `partition`, the placement fold, or the `_pdfimpose_kwargs` candidate dict — never an `Impose` parameter; a new placement axis (a mirrored verso, a per-cell margin, a named OCG layer) is one `Placement` field carried straight onto the `show_pdf_page` keyword set — never a per-placement draw method; a deeper signature is the same `_folded` fold over a larger `Geometry.leaves`; a new press-finish concern (an output-intent, a crop-mark profile, a colour-bar route, a press-attachment) is one `Marks` field read in the finish pass; a new LOCAL press-form printer's mark is one `PressMark` member plus one `match` arm and one `Shape.draw_*` helper in `_press_marks` (the crop/fold/registration/colour-bar set the imposed-cell grid draws locally), while a FIGURE-placement overlay SHAPE still routes through the `Marks.overlay` seam to `composition/compose#COMPOSE` `Overlay` (which sees figure placement, not the press-form cell grid) — the two are distinct seams, not one grafted arm; a nested reader-layer axis (a signature group, a recto/verso side group) is one `Placement.membership`/`policy` field the `set_ocmd` fold nests; the proof modality discriminates on the `Proof` `sheet` shape — an `int` proofs one imposed sheet, a tuple proofs a contact strip of the selected sheets — so a further proof axis (a new ink model, a raster codec, a per-sheet caption) is one `ProofInk`/`ProofRaster` member or one `ProofPolicy` field the `_rasterized`/`_proofed` fold reads, the ICC soft-proof gamut-warning overlay realized by the `ProofPolicy.proof` output-intent profile through `_softproof` with the out-of-gamut count on the `Preview` `scores` band, never a sibling proof op. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import math
from collections.abc import Callable, Iterable
from enum import StrEnum
from io import BytesIO
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

from anyio import CapacityLimiter, to_thread
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from builtins import frozendict
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, json, structs

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer

lazy import pymupdf

# the lcms2 soft-proof surface for the press-form separations preflight — the OUTPUT-INTENT/proof ICC profile
# arrives via the graphic/color/managed#MANAGED seam (which owns colour-cxf CxF3 + ImageCms), NEVER a direct
# colour-cxf import here; the shared pillow ImageCms lane is composed for the imposition proof, the profile the color owner's.
lazy from PIL import Image as PilImage, ImageChops, ImageCms
lazy from pdfimpose import UserError as PdfImposeUserError
lazy from pdfimpose.schema import cards as pdf_cards
lazy from pdfimpose.schema import copycutfold as pdf_copycutfold
lazy from pdfimpose.schema import cutstackfold as pdf_cutstackfold
lazy from pdfimpose.schema import hardcover as pdf_hardcover
lazy from pdfimpose.schema import onepagezine as pdf_onepagezine
lazy from pdfimpose.schema import saddle as pdf_saddle
lazy from pdfimpose.schema import wire as pdf_wire

if TYPE_CHECKING:
    from pymupdf import Colorspace, Document, Page, Pixmap, Shape

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Box = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Edge = Literal["left", "right", "top", "bottom"]
type Lay = Literal["short", "long"]  # the press lay/gripper edge — feed direction, distinct from the binding edge
type Place = Callable[[int, "Geometry"], "Block[Placement]"]

# the load-bearing geometry invariants the `partition`/`_folded` arithmetic divides by and offsets with —
# `Across`/`Leaves`/`Span` are the DIRECT scalar parameters of `_admit`, so the `_GUARD` beartype contract
# `Geometry.partition` calls at the division site rejects a non-positive grid count or negative extent
# before the divide, not a `ZeroDivisionError` deep in the fold (msgspec ignores `Is` at struct
# construction, so the scalar seam is the only site that enforces it). `Quarter` documents the 0/90/180/270
# verso-rotation set the `_folded` fold computes and the `show_pdf_page` `rotate` keyword admits; an
# off-axis angle reaching the draw is the `pymupdf` `ValueError` the `_FAULTS` tuple admits.
type Across = Annotated[int, Is[lambda n: n >= 1]]
type Leaves = Annotated[int, Is[lambda n: n >= 1]]
type Span = Annotated[float, Is[lambda v: v >= 0.0]]  # a non-negative gutter/trim/spine/bleed extent
type Quarter = Annotated[int, Is[lambda d: d in (0, 90, 180, 270)]]


class Scheme(StrEnum):
    # locally-placeable schemes carry a `PLANS` row (the show_pdf_page placement arm); provider-native
    # schemes carry a `_PDFIMPOSE_SCHEMAS` row ONLY — their fold geometry (wire spine, sewn signatures,
    # front/back cards, 2x4 zine) is pdfimpose's and has no local show_pdf_page equivalent, exactly as
    # `PERFECT_BIND` is the inverse (local-only, no provider schema). Membership in `PLANS` is the
    # locally-placeable discriminant every downstream consumer reads.
    NUP = "nup"
    BOOKLET = "booklet"
    SIGNATURE = "signature"
    WORK_AND_TURN = "work-and-turn"
    WORK_AND_TUMBLE = "work-and-tumble"
    CUT_AND_STACK = "cut-and-stack"
    COME_AND_GO = "come-and-go"
    PERFECT_BIND = "perfect-bind"
    SHEETWISE = "sheetwise"
    WIRE = "wire"  # provider-native: individual pages cut, stacked, wire/spiral-bound — the AEC drawing-set / spec-book bindery form (pdfimpose.schema.wire)
    HARDCOVER = (
        "hardcover"  # provider-native: sewn folded-signature book, distinct from the local `_folded` saddle block (pdfimpose.schema.hardcover)
    )
    CARDS = "cards"  # provider-native: front/back sample/swatch/keynote cards, `Geometry.back` orders the verso sources (pdfimpose.schema.cards)
    ZINE = "zine"  # provider-native: single-sheet 8-page fold-zine, fixed 2x4 fold (pdfimpose.schema.onepagezine)


class Artifact(StrEnum):  # the Composed receipt-shape discriminant — imposed vector PDF vs raster proof
    PDF = "pdf"
    PREVIEW = "preview"


class ImpositionEngine(StrEnum):
    LOCAL = "local"  # local placement facts over pymupdf show_pdf_page
    PDFIMPOSE = "pdfimpose"  # admitted pdfimpose schema wrapper, normalized back to local facts/receipts


class ProofInk(StrEnum):  # the Proof colorspace axis — pymupdf `get_pixmap(colorspace=)` selects the ink model
    RGB = "rgb"  # csRGB screen proof (default); pairs with a PNG/WEBP/AVIF raster
    CMYK = "cmyk"  # csCMYK press-separations proof a bindery reads; pairs with a CMYK-capable PSD/TIFF raster
    GRAY = "gray"  # csGRAY single-ink density proof


class ProofRaster(StrEnum):  # the Proof egress codec — native `Pixmap.tobytes` vs the `pil_tobytes` bridge
    PNG = "png"  # native tobytes RGB/GRAY raster (default)
    PSD = "psd"  # native tobytes CMYK-capable separations raster
    WEBP = "WEBP"  # pil_tobytes bridge — a format MuPDF's native tobytes lacks
    AVIF = "AVIF"  # pil_tobytes bridge
    TIFF = "TIFF"  # pil_tobytes bridge — CMYK-capable container


class Membership(StrEnum):  # the ISO 32000 OCMD visibility policy — the member value IS the pymupdf `set_ocmd(policy=)` token
    ANY_ON = "AnyOn"  # a placement is visible when ANY member OCG is on (default nested signature-layer union)
    ALL_ON = "AllOn"  # visible only when EVERY member OCG is on (signature AND side)


class PressMark(StrEnum):  # the local press-form printer's-mark set drawn at the imposed-cell/sheet boundaries
    CROP = "crop"  # L-shaped trim ticks at each imposed-cell corner — the guillotine cut guides
    FOLD = "fold"  # dashed fold lines down the inter-column gutters — the signature fold axes
    REGISTRATION = "registration"  # concentric target + crosshair at each sheet-edge midpoint — multi-plate alignment
    COLOR_BAR = "color-bar"  # a CMYK + gray control-patch row along the foot margin — the densitometer strip


# --- [CONSTANTS] ------------------------------------------------------------------------
# pymupdf raises `FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or
# zero-page source, a `PLANS`/`_PDFIMPOSE_SCHEMAS` miss raises `KeyError`, a malformed `Rect(*cell)`
# extent raises `ValueError`, an out-of-range `Proof` sheet index raises `IndexError`, the source
# stream raises `OSError`, pdfimpose raises `PdfImposeUserError`, and `_GUARD` raises
# `BeartypeCallHintViolation` on a non-positive grid count or off-axis verso rotation.
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)

# The native-offload bounded slot the `of` path threads; the `_composed` MuPDF/pdfimpose fold never runs on the event loop.
_GATE: CapacityLimiter = CapacityLimiter(4)

# the Proof raster codecs MuPDF's native `Pixmap.tobytes` lacks — routed through the `Pixmap.pil_tobytes`
# Pillow bridge; every other `ProofRaster` (PNG, PSD) rides the native encoder.
_PIL_RASTERS: Final[frozenset[ProofRaster]] = frozenset({ProofRaster.WEBP, ProofRaster.AVIF, ProofRaster.TIFF})

# the colour control-bar patches (registration black, C, M, Y, K, quarter/half gray) as pymupdf RGB float-triple
# `draw_rect` fills — the densitometer strip a press operator reads against a proof; a new patch is one row.
_BAR_PATCHES: Final[tuple[tuple[float, float, float], ...]] = (
    (0.0, 0.0, 0.0),
    (0.0, 1.0, 1.0),
    (1.0, 0.0, 1.0),
    (1.0, 1.0, 0.0),
    (0.75, 0.75, 0.75),
    (0.5, 0.5, 0.5),
)


# --- [BOUNDARIES] -----------------------------------------------------------------------
# the contract weave admitting the load-bearing geometry invariants the `partition` arithmetic divides by
# and offsets with: `_admit` takes the grid counts, leaf count, and span extents as DIRECT `Is`-refined
# scalar parameters, so beartype deep-checks each (a `msgspec.Struct` field or a `Block`/`tuple` element is
# NOT deep-checked by beartype — only a direct scalar parameter is), raising `BeartypeCallHintViolation`
# the `_FAULTS`-narrowed `async_boundary` lifts to its `api` `BoundaryFault` case before the divide.
# `Geometry.partition` calls it at the division site, so every impose/plan/planned/layers path crosses it;
# `_aspected` is never minted — the boundary's `CLASSIFY` row owns the violation-to-fault lift, this is only
# the scalar shape admission at the seam that feeds the divide.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit(
    across: Across, down: Across, leaves: Leaves, gutter: Span, head_trim: Span, spine: Span, creep: Span, bleed: Span, omargin: Span, /
) -> None:
    return None  # the `@_GUARD` beartype contract IS the work — every `Is`-refined scalar is deep-checked on call


# --- [MODELS] ---------------------------------------------------------------------------
class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    landscape: bool = False
    engine: ImpositionEngine = ImpositionEngine.LOCAL
    across: Across = 2
    down: Across = 1
    leaves: Leaves = 1  # signature leaf count — the fold depth a saddle/signature block reads
    gutter: Span = 0.0  # inner binding-edge gutter (imargin) between facing cells
    omargin: Span = 0.0  # outer/trim margin around the whole imposed sheet — insets the cell grid on every edge (pdfimpose `omargin`)
    head_trim: Span = 0.0  # top/bottom finished-trim allowance
    spine: Span = 0.0  # perfect-bind glue/spine allowance added at the binding edge
    creep: Span = 0.0  # per-fold creep extent compensating fold-thickness drift
    bleed: Span = 0.0  # cell expansion past the trim box
    binding: Edge = "left"
    lay: Lay = "long"  # the press feed edge: which sheet edge the grippers seize, reserving `gripper` unprintable margin there
    gripper: Span = 0.0  # the unprintable claw margin the press reserves on the `lay` feed edge
    shingle: bool = False  # creep direction: push-out (outer pages shift out, default) vs shingle-in for a thick bound block
    last: int = 0  # trailing source pages pinned to the document end (back-cover), blank-fill inserted before them — provider bindery semantics (pdfimpose `last`)
    back: str = ""  # (CARDS only) the verso-source ordering token pdfimpose `cards.impose(back=)` reads

    @property
    def slots(self) -> int:
        return self.across * self.down

    @property
    def oriented(self) -> Dimensions:
        width, height = self.sheet
        return (height, width) if self.landscape else (width, height)

    def partition(self, shift: float = 0.0) -> tuple[Box, ...]:
        # the `_admit` contract deep-checks every `Is`-refined scalar at the division site — a non-positive
        # grid count or negative extent rails as `BeartypeCallHintViolation` the `_FAULTS` tuple admits
        # before the divide, never a `ZeroDivisionError` deep in the fold.
        _admit(self.across, self.down, self.leaves, self.gutter, self.head_trim, self.spine, self.creep, self.bleed, self.omargin)
        # `shift` is the per-signature creep extent the fold passes; `shingle` flips its direction —
        # push-out (default) drifts the outer signature away from the spine, shingle-in toward it. The
        # `omargin` outer trim insets the whole grid on every edge, the `gripper` claw margin is reserved
        # on the lead edge of the feed axis the `lay` edge names.
        width, height = self.oriented
        grip_x, grip_y = (self.gripper, 0.0) if self.lay == "short" else (0.0, self.gripper)
        cell_w = (width - 2.0 * self.omargin - (self.across + 1) * self.gutter - self.spine - grip_x) / self.across
        cell_h = (height - 2.0 * self.omargin - (self.down + 1) * self.gutter - 2.0 * self.head_trim - grip_y) / self.down
        on_x = self.binding in ("left", "right")
        creep = -shift if self.shingle else shift
        dx, dy = (creep, 0.0) if on_x else (0.0, creep)
        sign = -1.0 if self.binding in ("right", "bottom") else 1.0
        ox = self.omargin + self.gutter + grip_x + (self.spine if self.binding == "left" else 0.0)
        oy = self.omargin + self.head_trim + self.gutter + grip_y
        return tuple(
            (
                ox + col * (cell_w + self.gutter) + sign * dx - self.bleed,
                oy + row * (cell_h + self.gutter) + sign * dy - self.bleed,
                ox + col * (cell_w + self.gutter) + cell_w + sign * dx + self.bleed,
                oy + row * (cell_h + self.gutter) + cell_h + sign * dy + self.bleed,
            )
            for row in range(self.down)
            for col in range(self.across)
        )


class Placement(Struct, frozen=True):
    source: int
    sheet: int
    cell: Box
    rotate: Quarter = 0  # verso head-to-head flip — the `show_pdf_page` `rotate` admits 0/90/180/270 only
    fit: bool = True  # keep_proportion — scale-to-fit the cell preserving aspect
    overlay: bool = True  # draw above (True) or below the imposed-sheet content already on the page
    clip: Box | None = None
    name: str = ""  # the SHARED signature-group label a placement is a member of — one OCG per unique name, the OCMD nests each member under it (never N flat duplicate `sig-N` groups)
    visible: bool = True  # the group default-on state the `set_layer` ui config writes, as layered.Layer.visible
    locked: bool = False  # the reader-side layer-lock hint `set_layer(locked=)` honors, as layered.Layer.locked
    policy: Membership = Membership.ANY_ON  # the `set_ocmd(policy=)` visibility over the placement's signature-group membership


class Marks(Struct, frozen=True):
    overlay: bool = False  # route a FIGURE-placement overlay SHAPE to composition/compose#COMPOSE via this OCG (compose sees figure placement, not the press-form cell grid)
    press: tuple[
        PressMark, ...
    ] = ()  # the LOCAL press-form printer's marks drawn on the imposed sheet at the cell/margin boundaries — crop/fold/registration/colour-bar the `overlay` compose seam never reaches
    controls: tuple[str, ...] = ()  # print-control mark names the press info dict records
    imposition_map: bool = False  # author a per-signature outline so a reader/press navigates each fold section
    cut_list: tuple[tuple[str, bytes], ...] = ()  # cut-list / fold-map / job-ticket files embedded as PDF associated files
    bake: bool = False  # flatten interactive annotations/widgets into content before press
    subset: bool = True
    recompress: bool = False
    scrub: bool = False  # redaction-grade hidden-content/metadata removal before press
    linearize: bool = True  # deterministic web-optimized save with garbage collection and deflate
    info: tuple[tuple[str, str], ...] = ()
    xmp: str | None = None

    def finished(self, document: "Document", geometry: Geometry, sheets: int) -> None:
        # the live-native-handle press-finish seam: a `for`/`if` mutation chain over the one document
        # exactly as the sibling sheet/egress arms drive their finish; never a domain accumulator fold.
        if self.imposition_map:  # set_toc page numbers are 1-based
            document.set_toc([[1, f"Sheet {n + 1}", n + 1] for n in range(sheets)])
        for name, payload in self.cut_list:  # cut-list/fold-map/job-ticket rides as a PDF associated file
            document.embfile_add(name, payload, filename=name, desc="imposition press file")
        if self.bake:
            document.bake(annots=True, widgets=True)
        if self.scrub:  # strip hostile/hidden content but PRESERVE the press files just attached and the explicit info/XMP writes below
            document.scrub(
                hidden_text=True,
                javascript=True,
                clean_pages=True,
                embedded_files=False,
                attached_files=False,
                metadata=False,
                xml_metadata=False,
                redactions=False,
            )
        if self.recompress:
            document.rewrite_images(dpi_threshold=300, dpi_target=300, lossy=True)
        if self.subset:
            document.subset_fonts(fallback=True)
        if self.info or self.controls or self.overlay:
            # the `overlay` route marker rides the press keywords so the downstream compose owner reading
            # the imposed PDF knows the registration/crop/colour-bar overlay was requested for this lay.
            marks = (*self.controls, *(("overlay",) if self.overlay else ()), geometry.binding, geometry.lay)
            document.set_metadata({**dict(self.info), "keywords": ",".join(marks)})
        if self.xmp is not None:
            document.set_xml_metadata(self.xmp)

    def serialize(self, document: "Document") -> bytes:
        # `tobytes` is the in-memory egress; `ez_save`/`save` write a path. `no_new_id=True` suppresses the
        # randomized `/ID` so the same imposition is byte-identical run-to-run and mints one stable `ContentKey`
        # (without it the content key drifts every save); the web-optimized form rides `garbage=4`/`use_objstms=1`,
        # the plain form `garbage=3`.
        return (
            document.tobytes(garbage=4, deflate=True, use_objstms=1, no_new_id=True)
            if self.linearize
            else document.tobytes(garbage=3, deflate=True, no_new_id=True)
        )


class ImposedPlan(Struct, frozen=True):
    scheme: Scheme
    sheet: Dimensions  # the oriented imposed press-sheet size a press operator validates the plan against before committing the draw
    sheets: int  # imposed press sheets the placement fold yields
    pages: int  # source pages read off the live document
    leaves: int  # signature leaf count the fold depth reads
    signatures: int  # bound folding units (ceil(pages / 4·leaves) for a folded scheme, sheets otherwise)
    padded: int  # blank pages the fold pads the count to the next signature multiple
    creep: float  # outward creep extent applied at the outermost signature
    placements: tuple[Placement, ...]
    engine: ImpositionEngine = ImpositionEngine.LOCAL


class Composed(Struct, frozen=True):  # the one evidence struct of/contribute/layers read — no second render
    data: bytes
    sheets: int
    kind: Artifact = Artifact.PDF
    extent: tuple[int, int] = (0, 0)  # the Proof pixmap width/height the raster receipt rides
    layers: int = 0  # the count of shared signature-group OCGs the draw fold minted, riding the ArtifactReceipt.Egress overlays slot
    scores: frozendict[str, float | str] = (
        frozendict()
    )  # the Proof `ArtifactReceipt.Preview` band — the soft-proof out-of-gamut pixel count when an output-intent profile is set, empty otherwise


class ProofPolicy(Struct, frozen=True):
    # the press-proof rendering policy one behavior-carrying value carries (POLICY_VALUES), never a flag
    # tail the raster body re-derives: `ink` selects the `get_pixmap(colorspace=)` model, `raster` the
    # egress codec, `clip` a sub-region proof, and `tint`/`gamma`/`negative` the registration-tint /
    # dot-gain gamma / film-negative finish. The RGB+PNG default is the screen proof; a CMYK separations
    # proof pairs `ink=CMYK` with a CMYK-capable `raster=PSD`/`TIFF`, the pairing the caller states rather
    # than a body-derived matrix.
    ink: ProofInk = ProofInk.RGB
    raster: ProofRaster = ProofRaster.PNG
    clip: Box | None = None  # proof only this trim/bleed sub-region — the `get_pixmap(clip=)` extent
    tint: tuple[int, int] | None = None  # `Pixmap.tint_with(black, white)` registration-tint the press reads against a plate
    gamma: float | None = None  # `Pixmap.gamma_with(gamma)` dot-gain / tone-curve finish simulating the on-press tonal shift
    negative: bool = False  # `Pixmap.invert_irect()` film-negative proof
    proof: bytes | None = (
        None  # the OUTPUT-INTENT/press ICC profile — when set, the proof is ICC soft-proofed under `ImageCms.buildProofTransform` + `Flags.SOFTPROOFING|GAMUTCHECK` with an out-of-gamut count, the RAW `get_pixmap(csCMYK)` device conversion cannot warn on; the profile arrives via the graphic/color/managed#MANAGED seam (colour-cxf CxF3 + ImageCms), never a direct colour-cxf import here
    )


class PdfImposeSchema(Struct, frozen=True):
    # one provider schema row: its `impose` function plus the `accepts` set of optional kwargs its verified
    # signature honors — the DERIVED_LOGIC primary the `_pdfimpose_kwargs` filter reads, never three parallel
    # bind/creep/group bools the builder re-branches. A schema that does not accept a kwarg (onepagezine
    # takes no `signature`/`imargin`, cards no `last`, wire no `bind`) drops it rather than raising a TypeError.
    impose: Callable[..., None]
    accepts: frozenset[str]


# --- [TABLES] ---------------------------------------------------------------------------
# the fold-scheme kwarg set the three creep-bearing schemas (saddle/cutstackfold/copycutfold) share, the one
# primary the provider-native rows derive from (`hardcover` drops `creep`); each row's `accepts` is the exact
# optional-kwarg set that schema's verified `impose(...)` signature honors, so `_pdfimpose_kwargs` filters the
# one candidate dict rather than re-branching per schema and never passes a kwarg a schema would reject.
_FOLD_KW: Final[frozenset[str]] = frozenset({"signature", "imargin", "omargin", "mark", "bind", "creep", "group", "last"})

_PDFIMPOSE_SCHEMAS: Final[frozendict[Scheme, PdfImposeSchema]] = frozendict({
    Scheme.BOOKLET: PdfImposeSchema(pdf_saddle.impose, _FOLD_KW),  # saddle, group=1 (single-leaf centerfold)
    Scheme.SIGNATURE: PdfImposeSchema(pdf_saddle.impose, _FOLD_KW),  # saddle, group=leaves (multi-leaf signatures)
    Scheme.CUT_AND_STACK: PdfImposeSchema(pdf_cutstackfold.impose, _FOLD_KW),
    Scheme.COME_AND_GO: PdfImposeSchema(pdf_copycutfold.impose, _FOLD_KW),
    Scheme.HARDCOVER: PdfImposeSchema(pdf_hardcover.impose, _FOLD_KW - {"creep"}),  # sewn signatures — no creep kwarg
    Scheme.WIRE: PdfImposeSchema(
        pdf_wire.impose, frozenset({"signature", "imargin", "omargin", "mark", "last"})
    ),  # cards-derived — no bind/creep/group
    Scheme.CARDS: PdfImposeSchema(
        pdf_cards.impose, frozenset({"signature", "imargin", "omargin", "mark", "bind", "back"})
    ),  # front/back — no creep/group/last
    Scheme.ZINE: PdfImposeSchema(pdf_onepagezine.impose, frozenset({"omargin", "mark", "bind", "last"})),  # fixed 2x4 fold — no signature/imargin
})


def _sheet_count(placements: Block[Placement]) -> int:
    return placements.fold(lambda acc, p: max(acc, p.sheet), -1) + 1


# --- [OPERATIONS] -----------------------------------------------------------------------
def _saddle(slots: int) -> tuple[int, ...]:
    # the centerfold pairing (last, first, second, second-last, …) as one flat comprehension — at fold
    # position `i` the outer leaf is `slots - 1 - i`, the inner `i`, alternating by parity, no mutable index.
    return tuple(leaf for i in range(slots // 2) for leaf in ((i, slots - 1 - i) if i % 2 == 0 else (slots - 1 - i, i)))


def _grid(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=page // slots, cell=cells[page % slots]) for page in range(pages))


def _folded(pages: int, geometry: Geometry) -> Block[Placement]:
    fold, slots, across = 4 * max(geometry.leaves, 1), geometry.slots, geometry.across
    padded = pages + (-pages % fold)
    signature = _saddle(fold)
    return Block.of_seq(
        Placement(
            source=position + base,
            sheet=base // slots + slot // slots,
            cell=geometry.partition(geometry.creep * (base // fold))[slot % slots],
            rotate=180 * ((slot % slots) // across) % 360,
            name=f"sig-{base // fold + 1}",
        )
        for base in range(0, padded, fold)
        for slot, position in enumerate(signature)
        if position + base < pages
    )


def _duplexed(on_across: bool, /) -> Place:
    # one plate, both sides: the back side (odd group) is the same plate flipped, so its cells mirror —
    # across the row (work-and-turn, gripper-edge side flip) or down the column (work-and-tumble, lead-edge
    # head-to-foot flip) — the press registration the duplex scheme runs, never a same-cell overlap. The two
    # schemes differ only by the mirror axis policy value, so one body carries both rows, never two functions.
    def place(pages: int, geometry: Geometry) -> Block[Placement]:
        slots, across, down, cells = geometry.slots, geometry.across, geometry.down, geometry.partition()

        def cell(page: int) -> Box:
            slot, col, row = page % slots, (page % slots) % across, (page % slots) // across
            mirrored = (across - 1 - col) + across * row if on_across else col + across * (down - 1 - row)
            return cells[mirrored if (page // slots) % 2 else slot]

        return Block.of_seq(Placement(source=page, sheet=page // slots, cell=cell(page)) for page in range(pages))

    return place


def _paired(pages: int, geometry: Geometry) -> Block[Placement]:
    cells = geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=page, cell=cells[slot]) for page in range(pages) for slot in range(geometry.slots))


def _stacked(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    stack = -(-pages // slots)
    return Block.of_seq(Placement(source=page, sheet=page % stack, cell=cells[page // stack]) for page in range(pages))


def _split(pages: int, geometry: Geometry) -> Block[Placement]:
    # sheetwise: recto (even) and verso (odd) pages stream to SEPARATE plates by parity (`page // 2` the
    # per-parity index), each plate packed n-up — distinct from the mirrored `_duplexed` turn/tumble and
    # from the sequential `_grid`; the plates print independently, so no cell mirror.
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(Placement(source=page, sheet=2 * (page // 2 // slots) + page % 2, cell=cells[page // 2 % slots]) for page in range(pages))


# perfect-bind is the spine-offset n-up sequence: the `Geometry.spine` field already widens the
# bind-edge origin in `partition`, so it is `_grid` over a spine-carrying geometry, not a degenerate
# one-page-per-sheet body. The two schemes differ by a `Geometry` field, never a parallel function.
PLANS: Final[frozendict[Scheme, Place]] = frozendict({
    Scheme.NUP: _grid,
    Scheme.BOOKLET: lambda pages, geo: _folded(pages, structs.replace(geo, leaves=1)),
    Scheme.SIGNATURE: _folded,
    Scheme.WORK_AND_TURN: _duplexed(on_across=True),
    Scheme.WORK_AND_TUMBLE: _duplexed(on_across=False),
    Scheme.CUT_AND_STACK: _stacked,
    Scheme.COME_AND_GO: _paired,
    Scheme.PERFECT_BIND: _grid,
    Scheme.SHEETWISE: _split,
})


@tagged_union(frozen=True)
class ImposeOp:  # the closed request vocabulary; the fault rail is BoundaryFault at the boundary
    tag: Literal["impose", "plan", "proof"] = tag()
    impose: tuple[bytes, Scheme, Geometry, Marks] = case()
    plan: tuple[bytes, Scheme, Geometry] = case()
    proof: tuple[bytes, float, int | tuple[int, ...], ProofPolicy] = case()

    @staticmethod
    def Impose(source: bytes, scheme: Scheme = Scheme.NUP, geometry: Geometry = Geometry(), marks: Marks = Marks()) -> "ImposeOp":
        return ImposeOp(impose=(source, scheme, geometry, marks))

    @staticmethod
    def Plan(source: bytes, scheme: Scheme, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(plan=(source, scheme, geometry))

    @staticmethod
    def Proof(
        source: bytes, dpi: float = 96.0, sheet: int | tuple[int, ...] = 0, policy: ProofPolicy = ProofPolicy()
    ) -> "ImposeOp":  # an `int` proofs one imposed sheet, a tuple a contact strip of those sheets (empty = every sheet); `policy` carries the RGB/CMYK ink, raster codec, clip, and tint/negative finish
        return ImposeOp(proof=(source, dpi, sheet, policy))


# --- [COMPOSITION] ----------------------------------------------------------------------
class Imposition(Struct, frozen=True):
    op: ImposeOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"impose.{self.op.tag}", self._keyed, catch=_FAULTS)

    async def _keyed(self) -> ContentKey:
        # the `_composed` MuPDF fold is a GIL-releasing native render — it offloads to the bounded
        # `to_thread` band so the imposition draw never blocks the loop, the same fold `contribute`
        # re-enters synchronously off the loop; the engine raise rides the `to_thread` await into `_FAULTS`.
        composed = await to_thread.run_sync(_composed, self.op, limiter=_GATE)
        return ContentIdentity.key(
            f"impose-{self.op.tag}", composed.data
        )  # bare synchronous accessor: the imposed bytes are an infallible whole-byte source, so `_keyed` returns a bare `ContentKey`, never the railed `of`

    def planned(self) -> Option[ImposedPlan]:
        match self.op:
            # both OR-alternatives must bind the SAME names — `marks` is irrelevant to the plan, so it is
            # the `_` wildcard (a `_marks` capture in only one alternative is a `SyntaxError`).
            case ImposeOp(tag="impose", impose=(source, scheme, geometry, _)) | ImposeOp(tag="plan", plan=(source, scheme, geometry)):
                return Some(_planned(source, scheme, geometry))
            case ImposeOp(tag="proof"):  # a proof op imposes no sheets — its pre-flight plan is a non-failing absence
                return Nothing
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> "Iterable[Receipt]":
        if self.op.tag in ("impose", "proof"):
            composed = _composed(self.op)
            key = ContentIdentity.key(f"impose-{self.op.tag}", composed.data)
            match composed.kind:
                case Artifact.PDF if composed.layers:  # the OCG-bearing imposed press form rides the minted-layer count on the Egress overlays slot
                    receipt = ArtifactReceipt.Egress(key, len(composed.data), composed.sheets, 0, 0, composed.layers)
                case Artifact.PDF:
                    receipt = ArtifactReceipt.Pdf(key, len(composed.data), composed.sheets)
                case Artifact.PREVIEW:
                    receipt = ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], composed.scores)
                case _:
                    assert_never(composed.kind)
            yield from receipt.contribute()

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


def _composed(op: ImposeOp) -> Composed:  # the one pure render fold both `of` and `contribute` read
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, marks)):
            # a provider-native scheme (no `PLANS` row: wire/hardcover/cards/zine) OR an explicit PDFIMPOSE
            # engine routes to the provider fold; every locally-placeable scheme draws through show_pdf_page.
            if geometry.engine is ImpositionEngine.PDFIMPOSE or scheme not in PLANS:
                return _pdfimposed(source, scheme, geometry, marks)
            with pymupdf.open(
                stream=source, filetype="pdf"
            ) as src:  # the native source handle closes deterministically once the imposed bytes are folded
                return _imposed(src, geometry, marks, PLANS[scheme](src.page_count, geometry))
        case ImposeOp(tag="plan", plan=(source, scheme, geometry)):
            plan = _planned(source, scheme, geometry)
            return Composed(data=json.encode(plan), sheets=plan.sheets)
        case ImposeOp(tag="proof", proof=(source, dpi, sheet, policy)):
            with pymupdf.open(stream=source, filetype="pdf") as src:  # the native source handle closes once the proof pixmap bytes are read
                pixmap = _contact(src, sheet, int(dpi), policy) if isinstance(sheet, tuple) else _rasterized(src[sheet], int(dpi), policy)
                blob, gamut = _proofed(pixmap, policy)  # ICC soft-proof when policy.proof is set (out-of-gamut count), else the raw device raster
                scores = frozendict({"gamut": float(gamut)}) if policy.proof is not None else frozendict()
                return Composed(blob, sheets=1, kind=Artifact.PREVIEW, extent=(pixmap.width, pixmap.height), scores=scores)
        case _:
            assert_never(op)


def _pdfimposed(source: bytes, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> Composed:
    schema = _PDFIMPOSE_SCHEMAS[scheme]
    sink = BytesIO()
    schema.impose((BytesIO(source),), sink, **_pdfimpose_kwargs(schema, scheme, geometry, marks))
    data = sink.getvalue()
    return Composed(data=data, sheets=_page_count(data))


def _pdfimpose_kwargs(schema: PdfImposeSchema, scheme: Scheme, geometry: Geometry, marks: Marks, /) -> dict[str, object]:
    # one candidate dict over the `Geometry`/`Marks` owners, filtered to exactly the kwargs the schema's
    # verified signature honors (`schema.accepts`) — so onepagezine never sees `signature`/`imargin`, cards
    # never sees `last`, and wire never sees `bind`, each of which its `impose(...)` would reject as a TypeError.
    crop_marks = "crop" in marks.controls or marks.overlay
    candidate: dict[str, object] = {
        "signature": (geometry.across, geometry.down),
        "imargin": geometry.gutter,
        "omargin": geometry.omargin,
        "mark": ["crop"] if crop_marks else [],
        "bind": geometry.binding,
        "creep": lambda sheets: geometry.creep * max(sheets - 1, 0),
        "group": 1 if scheme is Scheme.BOOKLET else geometry.leaves,
        "last": geometry.last,
        "back": geometry.back,
    }
    return {name: value for name, value in candidate.items() if name in schema.accepts}


def _imposed(src: "Document", geometry: Geometry, marks: Marks, placements: Block[Placement], /) -> Composed:
    # the live native document the engine grows in place is the platform-forced statement seam, BRACKETED
    # by `with` so the imposed handle closes on every exit (success, fault, cancellation); the `Geometry`
    # divisor invariant is already admitted by `partition`'s `_admit` guard inside the placement resolution
    # above, so a malformed grid count has railed as `api` before any page is allocated here.
    sheets = _sheet_count(placements)
    width, height = geometry.oriented
    with pymupdf.open() as out:
        for _ in range(sheets):
            out.new_page(width=width, height=height)
        groups = _mint_groups(out, placements)  # one shared OCG per unique signature-group name (dedupes the flat `sig-N` duplicates)
        minted = placements.map(lambda p: _draw_one(out, src, p, groups)).choose(
            _oc_state
        )  # live handle; each placement OCMD-nested under its signature group
        _configure_layers(out, minted)  # one ui-config write driving reader visibility/lock over the deduped shared groups
        marks.finished(out, geometry, sheets)
        _press_marks(out, geometry, marks.press)  # draw the LOCAL crop/fold/registration/colour-bar marks at the imposed-cell/sheet boundaries
        return Composed(data=marks.serialize(out), sheets=out.page_count, layers=len(groups))


def _mint_groups(out: "Document", placements: Block[Placement], /) -> "frozendict[str, int]":
    # mint ONE shared OCG per unique signature-group name across the placements — a signature is one reader-panel
    # toggle its N placements are members of via their OCMD, never a per-placement duplicate; a nameless (NUP/duplex)
    # placement contributes no group, so the mint is empty and the degenerate `Pdf` receipt form selects. The
    # sibling `composition/sheet#SHEET` `_mint_groups` mints the same shared-group family for its membership hierarchy.
    names = tuple(dict.fromkeys(p.name for p in placements if p.name))
    return frozendict({name: out.add_ocg(name, on=True, intent="View", usage="Artwork") for name in names})


def _draw_one(
    out: "Document", src: "Document", p: Placement, groups: "frozendict[str, int]"
) -> tuple[int, bool, bool]:  # (shared group xref, visible, locked); 0 if unlayered
    # each name-bearing placement is a MEMBER of its shared signature-group OCG through an OCMD membership
    # dictionary (`set_ocmd`, the `composition/sheet#SHEET` nested-layer pattern) — a reader toggles the whole
    # signature — rather than N flat duplicate `add_ocg` groups all labelled `sig-N`; the shared group xref rides `set_layer`.
    group = groups.get(p.name, 0) if p.name else 0
    oc = out.set_ocmd(ocgs=[group], policy=p.policy.value) if group else 0
    out[p.sheet].show_pdf_page(  # index the live doc; a held Page list outliving `out` faults on draw
        pymupdf.Rect(*p.cell),
        src,
        pno=p.source,
        keep_proportion=p.fit,
        overlay=p.overlay,
        rotate=p.rotate,
        clip=pymupdf.Rect(*p.clip) if p.clip is not None else None,
        oc=oc,
    )
    return group, p.visible, p.locked


def _oc_state(drawn: tuple[int, bool, bool], /) -> Option[tuple[int, bool, bool]]:  # keep only the rows that bound a real OCG group
    return Some(drawn) if drawn[0] else Nothing


def _configure_layers(out: "Document", minted: Block[tuple[int, bool, bool]], /) -> None:
    # one `set_layer` ui-config write over the DEDUPED shared signature-group OCGs (each placement returns its group,
    # so the set collapses the members) — the reader's panel toggles the signature `on`/`off` and honors `locked`,
    # exactly as the sibling `composition/sheet#SHEET` `_configure_layers` drives the shared groups; a group is off
    # only when every member placement is hidden.
    if not minted.is_empty():
        groups = {xref for xref, _visible, _locked in minted}
        hidden = groups - {xref for xref, visible, _locked in minted if visible}
        out.set_layer(0, on=list(groups - hidden), off=list(hidden), locked=list({xref for xref, _visible, locked in minted if locked}))


def _press_marks(out: "Document", geometry: Geometry, marks: tuple[PressMark, ...], /) -> None:
    # the LOCAL press-form printer's marks the imposed-sheet draw lacked — crop/fold/registration marks and a colour
    # control bar drawn at the computed imposed-cell rects (`geometry.partition()`) and sheet margins via `Page.new_shape`
    # + `draw_line`/`draw_rect`/`draw_circle` + `Shape.finish`/`commit`, the live-native-handle statement seam boundaries.md
    # names. This is DISTINCT from `Marks.overlay`, which routes a FIGURE-placement overlay SHAPE to composition/compose#COMPOSE
    # (which sees figure placement, not the press-form cell grid) — the compose seam stays for the figure overlay, this local
    # draw ADDS the press-mark set. `pdfimpose` honors only `mark=['crop']`, so a provider-native scheme stays crop-only.
    if not marks:
        return
    width, height = geometry.oriented
    cells = geometry.partition()
    for page in out:  # each imposed sheet — the live native page the shape commits onto
        shape = page.new_shape()
        for mark in marks:
            match mark:
                case PressMark.CROP:
                    _crop_marks(shape, cells)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.4)
                case PressMark.FOLD:
                    _fold_marks(shape, cells, height, geometry)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.3, dashes="[2 2] 0")
                case PressMark.REGISTRATION:
                    _registration_marks(shape, width, height, geometry.omargin or 12.0)
                    shape.finish(color=(0.0, 0.0, 0.0), width=0.4)
                case PressMark.COLOR_BAR:
                    _color_bar(shape, height, geometry.omargin or 12.0)  # each patch its OWN fill finish inside — no outer finish
                case _ as unreachable:
                    assert_never(unreachable)
        shape.commit()


def _crop_marks(shape: "Shape", cells: tuple[Box, ...], /) -> None:
    # L-shaped trim ticks just outside each imposed-cell corner — the bindery's guillotine cut guides.
    gap, tick = 3.0, 9.0
    for x0, y0, x1, y1 in cells:
        for cx, cy, sx, sy in ((x0, y0, -1.0, -1.0), (x1, y0, 1.0, -1.0), (x0, y1, -1.0, 1.0), (x1, y1, 1.0, 1.0)):
            shape.draw_line(pymupdf.Point(cx + sx * gap, cy), pymupdf.Point(cx + sx * (gap + tick), cy))
            shape.draw_line(pymupdf.Point(cx, cy + sy * gap), pymupdf.Point(cx, cy + sy * (gap + tick)))


def _fold_marks(shape: "Shape", cells: tuple[Box, ...], height: float, geometry: Geometry, /) -> None:
    # dashed fold lines down the inter-column gutter midpoints — the sheet fold axes a signature folds along.
    for col in range(1, geometry.across):
        x = (cells[col - 1][2] + cells[col][0]) / 2.0
        shape.draw_line(pymupdf.Point(x, 0.0), pymupdf.Point(x, height))


def _registration_marks(shape: "Shape", width: float, height: float, inset: float, /) -> None:
    # concentric registration targets + crosshairs at the four sheet-edge midpoints — the multi-plate press alignment.
    radius, arm = 6.0, 9.0
    for cx, cy in ((width / 2.0, inset / 2.0), (width / 2.0, height - inset / 2.0), (inset / 2.0, height / 2.0), (width - inset / 2.0, height / 2.0)):
        shape.draw_circle(pymupdf.Point(cx, cy), radius)
        shape.draw_circle(pymupdf.Point(cx, cy), radius / 2.0)
        shape.draw_line(pymupdf.Point(cx - arm, cy), pymupdf.Point(cx + arm, cy))
        shape.draw_line(pymupdf.Point(cx, cy - arm), pymupdf.Point(cx, cy + arm))


def _color_bar(shape: "Shape", height: float, inset: float, /) -> None:
    # a CMYK + gray control-patch row along the foot margin — each `_BAR_PATCHES` patch its OWN `draw_rect` + `finish(fill=)`
    # group (a shared finish cannot carry per-patch fills), the densitometer strip a press operator reads against a proof.
    size = 10.0
    for index, rgb in enumerate(_BAR_PATCHES):
        x = inset + index * size
        shape.draw_rect(pymupdf.Rect(x, height - inset - size, x + size, height - inset))
        shape.finish(color=(0.0, 0.0, 0.0), fill=rgb, width=0.2)


def _colorspace(ink: ProofInk, /) -> "Colorspace":
    # the pymupdf colorspace singleton resolves at call time (the lazy `pymupdf` proxy is already reified on
    # the worker band the fold runs in), never a module-level table that would reify the native import early.
    match ink:
        case ProofInk.RGB:
            return pymupdf.csRGB
        case ProofInk.CMYK:
            return pymupdf.csCMYK
        case ProofInk.GRAY:
            return pymupdf.csGRAY
        case _ as unreachable:
            assert_never(unreachable)


def _rasterized(page: "Page", dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    # the press-faithful proof rasterizer: `get_pixmap(colorspace=)` renders the ink model a bindery reads
    # (RGB screen, CMYK separations, GRAY density), `clip` proofs a trim/bleed sub-region, then the in-place
    # registration-tint / dot-gain gamma / film-negative finish rides the pixmap before egress — gamma
    # before the invert so the tone shift lands on the positive separations the film negative then mirrors.
    pixmap = page.get_pixmap(dpi=dpi, colorspace=_colorspace(policy.ink), clip=pymupdf.Rect(*policy.clip) if policy.clip is not None else None)
    if policy.tint is not None:
        pixmap.tint_with(policy.tint[0], policy.tint[1])
    if policy.gamma is not None:
        pixmap.gamma_with(policy.gamma)
    if policy.negative:
        pixmap.invert_irect()
    return pixmap


def _encoded(pixmap: "Pixmap", raster: ProofRaster, /) -> bytes:
    # native `tobytes` covers PNG (RGB/GRAY) and PSD (CMYK-capable separations); the `pil_tobytes` bridge
    # covers the WEBP/AVIF/TIFF formats MuPDF's native encoder lacks — one codec split keyed by the member.
    return pixmap.pil_tobytes(raster.value) if raster in _PIL_RASTERS else pixmap.tobytes(raster.value)


def _proofed(pixmap: "Pixmap", policy: ProofPolicy, /) -> tuple[bytes, int]:
    # no output-intent profile -> the raw device raster (`_encoded`), zero out-of-gamut; a profile -> the ICC soft-proof.
    return _softproof(pixmap, policy.proof, policy.raster) if policy.proof is not None else (_encoded(pixmap, policy.raster), 0)


def _softproof(pixmap: "Pixmap", profile: bytes, raster: ProofRaster, /) -> tuple[bytes, int]:
    # the ICC-managed press-form separations proof the RAW `get_pixmap(csCMYK)` device conversion cannot warn on:
    # bridge the imposed-sheet pixmap to a PIL Image (`pil_tobytes` -> `Image.open`), build the proof transform (working
    # sRGB reference input+display, `profile` the press OUTPUT-INTENT), apply once under `Flags.SOFTPROOFING` and once
    # under `SOFTPROOFING | GAMUTCHECK`, and count the differing pixels the gamut-check alarm paints — the out-of-press-gamut
    # set. The OUTPUT-INTENT profile arrives via the graphic/color/managed#MANAGED seam (colour-cxf CxF3 + ImageCms owner),
    # never a direct colour-cxf import here; the shared pillow ImageCms lcms2 lane is composed for the imposition preflight.
    origin = PilImage.open(BytesIO(pixmap.pil_tobytes("png"))).convert("RGB")
    with NamedTemporaryFile(suffix=".icc", delete_on_close=False) as handle:  # the press profile must outlive the lazy transform build
        handle.write(profile)
        handle.close()
        reference = ImageCms.createProfile("sRGB")
        keys = {"renderingIntent": ImageCms.Intent.RELATIVE_COLORIMETRIC, "proofRenderingIntent": ImageCms.Intent.ABSOLUTE_COLORIMETRIC}
        proofed = ImageCms.applyTransform(
            origin, ImageCms.buildProofTransform(reference, reference, handle.name, "RGB", "RGB", flags=ImageCms.Flags.SOFTPROOFING, **keys)
        )
        alarmed = ImageCms.applyTransform(
            origin,
            ImageCms.buildProofTransform(
                reference, reference, handle.name, "RGB", "RGB", flags=ImageCms.Flags.SOFTPROOFING | ImageCms.Flags.GAMUTCHECK, **keys
            ),
        )
    gamut = ImageChops.difference(proofed, alarmed).convert("L").point(lambda level: 255 if level else 0).histogram()[255]
    sink = BytesIO()
    alarmed.save(
        sink, format=raster.value if raster in _PIL_RASTERS else "PNG"
    )  # the soft-proof simulation is RGB, so PSD/native ProofRaster falls to PNG
    return sink.getvalue(), gamut


def _contact(src: "Document", sheets: tuple[int, ...], dpi: int, policy: ProofPolicy, /) -> "Pixmap":
    # the multi-sheet contact strip: lay each selected imposed sheet (every sheet when the tuple is empty)
    # row-major into one montage page through the same `show_pdf_page` cross-document draw the imposition
    # owns, then rasterize once through the same `_rasterized` press-proof path so a whole signature set
    # proofs in one CMYK/tinted image; the montage cell is the imposed-sheet rect and `dpi` sizes the strip.
    pages = sheets or tuple(range(src.page_count))
    columns, cell = math.isqrt(len(pages) - 1) + 1, src[pages[0]].rect
    with pymupdf.open() as montage:  # the montage handle closes once the strip pixmap is rendered off it
        montage.new_page(width=cell.width * columns, height=cell.height * -(-len(pages) // columns))
        for slot, index in enumerate(pages):  # index the live montage page; a held Page outliving `montage` faults on draw
            montage[0].show_pdf_page(
                pymupdf.Rect(
                    slot % columns * cell.width, slot // columns * cell.height, (slot % columns + 1) * cell.width, (slot // columns + 1) * cell.height
                ),
                src,
                pno=index,
            )
        return _rasterized(montage[0], dpi, policy)


def _planned(source: bytes, scheme: Scheme, geometry: Geometry) -> ImposedPlan:
    with pymupdf.open(stream=source, filetype="pdf") as src:  # only the page count is read off the source handle, which then closes
        pages = src.page_count
    # a provider-native scheme (no `PLANS` row) has no local placement model — its fold geometry is
    # pdfimpose's, so the pre-flight carries scheme/pages/sheet-size/engine with empty placements (a press
    # operator still validates the request) rather than a fabricated local stream that would diverge from
    # the provider's actual imposition; the local schemes compute the full placement pre-flight as before.
    local = scheme in PLANS
    placements = PLANS[scheme](pages, geometry) if local else Block.empty()
    sheets, folded = _sheet_count(placements), scheme in (Scheme.BOOKLET, Scheme.SIGNATURE)
    leaves = 1 if scheme is Scheme.BOOKLET else geometry.leaves  # the booklet row forces leaves=1
    fold = 4 * max(leaves, 1)  # the effective signature size the placement folded against
    return ImposedPlan(
        scheme=scheme,
        sheet=geometry.oriented,
        sheets=sheets,
        pages=pages,
        leaves=leaves,
        signatures=-(-pages // fold) if folded else sheets,
        padded=(-pages % fold) if folded else 0,
        creep=geometry.creep * max(-(-pages // fold) - 1, 0) if folded else 0.0,
        placements=tuple(placements),
        engine=geometry.engine if local else ImpositionEngine.PDFIMPOSE,
    )


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(op: ImposeOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    # the imposed press form projects outward as one named row per OCG-bound signature group the
    # `export/layered#LAYERED` owner binds into OCG/SVG layers — the row carries the union cell box
    # over that group's placements and the shared source; no frame re-render, no per-placement duplicate.
    # Only the LOCAL show_pdf_page arm mints per-signature OCGs, so a provider-native scheme (no `PLANS`
    # row) or an explicit PDFIMPOSE engine — whose imposed bytes carry no local OCG rows — projects nothing.
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, _)) if scheme in PLANS and geometry.engine is not ImpositionEngine.PDFIMPOSE:
            placements = PLANS[scheme](_page_count(source), geometry)
            boxes = placements.fold(
                lambda acc, p: acc.add(p.name, _union(acc.try_find(p.name).default_value(p.cell), p.cell)) if p.name else acc, Map.empty()
            )
            # signature order is the placement order, never `Map.items()` AVL key-sort (which orders `sig-10`
            # before `sig-2`); `dict.fromkeys` dedupes the ordered name stream so the `names` index aligns.
            ordered = tuple(dict.fromkeys(p.name for p in placements if p.name))
            return tuple(Layer(names[index] if index < len(names) else name, source, boxes[name]) for index, name in enumerate(ordered))
        case _:
            return ()


def _union(left: Box, right: Box) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


def _page_count(source: bytes) -> int:
    with pymupdf.open(stream=source, filetype="pdf") as doc:  # the count is read off the handle, which then closes
        return doc.page_count
```

## [03]-[RESEARCH]

- [PDFIMPOSE_ENGINE_SETTLED]: `pdfimpose` is admitted as a provider-contained engine row, not a second imposition owner. All seven of the package's schema wrappers share the stable public shape `impose(files: Sequence[str | pathlib.Path | io.BytesIO], output: str | pathlib.Path | io.BytesIO, *, ...)` but differ in the optional-kwarg set each honors, so `_PDFIMPOSE_SCHEMAS` binds each local `Scheme` to a `PdfImposeSchema(impose, accepts)` row whose `accepts` frozenset is EXACTLY that schema's verified kwarg set: `saddle.impose` (BOOKLET group=1 / SIGNATURE group=leaves), `cutstackfold.impose` (CUT_AND_STACK), and `copycutfold.impose` (COME_AND_GO) carry the full `_FOLD_KW` set `{signature, imargin, omargin, mark, bind, creep, group, last}`; `hardcover.impose` (HARDCOVER) is `_FOLD_KW - {creep}`; `wire.impose` (WIRE) `{signature, imargin, omargin, mark, last}` (no bind/creep/group); `cards.impose` (CARDS) `{signature, imargin, omargin, mark, bind, back}` (no creep/group/last, plus the `back=` verso-order token from `Geometry.back`); `onepagezine.impose` (ZINE) `{omargin, mark, bind, last}` (fixed 2x4 fold, no signature/imargin). `_pdfimpose_kwargs` computes ONE candidate dict from `Geometry`/`Marks` — `signature=(across, down)`, `imargin=gutter`, `omargin=geometry.omargin` (no longer hardcoded `0`), `bind=binding`, `creep=<closure>`, `group`, `last=geometry.last`, `back=geometry.back`, crop marks from `Marks` — then filters it to `schema.accepts`, so a schema never receives a kwarg its signature would reject as a `TypeError`. The `Impose` arm routes to `_pdfimposed` when `geometry.engine is ImpositionEngine.PDFIMPOSE` (a dual-capable scheme's provider override) OR when `scheme not in PLANS` (a provider-native `WIRE`/`HARDCOVER`/`CARDS`/`ZINE`, whose fold geometry has no local `show_pdf_page` model), reading only imposed PDF bytes back into `Composed`. Provider objects, `Matrix`, `Page`, `Margins`, and schema names never cross the owner boundary; local `ImposedPlan`, `Placement`, `Layer`, and `ArtifactReceipt` remain the only downstream facts. `PERFECT_BIND` is the inverse — local-only, because the provider exposes no `perfect.impose` schema — so under `PDFIMPOSE` it raises the `_PDFIMPOSE_SCHEMAS` `KeyError` the boundary catches, preferable to silently falling back to local placement and hiding the requested provider.
- [SCHEME_PLACEMENT_SETTLED]: the placement computation for each scheme is one `Place` row in the `frozendict[Scheme, Place]` policy table returning a `Block[Placement]`, the totality proof — `_grid` (document-order sequence to the next binding-aware cell, the `NUP` row AND the `PERFECT_BIND` row since perfect-bind is `_grid` over a `Geometry` carrying a `spine` allowance, two schemes differing by a field not a function), `_folded` (the centerfold computation padding the page count to the next multiple of `4·leaves`, folding the `_saddle` pairing `(last, first, second, second-last, ...)` so the bound stack reads in sequence, carrying the per-row 180° head-to-head rotation `180·((slot % slots) // across) % 360`, the per-fold `Geometry.creep` outward shift to compensate fold-thickness drift, and the `sig-N` OCG layer name per signature group, the `BOOKLET` row a `leaves=1` `structs.replace` over `_folded` and the `SIGNATURE` row `_folded` direct over `Geometry.leaves`), `_duplexed(on_across)` (work-and-turn AND work-and-tumble single-plate duplex — both halves on one sheet, the back side mirrored across the row for the turn and down the column for the tumble, the two schemes one parameterized body differing only by the `on_across` axis policy value, never two parallel functions), `_stacked` (cut-and-stack — sequential pages distributed down each guillotine-cut pile), `_paired` (come-and-go — each page duplicated across the sheet's slots so a single set tumbles into two finished copies), `_split` (sheetwise — recto (even) and verso (odd) pages streamed to separate plates by parity, each plate packed n-up: `sheet = 2·((page // 2) // slots) + page % 2`, `cell = cells[(page // 2) % slots]`, distinct from the mirrored `_duplexed` turn/tumble and the sequential `_grid`). Each scheme returns the full `Placement` `Block` directly — source, sheet, cell, rotate, fit, clip, name, visible, locked fused — so the draw fold reads one `Block`, never a second order/cell/rotate join, and the imposed-sheet count derives from `_sheet_count(placements)` (the `Block.fold` max over `sheet` + 1), structurally unable to drift from the placement. The `Geometry.partition(shift)` projection is the shared sheet-geometry surface every locally-placeable scheme reads with every field live: `oriented` swaps the dimensions on `landscape`, `binding` selects the gutter edge and the creep sign, `omargin` insets the whole cell grid by the outer/trim margin on every edge, `spine` widens the bind-edge origin for perfect-bind glue, `head_trim` insets the top/bottom finished trim, `creep` shifts the cell grid per fold, `bleed` expands the cell by the trim margin, `gutter` spaces the cells. The provider-native `WIRE`/`HARDCOVER`/`CARDS`/`ZINE` schemes carry NO `PLANS` row — their fold geometry (wire spine, sewn signatures, front/back cards, the fixed 2x4 zine fold) is `pdfimpose`'s and has no local `show_pdf_page` model, so `scheme in PLANS` is the locally-placeable discriminant `_composed`/`_planned`/`_placed_layers` each read to route the provider-native schemes to `_pdfimposed` and project no local placement/plan/layer for them, exactly as `PERFECT_BIND` is the inverse (local-only, no provider schema). The split is the disciplined collapse: a per-scheme `ImposeOp` case differing only by the literal scheme and a `leaves` knob, a parallel `_impose_nup`/`_impose_booklet`/`_impose_signature` method triple beside the one `PLANS`-keyed placement fold, a degenerate one-page-per-sheet `_flat` beside the `_grid` perfect-bind row, a separate `sheets` callable per scheme that drifts from the placement, and an `if scheme == ...` cascade beside the table dispatch are the rejected forms.
- [RAIL_NARROWING_SETTLED]: the fault rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` minted ONCE at `async_boundary(f"impose.{op.tag}", self._keyed, catch=_FAULTS)` where `_FAULTS = (RuntimeError, ValueError, KeyError, IndexError, OSError, BeartypeCallHintViolation, PdfImposeUserError)` is the real engine raise tuple — `pymupdf.FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or zero-page source, a `PLANS`/`_PDFIMPOSE_SCHEMAS` `KeyError`, a malformed `Rect(*cell)` extent or off-axis-rotation `ValueError`, an out-of-range `Proof` sheet-index `IndexError` (the one arm indexing the document by a caller-supplied page the per-sheet siblings never expose), a non-positive-grid / negative-extent `BeartypeCallHintViolation` from the `@_GUARD`-contracted `_admit` scalar seam `Geometry.partition` calls, a `pdfimpose.UserError` provider-schema fault, and the source stream `OSError` — so the `runtime/faults#FAULTS` `async_boundary[T](subject, thunk, *, catch=...)` `_convert` folds each caught cause through `BoundaryFault.of(subject, cause)` into its own structurally-addressable case (the `BeartypeCallHintViolation` landing as `api`, `TimeoutError`-derived `OSError` ordering owned by the `CLASSIFY` table, the residual `RuntimeError`/`ValueError`/`KeyError`/`IndexError`/`PdfImposeUserError` as subject-carrying boundary faults), exactly as the sibling `composition/sheet#SHEET` and `composition/compose#COMPOSE` arms do, rather than the `Exception` catch-all the faults owner rejects. The prior fence's parallel `ImposeFault` `Literal` rail and its `_aspected`/`_contracted` weave were the deleted illusion: the interior returned `Result[Composed, ImposeFault]` the boundary never read, and `_keyed` then `raise ValueError(keyed.error)` round-tripped a railed fault back into a raise the default `catch=Exception` re-collapsed — the rail-to-raise-to-rail erasure the `rails-and-effects.md` boundary law forbids. The `_keyed` thunk offloads `_composed(op)` to the bounded `to_thread` band and mints `ContentIdentity.key(...)` over its `.data`; the interior raises the provider exception the boundary owns through the `to_thread` await, never carries a second rail. The `_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))` shape contract is applied to `_admit`, the scalar admission seam `Geometry.partition` calls at the head of the division: its signature carries `across`/`down` as `Across`, `leaves` as `Leaves`, and `gutter`/`head_trim`/`spine`/`creep`/`bleed` as `Span` — every one a DIRECT `Is`-refined scalar parameter — so beartype deep-checks each and rails a non-positive grid count or negative extent as the `api` `BoundaryFault` case BEFORE the divide, every refinement alias load-bearing rather than a decorative annotation msgspec silently ignores at struct construction. A `@_GUARD` on `_imposed(geometry: Geometry, …, placements: Block[Placement])` or on `_composed(op: ImposeOp)` would be the dead-arm illusion the page rejects: beartype checks only that `geometry` is a `Geometry`, `placements` a `Block`, and `op` an `ImposeOp` (the type-checker already proves each) and never recurses into a `msgspec.Struct` field, a `Block` element, or a `case()` tuple payload where the `Is`-refined `Across`/`Leaves`/`Span` scalars live, so its `BeartypeCallHintViolation` arm would be as structurally dead as the `Result[T, Never]` traverse the `[PLACEMENT_SWEEP_SETTLED]` row rejects — the contract must guard the seam where the refined shape is a real scalar parameter, and `Geometry.partition` calling `_admit` at the division site IS that seam, crossed on every impose, plan, planned, and layers path before the placement resolution evaluates as an argument to `_imposed`. The verso `Quarter` rotation the `_folded` fold computes is always a member of `(0, 90, 180, 270)` by construction, and an off-axis `rotate` reaching `show_pdf_page` is the `pymupdf` `ValueError` the `_FAULTS` tuple admits, so rotation is gated at the draw boundary rather than re-checked by a struct-field contract beartype cannot enforce.
- [PLACEMENT_SWEEP_SETTLED]: the draw is one `_mint_groups(out, placements)` shared-signature-group mint then `placements.map(lambda p: _draw_one(out, src, p, groups)).choose(_oc_state)` sweep over the `Block[Placement]` writing each `show_pdf_page` onto the live native `out[p.sheet]`, `_configure_layers(out, minted)` writing the one `set_layer(0, on=, off=, locked=)` reader config over the deduped shared groups, `_press_marks(out, geometry, marks.press)` drawing the local printer's marks, and `marks.serialize(out)` running once on the all-placed document — the live-native-handle mutation the `boundaries.md` platform-forced statement seam names, exactly as the sibling `composition/sheet#SHEET` `_composed` and `export/layered#LAYERED` `_pdf_ocg` arms run it. The prior fence's `expression.extra.result.traverse(lambda p: _draw_one(...), placements)` over a `_draw_one -> Result[Document, Never]` was the deleted illusory-density form: the `Never` error half can never materialize (the draw rails nothing of its own, the provider raise the boundary catches replaces a per-element `Result` thread), so the `traverse` threaded a fail-fast applicative whose failure branch was structurally dead — decorative rail ceremony deciding nothing, the exact `Result[T, Never]`-over-`traverse` form the sibling `composition/compose#COMPOSE` `[ONE_FOLD_SETTLED]` row rejects. The prior fence further split the page allocation into a statement `for _ in range(sheets): out.new_page(...)` loop beside the `traverse` draw — two contradictory treatments of the one live-handle seam; the rebuild is one consistent native-handle mutation for both the allocation and the draw. `_draw_one` indexes the LIVE `out[p.sheet]` (the held-`Page`-list lifetime fault the `[SHOW_PDF_PAGE_VERIFIED]` row names), binds a `name`-bearing placement to its shared signature-group OCG through a `set_ocmd(ocgs=[group], policy=p.policy)` membership dictionary (the `[OCMD_NESTED_LAYERS]` nesting, never N flat duplicate `add_ocg`), and returns its `(group, visible, locked)` row; `Block.choose(_oc_state)` keeps the rows that bound a real group and `_configure_layers` partitions the DEDUPED group set into the `set_layer` `on`/`off`/`locked` arrays exactly as the sibling `composition/sheet#SHEET` arm does — a minted OCG group with no reader visibility/lock config behind it is the capability-thin rejected form. The `Marks.finished(out, geometry, sheets)` press pass folds the finish flags (`set_toc`/`embfile_add`/`bake`/`scrub`/`rewrite_images`/`subset_fonts`/`set_metadata`/`set_xml_metadata`) over the one document after the placement sweep, then `_press_marks(out, geometry, marks.press)` draws the LOCAL crop/fold/registration/colour-bar marks at the imposed-cell/sheet boundaries ([PRESS_MARKS]), and `Marks.serialize(out)` runs the deterministic `tobytes(garbage=, deflate=, use_objstms=)` (`tobytes` is the in-memory egress; `ez_save`/`save` write a path) — the FIGURE-placement overlay SHAPE still routes to `composition/compose#COMPOSE` via the `overlay` flag, the press-form printer's marks drawn locally here.
- [PROOF_AND_LAYERS_SETTLED]: the `Proof(source, dpi, sheet, policy)` arm discriminates the `sheet` selector on input shape — an `int` runs `_rasterized` over that one imposed sheet, a tuple drives `_contact` to lay the selected sheets (every sheet when empty) row-major into one montage page through the same `show_pdf_page` cross-document draw and rasterize the contact strip once through the SAME `_rasterized` press path — both keyed by the same `ContentKey` with the `Pixmap.width`/`height` riding the `ArtifactReceipt.Preview` extent. `_rasterized` is the press-faithful path the reading map's `pymupdf` proof gap closes: `get_pixmap(dpi=, colorspace=_colorspace(policy.ink), clip=)` renders the `ProofPolicy` ink model — `csRGB` screen (default), `csCMYK` press-separations a bindery reads against its plates, or `csGRAY` single-ink density — over an optional trim/bleed `clip`, then the in-place `tint_with(black, white)` registration-tint / `gamma_with(gamma)` dot-gain gamma / `invert_irect()` film-negative finish, and `_encoded` writes the policy raster (native `Pixmap.tobytes` PNG/PSD or the `Pixmap.pil_tobytes` WEBP/AVIF/TIFF bridge for the formats MuPDF's native encoder lacks, CMYK pairing with a CMYK-capable PSD/TIFF). So a bindery proofs ANY single imposed sheet — recto, verso, or an inner signature page — IN CMYK SEPARATIONS, or a whole-signature contact strip, rather than re-imposing or only ever the first in RGB; the `_contact` montage grid columns derive from `math.isqrt` over the selected-sheet count (never a layout knob). A separate `Contact` op or factory beside the one `Proof` arm, a `columns` knob the sheet count already answers, and a hardcoded RGB `get_pixmap` that cannot separate a CMYK plate are the rejected forms. The `Imposition.layers(names) -> tuple[Layer, ...]` projection hands the imposed layout outward to `export/layered#LAYERED` exactly as `composition/sheet#SHEET` `Sheet.layers` does: one `Layer(name, source, bbox)` row per OCG-bound signature group (the `_folded` `sig-N` names deduped through a `Map.add` fold whose value is the `_union` of that group's cell boxes), each filling the REQUIRED `bbox` positional the `export/layered` `Layer(name, source, bbox, visible, locked)` row declares (the imposed signature groups uniformly visible and unlocked, the `visible`/`locked` defaults the export row carries). Only the LOCAL `show_pdf_page` arm mints per-signature OCGs, so `_placed_layers` guards on `scheme in PLANS and geometry.engine is not ImpositionEngine.PDFIMPOSE` and projects `()` for a provider imposition (whose opaque bytes carry no local OCG rows) rather than the phantom layer stream a `PLANS[scheme]` lookup for a provider-native scheme would `KeyError` on or a PDFIMPOSE dual scheme would falsely mint. A `Proof` `Pdf` receipt over raster bytes (a PDF evidence shape over a pixmap), a per-placement duplicate `Layer` row, a projected local layer row for a provider imposition that authored none, and a layered-export arm grafted onto `ImposeOp` are the rejected forms.
- [EGRESS_DISTINCT] [RESOLVED]: this owner is the DEDICATED multi-sheet press-imposition engine, distinct from the `document/egress#FINISH` `IMPOSE` step. The egress IMPOSE is the in-document `pypdf.Transformation`+`merge_page` n-up grid fold over an already-finished PDF as one of ten finishing steps (encrypt/outline/watermark/attach/impose/rewrite/redact/sanitize/optimize/protect) — it imposes a finished PDF into a simple n-up grid and computes no booklet, signature, work-and-turn, creep, or verso rotation. This `Imposition` owner computes the saddle-stitch creep-compensated centerfold order, the folded-signature ordering with head-to-head rotation, the work-and-turn duplexing, and the come-and-go / perfect-bind / sheetwise schemes the egress step never reaches, placing each source page through the higher-fidelity `pymupdf.show_pdf_page` cross-document draw. The two never overlap: egress finishes one PDF as a finishing step, imposition is the press-imposition engine producing a folded form. The split is the disciplined collapse boundary: a signature-imposition branch grafted onto the egress `IMPOSE` step (which owns no scheme dispatch and no placement computation) and an n-up grid re-implemented here beside the egress `Transformation` fold are the rejected crossings — the simple in-document n-up lands once in egress, the multi-sheet press-imposition lands once here, and neither re-owns the other. The imposed press form this owner emits is handed to `document` assembly keyed by the same `ContentKey`; the egress n-up is handed onward as a finished PDF — the two feeds are distinct.
- [RECEIPT_THREAD_SETTLED]: the `Impose` op contributes `core/receipt#RECEIPT` through the receipt owner's named flat-scalar mints via `Imposition.contribute`, which reads the one `_composed(op)` fold (no recompute — the same fold `of` drives), mints the content key over the imposed bytes through the same `ContentIdentity.key` `of` uses (so the projection is self-contained with no caller-passed key), routes on the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count, and yields `receipt.contribute()` — the OCG-bearing form `ArtifactReceipt.Egress(key, len(composed.data), composed.sheets, 0, 0, composed.layers)` carrying the imposed byte count, imposed-sheet page count, and the minted-OCG layer count on the `overlays` slot (zero encryption-R / outline-depth — imposition is neither a security nor a navigation close, the same `overlays` slot the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms report their authored-layer count on), and the degenerate `composed.layers == 0` no-mark form `ArtifactReceipt.Pdf(key, len(composed.data), composed.sheets)`. The `core/receipt#RECEIPT` owner's public surface IS these named per-kind static mints — each keyword-constructs its flat-scalar `case()` `(ContentKey, <scalars>)` — so a per-kind facts `Struct` (`EgressFacts`/`PdfFacts`/`PreviewFacts`) re-wrapping the scalars the named mint already takes positionally, and an `of(key, facts)` indirection over such a struct, are the exact forms the receipt owner's Boundary row rejects; the imposition owner calls `ArtifactReceipt.Egress(key, bytes, pages, …)` directly and adds no receipt case. The `Proof` op selects `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1], composed.scores)` carrying the pixmap extent plus the soft-proof out-of-gamut band. The `Plan` compute-only op contributes NO receipt — a `0`-sheet `Pdf` receipt over the `ImposedPlan` JSON bytes is the deleted phantom (a PDF evidence shape whose bytes are JSON, not a PDF); the pre-flight projection rides `Imposition.planned() -> Option[ImposedPlan]` (`Some` carrying the `scheme`/`sheets`/`pages`/`leaves`/`signatures`/`padded`/`creep`/placement-rows a press operator reads to validate the imposition before committing the draw, `Nothing` for a `Proof` op that imposes no sheets — never an `assert_never` over the reachable proof case), not a fabricated receipt.
- [ICC_SOFT_PROOF] [RESOLVED]: the `Proof` arm's `_rasterized` `get_pixmap(colorspace=csCMYK)` + `tint_with`/`gamma_with` was a raw DEVICE conversion that could apply no output-intent ICC and warn on no out-of-press-gamut ink — the exact `ImageCms` role the reading map assigns. `_proofed` now branches on `ProofPolicy.proof`: no profile -> the raw `_encoded` policy raster (unchanged); a profile -> `_softproof`, which bridges the imposed-sheet pixmap to a PIL Image via `Pixmap.pil_tobytes("png")` -> `Image.open`, builds the proof transform twice (`ImageCms.buildProofTransform(reference, reference, proof, "RGB", "RGB", renderingIntent=RELATIVE_COLORIMETRIC, proofRenderingIntent=ABSOLUTE_COLORIMETRIC, flags=SOFTPROOFING)` and again with `SOFTPROOFING | GAMUTCHECK`), `applyTransform`s both, and counts the differing pixels the gamut-check alarm paints through `ImageChops.difference(...).convert("L").point(...).histogram()[255]` (a pillow-only count, no numpy dependency in imposition) onto the `Preview` `scores` band, saving the gamut-warned RGB simulation as the proof raster. Every member verifies against the cp315 `pillow 12.2` wheel: `ImageCms.buildProofTransform`/`applyTransform`/`Intent.RELATIVE_COLORIMETRIC`/`ABSOLUTE_COLORIMETRIC`/`Flags.SOFTPROOFING`/`GAMUTCHECK`/`createProfile`, `Pixmap.pil_tobytes`/`samples`, `ImageChops.difference`. BOUNDARY: the output-intent/proof ICC profile arrives via the `graphic/color/managed#MANAGED` seam (which owns `colour-cxf` CxF3 + `ImageCms` per its `separations()`/`_softproof`), never a direct `colour-cxf` import in imposition — the shared pillow `ImageCms` lcms2 lane is composed for the imposition press-form preflight, the CxF/profile resolution the color owner's. Justified on PACKAGE (the verified pillow `ImageCms` soft-proof surface the reading map flagged) and DOMAIN (an ICC-managed CMYK-separations PDF/X press proof).
- [PRESS_MARKS] [RESOLVED]: the local `_imposed` arm drew NO printer's marks — `Marks.overlay` merely set a keyword flag deferring ALL mark SHAPE to `composition/compose#COMPOSE`, which operates on figure placement, not the imposed press-form cell grid, so the imposed sheet carried no crop/fold/registration marks. `_press_marks(out, geometry, marks.press)` now draws the LOCAL press-mark set at the computed imposed-cell rects (`geometry.partition()`) and sheet margins through the verified `Page.new_shape()` + `Shape.draw_line`/`draw_rect`/`draw_circle` + `Shape.finish(color=, fill=, width=, dashes=)`/`commit` (all present on the installed pymupdf) — `_crop_marks` L-shaped trim ticks at each cell corner, `_fold_marks` dashed fold lines down the inter-column gutters, `_registration_marks` concentric target + crosshair at each sheet-edge midpoint, `_color_bar` a CMYK+gray control-patch row (`_BAR_PATCHES`) each patch its own fill finish — a closed `PressMark` vocabulary on the `Marks.press` tuple. The `Marks.overlay` routing to `composition/compose#COMPOSE` is RETAINED for the figure-overlay SHAPE (a distinct seam), the local draw ADDING the press-form marks the compose owner never reaches; `pdfimpose` honors only `mark=['crop']`, so a provider-native scheme stays crop-only while the local `show_pdf_page` path carries the full mark set. Justified on PACKAGE (the verified pymupdf `Shape` draw surface the reading map flagged) and DOMAIN (a press-form crop/fold/registration/colour-bar set the bindery reads).
- [OCMD_NESTED_LAYERS] [RESOLVED]: the `_imposed` arm minted only FLAT per-signature `add_ocg` — N placements of one signature each minted a distinct OCG all labelled `sig-N`, with no membership dictionary, so the reader panel showed N duplicate `sig-N` toggles. `_mint_groups` now mints ONE shared OCG per unique signature-group `name` and `_draw_one` binds each member placement to it through a `set_ocmd(ocgs=[group], policy=p.policy.value)` OCMD membership dictionary (the verified `Document.set_ocmd` member, the `Membership` `AnyOn`/`AllOn` policy the `p.policy` carries), mirroring the sibling `composition/sheet#SHEET` `_mint_groups` + `set_ocmd` nested-layer pattern — a reader toggles the WHOLE signature as one nested layer, and `_configure_layers` drives the reader visibility/lock over the DEDUPED shared groups (a group off only when every member is hidden). `Composed.layers = len(groups)` carries the signature-group count on the `ArtifactReceipt.Egress` overlays slot (the shared-group count, not the flat-placement count), and `_placed_layers` groups the `export/layered#LAYERED` `Layer` rows by the same signature `name`. Justified on PACKAGE (the verified pymupdf `set_ocmd` OCMD surface) and DOMAIN (a nested per-signature reader-layer tree in the press proof).
