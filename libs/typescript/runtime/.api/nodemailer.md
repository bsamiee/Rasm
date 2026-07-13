# [TS_RUNTIME_API_NODEMAILER]

`nodemailer` is the SMTP/mail transport `runtime/src/work/deliver.ts` internalizes as the single mail-egress owner: one polymorphic `createTransport` that discriminates on the transport-option shape — plain SMTP, a pooled SMTP connection set, AWS SESv2, `sendmail`, an in-memory `stream`, or a `json` sink — returning a `Transporter` whose `sendMail` sends one RFC-5322 message and whose `verify` proves the connection. The message is one rich `Options` shape (`from`/`to`/`cc`/`bcc`/`replyTo`, `subject`, `text`/`html`/`amp`/`watchHtml`, `icalEvent`, `attachments`/`alternatives`, `headers`, `list` for `List-Unsubscribe`, `dkim`, `priority`, `disableFileAccess`/`disableUrlAccess`), and every send returns a `SentMessageInfo` receipt (`accepted`/`rejected`/`rejectedErrors`/`messageId`/`response`/`envelopeTime`). Authentication is a discriminated union — `LOGIN` credentials, `OAUTH2` via the built-in `XOAuth2` refresh-token flow, or a `CUSTOM` handler — DKIM message signing is native (`domainName`/`keySelector`/`privateKey`), pooling carries `maxConnections`/`maxMessages`/`rateLimit`/`rateDelta`, and `wellKnown` resolves a provider name to host/port. The library is callback-and-`Promise`-based; the owner wraps it once: `Effect.tryPromise` lifts `sendMail`/`verify`, every secret (`pass`, `clientSecret`, `refreshToken`, DKIM `privateKey`) arrives as `Redacted` from `Config.redacted`, the `Transporter` is a scoped `Layer` service closed on teardown, and a durable mail job retries on `SMTPError.code` classification through a `Schedule` budget.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nodemailer`
- package: `nodemailer` (MIT-0 — no attribution clause, © Andris Reinman); declarations via `@types/nodemailer` (MIT, DefinitelyTyped)
- module format: CJS (`main: lib/nodemailer.js`, no `module`/`exports` map); named factory exports (`createTransport`, `createTestAccount`, `getTestMessageUrl`); transport classes are internal, reached through `createTransport`
- runtime target: Node-only — binds `node:net`/`node:tls`/`node:stream`/`node:dns` for SMTP; no browser build, no native addon. SESv2 is structural (`SES: { sesClient, SendEmailCommand }`), so `@aws-sdk/client-sesv2` is an optional app dependency, never bundled
- peer/asset: runtime has zero npm dependencies; the DKIM, XOAuth2, and MIME machinery are in-tree. `@types/node` is a transitive type dependency
- rail: mail egress (folder-tier; internalized once at `runtime/src/work/deliver.ts`)

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transporter, the message, and the send receipt
- rail: boundaries

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]   | [CONSUMER]                                                                  |
| :-----: | :----------------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Transporter<T, D>` = `Mail<T, D>`   | egress service  | the `EventEmitter` egress owner `mail` provides; methods in `[03]`          |
|  [02]   | `Mail.Options` (= `SendMailOptions`) | message shape   | the one message contract; every part in `[NODEMAILER_TOPOLOGY]`             |
|  [03]   | `Mail.Address`                       | address value   | `{ name?, address }` — structured recipient; escapes `Name <email>`         |
|  [04]   | `Mail.Attachment`                    | attachment      | the attachment shape (fence); where a `report`/jszip byte artifact attaches |
|  [05]   | `Mail.ListHeaders` / `Mail.Headers`  | header shape    | `list` builds `List-Unsubscribe`/`List-Help` — the suppression seam         |
|  [06]   | `SentMessageInfo`                    | send receipt    | delivery evidence (fence); `accepted`/`rejected` split per the intro        |
|  [07]   | `Transport<T, D>`                    | plugin contract | `{ name, version, send, verify?, close? }` — a custom egress backend        |

[PUBLIC_TYPE_SCOPE]: connection, authentication, and signing policy
- rail: system-apis

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CONSUMER]                                                 |
| :-----: | :----------------------------------------------- | :---------------- | :--------------------------------------------------------- |
|  [01]   | `SMTPConnection.Options`                         | connection policy | the SMTP dial policy (fence) — host/port/TLS/timeouts/auth |
|  [02]   | `SMTPPool.Options`                               | pool policy       | `pool: true`; bulk-send concurrency + rate limits          |
|  [03]   | `AuthenticationType` (`LOGIN`/`OAUTH2`/`CUSTOM`) | auth union        | the discriminated credential; secrets held as `Redacted`   |
|  [04]   | `XOAuth2.Options` / `XOAuth2.Token`              | oauth flow        | the refresh-token flow (fence); `token` grant              |
|  [05]   | `DKIM.Options`                                   | signing policy    | native RFC-6376 message signing (fence)                    |
|  [06]   | `SMTPConnection.DSNOptions`                      | delivery notify   | RFC-3461 delivery-status request (fence)                   |
|  [07]   | `SMTPConnection.SMTPError` / `shared.Logger`     | fault / log       | `code` (e.g. `EAUTH`) retry discriminant; `Logger` sink    |

```ts signature
interface Attachment {                                          // one of content|path, plus MIME framing
  content?: string | Buffer | Readable; path?: string; raw?: string | Buffer | Readable
  cid?: string; contentType?: string; contentTransferEncoding?: string; encoding?: string; contentDisposition?: string
}
interface SMTPConnectionOptions {                               // the SMTP dial policy
  host?: string; port?: number; secure?: boolean; requireTLS?: boolean; opportunisticTLS?: boolean; ignoreTLS?: boolean
  tls?: object; auth?: AuthenticationType; authMethod?: string; lmtp?: boolean
  connectionTimeout?: number; greetingTimeout?: number; socketTimeout?: number; dnsTimeout?: number
}
interface XOAuth2Options {                                      // refresh-token flow; secrets held as Redacted
  clientId?: string; clientSecret?: string; refreshToken?: string; accessToken?: string; accessUrl?: string; privateKey?: string
}
interface DKIMKeyOptions {                                      // DKIM.Options = SingleKeyOptions | MultipleKeysOptions (RFC-6376)
  domainName: string; keySelector: string; privateKey: string  // PEM, held as Redacted
  hashAlgo?: string; headerFieldNames?: string; skipFields?: string; cacheDir?: string; cacheTreshold?: number
}
interface DSNOptions { notify?: "SUCCESS" | "FAILURE" | "DELAY" | "NEVER"; ret?: string; envid?: string; orcpt?: string }
interface SentMessageInfo {                                     // SMTP | SES | stream variants
  accepted: Address[]; rejected: Address[]; rejectedErrors?: Error[]; pending?: Address[]
  messageId: string; response: string; envelope: object; envelopeTime: number; messageTime: number; messageSize: number
}
interface SMTPError extends Error { code?: string; response?: string; responseCode?: number; command?: string }
```

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transport, send, and verify
- rail: boundaries

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CONSUMER]                                                     |
| :-----: | :---------------------------------------------------- | :------------- | :------------------------------------------------------------- |
|  [01]   | `createTransport(options, defaults?)` → `Transporter` | construct      | one polymorphic factory; option shape selects it               |
|  [02]   | `transporter.sendMail(message, cb?)` → `Promise<T>`   | send           | the durable-job egress — `Effect.tryPromise`, `Schema`-encoded |
|  [03]   | `transporter.verify(cb?)` → `Promise<true>`           | preflight      | connection/credential probe at `Layer` build                   |
|  [04]   | `transporter.close()` / `transporter.isIdle()`        | lifetime       | `close` drains pool sockets; `isIdle` gates the outbox relay   |
|  [05]   | `transporter.use(step, plugin)` → `this`              | plugin         | `"compile"`/`"stream"` message-mutation step — header stamp    |
|  [06]   | `transporter.setupProxy(url)`                         | proxy          | route SMTP egress through an HTTP/SOCKS proxy                  |
|  [07]   | `transporter.set("proxy_handler_socks5"\|…, fn)`      | proxy          | register a `proxy_handler_*` socket handler                    |
|  [08]   | `transporter.on("idle"\|"error"\|"token", cb)`        | events         | `idle`/`error`/`token` events → `Queue`/`SubscriptionRef`      |

[ENTRYPOINT_SCOPE]: provider resolution, OAuth2, and test sinks
- rail: system-apis

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [CONSUMER]                                             |
| :-----: | :----------------------------------------------------------- | :-------------- | :----------------------------------------------------- |
|  [01]   | `wellKnown(key)` → `SMTPConnection.Options \| false`         | provider lookup | resolves `"Gmail"`/`"SendGrid"` aliases to the dial    |
|  [02]   | `shared.parseConnectionUrl(url)`                             | url decode      | decodes a `smtp://…` `Config` value to options         |
|  [03]   | `XOAuth2#getToken` / `#generateToken` / `#buildXOAuth2Token` | oauth token     | the refresh → access-token flow behind `OAUTH2` auth   |
|  [04]   | `DKIM#sign(input, extraOptions?)` → `PassThrough`            | dkim sign       | native signing; `Redacted` PEM key from `Config`       |
|  [05]   | `createTestAccount(apiUrl?)` → `Promise<TestAccount>`        | test sink       | Ethereal capture — a real SMTP inbox, no live delivery |
|  [06]   | `getTestMessageUrl(info)` → `string \| false`                | test sink       | the preview URL for a captured message                 |
|  [07]   | `streamTransport`/`jsonTransport` options                    | inspect sink    | raw MIME or JSON — spec/dry-run sinks                  |

## [04]-[IMPLEMENTATION_LAW]

[NODEMAILER_TOPOLOGY]:
- `createTransport` is one polymorphic factory: the option shape selects the transport — `pool: true` → pooled SMTP, `SES: {...}` → SESv2, `streamTransport`/`jsonTransport` → inspect sinks, `sendmail: true` → local binary, otherwise plain SMTP (a string URL or `Options`). The owner exposes one transport policy row per environment, never a factory per backend.
- The message is one shape decoded once: `Mail.Options` carries every part — bodies, alternatives, attachments, headers, `list`, `dkim`, `envelope`. The owner defines one `Schema` for the outbound message, decodes the caller payload at ingress, and encodes to `Options` at the send seam so `to`/`from`/`subject` are never assembled untyped.
- Authentication is a discriminated union, secrets are `Redacted` end to end: `LOGIN` (`user`/`pass`), `OAUTH2` (`XOAuth2` refresh flow), and `CUSTOM` are `_tag`-style arms. `pass`, `clientSecret`, `refreshToken`, and the DKIM `privateKey` flow as `Redacted` from `Config.redacted`, unwrapped with `Redacted.value` only at the `createTransport`/`sign` call — never logged, never in a receipt.
- The receipt is delivery evidence: `SentMessageInfo` splits `accepted`/`rejected`/`rejectedErrors`/`pending` with `messageId` and timing. A partial rejection (some recipients accepted, some not) is a domain outcome the durable job records and reconciles, not a thrown failure.
- Retry classifies on `SMTPError.code`: `EAUTH` is terminal (no retry), a `4xx` `responseCode` is transient (retry under a `Schedule`), a `5xx` is a permanent bounce. The owner maps the error code to a `Data.taggedEnum` and drives `Effect.retry`/`catchTag` off the tag rather than string-matching messages.
- The transporter is a scoped resource: pooled connections and OAuth2 timers live for the `Layer`'s `Scope`; `close()` is the finalizer, `isIdle()`/the `idle` event are the outbox-relay backpressure signal, and `verify()` at build time proves the credential before the first send.

[STACKS_WITH]:
- `effect` (`../../.api/effect.md`): `Effect.tryPromise` lifts `sendMail`/`verify`; `Layer.scoped(Mailer, build)` owns the `Transporter` with `close` as the release; `Config.redacted`/`Redacted` carry every secret; `Schema.decodeUnknown` decodes the outbound message; `SMTPError.code` becomes a `Data.taggedEnum` driving `Effect.retry(Schedule.exponential)`; the `idle`/`token` events bridge to a `SubscriptionRef`/`Queue`; `Effect.withSpan` tags the send with the `messageId` receipt.
- `@effect/platform` (`../../.api/effect-platform.md`): a `report`/jszip artifact attaches as `{ content: Uint8Array }` or `{ path }`; `FileSystem.stream` feeds a large attachment as a `Readable`; `PlatformConfigProvider.layerDotEnv` behind `Config` supplies the SMTP DSN and DKIM key without a `process.env` read.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): nodemailer is Node-bound, so the mail `Layer` composes under `NodeContext.layer`; `NodeStream.toReadable` converts an Effect `Stream<Uint8Array>` into the `Readable` an attachment or `raw` body expects.
- `jspdf` + `exceljs` + `papaparse` + `jszip` (`./jspdf.md`, `./exceljs.md`, `./papaparse.md`, `./jszip.md`): the `work/report` byte producers — a PDF/XLSX/CSV `Uint8Array` (one output-format policy row over the same decoded rows) or a `jszip` bundle attaches as `Mail.Attachment` `{ content: bytes, contentType }`; nodemailer transports what the report format arms and the archive container produce, never rendering a document itself.
- `security` (`../../security/.api/`): domain HMAC egress signing (webhook receipts) is `security/sign`'s concern; nodemailer owns RFC-6376 DKIM message signing natively — the two are distinct and never merged. `jose` may mint the OAuth2 assertion, but the SMTP `XOAuth2` refresh flow stays in-transport.
- `@effect/workflow` + `@effect/cluster` (`./effect-workflow.md`, `./effect-cluster.md`): a send is a durable `Activity` with a compensation arm (record a suppression on hard bounce); the `work/deliver` entity fences per-tenant send quota, and the outbox relay drains queued messages under the `isIdle` pool signal.

[LOCAL_ADMISSION]:
- Use one `createTransport` policy row discriminated by option shape and one `Schema`-decoded `Mail.Options`; never a factory per transport or an untyped message object assembled inline.
- Use `Config.redacted`/`Redacted` for `pass`/`clientSecret`/`refreshToken`/DKIM `privateKey`, unwrapping only at the transport/sign call; never inline a secret in `Options` or a log.
- Use `Layer.scoped` with `close()` as the finalizer and `verify()` at build; never a module-level transporter singleton or an unclosed pool.
- Use `SMTPError.code` classified into a `Data.taggedEnum` for `Effect.retry`; never string-match the error `message` or retry a terminal `EAUTH`.
- Use `SentMessageInfo.rejected`/`rejectedErrors` as a domain outcome the durable job reconciles; never treat a partial-rejection send as a total success or a thrown failure.
- Use `streamTransport`/`jsonTransport` + `createTestAccount` for kit-driven specs/dry-run; never send through a live SMTP transport in a deterministic test.

[RAIL_LAW]:
- Package: `nodemailer` (+ `@types/nodemailer`)
- Owns: mail egress — the polymorphic `createTransport`, `sendMail`/`verify`/`close`/`isIdle`, the one `Mail.Options` message shape, the `LOGIN`/`OAUTH2`/`CUSTOM` auth union, native DKIM signing, SMTP pooling with rate limits, `wellKnown` provider resolution, and the `SentMessageInfo` delivery receipt
- Accept: `Effect.tryPromise`-lifted send/verify, a scoped `Layer` transporter with `close` release, `Redacted`/`Config.redacted` secrets, one `Schema`-decoded message, `SMTPError.code` as a `Data.taggedEnum` retry discriminant, partial-rejection reconciliation, `streamTransport`/`jsonTransport` for kit-driven specs
- Reject: a factory per transport, an untyped inline message, secrets outside `Redacted`, a module-level transporter singleton, string-matched error retry, treating partial rejection as success, live SMTP in a deterministic test
