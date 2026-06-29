# [PY_ARTIFACTS_INDESIGN]

The IDML template-mutation hand-off — the editable InDesign deliverable authored by MUTATING an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is ONE frozen `msgspec.Struct` owner binding a `base: IdmlSource` template (the untrusted designer file admitted ONCE through the closed `IndesignPayload` `TypedDict` + module-level `TypeAdapter` at `Idml.of`) and a `steps: tuple[IdmlStep, ...]` fold of mutations threaded over the one running `IDMLPackage` and drained ONCE into a picklable `IdmlFact` — never a per-op base re-open and never four parallel `datas`/`prefixes`/`ats`/`onlys` lists zipped at the call site. `IdmlStep` is the closed fourteen-case `expression.tagged_union` over SimpleIDML's VERIFIED `IDMLPackage` `@use_working_copy` mutation algebra — the FULL step-eligible surface, not a slice: `Insert` (`insert_idml`), `AddPages` (`add_pages_from_idml`), `ImportXml` (`import_xml`), `PlacePdf` (`import_pdf` carrying the `PdfCrop` mode and `page_number`), `SetAttributes` (`set_attributes` — the verified `href` image-relink mechanism the old prose only asserted), `AddNote` (`add_note`), `MergeLayers` (`merge_layers`), `RemoveContent` (`remove_content`), `SuffixLayers` (`suffix_layers`), `RemoveLayer` (`remove_layer`), `RemoveOrphanLayers` (`remove_orphan_layers`), `RemoveGuides` (`remove_guides_on_layer`), `AddStory` (`add_story_with_content`), and `LeafToNode` (`xml_element_leaf_to_node`) — each carrying its own typed payload, dispatched by one total `match`+`assert_never`, never a `StrEnum` keyed against an erased `dict[str, object]` bag. Only `prefix` (the base namespace applied once on `Idml.base`) and the singular `add_page_from_idml` (subsumed by the batch `add_pages_from_idml`) are non-step members of the sixteen-method surface. It binds the placed layout `composition/compose#COMPOSE` produces into the IDML template the designer authored in InDesign: the template carries the named XML structure (the InDesign XML-ready document's tag tree), and this owner feeds content INTO that structure rather than emitting page geometry — keeping the data separate from the structure the IDML round-trip demands.

SimpleIDML is pure-Python over `zipfile`+`lxml`, so the mutation fold crosses the runtime `anyio.to_process` worker seam with the sibling `document/emit#EMIT` lxml arms, and only serialized package bytes plus picklable `IdmlFact` evidence cross back, never a live `IDMLPackage`/`_Element`. Each top-level `@use_working_copy` mutation EXTRACTS the package to a temp tree, mutates, REPACKAGES, overwrites the input file, closes the input, and returns a FRESH path-backed instance — so `_mutate` MUST thread the returned instance forward (a `BytesIO`-backed package crashes the decorator's `os.unlink(filename)`), bracket every spill and instance in one `contextlib.ExitStack`, and drain `Path(final.filename).read_bytes()` off the last instance. Each production is one `@receipted(_REDACTION)` `_emit` returning the stepped owner the harvest weave drains, contributing the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case (the IDML package IS an Office-class structured-document Zip archive) over the mutated-package byte count, never a new receipt case; `_REDACTION` is the keep-all `Redaction(classified=Map.empty())` the runtime owner verifies, never the phantom `Redaction.STRUCTURAL` named instance the `Struct(classified, salt)` does not define.

## [01]-[INDEX]


## [02]-[INDESIGN]

- Owner: `Idml` the one IDML template-mutation owner — a frozen `msgspec.Struct` binding `base: IdmlSource` (the InDesign-exported `.idml` template opened ONCE), `steps: tuple[IdmlStep, ...]` (the mutation fold threaded over the one running `IDMLPackage`), and `fact: IdmlFact | None` (the worker's threaded evidence), its `output` property projecting `fact.data`, never a per-op base re-open; `IdmlSource` the frozen row binding one bindable IDML document — `data` the package bytes, `prefix` the `IDMLPackage.prefix` namespace (an alphanumeric `\A\w+\Z` token applied before any merge to avoid reference collisions), `at` the destination XPath anchor, `only` the source sub-tree selector (defaulting `/Root`, NEVER `None` because `insert_idml`/`add_pages_from_idml` require a resolvable source xpath) — so a source is one row carrying its own bytes/prefix/anchors rather than four zipped parallel lists; `IdmlStep` the closed fourteen-case `expression.tagged_union` mutation family whose every case carries its own typed payload and whose one `facts` projection (the `StepFacts` carrier) feeds the admission gate in one total `match` — `sources` for data/prefix, `anchors` for the `_resolved` xpath check, `identifiers` for the non-empty layer/story/content-id check — collapsing the three parallel projection properties that each re-enumerated the same case family; `StepFacts` the frozen three-tuple admission projection (`sources`/`anchors`/`identifiers`) `of` folds once per step; `IdmlFact` the frozen evidence carrier the worker returns — the serialized `data` plus the `spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes` structural inventory read off the final package's introspection surface and the applied-`steps` count; `IdmlFault` the closed admission vocabulary `of` produces (`payload` the rejected `IndesignPayload` key paths, `empty_data`/`bad_prefix`/`empty_anchor`/`empty_ref` carrying the offending source/step/identifier index), distinct from the runtime `BoundaryFault` every worker provider raise converts to; `IndesignPayload` the closed `TypedDict` admitting the untrusted `template` bytes (and the base `prefix`) once through the module-level `_PAYLOAD = TypeAdapter`; `PdfCrop` the closed `StrEnum` of the VERIFIED eight-member Adobe `PDFCrop` enumeration (`CONTENT`/`CONTENT_VISIBLE`/`CONTENT_ALL`/`ART`/`PDF`/`TRIM`/`BLEED`/`MEDIA`) whose `.value` is the `import_pdf(crop=)` IDML token passed directly — never a hand-typed crop-string literal per call, never a `_CROP` dispatch table restating the enum's own values (the COLLAPSE_SCAN duplicate-entry form), and never the `CropPDF` spelling where the canonical `CROP_PDF` token is `PDFCrop` (the base `CONTENT = "CropContent"` is a distinct, dominant real-spread member beside the layer-scoped `CONTENT_VISIBLE`/`CONTENT_ALL`, never a phantom); the `SimpleIDML` `IDMLPackage` side-effect algebra (each `@use_working_copy` method returning a NEW path-backed instance) and its `spreads`/`stories`/`pages`/`font_families`/`style_groups`/`tags`/`referenced_layers`/`xml_structure`/`export_xml` introspection surface the worker-side mutation-and-read surface.
- Cases: `IdmlStep` cases — `Insert(module)` (template composition — open and prefix the `module` source, then `package.insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only))` so the sub-template's XML-tagged content lands at its destination XPath anchor, the returned fresh instance threaded into the next step) · `AddPages(pages)` (page combination — prefix each page source and fold `package.add_pages_from_idml([(part, number, at, only) …])` so the destination page count grows and the XML structure integrates each added file, the one SimpleIDML batch call) · `ImportXml(xml, at)` (XML content import — `package.import_xml(xml, at=…)` with the bytes passed directly, honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control attributes the source XML carries, so the data populates the structure without touching page geometry) · `PlacePdf(pdf, at, crop, page)` (PDF placement — spill a `composition/compose#COMPOSE`-produced `pdf`, then `package.import_pdf(uri, at=…, crop=crop.value, page_number=page)` so the chosen PDF page lands inside the named block as a linked, re-croppable PDF the designer re-links) · `SetAttributes(at, attrs)` (attribute/image rebind — `package.set_attributes(at, dict(attrs))`, the VERIFIED `href` image-resource relink mechanism — an `href` key updates the spread/story page-item resource path, an empty `href` removes the page item) · `AddNote(at, note, author)` (editorial annotation — `package.add_note(note, author, at=…)` attaches an InDesign collaboration note to the anchored element) · `MergeLayers(name)` (layer flatten — `package.merge_layers(with_name=name or None)` collapses every designmap layer into one and re-points the spread item layer references) · `RemoveContent(under)` (subtree clear — `package.remove_content(under)` recursively strips the content under the anchor, the template-reset inverse of `Insert`) · `SuffixLayers(suffix)` (layer-namespace suffix — `package.suffix_layers(suffix)` appends `suffix` to every designmap layer id, the layer-level analogue of the base `prefix` that keeps merged-in layers from colliding) · `RemoveLayer(layer_id)` (layer drop — `package.remove_layer(layer_id)` removes the designmap layer and, transitively, its guides) · `RemoveOrphanLayers()` (orphan sweep — `package.remove_orphan_layers()` drops every designmap layer no spread item references, the niladic post-merge cleanup) · `RemoveGuides(layer_id)` (guide clear — `package.remove_guides_on_layer(layer_id)` strips the guides on a layer while the layer itself stays, the narrower sibling of `RemoveLayer`) · `AddStory(story_id, element_id, element_tag)` (story creation — `package.add_story_with_content(story_id, element_id, element_tag)` mints a new story bound to an XML element and registers it in the designmap, the structure-population inverse of `RemoveContent`) · `LeafToNode(at, content_ref)` (leaf promotion — `package.xml_element_leaf_to_node(_resolved(package, at), content_ref)` converts a `Rectangle` leaf page-item into a `TextFrame` structural node so tagged content can nest beneath it) — matched by one total `match`/`case`+`assert_never`; never a sibling op per mutation and never an `if insert`/`if import` branch re-deriving the verb the case already names. The legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base: a template insert THEN a data import THEN a cover place THEN an image relink is ONE `Idml` over one threaded package rather than four re-opened round-trips.
- Auto: `_mutate` runs in the IDML worker seam: it spills `base.data` to a path-backed `tempfile.NamedTemporaryFile(delete_on_close=False)` (the `IDMLPackage` constructor opens a Zip over a real path; a `BytesIO` crashes the `@use_working_copy` `os.unlink(self.filename)` because a stream-backed `ZipFile.filename` is `None`), opens it as `idml.IDMLPackage(path)`, prefixes it through `package.prefix(base.prefix)`, and folds each `IdmlStep` through one total `match`+`assert_never` over a single `contextlib.ExitStack` that registers every spill and every returned instance for close-on-exit. Each top-level `@use_working_copy` mutation extracts to its own temp tree, mutates, repackages, overwrites the input file, closes the input, and returns a fresh path-backed `IDMLPackage`, so the fold THREADS the returned instance (`package = stack.enter_context(package.method(...))`) rather than reassigning one handle — a lost reference leaves an unclosed backing file the platform (notably Windows) cannot unlink. The `Insert` arm opens and prefixes the module then `insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only))`; `AddPages` builds the `(part, number, at, only)` batch with each `at` resolved against the destination and each `only` against its source; `ImportXml` passes the `xml` bytes straight to `import_xml`; `PlacePdf` spills the PDF, builds its `Path.as_uri()` `file:` URL, and `import_pdf(uri, crop=crop.value, page_number=page)`; `SetAttributes`/`AddNote`/`RemoveContent`/`LeafToNode` resolve their anchor (`LeafToNode` passing the resolved `at` and the unresolved `content_ref` id) and call the verified method; `MergeLayers`/`SuffixLayers`/`RemoveOrphanLayers` carry no anchor; `RemoveLayer`/`RemoveGuides`/`AddStory` pass their pre-admitted designmap/story ids straight to `remove_layer`/`remove_guides_on_layer`/`add_story_with_content` without `_resolved`, because a layer/story id keys the designmap, not the `xml_structure` tree an xpath resolves against. `_resolved` validates each XPath resolves in the live `package.xml_structure` lxml `_Element` tree through `_Element.xpath` BEFORE the mutation — an empty result raises `KeyError` so SimpleIDML never fails mid-fold on a phantom anchor, the raise crossing back to the `async_boundary` `BoundaryFault` conversion. After the fold, `_mutate` reads the full structural inventory off the final instance — `len(spreads)`/`len(stories)`/`len(pages)`/`len(font_families)`/`len(style_groups)`/`len(referenced_layers)`/`len(tags)` and the `xml_structure.iter()` node count — and drains `Path(package.filename).read_bytes()` into the `IdmlFact`.
- Receipt: `Idml.contribute` projects `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` off `self.fact` — an IDML package IS an Office-class structured-document Zip archive (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form — and the `@receipted(_REDACTION)` weave over the pure `_emit` drains the one-element `Iterable[Receipt]` and emits through `Signals`, exactly as `document/emit#EMIT` harvests its `DocumentPlan.contribute`. The rich structural evidence (`spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes`/`steps`) rides the `IdmlFact` carrier a consumer reads off `self.fact`, not the flat `Office` scalar — a structure-bearing receipt case is a `core/receipt#RECEIPT` growth concern, so IDML hand-off mints only the `Office` byte-count arity it owns. The content key is content-addressed off `fact.data` through `ContentIdentity.of`, so the `contribute` receipt mint and the `produced` return derive the identical key independently and the runtime reuse-fabric elides the duplicate.
- Growth: a future SimpleIDML mutation outside today's sixteen-method surface (a master-spread apply, a spread reorder, a hyperlink insert) is one `IdmlStep` case plus one `_mutate` arm plus one `facts` arm projecting its `StepFacts` (`sources`/`anchors`/`identifiers`) over the existing verified `IDMLPackage` algebra — never a re-implemented IDML Zip/XML serializer; a new source attribute (a target spread, a style override) is one field on `IdmlSource` threaded into the consuming arm; a new structural fact (a guide count, a hyperlink tally read off `xml_structure`) is one field on `IdmlFact`; a new admission cause is one `IdmlFault` case; a new untrusted ingress blob is one `IndesignPayload` band line; a new crop mode is one `PdfCrop` token whose `.value` threads into `import_pdf(crop=)`. The held-package fold absorbs an arbitrary step sequence over one running package; a second deliverable is one more element in `produced`'s `T | Iterable[T]`. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from collections.abc import Iterable
from contextlib import ExitStack
from copy import replace
from enum import StrEnum
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

from anyio import CapacityLimiter, to_process
from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

from artifacts.core.receipt import ArtifactReceipt

lazy from simple_idml import idml

if TYPE_CHECKING:
    from simple_idml.idml import IDMLPackage

# --- [TYPES] ----------------------------------------------------------------------------


class PdfCrop(StrEnum):
    # the VERIFIED eight-member Adobe `PDFCrop` enumeration; `.value` is the IDML `PDFCrop` attribute
    # token `import_pdf(crop=)` writes verbatim. `PDF` is the canonical `CROP_PDF` token `PDFCrop` (the
    # initialism breaks the `Crop` prefix), not the rarer `CropPDF` spelling; `CONTENT` is the base
    # `CropContent` member (the dominant real-spread token) beside its layer-scoped variants.
    CONTENT = "CropContent"                       # CROP_CONTENT — the content bounding box, layer-visibility-agnostic
    CONTENT_VISIBLE = "CropContentVisibleLayers"  # CROP_CONTENT_VISIBLE_LAYERS — the `import_pdf` default
    CONTENT_ALL = "CropContentAllLayers"          # CROP_CONTENT_ALL_LAYERS
    ART = "CropArt"                               # CROP_ART — the author-defined placeable artwork box
    PDF = "PDFCrop"                               # CROP_PDF — the Acrobat-displayed crop box
    TRIM = "CropTrim"                             # CROP_TRIM
    BLEED = "CropBleed"                           # CROP_BLEED
    MEDIA = "CropMedia"                           # CROP_MEDIA — the original physical paper size

# --- [CONSTANTS] ------------------------------------------------------------------------

_KIND: Final = "idml"                       # the `ContentIdentity.of` kind tag minted off the mutated-package bytes
_ROOT: Final = "/Root"                      # the IDML tag-tree root, the default destination/source anchor
_BASE_PREFIX: Final = "Base"                # the default base namespace prefix when the payload omits one
_PREFIX: Final = re.compile(r"\A\w+\Z")     # the alphanumeric word-char constraint `IDMLPackage.prefix` enforces (its `re.match(r"^\w+$")`)
# the keep-all `Redaction` the `@receipted` weave carries: IDML hand-off facts carry no secret field, so the
# receipt line redacts nothing. `Redaction(classified=Map.empty())` is the runtime owner's verified keep-all —
# `Redaction` is a `Struct(classified, salt)` with NO named instances, so `Redaction.STRUCTURAL` is the phantom it never defines.
_REDACTION: Final[Redaction] = Redaction(classified=Map.empty())

# --- [MODELS] ---------------------------------------------------------------------------


class IdmlSource(Struct, frozen=True):
    data: bytes
    prefix: str
    at: str = _ROOT      # destination XPath anchor (where this source's content lands in the running package)
    only: str = _ROOT    # source sub-tree selector; NEVER None — `insert_idml`/`add_pages_from_idml` require a resolvable xpath


class StepFacts(Struct, frozen=True):
    # the admission-relevant projection of one `IdmlStep`, the three obligation streams `of` folds: `sources`
    # feed the empty-data and `\A\w+\Z`-prefix checks, `anchors` the admission non-empty + worker `_resolved`
    # xpath gate over BOTH the destination `at` and source `only`, `identifiers` the non-empty designmap/story-id
    # check — one carrier so the gate reads one projection per step, not three.
    sources: tuple[IdmlSource, ...] = ()
    anchors: tuple[str, ...] = ()
    identifiers: tuple[str, ...] = ()


@tagged_union(frozen=True)
class IdmlStep:
    tag: Literal[
        "insert", "add_pages", "import_xml", "place_pdf", "set_attributes", "add_note", "merge_layers",
        "remove_content", "suffix_layers", "remove_layer", "remove_orphan_layers", "remove_guides", "add_story", "leaf_to_node",
    ] = tag()
    insert: IdmlSource = case()
    add_pages: tuple[tuple[IdmlSource, int], ...] = case()
    import_xml: tuple[bytes, str] = case()
    place_pdf: tuple[bytes, str, PdfCrop, int] = case()
    set_attributes: tuple[str, frozendict[str, str]] = case()
    add_note: tuple[str, str, str] = case()
    merge_layers: str = case()
    remove_content: str = case()
    suffix_layers: str = case()               # designmap layer-id namespace suffix, the layer-level analogue of the base `prefix`
    remove_layer: str = case()                # designmap layer `Self` id; drops the layer and its guides
    remove_orphan_layers: None = case()       # niladic — strips every layer no spread references
    remove_guides: str = case()               # layer id whose guides clear while the layer itself stays
    add_story: tuple[str, str, str] = case()  # (story_id, xml_element_id, xml_element_tag) — a new story bound to an XML element
    leaf_to_node: tuple[str, str] = case()    # (destination xpath anchor, xml content ref) — Rectangle leaf promoted to a TextFrame node

    @classmethod
    def Insert(cls, module: IdmlSource, /) -> Self:
        return cls(insert=module)

    @classmethod
    def AddPages(cls, pages: tuple[tuple[IdmlSource, int], ...], /) -> Self:
        return cls(add_pages=pages)

    @classmethod
    def ImportXml(cls, xml: bytes, at: str = _ROOT, /) -> Self:
        return cls(import_xml=(xml, at))

    @classmethod
    def PlacePdf(cls, pdf: bytes, at: str, crop: PdfCrop = PdfCrop.CONTENT_VISIBLE, page: int = 1, /) -> Self:
        return cls(place_pdf=(pdf, at, crop, page))

    @classmethod
    def SetAttributes(cls, at: str, attrs: frozendict[str, str], /) -> Self:
        return cls(set_attributes=(at, attrs))

    @classmethod
    def AddNote(cls, at: str, note: str, author: str, /) -> Self:
        return cls(add_note=(at, note, author))

    @classmethod
    def MergeLayers(cls, name: str = "", /) -> Self:
        return cls(merge_layers=name)

    @classmethod
    def RemoveContent(cls, under: str, /) -> Self:
        return cls(remove_content=under)

    @classmethod
    def SuffixLayers(cls, suffix: str, /) -> Self:
        return cls(suffix_layers=suffix)

    @classmethod
    def RemoveLayer(cls, layer_id: str, /) -> Self:
        return cls(remove_layer=layer_id)

    @classmethod
    def RemoveOrphanLayers(cls) -> Self:
        return cls(remove_orphan_layers=None)

    @classmethod
    def RemoveGuides(cls, layer_id: str, /) -> Self:
        return cls(remove_guides=layer_id)

    @classmethod
    def AddStory(cls, story_id: str, element_id: str, element_tag: str, /) -> Self:
        return cls(add_story=(story_id, element_id, element_tag))

    @classmethod
    def LeafToNode(cls, at: str, content_ref: str, /) -> Self:
        return cls(leaf_to_node=(at, content_ref))

    @property
    def facts(self) -> StepFacts:
        # one total `match` projecting all three admission obligations per case — collapsing the prior
        # parallel `sources`/`anchors`/`identifiers` properties, each re-enumerating the same 14-case family.
        # BOTH the destination `at` and the source `only` ride `anchors` (each an xpath the worker `_resolved`-checks
        # against its own tree; admission non-empty-checks both so an empty `only` never reaches `insert_idml`'s
        # `xpath(only)[0]`). Layer/story/content ids ride `identifiers` (non-empty-checked WITHOUT `_resolved`: they
        # key the designmap and story files, not the `xml_structure` tree an xpath anchor resolves against).
        match self:
            case IdmlStep(tag="insert", insert=module):
                return StepFacts(sources=(module,), anchors=(module.at, module.only))
            case IdmlStep(tag="add_pages", add_pages=pages):
                return StepFacts(sources=tuple(src for src, _ in pages), anchors=tuple(a for src, _ in pages for a in (src.at, src.only)))
            case (
                IdmlStep(tag="import_xml", import_xml=(_, at)) | IdmlStep(tag="place_pdf", place_pdf=(_, at, _, _))
                | IdmlStep(tag="set_attributes", set_attributes=(at, _)) | IdmlStep(tag="add_note", add_note=(at, _, _))
                | IdmlStep(tag="remove_content", remove_content=at)
            ):
                return StepFacts(anchors=(at,))
            case IdmlStep(tag="leaf_to_node", leaf_to_node=(at, content_ref)):
                return StepFacts(anchors=(at,), identifiers=(content_ref,))
            case IdmlStep(tag="remove_layer", remove_layer=ref) | IdmlStep(tag="remove_guides", remove_guides=ref):
                return StepFacts(identifiers=(ref,))
            case IdmlStep(tag="add_story", add_story=(story_id, element_id, _)):
                return StepFacts(identifiers=(story_id, element_id))
            case IdmlStep(tag="merge_layers") | IdmlStep(tag="suffix_layers") | IdmlStep(tag="remove_orphan_layers"):
                return StepFacts()
            case _ as unreachable:
                assert_never(unreachable)


class IdmlFact(Struct, frozen=True):
    data: bytes
    spreads: int = 0
    stories: int = 0
    pages: int = 0
    fonts: int = 0      # len(package.font_families)
    styles: int = 0     # len(package.style_groups)
    layers: int = 0     # len(package.referenced_layers)
    tags: int = 0       # len(package.tags)
    nodes: int = 0      # xml_structure.iter() node count
    steps: int = 0

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IdmlFault:
    # the closed ADMISSION vocabulary `of` produces; a worker provider raise (a malformed `.idml` Zip, an
    # `lxml.etree.XMLSyntaxError`, the `_resolved` missing-anchor `KeyError`, an `IDMLPackage.prefix`
    # `BaseException`) converts to the runtime `BoundaryFault` at the `async_boundary`, never this vocabulary.
    tag: Literal["payload", "empty_data", "bad_prefix", "empty_anchor", "empty_ref"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `IndesignPayload` key paths from the `TypeAdapter` miss
    empty_data: int = case()           # source-row index carrying empty `.idml` payload bytes
    bad_prefix: int = case()           # source-row index whose prefix is empty or fails `\A\w+\Z`
    empty_anchor: int = case()         # step-anchor index carrying an empty `at` destination or `only` source xpath
    empty_ref: int = case()            # step-identifier index carrying an empty layer/story/content id

# --- [BOUNDARIES] -----------------------------------------------------------------------


class IndesignPayload(TypedDict, closed=True):
    template: Required[ReadOnly[bytes]]  # the untrusted InDesign-exported `.idml` the steps mutate
    prefix: NotRequired[ReadOnly[str]]   # the base namespace prefix applied before any merge (default `_BASE_PREFIX`)


_PAYLOAD = TypeAdapter(IndesignPayload)

# --- [SERVICES] -------------------------------------------------------------------------


class Idml(Struct, frozen=True):
    base: IdmlSource
    steps: tuple[IdmlStep, ...]
    fact: IdmlFact | None = None

    @property
    def output(self) -> bytes:
        return self.fact.data if self.fact is not None else b""

    @classmethod
    def of(cls, steps: tuple[IdmlStep, ...], /, **raw: Unpack[IndesignPayload]) -> Result[Self, IdmlFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(IdmlFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        base = IdmlSource(data=payload["template"], prefix=payload.get("prefix", _BASE_PREFIX))
        projected = tuple(step.facts for step in steps)
        sources = (base, *(src for facts in projected for src in facts.sources))
        anchors = tuple(anchor for facts in projected for anchor in facts.anchors)
        identifiers = tuple(ref for facts in projected for ref in facts.identifiers)
        bad_data = next((i for i, src in enumerate(sources) if not src.data), None)
        bad_prefix = next((i for i, src in enumerate(sources) if not _PREFIX.match(src.prefix)), None)
        bad_anchor = next((i for i, anchor in enumerate(anchors) if not anchor), None)
        bad_ref = next((i for i, ref in enumerate(identifiers) if not ref), None)
        return (
            Error(IdmlFault(empty_data=bad_data)) if bad_data is not None
            else Error(IdmlFault(bad_prefix=bad_prefix)) if bad_prefix is not None
            else Error(IdmlFault(empty_anchor=bad_anchor)) if bad_anchor is not None
            else Error(IdmlFault(empty_ref=bad_ref)) if bad_ref is not None
            else Ok(cls(base=base, steps=steps))
        )

    @receipted(_REDACTION)
    async def _emit(self) -> Self:
        # returns the stepped owner (a `ReceiptContributor`) the harvest weave drains; the lxml-bound
# mutation crosses the IDML worker seam and only the serialized `IdmlFact` returns.
        return replace(self, fact=await to_process.run_sync(_mutate, self, limiter=_GATE))

    def contribute(self) -> Iterable[Receipt]:
        if (fact := self.fact) is None:
            return
        key = ContentIdentity.of(_KIND, fact.data)
        yield from ArtifactReceipt.Office(key, len(fact.data)).contribute()

# --- [OPERATIONS] -----------------------------------------------------------------------


async def produced(plans: "Idml | Iterable[Idml]", /) -> RuntimeRail[Block[ContentKey]]:
    block = Block.singleton(plans) if isinstance(plans, Idml) else Block.of_seq(plans)
    return await async_boundary("export.indesign", lambda: _produced(block))


async def _produced(block: "Block[Idml]", /) -> Block[ContentKey]:
    stepped = [await plan._emit() for plan in block]
    return Block.of_seq([ContentIdentity.of(_KIND, plan.output) for plan in stepped])


def _mutate(plan: Idml) -> IdmlFact:  # [RESEARCH]: simple_idml -> lxml runs behind the worker boundary
    with ExitStack() as stack:
        package = stack.enter_context(idml.IDMLPackage(str(_spill(stack, plan.base.data, ".idml"))).prefix(plan.base.prefix))
        for step in plan.steps:
            match step:
                case IdmlStep(tag="insert", insert=module):
                    part = stack.enter_context(idml.IDMLPackage(str(_spill(stack, module.data, ".idml"))).prefix(module.prefix))
                    package = stack.enter_context(package.insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only)))
                case IdmlStep(tag="add_pages", add_pages=pages):
                    parts = [stack.enter_context(idml.IDMLPackage(str(_spill(stack, src.data, ".idml"))).prefix(src.prefix)) for src, _ in pages]
                    specs = [(part, number, _resolved(package, src.at), _resolved(part, src.only)) for part, (src, number) in zip(parts, pages, strict=True)]
                    package = stack.enter_context(package.add_pages_from_idml(specs))
                case IdmlStep(tag="import_xml", import_xml=(xml, at)):
                    package = stack.enter_context(package.import_xml(xml, at=_resolved(package, at)))
                case IdmlStep(tag="place_pdf", place_pdf=(pdf, at, crop, page)):
                    package = stack.enter_context(package.import_pdf(_spill(stack, pdf, ".pdf").as_uri(), at=_resolved(package, at), crop=crop.value, page_number=page))
                case IdmlStep(tag="set_attributes", set_attributes=(at, attrs)):
                    package = stack.enter_context(package.set_attributes(_resolved(package, at), dict(attrs)))
                case IdmlStep(tag="add_note", add_note=(at, note, author)):
                    package = stack.enter_context(package.add_note(note, author, at=_resolved(package, at)))
                case IdmlStep(tag="merge_layers", merge_layers=name):
                    package = stack.enter_context(package.merge_layers(with_name=name or None))
                case IdmlStep(tag="remove_content", remove_content=under):
                    package = stack.enter_context(package.remove_content(_resolved(package, under)))
                case IdmlStep(tag="suffix_layers", suffix_layers=suffix):
                    package = stack.enter_context(package.suffix_layers(suffix))
                case IdmlStep(tag="remove_layer", remove_layer=layer_id):
                    package = stack.enter_context(package.remove_layer(layer_id))
                case IdmlStep(tag="remove_orphan_layers"):
                    package = stack.enter_context(package.remove_orphan_layers())
                case IdmlStep(tag="remove_guides", remove_guides=layer_id):
                    package = stack.enter_context(package.remove_guides_on_layer(layer_id))
                case IdmlStep(tag="add_story", add_story=(story_id, element_id, element_tag)):
                    package = stack.enter_context(package.add_story_with_content(story_id, element_id, element_tag))
                case IdmlStep(tag="leaf_to_node", leaf_to_node=(at, content_ref)):
                    package = stack.enter_context(package.xml_element_leaf_to_node(_resolved(package, at), content_ref))
                case _ as unreachable:
                    assert_never(unreachable)
        structure = package.xml_structure
        return IdmlFact(
            data=Path(package.filename).read_bytes(),
            spreads=len(package.spreads),
            stories=len(package.stories),
            pages=len(package.pages),
            fonts=len(package.font_families),
            styles=len(package.style_groups),
            layers=len(package.referenced_layers),
            tags=len(package.tags),
            nodes=sum(1 for _ in structure.iter()),
            steps=len(plan.steps),
        )


def _spill(stack: ExitStack, data: bytes, suffix: str, /) -> Path:
    # the `IDMLPackage` constructor opens a path-backed Zip and the `@use_working_copy` decorator
    # `os.unlink`s `self.filename`, so a stream-backed package (filename `None`) crashes; spill to a temp
    # file kept on disk past `close()` and unlinked on stack exit, the OS handle released so the package reopens it.
    handle = stack.enter_context(NamedTemporaryFile(suffix=suffix, delete_on_close=False))
    handle.write(data)
    handle.close()
    return Path(handle.name)


def _resolved(package: "IDMLPackage", xpath: str, /) -> str:
    # one guard for BOTH the destination `at` and the source `only`: the IDML tag tree is an lxml
    # `_Element`, and an empty `xpath` result is the missing-anchor signal raised before the mutation so
    # SimpleIDML never fails mid-fold and the `async_boundary` converts the `KeyError` to a `BoundaryFault`.
    if not package.xml_structure.xpath(xpath):
        raise KeyError(xpath)
    return xpath


# --- [COMPOSITION] ----------------------------------------------------------------------
# every IDML mutation offload threads this one bound, so N concurrent `produced` calls share a fixed
# subprocess pool instead of fanning out at the per-loop process-limiter default — the worker bound
# `export/layered#LAYERED`'s `ORA` worker carries. The lxml/SimpleIDML worker is heavy (zip extract +
# repackage + designmap rewrite per step), so the bound stays small.
_GATE: Final[CapacityLimiter] = CapacityLimiter(4)
```

## [03]-[RESEARCH]

- [STEP_PIPELINE] [RESOLVED]: the owner binds `base: IdmlSource` opened ONCE and folds `steps: tuple[IdmlStep, ...]` over the one running `IDMLPackage` — the VERIFIED `@use_working_copy` decorator (`src/simple_idml/decorators.py`) makes each top-level mutation extract the package to a temp tree, mutate, repackage to a new archive, `os.unlink` the input file, `shutil.move` the working copy over it, close the input, and return a FRESH `IDMLPackage(filename)`. So `_mutate` threads each returned instance forward and registers every instance and every spill in one `contextlib.ExitStack` for close-on-exit — a lost reference leaves an unclosed backing file the platform (notably Windows) cannot `os.unlink`, and a `BytesIO`-backed package crashes the decorator's `os.unlink(self.filename)` because a stream-backed `ZipFile.filename` is `None`, which is why `_spill` materializes every blob to a real temp path. This is the structural collapse of the prior four monolithic ops: a template insert THEN a data import THEN a cover place THEN an image relink is one `Idml(base, steps)` producing one deliverable, the base lifted out of every case and declared once on `Idml.base`, the SimpleIDML batch `add_pages_from_idml(specs)` one `AddPages` step while per-document `insert_idml` is one `Insert` step each.
- [MUTATION_FAMILY] [RESOLVED]: `IDMLPackage` carries SIXTEEN `@use_working_copy` mutations (verified against `src/simple_idml/idml.py`), and `IdmlStep` now models the FULL step-eligible fourteen — the two non-step members are `prefix` (the base namespace applied once on `Idml.base`, not a per-step verb) and the singular `add_page_from_idml` (subsumed by the batch `add_pages_from_idml`, which itself folds `(package, page_number, at, only)` tuples). Each case cites a VERIFIED member: `Insert`/`AddPages`/`ImportXml`/`PlacePdf` (`import_pdf(pdf_path, at, crop="CropContentVisibleLayers", page_number=1)`, the default crop and page-number both source-confirmed)/`SetAttributes` (the REAL `href` image-resource relink — `set_attributes` writes the spread/story resource path and removes the page item on an empty `href`)/`AddNote`/`MergeLayers`/`RemoveContent`, plus the layer-and-structure family the prior eight-case slice dropped: `SuffixLayers` (`suffix_layers(suffix)`, layer-id namespace suffix, the layer-level analogue of `prefix`), `RemoveLayer` (`remove_layer(layer_id)`, drops the layer and its guides), `RemoveOrphanLayers` (`remove_orphan_layers()`, niladic, strips every layer no spread references), `RemoveGuides` (`remove_guides_on_layer(layer_id)`, the guide-only narrower sibling), `AddStory` (`add_story_with_content(story_id, xml_element_id, xml_element_tag)`, the structure-population inverse of `RemoveContent`), and `LeafToNode` (`xml_element_leaf_to_node(xpath, xml_content_ref)`, `Rectangle`-to-`TextFrame` promotion). The earlier "ten-method surface, extended to eight" framing undercounted the real surface and deferred genuine verbs on a zero-consumer pretext; the family now closes the full step-eligible algebra so a new requirement that needs a layer suffix, an orphan sweep, or a story mint lands as an existing case, not a re-opened design.
- [ANCHOR_VALIDATION] [RESOLVED]: `_resolved` validates each XPath resolves in the live `package.xml_structure` lxml `_Element` tree through `_Element.xpath` (the folder `.api/lxml.md` query-entrypoint `[02]` row) BEFORE the mutation, raising `KeyError` on an empty result so SimpleIDML never fails mid-fold and the `async_boundary` lifts it to a `BoundaryFault`. The prior shape validated only the destination `at`; the one `_resolved` guard now also validates the source `only` selector (`insert_idml`/`add_pages_from_idml` read `idml_package.xml_structure.xpath(only)[0]`, raising `IndexError` on a phantom `only`) and `LeafToNode`'s `at`, collapsing what would have been a `_anchored`/`_sourced` pair into one helper keyed by which package it reads. The `insert`/`add_pages` `facts.anchors` stream now carries that source `only` beside the destination `at`, so admission non-empty-checks BOTH before the worker — an empty `only` is rejected as `empty_anchor` at `of` and never reaches the worker's `xpath(only)[0]` index — while resolvability against the live source/destination tree stays the worker's `_resolved` gate. Layer, story, and content ids (`RemoveLayer`/`RemoveGuides`/`AddStory`/`LeafToNode.content_ref`) are NOT routed through `_resolved` — they key the designmap and story files, not the `xml_structure` tree — so the `facts.identifiers` stream non-empty-checks them at admission while `_resolved` owns the xpath-resolvable anchors alone, the two gates kept distinct rather than forcing a designmap id through an xpath resolver that would wrongly reject it. The one `StepFacts` projection carries all three obligation streams (`sources`/`anchors`/`identifiers`) per case in a single total `match`, where the prior shape re-enumerated the fourteen-case family across three parallel properties.
- [CROP_ENUM] [RESOLVED]: `PdfCrop` is the VERIFIED eight-member Adobe `PDFCrop` enumeration (`developer.adobe.com/indesign/dom/api/p/PDFCrop` and real `.idml` spread files) — `CROP_CONTENT`/`CROP_CONTENT_VISIBLE_LAYERS`/`CROP_CONTENT_ALL_LAYERS`/`CROP_ART`/`CROP_PDF`/`CROP_TRIM`/`CROP_BLEED`/`CROP_MEDIA`. The base `CONTENT = "CropContent"` (the layer-visibility-agnostic content bounding box) is a distinct DOM member and the dominant token in real `PDFAttribute PDFCrop="…"` spread data, restored beside the layer-scoped `CONTENT_VISIBLE`/`CONTENT_ALL` variants — it is NOT a phantom. The one token correction is `PDF = "PDFCrop"`: the canonical `CROP_PDF` IDML token is `PDFCrop` (the SimpleIDML README `crop="PDFCrop"`), not the rarer `CropPDF` spelling. The default `CONTENT_VISIBLE = "CropContentVisibleLayers"` matches `import_pdf`'s `crop=` default and the token InDesign writes into real spread elements.
- [ADMISSION_GATE] [RESOLVED]: the untrusted `.idml` template is now admitted exactly once at `Idml.of` through the closed `IndesignPayload` `TypedDict` and the module-level `_PAYLOAD = TypeAdapter`, mapping a `ValidationError` to `IdmlFault.payload` — the lifecycle-spine fix the prior shape lacked entirely (the raw bytes flowed straight into `IdmlSource.data` with only non-empty checks), bringing IDML in line with the `document/emit#EMIT` `EmitPayload` and `export/layered#LAYERED` `ExportPayload` admission. `of` then validates `prefix` against the `_PREFIX` `\A\w+\Z` pattern — the alphanumeric constraint `IDMLPackage.prefix` raises `BaseException` on (its own `re.match(r"^\w+$")`) — where the prior `empty_prefix` arm only rejected emptiness, so a non-alphanumeric prefix can no longer slip through to crash the worker, and the `empty_ref` arm rejects an empty layer/story/content id before a layer-or-story mutation reaches the worker, so the worker fold stays total across the full fourteen-verb family rather than only the data/prefix/anchor checks the eight-case slice covered.
- [STRUCTURAL_FACT] [RESOLVED]: `IdmlFact` carries the FULL introspection inventory SimpleIDML exposes, not the prior four-field slice — `fonts` (`len(font_families)`), `styles` (`len(style_groups)`), `layers` (`len(referenced_layers)`), and `tags` (`len(tags)`) join `spreads`/`stories`/`pages`/`nodes`/`steps`, all read off the verified introspection properties on the final instance, so a consumer reading `self.fact` sees the template's real structural shape rather than a vertex-array-thin slice. The rich evidence rides the carrier; `contribute` mints only the `Office` byte-count arity, a structure-bearing receipt being a `core/receipt#RECEIPT` growth concern.
- [SIMPLEIDML_MEMBER_SURFACE] [RESOLVED]: the SimpleIDML member chain is admitted through the root manifest and `libs/python/artifacts/.api/simpleidml.md`, and is verified against `Starou/SimpleIDML` `src/simple_idml/idml.py` (the `IDMLPackage(zipfile.ZipFile)` class), `src/simple_idml/decorators.py` (the `@use_working_copy` extract-mutate-repackage contract), and `src/simple_idml/__init__.py` (`SETCONTENT_TAG`/`IGNORECONTENT_TAG`/`FORCECONTENT_TAG` the content-control attribute names, `VERSION = "1.3.1"`) — every mutation and introspection member this owner names exists with the spelled signature, and the full sixteen-method `@use_working_copy` surface was enumerated so the fourteen step-eligible verbs are modeled rather than a slice (`prefix` and the singular `add_page_from_idml` the two non-step members). `_spill`/backing-file-drain temp-file plumbing is the owner-local boundary around that admitted provider; raw `IDMLPackage` and lxml nodes never cross out of the worker.
- [SERVER_CONVERSION_OUT_OF_SCOPE] [RESOLVED]: the SimpleIDML InDesign-Server SOAP conversion path (`simple_idml.indesign.indesign.save_as(path, [{"fmt": "indd"|"pdf"|"jpeg"|"idml"|"zip"}], server_url, client_workdir, server_workdir)`) converts an `.idml` to `.indd`/`.pdf`/`.jpeg` through a LIVE InDesign Server and a shared working directory — OUT OF SCOPE for the host-free durable-output engine, which owns neither a network InDesign Server seat nor a co-mounted filesystem. The deliverable this owner produces is the editable `.idml` the designer opens in InDesign directly; the `.indd`/`.pdf` server render is the rejected arm.
- [LAYERED_SIBLING_BOUNDARY] [RESOLVED]: IDML is the InDesign template-mutation hand-off; `export/layered#LAYERED` is the Illustrator/Acrobat/Photoshop named-layer hand-off (SVG `<g id=>` Groups, PDF OCG optional-content groups, layered TIFF). The two share the editable-export domain but own disjoint editor families and disjoint formats — IDML mutates a designer's `.idml` template through its XML structure, layered authors named layers into SVG/PDF/TIFF from the placed sources — so a layered arm grafted onto `IdmlStep` or an IDML arm grafted onto `ExportTarget` is the rejected form. Both consume the same `composition/compose#COMPOSE` placed layout keyed by the same `ContentKey`, both admit untrusted external bytes through a closed `TypedDict`+`TypeAdapter` at `of`, and both thread one typed fact onto the frozen owner through `replace` for the `@receipted` weave — the IDML owner's `IdmlSource(data, prefix, at, only)` template-binding row is the layered owner's `Layer(name, source, bbox, …)` named-layer counterpart, each export owner binding the placed layout into its native editor format through its own typed source row, never a shared erased source bag.
