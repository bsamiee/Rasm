# PREFETCH — Remote/Cloud Filesystem Capability Lane (TS, Effect-native)

Charge: one parameterized surface owning local + remote file control, SSH exec, SFTP/SCP transfer,
resumable cross-origin sync, remote watch, and VPS-host interaction. Substrate is `effect` 3.21.4 +
`@effect/platform` 0.96.2 (+ `@effect/platform-node` / `-node-shared`). All version/license/maintenance
facts below are from the npm registry and package source; all cited API members are spelling-verified
against upstream README/source (see Appendix). Weekly download = adoption signal.

## Roster + Verdicts

| npm name | ver | license | last publish | weekly dl | maintenance | verdict |
|---|---|---|---|---|---|---|
| `ssh2` | 1.17.0 | MIT | 2026-05-13 | 8.75M | single maintainer (mscdex), actively patched, protocol-complete | **admit-substrate** |
| `ssh2-sftp-client` | 12.1.1 | Apache-2.0 | 2026-03-25 | 1.96M | active, Node>=20, tracks ssh2 | **mine-design-only** |
| `node-ssh` | 13.2.1 | MIT | 2025-03-20 | 397K | maintained, "extremely lightweight promise wrapper" (self-described) | **mine-design-only** |
| `ssh2-promise` | 1.0.3 | MIT | 2022-06-19 | 23K | stale (3.5yr), thin | **reject** |
| `node-scp` | 0.0.25 | MIT | 2025-04-27 | 28K | pre-1.0, thin scp-only wrapper | **reject** |
| `ssh2-streams` | 0.4.10 | MIT | 2022-06-26 | — | merged into ssh2 core; do not add | **reject** |
| `cpu-features` | 0.0.10 | BSD-3 | 2024-05-03 | — | ssh2 optionalDependency (native, cipher-list tuning); auto | **admit-substrate (transitive/optional)** |
| `basic-ftp` | 6.0.1 | MIT | 2026-05-03 | 25.6M | active, modern FTP/FTPS, zero-dep, TS-native | **admit-folder (if FTP in scope)** |
| `webdav` | 5.10.0 | MIT | 2026-05-03 | 154K | active, TS-native, isomorphic | **admit-folder (genuinely new surface)** |
| `@google-cloud/storage` | 7.21.0 | Apache-2.0 | 2026-06-08 | — | first-party, heavy | **admit-folder (only if GCS in scope)** |
| `@azure/storage-blob` | 12.33.0 | MIT | 2026-06-24 | — | first-party, heavy | **admit-folder (only if Azure in scope)** |
| `chokidar` | 5.0.0 | MIT | 2026-05-21 | high | active — LOCAL watch only, no remote | admit for local watch (already-settled lane) |
| rclone | — | MIT (Go binary) | — | — | design prior-art, not an npm dep | **mine-design-only** |

Note: `ssh2` `package.json` carries no SPDX `license` string but ships an MIT `LICENSE`; verified MIT.
`ssh2` engines `>=16`, runtime deps `asn1`+`bcrypt-pbkdf`, optionalDeps `cpu-features`+`nan`. Pure-JS
crypto fallback exists; native `cpu-features`/`nan` only tune the default cipher list — no hard native gate.

## (a) SSH / exec landscape

`ssh2` is the root. It is the only Node library implementing SSHv2 client+server in-process (event/
callback API over `Client`): `exec`, `shell`, `sftp`, `subsys`, `forwardOut`/`forwardIn`,
`openssh_forwardOutStreamLocal` (unix-socket fwd), `rekey`. Jump-host/bastion chaining is native via the
`sock` config field — feed a first connection's `forwardOut` duplex as the `sock` of a second `Client`,
no external proxy. Auth surface is full: `password`, `privateKey`+`passphrase`, `agent`+`agentForward`,
`tryKeyboard`+`keyboard-interactive`, custom `authHandler`, `hostVerifier`/`hostHash` for known-hosts.

`node-ssh` and `ssh2-promise` are Promise skins over `ssh2`. `node-ssh` adds real value only in
`putDirectory`/`getDirectory` (recursive walk + per-op concurrency + validate/filter callbacks) and
`putFiles` batching — an Effect-native layer re-wraps `ssh2` directly and re-implements that walk as a
`Stream`-driven traversal, so node-ssh is **mine-design-only** (steal the dir-concurrency + filter-hook
shape). `ssh2-promise` is stale/thin → **reject**. `node-scp` wraps only the SCP subsystem (a strict
subset of what `ssh2.exec`/SFTP already give) → **reject**. No credible non-ssh2 alternative exists; every
maintained option sits on `ssh2`.

Integration shape (in-process, the hard seam): `ssh2` is callback+EventEmitter, so the owner adapts at the
boundary — connection lifecycle as `Effect.acquireRelease` (connect on `ready` event, `end()` on release)
inside a `Scope`; a bounded connection reuse via an Effect pool keyed by `{host,port,user}`; `exec` and
`shell` channels are Node `Duplex` → lift with `NodeStream.fromDuplex` / `NodeStream.fromReadable` (stdout)
+ `NodeSink.fromWritable` (stdin); discrete events (`banner`, `hostkeys`, `keyboard-interactive`) via
`Effect.async` / `Stream.asyncPush`. Command exit code arrives on the channel `'exit'` event → resolve into
the result rail. This is the boundary-kernel exception where language-owned callback control is permitted.

## (b) SFTP / transfer + resumable

Two transfer engines, both parameterizable behind one op:

1. In-process SFTP (ssh2 `conn.sftp()` → SFTPWrapper). Full POSIX surface: `fastGet`/`fastPut`
   (parallel chunked, opts `concurrency`/`chunkSize`/`step`/`mode`/`fileSize` — `step(total,nb,fsize)` is
   the progress hook), `createReadStream`/`createWriteStream` (backpressured Node streams), byte-addressed
   `open`/`read`/`write`/`fstat`/`close` (the primitive for offset transfers), `readdir`, `stat`/`lstat`/
   `setstat`, `readFile`/`writeFile`, `rename` + `ext_openssh_*` posix-rename, `mkdir`/`rmdir`, `unlink`,
   `realpath`. `ssh2-sftp-client` is the ergonomic Promise layer over exactly this (`list`/`exists`/`get`/
   `put`/`fastGet`/`fastPut`/`append`/`uploadDir`/`downloadDir`/`posixRename`/`rcopy`, `concurrency` def 64,
   `chunkSize` def 32768, `promiseLimit` def 10) — **mine-design-only**: its `uploadDir`/`downloadDir` walk,
   `append`-for-resume, and default tuning are the design template, but re-wrap raw ssh2 SFTP streams so the
   transfer is a native Effect `Stream` with real backpressure and cancellation, not a re-wrapped Promise.

2. External `rsync`/`scp` binary via `@effect/platform` `Command` (see (c)). This is the robust
   resumable/delta engine and the recommended path for large or interrupted transfers.

Resumable strategy (parameterize as a policy row, no native SFTP resume exists):
- rsync-over-ssh (preferred): `rsync -e ssh --partial --partial-dir --append-verify --inplace --checksum`
  gives delta-transfer + interrupt-resume + integrity in one, driven as a `Command` with `exitCode`/stderr
  rail. This is what a "resumable sync between origins" op should default to when both ends have rsync.
- SFTP offset-resume (fallback, no rsync): `stat` remote size → `open(path,'a'|'r+')` → `read`/`write` at
  position from the byte primitives (or `ssh2-sftp-client.append`). Wrap as a resume-capable `Stream` sink.
- Chunked parallel: `fastGet`/`fastPut` with `concurrency`+`chunkSize`+`step` for throughput on single
  large files where the server permits parallel reads.

## (c) @effect/platform integration shape (verified)

`Command` already models a subprocess as `stdin: Sink`, `stdout: Stream`, `stderr: Stream`, `exitCode:
Effect` (verified: `CommandExecutor.Process` interface + `Command.Input = "inherit"|"pipe"|Stream<Uint8Array>`,
`Command.Output = "inherit"|"pipe"|Sink<Uint8Array,Uint8Array>`). Consequence: wrapping the OS `ssh`,
`rsync`, `scp` binaries is fully native and backpressure-correct — the external-binary transfer/sync/exec
lane is `Command.make("rsync", …)` piped through the `Process` streams, cancellation-safe under `Scope`.

The in-process `ssh2` lane does NOT use `Command` (it is a JS library, not a subprocess). Its seam is the
Node↔Effect stream bridge in `@effect/platform-node-shared` (re-exported by `@effect/platform-node`):
`NodeStream.fromReadable`, `NodeStream.toReadable`, `NodeStream.fromDuplex`, `NodeStream.pipeThroughDuplex`,
`NodeSink.fromWritable` (all spelling-verified from source). SFTP `createReadStream`→`NodeStream.fromReadable`,
`createWriteStream`→`NodeSink.fromWritable`, exec channel `Duplex`→`NodeStream.fromDuplex`. `Socket` from
`@effect/platform` is NOT the integration point for ssh2 (ssh2 owns its own transport); `Socket`/`Socket.makeNet`
is only relevant if a raw pre-connected duplex is injected via the ssh2 `sock` field for custom tunneling.

Per-candidate integration verdict: ssh2 → boundary-adapt (acquireRelease + NodeStream/NodeSink + async events);
external rsync/scp/ssh → `Command` (already native); ssh2-sftp-client/node-ssh → do not integrate, their
Promise layer fights Effect — mine their traversal/resume logic only.

## (d) Remote-fs prior art (design mining — rclone)

rclone is the reference architecture for the ONE parameterized surface. Mine, do not vendor:
- Backend interface (`fs.Fs`): a single abstract capability implemented per protocol (local, sftp, s3,
  webdav, ftp, gcs, azureblob…). Each backend advertises capability flags — `Move`/`Copy` (server-side),
  `PutStream` (unknown-size streaming upload), `MultithreadUpload`, `About` (free space), `ChangeNotify`,
  hash types. This is the exact polymorphic-row model: one op surface, backend rows carry capability bits,
  dispatch/degrade on the flags (e.g. no server-side `Move` ⇒ `Copy`+`delete`).
- VFS layer: adapts object stores into a filesystem (dir cache, chunked reading `--vfs-read-chunk-size`,
  write-back cache). Mirror as an optional caching projection over any backend for mount-like local views.
- Sync engine: comparison policy is `size`+`modtime` default, `--checksum` / `--size-only` overrides —
  parameterize the sync comparator as a policy row. `bisync` (bidirectional) keeps prior Path1/Path2 listings
  and reconciles deltas with `--resync`/`--recover`/`--resilient` — the model for "resumable sync between
  origins": persist listings, diff, apply, recover on interrupt.
- `chunker` overlay: transparently split/reassemble large files past provider size caps — a backend
  decorator, the template for a size-cap-transparent transfer wrapper.
- `ChangeNotify` = the remote-watch abstraction: backends that support it push change events; others fall
  back to poll-interval. Directly informs (f) below.

## (e) Cloud provider file surfaces beyond S3

S3-compatible + presigned is settled; note only genuinely new capability:
- `webdav` (5.10.0, MIT) — **admit-folder, the standout new surface**. Opens Nextcloud/ownCloud, SharePoint/
  OneDrive (via WebDAV), Apache/nginx DAV — none reachable through S3 or SSH. TS-native, isomorphic,
  clean method surface (`getDirectoryContents`, `getFileContents`/`createReadStream`, `putFileContents`/
  `createWriteStream`, `stat`, `copyFile`, `moveFile`, `deleteFile`, `createDirectory`, `getQuota`,
  `lock`/`unlock`). Streams + range requests → bridges to Effect `Stream` the same way.
- `basic-ftp` (6.0.1, MIT, 25.6M dl) — **admit-folder if FTP/FTPS in scope**. Modern, zero-dep, TS-native
  FTP+implicit/explicit TLS; a legacy-protocol origin many AEC/enterprise hosts still expose. Streaming
  up/download → Effect bridge identical.
- `@google-cloud/storage` (7.21.0, Apache-2.0) / `@azure/storage-blob` (12.33.0, MIT) — object-store PARITY
  with S3 (same origin shape, resumable-upload + SAS/signed-URL analogues), NOT new capability class. Admit
  only when GCS/Azure is an actual target cloud; otherwise the S3 backend row already models the shape.
- VPS lifecycle: no npm SDK belongs here. Provisioning/DNS/firewall/snapshots are owned by the `hostinger`
  MCP; the library's "VPS interaction" lane is purely SSH-exec + SFTP against the provisioned host (backend
  row = sftp/ssh over the VPS address), plus optional local-cmd `Command` for host tooling.

## (f) Remote watch

No SFTP native watch. Parameterize a watch-strategy row (rclone ChangeNotify model):
- ssh-exec push: run `inotifywait -m -r` (Linux) or `fswatch` (macOS) over an `ssh2.exec` channel; stdout
  `Duplex`→`NodeStream.fromDuplex`→a change `Stream`. Lowest latency, requires the tool on the host.
- SFTP poll: `Schedule`-driven `readdir`+`stat` mtime/size diff over a snapshot. Universal, no host deps,
  latency = poll interval. This is the safe default.
- Local watch stays on `chokidar` 5.0.0 (settled). The op dispatches strategy by origin kind + capability.

## Recommended surface (synthesis)

One `RemoteFs`-style capability, origin-addressed (URI scheme selects backend row: `file:` / `sftp:` /
`ssh:` / `ftp:`/`ftps:` / `webdav:` / `s3:` / `gcs:` / `azblob:`). Ops are polymorphic and dispatch on
origin + capability flags: `stat`, `list`, `read`(→Stream), `write`(←Sink), `copy`, `move`, `remove`,
`mkdir`, `transfer`(origin→origin), `sync`(comparator + resume policy), `watch`(strategy row), `exec`
(ssh/local only). Backends: `ssh2` (in-process sftp+exec, admit-substrate) and `@effect/platform` `Command`
(external rsync/scp/ssh, admit-substrate) are the two transfer engines; `webdav`/`basic-ftp`/cloud SDKs are
admit-folder backend rows added by scope. Capability flags (server-side copy/move, PutStream, multithread,
ChangeNotify, hash) drive degrade paths, exactly per rclone `fs.Fs`.

## Appendix — verified API spellings

ssh2 `Client` (verbatim, README): `connect` `exec` `shell` `sftp` `subsys` `forwardOut` `forwardIn`
`unforwardIn` `openssh_forwardOutStreamLocal` `openssh_forwardInStreamLocal`
`openssh_unforwardInStreamLocal` `openssh_noMoreSessions` `rekey` `setNoDelay` `end`.
Events: `ready` `error` `end` `close` `banner` `change password` `handshake` `hostkeys`
`keyboard-interactive` `rekey` `tcp connection` `unix connection` `x11`.
Config: `host` `port` `username` `password` `privateKey` `passphrase` `agent` `agentForward`
`localAddress` `localPort` `localHostname` `localUsername` `hostVerifier` `hostHash` `algorithms`
`authHandler` `readyTimeout` `keepaliveInterval` `keepaliveCountMax` `sock` `tryKeyboard`
`forceIPv4` `forceIPv6` `strictVendor` `debug`.
ssh2 SFTP: `fastGet` `fastPut` `createReadStream` `createWriteStream` `readFile` `writeFile`
`open` `read` `write` `close` `fstat` `stat` `lstat` `setstat` `readdir` `opendir` `unlink`
`rename` `mkdir` `rmdir` `realpath`. fastGet/fastPut opts: `concurrency` `chunkSize` `step`
`mode` `fileSize`.

ssh2-sftp-client `Client` (verbatim, README): `connect` `list` `exists` `stat` `get` `fastGet`
`put` `fastPut` `append` `mkdir` `rmdir` `uploadDir` `downloadDir` `delete` `rename` `posixRename`
`chmod` `realPath` `cwd` `createReadStream` `createWriteStream` `rcopy` `end`. Defaults:
`concurrency`=64, `chunkSize`=32768, `promiseLimit`=10. Node `>=20`.

node-ssh `NodeSSH` (verbatim, README): `connect` `execCommand` `exec` `getFile` `putFile` `putFiles`
`putDirectory` `getDirectory` `mkdir` `requestShell` `requestSFTP` `withSFTP` `dispose` `isConnected`.

@effect/platform Command / CommandExecutor (source-verified): `Command.Input` = `"inherit" | "pipe" |
Stream<Uint8Array, PlatformError>`; `Command.Output` = `"inherit" | "pipe" | Sink<Uint8Array, Uint8Array>`;
`Process` interface fields: `pid` `exitCode` `isRunning` `kill(signal)` `stderr: Stream` `stdin: Sink`
`stdout: Stream`; `CommandExecutor.start`. (Standard documented Command constructors — `Command.make`,
`Command.string`, `Command.lines`, `Command.stream`, `Command.streamLines`, `Command.exitCode`,
`Command.stdin`, `Command.feed`, `Command.pipeTo`, `Command.env`, `Command.workingDirectory`,
`Command.runInShell` — confirm exact arity at implementation via Context7 `/effect-ts/effect`.)

@effect/platform-node stream bridge (source-verified, `@effect/platform-node-shared` re-exported by
`@effect/platform-node`): `NodeStream.fromReadable` `NodeStream.toReadable` `NodeStream.fromDuplex`
`NodeStream.pipeThroughDuplex` `NodeStream.fromReadableChannel` `NodeStream.pipeThroughSimple`
`NodeStream.toString` `NodeStream.toUint8Array` `NodeStream.stdin` `NodeStream.stdout` `NodeStream.stderr`;
`NodeSink.fromWritable` `NodeSink.fromWritableChannel` `NodeSink.stdin` `NodeSink.stdout` `NodeSink.stderr`.
