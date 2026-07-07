# [PY_ARTIFACTS_FONT]

The font-binary engineering owner over the document rail. `FontEngineering` is ONE owner that takes a font (variable or static) plus a discriminated `FontJob` and folds it into a minimized, instanced, compiled, axis-introspected, outline-metered, feature-frozen, merged, or embed-validated deliverable. The job is a closed per-mode `@tagged_union` — each case carries ONLY its own fields, so a `SUBSET` job never sees a merge's extra fonts and a `COMPILE` job never sees a subset's retention map, collapsing the prior permissive `FontParams` bag whose ten fields were irrelevant to most ops. `fontTools` owns the binary model (`subset.Subsetter` footprint pruning, `varLib.instancer.instantiateVariableFont` partial-axis instancing, `varLib.build` variable-font compilation from a designspace, the `fvar`/`STAT` axis introspection, the `pens` outline algebra, `merge.Merger` multi-font combination, `feaLib.addOpenTypeFeaturesFromString` OpenType-Layout feature authoring, and the `unicodedata` script/OT-tag resolver); `opentype-feature-freezer` owns the categorical-best GSUB→`cmap` freeze. The `subset`/`instance`/`merge`/`freeze`/`feature`/`compile` chain feeds `document/emit#DOCUMENT` `FONT_EMBED` (the `dict[str, bytes]` face map consumed verbatim), the `axis_catalog`/`ScriptTags` face-selection feeds `typography/shape#SHAPE`, the `FaceMetrics` OT-metrics value (cap/x-height, vertical extents through `hb.Font.get_font_extents`/`get_metric_position_with_fallback(OTMetricsTag)`) feeds `drawing/regime#REGIME`'s lettering fold and `graphic/style#STYLE`'s type rows, and the `embed_audit` `EmbedReport.complete` gates the `exchange/conformance#CONFORMANCE` PDF/A close. The producer lands the one node contract: `emit()` returns the `ArtifactWork` whose key mints PRE-RUN over the canonical `(source-font ⊕ job)` input bytes, and `_emit` renders `apply` exactly once across the runtime-owned offload bound, threading the same key into the per-mode `ArtifactReceipt.Pdf`/`.Document` case — keyed admission is a real cache probe and no second render mints the receipt.

## [01]-[INDEX]

- [02]-[FONT]: fontTools + opentype-feature-freezer font-binary owner over the closed `FontJob` per-mode union — `SUBSET`/`INSTANCE`/`AXIS_CATALOG`/`OUTLINE`/`EMBED_AUDIT`/`MERGE`/`FREEZE`/`FEATURE`/`COMPILE` folded by one total `apply` `match` and projected onto its `ArtifactReceipt.Pdf`/`.Document` case by the twin `FontJob.receipt` `match`, both rendered from ONE payload across the runtime-owned offload bound `_emit` drives under the pre-run input key `emit()` mints; `AxisLimit` the per-axis pin-range-drop instancing policy, `InstancePolicy` the closed instancer knob struct, `FreezePolicy` the opentype-feature-freezer options carrier, `AxisCatalog` the `fvar`+`STAT` axis-and-named-instance projection, `OutlineCatalog` the per-glyph SVG-`d`-path plus `AreaPen`/`BoundsPen`/`StatisticsPen` outline-quality metrics, `DesignSpace` the `varLib.build` compilation input, `EmbedReport` the PDF/A embed-precondition, `ScriptTags` the `unicodedata` script→OT-tag+direction resolution the shape face-selection seam consumes, and `FaceMetrics` the once-per-face OT-metrics value the regime/style consumers read.

## [02]-[FONT]

- Owner: `FontEngineering` the one font-binary owner `(font, job)` discriminating the engineering step; `FontJob` the closed per-mode `@tagged_union` whose every case carries only its op's payload, folded by one total `apply(font)` `match` closed by `assert_never`, the closed family membership total over the arms by construction (the collapsed form of the prior permissive `FontParams` whose ten fields most ops ignored). fontTools owns the binary model, the partial/full instancer, the variable-font compiler, the glyph/feature/table subsetter, the pen outline algebra, the multi-font merger, the OpenType-Layout feature compiler, and the `fvar`/`STAT`/`cmap`/`unicodedata` introspection; opentype-feature-freezer owns the GSUB→`cmap` freeze fontTools does not provide as a one-call op. `AxisLimit` lowers a scalar to a static pin, a `(lower, upper)` tuple to a partial range, or `None` to an axis drop; `InstancePolicy` collapses the open `inplace`/`optimize`/`updateFontNames` keyword bag into three typed fields; `FreezePolicy` spreads the freezer's `features`/`script`/`lang`/`suffix`/`usesuffix`/`replacenames`/`zapnames`/`info` field set through one `namespace()` projection; `AxisCatalog`/`OutlineCatalog`/`EmbedReport`/`ScriptTags` are the carried value-object sub-owners the read arms fold. `emit()` lands the node contract with the pre-run input key; `_emit` renders `apply` exactly once across the runtime-owned offload bound, and `FontJob.receipt(key, payload)` is the twin per-mode `match` projecting that ONE payload onto its `ArtifactReceipt.Pdf`/`.Document` case — the GIL-holding fontTools transform never stalls the loop and no second render mints the evidence. `FaceMetrics` is the OT-metrics owner block: cap/x-height and vertical extents read once per face through the uharfbuzz metrics tier, the VALUE the regime lettering fold and the style type rows consume.
- Cases: `FontJob` cases — `SUBSET(unicodes, options)` (`subset.Subsetter(subset.Options(**options))` → `populate(unicodes=)` → `subset(font)` → `save`, the `Options` policy carrying `layout_features`/`name_IDs`/`hinting`/`flavor`/`retain_gids`/`glyph_names`/`desubroutinize`/`drop_tables`/`harfbuzz_repacker` retention as one typed knob map; the `Options.flavor` row re-flavors to WOFF/WOFF2 in the same pass) · `INSTANCE(axes, policy)` (`instancer.instantiateVariableFont(font, AxisLimits(limits), **policy.keywords())` over the per-axis `AxisLimit` map, `OverlapMode.KEEP_AND_SET_FLAGS` the instancer overlap default) · `AXIS_CATALOG` (the `fvar` axis-and-named-instance read plus the `STAT` design-axis-record projection folded into a typed `AxisCatalog` — `font["fvar"].axes` tag/min/default/max/flags, `font["fvar"].instances` named-instance coordinates, and `font["STAT"].table.DesignAxisRecord.Axis` `AxisTag`/`AxisNameID`/`AxisOrdering` style-attribute axes that supplement `fvar`) · `OUTLINE(glyph_names, location)` (`getGlyphSet(location=)[name].draw(RecordingPen())` replayed once into `SVGPathPen`/`AreaPen`/`BoundsPen`/`StatisticsPen` — the SVG `d`-path plus the signed contour area, exact glyph bbox, and slant outline-quality metrics folded into an `OutlineCatalog`, the outline traversed exactly once per glyph) · `EMBED_AUDIT(unicodes)` (the embed-completeness fold — glyph coverage against the requested set through `getBestCmap`, required-table presence over `keys`, and post-subset glyph count into an `EmbedReport` whose `complete` gates the `FONT_EMBED` precondition) · `MERGE(extras)` (`merge.Merger().merge([path, *extra_paths])` combining the anchor font and its `extras` binaries across a temp-path round-trip — the multi-font combine `[03]`-mandated tier-1 gain) · `FREEZE(policy)` (`opentype_feature_freezer.RemapByOTL(policy.namespace(...)).run()` over a temp-path round-trip, reading `engine.success` into the rail rather than trusting a silent return — bakes a GSUB single/alternate feature set into the default `cmap` for non-OpenType consumers, the categorical-best freeze) · `FEATURE(source)` (`feaLib.builder.addOpenTypeFeaturesFromString(font, source)` compiling `.fea` Layout features into GSUB/GPOS/GDEF — the feature-authoring half a font-engineering owner needs) · `COMPILE(designspace)` (`designspaceLib.DesignSpaceDocument` built from the anchor master at `default_location` plus the `DesignSpace.sources` masters, then `varLib.build(doc)` — the variable-font-authoring inverse of `INSTANCE`, masters spilled to temp paths) — selected by the frozen closed union, never a chain of `is`-probes.
- Auto: `SUBSET`/`INSTANCE`/`FEATURE` load the font through `TTFont(io.BytesIO(font))`, apply the transform, and `save(io.BytesIO())`; `MERGE`/`FREEZE`/`COMPILE` spill their binary inputs to temp paths through `_spill` (the merger, freezer, and varLib consume file paths), run, and read the output path back, `unlink`ing in a `finally`; `AXIS_CATALOG` reads `fvar.axes`/`instances` and, when `"STAT" in font`, `font["STAT"].table.DesignAxisRecord.Axis` (guarded `None`), folding both axis families into `AxisCatalog`; `OUTLINE` records each glyph once through `RecordingPen` then replays into the four pens, reading `SVGPathPen.getCommands()`, `AreaPen.value`, `BoundsPen.bounds`, and `StatisticsPen.slant`; `EMBED_AUDIT` intersects the requested Unicode set against `getBestCmap` keys, checks `_REQUIRED_TABLES` against `keys()`, counts `getGlyphOrder()`, and folds an `EmbedReport`; `ScriptTags.of(script)` resolves `unicodedata.ot_tags_from_script(script)` and `script_horizontal_direction(script)`, and `ScriptTags.itemize(text)` resolves the ordered-unique scripts a run carries into the per-script OT-tag+direction rows the shape face-selection reads.
- Receipt: `FontJob.receipt(key, payload)` is the one per-mode projection onto the shared `core/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt, reading the ONE payload `engineer` already rendered rather than a second `apply` — the font-binary deliverables (`SUBSET`/`INSTANCE`/`MERGE`/`FREEZE`/`FEATURE`/`COMPILE`) mint `ArtifactReceipt.Pdf(key, len(payload), _glyph_count(payload))` (the page-count slot reused for the output glyph roster length read lazily off the produced binary, never a re-cut); the catalog reads (`AXIS_CATALOG`/`OUTLINE`) mint `ArtifactReceipt.Document(key, len(payload))` over the encoded `AxisCatalog`/`OutlineCatalog` blob; and `EMBED_AUDIT` mints `ArtifactReceipt.Pdf(key, len(payload), _REPORT_DECODER.decode(payload).covered)`, decoding the covered-glyph count back off its own encoded `EmbedReport` so `EmbedReport.complete` gates the `FONT_EMBED` precondition without a recompute. The axis ranges, STAT ordering, outline metrics, cmap-closure result, and required-table presence stay interior evidence folded into the content-key derivation, not new receipt fields the shared `Pdf`/`Document` cases cannot carry. The `Pdf` page slot double-serving a font's glyph count is the anticipatory reuse `core/receipt#RECEIPT` legislates, never a parallel `Font` case.
- Packages: `fonttools` (settled: `subset.Subsetter`/`Options`, `ttLib.TTFont`/`save`/`getGlyphSet`/`getBestCmap`/`getGlyphOrder`/`keys`, `pens.svgPathPen.SVGPathPen`/`areaPen.AreaPen`/`boundsPen.BoundsPen`/`statisticsPen.StatisticsPen`/`recordingPen.RecordingPen`, `varLib.instancer.instantiateVariableFont`/`AxisLimits`/`OverlapMode`, `varLib.build`, `merge.Merger`, `feaLib.builder.addOpenTypeFeaturesFromString`, `designspaceLib.DesignSpaceDocument`/`AxisDescriptor`/`SourceDescriptor`, `unicodedata.ot_tags_from_script`/`script_horizontal_direction`/`script`, the `font["fvar"]`/`font["STAT"].table.DesignAxisRecord.Axis` table access; growth: `colorLib.builder.buildCOLR`/`buildCPAL` and `cu2qu`/`qu2cu`+`TTGlyphPen`/`T2CharStringPen` as the one-arm `COLOR`/`CONVERT` additions), `opentype-feature-freezer` (`RemapByOTL`/`run`/`success`), `uharfbuzz` (`Face`/`Font`/`get_font_extents`/`get_metric_position_with_fallback`/`OTMetricsTag` the FaceMetrics tier), runtime (`identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`, `lanes.LanePolicy.offload` the offload bound), `msgspec` (`Struct`/`msgpack.Encoder`/`msgpack.Decoder` the `EmbedReport` covered-count read-back), `core/receipt#RECEIPT` (`ArtifactReceipt.Pdf`/`.Document` the per-mode contributed cases, composed never re-declared).
- Growth: a new font-engineering step is one `FontJob` case plus one `apply` arm, one `receipt` arm, and one `_op` function (the two `assert_never` tails breaking both matches until the arms exist); a new subset-retention knob is one field on the `subset.Options` map; a new instancer knob is one field on `InstancePolicy`; a new freeze knob is one field on `FreezePolicy`; a new axis-introspection fact is one field on `AxisCatalog`; a new outline metric is one field on `GlyphOutline` reading one more pen; a `COLOR` colour-glyph arm is `colorLib.builder.buildCOLR`/`buildCPAL` via `FontBuilder.setupCOLR`/`setupCPAL` as one case; a `CONVERT` CFF↔glyf re-flavor is `cu2qu.curve_to_quadratic`/`qu2cu.quadratic_to_curves` + `TTGlyphPen`/`T2CharStringPen` as one case; a WOFF/WOFF2 re-flavor is the `subset.Options.flavor` row, never a parallel writer; a new face metric is one `FaceMetrics` field reading one more `OTMetricsTag`; zero new surface.
- Boundary: no PDF authoring (`document/emit#DOCUMENT`), no text shaping (`typography/shape#SHAPE`), no PAdES/PDF security (`exchange/conformance#CONFORMANCE`); the owner transforms a font binary and proves it embeddable, never producing a document. The `SUBSET`/`INSTANCE`/`MERGE`/`FREEZE`/`FEATURE`/`COMPILE` arms produce the `dict[str, bytes]` face-to-bytes map the `FONT_EMBED` arm consumes verbatim and the `EMBED_AUDIT` `EmbedReport.complete` is the PDF/A precondition. The uharfbuzz `SubsetInput`/`subset` HarfBuzz subsetter is the rejected duplicate of the fontTools `SUBSET` footprint (fontTools owns subsetting for Python-native `Options` feature-policy control); a hand-pruned `glyf`/`CFF` table walk is the rejected duplicate of `Subsetter.subset`, a per-instance hand-assembled static cut the rejected duplicate of `instantiateVariableFont`, a hand-assembled variable font the rejected duplicate of `varLib.build`, a hand-walked GSUB `ScriptList`/`FeatureList`/`LookupList` traversal or hand-built `cmap` rewrite the rejected duplicate of `RemapByOTL`, a hand-built GSUB/GPOS the rejected duplicate of `addOpenTypeFeaturesFromString`, a hand-coded `fvar`/`STAT` decode the rejected duplicate of the `font[tag]` read, a raw outline-coordinate decode the rejected duplicate of the pen bridge, a Python-list font-merge the rejected duplicate of `Merger.merge`, and a hand-coded script→OT-tag map the rejected duplicate of `unicodedata`. A permissive `FontParams` bag whose fields most ops ignore, a parallel `_woff` writer, and a `dict[str, object]` instancer keyword bag are the collapsed forms — the per-mode `FontJob` case carries only its op's fields. A synchronous on-loop `apply` where the runtime offload bound carries the GIL-holding subset/instancer/varLib work, a key minted over produced OUTPUT bytes where the pre-run input key makes keyed admission a real probe, a second `apply` to mint the receipt where `_render` derives the `ArtifactReceipt` from one produced payload, a re-parse of the deliverable to re-count glyphs where `_glyph_count` reads the roster off that single payload, and a parallel `ArtifactReceipt.Font` case where the `Pdf` page slot already carries the glyph count, are the deleted forms.

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


_REPORT_DECODER: Final = msgspec.msgpack.Decoder(EmbedReport)  # reads the covered-glyph count back off the EMBED_AUDIT payload for its receipt


class ScriptTags(Struct, frozen=True):
    script: str  # ISO 15924 code, e.g. "Latn"
    ot_tags: tuple[str, ...]  # OpenType script tags — multiple for Indic v1/v2 (e.g. "dev2"/"deva")
    direction: str  # "LTR" / "RTL"

    @staticmethod
    def of(script: str) -> "ScriptTags":
        return ScriptTags(script, tuple(unicodedata.ot_tags_from_script(script)), unicodedata.script_horizontal_direction(script))

    @staticmethod
    def itemize(text: str) -> tuple["ScriptTags", ...]:
        return tuple(ScriptTags.of(script) for script in dict.fromkeys(unicodedata.script(ch) for ch in text))


class FaceMetrics(Struct, frozen=True):
    # the font-METRICS owner surface: cap/x-height and vertical extents read ONCE per face through the
    # uharfbuzz OT metrics tier — drawing/regime's ISO 3098 lettering fold and style's type rows consume
    # this VALUE; no consumer re-reads a font binary for a metric.
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
        # the per-mode receipt projection over the ONE produced payload: a font-binary arm mints `Pdf` (the
        # page slot carrying the output glyph count), a catalog-read arm the byte-only `Document`, and
        # `EMBED_AUDIT` a `Pdf` whose page slot is the covered-glyph count `EmbedReport.complete` gates on.
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
        # key-over-INPUT: the canonical (source-font ⊕ job) bytes, minted PRE-RUN so keyed admission
        # probes the warm seed BEFORE the fontTools transform runs — never a key over the produced payload.
        return ContentIdentity.of(f"font-{self.job.tag}", (self.font, self.job), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the GIL-holding fontTools `apply` crosses the runtime-owned offload bound; the SAME pre-run key
        # threads into the per-mode receipt so `receipt.slot == node.key` — one payload, one receipt.
        return await LanePolicy.offload(self._render, modality=Modality.INTERPRETER)

    def _render(self) -> ArtifactReceipt:
        payload = self.job.apply(self.font)  # the ONE produced fact the receipt facts read
        return self.job.receipt(self._key, payload)


# --- [OPERATIONS] ----------------------------------------------------------------------


def _spill(data: bytes, suffix: str, /) -> Path:
    path = Path(NamedTemporaryFile(suffix=suffix, delete=False).name)
    path.write_bytes(data)  # the freezer/merger/varLib consume file paths, not bytes
    return path


def _glyph_count(font: bytes, /) -> int:
    return len(
        TTFont(io.BytesIO(font), lazy=True).getGlyphOrder()
    )  # output roster length for the `Pdf` receipt page slot — a lazy header read, not a re-cut


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

One owner, one closed job union, one render per deliverable: the node key mints over the source-font-and-job input before any transform runs, so a re-issued document re-cuts only the faces whose spec changed; the receipt facts read off the single produced payload; and every metric a downstream plane needs — the regime lettering cap correction, the style type-row pins, the shape face selection, the conformance embed gate — is a typed value this page reads once from the binary, never a consumer-side font parse.
