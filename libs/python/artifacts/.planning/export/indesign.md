# [PY_ARTIFACTS_INDESIGN]

`Idml` owns the IDML template-mutation hand-off — the editable InDesign deliverable authored by MUTATING an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is ONE frozen `msgspec.Struct` binding a `base: IdmlSource` template (admitted ONCE through the closed `IndesignPayload` `TypedDict` + module-level `TypeAdapter` at `Idml.of`) and a `steps: tuple[IdmlStep, ...]` fold threaded over the one running `IDMLPackage` and drained ONCE into a picklable `IdmlFact`, never a per-op base re-open. `IdmlStep` is the closed `expression.tagged_union` over SimpleIDML's verified `@use_working_copy` mutation algebra — the FULL step-eligible surface, each case a typed payload dispatched by one total `match`; only `prefix` (the base namespace applied once on `Idml.base`) and the singular `add_page_from_idml` (subsumed by the batch `add_pages_from_idml`) are the non-step members of the sixteen-method surface. It binds the placed layout `composition/compose#COMPOSE` produces into the template the designer authored: the IDML carries the named XML tag tree, and this owner feeds content INTO that structure rather than emitting page geometry, keeping data separate from the structure the round-trip demands.

SimpleIDML is pure-Python over `zipfile`+`lxml`, so the mutation fold crosses the runtime `to_process` worker seam beside the sibling `document/emit#EMIT` lxml arms, and only serialized package bytes plus the picklable `IdmlFact` cross back, never a live `IDMLPackage`/`_Element`. Each `@use_working_copy` mutation extracts to a temp tree, mutates, repackages, overwrites the input file, closes it, and returns a FRESH path-backed instance — so `_mutate` MUST thread the returned instance forward (a `BytesIO`-backed package crashes the decorator's `os.unlink(filename)`), bracket every spill and instance in one `ExitStack`, and drain `Path(final.filename).read_bytes()` off the last instance. One per-instance `emit -> RuntimeRail[ArtifactReceipt]` IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules (the `export/layered#LAYERED` `LayeredExport.emit` counterpart), whose `_emit` mints the content key over the mutated-package bytes and returns the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case directly — an IDML package IS an Office-class structured-document Zip.

## [01]-[INDEX]

- [01]-[INDESIGN]: the one IDML template-mutation owner — a frozen `Idml` folding `steps: tuple[IdmlStep, ...]` over one running `IDMLPackage` through SimpleIDML's verified `@use_working_copy` algebra, drained across the `to_process` worker into an `ArtifactReceipt.Office` production the `ArtifactPipeline` schedules.

## [02]-[INDESIGN]

- Owner: `Idml` a frozen `msgspec.Struct` binding `base: IdmlSource` (the `.idml` template opened ONCE), `steps: tuple[IdmlStep, ...]` (the mutation fold over the one running `IDMLPackage`), and `fact: IdmlFact | None` (the worker's threaded evidence), never a per-op base re-open. `IdmlSource` is one row carrying its own bytes/`prefix`/anchors — `at` the destination XPath, `only` the source sub-tree selector (defaulting `/Root`, NEVER `None` because `insert_idml`/`add_pages_from_idml` require a resolvable xpath) — rather than four zipped parallel lists. `IdmlStep` is the closed mutation family whose one `facts` projection feeds the admission gate: `sources` for the empty-data/prefix checks, `anchors` for the `_resolved` xpath gate over both `at` and `only`, `identifiers` for the non-empty layer/story/content-id check. `PdfCrop` is the verified eight-member Adobe `PDFCrop` enumeration whose `.value` is the `import_pdf(crop=)` token passed directly — never a `_CROP` dispatch table restating the enum's own values.
- Cases: `IdmlStep` cases fold over the one held package — `Insert` (`insert_idml` at a destination anchor), `AddPages` (the batch `add_pages_from_idml`), `ImportXml` (`import_xml`, honoring the source's content-control attributes), `PlacePdf` (`import_pdf` carrying the `PdfCrop` mode and page), `SetAttributes` (`set_attributes`, the verified `href` image-relink — an empty `href` removes the page item), `AddNote` (`add_note`), `MergeLayers`/`SuffixLayers`/`RemoveLayer`/`RemoveOrphanLayers`/`RemoveGuides` (the designmap layer algebra), `RemoveContent` (the template-reset inverse of `Insert`), `AddStory` (`add_story_with_content`), `LeafToNode` (a `Rectangle` leaf promoted to a `TextFrame` node so tagged content nests) — dispatched by one total `match`; the legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base.
- Auto: `_mutate` runs the worker seam — it spills `base.data` to a path-backed temp file (the `IDMLPackage` constructor opens a Zip over a real path; a `BytesIO` crashes the `@use_working_copy` `os.unlink` because a stream-backed `ZipFile.filename` is `None`), opens and prefixes it, and folds each step over one `ExitStack` that registers every spill and returned instance for close-on-exit. Because each mutation returns a fresh path-backed instance, the fold THREADS the returned instance forward rather than reassigning one handle — a lost reference leaves an unclosed backing file the platform (notably Windows) cannot unlink. `_resolved` validates each XPath against the live `package.xml_structure` BEFORE the mutation so SimpleIDML never fails mid-fold on a phantom anchor, the `KeyError` crossing to the `async_boundary`; layer/story ids skip `_resolved` because they key the designmap, not the xpath tree. After the fold `_mutate` reads the structural inventory off the final instance and drains its bytes into the `IdmlFact`.
- Output: `IdmlFact` is the picklable evidence carrier — the serialized `data` plus the `spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes` structural inventory read off the final package and the applied-`steps` count.
- Receipt: `Idml._emit` returns `ArtifactReceipt.Office(key, byte_count)` directly off the `IdmlFact` — an IDML package IS an Office-class structured-document Zip (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form. Rich structural evidence rides the `IdmlFact` a consumer reads, not the flat `Office` scalar — a structure-bearing receipt case is a `core/receipt#RECEIPT` growth concern. `ContentIdentity.of` mints the key over the mutated-package bytes, so the same key seeds the `core/plan#PLAN` `ArtifactWork` node and the reuse fabric elides a duplicate.
- Growth: a future SimpleIDML mutation outside today's surface (a master-spread apply, a hyperlink insert) is one `IdmlStep` case plus one `_mutate` arm plus one `facts` arm over the existing verified algebra — never a re-implemented IDML Zip/XML serializer; a new source attribute is one `IdmlSource` field; a new structural fact is one `IdmlFact` field; a new admission cause is one `IdmlFault` case; a new untrusted ingress is one `IndesignPayload` band line; a new crop mode is one `PdfCrop` token. A second deliverable is one more `ArtifactWork` node the `ArtifactPipeline` schedules. Zero new surface.
- Boundary: a per-op base re-open where the held-package fold threads one running `IDMLPackage`; four zipped `datas`/`prefixes`/`ats`/`onlys` lists where one `IdmlSource` row carries its own; a `StrEnum` keyed against an erased `dict[str, object]` where the typed `IdmlStep` payload dispatches; a `_CROP` table restating `PdfCrop`'s own values; a `BytesIO`-backed package the `@use_working_copy` `os.unlink` crashes on; a parallel `Idml`/`Indesign` receipt case where the `Office` case owns the Zip are the deleted forms. `export_xml`/`export_as_tree` tagged-content EGRESS stays `document/lens#LENS` (the recovered-tree read), not re-authored here.

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
    # `.value` is the IDML token `import_pdf(crop=)` writes verbatim. `PDF` is the canonical `PDFCrop` token (the
    # initialism breaks the `Crop` prefix), NOT the rarer `CropPDF` spelling; `CONTENT` is the base `CropContent` member.
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
    # one carrier so the admission gate reads one projection per step, not three parallel ones.
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
        # BOTH `at` and `only` ride `anchors` (each `_resolved` against its own tree; an empty `only` never reaches
        # `insert_idml`'s `xpath(only)[0]`); layer/story/content ids ride `identifiers`, non-empty-checked WITHOUT
        # `_resolved` because they key the designmap and story files, not the `xml_structure` tree an xpath resolves against.
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
    # the closed ADMISSION vocabulary `of` produces; a worker provider raise converts to the runtime
    # `BoundaryFault` at the `async_boundary`, never this vocabulary.
    tag: Literal["payload", "empty_data", "bad_prefix", "empty_anchor", "empty_ref"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `IndesignPayload` key paths from the `TypeAdapter` miss
    empty_data: int = case()  # source-row index carrying empty `.idml` payload bytes
    bad_prefix: int = case()  # source-row index whose prefix is empty or fails `\A\w+\Z`
    empty_anchor: int = case()  # step-anchor index carrying an empty `at` destination or `only` source xpath
    empty_ref: int = case()  # step-identifier index carrying an empty layer/story/content id


# --- [BOUNDARIES] -----------------------------------------------------------------------


class IndesignPayload(TypedDict, closed=True):
    template: Required[ReadOnly[bytes]]  # the untrusted InDesign-exported `.idml` the steps mutate
    # the prefix refined at the `TypeAdapter` to the `\A\w+\Z` pattern `IDMLPackage.prefix` enforces, so a
    # non-alphanumeric ingress prefix is an `IdmlFault.payload` at admission, not a worker-time `BaseException`.
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
        # the terminal receipt threads the PRE-RUN key (receipt.slot == node.key).
        return await async_boundary("export.indesign", self._produced)

    async def _produced(self) -> ArtifactReceipt:
        # the mutation crosses the IDML worker seam and only the serialized `IdmlFact` returns.
        fact = (await LanePolicy.offload(_mutate, self, modality=Modality.PROCESS, retry=RetryClass.OCCT)).default_with(_idml_raise)
        key = self._key
        return ArtifactReceipt.Office(key, len(fact.data))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _idml_raise(fault: object) -> object:
    raise ValueError(str(fault))


def _mutate(plan: Idml) -> IdmlFact:  # simple_idml -> lxml runs behind the worker boundary
    with ExitStack() as stack:
        opened = idml.IDMLPackage(str(_spill(stack, plan.base.data, ".idml")))
        # `is_prefixed` short-circuits an already-namespaced base: `prefix` re-extracts+repackages, so re-prefixing
        # both wastes the round-trip and double-namespaces every `Self` id.
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
    # the `IDMLPackage` constructor opens a path-backed Zip and `@use_working_copy` `os.unlink`s `self.filename`, so a
    # stream-backed package (filename `None`) crashes; spill to a temp file kept past `close()`, unlinked on stack exit.
    handle = stack.enter_context(NamedTemporaryFile(suffix=suffix, delete_on_close=False))
    handle.write(data)
    handle.close()
    return Path(handle.name)


def _resolved(package: "IDMLPackage", xpath: str, /) -> str:
    # one guard for BOTH `at` and `only`: an empty `xpath` result is the missing-anchor signal raised before the
    # mutation so SimpleIDML never fails mid-fold, the `async_boundary` converting the `KeyError` to a `BoundaryFault`.
    if not package.xml_structure.xpath(xpath):
        raise KeyError(xpath)
    return xpath


# --- [COMPOSITION] ----------------------------------------------------------------------
# every IDML mutation offload threads one bound (the `export/layered#LAYERED` `ORA` worker counterpart), so N
# concurrent `emit` calls share a fixed subprocess pool; the lxml/SimpleIDML worker is heavy, so the bound stays small.
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
