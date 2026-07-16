# [PY_ARTIFACTS_EGRESS]

The security-and-navigation finishing close over an emitted PDF or Office container: `DocumentEgress` takes bytes authored by `document/emit#DOCUMENT` and returns a sealed, watermarked, redaction-burned, imposed, form-baked, content-rewritten, or Office-(de)sealed artifact keyed by the runtime content key — it finishes an emitted artifact and never authors one. Re-signing routes to `../exchange/conformance#CONFORM`, PDF/A authoring to `document/emit#DOCUMENT`, named-layer authoring to `../export/layered#LAYERED`, descriptive-metadata authoring to `../exchange/metadata#METADATA`.

`EgressStep` is the closed `StrEnum` over the finishing operations and `FINISHERS` binds each step to one `Finisher` row — its `FinishFact`-returning arm, its `office` receipt discriminant, its optional `permissive` arm — so the table is the totality proof. `LicenseLane` selects the footing at the value: `AGPL_MAX` keeps the richest arm, `PERMISSIVE` the MIT/Apache/BSD arm so a closed-distributed deliverable carries no copyleft obligation. Every arm resolves in-process; the fold crosses the GIL-releasing runtime thread lane under the `@receipted(OPEN)` weave and contributes one `../core/receipt#RECEIPT` `ArtifactReceipt.Egress`/`.Office` case off the `office` discriminant. The OUTLINE and REDACT arms fold the `document/model#NODE` semantic tree — `SectionNode` headings into the bookmark hierarchy, `AnnotKind.REDACTION` rects into the burn targets.

## [01]-[INDEX]

- [01]-[FINISH]: the one finishing close discriminating `EgressStep` over the `FINISHERS` totality table under the `LicenseLane` footing.

## [02]-[FINISH]

- Owner: `DocumentEgress` — one `Finisher.arm` per step resolved off `FINISHERS`, never an `if step ==` cascade or a worker-side `match`; `LicenseLane` is read once at the value in `_stepped`, never a per-call knob. `Finishing` bundles every trusted policy value object while `Extras` carries the untrusted material (stamp bytes, attachment payload, Office credentials) — the admission split is trust, not concern, so passwords never ride the untrusted payload and stamp bytes never ride a trusted default.
- Entry: `of` admits untrusted material exactly once through the `EgressPayload` `TypedDict`, its `extra_items=str` band folding the format-discriminated Office credential axis into `Extras.credentials`, and rejects an under-supplied step through `_PREREQ` into `EgressFault.incomplete` before the fold runs — the interior is total over admitted owners and never re-validates. One polymorphic entry owns both the singular step and the chain: the `EgressStep | tuple[EgressStep, ...]` discriminant threads finished bytes step-to-step through one `reduce`, never a caller-orchestrated re-entry or a `mode` knob.
- Auto: each arm returns a `FinishFact` threaded onto a successor owner through `structs.replace`, so `contribute` reads the arm's own evidence without a second parse. `pypdf` is reserved for the structural OUTLINE/IMPOSE/NAVIGATE/FORMS arms and the gated SANITIZE/OPTIMIZE second passes it owns, never a parallel encryptor — qpdf authors every encryption strength through one `pikepdf.Encryption` leg. Native packages bind as module-scope `lazy import` reified on first arm use; no native import lands on the core owner.
- Receipt: the node key mints PRE-RUN over the canonical input (steps, source, finishing, extras, license) so keyed admission probes the warm seed before the fold runs, and `receipt.slot == node.key`. REWRITE's OCG-strip count and FORMS' baked-widget count ride the `overlays` slot — content-composition operations of the watermark-overlay family.
- Packages: `pikepdf` (MPL) owns the qpdf object model, encryption, composition, and save strategy; `pypdf` (BSD) the pure-Python structural arms; `pymupdf` (AGPL) the richest REDACT burn-in with `search_for` needle match, flagged for supersession on the permissive lane; `pdf_oxide` (MIT/Apache) the permissive REDACT/SANITIZE arms and the STRIP running-content removal no other step owns; `msoffcrypto` the bidirectional Office confidentiality rail.
- Growth: a new finishing step is one `EgressStep` row plus one `Finisher` row, plus one `_PREREQ` row when it needs material; a commercial-safe alternative is one `Finisher.permissive` arm, never a parallel license-keyed table; a new policy concern is one `Finishing` field carrying its own value object; a new receipt fact is one `FinishFact` field the arm populates, never a re-derivation off the bytes; an encryption strength is one `Strength` row plus its `_STRENGTHS` cell; a document-wide strip class is one `PruneClass` member plus one `_PRUNE` row; a deeper chain is one more step in the sequence the rail already folds.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterable, Iterator
from enum import StrEnum
from functools import reduce
from io import BytesIO
from itertools import batched
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

from expression import Error, Ok, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.runtime.identity import CANONICAL_POLICY, ContentIdentity, ContentKey
from rasm.runtime.faults import RuntimeRail, async_boundary
from rasm.runtime.lanes import LanePolicy, Modality
from rasm.runtime.resilience import RetryClass
from rasm.runtime.receipts import OPEN, Receipt, receipted

from artifacts.core.plan import Admission, ArtifactWork
from artifacts.core.receipt import ArtifactReceipt
from artifacts.document.model import AnnotKind, AnnotationNode, DocumentNode, SectionNode, walk

lazy import msoffcrypto
lazy import pdf_oxide
lazy import pikepdf
lazy import pymupdf
lazy from pikepdf import sanitize
lazy from pypdf import PdfReader, PdfWriter, Transformation
lazy from pypdf.generic import Fit

if TYPE_CHECKING:
    import pikepdf
    from pypdf import PageObject
    from pypdf.generic import IndirectObject


# --- [TYPES] ----------------------------------------------------------------------------
class EgressStep(StrEnum):
    ENCRYPT = "encrypt"
    OUTLINE = "outline"
    WATERMARK = "watermark"
    ATTACH = "attach"
    IMPOSE = "impose"
    NAVIGATE = "navigate"
    FORMS = "forms"
    REWRITE = "rewrite"
    REDACT = "redact"
    STRIP = "strip"  # repeat-detection running-content removal (pdf_oxide, MIT/Apache — already permissive)
    SANITIZE = "sanitize"
    OPTIMIZE = "optimize"
    PROTECT = "protect"


class LicenseLane(StrEnum):
    # a step with no `Finisher.permissive` arm runs its single arm under both footings.
    AGPL_MAX = "agpl-max"
    PERMISSIVE = "permissive"


class Strength(StrEnum):
    RC4_40 = "RC4-40"
    RC4_128 = "RC4-128"
    AES_128 = "AES-128"
    AES_256 = "AES-256"


class ImposeLayout(StrEnum):
    NUP = "nup"
    BOOKLET = "booklet"


class LayerMode(StrEnum):
    KEEP = "keep"
    FLATTEN = "flatten"
    STRIP = "strip"


class PruneClass(StrEnum):  # the pypdf document-wide object class the SANITIZE prune strips whole
    LINKS = "links"
    ANNOTATIONS = "annotations"
    IMAGES = "images"
    TEXT = "text"


class FitMode(StrEnum):  # the pypdf `generic.Fit` destination-zoom family the outline node carries
    FIT = "fit"
    FIT_H = "fit_h"
    FIT_V = "fit_v"
    FIT_B = "fit_b"
    XYZ = "xyz"


class StreamDecode(StrEnum):  # the `pikepdf.StreamDecodeLevel` re-encode strength the OPTIMIZE close projects
    NONE = "none"
    GENERALIZED = "generalized"
    SPECIALIZED = "specialized"
    ALL = "all"


class PageMode(StrEnum):  # the `pypdf.PdfWriter.page_mode` `/PageMode` open-panel state the NAVIGATE close authors
    NONE = "/UseNone"
    OUTLINES = "/UseOutlines"
    THUMBS = "/UseThumbs"
    FULLSCREEN = "/FullScreen"
    OPTIONAL_CONTENT = "/UseOC"
    ATTACHMENTS = "/UseAttachments"


class PageLayout(StrEnum):  # the `pypdf.PdfWriter.set_page_layout` `/PageLayout` spread the NAVIGATE close authors
    NONE = "/NoLayout"
    SINGLE = "/SinglePage"
    ONE_COLUMN = "/OneColumn"
    TWO_LEFT = "/TwoColumnLeft"
    TWO_RIGHT = "/TwoColumnRight"
    TWO_PAGE_LEFT = "/TwoPageLeft"
    TWO_PAGE_RIGHT = "/TwoPageRight"


class AFRelationship(StrEnum):  # the `/AFRelationship` an embedded source file declares for PDF/A-3
    SOURCE = "/Source"
    DATA = "/Data"
    ALTERNATIVE = "/Alternative"
    SUPPLEMENT = "/Supplement"
    UNSPECIFIED = "/Unspecified"


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class EgressFault:
    # the closed ADMISSION vocabulary `of` produces; arm-level provider raises (`pikepdf.PdfError`, `pymupdf.FileDataError`,
    # `msoffcrypto.DecryptionError`) convert to the runtime `BoundaryFault` at the `async_boundary` capsule, never into this vocabulary.
    tag: Literal["payload", "empty", "incomplete"] = tag()
    payload: tuple[str, ...] = case()  # the rejected payload key paths
    empty: None = case()  # an empty finishing chain
    incomplete: EgressStep = case()  # a step admitted without its required material/policy


# --- [CONSTANTS] ------------------------------------------------------------------------
_STRENGTHS: Final[Map[Strength, tuple[int, bool]]] = Map.of_seq([
    (Strength.RC4_40, (2, False)),
    (Strength.RC4_128, (4, False)),
    (Strength.AES_128, (4, True)),
    (Strength.AES_256, (6, True)),
])
_FITS: Final[Map[FitMode, str]] = Map.of_seq([  # the FitMode -> `generic.Fit` factory the OUTLINE node projects
    (FitMode.FIT, "fit"),
    (FitMode.FIT_H, "fit_horizontally"),
    (FitMode.FIT_V, "fit_vertically"),
    (FitMode.FIT_B, "fit_box"),
    (FitMode.XYZ, "xyz"),
])


# --- [MODELS] ---------------------------------------------------------------------------
class Permissions(Struct, frozen=True):
    accessibility: bool = True
    extract: bool = False
    modify: bool = False
    print_lowres: bool = True
    print_highres: bool = False
    annotate: bool = False
    fill_forms: bool = False
    assemble: bool = False

    def to_pikepdf(self) -> "pikepdf.Permissions":
        return pikepdf.Permissions(
            accessibility=self.accessibility,
            extract=self.extract,
            modify_other=self.modify,
            modify_annotation=self.annotate,
            modify_assembly=self.assemble,
            modify_form=self.fill_forms,
            print_lowres=self.print_lowres,
            print_highres=self.print_highres,
        )


class Encryption(Struct, frozen=True):
    owner: str
    user: str = ""
    strength: Strength = Strength.AES_256
    encrypt_metadata: bool = True

    @property
    def r(self) -> int:
        return _STRENGTHS[self.strength][0]

    @property
    def aes(self) -> bool:
        return _STRENGTHS[self.strength][1]

    @property
    def metadata(self) -> bool:
        return self.aes and self.encrypt_metadata  # qpdf rejects metadata encryption below AES; the RC4 cell is plaintext


class Bookmark(Struct, frozen=True):
    bold_top: bool = True
    italic: bool = False
    color: tuple[float, float, float] | None = None
    open_depth: int = 1
    fit: FitMode = FitMode.FIT
    fallback: tuple[tuple[str, int], ...] = ()  # flat `(title, page)` outline lowered when no `DocumentNode` tree is supplied

    def style(self, level: int, /) -> dict[str, object]:
        return {
            "bold": level == 1 and self.bold_top,
            "italic": self.italic,
            "color": self.color,
            "fit": getattr(Fit, _FITS[self.fit])(),
            "is_open": level < self.open_depth,
        }


class Watermark(Struct, frozen=True):
    under: bool = False
    rect: tuple[float, float, float, float] | None = None  # the target placement box; None stamps the whole page


class Attachment(Struct, frozen=True):
    name: str = ""
    description: str = ""
    mime: str = ""
    relationship: AFRelationship = AFRelationship.UNSPECIFIED


class Imposition(Struct, frozen=True):
    across: int = 2
    down: int = 1
    sheet: tuple[float, float] = (612.0, 792.0)
    layout: ImposeLayout = ImposeLayout.NUP

    @property
    def slots(self) -> int:
        return self.across * self.down

    def order(self, count: int, /) -> tuple[int, ...]:
        # the source-page index sequence per sheet cell: `NUP` is sequential, `BOOKLET` is the
        # saddle-stitch 2-up fold padded to a multiple of four with `-1` blanks the placer skips.
        if self.layout is ImposeLayout.NUP:
            return tuple(range(count))
        padded = -(-count // 4) * 4
        folded = (page for i in range(padded // 4) for page in (padded - 1 - 2 * i, 2 * i, 2 * i + 1, padded - 2 - 2 * i))
        return tuple(page if page < count else -1 for page in folded)


class Viewer(Struct, frozen=True):
    page_mode: PageMode = PageMode.NONE
    page_layout: PageLayout = PageLayout.NONE
    hide_toolbar: bool = False
    fit_window: bool = False
    center_window: bool = False
    display_doctitle: bool = True


class Forms(Struct, frozen=True):
    # `flatten` bakes filled widgets into static content (irreversible) and a `values`-empty flatten bakes existing field state;
    # `need_appearances` regenerates `/AP` streams and is meaningful only when NOT flattening, since a flatten bakes them.
    values: frozendict[str, str] = field(default_factory=frozendict)
    flatten: bool = True
    need_appearances: bool = False


class Scrub(Struct, frozen=True):
    metadata: bool = True
    xml_metadata: bool = True
    attached_files: bool = True
    embedded_files: bool = True
    hidden_text: bool = True
    javascript: bool = True
    remove_links: bool = False
    reset_fields: bool = False
    reset_responses: bool = False
    thumbnails: bool = False
    clean_pages: bool = True
    redact_images: int = 0

    def kwargs(self) -> dict[str, bool | int]:
        return {**structs.asdict(self), "redactions": False}


class Label(Struct, frozen=True):
    fontname: str = "helv"
    fontsize: float = 11.0
    align: int = 0
    fill: tuple[float, float, float] = (0.0, 0.0, 0.0)  # the burned-region fill colour
    text_color: tuple[float, float, float] = (1.0, 1.0, 1.0)  # the overlay-text colour
    cross_out: bool = True  # struck-through default when no overlay text rides the rect
    overlay_text: str = ""  # the label burned into every redacted region
    flatten_widgets: bool = True  # bake form widgets during the redaction `bake`
    needles: tuple[str, ...] = ()  # content-search terms resolved to rects through `Page.search_for`

    def annot(self) -> dict[str, object]:
        # the full `add_redact_annot` typography axis; a non-empty `overlay_text` suppresses the strike-out
        overlay = self.overlay_text or None
        return {
            "fontname": self.fontname,
            "fontsize": self.fontsize,
            "align": self.align,
            "fill": self.fill,
            "text_color": self.text_color,
            "cross_out": self.cross_out and overlay is None,
            "text": overlay,
        }


class Sanitize(Struct, frozen=True):
    javascript: bool = True
    external_access: bool = True
    multimedia: bool = True
    attachments: bool = False
    private_app_data: bool = True
    flatten_annotations: bool = True
    disable_signatures: bool = False
    prune: frozenset[PruneClass] = frozenset()  # document-wide object-class strip pikepdf.sanitize cannot; empty = pikepdf-only single pass


class RunningContent(Struct, frozen=True):
    # `threshold` is the page fraction a block must recur on to count as running content (0.8 = a header on four of five pages);
    # each sweep returns its removed-block count, and the per-page `erase_header`/`erase_footer` geometric arms stay unused.
    headers: bool = True
    footers: bool = True
    artifacts: bool = False
    threshold: float = 0.8


class Optimize(Struct, frozen=True):
    linearize: bool = True
    sweep_unreferenced: bool = True
    recompress: bool = True
    deterministic_id: bool = True
    object_streams: bool = True
    compress_streams: bool = True
    stream_decode: StreamDecode = StreamDecode.GENERALIZED
    flate_level: int = -1  # pikepdf `settings.set_flate_compression_level`; -1 keeps the zlib default, 0..9 tunes the recompress strength
    verify_syntax: bool = False  # capture qpdf `Pdf.check_pdf_syntax` warnings onto `FinishFact.syntax_warnings`
    dedup_objects: bool = False  # gated pypdf `compress_identical_objects` object-table dedup/GC pass beside the pikepdf sweep
    incremental: bool = False  # append-only signature-preserving write, mutually exclusive with the pikepdf recompress rewrite


class ContentEdit(Struct, frozen=True):
    drop_operators: frozenset[str] = frozenset()
    rename_resources: frozendict[str, str] = field(default_factory=frozendict)
    strip_layers: frozenset[str] = frozenset()
    flatten_layers: frozenset[str] = frozenset()

    @property
    def touches_layers(self) -> bool:
        return bool(self.strip_layers or self.flatten_layers)

    def layer_mode(self, name: str, /) -> LayerMode:
        return LayerMode.STRIP if name in self.strip_layers else LayerMode.FLATTEN if name in self.flatten_layers else LayerMode.KEEP


@tagged_union(frozen=True)
class Confidentiality:
    # the Office disposition: `unlock` an encrypted container through the format-discriminated
    # credential axis, or `reseal` a plaintext OOXML payload under a fresh agile container; only
    # the OOXML object carries `encrypt`, so a 97-container re-seal is unrepresentable by case.
    tag: Literal["unlock", "reseal"] = tag()
    unlock: bool = case()  # `verify` — fold the HMAC/password verification into load+decrypt
    reseal: str = case()  # the re-seal password


class FinishFact(Struct, frozen=True):
    data: bytes
    pages: int = 0
    encryption_r: int = 0
    outline_depth: int = 0
    overlays: int = 0  # content-composition op count: overlay placements, OCG strips, baked form fills, stripped running content
    layers_removed: int = 0
    fields_filled: int = 0
    content_stripped: int = 0  # STRIP running-header/footer/artifact blocks pdf_oxide repeat-detection removed
    syntax_warnings: int = 0  # OPTIMIZE qpdf `check_pdf_syntax` warning count — carrier-only evidence, not a receipt slot


class Finishing(Struct, frozen=True):
    permissions: Permissions = Permissions()
    encryption: Encryption | None = None
    bookmark: Bookmark = Bookmark()
    watermark: Watermark = Watermark()
    attachment: Attachment = Attachment()
    imposition: Imposition = Imposition()
    viewer: Viewer = Viewer()
    forms: Forms = Forms()
    scrub: Scrub = Scrub()
    label: Label = Label()
    running: RunningContent = RunningContent()
    sanitize: Sanitize = Sanitize()
    optimize: Optimize = Optimize()
    edit: ContentEdit = ContentEdit()
    confidentiality: Confidentiality = field(default_factory=lambda: Confidentiality(unlock=True))


class Extras(Struct, frozen=True, omit_defaults=True):
    stamp: bytes = b""  # the WATERMARK stamp PDF bytes
    attachment_data: bytes = b""  # the ATTACH embedded-file payload
    credentials: frozendict[str, str] = field(default_factory=frozendict)  # the PROTECT `keyTypes` credential axis


# --- [BOUNDARIES] -----------------------------------------------------------------------
class EgressPayload(TypedDict, extra_items=str):
    stamp: NotRequired[ReadOnly[bytes]]
    attachment_data: NotRequired[ReadOnly[bytes]]


_PAYLOAD: Final = TypeAdapter(EgressPayload)
_DECLARED: Final[frozenset[str]] = EgressPayload.__optional_keys__ | EgressPayload.__required_keys__


# --- [SERVICES] -------------------------------------------------------------------------
class Finisher(Struct, frozen=True):
    arm: Callable[["DocumentEgress"], FinishFact]  # the default (richest) arm, run under `AGPL_MAX`
    office: bool = False
    permissive: Callable[["DocumentEgress"], FinishFact] | None = (
        None  # the MIT/Apache/BSD arm `PERMISSIVE` selects; `None` = the one arm is already permissive
    )


class DocumentEgress(Struct, frozen=True):
    step: EgressStep | tuple[EgressStep, ...]
    source: bytes
    node: DocumentNode | None = None
    finishing: Finishing = field(default_factory=Finishing)
    extras: Extras = field(default_factory=Extras)
    license: LicenseLane = LicenseLane.AGPL_MAX
    fact: FinishFact | None = None

    @property
    def steps(self) -> tuple[EgressStep, ...]:
        return self.step if isinstance(self.step, tuple) else (self.step,)

    def _stepped(self, step: EgressStep, /) -> Self:
        staged = structs.replace(self, step=step)
        finisher = FINISHERS[step]
        arm = finisher.permissive if self.license is LicenseLane.PERMISSIVE and finisher.permissive is not None else finisher.arm
        fact = arm(staged)
        return structs.replace(staged, fact=fact, source=fact.data)

    def finished(self) -> Self:
        return reduce(lambda live, step: live._stepped(step), self.steps, self)

    def emit(self, /) -> ArtifactWork:
        return ArtifactWork(key=self._key, work=self._emit, parents=self._parents, admission=Admission(keyed=None), cost=float(len(self.source)))

    @property
    def _key(self) -> ContentKey:
        return ContentIdentity.of(f"egress-{self.steps[-1]}", (self.steps, self.source, self.finishing, self.extras, self.license), policy=CANONICAL_POLICY)

    @property
    def _parents(self) -> tuple[ContentKey, ...]:
        # the upstream emit-produced document rides as bytes; a caller that holds the producer's node key threads it here
        return ()

    @receipted(OPEN)  # egress facts carry no classified field, so the runtime keep-all `OPEN` policy rides directly, never a re-minted per-file `Redaction`
    async def _finished(self) -> Self:
        # heavy GIL-releasing native work crosses the runtime thread lane, never a folder-minted limiter.
        crossed = await LanePolicy.offload(self.finished, modality=Modality.THREAD, retry=RetryClass.OCCT)
        return crossed.default_with(lambda fault: _egress_raise(fault))

    async def _emit(self) -> RuntimeRail[ArtifactReceipt]:
        # the terminal receipt threads the PRE-RUN input key so receipt.slot == node.key.
        return (await async_boundary(f"egress.{'+'.join(self.steps)}", self._finished)).map(lambda live: live._receipt(self._key))

    def _receipt(self, key: ContentKey, /) -> ArtifactReceipt:
        fact = self.fact if self.fact is not None else FinishFact(data=self.source)
        return (
            ArtifactReceipt.Office(key, len(fact.data))
            if FINISHERS[self.steps[-1]].office
            else ArtifactReceipt.Egress(
                key,
                len(fact.data),
                fact.pages,
                fact.encryption_r,
                fact.outline_depth,
                fact.overlays + fact.layers_removed + fact.fields_filled + fact.content_stripped,
            )
        )

    def contribute(self) -> Iterable[Receipt]:
        if self.fact is None:  # contribute rides the finished owner the fold returned, never the pre-fold seed
            return
        yield from self._receipt(self._key).contribute()

    @classmethod
    def of(
        cls,
        step: EgressStep | tuple[EgressStep, ...],
        source: bytes,
        /,
        *,
        finishing: Finishing = Finishing(),
        node: DocumentNode | None = None,
        **raw: Unpack[EgressPayload],
    ) -> Result[Self, EgressFault]:
        if isinstance(step, tuple) and not step:
            return Error(EgressFault(empty=None))
        try:
            payload = _PAYLOAD.validate_python(raw)
        except ValidationError as fault:
            return Error(EgressFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        credentials = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        known = {name: value for name, value in payload.items() if name in _DECLARED}
        candidate = cls(step=step, source=source, node=node, finishing=finishing, extras=Extras(credentials=credentials, **known))
        missing = next((member for member in candidate.steps if member in _PREREQ and not _PREREQ[member](candidate)), None)
        return Error(EgressFault(incomplete=missing)) if missing is not None else Ok(candidate)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _egress_raise(fault: object) -> "DocumentEgress":
    # terminal collapse at the finishing boundary: an offload fault reconstructs the raise the node's rail folds.
    raise ValueError(str(fault))


def _sections(node: DocumentNode | None, /) -> Iterator[SectionNode]:
    return (n for n in walk(node) if isinstance(n, SectionNode)) if node is not None else iter(())


def _redaction_rects(node: DocumentNode | None, /) -> Map[int, tuple[tuple[float, float, float, float], ...]]:
    annots = (n for n in (walk(node) if node is not None else ()) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.REDACTION)
    return Block.of_seq(annots).fold(lambda acc, a: acc.change(a.meta.page, lambda cur: Some((*cur.default_value(()), a.target))), Map.empty())


def _encrypt(egress: DocumentEgress) -> FinishFact:
    enc, sink = egress.finishing.encryption, BytesIO()
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        pdf.save(
            sink,
            linearize=True,
            encryption=pikepdf.Encryption(
                owner=enc.owner, user=enc.user, R=enc.r, aes=enc.aes, metadata=enc.metadata, allow=egress.finishing.permissions.to_pikepdf()
            ),
        )
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), encryption_r=enc.r)


type _Outline = tuple[frozendict[int, "IndirectObject"], int]


def _outline(egress: DocumentEgress) -> FinishFact:
    bookmark = egress.finishing.bookmark
    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))

    def author(state: _Outline, section: SectionNode, /) -> _Outline:  # the boundary `add_outline_item` is the seam; the parent/depth thread is pure
        parents, depth = state
        node = writer.add_outline_item(
            "".join(run.text for run in section.heading), section.meta.page, parent=parents.get(section.level - 1), **bookmark.style(section.level)
        )
        return parents | {section.level: node}, max(depth, section.level)

    _parents, depth = Block.of_seq(_sections(egress.node)).fold(author, (frozendict(), 0))
    if egress.node is None:
        for title, page in bookmark.fallback:
            writer.add_outline_item(title, page)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), outline_depth=depth)


def _watermark(egress: DocumentEgress) -> FinishFact:
    wm = egress.finishing.watermark
    with pikepdf.open(BytesIO(egress.source)) as pdf, pikepdf.open(BytesIO(egress.extras.stamp)) as stamp:  # both closed deterministically
        mark = pikepdf.Page(stamp.pages[0])
        mark.contents_coalesce()
        rect = pikepdf.Rectangle(*wm.rect) if wm.rect is not None else None
        place = pikepdf.Page.add_underlay if wm.under else pikepdf.Page.add_overlay
        overlays = sum(place(pikepdf.Page(page), mark, rect) is not None for page in pdf.pages)  # add_overlay returns the placement `Name`
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), overlays=overlays)


def _attach(egress: DocumentEgress) -> FinishFact:
    att = egress.finishing.attachment
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        pdf.attachments[att.name] = pikepdf.AttachedFileSpec(
            pdf,
            egress.extras.attachment_data,
            filename=att.name,
            description=att.description,
            mime_type=att.mime,
            relationship=pikepdf.Name(att.relationship.value),
        )
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages))


def _impose(egress: DocumentEgress) -> FinishFact:
    imp = egress.finishing.imposition
    reader = PdfReader(BytesIO(egress.source))
    width, height = imp.sheet
    cell_w, cell_h = width / imp.across, height / imp.down
    writer = PdfWriter()

    def placed(
        sheet: "PageObject", indexed: tuple[int, int], /
    ) -> "PageObject":  # `add_transformation`/`merge_page` mutate in place at the pypdf seam
        offset, index = indexed
        if index < 0:  # a booklet pad slot keeps the cell blank
            return sheet
        row, col = divmod(offset, imp.across)
        source = reader.pages[index]
        source.add_transformation(
            Transformation()
            .scale(cell_w / source.mediabox.width, cell_h / source.mediabox.height)
            .translate(col * cell_w, (imp.down - 1 - row) * cell_h)
        )
        sheet.merge_page(source)
        return sheet

    def imposed(window: tuple[int, ...], /) -> object:
        return Block.of_seq(enumerate(window)).fold(placed, writer.add_blank_page(width=width, height=height))

    for window in batched(imp.order(len(reader.pages)), imp.slots):
        imposed(window)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


def _navigate(egress: DocumentEgress) -> FinishFact:
    view = egress.finishing.viewer
    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))
    writer.page_layout = view.page_layout.value
    writer.page_mode = view.page_mode.value
    prefs = writer.create_viewer_preferences()  # binds a `/ViewerPreferences` dict to the catalog; attribute sets persist
    prefs.hide_toolbar, prefs.fit_window, prefs.center_window, prefs.display_doctitle = (
        view.hide_toolbar,
        view.fit_window,
        view.center_window,
        view.display_doctitle,
    )
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages))


def _forms(egress: DocumentEgress) -> FinishFact:
    forms = egress.finishing.forms
    writer = PdfWriter(clone_from=PdfReader(BytesIO(egress.source)))
    # `page=None` fills across every page; `auto_regenerate=False` leaves the NeedAppearances flag to the explicit set below;
    # `flatten` bakes the filled widgets into static content so a `values`-empty flatten still bakes the existing field state.
    writer.update_page_form_field_values(None, dict(forms.values), auto_regenerate=False, flatten=forms.flatten)
    if forms.need_appearances and not forms.flatten:  # a flatten already bakes the appearances this flag would regenerate
        writer.set_need_appearances_writer(True)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), fields_filled=len(forms.values))


type _Instr = tuple[list[object], object]
type _Fold = tuple[tuple[LayerMode, ...], Block[_Instr]]


def _folded_stream(page: "pikepdf.Page", edit: ContentEdit) -> bytes:
    # the carried `(stack, kept)` threads the `/OC` BDC/BMC…EMC marked-content stack immutably: a STRIP layer's whole span is
    # omitted, a FLATTEN layer keeps its body without its markers, and every surviving operand renames `Name`-guarded only.
    rename = {pikepdf.Name(f"/{k}"): pikepdf.Name(f"/{v}") for k, v in edit.rename_resources.items()}

    def step(state: _Fold, instr: _Instr, /) -> _Fold:
        # a KEEP-mode marker survives; a STRIP/FLATTEN BDC/EMC marker is dropped (the FLATTEN body
        # then passes the STRIP-only guard below, the STRIP body is dropped by it).
        stack, kept = state
        operands, op = instr
        token = str(op)
        if token in ("BDC", "BMC"):
            name = _oc_name(page, operands) if operands and str(operands[0]) == "/OC" else ""
            mode = edit.layer_mode(name) if name else LayerMode.KEEP
            return (*stack, mode), (kept.append(Block.singleton(instr)) if mode is LayerMode.KEEP else kept)
        if token == "EMC":
            mode = stack[-1] if stack else LayerMode.KEEP
            return stack[:-1], (kept.append(Block.singleton(instr)) if mode is LayerMode.KEEP else kept)
        if any(m is LayerMode.STRIP for m in stack) or token in edit.drop_operators:
            return stack, kept
        renamed = [rename.get(tok, tok) if isinstance(tok, pikepdf.Name) else tok for tok in operands]
        return stack, kept.append(Block.singleton((renamed, op)))

    _residual, kept = Block.of_seq(pikepdf.parse_content_stream(page)).fold(step, ((), Block.empty()))
    return pikepdf.unparse_content_stream(list(kept))


def _oc_name(page: "pikepdf.Page", operands: list[object]) -> str:
    marker = operands[1] if len(operands) > 1 else None
    if isinstance(marker, pikepdf.Dictionary):
        return str(marker.get(pikepdf.Name.Name, ""))
    properties = page.obj.get(pikepdf.Name.Resources, pikepdf.Dictionary()).get(pikepdf.Name.Properties, pikepdf.Dictionary())
    ocg = properties.get(pikepdf.Name(str(marker)), pikepdf.Dictionary()) if marker is not None else pikepdf.Dictionary()
    target = ocg.get(pikepdf.Name.OCGs, ocg) if str(ocg.get(pikepdf.Name.Type, "")) == "/OCMD" else ocg
    return str(target.get(pikepdf.Name.Name, ""))


def _strip_ocg_catalog(pdf: "pikepdf.Pdf", removed: frozenset[str]) -> int:
    root = pdf.Root
    if pikepdf.Name.OCProperties not in root:
        return 0
    ocprops = root[pikepdf.Name.OCProperties]
    survivors = [ocg for ocg in ocprops.get(pikepdf.Name.OCGs, pikepdf.Array()) if str(ocg.get(pikepdf.Name.Name, "")) not in removed]
    cut = len(ocprops.get(pikepdf.Name.OCGs, pikepdf.Array())) - len(survivors)
    ocprops[pikepdf.Name.OCGs] = pikepdf.Array(survivors)
    config = ocprops.get(pikepdf.Name.D, pikepdf.Dictionary())
    for axis in (pikepdf.Name.ON, pikepdf.Name.OFF, pikepdf.Name("/Order")):
        if axis in config:
            config[axis] = pikepdf.Array([ref for ref in config[axis] if ref in survivors])
    return cut


def _rewrite(egress: DocumentEgress) -> FinishFact:
    edit = egress.finishing.edit
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        for page in pdf.pages:
            page.obj[pikepdf.Name.Contents] = pdf.make_stream(_folded_stream(page, edit))
        layers = _strip_ocg_catalog(pdf, edit.strip_layers) if edit.touches_layers else 0
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), layers_removed=layers)


def _redact(egress: DocumentEgress) -> FinishFact:
    # pymupdf owns needle content-search (`Page.search_for`) and the overlay-text/cross-out burn-in the permissive arm cannot express.
    label, scrub = egress.finishing.label, egress.finishing.scrub
    with pymupdf.open(stream=egress.source, filetype="pdf") as doc:  # deterministic close, never GC-reaped
        marks, tree_rects = label.annot(), _redaction_rects(egress.node)
        for index in range(doc.page_count):
            page = doc[index]
            hits = [
                *(pymupdf.Rect(rect) for rect in tree_rects.try_find(index).default_value(())),
                *(rect for needle in label.needles for rect in page.search_for(needle)),
            ]
            for rect in hits:
                page.add_redact_annot(rect, **marks)
            if hits:
                page.apply_redactions(
                    images=pymupdf.PDF_REDACT_IMAGE_REMOVE,
                    graphics=pymupdf.PDF_REDACT_LINE_ART_REMOVE_IF_TOUCHED,
                    text=pymupdf.PDF_REDACT_TEXT_REMOVE,
                )
        doc.bake(annots=True, widgets=label.flatten_widgets)
        doc.scrub(**scrub.kwargs())
        doc.subset_fonts(fallback=False)
        sink = BytesIO()
        doc.save(sink, garbage=4, deflate=True, clean=True)
        return FinishFact(sink.getvalue(), pages=doc.page_count)


def _redact_oxide(egress: DocumentEgress) -> FinishFact:
    # needle content-search stays AGPL_MAX-only — `PdfDocument.search_page` returns an untyped hit set, no verified rect shape to
    # burn; the Rust-core handle closes deterministically through the `with` bracket, never a GC-reaped native document.
    label, scrub, tree_rects = egress.finishing.label, egress.finishing.scrub, _redaction_rects(egress.node)
    with pdf_oxide.PdfDocument.from_bytes(egress.source) as doc:
        for index in range(doc.page_count):
            for rect in tree_rects.try_find(index).default_value(()):
                doc.add_redaction(index, rect, fill=label.fill)
        doc.apply_redactions_destructive(
            scrub_metadata=scrub.metadata, remove_javascript=scrub.javascript, remove_embedded_files=scrub.embedded_files
        )
        return FinishFact(doc.to_bytes(), pages=doc.page_count)


def _strip(egress: DocumentEgress) -> FinishFact:
    # repeat-detection strips content recurring on `threshold` of pages (running headers/footers, page-number artifacts);
    # the single arm is already permissive, so STRIP declares no `Finisher.permissive` alternate.
    rc = egress.finishing.running
    with pdf_oxide.PdfDocument.from_bytes(egress.source) as doc:
        removed = (
            (doc.remove_headers(threshold=rc.threshold) if rc.headers else 0)
            + (doc.remove_footers(threshold=rc.threshold) if rc.footers else 0)
            + (doc.remove_artifacts(threshold=rc.threshold) if rc.artifacts else 0)
        )
        return FinishFact(doc.to_bytes(), pages=doc.page_count, content_stripped=removed)


def _sanitize(egress: DocumentEgress) -> FinishFact:
    pol = egress.finishing.sanitize
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        strips = (
            (pol.javascript, sanitize.remove_javascript),
            (pol.external_access, sanitize.remove_external_access),
            (pol.multimedia, sanitize.remove_multimedia),
            (pol.attachments, sanitize.remove_attachments),
            (pol.private_app_data, sanitize.remove_private_app_data),
        )
        for _gate, strip in (row for row in strips if row[0]):
            strip(pdf)
        if pol.disable_signatures:
            pdf.acroform.disable_digital_signatures()
        if pol.flatten_annotations:
            pdf.flatten_annotations(mode="all")
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        scrubbed, pages = sink.getvalue(), len(pdf.pages)
    if not pol.prune:  # the pikepdf active-content scrub is the single pass; the object-class strip is the gated pypdf second pass
        return FinishFact(scrubbed, pages=pages)
    # pypdf owns document-wide object-class removal (`remove_links`/`remove_annotations`/`remove_images`/`remove_text`)
    # that pikepdf.sanitize does not; the second pass runs only when an explicit `PruneClass` set is admitted.
    writer = PdfWriter(clone_from=PdfReader(BytesIO(scrubbed)))
    for cut in pol.prune:
        _PRUNE[cut](writer)
    pruned = BytesIO()
    writer.write(pruned)
    return FinishFact(pruned.getvalue(), pages=len(writer.pages))


def _sanitize_oxide(egress: DocumentEgress) -> FinishFact:
    # one self-contained Rust-core pass pulling neither pikepdf nor pypdf; the `PruneClass` document-wide object strip stays an
    # `AGPL_MAX`-lane pypdf concern — this arm covers the active-content + annotation-flatten scrub the permissive lane needs.
    pol = egress.finishing.sanitize
    with pdf_oxide.PdfDocument.from_bytes(egress.source) as doc:
        doc.sanitize_document(scrub_metadata=pol.private_app_data, remove_javascript=pol.javascript, remove_embedded_files=pol.attachments)
        if pol.flatten_annotations:
            doc.flatten_all_annotations()
        return FinishFact(doc.to_bytes(), pages=doc.page_count)


def _optimize(egress: DocumentEgress) -> FinishFact:
    pol = egress.finishing.optimize
    if pol.incremental:
        # append-only signature-preserving write: original bytes plus one incremental xref delta — a recompress breaks an existing
        # `/Sig`, so an already-signed deliverable never touches the pikepdf recompress path below.
        writer = PdfWriter(BytesIO(egress.source), incremental=True)
        sink = BytesIO()
        writer.write(sink)
        return FinishFact(sink.getvalue(), pages=len(writer.pages))
    if pol.flate_level >= 0:
        pikepdf.settings.set_flate_compression_level(
            pol.flate_level
        )  # the global zlib level driving the recompress strength below, off the fixed flag alone
    with pikepdf.open(BytesIO(egress.source)) as pdf:  # deterministic close, never GC-reaped
        warnings = (
            len(pdf.check_pdf_syntax()) if pol.verify_syntax else 0
        )  # qpdf syntax warnings captured as evidence, never a raise into domain flow
        if pol.sweep_unreferenced:
            pdf.remove_unreferenced_resources()
        sink = BytesIO()
        pdf.save(
            sink,
            linearize=pol.linearize,
            compress_streams=pol.compress_streams,
            stream_decode_level=pikepdf.StreamDecodeLevel[pol.stream_decode.value],
            recompress_flate=pol.recompress,
            deterministic_id=pol.deterministic_id,
            object_stream_mode=pikepdf.ObjectStreamMode.generate if pol.object_streams else pikepdf.ObjectStreamMode.preserve,
        )
        recompressed, pages = sink.getvalue(), len(pdf.pages)
    if not pol.dedup_objects:  # the pikepdf stream recompress is the single pass; the object-table dedup is the gated pypdf second pass
        return FinishFact(recompressed, pages=pages, syntax_warnings=warnings)
    # pypdf owns object-table dedup/GC (`compress_identical_objects`) the pikepdf stream recompress does not:
    # the second pass merges identical object definitions and drops the unreferenced tail for a further size cut.
    writer = PdfWriter(clone_from=PdfReader(BytesIO(recompressed)))
    writer.compress_identical_objects(remove_duplicates=True, remove_unreferenced=True)
    deduped = BytesIO()
    writer.write(deduped)
    return FinishFact(deduped.getvalue(), pages=len(writer.pages), syntax_warnings=warnings)


def _protect(egress: DocumentEgress) -> FinishFact:
    office = msoffcrypto.OfficeFile(BytesIO(egress.source))
    sink = BytesIO()
    match egress.finishing.confidentiality:
        case Confidentiality(tag="reseal", reseal=password):  # the inverse rail — only the OOXML object carries `encrypt`
            if office.is_encrypted():  # an already-sealed container is the idempotent terminal, never a double-seal
                return FinishFact(egress.source)
            office.encrypt(password, sink)
        case Confidentiality(tag="unlock", unlock=verify):
            if not office.is_encrypted():  # an already-plaintext container is the idempotent unlock terminal
                return FinishFact(egress.source)
            ooxml = getattr(office, "format", "") == "ooxml"  # `format` is the factory-set discriminant; `keyTypes` is absent on 97/extensible/plain
            key_axis = getattr(office, "keyTypes", ("password",))
            credentials = {kind: egress.extras.credentials[kind] for kind in key_axis if kind in egress.extras.credentials}
            office.load_key(**credentials, **({"verify_password": True} if ooxml and verify else {}))
            office.decrypt(sink, **({"verify_integrity": verify} if ooxml else {}))
        case _ as unreachable:
            assert_never(unreachable)
    return FinishFact(sink.getvalue())


# --- [COMPOSITION] ----------------------------------------------------------------------
FINISHERS: Final[Map[EgressStep, Finisher]] = Map.of_seq([
    (EgressStep.ENCRYPT, Finisher(_encrypt)),
    (EgressStep.OUTLINE, Finisher(_outline)),
    (EgressStep.WATERMARK, Finisher(_watermark)),
    (EgressStep.ATTACH, Finisher(_attach)),
    (EgressStep.IMPOSE, Finisher(_impose)),
    (EgressStep.NAVIGATE, Finisher(_navigate)),
    (EgressStep.FORMS, Finisher(_forms)),  # pypdf (BSD) already permissive — no `permissive` arm needed
    (EgressStep.REWRITE, Finisher(_rewrite)),
    (EgressStep.REDACT, Finisher(_redact, permissive=_redact_oxide)),  # AGPL pymupdf default, MIT/Apache pdf_oxide under `PERMISSIVE`
    (EgressStep.STRIP, Finisher(_strip)),  # pdf_oxide (MIT/Apache) — the single arm is already permissive
    (EgressStep.SANITIZE, Finisher(_sanitize, permissive=_sanitize_oxide)),  # pikepdf default, self-contained pdf_oxide under `PERMISSIVE`
    (EgressStep.OPTIMIZE, Finisher(_optimize)),
    (EgressStep.PROTECT, Finisher(_protect, office=True)),
])
# the SANITIZE object-class prune dispatch: one bound pypdf document-wide remover per `PruneClass`;
# `remove_annotations(None)` strips every annotation subtype, the doc-wide supersets `remove_links` folds.
_PRUNE: Final[Map[PruneClass, Callable[["PdfWriter"], None]]] = Map.of_seq([
    (PruneClass.LINKS, lambda writer: writer.remove_links()),
    (PruneClass.ANNOTATIONS, lambda writer: writer.remove_annotations(None)),
    (PruneClass.IMAGES, lambda writer: writer.remove_images()),
    (PruneClass.TEXT, lambda writer: writer.remove_text()),
])
# the per-step admission prerequisite: `of` rejects a step whose required owner is absent into
# `EgressFault.incomplete` so the in-process fold is total — steps omitted here are always ready.
_PREREQ: Final[Map[EgressStep, Callable[[DocumentEgress], bool]]] = Map.of_seq([
    (EgressStep.ENCRYPT, lambda eg: eg.finishing.encryption is not None),
    (EgressStep.WATERMARK, lambda eg: bool(eg.extras.stamp)),
    (EgressStep.ATTACH, lambda eg: bool(eg.extras.attachment_data and eg.finishing.attachment.name)),
    (EgressStep.OUTLINE, lambda eg: eg.node is not None or bool(eg.finishing.bookmark.fallback)),
    (EgressStep.FORMS, lambda eg: bool(eg.finishing.forms.values) or eg.finishing.forms.flatten),
    (EgressStep.REDACT, lambda eg: eg.node is not None or bool(eg.finishing.label.needles)),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

- [OXIDE_SEARCH]-[OPEN]: does `pdf_oxide.PdfDocument.search_page` return typed rects usable for a permissive-lane needle redaction; verify against the folder `.api/pdf-oxide.md` catalog.
