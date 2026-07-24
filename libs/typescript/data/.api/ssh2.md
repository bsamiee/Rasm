# [TS_DATA_API_SSH2]

`ssh2` owns the in-process SSHv2 client: connection lifecycle and auth, `exec`/`shell` channels as Node `Duplex` values, the SFTP subsystem — parallel `fastGet`/`fastPut`, backpressured streams, byte-offset resume primitives — port forwarding, and `sock`-chained jump hosts.

Its callback + EventEmitter surface adapts wholly at the seam, so no member reaches domain code; every maintained SSH wrapper is a Promise skin over this root, admitted as the data remote-transfer root while the skins are mined for design.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `ssh2`
- package: `ssh2` (MIT)
- module: CJS, single root export
- runtime: node only; `asn1` + `bcrypt-pbkdf` deps, optional `cpu-features`/`nan` accelerate ciphers behind a pure-JS fallback, so no hard native gate
- rail: data remote-transfer — the SSH root under every remote exec, SFTP transfer, and remote-watch origin

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: client, events, and connection config

| [INDEX] | [SYMBOL]                                         | [TYPE_FAMILY]   | [CAPABILITY]                                                      |
| :-----: | :----------------------------------------------- | :-------------- | :---------------------------------------------------------------- |
|  [01]   | `Client`                                         | client          | the scoped connection the bracket owns                            |
|  [02]   | `Client` forwarding family                       | forwarding      | TCP/unix tunnels; `forwardOut` mints the jump-host duplex         |
|  [03]   | events `ready` / `error` / `end` / `close`       | lifecycle       | the acquireRelease bracket arms                                   |
|  [04]   | discrete events                                  | discrete events | lifted via `Effect.async` / `Stream.asyncPush`                    |
|  [05]   | connect auth config                              | connect config  | key, agent, keyboard-interactive, custom handler                  |
|  [06]   | config trust + tuning                            | connect config  | known-hosts verification, cipher policy, liveness budget          |
|  [07]   | config `sock`                                    | duplex inject   | bastion chaining — a `forwardOut` duplex feeds the next `connect` |
|  [08]   | exec/shell channel (Node `Duplex`; `exit` event) | channel         | stdout/stderr/stdin streams; exit code → the result rail          |

[CLIENT_MEMBERS]: `connect` `exec` `shell` `sftp` `subsys` `rekey` `setNoDelay` `end`
[CONNECT_AUTH]: `host` `port` `username` `password` `privateKey`+`passphrase` `agent`+`agentForward` `tryKeyboard` `authHandler`
[TRUST_TUNING]: `hostVerifier` `hostHash` `readyTimeout` `keepaliveInterval`
[DISCRETE_EVENTS]: `banner` `handshake` `hostkeys` `keyboard-interactive` `change password` `rekey` `tcp connection` `unix connection` `x11`

[PUBLIC_TYPE_SCOPE]: the SFTP subsystem (`SFTPWrapper`)

| [INDEX] | [SYMBOL]                                                | [TYPE_FAMILY]     | [CAPABILITY]                                            |
| :-----: | :------------------------------------------------------ | :---------------- | :------------------------------------------------------ |
|  [01]   | `fastGet` / `fastPut`                                   | parallel transfer | chunked-parallel throughput; `step` progress hook       |
|  [02]   | `createReadStream` / `createWriteStream`                | streams           | backpressured Node streams via the platform bridges     |
|  [03]   | `open`/`read`/`write`/`close`/`fstat`                   | byte primitives   | offset-addressed I/O — the resume primitive             |
|  [04]   | `readFile` / `writeFile`                                | whole-file        | small bounded payloads only                             |
|  [05]   | `readdir`/`opendir`/`stat`/`lstat`/`setstat`/`realpath` | census + attrs    | directory walk, attribute reads, poll-watch diff source |
|  [06]   | `rename`/`ext_openssh_*`/`mkdir`/`rmdir`/`unlink`       | namespace         | atomic-rename staging (posix-rename), tree maintenance  |

[TRANSFER_OPTS]: `concurrency` `chunkSize` `step(total, nb, fsize)` `mode` `fileSize`

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection, channels, and the jump-host chain

`connect` resolves on `ready`, fails on `error`, releases through `end()`, and pools keyed `{host,port,username}`.

| [INDEX] | [SURFACE]                                            | [CAPABILITY]                               |
| :-----: | :--------------------------------------------------- | :----------------------------------------- |
|  [01]   | `client.connect(config)`                             | acquire — scoped bracket, pooled reuse     |
|  [02]   | `client.exec(command, opts?, cb)` → channel `Duplex` | remote exec; `exit` → typed result         |
|  [03]   | `client.shell(opts?, cb)` → channel `Duplex`         | interactive PTY as one duplex `Channel`    |
|  [04]   | `client.sftp(cb)` → `SFTPWrapper`                    | SFTP subsystem, one wrapper per connection |
|  [05]   | `forwardOut(...)` → `connect({ sock })`              | jump host — `sock` is the injection point  |

[ENTRYPOINT_SCOPE]: transfer-resume and remote-watch policy rows

No native SFTP resume or watch exists; both are capability-dispatched policy rows the origin surface selects.

| [INDEX] | [SURFACE]                                                        | [CAPABILITY]                                      |
| :-----: | :--------------------------------------------------------------- | :------------------------------------------------ |
|  [01]   | `Command.make("rsync", …)`                                       | resume delta — preferred, external                |
|  [02]   | remote `stat` → `open('a'\|'r+')` → positioned `read`/`write`    | resume offset — no-rsync byte-offset `Sink`       |
|  [03]   | `fastGet`/`fastPut` with `concurrency`+`chunkSize`+`step`        | resume chunked — parallel on single large files   |
|  [04]   | `exec("inotifywait …" \| "fswatch …")` → `NodeStream.fromDuplex` | watch push — lowest latency; needs a host tool    |
|  [05]   | `Schedule`-driven `readdir` + `stat` mtime/size diff             | watch poll — universal default; latency = cadence |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- Callback and event surface adapts only at the boundary seam: `Effect.acquireRelease` brackets the connection (`ready` resolves, `end()` releases), `NodeStream`/`NodeSink` lift channels and SFTP streams, `Effect.async`/`Stream.asyncPush` carry discrete events, and the channel `exit` code folds into the typed result rail — language-owned callback control lives only here.
- Two transfer engines share one policy surface: in-process SFTP against external `rsync`/`scp`/`ssh` as `@effect/platform` `Command` processes, and selection is a policy row — rsync-over-ssh when both ends carry rsync, SFTP offset resume otherwise, chunked-parallel for raw throughput.
- `sock` is the sole transport door: ssh2 owns its own wire, so `Socket` never wraps it and a pre-connected duplex enters only through `sock` — exactly how jump-host chains and custom tunnels compose.

[STACKING]:
- `@effect/platform-node` (`.api/effect-platform-node.md`): `NodeStream.fromReadable`/`fromDuplex` and `NodeSink.fromWritable` are the only channel and stream seams; no raw `.on("data")` survives past the adapter.
- `@effect/platform` (`.api/effect-platform.md`): `Command` is the sibling engine — rsync-over-ssh and every host-tool invocation ride `CommandExecutor`, never a hand-spawned subprocess.
- `effect` (`.api/effect.md`): `acquireRelease`/`Scope` bracket the pooled connection, `Stream.asyncPush` lifts events, `Schedule` drives poll cadence, and typed fault rows carry auth/trust/channel failures.
- `webdav` / `basic-ftp` (`.api/webdav.md`, `.api/basic-ftp.md`): sibling remote-origin rows on the `object/file` plane — one origin-addressed surface dispatches on scheme + capability flags, and a missing server-side capability degrades by row, never by fork.
- `chokidar` (`.api/chokidar.md`): the local half of the watch row; the remote halves are the exec-push and poll rows.

[LOCAL_ADMISSION]:
- Acquire only through the scoped bracket with pooled reuse; a bare `new Client()` with ad-hoc listeners is rejected.
- Select the transfer engine by policy row: `Command` rsync for resumable/delta, byte primitives for offset resume, `fastGet`/`fastPut` for parallel throughput.
- Verify host trust through `hostVerifier`/`hostHash`; keys, passphrases, and agent sockets are config-sourced, never literals.

[RAIL_LAW]:
- Package: `ssh2`
- Owns: the in-process SSHv2 client — connection lifecycle and auth, `exec`/`shell` channels, the SFTP subsystem (parallel transfer, streams, byte-offset primitives, namespace ops), port forwarding, `sock`-chained jump hosts
- Accept: `acquireRelease`-bracketed connections resolved on `ready`, `NodeStream`/`NodeSink` channel lifts, `Effect.async` event lifts, policy-row transfer/resume/watch selection, `sock` as the only duplex injection
- Reject: Promise-wrapper packages (`node-ssh`, `ssh2-sftp-client`, `ssh2-promise`, `node-scp`), `Socket` as the ssh2 seam, raw EventEmitter consumption in domain code, in-process rsync-delta reimplementation, SCP-subsystem packages duplicating exec + SFTP
