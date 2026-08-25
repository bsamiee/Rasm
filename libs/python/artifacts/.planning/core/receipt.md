# [PY_ARTIFACTS_RECEIPT]

`ArtifactReceipt` is the one kind-discriminated artifact-evidence family every production sub-domain mints a case onto — one `@tagged_union(frozen=True)` keyed by the runtime `ContentKey`, satisfying the runtime `receipts.ReceiptContributor` port through `contribute`. Each case is a `(ContentKey, <facts>)` payload of native scalars called positionally; byte-only kinds are a bare pair, evidence-rich kinds close on one `frozendict` band as the LAST slot.

`_BAND` assigns each banded kind its namespace, `_facts` projects every band entry as `<namespace>.<fact>`, and every fixed fact name derives dot-free from its mint signature, so a band fact never shadows canonical evidence or the reserved `"artifact"` discriminant. Case roster is the ONE derivation owner — a load gate pins the `ArtifactKind` spelling, the fact-name table derives from the mint signatures — so the receipt stream stays one fact family the planner, the reuse-fabric elision, and the runtime `Metrics` projection read off one `contribute` fold, never a parallel rail per producer.

`ConformanceVerdict` is DECLARED ON THIS PAGE: `exchange/conformance` imports it DOWN and mints it, `delivery/transmittal` and `exchange/credential` read it off the receipt seam.

`contribute` projects the active payload onto the runtime receipt spine, records every `_METRIC`-governed fact through `Metrics.record(domain="artifact", kind=...)`, and fires the `core/hooks#POINTS` emitted tap at one fold — never a local metrics fief or a parallel fact family. Failed production mints no case; the producer's runtime `boundary` converts that raise into the `BoundaryFault` the spine's `rejected` line carries, and `graduates` hands any receipt outward on the compute hub's artifacts-origin case.

`evidence` is the branch's ONE durable-evidence construction — every kind funnels through it to build the `runtime/observability/journal#FACT` `AuditFact` and its `MeterFact` companions off the receipt's own ledger. Construction collapses here and the `await Journal.record(...)` seats at each producer's own async `_emit` fold, because recording SUSPENDS on the journal's bounded intake and `contribute` is a synchronous projection: a sync leg can only shed exactly what the never-shed rail refuses to shed.

## [01]-[INDEX]

- [02]-[RECEIPT]: `ArtifactReceipt` discriminates artifact evidence by kind over native-scalar `case()` payloads plus the texture case’s exact generated `ArtifactRef`, owns its roster-derived `_CASES`/`_KEYS`/`_BAND` tables and the `_RETENTION`/`_METERED` durable-plane rows, projects through `contribute` onto the runtime spine, builds durable facts through `evidence`, and hands off outward through `graduates`.
- [03]-[SIGNALS]: consumer seams reading the receipt fold, the outward figure hand-off, and the span-coverage charter.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a `tuple[ContentKey, ...]` whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named evidence, including texture's exact generated `ArtifactRef`, the evidence-rich `preview`/`color`/`texture`/`media`/`cad`/`scene`/`office`/`document`/`dashboard` kinds closing on one `frozendict` band as their last slot. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, and the union imports NO producer module — every case carries native scalars, the local `ConformanceVerdict`, a `frozendict` band, or texture's generated `ArtifactRef`, so no `c2pa-python`/`av`/`pysubs2`/`pikepdf` surface crosses in.
- Cases: the case-shape rulings the fence tuples cannot show — `Office`/`Report`/`Document` share the `(key, bytes_)` head but split by producer origin (a workbook/slide container, a composed report, and the generic `typography` document-rail blob for an axis catalog, glyph run, or line-broken stream that is neither a PDF nor an office file), `Office` and `Document` closing on a defaulted `facts` band (`finish`/`product` namespaces) while `Report` stays byte-only; `Egress` is reused by `document/tagged#ACCESS`, which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Schedule` is a distinct AEC case beside `Table` because a schedule carries its NCS/AIA kind and item cardinality the generic publication `Table` does not; `Cad` is distinct because a DXF document carries version/units/salvage-auditor evidence no publication case holds; `Drawing` is ONE shared case across the drawing plane, its `kind` the producer's own bare name (`dimension`/`annotate`/`symbol`/`detail`) — the case tag already spells `drawing`, so a `drawing-`-prefixed value restates it and forks the one discriminant the four producers share into two grammars a board then groups twice; its `style` the LOWERING that produced these bytes (`drawsvg`/`ezdxf`) — the one fact every drawing producer can state about the delivered artifact, where a `DimStyleFamily` or a leader convention echoes the request knob the caller already holds and the content key already folds, and the target is unrecoverable from a receipt carrying only an opaque key; a per-producer rail is the deleted form; the delivery pair `Register`/`Transmittal` are admitted as CASES beside `Spec`/`Drawing`/`Schedule`, never a parallel delivery-receipt rail; `Dashboard` is the composed-deck case beside the single-artifact kinds, its counts stating the deck's own composition while its band carries each chart pane's pre-pass evidence, since a dashboard's producers already minted their own receipts and this case attests the composition alone; and every banded kind closes on a `frozendict` band so a heterogeneous per-producer fact set rides one case — `Preview` absorbs BOTH the perceptual float band and the machine-readable-mark string facts under one `float | str` union, and `Media` folds every media page's `av`/`pysubs2` evidence onto its band rather than a fixed-scalar bag whose subtitle/loudness/scene fields default to zero on the pages that never produce them. `Texture` is ONE case over the whole texture estate and over BOTH producer altitudes: `kind` carries the set's own `pbr_set`/`hdri`/`ibl` discriminant so an environment product needs no second case, and `maps` reads 1 on a per-plane receipt and the produced-plane count on the set-level one, so the fan and its fold ride one shape. Its `mips` pyramid depth and `texels` working-set size are FIXED slots rather than band entries because both are metered — the deep-pixel plane's cost scales per texel and per level where a page count answers nothing, so a regression in either trends in production instead of only inside a graded bench window — and `tool` is fixed for the same reason one rung up: it fills the producing-leg dimension those two instruments declare, and a discriminant read off a band a producer may leave empty is a fan a board groups on and nothing answers. Per-plane receipts name the encoder that produced them; a set-level receipt names the leg only when every map took one, spelling a mixed set as absence rather than electing a winner. `_METRIC` projects per RECEIPT, not per product, so a producer minting both altitudes owes its own non-overlapping `bytes_` split — the set-level slot measures what the SET itself delivers, never a re-sum of planes its fan already recorded, because a sum there enters `rasm.artifact.byte_volume` a second time and inflates the distribution by the fan width. Its band holds the produced file's metadata — role, file, color space, depth, container, component count, mip depth, KTX payload, producing tool and version, and ordering `variant`; the exact reference carries identity and extent without a scalar mirror, and the twenty-seven SH coefficients ride the SET-level band as `sh_<band>_<channel>` scalars off the manifest's own `ibl` leg — so the producer publishes evidence rather than a second document, and a per-map fixed-scalar case defaults every environment fact to zero on a PBR set and every channel fact to zero on an IBL one. `Preview` and `Color` require a fixed `bytes_` slot so every raster and separation artifact records byte volume through the `_METRIC` fold rather than smuggling a `bytes` band fact past it or defaulting absent evidence to zero. Each mint is a `@classmethod` returning `Self`, binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the return type on every mint, and stays thin — keyword-constructing its `case()` and adding nothing — while the optional `scores`/`facts` bands and `Media.bit_rate` keep a band-free or unknown-rate encode valid.
- Law: bands are a declared grammar, never an open bag, and two band kinds stand lawful on their own axes. FACT bands (`preview`/`color`/`texture`/`media`/`scene`/`office`/`document`/`dashboard`) carry producer-owned measurement vocabulary — the `graphic/raster/measure` transform scores, `graphic/color/managed`'s `ManagedFact` rows, the `graphic/texture/set` map preimage and `graphic/texture/ibl` harmonic bands, the media loudness/scene facts, the office finishing and typography product facts — whose leaf names live at the producer measuring them; DOMAIN-KEYED tables (`cad` `counts`) carry domain names as keys, the uppercase DXF entity spellings, never fact names. Both project through the kind's `_BAND` namespace, so a band entry stays self-describing in the `EventDict`, shadows no fixed fact (fixed names are dot-free by the load gate), and never emits the reserved `"artifact"` key. That same grammar keeps every band LEAF dot-free: the projection is exactly `<namespace>.<leaf>` and a second dot forks the namespace an `EventDict` reader splits on, so a producer carrying a sub-axis spells it into the leaf (`sh_0_r`, never `sh_0.r`) — the load gate proves only the fixed names because they alone are known at import, and the leaf discipline is the producer's own construction. Band facts graduating to a governed metric, a metric DIMENSION, or a ceiling graduate FIRST to a fixed slot on their case — `bytes_` is the precedent — so `_METRIC` and `_CEILING` read declared fixed names alone, and a discriminant is no exception: a dimension filled off a band a producer may never write declares a fan the view allow-list admits and no series ever carries.
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the port declares, appending the `ArtifactKind` discriminant under the reserved `"artifact"` key — reserved because `contribute` overwrites it last, so no `_facts` arm may emit it, and the inner `"kind"` fact the `diagram`/`schedule`/`register`/`texture` cases carry and the `cad` DXF output format that rides `"format"` each stay distinct. At the same fold the measured-signals seam fires: the universal `_METRIC[""]` row merges with this kind's own row, and the merged measures record through `Metrics.record(kind=self.tag)` once PER SUBJECT, each subject derived from its own instrument names because the runtime census keys `(domain, name)` totally and a measure recorded under a subject its name does not spell misses that lookup outright. Kind-owned rows carry the dimensions they declare, filled from fixed slots and omitted where a slot is empty, while the universal pair carries none; merged rows naming no present fact record nothing. It takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers. That same fold fires the production-fact tap through `Production.fired` — ONE fire over the whole roster, every kind streaming `ReceiptEmitted` under `rasm.artifacts.receipt.emitted` with the `scoped` issue-baggage correlation id, so audit subscription rides the hook registry. Per-kind second fires here announce a case narrower than the evidence its producer holds, which is why `TRANSMITTAL_ISSUED` fires at `delivery/transmittal#TRANSMITTAL`'s own close instead.
- Entry: `evidence(self, /, *change, actor=_SERVICE, subjects=())` is the ONE durable-fact construction every kind funnels through, returning the `Block[Fact]` a producer's async fold hands `Journal.record`. `DOMAIN` and the kind's own tag spell the verb as `<domain>.<operation>`, so it greps against the series `contribute` already recorded under that same segment and no verb table stands between them; the target names the kind and carries `slot.hex`; `_RETENTION` supplies the class; the change tuple spreads the DECLARED facts as `Assigned` entries and appends whatever finer diff the producer passes positionally. Band leaves never enter it — the load gate makes fixed names dot-free, so the filter is exact — because a band's leaf set is the producer's own instrumentation and an audit row whose width tracks it is a diff no reader compares across two runs. `_METERED` fans the charges off the same declared facts: `bytes` charges `STORAGE`, the `pages`/`sheets` cardinalities charge `RECORD`, a zero or absent quantity charges nothing, and the resource's series is the journal's own `RESOURCES` row so no unit is spelled here. `actor` and `subjects` stay the producer's because construction cannot know either: the first defaults to `_SERVICE` and only a leg where a named party acts overrides it, the second is the portability index every producer touching no real identity leaves empty. This fold RECORDS nothing — recording suspends and this surface is synchronous — so the `await` seats at each producer's own async `_emit`, and an artifacts producer binds the journal's default scope, the plane holding no per-owner `ScopeKey`.
- Auto: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose or-pattern captures `(key, *_)` once and closes on `assert_never`, where a reflective `getattr(self, self.tag)[0]` erases the key to `object` and defeats the exhaustiveness witness the planner keys on. `_facts` is the second total `match self`: texture first projects its exact reference into durable scalar evidence, then the verdict, remaining banded, and flat arms stay total: `verdict` spreads the local `ConformanceVerdict.facts()`, the ONE banded or-pattern destructures `(_key, *tail, band)` and joins the mint-derived fixed zip with the namespaced band spread, and the flat or-pattern zips each scalar tail against `_KEYS[self.tag]` under `strict=True`, keeping native ints/floats intact. Roster stays the ONE derivation owner: `_CASES` reads the case fields through `annotationlib.get_annotations` over the union, the first load gate raises at import when the `ArtifactKind` Literal and the roster drift, `_KEYS` derives every fact tail — banded kinds dropping the trailing band parameter — from its mint signature under the `removesuffix("_")` builtin-collision rule, and the second load gate raises when any fixed fact name — a mint-derived row or a spread `ConformanceVerdict` field off `structs.fields` — carries a dot or spells the reserved `"artifact"` — one declaration site, so renaming a mint parameter renames the emitted fact and an unlawful name breaks at import, never at a fold.
- Output: `graduates(self, *, ceiling=None)` is the one outward figure hand-off — it projects ANY receipt onto the compute graduation hub's artifacts-origin case, keyed by the receipt's own `slot` with no re-mint, `measured` the numeric `_facts` ledger (every `int`/`float` including namespaced band floats, `bool` excluded — the verdict flags are admission facts, not residuals) and `bars` the kind-scoped `_CEILING` row combined by pointwise minimum with the caller's tighter per-key override. Admission, the `content.graduate` span, and the `planned`-receipt egress stay compute's — this page composes the hub DOWNWARD and adds no second fence, span, or re-wrap, since a second `@receipted` over the returned rail double-streams the receipt. `exchange/conformance#CONFORMANCE` is the standing consumer: its `_emit` binds the minted `Verdict` receipt through this projection, so the compliance figure `delivery/transmittal#TRANSMITTAL` already reads publishes on the hub at the same fold and a barred residual refuses the emit rather than shipping an ungoverned figure. Bars are CEILINGS and nothing else — the hub clears on `measured[name] <= cap` over every barred key AND demands each key be present in the measured ledger — so a `_CEILING` row names a fixed fact its keyed kind ALWAYS measures as a non-bool number: `color` bars `tac_peak` at `320.0`, `cad` bars `errors` at `0.0`, `verdict` bars `signatures_broken` at `0.0`. `bool` fields stay out of `measured` by construction, so a row naming one refuses every graduation of its kind; an at-least invariant such as a verified-entry floor inverts the comparison and stays a producer-side gate with no seat here; and an ungoverned kind supplies no inherited bar, graduating its ledger under the caller's own. `model_asset` is a compute-own subject figures never ride, and the projection re-mints no canonical concept.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union, `Map` the derived `_KEYS`/`_METRIC`/`_CEILING` tables, `Block` the per-subject record fold); `msgspec` (`Struct` + `structs.asdict` the re-homed `ConformanceVerdict`); the builtin `frozendict` (the case evidence bands and the `_BAND` namespace rows — msgspec-native and hashable where `Map` is not); stdlib `annotationlib.get_annotations` + `inspect.signature` (the `_CASES` roster and `_KEYS` mint-signature derivations); `opentelemetry-api` (`context.get_current` feeding the `scoped` baggage read); core hooks (`Production`/`ArtifactHook` and the `ReceiptEmitted` projection, the floor sibling); runtime (`identity.ContentKey`, `receipts.Receipt` and the structurally-satisfied `ReceiptContributor` port, `metrics.Metrics.record` the domain/kind arm with `metrics.Dimension` typing the `stamps` keys, `faults.RuntimeRail`, and the `journal` fact family — `AuditFact`/`MeterFact`/`Party`/`Actor`/`Assigned`/`Change`/`Resource`/`Retain`/`Subject` — consumed as the durable vocabulary this page builds into and never as a writer, since `Journal` itself is imported by the recording legs alone); compute (`graduation.handoff.GraduationReceipt`/`HandoffAxis`, imported downward). No producer module crosses in.
- Growth: a new artifact kind is one `ArtifactKind` token, one `case()`, one `@classmethod` mint, one `slot` or-pattern arm, one `_facts` or-pattern alternative, and one `_RETENTION` row — the load gates and the `assert_never` tails break at import until every piece exists. Flat kinds add nothing else (their `_KEYS` row derives from the mint signature); a banded kind adds its `_BAND` namespace row, so a new band fact is one key the producer fills with zero receipt edit and the namespace projection lands it lawfully. Every new scalar on a flat or banded kind is one slot on its `case()` and one mint parameter, the `strict=True` zip raising on drift; a new measured signal is one FIXED slot with one `_METRIC` `measures` row under its kind here — under the `""` row when every kind carries it — and one `InstrumentSpec` row on the runtime metrics owner, its own name naming the subject the write lands under with no second table edited; a new discriminant on those rows is one FIXED slot and one `stamps` row; a new governed residual bar is one `_CEILING` row over a fixed non-bool numeric fact its keyed kind always measures; a newly charged quantity is one `_METERED` row over a fixed slot beside its `RESOURCES` row at the journal owner; a producer-specific durable diff is positional `Change` entries at that producer's own record seat, never a field on any case.
- Boundary: one union carries every domain's evidence as a case, never a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family or a parallel `ColorReceipt`/AEC/delivery rail beside it, and never a per-kind evidence `Struct` re-wrapping scalars the producer passes positionally; the `media` band carries each page's own evidence, never a monolithic fixed-scalar case defaulting the absent pages to zero. `_KEYS`, `_BAND`, and `ArtifactKind` derive from or pin to the roster, never a hand-synced parallel table; a band entry projects only under its kind's namespace, never spread bare where it stamps over a fixed fact; `ConformanceVerdict` is re-homed DOWN, never imported upward into the spine. Metrics route through `Metrics.record`, never a page-owned histogram/counter/logging fief, and no render DURATION fact rides a case — the runtime metrics aspect owns timing. Facts reach the `dict[str, object]` `EventDict` as native scalars, never `str()`-pre-formatted; `slot`/`_facts` read through the total `match`, never a reflective `getattr`; `contribute` carries no `phase` parameter and no artifacts-side `rejected` forwarder; `graduates` adds no second `@receipted` re-wrap; and the union imports no producer module. Durable facts BUILD here and record nowhere — no `Journal` import, no `await`, and no fact minted off an unsettled value — while the series stay `contribute`'s and the facts stay the journal's, so neither fold re-mints the other's number; a per-kind audit builder, a second verb table beside the `<domain>.<operation>` derivation, a retention class carried on a case, or a band leaf reaching a `Change` entry or a `MeterFact` quantity is the deleted form.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from annotationlib import get_annotations
from collections.abc import Iterable, Mapping
from inspect import signature
from typing import Final, Literal, Self, assert_never, get_args

from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Block, Map
from msgspec import Struct, structs
from opentelemetry import context as otel_context

from rasm.artifacts.core.hooks import ArtifactHook, Production, ReceiptEmitted, scoped
from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.contracts.rasm.contracts.artifact.artifact_pb import ArtifactRef
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.journal import Actor, Assigned, AuditFact, Change, Fact, MeterFact, Party, Resource, Retain, Subject
from rasm.runtime.metrics import Dimension, Metrics
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type ArtifactKind = Literal[
    "pdf",
    "office",
    "report",
    "document",
    "chart",
    "dashboard",
    "scene",
    "table",
    "preview",
    "color",
    "texture",
    "bundle",
    "introspection",
    "egress",
    "verdict",
    "credential",
    "media",
    "diagram",
    "metadata",
    "drawing",
    "schedule",
    "spec",
    "cad",
    "register",
    "transmittal",
]

# --- [CONSTANTS] ------------------------------------------------------------------------

OWNER: Final[str] = "artifacts"
DOMAIN: Final[str] = "artifact"

_SERVICE: Final[Party[Actor]] = Party(kind=Actor.SERVICE, key=OWNER)

# --- [MODELS] ---------------------------------------------------------------------------


class MetricRow(Struct, frozen=True, gc=False):
    measures: Map[str, str] = Map.empty()
    stamps: Map[Dimension, str] = Map.empty()


class ConformanceVerdict(Struct, frozen=True, gc=False):
    pades_level: str
    pages: int
    signatures: int
    timestamps: int
    fields_awaiting: int
    signatures_valid: int
    signatures_trusted: int
    signatures_broken: int
    signature_valid: bool
    trusted: bool
    revoked: bool
    coverage_level: str
    modification_level: str
    docmdp_ok: bool
    seed_value_ok: bool
    certification_level: str
    signer_subject: str
    signer_issuer: str
    signer_serial: str
    digest_algorithm: str
    signature_mechanism: str
    signed_at: str
    timestamp_at: str
    timestamp_valid: bool
    content_timestamp_valid: bool
    archival_timestamps_valid: bool
    qualified: bool
    ltv_complete: bool
    dss_certs: int
    dss_ocsps: int
    dss_crls: int
    dss_vri: int
    structural_conformant: bool
    archival_conformant: bool
    prepress_conformant: bool
    pdfa_claim: str
    pdfx_claim: str

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


@tagged_union(frozen=True)
class ArtifactReceipt:
    tag: ArtifactKind = tag()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int, frozendict[str, float | str]] = case()
    report: tuple[ContentKey, int] = case()
    document: tuple[ContentKey, int, frozendict[str, float | str]] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    dashboard: tuple[ContentKey, int, int, int, int, int, frozendict[str, float | str]] = case()
    scene: tuple[ContentKey, str, int, frozendict[str, float | str]] = case()
    table: tuple[ContentKey, str, int] = case()
    preview: tuple[ContentKey, int, int, int, frozendict[str, float | str]] = case()
    color: tuple[ContentKey, str, str, float, int, int, frozendict[str, float | str]] = case()
    texture: tuple[
        ContentKey,
        str,
        int,
        int,
        int,
        int,
        int,
        int,
        str,
        ArtifactRef | None,
        frozendict[str, float | str],
    ] = case()
    bundle: tuple[ContentKey, int, str, int, int, int, int, int, float] = case()
    introspection: tuple[ContentKey, int, int, int, int] = case()
    egress: tuple[ContentKey, int, int, int, int, int] = case()
    verdict: tuple[ContentKey, ConformanceVerdict] = case()
    credential: tuple[ContentKey, str, str, int, str] = case()
    media: tuple[ContentKey, str, str, float, int, int, int, frozendict[str, float | str]] = case()
    diagram: tuple[ContentKey, str, int, int, str, int] = case()
    metadata: tuple[ContentKey, str, int, int] = case()
    drawing: tuple[ContentKey, str, int, str, int, int, int] = case()
    schedule: tuple[ContentKey, str, int, int, str, int] = case()
    spec: tuple[ContentKey, str, int, int, int, int] = case()
    cad: tuple[ContentKey, str, str, str, int, int, int, int, int, frozendict[str, int]] = case()
    register: tuple[ContentKey, str, int, str, str, str, str, int] = case()
    transmittal: tuple[ContentKey, str, int, str, str, str] = case()

    @classmethod
    def Pdf(cls, key: ContentKey, bytes_: int, pages: int, /) -> Self:
        return cls(pdf=(key, bytes_, pages))

    @classmethod
    def Office(cls, key: ContentKey, bytes_: int, facts: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(office=(key, bytes_, facts))

    @classmethod
    def Report(cls, key: ContentKey, bytes_: int, /) -> Self:
        return cls(report=(key, bytes_))

    @classmethod
    def Document(cls, key: ContentKey, bytes_: int, facts: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(document=(key, bytes_, facts))

    @classmethod
    def Chart(cls, key: ContentKey, engine: str, dialect: str, scale: float, theme: str, bytes_: int, /) -> Self:
        return cls(chart=(key, engine, dialect, scale, theme, bytes_))

    @classmethod
    def Dashboard(
        cls, key: ContentKey, bytes_: int, panes: int, charts: int, tables: int, diagrams: int, facts: frozendict[str, float | str] = frozendict(), /
    ) -> Self:
        return cls(dashboard=(key, bytes_, panes, charts, tables, diagrams, facts))

    @classmethod
    def Scene(cls, key: ContentKey, target: str, bytes_: int = 0, facts: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(scene=(key, target, bytes_, facts))

    @classmethod
    def Table(cls, key: ContentKey, format_: str, bytes_: int = 0, /) -> Self:
        return cls(table=(key, format_, bytes_))

    @classmethod
    def Preview(cls, key: ContentKey, width: int, height: int, bytes_: int, scores: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(preview=(key, width, height, bytes_, scores))

    @classmethod
    def Color(
        cls,
        key: ContentKey,
        space: str,
        intent: str,
        tac_peak: float,
        plates: int,
        bytes_: int,
        facts: frozendict[str, float | str] = frozendict(),
        /,
    ) -> Self:
        return cls(color=(key, space, intent, tac_peak, plates, bytes_, facts))

    @classmethod
    def Texture(
        cls,
        key: ContentKey,
        kind: str,
        width: int,
        height: int,
        maps: int,
        bytes_: int,
        mips: int = 0,
        texels: int = 0,
        tool: str = "",
        reference: ArtifactRef | None = None,
        facts: frozendict[str, float | str] = frozendict(),
        /,
    ) -> Self:
        return cls(texture=(key, kind, width, height, maps, bytes_, mips, texels, tool, reference, facts))

    @classmethod
    def Bundle(cls, key: ContentKey, bytes_: int, algo: str, level: int, dict_id: int, frame_size: int, entries: int, verified: int, ratio: float, /) -> Self:
        return cls(bundle=(key, bytes_, algo, level, dict_id, frame_size, entries, verified, ratio))

    @classmethod
    def Introspection(cls, key: ContentKey, nodes: int, text_len: int, images: int, hits: int, /) -> Self:
        return cls(introspection=(key, nodes, text_len, images, hits))

    @classmethod
    def Egress(cls, key: ContentKey, bytes_: int, pages: int, encryption_r: int, outline_depth: int, overlays: int, /) -> Self:
        return cls(egress=(key, bytes_, pages, encryption_r, outline_depth, overlays))

    @classmethod
    def Verdict(cls, key: ContentKey, verdict: ConformanceVerdict, /) -> Self:
        return cls(verdict=(key, verdict))

    @classmethod
    def Credential(cls, key: ContentKey, manifest_id: str, signer: str, assertions: int, validation_state: str, /) -> Self:
        return cls(credential=(key, manifest_id, signer, assertions, validation_state))

    @classmethod
    def Media(
        cls,
        key: ContentKey,
        container: str,
        codec: str,
        duration: float,
        bytes_: int,
        frames: int,
        bit_rate: int = 0,
        facts: frozendict[str, float | str] = frozendict(),
        /,
    ) -> Self:
        return cls(media=(key, container, codec, duration, bytes_, frames, bit_rate, facts))

    @classmethod
    def Diagram(cls, key: ContentKey, kind: str, nodes: int, edges: int, algorithm: str, bytes_: int, /) -> Self:
        return cls(diagram=(key, kind, nodes, edges, algorithm, bytes_))

    @classmethod
    def Metadata(cls, key: ContentKey, carrier: str, fields: int, bytes_: int, /) -> Self:
        return cls(metadata=(key, carrier, fields, bytes_))

    @classmethod
    def Drawing(cls, key: ContentKey, kind: str, entities: int, style: str, width: int, height: int, bytes_: int, /) -> Self:
        return cls(drawing=(key, kind, entities, style, width, height, bytes_))

    @classmethod
    def Schedule(cls, key: ContentKey, kind: str, rows: int, columns: int, format_: str, bytes_: int, /) -> Self:
        return cls(schedule=(key, kind, rows, columns, format_, bytes_))

    @classmethod
    def Spec(cls, key: ContentKey, section: str, division: int, parts: int, articles: int, bytes_: int, /) -> Self:
        return cls(spec=(key, section, division, parts, articles, bytes_))

    @classmethod
    def Cad(
        cls,
        key: ContentKey,
        dxfversion: str,
        units: str,
        format_: str,
        bytes_: int,
        layers: int,
        blocks: int,
        errors: int,
        fixes: int,
        counts: frozendict[str, int],
        /,
    ) -> Self:
        return cls(cad=(key, dxfversion, units, format_, bytes_, layers, blocks, errors, fixes, counts))

    @classmethod
    def Register(
        cls, key: ContentKey, kind: str, sheets: int, suitability: str, revision: str, classification: str, validation: str, bytes_: int, /
    ) -> Self:
        return cls(register=(key, kind, sheets, suitability, revision, classification, validation, bytes_))

    @classmethod
    def Transmittal(cls, key: ContentKey, transmittal_id: str, sheets: int, suitability: str, container: str, validation_state: str, /) -> Self:
        return cls(transmittal=(key, transmittal_id, sheets, suitability, container, validation_state))

    @property
    def slot(self) -> ContentKey:
        match self:
            case (
                ArtifactReceipt(tag="pdf", pdf=(key, *_))
                | ArtifactReceipt(tag="office", office=(key, *_))
                | ArtifactReceipt(tag="report", report=(key, *_))
                | ArtifactReceipt(tag="document", document=(key, *_))
                | ArtifactReceipt(tag="chart", chart=(key, *_))
                | ArtifactReceipt(tag="dashboard", dashboard=(key, *_))
                | ArtifactReceipt(tag="scene", scene=(key, *_))
                | ArtifactReceipt(tag="table", table=(key, *_))
                | ArtifactReceipt(tag="preview", preview=(key, *_))
                | ArtifactReceipt(tag="color", color=(key, *_))
                | ArtifactReceipt(tag="texture", texture=(key, *_))
                | ArtifactReceipt(tag="bundle", bundle=(key, *_))
                | ArtifactReceipt(tag="introspection", introspection=(key, *_))
                | ArtifactReceipt(tag="egress", egress=(key, *_))
                | ArtifactReceipt(tag="verdict", verdict=(key, *_))
                | ArtifactReceipt(tag="credential", credential=(key, *_))
                | ArtifactReceipt(tag="media", media=(key, *_))
                | ArtifactReceipt(tag="diagram", diagram=(key, *_))
                | ArtifactReceipt(tag="metadata", metadata=(key, *_))
                | ArtifactReceipt(tag="drawing", drawing=(key, *_))
                | ArtifactReceipt(tag="schedule", schedule=(key, *_))
                | ArtifactReceipt(tag="spec", spec=(key, *_))
                | ArtifactReceipt(tag="cad", cad=(key, *_))
                | ArtifactReceipt(tag="register", register=(key, *_))
                | ArtifactReceipt(tag="transmittal", transmittal=(key, *_))
            ):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    def _facts(self) -> dict[str, object]:
        match self:
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return verdict.facts()
            case ArtifactReceipt(tag="texture", texture=(_key, *tail, reference, band)):
                names = tuple(name for name in _KEYS["texture"] if name != "reference")
                published = (
                    {"sha256": reference.sha256.hex(), "artifact_bytes": reference.artifact_bytes}
                    if reference is not None
                    else {}
                )
                return {
                    **dict(zip(names, tail, strict=True)),
                    **published,
                    **{f"{_BAND[self.tag]}.{fact}": value for fact, value in band.items()},
                }
            case (
                ArtifactReceipt(tag="preview", preview=(_key, *tail, band))
                | ArtifactReceipt(tag="color", color=(_key, *tail, band))
                | ArtifactReceipt(tag="media", media=(_key, *tail, band))
                | ArtifactReceipt(tag="cad", cad=(_key, *tail, band))
                | ArtifactReceipt(tag="scene", scene=(_key, *tail, band))
                | ArtifactReceipt(tag="dashboard", dashboard=(_key, *tail, band))
                | ArtifactReceipt(tag="office", office=(_key, *tail, band))
                | ArtifactReceipt(tag="document", document=(_key, *tail, band))
            ):
                named = dict(zip(_KEYS[self.tag], tail, strict=True))
                return {**named, **{f"{_BAND[self.tag]}.{fact}": value for fact, value in band.items()}}
            case (
                ArtifactReceipt(tag="pdf", pdf=(_key, *tail))
                | ArtifactReceipt(tag="report", report=(_key, *tail))
                | ArtifactReceipt(tag="chart", chart=(_key, *tail))
                | ArtifactReceipt(tag="table", table=(_key, *tail))
                | ArtifactReceipt(tag="bundle", bundle=(_key, *tail))
                | ArtifactReceipt(tag="introspection", introspection=(_key, *tail))
                | ArtifactReceipt(tag="egress", egress=(_key, *tail))
                | ArtifactReceipt(tag="credential", credential=(_key, *tail))
                | ArtifactReceipt(tag="diagram", diagram=(_key, *tail))
                | ArtifactReceipt(tag="metadata", metadata=(_key, *tail))
                | ArtifactReceipt(tag="drawing", drawing=(_key, *tail))
                | ArtifactReceipt(tag="schedule", schedule=(_key, *tail))
                | ArtifactReceipt(tag="spec", spec=(_key, *tail))
                | ArtifactReceipt(tag="register", register=(_key, *tail))
                | ArtifactReceipt(tag="transmittal", transmittal=(_key, *tail))
            ):
                return dict(zip(_KEYS[self.tag], tail, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        facts = self._facts()
        row = _METRIC.try_find(self.tag).default_value(_UNMETERED)
        governed = (*_METRIC[""].measures.items(), *row.measures.items())
        measures = {name: float(v) for slot, name in governed if isinstance(v := facts.get(slot), int | float) and not isinstance(v, bool)}
        stamps = {dimension: text for dimension, slot in row.stamps.items() if (text := str(facts.get(slot, "")))}
        owned = frozenset(_subject(name) for name in row.measures.values())
        Block.of_seq(sorted(frozenset(_subject(name) for name in measures))).fold(
            lambda _, subject: Metrics.record(
                {name: value for name, value in measures.items() if _subject(name) == subject},
                domain=subject,
                kind=self.tag,
                dimensions=stamps if subject in owned else {},
            ),
            None,
        )
        Production.fired(
            ArtifactHook.RECEIPT_EMITTED,
            ReceiptEmitted(kind=self.tag, key=self.slot.hex, scope=scoped(otel_context.get_current())),
        )
        return (Receipt.of(OWNER, ("emitted", self.slot.hex, {**facts, DOMAIN: self.tag})),)

    def evidence(self, /, *change: Change, actor: Party[Actor] = _SERVICE, subjects: tuple[Subject, ...] = ()) -> Block[Fact]:
        facts = self._facts()
        declared = tuple((name, value) for name, value in sorted(facts.items()) if "." not in name)
        audited = AuditFact(
            action=f"{DOMAIN}.{self.tag}",
            actor=actor,
            target=Party(kind=self.tag, key=self.slot.hex),
            retention=_RETENTION[self.tag],
            change=(*(Assigned(path=f"/{name}", next=str(value)) for name, value in declared), *change),
            subjects=subjects,
        )
        metered = tuple(
            MeterFact(resource=resource, quantity=value, surface=self.tag)
            for name, value in declared
            if isinstance(value, int) and not isinstance(value, bool) and value > 0
            for resource in _METERED.try_find(name).to_list()
        )
        return Block.of_seq((audited, *metered))

    def graduates(self, /, *, ceiling: Mapping[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        measured = {k: float(v) for k, v in self._facts().items() if isinstance(v, int | float) and not isinstance(v, bool)}
        governed = dict(_CEILING.try_find(self.tag).default_value(Map.empty()).items())
        requested = dict(ceiling or {})
        bars = {name: min(requested.get(name, cap), cap) for name, cap in governed.items()}
        return GraduationReceipt.graduates(OWNER, HandoffAxis(artifact=self.tag), self.slot, measured, bars)


# --- [TABLES] ---------------------------------------------------------------------------

_CASES: Final[tuple[str, ...]] = tuple(f for f in get_annotations(ArtifactReceipt) if f != "tag")
if set(_CASES) != set(get_args(ArtifactKind.__value__)):
    raise RuntimeError("ArtifactKind drifted from the case roster")

_BAND: Final[frozendict[str, str]] = frozendict(
    {
        "preview": "score",
        "color": "ink",
        "texture": "map",
        "media": "stream",
        "cad": "entity",
        "scene": "view",
        "office": "finish",
        "document": "product",
        "dashboard": "pane",
    }
)

_KEYS: Final[Map[str, tuple[str, ...]]] = Map.of_seq(
    (
        kind,
        tuple(
            name.removesuffix("_")
            for name in tuple(signature(getattr(ArtifactReceipt, kind.capitalize())).parameters)[1 : (-1 if kind in _BAND else None)]
        ),
    )
    for kind in _CASES
    if kind != "verdict"
)
if any("." in name or name == DOMAIN for row in (*_KEYS.values(), tuple(f.name for f in structs.fields(ConformanceVerdict))) for name in row):
    raise RuntimeError("a fixed fact name collides with the band namespace grammar or the reserved discriminant")


def _subject(name: str, /) -> str:
    return name.split(".", 2)[1]


_UNMETERED: Final[MetricRow] = MetricRow()
_METRIC: Final[Map[ArtifactKind | Literal[""], MetricRow]] = Map.of_seq([
    ("", MetricRow(measures=Map.of_seq([("bytes", "rasm.artifact.byte_volume"), ("ratio", "rasm.artifact.compression_ratio")]))),
    (
        "texture",
        MetricRow(
            measures=Map.of_seq([("mips", "rasm.texture.mip_depth"), ("texels", "rasm.texture.texels")]),
            stamps=Map.of_seq([(Dimension.TOOL, "tool")]),
        ),
    ),
])

_CEILING: Final[Map[ArtifactKind, Map[str, float]]] = Map.of_seq([
    ("color", Map.of_seq([("tac_peak", 320.0)])),
    ("cad", Map.of_seq([("errors", 0.0)])),
    ("verdict", Map.of_seq([("signatures_broken", 0.0)])),
])

_RETENTION: Final[Map[ArtifactKind, Retain]] = Map.of_seq([
    ("pdf", Retain.OPERATIONAL),
    ("office", Retain.OPERATIONAL),
    ("report", Retain.OPERATIONAL),
    ("document", Retain.OPERATIONAL),
    ("chart", Retain.OPERATIONAL),
    ("dashboard", Retain.OPERATIONAL),
    ("scene", Retain.OPERATIONAL),
    ("table", Retain.OPERATIONAL),
    ("preview", Retain.OPERATIONAL),
    ("color", Retain.OPERATIONAL),
    ("texture", Retain.OPERATIONAL),
    ("bundle", Retain.OPERATIONAL),
    ("introspection", Retain.OPERATIONAL),
    ("media", Retain.OPERATIONAL),
    ("diagram", Retain.OPERATIONAL),
    ("drawing", Retain.OPERATIONAL),
    ("schedule", Retain.OPERATIONAL),
    ("spec", Retain.OPERATIONAL),
    ("cad", Retain.OPERATIONAL),
    ("egress", Retain.REGULATORY),
    ("metadata", Retain.REGULATORY),
    ("verdict", Retain.REGULATORY),
    ("credential", Retain.REGULATORY),
    ("register", Retain.REGULATORY),
    ("transmittal", Retain.REGULATORY),
])
if frozenset(_RETENTION.keys()) != frozenset(_CASES):
    raise RuntimeError("a kind carries no retention class")

_METERED: Final[Map[str, Resource]] = Map.of_seq([
    ("bytes", Resource.STORAGE),
    ("pages", Resource.RECORD),
    ("sheets", Resource.RECORD),
])

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ("ArtifactKind", "ArtifactReceipt", "ConformanceVerdict")
```

## [03]-[SIGNALS]

- [REUSE_ELISION]: reuse-fabric leg is the receipt-fold CONSUMER of the runtime `runtime/execution/lanes#LANE` `(ContentKey, Work)` admission elision. Artifacts holds the consumer edge: every producer mints its key over the INPUT spec pre-run, threads it into the lane admission, and `_emit` threads it into the terminal receipt so `slot` reads it back for the hit/miss distinction — no new case, no new owner, no re-minted key.
- [METRIC_SIGNALS]: measured-signal leg is COMPOSED at the `contribute` fold — the universal `_METRIC[""]` row merged with the carried kind's own row projects present numeric facts onto `Metrics.record(kind=...)`, one write per subject those names spell, so nothing artifacts-local holds metric state. Render DURATION is NOT a receipt fact — the runtime aspect times the serve coroutine — and attribution stays runtime-owned through `_attributed`, the issue scope a log dimension by cardinality law.
- [EVIDENCE_PLANE]: durable-evidence leg is the one receipt consumer that does NOT ride `contribute` — `evidence` builds the `AuditFact` and its `MeterFact` fan here while each producer's async `_emit` awaits `Journal.record` over it, so the suspending write seats at the fold owning the settled artifact. That rail BINDS into the producer's verdict: an armed plane refusing a fact is a governance failure the caller owns, an uninstalled plane no-ops, retired custody refuses.
- [HOOK_TAPS]: production-fact leg fires at the same fold — `Production.fired` streams the `ReceiptEmitted` projection for every kind and nothing per-kind beside it, so audit, veto, and replay consumers subscribe at the app root through the `core/hooks#POINTS` registry and the receipt stream stays the one evidence truth the fired fact projects.
- [PLAN_FABRIC]: production-planning leg is the third receipt-fold CONSUMER — `core/plan#PLAN` reads each producer's resolved `ArtifactReceipt` as the content-keyed evidence its sub-graph elision distinguishes a hit from a miss on. `planned`-stage observability is the planner's OWN direct `Receipt.of` emit, so the `admitted`/`planned`/`emitted` line family is shared by that emit and the producers' `emitted` facts, never a phase threaded through `contribute`.
- [FIGURE_HANDOFF]: outward figure edge travels the compute `compute/graduation/handoff#GRADUATION` rail on its artifacts-origin `artifact` case. `graduates` projects any receipt — axis subject off the `ArtifactKind` tag, evidence key off `slot`, measured ledger off the numeric `_facts` scalars, ceiling off `_CEILING` under the caller's tighter override — and `exchange/conformance#CONFORMANCE` binds it at its `_emit` fold, so the edge carries a live figure.
- [SPAN_POLICY]: span coverage is charter, never a per-page choice — a producer opens exactly ONE OpenTelemetry span when its render crosses the runtime lane onto a foreign native kernel whose interior stages the lane aspect cannot attribute, and every other producer emits receipts and the `_METRIC` projection with no span, because the runtime lane and serve weaves already own their timing.
- [SPAN_CLASS]: `typography/layout`, `typography/shape`, `visualization/chart/export`, and `visualization/dashboard` are the native-offload class — each gains its span at the offload site over the runtime-configured pipeline, so no artifacts-side tracer configuration exists. Rosters here are exhaustive by construction, so a producer opening a span without a row, or a row naming a producer that opens none, is the drift this class forecloses.
- [SPAN_ERROR]: every offload rail folds INSIDE the span scope — the Error arm sets `Status(StatusCode.ERROR)` and emits the structured error log from `fault.facts()` before the span closes, so a failed render never exits an `UNSET` span or an uncorrelated log. Attribution arrives ambient: the error line inherits the issue-scope key through `merge_contextvars` and tenant through `PROMOTED_BAGGAGE`, so a producer binding either re-owns a runtime seam.
- [SPAN_EXCEPTION]: `Span.record_exception` and the `dict_tracebacks` expansion are void at this seam — the lane rail carries the typed `BoundaryFault`, the live raise converted at the runtime `boundary` and unpicklable across the worker crossing, so the flat `facts()` projection is the whole error evidence.
- [SPAN_STAGE]: every producer whose span brackets more than one lane crossing marks each stage boundary with `Span.add_event` — the chart export's pre-pass is that class; a single-crossing producer carries no stage events, its worker interior unattributable by charter.

## [04]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[OPEN|BLOCKED]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
