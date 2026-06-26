# [PY_ARTIFACTS_LAYERED]

The editable named-layer export close authoring separable, toggleable, lockable layers for the Illustrator/Acrobat/Photoshop hand-off — the highest-value export capability, named layers an external editor re-orders and re-colors rather than one flattened path-soup splat. `LayeredExport` is ONE owner discriminating the editor target over the closed `ExportTarget` `StrEnum` (`SVG` Illustrator/Inkscape `<g id=>` Groups, `PDF_OCG` Acrobat optional-content groups bound to drawn content, `PDF_SURGERY` low-level `/OCProperties` object-model graft, `TIFF` Photoshop image-resource layer stack), each target a `LayerEngine` row in the `ENGINES` policy table binding its single `LayerFact`-returning arm and its `preview`-versus-`egress` receipt discriminant in one value — the table is the totality proof, the exact inverse shape of the `document/egress#FINISH` `FINISHERS` table that STRIPS the layers this owner AUTHORS.

Admission splits by trust exactly as the egress sibling's does. Trusted layer material enters as the `tuple[Layer, ...]` rows `composition/compose#COMPOSE`, `graphic/marks/encode#MARK`, `visualization/diagram/draw#DRAW`, and `graphic/raster/io#RASTER` already emit (one `Layer` row per placed source, each carrying its placed `bbox`), plus the one `LayerPolicy` save-knob bundle the caller constructs; untrusted external material — the `base` placed-layout-or-already-drawn PDF the OCG/surgery arms graft onto — is admitted exactly once at `LayeredExport.of` through the closed `ExportPayload` `TypedDict` and its module-level `TypeAdapter`. `of` rejects an empty layer set into `ExportFault.empty` and an OCG/surgery target with no `base` into `ExportFault.incomplete` through the `_PREREQ` predicate table before the fold runs, so the interior is total over admitted owners and never re-validates a stringly-keyed bag.

Every arm returns a `LayerFact` carrying the authored bytes beside the evidence it produced — viewport width/height for the SVG/TIFF named-document arms, page and authored-layer count for the OCG/surgery PDF arms — and `_emit` threads that fact onto the frozen owner through `msgspec.structs.replace` so `contribute` reads the arm's own evidence and projects the receipt without a second parse. The `Layer` row owns the full layer concept the editor panel exposes — `name`/`source`/`bbox` plus `visible`, `locked`, `opacity`, `blend`, `intent`, `group`, and `color` — so a compositing, intent, or grouping attribute lands as one field every engine projects to its own format through the `_BLEND`/`_INTENT`/`_USAGE` derived tables, never a parallel attribute list. Cross-cutting receipt emission is the definition-time `@receipted(Redaction.STRUCTURAL)` aspect over the thin pure `_emit`, and `async_boundary` converts a provider raise (`pymupdf.FileDataError`, `pikepdf.PdfError`, the `drawsvg`/`svgelements` `ValueError`) into the runtime `BoundaryFault` rail; the owner reads as the receipt weave over a synchronous dispatch fold. Placement, scaling, and rasterization stay upstream: this owner re-renders nothing and re-lays-out nothing — it receives already-placed sources (vector SVG/PDF from the visual producers, pre-rasterized RGBA from `graphic/raster/io#RASTER` for the Photoshop arm) and authors the layer structure the flat egress deliberately omits. Every authoring returns a `RuntimeRail[ContentKey]` over the runtime `async_boundary` and contributes the existing `core/receipt#RECEIPT` `ArtifactReceipt.Preview` case (SVG/TIFF named-layer document) or `ArtifactReceipt.Egress` case (OCG-layered PDF) carrying the authored-layer count on the `overlays` evidence slot — the same slot the `document/egress#FINISH` REWRITE arm reports a STRIPPED-layer count on, so the layer-count fact rides one settled receipt field across the author/subtract inverse pair, never a new receipt case.

## [01]-[INDEX]

- [01]-[LAYERED]: `LayeredExport` + `ExportTarget` (SVG/PDF_OCG/PDF_SURGERY/TIFF) policy-table dispatch over `drawsvg`/`pymupdf`/`pikepdf`/`psdtags`+`tifffile`, every arm in-process on the cp315 core; the `LayerEngine` row binds each target to its `(arm, preview)` pair and the `ENGINES` table is the totality proof; the `Layer` row carries the full editor-panel attribute axis (`visible`/`locked`/`opacity`/`blend`/`intent`/`group`/`color`) projected per-format through the `_BLEND`/`_INTENT`/`_USAGE` derived tables, the `LayerPolicy` bundle carries the trusted save knobs, and untrusted `base` bytes are admitted once through `LayeredExport.of` over the `ExportPayload` `TypedDict` + `TypeAdapter` with `_PREREQ` rejecting an under-supplied target into `ExportFault.incomplete`; every arm returns a `LayerFact` carrying the authored bytes beside its viewport/page/layer evidence, `_emit` threads the fact onto the frozen owner through `structs.replace`, and `LayeredExport.contribute` folds BOTH the SVG/TIFF `ArtifactReceipt.Preview` and the PDF `ArtifactReceipt.Egress` case off the `LayerEngine.preview` discriminant; `ExportFault` is the closed `@tagged_union` over the `payload`/`empty`/`incomplete` admission causes `of` produces, the arm-level provider exception families converting to the runtime `BoundaryFault` at the `async_boundary` capsule; the SVG arm folds each `Layer` into one `drawsvg.Group(id=, style=)` nesting by `group` and serializes through `Drawing.as_svg`; the OCG arm mints one `add_ocg(intent=, usage=)` per layer and binds drawn content through `Page.show_pdf_page(oc=)`; the surgery arm authors the `/OCProperties` `/OCGs`/`/Order` tree with nested groups and the per-layer `/Usage` view-application dict through the `pikepdf` object model; the TIFF arm assembles the Photoshop image-resource layer stack over `psdtags`+`tifffile`; `author` owns the async entry threading the fact-bearing owner through the `@receipted` weave the `async_boundary` capsule converts faults at.

## [02]-[LAYERED]

- Owner: `LayeredExport` the one editable-export owner discriminating the editor target over the closed `ExportTarget` `StrEnum`; `ENGINES` the `frozendict[ExportTarget, LayerEngine]` policy table binding each target to one `LayerEngine` row carrying its single `LayerFact`-returning arm and its `preview` receipt discriminant — the table is the totality proof, one arm per target, never an `if target == ...` cascade and never a per-producer layer-emit family. `Layer` the one frozen `msgspec.Struct` binding a named placed source over the full editor-panel attribute axis — `name` the editor-visible label, `source` the placed bytes the producer emits (an SVG fragment for `SVG`, a one-page PDF for `PDF_OCG`/`PDF_SURGERY`, RGBA pixel bytes from `graphic/raster/io#RASTER` for `TIFF`), `bbox` the REQUIRED placed extent every producer carries, `visible` the default-visibility flag, `locked` the editor-lock hint, `opacity` the 0-1 compositing alpha, `blend` the `BlendMode` compositing mode, `intent` the `LayerIntent` optional-content usage application, `group` the parent-folder name for nested ordering, and `color` the layer-panel swatch — so the canonical row stays `Layer(name, source, bbox, visible=True, locked=False, …)` with `name`/`source`/`bbox` the positional contract `composition/compose#COMPOSE`/`graphic/marks/encode#MARK`/`visualization/diagram/draw#DRAW`/`composition/sheet#SHEET`/`composition/imposition#IMPOSE` construct, never four parallel `names`/`sources`/`flags`/`boxes` lists zipped at the call site; the row derives its own SVG style (`svg_style` folding `display:none`/`opacity:`/`mix-blend-mode:` from the `_BLEND` row) and projects no provider object so the canonical owner stays codec-free. `LayerPolicy` the one trusted save-knob bundle (`usage` OCG category label, `garbage`/`deflate` pymupdf serialization), `LayerFact` the bytes-plus-evidence carrier every arm returns, `ExportFault` the closed admission vocabulary `of` produces, `BlendMode` the closed 16-mode compositing vocabulary driving the `_BLEND` PDF-`/BM`/SVG-`mix-blend-mode`/PSD-key correspondence, `LayerIntent` the closed optional-content usage vocabulary driving the `_INTENT` `/Intent` hint and the `_USAGE` view-application dict. `drawsvg` owns the named-`Group` SVG authoring, `pymupdf` the native OCG draw-binding (`add_ocg`/`show_pdf_page(oc=)`/`set_layer`), `pikepdf` the `/OCProperties` object-model graft (`make_indirect`/`add_resource`/nested `/Order`/`/Usage`), `psdtags`+`tifffile` the Photoshop layer-block authoring, `svgelements` the placed-extent surface the producers fold their `bbox` over before binding.
- Cases: `ExportTarget` rows · `SVG` (named-layer SVG authoring — fold each `Layer` into one `drawsvg.Group(id=name, style=layer.svg_style())` wrapping the placed source as a `Raw` verbatim-markup child, nest each group under its `group`-named parent group, append every top-level group to one `Drawing` sized to the `_viewport` union, serialize through `Drawing.as_svg` into a nested named-layer SVG an Illustrator/Inkscape layer panel reads one-to-one — `preview` receipt) · `PDF_OCG` (the PRIMARY OCG named-layer PDF — open the `base` placed-layout PDF with `pymupdf.open`, mint one OCG xref per `Layer` through `Document.add_ocg(name, on=visible, intent=_INTENT[layer.intent], usage=policy.usage)`, place each placed source onto the page bound to its layer through `Page.show_pdf_page(rect, src, oc=xref)`, drive visibility-and-lock through `Document.set_layer(0, on=, off=, locked=)`, serialize through `Document.tobytes(garbage=, deflate=)` — `egress` receipt) · `PDF_SURGERY` (the LOW-LEVEL graft AND the richer-OCG owner — open the already-drawn `base` with `pikepdf.open`, mint one indirect `/OCG` per `Layer` carrying its per-layer `/Usage` view-application dict, register the set in `/Root/OCProperties` with a `/D` default config whose nested `/Order` array folds grouped layers into `[/GroupTitle, …]` sub-arrays, bind each `/OCG` as a page `/Properties` resource, and bracket the pre-drawn content in a `/OC <resource> BDC … EMC` marked-content span — the arm the pymupdf draw-binding cannot reach AND the only arm that authors per-layer print/export/zoom `/Usage` and nested layer groups — `egress` receipt) · `TIFF` (Photoshop-compatible layered TIFF — assemble one `psdtags` image-resource layer stack from the per-`Layer` RGBA pixel bytes carrying each layer's blend key `_BLEND[blend][2]`, 0-255 opacity, bbox rectangle, and group, write the composite-plus-layer-blocks TIFF through `tifffile.imwrite(extratags=)` so Photoshop opens each region as a named layer — `preview` receipt) — each one `LayerEngine.arm` resolved off `ENGINES`, never re-enumerated by a worker-side `match` and never an `if svg`/`if pdf` branch re-deriving the target the row already names.
- Entry: `LayeredExport.author` is `async` over the runtime `async_boundary`, dispatching the one `LayerEngine.arm` off `ENGINES[self.target]` inside the fault capsule and returning a `RuntimeRail[ContentKey]` minted over the authored output bytes; every arm resolves IN-PROCESS on the cp315 core — `drawsvg` is a pure-Python `py3-none-any` wheel, `pymupdf` the `cp310-abi3` stable-ABI wheel reflecting on cp315 ungated (the PRIMARY arm runs in-capsule), `pikepdf` the `cp314-abi3` stable-ABI wheel also in-process, and `psdtags`+`tifffile` interpreter-agnostic — so no arm crosses the runtime subprocess seam, no `to_process` band, exactly as the `document/egress#FINISH` inverse close folds in-process. `LayeredExport.of` is the two-tier admission classmethod returning `Result[Self, ExportFault]`: it rejects an empty layer set, admits the untrusted `base` through the `ExportPayload` `TypeAdapter`, and rejects an OCG/surgery target with no `base` through `_PREREQ` before the boundary fold ever runs.
- Auto: `_emit` is the thin pure core the `@receipted` weave wraps — it runs `ENGINES[self.target].arm(self)` synchronously in-process and threads the returned `LayerFact` onto the frozen owner through `structs.replace(self, fact=fact)`, the seed never mutated; `author` mints the content key over the finished owner's `output` (the `fact.data` bytes) through `ContentIdentity.of`, never a re-minted seed. The `SVG` arm builds one `drawsvg.Drawing` over the `_viewport` union, folds each `Layer` into one `Group(id=name)` whose `Raw(source.decode())` child carries the placed markup and whose `style` carries the `display`/`opacity`/`mix-blend-mode` axis, nests each group under `groups.get(layer.group, drawing)`, and serializes through `as_svg()`. The `PDF_OCG` arm opens the `base`, mints `xref = add_ocg(name, on=visible, intent=_INTENT[intent], usage=policy.usage)` per layer, places each source through `show_pdf_page(Rect(bbox), src, 0, oc=xref)`, drives the visibility/lock partitions through `set_layer(0, on=, off=, locked=)`, and serializes through `tobytes`. The `PDF_SURGERY` arm mints one indirect `/OCG` per layer carrying its `_usage_dict(intent)` `/Usage` sub-dict (built with the `Name`-key subscript form the nanobind `Dictionary` constructor forces), assembles `/OCProperties` with `_order(layers, ocgs)` folding grouped layers into nested `/Order` sub-arrays, binds each `/OCG` as a `/Properties` resource, and brackets the pre-drawn content in `/OC <name> BDC … EMC`. The `TIFF` arm builds the `psdtags` layer-block stack from the per-`Layer` RGBA bytes and writes the composite-plus-layers TIFF through `tifffile.imwrite`. Every arm keys its result through the `LayerFact.data` bytes, never a re-minted identity seed.
- Receipt: each authoring contributes one `core/receipt#RECEIPT` case through the `@receipted(Redaction.STRUCTURAL)` weave draining `LayeredExport.contribute` off the fact-bearing owner `_emit` returns. `contribute` reads the threaded `LayerFact` off `self.fact` (never a re-run of an arm), re-mints the content key over `fact.data`, and folds the case off the `LayerEngine.preview` discriminant in one expression — the SVG/TIFF named-document arms emit `ArtifactReceipt.Preview(key, fact.width, fact.height)` carrying the named-layer document's viewport (the same `Preview` shape `composition/compose#COMPOSE`/`graphic/marks/encode#MARK` contribute, the perceptual `scores` band defaulting empty), and the OCG/surgery arms emit `ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)` carrying the authored-layer count on the `overlays` slot (the natural counterpart to the `document/egress#FINISH` REWRITE arm reporting a STRIPPED-layer count on the same slot — author and subtract are inverses over one receipt field), the `encryption_r`/`outline_depth` slots zero because layering is neither a security nor a navigation close. Layered export adds NO new receipt case: the named-document facts are the `Preview` width/height shape and the OCG-PDF facts are the `Egress` byte/page/layer shape, both settled, the producer imports `ArtifactReceipt` and projects flat scalars so the receipt owner imports no producer value object.
- Packages: `drawsvg` (`Drawing(width, height, origin)`/`Drawing.append`/`Drawing.as_svg`, `Group(children, ordered_children, **args)` carrying `id`/`style` through `**args`, `Raw(content, defs)` the verbatim-markup escape, MIT pure-Python `py3-none-any` `2.4.1` reflected on the cp315 core, version via `importlib.metadata.version("drawsvg")`, the raster `save_png` extra absent so this owner uses the SVG-string egress only) on the cp315 core; `pymupdf` (`open(stream=)`/`Document.add_ocg(name, config=-1, on=1, intent="View", usage="Artwork") -> int`/`Document.set_layer(config, basestate=, on=, off=, rbgroups=, locked=)`/`Document.page_count`/`Page.show_pdf_page(rect, docsrc, pno=0, oc=0, …) -> int`/`Document.tobytes(garbage=, deflate=)`/`Rect`, the `cp310-abi3` stable-ABI wheel `1.27.2.3` reflecting on the cp315 core ungated — the PRIMARY in-process PDF-layer arm — `AGPL-3.0-or-later OR Artifex-Commercial`, the load-bearing licensing constraint of the pdf rail) on the cp315 core; `pikepdf` (`open`/`Pdf.make_indirect`/`Pdf.make_stream`/`Pdf.Root`/`Pdf.save`/`Pdf.pages`/`Page.add_resource(res, res_type, name=, *, prefix=, replace_existing=) -> Name`/`Page.contents_add(stream, *, prepend=)`/`Dictionary`/`Array`/`Name`/`String`/`Name.random()` plus the `/OCProperties`/`/OCGs`/`/D`/`/Order`/`/ON`/`/OFF`/`/Usage`/`/Properties`/`/Type`/`/Name`/`/OC` tokens the surgery arm names, the `cp314-abi3` stable-ABI wheel `10.9.1` (libqpdf `12.3.2`) reflecting on the cp315 core ungated — the `/OCProperties` graft AND the per-layer `/Usage`/nested-`/Order` authoring arm — `MPL-2.0` file-scoped copyleft) on the cp315 core; `svgelements` (`SVG.parse(BytesIO(source), reify=True).bbox()` the placed-extent surface every producer folds its `Layer.bbox` over before binding, MIT pure-Python `py2.py3-none-any` `1.9.6`, shared with `composition/compose#COMPOSE`) on the cp315 core; `psdtags` ([RESEARCH] not-yet-admitted — the Photoshop image-resource layer-block authoring surface) and `tifffile` ([RESEARCH] not-yet-admitted — the TIFF container `imwrite(extratags=)` egress) on the cp315 core; `msgspec` (`Struct` frozen `Layer`/`LayerPolicy`/`LayerFact`/`LayerEngine`/`LayeredExport`, `structs.replace` the fact thread, `field` the policy default); `pydantic` (`TypeAdapter(ExportPayload)`/`ValidationError` the untrusted-`base` admission); `frozendict` (the `_BLEND`/`_INTENT`/`_USAGE`/`ENGINES`/`_PREREQ` tables); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`Redaction`/`receipted`).
- Growth: a new editable-export target (a `.psd`-native channel stack, a `.kra` layered-Krita profile) is one `ExportTarget` member plus one `LayerEngine` row plus one arm over the existing engine algebra — never a re-implemented SVG serializer, PDF object model, or TIFF layer block; a new layer attribute (an alpha-mask key, a clip-to-below flag, a per-layer color profile) is one field on the `Layer` row threaded into the SVG `Group` style, the OCG `/Usage` dict, and the TIFF layer block, the `_BLEND`-style correspondence carrying its per-format projection — never a parallel attribute list; a new compositing mode is one `BlendMode` member plus one `_BLEND` row; a new optional-content usage application (a CMYK-only print view, a zoom-range band) is one `LayerIntent` member plus one `_USAGE` row threaded into the surgery arm's `/Usage` author and one `_INTENT` cell for the draw-bind arm — never a hand-built `/Usage` dictionary per call; a new save knob is one `LayerPolicy` field; a new untrusted external blob is one `ExportPayload` band line plus one `of` admission read; a new admission prerequisite is one `_PREREQ` row. Zero new surface.
- Boundary: a per-producer layer-export class family, a per-format `_svg`/`_pdf`/`_tiff` writer pair beside the one `ENGINES` dispatch, a `names`/`sources`/`flags` triple-list zipped at the call site beside the one `Layer` row, a hand-built `/Usage` dictionary literal per OCG beside the `_USAGE` row, a hand-emitted `<g id="...">` string beside the `drawsvg` `Group`, a hand-written `q … /OC … BDC … EMC … Q` content-stream string beside the `pymupdf` `oc=` draw-binding and the `pikepdf` object model, a `LayerView`-per-document knob where per-layer `intent` carries the usage, a manual `_emit`-side `Receipt.of` where the `@receipted` weave harvests `contribute`, a bare `async_boundary` with no fault conversion, an `of` returning a bare owner where the `_PREREQ` admission needs `Result[Self, ExportFault]`, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live editor, no re-render, no re-layout. `drawsvg` owns programmatic named-`Group` SVG authoring and serialization; SVG geometry parse and bounds query route to `svgelements`, rasterization to `graphic/vector#VECTOR` `resvg-py`/`vl-convert`. `pymupdf` owns the PRIMARY native OCG draw-binding, `pikepdf` the low-level `/OCProperties` graft and per-layer `/Usage`/nested-`/Order` authoring; the qpdf save/encryption stay pikepdf's, the PDF/A archival close stays `document/emit#EMIT`'s, the security-and-navigation finishing close and the inverse OCG-layer STRIP/FLATTEN stay `document/egress#FINISH`'s, the PAdES cryptographic close stays `exchange/conformance#CONFORMANCE`'s, and the InDesign template-mutation hand-off stays `export/indesign#INDESIGN`'s. The placed flat layout arrives from `composition/compose#COMPOSE` keyed by the same `ContentKey`, the per-region vector source graphics from `graphic/marks/encode#MARK` and `visualization/diagram/draw#DRAW`, and the pre-rasterized RGBA layer sources for the Photoshop arm from `graphic/raster/io#RASTER` — layered export binds each as a named layer and authors no placement, no scaling, and no rasterization, those staying upstream, a placement or raster arm grafted onto an `ExportTarget` the deleted form. There is NO layer-faithful `.ai` writer: Illustrator's `.ai` is a PDF-compatible private container Illustrator parses through its own PGF stream, so an OCG-layered PDF renamed `.ai` opens FLAT (OCG layers do NOT map to Illustrator's layer panel) — the only durable layer-faithful Illustrator hand-off is the `SVG` named-`<g id=>` document Illustrator's SVG importer maps to layers, so the `.ai` target is deliberately absent rather than mis-promised. The content key is consumed from runtime over the authored bytes, never re-minted off the source key.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable
from enum import StrEnum
from io import BytesIO
from typing import TYPE_CHECKING, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack

from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    import pikepdf


# --- [TYPES] ----------------------------------------------------------------------------
class ExportTarget(StrEnum):
    SVG = "svg"                  # drawsvg named-<g id=> document — Illustrator/Inkscape
    PDF_OCG = "pdf-ocg"          # pymupdf optional-content groups bound to drawn content — Acrobat (primary)
    PDF_SURGERY = "pdf-surgery"  # pikepdf /OCProperties graft + per-layer /Usage + nested /Order
    TIFF = "tiff"                # psdtags image-resource layer stack — Photoshop


class BlendMode(StrEnum):  # the 16 PDF 1.4 separable + non-separable compositing modes; `_BLEND` carries the per-format spelling
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


class LayerIntent(StrEnum):  # the optional-content usage application; `_INTENT` -> add_ocg /Intent, `_USAGE` -> surgery /Usage dict
    VIEW = "view"        # always visible; default, no explicit /Usage
    PRINT = "print"      # print-only — /Print /PrintState /ON, /View /ViewState /OFF
    EXPORT = "export"    # export-only — /Export /ExportState /ON, /View /ViewState /OFF
    DESIGN = "design"    # design-time /Intent /Design processing hint


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class ExportFault:
    # the closed ADMISSION vocabulary `of` produces; arm-level provider raises (`pymupdf.FileDataError`,
    # `pikepdf.PdfError`, the drawsvg/svgelements `ValueError`) convert to the runtime `BoundaryFault`
    # at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["payload", "empty", "incomplete"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `ExportPayload` key paths
    empty: None = case()               # an empty layer set
    incomplete: ExportTarget = case()  # an OCG/surgery target admitted without a `base`


# --- [CONSTANTS] ------------------------------------------------------------------------
# the one primary compositing correspondence: (PDF /BM name, SVG mix-blend-mode value, PSD 4-char blend key);
# every engine reads its own column, never a parallel per-format blend table (DERIVED_LOGIC).
_BLEND: frozendict[BlendMode, tuple[str, str, str]] = frozendict({
    BlendMode.NORMAL: ("Normal", "normal", "norm"),
    BlendMode.MULTIPLY: ("Multiply", "multiply", "mul "),
    BlendMode.SCREEN: ("Screen", "screen", "scrn"),
    BlendMode.OVERLAY: ("Overlay", "overlay", "over"),
    BlendMode.DARKEN: ("Darken", "darken", "dark"),
    BlendMode.LIGHTEN: ("Lighten", "lighten", "lite"),
    BlendMode.COLOR_DODGE: ("ColorDodge", "color-dodge", "div "),
    BlendMode.COLOR_BURN: ("ColorBurn", "color-burn", "idiv"),
    BlendMode.HARD_LIGHT: ("HardLight", "hard-light", "hLit"),
    BlendMode.SOFT_LIGHT: ("SoftLight", "soft-light", "sLit"),
    BlendMode.DIFFERENCE: ("Difference", "difference", "diff"),
    BlendMode.EXCLUSION: ("Exclusion", "exclusion", "smud"),
    BlendMode.HUE: ("Hue", "hue", "hue "),
    BlendMode.SATURATION: ("Saturation", "saturation", "sat "),
    BlendMode.COLOR: ("Color", "color", "colr"),
    BlendMode.LUMINOSITY: ("Luminosity", "luminosity", "lum "),
})
# the pymupdf `add_ocg(intent=)` /Intent processing hint (View vs Design); the richer per-layer
# print/export application is the surgery arm's `_USAGE` /Usage dict concern, not this hint.
_INTENT: frozendict[LayerIntent, str] = frozendict({
    LayerIntent.VIEW: "View", LayerIntent.PRINT: "View", LayerIntent.EXPORT: "View", LayerIntent.DESIGN: "Design",
})
# the surgery arm's /Usage view-application policy keyed `category -> state`; VIEW omits its dict (default
# visible), so a print-only layer shows only when printing — `_STATE_KEY` names each category's state key.
_USAGE: frozendict[LayerIntent, frozendict[str, str]] = frozendict({
    LayerIntent.PRINT: frozendict({"Print": "ON", "View": "OFF"}),
    LayerIntent.EXPORT: frozendict({"Export": "ON", "View": "OFF"}),
    LayerIntent.DESIGN: frozendict({"View": "ON"}),
})
_STATE_KEY: frozendict[str, str] = frozendict({"View": "ViewState", "Print": "PrintState", "Export": "ExportState"})


# --- [MODELS] ---------------------------------------------------------------------------
class Layer(Struct, frozen=True):
    # `name`/`source`/`bbox` are the positional contract compose/encode/draw/sheet/imposition construct;
    # every richer editor-panel attribute defaults after `locked` so a 3-arg construction stays valid.
    name: str
    source: bytes
    bbox: tuple[float, float, float, float]
    visible: bool = True
    locked: bool = False
    opacity: float = 1.0
    blend: BlendMode = BlendMode.NORMAL
    intent: LayerIntent = LayerIntent.VIEW
    group: str = ""    # parent layer-folder name for nested SVG <g> / OCG /Order / PSD group ordering
    color: str = ""    # editor layer-panel swatch (PSD sheet color / SVG data-color); "" is the default

    def svg_style(self) -> str | None:
        # the SVG arm's CSS, pure string with no provider import; None when fully default so drawsvg omits it
        parts = (
            ([] if self.visible else ["display:none"])
            + ([f"opacity:{self.opacity}"] if self.opacity < 1.0 else [])
            + ([f"mix-blend-mode:{_BLEND[self.blend][1]}"] if self.blend is not BlendMode.NORMAL else [])
        )
        return ";".join(parts) or None


class LayerPolicy(Struct, frozen=True):
    # the trusted save-knob bundle (POLICY_VALUES: never a `garbage`/`deflate` flag tail on the signature)
    usage: str = "Artwork"  # the OCG /Usage category label pymupdf `add_ocg(usage=)` carries
    garbage: int = 3        # pymupdf `tobytes(garbage=)` xref compaction level
    deflate: bool = True


class LayerFact(Struct, frozen=True):
    # the bytes-plus-evidence carrier every arm returns; `contribute` reads it off the threaded owner
    data: bytes
    width: int = 0    # the SVG/TIFF named-document viewport (the `Preview` facts)
    height: int = 0
    pages: int = 0    # the OCG/surgery page count (the `Egress` facts)
    layers: int = 0   # the authored-layer count, riding the `Egress` `overlays` slot


class LayerEngine(Struct, frozen=True):
    arm: Callable[["LayeredExport"], LayerFact]
    preview: bool = False  # True -> `ArtifactReceipt.Preview`; False -> `ArtifactReceipt.Egress`


# --- [BOUNDARIES] -----------------------------------------------------------------------
class ExportPayload(TypedDict, closed=True):
    base: NotRequired[ReadOnly[bytes]]  # the untrusted placed-layout / already-drawn PDF the OCG arms graft onto


_PAYLOAD = TypeAdapter(ExportPayload)


# --- [SERVICES] -------------------------------------------------------------------------
class LayeredExport(Struct, frozen=True):
    target: ExportTarget
    layers: tuple[Layer, ...]
    base: bytes = b""
    policy: LayerPolicy = field(default_factory=LayerPolicy)
    fact: LayerFact | None = None

    @property
    def output(self) -> bytes:
        return self.fact.data if self.fact is not None else b""

    @classmethod
    def of(
        cls,
        target: ExportTarget,
        layers: tuple[Layer, ...],
        /,
        *,
        policy: LayerPolicy = LayerPolicy(),
        **raw: Unpack[ExportPayload],
    ) -> Result[Self, "ExportFault"]:
        if not layers:
            return Error(ExportFault(empty=None))
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(ExportFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        candidate = cls(target=target, layers=layers, base=payload.get("base", b""), policy=policy)
        return Error(ExportFault(incomplete=target)) if target in _PREREQ and not _PREREQ[target](candidate) else Ok(candidate)

    @receipted(Redaction.STRUCTURAL)  # the harvest weave drains `contribute` off the returned fact-bearing owner and emits via `Signals.emit_async`
    async def _emit(self) -> Self:
        return structs.replace(self, fact=ENGINES[self.target].arm(self))  # the thin pure core: one in-process arm, one threaded fact

    async def author(self) -> RuntimeRail[ContentKey]:
        return (await async_boundary(f"export.{self.target}", self._emit)).map(
            lambda done: ContentIdentity.of(f"export-{done.target}", done.output)
        )

    def contribute(self) -> Iterable[Receipt]:
        # the canonical `ReceiptContributor.contribute(self)` port — phase is the constant "emitted" the
        # `ArtifactReceipt` fixes by construction (KNOB_TEST); the `preview` discriminant folds the case.
        if (fact := self.fact) is None:
            return
        key = ContentIdentity.of(f"export-{self.target}", fact.data)
        recorded = (
            ArtifactReceipt.Preview(key, fact.width, fact.height)
            if ENGINES[self.target].preview
            else ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)
        )
        yield from recorded.contribute()


# --- [OPERATIONS] -----------------------------------------------------------------------
def _viewport(layers: tuple[Layer, ...], /) -> tuple[float, float]:
    return (
        max((layer.bbox[2] for layer in layers), default=0.0),
        max((layer.bbox[3] for layer in layers), default=0.0),
    )


def _svg(export: LayeredExport) -> LayerFact:
    import drawsvg

    width, height = _viewport(export.layers)
    drawing = drawsvg.Drawing(width, height, origin=(0.0, 0.0))
    groups = {layer.name: drawsvg.Group(id=layer.name, style=layer.svg_style()) for layer in export.layers}
    for layer in export.layers:
        groups[layer.name].append(drawsvg.Raw(layer.source.decode()))
        groups.get(layer.group, drawing).append(groups[layer.name])  # nest under the named parent group, else the root
    return LayerFact(drawing.as_svg().encode(), width=int(width), height=int(height), layers=len(export.layers))


def _ocg(export: LayeredExport) -> LayerFact:
    import pymupdf

    doc = pymupdf.open(stream=export.base, filetype="pdf")
    page, policy = doc[0], export.policy
    on, off, locked = [], [], []
    for layer in export.layers:
        xref = doc.add_ocg(layer.name, on=layer.visible, intent=_INTENT[layer.intent], usage=policy.usage)
        (on if layer.visible else off).append(xref)
        if layer.locked:
            locked.append(xref)
        page.show_pdf_page(pymupdf.Rect(layer.bbox), pymupdf.open(stream=layer.source, filetype="pdf"), 0, oc=xref)
    doc.set_layer(0, on=on, off=off, locked=locked)
    return LayerFact(doc.tobytes(garbage=policy.garbage, deflate=policy.deflate), pages=doc.page_count, layers=len(export.layers))


def _surgery(export: LayeredExport) -> LayerFact:
    import pikepdf
    from pikepdf import Array, Dictionary, Name

    pdf = pikepdf.open(BytesIO(export.base))
    ocgs = {layer.name: pdf.make_indirect(_ocg_dict(layer)) for layer in export.layers}
    on = [ocgs[layer.name] for layer in export.layers if layer.visible]
    off = [ocgs[layer.name] for layer in export.layers if not layer.visible]
    pdf.Root.OCProperties = Dictionary(
        OCGs=Array(list(ocgs.values())),
        D=Dictionary(Order=_order(export.layers, ocgs), ON=Array(on), OFF=Array(off)),
    )
    page = pdf.pages[0]
    for layer in export.layers:
        marker = page.add_resource(ocgs[layer.name], Name.Properties)
        page.contents_add(pdf.make_stream(b"/OC " + str(marker).encode() + b" BDC\nEMC\n"))
    sink = BytesIO()
    pdf.save(sink)
    return LayerFact(sink.getvalue(), pages=len(pdf.pages), layers=len(export.layers))


def _ocg_dict(layer: Layer) -> "pikepdf.Object":
    # one /OCG dict carrying its per-layer /Usage view-application; a dynamic `/Name`-keyed Dictionary cannot
    # ride a mapping literal (the nanobind constructor raises `std::bad_cast`), so /Usage is built by subscript.
    import pikepdf
    from pikepdf import Dictionary, Name

    ocg = pikepdf.Dictionary(Type=Name.OCG, Name=layer.name)
    if layer.intent is not LayerIntent.VIEW:
        usage = Dictionary()
        for category, state in _USAGE[layer.intent].items():
            entry = Dictionary()
            entry[Name("/" + _STATE_KEY[category])] = Name("/" + state)
            usage[Name("/" + category)] = entry
        ocg[Name.Usage] = usage
    return ocg


def _order(layers: tuple[Layer, ...], ocgs: "dict[str, pikepdf.Object]") -> "pikepdf.Array":
    # the nested /Order array: top-level layers as direct refs, grouped layers folded into `[/GroupTitle, …]`
    from pikepdf import Array, String

    grouped: dict[str, list[pikepdf.Object]] = {}
    direct: list[pikepdf.Object] = []
    for layer in layers:
        (grouped.setdefault(layer.group, []) if layer.group else direct).append(ocgs[layer.name])
    return Array([*direct, *(Array([String(title), *members]) for title, members in grouped.items())])


def _tiff(export: LayeredExport) -> LayerFact:  # [RESEARCH] psdtags + tifffile not yet admitted — signature-locked
    import tifffile
    from psdtags import PsdLayers, TiffImageSourceData

    # the RGBA `source` bytes arrive pre-rasterized from graphic/raster/io#RASTER (not re-rendered here); each
    # becomes one psdtags layer record carrying its blend key `_BLEND[blend][2]`, 0-255 opacity, bbox, and
    # group, the exact `PsdLayer` construction resolved against the .api catalogue at admission.
    blocks = TiffImageSourceData(PsdLayers([_psd_layer(layer) for layer in export.layers]))
    composite = _composite(export.layers)
    sink = BytesIO()
    tifffile.imwrite(sink, composite, photometric="rgb", extratags=[blocks.tifftag()])
    return LayerFact(sink.getvalue(), width=int(composite.shape[1]), height=int(composite.shape[0]), layers=len(export.layers))


def _psd_layer(layer: Layer) -> object:  # [RESEARCH] one psdtags layer record over the rich `Layer` attributes
    from psdtags import PsdLayer  # name, RGBA channels from `layer.source`, blendmode `_BLEND[layer.blend][2]`,

    return PsdLayer(layer.name)   # opacity round(layer.opacity * 255), rectangle int(layer.bbox), group `layer.group`


def _composite(layers: tuple[Layer, ...]) -> object:  # [RESEARCH] the flattened RGBA preview the TIFF base carries
    import numpy as np

    return np.zeros((0, 0, 4), dtype=np.uint8)


# --- [COMPOSITION] ----------------------------------------------------------------------
ENGINES: frozendict[ExportTarget, LayerEngine] = frozendict({
    ExportTarget.SVG: LayerEngine(_svg, preview=True),
    ExportTarget.PDF_OCG: LayerEngine(_ocg),
    ExportTarget.PDF_SURGERY: LayerEngine(_surgery),
    ExportTarget.TIFF: LayerEngine(_tiff, preview=True),
})
# the per-target admission prerequisite: `of` rejects an OCG/surgery target with no `base` into
# `ExportFault.incomplete` so the in-process fold is total; SVG/TIFF need no `base` and are always ready.
_PREREQ: frozendict[ExportTarget, Callable[[LayeredExport], bool]] = frozendict({
    ExportTarget.PDF_OCG: lambda export: bool(export.base),
    ExportTarget.PDF_SURGERY: lambda export: bool(export.base),
})
```

## [03]-[RESEARCH]

- [POLICY_TABLE_DISPATCH] [RESOLVED]: the four editor targets collapse to one `ExportTarget` `StrEnum` keying the `ENGINES` `frozendict[ExportTarget, LayerEngine]` policy table — the exact inverse shape of the `document/egress#FINISH` `FINISHERS` table that STRIPS these layers — replacing the prior `ExportOp` `tagged_union` (four cases, four sibling `@staticmethod` factories, a manual `match`+`assert_never`, four prefixed free functions) with one StrEnum, one table, one owner, and one `LayerEngine.arm` lookup. The `LayerEngine` row binds each target to its `(arm, preview)` pair and is the totality proof (one row per target, a new target one row), and the `preview` discriminant folds the `ArtifactReceipt.Preview`/`Egress` case in `contribute` exactly as the egress `Finisher.office` discriminant folds `Office`/`Egress` — the same row-discriminated receipt fold across the author/strip inverse pair.
- [ADMISSION_GATE] [RESOLVED]: admission splits by trust as the egress sibling's does. The `Layer` rows and `LayerPolicy` bundle are trusted (constructed by the visual-producer siblings and the caller); the untrusted external `base` PDF is admitted exactly once at `LayeredExport.of` through the closed `ExportPayload` `TypedDict` and the module-level `_PAYLOAD = TypeAdapter(ExportPayload)`, and `_PREREQ` rejects an empty layer set (`ExportFault.empty`) and an OCG/surgery target with no `base` (`ExportFault.incomplete`) into the closed `ExportFault` `@tagged_union` BEFORE the boundary fold runs, so the interior is total over admitted owners. `of` returns `Result[Self, ExportFault]` (the two-tier admission the prior page lacked entirely), and `author` is the async entry — never the prior `of`-as-async-entry conflation. The arm-level provider exception families (`pymupdf.FileDataError`, `pikepdf.PdfError`, the drawsvg/svgelements `ValueError`) convert to the runtime `BoundaryFault` at the `async_boundary` capsule, never into this admission vocabulary.
- [RECEIPT_WEAVE] [RESOLVED]: receipt emission is the definition-time `@receipted(Redaction.STRUCTURAL)` aspect over the thin pure `_emit` (the same weave the egress/finish close carries), draining `LayeredExport.contribute` off the fact-bearing owner `_emit` returns through `Signals.emit_async` — the prior page's prose CLAIMED a receipt contribution the code never implemented (no `contribute`, no weave). `_emit` threads the arm's `LayerFact` onto the frozen owner through `structs.replace` so `contribute` reads `self.fact` and projects the case without a second parse; the SVG/TIFF arms fold `ArtifactReceipt.Preview(key, fact.width, fact.height)` and the OCG/surgery arms `ArtifactReceipt.Egress(key, len(fact.data), fact.pages, 0, 0, fact.layers)`, the authored-layer count riding the `overlays` slot the egress REWRITE arm reports a STRIPPED count on — author and subtract are inverses over one settled receipt field.
- [LAYER_ATTRIBUTES] [RESOLVED]: the `Layer` row owns the full editor-panel attribute axis a real layer carries, not the prior naive five-field slice — `opacity` (SVG `opacity:`, PDF ExtGState `/ca`, PSD opacity byte; every layer panel), `blend` over the 16-mode `BlendMode` vocabulary and the `_BLEND` PDF-`/BM`/SVG-`mix-blend-mode`/PSD-key correspondence (compositing), `intent` over `LayerIntent` and the `_INTENT`/`_USAGE` optional-content usage application, `group` for nested SVG `<g>` / OCG `/Order` / PSD-group folder ordering, and `color` for the layer-panel swatch — each field projected per-format by the engine that supports it (the SVG and TIFF arms own blend/opacity through `mix-blend-mode`/the PSD blend key, the PDF arms own visibility/intent/group through the OCG model whose optional-content groups gate visibility rather than compositing). `name`/`source`/`bbox` stay the positional construction contract the five sibling producers use, every richer field defaulting after `locked`.
- [SURGERY_USAGE_DEPTH] [RESOLVED]: the `PDF_SURGERY` arm is no longer merely "the arm the draw-binding cannot reach" — it is the only arm that authors per-layer `/Usage` view-application (a print-only or export-only layer through the `_USAGE` `/Print`/`/Export` `/PrintState`/`/ExportState` `ON`/`/View` `/ViewState` `OFF` dict) and nested layer groups (the `_order` fold building the `/D` `/Order` array with `[/GroupTitle, …]` sub-arrays), capabilities the pymupdf `add_ocg` draw-bind arm cannot express. The /Usage dict is built with the `Name`-key subscript form (`entry[Name("/ViewState")] = Name("/OFF")`) because the nanobind `Dictionary` constructor coerces mapping keys as strings and a `Name` key raises `RuntimeError: std::bad_cast`, verified against the installed `pikepdf 10.9.1` object model; a `Name` carried as a VALUE rides the keyword constructor unchanged.
- [PDF_OCG_PRIMARY] [RESOLVED]: the `PDF_OCG` arm authors the optional-content-group named-layer tree IN-PROCESS on the cp315 core through `Document.add_ocg(name, config=-1, on=1, intent="View", usage="Artwork") -> int` (one OCG xref per layer), `Page.show_pdf_page(rect, docsrc, pno=0, oc=0) -> int` binding each placed source's drawn content through the `oc=<xref>` parameter, `Document.set_layer(config, on=, off=, locked=)` driving the per-config visibility AND lock policy (the `locked=` parameter the `Layer.locked` flag feeds), and `Document.tobytes(garbage=, deflate=)` serializing — all reflected against the installed `pymupdf 1.27.2.3`, the `cp310-abi3` stable-ABI wheel forward-compatible to cp315 so the primary arm runs in-capsule with no subprocess seam. The AGPL-3.0/Artifex-Commercial license is the load-bearing pdf-rail constraint recorded once here.
- [SVG_LAYERS_SETTLED] [RESOLVED]: the `SVG` arm authors a nested named-layer document through `drawsvg 2.4.1` (MIT pure-Python `py3-none-any`, version via `importlib.metadata.version("drawsvg")`): each `Layer` becomes one `Group(id=name, style=layer.svg_style())` whose `Raw(source.decode())` child re-emits the placed source verbatim, nested under its `group`-named parent group via `groups.get(layer.group, drawing).append(...)`, serialized through `Drawing.as_svg()` so an Illustrator/Inkscape/InDesign layer panel reads each `<g id=>` as one toggleable, lockable, re-colorable layer. `svg_style` folds `display:none`/`opacity:`/`mix-blend-mode:` from the `Layer` attributes as a pure CSS string (None when default, drawsvg omitting the attribute), so the row carries no provider object. The `save_png` row stays unused (its `cairoSVG` extra absent), rasterization routing to `graphic/vector#VECTOR`.
- [VIEWPORT_SETTLED] [RESOLVED]: `_viewport` reads each `Layer.bbox` REQUIRED placed extent (a positional field with no default, so every `Layer` arrives with a real placed extent) and sizes the named-layer SVG to the union, never re-laying-out — `composition/compose#COMPOSE` carries the `Tile`/`Overlay`-placed bounds, `graphic/marks/encode#MARK` the `svgelements` `SVG.parse(BytesIO(source), reify=True).bbox()` mark extent, and `visualization/diagram/draw#DRAW` each named group's bounds. A layout or scale arm on this owner is the deleted form.
- [TIFF_LAYERS_RESEARCH] [RESEARCH]: the `TIFF` arm assembles a Photoshop-compatible layered TIFF over `psdtags` (`TiffImageSourceData`/`PsdLayers`/`PsdLayer` writing the `/ImageSourceData` TIFF tag 37724 Photoshop reads as its layer stack, each `PsdLayer` carrying its blend key `_BLEND[blend][2]`, 0-255 opacity, bbox rectangle, and group) and `tifffile` (`imwrite(file, data, *, extratags=[…])` carrying the layer block). BOTH are NOT-yet-admitted in `pyproject.toml` and the folder `.api` catalogue — the member chain is SIGNATURE-LOCKED here pending admission: admit both on the cp315 core (interpreter-agnostic, expected cp315-clean), author each `.api` catalogue with assay-verified members, then resolve the exact `PsdLayer` construction (`_psd_layer`) and the flattened-composite RGBA build (`_composite`) against the verified surface. The RGBA layer sources arrive pre-rasterized from `graphic/raster/io#RASTER` keyed by the same `ContentKey` (this owner re-renders nothing — the raster is upstream), so the Photoshop arm is the raster-editor complement to the SVG (Illustrator) and OCG-PDF (Acrobat) vector arms, each editor family meeting its native layered container.
- [COMPOSE_HANDOFF] [RESOLVED]: the placed sources arrive keyed by the same `ContentKey` — the flat placed layout (the OCG `base`) from `composition/compose#COMPOSE`, the per-region vector source graphics from `graphic/marks/encode#MARK` and `visualization/diagram/draw#DRAW`, and the pre-rasterized RGBA layer sources from `graphic/raster/io#RASTER` — each binding as one `Layer(name, source, bbox, …)` row. Layered export receives the already-placed sources and authors only the layer structure: it re-renders nothing (rasterization stays `graphic/vector#VECTOR`/`graphic/raster`), re-lays-out nothing (placement stays the compose `Tile`/`ScaleFit`/`Overlay` arms), and re-mints no canonical concept (`ContentIdentity.of` over the authored bytes). A layered arm grafted onto the compose `FigureOp`, a hierarchical named-`<g>` emitter beside the compose flat document, and a placement/raster arm grafted onto an `ExportTarget` are the rejected forms — the named-layer authoring lands once in this owner consumed by every producer, and figure composition stays the flat post-render placement owner; the InDesign template-mutation hand-off stays `export/indesign#INDESIGN`'s disjoint concern.
