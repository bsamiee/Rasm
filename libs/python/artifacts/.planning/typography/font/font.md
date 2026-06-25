# [PY_ARTIFACTS_FONT]

The font-binary engineering owner over the document rail. `FontEngineering` is ONE owner that takes a font (variable or static) and folds it through a closed `FontOp` family into a minimized, instanced, axis-introspected, outline-bridged, embed-validated deliverable: fonttools `subset.Subsetter` reduces the embedded glyph footprint, `varLib.instancer.instantiateVariableFont` partial-axis-instances through the `AxisLimit` pin-range-drop policy under the typed `InstancePolicy` knob struct, the `fvar`/`STAT` tables project into a typed `AxisCatalog` of named instances and axis ranges, `SVGPathPen` bridges each glyph to its real vector `d`-path, and an embed-completeness audit folds the glyph-coverage / cmap-closure / table-presence facts a downstream PDF/A close requires. `fonttools` is the single package this owner composes; the `subset` -> `instance` -> `embed-precondition` chain is consumed by `documents/emit#DOCUMENT` `FONT_EMBED`, the face selection and variation location by `typography/shape#SHAPE`, and the embed-audit precondition by `typography/sign#SIGN`. Every arm returns a `RuntimeRail[ContentKey]` keyed by the runtime content key and contributes one `ArtifactReceipt.Pdf`.

## [01]-[INDEX]

- [01]-[FONT]: fonttools subset/partial-instance/axis-introspection/outline/embed-audit owner over the closed `FontOp` step table; `AxisLimit` is the per-axis pin-range-drop instancing policy, `InstancePolicy` the closed instancer knob struct, and `AxisCatalog` the `fvar`/`STAT` axis-and-named-instance projection the shape and embed arms read.

## [02]-[FONT]

- Owner: `FontEngineering` the one font-binary owner discriminating the engineering step; `FontOp` the closed `StrEnum` over footprint subsetting, partial-axis instancing, axis introspection, outline extraction, and embed validation; one frozen `_FONT_TABLE` `MappingProxyType` data-row dispatch maps each step to its `FontAcceptor` with zero `match`/`case` sprawl, the closed `StrEnum` membership total over the table by construction. fonttools owns the binary font model, the partial/full instancer, the glyph/feature/table subsetter, the pen outline algebra, and the `fvar`/`STAT`/`cmap` introspection the catalog. `AxisLimit` is the per-axis pin-range-drop policy whose `of`/`resolve` lower a scalar to a static pin, a `(lower, upper)` tuple to a partial range, or `None` to an axis drop; `InstancePolicy` collapses the open `inplace`/`optimize`/`updateFontNames` keyword bag into three typed fields whose `keywords()` projection spreads the closed knob set; `AxisCatalog` and `EmbedReport` are the two carried value-object sub-owners the `AXIS_CATALOG` and `EMBED_AUDIT` arms fold.
- Cases: `FontOp` rows `SUBSET` (fontTools `subset.Subsetter(Options(...))` -> `populate(unicodes=...)` -> `subset(font)` -> `TTFont.save`, the `Options` policy carrying `layout_features`/`name_IDs`/`hinting`/`flavor`/`retain_gids`/`glyph_names`/`desubroutinize`/`drop_tables`/`harfbuzz_repacker` retention as one typed knob map, never a hand-pruned table walk) · `INSTANCE` (`varLib.instancer.instantiateVariableFont(font, axisLimits, **InstancePolicy.keywords())` over the per-axis `AxisLimit` policy — a scalar `pin` collapses the axis to a static instance, a `(lower, upper)` partial-range keeps the axis variable inside the clamped span, and an empty limit drops the axis at its default; `OverlapMode.KEEP_AND_SET_FLAGS` is the instancer overlap default the `InstancePolicy` does not override) · `AXIS_CATALOG` (the `fvar` axis-and-named-instance read plus the `STAT` design-axis-record projection folded into a typed `AxisCatalog` — `font["fvar"].axes` axis tag/min/default/max/flags, `font["fvar"].instances` named-instance coordinate maps, and the `STAT` design-axis records resolved into `AxisCatalog.axes`/`named_instances`, the introspection the `SHAPE` variation location and the `INSTANCE` limit validation consume) · `OUTLINE` (the `TTFont.getGlyphSet()[name].draw(SVGPathPen(glyphSet))` -> `SVGPathPen.getCommands()` vector bridge producing each glyph's SVG `d`-path, the `glyf`/`CFF` outline read through the pen protocol, never a raw coordinate decode) · `EMBED_AUDIT` (the embed-completeness fold over the subsetted/instanced font — glyph coverage against the requested Unicode set through `TTFont.getBestCmap`, cmap closure, required-table presence over `TTFont.keys`, and post-subset glyph count into a typed `EmbedReport` whose `complete` projection gates the PDF/A `FONT_EMBED` precondition `documents/emit#DOCUMENT` and the PDF/A close `typography/sign#SIGN` require) — selected by the frozen `_FONT_TABLE` row, never a chain of `is`-probes; the instancing policy is the `InstancePolicy` knob struct carried in `params`, the per-axis limit the `AxisLimit` policy, and the subset retention the `subset.Options` knob map, each a typed value not an open keyword bag.
- Entry: `FontEngineering.engineer` dispatches the step over the input font through the one `_FONT_TABLE[step]` acceptor lookup and returns a `RuntimeRail[ContentKey]`; subsetting and partial-instancing reduce the embedded-font footprint, axis introspection projects the variable design space the shape and instance arms read, outline extraction produces each glyph's real `d`-path, and the embed audit proves the subset/instance close is PDF/A-embeddable. `SUBSET`/`INSTANCE`/`AXIS_CATALOG`/`OUTLINE`/`EMBED_AUDIT` all resolve synchronously over the cp315-core pure-Python fonttools wheel (`py3-none-any`, no ABI gate), so the owner stays on the synchronous runtime `boundary` and never crosses the gated subprocess band.
- Auto: subsetting folds the used glyph set through `Subsetter(Options(**subset_options))` `populate(unicodes=...)` then `subset(font)` then `TTFont.save`; instancing folds the per-axis `AxisLimit.of(raw).resolve()` map into a `fontTools.varLib.instancer.AxisLimits` and calls `instantiateVariableFont(font, axisLimits, **InstancePolicy.keywords())` where `pin` collapses the axis to a static value, `(lower, upper)` restricts it to a partial range, and an empty limit drops it; axis introspection reads `font["fvar"].axes` (each `Axis(axisTag, minValue, defaultValue, maxValue, flags)`) and `font["fvar"].instances` plus the `STAT` design-axis records into an `AxisCatalog` of `(tag, minimum, default, maximum, hidden)` axis rows and `(name, coordinates)` named-instance rows; outline extraction loads `TTFont.getGlyphSet(location=...)` and folds each requested glyph through `glyph_set[name].draw(SVGPathPen(glyph_set))` then `SVGPathPen.getCommands()`; the embed audit resolves `TTFont.getBestCmap()`, intersects the requested Unicode set against the cmap keys for coverage, checks the required-table tag set against `TTFont.keys()`, counts `getGlyphOrder()`, and folds an `EmbedReport`.
- Receipt: every arm projects its output onto the shared `receipt/receipt#RECEIPT` `ArtifactReceipt` family, never a per-step receipt — the `SUBSET`/`INSTANCE`/`OUTLINE` arms contribute `ArtifactReceipt.Pdf` carrying the content key, the output byte count, and the glyph count (the page-count slot reused for the glyph count of a font deliverable), the `AXIS_CATALOG` arm contributes `ArtifactReceipt.Document` carrying the content key and the encoded `AxisCatalog` byte length, and the `EMBED_AUDIT` arm contributes `ArtifactReceipt.Pdf` carrying the content key, the post-subset byte count, and the covered-glyph count whose `EmbedReport.complete` flag the embed-precondition consumer reads off the content-key derivation. The axis ranges, named-instance coordinates, cmap-closure result, and required-table presence the `AXIS_CATALOG`/`EMBED_AUDIT` arms compute stay interior evidence the arm folds into its content-key derivation, not new receipt fields the shared `Pdf`/`Document` cases cannot carry.
- Packages: `fonttools` (settled: `subset.Subsetter`/`subset.Options`/`ttLib.TTFont`/`TTFont.save`/`TTFont.getGlyphSet`/`TTFont.getBestCmap`/`TTFont.getGlyphOrder`/`TTFont.keys`/`pens.svgPathPen.SVGPathPen`/`SVGPathPen.getCommands`/`varLib.instancer.instantiateVariableFont`/`varLib.instancer.AxisLimits`/`OverlapMode.KEEP_AND_SET_FLAGS`, the `fvar` `font["fvar"].axes`/`instances` axis-and-named-instance read and the `font["fvar"]`/`font["STAT"]` table access; RESEARCH: the `STAT` `font["STAT"].table.DesignAxisRecord`/`AxisValueArray` member spellings the catalogue rows as table access without enumerating the `STAT` sub-record members), runtime (`content_identity.ContentIdentity`, `faults.RuntimeRail`/`boundary`), `msgspec` (`Struct`/`msgpack.Encoder`).
- Growth: a new font-engineering step is one `FontOp` row plus one `_FONT_TABLE` acceptor entry; a new subset-retention knob is one field on the `subset.Options` map; a new instancer knob is one field on `InstancePolicy`; a new axis-introspection fact is one field on `AxisCatalog`; a new embed-audit fact is one field on `EmbedReport`; a WOFF/WOFF2 re-flavor is the `subset.Options.flavor` row, never a parallel writer; zero new surface.
- Boundary: no PDF authoring (that stays at `documents/emit#DOCUMENT`), no text shaping (that is `typography/shape#SHAPE`), no PAdES/PDF security (that is `typography/sign#SIGN`); the owner transforms a font binary and proves it embeddable, never producing a document. The `SUBSET`/`INSTANCE` arms produce the `dict[str, bytes]` face-to-bytes map the `documents/emit#DOCUMENT` `FONT_EMBED` arm consumes verbatim and the `EMBED_AUDIT` `EmbedReport.complete` is the precondition the PDF/A close reads. The uharfbuzz `SubsetInput`/`subset` and `subset.Options(harfbuzz_repacker=True)` HarfBuzz repacker are the rejected duplicate of the fontTools `SUBSET` footprint — fontTools owns subsetting for Python-native feature-policy control through `Options`; a hand-pruned `glyf`/`CFF` table walk is the rejected duplicate of `Subsetter.subset`, a per-instance hand-assembled static cut the rejected duplicate of `instantiateVariableFont`, a hand-coded `fvar`/`STAT` decode the rejected duplicate of the `font[tag]` table read, and a raw outline-coordinate decode the rejected duplicate of the `SVGPathPen` pen bridge. A second `_woff`/`_woff2`-style writer function pair, a parallel `_AxisLimit`/`_InstancePolicy` struct beside the policy, and a weak `dict[str, object]` instancer keyword bag are the collapsed forms — `AxisLimit` carries the per-axis pin-range-drop, `InstancePolicy.keywords()` spreads the closed instancer knobs, and `subset.Options` carries the retention policy. The `AXIS_CATALOG` arm reads `fvar`/`STAT` over the live `TTFont` rather than re-parsing the binary tables, and the `EMBED_AUDIT` arm reads cmap/table presence over `TTFont.getBestCmap`/`keys` rather than a hand-rolled completeness scan.

```python signature
import io
from collections.abc import Callable, Mapping, Sequence
from enum import StrEnum
from types import MappingProxyType
from typing import Final

import msgspec
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, boundary

type AxisPin = float | tuple[float, float] | None
type AxisValue = float | tuple[float | None, float | None] | None
type FontAcceptor = Callable[["FontEngineering"], bytes]

_REQUIRED_TABLES: Final[frozenset[str]] = frozenset({"cmap", "head", "hhea", "hmtx", "maxp", "name", "post"})
_CATALOG_ENCODER: Final = msgspec.msgpack.Encoder()
_REPORT_ENCODER: Final = msgspec.msgpack.Encoder()


class FontOp(StrEnum):
    SUBSET = "subset"
    INSTANCE = "instance"
    AXIS_CATALOG = "axis_catalog"
    OUTLINE = "outline"
    EMBED_AUDIT = "embed_audit"


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


class AxisRecord(Struct, frozen=True):
    tag: str
    minimum: float
    default: float
    maximum: float
    hidden: bool


class NamedInstance(Struct, frozen=True):
    name: str
    coordinates: Mapping[str, float]


class AxisCatalog(Struct, frozen=True):
    axes: tuple[AxisRecord, ...]
    named_instances: tuple[NamedInstance, ...]

    @property
    def axis_count(self) -> int:
        return len(self.axes)


class EmbedReport(Struct, frozen=True):
    requested: int
    covered: int
    glyph_count: int
    missing_tables: tuple[str, ...]

    @property
    def complete(self) -> bool:
        return self.covered == self.requested and not self.missing_tables


class FontParams(Struct, frozen=True, kw_only=True):
    unicodes: tuple[int, ...] = ()
    subset_options: Mapping[str, object] = {}
    axes: Mapping[str, AxisPin] = {}
    instance_policy: InstancePolicy = InstancePolicy()
    glyph_names: tuple[str, ...] = ()
    location: Mapping[str, float] = {}


class FontEngineering(Struct, frozen=True):
    step: FontOp
    font: bytes
    params: FontParams

    def engineer(self) -> RuntimeRail[ContentKey]:
        return boundary(f"font.{self.step}", self._emit)

    def _emit(self) -> ContentKey:
        return ContentIdentity.of(f"font-{self.step}", _FONT_TABLE[self.step](self))


def _subset_font(engineering: "FontEngineering") -> bytes:
    from fontTools import subset
    from fontTools.ttLib import TTFont

    font = TTFont(io.BytesIO(engineering.font))
    subsetter = subset.Subsetter(options=subset.Options(**engineering.params.subset_options))
    subsetter.populate(unicodes=engineering.params.unicodes)
    subsetter.subset(font)
    sink = io.BytesIO()
    font.save(sink)
    return sink.getvalue()


def _instance_font(engineering: "FontEngineering") -> bytes:
    from fontTools.ttLib import TTFont
    from fontTools.varLib import instancer

    font = TTFont(io.BytesIO(engineering.font))
    limits: dict[str, AxisValue] = {tag: AxisLimit.of(raw).resolve() for tag, raw in engineering.params.axes.items()}
    instance = instancer.instantiateVariableFont(
        font, instancer.AxisLimits(limits), **engineering.params.instance_policy.keywords()
    )
    sink = io.BytesIO()
    instance.save(sink)
    return sink.getvalue()


def _axis_catalog(engineering: "FontEngineering") -> bytes:
    from fontTools.ttLib import TTFont

    font = TTFont(io.BytesIO(engineering.font), lazy=True)
    fvar = font["fvar"]
    axes = tuple(
        AxisRecord(axis.axisTag, axis.minValue, axis.defaultValue, axis.maxValue, bool(axis.flags & 0x0001))
        for axis in fvar.axes
    )
    tags = [axis.axisTag for axis in fvar.axes]
    named = tuple(
        NamedInstance(_instance_name(font, instance), dict(zip(tags, instance.coordinates.values(), strict=False)))
        for instance in fvar.instances
    )
    return _CATALOG_ENCODER.encode(AxisCatalog(axes=axes, named_instances=named))


def _instance_name(font: object, instance: object) -> str:
    record = font["name"].getDebugName(instance.subfamilyNameID)
    return record if record is not None else f"instance-{instance.subfamilyNameID}"


def _outline_paths(engineering: "FontEngineering") -> bytes:
    from fontTools.pens.svgPathPen import SVGPathPen
    from fontTools.ttLib import TTFont

    font = TTFont(io.BytesIO(engineering.font))
    glyph_set = font.getGlyphSet(location=dict(engineering.params.location) or None)
    names = engineering.params.glyph_names or tuple(glyph_set.keys())
    paths: dict[str, str] = {}
    for name in names:
        pen = SVGPathPen(glyph_set)
        glyph_set[name].draw(pen)
        paths[name] = pen.getCommands()
    return _CATALOG_ENCODER.encode(paths)


def _embed_audit(engineering: "FontEngineering") -> bytes:
    from fontTools.ttLib import TTFont

    font = TTFont(io.BytesIO(engineering.font), lazy=True)
    cmap = font.getBestCmap()
    requested = frozenset(engineering.params.unicodes) or frozenset(cmap.keys())
    covered = sum(1 for codepoint in requested if codepoint in cmap)
    missing = tuple(sorted(_REQUIRED_TABLES.difference(font.keys())))
    return _REPORT_ENCODER.encode(
        EmbedReport(requested=len(requested), covered=covered, glyph_count=len(font.getGlyphOrder()), missing_tables=missing)
    )


_FONT_TABLE: Final[MappingProxyType[FontOp, FontAcceptor]] = MappingProxyType({
    FontOp.SUBSET: _subset_font,
    FontOp.INSTANCE: _instance_font,
    FontOp.AXIS_CATALOG: _axis_catalog,
    FontOp.OUTLINE: _outline_paths,
    FontOp.EMBED_AUDIT: _embed_audit,
})
```

## [03]-[RESEARCH]

- [INSTANCE] [RESOLVED]: the `varLib.instancer.instantiateVariableFont(varfont, axisLimits, inplace=False, optimize=True, overlap=OverlapMode.KEEP_AND_SET_FLAGS, updateFontNames=False, *, downgradeCFF2=False, static=False)` call, the `inplace`/`optimize`/`updateFontNames` keyword spread the `InstancePolicy.keywords()` projection feeds, and the `fontTools.varLib.instancer.AxisLimits` limit type the per-axis `AxisLimit.resolve()` map normalizes into verify against the folder `.api` catalogue for `fonttools` (`4.63.0`) entrypoint row `[02]` and the public-type instancer surface — the catalogue spells the full call signature with the `axisLimits`/`inplace`/`optimize`/`overlap`/`updateFontNames` parameters, so the `_instance_font` body and the `InstancePolicy.keywords()` spread are settled fence code. The `AxisLimit` policy is the one per-axis pin-range-drop owner — `AxisLimit.of(raw)` `match`-projects a scalar to a `pin`, a `(lower, upper)` tuple to a partial range, and `None` to an axis drop, and `resolve()` lowers the policy to the `AxisValue` the `AxisLimits` map admits: a scalar collapses the axis to a static instance, a tuple narrows the design space without removing the axis, a dropped axis falls back to its default. The `OverlapMode.KEEP_AND_SET_FLAGS` overlap default the instancer applies is the catalogued default; the `InstancePolicy` does not override it.
- [SUBSET] [RESOLVED]: the fontTools `subset.Subsetter(options=Options(...))` -> `populate(unicodes=...)` -> `subset(font)` -> `ttLib.TTFont`/`save` subsetting chain and the `Options(layout_features=, name_IDs=, hinting=, flavor=, retain_gids=, glyph_names=, desubroutinize=, drop_tables=, harfbuzz_repacker=)` retention knob map verify against the folder `.api` catalogue for `fonttools` entrypoint rows `[01]`-`[03]` and public-type rows `[05]`/`[06]`. The `Options.flavor` re-flavor to WOFF/WOFF2 is the one row the subset pass carries; a parallel WOFF writer is the rejected form. The uharfbuzz `SubsetInput`/`subset` and `Options(harfbuzz_repacker=True)` HarfBuzz repacker overlap the fontTools `Subsetter` the `SUBSET` row owns; fontTools owns subsetting for Python-native feature-policy control through `Options`, so no `FontOp` splits subsetting across the two packages.
- [AXIS_CATALOG] [RESOLVED]: the `fvar` axis-and-named-instance introspection — `font["fvar"].axes` (each `Axis(axisTag, minValue, defaultValue, maxValue, flags)`), `font["fvar"].instances` (each `NamedInstance(subfamilyNameID, coordinates)`), and the `font["name"].getDebugName(nameID)` name resolution — folds the variable design space into the typed `AxisCatalog` the `SHAPE` variation location and the `INSTANCE` limit validation consume, the `font[tag]` table access verified against the folder `.api` catalogue for `fonttools` entrypoint row `[08]` (`TTFont.keys`/`TTFont[tag]`). RESEARCH: the `STAT` design-axis read — `font["STAT"].table.DesignAxisRecord`/`AxisValueArray` for the style-attribute axis records that supplement `fvar` — is the catalogued `font[tag]` table access without the `STAT` sub-record member spellings rowed, so the `_axis_catalog` body folds the `fvar` axes and named instances (settled) and the `STAT` design-axis supplement stays a marked growth edge until the `fonttools` catalogue rows the `STAT` table sub-record members. The `Axis.flags & 0x0001` hidden-axis bit is the OpenType `fvar` `HIDDEN_AXIS` flag mask read off the settled `flags` field.
- [OUTLINE] [RESOLVED]: the `TTFont.getGlyphSet(preferCFF=True, location=None, normalized=False, recalcBounds=True)` glyph factory, the `glyph_set[name].draw(pen)` segment-protocol replay, the `fontTools.pens.svgPathPen.SVGPathPen(glyphSet, ntos=str)` SVG pen, and the `SVGPathPen.getCommands()` `d`-path accessor verify against the folder `.api` catalogue for `fonttools` entrypoint row `[04]` and public-type pen row `[07]` — the catalogue spells `SVGPathPen` with `.getCommands()` as the SVG-`d` accessor and `getGlyphSet()[name].draw(pen)` as the single read path, so the `_outline_paths` body is settled fence code. The `location` argument cuts a variable instance for the outline read so an instanced glyph's `d`-path matches the pinned location; a raw `glyf`/`CFF` coordinate decode is the rejected duplicate of the pen bridge.
- [EMBED_AUDIT] [RESOLVED]: the `TTFont.getBestCmap(cmapPreferences=...)` Unicode-to-glyph map, the `TTFont.getGlyphOrder()` glyph-name list, and the `TTFont.keys()` present-table-tag enumeration verify against the folder `.api` catalogue for `fonttools` entrypoint rows `[05]`/`[06]`/`[08]`; the embed audit folds glyph coverage (requested Unicode set intersected against the `getBestCmap` keys), required-table presence (the `_REQUIRED_TABLES` OpenType base set against `keys()`), and post-subset glyph count into the `EmbedReport` whose `complete` projection gates the `documents/emit#DOCUMENT` `FONT_EMBED` precondition and the `typography/sign#SIGN` PDF/A close. The `_REQUIRED_TABLES` set is the minimal OpenType deliverable table family (`cmap`/`head`/`hhea`/`hmtx`/`maxp`/`name`/`post`) a PDF-embeddable font must carry; a hand-rolled completeness scan over raw table bytes is the rejected duplicate of the `keys()`/`getBestCmap` reads.
