# [PY_ARTIFACTS_SHAPE]

The text-shaping and color-glyph rasterization owner over the document rail. `Shaping` is ONE owner that takes a Unicode text run and a font and folds it through a closed `ShapeOp` family into a positioned glyph run and its rendered color glyphs: uharfbuzz shapes the run (`Face` -> `Font` -> `Buffer` -> `shape`) into a `PositionedGlyphRun` carrying per-glyph GID / cluster / advance / offset plus the `SVGPathPen` outline bridge, a python-bidi UAX#9 reorder pass resolves mixed-direction logical-to-visual order before shaping on the gated band, and blackrenderer folds each glyph of the shaped run through its in-package COLRv1/COLRv0 `drawGlyph` paint-graph traversal onto a backend `Surface` and serializes to PNG/PDF/SVG. uharfbuzz, fonttools, blackrenderer, and python-bidi are admitted in the manifest; the `PositionedGlyphRun` is the one value object the shaped-run arm carries, consumed by `documents/emit#DOCUMENT` text placement and `figures/compose#COMPOSE` annotation, and the face selection and variation location are read from `typography/font#FONT`. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Document`/`.Preview`.

## [01]-[INDEX]

- [01]-[SHAPE]: uharfbuzz OpenType shaping, python-bidi UAX#9 reorder, and blackrenderer COLRv1 color-glyph rasterization owner over the closed `ShapeOp` step table; `PositionedGlyphRun` is the shaped-run value object the shape arm carries and the rasterize arm reshapes, `RasterBackend` the `getSurfaceClass` backend-registry policy, the Bidi arm gated `python_version<'3.15'` on the runtime subprocess band.

## [02]-[SHAPE]

- Owner: `Shaping` the one shaping-and-glyph-render owner discriminating the shaping step; `ShapeOp` the closed `StrEnum` over uharfbuzz shaping, python-bidi UAX#9 reordering, and blackrenderer COLRv1 rasterization; one frozen `_SHAPE_TABLE` `MappingProxyType` data-row dispatch maps each step to its `ShapeAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. uharfbuzz owns the OpenType layout engine, the `ot_layout_*`/`axis_infos` live-table introspection, and the cluster/feature/variation shaping surface; fonttools owns the binary font model and the `SVGPathPen` outline pen; blackrenderer owns the COLRv1 paint-graph rasterizer over fonttools+HarfBuzz; python-bidi owns the UAX#9 bidirectional reorder. `RasterBackend` is the policy-as-value `StrEnum` row whose member value is the `getSurfaceClass` registry key; `PositionedGlyphRun` is the carried shaped-run value object the `SHAPE` arm produces and the `RASTERIZE` arm reshapes.
- Cases: `ShapeOp` rows `SHAPE` (uharfbuzz `shape` over the `Face.create` -> `Font.create` -> `Buffer.create`/`add_str`/`guess_segment_properties` pipeline returning a `PositionedGlyphRun`, with `Font.set_variations` axis pinning and the `Font.draw_glyph_with_pen(gid, SVGPathPen(glyphSet))` -> `SVGPathPen.getCommands()` outline bridge driving `PositionedGlyphRun.to_svg_path`; `Buffer.direction`/`script`/`language` set explicitly or inferred by `guess_segment_properties`, and `GlyphInfo.flags & GlyphFlags.UNSAFE_TO_BREAK` the line-break safety signal a layout consumer reads) · `BIDI` (python-bidi UAX#9 mixed-direction logical-to-visual reorder over `bidi.get_display(text, base_dir=..., debug=False)` resolving the visual character order before shaping, gated `python_version<'3.15'` on the runtime subprocess band — the Rust pyo3 binding lacks a CPython 3.15 wheel — returning the reordered text the `SHAPE` arm then shapes) · `RASTERIZE` (blackrenderer per-glyph COLRv1/COLRv0 paint-graph traversal — `getSurfaceClass` resolves the backend `Surface`, `BlackRendererFont(ttFont=..., hbFont=...)` loads the paired font without a path round-trip, `setLocation`/`getPalette` instance the variable-font location and resolved palette, `buildGlyphLine`/`calcGlyphLineBounds` shape the HarfBuzz run and compute font-unit bounds, and `drawGlyph` folds each glyph onto the `Surface.canvas` under a `savedState`/`transform` offset-and-advance walk over each `GlyphInfo.xOffset`/`yOffset`/`xAdvance`/`yAdvance`) — selected by the frozen `_SHAPE_TABLE` row, never a chain of `is`-probes; the raster backend (`svg`/`skia`/`cairo`/`coregraphics`) is the `RasterBackend` row keyed on the `getSurfaceClass` registry, the variation location a `params` axis map, and the feature set the catalogued `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` feature-range shape.
- Entry: `Shaping.run` dispatches the step over the input text and font through the one `_SHAPE_TABLE[step]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; shaping produces a positioned-glyph run and its real outline, the Bidi reorder resolves mixed-direction visual order, rasterizing renders color glyphs glyph-by-glyph. `SHAPE` and `RASTERIZE` resolve synchronously over the cp315-core uharfbuzz/fonttools/blackrenderer wheels — `SHAPE` over the uharfbuzz Cython binding, `RASTERIZE` driving blackrenderer at boundary-scope import only (skia/cairo/coregraphics load native libraries on backend resolution, the pure-Python SVG backend needs none); the `BIDI` arm crosses `anyio.to_process.run_sync` onto the gated `python_version<'3.15'` subprocess band where the gated-band worker imports `bidi` at module scope, so the cp315-core process imports no gated distribution.
- Auto: shaping folds the text run through `Face.create(font_bytes, face_index)` -> `Font.create(face)` -> `Buffer.create()`/`add_str(text)`/`guess_segment_properties()` -> `shape(font, buffer, features)` then reads `glyph_infos`/`glyph_positions` (zipped strict) into a `PositionedGlyphRun`, with `Font.set_variations(variations)` axis pinning and the `Font.draw_glyph_with_pen(gid, SVGPathPen(TTFont(...).getGlyphSet()))` -> `SVGPathPen.getCommands()` outline bridge feeding `to_svg_path`; the Bidi reorder folds the logical text through `bidi.get_display(text, base_dir=base_direction)` on the gated worker into the visual-order text; rasterizing resolves the `getSurfaceClass(backendName)` surface (a `None` return raises `BackendUnavailableError`), loads one `BlackRendererFont(ttFont=TTFont(io.BytesIO(font), fontNumber=0, lazy=True), hbFont=...)`, applies `setLocation(location)` and mirrors it onto the shaping font via `hb_font.set_variations` for COLRv1 `PaintVar*`-versus-advance agreement, reads `getPalette(palette_index)`, shapes the run into `buildGlyphLine(glyph_infos, glyph_positions, glyphNames)`, computes `calcGlyphLineBounds(glyph_line, font)`, derives the `font_size`/`unitsPerEm` pixel scale, opens one `Surface.canvas(bounds)` margin-inset context concatenated with that scale, and folds each shaped glyph through `drawGlyph(glyph.name, canvas, palette=...)` inside a `savedState`/`transform` offset-and-advance walk over `glyph.xOffset`/`yOffset`/`xAdvance`/`yAdvance` before `Surface.saveImage`.
- Receipt: every arm projects its output onto the shared `receipt/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `SHAPE` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `PositionedGlyphRun` byte count, the `BIDI` arm contributes `ArtifactReceipt.Document` carrying the content key and the reordered-text byte count, and the `RASTERIZE` arm contributes `ArtifactReceipt.Preview` carrying the content key and the pixel width/height. The COLR version, resolved palette index, backend name, glyph count, and pixel bounds the `RASTERIZE` arm computes stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Document`/`Preview` cases cannot carry.
- Packages: `uharfbuzz` (settled: `Face.create`/`Font.create`/`Buffer.create`/`add_str`/`guess_segment_properties`/`shape`/`glyph_infos`/`glyph_positions`/`GlyphInfo.codepoint`/`cluster`/`flags`/`GlyphPosition.x_advance`/`y_advance`/`x_offset`/`y_offset`/`GlyphFlags.UNSAFE_TO_BREAK`/`Font.set_variations`/`Font.draw_glyph_with_pen`/`Buffer.direction`/`script`/`language` all rowed in the folder `.api` catalogue), `fonttools` (settled: `ttLib.TTFont`/`TTFont.getGlyphSet`/`pens.svgPathPen.SVGPathPen`/`SVGPathPen.getCommands`), `blackrenderer` (settled: `font.BlackRendererFont(ttFont=, hbFont=)`/`drawGlyph`/`setLocation`/`getPalette`/`glyphNames`/`unitsPerEm`/`render.buildGlyphLine`/`render.calcGlyphLineBounds`/`render.BackendUnavailableError`/`render.GlyphInfo(name, gid, xAdvance, yAdvance, xOffset, yOffset)`/`backends.getSurfaceClass`/`Surface.canvas`/`Surface.saveImage`/`Canvas.savedState`/`Canvas.transform`), `python-bidi` (RESEARCH: `bidi.get_display(text, base_dir=, debug=)` and `bidi.algorithm.get_display`/`get_base_level` — gated `python_version<'3.15'`, no cp315 wheel and no folder `.api` catalogue yet, so the member spellings stay unverified until the gated-band reflection lands); runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`/`async_boundary`, `anyio.to_process.run_sync` the runtime subprocess lane for the gated Bidi arm), `msgspec` (`Struct`/`msgpack.Encoder`).
- Growth: a new shaping feature is one `shape` feature-dict row; a new raster backend is one `RasterBackend` row keyed on the `getSurfaceClass` registry; a new bidi base direction is one `base_dir` argument value; a new shaped-run fact is one field on `PositionedGlyphRun`; zero new surface.
- Boundary: no font subsetting/instancing (that stays at `typography/font#FONT`), no PDF authoring (that is `documents/emit#DOCUMENT`), no PAdES/PDF security (that is `typography/sign#SIGN`); the owner shapes text and renders glyphs, never producing a document. The `SHAPE` arm produces the `PositionedGlyphRun` the `documents/emit#DOCUMENT` text placement and the `figures/compose#COMPOSE` annotation owners consume, never a parallel shaping owner; the `RASTERIZE` arm's SVG-backend output feeds the document and `figures/compose#COMPOSE` owners directly with no native dependency, PNG/PDF raster routing through the skia or cairo backend the `getSurfaceClass` registry selects. The uharfbuzz `SubsetInput`/`subset` is the rejected duplicate of the `typography/font#FONT` `SUBSET` footprint; a hand-rolled COLRv1 `PaintFormat` dispatch at the call site is the rejected duplicate of blackrenderer's in-package `drawGlyph` paint-graph traversal — the `RASTERIZE` arm composes `drawGlyph` over the `Surface.canvas`, never re-implementing solid/linear/radial/sweep/composite paint formats; the `renderText` one-shot is the rejected lower-capability form (it hides the palette, location, and glyph-bounds evidence the receipt carries); a hand-rolled UAX#9 reorder is the rejected duplicate of `bidi.get_display`. A parallel `_RasterBackend` enum beside the policy and a second shaping-buffer construction are the collapsed forms — `RasterBackend` keys the registry and the one `Buffer` pipeline shapes every run.

```python signature
import io
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final

import msgspec
from anyio import to_process
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

type FeatureSpec = Mapping[str, int | bool | Sequence[tuple[int, int, int | bool]]]
type ShapeAcceptor = Callable[["Shaping"], bytes]

_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
GATED: Final[frozenset["ShapeOp"]] = frozenset()


class ShapeOp(StrEnum):
    SHAPE = "shape"
    BIDI = "bidi"
    RASTERIZE = "rasterize"


class RasterBackend(StrEnum):
    SVG = "svg"
    SKIA = "skia"
    CAIRO = "cairo"
    COREGRAPHICS = "coregraphics"


class PositionedGlyphRun(Struct, frozen=True):
    glyphs: tuple[tuple[int, int, int, int, int, int], ...]
    outline: str = ""

    @property
    def count(self) -> int:
        return len(self.glyphs)

    def to_svg_path(self) -> str:
        return self.outline


class ShapeParams(Struct, frozen=True, kw_only=True):
    text: str = ""
    face_index: int = 0
    variations: Mapping[str, float] = {}
    features: FeatureSpec | None = None
    base_direction: str | None = None
    raster_backend: RasterBackend = RasterBackend.SVG
    font_size: float = _DEFAULT_FONT_SIZE
    margin: int = _DEFAULT_MARGIN
    palette_index: int = 0


class Shaping(Struct, frozen=True):
    step: ShapeOp
    font: bytes
    params: ShapeParams

    async def run(self) -> RuntimeRail[ContentKey]:
        return await async_boundary(f"shape.{self.step}", self._emit)

    async def _emit(self) -> ContentKey:
        data = (
            await to_process.run_sync(_gated_bidi, self.params.text, self.params.base_direction)
            if self.step is ShapeOp.BIDI
            else _SHAPE_TABLE[self.step](self)
        )
        return ContentIdentity.of(f"shape-{self.step}", data)


def _shape_text(shaping: "Shaping") -> bytes:
    import uharfbuzz as hb
    from fontTools.pens.svgPathPen import SVGPathPen
    from fontTools.ttLib import TTFont

    params = shaping.params
    font = hb.Font.create(hb.Face.create(shaping.font, params.face_index))
    if params.variations:
        font.set_variations(dict(params.variations))
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    buffer.guess_segment_properties()
    hb.shape(font, buffer, dict(params.features) if params.features else None)
    glyphs = tuple(
        (info.codepoint, info.cluster, pos.x_advance, pos.y_advance, pos.x_offset, pos.y_offset)
        for info, pos in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True)
    )
    pen = SVGPathPen(TTFont(io.BytesIO(shaping.font)).getGlyphSet())
    for gid, *_ in glyphs:
        font.draw_glyph_with_pen(gid, pen)
    return _RUN_ENCODER.encode(PositionedGlyphRun(glyphs=glyphs, outline=pen.getCommands()))


def _gated_bidi(text: str, base_direction: str | None) -> bytes:
    import bidi

    # RESEARCH: bidi.get_display(text, base_dir=, debug=) keyword spellings are unverified — python-bidi is gated
    # python_version<'3.15' (the Rust pyo3 binding lacks a CPython 3.15 wheel) and carries no folder .api catalogue
    # yet, so the get_display call and its base_dir keyword stay marked until the gated-band reflection rows them;
    # the UAX#9 logical-to-visual reorder feeding the SHAPE arm is the settled design shape the reorder admits.
    visual = bidi.get_display(text, base_dir=base_direction) if base_direction is not None else bidi.get_display(text)
    return visual.encode("utf-8")


def _rasterize_color(shaping: "Shaping") -> bytes:
    import uharfbuzz as hb
    from blackrenderer.backends import getSurfaceClass
    from blackrenderer.font import BlackRendererFont
    from blackrenderer.render import BackendUnavailableError, buildGlyphLine, calcGlyphLineBounds
    from fontTools.ttLib import TTFont

    params = shaping.params
    surface_class = getSurfaceClass(params.raster_backend.value)
    if surface_class is None:
        raise BackendUnavailableError(params.raster_backend.value)
    hb_font = hb.Font.create(hb.Face.create(shaping.font, params.face_index))
    font = BlackRendererFont(ttFont=TTFont(io.BytesIO(shaping.font), fontNumber=0, lazy=True), hbFont=hb_font)
    if params.variations:
        font.setLocation(dict(params.variations))
        hb_font.set_variations(dict(params.variations))
    palette = font.getPalette(params.palette_index)
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    buffer.guess_segment_properties()
    hb.shape(hb_font, buffer, dict(params.features) if params.features else None)
    glyph_line = buildGlyphLine(buffer.glyph_infos, buffer.glyph_positions, font.glyphNames)
    x_min, y_min, x_max, y_max = calcGlyphLineBounds(glyph_line, font) or (0.0, 0.0, 0.0, 0.0)
    scale = params.font_size / font.unitsPerEm
    margin = params.margin
    surface = surface_class()
    with surface.canvas(
        (x_min * scale - margin, y_min * scale - margin, x_max * scale + margin, y_max * scale + margin)
    ) as canvas:
        canvas.transform((scale, 0, 0, scale, 0, 0))
        for glyph in glyph_line:
            with canvas.savedState():
                canvas.transform((1, 0, 0, 1, glyph.xOffset, glyph.yOffset))
                font.drawGlyph(glyph.name, canvas, palette=palette)
            canvas.transform((1, 0, 0, 1, glyph.xAdvance, glyph.yAdvance))
    sink = io.BytesIO()
    surface.saveImage(sink)
    return sink.getvalue()


_SHAPE_TABLE: Final[MappingProxyType[ShapeOp, ShapeAcceptor]] = MappingProxyType({
    ShapeOp.SHAPE: _shape_text,
    ShapeOp.RASTERIZE: _rasterize_color,
})
```

## [03]-[RESEARCH]

- [SHAPING] [RESOLVED]: the uharfbuzz `Face.create(payload, index)`/`Font.create(face)`/`Buffer.create()`/`add_str(text)`/`guess_segment_properties()`/`shape(font, buffer, features)`/`GlyphInfo`(`codepoint`/`cluster`/`flags`)/`GlyphPosition`(`x_advance`/`y_advance`/`x_offset`/`y_offset`)/`GlyphFlags.UNSAFE_TO_BREAK`/`Font.set_variations`/`Font.draw_glyph_with_pen`/`Buffer.direction`/`script`/`language` spellings verify against the folder `.api` catalogue for `uharfbuzz` (`0.55.0` reflected on cp315) public-type rows `[06]`/`[07]`/`[08]` and entrypoint rows `[02]`/`[04]`/`[01]`/`[02]`/`[05]`/`[06]`/`[08]`/`[10]`. `Face.create(payload, index)` admits raw `bytes` directly, so the `Blob.from_file_path` file-load row is unused (this owner holds the font payload in memory). `buffer.glyph_infos`/`glyph_positions` are read after `shape` as lists of `GlyphInfo`/`GlyphPosition`; `guess_segment_properties` infers direction/script/language before shaping when explicit properties are not set; `GlyphInfo.flags & GlyphFlags.UNSAFE_TO_BREAK` is the line-break safety signal a layout consumer reads to decide where a shaped run may be split. The `features` parameter to `shape` accepts the catalogued `dict[str, int | bool | Sequence[tuple[int, int, int | bool]]]` feature-range shape. The vector-outline bridge drives `Font.draw_glyph_with_pen(gid, SVGPathPen(glyphSet))` from `fontTools.pens.svgPathPen`; the `SVGPathPen.getCommands()` SVG-`d` accessor and `TTFont.getGlyphSet()` glyph factory verify against the folder `.api` catalogue for `fonttools` pen row `[07]` and entrypoint row `[04]`, so the `_shape_text` outline bridge is settled fence code. The COLRv1 color-glyph path is owned by the `RASTERIZE` arm through blackrenderer's `drawGlyph` rather than a raw `Font.draw_glyph(gid, PaintFuncs, state)` call here, so the page never holds two color-glyph traversals.
- [BIDI] [RESEARCH]: the python-bidi UAX#9 reorder is the gated `python_version<'3.15'` arm crossing `anyio.to_process.run_sync` onto the runtime subprocess band — the Rust pyo3 binding lacks a CPython 3.15 wheel (`pyo3-ffi 0.27` caps at 3.14), so the cp315-core process never imports `bidi` and the gated-band worker imports it at module scope. `bidi.get_display(text, base_dir=...)` resolves the logical-to-visual mixed-direction character order before shaping so an Arabic/Hebrew-embedded run shapes in visual order; the reordered text the worker returns is the input the `SHAPE` arm then shapes. RESEARCH: the `bidi.get_display(text, base_dir=, debug=)` keyword spellings and the `bidi.algorithm.get_display`/`get_base_level` module surface are unverified — python-bidi carries no folder `.api` catalogue yet (no cp315 wheel to reflect against), so the `_gated_bidi` body gates the `get_display` call and its `base_dir` keyword behind the explicit `# RESEARCH` comment and they stay unverified until the gated-band reflection rows the member surface; the UAX#9 logical-to-visual reorder feeding the `SHAPE` arm is the settled design shape the reorder admits.
- [RASTERIZE] [RESOLVED]: the blackrenderer `font.BlackRendererFont(path=None, *, fontNumber=0, lazy=True, ttFont=None, hbFont=None)`, `BlackRendererFont.drawGlyph(glyphName, canvas, *, palette, textColor)`, `setLocation(location)`, `getPalette(paletteIndex)`, `glyphNames`, `unitsPerEm`, `render.buildGlyphLine(infos, positions, glyphNames)`, `render.calcGlyphLineBounds(glyphLine, font)`, `render.BackendUnavailableError`, `backends.getSurfaceClass(backendName, imageExtension)`, `Surface.canvas(boundingBox)`, `Surface.saveImage(path)`, `Canvas.savedState()`, and `Canvas.transform(transform)` entrypoint spellings verify against the folder `.api` catalogue for `blackrenderer` (`0.8.2` reflected on cp315) entrypoint rows `[01]`-`[04]`/`[02]`/`[03]` and `Surface`/`Canvas` protocol rows `[01]`/`[02]`/`[11]`/`[12]`. The `GlyphInfo.xOffset`/`yOffset`/`xAdvance`/`yAdvance`/`name` field spellings the per-glyph offset-and-advance walk reads verify against the folder `.api` catalogue for `blackrenderer` public-type row `[05]` (`render.GlyphInfo` is the `NamedTuple` `(name, gid, xAdvance, yAdvance, xOffset, yOffset)`), so the `_rasterize_color` offset-and-advance walk is settled fence code. The `font_size`/`unitsPerEm` pixel scale is concatenated onto the `Surface.canvas` context as one `(scale, 0, 0, scale, 0, 0)` transform and folded into the bounds so the COLRv1 raster renders at a real pixel size rather than raw font units, never the `renderText` `fontSize=` one-shot. The `RASTERIZE` arm composes the per-glyph path — `getSurfaceClass(backendName)` resolves the concrete `Surface` (a `None` return raises `BackendUnavailableError`), `BlackRendererFont(ttFont=..., hbFont=...)` loads the paired font without a path round-trip, `setLocation`/`getPalette` instance the variable-font location and resolved palette, `buildGlyphLine`/`calcGlyphLineBounds` shape the HarfBuzz run and compute font-unit bounds, and `drawGlyph` folds each glyph onto the margin-inset `Surface.canvas` under a `savedState`/`transform` offset-and-advance walk — never the `renderText` one-shot, which hides the palette, location, and bounds evidence the `RASTERIZE` receipt carries. The in-package `Canvas` paint surface (`newPath`/`drawPathSolid`/`drawPathLinearGradient`/`drawPathRadialGradient`/`drawPathSweepGradient`/`compositeMode`/`clipPath`) is the COLRv1 `PaintFormat` traversal `drawGlyph` drives; a hand-rolled paint dispatch at the call site is the rejected duplicate. `setLocation` applies the variable-font location to the `BlackRendererFont` and `hb_font.set_variations` mirrors it onto the shaping font so the COLRv1 `PaintVar*` deltas and the shaped advances agree. The SVG backend serializes with no native dependency; PNG/PDF raster routing engages the `getSurfaceClass`-selected skia or cairo backend. Boundary-scope import only — skia/cairo/coregraphics load native libraries on backend resolution.
