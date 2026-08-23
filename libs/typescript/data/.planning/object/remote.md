# [DATA_REMOTE]

`Remote` owns every non-local byte tree behind ONE origin-addressed surface — SFTP and SSH-exec over the in-process `ssh2` root, FTP/FTPS over `basic-ftp`, WebDAV over `webdav`, the object plane reached as the `s3:` row — carrying capability flags as data so every polymorphic verb degrades by row and never by fork.

`Origin` fuses scheme, coordinate, and path; its scheme selects the backend row, whose flags decide server-side against piped copy, push against poll watch, and rsync-delta against offset or parallel-chunk resume. Two transfer engines share one policy surface — the in-process ssh2 lane over `NodeStream`/`NodeSink` channel lifts, and external `rsync`/`scp`/`ssh` binaries as `@effect/platform` `Command` processes. Sessions lease from `lane/cache.md`'s bounded origin pool, and every remote read feeds the SAME content-addressed intake fold as local disk.

## [01]-[INDEX]

- [02]-[ORIGIN_ROWS]: `Origin` class, scheme capability-flag table, reason-discriminated fault.
- [03]-[SESSION_ROWS]: tagged session family carrying its proven flags, per-scheme scoped brackets, pooled reuse.
- [04]-[OP_SURFACE]: polymorphic verb set — stat/list/read/write/copy/move/remove/mkdir/lock, degrade.
- [05]-[TRANSFER_ENGINES]: resume policy rows — rsync delta, offset, chunked-parallel — and the intake fold.
- [06]-[SYNC_ENGINE]: persisted listings, comparator rows, diff-apply-recover fold.
- [07]-[WATCH_ROWS]: watch strategy rows — ssh exec-push, universal poll; local intake stays owned.
- [08]-[EXEC]: remote command execution — typed channels, exit disposition, local `Command` twin.
- [09]-[INSTRUMENT_ROWS]: Convention projections — one `_measured` fold at the entry record, bounded census taps.

## [02]-[ORIGIN_ROWS]

- Owner: `Origin` fuses URI decode with ONE scheme row table carrying the addressing group (`port`, `pooled`, `local`, `tls`) beside the `flags` capability group; `RemoteFault` closes its reasons through `Fault.Class.family`.
- Law: one scheme-keyed table owns every per-scheme fact — a second table beside it (a port map, a TLS map, a connectionless roster) is the shape that lets an arm spell `scheme === "<name>"` where a column belongs, so the dial, the pool road, the rsync argv rendering, and the TLS posture all read columns and the page holds no scheme-name test.
- Packages: `effect` (`Array`, `Data`, `Either`, `Option`, `ParseResult`, `Schema`); `@rasm/ts/core` (`Fault`).
- Entry: every consumer addresses the plane by `Origin` value — `Origin.parse("sftp://deploy@vps.example:22/srv/artifacts")` at a config seam, never scheme-forked code; the address seeds the acquire, and `session.flags` is the dispatch datum every operation reads.
- Growth: a new protocol is one scheme key, one flag row, one session arm, and the op arms it earns — every consumer inherits it through the flags; a flag a server refuses at runtime narrows by row override, never by fork.
- Law: capability flags are decision data — `serverCopy: false` makes `copy` degrade to read-then-write, `serverMove: false` makes `move` degrade to copy-then-remove, `changeNotify: false` routes `watch` to the poll row, `exec: false` refuses `exec` typed, `modTime: false` makes `stat` publish absence instead of sending a command the server never advertised; the degrade paths live in the op arms once, so no caller ever probes a protocol.
- Law: the `s3:` row is a bridge, not a re-implementation — its ops delegate to `object/store.md` and `object/stream.md` owners (`head`/`rekey`/`Rail.range`, the intake fold for ingress), so the object plane's conditional-put and grant law hold unchanged behind the origin address.
- Law: `RemoteFault` reasons route recovery as a fold — `connect` and `auth` invalidate the pooled session, `op` and `transfer` re-drive, `watch` re-arms the strategy, `exec` carries the exit disposition; a free-string-only fault is the named unroutable defect.
- Law: fault recovery derives from `Fault.Class`; unavailable operations re-drive, while denied authentication and execution fail fast.
- Law: each reason declares its own subject and renders its own sentence, and the raise carries ONE `case` payload — a free `detail` field beside a closed `reason` and a hand-written message template both delete at the class.
- Law: legs partition the census by the surface that DECIDES the reason — session, op, transfer, watch, exec — so a refusal names its seam without re-deriving it from the scheme.
- Law: the scheme row answers the descriptor a consumer selects on, and its two honest NON-answers carry as much weight as its columns — `flags` names what a row FITS and IS its degrade statement, since every false column is a capability given up that the op arms then degrade around, while `Remote.intake` is the one ADMISSION making remote bytes durable in this branch; TENANCY and LIFETIME no network row decides, because a foreign filesystem's isolation belongs to whoever operates it and its bytes outlive this branch's interest entirely, so `remove` is a caller verb rather than a retention policy and a row claiming either coordinate asserts authority over a host it merely dials; the `s3:` row alone answers both by delegating to the object plane's reference ledger, which is precisely why it is a bridge rather than a seventh protocol.

```typescript signature
import { Array, Data, Either, Option, ParseResult, Schema } from "effect"
import { Fault } from "@rasm/ts/core"

const _SCHEME_KEYS = ["file", "sftp", "ssh", "ftp", "ftps", "webdav", "s3"] as const

// ONE row per scheme carrying two column groups: the ADDRESSING columns a dial reads once (`port` the default the URI
// omits, `pooled` whether the scheme holds a control session a pool leases, `local` whether its paths address this
// host, `tls` the transport-security posture its own dialer negotiates) and the `flags` capability group every verb
// dispatches on. A second scheme-keyed table beside this one is the shape that lets a fork spell `scheme === "file"`
// where a column belongs, so the addressing facts live here and no arm re-derives them from a scheme name.
const _SCHEMES = {
  file: {
    port: 0, pooled: false, local: true, tls: "none",
    flags: { serverCopy: true, serverMove: true, putStream: true, changeNotify: true, exec: true, offsetResume: true, modTime: true, parallel: false, lock: false },
  },
  sftp: {
    port: 22, pooled: true, local: false, tls: "none",
    flags: { serverCopy: true, serverMove: true, putStream: true, changeNotify: false, exec: true, offsetResume: true, modTime: true, parallel: true, lock: false },
  },
  ssh: {
    port: 22, pooled: true, local: false, tls: "none",
    flags: { serverCopy: true, serverMove: true, putStream: true, changeNotify: false, exec: true, offsetResume: true, modTime: true, parallel: true, lock: false },
  },
  ftp: {
    port: 21, pooled: true, local: false, tls: "explicit",
    flags: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, modTime: true, parallel: false, lock: false },
  },
  ftps: {
    port: 990, pooled: true, local: false, tls: "implicit",
    flags: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, modTime: true, parallel: false, lock: false },
  },
  webdav: {
    port: 443, pooled: true, local: false, tls: "none",
    flags: { serverCopy: true, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, modTime: true, parallel: false, lock: true },
  },
  s3: {
    port: 443, pooled: false, local: false, tls: "none",
    flags: { serverCopy: true, serverMove: false, putStream: false, changeNotify: false, exec: false, offsetResume: true, modTime: true, parallel: true, lock: false },
  },
} as const

// Every reason on this plane refuses about ONE dialed origin and carries the evidence its own raise site held, so the
// subject is the record every row shares and each row renders the sentence its reason means — a free-string `detail`
// standing alone on the raise re-opens the axis `reason` already closes and leaves the message hand-templated at the
// class. Retryability, blame, and quarantine stay the core Fault.Class row table's, so no rank or retry column rides
// here to disagree with the lattice. Legs partition by the SURFACE that decides the reason — the dial and its
// credential are the session's, the verb set is the op surface's, and the transfer, watch, and exec planes each own
// theirs — so a census reads which seam refused without re-deriving it from the scheme.
const _Subject = Schema.Struct({ origin: Schema.String, detail: Schema.String })

const _family = Fault.Class.family(["connect", "auth", "op", "transfer", "watch", "exec"] as const, {
  connect: Fault.Class.row({
    class: "unavailable",
    leg: "session",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} refused the dial — ${detail}`,
  }),
  auth: Fault.Class.row({
    class: "denied",
    leg: "session",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} refused the credential — ${detail}`,
  }),
  op: Fault.Class.row({
    class: "unavailable",
    leg: "op",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} refused the operation — ${detail}`,
  }),
  transfer: Fault.Class.row({
    class: "unavailable",
    leg: "transfer",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} broke the transfer — ${detail}`,
  }),
  watch: Fault.Class.row({
    class: "unavailable",
    leg: "watch",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} dropped the watch — ${detail}`,
  }),
  exec: Fault.Class.row({
    class: "denied",
    leg: "exec",
    detail: _Subject,
    render: ({ origin, detail }) => `${origin} refused the command — ${detail}`,
  }),
})

declare namespace Remote {
  type Scheme = (typeof _SCHEME_KEYS)[number]
  type Flags = (typeof _SCHEMES)[Scheme]["flags"]
  type Tls = (typeof _SCHEMES)[Scheme]["tls"]
  type Reason = (typeof _family.kinds)[number]
  type _Rows<
    T extends {
      readonly [S in Scheme]: {
        readonly port: number
        readonly pooled: boolean
        readonly local: boolean
        readonly tls: "none" | "explicit" | "implicit"
        readonly flags: { readonly [F in keyof Flags]: boolean }
      }
    } = typeof _SCHEMES,
  > = T
}

class RemoteFault extends Schema.TaggedError<RemoteFault>()("RemoteFault", {
  case: _family.payload,
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

class Origin extends Schema.Class<Origin>("Origin")({
  scheme: Schema.Literal(..._SCHEME_KEYS),
  host: Schema.String,
  port: Schema.Number,
  username: Schema.String,
  path: Schema.String,
}) {
  static readonly parse = Schema.decodeUnknown(Schema.transformOrFail(Schema.String, Origin, {
    strict: true,
    decode: (uri, _options, ast) =>
      Either.flatMap(
        Either.try({ try: () => new URL(uri), catch: () => new ParseResult.Type(ast, uri) }),
        (parsed) => {
          const scheme = Array.findFirst(_SCHEME_KEYS, (key) => `${key}:` === parsed.protocol)
          return Option.match(scheme, {
            onNone: () => Either.left(new ParseResult.Type(ast, uri)),
            onSome: (admitted) => Either.right({
              scheme: admitted,
              host: parsed.hostname,
              port: parsed.port === "" ? _SCHEMES[admitted].port : Number.parseInt(parsed.port, 10),
              username: decodeURIComponent(parsed.username),
              path: decodeURIComponent(parsed.pathname),
            }),
          })
        },
      ),
    encode: (origin) =>
      ParseResult.succeed(`${origin.scheme}://${origin.username}@${origin.host}:${origin.port}${origin.path}`),
  }))

  // This getter answers the DECLARED floor alone — `_session` seeds a session with it and `_probe` narrows it against
  // server truth. Every verb dispatches on the SESSION's flags, so an arm reading here answers what the protocol
  // permits rather than what this server admits.
  get flags(): Remote.Flags {
    return _SCHEMES[this.scheme].flags
  }

  get row(): (typeof _SCHEMES)[Remote.Scheme] {
    return _SCHEMES[this.scheme]
  }

  get key(): OriginKey {
    return new OriginKey({ scheme: this.scheme, host: this.host, port: this.port, username: this.username })
  }

  at(path: string): Origin {
    return new Origin({ scheme: this.scheme, host: this.host, port: this.port, username: this.username, path })
  }
}
```

## [03]-[SESSION_ROWS]

- Owner: `Session` — one closed `Data.taggedEnum` family (`Ssh | Ftp | Dav | Bucket | Local`) carrying each arm's proven flag row and capacity fact, which every op narrows through `$match`, so a client cast is unspellable; the per-scheme session brackets — the ssh2 connect-on-`ready`/`end()`-on-release bracket with the SFTP subsystem lift, the `basic-ftp` `access` dial, the `webdav` client mint — `_opened` and `_session`, the one scheme-dispatched acquire and its in-bracket probe; and `Remote.sessions`, the one acquisition surface routing on the row's `pooled` column between `lane/cache.md`'s `CacheLane.origins` lease and a directly minted value.
- Packages: `ssh2` (`Client` — `connect`, `sftp`, `end`; events `ready`/`error`; config auth/trust/keepalive rows; `sock` jump-host injection); `basic-ftp` (`Client`, `access`, `close`); `webdav` (`createClient`, `AuthType`); `lane/cache.md` (`CacheLane.origins`, `CacheLane.lease`, `OriginKey`); `effect` (`Effect`, `Data`, `Redacted`, `Scope`).
- Entry: an operation calls `sessions.get(origin)` — the row's `pooled` column decides the road, so a scheme holding a control session leases through `CacheLane.lease` under the caller's `Scope` (the FTP one-transfer-per-control-connection law and SSH connection reuse are pool facts) while a row that holds none mints a free value, its capability arriving from the `FileSystem`/`ObjectStore` requirement channel with no inert pool key; naming the connectionless schemes at this branch instead of reading the column strands every future row on whichever side the list forgot.
- Growth: an auth posture (agent, keyboard-interactive, custom `authHandler`) is an `Auth` field flowing into the connect config; a bastion chain is the prior hop's `forwardOut` duplex entering the next `connect` through `sock` — config data, never topology code.
- Law: sessions are scoped brackets — ssh resolves on `ready`, fails typed on `error`, releases through `end()`; ftp dials through `access` alone (the split connect/login members are probes) and releases through `close()`; a bare client with ad-hoc listeners in domain code is the rejected spelling.
- Law: credentials are `Redacted` config rows — password, private key, passphrase never appear as literals; host trust rides `hostVerifier` where the deployment pins keys; TLS on the ftp row is the `Auth.secure` config value (`true` explicit upgrade, `"implicit"` wrapped, scheme-derived by default and overridable only as ruled config for a plaintext-only origin), never a scheme fork beyond the `ftps:` port default.
- Law: the probed flag row RIDES the session, so narrowing is load-bearing rather than reported — `_session` opens on the scheme's declared row and runs `_probe` inside the same acquire, `_probe` re-mints the arm (`getDAVCompliance` class `"2"` proving the lock row, `features()` `REST` proving offset resume and `MDTM` the modification-time read, `getQuota` carried as the `Option` capacity fact), and every verb dispatches on `session.flags`; a probe answering a report BESIDE the session leaves each op reading `origin.flags`, which states what the protocol permits rather than what this server admits, and the discovery then narrows nothing at all.
- Law: `Origin.flags` is the DECLARED floor and `Origin.row` the addressing group — the getter seeds `_session` and names the ceiling a probe narrows from, while `port`, `pooled`, `local`, and `tls` decide the dial; an op reading either past acquire re-derives a fact the session already proved.
- Law: every ssh2 kernel guards its SYNCHRONOUS arm — `connect` refuses an absent username, an unparseable or public-only private key, and an agent-forward request with no agent path; `sftp` and `exec` refuse a dropped session — each throwing before any callback runs, so the guard lands those refusals on `auth` and `connect` and blame stays declared rather than collapsing onto the `defect` row every budget refuses.
- Boundary: the ssh2 and basic-ftp surfaces are callback/Promise boundary kernels — every listener registration and promise lives inside these brackets, and above them only `Stream`/`Sink`/typed effects exist.

```typescript signature
import { Effect, Option, Redacted, type Scope } from "effect"
import { Client as SshClient, type SFTPWrapper } from "ssh2"
import { Client as FtpClient } from "basic-ftp"
import { AuthType, createClient, type WebDAVClient } from "webdav"
import { CacheLane, OriginKey } from "../lane/cache.ts"

declare namespace Remote {
  type Auth = {
    readonly password?: Redacted.Redacted
    readonly privateKey?: Redacted.Redacted
    readonly passphrase?: Redacted.Redacted
    readonly agent?: string
    readonly secure?: boolean | "implicit"
    readonly readyTimeout?: number
    readonly keepaliveInterval?: number
  }
  type Capacity = Option.Option<{ readonly used: number; readonly available: Option.Option<number> }>
  // Every arm carries the PROVEN capability row and the capacity fact the same acquire established, so a verb reads
  // what this server admits rather than what the scheme declares, and `Remote.probe` re-narrows by re-minting the arm.
  type Session = Data.TaggedEnum<{
    Ssh: { readonly client: SshClient; readonly flags: Flags; readonly quota: Capacity }
    Ftp: { readonly client: FtpClient; readonly flags: Flags; readonly quota: Capacity }
    Dav: { readonly client: WebDAVClient; readonly flags: Flags; readonly quota: Capacity }
    Bucket: { readonly flags: Flags; readonly quota: Capacity }
    Local: { readonly flags: Flags; readonly quota: Capacity }
  }>
  type End = { readonly origin: Origin; readonly session: Session }
  type Sessions = {
    readonly get: (origin: Origin) => Effect.Effect<Session, RemoteFault, Scope.Scope>
  }
}

const _Session = Data.taggedEnum<Remote.Session>()

const _ssh = (origin: Origin, auth: Remote.Auth): Effect.Effect<SshClient, RemoteFault, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.async<SshClient, RemoteFault>((resume) => {
      const client = new SshClient()
      try {
        client
          .once("ready", () => resume(Effect.succeed(client)))
          .once("error", (cause) =>
            resume(Effect.fail(new RemoteFault({ case: { reason: "connect", origin: origin.host, detail: String(cause) } }))))
          .connect({
            host: origin.host,
            port: origin.port,
            username: origin.username,
            ...(auth.password !== undefined && { password: Redacted.value(auth.password) }),
            ...(auth.privateKey !== undefined && { privateKey: Redacted.value(auth.privateKey) }),
            ...(auth.passphrase !== undefined && { passphrase: Redacted.value(auth.passphrase) }),
            ...(auth.agent !== undefined && { agent: auth.agent }),
            readyTimeout: auth.readyTimeout ?? 20_000,
            keepaliveInterval: auth.keepaliveInterval ?? 15_000,
          })
      } catch (cause) {
        // `connect` validates its config SYNCHRONOUSLY and throws before a socket exists, so the `error` listener
        // never fires and no handle leaks. Catching seats the refusal on `auth`, whose `denied` row states caller
        // blame and non-retryability; an escaping throw is a defect instead, which the class table grades
        // non-retryable under SYSTEM blame, so every budget refuses it for the wrong reason and misattributes it.
        resume(Effect.fail(new RemoteFault({ case: { reason: "auth", origin: origin.host, detail: String(cause) } })))
      }
      return Effect.sync(() => client.end())
    }),
    (client) => Effect.sync(() => client.end()),
  )

// `sftp` throws `Not connected` SYNCHRONOUSLY when the session dropped between lease and use rather than settling its
// callback, so the guard keeps a dropped connection on `connect` — `unavailable`, which every budget re-drives —
// where an escaping throw freezes it as a defect no budget in the branch retries.
const _sftp = (client: SshClient, origin: Origin): Effect.Effect<SFTPWrapper, RemoteFault> =>
  Effect.async<SFTPWrapper, RemoteFault>((resume) => {
    try {
      client.sftp((cause, wrapper) =>
        cause === undefined || cause === null
          ? resume(Effect.succeed(wrapper))
          : resume(Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: String(cause) } }))))
    } catch (cause) {
      resume(Effect.fail(new RemoteFault({ case: { reason: "connect", origin: origin.host, detail: String(cause) } })))
    }
  })

// Total over the row's posture vocabulary, so a new posture fails here rather than defaulting a dial to plaintext.
const _FTP_TLS: { readonly [P in Remote.Tls]: boolean | "implicit" } = {
  none: false,
  explicit: true,
  implicit: "implicit",
}

const _ftp = (origin: Origin, auth: Remote.Auth): Effect.Effect<FtpClient, RemoteFault, Scope.Scope> =>
  Effect.acquireRelease(
    Effect.tryPromise({
      try: async () => {
        const client = new FtpClient()
        await client.access({
          host: origin.host,
          port: origin.port,
          user: origin.username,
          password: auth.password === undefined ? undefined : Redacted.value(auth.password),
          // `_FTP_TLS` maps the row's own posture total over the vocabulary onto the dialer's value; `auth.secure`
          // overrides for the plaintext-only origin, a ruled config value.
          secure: auth.secure ?? _FTP_TLS[origin.row.tls],
        })
        return client
      },
      catch: (cause) => new RemoteFault({ case: { reason: "connect", origin: origin.host, detail: String(cause) } }),
    }),
    (client) => Effect.sync(() => client.close()),
  )

const _dav = (origin: Origin, auth: Remote.Auth): Effect.Effect<WebDAVClient, RemoteFault> =>
  Effect.try({
    try: () =>
      createClient(`https://${origin.host}:${origin.port}`, {
        authType: AuthType.Auto,
        username: origin.username,
        password: auth.password === undefined ? undefined : Redacted.value(auth.password),
      }),
    catch: (cause) => new RemoteFault({ case: { reason: "auth", origin: origin.host, detail: String(cause) } }),
  })

// Sessions open on the scheme's DECLARED row and `_probe` runs inside the same acquire, so every session a caller ever
// holds already carries server-proven flags and no verb has a static row left to read by accident.
const _opened = (origin: Origin, auth: Remote.Auth): Effect.Effect<Remote.Session, RemoteFault, Scope.Scope> => {
  const seed = { flags: origin.flags, quota: Option.none() } satisfies {
    readonly flags: Remote.Flags
    readonly quota: Remote.Capacity
  }
  return ({
    file: () => Effect.succeed(_Session.Local(seed)),
    sftp: () => Effect.map(_ssh(origin, auth), (client) => _Session.Ssh({ client, ...seed })),
    ssh: () => Effect.map(_ssh(origin, auth), (client) => _Session.Ssh({ client, ...seed })),
    ftp: () => Effect.map(_ftp(origin, auth), (client) => _Session.Ftp({ client, ...seed })),
    ftps: () => Effect.map(_ftp(origin, auth), (client) => _Session.Ftp({ client, ...seed })),
    webdav: () => Effect.map(_dav(origin, auth), (client) => _Session.Dav({ client, ...seed })),
    s3: () => Effect.succeed(_Session.Bucket(seed)),
  } satisfies { readonly [S in Remote.Scheme]: () => Effect.Effect<Remote.Session, RemoteFault, Scope.Scope> })[
    origin.scheme
  ]()
}

const _session = (origin: Origin, auth: Remote.Auth): Effect.Effect<Remote.Session, RemoteFault, Scope.Scope> =>
  Effect.flatMap(_opened(origin, auth), (session) => _probe(origin, session))

const _sessions = (auth: (key: OriginKey) => Remote.Auth): Effect.Effect<Remote.Sessions, never, Scope.Scope> =>
  Effect.map(
    CacheLane.origins((key: OriginKey) =>
      Effect.flatMap(
        Effect.mapError(
          Origin.parse(`${key.scheme}://${key.username}@${key.host}:${key.port}/`),
          (fault) => new RemoteFault({ case: { reason: "connect", origin: key.host, detail: String(fault) } }),
        ),
        (origin) => _session(origin, auth(key)),
      )),
    (pool) => ({
      // `CacheLane.lease` is the ONE road out of that pool and carries the held level with it, so this page takes the
      // lane's road rather than `KeyedPool.get` beneath a second bracket of its own. A connectionless origin mints a
      // free value and holds no session, so it takes no lease and raises no level the plane never took.
      get: (origin) =>
        origin.row.pooled
          ? CacheLane.lease(pool, origin.key)
          : _session(origin, {}),
    }),
  )

// `_probe` answers a SESSION, never a value beside one: server truth narrows the arm's own flag row, and every verb
// dispatches on that arm, so narrowing is load-bearing by construction. Returning a report the ops never consult is
// exactly the shape that lets a page claim capability discovery while every arm still reads the static row.
const _probe = (origin: Origin, session: Remote.Session): Effect.Effect<Remote.Session, RemoteFault> =>
  _Session.$match(session, {
    Dav: ({ client, flags }) =>
      Effect.zipWith(
        Effect.tryPromise({ try: () => client.getDAVCompliance(origin.path), catch: _fault(origin, "op") }),
        Effect.tryPromise({ try: () => client.getQuota(), catch: _fault(origin, "op") }),
        (compliance, quota) =>
          _Session.Dav({
            client,
            flags: { ...flags, lock: compliance.compliance.includes("2") },
            quota: Option.map(Option.fromNullable(quota), (held) => ({
              used: typeof held.used === "number" ? held.used : 0,
              available: typeof held.available === "number" ? Option.some(held.available) : Option.none(),
            })),
          }),
      ),
    // `FEAT` answers both columns this arm narrows: `REST` proves restart-at-offset, and `MDTM` proves the one command
    // `lastMod` sends verbatim. `MLST` supersedes nothing here — it upgrades the LISTING to MLSD, whose per-entry
    // parsed date the census arm already carries as its own `Option`, so the column stays the single-path probe's.
    Ftp: ({ client, flags }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.features(), catch: _fault(origin, "op") }),
        (features) =>
          _Session.Ftp({
            client,
            flags: { ...flags, offsetResume: features.has("REST"), modTime: features.has("MDTM") },
            quota: Option.none(),
          }),
      ),
    Ssh: () => Effect.succeed(session),
    Bucket: () => Effect.succeed(session),
    Local: () => Effect.succeed(session),
  })
```

## [04]-[OP_SURFACE]

- Owner: the polymorphic verb set — `stat`, `list`, `read` (→ backpressured `Stream`), `write` (← `Sink`, offset-positioned when resuming), `copy`, `move`, `remove`, `mkdir` — each ONE entry dispatching through `Session.$match` with flag-driven degrade arms; and `Remote.intake`, the content-addressed landing that runs any remote read through the SAME identity fold as local disk.
- Packages: `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable` — the only stream seams); `ssh2` (SFTP `stat`, `readdir`, `createReadStream`, `createWriteStream`, `rename`, `unlink`, `mkdir`, `rmdir`, `open`, `close`, `ext_copy_data`); `webdav` (`stat`, `getDirectoryContents`, `createReadStream`, `createWriteStream`, `copyFile`, `moveFile`, `deleteFile`, `createDirectory`); `basic-ftp` (`list`, `size`, `lastMod`, `downloadTo`, `uploadFrom`, `appendFrom`, `rename`, `remove`, `removeDir`, `ensureDir`); `@aws-sdk/client-s3` (`paginateListObjectsV2` — the bucket census walk); `object/stream.md` (`Rail.bytes`, `Rail.chunked`, `Rail.identity`, `Rail.range`), `object/store.md` (`ObjectStore`).
- Entry: `Remote.read(origin, session)` yields `Stream<Uint8Array, RemoteFault>` on every scheme; `Remote.intake(origin, session, retention)` is the one cloud-ingestion entry — read, cut, digest, conditional put, reference row, retention tag — identical receipts to `Disk.intake`.
- Growth: a new verb is one dispatch surface with per-row arms; per-server capability discovery is `[3]`'s `Remote.probe`, narrowing the flag row before the arms dispatch, never a caller branch.
- Law: `lock`/`unlock` realize the flag row's `lock` column — RFC 4918 tokens on the DAV arm coordinating against concurrent DAV writers, a typed refusal everywhere else (the bucket arm names why: the conditional put already owns write races) — and the DAV arm reads the SESSION's probed column, so a server whose compliance answer proved no class 2 refuses at the verb rather than sending a LOCK the acquire disproved; `unlock` stays unguarded there because a token in hand IS the capability proof.
- Law: reads and writes are backpressured lifts — SFTP and DAV node streams cross through `NodeStream.fromReadable`/`NodeSink.fromWritable`, the FTP arm bridges its `Writable`-consuming transfer through one relay duplex inside the boundary; no raw `.on("data")` consumption exists past the adapter.
- Law: the FTP relay is a BRACKETED resource on both arms, never a bare mint — the relay's life IS the transfer's, so a consumer interrupted mid-stream releases it and the pooled control connection returns usable; an unbracketed relay leaves `downloadTo`/`uploadFrom` writing into a duplex nobody drains, which strands that connection for the pool's whole lease with no fault raised anywhere, and `_piped` scopes the pair so the sink's bracket closes with the run it fed.
- Law: the FTP transfer bound counts only time spent waiting for the SERVER, so a consumer draining the relay slowly — a download folded into a decompressor, an upload fed by a computing source — holds the data connection idle without refusal, and the dial therefore carries a bound tight enough to convict a stalled origin rather than one widened to survive this branch's own backpressure.
- Law: degrade is structural — `copy` on a row without `serverCopy` (or across hosts) composes `read` into `write`; `move` without `serverMove` composes `copy` then `remove`; `remove` discriminates file-versus-directory on the `stat` verdict, never a caller flag; a caller cannot observe which arm ran except through the receipt.
- Law: the SSH rows carry `serverCopy` because the protocol ships it — SFTP's `copy-data` extension moves bytes server-side and `ext_copy_data` is its member, so a `false` cell there understated the protocol and sent every same-host SFTP copy through this process for no reason; the extension is advertised per server and reaches no typed probe surface, so the arm attempts it and degrades to the piped copy on refusal, which keeps the fast path a capability the row claims rather than a claim the row shrank to avoid.
- Law: `putStream` decides which rows a piped road may WRITE into, and the bucket row answers `false` — its write arm refuses every byte because a content object is addressed by what it holds, so no engine row lists `s3` among the target schemes it serves and a bucket destination refuses at SELECTION, naming the row, rather than mid-transfer inside a sink the caller already opened.
- Law: the `s3:` arms honor content addressing — reads ride `Rail.range`, the server-side copy rides `rekey` against the probed ETag, byte ingress rides `Remote.intake`, and deletion rides the object plane's reference release; a raw bucket sink, a unilateral bucket delete, or a bucket-source `move` refuses typed BEFORE any byte moves — re-parenting a content object is a ledger verb, and a copy-then-refuse partial mutation is unspellable because the refusal guards the whole verb.
- Law: every remote byte that becomes durable rides `Remote.intake` — the origin row grows no second addressing vocabulary, dedup and 412-idempotency arrive from the object plane for free, and a remote origin is therefore a first-class artifact source; its custody coordinate is the store's `remote` owner row spent as scheme, host, and path SEGMENTS, so the origin the ledger records is the one a scan parses back rather than an authority string a separator re-splits.
- Law: `Remote.Stat` IS the listing model's projection — `[6]`'s `Model.Class` over `sync_listing` states path, span, kind, and the two optional halves once, so the census an arm publishes, the value the comparator reads, and the row the sync fold persists cannot drift a field apart, and a new stat column is one model row every arm inherits.
- Law: `Remote.Stat.modified` carries ONE spelling from every arm — `_stamped` normalizes to ISO-8601 text at the boundary and answers absence on an unparseable reply, because the sync comparator equates two arms' values directly and persists them, so a WebDAV RFC-1123 string beside an SFTP epoch second reports every shared path changed on every run; the FTP census publishes the MLSD-parsed `modifiedAt` alone, never the `rawModifiedAt` a LIST reply prints for a human and no parser reads back.
- Boundary: the SFTP callback verbs (`stat`, `readdir`, `mkdir`, `rmdir`, `unlink`, `rename`) are the page's callback kernels — each wraps one `Effect.async` settle and nothing else.

```typescript signature
import { Chunk, DateTime, Number, Ref, Sink, Stream } from "effect"
import { FileSystem } from "@effect/platform"
import { NodeSink, NodeStream } from "@effect/platform-node"
import { PassThrough } from "node:stream"
import { paginateListObjectsV2 } from "@aws-sdk/client-s3"
import { Digest } from "@rasm/ts/core"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Remote {
  // The census answer IS `[6]`'s listing model minus the coordinates the RELATION adds, so every arm here publishes the
  // shape the sync engine persists and re-admits, and a widened census lands at that one declaration.
  type Stat = typeof _Stat.Type
}

const _fault = (origin: Origin, reason: Remote.Reason) => (cause: unknown): RemoteFault =>
  new RemoteFault({ case: { reason, origin: origin.host, detail: String(cause) } })

// ONE stamp spelling crosses this page, because the sync comparator equates two arms' values directly and persists
// them: WebDAV answers an RFC-1123 string, MLSD a parsed date, SFTP an epoch second, so arms publishing each provider's
// own spelling report every shared path changed on every run forever. Unparseable input answers ABSENCE, never a
// spelling no other arm can compare against.
const _stamped = (raw: string | Date | undefined | null): Option.Option<string> =>
  Option.flatMap(Option.fromNullable(raw), (held) => {
    const at = held instanceof Date ? held : new Date(held)
    return Number.isNaN(at.getTime()) ? Option.none() : Option.some(at.toISOString())
  })

const _keyed = (origin: Origin): Effect.Effect<Digest.Key<"content">, RemoteFault> =>
  Effect.mapError(
    Schema.decodeUnknown(Digest.Key.content)(origin.path.slice(1)),
    (fault) => new RemoteFault({ case: { reason: "op", origin: origin.host, detail: String(fault) } }),
  )

const _read = (origin: Origin, session: Remote.Session, offset?: number): Stream.Stream<Uint8Array, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Stream.unwrap(
        Effect.map(_sftp(client, origin), (sftp) =>
          NodeStream.fromReadable(
            () => sftp.createReadStream(origin.path, offset === undefined ? {} : { start: offset }),
            _fault(origin, "op"),
          ))),
    Ftp: ({ client }) =>
      // `downloadTo` writes into this relay until it ends or dies, so the relay IS the transfer's lifetime: an
      // interrupted consumer leaving it alive strands the pooled control connection mid-transfer with no fault
      // anywhere. Release runs on interruption exactly as it runs on completion, so one bracket owns both edges.
      Stream.unwrapScoped(
        Effect.map(
          Effect.acquireRelease(
            Effect.sync(() => {
              const relay = new PassThrough()
              void client.downloadTo(relay, origin.path, offset ?? 0)
                .catch((cause: unknown) => relay.destroy(new Error(String(cause))))
              return relay
            }),
            (relay) => Effect.sync(() => relay.destroy()),
          ),
          (relay) => NodeStream.fromReadable(() => relay, _fault(origin, "transfer")),
        )),
    Dav: ({ client }) =>
      NodeStream.fromReadable(
        () => client.createReadStream(origin.path, offset === undefined ? {} : { range: { start: offset } }),
        _fault(origin, "op"),
      ),
    Bucket: () =>
      Stream.unwrap(
        Effect.map(_keyed(origin), (key) =>
          Rail.range(key, offset === undefined ? undefined : { from: offset }).pipe(
            Stream.mapError((fault) => new RemoteFault({ case: { reason: "op", origin: origin.host, detail: fault.case.detail } })),
          ))),
    Local: () =>
      Stream.unwrap(
        Effect.map(FileSystem.FileSystem, (fs) =>
          fs.stream(origin.path).pipe(Stream.mapError(_fault(origin, "op"))))),
  })

const _write = (origin: Origin, session: Remote.Session, at?: number) =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Effect.map(_sftp(client, origin), (sftp) =>
        NodeSink.fromWritable(
          () => sftp.createWriteStream(origin.path, at === undefined ? {} : { flags: "r+", start: at }),
          _fault(origin, "op"),
        )),
    Ftp: ({ client }) =>
      // Same bracket as the read arm, same reason: an abandoned upload relay holds the control connection open
      // mid-write, and release is the only edge reaching it under interruption.
      Effect.map(
        Effect.acquireRelease(
          Effect.sync(() => {
            const relay = new PassThrough()
            void (at === undefined ? client.uploadFrom(relay, origin.path) : client.appendFrom(relay, origin.path))
              .catch((cause: unknown) => relay.destroy(new Error(String(cause))))
            return relay
          }),
          (relay) => Effect.sync(() => relay.destroy()),
        ),
        (relay) => NodeSink.fromWritable(() => relay, _fault(origin, "transfer")),
      ),
    Dav: ({ client }) =>
      at === undefined
        ? Effect.succeed(NodeSink.fromWritable(() => client.createWriteStream(origin.path), _fault(origin, "op")))
        : // the DAV resume arm: the tail lands as RANGED PATCHES at a running offset — one `partialUpdateFileContents`
          // per chunk window, so memory holds one window whatever span the resume covers; a collected whole-tail
          // buffer reads as one PATCH and buys that economy with unbounded memory, which is the rejected trade
          Effect.map(Ref.make(at), (cursor) =>
            Sink.forEachChunk((parts: Chunk.Chunk<Uint8Array>) => {
              const bytes = Buffer.concat(Chunk.toReadonlyArray(parts))
              return bytes.byteLength === 0
                ? Effect.void
                : Effect.flatMap(Ref.getAndUpdate(cursor, (from) => from + bytes.byteLength), (from) =>
                    Effect.asVoid(Effect.tryPromise({
                      try: () => client.partialUpdateFileContents(origin.path, from, from + bytes.byteLength - 1, bytes),
                      catch: _fault(origin, "transfer"),
                    })))
            })),
    Bucket: () =>
      Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<bucket:write-rides-intake>" } })),
    Local: () =>
      Effect.map(FileSystem.FileSystem, (fs) => Sink.mapError(fs.sink(origin.path), _fault(origin, "op"))),
  })

const _stat = (origin: Origin, session: Remote.Session): Effect.Effect<Remote.Stat, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Effect.flatMap(_sftp(client, origin), (sftp) =>
        Effect.async<Remote.Stat, RemoteFault>((resume) => {
          sftp.stat(origin.path, (cause, held) =>
            cause === undefined || cause === null
              ? resume(Effect.succeed({
                  path: origin.path,
                  bytes: held.size,
                  modified: _stamped(new Date(held.mtime * 1000)),
                  kind: held.isDirectory() ? "directory" as const : "file" as const,
                  etag: Option.none(),
                }))
              : resume(Effect.fail(_fault(origin, "op")(cause))))
        })),
    // `lastMod` sends `MDTM` with no guard of its own, so an unadvertising server faults an ordinary stat. The probed
    // column degrades the axis instead: size still answers, and the comparator abstains on the absent half.
    Ftp: ({ client, flags }) =>
      Effect.tryPromise({
        try: async () => {
          const bytes = await client.size(origin.path)
          const modified = flags.modTime ? await client.lastMod(origin.path) : undefined
          return { path: origin.path, bytes, modified: _stamped(modified), kind: "file" as const, etag: Option.none() }
        },
        catch: _fault(origin, "op"),
      }),
    Dav: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.stat(origin.path), catch: _fault(origin, "op") }),
        (held) => {
          const row = "data" in held ? held.data : held
          return {
            path: origin.path,
            bytes: row.size,
            modified: _stamped(row.lastmod),
            kind: row.type === "directory" ? "directory" as const : "file" as const,
            etag: Option.fromNullable(row.etag),
          }
        }),
    Bucket: () =>
      Effect.flatMap(_keyed(origin), (key) =>
        Effect.flatMap(ObjectStore, (store) =>
          Effect.map(
            Effect.mapError(store.head(key), (fault) => new RemoteFault({ case: { reason: "op", origin: origin.host, detail: fault.case.detail } })),
            (head) => ({
              path: origin.path,
              bytes: head.bytes,
              modified: Option.map(head.modified, DateTime.formatIso),
              kind: "file" as const,
              etag: head.etag,
            }),
          ))),
    Local: () =>
      Effect.flatMap(FileSystem.FileSystem, (fs) =>
        Effect.map(
          Effect.mapError(fs.stat(origin.path), _fault(origin, "op")),
          (info) => ({
            path: origin.path,
            bytes: Number(info.size),
            modified: Option.map(info.mtime, (time) => time.toISOString()),
            kind: info.type === "Directory" ? "directory" as const : "file" as const,
            etag: Option.none(),
          }))),
  })

const _list = (origin: Origin, session: Remote.Session): Effect.Effect<ReadonlyArray<Remote.Stat>, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Effect.flatMap(_sftp(client, origin), (sftp) =>
        Effect.async<ReadonlyArray<Remote.Stat>, RemoteFault>((resume) => {
          sftp.readdir(origin.path, (cause, entries) =>
            cause === undefined || cause === null
              ? resume(Effect.succeed(entries.map((entry) => ({
                  path: `${origin.path}/${entry.filename}`,
                  bytes: entry.attrs.size,
                  modified: _stamped(new Date(entry.attrs.mtime * 1000)),
                  kind: entry.attrs.isDirectory() ? "directory" as const : "file" as const,
                  etag: Option.none(),
                }))))
              : resume(Effect.fail(_fault(origin, "op")(cause))))
        })),
    Ftp: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.list(origin.path), catch: _fault(origin, "op") }),
        // `modifiedAt` is the MLSD-parsed date and exists only there; `rawModifiedAt` always exists and is whatever the
        // LIST reply printed for a human, which parses reliably nowhere — publishing it hands the comparator a value
        // no other arm's spelling can equal.
        (entries) => entries.map((entry) => ({
          path: `${origin.path}/${entry.name}`,
          bytes: entry.size,
          modified: _stamped(entry.modifiedAt),
          kind: entry.isDirectory ? "directory" as const : "file" as const,
          etag: Option.none(),
        }))),
    Dav: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.getDirectoryContents(origin.path), catch: _fault(origin, "op") }),
        (held) => ("data" in held ? held.data : held).map((row) => ({
          path: row.filename,
          bytes: row.size,
          modified: _stamped(row.lastmod),
          kind: row.type === "directory" ? "directory" as const : "file" as const,
          etag: Option.fromNullable(row.etag),
        }))),
    Bucket: () =>
      Effect.flatMap(ObjectStore, (store) =>
        Effect.map(
          Stream.runCollect(
            Stream.fromAsyncIterable(
              paginateListObjectsV2({ client: store.client }, { Bucket: store.bucket, Prefix: origin.path.slice(1) }),
              (cause) => new RemoteFault({ case: { reason: "op", origin: origin.host, detail: String(cause) } }),
            ).pipe(
              Stream.mapConcatEffect((page) =>
                Effect.forEach(page.Contents ?? [], (entry) =>
                  entry.Key === undefined || entry.Size === undefined
                    ? Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<incomplete-list-entry>" } }))
                    : Effect.succeed<Remote.Stat>({
                        path: `/${entry.Key}`,
                        bytes: entry.Size,
                        modified: _stamped(entry.LastModified),
                        kind: "file",
                        etag: Option.fromNullable(entry.ETag),
                      }))),
            ),
          ),
          Chunk.toReadonlyArray,
        )),
    Local: () =>
      Effect.flatMap(FileSystem.FileSystem, (fs) =>
        Effect.mapError(
          Effect.flatMap(fs.readDirectory(origin.path), (names) =>
            Effect.forEach(names, (name) =>
              Effect.map(fs.stat(`${origin.path}/${name}`), (info) => ({
                path: `${origin.path}/${name}`,
                bytes: Number(info.size),
                modified: Option.map(info.mtime, (time) => time.toISOString()),
                kind: info.type === "Directory" ? "directory" as const : "file" as const,
                etag: Option.none(),
              })))),
          _fault(origin, "op"),
        )),
  })

const _mkdir = (origin: Origin, session: Remote.Session): Effect.Effect<void, RemoteFault, FileSystem.FileSystem> =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Effect.flatMap(_sftp(client, origin), (sftp) =>
        Effect.async<void, RemoteFault>((resume) => {
          sftp.mkdir(origin.path, (cause) =>
            cause === undefined || cause === null ? resume(Effect.void) : resume(Effect.fail(_fault(origin, "op")(cause))))
        })),
    Ftp: ({ client }) =>
      Effect.asVoid(Effect.tryPromise({ try: () => client.ensureDir(origin.path), catch: _fault(origin, "op") })),
    Dav: ({ client }) =>
      Effect.tryPromise({ try: () => client.createDirectory(origin.path, { recursive: true }), catch: _fault(origin, "op") }),
    Bucket: () => Effect.void,
    Local: () =>
      Effect.flatMap(FileSystem.FileSystem, (fs) =>
        Effect.mapError(fs.makeDirectory(origin.path, { recursive: true }), _fault(origin, "op"))),
  })

const _remove = (origin: Origin, session: Remote.Session): Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  Effect.flatMap(_stat(origin, session), (held) =>
    _Session.$match(session, {
      Ssh: ({ client }) =>
        Effect.flatMap(_sftp(client, origin), (sftp) =>
          Effect.async<void, RemoteFault>((resume) => {
            const settle = (cause: unknown) =>
              cause === undefined || cause === null ? resume(Effect.void) : resume(Effect.fail(_fault(origin, "op")(cause)))
            if (held.kind === "directory") {
              sftp.rmdir(origin.path, settle)
            } else {
              sftp.unlink(origin.path, settle)
            }
          })),
      Ftp: ({ client }) =>
        Effect.asVoid(Effect.tryPromise({
          try: () => held.kind === "directory" ? client.removeDir(origin.path) : client.remove(origin.path),
          catch: _fault(origin, "op"),
        })),
      Dav: ({ client }) =>
        Effect.tryPromise({ try: () => client.deleteFile(origin.path), catch: _fault(origin, "op") }),
      Bucket: () =>
        Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<bucket:remove-is-release>" } })),
      Local: () =>
        Effect.flatMap(FileSystem.FileSystem, (fs) =>
          Effect.mapError(fs.remove(origin.path, { recursive: held.kind === "directory" }), _fault(origin, "op"))),
    }))

// Scoping here closes the sink's transfer bracket with the pipe, so a failed or interrupted run tears its relay down
// rather than leaving the receiving end holding a half-written transfer.
// SFTP handles are scoped resources: the copy needs one on each path, and release closes them whichever way the
// extension settles. Close faults drop, because a leaked handle on a pooled session outlives every caller and the
// copy's own verdict already landed.
const _handle = (sftp: SFTPWrapper, origin: Origin, path: string, mode: "r" | "w") =>
  Effect.acquireRelease(
    Effect.async<Buffer, RemoteFault>((resume) => {
      sftp.open(path, mode, (cause, handle) =>
        cause === undefined || cause === null
          ? resume(Effect.succeed(handle))
          : resume(Effect.fail(_fault(origin, "op")(cause))))
    }),
    (handle) => Effect.async<void>((resume) => { sftp.close(handle, () => resume(Effect.void)) }),
  )

// The SFTP `copy-data` extension moves bytes server-side with no hop through this process, `len: 0` reading the source
// to EOF. `ext_copy_data` THROWS SYNCHRONOUSLY when the server never advertised the extension rather than settling its
// callback, so an `Effect.async` handling only the callback path lets that throw escape as a defect past every typed
// handler; catching it answers `false` and the caller degrades. Its other sync throws — server-mode misuse, a
// non-Buffer handle — are unreachable from a client session whose handles both come from `_handle`.
const _serverCopied = (sftp: SFTPWrapper, from: Origin, to: Origin): Effect.Effect<boolean, RemoteFault, Scope.Scope> =>
  Effect.flatMap(
    Effect.all([_handle(sftp, from, from.path, "r"), _handle(sftp, to, to.path, "w")]),
    ([source, target]) =>
      Effect.async<boolean, RemoteFault>((resume) => {
        try {
          sftp.ext_copy_data(source, 0, 0, target, 0, (cause) =>
            cause === undefined || cause === null
              ? resume(Effect.succeed(true))
              : resume(Effect.fail(_fault(to, "transfer")(cause))))
        } catch {
          resume(Effect.succeed(false))
        }
      }),
  )

const _piped = (from: Remote.End, to: Remote.End, at?: number) =>
  Effect.scoped(
    Effect.asVoid(
      Effect.flatMap(_write(to.origin, to.session, at), (sink) =>
        Stream.run(_read(from.origin, from.session, at), sink))))

const _copy = (from: Remote.End, to: Remote.End): Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  from.origin.scheme !== to.origin.scheme || from.origin.host !== to.origin.host || !from.session.flags.serverCopy
    ? _piped(from, to)
    : _Session.$match(to.session, {
        Ssh: ({ client }) =>
          Effect.scoped(
            Effect.flatMap(_sftp(client, from.origin), (sftp) =>
              Effect.flatMap(_serverCopied(sftp, from.origin, to.origin), (copied) =>
                copied ? Effect.void : _piped(from, to)))),
        Ftp: () => _piped(from, to),
        Dav: ({ client }) =>
          Effect.tryPromise({ try: () => client.copyFile(from.origin.path, to.origin.path), catch: _fault(to.origin, "op") }),
        Bucket: () =>
          Effect.flatMap(Effect.all([_keyed(from.origin), _keyed(to.origin)]), ([source, target]) =>
            Effect.flatMap(ObjectStore, (store) =>
              Effect.asVoid(
                Effect.mapError(
                  store.rekey(source, target),
                  (fault) => new RemoteFault({ case: { reason: "op", origin: to.origin.host, detail: fault.case.detail } }),
                )))),
        Local: () =>
          Effect.flatMap(FileSystem.FileSystem, (fs) =>
            Effect.mapError(fs.copy(from.origin.path, to.origin.path), _fault(to.origin, "op"))),
      })

const _move = (from: Remote.End, to: Remote.End): Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  _Session.$is("Bucket")(from.session)
    ? Effect.fail(new RemoteFault({ case: { reason: "op", origin: from.origin.host, detail: "<bucket:move-rides-ledger>" } }))
    : from.origin.scheme === to.origin.scheme && from.origin.host === to.origin.host && from.session.flags.serverMove
    ? _Session.$match(to.session, {
        Ssh: ({ client }) =>
          Effect.flatMap(_sftp(client, from.origin), (sftp) =>
            Effect.async<void, RemoteFault>((resume) => {
              sftp.rename(from.origin.path, to.origin.path, (cause) =>
                cause === undefined || cause === null ? resume(Effect.void) : resume(Effect.fail(_fault(to.origin, "op")(cause))))
            })),
        Ftp: ({ client }) =>
          Effect.asVoid(Effect.tryPromise({ try: () => client.rename(from.origin.path, to.origin.path), catch: _fault(to.origin, "op") })),
        Dav: ({ client }) =>
          Effect.tryPromise({ try: () => client.moveFile(from.origin.path, to.origin.path), catch: _fault(to.origin, "op") }),
        Bucket: () =>
          Effect.fail(new RemoteFault({ case: { reason: "op", origin: to.origin.host, detail: "<bucket:move-rides-ledger>" } })),
        Local: () =>
          Effect.flatMap(FileSystem.FileSystem, (fs) =>
            Effect.mapError(fs.rename(from.origin.path, to.origin.path), _fault(to.origin, "op"))),
      })
    : Effect.zipRight(_copy(from, to), _remove(from.origin, from.session))

const _intake = (origin: Origin, session: Remote.Session, retention: Retain.Class) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const flow = _read(origin, session).pipe(
      Stream.mapError((fault) => new ObjectFault({ case: { reason: "io", key: origin.path, detail: fault.case.detail } })),
    )
    const identity = yield* Rail.identity(Rail.chunked(flow, Rail.cut))
    const landed = yield* store.putKeyed(
      identity.key,
      yield* Stream.toReadableStreamEffect(
        _read(origin, session).pipe(
          Stream.mapError((fault) => new ObjectFault({ case: { reason: "io", key: origin.path, detail: fault.case.detail } })),
        )),
      identity.bytes,
    )
    // the origin's three coordinates are three encoded SEGMENTS, never one interpolated authority: a raw `://` and a
    // path bearing `:` both re-split the owner a custody scan parses back, so the mint percent-encodes each
    yield* store.refer(identity.key, ObjectStore.owner("remote", origin.scheme, origin.host, origin.path), retention) // the derived retention tag lands with the reference row
    return { key: identity.key, bytes: identity.bytes, written: landed.written, origin }
  })

const _lock = (origin: Origin, session: Remote.Session): Effect.Effect<{ readonly token: string }, RemoteFault> =>
  _Session.$match(session, {
    // `flags.lock` gates the arm off the PROBED column: a server whose compliance answer carried no class 2 refuses
    // here rather than receiving a LOCK the acquire already disproved — narrowing is load-bearing on the verb.
    Dav: ({ client, flags }) =>
      flags.lock
        ? Effect.map(
            Effect.tryPromise({ try: () => client.lock(origin.path), catch: _fault(origin, "op") }),
            (held) => ({ token: held.token }),
          )
        : Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<lock:class-2-unproven>" } })),
    Ssh: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<lock:unsupported>" } })),
    Ftp: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<lock:unsupported>" } })),
    Bucket: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<lock:conditional-put-owns-races>" } })),
    Local: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<lock:unsupported>" } })),
  })

const _unlock = (origin: Origin, session: Remote.Session, token: string): Effect.Effect<void, RemoteFault> =>
  _Session.$match(session, {
    Dav: ({ client }) => Effect.tryPromise({ try: () => client.unlock(origin.path, token), catch: _fault(origin, "op") }),
    Ssh: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<unlock:unsupported>" } })),
    Ftp: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<unlock:unsupported>" } })),
    Bucket: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<unlock:unsupported>" } })),
    Local: () => Effect.fail(new RemoteFault({ case: { reason: "op", origin: origin.host, detail: "<unlock:unsupported>" } })),
  })
```

## [05]-[TRANSFER_ENGINES]

- Owner: the `_ENGINES` policy rows — `rsyncDelta` (the primary resumable/delta lane over the external binary), `chunkedParallel` (`fastGet`/`fastPut` with the mined tuning defaults), `ftpOffset` (`startAt`/`appendFrom` arithmetic), `davRange` (the ranged-PATCH resume), `sftpOffset` (byte-offset resume from the target's `stat` size into a positioned write) — the `_ENDS` admission arms and the `_RESUMES` execution record over them, and `Remote.transfer(from, to, policy?)`, the one end-to-end move whose engine selection is row-derived data and whose `step` hook feeds the fact stream's meter row.
- Packages: `@effect/platform` (`Command.make`, `Command.exitCode` — the external `rsync`/`scp`/`ssh` engine; `stdin: Sink`/`stdout: Stream` process shape); `ssh2` (SFTP `fastGet`/`fastPut` — `concurrency`/`chunkSize`/`step`; `stat`, `open`, `read`, `write`, `close`); `basic-ftp` (`downloadTo(destination, path, startAt)`, `appendFrom`, `uploadFrom` slice options).
- Entry: `Remote.transfer(from, to)` walks `_ENGINES` in DECLARATION ORDER and takes the first row both origins admit; `policy.engine` pins a row that still answers the same admission, and `policy.step(progress)` observes transferred bytes per chunk (the ftp arm bridges it through a bracketed `trackProgress`).
- Growth: an engine tuning posture is a `_TUNE` override on the call; a new engine (a provider's accelerated transfer) is one row carrying its capability column, its end arm, its served target schemes, and its `resumes` value — selection, admission, and execution all inherit it.
- Law: every engine column is DECISION DATA the dispatch reads — `needs` names the capability flag, `ends` names which origin must carry it, `schemes` names the target schemes the row serves, and `resumes` selects the execution arm; a pinned engine answers the same three admission columns as a derived one, so `policy.engine` narrows the choice and can never spawn a binary against an address its row does not serve. Columns no arm reads are the flags that let a pin reach a lane the origins cannot run.
- Law: the `resumes` dispatch is TOTAL over its vocabulary — `_RESUMES` is a record keyed by the resume kind, so a declared engine is reachable by construction rather than by a predicate ladder whose tail silently absorbed every unmatched row; `range` and `offset` share the same probe-then-position arithmetic and differ at `_write`'s own DAV arm, which lands the tail as ranged PATCHes at a running offset.
- Law: no origin pair admitting no engine transfers — the selection answers `Option` and its absence refuses with the `transfer` reason naming the pinned or derived row, never a silent fall-through to a fallback engine the ends cannot run.
- Law: rsync flags are the sealed resume contract — `--partial --append-verify --inplace --checksum` gives delta transfer, interrupt resume, and integrity in one engine; the command is a `Command` value whose `exitCode` folds into the typed rail and whose cancellation rides the `Scope`.
- Law: resume is arithmetic where rsync is absent — `resume: true` probes the target, propagates a missing or unreadable-target fault unchanged, opens the positioned write (`flags: "r+"`, `appendFrom` on ftp), and streams the source from the verified byte; the default restart writes from byte zero without manufacturing absence through `Effect.option`.
- Law: the resume tap fires where the engine is IN HAND — `remoteResumed` is the only evidence the resume flags ever resumed, and the row that ran is known at the selection, so the counter tags the settled engine rather than a re-derivation at the entry record answering a second, unrelated walk.
- Law: the mined tuning defaults are policy values — `concurrency: 64`, `chunkSize: 32768` arrived from the wrapper ecosystem's measured defaults and live in `_TUNE`, never inline literals.

```typescript signature
import { Array, Struct } from "effect"
import { Command } from "@effect/platform"

const _TUNE = { concurrency: 64, chunkSize: 32_768 } as const

const _RSYNC = ["--partial", "--append-verify", "--inplace", "--checksum"] as const

const _RESUME_KEYS = ["delta", "offset", "chunk", "range"] as const

declare namespace Remote {
  type Engine = keyof typeof _ENGINES
  type Resume = (typeof _RESUME_KEYS)[number]
  type Ends = keyof typeof _ENDS
  type Progress = { readonly total: number; readonly transferred: number }
  type Policy = {
    readonly engine?: Engine
    readonly resume?: boolean
    readonly step?: (progress: Progress) => void
  }
}

// Which END must carry the row's capability flag: the delta lane runs its binary on both sides, the parallel lane
// hands a LOCAL path to the target's own uploader, and the offset family resumes into the receiving row alone.
const _ENDS = {
  both: (flag: keyof Remote.Flags, from: Remote.End, to: Remote.End) => from.session.flags[flag] && to.session.flags[flag],
  target: (flag: keyof Remote.Flags, _from: Remote.End, to: Remote.End) => to.session.flags[flag],
  // `local` is the row column, never a scheme name: the parallel lane hands a filesystem PATH to the target's own
  // uploader, so what it needs from the source end is that its paths address this host.
  localSource: (flag: keyof Remote.Flags, from: Remote.End, to: Remote.End) =>
    from.origin.row.local && to.session.flags[flag],
} as const

// DECLARATION ORDER is the preference and every column is read: `needs` names the capability flag, `ends` the origin
// that must carry it, `schemes` the target schemes the row serves, `resumes` the execution arm. A pinned engine
// answers the same three admission columns, so `policy.engine` narrows the walk and never bypasses capability.
const _ENGINES = {
  rsyncDelta: { needs: "exec", ends: "both", schemes: _SCHEME_KEYS, resumes: "delta" },
  chunkedParallel: { needs: "parallel", ends: "localSource", schemes: ["sftp", "ssh"], resumes: "chunk" },
  ftpOffset: { needs: "offsetResume", ends: "target", schemes: ["ftp", "ftps"], resumes: "offset" },
  davRange: { needs: "offsetResume", ends: "target", schemes: ["webdav"], resumes: "range" },
  sftpOffset: { needs: "offsetResume", ends: "target", schemes: ["file", "sftp", "ssh"], resumes: "offset" },
} as const satisfies {
  readonly [key: string]: {
    readonly needs: keyof Remote.Flags
    readonly ends: Remote.Ends
    readonly schemes: ReadonlyArray<Remote.Scheme>
    readonly resumes: Remote.Resume
  }
}

// A row admits when the target scheme is one it serves AND the end its `ends` column names carries its flag; the
// same predicate serves the ordered derivation and the pin, so neither road can reach a lane the origins cannot run.
const _admits = (engine: Remote.Engine, from: Remote.End, to: Remote.End): boolean => {
  const row = _ENGINES[engine]
  return Array.contains(row.schemes, to.origin.scheme) && _ENDS[row.ends](row.needs, from, to)
}

const _selected = (from: Remote.End, to: Remote.End, pinned: Remote.Engine | undefined): Option.Option<Remote.Engine> =>
  pinned === undefined
    ? Array.findFirst(Struct.keys(_ENGINES), (engine) => _admits(engine, from, to))
    : Option.filter(Option.some(pinned), (engine) => _admits(engine, from, to))

const _rsync = (from: Origin, to: Origin) =>
  Command.make(
    "rsync",
    "-e", "ssh",
    ..._RSYNC,
    // `local` decides which end renders as a bare path and which as an ssh address, and a scheme-name test here
    // forks every future local-addressing row onto the remote spelling.
    from.row.local ? from.path : `${from.username}@${from.host}:${from.path}`,
    to.row.local ? to.path : `${to.username}@${to.host}:${to.path}`,
  ).pipe(
    Command.exitCode,
    Effect.filterOrFail(
      (code) => code === 0,
      (code) => new RemoteFault({ case: { reason: "transfer", origin: to.host, detail: `rsync:${code}` } }),
    ),
  )

// `_localEnd` mints the synthetic source the chunked lane pipes from: the `file` row's declared flags ARE its proven
// flags, because a filesystem admits no server able to refuse them, so this seed needs no probe.
const _localEnd = (path: string): Remote.End => {
  const origin = Origin.make({ scheme: "file", host: "", port: _SCHEMES.file.port, username: "", path })
  return { origin, session: _Session.Local({ flags: origin.flags, quota: Option.none() }) }
}

const _fastPut = (to: Remote.End, local: string, step?: (progress: Remote.Progress) => void) =>
  _Session.$match(to.session, {
    Ssh: ({ client }) =>
      Effect.flatMap(_sftp(client, to.origin), (sftp) =>
        Effect.async<void, RemoteFault>((resume) => {
          sftp.fastPut(local, to.origin.path, {
            concurrency: _TUNE.concurrency,
            chunkSize: _TUNE.chunkSize,
            step: (total, _chunk, size) => step?.({ total: size, transferred: total }),
          }, (cause) =>
            cause === undefined || cause === null
              ? resume(Effect.void)
              : resume(Effect.fail(new RemoteFault({ case: { reason: "transfer", origin: to.origin.host, detail: String(cause) } }))))
        })),
    Ftp: () => _piped(_localEnd(local), to),
    Dav: () => _piped(_localEnd(local), to),
    Bucket: () => Effect.fail(new RemoteFault({ case: { reason: "transfer", origin: to.origin.host, detail: "<bucket:transfer-rides-intake>" } })),
    Local: () => Effect.fail(new RemoteFault({ case: { reason: "transfer", origin: to.origin.host, detail: "<local:transfer-is-copy>" } })),
  })

const _metered = (session: Remote.Session, step: ((progress: Remote.Progress) => void) | undefined) =>
  <A, E, R>(work: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
    step === undefined
      ? work
      : _Session.$match(session, {
          // trackProgress is client-global on the ftp control connection: bracketed on, handler detached on release
          Ftp: ({ client }) =>
            Effect.acquireUseRelease(
              Effect.sync(() => client.trackProgress((info) => step({ total: info.bytesOverall, transferred: info.bytes }))),
              () => work,
              () => Effect.sync(() => client.trackProgress()),
            ),
          Ssh: () => work,
          Dav: () => work,
          Bucket: () => work,
          Local: () => work,
        })

// The offset family's whole arithmetic: probe ONLY under the resume policy so a missing or unreadable target stays
// typed instead of being rewritten to byte zero, then position the write and stream the source from the verified byte.
const _offset = (from: Remote.End, to: Remote.End, policy: Remote.Policy | undefined) =>
  Effect.flatMap(
    policy?.resume === true ? Effect.map(_stat(to.origin, to.session), (held) => held.bytes) : Effect.succeed(0),
    (at) => _metered(to.session, policy?.step)(_piped(from, to, at > 0 ? at : undefined)),
  )

// Total over the resume vocabulary, so every declared engine is reachable by construction: `range` shares the offset
// arithmetic and diverges at `_write`'s own DAV arm, which lands the tail as ranged PATCHes at a running offset.
const _RESUMES: {
  readonly [R in Remote.Resume]: (
    from: Remote.End,
    to: Remote.End,
    policy: Remote.Policy | undefined,
  ) => Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem | CommandExecutor.CommandExecutor>
} = {
  delta: (from, to) => Effect.asVoid(_rsync(from.origin, to.origin)),
  chunk: (from, to, policy) => _fastPut(to, from.origin.path, policy?.step),
  offset: _offset,
  range: _offset,
}

const _transfer = (from: Remote.End, to: Remote.End, policy?: Remote.Policy) =>
  Option.match(_selected(from, to, policy?.engine), {
    // no admitting row is a typed refusal naming the pinned or derived choice: a fall-through to a fallback engine
    // would spawn against an address the row does not serve and report the failure as the binary's own
    onNone: () =>
      Effect.fail(new RemoteFault({
        case: {
          reason: "transfer",
          origin: to.origin.host,
          detail: `<engine:${policy?.engine ?? "derived"}:${to.origin.scheme}>`,
        },
      })),
    onSome: (engine) =>
      // the resume tap fires where the engine is IN HAND: the rsync contract and the offset arms both claim
      // resumability and neither leaves a trace, so the counter tags the row that actually ran
      policy?.resume === true
        ? Effect.tap(
            _RESUMES[_ENGINES[engine].resumes](from, to, policy),
            () => Metric.increment(Metric.tagged(_resumed, Convention.rasm.remoteEngine, engine)),
          )
        : _RESUMES[_ENGINES[engine].resumes](from, to, policy),
  })
```

## [06]-[SYNC_ENGINE]

- Owner: the bisync fold — the `sync_listing` model owning both the persisted per-side listing and the census projection `[4]` publishes, the `_COMPARE` comparator rows (`sizeModtime` default, `checksum`, `sizeOnly`), the per-side delta census, the reconcile fold producing typed `SyncAction` rows, apply-through-`transfer`, and resync recovery after interrupt.
- Packages: `@effect/sql` (`Model.Class`, `Model.GeneratedByApp`, `Model.FieldOption`, `Model.DateTimeInsert`, `SqlSchema`, `sql.insert`); `lane/capability.md` (`Capability.Ensure` — the listing relation rides the same DDL split); `journal/append.md` (`Journal.Version` — the number-or-string codec the BIGINT `bytes` column decodes through on the spine wire); composition over `[4]`/`[5]` values.
- Entry: `Remote.sync(pair, left, right, comparator?)` — census both ends, delta each side against its persisted listing, reconcile (a change on one side transfers to the other, a removal propagates, ANY concurrent change on both sides — modify against modify, and a removal racing a modification alike — surfaces as a `Conflict` row the caller routes), apply, then persist the settled listings in one transaction.
- Growth: a listing column is one model field row both SQL edges and every census arm inherit; a comparator is one row; a conflict policy (`leftWins`, `newerWins`, `surface`) is a caller fold over the returned `Conflict` rows; a third replica is pairwise composition, never a widened engine.
- Law: the listing relation is ONE `Model.Class` — `Model.FieldOption` owns the nullable-column-versus-`Option` crossing on `modified` and `etag` in BOTH directions, `Model.GeneratedByApp` seats the `pair`/`side` key columns the relation adds beside the census, and `Model.DateTimeInsert` mints `listed_at` on the rail; a row struct hand-spelled beside the census type is the twin that lets the read edge's null fold drift from the write edge's, and the drift reports every shared path changed with nothing to blame.
- Law: the relation mutates by whole-side replacement, so it takes no repository and no `Query.table` binding — the fold settles a side's entire listing inside one transaction, where per-row CRUD over a set replaced wholesale lands partial state no comparator can read.
- Law: a `Conflict` performs no transfer and no removal, and its path persists with its PRIOR listing row on both sides — the unresolved delta re-surfaces on every subsequent run until the caller rules it, so an unrouted conflict can never silently become a propagated winner.
- Law: listings are the resume substrate — an interrupted sync re-runs against the persisted rows and the already-applied transfers land as no-ops; absence crosses provider read, SQL landing, and SQL re-admission unchanged, so checksum comparison falls back to size-plus-modified only when either side lacks an ETag, never to null, empty-string, or zero sentinels.
- Law: the comparator is a policy row, never a fork — `sizeModtime` reads the census, `checksum` compares content evidence (`etag` where the backend mints one, the content-addressed intake fold's key where it does not), `sizeOnly` serves append-only trees; the row travels on the pair.
- Law: `sizeModtime` degrades BY ROW rather than by fault — a timestamp votes only where both sides carry one, so a server whose `modTime` column came back false compares on span alone and an origin pair mixing a timestamped side with a bare one abstains on that axis instead of declaring every shared path changed forever; `checksum` falls to the same named row, so one degrade is spelled once and neither comparator invents a sentinel for absence.

```typescript signature
import { HashSet } from "effect"
import { Model, SqlClient, SqlSchema } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal } from "../journal/append.ts"

const _listingDdl: Capability.Ensure = {
  relation: "sync_listing",
  pg: `CREATE TABLE IF NOT EXISTS sync_listing (
    pair TEXT NOT NULL, side TEXT NOT NULL, path TEXT NOT NULL,
    bytes BIGINT NOT NULL, kind TEXT NOT NULL, modified TEXT, etag TEXT,
    listed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (pair, side, path));`,
  sqlite: `CREATE TABLE IF NOT EXISTS sync_listing (
    pair TEXT NOT NULL, side TEXT NOT NULL, path TEXT NOT NULL,
    bytes INTEGER NOT NULL, kind TEXT NOT NULL, modified TEXT, etag TEXT,
    listed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (pair, side, path));`,
}

// ONE declaration over `sync_listing`: the census an op arm answers and the row this engine persists are the same
// truth, so `Model.FieldOption` carries the nullable column against the decoded `Option` in both directions and no edge
// hand-folds absence into a spelling the other edge misreads. Field names ARE column names, so the model's own order is
// the relation's.
class _Listing extends Model.Class<_Listing>("Remote.Listing")({
  pair: Model.GeneratedByApp(Schema.String),
  side: Model.GeneratedByApp(Schema.Literal("left", "right")),
  path: Schema.String,
  bytes: Journal.Version,
  kind: Schema.Literal("file", "directory"),
  modified: Model.FieldOption(Schema.String),
  etag: Model.FieldOption(Schema.String),
  listed_at: Model.DateTimeInsert,
}) {}

// `Remote.Stat` IS this projection: a provider knows no pair, no side, and no landing stamp, so the census reads the
// listing's own fields without the coordinates the RELATION adds and the insert variant puts them back.
const _Stat = Schema.Struct(_Listing.fields).pipe(Schema.omit("pair", "side", "listed_at"))

// One key value addresses both edges, so the side a read holds and the side a write replaces are the same datum.
const _Side = Schema.Struct({ pair: _Listing.fields.pair, side: _Listing.fields.side })

// Timestamps VOTE only where both sides carry one: a row whose server proved no modification-time read publishes
// absence, and an absent half asserting inequality reports every shared path changed on every run forever. One named
// row also gives the checksum comparator its fallback, so the two spell the degrade once.
const _sizeModtime = (left: Remote.Stat, right: Remote.Stat) =>
  left.bytes !== right.bytes ||
  Option.getOrElse(Option.zipWith(left.modified, right.modified, (leftAt, rightAt) => leftAt !== rightAt), () => false)

const _COMPARE = {
  sizeModtime: _sizeModtime,
  sizeOnly: (left: Remote.Stat, right: Remote.Stat) => left.bytes !== right.bytes,
  checksum: (left: Remote.Stat, right: Remote.Stat) =>
    Option.getOrElse(
      Option.zipWith(left.etag, right.etag, (leftEtag, rightEtag) => leftEtag !== rightEtag),
      () => _sizeModtime(left, right),
    ),
} as const

declare namespace Remote {
  type Comparator = keyof typeof _COMPARE
  type SyncAction = Data.TaggedEnum<{
    CopyLeft: { readonly path: string }
    CopyRight: { readonly path: string }
    RemoveLeft: { readonly path: string }
    RemoveRight: { readonly path: string }
    Conflict: { readonly path: string }
  }>
}

const _SyncAction = Data.taggedEnum<Remote.SyncAction>()

const _held = (sql: SqlClient.SqlClient) =>
  SqlSchema.findAll({
    Request: _Side,
    Result: _Stat,
    execute: (at) => sql`SELECT path, bytes, kind, modified, etag FROM sync_listing WHERE pair = ${at.pair} AND side = ${at.side}`,
  })

// The insert variant's own encode IS the null mapping: the optional halves write `null` where the census holds none and
// the landing stamp mints on the rail, so no site beside the model declaration spells either crossing.
const _encoded = Schema.encode(_Listing.insert)

const _persist = (sql: SqlClient.SqlClient, at: typeof _Side.Type, census: ReadonlyArray<Remote.Stat>) =>
  Effect.zipRight(
    sql`DELETE FROM sync_listing WHERE pair = ${at.pair} AND side = ${at.side}`,
    census.length === 0
      ? Effect.void
      : Effect.flatMap(
          Effect.forEach(census, (row) => _encoded(_Listing.insert.make({ ...at, ...row }))),
          (rows) => sql`INSERT INTO sync_listing ${sql.insert(rows)}`,
        ),
  )

// Re-admitted rows arrive decoded, so the prior side is an index over the census shape rather than a second fold.
const _snapshot = (rows: ReadonlyArray<Remote.Stat>): HashMap.HashMap<string, Remote.Stat> =>
  HashMap.fromIterable(rows.map((row) => [row.path, row] as const))

const _delta = (
  compare: (typeof _COMPARE)[Remote.Comparator],
  prior: HashMap.HashMap<string, Remote.Stat>,
  fresh: ReadonlyArray<Remote.Stat>,
) => {
  const held = HashSet.fromIterable(fresh.map((row) => row.path))
  return {
    touched: HashSet.fromIterable(
      fresh
        .filter((row) => Option.match(HashMap.get(prior, row.path), { onNone: () => true, onSome: (past) => compare(past, row) }))
        .map((row) => row.path),
    ),
    removed: HashSet.fromIterable(
      [...HashMap.keys(prior)].filter((path) => !HashSet.has(held, path)),
    ),
  }
}

type _SideState = "touched" | "removed" | "silent"

const _stateOf = (side: { readonly touched: HashSet.HashSet<string>; readonly removed: HashSet.HashSet<string> }, path: string): _SideState =>
  HashSet.has(side.touched, path) ? "touched" : HashSet.has(side.removed, path) ? "removed" : "silent"

const _reconcile = (
  left: { readonly touched: HashSet.HashSet<string>; readonly removed: HashSet.HashSet<string> },
  right: { readonly touched: HashSet.HashSet<string>; readonly removed: HashSet.HashSet<string> },
): ReadonlyArray<Remote.SyncAction> =>
  [...HashSet.union(
    HashSet.union(left.touched, right.touched),
    HashSet.union(left.removed, right.removed),
  )].flatMap((path): ReadonlyArray<Remote.SyncAction> => {
    const l = _stateOf(left, path)
    const r = _stateOf(right, path)
    // ANY change on both sides is a conflict — touched/touched, touched/removed, and removed/touched alike — so a remove racing a modify can neither overwrite nor resurrect
    return l === "touched" && r === "silent"
      ? [_SyncAction.CopyRight({ path })]
      : l === "silent" && r === "touched"
        ? [_SyncAction.CopyLeft({ path })]
        : l === "removed" && r === "silent"
          ? [_SyncAction.RemoveRight({ path })]
          : l === "silent" && r === "removed"
            ? [_SyncAction.RemoveLeft({ path })]
            : l === "removed" && r === "removed"
              ? [] // both sides already agree: nothing to propagate
              : [_SyncAction.Conflict({ path })]
  })

const _end = (side: Remote.End, path: string): Remote.End => ({ origin: side.origin.at(path), session: side.session })

const _settled = (
  conflicted: HashSet.HashSet<string>,
  prior: HashMap.HashMap<string, Remote.Stat>,
  fresh: ReadonlyArray<Remote.Stat>,
): ReadonlyArray<Remote.Stat> => [
  // a conflict path keeps its PRIOR listing row, so the unresolved delta re-surfaces on every run until the caller rules it
  ...fresh.filter((row) => !HashSet.has(conflicted, row.path)),
  ...[...conflicted].flatMap((path) => Option.match(HashMap.get(prior, path), { onNone: () => [], onSome: (row) => [row] })),
]

const _sync = (pair: string, left: Remote.End, right: Remote.End, comparator: Remote.Comparator = "sizeModtime") =>
  Effect.gen(function* () {
    const sql = yield* SqlClient.SqlClient
    const listings = _held(sql)
    const compare = _COMPARE[comparator]
    const priorLeft = _snapshot(yield* listings({ pair, side: "left" }))
    const priorRight = _snapshot(yield* listings({ pair, side: "right" }))
    const leftCensus = yield* _list(left.origin, left.session)
    const rightCensus = yield* _list(right.origin, right.session)
    const actions = _reconcile(
      _delta(compare, priorLeft, leftCensus),
      _delta(compare, priorRight, rightCensus),
    )
    yield* Effect.forEach(actions, (action) =>
      _SyncAction.$match(action, {
        CopyLeft: ({ path }) => _transfer(_end(right, path), _end(left, path)),
        CopyRight: ({ path }) => _transfer(_end(left, path), _end(right, path)),
        RemoveLeft: ({ path }) => _remove(left.origin.at(path), left.session),
        RemoveRight: ({ path }) => _remove(right.origin.at(path), right.session),
        Conflict: () => Effect.void,
      }), { discard: true })
    const conflicted = HashSet.fromIterable(actions.filter(_SyncAction.$is("Conflict")).map((action) => action.path))
    const settledLeft = _settled(conflicted, priorLeft, yield* _list(left.origin, left.session))
    const settledRight = _settled(conflicted, priorRight, yield* _list(right.origin, right.session))
    yield* sql.withTransaction(Effect.zipRight(
      _persist(sql, { pair, side: "left" }, settledLeft),
      _persist(sql, { pair, side: "right" }, settledRight),
    ))
    return { actions, conflicts: actions.filter(_SyncAction.$is("Conflict")) }
  })
```

## [07]-[WATCH_ROWS]

- Owner: the `_WATCHERS` strategy rows — `nativeWatch` (the platform watcher over a row whose protocol pushes changes natively), `execPush` (`inotifywait -m -r` over an ssh exec channel, the lowest-latency remote arm), `poll` (`Schedule`-driven census diff, the universal floor) — the `_FEEDS` record over them, `_strategyOf`, the ONE derivation both the dispatch and the census tap read, and `Remote.watch(origin, session, strategy?)`; intake-grade LOCAL watching stays `object/file.md`'s `Disk.watch` with its settle guard, so the `file:` row here is the non-intake observation posture a raw event answers.
- Packages: `ssh2` (`exec` — the push channel); `@effect/platform` (`FileSystem.watch` — `Stream<WatchEvent, PlatformError>` over the `Create`/`Update`/`Remove` tags); `@effect/platform-node` (`NodeStream.fromReadable`); `effect` (`Stream.splitLines`, `Schedule`, `HashMap`).
- Entry: a mirrored drop tree on a VPS rides `execPush` where the host carries a notify tool; a mounted local tree rides `nativeWatch`; a DAV or FTP origin rides `poll` diffing `etag`/`size`/`modified` snapshots; each emission is a `Remote.Change` the consumer routes into `Remote.intake` or the sync fold.
- Growth: a new strategy is one `_WATCHERS` row naming the capability column it reads, with one `_FEEDS` arm; the poll cadence and the push-tool roster are policy values.
- Law: strategy is capability-derived from the SESSION's proven flag row — `changeNotify` selects the native watcher, `exec` the notify-tool push, and the terminal row needs no column so the walk is total over every scheme; `_strategyOf` is the ONE derivation, so the dispatch and the census tag can never answer different strategies for one session, a server that refused its notify capability at acquire routes to the poll floor instead of arming an arm the probe disproved, and a capability column the selection never reads is the decorative flag this walk forecloses.
- Law: the consumer subscribes ONE change stream regardless — `_FEEDS` is total over the strategy vocabulary, so a strategy with no arm fails at the declaration and strategy stays invisible past the dispatch.
- Law: the native arm reports EVENTS, never census — the platform watcher's three tags map onto the change vocabulary through one frozen row, so a rename-swap arrives as its own pair rather than a diffed absence, and the settle guard an intake needs stays `Disk.watch`'s because this surface promises observation, not admission.
- Law: the poll arm is diff-exact — each cycle's census diffs against the held snapshot by the same comparator rows the sync engine reads, emitting `add`/`change`/`remove` with no phantom events on unchanged trees; a lost push connection re-arms through `Stream.retry` and one full poll cycle reconciles anything missed.

```typescript signature
import { HashMap, Schedule } from "effect"

const _WATCH_ORDER = ["nativeWatch", "execPush", "poll"] as const

declare namespace Remote {
  type Change = { readonly path: string; readonly kind: "add" | "change" | "remove" }
  type WatchStrategy = (typeof _WATCH_ORDER)[number]
}

const _POLL = { cadence: "30 seconds" } as const

// Preference order IS the tuple and the capability column IS the predicate: the terminal row needs none, so the walk
// is total and `changeNotify` becomes the decision datum the flag row declared it to be.
const _WATCHERS = {
  nativeWatch: { needs: Option.some("changeNotify" as const) },
  execPush: { needs: Option.some("exec" as const) },
  poll: { needs: Option.none<keyof Remote.Flags>() },
} as const satisfies { readonly [S in Remote.WatchStrategy]: { readonly needs: Option.Option<keyof Remote.Flags> } }

// `_strategyOf` reads the SESSION's proven flags, so a server refusing its notify capability at acquire routes to
// poll here rather than arming a push arm the probe already disproved.
const _strategyOf = (session: Remote.Session): Remote.WatchStrategy =>
  Option.getOrElse(
    Array.findFirst(_WATCH_ORDER, (strategy) =>
      Option.match(_WATCHERS[strategy].needs, { onNone: () => true, onSome: (flag) => session.flags[flag] })),
    () => "poll" as const, // unreachable: the terminal row's absent column admits every session
  )

// The platform watcher's own tags ARE the change vocabulary: one frozen row, so a widened event family fails here
// rather than folding an unknown tag into `change`.
const _WATCH_EVENTS = {
  Create: "add",
  Update: "change",
  Remove: "remove",
} as const satisfies { readonly [K in FileSystem.WatchEvent["_tag"]]: Remote.Change["kind"] }

const _native = (origin: Origin): Stream.Stream<Remote.Change, RemoteFault, FileSystem.FileSystem> =>
  Stream.unwrap(
    Effect.map(FileSystem.FileSystem, (fs) =>
      fs.watch(origin.path, { recursive: true }).pipe(
        Stream.mapError(_fault(origin, "watch")),
        Stream.map((event): Remote.Change => ({ path: event.path, kind: _WATCH_EVENTS[event._tag] })),
      )),
  )

const _execPush = (origin: Origin, session: Remote.Session): Stream.Stream<Remote.Change, RemoteFault, CommandExecutor.CommandExecutor | Scope.Scope> =>
  Stream.unwrap(
    _exec(origin, session, {
      file: "inotifywait",
      args: ["-m", "-r", "--format", "%e|%w%f", origin.path],
    }).pipe(
      Effect.map((channel) =>
        channel.stdout.pipe(
          Stream.decodeText(),
          Stream.splitLines,
          Stream.filterMap((line) => {
            const [events, path] = line.split("|")
            return events === undefined || path === undefined
              ? Option.none()
              : Option.some<Remote.Change>({
                  path,
                  kind: events.includes("DELETE") ? "remove" : events.includes("CREATE") ? "add" : "change",
                })
          }),
        )),
    ))

const _poll = <R>(
  census: Effect.Effect<ReadonlyArray<Remote.Stat>, RemoteFault, R>,
): Stream.Stream<Remote.Change, RemoteFault, R> =>
  Stream.repeatEffectWithSchedule(census, Schedule.spaced(_POLL.cadence)).pipe(
    Stream.mapAccum(HashMap.empty<string, Remote.Stat>(), (held, snapshot) => {
      const fresh = HashMap.fromIterable(snapshot.map((row) => [row.path, row] as const))
      const changes = [
        ...snapshot.flatMap((row): ReadonlyArray<Remote.Change> =>
          Option.match(HashMap.get(held, row.path), {
            onNone: () => [{ path: row.path, kind: "add" }],
            onSome: (prior) => _COMPARE.sizeModtime(prior, row) ? [{ path: row.path, kind: "change" }] : [],
          })),
        ...[...HashMap.keys(held)].flatMap((path): ReadonlyArray<Remote.Change> =>
          HashMap.has(fresh, path) ? [] : [{ path, kind: "remove" }]),
      ]
      return [fresh, changes] as const
    }),
    Stream.flattenIterables,
  )

type _Feed = Stream.Stream<
  Remote.Change,
  RemoteFault,
  CommandExecutor.CommandExecutor | Scope.Scope | ObjectStore | FileSystem.FileSystem
>

// Total over the strategy vocabulary: a row with no arm fails at the declaration, so the dispatch never widens.
const _FEEDS: { readonly [S in Remote.WatchStrategy]: (origin: Origin, session: Remote.Session) => _Feed } = {
  nativeWatch: (origin) => _native(origin),
  // Re-arms a DROPPED channel, never a refusal, and the curve is `Fault.Budget`'s own feed row rather than a ladder
  // spelled here: its gate reads the recovery BAND (`terminal` never re-drives), so an `exec` denial — a missing
  // notify tool, a command the host rejects — surfaces once instead of re-dialing every cadence tick forever, and its
  // reset window returns the whole attempt budget to any channel that has run clean, so a long-lived watch re-arms
  // indefinitely while a permanently dropped one ends on the row's elapsed ceiling instead of re-dialing silently.
  execPush: (origin, session) => _execPush(origin, session).pipe(Stream.retry(Fault.Budget.schedule("feed"))),
  poll: (origin, session) => _poll(_list(origin, session)),
}

const _watch = (origin: Origin, session: Remote.Session, strategy?: Remote.WatchStrategy): _Feed =>
  _FEEDS[strategy ?? _strategyOf(session)](origin, session)
```

## [08]-[EXEC]

- Owner: `Remote.exec` — command execution as ONE typed surface over the session family: `Remote.Invocation { file, args }` is the sole ingress, the ssh2 boundary shell-quotes each atom once, and the local arm passes the same atoms directly to `Command.make`; `stdout`/`stderr` remain backpressured `Stream`s, `stdin` remains a `Sink`, and `exit` remains a typed effect.
- Packages: `ssh2` (`exec` — channel `Duplex`, `exit` event); `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable`); `@effect/platform` (`Command.make`, `Command.start` — the local arm's process shape).
- Entry: VPS interaction is exec with SFTP against the provisioned address — a deployment probe, a remote build step, the `execPush` watch tool all ride this one surface; provisioning, DNS, firewall, and snapshot lifecycle stay on the deploy plane and never enter this lane.
- Growth: a PTY session is the `shell` sibling over the same channel lift; a jump-host exec is the same call over a `sock`-chained session.
- Law: command structure survives every arm — callers cannot inject a `sh -c` program or interpolate a path into command text; only the SSH boundary renders argv into a POSIX command using single-quote isolation, while the local boundary executes `file` and `args` without a shell. Non-zero exits are data on the result, never exceptions; a channel-level failure is the `exec` fault reason.
- Boundary: the exec callback and the `exit` listener are the channel's boundary kernel — the last statement flow on the page.

```typescript signature
import { CommandExecutor } from "@effect/platform"

declare namespace Remote {
  type Invocation = { readonly file: string; readonly args: ReadonlyArray<string> }
  type Executed = {
    readonly stdin: Sink.Sink<void, Uint8Array, never, RemoteFault>
    readonly stdout: Stream.Stream<Uint8Array, RemoteFault>
    readonly stderr: Stream.Stream<Uint8Array, RemoteFault>
    readonly exit: Effect.Effect<number, RemoteFault>
  }
}

const _shell = (invocation: Remote.Invocation): string =>
  [invocation.file, ...invocation.args].map((part) => `'${part.replaceAll("'", "'\"'\"'")}'`).join(" ")

const _exec = (
  origin: Origin,
  session: Remote.Session,
  invocation: Remote.Invocation,
): Effect.Effect<Remote.Executed, RemoteFault, CommandExecutor.CommandExecutor | Scope.Scope> =>
  _Session.$match(session, {
    Ssh: ({ client }) =>
      Effect.async<Remote.Executed, RemoteFault>((resume) => {
        try {
          client.exec(_shell(invocation), (cause, channel) => {
            if (cause !== undefined && cause !== null) {
              resume(Effect.fail(new RemoteFault({ case: { reason: "exec", origin: origin.host, detail: String(cause) } })))
              return
            }
            let disposition: number | null = null
            channel.once("exit", (code) => {
              disposition = typeof code === "number" ? code : -1
            })
            resume(Effect.succeed({
              stdin: NodeSink.fromWritable(() => channel, _fault(origin, "exec")),
              stdout: NodeStream.fromReadable(() => channel, _fault(origin, "exec")),
              stderr: NodeStream.fromReadable(() => channel.stderr, _fault(origin, "exec")),
              exit: Effect.async<number, RemoteFault>((settle) => {
                if (disposition !== null) {
                  settle(Effect.succeed(disposition))
                  return
                }
                channel.once("close", () => settle(Effect.succeed(disposition ?? -1)))
              }),
            }))
          })
        } catch (cause) {
          // `exec` throws `Not connected` SYNCHRONOUSLY on a session that dropped since acquire; `connect` carries it
          // as `unavailable` so a redial re-drives, where the `exec` reason's `denied` row and an escaping defect
          // both refuse a channel the next dial opens.
          resume(Effect.fail(new RemoteFault({ case: { reason: "connect", origin: origin.host, detail: String(cause) } })))
        }
      }),
    Local: () =>
      Effect.map(
        Effect.mapError(Command.start(Command.make(invocation.file, ...invocation.args)), _fault(origin, "exec")),
        (process): Remote.Executed => ({
          stdin: Sink.mapError(process.stdin, _fault(origin, "exec")),
          stdout: Stream.mapError(process.stdout, _fault(origin, "exec")),
          stderr: Stream.mapError(process.stderr, _fault(origin, "exec")),
          exit: Effect.mapError(process.exitCode, _fault(origin, "exec")),
        }),
      ),
    Ftp: () => Effect.fail(new RemoteFault({ case: { reason: "exec", origin: origin.host, detail: "<exec:unsupported>" } })),
    Dav: () => Effect.fail(new RemoteFault({ case: { reason: "exec", origin: origin.host, detail: "<exec:unsupported>" } })),
    Bucket: () => Effect.fail(new RemoteFault({ case: { reason: "exec", origin: origin.host, detail: "<exec:unsupported>" } })),
  })

// `_OPS` closes the verb axis: Effect-shaped verbs fold through `_measured`, stream verbs tap per emission inside
// their feeds, keys outside this tuple are data the record publishes rather than verbs on the axis, and verbs the
// record never publishes fail the guard.
const _OPS = [
  "probe", "stat", "list", "read", "write", "mkdir", "remove", "copy", "move", "lock", "unlock", "intake",
  "transfer", "sync", "watch", "exec",
] as const

// Every verb crosses ONE combinator at the record, so measurement is a property of the surface rather than of each
// implementation: an origin-addressed verb names its own origin, a pair-addressed one measures at the destination it
// writes, and the two stream verbs tap their census PER EMISSION inside the feed — a stream's span is its consumer's
// lifetime, so the number lands where each emission already holds it and no setup wrapper pretends to measure a
// life it never sees. A new verb is a row that arrives already measured.
const Remote = {
  schemes: _SCHEMES,
  engines: _ENGINES,
  compare: _COMPARE,
  Session: _Session,
  SyncAction: _SyncAction,
  ddl: [_listingDdl],
  session: _session,
  sessions: _sessions,
  probe: (origin: Origin, session: Remote.Session) => _measured("probe", origin, _probe(origin, session)),
  stat: (origin: Origin, session: Remote.Session) => _measured("stat", origin, _stat(origin, session)),
  list: (origin: Origin, session: Remote.Session) => _measured("list", origin, _list(origin, session)),
  // The octet census taps per chunk, where the count already exists; the span belongs to the consumer that drains it.
  read: (origin: Origin, session: Remote.Session, offset?: number) =>
    Stream.tap(_read(origin, session, offset), (chunk) => _moved(chunk.byteLength)),
  write: (origin: Origin, session: Remote.Session, at?: number) => _measured("write", origin, _write(origin, session, at)),
  mkdir: (origin: Origin, session: Remote.Session) => _measured("mkdir", origin, _mkdir(origin, session)),
  remove: (origin: Origin, session: Remote.Session) => _measured("remove", origin, _remove(origin, session)),
  copy: (from: Remote.End, to: Remote.End) => _measured("copy", to.origin, _copy(from, to)),
  move: (from: Remote.End, to: Remote.End) => _measured("move", to.origin, _move(from, to)),
  lock: (origin: Origin, session: Remote.Session) => _measured("lock", origin, _lock(origin, session)),
  unlock: (origin: Origin, session: Remote.Session, token: string) => _measured("unlock", origin, _unlock(origin, session, token)),
  // The remote hop alone: the octets crossing into `putKeyed` are already `objectSize`'s census on the object plane.
  intake: (origin: Origin, session: Remote.Session, retention: Retain.Class) =>
    _measured("intake", origin, Effect.tap(_intake(origin, session, retention), (landed) => _moved(landed.bytes))),
  // The resume census rides `_transfer`, where the settled engine is in hand: a re-derivation here would answer a
  // second walk over origins the selection already adjudicated.
  transfer: (from: Remote.End, to: Remote.End, policy?: Remote.Policy) =>
    _measured("transfer", to.origin, _transfer(from, to, policy)),
  sync: (pair: string, left: Remote.End, right: Remote.End, comparator: Remote.Comparator = "sizeModtime") =>
    _measured(
      "sync",
      right.origin,
      Effect.tap(_sync(pair, left, right, comparator), (settled) =>
        Effect.forEach(settled.actions, (action) =>
          Metric.increment(Metric.tagged(_syncActions, Convention.rasm.remoteAction, action._tag)), {
          concurrency: "inherit",
          discard: true,
        })),
    ),
  // The change census taps per emission inside the feed, reading the SAME `_strategyOf` derivation the dispatch took,
  // so the tag can never name a strategy other than the one arming the stream.
  watch: (origin: Origin, session: Remote.Session, strategy?: Remote.WatchStrategy) =>
    Stream.tap(_watch(origin, session, strategy), () =>
      Metric.increment(
        Metric.tagged(
          Metric.tagged(_watched, Convention.rasm.remoteScheme, origin.scheme),
          Convention.rasm.remoteWatch,
          strategy ?? _strategyOf(session),
        ),
      )),
  // A non-zero exit is DATA, so the exit census fans on the disposition rather than on a fault the channel never raised.
  exec: (origin: Origin, session: Remote.Session, invocation: Remote.Invocation) =>
    _measured(
      "exec",
      origin,
      Effect.map(_exec(origin, session, invocation), (executed): Remote.Executed => ({
        ...executed,
        exit: Effect.tap(executed.exit, (code) =>
          Metric.increment(
            Metric.tagged(
              Metric.tagged(_exits, Convention.rasm.remoteScheme, origin.scheme),
              Convention.attr.errorType,
              code === 0 ? "resolved" : `exit:${code}`,
            ),
          )),
      })),
    ),
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Origin, Remote, RemoteFault }
```

## [09]-[INSTRUMENT_ROWS]

- Owner: the remote plane's Convention projections — the mounted instrument rows, `_measured`, the ONE combinator the `Remote` entry record folds every verb through, and the bounded taps the legs that own their own census carry (`_read`/`_write` octets, `_transfer` resume, `_sync`/`_watch`/`_exec` rows); the pooled-session level stays `CacheLane.lease`'s.
- Packages: `effect` (`Metric`, `Effect`, `Duration`); `@rasm/ts/core` (`Convention` — the instrument, axis, outcome, and duration projections).
- Entry: the `Remote` record folds each verb through `_measured(op, origin, self)` at ONE site, so the surface is instrumented by the record rather than by a tap per member and a new verb inherits measurement by construction; the census taps ride inside the legs that already hold the number.
- Growth: a new verb is one entry-record row the fold already covers; a new axis is one `Convention` row with its tap on the owning leg.
- Law: the operation counter is the core outcome aspect, never a hand-rolled fold — `Convention.outcome(Convention.metric.remoteOps, Convention.attr.errorType, Fault.Class, …)` owns the single emission point, the interrupt-first discrimination, and the `halted`/`crashed`/reason vocabulary; the third argument is the CENSUS the words come from, so `Fault.Class` names the roster and this page supplies only its own projection onto it (`RemoteFault.class`, the core kind the family already derives) and spells no `Effect.onExit`; a page-local exit fold double-counts every retried attempt and never sees a defect.
- Law: the region axes ride the FIBER, not the handle — `Effect.tagMetrics` stamps `remoteOp` and `remoteScheme` across every metric the governed effect updates, so the outcome aspect keeps its own mounted handle while the two dimensions the row declares still land; pre-tagging the aspect's handle is unspellable because the aspect mounts internally, and re-mounting the row beside it forks one series into two registry entries.
- Law: `remoteDuration` is a SUMMARY, so its update takes the scaled NUMBER — the row names a sliding quantile window because a local `stat` and a multi-gigabyte rsync ride one instrument and no frozen ladder answers both, and Effect constrains a summary's carrier to a bare number, so the site passes `Convention.duration(...)` where a bucketed row takes the `Duration` itself; handing this row a `Duration` is the one mount-shape error the kind's carrier makes unspellable.
- Law: octets count where the count already EXISTS — `remoteBytes` taps the transfer legs that hold a byte number, never a stream the page measures by adding a fold; bytes crossing into `putKeyed` are already `objectSize`'s, so `Remote.intake` taps the remote hop alone and the object plane keeps its own census with no double count.
- Law: the pooled-session level belongs to the POOL, so `CacheLane.lease` owns it whole and this page mounts no second row — one pool answering two series hands a board two held-connection numbers no reader can reconcile, and the lane's road already brackets the caller's own scope, tags the key's scheme, and serves every other consumer of that pool by construction; a level minted here measures the lane's lease under a name only this page publishes.
- Law: identifier-grade context rides the SPAN — `Effect.withSpan("data.remote", { attributes: { op, scheme, host } })` carries the host, while the metric axes stay the bounded scheme, verb, action, engine, and strategy vocabularies each closed by its own roster; a host interpolated into a tag mints one series per origin.

```typescript signature
import { Duration, Effect, Metric } from "effect"
import { Convention, Fault } from "@rasm/ts/core"

const _bytes = Convention.mount(Convention.metric.remoteBytes)
const _exits = Convention.mount(Convention.metric.remoteExecExits)
const _resumed = Convention.mount(Convention.metric.remoteResumed)
const _spanned = Convention.mount(Convention.metric.remoteDuration)
const _syncActions = Convention.mount(Convention.metric.remoteSyncActions)
const _watched = Convention.mount(Convention.metric.remoteWatchChanges)

// The core aspect owns the emission point and the outcome vocabulary; this page supplies only the reason projection,
// and the reason IS the fault's own core kind, so the refusal fan and the landed half partition one series.
const _counted = Convention.outcome(
  Convention.metric.remoteOps,
  Convention.attr.errorType,
  Fault.Class,
  (fault: RemoteFault) => fault.class,
)

declare namespace Remote {
  type Op = (typeof _OPS)[number] // the verb roster closes the axis; the record derives from it, never the reverse
  type _Verbs<K extends keyof typeof Remote = Op> = K // a measured verb the entry record does not publish fails here
}

// ONE fold at the entry record instruments every verb. The two region axes ride the fiber through `tagMetrics`
// because the outcome aspect mounts its own handle internally and admits no pre-tag, and the summary takes the
// scaled number its carrier declares rather than the `Duration` a bucketed row would accept.
const _measured = <A, R>(op: Remote.Op, origin: Origin, self: Effect.Effect<A, RemoteFault, R>): Effect.Effect<A, RemoteFault, R> =>
  Effect.timed(self).pipe(
    Effect.tap(([elapsed]) => Metric.update(_spanned, Convention.duration(Convention.metric.remoteDuration, elapsed))),
    Effect.map(([, value]) => value),
    _counted,
    Effect.tagMetrics({ [Convention.rasm.remoteOp]: op, [Convention.rasm.remoteScheme]: origin.scheme }),
    Effect.withSpan("data.remote", { attributes: { op, scheme: origin.scheme, host: origin.host } }),
  )

// Octets tap where the leg already holds the number, so no fold is added to measure what a transfer already counted.
const _moved = (octets: number): Effect.Effect<void> => Metric.incrementBy(_bytes, octets)
```

## [10]-[RESEARCH]

<!-- source-only: research row template; every landed row opens on the list dash this placeholder omits, the census reading `^- [TOKEN]-[STATUS]:` alone:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
