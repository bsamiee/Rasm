# [PY_ARTIFACTS_LAYERED]

`LayeredExport` owns the editable named-layer export close — it authors the separable, toggleable, lockable layer structure an external editor re-orders and re-colors, the inverse of the `document/egress#FINISH` `FINISHERS` table that STRIPS the layers this owner authors. ONE owner discriminates the editor family over the closed `ExportTarget` `StrEnum` (`SVG`/`PDF`/`ORA`/`PSD`/`PSB`/`TIFF`), each target a `LayerEngine` row in the `ENGINES` policy table binding its single `LayerFact`-returning arm, its `preview`-versus-egress receipt discriminant, and its execution `Band` — the table the totality proof and the only dispatch. Placement, scaling, and rasterization stay upstream: this owner re-renders nothing and re-lays-out nothing, receiving already-placed sources and authoring the layer structure the flat egress deliberately omits.

Admission is trusted layer material plus one optional untrusted blob: the `tuple[Layer, ...]` rows arrive from the visual producers (`composition/compose#COMPOSE`, `graphic/marks/encode#MARK`, `visualization/diagram/draw#DRAW`, `graphic/raster/io#RASTER`) beside one trusted `LayerPolicy`, the untrusted external `base` PDF admitted once at `of` through the closed `ExportPayload` `TypedDict` + `TypeAdapter`, an empty layer set rejected into `ExportFault.empty` before the fold. `Layer(name, source, bbox)`'s positional contract the five producers construct stays a three-argument call while `visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color` carry the full editor-panel axis. Placement is a `Band` policy value the `_PLACE` table resolves — the `SVG`/`PDF` arms cross the runtime thread lane under the `_THREAD_GATE` `CapacityLimiter` so the GIL-releasing `pymupdf`/`pikepdf` render never runs on the event loop, the `ORA`/`PSD`/`PSB`/`TIFF` arms cross the process lane under the module-level `_GATE` because libvips and the libxml2-backed `lxml` are off the runtime loader path. One per-instance `LayeredExport.emit -> RuntimeRail[ArtifactReceipt]` IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules (the `export/indesign#INDESIGN` `Idml.emit` counterpart recorded in ARCHITECTURE `[02]-[SEAMS]` as the single-production-entry seam), returning the existing `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case (the named-layer documents) or `ArtifactReceipt.Egress` case (the OCG-layered PDF) with the authored-layer count on the `overlays` slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED count on.

## [01]-[INDEX]

- [01]-[LAYERED]: the one editable named-layer export owner — `LayeredExport` discriminating the editor family over the closed `ExportTarget` (`SVG`/`PDF`/`ORA`/`PSD`/`PSB`/`TIFF`) keyed to the `ENGINES` band-dispatched table, each arm authoring the layer structure the flat egress omits and returning `ArtifactReceipt.Preview` or `ArtifactReceipt.Egress`.

## [02]-[LAYERED]

- Owner: `LayeredExport` discriminates the editor family over the closed `ExportTarget` keyed to the `ENGINES` policy table binding each arm's `LayerFact` body, `preview`-versus-egress discriminant, and execution `Band`. `Layer` is the row every visual producer constructs — `name`/`source`/`bbox` positional, the editor-panel axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) defaulting after `bbox`; `group` is a folder LABEL projected to all three editors (the SVG parent `<g>`, the OCG `/Order` folder, the ORA `<stack name=>`), never a parent-layer-NAME reference. `LayerPolicy` is the trusted save-knob bundle; `BlendMode` is the 16-mode CSS enum whose value IS the SVG `mix-blend-mode` token and whose member NAME derives the `psdtags.PsdBlendMode`/`psd_constants.BlendMode` members with no parallel table.
- Cases: each `ExportTarget` is a `LayerEngine` row with its editability ruling — `SVG` (`drawsvg` named `<g inkscape:groupmode=layer>` groups read as layers by the Illustrator and Inkscape panels), `PDF` (`pymupdf` OCG placement enriched by the `pikepdf` `/OCProperties` object model for Acrobat), `ORA` (an OpenRaster `pyvips`/`lxml`/`stream-zip` container for GIMP/Krita), `PSD`/`PSB` (a native Photoshop channel-stack document through `psd_tools`, the cp315-present standing author, PhotoshopAPI the categorical-best native writer selected once a wheel lands, per-channel codec capability-detected off the `imagecodecs` `<CODEC>.available` backend), `TIFF` (a Photoshop-compatible layered TIFF through `psdtags`+`tifffile`, RETAINED for the TIFF container only). There is NO layer-faithful `.ai` target: Illustrator's `.ai` OCG layers do NOT map to its layer panel, so an OCG-PDF renamed `.ai` opens FLAT — the durable Illustrator hand-off is the `SVG` named-layer document, so `.ai` is deliberately absent.
- Auto: `_emit` resolves `engine = ENGINES[self.target]`, awaits `_PLACE[engine.band](engine.arm, self)` so the band table runs the arm on the `to_thread` lane or the subprocess worker, never inline on the event loop, mints the content key over the returned `LayerFact.data`, and returns the `preview`-discriminated `ArtifactReceipt` directly. `SVG` folds each `Layer` into one leaf `Group(**layer.svg_attrs())` nested under one `<g inkscape:groupmode=layer>` folder minted per distinct `group`. `PDF` opens the `pymupdf` document in a `with` that closes the native handle (never GC-reaped), mints one `add_ocg` xref per layer, places each source in a nested `with`, drives the visibility/lock partitions through `set_layer`, then `_enriched` folds the per-layer `/Usage` sub-dict and `/Order` folder tree onto the `pikepdf` `/OCProperties` catalog. `ORA` scales alpha by `opacity` and stacks the visible layers through the native `composite(modes)` under their `_vips_blend` modes for a FAITHFUL `mergedimage.png`, authors `stack.xml` through `lxml`, and frames the ZIP through `stream_zip`.
- Output: `LayerFact` is the bytes-plus-evidence carrier every arm returns — the `data` plus the SVG/ORA viewport (`width`/`height`, the `Preview` facts) and the PDF `pages`/`layers` (the `Egress` facts).
- Receipt: `_emit` mints the content key over the arm's `LayerFact.data` and folds the case off the `LayerEngine.preview` discriminant — the named-document arms return `ArtifactReceipt.Preview(key, width, height)` (the same `Preview` shape the visual producers return), the `PDF` arm returns `ArtifactReceipt.Egress(key, bytes, pages, 0, 0, layers)` carrying the authored-layer count on the `overlays` slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED count on — author and subtract are inverses over one field. Layered export adds NO new receipt case; the producer projects flat scalars so the receipt owner imports no producer value object.
- Growth: a new editable-export target (a Scribus `.sla`, an Affinity `.afphoto`) is one `ExportTarget` member plus one `LayerEngine` row plus one arm over the existing engine algebra — never a re-implemented serializer, object model, channel writer, or container; a new layer attribute is one `Layer` field threaded into each format's projection; a new compositing mode is one `BlendMode` member, its value the SVG spelling and its member NAME the `psdtags`/`psd_constants` correspondence (the 12 native-only Photoshop modes are a separate growth axis, absent from the CSS-value contract the SVG arm demands); a new per-channel codec is one `PsdCompression` member plus one `_CHANNEL_CODEC` row; a further usage application is one `LayerIntent` member plus one `_USAGE`/`_STATE_KEY` row (`_INTENT` is DERIVED, so the member auto-derives its hint), only a Real-valued `/Zoom` min/max band adding one `_usage` arm; a new execution band is one `Band` member plus one `_PLACE` row; a new save knob is one `LayerPolicy` field; a new admission invariant is one `ExportFault` case plus one `of` guard; a new untrusted blob is one `ExportPayload` band line. Zero new surface.
- Boundary: a per-producer layer-export class family where one `ENGINES`-dispatched owner suffices; a `names`/`sources`/`flags` triple-list zipped at the call site where one `Layer` row carries its own; a hand-emitted `<g id>` string where the `drawsvg` `Group` authors it, and a plain `<g id>` (a GROUP, not a layer) where `inkscape:groupmode="layer"` makes it a real layer; a `groups[layer.name]` parent lookup where `group` references a folder LABEL, not a layer NAME; a hand-written `/OC … BDC … EMC` content-stream where the `pymupdf` `oc=` placement brackets the span natively; a multi-column per-format `_BLEND` table where the shared member NAME derives every consumer; a `reduce(composite2, OVER)` flatten discarding per-layer blend and opacity where the native `composite(modes)` keeps a FAITHFUL `mergedimage.png`; an unbounded `to_process` offload where the `_GATE`/`_THREAD_GATE` `CapacityLimiter` bounds it; an arm run inline on the event loop where the `to_thread` lane offloads the GIL-releasing render; a native `pymupdf`/`pikepdf` handle left for GC where the arm brackets it in a `with`; a module-level batch entry where the one per-instance `emit` the `ArtifactPipeline` schedules; a duplicate layer name where `of`'s `Counter` gate rejects it into `ExportFault.duplicate` are the deleted forms. On the PSD plane: a `psd_tools`-plus-PhotoshopAPI writer admitted on ONE interpreter (the native writer supersedes, never a second parallel owner), a `psd_tools` array handed straight to `tifffile` (the TIFF arm owns its own `psdtags.PsdLayer` lowering), the 12 native-only Photoshop blend modes forced onto the CSS-value `BlendMode`, and a channel routed through an unprobed codec are the rejected forms. Rasterization stays `graphic/raster/io#RASTER`/`graphic/vector/region#REGION`, placement the compose `Tile`/`ScaleFit`/`Overlay` arms; the inverse OCG-layer STRIP/FLATTEN stays `document/egress#FINISH`, the PDF/A close `document/emit#EMIT`, the PAdES close `exchange/conformance#CONFORMANCE`, the InDesign hand-off `export/indesign#INDESIGN`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Awaitable, Callable
from enum import IntEnum, StrEnum
from io import BytesIO
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack

from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, field
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt

# core-arm providers; each proxy reifies in-process on first SVG/PDF arm use
lazy import drawsvg
lazy import numpy as np
lazy import pikepdf
lazy import psdtags
lazy import pymupdf
lazy import tifffile
lazy from pikepdf import Array, Dictionary, Name, String

# ORA-arm deps; each proxy reifies on first `_ora` use in the to_process worker
lazy import pyvips
lazy import zlib
lazy from datetime import UTC, datetime
lazy from lxml import etree
lazy from stream_zip import NO_COMPRESSION_32, ZIP_AUTO, stream_zip

# PSD/PSB-arm + channel-codec deps; each proxy reifies on first `_psd`/`_tiff` use in the to_process worker
lazy import imagecodecs
lazy from PIL import Image
lazy from psd_tools import PSDImage
lazy from psd_tools import constants as psd_constants

if TYPE_CHECKING:
    import pikepdf
    from psd_tools.api.layers import Group  # the `_psd` group-folder type, annotation-only (never loaded at runtime)


# --- [TYPES] ----------------------------------------------------------------------------
class ExportTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer <g inkscape:groupmode=layer> document — Illustrator/Inkscape (core)
    PDF = "pdf"  # pymupdf OCG placement + pikepdf /Usage+/Order catalog enrichment — Acrobat (core)
    ORA = "ora"  # OpenRaster layered container (pyvips + lxml + stream-zip) — GIMP/Krita
    PSD = "psd"  # native Photoshop channel-stack document (psd-tools standing author; PhotoshopAPI the gated native writer) — Photoshop
    PSB = "psb"  # the large-document Photoshop Big container, the same `_psd` arm keyed by target/dimensions — Photoshop
    TIFF = "tiff"  # Photoshop-compatible layered TIFF via psdtags ImageSourceData + tifffile extratags


class Band(StrEnum):
    CORE = "core"  # in-process `to_thread` offload — drawsvg author + GIL-releasing pymupdf/pikepdf native render, off the event loop
    WORKER = "worker"  # the to_process worker — pyvips + lxml + stream_zip, off the runtime loader path


class BlendMode(StrEnum):  # the 16 CSS `mix-blend-mode` modes; the value IS the SVG token, `_ora_op`/`_vips_blend` derive the OpenRaster/libvips ops
    NORMAL = "normal"
    MULTIPLY = "multiply"
    SCREEN = "screen"
    OVERLAY = "overlay"
    DARKEN = "darken"
    LIGHTEN = "lighten"
    COLOR_DODGE = "color-dodge"
    COLOR_BURN = "color-burn"
    HARD_LIGHT = "hard-light"
    SOFT_LIGHT = "soft-light"
    DIFFERENCE = "difference"
    EXCLUSION = "exclusion"
    HUE = "hue"
    SATURATION = "saturation"
    COLOR = "color"
    LUMINOSITY = "luminosity"


class LayerIntent(
    StrEnum
):  # the ISO 32000 OCG `/Usage` application + `/Intent` hint; `_INTENT` -> add_ocg /Intent, `_USAGE` -> the `_enriched` /Usage sub-dict
    VIEW = "view"  # always visible; default, no explicit /Usage
    PRINT = "print"  # print-only — /Print /PrintState /ON, /View /ViewState /OFF
    EXPORT = "export"  # export-only — /Export /ExportState /ON, /View /ViewState /OFF
    DESIGN = "design"  # design-time /Intent /Design processing hint
    BACKGROUND = "background"  # /PageElement /Subtype /BG — structural background plate
    HEADER_FOOTER = "header_footer"  # /PageElement /Subtype /HF — running header/footer furniture
    FOREGROUND = "foreground"  # /PageElement /Subtype /FG — foreground overlay plate
    LOGO = "logo"  # /PageElement /Subtype /L — brand/logo mark


class PsdCompression(IntEnum):  # the PSD/PSB per-channel method code — ALSO the `psd_tools.constants.Compression` member value
    RAW = 0  # store raw channel bytes (imagecodecs `none`)
    RLE = 1  # PackBits RLE per scanline (imagecodecs `packbits`)
    ZIP = 2  # zlib deflate (imagecodecs `zlib`)
    ZIP_PREDICTION = 3  # delta predictor + raw-deflate — best ratio (imagecodecs `delta`+`deflate`)


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ExportFault:
    # the closed ADMISSION vocabulary `of` produces; a worker provider raise (`pyvips.Error`, `lxml.etree.LxmlError`,
    # a `BrokenWorkerProcess`) converts to the runtime `BoundaryFault` at the `async_boundary` `CLASSIFY` table, never here.
    tag: Literal["payload", "empty", "duplicate"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `ExportPayload` key paths
    empty: None = case()  # an empty layer set
    duplicate: tuple[str, ...] = (
        case()
    )  # layer names colliding across the by-`name` OCG match, the ORA `data/<name>.png` path, and the SVG group keying


# --- [CONSTANTS] ------------------------------------------------------------------------
_ORA_MIME: Final[bytes] = b"image/openraster"  # the OpenRaster magic, stored first and uncompressed in the ZIP
_THUMB: Final[int] = 256  # the OpenRaster `Thumbnails/thumbnail.png` 256x256 long-edge bound
_INKSCAPE_NS: Final = "http://www.inkscape.org/namespaces/inkscape"  # the `inkscape:` namespace the `<g inkscape:groupmode=layer>` layer idiom declares on the root `<svg>`
# NORMAL plus the four non-separable HSL modes are absent from `VipsBlendMode`, so `_vips_blend` falls them
# to `over` in the FLATTENED preview while the layer `stack.xml` carries their full `svg:` op for the editor.
_VIPS_UNMAPPED: Final[frozenset[BlendMode]] = frozenset({
    BlendMode.NORMAL,
    BlendMode.HUE,
    BlendMode.SATURATION,
    BlendMode.COLOR,
    BlendMode.LUMINOSITY,
})
# the pymupdf `add_ocg(intent=)` /Intent hint, DERIVED over `LayerIntent`: only `DESIGN` is design-time, every
# other intent the default `View`. The richer view-application and /PageElement marking are the `_enriched` /Usage concern.
_INTENT: Final[Map[LayerIntent, str]] = Map.of_seq((intent, "Design" if intent is LayerIntent.DESIGN else "View") for intent in LayerIntent)
# the /Usage sub-dict policy keyed `category -> cell`; VIEW omits its dict (default visible), the PRINT/EXPORT
# rows ride the `state` cell and the /PageElement rows ride the `Subtype` cell, `_STATE_KEY` naming each
# category's inner key so `_usage` emits `/<Category> << /<StateKey> /<Cell> >>` uniformly with no per-kind arm.
_USAGE: Final[Map[LayerIntent, frozendict[str, str]]] = Map.of_seq([
    (LayerIntent.PRINT, frozendict({"Print": "ON", "View": "OFF"})),
    (LayerIntent.EXPORT, frozendict({"Export": "ON", "View": "OFF"})),
    (LayerIntent.DESIGN, frozendict({"View": "ON"})),
    (LayerIntent.BACKGROUND, frozendict({"PageElement": "BG"})),
    (LayerIntent.HEADER_FOOTER, frozendict({"PageElement": "HF"})),
    (LayerIntent.FOREGROUND, frozendict({"PageElement": "FG"})),
    (LayerIntent.LOGO, frozendict({"PageElement": "L"})),
])
_STATE_KEY: Final[Map[str, str]] = Map.of_seq([("View", "ViewState"), ("Print", "PrintState"), ("Export", "ExportState"), ("PageElement", "Subtype")])


# --- [MODELS] ---------------------------------------------------------------------------
class Layer(Struct, frozen=True):
    # `name`/`source`/`bbox` are the positional contract compose/encode/draw/sheet/imposition construct;
    # every richer editor-panel attribute defaults after `bbox` so a 3-arg construction stays valid.
    name: str
    source: bytes
    bbox: tuple[float, float, float, float]
    visible: bool = True
    locked: bool = False
    opacity: float = 1.0
    blend: BlendMode = BlendMode.NORMAL
    intent: LayerIntent = LayerIntent.VIEW
    group: str = ""  # folder label; "" roots the layer at the drawing root
    color: str = ""  # SVG <g> `data-color` swatch (OCG/ORA carry no color slot); "" omits it

    def svg_attrs(self) -> dict[str, str]:
        # `inkscape:groupmode="layer"`+`inkscape:label` make Inkscape read the group AS a layer (a bare `<g id=>` is a
        # GROUP, not a layer) while `id` carries the Illustrator layer name; `blend.value` IS the `mix-blend-mode` token,
        # so no per-format blend table; the swatch rides a `data-color` attribute (no portable SVG layer-colour slot exists).
        style = ";".join(
            ([] if self.visible else ["display:none"])
            + ([f"opacity:{self.opacity:g}"] if self.opacity < 1.0 else [])
            + ([f"mix-blend-mode:{self.blend.value}"] if self.blend is not BlendMode.NORMAL else [])
        )
        return (
            {"id": self.name, "inkscape:groupmode": "layer", "inkscape:label": self.name}
            | ({"style": style} if style else {})
            | ({"data-color": self.color} if self.color else {})
        )


class LayerPolicy(Struct, frozen=True):
    # the trusted save-knob bundle (POLICY_VALUES: never a `garbage`/`deflate` flag tail on the signature)
    usage: str = "Artwork"  # the OCG /Usage category label pymupdf `add_ocg(usage=)` carries
    garbage: int = 3  # pymupdf `tobytes(garbage=)` xref compaction level
    deflate: bool = True
    channel: PsdCompression = (
        PsdCompression.ZIP_PREDICTION
    )  # the PSD/PSB/TIFF per-channel codec, capability-detected via imagecodecs `<CODEC>.available`


class LayerFact(Struct, frozen=True):
    # the bytes-plus-evidence carrier every arm returns; `contribute` reads it off the threaded owner
    data: bytes
    width: int = 0  # the SVG/ORA named-document viewport (the `Preview` facts)
    height: int = 0
    pages: int = 0  # the PDF page count (the `Egress` facts)
    layers: int = 0  # the authored-layer count, riding the `Egress` `overlays` slot


class LayerEngine(Struct, frozen=True):
    arm: Callable[["LayeredExport"], LayerFact]
    preview: bool = False  # True -> `ArtifactReceipt.Preview`; False -> `ArtifactReceipt.Egress`
    band: Band = Band.CORE  # the `_PLACE` placement lane: in-process core, or the `to_process` worker


# --- [BOUNDARIES] -----------------------------------------------------------------------
class ExportPayload(TypedDict, closed=True):
    base: NotRequired[ReadOnly[bytes]]  # the optional untrusted placed-layout PDF the `PDF` arm grafts onto


_PAYLOAD: Final = TypeAdapter(ExportPayload)


# --- [SERVICES] -------------------------------------------------------------------------
class LayeredExport(Struct, frozen=True):
    target: ExportTarget
    layers: tuple[Layer, ...]
    base: bytes = b""
    policy: LayerPolicy = field(default_factory=LayerPolicy)

    @classmethod
    def of(
        cls, target: ExportTarget, layers: tuple[Layer, ...], /, *, policy: LayerPolicy = LayerPolicy(), **raw: Unpack[ExportPayload]
    ) -> Result[Self, "ExportFault"]:
        if not layers:
            return Error(ExportFault(empty=None))
        if collisions := tuple(name for name, n in Counter(layer.name for layer in layers).items() if n > 1):
            return Error(ExportFault(duplicate=collisions))  # the interior keys layers by `name`; a collision silently drops an OCG/ORA-file/SVG leaf
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ExportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        return Ok(cls(target=target, layers=layers, base=payload.get("base", b""), policy=policy))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.layers) or 1))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (target ⊕ layers ⊕ base ⊕ policy) minted PRE-RUN — never a key over authored bytes.
        return ContentIdentity.of(f"export-{self.target}", (self.target, self.layers, self.base, self.policy), policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        return await async_boundary(f"export.layered.{self.target}", self._produced)

    async def _produced(self) -> ArtifactReceipt:
        engine = ENGINES[self.target]  # the thin pure core: one band-placed arm, one minted fact, one receipt
        fact = await _PLACE[engine.band](engine.arm, self)
        key = self._key
        # the `preview` discriminant folds the case: SVG/ORA/PSD/PSB/TIFF -> Preview, the OCG-layered PDF -> Egress
        return (
            ArtifactReceipt.Preview(key, fact.width, fact.height)
            if engine.preview
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)
        )


# --- [TABLES] ---------------------------------------------------------------------------
# the Photoshop blend correspondence is the shared member NAME, not a table: `BlendMode` names ARE the
# `psdtags.PsdBlendMode`/`psd_constants.BlendMode` names, so `<Enum>[blend.name]` derives both consumers with no table.
# the imagecodecs backend + tifffile codec name each PSD method code selects, capability-detected via `<CODEC>.available`.
_CHANNEL_CODEC: Final[Map[PsdCompression, str]] = Map.of_seq([
    (PsdCompression.RAW, "none"),
    (PsdCompression.RLE, "packbits"),
    (PsdCompression.ZIP, "zlib"),
    (PsdCompression.ZIP_PREDICTION, "deflate"),
])


def _channel(method: PsdCompression, /) -> PsdCompression:
    # capability-detection at the boundary (the `media/filtergraph#FILTER` native-vs-substitute shape): probe the
    # imagecodecs backend `<CODEC>.available` flag; an unbuilt channel core falls to RAW rather than a mid-write
    # `DelayedImportError`, so the achieved method rides the PSD/TIFF channel receipt and a build without a codec still writes.
    codec = getattr(imagecodecs, _CHANNEL_CODEC[method].upper())
    return method if codec.available else PsdCompression.RAW


# --- [OPERATIONS] -----------------------------------------------------------------------
def _viewport(layers: tuple[Layer, ...], /) -> tuple[float, float]:
    return (max((layer.bbox[2] for layer in layers), default=0.0), max((layer.bbox[3] for layer in layers), default=0.0))


def _ora_op(blend: BlendMode, /) -> str:
    # the OpenRaster `composite-op` for `stack.xml`: SVG-namespaced, `normal` mapping to the spec's `svg:src-over`.
    return "svg:src-over" if blend is BlendMode.NORMAL else f"svg:{blend.value}"


def _vips_blend(blend: BlendMode, /) -> str:
    # the libvips composite nickname for the FLATTENED `mergedimage.png`, derived off `blend.value`: the 11
    # separable CSS modes map by value (libvips spells the British `colour-dodge`/`colour-burn`), while the
    # `_VIPS_UNMAPPED` set (NORMAL + the 4 non-separable HSL modes absent from `VipsBlendMode`) falls to `over`.
    return "over" if blend in _VIPS_UNMAPPED else blend.value.replace("color", "colour")


def _svg(export: LayeredExport) -> LayerFact:
    width, height = _viewport(export.layers)
    drawing = drawsvg.Drawing(width, height, origin=(0.0, 0.0), **{"xmlns:inkscape": _INKSCAPE_NS})
    folders: dict[str, drawsvg.Group] = {}  # one `<g inkscape:groupmode=layer>` folder per distinct `group` label, NOT a parent-layer-name reference
    for layer in export.layers:
        leaf = drawsvg.Group(**layer.svg_attrs())
        leaf.append(drawsvg.Raw(layer.source.decode()))
        if layer.group and layer.group not in folders:
            folders[layer.group] = drawsvg.Group(**{"id": layer.group, "inkscape:groupmode": "layer", "inkscape:label": layer.group})
            drawing.append(folders[layer.group])
        folders.get(layer.group, drawing).append(leaf)  # nest under the group folder, else the root
    return LayerFact(drawing.as_svg().encode(), width=int(width), height=int(height), layers=len(export.layers))


def _pdf(export: LayeredExport) -> LayerFact:
    width, height = _viewport(export.layers)
    # the base-or-fresh pymupdf document brackets in one `with` so the native handle closes deterministically,
    # never GC-reaped; the per-source `src` brackets in its own nested `with`. `tobytes` runs while the handle is live.
    with pymupdf.open(stream=export.base, filetype="pdf") if export.base else pymupdf.open() as doc:
        page = doc[0] if export.base else doc.new_page(width=width, height=height)
        placed = []  # one `(layer, xref)` evidence stream; the visibility/lock partitions derive from it, never three co-mutated lists
        for layer in export.layers:
            xref = doc.add_ocg(layer.name, on=layer.visible, intent=_INTENT[layer.intent], usage=export.policy.usage)
            with pymupdf.open(stream=layer.source, filetype="pdf") as src:  # close each placed source once `show_pdf_page` copies it
                page.show_pdf_page(pymupdf.Rect(layer.bbox), src, 0, oc=xref)
            placed.append((layer, xref))
        doc.set_layer(
            0,
            on=[xref for layer, xref in placed if layer.visible],
            off=[xref for layer, xref in placed if not layer.visible],
            locked=[xref for layer, xref in placed if layer.locked],
        )
        rendered = doc.tobytes(garbage=export.policy.garbage, deflate=export.policy.deflate)
    return _enriched(rendered, export)


def _enriched(placed: bytes, export: LayeredExport) -> LayerFact:
    # the pikepdf catalog enrichment the pymupdf `add_ocg` placement cannot author: the per-layer `/Usage` and the
    # nested `/D/Order` folder tree, matched onto the placed OCGs by /Name; the `with` closes the native handle deterministically.
    with pikepdf.open(BytesIO(placed)) as pdf:
        ocprops = pdf.Root[Name.OCProperties]
        by_name = {str(ocg.get(Name.Name, "")): ocg for ocg in ocprops.get(Name.OCGs, Array())}
        for layer in export.layers:
            if layer.intent is not LayerIntent.VIEW and (ocg := by_name.get(layer.name)) is not None:
                ocg[Name.Usage] = _usage(layer.intent)
        ocprops[Name.D][Name("/Order")] = _order(export.layers, by_name)
        sink = BytesIO()
        pdf.save(sink)
        return LayerFact(sink.getvalue(), pages=len(pdf.pages), layers=len(export.layers))


def _usage(intent: LayerIntent) -> "pikepdf.Object":
    # the /Usage sub-dict — PRINT/EXPORT/DESIGN view-application OR /PageElement structural marking, both
    # `category -> {stateKey: cell}` rows — emitted uniformly; the nanobind `Dictionary` constructor coerces
    # keys to strings and rejects a `Name` key (`std::bad_cast`), so each `/Category /StateKey /Cell` rides subscript.
    usage = Dictionary()
    for category, state in _USAGE[intent].items():
        entry = Dictionary()
        entry[Name("/" + _STATE_KEY[category])] = Name("/" + state)
        usage[Name("/" + category)] = entry
    return usage


def _order(layers: tuple[Layer, ...], ocgs: "dict[str, pikepdf.Object]") -> "pikepdf.Array":
    # the nested /Order: top-level layers as direct OCG refs, grouped layers folded into `[/GroupTitle, …]`.
    grouped: dict[str, list[pikepdf.Object]] = {}
    direct: list[pikepdf.Object] = []
    for layer in layers:
        if (ref := ocgs.get(layer.name)) is not None:
            (grouped.setdefault(layer.group, []) if layer.group else direct).append(ref)
    return Array([*direct, *(Array([String(title), *members]) for title, members in grouped.items())])


def _ora(export: LayeredExport) -> LayerFact:
    # `pyvips` scales each layer's alpha by `opacity` and stacks the visible layers through the native `composite`
    # (ONE call, per-layer mode via `_vips_blend`) so the flattened `mergedimage.png` is FAITHFUL to the stack the editor
    # re-composites from; `lxml` authors `stack.xml`, `stream_zip` frames the ZIP with the `mimetype` stored first.
    width, height = (int(extent) for extent in _viewport(export.layers))
    loaded = [(layer, pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers]
    pngs = frozendict({layer.name: image.write_to_buffer(".png") for layer, image in loaded})
    visible = [(layer, image) for layer, image in loaded if layer.visible]
    placed = [
        (rgba * [1.0, 1.0, 1.0, layer.opacity] if layer.opacity < 1.0 else rgba).embed(
            int(layer.bbox[0]), int(layer.bbox[1]), width, height, extend=pyvips.Extend.BACKGROUND
        )
        for layer, image in visible
        if (rgba := image if image.hasalpha() else image.addalpha()) is not None  # the alpha-scale base bound once per layer
    ]
    modes = [_vips_blend(layer.blend) for layer, _ in visible]
    flattened = (
        placed[0].composite(placed[1:], modes[1:])
        if len(placed) > 1  # bottom layer is the OVER base; the rest carry their mode
        else placed[0]
        if placed
        else None
    )
    merged = flattened.write_to_buffer(".png") if flattened is not None else b""
    thumb = pyvips.Image.thumbnail_buffer(merged, _THUMB, height=_THUMB).write_to_buffer(".png") if merged else b""
    root = etree.Element("image", version="0.0.3", w=str(width), h=str(height))
    stack = etree.SubElement(root, "stack")
    folders: dict[str, etree._Element] = {}  # one `<stack name=group>` per distinct `group`, the ORA folder counterpart
    for layer in reversed(export.layers):  # OpenRaster lists the topmost layer first; the tuple is bottom-up paint order
        if layer.group and layer.group not in folders:
            folders[layer.group] = etree.SubElement(stack, "stack", name=layer.group)
        etree.SubElement(
            folders.get(layer.group, stack),
            "layer",
            name=layer.name,
            src=f"data/{layer.name}.png",
            x=str(int(layer.bbox[0])),
            y=str(int(layer.bbox[1])),
            opacity=f"{layer.opacity:g}",
            visibility="visible" if layer.visible else "hidden",
            **{"composite-op": _ora_op(layer.blend)},
        )
    manifest = etree.tostring(root, xml_declaration=True, encoding="UTF-8")
    now = datetime.now(UTC)
    members = (
        ("mimetype", now, 0o644, NO_COMPRESSION_32(len(_ORA_MIME), zlib.crc32(_ORA_MIME)), (_ORA_MIME,)),
        ("stack.xml", now, 0o644, ZIP_AUTO(len(manifest)), (manifest,)),
        *((f"data/{name}.png", now, 0o644, ZIP_AUTO(len(png)), (png,)) for name, png in pngs.items()),
        ("mergedimage.png", now, 0o644, ZIP_AUTO(len(merged)), (merged,)),
        ("Thumbnails/thumbnail.png", now, 0o644, ZIP_AUTO(len(thumb)), (thumb,)),
    )
    return LayerFact(b"".join(stream_zip(members)), width=width, height=height, layers=len(export.layers))


def _rgba_array(image: "pyvips.Image", /) -> "np.ndarray":
    rgba = image if image.hasalpha() else image.addalpha()
    rgba = rgba.cast("uchar")
    return np.ndarray(buffer=rgba.write_to_memory(), dtype=np.uint8, shape=(rgba.height, rgba.width, rgba.bands))[:, :, :4].copy()


def _psd_flags(layer: Layer, /) -> object:
    # `PsdLayerFlag.VISIBLE` is the RAW Adobe flag bit (value 2) whose SET state means the layer is HIDDEN
    # (ISO/Adobe layer-flags bit 1: 0 = visible, 1 = hidden); psdtags is a raw codec with no inverting accessor,
    # so authoring `VISIBLE` writes byte 0x02 which every conforming reader (Photoshop, psd_tools' `not flags & 2`)
    # decodes as hidden — the bit is therefore set ONLY for a hidden layer, else a visible layer opens hidden.
    hidden = psdtags.PsdLayerFlag.VISIBLE if not layer.visible else psdtags.PsdLayerFlag(0)
    return hidden | psdtags.PsdLayerFlag.TRANSPARENCY_PROTECTED if layer.locked else hidden


def _psd_layer(layer: Layer, image: "pyvips.Image", compression: "psdtags.PsdCompressionType", /) -> "psdtags.PsdLayer":
    # every layer channel carries the capability-detected `compression` so `TiffImageSourceData.tifftag` compresses the
    # real layer payload, never the silent RAW default that would leave the layers uncompressed while only the strip coded.
    rgba = _rgba_array(image)
    top, left, bottom, right = int(layer.bbox[1]), int(layer.bbox[0]), int(layer.bbox[3]), int(layer.bbox[2])
    return psdtags.PsdLayer(
        name=layer.name,
        rectangle=psdtags.PsdRectangle(top, left, bottom, right),
        channels=[
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL0, compression, data=rgba[:, :, 0]),
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL1, compression, data=rgba[:, :, 1]),
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL2, compression, data=rgba[:, :, 2]),
            psdtags.PsdChannel(psdtags.PsdChannelId.TRANSPARENCY_MASK, compression, data=rgba[:, :, 3]),
        ],
        opacity=max(0, min(255, round(layer.opacity * 255))),
        blendmode=psdtags.PsdBlendMode[layer.blend.name],  # derived by shared member name, no parallel blend table
        flags=_psd_flags(layer),
    )


def _tiff(export: LayeredExport) -> LayerFact:
    width, height = (int(extent) for extent in _viewport(export.layers))
    loaded = [(layer, pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers]
    visible = [(layer, image) for layer, image in loaded if layer.visible]
    placed = [
        (rgba * [1.0, 1.0, 1.0, layer.opacity] if layer.opacity < 1.0 else rgba).embed(
            int(layer.bbox[0]), int(layer.bbox[1]), width, height, extend=pyvips.Extend.BACKGROUND
        )
        for layer, image in visible
        if (rgba := image if image.hasalpha() else image.addalpha()) is not None
    ]
    modes = [_vips_blend(layer.blend) for layer, _ in visible]
    flattened = placed[0].composite(placed[1:], modes[1:]) if len(placed) > 1 else placed[0] if placed else None
    merged = _rgba_array(flattened) if flattened is not None else np.zeros((height, width, 4), dtype=np.uint8)
    method = _channel(export.policy.channel)  # capability-detected ONCE, driving BOTH the layer channels and the merged strip
    channel_codec = psdtags.PsdCompressionType(
        int(method)
    )  # PSD method code -> psdtags per-channel codec (RAW fallback already applied by `_channel`)
    source_data = psdtags.TiffImageSourceData(
        psdtags.PsdFormat.BE32BIT,
        psdtags.PsdLayers(psdtags.PsdKey.LAYER, [_psd_layer(layer, image, channel_codec) for layer, image in loaded], has_transparency=True),
        psdtags.PsdUserMask(),
        name="layered.tif",
    )
    resources = psdtags.TiffImageResources(psdtags.PsdFormat.BE32BIT, [], name="layered.tif")
    sink = BytesIO()
    codec = _CHANNEL_CODEC[method]  # the same method as the merged-strip codec (tifffile routes to the same imagecodecs core)
    tifffile.imwrite(
        sink,
        merged,
        photometric="rgb",
        extrasamples=("unassalpha",),
        metadata=None,
        compression=None if codec == "none" else codec,
        byteorder=source_data.byteorder,
        extratags=(source_data.tifftag(maxworkers=4), resources.tifftag()),
    )
    return LayerFact(sink.getvalue(), width=width, height=height, layers=len(export.layers))


def _layered_raise(fault: object) -> object:
    raise ValueError(str(fault))


def _psd(export: LayeredExport) -> LayerFact:
    # the native Photoshop channel-stack author on the `Band.WORKER` worker: one `PSDImage` holding one
    # `create_pixel_layer` per `Layer`, one native group per distinct `group` label, and a per-channel `Compression`
    # capability-detected off the `imagecodecs` backend. psd-tools is the cp315-present standing author; PhotoshopAPI
    # (gated on a cp315 wheel) is the categorical-best writer selected in its place ONCE one lands — one writer per
    # interpreter, never two parallel. Bit depth is fixed at 8 off the `uint8` `_rgba_array`, never a caller knob.
    width, height = (int(extent) for extent in _viewport(export.layers))
    compression = psd_constants.Compression(int(_channel(export.policy.channel)))  # PSD method code -> psd_tools member by value
    document = PSDImage.new("RGBA", (width, height), depth=8)
    if export.target is ExportTarget.PSB:
        # Exemption: psd-tools exposes no public PSB author (`new` has no `version` arg and `.version` has no setter), so the
        # header `version = 2` field is the one private seam that widens the length fields to the PSB (>30000px) container — the
        # `PSB` target authoring a real Big-document file rather than silently degrading to a `version = 1` PSD.
        document._record.header.version = 2
    folders: dict[str, "Group"] = {}  # one native `Group` per distinct `group` label, minted on first use and re-parented into
    for layer in export.layers:
        image = Image.fromarray(
            _rgba_array(pyvips.Image.new_from_buffer(layer.source, ""))
        )  # the shared ORA/TIFF decode, lifted to PIL for `create_pixel_layer`
        node = document.create_pixel_layer(
            image,
            name=layer.name,
            top=int(layer.bbox[1]),
            left=int(layer.bbox[0]),
            compression=compression,
            opacity=round(layer.opacity * 255),
            blend_mode=psd_constants.BlendMode[layer.blend.name],
        )
        node.visible = layer.visible
        if layer.locked:
            node.lock(psd_constants.ProtectedFlags.COMPLETE)
        if layer.group:  # mint one native `Group` per distinct label on first use (never `setdefault`, whose default arg
            if layer.group not in folders:  # would re-mint an empty group per layer through the `create_group` side effect)
                folders[layer.group] = document.create_group(name=layer.group)
            node.move_to_group(folders[layer.group])
    sink = BytesIO()
    document.save(sink)  # psd-tools `save` writes the header-version-keyed PSD/PSB stream (no scratch path); the `PSB` target forced version 2 above
    return LayerFact(sink.getvalue(), width=width, height=height, layers=len(export.layers))


# --- [COMPOSITION] ----------------------------------------------------------------------
# the worker offloads thread the module-level `_GATE` `CapacityLimiter` (the `export/indesign#INDESIGN` IDML worker
# counterpart) so N concurrent `emit` calls share a fixed subprocess pool; the `async_boundary` default `Exception`
# capture plus its `CLASSIFY` table routes a worker death to `resource` where a hand-named `catch` tuple would miss it
# (`import pyvips` itself raises with native libvips unprovisioned). The `SVG`/`PDF` arms ride the separate `_THREAD_GATE`.


async def _threaded(arm: Callable[["LayeredExport"], LayerFact], export: "LayeredExport", /) -> LayerFact:
    # the in-process lane: the GIL-releasing pymupdf/pikepdf render crosses `to_thread` so the body never runs on the loop.
    return (await LanePolicy.offload(arm, export, modality=Modality.THREAD, retry=RetryClass.OCCT)).default_with(_layered_raise)


async def _offloaded(arm: Callable[["LayeredExport"], LayerFact], export: "LayeredExport", /) -> LayerFact:
    return (await LanePolicy.offload(arm, export, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_layered_raise)


_PLACE: Final[Map[Band, Callable[..., Awaitable[LayerFact]]]] = Map.of_seq([(Band.CORE, _threaded), (Band.WORKER, _offloaded)])
ENGINES: Final[Map[ExportTarget, LayerEngine]] = Map.of_seq([
    (ExportTarget.SVG, LayerEngine(_svg, preview=True)),
    (ExportTarget.PDF, LayerEngine(_pdf)),
    (ExportTarget.ORA, LayerEngine(_ora, preview=True, band=Band.WORKER)),
    (ExportTarget.PSD, LayerEngine(_psd, preview=True, band=Band.WORKER)),
    (ExportTarget.PSB, LayerEngine(_psd, preview=True, band=Band.WORKER)),  # the same `_psd` arm; the target/dimensions select the PSB container
    (ExportTarget.TIFF, LayerEngine(_tiff, preview=True, band=Band.WORKER)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
