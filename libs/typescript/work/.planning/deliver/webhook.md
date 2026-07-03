# [WORK_WEBHOOK]

Signed webhook egress is one durable attempt shape: the envelope encodes exactly once into the bytes that are signed, the signature is the `security` HMAC over those held bytes under the endpoint's `Redacted` key, the dispatch dials the branch `batch` client lane, and every outcome lands as either a receipt — the delivery evidence a workflow persists — or a fault whose reason row carries the kernel class and the suppression column. Sign-then-send over held bytes is the byte-identity law applied to egress: the wire body and the signed body cannot diverge because they are one value, and a re-encode between signing and sending is the named defect. Durability comes from composition, not machinery: `Hook.deliver` is the raw attempt, and `Step.run` wraps it under the `bulk` budget so redelivery, backoff, and the whole-delivery deadline are the standing kernel rows. Suppression is an evidence fold — consecutive terminal rejections trip the endpoint's suppress verdict in the caller's ledger — never a silent drop. A raw `fetch`, an unsigned emergency path, a secret outside `Redacted`, and a receipt minted before the response settles are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]      | [OWNS]                                                                        |
| :-----: | :------------- | :------------------------------------------------------------------------------ |
|  [01]   | [SIGNED_SEND]  | the envelope, the scheme row, the signed dispatch, receipt and fault families    |
|  [02]   | [DELIVERY_JOB] | the durable composition, suppression fold, callback-gate resolution              |

## [2]-[SIGNED_SEND]

[SIGNED_SEND]:
- Owner: `Hook` — the signed-egress owner; one export carries the envelope constructor, the receipt constructor, and the attempt, with every companion type on the merged hub. `Hook.Envelope` is the one wire shape: the event id, the event kind, the emission instant, and the payload held as the app-encoded JSON string — an opaque band this page signs and forwards verbatim, never re-parses. The interior codec is `Schema.parseJson` over the envelope — encode yields the exact body string, its UTF-8 bytes are what `Crypto.sign` MACs and what the request carries via `HttpBody.uint8Array`, so signature and body are projections of one value. The scheme row fixes the header vocabulary once: `webhook-id`, `webhook-timestamp`, `webhook-signature` with the `v1,` prefix.
- Law: the endpoint is a typed row — `Hook.Endpoint` carries the URL, the `Redacted` signing key, and the owning `TenantContext` — and rows arrive as app-passed values from the caller's own registry; this page never stores endpoints, and the key unwraps only inside the `Crypto.sign` call.
- Law: the fault family follows the folder convention — reasons `encode` (the envelope refused its own schema; `invalid`), `signing` (the crypto primitive failed; `defect`), `refused` (transport never got a response; `unavailable`, the re-drive arm), `rejected` (the endpoint answered non-2xx; `invalid`, the suppression-counting arm), `lapsed` (the lane budget expired; `expired`) — each row carrying `class` and `suppress`, projected by `get policy()`/`get class()` so kernel budget gates re-drive exactly `refused` and `lapsed`.
- Law: the receipt is settled evidence — it stamps the envelope id, the endpoint URL, the attempt ordinal read from `Activity.CurrentAttempt`, the settlement instant, and the elapsed span measured on the rail; it is constructed only after the response settles, so a receipt existing proves delivery happened.
- Law: dispatch rides the branch lane — `Client.dial("batch", request)` inherits status admission, transient transport retry, redirect ceiling, and trace propagation from the host policy row; the response modality closes its scope immediately because a webhook consumes status, not body, and the three dial fault tags map onto reason rows in one `catchTags` record.
- Boundary: HMAC mechanics and key custody are `security`'s `Crypto`; the lane table is `host/net`'s; the inbound half — verifying a foreign webhook — is `edge/hook`'s, and the two never share a page.
- Entry: `Hook.deliver(endpoint, envelope)` — the single attempt; its fault union and requirement set are the annotation.
- Growth: a new scheme version is one `_SCHEME` field; a new refusal cause is one policy row.
- Packages: `@effect/platform` (`HttpBody`, `HttpClient`, `HttpClientRequest`), `@effect/workflow` (`Activity`), `effect` (`DateTime`, `Duration`, `Effect`, `Redacted`, `Schema`), `@rasm/ts/host` (`Client`), `@rasm/ts/kernel` (`FaultClass`, `TenantContext`), `@rasm/ts/security` (`Crypto`).

```typescript
import { HttpBody, type HttpClient, HttpClientRequest } from "@effect/platform"
import { Activity } from "@effect/workflow"
import { Client } from "@rasm/ts/host"
import type { FaultClass, TenantContext } from "@rasm/ts/kernel"
import { Crypto } from "@rasm/ts/security"
import { DateTime, Duration, Effect, type Redacted, Schema, type Types } from "effect"

const _SCHEME = { id: "webhook-id", at: "webhook-timestamp", sign: "webhook-signature", version: "v1" } as const

const _reasons = ["encode", "signing", "refused", "rejected", "lapsed"] as const
const _policy = {
  encode: { class: "invalid", suppress: false },
  signing: { class: "defect", suppress: false },
  refused: { class: "unavailable", suppress: false },
  rejected: { class: "invalid", suppress: true },
  lapsed: { class: "expired", suppress: false },
} as const

class _Envelope extends Schema.Class<_Envelope>("HookEnvelope")({
  id: Schema.UUID,
  kind: Schema.NonEmptyString,
  at: Schema.DateTimeUtc,
  payload: Schema.String,
}) {}

class _Fault extends Schema.TaggedError<_Fault>()("HookFault", {
  reason: Schema.Literal(..._reasons),
  endpoint: Schema.String,
  id: Schema.UUID,
  detail: Schema.String,
}) {
  get policy(): (typeof _policy)[_Fault["reason"]] {
    return _policy[this.reason]
  }
  get class(): FaultClass.Kind {
    return _policy[this.reason].class
  }
  override get message(): string {
    return `<hook:${this.reason}> ${this.endpoint} ${this.detail}`
  }
}

class _Receipt extends Schema.Class<_Receipt>("HookReceipt")({
  id: Schema.UUID,
  endpoint: Schema.String,
  attempt: Schema.Int,
  at: Schema.DateTimeUtc,
  elapsedMillis: Schema.NonNegative,
}) {}

declare namespace Hook {
  type Envelope = _Envelope
  type Fault = _Fault
  type Receipt = _Receipt
  type Reason = keyof typeof _policy
  type Row = { readonly class: FaultClass.Kind; readonly suppress: boolean }
  type Endpoint = {
    readonly url: string
    readonly key: Redacted.Redacted<Uint8Array>
    readonly tenant: TenantContext
  }
  type Shape = Types.Simplify<{
    readonly Envelope: typeof _Envelope
    readonly Receipt: typeof _Receipt
    readonly deliver: typeof _deliver
  }>
  type _Rows<T extends Record<Reason, Row> = typeof _policy> = T
  type _Keys<K extends keyof typeof _policy = Reason> = K
}

const _utf8 = new TextEncoder()
const _encoded = Schema.encode(Schema.parseJson(_Envelope))

const _faulted = (reason: Hook.Reason, endpoint: Hook.Endpoint, envelope: _Envelope) => (cause: unknown): _Fault =>
  new _Fault({ reason, endpoint: endpoint.url, id: envelope.id, detail: String(cause) })

const _deliver = (
  endpoint: Hook.Endpoint,
  envelope: _Envelope,
): Effect.Effect<_Receipt, _Fault, Crypto | HttpClient.HttpClient> =>
  Effect.gen(function* () {
    const body = yield* Effect.mapError(_encoded(envelope), _faulted("encode", endpoint, envelope))
    const bytes = _utf8.encode(body)
    const signature = yield* Effect.mapError(Crypto.sign(endpoint.key, bytes), _faulted("signing", endpoint, envelope))
    const opened = yield* DateTime.now
    yield* Effect.scoped(
      Client.dial(
        "batch",
        HttpClientRequest.post(endpoint.url).pipe(
          HttpClientRequest.setHeader(_SCHEME.id, envelope.id),
          HttpClientRequest.setHeader(_SCHEME.at, DateTime.formatIso(envelope.at)),
          HttpClientRequest.setHeader(_SCHEME.sign, `${_SCHEME.version},${signature}`),
          HttpClientRequest.setBody(HttpBody.uint8Array(bytes, "application/json")),
        ),
      ),
    ).pipe(
      Effect.catchTags({
        Lapse: _faulted("lapsed", endpoint, envelope),
        RequestError: _faulted("refused", endpoint, envelope),
        ResponseError: _faulted("rejected", endpoint, envelope),
      }),
    )
    const settled = yield* DateTime.now
    return new _Receipt({
      id: envelope.id,
      endpoint: endpoint.url,
      attempt: yield* Activity.CurrentAttempt,
      at: settled,
      elapsedMillis: Duration.toMillis(DateTime.distanceDuration(opened, settled)),
    })
  })

const Hook: Hook.Shape = { Envelope: _Envelope, Receipt: _Receipt, deliver: _deliver }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Hook }
```

## [3]-[DELIVERY_JOB]

[DELIVERY_JOB]:
- Owner: the durable composition law. A webhook delivery inside a workflow is `Step.run({ name, budget: "bulk", execute: Hook.deliver(endpoint, envelope) })` — redelivery cadence, jitter, the attempt bound, and the total window are the kernel `bulk` row; only `refused` and `lapsed` re-drive because the class column gates the schedule structurally; the receipt is the step's durable exit, so a replay after a crash returns the recorded receipt instead of re-sending.
- Law: suppression is a fold over evidence — the caller's endpoint ledger counts consecutive faults whose row says `suppress: true` and trips the endpoint's disabled verdict at its own threshold; delivery never mutates the registry it was handed, and a suppressed endpoint is the caller declining to enqueue, not this page dropping silently.
- Law: an acknowledging callback resolves a flow gate — when a delivery contract includes an out-of-band ack, the workflow awaits `Flow.gate` and the inbound signed callback (verified at `edge/hook`) resolves the gate's token; this page owns only the outbound half.
- Law: fan-out belongs to the relay — bulk delivery of one event to many endpoints drains through `deliver/relay.md`'s claim loop with `Hook.deliver` as the webhook channel handler; a hand `forEach` over endpoints inside a workflow forfeits the per-tenant egress quota.
- Boundary: `Step` geometry is `flow/activity.md`'s; the gate is `flow/durable.md`'s; the relay's quota and claim discipline are `deliver/relay.md`'s.
- Growth: a new delivery contract (an ack window, a mutual-TLS row) is one field on `Endpoint` consumed inside `deliver`.
