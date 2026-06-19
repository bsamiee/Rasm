# [PROJECTION_CORRELATION]

The receipt and evidence projection — one `EvidenceProjection` store binding the compute-receipt keyed fold, the HLC-ordered evidence fold, and content-keyed evidence correlation that keys and dedups evidence rows on the `interchange`-assembled `ContentKey`. Reassembly lives in `interchange/Codec/frame#FRAME_RAIL` (`ArtifactFrameRail`: per-frame Crc32 + pre-sized `Uint8Array` sink + whole-artifact `xxhash-wasm` content-key derivation, off-main-thread in `platform`'s `DecodeWorkerPool`). This fold consumes the already-assembled `ArtifactBlob` and 16-byte `ContentKey` and correlates evidence by digest — a re-fetch of identical content hits the cached row — and never re-derives the hash. A second hash mint here is the named cross-language drift defect. The folds are rows of the same `combinators#KEYED_FOLD` algebra, split from the stream stores only by payload.

## [1]-[INDEX]

- [1]-[EVIDENCE_CORRELATION]: Owns `EvidenceProjection`, its receipt and evidence folds, and the content-keyed `byContentKey` correlation lookup.

## [2]-[EVIDENCE_CORRELATION]

- Owner: `EvidenceProjection`, the one store binding the compute-receipt keyed fold, the HLC-ordered evidence index, and the content-keyed correlation, forked into one `Scope` by `evidenceProjection`. The receipt fold keys on the `convergence/merge#LWW_MERGE` `keyHex` transcription so a cache hit by digest dedups without re-decoding; `byContentKey` is the derived lookup off that map, never a parallel query surface; `byHlc` is the lexicographic HLC `Order` the evidence timeline keys by, one composed ordering value rather than an inline comparator per sort. The timeline is not a re-sorted array — it is an `effect` `RedBlackTree` keyed by the composed HLC `Order`, so an arrived row is one O(log n) `RedBlackTree.insert` and the since-T window is a `RedBlackTree.greaterThanEqual` cursor walk over the ordered structure, replacing the `[...rows, e].sort(byHlc)` whole-array re-sort that paid O(n log n) per arrived row — the verified C# floor anti-pattern the read-model tier is defined to avoid.
- Cases: the receipt fold folds the compute-receipt union against `csharp:Rasm.Compute/Runtime/receipts#TS_PROJECTION`, grouped by the `ContentKey` hex so every receipt referencing one assembled `ArtifactBlob` shares one correlation cell; the evidence index inserts each row into the `RedBlackTree` keyed by its HLC stamp against `csharp:Rasm.AppUi/Render/evidence#TS_PROJECTION`, decoding embedded geometry through the `interchange` `GeometryRail`; the ordered read is the in-order `RedBlackTree.values` traversal, and `since` is the `greaterThanEqual` cursor walk yielding only the window above a mark so the timeline tail reads bounded-cost regardless of session length; the correlation hex is the convergence identity `merge#LWW_MERGE` settles, read here for grouping, never re-minted. The HLC `Order` keys the tree, so two rows sharing a `(physical, logical)` stamp disambiguate on the `ContentKey` hex appended to the key, keeping the order total and the insert idempotent under reconnect-replay — the persistent tree shares structure under insert so a replayed row re-inserts to the identical node.
- Packages: `effect` for `Schema`, `Stream`, `SubscriptionRef`, `Effect`, `Scope`, `HashMap`, `Option`, `Order`, and `RedBlackTree` (the `insert`/`greaterThanEqual`/`values`/`first`/`last` ordered-index owner the timeline since-T window reads, the branch `.api/effect.md` ordered-collection coverage seam its spelling rides).
- Growth: a new receipt payload lands as one payload row bound through `envelope#RECEIPT_ENVELOPE`, zero new carrier; a new evidence kind lands as one fold arm; the `byContentKey` lookup and the `RedBlackTree` index never grow.
- Boundary: cluster geometry rides inside the envelope payload, not on the evidence row shape; embedded geometry decodes through the `interchange` rail, never a second decode here; the `ContentKey` arrives assembled from `interchange/Codec/frame#FRAME_RAIL` and is never re-minted; both folds pipe through `policy#STREAM_POLICY`; the ordered index is the persistent `RedBlackTree` keyed by the composed HLC `Order` so the timeline never re-sorts an array and the since-T read is an ordered cursor walk; the domain dials no transport.

```ts contract
import { Effect, HashMap, Option, Order, RedBlackTree, Scope, Schema, Stream, SubscriptionRef } from "effect";
import type { ComputeReceiptWire, ContentKey, EvidenceRowWire } from "@rasm/interchange";
import { foldStream, keyedFold } from "../fold/combinators";
import type { StreamPolicy } from "../fold/policy";
import { ReceiptEnvelopeCarrier } from "../evidence/envelope";
import { keyHex } from "../convergence/merge";

const EvidenceEnvelope = ReceiptEnvelopeCarrier(EvidenceRowWire);
type EvidenceEnvelope = Schema.Schema.Type<typeof EvidenceEnvelope>;

interface HlcKey {
  readonly physicalMs: number;
  readonly logical: bigint;
  readonly digest: string;
}

const hlcKeyOf = (e: EvidenceEnvelope): HlcKey => ({
  physicalMs: Date.parse(e.hlc.physical),
  logical: e.hlc.logical,
  digest: keyHex(e.contentKey),
});

const HlcOrder: Order.Order<HlcKey> = Order.combineAll([
  Order.mapInput(Order.number, (k: HlcKey) => k.physicalMs),
  Order.mapInput(Order.bigint, (k: HlcKey) => k.logical),
  Order.mapInput(Order.string, (k: HlcKey) => k.digest),
]);

interface EvidenceProjection {
  readonly receipts: SubscriptionRef.SubscriptionRef<HashMap.HashMap<string, ReadonlyArray<ComputeReceiptWire>>>;
  readonly timeline: SubscriptionRef.SubscriptionRef<RedBlackTree.RedBlackTree<HlcKey, EvidenceEnvelope>>;
  readonly byContentKey: (key: ContentKey) => Effect.Effect<ReadonlyArray<ComputeReceiptWire>>;
  readonly since: (markMs: number) => Effect.Effect<ReadonlyArray<EvidenceEnvelope>>;
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
    const timeline = yield* foldStream(
      evidenceFeed,
      RedBlackTree.empty<HlcKey, EvidenceEnvelope>(HlcOrder),
      (tree, e) => RedBlackTree.insert(tree, hlcKeyOf(e), e),
      policy,
    );
    return {
      receipts,
      timeline,
      byContentKey: (key) =>
        SubscriptionRef.get(receipts).pipe(
          Effect.map((m) => HashMap.get(m, keyHex(key)).pipe(Option.getOrElse(() => [] as ReadonlyArray<ComputeReceiptWire>))),
        ),
      since: (markMs) =>
        SubscriptionRef.get(timeline).pipe(
          Effect.map((tree) =>
            Array.from(RedBlackTree.greaterThanEqual(tree, { physicalMs: markMs, logical: 0n, digest: "" }), ([, e]) => e)),
        ),
    };
  });
```

The `foldStream` accumulator is the persistent `RedBlackTree` itself, so the make-fork-update scaffold from `combinators#KEYED_FOLD` carries unchanged; `RedBlackTree.insert` returns a structurally-shared tree so the held `SubscriptionRef` swaps to the new root in O(log n) and a reconnect-replay re-inserts to the identical node, never re-sorting the prefix. The ordered read is `RedBlackTree.values` for the whole timeline and `greaterThanEqual` for the bounded since-T tail.
