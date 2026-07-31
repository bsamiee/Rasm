# [DATA_REMOTE]

The remote-origin filesystem plane: ONE origin-addressed surface owning every non-local byte tree — SFTP and SSH-exec over the in-process `ssh2` root, FTP/FTPS over `basic-ftp`, WebDAV over `webdav`, the object plane reached as the `s3:` row — with capability flags as data so every polymorphic operation degrades by row, never by fork. An `Origin` value carries scheme, coordinate, and path; its scheme selects the backend row; the row's flags decide whether a copy is server-side or read-then-write, whether a watch pushes or polls, whether a transfer resumes by rsync delta, byte offset, or parallel chunks. Two transfer engines share one policy surface: the in-process ssh2 lane (boundary-adapted callbacks, `NodeStream`/`NodeSink` channel lifts) and the external `rsync`/`scp`/`ssh` binaries as `@effect/platform` `Command` processes. Connections pool through `lane/cache.md`'s bounded origin row; every remote read feeds the SAME content-addressed intake fold as local disk, so a cloud project grows zero new addressing vocabulary; the sync engine persists listings and reconciles deltas with resume and recover, and remote watch is a strategy row beside the local chokidar arm.

## [01]-[INDEX]

- [02]-[ORIGIN_ROWS]: the `Origin` class, the scheme capability-flag table, the reason-discriminated fault.
- [03]-[SESSION_ROWS]: the tagged session family, per-scheme scoped brackets, pooled reuse, the flag probe.
- [04]-[OP_SURFACE]: the polymorphic verb set — stat/list/read/write/copy/move/remove/mkdir/lock, degrade.
- [05]-[TRANSFER_ENGINES]: the resume policy rows — rsync delta, offset, chunked-parallel — and the intake fold.
- [06]-[SYNC_ENGINE]: persisted listings, comparator rows, the diff-apply-recover fold.
- [07]-[WATCH_ROWS]: the watch strategy rows — ssh exec-push, universal poll; local intake stays owned.
- [08]-[EXEC]: remote command execution — typed channels, exit disposition, the local `Command` twin.
- [09]-[INSTRUMENT_ROWS]: the Convention projections — the one `_measured` fold at the entry record and the bounded census taps.

## [02]-[ORIGIN_ROWS]

- Owner: `Origin` — one `Schema.Class` whose `scheme` field selects the backend row, with `Origin.parse` decoding a URI string through one fused codec; the `_SCHEMES` capability-flag table (the rclone backend model: server-side copy/move, streaming put, change notify, exec, offset resume, parallel transfer, locks); `RemoteFault` — the one reason-discriminated fault of the plane, its rows closed through the core `FaultClass.family` seam.
- Packages: `effect` (`Schema`, `Data`, `Either`, `ParseResult`); `@rasm/ts/core` (`FaultClass`).
- Entry: every consumer addresses the plane by `Origin` value — `Origin.parse("sftp://deploy@vps.example:22/srv/artifacts")` at a config seam, never scheme-forked code; `origin.flags` is the dispatch datum every operation reads.
- Growth: a new protocol is one scheme key, one flag row, one session arm, and the op arms it earns — every consumer inherits it through the flags; a flag a server refuses at runtime narrows by row override, never by fork.
- Law: capability flags are decision data — `serverCopy: false` makes `copy` degrade to read-then-write, `serverMove: false` makes `move` degrade to copy-then-remove, `changeNotify: false` routes `watch` to the poll row, `exec: false` refuses `exec` typed; the degrade paths live in the op arms once, so no caller ever probes a protocol.
- Law: the `s3:` row is a bridge, not a re-implementation — its ops delegate to `object/store.md` and `object/stream.md` owners (`head`/`rekey`/`Rail.range`, the intake fold for ingress), so the object plane's conditional-put and grant law hold unchanged behind the origin address.
- Law: `RemoteFault` reasons route recovery as a fold — `connect` and `auth` invalidate the pooled session, `op` and `transfer` re-drive, `watch` re-arms the strategy, `exec` carries the exit disposition; a free-string-only fault is the named unroutable defect.
- Law: recovery policy reads the core lattice off `class` — `connect`, `op`, `transfer`, and `watch` classify `unavailable` (system-blamed, retryable: a re-dial, a re-issued operation, and a re-armed notify all heal them), while `auth` and `exec` classify `denied` (caller-blamed, never re-driven: refused credentials and a refused or non-idempotent remote invocation) — so retryability, blame, and quarantine derive from the core `FaultClass` row table and no rank or retry column rides beside `class`.

```typescript signature
import { Data, Either, ParseResult, Schema } from "effect"
import { FaultClass } from "@rasm/ts/core"

const _SCHEME_KEYS = ["file", "sftp", "ssh", "ftp", "ftps", "webdav", "s3"] as const

const _SCHEMES = {
  file: { serverCopy: true, serverMove: true, putStream: true, changeNotify: true, exec: true, offsetResume: true, parallel: false, lock: false },
  sftp: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: true, offsetResume: true, parallel: true, lock: false },
  ssh: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: true, offsetResume: true, parallel: true, lock: false },
  ftp: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, parallel: false, lock: false },
  ftps: { serverCopy: false, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, parallel: false, lock: false },
  webdav: { serverCopy: true, serverMove: true, putStream: true, changeNotify: false, exec: false, offsetResume: true, parallel: false, lock: true },
  s3: { serverCopy: true, serverMove: false, putStream: true, changeNotify: false, exec: false, offsetResume: true, parallel: true, lock: false },
} as const

const _PORTS = { file: 0, sftp: 22, ssh: 22, ftp: 21, ftps: 990, webdav: 443, s3: 443 } as const

// One row per reason: the core kind alone. Retryability, blame, and quarantine are the core FaultClass row
// table's — a rank or retry column here would fork that taxonomy into this folder.
const _family = FaultClass.family(["connect", "auth", "op", "transfer", "watch", "exec"] as const, {
  connect: { class: "unavailable" },
  auth: { class: "denied" },
  op: { class: "unavailable" },
  transfer: { class: "unavailable" },
  watch: { class: "unavailable" },
  exec: { class: "denied" },
})

declare namespace Remote {
  type Scheme = (typeof _SCHEME_KEYS)[number]
  type Flags = (typeof _SCHEMES)[Scheme]
  type Reason = (typeof _family.reasons)[number]
  type _Rows<T extends { readonly [S in Scheme]: { readonly [F in keyof Flags]: boolean } } = typeof _SCHEMES> = T
}

class RemoteFault extends Schema.TaggedError<RemoteFault>()("RemoteFault", {
  reason: _family.schema,
  origin: Schema.String,
  detail: Schema.String,
}) {
  get class(): FaultClass.Kind {
    return _family.classOf(this.reason)
  }
  override get message(): string {
    return `<remote:${this.reason}> ${this.origin}: ${this.detail}`
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
      Either.try({
        try: () => {
          const parsed = new URL(uri)
          const scheme = _SCHEME_KEYS.find((key) => `${key}:` === parsed.protocol)
          if (scheme === undefined) {
            throw new Error(parsed.protocol)
          }
          return {
            scheme,
            host: parsed.hostname,
            port: parsed.port === "" ? _PORTS[scheme] : Number.parseInt(parsed.port, 10),
            username: decodeURIComponent(parsed.username),
            path: decodeURIComponent(parsed.pathname),
          }
        },
        catch: () => new ParseResult.Type(ast, uri),
      }),
    encode: (origin) =>
      ParseResult.succeed(`${origin.scheme}://${origin.username}@${origin.host}:${origin.port}${origin.path}`),
  }))

  get flags(): Remote.Flags {
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

- Owner: `Session` — one closed `Data.taggedEnum` family (`Ssh | Ftp | Dav | Bucket | Local`) every op narrows through `$match`, so a client cast is unspellable; the per-scheme session brackets — the ssh2 connect-on-`ready`/`end()`-on-release bracket with the SFTP subsystem lift, the `basic-ftp` `access` dial, the `webdav` client mint — `_session`, the one scheme-dispatched acquire; and `Remote.sessions`, the one acquisition surface that pools network control sessions through `lane/cache.md`'s `CacheLane.origins` while minting connectionless `Bucket` and `Local` values directly.
- Packages: `ssh2` (`Client` — `connect`, `sftp`, `end`; events `ready`/`error`; config auth/trust/keepalive rows; `sock` jump-host injection); `basic-ftp` (`Client`, `access`, `close`); `webdav` (`createClient`, `AuthType`); `lane/cache.md` (`CacheLane.origins`, `OriginKey`); `effect` (`Effect`, `Data`, `KeyedPool.get`, `Redacted`, `Scope`).
- Entry: an operation calls `sessions.get(origin)` — network origins lease through `KeyedPool.get` under the caller's `Scope`, so the FTP one-transfer-per-control-connection law and SSH connection reuse are pool facts; `file:` and `s3:` origins mint free values because their capability arrives from the `FileSystem`/`ObjectStore` requirement channel and no inert pool key exists.
- Growth: an auth posture (agent, keyboard-interactive, custom `authHandler`) is an `Auth` field flowing into the connect config; a bastion chain is the prior hop's `forwardOut` duplex entering the next `connect` through `sock` — config data, never topology code.
- Law: sessions are scoped brackets — ssh resolves on `ready`, fails typed on `error`, releases through `end()`; ftp dials through `access` alone (the split connect/login members are probes) and releases through `close()`; a bare client with ad-hoc listeners in domain code is the rejected spelling.
- Law: credentials are `Redacted` config rows — password, private key, passphrase never appear as literals; host trust rides `hostVerifier` where the deployment pins keys; TLS on the ftp row is the `Auth.secure` config value (`true` explicit upgrade, `"implicit"` wrapped, scheme-derived by default and overridable only as ruled config for a plaintext-only origin), never a scheme fork beyond the `ftps:` port default.
- Law: capability discovery narrows the flag row at acquire — `Remote.probe` reads `getDAVCompliance` on the DAV arm (class `"2"` proves the lock row) and `features()` on the FTP arm (`REST` proves offset resume), folding server truth over the scheme's static flags and carrying the `getQuota` capacity fact as an `Option` — a flag a server refuses therefore narrows by data before any op dispatches, never by a caller branch.
- Boundary: the ssh2 and basic-ftp surfaces are callback/Promise boundary kernels — every listener registration and promise lives inside these brackets, and above them only `Stream`/`Sink`/typed effects exist.

```typescript signature
import { Effect, KeyedPool, Option, Redacted, type Scope } from "effect"
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
  type Session = Data.TaggedEnum<{
    Ssh: { readonly client: SshClient }
    Ftp: { readonly client: FtpClient }
    Dav: { readonly client: WebDAVClient }
    Bucket: {}
    Local: {}
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
      client
        .once("ready", () => resume(Effect.succeed(client)))
        .once("error", (cause) =>
          resume(Effect.fail(new RemoteFault({ reason: "connect", origin: origin.host, detail: String(cause) }))))
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
      return Effect.sync(() => client.end())
    }),
    (client) => Effect.sync(() => client.end()),
  )

const _sftp = (client: SshClient, origin: Origin): Effect.Effect<SFTPWrapper, RemoteFault> =>
  Effect.async<SFTPWrapper, RemoteFault>((resume) => {
    client.sftp((cause, wrapper) =>
      cause === undefined || cause === null
        ? resume(Effect.succeed(wrapper))
        : resume(Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: String(cause) }))))
  })

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
          // the scheme derives the TLS row — ftps wraps implicit, ftp upgrades explicit — and auth.secure overrides for the plaintext-only origin, a ruled config value
          secure: auth.secure ?? (origin.scheme === "ftps" ? "implicit" : true),
        })
        return client
      },
      catch: (cause) => new RemoteFault({ reason: "connect", origin: origin.host, detail: String(cause) }),
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
    catch: (cause) => new RemoteFault({ reason: "auth", origin: origin.host, detail: String(cause) }),
  })

const _session = (origin: Origin, auth: Remote.Auth): Effect.Effect<Remote.Session, RemoteFault, Scope.Scope> =>
  ({
    file: () => Effect.succeed(_Session.Local()),
    sftp: () => Effect.map(_ssh(origin, auth), (client) => _Session.Ssh({ client })),
    ssh: () => Effect.map(_ssh(origin, auth), (client) => _Session.Ssh({ client })),
    ftp: () => Effect.map(_ftp(origin, auth), (client) => _Session.Ftp({ client })),
    ftps: () => Effect.map(_ftp(origin, auth), (client) => _Session.Ftp({ client })),
    webdav: () => Effect.map(_dav(origin, auth), (client) => _Session.Dav({ client })),
    s3: () => Effect.succeed(_Session.Bucket()),
  } satisfies { readonly [S in Remote.Scheme]: () => Effect.Effect<Remote.Session, RemoteFault, Scope.Scope> })[origin.scheme]()

const _sessions = (auth: (key: OriginKey) => Remote.Auth): Effect.Effect<Remote.Sessions, never, Scope.Scope> =>
  Effect.map(
    CacheLane.origins((key: OriginKey) =>
      Effect.flatMap(
        Effect.mapError(
          Origin.parse(`${key.scheme}://${key.username}@${key.host}:${key.port}/`),
          (fault) => new RemoteFault({ reason: "connect", origin: key.host, detail: String(fault) }),
        ),
        (origin) => _session(origin, auth(key)),
      )),
    (pool) => ({
      // The lease level rides the POOL arm alone: a connectionless origin mints a free value and holds no session, so
      // gauging it would report a lease the plane never took. The bracket owns both edges, so an interrupted acquire
      // cannot leave the level raised.
      get: (origin) =>
        origin.scheme === "file" || origin.scheme === "s3"
          ? _session(origin, {})
          : _leased(origin.scheme, KeyedPool.get(pool, origin.key)),
    }),
  )

declare namespace Remote {
  type Probed = {
    // server truth widens the static literal row: every flag stays present, its value proven live
    readonly flags: { readonly [F in keyof Flags]: boolean }
    readonly quota: Option.Option<{ readonly used: number; readonly available: Option.Option<number> }>
  }
}

const _probe = (origin: Origin, session: Remote.Session): Effect.Effect<Remote.Probed, RemoteFault> =>
  _Session.$match(session, {
    Dav: ({ client }) =>
      Effect.zipWith(
        Effect.tryPromise({ try: () => client.getDAVCompliance(origin.path), catch: _fault(origin, "op") }),
        Effect.tryPromise({ try: () => client.getQuota(), catch: _fault(origin, "op") }),
        (compliance, quota): Remote.Probed => ({
          flags: { ...origin.flags, lock: compliance.compliance.includes("2") },
          quota: Option.map(Option.fromNullable(quota), (held) => ({
            used: typeof held.used === "number" ? held.used : 0,
            available: typeof held.available === "number" ? Option.some(held.available) : Option.none(),
          })),
        }),
      ),
    Ftp: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.features(), catch: _fault(origin, "op") }),
        (features): Remote.Probed => ({
          flags: { ...origin.flags, offsetResume: features.has("REST") },
          quota: Option.none(),
        }),
      ),
    Ssh: () => Effect.succeed<Remote.Probed>({ flags: origin.flags, quota: Option.none() }),
    Bucket: () => Effect.succeed<Remote.Probed>({ flags: origin.flags, quota: Option.none() }),
    Local: () => Effect.succeed<Remote.Probed>({ flags: origin.flags, quota: Option.none() }),
  })
```

## [04]-[OP_SURFACE]

- Owner: the polymorphic verb set — `stat`, `list`, `read` (→ backpressured `Stream`), `write` (← `Sink`, offset-positioned when resuming), `copy`, `move`, `remove`, `mkdir` — each ONE entry dispatching through `Session.$match` with flag-driven degrade arms; and `Remote.intake`, the content-addressed landing that runs any remote read through the SAME identity fold as local disk.
- Packages: `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable` — the only stream seams); `ssh2` (SFTP `stat`, `readdir`, `createReadStream`, `createWriteStream`, `rename`, `unlink`, `mkdir`, `rmdir`); `webdav` (`stat`, `getDirectoryContents`, `createReadStream`, `createWriteStream`, `copyFile`, `moveFile`, `deleteFile`, `createDirectory`); `basic-ftp` (`list`, `size`, `lastMod`, `downloadTo`, `uploadFrom`, `appendFrom`, `rename`, `remove`, `removeDir`, `ensureDir`); `@aws-sdk/client-s3` (`paginateListObjectsV2` — the bucket census walk); `object/stream.md` (`Rail.bytes`, `Rail.chunked`, `Rail.identity`, `Rail.range`), `object/store.md` (`ObjectStore`).
- Entry: `Remote.read(origin, session)` yields `Stream<Uint8Array, RemoteFault>` on every scheme; `Remote.intake(origin, session, retention)` is the one cloud-ingestion entry — read, cut, digest, conditional put, reference row, retention tag — identical receipts to `Disk.intake`.
- Growth: a new verb is one dispatch surface with per-row arms; per-server capability discovery is `[3]`'s `Remote.probe`, narrowing the flag row before the arms dispatch, never a caller branch.
- Law: `lock`/`unlock` realize the flag row's `lock` column — RFC 4918 tokens on the DAV arm coordinating against concurrent DAV writers, a typed refusal everywhere else (the bucket arm names why: the conditional put already owns write races) — so a `lock: true` flag is load-bearing capability, never decorative data.
- Law: reads and writes are backpressured lifts — SFTP and DAV node streams cross through `NodeStream.fromReadable`/`NodeSink.fromWritable`, the FTP arm bridges its `Writable`-consuming transfer through one relay duplex inside the boundary; no raw `.on("data")` consumption exists past the adapter.
- Law: degrade is structural — `copy` on a row without `serverCopy` (or across hosts) composes `read` into `write`; `move` without `serverMove` composes `copy` then `remove`; `remove` discriminates file-versus-directory on the `stat` verdict, never a caller flag; a caller cannot observe which arm ran except through the receipt.
- Law: the `s3:` arms honor content addressing — reads ride `Rail.range`, the server-side copy rides `rekey` against the probed ETag, byte ingress rides `Remote.intake`, and deletion rides the object plane's reference release; a raw bucket sink, a unilateral bucket delete, or a bucket-source `move` refuses typed BEFORE any byte moves — re-parenting a content object is a ledger verb, and a copy-then-refuse partial mutation is unspellable because the refusal guards the whole verb.
- Law: every remote byte that becomes durable rides `Remote.intake` — the origin row grows no second addressing vocabulary, dedup and 412-idempotency arrive from the object plane for free, and a remote origin is therefore a first-class artifact source.
- Boundary: the SFTP callback verbs (`stat`, `readdir`, `mkdir`, `rmdir`, `unlink`, `rename`) are the page's callback kernels — each wraps one `Effect.async` settle and nothing else; timestamp normalization to ISO text lives inside those kernels.

```typescript signature
import { Chunk, DateTime, Number, Sink, Stream } from "effect"
import { FileSystem } from "@effect/platform"
import { NodeSink, NodeStream } from "@effect/platform-node"
import { PassThrough } from "node:stream"
import { paginateListObjectsV2 } from "@aws-sdk/client-s3"
import { ContentKey } from "@rasm/ts/core"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Remote {
  type Stat = {
    readonly path: string
    readonly bytes: number
    readonly modified: Option.Option<string>
    readonly kind: "file" | "directory"
    readonly etag: Option.Option<string>
  }
}

const _fault = (origin: Origin, reason: Remote.Reason) => (cause: unknown): RemoteFault =>
  new RemoteFault({ reason, origin: origin.host, detail: String(cause) })

const _keyed = (origin: Origin): Effect.Effect<ContentKey, RemoteFault> =>
  Effect.mapError(
    Schema.decodeUnknown(ContentKey)(origin.path.slice(1)),
    (fault) => new RemoteFault({ reason: "op", origin: origin.host, detail: String(fault) }),
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
      Stream.unwrap(
        Effect.sync(() => {
          const relay = new PassThrough()
          void client.downloadTo(relay, origin.path, offset ?? 0).catch((cause: unknown) => relay.destroy(new Error(String(cause))))
          return NodeStream.fromReadable(() => relay, _fault(origin, "transfer"))
        })),
    Dav: ({ client }) =>
      NodeStream.fromReadable(
        () => client.createReadStream(origin.path, offset === undefined ? {} : { range: { start: offset } }),
        _fault(origin, "op"),
      ),
    Bucket: () =>
      Stream.unwrap(
        Effect.map(_keyed(origin), (key) =>
          Rail.range(key, offset === undefined ? undefined : { from: offset }).pipe(
            Stream.mapError((fault) => new RemoteFault({ reason: "op", origin: origin.host, detail: fault.detail })),
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
      Effect.sync(() => {
        const relay = new PassThrough()
        void (at === undefined ? client.uploadFrom(relay, origin.path) : client.appendFrom(relay, origin.path))
          .catch((cause: unknown) => relay.destroy(new Error(String(cause))))
        return NodeSink.fromWritable(() => relay, _fault(origin, "transfer"))
      }),
    Dav: ({ client }) =>
      at === undefined
        ? Effect.succeed(NodeSink.fromWritable(() => client.createWriteStream(origin.path), _fault(origin, "op")))
        : Effect.succeed(
            // the DAV resume arm: a bounded tail collects and lands as one ranged PATCH — partialUpdateFileContents is the davRange row's write half
            Sink.mapEffect(Sink.collectAll<Uint8Array>(), (held) => {
              const parts = Chunk.toReadonlyArray(held)
              const span = Number.sumAll(parts.map((part) => part.byteLength))
              return Effect.tryPromise({
                try: () => client.partialUpdateFileContents(origin.path, at, at + span - 1, Buffer.concat(parts)),
                catch: _fault(origin, "transfer"),
              })
            }),
          ),
    Bucket: () =>
      Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<bucket:write-rides-intake>" })),
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
                  modified: Option.some(new Date(held.mtime * 1000).toISOString()),
                  kind: held.isDirectory() ? "directory" as const : "file" as const,
                  etag: Option.none(),
                }))
              : resume(Effect.fail(_fault(origin, "op")(cause))))
        })),
    Ftp: ({ client }) =>
      Effect.tryPromise({
        try: async () => {
          const bytes = await client.size(origin.path)
          const modified = await client.lastMod(origin.path)
          return { path: origin.path, bytes, modified: Option.some(modified.toISOString()), kind: "file" as const, etag: Option.none() }
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
            modified: Option.fromNullable(row.lastmod),
            kind: row.type === "directory" ? "directory" as const : "file" as const,
            etag: Option.fromNullable(row.etag),
          }
        }),
    Bucket: () =>
      Effect.flatMap(_keyed(origin), (key) =>
        Effect.flatMap(ObjectStore, (store) =>
          Effect.map(
            Effect.mapError(store.head(key), (fault) => new RemoteFault({ reason: "op", origin: origin.host, detail: fault.detail })),
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
                  modified: Option.some(new Date(entry.attrs.mtime * 1000).toISOString()),
                  kind: entry.attrs.isDirectory() ? "directory" as const : "file" as const,
                  etag: Option.none(),
                }))))
              : resume(Effect.fail(_fault(origin, "op")(cause))))
        })),
    Ftp: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.list(origin.path), catch: _fault(origin, "op") }),
        (entries) => entries.map((entry) => ({
          path: `${origin.path}/${entry.name}`,
          bytes: entry.size,
          modified: Option.fromNullable(entry.rawModifiedAt),
          kind: entry.isDirectory ? "directory" as const : "file" as const,
          etag: Option.none(),
        }))),
    Dav: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.getDirectoryContents(origin.path), catch: _fault(origin, "op") }),
        (held) => ("data" in held ? held.data : held).map((row) => ({
          path: row.filename,
          bytes: row.size,
          modified: Option.fromNullable(row.lastmod),
          kind: row.type === "directory" ? "directory" as const : "file" as const,
          etag: Option.fromNullable(row.etag),
        }))),
    Bucket: () =>
      Effect.flatMap(ObjectStore, (store) =>
        Effect.map(
          Stream.runCollect(
            Stream.fromAsyncIterable(
              paginateListObjectsV2({ client: store.client }, { Bucket: store.bucket, Prefix: origin.path.slice(1) }),
              (cause) => new RemoteFault({ reason: "op", origin: origin.host, detail: String(cause) }),
            ).pipe(
              Stream.mapConcatEffect((page) =>
                Effect.forEach(page.Contents ?? [], (entry) =>
                  entry.Key === undefined || entry.Size === undefined
                    ? Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<incomplete-list-entry>" }))
                    : Effect.succeed<Remote.Stat>({
                        path: `/${entry.Key}`,
                        bytes: entry.Size,
                        modified: Option.map(Option.fromNullable(entry.LastModified), (time) => time.toISOString()),
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
        Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<bucket:remove-is-release>" })),
      Local: () =>
        Effect.flatMap(FileSystem.FileSystem, (fs) =>
          Effect.mapError(fs.remove(origin.path, { recursive: held.kind === "directory" }), _fault(origin, "op"))),
    }))

const _piped = (from: Remote.End, to: Remote.End, at?: number) =>
  Effect.asVoid(
    Effect.flatMap(_write(to.origin, to.session, at), (sink) =>
      Stream.run(_read(from.origin, from.session, at), sink)))

const _copy = (from: Remote.End, to: Remote.End): Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  from.origin.scheme !== to.origin.scheme || from.origin.host !== to.origin.host || !from.origin.flags.serverCopy
    ? _piped(from, to)
    : _Session.$match(to.session, {
        Ssh: () => _piped(from, to),
        Ftp: () => _piped(from, to),
        Dav: ({ client }) =>
          Effect.tryPromise({ try: () => client.copyFile(from.origin.path, to.origin.path), catch: _fault(to.origin, "op") }),
        Bucket: () =>
          Effect.flatMap(Effect.all([_keyed(from.origin), _keyed(to.origin)]), ([source, target]) =>
            Effect.flatMap(ObjectStore, (store) =>
              Effect.asVoid(
                Effect.mapError(
                  store.rekey(source, target),
                  (fault) => new RemoteFault({ reason: "op", origin: to.origin.host, detail: fault.detail }),
                )))),
        Local: () =>
          Effect.flatMap(FileSystem.FileSystem, (fs) =>
            Effect.mapError(fs.copy(from.origin.path, to.origin.path), _fault(to.origin, "op"))),
      })

const _move = (from: Remote.End, to: Remote.End): Effect.Effect<void, RemoteFault, ObjectStore | FileSystem.FileSystem> =>
  _Session.$is("Bucket")(from.session)
    ? Effect.fail(new RemoteFault({ reason: "op", origin: from.origin.host, detail: "<bucket:move-rides-ledger>" }))
    : from.origin.scheme === to.origin.scheme && from.origin.host === to.origin.host && from.origin.flags.serverMove
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
          Effect.fail(new RemoteFault({ reason: "op", origin: to.origin.host, detail: "<bucket:move-rides-ledger>" })),
        Local: () =>
          Effect.flatMap(FileSystem.FileSystem, (fs) =>
            Effect.mapError(fs.rename(from.origin.path, to.origin.path), _fault(to.origin, "op"))),
      })
    : Effect.zipRight(_copy(from, to), _remove(from.origin, from.session))

const _intake = (origin: Origin, session: Remote.Session, retention: Retain.Class) =>
  Effect.gen(function* () {
    const store = yield* ObjectStore
    const flow = _read(origin, session).pipe(
      Stream.mapError((fault) => new ObjectFault({ reason: "io", key: origin.path, detail: fault.detail })),
    )
    const identity = yield* Rail.identity(Rail.chunked(flow, Rail.cut))
    const landed = yield* store.putKeyed(
      identity.key,
      yield* Stream.toReadableStreamEffect(
        _read(origin, session).pipe(
          Stream.mapError((fault) => new ObjectFault({ reason: "io", key: origin.path, detail: fault.detail })),
        )),
      identity.bytes,
    )
    yield* store.refer(identity.key, `remote:${origin.scheme}://${origin.host}${origin.path}`, retention) // the derived retention tag lands with the reference row
    return { key: identity.key, bytes: identity.bytes, written: landed.written, origin }
  })

const _lock = (origin: Origin, session: Remote.Session): Effect.Effect<{ readonly token: string }, RemoteFault> =>
  _Session.$match(session, {
    Dav: ({ client }) =>
      Effect.map(
        Effect.tryPromise({ try: () => client.lock(origin.path), catch: _fault(origin, "op") }),
        (held) => ({ token: held.token }),
      ),
    Ssh: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<lock:unsupported>" })),
    Ftp: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<lock:unsupported>" })),
    Bucket: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<lock:conditional-put-owns-races>" })),
    Local: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<lock:unsupported>" })),
  })

const _unlock = (origin: Origin, session: Remote.Session, token: string): Effect.Effect<void, RemoteFault> =>
  _Session.$match(session, {
    Dav: ({ client }) => Effect.tryPromise({ try: () => client.unlock(origin.path, token), catch: _fault(origin, "op") }),
    Ssh: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<unlock:unsupported>" })),
    Ftp: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<unlock:unsupported>" })),
    Bucket: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<unlock:unsupported>" })),
    Local: () => Effect.fail(new RemoteFault({ reason: "op", origin: origin.host, detail: "<unlock:unsupported>" })),
  })
```

## [05]-[TRANSFER_ENGINES]

- Owner: the `_ENGINES` policy rows — `rsyncDelta` (the primary resumable/delta lane over the external binary), `chunkedParallel` (`fastGet`/`fastPut` with the mined tuning defaults), `ftpOffset` (`startAt`/`appendFrom` arithmetic), `davRange` (the ranged-PATCH resume), `sftpOffset` (byte-offset resume from the target's `stat` size into a positioned write) — the `_ENDS` admission arms and the `_RESUMES` execution record over them, and `Remote.transfer(from, to, policy?)`, the one end-to-end move whose engine selection is row-derived data and whose `step` hook feeds the fact stream's meter row.
- Packages: `@effect/platform` (`Command.make`, `Command.exitCode` — the external `rsync`/`scp`/`ssh` engine; `stdin: Sink`/`stdout: Stream` process shape); `ssh2` (SFTP `fastGet`/`fastPut` — `concurrency`/`chunkSize`/`step`; `stat`, `open`, `read`, `write`, `close`); `basic-ftp` (`downloadTo(destination, path, startAt)`, `appendFrom`, `uploadFrom` slice options).
- Entry: `Remote.transfer(from, to)` walks `_ENGINES` in DECLARATION ORDER and takes the first row both origins admit; `policy.engine` pins a row that still answers the same admission, and `policy.step(progress)` observes transferred bytes per chunk (the ftp arm bridges it through a bracketed `trackProgress`).
- Growth: an engine tuning posture is a `_TUNE` override on the call; a new engine (a provider's accelerated transfer) is one row carrying its capability column, its end arm, its served target schemes, and its `resumes` value — selection, admission, and execution all inherit it.
- Law: every engine column is DECISION DATA the dispatch reads — `needs` names the capability flag, `ends` names which origin must carry it, `schemes` names the target schemes the row serves, and `resumes` selects the execution arm; a pinned engine answers the same three admission columns as a derived one, so `policy.engine` narrows the choice and can never spawn a binary against an address its row does not serve. A column no arm reads is the flag that lets a pin reach a lane the origins cannot run.
- Law: the `resumes` dispatch is TOTAL over its vocabulary — `_RESUMES` is a record keyed by the resume kind, so a declared engine is reachable by construction rather than by a predicate ladder whose tail silently absorbed every unmatched row; `range` and `offset` share the same probe-then-position arithmetic and differ at `_write`'s own DAV arm, which lands the bounded tail as one ranged PATCH.
- Law: no origin pair admitting no engine transfers — the selection answers `Option` and its absence refuses with the `transfer` reason naming the pinned or derived row, never a silent fall-through to a fallback engine the ends cannot run.
- Law: rsync flags are the sealed resume contract — `--partial --append-verify --inplace --checksum` gives delta transfer, interrupt resume, and integrity in one engine; the command is a `Command` value whose `exitCode` folds into the typed rail and whose cancellation rides the `Scope`.
- Law: resume is arithmetic where rsync is absent — `resume: true` probes the target, propagates a missing or unreadable-target fault unchanged, opens the positioned write (`flags: "r+"`, `appendFrom` on ftp), and streams the source from the verified byte; the default restart writes from byte zero without manufacturing absence through `Effect.option`.
- Law: the resume tap fires where the engine is IN HAND — `remoteResumed` is the only evidence the resume flags ever resumed, and the row that ran is known at the selection, so the counter tags the settled engine rather than a re-derivation at the entry record that would answer a second, unrelated walk.
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
  both: (flag: keyof Remote.Flags, from: Remote.End, to: Remote.End) => from.origin.flags[flag] && to.origin.flags[flag],
  target: (flag: keyof Remote.Flags, _from: Remote.End, to: Remote.End) => to.origin.flags[flag],
  localSource: (flag: keyof Remote.Flags, from: Remote.End, to: Remote.End) =>
    from.origin.scheme === "file" && to.origin.flags[flag],
} as const

// DECLARATION ORDER is the preference and every column is read: `needs` names the capability flag, `ends` the origin
// that must carry it, `schemes` the target schemes the row serves, `resumes` the execution arm. A pinned engine
// answers the same three admission columns, so `policy.engine` narrows the walk and never bypasses capability.
const _ENGINES = {
  rsyncDelta: { needs: "exec", ends: "both", schemes: _SCHEME_KEYS, resumes: "delta" },
  chunkedParallel: { needs: "parallel", ends: "localSource", schemes: ["sftp", "ssh"], resumes: "chunk" },
  ftpOffset: { needs: "offsetResume", ends: "target", schemes: ["ftp", "ftps"], resumes: "offset" },
  davRange: { needs: "offsetResume", ends: "target", schemes: ["webdav"], resumes: "range" },
  sftpOffset: { needs: "offsetResume", ends: "target", schemes: ["file", "sftp", "ssh", "s3"], resumes: "offset" },
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
    from.scheme === "file" ? from.path : `${from.username}@${from.host}:${from.path}`,
    to.scheme === "file" ? to.path : `${to.username}@${to.host}:${to.path}`,
  ).pipe(
    Command.exitCode,
    Effect.filterOrFail(
      (code) => code === 0,
      (code) => new RemoteFault({ reason: "transfer", origin: to.host, detail: `rsync:${code}` }),
    ),
  )

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
              : resume(Effect.fail(new RemoteFault({ reason: "transfer", origin: to.origin.host, detail: String(cause) }))))
        })),
    Ftp: () => _piped({ origin: Origin.make({ scheme: "file", host: "", port: 0, username: "", path: local }), session: _Session.Local() }, to),
    Dav: () => _piped({ origin: Origin.make({ scheme: "file", host: "", port: 0, username: "", path: local }), session: _Session.Local() }, to),
    Bucket: () => Effect.fail(new RemoteFault({ reason: "transfer", origin: to.origin.host, detail: "<bucket:transfer-rides-intake>" })),
    Local: () => Effect.fail(new RemoteFault({ reason: "transfer", origin: to.origin.host, detail: "<local:transfer-is-copy>" })),
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
// arithmetic and diverges at `_write`'s own DAV arm, which lands the bounded tail as one ranged PATCH.
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
        reason: "transfer",
        origin: to.origin.host,
        detail: `<engine:${policy?.engine ?? "derived"}:${to.origin.scheme}>`,
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

- Owner: the bisync fold — persisted per-side listings in the `sync_listing` relation, the `_COMPARE` comparator rows (`sizeModtime` default, `checksum`, `sizeOnly`), the per-side delta census, the reconcile fold producing typed `SyncAction` rows, apply-through-`transfer`, and resync recovery after interrupt.
- Packages: `@effect/sql` (`SqlSchema`, `sql.insert`); `lane/capability.md` (`Capability.Ensure` — the listing relation rides the same DDL split); `journal/append.md` (`Journal.Version` — the number-or-string codec the BIGINT `bytes` column decodes through on the spine wire); composition over `[4]`/`[5]` values.
- Entry: `Remote.sync(pair, left, right, comparator?)` — census both ends, delta each side against its persisted listing, reconcile (a change on one side transfers to the other, a removal propagates, ANY concurrent change on both sides — modify against modify, and a removal racing a modification alike — surfaces as a `Conflict` row the caller routes), apply, then persist the settled listings in one transaction.
- Growth: a comparator is one row; a conflict policy (`leftWins`, `newerWins`, `surface`) is a caller fold over the returned `Conflict` rows; a third replica is pairwise composition, never a widened engine.
- Law: a `Conflict` performs no transfer and no removal, and its path persists with its PRIOR listing row on both sides — the unresolved delta re-surfaces on every subsequent run until the caller rules it, so an unrouted conflict can never silently become a propagated winner.
- Law: listings are the resume substrate — an interrupted sync re-runs against persisted `{ path, kind, bytes, modified, etag }` rows and the already-applied transfers land as no-ops; `modified` and `etag` remain `Option` from provider read through SQL re-admission, and checksum comparison falls back to size-plus-modified only when either side lacks an ETag, never to null, empty-string, or zero sentinels.
- Law: the comparator is a policy row, never a fork — `sizeModtime` reads the census, `checksum` compares content evidence (`etag` where the backend mints one, the content-addressed intake fold's key where it does not), `sizeOnly` serves append-only trees; the row travels on the pair.

```typescript signature
import { Equal, HashSet } from "effect"
import { SqlClient, SqlSchema } from "@effect/sql"
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

const _COMPARE = {
  sizeModtime: (left: Remote.Stat, right: Remote.Stat) => left.bytes !== right.bytes || !Equal.equals(left.modified, right.modified),
  sizeOnly: (left: Remote.Stat, right: Remote.Stat) => left.bytes !== right.bytes,
  checksum: (left: Remote.Stat, right: Remote.Stat) =>
    Option.match(Option.zipWith(left.etag, right.etag, (leftEtag, rightEtag) => leftEtag !== rightEtag), {
      onNone: () => left.bytes !== right.bytes || !Equal.equals(left.modified, right.modified),
      onSome: (changed) => changed,
    }),
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

const _ListingRow = Schema.Struct({
  path: Schema.String,
  bytes: Journal.Version,
  kind: Schema.Literal("file", "directory"),
  modified: Schema.NullOr(Schema.String),
  etag: Schema.NullOr(Schema.String),
})

const _held = (sql: SqlClient.SqlClient) =>
  SqlSchema.findAll({
    Request: Schema.Struct({ pair: Schema.String, side: Schema.Literal("left", "right") }),
    Result: _ListingRow,
    execute: (side) => sql`SELECT path, bytes, kind, modified, etag FROM sync_listing WHERE pair = ${side.pair} AND side = ${side.side}`,
  })

const _persist = (sql: SqlClient.SqlClient, pair: string, side: "left" | "right", census: ReadonlyArray<Remote.Stat>) =>
  Effect.zipRight(
    sql`DELETE FROM sync_listing WHERE pair = ${pair} AND side = ${side}`,
    census.length === 0
      ? Effect.void
      : sql`INSERT INTO sync_listing ${sql.insert(census.map((row) => ({
          pair,
          side,
          path: row.path,
          bytes: row.bytes,
          kind: row.kind,
          modified: Option.getOrElse(row.modified, () => null),
          etag: Option.getOrElse(row.etag, () => null),
        })))}`,
  )

const _snapshot = (rows: ReadonlyArray<typeof _ListingRow.Type>): HashMap.HashMap<string, Remote.Stat> =>
  HashMap.fromIterable(rows.map((row) => [
    row.path,
    {
      path: row.path,
      bytes: row.bytes,
      kind: row.kind,
      modified: Option.fromNullable(row.modified),
      etag: Option.fromNullable(row.etag),
    } satisfies Remote.Stat,
  ] as const))

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
      _persist(sql, pair, "left", settledLeft),
      _persist(sql, pair, "right", settledRight),
    ))
    return { actions, conflicts: actions.filter(_SyncAction.$is("Conflict")) }
  })
```

## [07]-[WATCH_ROWS]

- Owner: the `_WATCHERS` strategy rows — `nativeWatch` (the platform watcher over a row whose protocol pushes changes natively), `execPush` (`inotifywait -m -r` over an ssh exec channel, the lowest-latency remote arm), `poll` (`Schedule`-driven census diff, the universal floor) — the `_FEEDS` record over them, `_strategyOf`, the ONE derivation both the dispatch and the census tap read, and `Remote.watch(origin, session, strategy?)`; intake-grade LOCAL watching stays `object/file.md`'s `Disk.watch` with its settle guard, so the `file:` row here is the non-intake observation posture a raw event answers.
- Packages: `ssh2` (`exec` — the push channel); `@effect/platform` (`FileSystem.watch` — `Stream<WatchEvent, PlatformError>` over the `Create`/`Update`/`Remove` tags); `@effect/platform-node` (`NodeStream.fromReadable`); `effect` (`Stream.splitLines`, `Schedule`, `HashMap`).
- Entry: a mirrored drop tree on a VPS rides `execPush` where the host carries a notify tool; a mounted local tree rides `nativeWatch`; a DAV or FTP origin rides `poll` diffing `etag`/`size`/`modified` snapshots; each emission is a `Remote.Change` the consumer routes into `Remote.intake` or the sync fold.
- Growth: a new strategy is one `_WATCHERS` row naming the capability column it reads plus one `_FEEDS` arm; the poll cadence and the push-tool roster are policy values.
- Law: strategy is capability-derived from the flag row itself — `changeNotify` selects the native watcher, `exec` the notify-tool push, and the terminal row needs no column so the walk is total over every scheme; `_strategyOf` is the ONE derivation, so the dispatch and the census tag can never answer different strategies for one origin, and a capability column the selection never reads is the decorative flag this walk forecloses.
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

const _strategyOf = (origin: Origin): Remote.WatchStrategy =>
  Option.getOrElse(
    Array.findFirst(_WATCH_ORDER, (strategy) =>
      Option.match(_WATCHERS[strategy].needs, { onNone: () => true, onSome: (flag) => origin.flags[flag] })),
    () => "poll" as const, // unreachable: the terminal row's absent column admits every origin
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
  execPush: (origin, session) => _execPush(origin, session).pipe(Stream.retry(Schedule.spaced(_POLL.cadence))),
  poll: (origin, session) => _poll(_list(origin, session)),
}

const _watch = (origin: Origin, session: Remote.Session, strategy?: Remote.WatchStrategy): _Feed =>
  _FEEDS[strategy ?? _strategyOf(origin)](origin, session)
```

## [08]-[EXEC]

- Owner: `Remote.exec` — command execution as ONE typed surface over the session family: `Remote.Invocation { file, args }` is the sole ingress, the ssh2 boundary shell-quotes each atom once, and the local arm passes the same atoms directly to `Command.make`; `stdout`/`stderr` remain backpressured `Stream`s, `stdin` remains a `Sink`, and `exit` remains a typed effect.
- Packages: `ssh2` (`exec` — channel `Duplex`, `exit` event); `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable`); `@effect/platform` (`Command.make`, `Command.start` — the local arm's process shape).
- Entry: VPS interaction is exec plus SFTP against the provisioned address — a deployment probe, a remote build step, the `execPush` watch tool all ride this one surface; provisioning, DNS, firewall, and snapshot lifecycle stay on the deploy plane and never enter this lane.
- Growth: a PTY session is the `shell` sibling over the same channel lift; a jump-host exec is the same call over a `sock`-chained session.
- Law: command structure survives every arm — callers cannot inject a `sh -c` program or interpolate a path into command text; only the SSH boundary renders argv into a POSIX command using single-quote isolation, while the local boundary executes `file` plus `args` without a shell. A non-zero exit is data on the result, never an exception; a channel-level failure is the `exec` fault reason.
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
        client.exec(_shell(invocation), (cause, channel) => {
          if (cause !== undefined && cause !== null) {
            resume(Effect.fail(new RemoteFault({ reason: "exec", origin: origin.host, detail: String(cause) })))
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
    Ftp: () => Effect.fail(new RemoteFault({ reason: "exec", origin: origin.host, detail: "<exec:unsupported>" })),
    Dav: () => Effect.fail(new RemoteFault({ reason: "exec", origin: origin.host, detail: "<exec:unsupported>" })),
    Bucket: () => Effect.fail(new RemoteFault({ reason: "exec", origin: origin.host, detail: "<exec:unsupported>" })),
  })

// The verb roster the measurement fold covers, closed against the entry record below: a key outside this tuple is
// data the record publishes rather than a measured verb, and a verb the record does not publish fails the guard.
const _OPS = [
  "probe", "stat", "list", "read", "write", "mkdir", "remove", "copy", "move", "lock", "unlock", "intake",
  "transfer", "sync", "watch", "exec",
] as const

// Every verb crosses ONE combinator at the record, so measurement is a property of the surface rather than of each
// implementation: an origin-addressed verb names its own origin, a pair-addressed one measures at the destination it
// writes, and the two stream verbs measure their SETUP (the dial and the registration) because a stream's own span is
// its consumer's lifetime, never the call that opened it. A new verb is a row that arrives already measured.
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
          strategy ?? _strategyOf(origin),
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

- Owner: the remote plane's Convention projections — the mounted instrument rows, `_measured`, the ONE combinator the `Remote` entry record folds every verb through, and the bounded taps the legs that own their own census carry (`_read`/`_write` octets, `_sessions` lease level, `_transfer` resume, `_sync`/`_watch`/`_exec` rows).
- Packages: `effect` (`Metric`, `Effect`, `Duration`); `@rasm/ts/core` (`Convention` — the instrument, axis, outcome, and duration projections).
- Entry: the `Remote` record folds each verb through `_measured(op, origin, self)` at ONE site, so the surface is instrumented by the record rather than by a tap per member and a new verb inherits measurement by construction; the census taps ride inside the legs that already hold the number.
- Growth: a new verb is one entry-record row the fold already covers; a new axis is one `Convention` row with its tap on the owning leg.
- Law: the operation counter is the core outcome aspect, never a hand-rolled fold — `Convention.outcome(Convention.metric.remoteOps, Convention.attr.errorType, …)` owns the single emission point, the interrupt-first discrimination, and the `halted`/`crashed`/reason vocabulary, so this page supplies only its own reason projection (`RemoteFault.class`, the core kind the family already derives) and spells no `Effect.onExit`; a page-local exit fold would double-count every retried attempt and never see a defect.
- Law: the region axes ride the FIBER, not the handle — `Effect.tagMetrics` stamps `remoteOp` and `remoteScheme` across every metric the governed effect updates, so the outcome aspect keeps its own mounted handle while the two dimensions the row declares still land; pre-tagging the aspect's handle is unspellable because the aspect mounts internally, and re-mounting the row beside it would fork one series into two registry entries.
- Law: `remoteDuration` is a SUMMARY, so its update takes the scaled NUMBER — the row names a sliding quantile window because a local `stat` and a multi-gigabyte rsync ride one instrument and no frozen ladder answers both, and Effect constrains a summary's carrier to a bare number, so the site passes `Convention.duration(...)` where a bucketed row would take the `Duration` itself; handing this row a `Duration` is the one mount-shape error the kind's carrier makes unspellable.
- Law: octets count where the count already EXISTS — `remoteBytes` taps the transfer legs that hold a byte number, never a stream the page would have to measure by adding a fold; bytes crossing into `putKeyed` are already `objectSize`'s, so `Remote.intake` taps the remote hop alone and the object plane keeps its own census with no double count.
- Law: the session gauge is a LEVEL both ways — `remoteSessionsHeld` is the `updown` row, incremented on acquire and decremented on release inside the pool's own bracket, so a pooled reuse that never dials shows as a level that never rose and the lease census stays true under interruption.
- Law: identifier-grade context rides the SPAN — `Effect.withSpan("data.remote", { attributes: { op, scheme, host } })` carries the host, while the metric axes stay the bounded scheme, verb, action, engine, and strategy vocabularies each closed by its own roster; a host interpolated into a tag mints one series per origin.

```typescript signature
import { Duration, Effect, Metric } from "effect"
import { Convention } from "@rasm/ts/core"

const _bytes = Convention.mount(Convention.metric.remoteBytes)
const _exits = Convention.mount(Convention.metric.remoteExecExits)
const _held = Convention.mount(Convention.metric.remoteSessionsHeld)
const _resumed = Convention.mount(Convention.metric.remoteResumed)
const _spanned = Convention.mount(Convention.metric.remoteDuration)
const _syncActions = Convention.mount(Convention.metric.remoteSyncActions)
const _watched = Convention.mount(Convention.metric.remoteWatchChanges)

// The core aspect owns the emission point and the outcome vocabulary; this page supplies only the reason projection,
// and the reason IS the fault's own core kind, so the refusal fan and the landed half partition one series.
const _counted = Convention.outcome(
  Convention.metric.remoteOps,
  Convention.attr.errorType,
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

// A level, not a total: the pool bracket owns both edges, so an interrupted acquire cannot leave the gauge raised.
const _leased = <A, E, R>(scheme: Remote.Scheme, self: Effect.Effect<A, E, R>): Effect.Effect<A, E, R> =>
  Effect.acquireUseRelease(
    Metric.incrementBy(Metric.tagged(_held, Convention.rasm.remoteScheme, scheme), 1),
    () => self,
    () => Metric.incrementBy(Metric.tagged(_held, Convention.rasm.remoteScheme, scheme), -1),
  )
```

## [10]-[RESEARCH]

<!-- source-only: research row template:
[TOKEN]-[OPEN|BLOCKED]: <exact question>; <verification route>.
[SPLIT_MEMBER]-[OPEN]: does `shape-core` expose `split_all`; verify against the member rail.
-->

(none)
