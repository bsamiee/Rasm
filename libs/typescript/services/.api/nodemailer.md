# [API_CATALOGUE] nodemailer

`nodemailer` supplies Node.js email sending: `createTransport` builds a `Transporter` backed by SMTP, SMTP pool, SES, sendmail, stream, or JSON transport, and `sendMail` dispatches messages with full RFC 2822 field coverage including HTML/text bodies, attachments, embedded images, and iCal events. `createTestAccount` and `getTestMessageUrl` support test account provisioning via Ethereal.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `nodemailer`
- package: `nodemailer`
- module: `nodemailer`
- asset: `createTransport`, `createTestAccount`, `getTestMessageUrl`, transport classes, message option interfaces
- rail: messaging

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: transport and transporter family
- rail: messaging

| [INDEX] | [SYMBOL]                        | [TYPE_FAMILY] | [RAIL]                                    |
| :-----: | :------------------------------ | :------------ | :---------------------------------------- |
|   [1]   | `Transporter<T, D>`             | class alias   | `Mail<T, D>` — the live transport wrapper |
|   [2]   | `Transport<T, D>`               | interface     | custom transport contract                 |
|   [3]   | `TransportOptions`              | interface     | base transport options `{ component? }`   |
|   [4]   | `SMTPTransport`                 | class         | single-connection SMTP transport          |
|   [5]   | `SMTPTransport.Options`         | interface     | SMTP connection + mail options            |
|   [6]   | `SMTPTransport.SentMessageInfo` | interface     | accepted/rejected/response info           |
|   [7]   | `SMTPPool`                      | class         | pooled SMTP transport                     |
|   [8]   | `SMTPPool.Options`              | interface     | pool size and connection options          |
|   [9]   | `SESTransport`                  | class         | AWS SES transport                         |
|  [10]   | `SendmailTransport`             | class         | sendmail process transport                |
|  [11]   | `StreamTransport`               | class         | writable stream transport                 |
|  [12]   | `JSONTransport`                 | class         | JSON-serialized message transport         |

[PUBLIC_TYPE_SCOPE]: message and address family
- rail: messaging

| [INDEX] | [SYMBOL]                 | [TYPE_FAMILY] | [RAIL]                                        |
| :-----: | :----------------------- | :------------ | :-------------------------------------------- |
|   [1]   | `Mail.Options`           | interface     | full message options (to/from/subject/body/…) |
|   [2]   | `SendMailOptions`        | type alias    | alias for `Mail.Options`                      |
|   [3]   | `Mail.Address`           | interface     | `{ name: string, address: string }`           |
|   [4]   | `Mail.Attachment`        | interface     | attachment with content/path/cid/encoding     |
|   [5]   | `Mail.AmpAttachment`     | interface     | AMP4EMAIL attachment                          |
|   [6]   | `Mail.IcalAttachment`    | interface     | iCalendar event attachment                    |
|   [7]   | `Mail.Envelope`          | interface     | SMTP envelope `{ from, to, cc, bcc }`         |
|   [8]   | `Mail.Headers`           | type          | header map or `{ key, value }[]`              |
|   [9]   | `TestAccount`            | interface     | Ethereal test account credentials             |
|  [10]   | `Mail.PluginFunction<T>` | function type | middleware plugin for the send pipeline       |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: transport construction
- rail: messaging

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                     |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------- |
|   [1]   | `createTransport(transport: SMTPPool \| SMTPPool.Options, defaults?)`                      | factory        | pooled SMTP transporter    |
|   [2]   | `createTransport(transport: SendmailTransport \| SendmailTransport.Options, defaults?)`    | factory        | sendmail transporter       |
|   [3]   | `createTransport(transport: StreamTransport \| StreamTransport.Options, defaults?)`        | factory        | stream transporter         |
|   [4]   | `createTransport(transport: JSONTransport \| JSONTransport.Options, defaults?)`            | factory        | JSON transporter           |
|   [5]   | `createTransport(transport: SESTransport \| SESTransport.Options, defaults?)`              | factory        | AWS SES transporter        |
|   [6]   | `createTransport(transport?: SMTPTransport \| SMTPTransport.Options \| string, defaults?)` | factory        | SMTP transporter (default) |
|   [7]   | `createTransport<T>(transport: Transport<T> \| TransportOptions, defaults?)`               | factory        | custom transport wrapper   |

[ENTRYPOINT_SCOPE]: send, verify, and lifecycle
- rail: messaging

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                                    |
| :-----: | :-------------------------------------------- | :------------- | :---------------------------------------- |
|   [1]   | `transporter.sendMail(mailOptions)`           | send           | `Promise<T>` — dispatches the message     |
|   [2]   | `transporter.sendMail(mailOptions, callback)` | send           | callback form                             |
|   [3]   | `transporter.verify()`                        | verify         | `Promise<true>` — tests SMTP connectivity |
|   [4]   | `transporter.verify(callback)`                | verify         | callback form                             |
|   [5]   | `transporter.close()`                         | lifecycle      | closes pooled connections                 |
|   [6]   | `transporter.isIdle()`                        | lifecycle      | `true` when queue has free slots          |
|   [7]   | `transporter.use(step, plugin)`               | plugin         | registers a middleware plugin for `step`  |

[ENTRYPOINT_SCOPE]: test account utilities
- rail: messaging

| [INDEX] | [SURFACE]                               | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :-------------------------------------- | :------------- | :------------------------------------------- |
|   [1]   | `createTestAccount(apiUrl?, callback?)` | test           | provisions Ethereal SMTP test account        |
|   [2]   | `createTestAccount(callback?)`          | test           | callback form                                |
|   [3]   | `getTestMessageUrl(info)`               | test           | Ethereal preview URL for a sent test message |

## [4]-[IMPLEMENTATION_LAW]

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
