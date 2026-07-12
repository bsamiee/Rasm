# [TS_DATA_API_SSH2]

`ssh2` is the only Node library implementing the SSHv2 client protocol in-process — every maintained SSH wrapper (`node-ssh`, `ssh2-sftp-client`, `ssh2-promise`, `node-scp`) is a Promise skin over this package, and the branch admits the root while mining the skins for design only. It owns the connection (`connect` with full auth, host-trust, and keepalive config), the `exec`/`shell` channels (Node `Duplex` values whose exit disposition arrives on the channel `exit` event), the SFTP subsystem (parallel-chunked `fastGet`/`fastPut`, backpressured streams, and the byte-offset `open`/`read`/`write` primitives that make resume offset arithmetic), port forwarding, and native jump-host chaining through the `sock` config field. The surface is callback + EventEmitter — the permitted boundary-kernel exception — so the owner adapts at the seam: `Effect.acquireRelease` brackets the connection (resolve on `ready`, `end()` on release), `NodeStream.fromDuplex`/`fromReadable` + `NodeSink.fromWritable` lift the channels, and `Effect.async`/`Stream.asyncPush` carry the discrete events. Two transfer engines share one policy surface: this package is the in-process lane, and the external `rsync`/`scp`/`ssh` binaries ride `@effect/platform` `Command` — `Command` owns the rsync-over-ssh resumable/delta lane. `Socket` is NOT the seam: ssh2 owns its own transport, and the `sock` field is the single point where a pre-connected duplex enters.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ssh2`
- package: `ssh2` (MIT — the manifest carries no SPDX string; the shipped `LICENSE` is MIT)
- module format: CJS; single root export
- runtime target: node only; runtime deps `asn1` + `bcrypt-pbkdf`; optional `cpu-features` + `nan` tune the default cipher list only — a pure-JS crypto recovery exists, so there is no hard native gate
- rail: data remote-transfer — the SSH root under every remote exec, SFTP transfer, and remote-watch origin row; catalogued once at the data tier
- boundary: callback + EventEmitter surface adapted entirely at the seam; no member is consumed in domain code

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, events, and connection config
- rail: boundaries

| [INDEX] | [SYMBOL]                                                                                                                                         | [TYPE_FAMILY]   | [CONSUMER]                                                                             |
| :-----: | :----------------------------------------------------------------------------------------------------------------------------------------------- | :-------------- | :------------------------------------------------------------------------------------- |
|  [01]   | `Client` (`connect`, `exec`, `shell`, `sftp`, `subsys`, `rekey`, `setNoDelay`, `end`)                                                            | client          | the scoped connection the boundary adapter brackets                                    |
|  [02]   | `Client` forwarding family                                                                                                                       | forwarding      | TCP/unix tunnels; `forwardOut` mints the jump-host duplex                              |
|  [03]   | events `ready` / `error` / `end` / `close`                                                                                                       | lifecycle       | the acquireRelease bracket arms                                                        |
|  [04]   | events `banner` / `handshake` / `hostkeys` / `keyboard-interactive` / `change password` / `rekey` / `tcp connection` / `unix connection` / `x11` | discrete events | lifted via `Effect.async` / `Stream.asyncPush`                                         |
|  [05]   | config auth (`host`, `port`, `username`, `password`, `privateKey` + `passphrase`, `agent` + `agentForward`, `tryKeyboard`, `authHandler`)        | connect config  | the full auth surface — key, agent, keyboard-interactive, custom handler               |
|  [06]   | config trust + tuning                                                                                                                            | connect config  | known-hosts verification, cipher policy, liveness budget                               |
|  [07]   | config `sock`                                                                                                                                    | duplex inject   | jump-host chaining — a prior connection's `forwardOut` duplex feeds the next `connect` |
|  [08]   | exec/shell channel (Node `Duplex`; `exit` event)                                                                                                 | channel         | stdout/stderr/stdin streams; exit code resolves into the result rail                   |

[PUBLIC_TYPE_SCOPE]: the SFTP subsystem (`SFTPWrapper`)
- rail: boundaries

| [INDEX] | [SYMBOL]                                                                            | [TYPE_FAMILY]     | [CONSUMER]                                                                                                        |
| :-----: | :---------------------------------------------------------------------------------- | :---------------- | :---------------------------------------------------------------------------------------------------------------- |
|  [01]   | `fastGet` / `fastPut` (opts `concurrency`, `chunkSize`, `step`, `mode`, `fileSize`) | parallel transfer | chunked-parallel single-file throughput; `step(total, nb, fsize)` is the progress hook                            |
|  [02]   | `createReadStream` / `createWriteStream`                                            | streams           | backpressured Node streams lifted through the platform bridges                                                    |
|  [03]   | `open` / `read` / `write` / `close` / `fstat`                                       | byte primitives   | offset-addressed I/O — the resume primitive (`stat` size → `open(path, 'a' \| 'r+')` → positioned `read`/`write`) |
|  [04]   | `readFile` / `writeFile`                                                            | whole-file        | small bounded payloads only                                                                                       |
|  [05]   | `readdir` / `opendir` / `stat` / `lstat` / `setstat` / `realpath`                   | census + attrs    | directory walk, attribute reads, the poll-watch diff source                                                       |
|  [06]   | `rename` + `ext_openssh_*` posix-rename / `mkdir` / `rmdir` / `unlink`              | namespace         | atomic-rename staging, tree maintenance                                                                           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection, channels, and the jump-host chain
- rail: boundaries

| [INDEX] | [SURFACE]                                                                           | [ENTRY_FAMILY] | [CONSUMER]                                                                                                                          |
| :-----: | :---------------------------------------------------------------------------------- | :------------- | :---------------------------------------------------------------------------------------------------------------------------------- |
|  [01]   | `client.connect(config)` — resolve on `ready`, fail on `error`, release via `end()` | acquire        | `Effect.acquireRelease` in a `Scope`; bounded reuse via a pool keyed `{host, port, username}`                                       |
|  [02]   | `client.exec(command, opts?, cb)` → channel `Duplex`                                | remote exec    | stdout → `NodeStream.fromReadable`, stdin → `NodeSink.fromWritable`, whole channel → `NodeStream.fromDuplex`; `exit` → typed result |
|  [03]   | `client.shell(opts?, cb)` → channel `Duplex`                                        | interactive    | PTY session as one duplex `Channel`                                                                                                 |
|  [04]   | `client.sftp(cb)` → `SFTPWrapper`                                                   | subsystem      | the SFTP surface of [02]; one wrapper per connection                                                                                |
|  [05]   | `first.forwardOut(...)` → duplex → `second.connect({ sock: duplex, ... })`          | jump host      | bastion chaining with no external proxy — `sock` is the injection point                                                             |

[ENTRYPOINT_SCOPE]: transfer-resume and remote-watch policy rows
- rail: boundaries
- No native SFTP resume or watch exists; both are policy rows the origin surface dispatches on capability.

| [INDEX] | [SURFACE]                                                                                            | [ENTRY_FAMILY]  | [CONSUMER]                                                                  |
| :-----: | :--------------------------------------------------------------------------------------------------- | :-------------- | :-------------------------------------------------------------------------- |
|  [01]   | `Command.make("rsync", "-e", "ssh", "--partial", "--append-verify", "--inplace", "--checksum", ...)` | resume: delta   | the preferred resumable/delta row — external binary, `Command`-native       |
|  [02]   | `stat` remote size → `open(path, 'a' \| 'r+')` → positioned `read`/`write`                           | resume: offset  | the no-rsync recovery row — byte-offset arithmetic, a resume-capable `Sink` |
|  [03]   | `fastGet`/`fastPut` with `concurrency` + `chunkSize` + `step`                                        | resume: chunked | parallel throughput on single large files where the server permits          |
|  [04]   | `exec("inotifywait -m -r ..." \| "fswatch ...")` → `NodeStream.fromDuplex` → change `Stream`         | watch: push     | lowest latency; requires the tool on the host                               |
|  [05]   | `Schedule`-driven `readdir` + `stat` mtime/size diff                                                 | watch: poll     | universal safe default; latency = poll cadence                              |

## [04]-[IMPLEMENTATION_LAW]

[SSH_TOPOLOGY]:
- root law: every credible SSH capability sits on this package; the Promise skins are rejected as dependencies and mined as design — the recursive dir walk with per-op concurrency and validate/filter hooks re-expresses as a `Stream`-driven traversal, append-for-resume as an offset `Sink`, and the mined tuning defaults (`concurrency` 64, `chunkSize` 32768) become policy values, never a wrapper import.
- boundary shape: connect-on-`ready`/`end()`-on-release inside `Effect.acquireRelease`; channels and SFTP streams cross through `NodeStream`/`NodeSink` only; discrete events lift through `Effect.async`/`Stream.asyncPush`; the channel `exit` code folds into the typed result rail. Language-owned callback control exists only inside this seam.
- two engines, one policy surface: in-process SFTP (this package) versus external `rsync`/`scp`/`ssh` as `Command` processes (`stdin: Sink`, `stdout`/`stderr: Stream`, `exitCode: Effect`, cancellation-safe under `Scope`). Transfer selection is a policy row — rsync-over-ssh when both ends carry rsync, SFTP offset resume otherwise, chunked-parallel for raw throughput.
- `sock` is the transport door: ssh2 owns its own wire, so `@effect/platform` `Socket` never wraps it; a pre-connected duplex enters only through `sock`, which is exactly how jump-host chains and custom tunnels compose.
- VPS interaction is exec + SFTP against a provisioned address; provisioning, DNS, firewall, and snapshot lifecycle are deploy-plane concerns that never enter this lane.

[STACKS_WITH]:
- `@effect/platform-node` (`.api/effect-platform-node.md`): `NodeStream.fromReadable`/`toReadable`/`fromDuplex` and `NodeSink.fromWritable` are the only stream seams; no raw `.on("data")` consumption exists past the adapter.
- `@effect/platform` (`.api/effect-platform.md`): `Command` is the sibling engine — the rsync-over-ssh lane and any host-tool invocation ride `CommandExecutor`, never a hand-spawned subprocess.
- `effect` (`.api/effect.md`): `acquireRelease`/`Scope` lifecycle, pooled connection reuse, `Stream.asyncPush` event lift, `Schedule` poll cadence, typed fault rows for auth/trust/channel failures.
- `webdav` / `basic-ftp` (`.api/webdav.md`, `.api/basic-ftp.md`): sibling remote-origin rows on the `object/file` plane — one origin-addressed surface dispatches on scheme + capability flags, and a missing server-side capability degrades by row, never by fork.
- `chokidar` (`.api/chokidar.md`): the local half of the watch strategy row; the remote halves are the exec-push and poll rows above.

[LOCAL_ADMISSION]:
- Acquire connections only through the scoped bracket with pooled reuse; a bare `new Client()` with ad-hoc listeners in domain code is rejected.
- Adapt every callback and event at the boundary; downstream composes `Stream`/`Sink`/typed results exclusively.
- Select transfer engine by policy row, never inline: `Command` rsync for resumable/delta, byte primitives for offset resume, `fastGet`/`fastPut` for parallel throughput.
- Verify host trust through `hostVerifier`/`hostHash`; keys, passphrases, and agent sockets are config-sourced, never literals.

[RAIL_LAW]:
- Package: `ssh2`
- Owns: the in-process SSHv2 client — connection lifecycle and auth, exec/shell channels, the SFTP subsystem (parallel transfer, streams, byte-offset primitives, namespace ops), port forwarding, and `sock`-chained jump hosts
- Accept: `acquireRelease`-bracketed connections resolved on `ready`, `NodeStream`/`NodeSink` channel lifts, `Effect.async` event lifts, policy-row transfer/resume/watch selection, `sock` as the only duplex injection
- Reject: Promise-wrapper packages (`node-ssh`, `ssh2-sftp-client`, `ssh2-promise`, `node-scp`), `Socket` as the ssh2 seam, raw EventEmitter consumption in domain code, re-implementing rsync delta in-process, SCP-subsystem packages duplicating what exec + SFTP already own
