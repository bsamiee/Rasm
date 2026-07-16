# [PY_ARTIFACTS_RECEIPT]

`ArtifactReceipt` is the one kind-discriminated artifact-evidence family every production sub-domain mints a case onto — one `@tagged_union(frozen=True)` keyed by the runtime `ContentKey`, satisfying the runtime `receipts.ReceiptContributor` port through `contribute`. Each case is a `(ContentKey, <facts>)` payload of native scalars the producer calls positionally: byte-only kinds a bare pair, the evidence-rich kinds (`preview`/`color`/`media`/`cad`/`scene`) closing on one `frozendict` band that absorbs a heterogeneous per-producer fact set without a fixed-scalar bag. The case roster is the ONE derivation owner — the `ArtifactKind` spelling is pinned to it by a load gate and the flat fact-name table derives from the mint signatures, so no hand-synced parallel list drifts — and every domain lands one case, so the receipt stream stays one fact family the planner, the reuse-fabric elision, and the `MeterProvider` read off one `contribute` fold rather than a parallel rail per producer.

The `verdict` case stores `ConformanceVerdict`, a value object DECLARED ON THIS PAGE so `exchange/conformance` imports it DOWN and mints it while `delivery/transmittal` and `exchange/credential` read it off the receipt seam. `contribute` projects the active payload onto the runtime receipt spine and, at the same fold, records the numeric byte-volume and compression-ratio facts through the runtime `Metrics.record(domain="artifact", kind=...)` arm — the measured-signals seam, never a local metrics fief. A failed production is never an artifacts case: the producer's runtime `boundary` converts the raise into a `BoundaryFault` the spine's own `rejected` line carries. `graduates` is the one outward figure hand-off, projecting any receipt onto the compute graduation hub's artifacts-origin axis case.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over native-scalar `case()` payloads, its roster-derived `_CASES`/`_KEYS` owner, the `contribute` projection onto the runtime spine, and the `graduates` outward hand-off.
- [02]-[SIGNALS]: the three receipt-fold consumer seams — reuse elision, measured signals, plan fabric — and the outward figure hand-off.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a `tuple[ContentKey, ...]` whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars, the evidence-rich `preview`/`color`/`media`/`cad`/`scene` kinds closing on one `frozendict` band as their last slot. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, and the union imports NO producer module — every case carries native scalars, the local `ConformanceVerdict`, or a `frozendict` band, so no `c2pa-python`/`av`/`pysubs2`/`pikepdf` surface crosses in.
- Cases: the case-shape rulings the fence tuples cannot show — the byte-only trio `Office`/`Report`/`Document` share a shape but split by producer origin (a workbook/slide container, a composed report, and the generic `typography` document-rail blob for an axis catalog, glyph run, or line-broken stream that is neither a PDF nor an office file); `Egress` is reused by `document/tagged#ACCESS`, which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Schedule` is a distinct AEC case beside `Table` because a schedule carries its NCS/AIA kind and item cardinality the generic publication `Table` does not; `Cad` is distinct because a DXF document carries version/units/salvage-auditor evidence no publication case holds; `Drawing` is ONE shared case across the drawing plane (`dimension`/`annotate`/`symbol`/`detail`), its `style` a neutral slot each producer fills, never a per-producer rail; the delivery pair `Register`/`Transmittal` are admitted as CASES beside `Spec`/`Drawing`/`Schedule`, never a parallel delivery-receipt rail; and the five banded kinds close on a `frozendict` band so a heterogeneous per-producer fact set rides one case — `Preview` absorbs BOTH the perceptual float band and the machine-readable-mark string facts under one `float | str` union, and `Media` folds every media page's `av`/`pysubs2` evidence onto its band rather than a fixed-scalar bag whose subtitle/loudness/scene fields default to zero on the pages that never produce them. Each mint is a `@classmethod` returning `Self`, binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the return type on every mint, and stays thin — keyword-constructing its `case()` and adding nothing — while the defaulted slots (`scores`/`facts`/`bytes_`/`bit_rate`) keep a bare save and a band-free encode valid.
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the port declares, appending the `ArtifactKind` discriminant under the reserved `"artifact"` key — reserved because `contribute` overwrites it last, so no `_facts` arm may emit it, and the inner `"kind"` fact the `diagram`/`schedule`/`register` cases carry and the `cad` DXF output format that rides `"format"` each stay distinct. At the same fold the measured-signals seam fires: every `_METRIC`-named numeric fact records through `Metrics.record(domain="artifact", kind=self.tag)`, a kind carrying no `_METRIC` fact recording nothing. It takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers.
- Auto: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose or-pattern captures `(key, *_)` once and closes on `assert_never`, where a reflective `getattr(self, self.tag)[0]` erases the key to `object` and defeats the exhaustiveness witness the planner keys on. `_facts` is the second total `match self`: its general arm zips each flat scalar tail against the mint-derived `_KEYS[self.tag]` field-name row under `strict=True`, keeping native ints/floats intact, while `preview`/`color`/`media`/`cad`/`scene` flatten their `frozendict` band and `verdict` spreads the local `ConformanceVerdict.facts()`. The roster is the ONE derivation owner: `_CASES` reads the case fields off the union's annotations, a load gate raises at import when the `ArtifactKind` Literal and the roster drift, and `_KEYS` derives each flat fact tail from its mint signature under the `removesuffix("_")` builtin-collision rule — one declaration site, so renaming a mint parameter renames the emitted fact.
- Output: `graduates(self, *, ceiling=None)` is the one outward figure hand-off — it projects ANY receipt onto the compute graduation hub's artifacts-origin case, keyed by the receipt's own `slot` with no re-mint, `measured` the numeric `_facts` ledger (every `int`/`float`, `bool` excluded — the verdict flags are admission facts, not residuals) and `bars` the governed `_CEILING` row under the caller's tighter per-key override. Admission, the `content.graduate` span, and the `planned`-receipt egress stay compute's — this page composes the hub DOWNWARD and adds no second fence, span, or re-wrap, since a second `@receipted` over the returned rail double-streams the receipt. A `_CEILING` row names only barred facts and every named key must be measured, so a governed bar lands over a fact the barred kinds actually carry; `model_asset` is a compute-own subject figures never ride, and the projection re-mints no canonical concept.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union, `Map` the derived `_KEYS`/`_METRIC`/`_CEILING` tables); `msgspec` (`Struct` + `structs.asdict` the re-homed `ConformanceVerdict`); the builtin `frozendict` (the case evidence bands — msgspec-native and hashable where `Map` is not); stdlib `inspect.signature` (the `_KEYS` mint-signature derivation); runtime (`identity.ContentKey`, `receipts.Receipt` and the structurally-satisfied `ReceiptContributor` port, `metrics.Metrics.record` the domain/kind arm, `faults.RuntimeRail`); compute (`graduation.handoff.GraduationReceipt`/`HandoffAxis`, imported downward). No producer module crosses in.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `case()` plus one `@classmethod` mint plus one `slot` or-pattern arm — the load gate and the `assert_never` tails break at import until every piece exists. A flat kind adds nothing else (its `_KEYS` row derives from the mint signature); a banded kind adds its `_BANDED` entry plus one `_facts` flatten arm, so a new band fact is one key the producer fills with zero receipt edit. A new scalar on a flat kind is one slot on its `case()` plus one mint parameter, the `strict=True` zip raising on drift; a new measured signal is one `_METRIC` row here plus one `InstrumentSpec` row on the runtime metrics owner; a new governed residual bar is one `_CEILING` row.
- Boundary: one union carries every domain's evidence as a case, never a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family or a parallel `ColorReceipt`/AEC/delivery rail beside it, and never a per-kind evidence `Struct` re-wrapping scalars the producer passes positionally; the `media` band carries each page's own evidence, never a monolithic fixed-scalar case defaulting the absent pages to zero. `_KEYS` and `ArtifactKind` derive from the roster, never a hand-synced parallel table; `ConformanceVerdict` is re-homed DOWN, never imported upward into the spine. Metrics route through `Metrics.record`, never a page-owned histogram/counter/logging fief, and no render DURATION fact rides a case — the runtime metrics aspect owns timing. Facts reach the `dict[str, object]` `EventDict` as native scalars, never `str()`-pre-formatted; `slot`/`_facts` read through the total `match`, never a reflective `getattr`; `contribute` carries no `phase` parameter and no artifacts-side `rejected` forwarder; `graduates` adds no second `@receipted` re-wrap; and the union imports no producer module.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable, Mapping
from inspect import signature
from typing import Final, Literal, Self, assert_never, get_args

from expression import case, tag, tagged_union
from expression.collections import Map
from msgspec import Struct, structs

from rasm.compute.graduation.handoff import GraduationReceipt, HandoffAxis
from rasm.runtime.faults import RuntimeRail
from rasm.runtime.identity import ContentKey
from rasm.runtime.metrics import Metrics
from rasm.runtime.receipts import Receipt

# --- [TYPES] ----------------------------------------------------------------------------

# pinned to the case roster by the [TABLES] load gate; a token without its case (or the reverse) fails at import.
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
    # re-homed onto the receipt spine (the shape's consumers all sit on this seam): `exchange/conformance`
    # imports it DOWN and mints it; `document/tagged` contributes `.Egress`/`.Pdf` bands and never reads it.
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
    pdfa_claim: str
    pdfx_claim: str

    def facts(self) -> dict[str, object]:
        return structs.asdict(self)


@tagged_union(frozen=True)
class ArtifactReceipt:
    # every case is `(ContentKey, *scalars)`; `preview`/`color`/`media`/`cad`/`scene` close on a `frozendict` band.
    tag: ArtifactKind = tag()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    document: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    scene: tuple[ContentKey, str, int, frozendict[str, float | str]] = case()
    table: tuple[ContentKey, str, int] = case()
    preview: tuple[ContentKey, int, int, frozendict[str, float | str]] = case()
    color: tuple[ContentKey, str, str, float, int, frozendict[str, float | str]] = case()
    bundle: tuple[ContentKey, str, int, int, int, int, int, float] = case()
    introspection: tuple[ContentKey, int, int, int, int] = case()
    egress: tuple[ContentKey, int, int, int, int, int] = case()
    verdict: tuple[ContentKey, ConformanceVerdict] = case()
    credential: tuple[ContentKey, str, str, int, str] = case()
    media: tuple[ContentKey, str, str, float, int, int, int, frozendict[str, float | str]] = case()
    diagram: tuple[ContentKey, str, int, int, str] = case()
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
    def Office(cls, key: ContentKey, bytes_: int, /) -> Self:
        return cls(office=(key, bytes_))

    @classmethod
    def Report(cls, key: ContentKey, bytes_: int, /) -> Self:
        return cls(report=(key, bytes_))

    @classmethod
    def Document(cls, key: ContentKey, bytes_: int, /) -> Self:
        return cls(document=(key, bytes_))

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
    def Preview(cls, key: ContentKey, width: int, height: int, scores: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(preview=(key, width, height, scores))

    @classmethod
    def Color(cls, key: ContentKey, space: str, intent: str, tac_peak: float, plates: int, facts: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(color=(key, space, intent, tac_peak, plates, facts))

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
    def Diagram(cls, key: ContentKey, kind: str, nodes: int, edges: int, algorithm: str, /) -> Self:
        return cls(diagram=(key, kind, nodes, edges, algorithm))

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
        artifact: str,
        bytes_: int,
        layers: int,
        blocks: int,
        errors: int,
        fixes: int,
        counts: frozendict[str, int],
        /,
    ) -> Self:
        return cls(cad=(key, dxfversion, units, artifact, bytes_, layers, blocks, errors, fixes, counts))

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
        # the shared `ContentKey` head, typed by the total `match`; a `getattr(self, self.tag)[0]`
        # would erase it to `object` and defeat the exhaustiveness witness the planner keys on.
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
        # general arm zips each flat tail against its mint-derived `_KEYS` row (`strict=True` flags drift).
        match self:
            case ArtifactReceipt(tag="preview", preview=(_key, width, height, scores)):
                return {"width": width, "height": height, **scores}
            case ArtifactReceipt(tag="color", color=(_key, space, intent, tac_peak, plates, facts)):
                return {"space": space, "intent": intent, "tac_peak": tac_peak, "plates": plates, **facts}
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return {**verdict.facts()}
            case ArtifactReceipt(tag="media", media=(_key, container, codec, duration, bytes_, frames, bit_rate, facts)):
                return {
                    "container": container,
                    "codec": codec,
                    "duration": duration,
                    "bytes": bytes_,
                    "frames": frames,
                    "bit_rate": bit_rate,
                    **facts,
                }
            case ArtifactReceipt(tag="cad", cad=(_key, dxfversion, units, artifact, bytes_, layers, blocks, errors, fixes, counts)):
                return {
                    "dxfversion": dxfversion,
                    "units": units,
                    "format": artifact,
                    "bytes": bytes_,
                    "layers": layers,
                    "blocks": blocks,
                    "errors": errors,
                    "fixes": fixes,
                    **counts,
                }  # DXF output format rides `"format"`, never `"artifact"` (the reserved discriminant)
            case ArtifactReceipt(tag="scene", scene=(_key, target, bytes_, facts)):
                return {"target": target, "bytes": bytes_, **facts}
            case (
                ArtifactReceipt(tag="pdf", pdf=(_key, *tail))
                | ArtifactReceipt(tag="office", office=(_key, *tail))
                | ArtifactReceipt(tag="report", report=(_key, *tail))
                | ArtifactReceipt(tag="document", document=(_key, *tail))
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
        # phase is the constant `"emitted"`; `"artifact"` is the reserved discriminant appended last. The
        # measured-signals seam fires here: every `_METRIC`-named numeric fact records through the domain/kind arm.
        facts = self._facts()
        measures = {name: float(v) for slot, name in _METRIC.items() if isinstance(v := facts.get(slot), int | float) and not isinstance(v, bool)}
        if measures:
            Metrics.record(measures, domain="artifact", kind=self.tag)
        return (Receipt.of("artifacts", ("emitted", self.slot.hex, {**facts, "artifact": self.tag})),)

    def graduates(self, /, *, ceiling: Mapping[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        # any receipt projects onto compute's artifacts-origin axis, keyed by the already-minted `slot`; the
        # governed `_CEILING` merges under the caller's tighter override. Admission, span, egress stay compute's.
        measured = {k: float(v) for k, v in self._facts().items() if isinstance(v, int | float) and not isinstance(v, bool)}
        bars = {**dict(_CEILING.items()), **(dict(ceiling) if ceiling is not None else {})}
        return GraduationReceipt.graduates("artifacts", HandoffAxis(artifact=self.tag), self.slot, measured, bars)


# --- [TABLES] ---------------------------------------------------------------------------

# the case roster is the ONE owner: `_CASES` reads it off the union's annotations, the load gate pins
# the `ArtifactKind` spelling to it, and `_KEYS` derives each flat fact tail from its mint signature.
_CASES: Final[tuple[str, ...]] = tuple(f for f in ArtifactReceipt.__annotations__ if f != "tag")
if set(_CASES) != set(get_args(ArtifactKind.__value__)):
    raise RuntimeError("ArtifactKind drifted from the case roster")

# the banded kinds fold specially in `_facts` (band flatten / `facts()` spread) and carry no derived row.
_BANDED: Final[frozenset[str]] = frozenset({"preview", "color", "media", "cad", "scene", "verdict"})

_KEYS: Final[Map[str, tuple[str, ...]]] = Map.of_seq(
    (kind, tuple(n.removesuffix("_") for n in tuple(signature(getattr(ArtifactReceipt, kind.capitalize())).parameters)[1:]))
    for kind in _CASES
    if kind not in _BANDED
)

# measured-signals rows: fact key -> the runtime `domain="artifact"` instrument name; a new signal is a
# row here + a runtime InstrumentSpec row.
_METRIC: Final[Map[str, str]] = Map.of_seq([("bytes", "artifact.byte_volume"), ("ratio", "artifact.compression_ratio")])

# governed residual bar every outward figure clears, a caller's tighter row overriding per key; a bar names
# a fact the barred kinds measure, never a per-call literal.
_CEILING: Final[Map[str, float]] = Map.empty()
```

## [03]-[SIGNALS]

- [REUSE_ELISION]: the reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision. The artifacts side is the consumer edge: every producer mints its key over the INPUT spec pre-run, threads it into the lane admission, and `_emit` threads it into the terminal receipt so `slot` reads it back for the hit/miss distinction — no new case, no new owner, no re-minted key.
- [METRIC_SIGNALS]: the measured-signal leg is COMPOSED at the `contribute` fold — the `_METRIC` rows project the present numeric facts onto `Metrics.record(domain="artifact", kind=...)`, keyed by the carried kind, so per-kind size/ratio distributions route off one stream with zero artifacts-local metric state. Render DURATION is NOT a receipt fact — the runtime metrics aspect times the serve coroutine. A new upstream `domain="artifact"` instrument is one `_METRIC` row here.
- [PLAN_FABRIC]: the production-planning leg is the third receipt-fold CONSUMER — `core/plan#PLAN` reads each producer's resolved `ArtifactReceipt` as the content-keyed evidence its sub-graph elision distinguishes a hit from a miss on. The `planned`-stage observability is the planner's OWN direct `Receipt.of` emit, so the `admitted`/`planned`/`emitted` line family is shared by that emit and the producers' `emitted` facts, never a phase threaded through `contribute`.
- [FIGURE_HANDOFF]: the outward figure edge to a sibling package travels the compute `graduation/handoff#GRADUATION` rail on its artifacts-origin `artifact` case. `graduates` projects any receipt — the axis subject the `ArtifactKind` tag, the evidence key the receipt's own `slot`, the measured ledger the numeric `_facts` scalars, the ceiling the governed `_CEILING`. `model_asset` is a compute-own subject figures never ride, and the returned rail is compute's own egress, never re-wrapped here.

## [04]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
-->

(none)
