# [PY_ARTIFACTS_IMPOSITION]

The press-imposition owner laying emitted sheets into a press-ready imposed form. `Imposition` is ONE owner over the imposition pipeline carrying a closed-payload `ImposeOp` `expression.tagged_union` whose every case carries its own typed payload — `Impose` the drawing case carrying the `source`, `Scheme`, `Geometry`, and `Marks` it discriminates on, `Plan` the compute-only pre-flight case, `Proof` the raster-thumbnail case — never a per-scheme `Nup`/`Booklet`/`Signature` draw family differing only by a literal scheme and a `leaves` knob, and never a `StrEnum` keyed against an erased `dict[str, object]` bag. It reads an already-emitted multi-page PDF (the sheet set from `composition/sheet#SHEET`, the document body from `document/emit#DOCUMENT`) and re-orders, scales, rotates, and crops each source page onto a larger imposed sheet — n-up grid placement, saddle-stitch booklet pagination, folded-signature ordering, the work-and-turn / work-and-tumble single-plate duplex schemes, cut-and-stack, and the come-and-go / perfect-bind / sheetwise impositions — through `pymupdf.Page.show_pdf_page(rect, docsrc, pno=, keep_proportion=, overlay=, oc=, rotate=, clip=)`, the cross-document vector copy that draws each source page into its imposed-sheet cell at the page-order, head-to-head verso rotation, scale, and bleed clip the scheme computes. `Scheme` is the closed `StrEnum` over the imposition schemes, each a `PLANS` policy-table row binding ONE `place(pages, geometry)` placement computation that fuses page order, recto/verso rotation, and per-sheet creep shift, so a new scheme is one member plus one row, never a parallel imposer class; the imposed-sheet count is DERIVED from that one placement `Block` (`_sheet_count` the `Block.fold` max over each `Placement.sheet` plus one), never a second parallel formula that drifts from the placement. `Placement` is the closed-payload value object each computed placement carries — source page index, target imposed-sheet index, target cell rect, the `Quarter`-refined verso-rotation axis, the fit-scale and over/under-draw flags, OCG layer name, the OCG default-visibility and reader-lock state, and bleed clip — so the imposition fold reads every placement field straight onto the `show_pdf_page` keyword set over the one live native page, then a single `Document.set_layer(0, on=, off=, locked=)` ui-config write drives each minted signature group's reader-panel toggle and lock state exactly as the sibling `composition/sheet#SHEET` `_configure_layers` does, never a per-scheme mutable draw loop, never a hard-coded `rotate=0`, and never a minted OCG with no reader visibility/lock config behind it. `Geometry` is the row-policy value object the imposition carries — the imposed-sheet size and orientation, the `Across`-refined n-across/n-down grid, the binding-edge gutter, the head/foot trim, the spine allowance, the `Leaves`-refined fold/signature leaf count, the per-signature creep extent and its push-out / shingle-in direction, the lay feed edge and its reserved gripper claw margin, and the bleed/trim margin — projecting once through `partition(shift)` to the binding-and-gripper-aware cell-rect grid every scheme's placement list reads, every field live in that projection, the load-bearing divisor invariants `Is`-refined so a non-positive grid count rails as `api` at admission rather than dividing by zero in the fold. `Marks` is the press-finishing policy the imposed document carries — the registration-mark / crop-mark / colour-bar overlay route, the print-control marks the press needs, the per-signature imposition-map navigation outline, the cut-list / fold-map embedded press file, the interactive-layer bake, font subsetting, image recompression, hidden-content scrub, the optional-content layer config, the deterministic linearized save, and the press info/XMP metadata — so a press-ready imposed PDF is one finishing pass, never a parallel sheet re-author. One imposition surface discriminating the operation, not a per-scheme imposer family. `pymupdf` resolves on the cp315 core (native MuPDF cp310-abi3), so the imposition arm resolves in-process and imports its engine once at boundary scope inside the fold body. The imposed sheets the fold produces are handed onward to `document` assembly (the imposed body is the document the assembler collates), so this owner computes the imposition and places the pages but assembles no document; it projects the imposed layout outward as `Imposition.layers -> tuple[Layer, ...]` (one row per OCG-bound placement) to `export/layered#LAYERED` exactly as the sibling `composition/sheet#SHEET` does, so the named-layer authoring lands once in the export owner and this owner projects the rows. This owner is the DEDICATED booklet/signature press-imposition engine distinct from the simple `document/egress#FINISH` `IMPOSE` step, which is the in-document `pypdf.Transformation` n-up grid fold over an already-finished PDF — egress imposes a finished PDF as one finishing step among ten, while this owner computes the saddle-stitch creep-compensated page order, the folded-signature ordering, and the work-and-turn duplexing the egress step never reaches. The fault rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(subject, thunk, catch=_FAULTS)` over the real engine raise tuple `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` — admitting `pymupdf.FileDataError`/`EmptyFileError` (`RuntimeError`-derived), a malformed `Rect(*cell)` extent `ValueError`, a non-positive-grid / off-axis-rotation `BeartypeCallHintViolation` from the `_GUARD`-contracted `_imposed` admission seam, a `PLANS` key miss, and the stream/source file fault as distinct `BoundaryFault` cases rather than the `Exception` catch-all the faults owner rejects and rather than a parallel `ImposeFault` `Literal` the boundary never reads; the interior raises nothing of its own, the live-native-page mutation the one platform-forced statement seam `boundaries.md` names, cancellation excluded from `_FAULTS` and re-raised as the structured signal. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` through the receipt owner's named flat-scalar mints selected by the `Composed.kind` discriminant — the OCG-bearing `Impose` arm `ArtifactReceipt.Egress(key, bytes, sheets, 0, 0, overlays)` carrying the imposed byte count, the REAL `Document.page_count`, and the minted-OCG layer count on the `overlays` slot exactly as the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms do (a no-mark imposition the degenerate `ArtifactReceipt.Pdf(key, bytes, sheets)` form), the `Proof` arm `ArtifactReceipt.Preview(key, width, height)` carrying the pixmap extent, and the `Plan` arm none — its `ImposedPlan` pre-flight rides `Imposition.planned`, never a fabricated zero-sheet `Pdf` receipt over plan-JSON bytes.

## [01]-[INDEX]

- [01]-[IMPOSE]: n-up / booklet / signature / work-and-turn / cut-and-stack / come-and-go / perfect-bind / sheetwise press-imposition owner over the closed-payload `ImposeOp` `tagged_union` (`Impose` the drawing case discriminating on the `Scheme` value, `Plan` the compute-only pre-flight case, `Proof` the raster-thumbnail case) folded once into a `Composed` evidence struct the `of`/`contribute`/`layers` projections share, rail-typed `RuntimeRail[ContentKey]` over `async_boundary(catch=_FAULTS)`, dispatched to `pymupdf` (`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, rotate, clip, oc)` cross-document source-page placement with head-to-head verso rotation, fit-scale, bleed clip, and OCG layer binding; `add_ocg`+`set_layer` OCG mint and reader visibility/lock config; `set_toc`/`embfile_add`/`bake`/`scrub`/`subset_fonts`/`rewrite_images`/`set_metadata`/`set_xml_metadata`/`tobytes`/`get_pixmap` press finishing and proof; core); `Scheme`/`Place`/`Placement`/`Geometry`/`Marks`/`ImposedPlan`/`Artifact` the closed scheme/placement/geometry/finishing/evidence value-object family; the `frozendict[Scheme, Place]` `PLANS` totality table; the one `_composed` total `match` lowering the `Impose` arm to the `@_GUARD`-contracted `_imposed` admission seam whose `Geometry`/`Placement` shape violation rides the `_FAULTS` tuple; projects `Imposition.layers` to `export/layered#LAYERED` `Layer`; threads `core/receipt#RECEIPT` through the receipt owner's named `ArtifactReceipt.Egress`/`Pdf`/`Preview` flat-scalar mints selected by the `Composed.kind` discriminant.

## [02]-[IMPOSE]

- Owner: `Imposition` the one imposition owner discriminating the operation over the closed `ImposeOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; the verb family is three cases — `Impose` the drawing case carrying `(source, Scheme, Geometry, Marks)` discriminating on the `Scheme` value, `Plan` the compute-only pre-flight case, and `Proof` the raster-thumbnail case — never a `Nup`/`Booklet`/`Signature` sibling triple differing only by the literal scheme and a `leaves` knob the `Geometry` already carries. `Scheme` the closed `StrEnum` over the press schemes (`NUP` raw n-across-by-n-down grid, `BOOKLET` saddle-stitch 2-up centerfold, `SIGNATURE` folded-signature multi-leaf, `WORK_AND_TURN` single-plate duplex flipped across the gripper edge, `WORK_AND_TUMBLE` single-plate duplex flipped head-to-foot down the lead edge, `CUT_AND_STACK` guillotine-cut sequential stack, `COME_AND_GO` two-up duplicate-set tumble, `PERFECT_BIND` n-up flat bind-edge stack, `SHEETWISE` recto/verso separate-plate grid); `PLANS` the one `frozendict[Scheme, Place]` policy table binding each scheme straight to its `place(pages, geometry)` placement function — page order, recto/verso `rotate`, fit-scale, and per-sheet creep shift fused into one sweep returning the full `Placement` list — the table the totality proof of one placement arm per scheme, never an `if scheme == ...` cascade and never a parallel imposer class; the imposed-sheet count is the derived `_sheet_count(placements)` over that one list, never a second `sheets` callable that drifts from the placement; `Placement` the closed-payload value object each placement carries (source page index, imposed-sheet index, cell rect, rotate axis, fit-scale flag, OCG layer name, the OCG default-visibility and reader-lock state, bleed clip rect) projecting every field straight onto the `show_pdf_page` keyword set and feeding the one `set_layer` reader-config write; `Geometry` the row-policy value object carrying the imposed-sheet size and orientation, the n-across/n-down grid, the binding-edge gutter, the head/foot trim, the spine allowance, the fold count and signature leaf count, the per-sheet creep shift, and the bleed/trim, projecting once through `partition` to the binding-aware cell grid every scheme reads with every field live; `Marks` the press-finishing policy carrying the overlay route, the print-control mark set, the per-signature outline route, the cut-list/fold-map attachment, the interactive-layer bake flag, the font-subset flag, the image-recompress flag, the hidden-content scrub flag, the optional-content layer config, the deterministic linearized-save flag, and the press info/XMP metadata. `pymupdf` owns the `Page.show_pdf_page` cross-document source-page draw (every `keep_proportion`/`rotate`/`clip`/`oc` keyword a `Placement`/`Marks` field), the `open`/`new_page`/`paper_rect` imposed-sheet construction, the `add_ocg`-mint-then-`set_layer(0, on=, off=, locked=)`-config OCG-layer reader binding, the `set_toc`/`embfile_add`/`bake`/`subset_fonts`/`rewrite_images`/`scrub`/`set_metadata`/`set_xml_metadata` press finishing, the `get_pixmap` proof raster, and the `tobytes(garbage=, deflate=, use_objstms=)` deterministic serialize; the per-scheme page-order, creep, verso-rotation, and tumble arithmetic is this owner's page-settled fold over that one cross-document placement floor, never a re-implemented PDF byte emitter.
- Cases: `ImposeOp` cases — `Impose(source, scheme, geometry, marks)` (the one drawing case — the `_composed` arm resolves `PLANS[scheme]` over the source page count and `Geometry`, opens the source through `pymupdf.open(stream=source)`, raises the `pymupdf.EmptyFileError` the `_FAULTS` tuple admits on a zero-page source, computes the `Placement` list and the derived `_sheet_count` once, allocates that many imposed pages at the `Geometry` imposed-sheet size on the live document, draws `_draw_one` over the placements where each `_draw_one` mints `add_ocg(p.name, on=p.visible)` for a `name`-bearing placement and draws `out[p.sheet].show_pdf_page(...)` on the live native page returning its `(xref, visible, locked)` mint row, `Block.choose(_oc_state)` keeps the rows that minted a real xref, one `_configure_layers` `set_layer(0, on=, off=, locked=)` write drives the reader visibility/lock config over the minted set, so `Marks.serialize` runs once on the all-placed document and `Composed.layers` carries the minted total, then runs the `Marks.finished` press finish — the saddle order, the head-to-head verso rotation `180·(cell // across) % 360`, the per-fold creep, the work-and-turn / work-and-tumble duplex mirror, the come-and-go duplicate-set, and the sheetwise recto/verso split all carried by the resolved `PLANS` row, the case unchanged) · `Plan(source, scheme, geometry)` (compute-only pre-flight — `_planned` resolves `PLANS[scheme]` over the source page count and `Geometry`, returning the `ImposedPlan` `msgspec.Struct` carrying the placement rows, the per-scheme metrics — signature count, fold depth, sheets, blank-pad count, per-sheet creep extent — and the derived sheet count without drawing, the pre-flight arm a press operator reads to validate the imposition before committing the draw) · `Proof(source, dpi, sheet)` (raster thumbnail — the `_composed` proof arm opens the already-imposed press-form PDF and `get_pixmap(dpi=)` rasterizes the `sheet`-indexed imposed sheet to PNG keyed by the same `ContentKey`, the `Pixmap.width`/`height` riding the `Preview` receipt, so a press operator proofs ANY imposed sheet — recto, verso, or an inner signature page — rather than only the first, the multi-sheet contact-strip proof one further `Proof` field over the same arm) — matched by one total `match`/`case`; never a per-scheme imposer sibling, never a parallel booklet-versus-signature draw method, never a per-page draw call family, never a hard-coded `rotate=0`. The `Marks` finish, the fit-scale, and the bleed `clip` ride the one drawing case through the shared `_draw_one` live-page sweep, so a new scheme is one `Scheme` member plus one `PLANS` row, never a new case.
- Entry: `Imposition.of` is `async` over `async_boundary(f"impose.{self.op.tag}", self._keyed, catch=_FAULTS)` where `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` is the real engine raise tuple — `pymupdf.FileDataError`/`EmptyFileError` (both `RuntimeError`-derived), a malformed-`Geometry`/`Placement` `BeartypeCallHintViolation` from the `_GUARD`-contracted fold, a `PLANS` `KeyError`, and the source stream/file `OSError` — so the boundary discriminates each into its own `BoundaryFault` case rather than the `Exception` catch-all the `runtime/faults#FAULTS` owner rejects, and never a parallel `ImposeFault` `Literal` rail the boundary never reads. `_keyed` is the awaitable thunk computing the one `Composed` fold and minting `ContentIdentity.of(f"impose-{self.op.tag}", composed.data)` — the boundary catches the provider raise and converts, so the interior raises nothing of its own and returns `ContentKey`, never a nested `Result` inside the `RuntimeRail` the boundary already owns. Every arm resolves synchronously on the cp315 core inside the async capsule (`pymupdf`, native MuPDF cp310-abi3, imports once on the core band at boundary scope inside the fold), so no imposition arm crosses a process seam and the `async_boundary` is the uniform contract `document` assembly and `export/layered#LAYERED` `await`.
- Auto: `_composed(op) -> Composed` is the ONE total `match` both `of` and `contribute` read — no second render recomputes the bytes. The `Impose` arm resolves `PLANS[scheme]` over the source page count and `Geometry` and hands the resolved `Placement` `Block` plus the `Geometry`/`Marks` to the `@_GUARD`-contracted `_imposed` admission seam, whose direct `Geometry`/`Block[Placement]` parameters let beartype deep-check the `Is`-refined grid/extent/rotation fields and rail a non-positive grid count or off-axis verso rotation as `api` BEFORE the native draw; `_imposed` computes the derived `_sheet_count` once, allocates that many imposed pages through `Document.new_page(width=, height=)` at the `Geometry` imposed-sheet size on the live `out` document, and folds `_draw_one` over the placement `Block` — each `_draw_one` minting `add_ocg(p.name, on=p.visible)` for a `name`-bearing placement and drawing `out[p.sheet].show_pdf_page(Rect(*p.cell), src, pno=p.source, keep_proportion=p.fit, overlay=p.overlay, rotate=p.rotate, clip=Rect(*p.clip) if p.clip else None, oc=xref)` on the live native page (never a held `Page` list that outlives `out` and faults on draw, the live-handle mutation the `boundaries.md` platform-forced statement seam), returning its `(xref, visible, locked)` mint row — drawing each source page into its computed cell at its computed rotation, fit-scale, over/under draw, and bleed clip; `Block.choose(_oc_state)` keeps the rows that minted a real xref and one `_configure_layers` `set_layer(0, on=[xref … if visible], off=[xref … if not visible], locked=[xref … if locked])` write drives the reader's per-signature-group visibility and lock panel exactly as the sibling `composition/sheet#SHEET` `_configure_layers` does, then runs the `Marks.finished(out, geometry, sheets)` press finish (the OCG-bound imposition-map `set_toc`, the cut-list `embfile_add`, the `bake`/`scrub`/`subset_fonts(fallback=True)`/`rewrite_images`/`set_metadata`/`set_xml_metadata` keyed off the `Marks` flags) and the `Marks.serialize(out)` deterministic `tobytes`, returning `Composed(data, sheets=out.page_count, layers=len(minted))` reading the REAL `Document.page_count` off the imposed document. The native-handle `Block.map`/`Block.choose` sweep is the platform-forced seam exactly as the sibling `composition/sheet#SHEET` `_composed` and `export/layered#LAYERED` `_pdf_ocg` arms run it — a `Block.fold`/`traverse` over a discarded mutated-document `Result[Document, Never]` is the index-threaded fold the `rails-and-effects.md` page rejects, not the dense form, because the engine raise the boundary converts replaces a per-element `Result` thread that can never carry an `Error`. The `_grid`/`_folded`/`_duplexed`/`_stacked`/`_paired`/`_split` placement functions are the `PLANS` rows (the `NUP` and `PERFECT_BIND` schemes both bind `_grid`, perfect-bind carrying its `spine` allowance on the `Geometry`; the `WORK_AND_TURN` and `WORK_AND_TUMBLE` schemes both bind `_duplexed`, differing only by the `on_across` mirror-axis policy value, two rows over one body never two functions); the `cell` for each placement comes from `partition` (binding-edge gutter, head/foot trim, spine allowance, the `shingle`-signed creep shift, bleed margin all applied there). The `Plan` arm `_planned` resolves the same placement function over the page count and projects the `ImposedPlan`, drawing no surface; the `Proof` arm `open(stream=source)[sheet].get_pixmap(dpi=)` then `Pixmap.tobytes("png")` over the `sheet`-indexed imposed page, carrying the pixmap extent onto the `Composed.kind` raster discriminant. `of` maps the `Composed` to the content key through `ContentIdentity.of`; `contribute` reads the same `_composed` and routes on `Composed.kind` plus the `Composed.layers` count to the receipt owner's named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mint, so the byte count, page count, and OCG-layer count are the computed facts, never a recompute and never a fabricated zero-sheet `Pdf` receipt over non-PDF bytes.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the receipt owner's named flat-scalar mints — the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count select the mint once and each named static factory keyword-constructs its `case()`, so the imposition owner adds NO sibling factory and NO new receipt case. The OCG-bearing `Impose` op selects `ArtifactReceipt.Egress(key, len(data), composed.sheets, 0, 0, composed.layers)` carrying the imposed byte count, the REAL imposed-sheet page count, and the minted-OCG layer count on the `overlays` slot (zero encryption-R/outline-depth — imposition is neither a security nor a navigation close, exactly the slot the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms report their authored-layer count on), and a degenerate no-mark imposition (`composed.layers == 0`) selects `ArtifactReceipt.Pdf(key, len(data), composed.sheets)`; the `Proof` op selects `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])` carrying the pixmap extent. `contribute` reads the SAME `_composed(op)` `of` drives (one fold), mints the key over `Composed.data` through the same `ContentIdentity.of` `of` uses (self-contained, no caller-passed key), and yields `receipt.contribute()` — never a per-kind facts `Struct` re-wrapping the scalars the named mint already takes positionally (the indirection the receipt owner rejects), and never a `dispatch(kind, *scalars)` bag. The `Plan` op contributes no receipt — its `ImposedPlan` projection is a pre-flight payload, not an imposed PDF, so a fake `0`-page `Pdf` receipt over plan-JSON bytes is the deleted form; a press operator reads the `ImposedPlan` directly through `Imposition.planned`.
- Packages: `pymupdf` (`open(stream=)`/`Document.new_page(width=, height=)`/`paper_rect`/`Document.__getitem__`/`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, overlay, oc, rotate, clip)`/`Page.get_pixmap(dpi=)`/`Pixmap.width`/`Pixmap.height`/`Pixmap.tobytes`/`Rect`/`Document.page_count`/`Document.add_ocg(name, config=-1, on=, intent=, usage=)`/`Document.set_layer(config, basestate=, on=, off=, rbgroups=, locked=)`/`Document.set_toc`/`Document.embfile_add`/`Document.bake`/`Document.subset_fonts`/`Document.rewrite_images`/`Document.scrub`/`Document.set_metadata`/`Document.set_xml_metadata`/`Document.tobytes(garbage=, deflate=, use_objstms=)`, all reflection-verified on cp315, native MuPDF cp310-abi3, AGPL-3.0 — internal pipeline use) on the cp315 core; `msgspec` (`Struct` frozen rows, `structs.replace` the `BOOKLET` `leaves=1` row, `json.Encoder` the `ImposedPlan` egress); `beartype` (`BeartypeConf(violation_type=BeartypeCallHintViolation)` the `_GUARD` shape contract guarding the `_imposed` admission seam, `beartype.vale.Is` the `Across`/`Leaves`/`Span`/`Quarter` refinement aliases whose violation rides the `_FAULTS` tuple); `expression` (`Option`/`Some`/`Nothing` the `_oc_state` mint, the `tagged_union` case family, `Block`/`Block.of_seq`/`Block.fold`/`Block.map`/`Block.choose` the placement carrier and the minted-OCG fold, `Map`/`Map.empty`/`Map.add`/`Map.try_find` the `layers` group fold); `core/receipt#RECEIPT` (the named `ArtifactReceipt.Egress(key, bytes, pages, encryption_r, outline_depth, overlays)`/`Pdf(key, bytes, pages)`/`Preview(key, width, height)` flat-scalar mints selected by the `Composed.kind` discriminant); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`/`BoundaryFault`); projects `export/layered#LAYERED` `Layer`.
- Growth: a new imposition scheme is one `Scheme` member plus one `Place` row in `PLANS` whose `place` computes its order, rotation, fit-scale, creep, and count — never a parallel imposer class, never an `if scheme == ...` branch, never a new `ImposeOp` case; a new geometry knob (a variable gutter, a crossover bleed, a per-signature creep curve, a lay-edge jog) is one `Geometry` field read in `partition` or the placement fold — never an `Impose` parameter; a new placement axis (a mirrored verso, a per-cell margin, a named OCG layer) is one `Placement` field carried straight onto the `show_pdf_page` keyword set — never a per-placement draw method; a deeper signature is the same `_folded` fold over a larger `Geometry.leaves`; a new press-finish concern (an output-intent, a crop-mark profile, a colour-bar route, a press-attachment) is one `Marks` field read in the finish pass; a registration-mark or crop-guide overlay SHAPE on the imposed sheet routes through the `Marks.overlay` seam to `composition/compose#COMPOSE` `Overlay`, never a mark-emit arm grafted here; a new proof modality (a multi-sheet contact strip) is one field on the `Proof` case. Zero new surface.
- Boundary: this owner imposes already-emitted sheets and assembles no document — a per-scheme imposer class family (a `NupImposer`/`BookletImposer`/`SignatureImposer` triple), a per-scheme `ImposeOp` case differing only by the literal scheme, a stringly-typed `if scheme == "booklet"` branch beside the `Scheme` row dispatch, a second `match` recomputing the bytes for the receipt beside the one `Composed` fold, a `traverse`/`Block.fold` over a sentinel-returning `_draw_one -> Result[Document, Never]` whose error half can never materialize beside the live-page `Block.map`/`Block.choose` sweep, a hard-coded `rotate=0` beside the plan-computed rotation, a minted `add_ocg` group with no `set_layer` reader-config behind it, a parallel `ImposeFault` `Literal` rail beside the `_FAULTS`-narrowed boundary, a fake `0`-page `Pdf` receipt over plan-JSON bytes, a per-kind facts `Struct` (`EgressFacts`/`PdfFacts`/`PreviewFacts`) or an `of(key, facts)` indirection re-wrapping the scalars the receipt owner's named `ArtifactReceipt.Egress`/`Pdf`/`Preview` mints already take positionally, a hand-rolled PDF byte emitter beside the admitted `pymupdf` engine, and a parallel imposition-receipt type beside the named flat-scalar contribution are the deleted forms; no UI, no live imposition viewer, no document assembly, no sheet re-author. The sheets the imposition reads arrive already-emitted from `composition/sheet#SHEET` (the framed titled sheet) and `document/emit#DOCUMENT` (the document body) — this owner re-orders, rotates, crops, and places them and re-authors no sheet; the sheet-authoring concern stays upstream. The imposed body is handed to `document` assembly and projects `Imposition.layers -> tuple[Layer, ...]` to `export/layered#LAYERED` (one row per OCG-bound placement carrying its placement `Box` extent) — this owner imposes the press form and grows no assembly arm and no per-layer writer, the named-layer authoring landing once in the export owner; a layered-export arm grafted onto `ImposeOp` and a frame re-render inside `layers` are the rejected forms. This owner is the DEDICATED booklet/signature press-imposition engine distinct from `document/egress#FINISH` `IMPOSE`: the egress IMPOSE step is the in-document `pypdf.Transformation` n-up grid fold over a finished PDF as one of ten finishing steps, computing no booklet/signature page order, creep, or verso rotation; this owner computes the saddle-stitch creep-compensated order, the folded-signature ordering, and the work-and-turn duplexing over `pymupdf.show_pdf_page` (the higher-fidelity cross-document draw), so the two never overlap — egress finishes one PDF, imposition imposes a press form. A signature-imposition branch grafted onto the egress step and an n-up grid re-implemented here beside the egress `Transformation` fold are the rejected boundary crossings. PDF security finishing (encrypt/sign/watermark) routes to `document/egress#FINISH`; registration/crop-mark overlay AUTHORING routes through `Marks.overlay` to `composition/compose#COMPOSE` (this owner carries the overlay-route flag, the mark-shape emission lands there). `pymupdf` resolves on the cp315 core and imports at boundary scope; the AGPL `pymupdf` placement leg is reserved for the internal imposition pipeline per the rail's license constraint. The content key mints over the emitted imposed bytes through the runtime `ContentIdentity.of`, never re-minted off a source sheet key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from typing import TYPE_CHECKING, Annotated, Final, Literal, assert_never

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

if TYPE_CHECKING:
    from pymupdf import Document

    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Box = tuple[float, float, float, float]
type Dimensions = tuple[float, float]
type Edge = Literal["left", "right", "top", "bottom"]
type Lay = Literal["short", "long"]  # the press lay/gripper edge — feed direction, distinct from the binding edge
type Place = Callable[[int, "Geometry"], "Block[Placement]"]

# the load-bearing geometry invariants the `partition`/`_folded` arithmetic divides by — a zero or
# negative grid count is a malformed `Geometry` the `_GUARD` rejects at the admission seam before the
# division, not a `ZeroDivisionError` deep in the fold. `Quarter` is the 0/90/180/270 verso-rotation
# the `show_pdf_page` `rotate` keyword admits; an off-axis angle is the contract violation.
type Across = Annotated[int, Is[lambda n: n >= 1]]
type Leaves = Annotated[int, Is[lambda n: n >= 1]]
type Span = Annotated[float, Is[lambda v: v >= 0.0]]  # a non-negative gutter/trim/spine/bleed extent
type Quarter = Annotated[int, Is[lambda d: d in (0, 90, 180, 270)]]

# pymupdf raises `FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or
# zero-page source, a `PLANS` miss raises `KeyError`, a malformed `Rect(*cell)` extent raises
# `ValueError`, the source stream raises `OSError`, and the `_GUARD` shape contract raises
# `BeartypeCallHintViolation` on a non-positive grid count or off-axis verso rotation. The boundary
# narrows on this real raise tuple so `async_boundary` discriminates each into its `BoundaryFault`
# case rather than the `Exception` catch-all the faults owner rejects; cancellation is excluded.
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)


class Scheme(StrEnum):
    NUP = "nup"
    BOOKLET = "booklet"
    SIGNATURE = "signature"
    WORK_AND_TURN = "work-and-turn"
    WORK_AND_TUMBLE = "work-and-tumble"
    CUT_AND_STACK = "cut-and-stack"
    COME_AND_GO = "come-and-go"
    PERFECT_BIND = "perfect-bind"
    SHEETWISE = "sheetwise"


class Artifact(StrEnum):  # the Composed receipt-shape discriminant — imposed vector PDF vs raster proof
    PDF = "pdf"
    PREVIEW = "preview"


# --- [BOUNDARIES] -----------------------------------------------------------------------
# the contract weave admitting the load-bearing geometry invariant the `partition` arithmetic divides
# by: `_admit` takes the grid counts as DIRECT `Is`-refined scalar parameters, so beartype enforces them
# (a `msgspec.Struct` field or a `Block`/`tuple` element is NOT deep-checked by beartype — only a direct
# scalar parameter is), raising `BeartypeCallHintViolation` the `_FAULTS`-narrowed `async_boundary` lifts
# to its `api` `BoundaryFault` case before the divide. `_aspected` is never minted — the boundary's
# `CLASSIFY` row owns the violation-to-fault lift, this is only the shape admission at the seam that reads it.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


@_GUARD
def _admit(across: Across, down: Across, /) -> tuple[int, int]:
    return across, down


# --- [MODELS] ---------------------------------------------------------------------------
class Geometry(Struct, frozen=True):
    sheet: Dimensions = (1190.55, 841.89)
    landscape: bool = False
    across: Across = 2
    down: Across = 1
    leaves: Leaves = 1  # signature leaf count — the fold depth a saddle/signature block reads
    gutter: Span = 0.0  # binding-edge gutter between facing cells
    head_trim: Span = 0.0  # top/bottom finished-trim allowance
    spine: Span = 0.0  # perfect-bind glue/spine allowance added at the binding edge
    creep: Span = 0.0  # per-fold creep extent compensating fold-thickness drift
    bleed: Span = 0.0  # cell expansion past the trim box
    binding: Edge = "left"
    lay: Lay = "long"  # the press feed edge: which sheet edge the grippers seize, reserving `gripper` unprintable margin there
    gripper: Span = 0.0  # the unprintable claw margin the press reserves on the `lay` feed edge
    shingle: bool = False  # creep direction: push-out (outer pages shift out, default) vs shingle-in for a thick bound block

    @property
    def slots(self) -> int:
        return self.across * self.down

    @property
    def oriented(self) -> Dimensions:
        width, height = self.sheet
        return (height, width) if self.landscape else (width, height)

    def partition(self, shift: float = 0.0) -> tuple[Box, ...]:
        # `shift` is the per-signature creep extent the fold passes; `shingle` flips its direction —
        # push-out (default) drifts the outer signature away from the spine, shingle-in toward it. The
        # `gripper` claw margin is reserved on the lead edge of the feed axis the `lay` edge names.
        width, height = self.oriented
        grip_x, grip_y = (self.gripper, 0.0) if self.lay == "short" else (0.0, self.gripper)
        cell_w = (width - (self.across + 1) * self.gutter - self.spine - grip_x) / self.across
        cell_h = (height - (self.down + 1) * self.gutter - 2.0 * self.head_trim - grip_y) / self.down
        on_x = self.binding in ("left", "right")
        creep = -shift if self.shingle else shift
        dx, dy = (creep, 0.0) if on_x else (0.0, creep)
        sign = -1.0 if self.binding in ("right", "bottom") else 1.0
        ox = self.gutter + grip_x + (self.spine if self.binding == "left" else 0.0)
        oy = self.head_trim + self.gutter + grip_y
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
    name: str = ""  # an OCG layer label binds this placement to a minted optional-content group when set
    visible: bool = True  # the OCG default-on state the `set_layer` ui config writes, as layered.Layer.visible
    locked: bool = False  # the reader-side layer-lock hint `set_layer(locked=)` honors, as layered.Layer.locked


class Marks(Struct, frozen=True):
    overlay: bool = False  # route a registration/crop overlay to composition/compose#COMPOSE via this OCG
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
            document.scrub(hidden_text=True, javascript=True, clean_pages=True, embedded_files=False, attached_files=False, metadata=False, xml_metadata=False, redactions=False)
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
        # `tobytes` is the in-memory egress; `ez_save`/`save` write a path. The deterministic
        # web-optimized form rides `garbage=4`/`use_objstms=1`, the plain form `garbage=3`.
        return document.tobytes(garbage=4, deflate=True, use_objstms=1) if self.linearize else document.tobytes(garbage=3, deflate=True)


class ImposedPlan(Struct, frozen=True):
    scheme: Scheme
    sheets: int  # imposed press sheets the placement fold yields
    pages: int  # source pages read off the live document
    leaves: int  # signature leaf count the fold depth reads
    signatures: int  # bound folding units (ceil(pages / 4·leaves) for a folded scheme, sheets otherwise)
    padded: int  # blank pages the fold pads the count to the next signature multiple
    creep: float  # outward creep extent applied at the outermost signature
    placements: tuple[Placement, ...]


class Composed(Struct, frozen=True):  # the one evidence struct of/contribute/layers read — no second render
    data: bytes
    sheets: int
    kind: Artifact = Artifact.PDF
    extent: tuple[int, int] = (0, 0)  # the Proof pixmap width/height the raster receipt rides
    layers: int = 0  # the count of OCGs the draw fold minted, riding the ArtifactReceipt.Egress overlays slot


def _sheet_count(placements: Block[Placement]) -> int:
    return placements.fold(lambda acc, p: max(acc, p.sheet), -1) + 1


# --- [OPERATIONS] -----------------------------------------------------------------------
def _saddle(slots: int) -> tuple[int, ...]:
    order: list[int] = []
    left, right = slots - 1, 0
    for _ in range(slots // 2):
        order.extend((right, left) if right % 2 == 0 else (left, right))
        left, right = left - 1, right + 1
    return tuple(order)


def _grid(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(
        Placement(source=page, sheet=page // slots, cell=cells[page % slots]) for page in range(pages)
    )


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
    return Block.of_seq(
        Placement(source=page, sheet=page, cell=cells[slot])
        for page in range(pages)
        for slot in range(geometry.slots)
    )


def _stacked(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    stack = -(-pages // slots)
    return Block.of_seq(
        Placement(source=page, sheet=page % stack, cell=cells[page // stack]) for page in range(pages)
    )


def _split(pages: int, geometry: Geometry) -> Block[Placement]:
    slots, cells = geometry.slots, geometry.partition()
    return Block.of_seq(
        Placement(source=page, sheet=2 * (page // slots) + page % 2, cell=cells[page % slots])
        for page in range(pages)
    )


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
    proof: tuple[bytes, float, int] = case()

    @staticmethod
    def Impose(source: bytes, scheme: Scheme = Scheme.NUP, geometry: Geometry = Geometry(), marks: Marks = Marks()) -> "ImposeOp":
        return ImposeOp(impose=(source, scheme, geometry, marks))

    @staticmethod
    def Plan(source: bytes, scheme: Scheme, geometry: Geometry = Geometry()) -> "ImposeOp":
        return ImposeOp(plan=(source, scheme, geometry))

    @staticmethod
    def Proof(source: bytes, dpi: float = 96.0, sheet: int = 0) -> "ImposeOp":  # `sheet` proofs any imposed sheet, not only the first
        return ImposeOp(proof=(source, dpi, sheet))


# --- [COMPOSITION] ----------------------------------------------------------------------
class Imposition(Struct, frozen=True):
    op: ImposeOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"impose.{self.op.tag}", self._keyed, catch=_FAULTS)

    async def _keyed(self) -> ContentKey:
        return ContentIdentity.of(f"impose-{self.op.tag}", _composed(self.op).data)

    def planned(self) -> ImposedPlan:
        match self.op:
            case ImposeOp(tag="impose", impose=(source, scheme, geometry, _marks)) | ImposeOp(tag="plan", plan=(source, scheme, geometry)):
                return _planned(source, scheme, geometry)
            case _:
                assert_never(self.op)

    def contribute(self) -> "Iterable[Receipt]":
        if self.op.tag in ("impose", "proof"):
            composed = _composed(self.op)
            key = ContentIdentity.of(f"impose-{self.op.tag}", composed.data)
            match composed.kind:
                case Artifact.PDF if composed.layers:  # the OCG-bearing imposed press form rides the minted-layer count on the Egress overlays slot
                    receipt = ArtifactReceipt.Egress(key, len(composed.data), composed.sheets, 0, 0, composed.layers)
                case Artifact.PDF:
                    receipt = ArtifactReceipt.Pdf(key, len(composed.data), composed.sheets)
                case Artifact.PREVIEW:
                    receipt = ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])
                case _:
                    assert_never(composed.kind)
            yield from receipt.contribute()

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


def _composed(op: ImposeOp) -> Composed:  # the one pure render fold both `of` and `contribute` read
    import pymupdf

    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, marks)):
            src = pymupdf.open(stream=source, filetype="pdf")
            return _imposed(src, geometry, marks, PLANS[scheme](src.page_count, geometry))
        case ImposeOp(tag="plan", plan=(source, scheme, geometry)):
            plan = _planned(source, scheme, geometry)
            return Composed(data=json.Encoder().encode(plan), sheets=plan.sheets)
        case ImposeOp(tag="proof", proof=(source, dpi, sheet)):
            pixmap = pymupdf.open(stream=source, filetype="pdf")[sheet].get_pixmap(dpi=int(dpi))
            return Composed(pixmap.tobytes("png"), sheets=1, kind=Artifact.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _imposed(src: "Document", geometry: Geometry, marks: Marks, placements: Block[Placement], /) -> Composed:
    # the live native document the engine grows in place is the platform-forced statement seam; the
    # `Geometry` divisor invariant is already admitted by `partition`'s `_admit` guard inside the placement
    # resolution above, so a malformed grid count has railed as `api` before any page is allocated here.
    import pymupdf

    sheets = _sheet_count(placements)
    width, height = geometry.oriented
    out = pymupdf.open()
    for _ in range(sheets):
        out.new_page(width=width, height=height)
    minted = placements.map(lambda p: _draw_one(out, src, p)).choose(_oc_state)  # live handle; held Page faults on draw
    _configure_layers(out, minted)  # one ui-config write driving reader visibility/lock over the minted OCG set
    marks.finished(out, geometry, sheets)
    return Composed(data=marks.serialize(out), sheets=out.page_count, layers=len(minted))


def _draw_one(out: "Document", src: "Document", p: Placement) -> tuple[int, bool, bool]:  # (xref, visible, locked); xref 0 if unlayered
    import pymupdf

    xref = out.add_ocg(p.name, on=p.visible, intent="View", usage="Artwork") if p.name else 0
    out[p.sheet].show_pdf_page(  # index the live doc; a held Page list outliving `out` faults on draw
        pymupdf.Rect(*p.cell), src, pno=p.source, keep_proportion=p.fit, overlay=p.overlay, rotate=p.rotate,
        clip=pymupdf.Rect(*p.clip) if p.clip is not None else None, oc=xref,
    )
    return xref, p.visible, p.locked


def _oc_state(drawn: tuple[int, bool, bool], /) -> Option[tuple[int, bool, bool]]:  # keep only the rows that minted a real OCG xref
    return Some(drawn) if drawn[0] else Nothing


def _configure_layers(out: "Document", minted: Block[tuple[int, bool, bool]], /) -> None:
    # one `set_layer` ui-config write over the minted OCG set — the reader's panel toggles `on`/`off` and
    # honors `locked`, exactly as the sibling `composition/sheet#SHEET` `_configure_layers` drives the placed layers.
    if not minted.is_empty():
        out.set_layer(
            0,
            on=[xref for xref, visible, _locked in minted if visible],
            off=[xref for xref, visible, _locked in minted if not visible],
            locked=[xref for xref, _visible, locked in minted if locked],
        )


def _planned(source: bytes, scheme: Scheme, geometry: Geometry) -> ImposedPlan:
    import pymupdf

    src = pymupdf.open(stream=source, filetype="pdf")
    pages = src.page_count
    placements = PLANS[scheme](pages, geometry)
    sheets, folded = _sheet_count(placements), scheme in (Scheme.BOOKLET, Scheme.SIGNATURE)
    leaves = 1 if scheme is Scheme.BOOKLET else geometry.leaves  # the booklet row forces leaves=1
    fold = 4 * max(leaves, 1)  # the effective signature size the placement folded against
    return ImposedPlan(
        scheme=scheme, sheets=sheets, pages=pages, leaves=leaves,
        signatures=-(-pages // fold) if folded else sheets,
        padded=(-pages % fold) if folded else 0,
        creep=geometry.creep * max(-(-pages // fold) - 1, 0) if folded else 0.0,
        placements=tuple(placements),
    )


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(op: ImposeOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    # the imposed press form projects outward as one named row per OCG-bound signature group the
    # `export/layered#LAYERED` owner binds into OCG/SVG layers — the row carries the union cell box
    # over that group's placements and the shared source; no frame re-render, no per-placement duplicate.
    match op:
        case ImposeOp(tag="impose", impose=(source, scheme, geometry, _marks)):
            grouped = Block.of_seq(p for p in PLANS[scheme](_page_count(source), geometry) if p.name).fold(
                lambda acc, p: acc.add(p.name, _union(acc.try_find(p.name).default_value(p.cell), p.cell)), Map.empty()
            )
            return tuple(Layer(_name(names, index, name), source, box) for index, (name, box) in enumerate(grouped.items()))
        case _:
            return ()


def _union(left: Box, right: Box) -> Box:
    return (min(left[0], right[0]), min(left[1], right[1]), max(left[2], right[2]), max(left[3], right[3]))


def _name(names: tuple[str, ...], index: int, fallback: str) -> str:
    return names[index] if index < len(names) else fallback


def _page_count(source: bytes) -> int:
    import pymupdf

    return pymupdf.open(stream=source, filetype="pdf").page_count
```

## [03]-[RESEARCH]

- [SHOW_PDF_PAGE_VERIFIED]: the imposition draws each source page into its imposed-sheet cell through `pymupdf.Page.show_pdf_page(page, rect, docsrc, pno=0, keep_proportion=True, overlay=True, oc=0, rotate=0, clip=None) -> int`, a VERIFIED REAL member of `pymupdf.Page` reflected on cp315 (`pymupdf 1.27.2.3`, native MuPDF cp310-abi3, the reflected signature confirming the exact keyword set) — the member vector-copies the `docsrc` page `pno` into the target `rect` of the host page, `keep_proportion` preserving aspect (the `Placement.fit` axis), `rotate` placing at 0/90/180/270 (the head-to-head verso flip the `_folded` plan computes), `clip` restricting the source region to the bleed/trim box, and `oc` binding the placed page to an optional-content group. The whole placement axis is `Placement` fields projected straight onto these keywords — `pno=p.source`, `keep_proportion=p.fit`, `rotate=p.rotate`, `clip=Rect(*p.clip) if p.clip else None`, `oc=out.add_ocg(p.name, on=p.visible, intent="View", usage="Artwork")` when `p.name` is set — so the draw fold reads no second join and carries no hard-coded `rotate=0`. The press-finish, OCG-config, and proof members are equally reflection-verified on cp315 against the folder catalogue: `Document.new_page(doc, pno=-1, width=595, height=842) -> Page`, `Document.__getitem__` (index the live doc for the draw and the `sheet`-indexed proof), `Document.add_ocg(name, config=-1, on=1, intent=None, usage=None) -> int`, `Document.set_layer(config, basestate=None, on=None, off=None, rbgroups=None, locked=None)` (the reader visibility/lock config over the minted OCG set), `Document.set_toc(toc, collapse=1) -> int` (the per-signature imposition-map outline), `Document.embfile_add(name, buffer_, filename=, desc=)` (the cut-list/fold-map press file), `Document.bake(annots=True, widgets=True)` (the interactive-layer flatten), `Document.subset_fonts(fallback=True)`, `Document.rewrite_images(dpi_threshold=, dpi_target=, lossy=)`, `Document.scrub(...)`, `Document.set_metadata(dict)`, `Document.set_xml_metadata(str)`, `Document.tobytes(garbage=, deflate=, use_objstms=)` (the deterministic in-memory egress — `ez_save`/`save` write a path, `tobytes` returns the bytes), `Page.get_pixmap(dpi=) -> Pixmap` with `Pixmap.width`/`Pixmap.height` and `Pixmap.tobytes("png")` (the `Proof` raster), and the module `paper_rect`/`Rect`. The lifetime law is reflection-confirmed: a held `Page` list (`[out.new_page(...) for _ in range(sheets)]`) outliving its `Document` local faults the next draw with `AttributeError: 'NoneType' object has no attribute 'is_pdf'`, so the fold indexes the live `out[p.sheet]` exactly as the sibling `composition/sheet#SHEET` `Place` and `export/layered#LAYERED` `PdfOcg` arms do. The folder `.api` catalogue for `pymupdf` rows `subset_fonts`/`rewrite_images`/`scrub`/`set_xml_metadata`/`add_ocg`/`set_toc`/`embfile_add`/`bake`/`new_page`/`get_pixmap`/`tobytes` but does NOT yet row `show_pdf_page` (it rows the sibling `Page.insert_image`/`Document.insert_pdf` cross-document members), so that one member is catalogue-pending — the signature is reflection-verified against the installed distribution, the close-condition the folder catalogue adding the `show_pdf_page` row to the render/vector-drawing scope. The `show_pdf_page` cross-document draw is the higher-fidelity placement the imposition needs (it vector-copies the source page content stream into the imposed cell, preserving text/vector rather than rasterizing), distinct from the `pypdf.Transformation`+`merge_page` fold the `document/egress#FINISH` `IMPOSE` step uses for in-document n-up.
- [SCHEME_PLACEMENT_SETTLED]: the placement computation for each scheme is one `Place` row in the `frozendict[Scheme, Place]` policy table returning a `Block[Placement]`, the totality proof — `_grid` (document-order sequence to the next binding-aware cell, the `NUP` row AND the `PERFECT_BIND` row since perfect-bind is `_grid` over a `Geometry` carrying a `spine` allowance, two schemes differing by a field not a function), `_folded` (the centerfold computation padding the page count to the next multiple of `4·leaves`, folding the `_saddle` pairing `(last, first, second, second-last, ...)` so the bound stack reads in sequence, carrying the per-row 180° head-to-head rotation `180·((slot % slots) // across) % 360`, the per-fold `Geometry.creep` outward shift to compensate fold-thickness drift, and the `sig-N` OCG layer name per signature group, the `BOOKLET` row a `leaves=1` `structs.replace` over `_folded` and the `SIGNATURE` row `_folded` direct over `Geometry.leaves`), `_duplexed(on_across)` (work-and-turn AND work-and-tumble single-plate duplex — both halves on one sheet, the back side mirrored across the row for the turn and down the column for the tumble, the two schemes one parameterized body differing only by the `on_across` axis policy value, never two parallel functions), `_stacked` (cut-and-stack — sequential pages distributed down each guillotine-cut pile), `_paired` (come-and-go — each page duplicated across the sheet's slots so a single set tumbles into two finished copies), `_split` (sheetwise — recto and verso assigned to separate plates `2·(page // slots) + page % 2`). Each scheme returns the full `Placement` `Block` directly — source, sheet, cell, rotate, fit, clip, name, visible, locked fused — so the draw fold reads one `Block`, never a second order/cell/rotate join, and the imposed-sheet count derives from `_sheet_count(placements)` (the `Block.fold` max over `sheet` + 1), structurally unable to drift from the placement. The `Geometry.partition(shift)` projection is the shared sheet-geometry surface every scheme reads with every field live: `oriented` swaps the dimensions on `landscape`, `binding` selects the gutter edge and the creep sign, `spine` widens the bind-edge origin for perfect-bind glue, `head_trim` insets the top/bottom finished trim, `creep` shifts the cell grid per fold, `bleed` expands the cell by the trim margin, `gutter` spaces the cells. The split is the disciplined collapse: a per-scheme `ImposeOp` case differing only by the literal scheme and a `leaves` knob, a parallel `_impose_nup`/`_impose_booklet`/`_impose_signature` method triple beside the one `PLANS`-keyed placement fold, a degenerate one-page-per-sheet `_flat` beside the `_grid` perfect-bind row, a separate `sheets` callable per scheme that drifts from the placement, and an `if scheme == ...` cascade beside the table dispatch are the rejected forms.
- [RAIL_NARROWING_SETTLED]: the fault rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` minted ONCE at `async_boundary(f"impose.{op.tag}", self._keyed, catch=_FAULTS)` where `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` is the real engine raise tuple — `pymupdf.FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or zero-page source, a `PLANS` `KeyError`, a malformed `Rect(*cell)` extent `ValueError`, a non-positive-grid / off-axis-rotation `BeartypeCallHintViolation` from the `_GUARD`-contracted `_imposed` seam, and the source stream `OSError` — so the `runtime/faults#FAULTS` `async_boundary[T](subject, thunk, *, catch=...)` `_convert` folds each caught cause through `BoundaryFault.of(subject, cause)` into its own structurally-addressable case (the `BeartypeCallHintViolation` landing as `api`, `TimeoutError`-derived `OSError` ordering owned by the `CLASSIFY` table, the residual `RuntimeError`/`ValueError`/`KeyError` as the subject-carrying `boundary` catch-all), exactly as the sibling `composition/sheet#SHEET` and `composition/compose#COMPOSE` arms do, rather than the `Exception` catch-all the faults owner rejects. The prior fence's parallel `ImposeFault` `Literal` rail and its `_aspected`/`_contracted` weave were the deleted illusion: the interior returned `Result[Composed, ImposeFault]` the boundary never read, and `_keyed` then `raise ValueError(keyed.error)` round-tripped a railed fault back into a raise the default `catch=Exception` re-collapsed — the rail-to-raise-to-rail erasure the `rails-and-effects.md` boundary law forbids. The `_keyed` thunk computes `_composed(op).data` and mints `ContentIdentity.of(...)` directly; the interior raises the provider exception the boundary owns, never carries a second rail. The `_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))` shape contract is applied to `_imposed` — whose signature carries `Geometry` and `Block[Placement]` as DIRECT parameters, so beartype deep-checks their `Is`-refined `Across`/`Leaves`/`Span`/`Quarter` fields and rails a malformed grid count or off-axis verso rotation as the `api` `BoundaryFault` case BEFORE the native draw. A `@_GUARD` on `_composed(op: ImposeOp)` would be the dead-arm illusion the page rejects: beartype checks only that `op` is an `ImposeOp` (the type-checker already proves it) and never recurses into the `case()` tuple payloads where `Geometry`/`Placement` live, so the `BeartypeCallHintViolation` arm would be as structurally dead as the `Result[T, Never]` traverse the `[PLACEMENT_SWEEP_SETTLED]` row rejects — the contract must guard the seam where the refined shape is a real signature parameter.
- [PLACEMENT_SWEEP_SETTLED]: the draw is one `placements.map(lambda p: _draw_one(out, src, p)).choose(_oc_state)` sweep over the `Block[Placement]` writing each `show_pdf_page` onto the live native `out[p.sheet]`, `_configure_layers(out, minted)` writing the one `set_layer(0, on=, off=, locked=)` reader config over the kept OCG rows, and `marks.serialize(out)` running once on the all-placed document — the live-native-handle mutation the `boundaries.md` platform-forced statement seam names, exactly as the sibling `composition/sheet#SHEET` `_composed` and `export/layered#LAYERED` `_pdf_ocg` arms run it. The prior fence's `expression.extra.result.traverse(lambda p: _draw_one(...), placements)` over a `_draw_one -> Result[Document, Never]` was the deleted illusory-density form: the `Never` error half can never materialize (the draw rails nothing of its own, the provider raise the boundary catches replaces a per-element `Result` thread), so the `traverse` threaded a fail-fast applicative whose failure branch was structurally dead — decorative rail ceremony deciding nothing, the exact `Result[T, Never]`-over-`traverse` form the sibling `composition/compose#COMPOSE` `[ONE_FOLD_SETTLED]` row rejects. The prior fence further split the page allocation into a statement `for _ in range(sheets): out.new_page(...)` loop beside the `traverse` draw — two contradictory treatments of the one live-handle seam; the rebuild is one consistent native-handle mutation for both the allocation and the draw. `_draw_one` indexes the LIVE `out[p.sheet]` (the held-`Page`-list lifetime fault the `[SHOW_PDF_PAGE_VERIFIED]` row names), mints `out.add_ocg(p.name, on=p.visible, …)` only for a `name`-bearing placement, and returns its `(xref, visible, locked)` row; `Block.choose(_oc_state)` keeps the rows that minted a real xref and `_configure_layers` partitions that set into the `set_layer` `on`/`off`/`locked` arrays exactly as the sibling `composition/sheet#SHEET` arm does — a minted OCG group with no reader visibility/lock config behind it is the capability-thin rejected form. The `Marks.finished(out, geometry, sheets)` press pass folds the finish flags (`set_toc`/`embfile_add`/`bake`/`scrub`/`rewrite_images`/`subset_fonts`/`set_metadata`/`set_xml_metadata`) over the one document after the placement sweep, and `Marks.serialize(out)` runs the deterministic `tobytes(garbage=, deflate=, use_objstms=)` (`tobytes` is the in-memory egress; `ez_save`/`save` write a path) — the registration/crop-mark SHAPE emission lands in `composition/compose#COMPOSE`, this owner carrying only the OCG-bound `overlay` flag and the print-control mark names.
- [PROOF_AND_LAYERS_SETTLED]: the `Proof(source, dpi, sheet)` arm `open(stream=source)[sheet].get_pixmap(dpi=)` rasterizes the `sheet`-indexed imposed sheet to a PNG keyed by the same `ContentKey`, the `Pixmap.width`/`height` riding the `ArtifactReceipt.Preview` extent so a press-check or contact-sheet consumer proofs ANY imposed sheet — recto, verso, or an inner signature page — rather than re-imposing or only ever the first, the same raster-proof capability the sibling `composition/sheet#SHEET` `Preview` and `composition/compose#COMPOSE` arms expose, an imposition engine that cannot show any imposed sheet being capability-thin; the multi-sheet contact-strip proof is one further `Proof` field over the same arm. The `Imposition.layers(names) -> tuple[Layer, ...]` projection hands the imposed layout outward to `export/layered#LAYERED` exactly as `composition/sheet#SHEET` `Sheet.layers` does: one `Layer(name, source, bbox)` row per OCG-bound signature group (the `_folded` `sig-N` names deduped through a `Map.add` fold whose value is the `_union` of that group's cell boxes), each filling the REQUIRED `bbox` positional the `export/layered` `Layer(name, source, bbox, visible, locked)` row declares (the imposed signature groups uniformly visible and unlocked, the `visible`/`locked` defaults the export row carries) — so the imposed press form hands outward as named rows the export owner binds into OCG/SVG layers, the named-layer authoring landing once in the export owner and this owner projecting the rows. A `Proof` `Pdf` receipt over PNG bytes (a PDF evidence shape over a raster), a per-placement duplicate `Layer` row, and a layered-export arm grafted onto `ImposeOp` are the rejected forms.
- [EGRESS_DISTINCT] [RESOLVED]: this owner is the DEDICATED multi-sheet press-imposition engine, distinct from the `document/egress#FINISH` `IMPOSE` step. The egress IMPOSE is the in-document `pypdf.Transformation`+`merge_page` n-up grid fold over an already-finished PDF as one of ten finishing steps (encrypt/outline/watermark/attach/impose/rewrite/redact/sanitize/optimize/protect) — it imposes a finished PDF into a simple n-up grid and computes no booklet, signature, work-and-turn, creep, or verso rotation. This `Imposition` owner computes the saddle-stitch creep-compensated centerfold order, the folded-signature ordering with head-to-head rotation, the work-and-turn duplexing, and the come-and-go / perfect-bind / sheetwise schemes the egress step never reaches, placing each source page through the higher-fidelity `pymupdf.show_pdf_page` cross-document draw. The two never overlap: egress finishes one PDF as a finishing step, imposition is the press-imposition engine producing a folded form. The split is the disciplined collapse boundary: a signature-imposition branch grafted onto the egress `IMPOSE` step (which owns no scheme dispatch and no placement computation) and an n-up grid re-implemented here beside the egress `Transformation` fold are the rejected crossings — the simple in-document n-up lands once in egress, the multi-sheet press-imposition lands once here, and neither re-owns the other. The imposed press form this owner emits is handed to `document` assembly keyed by the same `ContentKey`; the egress n-up is handed onward as a finished PDF — the two feeds are distinct.
- [RECEIPT_THREAD_SETTLED]: the `Impose` op contributes `core/receipt#RECEIPT` through the receipt owner's named flat-scalar mints via `Imposition.contribute`, which reads the one `_composed(op)` fold (no recompute — the same fold `of` drives), mints the content key over the imposed bytes through the same `ContentIdentity.of` `of` uses (so the projection is self-contained with no caller-passed key), routes on the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count, and yields `receipt.contribute()` — the OCG-bearing form `ArtifactReceipt.Egress(key, len(composed.data), composed.sheets, 0, 0, composed.layers)` carrying the imposed byte count, imposed-sheet page count, and the minted-OCG layer count on the `overlays` slot (zero encryption-R / outline-depth — imposition is neither a security nor a navigation close, the same `overlays` slot the sibling `export/layered#LAYERED` `PdfOcg` and `composition/sheet#SHEET` `Place` arms report their authored-layer count on), and the degenerate `composed.layers == 0` no-mark form `ArtifactReceipt.Pdf(key, len(composed.data), composed.sheets)`. The `core/receipt#RECEIPT` owner's public surface IS these named per-kind static mints — each keyword-constructs its flat-scalar `case()` `(ContentKey, <scalars>)` — so a per-kind facts `Struct` (`EgressFacts`/`PdfFacts`/`PreviewFacts`) re-wrapping the scalars the named mint already takes positionally, and an `of(key, facts)` indirection over such a struct, are the exact forms the receipt owner's Boundary row rejects; the imposition owner calls `ArtifactReceipt.Egress(key, bytes, pages, …)` directly and adds no receipt case. The `Proof` op selects `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])` carrying the pixmap extent. The `Plan` compute-only op contributes NO receipt — a `0`-sheet `Pdf` receipt over the `ImposedPlan` JSON bytes is the deleted phantom (a PDF evidence shape whose bytes are JSON, not a PDF); the pre-flight projection rides `Imposition.planned() -> ImposedPlan` (the `scheme`/`sheets`/`pages`/`leaves`/`signatures`/`padded`/`creep`/placement-rows a press operator reads to validate the imposition before committing the draw), not a fabricated receipt.
```
