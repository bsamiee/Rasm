# [API_CATALOGUE] nodemailer

`nodemailer` is the mail-dispatch codec the `persistence/work#WORK_AND_SIGNALS` notification owner rides: `createTransport(descriptor)` is one polymorphic entry discriminating on the transport shape (SMTP, SMTP-pool, SESv2, sendmail, stream, JSON, or a custom `Transport`) into a `Transporter` (a `Mail<T,D>`), and `transporter.sendMail(options)` dispatches one RFC-2822 message with full field coverage — HTML/text/AMP/watch bodies, attachments, inline `cid` images, iCal events, DKIM signing, list headers, and per-transport `SentMessageInfo`. `verify()` probes connectivity at the composition root; `createTestAccount`/`getTestMessageUrl` provision Ethereal previews. In `services` the live provider is `@aws-sdk/client-sesv2` behind the `SESTransport` — `createTransport({ SES: { sesClient, SendEmailCommand } })` — dispatched at an `Effect.tryPromise` boundary, with SMTP/OAuth2 credentials resolved through `security/secret#SECRET_STORE`, never a bare env read.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nodemailer`
- package: `nodemailer` (target 9.0.3, `MIT-0`, © Andris Reinman)
- types: `@types/nodemailer` 8.0.1 (MIT, DefinitelyTyped) — declared separately in the workspace catalog; the runtime package ships no bundled `.d.ts`
- module format: CJS (`lib/nodemailer.js`); node-only — SMTP sockets, sendmail child process, `node:stream`/`node:tls`/`node:net`; never a browser bundle; zero native ABI
- runtime target: Node `>=6` (modern node); the credentialed transport is node-only
- asset: `createTransport`/`createTestAccount`/`getTestMessageUrl`, the `SMTPTransport`/`SMTPPool`/`SESTransport`/`SendmailTransport`/`StreamTransport`/`JSONTransport` classes, the `Mail`/`Mail.Options`/`DKIM`/`XOAuth2`/`SMTPConnection` type families
- consumer: `persistence/work#WORK_AND_SIGNALS` — the notification/mail dispatch; backed by `@aws-sdk/client-sesv2` (`.api/aws-sdk-client-sesv2.md`) `SESTransport`
- rail: messaging / email

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport descriptor family — the one parameterized axis
`createTransport` discriminates on the transport descriptor; each transport class carries its own `Options` and `SentMessageInfo`. A `Transporter` is a `Mail<T,D>` whose `T` is the transport's `SentMessageInfo`. There is no `createSmtpTransport`/`createSesTransport` split — the descriptor is the discriminant.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `Transporter<T, D>`               | type alias    | `= Mail<T, D>` — the live transport wrapper (`sendMail`/`verify`/`close`) |
|  [02]   | `Transport<T, D>`                 | interface     | custom-transport contract — `{ name, version, send(mail, cb), verify?, close? }` |
|  [03]   | `TransportOptions`                | interface     | base transport options `{ component? }`                            |
|  [04]   | `SMTPTransport` / `SMTPTransport.Options` | class     | single-connection SMTP; `Options extends SMTPConnection.Options` (host/port/secure/auth/tls/timeouts) + `service`/`url` |
|  [05]   | `SMTPPool` / `SMTPPool.Options`   | class         | pooled SMTP; `Options` adds `pool: true`/`maxConnections`/`maxMessages`/`rateDelta`/`rateLimit` |
|  [06]   | `SESTransport` / `SESTransport.Options` | class    | AWS SESv2 — `Options.SES = { sesClient: SESv2ClientLike, SendEmailCommand }`; per-message `ses?` extra SESv2 fields |
|  [07]   | `SendmailTransport.Options`       | interface     | `{ sendmail: true, path?, newline?, args? }`                       |
|  [08]   | `StreamTransport.Options`         | interface     | `{ streamTransport: true, buffer?, newline? }` — build MIME to a stream/buffer |
|  [09]   | `JSONTransport.Options`           | interface     | `{ jsonTransport: true, skipEncoding? }` — serialize the message to JSON |

[PUBLIC_TYPE_SCOPE]: message and address family — `Mail.Options` is the full RFC-2822 surface
One `Mail.Options` carries every message facet; addresses accept a string, an `Address`, or an array; bodies and attachments accept `string`/`Buffer`/`Readable`/`AttachmentLike`.

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [CAPABILITY]                                                              |
| :-----: | :----------------------- | :------------ | :----------------------------------------------------------------------- |
|  [01]   | `Mail.Options`           | interface     | `from`/`sender`/`to`/`cc`/`bcc`/`replyTo`/`inReplyTo`/`references`/`subject`/`text`/`html`/`watchHtml`/`amp`/`icalEvent`/`headers`/`list`/`attachments`/`alternatives`/`envelope`/`messageId`/`date`/`dkim`/`priority`/`attachDataUrls`/`textEncoding`/`disableFileAccess`/`disableUrlAccess`/`raw` |
|  [02]   | `SendMailOptions`        | type alias    | `= Mail.Options`                                                          |
|  [03]   | `Mail.Address`           | interface     | `{ name?: string; address: string }`                                     |
|  [04]   | `Mail.Attachment`        | interface     | `content`/`path`/`href` + `filename`/`cid`/`contentType`/`contentDisposition`/`contentTransferEncoding`/`encoding`/`headers`/`raw` |
|  [05]   | `Mail.AmpAttachment` / `Mail.IcalAttachment` | interface | AMP4EMAIL body / iCalendar event (`method` defaults `PUBLISH`) |
|  [06]   | `Mail.Envelope`          | interface     | SMTP envelope `{ from, to, cc, bcc }` overriding the auto-derived one     |
|  [07]   | `Mail.Headers` / `Mail.ListHeaders` | type      | header map / `{ key, value }[]`; `list` → `List-*` headers               |
|  [08]   | `Mail.PluginFunction<T>` | function type | `(mail: MailMessage<T>, cb) => void` — a `use(step, plugin)` middleware   |
|  [09]   | `TestAccount`            | interface     | Ethereal creds `{ user, pass, smtp, imap, pop3, web }`                    |

[PUBLIC_TYPE_SCOPE]: auth, signing, and low-level family
`SMTPConnection.Options.auth` is the closed auth union; `XOAuth2` mints/refreshes OAuth2 tokens; `DKIM` signs the outgoing MIME. `SMTPConnection` is the raw socket surface transports own internally — not domain material.

| [INDEX] | [SYMBOL]                          | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :-------------------------------- | :------------ | :---------------------------------------------------------------- |
|  [01]   | `SMTPConnection.AuthenticationType` | union       | `Login { user, pass }` \| `OAuth2 (XOAuth2.Options)` \| `Custom { user, pass, method }` |
|  [02]   | `XOAuth2` / `XOAuth2.Options` / `XOAuth2.Token` | class    | OAuth2 token source — `clientId`/`clientSecret`/`refreshToken`/`accessToken`/`privateKey`/`provisionCallback`; `getToken`/`generateToken` |
|  [03]   | `DKIM.Options`                    | union         | `SingleKey { domainName, keySelector, privateKey }` \| `MultipleKeys { keys[] }` + `hashAlgo`/`headerFieldNames` |
|  [04]   | `SMTPConnection.DSNOptions`       | interface     | delivery-status-notification `{ ret?, envid?, notify?, orcpt? }`   |
|  [05]   | `SMTPConnection.Options`          | interface     | the socket/TLS/timeout knob set every SMTP transport inherits      |
|  [06]   | `SMTPConnection.SMTPError`        | interface     | `NodeJS.ErrnoException` + `code`/`response`/`responseCode`/`command` |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport construction — one entry, seven descriptors
`createTransport(descriptor, defaults?)` returns a `Transporter` typed to the descriptor's `SentMessageInfo`; a bare string (`"smtps://user:pass@host"`) or `{ service: "gmail", auth }` shorthand resolves the SMTP transport.

| [INDEX] | [SURFACE]                                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :---------------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `createTransport(transport?: SMTPTransport.Options \| string, defaults?)`     | factory        | SMTP transporter (default) — string URL or `service` shorthand accepted |
|  [02]   | `createTransport(transport: SMTPPool.Options, defaults?)`                     | factory        | pooled SMTP (`pool: true`, rate-limited)             |
|  [03]   | `createTransport(transport: SESTransport.Options, defaults?)`                 | factory        | SESv2 transporter — `{ SES: { sesClient, SendEmailCommand } }` |
|  [04]   | `createTransport(transport: SendmailTransport.Options \| StreamTransport.Options \| JSONTransport.Options, defaults?)` | factory | sendmail / stream / JSON transporter |
|  [05]   | `createTransport<T>(transport: Transport<T> \| TransportOptions, defaults?)`  | factory        | custom `Transport` wrapper                           |

[ENTRYPOINT_SCOPE]: send, verify, and lifecycle — promise mirror is canonical
`sendMail`/`verify` each expose a callback and a `Promise` arity; the `Promise` form is the Effect-bridged one. `use`/`set`/`get`/`setupProxy` tune the pipeline; `error`/`idle`/`token` are the transporter events.

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `transporter.sendMail(options): Promise<T>` / `sendMail(options, cb)` | send    | dispatch one message → transport `SentMessageInfo`   |
|  [02]   | `transporter.verify(): Promise<true>` / `verify(cb)`         | verify         | probe transport connectivity (composition-root health) |
|  [03]   | `transporter.close()` / `isIdle()`                           | lifecycle      | drain pooled connections / free-slot probe           |
|  [04]   | `transporter.use(step, plugin: Mail.PluginFunction)`         | plugin         | register `"compile"`/`"stream"` middleware           |
|  [05]   | `transporter.set(key, value)` / `get(key)` / `setupProxy(url)` | config       | `oauth2_provision_cb`, `proxy_handler_*`, proxy setup |
|  [06]   | `transporter.on("error"\|"idle"\|"token", listener)`         | event          | pool-error / idle-slot / refreshed `XOAuth2.Token`   |

[ENTRYPOINT_SCOPE]: test-account utilities

| [INDEX] | [SURFACE]                                                     | [ENTRY_FAMILY] | [CAPABILITY]                                          |
| :-----: | :------------------------------------------------------------ | :------------- | :--------------------------------------------------- |
|  [01]   | `createTestAccount(apiUrl?): Promise<TestAccount>` / `createTestAccount(cb)` | test | provision an Ethereal SMTP test account              |
|  [02]   | `getTestMessageUrl(info): string \| false`                   | test           | Ethereal preview URL for a sent test message         |

## [04]-[IMPLEMENTATION_LAW]

[NODEMAILER_TOPOLOGY]:
- `createTransport` returns a `Mail`/`Transporter`; one transporter per transport scope. Pool transports (`pool: true`) manage connection reuse and rate limits (`maxConnections`/`maxMessages`/`rateDelta`/`rateLimit`) internally — never a per-message reconnect.
- `Mail.Options.from`/`to`/`cc`/`bcc`/`replyTo` accept a string, `Mail.Address`, or an array; attachments use `content` (string/Buffer/Readable), `path` (file/URL/data-URI), or `raw` (full MIME override); `cid` embeds an inline image; `icalEvent` attaches a calendar event; `dkim` signs the outgoing MIME.
- `sendMail` resolves with the transport-specific `SentMessageInfo` — `envelope`/`messageId`/`accepted`/`rejected`/`pending`/`response` (SMTP/SES) or `message` (stream/JSON). Narrow delivery outcome on `accepted`/`rejected`, not on the response string.
- OAuth2: `auth: { type: "OAUTH2", user, ... }` drives an `XOAuth2` token source; the `token` event carries a refreshed `XOAuth2.Token`, and `set("oauth2_provision_cb", cb)` supplies renewal.

[LOCAL_ADMISSION]:
- The SESv2 transport is `createTransport({ SES: { sesClient: new SESv2Client(cfg), SendEmailCommand } })`; per-message `ses` fields (`ConfigurationSetName`, `EmailTags`, `ListManagementOptions`) pass through to the SESv2 `SendEmailCommand`. This is the only admitted provider path — no direct SMTP host in `services`.
- SMTP `auth` is `{ type: "LOGIN", user, pass }`, `{ type: "OAUTH2", user, ... }`, or a `custom` handler; `secure: true` selects implicit TLS (port 465), `requireTLS` forces STARTTLS on 587. `@types/nodemailer` is a separate catalog pin from the runtime.
- `SMTPConnection` is the transports' internal socket surface; domain code composes `Mail.Options` and never touches it.

[STACKING]:
- Effect boundary: `sendMail(options): Promise<SentMessageInfo>` folds at `Effect.tryPromise({ try, catch })` into the `persistence/work` typed mail-fault rail, mapping the SESv2 `SESv2ServiceException` subclasses (`MessageRejected`/`TooManyRequestsException`/`MailFromDomainNotVerifiedException`) the `SESTransport` surfaces (`.api/aws-sdk-client-sesv2.md`); `verify(): Promise<true>` runs once at the composition root as a startup health probe.
- SESv2 backing: the `SES.sesClient` is one `SESv2Client` reused per scope; the raw templated/bulk SESv2 operations `nodemailer` does not model (`SendBulkEmailCommand`, `Template`) are called on that same client directly — the mail owner shares one credentialed client between the `nodemailer` message path and the raw command path.
- Secret + tenancy: SMTP/SES credentials and the DKIM `privateKey` resolve through `security/secret#SECRET_STORE` (`SecretStore.resolve(SecretRef.Doppler(...))`), never `process.env`; the sending identity, `list` unsubscribe headers, and DKIM domain are scoped to `app.current_tenant` (`persistence/tenancy#TENANCY`) so a tenant never sends under another's identity.
- Asset attachments: `persistence/object#OBJECT_STORE` codec outputs ride `Mail.Options.attachments` — a `jspdf` `output("arraybuffer")` report (`.api/jspdf.md`), an `exceljs` workbook, or a `jszip` bundle (`.api/jszip.md`) as `{ filename, content: Buffer }`, and a stored-object `Readable` bridges in via `@effect/platform-node` `NodeStream.toReadable` (`.api/effect-platform-node.md`).
- Outbox receipt + telemetry: `sendMail`'s `SentMessageInfo.{ accepted, rejected, messageId }` becomes an `execution/outbox#TRANSACTIONAL_OUTBOX` `DeliverySink` receipt, and dispatch latency/failure rides an OTLP `Metric`/span on the `telemetry` spine (`.api/effect-opentelemetry.md`), never a parallel counter.

[RAIL_LAW]:
- package: `nodemailer`
- owns: transport construction, RFC-2822 message dispatch, connectivity verification, DKIM signing, OAuth2 token flow, and the plugin/middleware pipeline
- accept: one `Transporter` per transport scope; the SESv2 `SESTransport` as the admitted provider; `sendMail`/`verify` `Promise` arities bridged at `tryPromise`; credentials resolved through `SecretStore`
- reject: hand-rolled SMTP socket code; direct `SMTPConnection` use in domain code; a `createSmtpTransport`/`createSesTransport` method family where the descriptor discriminates; a bare `process.env` credential read; a parallel notification path beside the outbox `DeliverySink`
