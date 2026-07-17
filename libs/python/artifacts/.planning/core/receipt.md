# [PY_ARTIFACTS_RECEIPT]

`ArtifactReceipt` is the one kind-discriminated artifact-evidence family every production sub-domain mints a case onto — one `@tagged_union(frozen=True)` keyed by the runtime `ContentKey`, satisfying the runtime `receipts.ReceiptContributor` port through `contribute`. Each case is a `(ContentKey, <facts>)` payload of native scalars the producer calls positionally: byte-only kinds a bare pair, the evidence-rich kinds (`preview`/`color`/`media`/`cad`/`scene`/`office`/`document`) closing on one `frozendict` band as the LAST slot. Band evidence rides a declared two-grammar law: the `_BAND` row assigns each banded kind its namespace (`score`/`ink`/`stream`/`entity`/`view`/`finish`/`product`), `_facts` projects every band entry as `<namespace>.<fact>`, and every fixed fact name derives dot-free from its mint signature — so a band fact can never shadow canonical evidence or the reserved `"artifact"` discriminant, structurally, with no runtime allowlist. Case roster is the ONE derivation owner — the `ArtifactKind` spelling is pinned to it by a load gate and the fact-name table derives from the mint signatures for flat AND banded kinds — and every domain lands one case, so the receipt stream stays one fact family the planner, the reuse-fabric elision, and the `MeterProvider` read off one `contribute` fold rather than a parallel rail per producer.

`ConformanceVerdict` lives in the `verdict` case and is DECLARED ON THIS PAGE so `exchange/conformance` imports it DOWN and mints it while `delivery/transmittal` and `exchange/credential` read it off the receipt seam. `contribute` projects the active payload onto the runtime receipt spine and, at the same fold, records the numeric byte-volume and compression-ratio facts through the runtime `Metrics.record(domain="artifact", kind=...)` arm — the measured-signals seam, never a local metrics fief. A failed production is never an artifacts case: the producer's runtime `boundary` converts the raise into a `BoundaryFault` the spine's own `rejected` line carries. `graduates` is the one outward figure hand-off, projecting any receipt onto the compute graduation hub's artifacts-origin axis case.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over native-scalar `case()` payloads, its roster-derived `_CASES`/`_KEYS`/`_BAND` owner, the `contribute` projection onto the runtime spine, and the `graduates` outward hand-off.
- [02]-[SIGNALS]: the three receipt-fold consumer seams — reuse elision, measured signals, plan fabric — and the outward figure hand-off.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a `tuple[ContentKey, ...]` whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars, the evidence-rich `preview`/`color`/`media`/`cad`/`scene`/`office`/`document` kinds closing on one `frozendict` band as their last slot. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, and the union imports NO producer module — every case carries native scalars, the local `ConformanceVerdict`, or a `frozendict` band, so no `c2pa-python`/`av`/`pysubs2`/`pikepdf` surface crosses in.
- Cases: the case-shape rulings the fence tuples cannot show — `Office`/`Report`/`Document` share the `(key, bytes_)` head but split by producer origin (a workbook/slide container, a composed report, and the generic `typography` document-rail blob for an axis catalog, glyph run, or line-broken stream that is neither a PDF nor an office file), `Office` and `Document` closing on a defaulted `facts` band (`finish`/`product` namespaces) while `Report` stays byte-only; `Egress` is reused by `document/tagged#ACCESS`, which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Schedule` is a distinct AEC case beside `Table` because a schedule carries its NCS/AIA kind and item cardinality the generic publication `Table` does not; `Cad` is distinct because a DXF document carries version/units/salvage-auditor evidence no publication case holds; `Drawing` is ONE shared case across the drawing plane (`dimension`/`annotate`/`symbol`/`detail`), its `style` a neutral slot each producer fills, never a per-producer rail; the delivery pair `Register`/`Transmittal` are admitted as CASES beside `Spec`/`Drawing`/`Schedule`, never a parallel delivery-receipt rail; and the seven banded kinds close on a `frozendict` band so a heterogeneous per-producer fact set rides one case — `Preview` absorbs BOTH the perceptual float band and the machine-readable-mark string facts under one `float | str` union, and `Media` folds every media page's `av`/`pysubs2` evidence onto its band rather than a fixed-scalar bag whose subtitle/loudness/scene fields default to zero on the pages that never produce them. `Preview` and `Color` require a fixed `bytes_` slot so every raster and separation artifact records byte volume through the `_METRIC` fold rather than smuggling a `bytes` band fact past it or defaulting absent evidence to zero. Each mint is a `@classmethod` returning `Self`, binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the return type on every mint, and stays thin — keyword-constructing its `case()` and adding nothing — while the optional `scores`/`facts` bands and `Media.bit_rate` keep a band-free or unknown-rate encode valid.
- Bands: the band is a declared grammar, never an open bag — two band kinds exist and each is lawful on its own axis. A FACT band (`preview`/`color`/`media`/`scene`/`office`/`document`) carries producer-owned measurement vocabulary (the `graphic/raster/measure` transform scores, `graphic/color/managed`'s `ManagedFact` rows, the media loudness/scene facts, the office finishing and typography product facts) whose leaf names live at the producer that measures them; a DOMAIN-KEYED table (`cad` `counts`) carries domain names as keys — the uppercase DXF entity spellings — never fact names. Both project through the kind's `_BAND` namespace (`score.`/`ink.`/`stream.`/`view.`/`entity.`/`finish.`/`product.`), so a band entry is self-describing in the `EventDict`, cannot shadow a fixed fact (fixed names are dot-free by the load gate), and cannot emit the reserved `"artifact"` key. A band fact that graduates to a governed metric or ceiling first graduates to a FIXED slot on its case — the `bytes_` promotion is the precedent — so `_METRIC` and `_CEILING` read only declared fixed names.
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the port declares, appending the `ArtifactKind` discriminant under the reserved `"artifact"` key — reserved because `contribute` overwrites it last, so no `_facts` arm may emit it, and the inner `"kind"` fact the `diagram`/`schedule`/`register` cases carry and the `cad` DXF output format that rides `"format"` each stay distinct. At the same fold the measured-signals seam fires: every `_METRIC`-named numeric fact records through `Metrics.record(domain="artifact", kind=self.tag)`, a kind carrying no `_METRIC` fact recording nothing. It takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers.
- Auto: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose or-pattern captures `(key, *_)` once and closes on `assert_never`, where a reflective `getattr(self, self.tag)[0]` erases the key to `object` and defeats the exhaustiveness witness the planner keys on. `_facts` is the second total `match self`, three arms total: `verdict` spreads the local `ConformanceVerdict.facts()`, the ONE banded or-pattern destructures `(_key, *tail, band)` and joins the mint-derived fixed zip with the namespaced band spread, and the flat or-pattern zips each scalar tail against `_KEYS[self.tag]` under `strict=True`, keeping native ints/floats intact. Roster stays the ONE derivation owner: `_CASES` reads the case fields through `annotationlib.get_annotations` over the union, the first load gate raises at import when the `ArtifactKind` Literal and the roster drift, `_KEYS` derives every fact tail — banded kinds dropping the trailing band parameter — from its mint signature under the `removesuffix("_")` builtin-collision rule, and the second load gate raises when any fixed fact name — a mint-derived row or a spread `ConformanceVerdict` field off `structs.fields` — carries a dot or spells the reserved `"artifact"` — one declaration site, so renaming a mint parameter renames the emitted fact and an unlawful name breaks at import, never at a fold.
- Output: `graduates(self, *, ceiling=None)` is the one outward figure hand-off — it projects ANY receipt onto the compute graduation hub's artifacts-origin case, keyed by the receipt's own `slot` with no re-mint, `measured` the numeric `_facts` ledger (every `int`/`float` including namespaced band floats, `bool` excluded — the verdict flags are admission facts, not residuals) and `bars` the kind-scoped `_CEILING` row combined by pointwise minimum with the caller's tighter per-key override. Admission, the `content.graduate` span, and the `planned`-receipt egress stay compute's — this page composes the hub DOWNWARD and adds no second fence, span, or re-wrap, since a second `@receipted` over the returned rail double-streams the receipt. A `_CEILING` row names only fixed facts its keyed kind measures: `color` bars `tac_peak` at `320.0`, `cad` bars `errors` at `0.0`, and an ungoverned kind supplies no inherited bar; `model_asset` is a compute-own subject figures never ride, and the projection re-mints no canonical concept.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union, `Map` the derived `_KEYS`/`_METRIC`/`_CEILING` tables); `msgspec` (`Struct` + `structs.asdict` the re-homed `ConformanceVerdict`); the builtin `frozendict` (the case evidence bands and the `_BAND` namespace rows — msgspec-native and hashable where `Map` is not); stdlib `annotationlib.get_annotations` + `inspect.signature` (the `_CASES` roster and `_KEYS` mint-signature derivations); runtime (`identity.ContentKey`, `receipts.Receipt` and the structurally-satisfied `ReceiptContributor` port, `metrics.Metrics.record` the domain/kind arm, `faults.RuntimeRail`); compute (`graduation.handoff.GraduationReceipt`/`HandoffAxis`, imported downward). No producer module crosses in.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `case()` plus one `@classmethod` mint plus one `slot` or-pattern arm plus one `_facts` or-pattern alternative — the load gates and the `assert_never` tails break at import until every piece exists. A flat kind adds nothing else (its `_KEYS` row derives from the mint signature); a banded kind adds its `_BAND` namespace row, so a new band fact is one key the producer fills with zero receipt edit and the namespace projection lands it lawfully. A new scalar on a flat OR banded kind is one slot on its `case()` plus one mint parameter, the `strict=True` zip raising on drift; a new measured signal is one FIXED slot plus one `_METRIC` row here plus one `InstrumentSpec` row on the runtime metrics owner; a new governed residual bar is one `_CEILING` row over a fixed fact the barred kinds measure.
- Boundary: one union carries every domain's evidence as a case, never a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family or a parallel `ColorReceipt`/AEC/delivery rail beside it, and never a per-kind evidence `Struct` re-wrapping scalars the producer passes positionally; the `media` band carries each page's own evidence, never a monolithic fixed-scalar case defaulting the absent pages to zero. `_KEYS`, `_BAND`, and `ArtifactKind` derive from or pin to the roster, never a hand-synced parallel table; a band entry projects only under its kind's namespace, never spread bare where it stamps over a fixed fact; `ConformanceVerdict` is re-homed DOWN, never imported upward into the spine. Metrics route through `Metrics.record`, never a page-owned histogram/counter/logging fief, and no render DURATION fact rides a case — the runtime metrics aspect owns timing. Facts reach the `dict[str, object]` `EventDict` as native scalars, never `str()`-pre-formatted; `slot`/`_facts` read through the total `match`, never a reflective `getattr`; `contribute` carries no `phase` parameter and no artifacts-side `rejected` forwarder; `graduates` adds no second `@receipted` re-wrap; and the union imports no producer module.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from annotationlib import get_annotations
from collections.abc import Iterable, Mapping
from inspect import signature
from typing import Final, Literal, Self, assert_never, get_args

from builtins import frozendict
from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

type ArtifactKind = Literal[
    "pdf",
    "office",
    "report",
    "document",
    "chart",
    "scene",
    "table",
    "preview",
    "color",
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

# --- [MODELS] ---------------------------------------------------------------------------


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
    scene: tuple[ContentKey, str, int, frozendict[str, float | str]] = case()
    table: tuple[ContentKey, str, int] = case()
    preview: tuple[ContentKey, int, int, int, frozendict[str, float | str]] = case()
    color: tuple[ContentKey, str, str, float, int, int, frozendict[str, float | str]] = case()
    bundle: tuple[ContentKey, str, int, int, int, int, int, float] = case()
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
    def Bundle(cls, key: ContentKey, algo: str, level: int, dict_id: int, frame_size: int, entries: int, verified: int, ratio: float, /) -> Self:
        return cls(bundle=(key, algo, level, dict_id, frame_size, entries, verified, ratio))

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
                | ArtifactReceipt(tag="scene", scene=(key, *_))
                | ArtifactReceipt(tag="table", table=(key, *_))
                | ArtifactReceipt(tag="preview", preview=(key, *_))
                | ArtifactReceipt(tag="color", color=(key, *_))
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
            case (
                ArtifactReceipt(tag="preview", preview=(_key, *tail, band))
                | ArtifactReceipt(tag="color", color=(_key, *tail, band))
                | ArtifactReceipt(tag="media", media=(_key, *tail, band))
                | ArtifactReceipt(tag="cad", cad=(_key, *tail, band))
                | ArtifactReceipt(tag="scene", scene=(_key, *tail, band))
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
        measures = {name: float(v) for slot, name in _METRIC.items() if isinstance(v := facts.get(slot), int | float) and not isinstance(v, bool)}
        Metrics.record(measures, domain="artifact", kind=self.tag)
        return (Receipt.of("artifacts", ("emitted", self.slot.hex, {**facts, "artifact": self.tag})),)

    def graduates(self, /, *, ceiling: Mapping[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        measured = {k: float(v) for k, v in self._facts().items() if isinstance(v, int | float) and not isinstance(v, bool)}
        governed = dict(_CEILING.try_find(self.tag).default_value(Map.empty()).items())
        requested = dict(ceiling or {})
        bars = {name: min(requested.get(name, cap), cap) for name, cap in governed.items()}
        return GraduationReceipt.graduates("artifacts", HandoffAxis(artifact=self.tag), self.slot, measured, bars)


# --- [TABLES] ---------------------------------------------------------------------------

_CASES: Final[tuple[str, ...]] = tuple(f for f in get_annotations(ArtifactReceipt) if f != "tag")
if set(_CASES) != set(get_args(ArtifactKind.__value__)):
    raise RuntimeError("ArtifactKind drifted from the case roster")

_BAND: Final[frozendict[str, str]] = frozendict(
    {"preview": "score", "color": "ink", "media": "stream", "cad": "entity", "scene": "view", "office": "finish", "document": "product"}
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
if any("." in name or name == "artifact" for row in (*_KEYS.values(), tuple(f.name for f in structs.fields(ConformanceVerdict))) for name in row):
    raise RuntimeError("a fixed fact name collides with the band namespace grammar or the reserved discriminant")

_METRIC: Final[Map[str, str]] = Map.of_seq([("bytes", "artifact.byte_volume"), ("ratio", "artifact.compression_ratio")])

_CEILING: Final[Map[ArtifactKind, Map[str, float]]] = Map.of_seq([
    ("color", Map.of_seq([("tac_peak", 320.0)])),
    ("cad", Map.of_seq([("errors", 0.0)])),
])

# --- [EXPORTS] ----------------------------------------------------------------------------

__all__ = ("ArtifactKind", "ArtifactReceipt", "ConformanceVerdict")
```

## [03]-[SIGNALS]

- [REUSE_ELISION]: reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision. Artifacts holds the consumer edge: every producer mints its key over the INPUT spec pre-run, threads it into the lane admission, and `_emit` threads it into the terminal receipt so `slot` reads it back for the hit/miss distinction — no new case, no new owner, no re-minted key.
- [METRIC_SIGNALS]: measured-signal leg is COMPOSED at the `contribute` fold — the `_METRIC` rows project the present numeric facts onto `Metrics.record(domain="artifact", kind=...)`, keyed by the carried kind, so per-kind size/ratio distributions route off one stream with zero artifacts-local metric state. Render DURATION is NOT a receipt fact — the runtime metrics aspect times the serve coroutine. A new upstream `domain="artifact"` instrument is one `_METRIC` row here.
- [PLAN_FABRIC]: production-planning leg is the third receipt-fold CONSUMER — `core/plan#PLAN` reads each producer's resolved `ArtifactReceipt` as the content-keyed evidence its sub-graph elision distinguishes a hit from a miss on. `planned`-stage observability is the planner's OWN direct `Receipt.of` emit, so the `admitted`/`planned`/`emitted` line family is shared by that emit and the producers' `emitted` facts, never a phase threaded through `contribute`.
- [FIGURE_HANDOFF]: outward figure edge to a sibling package travels the compute `graduation/handoff#GRADUATION` rail on its artifacts-origin `artifact` case. `graduates` projects any receipt — the axis subject the `ArtifactKind` tag, the evidence key the receipt's own `slot`, the measured ledger the numeric `_facts` scalars, the ceiling the governed `_CEILING`. `model_asset` is a compute-own subject figures never ride, and the returned rail is compute's own egress, never re-wrapped here.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
