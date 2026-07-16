# [PY_ARTIFACTS_SHAPE]

`Shaping` is the text-shaping, itemization, and color-glyph rasterization owner over the document rail — one owner folding a Unicode text run and a font through a closed 7-arm `ShapeOp` family: `NORMALIZE` (UAX#15 NFC), `BIDI` (UAX#9 logical→visual reorder), `ITEMIZE` (script/direction visual-run split into typed `ItemizedRun` spans carrying the `typography/font#FONT` `ScriptTags` resolution at the seam), `FALLBACK` (per-cluster covering-face assignment over a grapheme/UVS/ZWJ-aware probe), `SHAPE` (uharfbuzz OpenType shaping into a `PositionedGlyphRun` carrying GID/cluster/advance/offset, the HarfBuzz break-safety flags, the ink-bbox and font-extents columns, the OT baseline, the resolved `StyleValues`, and the advance-threaded outline), `RASTERIZE` (color-format-probed COLRv1 / CBDT-sbix-PNG / SVG-table / CPU-BGRA32 glyph render, the CPAL palette selected by light/dark-background flag), and `QA` (vharfbuzz golden round-trip with the diff-gated per-lookup GSUB/GPOS trace).

Each arm's offload lane is one row on the frozen `_SHAPE_TABLE` — `THREAD` for the GIL-releasing native shape/raster/QA/fallback, `PROCESS` for the gated python-bidi/PyICU reorder-normalize-itemize workers — while the bidi/segment owner is a `BidiEngine`/`SegmentEngine` policy value per run and the cluster granularity a `ClusterLevel` axis, never a parallel shaping owner; the `PositionedGlyphRun` carries the `GlyphFlags` column so `typography/layout#LAYOUT` reads `UNSAFE_TO_BREAK`/`UNSAFE_TO_CONCAT` for break refusal and `SAFE_TO_INSERT_TATWEEL` for kashida, plus the extents/ascender/descender/line-gap/baseline metrics layout reads for line-height and mixed-script alignment; it feeds `document/emit#DOCUMENT` text placement and `composition/compose#COMPOSE` annotation, while face selection, the `ScriptTags` seam, and variation location arrive from `typography/font#FONT`. Every arm keys by the runtime content key and contributes one `ArtifactReceipt.Document`/`.Preview`.

## [01]-[INDEX]

- [01]-[SHAPE]: the 7-arm `ShapeOp` shaping/itemization/rasterization owner over the frozen `_SHAPE_TABLE`, each row a `(ShapeAcceptor, Lane)` pair so the offload lane is a row property — `NORMALIZE`/`BIDI`/`ITEMIZE`/`FALLBACK`/`SHAPE`/`RASTERIZE`/`QA`, `PositionedGlyphRun` the shaped-run value object and `ItemizedRun` the itemized span.

## [02]-[SHAPE]

- Owner: `Shaping` folds `(step, font, params)` through the frozen `_SHAPE_TABLE`, each row a `(ShapeAcceptor, Lane)` pair — the lane is a row property, never a smuggled `if self.step is BIDI`. uharfbuzz owns the OpenType layout engine, the `ot_layout_*`/`axis_infos` introspection, the coverage probe, the `GlyphFlags` signal, the CBDT/sbix+SVG color extractors, and the zero-native-dep BGRA32 CPU rasterizer; fonttools owns the binary model, the `SVGPathPen` outline, and the `fontTools.unicodedata.script` itemize fallback; blackrenderer owns the COLRv1 paint-graph rasterizer; python-bidi owns the UAX#9 default; PyICU owns the locale-aware upgrade behind the `SegmentEngine`/`BidiEngine` row.
- Cases: seven arms on one `ShapeOp`, each dispatched to its library-owned acceptor per the Owner split — `ITEMIZE` enriches each single-direction/single-script span by `ScriptTags.of` at the seam, `FALLBACK` resolves a per-cluster covering face over the grapheme/UVS/ZWJ probe (`-1` marking tofu), `SHAPE` runs the one `Buffer` pipeline into a `PositionedGlyphRun`, `RASTERIZE` dispatches the `_COLOR_TABLE` row the format probe selects. `WritingDirection`/`ClusterLevel`/`RasterBackend`/`ColorFormat`/`PaletteUsage` drive the buffer and the raster surface.
- Entry: `emit()` returns the one `ArtifactWork` keyed pre-run over `(step ⊕ font ⊕ params)`; `_emit` maps the crossed bytes onto `ArtifactReceipt.Document`; the `_SHAPE_TABLE` row's `Lane` picks `Modality.PROCESS`/`.THREAD`, the PROCESS arms reading only `params` and ignoring the shared font bytes, spanned once.
- Auto: every arm offloads off the event loop under one `CapacityLimiter`, its lane read off the `_SHAPE_TABLE` row; `SHAPE` draws each glyph twice — the advance-threaded combined `outline` plus the per-glyph origin outlines `run.on_path()` hands to `graphic/vector/region#REGION` `text_path` — reading extents, font extents, the OT baseline, and `StyleValues` in one pass; `FALLBACK` folds a base+marks, a UVS selector, and a ZWJ sequence to one cluster probe; `RASTERIZE` selects the CPAL palette by the light/dark-background flag, never a raw index; `QA`'s `onchange` trace fires only on a buffer-changing lookup, the regression oracle the production path omits.
- Receipt: `SHAPE`/`BIDI`/`ITEMIZE`/`NORMALIZE`/`FALLBACK`/`QA` contribute `ArtifactReceipt.Document` carrying the content key and encoded byte count; `RASTERIZE` contributes `ArtifactReceipt.Preview` carrying the pixel width/height. Resolved COLR version, chosen `ColorFormat`, selected palette and its flags, backend name, glyph count, resolved script/direction/OT-tags, the font-extents/baseline/`StyleValues` metrics, and pixel bounds stay interior evidence in the content key and the span, never new `Document`/`Preview` fields.
- Packages: `uharfbuzz` (the layout engine, `ot_layout_*`/`axis_infos` introspection, the coverage probe, the `GlyphFlags` signal, the color extractors, the `RasterPaint` CPU rasterizer), `fonttools` (`SVGPathPen`/`TransformPen`, `fontTools.unicodedata.script`), `blackrenderer` (the COLRv1 rasterizer, `listBackends`), `python-bidi`, `vharfbuzz`, `PyICU`, `uniseg` (the grapheme probe), `core/receipt#RECEIPT` (`ArtifactReceipt.Document`/`.Preview`, composed never re-declared).
- Growth: a new shaping feature is one `shape` feature-dict row; a new arm one `ShapeOp` member plus one `_SHAPE_TABLE` row; a new raster backend one `RasterBackend` row; a new color format one `ColorFormat` member plus one `_COLOR_PROBE` predicate and one `_COLOR_TABLE` row; a new writing direction one `WritingDirection` member plus its `_HB_DIRECTION`/`_BIDI_BASE` rows; a new cluster granularity one `ClusterLevel` member; a new bidi/segment owner one `BidiEngine`/`SegmentEngine` member plus one arm; a new palette policy one `PaletteUsage` member; a new style read one `StyleValues` field plus one `hb.StyleTag`; a new shaped-run fact one column on the glyph tuple or one per-run field; a new itemization fact one `ItemizedRun` field.
- Boundary: no font subsetting/instancing (`typography/font#FONT`), no line-break/hyphenation/paragraph layout (`typography/layout#LAYOUT`, which reads the break-safety column), no PDF authoring (`document/emit#DOCUMENT`), no PAdES/PDF security (`exchange/conformance#CONFORMANCE`) — the owner shapes, itemizes, reorders, normalizes, resolves fallback, and renders glyphs, never breaking a paragraph or producing a document. Text-on-path is the landed `graphic/vector/region#REGION` `text_path` entrypoint's `skia-pathops`/`svgelements` algebra: `SHAPE` draws each glyph to its own origin pen and a curved-baseline consumer hands `run.on_path()` to `vector.text_path`, never a `pathops` import here. A uharfbuzz subsetter, a hand-rolled COLRv1 dispatch, the `renderText` one-shot (it hides the palette/glyph-bounds/backend evidence), a hand-rolled UAX#9 reorder, a hand-rolled break-class table, and a hand-coded script→OT-tag map are each rejected against blackrenderer, `bidi.get_display`, `uniseg`, `fontTools.unicodedata.script`, or the `typography/font#FONT` op that owns them; a parallel `_RasterBackend` enum, a second buffer construction, and a smuggled lane branch collapse into the `_SHAPE_TABLE` row and the one `Buffer` pipeline.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import io
import os
import tempfile
import unicodedata
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from itertools import accumulate, groupby, pairwise
from pathlib import Path
from typing import Final, assert_never

import msgspec
import structlog
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
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
lazy from uniseg.graphemecluster import grapheme_cluster_boundaries  # UAX#29 extended grapheme boundaries for the per-cluster fallback probe
lazy from icu import Bidi, Normalizer2, Script  # gated PyICU upgrade behind the .ICU rows; absent, the default arms run
lazy from vharfbuzz import Vharfbuzz

lazy from artifacts.typography.font import (
    ScriptTags,
)  # the face-selection seam: script -> (OT tags, direction), composed at ITEMIZE

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
    THREAD = "thread"  # GIL-releasing native lane; the worker shares the font bytes zero-copy
    PROCESS = "process"  # gated PyO3/native-C++ lane


class WritingDirection(StrEnum):
    AUTO = "auto"
    LTR = "ltr"
    RTL = "rtl"
    TTB = "ttb"


class BidiEngine(StrEnum):
    PYTHON_BIDI = "python-bidi"  # the locale-free UAX#9 default
    ICU = "icu"  # the locale/explicit-level PyICU upgrade


class SegmentEngine(StrEnum):
    DEFAULT = "default"  # locale-free default: fontTools.unicodedata.script itemize + stdlib NFC (itemize owner is fontTools, not uniseg)
    ICU = "icu"  # the CLDR-tailored PyICU upgrade


class ClusterLevel(StrEnum):
    # hb.BufferClusterLevel names, resolved via getattr; drives caret/mark-attachment/grapheme selection
    MONOTONE_GRAPHEMES = "MONOTONE_GRAPHEMES"  # the HarfBuzz default
    MONOTONE_CHARACTERS = "MONOTONE_CHARACTERS"
    GRAPHEMES = "GRAPHEMES"
    CHARACTERS = "CHARACTERS"


class RasterBackend(StrEnum):
    SVG = "svg"
    SKIA = "skia"
    CAIRO = "cairo"
    COREGRAPHICS = "coregraphics"


class ColorFormat(StrEnum):
    PAINT = "paint"  # COLRv1 paint graph via blackrenderer
    PNG = "png"  # CBDT/sbix bitmap via Font.get_glyph_color_png
    SVG = "svg"  # OT-SVG table via Face.get_glyph_color_svg
    RASTER = "raster"  # zero-native-dep uharfbuzz RasterPaint BGRA32 CPU fallback


class PaletteUsage(StrEnum):
    ANY = "any"  # the explicit `palette_index`
    LIGHT = "light"  # first CPAL palette flagged USABLE_WITH_LIGHT_BACKGROUND
    DARK = "dark"  # first CPAL palette flagged USABLE_WITH_DARK_BACKGROUND


# --- [CONSTANTS] ------------------------------------------------------------------------
_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20
_SHAPE_SLOTS: Final[int] = os.process_cpu_count() or 4
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_UNSAFE_TO_BREAK: Final = 0x0001  # mirrors hb.GlyphFlags.UNSAFE_TO_BREAK — layout refuses a break inside the cluster
_UNSAFE_TO_CONCAT: Final = 0x0002  # mirrors hb.GlyphFlags.UNSAFE_TO_CONCAT — the run cache refuses a shaped-run splice
_SAFE_TO_INSERT_TATWEEL: Final = 0x0004  # mirrors hb.GlyphFlags.SAFE_TO_INSERT_TATWEEL — kashida tatweel-insertion points
_VARIATION_SELECTORS: Final[frozenset[int]] = frozenset(range(0xFE00, 0xFE10)) | frozenset(
    range(0xE0100, 0xE01F0)
)  # VS1-16 + VS17-256 selecting a UVS glyph variant
_IDEOGRAPHIC_SCRIPTS: Final[frozenset[str]] = frozenset({
    "hani",
    "hang",
    "hira",
    "kana",
    "bopo",
    "yiii",
    "hant",
    "hans",
})  # the `ideo` OT baseline tag; every other script reads `romn`
_LOG: Final = structlog.get_logger()
_TRACER: Final = trace.get_tracer(__name__)
_HB_DIRECTION: Final[Map[WritingDirection, str | None]] = Map.of_seq([
    (WritingDirection.AUTO, None),
    (WritingDirection.LTR, "ltr"),
    (WritingDirection.RTL, "rtl"),
    (WritingDirection.TTB, "ttb"),
])
_BIDI_BASE: Final[Map[WritingDirection, str | None]] = Map.of_seq([
    (WritingDirection.AUTO, None),
    (WritingDirection.LTR, "L"),
    (WritingDirection.RTL, "R"),
    (WritingDirection.TTB, None),
])
_ICU_LEVEL: Final[Map[WritingDirection, int]] = (
    Map.of_seq(  # 0 = LTR base, 1 = RTL (UAX#9); AUTO falls to the LTR base
        [(WritingDirection.AUTO, 0), (WritingDirection.LTR, 0), (WritingDirection.RTL, 1), (WritingDirection.TTB, 0)]
    )
)


# --- [MODELS] ---------------------------------------------------------------------------
class StyleValues(Struct, frozen=True):
    # resolved OT style-attribute values (Font.get_style_value(StyleTag)) the opsz-aware shaper carries as evidence
    weight: float = 400.0
    width: float = 100.0
    optical_size: float = 0.0
    italic: float = 0.0
    slant_angle: float = 0.0


class ItemizedRun(Struct, frozen=True):
    # one single-direction/single-script span carrying the ScriptTags resolution FONT selects a face per, SHAPE shapes per
    start: int
    stop: int
    script: str  # ISO 15924 / ICU short script code
    ot_tags: tuple[str, ...]  # OT script tags (multi for Indic v1/v2) via ScriptTags.of, never a re-derived map
    direction: str  # "LTR" / "RTL" from ScriptTags.direction
    level: int  # bidi embedding level (0 = LTR base)


class PositionedGlyphRun(Struct, frozen=True):
    # glyph = (codepoint/GID, cluster, x_advance, y_advance, x_offset, y_offset, flags); flags is the hb.GlyphFlags column layout reads
    glyphs: tuple[tuple[int, int, int, int, int, int, int], ...]
    outline: str = ""
    direction: str = "ltr"
    script: str = ""
    glyph_outlines: tuple[
        str, ...
    ] = ()  # per-glyph origin-drawn d-strings `graphic/vector/region#REGION` `text_path` threads arc-length; empty when the consumer reads `outline`
    extents: tuple[
        tuple[int, int, int, int], ...
    ] = ()  # per-glyph ink bbox (x_bearing, y_bearing, width, height) from Font.get_glyph_extents, layout reads
    ascender: int = 0  # Font.get_font_extents(direction).ascender
    descender: int = 0  # .descender (negative below the baseline)
    line_gap: int = 0  # .line_gap — the run's vertical leading layout reads for line-height
    baseline: int = (
        0  # get_layout_baseline: the per-run OT baseline a mixed Latin+CJK union aligns on (0 = no BASE table)
    )
    style: StyleValues = StyleValues()

    @property
    def count(self) -> int:
        return len(self.glyphs)

    @property
    def line_height(self) -> int:
        # the natural leading layout folds into LineBrokenRun.line_height
        return self.ascender - self.descender + self.line_gap

    def on_path(self) -> tuple[tuple[str, float], ...]:
        # each glyph's origin-drawn outline paired with its x-advance for `graphic/vector/region#REGION` `text_path`.
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
    script: str | None = None  # explicit OT script tag pinned through set_script_from_ot_tag; None guesses
    language: str | None = None  # explicit OT language tag pinned through set_language_from_ot_tag; None guesses
    fallback_faces: tuple[bytes, ...] = ()
    bidi_engine: BidiEngine = BidiEngine.PYTHON_BIDI
    segment_engine: SegmentEngine = SegmentEngine.DEFAULT
    cluster_level: ClusterLevel = (
        ClusterLevel.MONOTONE_GRAPHEMES
    )  # caret/mark-attachment/grapheme-selection granularity threaded into Buffer.cluster_level
    raster_backend: RasterBackend = RasterBackend.SVG
    synthetic_bold: float = 0.0  # faux-bold embolden ratio when the face lacks a real bold instance (Font.synthetic_bold)
    synthetic_slant: float = 0.0  # faux-italic slant when the face lacks a real italic (Font.synthetic_slant)
    font_size: float = _DEFAULT_FONT_SIZE
    margin: int = _DEFAULT_MARGIN
    palette_index: int = 0
    palette_usage: PaletteUsage = PaletteUsage.ANY  # CPAL palette selection by light/dark-background flag, not a raw index


class Shaping(Struct, frozen=True):
    step: ShapeOp
    font: bytes
    params: ShapeParams

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over (step ⊕ font ⊕ params); never a key over shaped output bytes.
        return ContentIdentity.of(f"shape-{self.step}", (self.step, self.font, self.params), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # lane is a `_SHAPE_TABLE` row; the PROCESS arms read only `params`, ignoring the shared `font` bytes.
        return (await async_boundary(f"shape.{self.step}", self._shaped)).map(
            lambda data: ArtifactReceipt.Document(self._key, len(data))
        )

    async def _shaped(self) -> bytes:
        acceptor, lane = _SHAPE_TABLE[self.step]
        with _TRACER.start_as_current_span(f"shape.{self.step}") as span:
            span.set_attributes({"step": self.step, "lane": lane, "direction": self.params.direction, "backend": self.params.raster_backend})
            modality = Modality.PROCESS if lane is Lane.PROCESS else Modality.THREAD
            crossed = await LanePolicy.offload(acceptor, self, modality=modality, retry=RetryClass.OCCT)
            data = crossed.default_with(_shape_raise)
        _LOG.info("shape.emit", step=self.step, lane=lane, bytes=len(data))
        return data


# --- [OPERATIONS] -----------------------------------------------------------------------
def _shape_raise(fault: object) -> bytes:
    raise ValueError(str(fault))


def _segment(buffer: object, params: "ShapeParams", /) -> None:
    # Exemption: wiring the native hb.Buffer — pin explicit direction/script/language/cluster-level before `guess` fills the rest
    buffer.flags = hb.BufferFlags.PRODUCE_UNSAFE_TO_CONCAT
    buffer.cluster_level = getattr(hb.BufferClusterLevel, params.cluster_level.name)
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
        case SegmentEngine.DEFAULT:
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


def _span(start: int, stop: int, script: str, level: int, /) -> ItemizedRun:
    # compose typography/font#FONT ScriptTags.of at the seam: script -> (OT tags, direction), never a re-derived script->tag map
    tags = ScriptTags.of(script)
    return ItemizedRun(start=start, stop=stop, script=script, ot_tags=tags.ot_tags, direction=tags.direction, level=level)


def _itemize_runs(shaping: "Shaping") -> bytes:
    # partition into single-direction/single-script ItemizedRun spans, each span's OT tags + direction via ScriptTags at the seam.
    params = shaping.params
    match params.segment_engine:
        case SegmentEngine.ICU:
            engine = Bidi()
            engine.setPara(params.text, _ICU_LEVEL[params.direction])
            spans = tuple(
                (start, start + length, Script.getScript(ord(params.text[start])).getShortName(), level)
                for start, length, level in (engine.getVisualRun(i) for i in range(engine.countRuns()))
            )
        case SegmentEngine.DEFAULT:
            groups = tuple((script, sum(1 for _ in members)) for script, members in groupby(params.text, key=ot_script))
            prefix = tuple(accumulate((length for _, length in groups), initial=0))
            spans = tuple((prefix[i], prefix[i + 1], script, 0) for i, (script, _length) in enumerate(groups))
        case _ as unreachable:
            assert_never(unreachable)
    return _RUN_ENCODER.encode(tuple(_span(start, stop, script, level) for start, stop, script, level in spans))


def _cluster_probes(text: str, /) -> tuple[tuple[int, int, int], ...]:
    # per grapheme cluster -> (cluster start, base codepoint, trailing variation selector or 0); uniseg owns the
    # boundary (ZWJ/Indic-conjunct/regional-indicator/combining sequences fold to one cluster), never a hand-rolled UAX#29 table.
    return tuple(
        (start, ord(text[start]), next((ord(ch) for ch in text[start + 1 : stop] if ord(ch) in _VARIATION_SELECTORS), 0))
        for start, stop in pairwise(grapheme_cluster_boundaries(text))
    )


def _covers(font: object, base: int, vs: int, /) -> bool:
    gid = font.get_variation_glyph(base, vs) if vs else font.get_nominal_glyph(base)  # UVS-selected glyph when a variation selector follows
    return gid not in (0, None)


def _fallback_coverage(shaping: "Shaping") -> bytes:
    # per-cluster covering-face assignment: (cluster-start, face-rank) over primary + fallback_faces,
    # -1 marking a cluster no face covers (tofu the Buffer.not_found_glyph override renders).
    params = shaping.params
    fonts = tuple(
        hb.Font.create(hb.Face.create(data, index))
        for data, index in ((shaping.font, params.face_index), *((face, 0) for face in params.fallback_faces))
    )
    assignment = tuple(
        (start, next((rank for rank, font in enumerate(fonts) if _covers(font, base, vs)), -1)) for start, base, vs in _cluster_probes(params.text)
    )
    return _RUN_ENCODER.encode(assignment)


def _glyph_bbox(font: object, gid: int, /) -> tuple[int, int, int, int]:
    extents = font.get_glyph_extents(gid)  # GlyphExtents: x_bearing/y_bearing/width/height (the ink box)
    return (extents.x_bearing, extents.y_bearing, extents.width, extents.height) if extents is not None else (0, 0, 0, 0)


def _run_baseline(font: object, script: str, direction: str, /) -> int:
    # per-run OT baseline: `ideo` for a CJK script, `romn` otherwise; 0 when the font carries no BASE table
    tag = "ideo" if script.lower()[:4] in _IDEOGRAPHIC_SCRIPTS else "romn"
    return font.get_layout_baseline(tag, direction or "ltr", script.lower()[:4], "") or 0


def _style_values(font: object, /) -> StyleValues:
    read = font.get_style_value  # searches the resolved variation location first, then STAT/OS2 — the opsz-aware evidence
    return StyleValues(
        weight=read(hb.StyleTag.WEIGHT),
        width=read(hb.StyleTag.WIDTH),
        optical_size=read(hb.StyleTag.OPTICAL_SIZE),
        italic=read(hb.StyleTag.ITALIC),
        slant_angle=read(hb.StyleTag.SLANT_ANGLE),
    )


def _shape_text(shaping: "Shaping") -> bytes:
    params = shaping.params
    font = hb.Font.create(hb.Face.create(shaping.font, params.face_index))
    if params.variations:
        font.set_variations(dict(params.variations))
    if params.synthetic_bold:  # faux-bold when the face lacks a real bold instance
        font.synthetic_bold = (params.synthetic_bold, params.synthetic_bold)
    if params.synthetic_slant:  # faux-italic when the face lacks a real italic
        font.synthetic_slant = params.synthetic_slant
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
    for (
        gid,
        _cluster,
        x_advance,
        y_advance,
        x_offset,
        y_offset,
        _flags,
    ) in glyphs:  # a bare origin draw stacks glyphs at (0, 0); origin-drawn pens let `graphic/vector/region#REGION` `text_path` curve the baseline
        font.draw_glyph_with_pen(gid, TransformPen(pen, (1.0, 0.0, 0.0, 1.0, cursor_x + x_offset, cursor_y + y_offset)))
        glyph_pen = SVGPathPen(glyph_set)
        font.draw_glyph_with_pen(gid, glyph_pen)
        outlines.append(glyph_pen.getCommands())
        cursor_x += x_advance
        cursor_y += y_advance
    metrics = font.get_font_extents(buffer.direction)  # FontExtents: ascender/descender/line_gap for the run's line-height
    return _RUN_ENCODER.encode(
        PositionedGlyphRun(
            glyphs=glyphs,
            outline=pen.getCommands(),
            glyph_outlines=tuple(outlines),
            direction=buffer.direction,
            script=buffer.script,
            extents=tuple(_glyph_bbox(font, glyph[0]) for glyph in glyphs),
            ascender=metrics.ascender,
            descender=metrics.descender,
            line_gap=metrics.line_gap,
            baseline=_run_baseline(font, buffer.script, buffer.direction),
            style=_style_values(font),
        )
    )


_COLOR_PROBE: Final[tuple[tuple[Callable[[object, RasterBackend], bool], ColorFormat], ...]] = (
    (lambda face, backend: face.has_color_paint and getSurfaceClass(backend.value) is not None, ColorFormat.PAINT),
    (lambda face, _backend: face.has_color_png, ColorFormat.PNG),
    (lambda face, _backend: face.has_color_svg, ColorFormat.SVG),
)


def _probe_color_format(face: object, backend: RasterBackend, /) -> ColorFormat:
    return next((fmt for probe, fmt in _COLOR_PROBE if probe(face, backend)), ColorFormat.RASTER)


def _select_palette(face: object, usage: PaletteUsage, explicit: int, /) -> int:
    # a color-glyph publication engine selects the CPAL palette by USABLE_WITH_LIGHT/DARK_BACKGROUND flag, not a raw index
    if usage is PaletteUsage.ANY:
        return explicit
    want = hb.OTColorPaletteFlags.USABLE_WITH_DARK_BACKGROUND if usage is PaletteUsage.DARK else hb.OTColorPaletteFlags.USABLE_WITH_LIGHT_BACKGROUND
    return next((index for index, palette in enumerate(face.color_palettes) if palette.flags & want), explicit)


def _rasterize_color(shaping: "Shaping") -> bytes:
    params = shaping.params
    face = hb.Face.create(shaping.font, params.face_index)
    hb_font = hb.Font.create(face)
    if params.variations:
        hb_font.set_variations(dict(params.variations))
    return _COLOR_TABLE[_probe_color_format(face, params.raster_backend)](shaping, face, hb_font)


def _raster_colr(shaping: "Shaping", face: object, hb_font: object) -> bytes:
    params = shaping.params
    surface_class = getSurfaceClass(params.raster_backend.value)
    if surface_class is None:
        raise BackendUnavailableError(params.raster_backend.value)
    font = BlackRendererFont(ttFont=TTFont(io.BytesIO(shaping.font), fontNumber=0, lazy=True), hbFont=hb_font)
    if params.variations:
        font.setLocation(dict(params.variations))
    palette = font.getPalette(_select_palette(face, params.palette_usage, params.palette_index))
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
    return blobs[0] if blobs else b""  # OT-SVG table blob; graphic/vector/region composes a multi-glyph run


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
    # the hb-shape golden round-trip with the diff-gated `shape(onchange=)` trace (vharfbuzz installs set_message_func): the
    # (stage, lookup-id) pairs record which GSUB/GPOS lookup mutated the buffer, fired only on a buffer-changing lookup.
    params = shaping.params
    with tempfile.NamedTemporaryFile(suffix=".ttf", delete=False) as handle:
        sink = Path(handle.name)
    try:
        sink.write_bytes(shaping.font)
        vhb, trace = Vharfbuzz(str(sink)), []

        def traced(_vhb: object, stage: str, lookup: int, _snapshot: object, /) -> None:
            trace.append((stage, lookup))

        buffer = vhb.shape(params.text, {"features": dict(params.features)} if params.features else None, onchange=traced)
        return _RUN_ENCODER.encode({"golden": vhb.serialize_buf(buffer), "trace": tuple(trace)})
    finally:
        sink.unlink(missing_ok=True)


# --- [TABLES] ---------------------------------------------------------------------------
_ARM: Final[Map[Lane, Callable[..., object]]] = Map.of_seq([(Lane.THREAD, to_thread.run_sync), (Lane.PROCESS, to_process.run_sync)])
_COLOR_TABLE: Final[Map[ColorFormat, ColorAcceptor]] = Map.of_seq([
    (ColorFormat.PAINT, _raster_colr),
    (ColorFormat.PNG, _raster_png),
    (ColorFormat.SVG, _raster_svg),
    (ColorFormat.RASTER, _raster_cpu),
])
_SHAPE_TABLE: Final[Map[ShapeOp, tuple[ShapeAcceptor, Lane]]] = Map.of_seq([
    (ShapeOp.NORMALIZE, (_normalize_nfc, Lane.PROCESS)),
    (ShapeOp.BIDI, (_bidi_reorder, Lane.PROCESS)),
    (ShapeOp.ITEMIZE, (_itemize_runs, Lane.PROCESS)),
    (ShapeOp.FALLBACK, (_fallback_coverage, Lane.THREAD)),
    (ShapeOp.SHAPE, (_shape_text, Lane.THREAD)),
    (ShapeOp.RASTERIZE, (_rasterize_color, Lane.THREAD)),
    (ShapeOp.QA, (_shape_qa, Lane.THREAD)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
