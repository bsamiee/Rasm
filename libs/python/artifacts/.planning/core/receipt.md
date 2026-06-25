# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact receipt family shared across every production sub-domain. `ArtifactReceipt` replaces scattered per-type receipts with a single tagged union keyed by the runtime content key and wired through the runtime `ReceiptContributor` port. Every sub-domain — documents, reporting, charts, scene3d, tables, imaging, typography, compression, provenance, media, diagrams, descriptive-metadata — contributes one `ArtifactReceipt` case, so the receipt stream is one fact family, never a parallel rail per producer.

## [01]-[INDEX]

- [01]-[RECEIPT]: kind-discriminated artifact receipt family every production sub-domain contributes a case to, including the bidirectional-seam introspection case, the egress finishing case, the conformance-audit verdict case (its `ConformanceVerdict` value sourced from `exchange/conformance#CONFORMANCE`), the C2PA content-credential case, the media-encode case, the diagram coordinate-assignment case, and the descriptive-metadata case; the one `contribute` fold the runtime reuse-fabric elision, the `MeterProvider` signal stream, and the `core/plan#PLAN` content-keyed production planner all consume.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` the one kind-discriminated receipt family satisfying the structural runtime `receipts.ReceiptContributor` Protocol through its `contribute` method, keyed by the runtime `content_identity.ContentKey`; `contribute` yields the one-element `Iterable[Receipt]` stream the port declares, projecting the active case through the 2-positional `Receipt.of("artifacts", ("emitted", self.tag, facts))` contract — `"artifacts"` the owner, `("emitted", self.tag, facts)` the canonical `(Phase, subject, facts)` triple whose string fact map projects through `ContentKey.hex`. The seam cases — `Introspection` (the `document/lens#LENS` recover-TO half), `Egress` (the `document/egress#FINISH` security-and-navigation close, also the settled target the `document/tagged#ACCESS` structural-conformance producer reuses rather than minting a parallel access case), `Verdict` (the `exchange/conformance#CONFORMANCE` `ConformanceVerdict` audit), `Credential` (the `exchange/credential#PROVENANCE` C2PA content-credential bind), `Media` (the `media/video#MEDIA`/`media/audio#MEDIA` container/codec encode), `Diagram` (the `visualization/diagram/layout#LAYOUT` coordinate-assignment kind/node/edge/algorithm facts), and `Metadata` (the `exchange/metadata#METADATA` EXIF/XMP/IPTC descriptive-field facts) — land here once so each downstream owner contributes a settled case rather than conflicting same-file edits.
- Cases: `ArtifactReceipt` cases `Document` (content key, byte count) · `Pdf` (content key, byte count, page count) · `Office` (content key, byte count) · `Report` (content key, byte count) · `Chart` (content key, engine, spec dialect, render scale, resolved theme, output byte length — the host-free render facts the chart owner produces, the engine being the matched `ChartSpec.tag` not a parallel enum) · `Scene` (content key, target) · `Table` (content key, format) · `Preview` (content key, width, height) · `Bundle` (content key, algorithm, compression level, dictionary id, frame size, entry count, CRC-verified count, ratio — the typed compression-evidence facts the bundle owner's `BundleEvidence` produces, flattened to scalar fields so the receipt owner imports no producer module and no codec handle crosses the seam) · `Introspection` (content key, node count, text length, image count, search-hit count — the recovered-tree-shape facts the lens recovers TO `DocumentNode`) · `Egress` (content key, post-finish byte count, page count, encryption-R level, outline depth, overlay count — the security-and-navigation finishing facts) · `Verdict` (content key, the `ConformanceVerdict` value carrying PAdES validity / coverage level / modification-difference level / DSS-LTV completeness / archival-metadata presence) · `Credential` (content key, c2pa manifest id, signer identity, assertion count, validation-state — the C2PA content-credential facts the `exchange/credential#PROVENANCE` producer reads off the signed manifest store, flat scalars) · `Media` (content key, container, codec, duration, byte count, frame count — the temporal-artifact encode facts the `media/video#MEDIA`/`media/audio#MEDIA` producer measures off the muxed output container, flat scalars) · `Diagram` (content key, diagram kind, node count, edge count, layout algorithm — the coordinate-assignment facts the `visualization/diagram/layout#LAYOUT` owner produces off the built presentation graph, the kind the `DiagramKind` value and the algorithm the matched `LayoutPolicy.tag`, flat scalars so the receipt owner imports no `DiagramGlyph`/`DiagramLayout` value object) · `Metadata` (content key, metadata standard, field count, byte length — the descriptive-metadata facts the `exchange/metadata#METADATA` owner recovers/binds across the EXIF/XMP/IPTC standards, the standard the `MetaStandard` value and the field count the populated descriptive-field tally off `MetaFacts`, flat scalars so the receipt owner imports no `MetaFacts` value object) — each a frozen `case()` carrying the content key and the mode-specific facts; three-or-more per-bucket constructions collapse into this one stream. The producer-evidence cases carry flat scalar fields rather than a producer value object (`Egress`/`Introspection`/`Chart`/`Bundle`/`Credential`/`Media`/`Diagram`/`Metadata` all flatten so the receipt owner imports neither `c2pa-python` nor `av` nor `drawsvg`/`rustworkx` nor `pikepdf`/`exif`/`iptcinfo3`, and no native manifest, container, graph, or document handle crosses the seam), so the only value object the receipt owner imports is the `exchange/conformance`-leaf `ConformanceVerdict`, which the `conformance` owner never reciprocally imports — the one acyclic value-object edge the union admits.
- Entry: `contribute` yields the active case onto the runtime `Receipt.of` emitted-phase stream as a one-element `Iterable[Receipt]` through `yield Receipt.of("artifacts", ("emitted", self.tag, _facts(self)))` — the `artifacts` owner as the first positional, the `(Phase, subject, facts)` triple as the second carrying the `emitted` phase, the case tag as subject, and the case-specific string-valued fact map; the projection is one `_facts` `match` over the union feeding the streamed receipt, never a per-case contributor and never a four-positional `Receipt.of`. The same `contribute` fold is the one edge the runtime `execution/lanes` `(ContentKey, Work)` reuse-fabric elision threads its hit/miss distinction through, the runtime `observability/metrics` `MeterProvider` instrument set reads its measured-signal stream from, and the `core/plan#PLAN` content-keyed planner walks into its sub-graph-elision plan — the three consumers of the single fold, never a parallel cache, metric, or plan owner.
- Packages: `expression` (`tagged_union`/`tag`/`case`), runtime (`content_identity.ContentKey`, `receipts.Receipt`/`ReceiptContributor`); the `Verdict` case carries a `exchange/conformance#CONFORMANCE` `ConformanceVerdict` value, the union's sole imported producer value object — the `Credential`, `Media`, `Diagram`, and `Metadata` cases carry only flat scalars, so no `c2pa-python`, `av`, `drawsvg`/`rustworkx`, or `pikepdf`/`exif`/`iptcinfo3` surface crosses into this owner.
- Growth: a new artifact kind is one `ArtifactReceipt` case plus one constructor plus one `_facts` arm plus one `tag` `Literal` token; zero new surface. The reuse-fabric hit/miss distinction, the `MeterProvider` signal stream, and the `core/plan#PLAN` content-keyed planner are consumers of the existing `contribute` fold, never new cases — and a producer that finishes an existing kind (the `document/tagged#ACCESS` structural close over the `Egress`/`Pdf` case) reuses that case rather than minting a parallel one.
- Boundary: a per-type `DocumentReceipt`/`PdfReceipt`/`ChartReceipt`/`IntrospectionReceipt`/`EgressReceipt`/`VerdictReceipt`/`CredentialReceipt`/`MediaReceipt`/`DiagramReceipt`/`MetadataReceipt` family is the deleted form; the owner consumes the runtime port and `expression` only and re-mints no content key. The `Verdict` case stores the verdict value's facts projected to strings through `_facts`, never a second verdict owner; the `Credential`, `Media`, `Diagram`, and `Metadata` cases store the producer evidence as flat scalars rather than importing a `c2pa-python` manifest store, an `av` container handle, a `drawsvg`/`rustworkx` diagram graph, or a `MetaFacts` value object, so no producer-module cycle forms and `document/tagged#ACCESS` reuses the existing `Egress`/`Pdf` case rather than minting a parallel one; the reuse-fabric `(ContentKey, Work)` admission, the `MeterProvider` instrument set, and the `core/plan#PLAN` planner are consumed from runtime and the planner owner, never re-minted here.

```python signature
from collections.abc import Iterable
from typing import Literal, assert_never

from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.receipts import Receipt

from artifacts.exchange.conformance import ConformanceVerdict


@tagged_union(frozen=True)
class ArtifactReceipt:
    tag: Literal["document", "pdf", "office", "report", "chart", "scene", "table", "preview", "bundle", "introspection", "egress", "verdict", "credential", "media", "diagram", "metadata"] = tag()
    document: tuple[ContentKey, int] = case()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str, str, float, str, int] = case()
    scene: tuple[ContentKey, str] = case()
    table: tuple[ContentKey, str] = case()
    preview: tuple[ContentKey, int, int] = case()
    bundle: tuple[ContentKey, str, int, int, int, int, int, float] = case()
    introspection: tuple[ContentKey, int, int, int, int] = case()
    egress: tuple[ContentKey, int, int, int, int, int] = case()
    verdict: tuple[ContentKey, ConformanceVerdict] = case()
    credential: tuple[ContentKey, str, str, int, str] = case()
    media: tuple[ContentKey, str, str, float, int, int] = case()
    diagram: tuple[ContentKey, str, int, int, str] = case()
    metadata: tuple[ContentKey, str, int, int] = case()

    @staticmethod
    def Document(key: ContentKey, byte_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(document=(key, byte_count))

    @staticmethod
    def Pdf(key: ContentKey, byte_count: int, pages: int) -> "ArtifactReceipt":
        return ArtifactReceipt(pdf=(key, byte_count, pages))

    @staticmethod
    def Office(key: ContentKey, byte_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(office=(key, byte_count))

    @staticmethod
    def Report(key: ContentKey, byte_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(report=(key, byte_count))

    @staticmethod
    def Chart(key: ContentKey, engine: str, dialect: str, scale: float, theme: str, byte_len: int) -> "ArtifactReceipt":
        return ArtifactReceipt(chart=(key, engine, dialect, scale, theme, byte_len))

    @staticmethod
    def Scene(key: ContentKey, target: str) -> "ArtifactReceipt":
        return ArtifactReceipt(scene=(key, target))

    @staticmethod
    def Table(key: ContentKey, fmt: str) -> "ArtifactReceipt":
        return ArtifactReceipt(table=(key, fmt))

    @staticmethod
    def Preview(key: ContentKey, width: int, height: int) -> "ArtifactReceipt":
        return ArtifactReceipt(preview=(key, width, height))

    @staticmethod
    def Bundle(key: ContentKey, algo: str, level: int, dict_id: int, frame_size: int, entries: int, verified: int, ratio: float) -> "ArtifactReceipt":
        return ArtifactReceipt(bundle=(key, algo, level, dict_id, frame_size, entries, verified, ratio))

    @staticmethod
    def Introspection(key: ContentKey, nodes: int, text_len: int, images: int, hits: int) -> "ArtifactReceipt":
        return ArtifactReceipt(introspection=(key, nodes, text_len, images, hits))

    @staticmethod
    def Egress(key: ContentKey, byte_count: int, pages: int, encryption_r: int, outline_depth: int, overlays: int) -> "ArtifactReceipt":
        return ArtifactReceipt(egress=(key, byte_count, pages, encryption_r, outline_depth, overlays))

    @staticmethod
    def Verdict(key: ContentKey, verdict: ConformanceVerdict) -> "ArtifactReceipt":
        return ArtifactReceipt(verdict=(key, verdict))

    @staticmethod
    def Credential(key: ContentKey, manifest_id: str, signer: str, assertions: int, validation_state: str) -> "ArtifactReceipt":
        return ArtifactReceipt(credential=(key, manifest_id, signer, assertions, validation_state))

    @staticmethod
    def Media(key: ContentKey, container: str, codec: str, duration: float, byte_count: int, frame_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(media=(key, container, codec, duration, byte_count, frame_count))

    @staticmethod
    def Diagram(key: ContentKey, kind: str, nodes: int, edges: int, algorithm: str) -> "ArtifactReceipt":
        return ArtifactReceipt(diagram=(key, kind, nodes, edges, algorithm))

    @staticmethod
    def Metadata(key: ContentKey, standard: str, fields: int, byte_len: int) -> "ArtifactReceipt":
        return ArtifactReceipt(metadata=(key, standard, fields, byte_len))

    def contribute(self) -> Iterable[Receipt]:
        yield Receipt.of("artifacts", ("emitted", self.tag, _facts(self)))


def _facts(receipt: ArtifactReceipt) -> dict[str, str]:
    match receipt:
        case ArtifactReceipt(tag="document", document=(key, byte_count)):
            return {"key": key.hex, "bytes": str(byte_count)}
        case ArtifactReceipt(tag="pdf", pdf=(key, byte_count, pages)):
            return {"key": key.hex, "bytes": str(byte_count), "pages": str(pages)}
        case ArtifactReceipt(tag="office", office=(key, byte_count)):
            return {"key": key.hex, "bytes": str(byte_count)}
        case ArtifactReceipt(tag="report", report=(key, byte_count)):
            return {"key": key.hex, "bytes": str(byte_count)}
        case ArtifactReceipt(tag="chart", chart=(key, engine, dialect, scale, theme, byte_len)):
            return {"key": key.hex, "engine": engine, "dialect": dialect, "scale": f"{scale:.6f}", "theme": theme, "bytes": str(byte_len)}
        case ArtifactReceipt(tag="scene", scene=(key, target)):
            return {"key": key.hex, "target": target}
        case ArtifactReceipt(tag="table", table=(key, fmt)):
            return {"key": key.hex, "format": fmt}
        case ArtifactReceipt(tag="preview", preview=(key, width, height)):
            return {"key": key.hex, "width": str(width), "height": str(height)}
        case ArtifactReceipt(tag="bundle", bundle=(key, algo, level, dict_id, frame_size, entries, verified, ratio)):
            return {
                "key": key.hex,
                "algo": algo,
                "level": str(level),
                "dict_id": str(dict_id),
                "frame_size": str(frame_size),
                "entries": str(entries),
                "verified": str(verified),
                "ratio": f"{ratio:.6f}",
            }
        case ArtifactReceipt(tag="introspection", introspection=(key, nodes, text_len, images, hits)):
            return {"key": key.hex, "nodes": str(nodes), "text_len": str(text_len), "images": str(images), "hits": str(hits)}
        case ArtifactReceipt(tag="egress", egress=(key, byte_count, pages, encryption_r, outline_depth, overlays)):
            return {
                "key": key.hex,
                "bytes": str(byte_count),
                "pages": str(pages),
                "encryption_r": str(encryption_r),
                "outline_depth": str(outline_depth),
                "overlays": str(overlays),
            }
        case ArtifactReceipt(tag="verdict", verdict=(key, verdict)):
            return {"key": key.hex, **verdict.facts()}
        case ArtifactReceipt(tag="credential", credential=(key, manifest_id, signer, assertions, validation_state)):
            return {"key": key.hex, "manifest_id": manifest_id, "signer": signer, "assertions": str(assertions), "validation_state": validation_state}
        case ArtifactReceipt(tag="media", media=(key, container, codec, duration, byte_count, frame_count)):
            return {"key": key.hex, "container": container, "codec": codec, "duration": f"{duration:.6f}", "bytes": str(byte_count), "frames": str(frame_count)}
        case ArtifactReceipt(tag="diagram", diagram=(key, kind, nodes, edges, algorithm)):
            return {"key": key.hex, "kind": kind, "nodes": str(nodes), "edges": str(edges), "algorithm": algorithm}
        case ArtifactReceipt(tag="metadata", metadata=(key, standard, fields, byte_len)):
            return {"key": key.hex, "standard": standard, "fields": str(fields), "bytes": str(byte_len)}
        case _:
            assert_never(receipt)
```

## [03]-[SIGNALS]

- [REUSE_ELISION] [BLOCKED]: The reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes` `(ContentKey, Work)` admission elision — every artifacts `_emit` already returns `ContentIdentity.of(...)` (the `document/emit#DOCUMENT`, `graphic/raster/measure#PREVIEW`, `exchange/conformance#CONFORMANCE`, and `visualization/table#TABLE` `_emit` arms plus the `graphic/color#COLOR` derive), so the producers thread the same pre-minted key into the lane admission the runtime owns and the `ArtifactReceipt` carries the hit/miss distinction. The artifacts side is verification-and-consume only: no new case, no new owner, no re-minted key. BLOCKED-gated on the upstream runtime `execution/lanes` `(ContentKey, Work[T])` admission task (branch `CONTENT_ADDRESSED_REUSE_FABRIC`); close-condition: the runtime lane-admission surface lands and the producers thread the existing key into it. The fence above is already the settled consumer edge — the `contribute` fold is the one carrier.
- [METRIC_SIGNALS] [BLOCKED]: The measured-signal leg is the receipt-fold CONSUMER of the runtime `observability/metrics` `MeterProvider` instrument set — production duration / byte-volume / compression-ratio signals route through the single `contribute` fold rather than a parallel artifacts metric owner, so render-duration histograms and output-byte gauges are first-class observable instruments on the one branch stream. The artifacts side is consume-only: the `contribute` edge feeds the runtime instruments at composition; no new artifacts surface. BLOCKED-gated on the upstream runtime `observability/metrics` instrument-set task (branch `ONE_MEASURED_SIGNAL_STREAM`); close-condition: the runtime instrument set lands and `contribute` records against it. The fence above is the settled consumer edge — all three fabrics read the one fold.
- [PLAN_FABRIC] [BLOCKED]: The production-planning leg is the third receipt-fold CONSUMER beside reuse-fabric elision and the `MeterProvider` signal stream — the `core/plan#PLAN` `ArtifactPipeline` walks each producer's per-artifact `contribute` fold into the content-keyed sub-graph-elision plan, folding every `_emit` result's `ContentIdentity.of(...)` key into the `Keyed[T] = (ContentKey, Work[T])` pairs the runtime `execution/lanes` `LanePolicy.cached` admits, so an identical producer at an identical key is elided once across the whole plan rather than per-producer. The artifacts side is consume-only: the planner reads the existing `contribute` carrier and the existing pre-minted keys; no new receipt case and no parallel plan owner on this page. BLOCKED-gated on the `core/plan#PLAN` planner page authoring the `Keyed` fold over the runtime lane; close-condition: the planner lands and walks the `contribute` fold. The fence above is the settled carrier the planner reads.
