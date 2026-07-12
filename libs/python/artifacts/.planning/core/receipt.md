# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact-evidence family every production sub-domain mints a case onto, keyed by the runtime `ContentKey` and satisfying the runtime `receipts.ReceiptContributor` port structurally. `ArtifactReceipt` is one `@tagged_union(frozen=True)` whose roster is FIXED AT 23 cases — every case a `(ContentKey, <facts>)` payload of native scalars, the byte-only kinds a bare pair, the evidence-rich kinds (`preview`/`color`/`media`/`cad`/`scene`) closing on one `frozendict` band that absorbs a heterogeneous per-producer fact set without a fixed-scalar bag — the exact shape the producers already call, never a positional fact-tuple decoded by index and never a per-kind evidence `Struct` re-wrapping the scalars the caller hands in. Documents, reporting, typography, charts, scenes, tables, imaging, color management, compression, recovery, finishing, conformance, provenance, media, diagrams, descriptive metadata, the AEC drawing plane, AEC schedules, construction specifications, CAD exchange, and the ISO 19650 delivery register and transmittal issue each land one case through one named `@classmethod` mint returning `Self`, so the receipt stream is one fact family the `core/plan#PLAN` content-keyed planner, the runtime reuse-fabric elision, and the `observability/metrics#METRIC` `MeterProvider` read off one `contribute` fold, never a parallel rail per producer. Every read of the closed union — `slot` and `_facts` — is a total `match self` closed by `assert_never`, never a reflective `getattr(self, self.tag)` whose `object` residual escapes the match and makes the totality witness a lie. The case roster is the ONE derivation owner: the `ArtifactKind` spelling is pinned to it by a load gate and the flat fact-name table derives from the mint signatures, so no hand-synced parallel list can drift.

The owner composes the runtime receipt spine, never re-states it: the runtime `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` mints the lifecycle `fact` case from this page's `("emitted", subject, facts)` triple, and the runtime `@receipted(redaction)` aspect harvests this contributor stream and emits it. At the same `contribute` seam the numeric byte-volume/compression-ratio facts record through the runtime `Metrics.record(measures, domain="artifact", kind=...)` arm — the measured-signals seam, never a local logging fief. A failed production is never an artifacts case: the producer's runtime `boundary`/`async_boundary` converts the raise into a `BoundaryFault` on the `Result.Error` rail and the spine's own `Receipt.of("artifacts", fault)` mints the `rejected` line, so this page never re-spells a forwarder around that factory. This page owns the artifacts-side evidence vocabulary, its projection onto that spine, the re-homed `ConformanceVerdict` shape (`exchange/conformance` imports it DOWN and mints it), and the one outward figure hand-off — `graduates` projecting any receipt onto the compute graduation hub's artifacts-origin axis case.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over native-scalar `case()` payloads (the `preview`/`color`/`media`/`cad`/`scene` evidence-rich kinds closing on one `frozendict` band), the re-homed `ConformanceVerdict` value object the `verdict` case stores and `exchange/conformance` mints, the named per-kind `@classmethod` mints returning `Self`, the roster-derived `_CASES`/`_KEYS` owner (load-gated against the `ArtifactKind` Literal, fact names read off the mint signatures), the `contribute` projection onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` triple with the `ArtifactKind` discriminant carried on the facts and the `Metrics.record` measured-signals seam composed at the same fold, the total-`match` `slot` projection the planner and the reuse-fabric key on, and the `graduates` outward hand-off rail projecting any receipt onto the compute `HandoffAxis` artifacts-origin case under the governed `_CEILING` policy row. The `admitted`/`planned` lifecycle facts are the `core/plan#PLAN` planner's own direct `Receipt.of` emit, not an `ArtifactReceipt` case, and a failed production is the runtime spine's own `Receipt.of("artifacts", fault)` `rejected` line off the `reliability/faults#FAULT` `BoundaryFault`, never an artifacts case.
- [02]-[SIGNALS]: the three receipt-fold consumer seams (reuse elision, measured signals, plan fabric) and the outward figure hand-off, each recorded as landed fact.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a `tuple[ContentKey, ...]` `case()` of native scalars whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars — the evidence-rich `preview`/`color`/`media`/`cad`/`scene` kinds closing on one `frozendict` band as their last slot so a heterogeneous per-producer fact set rides one case rather than a per-page shape. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, which returns the one-element `Iterable[Receipt]` the port declares by projecting the active payload onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` contract — `"artifacts"` the owner, and the `(Phase, str, dict[str, object])` evidence triple (its phase the constant `"emitted"`, an `ArtifactReceipt` being by construction an emitted artifact's evidence) the runtime `of` matches onto its `fact` case. The `verdict` case stores `ConformanceVerdict`, DECLARED ON THIS PAGE: the shape lives on the receipt spine so `exchange/conformance` (s3) imports it down and mints it, `delivery/transmittal` and `exchange/credential` read it off the receipt seam, and `document/tagged` contributes `.Egress`/`.Pdf` receipt bands and never reads the verdict — the union therefore imports NO producer module, and every case carries native scalars, the local verdict value object, or a native `frozendict` band, so no `c2pa-python`/`av`/`pysubs2`/`drawsvg`/`pikepdf` surface crosses into this owner.
- Cases: each `case()` payload is `(ContentKey, <scalars>)` carrying the producer facts directly. `Pdf(key, bytes, pages)` the byte/page counts; `Office(key, bytes)`/`Report(key, bytes)`/`Document(key, bytes)` the byte-only finishing scalar three tags share — `Office` a workbook/slide container, `Report` a composed report, `Document` the generic document-rail blob the `typography/font#FONT`/`shape#SHAPE`/`layout#LAYOUT`/`math` arms mint for an axis catalog, positioned glyph run, or break/line-broken stream that is neither a PDF nor an office file. `Chart(key, engine, dialect, scale, theme, bytes)` (the engine the matched `ChartSpec.tag`); `Scene(key, target, bytes, facts)` the target, the produced byte count, and one `frozendict` render-evidence band (the `scene/render#SCENE` pyvista point/cell/window facts and the `scene/stage#STAGE` usd-core prim/layer/up-axis/meters-per-unit facts, flattened by the `scene` `_facts` arm exactly as `preview` flattens `scores`), and `Table(key, format, bytes)` the format plus the produced byte count (the `bytes_` slot defaulting `0` and `Scene`'s `facts` defaulting empty so a bare save is untouched); `Preview(key, width, height, scores)` where `scores: frozendict[str, float | str] = frozendict()` carries BOTH the `graphic/raster/measure#MEASURE` float perceptual band (`structural_similarity`/`peak_signal_noise_ratio`/`mean_squared_error`/`normalized_root_mse`/`normalized_mutual_information`/`hausdorff_distance`, plus the `contours`/`entropy`/`blobs`/`corners` and `shift`/`error` measurement facts) AND the `graphic/marks#MARK` string symbology/decode facts (the segno `designator`, the python-barcode `fullcode` check digit, the zxing `format`/`ec_level`, the decode `text`/`valid` round-trip) — an empty band for a bare save, the `RasterFact.score` row threaded through for a measured/encoded one, the same mint absorbing perceptual, raster, and machine-readable-mark evidence under the `float | str` value union; `Color(key, space, intent, tac_peak, plates, facts)` the `graphic/color/managed#MANAGED` LUT/plate/swatch/plate-set terminal evidence — the working color `space`, the rendering `intent`, the measured TAC ink-coverage `tac_peak` percent the TAC gate ruled on, the Separation/DeviceN `plates` count, and one `frozendict` band (per-plate coverage, spot names, LUT grid size, CxF3 record counts) the `color` `_facts` arm flattens exactly as `preview` flattens `scores`; `Bundle(key, algo, level, dict_id, frame_size, entries, verified, ratio)` the `package/codec#CODEC` `BundleEvidence` projection (the `entries`/`verified` slots the container codecs fill and the single-blob codecs leave zero); `Introspection(key, nodes, text_len, images, hits)` the `document/lens#LENS` recovered-tree shape; `Egress(key, bytes, pages, encryption_r, outline_depth, overlays)` the `document/egress#FINISH` finishing facts, reused by `document/tagged#ACCESS` which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Verdict(key, verdict)` the re-homed `ConformanceVerdict`; `Credential(key, manifest_id, signer, assertions, validation_state)` the `exchange/credential#PROVENANCE` C2PA store walk; `Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` the `media/container#CONTAINER`/`media/audio#MEDIA`/`media/*` encode read — the six shared `MediaEvidence` scalars, the `bit_rate` slot the `av` `VideoStream.bit_rate` read computes (defaulting `0`), and the `facts: frozendict[str, float | str] = frozendict()` per-page evidence band (empty for a bare video encode) the media producers spread their own `av`/`pysubs2` evidence onto: the `container` HDR-color tag and HLS/DASH segment count, the `filtergraph` filter-node count, the `audio` EBU R128 integrated-LUFS/true-peak-dBTP/loudness-range, the `subtitle` event/style counts, the `analysis` scene-cut/silence spans plus the two-pass `av.filter.loudnorm.stats` integrated-LUFS/true-peak/LRA band, the `timeline` clip/segment counts and lossless-vs-reencode strategy, and the `synthesis` fundamental-Hz/waveform/duration params — flattened by the `media` `_facts` arm exactly as `preview` flattens `scores`, so the ONE shared `Media` case absorbs every media page's heterogeneous evidence without a permissive fixed-scalar bag whose subtitle/loudness/scene fields would default to zero on the pages that never produce them; `Diagram(key, kind, nodes, edges, algorithm)` the `visualization/diagram/layout#LAYOUT` coordinate-assignment shape; `Metadata(key, carrier, fields, bytes)` the `exchange/metadata#METADATA` descriptive-metadata evidence (the `MetaCarrier` RASTER/PDF/MEDIA discriminant the producer fills slot-two with, the `MetaFacts.populated` descriptive-field tally, and the payload byte length); `Drawing(key, kind, entities, style, width, height, bytes)` the AEC drawing-plane evidence the `drawing/dimension#DIMENSION`/`annotate`/`symbol`/`detail` producers contribute (the drawing `kind`, the entity count, the drawing-plane `style` descriptor — one neutral slot every drawing producer fills with its own style/convention/engine token — the rendered extents, and the byte count), one shared case across the drawing plane rather than a per-producer receipt rail; `Schedule(key, kind, rows, columns, format, bytes)` the AEC-scheduling evidence the `drawing/schedule#SCHEDULE` producer contributes (the schedule/legend `kind`, the scheduled-item and column counts, the `TableFormat`, and the rendered byte count) — a distinct AEC-plane case beside `Table`, since a schedule carries its NCS/AIA kind and item cardinality where the generic publication `Table` case does not; `Spec(key, section, division, parts, articles, bytes)` the `specification/section#SECTION` construction-specification evidence (the MasterFormat section number, the `ClassCode.division()` head the `classify#CLASSIFY` crosswalk keys on, the count of present `SectionPart`s, the total article count, and the encoded `DocumentNode`-tree byte count); `Cad(key, dxfversion, units, artifact, bytes, layers, blocks, errors, fixes, counts)` the `export/dxf#DXF` CAD-exchange evidence (the DXF version, the units name, the `artifact` format the bytes ARE, the output byte count, the `doc.layers`/`doc.blocks` roster counts, the `Auditor` error+fix counts a salvaged `Recover` carries non-zero, and the `counts` per-dxftype census the `_facts` `cad` arm flattens), a distinct case since a DXF document carries its version/units/salvage-auditor evidence no publication case holds; `Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` the `delivery/register#REGISTER` ISO 19650 delivery-register evidence (the `RegisterOp` kind — `index`/`container`/`audit`/`render` — the information-container count, the register-aggregate `SuitabilityCode`, the `RevisionCode`, the classification-system token, the `Container`-XML schema-conformance state — the `isoschematron.Schematron` verdict `"valid"`/`"invalid:{n}"` the register folds off the built ISO 19650 tree before the C14N byte egress, `"unchecked"` for the non-XML ops that mint no validatable container — and the produced byte count); and `Transmittal(key, transmittal_id, sheets, suitability, container, validation_state)` the `delivery/transmittal#TRANSMITTAL` ISO 19650 issue-for-construction evidence (the transmittal number, the issued-sheet count, the purpose-of-issue suitability, the sealed-container content-key hex, and the folded PAdES/C2PA signed verdict `"valid"`/`"invalid"`/`"unsigned"`) — the delivery-plane pair admitted as CASES beside `Spec`/`Drawing`/`Schedule`, never a parallel delivery-receipt rail.
- Mints: each named `@classmethod` returning `Self` is the public construction surface a producer calls — `ArtifactReceipt.Pdf(key, bytes, pages)`, `ArtifactReceipt.Color(key, space, intent, tac_peak, plates, facts)`, `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` — the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the `@classmethod`-plus-`Self` form binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the return type on every mint. The mints are thin: each keyword-constructs its `case()` and adds nothing, so the producer reads the natural positional call its `_emit` arm already writes while the union owns the one interior — and the mint parameter names are LOAD-BEARING: `_KEYS` derives each flat kind's fact names from its mint signature (`bytes_`→`bytes`, `format_`→`format` under the `removesuffix("_")` builtin-collision rule), so renaming a mint parameter renames the emitted fact. `Preview` defaults `scores=frozendict()`, `Color` defaults `facts=frozendict()`, `Scene` defaults `bytes_=0` and `facts=frozendict()`, `Table` defaults `bytes_=0`, and `Media` defaults `bit_rate=0` and `facts=frozendict()`, so a bare save and a band-free encode stay valid while the richer slots wait for the producer's measured row, `len(data)`, rate read, or per-page evidence band.
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares — `(Receipt.of("artifacts", ("emitted", self.slot.hex, {**facts, "artifact": self.tag})),)` — the one-tuple shape the sibling `compute/graduation/handoff#GRADUATION` `GraduationReceipt.contribute` holds, never a bare `Receipt` against the `Iterable` port. The facts carry the `ArtifactKind` discriminant under the reserved `"artifact"` key — reserved because `contribute` overwrites it last, so no `_facts` arm may emit it: the `"kind"` inner fact the `diagram`/`schedule`/`register` cases carry and the `cad` DXF output format (which rides `"format"`, never `"artifact"`) each stay distinct from the appended discriminant. At the same fold the measured-signals seam fires: every `_METRIC`-named numeric fact present records through the runtime `Metrics.record(measures, domain="artifact", kind=self.tag)` polymorphic arm — each measure name resolved through the runtime `_DOMAIN_SLOT` onto its `artifact.byte_volume`/`artifact.compression_ratio` HISTOGRAM row — so per-kind size/ratio distributions route off the one landed instrument table with zero artifacts-local metric state; a kind carrying no `_METRIC` fact records nothing. `contribute` takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers (KNOB_TEST); the `admitted`/`planned` lifecycle facts ride the `core/plan#PLAN` planner's OWN direct `Receipt.of("artifacts", ("planned", ...))`, and a failed production rides the runtime spine's `Receipt.of("artifacts", fault)` `rejected` line.
- Graduation: `graduates(self, *, ceiling=None)` is the one outward figure hand-off — it projects ANY receipt onto the compute graduation hub's artifacts-origin case, `GraduationReceipt.graduates("artifacts", HandoffAxis(artifact=self.tag), self.slot, measured, bars)`, where `measured` is the numeric `_facts` ledger (every `int`/`float` fact, `bool` excluded — the verdict flags are admission facts, not residuals) and `bars` is the governed `_CEILING` policy row under the caller's tighter per-key override. The evidence key is the receipt's own `slot` — the `ContentIdentity`-minted `ContentKey` the producer threaded pre-run — so the hand-off is keyed by content identity with no re-mint. Admission, the `content.graduate` span, the residual-over-ceiling `_clear` fold, and the `planned`-receipt egress all stay compute's: this page composes the hub DOWNWARD and adds no second fence, no second span, and no re-wrap (a second `@receipted` over the returned rail would double-stream the receipt). A ceiling row names only the facts it bars and every named key must be measured (`_clear` requires `measured ⊇ ceiling` keys), so a governed bar lands as a `_CEILING` row over a fact the barred kinds actually carry; `model_asset` is a compute-own subject figures never ride, and the projection re-mints no canonical concept, so the runtime evidence `Structural.drift` query stays clean.
- Derivations: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose 23-tag or-pattern captures `(key, *_)` once and closes on `assert_never`, so the planner keys, the reuse-fabric elides, and the `MeterProvider` correlates on the one projection — sound over the closed union where a reflective `getattr(self, self.tag)` whose `tuple[object, ...]` residual erases the `ContentKey` type and defeats the exhaustiveness witness is the deleted form. `_facts` is the second total `match self` closed by `assert_never`: its general 17-tag or-pattern binds the case's scalar tail `(_key, *tail)` and zips it against the mint-derived `_KEYS[self.tag]` field-name row under `strict=True` (the derivation and the case tuples cannot drift) into the `dict[str, object]` the runtime `EventDict` carries with native ints/floats intact (the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializing them without a `str()` coerce), with `preview`/`color`/`media`/`cad`/`scene` flattening their `frozendict` band and `verdict` spreading the local `ConformanceVerdict.facts()` (already `dict[str, object]`) as the six special arms. The roster derivation is the E9 owner: `_CASES` reads the case fields off the union's own annotations, a load gate raises at import when the `ArtifactKind` Literal and the roster drift, and `_KEYS` derives each flat fact tail from its mint signature — ONE declaration site, no hand-synced parallel table.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union; `Map.of_seq`/`Map.empty` the derived `_KEYS` table, the `_METRIC` signal rows, and the `_CEILING` policy row — the dispatch/policy-table type per the shared-tier spine); `msgspec` (`Struct` + `structs.asdict` the re-homed `ConformanceVerdict`); the builtin `frozendict` (the five case evidence bands — msgspec-native and hashable where `Map` is not, the band-vs-table split); stdlib `inspect.signature` (the `_KEYS` mint-signature derivation); runtime (`identity.ContentKey` the key, `receipts.Receipt` the shape-polymorphic `of`, the structurally-satisfied `ReceiptContributor` port, `metrics.Metrics.record` the Gate-#1 domain/kind arm, `faults.RuntimeRail` the graduates return); compute (`graduation.handoff.GraduationReceipt`/`HandoffAxis` the Gate-#2 hub, imported downward). No producer module crosses in.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `case()` plus one `@classmethod` mint plus one `slot` or-pattern arm — the load gate and the `assert_never` tails breaking at import/type-check until every piece exists. A FLAT kind adds nothing else: its `_KEYS` row derives from the mint signature. A BANDED kind (heterogeneous per-producer facts) adds its `_BANDED` entry plus one `_facts` flatten arm instead — the `preview`/`color`/`media`/`cad`/`scene` form — so a new band fact is one key the producer fills with ZERO receipt edit. A new scalar on a flat kind is one slot on its `case()` tuple plus one mint parameter, the `strict=True` zip raising if they drift. A new measured signal is one `_METRIC` row here plus one `InstrumentSpec` row on the runtime metrics owner; a new governed residual bar is one `_CEILING` row. A producer that finishes an existing kind reuses that mint rather than declaring a parallel one.
- Boundary: the deleted forms are a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family where the named mints construct one union; the parallel `ColorReceipt`/`ColorReceiptWire` rail beside the union where the `Color` case IS the color plane's terminal evidence; a parallel AEC/delivery receipt rail where the one shared union carries every domain's evidence as a CASE; a per-kind evidence `Struct` re-wrapping the scalars the producer already passes positionally; a monolithic fixed-scalar `media` case carrying every media page's fields defaulted to zero where the `facts` band carries only each page's own evidence; a hand-synced `_KEYS` literal table beside the derivable mint signatures where the roster derivation owns both spellings; a stringly `ArtifactKind` maintained apart from the case roster where the load gate pins them; a conformance-owned `ConformanceVerdict` imported UPWARD into the spine where the re-homed shape makes the edge point down; a compute-side pre-mint of the artifacts handoff case where the joint motion lands the case in compute and the projection here; a local metrics fief — a page-owned histogram, counter, or logging table — beside the runtime `Metrics.record` domain arm; a render/production DURATION fact on any case where the runtime `Metrics.measured` aspect owns timing; a `dict[str, str]` egress pre-formatting `str(byte_count)` where the `dict[str, object]` `EventDict` keeps native scalars; a reflective `getattr(self, self.tag)` feeding `slot`/`_facts`; a `@staticmethod`-plus-forward-ref mint family; a `phase` parameter on `contribute`; an artifacts-side `rejected(fault)` forwarder; a second verdict owner; a producer-module import; a re-minted reuse-fabric admission; a second `@receipted` re-wrap of the graduates rail; and a hand-rolled `Signals.emit` where the `@receipted` aspect harvests this contributor.

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


# the owner is one kind-discriminated union over `(ContentKey, *scalars)` tuples; the case roster is the
# ONE derivation source — the [TABLES] block below reads the kind set and the flat fact names off it.


@tagged_union(frozen=True)
class ArtifactReceipt:
    # every case is `(ContentKey, *producer scalars)` producers call positionally; `preview`/`color`/`media`/`cad`/`scene` close on a `frozendict` band.
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
        # the general arm zips each flat tail against its mint-derived `_KEYS` row (`strict=True` flags
        # drift), keeping native scalars; the five banded kinds flatten their band, `verdict` spreads `facts()`.
        match self:
            case ArtifactReceipt(tag="preview", preview=(_key, width, height, scores)):
                return {"width": width, "height": height, **scores}
            case ArtifactReceipt(tag="color", color=(_key, space, intent, tac_peak, plates, facts)):
                return {"space": space, "intent": intent, "tac_peak": tac_peak, "plates": plates, **facts}
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return {**verdict.facts()}  # already `dict[str, object]`; native scalars pass through unstringified
            case ArtifactReceipt(tag="media", media=(_key, container, codec, duration, bytes_, frames, bit_rate, facts)):
                return {
                    "container": container,
                    "codec": codec,
                    "duration": duration,
                    "bytes": bytes_,
                    "frames": frames,
                    "bit_rate": bit_rate,
                    **facts,
                }  # flattens the per-page av/pysubs2 evidence band as `preview` flattens `scores`
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
                }  # DXF output format rides `"format"`, never `"artifact"` (the reserved discriminant `contribute` appends)
            case ArtifactReceipt(tag="scene", scene=(_key, target, bytes_, facts)):
                return {"target": target, "bytes": bytes_, **facts}  # flattens the pyvista/usd-core render-evidence band
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
        # phase is the constant `"emitted"` (KNOB_TEST: an `ArtifactReceipt` is by construction emitted),
        # so this is exactly the runtime `ReceiptContributor.contribute(self)` port; `"artifact"` is the
        # reserved discriminant appended last. The Gate-#1 seam fires here: every `_METRIC`-named numeric
        # fact records through the runtime domain/kind arm — never a local metric fief.
        facts = self._facts()
        measures = {name: float(v) for slot, name in _METRIC.items() if isinstance(v := facts.get(slot), int | float) and not isinstance(v, bool)}
        if measures:
            Metrics.record(measures, domain="artifact", kind=self.tag)
        return (Receipt.of("artifacts", ("emitted", self.slot.hex, {**facts, "artifact": self.tag})),)

    def graduates(self, /, *, ceiling: Mapping[str, float] | None = None) -> RuntimeRail[GraduationReceipt]:
        # the one outward figure hand-off: any receipt projects onto compute's artifacts-origin axis case,
        # keyed by the already-minted `ContentIdentity` key; the governed `_CEILING` row merges under the
        # caller's tighter per-key override. Admission, span, and `planned` egress stay compute's.
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

# Gate-#1 measured-signals rows: fact key -> the runtime `domain="artifact"` instrument name the
# `Metrics.record` arm resolves through `_DOMAIN_SLOT`; a new signal is a row here + a runtime InstrumentSpec row.
_METRIC: Final[Map[str, str]] = Map.of_seq([("bytes", "artifact.byte_volume"), ("ratio", "artifact.compression_ratio")])

# Gate-#2 governed residual bar: the default ceiling every outward figure clears; a caller's tighter row
# overrides per key. A governed bar lands as a row here naming a fact the barred kinds measure, never a per-call literal.
_CEILING: Final[Map[str, float]] = Map.empty()
```

## [03]-[SIGNALS]

- [REUSE_ELISION]: [RESOLVED]: the reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision — `execution/lanes#LANE` declares the `Admit.keyed: tuple[ContentKey, Work[T]]` admission with the content-keyed cache short-circuit and the threaded `DrainReceipt.cache`. The artifacts side is the settled consumer edge: every producer mints its key over the INPUT spec pre-run (`ContentIdentity.of` under `CANONICAL_POLICY`), threads the same key into the lane admission, and `_emit` threads it into the terminal receipt so `ArtifactReceipt.slot` reads it back for the hit/miss distinction — no new case, no new owner, no re-minted key.
- [METRIC_SIGNALS]: [RESOLVED]: the measured-signal leg is COMPOSED at the `contribute` fold — the `_METRIC` rows project the present numeric facts onto the runtime `Metrics.record(measures, domain="artifact", kind=...)` polymorphic arm, each measure name resolved through the runtime `_DOMAIN_SLOT` onto the landed `artifact.byte_volume` (`By`, slot `artifact_bytes`) and `artifact.compression_ratio` (`1`, slot `artifact_ratio`) `InstrumentSpec` HISTOGRAM rows, keyed by the carried kind, so per-kind size/ratio distributions route off one stream with zero artifacts-local metric state. Every other fact reaches the structured log line as native int/float beside the `"artifact"` discriminant (the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializes them unstringified). Render/production DURATION is NOT an `ArtifactReceipt` fact — the runtime `observability/metrics#METRIC` `Metrics.measured` aspect times the serve coroutine onto its own `companion.request.duration` histogram. The measure set widens as new `domain="artifact"` instrument rows land upstream, one `_METRIC` row per instrument here (page/plate/layer counts already ride receipt bands, ready to bind).
- [PLAN_FABRIC]: [RESOLVED]: the production-planning leg is the third receipt-fold CONSUMER — the `core/plan#PLAN` `ArtifactPipeline` reads each producer's resolved `ArtifactReceipt` (the `Work[ArtifactReceipt]` thunk's terminal) as the content-keyed evidence its sub-graph-elision plan distinguishes a hit from a miss on, folding every pre-minted key into the `(ContentKey, Work[ArtifactReceipt])` pairs the runtime lane admits as `keyed` units, so an identical producer at an identical key is elided once across the whole plan. The `planned`-stage observability is NOT a phase on an `ArtifactReceipt`: the planner mints its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts))` over the pipeline-topology + CPM facts, so the `admitted`/`planned`/`emitted` line family is shared by the planner's direct emit and the producers' `emitted` facts, never by threading a phase through `contribute`.
- [FIGURE_HANDOFF]: [RESOLVED]: the outward figure edge to a sibling package travels the compute `graduation/handoff#GRADUATION` rail on its artifacts-origin `artifact` case — landed in compute in the same motion this page landed `graduates` (one `HandoffAxis` case + one `_subject` arm, per that page's growth law). `graduates` projects any receipt: the axis subject is the `ArtifactKind` tag, the evidence key is the receipt's own `ContentIdentity`-minted `slot`, the measured ledger is the numeric `_facts` scalars, and the residual ceiling is the governed `_CEILING` policy row under the caller's tighter override. `model_asset` is a compute-own subject figures never ride; the projection re-mints no canonical concept, so the runtime evidence `Structural.drift` query stays clean; the returned rail is compute's own fenced/spanned/`@receipted` egress, never re-wrapped here.
