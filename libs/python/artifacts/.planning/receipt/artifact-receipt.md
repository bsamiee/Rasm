# [PY_ARTIFACTS_ARTIFACT_RECEIPT]

The one kind-discriminated artifact-receipt family shared across every production sub-domain. `ArtifactReceipt` replaces scattered per-type receipts with a single tagged union keyed by the runtime content key and wired through the runtime `ReceiptContributor` port. Every sub-domain — documents, reporting, charts, scene3d, tables, imaging, typography, compression — contributes one `ArtifactReceipt` case, so the receipt stream is one fact family, never a parallel rail per producer.

## [1]-[INDEX]

[CLUSTERS]: one cluster — `[2]-[RECEIPT]`, the kind-discriminated artifact-receipt family every production sub-domain contributes a case to.

## [2]-[RECEIPT]

- Owner: `ArtifactReceipt` the one kind-discriminated receipt family satisfying the structural runtime `receipts.ReceiptContributor` Protocol through its `contribute` method, keyed by the runtime `content_identity.ContentKey`; the case facts project to an emitted-phase `Receipt.of` string fact map through `ContentKey.hex`.
- Cases: `ArtifactReceipt` cases `Document` (content key, byte count) · `Pdf` (content key, byte count, page count) · `Office` (content key, byte count) · `Report` (content key, byte count) · `Chart` (content key, format) · `Scene` (content key, target) · `Table` (content key, format) · `Preview` (content key, width, height) · `Bundle` (content key, byte count) — each a frozen `case()` carrying the content key and the mode-specific facts; three-or-more per-bucket constructions collapse into this one stream.
- Entry: `contribute` folds the active case onto the runtime `Receipt.of` emitted-phase stream under the `artifacts` owner tag, the case tag as subject, and the case-specific facts as the string-valued fact map; the fold is one `match` over the union, never a per-case contributor.
- Packages: `expression` (`tagged_union`/`tag`/`case`), runtime (`content_identity.ContentKey`, `receipts.Receipt`/`ReceiptContributor`).
- Growth: a new artifact kind is one `ArtifactReceipt` case plus one constructor; zero new surface.
- Boundary: a per-type `DocumentReceipt`/`PdfReceipt`/`ChartReceipt` family is the deleted form; the owner consumes the runtime port and `expression` only and re-mints no content key.

```python signature
from typing import Literal, assert_never

from expression import case, tag, tagged_union

from rasm.runtime.content_identity import ContentKey
from rasm.runtime.receipts import Receipt


@tagged_union(frozen=True)
class ArtifactReceipt:
    tag: Literal["document", "pdf", "office", "report", "chart", "scene", "table", "preview", "bundle"] = tag()
    document: tuple[ContentKey, int] = case()
    pdf: tuple[ContentKey, int, int] = case()
    office: tuple[ContentKey, int] = case()
    report: tuple[ContentKey, int] = case()
    chart: tuple[ContentKey, str] = case()
    scene: tuple[ContentKey, str] = case()
    table: tuple[ContentKey, str] = case()
    preview: tuple[ContentKey, int, int] = case()
    bundle: tuple[ContentKey, int] = case()

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
    def Chart(key: ContentKey, fmt: str) -> "ArtifactReceipt":
        return ArtifactReceipt(chart=(key, fmt))

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
    def Bundle(key: ContentKey, byte_count: int) -> "ArtifactReceipt":
        return ArtifactReceipt(bundle=(key, byte_count))

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
        case ArtifactReceipt(tag="chart", chart=(key, fmt)):
            return {"key": key.hex, "format": fmt}
        case ArtifactReceipt(tag="scene", scene=(key, target)):
            return {"key": key.hex, "target": target}
        case ArtifactReceipt(tag="table", table=(key, fmt)):
            return {"key": key.hex, "format": fmt}
        case ArtifactReceipt(tag="preview", preview=(key, width, height)):
            return {"key": key.hex, "width": str(width), "height": str(height)}
        case ArtifactReceipt(tag="bundle", bundle=(key, byte_count)):
            return {"key": key.hex, "bytes": str(byte_count)}
        case _:
            assert_never(receipt)
```
