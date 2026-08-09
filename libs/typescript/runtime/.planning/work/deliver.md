# [RUNTIME_DELIVER]

Outbound delivery is ONE owner: mail and webhook egress are channel rows of one dispatch table sharing one settlement receipt, one reason-discriminated fault family, and one suppression fold, and the transactional-outbox relay is the cluster singleton draining every channel under the queue page's verdict vocabulary — retry, redelivery, parking, and replay never re-appear as channel-local machinery. A channel owns exactly three things: its payload's admission schema, the destination projection the suppression gate reads, and the fold from its transport's evidence into the shared receipt.

Claim admission, lease, urgency order, park ceiling, tenant egress quota, and replay arrive settled from `queue#LANE_POLICY` and `queue#THROTTLE`. Signing splits into two domains that never merge — the `Crypto` service signs webhook bodies byte-identical, the mail plane signs DKIM in-transport. Suppression is evidence on the record of truth: a bounce or gone endpoint appends a fact row, and the relay's lane rows compose the gate between admission and the wire, so a suppressed destination cannot reach a transport and the ledger stays history, never a mutable blocklist. Its module ships on the `./server` subpath as `runtime/src/work/deliver.ts`.

## [01]-[INDEX]

- [02]-[CHANNEL_FAMILY]: the channel dispatch table, the shared receipt, the one delivery fault family; `Deliver`.
- [03]-[MAIL_ROW]: the transport rows (dial and capture sinks), the one message shape, auth/DKIM/DSN policy, send evidence; `Mailer`.
- [04]-[HOOK_ROW]: byte-identity signed webhook egress and its settlement fold; `Hook`.
- [05]-[SUPPRESSION]: the shared suppress-by-evidence fold and the pre-send check; `Deliver`.
- [06]-[RELAY]: the singleton outbox drain — claim, quota, dispatch, verdict, wake, pacing; `Relay`.

## [02]-[CHANNEL_FAMILY]

[CHANNEL_FAMILY]:
- Owner: `Deliver` maps every channel row onto one settlement receipt and one `Fault.Class` family.
- Law: a channel is a row keyed by its kind — `{ payload, targets, weight, transmit }` minted through `Deliver.channel` so the members correlate on one payload type: the payload `Schema` is the admission authority the lane's `Lane.row` mint decodes against, `targets` projects the destinations the suppression gate answers over, and `transmit` carries the admitted payload to the wire with its transport evidence already folded into `Receipt | DeliverFault` on the rail; the relay dispatches on the claim's stream prefix through keyed lookup — zero `Match` arms — and a new channel (push, SMS, chat) is one table row with one relay lane row, never a sibling drain.
- Law: `_kinds` is the one channel roster — the receipt literal, the fault field, and the dispatch table derive from it, so a new channel cannot land while a second spelling still names the old pair.
- Law: a channel key IS its claim-tag namespace, so routing splits the tag against the roster and no row carries a route predicate re-spelling that grammar.
- Law: every row answers the consumption descriptor as data — `fits`, `admit`, `tenancy`, `lifetime` universal, `deliver`, `order`, `settle`, `replay`, `bound`, `refuse` the transport six, `degrade` closing — so a caller reads a forfeit instead of inferring it rather than meeting an omission that strands the fold reading one row beside its sibling.
- Law: `tenancy` carries the MECHANISM a row separates by, never `none|single|multi` — the closed axis at `core/value/identity#IDENTITY_OWNER` selects the row and the cell states which field or credential draws the boundary the quota, the claim, and the signing secret all read.
- Law: `settle` is the evidence shape a transport returns and `lifetime` the party that ends custody — the SMTP acceptance band from the next relay and the receiver's own `2xx` are distinct facts, so neither column stands in for the other and no row claims a span past the party ending it.
- Law: `order` pins FALSE on both rows with its ground — `FOR UPDATE SKIP LOCKED` claiming drains a batch unordered and neither payload carries a key selecting an ordering domain — because a foreclosed coordinate stated as a value reads beside its siblings where an omission reads as an unasked question.
- Law: `refuse` earns its column by DIVERGING — mail refuses synchronously on the SMTP code table and again asynchronously through an RFC-3461 report, while a webhook refuses on the response status and nothing arrives once the request closes; `bound` names `queue#THROTTLE` as the ceiling's owner instead of re-spelling one.
- Law: `replay` reads `queue#LANE_POLICY` and splits on the RECEIVER — a replayed mail re-sends under a fresh `messageId` no peer dedups, while a replayed webhook repeats its `webhook-id` and the receiver's own dedup absorbs it.
- Law: partial acceptance is a receipt, not a fault — a send where some recipients accept and some reject settles as a `Receipt` whose rejected band is non-empty; the suppression fold consumes the rejected band, and only a transmission that produced no acceptance at all folds to `DeliverFault`.
- Law: the payload is decoded once at the lane seam — each channel's `payload` schema rides `Lane.row`, so a decode failure parks `invalid` through the lane's poison short-circuit before any deliver code runs, and a drain-local decoder is unspellable.
- Growth: a new channel is one `Deliver.channel` row; a new settlement dimension is one `Receipt` field both channels populate; a new gate axis (a per-destination rate class, an allowlist) is a column on the channel row the relay lane reads.
- Packages: `effect`; `@rasm/ts/core` (`Fault.Class`).

```typescript
import { Array, Context, Data, DateTime, Duration, Effect, Option, Record, Redacted, Schema, Stream, pipe } from "effect"
import { HttpBody, HttpClientRequest } from "@effect/platform"
import { Singleton } from "@effect/cluster"
import { SqlClient, SqlError } from "@effect/sql"
import { createTestAccount, createTransport, getTestMessageUrl, type Transporter } from "nodemailer"
import type Mail from "nodemailer/lib/mailer"
import type SMTPConnection from "nodemailer/lib/smtp-connection"
import { type CloudEvent, CONSTANTS, HTTP, type Message, Mode } from "cloudevents"
import { Buffer } from "node:buffer"
import { Fact, Journal } from "@rasm/ts/data"
import { Crypto } from "@rasm/ts/security"
import { Fault, type Identity } from "@rasm/ts/core"
import { Client } from "../net/client.ts"
import { Pulse } from "../otel/meter.ts"
import { Setting } from "../proc/config.ts"
import { WorkClass } from "./entity.ts"
import { Lane, LaneVerdict, Throttle } from "./queue.ts"

// Channel identity is ONE roster the receipt literal, the fault field, and the dispatch table all read, so a new row
// cannot land while a second spelling still names the old pair. Each key IS its claim-tag namespace, so routing reads
// tag namespaces directly instead of a predicate every row re-spells, and the park fold agrees with it by grammar.
const _kinds = ["mail", "webhook"] as const
const _Kind = Schema.Literal(..._kinds)

class Receipt extends Schema.Class<Receipt>("DeliverReceipt")({
  channel: _Kind,
  transmission: Schema.NonEmptyString,
  accepted: Schema.Array(Schema.String),
  rejected: Schema.Array(Schema.Struct({ recipient: Schema.String, note: Schema.String })),
  at: Schema.DateTimeUtc,
  wire: Schema.Duration,
}) {}

const _family = Fault.Class.family(["dial", "refused", "bounced", "timeout", "schema"] as const, {
  dial: { class: "unavailable" },
  refused: { class: "denied" },
  bounced: { class: "invalid" },
  timeout: { class: "expired" },
  schema: { class: "invalid" },
})

class DeliverFault extends Data.TaggedError("DeliverFault")<{
  readonly reason: (typeof _family.reasons)[number]
  readonly channel: Deliver.Kind
  readonly detail: string
  readonly targets: ReadonlyArray<string>
}> {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<deliver:${this.reason}> ${this.channel}: ${this.detail}`
  }
}

declare namespace Deliver {
  type Kind = (typeof _kinds)[number]
  // Every channel answers the consumption descriptor as DATA, so a caller reads what a row gives up instead of
  // inferring it: `fits` the selection sentence, `admit` the entry, `tenancy` the MECHANISM this row separates by,
  // `lifetime` how long custody lasts AND who ends it. Transport six: `deliver` the guarantee a redrive produces,
  // `order` the ordering domain and the key member selecting it, `settle` the evidence shape the wire returns,
  // `replay` what a re-offered row does to a receiver that already saw it, `bound` what caps in-flight work, `refuse`
  // what shape a refusal arrives in. `degrade` closes with the honest forfeit and records any divergence the other
  // columns cannot express. Where this plane forecloses a coordinate, its cell pins that FALSE with the ground,
  // because an omission strands the fold reading one row beside its sibling and a guess is what callers hardcode.
  type Descriptor = {
    readonly fits: string
    readonly admit: string
    readonly tenancy: string
    readonly lifetime: string
    readonly deliver: string
    readonly order: string
    readonly settle: string
    readonly replay: string
    readonly bound: string
    readonly refuse: string
    readonly degrade: string
  }
  type Channel<A extends { readonly tenant: string }, I, R> = Descriptor & {
    readonly payload: Schema.Schema<A, I>
    readonly targets: (payload: A) => ReadonlyArray<string>
    readonly weight: (payload: A) => number
    readonly transmit: (payload: A) => Effect.Effect<Receipt, DeliverFault, R>
  }
  type _Table<T extends Record<Kind, Channel<never, never, never>> = typeof _channels> = T
}

const _channel = <A extends { readonly tenant: string }, I, R>(row: Deliver.Channel<A, I, R>): Deliver.Channel<A, I, R> => row
```

## [03]-[MAIL_ROW]

[MAIL_ROW]:
- Owner: `Mailer` — the scoped transport service built from the `Setting.mail` row, whose `transport` discriminant selects the sink: `smtp` (the pooled production dial carrying pool geometry, LOGIN credential, and DKIM material), `json` and `stream` (the capture sinks that open no socket), and `ethereal` (the sandbox dial whose credential `createTestAccount()` mints inside the same acquisition). `verify()` proves the connection at acquisition on the dialing arms alone, `close()` releases at teardown, `isIdle()` gates each claim, and the transporter's `idle` event becomes `Mailer.wake` through one scoped stream bridge. Secrets arrive `Redacted` on `Setting` and unwrap only inside the `smtp` row's own builder.
- Law: the sink is a transport row, never a code path — `_transports` keys the builder and the `dials` column off `Setting.mail.transport`, so pool and DKIM options exist only where a socket does, a capture sink is a root-config choice rather than a conditional inside the service, and a new sink (SES, a provider adapter) is one row. Without it the relay cannot be exercised without live SMTP, so the acceptance-band fold and the suppression tap have no reachable proof.
- Law: one receipt fold reads every sink — `_Sent` is the widened band the arms share, `envelopeTime` and `rejectedErrors` riding the SMTP connection alone, so a captured send settles as a real `Receipt` with a deterministic `messageId` and an empty rejected band and a second fold never appears; `getTestMessageUrl(info)` is the ethereal arm's own inspection read, an operator affordance beside the receipt and never a receipt field.
- Law: the message is one Schema — tenant, sender and recipient bands, reply threading, subject, plain/HTML/Watch/AMP/iCalendar alternatives, headers, priority, attachments, and the `list` block are fields of the channel payload decoded once. `_mailOptions` is the only conversion into `nodemailer`'s optional boundary shape; an untyped message object assembled at a call site is unspellable.
- Law: `list` is the six-key standard vocabulary, not one URL — `unsubscribe`, `help`, `subscribe`, `post`, `archive`, and `owner` each compose one interior `_ListEntry` field admitting either a bare URL or the `{ url, comment }` arm, and `_mailOptions` hands the decoded record through as `list` with absent keys dropped; the annotated arm is what nodemailer renders into the `List-Unsubscribe`/`List-Unsubscribe-Post` pair one-click unsubscribe requires, and the suppression fold's `regulatory` retention is retention OF that header, so a single string cannot express the evidence it claims to keep.
- Law: DKIM is native and mandatory on production rows — `domainName`/`keySelector`/`privateKey` ride the transport options so every message signs RFC-6376 in-transport; the security wave's HMAC domain never touches mail.
- Law: transport faults classify through the code table — `EAUTH` folds `refused` (terminal), a 4xx `responseCode` folds `dial` (transient — the lane's lease redelivers), a 5xx recipient failure folds `bounced` (the suppression fold consumes it); string-matching an error message is unspellable beside the table.
- Law: `isIdle()` and the `idle` event are the relay's pacing signal — the mail lane row reads pool capacity per claim and defers the claim onto its lease while the pool is saturated, so bulk sends respect pool geometry instead of queueing in the transport, and the `idle` event bridges into the wake race so a freed pool re-triggers the drain without waiting the lease width.
- Law: the `error` event binds inside the service scope — a pool fault arrives between sends where no claim owns it, and an emitter emitting `error` unlistened THROWS, so an unbound row converts a recoverable fault into a process death no verdict observes; it folds through `_classified` onto the log rail.
- Law: DSN rides the dialing row's transport options — `notify: ["FAILURE", "DELAY"]` with `ret: "HDRS"` requests RFC-3461 reports, because SMTP acceptance proves handoff alone and a late bounce reaches suppression only as an asynchronous report.
- Law: each transport row states its `degrade` — capture sinks forfeit wire evidence and the sandbox forfeits onward delivery, so a synthetic receipt reads as the evidence gap it is rather than as proof of delivery.
- Receipt: the interior `_Sent` band folds to the shared `Receipt` — `accepted`/`rejected`/`rejectedErrors` become the acceptance bands, `messageId` the transmission identity, `envelopeTime` the wire band, with the two SMTP-only members widened optional so an unopened envelope reads no span rather than a forged one. Nodemailer's top-level `SentMessageInfo` is `any`, so importing it erases the transport boundary under a confident receipt name.
- Growth: a provider, OAuth2, or inspect transport is one `_transports` row against one `Setting.mail.transport` value; a new message concern is one payload field and one `_mailOptions` projection.
- Packages: `nodemailer` (`createTransport`, `createTestAccount`, `getTestMessageUrl`, `Transporter`, `Mail.Address`, `Mail.ListHeader`); `effect` (`Layer`, `Option`, `Record`, `Redacted`); `../proc/config.ts` (`Setting`).

```typescript
// The band every transport arm answers: `envelopeTime` and `rejectedErrors` ride the SMTP connection alone, so the
// widened shape is what lets ONE `_mailReceipt` fold read a dialed send and a captured one without a second fold.
type _Sent = {
  readonly accepted: ReadonlyArray<string | Mail.Address>
  readonly rejected: ReadonlyArray<string | Mail.Address>
  readonly rejectedErrors?: ReadonlyArray<{ readonly recipient?: string; readonly response?: string }> | undefined
  readonly messageId: string
  readonly envelopeTime?: number | undefined
}

// RFC-3461 delivery-status notification is the mail plane's ONE asynchronous evidence channel: SMTP acceptance proves
// handoff to the next relay alone, so a `FAILURE`/`DELAY` request is what makes a late bounce arrive as a suppression
// fact instead of silence. `HDRS` returns headers rather than the whole message, so the report carries no body copy.
const _DSN = { notify: ["FAILURE", "DELAY"], ret: "HDRS" } as const satisfies SMTPConnection.DSNOptions

const _transports = {
  // Each arm builds its own transporter, so the factory's per-shape overload resolves at the row and never over a union.
  // `dials` is the column `verify` reads: a sink that opens no socket has nothing to prove at acquisition. `degrade`
  // names what an arm FORFEITS, so a capture sink's synthetic receipt reads as the evidence gap it is at the row.
  smtp: {
    dials: true,
    degrade: "<none>",
    open: (mail: Setting.Mail) =>
      Effect.sync(() =>
        createTransport({
          pool: true,
          host: mail.host,
          port: mail.port,
          secure: true,
          maxConnections: WorkClass.bulk.concurrency,
          rateLimit: mail.rate,
          auth: { user: mail.user, pass: Redacted.value(mail.pass) },
          dkim: { domainName: mail.domain, keySelector: mail.selector, privateKey: Redacted.value(mail.key) },
          dsn: _DSN,
        })
      ),
  },
  json: {
    dials: false,
    degrade: "<no-wire-evidence:accepted-band-is-the-envelope-echo>",
    open: () => Effect.sync(() => createTransport({ jsonTransport: true })),
  },
  stream: {
    dials: false,
    degrade: "<no-wire-evidence:accepted-band-is-the-envelope-echo>",
    open: () => Effect.sync(() => createTransport({ streamTransport: true, buffer: true })),
  },
  ethereal: {
    dials: true,
    degrade: "<no-onward-delivery:sandbox-captures-every-recipient>",
    open: () =>
      Effect.map(
        Effect.tryPromise({
          // the sandbox credential mints inside the same acquisition, so no environment row carries a throwaway secret
          try: () => createTestAccount(),
          catch: (cause) => new DeliverFault({ reason: "dial", channel: "mail", detail: String(cause), targets: [] }),
        }),
        (account) =>
          createTransport({
            host: account.smtp.host,
            port: account.smtp.port,
            secure: account.smtp.secure,
            auth: { user: account.user, pass: account.pass },
          }),
      ),
  },
} as const satisfies Record<string, {
  readonly dials: boolean
  readonly degrade: string
  readonly open: (mail: Setting.Mail) => Effect.Effect<Transporter<_Sent>, DeliverFault>
}>

class Mailer extends Effect.Service<Mailer>()("runtime/Mailer", {
  scoped: Effect.gen(function* () {
    const setting = yield* Setting
    const row = _transports[setting.mail.transport]
    const transporter: Transporter<_Sent> = yield* Effect.acquireRelease(
      row.open(setting.mail),
      (built) => Effect.sync(() => built.close()),
    )
    yield* row.dials
      ? Effect.tryPromise({
        try: () => transporter.verify(),
        catch: (cause) => new DeliverFault({ reason: "dial", channel: "mail", detail: String(cause), targets: [] }),
      })
      : Effect.void
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
    // Pool faults ride the emitter's OWN channel between sends, where no claim owns them, and an EventEmitter emitting
    // `error` with no listener THROWS — so leaving this row unbound turns a recoverable pool fault into a process death
    // no lane verdict, receipt, or suppression fold ever observes. Binding and draining inside the service scope folds
    // each through the same `_classified` table the send path reads; a send-time fault still reaches its own claim.
    yield* Effect.forkScoped(Stream.runForEach(
      Stream.asyncScoped<Error>((emit) =>
        Effect.acquireRelease(
          Effect.sync(() => {
            const onError = (cause: Error) => emit.single(cause)
            transporter.on("error", onError)
            return onError
          }),
          (onError) => Effect.sync(() => transporter.off("error", onError)),
        )),
      (cause) => Effect.logError(_classified(cause)),
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

const _mailReceipt = (info: _Sent, at: DateTime.Utc): Effect.Effect<Receipt, DeliverFault> => {
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
    // a sink that never opened an envelope measures no envelope time: the absent band reads zero span, never a forged one
    wire: Duration.millis(info.envelopeTime ?? 0),
  }))
}
```

## [04]-[HOOK_ROW]

[HOOK_ROW]:
- Owner: `Hook` — signed webhook egress under byte identity: the payload encodes to its wire bytes exactly once, the `Crypto` service signs THOSE bytes, and the transmission carries the v1 header triple — `webhook-id` (the deliverable identity — replay dedup on the receiving side), `webhook-timestamp` (the signing instant bounding replay windows), `webhook-signature` (`v1,<hex>` over `id.timestamp.body`) — so the receiver verifies the identical byte sequence and a re-serialization between sign and send is structurally impossible.
- Law: the HTTP leg is the branch client — `Client` default-policy rows own timeout, retry pacing, and proxy; this row adds only the signed request construction and the settlement fold: 2xx settles to `Receipt`, 410 folds `bounced` (the endpoint is gone — suppression consumes it), 429/5xx fold `dial` (the lease redelivers), a client timeout folds `timeout`.
- Law: endpoint secrets are per-destination `Redacted` material resolved through `Hook.Secret` by the payload's non-secret `keyRef`; raw key bytes never enter the persisted outbox body, a receipt, or a fault. Security composition supplies the resolver and rotates the material behind a stable reference without rewriting queued work.
- Law: `_hookProject` selects `HTTP.binary` or `HTTP.structured` through one `Mode` table.
- Law: Projected binding headers and content type enter `HookPayload` once.
- Law: `_hook` signs and transmits the same detached body bytes `_hookProject` produced.
- Boundary: envelope construction is upstream payload admission — `HookPayload.body` and `headers` preserve the admitted bytes and band unchanged; this row signs and transmits them and does not invent an envelope dialect. Framing (`content-type`, `content-length`, `transfer-encoding`) and the signature triple are reserved names the header-band admission refuses, so `payload.media` alone mints the outbound content type and a caller cannot smuggle contradictory framing beside it.
- Growth: a signing-scheme revision is a new version prefix beside `v1` in the same header; a destination policy axis (mTLS, custom header band) is a field on the destination row.
- Packages: `cloudevents`, `@effect/platform`, `effect`, `@rasm/ts/security`, and `../net/client.ts`.

```typescript
// single-sourced framing: media alone mints the content type, the signer alone mints the triple —
// a caller band re-spelling either refuses at the admission filter
const _RESERVED: ReadonlyArray<string> = [
  CONSTANTS.HEADER_CONTENT_TYPE, "content-length", "transfer-encoding",
  "webhook-id", "webhook-timestamp", "webhook-signature",
]

const HookPayload = Schema.Struct({
  tenant: Schema.NonEmptyString,
  destination: Schema.URL,
  deliverable: Schema.NonEmptyString,
  body: Schema.Uint8ArrayFromSelf,
  media: Schema.NonEmptyString,
  headers: Schema.optionalWith(
    Schema.Record({ key: Schema.String, value: Schema.String }).pipe(
      Schema.filter((band) => Object.keys(band).every((name) => !_RESERVED.includes(name.toLowerCase())), {
        message: () => "framing and signature headers are reserved — media mints the content type, the signer mints the triple",
      }),
    ),
    { default: () => ({}) },
  ),
  keyRef: Schema.NonEmptyString,
  weight: Schema.Number.pipe(Schema.int(), Schema.positive()),
})

const _hookBindings = {
  [Mode.BINARY]: HTTP.binary,
  [Mode.STRUCTURED]: HTTP.structured,
} as const

declare namespace Deliver {
  type HookMode = keyof typeof _hookBindings
  type HookDraft = Omit<typeof HookPayload.Type, "body" | "media">
}

const _hookUtf8 = new TextEncoder()

const _messageBody = (message: Message, target: string): Effect.Effect<Uint8Array, DeliverFault> =>
  message.body === undefined
    ? Effect.succeed(new Uint8Array())
    : typeof message.body === "string"
      ? Effect.succeed(_hookUtf8.encode(message.body))
      : message.body instanceof Uint8Array
        ? Effect.succeed(new Uint8Array(message.body))
        : Effect.fail(new DeliverFault({
          reason: "schema", channel: "webhook", detail: "<cloudevent-body>", targets: [target],
        }))

const _hookProject = (
  event: CloudEvent<unknown>,
  mode: Deliver.HookMode,
  draft: Deliver.HookDraft,
): Effect.Effect<typeof HookPayload.Type, DeliverFault> =>
  Effect.flatMap(
    Effect.try({
      try: () => _hookBindings[mode](event),
      catch: (cause) => new DeliverFault({
        reason: "schema", channel: "webhook", detail: String(cause), targets: [draft.destination.toString()],
      }),
    }),
    (message) =>
      pipe(message.headers[CONSTANTS.HEADER_CONTENT_TYPE], (media) => typeof media !== "string"
        ? Effect.fail(new DeliverFault({
          reason: "schema", channel: "webhook", detail: "<cloudevent-content-type>", targets: [draft.destination.toString()],
        }))
        : Effect.map(_messageBody(message, draft.destination.toString()), (body) => ({
          ...draft,
          body,
          media,
          headers: {
            ...draft.headers,
            ...Record.fromEntries(
              Array.filterMap(Object.entries(message.headers), ([name, value]) =>
                name.toLowerCase() === CONSTANTS.HEADER_CONTENT_TYPE || typeof value !== "string"
                  ? Option.none()
                  : Option.some([name, value] as const)),
            ),
          },
        }))),
  )

class _HookSecret extends Context.Tag("runtime/work/Hook/Secret")<_HookSecret, {
  readonly resolve: (keyRef: string) => Effect.Effect<Redacted.Redacted<Uint8Array>, DeliverFault>
}>() {}

const _signable = (id: string, stamp: string, body: Uint8Array): Uint8Array => {
  // BOUNDARY ADAPTER: byte-join kernel — the draft detaches immutable at the return
  const prefix = _hookUtf8.encode(`${id}.${stamp}.`)
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
          ...payload.headers,
          "webhook-id": payload.deliverable,
          "webhook-timestamp": stamp,
          "webhook-signature": `v1,${signed}`,
        }),
        HttpClientRequest.setBody(HttpBody.uint8Array(payload.body, payload.media)),
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
const _admissible = <R>(suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R>) =>
(channel: Deliver.Kind, targets: ReadonlyArray<string>): Effect.Effect<void, DeliverFault, R> =>
  Effect.findFirst(targets, (target) => suppressed(channel, target)).pipe(
    Effect.flatMap(Option.match({
      onNone: () => Effect.void,
      onSome: (target) => Effect.fail(new DeliverFault({
        reason: "refused", channel, detail: `<suppressed:${target}>`, targets: [target],
      })),
    })),
  )

const _suppress = (channel: Deliver.Kind, target: string, note: string) =>
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
- Owner: `Relay` — the one outbox drain: a `Singleton.make` (exactly one live instance cluster-wide, migrating on rebalance) whose pass fires on the merged wake stream — the journal's NOTIFY pulse handed in as the data-owned `wake` parameter, merged with the lease-width tick — claims a batch through `Journal.claimBatch` sized and leased by the `bulk` class row, and settles it through `Lane.settle` over the relay's lane rows: each row is `Lane.row(channel.payload, …)` composing the fixed sequence suppression gate → tenant throttle → `channel.transmit` → rejected-band suppression tap, so the drain body is route and composition, with zero retry, backoff, decode, or dead-letter machinery of its own.
- Law: every transmission passes one suppression decision — the gate sits inside the lane row between admission and the wire, so no route reaches `transmit` without it; a refused deliverable parks with the suppressed target as evidence through the lane's poison short-circuit.
- Law: quota precedes transmission — `Throttle.spend` runs before the wire and its exceeded posture is the durable delay, so a tenant's burst paces the drain inside the lease width instead of converting into provider-side rejections; a lease that expires mid-delay redelivers, attempts already incremented, and a quota-store fault (`RateLimiterError`) defers `unavailable`.
- Law: pacing composes the mail pool — the mail lane row reads `Mailer.idle` per claim and defers while the pool reports no capacity, so mail never queues inside the transport and webhook claims drain regardless of pool state.
- Law: the wake source is data-owned — the drain subscribes the journal's wake stream through the scope port; a poll loop or a second LISTEN binding here is unspellable.
- Law: the pass budget grades on `Journal.retryable`, never on the class default — every fault the pass raises is a store fault the journal already projects onto the shared class table, so a connection blip re-drives inside the tick and an undecodable claim batch refuses; accepting the property grader parks the whole shard's outbox on the first blip while the compiled budget records nothing.
- Receipt: each pass folds `Lane.settle`'s verdict roster into one `deliver.drained` meter fact — claims, settled, deferred, parked — and marks the settled count onto the `Pulse` throughput counter in the same fold, so the OTel series and the journal fact cannot disagree on a pass.
- Growth: a second relay concern (a per-region drain, a channel-partitioned drain) is a second singleton row over the same fold with a claim predicate — the drain body never forks.
- Packages: `@effect/cluster` (`Singleton`); `@effect/sql` (`SqlClient`, `SqlError`); `@rasm/ts/data` (`Journal`, `Fact`); `./queue.ts` (`Lane`, `LaneVerdict`, `Throttle`); `../otel/meter.ts` (`Pulse`).

```typescript
// one interior field schema every list key composes: the bare URL and the annotated arm are one decoded shape,
// so a key gains the comment form by declaration and no per-key variant exists to drift
const _ListEntry = Schema.optionalWith(
  Schema.Union(Schema.NonEmptyString, Schema.Struct({ url: Schema.NonEmptyString, comment: Schema.NonEmptyString })),
  { as: "Option" },
)

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
  // the six standard list keys nodemailer renders into List-* headers, each admitting the annotated arm; the
  // {url, comment} form is what renders the one-click List-Unsubscribe / List-Unsubscribe-Post pair the suppression
  // fold's regulatory retention is evidence of, and a bare URL string cannot express the header that evidence is about
  list: Schema.optionalWith(
    Schema.Struct({
      unsubscribe: _ListEntry,
      help: _ListEntry,
      subscribe: _ListEntry,
      post: _ListEntry,
      archive: _ListEntry,
      owner: _ListEntry,
    }),
    { as: "Option" },
  ),
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
  // the decoded record passes straight through: every present key is one ListHeader and every absent key omits
  list: Option.getOrUndefined(Option.map(message.list, Record.getSomes)),
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
    fits: "<addressed-human-notification-over-smtp>",
    admit: "<mail-prefixed-outbox-claim-decoded-through-MailPayload>",
    tenancy: "<per-payload-tenant-field:-it-keys-the-outbox-claim-and-the-queue#THROTTLE-egress-quota-that-meters-it>",
    // Custody ENDS at the accepting relay: this row cannot observe the mailbox, so it states the non-decision rather
    // than naming a span it never measures, and the DSN request is the only evidence that returns after handoff.
    lifetime: "<ends-at-smtp-acceptance-by-the-next-relay-which-then-owns-it;-undecided-past-handoff>",
    deliver: "<at-least-once:-the-outbox-lease-redelivers-and-no-smtp-peer-dedups,-so-a-lapse-mid-send-resends>",
    order: "<none:-skip-locked-claiming-drains-unordered-and-MailPayload-carries-no-key-selecting-a-domain>",
    settle: "<the-accepted/rejected-band-plus-messageId-and-envelopeTime;-a-capture-sink-synthesizes-the-same-band>",
    replay: "<queue#LANE_POLICY-re-offers-a-parked-row-under-a-fresh-messageId-no-peer-dedups,-so-the-mailbox-sees-two>",
    bound: "<queue#THROTTLE-tenantEgress-and-the-pool's-own-isIdle-capacity;-this-row-spells-neither-ceiling>",
    refuse: "<synchronously-a-reason-classed-DeliverFault-off-the-smtp-code-table,-then-an-rfc-3461-report-after-handoff>",
    degrade: "<acceptance-is-handoff-not-delivery:-a-late-bounce-returns-only-as-a-dsn-report-or-suppression-fact>",
    payload: MailPayload,
    targets: (message) => [...message.to, ...Option.getOrElse(message.cc, () => []), ...Option.getOrElse(message.bcc, () => [])],
    weight: (message) => message.weight,
    transmit: (message) => Effect.flatMap(Mailer, (mailer) => mailer.send(_mailOptions(message))),
  }),
  webhook: _channel({
    fits: "<machine-callback-to-a-tenant-registered-endpoint-under-byte-identity-signing>",
    admit: "<webhook-prefixed-outbox-claim-decoded-through-HookPayload>",
    tenancy: "<per-payload-tenant-field-and-per-destination-keyRef:-one-tenant's-secret-signs-only-its-own-endpoint>",
    lifetime: "<ends-at-the-receiver-2xx-which-the-receiver-itself-issues>",
    deliver: "<at-least-once-past-2xx:-a-redrive-repeats-the-event-id-and-the-receiver-owns-the-dedup>",
    order: "<none:-skip-locked-claiming-drains-unordered-and-HookPayload-carries-no-key-selecting-a-domain>",
    settle: "<the-receiver's-2xx-status-alone;-the-response-body-is-never-read-as-evidence>",
    replay: "<queue#LANE_POLICY-re-offers-a-parked-row-under-its-stable-webhook-id,-so-the-receiver's-dedup-absorbs-it>",
    bound: "<queue#THROTTLE-tenantEgress-and-the-Client-lane's-http-concurrency;-this-row-spells-neither-ceiling>",
    refuse: "<a-reason-classed-DeliverFault-folded-from-the-response-status;-nothing-returns-once-the-request-closes>",
    degrade: "<no-ack-past-2xx:-a-receiver-answering-then-dropping-the-work-reads-identically-to-one-that-kept-it>",
    payload: HookPayload,
    targets: (payload) => [payload.destination.toString()],
    weight: (payload) => payload.weight,
    transmit: _hook,
  }),
} as const

// Routing reads the claim tag's own namespace against the roster, so a channel key IS its prefix: a per-row predicate
// spells the same grammar once per row and lets one drift silently, and the park fold already splits the tag this way.
const _routed = (tag: string): Option.Option<Deliver.Kind> =>
  pipe(tag.split(":", 1)[0] ?? tag, (head) => Array.findFirst(_kinds, (kind) => kind === head))

const _sent = <A extends { readonly tenant: string }, I, R, R2>(
  kind: Deliver.Kind,
  row: Deliver.Channel<A, I, R>,
  suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R2>,
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
  pipe(Array.filter(verdicts, LaneVerdict.$is("Settled")).length, (settled) =>
    Effect.zipRight(
      Pulse.mark("drained", "deliver", settled),
      Fact.record({
        action: "deliver.drained",
        actor: { key: "relay", kind: "service" },
        change: [
          { _tag: "Assigned", path: "/claims", next: String(claims) },
          { _tag: "Assigned", path: "/settled", next: String(settled) },
          { _tag: "Assigned", path: "/deferred", next: String(Array.filter(verdicts, LaneVerdict.$is("Deferred")).length) },
          { _tag: "Assigned", path: "/parked", next: String(Array.filter(verdicts, LaneVerdict.$is("Parked")).length) },
        ],
        retention: "operational",
        target: { key: "deliver-relay", kind: "relay" },
      }),
    ))

// Composing the journal's published projection keeps ONE opinion about refusal, and the default gate reads a `class`
// property no driver fault carries: a claim or discharge `SqlError` grades on the shared class table, while a claim
// batch that will not decode re-reads identically and refuses instead of spending the budget.
const _redrivable = (fault: unknown): boolean => fault instanceof SqlError.SqlError && Journal.retryable(fault)

const _drain = <R>(
  app: Identity.App.Key,
  suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R>,
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
      leaseSeconds: Duration.toSeconds(Fault.Budget.at("bulk").attempt),
    })
    const verdicts = yield* Lane.settle(sql, "bulk", (tag) => Option.map(_routed(tag), (kind) => lanes[kind]), Lane.park)(claims)
    yield* _metered(claims.length, verdicts)
  })

const Relay = <R, R2>(
  app: Identity.App.Key,
  wake: Stream.Stream<unknown, never, R>,
  suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R2>,
) =>
  Singleton.make(
    "deliver-relay",
    Effect.flatMap(Mailer, (mailer) =>
      Stream.mergeAll([wake, mailer.wake, Stream.tick(Fault.Budget.at("bulk").attempt)], { concurrency: "unbounded" }).pipe(
        // Each pass contains its OWN failure: `claimBatch`, `settle`, and the meter fold all reach the database, and a
        // pass failure escaping the stream ends the branch's one outbox drain for the shard instead of skipping a tick.
        // Retry spends the class budget under the journal's own gate, the cause fold catches the defect the typed rail
        // cannot carry, and claims left unsettled ride their lease back onto the next wake — so a transient database
        // fault costs one tick, not the relay.
        Stream.runForEach(() =>
          _drain(app, suppressed).pipe(
            Effect.retry(Fault.Budget.schedule(WorkClass.bulk.budget, _redrivable)),
            Effect.catchAllCause(Effect.logError),
          )),
      )),
  )

const Deliver = {
  channel: _channel,
  channels: _channels,
  admissible: _admissible,
  suppress: _suppress,
  settled: _settled,
}

const Hook = {
  Secret: _HookSecret,
  payload: HookPayload,
  project: _hookProject,
  transmit: _hook,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Deliver, DeliverFault, Hook, Mailer, Receipt, Relay }
```

## [07]-[RESEARCH]

(none)
