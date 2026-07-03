# [WORK_MAIL]

Mail egress is one scoped transporter, one decoded message, and one policy spine: `createTransport` is a single polymorphic factory whose option shape selects the modality — plain SMTP, a pooled connection set, or the capture sink kit-driven specs and dry-runs use — so the `Mailer` service holds one transport row per environment, verifies the credential at build, and drains its pool on teardown. The message is rendered, never assembled: templates are locale-keyed render rows the app passes as values (message catalogs never ride a `ui` import), the locale fold walks exact tag, language head, then the catalog's default, and the rendered body lands in one `SendMailOptions` value whose unsubscribe headers come from the `list` row. Suppression is a port: the ledger Tag this page declares is satisfied at the app root by `store` journal Layers, every send consults it first, and a hard bounce records back into it — the compensation arm of a mail saga. Every secret is `Redacted` end to end; `SMTPError` codes classify through the folder fault convention so only transient deferrals re-drive; and a partial rejection is receipt evidence to reconcile, never a thrown failure. A module-level transporter, an untyped inline message, and a live SMTP dial inside a deterministic spec are the named defects.

## [1]-[INDEX]

| [INDEX] | [CLUSTER]        | [OWNS]                                                                 |
| :-----: | :--------------- | :------------------------------------------------------------------------ |
|  [01]   | [MESSAGE_RENDER] | the message shape, locale-keyed template rows, the suppression port        |
|  [02]   | [TRANSPORT_SEND] | transport rows, the scoped service, send classification, the receipt       |

## [2]-[MESSAGE_RENDER]

[MESSAGE_RENDER]:
- Owner: `Mailer.Message` and the template law. The message is one Schema class — recipient addresses, the template key, the kernel-branded `Refined.Locale`, and a flat string model the render row interpolates — decoded once at the caller's seam so no untyped `to`/`subject` assembly exists. A catalog is data: `Mailer.Catalog` maps template key to locale-keyed render rows, each row a pure `(model) => { subject, text, html? }` arrow, and the fold resolves exact locale, then the language head, then the catalog's declared default — three arms, one `Option` pipeline, total by the default row's existence.
- Law: catalogs arrive as app-passed values — the service takes the catalog at construction, so localization content lives with the app that owns its copy deck; a `ui` import for message text would couple the node egress lane to the browser bundle, the named inversion.
- Law: the suppression port is declared here and satisfied elsewhere — `Mailer.Suppress` is one Tag: `suppressed(address)` answers the pre-send check, `record(address, reason)` appends bounce/complaint/unsubscribe evidence — and the app root binds it to a `store` journal Layer because the ledger keeps `work → store` absent; the port's fault is absorbed to the safe side (an unreadable ledger suppresses nothing but logs loudly).
- Law: unsubscribe is a header row, not prose — the `list` value on the outbound options carries `List-Unsubscribe` (and its one-click sibling) derived from the message's unsubscribe URL field, so compliance is construction, never template discipline.
- Boundary: the locale brand is `kernel`'s `Refined.Locale`; who serves the unsubscribe endpoint is `edge`'s; this page owns the shape, the fold, and the port contract.
- Entry: `Mailer.render(catalog, message)` — the pure render fold; `Mailer.Suppress` at the root.
- Growth: a new template is one catalog row; a new suppression cause is one literal on the port's reason union.
- Packages: `effect` (`Context`, `Effect`, `Option`, `Schema`), `@rasm/ts/kernel` (`Refined`).

```typescript
import { type FaultClass, Refined } from "@rasm/ts/kernel"
import { Array, Config, Context, DateTime, Effect, Either, Option, Predicate, Redacted, Schema } from "effect"

class _Message extends Schema.Class<_Message>("MailMessage")({
  to: Schema.NonEmptyArray(Schema.NonEmptyString),
  cc: Schema.optionalWith(Schema.Array(Schema.NonEmptyString), { default: () => [] }),
  template: Schema.NonEmptyString,
  locale: Refined.Locale,
  model: Schema.Record({ key: Schema.String, value: Schema.String }),
  unsubscribe: Schema.optionalWith(Schema.String, { as: "Option" }),
}) {}

declare namespace Mailer {
  type Rendered = { readonly subject: string; readonly text: string; readonly html?: string }
  type Render = (model: Record<string, string>) => Rendered
  type Catalog = {
    readonly fallback: string
    readonly rows: Record<string, Record<string, Render>>
  }
  type SuppressReason = "bounced" | "complained" | "unsubscribed"
}

class _Suppress extends Context.Tag("work/deliver/Suppress")<_Suppress, {
  readonly suppressed: (address: string) => Effect.Effect<boolean>
  readonly record: (address: string, reason: Mailer.SuppressReason) => Effect.Effect<void>
}>() {}

const _render = (catalog: Mailer.Catalog, message: _Message): Option.Option<Mailer.Rendered> =>
  Option.flatMap(Option.fromNullable(catalog.rows[message.template]), (locales) =>
    Option.map(
      Option.fromNullable(locales[message.locale]).pipe(
        Option.orElse(() => Option.fromNullable(locales[message.locale.split("-")[0] ?? message.locale])),
        Option.orElse(() => Option.fromNullable(locales[catalog.fallback])),
      ),
      (render) => render(message.model),
    ),
  )
```

## [3]-[TRANSPORT_SEND]

[TRANSPORT_SEND]:
- Owner: `Mailer` — the scoped egress service, a Layer factory over the transport row and the catalog. Transport rows are one closed axis the option shape of `createTransport` discriminates: `smtp` (the direct dial assembled from `Config` reads with the credential `Config.redacted`), `pool` (the same dial with `pool: true`, `maxConnections`, `maxMessages` — bulk-send backpressure as construction policy), `capture` (`streamTransport: true, buffer: true` — the deterministic sink for kit-driven specs and dry-runs, no socket ever dialed). Construction is a bracket: acquire builds the transporter and proves the credential with `verify()` (capture rows skip the probe), release drains the pool through `close()`.
- Law: `send` is one pipeline — suppression filter over every recipient (all-suppressed short-circuits to the suppressed receipt arm), the render fold (a missing template row is the `template` reason, terminal), one `sendMail` under `Effect.tryPromise`, and the receipt fold splitting `accepted`/`rejected` per recipient with `messageId` and timing — so partial rejection is evidence the durable job reconciles, and only a transport-level failure enters the fault channel.
- Law: the transporter is pinned at the declaration — the root `SentMessageInfo` erases to `any`, so the binding states `Transporter<Mailer.Sent>` over the structural receipt view this page consumes, and the any never crosses the seam.
- Law: `SMTPError` classifies through the folder convention — `EAUTH` is `auth` (`denied`, terminal), a dial/socket failure is `dial` (`unavailable`, re-drives), a `4xx` response code is `deferred` (`exhausted`, re-drives under the kernel gate), a `5xx` is `bounced` (`invalid`, terminal and recorded into the suppression ledger by the caller's saga arm), and everything else is `refused` (`defect`) — the probe reads `code`/`responseCode` structurally, never string-matches messages.
- Law: secrets ride `Redacted` to the unwrap point — the DSN, and through it the credential, unwrap only inside the transport build; no receipt, fault, or log carries them.
- Law: a mail send inside a workflow is `Step.run({ budget: "lease", execute: Mailer.send(message) })` with the hard-bounce compensation recording suppression — the saga pair at the workflow top level per `flow/durable.md`.
- Boundary: DKIM signing is transport-native policy configured on the row (`dkim` keys from `Config.redacted`), distinct from `security/sign` domain HMAC and never merged; attachment bytes arrive from `deliver/report.md` as values.
- Entry: `Mailer.Default({ transport: "pool", catalog })` at the root; `yield* Mailer.send(message)` on the rail.
- Growth: a new transport environment is one row; a new refusal cause is one policy row inherited by every gate.
- Packages: `nodemailer` (`createTransport`, `SendMailOptions`, `Transporter`), `effect` (`Array`, `Config`, `DateTime`, `Effect`, `Either`, `Option`, `Predicate`, `Redacted`, `Schema`), `@rasm/ts/kernel` (`FaultClass`).

```typescript
import { createTransport, type SendMailOptions, type Transporter } from "nodemailer"

const _transports = ["smtp", "pool", "capture"] as const
const _reasons = ["auth", "dial", "deferred", "bounced", "template", "refused"] as const
const _policy = {
  auth: { class: "denied" },
  dial: { class: "unavailable" },
  deferred: { class: "exhausted" },
  bounced: { class: "invalid" },
  template: { class: "invalid" },
  refused: { class: "defect" },
} as const

class _Fault extends Schema.TaggedError<_Fault>()("MailFault", {
  reason: Schema.Literal(..._reasons),
  template: Schema.String,
  detail: Schema.String,
}) {
  get policy(): (typeof _policy)[_Fault["reason"]] {
    return _policy[this.reason]
  }
  get class(): FaultClass.Kind {
    return _policy[this.reason].class
  }
  override get message(): string {
    return `<mail:${this.reason}> ${this.template} ${this.detail}`
  }
}

class _Receipt extends Schema.Class<_Receipt>("MailReceipt")({
  template: Schema.NonEmptyString,
  accepted: Schema.Array(Schema.String),
  rejected: Schema.Array(Schema.String),
  suppressed: Schema.Array(Schema.String),
  messageId: Schema.optionalWith(Schema.String, { as: "Option" }),
  at: Schema.DateTimeUtc,
}) {}

declare namespace Mailer {
  type Transport = (typeof _transports)[number]
  type Fault = _Fault
  type Receipt = _Receipt
  type Message = _Message
  type Reason = keyof typeof _policy
  type Sent = {
    readonly accepted?: ReadonlyArray<string | { readonly address: string }>
    readonly rejected?: ReadonlyArray<string | { readonly address: string }>
    readonly messageId: string
  }
  type _Rows<T extends Record<(typeof _reasons)[number], { readonly class: FaultClass.Kind }> = typeof _policy> = T
  type _Keys<K extends (typeof _reasons)[number] = Reason> = K
}

const _code = (cause: unknown): _Fault["reason"] =>
  Predicate.hasProperty(cause, "code") && cause.code === "EAUTH"
    ? "auth"
    : Predicate.hasProperty(cause, "responseCode") && Predicate.isNumber(cause.responseCode)
      ? cause.responseCode >= 500 ? "bounced" : "deferred"
      : "dial"

const _outbound = (message: _Message, rendered: Mailer.Rendered, live: ReadonlyArray<string>): SendMailOptions => ({
  to: [...live],
  cc: [...message.cc],
  subject: rendered.subject,
  text: rendered.text,
  ...(rendered.html !== undefined && { html: rendered.html }),
  ...Option.match(message.unsubscribe, {
    onNone: () => ({}),
    onSome: (url) => ({ list: { unsubscribe: { url, comment: "unsubscribe" } } }),
  }),
})

const _dial = Effect.map(
  Config.unwrap({
    host: Config.string("MAIL_HOST").pipe(Config.withDescription("<smtp-host>")),
    port: Config.port("MAIL_PORT").pipe(Config.withDescription("<smtp-port>")),
    user: Config.string("MAIL_USER").pipe(Config.withDescription("<smtp-user>")),
    pass: Config.redacted("MAIL_PASS").pipe(Config.withDescription("<smtp-credential>")),
  }),
  ({ host, pass, port, user }) => ({ host, port, secure: true, auth: { user, pass: Redacted.value(pass) } }),
)

class Mailer extends Effect.Service<Mailer>()("work/deliver/Mailer", {
  scoped: (options: { readonly transport: Mailer.Transport; readonly catalog: Mailer.Catalog }) =>
    Effect.gen(function* () {
      const suppress = yield* _Suppress
      const transporter: Transporter<Mailer.Sent> = yield* Effect.acquireRelease(
        Effect.gen(function* () {
          const built = options.transport === "capture"
            ? createTransport({ streamTransport: true, buffer: true })
            : createTransport({
                ...(yield* _dial),
                ...(options.transport === "pool" && { pool: true, maxConnections: 4, maxMessages: 100 }),
              })
          yield* Effect.when(
            Effect.tryPromise({
              try: () => built.verify(),
              catch: (cause) => new _Fault({ reason: _code(cause), template: "<verify>", detail: String(cause) }),
            }),
            () => options.transport !== "capture",
          )
          return built
        }),
        (built) => Effect.sync(() => built.close()),
      )
      return {
        send: (message: _Message): Effect.Effect<_Receipt, _Fault> =>
          Effect.gen(function* () {
            const verdicts = yield* Effect.forEach(
              message.to,
              (address) => Effect.map(suppress.suppressed(address), (hit) => [address, hit] as const),
              { concurrency: "inherit" },
            )
            const [suppressed, live] = Array.partitionMap(verdicts, ([address, hit]) =>
              hit ? Either.left(address) : Either.right(address))
            const rendered = yield* Option.match(_render(options.catalog, message), {
              onNone: () => new _Fault({ reason: "template", template: message.template, detail: message.locale }),
              onSome: Effect.succeed,
            })
            const info = yield* Effect.when(
              Effect.tryPromise({
                try: () => transporter.sendMail(_outbound(message, rendered, live)),
                catch: (cause) => new _Fault({ reason: _code(cause), template: message.template, detail: String(cause) }),
              }),
              () => Array.isNonEmptyReadonlyArray(live),
            )
            const at = yield* DateTime.now
            return new _Receipt({
              template: message.template,
              accepted: Option.match(info, {
                onNone: () => [],
                onSome: (sent) => Array.map(sent.accepted ?? [], (entry) => Predicate.isString(entry) ? entry : entry.address),
              }),
              rejected: Option.match(info, {
                onNone: () => [],
                onSome: (sent) => Array.map(sent.rejected ?? [], (entry) => Predicate.isString(entry) ? entry : entry.address),
              }),
              suppressed,
              messageId: Option.map(info, (sent) => sent.messageId),
              at,
            })
          }),
      }
    }),
  accessors: true,
}) {
  static readonly Suppress = _Suppress
  static readonly render = _render
}

// --- [EXPORTS] --------------------------------------------------------------------------

export { Mailer }
```
