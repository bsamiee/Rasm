# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact-evidence family every production sub-domain mints a case onto, keyed by the runtime `ContentKey` and satisfying the runtime `receipts.ReceiptContributor` port structurally. `ArtifactReceipt` is one `@tagged_union(frozen=True)` whose every case is a flat-scalar `(ContentKey, <facts>)` payload — the exact shape the producers already call, never a positional fact-tuple decoded by index and never a per-kind evidence `Struct` re-wrapping the scalars the caller hands in. Documents, reporting, typography, charts, scenes, tables, imaging, compression, recovery, finishing, conformance, provenance, media, diagrams, and descriptive metadata each land one case through one named `@classmethod` mint returning `Self`, so the receipt stream is one fact family the `core/plan#PLAN` content-keyed planner, the runtime reuse-fabric elision, and the `observability/metrics#METRIC` `MeterProvider` read off one `contribute` fold, never a parallel rail per producer. Every read of the closed union — `slot` and `_facts` — is a total `match self` closed by `assert_never`, never a reflective `getattr(self, self.tag)` whose `object` residual escapes the match and makes the totality witness a lie.

The owner composes the runtime receipt spine, never re-states it: the runtime `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` mints the lifecycle `fact` case from this page's `("emitted", subject, facts)` triple, and the runtime `@receipted(redaction)` aspect harvests this contributor stream and emits it. A failed production is never an artifacts case: the producer's runtime `boundary`/`async_boundary` converts the raise into a `BoundaryFault` on the `Result.Error` rail and the spine's own `Receipt.of("artifacts", fault)` mints the `rejected` line, so this page never re-spells a forwarder around that factory. This page owns only the artifacts-side evidence vocabulary and its projection onto that spine.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over flat-scalar `case()` payloads, its named per-kind `@classmethod` mints returning `Self` and discriminating by the shape the producer calls, the `contribute` projecting the active payload through one `_KEYS`-derived total-`match` fold onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` triple with the `ArtifactKind` discriminant carried on the facts so the byte-only kinds stay distinguishable downstream, and the total-`match` `slot` projection the planner and the reuse-fabric key on — the one fold the runtime reuse-fabric elision, the `MeterProvider` stream, and the planner consume as the emitted-artifact evidence. The `admitted`/`planned` lifecycle facts are the `core/plan#PLAN` planner's own direct `Receipt.of` emit, not an `ArtifactReceipt` case, and a failed production is the runtime spine's own `Receipt.of("artifacts", fault)` `rejected` line off the `reliability/faults#FAULT` `BoundaryFault`, never an artifacts case.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` is one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a flat-scalar `tuple[ContentKey, ...]` `case()` whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, which returns the one-element `Iterable[Receipt]` the port declares by projecting the active payload onto the runtime `Receipt.of("artifacts", ("emitted", subject, facts))` contract — `"artifacts"` the owner, and the `(Phase, str, dict[str, object])` evidence triple (its phase the constant `"emitted"`, an `ArtifactReceipt` being by construction an emitted artifact's evidence) the runtime `of` matches onto its `fact` case. The `Verdict` case carries the one imported producer value object — the `exchange/conformance`-leaf `ConformanceVerdict` (the single acyclic value-object edge, never reciprocally imported); every other case carries native scalars, so the union imports no `c2pa-python`/`av`/`drawsvg`/`pikepdf` surface.
- Cases: each `case()` payload is `(ContentKey, <scalars>)` carrying the producer facts directly. `Pdf(key, bytes, pages)` the byte/page counts; `Office(key, bytes)`/`Report(key, bytes)`/`Document(key, bytes)` the byte-only finishing scalar three tags share — `Office` a workbook/slide container, `Report` a composed report, `Document` the generic document-rail blob the `typography/font#FONT`/`shape#SHAPE`/`layout#LAYOUT` arms mint for an axis catalog, positioned glyph run, or break/line-broken stream that is neither a PDF nor an office file. `Chart(key, engine, dialect, scale, theme, bytes)` (the engine the matched `ChartSpec.tag`); `Scene(key, target, bytes)`/`Table(key, format, bytes)` the target/format plus the produced byte count (the `bytes_` slot defaulting `0` so a producer that omits it is untouched); `Preview(key, width, height, scores)` where `scores: frozendict[str, float | str] = frozendict()` carries BOTH the `graphic/raster/measure#MEASURE` float perceptual band (`structural_similarity`/`peak_signal_noise_ratio`/`mean_squared_error`/`normalized_root_mse`/`normalized_mutual_information`/`hausdorff_distance`, plus the `contours`/`entropy`/`blobs`/`corners` and `shift`/`error` measurement facts) AND the `graphic/marks#MARK` string symbology/decode facts (the segno `designator`, the python-barcode `fullcode` check digit, the zxing `format`/`ec_level`, the decode `text`/`valid` round-trip) — an empty band for a bare save, the `RasterFact.score` row threaded through for a measured/encoded one, the same mint absorbing perceptual, raster, and machine-readable-mark evidence under the `float | str` value union; `Bundle(key, algo, level, dict_id, frame_size, entries, verified, ratio)` the `package/codec#CODEC` `BundleEvidence` projection (the `entries`/`verified` slots the container codecs fill and the single-blob codecs leave zero); `Introspection(key, nodes, text_len, images, hits)` the `document/lens#LENS` recovered-tree shape; `Egress(key, bytes, pages, encryption_r, outline_depth, overlays)` the `document/egress#FINISH` finishing facts, reused by `document/tagged#ACCESS` which maps its structural element count onto `outline_depth` and figure count onto `overlays` rather than declaring a parallel access case; `Verdict(key, verdict)` the leaf `ConformanceVerdict`; `Credential(key, manifest_id, signer, assertions, validation_state)` the `exchange/credential#PROVENANCE` C2PA store walk; `Media(key, container, codec, duration, bytes, frames, bit_rate)` the `media/video#MEDIA`/`media/audio#MEDIA` `MediaEvidence` six-scalar encode read plus the `bit_rate` slot the `av` `VideoStream.bit_rate` read computes (defaulting `0` so a producer spreading only the five non-rate scalars is untouched and a constant-bitrate audio encode has a slot for its rate); `Diagram(key, kind, nodes, edges, algorithm)` the `visualization/diagram/layout#LAYOUT` coordinate-assignment shape; and `Metadata(key, standard, fields, bytes)` the `exchange/metadata#METADATA` descriptive-field tally.
- Mints: each named `@classmethod` returning `Self` is the public construction surface a producer calls — `ArtifactReceipt.Pdf(key, bytes, pages)`, `ArtifactReceipt.Document(key, bytes)`, `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate)` — the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the `@classmethod`-plus-`Self` form binding the subtype once where a `@staticmethod`-plus-forward-ref re-spells the type sixteen times. The mints are thin: each keyword-constructs its `case()` and adds nothing, so the producer reads the natural positional call its `_emit` arm already writes while the union owns the one interior. `Preview` defaults `scores=frozendict()`, `Scene`/`Table` default `bytes_=0`, and `Media` defaults `bit_rate=0`, so a bare save and a legacy call stay valid while the richer slots wait for the producer's measured row, `len(data)`, or rate read.
- Entry: `contribute(self)` returns the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares — `(Receipt.of("artifacts", ("emitted", self.slot.hex, {**self._facts(), "artifact": self.tag})),)` — the one-tuple shape the sibling `compute/solvers/receipt#RECEIPT` `SolverReceipt.contribute` holds, never a bare `Receipt` against the `Iterable` port. The facts carry the `ArtifactKind` discriminant under the reserved `"artifact"` key (distinct from the `Diagram` case's own `"kind"` field), so the otherwise-identical byte-only `office`/`report`/`document` evidence and every other kind stay routable by the `MeterProvider` per-kind instrument and filterable by a structured-log consumer rather than reverse-inferred from the present key set. `contribute` takes NO `phase` parameter — an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact, so the phase is the constant `"emitted"` and a parameter is a knob the value already answers (KNOB_TEST); the `admitted`/`planned` lifecycle facts ride the `core/plan#PLAN` planner's OWN direct `Receipt.of("artifacts", ("planned", ...))`, and a failed production rides the runtime spine's `Receipt.of("artifacts", fault)` `rejected` line, so the signature is exactly the runtime `ReceiptContributor.contribute(self)` port with no lifecycle knob and no `rejected(fault)` forwarder.
- Derivations: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose 16-tag or-pattern captures `(key, *_)` once and closes on `assert_never`, so the planner keys, the reuse-fabric elides, and the `MeterProvider` correlates on the one projection — sound over the closed union where a reflective `getattr(self, self.tag)` whose `tuple[object, ...]` residual erases the `ContentKey` type and defeats the exhaustiveness witness is the deleted form. `_facts` is the second total `match self` closed by `assert_never`: its general 14-tag or-pattern binds the case's scalar tail `(_key, *tail)` and zips it against the `_KEYS[self.tag]` field-name row under `strict=True` (the table and the case tuples cannot drift) into the `dict[str, object]` the runtime `EventDict` carries with native ints/floats intact (the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializing them without a `str()` coerce), with `preview` flattening its `scores` band and `verdict` spreading the leaf `ConformanceVerdict.facts()` as the two structural arms. There is no separate `evidence` accessor: `_facts` binds the scalar tail inside its own arm, read once at the one projection site rather than re-discriminated by a second `getattr` property.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union); `frozendict` (the `_KEYS` field-name table and the `Preview` `scores` band); runtime (`content_identity.ContentKey` the key, `receipts.Receipt` the shape-polymorphic `of`, the structurally-satisfied `ReceiptContributor` port whose `Phase`-headed triple this owner fills with the constant `"emitted"`, and the `@receipted(Redaction)` aspect each producer threads onto its `_emit` to harvest this `contribute` stream and emit through `Signals.emit_async`). The `Verdict` case stores the `exchange/conformance#CONFORMANCE` `ConformanceVerdict`, the union's sole imported producer value object whose `facts()` the derivation spreads; every other case is a native scalar tuple, so no `c2pa-python`, `av`, `drawsvg`, `pikepdf`, `reliability.faults`, or `Redaction` surface crosses into this owner.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `@classmethod` mint plus one `_KEYS` row plus one or-pattern arm in each of `slot` and `_facts` — the `assert_never` tail breaking both matches at type-check until the arm exists, so the closed-union exhaustiveness is the growth gate rather than a silent reflective miss. A new scalar on an existing kind is one slot on its `case()` tuple plus one `_KEYS` field name, the `strict=True` zip raising if the two drift. A producer that finishes an existing kind reuses that mint rather than declaring a parallel one; the reuse-fabric hit/miss distinction, the `MeterProvider` signal stream, and the planner are consumers of the existing fold, never new cases.
- Boundary: the deleted forms are a per-type `DocumentReceipt`/`PdfReceipt`/`MediaReceipt` family where the named mints construct one union; a per-kind evidence `Struct` (`PdfFacts`/`ChartFacts`/…) re-wrapping the scalars the producer already passes positionally; a positional `tuple[ContentKey, str, int, float]` decoded by index; a 16-arm `_facts` `match` hand-re-spelling `{"key": …}` per kind; a `dict[str, str]` egress pre-formatting `str(byte_count)`/`f"{ratio:.6f}"` where the `dict[str, object]` `EventDict` keeps native scalars; a reflective `getattr(self, self.tag)` feeding `slot`/`_facts` whose `tuple[object, ...]` residual escapes the closed `match` and makes the `assert_never` totality witness a lie; a `@staticmethod`-plus-forward-ref mint family where `@classmethod`-plus-`Self` binds the subtype once; a `phase` parameter on `contribute` where the planning facts ride the `core/plan#PLAN` planner's own emit; an artifacts-side `rejected(fault)` arm forwarding verbatim to the spine's own `Receipt.of("artifacts", fault)` line; a second verdict owner; a producer-module import cycle; a re-minted reuse-fabric admission; and a hand-rolled `Signals.emit` where the `@receipted` aspect harvests this contributor.

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
    "pdf", "office", "report", "document", "chart", "scene", "table", "preview",
    "bundle", "introspection", "egress", "verdict", "credential", "media", "diagram", "metadata",
]

# --- [MODELS] ---------------------------------------------------------------------------

# the owner is one kind-discriminated union over flat `(ContentKey, *scalars)` tuples; `_KEYS`
# (declared after the class) names each tail so `_facts` derives the projection, never a per-kind `Struct`.


@tagged_union(frozen=True)
class ArtifactReceipt:
    # every case is `(ContentKey, *producer scalars)` — the flat shape producers call positionally.
    tag: ArtifactKind = tag()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    document: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    scene: tuple[ContentKey, str, int] = case()
    table: tuple[ContentKey, str, int] = case()
    preview: tuple[ContentKey, int, int, frozendict[str, float | str]] = case()
    bundle: tuple[ContentKey, str, int, int, int, int, int, float] = case()
    introspection: tuple[ContentKey, int, int, int, int] = case()
    egress: tuple[ContentKey, int, int, int, int, int] = case()
    verdict: tuple[ContentKey, ConformanceVerdict] = case()
    credential: tuple[ContentKey, str, str, int, str] = case()
    media: tuple[ContentKey, str, str, float, int, int, int] = case()
    diagram: tuple[ContentKey, str, int, int, str] = case()
    metadata: tuple[ContentKey, str, int, int] = case()

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
    def Scene(cls, key: ContentKey, target: str, bytes_: int = 0, /) -> Self:
        return cls(scene=(key, target, bytes_))

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
    def Media(cls, key: ContentKey, container: str, codec: str, duration: float, bytes_: int, frames: int, bit_rate: int = 0, /) -> Self:
        return cls(media=(key, container, codec, duration, bytes_, frames, bit_rate))

    @classmethod
    def Diagram(cls, key: ContentKey, kind: str, nodes: int, edges: int, algorithm: str, /) -> Self:
        return cls(diagram=(key, kind, nodes, edges, algorithm))

    @classmethod
    def Metadata(cls, key: ContentKey, standard: str, fields: int, bytes_: int, /) -> Self:
        return cls(metadata=(key, standard, fields, bytes_))

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
            ):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    def _facts(self) -> dict[str, object]:
        # the general arm zips each case's scalar tail against its `_KEYS` row (`strict=True` flags
        # drift), keeping native scalars; `preview` flattens `scores`, `verdict` spreads `facts()`.
        match self:
            case ArtifactReceipt(tag="preview", preview=(_key, width, height, scores)):
                return {"width": width, "height": height, **scores}
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return {**verdict.facts()}  # dict-display widens the `dict[str, str]` to the `dict[str, object]` EventDict
            case (
                ArtifactReceipt(tag="pdf", pdf=(_key, *tail)) | ArtifactReceipt(tag="office", office=(_key, *tail))
                | ArtifactReceipt(tag="report", report=(_key, *tail)) | ArtifactReceipt(tag="document", document=(_key, *tail))
                | ArtifactReceipt(tag="chart", chart=(_key, *tail)) | ArtifactReceipt(tag="scene", scene=(_key, *tail))
                | ArtifactReceipt(tag="table", table=(_key, *tail)) | ArtifactReceipt(tag="bundle", bundle=(_key, *tail))
                | ArtifactReceipt(tag="introspection", introspection=(_key, *tail)) | ArtifactReceipt(tag="egress", egress=(_key, *tail))
                | ArtifactReceipt(tag="credential", credential=(_key, *tail)) | ArtifactReceipt(tag="media", media=(_key, *tail))
                | ArtifactReceipt(tag="diagram", diagram=(_key, *tail)) | ArtifactReceipt(tag="metadata", metadata=(_key, *tail))
            ):
                return dict(zip(_KEYS[self.tag], tail, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self) -> Iterable[Receipt]:
        # phase is the constant `"emitted"` (KNOB_TEST: an `ArtifactReceipt` is by construction emitted),
        # so this is exactly the runtime `ReceiptContributor.contribute(self)` port; the reserved
        # `"artifact"` key (distinct from `diagram`'s own `"kind"`) keeps byte-only kinds routable.
        return (Receipt.of("artifacts", ("emitted", self.slot.hex, {**self._facts(), "artifact": self.tag})),)


# the primary scalar-tail -> field-name correspondence; `preview`/`verdict` fold specially in
# `_facts` and carry no row. Derived once here, the single edit site a new kind's facts reach.
_KEYS: frozendict[ArtifactKind, tuple[str, ...]] = frozendict({
    "pdf": ("bytes", "pages"),
    "office": ("bytes",),
    "report": ("bytes",),
    "document": ("bytes",),
    "chart": ("engine", "dialect", "scale", "theme", "bytes"),
    "scene": ("target", "bytes"),
    "table": ("format", "bytes"),
    "bundle": ("algo", "level", "dict_id", "frame_size", "entries", "verified", "ratio"),
    "introspection": ("nodes", "text_len", "images", "hits"),
    "egress": ("bytes", "pages", "encryption_r", "outline_depth", "overlays"),
    "credential": ("manifest_id", "signer", "assertions", "validation_state"),
    "media": ("container", "codec", "duration", "bytes", "frames", "bit_rate"),
    "diagram": ("kind", "nodes", "edges", "algorithm"),
    "metadata": ("standard", "fields", "bytes"),
})
```

## [03]-[SIGNALS]

- [REUSE_ELISION] [BLOCKED]: the reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision — every artifacts `_emit` already returns `ContentIdentity.of(...)`, so the producers thread the same pre-minted key into the lane admission the runtime owns and the `ArtifactReceipt.slot` projection reads it back for the hit/miss distinction. The artifacts side is verification-and-consume only: no new case, no new owner, no re-minted key. BLOCKED on the upstream runtime `execution/lanes` `(ContentKey, Work[T])` admission task; the `contribute` fold and the `slot` projection are the settled consumer edge.
- [METRIC_SIGNALS] [BLOCKED]: the measured-signal leg carries the artifact's byte-volume and compression-ratio facts onto the one `contribute` fold as NATIVE int/float the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializes unstringified — the `Bundle` `ratio`, the `Preview` band's float perceptual metrics, the `Scene`/`Table` byte count, the `Media` `duration` and `bit_rate` reach the structured log line as numbers, beside the `"artifact"` kind discriminant the fold now carries so a per-kind observable routes off one stream. Render/production DURATION is NOT an `ArtifactReceipt` fact — the runtime `observability/metrics#METRIC` `Metrics.measured` aspect times the serve coroutine onto its own `companion.request.duration` `Histogram`; a per-artifact byte-volume or compression-ratio observable is a NEW `INSTRUMENTS` `InstrumentSpec` row on the runtime metrics owner reading these `_facts` scalars keyed by the carried kind, not a capability `contribute` already delivers. The artifacts side is consume-only: the fold carries the native facts and the kind; no new artifacts surface. BLOCKED on the upstream runtime `observability/metrics` instrument-set task.
- [PLAN_FABRIC] [BLOCKED]: the production-planning leg is the third receipt-fold CONSUMER — the `core/plan#PLAN` `ArtifactPipeline` reads each producer's resolved `ArtifactReceipt` (the `Work[ArtifactReceipt]` thunk's terminal) as the content-keyed evidence its sub-graph-elision plan distinguishes a hit from a miss on, folding every `_emit` result's pre-minted key into the `(ContentKey, Work[T])` pairs the runtime lane admits as `keyed` units, so an identical producer at an identical key is elided once across the whole plan. The `planned`-stage observability is NOT a phase on an `ArtifactReceipt`: the planner mints its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", facts))` over the pipeline-topology facts (`cardinality`/`depth`/`width`/`critical_path`), so the `admitted`/`planned`/`emitted` line family is shared by the planner's direct emit and the producers' `emitted` facts, never by threading a phase through `contribute`. The artifacts side is consume-only: the planner reads the existing `contribute` carrier, the `slot` projection, and the existing pre-minted keys; no new receipt case and no parallel plan owner. BLOCKED on the `core/plan#PLAN` planner authoring the `(ContentKey, Work)` fold over the runtime lane.
