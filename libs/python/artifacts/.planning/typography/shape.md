# [PY_ARTIFACTS_SHAPE]

`Shaping` owns text shaping, itemization, fallback, and color-glyph rasterization through one closed `ShapeRequest` family. `NORMALIZE`, `BIDI`, and `ITEMIZE` carry text policy alone; `FALLBACK` shapes each extended grapheme cluster against each candidate face and rejects any `.notdef` result; `SHAPE` emits glyph, cluster, advance, offset, safety, extent, baseline, style, and outline evidence; `RASTERIZE` composes COLRv1, CBDT/sbix, OT-SVG, and CPU paint into a positioned PNG run; `QA` carries a golden serialization, lookup mutation trace, and SVG proof.

Each arm's `KernelTrait` is one row on the frozen `_SHAPE_TABLE` — `RELEASING` for the GIL-releasing native shape/raster/QA/fallback, `HOSTILE` for the gated python-bidi/PyICU reorder-normalize-itemize workers whose case payloads carry no font bytes by construction — while the bidi/segment owner is a `BidiEngine`/`SegmentEngine` policy value per request and the cluster granularity a `ClusterLevel` axis, never a parallel shaping owner. `PositionedGlyphRun.source` carries the source-of-truth text beside glyph cluster values that index its code points, so `typography/layout#LAYOUT` derives paragraph text rather than accepting a parallel copy; layout also reads `UNSAFE_TO_BREAK`/`UNSAFE_TO_CONCAT` for break refusal, `SAFE_TO_INSERT_TATWEEL` for kashida, and the extents/ascender/descender/line-gap/baseline metrics for line-height and mixed-script alignment. A produced run feeds `document/emit#DOCUMENT` text placement and `composition/compose#COMPOSE` annotation; face selection, the `ScriptTags` seam, and variation location arrive from `typography/font#FONT`. Every arm keys by the runtime content key and contributes `ArtifactReceipt.Document` — save `RASTERIZE`, which contributes `ArtifactReceipt.Preview` carrying the produced pixel bounds.

## [01]-[INDEX]

- [01]-[SHAPE]: the 7-arm `ShapeRequest` shaping/itemization/rasterization owner over the frozen `_SHAPE_TABLE`, each row a `(ShapeAcceptor, KernelTrait)` pair so the crossing trait is a row property — `NORMALIZE`/`BIDI`/`ITEMIZE`/`FALLBACK`/`SHAPE`/`RASTERIZE`/`QA`, `PositionedGlyphRun` the shaped-run value object, `ItemizedRun` the itemized span, `ShapeRun` the one shaping-input owner `SHAPE`/`RASTERIZE`/`QA` share.

## [02]-[SHAPE]

- Owner: `Shaping` folds one `ShapeRequest` through the frozen `_SHAPE_TABLE`, each row a `(ShapeAcceptor, KernelTrait)` pair — the trait is a row property, never a smuggled `if` on the tag. uharfbuzz owns the OpenType layout engine, the `ot_layout_*`/`axis_infos` introspection, the coverage probe, the `GlyphFlags` signal, the CBDT/sbix+SVG color extractors, and the zero-native-dep BGRA32 CPU rasterizer; fonttools owns the binary model, the `SVGPathPen` outline, and the `fontTools.unicodedata.script` itemize fallback; blackrenderer owns the COLRv1 paint-graph rasterizer; python-bidi owns the UAX#9 default; PyICU owns the locale-aware upgrade behind the `SegmentEngine`/`BidiEngine` rows.
- Cases: seven arms on one `ShapeRequest`, each case payload closed to its arm's inputs — `normalize` carries text plus its `NormalForm` (all four UAX #15 forms, one member selecting `unicodedata.normalize` or the matching `Normalizer2.get*Instance` singleton) and engine, `bidi`/`itemize` carry text plus direction/engine values alone (a `NORMALIZE` key is untouched by font, raster, or fallback state), `fallback` carries a `FallbackSpec` face stack, and `shape`/`rasterize`/`qa` share the one `ShapeRun` shaping-input owner (`rasterize` composing it inside `RasterSpec`) so the shaping knob set has one declaration site. `ITEMIZE` enriches each single-direction/single-script span by `ScriptTags.of` at the seam after folding `Zyyy`/`Zinh` common/inherited code points onto the surrounding strong script, its ICU arm converting `getVisualRun` UTF-16 offsets onto code-point indices before intersecting script spans and its `AUTO` direction riding `UBiDiLevel.DEFAULT_LTR` first-strong autodetect; `FALLBACK` resolves a per-cluster covering face over the whole-cluster probe (`-1` marking tofu); `SHAPE` runs the one `Buffer` pipeline into a `PositionedGlyphRun`; `RASTERIZE` dispatches the `_COLOR_TABLE` row the format probe selects. `WritingDirection`/`ClusterLevel`/`RasterBackend`/`ColorFormat`/`PaletteUsage` drive the buffer and the raster surface.
- Entry: `emit()` returns the one `ArtifactWork` keyed pre-run over the request value joined with the `_toolchain()` provider generations — resolved lazily at the first key mint, an absent extra-gated provider fingerprinting as absent — so a shaping-stack upgrade re-keys rather than replaying a stale durable-cache hit; `_emit` crosses the arm over `self.lane.offload(Kernel.of(acceptor, row_trait), request)` — the offload rail is the one fallibility carrier, a provider raise converting once at that boundary — and projects the crossed `ShapedPayload` onto the per-tag receipt case.
- Auto: every arm offloads under the trait row selected by `_SHAPE_TABLE`; `HOSTILE` requests carry no font bytes. `SHAPE` emits an advance-threaded outline plus origin-drawn per-glyph outlines and reads glyph extents, font extents, baseline, and `StyleValues` from one shaped buffer. `FALLBACK` shapes complete extended grapheme clusters, so UVS, combining, regional-indicator, Indic, and ZWJ sequences share one coverage predicate. `RASTERIZE` derives every plane from two-axis HarfBuzz advances, offsets, and bearings, then composites a positioned PNG with measured bounds.
- Receipt: `ShapedPayload.encoded` projects to `ArtifactReceipt.Document`; `ShapedPayload.raster` projects to `ArtifactReceipt.Preview` with byte volume and pixel bounds. One closed payload family makes absent raster dimensions unrepresentable and threads the pre-run key captured by `emit()`.
- Packages: `uharfbuzz` owns layout, shaped-cluster coverage, metrics, color extraction, and CPU paint; `fonttools` owns pens and script resolution; `blackrenderer` owns COLRv1 paint; `python-bidi` and PyICU own UAX#9; `uniseg` owns grapheme boundaries; `vharfbuzz` owns shaping proofs; `resvg-py` rasterizes OT-SVG glyphs; Pillow composites positioned glyph planes into PNG.
- Exemption: native buffer mutation, glyph drawing, raster compositing, and trace collection are measured provider kernels; their statement loops own mutable provider objects and never escape an operation.
- Growth: a new shaping feature is one `ShapeRun.features` dict row; a new arm one `ShapeRequest` case plus one `_SHAPE_TABLE` row (the receipt and dispatch `assert_never` tails breaking until both land); a new raster backend one `RasterBackend` row; a new color format one `ColorFormat` member plus one `_COLOR_PROBE` predicate and one `_COLOR_TABLE` row; a new writing direction one `WritingDirection` member plus its `_HB_DIRECTION`/`_BIDI_BASE` rows; a new cluster granularity one `ClusterLevel` member (names mirror `hb.BufferClusterLevel`, so no table row); a new normalization form one `NormalForm` member; a new bidi/segment owner one `BidiEngine`/`SegmentEngine` member plus one arm; a new palette policy one `PaletteUsage` member; a new style read one `StyleValues` field plus one `hb.StyleTag`; a new shaped-run fact one column on the glyph tuple or one per-run field; a new itemization fact one `ItemizedRun` field; a new arm-local knob one field on that arm's case payload, never a shared bag.
- Boundary: no font subsetting/instancing (`typography/font#FONT`), no line-break/hyphenation/paragraph layout (`typography/layout#LAYOUT`, which reads the break-safety column), no PDF authoring (`document/emit#DOCUMENT`), no PAdES/PDF security (`exchange/conformance#CONFORMANCE`) — the owner shapes, itemizes, reorders, normalizes, resolves fallback, and renders glyphs, never breaking a paragraph or producing a document. Text-on-path is the landed `graphic/vector/region#REGION` `text_path` entrypoint's `skia-pathops`/`svgelements` algebra: `SHAPE` draws each glyph to its own origin pen and a curved-baseline consumer hands `run.on_path()` to `vector.text_path`, never a `pathops` import here. A uharfbuzz subsetter, a hand-rolled COLRv1 dispatch, the `renderText` one-shot (it hides the palette/glyph-bounds/backend evidence), a hand-rolled UAX#9 reorder, a hand-rolled break-class table, and a hand-coded script→OT-tag map are each rejected against blackrenderer, `bidi.get_display`, `uniseg`, `fontTools.unicodedata.script`, or the `typography/font#FONT` op that owns them; a parallel raster-backend enum, a second buffer construction, an omnibus parameter bag spanning every arm, and a smuggled lane branch collapse into the per-case payload, the `_SHAPE_TABLE` row, and the one `Buffer` pipeline.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import importlib.metadata
import io
import tempfile
import unicodedata
from builtins import frozendict
from collections.abc import Callable
from contextlib import ExitStack
from enum import StrEnum
from functools import cache, partial
from itertools import pairwise
from math import ceil
from pathlib import Path
from typing import Final, Literal, assert_never

import msgspec
import structlog
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct
from opentelemetry import trace

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait

lazy import uharfbuzz as hb
lazy from bidi import get_base_level, get_display
lazy from bidi import algorithm as bidi_algorithm
lazy from blackrenderer.backends import getSurfaceClass
lazy from blackrenderer.font import BlackRendererFont
lazy from blackrenderer.render import BackendUnavailableError, buildGlyphLine, calcGlyphLineBounds
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from fontTools.pens.transformPen import TransformPen
lazy from fontTools.ttLib import TTFont
lazy from PIL import Image
lazy from uniseg.graphemecluster import grapheme_cluster_boundaries
lazy from icu import Bidi, Normalizer2
lazy from resvg_py import svg_to_bytes
lazy from vharfbuzz import Vharfbuzz

lazy from rasm.artifacts.typography.font import ScriptTags

# --- [TYPES] ----------------------------------------------------------------------------
type FeatureSpec = frozendict[str, int | bool | tuple[tuple[int, int, int | bool], ...]]
type ShapeAcceptor = Callable[["ShapeRequest"], "ShapedPayload"]
type ColorAcceptor = Callable[["RasterSpec", object, object], "ShapedPayload"]
type ShapeTag = Literal["normalize", "bidi", "itemize", "fallback", "shape", "rasterize", "qa"]


class ShapeOp(StrEnum):
    NORMALIZE = "normalize"
    BIDI = "bidi"
    ITEMIZE = "itemize"
    FALLBACK = "fallback"
    SHAPE = "shape"
    RASTERIZE = "rasterize"
    QA = "qa"


class WritingDirection(StrEnum):
    AUTO = "auto"
    LTR = "ltr"
    RTL = "rtl"
    TTB = "ttb"


class BidiEngine(StrEnum):
    PYTHON_BIDI = "python-bidi"
    ICU = "icu"


class SegmentEngine(StrEnum):
    DEFAULT = "default"
    ICU = "icu"


class NormalForm(StrEnum):
    # member names key the provider surfaces: `unicodedata.normalize(form.value, ...)` and `Normalizer2.get<form>Instance()`.
    NFC = "NFC"
    NFD = "NFD"
    NFKC = "NFKC"
    NFKD = "NFKD"


class ClusterLevel(StrEnum):
    MONOTONE_GRAPHEMES = "MONOTONE_GRAPHEMES"
    MONOTONE_CHARACTERS = "MONOTONE_CHARACTERS"
    GRAPHEMES = "GRAPHEMES"
    CHARACTERS = "CHARACTERS"


class RasterBackend(StrEnum):
    SVG = "svg"
    SKIA = "skia"
    CAIRO = "cairo"
    COREGRAPHICS = "coregraphics"


class ColorFormat(StrEnum):
    PAINT = "paint"
    PNG = "png"
    SVG = "svg"
    RASTER = "raster"


class PaletteUsage(StrEnum):
    ANY = "any"
    LIGHT = "light"
    DARK = "dark"


# --- [CONSTANTS] ------------------------------------------------------------------------
_DEFAULT_FONT_SIZE: Final = 250.0
_DEFAULT_MARGIN: Final = 20


def _canon_hook(value: object, /) -> object:
    # frozendict is not a dict subclass, so the canonical encoder lowers `ShapeRun.variations`/`features` (and any
    # future frozendict field) to the dict projection; `order="deterministic"` restores the stable preimage.
    if isinstance(value, frozendict):
        return dict(value)
    raise NotImplementedError(f"unencodable preimage member: {type(value).__name__}")


_CANON: Final = msgspec.msgpack.Encoder(order="deterministic", enc_hook=_canon_hook)  # the stable preimage encoding the bare `ContentIdentity.key` mint addresses


def _generation(name: str, /) -> str:
    try:
        return f"{name}:{importlib.metadata.version(name)}"
    except importlib.metadata.PackageNotFoundError:
        return f"{name}:absent"  # an install-extra-gated provider fingerprints as absent, so normalization stays importable without it


@cache
def _toolchain() -> tuple[str, ...]:
    # shaped output is a function of the shaping toolchain, not the request alone: the installed provider generations
    # join the key preimage so a uharfbuzz/fontTools/blackrenderer/Pillow/python-bidi/PyICU upgrade re-keys instead of
    # replaying a stale cross-run cache hit off the durable artifact index; resolution defers to the first key mint so
    # module import never pays or trips the metadata walk.
    return tuple(_generation(name) for name in ("uharfbuzz", "fonttools", "blackrenderer", "pillow", "python-bidi", "pyicu"))
_RUN_ENCODER: Final = msgspec.msgpack.Encoder()
_UNSAFE_TO_BREAK: Final = 0x0001
_UNSAFE_TO_CONCAT: Final = 0x0002
_SAFE_TO_INSERT_TATWEEL: Final = 0x0004
_IDEOGRAPHIC_SCRIPTS: Final[frozenset[str]] = frozenset({
    "hani",
    "hang",
    "hira",
    "kana",
    "bopo",
    "yiii",
    "hant",
    "hans",
})
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
_ICU_DEFAULT_LEVEL: Final = 0xFE  # icu.UBiDiLevel.DEFAULT_LTR — first-strong autodetect; the int literal keeps the gated icu import lazy
_ICU_LEVEL: Final[Map[WritingDirection, int]] = Map.of_seq([
    (WritingDirection.AUTO, _ICU_DEFAULT_LEVEL),
    (WritingDirection.LTR, 0),
    (WritingDirection.RTL, 1),
    (WritingDirection.TTB, _ICU_DEFAULT_LEVEL),
])


# --- [MODELS] ---------------------------------------------------------------------------
class StyleValues(Struct, frozen=True):
    weight: float
    width: float
    optical_size: float
    italic: float
    slant_angle: float


class ItemizedRun(Struct, frozen=True):
    start: int
    stop: int
    script: str
    ot_tags: tuple[str, ...]
    direction: str
    level: int


class PositionedGlyphRun(Struct, frozen=True):
    # Each glyph row is `(gid, cluster, x_advance, y_advance, x_offset, y_offset, flags)`; clusters index source code points.
    source: str
    glyphs: tuple[tuple[int, int, int, int, int, int, int], ...]
    outline: str
    direction: str
    script: str
    glyph_outlines: tuple[str, ...]
    extents: tuple[tuple[int, int, int, int], ...]
    ascender: int
    descender: int
    line_gap: int
    baseline: int
    style: StyleValues

    @property
    def count(self) -> int:
        return len(self.glyphs)

    @property
    def line_height(self) -> int:
        return self.ascender - self.descender + self.line_gap

    def on_path(self) -> tuple[tuple[str, float, float, float, float], ...]:
        # CANONICAL path-placement projection: (outline, x_advance, y_advance, x_offset, y_offset) — shaped
        # placement facts survive to the Region baseline so combining marks, kerning offsets, and vertical
        # advancement lower as the SAME geometry the straight placement renders.
        return tuple(
            (outline, float(glyph[2]), float(glyph[3]), float(glyph[4]), float(glyph[5]))
            for glyph, outline in zip(self.glyphs, self.glyph_outlines, strict=True)
        )

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


@tagged_union(frozen=True)
class ShapedPayload:
    tag: Literal["encoded", "raster"] = tag()
    encoded: bytes = case()
    raster: tuple[bytes, int, int] = case()

    @property
    def data(self) -> bytes:
        match self:
            case ShapedPayload(tag="encoded", encoded=data) | ShapedPayload(tag="raster", raster=(data, _, _)):
                return data
            case _ as unreachable:
                assert_never(unreachable)


class FallbackSpec(Struct, frozen=True, kw_only=True):
    text: str
    font: bytes
    face_index: int = 0
    fallback_faces: tuple[bytes, ...] = ()


class ShapeRun(Struct, frozen=True, kw_only=True):
    text: str
    font: bytes
    face_index: int = 0
    variations: frozendict[str, float] = frozendict()
    features: FeatureSpec | None = None
    direction: WritingDirection = WritingDirection.AUTO
    script: str | None = None
    language: str | None = None
    cluster_level: ClusterLevel = ClusterLevel.MONOTONE_GRAPHEMES
    synthetic_bold: float = 0.0
    synthetic_slant: float = 0.0
    not_found_glyph: int | None = None


class RasterSpec(Struct, frozen=True, kw_only=True):
    run: ShapeRun
    raster_backend: RasterBackend = RasterBackend.SVG
    font_size: float = _DEFAULT_FONT_SIZE
    margin: int = _DEFAULT_MARGIN
    palette_index: int = 0
    palette_usage: PaletteUsage = PaletteUsage.ANY


@tagged_union(frozen=True)
class ShapeRequest:
    tag: ShapeTag = tag()
    normalize: tuple[str, NormalForm, SegmentEngine] = case()
    bidi: tuple[str, WritingDirection, BidiEngine] = case()
    itemize: tuple[str, WritingDirection, SegmentEngine] = case()
    fallback: FallbackSpec = case()
    shape: ShapeRun = case()
    rasterize: RasterSpec = case()
    qa: ShapeRun = case()

    @property
    def op(self) -> ShapeOp:
        return ShapeOp(self.tag)


class Shaping(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    request: ShapeRequest
    lane: LanePolicy

    def emit(self, /) -> ArtifactWork:
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`); the `_toolchain()`
        # generations ride the preimage beside the request so provider upgrades never replay stale shaped output.
        key = ContentIdentity.key(f"shape-{self.request.tag}", _CANON.encode((_toolchain(), self.request)))
        return ArtifactWork(key=key, work=partial(self._emit, key), parents=(), admission=Admission(keyed=None), cost=1.0)

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[ArtifactReceipt]:
        acceptor, trait = _SHAPE_TABLE[self.request.op]
        with _TRACER.start_as_current_span(f"shape.{self.request.tag}") as span:
            span.set_attributes({"step": self.request.tag, "trait": trait.value})
            crossed = await self.lane.offload(Kernel.of(acceptor, trait), self.request)
        # egress fold is asymmetric: the Error arm logs once at this boundary, the Ok path stays silent on the span.
        return crossed.map(partial(self._receipted, key)).map_error(lambda fault: _logged_fault(self.request.tag, fault))

    def _receipted(self, key: ContentKey, payload: ShapedPayload, /) -> ArtifactReceipt:
        match payload:
            case ShapedPayload(tag="raster", raster=(data, width, height)):
                return ArtifactReceipt.Preview(key, width, height, len(data))
            case ShapedPayload(tag="encoded", encoded=data):
                return ArtifactReceipt.Document(key, len(data))
            case _ as unreachable:
                assert_never(unreachable)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _logged_fault[E](step: str, fault: E, /) -> E:
    _LOG.error("shape.emit", step=step, **fault.facts())
    return fault


def _segment(buffer: object, run: ShapeRun, /) -> None:
    # Exemption: wiring the native hb.Buffer — pin explicit direction/script/language/cluster-level before `guess` fills the rest
    buffer.flags = hb.BufferFlags.PRODUCE_UNSAFE_TO_CONCAT
    buffer.cluster_level = hb.BufferClusterLevel[run.cluster_level.value]  # ClusterLevel member names mirror the provider enum
    if run.not_found_glyph is not None:
        buffer.not_found_glyph = run.not_found_glyph
    if (direction := _HB_DIRECTION[run.direction]) is not None:
        buffer.direction = direction
    if run.script is not None:
        buffer.set_script_from_ot_tag(run.script)
    if run.language is not None:
        buffer.set_language_from_ot_tag(run.language)
    buffer.guess_segment_properties()


def _shaped_buffer(run: ShapeRun, hb_font: object, /) -> object:
    buffer = hb.Buffer.create()
    buffer.add_str(run.text)
    _segment(buffer, run)
    hb.shape(hb_font, buffer, dict(run.features) if run.features else None)
    return buffer


def _normalized(request: ShapeRequest) -> ShapedPayload:
    text, form, engine = request.normalize
    match engine:
        case SegmentEngine.ICU:
            return ShapedPayload(encoded=getattr(Normalizer2, f"get{form.value}Instance")().normalize(text).encode("utf-8"))
        case SegmentEngine.DEFAULT:
            return ShapedPayload(encoded=unicodedata.normalize(form.value, text).encode("utf-8"))
        case _ as unreachable:
            assert_never(unreachable)


def _bidi_reordered(request: ShapeRequest) -> ShapedPayload:
    text, direction, engine = request.bidi
    match engine:
        case BidiEngine.PYTHON_BIDI:
            return ShapedPayload(encoded=get_display(text, base_dir=_BIDI_BASE[direction]).encode("utf-8"))
        case BidiEngine.ICU:
            resolver = Bidi()
            resolver.setPara(text, _ICU_LEVEL[direction])
            return ShapedPayload(encoded=resolver.writeReordered(0).encode("utf-8"))
        case _ as unreachable:
            assert_never(unreachable)


def _span(start: int, stop: int, script: str, level: int, /) -> ItemizedRun:
    tags = ScriptTags.of(script)
    return ItemizedRun(start=start, stop=stop, script=script, ot_tags=tags.ot_tags, direction=tags.direction, level=level)


def _from_utf16(text: str, /) -> dict[int, int]:
    # ICU offsets are UTF-16 code units; every boundary the engine yields lands on a code-point edge this table recovers.
    units, table = 0, {0: 0}
    for index, char in enumerate(text, start=1):
        units += 2 if ord(char) > 0xFFFF else 1
        table[units] = index
    return table


def _itemized(request: ShapeRequest) -> ShapedPayload:
    text, direction, engine = request.itemize
    scripts = ScriptTags.runs(text)  # contiguous per-code-point script spans, the font owner's primary — total over the input
    match engine:
        case SegmentEngine.ICU:
            resolver = Bidi()
            resolver.setPara(text, _ICU_LEVEL[direction])
            offsets = _from_utf16(text)
            visual = tuple(
                (offsets[start], offsets[start + length], level)
                for start, length, level in (resolver.getVisualRun(i) for i in range(resolver.countRuns()))
            )
            spans = tuple(
                (max(v_start, s_start), min(v_stop, s_stop), script, level)
                for v_start, v_stop, level in visual
                for s_start, s_stop, script in scripts
                if max(v_start, s_start) < min(v_stop, s_stop)
            )
        case SegmentEngine.DEFAULT:
            # UAX#9 resolved LEVEL RUNS intersect the script partition — a paragraph base level copied across
            # spans cannot represent nested or opposing embeddings, so each emitted span carries its run's real
            # resolved level from the bidi.algorithm reference pipeline (the pure-Python stage family the catalog
            # verifies: get_embedding_levels -> explicit_embed_and_overrides -> weak -> neutral -> implicit).
            storage = bidi_algorithm.get_empty_storage()
            storage["base_level"] = get_base_level(text)
            storage["base_dir"] = ("L", "R")[storage["base_level"]]
            bidi_algorithm.get_embedding_levels(text, storage)
            bidi_algorithm.explicit_embed_and_overrides(storage)
            bidi_algorithm.resolve_weak_types(storage)
            bidi_algorithm.resolve_neutral_types(storage, False)
            bidi_algorithm.resolve_implicit_levels(storage, False)
            bidi_algorithm.calc_level_runs(storage)
            levels = tuple(
                (run["start"], run["start"] + run["length"], storage["chars"][run["start"]]["level"]) for run in storage["runs"]
            )
            spans = tuple(
                (max(r_start, s_start), min(r_stop, s_stop), script, level)
                for r_start, r_stop, level in levels
                for s_start, s_stop, script in scripts
                if max(r_start, s_start) < min(r_stop, s_stop)
            )
        case _ as unreachable:
            assert_never(unreachable)
    return ShapedPayload(encoded=_RUN_ENCODER.encode(tuple(_span(start, stop, script, level) for start, stop, script, level in spans)))


def _covers_cluster(font: object, cluster: str, /) -> bool:
    buffer = hb.Buffer.create()
    buffer.add_str(cluster)
    buffer.not_found_glyph = 0
    buffer.guess_segment_properties()
    hb.shape(font, buffer)
    return bool(buffer.glyph_infos) and all(info.codepoint != 0 for info in buffer.glyph_infos)


def _fallback_coverage(request: ShapeRequest) -> ShapedPayload:
    spec = request.fallback
    fonts = tuple(
        hb.Font.create(hb.Face.create(data, index))
        for data, index in ((spec.font, spec.face_index), *((face, 0) for face in spec.fallback_faces))
    )
    clusters = tuple((start, spec.text[start:stop]) for start, stop in pairwise(grapheme_cluster_boundaries(spec.text)))
    assignment = tuple(
        (start, next((rank for rank, font in enumerate(fonts) if _covers_cluster(font, cluster)), -1)) for start, cluster in clusters
    )
    return ShapedPayload(encoded=_RUN_ENCODER.encode(assignment))


def _glyph_bbox(font: object, gid: int, /) -> tuple[int, int, int, int]:
    extents = font.get_glyph_extents(gid)
    return (extents.x_bearing, extents.y_bearing, extents.width, extents.height) if extents is not None else (0, 0, 0, 0)


def _run_baseline(font: object, script: str, direction: str, /) -> int:
    tag_ = "ideo" if script.lower()[:4] in _IDEOGRAPHIC_SCRIPTS else "romn"
    return font.get_layout_baseline(tag_, direction or "ltr", script.lower()[:4], "") or 0


def _style_values(font: object, /) -> StyleValues:
    read = font.get_style_value
    return StyleValues(
        weight=read(hb.StyleTag.WEIGHT),
        width=read(hb.StyleTag.WIDTH),
        optical_size=read(hb.StyleTag.OPTICAL_SIZE),
        italic=read(hb.StyleTag.ITALIC),
        slant_angle=read(hb.StyleTag.SLANT_ANGLE),
    )


def _styled_font(run: ShapeRun, /) -> object:
    font = hb.Font.create(hb.Face.create(run.font, run.face_index))
    if run.variations:
        font.set_variations(dict(run.variations))
    if run.synthetic_bold:
        font.synthetic_bold = (run.synthetic_bold, run.synthetic_bold)
    if run.synthetic_slant:
        font.synthetic_slant = run.synthetic_slant
    return font


def _shape_text(request: ShapeRequest) -> ShapedPayload:
    run = request.shape
    font = _styled_font(run)
    buffer = _shaped_buffer(run, font)
    glyphs = tuple(
        (info.codepoint, info.cluster, pos.x_advance, pos.y_advance, pos.x_offset, pos.y_offset, int(info.flags))
        for info, pos in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True)
    )
    # hb draw funcs decompose components, so the pens need no glyph set — no whole-font parse in the shaping hot path.
    pen, outlines = SVGPathPen(None), []
    cursor_x = cursor_y = 0
    for (
        gid,
        _cluster,
        x_advance,
        y_advance,
        x_offset,
        y_offset,
        _flags,
    ) in glyphs:
        font.draw_glyph_with_pen(gid, TransformPen(pen, (1.0, 0.0, 0.0, 1.0, cursor_x + x_offset, cursor_y + y_offset)))
        glyph_pen = SVGPathPen(None)
        font.draw_glyph_with_pen(gid, glyph_pen)
        outlines.append(glyph_pen.getCommands())
        cursor_x += x_advance
        cursor_y += y_advance
    metrics = font.get_font_extents(buffer.direction)
    return ShapedPayload(
        encoded=_RUN_ENCODER.encode(
            PositionedGlyphRun(
                source=run.text,
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
    )


_COLOR_PROBE: Final[tuple[tuple[Callable[[object, RasterBackend], bool], ColorFormat], ...]] = (
    (lambda face, backend: face.has_color_paint and getSurfaceClass(backend.value) is not None, ColorFormat.PAINT),
    (lambda face, _backend: face.has_color_png, ColorFormat.PNG),
    (lambda face, _backend: face.has_color_svg, ColorFormat.SVG),
)


def _probe_color_format(face: object, backend: RasterBackend, /) -> ColorFormat:
    return next((fmt for probe, fmt in _COLOR_PROBE if probe(face, backend)), ColorFormat.RASTER)


def _select_palette(face: object, usage: PaletteUsage, explicit: int, /) -> int:
    if usage is PaletteUsage.ANY:
        return explicit
    want = hb.OTColorPaletteFlags.USABLE_WITH_DARK_BACKGROUND if usage is PaletteUsage.DARK else hb.OTColorPaletteFlags.USABLE_WITH_LIGHT_BACKGROUND
    return next((index for index, palette in enumerate(face.color_palettes) if palette.flags & want), explicit)


def _raster_geometry(
    spec: RasterSpec,
    hb_font: object,
    buffer: object,
    upem: int,
    /,
) -> tuple[int, int, tuple[tuple[int, int, int, int, int], ...]]:
    scale = spec.font_size / (upem or 1000)
    cursor_x = cursor_y = 0.0
    font_extents = hb_font.get_font_extents(buffer.direction)
    measured = []
    for info, position in zip(buffer.glyph_infos, buffer.glyph_positions, strict=True):
        extents = hb_font.get_glyph_extents(info.codepoint)
        x_bearing, y_bearing, glyph_width, glyph_height = (
            (extents.x_bearing, extents.y_bearing, extents.width, extents.height)
            if extents is not None
            else (
                0,
                font_extents.ascender,
                position.x_advance or upem,
                position.y_advance or font_extents.descender - font_extents.ascender,
            )
        )
        x0 = (cursor_x + position.x_offset + x_bearing) * scale
        y0 = (cursor_y + position.y_offset + y_bearing) * scale
        x1, y1 = x0 + glyph_width * scale, y0 + glyph_height * scale
        measured.append((info.codepoint, min(x0, x1), max(x0, x1), min(y0, y1), max(y0, y1)))
        cursor_x += position.x_advance
        cursor_y += position.y_advance
    x_min = min((row[1] for row in measured), default=0.0)
    x_max = max((row[2] for row in measured), default=0.0)
    y_min = min((row[3] for row in measured), default=0.0)
    y_max = max((row[4] for row in measured), default=0.0)
    placements = tuple(
        (
            gid,
            round(spec.margin + left - x_min),
            round(spec.margin + y_max - top),
            max(1, ceil(right - left)),
            max(1, ceil(top - bottom)),
        )
        for gid, left, right, bottom, top in measured
    )
    return max(1, ceil(x_max - x_min) + 2 * spec.margin), max(1, ceil(y_max - y_min) + 2 * spec.margin), placements


def _rasterized(request: ShapeRequest) -> ShapedPayload:
    spec = request.rasterize
    face = hb.Face.create(spec.run.font, spec.run.face_index)
    hb_font = _styled_font(spec.run)
    return _COLOR_TABLE[_probe_color_format(face, spec.raster_backend)](spec, face, hb_font)


def _raster_colr(spec: RasterSpec, face: object, hb_font: object) -> ShapedPayload:
    surface_class = getSurfaceClass(spec.raster_backend.value)
    if surface_class is None:
        raise BackendUnavailableError(spec.raster_backend.value)
    font = BlackRendererFont(ttFont=TTFont(io.BytesIO(spec.run.font), fontNumber=spec.run.face_index, lazy=True), hbFont=hb_font)
    if spec.run.variations:
        font.setLocation(dict(spec.run.variations))
    palette = font.getPalette(_select_palette(face, spec.palette_usage, spec.palette_index))
    buffer = _shaped_buffer(spec.run, hb_font)
    glyph_line = buildGlyphLine(buffer.glyph_infos, buffer.glyph_positions, font.glyphNames)
    x_min, y_min, x_max, y_max = calcGlyphLineBounds(glyph_line, font) or (0.0, 0.0, 0.0, 0.0)
    scale, margin = spec.font_size / font.unitsPerEm, spec.margin
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
        # an all-whitespace or zero-extent glyph line collapses the bounds to nothing; each dimension floors at one
        # pixel independently — the same floor `_png_canvas` holds — so a degenerate run never mints a 0-dim payload.
        width = max(1, int((x_max - x_min) * scale + 2 * margin))
        height = max(1, int((y_max - y_min) * scale + 2 * margin))
        return ShapedPayload(raster=(sink.read_bytes(), width, height))
    finally:
        sink.unlink(missing_ok=True)


def _png_canvas(width: int, height: int, placements: tuple[tuple["Image.Image", int, int], ...], /) -> bytes:
    with ExitStack() as stack, Image.new("RGBA", (max(width, 1), max(height, 1))) as canvas:
        for plane, x, y in placements:
            stack.callback(plane.close)
            canvas.alpha_composite(plane, dest=(x, y))
        sink = io.BytesIO()
        canvas.save(sink, format="PNG")
        return sink.getvalue()


def _embedded_raster(spec: RasterSpec, face: object, hb_font: object, extract: Callable[[int], bytes], /) -> ShapedPayload:
    buffer = _shaped_buffer(spec.run, hb_font)
    width, height, geometry = _raster_geometry(spec, hb_font, buffer, face.upem)
    placements: list[tuple[Image.Image, int, int]] = []
    for gid, x, y, glyph_width, glyph_height in geometry:
        if blob := extract(gid):
            with Image.open(io.BytesIO(blob)) as decoded, decoded.convert("RGBA") as converted:
                plane = converted.resize((glyph_width, glyph_height), Image.Resampling.LANCZOS)
            placements.append((plane, x, y))
    return ShapedPayload(raster=(_png_canvas(width, height, tuple(placements)), width, height))


def _raster_png(spec: RasterSpec, face: object, hb_font: object) -> ShapedPayload:
    return _embedded_raster(
        spec,
        face,
        hb_font,
        lambda gid: bytes(blob.data) if (blob := hb_font.get_glyph_color_png(gid)) is not None and blob.data else b"",
    )


def _raster_svg(spec: RasterSpec, face: object, hb_font: object) -> ShapedPayload:
    return _embedded_raster(
        spec,
        face,
        hb_font,
        lambda gid: svg_to_bytes(svg_string=bytes(blob.data).decode("utf-8"))
        if (blob := face.get_glyph_color_svg(gid)) is not None and blob.data
        else b"",
    )


def _raster_cpu(spec: RasterSpec, face: object, hb_font: object) -> ShapedPayload:
    buffer = _shaped_buffer(spec.run, hb_font)
    width, height, geometry = _raster_geometry(spec, hb_font, buffer, face.upem)
    scale, palette = spec.font_size / (face.upem or 1000), _select_palette(face, spec.palette_usage, spec.palette_index)
    placements: list[tuple[Image.Image, int, int]] = []
    for gid, x, y, glyph_width, glyph_height in geometry:
        raster = hb.RasterPaint()
        raster.scale_factor = scale
        raster.palette = palette
        raster.paint_glyph(hb_font, gid)
        if image := raster.render():
            extents = image.extents
            plane_size = (abs(extents.width), abs(extents.height))
            if 0 in plane_size:
                continue
            with Image.frombytes("RGBA", plane_size, bytes(image.buffer), "raw", "BGRA") as raw:
                plane = raw.resize((glyph_width, glyph_height), Image.Resampling.LANCZOS)
            placements.append((plane, x, y))
    return ShapedPayload(raster=(_png_canvas(width, height, tuple(placements)), width, height))


def _shape_qa(request: ShapeRequest) -> ShapedPayload:
    # `onchange` records only lookups that mutate the buffer; `buf_to_svg` carries the corresponding visual proof.
    run = request.qa
    with tempfile.NamedTemporaryFile(suffix=".ttf", delete=False) as handle:
        sink = Path(handle.name)
    try:
        sink.write_bytes(run.font)
        vhb, mutations = Vharfbuzz(str(sink)), []

        def traced(_vhb: object, stage: str, lookup: int, _snapshot: object, /) -> None:
            mutations.append((stage, lookup))

        # golden shapes under the production run's own parameters, so a QA oracle never diverges from the SHAPE arm.
        offered = (
            ("direction", _HB_DIRECTION[run.direction]),
            ("script", run.script),
            ("language", run.language),
            ("features", dict(run.features) if run.features else None),
            ("variations", dict(run.variations) if run.variations else None),
        )
        parameters = {name: value for name, value in offered if value is not None}
        buffer = vhb.shape(run.text, parameters or None, onchange=traced)
        return ShapedPayload(
            encoded=_RUN_ENCODER.encode({"golden": vhb.serialize_buf(buffer), "trace": tuple(mutations), "proof": vhb.buf_to_svg(buffer)})
        )
    finally:
        sink.unlink(missing_ok=True)


# --- [TABLES] ---------------------------------------------------------------------------
_COLOR_TABLE: Final[Map[ColorFormat, ColorAcceptor]] = Map.of_seq([
    (ColorFormat.PAINT, _raster_colr),
    (ColorFormat.PNG, _raster_png),
    (ColorFormat.SVG, _raster_svg),
    (ColorFormat.RASTER, _raster_cpu),
])
_SHAPE_TABLE: Final[Map[ShapeOp, tuple[ShapeAcceptor, KernelTrait]]] = Map.of_seq([
    (ShapeOp.NORMALIZE, (_normalized, KernelTrait.HOSTILE)),
    (ShapeOp.BIDI, (_bidi_reordered, KernelTrait.HOSTILE)),
    (ShapeOp.ITEMIZE, (_itemized, KernelTrait.HOSTILE)),
    (ShapeOp.FALLBACK, (_fallback_coverage, KernelTrait.RELEASING)),
    (ShapeOp.SHAPE, (_shape_text, KernelTrait.RELEASING)),
    (ShapeOp.RASTERIZE, (_rasterized, KernelTrait.RELEASING)),
    (ShapeOp.QA, (_shape_qa, KernelTrait.RELEASING)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

[PYICU]-[BLOCKED]: `PyICU; python_version<'3.15'` excludes the live interpreter, and `UV_CACHE_DIR=.cache/uv uv run --frozen python -m tools.assay api resolve PyICU` reports the gate; `Bidi.setPara`/`getVisualRun`/`writeReordered`, `UBiDiLevel.DEFAULT_LTR` (the `0xFE` `_ICU_DEFAULT_LEVEL` literal), and the `Normalizer2.get{NFC,NFD,NFKC,NFKD}Instance` family therefore ride the catalog-verified surface.
