# [RUNTIME_DELIVER]

Outbound delivery as ONE owner: mail and webhook egress are channel rows of one dispatch table, sharing one settlement receipt, one reason-discriminated fault family, and one suppression fold — the formerly twice-owned transport convention spelled once — and the transactional-outbox relay is the cluster singleton that drains every channel under the queue page's verdict vocabulary, so retry, redelivery, parking, and replay never re-appear here as channel-local machinery. A channel owns exactly two things: how its payload becomes a wire transmission and how its transport's evidence folds into the shared receipt; everything around the transmission — claim lease, urgency order, park ceiling, tenant egress quota, backfill replay — arrives settled from `queue#LANE_POLICY` and `queue#THROTTLE`. Signing rides the security wave: a webhook body is signed byte-identical by the `Crypto` service and the mail plane signs DKIM natively in-transport — two signature domains that never merge. Suppression is evidence on the record of truth: a hard bounce or a gone endpoint appends a fact row, and every send checks the suppression projection before the wire, so the ledger is history, never a mutable blocklist table. The module ships on the `./server` exports subpath as `runtime/src/work/deliver.ts`.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                          | [PUBLIC]        |
| :-----: | :--------------- | :--------------------------------------------------------------------------------- | :-------------- |
|  [01]   | `CHANNEL_FAMILY` | the channel dispatch table, the shared receipt, the one delivery fault family       | `Deliver`       |
|  [02]   | `MAIL_ROW`       | the scoped transporter, the one message shape, auth/DKIM/DSN policy, send evidence  | `Mailer`        |
|  [03]   | `HOOK_ROW`       | byte-identity signed webhook egress and its settlement fold                         | `Hook`          |
|  [04]   | `SUPPRESSION`    | the shared suppress-by-evidence fold and the pre-send check                         | `Deliver`       |
|  [05]   | `RELAY`          | the singleton outbox drain — claim, quota, dispatch, verdict, wake, pacing          | `Relay`         |

## [2]-[CHANNEL_FAMILY]

[CHANNEL_FAMILY]:
- Owner: `Deliver` — the channel dispatch table and the two shapes every channel speaks. `Deliver.Receipt` is the settlement evidence: channel kind, transmission identity (the SMTP `messageId`, the webhook delivery id), per-recipient acceptance splits, the wire instant, and the transport's timing band — one `Schema.Class` whose fields serve both channels because settlement IS the same concept. `DeliverFault` is the one fault family: a reason row (`dial | refused | bounced | timeout | schema`) carrying its `FaultClass` kind, so the lane's judge fold reads retryability off the class table and a channel never declares a private fault rail.
- Law: a channel is a row — `{ kind, transmit, settle }` where `transmit` turns a decoded payload into wire evidence and `settle` folds that evidence into `Receipt | DeliverFault`; the relay dispatches on the deliverable's channel kind through this table and a new channel (push, SMS, chat) is one row, never a sibling drain.
- Law: partial acceptance is a receipt, not a fault — a send where some recipients accept and some reject settles as a `Receipt` whose rejected band is non-empty; the suppression fold consumes the rejected band, and only a transmission that produced no acceptance at all folds to `DeliverFault`.
- Law: the payload is decoded once at the channel seam — each channel declares its payload `Schema` and the relay's claim body decodes against it before `transmit`; a decode failure is a `schema`-reasoned fault whose `invalid` class parks immediately through the lane's poison short-circuit.
- Growth: a new channel is one table row plus its payload schema; a new settlement dimension is one `Receipt` field both channels populate.
- Packages: `effect` (`Schema`, `Data`, `DateTime`, `Duration`, `Match`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { Config, Data, DateTime, Duration, Effect, Redacted, Schedule, Schema } from "effect"
import { HttpBody, HttpClientRequest } from "@effect/platform"
import { SqlClient } from "@effect/sql"
import { createTransport, type SentMessageInfo, type Transporter } from "nodemailer"
import { Fact, Journal } from "@rasm/ts/data"
import { Crypto } from "@rasm/ts/security"
import { type AppIdentity, Budget, FaultClass } from "@rasm/ts/core"
import { Client } from "../net/client.ts"
import { Setting } from "../proc/config.ts"
import { Grid, WorkClass } from "./entity.ts"
import { Lane, LaneVerdict, Throttle } from "./queue.ts"

class Receipt extends Schema.Class<Receipt>("DeliverReceipt")({
  channel: Schema.Literal("mail", "webhook"),
  transmission: Schema.NonEmptyString,
  accepted: Schema.Array(Schema.String),
  rejected: Schema.Array(Schema.Struct({ recipient: Schema.String, note: Schema.String })),
  at: Schema.DateTimeUtc,
  wire: Schema.Duration,
}) {}

const _reasons = {
  dial: "unavailable",
  refused: "denied",
  bounced: "invalid",
  timeout: "expired",
  schema: "invalid",
} as const satisfies { readonly [reason: string]: FaultClass.Kind }

class DeliverFault extends Data.TaggedError("DeliverFault")<{
  readonly reason: keyof typeof _reasons
  readonly channel: "mail" | "webhook"
  readonly detail: string
}> {
  get class(): FaultClass.Kind {
    return _reasons[this.reason]
  }
}

declare namespace Deliver {
  type Channel<A, I, R> = {
    readonly kind: Receipt["channel"]
    readonly payload: Schema.Schema<A, I>
    readonly transmit: (payload: A) => Effect.Effect<Receipt, DeliverFault, R>
  }
}
```

## [3]-[MAIL_ROW]

[MAIL_ROW]:
- Owner: `Mailer` — the scoped mail-egress service. Construction is one polymorphic `createTransport` whose option shape is the environment's transport policy row resolved from `Setting` — pooled SMTP (`pool: true` with `maxConnections`/`maxMessages`/`rateLimit`), a provider name through `wellKnown`, or the `streamTransport`/`jsonTransport` inspect sinks for kit-driven specs and dry runs — built inside `Layer.scoped` with `verify()` proving the credential at construction and `close()` as the finalizer draining pool sockets. Authentication is the discriminated union as data: `LOGIN` credentials, `OAUTH2` riding the built-in `XOAuth2` refresh flow with the `token` event bridged into a `Redacted` ref, every secret (`pass`, `clientSecret`, `refreshToken`, the DKIM `privateKey`) arriving as `Config.redacted` and unwrapped only at the transport call.
- Law: the message is one Schema — addresses, subject, body alternatives, attachments (the `report` page's bytes as `{ content, contentType }`), the `list` block building `List-Unsubscribe`, the `dkim` block, and `dsn` delivery-status requests are fields of the channel payload decoded once; an untyped message object assembled at a call site is unspellable.
- Law: DKIM is native and mandatory on production rows — `domainName`/`keySelector`/`privateKey` ride the transport options so every message signs RFC-6376 in-transport; the security wave's HMAC domain never touches mail.
- Law: transport faults classify through the code table — `EAUTH` folds `refused` (terminal), a 4xx `responseCode` folds `dial` (transient — the lane's lease redelivers), a 5xx recipient failure folds `bounced` (the suppression fold consumes it); string-matching an error message is unspellable beside the table.
- Law: `isIdle()` and the `idle` event are the relay's pacing signal — the drain claims a batch only when the pool reports capacity, bridging the event to the wake race so bulk sends respect pool geometry instead of queueing in the transport.
- Receipt: `SentMessageInfo` folds to the shared `Receipt` — `accepted`/`rejected`/`rejectedErrors` become the acceptance bands, `messageId` the transmission identity, `envelopeTime` the wire band.
- Growth: a new transport environment is one policy row; a new message concern (iCal invite, AMP body) is one payload field the schema admits.
- Packages: `nodemailer` (`createTransport`, `Transporter`, `SentMessageInfo`); `effect` (`Layer`, `Config`, `Redacted`); `../proc/config.ts` (`Setting`).

```typescript
class Mailer extends Effect.Service<Mailer>()("runtime/Mailer", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const pass = yield* Config.redacted("SMTP_PASS")
    const dkim = yield* Config.redacted("DKIM_KEY")
    const transporter: Transporter = yield* Effect.acquireRelease(
      Effect.sync(() =>
        createTransport({
          pool: true,
          host: setting.mail.host,
          port: setting.mail.port,
          secure: true,
          maxConnections: WorkClass.bulk.concurrency,
          rateLimit: setting.mail.rate,
          auth: { user: setting.mail.user, pass: Redacted.value(pass) },
          dkim: { domainName: setting.mail.domain, keySelector: setting.mail.selector, privateKey: Redacted.value(dkim) },
        })
      ),
      (built) => Effect.sync(() => built.close()),
    )
    yield* Effect.tryPromise({
      try: () => transporter.verify(),
      catch: (cause) => new DeliverFault({ reason: "dial", channel: "mail", detail: String(cause) }),
    })
    const send = (message: Parameters<Transporter["sendMail"]>[0]) =>
      Effect.tryPromise({
        try: () => transporter.sendMail(message),
        catch: (cause) => _classified(cause),
      }).pipe(Effect.map(_mailReceipt))
    const idle = Effect.sync(() => transporter.isIdle())
    return { send, idle } as const
  }),
}) {}

const _classified = (cause: unknown): DeliverFault => {
  const code = String((cause as { readonly code?: string }).code ?? "")
  const status = Number((cause as { readonly responseCode?: number }).responseCode ?? 0)
  return new DeliverFault({
    reason: code === "EAUTH" ? "refused" : status >= 500 ? "bounced" : "dial",
    channel: "mail",
    detail: `${code}:${status}`,
  })
}

const _mailReceipt = (info: SentMessageInfo): Receipt =>
  new Receipt({
    channel: "mail",
    transmission: info.messageId,
    accepted: info.accepted.map(String),
    rejected: (info.rejectedErrors ?? []).map((err) => ({ recipient: String(err.recipient ?? ""), note: err.response ?? "" })),
    at: DateTime.unsafeNow(),
    wire: Duration.millis(info.envelopeTime ?? 0),
  })
```

## [4]-[HOOK_ROW]

[HOOK_ROW]:
- Owner: `Hook` — signed webhook egress under byte identity: the payload encodes to its wire bytes exactly once, the `Crypto` service signs THOSE bytes, and the transmission carries the v1 header triple — `webhook-id` (the deliverable identity — replay dedup on the receiving side), `webhook-timestamp` (the signing instant bounding replay windows), `webhook-signature` (`v1,<hex>` over `id.timestamp.body`) — so the receiver verifies the identical byte sequence and a re-serialization between sign and send is structurally impossible.
- Law: the HTTP leg is the branch client — `Client` default-policy rows own timeout, retry pacing, and proxy; this row adds only the signed request construction and the settlement fold: 2xx settles to `Receipt`, 410 folds `bounced` (the endpoint is gone — suppression consumes it), 429/5xx fold `dial` (the lease redelivers), a client timeout folds `timeout`.
- Law: endpoint secrets are per-destination `Redacted` material resolved through the security wave's secret rows, keyed by the destination, never a shared signing key across tenants.
- Boundary: the inbound half — verifying a foreign webhook, resolving a `flow#SIGNAL_GATE` token from a verified callback — is the serving plane's mount; this row is egress only.
- Growth: a signing-scheme revision is a new version prefix beside `v1` in the same header; a destination policy axis (mTLS, custom header band) is a field on the destination row.
- Packages: `@effect/platform` (`HttpClientRequest`, `HttpBody`); `@rasm/ts/security` (`Crypto`); `../net/client.ts` (`Client`).

```typescript
const HookPayload = Schema.Struct({
  destination: Schema.URL,
  deliverable: Schema.NonEmptyString,
  body: Schema.Uint8ArrayFromSelf,
  key: Schema.Uint8ArrayFromSelf,
})

const _signable = (id: string, stamp: string, body: Uint8Array): Uint8Array => {
  const prefix = new TextEncoder().encode(`${id}.${stamp}.`)
  const joined = new Uint8Array(prefix.length + body.length)
  joined.set(prefix)
  joined.set(body, prefix.length)
  return joined
}

const _hook = (payload: typeof HookPayload.Type) =>
  Effect.gen(function* () {
    const crypto = yield* Crypto
    const client = yield* Client
    const at = yield* DateTime.now
    const stamp = String(Math.trunc(DateTime.toEpochMillis(at) / 1000))
    const signed = yield* crypto.sign(Redacted.make(payload.key), _signable(payload.deliverable, stamp, payload.body))
    const response = yield* client.request(
      HttpClientRequest.post(payload.destination.toString()).pipe(
        HttpClientRequest.setHeaders({
          "webhook-id": payload.deliverable,
          "webhook-timestamp": stamp,
          "webhook-signature": `v1,${signed}`,
        }),
        HttpClientRequest.setBody(HttpBody.uint8Array(payload.body, "application/json")),
      ),
    )
    return yield* _hookSettle(payload, response.status, at)
  })

const _hookSettle = (payload: typeof HookPayload.Type, status: number, at: DateTime.Utc) =>
  status < 300
    ? Effect.succeed(new Receipt({ channel: "webhook", transmission: payload.deliverable, accepted: [payload.destination.toString()], rejected: [], at, wire: Duration.zero }))
    : Effect.fail(new DeliverFault({ reason: status === 410 ? "bounced" : "dial", channel: "webhook", detail: String(status) }))
```

## [5]-[SUPPRESSION]

[SUPPRESSION]:
- Owner: the shared suppress-by-evidence fold — both channels feed it and both consult it. A `bounced`-reasoned fault or a receipt's rejected band appends one `deliver.suppressed` fact row (recipient or destination as target, the channel and note as change rows, `regulatory` retention for mail — the unsubscribe evidence — and `operational` for webhooks); `Deliver.admissible(recipient)` reads the suppression projection the data wave serves and answers before any `transmit`, so a suppressed destination never reaches the wire and the check is one read, not a channel-local list.
- Law: suppression is append-only history — reinstatement is a `deliver.reinstated` fact, and the projection folds the pair; deleting suppression evidence is unrepresentable.
- Law: the unsubscribe seam is one-way — the serving plane's unsubscribe endpoint appends the same fact shape; this fold never mounts a route.
- Growth: a suppression cause (complaint feedback loop, manual block) is one action verb on the same fact shape.
- Packages: `@rasm/ts/data` (`Fact`); `effect` (`Effect`, `Stream`).

```typescript
const _suppress = (channel: Receipt["channel"], target: string, note: string) =>
  Fact.record({
    action: "deliver.suppressed",
    actor: { key: "deliver", kind: "service" },
    change: [
      { _tag: "Assigned", path: "/channel", next: channel },
      { _tag: "Assigned", path: "/note", next: note },
    ],
    retention: channel === "mail" ? "regulatory" : "operational",
    target: { key: target, kind: "destination" },
  })

const _settled = (receipt: Receipt) =>
  Effect.forEach(receipt.rejected, (row) => _suppress(receipt.channel, row.recipient, row.note), { discard: true })
```

## [6]-[RELAY]

[RELAY]:
- Owner: `Relay` — the one outbox drain: a `Grid.singleton` (exactly one live instance cluster-wide, migrating on rebalance) that races the journal's NOTIFY wake against the lease-width tick, claims a batch through `Journal.claimBatch` sized and leased by the `bulk` class row, decodes each claim against its channel's payload schema, spends `Throttle.tenantEgress` per claim, dispatches through the channel table, and folds every outcome through `Lane.settle` — settle, defer, park with evidence — so the drain body is dispatch plus composition and contains zero retry, backoff, or dead-letter machinery of its own.
- Law: quota precedes transmission — an `exhausted` throttle verdict defers the claim (the lease redelivers after the window turns) without touching the wire, so a tenant's burst never converts into provider-side rejections.
- Law: pacing composes the mail pool — the claim step gates on `Mailer.idle` for mail-channel batches, deferring claim rather than queueing inside the transport.
- Law: the wake source is data-owned — the drain subscribes the journal's wake stream through the scope port; a poll loop or a second LISTEN binding here is unspellable.
- Receipt: each drained batch emits one meter fact (claims, settled, deferred, parked) — the relay's health is queryable history beside the lane evidence.
- Growth: a second relay concern (a per-region drain, a channel-partitioned drain) is a second singleton row over the same fold with a claim predicate — the drain body never forks.
- Packages: `@effect/cluster` via `./entity.ts` (`Grid.singleton`); `@rasm/ts/data` (`Journal`); `./queue.ts` (`Lane`, `Throttle`).

```typescript
const MailPayload = Schema.Struct({
  to: Schema.NonEmptyArray(Schema.String),
  subject: Schema.NonEmptyString,
  html: Schema.optionalWith(Schema.String, { as: "Option" }),
  text: Schema.String,
  attachments: Schema.Array(Schema.Struct({ filename: Schema.String, content: Schema.Uint8ArrayFromSelf, contentType: Schema.String })),
  list: Schema.optionalWith(Schema.Struct({ unsubscribe: Schema.String }), { as: "Option" }),
})

const _dispatch = (claim: Lane.Claim) =>
  claim.stream.startsWith("mail")
    ? Schema.decodeUnknown(MailPayload)(claim.body).pipe(
      Effect.mapError(() => new DeliverFault({ reason: "schema", channel: "mail", detail: claim.stream })),
      Effect.flatMap((message) => Mailer.pipe(Effect.flatMap((mailer) => mailer.send(message)))),
    )
    : Schema.decodeUnknown(HookPayload)(claim.body).pipe(
      Effect.mapError(() => new DeliverFault({ reason: "schema", channel: "webhook", detail: claim.stream })),
      Effect.flatMap(_hook),
    )

const _drain = (app: AppIdentity["app"]) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const mailer = yield* Mailer
    const idle = yield* mailer.idle
    const claims = idle
      ? yield* Journal.claimBatch(sql, app, WorkClass.bulk.concurrency * 4, Duration.toSeconds(Budget.bulk.attempt))
      : []
    yield* Lane.settle(sql, "bulk", (claim) =>
      Throttle.spend(Throttle.tenantEgress, claim.stream).pipe(
        Effect.zipRight(_dispatch(claim)),
        Effect.tap(_settled),
        Effect.as(LaneVerdict.Settled()),
        Effect.catchTag("DeliverFault", (fault) => Effect.succeed(Lane.judge(claim, { class: fault.class, detail: fault.detail }))),
        Effect.catchAll(() => Effect.succeed(LaneVerdict.Deferred({ class: "exhausted" }))),
      ), Lane.park)(claims)
  })

const Relay = (app: AppIdentity["app"]) =>
  Grid.singleton("deliver-relay", _drain(app).pipe(Effect.repeat(Schedule.spaced(Budget.bulk.base))))

const Deliver = {
  channels: { mail: "mail", webhook: "webhook" },
  suppress: _suppress,
  settled: _settled,
}

const Hook = { transmit: _hook, payload: HookPayload }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Deliver, DeliverFault, Hook, Mailer, Receipt, Relay }
```
