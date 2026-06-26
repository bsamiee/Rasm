# [PY_ARTIFACTS_COMPOSE]

The post-render figure/section placement owner turning emitted graphics into placed, annotated, color-correct figures. `Figure` is ONE owner over the post-render composition pipeline carrying a closed-payload `FigureOp` `expression.tagged_union` — each operation a case carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`. It reads the SVG that the regrouped `visualization/chart#CHART`, `graphic/marks#MARK`, and `visualization/table#TABLE` sibling owners already emit and lays it out — scale-to-fit a target viewport, tile an n-up sheet, crop to a bounds box, rotate-place at an `Angle`, overlay registration marks and crop guides — by IMPORTING the `graphic/vector#VECTOR` public geometry surface (`bounds`/`path`/`transform`/`svg`/`px`, the `Element` `Protocol`, and the `RenderPolicy` rasterize-policy owner) and composing it one hop, never re-declaring the `svgelements` `Matrix`/`Path`/`Color`/`Length`/`Angle`/`bbox` algebra or the `_svg`/`_path` egress this owner does not own; the rotate-place angle resolves through the local `_angle` over `svgelements.Angle.parse`, the same placement-math use of the geometry algebra the `Matrix` factories ride, never an imported vector function. It holds ONLY the placement-specific arm bodies the geometry primitive deliberately does not own — the scale-fit factor, the n-up cell placement, the `<clipPath>` crop egress, the rotate pivot, the registration-overlay fold, and the gated `pillow` finish — over that imported surface. It rasterizes its own placed SVG through the vector owner's `RenderPolicy`-driven `resvg_py.svg_to_bytes` raster floor on the core, then draws captions/legends/borders, fits or EXIF-orients the raster, applies a tone/sharpen/blur finish chain, and binds EXIF/XMP metadata over `pillow` on the gated `python_version<'3.15'` band. One figure surface discriminating the operation, not a per-graphic-type composer family. The vector geometry arms resolve in-process because `graphic/vector#VECTOR` is the host-free geometry floor — `svgelements` is a pure-Python `py2.py3-none-any` wheel and `resvg_py` the cp315 native extension, both importing on the core; the raster annotate/metadata arms first rasterize the placed SVG through that owner's `resvg_py` floor on the core, then offload the `pillow` draw/filter/metadata pass to a `to_process` worker as a GIL-hostile native call (concurrency chooser `[11]`), the `import PIL` landing at boundary scope inside the worker so the cp315-core module stays import-clean while the `pillow` arms run only on the `python_version<'3.15'` band the wheel ships for — the process seam is GIL isolation, not a cross-version dispatch. Figure composition rasterizes only its own placed SVG for the annotate pass through the `graphic/vector#VECTOR` `Rasterize` floor — chart/mark/nanoplot rasterization-to-export stays in `visualization/chart#EXPORT` `vl-convert` — and re-renders no chart; it places, rasterizes, and finishes already-emitted graphics. The placed multi-source layout the placement arms emit is one flat single-`<svg>` document — that flat-SVG egress is this owner's concern; the editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s concern, receiving the same placed layout and binding each placed source as a named editable layer rather than one flattened path soup, so figure composition emits the flat artifact and routes the named-layer authoring outward instead of growing a layer-emit arm. The one-page vector-PDF egress the `composition/sheet#SHEET` and `composition/imposition#IMPOSE` consumers draw through `pymupdf.show_pdf_page` is the `Pdf` case's `vl_convert.svg_to_pdf` wrap of that placed flat document — the documented post-edit-SVG-then-PDF path that closes the placed-figure-to-PDF seam those consumers need, never a chart re-render and never a second SVG-to-PDF sink. Every operation returns a `RuntimeRail[ContentKey]` over the runtime `async_boundary` narrowed to the real engine raise tuple `_FAULTS` (so a non-engine raise and cancellation propagate as defects rather than being railed, the `BoundaryFault` classification of each caught raise the runtime `faults#FAULTS` owner's `CLASSIFY` concern) and contributes through the existing `core/receipt#RECEIPT` named per-kind mints the producer calls positionally — `ArtifactReceipt.Preview(key, width, height)` placement evidence (empty perceptual band) or `ArtifactReceipt.Pdf(key, bytes, pages)` projection evidence — by threading the bytes `_emit` already computed, never a second re-composition or re-render and never the phantom `ArtifactReceipt.of(key, PreviewFacts(...))`/`PdfFacts` evidence-struct indirection the receipt owner deleted.

## [01]-[INDEX]

- [01]-[COMPOSE]: figure-placement, overlay, clip-crop, PDF-projection, rasterize, annotate, stamp-composite, and metadata owner over the closed-payload `FigureOp` `tagged_union` IMPORTING the `graphic/vector#VECTOR` public geometry surface (`bounds`/`path`/`transform`/`svg`/`px`, `Element`, `RenderPolicy`) for parse/transform/bounds/serialize/rasterize and holding only the placement-math arm bodies the primitive does not own, lifting the placed flat `<svg>` to a one-page vector PDF through `vl_convert.svg_to_pdf` for the `sheet`/`imposition` consumers, and offloading the gated `pillow` raster draw/composite/finish/metadata pass to a `to_process` worker on the `python_version<'3.15'` band under the `async_boundary` narrowed to the real engine raise tuple.

## [02]-[COMPOSE]

- Owner: `Figure` the one figure-composition owner discriminating operation over the closed `FigureOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `RenderPolicy` is NOT this owner's — it is `graphic/vector#VECTOR`'s rasterize-policy owner (the resvg sizing/parsing/font/policy/diagnostic axis projected to `svg_to_bytes` through one `asdict`-driven spread), IMPORTED here and carried into the `Annotate` case so the rasterize policy has one edit site across the chart/diagram/figure consumers; `DrawOp` the closed-payload `tagged_union` collapsing the rejected `CaptionSpec`/`BoxSpec`/`PostSpec` draw triple into one pillow draw family — a `Text` case (`ImageDraw.text`), a `Box` case (`ImageDraw.rectangle`), a `Line` case (`ImageDraw.line` leader/connector polyline), an `Ellipse` case (`ImageDraw.ellipse` highlight ring), a `Polygon` case (`ImageDraw.polygon` filled legend swatch / leader arrowhead), a `Stamp` case (`Image.paste` alpha-masked watermark/logo/seal composite over an opacity-scaled `getchannel("A")`/`point`/`putalpha` mask), and a `Frame` case carrying the border/fit/exif/`FilterKind` finish axis — folded by one total `match` over the gated worker so a new annotation primitive (a callout `ImageDraw.arc`, a pie wedge `ImageDraw.pieslice`, a full-frame `ImageChops` blend) is one case, never a parallel spec struct; `RasterSource` the closed-payload `tagged_union` collapsing `svg_string` markup and an `svg_path` `.svg`/`.svgz` file into one source case whose `keywords()` projects the live `svg_to_bytes` source keyword so a file source never grows a second render call; `MarkKind` the closed `Enum` of registration-overlay primitives (`CORNER` crop guide, `TICK` cut mark, `TARGET` color-bar dot, `REGISTRATION` concentric press target, `GUTTER` fold diamond, `MITER` corner chevron, `BLEED` trim box, `CROSS` four-spoke registration cross-hair, `STAR` eight-spoke slur/density target) whose every member carries its own catalogued-primitive name plus float-only arg row and resolves one `svgelements` shape through its bound `MarkKind.shape` builder so the overlay fold dispatches total over the member, never a `dict[MarkKind, Callable]` rebuilt per call against an erased shape bag; `FilterKind` the closed raster-finish vocabulary carried on the `Frame` case. The `graphic/vector#VECTOR` owner — not this one — owns the `svgelements` `SVG` document working surface, the `Matrix`/`Path`/`Color`/`Length`/`Angle`/`Point`/`bbox` algebra, the `bounds`/`path`/`transform`/`svg`/`px` composition functions, the `Element` `Protocol`, the `RenderPolicy` row, and the `resvg_py.svg_to_bytes` raster floor; figure composition IMPORTS that public surface and composes it one hop for every vector operation, re-declaring none of it, while the `pillow` `Image` is the raster annotate/metadata surface this owner holds on the gated floor.
- Cases: `FigureOp` cases — `ScaleFit(source, width, height)` (resolve the source viewport, derive the `Matrix.scale` that fits a target `Length` box preserving aspect, re-emit the transformed SVG) · `Tile(sources, columns, cell, gutter)` (n-up sheet — aspect-fit each source SVG centered into a gutter-spaced row-major grid cell, each placement a `Matrix.translate`-after-`Matrix.scale` of the source bounds into the cell, the gutter the inter-cell margin a press sheet needs and the centering the offset a top-left cram omits) · `Crop(source, x, y, width, height)` (the factory folds the four floats into one `Bounds` case payload; the `bbox` overlap pre-filter drops the fully-outside elements and the `_clipped` egress wraps the survivors in a `<clipPath>`-`<rect>`-bounded `<g clip-path="url(#crop)">` so the straddling geometry is genuinely clipped to the crop rect, not overlap-translated and left overflowing the viewBox) · `Rotate(source, angle, corner)` (rotate-place the source by an `Angle`-resolved `Matrix.rotate` about a `bbox` corner pivot) · `Overlay(source, marks)` (registration marks, gutters, and crop guides — fold a `MarkSpec` row list, each `MarkSpec.kind` member building one `svgelements` shape through its own bound `MarkKind.shape` builder positioned by document `bbox` corner offsets, every fragment serialized through the one `_path` styled-egress owner carrying a `Color`-admitted `Style` stroke axis, all appended to the source `SVG` document) · `Pdf(source, scale)` (lift the placed flat `<svg>` into a one-page vector PDF through `vl_convert.svg_to_pdf(source, scale=)` — the documented post-edit-SVG-then-PDF path that wraps the scale-fit/tile/crop/rotate/overlay-placed document the `composition/sheet#SHEET` and `composition/imposition#IMPOSE` consumers draw through `pymupdf.show_pdf_page`; a pure SVG-to-PDF projection of an already-placed document, never a re-composition) · `Annotate(source, render, draws)` (rasterize-then-draw — the `graphic/vector#VECTOR` `resvg_py.svg_to_bytes` floor rasterizes the `RasterSource` case on the core under the one `RenderPolicy`, then the gated band folds the `DrawOp` sequence by one total `match` — the `Text` arm runs `ImageDraw.text`, the `Box` arm `ImageDraw.rectangle`, the `Line` arm `ImageDraw.line`, the `Ellipse` arm `ImageDraw.ellipse`, the `Polygon` arm `ImageDraw.polygon`, the `Stamp` arm the `Image.paste` alpha-masked composite over a `getchannel("A")`/`point`/`putalpha` opacity mask, the `Frame` arm the `ImageOps.exif_transpose`/`expand`/`fit` frame pass and the `FilterKind` `ImageFilter`/`ImageEnhance` finish, re-binding the draw surface after each image-replacing frame op) · `Metadata(source, exif, xmp)` (bind/read EXIF and XMP — `Image.getexif`/`Image.Exif` tag map plus the `info["XML:com.adobe.xmp"]` XMP packet, re-encoded on save) — matched by one total `match`/`case`; never a sibling op per source media type, never a parallel mark emitter per primitive, and never a parallel figure class.
- Entry: `Figure.of` is `async` over the runtime `async_boundary` under `catch=_FAULTS` — the real engine raise tuple `(ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)` the providers throw (resvg/svgelements/vl-convert `ValueError` on invalid or empty SVG and render failure, `OSError` on the `.svgz` file arm, `BrokenWorkerProcess` from the gated `to_process` worker death, `BeartypeCallHintViolation` from the `_GUARD`-contracted `_fit`/`_rows` division seam on a degenerate source extent or a non-positive column count — `BrokenWorkerInterpreter` is excluded because this owner offloads only through `to_process`, never `to_interpreter`), so the boundary lifts each engine raise onto the runtime `BoundaryFault` rail while a non-engine raise propagates as a defect rather than riding the `Exception` catch-all the rail law rejects, the `CLASSIFY` mapping of each caught raise onto its fault case (`ValueError` to `boundary`, the `OSError`/broken-worker set to `resource`) the runtime `faults#FAULTS` owner's, not re-spelled here; cancellation is excluded from the tuple and re-raises as the structured signal. It dispatches the `FigureOp` case and returns a `RuntimeRail[ContentKey]`; the `ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay` vector arms render in-process composing the IMPORTED `graphic/vector#VECTOR` `bounds`/`transform`/`path`/`svg`/`px` surface (resolving on the host-free cp315 core where `svgelements` is pure-Python and `resvg_py` the cp315 native extension), the `Pdf` arm lifts the placed flat `<svg>` to a one-page vector PDF in-process through the core-resident `vl_convert.svg_to_pdf` (the Rust/Deno-embedded engine importing at boundary scope on the core, no process seam), the `Annotate` arm rasterizes its `RasterSource` case through `resvg_py.svg_to_bytes` on the core then rides the gated-band process seam for the `pillow` draw/filter pass, the `Metadata` arm rides the process seam directly; both gated workers import `PIL` at boundary scope inside the subprocess function so no gated distribution touches the cp315-core page.
- Auto: `_emit` folds the op — the vector arms (`ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay`) through `_compose_vector` which parses each source through the imported `bounds` (reading the document `bbox` through the imported `Element` protocol), composes the placement `Matrix` from `Matrix.translate`/`scale`/`rotate` or folds the `MarkSpec` rows whose `kind` member builds its own bound `MarkKind.shape` `svgelements` shape, and routes every fragment — base elements through the imported `transform` and styled overlay marks through the imported `path` carrying its `Color`-admitted `Style` stroke axis — onto the imported `svg` viewBox egress (or the one compose-owned `_clipped` `<clipPath>` egress for the crop arm); the `Annotate` arm rasterizes inline through `resvg_py.svg_to_bytes(**render.kwargs(source.keywords()))` at the one `_emit` dispatch site where the IMPORTED `RenderPolicy.kwargs` merges the `RasterSource.keywords()` source key with the `asdict`-projected sizing/parsing/font/policy/diagnostic axis, then hands PNG bytes to the gated-band worker where `Image.open`, `ImageDraw.Draw`, and the `DrawOp` fold's `ImageDraw.text`/`rectangle`/`line`/`ellipse`/`polygon`, the `Stamp` arm's `Image.paste`/`Image.getchannel`/`point`/`putalpha` alpha composite, `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`expand`/`fit`/`autocontrast`/`equalize`, `ImageFilter.UnsharpMask`/`GaussianBlur`, and `ImageEnhance.Sharpness`/`Contrast`/`Brightness`/`Color` resolve at boundary scope; the `Pdf` arm lifts the already-placed flat `<svg>` to a one-page vector PDF inline through `vl_convert.svg_to_pdf(source.decode(), scale=scale)` at the one `_emit` dispatch site, a pure projection that re-composes nothing; the `Metadata` arm runs the `Image.open`/`Image.getexif`/`Image.Exif`/XMP map directly on the gated band, folding the EXIF tag pairs through one `exif.update(...)`. The placement helpers this owner holds — `_compose_vector` (the scale-fit/tile/crop/rotate/overlay arm bodies), `_fit`/`_rows` (the `_GUARD`-contracted aspect-fit and grid-count division seams refining the divisor scalar), `_place` (n-up cell math), `_marks` (the registration-overlay mark fold both the flat and per-layer egresses compose), `_anchor` (corner placement), `_hit_elements`/`_hits` (crop bbox overlap pre-filter over the imported `elements(source)`), and `_clipped` (the `<clipPath>` crop egress composed onto the imported flat `svg`) — compose the imported geometry surface rather than re-implementing it; the geometry primitives themselves live once in `graphic/vector#VECTOR`. The in-process arms bind the placed bytes and the case-minted facts to one local in `_placed(op)`, so within any single projection the `ArtifactReceipt.Preview(key, width, height)`/`ArtifactReceipt.Pdf(key, bytes, pages)` evidence and the content key derive from the SAME `_compose_vector`/`svg_to_pdf` result and cannot diverge — never the deleted WITHIN-CALL twice-over that read width/height/byte-count off a SECOND re-emit beside the key's. `_placed` is a pure deterministic fold `of`/`contribute`/`layers` each re-enter (the frozen owner carries no `@cache` memo, and the recompute matches the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `Composed` re-entry); the async raster arms' receipt rides the `of` emission outward because their PNG mints only inside the subprocess `_emit`.
- Receipt: each in-process placement operation contributes through the `core/receipt#RECEIPT` owner's named `ArtifactReceipt.Preview(key, width, height)` `@classmethod` mint carrying the placed figure's pixel/viewport width and height under the `preview` kind (the optional `scores` perceptual band defaulting empty — placement carries no perceptual measurement, that band is `graphic/raster/measure#MEASURE`'s), while the `Pdf` projection mints `ArtifactReceipt.Pdf(key, bytes, 1)` under the `pdf` kind — figure composition reuses the existing `preview`/`pdf` kinds through the owner's flat-scalar named mints and adds NO new kind and NO new evidence `Struct`; the phantom `ArtifactReceipt.of(key, PreviewFacts(...))`/`PdfFacts` evidence-struct indirection is the rejected form the receipt owner explicitly deleted (its named per-kind `@classmethod` mints take the scalars positionally, never an `of(key, facts)` re-wrap, the same form the `sheet`/`imposition` siblings already mint through). `_placed(op)` folds the bytes and the placed viewport (read off the imported `bounds`, or the PDF byte count) into ONE `Placed` carrier per call, so the content key and the receipt read the SAME bytes within that fold — the deleted form re-emitted the whole placement once for the key and AGAIN purely to read width/height/byte-count, a redundant within-call re-composition that could diverge from the emitted bytes. `Figure.contribute()` is the `ReceiptContributor` projection over `_placed` threading the receipt's own `ArtifactReceipt.contribute()` generator — no `phase` parameter, because an `ArtifactReceipt` is by construction emitted-artifact evidence so the phase is the constant `"emitted"` the receipt owner fixes and the KNOB_TEST deletes (the rejected `Figure.contribute(phase)` the receipt owner names by that exact spelling); the `admitted`/`planned` lifecycle facts are NOT figure cases — the `core/plan#PLAN` planner mints them through its own direct `Receipt.of("artifacts", ("planned", ...))`, so this projection carries only the in-process `ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay`/`Pdf` emitted evidence; the gated `Annotate`/`Metadata` raster facts are unreproducible synchronously (the subprocess `to_process` pass mints the PNG only inside the async `_emit`), so their `Placed` carrier holds `Nothing` and `contribute` yields no row for them — their receipt rides the async `Figure.of` emission outward, the one outward async-receipt seam this owner does not close in-process.
- Packages: `graphic/vector#VECTOR` (the IMPORTED public geometry surface — `bounds(source) -> Bounds`, `elements(source) -> list[Element]`, `transform(source, matrix) -> list[str]`, `path(geometry, matrix, style) -> str`, `svg(fragments, width, height) -> bytes`, `px(length) -> float`, the `Element` `Protocol`, the `RenderPolicy` rasterize-policy owner and its `kwargs(source)` `svg_to_bytes` spread, and the `resvg_py.svg_to_bytes` raster floor those compose; figure composition re-declares none of these and resolves each in one hop, while the rotate angle rides the local `_angle` over `svgelements.Angle.parse`); `svgelements` (`Matrix`/`Matrix.scale`/`Matrix.translate`/`Matrix.rotate` composed by `*` to BUILD the placement affine the imported `transform` then applies, and the `Rect`/`Circle`/`Ellipse`/`Polygon`/`Polyline`/`SimpleLine` primitives the `MarkKind.shape` builder constructs — the placement-math use of the geometry algebra, never a re-implemented bounds/serialize surface, pure-Python `py2.py3-none-any` v1.9.6 on the cp315 core, version via `SVGELEMENTS_VERSION`); `pillow` (`Image.open`/`Image.getexif`/`Image.Exif`/`Image.Resampling`/`Image.paste`/`Image.getchannel`, `ImageDraw.Draw`/`text`/`rectangle`/`line`/`ellipse`/`polygon`, `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`expand`/`fit`/`autocontrast`/`equalize`, `ImageFilter.UnsharpMask`/`GaussianBlur`/`MedianFilter`, `ImageEnhance.Sharpness`/`Contrast`/`Brightness`/`Color`) gated `python_version<'3.15'`, offloaded through `anyio.to_process.run_sync`; `vl-convert-python` (`svg_to_pdf(svg, scale=) -> bytes`, the Rust/Deno-embedded `resvg`-core SVG-to-vector-PDF wrap for the `Pdf` placed-figure projection, on the cp315 core, distinct from the chart-render rows `visualization/chart#EXPORT` owns); `core/receipt#RECEIPT` (the named `ArtifactReceipt.Preview(key, width, height)` and `ArtifactReceipt.Pdf(key, bytes, pages)` flat-scalar `@classmethod` mints the placement and PDF-projection arms call positionally, and the no-phase `ArtifactReceipt.contribute()` the `Figure.contribute` fold drives); runtime (`content_identity.ContentIdentity`/`ContentKey`, `receipts.Receipt`/`ReceiptContributor`, `faults.RuntimeRail`/`async_boundary` under the narrowed `catch=_FAULTS` raise tuple); `anyio` (`to_process.run_sync` the GIL-hostile offload lane the gated `pillow` band rides under one shared `CapacityLimiter`, raising `BrokenWorkerProcess` on worker death); `beartype` (`BeartypeConf(violation_type=BeartypeCallHintViolation)` the `_GUARD` contract weave on the `_fit`/`_rows` division seams, `beartype.vale.Is` the `Extent`/`Columns` divisor refinements whose violation rides the `_FAULTS` tuple as the surfaces-and-dispatch contract aspect the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `_GUARD` seam carries).
- Growth: a new vector layout operation (a bleed-and-trim sheet, a margin-frame matte) is one `FigureOp` case plus one `_compose_vector` arm over the imported `bounds`/`transform`/`svg` surface and the `svgelements` `Matrix`/`angle` algebra — never a re-implemented SVG transform, while an n-up refinement (inter-cell gutter, cell alignment) is one field on the existing `Tile` case the `_place` math already reads; a new registration-overlay primitive (a collation lozenge, a page-information ladder mark) is one `MarkKind` member carrying its own catalogued-primitive name and float-only arg row resolved through the shared `MarkKind.shape` builder, dispatched by the same total overlay fold and serialized through the imported `path` styled owner — never a parallel mark emitter, never a `dict` arm grafted onto an erased shape bag; a new annotation source mode (a placed working document, a `.svgz` archive) is one `RasterSource` case projecting one entry into the `svg_to_bytes` source keyword on the one render call — never a second render entrypoint; a new raster annotation primitive (a curved callout `ImageDraw.arc`, a pie wedge `ImageDraw.pieslice`, a full-frame `ImageChops` blend composite) is one `DrawOp` case plus one `match` arm on the gated worker, and a new finish filter (a 3D-LUT grade via `ImageFilter.Color3DLUT`, an `ImageOps.colorize` recolor) is one `FilterKind` token plus one `match` arm on the `Frame` finish fold — never a parallel spec struct, never a per-primitive draw loop; a new resvg sizing/font/policy/diagnostic knob grows the IMPORTED `graphic/vector#VECTOR` `RenderPolicy` row at its owner, reaching this owner with zero edit — never a second rasterizer here; a new PDF-projection knob (a fit-to-page scale, a target page box) is one field on the existing `Pdf` case carried into the one `vl_convert.svg_to_pdf` call — never a second SVG-to-PDF sink or a parallel PDF writer; a new metadata channel (IPTC, ICC-profile name) is one tag read/write on the existing `Image` map. Zero new surface.
- Boundary: a per-graphic-type figure-composer class family, a per-primitive mark emitter, a per-input-mode render entrypoint, a `MarkKind`-keyed `dict[MarkKind, Callable]` shape table rebuilt per mark call, a per-`Corner` anchor `dict` literal rebuilt per call beside the total-`match` projection, a re-declared copy of the `graphic/vector#VECTOR` `path`/`svg`/`bounds`/`transform`/`px` surface or its `Element`/`RenderPolicy` owners beside the imported public ones, a single-call `_rasterize` forwarding hop beside the inline `svg_to_bytes` dispatch, a parallel `CaptionSpec`/`BoxSpec`/`PostSpec` draw-spec triple swept by three `for` loops plus scattered `if post` branches, a recompute receipt re-running `_compose_vector`/`svg_to_pdf` to read facts `_emit` already produced, a per-kind facts `Struct` (`PreviewFacts`/`PdfFacts`) or an `ArtifactReceipt.of(key, facts)` indirection re-wrapping the scalars the receipt owner's named `ArtifactReceipt.Preview`/`Pdf` mints already take positionally, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live viewer, no chart re-render. The `graphic/vector#VECTOR` owner owns SVG geometry/transform/parse/bounds/primitives/serialize and the `resvg_py` raster floor — figure composition IMPORTS that owner's public surface and COMPOSES it: it reads `bounds` (over the imported `Element` protocol), builds `SimpleLine`/`Circle`/`Ellipse`/`Polyline`/`Polygon`/`Rect` overlay shapes through the `MarkKind` member's own bound `MarkKind.shape` builder, composes the placement `Matrix` from the `svgelements` factories, and routes every fragment — transformed base element through the imported `transform`/`path` and styled overlay mark through the imported `path` — onto the imported `svg` viewBox egress (or the compose-owned `_clipped` crop egress), re-declaring no geometry function, no affine helper, no hand-emitted `<rect>`/`<line>` string, and no second SVG geometry engine. The vector owner's `RenderPolicy.kwargs` rasterizes the `RasterSource` case for the annotate pass over the one `svg_to_bytes` call whose `svg_string`/`svg_path` keywords carry markup and the `.svg`/`.svgz` file arm together; chart/mark/nanoplot rasterization-to-export routes to `visualization/chart#EXPORT` `vl-convert`, and the SVG sources arrive from the regrouped `visualization/chart#CHART`, `graphic/marks#MARK`, and `visualization/table#TABLE` siblings. The `Pdf` arm's `vl_convert.svg_to_pdf` wrap is the placed-flat-`<svg>`-to-one-page-vector-PDF projection — the documented post-edit-SVG-then-PDF path — distinct from the chart-spec render rows (`vegalite_to_pdf`/`vega_to_pdf`) `visualization/chart#EXPORT` owns: composition wraps an already-placed figure document, it renders no chart spec, so a second SVG-to-PDF sink, a chart re-render, or a `reportlab`/`weasyprint` PDF writer beside the one `svg_to_pdf` projection are the deleted forms, and the one-page figure PDF is the artifact the `composition/sheet#SHEET` `Place` and `composition/imposition#IMPOSE` arms draw through `pymupdf.show_pdf_page`. The placed multi-source layout the vector arms emit is a flat single-`<svg>` document and that flat-SVG egress stays this owner's concern; the named-layer editable egress is `export/layered#LAYERED`'s — the `Figure.layers(names)` projection exposes the placed layout as `tuple[Layer, ...]` (one `Layer(name, source, bbox)` row per placed source carrying its placed `bbox`: one row per `Tile` cell, a `base`-plus-`overlay` pair per `Overlay`, one row per single-source placement) handed to that owner to bind into named `drawsvg` groups and PDF OCG layers, so figure composition authors no named-layer structure and grows no per-layer emit arm — it projects the placed-source rows and routes the layer authoring outward; a layered-export arm grafted onto `FigureOp` and a hierarchical-group SVG emitter beside the one flat `_svg` document are the deleted forms. `pillow` annotate/metadata rides the gated `python_version<'3.15'` band where the wheel ships and never resolves in the cp315-core process, so the `pillow` pass dispatches onto the `anyio.to_process` offload lane (the GIL-hostile native isolation, not a cross-version dispatch) where the subprocess worker imports `PIL` at boundary scope inside the function — neither a module-top nor a core-page import lands, while `resvg_py` likewise imports at boundary scope on the core. ICC profile attachment and color management stay in `graphic/color/managed#MANAGED`; figure composition consumes a color-managed raster, it does not build the transform.

```python signature
from collections.abc import Callable, Iterable, Sequence
from enum import Enum
from typing import TYPE_CHECKING, Annotated, Literal, assert_never

from anyio import BrokenWorkerProcess, CapacityLimiter, to_process
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Nothing, Option, Some, case, tag, tagged_union
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

# graphic/vector#VECTOR owns the SVG-geometry primitive; figure composition IMPORTS its public
# composition surface and re-declares none of it — `bounds`/`elements`/`transform`/`path`/`svg`/`px`
# resolve geometry in one hop, `elements(source)` is the parse-and-bbox-filter the crop pre-filter
# reads (the page re-parses no document), `Element` is the bbox protocol it yields, and `RenderPolicy`
# is the resvg rasterize-policy owner carried into the `Annotate` case.
from artifacts.graphic.vector import Bounds, Element, Length, RenderPolicy, Style, bounds, elements, path, px, svg, transform
from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer

if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt
    from svgelements import Angle, Matrix

# resvg/svgelements/vl-convert raise `ValueError` on invalid SVG, an empty document, or render
# failure; the gated `to_process` worker death raises `BrokenWorkerProcess`; `OSError` rides the
# `.svgz` file arm; the `_GUARD`-contracted `_fit`/`_rows` division seams raise
# `BeartypeCallHintViolation` on a degenerate (zero-extent) source bbox or a non-positive n-up column
# count. The boundary narrows `catch` to this real raise tuple so a non-engine raise propagates as a
# defect rather than railing through the `Exception` catch-all the rail law rejects; the runtime
# `faults#FAULTS` `CLASSIFY` table maps each caught raise onto its `BoundaryFault` case.
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)

# the gated `pillow` band's bounded offload slot: every `to_process.run_sync` arm threads this one
# `CapacityLimiter` so N concurrent figures share a fixed subprocess pool instead of fanning out at
# the per-loop `current_default_process_limiter()` CPU-count default the concurrency owner rejects.
_GATE: CapacityLimiter = CapacityLimiter(4)

# the surfaces-and-dispatch contract aspect: the `_fit`/`_rows` division seams refine their divisor
# scalars (`Extent`/`Columns`), so a degenerate source extent or a non-positive grid count rails as
# `BeartypeCallHintViolation` the `_FAULTS` boundary converts rather than a `ZeroDivisionError` deep in
# the placement fold (a `Bounds`/case field is not deep-checked by beartype — only a direct scalar
# parameter is), matching the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `_GUARD` seam.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


type Corner = Literal["nw", "ne", "sw", "se", "n", "s", "e", "w", "center"]
type Anchor = tuple[float, float]
type Extent = Annotated[float, Is[lambda value: value > 0.0]]
type Columns = Annotated[int, Is[lambda count: count >= 1]]
type MarkArgs = Callable[[float, float, float], tuple[float, ...]]
type FilterKind = Literal["unsharp", "blur", "median", "sharpness", "contrast", "brightness", "saturation", "autocontrast", "equalize"]


class MarkKind(Enum):
    CORNER = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y))
    TICK = ("SimpleLine", lambda x, y, s: (x - s, y, x + s, y))
    TARGET = ("Circle", lambda x, y, s: (x, y, s))
    REGISTRATION = ("Ellipse", lambda x, y, s: (x, y, s, s * 0.5))
    GUTTER = ("Polyline", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s, x - s, y))
    MITER = ("Polygon", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s))
    BLEED = ("Rect", lambda x, y, s: (x - s, y - s, 2.0 * s, 2.0 * s))
    CROSS = ("Polyline", lambda x, y, s: (x - s, y, x, y, x, y - s, x, y, x + s, y, x, y, x, y + s))
    STAR = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y - s, x, y, x + s, y, x, y, x + s, y + s, x, y, x, y + s, x, y, x - s, y + s, x, y, x - s, y, x, y, x - s, y - s))

    def __init__(self, primitive: str, args: MarkArgs) -> None:
        self.primitive = primitive
        self.args = args

    def shape(self, anchor: Anchor, size: float) -> object:
        import svgelements

        return getattr(svgelements, self.primitive)(*self.args(*anchor, size))


class MarkSpec(Struct, frozen=True):
    kind: MarkKind
    corner: Corner = "nw"
    size: float = 12.0
    inset: float = 6.0
    stroke: str = "black"
    width: float = 0.5


@tagged_union(frozen=True)
class RasterSource:
    tag: Literal["markup", "file"] = tag()
    markup: bytes = case()
    file: str = case()

    @staticmethod
    def Markup(svg: bytes) -> "RasterSource":
        return RasterSource(markup=svg)

    @staticmethod
    def File(path: str) -> "RasterSource":
        return RasterSource(file=path)

    def keywords(self) -> dict[str, str]:
        match self:
            case RasterSource(tag="markup", markup=svg):
                return {"svg_string": svg.decode()}
            case RasterSource(tag="file", file=path):
                return {"svg_path": path}
            case _:
                assert_never(self)


@tagged_union(frozen=True)
class DrawOp:
    tag: Literal["text", "box", "line", "ellipse", "polygon", "stamp", "frame"] = tag()
    text: tuple[str, Anchor, str | None, float, str, str | None] = case()
    box: tuple[Bounds, str, str | None, int] = case()
    line: tuple[tuple[Anchor, ...], str, int] = case()
    ellipse: tuple[Bounds, str | None, str | None, int] = case()
    polygon: tuple[tuple[Anchor, ...], str | None, str, int] = case()
    stamp: tuple[bytes, Anchor, float] = case()
    frame: tuple[FilterKind | None, float, int, str, tuple[int, int] | None, bool] = case()

    @staticmethod
    def Text(text: str, xy: Anchor, fill: str = "black", font: str | None = None, size: float = 16.0, anchor: str | None = None) -> "DrawOp":
        return DrawOp(text=(text, xy, font, size, fill, anchor))

    @staticmethod
    def Box(box: Bounds, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(box=(box, outline, fill, width))

    @staticmethod
    def Line(points: tuple[Anchor, ...], fill: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(line=(points, fill, width))

    @staticmethod
    def Ellipse(box: Bounds, outline: str | None = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(ellipse=(box, outline, fill, width))

    @staticmethod
    def Polygon(points: tuple[Anchor, ...], fill: str | None = None, outline: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(polygon=(points, fill, outline, width))

    @staticmethod
    def Stamp(image: bytes, xy: Anchor, opacity: float = 1.0) -> "DrawOp":
        return DrawOp(stamp=(image, xy, opacity))

    @staticmethod
    def Frame(filter: FilterKind | None = None, radius: float = 2.0, border: int = 0, border_fill: str = "white", fit: tuple[int, int] | None = None, exif_orient: bool = False) -> "DrawOp":
        return DrawOp(frame=(filter, radius, border, border_fill, fit, exif_orient))


@tagged_union(frozen=True)
class FigureOp:
    tag: Literal["scale_fit", "tile", "crop", "rotate", "overlay", "pdf", "annotate", "metadata"] = tag()
    scale_fit: tuple[bytes, Length, Length] = case()
    tile: tuple[tuple[bytes, ...], int, Length, Length, float] = case()
    crop: tuple[bytes, Bounds] = case()
    rotate: tuple[bytes, str, Corner] = case()
    overlay: tuple[bytes, tuple[MarkSpec, ...]] = case()
    pdf: tuple[bytes, float] = case()
    annotate: tuple[RasterSource, RenderPolicy, tuple[DrawOp, ...]] = case()
    metadata: tuple[bytes, tuple[tuple[int, str], ...], str | None] = case()

    @staticmethod
    def ScaleFit(source: bytes, width: Length, height: Length) -> "FigureOp":
        return FigureOp(scale_fit=(source, width, height))

    @staticmethod
    def Tile(sources: tuple[bytes, ...], columns: int, cell_width: Length, cell_height: Length, gutter: float = 0.0) -> "FigureOp":
        return FigureOp(tile=(sources, columns, cell_width, cell_height, gutter))

    @staticmethod
    def Crop(source: bytes, x: float, y: float, width: float, height: float) -> "FigureOp":
        return FigureOp(crop=(source, (x, y, x + width, y + height)))

    @staticmethod
    def Rotate(source: bytes, angle: str, corner: Corner = "center") -> "FigureOp":
        return FigureOp(rotate=(source, angle, corner))

    @staticmethod
    def Overlay(source: bytes, marks: tuple[MarkSpec, ...]) -> "FigureOp":
        return FigureOp(overlay=(source, marks))

    @staticmethod
    def Pdf(source: bytes, scale: float = 1.0) -> "FigureOp":
        return FigureOp(pdf=(source, scale))

    @staticmethod
    def Annotate(source: RasterSource, render: RenderPolicy = RenderPolicy(), draws: tuple[DrawOp, ...] = ()) -> "FigureOp":
        return FigureOp(annotate=(source, render, draws))

    @staticmethod
    def Metadata(source: bytes, exif: tuple[tuple[int, str], ...] = (), xmp: str | None = None) -> "FigureOp":
        return FigureOp(metadata=(source, exif, xmp))


# the sync placement bound once per call: one `_compose_vector`/`svg_to_pdf` result feeds the content
# key AND the receipt evidence off the same local `data`, so the key and the facts can never diverge
# within a projection. `_placed` is a pure deterministic fold each projection re-enters (frozen owner,
# no mutable memo), matching the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `Composed` form.
class Placed(Struct, frozen=True):
    key: ContentKey
    data: bytes
    receipt: Option[ArtifactReceipt]


class Figure(Struct, frozen=True):
    op: FigureOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"figure.{self.op.tag}", self._emit, catch=_FAULTS)

    async def _emit(self) -> ContentKey:
        match self.op:
            case FigureOp(tag="annotate", annotate=(source, render, draws)):
                import resvg_py

                data = await to_process.run_sync(_gated_annotate, resvg_py.svg_to_bytes(**render.kwargs(source.keywords())), draws, limiter=_GATE)
            case FigureOp(tag="metadata", metadata=(source, exif, xmp)):
                data = await to_process.run_sync(_gated_metadata, source, exif, xmp, limiter=_GATE)
            case _:
                return _placed(self.op).key
        return ContentIdentity.of(f"figure-{self.op.tag}", data)

    def contribute(self) -> "Iterable[Receipt]":
        return _placed(self.op).receipt.map(lambda receipt: tuple(receipt.contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


# the gated `Annotate`/`Metadata` arms mint their raster facts inside the async `_emit`, so this
# sync carrier holds `Nothing` and a placeholder key for them; the runtime threads their emitted key.
def _placed(op: FigureOp) -> Placed:
    match op:
        case FigureOp(tag="pdf", pdf=(source, scale)):
            import vl_convert

            data = vl_convert.svg_to_pdf(source.decode(), scale=scale)
            key = ContentIdentity.of("figure-pdf", data)
            return Placed(key, data, Some(ArtifactReceipt.Pdf(key, len(data), 1)))
        case FigureOp(tag="annotate") | FigureOp(tag="metadata"):
            return Placed(ContentIdentity.of(f"figure-{op.tag}", b""), b"", Nothing)
        case _:
            data = _compose_vector(op)
            xmin, ymin, xmax, ymax = bounds(data)
            key = ContentIdentity.of(f"figure-{op.tag}", data)
            return Placed(key, data, Some(ArtifactReceipt.Preview(key, int(xmax - xmin), int(ymax - ymin))))


def _placed_layers(op: FigureOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    from svgelements import Matrix

    match op:
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            width, height = cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1)
            return tuple(
                Layer(
                    _name(names, index),
                    svg(_place(raw, index, columns, cw, ch, gutter), width, height),
                    (index % columns * (cw + gutter), index // columns * (ch + gutter), index % columns * (cw + gutter) + cw, index // columns * (ch + gutter) + ch),
                )
                for index, raw in enumerate(sources)
            )
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = bounds(source)
            xmin, ymin, xmax, ymax = extent
            base = Layer(_name(names, 0, "base"), svg(transform(source, Matrix()), xmax - xmin, ymax - ymin), extent)
            return (base, Layer(_name(names, 1, "overlay"), svg(_marks(extent, marks), xmax - xmin, ymax - ymin), extent))
        case FigureOp(tag="scale_fit", scale_fit=(source, *_)) | FigureOp(tag="crop", crop=(source, *_)) | FigureOp(tag="rotate", rotate=(source, *_)):
            # the single-source placed layer reads its OWN placed extent off the emitted bytes —
            # `scale_fit`/`crop` re-origin the viewport, so the source-document bounds is the wrong frame.
            placed = _compose_vector(op)
            return (Layer(_name(names, 0), placed, bounds(placed)),)
        case _:
            assert_never(op)


def _name(names: tuple[str, ...], index: int, fallback: str = "") -> str:
    return names[index] if index < len(names) else fallback or f"layer-{index}"


# the `_GUARD`-contracted division seams: an aspect-fit divides by the source extent and the n-up grid
# by the column count, so a degenerate zero-extent bbox or a non-positive `columns` rails as a
# `BeartypeCallHintViolation` at the refined `Extent`/`Columns` scalar rather than a `ZeroDivisionError`
# deep in the placement fold — the one place the placement math admits an external scalar.
@_GUARD
def _fit(width: float, height: float, extent_w: Extent, extent_h: Extent, /) -> float:
    return min(width / extent_w, height / extent_h)


@_GUARD
def _rows(count: int, columns: Columns, /) -> int:
    return -(-count // columns)


# the placement-math arm bodies this owner holds over the imported `graphic/vector#VECTOR` surface;
# the geometry primitives (`bounds`/`elements`/`transform`/`path`/`svg`/`px`) live once in that owner.
def _compose_vector(op: FigureOp) -> bytes:
    from svgelements import Matrix

    match op:
        case FigureOp(tag="scale_fit", scale_fit=(source, width, height)):
            (xmin, ymin, xmax, ymax), (tw, th) = bounds(source), (px(width), px(height))
            factor = _fit(tw, th, xmax - xmin, ymax - ymin)
            return svg(transform(source, Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)), tw, th)
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            placed = [fragment for index, raw in enumerate(sources) for fragment in _place(raw, index, columns, cw, ch, gutter)]
            return svg(placed, cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1))
        case FigureOp(tag="crop", crop=(source, box)):
            x0, y0, x1, y1 = box
            kept = [path(element, Matrix.translate(-x0, -y0)) for element in _hit_elements(source, box)]
            return _clipped(kept, x1 - x0, y1 - y0)
        case FigureOp(tag="rotate", rotate=(source, angle, corner)):
            extent = bounds(source)
            xmin, ymin, xmax, ymax = extent
            ax, ay = _anchor(corner, extent, 0.0)
            pivot = Matrix.translate(ax, ay) * Matrix.rotate(_angle(angle)) * Matrix.translate(-ax, -ay)
            return svg(transform(source, pivot), xmax - xmin, ymax - ymin)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = bounds(source)
            xmin, ymin, xmax, ymax = extent
            return svg([*transform(source, Matrix()), *_marks(extent, marks)], xmax - xmin, ymax - ymin)
        case _:
            assert_never(op)


# the registration-overlay mark fold — each `MarkSpec` builds its own `svgelements` shape through the
# bound `MarkKind.shape` row, positioned by document-corner offset and serialized through the imported
# `path` styled owner; the flat-`svg` and per-layer egresses both compose this one fold.
def _marks(extent: Bounds, marks: tuple[MarkSpec, ...]) -> list[str]:
    from svgelements import Matrix

    return [path(spec.kind.shape(_anchor(spec.corner, extent, spec.inset), spec.size), Matrix(), (spec.stroke, spec.width)) for spec in marks]


def _anchor(corner: Corner, extent: Bounds, inset: float) -> Anchor:
    xmin, ymin, xmax, ymax = extent
    cx, cy = (xmin + xmax) / 2.0, (ymin + ymax) / 2.0
    match corner:
        case "nw":
            return (xmin + inset, ymin + inset)
        case "ne":
            return (xmax - inset, ymin + inset)
        case "sw":
            return (xmin + inset, ymax - inset)
        case "se":
            return (xmax - inset, ymax - inset)
        case "n":
            return (cx, ymin + inset)
        case "s":
            return (cx, ymax - inset)
        case "e":
            return (xmax - inset, cy)
        case "w":
            return (xmin + inset, cy)
        case "center":
            return (cx, cy)
        case _:
            assert_never(corner)


# the crop overlap pre-filter: the imported `graphic/vector#VECTOR` `elements(source)` surface parses
# the document into the bbox-bearing `Element` list once, and only the elements straddling or inside
# the crop box survive — the page re-parses no document, the `SVG.parse` living once in the vector owner.
def _hit_elements(source: bytes, box: Bounds) -> list[Element]:
    return [element for element in elements(source) if _hits(element.bbox(), box)]


def _place(source: bytes, index: int, columns: int, cell_w: float, cell_h: float, gutter: float) -> list[str]:
    from svgelements import Matrix

    xmin, ymin, xmax, ymax = bounds(source)
    factor = _fit(cell_w, cell_h, xmax - xmin, ymax - ymin)
    column, row = index % columns, index // columns
    # aspect-fit then center the source within its gutter-spaced cell — a top-left cram is the naive n-up.
    ox = column * (cell_w + gutter) + (cell_w - (xmax - xmin) * factor) / 2.0
    oy = row * (cell_h + gutter) + (cell_h - (ymax - ymin) * factor) / 2.0
    return transform(source, Matrix.translate(ox - xmin * factor, oy - ymin * factor) * Matrix.scale(factor))


def _angle(value: str) -> "Angle":
    from svgelements import Angle

    return Angle.parse(value)


def _hits(box: Bounds | None, crop: Bounds) -> bool:
    return box is not None and not (box[2] < crop[0] or box[0] > crop[2] or box[3] < crop[1] or box[1] > crop[3])


def _clipped(fragments: Iterable[str], width: float, height: float) -> bytes:
    # the crop straddler is clipped to the rect by the `<defs><clipPath>` plus `<g clip-path>` the
    # imported flat `svg` egress omits — the one egress addition this owner composes ONTO that egress
    # rather than re-emitting the `<svg>` document shell by hand, so the shell lives once in vector.
    defs = f'<defs><clipPath id="crop"><rect x="0" y="0" width="{width}" height="{height}"/></clipPath></defs>'
    return svg((defs, '<g clip-path="url(#crop)">', *fragments, "</g>"), width, height)


def _gated_annotate(payload: bytes, draws: Sequence[DrawOp]) -> bytes:
    from io import BytesIO

    from PIL import Image, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps

    image = Image.open(BytesIO(payload)).convert("RGBA")
    surface = ImageDraw.Draw(image)
    for op in draws:
        match op:
            case DrawOp(tag="text", text=(text, xy, font, size, fill, anchor)):
                face = ImageFont.truetype(font, size) if font is not None else ImageFont.load_default(size)
                surface.text(xy, text, font=face, fill=fill, anchor=anchor)
            case DrawOp(tag="box", box=(box, outline, fill, width)):
                surface.rectangle(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="line", line=(points, fill, width)):
                surface.line(points, fill=fill, width=width)
            case DrawOp(tag="ellipse", ellipse=(box, outline, fill, width)):
                surface.ellipse(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="polygon", polygon=(points, fill, outline, width)):
                surface.polygon(points, fill=fill, outline=outline, width=width)
            case DrawOp(tag="stamp", stamp=(raw, xy, opacity)):
                mark = Image.open(BytesIO(raw)).convert("RGBA")
                mark.putalpha(mark.getchannel("A").point(lambda level: int(level * opacity)))
                image.paste(mark, (int(xy[0]), int(xy[1])), mark)
            case DrawOp(tag="frame", frame=(kind, radius, border, border_fill, fit, exif_orient)):
                image = ImageOps.exif_transpose(image) if exif_orient else image
                image = ImageOps.expand(image, border=border, fill=border_fill) if border else image
                image = ImageOps.fit(image, fit, method=Image.Resampling.LANCZOS) if fit is not None else image
                match kind:
                    case "unsharp":
                        image = image.filter(ImageFilter.UnsharpMask(radius=radius))
                    case "blur":
                        image = image.filter(ImageFilter.GaussianBlur(radius=radius))
                    case "median":
                        image = image.filter(ImageFilter.MedianFilter(size=max(3, int(radius) | 1)))
                    case "sharpness":
                        image = ImageEnhance.Sharpness(image).enhance(radius)
                    case "contrast":
                        image = ImageEnhance.Contrast(image).enhance(radius)
                    case "brightness":
                        image = ImageEnhance.Brightness(image).enhance(radius)
                    case "saturation":
                        image = ImageEnhance.Color(image).enhance(radius)
                    case "autocontrast":
                        image = ImageOps.autocontrast(image.convert("RGB"), cutoff=radius).convert("RGBA")
                    case "equalize":
                        image = ImageOps.equalize(image.convert("RGB")).convert("RGBA")
                    case None:
                        pass
                    case _:
                        assert_never(kind)
                surface = ImageDraw.Draw(image)
            case _:
                assert_never(op)
    sink = BytesIO()
    image.save(sink, format="PNG")
    return sink.getvalue()


def _gated_metadata(payload: bytes, exif_tags: Sequence[tuple[int, str]], xmp: str | None) -> bytes:
    from io import BytesIO

    from PIL import Image

    image = Image.open(BytesIO(payload))
    exif = image.getexif()
    exif.update(exif_tags)
    packet = xmp if xmp is not None else image.info.get("XML:com.adobe.xmp")
    sink = BytesIO()
    image.save(sink, format=image.format or "PNG", exif=exif, xmp=packet.encode() if isinstance(packet, str) else packet)
    return sink.getvalue()
```

## [03]-[RESEARCH]

- [VECTOR_COMPOSED] [OPEN]: the SVG-geometry primitive — `SVG.parse(source, reify=True)`/`SVG.elements`, `Path(geometry)`/`Path.d`/`Path.bbox`, `Matrix.scale`/`Matrix.translate`/`Matrix.rotate` (composed by `*`), `Length(value).value(ppi=...)`, `Angle.parse`, and the `Color(value)` color-admission value object — is OWNED by `graphic/vector#VECTOR` (a VERIFIED REAL `svgelements` reflection, `1.9.6` on the cp315 core, pure-Python `py2.py3-none-any`, version via `SVGELEMENTS_VERSION`), and figure composition now IMPORTS that owner's public composition surface rather than re-declaring it. The prior fence carried byte-identical private copies of `_bounds`/`_path`/`_transform`/`_elements`/`_svg`/`_px` plus the `Element` `Protocol`, the `RenderPolicy` struct, and the `Bounds`/`Length`/`Style`/render-`Literal` aliases — an ILLUSORY collapse: the prose claimed "composes the primitive" while the code re-owned the entire geometry surface verbatim, two divergence-prone copies of one concept. The rebuilt fence imports `bounds(source) -> Bounds`, `elements(source) -> list[Element]`, `transform(source, matrix) -> list[str]`, `path(geometry, matrix, style) -> str`, `svg(fragments, width, height) -> bytes`, `px(length) -> float`, plus `Element`/`RenderPolicy`/`Bounds`/`Length`/`Style`, and holds ONLY the placement-math arm bodies the primitive does not own: the scale-fit factor and the `Matrix.translate`-after-`scale` it builds, the n-up `_place` cell math, the `_anchor` corner placement, the `_hit_elements`/`_hits` crop overlap pre-filter, and the `_clipped` `<clipPath>`-`<rect>`-bounded `<g clip-path="url(#crop)">` egress (the one egress addition the imported flat `svg` omits, closing the straddling-element overflow against the SVG clip mechanism, never a boolean path-intersection svgelements does not own). The cross-file seam is OPEN because the vector owner currently privatizes these as `_bounds`/`_path`/`_transform`/`_svg`/`_px` (underscore-prefixed) and exposes `bounds` only as a comma-joined-bytes `VectorOp.Bounds` arm — so the import resolves only once `graphic/vector#VECTOR` publicizes the byte-sourced composition functions (`bounds`/`elements`/`transform`/`path`/`svg`/`px`) and the `Element`/`RenderPolicy`/`Bounds`/`Length`/`Style` owners as importable module surface. The `MarkKind` `Enum` carries the catalogued primitive name and the float-only arg-builder on each member, so the overlay fold dispatches behavior through the member's own `kind.primitive`/`kind.args` row — the row-owned-behavior collapse of the rejected `dict[MarkKind, Callable]` factory rebuilt per mark call — and `MarkKind.shape` resolves the `svgelements` class once at boundary scope through `getattr(svgelements, self.primitive)`. Base elements and overlay marks both serialize through the imported `path(geometry, matrix, style)` styled-egress owner: the optional `Style` tuple admits the overlay `stroke` literal through the catalogued `Color(value)` parse and emits the `Color(value).hex` channel literal. Close-condition: `graphic/vector#VECTOR` publicizes `bounds`/`elements`/`transform`/`path`/`svg`/`px` and the `Element`/`RenderPolicy`/`Bounds`/`Length`/`Style` owners, and this import resolves with zero re-declared geometry.
- [SHAPE_CTOR_RESEARCH]: the nine `MarkKind` members resolve their `svgelements` shape through `getattr(svgelements, self.primitive)(*self.args(*anchor, size))`, positionally constructing `SimpleLine`/`Circle`/`Ellipse`/`Polyline`/`Polygon`/`Rect` (the `CROSS`/`STAR` registration spokes reuse the catalogued `Polyline` point-sequence, adding no new shape class). The folder `.api` catalogue for `svgelements` rows each primitive by role (`Circle` "circle by center and radius", `Ellipse` "ellipse by center and two radii", `Rect` "axis-aligned rectangle", `Polygon`/`Polyline` "point sequence", `SimpleLine` "single line segment") but does NOT catalogue the positional constructor arity or argument order of any shape — the `Circle(cx, cy, r)`, `Ellipse(cx, cy, rx, ry)`, `Rect(x, y, w, h)`, `SimpleLine(x1, y1, x2, y2)`, and `*points`-spread `Polygon`/`Polyline` call grammar is a RESEARCH item, never settled fence code, until the catalogue reflects each shape's constructor signature. The isolation onto the one `MarkKind.shape` builder keeps every unverified construction spelling at a single cite-point so the `SCALE_FIT`/`TILE`/`CROP` placement arms and the `OVERLAY` algebra stay fully settled; the constructed shape feeds the imported `graphic/vector#VECTOR` `path` styled-egress owner, so this deepen item is shared between this owner's `MarkKind.shape` builder and the vector owner's `path` surface. Close-condition: the catalogue reflects each shape's positional constructor signature — resolved verification either confirms the positional grammar or rebinds each `args` row onto the catalogued keyword constructor.
- [ROTATE_PARSE_SETTLED]: the `ROTATE` arm resolves the angle string through `_angle`, the one boundary-scoped helper holding `svgelements.Angle.parse(angle)`. The folder `.api` catalogue for `svgelements` confirms the `Angle` value object (CSS `deg`/`rad`/`grad`/`turn`), `Matrix.rotate(angle)`, and the value-object row naming `Angle.parse(value)` as the string-admission classmethod — so `Angle.parse` is catalogue-confirmed settled fence code (resolving the prior `[ROTATE_PARSE_RESEARCH]` gate the `graphic/vector#VECTOR` page closed), and the `Rotate` arm's `Matrix.rotate(Angle.parse(...))` composition is settled.
- [RASTER_FLOOR_COMPOSED]: the `ANNOTATE` arm rasterizes its `RasterSource` case through `resvg_py.svg_to_bytes(**render.kwargs(source.keywords()))` on the cp315 core before the gated `pillow` draw/filter pass — the same `graphic/vector#VECTOR` `Rasterize` floor every placement consumer composes; `import resvg_py` resolves at boundary scope inline at the one `_emit` annotate dispatch site, never module-top and never behind a single-call forwarding helper. The `RasterSource` `tagged_union` (this owner's, the source-mode discriminant the annotate pass needs) collapses the `svg_string` markup arm and the `svg_path` `.svg`/`.svgz` file arm onto the one `svg_to_bytes` call — `keywords()` resolves the case to a single-entry `{"svg_string": ...}` or `{"svg_path": ...}` source dict through the statement-form `match`/`case` over the `tag` discriminant (the catalogue documents `tagged_union` with no instance `.match()` method, so the fence dispatches by statement), so a file source never grows a second render entrypoint. The `RenderPolicy` row and its `kwargs(source)` `svg_to_bytes` spread are the IMPORTED `graphic/vector#VECTOR` owner's — that owner's `kwargs` merges the source dict with the `msgspec.structs.asdict(self)` field projection coercing each `()`-default tuple field to `list(value) or None` so `languages`/`font_files`/`font_dirs` arrive as the catalogue's `list[str] | None` shape; figure composition adapts the spread to its `RasterSource` keyword (the vector owner's `kwargs(document: bytes)` keys `svg_string` directly, so the file-source arm rides this owner's `RasterSource.keywords()` projection composed into the same `**` spread). Every `svg_to_bytes` keyword verifies against the folder `.api` catalogue for `resvg-py` (`0.3.3` reflected on cp315, native `resvg_py.cpython-315-darwin.so` embedding the Rust `resvg 0.47.0` engine): the catalogue source/sizing/parsing/font/policy/logging axis table is the settled signature, the `RenderPolicy` field names match the catalogued keyword names one-for-one so the `asdict` spread is total over the signature, `svg_string` and `svg_path` are the one required-one source row pair (`.svgz` decompresses on the path arm), the `dpi=0.0` default defers to the SVG-declared size, the `log_information` boolean prints resvg debug logs as the diagnostics row, and the `shape_rendering`/`text_rendering`/`image_rendering` `Literal` policy defaults (`geometric_precision`/`optimize_legibility`/`optimize_quality`) match the reflected stub. `svg_to_bytes` returns PNG `bytes` and raises `ValueError` on empty or invalid SVG, an unparseable `background`, or render failure. The `graphic/vector#VECTOR` owner owns SVG-to-PNG with no Cairo, headless-browser, or external-process dependency — figure composition never re-implements SVG path flattening, text shaping, or PNG encoding the resvg engine already owns, and the chart/mark/nanoplot rasterization-to-export floor stays in `visualization/chart#EXPORT` `vl-convert`. All resvg-py members are catalogue-confirmed settled fence code; this page carries no resvg-py RESEARCH gate.
- [PDF_PROJECTION] [RESOLVED]: the `Pdf` arm closes the placed-figure-to-PDF seam the `composition/sheet#SHEET` `Place` and `composition/imposition#IMPOSE` arms require — both draw a one-page figure PDF through `pymupdf.Page.show_pdf_page`, but the vector arms emit a flat `<svg>` and the `Annotate` arm a PNG, so neither is the PDF those consumers read. `vl_convert.svg_to_pdf(svg, scale=None) -> bytes` ("wrap any SVG string in a vector PDF") is a VERIFIED REAL member of the folder `.api` catalogue for `vl-convert-python`, the Rust/Deno-embedded engine wrapping a finished SVG string through its bundled `resvg` core with no browser or Node runtime, on the cp315 core; the catalogue names this the documented post-edit-SVG-then-PDF path — `vegalite_to_svg -> svg_to_pdf` "the alternate SVG-first path when the owner post-edits the SVG (scale-to-fit, crop via `svgelements`) before rasterizing" — which is exactly figure composition's role (it post-edits via the `ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay` arms then projects). The `Pdf` case carries the already-placed flat `<svg>` plus the `svg_to_pdf` `scale` row and the arm is a pure projection (`vl_convert.svg_to_pdf(source.decode(), scale=scale)`), re-composing nothing; it imports `vl_convert` at boundary scope on the core. This is distinct from `visualization/chart#EXPORT`'s ownership of the chart-spec render rows (`vegalite_to_pdf`/`vega_to_pdf`): composition wraps an already-placed figure document and renders no chart spec, so the two compose the same package for disjoint concerns exactly as `svgelements` is shared across the vector/compose/layered owners. The arm mints `ArtifactReceipt.Pdf(key, len(data), 1)` (the one-page PDF byte/page evidence under the `pdf` kind) rather than the `preview` kind's `Preview` mint, through the receipt owner's named `Pdf` `@classmethod` taking the byte/page scalars positionally — never the phantom `ArtifactReceipt.of(key, PdfFacts(...))` evidence-struct form the receipt owner deleted. A second SVG-to-PDF sink (`reportlab`/`weasyprint`), a chart re-render here, and a placement re-composition inside the `Pdf` arm are the rejected forms.
- [FAULT_NARROWING] [RESOLVED]: `Figure.of` calls `async_boundary(f"figure.{self.op.tag}", self._emit, catch=_FAULTS)` where `_FAULTS = (ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)` is the real raise tuple the engine surface throws — `resvg_py.svg_to_bytes`/`vl_convert.svg_to_pdf`/`svgelements` parse raise `ValueError` on an empty or invalid SVG and on render failure (the imported `bounds` `min()`-over-empty on a no-bbox document and `Angle.parse`/`Length` on bad input raise `ValueError` too), the `.svgz` file source raises `OSError`, and the gated `anyio.to_process.run_sync` seam raises `BrokenWorkerProcess` on worker death, and the `_GUARD`-contracted `_fit`/`_rows` division seams raise `BeartypeCallHintViolation` on a degenerate zero-extent source bbox or a non-positive n-up column count — the surfaces-and-dispatch contract aspect the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `_GUARD` seam carries one-for-one, refining the divisor scalar (`Extent`/`Columns`) at the one site the placement math admits an external scalar so the violation rails rather than a `ZeroDivisionError` escaping the placement fold as an unconverted defect. `BrokenWorkerInterpreter` is NOT in the tuple: that is the `to_interpreter` worker-death raise the concurrency owner names, and this owner offloads `pillow` only through `to_process` (a GIL-hostile native call the subinterpreter lane cannot safely host), so the interpreter-death case is unreachable and a tuple member for it would be a phantom fault. The runtime `faults#FAULTS` owner's `async_boundary[T](subject, thunk, *, catch=...)` signature takes exactly this `type[BaseException] | tuple[...]` so the boundary "passes its real raise tuple rather than collapsing to the `Exception` catch-all" — its `_convert` folds each caught cause through `BoundaryFault.of(subject, cause)`, the `CLASSIFY` table mapping the tuple onto the closed `BoundaryFault` vocabulary (`ValueError` to `boundary`, the `OSError`/broken-worker set to `resource`) — that per-row classification the faults owner's concern, never re-spelled here, the narrowing's own contribution being that a non-engine raise propagates as a defect rather than railing through `boundary`. Cancellation is excluded from `_FAULTS` so the structured-cancellation signal re-raises past the boundary unconverted, as the concurrency owner requires; the broad `catch=Exception` default would rail an unexpected non-engine raise the `rails-and-effects#EXCEPTION_CAPTURE` law keeps as a defect, so the narrowed tuple — not the default — is the form that law admits, and the page's `RuntimeRail` claim is only real once the boundary names the tuple.
- [RASTER_SETTLED]: the gated `pillow` pass for `ANNOTATE`/`METADATA` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `PIL` at boundary scope inside the gated-band worker function, never on the cp315-core owner; the `ANNOTATE` worker receives the resvg-rasterized PNG and decodes it through `Image.open`, the `METADATA` worker the already-emitted figure raster. The `FilterKind` finish vocabulary the `Frame` case carries is the figure-finishing tone/sharpen axis the concept needs, every member a catalogued `pillow` row: `unsharp`/`blur` are `ImageFilter.UnsharpMask`/`GaussianBlur` and `median` is the `ImageFilter` `Median`-family `MedianFilter(size=)` (the odd-size rank filter that removes raster speckle a sharpen would amplify); `sharpness`/`contrast`/`brightness`/`saturation` are the `ImageEnhance.Sharpness`/`Contrast`/`Brightness`/`Color` enhancement factories the catalogue names ("also Color/Brightness/Sharpness"), so the full four-axis tone band lands and a figure needing a saturation lift is one token, not a re-route; `autocontrast`/`equalize` are the `ImageOps.autocontrast(cutoff=)`/`equalize` tone operations the catalogue rows — the histogram-stretch and histogram-equalize finishes a figure panel needs that a bare `contrast` enhance cannot supply, applied on the `RGB`-converted image because both operate on luminance channels. Each is one `Literal` token plus one `match` arm so a new finish (a 3D-LUT grade through `ImageFilter.Color3DLUT`, an `ImageOps.colorize` recolor) is one row, never a parallel filter loop; ICC color management stays `graphic/color/managed#MANAGED`'s. The `Image.open`/`Image.getexif`/`Image.Exif`/`Image.save`/`Image.Resampling`/`Image.paste`/`Image.getchannel`, `ImageDraw.Draw`/`text`/`rectangle`/`line`/`ellipse`/`polygon`, `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`fit`/`autocontrast`/`equalize`, `ImageFilter.UnsharpMask`/`GaussianBlur`/`MedianFilter`, and `ImageEnhance.Sharpness`/`Contrast`/`Brightness`/`Color` spellings verify against the folder `.api` catalogue for `pillow` (`12.2.0` reflected on the gated cp313 band): `ImageDraw.ImageDraw.text`/`rectangle`/`line`/`ellipse`/`polygon` are the catalogued draw rows (the catalogue's `ImageDraw` "(also rectangle/ellipse/line/polygon/arc/pieslice)" family — `line` the leader/connector polyline, `ellipse` the highlight ring, and `polygon` the filled legend swatch / leader arrowhead a figure annotation needs), the `Image.paste`/`Image.getchannel` pair the catalogued band/composite rows the `Stamp` arm alpha-composites a watermark/logo/seal through (the `getchannel("A")`/`point` opacity mask scaling the alpha before the masked `paste`), `ImageFont.truetype`/`load_default` the font rows, `ImageOps.exif_transpose`/`fit`/`autocontrast`/`equalize` the catalogued operation rows, `ImageFilter.GaussianBlur`/`UnsharpMask`/`Median` the filter factory rows, `ImageEnhance.Contrast`/`Sharpness`/`Brightness`/`Color` the catalogued enhancement rows (`Color` the saturation axis), `Image.Resampling.LANCZOS` the catalogued resample enum case, and `Image.Exif`/`Image.getexif` the mutable EXIF mapping over which the `METADATA` arm folds the tag pairs through one `exif.update(...)`. `ImageOps.expand` is the canonical `pillow` border-matte op the `Frame` finish composes (a uniform passe-partout border, never a hand-rolled `Image.new`+`paste` pass); it is the one cited member the folder `.api` catalogue does not yet enumerate beside `pad`/`fit`/`exif_transpose`, an additive catalogue gap rather than a phantom. The XMP packet rides `image.info["XML:com.adobe.xmp"]` and the `save(..., xmp=...)` kwarg. The figure rail uses no `ImageCms` transform spelling, so this page carries no pillow RESEARCH gate.
- [LAYERED_EGRESS] [RESOLVED]: the placed multi-source layout the `_compose_vector` placement arms emit is one flat single-`<svg>` document built by the imported `graphic/vector#VECTOR` `svg` egress that wraps every `Path.d()` body in a fresh viewBox-sized `<svg>` — that flat-SVG egress is figure composition's settled concern and adds no hierarchical-group or named-layer structure. The editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s owner: the `Figure.layers(names)` projection exposes the placed layout as `tuple[Layer, ...]`, ONE `export/layered#LAYERED` `Layer(name, source, bbox, visible=True, locked=False)` row per placed source carrying its placed `bbox` — the `Tile` arm yields one row per grid cell carrying the cell's `(column*cw, row*ch, (column+1)*cw, (row+1)*ch)` placed extent through `_placed_layers`/`_place`, the `Overlay` arm yields a `base` row plus a registration-`overlay` row over the same document bounds, and the single-source `ScaleFit`/`Crop`/`Rotate` arms yield one row over the placed document bounds — so the base-graphic/registration-overlay/n-up-sheet layout hands outward as named rows keyed by the same `ContentKey`, which `export/layered#LAYERED` binds each into a named `drawsvg` `Group` (the catalogued `Drawing`/`Group`/`as_svg` named-group SVG-authoring surface, folder `.api/drawsvg.md`) and a PDF OCG optional-content layer rather than the one flattened path soup the flat `svg` emits. The split is the disciplined collapse boundary: a layered-export arm grafted onto `FigureOp`, a hierarchical named-`<g>` group emitter beside the one flat document, and a per-layer SVG/PDF writer on this owner are the rejected forms — `Figure.layers` projects the placed-source-plus-`bbox` rows without authoring layer structure, the named-layer authoring is one outward seam to the `export/layered` sub-domain, so figure composition stays the flat post-render placement owner emitting the flat document and the `tuple[Layer, ...]` projection, and the editable-export concern lands once in its own owner consumed by every visual producer. The `_placed_layers` projection reads the placed bounds through the same imported `bounds(source)` the placement arms use, so the layer rows derive from one geometry surface. The `drawsvg` `Group(id=...)` named-group and pikepdf OCG member spellings are the `export/layered` page's verification burden, not this page's fence; this row carries the seam direction, the `Figure.layers` projection shape, and the flat-vs-named boundary.
- [HANDOFF_GUARD] [BLOCKED]: the outward figure edge to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a private per-artifact handoff, and the figure source re-mints no canonical concept so the `runtime/evidence` `Structural.drift` query stays clean. `Figure._emit` returns `ContentIdentity.of(...)` (re-minting no content-identity seed) and `Figure.contribute` projects the existing `preview`/`pdf` kinds through the `_placed` `Placed` carrier's `Option[ArtifactReceipt]` receipt and the receipt owner's no-phase `ArtifactReceipt.contribute()` generator (re-minting no receipt rail and re-running no placement), so the artifacts side is verification-and-alignment only. Close-condition: the upstream `compute/graduation` `HandoffAxis` model-asset case and the `runtime/evidence` `Structural.drift` detector land; the figure rail threads the same `ContentKey` into the one outward handoff with zero new surface.
- [VECTOR_SURFACE_PUBLICIZE] [OPEN]: figure composition imports `bounds`/`elements`/`transform`/`path`/`svg`/`px` (byte-sourced composition functions) and the `Element`/`RenderPolicy`/`Bounds`/`Length`/`Style` owners from `graphic/vector#VECTOR`, but that owner currently declares them as underscore-private (`_bounds`/`_path`/`_transform`/`_svg`/`_px`, the `_elements` parse-and-bbox-filter, the `Element` `Protocol`, the `RenderPolicy` struct) and exposes geometry only through the async `Vector.of`/`VectorOp` dispatch — its `_bounds`/`_transform`/`_path`/`_elements` take a parsed `SVG`, not bytes, and `VectorOp.Bounds` returns comma-joined bytes. Resolving this import requires the vector owner to publicize the byte-sourced composition surface as module-public functions — including `elements(source: bytes) -> list[Element]`, the `SVG.parse(reify=True)`-plus-`hasattr(element, "bbox")` filter the crop pre-filter reads so figure composition re-parses no document (the prior `_hit_elements` re-declared that parse verbatim, contradicting the import-not-re-declare thesis) — and to PARAMETERIZE `RenderPolicy.kwargs` over a source-keyword dict (the figure `RasterSource.keywords()` markup-or-file projection) rather than a hardcoded `{"svg_string": document.decode()}`, so the same `RenderPolicy` ingress sources from both the vector owner's document bytes and this owner's `RasterSource` case. Until vector.md realizes the public surface, this import is the signature target; the placement-math arm bodies (`_compose_vector`/`_fit`/`_rows`/`_place`/`_anchor`/`_hit_elements`/`_clipped`) stay this owner's and compose the imported surface in one hop.
