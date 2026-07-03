# [API_CATALOGUE] nodemailer

`nodemailer` supplies Node.js email sending: `createTransport` builds a `Transporter` backed by SMTP, SMTP pool, SES, sendmail, stream, or JSON transport, and `sendMail` dispatches messages with full RFC 2822 field coverage including HTML/text bodies, attachments, embedded images, and iCal events. `createTestAccount` and `getTestMessageUrl` support test account provisioning via Ethereal.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nodemailer`
- package: `nodemailer`
- module: `nodemailer`
- asset: `createTransport`, `createTestAccount`, `getTestMessageUrl`, transport classes, message option interfaces
- rail: messaging

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport and transporter family
- rail: messaging

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------ | :------------ | :---------------------------------------- |
|  [01]   | `Transporter<T, D>`             | class alias   | `Mail<T, D>` — the live transport wrapper |
|  [02]   | `Transport<T, D>`               | interface     | custom transport contract                 |
|  [03]   | `TransportOptions`              | interface     | base transport options `{ component? }`   |
|  [04]   | `SMTPTransport`                 | class         | single-connection SMTP transport          |
|  [05]   | `SMTPTransport.Options`         | interface     | SMTP connection + mail options            |
|  [06]   | `SMTPTransport.SentMessageInfo` | interface     | accepted/rejected/response info           |
|  [07]   | `SMTPPool`                      | class         | pooled SMTP transport                     |
|  [08]   | `SMTPPool.Options`              | interface     | pool size and connection options          |
|  [09]   | `SESTransport`                  | class         | AWS SES transport                         |
|  [10]   | `SendmailTransport`             | class         | sendmail process transport                |
|  [11]   | `StreamTransport`               | class         | writable stream transport                 |
|  [12]   | `JSONTransport`                 | class         | JSON-serialized message transport         |

[PUBLIC_TYPE_SCOPE]: message and address family
- rail: messaging

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :----------------------- | :------------ | :-------------------------------------------- |
|  [01]   | `Mail.Options`           | interface     | full message options (to/from/subject/body/…) |
|  [02]   | `SendMailOptions`        | type alias    | alias for `Mail.Options`                      |
|  [03]   | `Mail.Address`           | interface     | `{ name: string, address: string }`           |
|  [04]   | `Mail.Attachment`        | interface     | attachment with content/path/cid/encoding     |
|  [05]   | `Mail.AmpAttachment`     | interface     | AMP4EMAIL attachment                          |
|  [06]   | `Mail.IcalAttachment`    | interface     | iCalendar event attachment                    |
|  [07]   | `Mail.Envelope`          | interface     | SMTP envelope `{ from, to, cc, bcc }`         |
|  [08]   | `Mail.Headers`           | type          | header map or `{ key, value }[]`              |
|  [09]   | `TestAccount`            | interface     | Ethereal test account credentials             |
|  [10]   | `Mail.PluginFunction<T>` | function type | middleware plugin for the send pipeline       |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport construction
- rail: messaging

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------- |
|  [01]   | `createTransport(transport: SMTPPool \| SMTPPool.Options, defaults?)`                      | factory        | pooled SMTP transporter    |
|  [02]   | `createTransport(transport: SendmailTransport \| SendmailTransport.Options, defaults?)`    | factory        | sendmail transporter       |
|  [03]   | `createTransport(transport: StreamTransport \| StreamTransport.Options, defaults?)`        | factory        | stream transporter         |
|  [04]   | `createTransport(transport: JSONTransport \| JSONTransport.Options, defaults?)`            | factory        | JSON transporter           |
|  [05]   | `createTransport(transport: SESTransport \| SESTransport.Options, defaults?)`              | factory        | AWS SES transporter        |
|  [06]   | `createTransport(transport?: SMTPTransport \| SMTPTransport.Options \| string, defaults?)` | factory        | SMTP transporter (default) |
|  [07]   | `createTransport<T>(transport: Transport<T> \| TransportOptions, defaults?)`               | factory        | custom transport wrapper   |

[ENTRYPOINT_SCOPE]: send, verify, and lifecycle
- rail: messaging

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------- |
|  [01]   | `transporter.sendMail(mailOptions)`           | send           | `Promise<T>` — dispatches the message     |
|  [02]   | `transporter.sendMail(mailOptions, callback)` | send           | callback form                             |
|  [03]   | `transporter.verify()`                        | verify         | `Promise<true>` — tests SMTP connectivity |
|  [04]   | `transporter.verify(callback)`                | verify         | callback form                             |
|  [05]   | `transporter.close()`                         | lifecycle      | closes pooled connections                 |
|  [06]   | `transporter.isIdle()`                        | lifecycle      | `true` when queue has free slots          |
|  [07]   | `transporter.use(step, plugin)`               | plugin         | registers a middleware plugin for `step`  |

[ENTRYPOINT_SCOPE]: test account utilities
- rail: messaging

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `createTestAccount(apiUrl?, callback?)` | test           | provisions Ethereal SMTP test account        |
|  [02]   | `createTestAccount(callback?)`          | test           | callback form                                |
|  [03]   | `getTestMessageUrl(info)`               | test           | Ethereal preview URL for a sent test message |

## [04]-[IMPLEMENTATION_LAW]

[NODEMAILER_TOPOLOGY]:
- `createTransport` returns a `Mail` / `Transporter` instance; one transporter per deployment scope for SMTP; pool transports manage connection reuse internally.
- `SMTPTransport.Options` inherits `SMTPConnection.Options`; key fields: `host`, `port`, `secure` (TLS on port 465), `auth: { user, pass }`, `service` (well-known service shorthand).
- `Mail.Options.from` accepts plain address strings or `Mail.Address` objects; `to`, `cc`, `bcc`, `replyTo` accept arrays.
- Attachments use `content` (string/Buffer/Readable), `path` (file or URL), or `raw` (MIME node override); `cid` makes inline embedded images.
- `sendMail` resolves with the transport-specific `SentMessageInfo`; `SMTPTransport.SentMessageInfo` carries `envelope`, `messageId`, `accepted`, `rejected`, `pending`, `response`.
- `use(step, plugin)` where `step` is `"compile"` or `"stream"`; plugin signature is `(mail, callback) => void`.

[LOCAL_ADMISSION]:
- `SMTPConnection.Options.auth` accepts `{ type: "LOGIN" \| "OAUTH2" \| "CUSTOM", user, pass/credentials/oauth2 }`.
- For `SMTPPool`, `maxConnections` and `maxMessages` govern the pool; `close()` drains and shuts down.
- `@types/nodemailer` ships separately from the `nodemailer` runtime package; both are declared in the workspace catalog.

[RAIL_LAW]:
- Package: `nodemailer`
- Owns: email transport construction, message sending, SMTP verification, plugin middleware
- Accept: one `Transporter` per transport scope; `sendMail` for all dispatch
- Reject: hand-rolled SMTP socket code; direct use of internal `SMTPConnection` in domain code
