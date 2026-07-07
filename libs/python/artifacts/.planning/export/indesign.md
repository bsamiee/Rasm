# [PY_ARTIFACTS_INDESIGN]

The IDML template-mutation hand-off — the editable InDesign deliverable authored by MUTATING an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is ONE frozen `msgspec.Struct` owner binding a `base: IdmlSource` template (the untrusted designer file admitted ONCE through the closed `IndesignPayload` `TypedDict` + module-level `TypeAdapter` at `Idml.of`) and a `steps: tuple[IdmlStep, ...]` fold of mutations threaded over the one running `IDMLPackage` and drained ONCE into a picklable `IdmlFact` — never a per-op base re-open and never four parallel `datas`/`prefixes`/`ats`/`onlys` lists zipped at the call site. `IdmlStep` is the closed fourteen-case `expression.tagged_union` over SimpleIDML's VERIFIED `IDMLPackage` `@use_working_copy` mutation algebra — the FULL step-eligible surface, not a slice: `Insert` (`insert_idml`), `AddPages` (`add_pages_from_idml`), `ImportXml` (`import_xml`), `PlacePdf` (`import_pdf` carrying the `PdfCrop` mode and `page_number`), `SetAttributes` (`set_attributes` — the verified `href` image-relink mechanism the old prose only asserted), `AddNote` (`add_note`), `MergeLayers` (`merge_layers`), `RemoveContent` (`remove_content`), `SuffixLayers` (`suffix_layers`), `RemoveLayer` (`remove_layer`), `RemoveOrphanLayers` (`remove_orphan_layers`), `RemoveGuides` (`remove_guides_on_layer`), `AddStory` (`add_story_with_content`), and `LeafToNode` (`xml_element_leaf_to_node`) — each carrying its own typed payload, dispatched by one total `match`+`assert_never`, never a `StrEnum` keyed against an erased `dict[str, object]` bag. Only `prefix` (the base namespace applied once on `Idml.base`) and the singular `add_page_from_idml` (subsumed by the batch `add_pages_from_idml`) are non-step members of the sixteen-method surface. It binds the placed layout `composition/compose#COMPOSE` produces into the IDML template the designer authored in InDesign: the template carries the named XML structure (the InDesign XML-ready document's tag tree), and this owner feeds content INTO that structure rather than emitting page geometry — keeping the data separate from the structure the IDML round-trip demands.

SimpleIDML is pure-Python over `zipfile`+`lxml`, so the mutation fold crosses the runtime `anyio.to_process` worker seam with the sibling `document/emit#EMIT` lxml arms, and only serialized package bytes plus picklable `IdmlFact` evidence cross back, never a live `IDMLPackage`/`_Element`. Each top-level `@use_working_copy` mutation EXTRACTS the package to a temp tree, mutates, REPACKAGES, overwrites the input file, closes the input, and returns a FRESH path-backed instance — so `_mutate` MUST thread the returned instance forward (a `BytesIO`-backed package crashes the decorator's `os.unlink(filename)`), bracket every spill and instance in one `contextlib.ExitStack`, and drain `Path(final.filename).read_bytes()` off the last instance. Each production is one per-instance `emit(self) -> RuntimeRail[ArtifactReceipt]` — the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules, mirroring `delivery/register#REGISTER` `Register.emit` — whose `_emit` mints the content key over the mutated-package bytes and returns the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case (the IDML package IS an Office-class structured-document Zip archive) directly, never a new receipt case and never a module-level batch entry beside the pipeline; the returned receipt IS the `ReceiptContributor` the runtime lane's `@drained` harvest emits.

## [01]-[INDEX]

- [01]-[INDESIGN]: the one IDML template-mutation owner — a frozen `Idml` `msgspec.Struct` binding `base: IdmlSource` opened ONCE and `steps: tuple[IdmlStep, ...]` folded over the one threaded `IDMLPackage`, the closed fourteen-case `IdmlStep` `expression.tagged_union` over SimpleIDML's VERIFIED sixteen-method `@use_working_copy` algebra (the two non-step members `prefix` + the singular `add_page_from_idml`), each case dispatched by one total `match`+`assert_never` and projecting its `StepFacts` (`sources`/`anchors`/`identifiers`) to the `of` admission gate, every `IdmlStep` factory carrying the shared `@beartype(conf=FAULT_CONF)` construction contract the sibling `exchange/metadata`/`exchange/credential` factories carry; the untrusted `.idml` template admitted ONCE through the closed `IndesignPayload` `TypedDict` (its `prefix` band refined at the `_PAYLOAD` `TypeAdapter` to the `\A\w+\Z` `IDMLPackage.prefix` pattern) + module-level `TypeAdapter`; the `is_prefixed` idempotence guard short-circuiting a re-prefix of an already-namespaced base; the eight-member `PdfCrop` Adobe enumeration whose `.value` is the `import_pdf(crop=)` token; the `IdmlFact` structural-inventory carrier the worker drains off the final instance; the per-instance `emit -> RuntimeRail[ArtifactReceipt]` production entry over the `to_process` worker `_emit` returning the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case (the IDML package IS an Office-class structured-document Zip). The `export_xml`/`export_as_tree` tagged-content EGRESS is scoped to `document/lens#LENS` (the recovered-tree read), not re-authored here. `Idml.emit` IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules — the `export/layered#LAYERED` `LayeredExport.emit` counterpart, both recorded in ARCHITECTURE `[02]-[SEAMS]` as the `ArtifactPipeline` single-production-entry seam, never a parallel module-level batch entry.

## [02]-[INDESIGN]

- Owner: `Idml` the one IDML template-mutation owner — a frozen `msgspec.Struct` binding `base: IdmlSource` (the InDesign-exported `.idml` template opened ONCE), `steps: tuple[IdmlStep, ...]` (the mutation fold threaded over the one running `IDMLPackage`), and `fact: IdmlFact | None` (the worker's threaded evidence), its `output` property projecting `fact.data`, never a per-op base re-open; `IdmlSource` the frozen row binding one bindable IDML document — `data` the package bytes, `prefix` the `IDMLPackage.prefix` namespace (an alphanumeric `\A\w+\Z` token applied before any merge to avoid reference collisions), `at` the destination XPath anchor, `only` the source sub-tree selector (defaulting `/Root`, NEVER `None` because `insert_idml`/`add_pages_from_idml` require a resolvable source xpath) — so a source is one row carrying its own bytes/prefix/anchors rather than four zipped parallel lists; `IdmlStep` the closed fourteen-case `expression.tagged_union` mutation family whose every case carries its own typed payload and whose one `facts` projection (the `StepFacts` carrier) feeds the admission gate in one total `match` — `sources` for data/prefix, `anchors` for the `_resolved` xpath check, `identifiers` for the non-empty layer/story/content-id check — collapsing the three parallel projection properties that each re-enumerated the same case family; `StepFacts` the frozen three-tuple admission projection (`sources`/`anchors`/`identifiers`) `of` folds once per step; `IdmlFact` the frozen evidence carrier the worker returns — the serialized `data` plus the `spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes` structural inventory read off the final package's introspection surface and the applied-`steps` count; `IdmlFault` the closed admission vocabulary `of` produces (`payload` the rejected `IndesignPayload` key paths, `empty_data`/`bad_prefix`/`empty_anchor`/`empty_ref` carrying the offending source/step/identifier index), distinct from the runtime `BoundaryFault` every worker provider raise converts to; `IndesignPayload` the closed `TypedDict` admitting the untrusted `template` bytes (and the base `prefix`) once through the module-level `_PAYLOAD = TypeAdapter`; `PdfCrop` the closed `StrEnum` of the VERIFIED eight-member Adobe `PDFCrop` enumeration (`CONTENT`/`CONTENT_VISIBLE`/`CONTENT_ALL`/`ART`/`PDF`/`TRIM`/`BLEED`/`MEDIA`) whose `.value` is the `import_pdf(crop=)` IDML token passed directly — never a hand-typed crop-string literal per call, never a `_CROP` dispatch table restating the enum's own values (the COLLAPSE_SCAN duplicate-entry form), and never the `CropPDF` spelling where the canonical `CROP_PDF` token is `PDFCrop` (the base `CONTENT = "CropContent"` is a distinct, dominant real-spread member beside the layer-scoped `CONTENT_VISIBLE`/`CONTENT_ALL`, never a phantom); the `SimpleIDML` `IDMLPackage` side-effect algebra (each `@use_working_copy` method returning a NEW path-backed instance) and its `spreads`/`stories`/`pages`/`font_families`/`style_groups`/`tags`/`referenced_layers`/`xml_structure`/`export_xml` introspection surface the worker-side mutation-and-read surface.
- Cases: `IdmlStep` cases — `Insert(module)` (template composition — open and prefix the `module` source, then `package.insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only))` so the sub-template's XML-tagged content lands at its destination XPath anchor, the returned fresh instance threaded into the next step) · `AddPages(pages)` (page combination — prefix each page source and fold `package.add_pages_from_idml([(part, number, at, only) …])` so the destination page count grows and the XML structure integrates each added file, the one SimpleIDML batch call) · `ImportXml(xml, at)` (XML content import — `package.import_xml(xml, at=…)` with the bytes passed directly, honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control attributes the source XML carries, so the data populates the structure without touching page geometry) · `PlacePdf(pdf, at, crop, page)` (PDF placement — spill a `composition/compose#COMPOSE`-produced `pdf`, then `package.import_pdf(uri, at=…, crop=crop.value, page_number=page)` so the chosen PDF page lands inside the named block as a linked, re-croppable PDF the designer re-links) · `SetAttributes(at, attrs)` (attribute/image rebind — `package.set_attributes(at, dict(attrs))`, the VERIFIED `href` image-resource relink mechanism — an `href` key updates the spread/story page-item resource path, an empty `href` removes the page item) · `AddNote(at, note, author)` (editorial annotation — `package.add_note(note, author, at=…)` attaches an InDesign collaboration note to the anchored element) · `MergeLayers(name)` (layer flatten — `package.merge_layers(with_name=name or None)` collapses every designmap layer into one and re-points the spread item layer references) · `RemoveContent(under)` (subtree clear — `package.remove_content(under)` recursively strips the content under the anchor, the template-reset inverse of `Insert`) · `SuffixLayers(suffix)` (layer-namespace suffix — `package.suffix_layers(suffix)` appends `suffix` to every designmap layer id, the layer-level analogue of the base `prefix` that keeps merged-in layers from colliding) · `RemoveLayer(layer_id)` (layer drop — `package.remove_layer(layer_id)` removes the designmap layer and, transitively, its guides) · `RemoveOrphanLayers()` (orphan sweep — `package.remove_orphan_layers()` drops every designmap layer no spread item references, the niladic post-merge cleanup) · `RemoveGuides(layer_id)` (guide clear — `package.remove_guides_on_layer(layer_id)` strips the guides on a layer while the layer itself stays, the narrower sibling of `RemoveLayer`) · `AddStory(story_id, element_id, element_tag)` (story creation — `package.add_story_with_content(story_id, element_id, element_tag)` mints a new story bound to an XML element and registers it in the designmap, the structure-population inverse of `RemoveContent`) · `LeafToNode(at, content_ref)` (leaf promotion — `package.xml_element_leaf_to_node(_resolved(package, at), content_ref)` converts a `Rectangle` leaf page-item into a `TextFrame` structural node so tagged content can nest beneath it) — matched by one total `match`/`case`+`assert_never`; never a sibling op per mutation and never an `if insert`/`if import` branch re-deriving the verb the case already names. The legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base: a template insert THEN a data import THEN a cover place THEN an image relink is ONE `Idml` over one threaded package rather than four re-opened round-trips.
- Auto: `_mutate` runs in the IDML worker seam: it spills `base.data` to a path-backed `tempfile.NamedTemporaryFile(delete_on_close=False)` (the `IDMLPackage` constructor opens a Zip over a real path; a `BytesIO` crashes the `@use_working_copy` `os.unlink(self.filename)` because a stream-backed `ZipFile.filename` is `None`), opens it as `idml.IDMLPackage(path)`, prefixes it through `package.prefix(base.prefix)`, and folds each `IdmlStep` through one total `match`+`assert_never` over a single `contextlib.ExitStack` that registers every spill and every returned instance for close-on-exit. Each top-level `@use_working_copy` mutation extracts to its own temp tree, mutates, repackages, overwrites the input file, closes the input, and returns a fresh path-backed `IDMLPackage`, so the fold THREADS the returned instance (`package = stack.enter_context(package.method(...))`) rather than reassigning one handle — a lost reference leaves an unclosed backing file the platform (notably Windows) cannot unlink. The `Insert` arm opens and prefixes the module then `insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only))`; `AddPages` builds the `(part, number, at, only)` batch with each `at` resolved against the destination and each `only` against its source; `ImportXml` passes the `xml` bytes straight to `import_xml`; `PlacePdf` spills the PDF, builds its `Path.as_uri()` `file:` URL, and `import_pdf(uri, crop=crop.value, page_number=page)`; `SetAttributes`/`AddNote`/`RemoveContent`/`LeafToNode` resolve their anchor (`LeafToNode` passing the resolved `at` and the unresolved `content_ref` id) and call the verified method; `MergeLayers`/`SuffixLayers`/`RemoveOrphanLayers` carry no anchor; `RemoveLayer`/`RemoveGuides`/`AddStory` pass their pre-admitted designmap/story ids straight to `remove_layer`/`remove_guides_on_layer`/`add_story_with_content` without `_resolved`, because a layer/story id keys the designmap, not the `xml_structure` tree an xpath resolves against. `_resolved` validates each XPath resolves in the live `package.xml_structure` lxml `_Element` tree through `_Element.xpath` BEFORE the mutation — an empty result raises `KeyError` so SimpleIDML never fails mid-fold on a phantom anchor, the raise crossing back to the `async_boundary` `BoundaryFault` conversion. After the fold, `_mutate` reads the full structural inventory off the final instance — `len(spreads)`/`len(stories)`/`len(pages)`/`len(font_families)`/`len(style_groups)`/`len(referenced_layers)`/`len(tags)` and the `xml_structure.iter()` node count — and drains `Path(package.filename).read_bytes()` into the `IdmlFact`.
- Receipt: `Idml._emit` returns `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` directly off the worker `IdmlFact` — an IDML package IS an Office-class structured-document Zip archive (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form — the returned receipt the `ReceiptContributor` the runtime lane's `@drained` harvest emits, exactly as `delivery/register#REGISTER` `Register.emit` returns its `ArtifactReceipt.Register`. The rich structural evidence (`spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes`/`steps`) rides the worker `IdmlFact` a consumer reads through the mutation, not the flat `Office` scalar — a structure-bearing receipt case is a `core/receipt#RECEIPT` growth concern, so IDML hand-off mints only the `Office` byte-count arity it owns. The content key is content-addressed off the mutated-package bytes through `ContentIdentity.of`, so the same key the receipt carries seeds the `core/plan#PLAN` `ArtifactWork` node and the runtime reuse-fabric elides a duplicate.
- Growth: a future SimpleIDML mutation outside today's sixteen-method surface (a master-spread apply, a spread reorder, a hyperlink insert) is one `IdmlStep` case plus one `_mutate` arm plus one `facts` arm projecting its `StepFacts` (`sources`/`anchors`/`identifiers`) over the existing verified `IDMLPackage` algebra — never a re-implemented IDML Zip/XML serializer; a new source attribute (a target spread, a style override) is one field on `IdmlSource` threaded into the consuming arm; a new structural fact (a guide count, a hyperlink tally read off `xml_structure`) is one field on `IdmlFact`; a new admission cause is one `IdmlFault` case; a new untrusted ingress blob is one `IndesignPayload` band line; a new crop mode is one `PdfCrop` token whose `.value` threads into `import_pdf(crop=)`. The held-package fold absorbs an arbitrary step sequence over one running package; a second deliverable is one more `ArtifactWork` node the `core/plan#PLAN` `ArtifactPipeline` schedules. Zero new surface.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
import re
from contextlib import ExitStack
from enum import StrEnum
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Annotated, Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

from beartype import beartype
from expression import Error, Ok, Result, case, tag, tagged_union
from msgspec import Struct
from pydantic import StringConstraints, TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.faults import FAULT_CONF, RuntimeRail, async_boundary

from artifacts.core.plan import Admission, ArtifactWork
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
    CONTENT = "CropContent"  # CROP_CONTENT — the content bounding box, layer-visibility-agnostic
    CONTENT_VISIBLE = "CropContentVisibleLayers"  # CROP_CONTENT_VISIBLE_LAYERS — the `import_pdf` default
    CONTENT_ALL = "CropContentAllLayers"  # CROP_CONTENT_ALL_LAYERS
    ART = "CropArt"  # CROP_ART — the author-defined placeable artwork box
    PDF = "PDFCrop"  # CROP_PDF — the Acrobat-displayed crop box
    TRIM = "CropTrim"  # CROP_TRIM
    BLEED = "CropBleed"  # CROP_BLEED
    MEDIA = "CropMedia"  # CROP_MEDIA — the original physical paper size


# --- [CONSTANTS] ------------------------------------------------------------------------

_KIND: Final = "idml"  # the `ContentIdentity.of` kind tag minted off the mutated-package bytes
_ROOT: Final = "/Root"  # the IDML tag-tree root, the default destination/source anchor
_BASE_PREFIX: Final = "Base"  # the default base namespace prefix when the payload omits one
_PREFIX: Final = re.compile(r"\A\w+\Z")  # the alphanumeric word-char constraint `IDMLPackage.prefix` enforces (its `re.match(r"^\w+$")`)

# --- [MODELS] ---------------------------------------------------------------------------


class IdmlSource(Struct, frozen=True):
    data: bytes
    prefix: str
    at: str = _ROOT  # destination XPath anchor (where this source's content lands in the running package)
    only: str = _ROOT  # source sub-tree selector; NEVER None — `insert_idml`/`add_pages_from_idml` require a resolvable xpath


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
        "insert",
        "add_pages",
        "import_xml",
        "place_pdf",
        "set_attributes",
        "add_note",
        "merge_layers",
        "remove_content",
        "suffix_layers",
        "remove_layer",
        "remove_orphan_layers",
        "remove_guides",
        "add_story",
        "leaf_to_node",
    ] = tag()
    insert: IdmlSource = case()
    add_pages: tuple[tuple[IdmlSource, int], ...] = case()
    import_xml: tuple[bytes, str] = case()
    place_pdf: tuple[bytes, str, PdfCrop, int] = case()
    set_attributes: tuple[str, frozendict[str, str]] = case()
    add_note: tuple[str, str, str] = case()
    merge_layers: str = case()
    remove_content: str = case()
    suffix_layers: str = case()  # designmap layer-id namespace suffix, the layer-level analogue of the base `prefix`
    remove_layer: str = case()  # designmap layer `Self` id; drops the layer and its guides
    remove_orphan_layers: None = case()  # niladic — strips every layer no spread references
    remove_guides: str = case()  # layer id whose guides clear while the layer itself stays
    add_story: tuple[str, str, str] = case()  # (story_id, xml_element_id, xml_element_tag) — a new story bound to an XML element
    leaf_to_node: tuple[str, str] = case()  # (destination xpath anchor, xml content ref) — Rectangle leaf promoted to a TextFrame node

    @classmethod
    @beartype(conf=FAULT_CONF)
    def Insert(cls, module: IdmlSource, /) -> Self:
        return cls(insert=module)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def AddPages(cls, pages: tuple[tuple[IdmlSource, int], ...], /) -> Self:
        return cls(add_pages=pages)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def ImportXml(cls, xml: bytes, at: str = _ROOT, /) -> Self:
        return cls(import_xml=(xml, at))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def PlacePdf(cls, pdf: bytes, at: str, crop: PdfCrop = PdfCrop.CONTENT_VISIBLE, page: int = 1, /) -> Self:
        return cls(place_pdf=(pdf, at, crop, page))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def SetAttributes(cls, at: str, attrs: frozendict[str, str], /) -> Self:
        return cls(set_attributes=(at, attrs))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def AddNote(cls, at: str, note: str, author: str, /) -> Self:
        return cls(add_note=(at, note, author))

    @classmethod
    @beartype(conf=FAULT_CONF)
    def MergeLayers(cls, name: str = "", /) -> Self:
        return cls(merge_layers=name)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def RemoveContent(cls, under: str, /) -> Self:
        return cls(remove_content=under)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def SuffixLayers(cls, suffix: str, /) -> Self:
        return cls(suffix_layers=suffix)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def RemoveLayer(cls, layer_id: str, /) -> Self:
        return cls(remove_layer=layer_id)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def RemoveOrphanLayers(cls) -> Self:
        return cls(remove_orphan_layers=None)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def RemoveGuides(cls, layer_id: str, /) -> Self:
        return cls(remove_guides=layer_id)

    @classmethod
    @beartype(conf=FAULT_CONF)
    def AddStory(cls, story_id: str, element_id: str, element_tag: str, /) -> Self:
        return cls(add_story=(story_id, element_id, element_tag))

    @classmethod
    @beartype(conf=FAULT_CONF)
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
                IdmlStep(tag="import_xml", import_xml=(_, at))
                | IdmlStep(tag="place_pdf", place_pdf=(_, at, _, _))
                | IdmlStep(tag="set_attributes", set_attributes=(at, _))
                | IdmlStep(tag="add_note", add_note=(at, _, _))
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
    fonts: int = 0  # len(package.font_families)
    styles: int = 0  # len(package.style_groups)
    layers: int = 0  # len(package.referenced_layers)
    tags: int = 0  # len(package.tags)
    nodes: int = 0  # xml_structure.iter() node count
    steps: int = 0


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IdmlFault:
    # the closed ADMISSION vocabulary `of` produces; a worker provider raise (a malformed `.idml` Zip, an
    # `lxml.etree.XMLSyntaxError`, the `_resolved` missing-anchor `KeyError`, an `IDMLPackage.prefix`
    # `BaseException`) converts to the runtime `BoundaryFault` at the `async_boundary`, never this vocabulary.
    tag: Literal["payload", "empty_data", "bad_prefix", "empty_anchor", "empty_ref"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `IndesignPayload` key paths from the `TypeAdapter` miss
    empty_data: int = case()  # source-row index carrying empty `.idml` payload bytes
    bad_prefix: int = case()  # source-row index whose prefix is empty or fails `\A\w+\Z`
    empty_anchor: int = case()  # step-anchor index carrying an empty `at` destination or `only` source xpath
    empty_ref: int = case()  # step-identifier index carrying an empty layer/story/content id


# --- [BOUNDARIES] -----------------------------------------------------------------------


class IndesignPayload(TypedDict, closed=True):
    template: Required[ReadOnly[bytes]]  # the untrusted InDesign-exported `.idml` the steps mutate
    # the base namespace prefix refined at the `TypeAdapter` to the alphanumeric `\A\w+\Z` pattern `IDMLPackage.prefix`
    # itself enforces (its `re.match(r"^\w+$")`) — a non-alphanumeric ingress prefix is an `IdmlFault.payload` at admission,
    # not a worker-time `BaseException`, the refined-admission parity the sibling `document/emit`/`export/layered` payloads carry.
    prefix: NotRequired[ReadOnly[Annotated[str, StringConstraints(pattern=r"\A\w+\Z")]]]  # default `_BASE_PREFIX`


_PAYLOAD = TypeAdapter(IndesignPayload)

# --- [SERVICES] -------------------------------------------------------------------------


class Idml(Struct, frozen=True):
    base: IdmlSource
    steps: tuple[IdmlStep, ...]

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
            Error(IdmlFault(empty_data=bad_data))
            if bad_data is not None
            else Error(IdmlFault(bad_prefix=bad_prefix))
            if bad_prefix is not None
            else Error(IdmlFault(empty_anchor=bad_anchor))
            if bad_anchor is not None
            else Error(IdmlFault(empty_ref=bad_ref))
            if bad_ref is not None
            else Ok(cls(base=base, steps=steps))
        )

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen spec minted PRE-RUN — never a key over the produced package bytes.
        return ContentIdentity.of("idml", self, policy=CANONICAL_POLICY)

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the renamed private thunk; the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        return await async_boundary("export.indesign", self._produced)

    async def _produced(self) -> ArtifactReceipt:
        # the lxml-bound mutation crosses the IDML worker seam and only the serialized `IdmlFact` returns; the
        # content key is minted over the mutated-package bytes and the flat `Office` case returned directly.
        fact = (await LanePolicy.offload(_mutate, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_idml_raise)
        key = self._key
        return ArtifactReceipt.Office(key, len(fact.data))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _idml_raise(fault: object) -> object:
    raise ValueError(str(fault))


def _mutate(plan: Idml) -> IdmlFact:  # simple_idml -> lxml runs behind the worker boundary
    with ExitStack() as stack:
        opened = idml.IDMLPackage(str(_spill(stack, plan.base.data, ".idml")))
        # `is_prefixed` short-circuits an already-namespaced base: `prefix` is a `@use_working_copy` mutation that
        # re-extracts+repackages, so re-prefixing an already-prefixed template both wastes the round-trip and double-namespaces
        # every `Self` id. An un-prefixed base takes the fresh prefixed instance; an already-prefixed one is entered as-is.
        package = stack.enter_context(opened if opened.is_prefixed(plan.base.prefix) else opened.prefix(plan.base.prefix))
        for step in plan.steps:
            match step:
                case IdmlStep(tag="insert", insert=module):
                    part = stack.enter_context(idml.IDMLPackage(str(_spill(stack, module.data, ".idml"))).prefix(module.prefix))
                    package = stack.enter_context(package.insert_idml(part, at=_resolved(package, module.at), only=_resolved(part, module.only)))
                case IdmlStep(tag="add_pages", add_pages=pages):
                    parts = [stack.enter_context(idml.IDMLPackage(str(_spill(stack, src.data, ".idml"))).prefix(src.prefix)) for src, _ in pages]
                    specs = [
                        (part, number, _resolved(package, src.at), _resolved(part, src.only))
                        for part, (src, number) in zip(parts, pages, strict=True)
                    ]
                    package = stack.enter_context(package.add_pages_from_idml(specs))
                case IdmlStep(tag="import_xml", import_xml=(xml, at)):
                    package = stack.enter_context(package.import_xml(xml, at=_resolved(package, at)))
                case IdmlStep(tag="place_pdf", place_pdf=(pdf, at, crop, page)):
                    package = stack.enter_context(
                        package.import_pdf(_spill(stack, pdf, ".pdf").as_uri(), at=_resolved(package, at), crop=crop.value, page_number=page)
                    )
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
# every IDML mutation offload threads this one bound, so N concurrent `emit` calls share a fixed
# subprocess pool instead of fanning out at the per-loop process-limiter default — the worker bound
# `export/layered#LAYERED`'s `ORA` worker carries. The lxml/SimpleIDML worker is heavy (zip extract +
# repackage + designmap rewrite per step), so the bound stays small.
```
