# [PY_ARTIFACTS_INDESIGN]

`Idml` owns the IDML template-mutation hand-off — the editable InDesign deliverable authored by mutating an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is one frozen `msgspec.Struct` binding a `base: IdmlSource` template admitted through `IndesignPayload` and a `steps: tuple[IdmlStep, ...]` fold threaded over one running `IDMLPackage` and drained once into `IdmlFact`. `IdmlStep` is the closed `expression.tagged_union` over SimpleIDML's step-eligible `@use_working_copy` algebra; `prefix` applies once to `Idml.base`, and batch `add_pages_from_idml` subsumes singular page insertion. IDML carries the named XML tag tree, so this owner feeds content into designer-authored structure instead of emitting page geometry.

SimpleIDML is pure-Python over `zipfile`+`lxml`, so the mutation fold crosses the runtime lane through the instance seam — `self.lane.offload(Kernel.of(_mutate, KernelTrait.HOSTILE), self)` over the one `lane: LanePolicy` field, the heavy per-step extract/repackage keeping the lane capacity small — and only the picklable `IdmlFact` crosses back, never a live `IDMLPackage`/`_Element`; the lane's boundary converts a worker raise (a malformed Zip, the `_resolved` `KeyError`, an lxml raise) to the `BoundaryFault` rail, so no raise-bridge and no second boundary exist here. Each `@use_working_copy` mutation extracts to a temp tree, mutates, repackages, overwrites the input file, closes it, and returns a FRESH path-backed instance — so `_mutate` MUST thread the returned instance forward (a `BytesIO`-backed package crashes the decorator's `os.unlink(filename)`), bracket every spill and instance in one `ExitStack`, and drain `Path(final.filename).read_bytes()` off the last instance. One per-instance `emit -> RuntimeRail[ArtifactReceipt]` IS the `core/plan#PLAN` `ArtifactWork.work` coroutine the ONE `ArtifactPipeline` schedules (the `export/layered#LAYERED` `LayeredExport.emit` counterpart), threading the PRE-RUN `_key` — minted over the canonical frozen spec, never the produced package bytes, so `receipt.slot == node.key` and the reuse fabric elides a duplicate — into the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case directly: an IDML package IS an Office-class structured-document Zip.

## [01]-[INDEX]

- [01]-[INDESIGN]: the one IDML template-mutation owner — a frozen `Idml` folding `steps: tuple[IdmlStep, ...]` over one running `IDMLPackage` through SimpleIDML's verified `@use_working_copy` algebra, drained across the process worker into an `ArtifactReceipt.Office` production the `ArtifactPipeline` schedules.

## [02]-[INDESIGN]

- Owner: `Idml` binds `base: IdmlSource`, `steps: tuple[IdmlStep, ...]`, and `lane: LanePolicy`. `IdmlSource` carries bytes, prefix, destination `at`, and source selector `only`. `IdmlStep.facts` feeds fail-fast admission: `sources` carry template data and prefixes, `blobs` carry XML/PDF bodies, `batches` carry plural cardinality, `pages` carry positive page indices, `anchors` carry both XPath axes, and `identifiers` carry layer/story/content ids and tags. `PdfCrop.value` is the verified `import_pdf(crop=)` token.
- Cases: `IdmlStep` cases fold over the one held package — `insert` (`insert_idml` at a destination anchor), `add_pages` (the batch `add_pages_from_idml`), `import_xml` (honoring the source's content-control attributes), `place_pdf` (`import_pdf` carrying the `PdfCrop` mode and page), `set_attributes` (the verified `href` image-relink — an empty `href` removes the page item), `add_note`, `merge_layers`/`suffix_layers`/`remove_layer`/`remove_orphan_layers`/`remove_guides` (the designmap layer algebra), `remove_content` (the template-reset inverse of `insert`), `add_story` (`add_story_with_content`), `leaf_to_node` (a `Rectangle` leaf promoted to a `TextFrame` node so tagged content nests) — dispatched by one total `match`; the legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base.
- Auto: `_mutate` runs the worker seam — it spills `base.data` to a path-backed temporary file, opens and prefixes it, and folds `Block.of_seq(plan.steps)` over one `ExitStack` that registers every spill and returned instance for close-on-exit. Each mutation returns a fresh path-backed instance, so the fold threads that successor into the next step. Nested `spill`, `resolved`, and `apply` kernels own the platform statements: file lifetime, live-tree XPath admission, and provider mutation dispatch never escape as module-level helpers. `_resolved` validates each XPath against the current `package.xml_structure` before its mutation; layer and story ids skip XPath admission because they key the design map and story files. `_mutate` drains bytes and structural inventory from the terminal instance.
- Output: `IdmlFact` is the picklable evidence carrier — the serialized `data` plus the `spreads`/`stories`/`pages`/`fonts`/`styles`/`layers`/`tags`/`nodes` structural inventory read off the final package and the applied-`steps` count.
- Receipt: `_emit` maps the worker rail onto `ArtifactReceipt.Office(self._key, len(fact.data), inventory)` — the structural inventory riding the Office `finish.*` band; IDML is an Office-class structured-document ZIP, so no parallel receipt case exists. `IdmlFact` forces terminal package bytes and structural inventory through one worker result the receipt projection preserves. `ContentIdentity.key` mints the pre-run key over the spec's deterministic-msgpack bytes, and the same key seeds `ArtifactWork`.
- Growth: a SimpleIDML mutation is one `IdmlStep` case plus one `apply` arm plus one `facts` arm over the verified algebra; a source attribute is one `IdmlSource` field; a structural fact is one required `IdmlFact` field; an admission cause is one `IdmlFault` case; an untrusted ingress is one `IndesignPayload` band line; a crop mode is one `PdfCrop` token. Another deliverable is one `ArtifactWork` node the `ArtifactPipeline` schedules.
- Boundary: per-operation base reopen, parallel source lists, erased dictionaries, forwarding case constructors, crop dispatch tables, `BytesIO` package mutation, class-qualified offload, raise bridges, and parallel IDML receipts are rejected. `export_xml`/`export_as_tree` tagged-content egress stays `document/lens#LENS`.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
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
from rasm.runtime.workers import Kernel, KernelTrait
from rasm.runtime.faults import RuntimeRail

from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.core.receipt import ArtifactReceipt

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

_KIND: Final = "idml"  # the `ContentIdentity.key` fmt tag for the PRE-RUN spec key
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
    blobs: tuple[bytes, ...] = ()
    batches: tuple[int, ...] = ()
    pages: tuple[int, ...] = ()
    anchors: tuple[str, ...] = ()
    identifiers: tuple[str, ...] = ()


@tagged_union(frozen=True)
class IdmlStep:
    # constructed directly by keyword case — `IdmlStep(place_pdf=(pdf, at, PdfCrop.CONTENT_VISIBLE, 1))`; the one
    # validated ingress is `Idml.of` over the `facts` projection, never a forwarding classmethod per case.
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
    add_pages: tuple[tuple[IdmlSource, int], ...] = case()  # (source, page_number) rows the batch `add_pages_from_idml` folds
    import_xml: tuple[bytes, str] = case()  # (source xml, destination xpath)
    place_pdf: tuple[bytes, str, PdfCrop, int] = case()  # (pdf bytes, destination xpath, crop mode, page number)
    set_attributes: tuple[str, frozendict[str, str]] = case()  # (xpath, attrs); an `href` key relinks, an empty `href` removes
    add_note: tuple[str, str, str] = case()  # (xpath, note, author)
    merge_layers: str = case()  # merged layer name; "" keeps the active layer's name
    remove_content: str = case()  # xpath whose tagged subtree clears
    suffix_layers: str = case()  # designmap layer-id namespace suffix, the layer-level analogue of the base `prefix`
    remove_layer: str = case()  # designmap layer `Self` id; drops the layer and its guides
    remove_orphan_layers: None = case()  # niladic — strips every layer no spread references
    remove_guides: str = case()  # layer id whose guides clear while the layer itself stays
    add_story: tuple[str, str, str] = case()  # (story_id, xml_element_id, xml_element_tag) — a new story bound to an XML element
    leaf_to_node: tuple[str, str] = case()  # (destination xpath anchor, xml content ref) — Rectangle leaf promoted to a TextFrame node

    @property
    def facts(self) -> StepFacts:
        # BOTH `at` and `only` ride `anchors` (each `_resolved` against its own tree; an empty `only` never reaches
        # `insert_idml`'s `xpath(only)[0]`); layer/story/content ids ride `identifiers`, non-empty-checked WITHOUT
        # `_resolved` because they key the designmap and story files, not the `xml_structure` tree an xpath resolves against.
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
    # closed ADMISSION vocabulary `of` produces; a worker provider raise converts to the runtime
    # `BoundaryFault` at the lane boundary, never this vocabulary.
    tag: Literal["payload", "empty_data", "empty_blob", "empty_batch", "invalid_page", "bad_prefix", "empty_anchor", "empty_ref"] = tag()
    payload: tuple[str, ...] = case()  # the rejected `IndesignPayload` key paths from the `TypeAdapter` miss
    empty_data: int = case()  # source-row index carrying empty `.idml` payload bytes
    empty_blob: int = case()  # XML/PDF body index carrying no bytes
    empty_batch: int = case()  # plural page or attribute-step index carrying no rows
    invalid_page: int = case()  # page-number index carrying a non-positive index
    bad_prefix: int = case()  # source-row index whose prefix is empty or fails `\A\w+\Z`
    empty_anchor: int = case()  # step-anchor index carrying an empty `at` destination or `only` source xpath
    empty_ref: int = case()  # step-identifier index carrying an empty layer/story/content id


# --- [BOUNDARIES] -----------------------------------------------------------------------


class IndesignPayload(TypedDict, closed=True):
    template: Required[ReadOnly[bytes]]  # the untrusted InDesign-exported `.idml` the steps mutate
    # prefix refined at the `TypeAdapter` to the `\A\w+\Z` pattern `IDMLPackage.prefix` enforces, so a
    # non-alphanumeric ingress prefix is an `IdmlFault.payload` at admission, not a worker-time `BaseException`.
    prefix: NotRequired[ReadOnly[Annotated[str, StringConstraints(pattern=r"\A\w+\Z")]]]  # default `_BASE_PREFIX`


_PAYLOAD = TypeAdapter(IndesignPayload)

# --- [SERVICES] -------------------------------------------------------------------------


class Idml(Struct, frozen=True):
    # `lane` arrives projected via LanePolicy.of(context) at the composition root — a capacity literal has no owner; the
    # heavy per-step extract/repackage worker earns a small projected capacity.
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
        bad_data = next((i for i, src in enumerate(sources) if not src.data), None)
        bad_blob = next((i for i, blob in enumerate(blobs) if not blob), None)
        bad_batch = next((i for i, size in enumerate(batches) if size == 0), None)
        bad_page = next((i for i, page in enumerate(pages) if page <= 0), None)
        bad_prefix = next((i for i, src in enumerate(sources) if not _PREFIX.match(src.prefix)), None)
        bad_anchor = next((i for i, anchor in enumerate(anchors) if not anchor), None)
        bad_ref = next((i for i, ref in enumerate(identifiers) if not ref), None)
        match (bad_data, bad_blob, bad_batch, bad_page, bad_prefix, bad_anchor, bad_ref):
            case (int(index), _, _, _, _, _, _):
                return Error(IdmlFault(empty_data=index))
            case (_, int(index), _, _, _, _, _):
                return Error(IdmlFault(empty_blob=index))
            case (_, _, int(index), _, _, _, _):
                return Error(IdmlFault(empty_batch=index))
            case (_, _, _, int(index), _, _, _):
                return Error(IdmlFault(invalid_page=index))
            case (_, _, _, _, int(index), _, _):
                return Error(IdmlFault(bad_prefix=index))
            case (_, _, _, _, _, int(index), _):
                return Error(IdmlFault(empty_anchor=index))
            case (_, _, _, _, _, _, int(index)):
                return Error(IdmlFault(empty_ref=index))
            case _:
                return Ok(cls(base=base, steps=steps, lane=lane))

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=(), admission=Admission(keyed=None), cost=1.0)

    @property
    def _key(self) -> ContentKey:
        # key-over-INPUT: the canonical frozen spec minted PRE-RUN — never a key over the produced package bytes;
        # `ContentIdentity.key` is the bare mint (`of` returns the railed `RuntimeRail[ContentKey]`).
        return ContentIdentity.key(_KIND, _CANON.encode((self.base, self.steps)))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # one lane crossing, one rail: the mutation crosses the IDML worker seam, only the picklable `IdmlFact`
        # returns, and `.map` threads the PRE-RUN key onto the receipt (receipt.slot == node.key).
        crossed = await self.lane.offload(Kernel.of(_mutate, KernelTrait.HOSTILE), self)
        # structural inventory SURVIVES the receipt boundary on the `finish.*` band — spreads, stories,
        # pages, fonts, styles, layers, tags, nodes, steps are facts, never projected away.
        return crossed.map(
            lambda fact: ArtifactReceipt.Office(
                self._key,
                len(fact.data),
                frozendict({
                    name: float(getattr(fact, name))
                    for name in ("spreads", "stories", "pages", "fonts", "styles", "layers", "tags", "nodes", "steps")
                }),
            )
        )


# --- [OPERATIONS] -----------------------------------------------------------------------


def _canonized(raw: object, /) -> object:
    # msgpack enc_hook: an `IdmlStep` lowers to its (tag, payload) pair, a frozendict to its dict view under the
    # deterministic key order; every other payload member (Struct/StrEnum/bytes) encodes natively.
    match raw:
        case IdmlStep() as step:
            return (step.tag, getattr(step, step.tag))
        case frozendict() as row:
            return dict(row)
        case _:
            raise NotImplementedError(type(raw).__name__)


_CANON: Final[Encoder] = Encoder(order="deterministic", enc_hook=_canonized)


def _mutate(plan: Idml) -> IdmlFact:  # simple_idml -> lxml runs behind the worker boundary
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

        # `opened` registers before any prefix successor mints, so the base zip handle closes on every exit —
        # a raise inside `.prefix()` included — and the already-prefixed package is reused directly.
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

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
