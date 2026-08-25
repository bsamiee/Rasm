# [PY_ARTIFACTS_FONT]

`FontEngineering` is the font-binary engineering owner over the document rail — one owner folding a font plus a discriminated `FontJob` into a minimized, instanced, compiled, synthesized, axis-introspected, outline-metered, feature-frozen, feature-varied, merged, or embed-validated deliverable. Each `FontJob` case carries only its operation's fields. `fontTools` owns the binary model, typed footprint retention, partial-axis instancing, designspace compilation, whole-font synthesis, color tables, axis introspection, outline algebra, merge, feature authoring, conditional GSUB, and script resolution; `opentype-feature-freezer` owns the GSUB→`cmap` freeze.

## [01]-[INDEX]

## [02]-[FONT]

- Owner: `FontEngineering` folds `(font, job)` into one deliverable; `FontJob` is the closed per-mode `@tagged_union`, `apply(font)` total over the arms by `assert_never`. fontTools owns the binary model, synthesis, and the `fvar`/`STAT`/`cmap`/`unicodedata` introspection; opentype-feature-freezer owns the GSUB→`cmap` freeze alone.
- Cases: eleven ops on one union — `SUBSET` (footprint prune under the typed `SubsetPolicy` retention owner, `SubsetPolicy.flavor` re-flavoring to WOFF/WOFF2 in the same pass), `INSTANCE` (partial-axis instancing over the per-axis `AxisLimit` pin/range/drop map), `AXIS_CATALOG` (`fvar` axes and named instances plus the `STAT` design-axis records into `AxisCatalog`), `OUTLINE` (one `RecordingPen` traversal per glyph replayed into the SVG/area/bounds/statistics pens), `EMBED_AUDIT` (cmap coverage against the requested set plus required-table presence into `EmbedReport`), `MERGE`, `FREEZE` (GSUB single/alternate baked into the default `cmap` for non-OpenType consumers), `FEATURE` (`.fea` into GSUB/GPOS/GDEF), `FEATURE_VARIATIONS` (rvrn-style conditional GSUB rows added before instancing pins them), `COMPILE` (the designspace-to-varfont inverse of `INSTANCE`), `SYNTHESIZE` (`FontBuilder` whole-font authoring from recorded pen contours — the drawing-symbol and diagram-glyph vocabularies packaged as one embeddable face, COLRv0 layers plus CPAL palettes when color-capable).
- Auto: `SUBSET`/`INSTANCE`/`FEATURE`/`FEATURE_VARIATIONS`/`AXIS_CATALOG`/`OUTLINE`/`EMBED_AUDIT` load and save through `io.BytesIO`; `MERGE`/`FREEZE` spill their binary inputs through `_spill` — the handle closes inside its `with` block, the path outlives it for the path-only merger and freezer, and `unlink` runs in a `finally`. `ScriptTags.itemize` resolves the ordered-unique scripts a run carries into the per-script OT-tag+direction rows shape face-selection reads.
- Entry: `emit()` returns one `ArtifactWork` whose pre-run key covers `(source-font ⊕ job)` under `CANONICAL_POLICY`; `_emit` crosses `_rendered` through the `INTERPRETER` offload lane.
- Exemption: outline replay, temporary-path bracketing, designspace source assembly, and glyph synthesis are measured provider kernels; their statement loops own mutable package objects and never escape the operation.
- Growth: a new engineering operation is one `FontJob` case plus its total `apply` arm; a new retention, instancing, freeze, synthesis, or outline axis is one field on its existing policy owner. CFF↔glyf conversion lands as one `FontJob` case over `cu2qu`/`qu2cu`, while WOFF/WOFF2 remains `SubsetPolicy.flavor`.
- Boundary: no PDF authoring (`document/emit#DOCUMENT`), no text shaping (`typography/shape#SHAPE`), no PAdES/PDF security (`exchange/conformance#CONFORMANCE`) — the owner transforms or authors a font binary and proves it embeddable, never producing a document. `typography/shape#SHAPE`'s `QA` request proves shaping invariance over the produced binary after `SUBSET`/`INSTANCE`/`FREEZE` — the vharfbuzz serialization oracle lives there, never a second QA arm here. A hand-walked `glyf`/`CFF`/`GSUB`/`fvar`/`STAT`/outline decode, a hand-assembled static cut or variable font, a Python-list font-merge, hand-built COLR/CPAL tables, and a hand-coded script→OT-tag map are each rejected against the fontTools op that owns them; the uharfbuzz HarfBuzz subsetter loses to fontTools `SUBSET` for Python-native `Options` feature-policy control. A permissive `Mapping[str, object]` option bag, a parallel `_woff` writer, and a `dict` instancer keyword bag collapse into the per-mode `FontJob` case carrying only its op's typed fields.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import io
import math
from builtins import frozendict
from contextlib import ExitStack
from enum import StrEnum
from itertools import accumulate, groupby, islice
from pathlib import Path
from tempfile import NamedTemporaryFile
from types import SimpleNamespace
from typing import Final, Literal, assert_never

import msgspec
from expression import Result, case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentIdentity
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait

lazy import uharfbuzz as hb
lazy from fontTools import subset, unicodedata
lazy from fontTools.designspaceLib import AxisDescriptor, DesignSpaceDocument, SourceDescriptor
lazy from fontTools.feaLib.builder import addOpenTypeFeaturesFromString
lazy from fontTools.fontBuilder import FontBuilder
lazy from fontTools.merge import Merger
lazy from fontTools.pens.areaPen import AreaPen
lazy from fontTools.pens.boundsPen import BoundsPen
lazy from fontTools.pens.recordingPen import RecordingPen
lazy from fontTools.pens.statisticsPen import StatisticsPen
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from fontTools.pens.ttGlyphPen import TTGlyphPen
lazy from fontTools.ttLib import TTFont, TTLibError
lazy from fontTools.varLib import build as build_varfont
lazy from fontTools.varLib import instancer
lazy from fontTools.varLib.featureVars import addFeatureVariations
lazy from opentype_feature_freezer import RemapByOTL

# --- [TYPES] ----------------------------------------------------------------------------

type AxisPin = float | tuple[float, float] | None
type AxisValue = float | tuple[float | None, float | None] | None
type Point = tuple[float, float]
type Affine = tuple[float, float, float, float, float, float]
type FontOpTag = Literal[
    "subset",
    "instance",
    "axis_catalog",
    "outline",
    "embed_audit",
    "merge",
    "freeze",
    "feature",
    "feature_variations",
    "compile",
    "synthesize",
]


class FontFlavor(StrEnum):
    SFNT = "sfnt"
    WOFF = "woff"
    WOFF2 = "woff2"


@tagged_union(frozen=True)
class PenCommand:
    tag: Literal["move", "line", "quadratic", "cubic", "close", "end", "component"] = tag()
    move: Point = case()
    line: Point = case()
    quadratic: tuple[Point, ...] = case()
    cubic: tuple[Point, ...] = case()
    close: None = case()
    end: None = case()
    component: tuple[str, Affine] = case()

    def replay(self, pen: "TTGlyphPen", /) -> None:
        match self:
            case PenCommand(tag="move", move=point):
                pen.moveTo(point)
            case PenCommand(tag="line", line=point):
                pen.lineTo(point)
            case PenCommand(tag="quadratic", quadratic=points):
                pen.qCurveTo(*points)
            case PenCommand(tag="cubic", cubic=points):
                pen.curveTo(*points)
            case PenCommand(tag="close"):
                pen.closePath()
            case PenCommand(tag="end"):
                pen.endPath()
            case PenCommand(tag="component", component=(name, transform)):
                pen.addComponent(name, transform)
            case _ as unreachable:
                assert_never(unreachable)


# --- [CONSTANTS] ------------------------------------------------------------------------

_REQUIRED_TABLES: Final[frozenset[str]] = frozenset({"cmap", "head", "hhea", "hmtx", "maxp", "name", "post"})
_COMMON_SCRIPTS: Final[frozenset[str]] = frozenset({"Zinh", "Zyyy", "Zzzz"})
_HIDDEN_AXIS: Final = 0x0001
_RESTRICTED_EMBED: Final = 0x0202
_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic", enc_hook=dict)

# --- [MODELS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class AxisLimit:
    tag: Literal["drop", "pin", "range"] = tag()
    drop: None = case()
    pin: float = case()
    range: tuple[float, float] = case()

    @staticmethod
    def of(raw: AxisPin) -> "AxisLimit":
        match raw:
            case None:
                return AxisLimit(drop=None)
            case (lower, upper) if not (math.isfinite(lower) and math.isfinite(upper)) or lower > upper:
                raise ValueError(f"<axis-limit:range:{lower},{upper}>")
            case (lower, upper):
                return AxisLimit(range=(lower, upper))
            case value if not math.isfinite(value):
                raise ValueError(f"<axis-limit:pin:{value}>")
            case value:
                return AxisLimit(pin=value)

    def resolve(self) -> AxisValue:
        match self:
            case AxisLimit(tag="drop"):
                return None
            case AxisLimit(tag="range", range=(lower, upper)):
                return (lower, upper)
            case AxisLimit(tag="pin", pin=pin):
                return pin
            case _ as unreachable:
                assert_never(unreachable)


class SubsetPolicy(Struct, frozen=True, kw_only=True):
    unicodes: tuple[int, ...] = ()
    text: str = ""
    layout_features: tuple[str, ...] | None = None
    flavor: FontFlavor = FontFlavor.SFNT
    hinting: bool = True
    retain_gids: bool = False
    glyph_names: bool = False
    desubroutinize: bool = False
    drop_tables: tuple[str, ...] = ()
    name_ids: tuple[int, ...] | None = None
    harfbuzz_repacker: bool = False
    with_zopfli: bool = False

    def options(self) -> "subset.Options":
        selected = {"layout_features": self.layout_features, "drop_tables": self.drop_tables or None, "name_IDs": self.name_ids}
        return subset.Options(
            hinting=self.hinting,
            retain_gids=self.retain_gids,
            glyph_names=self.glyph_names,
            desubroutinize=self.desubroutinize,
            harfbuzz_repacker=self.harfbuzz_repacker,
            with_zopfli=self.with_zopfli,
            flavor=None if self.flavor is FontFlavor.SFNT else self.flavor.value,
            **{name: list(value) for name, value in selected.items() if value is not None},
        )


class InstancePolicy(Struct, frozen=True):
    inplace: bool = True
    optimize: bool = True
    update_font_names: bool = False

    def keywords(self) -> dict[str, bool]:
        return {"inplace": self.inplace, "optimize": self.optimize, "updateFontNames": self.update_font_names}


class FreezePolicy(Struct, frozen=True, kw_only=True):
    features: str = ""
    script: str | None = None
    lang: str | None = None
    suffix: bool = False
    usesuffix: str = ""
    replacenames: str = ""
    zapnames: bool = False
    info: bool = False

    def namespace(self, inpath: str, outpath: str) -> object:
        return SimpleNamespace(
            inpath=inpath,
            outpath=outpath,
            features=self.features,
            script=self.script,
            lang=self.lang,
            suffix=self.suffix,
            usesuffix=self.usesuffix,
            replacenames=self.replacenames,
            zapnames=self.zapnames,
            info=self.info,
            report=False,
            names=False,
        )


class FeatureVariation(Struct, frozen=True):
    region: tuple[frozendict[str, tuple[float, float]], ...]
    substitution: frozendict[str, str]


class AxisRecord(Struct, frozen=True):
    tag: str
    minimum: float
    default: float
    maximum: float
    hidden: bool = False


class NamedInstance(Struct, frozen=True):
    name: str
    coordinates: frozendict[str, float]


class StatAxis(Struct, frozen=True):
    tag: str
    name_id: int
    ordering: int


class AxisCatalog(Struct, frozen=True):
    axes: tuple[AxisRecord, ...]
    named_instances: tuple[NamedInstance, ...]
    design_axes: tuple[StatAxis, ...] = ()

    @property
    def axis_count(self) -> int:
        return len(self.axes)


class GlyphOutline(Struct, frozen=True):
    name: str
    path: str
    area: float
    bounds: tuple[float, float, float, float]
    slant: float


class OutlineCatalog(Struct, frozen=True):
    glyphs: tuple[GlyphOutline, ...]


class EmbedReport(Struct, frozen=True):
    requested: int
    covered: int
    glyph_count: int
    missing_unicodes: tuple[int, ...]
    missing_tables: tuple[str, ...]
    embedding_bits: int
    layout_tables: tuple[str, ...]
    variation_tables: tuple[str, ...]
    color_tables: tuple[str, ...]

    @property
    def complete(self) -> bool:
        return self.covered == self.requested and not self.missing_tables and not self.embedding_bits & _RESTRICTED_EMBED


_REPORT_DECODER: Final = msgspec.msgpack.Decoder(EmbedReport)


class ScriptTags(Struct, frozen=True):
    script: str
    ot_tags: tuple[str, ...]
    direction: str

    @staticmethod
    def of(script: str) -> "ScriptTags":
        return ScriptTags(script, tuple(unicodedata.ot_tags_from_script(script)), unicodedata.script_horizontal_direction(script))

    @staticmethod
    def runs(text: str) -> tuple[tuple[int, int, str], ...]:
        raw = tuple(unicodedata.script(ch) for ch in text)
        seed = next((script for script in raw if script not in _COMMON_SCRIPTS), "Latn")
        folded = islice(accumulate(raw, lambda held, seen: held if seen in _COMMON_SCRIPTS else seen, initial=seed), 1, None)
        spans = tuple((script, sum(1 for _ in members)) for script, members in groupby(folded))
        prefix = tuple(accumulate((length for _, length in spans), initial=0))
        return tuple((prefix[index], prefix[index + 1], script) for index, (script, _length) in enumerate(spans))

    @staticmethod
    def resolve(text: str) -> tuple[str, ...]:
        return tuple(dict.fromkeys(script for _start, _stop, script in ScriptTags.runs(text)))

    @staticmethod
    def itemize(text: str) -> tuple["ScriptTags", ...]:
        return tuple(ScriptTags.of(script) for script in ScriptTags.resolve(text))


class FaceMetrics(Struct, frozen=True):
    units_per_em: int
    cap_height: float
    x_height: float
    ascender: float
    descender: float
    line_gap: float
    underline_offset: float
    underline_size: float
    strikeout_offset: float
    strikeout_size: float

    @property
    def cap_fraction(self) -> float:
        return self.cap_height / self.units_per_em if self.units_per_em else 0.7

    def point_size(self, nominal_mm: float, /) -> float:
        return nominal_mm / self.cap_fraction

    @staticmethod
    def of(font: bytes, /) -> "FaceMetrics":
        face = hb.Face.create(font, 0)
        hb_font = hb.Font.create(face)
        extents = hb_font.get_font_extents("ltr")
        position = hb_font.get_metric_position_with_fallback
        return FaceMetrics(
            units_per_em=face.upem,
            cap_height=float(position(hb.OTMetricsTag.CAP_HEIGHT)),
            x_height=float(position(hb.OTMetricsTag.X_HEIGHT)),
            ascender=float(extents.ascender),
            descender=float(extents.descender),
            line_gap=float(extents.line_gap),
            underline_offset=float(position(hb.OTMetricsTag.UNDERLINE_OFFSET)),
            underline_size=float(position(hb.OTMetricsTag.UNDERLINE_SIZE)),
            strikeout_offset=float(position(hb.OTMetricsTag.STRIKEOUT_OFFSET)),
            strikeout_size=float(position(hb.OTMetricsTag.STRIKEOUT_SIZE)),
        )


class MasterSource(Struct, frozen=True):
    font: bytes
    location: frozendict[str, float]


class DesignSpace(Struct, frozen=True, kw_only=True):
    axes: tuple[AxisRecord, ...]
    default_location: frozendict[str, float]
    sources: tuple[MasterSource, ...]


class SynthGlyph(Struct, frozen=True, kw_only=True):
    name: str
    codepoint: int = 0
    advance: int = 0
    vertical_advance: int = 0
    contours: tuple[PenCommand, ...] = ()
    layers: tuple[tuple[str, int], ...] = ()


class FontSynthesis(Struct, frozen=True, kw_only=True):
    family: str
    style: str = "Regular"
    version: str = "1.000"
    upem: int = 1000
    ascent: int = 800
    descent: int = -200
    line_gap: int = 0
    weight_class: int = 400
    width_class: int = 5
    italic_angle: float = 0.0
    glyphs: tuple[SynthGlyph, ...] = ()
    palettes: tuple[tuple[tuple[float, float, float, float], ...], ...] = ()


@tagged_union(frozen=True)
class FontJob:
    tag: FontOpTag = tag()
    subset: SubsetPolicy = case()
    instance: tuple[frozendict[str, AxisPin], InstancePolicy] = case()
    axis_catalog: None = case()
    outline: tuple[tuple[str, ...], frozendict[str, float]] = case()
    embed_audit: tuple[int, ...] = case()
    merge: tuple[bytes, ...] = case()
    freeze: FreezePolicy = case()
    feature: str = case()
    feature_variations: tuple[FeatureVariation, ...] = case()
    compile: DesignSpace = case()
    synthesize: FontSynthesis = case()

    def apply(self, font: bytes) -> bytes:
        match self:
            case FontJob(tag="subset", subset=policy):
                return _subset(font, policy)
            case FontJob(tag="instance", instance=(axes, policy)):
                return _instance(font, axes, policy)
            case FontJob(tag="axis_catalog"):
                return _axis_catalog(font)
            case FontJob(tag="outline", outline=(names, location)):
                return _outline(font, names, location)
            case FontJob(tag="embed_audit", embed_audit=unicodes):
                return _embed_audit(font, unicodes)
            case FontJob(tag="merge", merge=extras):
                return _merge(font, extras)
            case FontJob(tag="freeze", freeze=policy):
                return _freeze(font, policy)
            case FontJob(tag="feature", feature=source):
                return _feature(font, source)
            case FontJob(tag="feature_variations", feature_variations=rows):
                return _feature_variations(font, rows)
            case FontJob(tag="compile", compile=space):
                return _compile(font, space)
            case FontJob(tag="synthesize", synthesize=spec):
                return _synthesize(spec)
            case _:
                assert_never(self)

class FontEngineering(Struct, frozen=True):
    font: bytes
    job: FontJob
    lane: LanePolicy

    def emit(self, /) -> ArtifactWork[bytes]:
        key = ContentIdentity.key(f"font-{self.job.tag}", _ENCODER.encode((self.font, self.job)))
        return ArtifactWork(key=key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    async def _emit(self, /) -> RuntimeRail[bytes]:
        rendered = await self.lane.offload(Kernel.of(_rendered, KernelTrait.PURE), self.font, self.job)
        match rendered:
            case Result(tag="ok", ok=payload):
                Metrics.record({BYTE_VOLUME: float(len(payload))}, domain=DOMAIN, kind="document", scope=self.lane.scope)
        return rendered


# --- [OPERATIONS] -----------------------------------------------------------------------


def _rendered(font: bytes, job: FontJob, /) -> bytes:
    return job.apply(font)


def _spill(data: bytes, suffix: str, /) -> Path:
    with NamedTemporaryFile(suffix=suffix, delete=False) as handle:
        handle.write(data)
        return Path(handle.name)


def _subset(font: bytes, policy: SubsetPolicy) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    subsetter = subset.Subsetter(options=policy.options())
    subsetter.populate(unicodes=list(policy.unicodes), text=policy.text)
    subsetter.subset(ttfont)
    sink = io.BytesIO()
    ttfont.save(sink)
    return sink.getvalue()


def _instance(font: bytes, axes: frozendict[str, AxisPin], policy: InstancePolicy) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    limits: dict[str, AxisValue] = {tag: AxisLimit.of(raw).resolve() for tag, raw in axes.items()}
    instance = instancer.instantiateVariableFont(ttfont, instancer.AxisLimits(limits), **policy.keywords())
    sink = io.BytesIO()
    instance.save(sink)
    return sink.getvalue()


def _stat_axes(ttfont: object) -> tuple[StatAxis, ...]:
    if "STAT" not in ttfont or (records := ttfont["STAT"].table.DesignAxisRecord) is None:
        return ()
    return tuple(StatAxis(rec.AxisTag, rec.AxisNameID, rec.AxisOrdering) for rec in records.Axis)


def _instance_name(ttfont: object, instance: object) -> str:
    record = ttfont["name"].getDebugName(instance.subfamilyNameID)
    return record if record is not None else f"instance-{instance.subfamilyNameID}"


def _axis_catalog(font: bytes) -> bytes:
    ttfont = TTFont(io.BytesIO(font), lazy=True)
    fvar = ttfont["fvar"]
    axes = tuple(AxisRecord(a.axisTag, a.minValue, a.defaultValue, a.maxValue, bool(a.flags & _HIDDEN_AXIS)) for a in fvar.axes)
    named = tuple(NamedInstance(_instance_name(ttfont, i), frozendict(i.coordinates)) for i in fvar.instances)
    return _ENCODER.encode(AxisCatalog(axes=axes, named_instances=named, design_axes=_stat_axes(ttfont)))


def _outline(font: bytes, glyph_names: tuple[str, ...], location: frozendict[str, float]) -> bytes:
    glyph_set = TTFont(io.BytesIO(font)).getGlyphSet(location=dict(location) or None)
    names = glyph_names or tuple(glyph_set.keys())
    glyphs: list[GlyphOutline] = []
    for name in names:
        record, svg, area, bounds, stats = RecordingPen(), SVGPathPen(glyph_set), AreaPen(glyph_set), BoundsPen(glyph_set), StatisticsPen(glyph_set)
        glyph_set[name].draw(record)
        for pen in (svg, area, bounds, stats):
            record.replay(pen)
        glyphs.append(
            GlyphOutline(name=name, path=svg.getCommands(), area=area.value, bounds=bounds.bounds or (0.0, 0.0, 0.0, 0.0), slant=stats.slant)
        )
    return _ENCODER.encode(OutlineCatalog(glyphs=tuple(glyphs)))


def _embed_audit(font: bytes, unicodes: tuple[int, ...]) -> bytes:
    ttfont = TTFont(io.BytesIO(font), lazy=True)
    cmap = ttfont.getBestCmap()
    requested = frozenset(unicodes) or frozenset(cmap.keys())
    missing_unicodes = tuple(sorted(requested.difference(cmap)))
    missing = tuple(sorted(_REQUIRED_TABLES.difference(ttfont.keys())))
    embedding_bits = int(ttfont["OS/2"].fsType) if "OS/2" in ttfont else 0
    return _ENCODER.encode(
        EmbedReport(
            requested=len(requested),
            covered=len(requested) - len(missing_unicodes),
            glyph_count=len(ttfont.getGlyphOrder()),
            missing_unicodes=missing_unicodes,
            missing_tables=missing,
            embedding_bits=embedding_bits,
            layout_tables=tuple(tag for tag in ("BASE", "GDEF", "GPOS", "GSUB", "JSTF", "MATH") if tag in ttfont),
            variation_tables=tuple(tag for tag in ("avar", "cvar", "fvar", "gvar", "HVAR", "MVAR", "STAT", "VVAR") if tag in ttfont),
            color_tables=tuple(tag for tag in ("CBDT", "CBLC", "COLR", "CPAL", "sbix", "SVG ") if tag in ttfont),
        )
    )


def _merge(font: bytes, extras: tuple[bytes, ...]) -> bytes:
    paths = [_spill(data, ".ttf") for data in (font, *extras)]
    try:
        merged = Merger().merge([str(path) for path in paths])
        sink = io.BytesIO()
        merged.save(sink)
        return sink.getvalue()
    finally:
        for path in paths:
            path.unlink(missing_ok=True)


def _freeze(font: bytes, policy: FreezePolicy) -> bytes:
    src = _spill(font, ".otf")
    dst = _spill(b"", ".otf")
    try:
        engine = RemapByOTL(policy.namespace(str(src), str(dst)))
        engine.run()
        if not engine.success:
            raise TTLibError(f"font freeze failed: {policy.features!r}")
        return dst.read_bytes()
    finally:
        src.unlink(missing_ok=True)
        dst.unlink(missing_ok=True)


def _feature(font: bytes, source: str) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    addOpenTypeFeaturesFromString(ttfont, source)
    sink = io.BytesIO()
    ttfont.save(sink)
    return sink.getvalue()


def _feature_variations(font: bytes, rows: tuple[FeatureVariation, ...]) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    conditional = [([dict(box) for box in row.region], dict(row.substitution)) for row in rows]
    addFeatureVariations(ttfont, conditional)
    sink = io.BytesIO()
    ttfont.save(sink)
    return sink.getvalue()


def _compile(font: bytes, space: DesignSpace) -> bytes:
    with ExitStack() as stack:
        document = DesignSpaceDocument()
        for axis in space.axes:
            document.addAxis(AxisDescriptor(tag=axis.tag, name=axis.tag, minimum=axis.minimum, default=axis.default, maximum=axis.maximum))
        masters = ((font, space.default_location), *((source.font, source.location) for source in space.sources))
        for data, location in masters:
            handle = stack.enter_context(NamedTemporaryFile(suffix=".ttf"))
            handle.write(data)
            handle.flush()
            document.addSource(SourceDescriptor(path=handle.name, location=dict(location)))
        varfont, _model, _masters = build_varfont(document)
        sink = io.BytesIO()
        varfont.save(sink)
        return sink.getvalue()


def _synthesize(spec: FontSynthesis) -> bytes:
    names = tuple(glyph.name for glyph in spec.glyphs)
    if len(set(names)) != len(names) or ".notdef" in names:
        raise ValueError("<synthesize:glyph-name>")
    mapped = tuple(glyph.codepoint for glyph in spec.glyphs if glyph.codepoint)
    if len(set(mapped)) != len(mapped):
        raise ValueError("<synthesize:codepoint>")
    if any(not (0 < code <= 0x10FFFF) or 0xD800 <= code <= 0xDFFF for code in mapped):
        raise ValueError("<synthesize:codepoint-range>")
    replayed: dict[str, object] = {".notdef": TTGlyphPen(None).glyph()}
    metrics: dict[str, tuple[int, int]] = {".notdef": (spec.upem // 2, 0)}
    for glyph in spec.glyphs:
        pen = TTGlyphPen(None)
        for command in glyph.contours:
            command.replay(pen)
        replayed[glyph.name] = pen.glyph()
        metrics[glyph.name] = (glyph.advance, 0)
    builder = FontBuilder(spec.upem, isTTF=True)
    builder.setupGlyphOrder([".notdef", *(glyph.name for glyph in spec.glyphs)])
    builder.setupCharacterMap({glyph.codepoint: glyph.name for glyph in spec.glyphs if glyph.codepoint})
    builder.setupGlyf(replayed)
    builder.setupHorizontalMetrics(metrics)
    builder.setupHorizontalHeader(ascent=spec.ascent, descent=spec.descent, lineGap=spec.line_gap)
    if any(glyph.vertical_advance for glyph in spec.glyphs):
        builder.setupVerticalMetrics({".notdef": (spec.upem, 0), **{glyph.name: (glyph.vertical_advance, 0) for glyph in spec.glyphs}})
        builder.setupVerticalHeader(ascent=spec.ascent, descent=spec.descent, lineGap=spec.line_gap)
    ps_name = f"{spec.family}-{spec.style}".replace(" ", "-")
    builder.setupNameTable(
        {
            "familyName": spec.family,
            "styleName": spec.style,
            "uniqueFontIdentifier": f"{ps_name};{spec.version}",
            "fullName": f"{spec.family} {spec.style}",
            "psName": ps_name,
            "version": spec.version,
        }
    )
    builder.setupOS2(
        sTypoAscender=spec.ascent,
        sTypoDescender=spec.descent,
        sTypoLineGap=spec.line_gap,
        usWinAscent=max(spec.ascent, 0),
        usWinDescent=max(-spec.descent, 0),
        usWeightClass=spec.weight_class,
        usWidthClass=spec.width_class,
    )
    builder.setupPost(italicAngle=spec.italic_angle)
    if colr := {glyph.name: list(glyph.layers) for glyph in spec.glyphs if glyph.layers}:
        depth = min((len(palette) for palette in spec.palettes), default=0)
        if depth == 0 or any(not 0 <= index < depth for layers in colr.values() for _layer, index in layers):
            raise ValueError("<synthesize:palette-index>")
        builder.setupCOLR(colr)
        builder.setupCPAL([list(palette) for palette in spec.palettes])
    sink = io.BytesIO()
    builder.font.save(sink)
    return sink.getvalue()
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
