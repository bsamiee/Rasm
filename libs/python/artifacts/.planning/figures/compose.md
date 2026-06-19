# [PY_ARTIFACTS_COMPOSE]

The figure-composition owner turning emitted graphics into placed, annotated, color-correct figures. `Figure` is ONE owner over the post-render composition pipeline: it reads the SVG that the `figures/chart#CHART`, `figures/preview#PREVIEW` `MARK`, and `figures/table#TABLE` owners already emit and lays it out — scale-to-fit a target viewport, tile an n-up sheet, crop to a bounds box — over `svgelements` on the cp315 core in-process, then draws captions/legends/borders, applies a sharpen/blur post-filter, and binds EXIF/XMP metadata over `pillow` on the gated `python_version<'3.15'` band. One figure surface discriminating the operation, not a per-graphic-type composer family. The vector layout arms resolve in-process because `svgelements` is a pure-Python `py3-none-any` wheel that imports on the core; the raster annotate/metadata arms ride the runtime subprocess seam because the cp315-core process imports no gated distribution. Figure composition rasterizes nothing of its own (rasterization routes to `figures/chart#EXPORT` `vl-convert`/`kaleido`) and re-renders no chart; it places and finishes already-emitted graphics. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `receipt/receipt#RECEIPT` `ArtifactReceipt.Preview` case.

## [1]-[INDEX]

- [1]-[COMPOSE]: figure-placement, annotate, and metadata owner over `svgelements` (vector layout, core) and `pillow` (raster annotate/metadata, gated band).

## [2]-[COMPOSE]

- Owner: `Figure` the one figure-composition owner discriminating operation; `FigureOp` the closed `StrEnum` of composition operations; the `svgelements` `SVG` document is the vector working surface, the `Matrix`/`Path`/`bbox` triad the layout algebra, the `pillow` `Image` the raster annotate/metadata surface on the gated floor.
- Cases: `FigureOp` rows `SCALE_FIT` (resolve the source viewport, derive the `Matrix.scale` that fits a target `Length` box preserving aspect, re-emit the transformed SVG) · `TILE` (n-up sheet — place each source SVG into a row-major grid cell, each placement a `Matrix.translate`-after-`Matrix.scale` of the source bounds into the cell) · `CROP` (intersect each element bounds with a crop box, drop the out-of-bounds elements, re-emit the clipped SVG) · `ANNOTATE` (rasterize-then-draw — `ImageDraw` caption/legend/border text and shapes plus an `ImageOps.expand` border and an `ImageFilter` sharpen/blur post-pass over the gated band) · `METADATA` (bind/read EXIF and XMP — `Image.getexif`/`Image.Exif` tag map plus the `info["XML:com.adobe.xmp"]` XMP packet, re-encoded on save) — matched by `match`/`case`; never a sibling op per source media type, and never a parallel figure class.
- Entry: `Figure.of` is `async` over the runtime `async_boundary`, dispatches the operation, and returns a `RuntimeRail[ContentKey]`; the `SCALE_FIT`/`TILE`/`CROP` vector arms render in-process with no pillow dependency (`svgelements` is pure-Python and imports on the core), the `ANNOTATE`/`METADATA` raster arms ride the subprocess seam (`anyio.to_process.run_sync`) where the gated-band worker imports `PIL` at module scope.
- Auto: `_emit` folds the op — the vector arms (`SCALE_FIT`/`TILE`/`CROP`) through `_compose_vector` which parses each source through `SVG.parse(..., reify=True)`, reads the document/element `bbox`, composes the `Matrix` transform, and re-serializes via the placed `Path.d()`/element transform onto a fresh `SVG` document; the raster arms (`ANNOTATE`/`METADATA`) through the gated-band worker where `Image.open`, `ImageDraw.Draw`, `ImageFont.truetype`, `ImageOps.expand`, `ImageFilter.UnsharpMask`/`GaussianBlur`, and the `Image.Exif`/XMP map run at module scope.
- Receipt: each operation contributes `receipt/receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the placed figure's pixel/viewport width and height; figure composition adds NO new receipt case — the placed-figure facts are width/height, the Preview shape.
- Packages: `svgelements` (`SVG.parse`/`SVG.elements`, `Path`/`Path.d`/`Path.bbox`, `Matrix`/`Matrix.scale`/`Matrix.translate`/`Matrix.rotate`, `Length.value`, `Color`, `Point`, the `Rect`/`Circle`/`Ellipse`/`Polygon`/`Polyline`/`SimpleLine` primitives, pure-Python `py3-none-any` v1.9.6 reflected on cp315) on the cp315 core; `pillow` (`Image.open`/`Image.new`/`Image.getexif`/`Image.Exif`, `ImageDraw.Draw`/`text`/`rectangle`, `ImageFont.truetype`/`load_default`, `ImageOps.expand`/`fit`, `ImageFilter.UnsharpMask`/`GaussianBlur`) gated `python_version<'3.15'`; runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `anyio.to_process.run_sync` the gated-band subprocess lane).
- Growth: a new vector layout operation (rotate-place, gutter/margin policy, registration-mark overlay) is one `FigureOp` row plus one `_compose_vector` arm over the existing `Matrix`/`bbox` algebra — never a re-implemented SVG transform; a new raster annotation primitive is one `ImageDraw` call on the gated worker; a new metadata channel (IPTC, ICC-profile name) is one tag read/write on the existing `Image` map; a second SVG renderer is rejected — `vl-convert` rasterizes, `svgelements` lays out. Zero new surface.
- Boundary: a per-graphic-type figure-composer class family is the deleted form; no UI, no live viewer, no chart re-render. `svgelements` owns SVG geometry/transform/parse/bounds — figure composition reads `bbox` and re-serializes through `Path.d()`/element transform, never a hand-rolled affine helper or a re-parsed path string. Rasterization routes to `figures/chart#EXPORT` `vl-convert`/`kaleido`; the chart/QR/nanoplot SVG sources arrive from `figures/chart#CHART`, `figures/preview#PREVIEW` `MARK`, and `figures/table#TABLE`. `pillow` annotate/metadata rides the gated `python_version<'3.15'` band and never resolves in the cp315-core process, so the raster arms dispatch onto the runtime subprocess lane where the gated-band worker imports `PIL` at module scope — neither a module-top nor a lazy gated import lands on the core page. ICC profile attachment and color management stay in `figures/color#COLOR` `MANAGED`; figure composition consumes a color-managed raster, it does not build the transform.

```python signature
from enum import StrEnum
from io import BytesIO
from typing import assert_never

from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary


class FigureOp(StrEnum):
    SCALE_FIT = "scale_fit"
    TILE = "tile"
    CROP = "crop"
    ANNOTATE = "annotate"
    METADATA = "metadata"


GATED: frozenset[FigureOp] = frozenset({FigureOp.ANNOTATE, FigureOp.METADATA})


class Figure(Struct, frozen=True):
    op: FigureOp
    sources: tuple[bytes, ...]
    params: dict[str, object]

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"figure.{self.op}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_raster, self.op.value, self.sources[0], self.params)
            if self.op in GATED
            else _compose_vector(self.op, self.sources, self.params)
        )
        return ContentIdentity.of(f"figure-{self.op}", data)


def _compose_vector(op: FigureOp, sources: tuple[bytes, ...], params: dict[str, object]) -> bytes:
    from svgelements import SVG, Length, Matrix

    match op:
        case FigureOp.SCALE_FIT:
            document = SVG.parse(BytesIO(sources[0]), reify=True)
            target_w = Length(params["width"]).value(ppi=params.get("ppi", 96.0))
            target_h = Length(params["height"]).value(ppi=params.get("ppi", 96.0))
            xmin, ymin, xmax, ymax = _document_bounds(document)
            factor = min(target_w / (xmax - xmin), target_h / (ymax - ymin))
            transform = Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)
            return _serialize(document, transform, target_w, target_h)
        case FigureOp.TILE:
            columns = int(params["columns"])
            cell_w = Length(params["cell_width"]).value(ppi=params.get("ppi", 96.0))
            cell_h = Length(params["cell_height"]).value(ppi=params.get("ppi", 96.0))
            placements = [_place(SVG.parse(BytesIO(source), reify=True), index, columns, cell_w, cell_h) for index, source in enumerate(sources)]
            rows = (len(sources) + columns - 1) // columns
            return _compose(placements, cell_w * columns, cell_h * rows)
        case FigureOp.CROP:
            document = SVG.parse(BytesIO(sources[0]), reify=True)
            box = (params["x"], params["y"], params["x"] + params["width"], params["y"] + params["height"])
            kept = [element for element in document.elements() if _intersects(_element_bounds(element), box)]
            transform = Matrix.translate(-box[0], -box[1])
            return _compose([(element, transform) for element in kept], params["width"], params["height"])
        case _:
            assert_never(op)


def _document_bounds(document: object) -> tuple[float, float, float, float]:
    boxes = [_element_bounds(element) for element in document.elements() if _element_bounds(element) is not None]
    return (min(box[0] for box in boxes), min(box[1] for box in boxes), max(box[2] for box in boxes), max(box[3] for box in boxes))


def _element_bounds(element: object) -> tuple[float, float, float, float] | None:
    bbox = getattr(element, "bbox", None)
    return bbox() if callable(bbox) else None


def _place(document: object, index: int, columns: int, cell_w: float, cell_h: float) -> tuple[object, object]:
    from svgelements import Matrix

    xmin, ymin, xmax, ymax = _document_bounds(document)
    factor = min(cell_w / (xmax - xmin), cell_h / (ymax - ymin))
    column, row = index % columns, index // columns
    transform = Matrix.translate(column * cell_w - xmin * factor, row * cell_h - ymin * factor) * Matrix.scale(factor)
    return (document, transform)


def _intersects(bounds: tuple[float, float, float, float] | None, box: tuple[float, float, float, float]) -> bool:
    if bounds is None:
        return False
    return not (bounds[2] < box[0] or bounds[0] > box[2] or bounds[3] < box[1] or bounds[1] > box[3])


def _serialize(document: object, transform: object, width: float, height: float) -> bytes:
    from svgelements import Path

    segments = [(Path(element) * transform).d() for element in document.elements() if hasattr(element, "d") or hasattr(element, "bbox")]
    return _svg_document(segments, width, height)


def _compose(placements: list[tuple[object, object]], width: float, height: float) -> bytes:
    from svgelements import Path

    segments = []
    for element, transform in placements:
        elements = element.elements() if hasattr(element, "elements") else (element,)
        segments.extend((Path(child) * transform).d() for child in elements)
    return _svg_document(segments, width, height)


def _svg_document(segments: list[str], width: float, height: float) -> bytes:
    paths = "".join(f'<path d="{segment}"/>' for segment in segments)
    body = f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">{paths}</svg>'
    return body.encode()


def _gated_raster(op: str, payload: bytes, params: dict[str, object]) -> bytes:
    from io import BytesIO

    from PIL import Image, ImageDraw, ImageFilter, ImageFont, ImageOps

    match FigureOp(op):
        case FigureOp.ANNOTATE:
            image = Image.open(BytesIO(payload)).convert("RGBA")
            border = int(params.get("border", 0))
            if border:
                image = ImageOps.expand(image, border=border, fill=params.get("border_fill", "white"))
            draw = ImageDraw.Draw(image)
            font = ImageFont.truetype(params["font"], int(params.get("font_size", 16))) if "font" in params else ImageFont.load_default()
            for caption in params.get("captions", ()):
                draw.text(tuple(caption["xy"]), caption["text"], font=font, fill=caption.get("fill", "black"), anchor=caption.get("anchor"))
            for legend in params.get("boxes", ()):
                draw.rectangle(
                    tuple(legend["box"]), outline=legend.get("outline", "black"), fill=legend.get("fill"), width=int(legend.get("width", 1))
                )
            sharpen = params.get("sharpen")
            blur = params.get("blur")
            if sharpen:
                image = image.filter(ImageFilter.UnsharpMask(radius=float(sharpen)))
            elif blur:
                image = image.filter(ImageFilter.GaussianBlur(radius=float(blur)))
            sink = BytesIO()
            image.save(sink, format=params.get("format", "PNG"))
            return sink.getvalue()
        case FigureOp.METADATA:
            image = Image.open(BytesIO(payload))
            exif = image.getexif()
            for tag, value in params.get("exif", {}).items():
                exif[int(tag)] = value
            xmp = params.get("xmp")
            if xmp is not None:
                image.info["XML:com.adobe.xmp"] = xmp
            sink = BytesIO()
            image.save(sink, format=params.get("format") or image.format, exif=exif, xmp=xmp.encode() if isinstance(xmp, str) else xmp)
            return sink.getvalue()
        case _:
            assert_never(op)
```

## [3]-[RESEARCH]

- [VECTOR_SETTLED]: the in-process `SVG.parse(source, reify=True)`/`SVG.elements`, `Path(element)`/`Path.d`/`Path.bbox`, `Matrix.scale`/`Matrix.translate`/`Matrix.rotate` (composed by `*`), `Length(value).value(ppi=...)`, and the `Color`/`Point` value objects verify against the folder `.api` catalogue for `svgelements`, a VERIFIED REAL reflection (`1.9.6` on the cp315 core, pure-Python `py3-none-any`). The `SCALE_FIT`/`TILE`/`CROP` arms are SETTLED fence code: `reify=True` resolves transforms into element geometry so the `bbox()` read returns absolute coordinates, and each placement composes through one `Matrix` (`translate`-after-`scale`), never a hand-rolled affine. Bounds query is `bbox()` on the document elements; layout re-serializes the placed `Path.d()` onto a fresh `SVG` document. svgelements owns the SVG path grammar and affine algebra; figure composition never re-implements either, never rasterizes (rasterization routes to `figures/chart#EXPORT` `vl-convert`/`kaleido`), and never re-renders a chart.
- [RASTER_SETTLED]: the `ANNOTATE`/`METADATA` arms run on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `PIL` at boundary scope inside the gated-band worker, never on the cp315-core owner. The `Image.open`/`Image.getexif`/`Image.Exif`/`Image.save`, `ImageDraw.Draw`/`text`/`rectangle`, `ImageFont.truetype`/`load_default`, `ImageOps.expand`, and `ImageFilter.UnsharpMask`/`GaussianBlur` spellings verify against the folder `.api` catalogue for `pillow` (`12.2.0` reflected on the gated cp313 band): `ImageDraw.ImageDraw.text`/`rectangle` are the catalogued draw rows, `ImageFont.truetype`/`load_default` the font rows, `ImageOps` the operation-module surface, `ImageFilter.GaussianBlur`/`UnsharpMask` the filter factory rows, and `Image.Exif`/`Image.getexif` the metadata map. The XMP packet rides `image.info["XML:com.adobe.xmp"]` and the `save(..., xmp=...)` kwarg. All figure pillow members are catalogue-confirmed settled fence code — the figure rail uses no `ImageCms` transform spelling (ICC color management is `figures/color#COLOR` `MANAGED`, which holds the `ImageCms` build/apply surface as its own [RESEARCH] seam), so this page carries no pillow RESEARCH gate.
- [HANDOFF_GUARD] [BLOCKED]: the outward figure edge to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a private per-artifact handoff, and the figure source re-mints no canonical concept so the `runtime/evidence` `Structural.drift` query stays clean. `Figure._emit` returns `ContentIdentity.of(...)` (re-minting no content-identity seed) and contributes the existing `ArtifactReceipt.Preview` case (re-minting no receipt rail), so the artifacts side is verification-and-alignment only. Close-condition: the upstream `compute/graduation` `HandoffAxis` model-asset case and the `runtime/evidence` `Structural.drift` detector land; the figure rail threads the same `ContentKey` into the one outward handoff with zero new surface.
