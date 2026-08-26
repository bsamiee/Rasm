# [PY_ARTIFACTS_INDESIGN]

`Idml` owns the IDML template-mutation hand-off — the editable InDesign deliverable authored by mutating an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is one frozen `msgspec.Struct` binding a `base: IdmlSource` template admitted through `IndesignPayload` and a `steps: tuple[IdmlStep, ...]` fold threaded over one running `IDMLPackage` and drained once into `IdmlFact`. `IdmlStep` is the closed `expression.tagged_union` over SimpleIDML's step-eligible `@use_working_copy` algebra; `prefix` applies once to `Idml.base`, and batch `add_pages_from_idml` subsumes singular page insertion. IDML carries the named XML tag tree, so this owner feeds content into designer-authored structure instead of emitting page geometry.

## [01]-[INDEX]

## [02]-[INDESIGN]

- Owner: `Idml` binds `base: IdmlSource`, `steps: tuple[IdmlStep, ...]`, and `lane: LanePolicy`. `IdmlSource` carries bytes, prefix, destination `at`, and source selector `only`. `IdmlStep.facts` feeds one accumulating admission: `sources` carry template data and prefixes, `blobs` carry XML/PDF bodies, `batches` carry plural cardinality, `pages` carry positive page indices, `anchors` carry both XPath axes, and `identifiers` carry layer/story/content ids and tags — every axis scans whole and every casualty reduces through the associative `IdmlFault.combined` monoid, so one refusal names every offending index across all seven axes. `PdfCrop.value` is the verified `import_pdf(crop=)` token.
- Cases: `IdmlStep` cases fold over the one held package — `insert` (`insert_idml` at a destination anchor), `add_pages` (the batch `add_pages_from_idml`), `import_xml` (honoring the source's content-control attributes), `place_pdf` (`import_pdf` carrying the `PdfCrop` mode and page), `set_attributes` (the verified `href` image-relink — an empty `href` removes the page item), `add_note`, `merge_layers`/`suffix_layers`/`remove_layer`/`remove_orphan_layers`/`remove_guides` (the designmap layer algebra), `remove_content` (the template-reset inverse of `insert`), `add_story` (`add_story_with_content`), `leaf_to_node` (a `Rectangle` leaf promoted to a `TextFrame` node so tagged content nests) — dispatched by one total `match`; the legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base.
- Auto: `_mutate` runs the worker boundary — it spills `base.data` to a path-backed temporary file, opens and prefixes it, and folds `Block.of_seq(plan.steps)` over one `ExitStack` that registers every spill and returned instance for close-on-exit. Each mutation returns a fresh path-backed instance, so the fold threads that successor into the next step. Nested `spill`, `resolved`, and `apply` kernels own the platform statements: file lifetime, live-tree XPath admission, and provider mutation dispatch never escape as module-level helpers. `_resolved` validates each XPath against the current `package.xml_structure` before its mutation; layer and story ids skip XPath admission because they key the design map and story files. `_mutate` drains bytes and structural inventory from the terminal instance.
- Output: `IdmlFact` is the picklable evidence carrier — the serialized `data` plus the `spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes` structural inventory read off the final package and the applied-`steps` count.
- Growth: a SimpleIDML mutation is one `IdmlStep` case plus one `apply` arm plus one `facts` arm over the verified algebra; a source attribute is one `IdmlSource` field; a structural fact is one required `IdmlFact` field; an admission cause is one `IdmlFault` case beside one casualty comprehension the monoid already reduces; an untrusted ingress is one `IndesignPayload` band line; a crop mode is one `PdfCrop` token. Another deliverable is one `ArtifactWork` node the `ArtifactPipeline` schedules.
- Boundary: per-operation base reopen, parallel source lists, erased dictionaries, forwarding case constructors, crop dispatch tables, `BytesIO` package mutation, class-qualified offload, raise bridges, and parallel IDML outputs are rejected. `export_xml`/`export_as_tree` tagged-content egress stays `document/lens#LENS`.

```python
# --- [IMPORTS] --------------------------------------------------------------------------
import re
from contextlib import ExitStack
from enum import StrEnum
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Annotated, Final, Literal, NotRequired, ReadOnly, Required, Self, TypedDict, Unpack, assert_never

from builtins import frozendict
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct
from msgspec.msgpack import Encoder
from pydantic import StringConstraints, TypeAdapter, ValidationError

from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeResult

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN
from rasm.artifacts.core.plan import Admission, ArtifactWork

lazy from simple_idml import idml

if TYPE_CHECKING:
    from simple_idml.idml import IDMLPackage

# --- [TYPES] ----------------------------------------------------------------------------


class PdfCrop(StrEnum):
    CONTENT = "CropContent"
    CONTENT_VISIBLE = "CropContentVisibleLayers"
    CONTENT_ALL = "CropContentAllLayers"
    ART = "CropArt"
    PDF = "PDFCrop"
    TRIM = "CropTrim"
    BLEED = "CropBleed"
    MEDIA = "CropMedia"


# --- [CONSTANTS] ------------------------------------------------------------------------

_KIND: Final = "idml"
_ROOT: Final = "/Root"
_BASE_PREFIX: Final = "Base"
_PREFIX: Final = re.compile(r"\A\w+\Z")

# --- [MODELS] ---------------------------------------------------------------------------


class IdmlSource(Struct, frozen=True):
    data: bytes
    prefix: str
    at: str = _ROOT
    only: str = _ROOT


class StepFacts(Struct, frozen=True):
    sources: tuple[IdmlSource, ...] = ()
    blobs: tuple[bytes, ...] = ()
    batches: tuple[int, ...] = ()
    pages: tuple[int, ...] = ()
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
    suffix_layers: str = case()
    remove_layer: str = case()
    remove_orphan_layers: None = case()
    remove_guides: str = case()
    add_story: tuple[str, str, str] = case()
    leaf_to_node: tuple[str, str] = case()

    @property
    def facts(self) -> StepFacts:
        match self:
            case IdmlStep(tag="insert", insert=module):
                return StepFacts(sources=(module,), anchors=(module.at, module.only))
            case IdmlStep(tag="add_pages", add_pages=pages):
                return StepFacts(
                    sources=tuple(src for src, _ in pages),
                    batches=(len(pages),),
                    pages=tuple(number for _, number in pages),
                    anchors=tuple(a for src, _ in pages for a in (src.at, src.only)),
                )
            case IdmlStep(tag="import_xml", import_xml=(xml, at)):
                return StepFacts(blobs=(xml,), anchors=(at,))
            case IdmlStep(tag="place_pdf", place_pdf=(pdf, at, _, page)):
                return StepFacts(blobs=(pdf,), pages=(page,), anchors=(at,))
            case IdmlStep(tag="set_attributes", set_attributes=(at, attrs)):
                return StepFacts(batches=(len(attrs),), anchors=(at,))
            case IdmlStep(tag="add_note", add_note=(at, _, _)) | IdmlStep(tag="remove_content", remove_content=at):
                return StepFacts(anchors=(at,))
            case IdmlStep(tag="leaf_to_node", leaf_to_node=(at, content_ref)):
                return StepFacts(anchors=(at,), identifiers=(content_ref,))
            case IdmlStep(tag="remove_layer", remove_layer=ref) | IdmlStep(tag="remove_guides", remove_guides=ref):
                return StepFacts(identifiers=(ref,))
            case IdmlStep(tag="suffix_layers", suffix_layers=suffix):
                return StepFacts(identifiers=(suffix,))
            case IdmlStep(tag="add_story", add_story=(story_id, element_id, element_tag)):
                return StepFacts(identifiers=(story_id, element_id, element_tag))
            case IdmlStep(tag="merge_layers") | IdmlStep(tag="remove_orphan_layers"):
                return StepFacts()
            case _ as unreachable:
                assert_never(unreachable)


class IdmlFact(Struct, frozen=True):
    data: bytes
    spreads: int
    stories: int
    pages: int
    fonts: int
    styles: int
    layers: int
    tags: int
    nodes: int
    steps: int


# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IdmlFault:
    tag: Literal[
        "payload", "empty_data", "empty_blob", "empty_batch", "invalid_page", "bad_prefix", "empty_anchor", "empty_ref", "aggregate"
    ] = tag()
    payload: tuple[str, ...] = case()
    empty_data: int = case()
    empty_blob: int = case()
    empty_batch: int = case()
    invalid_page: int = case()
    bad_prefix: int = case()
    empty_anchor: int = case()
    empty_ref: int = case()
    aggregate: tuple["IdmlFault", ...] = case()

    @staticmethod
    def _members(fault: "IdmlFault", /) -> tuple["IdmlFault", ...]:
        return fault.aggregate if fault.tag == "aggregate" else (fault,)

    @staticmethod
    def combined(left: "IdmlFault", right: "IdmlFault", /) -> "IdmlFault":
        return IdmlFault(aggregate=(*IdmlFault._members(left), *IdmlFault._members(right)))


# --- [BOUNDARIES] -----------------------------------------------------------------------


class IndesignPayload(TypedDict, closed=True):
    template: Required[ReadOnly[bytes]]
    prefix: NotRequired[ReadOnly[Annotated[str, StringConstraints(pattern=r"\A\w+\Z")]]]


_PAYLOAD = TypeAdapter(IndesignPayload)

# --- [SERVICES] -------------------------------------------------------------------------


class Idml(Struct, frozen=True):
    base: IdmlSource
    steps: tuple[IdmlStep, ...]
    lane: LanePolicy

    @classmethod
    def of(cls, steps: tuple[IdmlStep, ...], /, *, lane: LanePolicy, **raw: Unpack[IndesignPayload]) -> Result[Self, IdmlFault]:
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(IdmlFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        base = IdmlSource(data=payload["template"], prefix=payload.get("prefix", _BASE_PREFIX))
        projected = tuple(step.facts for step in steps)
        sources = (base, *(src for facts in projected for src in facts.sources))
        blobs = tuple(blob for facts in projected for blob in facts.blobs)
        batches = tuple(size for facts in projected for size in facts.batches)
        pages = tuple(page for facts in projected for page in facts.pages)
        anchors = tuple(anchor for facts in projected for anchor in facts.anchors)
        identifiers = tuple(ref for facts in projected for ref in facts.identifiers)
        casualties = Block.of_seq((
            *(IdmlFault(empty_data=index) for index, src in enumerate(sources) if not src.data),
            *(IdmlFault(empty_blob=index) for index, blob in enumerate(blobs) if not blob),
            *(IdmlFault(empty_batch=index) for index, size in enumerate(batches) if size == 0),
            *(IdmlFault(invalid_page=index) for index, page in enumerate(pages) if page <= 0),
            *(IdmlFault(bad_prefix=index) for index, src in enumerate(sources) if not _PREFIX.match(src.prefix)),
            *(IdmlFault(empty_anchor=index) for index, anchor in enumerate(anchors) if not anchor),
            *(IdmlFault(empty_ref=index) for index, ref in enumerate(identifiers) if not ref),
        ))
        return Ok(cls(base=base, steps=steps, lane=lane)) if casualties.is_empty() else Error(casualties.reduce(IdmlFault.combined))

    def emit(self, /) -> ArtifactWork[IdmlFact]:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.key(_KIND, _CANON.encode((self.base, self.steps)))

    async def _emit(self) -> RuntimeResult[IdmlFact]:
        crossed = await self.lane.offload(Kernel.of(_mutate, KernelTrait.HOSTILE), self)
        match crossed:
            case Result(tag="ok", ok=fact):
                Metrics.record({BYTE_VOLUME: float(len(fact.data))}, domain=DOMAIN, kind="office", scope=self.lane.scope)
                return Ok(fact)
            case refused:
                return Error(refused.error)


# --- [OPERATIONS] -----------------------------------------------------------------------


def _canonized(raw: object, /) -> object:
    match raw:
        case IdmlStep() as step:
            return (step.tag, getattr(step, step.tag))
        case frozendict() as row:
            return dict(row)
        case _:
            raise NotImplementedError(type(raw).__name__)


_CANON: Final[Encoder] = Encoder(order="deterministic", enc_hook=_canonized)


def _mutate(plan: Idml) -> IdmlFact:
    with ExitStack() as stack:
        def spill(data: bytes, suffix: str, /) -> Path:
            handle = stack.enter_context(NamedTemporaryFile(suffix=suffix, delete_on_close=False))
            handle.write(data)
            handle.close()
            return Path(handle.name)

        def resolved(package: "IDMLPackage", xpath: str, /) -> str:
            if not package.xml_structure.xpath(xpath):
                raise KeyError(xpath)
            return xpath

        def apply(package: "IDMLPackage", step: IdmlStep, /) -> "IDMLPackage":
            match step:
                case IdmlStep(tag="insert", insert=module):
                    part = stack.enter_context(idml.IDMLPackage(str(spill(module.data, ".idml"))).prefix(module.prefix))
                    return stack.enter_context(package.insert_idml(part, at=resolved(package, module.at), only=resolved(part, module.only)))
                case IdmlStep(tag="add_pages", add_pages=pages):
                    parts = tuple(stack.enter_context(idml.IDMLPackage(str(spill(src.data, ".idml"))).prefix(src.prefix)) for src, _ in pages)
                    specs = tuple(
                        (part, number, resolved(package, src.at), resolved(part, src.only))
                        for part, (src, number) in zip(parts, pages, strict=True)
                    )
                    return stack.enter_context(package.add_pages_from_idml(specs))
                case IdmlStep(tag="import_xml", import_xml=(xml, at)):
                    return stack.enter_context(package.import_xml(xml, at=resolved(package, at)))
                case IdmlStep(tag="place_pdf", place_pdf=(pdf, at, crop, page)):
                    return stack.enter_context(
                        package.import_pdf(spill(pdf, ".pdf").as_uri(), at=resolved(package, at), crop=crop.value, page_number=page)
                    )
                case IdmlStep(tag="set_attributes", set_attributes=(at, attrs)):
                    return stack.enter_context(package.set_attributes(resolved(package, at), dict(attrs)))
                case IdmlStep(tag="add_note", add_note=(at, note, author)):
                    return stack.enter_context(package.add_note(note, author, at=resolved(package, at)))
                case IdmlStep(tag="merge_layers", merge_layers=name):
                    return stack.enter_context(package.merge_layers(with_name=name or None))
                case IdmlStep(tag="remove_content", remove_content=under):
                    return stack.enter_context(package.remove_content(resolved(package, under)))
                case IdmlStep(tag="suffix_layers", suffix_layers=suffix):
                    return stack.enter_context(package.suffix_layers(suffix))
                case IdmlStep(tag="remove_layer", remove_layer=layer_id):
                    return stack.enter_context(package.remove_layer(layer_id))
                case IdmlStep(tag="remove_orphan_layers"):
                    return stack.enter_context(package.remove_orphan_layers())
                case IdmlStep(tag="remove_guides", remove_guides=layer_id):
                    return stack.enter_context(package.remove_guides_on_layer(layer_id))
                case IdmlStep(tag="add_story", add_story=(story_id, element_id, element_tag)):
                    return stack.enter_context(package.add_story_with_content(story_id, element_id, element_tag))
                case IdmlStep(tag="leaf_to_node", leaf_to_node=(at, content_ref)):
                    return stack.enter_context(package.xml_element_leaf_to_node(resolved(package, at), content_ref))
                case _ as unreachable:
                    assert_never(unreachable)

        opened = stack.enter_context(idml.IDMLPackage(str(spill(plan.base.data, ".idml"))))
        initial = opened if opened.is_prefixed(plan.base.prefix) else stack.enter_context(opened.prefix(plan.base.prefix))
        package = Block.of_seq(plan.steps).fold(apply, initial)
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


# --- [EXPORTS] --------------------------------------------------------------------------
__all__ = [
    "Idml",
    "IdmlFact",
    "IdmlFault",
    "IdmlSource",
    "IdmlStep",
    "IndesignPayload",
    "PdfCrop",
    "StepFacts",
]
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
