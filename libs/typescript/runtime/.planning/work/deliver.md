# [RUNTIME_DELIVER]

Outbound delivery as ONE owner: mail and webhook egress are channel rows of one dispatch table, sharing one settlement receipt, one reason-discriminated fault family, and one suppression fold — the formerly twice-owned transport convention spelled once — and the transactional-outbox relay is the cluster singleton that drains every channel under the queue page's verdict vocabulary, so retry, redelivery, parking, and replay never re-appear here as channel-local machinery. A channel owns exactly three things: its payload's admission schema, the destination projection the suppression gate reads, and how its transport's evidence folds into the shared receipt; everything around the transmission — claim admission, claim lease, urgency order, park ceiling, tenant egress quota, backfill replay — arrives settled from `queue#LANE_POLICY` and `queue#THROTTLE`. Signing rides the security wave: a webhook body is signed byte-identical by the `Crypto` service and the mail plane signs DKIM natively in-transport — two signature domains that never merge. Suppression is evidence on the record of truth: a hard bounce or a gone endpoint appends a fact row, and the relay's lane rows compose the suppression gate between admission and the wire, so a suppressed destination structurally cannot be transmitted to and the ledger stays history, never a mutable blocklist table. The module ships on the `./server` exports subpath as `runtime/src/work/deliver.ts`.

## [01]-[CLUSTERS]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                             | [PUBLIC]  |
| :-----: | :--------------- | :--------------------------------------------------------------------------------- | :-------- |
|  [01]   | `CHANNEL_FAMILY` | the channel dispatch table, the shared receipt, the one delivery fault family      | `Deliver` |
|  [02]   | `MAIL_ROW`       | the scoped transporter, the one message shape, auth/DKIM/DSN policy, send evidence | `Mailer`  |
|  [03]   | `HOOK_ROW`       | byte-identity signed webhook egress and its settlement fold                        | `Hook`    |
|  [04]   | `SUPPRESSION`    | the shared suppress-by-evidence fold and the pre-send check                        | `Deliver` |
|  [05]   | `RELAY`          | the singleton outbox drain — claim, quota, dispatch, verdict, wake, pacing         | `Relay`   |

## [02]-[CHANNEL_FAMILY]

[CHANNEL_FAMILY]:
- Owner: `Deliver` — the channel dispatch table and the shapes every channel speaks. `Deliver.Receipt` is the settlement evidence: channel kind, transmission identity (the SMTP `messageId`, the webhook delivery id), per-recipient acceptance splits, the wire instant, and the transport's timing band — one `Schema.Class` whose fields serve both channels because settlement IS the same concept. `DeliverFault` is the one fault family: a reason row (`dial | refused | bounced | timeout | schema`) carrying its `FaultClass` kind, so the lane's judge fold reads retryability off the class table and a channel never declares a private fault rail.
- Law: a channel is a row keyed by its kind — `{ payload, targets, transmit }` minted through `Deliver.channel` so the three members correlate on one payload type: the payload `Schema` is the admission authority the lane's `Lane.row` mint decodes against, `targets` projects the destinations the suppression gate answers over, and `transmit` carries the admitted payload to the wire with its transport evidence already folded into `Receipt | DeliverFault` on the rail; the relay dispatches on the claim's stream prefix through keyed lookup — zero `Match` arms — and a new channel (push, SMS, chat) is one table row plus one relay lane row, never a sibling drain.
- Law: partial acceptance is a receipt, not a fault — a send where some recipients accept and some reject settles as a `Receipt` whose rejected band is non-empty; the suppression fold consumes the rejected band, and only a transmission that produced no acceptance at all folds to `DeliverFault`.
- Law: the payload is decoded once at the lane seam — each channel's `payload` schema rides `Lane.row`, so a decode failure parks `invalid` through the lane's poison short-circuit before any deliver code runs, and a drain-local decoder is unspellable.
- Growth: a new channel is one `Deliver.channel` row; a new settlement dimension is one `Receipt` field both channels populate; a new gate axis (a per-destination rate class, an allowlist) is a column on the channel row the relay lane reads.
- Packages: `effect` (`Schema`, `Data`, `DateTime`, `Duration`); `@rasm/ts/core` (`FaultClass`).

```typescript
import { Array, Context, Data, DateTime, Duration, Effect, Option, Redacted, Schema, Stream, Struct } from "effect"
import { HttpBody, HttpClientRequest } from "@effect/platform"
import { Singleton } from "@effect/cluster"
import { SqlClient } from "@effect/sql"
import { createTransport, type Transporter } from "nodemailer"
import type SMTPPool from "nodemailer/lib/smtp-pool"
import { Buffer } from "node:buffer"
import { Fact, Journal } from "@rasm/ts/data"
import { Crypto } from "@rasm/ts/security"
import { type AppIdentity, Budget, FaultClass } from "@rasm/ts/core"
import { Client } from "../net/client.ts"
import { Setting } from "../proc/config.ts"
import { WorkClass } from "./entity.ts"
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
} as const satisfies Record<string, FaultClass.Kind>

class DeliverFault extends Data.TaggedError("DeliverFault")<{
  readonly reason: keyof typeof _reasons
  readonly channel: "mail" | "webhook"
  readonly detail: string
  readonly targets: ReadonlyArray<string>
}> {
  get class(): FaultClass.Kind {
    return _reasons[this.reason]
  }
}

declare namespace Deliver {
  type Channel<A extends { readonly tenant: string }, I, R> = {
    readonly payload: Schema.Schema<A, I>
    readonly route: (stream: string) => boolean
    readonly targets: (payload: A) => ReadonlyArray<string>
    readonly weight: (payload: A) => number
    readonly transmit: (payload: A) => Effect.Effect<Receipt, DeliverFault, R>
  }
}

const _channel = <A extends { readonly tenant: string }, I, R>(row: Deliver.Channel<A, I, R>): Deliver.Channel<A, I, R> => row
```

## [03]-[MAIL_ROW]

[MAIL_ROW]:
- Owner: `Mailer` — the scoped pooled-SMTP service built from the existing `Setting.mail` row. `createTransport` receives the pool geometry, LOGIN credential, and DKIM material; `verify()` proves the connection at acquisition, `close()` drains sockets at release, `isIdle()` gates each claim, and the transporter's `idle` event becomes `Mailer.wake` through one scoped stream bridge. Secrets arrive `Redacted` on `Setting` and unwrap only in `createTransport`.
- Law: the message is one Schema — tenant, sender and recipient bands, reply threading, subject, plain/HTML/Watch/AMP/iCalendar alternatives, headers, priority, attachments, and the `list` block are fields of the channel payload decoded once. `_mailOptions` is the only conversion into `nodemailer`'s optional boundary shape; an untyped message object assembled at a call site is unspellable.
- Law: DKIM is native and mandatory on production rows — `domainName`/`keySelector`/`privateKey` ride the transport options so every message signs RFC-6376 in-transport; the security wave's HMAC domain never touches mail.
- Law: transport faults classify through the code table — `EAUTH` folds `refused` (terminal), a 4xx `responseCode` folds `dial` (transient — the lane's lease redelivers), a 5xx recipient failure folds `bounced` (the suppression fold consumes it); string-matching an error message is unspellable beside the table.
- Law: `isIdle()` and the `idle` event are the relay's pacing signal — the mail lane row reads pool capacity per claim and defers the claim onto its lease while the pool is saturated, so bulk sends respect pool geometry instead of queueing in the transport, and the `idle` event bridges into the wake race so a freed pool re-triggers the drain without waiting the lease width.
- Receipt: `SMTPPool.SentMessageInfo` folds to the shared `Receipt` — `accepted`/`rejected`/`rejectedErrors` become the acceptance bands, `messageId` the transmission identity, `envelopeTime` the wire band. Nodemailer's top-level `SentMessageInfo` is `any`, so importing it erases the transport boundary under a confident receipt name.
- Growth: a provider, OAuth2, or inspect transport becomes one discriminated `Setting.mail` policy row consumed by this same scoped service; a new message concern is one payload field and one `_mailOptions` projection.
- Packages: `nodemailer` (`createTransport`, `Transporter`, `SMTPPool.SentMessageInfo`, `SMTPPool.Options`); `effect` (`Layer`, `Config`, `Redacted`); `../proc/config.ts` (`Setting`).

```typescript
class Mailer extends Effect.Service<Mailer>()("runtime/Mailer", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const transporter: Transporter<SMTPPool.SentMessageInfo, SMTPPool.Options> = yield* Effect.acquireRelease(
      Effect.sync(() =>
        createTransport({
          pool: true,
          host: setting.mail.host,
          port: setting.mail.port,
          secure: true,
          maxConnections: WorkClass.bulk.concurrency,
          rateLimit: setting.mail.rate,
          auth: { user: setting.mail.user, pass: Redacted.value(setting.mail.pass) },
          dkim: { domainName: setting.mail.domain, keySelector: setting.mail.selector, privateKey: Redacted.value(setting.mail.key) },
        })
      ),
      (built) => Effect.sync(() => built.close()),
    )
    yield* Effect.tryPromise({
      try: () => transporter.verify(),
      catch: (cause) => new DeliverFault({ reason: "dial", channel: "mail", detail: String(cause), targets: [] }),
    })
    const send = (message: Parameters<Transporter["sendMail"]>[0]) => Effect.gen(function* () {
      const info = yield* Effect.tryPromise({
        try: () => transporter.sendMail(message),
        catch: (cause) => _classified(cause),
      })
      const at = yield* DateTime.now
      return yield* _mailReceipt(info, at)
    })
    const idle = Effect.sync(() => transporter.isIdle())
    const wake = Stream.asyncScoped<void>((emit) =>
      Effect.acquireRelease(
        Effect.sync(() => {
          const onIdle = () => emit.single(undefined)
          transporter.on("idle", onIdle)
          return onIdle
        }),
        (onIdle) => Effect.sync(() => transporter.off("idle", onIdle)),
      ))
    return { send, idle, wake } as const
  }),
}) {}

const _MailFailure = Schema.Struct({
  code: Schema.optional(Schema.String),
  responseCode: Schema.optional(Schema.Number),
  recipient: Schema.optional(Schema.String),
  response: Schema.optional(Schema.String),
})

const _classified = (cause: unknown): DeliverFault => {
  const admitted = Option.getOrElse(Schema.decodeUnknownOption(_MailFailure)(cause), (): typeof _MailFailure.Type => ({}))
  const code = admitted.code ?? ""
  const status = admitted.responseCode ?? 0
  return new DeliverFault({
    reason: code === "EAUTH" ? "refused" : status >= 500 ? "bounced" : "dial",
    channel: "mail",
    detail: `${code}:${status}`,
    targets: admitted.recipient === undefined ? [] : [admitted.recipient],
  })
}

const _mailReceipt = (info: SMTPPool.SentMessageInfo, at: DateTime.Utc): Effect.Effect<Receipt, DeliverFault> => {
  const accepted = Array.map(info.accepted, String)
  const rejected = Array.map(info.rejectedErrors ?? [], (fault) => ({ recipient: String(fault.recipient ?? ""), note: fault.response ?? "" }))
  return accepted.length === 0
    ? Effect.fail(new DeliverFault({ reason: "bounced", channel: "mail", detail: "<all-rejected>", targets: Array.map(info.rejected, String) }))
    : Effect.succeed(new Receipt({
    channel: "mail",
    transmission: info.messageId,
    accepted,
    rejected,
    at,
    wire: Duration.millis(info.envelopeTime ?? 0),
  }))
}
```

## [04]-[HOOK_ROW]

[HOOK_ROW]:
- Owner: `Hook` — signed webhook egress under byte identity: the payload encodes to its wire bytes exactly once, the `Crypto` service signs THOSE bytes, and the transmission carries the v1 header triple — `webhook-id` (the deliverable identity — replay dedup on the receiving side), `webhook-timestamp` (the signing instant bounding replay windows), `webhook-signature` (`v1,<hex>` over `id.timestamp.body`) — so the receiver verifies the identical byte sequence and a re-serialization between sign and send is structurally impossible.
- Law: the HTTP leg is the branch client — `Client` default-policy rows own timeout, retry pacing, and proxy; this row adds only the signed request construction and the settlement fold: 2xx settles to `Receipt`, 410 folds `bounced` (the endpoint is gone — suppression consumes it), 429/5xx fold `dial` (the lease redelivers), a client timeout folds `timeout`.
- Law: endpoint secrets are per-destination `Redacted` material resolved through `Hook.Secret` by the payload's non-secret `keyRef`; raw key bytes never enter the persisted outbox body, a receipt, or a fault. The security composition supplies the resolver and may rotate the material behind a stable reference without rewriting queued work.
- Boundary: the inbound half — verifying a foreign webhook, resolving a `flow#SIGNAL_GATE` token from a verified callback — is the serving plane's mount; this row is egress only.
- Growth: a signing-scheme revision is a new version prefix beside `v1` in the same header; a destination policy axis (mTLS, custom header band) is a field on the destination row.
- Packages: `@effect/platform` (`HttpClientRequest`, `HttpBody`); `@rasm/ts/security` (`Crypto`); `../net/client.ts` (`Client`).

```typescript
const HookPayload = Schema.Struct({
  tenant: Schema.NonEmptyString,
  destination: Schema.URL,
  deliverable: Schema.NonEmptyString,
  body: Schema.Uint8ArrayFromSelf,
  keyRef: Schema.NonEmptyString,
  weight: Schema.Number.pipe(Schema.int(), Schema.positive()),
})

class _HookSecret extends Context.Tag("runtime/work/Hook/Secret")<_HookSecret, {
  readonly resolve: (keyRef: string) => Effect.Effect<Redacted.Redacted<Uint8Array>, DeliverFault>
}>() {}

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
    const secrets = yield* _HookSecret
    const at = yield* DateTime.now
    const stamp = String(Math.trunc(DateTime.toEpochMillis(at) / 1000))
    const key = yield* secrets.resolve(payload.keyRef)
    const signed = yield* crypto.sign(key, _signable(payload.deliverable, stamp, payload.body)).pipe(
      Effect.mapError((fault) => new DeliverFault({
        reason: "refused", channel: "webhook", detail: fault.reason, targets: [payload.destination.toString()],
      })),
    )
    return yield* Client.dial(
      "batch",
      HttpClientRequest.post(payload.destination.toString()).pipe(
        HttpClientRequest.setHeaders({
          "webhook-id": payload.deliverable,
          "webhook-timestamp": stamp,
          "webhook-signature": `v1,${signed}`,
        }),
        HttpClientRequest.setBody(HttpBody.uint8Array(payload.body, "application/json")),
      ),
    ).pipe(
      Effect.scoped,
      Effect.as(new Receipt({ channel: "webhook", transmission: payload.deliverable, accepted: [payload.destination.toString()], rejected: [], at, wire: Duration.zero })),
      Effect.catchTags({
        ResponseError: (fault) => _hookSettle(fault.response.status, payload.destination.toString()),
        RequestError: () => Effect.fail(new DeliverFault({
          reason: "dial", channel: "webhook", detail: "<transport>", targets: [payload.destination.toString()],
        })),
        Lapse: () => Effect.fail(new DeliverFault({
          reason: "timeout", channel: "webhook", detail: "<budget>", targets: [payload.destination.toString()],
        })),
      }),
    )
  })

const _hookSettle = (status: number, target: string): Effect.Effect<never, DeliverFault> =>
  Effect.fail(new DeliverFault({
    reason: status === 404 || status === 410 ? "bounced" : status === 401 || status === 403 ? "refused" : status === 408 ? "timeout" : "dial",
    channel: "webhook",
    detail: String(status),
    targets: [target],
  }))
```

## [05]-[SUPPRESSION]

[SUPPRESSION]:
- Owner: the shared suppress-by-evidence fold — both channels feed it and both consult it. A `bounced`-reasoned fault or a receipt's rejected band appends one `deliver.suppressed` fact row (recipient or destination as target, the channel and note as change rows, `regulatory` retention for mail — the unsubscribe evidence — and `operational` for webhooks); `Deliver.admissible(suppressed)(channel, targets)` folds the channel row's projected targets over the suppression read the data wave serves and answers before any `transmit` — the relay's lane rows compose it between lane admission and the wire, so a suppressed destination structurally cannot reach a transport effect, and a direct send outside the relay composes the same gate at its own seam.
- Law: a suppressed target refuses the whole deliverable — the gate fails `refused` (`denied` class), the lane's poison short-circuit parks it on first refusal with the suppressed target as evidence, and replay after reinstatement is the one path back; a silently narrowed recipient list erases the evidence the park row carries.
- Law: suppression is append-only history — reinstatement is a `deliver.reinstated` fact, and the projection folds the pair; deleting suppression evidence is unrepresentable.
- Law: the unsubscribe seam is one-way — the serving plane's unsubscribe endpoint appends the same fact shape; this fold never mounts a route.
- Growth: a suppression cause (complaint feedback loop, manual block) is one action verb on the same fact shape.
- Packages: `@rasm/ts/data` (`Fact`); `effect` (`Effect`, `Option`).

```typescript
const _admissible = <R>(suppressed: (channel: Receipt["channel"], target: string) => Effect.Effect<boolean, never, R>) =>
(channel: Receipt["channel"], targets: ReadonlyArray<string>): Effect.Effect<void, DeliverFault, R> =>
  Effect.findFirst(targets, (target) => suppressed(channel, target)).pipe(
    Effect.flatMap(Option.match({
      onNone: () => Effect.void,
      onSome: (target) => Effect.fail(new DeliverFault({
        reason: "refused", channel, detail: `<suppressed:${target}>`, targets: [target],
      })),
    })),
  )

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

## [06]-[RELAY]

[RELAY]:
- Owner: `Relay` — the one outbox drain: a `Singleton.make` (exactly one live instance cluster-wide, migrating on rebalance) whose pass fires on the merged wake stream — the journal's NOTIFY pulse handed in as the data-owned `wake` parameter, merged with the lease-width tick — claims a batch through `Journal.claimBatch` sized and leased by the `bulk` class row, and settles it through `Lane.settle` over the relay's lane rows: each row is `Lane.row(channel.payload, …)` composing the fixed sequence suppression gate → tenant throttle → `channel.transmit` → rejected-band suppression tap, so the drain body is route plus composition and contains zero retry, backoff, decode, or dead-letter machinery of its own.
- Law: every transmission passes one suppression decision — the gate sits inside the lane row between admission and the wire, so no route reaches `transmit` without it; a refused deliverable parks with the suppressed target as evidence through the lane's poison short-circuit.
- Law: quota precedes transmission — `Throttle.spend` runs before the wire and its exceeded posture is the durable delay, so a tenant's burst paces the drain inside the lease width instead of converting into provider-side rejections; a lease that expires mid-delay simply redelivers, attempts already incremented, and a quota-store fault (`RateLimiterError`) defers `unavailable`.
- Law: pacing composes the mail pool — the mail lane row reads `Mailer.idle` per claim and defers while the pool reports no capacity, so mail never queues inside the transport and webhook claims drain regardless of pool state.
- Law: the wake source is data-owned — the drain subscribes the journal's wake stream through the scope port; a poll loop or a second LISTEN binding here is unspellable.
- Receipt: each pass folds `Lane.settle`'s verdict roster into one `deliver.drained` meter fact — claims, settled, deferred, parked — so the relay's health is queryable history beside the lane evidence.
- Growth: a second relay concern (a per-region drain, a channel-partitioned drain) is a second singleton row over the same fold with a claim predicate — the drain body never forks.
- Packages: `@effect/cluster` (`Singleton`); `@rasm/ts/data` (`Journal`, `Fact`); `./queue.ts` (`Lane`, `LaneVerdict`, `Throttle`).

```typescript
const MailPayload = Schema.Struct({
  tenant: Schema.NonEmptyString,
  from: Schema.NonEmptyString,
  to: Schema.NonEmptyArray(Schema.String),
  cc: Schema.optionalWith(Schema.Array(Schema.String), { as: "Option" }),
  bcc: Schema.optionalWith(Schema.Array(Schema.String), { as: "Option" }),
  replyTo: Schema.optionalWith(Schema.String, { as: "Option" }),
  subject: Schema.NonEmptyString,
  html: Schema.optionalWith(Schema.String, { as: "Option" }),
  text: Schema.String,
  watchHtml: Schema.optionalWith(Schema.String, { as: "Option" }),
  amp: Schema.optionalWith(Schema.String, { as: "Option" }),
  icalEvent: Schema.optionalWith(Schema.String, { as: "Option" }),
  headers: Schema.Record({ key: Schema.String, value: Schema.String }),
  priority: Schema.Literal("high", "normal", "low"),
  attachments: Schema.Array(Schema.Struct({
    filename: Schema.String,
    content: Schema.Uint8ArrayFromSelf,
    contentType: Schema.String,
    disposition: Schema.Literal("attachment", "inline"),
    cid: Schema.optionalWith(Schema.String, { as: "Option" }),
  })),
  list: Schema.optionalWith(Schema.Struct({ unsubscribe: Schema.String }), { as: "Option" }),
  weight: Schema.Number.pipe(Schema.int(), Schema.positive()),
})

const _mailOptions = (message: typeof MailPayload.Type): Parameters<Transporter["sendMail"]>[0] => ({
  from: message.from,
  to: [...message.to],
  cc: Option.getOrUndefined(message.cc),
  bcc: Option.getOrUndefined(message.bcc),
  replyTo: Option.getOrUndefined(message.replyTo),
  subject: message.subject,
  text: message.text,
  html: Option.getOrUndefined(message.html),
  watchHtml: Option.getOrUndefined(message.watchHtml),
  amp: Option.getOrUndefined(message.amp),
  icalEvent: Option.getOrUndefined(message.icalEvent),
  headers: message.headers,
  priority: message.priority,
  list: Option.match(message.list, { onNone: () => undefined, onSome: (list) => ({ unsubscribe: list.unsubscribe }) }),
  attachments: Array.map(message.attachments, (attachment) => ({
    filename: attachment.filename,
    content: Buffer.from(attachment.content),
    contentType: attachment.contentType,
    contentDisposition: attachment.disposition,
    cid: Option.getOrUndefined(attachment.cid),
  })),
})

const _channels = {
  mail: _channel({
    payload: MailPayload,
    route: (tag) => tag.startsWith("mail:"),
    targets: (message) => [...message.to, ...Option.getOrElse(message.cc, () => []), ...Option.getOrElse(message.bcc, () => [])],
    weight: (message) => message.weight,
    transmit: (message) => Effect.flatMap(Mailer, (mailer) => mailer.send(_mailOptions(message))),
  }),
  webhook: _channel({
    payload: HookPayload,
    route: (tag) => tag.startsWith("webhook:"),
    targets: (payload) => [payload.destination.toString()],
    weight: (payload) => payload.weight,
    transmit: _hook,
  }),
} as const

const _routed = (tag: string): Option.Option<Receipt["channel"]> =>
  Array.findFirst(Struct.keys(_channels), (kind) => _channels[kind].route(tag))

const _sent = <A extends { readonly tenant: string }, I, R, R2>(
  kind: Receipt["channel"],
  row: Deliver.Channel<A, I, R>,
  suppressed: (channel: Receipt["channel"], target: string) => Effect.Effect<boolean, never, R2>,
) =>
(payload: A, meta: Lane.Meta) =>
  _admissible(suppressed)(kind, row.targets(payload)).pipe(
    Effect.zipRight(Throttle.spend(Throttle.tenantEgress, {
      tenant: payload.tenant,
      channel: kind,
      weight: row.weight(payload),
    })),
    Effect.zipRight(row.transmit(payload)),
    Effect.tap(_settled),
    Effect.as(LaneVerdict.Settled()),
    Effect.tapErrorTag("DeliverFault", (fault) =>
      fault.reason === "bounced"
        ? Effect.forEach(fault.targets, (target) => _suppress(kind, target, fault.detail), { discard: true })
        : Effect.void),
    Effect.catchTags({
      DeliverFault: (fault) => Effect.succeed(Lane.judge(meta, "bulk", { class: fault.class, detail: fault.detail })),
      RateLimiterError: () => Effect.succeed(LaneVerdict.Deferred({ class: "unavailable" })),
    }),
  )

const _metered = (claims: number, verdicts: ReadonlyArray<LaneVerdict>) =>
  Fact.record({
    action: "deliver.drained",
    actor: { key: "relay", kind: "service" },
    change: [
      { _tag: "Assigned", path: "/claims", next: String(claims) },
      { _tag: "Assigned", path: "/settled", next: String(Array.filter(verdicts, LaneVerdict.$is("Settled")).length) },
      { _tag: "Assigned", path: "/deferred", next: String(Array.filter(verdicts, LaneVerdict.$is("Deferred")).length) },
      { _tag: "Assigned", path: "/parked", next: String(Array.filter(verdicts, LaneVerdict.$is("Parked")).length) },
    ],
    retention: "operational",
    target: { key: "deliver-relay", kind: "relay" },
  })

const _drain = <R>(
  app: AppIdentity["app"],
  suppressed: (channel: Receipt["channel"], target: string) => Effect.Effect<boolean, never, R>,
) =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const mailer = yield* Mailer
    const lanes = {
      mail: Lane.row(_channels.mail.payload, (message, meta) =>
        Effect.flatMap(mailer.idle, (idle) =>
          idle
            ? _sent("mail", _channels.mail, suppressed)(message, meta)
            : Effect.succeed(LaneVerdict.Deferred({ class: "exhausted" })))),
      webhook: Lane.row(_channels.webhook.payload, _sent("webhook", _channels.webhook, suppressed)),
    } as const
    const claims = yield* Journal.claimBatch(sql, {
      app,
      take: WorkClass.bulk.concurrency * 4,
      leaseSeconds: Duration.toSeconds(Budget.bulk.attempt),
    })
    const verdicts = yield* Lane.settle(sql, "bulk", (tag) => Option.map(_routed(tag), (kind) => lanes[kind]), Lane.park)(claims)
    yield* _metered(claims.length, verdicts)
  })

const Relay = <R, R2>(
  app: AppIdentity["app"],
  wake: Stream.Stream<unknown, never, R>,
  suppressed: (channel: Receipt["channel"], target: string) => Effect.Effect<boolean, never, R2>,
) =>
  Singleton.make(
    "deliver-relay",
    Effect.flatMap(Mailer, (mailer) =>
      Stream.mergeAll([wake, mailer.wake, Stream.tick(Budget.bulk.attempt)], { concurrency: "unbounded" }).pipe(
        Stream.runForEach(() => _drain(app, suppressed)),
      )),
  )

const Deliver = {
  channel: _channel,
  channels: _channels,
  admissible: _admissible,
  suppress: _suppress,
  settled: _settled,
}

const Hook = { Secret: _HookSecret, transmit: _hook, payload: HookPayload }

// --- [EXPORTS] --------------------------------------------------------------------------

export { Deliver, DeliverFault, Hook, Mailer, Receipt, Relay }
```
