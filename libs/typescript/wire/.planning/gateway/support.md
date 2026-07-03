# [WIRE_SUPPORT]

`gateway/support.ts` is the support-capture verb: `SupportCaptureWire` from `Rasm.AppHost` — a user- or host-initiated support report carrying its kind, note, environment fingerprint, and an opaque evidence band — decodes once and delivers to the `SupportIntake` port this module declares. Telemetry's crash plane consumes the intake at the app root (`telemetry` never imports `wire`; the app root projects telemetry's crash service into the port), so support reports join the same crash-evidence spine as runtime faults while the two folders stay ledger-clean.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]         | [OWNS]                                                         |
| :-----: | :---------------- | :--------------------------------------------------------------------- |
|   [1]   | `SUPPORT_CAPTURE` | the capture owner, the kind vocabulary, the intake port, the verb        |

## [2]-[SUPPORT_CAPTURE]

- Owner: `SupportCapture` — the decoded report class over the closed `kind` vocabulary (`crash`, `bug`, `feedback`), with the evidence band held opaque and the environment fingerprint typed; `SupportIntake`, the `Context.Tag` port whose one member accepts a decoded report and answers with the intake receipt.
- Entry: `SupportCapture.captured(octets)` — decode plus delivery in one rail: `Effect<SupportReceipt, ParseError, SupportIntake>`; the app root satisfies the port by projecting telemetry's crash intake (`Layer.project` at composition), and a headless deployment satisfies it with a journal-backed sink.
- Receipt: `SupportReceipt` — the intake's acknowledgment: the assigned reference, the kind, the intake instant — the value the shell surfaces to the reporting user so a support report is never fire-and-forget.
- Growth: a new report kind is one literal row; a new evidence axis (an attached log window, a session replay coordinate) is one field mirroring the C# emit — the band stays opaque and the port signature is untouched.
- Law: the evidence band is opaque carriage — crash dumps, log windows, and replay fragments cross as held octets; interpretation belongs to the intake's consumer (telemetry redaction and retention policy run there), and this rail never opens the band.
- Law: the port is wire-declared, consumer-satisfied — one Tag key, one owning declaration here; telemetry's crash service is projected into it at the app root, so the `telemetry -> wire` and `wire -> telemetry` edges both stay structurally absent.
- Law: the kind vocabulary is closed at the seam — the C# capture verbs and this literal row set move together; a free-string kind is the rejected shape.
- Boundary: crash reconstruction from wire faults is `fault/detail.ts`'s enricher lane — a support CAPTURE may reference a fault but never re-reconstructs one; redaction, retention, and triage are telemetry's pages.

```typescript
import { Context, Effect, type ParseResult, Schema } from "effect"
import { ProtoCodec } from "../codec/proto.ts"

const _kinds = ["crash", "bug", "feedback"] as const

class SupportCapture extends Schema.Class<SupportCapture>("SupportCapture")({
  kind: Schema.Literal(..._kinds),
  note: Schema.NonEmptyString,
  fingerprint: Schema.NonEmptyString,
  evidence: Schema.Uint8ArrayFromSelf,
  at: Schema.DateTimeUtc,
}) {
  static readonly FromBytes: Schema.Schema<SupportCapture, Uint8Array> = ProtoCodec.family(ProtoCodec.suite.SupportCaptureWire, SupportCapture)
  static readonly captured = (octets: Uint8Array): Effect.Effect<SupportReceipt, ParseResult.ParseError, SupportIntake> =>
    Schema.decodeUnknown(SupportCapture.FromBytes)(octets).pipe(
      Effect.flatMap((report) => Effect.flatMap(SupportIntake, (intake) => intake.deliver(report))),
    )
}

declare namespace SupportCapture {
  type Kind = (typeof _kinds)[number]
}

class SupportReceipt extends Schema.Class<SupportReceipt>("SupportReceipt")({
  reference: Schema.NonEmptyString,
  kind: Schema.Literal(..._kinds),
  at: Schema.DateTimeUtc,
}) {}

class SupportIntake extends Context.Tag("wire/SupportIntake")<SupportIntake, {
  readonly deliver: (report: SupportCapture) => Effect.Effect<SupportReceipt>
}>() {}

// --- [EXPORTS] --------------------------------------------------------------------------

export { SupportCapture, SupportIntake }
```
