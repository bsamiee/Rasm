# [PY_ARTIFACTS_LAYERED]

The editable named-layer export close: `LayeredExport` authors the separable, toggleable, lockable layer structure an external editor re-orders and re-colors, the inverse shape of the `document/egress#FINISH` `FINISHERS` table that STRIPS the layers this owner authors. ONE owner discriminates the editor family over the closed `ExportTarget` `StrEnum` — `SVG` (`drawsvg` named-layer `<g inkscape:groupmode=layer>` groups read as layers by both the Illustrator and the Inkscape panel), `PDF` (`pymupdf` optional-content-group placement enriched by the `pikepdf` `/OCProperties` object model for Acrobat), `ORA` (an OpenRaster `pyvips`/`lxml`/`stream-zip` layered container for GIMP/Krita/raster editors), `PSD`/`PSB` (a native Photoshop channel-stack document through `psd_tools` `PSDImage.create_pixel_layer`/`create_group`/`save` — the cp315-present standing author, PhotoshopAPI the categorical-best native writer selected once a wheel lands — with the per-channel codec capability-detected off the `imagecodecs` `<CODEC>.available` backend), and `TIFF` (Photoshop-compatible layered TIFF through `psdtags.TiffImageSourceData.tifftag(...)` plus `tifffile.imwrite(..., extratags=...)`, RETAINED for the TIFF container only) — each target a `LayerEngine` row in the `ENGINES` policy table binding its single `LayerFact`-returning arm, its `preview`-versus-egress receipt discriminant, and its execution `Band`, the table the totality proof and the only dispatch.

Admission is trusted layer material plus one optional untrusted blob. The `tuple[Layer, ...]` rows arrive from the visual producers (`composition/compose#COMPOSE`, `graphic/marks/encode#MARK`, `visualization/diagram/draw#DRAW`, `graphic/raster/io#RASTER`), each carrying its placed `bbox`, beside the one trusted `LayerPolicy` save bundle; the untrusted external `base` PDF the `PDF` arm optionally grafts onto is admitted exactly once at `LayeredExport.of` through the closed `ExportPayload` `TypedDict` and its module-level `TypeAdapter`. `of` rejects an empty layer set into `ExportFault.empty` and a malformed payload into `ExportFault.payload` before the fold runs, so the interior is total over admitted owners and never re-validates a stringly-keyed bag. Every richer `Layer` attribute defaults after `bbox`, so the `Layer(name, source, bbox)` positional contract the five producers construct stays a three-argument call while `visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color` carry the full editor-panel axis.

Each arm returns a `LayerFact` carrying the authored bytes beside its evidence — the viewport for the `SVG`/`ORA` named documents, the page and authored-layer count for the `PDF` arm — and `_emit` mints the content key over `LayerFact.data` and returns the `preview`-discriminated `ArtifactReceipt` directly, without a second parse. Placement is a `Band` policy value the `_PLACE` table resolves: the `SVG` and `PDF` arms cross one the runtime thread lane in-process worker seam threading the `_THREAD_GATE` `CapacityLimiter` so the GIL-releasing `pymupdf`/`pikepdf` native render and the `drawsvg` author never run on the event loop, and the `ORA` arm crosses one the runtime process lane worker seam threading the module-level `_GATE` `CapacityLimiter` because libvips and the libxml2-backed `lxml` are off the runtime loader path. Receipt emission is the runtime lane's `@drained` harvest of the `ArtifactReceipt` each `_emit` returns (mirroring `delivery/register#REGISTER` `Register.emit`, not a producer-level `@receipted` weave), and the runtime `async_boundary` default `Exception` capture converts a worker death or any engine provider raise into the runtime `BoundaryFault` rail through its `CLASSIFY` table; the owner reads as a band-dispatched fold returning one receipt. Placement, scaling, and rasterization stay upstream: this owner re-renders nothing and re-lays-out nothing — it receives already-placed sources (vector `SVG` fragments and one-page `PDF`s from the visual producers, encoded raster from `graphic/raster/io#RASTER` for the OpenRaster arm) and authors the layer structure the flat egress deliberately omits. The one per-instance production entry `LayeredExport.emit(self) -> RuntimeRail[ArtifactReceipt]` — mirroring `delivery/register#REGISTER` `Register.emit` / `visualization/chart/export#EXPORT` `ChartExport.render` — IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules (no parallel module-level batch entry: the batch is the pipeline's `ArtifactWork | Iterable[ArtifactWork]` normalization), returning the existing `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case (the `SVG`/`ORA`/`PSD`/`PSB`/`TIFF` named-layer documents) or `ArtifactReceipt.Egress` case (the OCG-layered `PDF`) carrying the authored-layer count on the `overlays` slot — the same slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED-layer count on, so the layer-count fact rides one settled receipt field across the author/subtract inverse pair, never a new receipt case.

## [01]-[INDEX]

- [01]-[LAYERED]: the one editable named-layer export owner — `LayeredExport` discriminating the editor family over the closed `ExportTarget` `StrEnum` (`SVG`/`PDF`/`ORA`/`PSD`/`PSB`/`TIFF`) keyed to the `ENGINES` policy table binding each arm's `LayerFact`-returning body, `preview`-versus-egress receipt discriminant, and execution `Band`; the `tuple[Layer, ...]` trusted rows from the visual producers (`composition/compose#COMPOSE`, `graphic/marks/encode#MARK`, `visualization/diagram/draw#DRAW`, `graphic/raster/io#RASTER`) plus one optional untrusted `base` PDF admitted through the closed `ExportPayload` `TypedDict` + `TypeAdapter` at `of`; the full editor-panel `Layer` axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) projected per-format; the 16-mode CSS `BlendMode` whose value IS the SVG `mix-blend-mode` token and whose member NAME derives the `psdtags.PsdBlendMode`/`psd_constants.BlendMode` members with no parallel table; the `PsdCompression` per-channel method code capability-detected off the `imagecodecs` `<CODEC>.available` backend (the `media/filtergraph#FILTER` native-vs-substitute shape); the per-instance `emit -> RuntimeRail[ArtifactReceipt]` production entry over the `_PLACE` band-dispatched fold returning `ArtifactReceipt.Preview` (the `SVG`/`ORA`/`PSD`/`PSB`/`TIFF` named documents) or `ArtifactReceipt.Egress` (the OCG-layered `PDF`, the authored-layer count on the `overlays` slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED count on). `LayeredExport.emit` IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules — the `export/indesign#INDESIGN` `Idml.emit` counterpart, both recorded in ARCHITECTURE `[02]-[SEAMS]` as the `ArtifactPipeline` single-production-entry seam, never a parallel module-level batch entry. PSD/PSB authority moves off the layered-TIFF approximation to the native channel-stack author; `psdtags`/`tifffile` are RETAINED for the TIFF container only.

## [02]-[LAYERED]

- Auto: `_emit` is the thin pure core `emit` wraps in the `async_boundary` — it resolves `engine = ENGINES[self.target]`, awaits `_PLACE[engine.band](engine.arm, self)` so the band table runs the arm on the in-process `to_thread` lane (`_threaded`, `_THREAD_GATE`-bounded) or across the subprocess worker (`_offloaded` over the runtime process lane, `_GATE`-bounded), never inline on the event loop, mints the content key over the returned `LayerFact.data` through `ContentIdentity.of`, and returns the `preview`-discriminated `ArtifactReceipt` directly — no owner mutation, no re-minted seed, the returned receipt the `ReceiptContributor` the runtime lane harvests. The `SVG` arm builds one `drawsvg.Drawing` over the `_viewport` union (declaring `xmlns:inkscape`), folds each `Layer` into one leaf `Group(**layer.svg_attrs())` whose `Raw(source.decode())` child carries the placed markup, nests each leaf under `folders.get(layer.group, drawing)` — one `<g inkscape:groupmode=layer id=group>` folder minted per distinct `group` label and appended to the drawing once — and serializes through `as_svg`. The `PDF` arm opens the `base`-or-fresh `pymupdf` document in a `with` that deterministically closes the native handle (never GC-reaped), mints `xref = add_ocg(name, on=visible, intent=_INTENT[intent], usage=policy.usage)` per layer, places each source — opened in a nested `with` that closes it once `show_pdf_page(Rect(bbox), src, 0, oc=xref)` copies it — drives the visibility/lock partitions through `set_layer`, serializes through `tobytes` while the handle is live, and `_enriched` opens the `pikepdf` document in its own `with` and folds the per-layer `_usage` `/Usage` sub-dict and the `_order` nested folder tree onto the `/OCProperties` catalog before the bracket closes it. The `ORA` arm decodes each source, applies `opacity` by scaling alpha on the `addalpha`-normalized image, stacks the visible layers through the native `composite(placed[1:], modes[1:])` under their `_vips_blend` modes for a faithful `mergedimage.png`, authors `stack.xml` through `lxml`, and frames the OpenRaster ZIP through `stream_zip`. Every arm keys its result through the `LayerFact.data` bytes, never a re-minted identity seed.
- Receipt: each authoring returns one `core/receipt#RECEIPT` case directly off `_emit` — the returned `ArtifactReceipt` IS the `ReceiptContributor` the runtime lane's `@drained` harvest emits, never a `@receipted contribute` re-read off a threaded owner. `_emit` mints the content key over the arm's `LayerFact.data` (never a re-run of an arm) and folds the case off the `LayerEngine.preview` discriminant in one expression — the `SVG`/`ORA`/`PSD`/`PSB`/`TIFF` named-document arms return `ArtifactReceipt.Preview(key, fact.width, fact.height)` carrying the named-layer document's viewport (the same `Preview` shape `composition/compose#COMPOSE`/`graphic/marks/encode#MARK`/`graphic/raster/io#RASTER` return, the perceptual `scores` band defaulting empty), and the `PDF` arm returns `ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)` carrying the authored-layer count on the `overlays` slot (the natural counterpart to the `document/egress#FINISH` REWRITE arm reporting a STRIPPED-layer count on the same slot — author and subtract are inverses over one receipt field), the `encryption_r`/`outline_depth` slots zero because layering is neither a security nor a navigation close. Layered export adds NO new receipt case: the named-document facts are the `Preview` width/height shape and the OCG-PDF facts are the `Egress` byte/page/layer shape, both settled, the producer importing `ArtifactReceipt` and projecting flat scalars so the receipt owner imports no producer value object.
- Growth: a new editable-export target (a Scribus `.sla` profile, an Affinity `.afphoto` container) is one `ExportTarget` member plus one `LayerEngine` row plus one arm over the existing engine algebra — never a re-implemented SVG serializer, PDF object model, PSD channel writer, TIFF extratag writer, or ZIP container; a new layer attribute (an alpha-mask key, a clip-to-below flag, a per-layer color profile) is one field on the `Layer` row threaded into the SVG `Group` style, the OCG enrichment, the `stack.xml` author, the PSD `create_pixel_layer`/`create_mask` call, and the TIFF PSD layer record where the format supports it, never a parallel attribute list; a new compositing mode is one `BlendMode` member, its value the SVG spelling, the `_ora_op` derivation the OpenRaster `svg:` cell, `_vips_blend` the libvips merge nickname, and its shared member NAME the `psdtags.PsdBlendMode`/`psd_constants.BlendMode` correspondence `<Enum>[blend.name]` derives with no table (the 12 native-only Photoshop modes `vividlight`/`linearlight`/`pinlight`/`hardmix`/`subtract`/`divide` are a separate growth axis, absent from the CSS-value contract the SVG arm's `mix-blend-mode` token demands); a new per-channel PSD/TIFF codec is one `PsdCompression` member plus one `_CHANNEL_CODEC` row, the `_channel` capability probe and both container derivations following; the four `/PageElement` roles and the print/export view-applications already span the OCG `category -> {stateKey: cell}` shape, so a further usage application (a CMYK-only print view) is one `LayerIntent` member plus one `_USAGE`/`_STATE_KEY` row — `_INTENT` is DERIVED over `LayerIntent` (only `DESIGN` is non-`View`), so the new member auto-derives its hint with no cell and `_usage`/`_enriched` untouched — only a Real-valued `/Zoom` min/max band, the one entry the uniform `Name`-emitting `_usage` cannot carry, would add a single `_usage` arm beside the `Layer` field holding the range; a new execution band (a free-threaded lane, a subinterpreter isolate) is one `Band` member plus one `_PLACE` row, every arm untouched; a new save knob is one `LayerPolicy` field; a new admission invariant (a name-uniqueness, a bbox-bounds check) is one `ExportFault` case plus one `of` guard; a new untrusted external blob is one `ExportPayload` band line plus one `of` admission read. Zero new surface.
- Boundary: a per-producer layer-export class family, a parallel `_ocg`/`_surgery` pair beside the one `PDF` arm, a `names`/`sources`/`flags` triple-list zipped at the call site beside the one `Layer` row, a hand-emitted `<g id="...">` string beside the `drawsvg` `Group`, a hand-written `/OC … BDC … EMC` content-stream string beside the `pymupdf` `oc=` placement that brackets the span natively, a multi-column `_BLEND` per-format blend table whose SVG column duplicates `blend.value` and whose PDF `/BM` column has no consumer (the OCG model gates visibility, not compositing), a single-column `_PSD_BLEND` `frozendict[BlendMode, psdtags.PsdBlendMode]` where the shared member NAME `<Enum>[blend.name]` derives both the `psdtags.PsdBlendMode` (TIFF arm) and `psd_constants.BlendMode` (PSD/PSB arm) consumers with no table — distinct from the live `_vips_blend` value-derivation that feeds the faithful `mergedimage.png`, a `reduce(composite2, BlendMode.OVER)` flatten discarding every per-layer blend and `opacity` into an unfaithful `mergedimage.png`, an `if engine.gated` offload branch beside the `_PLACE` band table, a bare unbounded `to_process` offload trusting the per-loop process-limiter default beside the `_GATE`-bounded crossing, a synchronous arm run inline on the event loop where the `_THREAD_GATE`-bounded `to_thread` lane offloads the GIL-releasing render, a native `pymupdf` `doc`/`pikepdf` `pdf` handle left for GC where the arm brackets it in a `with`, a module-level `exported(... | Iterable)` batch entry returning `RuntimeRail[Block[ContentKey]]` beside the one per-instance `emit -> RuntimeRail[ArtifactReceipt]` the `ArtifactPipeline` schedules (a `Block[ContentKey]` fails `Work[ArtifactReceipt]` and duplicates the pipeline's batch), a per-instance `author` method or a producer-level `@receipted` weave beside that one `emit` production coroutine, a `Layer.svg_style` returning a bare CSS string where `svg_attrs` carries the whole `id`/`inkscape:groupmode`/`inkscape:label`/`style`/`data-color` group dict, a plain `<g id=>` group Inkscape reads as a GROUP not a layer where the `inkscape:groupmode="layer"` idiom makes it a real layer, a `groups[layer.name]`-keyed parent lookup where `group` references a layer NAME instead of the `folders[group]` label folder, a silent duplicate-layer-name collapse where the `of` `Counter` gate rejects it into `ExportFault.duplicate`, an `_INTENT` row hand-enumerated per intent where the comprehension derives it, a manual `_emit`-side `Signals.emit` where the runtime lane `@drained` harvests the returned `ArtifactReceipt`, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live editor, no re-render, no re-layout. `drawsvg` owns programmatic named-`Group` SVG authoring; `pymupdf` owns the native OCG placement and `pikepdf` the `/OCProperties` catalog enrichment, meeting at PDF bytes; `pyvips`/`lxml`/`stream-zip` own the OpenRaster composite/manifest/container; `psd_tools` owns the native PSD/PSB channel-stack author (`PSDImage.create_pixel_layer`/`create_group`/`save`, the cp315-present standing writer) and PhotoshopAPI the categorical-best native writer selected in its place once a wheel lands (one writer per interpreter, never two parallel); `imagecodecs` owns the per-channel byte codec beneath the PSD/TIFF channel path (capability-detected via `<CODEC>.available`, the merged-strip codec routed to tifffile's own imagecodecs core); `psdtags`/`tifffile` are RETAINED for the layered-TIFF container ONLY — PSD/PSB authority moves off the layered-TIFF approximation to the native channel-stack author. A parallel `psd-tools`-plus-PhotoshopAPI PSD writer admitted on ONE interpreter (the native writer is the gated superseding arm, not a second parallel owner), a `psapi`/`psd_tools` array handed straight to `tifffile` (the TIFF arm owns its own `psdtags.PsdLayer` lowering), the 12 native-only Photoshop blend modes forced onto the CSS-value `BlendMode` (breaking the SVG `mix-blend-mode` token), and a channel routed through a codec whose `<CODEC>.available` was not probed are the deleted PSD-plane forms. Rasterization stays `graphic/raster/io#RASTER`/`graphic/vector/region#REGION`, placement stays the compose `Tile`/`ScaleFit`/`Overlay` arms, and the encoded raster layer sources for `ORA`/`TIFF` arrive pre-rendered keyed by the same `ContentKey` — layered export binds each as a named layer and authors no placement, scaling, or rasterization. The inverse OCG-layer STRIP/FLATTEN stays `document/egress#FINISH`'s, the PDF/A archival close stays `document/emit#EMIT`'s, the PAdES cryptographic close stays `exchange/conformance#CONFORMANCE`'s, and the InDesign template-mutation hand-off stays `export/indesign#INDESIGN`'s. There is NO layer-faithful `.ai` writer: Illustrator's `.ai` is a PDF-compatible private container whose OCG layers do NOT map to the Illustrator layer panel, so an OCG-layered PDF renamed `.ai` opens FLAT — the only durable layer-faithful Illustrator hand-off is the `SVG` named-layer `<g>` document (its `id` the Illustrator layer name, its `inkscape:groupmode="layer"` the Inkscape recognition), so the `.ai` target is deliberately absent rather than mis-promised. The content key is consumed from runtime over the authored bytes, never re-minted off the source key.

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


class BlendMode(
    StrEnum
):  # the 16 CSS `mix-blend-mode` modes; the value IS the SVG token (`svg_attrs`), `_ora_op` derives the OpenRaster `svg:` op, `_vips_blend` the libvips composite nickname
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
    # drawsvg `ValueError`, the worker's `pyvips.Error`/`lxml.etree.LxmlError`, a `BrokenWorkerProcess`) convert
    # to the runtime `BoundaryFault` at the `async_boundary` capsule's `CLASSIFY` table, never into this interior vocabulary.
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
# the pymupdf `add_ocg(intent=)` /Intent processing hint, DERIVED over the closed `LayerIntent`: only `DESIGN`
# is the design-time-only hint and every other intent is the default `View`, so a new intent auto-derives its
# hint with no row. The richer per-layer view-application AND the /PageElement structural marking are the
# `_enriched` /Usage sub-dict concern (`_USAGE`/`_STATE_KEY`), never this binary hint.
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
    group: str = ""  # folder label projected to all three editors: the SVG parent <g> the arm nests under, the OCG `/Order` folder title, and the ORA `<stack name=>` organizational folder; "" roots the layer
    color: str = ""  # editor layer-panel swatch, projected to the SVG <g> `data-color` (OCG/ORA carry no standard color slot); "" omits it

    def svg_attrs(self) -> dict[str, str]:
        # the SVG <g> attribute dict the `_svg` arm splats: `inkscape:groupmode="layer"`+`inkscape:label` make
        # Inkscape read the group AS a layer (a bare `<g id=>` is a GROUP, not a layer, in Inkscape) while `id`
        # carries the Illustrator layer name, the folded CSS `style` (`blend.value` IS the `mix-blend-mode` token,
        # so no per-format blend table), and the panel swatch rides a PRESERVED `data-color` data attribute — no
        # portable SVG layer-colour slot exists, so the swatch is machine-readable evidence, not an editor-panel
        # field a viewer renders. Each cell omitted when default so a plain layer writes the minimal group.
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
        # the renamed private thunk; the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
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
# the Photoshop blend correspondence is the shared member NAME, not a table: the page `BlendMode` StrEnum member
# names (`NORMAL`/`MULTIPLY`/`COLOR_DODGE`/...) are exactly the `psdtags.PsdBlendMode` and `psd_constants.BlendMode`
# member names, so `<Enum>[blend.name]` derives both consumers (`_psd_layer` for the TIFF arm, `_psd` for the PSD/PSB
# arm) with ZERO parallel table — the same derivation the deleted multi-column `_BLEND` table was collapsed into.
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
    folders: dict[
        str, drawsvg.Group
    ] = {}  # one `<g inkscape:groupmode=layer id=group>` sublayer-folder per distinct `group` label — the ORA `<stack name=>`/PDF `/Order` counterpart, NOT a parent-layer-name reference
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
    # the pikepdf catalog enrichment the pymupdf `add_ocg` placement cannot author: the per-layer `/Usage`
    # print/export view-application and the nested `/D/Order` folder tree, matched onto the placed OCGs by /Name.
    # the pikepdf document brackets in one `with` so the native handle closes deterministically, never GC-reaped;
    # `save`/`len(pdf.pages)` run while the handle is live.
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
    # the OpenRaster layered container on the `to_process` worker: `pyvips` decodes each placed raster,
    # scales its alpha by `opacity`, and stacks the visible layers through the native `composite` (ONE call,
    # per-layer `BlendMode` via `_vips_blend`) so the flattened `mergedimage.png` is FAITHFUL to the layer
    # stack the editor re-composites from — never `reduce(composite2, OVER)` discarding every blend and
    # opacity; `lxml` authors `stack.xml` (top-to-bottom, each `composite-op` from `blend.value`); `stream_zip`
    # frames the ZIP with the `mimetype` stored first. Layer sources are RGB/RGBA per the raster-producer contract.
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
    folders: dict[
        str, etree._Element
    ] = {}  # one organizational `<stack name=group>` per distinct `group` — the ORA folder counterpart to the SVG nested `<g>` and the PDF `/Order` tree
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
    # every editable layer channel carries the capability-detected `compression` (RAW/RLE/ZIP/ZIP_PREDICTED) so
    # `TiffImageSourceData.tifftag` compresses the real layer payload through `imagecodecs`, never the silent RAW
    # default that would leave the actual layers uncompressed while only the merged preview strip was coded.
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
    # the native Photoshop channel-stack author on the `Band.WORKER` `to_process` worker (psd-tools/PhotoshopAPI are off
    # the runtime loader path exactly as pyvips/lxml are): one `PSDImage` document holding one `create_pixel_layer` per
    # `Layer` row over the shared `_rgba_array` decode (lifted to PIL), the editor-panel axis attached through native setters,
    # one native group per distinct `group` label (the PSD counterpart to the SVG `<g>` / OCG `/Order` / ORA `<stack>` folder),
    # and a per-channel `Compression` capability-detected off the `imagecodecs` backend the same `_channel` probe gates.
    # psd-tools is the cp315-present standing author; PhotoshopAPI (native, faster, `python_version<'3.15'`-gated, no cp315
    # wheel yet) is the categorical-best writer selected in its place ONCE a wheel or source-build lands — one writer per
    # interpreter, never two parallel. The bit depth is fixed at 8 off the `uint8` `_rgba_array`, never a caller knob.
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
# the `ORA` offload threads the module-level `_GATE` `CapacityLimiter` so N concurrent `emit` calls
# share a fixed worker subprocess pool instead of fanning out at the per-loop process-limiter default — the
# bounded crossing `export/indesign#INDESIGN`'s IDML worker rides — and the runtime `async_boundary` default
# `Exception` capture plus its `CLASSIFY` table routes a worker death to `resource` and every other engine raise
# (neither a `ValueError`, and `import pyvips` itself raises with native libvips unprovisioned), so a hand-named
# `catch` tuple would silently miss the dominant `ORA` failure. The in-process `SVG`/`PDF` arms ride the separate
# `_THREAD_GATE` thread bound so the GIL-releasing native render is bounded off the per-loop 40-token thread default.


async def _threaded(arm: Callable[["LayeredExport"], LayerFact], export: "LayeredExport", /) -> LayerFact:
    # the in-process lane: the GIL-releasing pymupdf/pikepdf native render and the drawsvg author cross
    # `to_thread` so the synchronous body never runs on the event loop — there is no inline "on the loop" lane.
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
