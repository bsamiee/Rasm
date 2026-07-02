# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact-evidence family every production sub-domain mints a case onto, keyed by the runtime `ContentKey` and satisfying the runtime `receipts.ReceiptContributor` port structurally. `ArtifactReceipt` is one `@tagged_union(frozen=True)` whose every case is a `(ContentKey, <facts>)` payload of native scalars — the byte-only kinds a bare pair, the evidence-rich kinds (`preview`/`media`/`cad`/`scene`) closing on one `frozendict` band that absorbs a heterogeneous per-producer fact set without a fixed-scalar bag — the exact shape the producers already call, never a positional fact-tuple decoded by index and never a per-kind evidence `Struct` re-wrapping the scalars the caller hands in. Documents, reporting, typography, charts, scenes, tables, imaging, compression, recovery, finishing, conformance, provenance, media, diagrams, descriptive metadata, the AEC drawing plane, AEC schedules, construction specifications, CAD exchange, and the ISO 19650 delivery register and transmittal issue each land one case through one named `@classmethod` mint returning `Self`, so the receipt stream is one fact family the `core/plan#PLAN` content-keyed planner, the runtime reuse-fabric elision, and the `observability/metrics#METRIC` `MeterProvider` read off one `contribute` fold, never a parallel rail per producer. Every read of the closed union — `slot` and `_facts` — is a total `match self` closed by `assert_never`, never a reflective `getattr(self, self.tag)` whose `object` residual escapes the match and makes the totality witness a lie.

The owner composes the runtime receipt spine, never re-states it: the runtime `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` mints the lifecycle `fact` case from this page's `("emitted", subject, facts)` triple, and the runtime `@receipted(redaction)` aspect harvests this contributor stream and emits it. A failed production is never an artifacts case: the producer's runtime `boundary`/`async_boundary` converts the raise into a `BoundaryFault` on the `Result.Error` rail and the spine's own `Receipt.of("artifacts", fault)` mints the `rejected` line, so this page never re-spells a forwarder around that factory. This page owns only the artifacts-side evidence vocabulary and its projection onto that spine.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over native-scalar `case()` payloads (the `preview`/`media`/`cad` evidence-rich kinds closing on one `frozendict` band), its named per-kind `@classmethod` mints returning `Self` and discriminating by the shape the producer calls, the `contribute` projecting the active payload through one `_KEYS`-derived total-`match` fold onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` triple with the `ArtifactKind` discriminant carried on the facts so the byte-only kinds stay distinguishable downstream, and the total-`match` `slot` projection the planner and the reuse-fabric key on — the one fold the runtime reuse-fabric elision, the `MeterProvider` stream, and the planner consume as the emitted-artifact evidence. The `[07]-[SEAM_UNIFICATION]` AEC-plane cases (`Drawing`/`Schedule`/`Spec` and the `delivery/` `Register`/`Transmittal` pair) contribute AS CASES on this one family, never a parallel per-domain receipt rail. The `admitted`/`planned` lifecycle facts are the `core/plan#PLAN` planner's own direct `Receipt.of` emit, not an `ArtifactReceipt` case, and a failed production is the runtime spine's own `Receipt.of("artifacts", fault)` `rejected` line off the `reliability/faults#FAULT` `BoundaryFault`, never an artifacts case.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a `tuple[ContentKey, ...]` `case()` of native scalars whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars — the evidence-rich `preview`/`media`/`cad` kinds closing on one `frozendict` band as their last slot so a heterogeneous per-producer fact set rides one case rather than a per-page shape. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, which returns the one-element `Iterable[Receipt]` the port declares by projecting the active payload onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` contract — `"artifacts"` the owner, and the `(Phase, str, dict[str, object])` evidence triple (its phase the constant `"emitted"`, an `ArtifactReceipt` being by construction an emitted artifact's evidence) the runtime `of` matches onto its `fact` case. The `Verdict` case carries the one imported producer value object — the `exchange/conformance`-leaf `ConformanceVerdict` (the single acyclic value-object edge, never reciprocally imported); every other case carries native scalars or a native `frozendict` band, so the union imports no `c2pa-python`/`av`/`pysubs2`/`drawsvg`/`pikepdf` surface.
- Cases: each `case()` payload is `(ContentKey, <scalars>)` carrying the producer facts directly. `Pdf(key, bytes, pages)` the byte/page counts; `Office(key, bytes)`/`Report(key, bytes)`/`Document(key, bytes)` the byte-only finishing scalar three tags share — `Office` a workbook/slide container, `Report` a composed report, `Document` the generic document-rail blob the `typography/font#FONT`/`shape#SHAPE`/`layout#LAYOUT` arms mint for an axis catalog, positioned glyph run, or break/line-broken stream that is neither a PDF nor an office file. `Chart(key, engine, dialect, scale, theme, bytes)` (the engine the matched `ChartSpec.tag`); `Scene(key, target, bytes, facts)` the target, the produced byte count, and one `frozendict` render-evidence band (the `scene/render#SCENE` pyvista point/cell/window facts and the `scene/stage#STAGE` usd-core prim/layer/up-axis/meters-per-unit facts, flattened by the `scene` `_facts` arm exactly as `preview` flattens `scores`), and `Table(key, format, bytes)` the format plus the produced byte count (the `bytes_` slot defaulting `0` and `Scene`'s `facts` defaulting empty so a bare save is untouched); `Preview(key, width, height, scores)` where `scores: frozendict[str, float | str] = frozendict()` carries BOTH the `graphic/raster/measure#MEASURE` float perceptual band (`structural_similarity`/`peak_signal_noise_ratio`/`mean_squared_error`/`normalized_root_mse`/`normalized_mutual_information`/`hausdorff_distance`, plus the `contours`/`entropy`/`blobs`/`corners` and `shift`/`error` measurement facts) AND the `graphic/marks#MARK` string symbology/decode facts (the segno `designator`, the python-barcode `fullcode` check digit, the zxing `format`/`ec_level`, the decode `text`/`valid` round-trip) — an empty band for a bare save, the `RasterFact.score` row threaded through for a measured/encoded one, the same mint absorbing perceptual, raster, and machine-readable-mark evidence under the `float | str` value union; `Bundle(key, algo, level, dict_id, frame_size, entries, verified, ratio)` the `package/codec#CODEC` `BundleEvidence` projection (the `entries`/`verified` slots the container codecs fill and the single-blob codecs leave zero); `Introspection(key, nodes, text_len, images, hits)` the `document/lens#LENS` recovered-tree shape; `Egress(key, bytes, pages, encryption_r, outline_depth, overlays)` the `document/egress#FINISH` finishing facts, reused by `document/tagged#ACCESS` which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Verdict(key, verdict)` the leaf `ConformanceVerdict`; `Credential(key, manifest_id, signer, assertions, validation_state)` the `exchange/credential#PROVENANCE` C2PA store walk; `Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` the `media/container#CONTAINER`/`media/audio#MEDIA`/`media/*` encode read — the six shared `MediaEvidence` scalars, the `bit_rate` slot the `av` `VideoStream.bit_rate` read computes (defaulting `0`), and the `facts: frozendict[str, float | str] = frozendict()` per-page evidence band (empty for a bare video encode) the seven restructured media producers spread their own `av`/`pysubs2` evidence onto: the `container` HDR-color tag and HLS/DASH segment count, the `filtergraph` filter-node count, the `audio` EBU R128 integrated-LUFS/true-peak-dBTP/loudness-range, the `subtitle` event/style counts, the `analysis` scene-cut/silence spans plus the two-pass `av.filter.loudnorm.stats` integrated-LUFS/true-peak/LRA band, the `timeline` clip/segment counts and lossless-vs-reencode strategy, and the `synthesis` fundamental-Hz/waveform/duration params — flattened by the `media` `_facts` arm exactly as `preview` flattens `scores`, so the ONE shared `Media` case absorbs every media page's heterogeneous evidence (the `[07]-[SEAM_UNIFICATION]` "single extended media case", never seven sibling receipt shapes) without a permissive fixed-scalar bag whose subtitle/loudness/scene fields would default to zero on the pages that never produce them; `Diagram(key, kind, nodes, edges, algorithm)` the `visualization/diagram/layout#LAYOUT` coordinate-assignment shape; `Metadata(key, carrier, fields, bytes)` the `exchange/metadata#METADATA` descriptive-metadata evidence (the `MetaCarrier` RASTER/PDF/MEDIA discriminant the producer fills slot-two with, the `MetaFacts.populated` descriptive-field tally, and the payload byte length); `Drawing(key, kind, entities, style, width, height, bytes)` the AEC drawing-plane evidence the `drawing/dimension#DIMENSION`/`annotate`/`symbol`/`detail` producers contribute (the drawing `kind`, the entity count, the drawing-plane `style` descriptor — the `dimension` ISO 129-1 dimension-style name, the `annotate` ISO 128-2 leader convention, or the `symbol`/`detail` render engine, one neutral slot every drawing producer fills with its own style/convention/engine token — the rendered extents, and the byte count), one shared case across the drawing plane rather than a per-producer receipt rail; and `Schedule(key, kind, rows, columns, format, bytes)` the AEC-scheduling evidence the `drawing/schedule#SCHEDULE` producer contributes (the schedule/legend `kind`, the scheduled-item and column counts, the `TableFormat`, and the rendered byte count) — a distinct AEC-plane case beside `Table`, since a schedule carries its NCS/AIA kind and item cardinality where the generic publication `Table` case does not; `Spec(key, section, division, parts, articles, bytes)` the `specification/section#SECTION` construction-specification evidence (the MasterFormat section number, the `ClassCode.division()` head the `classify#CLASSIFY` crosswalk keys on, the count of present `SectionPart`s, the total article count, and the encoded `DocumentNode`-tree byte count), a distinct AEC-plane case since a CSI SectionFormat section carries its MasterFormat division and 3-part/article cardinality no publication case holds; `Cad(key, dxfversion, units, artifact, bytes, layers, blocks, errors, fixes, counts)` the `export/dxf#DXF` CAD-exchange evidence (the DXF version, the units name, the `artifact` format the bytes ARE, the output byte count, the `doc.layers`/`doc.blocks` roster counts, the `Auditor` error+fix counts a salvaged `Recover` carries non-zero, and the `counts` per-dxftype census the `_facts` `cad` arm flattens exactly as `preview` flattens its `scores` band), a distinct case since a DXF document carries its version/units/salvage-auditor evidence no publication case holds; `Register(key, kind, sheets, suitability, revision, classification, validation, bytes)` the `delivery/register#REGISTER` ISO 19650 delivery-register evidence (the `RegisterOp` kind — `index`/`container`/`audit`/`render` — the information-container count, the register-aggregate `SuitabilityCode`, the `RevisionCode`, the `ClassificationSystem`, the `Container`-XML schema-conformance state — the `isoschematron.Schematron` verdict `"valid"`/`"invalid:{n}"` the register folds off the built ISO 19650 tree before the C14N byte egress, `"unchecked"` for the non-XML `index`/`audit`/`render` ops that mint no validatable container — and the produced byte count of the sheet-index/COBie-XML/audit/spreadsheet the register lowers to); and `Transmittal(key, transmittal_id, sheets, suitability, container, validation_state)` the `delivery/transmittal#TRANSMITTAL` ISO 19650 issue-for-construction evidence (the transmittal number, the issued-sheet count, the purpose-of-issue suitability, the sealed-container content-key hex, and the folded PAdES/C2PA signed verdict `"valid"`/`"invalid"`/`"unsigned"`) — the delivery-plane pair the `[07]-[SEAM_UNIFICATION]` target admits as CASES beside `Spec`/`Drawing`/`Schedule`, never a parallel delivery-receipt rail.
- Mints: each named `@classmethod` returning `Self` is the public construction surface a producer calls — `ArtifactReceipt.Pdf(key, bytes, pages)`, `ArtifactReceipt.Document(key, bytes)`, `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate, facts)` — the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the `@classmethod`-plus-`Self` form binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the return type on every mint. The mints are thin: each keyword-constructs its `case()` and adds nothing, so the producer reads the natural positional call its `_emit` arm already writes while the union owns the one interior. `Preview` defaults `scores=frozendict()`, `Scene` defaults `bytes_=0` and `facts=frozendict()`, `Table` defaults `bytes_=0`, and `Media` defaults `bit_rate=0` and `facts=frozendict()`, so a bare save and a band-free video encode stay valid while the richer slots wait for the producer's measured row, `len(data)`, rate read, or per-page evidence band; the new AEC mints `Spec`/`Register`/`Transmittal` carry only native scalars, so the union imports no specification, register, or transmittal value object (the `Verdict` case's `ConformanceVerdict` stays the sole imported producer object).
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares — `(Receipt.of("artifacts", ("emitted", self.slot.hex, {**self._facts(), "artifact": self.tag})),)` — the one-tuple shape the sibling `compute/solvers/receipt#RECEIPT` `SolverReceipt.contribute` holds, never a bare `Receipt` against the `Iterable` port. The facts carry the `ArtifactKind` discriminant under the reserved `"artifact"` key — reserved because `contribute` overwrites it last, so no `_facts` arm may emit it: the `"kind"` inner fact the `diagram`/`schedule`/`register` cases carry and the `cad` DXF output format (which rides `"format"`, never `"artifact"`) each stay distinct from the appended discriminant rather than being clobbered by it — so the otherwise-identical byte-only `office`/`report`/`document` evidence and every other kind stay routable by the `MeterProvider` per-kind instrument and filterable by a structured-log consumer rather than reverse-inferred from the present key set. `contribute` takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers (KNOB_TEST); the `admitted`/`planned` lifecycle facts ride the `core/plan#PLAN` planner's OWN direct `Receipt.of("artifacts", ("planned", ...))`, and a failed production rides the runtime spine's `Receipt.of("artifacts", fault)` `rejected` line, so the signature is exactly the runtime `ReceiptContributor.contribute(self)` port with no lifecycle knob and no `rejected(fault)` forwarder.
- Derivations: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose 22-tag or-pattern captures `(key, *_)` once and closes on `assert_never`, so the planner keys, the reuse-fabric elides, and the `MeterProvider` correlates on the one projection — sound over the closed union where a reflective `getattr(self, self.tag)` whose `tuple[object, ...]` residual erases the `ContentKey` type and defeats the exhaustiveness witness is the deleted form. `_facts` is the second total `match self` closed by `assert_never`: its general 17-tag or-pattern binds the case's scalar tail `(_key, *tail)` and zips it against the `_KEYS[self.tag]` field-name row under `strict=True` (the table and the case tuples cannot drift) into the `dict[str, object]` the runtime `EventDict` carries with native ints/floats intact (the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializing them without a `str()` coerce), with `preview` flattening its `scores` band, `media` flattening its per-page `av`/`pysubs2` evidence band, `cad` flattening its per-dxftype `counts` census, `scene` flattening its pyvista/usd-core render-evidence band, and `verdict` spreading the leaf `ConformanceVerdict.facts()` (already `dict[str, object]`) as the five special arms. There is no separate `evidence` accessor: `_facts` binds the scalar tail inside its own arm, read once at the one projection site rather than re-discriminated by a second `getattr` property.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union); `frozendict` (the `_KEYS` field-name table plus the three native evidence bands — the `Preview` `scores`, the `Media` `av`/`pysubs2` per-page facts, and the `Cad` per-dxftype census); runtime (`content_identity.ContentKey` the key, `receipts.Receipt` the shape-polymorphic `of`, the structurally-satisfied `ReceiptContributor` port whose `Phase`-headed triple this owner fills with the constant `"emitted"`, and the `@receipted(Redaction)` aspect each producer threads onto its `_emit` to harvest this `contribute` stream and emit through `Signals.emit_async`). The `Verdict` case stores the `exchange/conformance#CONFORMANCE` `ConformanceVerdict`, the union's sole imported producer value object whose `facts()` the derivation spreads; every other case is a native scalar tuple (the `Media` band a native `frozendict` the producer fills, never an `av` handle), so no `c2pa-python`, `av`, `pysubs2`, `drawsvg`, `pikepdf`, `reliability.faults`, or `Redaction` surface crosses into this owner.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `@classmethod` mint plus one `_KEYS` row plus one or-pattern arm in each of `slot` and `_facts` — the `assert_never` tail breaking both matches at type-check until the arm exists, so the closed-union exhaustiveness is the growth gate rather than a silent reflective miss (the `Spec`/`Register`/`Transmittal` AEC cases are exactly this growth, each a token, mint, `_KEYS` row, and pair of arms). A rich-evidence kind whose facts are heterogeneous per producer takes a `frozendict` band field plus a special `_facts` flatten arm instead of a flat `_KEYS` row and general-arm zip (the `media`/`preview`/`cad` form), so a new media evidence fact is one band key the producer fills with ZERO receipt edit — the anticipatory collapse that lets the seven `media/*` pages and every future one contribute onto the one `Media` case. A new scalar on a flat existing kind is one slot on its `case()` tuple plus one `_KEYS` field name, the `strict=True` zip raising if the two drift. A producer that finishes an existing kind reuses that mint rather than declaring a parallel one; the reuse-fabric hit/miss distinction, the `MeterProvider` signal stream, and the planner are consumers of the existing fold, never new cases.
- Boundary: the deleted forms are a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family where the named mints construct one union; a parallel `Spec`/`Register`/`Transmittal` AEC-plane or delivery-plane receipt rail where the one shared union carries every domain's evidence as a CASE (the `[07]-[SEAM_UNIFICATION]` target); a per-kind evidence `Struct` (`PdfFacts`/`ChartFacts`/…) re-wrapping the scalars the producer already passes positionally; a monolithic fixed-scalar `media` case carrying every media page's loudness/true-peak/scene-cut/subtitle/segment fields defaulted to zero for the pages that never produce them where the `facts` band carries only each page's own evidence (the DERIVED-NOT-PARALLEL permissive-bag defect); seven sibling media receipt shapes where the one extended `Media` case absorbs the restructured `media/*` plane; a positional `tuple[ContentKey, str, int, float]` decoded by index; a per-case `_facts` `match` hand-re-spelling `{"key": …}` where the `_KEYS`-derived zip and the four `preview`/`media`/`cad`/`verdict` band arms carry the whole projection; a `dict[str, str]` egress pre-formatting `str(byte_count)`/`f"{ratio:.6f}"` where the `dict[str, object]` `EventDict` keeps native scalars; a reflective `getattr(self, self.tag)` feeding `slot`/`_facts` whose `tuple[object, ...]` residual escapes the closed `match` and makes the `assert_never` totality witness a lie; a `@staticmethod`-plus-forward-ref mint family where `@classmethod`-plus-`Self` binds the subtype once; a `phase` parameter on `contribute` where the planning facts ride the `core/plan#PLAN` planner's own emit; an artifacts-side `rejected(fault)` arm forwarding verbatim to the spine's own `Receipt.of("artifacts", fault)` line; a second verdict owner; a producer-module import cycle; a re-minted reuse-fabric admission; and a hand-rolled `Signals.emit` where the `@receipted` aspect harvests this contributor.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from typing import Literal, Self, assert_never

from builtins import frozendict
from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.receipts import Receipt

from artifacts.exchange.conformance import ConformanceVerdict

# --- [TYPES] ----------------------------------------------------------------------------

type ArtifactKind = Literal[
    "pdf", "office", "report", "document", "chart", "scene", "table", "preview", "bundle", "introspection",
    "egress", "verdict", "credential", "media", "diagram", "metadata", "drawing", "schedule", "spec", "cad",
    "register", "transmittal",
]

# --- [MODELS] ---------------------------------------------------------------------------

# the owner is one kind-discriminated union over `(ContentKey, *scalars)` tuples (the evidence-rich
# `preview`/`media`/`cad` kinds closing on a `frozendict` band tail); `_KEYS` (declared after the class)
# names each flat tail so `_facts` derives the projection, never a per-kind `Struct`.


@tagged_union(frozen=True)
class ArtifactReceipt:
    # every case is `(ContentKey, *producer scalars)` producers call positionally; `preview`/`media`/`cad`/`scene` close on a `frozendict` evidence band.
    tag: ArtifactKind = tag()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    document: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    scene: tuple[ContentKey, str, int, frozendict[str, float | str]] = case()
    table: tuple[ContentKey, str, int] = case()
    preview: tuple[ContentKey, int, int, frozendict[str, float | str]] = case()
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
    def Table(cls, key: ContentKey, fmt: str, bytes_: int = 0, /) -> Self:
        return cls(table=(key, fmt, bytes_))

    @classmethod
    def Preview(cls, key: ContentKey, width: int, height: int, scores: frozendict[str, float | str] = frozendict(), /) -> Self:
        return cls(preview=(key, width, height, scores))

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
    def Media(cls, key: ContentKey, container: str, codec: str, duration: float, bytes_: int, frames: int, bit_rate: int = 0, facts: frozendict[str, float | str] = frozendict(), /) -> Self:
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
    def Schedule(cls, key: ContentKey, kind: str, rows: int, columns: int, fmt: str, bytes_: int, /) -> Self:
        return cls(schedule=(key, kind, rows, columns, fmt, bytes_))

    @classmethod
    def Spec(cls, key: ContentKey, section: str, division: int, parts: int, articles: int, bytes_: int, /) -> Self:
        return cls(spec=(key, section, division, parts, articles, bytes_))

    @classmethod
    def Cad(cls, key: ContentKey, dxfversion: str, units: str, artifact: str, bytes_: int, layers: int, blocks: int, errors: int, fixes: int, counts: frozendict[str, int], /) -> Self:
        return cls(cad=(key, dxfversion, units, artifact, bytes_, layers, blocks, errors, fixes, counts))

    @classmethod
    def Register(cls, key: ContentKey, kind: str, sheets: int, suitability: str, revision: str, classification: str, validation: str, bytes_: int, /) -> Self:
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
                ArtifactReceipt(tag="pdf", pdf=(key, *_)) | ArtifactReceipt(tag="office", office=(key, *_))
                | ArtifactReceipt(tag="report", report=(key, *_)) | ArtifactReceipt(tag="document", document=(key, *_))
                | ArtifactReceipt(tag="chart", chart=(key, *_)) | ArtifactReceipt(tag="scene", scene=(key, *_))
                | ArtifactReceipt(tag="table", table=(key, *_)) | ArtifactReceipt(tag="preview", preview=(key, *_))
                | ArtifactReceipt(tag="bundle", bundle=(key, *_)) | ArtifactReceipt(tag="introspection", introspection=(key, *_))
                | ArtifactReceipt(tag="egress", egress=(key, *_)) | ArtifactReceipt(tag="verdict", verdict=(key, *_))
                | ArtifactReceipt(tag="credential", credential=(key, *_)) | ArtifactReceipt(tag="media", media=(key, *_))
                | ArtifactReceipt(tag="diagram", diagram=(key, *_)) | ArtifactReceipt(tag="metadata", metadata=(key, *_))
                | ArtifactReceipt(tag="drawing", drawing=(key, *_)) | ArtifactReceipt(tag="schedule", schedule=(key, *_))
                | ArtifactReceipt(tag="spec", spec=(key, *_)) | ArtifactReceipt(tag="cad", cad=(key, *_))
                | ArtifactReceipt(tag="register", register=(key, *_)) | ArtifactReceipt(tag="transmittal", transmittal=(key, *_))
            ):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    def _facts(self) -> dict[str, object]:
        # the general arm zips each case's scalar tail against its `_KEYS` row (`strict=True` flags
        # drift), keeping native scalars; `preview`/`media` flatten their band, `cad` its census, `verdict` spreads `facts()`.
        match self:
            case ArtifactReceipt(tag="preview", preview=(_key, width, height, scores)):
                return {"width": width, "height": height, **scores}
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return {**verdict.facts()}  # `ConformanceVerdict.facts()` is already `dict[str, object]`; the native scalars pass through unstringified
            case ArtifactReceipt(tag="media", media=(_key, container, codec, duration, bytes_, frames, bit_rate, facts)):
                return {"container": container, "codec": codec, "duration": duration, "bytes": bytes_,
                        "frames": frames, "bit_rate": bit_rate, **facts}  # flattens the per-page av/pysubs2 evidence band as `preview` flattens `scores`
            case ArtifactReceipt(tag="cad", cad=(_key, dxfversion, units, artifact, bytes_, layers, blocks, errors, fixes, counts)):
                return {"dxfversion": dxfversion, "units": units, "format": artifact, "bytes": bytes_,
                        "layers": layers, "blocks": blocks, "errors": errors, "fixes": fixes, **counts}  # DXF output format rides `"format"`, never `"artifact"` (the reserved discriminant `contribute` appends); flattens the census as `preview` flattens `scores`
            case ArtifactReceipt(tag="scene", scene=(_key, target, bytes_, facts)):
                return {"target": target, "bytes": bytes_, **facts}  # flattens the pyvista point/cell/window + usd-core prim/layer/up-axis/meters-per-unit render evidence band as `preview` flattens `scores`
            case (
                ArtifactReceipt(tag="pdf", pdf=(_key, *tail)) | ArtifactReceipt(tag="office", office=(_key, *tail))
                | ArtifactReceipt(tag="report", report=(_key, *tail)) | ArtifactReceipt(tag="document", document=(_key, *tail))
                | ArtifactReceipt(tag="chart", chart=(_key, *tail))
                | ArtifactReceipt(tag="table", table=(_key, *tail)) | ArtifactReceipt(tag="bundle", bundle=(_key, *tail))
                | ArtifactReceipt(tag="introspection", introspection=(_key, *tail)) | ArtifactReceipt(tag="egress", egress=(_key, *tail))
                | ArtifactReceipt(tag="credential", credential=(_key, *tail)) | ArtifactReceipt(tag="diagram", diagram=(_key, *tail))
                | ArtifactReceipt(tag="metadata", metadata=(_key, *tail)) | ArtifactReceipt(tag="drawing", drawing=(_key, *tail))
                | ArtifactReceipt(tag="schedule", schedule=(_key, *tail)) | ArtifactReceipt(tag="spec", spec=(_key, *tail))
                | ArtifactReceipt(tag="register", register=(_key, *tail)) | ArtifactReceipt(tag="transmittal", transmittal=(_key, *tail))
            ):
                return dict(zip(_KEYS[self.tag], tail, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        # phase is the constant `"emitted"` (KNOB_TEST: an `ArtifactReceipt` is by construction emitted),
        # so this is exactly the runtime `ReceiptContributor.contribute(self)` port; `"artifact"` is the
        # reserved discriminant appended last (never emitted by a `_facts` arm — `cad` uses `"format"`,
        # the `"kind"` fact rides its own key), keeping every kind routable by the `MeterProvider`.
        return (Receipt.of("artifacts", ("emitted", self.slot.hex, {**self._facts(), "artifact": self.tag})),)


# the primary scalar-tail -> field-name correspondence; `preview`/`media`/`cad`/`scene`/`verdict` fold specially in
# `_facts` and carry no row. Derived once here, the single edit site a new kind's facts reach.
_KEYS: frozendict[ArtifactKind, tuple[str, ...]] = frozendict({
    "pdf": ("bytes", "pages"),
    "office": ("bytes",),
    "report": ("bytes",),
    "document": ("bytes",),
    "chart": ("engine", "dialect", "scale", "theme", "bytes"),
    "table": ("format", "bytes"),
    "bundle": ("algo", "level", "dict_id", "frame_size", "entries", "verified", "ratio"),
    "introspection": ("nodes", "text_len", "images", "hits"),
    "egress": ("bytes", "pages", "encryption_r", "outline_depth", "overlays"),
    "credential": ("manifest_id", "signer", "assertions", "validation_state"),
    "diagram": ("kind", "nodes", "edges", "algorithm"),
    "metadata": ("carrier", "fields", "bytes"),
    "drawing": ("kind", "entities", "style", "width", "height", "bytes"),
    "schedule": ("kind", "rows", "columns", "format", "bytes"),
    "spec": ("section", "division", "parts", "articles", "bytes"),
    "register": ("kind", "sheets", "suitability", "revision", "classification", "validation", "bytes"),
    "transmittal": ("transmittal_id", "sheets", "suitability", "container", "validation_state"),
})
```

## [03]-[SIGNALS]

- [REUSE_ELISION] [RESOLVED]: the reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision, and the upstream is now authored — `execution/lanes#LANE` declares the `Admit.keyed: tuple[ContentKey, Work[T]]` admission with the content-keyed cache short-circuit and the threaded `DrainReceipt.cache`, so the elision this fold feeds exists upstream. The artifacts side is the settled consumer edge: every `_emit` returns `ContentIdentity.of(...)`, the producers thread the same pre-minted key into the lane admission, and the `ArtifactReceipt.slot` projection reads it back for the hit/miss distinction — no new case, no new owner, no re-minted key. No open cross-file item remains on this seam.
- [METRIC_SIGNALS] [RESOLVED]: the measured-signal leg carries the artifact's byte-volume and compression-ratio facts onto the one `contribute` fold as NATIVE int/float the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializes unstringified — the `Bundle` `ratio`, the `Preview` band's float perceptual metrics, the `Scene`/`Table` byte count, the `Media` `duration`/`bit_rate` and its `facts` band's native metrics (the EBU R128 integrated-LUFS, the scene-cut/silence spans, the filter-node/HLS-segment counts), and the `Spec`/`Register` AEC cardinality counts reach the structured log line as numbers, beside the `"artifact"` kind discriminant the fold now carries so a per-kind observable routes off one stream. Render/production DURATION is NOT an `ArtifactReceipt` fact — the runtime `observability/metrics#METRIC` `Metrics.measured` aspect times the serve coroutine onto its own `companion.request.duration` `Histogram`; a per-artifact byte-volume or compression-ratio observable is a NEW `INSTRUMENTS` `InstrumentSpec` row on the runtime metrics owner reading these `_facts` scalars keyed by the carried kind, not a capability `contribute` already delivers. The artifacts side is consume-only and complete: the fold carries the native facts and the kind; no new artifacts surface. RESOLVED upstream — the runtime `observability/metrics#METRIC` `Metrics` owner now carries the `artifact.byte_volume` (`By`) and `artifact.compression_ratio` (`1`) `InstrumentSpec` HISTOGRAM rows on its `INSTRUMENTS` table plus the `record_artifact(kind, byte_volume, ratio)` recorder the artifacts emit-harvest seam composes (exactly as `retry_hook` is resilience-composed), reading these `_facts` scalars keyed by the carried `artifact` kind, so a per-kind size/ratio distribution routes off one stream beside the `companion.request.duration`/`retry.attempts`/lane-drain/lane-saturation/process rows.
- [PLAN_FABRIC] [RESOLVED]: the production-planning leg is the third receipt-fold CONSUMER, and `core/plan#PLAN` now authors it — the `ArtifactPipeline` reads each producer's resolved `ArtifactReceipt` (the `Work[ArtifactReceipt]` thunk's terminal) as the content-keyed evidence its sub-graph-elision plan distinguishes a hit from a miss on, folding every `_emit` result's pre-minted key into the `(ContentKey, Work[ArtifactReceipt])` pairs the runtime lane admits as `keyed` units, so an identical producer at an identical key is elided once across the whole plan. The `planned`-stage observability is NOT a phase on an `ArtifactReceipt`: the planner mints its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", plan.facts))` over the pipeline-topology + CPM facts (`cardinality`/`depth`/`width`/`contention`/`critical_path`/`makespan`/`severed`), threaded through its `_emitted` tap under the runtime-owned keep-all `OPEN` `Redaction`, so the `admitted`/`planned`/`emitted` line family is shared by the planner's direct emit and the producers' `emitted` facts, never by threading a phase through `contribute`. The artifacts side is the settled consumer edge: the planner reads the existing `contribute` carrier, the `slot` projection, and the existing pre-minted keys; no new receipt case and no parallel plan owner. No open cross-file item remains on this seam.
