# [DATA_REMOTE]

The remote-origin filesystem plane: ONE origin-addressed surface owning every non-local byte tree — SFTP and SSH-exec over the in-process `ssh2` root, FTP/FTPS over `basic-ftp`, WebDAV over `webdav`, the object plane reached as the `s3:` row — with capability flags as data so every polymorphic operation degrades by row, never by fork. An `Origin` value carries scheme, coordinate, and path; its scheme selects the backend row; the row's flags decide whether a copy is server-side or read-then-write, whether a watch pushes or polls, whether a transfer resumes by rsync delta, byte offset, or parallel chunks. Two transfer engines share one policy surface: the in-process ssh2 lane (boundary-adapted callbacks, `NodeStream`/`NodeSink` channel lifts) and the external `rsync`/`scp`/`ssh` binaries as `@effect/platform` `Command` processes. Connections pool through `lane/cache.md`'s bounded origin row; every remote read feeds the SAME content-addressed intake fold as local disk, so a cloud project grows zero new addressing vocabulary; the sync engine persists listings and reconciles deltas with resume and recover, and remote watch is a strategy row beside the local chokidar arm.

## [1]-[CLUSTERS]

| [INDEX] | [CLUSTER]          | [OWNS]                                                                              |
| :-----: | :----------------- | :------------------------------------------------------------------------------------ |
|  [01]   | `ORIGIN_ROWS`      | the `Origin` class, the scheme capability-flag table, the reason-discriminated fault    |
|  [02]   | `SESSION_ROWS`     | the per-scheme scoped session brackets and the pooled origin reuse                      |
|  [03]   | `OP_SURFACE`       | the polymorphic verb set — stat/list/read/write/copy/move/remove/mkdir, flag degrade    |
|  [04]   | `TRANSFER_ENGINES` | the resume policy rows — rsync delta, offset, chunked-parallel — and the intake fold    |
|  [05]   | `SYNC_ENGINE`      | persisted listings, comparator rows, the diff-apply-recover fold                        |
|  [06]   | `WATCH_ROWS`       | the watch strategy rows — local settle-guard, ssh exec-push, universal poll             |
|  [07]   | `EXEC`             | remote command execution — typed channels, exit disposition, the local `Command` twin   |

## [2]-[ORIGIN_ROWS]

- Owner: `Origin` — one `Schema.Class` whose `scheme` field selects the backend row, with `Origin.parse` decoding a URI string through one fused codec; the `_SCHEMES` capability-flag table (the rclone backend model: server-side copy/move, streaming put, change notify, exec, offset resume, parallel transfer, locks); `RemoteFault` — the one reason-discriminated fault of the plane.
- Packages: `effect` (`Schema`, `Data`, `Either`, `ParseResult`).
- Entry: every consumer addresses the plane by `Origin` value — `Origin.parse("sftp://deploy@vps.example:22/srv/artifacts")` at a config seam, never scheme-forked code; `origin.flags` is the dispatch datum every operation reads.
- Growth: a new protocol is one scheme key, one flag row, one session arm, and the op arms it earns — every consumer inherits it through the flags; a flag a server refuses at runtime narrows by row override, never by fork.
- Law: capability flags are decision data — `serverCopy: false` makes `copy` degrade to read-then-write, `serverMove: false` makes `move` degrade to copy-then-remove, `changeNotify: false` routes `watch` to the poll row, `exec: false` refuses `exec` typed; the degrade paths live in the op arms once, so no caller ever probes a protocol.
- Law: the `s3:` row is a bridge, not a re-implementation — its ops delegate to `object/store.md` and `object/stream.md` owners (`get`/`putKeyed`/`rekey`/`Rail.range`), so the object plane's conditional-put and grant law hold unchanged behind the origin address.
- Law: `RemoteFault` reasons route recovery as a fold — `connect` and `auth` invalidate the pooled session, `op` and `transfer` retry under the engine's policy row, `watch` re-arms the strategy, `exec` carries the exit disposition; a free-string-only fault is the named unroutable defect.

```typescript
import { Data, Either, ParseResult, Schema } from "effect"

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

declare namespace Remote {
  type Scheme = keyof typeof _SCHEMES
  type Flags = (typeof _SCHEMES)[Scheme]
  type Reason = "connect" | "auth" | "op" | "transfer" | "watch" | "exec"
  type _Rows<T extends Record<Scheme, Record<keyof Flags, boolean>> = typeof _SCHEMES> = T
}

class RemoteFault extends Data.TaggedError("RemoteFault")<{
  readonly reason: Remote.Reason
  readonly origin: string
  readonly detail: string
}> {}

class Origin extends Schema.Class<Origin>("Origin")({
  scheme: Schema.Literal(...Object.keys(_SCHEMES) as ReadonlyArray<Remote.Scheme>),
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
          const scheme = parsed.protocol.slice(0, -1)
          return {
            scheme,
            host: parsed.hostname,
            port: parsed.port === "" ? _PORTS[scheme as Remote.Scheme] : Number.parseInt(parsed.port, 10),
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
}
```

## [3]-[SESSION_ROWS]

- Owner: the per-scheme session brackets — the ssh2 connect-on-`ready`/`end()`-on-release bracket with the SFTP subsystem lift, the `basic-ftp` `access` dial, the `webdav` client mint — and `Remote.sessions`, the bounded pooled reuse over `lane/cache.md`'s `CacheLane.origins` keyed by the structural `OriginKey`.
- Packages: `ssh2` (`Client` — `connect`, `sftp`, `end`; events `ready`/`error`; config auth/trust/keepalive rows; `sock` jump-host injection); `basic-ftp` (`Client`, `access`, `close`); `webdav` (`createClient`, `AuthType`); `lane/cache.md` (`CacheLane.origins`, `OriginKey`); `effect` (`Effect`, `Redacted`, `Scope`).
- Entry: an operation leases its session through the keyed pool — `KeyedPool.get` under the caller's `Scope` — so the FTP one-transfer-per-control-connection law and SSH connection reuse are both pool facts; `Stores`-style consumers never construct a session directly.
- Growth: an auth posture (agent, keyboard-interactive, custom `authHandler`) is an `Auth` field flowing into the connect config; a bastion chain is the prior hop's `forwardOut` duplex entering the next `connect` through `sock` — config data, never topology code.
- Law: sessions are scoped brackets — ssh resolves on `ready`, fails typed on `error`, releases through `end()`; ftp dials through `access` alone (the split connect/login members are probes) and releases through `close()`; a bare client with ad-hoc listeners in domain code is the rejected spelling.
- Law: credentials are `Redacted` config rows — password, private key, passphrase never appear as literals; host trust rides `hostVerifier` where the deployment pins keys; TLS on the ftp row is the `secure` config value (`true` explicit, `"implicit"` wrapped), never a scheme fork beyond the `ftps:` port default.
- Boundary: the ssh2 and basic-ftp surfaces are callback/Promise boundary kernels — every listener registration and promise lives inside these brackets, and above them only `Stream`/`Sink`/typed effects exist.

```typescript
import { Effect, Redacted, type Scope } from "effect"
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
    readonly readyTimeout?: number
    readonly keepaliveInterval?: number
  }
}

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
          secure: origin.scheme === "ftps" ? "implicit" : true,
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

const _sessions = (auth: (key: OriginKey) => Remote.Auth) =>
  CacheLane.origins((key: OriginKey) =>
    _ssh(new Origin({ scheme: "sftp", host: key.host, port: key.port, username: key.username, path: "/" }), auth(key)))
```

## [4]-[OP_SURFACE]

- Owner: the polymorphic verb set — `stat`, `list`, `read` (→ backpressured `Stream`), `write` (← `Sink`), `copy`, `move`, `remove`, `mkdir` — each ONE entry dispatching on `origin.scheme` with flag-driven degrade arms; and `Remote.intake`, the content-addressed landing that runs any remote read through the SAME identity fold as local disk.
- Packages: `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable` — the only stream seams); `ssh2` (SFTP `stat`, `readdir`, `createReadStream`, `createWriteStream`, `rename`, `unlink`, `mkdir`, `rmdir`); `webdav` (`stat`, `getDirectoryContents`, `createReadStream`, `createWriteStream`, `copyFile`, `moveFile`, `deleteFile`, `createDirectory`); `basic-ftp` (`list`, `size`, `lastMod`, `downloadTo`, `uploadFrom`, `rename`, `remove`, `removeDir`, `ensureDir`); `object/stream.md` (`Rail.bytes`, `Rail.chunked`, `Rail.identity`, `Rail.range`), `object/store.md` (`ObjectStore`).
- Entry: `Remote.read(origin)` yields `Stream<Uint8Array, RemoteFault>` on every scheme; `Remote.intake(origin, retention)` is the one cloud-ingestion entry — read, cut, digest, conditional put, reference row, retention tag — identical receipts to `Disk.intake`.
- Growth: a new verb is one dispatch surface with per-row arms; a per-server capability discovery (`getDAVCompliance`, `features`) is a probe that narrows the flag row at session acquire, never a caller branch.
- Law: reads and writes are backpressured lifts — SFTP and DAV node streams cross through `NodeStream.fromReadable`/`NodeSink.fromWritable`, the FTP arm bridges its `Writable`-consuming transfer through one relay duplex inside the boundary; no raw `.on("data")` consumption exists past the adapter.
- Law: degrade is structural — `copy` on a row without `serverCopy` composes `read` into `write`; `move` without `serverMove` composes `copy` then `remove`; the `s3:` arms delegate to the object plane owners; a caller cannot observe which arm ran except through the receipt's engine field.
- Law: every remote byte that becomes durable rides `Remote.intake` — the origin row grows no second addressing vocabulary, dedup and 412-idempotency arrive from the object plane for free, and a remote origin is therefore a first-class artifact source.

```typescript
import { Option, Stream } from "effect"
import { FileSystem } from "@effect/platform"
import { NodeSink, NodeStream } from "@effect/platform-node"
import { PassThrough } from "node:stream"
import { ContentKey } from "@rasm/ts/core"
import { ObjectFault, ObjectStore } from "./store.ts"
import { Rail } from "./stream.ts"
import type { Retain } from "../journal/retain.ts"

declare namespace Remote {
  type Stat = { readonly path: string; readonly bytes: number; readonly modified: string; readonly kind: "file" | "directory"; readonly etag?: string }
  type Session = { readonly ssh?: SshClient; readonly ftp?: FtpClient; readonly dav?: WebDAVClient }
}

const _fault = (origin: Origin, reason: Remote.Reason) => (cause: unknown): RemoteFault =>
  new RemoteFault({ reason, origin: origin.host, detail: String(cause) })

const _read = (origin: Origin, session: Remote.Session, offset?: number): Stream.Stream<Uint8Array, RemoteFault> => {
  switch (origin.scheme) {
    case "sftp":
    case "ssh":
      return Stream.unwrap(
        Effect.map(_sftp(session.ssh as SshClient, origin), (sftp) =>
          NodeStream.fromReadable(
            () => sftp.createReadStream(origin.path, offset === undefined ? {} : { start: offset }),
            _fault(origin, "op"),
          )))
    case "webdav":
      return NodeStream.fromReadable(
        () => (session.dav as WebDAVClient).createReadStream(origin.path, offset === undefined ? {} : { range: { start: offset } }),
        _fault(origin, "op"),
      )
    case "ftp":
    case "ftps":
      return Stream.unwrap(
        Effect.sync(() => {
          const relay = new PassThrough()
          void (session.ftp as FtpClient).downloadTo(relay, origin.path, offset ?? 0).catch((cause) => relay.destroy(cause as Error))
          return NodeStream.fromReadable(() => relay, _fault(origin, "transfer"))
        }))
    case "s3":
      return Rail.range(origin.path as ContentKey, offset === undefined ? undefined : { from: offset }).pipe(
        Stream.mapError((fault) => new RemoteFault({ reason: "op", origin: origin.host, detail: fault.detail })),
      )
    case "file":
      return Stream.unwrap(
        Effect.map(FileSystem.FileSystem, (fs) =>
          fs.stream(origin.path).pipe(Stream.mapError(_fault(origin, "op")))))
  }
}

const _write = (origin: Origin, session: Remote.Session) => {
  switch (origin.scheme) {
    case "sftp":
    case "ssh":
      return Effect.map(_sftp(session.ssh as SshClient, origin), (sftp) =>
        NodeSink.fromWritable(() => sftp.createWriteStream(origin.path), _fault(origin, "op")))
    case "webdav":
      return Effect.succeed(
        NodeSink.fromWritable(() => (session.dav as WebDAVClient).createWriteStream(origin.path), _fault(origin, "op")))
    default:
      return Effect.sync(() => {
        const relay = new PassThrough()
        void (session.ftp as FtpClient).uploadFrom(relay, origin.path)
        return NodeSink.fromWritable(() => relay, _fault(origin, "transfer"))
      })
  }
}

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
    yield* store.refer(identity.key, `remote:${origin.scheme}://${origin.host}${origin.path}`, retention)
    yield* store.classify(identity.key, retention)
    return { key: identity.key, bytes: identity.bytes, written: landed.written, origin }
  })
```

## [5]-[TRANSFER_ENGINES]

- Owner: the `_ENGINES` policy rows — `rsyncDelta` (the preferred resumable/delta lane over the external binary), `sftpOffset` (byte-offset resume from `stat` size), `chunkedParallel` (`fastGet`/`fastPut` with the mined tuning defaults), `ftpOffset` (`startAt`/`appendFrom` arithmetic), `davRange` (ranged read resume) — and `Remote.transfer(from, to, policy?)`, the one origin-to-origin move whose engine selection is flag-derived data.
- Packages: `@effect/platform` (`Command.make`, `Command.exitCode`, `Command.string` — the external `rsync`/`scp`/`ssh` engine; `stdin: Sink`/`stdout: Stream` process shape); `ssh2` (SFTP `fastGet`/`fastPut` — `concurrency`/`chunkSize`/`step`; `stat`, `open`, `read`, `write`, `close`); `basic-ftp` (`downloadTo(destination, path, startAt)`, `appendFrom`, `uploadFrom` slice options).
- Entry: `Remote.transfer(from, to)` derives the engine — rsync where both ends speak ssh and carry the binary, chunked-parallel for a single large SFTP file, offset arithmetic elsewhere — and an explicit `policy` pins a row; the `step(total, transferred, size)` progress hook feeds the fact stream's meter row.
- Growth: an engine tuning posture is a `_TUNE` override on the call; a new engine (a provider's accelerated transfer) is one row with its selection predicate.
- Law: rsync flags are the sealed resume contract — `--partial --append-verify --inplace --checksum` gives delta transfer, interrupt resume, and integrity in one engine; the command is a `Command` value whose `exitCode` folds into the typed rail and whose cancellation rides the `Scope`.
- Law: resume is arithmetic where rsync is absent — `stat` the remote size, `open(path, "a" | "r+")`, positioned `read`/`write` from the verified byte; a full re-transfer where an offset resume was possible is the named defect.
- Law: the mined tuning defaults are policy values — `concurrency: 64`, `chunkSize: 32768` arrived from the wrapper ecosystem's measured defaults and live in `_TUNE`, never inline literals.

```typescript
import { Command } from "@effect/platform"

const _TUNE = { concurrency: 64, chunkSize: 32_768 } as const

const _RSYNC = ["--partial", "--append-verify", "--inplace", "--checksum"] as const

declare namespace Remote {
  type Engine = keyof typeof _ENGINES
  type Progress = { readonly total: number; readonly transferred: number }
}

const _ENGINES = {
  rsyncDelta: { needs: "exec", resumes: "delta" },
  sftpOffset: { needs: "offsetResume", resumes: "offset" },
  chunkedParallel: { needs: "parallel", resumes: "chunk" },
  ftpOffset: { needs: "offsetResume", resumes: "offset" },
  davRange: { needs: "offsetResume", resumes: "range" },
} as const

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

const _fastPut = (origin: Origin, session: Remote.Session, local: string, step?: (progress: Remote.Progress) => void) =>
  Effect.flatMap(_sftp(session.ssh as SshClient, origin), (sftp) =>
    Effect.async<void, RemoteFault>((resume) => {
      sftp.fastPut(local, origin.path, {
        concurrency: _TUNE.concurrency,
        chunkSize: _TUNE.chunkSize,
        step: (total, _chunk, size) => step?.({ total: size, transferred: total }),
      }, (cause) =>
        cause === undefined || cause === null
          ? resume(Effect.void)
          : resume(Effect.fail(new RemoteFault({ reason: "transfer", origin: origin.host, detail: String(cause) }))))
    }))

const _transfer = (from: Origin, to: Origin, session: Remote.Session, engine?: Remote.Engine) => {
  const selected = engine
    ?? (from.flags.exec && to.flags.exec ? "rsyncDelta" : from.flags.parallel ? "chunkedParallel" : "sftpOffset")
  return selected === "rsyncDelta"
    ? _rsync(from, to)
    : Effect.flatMap(_write(to, session), (sink) => Stream.run(_read(from, session), sink)).pipe(
        Effect.mapError((fault) =>
          fault._tag === "RemoteFault" ? fault : new RemoteFault({ reason: "transfer", origin: to.host, detail: String(fault) })),
        Effect.asVoid,
      )
}
```

## [6]-[SYNC_ENGINE]

- Owner: the bisync fold — persisted per-side listings in the `sync_listing` relation, the `_COMPARE` comparator rows (`sizeModtime` default, `checksum`, `sizeOnly`), the diff fold producing typed actions, apply-through-`transfer`, and resync recovery after interrupt.
- Packages: `@effect/sql` (`SqlSchema`, `sql.insert`); `lane/capability.md` (`Capability.Ensure` — the listing relation rides the same DDL split); composition over `[4]`/`[5]` values.
- Entry: `Remote.sync(pair, left, right, comparator?)` — census both origins, diff each side against its persisted listing, reconcile (a change on one side transfers to the other, changes on both sides route to the newer under `sizeModtime`, equal-change conflicts surface as typed rows), persist the fresh listings in the same transaction as the apply receipts.
- Growth: a comparator is one row; a conflict policy (prefer-left, prefer-newer, surface) is a policy value on the call; a third replica is pairwise composition, never a widened engine.
- Law: listings are the resume substrate — an interrupted sync re-runs against the persisted listings and the already-applied transfers land as no-ops (the intake fold's 412 law where the target is the object plane, size+mtime equality elsewhere); `resync` rebuilds both listings from live census when the pair's history is untrusted.
- Law: the comparator is a policy row, never a fork — `sizeModtime` reads the census, `checksum` runs the content-addressed intake fold on both sides and compares keys (exact, engine-independent), `sizeOnly` serves append-only trees; the row travels on the pair.

```typescript
import { SqlClient, SqlSchema } from "@effect/sql"
import type { Capability } from "../lane/capability.ts"
import { Journal } from "../journal/append.ts"

const _listingDdl: Capability.Ensure = {
  relation: "sync_listing",
  pg: `CREATE TABLE IF NOT EXISTS sync_listing (
    pair TEXT NOT NULL, side TEXT NOT NULL, path TEXT NOT NULL,
    bytes BIGINT NOT NULL, modified TEXT NOT NULL, etag TEXT,
    listed_at TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (pair, side, path));`,
  sqlite: `CREATE TABLE IF NOT EXISTS sync_listing (
    pair TEXT NOT NULL, side TEXT NOT NULL, path TEXT NOT NULL,
    bytes INTEGER NOT NULL, modified TEXT NOT NULL, etag TEXT,
    listed_at TEXT NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ','now')),
    PRIMARY KEY (pair, side, path));`,
}

const _COMPARE = {
  sizeModtime: (left: Remote.Stat, right: Remote.Stat) => left.bytes !== right.bytes || left.modified !== right.modified,
  sizeOnly: (left: Remote.Stat, right: Remote.Stat) => left.bytes !== right.bytes,
  checksum: (left: Remote.Stat, right: Remote.Stat) => left.etag === undefined || left.etag !== right.etag,
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

const _ListingRow = Schema.Struct({ path: Schema.String, bytes: Schema.Number, modified: Schema.String, etag: Schema.NullOr(Schema.String) })

const _held = (sql: SqlClient.SqlClient) =>
  SqlSchema.findAll({
    Request: Schema.Struct({ pair: Schema.String, side: Schema.Literal("left", "right") }),
    Result: _ListingRow,
    execute: (side) => sql`SELECT path, bytes, modified, etag FROM sync_listing WHERE pair = ${side.pair} AND side = ${side.side}`,
  })

const _persist = (sql: SqlClient.SqlClient, pair: string, side: "left" | "right", census: ReadonlyArray<Remote.Stat>) =>
  Effect.zipRight(
    sql`DELETE FROM sync_listing WHERE pair = ${pair} AND side = ${side}`,
    census.length === 0
      ? Effect.void
      : sql`INSERT INTO sync_listing ${sql.insert(census.map((row) => ({
          pair, side, path: row.path, bytes: row.bytes, modified: row.modified, etag: row.etag ?? null,
        })))}`,
  )
```

## [7]-[WATCH_ROWS]

- Owner: the watch strategy rows — `local` (the chokidar settle-guarded arm `object/file.md` owns), `execPush` (`inotifywait -m -r` or `fswatch` over an ssh exec channel, the lowest-latency remote arm), `poll` (`Schedule`-driven census diff, the universal default) — and `Remote.watch(origin, strategy?)` dispatching on `origin.flags.changeNotify`/`exec`.
- Packages: `ssh2` (`exec` — the push channel); `@effect/platform-node` (`NodeStream.fromReadable`); `effect` (`Stream.splitLines`, `Schedule`, `HashMap`); the local arm delegates to `object/file.md`'s `Disk.watch`.
- Entry: a mirrored drop tree on a VPS rides `execPush` where the host carries a notify tool; a DAV or FTP origin rides `poll` diffing `etag`/`size`/`modified` snapshots; each emission is a `Remote.Change` the consumer routes into `Remote.intake` or the sync fold.
- Growth: a new strategy is one row with its selection predicate; the poll cadence and the push-tool roster are policy values.
- Law: strategy is capability-derived — `changeNotify` rows push natively, `exec` rows push through the notify tool, everything else polls; the consumer subscribes ONE change stream regardless, so strategy is invisible past the dispatch.
- Law: the poll arm is diff-exact — each cycle's census diffs against the held snapshot by the same comparator rows the sync engine reads, emitting `add`/`change`/`remove` with no phantom events on unchanged trees; a lost push connection re-arms through `Stream.retry` and one full poll cycle reconciles anything missed.

```typescript
import { HashMap, Schedule } from "effect"

declare namespace Remote {
  type Change = { readonly path: string; readonly kind: "add" | "change" | "remove" }
  type WatchStrategy = "local" | "execPush" | "poll"
}

const _POLL = { cadence: "30 seconds" } as const

const _execPush = (origin: Origin, session: Remote.Session): Stream.Stream<Remote.Change, RemoteFault> =>
  Stream.unwrap(
    _exec(origin, session, `inotifywait -m -r --format '%e|%w%f' ${origin.path}`).pipe(
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

const _poll = (origin: Origin, session: Remote.Session, census: Effect.Effect<ReadonlyArray<Remote.Stat>, RemoteFault>): Stream.Stream<Remote.Change, RemoteFault> =>
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
```

## [8]-[EXEC]

- Owner: `Remote.exec` — remote command execution over the ssh2 exec channel as ONE typed surface: `stdout`/`stderr` as backpressured `Stream`s, `stdin` as a `Sink`, `exit` as a typed effect resolved from the channel's `exit` event — and the local twin riding `@effect/platform` `Command` for host tooling beside the transfer engines.
- Packages: `ssh2` (`exec` — channel `Duplex`, `exit` event); `@effect/platform-node` (`NodeStream.fromReadable`, `NodeSink.fromWritable`); `@effect/platform` (`Command` — the local arm).
- Entry: VPS interaction is exec plus SFTP against the provisioned address — a deployment probe, a remote build step, the `execPush` watch tool all ride this one surface; provisioning, DNS, firewall, and snapshot lifecycle stay on the deploy plane and never enter this lane.
- Growth: a PTY session is the `shell` sibling over the same channel lift; a jump-host exec is the same call over a `sock`-chained session.
- Law: the exit disposition is typed — a non-zero exit is data on the result (`{ code }`), never an exception; a channel-level failure is the `exec` fault reason; the consumer folds both.
- Boundary: the exec callback and the `exit` listener are the channel's boundary kernel — the last statement flow on the page.

```typescript
declare namespace Remote {
  type Executed = {
    readonly stdout: Stream.Stream<Uint8Array, RemoteFault>
    readonly stderr: Stream.Stream<Uint8Array, RemoteFault>
    readonly exit: Effect.Effect<number, RemoteFault>
  }
}

const _exec = (origin: Origin, session: Remote.Session, command: string): Effect.Effect<Remote.Executed, RemoteFault> =>
  Effect.async<Remote.Executed, RemoteFault>((resume) => {
    ;(session.ssh as SshClient).exec(command, (cause, channel) => {
      if (cause !== undefined && cause !== null) {
        resume(Effect.fail(new RemoteFault({ reason: "exec", origin: origin.host, detail: String(cause) })))
        return
      }
      resume(Effect.succeed({
        stdout: NodeStream.fromReadable(() => channel, _fault(origin, "exec")),
        stderr: NodeStream.fromReadable(() => channel.stderr, _fault(origin, "exec")),
        exit: Effect.async<number, RemoteFault>((settle) => {
          channel.once("exit", (code) => settle(Effect.succeed(typeof code === "number" ? code : -1)))
        }),
      }))
    })
  })

const Remote = {
  schemes: _SCHEMES,
  engines: _ENGINES,
  compare: _COMPARE,
  SyncAction: _SyncAction,
  ddl: [_listingDdl],
  sessions: _sessions,
  read: _read,
  write: _write,
  intake: _intake,
  transfer: _transfer,
  exec: _exec,
  execPush: _execPush,
  poll: _poll,
} as const

// --- [EXPORTS] --------------------------------------------------------------------------

export { Origin, Remote, RemoteFault }
```
