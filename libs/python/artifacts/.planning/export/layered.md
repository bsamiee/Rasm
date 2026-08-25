# [PY_ARTIFACTS_LAYERED]

`LayeredExport` owns the editable named-layer export close — it authors the separable, toggleable, lockable layer structure an external editor re-orders and re-colors, the inverse of the `document/egress#FINISH` `FINISHERS` table that strips the layers this owner authors. One owner discriminates the editor family over the closed `ExportTarget` vocabulary, each target a `LayerEngine` row in `ENGINES` binding one `LayerFact` arm and its crossing `KernelTrait`. `LayerFact` separates `preview` and `egress` payloads, so output dispatch reads the fact without a boolean or default-filled cross-mode record. Placement, scaling, and rasterization stay upstream. `BlendMode` composes from `graphic/color/derive#DERIVE`; its value derives SVG, and its name derives both `psd_tools.constants.BlendMode` and `psdtags.PsdBlendMode` off one shared correspondence.

## [01]-[INDEX]

## [02]-[LAYERED]

- Owner: `LayeredExport` discriminates the editor family over the closed `ExportTarget` keyed to the `ENGINES` policy table binding each arm's `LayerFact` body and crossing `KernelTrait`. `LayerFact.preview` carries named-document bytes, viewport, and layer count; `LayerFact.egress` carries PDF bytes, page count, and authored-layer count. `Layer` is the row every visual producer constructs — `name`/`source`/`bbox` positional, the editor-panel axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) defaulting after `bbox`; `group` is a folder label projected to all editors, never a parent-layer-name reference. `Layer.renamed` is the ONE rename projection the `composition/compose#COMPOSE`, `composition/sheet#SHEET`, and `composition/imposition#IMPOSE` placement owners compose for their `layers(names)` egress — a positional override roster over its prefix, each uncovered row keeping its own name, and a blank name — supplied override and own name alike — falling to a `layer-{index}` synthetic reserved against every projected name, since a blank is the one name this owner's duplicate and identity refusals both reject. `OcgIntent` absorbs the producer vocabulary and lowers every editorial semantic onto one OCG usage row. `LayerPolicy` is the trusted save-policy bundle; `BlendMode` is `graphic/color/derive#DERIVE`'s canonical vocabulary whose value is the SVG `mix-blend-mode` token and whose name derives the Photoshop members.
- Cases: each `ExportTarget` is a `LayerEngine` row with its editability ruling — `SVG` uses named `drawsvg` layer groups, `PDF` uses `pymupdf` OCG placement plus `pikepdf` `/OCProperties`, `ORA` uses `pyvips`/`lxml`/`stream-zip`, `PSD`/`PSB` use `psd-tools` native channel stacks — the one owner authoring the document and re-proving the finished bytes — and `TIFF` uses `psdtags`/`tifffile`. `_PSB_FLOOR` refuses a target on the wrong side of the PSD dimension bound, `PSDImage.new` derives the container version off that same bound, and one record-tier save tail serves both containers, so the target needs no version knob and no second save path. Illustrator consumes the named-layer `SVG`; renamed OCG PDF does not create Illustrator layer-panel structure.
- Output: `LayerFact` is the closed bytes-plus-evidence family every arm returns; each case carries only its mode's required payload, with no null or default ghost fields.
- Growth: a new editable-export target is one `ExportTarget` member, one `LayerEngine` row, and one arm over the engine algebra. A new layer attribute is one `Layer` field threaded into each projection. A compositing mode extends `graphic/color/derive#DERIVE`, and each lowering derives the provider member by value or name; Photoshop-only modes remain outside the CSS contract. Codec, OCG usage, lane, save, admission, and untrusted-payload growth extend their existing closed owners.
- Boundary: a per-producer export class family, parallel name/source/flag lists, hand-emitted SVG groups, hand-written PDF OCG streams, local `BlendMode` twins, per-format blend tables, lossy flattening, class-qualified offload, inline native work, unbracketed document handles, duplicate names, and module batch entrypoints are rejected. `psd-tools` alone authors PSD/PSB and re-proves its own output; no second native writer rides the interpreter. The PSD/PSB arms author the display-referred 8-bit RGBA family — `PixelLayer.frompil` admits pixels only across the PIL seam, which carries no multichannel plane past 8 bits — so bit depth follows the plane's referent and every deep plane stays with the TIFF/ORA/EXR owners. `psdtags`/`tifffile` own TIFF. Rasterization stays graphic-owned, placement stays composition-owned, and PDF/A/PAdES/flat egress stay their close owners.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
from collections import Counter
from collections.abc import Callable
from enum import IntEnum, StrEnum
from itertools import count, groupby
from io import BytesIO
from math import ceil
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from builtins import frozendict
from expression import Error, Nothing, Ok, Option, Result, Some, case, tag, tagged_union
from msgspec import Struct, field, structs
from msgspec.msgpack import Encoder
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.graphic.color.derive import BlendMode
from rasm.artifacts.graphic.layer import FlatLayer, LayerFault, LayerPlan, flattened as graphic_flattened

lazy import drawsvg
lazy import numpy as np
lazy import pikepdf
lazy import psdtags
lazy import pymupdf
lazy import tifffile
lazy from pikepdf import Array, Dictionary, Name, String

lazy import pyvips
lazy import zlib
lazy from datetime import UTC, datetime
lazy from lxml import etree
lazy from stream_zip import NO_COMPRESSION_32, ZIP_AUTO, stream_zip

lazy import imagecodecs
lazy from PIL import Image
lazy from psd_tools import PSDImage
lazy from psd_tools.api.layers import Group, PixelLayer
lazy from psd_tools.constants import BlendMode as PsdBlendMode, Compression as PsdCodec, ProtectedFlags

if TYPE_CHECKING:
    import pikepdf


# --- [TYPES] ----------------------------------------------------------------------------
class ExportTarget(StrEnum):
    SVG = "svg"
    PDF = "pdf"
    ORA = "ora"
    PSD = "psd"
    PSB = "psb"
    TIFF = "tiff"


class OcgIntent(StrEnum):
    VIEW = "view"
    PRINT = "print"
    EXPORT = "export"
    DESIGN = "design"
    BACKGROUND = "background"
    HEADER_FOOTER = "header_footer"
    FOREGROUND = "foreground"
    LOGO = "logo"
    ANNOTATION = "annotation"
    FIGURE = "figure"
    LINEWORK = "linework"
    OVERLAY = "overlay"
    REFERENCE = "reference"
    SYMBOL = "symbol"


class PsdCompression(IntEnum):
    RAW = 0
    RLE = 1
    ZIP = 2
    ZIP_PREDICTION = 3


# --- [CONSTANTS] ------------------------------------------------------------------------
_ORA_MIME: Final[bytes] = b"image/openraster"
_PSB_FLOOR: Final[int] = 30_000
_PSB_CEILING: Final[int] = 300_000
_THUMB: Final[int] = 256
_INKSCAPE_NS: Final = "http://www.inkscape.org/namespaces/inkscape"
_VIPS_UNMAPPED: Final[frozenset[BlendMode]] = frozenset({
    BlendMode.NORMAL,
    BlendMode.HUE,
    BlendMode.SATURATION,
    BlendMode.COLOR,
    BlendMode.LUMINOSITY,
})
_INTENT: Final[frozendict[OcgIntent, str]] = frozendict({intent: "Design" if intent is OcgIntent.DESIGN else "View" for intent in OcgIntent})
_OCG_OF_SEMANTIC: Final[frozendict[str, OcgIntent]] = frozendict({
    "background": OcgIntent.BACKGROUND,
    "guide": OcgIntent.DESIGN,
    "mask": OcgIntent.DESIGN,
    "grid": OcgIntent.DESIGN,
    "datum": OcgIntent.DESIGN,
    "issue": OcgIntent.PRINT,
    "overlay": OcgIntent.FOREGROUND,
})
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
_CHANNEL_CODEC: Final[frozendict[PsdCompression, tuple[str, ...]]] = frozendict({
    PsdCompression.RAW: ("none",),
    PsdCompression.RLE: ("packbits",),
    PsdCompression.ZIP: ("zlib",),
    PsdCompression.ZIP_PREDICTION: ("delta", "deflate"),
})


# --- [MODELS] ---------------------------------------------------------------------------
class Layer(Struct, frozen=True):
    name: str
    source: bytes
    bbox: tuple[float, float, float, float]
    visible: bool = True
    locked: bool = False
    opacity: float = 1.0
    blend: BlendMode = BlendMode.NORMAL
    intent: OcgIntent = OcgIntent.VIEW
    group: str = ""
    color: str = ""

    @classmethod
    def of_plan(cls, plan: "LayerPlan", bbox: tuple[float, float, float, float], /) -> "Result[tuple[Layer, ...], LayerFault]":
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

    @staticmethod
    def renamed(layers: tuple["Layer", ...], names: tuple[str, ...], /) -> tuple["Layer", ...]:
        chosen = tuple((names[index] if index < len(names) else layer.name).strip() for index, layer in enumerate(layers))
        taken = {name for name in chosen if name}

        def minted(index: int, /) -> str:
            fresh = next(name for step in count(index) if (name := f"layer-{step}") not in taken)
            taken.add(fresh)
            return fresh

        return tuple(structs.replace(layer, name=name or minted(index)) for index, (layer, name) in enumerate(zip(layers, chosen, strict=True)))

    @property
    def origin(self) -> tuple[int, int]:
        return round(self.bbox[0]), round(self.bbox[1])

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
    usage: str = "Artwork"
    garbage: int = 3
    deflate: bool = True
    channel: PsdCompression = PsdCompression.ZIP_PREDICTION


@tagged_union(frozen=True)
class LayerFact:
    tag: Literal["preview", "egress"] = tag()
    preview: tuple[bytes, int, int, int] = case()
    egress: tuple[bytes, int, int] = case()


class LayerEngine(Struct, frozen=True):
    arm: Callable[["LayeredExport"], LayerFact]
    trait: KernelTrait = KernelTrait.RELEASING


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ExportFault:
    tag: Literal["payload", "empty", "layer", "duplicate", "container"] = tag()
    payload: tuple[str, ...] = case()
    empty: None = case()
    layer: tuple[int, str] = case()
    duplicate: tuple[str, ...] = case()
    container: tuple[int, int] = case()


# --- [BOUNDARIES] -----------------------------------------------------------------------
class ExportPayload(TypedDict, closed=True):
    base: NotRequired[ReadOnly[bytes]]


_PAYLOAD: Final = TypeAdapter(ExportPayload)


# --- [SERVICES] -------------------------------------------------------------------------
class LayeredExport(Struct, frozen=True):
    target: ExportTarget
    layers: tuple[Layer, ...]
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
            return Error(ExportFault(duplicate=collisions))
        width, height = (ceil(extent) for extent in _viewport(layers))
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

    def emit(self, /) -> ArtifactWork[LayerFact]:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=float(len(self.layers) or 1))

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(f"export-{self.target}", _CANON.encode((self.target, self.layers, self.base, self.policy)))

    async def _emit(self) -> RuntimeRail[LayerFact]:
        engine = ENGINES[self.target]
        crossed = await self.lane.offload(Kernel.of(engine.arm, engine.trait), self)
        match crossed:
            case Result(tag="ok", ok=LayerFact(tag="preview", preview=(data, _width, _height, _layers)) as fact):
                Metrics.record({BYTE_VOLUME: float(len(data))}, domain=DOMAIN, kind="preview", scope=self.lane.scope)
                return Ok(fact)
            case Result(tag="ok", ok=LayerFact(tag="egress", egress=(data, _pages, _layers)) as fact):
                Metrics.record({BYTE_VOLUME: float(len(data))}, domain=DOMAIN, kind="egress", scope=self.lane.scope)
                return Ok(fact)
            case refused:
                return Error(refused.error)


# --- [OPERATIONS] -----------------------------------------------------------------------
_CANON: Final[Encoder] = Encoder(order="deterministic")


def _viewport(layers: tuple[Layer, ...], /) -> tuple[float, float]:
    return (max((layer.bbox[2] for layer in layers), default=0.0), max((layer.bbox[3] for layer in layers), default=0.0))


def _channel(method: PsdCompression, /) -> PsdCompression:
    return method if all(getattr(imagecodecs, name.upper()).available for name in _CHANNEL_CODEC[method]) else PsdCompression.RAW


def _ora_op(blend: BlendMode, /) -> str:
    return "svg:src-over" if blend is BlendMode.NORMAL else f"svg:{blend.value}"


def _vips_blend(blend: BlendMode, /) -> str:
    return "over" if blend in _VIPS_UNMAPPED else blend.value.replace("color", "colour")


def _svg(export: LayeredExport) -> LayerFact:
    width, height = _viewport(export.layers)
    drawing = drawsvg.Drawing(width, height, origin=(0.0, 0.0), **{"xmlns:inkscape": _INKSCAPE_NS})
    folders: dict[str, drawsvg.Group] = {}
    for layer in export.layers:
        leaf = drawsvg.Group(**layer.svg_attrs())
        leaf.append(drawsvg.Raw(layer.source.decode()))
        if layer.group and layer.group not in folders:
            folders[layer.group] = drawsvg.Group(**{"id": layer.group, "inkscape:groupmode": "layer", "inkscape:label": layer.group})
            drawing.append(folders[layer.group])
        folders.get(layer.group, drawing).append(leaf)
    return LayerFact(preview=(drawing.as_svg().encode(), int(width), int(height), len(export.layers)))


def _pdf(export: LayeredExport) -> LayerFact:
    width, height = _viewport(export.layers)
    with pymupdf.open(stream=export.base, filetype="pdf") if export.base else pymupdf.open() as doc:
        page = doc[0] if export.base else doc.new_page(width=width, height=height)
        placed = []
        for layer in export.layers:
            xref = doc.add_ocg(layer.name, on=layer.visible, intent=_INTENT[layer.intent], usage=export.policy.usage)
            with pymupdf.open(stream=layer.source, filetype="pdf") as src:
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
    usage = Dictionary()
    for category, state in _USAGE[intent].items():
        entry = Dictionary()
        entry[Name("/" + _STATE_KEY[category])] = Name("/" + state)
        usage[Name("/" + category)] = entry
    return usage


def _order(layers: tuple[Layer, ...], ocgs: "dict[str, pikepdf.Object]") -> "pikepdf.Array":
    grouped: dict[str, list[pikepdf.Object]] = {}
    direct: list[pikepdf.Object] = []
    for layer in layers:
        if (ref := ocgs.get(layer.name)) is not None:
            (grouped.setdefault(layer.group, []) if layer.group else direct).append(ref)
    return Array([*direct, *(Array([String(title), *members]) for title, members in grouped.items())])


def _flattened(export: LayeredExport, width: int, height: int, /) -> "tuple[tuple[tuple[Layer, pyvips.Image], ...], Option[pyvips.Image]]":
    loaded = tuple((layer, pyvips.Image.new_from_buffer(layer.source, "")) for layer in export.layers)
    visible = tuple((layer, image) for layer, image in loaded if layer.visible)

    def canvas(row: tuple[Layer, "pyvips.Image"], /) -> "pyvips.Image":
        layer, image = row
        rgba = image if image.hasalpha() else image.addalpha()
        return (rgba * [1.0, 1.0, 1.0, layer.opacity] if layer.opacity < 1.0 else rgba).embed(
            *layer.origin, width, height, extend=pyvips.Extend.BACKGROUND
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
    width, height = (ceil(extent) for extent in _viewport(export.layers))
    loaded, flattened = _flattened(export, width, height)
    pngs = frozendict({layer.name: image.write_to_buffer(".png") for layer, image in loaded})
    merged = flattened.map(lambda image: image.write_to_buffer(".png")).default_with(
        lambda: pyvips.Image.black(width, height, bands=4).write_to_buffer(".png")
    )
    thumb = pyvips.Image.thumbnail_buffer(merged, _THUMB, height=_THUMB).write_to_buffer(".png")
    root = etree.Element("image", version="0.0.3", w=str(width), h=str(height))
    stack = etree.SubElement(root, "stack")
    folders: dict[str, etree._Element] = {}
    for layer in reversed(export.layers):
        if layer.group and layer.group not in folders:
            folders[layer.group] = etree.SubElement(stack, "stack", name=layer.group)
        etree.SubElement(
            folders.get(layer.group, stack),
            "layer",
            name=layer.name,
            src=f"data/{layer.name}.png",
            x=str(layer.origin[0]),
            y=str(layer.origin[1]),
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
    hidden = psdtags.PsdLayerFlag.VISIBLE if not layer.visible else psdtags.PsdLayerFlag.BASE
    return hidden | psdtags.PsdLayerFlag.TRANSPARENCY_PROTECTED if layer.locked else hidden


def _psd_layer(layer: Layer, image: "pyvips.Image", compression: "psdtags.PsdCompressionType", /) -> "psdtags.PsdLayer":
    rgba = _rgba_array(image)
    left, top = layer.origin
    return psdtags.PsdLayer(
        name=layer.name,
        rectangle=psdtags.PsdRectangle(top, left, top + rgba.shape[0], left + rgba.shape[1]),
        channels=[
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL0, compression, data=rgba[:, :, 0]),
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL1, compression, data=rgba[:, :, 1]),
            psdtags.PsdChannel(psdtags.PsdChannelId.CHANNEL2, compression, data=rgba[:, :, 2]),
            psdtags.PsdChannel(psdtags.PsdChannelId.TRANSPARENCY_MASK, compression, data=rgba[:, :, 3]),
        ],
        opacity=max(0, min(255, round(layer.opacity * 255))),
        blendmode=psdtags.PsdBlendMode[layer.blend.name],
        flags=_psd_flags(layer),
    )


def _psd_divider(name: str, kind: "psdtags.PsdSectionDividerType", /) -> "psdtags.PsdLayer":
    return psdtags.PsdLayer(
        name=name,
        channels=[],
        rectangle=psdtags.PsdRectangle(0, 0, 0, 0),
        info=[psdtags.PsdSectionDividerSetting(kind)],
    )


def _tiff_rows(loaded: "tuple[tuple[Layer, pyvips.Image], ...]", codec: "psdtags.PsdCompressionType", /) -> "list[psdtags.PsdLayer]":
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
    method = _channel(export.policy.channel)
    channel_codec = psdtags.PsdCompressionType(int(method))
    source_data = psdtags.TiffImageSourceData(
        psdtags.PsdFormat.BE32BIT,
        psdtags.PsdLayers(psdtags.PsdKey.LAYER, _tiff_rows(loaded, channel_codec), has_transparency=True),
        psdtags.PsdUserMask(),
        name="layered.tif",
    )
    resources = psdtags.TiffImageResources(psdtags.PsdFormat.BE32BIT, [], name="layered.tif")
    sink = BytesIO()
    codec = _CHANNEL_CODEC[method][-1]
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


def _psd(export: LayeredExport) -> LayerFact:
    width, height = (ceil(extent) for extent in _viewport(export.layers))
    loaded, flattened = _flattened(export, width, height)
    codec = PsdCodec(int(export.policy.channel))
    document = PSDImage.new(mode="RGBA", size=(width, height))
    folders: dict[str, "Group"] = {}
    for layer, image in loaded:
        if layer.group and layer.group not in folders:
            folders[layer.group] = Group.new(document, name=layer.group, open_folder=True)
        left, top = layer.origin
        node = PixelLayer.frompil(
            Image.fromarray(_rgba_array(image), "RGBA"),
            folders.get(layer.group, document),
            name=layer.name,
            top=top,
            left=left,
            compression=codec,
        )
        node.blend_mode = PsdBlendMode[layer.blend.name]
        node.opacity = max(0, min(255, round(layer.opacity * 255)))
        node.visible = layer.visible
        if layer.locked:
            node.lock(ProtectedFlags.COMPLETE)
    merged = flattened.map(_rgba_array).default_with(lambda: np.zeros((height, width, 4), dtype=np.uint8))
    document._record.image_data.set_data([merged[:, :, band].tobytes() for band in range(4)], document._record.header)
    sink = BytesIO()
    document._record.write(sink)
    data = sink.getvalue()
    decoded = PSDImage.open(BytesIO(data), max_alloc_bytes=max(len(data) * 8, width * height * 4 * (len(export.layers) + 1)))
    if any((found := decoded.find(layer.name)) is None or (found.left, found.top) != layer.origin for layer in export.layers):
        raise ValueError("Photoshop layer tree lost authored leaves or their placement")
    return LayerFact(preview=(data, width, height, len(export.layers)))


# --- [COMPOSITION] ----------------------------------------------------------------------
ENGINES: Final[frozendict[ExportTarget, LayerEngine]] = frozendict({
    ExportTarget.SVG: LayerEngine(_svg),
    ExportTarget.PDF: LayerEngine(_pdf),
    ExportTarget.ORA: LayerEngine(_ora, trait=KernelTrait.HOSTILE),
    ExportTarget.PSD: LayerEngine(_psd, trait=KernelTrait.HOSTILE),
    ExportTarget.PSB: LayerEngine(_psd, trait=KernelTrait.HOSTILE),
    ExportTarget.TIFF: LayerEngine(_tiff, trait=KernelTrait.HOSTILE),
})

_COVERED: Final[tuple[tuple[frozenset[object], frozenset[object]], ...]] = (
    (frozenset(ENGINES), frozenset(ExportTarget)),
    (frozenset(_CHANNEL_CODEC), frozenset(PsdCompression)),
    (frozenset(_USAGE), frozenset(OcgIntent) - {OcgIntent.VIEW}),
    (frozenset(_STATE_KEY), frozenset(category for row in _USAGE.values() for category in row)),
)
if any(rows != vocabulary for rows, vocabulary in _COVERED):
    raise RuntimeError("layered-export tables do not cover their vocabularies")


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

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
