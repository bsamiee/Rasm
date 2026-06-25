# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact-evidence family every production sub-domain mints a case onto, keyed by the runtime `ContentKey` and satisfying the runtime `receipts.ReceiptContributor` port structurally. `ArtifactReceipt` is one `@tagged_union(frozen=True)` whose every case is a flat-scalar `(ContentKey, <facts>)` payload — the exact shape the producers already call, never a positional fact-tuple decoded by index and never a per-kind evidence `Struct` re-wrapping the scalars the caller hands in. Documents, reporting, charts, scenes, tables, imaging, compression, recovery, finishing, conformance, provenance, media, diagrams, and descriptive metadata each land one case through one named `@classmethod` mint returning `Self`, so the receipt stream is one fact family the `core/plan#PLAN` content-keyed planner, the runtime reuse-fabric elision, and the `observability/metrics#METRIC` `MeterProvider` read off one `contribute` fold, never a parallel rail per producer. Every read of the closed union — `slot` and `_facts` — is a total `match self` closed by `assert_never`, never a reflective `getattr(self, self.tag)` whose `object` residual escapes the match and makes the totality witness a lie.

The owner composes the runtime receipt spine, never re-states it: the runtime `observability/receipts#RECEIPT` `Receipt.of(owner, evidence)` mints the lifecycle `fact` case from this page's `(phase, subject, facts)` triple and its own `rejected` case directly from a `reliability/faults#FAULT` `BoundaryFault`, and the runtime `@receipted(redaction)` aspect harvests this contributor stream and emits it. A failed production is never a second artifacts case: the producer's runtime `boundary`/`async_boundary` converts the raise into a `BoundaryFault` on the `Result.Error` rail and the spine's `Receipt.of("artifacts", fault)` mints the `rejected` line, so this page never re-spells a forwarder around that factory. This page owns only the artifacts-side evidence vocabulary and its projection onto that spine.

## [01]-[INDEX]

- [01]-[RECEIPT]: the kind-discriminated artifact-evidence union over flat-scalar `case()` payloads, its named per-kind `@classmethod` mints returning `Self` and discriminating by the shape the producer calls, the `contribute` projecting the active payload through one `_KEYS`-derived total-`match` fold onto the runtime `Receipt.of("artifacts", (phase, subject, facts))` triple, and the total-`match` `slot` projection the planner and the reuse-fabric key on — the one fold the runtime reuse-fabric elision, the `MeterProvider` stream, and the planner consume as the emitted-artifact evidence; the `admitted`/`planned` lifecycle facts are the `core/plan#PLAN` planner's own direct `Receipt.of` emit, not an `ArtifactReceipt` case, and a failed production is the runtime spine's own `Receipt.of("artifacts", fault)` `rejected` line off the `reliability/faults#FAULT` `BoundaryFault`, never an artifacts case.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` the one `@tagged_union(frozen=True)` keyed by the `ArtifactKind` discriminant, every case a flat-scalar `tuple[ContentKey, ...]` `case()` whose first slot is the shared `ContentKey` and whose remaining slots are the producer's named scalars — never a per-kind `Struct` re-wrapping the scalars the producer already passes positionally, and never a positional `tuple[ContentKey, str, int, float]` decoded by index. It satisfies the structural `receipts.ReceiptContributor` Protocol through `contribute`, which returns the one-element `Iterable[Receipt]` the port declares by projecting the active payload through the runtime `Receipt.of("artifacts", (phase, subject, facts))` contract — `"artifacts"` the owner, `(phase, subject, facts)` the `(Phase, str, dict[str, object])` evidence triple the runtime `of` matches onto its `fact` case. The per-case scalars are the producer facts directly: `Pdf` the byte/page counts, `Office`/`Report` the byte count, `Chart` the engine/dialect/scale/theme/byte facts (the engine the matched `ChartSpec.tag`), `Scene`/`Table` the target/format, `Preview` the dimensions plus the optional `graphic/raster/measure#MEASURE` perceptual-score band, `Bundle` the seven `package/codec#CODEC` `BundleEvidence` compression scalars (`algo`/`level`/`dict_id`/`frame_size`/`entries`/`verified`/`ratio`), `Introspection` the `document/lens#LENS` recovered-tree shape, `Egress` the `document/egress#FINISH`/`document/tagged#ACCESS` finishing facts, `Verdict` the `exchange/conformance#CONFORMANCE` `ConformanceVerdict`, `Credential` the `exchange/credential#PROVENANCE` C2PA facts, `Media` the `media/video#MEDIA`/`media/audio#MEDIA` `MediaEvidence` six-scalar encode read (container/codec/duration/byte count/frame count plus the `bit_rate` slot the `MediaEvidence` owner carries on its `measure` projection — the richest shape this owner admits so a constant-bitrate audio encode has a slot for its rate; the producer `_emit` that currently spreads only the five non-rate scalars is the seam this owner's mint is shaped to receive), `Diagram` the `visualization/diagram/layout#LAYOUT` coordinate-assignment facts, and `Metadata` the `exchange/metadata#METADATA` descriptive-field tally. The `Verdict` case is the one imported producer value object — the `exchange/conformance`-leaf `ConformanceVerdict` (the single acyclic value-object edge, never reciprocally imported); every other case carries native scalars, so the union imports no `c2pa-python`/`av`/`drawsvg`/`pikepdf` surface.
- Cases: each `case()` payload is `(ContentKey, <scalars>)` — `Pdf(key, bytes, pages)`, `Office(key, bytes)`/`Report(key, bytes)` (the byte-only finishing scalar two tags share), `Chart(key, engine, dialect, scale, theme, bytes)`, `Scene(key, target)`, `Table(key, format)`, `Preview(key, width, height, scores)` where `scores: frozendict[str, float] = frozendict()` carries the `graphic/raster/measure#MEASURE` perceptual band (`structural_similarity`/`peak_signal_noise_ratio`/`mean_squared_error`/`normalized_root_mse`/`normalized_mutual_information`/`hausdorff_distance`, plus the `contours`/`entropy`/`blobs`/`corners` and `shift`/`error` measurement facts) — an empty band for a bare save (the `scores` default the `graphic/raster/io#IO` `_emit` currently constructs), the `RasterFact.score` row a `graphic/raster/measure#MEASURE` arm produces threaded through for a measured one — the same `Preview` mint absorbing both rather than a parallel measured-raster case, the `[SCORE_FACTS]` seam that widening rides; `Bundle(key, algo, level, dict_id, frame_size, entries, verified, ratio)` the `package/codec#CODEC` `BundleEvidence` projection (the `entries`/`verified` slots the container codecs fill and the single-blob codecs leave zero); `Introspection(key, nodes, text_len, images, hits)` the recovered-tree shape; `Egress(key, bytes, pages, encryption_r, outline_depth, overlays)` the finishing facts (the `document/tagged#ACCESS` structural close mapping its element count onto `outline_depth` and figure count onto `overlays`, reusing this case rather than a parallel access case); `Verdict(key, verdict)` carrying the leaf `ConformanceVerdict`; `Credential(key, manifest_id, signer, assertions, validation_state)` the C2PA store walk; `Media(key, container, codec, duration, bytes, frames, bit_rate)` the six-scalar encode read (the `bit_rate` slot the `MediaEvidence` owner carries on `measure`, so this case has a slot for a constant-bitrate audio encode's rate; the `media/video#MEDIA` `_emit` spreading only the five non-rate scalars is the seam this mint receives, the rate slot the shape it is sized to fill); `Diagram(key, kind, nodes, edges, algorithm)` the coordinate-assignment shape; and `Metadata(key, standard, fields, bytes)` the descriptive-field tally. Three-or-more per-bucket constructions collapse into this one stream; the byte-only `Office`/`Report` share one scalar shape rather than two identical records.
- Mints: each named `@classmethod` returning `Self` is the public construction surface a producer calls — `ArtifactReceipt.Pdf(key, bytes, pages)`, `ArtifactReceipt.Media(key, container, codec, duration, bytes, frames, bit_rate)`, `ArtifactReceipt.Diagram(key, kind, nodes, edges, algorithm)` — the closed family's own constructors exactly as `Ok`/`Some` are `Result`/`Option`'s, the `@classmethod`-plus-`Self` form binding the subtype once where a `@staticmethod`-plus-`"ArtifactReceipt"`-forward-ref re-spells the type fifteen times; never a `dispatch(kind: str, *scalars)` bag and never a per-type `PdfReceipt`/`ChartReceipt` parallel family. The mints are thin: each keyword-constructs its `case()` and adds nothing, so the producer reads the natural positional call its `_emit` arm already writes while the union owns the one interior. The `Preview` mint defaults `scores=frozendict()` so a bare save calls `Preview(key, width, height)` and a measured raster threads its `RasterFact.score` row as `Preview(key, width, height, scores)`, the score band one optional argument rather than a parallel case.
- Entry: `contribute` returns the one-element `Iterable[Receipt]` the `ReceiptContributor` port declares — `return (Receipt.of("artifacts", (phase, self.slot.hex, self._facts())),)`, the one-tuple shape the sibling `compute/solvers/receipt#RECEIPT` `SolverReceipt.contribute` and `compute/solvers/field#FIELD` `FieldReceipt.contribute` hold, never a bare `Receipt` against the `Iterable` port. `phase` defaults to `emitted` because an `ArtifactReceipt` is by construction the evidence of an EMITTED artifact; the `admitted`/`planned` lifecycle facts are NOT `ArtifactReceipt` cases — the `core/plan#PLAN` planner mints them through its OWN direct `Receipt.of("artifacts", ("planned", "pipeline.plan", facts))` over the pipeline-topology facts, so the only caller threading a non-default phase is the `composition/compose#COMPOSE` `Figure.contribute(phase)` family that re-exposes the same modal signature. A failed production has no artifacts arm at all: the producer's runtime `boundary`/`async_boundary` converts the raise into a `BoundaryFault` on the `Result.Error` rail and the spine's own `Receipt.of("artifacts", fault)` mints its `rejected` line, so this owner never re-spells a `rejected(fault)` forwarder around the factory it composes. `_facts` is the one total `match self` closed by `assert_never`: its general 13-tag or-pattern binds the case's scalar tail `(_key, *tail)` and zips it against the `_KEYS[self.tag]` field-name row under `strict=True` (the table and the case tuples cannot drift) into the `dict[str, object]` the runtime `EventDict` carries with native ints/floats intact (the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializing them without a `str()` coerce), with `preview` flattening its `scores` band and `verdict` spreading the leaf `ConformanceVerdict.facts()` as the two structural arms, so a new flat kind reaches the fact map by adding its `_KEYS` row plus its or-pattern arm with the `assert_never` tail forcing the addition at type-check — never the reflective `getattr(self, self.tag)` `case _:` whose `object` residual silently absorbs a missing kind. The same `contribute` fold is the edge the runtime `execution/lanes#LANE` `(ContentKey, Work)` reuse-fabric elision threads its hit/miss distinction through, the `observability/metrics#METRIC` `MeterProvider` instrument set reads its measured-signal stream from, and the planner reads as the keyed evidence its sub-graph-elision plan distinguishes a hit from a miss on.
- Derivations: `slot` is the `ContentKey` head every case shares, bound through one total `match self` whose 15-tag or-pattern captures `(key, *_)` once and closes on `assert_never`, so the planner keys, the reuse-fabric elides, and the `MeterProvider` correlates on the one projection — sound over the closed union where a reflective `getattr(self, self.tag)` whose `tuple[object, ...]` residual erases the `ContentKey` type and defeats the exhaustiveness witness is the deleted form the sibling `compute/solvers/receipt#RECEIPT` also refuses. There is no separate `evidence` accessor: `_facts` binds the scalar tail inside its own total `match` arm, so the tail is read once at the one projection site rather than re-discriminated by a second `getattr` property. A `@receipted(redaction)`-decorated `_emit` is how a producer threads this contributor onto the egress: the runtime aspect harvests `contribute` (calling it with no argument, the default `emitted` phase) and emits through `Signals.emit_async`, so receipt production is the decorator rail the runtime owns, never an inline `emit` per producer.
- Packages: `expression` (`tagged_union`/`tag`/`case` the union); `frozendict` (the `_KEYS` field-name table and the `Preview` `scores` band); runtime (`content_identity.ContentKey` the key, `receipts.Receipt`/`ReceiptContributor`/`Phase` the port and the shape-polymorphic `of`). The failed-production `rejected` line is the spine's own `Receipt.of("artifacts", fault)` off the producer's runtime `boundary` `BoundaryFault`, so this owner imports no `reliability.faults` surface — the success contributor is the page's whole rail. The runtime `receipts.@receipted(Redaction)` aspect is the producer's harvest-and-emit composition surface — it wraps each `_emit` arm, harvests this `contribute` stream, and emits through `Signals.emit_async`, so this owner declares the contributor and the producer threads the aspect, never a `Redaction`/`@receipted` import on this page. The `Verdict` case carries the `exchange/conformance#CONFORMANCE` `ConformanceVerdict`, the union's sole imported producer value object whose `facts()` the derivation spreads; every other case is a native scalar tuple, so no `c2pa-python`, `av`, `drawsvg`, or `pikepdf` surface crosses into this owner.
- Growth: a new artifact kind is one `ArtifactKind` token plus one `@classmethod` mint plus one `_KEYS` row plus one or-pattern arm in each of `slot` and `_facts` — the `assert_never` tail breaking both matches at type-check until the arm exists, so the closed-union exhaustiveness is the growth gate rather than a silent reflective miss. A new scalar on an existing kind is one slot on its `case()` tuple plus one `_KEYS` field name, the `strict=True` zip raising if the two drift. A producer that finishes an existing kind (`document/tagged#ACCESS` over the `Egress`/`Pdf` case) reuses that mint rather than declaring a parallel one. The reuse-fabric hit/miss distinction, the `MeterProvider` signal stream, and the planner are consumers of the existing fold, never new cases.
- Boundary: a per-type `DocumentReceipt`/`PdfReceipt`/`ChartReceipt`/`MediaReceipt` family is the deleted form — the named mints construct one union. A per-kind evidence `Struct` (`PdfFacts`/`ChartFacts`/…) re-wrapping the scalars the producer already passes positionally is the rejected indirection: the producer calls `ArtifactReceipt.Pdf(key, bytes, pages)`, never `of(key, PdfFacts(bytes=…, pages=…))`, so the case holds the flat scalars and `_KEYS` names them. A positional `tuple[ContentKey, str, int, float]` decoded by index, a 15-arm `_facts` `match` hand-re-spelling `{"key": …}` per kind, and a `dict[str, str]` egress pre-formatting `str(byte_count)`/`f"{scale:.6f}"`/`f"{ratio:.6f}"` are the rejected forms the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer and its `dict[str, object]` `EventDict` are built to avoid, so the evidence stays native scalars projected once. A reflective `getattr(self, self.tag)` feeding `slot`/`_facts` — a `tuple[object, ...]` residual that escapes the closed `match` and makes the `assert_never` totality witness a lie — is the rejected escape the sibling `compute/solvers/receipt#RECEIPT` deletes by the same name; the total `match self` or-pattern over the closed union is the sound form, the `strict=True` zip the drift guard. A `@staticmethod`-plus-`"ArtifactReceipt"`-forward-ref mint family where the `@classmethod`-plus-`Self` form binds the subtype once is the deleted constructor form. A `phase` parameter sold as the lifecycle-stage discriminant is itself a near-miss: an `ArtifactReceipt` is always an emitted artifact's evidence, so the planning-stage facts ride the `core/plan#PLAN` planner's own direct `Receipt.of("artifacts", ("planned", ...))`, never an `ArtifactReceipt` case, and `phase` survives only as the `composition/compose#COMPOSE` `Figure.contribute(phase)` family's threaded signature rather than a knob this owner re-reads. An artifacts-side `rejected(fault)` arm is itself a rejected form — it forwards verbatim to the spine's own `Receipt.of("artifacts", fault)` `rejected` case (a one-hop rename of the factory carrying no artifact kind and no `ContentKey`), so a failed production rides the producer's runtime `boundary` `BoundaryFault` straight onto that spine line and this union stays success-only. The owner re-mints no content key, imports no producer engine, stores the `Verdict` case's `ConformanceVerdict` as the one leaf value object whose `facts()` the derivation spreads, and threads its egress through the runtime `@receipted` aspect rather than an inline `Signals.emit` — never a second verdict owner, a producer-module cycle, a re-minted reuse-fabric admission, a `rejected` forwarder around `Receipt.of`, or a hand-rolled emit.

```python signature
# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------
from collections.abc import Iterable
from typing import Literal, Self, assert_never

from builtins import frozendict
from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.receipts import Phase, Receipt

from artifacts.exchange.conformance import ConformanceVerdict

# --- [TYPES] ----------------------------------------------------------------------------

type ArtifactKind = Literal[
    "pdf", "office", "report", "chart", "scene", "table", "preview",
    "bundle", "introspection", "egress", "verdict", "credential", "media", "diagram", "metadata",
]

# --- [MODELS] ---------------------------------------------------------------------------

# the one primary correspondence the named mints and `_facts` share: each kind's full case slots
# (the `ContentKey` head plus its producer-scalar tail) mapped to its field names, declared once
# so `_facts` zips the matched case payload against its row under `strict=True` — the table and
# the case tuples cannot drift, a slot added to a case without its `_KEYS` name raising at the
# zip rather than dropping silently. The projection is a total `match self` closed by
# `assert_never` over the closed union, NEVER a reflective `getattr(self, self.tag)` whose
# `object` residual escapes the match and makes the `assert_never` tail a lie. `preview` carries
# its `scores` `frozendict` band as one tail slot the zip names `scores` and `_facts` flattens,
# and `verdict` is the one arm read structurally to spread the leaf `ConformanceVerdict.facts()`.


@tagged_union(frozen=True)
class ArtifactReceipt:
    # every case is `(ContentKey, <producer scalars>)` — the flat positional shape the producers
    # already call, never a per-kind facts `Struct` re-wrapping the scalars the caller passes.
    tag: ArtifactKind = tag()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    scene: tuple[ContentKey, str] = case()
    table: tuple[ContentKey, str] = case()
    preview: tuple[ContentKey, int, int, frozendict[str, float]] = case()
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
    def Chart(cls, key: ContentKey, engine: str, dialect: str, scale: float, theme: str, bytes_: int, /) -> Self:
        return cls(chart=(key, engine, dialect, scale, theme, bytes_))

    @classmethod
    def Scene(cls, key: ContentKey, target: str, /) -> Self:
        return cls(scene=(key, target))

    @classmethod
    def Table(cls, key: ContentKey, fmt: str, /) -> Self:
        return cls(table=(key, fmt))

    @classmethod
    def Preview(cls, key: ContentKey, width: int, height: int, scores: frozendict[str, float] = frozendict(), /) -> Self:
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
    def Media(cls, key: ContentKey, container: str, codec: str, duration: float, bytes_: int, frames: int, bit_rate: int, /) -> Self:
        return cls(media=(key, container, codec, duration, bytes_, frames, bit_rate))

    @classmethod
    def Diagram(cls, key: ContentKey, kind: str, nodes: int, edges: int, algorithm: str, /) -> Self:
        return cls(diagram=(key, kind, nodes, edges, algorithm))

    @classmethod
    def Metadata(cls, key: ContentKey, standard: str, fields: int, bytes_: int, /) -> Self:
        return cls(metadata=(key, standard, fields, bytes_))

    @property
    def slot(self) -> ContentKey:
        # the shared `ContentKey` head of every case, bound through one total `match self` whose
        # 15-tag or-pattern captures `(key, *_)` once and closes on `assert_never` — the closed
        # union proves exhaustiveness, so a new kind without its arm breaks at type-check, never
        # the reflective `getattr(self, self.tag)` whose `tuple[object, ...]` residual silently
        # defeats the match and erases the `ContentKey` type the planner / reuse-fabric key on.
        match self:
            case (
                ArtifactReceipt(tag="pdf", pdf=(key, *_)) | ArtifactReceipt(tag="office", office=(key, *_))
                | ArtifactReceipt(tag="report", report=(key, *_)) | ArtifactReceipt(tag="chart", chart=(key, *_))
                | ArtifactReceipt(tag="scene", scene=(key, *_)) | ArtifactReceipt(tag="table", table=(key, *_))
                | ArtifactReceipt(tag="preview", preview=(key, *_)) | ArtifactReceipt(tag="bundle", bundle=(key, *_))
                | ArtifactReceipt(tag="introspection", introspection=(key, *_)) | ArtifactReceipt(tag="egress", egress=(key, *_))
                | ArtifactReceipt(tag="verdict", verdict=(key, *_)) | ArtifactReceipt(tag="credential", credential=(key, *_))
                | ArtifactReceipt(tag="media", media=(key, *_)) | ArtifactReceipt(tag="diagram", diagram=(key, *_))
                | ArtifactReceipt(tag="metadata", metadata=(key, *_))
            ):
                return key
            case _ as unreachable:
                assert_never(unreachable)

    def _facts(self) -> dict[str, object]:
        # the ONE derivation replacing a 15-arm `{"bytes": ..., ...}` re-spelling: a total `match
        # self` whose general or-pattern binds the case's scalar tail `(_key, *tail)` and zips it
        # against the `_KEYS[self.tag]` field-name row under `strict=True` (the table and the case
        # tuples cannot drift — a slot added without its name raises at the zip), keeping native
        # ints/floats the deterministic renderer serializes. `preview` flattens its `scores`
        # `frozendict` band onto the map and `verdict` spreads the leaf `ConformanceVerdict.facts()`
        # at top level, the two structural arms; `assert_never` closes the closed union so a new
        # kind without its row is a type error, never the `getattr`-escape `case _:` that lies.
        match self:
            case ArtifactReceipt(tag="preview", preview=(_key, width, height, scores)):
                return {"width": width, "height": height, **scores}
            case ArtifactReceipt(tag="verdict", verdict=(_key, verdict)):
                return verdict.facts()
            case (
                ArtifactReceipt(tag="pdf", pdf=(_key, *tail)) | ArtifactReceipt(tag="office", office=(_key, *tail))
                | ArtifactReceipt(tag="report", report=(_key, *tail)) | ArtifactReceipt(tag="chart", chart=(_key, *tail))
                | ArtifactReceipt(tag="scene", scene=(_key, *tail)) | ArtifactReceipt(tag="table", table=(_key, *tail))
                | ArtifactReceipt(tag="bundle", bundle=(_key, *tail)) | ArtifactReceipt(tag="introspection", introspection=(_key, *tail))
                | ArtifactReceipt(tag="egress", egress=(_key, *tail)) | ArtifactReceipt(tag="credential", credential=(_key, *tail))
                | ArtifactReceipt(tag="media", media=(_key, *tail)) | ArtifactReceipt(tag="diagram", diagram=(_key, *tail))
                | ArtifactReceipt(tag="metadata", metadata=(_key, *tail))
            ):
                return dict(zip(_KEYS[self.tag], tail, strict=True))
            case _ as unreachable:
                assert_never(unreachable)

    def contribute(self, phase: Phase = "emitted") -> Iterable[Receipt]:
        # an `ArtifactReceipt` is by construction an EMITTED artifact's evidence, so `phase`
        # defaults to `emitted` and the `admitted`/`planned` lifecycle facts ride the
        # `core/plan#PLAN` planner's OWN direct `Receipt.of("artifacts", ("planned", ...))` emit,
        # never an `ArtifactReceipt` case; the parameter stays only because the `composition/
        # compose#COMPOSE` `Figure.contribute(phase)` family threads it through. Returns the
        # one-element `Iterable[Receipt]` the `ReceiptContributor` port declares, matching the
        # sibling `SolverReceipt`/`FieldReceipt` one-tuple shape, never a bare `Receipt`.
        return (Receipt.of("artifacts", (phase, self.slot.hex, self._facts())),)


# the primary scalar-tail -> field-name correspondence; `preview`/`verdict` fold specially in
# `_facts` and carry no row. Derived once here, the single edit site a new kind's facts reach.
_KEYS: frozendict[ArtifactKind, tuple[str, ...]] = frozendict({
    "pdf": ("bytes", "pages"),
    "office": ("bytes",),
    "report": ("bytes",),
    "chart": ("engine", "dialect", "scale", "theme", "bytes"),
    "scene": ("target",),
    "table": ("format",),
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

- [REUSE_ELISION] [BLOCKED]: The reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes#LANE` `(ContentKey, Work)` admission elision — every artifacts `_emit` already returns `ContentIdentity.of(...)` (the `document/emit#DOCUMENT`, `graphic/raster/io#IO`, `exchange/conformance#CONFORMANCE`, and `visualization/table#TABLE` `_emit` arms plus the `graphic/color/managed#MANAGED` derive), so the producers thread the same pre-minted key into the lane admission the runtime owns and the `ArtifactReceipt.slot` projection reads it back for the hit/miss distinction. The artifacts side is verification-and-consume only: no new case, no new owner, no re-minted key. BLOCKED-gated on the upstream runtime `execution/lanes` `(ContentKey, Work[T])` admission task (branch `CONTENT_ADDRESSED_REUSE_FABRIC`); close-condition: the runtime lane-admission surface lands and the producers thread the existing key into it. The fence above is the settled consumer edge — the `contribute` fold and the `slot` projection are the one carrier.
- [METRIC_SIGNALS] [BLOCKED]: The measured-signal leg carries the artifact's byte-volume and compression-ratio facts onto the one `contribute` fold as NATIVE int/float the runtime `Encoder(enc_hook=repr, order="deterministic")` renderer serializes unstringified — the `Bundle` `ratio`, the `Preview` `scores` band, the `Media` `duration` and `bit_rate` reach the structured log line as numbers a downstream consumer reads, never a `str()`-collapsed metric input. The scope boundary is exact: render/production DURATION is NOT an `ArtifactReceipt` fact — the runtime `observability/metrics#METRIC` `Metrics.measured` aspect times the serve coroutine with `perf_counter` onto its own `companion.request.duration` `Histogram`, and lane-drain counts ride the `DrainReceipt` over `DRAIN_COLUMNS`, neither read off this fold; so a per-artifact byte-volume or compression-ratio observable is a NEW `INSTRUMENTS` `InstrumentSpec` row on the runtime metrics owner reading these `_facts` scalars, not a capability `contribute` already delivers. The artifacts side is consume-only: the fold carries the native facts; no new artifacts surface. BLOCKED-gated on the upstream runtime `observability/metrics` instrument-set task (branch `ONE_MEASURED_SIGNAL_STREAM`); close-condition: the runtime owner adds the per-artifact byte/ratio instrument rows reading the `_facts` scalars. The fence above is the settled carrier — the native-scalar facts the runtime rows would read.
- [PLAN_FABRIC] [BLOCKED]: The production-planning leg is the third receipt-fold CONSUMER beside reuse-fabric elision and the `MeterProvider` signal stream — the `core/plan#PLAN` `ArtifactPipeline` reads each producer's resolved `ArtifactReceipt` (the `Work[ArtifactReceipt]` thunk's terminal) as the content-keyed evidence its sub-graph-elision plan distinguishes a hit from a miss on, folding every `_emit` result's pre-minted `ContentIdentity.of(...)` key into the `(ContentKey, Work[T])` pairs the runtime `execution/lanes#LANE` `LanePolicy.drain` admits as `keyed` units, so an identical producer at an identical key is elided once across the whole plan rather than per-producer. The `planned`-stage observability is NOT a phase on an `ArtifactReceipt`: the planner mints its OWN `Receipt.of("artifacts", ("planned", "pipeline.plan", facts))` directly over the pipeline-topology facts (`cardinality`/`depth`/`width`/`critical_path`), a `fact` case the runtime `PHASE_LEVEL` logs at `debug` on the same one stream — so the `admitted`/`planned`/`emitted` line family is shared by the planner's direct emit and the producers' `emitted` `ArtifactReceipt` facts, never by threading a phase through `ArtifactReceipt.contribute`. A severed-front producer surfaces its `BoundaryFault` through the runtime spine's own `Receipt.of("artifacts", fault)` `rejected` line, never an artifacts case. The artifacts side is consume-only: the planner reads the existing `contribute` carrier, the `slot` projection, and the existing pre-minted keys; no new receipt case and no parallel plan owner on this page. BLOCKED-gated on the `core/plan#PLAN` planner page authoring the `(ContentKey, Work)` fold over the runtime lane; close-condition: the planner lands and reads the `contribute` fold. The fence above is the settled carrier the planner reads.
