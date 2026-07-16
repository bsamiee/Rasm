# [PY_ARTIFACTS_FONT]

`FontEngineering` is the font-binary engineering owner over the document rail — one owner folding a font (variable or static) plus a discriminated `FontJob` into a minimized, instanced, compiled, axis-introspected, outline-metered, feature-frozen, merged, or embed-validated deliverable. Each `FontJob` case carries only its op's fields, so a `SUBSET` job never sees a merge's fonts nor a `COMPILE` job a subset's retention map. `fontTools` owns the binary model — footprint subsetting, partial-axis instancing, designspace compilation, `fvar`/`STAT` introspection, the pen outline algebra, multi-font merge, `.fea` feature authoring, and the `unicodedata` script resolver; `opentype-feature-freezer` owns the GSUB→`cmap` freeze fontTools exposes as no one-call op.

`emit()` lands the one node contract: its key mints PRE-RUN over the canonical `(source-font ⊕ job)` input bytes, `_emit` renders `apply` once across the runtime-owned offload bound, and `FontJob.receipt` threads that same key into the per-mode `ArtifactReceipt.Pdf`/`.Document` case, so keyed admission is a real cache probe and no second render mints the receipt. Downstream, the `subset`/`instance`/`merge`/`freeze`/`feature`/`compile` chain feeds `document/emit#DOCUMENT` `FONT_EMBED` the `dict[str, bytes]` face map consumed verbatim, `axis_catalog`/`ScriptTags` feed `typography/shape#SHAPE` face selection, `FaceMetrics` cap/x-height and vertical extents feed `drawing/regime#REGIME` lettering and `graphic/style#STYLE` type rows, and `EmbedReport.complete` gates the `exchange/conformance#CONFORMANCE` PDF/A close.

## [01]-[INDEX]

- [01]-[FONT]: fontTools + opentype-feature-freezer font-binary owner over the closed per-mode `FontJob` union, one total `apply` match folding every mode and `FontJob.receipt` projecting the one rendered payload onto its `ArtifactReceipt` case.

## [02]-[FONT]

- Owner: `FontEngineering` folds `(font, job)` into one deliverable; `FontJob` is the closed per-mode `@tagged_union`, `apply(font)` total over the arms by `assert_never`. fontTools owns the binary model and the `fvar`/`STAT`/`cmap`/`unicodedata` introspection; opentype-feature-freezer owns the GSUB→`cmap` freeze alone.
- Cases: nine ops on one union — `SUBSET` (footprint prune under the `subset.Options` retention map, `Options.flavor` re-flavoring to WOFF/WOFF2 in the same pass), `INSTANCE` (partial-axis instancing over the per-axis `AxisLimit` pin/range/drop map), `AXIS_CATALOG` (`fvar` axes and named instances plus the `STAT` design-axis records into `AxisCatalog`), `OUTLINE` (one `RecordingPen` traversal per glyph replayed into the SVG/area/bounds/statistics pens), `EMBED_AUDIT` (cmap coverage against the requested set plus required-table presence into `EmbedReport`), `MERGE`, `FREEZE` (GSUB single/alternate baked into the default `cmap` for non-OpenType consumers), `FEATURE` (`.fea` into GSUB/GPOS/GDEF), `COMPILE` (the designspace-to-varfont inverse of `INSTANCE`).
- Auto: `SUBSET`/`INSTANCE`/`FEATURE`/`AXIS_CATALOG`/`OUTLINE`/`EMBED_AUDIT` load and save through `io.BytesIO`; `MERGE`/`FREEZE`/`COMPILE` spill their binary inputs to temp paths (the merger, freezer, and varLib consume file paths, never bytes) and `unlink` in a `finally`. `ScriptTags.itemize` resolves the ordered-unique scripts a run carries into the per-script OT-tag+direction rows shape face-selection reads.
- Entry: `emit()` returns the one `ArtifactWork` — key minted pre-run over the `(source-font ⊕ job)` bytes under `CANONICAL_POLICY`, `work=self._emit`, `admission=Admission(keyed=None)`; `_render` runs `apply` once off-loop, then `FontJob.receipt` derives the receipt from that single payload with no second render.
- Receipt: `FontJob.receipt(key, payload)` projects the one payload onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` — the binary deliverables mint `Pdf` with the output glyph count in the page slot (read lazily off the produced binary), the catalog reads mint byte-only `Document`, and `EMBED_AUDIT` mints `Pdf` whose page slot is the covered-glyph count decoded back off its own `EmbedReport`. Axis ranges, STAT ordering, outline metrics, and cmap-closure stay interior evidence in the content key, never new receipt fields — the `Pdf` page slot double-serving glyph count is the anticipatory reuse, never a parallel `Font` case.
- Packages: `fonttools` (`subset`, `ttLib`, the `pens.*` outline algebra, `varLib.instancer`/`build`, `merge.Merger`, `feaLib`, `designspaceLib`, `unicodedata`, `fvar`/`STAT` table access), `opentype-feature-freezer` (`RemapByOTL`), `uharfbuzz` (the `FaceMetrics` OT-metrics tier), `msgspec` (the `EmbedReport` covered-count read-back), `core/receipt#RECEIPT` (`ArtifactReceipt.Pdf`/`.Document`, composed never re-declared).
- Growth: a new engineering step is one `FontJob` case plus one `apply` arm, one `receipt` arm, and one `_op` (both `assert_never` tails breaking until it lands); a new retention, instancer, freeze, axis, or outline knob is one field on its policy struct; a `COLOR` COLR/CPAL arm and a `CONVERT` CFF↔glyf re-flavor are each one case; a WOFF/WOFF2 re-flavor is the `subset.Options.flavor` row, never a parallel writer; a new face metric is one `FaceMetrics` field reading one more `OTMetricsTag`.
- Boundary: no PDF authoring (`document/emit#DOCUMENT`), no text shaping (`typography/shape#SHAPE`), no PAdES/PDF security (`exchange/conformance#CONFORMANCE`) — the owner transforms a font binary and proves it embeddable, never producing a document. A hand-walked `glyf`/`CFF`/`GSUB`/`fvar`/`STAT`/outline decode, a hand-assembled static cut or variable font, a Python-list font-merge, and a hand-coded script→OT-tag map are each rejected against the fontTools op that owns them; the uharfbuzz HarfBuzz subsetter loses to fontTools `SUBSET` for Python-native `Options` feature-policy control. A permissive `FontParams` bag, a parallel `_woff` writer, and a `dict` instancer keyword bag collapse into the per-mode `FontJob` case carrying only its op's fields.

```python signature
# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------
import io
from collections.abc import Mapping
from contextlib import ExitStack
from pathlib import Path
from tempfile import NamedTemporaryFile
from types import SimpleNamespace
from typing import Final, Literal, assert_never

import msgspec
from expression import case, tag, tagged_union
from msgspec import Struct

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality

lazy import uharfbuzz as hb
lazy from fontTools import subset, unicodedata
lazy from fontTools.designspaceLib import AxisDescriptor, DesignSpaceDocument, SourceDescriptor
lazy from fontTools.feaLib.builder import addOpenTypeFeaturesFromString
lazy from fontTools.merge import Merger
lazy from fontTools.pens.areaPen import AreaPen
lazy from fontTools.pens.boundsPen import BoundsPen
lazy from fontTools.pens.recordingPen import RecordingPen
lazy from fontTools.pens.statisticsPen import StatisticsPen
lazy from fontTools.pens.svgPathPen import SVGPathPen
lazy from fontTools.ttLib import TTFont, TTLibError
lazy from fontTools.varLib import build as build_varfont
lazy from fontTools.varLib import instancer
lazy from opentype_feature_freezer import RemapByOTL

# --- [TYPES] ---------------------------------------------------------------------------

type AxisPin = float | tuple[float, float] | None
type AxisValue = float | tuple[float | None, float | None] | None
type FontOpTag = Literal["subset", "instance", "axis_catalog", "outline", "embed_audit", "merge", "freeze", "feature", "compile"]

# --- [CONSTANTS] -----------------------------------------------------------------------

_REQUIRED_TABLES: Final[frozenset[str]] = frozenset({"cmap", "head", "hhea", "hmtx", "maxp", "name", "post"})
_HIDDEN_AXIS: Final = 0x0001  # fvar Axis flags HIDDEN_AXIS bit
_ENCODER: Final = msgspec.msgpack.Encoder()

# --- [MODELS] --------------------------------------------------------------------------


class AxisLimit(Struct, frozen=True):
    pin: float | None = None
    lower: float | None = None
    upper: float | None = None

    @staticmethod
    def of(raw: AxisPin) -> "AxisLimit":
        match raw:
            case None:
                return AxisLimit()
            case (lower, upper):
                return AxisLimit(lower=lower, upper=upper)
            case value:
                return AxisLimit(pin=value)

    def resolve(self) -> AxisValue:
        if self.pin is not None:
            return self.pin
        if self.lower is None and self.upper is None:
            return None
        return (self.lower, self.upper)


class InstancePolicy(Struct, frozen=True):
    inplace: bool = True
    optimize: bool = True
    update_font_names: bool = False

    def keywords(self) -> dict[str, bool]:
        return {"inplace": self.inplace, "optimize": self.optimize, "updateFontNames": self.update_font_names}


class FreezePolicy(Struct, frozen=True, kw_only=True):
    features: str = ""  # comma-separated GSUB tags: "smcp,c2sc,onum"
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


class AxisRecord(Struct, frozen=True):
    tag: str
    minimum: float
    default: float
    maximum: float
    hidden: bool = False


class NamedInstance(Struct, frozen=True):
    name: str
    coordinates: Mapping[str, float]


class StatAxis(Struct, frozen=True):
    tag: str  # STAT AxisTag
    name_id: int  # AxisNameID
    ordering: int  # AxisOrdering


class AxisCatalog(Struct, frozen=True):
    axes: tuple[AxisRecord, ...]  # fvar variation axes
    named_instances: tuple[NamedInstance, ...]  # fvar named instances
    design_axes: tuple[StatAxis, ...] = ()  # STAT style-attribute axes supplementing fvar

    @property
    def axis_count(self) -> int:
        return len(self.axes)


class GlyphOutline(Struct, frozen=True):
    name: str
    path: str  # SVG d-path
    area: float  # AreaPen signed contour area
    bounds: tuple[float, float, float, float]  # BoundsPen exact bbox
    slant: float  # StatisticsPen slant (outline-quality metric)


class OutlineCatalog(Struct, frozen=True):
    glyphs: tuple[GlyphOutline, ...]


class EmbedReport(Struct, frozen=True):
    requested: int
    covered: int
    glyph_count: int
    missing_tables: tuple[str, ...]

    @property
    def complete(self) -> bool:
        return self.covered == self.requested and not self.missing_tables


_REPORT_DECODER: Final = msgspec.msgpack.Decoder(EmbedReport)  # covered-count read-back off the EMBED_AUDIT payload


class ScriptTags(Struct, frozen=True):
    script: str  # ISO 15924 code, e.g. "Latn"
    ot_tags: tuple[str, ...]  # OpenType script tags, multiple for Indic v1/v2 (dev2/deva)
    direction: str  # "LTR" / "RTL"

    @staticmethod
    def of(script: str) -> "ScriptTags":
        return ScriptTags(script, tuple(unicodedata.ot_tags_from_script(script)), unicodedata.script_horizontal_direction(script))

    @staticmethod
    def itemize(text: str) -> tuple["ScriptTags", ...]:
        return tuple(ScriptTags.of(script) for script in dict.fromkeys(unicodedata.script(ch) for ch in text))


class FaceMetrics(Struct, frozen=True):
    # OT-metrics read once per face; consumers read this value, never re-parsing the binary for a metric.
    units_per_em: int
    cap_height: float  # font units — OTMetricsTag.CAP_HEIGHT (fallback-synthesized when absent)
    x_height: float  # OTMetricsTag.X_HEIGHT
    ascender: float
    descender: float

    @property
    def cap_fraction(self) -> float:
        return self.cap_height / self.units_per_em if self.units_per_em else 0.7

    def point_size(self, nominal_mm: float, /) -> float:
        # ISO 3098 nominal height is a CAP height; the em point size scales it by the em/cap ratio.
        return nominal_mm / self.cap_fraction

    @staticmethod
    def of(font: bytes, /) -> "FaceMetrics":
        face = hb.Face(font)
        hb_font = hb.Font(face)
        extents = hb_font.get_font_extents("ltr")
        cap = hb_font.get_metric_position_with_fallback(hb.OTMetricsTag.CAP_HEIGHT)
        ex = hb_font.get_metric_position_with_fallback(hb.OTMetricsTag.X_HEIGHT)
        return FaceMetrics(
            units_per_em=face.upem,
            cap_height=float(cap),
            x_height=float(ex),
            ascender=float(extents.ascender),
            descender=float(extents.descender),
        )


class MasterSource(Struct, frozen=True):
    font: bytes
    location: Mapping[str, float]


class DesignSpace(Struct, frozen=True, kw_only=True):
    axes: tuple[AxisRecord, ...]  # tag/min/default/max per variation axis
    default_location: Mapping[str, float]  # the location of the anchor `font`
    sources: tuple[MasterSource, ...]  # the additional masters


@tagged_union(frozen=True)
class FontJob:
    tag: FontOpTag = tag()
    subset: tuple[tuple[int, ...], Mapping[str, object]] = case()  # unicodes, subset.Options kwargs
    instance: tuple[Mapping[str, AxisPin], InstancePolicy] = case()
    axis_catalog: None = case()
    outline: tuple[tuple[str, ...], Mapping[str, float]] = case()  # glyph names, variation location
    embed_audit: tuple[int, ...] = case()  # requested unicodes
    merge: tuple[bytes, ...] = case()  # additional font binaries
    freeze: FreezePolicy = case()
    feature: str = case()  # .fea source
    compile: DesignSpace = case()

    def apply(self, font: bytes) -> bytes:
        match self:
            case FontJob(tag="subset", subset=(unicodes, options)):
                return _subset(font, unicodes, options)
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
            case FontJob(tag="compile", compile=space):
                return _compile(font, space)
            case _:
                assert_never(self)

    def receipt(self, key: ContentKey, payload: bytes, /) -> ArtifactReceipt:
        # per-mode projection over the one payload: binary -> Pdf(glyph count), catalog -> Document, EMBED_AUDIT -> Pdf(covered count).
        match self.tag:
            case "subset" | "instance" | "merge" | "freeze" | "feature" | "compile":
                return ArtifactReceipt.Pdf(key, len(payload), _glyph_count(payload))
            case "axis_catalog" | "outline":
                return ArtifactReceipt.Document(key, len(payload))
            case "embed_audit":
                return ArtifactReceipt.Pdf(key, len(payload), _REPORT_DECODER.decode(payload).covered)
            case _ as unreachable:
                assert_never(unreachable)


class FontEngineering(Struct, frozen=True):
    font: bytes
    job: FontJob

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key over the (source-font ⊕ job) input, minted pre-run so keyed admission probes before the transform runs.
        return ContentIdentity.of(f"font-{self.job.tag}", (self.font, self.job), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # GIL-holding apply crosses the offload bound; the pre-run key threads the receipt (receipt.slot == node.key).
        return await LanePolicy.offload(self._render, modality=Modality.INTERPRETER)

    def _render(self) -> ArtifactReceipt:
        payload = self.job.apply(self.font)  # the one produced fact the receipt reads
        return self.job.receipt(self._key, payload)


# --- [OPERATIONS] ----------------------------------------------------------------------


def _spill(data: bytes, suffix: str, /) -> Path:
    path = Path(NamedTemporaryFile(suffix=suffix, delete=False).name)
    path.write_bytes(data)  # the freezer/merger/varLib consume file paths, not bytes
    return path


def _glyph_count(font: bytes, /) -> int:
    return len(
        TTFont(io.BytesIO(font), lazy=True).getGlyphOrder()
    )  # output roster length for the Pdf page slot; a lazy header read, not a re-cut


def _subset(font: bytes, unicodes: tuple[int, ...], options: Mapping[str, object]) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    subsetter = subset.Subsetter(options=subset.Options(**options))
    subsetter.populate(unicodes=unicodes)
    subsetter.subset(ttfont)
    sink = io.BytesIO()
    ttfont.save(sink)
    return sink.getvalue()


def _instance(font: bytes, axes: Mapping[str, AxisPin], policy: InstancePolicy) -> bytes:
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
    named = tuple(NamedInstance(_instance_name(ttfont, i), dict(i.coordinates)) for i in fvar.instances)
    return _ENCODER.encode(AxisCatalog(axes=axes, named_instances=named, design_axes=_stat_axes(ttfont)))


def _outline(font: bytes, glyph_names: tuple[str, ...], location: Mapping[str, float]) -> bytes:
    glyph_set = TTFont(io.BytesIO(font)).getGlyphSet(location=dict(location) or None)
    names = glyph_names or tuple(glyph_set.keys())
    glyphs: list[GlyphOutline] = []
    for name in names:
        record, svg, area, bounds, stats = RecordingPen(), SVGPathPen(glyph_set), AreaPen(glyph_set), BoundsPen(glyph_set), StatisticsPen(glyph_set)
        glyph_set[name].draw(record)  # traverse the outline ONCE, replay into every pen
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
    covered = sum(1 for codepoint in requested if codepoint in cmap)
    missing = tuple(sorted(_REQUIRED_TABLES.difference(ttfont.keys())))
    return _ENCODER.encode(EmbedReport(requested=len(requested), covered=covered, glyph_count=len(ttfont.getGlyphOrder()), missing_tables=missing))


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
    dst = Path(NamedTemporaryFile(suffix=".otf", delete=False).name)
    try:
        engine = RemapByOTL(policy.namespace(str(src), str(dst)))
        engine.run()  # open -> GSUB->cmap remap -> rename -> save, gated on .success
        if not engine.success:  # the engine sets .success rather than raising on an unopenable/unsavable font
            raise TTLibError(f"font freeze failed: {policy.features!r}")
        return dst.read_bytes()
    finally:
        src.unlink(missing_ok=True)
        dst.unlink(missing_ok=True)


def _feature(font: bytes, source: str) -> bytes:
    ttfont = TTFont(io.BytesIO(font))
    addOpenTypeFeaturesFromString(ttfont, source)  # compile .fea into GSUB/GPOS/GDEF
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
        varfont, _model, _masters = build_varfont(document)  # the variable-font-authoring inverse of INSTANCE
        sink = io.BytesIO()
        varfont.save(sink)
        return sink.getvalue()
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
