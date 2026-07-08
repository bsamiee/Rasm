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

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `Transporter<T, D>` = `Mail<T, D>` | egress service | the `EventEmitter` owner — `sendMail`, `verify`, `close`, `isIdle`, `use`; the `Layer`-wrapped service `mail` provides |
| [02] | `Mail.Options` (= `SendMailOptions`) | message shape | the one message contract — addresses, `subject`, `text`/`html`/`amp`/`watchHtml`, `icalEvent`, `attachments`, `alternatives`, `headers`, `list`, `dkim`, `priority`, `envelope`, `disableFileAccess`/`disableUrlAccess` |
| [03] | `Mail.Address` | address value | `{ name?, address }` — the structured recipient; Nodemailer escapes `Name <email>` formatting |
| [04] | `Mail.Attachment` | attachment | `content` (`string`/`Buffer`/`Readable`) or `path`; `cid` for inline images, `contentType`, `contentTransferEncoding`, `encoding`, `contentDisposition`, `raw` — where a `report`/jszip byte artifact attaches |
| [05] | `Mail.ListHeaders` / `Mail.Headers` | header shape | the `list` key builds `List-Unsubscribe`/`List-Help` — the suppression/unsubscribe seam `mail` rows against |
| [06] | `SentMessageInfo` (`SMTP`/`SES`/`stream` variants)| send receipt | `accepted`/`rejected`/`rejectedErrors`/`pending`, `messageId`, `response`, `envelope`, `envelopeTime`/`messageTime`/`messageSize` — the durable-job delivery evidence |
| [07] | `Transport<T, D>` | plugin contract | `{ name, version, send, verify?, close? }` — the custom-transport interface for a bespoke egress backend |

[PUBLIC_TYPE_SCOPE]: connection, authentication, and signing policy
- rail: system-apis

| [INDEX] | [SYMBOL] | [TYPE_FAMILY] | [CONSUMER] |
|:-----: |:------------------------------------------------ |:---------------- |:--------------------------------------------------------------------- |
| [01] | `SMTPConnection.Options` | connection policy | `host`/`port`/`secure`/`requireTLS`/`opportunisticTLS`/`ignoreTLS`, `tls`, `auth`, `connectionTimeout`/`greetingTimeout`/`socketTimeout`/`dnsTimeout`, `authMethod`, `lmtp` — the SMTP dial policy |
| [02] | `SMTPPool.Options` | pool policy | `pool: true`, `maxConnections`, `maxMessages`, `rateLimit`, `rateDelta` — bounded concurrency + shared rate limit for bulk sends |
| [03] | `AuthenticationType` (`LOGIN`/`OAUTH2`/`CUSTOM`) | auth union | the discriminated credential — `Credentials { user, pass }`, `XOAuth2.Options`, or a `CustomAuthenticationHandlers` map; secrets held as `Redacted` |
| [04] | `XOAuth2.Options` / `XOAuth2.Token` | oauth flow | `clientId`/`clientSecret`/`refreshToken`/`accessToken`/`accessUrl`/`privateKey`; the `token` event carries a refreshed `{ user, accessToken, expires }` |
| [05] | `DKIM.Options` (`SingleKeyOptions`/`MultipleKeysOptions`) | signing policy | `domainName`, `keySelector`, `privateKey` (PEM), `hashAlgo`, `headerFieldNames`/`skipFields`, `cacheDir`/`cacheTreshold` — native RFC-6376 message signing |
| [06] | `SMTPConnection.DSNOptions` | delivery notify | `notify` (`SUCCESS`/`FAILURE`/`DELAY`/`NEVER`), `ret`, `envid`, `orcpt` — RFC-3461 delivery-status requests |
| [07] | `SMTPConnection.SMTPError` / `shared.Logger` | fault / log | `code` (e.g. `EAUTH`), `response`, `responseCode`, `command` — the retry-classification discriminant; `Logger` is the `trace`→`fatal` sink swapped for `structlog`/`telemetry` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transport, send, and verify
- rail: boundaries

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `createTransport(options, defaults?)` → `Transporter` | construct | one polymorphic factory — the option shape (`pool`/`SES`/`streamTransport`/`jsonTransport`/`sendmail`/SMTP) selects the modality; built once inside `Layer.scoped` |
| [02] | `transporter.sendMail(message, cb?)` → `Promise<T>` | send | the durable-job egress — `Effect.tryPromise`-lifted; the `message` is `Schema`-encoded, secrets pre-resolved from `Redacted` |
| [03] | `transporter.verify(cb?)` → `Promise<true>` | preflight | connection/credential probe at `Layer` build or a health activity; fails into the `Effect` channel on `SMTPError` |
| [04] | `transporter.close()` / `transporter.isIdle()` | lifetime | `close` is the scoped-`Layer` finalizer draining pool sockets; `isIdle` gates the outbox relay against free pool slots |
| [05] | `transporter.use(step, plugin)` → `this` / `transporter.setupProxy(url)` / `transporter.set("proxy_handler_socks5"\|…, fn)` | plugin / proxy | `"compile"`/`"stream"` message-mutation steps — a canonical header/footer or tenant-tag stamp; `setupProxy`/the `proxy_handler_*` setters route SMTP egress through an HTTP/SOCKS proxy |
| [06] | `transporter.on("idle"\|"error"\|"token", cb)` | events | pool backpressure (`idle`), transport failure (`error`), and OAuth2 refresh (`token`) bridged to a `Queue`/`SubscriptionRef` |

[ENTRYPOINT_SCOPE]: provider resolution, OAuth2, and test sinks
- rail: system-apis

| [INDEX] | [SURFACE] | [ENTRY_FAMILY] | [CONSUMER] |
|:-----: |:-------------------------------------------------------------------------------------------- |:-------------- |:--------------------------------------------------------------- |
| [01] | `wellKnown(key)` → `SMTPConnection.Options \| false` | provider lookup | resolves `"Gmail"`/`"SendGrid"`/an alias to host/port/secure so `mail` names a provider instead of hardcoding the dial |
| [02] | `shared.parseConnectionUrl(url)` → `SMTPConnection.Options` | url decode | decodes a `smtp://user:pass@host:port` `Config` value into structured options at the boundary |
| [03] | `XOAuth2#getToken(renew, cb)` / `#generateToken(cb)` / `#buildXOAuth2Token(accessToken)` | oauth token | the refresh-token → access-token flow behind `OAUTH2` auth; the owner caches the `token` event value in a `Ref` under `Redacted` |
| [04] | `DKIM#sign(input, extraOptions?)` → `PassThrough` | dkim sign | native message signing; keys arrive as `Redacted` PEM from `Config`, never inline in the message |
| [05] | `createTestAccount(apiUrl?)` → `Promise<TestAccount>` / `getTestMessageUrl(info)` → `string \| false` | test sink | Ethereal capture for kit-driven specs/staging — a real SMTP inbox with a preview URL, no live delivery |
| [06] | `streamTransport`/`jsonTransport` options | inspect sink | `{ streamTransport: true, buffer }` yields the raw MIME `Buffer`/`Readable`; `{ jsonTransport: true }` yields the message as JSON — the deterministic kit-driven spec and dry-run modalities |

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
