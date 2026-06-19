# [PROJECTION_EVIDENCE_CORRELATION]

The receipt and evidence projection — one `EvidenceProjection` store binding the compute-receipt keyed fold, the HLC-ordered evidence fold, and content-keyed evidence correlation that keys and dedups evidence rows on the `interchange`-assembled `ContentKey`. Reassembly lives in `interchange/artifacts/frame-reassembly#FRAME_RAIL` (`ArtifactFrameRail`: per-frame Crc32 + pre-sized `Uint8Array` sink + whole-artifact `xxhash-wasm` content-key derivation, off-main-thread in `platform`'s `DecodeWorkerPool`). This fold consumes the already-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest — a re-fetch of identical content hits the cached row — and never re-derives the hash. A second hash mint here is the named cross-language drift defect. The folds are rows of the same `keyed-fold#KEYED_FOLD` algebra, split from the stream stores only by payload.

## [1]-[INDEX]

One cluster: `[2]-[EVIDENCE_CORRELATION]` owns `EvidenceProjection`, its receipt and evidence folds, and the content-keyed `byContentKey` correlation lookup.

## [2]-[EVIDENCE_CORRELATION]

- Owner: `EvidenceProjection`, the one store binding the compute-receipt keyed fold, the HLC-ordered evidence fold, and the content-keyed correlation, forked into one `Scope` by `evidenceProjection`. The receipt fold keys on the `convergence/lww-merge#LWW_MERGE` `keyHex` transcription so a cache hit by digest dedups without re-decoding; `byContentKey` is the derived lookup off that map, never a parallel query surface; `byHlc` is the lexicographic HLC `Order` the evidence timeline sorts by, one composed ordering value rather than an inline comparator per sort.
- Cases: the receipt fold folds the compute-receipt union against `csharp:Rasm.Compute/receipts/receipts#TS_PROJECTION`, grouped by the `ContentKey` hex so every receipt referencing one assembled `ArtifactBlob` shares one correlation cell; the evidence fold orders rows by the HLC stamp against `csharp:Rasm.AppUi/evidence/diagnostics-evidence#TS_PROJECTION`, decoding embedded geometry through the `interchange` `GeometryRail`; the correlation hex is the convergence identity `lww-merge#LWW_MERGE` settles, read here for grouping, never re-minted.
- Packages: `effect` for `Schema`, `Stream`, `SubscriptionRef`, `Effect`, `Scope`, `HashMap`, `Option`, and `Order`.
- Growth: a new receipt payload lands as one payload row bound through `envelope/receipt-envelope#RECEIPT_ENVELOPE`, zero new carrier; a new evidence kind lands as one fold arm; the `byContentKey` lookup never grows.
- Boundary: cluster geometry rides inside the envelope payload, not on the evidence row shape; embedded geometry decodes through the `interchange` rail, never a second decode here; the `ContentKey` arrives assembled from `interchange/artifacts/frame-reassembly#FRAME_RAIL` and is never re-minted; both folds pipe through `stream-policy#STREAM_POLICY`; the domain dials no transport.

```ts contract
import { Effect, HashMap, Option, Order, Scope, Schema, Stream, SubscriptionRef } from "effect";
import type { ComputeReceiptWire, ContentKey, EvidenceRowWire } from "@rasm/interchange";
import { foldStream, keyedFold } from "../fold-core/keyed-fold";
import type { StreamPolicy } from "../fold-core/stream-policy";
import { ReceiptEnvelopeCarrier } from "../envelope/receipt-envelope";
import { keyHex } from "../convergence/lww-merge";

const EvidenceEnvelope = ReceiptEnvelopeCarrier(EvidenceRowWire);
type EvidenceEnvelope = Schema.Schema.Type<typeof EvidenceEnvelope>;

const byHlc = Order.combine(
  Order.mapInput(Order.Date, (e: EvidenceEnvelope) => new Date(e.hlc.physical)),
  Order.mapInput(Order.bigint, (e: EvidenceEnvelope) => e.hlc.logical),
);

interface EvidenceProjection {
  readonly receipts: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, ReadonlyArray<ComputeReceiptWire>>>;
  readonly ordered: SubscriptionRef.SubscriptionRef<ReadonlyArray<EvidenceEnvelope>>;
  readonly byContentKey: (key: ContentKey) => Effect.Effect<ReadonlyArray<ComputeReceiptWire>>;
}

const evidenceProjection = (
  receiptFeed: Stream.Stream<ComputeReceiptWire>,
  evidenceFeed: Stream.Stream<EvidenceEnvelope>,
  policy: StreamPolicy,
): Effect.Effect<EvidenceProjection, never, Scope.Scope> =>
  Effect.gen(function* () {
    const receipts = yield* keyedFold(
      receiptFeed,
      (r) => keyHex(r.contentKey),
      (prior, r) => [...Option.getOrElse(prior, () => [] as ReadonlyArray<ComputeReceiptWire>), r],
      policy,
    );
    const ordered = yield* foldStream(
      evidenceFeed,
      [] as ReadonlyArray<EvidenceEnvelope>,
      (rows, e) => [...rows, e].sort(byHlc),
      policy,
    );
    return {
      receipts,
      ordered,
      byContentKey: (key) =>
        SubscriptionRef.get(receipts).pipe(
          Effect.map((m) => HashMap.get(m, keyHex(key)).pipe(Option.getOrElse(() => [] as ReadonlyArray<ComputeReceiptWire>))),
        ),
    };
  });
```
