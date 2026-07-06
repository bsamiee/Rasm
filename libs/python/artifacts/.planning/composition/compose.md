# [PY_ARTIFACTS_COMPOSE]

The post-render figure/section placement owner turning emitted graphics into placed, annotated, color-correct figures. `Figure` is ONE owner over the post-render composition pipeline carrying a closed-payload `FigureOp` `expression.tagged_union` — each operation a case carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`. It reads the SVG that the regrouped `visualization/chart#CHART`, `graphic/marks#MARK`, and `visualization/table#TABLE` sibling owners already emit and lays it out — scale-to-fit a target viewport, tile an n-up sheet, crop to a bounds box, rotate-place at an `Angle`, overlay registration marks and crop guides — by IMPORTING the `graphic/vector/path#PATH` query/affine surface (`bounds`/`elements`/`fragment`/`px` plus the `Bounds`/`Span` value objects and the `PathFault` rail) and the `graphic/vector/region#REGION` set-op/serialization surface (`clip`/`boolean`/`outline`, the `document` assembly, the `RenderPolicy` rasterize-policy owner, and the `RegionFault` rail) and composing it one hop, never re-declaring the `svgelements` `Matrix`/`Path`/`Color`/`Length`/`Angle`/`Style` algebra or the `document`/`fragment` egress pair this owner does not own; the rotate-place angle resolves through the local `_angle` over `svgelements.Angle.parse`, the same placement-math use of the geometry algebra the `Matrix` factories ride, never an imported vector function. It holds ONLY the placement-specific arm bodies the geometry primitive deliberately does not own — the scale-fit factor, the n-up cell placement, the crop delegation to the imported `clip`, the rotate pivot, the registration-overlay fold, and the gated `pillow` finish — folding the imported `elements(source)` shapes through the imported `fragment(shape, matrix)` d-string egress onto the imported `document(fragments, viewbox)` assembly. The imported `bounds(source)` is `PathFault`-railed; figure composition threads that rail and lifts it to the boundary raise at the single `_extent` seam, so the foreign `PathFault`/`RegionFault` domains cross into the `BoundaryFault` domain this owner rails in exactly once, uniform with the pillow/`vl_convert` engine raises.

It rasterizes its own placed SVG through the `graphic/vector/region#REGION` `RenderPolicy`-driven `resvg_py.svg_to_bytes` raster floor ON THE WORKER, then draws captions/legends/borders, fits or EXIF-orients the raster, applies a tone/sharpen/grade/blend finish chain, and binds EXIF/XMP metadata over `pillow` on that same worker band, so no native render touches the event loop. One figure surface discriminating the operation, not a per-graphic-type composer family. The vector geometry arms are synchronous pure-Python `svgelements` work — `svgelements` imports on the core, so the arm bodies compose in-process, but the CONCERN offloads through a `to_thread.run_sync` band per the concurrency `OFFLOAD_LANE` law so no placement fold runs on the event loop; the raster annotate/metadata arms offload the WHOLE pass — the `resvg_py` rasterize AND the `pillow` draw/filter/metadata fold — to a `to_process` worker as a GIL-hostile native call (concurrency chooser `[11]`), the module-scope `lazy import resvg_py`/`lazy from PIL` proxies reifying inside the worker so the runtime module stays import-clean while the native arms run only on the worker band the package ships for. The `to_process` annotate/metadata band rides one `stamina.AsyncRetryingCaller(...).on(BrokenWorkerProcess)` retry so a transient OOM/signal worker death recovers before the `_FAULTS` boundary rails an exhausted death as a defect — the same worker-seam retry the `graphic/raster/io#IO` arm that shares the lane carries.

Figure composition rasterizes only its own placed SVG for the annotate pass through the `graphic/vector/region#REGION` `resvg_py.svg_to_bytes` floor — chart/mark/nanoplot rasterization-to-export stays in `visualization/chart#EXPORT` `vl-convert` — and re-renders no chart; it places, rasterizes, and finishes already-emitted graphics. The placed multi-source layout the placement arms emit is one flat single-`<svg>` document — that flat-SVG egress is this owner's concern; the editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s concern, receiving the same placed layout through `Figure.layers -> tuple[Layer, ...]` and binding each placed source as a named editable layer rather than one flattened path soup, so figure composition emits the flat artifact and routes the named-layer authoring outward. The one-page vector-PDF egress the `composition/sheet#SHEET` and `composition/imposition#IMPOSE` consumers draw through `pymupdf.show_pdf_page` is the `Pdf` case's `vl_convert.svg_to_pdf` wrap of that placed flat document — the documented post-edit-SVG-then-PDF path, never a chart re-render and never a second SVG-to-PDF sink.

Every operation returns a `RuntimeRail[ContentKey]` over the runtime `async_boundary` narrowed to the real engine raise tuple `_FAULTS` (so a non-engine raise and cancellation propagate as defects rather than being railed, the `BoundaryFault` classification of each caught raise the runtime `faults#FAULTS` owner's `CLASSIFY` concern) and contributes through the existing `core/receipt#RECEIPT` named per-kind mints the producer calls positionally — `ArtifactReceipt.Preview(key, width, height)` placement evidence (empty perceptual band) or `ArtifactReceipt.Pdf(key, bytes, pages)` projection evidence — by threading the bytes `_emit` already computed, never a second re-composition or re-render and never the phantom `ArtifactReceipt.of(key, PreviewFacts(...))`/`PdfFacts` evidence-struct indirection the receipt owner deleted.

## [01]-[INDEX]

- [01]-[COMPOSE]: the post-render placement owner over the closed-payload `FigureOp` `tagged_union` (`ScaleFit`/`Tile`/`Crop`/`Merge`/`Matte`/`Rotate`/`Overlay` the in-process vector-placement arms, `Pdf` the SVG-to-PDF projection, `Annotate`/`Metadata` the gated raster arms), rail-typed `RuntimeRail[ContentKey]` over `async_boundary(catch=_FAULTS)`, composing the imported `graphic/vector/path#PATH` `bounds`/`elements`/`fragment`/`px` and `graphic/vector/region#REGION` `document`/`clip`/`boolean`/`outline` surfaces one hop; the `DrawOp` closed pillow-annotation family (`Text`/`Caption`/`Box`/`RoundBox`/`Line`/`Ellipse`/`Arc`/`Polygon`/`Regular`/`Stamp`/`Blend`/`Grade`/`Frame`) with the reusable `TextStyle` measured-text owner, the `MarkKind` registration-overlay vocabulary, the `RasterSource` markup/file source case, and the `FilterKind`/`ArcKind`/`BlendKind` finish/primitive vocabularies; the `Placed` in-process evidence carrier the `of`/`contribute` split reads once; the ICC-tagged PNG egress (`Annotate` `icc` embed) beside the outward `graphic/color/managed#MANAGED` full-transform seam and the `exchange/metadata#METADATA` authoritative descriptive-metadata seam; projects `Figure.layers` to `export/layered#LAYERED` `Layer`; threads `core/receipt#RECEIPT` through the receipt owner's named `ArtifactReceipt.Preview`/`Pdf` mints; every figure routes through the single `core/plan#PLAN` `ArtifactPipeline` production entry as a producer node.

## [02]-[COMPOSE]

- Owner: `Figure` the one figure-composition owner discriminating operation over the closed `FigureOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `RenderPolicy` is NOT this owner's — it is `graphic/vector/region#REGION`'s rasterize-policy owner (the resvg sizing/parsing/font/policy/diagnostic axis projected to `svg_to_bytes` through one `asdict`-driven spread), IMPORTED here and carried into the `Annotate` case so the rasterize policy has one edit site across the chart/diagram/figure consumers; `TextStyle` the reusable measured-text styling owner (font/size/fill/anchor/align/spacing/stroke/direction/language/features/variation) both the `Text` and `Caption` arms compose so a stroked complex-script variable-font caption and a bare label share one styling shape, never a per-arm text-param wall; `DrawOp` the closed-payload `tagged_union` collapsing the rejected `CaptionSpec`/`BoxSpec`/`PostSpec` draw triple into one pillow draw family — a `Text` case (`ImageDraw.multiline_text` measured/multi-line/stroked/`Layout.RAQM`-shaped/variable-axis), a `Caption` case (`multiline_textbbox`-measured `rounded_rectangle` backing sized to its own text), a `Box` case (`ImageDraw.rectangle`), a `RoundBox` case (`rounded_rectangle`), a `Line` case (`ImageDraw.line` leader/connector polyline), an `Ellipse` case (`ImageDraw.ellipse` highlight ring), an `Arc` case (`arc`/`chord`/`pieslice` keyed by `ArcKind`), a `Polygon` case (`ImageDraw.polygon` filled legend swatch / leader arrowhead), a `Regular` case (`regular_polygon` n-gon marker), a `Stamp` case (`Image.alpha_composite` premultiplied over-composite of an opacity-scaled watermark/logo/seal), a `Blend` case (`ImageChops` full-frame Porter-Duff blend composite keyed by `BlendKind`), a `Grade` case (`ImageFilter.Color3DLUT` trilinear 3D-LUT grade), and a `Frame` case carrying the border/fit/exif/`FilterKind` finish axis — folded by one total `match` over the worker so a new annotation primitive is one case, never a parallel spec struct; `RasterSource` the closed-payload `tagged_union` collapsing `svg_string` markup and an `svg_path` `.svg`/`.svgz` file into one source case whose `keywords()` projects the live `svg_to_bytes` source keyword so a file source never grows a second render call; `MarkKind` the closed `Enum` of registration-overlay primitives (`CORNER` crop guide, `TICK` cut mark, `TARGET` color-bar dot, `REGISTRATION` concentric press target, `GUTTER` fold diamond, `MITER` corner chevron, `BLEED` trim box, `CROSS` four-spoke registration cross-hair, `STAR` eight-spoke slur/density target) whose every member carries its own verified-catalogued-primitive name plus float-only arg row and resolves one `svgelements` shape through its bound `MarkKind.shape` builder so the overlay fold dispatches total over the member, never a `dict[MarkKind, Callable]` rebuilt per call against an erased shape bag; `FilterKind`/`ArcKind`/`BlendKind` the closed raster-finish vocabularies carried on the `Frame`/`Arc`/`Blend` cases. The `graphic/vector/path#PATH` owner — not this one — owns the `svgelements` `SVG` document working surface, the `Matrix`/`Path`/`Color`/`Length`/`Angle`/`Point`/`bbox` algebra, the `bounds`/`elements`/`fragment`/`px` composition functions, the `Element` `Protocol`, and the `PathFault` rail, while `graphic/vector/region#REGION` owns the `document` assembly, the `RenderPolicy` row, the `RegionFault` rail, and the `resvg_py.svg_to_bytes` raster floor; figure composition IMPORTS that public surface and composes it one hop for every vector operation, re-declaring none of it, while the `pillow` `Image` is the raster annotate/metadata surface this owner holds on the gated floor.
- Cases: `FigureOp` cases — `ScaleFit(source, width, height)` (resolve the source viewport through the imported `bounds`, derive the `Matrix.scale` that fits a target `Span` box preserving aspect, re-emit each imported `elements(source)` shape through `fragment(shape, matrix)` framed to the target viewbox) · `Tile(sources, columns, cell, gutter)` (n-up sheet — aspect-fit each source SVG centered into a gutter-spaced row-major grid cell, each placement a `Matrix.translate`-after-`Matrix.scale` of the source bounds into the cell, the gutter the inter-cell margin a press sheet needs and the centering the offset a top-left cram omits) · `Crop(source, x, y, width, height)` (the factory folds the four floats into one `Bounds` payload; the imported `graphic/vector/region#REGION` `clip(source, rect)` intersects each drawable shape against the crop rect through `skia-pathops` `PathOp.INTERSECTION`, keeping only the non-empty survivors as separately-framed `<path>` fragments cut to the crop rect — a real geometry sever, not a CSS `<clipPath>` mask a downstream must honor) · `Merge(sources, op)` (the N-source planar set-op — the imported `graphic/vector/region#REGION` `boolean(sources, op)` folds each source outline through the `skia-pathops` `OpBuilder` UNION/DIFFERENCE/INTERSECTION/XOR, merging tile silhouettes on UNION, knocking out on DIFFERENCE, masking an overlap on INTERSECTION, one composed set-op the placement plane needs for a bleed-and-trim silhouette, mirroring the `Crop` arm's one-hop `clip` compose) · `Matte(source, width, stroke)` (the fixed-width offset keyline/matte — the imported `graphic/vector/region#REGION` `outline(source, width)` strokes the artwork silhouette into a closed filled boundary through `skia-pathops` `Path.stroke`, the offset algebra `svgelements` cannot express, the matte `stroke` row lowering at the `document` paint axis and framed under the source for a margin-frame matte or a trim/bleed keyline) · `Rotate(source, angle, corner)` (rotate-place the source by an `Angle`-resolved `Matrix.rotate` about a `bounds` corner pivot) · `Overlay(source, marks)` (registration marks, gutters, and crop guides — fold a `MarkSpec` row list, each `MarkSpec.kind` member building one `svgelements` shape through its own bound `MarkKind.shape` builder positioned by document-corner offsets, every fragment serialized through the imported `fragment` d-string egress (each spec's `(stroke, width)` row lowering at the `document` paint axis), appended to the imported `elements(source)` base fragments) · `Pdf(source, scale)` (lift the placed flat `<svg>` into a one-page vector PDF through `vl_convert.svg_to_pdf(source, scale=)` — the post-edit-SVG-then-PDF path the `composition/sheet#SHEET`/`imposition#IMPOSE` consumers draw through `pymupdf.show_pdf_page`; a pure SVG-to-PDF projection, never a re-composition) · `Annotate(source, render, draws)` (rasterize-then-draw — the worker lane runs the `graphic/vector/region#REGION` `resvg_py.svg_to_bytes` floor over the `RasterSource` case under the one `RenderPolicy` to rasterize, then folds the `DrawOp` sequence by one total `match` — the `Text` arm runs `multiline_text` over the `_face`-resolved `TextStyle`, the `Caption` arm measures via `multiline_textbbox` and draws a `rounded_rectangle` backing then the text, the `Box`/`RoundBox`/`Line`/`Ellipse`/`Arc`/`Polygon`/`Regular` arms the matching `ImageDraw` primitive, the `Stamp` arm `Image.alpha_composite`, the `Blend` arm the `ImageChops`-mode full-frame composite, the `Grade` arm `ImageFilter.Color3DLUT`, the `Frame` arm the `ImageOps.exif_transpose`/`expand`/`fit` frame pass and the `FilterKind` `ImageFilter`/`ImageEnhance`/`ImageOps` finish, re-binding the draw surface after each image-replacing op — the trailing `icc` embedding the working-space ICC profile on the PNG egress through `Image.save(icc_profile=)` so the placed figure egresses color-tagged for the pub/print AND ISO drawing-sheet plane, the FULL source->dest ICC transform / lcms2 soft-proof / CxF3 CMYK separations riding `graphic/color/managed#MANAGED` outward, never re-owned here) · `Metadata(source, exif, xmp)` (the figure-egress-LOCAL descriptive-metadata convenience — an in-worker `Image.getexif`/`Image.Exif` tag map plus the `info["XML:com.adobe.xmp"]` XMP packet re-encoded on save, a bare figure carrying its title/creator/rights without a second async hop; NOT the authoritative cross-format seal — `exchange/metadata#METADATA` is the categorical-best owner over EXIF+IPTC+XMP+ICC+GPS+maker-notes in one `pyexiftool` cross-format pass, composed outward when a full descriptive-metadata write is the deliverable, this the deliberate in-worker PNG-tag boundary) — matched by one total `match`/`case`; never a sibling op per source media type, never a parallel mark emitter per primitive, and never a parallel figure class.
- Auto: `_emit` folds the op — the vector arms (`ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay`) cross one `to_thread.run_sync(_compose_vector, self.op, limiter=_GATE)` band OFF the loop (concurrency `OFFLOAD_LANE`; the pure-Python `svgelements` fold never runs on the event loop even though `svgelements` imports on the core), `_compose_vector` parsing each source through the imported `elements` (the placement extent through the imported `bounds` at the `_extent` seam, the `Crop` arm delegating the geometry cut to the imported `clip`), composing the placement `Matrix` from `Matrix.translate`/`scale`/`rotate` or folding the `MarkSpec` rows whose `kind` member builds its own bound `MarkKind.shape` `svgelements` shape, and routing every fragment through the imported `fragment` d-string egress onto the imported `document(fragments, viewbox)` assembly (the `Crop` arm instead delegating to the imported `clip` geometric crop); the `Annotate` arm hands the cheap `render.kwargs(source.keywords())` dict — the IMPORTED `RenderPolicy.kwargs` merge of the `RasterSource.keywords()` source key with the `asdict`-projected sizing/parsing/font/policy/diagnostic axis — to the `_TRANSIENT.on(BrokenWorkerProcess)`-retried `to_process` worker, where `resvg_py.svg_to_bytes(**render_kwargs)` rasterizes OFF the loop and then `Image.open`, `ImageDraw.Draw`, and the `DrawOp` fold's `multiline_text`/`multiline_textbbox`/`rounded_rectangle`/`rectangle`/`line`/`ellipse`/`arc`/`chord`/`pieslice`/`polygon`/`regular_polygon`, the `Stamp` arm's `Image.alpha_composite`/`getchannel`/`point`/`putalpha`, the `Blend` arm's `ImageChops` mode, the `Grade` arm's `ImageFilter.Color3DLUT`, `ImageFont.truetype(layout_engine=Layout.RAQM)`/`load_default`/`set_variation_by_axes`, `ImageOps.exif_transpose`/`expand`/`fit`/`autocontrast`/`equalize`/`posterize`/`solarize`, `ImageFilter.UnsharpMask`/`GaussianBlur`/`MedianFilter`, and `ImageEnhance.Sharpness`/`Contrast`/`Brightness`/`Color` resolve at boundary scope; the `Pdf` arm lifts the already-placed flat `<svg>` to a one-page vector PDF OFF the loop through `to_thread.run_sync(vl_convert.svg_to_pdf, source.decode(), scale, limiter=_GATE)` — the GIL-releasing native render bounded by the same offload slot, a pure projection that re-composes nothing; the `Metadata` arm runs the `Image.open`/`Image.getexif`/`Image.Exif`/XMP map on the retried worker, folding the EXIF tag pairs through one `exif.update(...)`. The placement helpers this owner holds — `_compose_vector` (the scale-fit/tile/crop/rotate/overlay arm bodies), `_fit`/`_rows` (the `_GUARD`-contracted aspect-fit and grid-count division seams refining the divisor scalar), `_place` (n-up cell math), `_marks` (the registration-overlay mark fold both the flat and per-layer egresses compose), `_anchor` (corner placement), `_extent`/`_source_fault` (the ONE `bounds`/`clip`-rail-to-`ValueError` boundary seam), and `_face` (the `TextStyle`-to-`FreeTypeFont` resolver gating `Layout.RAQM` on `features.check("raqm")`) — compose the imported geometry surface rather than re-implementing it; the geometry primitives themselves live once in `graphic/vector/path#PATH` and `graphic/vector/region#REGION`. The in-process arms derive the content key (`_emit`) and the `ArtifactReceipt.Preview`/`Pdf` evidence (`_placed`) from the SAME deterministic `_compose_vector`/`svg_to_pdf` render of the one source within each projection, so the key and the facts cannot diverge within a call — never a SECOND re-emit reading width/height/byte-count beside the key's. `_emit` and `_placed` re-render per projection (the frozen owner carries no `@cache` memo, the recompute matching the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `Composed` re-entry); the gated annotate/metadata arms' receipt rides the `of` emission outward because their PNG mints only inside the subprocess `_emit`, so `_placed` returns `Nothing` for them and `contribute` yields no row — never a placeholder `Placed` over empty bytes.
- Receipt: each in-process placement operation contributes through the `core/receipt#RECEIPT` owner's named `ArtifactReceipt.Preview(key, width, height)` `@classmethod` mint carrying the placed figure's pixel/viewport width and height under the `preview` kind (the optional `scores` perceptual band defaulting empty — placement carries no perceptual measurement, that band is `graphic/raster/measure#MEASURE`'s), while the `Pdf` projection mints `ArtifactReceipt.Pdf(key, bytes, 1)` under the `pdf` kind — figure composition reuses the existing `preview`/`pdf` kinds through the owner's flat-scalar named mints and adds NO new kind and NO new evidence `Struct`; the phantom `ArtifactReceipt.of(key, PreviewFacts(...))`/`PdfFacts` evidence-struct indirection is the rejected form the receipt owner explicitly deleted (its named per-kind `@classmethod` mints take the scalars positionally, never an `of(key, facts)` re-wrap, the same form the `sheet`/`imposition` siblings already mint through). `_placed(op)` folds the bytes and the placed viewport (read off the imported `bounds` at the `_extent` seam, or the PDF byte count) into ONE `Placed` carrier per call, so the content key and the receipt read the SAME bytes within that fold. `Figure.contribute()` is the `ReceiptContributor` projection over `_placed` threading the receipt's own `ArtifactReceipt.contribute()` generator — no `phase` parameter, because an `ArtifactReceipt` is by construction emitted-artifact evidence so the phase is the constant `"emitted"` the receipt owner fixes and the KNOB_TEST deletes; the `admitted`/`planned` lifecycle facts are NOT figure cases — the `core/plan#PLAN` planner mints them through its own direct `Receipt.of("artifacts", ("planned", ...))`, so this projection carries only the in-process `ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay`/`Pdf` emitted evidence; the gated `Annotate`/`Metadata` raster facts are unreproducible synchronously (the subprocess `to_process` pass mints the PNG only inside the async `_emit`), so `_placed` returns `Nothing` for them and `contribute` yields no row — their receipt rides the async `Figure.of` emission outward, the one outward async-receipt seam this owner does not close in-process, never a placeholder key over empty bytes.
- Growth: the bleed-and-trim sheet and margin-frame matte the growth axis named are now REALIZED — `Merge` composes the imported `graphic/vector/region#REGION` `boolean(sources, op)` planar set-op and `Matte` the imported `outline(source, width)` stroke-to-outline offset, one `FigureOp` case plus one thin `_compose_vector` arm each over the imported surface (never a re-implemented `skia-pathops` call, mirroring the `Crop` arm's `clip` compose), so a new planar op is one `BooleanOp` member the `Merge` arm already reads and a new offset cap/join one `graphic/vector/region#REGION` `CapStyle`/`JoinStyle` reaching the `outline` compose with zero edit here; a further vector layout operation is one more `FigureOp` case plus one `_compose_vector` arm over the imported `bounds`/`elements`/`fragment`/`document`/`clip`/`boolean`/`outline` surface, while an n-up refinement (inter-cell gutter, cell alignment) is one field on the existing `Tile` case the `_place` math already reads; a new registration-overlay primitive (a collation lozenge, a page-information ladder mark) is one `MarkKind` member carrying its own catalogued-primitive name and float-only arg row resolved through the shared `MarkKind.shape` builder, dispatched by the same total overlay fold and serialized through the imported `fragment` d-string egress — never a parallel mark emitter, never a `dict` arm grafted onto an erased shape bag; a new annotation source mode (a placed working document, a `.svgz` archive) is one `RasterSource` case projecting one entry into the `svg_to_bytes` source keyword on the one render call — never a second render entrypoint; a new raster annotation primitive is one `DrawOp` case plus one `match` arm on the worker, a new finish filter (an `ImageOps.colorize` recolor, an `ImageMorph` pass) is one `FilterKind` token plus one `match` arm on the `Frame` finish fold, a new blend mode one `BlendKind` token, a new text axis one `TextStyle` field the `_face`/`multiline_text` call already reads — never a parallel spec struct, never a per-primitive draw loop; a true geometric crop is the imported `graphic/vector/region#REGION` `clip(source, rect)` (a `skia-pathops` `PathOp.INTERSECTION` per-shape cut) composed one hop into the `Crop` arm with zero new engine here; a new resvg sizing/font/policy/diagnostic knob grows the IMPORTED `graphic/vector/region#REGION` `RenderPolicy` row at its owner, reaching this owner with zero edit — never a second rasterizer here; a new PDF-projection knob (a fit-to-page scale, a target page box) is one field on the existing `Pdf` case carried into the one `vl_convert.svg_to_pdf` call — never a second SVG-to-PDF sink; a color-tagged egress is the `Annotate` `icc` field embedded through `Image.save(icc_profile=)`, and a full source->dest ICC transform / soft-proof / CMYK separations is one outward compose of `graphic/color/managed#MANAGED` on the emitted raster — never a second color engine here; the authoritative cross-format descriptive-metadata write (a full IPTC/XMP/provenance seal) is one outward compose of `exchange/metadata#METADATA` on the emitted figure, the in-worker `Metadata` arm carrying only the figure-egress-local PNG EXIF/XMP tag convenience. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Sequence
from enum import Enum
from io import BytesIO
from typing import TYPE_CHECKING, Annotated, Literal, NoReturn, assert_never

from anyio import BrokenWorkerProcess, CapacityLimiter, to_process, to_thread
from beartype import BeartypeConf, beartype
from beartype.roar import BeartypeCallHintViolation
from beartype.vale import Is
from expression import Nothing, Option, Some, case, tag, tagged_union
from msgspec import Struct

import stamina

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

# graphic/vector/path#PATH owns the SVG parse/query/affine substrate and graphic/vector/region#REGION the
# set-op/serialization surface; figure composition IMPORTS both and re-declares none — `bounds`/`elements`/
# `fragment`/`px` resolve geometry in one hop, `elements(source)` is the parse-and-drawable-shape sweep the
# placement folds read (the page re-parses no document), `clip(source, rect)` the `skia-pathops`
# `PathOp.INTERSECTION` per-shape geometric crop the `Crop` arm composes, `boolean(sources, op)` the N-ary
# UNION/DIFFERENCE/INTERSECTION/XOR set-op (the `OpBuilder` planar algebra `svgelements` cannot express) the
# `Merge` arm composes for merged tile silhouettes / knockouts, `outline(source, width)` the `skia-pathops`
# stroke-to-outline / fixed-width offset (`Path.stroke`; no `Path.offset`, the stroke IS the offset) the `Matte`
# arm composes for a margin-frame / bleed-trim keyline, `PathFault` the rail the `_extent` bounds seam lifts,
# `RegionFault` the clip/boolean/outline rail, `document(fragments, viewbox)` the ONE drawsvg assembly, and
# `RenderPolicy` the resvg policy owner carried into `Annotate`. The heavy CMYK/ICC transform, soft-proof, and
# CxF3 separations are `graphic/color/managed#MANAGED`'s, composed outward, never re-owned here.
from artifacts.graphic.vector.path import Bounds, PathFault, Span, bounds, elements, fragment, px
from artifacts.graphic.vector.region import BooleanOp, RegionFault, RenderPolicy, Stroked, boolean, clip, document, outline
from artifacts.core.receipt import ArtifactReceipt
from artifacts.export.layered import Layer

lazy import resvg_py
lazy import svgelements
lazy import vl_convert
lazy from PIL import Image, ImageChops, ImageDraw, ImageEnhance, ImageFilter, ImageFont, ImageOps, features
lazy from svgelements import Angle, Matrix, Point

if TYPE_CHECKING:
    from rasm.runtime.receipts import Receipt


# --- [TYPES] ----------------------------------------------------------------------------
type Corner = Literal["nw", "ne", "sw", "se", "n", "s", "e", "w", "center"]
type Anchor = tuple[float, float]
type Extent = Annotated[float, Is[lambda value: value > 0.0]]
type Columns = Annotated[int, Is[lambda count: count >= 1]]
type MarkArgs = Callable[[float, float, float], tuple[float, ...]]
type FilterKind = Literal[
    "unsharp", "blur", "median", "sharpness", "contrast", "brightness", "saturation", "autocontrast", "equalize", "posterize", "solarize"
]
type ArcKind = Literal["arc", "chord", "pieslice"]
type BlendKind = Literal["multiply", "screen", "overlay", "soft_light", "hard_light", "difference", "add", "subtract", "darker", "lighter"]


class MarkKind(Enum):
    # each member is (svgelements-primitive-name, float-arg builder); the positional constructor
    # arity is verified — Circle(cx, cy, r), Ellipse(cx, cy, rx, ry), Rect(x, y, w, h),
    # SimpleLine(x1, y1, x2, y2), Polyline/Polygon(*points) — so `MarkKind.shape` is settled fence.
    CORNER = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y))
    TICK = ("SimpleLine", lambda x, y, s: (x - s, y, x + s, y))
    TARGET = ("Circle", lambda x, y, s: (x, y, s))
    REGISTRATION = ("Ellipse", lambda x, y, s: (x, y, s, s * 0.5))
    GUTTER = ("Polyline", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s, x - s, y))
    MITER = ("Polygon", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s))
    BLEED = ("Rect", lambda x, y, s: (x - s, y - s, 2.0 * s, 2.0 * s))
    CROSS = ("Polyline", lambda x, y, s: (x - s, y, x, y, x, y - s, x, y, x + s, y, x, y, x, y + s))
    STAR = (
        "Polyline",
        lambda x, y, s: (
            x,
            y - s,
            x,
            y,
            x + s,
            y - s,
            x,
            y,
            x + s,
            y,
            x,
            y,
            x + s,
            y + s,
            x,
            y,
            x,
            y + s,
            x,
            y,
            x - s,
            y + s,
            x,
            y,
            x - s,
            y,
            x,
            y,
            x - s,
            y - s,
        ),
    )

    def __init__(self, primitive: str, args: MarkArgs) -> None:
        self.primitive = primitive
        self.args = args

    def shape(self, anchor: Anchor, size: float) -> object:
        return getattr(svgelements, self.primitive)(*self.args(*anchor, size))


# --- [CONSTANTS] ------------------------------------------------------------------------
# resvg/svgelements/vl-convert raise `ValueError` on invalid SVG, an empty document, or render
# failure; the gated `to_process` worker death raises `BrokenWorkerProcess` (rides `_TRANSIENT` first,
# reaching this tuple only when the retry is exhausted); `OSError` rides the `.svgz` file arm; the
# `_GUARD`-contracted `_fit`/`_rows` division seams and the `_extent` `bounds`-rail lift raise
# `BeartypeCallHintViolation`/`ValueError` on a degenerate source bbox or a non-positive column count.
# The boundary narrows `catch` to this real raise tuple so a non-engine raise propagates as a defect
# rather than railing through the `Exception` catch-all; the runtime `faults#FAULTS` `CLASSIFY` table
# maps each caught raise onto its `BoundaryFault` case.
_FAULTS: tuple[type[Exception], ...] = (ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)

# the native-offload bounded slot every figure render threads: the GIL-hostile `to_process`
# `pillow`/resvg annotate band, the GIL-releasing `to_thread` `vl_convert` PDF band, AND the
# pure-Python `to_thread` vector-placement band share this one `CapacityLimiter` so N concurrent
# figures bound a fixed offload pool instead of fanning out at the per-loop `current_default_*_limiter()`
# defaults the concurrency owner rejects.
_GATE: CapacityLimiter = CapacityLimiter(4)

# the worker-seam resilience the pillow/vector arms that share the lane carry: a transient OOM/signal
# `to_process` worker death recovers through one bounded retry before the `_FAULTS` boundary rails an
# exhausted death as a `BoundaryFault` resource case; the structural worker death alone rides `.on`, never
# the domain `ValueError` a bad SVG raises (that is content, not transport, and surfaces immediately).
_TRANSIENT = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)

# the surfaces-and-dispatch contract aspect: the `_fit`/`_rows` division seams refine their divisor
# scalars (`Extent`/`Columns`), so a degenerate source extent or a non-positive grid count rails as
# `BeartypeCallHintViolation` the `_FAULTS` boundary converts rather than a `ZeroDivisionError` deep in
# the placement fold (a `Bounds`/case field is not deep-checked by beartype — only a direct scalar
# parameter is), matching the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `_GUARD` seam.
_GUARD = beartype(conf=BeartypeConf(violation_type=BeartypeCallHintViolation))


# --- [MODELS] ---------------------------------------------------------------------------
class MarkSpec(Struct, frozen=True):
    kind: MarkKind
    corner: Corner = "nw"
    size: float = 12.0
    inset: float = 6.0
    stroke: str = "black"
    width: float = 0.5


# the reusable measured-text styling owner the `Text` and `Caption` arms both compose, so a journal
# figure caption AND an AEC drawing keynote source one shape — the `_face` resolver reads `font`/`size`/
# `variation` and gates `Layout.RAQM` on `features.check("raqm")`, the draw reads the rest; a stroked
# halo (`stroke_width`/`stroke_fill`) keeps text legible over a busy figure, `direction`/`language`/
# `features` drive complex-script shaping, `variation` drives a variable-font axis vector.
class TextStyle(Struct, frozen=True):
    font: str | None = None
    size: float = 16.0
    fill: str = "black"
    anchor: str | None = None
    align: str = "left"
    spacing: float = 4.0
    stroke_width: int = 0
    stroke_fill: str | None = None
    direction: str | None = None
    language: str | None = None
    features: tuple[str, ...] = ()
    variation: tuple[float, ...] = ()


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
    tag: Literal["text", "caption", "box", "round_box", "line", "ellipse", "arc", "polygon", "regular", "stamp", "blend", "grade", "frame"] = tag()
    text: tuple[str, Anchor, TextStyle] = case()
    caption: tuple[str, Anchor, TextStyle, float, float, str | None, str | None] = case()
    box: tuple[Bounds, str, str | None, int] = case()
    round_box: tuple[Bounds, float, str, str | None, int] = case()
    line: tuple[tuple[Anchor, ...], str, int] = case()
    ellipse: tuple[Bounds, str | None, str | None, int] = case()
    arc: tuple[Bounds, float, float, ArcKind, str | None, str | None, int] = case()
    polygon: tuple[tuple[Anchor, ...], str | None, str, int] = case()
    regular: tuple[Anchor, float, int, float, str | None, str, int] = case()
    stamp: tuple[bytes, Anchor, float] = case()
    blend: tuple[bytes, BlendKind] = case()
    grade: tuple[int, tuple[float, ...]] = case()
    frame: tuple[FilterKind | None, float, int, str, tuple[int, int] | None, bool] = case()

    @staticmethod
    def Text(text: str, xy: Anchor, style: TextStyle = TextStyle()) -> "DrawOp":
        return DrawOp(text=(text, xy, style))

    @staticmethod
    def Caption(
        text: str,
        xy: Anchor,
        style: TextStyle = TextStyle(),
        pad: float = 6.0,
        radius: float = 4.0,
        box_fill: str | None = "white",
        box_outline: str | None = "black",
    ) -> "DrawOp":
        return DrawOp(caption=(text, xy, style, pad, radius, box_fill, box_outline))

    @staticmethod
    def Box(box: Bounds, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(box=(box, outline, fill, width))

    @staticmethod
    def RoundBox(box: Bounds, radius: float = 4.0, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(round_box=(box, radius, outline, fill, width))

    @staticmethod
    def Line(points: tuple[Anchor, ...], fill: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(line=(points, fill, width))

    @staticmethod
    def Ellipse(box: Bounds, outline: str | None = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(ellipse=(box, outline, fill, width))

    @staticmethod
    def Arc(
        box: Bounds, start: float, end: float, kind: ArcKind = "arc", fill: str | None = "black", outline: str | None = "black", width: int = 1
    ) -> "DrawOp":
        return DrawOp(arc=(box, start, end, kind, fill, outline, width))

    @staticmethod
    def Polygon(points: tuple[Anchor, ...], fill: str | None = None, outline: str = "black", width: int = 1) -> "DrawOp":
        return DrawOp(polygon=(points, fill, outline, width))

    @staticmethod
    def Regular(
        center: Anchor, radius: float, sides: int, rotation: float = 0.0, fill: str | None = None, outline: str = "black", width: int = 1
    ) -> "DrawOp":
        return DrawOp(regular=(center, radius, sides, rotation, fill, outline, width))

    @staticmethod
    def Stamp(image: bytes, xy: Anchor, opacity: float = 1.0) -> "DrawOp":
        return DrawOp(stamp=(image, xy, opacity))

    @staticmethod
    def Blend(image: bytes, mode: BlendKind = "multiply") -> "DrawOp":
        return DrawOp(blend=(image, mode))

    @staticmethod
    def Grade(size: int, table: tuple[float, ...]) -> "DrawOp":
        return DrawOp(grade=(size, table))

    @staticmethod
    def Frame(
        filter: FilterKind | None = None,
        radius: float = 2.0,
        border: int = 0,
        border_fill: str = "white",
        fit: tuple[int, int] | None = None,
        exif_orient: bool = False,
    ) -> "DrawOp":
        return DrawOp(frame=(filter, radius, border, border_fill, fit, exif_orient))


@tagged_union(frozen=True)
class FigureOp:
    tag: Literal["scale_fit", "tile", "crop", "merge", "matte", "rotate", "overlay", "pdf", "annotate", "metadata"] = tag()
    scale_fit: tuple[bytes, Span, Span] = case()
    tile: tuple[tuple[bytes, ...], int, Span, Span, float] = case()
    crop: tuple[bytes, Bounds] = case()
    merge: tuple[tuple[bytes, ...], BooleanOp] = (
        case()
    )  # N-source planar set-op (merged tile silhouettes / knockout) — imported graphic/vector `boolean`
    matte: tuple[bytes, float, str] = (
        case()
    )  # fixed-width offset keyline/matte around the artwork silhouette — imported graphic/vector `outline` stroke-to-outline
    rotate: tuple[bytes, str, Corner] = case()
    overlay: tuple[bytes, tuple[MarkSpec, ...]] = case()
    pdf: tuple[bytes, float] = case()
    annotate: tuple[RasterSource, RenderPolicy, tuple[DrawOp, ...], bytes | None] = (
        case()
    )  # trailing `icc` embeds the working-space ICC profile on the PNG egress (color-tagged figure);
    # the full source->dest transform is graphic/color/managed#MANAGED's
    metadata: tuple[bytes, tuple[tuple[int, str], ...], str | None] = case()

    @staticmethod
    def ScaleFit(source: bytes, width: Span, height: Span) -> "FigureOp":
        return FigureOp(scale_fit=(source, width, height))

    @staticmethod
    def Tile(sources: tuple[bytes, ...], columns: int, cell_width: Span, cell_height: Span, gutter: float = 0.0) -> "FigureOp":
        return FigureOp(tile=(sources, columns, cell_width, cell_height, gutter))

    @staticmethod
    def Crop(source: bytes, x: float, y: float, width: float, height: float) -> "FigureOp":
        return FigureOp(crop=(source, (x, y, x + width, y + height)))

    @staticmethod
    def Merge(sources: tuple[bytes, ...], op: BooleanOp = BooleanOp.UNION) -> "FigureOp":
        return FigureOp(merge=(sources, op))

    @staticmethod
    def Matte(source: bytes, width: float = 3.0, stroke: str = "black") -> "FigureOp":
        return FigureOp(matte=(source, width, stroke))

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
    def Annotate(source: RasterSource, render: RenderPolicy = RenderPolicy(), draws: tuple[DrawOp, ...] = (), icc: bytes | None = None) -> "FigureOp":
        return FigureOp(annotate=(source, render, draws, icc))

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
    receipt: ArtifactReceipt


# --- [SERVICES] -------------------------------------------------------------------------
class Figure(Struct, frozen=True):
    op: FigureOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"figure.{self.op.tag}", self._emit, catch=_FAULTS)

    async def _emit(self) -> ContentKey:
        match self.op:
            case FigureOp(tag="annotate", annotate=(source, render, draws, icc)):
                # both the resvg rasterize AND the pillow fold ride the worker BESIDE each other — the
                # heavy native render never evaluates as a loop-thread arg; only the cheap `render.kwargs`
                # dict + the optional ICC profile bytes cross, and `_TRANSIENT` retries a transient
                # `BrokenWorkerProcess` before `_FAULTS`. The `icc` embeds the working-space profile on the
                # PNG egress (a color-tagged figure for the pub/print + drawing-sheet plane), the FULL
                # source->dest ICC transform / soft-proof / CMYK separations riding graphic/color/managed#MANAGED.
                data = await _TRANSIENT(to_process.run_sync, _gated_annotate, render.kwargs(source.keywords()), draws, icc, limiter=_GATE)
            case FigureOp(tag="metadata", metadata=(source, exif, xmp)):
                data = await _TRANSIENT(to_process.run_sync, _gated_metadata, source, exif, xmp, limiter=_GATE)
            case FigureOp(tag="pdf", pdf=(source, scale)):
                # `vl_convert` is a GIL-releasing native (Deno/V8 + resvg) render, so the SVG-to-PDF wrap
                # offloads to the bounded `to_thread` band; only the cheap `source.decode()` crosses as an arg.
                data = await to_thread.run_sync(vl_convert.svg_to_pdf, source.decode(), scale, limiter=_GATE)
            case _:
                # the pure-Python `svgelements` placement fold crosses `to_thread` OFF the loop
                # (OFFLOAD_LANE), the `_extent` `bounds`-rail lift raising into the `_FAULTS` boundary.
                data = await to_thread.run_sync(_compose_vector, self.op, limiter=_GATE)
        return ContentIdentity.key(
            f"figure-{self.op.tag}", data
        )  # bare synchronous accessor: the whole-byte figure source is infallible (no canonical encode),
        # so `_emit` returns a bare `ContentKey` the boundary wraps, never the railed `of`

    def contribute(self) -> "Iterable[Receipt]":
        # `Some` for the in-process arms (bytes/key/receipt off the ONE sync render), `Nothing` for the
        # gated annotate/metadata arms whose receipt rides the async `Figure.of` outward — no placeholder.
        return _placed(self.op).map(lambda placed: tuple(placed.receipt.contribute())).default_value(())

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


# --- [OPERATIONS] -----------------------------------------------------------------------
# the in-process placement carrier the receipt path reads: `Some` for the pdf and vector arms whose
# bytes/key/receipt all derive from the ONE sync render, `Nothing` for the gated annotate/metadata arms
# that mint only inside the async `_emit` — a non-failing absence, never an empty-byte placeholder key
# that would collide across calls and diverge from the real emitted key.
def _placed(op: FigureOp) -> Option[Placed]:
    match op:
        case FigureOp(tag="pdf", pdf=(source, scale)):
            data = vl_convert.svg_to_pdf(source.decode(), scale=scale)
            key = ContentIdentity.key("figure-pdf", data)
            return Some(Placed(key, data, ArtifactReceipt.Pdf(key, len(data), 1)))
        case FigureOp(tag="annotate") | FigureOp(tag="metadata"):
            return Nothing
        case _:
            data = _compose_vector(op)
            xmin, ymin, xmax, ymax = _extent(data)
            key = ContentIdentity.key(f"figure-{op.tag}", data)
            return Some(Placed(key, data, ArtifactReceipt.Preview(key, int(xmax - xmin), int(ymax - ymin))))


def _placed_layers(op: FigureOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    match op:
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            width, height = cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1)
            return tuple(
                Layer(
                    _name(names, index),
                    document(_place(raw, index, columns, cw, ch, gutter), (0.0, 0.0, width, height)),
                    (
                        index % columns * (cw + gutter),
                        index // columns * (ch + gutter),
                        index % columns * (cw + gutter) + cw,
                        index // columns * (ch + gutter) + ch,
                    ),
                )
                for index, raw in enumerate(sources)
            )
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = _extent(source)
            base = Layer(_name(names, 0, "base"), document([fragment(shape) for shape in elements(source)], extent), extent)
            return (base, Layer(_name(names, 1, "overlay"), document(_marks(extent, marks), extent), extent))
        case FigureOp(tag="scale_fit") | FigureOp(tag="crop") | FigureOp(tag="rotate") | FigureOp(tag="merge") | FigureOp(tag="matte"):
            # the single-output placed layer reads its OWN placed extent off the emitted bytes —
            # `scale_fit`/`crop` re-origin the viewport and `merge`/`matte` re-shape it, so the source-document
            # bounds is the wrong frame; one placed layer over the composed result.
            placed = _compose_vector(op)
            return (Layer(_name(names, 0), placed, _extent(placed)),)
        case _:
            # `pdf`/`annotate`/`metadata` carry no named-layer projection (the flat artifact is one
            # implicit layer), so the egress is empty — a VALID arm reached by real cases, never an
            # `assert_never` (which would mis-fire at type-check because those tags are reachable here).
            return ()


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


# the placement-math arm bodies this owner holds over the imported vector surfaces; `bounds`/`elements`/
# `fragment`/`px` live once on graphic/vector/path#PATH, `document`/`clip`/`boolean`/`outline` on
# graphic/vector/region#REGION. Every arm folds the imported `elements(source)` shapes through
# `fragment(shape, matrix)` onto `document(fragments, viewbox)` —
# never the imported `transform` (a whole-document re-frame this owner does not want) and never a
# re-parsed `<svg>` shell. Raises inside cross the `to_thread` seam into the `_FAULTS` boundary.
def _compose_vector(op: FigureOp) -> bytes:
    match op:
        case FigureOp(tag="scale_fit", scale_fit=(source, width, height)):
            (xmin, ymin, xmax, ymax), (tw, th) = _extent(source), (px(width), px(height))
            factor = _fit(tw, th, xmax - xmin, ymax - ymin)
            matrix = Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)
            return document([fragment(shape, matrix) for shape in elements(source)], (0.0, 0.0, tw, th))
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height, gutter)):
            cw, ch = px(cell_width), px(cell_height)
            rows = _rows(len(sources), columns)
            placed = [frag for index, raw in enumerate(sources) for frag in _place(raw, index, columns, cw, ch, gutter)]
            return document(placed, (0.0, 0.0, cw * columns + gutter * (columns - 1), ch * rows + gutter * (rows - 1)))
        case FigureOp(tag="crop", crop=(source, box)):
            # the true geometric crop: `graphic/vector/region#REGION` `clip(source, rect)` intersects each drawable
            # shape against the crop rect through `skia-pathops` `PathOp.INTERSECTION` and frames to the rect —
            # a real severed outline in place of the prior CSS `<clipPath>`; the rail lifts at the shared fault seam.
            return clip(source, box).default_with(_source_fault)
        case FigureOp(tag="merge", merge=(sources, op)):
            # the N-source planar set-op: `graphic/vector/region#REGION` `boolean(sources, op)` folds each source outline
            # through the `skia-pathops` `OpBuilder` UNION/DIFFERENCE/INTERSECTION/XOR — merged tile silhouettes on
            # UNION, a knockout on DIFFERENCE, an overlap mask on INTERSECTION — one composed set-op, never a per-op
            # method, mirroring the `Crop` arm's one-hop `clip` compose; the rail lifts at the shared fault seam.
            return boolean(sources, op).default_with(_source_fault)
        case FigureOp(tag="matte", matte=(source, width, stroke)):
            # the fixed-width offset keyline/matte: `graphic/vector/region#REGION` `outline(source, width)` strokes the
            # artwork silhouette into a closed filled boundary through `skia-pathops` `Path.stroke` (the offset
            # algebra `svgelements` cannot express; `skia-pathops` exposes no `Path.offset`, the stroke IS the offset),
            # the matte `stroke` row lowering at the `document` paint axis and framed UNDER the source so
            # the artwork sits on its own trim/bleed keyline — a bleed-and-trim sheet or margin-frame matte in one hop.
            matte = outline(source, width).default_with(_source_fault)
            return document([*(fragment(shape) for shape in elements(matte)), *(fragment(shape) for shape in elements(source))], _extent(matte))
        case FigureOp(tag="rotate", rotate=(source, angle, corner)):
            extent = _extent(source)
            ax, ay = _anchor(corner, extent, 0.0)
            pivot = Matrix.translate(ax, ay) * Matrix.rotate(_angle(angle)) * Matrix.translate(-ax, -ay)
            # the corner-pivot rotation expands the AABB, so frame to the pivot-transformed extent corners —
            # the pre-rotation extent clips the turned artwork; the shapes lower through the same pivot.
            xmin, ymin, xmax, ymax = extent
            turned = [Point(x, y).matrix_transform(pivot) for x, y in ((xmin, ymin), (xmax, ymin), (xmax, ymax), (xmin, ymax))]
            frame = (min(p.x for p in turned), min(p.y for p in turned), max(p.x for p in turned), max(p.y for p in turned))
            return document([fragment(shape, pivot) for shape in elements(source)], frame)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            extent = _extent(source)
            return document([*(fragment(shape) for shape in elements(source)), *_marks(extent, marks)], extent)
        case _:
            # the in-process vector composer owns only the five SVG-geometry arms; `pdf` rides
            # `vl_convert.svg_to_pdf` and the gated `annotate`/`metadata` arms render on the worker, so a
            # non-vector op cannot reach here through any caller — the boundary `_FAULTS` tuple converts
            # this invariant guard (a partial fold over the closed family forbids `assert_never`).
            raise ValueError(f"figure {op.tag} has no in-process vector composition")


# the ONE rail-to-raise boundary seam: the imported `bounds(source)` `PathFault` rail lifts to the
# `ValueError` the `_FAULTS`-narrowed `async_boundary` classifies into `BoundaryFault`, uniform with the
# pillow/`vl_convert` engine raises — figure composition rails in `BoundaryFault`, so vector's foreign
# `PathFault`/`RegionFault` domains cross into it here rather than a scattered `.default_value` that swallows the cause.
def _extent(source: bytes) -> Bounds:
    return bounds(source).default_with(_source_fault)


def _source_fault(fault: PathFault | RegionFault) -> "NoReturn":
    # the ONE rail-to-raise lift shared by the `_extent` bounds seam and the `Crop` arm's `clip` seam: a
    # `PathFault`/`RegionFault` (empty/parse/degenerate/open_path) becomes the `ValueError` the `_FAULTS` boundary classifies.
    raise ValueError(f"figure source: {fault.tag}")


# the registration-overlay mark fold — each `MarkSpec` builds its own `svgelements` shape through the
# bound `MarkKind.shape` row, positioned by document-corner offset and serialized through the imported
# `fragment` d-string egress on a `Stroked` (d, stroke, width) fragment `document` strokes per mark; the flat and per-layer egresses both compose this one fold.
def _marks(extent: Bounds, marks: tuple[MarkSpec, ...]) -> list[Stroked]:
    return [(fragment(spec.kind.shape(_anchor(spec.corner, extent, spec.inset), spec.size)), spec.stroke, spec.width) for spec in marks]


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


def _place(source: bytes, index: int, columns: int, cell_w: float, cell_h: float, gutter: float) -> list[str]:
    xmin, ymin, xmax, ymax = _extent(source)
    factor = _fit(cell_w, cell_h, xmax - xmin, ymax - ymin)
    column, row = index % columns, index // columns
    # aspect-fit then center the source within its gutter-spaced cell — a top-left cram is the naive n-up.
    ox = column * (cell_w + gutter) + (cell_w - (xmax - xmin) * factor) / 2.0
    oy = row * (cell_h + gutter) + (cell_h - (ymax - ymin) * factor) / 2.0
    matrix = Matrix.translate(ox - xmin * factor, oy - ymin * factor) * Matrix.scale(factor)
    return [fragment(shape, matrix) for shape in elements(source)]


def _angle(value: str) -> "Angle":
    return Angle.parse(value)


def _face(style: TextStyle) -> "ImageFont.FreeTypeFont | ImageFont.ImageFont":
    # the `TextStyle`-to-`FreeTypeFont` resolver: `Layout.RAQM` (HarfBuzz/FriBidi complex-script shaping)
    # is gated on `features.check("raqm")` because a wheel built without libraqm silently falls back, so
    # a build-dependent arm routes on the capability probe rather than assuming the feature; a variable
    # font drives its axes through `set_variation_by_axes`.
    complex_ = bool(style.features or style.direction)
    engine = ImageFont.Layout.RAQM if complex_ and features.check("raqm") else ImageFont.Layout.BASIC
    face = ImageFont.truetype(style.font, style.size, layout_engine=engine) if style.font is not None else ImageFont.load_default(style.size)
    if style.variation and style.font is not None:
        face.set_variation_by_axes(list(style.variation))
    return face


def _gated_annotate(render_kwargs: dict[str, object], draws: Sequence[DrawOp], icc: bytes | None) -> bytes:
    # both the resvg rasterize AND the pillow draw fold run on the worker, so no native render touches
    # the event loop; the `lazy import resvg_py` / `lazy from PIL` proxies reify inside the subprocess.
    image = Image.open(BytesIO(resvg_py.svg_to_bytes(**render_kwargs))).convert("RGBA")
    surface = ImageDraw.Draw(image)
    for op in draws:
        match op:
            case DrawOp(tag="text", text=(content, xy, style)):
                surface.multiline_text(
                    xy,
                    content,
                    font=_face(style),
                    fill=style.fill,
                    anchor=style.anchor,
                    align=style.align,
                    spacing=style.spacing,
                    direction=style.direction,
                    features=list(style.features) or None,
                    language=style.language,
                    stroke_width=style.stroke_width,
                    stroke_fill=style.stroke_fill,
                )
            case DrawOp(tag="caption", caption=(content, xy, style, pad, radius, box_fill, box_outline)):
                face = _face(style)
                left, top, right, bottom = surface.multiline_textbbox(
                    xy, content, font=face, align=style.align, spacing=style.spacing, stroke_width=style.stroke_width
                )
                surface.rounded_rectangle((left - pad, top - pad, right + pad, bottom + pad), radius=radius, fill=box_fill, outline=box_outline)
                surface.multiline_text(
                    xy,
                    content,
                    font=face,
                    fill=style.fill,
                    align=style.align,
                    spacing=style.spacing,
                    stroke_width=style.stroke_width,
                    stroke_fill=style.stroke_fill,
                )
            case DrawOp(tag="box", box=(box, outline, fill, width)):
                surface.rectangle(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="round_box", round_box=(box, radius, outline, fill, width)):
                surface.rounded_rectangle(box, radius=radius, outline=outline, fill=fill, width=width)
            case DrawOp(tag="line", line=(points, fill, width)):
                surface.line(points, fill=fill, width=width)
            case DrawOp(tag="ellipse", ellipse=(box, outline, fill, width)):
                surface.ellipse(box, outline=outline, fill=fill, width=width)
            case DrawOp(tag="arc", arc=(box, start, end, kind, fill, outline, width)):
                match kind:
                    case "arc":
                        surface.arc(box, start, end, fill=fill, width=width)
                    case "chord":
                        surface.chord(box, start, end, fill=fill, outline=outline, width=width)
                    case "pieslice":
                        surface.pieslice(box, start, end, fill=fill, outline=outline, width=width)
                    case _ as unreachable:
                        assert_never(unreachable)
            case DrawOp(tag="polygon", polygon=(points, fill, outline, width)):
                surface.polygon(points, fill=fill, outline=outline, width=width)
            case DrawOp(tag="regular", regular=(center, radius, sides, rotation, fill, outline, width)):
                surface.regular_polygon((center[0], center[1], radius), sides, rotation=int(rotation), fill=fill, outline=outline, width=width)
            case DrawOp(tag="stamp", stamp=(raw, xy, opacity)):
                mark = Image.open(BytesIO(raw)).convert("RGBA")
                if opacity < 1.0:
                    mark.putalpha(mark.getchannel("A").point(lambda level: int(level * opacity)))
                image.alpha_composite(mark, (int(xy[0]), int(xy[1])))
            case DrawOp(tag="blend", blend=(raw, mode)):
                overlay = Image.open(BytesIO(raw)).convert("RGBA")
                image = getattr(ImageChops, mode)(
                    image, overlay if overlay.size == image.size else overlay.resize(image.size, Image.Resampling.LANCZOS)
                )
                surface = ImageDraw.Draw(image)
            case DrawOp(tag="grade", grade=(size, table)):
                image = image.filter(ImageFilter.Color3DLUT(size, list(table)))
                surface = ImageDraw.Draw(image)
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
                    case "posterize":
                        image = ImageOps.posterize(image.convert("RGB"), min(8, max(1, int(radius)))).convert("RGBA")
                    case "solarize":
                        image = ImageOps.solarize(image.convert("RGB"), int(radius)).convert("RGBA")
                    case None:
                        pass
                    case _:
                        assert_never(kind)
                surface = ImageDraw.Draw(image)
            case _:
                assert_never(op)
    sink = BytesIO()
    # ICC-aware egress: `icc_profile=` tags the placed figure PNG with its working-space profile so a downstream
    # consumer reads the figure in a managed space (the pub/print AND ISO drawing-sheet figure plane); `None` is a
    # no-op. The FULL source->dest ICC transform, lcms2 soft-proof, and CxF3 CMYK separations are
    # graphic/color/managed#MANAGED's owned lcms2/pyvips surface, composed outward on the emitted raster, never here.
    image.save(sink, format="PNG", icc_profile=icc)
    return sink.getvalue()


def _gated_metadata(payload: bytes, exif_tags: Sequence[tuple[int, str]], xmp: str | None) -> bytes:
    # figure-egress-local descriptive-metadata convenience: an in-worker pillow EXIF/XMP tag write on the placed PNG
    # so a bare figure carries its title/creator/rights without a second async hop. This is NOT the authoritative
    # cross-format descriptive-metadata seal — exchange/metadata#METADATA is the categorical-best owner (pyexiftool
    # over EXIF+IPTC+XMP+ICC+GPS+maker-notes in one cross-format pass), composed outward on the emitted figure when a
    # full IPTC/XMP/provenance write is the deliverable; the deliberate boundary is the in-worker PNG-tag convenience here.
    image = Image.open(BytesIO(payload))
    exif = image.getexif()
    exif.update(exif_tags)
    packet = xmp if xmp is not None else image.info.get("XML:com.adobe.xmp")
    sink = BytesIO()
    image.save(sink, format=image.format or "PNG", exif=exif, xmp=packet.encode() if isinstance(packet, str) else packet)
    return sink.getvalue()
```

## [03]-[RESEARCH]

- [SHAPE_CTOR_RESEARCH] [RESOLVED]: the nine `MarkKind` members resolve their `svgelements` shape through `getattr(svgelements, self.primitive)(*self.args(*anchor, size))`, and every positional constructor is now verified against the installed `svgelements 1.9.6` — `Circle(cx, cy, r)`, `Ellipse(cx, cy, rx, ry)`, `Rect(x, y, w, h)`, `SimpleLine(x1, y1, x2, y2)`, and the `*points`-spread `Polyline`/`Polygon` (the `CROSS`/`STAR` registration spokes reuse the `Polyline` point-sequence, adding no new shape class), each confirmed to populate `cx`/`cy`/`rx`/`x`/`width`/`x1`/`points` and to accept a `Path(shape)` conversion for the `fragment` d-string egress owner. The isolation onto the one `MarkKind.shape` builder keeps every construction spelling at a single cite-point; the `SCALE_FIT`/`TILE`/`CROP`/`ROTATE`/`OVERLAY` placement arms and the `MarkKind.shape` builder are settled fence.
- [ROTATE_PARSE_SETTLED]: the `ROTATE` arm resolves the angle string through `_angle`, the one boundary-scoped helper holding `svgelements.Angle.parse(angle)`, verified to return the radian float `Matrix.rotate` admits (`Angle.parse("45deg") == 0.785398...`), so the `Rotate` arm's `Matrix.rotate(Angle.parse(...))` composition is settled fence (resolving the prior `[ROTATE_PARSE_RESEARCH]` gate the `graphic/vector/path#PATH` page closed).
- [OFFLOAD_LANE_SETTLED]: the vector placement arms are pure-Python `svgelements` work whose imports resolve on the core (host-free geometry floor), but the CONCERN crosses `to_thread.run_sync(_compose_vector, self.op, limiter=_GATE)` off the event loop per the concurrency `OFFLOAD_LANE` law rather than running inline — a light SVG fold still starves the loop if it runs on it, so the offload seam is load-bearing, and `contribute`/`_placed` re-enter the same deterministic `_compose_vector` synchronously off the loop (the sibling `composition/sheet#SHEET`/`imposition#IMPOSE` `Composed` re-entry). The `to_thread` band (shared address space, imports already loaded on the core) is the lane, not `to_interpreter` (a subinterpreter cannot cheaply re-import the geometry surface) and not `to_process` (reserved for the GIL-hostile `pillow`/resvg native arms).
- [WORKER_RETRY_SETTLED]: the `to_process` annotate/metadata band rides `_TRANSIENT = stamina.AsyncRetryingCaller(attempts=3, timeout=30.0).on(BrokenWorkerProcess)`, verified against `stamina 26.1.0` (`AsyncRetryingCaller(...).on(exc)` returns a reusable `BoundAsyncRetryingCaller` whose `__call__(afn, /, *a, **kw)` opens a fresh retry context per call), so a transient OOM/signal worker death recovers before the `_FAULTS` boundary rails an exhausted death as a `BoundaryFault` resource case — the same worker-seam retry the sibling `resvg`/`pillow` worker arms sharing the lane carry, and the reason `BrokenWorkerProcess` stays in `_FAULTS` (the terminal after the retry exhausts) rather than being absent. The domain `ValueError` a bad SVG raises is NOT in the `.on` set — content, not transport, surfaces immediately.
- [PILLOW_ANNOTATION_SETTLED]: the enriched `DrawOp` family is verified against `pillow 12.2.0` — `ImageDraw.multiline_text`/`multiline_textbbox`/`textbbox`/`rounded_rectangle`/`regular_polygon`/`arc`/`chord`/`pieslice`, `Image.alpha_composite` (the in-place premultiplied over-composite method), `ImageFilter.Color3DLUT`, the `ImageChops` blend family (`multiply`/`screen`/`overlay`/`soft_light`/`hard_light`/`difference`/`add`/`subtract`/`darker`/`lighter`), `ImageOps.posterize`/`solarize`, `ImageFont.Layout.RAQM`, and `FreeTypeFont.set_variation_by_axes` all confirmed present. `features.check("raqm")` returned `False` on the built wheel, so the `_face` capability gate is load-bearing: complex-script text falls back to `Layout.BASIC` when the build lacks libraqm rather than assuming the feature. The `Stamp` arm's `Image.alpha_composite` replaces the prior `Image.paste`+`getchannel` mask fold as the correct premultiplied over-composite; the `Blend`/`Grade` arms rebind the draw surface because `ImageChops`/`Color3DLUT` return a new image.
- [FAULT_NARROWING] [RESOLVED]: `Figure.of` calls `async_boundary(f"figure.{self.op.tag}", self._emit, catch=_FAULTS)` where `_FAULTS = (ValueError, OSError, BrokenWorkerProcess, BeartypeCallHintViolation)` is the real raise tuple the engine surface throws — `resvg_py.svg_to_bytes`/`vl_convert.svg_to_pdf`/`svgelements` parse raise `ValueError` on an empty or invalid SVG and on render failure (the `_extent` seam lifting the imported `bounds` `PathFault` rail onto a `ValueError` too), the `.svgz` file source raises `OSError`, the gated `anyio.to_process.run_sync` seam raises `BrokenWorkerProcess` on an exhausted worker death after `_TRANSIENT`, and the `_GUARD`-contracted `_fit`/`_rows` division seams raise `BeartypeCallHintViolation` on a degenerate zero-extent source bbox or a non-positive n-up column count. The runtime `faults#FAULTS` owner's `async_boundary[T](subject, thunk, *, catch=...)` wraps the awaited thunk's bare return in `Ok` and its `_convert` folds each caught cause through `BoundaryFault.of(subject, cause)`, the `CLASSIFY` table mapping `ValueError` to the `boundary` catch-all (carrying the `_extent` fault tag as `str(cause)`), `OSError`/`BrokenWorkerProcess` to `resource`, and `BeartypeCallHintViolation` to `api`. `BrokenWorkerInterpreter` is NOT in the tuple: this owner offloads only through `to_process`/`to_thread`, never `to_interpreter`, so the interpreter-death case is unreachable and a tuple member for it would be a phantom fault. Cancellation is excluded so the structured-cancellation signal re-raises past the boundary unconverted, as the concurrency owner requires.
- [LAYERED_EGRESS] [RESOLVED]: the placed multi-source layout the `_compose_vector` placement arms emit is one flat single-`<svg>` document built by the imported `graphic/vector/region#REGION` `document(fragments, viewbox)` assembly; the editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s owner. The `Figure.layers(names)` projection exposes the placed layout as `tuple[Layer, ...]`, ONE `export/layered#LAYERED` `Layer(name, source, bbox, visible=True, locked=False)` row per placed source carrying its placed `bbox` — the `Tile` arm yields one row per grid cell carrying the cell's placed extent through `_placed_layers`/`_place`, the `Overlay` arm yields a `base` row (the imported `elements(source)` base fragments) plus a registration-`overlay` row over the same document bounds, and the single-source `ScaleFit`/`Crop`/`Rotate` arms yield one row over the placed document bounds. The named-layer authoring is one outward seam to the `export/layered` sub-domain, so figure composition stays the flat post-render placement owner emitting the flat document and the `tuple[Layer, ...]` projection. The `drawsvg` `Group(id=...)` named-group and pikepdf OCG member spellings are the `export/layered` page's verification burden, not this page's fence.
- [VECTOR_SURFACE_PUBLICIZE] [RESOLVED]: figure composition imports `Bounds`/`PathFault`/`Span`/`bounds`/`elements`/`fragment`/`px` from `graphic/vector/path#PATH` and `BooleanOp`/`RegionFault`/`RenderPolicy`/`boolean`/`clip`/`document`/`outline` from `graphic/vector/region#REGION`; both `__all__` rosters publicize the full set (byte-sourced and rail-typed), `elements(source) -> list[Element]` is the parse-and-drawable sweep so figure composition re-parses no document, `Span` is the CSS-length value the `ScaleFit`/`Tile` `width`/`height` fields carry and `px` resolves, and `RenderPolicy.kwargs(source: Mapping[str, str])` takes the source-keyword dict the figure `RasterSource.keywords()` projection feeds, so `render.kwargs(source.keywords())` merges the markup-or-file key with the policy rows; the placement-math arm bodies stay this owner's and compose the imported surfaces in one hop (the prior `_hit_elements` re-declared parse stays retired, the crop reading the imported `clip`).
- [GEOMETRIC_CROP] [RESOLVED]: the `Crop` arm now composes the imported `graphic/vector/region#REGION` `clip(source, rect)` — the per-shape `pathops.op(shape, window, PathOp.INTERSECTION)` cut (verified `pathops.op` + `PathOp.INTERSECTION=1` against `skia-pathops 0.9.2`) that intersects each straddling `<path>` against the crop rect and emits a real clipped outline framed to the rect — through `clip(source, box).default_with(_source_fault)`, so a straddling shape is genuinely severed at the crop edge and separate shapes stay separate fragments. The CSS `<defs><clipPath>` `_clipped` egress (and the `_hit_elements`/`_hits` bbox pre-filter it needed) is RETIRED; `skia-pathops` stays `graphic/vector/region#REGION`'s admitted boolean/offset owner (brief FOCUS_SET 3) composed one hop with zero new engine here.
- [BOOLEAN_OFFSET_ARMS] [RESOLVED]: the `Merge` and `Matte` arms deepen the `skia-pathops` composition beyond the single `clip` INTERSECTION the `Crop` arm used — `Merge` composes the imported `graphic/vector/region#REGION` `boolean(sources, op)` N-ary `OpBuilder` set-op (verified `PathOp.UNION`/`DIFFERENCE`/`INTERSECTION`/`XOR`/`REVERSE_DIFFERENCE` against `skia-pathops 0.9.2`) for the merged tile silhouette / knockout / overlap the brief's Growth axis named, and `Matte` composes the imported `outline(source, width)` stroke-to-outline (verified `Path.stroke` present, `Path.offset` ABSENT on `skia-pathops 0.9.2` — the stroke IS the offset, so the fixed-width offset keyline rides `Path.stroke`, never a phantom `Path.offset`) for the margin-frame matte / bleed-trim keyline. Both are thin one-hop composes of the imported public `graphic/vector/region#REGION` surface (`boolean`/`outline` are `RegionFault`-railed, lifted through the shared `_source_fault` seam exactly as `clip` is), never a direct `skia-pathops` import here — the boolean/offset engine stays `graphic/vector/region#REGION`'s (brief FOCUS_SET 3) and figure composition composes it. Justified on PACKAGE (the verified `skia-pathops` boolean/stroke surface the reading map flagged, composed through its `graphic/vector/region#REGION` owner) and DOMAIN (a press bleed-and-trim sheet / margin-frame matte / merged silhouette).
- [ICC_TAGGED_EGRESS] [RESOLVED]: the `Annotate` arm's `icc: bytes | None` embeds the working-space ICC profile on the placed figure PNG through `Image.save(sink, format="PNG", icc_profile=icc)` (a pillow-native egress tag verified on `pillow 12.2.0`, `None` a no-op), so the color-correct figure egresses tagged for the pub/print AND ISO drawing-sheet plane the brief grades `compose` against — the color-tag the reading map named. This is a DISTINCT control from a color CONVERSION: the FULL source->dest ICC transform, the lcms2 soft-proof (`ImageCms.buildProofTransform` + `Flags.SOFTPROOFING`/`GAMUTCHECK`), and the CxF3 CMYK separations are `graphic/color/managed#MANAGED`'s owned `pyvips`/`ImageCms`/`colour-cxf` surface (the `ManageOp.Managed` async owner), composed OUTWARD on the emitted RGBA raster at the async `Figure` level for the press plane, never re-owned in the sync worker — so the tag is figure-local and the transform is the color owner's, no engine overlap. Justified on PACKAGE (the verified pillow `icc_profile=` save the reading map flagged) and DOMAIN (a color-managed journal-figure / drawing-sheet figure source).
- [METADATA_BOUNDARY] [RESOLVED]: the `Metadata` arm's in-worker pillow `getexif`/`Image.Exif`/`info["XML:com.adobe.xmp"]` write is DELIBERATELY scoped to the figure-egress-local convenience — a bare placed figure carrying its title/creator/rights on the immediate PNG without a second async hop. It is NOT the authoritative cross-format descriptive-metadata seal: `exchange/metadata#METADATA` is the categorical-best owner (the `pyexiftool` `-stay_open` cross-format pass over EXIF+IPTC+XMP+ICC+GPS+maker-notes, plus the `pikepdf`/`av` PDF/media carriers, returning the `(ContentKey, MetaFacts, bytes)` triple onto the `core/receipt#RECEIPT` `ArtifactReceipt.Metadata` case), composed OUTWARD on the emitted figure when a full IPTC/XMP/provenance write is the deliverable. The overlap the reading map flagged is resolved by explicit boundary, not a thin pillow re-do beside the categorical-best owner: figure composition stays the placement owner and routes the authoritative descriptive-metadata write to its owner, the in-worker arm carrying only the PNG-tag convenience. Justified on CONSUMER (the seam-unification boundary the categorical-best mandate fixes).
