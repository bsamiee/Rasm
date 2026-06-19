# [PROJECTION_ENVELOPE]

`ReceiptEnvelopeCarrier` is the one `Schema` factory binding every structured-text receipt payload as a single envelope — the package marker, the HLC stamp, the tenant, the correlation key, and the payload — feeding both the runtime fold and the `evidence#EVIDENCE_CORRELATION` fold. Every structured-text receipt payload binds through this carrier with the envelope discriminant mirroring the payload discriminant; a second branch-side aggregation of the envelope shape is the deleted form.

## [1]-[INDEX]

- [1]-[RECEIPT_ENVELOPE]: Owns the `ReceiptEnvelopeCarrier` payload-bound `Schema` factory.

## [2]-[RECEIPT_ENVELOPE]

- Owner: `ReceiptEnvelopeCarrier`, the generic `Schema.Struct` factory that resolves the package marker, the HLC-stamp shape, the tenant context, the correlation key, and the bound payload against `csharp:Rasm.AppHost/Runtime/ports#TS_PROJECTION`.
- Cases: every structured-text receipt payload binds as the envelope payload type; the evidence timeline carries envelopes whole; each payload decodes against its owning package contract.
- Packages: `effect` for `Schema`.
- Growth: a new receipt payload lands as one payload type bound through the carrier, zero new carrier.
- Boundary: the envelope shape is never re-aggregated branch-side; the `csharp:Rasm.AppHost/Runtime/ports#WIRE_LAW` anchor grounds the HLC-stamp ordering discipline only, never token authoring; the HLC logical counter resets on every physical advance so it never approaches the JSON number envelope; the domain dials no transport.

```ts contract
import { Schema } from "effect";
import { HlcStampWire, RasmPackage, TenantContextWire } from "@rasm/interchange";

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
```
