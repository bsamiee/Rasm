# [RUNTIME_DELIVER]

Outbound delivery is ONE owner: mail and webhook egress are channel rows of one dispatch table sharing the work plane's settled-receipt carrier, one reason-discriminated fault family, and one suppression fold, and the transactional-outbox relay is the cluster singleton draining every channel under the queue page's verdict vocabulary — retry, redelivery, parking, and replay never re-appear as channel-local machinery. Each channel owns exactly four things: its payload's admission schema, the destination projection the suppression gate reads, the service class its host crossing re-drives under, and the fold from its transport's evidence into the shared carrier.

Claim admission, lease, urgency order, park ceiling, tenant egress quota, and replay arrive settled from `queue#LANE_POLICY` and `queue#THROTTLE`. Signing splits into two domains that never merge — the `Crypto` service signs webhook bodies byte-identical, the mail plane signs DKIM in-transport.

Suppression is evidence on the record of truth: a bounce or gone endpoint appends a fact row, and the relay's lane rows compose the gate between admission and the wire, so a suppressed destination cannot reach a transport and the ledger stays history, never a mutable blocklist. Its module ships on the `./server` subpath as `runtime/src/work/deliver.ts`.

## [01]-[INDEX]

- [02]-[CHANNEL_FAMILY]: `Deliver` — the channel dispatch table, the settled-receipt projection, the one delivery fault family.
- [03]-[MAIL_ROW]: `Mailer` — dial and capture transport rows, one message shape, auth/DKIM/DSN policy, send evidence.
- [04]-[HOOK_ROW]: byte-identity signed webhook egress and its settlement fold; `Hook`.
- [05]-[SUPPRESSION]: `Deliver.admissible` — the shared suppress-by-evidence fold and its pre-send check.
- [06]-[RELAY]: `Relay` — the singleton outbox drain: claim, quota, dispatch, verdict, wake, pacing.

## [02]-[CHANNEL_FAMILY]

- Owner: `Deliver` maps every channel row onto the `entity#SETTLED_RECEIPT` carrier and one `Fault.Class` family.
- Law: a channel is a row keyed by its kind — `{ payload, targets, weight, clazz, transmit }` minted through `Deliver.channel` so the members correlate on one payload type: the payload `Schema` is the admission authority the lane's `Lane.row` mint decodes against, `targets` projects the destinations the suppression gate answers over, `clazz` prices the re-drive its host crossing earns, and `transmit` carries the admitted payload to the wire with its transport evidence already folded into `Delivery | DeliverFault` on the rail; the relay dispatches on the claim's stream prefix through keyed lookup — zero `Match` arms — and a new channel (push, SMS, chat) is one table row with one relay lane row, never a sibling drain.
- Law: `clazz` is the row's own re-drive price and the drain's is the drain's — a pooled SMTP dial and a signed HTTP POST are two host crossings, so the row elects the class `Lane.judge` grades against while `Lane.settle`'s fan-out and the claim's lease width stay the RELAY's `bulk`; a table with no class column hands one arm the ceiling its sibling earned, which is the split every other dispatch table in the branch already forecloses at its row.
- Law: the settlement is the work plane's one carrier, extended here and never restated — `Delivery` widens `entity#SETTLED_RECEIPT`'s spine with the channel's own evidence (which channel crossed, the acceptance bands, the SMTP envelope span), so a consumer reading across a rendered document and a delivered message reads one partition, one provenance pair, and one warning band; a second class re-spelling those columns is the parallel-receipt defect the carrier exists to close.
- Law: `_kinds` is the one channel roster — the receipt literal, the fault field, and the dispatch table derive from it, so a new channel cannot land while a second spelling still names the old pair.
- Law: a channel key IS its claim-tag namespace, so routing splits the tag against the roster and no row carries a route predicate re-spelling that grammar.
- Law: every row answers the consumption descriptor as data — `fits`, `admit`, `tenancy`, `lifetime` universal, `deliver`, `order`, `settle`, `replay`, `bound`, `refuse` the transport six, `degrade` closing — so a caller reads a forfeit instead of inferring it rather than meeting an omission that strands the fold reading one row beside its sibling.
- Law: `tenancy` carries the MECHANISM a row separates by, never `none|single|multi` — the closed axis at `core/value/identity#IDENTITY_OWNER` selects the row and the cell states which field or credential draws the boundary the quota, the claim, and the signing secret all read.
- Law: `settle` is the evidence shape a transport returns and `lifetime` the party that ends custody — the SMTP acceptance band from the next relay and the receiver's own `2xx` are distinct facts, so neither column stands in for the other and no row claims a span past the party ending it.
- Law: `order` pins FALSE on both rows with its ground — `FOR UPDATE SKIP LOCKED` claiming drains a batch unordered and neither payload carries a key selecting an ordering domain — because a foreclosed coordinate stated as a value reads beside its siblings where an omission reads as an unasked question.
- Law: `refuse` earns its column by DIVERGING — mail refuses synchronously on the SMTP code table and again asynchronously through an RFC-3461 report, while a webhook refuses on the response status and nothing arrives once the request closes; `bound` names `queue#THROTTLE` as the ceiling's owner instead of re-spelling one.
- Law: `replay` reads `queue#LANE_POLICY` and splits on the RECEIVER — a replayed mail re-sends under a fresh `messageId` no peer dedups, while a replayed webhook repeats its `webhook-id` and the receiver's own dedup absorbs it.
- Law: partial acceptance is a settlement, not a fault — a send where some recipients accept and some reject lands `partition: "partial"` with the rejected band graded onto the warning band under the class its refusal would have taken; the suppression fold consumes the recipients off that same band, and only a transmission that produced no acceptance at all folds to `DeliverFault`.
- Law: a refusal carries its family CASE, never a free string beside a closed reason — each row declares the operands it renders (the channel crossed, the destinations carried, the transport's own diagnostic) and renders its own sentence, so the park evidence a dead-set reader sees is the row's text rather than a template the raise hand-wrote.
- Law: a stated window rides the VALUE under one word — a receiver answering `429` with `Retry-After` is the branch's inbound producer of a measured wait, so the refusal carries `after` and `Fault.Class.statedOf` seats it on the lane verdict; every other refusal measured nothing and stays absent rather than inventing a zero that re-offers immediately.
- Law: the payload is decoded once at the lane seam — each channel's `payload` schema rides `Lane.row`, so a decode failure parks `invalid` through the lane's poison short-circuit before any deliver code runs, and a drain-local decoder is unspellable.
- Growth: a new channel is one `Deliver.channel` row; a new settlement dimension is one spine field at `entity#SETTLED_RECEIPT` every producer populates; a new gate axis (a per-destination rate class, an allowlist) is a column on the channel row the relay lane reads.
- Packages: `effect` (`Array`, `Duration`, `Option`, `Schema`); `@rasm/core` (`Fault.Class`); `./entity.ts` (`Settled`, `WorkClass`).

```typescript signature
import { VariantSchema } from "@effect/experimental"
import {
  Array, Context, DateTime, Duration, Effect, Number, Option, Record, Redacted, Schema, Stream, pipe,
} from "effect"
import { Headers, HttpBody, HttpClientRequest, type HttpClientResponse } from "@effect/platform"
import { RateLimiter as Fleet } from "@effect/experimental"
import { Singleton } from "@effect/cluster"
import { SqlClient, SqlError } from "@effect/sql"
import { createTestAccount, createTransport, getTestMessageUrl, type Transporter } from "nodemailer"
import type Mail from "nodemailer/lib/mailer"
import type SMTPConnection from "nodemailer/lib/smtp-connection"
import { CONSTANTS, HTTP, type CloudEventV1, type Message } from "cloudevents"
import { Buffer } from "node:buffer"
import { Fact, Journal, Tenancy } from "@rasm/data"
import { Crypto } from "@rasm/security"
import { Event, Fault, type Identity } from "@rasm/core"
import { Client, Lapse, WebhookOrigin } from "../net/client.ts"
import { Propagation } from "../otel/emit.ts"
import { Pulse } from "../otel/meter.ts"
import { Setting } from "../proc/config.ts"
import { Settled, WorkClass } from "./entity.ts"
import { Lane, LaneVerdict, Throttle } from "./queue.ts"

// Channel identity is ONE roster the settlement evidence, the refusal subject, and the dispatch table all read, so a
// new row cannot land while a second spelling still names the old pair. Each key IS its claim-tag namespace, so routing reads
// tag namespaces directly instead of a predicate every row re-spells, and the park fold agrees with it by grammar.
const _kinds = ["mail", "webhook"] as const
const _Kind = Schema.Literal(..._kinds)

// The channel's own evidence beside the work plane's spine: which channel crossed, the two acceptance bands the
// transport answered, and the SMTP connection's envelope span. Partition, provenance, warning band, and stamp pair
// are the carrier's, so a consumer reading a delivered message and a rendered document reads ONE settlement shape.
class Delivery extends Settled.extend<Delivery>("Deliver.Delivery")({
  evidence: Schema.Struct({
    channel: _Kind,
    accepted: Schema.Array(Schema.String),
    // the recipients the transport named, kept structured because the suppression tap addresses them one by one —
    // the warning band grades the same list and cannot carry an address a fact row has to target
    rejected: Schema.Array(Schema.Struct({ recipient: Schema.String, note: Schema.String })),
    // SMTP-only: the connection's own envelope time. A capture sink opened no envelope and states absence, because a
    // zero here reads identically to an instant handoff and the spine's `span` already times the settlement.
    envelope: Schema.optionalWith(Schema.Duration, { as: "Option" }),
  }),
}) {}

// Every refusal here is raised on one surface — the channel's own crossing, from the suppression gate through the
// projection to the wire — so the family carries one leg and partitions on its reason alone; no row decides on a
// surface its siblings cannot reach, and `refused` alone spans the gate, the signer, and the peer.
const _LEG = "channel"

// Every refusal names the same three operands — the channel it crossed, the destinations it was carrying, and the
// transport's own diagnostic — because the suppression tap addresses targets off the refusal and the park row
// renders it. What the rows do NOT share is the sentence, so each renders its own subject.
const _Subject = Schema.Struct({
  channel: _Kind,
  targets: Schema.Array(Schema.String),
  detail: Schema.String,
})

const _family = Fault.Class.family(["dial", "refused", "bounced", "quota", "timeout", "schema"] as const, {
  dial: Fault.Class.row({
    class: "unavailable",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail }) => `${channel} transport would not carry the message — ${detail}`,
  }),
  refused: Fault.Class.row({
    class: "denied",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail, targets }) => `${channel} refused ${Array.join(targets, ", ")} — ${detail}`,
  }),
  bounced: Fault.Class.row({
    class: "invalid",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail, targets }) => `${channel} rejected ${Array.join(targets, ", ")} — ${detail}`,
  }),
  // The ONE band whose producer states its own window: a receiver's `Retry-After` is a measured wait, so this reason
  // classes `exhausted` and its raise fills `after` rather than spending a curve nobody's peer asked for.
  quota: Fault.Class.row({
    class: "exhausted",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail, targets }) => `${channel} is over quota at ${Array.join(targets, ", ")} — ${detail}`,
  }),
  timeout: Fault.Class.row({
    class: "expired",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail }) => `${channel} outlived its budget — ${detail}`,
  }),
  schema: Fault.Class.row({
    class: "invalid",
    leg: _LEG,
    detail: _Subject,
    render: ({ channel, detail }) => `${channel} would not project onto the wire — ${detail}`,
  }),
})

class DeliverFault extends Schema.TaggedError<DeliverFault>()("DeliverFault", {
  case: _family.payload,
  // The stated window rides the VALUE under the one word `core/value/fault#CLASS_VOCABULARY` fixes, so
  // `Fault.Class.statedOf` reads exactly this field and `Fault.Budget.schedule` re-seats its base from it; every
  // row but `quota` states absence rather than a zero a drain would re-offer against immediately.
  after: Fault.Class.After,
}) {
  get class(): Fault.Class.Kind {
    return _family.classOf(this.case.reason)
  }
  get leg(): string {
    return _family.legOf(this.case.reason)
  }
  override get message(): string {
    return _family.render(this.case)
  }
}

// The page's one raise: a refusal names its reason and the operands its row renders, and states a window only where
// it MEASURED one — every other raise takes the absent arm rather than an invented zero that re-offers immediately.
// The parameter carries its own name because `case` binds nothing at that position; the FIELD keeps the estate word.
const _refuse = (refusal: Deliver.Case, after: Option.Option<Duration.Duration> = Option.none()): DeliverFault =>
  new DeliverFault({ case: refusal, after })

declare namespace Deliver {
  type Kind = (typeof _kinds)[number]
  type Reason = (typeof _family.kinds)[number]
  // The refusal subject a row renders: the reason discriminates and the operands come with it, so a raise cannot
  // present a reason without the evidence its own row was declared to read.
  type Case = typeof _family.payload.Type
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
  // Every outbox row IS an announced fact, so every channel receives the announcement its own claimed row projects
  // beside the payload it decoded — a channel with no use for it simply takes fewer parameters, and none re-derives
  // one from its payload column, which would announce a fact the journal never recorded.
  type Announcement = CloudEventV1<unknown>
  type Channel<A extends { readonly tenant: string }, I, R> = Descriptor & {
    readonly payload: Schema.Schema<A, I>
    readonly targets: (payload: A) => ReadonlyArray<string>
    readonly weight: (payload: A) => number
    // The re-drive price of THIS row's host crossing — the same column `Job.Spec`, `Actor.Spec`, and `Cadence.Policy`
    // already carry, so the judge reads a class the table elected rather than one the drain happened to be running.
    readonly clazz: WorkClass.Kind
    readonly transmit: (payload: A, announced: Announcement) => Effect.Effect<Delivery, DeliverFault, R>
  }
  type _Table<T extends Record<Kind, Channel<never, never, never>> = typeof _channels> = T
}

const _channel = <A extends { readonly tenant: string }, I, R>(row: Deliver.Channel<A, I, R>): Deliver.Channel<A, I, R> => row
```

## [03]-[MAIL_ROW]

- Owner: `Mailer` — the scoped transport service built from the `Setting.mail` row, whose `transport` discriminant selects the sink: `smtp` (the pooled production dial carrying pool geometry, LOGIN credential, and DKIM material), `json` and `stream` (the capture sinks that open no socket), and `ethereal` (the sandbox dial whose credential `createTestAccount()` mints inside the same acquisition). `verify()` proves the connection at acquisition on the dialing arms alone, `close()` releases at teardown, `isIdle()` gates each claim, and the transporter's `idle` event becomes `Mailer.wake` through one scoped stream bridge. Secrets arrive `Redacted` on `Setting` and unwrap only inside the `smtp` row's own builder.
- Law: the sink is a transport row, never a code path — `_transports` keys the builder and the `dials` column off `Setting.mail.transport`, so pool and DKIM options exist only where a socket does, a capture sink is a root-config choice rather than a conditional inside the service, and a new sink (SES, a provider adapter) is one row. Without it the relay cannot be exercised without live SMTP, so the acceptance-band fold and the suppression tap have no reachable proof.
- Law: one settlement fold reads every sink — `_Sent` is the widened band the arms share, `envelopeTime` and `rejectedErrors` riding the SMTP connection alone, so a captured send settles as a real `Delivery` with a deterministic `messageId` and an empty rejected band and a second fold never appears; `getTestMessageUrl(info)` is the ethereal arm's own inspection read, an operator affordance beside the settlement and never a column on it.
- Law: the message is one Schema — tenant, sender and recipient bands, reply threading, subject, plain/HTML/Watch/AMP/iCalendar alternatives, headers, priority, attachments, and the `list` block are fields of the channel payload decoded once. `_mailOptions` is the only conversion into `nodemailer`'s optional boundary shape; an untyped message object assembled at a call site is unspellable.
- Law: `list` is the six-key standard vocabulary, not one URL — `unsubscribe`, `help`, `subscribe`, `post`, `archive`, and `owner` each compose one interior `_ListEntry` field admitting either a bare URL or the `{ url, comment }` arm, and `_mailOptions` hands the decoded record through as `list` with absent keys dropped; the annotated arm is what nodemailer renders into the `List-Unsubscribe`/`List-Unsubscribe-Post` pair one-click unsubscribe requires, and the suppression fold's `regulatory` retention is retention OF that header, so a single string cannot express the evidence it claims to keep.
- Law: DKIM is native and mandatory on production rows — `domainName`/`keySelector`/`privateKey` ride the transport options so every message signs RFC-6376 in-transport; the security wave's HMAC domain never touches mail.
- Law: transport faults classify through the code table — `EAUTH` folds `refused` (terminal), a 4xx `responseCode` folds `dial` (transient — the lane's lease redelivers), a 5xx recipient failure folds `bounced` (the suppression fold consumes it); string-matching an error message is unspellable beside the table.
- Law: `isIdle()` and the `idle` event are the relay's pacing signal — the mail lane row reads pool capacity per claim and defers the claim onto its lease while the pool is saturated, so bulk sends respect pool geometry instead of queueing in the transport, and the `idle` event bridges into the wake race so a freed pool re-triggers the drain without waiting the lease width.
- Law: the `error` event binds inside the service scope — a pool fault arrives between sends where no claim owns it, and an emitter emitting `error` unlistened THROWS, so an unbound row converts a recoverable fault into a process death no verdict observes; it folds through `_classified` onto the log rail.
- Law: DSN rides the dialing row's transport options — `notify: ["FAILURE", "DELAY"]` with `ret: "HDRS"` requests RFC-3461 reports, because SMTP acceptance proves handoff alone and a late bounce reaches suppression only as an asynchronous report.
- Law: each transport row states its `degrade` — capture sinks forfeit wire evidence and the sandbox forfeits onward delivery, so a synthetic receipt reads as the evidence gap it is rather than as proof of delivery.
- Receipt: the interior `_Sent` band folds to the shared `Delivery` — `accepted`/`rejectedErrors` become the evidence bands and the warning band grading them, `messageId` the produced provenance, the announcement's own id the consumed provenance, `envelopeTime` the evidence's optional envelope span, and `Effect.timed` around the send the spine's own `span`, so the settlement times what this process measured and the connection's envelope stays a separate, absent-where-unopened fact. Nodemailer's top-level `SentMessageInfo` is `any`, so importing it erases the transport boundary under a confident receipt name.
- Growth: a provider, OAuth2, or inspect transport is one `_transports` row against one `Setting.mail.transport` value; a new message concern is one payload field and one `_mailOptions` projection.
- Packages: `nodemailer` (`createTransport`, `createTestAccount`, `getTestMessageUrl`, `Transporter`, `Mail.Address`, `Mail.ListHeader`); `effect` (`Duration`, `Layer`, `Option`, `Record`, `Redacted`); `../proc/config.ts` (`Setting`).

```typescript signature
// Every transport arm answers this band: `envelopeTime` and `rejectedErrors` ride the SMTP connection alone, so the
// widened shape is what lets ONE `_mailSettled` fold read a dialed send and a captured one without a second fold.
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
          // Sandbox credentials mint inside the same acquisition, so no environment row carries a throwaway secret
          try: () => createTestAccount(),
          catch: (cause) => _refuse({ reason: "dial", channel: "mail", targets: [], detail: String(cause) }),
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
        catch: (cause) => _refuse({ reason: "dial", channel: "mail", targets: [], detail: String(cause) }),
      })
      : Effect.void
    // `consumed` is the announcement identity this send SPENDS, threaded from the claimed row the relay projected —
    // the settlement's backward join, which a fold re-deriving it from the message body would have to invent.
    const send = (message: Parameters<Transporter["sendMail"]>[0], consumed: ReadonlyArray<string>) =>
      Effect.gen(function* () {
        // the spine's span is what THIS process measured around the transport call; the connection's own envelope
        // time is a separate SMTP-only fact that a capture sink honestly lacks
        const [span, info] = yield* Effect.timed(Effect.tryPromise({
          try: () => transporter.sendMail(message),
          catch: (cause) => _classified(cause),
        }))
        const at = yield* DateTime.now
        return yield* _mailSettled(info, { at, consumed, span })
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
  return _refuse({
    reason: code === "EAUTH" ? "refused" : status >= 500 ? "bounced" : "dial",
    channel: "mail",
    targets: admitted.recipient === undefined ? [] : [admitted.recipient],
    detail: `${code}:${status}`,
  })
}

// The rejected band is the transport's own evidence AND its grade: one fold mints both, so the suppression tap
// addresses recipients off the structured band while a consumer reads the dominant degradation off `degraded`
// without a second traversal. A send that accepted nobody is a refusal, never an `empty` settlement — the outbox
// row has work left to re-offer, which is the one thing an empty partition would tell every reader it does not.
const _mailSettled = (
  info: _Sent,
  stamp: { readonly at: DateTime.Utc; readonly consumed: ReadonlyArray<string>; readonly span: Duration.Duration },
): Effect.Effect<Delivery, DeliverFault> => {
  const accepted = Array.map(info.accepted, String)
  const rejected = Array.map(info.rejectedErrors ?? [], (fault) => ({ recipient: String(fault.recipient ?? ""), note: fault.response ?? "" }))
  return Array.isNonEmptyReadonlyArray(accepted)
    ? Effect.succeed(
      new Delivery({
        partition: Array.isNonEmptyReadonlyArray(rejected) ? "partial" : "whole",
        provenance: { consumed: stamp.consumed, produced: info.messageId },
        warnings: Array.map(rejected, (row) => ({
          class: _family.classOf("bounced"),
          reason: "bounced",
          note: `${row.recipient}: ${row.note}`,
        })),
        at: stamp.at,
        span: stamp.span,
        evidence: {
          channel: "mail",
          accepted,
          rejected,
          // a sink that never opened an envelope MEASURED no envelope time and says so, because a zero here reads
          // identically to an instant handoff and the spine's own span already carries what this process timed
          envelope: Option.map(Option.fromNullable(info.envelopeTime), Duration.millis),
        },
      }),
    )
    : Effect.fail(_refuse({
      reason: "bounced",
      channel: "mail",
      targets: Array.map(info.rejected, String),
      detail: "<all-rejected>",
    }))
}
```

## [04]-[HOOK_ROW]

- Owner: `Hook` — signed webhook egress under byte identity, carrying the projection, the validation handshake, and the transmit: the payload encodes to its wire bytes exactly once, the `Crypto` service signs THOSE bytes, and the transmission carries the v1 header triple — `webhook-id` (the deliverable identity — replay dedup on the receiving side), `webhook-timestamp` (the signing instant bounding replay windows), `webhook-signature` (`v1,<hex>` over `id.timestamp.body`) — so the receiver verifies the identical byte sequence and a re-serialization between sign and send is structurally impossible.
- Law: the HTTP leg is the branch client's zero-redirect `batch` row — its total budget, retry pacing, circuit, and proxy apply while the signed POST stays pinned to the addressed origin. Webhook's 200/201/202/204 statuses settle; every other 2xx, every 3xx, and 405 refuse; 404/410 bounce; 401/403 refuse; 408 times out. The client alone admits a protocol-valid 429 `Retry-After`; one without it remains a dial refusal.
- Law: endpoint registration is per-destination material resolved through `Hook.Registration` by the payload's non-secret `keyRef`; raw key bytes never enter the persisted outbox body, a receipt, or a fault. Security composition supplies the resolver and rotates the material behind a stable reference without rewriting queued work.
- Law: the target's granted rate is SPENT, never merely proved — `Hook.validate` reads `webhook-allowed-rate` off the grant and the registration seats it, so every delivery paces against `queue#THROTTLE`'s `webhookGrant` row before the wire and the ceiling this sender accepted is one the receiver can hold it to; a `*` grant states no ceiling, seats no rate, and reaches the row never.
- Law: content mode is an OWNED literal row — `Mode` is a TypeScript `enum` this branch cannot declare, so `_hookBindings` keys `binary`/`structured` to the package's own two serializers and the enum value crosses nowhere else.
- Law: `Hook.project` is the relay's OWN step and runs at claim time over the announcement `data:journal/append#RELAY_ROWS` projects from the same claimed row — so the stored draft carries destination and signing material alone, a binding change re-frames every queued row, and no enqueue stores transport framing it must keep in step with a binding it never reached; projected binding headers and content type enter the projection exactly once, and `_signed` transmits the same detached octets that projection produced.
- Law: `HookRow` is ONE field record the variant axis projects twice — the `draft` the enqueue stores and the `payload` the claim projects — so the framed octets and their media type restrict to the projected variant at the field and a second hand-spelled struct restating the shared eight columns is unspellable; the class decodes the default `draft` and carries the projection as its same-name static, which `Hook.row` alone exports.
- Law: `body` crosses as base64, so the projected variant decodes from a JSON column exactly as it decodes from a projection — a `FromSelf` byte field arrives out of neither, which is what made a pre-projected outbox payload structurally unreadable at the claim seam.
- Law: the transport triple is the sole webhook signature and binds this hop's exact octets; no CloudEvents extension claims a second signature or publishes an unverifiable attribute preimage.
- Law: abuse protection is the specification's `OPTIONS` validation request and `Hook.validate` is its sender half. The DNS origin is independent of the HTTPS destination; the grant accepts that origin or `*` and requires the paired allowed-rate row. Missing or malformed grant evidence refuses registration, the origin rides every POST, and the granted rate lands on the registration the deliveries pace against.
- Law: every delivery POST uses `Client.authorized`, so the audience-specific `MachinePrincipal` stamps its own authorization scheme or the delivery refuses before I/O. The exact-body signature remains independent integrity evidence and cannot substitute for receiver authorization.
- Law: destinations are HTTPS and every POST body is non-empty — the row's destination schema refuses every other scheme, the direct validation entry crosses the same schema, and projection refuses an SDK frame with no payload instead of emitting a Webhook request the binding specification forbids.
- Boundary: the announcement is `data:journal/append#RELAY_ROWS`'s projection and its grammar `core:interchange/carrier#EVENT_ENVELOPE`'s; this row seals, frames, signs, and transmits it and invents no envelope dialect. Framing (`content-type`, `content-length`, `transfer-encoding`) and the signature triple are reserved names the header-band admission refuses, so `payload.media` alone mints the outbound content type and a caller cannot smuggle contradictory framing beside it.
- Growth: a signing-scheme revision is a new version prefix beside `v1` in the same header; a destination policy axis (mTLS, custom header band) is a field on the destination row.
- Packages: `cloudevents` (`HTTP`, `CONSTANTS`), `@effect/experimental` (`VariantSchema.make`, `Class`, `FieldOnly`; `RateLimiter` — the store Tag the grant row spends), `@effect/platform` (`Headers`, `HttpBody`, `HttpClientRequest`, `HttpClientResponse`), `effect` (`Duration`, `Number`, `Record`), `@rasm/core` (`Event`), `@rasm/security` (`Crypto`), and `../net/client.ts`.

```typescript signature
// Single-sourced framing: media alone mints the content type, the signer alone mints the triple, and the abuse-
// protection origin rides the row below — a caller band re-spelling any of them refuses at the admission filter.
const _RESERVED: ReadonlyArray<string> = [
  CONSTANTS.HEADER_CONTENT_TYPE, "content-length", "transfer-encoding",
  "webhook-id", "webhook-timestamp", "webhook-signature", "webhook-request-origin",
]

const _band = Schema.optionalWith(
  Schema.Record({ key: Schema.String, value: Schema.String }).pipe(
    Schema.filter((band) => Object.keys(band).every((name) => !_RESERVED.includes(name.toLowerCase())), {
      message: () => "framing, signature, and origin headers are reserved — media mints the content type, the signer mints the triple",
    }),
  ),
  { default: () => ({}) },
)

// `Mode` is a TypeScript `enum` this branch cannot declare, so content mode is an OWNED literal row and the package's
// two serializers cross at this one table rather than an enum value travelling every signature.
const _hookModes = ["binary", "structured"] as const
const _hookBindings = { binary: HTTP.binary, structured: HTTP.structured } as const
const _WebhookUrl = Schema.URL.pipe(
  Schema.filter((url) =>
    url.protocol === "https:"
    && url.username === ""
    && url.password === ""
    && url.hash === ""
    && !url.searchParams.has("access_token"), {
    message: () => "webhook URLs require HTTPS and forbid embedded credentials, access tokens, and fragments",
  }),
)

// ONE decoded truth the variant axis projects twice: the outbox stores the `draft` — destination, signing reference,
// claimed origin, content mode — and never transport framing an enqueue must keep in step with a binding it never
// reached, while the relay's own claim-time step lands the `payload`. The announced fact is the JOURNAL's projection
// over the same claimed row, so a binding change re-frames every queued row and no enqueue ever stored a re-encoding
// whose float forms, key order, and escapes the journal itself never wrote.
const _hookVariants = VariantSchema.make({ variants: ["draft", "payload"], defaultVariant: "draft" })

class HookRow extends _hookVariants.Class<HookRow>("HookRow")({
  tenant: Schema.NonEmptyString,
  destination: _WebhookUrl,
  deliverable: Schema.NonEmptyString,
  origin: WebhookOrigin,
  mode: Schema.Literal(..._hookModes),
  headers: _band,
  keyRef: Schema.NonEmptyString,
  weight: Schema.Number.pipe(Schema.int(), Schema.positive()),
  // Projection alone mints the wire pair, so the field record subtracts both from the stored draft by declaration.
  // `body` crosses as base64 rather than a live `Uint8Array`, so the projected variant decodes from a JSON column
  // exactly as it decodes from a projection — a `FromSelf` byte field arrives out of neither, which is what made a
  // pre-projected payload unreadable at the claim seam.
  body: _hookVariants.FieldOnly("payload")(Schema.Uint8ArrayFromBase64),
  media: _hookVariants.FieldOnly("payload")(Schema.NonEmptyString),
}) {}

declare namespace Deliver {
  type Registration = { readonly key: Redacted.Redacted<Uint8Array>; readonly rate: Option.Option<number> }
  type HookMode = (typeof _hookModes)[number]
  type HookDraft = typeof HookRow.Type
  type HookPayload = typeof HookRow.payload.Type
}

const _hookUtf8 = new TextEncoder()

const _messageBody = (message: Message, target: string): Effect.Effect<Uint8Array, DeliverFault> =>
  message.body === undefined
    ? Effect.fail(_refuse({ reason: "schema", channel: "webhook", targets: [target], detail: "<empty-webhook-body>" }))
    : typeof message.body === "string"
      ? pipe(_hookUtf8.encode(message.body), (body) => body.byteLength === 0
        ? Effect.fail(_refuse({ reason: "schema", channel: "webhook", targets: [target], detail: "<empty-webhook-body>" }))
        : Effect.succeed(body))
      : message.body instanceof Uint8Array
        ? message.body.byteLength === 0
          ? Effect.fail(_refuse({ reason: "schema", channel: "webhook", targets: [target], detail: "<empty-webhook-body>" }))
          : Effect.succeed(new Uint8Array(message.body))
        : Effect.fail(_refuse({
          reason: "schema", channel: "webhook", targets: [target], detail: "<message-envelope-body>",
        }))

const _hookProject = (
  envelope: CloudEventV1<unknown>,
  draft: Deliver.HookDraft,
): Effect.Effect<Deliver.HookPayload, DeliverFault> =>
  Effect.flatMap(
    Effect.try({
      try: () => _hookBindings[draft.mode](envelope),
      catch: (cause) => _refuse({
        reason: "schema", channel: "webhook", targets: [draft.destination.toString()], detail: String(cause),
      }),
    }),
    (message) =>
      pipe(message.headers[CONSTANTS.HEADER_CONTENT_TYPE], (media) => typeof media !== "string"
        ? Effect.fail(_refuse({
          reason: "schema", channel: "webhook", targets: [draft.destination.toString()], detail: "<message-envelope-content-type>",
        }))
        : Effect.map(_messageBody(message, draft.destination.toString()), (body) => ({
          tenant: draft.tenant,
          destination: draft.destination,
          deliverable: draft.deliverable,
          origin: draft.origin,
          mode: draft.mode,
          keyRef: draft.keyRef,
          weight: draft.weight,
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

// One registration read per delivery, because a destination's registration holds TWO facts that rotate together —
// signing material beside the rate that endpoint's own `OPTIONS` grant stated — and a second port keyed by the same
// reference lets them rotate apart. `rate` is absent where the target granted `*`, the specification's own "no stated
// ceiling", never a zero a sender would read as a stop.
class _HookRegistry extends Context.Tag("runtime/work/Hook/Registration")<_HookRegistry, {
  readonly resolve: (keyRef: string) => Effect.Effect<Deliver.Registration, DeliverFault>
}>() {}

const _signable = (id: string, stamp: string, body: Uint8Array): Uint8Array => {
  // BOUNDARY ADAPTER: byte-join kernel — the draft detaches immutable at the return
  const prefix = _hookUtf8.encode(`${id}.${stamp}.`)
  const joined = new Uint8Array(prefix.length + body.length)
  joined.set(prefix)
  joined.set(body, prefix.length)
  return joined
}

const _hookLapseReasons = {
  binding: "refused",
  break: "dial",
  budget: "timeout",
  credential: "refused",
  throttled: "quota",
} as const satisfies Record<Lapse["case"]["reason"], Deliver.Reason>

const _hookAccepted: ReadonlyArray<number> = [200, 201, 202, 204]

const _hookLapse = (fault: Lapse, target: string): Effect.Effect<never, DeliverFault> => {
  const reason = _hookLapseReasons[fault.case.reason]
  return Effect.fail(_refuse(
    { reason, channel: "webhook", targets: [target], detail: fault.message },
    reason === "quota" ? Fault.Class.statedOf(fault) : Option.none(),
  ))
}

// `consumed` is the announcement identity this hop SPENDS: the relay's own transmit threads the claimed row's
// announcement, while a direct `Hook.transmit` spent none and says so rather than joining the payload to itself.
const _signed = (payload: Deliver.HookPayload, key: Redacted.Redacted<Uint8Array>, consumed: ReadonlyArray<string>) =>
  Effect.gen(function* () {
    const crypto = yield* Crypto
    const signedAt = yield* DateTime.now
    const stamp = String(Math.trunc(DateTime.toEpochMillis(signedAt) / 1000))
    const signed = yield* crypto.sign(key, _signable(payload.deliverable, stamp, payload.body)).pipe(
      Effect.mapError((fault) =>
        _refuse({ reason: "refused", channel: "webhook", targets: [payload.destination.toString()], detail: fault.reason })
      ),
    )
    return yield* Effect.timed(Effect.scoped(Client.authorized(
      "batch",
      HttpClientRequest.post(payload.destination.toString()).pipe(
        HttpClientRequest.setHeaders({
          ...payload.headers,
          // Every delivery rides the claimed origin, not the one validation exchange: a target re-reads it per
          // message, so a rotated or revoked sender identity refuses on the next request rather than at re-handshake.
          "webhook-request-origin": payload.origin,
          "webhook-id": payload.deliverable,
          "webhook-timestamp": stamp,
          "webhook-signature": `v1,${signed}`,
        }),
        HttpClientRequest.setBody(HttpBody.uint8Array(payload.body, payload.media)),
      ),
    ))).pipe(
      // The request round trip IS this settlement's span, so the receipt states a duration this hop measured; the
      // `webhook-id` doubles as the produced identity because that word is what the receiver dedups on.
      Effect.flatMap(([span, response]) =>
        Array.contains(_hookAccepted, response.status)
          ? Effect.map(DateTime.now, (at) =>
          new Delivery({
            partition: "whole",
            provenance: { consumed, produced: payload.deliverable },
            warnings: [],
            at,
            span,
            evidence: {
              channel: "webhook",
              accepted: [payload.destination.toString()],
              rejected: [],
              // no envelope exists off SMTP, so this arm states absence rather than a span belonging to another wire
              envelope: Option.none(),
            },
          }))
          : _hookSettle(response, payload.destination.toString())
      ),
      Effect.catchTags({
        ResponseError: (fault) => _hookSettle(fault.response, payload.destination.toString()),
        RequestError: () =>
          Effect.fail(_refuse({
            reason: "dial", channel: "webhook", targets: [payload.destination.toString()], detail: "<transport>",
          })),
        Lapse: (fault) => _hookLapse(fault, payload.destination.toString()),
      }),
    )
  })

// Quota STORE faults measure no window, and this row's `transmit` closes its channel on `DeliverFault`, so that
// arm folds to the transient `dial` refusal whose lease IS the wait — the identical posture the relay's own store arm
// takes, spelled here because a foreign tag reaching `transmit` would arrive with no row to render it.
const _paced = <A, R>(
  granted: Deliver.Registration,
  draft: { readonly tenant: string; readonly destination: URL; readonly weight: number },
  self: Effect.Effect<A, DeliverFault, R>,
): Effect.Effect<A, DeliverFault, R | Fleet.RateLimiter> =>
  Option.match(granted.rate, {
    onNone: () => self,
    onSome: (rate) =>
      Throttle.pace(Throttle.webhookGrant, {
        tenant: draft.tenant,
        destination: draft.destination.toString(),
        weight: draft.weight,
        rate,
      })(self).pipe(
        Effect.catchTag("RateLimiterError", (fault) =>
          Effect.fail(_refuse({
            reason: "dial", channel: "webhook", targets: [draft.destination.toString()], detail: fault.message,
          }))),
      ),
  })

// The stable key reference resolves at transmission, so rotation changes the material behind queued drafts without
// rewriting them or changing the byte-identical signature input.
const _hook = (payload: Deliver.HookPayload) =>
  Effect.flatMap(_HookRegistry, (registry) =>
    Effect.flatMap(registry.resolve(payload.keyRef), (granted) =>
      _paced(granted, payload, _signed(payload, granted.key, []))))

// This is the channel's own transmit and the whole webhook order: resolve the material once, recover the
// announcement, project the wire payload through the mode's binding, and sign the encoded octets exactly once. Every
// step composes an owner — the admitted envelope, the package's binding, and the security wave's signer — so this row
// adds routing and no second signature dialect.
const _hookDeliver = (draft: Deliver.HookDraft, announced: Deliver.Announcement) =>
  Effect.gen(function* () {
    const registry = yield* _HookRegistry
    const granted = yield* registry.resolve(draft.keyRef)
    const payload = yield* _hookProject(announced, draft)
    // Registration ACCEPTED the target's own ceiling, so this delivery spends that grant before the wire over
    // whichever store every other quota rides; a grant proved once and never spent is a promise the receiver has no
    // way to hold this sender to, and the handshake exists for nothing else.
    return yield* _paced(granted, draft, _signed(payload, granted.key, [announced.id]))
  })

// Abuse protection is the specification's own validation request and this member is its sender half: the target
// answers a grant or 405, and a refusing target is a registration verdict rather than a delivery fault, so the
// roster never queues work for an endpoint that already declined the origin.
const _hookGrant = (
  response: HttpClientResponse.HttpClientResponse,
  destination: string,
  claimed: string,
): Effect.Effect<{ readonly origin: string; readonly rate: "*" | number }, DeliverFault> =>
  Option.match(Headers.get(response.headers, "webhook-allowed-origin"), {
    onNone: () => Effect.fail(_refuse({
      reason: "refused", channel: "webhook", targets: [destination], detail: "<webhook-consent-absent>",
    })),
    onSome: (origin) => {
      if (origin !== "*" && origin.toLowerCase() !== claimed.toLowerCase()) return Effect.fail(_refuse({
        reason: "refused", channel: "webhook", targets: [destination], detail: "<webhook-origin-mismatch>",
      }))
      return Option.match(Headers.get(response.headers, "webhook-allowed-rate"), {
        onNone: () => Effect.fail(_refuse({
          reason: "refused", channel: "webhook", targets: [destination], detail: "<webhook-rate-absent>",
        })),
        onSome: (stated) => stated === "*"
          ? Effect.succeed({ origin, rate: stated })
          : Option.match(
            Option.filter(Number.parse(stated), (rate) => globalThis.Number.isInteger(rate) && rate > 0),
            {
              onNone: () => Effect.fail(_refuse({
                reason: "refused", channel: "webhook", targets: [destination], detail: "<webhook-rate-invalid>",
              })),
              onSome: (rate) => Effect.succeed({ origin, rate }),
            },
          ),
      })
    },
  })

const _hookValidate = (destination: URL, origin: string) =>
  Effect.all({ destination: Schema.decodeUnknown(_WebhookUrl)(destination), origin: Schema.decodeUnknown(WebhookOrigin)(origin) }).pipe(
    Effect.mapError((issue) => _refuse({
      reason: "refused", channel: "webhook", targets: [destination.toString()], detail: issue.message,
    })),
    Effect.flatMap((admitted) => Client.dial(
      "batch",
      HttpClientRequest.options(admitted.destination.toString()).pipe(
        HttpClientRequest.setHeaders({ "webhook-request-origin": admitted.origin }),
      ),
    )),
    Effect.scoped,
    Effect.flatMap((response) => _hookGrant(response, destination.toString(), origin)),
    Effect.catchTags({
      ResponseError: (fault) => _hookSettle(fault.response, destination.toString()),
      RequestError: () =>
        Effect.fail(_refuse({
          reason: "dial", channel: "webhook", targets: [destination.toString()], detail: "<transport>",
        })),
      Lapse: (fault) => _hookLapse(fault, destination.toString()),
    }),
  )

// Status is a ROW LOOKUP with one default, never a ternary ladder whose arm order decides the answer: each listed
// status names the posture it earns and everything else rides the lease as a transient dial refusal. Keys are the
// decimal spelling and the record annotates its key as `string`, because a status is a runtime coordinate rather
// than a member of a closed vocabulary and no call site should cast to reach its own table.
const _hookStatuses: Record.ReadonlyRecord<string, Deliver.Reason> = {
  "401": "refused",
  "403": "refused",
  "404": "bounced",
  "405": "refused",
  "408": "timeout",
  "410": "bounced",
}

// `Client` already consumed every 429 carrying a protocol-valid `Retry-After` and raised a typed throttled lapse.
// Reaching this fold means the response stated no admissible window, so it remains a dial refusal; parsing the header
// again here would fork the grammar and make HTTP-date windows unreachable.
const _hookSettle = (
  response: HttpClientResponse.HttpClientResponse,
  target: string,
): Effect.Effect<never, DeliverFault> =>
  Effect.fail(_refuse(
    {
      reason: response.status >= 200 && response.status < 400
        ? "refused"
        : Option.getOrElse(Record.get(_hookStatuses, String(response.status)), () => "dial" as const),
      channel: "webhook",
      targets: [target],
      detail: String(response.status),
    },
    Option.none(),
  ))
```

## [05]-[SUPPRESSION]

- Owner: the shared suppress-by-evidence fold — both channels feed it and both consult it. Either a `bounced`-reasoned fault or a settlement's rejected evidence band appends one `deliver.suppressed` fact row (recipient or destination as target, the channel and note as change rows, `regulatory` retention for mail — the unsubscribe evidence — and `operational` for webhooks); `Deliver.admissible(suppressed)(channel, targets)` folds the channel row's projected targets over the suppression read the data wave serves and answers before any `transmit` — the relay's lane rows compose it between lane admission and the wire, so a suppressed destination structurally cannot reach a transport effect, and a direct send outside the relay composes the same gate at its own seam.
- Law: a suppressed target refuses the whole deliverable — the gate fails `refused` (`denied` class), the lane's poison short-circuit parks it on first refusal with the suppressed target as evidence, and replay after reinstatement is the one path back; a silently narrowed recipient list erases the evidence the park row carries.
- Law: suppression is append-only history — reinstatement is a `deliver.reinstated` fact, and the projection folds the pair; deleting suppression evidence is unrepresentable.
- Law: the unsubscribe seam is one-way — the serving plane's unsubscribe endpoint appends the same fact shape; this fold never mounts a route.
- Growth: a suppression cause (complaint feedback loop, manual block) is one action verb on the same fact shape.
- Packages: `@rasm/data` (`Fact`); `effect` (`Effect`, `Option`).

```typescript signature
const _admissible = <R>(suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R>) =>
(channel: Deliver.Kind, targets: ReadonlyArray<string>): Effect.Effect<void, DeliverFault, R> =>
  Effect.findFirst(targets, (target) => suppressed(channel, target)).pipe(
    Effect.flatMap(Option.match({
      onNone: () => Effect.void,
      onSome: (target) => Effect.fail(_refuse({
        reason: "refused", channel, targets: [target], detail: `<suppressed:${target}>`,
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

// The structured band is what makes this tap addressable: the warning band beside it grades the same rejects for a
// consumer, but only the evidence rows name the recipient a suppression fact has to target.
const _settled = (sent: Delivery) =>
  Effect.forEach(sent.evidence.rejected, (row) => _suppress(sent.evidence.channel, row.recipient, row.note), {
    discard: true,
  })
```

## [06]-[RELAY]

- Law: the announcement has ONE owner and this pass is its consumer — `Journal.Deliverable.envelope` mints it from the claimed row under the drain's live context, so a channel never re-derives a fact from its own payload column and a row that will not project parks as its own claim rather than failing the pass; the claim reaches the projection as the lane's own `meta`, so no batch-keyed index stands between a claim and the announcement it already carries.
- Owner: `Relay` — the one outbox drain: a `Singleton.make` (exactly one live instance cluster-wide, migrating on rebalance) whose pass fires on the merged wake stream — the journal's NOTIFY pulse handed in as the data-owned `wake` parameter, merged with the lease-width tick — claims a batch through `Journal.claimBatch` sized and leased by the `bulk` class row, and settles it through `Lane.settle` over the relay's lane rows: each row is `Lane.row(channel.payload, …)` composing the fixed sequence suppression gate → tenant throttle → `channel.transmit` → rejected-band suppression tap, so the drain body is route and composition, with zero retry, backoff, decode, or dead-letter machinery of its own.
- Law: every transmission passes one suppression decision — the gate sits inside the lane row between admission and the wire, so no route reaches `transmit` without it; a refused deliverable parks with the suppressed target as evidence through the lane's poison short-circuit.
- Law: quota precedes transmission — `Throttle.spend` runs before the wire and its exceeded posture is the durable delay, so a tenant's burst paces the drain inside the lease width instead of converting into provider-side rejections; a lease that expires mid-delay redelivers, attempts already incremented, and a quota-STORE fault (`RateLimiterError`) defers `unavailable` with no stated window, because both throttle arms delay on exhaustion and a broken counter measured nothing.
- Law: the verdict's class comes off the channel ROW and the pass's fan-out off the DRAIN — `Lane.judge` grades each refusal under `row.clazz` while `Lane.settle`'s concurrency and the claim's lease width stay the relay's `bulk`, so one batch spanning both channels re-drives each on the budget its own host crossing earns.
- Law: pacing composes the mail pool — the mail lane row reads `Mailer.idle` per claim and defers while the pool reports no capacity, so mail never queues inside the transport and webhook claims drain regardless of pool state; the pool publishes an idle EVENT rather than a window, so that deferral states no wait and `Mailer.wake` frees the claim on the pulse.
- Law: the wake source is data-owned — the drain subscribes the journal's wake stream through the scope port; a poll loop or a second LISTEN binding here is unspellable.
- Law: the claim rides the MAINTENANCE plane — one relay drains every tenant of an app, so `Journal.claimBatch` composes `Tenancy.sweep` per `queue#LANE_POLICY`'s posture law: unpinned it claims zero rows under FORCE RLS and each pass reports a healthy empty cycle over an aging backlog, and a `Tenant.within`-opened pass claims one tenant exclusively while every other tenant's deliverables lapse behind re-leased claims; the sweep transaction closes at the claim statement, so the settle fold, the transmits, and the meter fold all run outside it.
- Law: the pass budget grades on `Journal.retryable`, never on the class default — every fault the pass raises is a store fault the journal already projects onto the shared class table, so a connection blip re-drives inside the tick and an undecodable claim batch refuses; accepting the property grader parks the whole shard's outbox on the first blip while the compiled budget records nothing.
- Receipt: each pass folds `Lane.settle`'s outcome roster into one `deliver.drained` meter fact — claims, settled, deferred, parked, and REFUSED, the discharges the store's own fence turned down — and marks the settled count onto the `Pulse` throughput counter in the same fold, so the OTel series and the journal fact cannot disagree on a pass. Throughput stays the decision count because a refused fence redelivers the row, which the at-least-once law already admits; a pass folding the two into one number reports a mark it never landed as delivered.
- Growth: a second relay concern (a per-region drain, a channel-partitioned drain) is a second singleton row over the same fold with a claim predicate — the drain body never forks.
- Packages: `@effect/cluster` (`Singleton`); `@effect/sql` (`SqlClient`, `SqlError`); `@rasm/data` (`Journal`, `Fact`, `Tenancy`); `@rasm/core` (`Fault.Budget`, `Fault.Class`); `./entity.ts` (`WorkClass`); `./queue.ts` (`Lane`, `LaneVerdict`, `Throttle`); `../otel/emit.ts` (`Propagation`); `../otel/meter.ts` (`Pulse`).

```typescript signature
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
  // Six standard list keys render into List-* headers, each admitting the annotated arm; the
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
  // Decoded records pass straight through: every present key is one ListHeader and every absent key omits
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
    admit: "<deliver-subject-outbox-claim-decoded-through-MailPayload>",
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
    // The pooled dial IS bulk geometry — the same row prices `maxConnections` above — so a saturated pool and a 4xx
    // relay both re-drive on the deep budget the pool was sized against.
    clazz: "bulk",
    // The announcement never reaches the wire here — this transport frames its message from the decoded fields — but
    // its identity is what the settlement SPENT, so the row consumes the id and lets the body alone stay unused.
    transmit: (message, announced) =>
      Effect.flatMap(Mailer, (mailer) => mailer.send(_mailOptions(message), [announced.id])),
  }),
  webhook: _channel({
    fits: "<machine-callback-to-a-tenant-registered-endpoint-under-byte-identity-signing>",
    admit: "<deliver-subject-outbox-claim-decoded-through-HookRow's-draft-variant>",
    tenancy: "<per-payload-tenant-field-and-per-destination-keyRef:-one-tenant's-secret-signs-only-its-own-endpoint>",
    lifetime: "<ends-at-the-receiver-2xx-which-the-receiver-itself-issues>",
    deliver: "<at-least-once-past-2xx:-a-redrive-repeats-the-event-id-and-the-receiver-owns-the-dedup>",
    order: "<none:-skip-locked-claiming-drains-unordered-and-HookRow-carries-no-key-selecting-a-domain>",
    settle: "<the-receiver's-200/201/202/204-status-alone;-the-response-body-is-never-read-as-evidence,-and-the-OPTIONS-grant-settles-registration-not-delivery>",
    replay: "<queue#LANE_POLICY-re-offers-a-parked-row-under-its-stable-webhook-id,-so-the-receiver's-dedup-absorbs-it>",
    bound: "<queue#THROTTLE-tenantEgress-and-webhookGrant-plus-the-Client-lane's-http-concurrency;-this-row-spells-no-ceiling>",
    refuse: "<a-reason-classed-DeliverFault-folded-from-the-response-status;-nothing-returns-once-the-request-closes>",
    degrade: "<no-ack-past-2xx:-a-receiver-answering-then-dropping-the-work-reads-identically-to-one-that-kept-it>",
    payload: HookRow,
    targets: (draft) => [draft.destination.toString()],
    weight: (draft) => draft.weight,
    // A signed POST is a request-latency crossing, not a pooled batch, so this row takes the steady budget and its
    // shallower park ceiling; the loss is three attempts the receiver's own dedup (`replay` above) would discard.
    clazz: "steady",
    transmit: _hookDeliver,
  }),
} as const

// Routing reads the ANNOUNCED grammar through its one owner: `queue#PARK_REPLAY`'s `Lane.channel` reads `<subject>`
// off the claim tag `data:journal/append#RELAY_ROWS` mints as the envelope's own `type`, so `rasm.deliver.webhook.queued.v1`
// routes here and fans the park series there under one reading. Re-splitting the tag beside that owner is how a
// routing predicate and a metric dimension come to disagree about what a channel is.
const _routed = (tag: string): Option.Option<Deliver.Kind> =>
  Option.flatMap(Lane.channel(tag), (subject) => Array.findFirst(_kinds, (kind) => kind === subject))

const _sent = <A extends { readonly tenant: string }, I, R, R2>(
  kind: Deliver.Kind,
  row: Deliver.Channel<A, I, R>,
  suppressed: (channel: Deliver.Kind, target: string) => Effect.Effect<boolean, never, R2>,
) =>
(payload: A, meta: Lane.Meta<Journal.Deliverable>, announced: Deliver.Announcement) =>
  _admissible(suppressed)(kind, row.targets(payload)).pipe(
    Effect.zipRight(Throttle.spend(Throttle.tenantEgress, {
      tenant: payload.tenant,
      channel: kind,
      weight: row.weight(payload),
    })),
    Effect.zipRight(row.transmit(payload, announced)),
    Effect.tap(_settled),
    Effect.as(LaneVerdict.Settled()),
    Effect.tapErrorTag("DeliverFault", (fault) =>
      fault.case.reason === "bounced"
        ? Effect.forEach(fault.case.targets, (target) => _suppress(kind, target, fault.message), { discard: true })
        : Effect.void),
    Effect.catchTags({
      // The WHOLE fault crosses: the judge reads its class and detail the same either way, and passing it entire is
      // what lets `Fault.Class.statedOf` recover the receiver's own `Retry-After` a rebuilt pair would have dropped.
      // The class comes off the ROW, so mail's pooled dial and the webhook's POST re-drive on their own budgets.
      DeliverFault: (fault) => Effect.succeed(Lane.judge(meta, row.clazz, fault)),
      // The quota STORE failed, not the quota — `queue#THROTTLE`'s two arms both DELAY on exhaustion, so nothing
      // here measured a window and the lease is the wait. Classing this `exhausted` would claim a window the store
      // never named and hand the lane a wait no producer took.
      RateLimiterError: () =>
        Effect.succeed(LaneVerdict.Deferred({
          class: "unavailable",
          route: Fault.Class.reofferOf("unavailable"),
          after: Option.none(),
        })),
    }),
  )

// A pass answers TWO facts per claim and neither count carries the other: the VERDICT is what the drain decided, the
// FENCE is what the store did with the discharge that decision asked for. `refused` counts the discharges the store
// turned down — a lapsed lease a sibling already displaced, an identity the groom took — so it crosses the two
// discharging verdicts rather than replacing either, and a relay reading settled off the verdicts alone reports a
// mark it never landed as delivered. Throughput stays the DECISION count, because a refused fence redelivers the
// row and the effect is at-least-once by declared law.
const _VERDICTS = ["Settled", "Deferred", "Parked"] as const

const _tallied = (outcomes: ReadonlyArray<Lane.Outcome>) => ({
  ...Record.fromEntries(Array.map(
    _VERDICTS,
    (tag) => [tag, Array.filter(outcomes, (outcome) => outcome.verdict._tag === tag).length] as const,
  )),
  refused: Array.filter(
    outcomes,
    (outcome) => Option.match(outcome.discharge, { onNone: () => false, onSome: (fence) => fence._tag !== "Advanced" }),
  ).length,
})

const _metered = (claims: number, outcomes: ReadonlyArray<Lane.Outcome>) =>
  pipe(_tallied(outcomes), (tally) =>
    Effect.zipRight(
      Pulse.mark("drained", "deliver", tally.Settled),
      Fact.record({
        action: "deliver.drained",
        actor: { key: "relay", kind: "service" },
        change: [
          { _tag: "Assigned", path: "/claims", next: String(claims) },
          { _tag: "Assigned", path: "/settled", next: String(tally.Settled) },
          { _tag: "Assigned", path: "/deferred", next: String(tally.Deferred) },
          { _tag: "Assigned", path: "/parked", next: String(tally.Parked) },
          // the discharge the STORE turned down: evidence a fold reading the statement's silence reports as delivered
          { _tag: "Assigned", path: "/refused", next: String(tally.refused) },
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
    // Claims widen to the app's whole estate only under the stated plane pin; the sweep transaction closes here,
    // so the lease predicate — not a held lock — guards the claims while the lanes transmit.
    const claims = yield* Tenancy.sweep(sql)(Journal.claimBatch(sql, {
      app,
      take: WorkClass.bulk.concurrency * 4,
      leaseSeconds: Duration.toSeconds(Fault.Budget.at("bulk").attempt),
    }))
    // Claimed rows own their announcement and this pass consumes it: each row projects under the drain's live
    // context, so a delivered body is the fact the journal recorded rather than a lane's re-derivation from its own
    // payload column, and a row that will not project parks as its own claim rather than failing the pass. The claim
    // reaches the projection through the lane's own `meta`, so this pass builds no batch-keyed index and carries no
    // absent-row arm that the fold it indexes could never reach.
    const context = yield* Propagation.current
    const projected = <A extends { readonly tenant: string }, I, R>(
      kind: Deliver.Kind,
      row: Deliver.Channel<A, I, R>,
    ) =>
    (payload: A, meta: Lane.Meta<Journal.Deliverable>) =>
      Effect.matchEffect(meta.envelope(context), {
        // a row that will not project is this CHANNEL's refusal, so it re-drives on the channel's own class exactly
        // as a transport refusal does rather than on whichever class the drain happens to run under
        onFailure: (fault) => Effect.succeed(Lane.judge(meta, row.clazz, fault)),
        onSuccess: (announcement) => _sent(kind, row, suppressed)(payload, meta, announcement),
      })
    const lanes = {
      mail: Lane.row(_channels.mail.payload, (message, meta) =>
        Effect.flatMap(mailer.idle, (idle) =>
          idle
            ? projected("mail", _channels.mail)(message, meta)
            // The pool publishes an idle EVENT and no window, so this re-offer states no wait: `mailer.wake` already
            // merges into the drain's wake race, which frees the claim on the pulse rather than on a guessed duration.
            : Effect.succeed(LaneVerdict.Deferred({
              class: "exhausted",
              route: Fault.Class.reofferOf("exhausted"),
              after: Option.none(),
            })))),
      webhook: Lane.row(_channels.webhook.payload, projected("webhook", _channels.webhook)),
    } as const
    // The DRAIN's own class stays `bulk` — it prices this fold's fan-out and the lease width one batch spans across
    // both channels — while each claim's re-drive priced off its channel row above; conflating the two would pin
    // every webhook to the pool's geometry.
    const outcomes = yield* Lane.settle(sql, "bulk", (tag) => Option.map(_routed(tag), (kind) => lanes[kind]), Lane.park)(claims)
    yield* _metered(claims.length, outcomes)
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
  Origin: WebhookOrigin,
  Registration: _HookRegistry,
  modes: _hookModes,
  project: _hookProject,
  row: HookRow,
  transmit: _hook,
  validate: _hookValidate,
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Deliver, DeliverFault, Delivery, Hook, Mailer, Relay }
```

## [07]-[RESEARCH]

(none)
