# [PY_ARTIFACTS_INDESIGN]

The IDML template-mutation hand-off — the editable InDesign deliverable authored by MUTATING an InDesign-exported `.idml` template, never synthesized from scratch. `Idml` is ONE frozen `msgspec.Struct` owner binding a `base: IdmlSource` template opened ONCE and a `steps: tuple[IdmlStep, ...]` fold of mutations threaded over the one running `IDMLPackage` through its `with`-context new-instance chain and serialized ONCE off the final instance — never a per-op base re-open and never four parallel `datas`/`prefixes`/`ats`/`onlys` lists zipped at the call site. `IdmlStep` is the closed `expression.tagged_union` mutation family — an `Insert` case (one IDML's XML-tagged content into the running package at an XPath anchor through `insert_idml`), an `AddPages` case (whole pages gathered from sibling documents through `add_pages_from_idml`), an `ImportXml` case (an external XML document pushed into the template tag tree through `import_xml` honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control flags and the `href` image references), and a `PlacePdf` case (a `composition/compose#COMPOSE`-produced PDF dropped into a named block placeholder through `import_pdf`) — each carrying its own typed payload, dispatched by one total `match`+`assert_never`, never a `StrEnum` keyed against an erased `dict[str, object]` bag. It binds the placed layout `composition/compose#COMPOSE` produces into the IDML template the designer authored in InDesign: the template carries the named XML structure (the InDesign XML-ready document's tag tree), and this owner feeds content INTO that structure rather than emitting page geometry — keeping the data separate from the structure the IDML round-trip demands. SimpleIDML is pure-Python over `zipfile`+`lxml`, and `lxml` has NO cp315 wheel, so the WHOLE mutation fold crosses the runtime `anyio.to_process` worker seam onto the sub-3.15 interpreter — the same gated band `document/emit#EMIT` dispatches its lxml arms over — and only the serialized package bytes plus the picklable `IdmlFact` evidence cross back, never a live `IDMLPackage`/`_Element`. Each production is one `@receipted(Redaction.STRUCTURAL)` `_emit` returning the stepped owner the harvest weave drains, contributing the existing `core/receipt#RECEIPT` `ArtifactReceipt.Office` case (the IDML package IS an Office-class structured-document Zip archive) over the mutated-package byte count, never a new receipt case.

## [01]-[INDEX]

- [01]-[INDESIGN]: IDML template-mutation owner over a `base: IdmlSource` opened once and a closed `IdmlStep` `tagged_union` fold (`Insert`/`AddPages`/`ImportXml`/`PlacePdf`) threaded over one running `IDMLPackage` through `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` and serialized once, crossing the runtime `anyio.to_process` worker seam because `lxml` is gated off cp315, validating each XPath anchor against the live `xml_structure` lxml tree, returning structural `IdmlFact` evidence, admitted through `Idml.of` over the closed `IdmlFault` precondition vocabulary, produced through the one modal-arity `produced(Idml | Iterable[Idml])` entrypoint, and folding into the shared content key plus the `@receipted`-harvested `ArtifactReceipt.Office` case.

## [02]-[INDESIGN]

- Owner: `Idml` the one IDML template-mutation owner — a frozen `msgspec.Struct` binding `base: IdmlSource` (the InDesign-exported `.idml` template opened ONCE), `steps: tuple[IdmlStep, ...]` (the mutation fold threaded over the one running `IDMLPackage`), and `fact: IdmlFact | None` (the worker's threaded evidence), never a per-op base re-open; `IdmlSource` the frozen row binding one bindable IDML document — `data` the package bytes, `prefix` the `IDMLPackage.prefix` namespace applied before any merge to avoid reference collisions, `at` the XPath anchor in the destination structure, `only` the sub-tree selector to lift from the source — so a source is one row carrying its own bytes/prefix/anchors rather than four zipped parallel lists; `IdmlStep` the closed `expression.tagged_union` mutation family whose every case carries its own typed payload and whose `sources`/`anchors` member projections feed the admission gate; `IdmlFact` the frozen evidence carrier the worker returns — the serialized `data` plus the `spreads`/`stories`/`pages`/`nodes` structural inventory read off the final package and the applied-`steps` count; `IdmlFault` the closed admission vocabulary `of` produces (`empty_prefix`/`empty_anchor` carrying the offending index), distinct from the runtime `BoundaryFault` every worker provider raise converts to; `PdfCrop` the closed `StrEnum` of IDML `PDFCrop_EnumValue` crop modes (`CONTENT_VISIBLE`/`CONTENT_ALL`/`ART`/`PDF`/`TRIM`/`BLEED`/`MEDIA`/`CONTENT`) whose `.value` is the `import_pdf(crop=)` token passed directly — never a hand-typed crop-string literal per call and never a `_CROP` dispatch table restating the enum's own values, the COLLAPSE_SCAN duplicate-entry form; the `SimpleIDML` `IDMLPackage` `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` side-effect algebra (each returning a NEW `with`-context instance) and its `spreads`/`stories`/`pages`/`xml_structure`/`export_xml` introspection surface the worker-side mutation-and-read surface.
- Cases: `IdmlStep` cases — `Insert(module)` (template composition — open and prefix the `module` source, then `package.insert_idml(part, at=module.at, only=module.only)` so the sub-template's XML-tagged content lands at its XPath anchor in the running package, the returned new `with`-context instance threaded into the next step) · `AddPages(pages)` (page combination — prefix each page source and fold `package.add_pages_from_idml([(part, number, src.at, src.only) …])` so the destination page count grows and the XML structure integrates each added file, the one SimpleIDML batch call) · `ImportXml(xml, at)` (XML content import — `package.import_xml(xml.decode(), at=at)` honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` content-control flags and resolving each `href` image reference, so the data populates the structure the designer authored without touching page geometry) · `PlacePdf(pdf, at, crop)` (PDF placement — drop a `composition/compose#COMPOSE`-produced `pdf` into a named block placeholder through `package.import_pdf(url, at=at, crop=crop.value)` so the placed flat layout lands inside the InDesign block as a linked, re-croppable PDF the designer re-links) — matched by one total `match`/`case`+`assert_never`; never a sibling op per template and never an `if insert`/`if import` branch re-deriving the mutation the case already names. The legacy monolithic `Compose`/`Combine`/`Import`/`Place` ops collapse into this step fold over one base: a `Compose` is `(Insert(m) …)` over the modules, a `Combine` is one `AddPages(pages)`, so a template insert THEN a data import THEN a cover place is ONE `Idml` over one running package rather than three re-serialized round-trips — the per-op base re-open the old shape forced is the deleted form.
- Entry: `Idml.of(base, steps)` is the admission classmethod returning `Result[Self, IdmlFault]` — it folds `step.sources` for a non-empty `prefix` (an empty prefix is the reference-collision cause the IDML merge contract names) and `step.anchors` for a non-empty `at`, rejecting the first offending index, so the worker fold is total over well-formed plans. `produced(plans: Idml | Iterable[Idml])` is the ONE modal-arity production entrypoint discriminating on the INPUT SHAPE — a lone plan is `Block.singleton`, an iterable `Block.of_seq`, normalized once at the head — returning `RuntimeRail[Block[ContentKey]]` over the runtime `async_boundary`, with NO `batch`/`mode` knob. Each plan's `_emit` is `@receipted(Redaction.STRUCTURAL)` and returns the stepped `Self` carrying the `IdmlFact` the harvest weave drains through `contribute`; `_emit` crosses the runtime `anyio.to_process` worker seam onto `_mutate` because SimpleIDML is pure-Python over `zipfile`+`lxml` and `lxml` has NO cp315 wheel — the IDML mutation runs on the sub-3.15 interpreter, the same gated band `document/emit#EMIT` dispatches its lxml arms over, and only the serialized package bytes plus the picklable `IdmlFact` cross back, never a live `IDMLPackage`/`_Element` (unpicklable across the interpreter boundary). The content key is minted off `IdmlFact.data` through `ContentIdentity.of`, never re-minted off a source key.
- Auto: `_mutate` runs in the sub-3.15 worker — it opens the `base` as a path-backed `IDMLPackage` (the constructor opens a Zip package over a `tempfile.NamedTemporaryFile` spill), prefixes it through `package.prefix(base.prefix)`, and folds each `IdmlStep` through one total `match`+`assert_never` over a single `contextlib.ExitStack` that registers every returned new instance and every temp spill for close-on-exit — the IDML `with`-context contract, where a lost instance leaves an unclosed backing file the platform (notably Windows) cannot unlink. The `Insert` arm opens and prefixes the module then `package.insert_idml(part, at=_anchored(package, module.at), only=module.only)`; the `AddPages` arm prefixes each source and folds `package.add_pages_from_idml(specs)`; the `ImportXml` arm `package.import_xml(xml.decode(), at=_anchored(package, at))`; the `PlacePdf` arm spills the PDF, builds its `Path.as_uri()` `file:` URL, and `package.import_pdf(url, at=_anchored(package, at), crop=crop.value)`. `_anchored` validates the XPath anchor resolves in the live `package.xml_structure` lxml tree through `_Element.xpath(at)` BEFORE the mutation — an empty result raises so SimpleIDML never fails mid-fold on a phantom anchor, the raise crossing back to the `async_boundary` `BoundaryFault` conversion. After the fold, `_mutate` reads the structural inventory off the final instance — `len(package.spreads)`/`len(package.stories)`/`len(package.pages)` and the `package.xml_structure.iter()` node count — and drains `Path(package.filename).read_bytes()` (the final instance's mutation already serialized its backing file) into the `IdmlFact`.
- Receipt: `Idml.contribute` projects `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` off `self.fact` — an IDML package IS an Office-class structured-document Zip archive (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form — and the `@receipted(Redaction.STRUCTURAL)` weave over the pure `_emit` drains the one-element `Iterable[Receipt]` and emits through `Signals`, exactly as `document/emit#EMIT` harvests its `DocumentPlan.contribute`. The rich structural evidence (`spreads`/`stories`/`pages`/`nodes`/`steps`) rides the `IdmlFact` carrier a consumer reads off `self.fact`, not the flat `Office` scalar — a structure-bearing receipt case is a `core/receipt#RECEIPT` growth concern, so IDML hand-off mints only the `Office` byte-count arity it owns. The content key is content-addressed off `fact.data` through `ContentIdentity.of`, so the `contribute` receipt mint and the `produced` return derive the identical key independently and the runtime reuse-fabric elides the duplicate.
- Packages: `SimpleIDML` ([RESEARCH] not-yet-admitted, signature-locked — `from simple_idml import idml`/`idml.IDMLPackage(path)` the package root, `IDMLPackage.prefix(prefix) -> IDMLPackage`/`insert_idml(other, at, only) -> IDMLPackage`/`add_pages_from_idml(specs) -> IDMLPackage`/`import_xml(xml, at) -> IDMLPackage`/`import_pdf(url, at, crop) -> IDMLPackage` the `with`-context side-effect algebra each returning a NEW instance, `spreads`/`stories`/`pages`/`xml_structure`/`export_xml()` the introspection surface, pure-Python over `zipfile`+`lxml`) on the runtime sub-3.15 worker band pending admission; `lxml` (the `xml_structure` `_Element.xpath` anchor validation, manifest-gated `python_version<'3.15'` — the dependency constraint that puts the whole fold on the worker seam, shared with `document/emit#EMIT`); `anyio` (`to_process.run_sync` the worker-seam crossing); `msgspec` (`Struct` frozen `IdmlSource`/`IdmlFact`/`Idml`); `expression` (`tagged_union`/`tag`/`case` the `IdmlStep`/`IdmlFault` families, `Result`/`Ok`/`Error` the admission rail, `Block` the modal carrier); stdlib `contextlib.ExitStack` (the worker resource bracket), `tempfile.NamedTemporaryFile(delete_on_close=False)` (the path-backed spill the `IDMLPackage` constructor opens), `pathlib.Path` (the `as_uri` PDF `file:` URL and the backing-file drain); runtime (`content_identity.ContentIdentity`/`ContentKey`, `faults.RuntimeRail`/`async_boundary`, `receipts.Receipt`/`Redaction`/`receipted`/`Signals`).
- Growth: a new template-mutation kind (a story-text replace, a spread reorder, a layer-swap) is one `IdmlStep` case plus one `_mutate` arm over the existing `IDMLPackage` algebra — never a re-implemented IDML Zip/XML serializer; a new source attribute (a target spread, a style override) is one field on `IdmlSource` threaded into the consuming arm; a new structural fact is one field on `IdmlFact`; a new admission cause is one `IdmlFault` case; a new crop mode is one `PdfCrop` token whose `.value` threads into `import_pdf(crop=)`, never a hand-typed crop string. The held-package fold absorbs an arbitrary step sequence over one running package; a second deliverable is one more element in `produced`'s `T | Iterable[T]`. Zero new surface.
- Boundary: a from-scratch InDesign-document synthesizer, a per-op `_compose`/`_import`/`_place` writer family beside the one `_mutate` dispatch, a per-op base re-open beside the one `base`-opened-once fold, a `datas`/`prefixes`/`ats` triple-list zipped at the call site beside the one `IdmlSource` row, a hand-typed `PDFCrop_EnumValue` literal or a `_CROP` table restating `PdfCrop`'s own values beside `crop.value`, a hand-edited IDML Zip-member XML string beside the `IDMLPackage` methods, an in-capsule cp315 mutation beside the `anyio.to_process` worker seam the lxml gate forces, a live `IDMLPackage`/`_Element` returned across the worker boundary beside the serialized-bytes-plus-`IdmlFact` return, a prose-only receipt beside the wired `@receipted`/`contribute`, and a `StrEnum`-plus-`dict[str, object]` erased-bag dispatch are the deleted forms; no UI, no live InDesign editor, no page-geometry synthesis. `SimpleIDML` owns the IDML package mutation and introspection — the Zip-archive structure, the named XML tag tree, the `prefix`/`insert_idml`/`add_pages_from_idml`/`import_xml`/`import_pdf` side-effect algebra, and the `xml_structure`/`export_xml` introspection read — and this owner composes the mutation algebra plus the `xml_structure` read (anchor validation and the structural inventory) through the `with`-context new-instance boundary, re-implementing no IDML serialization. The InDesign-Server SOAP `save_as` conversion to `.indd`/`.pdf`/`.jpeg` stays outside this package (it needs a live network InDesign Server seat and a co-mounted working directory the host-free engine does not own) — this owner produces the editable `.idml` deliverable, never an `.indd`/`.pdf` server render. The placed flat layout arrives from `composition/compose#COMPOSE` keyed by the same `ContentKey` and binds as a placed PDF through `PlacePdf` or as XML-tagged content through `ImportXml`; the editable-named-layer SVG/PDF/TIFF hand-off stays `export/layered#LAYERED`'s — IDML is the InDesign template-mutation complement, layered the Illustrator/Acrobat/Photoshop named-layer complement, each meeting its native editor family.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from contextlib import ExitStack
from copy import replace
from enum import StrEnum
from pathlib import Path
from tempfile import NamedTemporaryFile
from typing import TYPE_CHECKING, Final, Literal, Self, assert_never

from anyio import to_process
from expression import Error, Ok, Result, case, tag, tagged_union
from expression.collections import Block
from msgspec import Struct

from rasm.runtime.content_identity import ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.receipts import Receipt, Redaction, receipted

from artifacts.core.receipt import ArtifactReceipt

if TYPE_CHECKING:
    from simple_idml.idml import IDMLPackage  # [RESEARCH] not-yet-admitted; the worker-side IDML package type `_anchored` reads `xml_structure` off

# --- [TYPES] ----------------------------------------------------------------------------


class PdfCrop(StrEnum):
    CONTENT_VISIBLE = "CropContentVisibleLayers"
    CONTENT_ALL = "CropContentAllLayers"
    CONTENT = "CropContent"
    ART = "CropArt"
    PDF = "CropPDF"
    TRIM = "CropTrim"
    BLEED = "CropBleed"
    MEDIA = "CropMedia"

# --- [CONSTANTS] ------------------------------------------------------------------------

_KIND: Final = "idml"      # the `ContentIdentity.of` kind tag minted off the mutated-package bytes
_ROOT: Final = "/Root"     # the IDML tag-tree root, the default destination anchor an `at` resolves against

# --- [MODELS] ---------------------------------------------------------------------------


class IdmlSource(Struct, frozen=True):
    data: bytes
    prefix: str
    at: str = _ROOT
    only: str | None = None


@tagged_union(frozen=True)
class IdmlStep:
    tag: Literal["insert", "add_pages", "import_xml", "place_pdf"] = tag()
    insert: IdmlSource = case()
    add_pages: tuple[tuple[IdmlSource, int], ...] = case()
    import_xml: tuple[bytes, str] = case()
    place_pdf: tuple[bytes, str, PdfCrop] = case()

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
    def PlacePdf(cls, pdf: bytes, at: str, crop: PdfCrop = PdfCrop.CONTENT_VISIBLE, /) -> Self:
        return cls(place_pdf=(pdf, at, crop))

    @property
    def sources(self) -> tuple[IdmlSource, ...]:
        match self:
            case IdmlStep(tag="insert", insert=module):
                return (module,)
            case IdmlStep(tag="add_pages", add_pages=pages):
                return tuple(src for src, _ in pages)
            case IdmlStep(tag="import_xml") | IdmlStep(tag="place_pdf"):
                return ()
            case _ as unreachable:
                assert_never(unreachable)

    @property
    def anchors(self) -> tuple[str, ...]:
        match self:
            case IdmlStep(tag="insert", insert=module):
                return (module.at,)
            case IdmlStep(tag="add_pages", add_pages=pages):
                return tuple(src.at for src, _ in pages)
            case IdmlStep(tag="import_xml", import_xml=(_, at)) | IdmlStep(tag="place_pdf", place_pdf=(_, at, _)):
                return (at,)
            case _ as unreachable:
                assert_never(unreachable)


class IdmlFact(Struct, frozen=True):
    data: bytes
    spreads: int = 0
    stories: int = 0
    pages: int = 0
    nodes: int = 0
    steps: int = 0

# --- [ERRORS] ---------------------------------------------------------------------------


@tagged_union(frozen=True)
class IdmlFault:
    # the closed ADMISSION vocabulary `of` produces; a worker provider raise (a malformed `.idml`
    # Zip, an `lxml.etree.XMLSyntaxError`, the `_anchored` missing-anchor) converts to the runtime
    # `BoundaryFault` at the `async_boundary` capsule, never into this interior vocabulary.
    tag: Literal["empty_data", "empty_prefix", "empty_anchor"] = tag()
    empty_data: int = case()    # index of the source row carrying empty `.idml` payload bytes
    empty_prefix: int = case()  # index of the source row carrying an empty namespace prefix
    empty_anchor: int = case()  # index of the step anchor carrying an empty `at`

# --- [SERVICES] -------------------------------------------------------------------------


class Idml(Struct, frozen=True):
    base: IdmlSource
    steps: tuple[IdmlStep, ...]
    fact: IdmlFact | None = None

    @classmethod
    def of(cls, base: IdmlSource, steps: tuple[IdmlStep, ...], /) -> Result[Self, IdmlFault]:
        sources = (base, *(src for step in steps for src in step.sources))
        anchors = tuple(anchor for step in steps for anchor in step.anchors)
        bad_data = next((i for i, src in enumerate(sources) if not src.data), None)
        bad_prefix = next((i for i, src in enumerate(sources) if not src.prefix), None)
        bad_anchor = next((i for i, anchor in enumerate(anchors) if not anchor), None)
        return (
            Error(IdmlFault(empty_data=bad_data)) if bad_data is not None
            else Error(IdmlFault(empty_prefix=bad_prefix)) if bad_prefix is not None
            else Error(IdmlFault(empty_anchor=bad_anchor)) if bad_anchor is not None
            else Ok(cls(base=base, steps=steps))
        )

    @receipted(Redaction.STRUCTURAL)
    async def _emit(self) -> Self:
        # returns the stepped owner (a `ReceiptContributor`) the harvest weave drains; the lxml-bound
        # mutation crosses the sub-3.15 worker seam and only the serialized `IdmlFact` returns.
        return replace(self, fact=await to_process.run_sync(_mutate, self))

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
    return Block.of_seq([ContentIdentity.of(_KIND, plan.fact.data) for plan in stepped])


def _mutate(plan: Idml) -> IdmlFact:  # [RESEARCH] sub-3.15 worker: simple_idml -> lxml is gated off the cp315 core
    from simple_idml import idml

    with ExitStack() as stack:
        package = stack.enter_context(idml.IDMLPackage(str(_spill(stack, plan.base.data, ".idml"))).prefix(plan.base.prefix))
        for step in plan.steps:
            match step:
                case IdmlStep(tag="insert", insert=module):
                    part = stack.enter_context(idml.IDMLPackage(str(_spill(stack, module.data, ".idml"))).prefix(module.prefix))
                    package = stack.enter_context(package.insert_idml(part, at=_anchored(package, module.at), only=module.only))
                case IdmlStep(tag="add_pages", add_pages=pages):
                    specs = [(stack.enter_context(idml.IDMLPackage(str(_spill(stack, src.data, ".idml"))).prefix(src.prefix)), number, _anchored(package, src.at), src.only) for src, number in pages]
                    package = stack.enter_context(package.add_pages_from_idml(specs))
                case IdmlStep(tag="import_xml", import_xml=(xml, at)):
                    package = stack.enter_context(package.import_xml(xml.decode(), at=_anchored(package, at)))
                case IdmlStep(tag="place_pdf", place_pdf=(pdf, at, crop)):
                    package = stack.enter_context(package.import_pdf(_spill(stack, pdf, ".pdf").as_uri(), at=_anchored(package, at), crop=crop.value))
                case _ as unreachable:
                    assert_never(unreachable)
        structure = package.xml_structure
        return IdmlFact(
            data=Path(package.filename).read_bytes(),
            spreads=len(package.spreads),
            stories=len(package.stories),
            pages=len(package.pages),
            nodes=sum(1 for _ in structure.iter()),
            steps=len(plan.steps),
        )


def _spill(stack: ExitStack, data: bytes, suffix: str, /) -> Path:
    # the `IDMLPackage` constructor opens a path-backed Zip; spill the bytes to a temp file kept on
    # disk past `close()` and unlinked on stack exit, the OS handle released so the package reopens it.
    handle = stack.enter_context(NamedTemporaryFile(suffix=suffix, delete_on_close=False))
    handle.write(data)
    handle.close()
    return Path(handle.name)


def _anchored(package: "IDMLPackage", at: str, /) -> str:
    # the IDML tag tree is an lxml `_Element`; an empty `xpath` result is the missing-anchor signal,
    # raised before the mutation so SimpleIDML never fails mid-fold and the `async_boundary` converts it.
    if not package.xml_structure.xpath(at):
        raise KeyError(at)
    return at
```

## [03]-[RESEARCH]

- [IDML_WORKER_SEAM] [RESEARCH]: the IDML mutation runs on the runtime sub-3.15 worker band, NOT in-capsule on cp315 — SimpleIDML is pure-Python but depends on `lxml`, and `lxml` is manifest-gated `python_version<'3.15'` (no CPython 3.15 wheel for the libxml2 C-extension, verified against the folder `.api/lxml.md` header), so `import simple_idml` transitively imports `lxml` and fails on the cp315 core. The whole `_mutate` fold therefore crosses the runtime `anyio.to_process.run_sync(_mutate, plan)` worker seam onto the sub-3.15 interpreter — the same gated band `document/emit#EMIT` dispatches its `Band.WORKER` lxml/pikepdf arms over (`to_process.run_sync(_worker_emit, self)`) and `composition/compose#COMPOSE` offloads its `python_version<'3.15'` pillow pass over. Only the serialized package bytes plus the picklable `IdmlFact` (bytes + ints) cross back; a live `IDMLPackage` (a `zipfile.ZipFile` subclass holding an open file) and the `xml_structure` `_Element` tree are unpicklable across the interpreter boundary and never return. A worker death surfaces as `anyio.BrokenWorkerProcess` the runtime `reliability/faults#FAULT` `CLASSIFY` `resource` row converts; an in-worker provider raise (`lxml.etree.XMLSyntaxError` on a malformed `.idml`, the `_anchored` `KeyError`) re-raises in the host where `async_boundary` lifts it to a `BoundaryFault`. The lxml dependency graph forbids any in-capsule cp315 mutation arm: `import simple_idml` cannot resolve on the core interpreter, so the worker seam is forced, not chosen.
- [STEP_PIPELINE] [RESOLVED]: the owner binds `base: IdmlSource` opened ONCE and folds `steps: tuple[IdmlStep, ...]` over the one running `IDMLPackage`, serialized once — the SimpleIDML `with`-context contract states every side-effect method RETURNS A NEW INSTANCE whose backing file must be closed (a lost reference leaves an unclosed file the platform, notably Windows, cannot `os.unlink`), so `_mutate` threads each mutation through one `contextlib.ExitStack` registering every returned instance and every `NamedTemporaryFile` spill for close-on-exit rather than reassigning a single handle. This is the structural collapse of the prior four monolithic ops: each old `Compose`/`Combine`/`Import`/`Place` re-opened its own base, did all its mutations, and serialized — so chaining required N re-serialized byte round-trips and N content keys. The step fold opens once and applies an arbitrary sequence in one worker hop: a template insert THEN a data import THEN a cover place is one `Idml(base, steps)` producing one deliverable. The base source lifts out of every case (declared once on `Idml.base`), and the SimpleIDML batch `add_pages_from_idml(specs)` stays one `AddPages` step while per-document `insert_idml` is one `Insert` step each, faithful to the two distinct member arities.
- [ANCHOR_VALIDATION] [RESOLVED]: `_anchored` validates each destination `at` XPath anchor resolves in the live `package.xml_structure` lxml `_Element` tree through `_Element.xpath(at)` (the folder `.api/lxml.md` query-entrypoint `[02]` row) BEFORE the `insert_idml`/`import_xml`/`import_pdf` mutation — the source-side `only` sub-tree selector filters the source document, never the destination, so it is not anchor-checked here — SimpleIDML raises mid-fold on a phantom anchor, leaving partial state, so the empty-`xpath`-result pre-check converts the failure into a clean `KeyError(at)` the `async_boundary` lifts to a `BoundaryFault(boundary=(subject, detail))` carrying the anchor. This realizes the introspection surface the owner names: `xml_structure` is read for validation, and the final-instance `spreads`/`stories`/`pages` counts plus the `xml_structure.iter()` node count ride the `IdmlFact` evidence carrier (the emit-page `EmitFact` pattern), so a downstream consumer reading `self.fact` sees the template's structural inventory rather than the owner asserting a read it never performs.
- [RECEIPT_WIRING] [RESOLVED]: IDML hand-off contributes the EXISTING `core/receipt#RECEIPT` `ArtifactReceipt.Office(key, byte_count)` case (`office: tuple[ContentKey, int]`) — an IDML package IS an Office-class structured-document Zip archive (the same case the OOXML/ODF producers contribute), so a parallel `Idml`/`Indesign` receipt case is the rejected form. The contribution is wired, not prose: `_emit` is `@receipted(Redaction.STRUCTURAL)` and returns the stepped `Self` (a `ReceiptContributor`), and `contribute` reads `self.fact`, mints the content key off `fact.data`, and yields `ArtifactReceipt.Office(key, len(fact.data)).contribute()` — the harvest weave draining and emitting through `Signals` exactly as `document/emit#EMIT` harvests `DocumentPlan.contribute`. The structural `spreads`/`stories`/`pages`/`nodes`/`steps` evidence rides the `IdmlFact` carrier, not the flat `Office` scalar; a structure-bearing receipt case is a `core/receipt#RECEIPT` growth concern, so this owner mints only the `Office` byte-count arity it owns.
- [SIMPLEIDML_SIGNATURE_LOCK] [RESEARCH]: the SimpleIDML member chain is verified against the official `Starou/SimpleIDML` README (latest `v0.91.6`, active master, Python `3.9+`, pure-Python over `zipfile`+`lxml`) and SIGNATURE-LOCKED pending admission — `from simple_idml import idml`/`idml.IDMLPackage(path)` opens the Zip-archive package; `IDMLPackage.prefix(prefix)` namespaces every internal reference (the README mandates prefixing before any merge); `insert_idml(other, at="/Root/…", only="/Root/…")` inserts XML-tagged content at an XPath anchor; `add_pages_from_idml([(pkg, page, at, only), …])` gathers whole pages; `import_xml(xml, at)` populates the tag tree honoring the `simpleidml-setcontent`/`simpleidml-ignorecontent`/`simpleidml-forcecontent` flags and `href` references; `import_pdf(url, at, crop)` drops a PDF into a block with crop one of the IDML `PDFCrop_EnumValue` set; `export_xml()`/`spreads`/`stories`/`pages`/`xml_structure`/`xml_structure_pretty()` introspect. SimpleIDML is NOT-yet-admitted in `pyproject.toml` and carries no folder `.api` catalogue. Admit `simpleidml` on the sub-3.15 worker band (alongside the already-gated `lxml`), author its `.api` catalogue with reflection-verified members, then resolve the exact `_spill`/backing-file-drain temp-file plumbing and the `prefix`/`insert_idml`/`add_pages_from_idml` return-shape against the verified surface — the `from simple_idml import idml` boundary import marks the not-yet-admitted dependency until then.
- [SERVER_CONVERSION_OUT_OF_SCOPE] [RESOLVED]: the SimpleIDML InDesign-Server SOAP conversion path (`simple_idml.indesign.indesign.save_as(path, [{"fmt": "indd"|"pdf"|"jpeg"|"idml"|"zip"}], server_url, client_workdir, server_workdir)`) converts an `.idml` to `.indd`/`.pdf`/`.jpeg` through a LIVE InDesign Server and a shared readable/writable working directory — OUT OF SCOPE for the host-free durable-output engine, which owns neither a network InDesign Server seat nor a co-mounted filesystem. The deliverable this owner produces is the editable `.idml` package the designer opens in InDesign directly; the `.indd`/`.pdf` server render is the rejected arm.
- [LAYERED_SIBLING_BOUNDARY] [RESOLVED]: IDML is the InDesign template-mutation hand-off; `export/layered#LAYERED` is the Illustrator/Acrobat/Photoshop named-layer hand-off (SVG `<g id=>` Groups, PDF OCG optional-content groups, layered TIFF). The two share the editable-export domain but own disjoint editor families and disjoint formats — IDML mutates a designer's `.idml` template through its XML structure, layered authors named layers into SVG/PDF/TIFF from the placed sources — so a layered arm grafted onto `IdmlStep` or an IDML arm grafted onto `ExportOp` is the rejected form. Both consume the same `composition/compose#COMPOSE` placed layout keyed by the same `ContentKey`; the layered owner's `Layer(name, source, bbox, …)` named-layer binding is the IDML owner's `IdmlSource(data, prefix, at, only)` template-binding counterpart — each export owner binds the placed layout into its native editor format through its own typed source row, never a shared erased source bag.
