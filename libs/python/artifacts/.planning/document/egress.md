# [PY_ARTIFACTS_EGRESS]

Security-and-navigation finishing closes over an emitted PDF or Office container: `DocumentEgress` takes bytes authored by `document/emit#DOCUMENT` and returns a sealed, watermarked, redaction-burned, imposed, form-baked, content-rewritten, or Office-(de)sealed artifact keyed by the runtime content key — it finishes an emitted artifact and never authors one. Re-signing routes to `exchange/conformance#CONFORMANCE`, PDF/A authoring to `document/emit#DOCUMENT`, named-layer authoring to `export/layered#LAYERED`, descriptive-metadata authoring to `exchange/metadata#METADATA`.

## [01]-[INDEX]

- [02]-[FINISH]: one finishing close discriminating `EgressStep` over the `FINISHERS` totality table under the `LicenseLane` footing.

## [02]-[FINISH]

- Owner: `DocumentEgress` — one `Finisher.arm` per step resolved off `FINISHERS`, never an `if step ==` cascade or a worker-side `match`; `LicenseLane` is read once at the value in `_stepped`, never a per-call knob. `Finishing` bundles every trusted policy value object while `Extras` carries the untrusted material (stamp bytes, attachment payload, Office credentials) — the admission split is trust, not concern, so passwords never ride the untrusted payload and stamp bytes never ride a trusted default.
- Entry: `of` admits untrusted material exactly once through the `EgressPayload` `TypedDict`, its `extra_items=str` band folding the format-discriminated Office credential axis into `Extras.credentials`, and rejects an under-supplied step through `_PREREQ` into `EgressFault.incomplete` before the fold runs — the interior is total over admitted owners and never re-validates; material the selected footing's arm cannot express (a needle-bearing REDACT, an active-content SANITIZE demand, an ENCRYPT request at all under `PERMISSIVE`) refuses through the `_LANE_GAPS` predicate table as `EgressFault.lane`, never a silent drop or a weakened seal. One polymorphic entry owns both the singular step and the chain: the `EgressStep | tuple[EgressStep, ...]` discriminant threads finished bytes step-to-step through one `reduce`, never a caller-orchestrated re-entry or a `mode` knob.
- Auto: each arm returns a `FinishFact` merged through `FinishFact.combined`; terminal bytes and page count ride the newest fact, single-owner scalars survive by right-or-left, and additive counters sum. `pypdf` owns the structural OUTLINE/IMPOSE/NAVIGATE/FORMS arms and the gated SANITIZE/OPTIMIZE second passes; qpdf owns every encryption strength through one `pikepdf.Encryption` leg. Native packages bind as module-scope `lazy import` reified on first arm use.
- Output: `_emit` returns the settled `FinishFact`; `Metrics.record` records byte volume and `Journal.record` persists the regulatory changes held by `_diff`.
- Packages: `pikepdf` (MPL) owns the qpdf object model, encryption, composition, and save strategy; `pypdf` (BSD) the pure-Python structural arms and the `ObjectDeletionFlag` object pruner; `pymupdf` (AGPL) the richest REDACT burn-in with `search_for` needle match, flagged for supersession on the permissive lane; `pdf_oxide` (MIT/Apache) the permissive REDACT/SANITIZE arms and the STRIP running-content removal no other step owns, never an ENCRYPT arm; `msoffcrypto` the bidirectional Office confidentiality rail.
- Growth: a new finishing step is one `EgressStep` row, one `Finisher` row, and one `_PREREQ` row when it needs material; a commercial-safe alternative is one `Finisher.permissive` arm; a new policy concern is one `Finishing` field carrying its own value object; a new measured fact is one `FinishFact` field with its `combined` column; an encryption strength is one `Strength` row with its `_STRENGTHS` cell; a document-wide strip class is one `PruneClass` member with one `_PRUNE` row; a deeper chain is one more step in the sequence the rail already folds.

```python
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Callable, Iterator
from enum import StrEnum
from functools import partial, reduce
from io import BytesIO
from itertools import batched
from math import isfinite
from typing import TYPE_CHECKING, Final, Literal, NotRequired, ReadOnly, Self, TypedDict, Unpack, assert_never

import msgspec
from builtins import frozendict
from expression import Error, Ok, Result, Some, case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, field, structs
from pydantic import TypeAdapter, ValidationError

from rasm.artifacts.core.hooks import BYTE_VOLUME, DOMAIN, ArtifactKind, ArtifactsLeg
from rasm.artifacts.core.plan import Admission, ArtifactWork
from rasm.artifacts.document.model import AnnotKind, AnnotationNode, DocumentNode, Lapse, SectionNode, lapsed, node_digest, walk
from rasm.runtime.identity import ContentIdentity, ContentKey
from rasm.runtime.faults import TRANSIENT, Catch, FaultRow, RuntimeRail, async_boundary, rostered
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Cleared, Journal, Party, Retain
from rasm.runtime.lanes import LanePolicy
from rasm.runtime.metrics import Metrics
from rasm.runtime.workers import Kernel, KernelTrait

lazy import msoffcrypto
lazy import pdf_oxide
lazy import pikepdf
lazy import pymupdf
lazy from pikepdf import sanitize
lazy from pypdf import ObjectDeletionFlag, PdfReader, PdfWriter, Transformation
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
    STRIP = "strip"
    SANITIZE = "sanitize"
    OPTIMIZE = "optimize"
    PROTECT = "protect"


class LicenseLane(StrEnum):
    AGPL_MAX = "agpl-max"
    PERMISSIVE = "permissive"


class OfficeVerification(StrEnum):
    NONE = "none"
    KEY = "key"
    INTEGRITY = "integrity"
    FULL = "full"


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


class PruneClass(StrEnum):
    LINKS = "links"
    ANNOTATIONS = "annotations"
    IMAGES = "images"
    TEXT = "text"
    OBJECTS_3D = "objects-3d"
    XOBJECT_IMAGES = "xobject-images"
    INLINE_IMAGES = "inline-images"
    DRAWING_IMAGES = "drawing-images"


class FitMode(StrEnum):
    FIT = "fit"
    FIT_H = "fit_h"
    FIT_V = "fit_v"
    FIT_B = "fit_b"
    XYZ = "xyz"


class StreamDecode(StrEnum):
    NONE = "none"
    GENERALIZED = "generalized"
    SPECIALIZED = "specialized"
    ALL = "all"


class PageMode(StrEnum):
    NONE = "/UseNone"
    OUTLINES = "/UseOutlines"
    THUMBS = "/UseThumbs"
    FULLSCREEN = "/FullScreen"
    OPTIONAL_CONTENT = "/UseOC"
    ATTACHMENTS = "/UseAttachments"


class PageLayout(StrEnum):
    NONE = "/NoLayout"
    SINGLE = "/SinglePage"
    ONE_COLUMN = "/OneColumn"
    TWO_LEFT = "/TwoColumnLeft"
    TWO_RIGHT = "/TwoColumnRight"
    TWO_PAGE_LEFT = "/TwoPageLeft"
    TWO_PAGE_RIGHT = "/TwoPageRight"


class AFRelationship(StrEnum):
    SOURCE = "/Source"
    DATA = "/Data"
    ALTERNATIVE = "/Alternative"
    SUPPLEMENT = "/Supplement"
    UNSPECIFIED = "/Unspecified"


# --- [ERRORS] ---------------------------------------------------------------------------
@tagged_union(frozen=True)
class EgressFault:
    tag: Literal["payload", "empty", "incomplete", "chain", "container", "lane"] = tag()
    payload: tuple[str, ...] = case()
    empty: None = case()
    incomplete: EgressStep = case()
    chain: tuple[EgressStep, ...] = case()
    container: str = case()
    lane: tuple[EgressStep, LicenseLane] = case()


# --- [CONSTANTS] ------------------------------------------------------------------------
_KEY_ENCODER: Final = msgspec.msgpack.Encoder(order="deterministic")
_STRENGTHS: Final[Map[Strength, tuple[int, bool]]] = Map.of_seq([
    (Strength.RC4_40, (2, False)),
    (Strength.RC4_128, (4, False)),
    (Strength.AES_128, (4, True)),
    (Strength.AES_256, (6, True)),
])
_FITS: Final[Map[FitMode, str]] = Map.of_seq([
    (FitMode.FIT, "fit"),
    (FitMode.FIT_H, "fit_horizontally"),
    (FitMode.FIT_V, "fit_vertically"),
    (FitMode.FIT_B, "fit_box"),
    (FitMode.XYZ, "xyz"),
])
_VERIFICATION: Final[Map[OfficeVerification, tuple[bool, bool]]] = Map.of_seq([
    (OfficeVerification.NONE, (False, False)),
    (OfficeVerification.KEY, (True, False)),
    (OfficeVerification.INTEGRITY, (False, True)),
    (OfficeVerification.FULL, (True, True)),
])



# --- [TABLES] ---------------------------------------------------------------------------

EGRESS_FINISH: Final[FaultRow[ArtifactsLeg]] = FaultRow(
    leg=ArtifactsLeg.EGRESS, point="finish", arm="boundary", defect="finish-fold", retriability=TRANSIENT
)
RAISES: Final[Block[FaultRow[ArtifactsLeg]]] = rostered(Block.of_seq([EGRESS_FINISH]))

_FINISH_RAISES: Final[Catch] = (Lapse,)

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
        return self.aes and self.encrypt_metadata


class Bookmark(Struct, frozen=True):
    bold_top: bool = True
    italic: bool = False
    color: tuple[float, float, float] | None = None
    open_depth: int = 1
    fit: FitMode = FitMode.FIT
    fallback: tuple[tuple[str, int], ...] = ()

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
    rect: tuple[float, float, float, float] | None = None


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
    fill: tuple[float, float, float] = (0.0, 0.0, 0.0)
    text_color: tuple[float, float, float] = (1.0, 1.0, 1.0)
    cross_out: bool = True
    overlay_text: str = ""
    flatten_widgets: bool = True
    needles: tuple[str, ...] = ()

    def annot(self) -> dict[str, object]:
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
    prune: frozenset[PruneClass] = frozenset()


class RunningContent(Struct, frozen=True):
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
    flate_level: int = -1
    verify_syntax: bool = False
    dedup_objects: bool = False
    incremental: bool = False


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
    tag: Literal["unlock", "reseal"] = tag()
    unlock: OfficeVerification = case()
    reseal: str = case()


class FinishFact(Struct, frozen=True):
    data: bytes
    pages: int | None = None
    encryption_r: int | None = None
    outline_depth: int | None = None
    overlays: int = 0
    layers_removed: int = 0
    fields_filled: int = 0
    content_stripped: int = 0
    syntax_warnings: int = 0
    office_format: str = ""
    office_scheme: str = ""
    credential_kinds: tuple[str, ...] = ()
    verification: OfficeVerification | None = None

    @staticmethod
    def combined(left: "FinishFact", right: "FinishFact", /) -> "FinishFact":
        return FinishFact(
            data=right.data,
            pages=right.pages if right.pages is not None else left.pages,
            encryption_r=right.encryption_r if right.encryption_r is not None else left.encryption_r,
            outline_depth=right.outline_depth if right.outline_depth is not None else left.outline_depth,
            overlays=left.overlays + right.overlays,
            layers_removed=left.layers_removed + right.layers_removed,
            fields_filled=left.fields_filled + right.fields_filled,
            content_stripped=left.content_stripped + right.content_stripped,
            syntax_warnings=left.syntax_warnings + right.syntax_warnings,
            office_format=right.office_format or left.office_format,
            office_scheme=right.office_scheme or left.office_scheme,
            credential_kinds=right.credential_kinds or left.credential_kinds,
            verification=right.verification if right.verification is not None else left.verification,
        )


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
    confidentiality: Confidentiality = field(default_factory=lambda: Confidentiality(unlock=OfficeVerification.FULL))


class Extras(Struct, frozen=True, omit_defaults=True):
    stamp: bytes = b""
    attachment_data: bytes = b""
    credentials: frozendict[str, str] = field(default_factory=frozendict)


# --- [BOUNDARIES] -----------------------------------------------------------------------
class EgressPayload(TypedDict, extra_items=str):
    stamp: NotRequired[ReadOnly[bytes]]
    attachment_data: NotRequired[ReadOnly[bytes]]


_PAYLOAD: Final = TypeAdapter(EgressPayload)
_DECLARED: Final[frozenset[str]] = EgressPayload.__optional_keys__ | EgressPayload.__required_keys__


# --- [SERVICES] -------------------------------------------------------------------------
class Finisher(Struct, frozen=True):
    arm: Callable[["DocumentEgress"], FinishFact]
    office: bool = False
    permissive: Callable[["DocumentEgress"], FinishFact] | None = (
        None
    )


class DocumentEgress(Struct, frozen=True):
    step: EgressStep | tuple[EgressStep, ...]
    source: bytes
    lane: LanePolicy
    key: ContentKey
    node: DocumentNode | None = None
    finishing: Finishing = field(default_factory=Finishing)
    extras: Extras = field(default_factory=Extras)
    footing: LicenseLane = LicenseLane.AGPL_MAX
    fact: FinishFact | None = None
    parents: tuple[ContentKey, ...] = ()

    @property
    def steps(self) -> tuple[EgressStep, ...]:
        return self.step if isinstance(self.step, tuple) else (self.step,)

    def _stepped(self, step: EgressStep, /) -> Self:
        staged = structs.replace(self, step=step)
        finisher = FINISHERS[step]
        arm = finisher.permissive if self.footing is LicenseLane.PERMISSIVE and finisher.permissive is not None else finisher.arm
        fact = arm(staged)
        merged = fact if self.fact is None else FinishFact.combined(self.fact, fact)
        return structs.replace(staged, fact=merged, source=fact.data)

    def finished(self) -> Self:
        return reduce(lambda live, step: live._stepped(step), self.steps, self)

    def emit(self, /) -> ArtifactWork[FinishFact]:
        return ArtifactWork(key=self.key, work=partial(self._emit, self.key), parents=self.parents, admission=Admission(keyed=None), cost=float(len(self.source)))

    async def _finished(self) -> Self:
        crossed = await self.lane.offload(Kernel.of(self.finished, KernelTrait.RELEASING))
        return crossed.default_with(lapsed)

    async def _emit(self, key: ContentKey, /) -> RuntimeRail[FinishFact]:
        match await async_boundary(EGRESS_FINISH, self._finished, catch=_FINISH_RAISES):
            case Result(tag="ok", ok=live):
                fact = live._fact
                kind: ArtifactKind = "office" if FINISHERS[self.steps[-1]].office else "egress"
                audited = AuditFact(
                    action="artifacts.document.egress",
                    actor=Party(kind=Actor.SERVICE, key="artifacts"),
                    target=Party(kind=kind, key=key.hex),
                    retention=Retain.REGULATORY,
                    change=live._diff,
                )
                Metrics.record({BYTE_VOLUME: float(len(fact.data))}, domain=DOMAIN, kind=kind, scope=self.lane.scope)
                await Journal.record((audited,), scope=self.lane.scope)
                return Ok(fact)
            case refused:
                return Error(refused.error)

    @property
    def _fact(self) -> FinishFact:
        return self.fact if self.fact is not None else FinishFact(data=self.source)

    @property
    def _diff(self) -> tuple[Change, ...]:
        fact = self._fact
        return tuple(
            Cleared(path=f"/{name}", prior=str(count)) if departed else Assigned(path=f"/{name}", next=str(count))
            for name, count, departed in (
                ("content_stripped", fact.content_stripped, True),
                ("layers_removed", fact.layers_removed, True),
                ("fields_filled", fact.fields_filled, False),
            )
            if count
        )

    @classmethod
    def of(
        cls,
        step: EgressStep | tuple[EgressStep, ...],
        source: bytes,
        /,
        *,
        lane: LanePolicy,
        finishing: Finishing = Finishing(),
        node: DocumentNode | None = None,
        footing: LicenseLane = LicenseLane.AGPL_MAX,
        **raw: Unpack[EgressPayload],
    ) -> Result[Self, EgressFault]:
        if isinstance(step, tuple) and not step:
            return Error(EgressFault(empty=None))
        steps = step if isinstance(step, tuple) else (step,)
        if EgressStep.PROTECT in steps and len(steps) > 1:
            return Error(EgressFault(chain=steps))
        try:
            payload = _PAYLOAD.validate_python(raw, strict=True)
        except ValidationError as fault:
            return Error(EgressFault(payload=tuple(str(error["loc"]) for error in fault.errors())))
        credentials = frozendict({name: value for name, value in payload.items() if name not in _DECLARED})
        known = {name: value for name, value in payload.items() if name in _DECLARED}
        extras = Extras(credentials=credentials, **known)
        candidate = cls(
            step=step,
            source=source,
            lane=lane,
            key=_minted(steps, source, node, finishing, extras, footing),
            node=node,
            finishing=finishing,
            extras=extras,
            footing=footing,
        )
        if footing is LicenseLane.PERMISSIVE:
            gap = next((member for member in candidate.steps if member in _LANE_GAPS and _LANE_GAPS[member](candidate)), None)
            if gap is not None:
                return Error(EgressFault(lane=(gap, footing)))
        missing = next((member for member in candidate.steps if member in _PREREQ and not _PREREQ[member](candidate)), None)
        if missing is not None:
            return Error(EgressFault(incomplete=missing))
        if EgressStep.PROTECT not in steps:
            return Ok(candidate)
        try:
            office = msoffcrypto.OfficeFile(BytesIO(source))
        except msoffcrypto.exceptions.FileFormatError:
            return Error(EgressFault(container="unrecognized"))
        confidentiality = candidate.finishing.confidentiality
        if confidentiality.tag == "reseal" and getattr(office, "format", "") != "ooxml":
            return Error(EgressFault(container="reseal-requires-ooxml"))
        supported = tuple(getattr(office, "keyTypes", ("password",)))
        if confidentiality.tag == "unlock" and office.is_encrypted() and not any(kind in candidate.extras.credentials for kind in supported):
            return Error(EgressFault(incomplete=EgressStep.PROTECT))
        return Ok(candidate)


# --- [OPERATIONS] -----------------------------------------------------------------------
def _minted(
    steps: tuple[EgressStep, ...], source: bytes, node: DocumentNode | None, finishing: Finishing, extras: Extras, footing: LicenseLane, /
) -> ContentKey:
    spec = ContentIdentity.key(f"egress-{steps[-1]}", _KEY_ENCODER.encode((steps, source, finishing, extras, footing)))
    return spec if node is None else ContentIdentity.key(f"egress-{steps[-1]}", (spec, node_digest(node)))



def _sections(node: DocumentNode | None, /) -> Iterator[SectionNode]:
    return (n for n in walk(node) if isinstance(n, SectionNode)) if node is not None else iter(())


def _redaction_rects(node: DocumentNode | None, /) -> Map[int, tuple[tuple[float, float, float, float], ...]]:
    annots = (n for n in (walk(node) if node is not None else ()) if isinstance(n, AnnotationNode) and n.annot is AnnotKind.REDACTION)
    return Block.of_seq(annots).fold(lambda acc, a: acc.change(a.meta.page, lambda cur: Some((*cur.default_value(()), a.target))), Map.empty())


def _encrypt(egress: DocumentEgress) -> FinishFact:
    enc, sink = egress.finishing.encryption, BytesIO()
    assert enc is not None
    with pikepdf.open(BytesIO(egress.source)) as pdf:
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

    def author(state: _Outline, section: SectionNode, /) -> _Outline:
        parents, depth = state
        node = writer.add_outline_item(
            "".join(run.text for run in section.heading), section.meta.page, parent=parents.get(section.level - 1), **bookmark.style(section.level)
        )
        return parents | {section.level: node}, max(depth, section.level)

    _parents, depth = Block.of_seq(_sections(egress.node)).fold(author, (frozendict(), 0))

    def author_fallback(depth: int, item: tuple[str, int], /) -> int:
        writer.add_outline_item(*item)
        return max(depth, 1)

    depth = Block.of_seq(bookmark.fallback).fold(author_fallback, depth) if egress.node is None else depth
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), outline_depth=depth)


def _watermark(egress: DocumentEgress) -> FinishFact:
    wm = egress.finishing.watermark
    with pikepdf.open(BytesIO(egress.source)) as pdf, pikepdf.open(BytesIO(egress.extras.stamp)) as stamp:
        mark = pikepdf.Page(stamp.pages[0])
        mark.contents_coalesce()
        rect = pikepdf.Rectangle(*wm.rect) if wm.rect is not None else None
        place = pikepdf.Page.add_underlay if wm.under else pikepdf.Page.add_overlay
        overlays = sum(place(pikepdf.Page(page), mark, rect) is not None for page in pdf.pages)
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), overlays=overlays)


def _attach(egress: DocumentEgress) -> FinishFact:
    att = egress.finishing.attachment
    with pikepdf.open(BytesIO(egress.source)) as pdf:
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
    ) -> "PageObject":
        offset, index = indexed
        if index < 0:
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
    prefs = writer.create_viewer_preferences()
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
    writer.update_page_form_field_values(None, dict(forms.values), auto_regenerate=False, flatten=forms.flatten)
    if forms.need_appearances and not forms.flatten:
        writer.set_need_appearances_writer(True)
    sink = BytesIO()
    writer.write(sink)
    return FinishFact(sink.getvalue(), pages=len(writer.pages), fields_filled=len(forms.values))


type _Instr = tuple[list[object], object]
type _Fold = tuple[tuple[LayerMode, ...], Block[_Instr]]


def _folded_stream(page: "pikepdf.Page", edit: ContentEdit) -> bytes:
    rename = {pikepdf.Name(f"/{k}"): pikepdf.Name(f"/{v}") for k, v in edit.rename_resources.items()}

    def step(state: _Fold, instr: _Instr, /) -> _Fold:
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
    with pikepdf.open(BytesIO(egress.source)) as pdf:
        for page in pdf.pages:
            page.obj[pikepdf.Name.Contents] = pdf.make_stream(_folded_stream(page, edit))
        layers = _strip_ocg_catalog(pdf, edit.strip_layers) if edit.touches_layers else 0
        sink = BytesIO()
        pdf.save(sink, linearize=True)
        return FinishFact(sink.getvalue(), pages=len(pdf.pages), layers_removed=layers)


def _redact(egress: DocumentEgress) -> FinishFact:
    label, scrub = egress.finishing.label, egress.finishing.scrub
    with pymupdf.open(stream=egress.source, filetype="pdf") as doc:
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


def _needle_rects(doc: "pdf_oxide.PdfDocument", page: int, needles: tuple[str, ...]) -> tuple[tuple[float, float, float, float], ...]:
    words = tuple(doc.extract_words(page))
    runs = tuple(
        tuple(word.bbox for word in words[index : index + len(tokens)])
        for needle in needles
        for tokens in (tuple(needle.split()),)
        if tokens
        for index in range(len(words) - len(tokens) + 1)
        if all(words[index + offset].text == token for offset, token in enumerate(tokens))
    )
    return tuple(
        (
            min(box[0] for box in boxes),
            min(box[1] for box in boxes),
            max(box[0] + box[2] for box in boxes),
            max(box[1] + box[3] for box in boxes),
        )
        for boxes in runs
    )


def _redact_oxide(egress: DocumentEgress) -> FinishFact:
    label, scrub, tree_rects = egress.finishing.label, egress.finishing.scrub, _redaction_rects(egress.node)
    with pdf_oxide.PdfDocument.from_bytes(egress.source) as doc:
        for index in range(doc.page_count):
            for rect in (*tree_rects.try_find(index).default_value(()), *_needle_rects(doc, index, label.needles)):
                doc.add_redaction(index, rect, fill=label.fill)
        doc.apply_redactions_destructive(
            scrub_metadata=scrub.metadata, remove_javascript=scrub.javascript, remove_embedded_files=scrub.embedded_files
        )
        return FinishFact(doc.to_bytes(), pages=doc.page_count)


def _strip(egress: DocumentEgress) -> FinishFact:
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
    with pikepdf.open(BytesIO(egress.source)) as pdf:
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
    if not pol.prune:
        return FinishFact(scrubbed, pages=pages)
    return FinishFact(*_prune_pass(scrubbed, pol.prune))


def _prune_pass(data: bytes, prune: frozenset[PruneClass], /) -> tuple[bytes, int]:
    writer = PdfWriter(clone_from=PdfReader(BytesIO(data)))
    for cut in prune:
        _PRUNE[cut](writer)
    pruned = BytesIO()
    writer.write(pruned)
    return pruned.getvalue(), len(writer.pages)


def _sanitize_oxide(egress: DocumentEgress) -> FinishFact:
    pol = egress.finishing.sanitize
    with pdf_oxide.PdfDocument.from_bytes(egress.source) as doc:
        doc.sanitize_document(scrub_metadata=pol.private_app_data, remove_javascript=pol.javascript, remove_embedded_files=pol.attachments)
        if pol.flatten_annotations:
            doc.flatten_all_annotations()
        scrubbed, pages = doc.to_bytes(), doc.page_count
    if not pol.prune:
        return FinishFact(scrubbed, pages=pages)
    return FinishFact(*_prune_pass(scrubbed, pol.prune))


def _optimize(egress: DocumentEgress) -> FinishFact:
    pol = egress.finishing.optimize
    if pol.incremental:
        writer = PdfWriter(BytesIO(egress.source), incremental=True)
        sink = BytesIO()
        writer.write(sink)
        return FinishFact(sink.getvalue(), pages=len(writer.pages))
    if pol.flate_level >= 0:
        pikepdf.settings.set_flate_compression_level(
            pol.flate_level
        )
    with pikepdf.open(BytesIO(egress.source)) as pdf:
        warnings = (
            len(pdf.check_pdf_syntax()) if pol.verify_syntax else 0
        )
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
    if not pol.dedup_objects:
        return FinishFact(recompressed, pages=pages, syntax_warnings=warnings)
    writer = PdfWriter(clone_from=PdfReader(BytesIO(recompressed)))
    writer.compress_identical_objects(remove_duplicates=True, remove_unreferenced=True)
    deduped = BytesIO()
    writer.write(deduped)
    return FinishFact(deduped.getvalue(), pages=len(writer.pages), syntax_warnings=warnings)


def _protect(egress: DocumentEgress) -> FinishFact:
    office = msoffcrypto.OfficeFile(BytesIO(egress.source))
    sink = BytesIO()
    format_ = getattr(office, "format", "legacy-97")
    scheme = getattr(office, "type", "legacy-97")
    key_axis = tuple(getattr(office, "keyTypes", ("password",)))
    verification_fact = None
    match egress.finishing.confidentiality:
        case Confidentiality(tag="reseal", reseal=password):
            if office.is_encrypted():
                return FinishFact(egress.source, office_format=format_, office_scheme=scheme, credential_kinds=key_axis)
            office.encrypt(password, sink)
            scheme = "agile"
        case Confidentiality(tag="unlock", unlock=verification):
            verify_key, verify_integrity = _VERIFICATION[verification]
            if not office.is_encrypted():
                return FinishFact(
                    egress.source,
                    office_format=format_,
                    office_scheme=scheme,
                    credential_kinds=key_axis,
                    verification=OfficeVerification.NONE,
                )
            ooxml = format_ == "ooxml"
            verification_fact = verification if ooxml else OfficeVerification.NONE
            credentials = {kind: egress.extras.credentials[kind] for kind in key_axis if kind in egress.extras.credentials}
            office.load_key(**credentials, **({"verify_password": True} if ooxml and verify_key else {}))
            office.decrypt(sink, **({"verify_integrity": verify_integrity} if ooxml else {}))
        case _ as unreachable:
            assert_never(unreachable)
    return FinishFact(
        sink.getvalue(),
        office_format=format_,
        office_scheme=scheme,
        credential_kinds=key_axis,
        verification=verification_fact,
    )


def _pruned_pages(writer: "PdfWriter", flag: "ObjectDeletionFlag", /) -> None:
    for page in writer.pages:
        writer.remove_objects_from_page(page, flag)


# --- [COMPOSITION] ----------------------------------------------------------------------
FINISHERS: Final[Map[EgressStep, Finisher]] = Map.of_seq([
    (EgressStep.ENCRYPT, Finisher(_encrypt)),
    (EgressStep.OUTLINE, Finisher(_outline)),
    (EgressStep.WATERMARK, Finisher(_watermark)),
    (EgressStep.ATTACH, Finisher(_attach)),
    (EgressStep.IMPOSE, Finisher(_impose)),
    (EgressStep.NAVIGATE, Finisher(_navigate)),
    (EgressStep.FORMS, Finisher(_forms)),
    (EgressStep.REWRITE, Finisher(_rewrite)),
    (EgressStep.REDACT, Finisher(_redact, permissive=_redact_oxide)),
    (EgressStep.STRIP, Finisher(_strip)),
    (EgressStep.SANITIZE, Finisher(_sanitize, permissive=_sanitize_oxide)),
    (EgressStep.OPTIMIZE, Finisher(_optimize)),
    (EgressStep.PROTECT, Finisher(_protect, office=True)),
])
_PRUNE: Final[Map[PruneClass, Callable[["PdfWriter"], None]]] = Map.of_seq([
    (PruneClass.LINKS, lambda writer: writer.remove_links()),
    (PruneClass.ANNOTATIONS, lambda writer: writer.remove_annotations(None)),
    (PruneClass.IMAGES, lambda writer: writer.remove_images()),
    (PruneClass.TEXT, lambda writer: writer.remove_text()),
    (PruneClass.OBJECTS_3D, lambda writer: _pruned_pages(writer, ObjectDeletionFlag.OBJECTS_3D)),
    (PruneClass.XOBJECT_IMAGES, lambda writer: _pruned_pages(writer, ObjectDeletionFlag.XOBJECT_IMAGES)),
    (PruneClass.INLINE_IMAGES, lambda writer: _pruned_pages(writer, ObjectDeletionFlag.INLINE_IMAGES)),
    (PruneClass.DRAWING_IMAGES, lambda writer: _pruned_pages(writer, ObjectDeletionFlag.DRAWING_IMAGES)),
])
_PREREQ: Final[Map[EgressStep, Callable[[DocumentEgress], bool]]] = Map.of_seq([
    (EgressStep.ENCRYPT, lambda eg: eg.finishing.encryption is not None),
    (
        EgressStep.IMPOSE,
        lambda eg: eg.finishing.imposition.across > 0
        and eg.finishing.imposition.down > 0
        and all(isfinite(side) and side > 0.0 for side in eg.finishing.imposition.sheet)
        and (eg.finishing.imposition.layout is not ImposeLayout.BOOKLET or eg.finishing.imposition.slots == 2),
    ),
    (EgressStep.WATERMARK, lambda eg: bool(eg.extras.stamp)),
    (EgressStep.ATTACH, lambda eg: bool(eg.extras.attachment_data and eg.finishing.attachment.name)),
    (EgressStep.OUTLINE, lambda eg: eg.node is not None or bool(eg.finishing.bookmark.fallback)),
    (EgressStep.FORMS, lambda eg: bool(eg.finishing.forms.values) or eg.finishing.forms.flatten),
    (EgressStep.REDACT, lambda eg: eg.node is not None or bool(eg.finishing.label.needles)),
])
_LANE_GAPS: Final[Map[EgressStep, Callable[[DocumentEgress], bool]]] = Map.of_seq([
    (EgressStep.ENCRYPT, lambda _eg: True),
    (
        EgressStep.SANITIZE,
        lambda eg: eg.finishing.sanitize.external_access or eg.finishing.sanitize.multimedia or eg.finishing.sanitize.disable_signatures,
    ),
])
```

## [03]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
