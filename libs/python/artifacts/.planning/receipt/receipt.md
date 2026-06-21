# [PY_ARTIFACTS_RECEIPT]

The one kind-discriminated artifact receipt family shared across every production sub-domain. `ArtifactReceipt` replaces scattered per-type receipts with a single tagged union keyed by the runtime content key and wired through the runtime `ReceiptContributor` port. Every sub-domain — documents, reporting, charts, scene3d, tables, imaging, typography, compression — contributes one `ArtifactReceipt` case, so the receipt stream is one fact family, never a parallel rail per producer.

## [01]-[INDEX]

- [01]-[RECEIPT]: kind-discriminated artifact receipt family every production sub-domain contributes a case to, including the bidirectional-seam introspection case, the egress finishing case, and the conformance-audit verdict case; the one `contribute` fold the runtime reuse-fabric elision and `MeterProvider` signal stream both consume.

## [02]-[RECEIPT]

- Owner: `ArtifactReceipt` the one kind-discriminated receipt family satisfying the structural runtime `receipts.ReceiptContributor` Protocol through its `contribute` method, keyed by the runtime `content_identity.ContentKey`; the case facts project to an emitted-phase `Receipt.of` string fact map through `ContentKey.hex`. The three seam cases — `Introspection` (the `documents/lens#LENS` recover-TO half), `Egress` (the `documents/egress#FINISH` security-and-navigation close), and `Verdict` (the `typography/conformance#CONFORM` `ConformanceVerdict` audit) — land here once so the three downstream owners contribute a settled case rather than three conflicting same-file edits.
- Cases: `ArtifactReceipt` cases `Document` (content key, byte count) · `Pdf` (content key, byte count, page count) · `Office` (content key, byte count) · `Report` (content key, byte count) · `Chart` (content key, engine, spec dialect, render scale, resolved theme, output byte length — the host-free render facts the chart owner produces, the engine being the matched `ChartSpec.tag` not a parallel enum) · `Scene` (content key, target) · `Table` (content key, format) · `Preview` (content key, width, height) · `Bundle` (content key, algorithm, compression level, dictionary id, frame size, entry count, CRC-verified count, ratio — the typed compression-evidence facts the bundle owner's `BundleEvidence` produces, flattened to scalar fields so the receipt owner imports no producer module and no codec handle crosses the seam) · `Introspection` (content key, node count, text length, image count, search-hit count — the recovered-tree-shape facts the lens recovers TO `DocumentNode`) · `Egress` (content key, post-finish byte count, page count, encryption-R level, outline depth, overlay count — the security-and-navigation finishing facts) · `Verdict` (content key, the `ConformanceVerdict` value carrying PAdES validity / coverage level / modification-difference level / DSS-LTV completeness / archival-metadata presence) — each a frozen `case()` carrying the content key and the mode-specific facts; three-or-more per-bucket constructions collapse into this one stream. The producer-evidence cases carry flat scalar fields rather than a producer value object (`Egress`/`Introspection`/`Chart`/`Bundle` all flatten), so the only value object the receipt owner imports is the `conformance`-leaf `ConformanceVerdict`, which the `conformance` owner never reciprocally imports — the one acyclic value-object edge the union admits.
- Entry: `contribute` folds the active case onto the runtime `Receipt.of` emitted-phase stream under the `artifacts` owner tag, the case tag as subject, and the case-specific facts as the string-valued fact map; the fold is one `match` over the union, never a per-case contributor. The same `contribute` fold is the one edge the runtime `execution/lanes` `(ContentKey, Work)` reuse-fabric elision threads its hit/miss distinction through and the runtime `observability/metrics` `MeterProvider` instrument set reads its measured-signal stream from — both consumers of the single fold, never a parallel cache or metric owner.
- Packages: `expression` (`tagged_union`/`tag`/`case`), runtime (`content_identity.ContentKey`, `receipts.Receipt`/`ReceiptContributor`); the `Verdict` case carries a `typography/conformance#CONFORM` `ConformanceVerdict` value.
- Growth: a new artifact kind is one `ArtifactReceipt` case plus one constructor plus one `_facts` arm; zero new surface. The reuse-fabric hit/miss distinction and the `MeterProvider` signal stream are consumers of the existing fold, never new cases.
- Boundary: a per-type `DocumentReceipt`/`PdfReceipt`/`ChartReceipt`/`IntrospectionReceipt`/`EgressReceipt`/`VerdictReceipt` family is the deleted form; the owner consumes the runtime port and `expression` only and re-mints no content key. The `Verdict` case stores the verdict value's facts projected to strings through `_facts`, never a second verdict owner; the reuse-fabric `(ContentKey, Work)` admission and the `MeterProvider` instrument set are consumed from runtime, never re-minted here.

```python signature
from typing import Literal, assert_never

from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.receipts import Receipt

from artifacts.typography.conformance import ConformanceVerdict


@tagged_union(frozen=True)
class ArtifactReceipt:
    tag: Literal["document", "pdf", "office", "report", "chart", "scene", "table", "preview", "bundle", "introspection", "egress", "verdict"] = tag()
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

    def contribute(self) -> Receipt:
        return Receipt.of("emitted", "artifacts", self.tag, _facts(self))


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
        case _:
            assert_never(receipt)
```

## [03]-[SIGNALS]

- [REUSE_ELISION] [BLOCKED]: The reuse-fabric leg is the receipt-fold CONSUMER of the runtime `execution/lanes` `(ContentKey, Work)` admission elision — every artifacts `_emit` already returns `ContentIdentity.of(...)` (the `documents/emit#DOCUMENT`, `figures/preview#PREVIEW`, `typography/conformance#CONFORM`, and `figures/table#TABLE` `_emit` arms plus the `figures/color#COLOR` derive), so the producers thread the same pre-minted key into the lane admission the runtime owns and the `ArtifactReceipt` carries the hit/miss distinction. The artifacts side is verification-and-consume only: no new case, no new owner, no re-minted key. BLOCKED-gated on the upstream runtime `execution/lanes` `(ContentKey, Work[T])` admission task (branch `CONTENT_ADDRESSED_REUSE_FABRIC`); close-condition: the runtime lane-admission surface lands and the producers thread the existing key into it. The fence above is already the settled consumer edge — the `contribute` fold is the one carrier.
- [METRIC_SIGNALS] [BLOCKED]: The measured-signal leg is the receipt-fold CONSUMER of the runtime `observability/metrics` `MeterProvider` instrument set — production duration / byte-volume / compression-ratio signals route through the single `contribute` fold rather than a parallel artifacts metric owner, so render-duration histograms and output-byte gauges are first-class observable instruments on the one branch stream. The artifacts side is consume-only: the `contribute` edge feeds the runtime instruments at composition; no new artifacts surface. BLOCKED-gated on the upstream runtime `observability/metrics` instrument-set task (branch `ONE_MEASURED_SIGNAL_STREAM`); close-condition: the runtime instrument set lands and `contribute` records against it. The fence above is the settled consumer edge — both fabrics read the one fold.
