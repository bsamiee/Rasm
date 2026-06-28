# [PY_ARTIFACTS_SHAPE]

The text-shaping and color-glyph rasterization owner over the document rail. `Shaping` is ONE owner that takes a Unicode text run and a font and folds it through a closed `ShapeOp` family into a positioned glyph run and its rendered color glyphs: uharfbuzz shapes the run (`Face` -> `Font` -> `Buffer` -> `shape`) into a `PositionedGlyphRun` carrying per-glyph GID / cluster / advance / offset plus the `SVGPathPen` outline bridge, a python-bidi UAX#9 reorder pass resolves mixed-direction logical-to-visual order before shaping on the worker lane, and blackrenderer folds each glyph of the shaped run through its in-package COLRv1/COLRv0 `drawGlyph` paint-graph traversal onto a backend `Surface` and serializes to PNG/PDF/SVG. uharfbuzz, fonttools, blackrenderer, and python-bidi are admitted in the manifest; the `PositionedGlyphRun` is the one value object the shaped-run arm carries, consumed by `document/emit#DOCUMENT` text placement, `composition/compose#COMPOSE` annotation, and the `typography/layout#LAYOUT` line-break/Knuth-Plass paragraph owner, and the face selection and variation location are read from `typography/font#FONT`. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Document`/`.Preview`.

## [01]-[INDEX]

- [01]-[SHAPE]: uharfbuzz OpenType shaping, python-bidi UAX#9 reorder, and blackrenderer COLRv1 rasterization owner over the closed `ShapeOp` step table; `PositionedGlyphRun` is the carried shaped-run value object the SHAPE arm produces (its outline bridged through the advance-threaded `SVGPathPen`/`TransformPen` pen) and the RASTERIZE arm reshapes, `RasterBackend` the `getSurfaceClass` registry-key policy row, every native render offloaded off the event loop under one `CapacityLimiter`.

## [02]-[SHAPE]

- Owner: `Shaping` the one shaping-and-glyph-render owner discriminating the shaping step; `ShapeOp` the closed `StrEnum` over uharfbuzz shaping, python-bidi UAX#9 reordering, and blackrenderer COLRv1 rasterization; one frozen `_SHAPE_TABLE` `MappingProxyType` data-row dispatch maps each step to its `ShapeAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. uharfbuzz owns the OpenType layout engine, the `ot_layout_*`/`axis_infos` live-table introspection, and the cluster/feature/variation shaping surface; fonttools owns the binary font model and the `SVGPathPen` outline pen; blackrenderer owns the COLRv1 paint-graph rasterizer over fonttools+HarfBuzz; python-bidi owns the UAX#9 bidirectional reorder. `RasterBackend` is the policy-as-value `StrEnum` row whose member value is the `getSurfaceClass` registry key; `PositionedGlyphRun` is the carried shaped-run value object the `SHAPE` arm produces and the `RASTERIZE` arm reshapes.
- Auto: every native arm offloads off the event loop under one `CapacityLimiter` — uharfbuzz shaping and blackrenderer rasterization ride `to_thread` (GIL-releasing native, zero-copy of the font bytes the worker shares), the python-bidi reorder rides the gated `to_process` seam its binding requires. Shaping folds the text run through `Face.create(font_bytes, face_index)` -> `Font.create(face)` -> `Buffer.create()`/`add_str(text)`/`guess_segment_properties()` -> `shape(font, buffer, features)` then reads `glyph_infos`/`glyph_positions` (zipped strict) into a `PositionedGlyphRun`, with `Font.set_variations(variations)` axis pinning and the `Font.draw_glyph_with_pen(gid, TransformPen(SVGPathPen(TTFont(...).getGlyphSet()), (1, 0, 0, 1, cursor_x + x_offset, cursor_y + y_offset)))` -> `SVGPathPen.getCommands()` outline bridge that THREADS each glyph's `x_advance`/`y_advance` cursor and `x_offset`/`y_offset` so the positioned run's outline is laid along the baseline rather than collapsed at the origin, feeding `to_svg_path`; the Bidi reorder folds the logical text through `bidi.get_display(text, base_dir=base_direction)` on the worker into the visual-order text; rasterizing resolves the `getSurfaceClass(backendName)` surface (a `None` return raises `BackendUnavailableError`), loads one `BlackRendererFont(ttFont=TTFont(io.BytesIO(font), fontNumber=0, lazy=True), hbFont=...)`, applies `setLocation(location)` and mirrors it onto the shaping font via `hb_font.set_variations` for COLRv1 `PaintVar*`-versus-advance agreement, reads `getPalette(palette_index)`, shapes the run into `buildGlyphLine(glyph_infos, glyph_positions, glyphNames)`, computes `calcGlyphLineBounds(glyph_line, font)`, derives the `font_size`/`unitsPerEm` pixel scale, opens one `Surface.canvas(bounds)` margin-inset context concatenated with that scale, and folds each shaped glyph through `drawGlyph(glyph.name, canvas, palette=...)` inside a `savedState`/`transform` offset-and-advance walk over `glyph.xOffset`/`yOffset`/`xAdvance`/`yAdvance` before `Surface.saveImage`.
- Receipt: every arm projects its output onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `SHAPE` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `PositionedGlyphRun` byte count, the `BIDI` arm contributes `ArtifactReceipt.Document` carrying the content key and the reordered-text byte count, and the `RASTERIZE` arm contributes `ArtifactReceipt.Preview` carrying the content key and the pixel width/height. The COLR version, resolved palette index, backend name, glyph count, and pixel bounds the `RASTERIZE` arm computes stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Document`/`Preview` cases cannot carry.
- Growth: a new shaping feature is one `shape` feature-dict row; a new raster backend is one `RasterBackend` row keyed on the `getSurfaceClass` registry; a new bidi base direction is one `base_dir` argument value; a new shaped-run fact is one field on `PositionedGlyphRun`; zero new surface.
- Boundary: no font subsetting/instancing (that stays at `typography/font#FONT`), no line-break/hyphenation/paragraph layout (that is `typography/layout#LAYOUT`), no PDF authoring (that is `document/emit#DOCUMENT`), no PAdES/PDF security (that is `exchange/conformance#CONFORMANCE`); the owner shapes text and renders glyphs, never breaking a paragraph or producing a document. The `SHAPE` arm produces the `PositionedGlyphRun` the `document/emit#DOCUMENT` text placement, the `composition/compose#COMPOSE` annotation, and the `typography/layout#LAYOUT` paragraph owners consume, never a parallel shaping owner; the `RASTERIZE` arm's SVG-backend output feeds the document and `composition/compose#COMPOSE` owners directly with no native dependency, PNG/PDF raster routing through the skia or cairo backend the `getSurfaceClass` registry selects. The uharfbuzz `SubsetInput`/`subset` is the rejected duplicate of the `typography/font#FONT` `SUBSET` footprint; a hand-rolled COLRv1 `PaintFormat` dispatch at the call site is the rejected duplicate of blackrenderer's in-package `drawGlyph` paint-graph traversal — the `RASTERIZE` arm composes `drawGlyph` over the `Surface.canvas`, never re-implementing solid/linear/radial/sweep/composite paint formats; the `renderText` one-shot is the rejected lower-capability form (it hides the palette, location, and glyph-bounds evidence the receipt carries); a hand-rolled UAX#9 reorder is the rejected duplicate of `bidi.get_display`. A parallel `_RasterBackend` enum beside the policy and a second shaping-buffer construction are the collapsed forms — `RasterBackend` keys the registry and the one `Buffer` pipeline shapes every run.

```python signature
import io
import os
import tempfile
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from pathlib import Path
from types import MappingProxyType
from typing import Final

import msgspec
from anyio import CapacityLimiter, to_process, to_thread
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

lazy import uharfbuzz as hb
lazy from bidi import get_display
lazy from blackrenderer.backends import getSurfaceClass
lazy from blackrenderer.font import BlackRendererFont
lazy from blackrenderer.render import BackendUnavailableError, buildGlyphLine, calcGlyphLineBounds
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from fontTools.pens.transformPen import TransformPen
lazy from fontTools.ttLib import TTFont

type FeatureSpec = Mapping[str, int | bool | Sequence[tuple[int, int, int | bool]]]
type ShapeAcceptor = Callable[["Shaping"], bytes]

_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20
_SHAPE_SLOTS: Final[int] = os.process_cpu_count() or 4
_SHAPE_LIMITER: Final = CapacityLimiter(_SHAPE_SLOTS)
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()


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
            await to_process.run_sync(_gated_bidi, self.params.text, self.params.base_direction, limiter=_SHAPE_LIMITER)
            if self.step is ShapeOp.BIDI
            else await to_thread.run_sync(_SHAPE_TABLE[self.step], self, limiter=_SHAPE_LIMITER)
        )
        return ContentIdentity.of(f"shape-{self.step}", data)


def _shape_text(shaping: "Shaping") -> bytes:
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
    cursor_x = cursor_y = 0
    for gid, _cluster, x_advance, y_advance, x_offset, y_offset in glyphs:  # thread the shaped pen position; a bare origin draw stacks every glyph at (0, 0)
        font.draw_glyph_with_pen(gid, TransformPen(pen, (1.0, 0.0, 0.0, 1.0, cursor_x + x_offset, cursor_y + y_offset)))
        cursor_x += x_advance
        cursor_y += y_advance
    return _RUN_ENCODER.encode(PositionedGlyphRun(glyphs=glyphs, outline=pen.getCommands()))


def _gated_bidi(text: str, base_direction: str | None) -> bytes:
    # UAX#9 logical-to-visual reorder BEFORE shaping so a mixed Arabic/Hebrew + Latin run reaches the
    # SHAPE arm in visual order; `base_dir=None` auto-computes the paragraph level, `'L'|'R'` pins it.
    return get_display(text, base_dir=base_direction).encode("utf-8")


def _rasterize_color(shaping: "Shaping") -> bytes:
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
    # every blackrenderer Surface.saveImage writes only to a real path (`open(path)`/`os.fspath`), never a file-like
    with tempfile.NamedTemporaryFile(suffix=surface.fileExtension, delete=False) as handle:
        sink = Path(handle.name)
    try:
        surface.saveImage(str(sink))
        return sink.read_bytes()
    finally:
        sink.unlink(missing_ok=True)


_SHAPE_TABLE: Final[MappingProxyType[ShapeOp, ShapeAcceptor]] = MappingProxyType({
    ShapeOp.SHAPE: _shape_text,
    ShapeOp.RASTERIZE: _rasterize_color,
})
```

## [03]-[RESEARCH]

- [SHAPE] [RESOLVED]: the `hb.Face.create(font_bytes, face_index)` -> `hb.Font.create(face)` -> `hb.Buffer.create()`/`add_str`/`guess_segment_properties`/`hb.shape(font, buffer, features)` -> `buffer.glyph_infos`/`glyph_positions` pipeline, the `Font.set_variations(dict)` axis pinning, and the `Font.draw_glyph_with_pen(gid, pen)` outline export verify against the folder `.api/uharfbuzz.md` (shaping entry rows `[06]`/`[08]`, outline-export row `[06]`, variation row `[10]`); the `fontTools.pens.transformPen.TransformPen(outPen, transformation)` per-glyph affine and `fontTools.pens.svgPathPen.SVGPathPen.getCommands()` SVG-`d` accessor verify against `.api/fonttools.md` pen rows `[08]`/`[07]` — the `TransformPen` threads the shaped advance so the positioned run's outline lands along the baseline, where a bare `draw_glyph_with_pen(gid, pen)` per glyph stacks every contour at the origin. The shaped `x_advance`/`x_offset` ride font units because `Font.create` defaults the scale to the face upem, matching the `SVGPathPen` glyph-set coordinate space.
- [OFFLOAD] [RESOLVED]: the native render arms route off the event loop — `to_thread.run_sync(..., limiter=_SHAPE_LIMITER)` carries the GIL-releasing uharfbuzz shaping and blackrenderer rasterization (the worker shares the font bytes with zero serialization), `to_process.run_sync(..., limiter=_SHAPE_LIMITER)` carries the python-bidi reorder its `.api/python-bidi.md` requires (the PyO3 binding has no in-process gated package), one `CapacityLimiter` bounding the whole shaping subsystem; an inline `_SHAPE_TABLE[step](self)` on the loop is the rejected form that stalls the scheduler on every native call.
- [RASTERIZE] [RESOLVED]: the blackrenderer `getSurfaceClass(backend)` -> `BlackRendererFont(ttFont=, hbFont=)` -> `setLocation`/`getPalette`/`buildGlyphLine`/`calcGlyphLineBounds` -> `Surface.canvas(bounds)`/`drawGlyph`/`saveImage` COLRv1 traversal verifies against `.api/blackrenderer.md` (font rows `[01]`-`[07]`, render rows `[02]`/`[03]`, backend row `[04]`, surface rows `[01]`/`[02]`); the `savedState`/`transform` offset-and-advance walk over `glyph.xOffset`/`yOffset`/`xAdvance`/`yAdvance` is the in-package paint-graph composition, never a hand-rolled `PaintFormat` dispatch, and the `with surface.canvas(...)` context closes the draw handle deterministically. `Surface.saveImage(path)` writes only to a real path (`open(path)`/`os.fspath`/`cairo.PDFSurface(path)` across every backend), never a file-like, so the arm captures the rendered bytes through a bracketed `tempfile.NamedTemporaryFile` round-trip keyed off `surface.fileExtension` and `unlink`s in a `finally`, rather than a `BytesIO` sink the path-only `saveImage` rejects.
