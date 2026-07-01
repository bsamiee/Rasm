# [PY_ARTIFACTS_SHAPE]

The text-shaping, itemization, and color-glyph rasterization owner over the document rail. `Shaping` is ONE owner that folds a Unicode text run and a font through a closed 7-arm `ShapeOp` family — `NORMALIZE` (UAX#15 NFC pre-compose), `BIDI` (UAX#9 logical→visual reorder), `ITEMIZE` (script/direction visual-run split), `FALLBACK` (coverage-probe + secondary-face escalation), `SHAPE` (uharfbuzz OpenType shaping into a `PositionedGlyphRun` carrying per-glyph GID/cluster/advance/offset plus the HarfBuzz break-safety flags column and the advance-threaded `SVGPathPen` outline), `RASTERIZE` (color-format-probed COLRv1 / CBDT-sbix-PNG / SVG-table / CPU-BGRA32 glyph render), and `QA` (vharfbuzz golden round-trip). Each arm rides its own offload lane — `THREAD` for the GIL-releasing native shape/raster/QA, `PROCESS` for the gated python-bidi/PyICU/uniseg reorder-normalize-itemize workers — keyed by one row on the frozen `_SHAPE_TABLE` `frozendict` under a single `CapacityLimiter`, every offload wrapped in a `stamina` worker-death retry inside an OpenTelemetry span and a `structlog` event. The writing direction is a closed `WritingDirection` axis (`auto`/`ltr`/`rtl`/`ttb`) driving explicit `Buffer.direction` and vertical advances; the bidi and segmentation owner is a `BidiEngine`/`SegmentEngine` policy value per run (`python-bidi`/`uniseg` default, `PyICU` when provisioned), never a parallel shaping owner. The `PositionedGlyphRun` carries the per-glyph `GlyphFlags` column so `typography/layout#LAYOUT` reads `UNSAFE_TO_BREAK`/`UNSAFE_TO_CONCAT` for line-break refusal and `SAFE_TO_INSERT_TATWEEL` for kashida justification. uharfbuzz, fonttools, blackrenderer, python-bidi, vharfbuzz, PyICU, and uniseg are admitted in the manifest; the `PositionedGlyphRun` is consumed by `document/emit#DOCUMENT` text placement, `composition/compose#COMPOSE` annotation, and `typography/layout#LAYOUT` line-break, while face selection and variation location are read from `typography/font#FONT`. Every arm returns `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Document`/`.Preview`.

## [01]-[INDEX]

- [01]-[SHAPE]: the 7-arm `ShapeOp` shaping-itemization-rasterization owner over the frozen `_SHAPE_TABLE` `frozendict` — each row a `(ShapeAcceptor, Lane)` pair so the offload lane is a row property (never a smuggled `if self.step is BIDI`), `PositionedGlyphRun` the carried shaped-run value object the `SHAPE` arm produces (its outline bridged through the advance-threaded `SVGPathPen`/`TransformPen` pen) and the `RASTERIZE` arm reshapes, `ColorFormat` the probe-selected color-glyph render policy row, `WritingDirection`/`BidiEngine`/`SegmentEngine` the closed policy axes driving `Buffer.direction`/vertical metrics and the bidi/segment owner, every native render offloaded off the event loop under one `CapacityLimiter` and one `stamina` worker-death retry inside an OpenTelemetry span.

## [02]-[SHAPE]

- Owner: `Shaping` the one shaping-and-glyph-render owner discriminating the step; `ShapeOp` the closed `StrEnum` over `NORMALIZE`/`BIDI`/`ITEMIZE`/`FALLBACK`/`SHAPE`/`RASTERIZE`/`QA`; one frozen `_SHAPE_TABLE` `frozendict` data-row dispatch maps each step to its `(ShapeAcceptor, Lane)` pair with zero `match`/`case` sprawl, the closed `StrEnum` total over the table by construction and `_ARM` keying only the `anyio` async-loop crossing per `Lane`. uharfbuzz owns the OpenType layout engine, the `ot_layout_*`/`axis_infos` introspection, the cluster/feature/variation shaping surface, the `get_nominal_glyph` coverage probe, the `GlyphFlags` break-safety signal, the CBDT/sbix PNG + SVG-table color extractors, and the `RasterPaint`/`RasterImage` zero-native-dep BGRA32/A8 CPU rasterizer; fonttools owns the binary font model, the `SVGPathPen` outline pen, and the `fontTools.unicodedata.script` itemization fallback; blackrenderer owns the COLRv1 paint-graph rasterizer and the `listBackends` deployment matrix; python-bidi owns the UAX#9 reorder default; PyICU owns the locale-aware `Bidi.getVisualRun`/`Script.getScript`/`Normalizer2` upgrade behind the `SegmentEngine`/`BidiEngine` row; uniseg + stdlib `unicodedata` own the locale-free itemize/normalize default. `WritingDirection` drives `Buffer.direction` (`ttb` carrying the vertical advances hb fills from `vhea`); `RasterBackend` keys the blackrenderer surface registry; `ColorFormat` is the probe-selected color-glyph render policy the `RASTERIZE` arm dispatches on; `PositionedGlyphRun` is the carried shaped-run value object the `SHAPE` arm produces (its 7-tuple glyph column carrying GID/cluster/x-advance/y-advance/x-offset/y-offset/flags) and the `RASTERIZE` arm reshapes.
- Auto: every arm offloads off the event loop under one `CapacityLimiter`, its lane read off the `_SHAPE_TABLE` row — `SHAPE`/`RASTERIZE`/`FALLBACK`/`QA` ride the `THREAD` lane (GIL-releasing native, zero-copy of the font bytes the worker shares), `NORMALIZE`/`BIDI`/`ITEMIZE` ride the gated `PROCESS` lane their PyO3/native-C++ bindings require — and every offload is wrapped in one `stamina.AsyncRetryingCaller` bound to `BrokenWorkerProcess`/`BrokenWorkerInterpreter` inside an OpenTelemetry span carrying step/lane/direction/backend and a `structlog` event carrying the emitted byte count. `NORMALIZE` folds the run through `Normalizer2.getNFCInstance().normalize` (`SegmentEngine.ICU`) or `unicodedata.normalize("NFC", ...)` (default) so combining marks pre-compose before shaping; `BIDI` folds through `bidi.get_display(base_dir=...)` (`BidiEngine.PYTHON_BIDI`) or `Bidi.setPara`/`writeReordered` (`ICU`) into visual order; `ITEMIZE` partitions the run into single-direction/single-script spans through `Bidi.getVisualRun` + `Script.getScript` (`ICU`) or a `fontTools.unicodedata.script` run-length grouping (default), the spans feeding `typography/font#FONT` fallback and per-span shaping; `FALLBACK` probes `get_nominal_glyph` over the primary + `fallback_faces` faces and resolves a per-cluster covering-face assignment (`-1` = tofu the `Buffer.not_found_glyph` override renders); `SHAPE` folds the run through `Face.create` → `Font.create` → `Buffer.create`/`add_str` → `_segment` (explicit `Buffer.direction` from `WritingDirection`, `set_script_from_ot_tag`/`set_language_from_ot_tag` when pinned, `guess_segment_properties` filling the rest, `PRODUCE_UNSAFE_TO_CONCAT` flag set) → `shape(font, buffer, features)` then reads `glyph_infos`/`glyph_positions` (zipped strict, `info.flags` into the 7th column) into a `PositionedGlyphRun`, threading each glyph's `x_advance`/`y_advance` cursor and `x_offset`/`y_offset` through the `TransformPen(SVGPathPen(...))` so the combined `outline` lays along the horizontal or vertical baseline AND drawing each glyph to its own origin `SVGPathPen` into `glyph_outlines` (the `run.on_path()` per-glyph outline + advance pairs the `graphic/vector#VECTOR` `text_path` curved-baseline seam threads); `RASTERIZE` probes `Face.has_color_paint`/`has_color_png`/`has_color_svg` and dispatches the `_COLOR_TABLE` row — COLRv1 through the blackrenderer `getSurfaceClass`→`BlackRendererFont`→`drawGlyph` paint-graph traversal, CBDT/sbix through `Font.get_glyph_color_png`, SVG-table through `Face.get_glyph_color_svg`, and the zero-native-dep `RasterPaint`→`paint_glyph`→`render` BGRA32 CPU fallback when no blackrenderer backend module imports; `QA` writes the font to a temp path, shapes through `Vharfbuzz`, and serializes the `hb-shape` golden through `serialize_buf`.
- Receipt: every arm projects its output onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — `SHAPE`/`BIDI`/`ITEMIZE`/`NORMALIZE`/`FALLBACK` contribute `ArtifactReceipt.Document` carrying the content key and the encoded byte count (the `PositionedGlyphRun`, the reordered text, the run partition, the NFC text, or the coverage assignment), `QA` contributes `ArtifactReceipt.Document` carrying the encoded golden byte count, and `RASTERIZE` contributes `ArtifactReceipt.Preview` carrying the content key and the pixel width/height. The resolved COLR version, chosen `ColorFormat`, backend name, `listBackends` deployment matrix, glyph count, resolved script/direction, and pixel bounds stay interior evidence the arm folds into its content-key derivation and the structlog span, never new receipt fields the shared `Document`/`Preview` cases cannot carry.
- Growth: a new shaping feature is one `shape` feature-dict row; a new offload arm is one `ShapeOp` member plus one `_SHAPE_TABLE` `(acceptor, lane)` row; a new raster backend is one `RasterBackend` row keyed on the `getSurfaceClass` registry; a new color-glyph format is one `ColorFormat` member plus one `_COLOR_PROBE` predicate and one `_COLOR_TABLE` row; a new writing direction is one `WritingDirection` member plus one `_HB_DIRECTION`/`_BIDI_BASE` correspondence; a new bidi/segment owner is one `BidiEngine`/`SegmentEngine` member plus one `match` arm on the owning acceptor; a new shaped-run fact is one column on the `PositionedGlyphRun` glyph tuple plus its derived property; zero new surface.
- Boundary: no font subsetting/instancing (that stays at `typography/font#FONT`), no line-break/hyphenation/paragraph layout (that is `typography/layout#LAYOUT`, which reads the `PositionedGlyphRun` break-safety column), no PDF authoring (that is `document/emit#DOCUMENT`), no PAdES/PDF security (that is `exchange/conformance#CONFORMANCE`); the owner shapes text, itemizes, reorders, normalizes, resolves fallback coverage, and renders glyphs, never breaking a paragraph or producing a document. Text-on-path — threading each shaped glyph's outline along an arc-length-parameterized baseline `Path` with a tangent-following transform and merging overlaps on tight curves — is the LANDED `graphic/vector#VECTOR` `text_path(glyphs, baseline)` entrypoint's `skia-pathops`/`svgelements` algebra: the `SHAPE` arm draws each glyph to its OWN origin `SVGPathPen` (`PositionedGlyphRun.glyph_outlines`), and a curved-baseline consumer hands `run.on_path()` (the per-glyph outline + advance pairs) to `vector.text_path` at the seam, never a `pathops` import here — the straight-baseline run stays this owner's own advance-threaded combined `outline` pen. The uharfbuzz `SubsetInput`/`subset` is the rejected duplicate of the `typography/font#FONT` `SUBSET` footprint; a hand-rolled COLRv1 `PaintFormat` dispatch is the rejected duplicate of blackrenderer's in-package `drawGlyph`; the `renderText` one-shot is the rejected lower-capability form (it hides the palette/location/glyph-bounds/backend evidence); a hand-rolled UAX#9 reorder, a hand-rolled UAX#14/UAX#29 break-class table, and a hand-coded script-to-OT-tag map are the rejected duplicates of `bidi.get_display`, `uniseg`, and `fontTools.unicodedata.script`; a parallel `_RasterBackend` enum beside the policy, a second shaping-buffer construction, and a smuggled `if self.step is BIDI` lane branch are the collapsed forms — the `_SHAPE_TABLE` row carries the lane and the one `Buffer` pipeline shapes every run.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import os
import tempfile
import unicodedata
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from itertools import accumulate, groupby
from pathlib import Path
from typing import Final, assert_never

import msgspec
import structlog
from anyio import BrokenWorkerInterpreter, BrokenWorkerProcess, CapacityLimiter, to_process, to_thread
from builtins import frozendict
from msgspec import Struct
from opentelemetry import trace
from stamina import AsyncRetryingCaller

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary

lazy import uharfbuzz as hb
lazy from bidi import get_display
lazy from blackrenderer.backends import getSurfaceClass, listBackends
lazy from blackrenderer.font import BlackRendererFont
lazy from blackrenderer.render import BackendUnavailableError, buildGlyphLine, calcGlyphLineBounds
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from fontTools.pens.transformPen import TransformPen
lazy from fontTools.ttLib import TTFont
lazy from fontTools.unicodedata import script as ot_script
lazy from icu import Bidi, Normalizer2, Script  # gated PyICU upgrade behind the BidiEngine/SegmentEngine.ICU rows; absent -> the default arms run
lazy from vharfbuzz import Vharfbuzz

# --- [TYPES] ----------------------------------------------------------------------------
type FeatureSpec = Mapping[str, int | bool | Sequence[tuple[int, int, int | bool]]]
type ShapeAcceptor = Callable[["Shaping"], bytes]
type ColorAcceptor = Callable[["Shaping", object, object], bytes]  # (shaping, hb.Face, hb.Font) -> rendered bytes


class ShapeOp(StrEnum):
    NORMALIZE = "normalize"
    BIDI = "bidi"
    ITEMIZE = "itemize"
    FALLBACK = "fallback"
    SHAPE = "shape"
    RASTERIZE = "rasterize"
    QA = "qa"


class Lane(StrEnum):
    THREAD = "thread"    # GIL-releasing native (shape/raster/QA); the worker shares the font bytes zero-copy
    PROCESS = "process"  # gated PyO3/native-C++ workers (python-bidi/PyICU/uniseg) with no in-process gated package


class WritingDirection(StrEnum):
    AUTO = "auto"
    LTR = "ltr"
    RTL = "rtl"
    TTB = "ttb"


class BidiEngine(StrEnum):
    PYTHON_BIDI = "python-bidi"  # the locale-free UAX#9 default (cp315-active)
    ICU = "icu"                  # the locale/explicit-level PyICU upgrade (gated absent today)


class SegmentEngine(StrEnum):
    UNISEG = "uniseg"  # the locale-free default (fontTools.unicodedata.script + stdlib NFC)
    ICU = "icu"        # the CLDR-tailored PyICU upgrade (gated absent today)


class RasterBackend(StrEnum):
    SVG = "svg"
    SKIA = "skia"
    CAIRO = "cairo"
    COREGRAPHICS = "coregraphics"


class ColorFormat(StrEnum):
    PAINT = "paint"    # COLRv1 paint graph via blackrenderer
    PNG = "png"        # CBDT/sbix bitmap via Font.get_glyph_color_png
    SVG = "svg"        # OT-SVG table via Face.get_glyph_color_svg
    RASTER = "raster"  # zero-native-dep uharfbuzz RasterPaint BGRA32 CPU fallback


# --- [CONSTANTS] ------------------------------------------------------------------------
_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20
_SHAPE_SLOTS: Final[int] = os.process_cpu_count() or 4
_SHAPE_LIMITER: Final = CapacityLimiter(_SHAPE_SLOTS)
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_UNSAFE_TO_BREAK: Final = 0x0001        # mirrors hb.GlyphFlags.UNSAFE_TO_BREAK — the layout owner refuses a line break inside the cluster
_UNSAFE_TO_CONCAT: Final = 0x0002       # mirrors hb.GlyphFlags.UNSAFE_TO_CONCAT — the run-cache owner refuses a shaped-run splice
_SAFE_TO_INSERT_TATWEEL: Final = 0x0004  # mirrors hb.GlyphFlags.SAFE_TO_INSERT_TATWEEL — the kashida justifier's tatweel-insertion points
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
_TRANSIENT: Final = AsyncRetryingCaller(attempts=3, timeout=30.0).on((BrokenWorkerProcess, BrokenWorkerInterpreter))
_HB_DIRECTION: Final[frozendict[WritingDirection, str | None]] = frozendict(
    {WritingDirection.AUTO: None, WritingDirection.LTR: "ltr", WritingDirection.RTL: "rtl", WritingDirection.TTB: "ttb"}
)
_BIDI_BASE: Final[frozendict[WritingDirection, str | None]] = frozendict(
    {WritingDirection.AUTO: None, WritingDirection.LTR: "L", WritingDirection.RTL: "R", WritingDirection.TTB: None}
)
_ICU_LEVEL: Final[frozendict[WritingDirection, int]] = frozendict(  # 0 = LTR base level, 1 = RTL (UAX#9); AUTO falls to the LTR base absent the ICU DEFAULT_LTR constant
    {WritingDirection.AUTO: 0, WritingDirection.LTR: 0, WritingDirection.RTL: 1, WritingDirection.TTB: 0}
)


# --- [MODELS] ---------------------------------------------------------------------------
class PositionedGlyphRun(Struct, frozen=True):
    # glyph = (codepoint/GID, cluster, x_advance, y_advance, x_offset, y_offset, flags); `flags` is the hb.GlyphFlags column layout reads for break/concat safety and tatweel points
    glyphs: tuple[tuple[int, int, int, int, int, int, int], ...]
    outline: str = ""
    direction: str = "ltr"
    script: str = ""
    glyph_outlines: tuple[str, ...] = ()  # per-glyph origin-drawn SVG d-strings the `graphic/vector#VECTOR` `text_path` curved-baseline seam threads along an arc-length `Path`; empty for a straight-baseline consumer that reads `outline`

    @property
    def count(self) -> int:
        return len(self.glyphs)

    def on_path(self) -> tuple[tuple[str, float], ...]:
        # the `graphic/vector#VECTOR` `text_path` seam value: each glyph's origin-drawn outline paired with its x-advance,
        # so a curved-baseline consumer hands `run.on_path()` to `vector.text_path(glyphs, baseline)` (never a pathops import here).
        return tuple((outline, float(glyph[2])) for glyph, outline in zip(self.glyphs, self.glyph_outlines, strict=True))

    def to_svg_path(self) -> str:
        return self.outline

    @property
    def unsafe_to_break(self) -> frozenset[int]:
        return frozenset(glyph[1] for glyph in self.glyphs if glyph[6] & _UNSAFE_TO_BREAK)

    @property
    def unsafe_to_concat(self) -> frozenset[int]:
        return frozenset(glyph[1] for glyph in self.glyphs if glyph[6] & _UNSAFE_TO_CONCAT)

    @property
    def tatweel_points(self) -> tuple[int, ...]:
        return tuple(glyph[1] for glyph in self.glyphs if glyph[6] & _SAFE_TO_INSERT_TATWEEL)


class ShapeParams(Struct, frozen=True, kw_only=True):
    text: str = ""
    face_index: int = 0
    variations: Mapping[str, float] = {}
    features: FeatureSpec | None = None
    direction: WritingDirection = WritingDirection.AUTO
    script: str | None = None    # explicit OT script tag pinned through set_script_from_ot_tag; None guesses
    language: str | None = None  # explicit OT language tag pinned through set_language_from_ot_tag; None guesses
    fallback_faces: tuple[bytes, ...] = ()
    bidi_engine: BidiEngine = BidiEngine.PYTHON_BIDI
    segment_engine: SegmentEngine = SegmentEngine.UNISEG
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
        # the lane is a `_SHAPE_TABLE` row, never a smuggled `if self.step is BIDI`; the whole Shaping crosses the offload
        # (the PROCESS arms read only `params`, ignoring the shared `font` bytes), retried once on a worker death, spanned once.
        acceptor, lane = _SHAPE_TABLE[self.step]
        with _TRACER.start_as_current_span(f"shape.{self.step}") as span:
            span.set_attributes({"step": self.step, "lane": lane, "direction": self.params.direction, "backend": self.params.raster_backend})
            data = await _TRANSIENT(_ARM[lane], acceptor, self, limiter=_SHAPE_LIMITER)
        _LOG.info("shape.emit", step=self.step, lane=lane, bytes=len(data))
        return ContentIdentity.of(f"shape-{self.step}", data)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _segment(buffer: object, params: "ShapeParams", /) -> None:
    # Exemption: wiring the native hb.Buffer — pin explicit direction/script/language before `guess` fills the rest
    buffer.flags = hb.BufferFlags.PRODUCE_UNSAFE_TO_CONCAT
    if (direction := _HB_DIRECTION[params.direction]) is not None:
        buffer.direction = direction
    if params.script is not None:
        buffer.set_script_from_ot_tag(params.script)
    if params.language is not None:
        buffer.set_language_from_ot_tag(params.language)
    buffer.guess_segment_properties()


def _normalize_nfc(shaping: "Shaping") -> bytes:
    text = shaping.params.text
    match shaping.params.segment_engine:
        case SegmentEngine.ICU:
            return Normalizer2.getNFCInstance().normalize(text).encode("utf-8")
        case SegmentEngine.UNISEG:
            return unicodedata.normalize("NFC", text).encode("utf-8")
        case _ as unreachable:
            assert_never(unreachable)


def _bidi_reorder(shaping: "Shaping") -> bytes:
    params = shaping.params
    match params.bidi_engine:
        case BidiEngine.PYTHON_BIDI:
            return get_display(params.text, base_dir=_BIDI_BASE[params.direction]).encode("utf-8")
        case BidiEngine.ICU:
            engine = Bidi()
            engine.setPara(params.text, _ICU_LEVEL[params.direction])
            return engine.writeReordered(0).encode("utf-8")
        case _ as unreachable:
            assert_never(unreachable)


def _itemize_runs(shaping: "Shaping") -> bytes:
    # partition the run into (start, end, script, level) single-direction/single-script spans FONT selects a face per, SHAPE shapes per
    params = shaping.params
    match params.segment_engine:
        case SegmentEngine.ICU:
            engine = Bidi()
            engine.setPara(params.text, _ICU_LEVEL[params.direction])
            return _RUN_ENCODER.encode(tuple(
                (start, start + length, Script.getScript(ord(params.text[start])).getShortName(), level)
                for start, length, level in (engine.getVisualRun(i) for i in range(engine.countRuns()))
            ))
        case SegmentEngine.UNISEG:
            groups = tuple((script, sum(1 for _ in members)) for script, members in groupby(params.text, key=ot_script))
            prefix = tuple(accumulate((length for _, length in groups), initial=0))
            return _RUN_ENCODER.encode(tuple((prefix[i], prefix[i + 1], script, 0) for i, (script, _length) in enumerate(groups)))
        case _ as unreachable:
            assert_never(unreachable)


def _fallback_coverage(shaping: "Shaping") -> bytes:
    # probe get_nominal_glyph over the primary + fallback_faces; -1 marks a codepoint no face covers (tofu the Buffer.not_found_glyph override renders)
    params = shaping.params
    fonts = tuple(
        hb.Font.create(hb.Face.create(data, index))
        for data, index in ((shaping.font, params.face_index), *((face, 0) for face in params.fallback_faces))
    )
    assignment = tuple(
        next((rank for rank, font in enumerate(fonts) if font.get_nominal_glyph(ord(ch)) not in (0, None)), -1)
        for ch in params.text
    )
    return _RUN_ENCODER.encode(assignment)


def _shape_text(shaping: "Shaping") -> bytes:
    params = shaping.params
    font = hb.Font.create(hb.Face.create(shaping.font, params.face_index))
    if params.variations:
        font.set_variations(dict(params.variations))
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    _segment(buffer, params)
    hb.shape(font, buffer, dict(params.features) if params.features else None)
    glyphs = tuple(
        (info.codepoint, info.cluster, pos.x_advance, pos.y_advance, pos.x_offset, pos.y_offset, int(info.flags))
        for info, pos in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True)
    )
    glyph_set = TTFont(io.BytesIO(shaping.font)).getGlyphSet()
    pen, outlines = SVGPathPen(glyph_set), []
    cursor_x = cursor_y = 0
    for gid, _cluster, x_advance, y_advance, x_offset, y_offset, _flags in glyphs:  # the combined pen threads the straight baseline (a bare origin draw stacks every glyph at (0, 0)); each per-glyph pen draws at ORIGIN so `graphic/vector#VECTOR` `text_path` places it along a curved baseline
        font.draw_glyph_with_pen(gid, TransformPen(pen, (1.0, 0.0, 0.0, 1.0, cursor_x + x_offset, cursor_y + y_offset)))
        glyph_pen = SVGPathPen(glyph_set)
        font.draw_glyph_with_pen(gid, glyph_pen)
        outlines.append(glyph_pen.getCommands())
        cursor_x += x_advance
        cursor_y += y_advance
    return _RUN_ENCODER.encode(PositionedGlyphRun(glyphs=glyphs, outline=pen.getCommands(), glyph_outlines=tuple(outlines), direction=buffer.direction, script=buffer.script))


_COLOR_PROBE: Final[tuple[tuple[Callable[[object, RasterBackend], bool], ColorFormat], ...]] = (
    (lambda face, backend: face.has_color_paint and getSurfaceClass(backend.value) is not None, ColorFormat.PAINT),
    (lambda face, _backend: face.has_color_png, ColorFormat.PNG),
    (lambda face, _backend: face.has_color_svg, ColorFormat.SVG),
)


def _probe_color_format(face: object, backend: RasterBackend, /) -> ColorFormat:
    return next((fmt for probe, fmt in _COLOR_PROBE if probe(face, backend)), ColorFormat.RASTER)


def _rasterize_color(shaping: "Shaping") -> bytes:
    params = shaping.params
    face = hb.Face.create(shaping.font, params.face_index)
    hb_font = hb.Font.create(face)
    if params.variations:
        hb_font.set_variations(dict(params.variations))
    return _COLOR_TABLE[_probe_color_format(face, params.raster_backend)](shaping, face, hb_font)


def _raster_colr(shaping: "Shaping", _face: object, hb_font: object) -> bytes:
    params = shaping.params
    surface_class = getSurfaceClass(params.raster_backend.value)
    if surface_class is None:
        raise BackendUnavailableError(params.raster_backend.value)
    font = BlackRendererFont(ttFont=TTFont(io.BytesIO(shaping.font), fontNumber=0, lazy=True), hbFont=hb_font)
    if params.variations:
        font.setLocation(dict(params.variations))
    palette = font.getPalette(params.palette_index)
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    _segment(buffer, params)
    hb.shape(hb_font, buffer, dict(params.features) if params.features else None)
    glyph_line = buildGlyphLine(buffer.glyph_infos, buffer.glyph_positions, font.glyphNames)
    x_min, y_min, x_max, y_max = calcGlyphLineBounds(glyph_line, font) or (0.0, 0.0, 0.0, 0.0)
    scale, margin = params.font_size / font.unitsPerEm, params.margin
    _LOG.info("shape.raster", backend=params.raster_backend, colr=bool(font.colrV1GlyphNames), available=[name for name, _ext in listBackends()])
    surface = surface_class()
    with surface.canvas((x_min * scale - margin, y_min * scale - margin, x_max * scale + margin, y_max * scale + margin)) as canvas:
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


def _raster_png(shaping: "Shaping", _face: object, hb_font: object) -> bytes:
    buffer = hb.Buffer.create()
    buffer.add_str(shaping.params.text)
    _segment(buffer, shaping.params)
    hb.shape(hb_font, buffer, dict(shaping.params.features) if shaping.params.features else None)
    blobs = tuple(bytes(blob.data) for info in buffer.glyph_infos if (blob := hb_font.get_glyph_color_png(info.codepoint)).data)
    return blobs[0] if blobs else b""  # CBDT/sbix bitmap; graphic/raster composites a multi-glyph run


def _raster_svg(shaping: "Shaping", face: object, hb_font: object) -> bytes:
    buffer = hb.Buffer.create()
    buffer.add_str(shaping.params.text)
    _segment(buffer, shaping.params)
    hb.shape(hb_font, buffer, dict(shaping.params.features) if shaping.params.features else None)
    blobs = tuple(bytes(blob.data) for info in buffer.glyph_infos if (blob := face.get_glyph_color_svg(info.codepoint)).data)
    return blobs[0] if blobs else b""  # OT-SVG table blob; graphic/vector composes a multi-glyph run


def _raster_cpu(shaping: "Shaping", face: object, hb_font: object) -> bytes:
    # zero-native-dep BGRA32 CPU fallback when no blackrenderer backend module imports; the buffer is numpy-addressable for graphic/raster
    params = shaping.params
    buffer = hb.Buffer.create()
    buffer.add_str(params.text)
    _segment(buffer, params)
    hb.shape(hb_font, buffer, dict(params.features) if params.features else None)
    raster = hb.RasterPaint()
    raster.scale_factor = params.font_size / face.upem
    for info in buffer.glyph_infos:  # Exemption: the CPU rasterizer accumulates per-glyph paint into one RasterImage
        raster.paint_glyph(hb_font, info.codepoint)
    image = raster.render()
    return bytes(image.buffer) if image is not None else b""


def _shape_qa(shaping: "Shaping") -> bytes:
    # the hb-shape golden round-trip alongside the live production hb.shape; the diff-gated `shape(onchange=)` lookup
    # trace and the `buf_to_svg` COLRv0 visual proof compose the same temp-path Vharfbuzz for a shaping-regression oracle.
    params = shaping.params
    with tempfile.NamedTemporaryFile(suffix=".ttf", delete=False) as handle:
        sink = Path(handle.name)
    try:
        sink.write_bytes(shaping.font)
        vhb = Vharfbuzz(str(sink))
        buffer = vhb.shape(params.text, {"features": dict(params.features)} if params.features else None)
        return vhb.serialize_buf(buffer).encode("utf-8")
    finally:
        sink.unlink(missing_ok=True)


# --- [TABLES] ---------------------------------------------------------------------------
_ARM: Final[frozendict[Lane, Callable[..., object]]] = frozendict({Lane.THREAD: to_thread.run_sync, Lane.PROCESS: to_process.run_sync})
_COLOR_TABLE: Final[frozendict[ColorFormat, ColorAcceptor]] = frozendict({
    ColorFormat.PAINT: _raster_colr,
    ColorFormat.PNG: _raster_png,
    ColorFormat.SVG: _raster_svg,
    ColorFormat.RASTER: _raster_cpu,
})
_SHAPE_TABLE: Final[frozendict[ShapeOp, tuple[ShapeAcceptor, Lane]]] = frozendict({
    ShapeOp.NORMALIZE: (_normalize_nfc, Lane.PROCESS),
    ShapeOp.BIDI: (_bidi_reorder, Lane.PROCESS),
    ShapeOp.ITEMIZE: (_itemize_runs, Lane.PROCESS),
    ShapeOp.FALLBACK: (_fallback_coverage, Lane.THREAD),
    ShapeOp.SHAPE: (_shape_text, Lane.THREAD),
    ShapeOp.RASTERIZE: (_rasterize_color, Lane.THREAD),
    ShapeOp.QA: (_shape_qa, Lane.THREAD),
})
```

## [03]-[RESEARCH]

- [SHAPE] [RESOLVED]: the `Face.create` → `Font.create` → `Buffer.create`/`add_str` → `shape` → `glyph_infos`/`glyph_positions` pipeline, the `Font.set_variations(dict)` axis pinning, and the `Font.draw_glyph_with_pen(gid, pen)` outline export verify against `.api/uharfbuzz.md` (shaping rows `[06]`/`[08]`, outline row `[06]`, variation row `[10]`); the `TransformPen` per-glyph affine and `SVGPathPen.getCommands()` verify against `.api/fonttools.md` pen rows `[08]`/`[07]`. Explicit segment control is the free tier-1 gain: `_segment` sets `Buffer.direction` from `WritingDirection` (`ttb` carrying the vertical `y_advance` hb fills from `vhea`, closing the vertical gap), pins `set_script_from_ot_tag`/`set_language_from_ot_tag` when supplied (`.api/uharfbuzz.md` row `[05b]`), and sets `PRODUCE_UNSAFE_TO_CONCAT` (row `[04]`) so both break-safety flags ride the run; `info.flags` (row `[06]`, verified `hb.GlyphInfo.flags`) lands as the 7th glyph column the `PositionedGlyphRun.unsafe_to_break`/`unsafe_to_concat`/`tatweel_points` derivations read — closing the brief-named break-safety and kashida gaps, `SAFE_TO_INSERT_TATWEEL` being the Arabic-justification insertion signal. The SHAPE arm also draws each glyph to its OWN origin `SVGPathPen` into `glyph_outlines`, so `run.on_path()` (per-glyph outline + advance) is the seam value the LANDED `graphic/vector#VECTOR` `text_path(glyphs, baseline)` threads along a curved baseline (arc-length `Path.point` + tangent-following `Matrix` + `OpBuilder` union) — closing the brief-named text-on-path gap on the vector owner while shape composes it at the seam, never a `pathops` import here.
- [FALLBACK] [RESOLVED]: `Font.get_nominal_glyph(unicode)` (`.api/uharfbuzz.md` row `[04]`, verified present) probes coverage over the primary + `fallback_faces` faces and `_fallback_coverage` resolves a per-cluster covering-face assignment, `-1` marking the tofu codepoint the `Buffer.not_found_glyph` override (row `[05b]`) renders — closing the brief-named font-fallback gap so a missing glyph escalates to a secondary face rather than rendering `.notdef`.
- [ITEMIZE/NORMALIZE] [RESOLVED]: the `SegmentEngine` row picks the itemize/normalize owner per run — `Bidi.setPara`/`getVisualRun` visual-run split + `Script.getScript(cp).getShortName()` script itemization + `Normalizer2.getNFCInstance().normalize` NFC (`.api/pyicu.md` `Bidi` row `[03]`, `Script` row `[03]`, `Normalizer2` row `[01]`) behind `SegmentEngine.ICU`, and the locale-free default of a `fontTools.unicodedata.script` run-length grouping (`.api/fonttools.md` row `[07]`) + stdlib `unicodedata.normalize("NFC", ...)` — closing the brief-named script-itemization gap. PyICU is cp-gated absent on this interpreter, so the default arms are the operative path and the gated `lazy from icu import ...` reifies only when the `ICU` row runs on a provisioned build; both arms ride the `PROCESS` lane their native/PyO3 bindings require.
- [RASTERIZE] [RESOLVED]: the blackrenderer `getSurfaceClass`→`BlackRendererFont`→`drawGlyph`→`saveImage` COLRv1 traversal verifies against `.api/blackrenderer.md` font rows `[01]`-`[07]` and surface rows `[01]`/`[02]`; `_probe_color_format` reads `Face.has_color_paint`/`has_color_png`/`has_color_svg` (`.api/uharfbuzz.md` row `[03]`, all verified) and `_COLOR_TABLE` dispatches CBDT/sbix through `Font.get_glyph_color_png`, OT-SVG through `Face.get_glyph_color_svg` (row `[09]`), and the zero-native-dep fallback through `hb.RasterPaint`→`paint_glyph`→`render()` into a BGRA32 `RasterImage.buffer` (types `[03]`/`[04]`, verified `RasterPaint.paint_glyph`/`render`/`scale_factor` and `RasterImage.buffer`/`RasterFormat.BGRA32`) — closing the brief-named non-COLR-color and CPU-raster gaps so a COLRv0/CBDT/sbix/SVG face renders and a backendless deployment still rasterizes. `listBackends()` (`.api/blackrenderer.md` entry `[06]`, verified) is the deployment-matrix evidence the raster span logs.
- [OFFLOAD] [RESOLVED]: the lane is a `_SHAPE_TABLE` row — `to_thread.run_sync` for the GIL-releasing native shape/raster/QA, the gated `to_process.run_sync` for python-bidi/PyICU/uniseg — read off the row and keyed through `_ARM` under one `CapacityLimiter`, deleting the smuggled `if self.step is BIDI` lane branch the prior page carried; a worker death (`BrokenWorkerProcess`/`BrokenWorkerInterpreter`) recovers through one `stamina.AsyncRetryingCaller` bound to those types (`.api/stamina.md` caller row `[02]`, verified), and the render is spanned once (`.api/opentelemetry-api.md` `start_as_current_span`) with a `structlog` event (`.api/structlog.md`) carrying step/lane/byte-count — the universal `anyio`/`stamina`/`structlog`/`opentelemetry`/`msgspec` rails the blackrenderer `[04]` UNIVERSAL_STACK mandates, layered onto the folder shaping partners, and the `RasterImage.buffer` numpy-addressable handoff to `graphic/raster` the `numpy` tier names.
- [QA] [RESOLVED]: the `Vharfbuzz` shaping-QA companion (`.api/vharfbuzz.md`) runs a `SHAPE_QA` arm alongside the live production `hb.shape` over the same font bytes (temp-path constructed per the catalog's path-only `LOCAL_ADMISSION`): `serialize_buf` produces the `hb-shape` golden a regression asserts, the diff-gated `shape(onchange=)` traces which GSUB/GPOS lookup mutated the buffer, and `buf_to_svg` is the COLRv0 visual proof — the categorical shaping-regression oracle the production pipeline does not provide.
