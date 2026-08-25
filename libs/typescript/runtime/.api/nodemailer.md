# [TS_RUNTIME_API_NODEMAILER]

`nodemailer` is the SMTP/mail transport `runtime/src/work/deliver.ts` internalizes as the single mail-egress owner: one polymorphic `createTransport` discriminates on the transport-option shape to return a `Transporter` whose `sendMail` sends one RFC-5322 `Mail.Options` message, whose `verify` proves the connection, and whose every send yields a `SentMessageInfo` splitting accepted from rejected recipients, over a `LOGIN`/`OAUTH2`/`CUSTOM` auth union with native DKIM signing.

`Effect.tryPromise` lifts `sendMail`/`verify` around the callback/`Promise` library, `Redacted` carries every secret, the `Transporter` is a scoped `Layer`, and a durable job retries on `SMTPError.code` classification.

## [01]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: the transporter, the message, and the send result
- rail: boundaries

| [INDEX] | [SYMBOL]                             | [TYPE_FAMILY]   | [CONSUMER]                                                                   |
| :-----: | :----------------------------------- | :-------------- | :--------------------------------------------------------------------------- |
|  [01]   | `Transporter<T, D>` = `Mail<T, D>`   | egress service  | `EventEmitter` egress owner; methods in `[03]`                               |
|  [02]   | `Mail.Options` (= `SendMailOptions`) | message shape   | one message contract; parts in `[TOPOLOGY]`                                  |
|  [03]   | `Mail.Address`                       | address value   | `{ name?, address }` recipient; escapes `Name <email>`                       |
|  [04]   | `Mail.Attachment`                    | attachment      | a `report`/jszip byte artifact attaches here                                 |
|  [05]   | `Mail.ListHeaders` / `Mail.Headers`  | header shape    | `list` builds `List-Unsubscribe`; suppression seam                           |
|  [06]   | `SentMessageInfo`                    | send result     | top-level export is `any`; `SMTPTransport.SentMessageInfo` carries the shape |
|  [07]   | `Transport<T, D>`                    | plugin contract | `{ name, version, send, verify?, close? }` backend                           |

- `SentMessageInfo`: nodemailer erases this root export to `any`, so a consumer declares its own widened band over `SMTPTransport.SentMessageInfo` rather than importing the erased name.

[PUBLIC_TYPE_SCOPE]: connection, authentication, and signing policy
- rail: system-apis

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]     | [CONSUMER]                                         |
| :-----: | :----------------------------------------------- | :---------------- | :------------------------------------------------- |
|  [01]   | `SMTPConnection.Options`                         | connection policy | SMTP dial policy — host/port/TLS/timeouts/auth     |
|  [02]   | `SMTPPool.Options`                               | pool policy       | `pool: true`; bulk-send concurrency + rate limits  |
|  [03]   | `AuthenticationType` (`LOGIN`/`OAUTH2`/`CUSTOM`) | auth union        | discriminated credential; secrets `Redacted`       |
|  [04]   | `XOAuth2.Options` / `XOAuth2.Token`              | oauth flow        | refresh-token flow; `token` grant                  |
|  [05]   | `DKIM.Options`                                   | signing policy    | native RFC-6376 message signing                    |
|  [06]   | `SMTPConnection.DSNOptions`                      | delivery notify   | RFC-3461 delivery-status request                   |
|  [07]   | `SMTPConnection.SMTPError` / `shared.Logger`     | fault / log       | `code` (`EAUTH`) retry discriminant; `Logger` sink |

[ATTACHMENT]: `Attachment.content: string|Buffer|Readable` `Attachment.path: string` `Attachment.raw: string|Buffer|Readable` `Attachment.cid: string` `Attachment.contentType: string` `Attachment.contentTransferEncoding: "7bit"|"base64"|"quoted-printable"|false` `Attachment.encoding: string` `Attachment.contentDisposition: "attachment"|"inline"`
[SMTPCONNECTION_OPTIONS]: `host: string` `port: number` `secure: boolean` `requireTLS: boolean` `opportunisticTLS: boolean` `ignoreTLS: boolean` `tls: object` `auth: AuthenticationType` `authMethod: string` `lmtp: boolean` `connectionTimeout: number` `greetingTimeout: number` `socketTimeout: number` `dnsTimeout: number` — `AuthenticationType`'s `type` tag is OPTIONAL, so the union narrows on member presence, never a discriminant switch
[XOAUTH2_OPTIONS]: `XOAuth2.Options` (namespace member, every field optional) — `clientId` `clientSecret` `refreshToken` `accessToken` `accessUrl` `privateKey`; a bare `XOAuth2Options` symbol resolves nowhere
[DKIM_OPTIONS]: `DKIM.Options = SingleKeyOptions | MultipleKeysOptions` — the single arm carries `domainName` `keySelector` `privateKey` beside `hashAlgo` `headerFieldNames` `skipFields` `cacheDir` `cacheTreshold`, the multi arm a `keys` roster; a bare `DKIMKeyOptions` symbol resolves nowhere
[DSNOPTIONS]: `DSNOptions.notify: "SUCCESS"|"FAILURE"|"DELAY"|"NEVER"` `DSNOptions.ret: "Full"|"HDRS"` `DSNOptions.envid: string` `DSNOptions.orcpt: string`
[SENT_MESSAGE_INFO]: `SMTPTransport.SentMessageInfo` — `envelope` `messageId` `accepted` `rejected` `pending` `response`; `rejectedErrors` and the `envelopeTime`/`messageTime`/`messageSize` timing band ride the `SMTPConnection`/`SMTPPool` variants alone, which is why the consuming fence declares a widened optional band
[SMTPERROR]: `SMTPError.code: string` `SMTPError.response: string` `SMTPError.responseCode: number` `SMTPError.command: string`

## [02]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: build a transport, send, and verify
- rail: boundaries

| [INDEX] | [SURFACE]                                             | [ENTRY_FAMILY] | [CONSUMER]                                            |
| :-----: | :---------------------------------------------------- | :------------- | :---------------------------------------------------- |
|  [01]   | `createTransport(options, defaults?)` → `Transporter` | construct      | one polymorphic factory; option shape selects it      |
|  [02]   | `transporter.sendMail(message, cb?)` → `Promise<T>`   | send           | durable-job egress; `Effect.tryPromise`, `Schema`     |
|  [03]   | `transporter.verify(cb?)` → `Promise<true>`           | preflight      | connection/credential probe at `Layer` build          |
|  [04]   | `transporter.close()` / `transporter.isIdle()`        | lifetime       | `close` drains pool sockets; `isIdle` gates the relay |
|  [05]   | `transporter.use(step, plugin)` → `this`              | plugin         | `"compile"`/`"stream"` message-mutation step          |
|  [06]   | `transporter.setupProxy(url)`                         | proxy          | route SMTP egress through an HTTP/SOCKS proxy         |
|  [07]   | `transporter.set("proxy_handler_socks5"\|…, fn)`      | proxy          | register a `proxy_handler_*` socket handler           |
|  [08]   | `transporter.on("idle"\|"error"\|"token", cb)`        | events         | events → `Queue`/`SubscriptionRef`                    |

[ENTRYPOINT_SCOPE]: provider resolution, OAuth2, and test sinks
- rail: system-apis

| [INDEX] | [SURFACE]                                                    | [ENTRY_FAMILY]  | [CONSUMER]                                           |
| :-----: | :----------------------------------------------------------- | :-------------- | :--------------------------------------------------- |
|  [01]   | `wellKnown(key)` → `SMTPConnection.Options \| false`         | provider lookup | resolves `"Gmail"`/`"SendGrid"` to the dial options  |
|  [02]   | `shared.parseConnectionUrl(url)`                             | url decode      | decodes a `smtp://…` URL to `SMTPConnection.Options` |
|  [03]   | `XOAuth2#getToken` / `#generateToken` / `#buildXOAuth2Token` | oauth token     | refresh → access-token flow behind `OAUTH2`          |
|  [04]   | `DKIM#sign(input, extraOptions?)` → `PassThrough`            | dkim sign       | native signing; `Redacted` PEM key                   |
|  [05]   | `createTestAccount(apiUrl?)` → `Promise<TestAccount>`        | test sink       | Ethereal capture; a real inbox, no delivery          |
|  [06]   | `getTestMessageUrl(info)` → `string \| false`                | test sink       | preview URL for a captured message                   |
|  [07]   | `streamTransport`/`jsonTransport` options                    | inspect sink    | raw MIME or JSON — spec/dry-run sinks                |

- `wellKnown` and `shared.parseConnectionUrl` are DEEP imports (`nodemailer/lib/well-known`, `nodemailer/lib/shared`), never root entrypoints.

## [03]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- `createTransport` is one polymorphic factory: the option shape selects the transport — `pool: true` → pooled SMTP, `SES: {...}` → SESv2, `streamTransport`/`jsonTransport` → inspect sinks, `sendmail: true` → local binary, else plain SMTP from a URL string or `Options`. One transport policy row per environment replaces a factory per backend.
- `Mail.Options` is one shape decoded once, carrying every part — bodies, alternatives, attachments, headers, `list`, `dkim`, `envelope`. One outbound `Schema` decodes the caller payload at ingress and encodes to `Options` at the send seam, so `to`/`from`/`subject` never assemble untyped.
- Authentication is a discriminated union with `Redacted` secrets end to end: `LOGIN` (`user`/`pass`), `OAUTH2` (`XOAuth2` refresh flow), and `CUSTOM` are `_tag`-style arms. `pass`, `clientSecret`, `refreshToken`, and the DKIM `privateKey` flow from `Config.redacted`, unwrapped with `Redacted.value` only at the `createTransport`/`sign` call.
- `SentMessageInfo` is delivery evidence, splitting `accepted`/`rejected`/`rejectedErrors`/`pending` with `messageId` and timing. Partial rejection is a domain outcome the durable job records and reconciles, never a thrown failure.
- Retry classifies on `SMTPError.code`: `EAUTH` is terminal, a `4xx` `responseCode` retries under a `Schedule`, a `5xx` is a permanent bounce. `Data.taggedEnum` maps the code and drives `Effect.retry`/`catchTag` off the tag.
- `Transporter` is a scoped resource: pooled connections and OAuth2 timers live for the `Layer` `Scope`, `close()` is the finalizer, `isIdle()`/the `idle` event are the outbox-relay backpressure signal, and `verify()` at build proves the credential before the first send.

[STACKING]:
- `effect` (`../../.api/effect.md`): `Effect.tryPromise` lifts `sendMail`/`verify`; `Layer.scoped(Mailer, build)` owns the `Transporter` with `close` as release; `Config.redacted`/`Redacted` carry every secret; `Schema.decodeUnknown` decodes the outbound message; `SMTPError.code` becomes a `Data.taggedEnum` driving `Effect.retry(Schedule.exponential)`; the `idle`/`token` events bridge to a `SubscriptionRef`/`Queue`; `Effect.withSpan` tags the send with the `messageId`.
- `@effect/platform` (`../../.api/effect-platform.md`): a `report`/jszip artifact attaches as `{ content: Uint8Array }` or `{ path }`; `FileSystem.stream` feeds a large attachment as a `Readable`; `PlatformConfigProvider.layerDotEnv` behind `Config` supplies the SMTP DSN and DKIM key without a `process.env` read.
- `@effect/platform-node` (`../../.api/effect-platform-node.md`): the mail `Layer` composes under `NodeContext.layer`; `NodeStream.toReadable` converts an Effect `Stream<Uint8Array>` into the `Readable` an attachment or `raw` body expects.
- `jspdf` + `exceljs` + `papaparse` + `jszip` (`./jspdf.md`, `./exceljs.md`, `./papaparse.md`, `./jszip.md`): the `work/report` byte producers — a PDF/XLSX/CSV `Uint8Array` or a `jszip` bundle attaches as `Mail.Attachment` `{ content: bytes, contentType }`; nodemailer transports what the report format and archive container produce, never rendering a document itself.
- `security` (`../../security/.api/`): `crypt/sign` owns domain HMAC egress signing (webhook bodies); nodemailer owns RFC-6376 DKIM message signing natively, the two distinct and never merged. `jose` may mint the OAuth2 assertion, but the SMTP `XOAuth2` refresh flow stays in-transport.
- `@effect/workflow` + `@effect/cluster` (`./effect-workflow.md`, `./effect-cluster.md`): a send is a durable `Activity` with a compensation arm recording a suppression on hard bounce; the `work/deliver` entity fences per-tenant send quota, and the outbox relay drains queued messages under the `isIdle` pool signal.

[LOCAL_ADMISSION]:
- One `createTransport` policy row discriminated by option shape carries every backend; the message is one `Schema`-decoded `Mail.Options`.
- `Config.redacted`/`Redacted` hold `pass`/`clientSecret`/`refreshToken`/DKIM `privateKey`, unwrapped only at the transport/sign call.
- `Layer.scoped` owns the transporter with `close()` as finalizer and `verify()` at build.
- `SMTPError.code` classifies into a `Data.taggedEnum` for `Effect.retry`; `EAUTH` is terminal.
- `SentMessageInfo.rejected`/`rejectedErrors` is a domain outcome the durable job reconciles.
- `streamTransport`/`jsonTransport` + `createTestAccount` drive kit-driven specs and dry-runs.
