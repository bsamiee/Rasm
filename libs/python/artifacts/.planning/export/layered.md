# [PY_ARTIFACTS_LAYERED]

`LayeredExport` owns the editable named-layer export close — it authors the separable, toggleable, lockable layer structure an external editor re-orders and re-colors, the inverse of the `document/egress#FINISH` `FINISHERS` table that strips the layers this owner authors. One owner discriminates the editor family over the closed `ExportTarget` vocabulary, each target a `LayerEngine` row in `ENGINES` binding one `LayerFact` arm and its crossing `KernelTrait`. `LayerFact` separates `preview` and `egress` payloads, so receipt dispatch reads the fact without a boolean or default-filled cross-mode record. Placement, scaling, and rasterization stay upstream. `BlendMode` composes from `graphic/color/derive#DERIVE`; its value derives SVG and `photoshopapi.enum.BlendMode`, and its name derives `psdtags.PsdBlendMode`.

Admission is trusted layer material plus one optional untrusted blob: `Layer.issue` rejects empty identity/source, degenerate bounds, and invalid opacity; `LayeredExport.of` rejects an empty layer set, duplicate names, and PSD/PSB dimensions outside the selected container. `ExportPayload` and `TypeAdapter` admit the external `base` PDF. Each arm crosses `self.lane.offload(Kernel.of(engine.arm, engine.trait), self)`; SVG/PDF ride the `RELEASING` thread arm, and ORA/PSD/PSB/TIFF the `HOSTILE` process pool. Runtime lanes own capacity and fault conversion. `LayeredExport.emit` is the `ArtifactWork.work` coroutine and threads the pre-run `_key` into `ArtifactReceipt.Preview` or `ArtifactReceipt.Egress`.

## [01]-[INDEX]

- [01]-[LAYERED]: the one editable named-layer export owner — `LayeredExport` discriminating the editor family over the closed `ExportTarget` (`SVG`/`PDF`/`ORA`/`PSD`/`PSB`/`TIFF`) keyed to the `ENGINES` modality-dispatched table, each arm authoring the layer structure the flat egress omits and returning `ArtifactReceipt.Preview` or `ArtifactReceipt.Egress`.

## [02]-[LAYERED]

- Owner: `LayeredExport` discriminates the editor family over the closed `ExportTarget` keyed to the `ENGINES` policy table binding each arm's `LayerFact` body and crossing `KernelTrait`. `LayerFact.preview` carries named-document bytes, viewport, and layer count; `LayerFact.egress` carries PDF bytes, page count, and authored-layer count. `Layer` is the row every visual producer constructs — `name`/`source`/`bbox` positional, the editor-panel axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) defaulting after `bbox`; `group` is a folder label projected to all editors, never a parent-layer-name reference. `OcgIntent` absorbs the producer vocabulary and lowers every editorial semantic onto one OCG usage row. `LayerPolicy` is the trusted save-policy bundle; `BlendMode` is `graphic/color/derive#DERIVE`'s canonical vocabulary whose value is the SVG `mix-blend-mode` token and whose name derives the Photoshop members.
- Cases: each `ExportTarget` is a `LayerEngine` row with its editability ruling — `SVG` uses named `drawsvg` layer groups, `PDF` uses `pymupdf` OCG placement plus `pikepdf` `/OCProperties`, `ORA` uses `pyvips`/`lxml`/`stream-zip`, `PSD`/`PSB` use `PhotoshopAPI` native channel stacks with `psd-tools` structural readback, and `TIFF` uses `psdtags`/`tifffile`. `_PSB_FLOOR` refuses a target on the wrong side of the PSD dimension bound, and the output suffix selects the native container. Illustrator consumes the named-layer `SVG`; renamed OCG PDF does not create Illustrator layer-panel structure.
- Auto: `_emit` resolves `engine = ENGINES[self.target]`, crosses `self.lane.offload(Kernel.of(engine.arm, engine.trait), self)` so the arm runs on the thread arm or the warm process pool, never inline on the event loop, and maps the returned rail onto the `preview`-discriminated `ArtifactReceipt` threading the PRE-RUN `_key` — key-over-INPUT (`ContentIdentity.key` over the deterministic-msgpack `target ⊕ layers ⊕ base ⊕ policy` bytes), never a key over authored bytes, so `receipt.slot == node.key` and the plan's sub-graph elision short-circuits before the arm runs. `_svg` folds each `Layer` into one leaf `Group(**layer.svg_attrs())` nested under one `<g inkscape:groupmode=layer>` folder minted per distinct `group`. `_pdf` opens the `pymupdf` document in a `with` that closes the native handle (never GC-reaped), mints one `add_ocg` xref per layer, places each source in a nested `with`, drives the visibility/lock partitions through `set_layer`, then `_enriched` folds the per-layer `/Usage` sub-dict and `/Order` folder tree onto the `pikepdf` `/OCProperties` catalog. `_ora` scales alpha by `opacity` and stacks the visible layers through the native `composite(modes)` under their `_vips_blend` modes for a FAITHFUL `mergedimage.png`, authors `stack.xml` through `lxml`, and frames the ZIP through `stream_zip`; the ORA/TIFF alpha-scale-embed-composite stack is the one shared `_flattened` fold, never a per-arm copy. `_tiff_rows` projects `group` onto the flat layered-TIFF grammar — a `PsdSectionDividerSetting` `BOUNDING_SECTION_DIVIDER` row below the members and a named `OPEN_FOLDER` row above them at the group's first paint position — so every editor family reads the same folder structure.
- Output: `LayerFact` is the closed bytes-plus-evidence family every arm returns; each case carries only its mode's required payload, with no null or default ghost fields.
- Receipt: `_emit` totally matches `LayerFact`. `preview` maps to `ArtifactReceipt.Preview` with layer count and target in `scores`; `egress` maps to `ArtifactReceipt.Egress` with authored layers in `overlays`. Layered export adds no receipt case.
- Growth: a new editable-export target is one `ExportTarget` member, one `LayerEngine` row, and one arm over the engine algebra. A new layer attribute is one `Layer` field threaded into each projection. A compositing mode extends `graphic/color/derive#DERIVE`, and each lowering derives the provider member by value or name; Photoshop-only modes remain outside the CSS contract. Codec, OCG usage, lane, save, admission, and untrusted-payload growth extend their existing closed owners.
- Boundary: a per-producer export class family, parallel name/source/flag lists, hand-emitted SVG groups, hand-written PDF OCG streams, local `BlendMode` twins, per-format blend tables, lossy flattening, class-qualified offload, inline native work, unbracketed document handles, duplicate names, and module batch entrypoints are rejected. `PhotoshopAPI` alone authors PSD/PSB; `psd-tools` only reopens the finished bytes. `psdtags`/`tifffile` own TIFF. Rasterization stays graphic-owned, placement stays composition-owned, and PDF/A/PAdES/flat egress stay their close owners.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections import Counter
from collections.abc import Callable
from enum import IntEnum, StrEnum
from itertools import groupby
from io import BytesIO
from math import ceil
from pathlib import Path
from tempfile import TemporaryDirectory
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from msgspec import Struct, field
from msgspec.msgpack import Encoder
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt
from rasm.artifacts.graphic.color.derive import BlendMode
from rasm.artifacts.graphic.layer import FlatLayer, LayerFault, LayerPlan, flattened as graphic_flattened

# core-arm providers; each proxy reifies in-process on first SVG/PDF arm use
lazy import drawsvg
lazy import numpy as np
lazy import pikepdf
lazy import psdtags
lazy import pymupdf
lazy import tifffile
lazy from pikepdf import Array, Dictionary, Name, String

# ORA-arm deps; each proxy reifies on first `_ora` use in the process worker
lazy import pyvips
lazy import zlib
lazy from datetime import UTC, datetime
lazy from lxml import etree
lazy from stream_zip import NO_COMPRESSION_32, ZIP_AUTO, stream_zip

# PSD/PSB-arm + channel-codec deps; each proxy reifies on first `_psd`/`_tiff` use in the process worker
lazy import imagecodecs
lazy import photoshopapi as psapi
lazy from psd_tools import PSDImage

if TYPE_CHECKING:
    import pikepdf


# --- [TYPES] ----------------------------------------------------------------------------
class ExportTarget(StrEnum):
    SVG = "svg"  # drawsvg named-layer <g inkscape:groupmode=layer> document — Illustrator/Inkscape (core)
    PDF = "pdf"  # pymupdf OCG placement + pikepdf /Usage+/Order catalog enrichment — Acrobat (core)
    ORA = "ora"  # OpenRaster layered container (pyvips + lxml + stream-zip) — GIMP/Krita
    PSD = "psd"  # native Photoshop channel-stack document — Photoshop
    PSB = "psb"  # the large-document Photoshop Big container, the same `_psd` arm past the 30000 px `_PSB_FLOOR` — Photoshop
    TIFF = "tiff"  # Photoshop-compatible layered TIFF via psdtags ImageSourceData + tifffile extratags


class OcgIntent(StrEnum):
    # ISO 32000 OCG `/Usage` application + `/Intent` hint; `_INTENT` -> add_ocg /Intent, `_USAGE` -> the `_enriched` /Usage sub-dict
    VIEW = "view"  # always visible; default, no explicit /Usage
    PRINT = "print"  # print-only — /Print /PrintState /ON, /View /ViewState /OFF
    EXPORT = "export"  # export-only — /Export /ExportState /ON, /View /ViewState /OFF
    DESIGN = "design"  # design-time /Intent /Design processing hint
    BACKGROUND = "background"  # /PageElement /Subtype /BG — structural background plate
    HEADER_FOOTER = "header_footer"  # /PageElement /Subtype /HF — running header/footer furniture
    FOREGROUND = "foreground"  # /PageElement /Subtype /FG — foreground overlay plate
    LOGO = "logo"  # /PageElement /Subtype /L — brand/logo mark
    ANNOTATION = "annotation"
    FIGURE = "figure"
    LINEWORK = "linework"
    OVERLAY = "overlay"
    REFERENCE = "reference"
    SYMBOL = "symbol"


class PsdCompression(IntEnum):  # channel compression policy shared by native PSD and layered TIFF lowering
    RAW = 0  # store raw channel bytes (imagecodecs `none`)
    RLE = 1  # PackBits RLE per scanline (imagecodecs `packbits`)
    ZIP = 2  # zlib deflate (imagecodecs `zlib`)
    ZIP_PREDICTION = 3  # delta predictor + raw-deflate — best ratio (imagecodecs `delta`+`deflate`)


# --- [CONSTANTS] ------------------------------------------------------------------------
_ORA_MIME: Final[bytes] = b"image/openraster"  # the OpenRaster magic, stored first and uncompressed in the ZIP
_PSB_FLOOR: Final[int] = 30_000  # PSD dimension ceiling; larger canvases require PSB
_PSB_CEILING: Final[int] = 300_000  # native PSD/PSB hard dimension cap
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
# pymupdf `add_ocg(intent=)` /Intent hint, DERIVED over `OcgIntent`: only `DESIGN` is design-time, every
# other intent the default `View`. The richer view-application and /PageElement marking are the `_enriched` /Usage concern.
_INTENT: Final[frozendict[OcgIntent, str]] = frozendict({intent: "Design" if intent is OcgIntent.DESIGN else "View" for intent in OcgIntent})
# semantic graphic/layer intent value -> the OCG usage row `Layer.of_plan` lowers it onto; unmapped semantics stay VIEW.
_OCG_OF_SEMANTIC: Final[frozendict[str, OcgIntent]] = frozendict({
    "background": OcgIntent.BACKGROUND,
    "guide": OcgIntent.DESIGN,
    "mask": OcgIntent.DESIGN,
    "grid": OcgIntent.DESIGN,
    "datum": OcgIntent.DESIGN,
    "issue": OcgIntent.PRINT,
    "overlay": OcgIntent.FOREGROUND,
})
# /Usage sub-dict policy keyed `category -> cell`; VIEW omits its dict (default visible), the PRINT/EXPORT
# rows ride the `state` cell and the /PageElement rows ride the `Subtype` cell, `_STATE_KEY` naming each
# category's inner key so `_usage` emits `/<Category> << /<StateKey> /<Cell> >>` uniformly with no per-kind arm.
_USAGE: Final[frozendict[OcgIntent, frozendict[str, str]]] = frozendict({
    OcgIntent.PRINT: frozendict({"Print": "ON", "View": "OFF"}),
    OcgIntent.EXPORT: frozendict({"Export": "ON", "View": "OFF"}),
    OcgIntent.DESIGN: frozendict({"View": "ON"}),
    OcgIntent.BACKGROUND: frozendict({"PageElement": "BG"}),
    OcgIntent.HEADER_FOOTER: frozendict({"PageElement": "HF"}),
    OcgIntent.FOREGROUND: frozendict({"PageElement": "FG"}),
    OcgIntent.LOGO: frozendict({"PageElement": "L"}),
    OcgIntent.ANNOTATION: frozendict({"View": "ON"}),
    OcgIntent.FIGURE: frozendict({"View": "ON"}),
    OcgIntent.LINEWORK: frozendict({"View": "ON"}),
    OcgIntent.OVERLAY: frozendict({"View": "ON"}),
    OcgIntent.REFERENCE: frozendict({"View": "ON"}),
    OcgIntent.SYMBOL: frozendict({"View": "ON"}),
})
_STATE_KEY: Final[frozendict[str, str]] = frozendict({"View": "ViewState", "Print": "PrintState", "Export": "ExportState", "PageElement": "Subtype"})
# imagecodecs backends + the tifffile codec name each PSD method code selects; ZIP_PREDICTION probes BOTH the
# delta predictor and the deflate core, and the LAST name is the tifffile merged-strip codec.
_CHANNEL_CODEC: Final[frozendict[PsdCompression, tuple[str, ...]]] = frozendict({
    PsdCompression.RAW: ("none",),
    PsdCompression.RLE: ("packbits",),
    PsdCompression.ZIP: ("zlib",),
    PsdCompression.ZIP_PREDICTION: ("delta", "deflate"),
})


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
    intent: OcgIntent = OcgIntent.VIEW
    group: str = ""  # folder label; "" roots the layer at the drawing root
    color: str = ""  # SVG <g> `data-color` swatch (OCG/ORA carry no color slot); "" omits it

    @classmethod
    def of_plan(cls, plan: "LayerPlan", bbox: tuple[float, float, float, float], /) -> "Result[tuple[Layer, ...], LayerFault]":
        # semantic-tree bridge: graphic/layer's `flattened` projection lowers each FRAGMENT leaf into one
        # writer row — the composed path names the folder `group`, and the leaf's `LayerState` editor axes land on the
        # row's own fields — so the tree stays the ONE semantic model and this flat value stays its writer row.
        def _row(flat: "FlatLayer") -> "Layer | None":
            path, node = flat
            if node.tag != "leaf" or node.leaf[1].tag != "fragment":
                return None
            meta, content = node.leaf
            return cls(
                name=path[-1],
                source=content.fragment,
                bbox=bbox,
                visible=meta.state.visible,
                locked=meta.state.locked,
                opacity=meta.opacity,
                blend=meta.blend,
                intent=_OCG_OF_SEMANTIC.get(meta.intent.value, OcgIntent.VIEW),
                group="/".join(path[:-1]),
            )

        return graphic_flattened(plan).map(lambda rows: tuple(row for flat in rows if (row := _row(flat)) is not None))

    @property
    def issue(self) -> Option[str]:
        faults = (
            Some("name") if not self.name.strip() else Nothing,
            Some("source") if not self.source else Nothing,
            Some("bbox")
            if self.bbox[0] < 0.0 or self.bbox[1] < 0.0 or self.bbox[2] <= self.bbox[0] or self.bbox[3] <= self.bbox[1]
            else Nothing,
            Some("opacity") if not 0.0 <= self.opacity <= 1.0 else Nothing,
        )
        return next((fault for fault in faults if fault.is_some()), Nothing)

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
    # trusted save-knob bundle (POLICY_VALUES: never a `garbage`/`deflate` flag tail on the signature)
    usage: str = "Artwork"  # the OCG /Usage category label pymupdf `add_ocg(usage=)` carries
    garbage: int = 3  # pymupdf `tobytes(garbage=)` xref compaction level
    deflate: bool = True
    channel: PsdCompression = PsdCompression.ZIP_PREDICTION  # native PSD/PSB policy; TIFF capability-detects the matching imagecodecs backend


@tagged_union(frozen=True)
class LayerFact:
    tag: Literal["preview", "egress"] = tag()
    preview: tuple[bytes, int, int, int] = case()
    egress: tuple[bytes, int, int] = case()


class LayerEngine(Struct, frozen=True):
    arm: Callable[["LayeredExport"], LayerFact]
    trait: KernelTrait = KernelTrait.RELEASING  # the kernel crossing: RELEASING for the in-process arms, HOSTILE for the loader-hostile workers


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ExportFault:
    # closed ADMISSION vocabulary `of` produces; a worker provider raise (`pyvips.Error`, `lxml.etree.LxmlError`,
    # a `BrokenProcessPool`) converts to the runtime `BoundaryFault` at the lane's boundary CLASSIFY table, never here.
    tag: Literal["payload", "empty", "layer", "duplicate", "container"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `ExportPayload` key paths
    empty: None = case()  # an empty layer set
    layer: tuple[int, str] = case()  # layer index and invalid field
    duplicate: tuple[str, ...] = case()  # layer names colliding across the by-`name` OCG match, the ORA `data/<name>.png` path, and the SVG group keying
    container: tuple[int, int] = case()  # a PSD canvas past `_PSB_FLOOR`, or a PSB canvas at or under it or past `_PSB_CEILING`


# --- [BOUNDARIES] -----------------------------------------------------------------------
class ExportPayload(TypedDict, closed=True):
    base: NotRequired[ReadOnly[bytes]]  # the optional untrusted placed-layout PDF the `PDF` arm grafts onto


_PAYLOAD: Final = TypeAdapter(ExportPayload)


# --- [SERVICES] -------------------------------------------------------------------------
class LayeredExport(Struct, frozen=True):
    target: ExportTarget
    layers: tuple[Layer, ...]
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner.
    lane: LanePolicy
    base: bytes = b""
    policy: LayerPolicy = field(default_factory=LayerPolicy)

    @classmethod
    def of(
        cls,
        target: ExportTarget,
        layers: tuple[Layer, ...],
        lane: LanePolicy,
        /,
        *,
        policy: LayerPolicy = LayerPolicy(),
        **raw: Unpack[ExportPayload],
    ) -> Result[Self, "ExportFault"]:
        # `lane` is positional-only ahead of the `Unpack` band, so payload validation never sees it and the
        # factory constructs the same lane-bearing owner the composition root projects.
        if not layers:
            return Error(ExportFault(empty=None))
        issues = tuple((index, layer.issue) for index, layer in enumerate(layers))
        invalid = next((issue.map(lambda reason: (index, reason)) for index, issue in issues if issue.is_some()), Nothing)
        match invalid:
            case Option(tag="some", some=fault):
                return Error(ExportFault(layer=fault))
            case _:
                pass
        if collisions := tuple(name for name, n in Counter(layer.name for layer in layers).items() if n > 1):
            return Error(ExportFault(duplicate=collisions))  # the interior keys layers by `name`; a collision silently drops an OCG/ORA-file/SVG leaf
        width, height = (ceil(extent) for extent in _viewport(layers))
        # PhotoshopAPI selects the container by suffix and enforces the PSD/PSB dimension bounds.
        if target in {ExportTarget.PSD, ExportTarget.PSB} and (
            max(width, height) > _PSB_CEILING
            or (target is ExportTarget.PSB and max(width, height) <= _PSB_FLOOR)
            or (target is ExportTarget.PSD and max(width, height) > _PSB_FLOOR)
        ):
            return Error(ExportFault(container=(width, height)))
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ExportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        return Ok(cls(target=target, layers=layers, lane=lane, base=payload.get("base", b""), policy=policy))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.layers) or 1))

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: canonical (target ⊕ layers ⊕ base ⊕ policy) minted PRE-RUN — never a key over authored bytes;
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`).
        return ContentIdentity.key(f"export-{self.target}", _CANON.encode((self.target, self.layers, self.base, self.policy)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # one lane crossing, one rail: the offload converts a worker raise to `BoundaryFault`, `.map` threads the
        # PRE-RUN key onto the receipt (receipt.slot == node.key) — no second boundary, no raise-bridge.
        engine = ENGINES[self.target]
        crossed = await self.lane.offload(Kernel.of(engine.arm, engine.trait), self)

        def receipt(fact: LayerFact, /) -> ArtifactReceipt:
            match fact:
                case LayerFact(tag="preview", preview=(data, width, height, layers)):
                    return ArtifactReceipt.Preview(
                        self._key,
                        width,
                        height,
                        len(data),
                        frozendict({"layers": float(layers), "target": self.target.value}),
                    )
                case LayerFact(tag="egress", egress=(data, pages, layers)):
                    return ArtifactReceipt.Egress(self._key, len(data), pages, 0, 0, layers)
                case unreachable:
                    assert_never(unreachable)

        return crossed.map(receipt)


# --- [OPERATIONS] -----------------------------------------------------------------------
_CANON: Final[Encoder] = Encoder(order="deterministic")  # every `_key` payload member is msgpack-native (Struct/StrEnum/IntEnum/bytes)


def _viewport(layers: tuple[Layer, ...], /) -> tuple[float, float]:
    return (max((layer.bbox[2] for layer in layers), default=0.0), max((layer.bbox[3] for layer in layers), default=0.0))


def _channel(method: PsdCompression, /) -> PsdCompression:
    # capability-detection at the boundary (the `media/filtergraph#FILTER` native-vs-substitute shape): probe every
    # imagecodecs backend the method layers (`ZIP_PREDICTION` = delta + deflate); an unbuilt core falls to RAW rather
    # than a mid-write `DelayedImportError`, so the achieved method rides the channel bytes and a lean build still writes.
    return method if all(getattr(imagecodecs, name.upper()).available for name in _CHANNEL_CODEC[method]) else PsdCompression.RAW


def _ora_op(blend: BlendMode, /) -> str:
    # OpenRaster `composite-op` for `stack.xml`: SVG-namespaced, `normal` mapping to the spec's `svg:src-over`.
    return "svg:src-over" if blend is BlendMode.NORMAL else f"svg:{blend.value}"


def _vips_blend(blend: BlendMode, /) -> str:
    # libvips composite nickname for the FLATTENED `mergedimage.png`, derived off `blend.value`: the 11
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
    return LayerFact(preview=(drawing.as_svg().encode(), int(width), int(height), len(export.layers)))


def _pdf(export: LayeredExport) -> LayerFact:
    width, height = _viewport(export.layers)
    # base-or-fresh pymupdf document brackets in one `with` so the native handle closes deterministically,
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
    # pikepdf catalog enrichment the pymupdf `add_ocg` placement cannot author: the per-layer `/Usage` and the
    # nested `/D/Order` folder tree, matched onto the placed OCGs by /Name; the `with` closes the native handle deterministically.
    with pikepdf.open(BytesIO(placed)) as pdf:
        ocprops = pdf.Root[Name.OCProperties]
        by_name = {str(ocg.get(Name.Name, "")): ocg for ocg in ocprops.get(Name.OCGs, Array())}
        for layer in export.layers:
            if layer.intent is not OcgIntent.VIEW and (ocg := by_name.get(layer.name)) is not None:
                ocg[Name.Usage] = _usage(layer.intent)
        ocprops[Name.D][Name("/Order")] = _order(export.layers, by_name)
        sink = BytesIO()
        pdf.save(sink)
        return LayerFact(egress=(sink.getvalue(), len(pdf.pages), len(export.layers)))


def _usage(intent: OcgIntent) -> "pikepdf.Object":
    # /Usage sub-dict — PRINT/EXPORT/DESIGN view-application OR /PageElement structural marking, both
    # `category -> {stateKey: cell}` rows — emitted uniformly; the nanobind `Dictionary` constructor coerces
    # keys to strings and rejects a `Name` key (`std::bad_cast`), so each `/Category /StateKey /Cell` rides subscript.
    usage = Dictionary()
    for category, state in _USAGE[intent].items():
        entry = Dictionary()
        entry[Name("/" + _STATE_KEY[category])] = Name("/" + state)
        usage[Name("/" + category)] = entry
    return usage


def _order(layers: tuple[Layer, ...], ocgs: "dict[str, pikepdf.Object]") -> "pikepdf.Array":
    # nested /Order: top-level layers as direct OCG refs, grouped layers folded into `[/GroupTitle, …]`.
    grouped: dict[str, list[pikepdf.Object]] = {}
    direct: list[pikepdf.Object] = []
    for layer in layers:
        if (ref := ocgs.get(layer.name)) is not None:
            (grouped.setdefault(layer.group, []) if layer.group else direct).append(ref)
    return Array([*direct, *(Array([String(title), *members]) for title, members in grouped.items())])


def _flattened(export: LayeredExport, width: int, height: int, /) -> "tuple[tuple[tuple[Layer, pyvips.Image], ...], Option[pyvips.Image]]":
    # shared ORA/TIFF stack: decode each layer once, scale alpha by `opacity`, embed at its bbox, and composite the
    # visible layers in ONE native call under their `_vips_blend` modes — the bottom layer the OVER base, the rest
    # carrying their own mode — so the flattened preview is FAITHFUL to the stack the editor re-composites from.
    loaded = tuple((layer, pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers)
    visible = tuple((layer, image) for layer, image in loaded if layer.visible)

    def canvas(row: tuple[Layer, "pyvips.Image"], /) -> "pyvips.Image":
        layer, image = row
        rgba = image if image.hasalpha() else image.addalpha()
        return (rgba * [1.0, 1.0, 1.0, layer.opacity] if layer.opacity < 1.0 else rgba).embed(
            int(layer.bbox[0]), int(layer.bbox[1]), width, height, extend=pyvips.Extend.BACKGROUND
        )

    placed = tuple(map(canvas, visible))
    modes = tuple(_vips_blend(layer.blend) for layer, _ in visible)
    flattened = Some(placed[0].composite(placed[1:], modes[1:])) if len(placed) > 1 else Some(placed[0]) if placed else Nothing
    return loaded, flattened


def _rgba_array(image: "pyvips.Image", /) -> "np.ndarray":
    rgba = image if image.hasalpha() else image.addalpha()
    rgba = rgba.cast("uchar")
    return np.ndarray(buffer=rgba.write_to_memory(), dtype=np.uint8, shape=(rgba.height, rgba.width, rgba.bands))[:, :, :4].copy()


def _ora(export: LayeredExport) -> LayerFact:
    # flattened stack feeds `mergedimage.png`; `lxml` authors `stack.xml`, `stream_zip` frames the ZIP with the
    # `mimetype` member stored first and uncompressed per the OpenRaster spec.
    width, height = (ceil(extent) for extent in _viewport(export.layers))
    loaded, flattened = _flattened(export, width, height)
    pngs = frozendict({layer.name: image.write_to_buffer(".png") for layer, image in loaded})
    merged = flattened.map(lambda image: image.write_to_buffer(".png")).default_with(
        lambda: pyvips.Image.black(width, height, bands=4).write_to_buffer(".png")
    )
    thumb = pyvips.Image.thumbnail_buffer(merged, _THUMB, height=_THUMB).write_to_buffer(".png")
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
    return LayerFact(preview=(b"".join(stream_zip(members)), width, height, len(export.layers)))


def _psd_flags(layer: Layer, /) -> "psdtags.PsdLayerFlag":
    # `PsdLayerFlag.VISIBLE` is the RAW Adobe flag bit (value 2) whose SET state means the layer is HIDDEN
    # (ISO/Adobe layer-flags bit 1: 0 = visible, 1 = hidden); psdtags is a raw codec with no inverting accessor,
    # so the bit is set ONLY for a hidden layer, else a visible layer opens hidden in every conforming reader.
    hidden = psdtags.PsdLayerFlag.VISIBLE if not layer.visible else psdtags.PsdLayerFlag.BASE
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


def _psd_divider(name: str, kind: "psdtags.PsdSectionDividerType", /) -> "psdtags.PsdLayer":
    # a zero-channel marker row: the layered-TIFF group grammar is FLAT — a BOUNDING_SECTION_DIVIDER row below the
    # members and an OPEN_FOLDER row carrying the group name above them (the list is bottom-up paint order).
    return psdtags.PsdLayer(
        name=name,
        channels=[],
        rectangle=psdtags.PsdRectangle(0, 0, 0, 0),
        info=[psdtags.PsdSectionDividerSetting(kind)],
    )


def _tiff_rows(loaded: "tuple[tuple[Layer, pyvips.Image], ...]", codec: "psdtags.PsdCompressionType", /) -> "list[psdtags.PsdLayer]":
    # ORA folder position projected onto the flat grammar: an ungrouped layer lands inline, a group lands
    # whole where its members paint — divider below, members in paint order, named folder row above. The flat
    # PSD grammar cannot interleave a foreign layer inside a folder run, so a non-contiguous group refuses
    # before export rather than silently reordering the paint sequence at its first occurrence.
    runs = tuple(key for key, _ in groupby(layer.group for layer, _ in loaded) if key)
    if broken := tuple(sorted({key for key in runs if runs.count(key) > 1})):
        raise ValueError(f"layered TIFF groups interleaved with foreign layers: {broken}")
    seen: set[str] = set()
    rows: list[psdtags.PsdLayer] = []
    for layer, image in loaded:
        if not layer.group:
            rows.append(_psd_layer(layer, image, codec))
        elif layer.group not in seen:
            seen.add(layer.group)
            members = [_psd_layer(member, art, codec) for member, art in loaded if member.group == layer.group]
            rows += [
                _psd_divider("</Layer group>", psdtags.PsdSectionDividerType.BOUNDING_SECTION_DIVIDER),
                *members,
                _psd_divider(layer.group, psdtags.PsdSectionDividerType.OPEN_FOLDER),
            ]
    return rows


def _tiff(export: LayeredExport) -> LayerFact:
    width, height = (ceil(extent) for extent in _viewport(export.layers))
    loaded, flattened = _flattened(export, width, height)
    merged = flattened.map(_rgba_array).default_with(lambda: np.zeros((height, width, 4), dtype=np.uint8))
    method = _channel(export.policy.channel)  # capability-detected ONCE, driving BOTH the layer channels and the merged strip
    channel_codec = psdtags.PsdCompressionType(int(method))  # PSD method code -> psdtags per-channel codec (RAW fallback already applied)
    source_data = psdtags.TiffImageSourceData(
        psdtags.PsdFormat.BE32BIT,
        psdtags.PsdLayers(psdtags.PsdKey.LAYER, _tiff_rows(loaded, channel_codec), has_transparency=True),
        psdtags.PsdUserMask(),
        name="layered.tif",
    )
    resources = psdtags.TiffImageResources(psdtags.PsdFormat.BE32BIT, [], name="layered.tif")
    sink = BytesIO()
    codec = _CHANNEL_CODEC[method][-1]  # the merged-strip codec (tifffile routes to the same imagecodecs core)
    tifffile.imwrite(
        sink,
        merged,
        photometric="rgb",
        extrasamples=("unassalpha",),
        metadata=None,
        compression=None if codec == "none" else codec,
        predictor=tifffile.PREDICTOR.HORIZONTAL if method is PsdCompression.ZIP_PREDICTION else None,
        byteorder=source_data.byteorder,
        extratags=(source_data.tifftag(maxworkers=4), resources.tifftag()),
    )
    return LayerFact(preview=(sink.getvalue(), width, height, len(export.layers)))


def _plane_array(image: "pyvips.Image", /) -> "np.ndarray":
    # dtype-PRESERVING decode: uchar/ushort/float planes survive to the family selection — the PhotoshopAPI
    # specialization derives off the DECODED dtype (catalog law), never an unconditional `uchar` quantization.
    rgba = image if image.hasalpha() else image.addalpha()
    rgba = rgba if rgba.format in ("uchar", "ushort", "float") else rgba.cast("uchar")
    dtype = {"uchar": np.uint8, "ushort": np.uint16, "float": np.float32}[rgba.format]
    return np.ndarray(buffer=rgba.write_to_memory(), dtype=dtype, shape=(rgba.height, rgba.width, rgba.bands))[:, :, :4].copy()


def _promoted(plane: "np.ndarray", grade: str, /) -> "np.ndarray":
    # a mixed-depth stack promotes UP to the richest decoded grade — precision widens, never quantizes.
    if str(plane.dtype) == grade:
        return plane
    if grade == "float32":
        return (plane.astype(np.float32) / float(np.iinfo(plane.dtype).max)) if plane.dtype != np.float32 else plane
    return plane.astype(np.uint16) * 257 if grade == "uint16" else plane


def _psd(export: LayeredExport) -> LayerFact:
    # PhotoshopAPI authors once to its path boundary; psd-tools reopens the bytes as structural evidence. The
    # document and layer families specialize off the richest decoded plane dtype (uint8/uint16/float32).
    width, height = (ceil(extent) for extent in _viewport(export.layers))
    planes = tuple(_plane_array(pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers)
    grade = max((str(plane.dtype) for plane in planes), key=("uint8", "uint16", "float32").index, default="uint8")
    family = {"uint8": "8bit", "uint16": "16bit", "float32": "32bit"}[grade]
    document = getattr(psapi, f"LayeredFile_{family}")(psapi.enum.ColorMode.rgb, width, height)
    folders: dict[str, object] = {}
    for layer, decoded in zip(export.layers, planes, strict=True):
        rgba = _promoted(decoded, grade)
        node = getattr(psapi, f"ImageLayer_{family}")(
            {
                psapi.enum.ChannelID.red: rgba[:, :, 0],
                psapi.enum.ChannelID.green: rgba[:, :, 1],
                psapi.enum.ChannelID.blue: rgba[:, :, 2],
                psapi.enum.ChannelID.alpha: rgba[:, :, 3],
            },
            layer_name=layer.name,
            width=rgba.shape[1],
            height=rgba.shape[0],
            blend_mode=getattr(psapi.enum.BlendMode, layer.blend.value.replace("-", "")),
            pos_x=int(layer.bbox[0]),
            pos_y=int(layer.bbox[1]),
            opacity=layer.opacity,
            compression=getattr(psapi.enum.Compression, export.policy.channel.name.lower().replace("_", "")),
            color_mode=psapi.enum.ColorMode.rgb,
            is_visible=layer.visible,
            is_locked=layer.locked,
        )
        if not layer.group:
            document.add_layer(node)
            continue
        if layer.group not in folders:
            folders[layer.group] = getattr(psapi, f"GroupLayer_{family}")(layer.group, width=width, height=height)
            document.add_layer(folders[layer.group])
        folders[layer.group].add_layer(document, node)
    with TemporaryDirectory(prefix="rasm-layered-") as scratch:
        sink = Path(scratch) / f"layered.{export.target.value}"
        document.write(str(sink), force_overwrite=True)
        data = sink.read_bytes()
    decoded = PSDImage.open(BytesIO(data), max_alloc_bytes=max(len(data) * 8, width * height * 4 * (len(export.layers) + 1)))
    if not all(decoded.find(layer.name) is not None for layer in export.layers):
        raise ValueError("Photoshop layer tree lost authored leaves")
    return LayerFact(preview=(data, width, height, len(export.layers)))


# --- [COMPOSITION] ----------------------------------------------------------------------
ENGINES: Final[frozendict[ExportTarget, LayerEngine]] = frozendict({
    ExportTarget.SVG: LayerEngine(_svg),
    ExportTarget.PDF: LayerEngine(_pdf),
    ExportTarget.ORA: LayerEngine(_ora, trait=KernelTrait.HOSTILE),
    ExportTarget.PSD: LayerEngine(_psd, trait=KernelTrait.HOSTILE),
    ExportTarget.PSB: LayerEngine(_psd, trait=KernelTrait.HOSTILE),  # the same arm; the target suffix selects the PSB container
    ExportTarget.TIFF: LayerEngine(_tiff, trait=KernelTrait.HOSTILE),
})


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "ExportFault",
    "ExportPayload",
    "ExportTarget",
    "Layer",
    "LayerEngine",
    "LayerFact",
    "OcgIntent",
    "LayerPolicy",
    "LayeredExport",
    "PsdCompression",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [PSAPI_CP315]-[BLOCKED]: `PhotoshopAPI` publishes `cp38`–`cp313` wheels only and the manifest gates `PhotoshopAPI; python_version<'3.15'`, so the PSD/PSB arms have no admitted runtime provider on this interpreter; re-verify the wheel tags each release via `uv run python -m tools.assay api resolve photoshopapi` and drop the gate the moment a `cp315` wheel or source-build provision lands.
