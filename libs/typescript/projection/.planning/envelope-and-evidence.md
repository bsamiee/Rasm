# [PROJECTION_ENVELOPE_AND_EVIDENCE]

One page owns the receipt, evidence, and availability folds, the envelope carrier that binds the payload type for every structured-text receipt, and the HLC skew-band correlation fold elevated to a first-class confidence-interval projection output. The three folds are rows of the same `keyedFold` algebra `fold-algebra.md` owns — `ReceiptStore` and `EvidenceFeed` are split from the five stream stores only by payload, never by altitude, and `AvailabilityStore` is identical in kind, a pure read-side fold the `@rasm/interchange` `CommandGateway` reads across the package boundary. The folds are platform-neutral projections of the wire vocabulary, transport-free, depending only on the `@rasm/interchange` decoded shapes.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]              | [OWNS]                                                    |
| :-----: | :--------------------- | :------------------------------------------------------- |
|   [1]   | ENVELOPE_AND_EVIDENCE  | the receipt/evidence/availability folds + the envelope carrier + the SkewBand confidence-interval projection |

## [2]-[ENVELOPE_AND_EVIDENCE]

- Owner: `ReceiptStore` for the compute-receipt fold, `EvidenceFeed` for the HLC-ordered evidence fold, `AvailabilityStore` for the command-availability read gate, `ReceiptEnvelopeCarrier` for the payload-bound envelope shape, and `SkewBand` for the HLC skew-band correlation fold projected as a confidence interval. Each store is one `keyedFold`/`scan` row of the `fold-algebra.md` algebra, never a parallel store.
- Cases: `ReceiptStore` folds the compute-receipt union against `receipts-and-benchmarks.md#TS_PROJECTION`; `EvidenceFeed` orders rows by the HLC stamp with skew bands against `diagnostics-evidence.md#TS_PROJECTION`, decoding embedded geometry through the `@rasm/interchange` `GeometryRail`; `AvailabilityStore` folds availability rows against `commands-availability.md#TS_PROJECTION` into an immutable keyed map the `CommandGateway` reads as a dial-time gate so a disabled command never fires, the degradation level key an input to availability never re-derived; the envelope carrier resolves the package marker, the HLC-stamp shape, and the receipt-envelope carrier against `runtime-ports.md#TS_PROJECTION` and feeds both the runtime and evidence folds; `SkewBand` folds the correlation-keyed `SkewBandWire` earliest/latest pair into a `[lower, upper]` confidence interval so a browser dashboard renders "within +/-N ms confidence" without recomputing the HLC fold — the one place the web domain surfaces distributed-systems clock-uncertainty as product UI, a capability the C# AppUi structurally does not own.
- Entry: every structured-text receipt payload binds as the envelope payload type with the envelope discriminant mirroring the payload discriminant; the evidence timeline carries envelopes whole; each payload decodes against its owning package contract; cluster-11 geometry rides inside the envelope payload rather than on the evidence row shape; `AvailabilityStore` exposes a read interface the `@rasm/interchange` `CommandGateway` consumes as a gate at dial time — the gate value is read across the package boundary, the gateway never re-mints availability, and co-location follows altitude not read-dependency so the dialing gateway lives in `@rasm/interchange`.
- Packages: `effect` for the fold, ordering, and `SubscriptionRef` primitives; the `@effect-atom/atom` cell bridge is consumed at the `@rasm/web` boundary, never imported here.
- Growth: a new receipt payload lands as one payload row bound through the envelope, zero new carrier; a new evidence kind lands as one fold arm; a new gating input lands as one availability-row field; a new confidence projection lands as one `SkewBand` derivation arm.
- Boundary: the envelope shape is never re-aggregated branch-side; the `runtime-ports.md#WIRE_LAW` anchor is read for the HLC-stamp ordering discipline only, never for token grounding; the HLC logical counter resets on every physical advance so it never approaches the JSON number envelope; the domain dials no transport, imports no `@connectrpc/*`, and embedded geometry decodes through the `@rasm/interchange` rail, never a second decode here; `AvailabilityStore` is a fold in the same altitude as the five stream stores and stays transport-free.

```ts contract
const ReceiptEnvelopeCarrier = <Payload extends Schema.Schema.Any>(payload: Payload) =>
  Schema.Struct({
    package: RasmPackage,
    kind: Schema.String,
    hlc: HlcStampWire,
    tenant: TenantContextWire,
    correlation: Schema.String,
    payload,
  });
type ReceiptEnvelopeCarrier<Payload extends Schema.Schema.Any> = Schema.Schema.Type<ReturnType<typeof ReceiptEnvelopeCarrier<Payload>>>;

const EvidenceEnvelope = ReceiptEnvelopeCarrier(EvidenceRowWire);
const decodeEvidence = Schema.decodeUnknown(Schema.Array(EvidenceEnvelope));

interface EvidenceFeed {
  readonly ordered: SubscriptionRef.SubscriptionRef<ReadonlyArray<Schema.Schema.Type<typeof EvidenceEnvelope>>>;
}

interface ReceiptStore {
  readonly rows: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, ComputeReceiptWire>>;
}

interface AvailabilityStore {
  readonly rows: SubscriptionRef.SubscriptionRef<ReadonlyMap<string, CommandAvailabilityWire>>;
  readonly isEnabled: (intentKey: string) => Effect.Effect<boolean>;
}

const SkewBandWire = Schema.Struct({ earliest: Schema.String, latest: Schema.String });
type SkewBandWire = Schema.Schema.Type<typeof SkewBandWire>;

const skewInterval = (band: SkewBandWire): readonly [number, number] => {
  const lo = Date.parse(band.earliest);
  const hi = Date.parse(band.latest);
  const mid = (lo + hi) / 2;
  return [mid - (hi - lo) / 2, mid + (hi - lo) / 2] as const;
};
```
