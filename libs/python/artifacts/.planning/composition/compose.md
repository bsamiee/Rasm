# [PY_ARTIFACTS_COMPOSE]

The post-render figure/section placement owner turning emitted graphics into placed, annotated, color-correct figures. `Figure` is ONE owner over the post-render composition pipeline carrying a closed-payload `FigureOp` `expression.tagged_union` — each operation a case carrying its own typed payload, never a `StrEnum` keyed against an erased `dict[str, object]` bag — dispatched by one total `match`. It reads the SVG that the regrouped `visualization/chart#CHART`, `graphic/marks#MARK`, and `visualization/table#TABLE` sibling owners already emit and lays it out — scale-to-fit a target viewport, tile an n-up sheet, crop to a bounds box, rotate-place at an `Angle`, overlay registration marks and crop guides — composing the `graphic/vector#VECTOR` SVG-geometry primitive (the `svgelements` `SVG`/`Matrix`/`Path`/`Color`/`Length`/`Angle`/`Point`/`bbox` parse-transform-bounds-serialize surface) on the cp315 core in-process, rasterizes its own placed SVG through that primitive's `resvg_py.svg_to_bytes` raster floor on the core, then draws captions/legends/borders, fits or EXIF-orients the raster, applies a sharpen/blur/enhance post-chain, and binds EXIF/XMP metadata over `pillow` on the gated `python_version<'3.15'` band. One figure surface discriminating the operation, not a per-graphic-type composer family. The vector geometry arms resolve in-process because `graphic/vector#VECTOR` is the host-free geometry floor — `svgelements` is a pure-Python `py2.py3-none-any` wheel and `resvg_py` the cp315 native extension, both importing on the core; the raster annotate/metadata arms first rasterize the placed SVG through that primitive's `resvg_py` floor on the core, then ride the runtime subprocess seam for the `pillow` draw/filter/metadata pass because the cp315-core process imports no gated distribution. Figure composition rasterizes only its own placed SVG for the annotate pass through the `graphic/vector#VECTOR` `Rasterize` floor — chart/mark/nanoplot rasterization-to-export stays in `visualization/chart#EXPORT` `vl-convert` — and re-renders no chart; it places, rasterizes, and finishes already-emitted graphics. The placed multi-source layout the vector arms emit is one flat single-`<svg>` document — that flat-SVG egress is this owner's concern; the editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s concern, receiving the same placed layout and binding each placed source as a named editable layer rather than one flattened path soup, so figure composition emits the flat artifact and routes the named-layer authoring outward instead of growing a layer-emit arm. Every operation returns a `RuntimeRail[ContentKey]` and contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case carrying the placed-figure width and height.

## [01]-[INDEX]

- [01]-[COMPOSE]: figure-placement, overlay, rasterize, annotate, and metadata owner over the closed-payload `FigureOp` `tagged_union` composing the `graphic/vector#VECTOR` geometry primitive (`svgelements` vector layout + `resvg_py` SVG-to-PNG raster floor, both cp315-core) for parse/transform/bounds/serialize/rasterize, and dispatching the gated `pillow` raster annotate/metadata pass on the `python_version<'3.15'` band.

## [02]-[COMPOSE]

- Owner: `Figure` the one figure-composition owner discriminating operation over the closed `FigureOp` `expression.tagged_union` whose every case carries its own typed payload, never a `StrEnum` keyed against a shared erased `dict[str, object]`; `RenderPolicy` the one frozen `msgspec.Struct` that carries the full resvg sizing/parsing/font/policy/diagnostic axis as typed fields and projects them to the `svg_to_bytes` keyword set through one `asdict`-driven `kwargs(source)` spread rather than a hand-forwarded keyword wall, collapsing the rejected `RasterPolicy`/`FontPolicy` policy-pair into one row; `DrawOp` the closed-payload `tagged_union` collapsing the rejected `CaptionSpec`/`BoxSpec`/`PostSpec` draw triple into one pillow draw family — a `Text` case, a `Box` case, and a `Frame` case carrying the border/fit/exif/`FilterKind` finish axis — folded by one total `match` over the gated worker so a new annotation primitive is one case, never a parallel spec struct; `RasterSource` the closed-payload `tagged_union` collapsing `svg_string` markup and an `svg_path` `.svg`/`.svgz` file into one source case whose `keywords()` projects the live `svg_to_bytes` source keyword so a file source never grows a second render call; `MarkKind` the closed `Enum` of registration-overlay primitives (`CORNER` crop guide, `TICK` cut mark, `TARGET` color-bar dot, `REGISTRATION` concentric press target, `GUTTER` fold diamond, `MITER` corner chevron, `BLEED` trim box) whose every member carries its own catalogued-primitive name plus float-only arg row and resolves one `svgelements` shape through its bound `MarkKind.shape` builder so the overlay fold dispatches total over the member, never a `dict[MarkKind, Callable]` rebuilt per call against an erased shape bag; the `graphic/vector#VECTOR` primitive owns the `svgelements` `SVG` document working surface, the `Matrix`/`Path`/`Color`/`Length`/`Angle`/`Point`/`bbox` algebra the layout-and-overlay surface, and the `resvg_py.svg_to_bytes` in-process SVG-to-PNG raster floor on the core, while the `pillow` `Image` is the raster annotate/metadata surface on the gated floor — figure composition composes that primitive for every vector operation rather than re-owning the SVG geometry.
- Cases: `FigureOp` cases — `ScaleFit(source, width, height)` (resolve the source viewport, derive the `Matrix.scale` that fits a target `Length` box preserving aspect, re-emit the transformed SVG) · `Tile(sources, columns, cell)` (n-up sheet — place each source SVG into a row-major grid cell, each placement a `Matrix.translate`-after-`Matrix.scale` of the source bounds into the cell) · `Crop(source, x, y, width, height)` (the factory folds the four floats into one `Bounds` case payload, intersects each element `bbox` with the crop box, drops the out-of-bounds elements, re-emits the clipped SVG) · `Rotate(source, angle, corner)` (rotate-place the source by an `Angle`-resolved `Matrix.rotate` about a `bbox` corner pivot) · `Overlay(source, marks)` (registration marks, gutters, and crop guides — fold a `MarkSpec` row list, each `MarkSpec.kind` member building one `svgelements` shape through its own bound `MarkKind.shape` builder positioned by document `bbox` corner offsets, every fragment serialized through the one `_path` styled-egress owner carrying a `Color`-admitted `Style` stroke axis, all appended to the source `SVG` document) · `Annotate(source, render, draws)` (rasterize-then-draw — the `graphic/vector#VECTOR` `resvg_py.svg_to_bytes` floor rasterizes the `RasterSource` case on the core under the one `RenderPolicy`, then the gated band folds the `DrawOp` sequence by one total `match` — the `Text` arm runs `ImageDraw.text`, the `Box` arm `ImageDraw.rectangle`, the `Frame` arm the `ImageOps.exif_transpose`/`expand`/`fit` frame pass and the `FilterKind` `ImageFilter`/`ImageEnhance` finish, re-binding the draw surface after each image-replacing frame op) · `Metadata(source, exif, xmp)` (bind/read EXIF and XMP — `Image.getexif`/`Image.Exif` tag map plus the `info["XML:com.adobe.xmp"]` XMP packet, re-encoded on save) — matched by one total `match`/`case`; never a sibling op per source media type, never a parallel mark emitter per primitive, and never a parallel figure class.
- Entry: `Figure.of` is `async` over the runtime `async_boundary`, dispatches the `FigureOp` case, and returns a `RuntimeRail[ContentKey]`; the `ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay` vector arms render in-process composing the `graphic/vector#VECTOR` host-free geometry primitive (`svgelements` is pure-Python and `resvg_py` the cp315 native extension, both importing on the core), the `Annotate` arm rasterizes its `RasterSource` case through `resvg_py.svg_to_bytes` on the core then rides the gated-band process seam for the `pillow` draw/filter pass, the `Metadata` arm rides the process seam directly; both gated workers import `PIL` at boundary scope inside the subprocess function so no gated distribution touches the cp315-core page.
- Auto: `_emit` folds the op — the vector arms (`ScaleFit`/`Tile`/`Crop`/`Rotate`/`Overlay`) through `_compose_vector` which parses each source through `SVG.parse(..., reify=True)`, reads the document/element `bbox` through the `Element` protocol over `_elements`, composes the `Matrix` transform or folds the `MarkSpec` rows whose `kind` member builds its own bound `MarkKind.shape` `svgelements` shape and routes it through the one `_path` styled owner carrying its `Color`-admitted `Style` stroke axis, and serializes every fragment — base and overlay alike — through that one `_path` owner onto one `_svg` `<svg>` egress that wraps the `Path.d()` bodies in a fresh viewBox-sized document; the `Annotate` arm rasterizes inline through `resvg_py.svg_to_bytes(**render.kwargs(source))` at the one `_emit` dispatch site where `RenderPolicy.kwargs` merges the `RasterSource.keywords()` source key with the `asdict`-projected axis set (`width`/`height`/`zoom`/`dpi` sizing, `background`/`style_sheet`/`resources_dir`/`languages` parsing, `skip_system_fonts`/`font_size`/`font_files`/`font_dirs` and the six `*_family` generic-family rows, the `shape_rendering`/`text_rendering`/`image_rendering` `Literal` policy, and `log_information` diagnostics) — every empty-tuple sequence field coercing to `None` so the one spread replaces the hand-forwarded keyword wall — then hands PNG bytes to the gated-band worker where `Image.open`, `ImageDraw.Draw`, and the `DrawOp` fold's `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`expand`/`fit`, `ImageFilter.UnsharpMask`/`GaussianBlur`, and `ImageEnhance.Sharpness`/`Contrast` resolve at boundary scope; the `Metadata` arm runs the `Image.open`/`Image.getexif`/`Image.Exif`/XMP map directly on the gated band, folding the EXIF tag pairs through one `exif.update(...)`. The `_compose_vector`/`_path`/`_svg`/`_elements`/`_bounds`/`_px`/`_angle` geometry helpers are the same parse-transform-bounds-serialize surface `graphic/vector#VECTOR` owns; figure composition holds the placement-specific arm bodies (scale-fit math, n-up cell placement, crop intersection, rotate pivot, overlay fold) over that shared primitive surface rather than re-implementing the geometry.
- Receipt: each operation contributes `core/receipt#RECEIPT` `ArtifactReceipt.Preview` carrying the content key and the placed figure's pixel/viewport width and height; figure composition adds NO new receipt case — the placed-figure facts are width/height, the Preview shape.
- Packages: `svgelements` (`SVG.parse`/`SVG.elements`, `Path`/`Path.d`/`Path.bbox`, `Matrix`/`Matrix.scale`/`Matrix.translate`/`Matrix.rotate` composed by `*`, `Length.value`, `Angle`/`Angle.parse`, `Color`/`Color.hex`, `Point`, the `Rect`/`Circle`/`Ellipse`/`Polygon`/`Polyline`/`SimpleLine` primitives, pure-Python `py2.py3-none-any` v1.9.6 reflected on cp315, version via `SVGELEMENTS_VERSION`) on the cp315 core composed through `graphic/vector#VECTOR`, the overlay fold serializing every fragment through `Path.d()` onto one `_svg`-built `<svg>` document; `resvg-py` (`svg_to_bytes` SVG-to-PNG over the embedded Rust `resvg 0.47.0` engine — `svg_string`/`svg_path` source (`.svgz` decompresses on the path arm), `width`/`height`/`zoom`/`dpi` sizing, `background`/`style_sheet`/`resources_dir`/`languages` parsing, `skip_system_fonts`/`font_size`/`font_files`/`font_dirs`/`font_family`/`serif_family`/`sans_serif_family`/`cursive_family`/`fantasy_family`/`monospace_family` font, `shape_rendering`/`text_rendering`/`image_rendering` policy, `log_information` diagnostics, `__resvg_version__` engine tag, native `resvg_py.cpython-315-darwin.so` v0.3.3 reflected on cp315) on the cp315 core composed through `graphic/vector#VECTOR`; `pillow` (`Image.open`/`Image.getexif`/`Image.Exif`/`Image.Resampling`, `ImageDraw.Draw`/`text`/`rectangle`, `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`expand`/`fit`, `ImageFilter.UnsharpMask`/`GaussianBlur`, `ImageEnhance.Sharpness`/`Contrast`) gated `python_version<'3.15'`; `msgspec` (`Struct` frozen rows, `structs.asdict` projecting the `RenderPolicy` axis to the `svg_to_bytes` keyword set); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, the gated-band cross-version process lane).
- Growth: a new vector layout operation (gutter/margin policy, bleed sheet) is one `FigureOp` case plus one `_compose_vector` arm over the existing `graphic/vector#VECTOR` `Matrix`/`Angle`/`bbox` algebra — never a re-implemented SVG transform; a new registration-overlay primitive (slur mark, star target) is one `MarkKind` member carrying its own catalogued-primitive name and float-only arg row resolved through the shared `MarkKind.shape` builder, dispatched by the same total overlay fold over the `SVG` document append and serialized through the one `_path` styled owner — never a parallel mark emitter, never a `dict` arm grafted onto an erased shape bag; a new annotation source mode (a placed working document, a `.svgz` archive) is one `RasterSource` case projecting one entry into the `svg_to_bytes` source keyword on the one render call — never a second render entrypoint; a new raster annotation primitive (a legend swatch, an arrow) is one `DrawOp` case plus one `match` arm on the gated worker, and a new finish filter is one `FilterKind` row on the `Frame` arm — never a parallel spec struct, never a per-primitive draw loop; a new resvg sizing/font/policy/diagnostic knob is one field on the existing `RenderPolicy` row carried into the one `asdict` spread — never a second rasterizer; a new metadata channel (IPTC, ICC-profile name) is one tag read/write on the existing `Image` map. Zero new surface.
- Boundary: a per-graphic-type figure-composer class family, a per-primitive mark emitter, a per-input-mode render entrypoint, a `MarkKind`-keyed `dict[MarkKind, Callable]` shape table rebuilt per mark call, a per-`Corner` anchor `dict` literal rebuilt per call beside the total-`match` projection, a parallel base-versus-mark path-string emitter pair beside the one styled `_path` owner, a single-call `_rasterize` forwarding hop beside the inline `svg_to_bytes` dispatch, a parallel `RasterPolicy`/`FontPolicy` policy pair hand-forwarded as a 24-keyword `svg_to_bytes` wall, a parallel `CaptionSpec`/`BoxSpec`/`PostSpec` draw-spec triple swept by three `for` loops plus scattered `if post` branches, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live viewer, no chart re-render. The `graphic/vector#VECTOR` primitive owns SVG geometry/transform/parse/bounds/primitives/serialize and the `resvg_py` raster floor — figure composition COMPOSES that primitive: it reads `bbox` through the `Element` protocol, builds `SimpleLine`/`Circle`/`Ellipse`/`Polyline`/`Polygon`/`Rect` overlay shapes through the `MarkKind` member's own bound `MarkKind.shape` builder, transforms each through `Path(geometry) * Matrix`, and serializes every fragment — transformed base element and styled overlay mark alike — through the one `_path` styled-egress owner onto one `_svg`-built viewBox-sized `<svg>` document, never a second path-string emitter, a hand-rolled affine helper, a hand-emitted `<rect>`/`<line>` string, a re-parsed path string, or a re-owned SVG geometry engine beside `graphic/vector#VECTOR`. The `resvg_py` raster floor rasterizes the `RasterSource` case for the annotate pass over the one `svg_to_bytes` call whose `svg_string`/`svg_path` keywords carry markup and the `.svg`/`.svgz` file arm together; chart/mark/nanoplot rasterization-to-export routes to `visualization/chart#EXPORT` `vl-convert`, and the SVG sources arrive from the regrouped `visualization/chart#CHART`, `graphic/marks#MARK`, and `visualization/table#TABLE` siblings. The placed multi-source layout the vector arms emit is a flat single-`<svg>` document and that flat-SVG egress stays this owner's concern; the named-layer editable egress is `export/layered#LAYERED`'s — the `Figure.layers(names)` projection exposes the placed layout as `tuple[Layer, ...]` (one `Layer(name, source, bbox)` row per placed source carrying its placed `bbox`: one row per `Tile` cell, a `base`-plus-`overlay` pair per `Overlay`, one row per single-source placement) handed to that owner to bind into named `drawsvg` groups and PDF OCG layers, so figure composition authors no named-layer structure and grows no per-layer emit arm — it projects the placed-source rows and routes the layer authoring outward; a layered-export arm grafted onto `FigureOp` and a hierarchical-group SVG emitter beside the one flat `_svg` document are the deleted forms. `pillow` annotate/metadata rides the gated `python_version<'3.15'` band and never resolves in the cp315-core process, so the `pillow` pass dispatches onto the runtime gated-band process lane where the subprocess worker imports `PIL` at boundary scope inside the function — neither a module-top nor a core-page import lands, while `resvg_py` likewise imports at boundary scope on the core. ICC profile attachment and color management stay in `graphic/color/managed#MANAGED`; figure composition consumes a color-managed raster, it does not build the transform.

```python signature
from collections.abc import Callable, Iterable, Sequence
from enum import Enum
from io import BytesIO
from typing import TYPE_CHECKING, Literal, Protocol, assert_never

from anyio import to_process
from expression import case, tag, tagged_union
from msgspec import Struct
from msgspec.structs import asdict

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.export.layered import Layer

if TYPE_CHECKING:
    from svgelements import SVG, Angle, Matrix


type Bounds = tuple[float, float, float, float]
type Corner = Literal["nw", "ne", "sw", "se", "center"]
type Length = str | float
type Anchor = tuple[float, float]
type Style = tuple[str, float] | None
type MarkArgs = Callable[[float, float, float], tuple[float, ...]]
type ShapeRendering = Literal["optimize_speed", "crisp_edges", "geometric_precision"]
type TextRendering = Literal["optimize_speed", "optimize_legibility", "geometric_precision"]
type ImageRendering = Literal["optimize_quality", "optimize_speed"]
type FilterKind = Literal["unsharp", "blur", "sharpness", "contrast"]


class Element(Protocol):
    def bbox(self) -> Bounds | None: ...


class MarkKind(Enum):
    CORNER = ("Polyline", lambda x, y, s: (x, y - s, x, y, x + s, y))
    TICK = ("SimpleLine", lambda x, y, s: (x - s, y, x + s, y))
    TARGET = ("Circle", lambda x, y, s: (x, y, s))
    REGISTRATION = ("Ellipse", lambda x, y, s: (x, y, s, s * 0.5))
    GUTTER = ("Polyline", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s, x - s, y))
    MITER = ("Polygon", lambda x, y, s: (x - s, y, x, y - s, x + s, y, x, y + s))
    BLEED = ("Rect", lambda x, y, s: (x - s, y - s, 2.0 * s, 2.0 * s))

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


class RenderPolicy(Struct, frozen=True):
    width: int | None = None
    height: int | None = None
    zoom: float | None = None
    dpi: float = 0.0
    background: str | None = None
    style_sheet: str | None = None
    resources_dir: str | None = None
    languages: tuple[str, ...] = ()
    skip_system_fonts: bool = False
    font_size: float = 16.0
    font_files: tuple[str, ...] = ()
    font_dirs: tuple[str, ...] = ()
    font_family: str | None = None
    serif_family: str | None = None
    sans_serif_family: str | None = None
    cursive_family: str | None = None
    fantasy_family: str | None = None
    monospace_family: str | None = None
    shape_rendering: ShapeRendering = "geometric_precision"
    text_rendering: TextRendering = "optimize_legibility"
    image_rendering: ImageRendering = "optimize_quality"
    log_information: bool = False

    def kwargs(self, source: RasterSource) -> dict[str, object]:
        return {**source.keywords(), **{key: list(value) or None if isinstance(value, tuple) else value for key, value in asdict(self).items()}}


@tagged_union(frozen=True)
class DrawOp:
    tag: Literal["text", "box", "frame"] = tag()
    text: tuple[str, Anchor, str | None, float, str, str | None] = case()
    box: tuple[Bounds, str, str | None, int] = case()
    frame: tuple[FilterKind | None, float, int, str, tuple[int, int] | None, bool] = case()

    @staticmethod
    def Text(text: str, xy: Anchor, fill: str = "black", font: str | None = None, size: float = 16.0, anchor: str | None = None) -> "DrawOp":
        return DrawOp(text=(text, xy, font, size, fill, anchor))

    @staticmethod
    def Box(box: Bounds, outline: str = "black", fill: str | None = None, width: int = 1) -> "DrawOp":
        return DrawOp(box=(box, outline, fill, width))

    @staticmethod
    def Frame(filter: FilterKind | None = None, radius: float = 2.0, border: int = 0, border_fill: str = "white", fit: tuple[int, int] | None = None, exif_orient: bool = False) -> "DrawOp":
        return DrawOp(frame=(filter, radius, border, border_fill, fit, exif_orient))


@tagged_union(frozen=True)
class FigureOp:
    tag: Literal["scale_fit", "tile", "crop", "rotate", "overlay", "annotate", "metadata"] = tag()
    scale_fit: tuple[bytes, Length, Length] = case()
    tile: tuple[tuple[bytes, ...], int, Length, Length] = case()
    crop: tuple[bytes, Bounds] = case()
    rotate: tuple[bytes, str, Corner] = case()
    overlay: tuple[bytes, tuple[MarkSpec, ...]] = case()
    annotate: tuple[RasterSource, RenderPolicy, tuple[DrawOp, ...]] = case()
    metadata: tuple[bytes, tuple[tuple[int, str], ...], str | None] = case()

    @staticmethod
    def ScaleFit(source: bytes, width: Length, height: Length) -> "FigureOp":
        return FigureOp(scale_fit=(source, width, height))

    @staticmethod
    def Tile(sources: tuple[bytes, ...], columns: int, cell_width: Length, cell_height: Length) -> "FigureOp":
        return FigureOp(tile=(sources, columns, cell_width, cell_height))

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
    def Annotate(source: RasterSource, render: RenderPolicy = RenderPolicy(), draws: tuple[DrawOp, ...] = ()) -> "FigureOp":
        return FigureOp(annotate=(source, render, draws))

    @staticmethod
    def Metadata(source: bytes, exif: tuple[tuple[int, str], ...] = (), xmp: str | None = None) -> "FigureOp":
        return FigureOp(metadata=(source, exif, xmp))


class Figure(Struct, frozen=True):
    op: FigureOp

    async def of(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"figure.{self.op.tag}", self._emit)

    async def _emit(self) -> ContentKey:
        match self.op:
            case FigureOp(tag="annotate", annotate=(source, render, draws)):
                import resvg_py

                data = await to_process.run_sync(_gated_annotate, resvg_py.svg_to_bytes(**render.kwargs(source)), draws)
            case FigureOp(tag="metadata", metadata=(source, exif, xmp)):
                data = await to_process.run_sync(_gated_metadata, source, exif, xmp)
            case _:
                data = _compose_vector(self.op)
        return ContentIdentity.of(f"figure-{self.op.tag}", data)

    def layers(self, names: tuple[str, ...] = ()) -> tuple[Layer, ...]:
        return _placed_layers(self.op, names)


def _placed_layers(op: FigureOp, names: tuple[str, ...]) -> tuple[Layer, ...]:
    from svgelements import SVG, Matrix

    match op:
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height)):
            cw, ch = _px(cell_width), _px(cell_height)
            placed = []
            for index, raw in enumerate(sources):
                document = SVG.parse(BytesIO(raw), reify=True)
                column, row = index % columns, index // columns
                box = (column * cw, row * ch, (column + 1) * cw, (row + 1) * ch)
                fragment = _svg(_place(document, index, columns, cw, ch), cw * columns, ch * -(-len(sources) // columns))
                placed.append(Layer(_name(names, index), fragment, box))
            return tuple(placed)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            document = SVG.parse(BytesIO(source), reify=True)
            xmin, ymin, xmax, ymax = bounds = _bounds(document)
            base = Layer(_name(names, 0, "base"), _svg(_transform(document, Matrix()), xmax - xmin, ymax - ymin), bounds)
            overlay = (_path(spec.kind.shape(_anchor(spec.corner, bounds, spec.inset), spec.size), Matrix(), (spec.stroke, spec.width)) for spec in marks)
            return (base, Layer(_name(names, 1, "overlay"), _svg(overlay, xmax - xmin, ymax - ymin), bounds))
        case FigureOp(tag="scale_fit", scale_fit=(source, *_)) | FigureOp(tag="crop", crop=(source, *_)) | FigureOp(tag="rotate", rotate=(source, *_)):
            bounds = _bounds(SVG.parse(BytesIO(source), reify=True))
            return (Layer(_name(names, 0), _compose_vector(op), bounds),)
        case _:
            assert_never(op)


def _name(names: tuple[str, ...], index: int, fallback: str = "") -> str:
    return names[index] if index < len(names) else fallback or f"layer-{index}"


def _compose_vector(op: FigureOp) -> bytes:
    from svgelements import SVG, Matrix

    match op:
        case FigureOp(tag="scale_fit", scale_fit=(source, width, height)):
            document = SVG.parse(BytesIO(source), reify=True)
            (xmin, ymin, xmax, ymax), (tw, th) = _bounds(document), (_px(width), _px(height))
            factor = min(tw / (xmax - xmin), th / (ymax - ymin))
            return _svg(_transform(document, Matrix.translate(-xmin, -ymin) * Matrix.scale(factor)), tw, th)
        case FigureOp(tag="tile", tile=(sources, columns, cell_width, cell_height)):
            cw, ch = _px(cell_width), _px(cell_height)
            documents = [SVG.parse(BytesIO(source), reify=True) for source in sources]
            placed = [path for index, document in enumerate(documents) for path in _place(document, index, columns, cw, ch)]
            return _svg(placed, cw * columns, ch * -(-len(sources) // columns))
        case FigureOp(tag="crop", crop=(source, box)):
            document = SVG.parse(BytesIO(source), reify=True)
            x0, y0, x1, y1 = box
            kept = (_path(element, Matrix.translate(-x0, -y0)) for element in _elements(document) if _hits(element.bbox(), box))
            return _svg(kept, x1 - x0, y1 - y0)
        case FigureOp(tag="rotate", rotate=(source, angle, corner)):
            document = SVG.parse(BytesIO(source), reify=True)
            xmin, ymin, xmax, ymax = bounds = _bounds(document)
            px, py = _anchor(corner, bounds, 0.0)
            pivot = Matrix.translate(px, py) * Matrix.rotate(_angle(angle)) * Matrix.translate(-px, -py)
            return _svg(_transform(document, pivot), xmax - xmin, ymax - ymin)
        case FigureOp(tag="overlay", overlay=(source, marks)):
            document = SVG.parse(BytesIO(source), reify=True)
            xmin, ymin, xmax, ymax = bounds = _bounds(document)
            base = _transform(document, Matrix())
            overlays = (_path(spec.kind.shape(_anchor(spec.corner, bounds, spec.inset), spec.size), Matrix(), (spec.stroke, spec.width)) for spec in marks)
            return _svg([*base, *overlays], xmax - xmin, ymax - ymin)
        case _:
            assert_never(op)


def _anchor(corner: Corner, bounds: Bounds, inset: float) -> Anchor:
    xmin, ymin, xmax, ymax = bounds
    match corner:
        case "nw":
            return (xmin + inset, ymin + inset)
        case "ne":
            return (xmax - inset, ymin + inset)
        case "sw":
            return (xmin + inset, ymax - inset)
        case "se":
            return (xmax - inset, ymax - inset)
        case "center":
            return ((xmin + xmax) / 2.0, (ymin + ymax) / 2.0)
        case _:
            assert_never(corner)


def _elements(document: "SVG") -> list[Element]:
    return [element for element in document.elements() if hasattr(element, "bbox")]


def _bounds(document: "SVG") -> Bounds:
    boxes = [box for element in _elements(document) if (box := element.bbox()) is not None]
    return (min(b[0] for b in boxes), min(b[1] for b in boxes), max(b[2] for b in boxes), max(b[3] for b in boxes))


def _path(geometry: object, transform: "Matrix", style: Style = None) -> str:
    from svgelements import Color, Path

    body = (Path(geometry) * transform).d()
    stroke = "" if style is None else f' fill="none" stroke="{Color(style[0]).hex}" stroke-width="{style[1]}"'
    return f'<path d="{body}"{stroke}/>'


def _transform(document: "SVG", transform: "Matrix") -> list[str]:
    return [_path(element, transform) for element in _elements(document)]


def _place(document: "SVG", index: int, columns: int, cell_w: float, cell_h: float) -> list[str]:
    from svgelements import Matrix

    xmin, ymin, xmax, ymax = _bounds(document)
    factor = min(cell_w / (xmax - xmin), cell_h / (ymax - ymin))
    column, row = index % columns, index // columns
    return _transform(document, Matrix.translate(column * cell_w - xmin * factor, row * cell_h - ymin * factor) * Matrix.scale(factor))


def _px(length: Length) -> float:
    from svgelements import Length as SvgLength

    return SvgLength(length).value(ppi=96.0)


def _angle(angle: str) -> "Angle":
    from svgelements import Angle

    return Angle.parse(angle)


def _hits(bounds: Bounds | None, box: Bounds) -> bool:
    return bounds is not None and not (bounds[2] < box[0] or bounds[0] > box[2] or bounds[3] < box[1] or bounds[1] > box[3])


def _svg(fragments: Iterable[str], width: float, height: float) -> bytes:
    body = "".join(fragments)
    return f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">{body}</svg>'.encode()


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
            case DrawOp(tag="frame", frame=(kind, radius, border, border_fill, fit, exif_orient)):
                image = ImageOps.exif_transpose(image) if exif_orient else image
                image = ImageOps.expand(image, border=border, fill=border_fill) if border else image
                image = ImageOps.fit(image, fit, method=Image.Resampling.LANCZOS) if fit is not None else image
                match kind:
                    case "unsharp":
                        image = image.filter(ImageFilter.UnsharpMask(radius=radius))
                    case "blur":
                        image = image.filter(ImageFilter.GaussianBlur(radius=radius))
                    case "sharpness":
                        image = ImageEnhance.Sharpness(image).enhance(radius)
                    case "contrast":
                        image = ImageEnhance.Contrast(image).enhance(radius)
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
    image.info["XML:com.adobe.xmp"] = xmp if xmp is not None else image.info.get("XML:com.adobe.xmp")
    sink = BytesIO()
    image.save(sink, format=image.format or "PNG", exif=exif, xmp=xmp.encode() if xmp is not None else None)
    return sink.getvalue()
```

## [03]-[RESEARCH]

- [VECTOR_COMPOSED] [RESOLVED]: the SVG-geometry primitive — `SVG.parse(source, reify=True)`/`SVG.elements`, `Path(geometry)`/`Path.d`/`Path.bbox`, `Matrix.scale`/`Matrix.translate`/`Matrix.rotate` (composed by `*`), `Length(value).value(ppi=...)`, `Angle.parse`, and the `Color(value)` color-admission value object — is OWNED by `graphic/vector#VECTOR` (a VERIFIED REAL `svgelements` reflection, `1.9.6` on the cp315 core, pure-Python `py2.py3-none-any`, version via `SVGELEMENTS_VERSION`), and figure composition COMPOSES it. The `ScaleFit`/`Tile`/`Crop`/`Rotate` arms are SETTLED placement code over that primitive: `reify=True` resolves transforms into element geometry so the `bbox()` read returns absolute coordinates, each placement composes through one `Matrix` (`translate`-after-`scale`) never a hand-rolled affine, and every fragment serializes through `Path(geometry).d()`. The `OVERLAY` arm's `Matrix`/`Path`/`Color`/`bbox` algebra and corner-anchored placement are settled on the same primitive surface; only the shape-primitive positional constructor grammar that `MarkKind.shape` resolves is gated by `[SHAPE_CTOR_RESEARCH]` (the one deepen item this owner shares with the `graphic/vector#VECTOR` `_path` cite-point). The `MarkKind` `Enum` carries the catalogued primitive name and the float-only arg-builder on each member, so the overlay fold dispatches behavior through the member's own `kind.primitive`/`kind.args` row — the row-owned-behavior collapse of the rejected `dict[MarkKind, Callable]` factory rebuilt per mark call — and `MarkKind.shape` resolves the `svgelements` class once at boundary scope through `getattr(svgelements, self.primitive)`, never a module-scope `svgelements` reference. Base elements and overlay marks both serialize through the one `_path(geometry, transform, style)` styled-egress owner: the optional `Style` tuple admits the overlay `stroke` literal through the catalogued `Color(value)` parse (an invalid SVG/CSS color rejects at admission) and emits the `Color(value).hex` channel literal — `.hex` is the catalogued `Color` accessor — while a `None` style emits the bare `<path d="..."/>` base fragment, so a single path-string owner replaces the rejected parallel `_mark`/`_path` emitters and the rejected hand-emitted `<rect>`/`<line>` string. The iterated element rides the local `Element` `Protocol` declaring the one `bbox()` method the fence touches, never an erased `object` and never an uncatalogued `svgelements` base type; `_elements` narrows the `SVG.elements()` sweep by `hasattr(element, "bbox")`. The placement geometry helpers (`_compose_vector` placement bodies, `_path`/`_svg`/`_elements`/`_bounds`/`_px`/`_angle`) mirror the `graphic/vector#VECTOR` `Parse`/`Bounds`/`Transform`/`Serialize`/`Rasterize` surface; the split is the disciplined collapse boundary — the geometry primitive lands once in `graphic/vector#VECTOR`, the placement logic (scale-fit/n-up/crop/rotate/overlay) lands once here over it, and neither re-owns the other.
- [SHAPE_CTOR_RESEARCH]: the seven `MarkKind` members resolve their `svgelements` shape through `getattr(svgelements, self.primitive)(*self.args(*anchor, size))`, positionally constructing `SimpleLine`/`Circle`/`Ellipse`/`Polyline`/`Polygon`/`Rect`. The folder `.api` catalogue for `svgelements` rows each primitive by role (`Circle` "circle by center and radius", `Ellipse` "ellipse by center and two radii", `Rect` "axis-aligned rectangle", `Polygon`/`Polyline` "point sequence", `SimpleLine` "single line segment") but does NOT catalogue the positional constructor arity or argument order of any shape — the `Circle(cx, cy, r)`, `Ellipse(cx, cy, rx, ry)`, `Rect(x, y, w, h)`, `SimpleLine(x1, y1, x2, y2)`, and `*points`-spread `Polygon`/`Polyline` call grammar is a RESEARCH item, never settled fence code, until the catalogue reflects each shape's constructor signature. The isolation onto the one `MarkKind.shape` builder keeps every unverified construction spelling at a single cite-point so the `SCALE_FIT`/`TILE`/`CROP` placement arms and the `OVERLAY` algebra stay fully settled; this deepen item is shared between this owner's `MarkKind.shape` builder and the `graphic/vector#VECTOR` `_path` cite-point. Close-condition: the catalogue reflects each shape's positional constructor signature — resolved verification either confirms the positional grammar or rebinds each `args` row onto the catalogued keyword constructor.
- [ROTATE_PARSE_SETTLED]: the `ROTATE` arm resolves the angle string through `_angle`, the one boundary-scoped helper holding `svgelements.Angle.parse(angle)`. The folder `.api` catalogue for `svgelements` confirms the `Angle` value object (CSS `deg`/`rad`/`grad`/`turn`), `Matrix.rotate(angle)`, and the value-object row naming `Angle.parse(value)` as the string-admission classmethod — so `Angle.parse` is catalogue-confirmed settled fence code (resolving the prior `[ROTATE_PARSE_RESEARCH]` gate the `graphic/vector#VECTOR` page closed), and the `Rotate` arm's `Matrix.rotate(Angle.parse(...))` composition is settled.
- [RASTER_FLOOR_COMPOSED]: the `ANNOTATE` arm rasterizes its `RasterSource` case through `resvg_py.svg_to_bytes(**render.kwargs(source))` on the cp315 core before the gated `pillow` draw/filter pass — the same `graphic/vector#VECTOR` `Rasterize` floor every placement consumer composes; `import resvg_py` resolves at boundary scope inline at the one `_emit` annotate dispatch site, never module-top and never behind a single-call forwarding helper. The `RasterSource` `tagged_union` collapses the `svg_string` markup arm and the `svg_path` `.svg`/`.svgz` file arm onto the one `svg_to_bytes` call — `keywords()` resolves the case to a single-entry `{"svg_string": ...}` or `{"svg_path": ...}` source dict through the page-settled statement-form `match`/`case` over the `tag` discriminant (the catalogue documents `tagged_union` with no instance `.match()` method, so the fence dispatches by statement), so a file source never grows a second render entrypoint. `RenderPolicy.kwargs(source)` merges that source dict with the `msgspec.structs.asdict(self)` field projection — `asdict` verifies against the branch `.api` catalogue for `msgspec` as the `structs.asdict(struct)` row — coercing each `()`-default tuple field to `list(value) or None` so `languages`/`font_files`/`font_dirs` arrive as the catalogue's `list[str] | None` shape and the one spread replaces the rejected 24-keyword hand-forward. Every `svg_to_bytes` keyword the spread carries verifies against the folder `.api` catalogue for `resvg-py` (`0.3.3` reflected on cp315, native `resvg_py.cpython-315-darwin.so` embedding the Rust `resvg 0.47.0` engine): the catalogue source/sizing/parsing/font/policy/logging axis table is the settled signature, the `RenderPolicy` field names match the catalogued keyword names one-for-one so the `asdict` spread is total over the signature, `svg_string` and `svg_path` are the one required-one source row pair (`.svgz` decompresses on the path arm), the `dpi=0.0` default defers to the SVG-declared size, the `log_information` boolean prints resvg debug logs as the diagnostics row, and the `shape_rendering`/`text_rendering`/`image_rendering` `Literal` policy defaults (`geometric_precision`/`optimize_legibility`/`optimize_quality`) match the reflected stub. `svg_to_bytes` returns PNG `bytes` and raises `ValueError` on empty or invalid SVG, an unparseable `background`, or render failure. The `graphic/vector#VECTOR` primitive owns SVG-to-PNG with no Cairo, headless-browser, or external-process dependency — figure composition never re-implements SVG path flattening, text shaping, or PNG encoding the resvg engine already owns, and the chart/mark/nanoplot rasterization-to-export floor stays in `visualization/chart#EXPORT` `vl-convert`. All resvg-py members are catalogue-confirmed settled fence code; this page carries no resvg-py RESEARCH gate.
- [RASTER_SETTLED]: the gated `pillow` pass for `ANNOTATE`/`METADATA` runs on the `python_version<'3.15'` band through `anyio.to_process.run_sync`, importing `PIL` at boundary scope inside the gated-band worker function, never on the cp315-core owner; the `ANNOTATE` worker receives the resvg-rasterized PNG and decodes it through `Image.open`, the `METADATA` worker the already-emitted figure raster. The `Image.open`/`Image.getexif`/`Image.Exif`/`Image.save`/`Image.Resampling`, `ImageDraw.Draw`/`text`/`rectangle`, `ImageFont.truetype`/`load_default`, `ImageOps.exif_transpose`/`fit`, `ImageFilter.UnsharpMask`/`GaussianBlur`, and `ImageEnhance.Sharpness`/`Contrast` spellings verify against the folder `.api` catalogue for `pillow` (`12.2.0` reflected on the gated cp313 band): `ImageDraw.ImageDraw.text`/`rectangle` are the catalogued draw rows, `ImageFont.truetype`/`load_default` the font rows, `ImageOps.exif_transpose`/`ImageOps.fit` the catalogued operation rows, `ImageFilter.GaussianBlur`/`UnsharpMask` the filter factory rows, `ImageEnhance.Contrast`/`Sharpness` the catalogued enhancement rows, `Image.Resampling.LANCZOS` the catalogued resample enum case, and `Image.Exif`/`Image.getexif` the mutable EXIF mapping over which the `METADATA` arm folds the tag pairs through one `exif.update(...)`. `ImageOps.expand` is a member of the catalogued `ImageOps` operation-module surface (verified by module, the catalogue naming `ImageOps` as the pure-function operation surface) rather than an individually-rowed entry, never a hand-rolled border pass. The XMP packet rides `image.info["XML:com.adobe.xmp"]` and the `save(..., xmp=...)` kwarg. The figure rail uses no `ImageCms` transform spelling (ICC color management is `graphic/color/managed#MANAGED`, which holds the `ImageCms` build/apply surface as its own [RESEARCH] seam), so this page carries no pillow RESEARCH gate.
- [LAYERED_EGRESS] [RESOLVED]: the placed multi-source layout the `_compose_vector` vector arms emit is one flat single-`<svg>` document built by the one `_svg` egress that wraps every `Path.d()` body in a fresh viewBox-sized `<svg>` — that flat-SVG egress is figure composition's settled concern and adds no hierarchical-group or named-layer structure. The editable named-layer egress for an Illustrator/InDesign hand-off is `export/layered#LAYERED`'s owner: the `Figure.layers(names)` projection exposes the placed layout as `tuple[Layer, ...]`, ONE `export/layered#LAYERED` `Layer(name, source, bbox, visible=True, locked=False)` row per placed source carrying its placed `bbox` — the `Tile` arm yields one row per grid cell carrying the cell's `(column*cw, row*ch, (column+1)*cw, (row+1)*ch)` placed extent through `_placed_layers`/`_place`, the `Overlay` arm yields a `base` row plus a registration-`overlay` row over the same document bounds, and the single-source `ScaleFit`/`Crop`/`Rotate` arms yield one row over the placed document bounds — so the base-graphic/registration-overlay/n-up-sheet layout hands outward as named rows keyed by the same `ContentKey`, which `export/layered#LAYERED` binds each into a named `drawsvg` `Group` (the catalogued `Drawing`/`Group`/`as_svg` named-group SVG-authoring surface, folder `.api/drawsvg.md`) and a PDF OCG optional-content layer rather than the one flattened path soup `_svg` emits. The split is the disciplined collapse boundary: a layered-export arm grafted onto `FigureOp`, a hierarchical named-`<g>` group emitter beside the one flat `_svg` document, and a per-layer SVG/PDF writer on this owner are the rejected forms — `Figure.layers` projects the placed-source-plus-`bbox` rows without authoring layer structure, the named-layer authoring is one outward seam to the `export/layered` sub-domain, so figure composition stays the flat post-render placement owner emitting the flat document and the `tuple[Layer, ...]` projection, and the editable-export concern lands once in its own owner consumed by every visual producer. The `drawsvg` `Group(id=...)` named-group and pikepdf OCG `/OCProperties`/`/OCGs` member spellings are the `export/layered` page's verification burden, not this page's fence; this row carries the seam direction, the `Figure.layers` projection shape, and the flat-vs-named boundary, the export owner carries the layer-authoring fence.
- [HANDOFF_GUARD] [BLOCKED]: the outward figure edge to a sibling package travels only as the `compute/graduation` `HandoffAxis` model-asset case keyed by `ContentIdentity`, never a private per-artifact handoff, and the figure source re-mints no canonical concept so the `runtime/evidence` `Structural.drift` query stays clean. `Figure._emit` returns `ContentIdentity.of(...)` (re-minting no content-identity seed) and contributes the existing `ArtifactReceipt.Preview` case (re-minting no receipt rail), so the artifacts side is verification-and-alignment only. Close-condition: the upstream `compute/graduation` `HandoffAxis` model-asset case and the `runtime/evidence` `Structural.drift` detector land; the figure rail threads the same `ContentKey` into the one outward handoff with zero new surface.
