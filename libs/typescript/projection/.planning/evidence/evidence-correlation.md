# [PROJECTION_EVIDENCE_CORRELATION]

The receipt and evidence projection — `ReceiptStore` over the compute-receipt union, `EvidenceFeed` ordered by HLC stamp, and content-keyed evidence correlation that keys and dedups evidence rows on the `interchange`-assembled `ContentKey`. Reassembly lives in `interchange/artifacts/frame-reassembly#FRAME_RAIL` (`ArtifactFrameRail`: per-frame Crc32 + pre-sized `Uint8Array` sink + whole-artifact `xxhash-wasm` content-key derivation, off-main-thread in `platform`'s `DecodeWorkerPool`). This fold consumes the already-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest — a re-fetch of identical content hits the cached row — and never re-derives the hash. A second hash mint here is the named cross-language drift defect. The folds are rows of the same `keyed-fold#KEYED_FOLD` algebra, split from the stream stores only by payload.

## [1]-[INDEX]

One cluster: `[2]-[EVIDENCE_CORRELATION]` owns `ReceiptStore`, `EvidenceFeed`, and the content-keyed correlation row.

## [2]-[EVIDENCE_CORRELATION]

- Owner: `ReceiptStore` for the compute-receipt fold, `EvidenceFeed` for the HLC-ordered evidence fold, and the content-keyed correlation that keys evidence and receipt rows on the imported `ContentKey` so a cache hit by digest dedups without re-decoding.
- Cases: `ReceiptStore` folds the compute-receipt union against `csharp:Rasm.Compute/receipts/receipts#TS_PROJECTION`; `EvidenceFeed` orders rows by the HLC stamp against `csharp:Rasm.AppUi/evidence/diagnostics-evidence#TS_PROJECTION`, decoding embedded geometry through the `interchange` `GeometryRail`; the correlation row keys on the `interchange` `ContentKey` brand so two evidence rows referencing the same assembled `ArtifactBlob` share one correlation cell.
- Packages: `effect` for `Schema`, `SubscriptionRef`, and ordering primitives.
- Growth: a new receipt payload lands as one payload row bound through `envelope/receipt-envelope#RECEIPT_ENVELOPE`, zero new carrier; a new evidence kind lands as one fold arm.
- Boundary: cluster geometry rides inside the envelope payload, not on the evidence row shape; embedded geometry decodes through the `interchange` rail, never a second decode here; the `ContentKey` arrives assembled from `interchange/artifacts/frame-reassembly#FRAME_RAIL` and is never re-minted; the domain dials no transport.

```ts contract
import { Schema, SubscriptionRef } from "effect";
import type { ComputeReceiptWire, ContentKey, EvidenceRowWire } from "@rasm/ts";
import { ReceiptEnvelopeCarrier } from "../envelope/receipt-envelope";

const EvidenceEnvelope = ReceiptEnvelopeCarrier(EvidenceRowWire);
const decodeEvidence = Schema.decodeUnknown(Schema.Array(EvidenceEnvelope));

interface EvidenceFeed {
  readonly ordered: SubscriptionRef.SubscriptionRef<ReadonlyArray<Schema.Schema.Type<typeof EvidenceEnvelope>>>;
}

interface ReceiptStore {
  readonly rows: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, ComputeReceiptWire>>;
  readonly byContentKey: (key: ContentKey) => ReadonlyArray<ComputeReceiptWire>;
}
```
