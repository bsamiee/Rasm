# [PY_ARTIFACTS_SHEET]

The architectural sheet-set owner placing emitted figures into a titled, framed, field-bound drawing sheet. `Sheet` is ONE owner over the title-block / drawing-frame / field-binding pipeline carrying a closed-payload `SheetOp` `expression.tagged_union` — each case its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` — dispatched by one total `match` and folded ONCE into a `Composed` evidence struct the `of`/`contribute`/`layers` projections share. No OSS sheet-set library exists, so this owner composes the sheet algebra over three admitted PDF authoring engines selected by one `Engine` policy row, every engine driven by ONE `Standard` archival/accessibility profile resolved through the `_STANDARD` token table (never a per-engine `archival: bool`): `reportlab` `Canvas`/`PDFTextObject`/`PDFPathObject` authors the frame, the `_zones`-derived ISO 5457 / ASME Y14.1 border reference grid, the `TitleBlock.cells`-derived title-block grid, and the revision table at absolute sheet coordinates with the `pdfVersion`/`lang` conformance-REQUEST row; `typst` markup `compile` authors the same titled frame from a templated source with its own compiler-owned `pdf_standards` PDF/A close; `weasyprint` `HTML.write_pdf` authors a CSS-templated frame and owns the full close end to end — `pdf_variant` selecting the profile, `pdf_tags` writing the PDF/UA structure tree, and the supplied `OutputIntent` ICC embedded through `output_intent=`. The `Standard` member states the conformance INTENT the frame author requests; `reportlab` carries only the version pin (its PDF/A output-intent/structure-tree close routes to `exchange/conformance#CONFORMANCE`), `typst` and `weasyprint` carry their own native PDF/A close, and `Standard.PRESERVED` drives the PDF/A-3b + PDF/UA-1 archival-and-accessible profile wherever the engine owns the close — never an `output_intent=None` hardcoded against a profile the variant requires an ICC for. `pymupdf.Page.show_pdf_page` then vector-copies each already-emitted figure PDF into a `Rect` carved from the drawing region and binds it to a freshly minted `add_ocg` optional-content group so the placed sheet is itself layer-separable in a reader — a sheet is one frame plus N OCG-bound placed figures, never a per-figure draw family.

The fault rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(subject, self._keyed, catch=_FAULTS)` over the real engine raise tuple `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` — `RuntimeError` admits `typst.TypstError`/`pymupdf.FileDataError`/`EmptyFileError`, `ValueError` a reportlab/weasyprint author raise, `KeyError` a `_SIZES`/`_FRAME`/`_STANDARD` miss, `OSError` an engine font/resource file-load fault, and `BeartypeCallHintViolation` a malformed `TitleBlock`/`FigurePlacement`/`Box` the `_GUARD`-contracted `_composed` fold rejects — each discriminated into its own `BoundaryFault` case rather than the `Exception` catch-all the faults owner rejects, matching the sibling `composition/imposition#IMPOSE` fault tuple one-for-one. The interior is a pure `_GUARD`-contracted expression fold raising nothing of its own: the engine arms compute over the admitted owner, and a malformed source, a frame-author failure, a `_SIZES`/`_FRAME`/`_STANDARD` miss, or a contract violation each surface as the provider's own raise the boundary converts — never an inline `try`/`except`/`for` ladder beside the fold, never a parallel `SheetFault` `Literal` the boundary never reads, and never a parallel `_aspected` contract factory beside the boundary's `CLASSIFY` row. Cancellation is excluded from `_FAULTS` and re-raises as the structured signal.

`SheetSize` is the closed `StrEnum` over the named ANSI A-E, ISO A0-A5, ARCH A-E1, and JIS-B formats the architectural/engineering sheet-set spans, its `_SIZES` `frozendict` the single point-dimension correspondence every consumer derives from — the ARCH/ANSI/JIS-B span exceeds the `reportlab.lib.pagesizes`/`pymupdf.paper_rect` built-in tables, so this owner carries the full correspondence, and the `Frame` case's `landscape` axis projects each through `_oriented` so an architectural sheet draws landscape, never a per-arm dimension literal; the `_zones` projection derives the ISO 5457 / ASME Y14.1 border reference grid from those same oriented dimensions so a drawing location is cited by zone (a `detail C-4` reference), the frame element a bare border rect omits. `TitleBlock` is the frozen owner carrying the full title-block concept — the `FieldRow` field grid, the `Revision` history table, the drawing `Scale` (its `ratio` plus the `bar(x, y)` divided graphic-scale ruler the reportlab author draws), the `NorthArrow`/`KeyPlan` graphic cells, the issue `status` purpose and `sheet_total` set count, and the discipline/project metadata — whose `cells(dims)` projection is the ONE field-rect correspondence the frame author draws labels at AND the `Fill` arm binds values INTO, so a bound value lands in the exact rect its label was drawn at rather than at a guessed offset disconnected from the frame. `FigurePlacement` is the closed-payload value object each placement carries — the figure PDF bytes, the bare drawing-region `Box` rect, the OCG `name`, and the `keep_proportion`/`rotate`/`overlay`/`clip`/`layered`/`visible`/`locked` placement axis — folded by `pymupdf.Page.show_pdf_page` over the placement `Block`, bound to a minted `add_ocg` group, and driven to its reader visibility/lock state through one `Document.set_layer` ui config so a multi-figure sheet is one OCG-layered, toggle-and-lock-controlled draw fold, never a per-figure draw call and never a phantom `oc` field with no group to bind to.

One sheet surface discriminating the operation. `reportlab`/`pymupdf` resolve on the cp315 core and `typst`/`weasyprint` import on the core, so no sheet arm crosses a process seam; every arm imports its engine at boundary scope. The figure PDFs the `Place` arm draws arrive already-emitted from `composition/compose#COMPOSE` and `visualization/chart#EXPORT` — this owner authors the frame and places the figures but re-renders no figure. The titled sheet is handed to `composition/imposition#IMPOSE` for the multi-sheet press form and projects `Sheet.layers` as `tuple[Layer, ...]` (frame layer plus one per placed figure) to `export/layered#LAYERED`, so the per-sheet authoring half stays here and the imposition and named-layer halves land outward. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` through the receipt owner's named flat-scalar mint that matches the `Composed.kind` discriminant — the `Frame`/`Fill`/`Stamp` arms `ArtifactReceipt.Pdf(key, bytes, pages)` carrying the sheet byte count and the REAL `Document.page_count`, the OCG-bearing `Place` arm `ArtifactReceipt.Egress(key, bytes, pages, 0, 0, overlays)` carrying the minted-layer count on the `overlays` slot exactly as the sibling `export/layered#LAYERED` `PdfOcg` arm does, and the raster `Preview` arm `ArtifactReceipt.Preview(key, width, height)` carrying the pixmap extent — the `Composed.kind` plus `layers` count selecting the mint once off the one `Composed` fold, `receipt.contribute()` projecting it, never a second render, never a per-kind facts `Struct` re-wrapping the scalars the mint already takes positionally, and never a phantom `ArtifactReceipt.of(key, facts)` the receipt owner does not declare.

## [01]-[INDEX]

- [01]-[SHEET]: architectural sheet-set / title-block / drawing-frame owner over the closed-payload `SheetOp` `tagged_union` folded once into a `Composed` evidence struct through the `_GUARD`-contracted `_composed` fold, rail-typed `RuntimeRail[ContentKey]` over `async_boundary(catch=_FAULTS)`, dispatched to `reportlab` (`Canvas`/`PDFTextObject`/`PDFPathObject` coordinate frame + `cells`-derived title-block authoring + the `Scale.bar` divided graphic-scale ruler + the `_zones`-derived ISO 5457 border reference grid drawn through `drawCentredString`, core), `typst` (`compile` markup frame with compiler-native `pdf_standards` PDF/A close, core), `weasyprint` (`HTML.write_pdf` CSS frame owning its full PDF/A close — `pdf_variant`/`pdf_tags`/`output_intent=` ICC, core), and `pymupdf` (`Page.show_pdf_page`+`add_ocg`+`set_layer` OCG-bound, visibility/lock-configured figure placement, `insert_textbox` field bind into the derived cell rect, `set_metadata`/`set_xml_metadata`/`set_toc`/`embfile_add` stamp, `get_pixmap` preview, core); the stdlib `xml.etree.ElementTree` XMP packet build for the `Stamp` arm (`lxml` is `python_version<'3.15'`-gated off the cp315 core); `Engine`/`SheetSize`/`Standard`/`OutputIntent`/`TitleBlock`/`Revision`/`FieldRow`/`Scale`/`NorthArrow`/`KeyPlan`/`FigurePlacement` the closed engine/format/profile/title/placement family; the `_STANDARD` conformance-request token table, the `_PDFAID` PDF/A part/conformance table, the `TitleBlock.cells` group-ordered field-rect correspondence, and the `_zones` ISO 5457 border reference grid the derived-geometry owners; the `Composed` fold the one render path `of`/`contribute`/`layers` read; projects `Sheet.layers` to `export/layered#LAYERED` `Layer`; threads `core/receipt#RECEIPT` through the receipt owner's named `ArtifactReceipt.Pdf`/`Egress`/`Preview` flat-scalar mint selected by the `Composed.kind` discriminant.

## [02]-[SHEET]

- Owner: `Sheet` the one architectural-sheet owner discriminating operation over the closed `SheetOp` `expression.tagged_union`, every arm folded ONCE into the `Composed` evidence struct (`data` bytes, `pages` the real `Document.page_count`, `kind` the `Artifact` receipt-shape discriminant) the `of`/`contribute`/`layers` projections share — no second `match` re-renders the PDF and no `@cache` memo stands in for the one fold. `Engine` is the closed `StrEnum` over the three frame-authoring engines (`REPORTLAB`/`TYPST`/`WEASYPRINT`), each a `_FRAME` `frozendict[Engine, Author]` row binding its frame author so the `Frame` arm dispatches by member; `Standard` is the closed `StrEnum` over the archival/accessibility profiles (`DRAFT`/`ARCHIVAL`/`ACCESSIBLE`/`PRESERVED`), its `_STANDARD` `frozendict[Standard, StandardRow]` the ONE profile-to-conformance-request-token correspondence every author derives from (`reportlab` `pdfVersion` tuple — the version pin only, its PDF/A close routing to `exchange/conformance#CONFORMANCE`; `typst` `pdf_standards` sequence; `weasyprint` `pdf_variant` string plus `pdf_tags` flag, the supplied `OutputIntent` ICC threaded into `output_intent=` where the variant requires it) and the `_PDFAID` `frozendict[Standard, tuple[str, str] | None]` the PDF/A part/conformance the `Stamp` XMP packet declares, so a fourth profile is one member plus one `_STANDARD` row plus one `_PDFAID` row, never a per-arm archival literal, the boolean `archival` flag a 4-state preservation axis would smuggle into two bodies, or an `output_intent=None` hardcoded against a profile the variant requires an ICC for. `SheetSize` is the closed `StrEnum` over the ANSI A-E / ISO A0-A5 / ARCH A-E1 / JIS-B formats whose `_SIZES` `frozendict[SheetSize, Dimensions]` is the one point-dimension correspondence the architectural span (broader than the `reportlab.lib.pagesizes`/`pymupdf.paper_rect` built-in tables) forces this owner to carry, never a per-arm literal; the `Frame` case's `landscape` axis swaps it through `_oriented` so an architectural sheet draws landscape, and `Fill` reads the emitted page's real `rect` rather than re-deriving the orientation. `TitleBlock` is the frozen owner carrying the whole title-block concept — the `FieldRow` grid, the `Revision` history `Block`, the drawing `Scale`, the `NorthArrow`/`KeyPlan` graphic cells, the issue `status` purpose and `sheet_total` set count, and the project/discipline metadata — whose `cells(dims)` projection is the ONE field-rect correspondence (top-left origin, derived once from the sheet dimensions and the `_BLOCK_*`/`_ROW_H` block geometry) the `_FRAME` author draws labels at and the `Fill` arm binds values INTO, so the label position and the bound-value position are the same rect rather than two divergent coordinate guesses; `grid()`/`history()`/`outline()`/`metadata()` derive the row stream, revision rows, outline entries, and info dict from that one owner, and `FieldRow` is its grid-cell value object (label, value, span, group) folding one field as one row — the `group` zone load-bearing in `cells()`, which group-key sorts the rows through `Block.sort_with` so a title block's `identity` and `approval` zones stay contiguous rather than interleaved, never a decorative dead field. `FigurePlacement` is the closed-payload value object each placement carries — figure PDF bytes, the bare drawing-region `Box` rect, the OCG `name`, and `keep_proportion`/`rotate`/`overlay`/`clip`/`layered`/`visible`/`locked` — folded by `pymupdf.Page.show_pdf_page` over the placement `Block` and bound to a minted `add_ocg` group whose default-on and reader-lock state the one `Document.set_layer` ui-config write drives (the `visible`/`locked` axes mirroring `export/layered#LAYERED` `Layer.visible`/`Layer.locked` one-for-one so a placed sheet is toggle-and-lock-controlled in a reader, not merely separable), the rect a bare `tuple[float, ...]` exactly as the sibling `composition/imposition#IMPOSE` `Placement.cell` and `export/layered#LAYERED` `Layer.bbox` carry it, never a one-field wrapper standing in for the scalar the placement already owns. `Scale` carries the drawing scale as both the `ratio` string the grid prints and the `bar(x, y)` projection of a divided graphic-scale ruler — alternating filled/clear division rects over `bar_length`/`segments` plus the `units` caption — the reportlab author draws as vector marks, so a graphic scale bar is a real drawn title-block cell rather than a dead field. `reportlab` owns the `Canvas` over a `BytesIO` (`beginText`/`PDFTextObject.textLines` the batched grid-plus-revision text, `beginPath`/`PDFPathObject.rect` the frame border, `rect`/`drawString` the `Scale.bar` divided ruler, `drawCentredString` the `_zones` ISO 5457 border reference-grid labels, `drawImage`/`lib.utils.ImageReader` the `NorthArrow`/`KeyPlan` raster cells, `Canvas.save()` the byte egress — the `platypus.Table`/`getpdfdata` flowable surfaces are the available data-shaped alternative the text-object form supersedes); `typst` owns `compile(..., pdf_standards=)` markup authoring; `weasyprint` owns `HTML.write_pdf(..., pdf_variant=, pdf_tags=, output_intent=)`; `pymupdf` owns `show_pdf_page`/`add_ocg`/`set_layer`/`new_page`/`paper_rect`/`insert_textbox`/`get_pixmap`/`set_metadata`/`set_xml_metadata`/`set_toc`/`embfile_add`/`tobytes`/`page_count`; no sheet-set library is admitted, so the sheet algebra (frame geometry, grid layout, region partition, cell placement, divided-bar geometry) is this owner's composition over those engines, never a re-implemented byte emitter.
- Cases: `SheetOp` cases — `Frame(size, engine, title, standard, output_intent, landscape)` (author the framed titled sheet at `_oriented(_SIZES[size], landscape)` — the `_FRAME[engine]` author draws the border, the `_zones`-derived ISO 5457 border reference grid (the reportlab coordinate author's vector marks; the templated `typst`/`weasyprint` authors carry the title block alone), the `TitleBlock.cells`-derived title-block grid, the `Revision` history, and the `NorthArrow`/`KeyPlan` cells at the derived field rects over the engine's own surface (`reportlab` `PDFPathObject`+`PDFTextObject`, `typst` `grid`/`grid.cell`, `weasyprint` CSS `<table>`), the `Standard` member routing the engine's `pdf_standards`/`pdf_variant`/`pdfVersion` conformance-request token through the `_STANDARD` row and the supplied `OutputIntent` ICC into the `output_intent=` slot of the engine that owns its PDF/A close, returning the empty framed sheet PDF) · `Place(sheet, placements)` (vector-copy N already-emitted figure PDFs into the drawing region — open the sheet through `pymupdf.open`, mint one `add_ocg(name, on=visible)` group per layered placement, draw each `FigurePlacement` over `Page.show_pdf_page(Rect(*box), docsrc, pno, keep_proportion=, rotate=, clip=, oc=xref)` on the one live native page so the placed figure is bound to its named optional-content group, then drive one `Document.set_layer(0, on=, off=, locked=)` ui config over the minted xref partitions so the reader's layer panel toggles and locks each placed figure; a malformed source raises the `pymupdf.FileDataError` the `_FAULTS` tuple admits, returning the OCG-layered figure-placed PDF and the minted-layer count) · `Fill(sheet, title)` (bind title-block field VALUES into the EXACT rects their labels were drawn at — read the emitted sheet's real `Page.rect` as the authoritative oriented geometry, resolve the SAME `TitleBlock` the frame drew through `title.cells((page.rect.width, page.rect.height))` for the one field-rect correspondence, and draw each value through `Page.insert_textbox` into its derived rect; the full `TitleBlock` reproduces the identical `grid()` head + extension ordering so a head-zone value (Project, Sheet, Title) lands at its drawn label — a loose `fields=rows` reconstruction offsets every value past the ten fixed head fields and is the deleted illusory-alignment form, and a re-passed `size` that mismatches the drawn frame is too) · `Stamp(sheet, title, standard, output_intent, attachments)` (bind document metadata + outline + XMP + source files — `Document.set_metadata` writes the `metadata` info dict, `set_toc` the `TitleBlock.outline()` entries, `embfile_add` attaches each source CAD/model/data file AND the supplied `OutputIntent` ICC as PDF associated files the conformance leg consumes, `set_xml_metadata` the stdlib-`xml.etree.ElementTree`-built archival XMP packet carrying the `_PDFAID` PDF/A part/conformance on a non-`DRAFT` `Standard`) · `Preview(sheet, dpi)` (project a raster preview — `Page.get_pixmap(dpi=)` rasterizes the sheet to PNG keyed by the same `ContentKey`, the `Pixmap.width`/`height` riding the `Preview` receipt, the thumbnail a downstream contact-sheet or proof consumer reads) — matched by one total `match`/`case` lowering to the one `Composed` fold; never a per-discipline sheet-builder sibling, never a per-engine `_emit` method, never a per-figure draw call.
- Entry: `Sheet.of` is `async` over `async_boundary(f"sheet.{op.tag}", self._keyed, catch=_FAULTS)` where `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` is the real engine raise tuple the providers throw — `typst.TypstError`/`pymupdf.FileDataError`/`pymupdf.EmptyFileError` (all `RuntimeError`-derived), a reportlab/weasyprint author raise, a `_SIZES`/`_FRAME`/`_STANDARD` key miss, an engine font/resource file-load fault, and the `_GUARD`-contracted malformed `TitleBlock`/`FigurePlacement`/`Box` violation — so the boundary discriminates each into its own `BoundaryFault` case rather than the `Exception` catch-all the `runtime/faults#FAULTS` owner rejects (cancellation excluded, re-raised as the structured signal). `_keyed` is the awaitable thunk computing the one `Composed` fold and minting `ContentIdentity.of(f"sheet-{op.tag}", composed.data)` — the boundary catches the provider raise and converts, so the interior raises nothing of its own and returns `ContentKey`, never a `Result` nested inside the `RuntimeRail` the boundary already owns. Every arm resolves synchronously on the cp315 core inside the async capsule — `reportlab`/`pymupdf`/`typst`/`weasyprint` all import on the core band at boundary scope inside each arm — so no sheet arm crosses a process seam and the `async_boundary` is the uniform contract `composition/imposition#IMPOSE` and `document` `await`.
- Auto: `_composed(op) -> Composed` is the ONE `_GUARD`-contracted total `match` over `SheetOp` both `of` and `contribute` read — no second render: the `Frame` arm calls `_FRAME[engine]` over `_oriented(_SIZES[size], landscape)`, the `TitleBlock`, and the `Standard` member, the author drawing labels at the `TitleBlock.cells` rects (the reportlab author also drawing the `_zones` ISO 5457 border reference grid at the same derived dimensions) and routing the `_STANDARD` token, any reportlab/typst/weasyprint failure propagating as the provider raise the boundary admits; the `Place` arm opens the sheet through `pymupdf.open`, folds `_draw_one` over the placements where each `_draw_one` mints `add_ocg(name, on=visible)` for a layered placement and draws `show_pdf_page(Rect(*box), docsrc, pno=, keep_proportion=, rotate=, clip=, oc=xref)` on the one live native page returning its `(xref, visible, locked)` row, `Block.choose(_oc_state)` keeps the rows that minted a real xref, and one `_configure_layers` `set_layer(0, on=, off=, locked=)` writes the reader visibility/lock config before `tobytes` runs once on the placed page so `Composed.layers` carries `len(minted)`; the `Fill` arm reads the emitted page's real `rect` and resolves the supplied `TitleBlock` through `title.cells((page.rect.width, page.rect.height))` once, drawing each cell's value through `Page.insert_textbox` into its derived rect; the `Stamp` arm binds `set_metadata`/`set_toc`/`embfile_add`/`set_xml_metadata`; the `Preview` arm `get_pixmap(dpi=)` then `Pixmap.tobytes("png")`, carrying the pixmap `width`/`height` onto the `Composed.kind` raster discriminant. The placement draws sweep the live `pymupdf` page through `_draw_one` and `Block.choose` only collects the minted OCG rows the one `set_layer` config consumes — the native handle the engine mutates in place is the platform-forced statement seam `boundaries.md` names, so a `Block.fold` over a discarded mutated-document result is the index-threaded fold the rails page rejects, not the dense form. Each arm returns `Composed(data, pages=doc.page_count, kind=, layers=)` reading the REAL `Document.page_count` off the live document (the `Preview` raster's `kind` carrying the pixmap extent, the `Place` arm's `layers` the OCG count), so the body stays one `match`-shaped path — never an inline `try`/`except` ladder beside it, never a `@cache` memo standing in for the one fold, and never a second `match` re-rendering the PDF for the receipt.
- Receipt: each operation contributes `core/receipt#RECEIPT` off the one `Composed` fold through the receipt owner's named flat-scalar mint — the `Frame`/`Fill`/`Stamp` arms `ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)` carrying byte count and the REAL page count read off `Document.page_count`, the OCG-bearing `Place` arm `ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)` carrying the minted-layer count on the `overlays` slot (zero encryption-R/outline-depth — placement is neither a security nor a navigation close, exactly the slot the sibling `export/layered#LAYERED` `PdfOcg` arm reports its authored-layer count on), and the raster `Preview` arm `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])` carrying the pixmap extent; the `Composed.kind` discriminant plus the `Composed.layers` count select the mint once and `receipt.contribute()` projects it, so the sheet owner adds NO sibling receipt type and NO new receipt case — the receipt owner's `Pdf`/`Egress`/`Preview` are the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the flat positional shape `_facts` zips against its `_KEYS` row. `contribute` reads the SAME `_composed(op)` the `of` projection reads (the same fold both entrypoints call at their lifecycle stage, the page count the live document's, never a hardcoded `1`), mints the content key over `composed.data` through the same `ContentIdentity.of` `_keyed` uses, and yields the selected mint's `contribute()` — never a per-kind facts `Struct` (`PdfFacts`/`EgressFacts`/`PreviewFacts`) re-wrapping the scalars the mint already takes positionally, never a phantom `ArtifactReceipt.of(key, facts)` the receipt owner explicitly rejects, never a parallel sheet-receipt type, never a second full render, and never a zero-byte placeholder hand-built in the projection.
- Packages: `reportlab` (`pdfgen.canvas.Canvas(BytesIO(), pagesize=, pdfVersion=)`, `beginText`/`PDFTextObject.textLines`/`drawText` batched grid-plus-revision text, `beginPath`/`PDFPathObject.rect`/`drawPath`/`rect` frame border, `rect`/`drawString` the divided `Scale.bar` graphic-scale ruler, `drawCentredString` the `_zones` ISO 5457 border reference-grid labels, `drawImage`/`lib.utils.ImageReader` north-arrow/key-plan raster, `Canvas(..., pdfVersion=, lang=)` the archival version+language pin, `Canvas.save()` over `BytesIO` egress, BSD core + cp315 accelerators) on the cp315 core; `typst` (`compile(input, output=None, format="pdf", pdf_standards=, ignore_system_fonts=, font_paths=, timestamp=)`, the `page`/`grid`/`grid.cell`/`line`/`rect`/`text` markup vocabulary, `Compiler` reusable world, `TypstError` `RuntimeError`-derived carrier, Apache-2.0 cp38-abi3) on the cp315 core; `weasyprint` (`HTML(string=, base_url=).write_pdf(target=None, font_config=, pdf_variant=, pdf_tags=, pdf_forms=, output_intent=, custom_metadata=, attachments=)` — `output_intent` taking the supplied `OutputIntent` ICC stream the `pdf_variant` PDF/A profile requires, `FontConfiguration` per-render registry, BSD `py3-none-any`) on the cp315 core; the stdlib `xml.etree.ElementTree` (`Element`/`SubElement`/`QName`/`register_namespace`/`tostring` building the archival XMP `pdfaid`/`dc` packet through the element tree, never a hand-concatenated string the `language.md` STRING_RECOVERY law rejects, cp315-native because `lxml` is `python_version<'3.15'`-gated off the core) for the `Stamp` arm's `_xmp` packet; `pymupdf` (`open(stream=)`/`Page.show_pdf_page(rect, docsrc, pno, keep_proportion, overlay, oc, rotate, clip)`/`Page.rect` the emitted sheet's authoritative oriented geometry/`Page.insert_textbox(rect, text, fontname, fontsize, align)`/`Page.get_pixmap(dpi=)`/`Pixmap.tobytes`/`Pixmap.width`/`Pixmap.height`/`Rect`/`Document.add_ocg(name, on=, intent=, usage=)`/`Document.set_layer(config, on=, off=, locked=)`/`Document.set_metadata`/`set_xml_metadata`/`set_toc`/`embfile_add`/`tobytes`/`page_count`, `FileDataError`/`EmptyFileError` `RuntimeError`-derived faults, native MuPDF cp310-abi3, AGPL-3.0 internal) on the cp315 core; `msgspec` (`Struct` frozen rows for the `TitleBlock`/`FieldRow`/`FigurePlacement`/`Composed` family); `expression` (`tagged_union`/`tag`/`case` the closed `SheetOp`, `Option`/`Some`/`Nothing` the `_oc_state` mint, `Block`/`Block.of_seq`/`Block.choose`/`Block.is_empty`/`Block.sort_with`/`Block.take`/`Block.append` the minted-OCG fold and the title-block grid order); `core/receipt#RECEIPT` (the named `ArtifactReceipt.Pdf(key, bytes, pages)`/`Egress(key, bytes, pages, encryption_r, outline_depth, overlays)`/`Preview(key, width, height)` flat-scalar mints selected by `Composed.kind`, `contribute()` the projection); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary` under the narrowed `catch=_FAULTS` raise tuple); projects `export/layered#LAYERED` `Layer`. The `beartype` contract weave is the `async_boundary` `CLASSIFY` `api` row's `BeartypeCallHintViolation`-to-`BoundaryFault` fold the runtime owner already legislates, so this page mints no parallel `_aspected` contract factory.
- Growth: a new sheet format is one `SheetSize` member plus one `_SIZES` row — never a per-arm dimension literal; a sheet orientation is the `Frame` case's `landscape` axis projected through `_oriented`, never a doubled `*_LANDSCAPE` enum; a new title-block field is one `FieldRow` in the existing grid `Block` (the issue `status` and the `sheet_total` set count are head fields `grid()` emits), a new revision one `Revision` in the history `Block` — never a `TitleBlock` parameter or a per-field draw call; a new frame-authoring engine is one `Engine` member plus one `_FRAME` row binding its `Author` — never a parallel sheet class; a new placement axis (a clipped detail callout, a rotated section, an OCG-bound layer, a reader-locked layer) is one `FigurePlacement` field carried into the existing `show_pdf_page`/`set_layer` fold (`clip`/`oc`/`visible`/`locked` are already rows on the member); a new graphic cell (a revision triangle, a discipline stamp) is one field on `TitleBlock` projected once and drawn at the one `_FRAME` author exactly as `Scale.bar` projects the divided ruler and `_zones` projects the border reference grid the reportlab author draws; a border-grid convention change (a finer zone pitch, a centring-mark family) is the `_zones` projection over the `_ZONE` policy value, never a per-arm grid literal; a new receipt shape is one `Artifact` `Composed.kind` discriminant arm breaking the `contribute` `match` at type-check; a new engine raise is one type in the `_FAULTS` module tuple; a new archival profile is one `Standard` member plus one `_STANDARD` conformance-request row plus one `_PDFAID` part/conformance row; a new colour intent (a CMYK output-intent for a print profile) is the supplied `OutputIntent` ICC threaded into the engine that owns its close, never a hardcoded `output_intent=None`. Zero new surface.
- Boundary: this owner authors a single titled sheet and imposes none — a per-discipline sheet-builder family (`ArchSheet`/`StructSheet`/`MepSheet`), an `if engine == "reportlab"` branch beside the `_FRAME` dispatch, a per-format literal beside `_SIZES`, a per-figure `show_pdf_page` call beside the one fold, a per-field draw call beside the `FieldRow` fold, a one-field wrapper beside the bare placement `Box`, a hand-concatenated XMP string beside the `xml.etree.ElementTree` element tree, an `output_intent=None` hardcoded against a PDF/A `Standard`, a hand-rolled byte emitter beside the engines, a parallel `SheetFault` `Literal` the boundary never reads, an `Exception` catch-all where `_FAULTS` names the real raise tuple, an inline `try`/`except`/`for` ladder beside the pure fold, a `@cache` memo or a second `match` re-rendering the PDF for the receipt beside the one `Composed` fold, and a hardcoded `pages=1` beside the live `Document.page_count` are the deleted forms; no UI, no live viewer, no figure re-render, no chart re-author. The figure PDFs the `Place` arm draws arrive already-emitted from `composition/compose#COMPOSE` and `visualization/chart#EXPORT` — drawn through `show_pdf_page`, re-rendered never. The multi-sheet n-up/booklet/signature imposition is `composition/imposition#IMPOSE`'s — a booklet fold grafted onto `SheetOp` and a signature emitter beside the one-page egress are the deleted forms. PDF security finishing (encrypt/sign/watermark) is `document/egress#FINISH`'s; the archival PDF/A conformance REQUEST rides the engine's standards row at frame-author time (`weasyprint`/`typst` owning their native PDF/A close, `reportlab` only the version pin), and the PDF/A VALIDATION close — output-intent ICC verification, structure-tree audit, PAdES signing — routes to `exchange/conformance#CONFORMANCE`, never a parallel signer or validator here; the supplied `OutputIntent` ICC the variant requires is embedded by the engine that owns its close (`weasyprint` `output_intent=`) or carried as an associated file the conformance leg consumes (the `Stamp` arm's `embfile_add`), never an `output_intent=None` masquerading as conformance. The placed layout projects outward as `Sheet.layers -> tuple[Layer, ...]` (the frame layer plus one `Layer(name, source, bbox, visible, locked)` per placed figure carrying its placement `Box` extent and threading the `FigurePlacement.visible`/`locked` axes straight onto the row rather than a guessed `locked=not layered`) to `export/layered#LAYERED` for the named-layer OCG/SVG hand-off, reading the one `_composed` fold rather than re-rendering the frame — a layered-export arm grafted onto `SheetOp`, a per-layer writer here, and a frame re-render inside `layers` are the deleted forms. All engines resolve on the cp315 core and import at boundary scope; the AGPL `pymupdf` placement leg is the internal pipeline per the rail's license constraint. The content key mints over the emitted bytes through `ContentIdentity.of`, never re-minted off a source figure key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from html import escape
from io import BytesIO
from typing import TYPE_CHECKING, Literal, assert_never

from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from builtins import frozendict
from expression import Nothing, Option, Some, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

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
type FieldCell = tuple["FieldRow", Box]  # one title-block field bound to its derived top-left-origin rect
type OutputIntent = tuple[str, bytes] | None  # (icc-name, sRGB/CMYK ICC profile bytes) the PDF/A output-intent embeds
type Author = Callable[[Dimensions, "TitleBlock", "Standard", OutputIntent], bytes]

# pymupdf raises `FileDataError`/`EmptyFileError` (both `RuntimeError`-derived) on a corrupt or
# zero-page source, the `_GUARD` shape contract raises `BeartypeCallHintViolation` on a malformed
# `TitleBlock`/`FigurePlacement`/`Box`, a `_SIZES`/`_FRAME` key miss raises `KeyError`, a
# reportlab/weasyprint/typst author raise is `RuntimeError`/`ValueError`, and an engine font/resource
# file-load fault is `OSError`. The boundary narrows on this real raise tuple so `async_boundary`
# discriminates each into its `BoundaryFault` case rather than the `Exception` catch-all the
# runtime faults owner rejects; cancellation is excluded and re-raises as the structured signal.
_FAULTS: tuple[type[BaseException], ...] = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)


class Engine(StrEnum):
    REPORTLAB = "reportlab"
    TYPST = "typst"
    WEASYPRINT = "weasyprint"


class Standard(StrEnum):  # the closed archival/accessibility profile — a vocabulary member, never an `archival: bool`
    DRAFT = "draft"  # no conformance pin
    ARCHIVAL = "archival"  # PDF/A-3b long-term-preservation profile
    ACCESSIBLE = "accessible"  # PDF/UA-1 tagged-structure profile (forces a `document(title:)`)
    PRESERVED = "preserved"  # PDF/A-3b + PDF/UA-1 archival AND accessible


class SheetSize(StrEnum):  # the architectural / engineering sheet formats — a new size is one member plus one `_SIZES` row
    A0 = "A0"
    A1 = "A1"
    A2 = "A2"
    A3 = "A3"
    A4 = "A4"
    A5 = "A5"
    ANSI_A = "ANSI-A"
    ANSI_B = "ANSI-B"
    ANSI_C = "ANSI-C"
    ANSI_D = "ANSI-D"
    ANSI_E = "ANSI-E"
    ARCH_A = "ARCH-A"
    ARCH_B = "ARCH-B"
    ARCH_C = "ARCH-C"
    ARCH_D = "ARCH-D"
    ARCH_E = "ARCH-E"
    ARCH_E1 = "ARCH-E1"
    JIS_B0 = "JIS-B0"
    JIS_B1 = "JIS-B1"


class Artifact(StrEnum):  # the Composed receipt-shape discriminant — vector PDF vs raster preview
    PDF = "pdf"
    PREVIEW = "preview"


# --- [CONSTANTS] ------------------------------------------------------------------------
_SIZES: frozendict[SheetSize, Dimensions] = frozendict({
    SheetSize.A0: (2383.94, 3370.39), SheetSize.A1: (1683.78, 2383.94), SheetSize.A2: (1190.55, 1683.78),
    SheetSize.A3: (841.89, 1190.55), SheetSize.A4: (595.28, 841.89), SheetSize.A5: (419.53, 595.28),
    SheetSize.ANSI_A: (612.0, 792.0), SheetSize.ANSI_B: (792.0, 1224.0), SheetSize.ANSI_C: (1224.0, 1584.0),
    SheetSize.ANSI_D: (1584.0, 2448.0), SheetSize.ANSI_E: (2448.0, 3168.0),
    SheetSize.ARCH_A: (648.0, 864.0), SheetSize.ARCH_B: (864.0, 1296.0), SheetSize.ARCH_C: (1296.0, 1728.0),
    SheetSize.ARCH_D: (1728.0, 2592.0), SheetSize.ARCH_E: (2592.0, 3456.0), SheetSize.ARCH_E1: (2160.0, 3024.0),
    SheetSize.JIS_B0: (2919.69, 4127.24), SheetSize.JIS_B1: (2063.62, 2919.69),
})

# the one profile->per-engine conformance-REQUEST row every author reads BY FIELD, never by magic index:
# `version` the reportlab `pdfVersion` pin (the PDF/A close is `exchange/conformance#CONFORMANCE`'s),
# `standards` the typst `pdf_standards` sequence (compiler-owned close), `variant` the weasyprint
# `pdf_variant` profile, `tags` the `pdf_tags`/`lang` structure-tree flag (the `OutputIntent` ICC threaded
# separately into `output_intent=`). A new profile is one `Standard` member plus one row, never a per-arm
# archival literal, a `bool` axis, or an `output_intent=None` hardcoded against a variant the engine needs an ICC for.
class StandardRow(Struct, frozen=True):
    version: tuple[int, int] | None  # reportlab `pdfVersion` pin
    standards: tuple[str, ...]  # typst `pdf_standards` tokens
    variant: str | None  # weasyprint `pdf_variant` profile
    tags: bool  # PDF/UA structure-tree + `lang` flag


_STANDARD: frozendict[Standard, StandardRow] = frozendict({
    Standard.DRAFT: StandardRow(version=None, standards=(), variant=None, tags=False),
    Standard.ARCHIVAL: StandardRow(version=(1, 7), standards=("a-3b",), variant="pdf/a-3b", tags=False),
    Standard.ACCESSIBLE: StandardRow(version=(1, 7), standards=("ua-1",), variant="pdf/ua-1", tags=True),
    Standard.PRESERVED: StandardRow(version=(1, 7), standards=("a-3b", "ua-1"), variant="pdf/a-3b", tags=True),
})

_BLOCK_W, _BLOCK_H, _MARGIN, _ROW_H = 510.0, 170.0, 28.35, 14.0  # title-block extent + margin + row pitch, points (top-left origin)
_ZONE: float = 141.73  # the ISO 5457 / ASME Y14.1 zone-field pitch (~50 mm in points) the frame reference grid divides each edge by


# --- [MODELS] ---------------------------------------------------------------------------
class FieldRow(Struct, frozen=True):  # one title-block grid cell — a field is one row, never a parameter
    label: str
    value: str = ""
    span: int = 1
    group: str = "general"


class Revision(Struct, frozen=True):  # one row of the revision history table
    mark: str
    date: str = ""
    description: str = ""
    by: str = ""


class Scale(Struct, frozen=True):  # the drawing scale, drawn as a ratio AND a graphic divided bar
    ratio: str = "1:1"
    bar_length: float = 0.0  # graphic-bar total extent, points; zero suppresses the bar
    segments: int = 4  # equal divisions the ruler is ticked into (architectural graphic-scale convention)
    units: str = "mm"

    def bar(self, x: float, y: float, /) -> tuple[tuple[Box, bool | str], ...]:
        # the divided graphic-scale ruler at (x, y) bottom-left: each division carries its fill state as a
        # `bool` (the author rects it) and the unit caption carries its text as a `str` (the author strings
        # it), so the consumer structural-matches the mark SHAPE rather than a magic "fill"/"clear" token;
        # an empty tuple when `bar_length` is zero so the author's fold skips it cleanly.
        if self.bar_length <= 0.0 or self.segments <= 0:
            return ()
        step, height = self.bar_length / self.segments, _ROW_H * 0.4
        divisions = tuple(((x + i * step, y, x + (i + 1) * step, y + height), bool(i % 2)) for i in range(self.segments))
        return (*divisions, ((x, y + height, x + self.bar_length, y + height + _ROW_H), f"0 — {self.bar_length:g} {self.units} ({self.ratio})"))


class NorthArrow(Struct, frozen=True):  # the north-arrow graphic cell
    bearing: float = 0.0
    glyph: bytes = b""


class KeyPlan(Struct, frozen=True):  # the key-plan reference graphic cell
    figure: bytes = b""
    highlight: str = ""


class TitleBlock(Struct, frozen=True):
    project: str = ""
    sheet_number: str = ""
    sheet_total: str = ""  # the sheet-set count drawn as "Sheet {number} of {total}" — the set this sheet belongs to
    sheet_title: str = ""
    discipline: str = ""
    status: str = ""  # the issue purpose ("FOR CONSTRUCTION" / "FOR TENDER" / "PRELIMINARY" / "AS-BUILT")
    date: str = ""
    drawn_by: str = ""
    checked_by: str = ""
    approved_by: str = ""
    scale: Scale = Scale()
    fields: tuple[FieldRow, ...] = ()
    revisions: tuple[Revision, ...] = ()
    north: NorthArrow = NorthArrow()
    key_plan: KeyPlan = KeyPlan()

    def grid(self) -> Block[FieldRow]:
        # head fields carry their real title-block ZONE in `group` (identity / approval) so `cells`
        # keeps each zone's rows contiguous — the architectural title-block convention, not a flat list.
        sheet = f"{self.sheet_number} of {self.sheet_total}" if self.sheet_total else self.sheet_number
        head = (FieldRow("Project", self.project, group="identity"), FieldRow("Sheet", sheet, group="identity"),
                FieldRow("Title", self.sheet_title, span=2, group="identity"), FieldRow("Discipline", self.discipline, group="identity"),
                FieldRow("Status", self.status, span=2, group="identity"), FieldRow("Scale", self.scale.ratio, group="identity"),
                FieldRow("Date", self.date, group="identity"), FieldRow("Drawn", self.drawn_by, group="approval"),
                FieldRow("Checked", self.checked_by, group="approval"), FieldRow("Approved", self.approved_by, group="approval"))
        return Block.of_seq(head).append(Block.of_seq(self.fields))

    def cells(self, dims: Dimensions) -> tuple[FieldCell, ...]:
        # the ONE field-rect correspondence the frame author draws and the `Fill` arm binds INTO, so a
        # bound value lands in the same rect the label was drawn at; top-left origin, points. A `span`
        # widens the value column LEFTWARD into the label gutter (right edge always the block edge), so a
        # wide field is bound inside the drawn block, never overshooting it; `group` keeps each title-block
        # zone contiguous via a STABLE group-key sort (insertion order preserved within a zone); the block
        # anchors bottom-right inside the margin and is clamped to the rows the `_BLOCK_H` extent admits.
        width, height = dims
        origin_y, col, right = height - _MARGIN - _BLOCK_H, _BLOCK_W * 0.5, width - _MARGIN
        zones: tuple[str, ...] = ("identity", "approval", "general")
        rank = frozendict({zone: index for index, zone in enumerate(zones)})
        ordered = self.grid().sort_with(lambda row: rank.get(row.group, len(zones)))
        return tuple(
            (row, (right - col * max(row.span, 1), origin_y + index * _ROW_H, right, origin_y + (index + 1) * _ROW_H))
            for index, row in enumerate(ordered.take(min(len(ordered), int(_BLOCK_H / _ROW_H))))
        )

    def history(self) -> Block[Revision]:
        return Block.of_seq(self.revisions)

    def outline(self) -> list[list[int | str]]:  # the pymupdf `set_toc` [level, title, page] row contract
        title = self.sheet_title or self.sheet_number
        revs: list[list[int | str]] = [[2, f"Rev {r.mark} {r.date}".strip(), 1] for r in self.revisions]
        return [[1, title, 1], *revs] if title else revs

    def metadata(self) -> dict[str, str]:
        return {"title": self.sheet_title or self.sheet_number, "subject": f"{self.discipline} {self.sheet_number}".strip(),
                "author": self.drawn_by, "keywords": self.project, "creator": "rasm.artifacts.sheet"}


class FigurePlacement(Struct, frozen=True):
    figure: bytes
    cell: Box  # the drawing-region rect — bare tuple, as imposition.Placement.cell and layered.Layer.bbox carry it
    name: str = "figure"  # the optional-content-group label the Place arm mints and binds this figure to
    page: int = 0
    keep_proportion: bool = True
    rotate: int = 0
    overlay: bool = True  # draw above (True) or below the sheet content already on the page, as imposition.Placement.overlay
    clip: Box | None = None
    layered: bool = True  # mint+bind an in-PDF OCG so the placed sheet is itself layer-separable in a reader
    visible: bool = True  # the OCG default-on state the `set_layer` ui config writes, exactly as layered.Layer.visible
    locked: bool = False  # the reader-side layer-lock hint `set_layer(locked=)` honors, exactly as layered.Layer.locked


class Composed(Struct, frozen=True):  # the one evidence struct of/contribute/layers read — no second render
    data: bytes
    pages: int
    kind: Artifact = Artifact.PDF
    extent: tuple[int, int] = (0, 0)  # the Preview pixmap width/height the raster receipt rides
    layers: int = 0  # the count of OCGs the Place arm minted, riding the Pdf receipt's overlay-count evidence


@tagged_union(frozen=True)
class SheetOp:  # the closed request vocabulary lowered once into Composed
    tag: Literal["frame", "place", "fill", "stamp", "preview"] = tag()
    frame: tuple[SheetSize, Engine, TitleBlock, Standard, OutputIntent, bool] = case()
    place: tuple[bytes, tuple[FigurePlacement, ...]] = case()
    fill: tuple[bytes, TitleBlock] = case()
    stamp: tuple[bytes, TitleBlock, Standard, OutputIntent, tuple[tuple[str, bytes], ...]] = case()
    preview: tuple[bytes, float] = case()

    @staticmethod
    def Frame(size: SheetSize, engine: Engine = Engine.REPORTLAB, title: TitleBlock = TitleBlock(), *, standard: Standard = Standard.DRAFT, output_intent: OutputIntent = None, landscape: bool = False) -> "SheetOp":
        return SheetOp(frame=(size, engine, title, standard, output_intent, landscape))

    @staticmethod
    def Place(sheet: bytes, placements: FigurePlacement | Iterable[FigurePlacement]) -> "SheetOp":
        return SheetOp(place=(sheet, _tupled(placements)))

    @staticmethod
    def Fill(sheet: bytes, title: TitleBlock = TitleBlock()) -> "SheetOp":
        return SheetOp(fill=(sheet, title))

    @staticmethod
    def Stamp(sheet: bytes, title: TitleBlock = TitleBlock(), *, standard: Standard = Standard.DRAFT, output_intent: OutputIntent = None, attachments: Iterable[tuple[str, bytes]] = ()) -> "SheetOp":
        return SheetOp(stamp=(sheet, title, standard, output_intent, tuple(attachments)))

    @staticmethod
    def Preview(sheet: bytes, dpi: float = 96.0) -> "SheetOp":
        return SheetOp(preview=(sheet, dpi))


# --- [SERVICES] -------------------------------------------------------------------------
class Sheet(Struct, frozen=True):
    op: SheetOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"sheet.{self.op.tag}", self._keyed, catch=_FAULTS)

    async def _keyed(self) -> ContentKey:
        return ContentIdentity.of(f"sheet-{self.op.tag}", _composed(self.op).data)

    def contribute(self) -> "Iterable[Receipt]":
        composed = _composed(self.op)
        key = ContentIdentity.of(f"sheet-{self.op.tag}", composed.data)
        match composed.kind:
            case Artifact.PDF if composed.layers:  # the OCG-bearing placed sheet rides the layer count on the Egress overlays slot
                receipt = ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)
            case Artifact.PDF:
                receipt = ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)
            case Artifact.PREVIEW:
                receipt = ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])
            case _:
                assert_never(composed.kind)
        yield from receipt.contribute()

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


# --- [OPERATIONS] -----------------------------------------------------------------------
# the surfaces-and-dispatch contract aspect: a malformed `TitleBlock`/`FigurePlacement`/`Box`
# inside a well-tagged `SheetOp` raises `BeartypeCallHintViolation` from this guarded fold, which
# the `_FAULTS`-narrowed `async_boundary` discriminates into its own `BoundaryFault` case — no
# parallel `_aspected` factory, the boundary's `CLASSIFY` row owns the violation-to-fault lift.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


def _tupled[T](items: T | Iterable[T], /) -> tuple[T, ...]:
    match items:
        case str() | bytes():  # the named platform-forced seam: str/bytes are themselves Iterable
            return (items,)  # type: ignore[return-value]
        case Iterable() as stream:
            return tuple(stream)
        case lone:
            return (lone,)


def _oriented(dims: Dimensions, landscape: bool, /) -> Dimensions:  # swap to landscape exactly as imposition.Geometry.oriented does
    return (dims[1], dims[0]) if landscape else dims


def _zones(dims: Dimensions, /) -> tuple[tuple[float, float, str], ...]:
    # the ISO 5457 / ASME Y14.1 border reference grid: each frame edge split into `_ZONE`-pitched
    # fields numbered 1.. across (bottom AND top, left to right) and lettered A.. down (left AND
    # right, top to bottom), each label centred in the margin strip so a drawing location is cited
    # by its zone ("detail C-4"). A frame property derived from the sheet dimensions exactly as
    # `TitleBlock.cells` / `Scale.bar` derive theirs, returned bottom-up for the reportlab author.
    width, height = dims
    span_w, span_h = width - 2.0 * _MARGIN, height - 2.0 * _MARGIN
    across, down = max(int(span_w // _ZONE), 1), max(int(span_h // _ZONE), 1)
    step_x, step_y, strip = span_w / across, span_h / down, _MARGIN * 0.5
    columns = tuple((_MARGIN + (col + 0.5) * step_x, edge, str(col + 1)) for col in range(across) for edge in (strip - 3.0, height - strip - 3.0))
    rows = tuple((edge, height - _MARGIN - (row + 0.5) * step_y - 3.0, chr(ord("A") + row % 26)) for row in range(down) for edge in (strip, width - strip))
    return (*columns, *rows)


@_GUARD
def _composed(op: SheetOp) -> Composed:  # the one pure render fold both `of` and `contribute` read
    import pymupdf

    match op:
        case SheetOp(tag="frame", frame=(size, engine, title, standard, intent, landscape)):
            data = _FRAME[engine](_oriented(_SIZES[size], landscape), title, standard, intent)
            return Composed(data, pages=pymupdf.open(stream=data, filetype="pdf").page_count)
        case SheetOp(tag="place", place=(sheet, placements)):
            document = pymupdf.open(stream=sheet, filetype="pdf")
            minted = Block.of_seq(_draw_one(document, placement) for placement in placements).choose(_oc_state)  # live handle; held Page faults on draw
            _configure_layers(document, minted)  # one ui-config write driving reader visibility/lock over the minted OCG set
            return Composed(document.tobytes(garbage=3, deflate=True), pages=document.page_count, layers=len(minted))
        case SheetOp(tag="fill", fill=(sheet, title)):
            document = pymupdf.open(stream=sheet, filetype="pdf")
            page = document[0]  # the live native handle; its real `rect` is the authoritative sheet geometry, oriented as drawn
            for row, rect in title.cells((page.rect.width, page.rect.height)):  # the SAME TitleBlock.cells geometry the frame drew, so each value lands in its own label's exact rect — head zones included, never the 10-field offset a `fields`-only reconstruction smuggled
                page.insert_textbox(pymupdf.Rect(*rect), row.value, fontname="helv", fontsize=8.0, align=0)
            return Composed(document.tobytes(garbage=3, deflate=True), pages=document.page_count)
        case SheetOp(tag="stamp", stamp=(sheet, title, standard, intent, attachments)):
            document = pymupdf.open(stream=sheet, filetype="pdf")
            document.set_metadata(title.metadata())
            document.set_toc(title.outline())
            for name, payload in (*attachments, *((f"{intent[0]}.icc", intent[1]),) if intent is not None else ()):  # source files + the PDF/A ICC output-intent ride as associated files
                document.embfile_add(name, payload, filename=name, desc=f"{title.discipline} source".strip())
            if standard is not Standard.DRAFT:  # the archival/accessible XMP packet is gated on the preservation intent, not a per-engine variant column
                document.set_xml_metadata(_xmp(title, standard))
            return Composed(document.tobytes(garbage=3, deflate=True), pages=document.page_count)
        case SheetOp(tag="preview", preview=(sheet, dpi)):
            pixmap = pymupdf.open(stream=sheet, filetype="pdf")[0].get_pixmap(dpi=int(dpi))
            return Composed(pixmap.tobytes("png"), pages=1, kind=Artifact.PREVIEW, extent=(pixmap.width, pixmap.height))
        case _:
            assert_never(op)


def _draw_one(document: "Document", placement: FigurePlacement) -> tuple[int, bool, bool]:  # (xref, visible, locked); xref 0 if unlayered
    import pymupdf

    page = document[0]
    xref = document.add_ocg(placement.name, on=placement.visible, intent="View", usage="Artwork") if placement.layered else 0
    docsrc = pymupdf.open(stream=placement.figure, filetype="pdf")
    clip = pymupdf.Rect(*placement.clip) if placement.clip is not None else None
    page.show_pdf_page(pymupdf.Rect(*placement.cell), docsrc, pno=placement.page, keep_proportion=placement.keep_proportion, overlay=placement.overlay, rotate=placement.rotate, clip=clip, oc=xref)
    return xref, placement.visible, placement.locked


def _oc_state(drawn: tuple[int, bool, bool], /) -> Option[tuple[int, bool, bool]]:  # keep only the rows that minted a real OCG xref
    return Some(drawn) if drawn[0] else Nothing


def _configure_layers(document: "Document", minted: Block[tuple[int, bool, bool]], /) -> None:
    # one `set_layer` ui-config write over the minted OCG set — the reader's panel toggles `on`/`off` and
    # honors `locked`, exactly as the sibling `export/layered#LAYERED` `PdfOcg` arm drives the placed layers.
    if not minted.is_empty():
        document.set_layer(
            0,
            on=[xref for xref, visible, _locked in minted if visible],
            off=[xref for xref, visible, _locked in minted if not visible],
            locked=[xref for xref, _visible, locked in minted if locked],
        )


def _frame_reportlab(dims: Dimensions, title: TitleBlock, standard: Standard, intent: OutputIntent) -> bytes:
    from reportlab.lib.utils import ImageReader
    from reportlab.pdfgen.canvas import Canvas

    width, height = dims
    sink = BytesIO()
    # reportlab authors only the conformance REQUEST (the `pdfVersion` pin + `lang` tag for PDF/UA);
    # the PDF/A output-intent ICC embed and the structure-tree close are `exchange/conformance#CONFORMANCE`'s.
    profile = _STANDARD[standard]
    canvas = Canvas(sink, pagesize=dims, pdfVersion=profile.version, lang="en" if profile.tags else None)
    border = canvas.beginPath()
    border.rect(_MARGIN, _MARGIN, width - 2 * _MARGIN, height - 2 * _MARGIN)
    canvas.setLineWidth(1.0)
    canvas.drawPath(border, stroke=1, fill=0)
    canvas.rect(width - _MARGIN - _BLOCK_W, _MARGIN, _BLOCK_W, _BLOCK_H)
    canvas.setFont("Helvetica", 7)
    for zx, zy, mark in _zones(dims):  # the ISO 5457 border reference grid, derived beside the title cells and the scale bar
        canvas.drawCentredString(zx, zy, mark)
    for row, (rx, top, _x1, _y1) in title.cells(dims):  # reportlab is bottom-up; flip the top-left cell rect into the page frame
        text = canvas.beginText(rx, height - top - _ROW_H + 3.0)
        text.setFont("Helvetica-Bold", 8)
        text.textLine(f"{row.label}: {row.value}")
        canvas.drawText(text)
    if title.revisions:
        revs = canvas.beginText(width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + 4.0)
        revs.setFont("Helvetica", 7)
        revs.textLines("\n".join(f"{r.mark} {r.date} {r.description} {r.by}".rstrip() for r in title.history()))
        canvas.drawText(revs)
    if title.north.glyph:  # the north arrow rotated to its `bearing` (CW-positive map convention -> CCW canvas)
        canvas.saveState()
        canvas.translate(width - _MARGIN - _BLOCK_W + _MARGIN, _MARGIN + _MARGIN)
        canvas.rotate(-title.north.bearing)
        canvas.drawImage(ImageReader(BytesIO(title.north.glyph)), -_MARGIN, -_MARGIN, width=_MARGIN * 2, height=_MARGIN * 2, preserveAspectRatio=True, mask="auto")
        canvas.restoreState()
    if title.key_plan.figure:
        kx, ky = width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + _MARGIN
        canvas.drawImage(ImageReader(BytesIO(title.key_plan.figure)), kx, ky, width=_MARGIN * 2, height=_MARGIN * 2, preserveAspectRatio=True, mask="auto")
        if title.key_plan.highlight:  # the covered-region label annotating the key plan
            canvas.drawString(kx, ky - _ROW_H + 3.0, title.key_plan.highlight)
    for shape, mark in title.scale.bar(width - _MARGIN - _BLOCK_W, _MARGIN + _BLOCK_H + _ROW_H):  # the divided graphic scale bar above the block
        match mark:
            case bool():  # a division rect — fill state carried as the bool, never a magic "fill"/"clear" token
                canvas.rect(shape[0], shape[1], shape[2] - shape[0], shape[3] - shape[1], stroke=1, fill=int(mark))
            case str():  # the unit caption text
                canvas.drawString(shape[0], shape[1], mark)
    canvas.showPage()
    canvas.save()
    return sink.getvalue()


def _frame_typst(dims: Dimensions, title: TitleBlock, standard: Standard, _intent: OutputIntent) -> bytes:
    import typst  # typst's PDF/A output-intent is the compiler's own; the ICC is not a frame-author input

    width, height = dims
    # interpolate every field value as a typst STRING (`[#"..."]`), never bracketed content (`[..]`): a
    # string renders its text literally, so a label/value/revision carrying `#`/`*`/`[`/`@` markup cannot
    # inject, and `_escape` (backslash + quote) is then the exact and sufficient string-literal escaping.
    grid = ", ".join(f'[#"{_escape(row.label)}"], [#"{_escape(row.value)}"]' for row in title.grid())
    revs = ", ".join(
        f'grid.cell(colspan: 2)[#"{_escape(r.mark)} {_escape(r.date)} {_escape(r.description)} {_escape(r.by)}"]'
        for r in title.history()
    )
    source = (
        f'#set document(title: "{_escape(title.sheet_title or title.sheet_number)}", author: "{_escape(title.drawn_by)}")\n'
        f"#set page(width: {width}pt, height: {height}pt, margin: {_MARGIN}pt)\n"
        "#rect(width: 100%, height: 100%, stroke: 1pt)\n"
        "#place(bottom + right, dx: -4pt, dy: -4pt, rect(stroke: 1pt, inset: 6pt, "
        f"grid(columns: 2, gutter: 4pt, {grid}, grid.cell(colspan: 2)[#line(length: 100%)], {revs})))\n"
    )
    return typst.compile(source.encode(), output=None, format="pdf", pdf_standards=_STANDARD[standard].standards, ignore_system_fonts=True, timestamp=0)


def _frame_weasyprint(dims: Dimensions, title: TitleBlock, standard: Standard, intent: OutputIntent) -> bytes:
    from weasyprint import HTML
    from weasyprint.text.fonts import FontConfiguration

    width, height = dims
    profile = _STANDARD[standard]
    variant, tags = profile.variant, profile.tags
    cells = "".join(f"<tr><th>{escape(r.label)}</th><td>{escape(r.value)}</td></tr>" for r in title.grid())
    revs = "".join(f"<tr><td colspan=2>{escape(f'{r.mark} {r.date} {r.description} {r.by}'.rstrip())}</td></tr>" for r in title.history())
    title_tag = f"<title>{escape(title.sheet_title or title.sheet_number)}</title>" if tags else ""
    html = (f"<style>@page{{size:{width}pt {height}pt;margin:{_MARGIN}pt}}body{{border:1pt solid}}"
            f"table{{position:fixed;bottom:4pt;right:4pt;border:1pt solid}}</style>{title_tag}<table>{cells}{revs}</table>")
    # weasyprint owns the PDF/A close end to end: `pdf_variant` selects the profile, `pdf_tags` writes the
    # structure tree, `output_intent` embeds the supplied ICC the variant requires (`None` only for `DRAFT`).
    return HTML(string=html, base_url=".").write_pdf(target=None, font_config=FontConfiguration(),
                                                     pdf_variant=variant, pdf_tags=tags,
                                                     output_intent=BytesIO(intent[1]) if intent is not None else None)


# --- [TABLES] ---------------------------------------------------------------------------
_FRAME: frozendict[Engine, Author] = frozendict({
    Engine.REPORTLAB: _frame_reportlab,
    Engine.TYPST: _frame_typst,
    Engine.WEASYPRINT: _frame_weasyprint,
})


# --- [BOUNDARIES] -----------------------------------------------------------------------
def _placed_layers(op: SheetOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    match op:
        case SheetOp(tag="frame", frame=(size, _engine, _title, _standard, _intent, landscape)):
            w, h = _oriented(_SIZES[size], landscape)
            return (Layer(_name(names, 0, "frame"), _composed(op).data, (0.0, 0.0, w, h)),)
        case SheetOp(tag="place", place=(_sheet, placements)):
            return tuple(Layer(_name(names, index, placement.name), placement.figure, placement.cell, visible=placement.visible, locked=placement.locked) for index, placement in enumerate(placements))
        case _:
            return ()


def _name(names: tuple[str, ...], index: int, fallback: str) -> str:
    return names[index] if index < len(names) else fallback


def _escape(value: str) -> str:  # typst markup escaping has no stdlib owner; HTML escaping rides `html.escape`
    return value.replace("\\", "\\\\").replace('"', '\\"')


_NS = frozendict({"x": "adobe:ns:meta/", "rdf": "http://www.w3.org/1999/02/22-rdf-syntax-ns#",
                  "dc": "http://purl.org/dc/elements/1.1/", "pdfaid": "http://www.aiim.org/pdfa/ns/id/"})
# the `Standard` -> PDF/A (part, conformance) the `pdfaid` XMP packet declares; `None` for a profile carrying no PDF/A part.
_PDFAID: frozendict[Standard, tuple[str, str] | None] = frozendict({
    Standard.DRAFT: None, Standard.ARCHIVAL: ("3", "B"), Standard.ACCESSIBLE: None, Standard.PRESERVED: ("3", "B"),
})


def _xmp(title: TitleBlock, standard: Standard) -> str:
    # `lxml` is manifest-gated `python_version<'3.15'` (no cp315 wheel) and is the subprocess-only XML rail,
    # so the in-process synchronous `_composed`/`contribute` fold builds this small fixed-schema XMP packet
    # through the cp315-native stdlib `xml.etree.ElementTree` rather than crossing a process seam for a dozen
    # elements — the structured tree the `language.md` STRING_RECOVERY law mandates over a concatenated string.
    # `register_namespace` binds the canonical `x`/`rdf`/`dc`/`pdfaid` prefixes the `_NS` table declares; the
    # Dublin-Core fields plus the `_PDFAID` part/conformance make the metadata STATE the PDF/A claim.
    from xml.etree.ElementTree import Element, QName, SubElement, register_namespace, tostring

    for prefix, uri in _NS.items():
        register_namespace(prefix, uri)
    rdf = Element(QName(_NS["rdf"], "RDF"))
    description = SubElement(rdf, QName(_NS["rdf"], "Description"), {QName(_NS["rdf"], "about"): ""})
    for ns, key, value in (("dc", "title", title.sheet_title), ("dc", "creator", title.drawn_by),
                           ("dc", "description", title.project), *(("pdfaid", slot, member) for slot, member in
                           zip(("part", "conformance"), _PDFAID[standard] or (), strict=False))):
        if value:
            SubElement(description, QName(_NS[ns], key)).text = value
    meta = Element(QName(_NS["x"], "xmpmeta"))
    meta.append(rdf)
    return tostring(meta, encoding="unicode")
```

## [03]-[RESEARCH]

- [SHOW_PDF_PAGE_VERIFIED]: the `Place` arm vector-copies each already-emitted figure PDF into a drawing-region `Box` rect through `pymupdf.Page.show_pdf_page(rect, docsrc, pno=0, keep_proportion=True, overlay=True, oc=0, rotate=0, clip=None) -> int`, a VERIFIED REAL member of `pymupdf.Page` reflected on cp315 (`pymupdf 1.27.2.3`, native MuPDF cp310-abi3) — `keep_proportion` preserves aspect, `rotate` places at 0/90/180/270, `clip` restricts the source region, and `oc` binds the placement to an optional-content-group xref. The OCG lifecycle is two reflection-confirmed `Document` members: `add_ocg(name, config=-1, on=1, intent=None, usage=None) -> int` mints the group with the per-placement `on=visible` default, and `set_layer(config, basestate=None, on=None, off=None, rbgroups=None, locked=None)` writes the reader ui config over the minted xref partitions so the `visible`/`locked` `FigurePlacement` axes reach the reader's layer panel, exactly as the sibling `export/layered#LAYERED` `PdfOcg` arm drives them. `Page.insert_textbox(rect, buffer, *, fontname="helv", fontsize=11, align=0, ...) -> float` is the reflection-confirmed `Fill` arm member that binds each field value into its derived cell rect. So a clipped detail callout, an OCG-bound layer, or a reader-locked layer is one `FigurePlacement` field carried into the existing `show_pdf_page`/`set_layer` fold, never a new arm. The folder `.api` catalogue for `pymupdf` rows `add_ocg`/`set_layer`/`insert_textbox`/`get_pixmap`/`set_toc`/`embfile_add`/`tobytes` but does NOT yet row `show_pdf_page` (it rows the sibling `Page.insert_image`/`Document.insert_pdf` cross-document members), the `set_metadata`/`set_xml_metadata` info/XMP-write pair (it rows only the `save(preserve_metadata=)`/`scrub(xml_metadata=)` adjacents), or the `Document.page_count` attribute, so those members are catalogue-pending: each signature is reflection-verified on cp315 and the close-condition is the catalogue adding the `show_pdf_page`/`set_metadata`/`set_xml_metadata`/`page_count` rows to its `[03]-[ENTRYPOINTS]` render/metadata scope (the sibling `composition/imposition#IMPOSE` and `export/layered#LAYERED` arms read the same pending members, so the catalogue add closes all three pages at once). `Page.get_pixmap(dpi=) -> Pixmap` (the catalogued render row, `int` dpi) with `Pixmap.width`/`Pixmap.height` and `Pixmap.tobytes("png") -> bytes` drive the `Preview` raster-thumbnail arm and its `ArtifactReceipt.Preview(key, width, height)` extent; `Document.page_count` (a reflection-confirmed `Document` attribute the sibling `composition/imposition#IMPOSE` reads identically) supplies the REAL page count every PDF arm's `Composed` carries; `Document.set_xml_metadata(xml: str)` — a reflection-confirmed `Document` member — writes the archival XMP packet the `Stamp` arm binds beside `set_metadata`/`set_toc`. A malformed figure or sheet source raises the `pymupdf.FileDataError`/`EmptyFileError` (`RuntimeError`-derived, catalogued `[02]` fault family) the `_FAULTS = (RuntimeError, ...)` tuple admits, so `async_boundary` converts it to a `BoundaryFault` rather than a bare provider raise escaping the capsule or a decorative local `<bad-source>` the rail never reads.
- [SHEET_ALGEBRA_SETTLED]: no OSS sheet-set / title-block / drawing-frame library is admitted, so the sheet algebra (frame border, the `_zones` ISO 5457 border reference grid, grid layout, region partition, cell placement, the `Scale.bar` divided graphic-scale ruler) is this owner's composition over the three engines, never a re-implemented byte emitter. The graphic scale bar is the architectural-sheet cell the domain demands: `Scale.bar(x, y)` projects the alternating filled/clear division rects over `bar_length`/`segments` plus the `units` caption, and the reportlab author draws them as vector `Canvas.rect`/`drawString` marks beside the `NorthArrow`/`KeyPlan` cells, so `bar_length`/`segments`/`units` are load-bearing rather than dead fields. The ISO 5457 / ASME Y14.1 border reference grid is the second domain-mandated frame element the bare border rect omitted: `_zones(dims)` derives the per-edge field labels (1.. across, A.. down) from the sheet dimensions and `_ZONE` pitch exactly as `cells`/`Scale.bar` derive theirs, and the reportlab author draws them through `drawCentredString` in the margin strip so a drawing location is cited by zone — a frame property always-derived from dimensions, never a `zones: bool` knob the KNOB_TEST deletes; the templated `typst`/`weasyprint` authors carry the title block alone, the reportlab coordinate author owning the full vector frame. The `reportlab` frame authors through the catalogued BATCHED dense forms rather than repeated `drawString`/`rect` — `Canvas.beginText -> PDFTextObject.textLines -> drawText` for the multi-row grid AND the `Revision` history lines and `Canvas.beginPath -> PDFPathObject.rect -> drawPath` for the border (`5.0.0` on cp315, BSD; the catalogue `[04]` `REPORTLAB_TOPOLOGY` names these the dense form that cuts operator count, the prior per-row `drawString` loop the rejected form), with `lib.utils.ImageReader` the decode-once `drawImage` source for the `NorthArrow.glyph`/`KeyPlan.figure` raster cells and `Canvas(..., pdfVersion=)` the archival version pin; the `platypus.Table`/`TableStyle` flowable surface is the catalogued data-shaped alternative the text-object form supersedes for the coordinate-placed sheet. The `typst.compile(input, output=None, format="pdf", pdf_standards=, ignore_system_fonts=, timestamp=)` markup surface and the `page`/`grid`/`grid.cell`/`line`/`rect` vocabulary verify against the folder catalogue (`0.15.0`, Apache-2.0 cp38-abi3): `pdf_standards=("a-3b", "ua-1")` is the archival+accessible PDF/A-PDF/UA frame-author row (a `"ua-1"` render hard-errors without a `document(title:)`, so the lowering always sets it), `ignore_system_fonts=True` plus `timestamp=0` pin byte-determinism (the content key mints over the emitted bytes), and `TypstError` (`RuntimeError`-derived) is the catalogued compile-failure carrier the `_FAULTS` `RuntimeError` row admits. Interpolated `TitleBlock` strings are Typst-escaped through `_escape` before emission. The `weasyprint` engine is the third row newly adopted from the folder catalogue (`69.0`, BSD `py3-none-any`): `HTML(string=, base_url=).write_pdf(target=None, font_config=FontConfiguration(), pdf_variant="pdf/a-3b", pdf_tags=True, output_intent=BytesIO(icc))` is the CSS-templated tagged/archival frame author owning its full PDF/A close — `pdf_variant` selecting the profile, `pdf_tags` writing the PDF/UA structure tree, and the supplied `OutputIntent` ICC stream embedded through `output_intent=` (the prior fence's hardcoded `output_intent=None` against a `pdf/a-3b` variant was the illusory-conformance defect the catalogue's "`output_intent` supplies the ICC profile PDF/A requires" rule names) — the `@page{size:..}` rule carrying the `SheetSize` dimensions, the catalogued `**options` variant axis the disciplined collapse of a parallel writer per profile. Interpolated HTML field values ride `html.escape` (the stdlib HTML-escaping owner) and Typst field values ride the local `_escape` (backslash + quote) inside typst STRING literals (`[#"…"]` cells, never bracketed `[…]` content), so neither an HTML metacharacter nor a typst markup metacharacter (`#`/`*`/`[`/`@`) in a title-block value can inject — each its own grammar's rule. The `pymupdf` `open(stream=)`/`Page.show_pdf_page`/`Page.insert_textbox`/`get_pixmap`/`add_ocg`/`set_layer`/`set_metadata`/`set_xml_metadata`/`set_toc`/`embfile_add`/`tobytes`/`paper_rect`/`page_count` members verify against the folder catalogue and the imposition sibling's reflection; `paper_rect(s) -> Rect` covers only the small named-paper subset, so the `_SIZES` `frozendict` carries the full ARCH/ANSI/JIS span the owner needs and the `Fill` arm reads the emitted `Page.rect` for the authoritative oriented geometry; `set_layer(config, on=, off=, locked=)` writes the placed-figure reader visibility/lock config the OCG mints carry.
- [ONE_FOLD_SETTLED]: `_composed(op) -> Composed` is the ONE pure render path both `of` and `contribute` read — the prior fence's `@functools.cache`-memoized `_rendered` returning `Result[bytes, SheetFault]` and the receipt-side re-read are the deleted defects, replaced by the sibling `composition/imposition#IMPOSE` `Composed` pattern: each arm renders its `pymupdf` document once, reads `Document.page_count` off the live document for the real page count (never a hardcoded `1`), and returns `Composed(data, pages, kind, extent)`. The `kind` `Artifact` discriminant plus the `layers` count select one receipt-owner mint `contribute` calls — `ArtifactReceipt.Pdf` for the vector arms, `ArtifactReceipt.Egress` for the OCG-bearing `Place` arm, `ArtifactReceipt.Preview` for the raster `Preview` arm — the `Preview` PNG was mis-shaped as a `Pdf` receipt with a phantom `1`-page count in the prior fence, now the `Preview` extent rides the pixmap `width`/`height` the catalogue rows. `_SIZES` and `_FRAME` are `frozendict` policy tables (the `FROZENDICT_TABLE_SITE` immutable-policy-table owner) so the `Frame` arm reads one row and dispatches by member, never an `if engine == ...` cascade or a module-level mutable `dict`. The placement and field draws mutate the one live `pymupdf` document — the page is the mutable native handle the engine owns (the platform-forced seam `boundaries.md` names), not a domain accumulator — and the `Place` arm's `Block.choose(_oc_state)` collects only the minted OCG xref rows the one `set_layer` config consumes, not a domain fold over a discarded mutated-document result; `tobytes` runs once on the placed document, and no `expression.extra.result.traverse` over a sentinel-returning `_draw_one` is reached, because the engine raise the boundary converts replaces the per-element `Result` thread the prior fence used.
- [RAIL_SETTLED]: the rail is the branch `RuntimeRail[ContentKey] = Result[ContentKey, BoundaryFault]` the `runtime/faults#FAULTS` owner legislates, minted ONCE at `async_boundary(f"sheet.{op.tag}", self._keyed, catch=_FAULTS)`. The prior fence's parallel `SheetFault` `Literal` and `_aspected` `beartype` contract weave are the deleted illusory-rail defects — `SheetFault` never reached the runtime rail (the prior `of` lambda mapped it into the `RuntimeRail` without collapsing, double-railing a `Result[ContentKey, SheetFault]` inside the `RuntimeRail` the boundary returns), and the `BeartypeCallHintViolation`-to-`<contract>` weave duplicated the `runtime/faults#FAULTS` `CLASSIFY` `api` row that already folds the same violation to `BoundaryFault(api=...)`. `_FAULTS = (RuntimeError, ValueError, KeyError, OSError, BeartypeCallHintViolation)` is the real module-level engine raise tuple — `RuntimeError` admits `typst.TypstError`/`pymupdf.FileDataError`/`pymupdf.EmptyFileError` (all `RuntimeError`-derived, so no boundary-scoped engine import is needed to name them), `ValueError`/`KeyError` admit a reportlab/weasyprint author raise (and an `xml.etree.ElementTree` malformed-name `ValueError` from the `_xmp` build) and a `_SIZES`/`_FRAME`/`_STANDARD`/`_PDFAID` key miss, `OSError` the stream/`.icc` file fault, and `BeartypeCallHintViolation` the `_GUARD`-contracted malformed `TitleBlock`/`FigurePlacement`/`Box` the `_composed` fold rejects — so the boundary discriminates each into its own `BoundaryFault` case rather than the `Exception` catch-all the faults owner rejects, matching the sibling `composition/imposition#IMPOSE` `_FAULTS` tuple one-for-one, and `_keyed` returns `ContentKey` (the boundary owns the rail) rather than a `Result` nested inside the rail. `_keyed` and `contribute` both read the one `_composed` fold; cancellation is excluded from `_FAULTS` and re-raises as the structured signal.
- [MODAL_ARITY_SETTLED]: `SheetOp.Place` takes `FigurePlacement | Iterable[FigurePlacement]` normalized once at the head through `_tupled`, the `surfaces-and-dispatch.md` `[ARITY_ABSORPTION]` structural `match` whose ONLY guard is the named `str`/`bytes` platform seam (`str`/`bytes` are themselves `Iterable`); a `msgspec.Struct` is NOT `Iterable` (no `__iter__`), so a lone `FigurePlacement` reaches the `lone` arm with no extra guard, and a `Struct` exclusion would be a phantom guard the doctrine rejects. A single placement and a placement sequence are one entrypoint discriminating on input shape, never a `Place`/`PlaceMany` sibling pair or a `batch: bool` knob. `Fill` is NOT modal-arity: it takes the one `TitleBlock` owner whose `cells((page.width, page.height))` reproduces the EXACT `grid()` head + extension ordering the `Frame` author drew, so a bound value lands in its own label's rect — the prior `Fill(sheet, rows)` normalized loose `FieldRow`s through `_tupled` into a `TitleBlock(fields=rows)` whose ten empty head fields offset every value past the head zones, the illusory-alignment defect the full-owner form deletes. The deadline/retry of the `async_boundary` capsule is the runtime scope, never a signature parameter.
- [IMPOSE_AND_LAYER_SEAM] [RESOLVED]: the titled sheet is handed to `composition/imposition#IMPOSE` keyed by the same `ContentKey` for the multi-sheet n-up/booklet/signature press form — a booklet fold grafted onto `SheetOp` and a signature emitter beside the one-page egress are the rejected forms; the sheet owner is the per-sheet authoring half, the imposition owner the multi-sheet half, meeting at the PDF bytes. `Sheet.layers(names) -> tuple[Layer, ...]` projects the placed layout outward to `export/layered#LAYERED` exactly as `composition/compose#COMPOSE` `Figure.layers` does: the `Frame` arm yields one `Layer(name, source, bbox)` over the full sheet box reading the one `_composed(op).data` fold (never a second frame render inside `layers`), the `Place` arm one row per `FigurePlacement` carrying its bare `Box` rect as the placed extent, each filling the REQUIRED `bbox` positional the `export/layered` `Layer(name, source, bbox, visible, locked)` row declares — so the frame-plus-figures layout hands outward as named rows the export owner binds into named `drawsvg` `Group`s and pymupdf OCG layers, the `oc` placement field the seam where a figure binds to its named OCG. A layered-export arm grafted onto `SheetOp`, a per-layer writer here, and a frame re-render inside `layers` are the rejected forms; the named-layer authoring lands once in the export owner, this owner projects the rows.
- [RECEIPT_THREAD_SETTLED]: each operation contributes one `core/receipt#RECEIPT` case through `Sheet.contribute`, which reads the same `_composed(op)` `Composed` the `of` projection reads (the same fold both entrypoints call), mints the content key over `composed.data` through the same `ContentIdentity.of` `_keyed` uses, and routes on the `Composed.kind` `Artifact` discriminant plus the `Composed.layers` count to select ONE receipt-owner mint — the `Frame`/`Fill`/`Stamp` arms `ArtifactReceipt.Pdf(key, len(composed.data), composed.pages)` carrying the REAL `Document.page_count` (never the prior hardcoded `1`), the OCG-bearing `Place` arm `ArtifactReceipt.Egress(key, len(composed.data), composed.pages, 0, 0, composed.layers)` carrying the minted-layer count on the `overlays` slot, the raster `Preview` arm `ArtifactReceipt.Preview(key, composed.extent[0], composed.extent[1])` carrying the pixmap extent — then yields the selected mint's `contribute()`, whose `slot` projection reads the `ContentKey` off the case and whose body projects the `Receipt.of("artifacts", (phase, self.slot.hex, self._facts()))` contract. The receipt owner's public construction surface IS these named flat-scalar mints (`Pdf`/`Egress`/`Preview` are the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the positional shape `_facts` zips against its `_KEYS` row), so a per-kind facts `Struct` (`PdfFacts`/`EgressFacts`/`PreviewFacts`) re-wrapping those scalars and a phantom `ArtifactReceipt.of(key, facts)` over them are the indirection that owner explicitly rejects, not its surface; the sheet owner adds no receipt case and no sibling factory — a failed render raises the engine fault the `async_boundary` converts to a `BoundaryFault` (the receipt owner's own `rejected` arm projecting it on the same stream) rather than a zero-byte placeholder hand-built in the projection, the prior zero-byte `Pdf` fallback the rejected form; a parallel `SheetReceipt` type and a hardcoded page count are the deleted forms.
- [ARCHIVAL_CONFORMANCE_SETTLED]: the `Standard` member states the conformance INTENT the frame author REQUESTS; the prior fence's `Standard.PRESERVED drives the PDF/A-3b + PDF/UA-1 profile across all three` overstated the reportlab leg, which sets only the `pdfVersion`/`lang` pin and writes NO output-intent or structure tree — a version number masquerading as conformance. The rebuild splits the close by engine ownership: `weasyprint` owns the full PDF/A close (`pdf_variant` profile + `pdf_tags` structure tree + the supplied `OutputIntent` ICC threaded into `output_intent=`, the catalogue's "`output_intent` supplies the ICC profile PDF/A requires" rule the prior `output_intent=None` violated against a `pdf/a-3b` variant — the illusory-conformance defect), `typst` owns its compiler-native `pdf_standards` close, and `reportlab` carries the version pin only — `pdfVersion=(1, 7)`, the base PDF/A-3b and PDF/UA-1 mandate (never the `(2, 0)` a PDF/A-4 base would take) — with its PDF/A output-intent/structure-tree close routing to `exchange/conformance#CONFORMANCE`. The `OutputIntent = tuple[str, bytes] | None` ICC rides the `Frame`/`Stamp` case payload (the ICC profile bytes are an input from `graphic/color/managed#MANAGED`, not minted here) — the `Frame` arm threads it into the engine's `output_intent=`, the `Stamp` arm embeds it as a PDF associated file the conformance leg consumes through `embfile_add`. A `output_intent=None` hardcoded against a PDF/A `Standard` is the rejected illusory form.
- [XMP_AND_GROUP_SETTLED]: the `Stamp` arm's archival XMP packet is built through the cp315-native stdlib `xml.etree.ElementTree` `Element`/`SubElement`/`QName`/`register_namespace`/`tostring` element tree carrying the Dublin-Core descriptive fields plus the `_PDFAID` `pdfaid` part/conformance the `Standard` declares — the prior fence's hand-concatenated `<?xpacket?><x:xmpmeta>…` f-string was the `language.md` STRING_RECOVERY-banned manual-XML defect, and it carried no `pdfaid` part so the XMP never declared the PDF/A claim the engine token requested. `lxml` is manifest-gated `python_version<'3.15'` (no cp315 wheel) and is the subprocess-only XML rail, so the synchronous in-process `_composed`/`contribute` fold builds this small fixed-schema XMP packet through the cp315-native stdlib `xml.etree.ElementTree` rather than crossing a process seam for a dozen elements (`register_namespace` binds the canonical `x`/`rdf`/`dc`/`pdfaid` prefixes); the prior fence's `lxml`-on-the-core claim was a phantom — `lxml` would `ImportError` on cp315. The `FieldRow.group` field was declared but read nowhere in the prior fence — a decorative dead field; the rebuild makes it load-bearing: `grid()` tags each head field with its real title-block ZONE (`identity`/`approval`) and `cells()` group-key sorts the rows through `Block.sort_with` over the `identity`->`approval`->`general` rank so each zone stays contiguous in the drawn block, the architectural title-block convention rather than a flat interleaved list.
