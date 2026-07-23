# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` is the pure-Python asyncio SSHv2/SFTP client over the `cryptography` backend; runtime dials it once at `RemoteEndpoint.dialed` in two slices: SFTP reads (`roots.md` `_sftp_session`) open `connect` -> `start_sftp_client` -> `SFTPClient.open`, and the workers arm (`workers.md` `WorkerPool._remote`) opens per-submit `create_process` sessions carrying the sealed kernel to a host. Both ride one `SSHClientConnectionOptions`; server sessions, forwarding, SCP, key-mint, and the ssh-agent stay out of scope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh` (`EPL-2.0 OR GPL-2.0-or-later`, copyleft)
- module: `asyncssh`
- rail: transport, worker fabric
- namespaces: `asyncssh`, `asyncssh.connection`, `asyncssh.sftp`, `asyncssh.known_hosts`

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection, options, and SFTP-read family
- rail: transport

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [CAPABILITY]                                                       |
| :-----: | :--------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SSHClientConnection`        | connection    | established client session (SFTP-open entry; async ctx mgr)        |
|  [02]   | `SSHClientConnectionOptions` | options       | client config object — runtime binds `password` + `known_hosts`    |
|  [03]   | `SFTPClient`                 | sftp          | SFTP session client (async ctx mgr; `open`/`read`)                 |
|  [04]   | `SFTPClientFile`             | sftp          | open remote file handle (`read`/`seek`/`stat`)                     |
|  [05]   | `SSHKnownHosts`              | verify        | known-hosts database (host-key trust) loaded by `read_known_hosts` |
|  [06]   | `SSHClientProcess`           | exec          | remote process session (async ctx mgr; binary stdio, signal kill)  |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport
- Exception type discriminates: `ConnectionLost`/`DisconnectError` are the transient retry class; `HostKeyNotVerifiable`/`PermissionDenied` and the `SFTP*` read faults are terminal.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [CAPABILITY]                                             |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Error`                                                      | fault base    | base SSH error (carries disconnect `code`/`reason`)      |
|  [02]   | `DisconnectError` / `ConnectionLost`                         | fault         | peer disconnect / unexpected connection loss (transient) |
|  [03]   | `HostKeyNotVerifiable` / `PermissionDenied`                  | fault         | host-key verification failure / auth denial (terminal)   |
|  [04]   | `SFTPError`                                                  | fault base    | SFTP operation error (status-coded base)                 |
|  [05]   | `SFTPNoSuchFile` / `SFTPNoSuchPath` / `SFTPPermissionDenied` | fault         | SFTP status subtypes for a read (POSIX-shaped)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and SFTP-read chain
- rail: transport

| [INDEX] | [SURFACE]                                                                | [SHAPE]        | [CAPABILITY]                               |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `await connect(host='', port=(), *, options=SSHClientConnectionOptions)` | connect        | establish a client connection              |
|  [02]   | `await conn.start_sftp_client()`                                         | sftp open      | open an SFTP session                       |
|  [03]   | `await sftp.open(path, FXF_READ, block_size=, max_requests=)`            | sftp open file | open a remote file for reading             |
|  [04]   | `await file.read(size=-1, offset=-1)`                                    | sftp io        | whole-fetch or offset-bounded chunked read |

[ENTRYPOINT_SCOPE]: connection-options and host-key verification
- rail: transport
- `SSHClientConnectionOptions` is built once from the settings model (`roots.md` `_ssh_options`) carrying credentials, trust, and channel policy in one object; `read_known_hosts` loads the host-key trust database at admission (`admission.md` `SecretBoundary.known_hosts`).

| [INDEX] | [SURFACE]                                                                                                             | [SHAPE] |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------ |
|  [01]   | `SSHClientConnectionOptions(*, password=, known_hosts=, connect_timeout=, keepalive_interval=, keepalive_count_max=)` | options |
|  [02]   | `read_known_hosts(filelist)`                                                                                          | verify  |

[ENTRYPOINT_SCOPE]: remote-exec crossing
- rail: worker fabric
- `create_process(command, encoding=None)` keeps both stdio streams binary for the sealed-blob round trip; the session is an async context manager whose exit closes the channel, so a cooperatively abandoned session HUP-reaps the far process.

| [INDEX] | [SURFACE]                                           | [SHAPE]   | [CAPABILITY]                                     |
| :-----: | :-------------------------------------------------- | :-------- | :----------------------------------------------- |
|  [01]   | `await conn.create_process(command, encoding=None)` | exec open | open a remote process session (binary stdio)     |
|  [02]   | `process.stdin.write(data)` / `write_eof()`         | exec io   | send the sealed blob, then EOF the request side  |
|  [03]   | `await process.stdout.read()`                       | exec io   | read the pickled verdict to EOF                  |
|  [04]   | `process.terminate()` / `process.kill()`            | exec kill | signal escalation on a tripped deadline          |
|  [05]   | `process.exit_status`                               | evidence  | far-process exit code on a torn verdict          |
|  [06]   | `conn.is_closed()` / `close()` / `abort()`          | lifecycle | channel liveness read, graceful close, hard tear |

## [04]-[IMPLEMENTATION_LAW]

[TOPOLOGY]:
- connection law: SFTP reads run `async with connect(..., options=...)` -> `start_sftp_client()` under the anyio lane, both contexts on one `AsyncExitStack` that closes the connection and SFTP client deterministically on exit (`roots.md` `_sftp_session`).
- options law: connection configuration is one `SSHClientConnectionOptions` from the settings-model password, the admission `known_hosts`, and the roots connect-timeout/keepalive constants; keepalive probes keep the workers arm's `is_closed` liveness read honest between submits.
- verification law: host keys verify against the `SSHKnownHosts` database `read_known_hosts` loads at admission; the `password.get_secret_value()` un-mask happens only inside `_ssh_options`.
- read law: file movement is SFTP read only — `SFTPClient.open(...).read()` for whole fetch and an offset-driven loop for streamed chunks (`roots.md` `_sftp_read`/`_sftp_chunks`).
- resilience law: transient connection faults retry, while `HostKeyNotVerifiable`/`PermissionDenied` and the `SFTP*` read faults lift to a `BoundaryFault` immediately.
- exec law: remote execution is one arm — `WorkerPool._remote` opens per-submit `create_process(command, encoding=None)` over the memoized `RemoteEndpoint.dialed` connection, the session-context exit reaps the far process, and a `TERMINAL` deadline escalates through `kill()`; the high-level `run()` conveniences stay unconsumed, the one-blob stdio contract needing no shell quoting or capture policy.

[STACKING]:
- `stamina`(`.api/stamina.md`): transient `ConnectionLost`/`DisconnectError` — `asyncssh.Error` subclasses, never builtin `ConnectionError` — retry through `stamina` keyed by `TransferPlan.retry_class`; the `RetryClass.SSH` row name-matches them at the BASE tier.
- `transport/roots`(`runtime/.planning/transport/roots.md`): `SSHClientConnectionOptions(password=<settings SecretStr>, known_hosts=<admission SSHKnownHosts>)` -> `RemoteEndpoint.dialed()` -> `start_sftp_client()` -> `SFTPClient.open(relative).read()` — one options object, one exit stack, configuration flowing from the `pydantic-settings` model and the admission host-key loader.
- `execution/workers`(`runtime/.planning/execution/workers.md`): `RemoteEndpoint.dialed()` -> memoized `SSHClientConnection` per REMOTE arm -> per-submit `create_process(f"{endpoint.python} -m rasm.runtime.workers", encoding=None)` -> sealed blob on `stdin` -> pickled verdict off `stdout`; cloudpickle seals the request, tblib carrying the fault frames home.

[LOCAL_ADMISSION]:
- Transport composes asyncssh for the SFTP-read companion seam only; the runtime owns no second SSH client (`paramiko` is not admitted) and no durable remote store.
- `cryptography` backs asyncssh as its crypto kernel, reached only through asyncssh's surface, never instantiated in parallel.
- Copyleft (`EPL-2.0`/`GPL-2.0-or-later`) constrains redistribution: asyncssh is consumed as an unmodified library dependency over its public API, never vendored, embedded, or modified in-tree.

[RAIL_LAW]:
- Package: `asyncssh`
- Owns: the SFTP-read companion seam — the `connect`/`start_sftp_client`/`SFTPClient.open` read chain, one `SSHClientConnectionOptions` (password, verified host keys), `read_known_hosts` host-key-database loading — and the workers remote-exec crossing's `create_process` sessions
- Accept: `RemoteEndpoint.dialed` as the one `connect` site, verified host keys via `read_known_hosts`/`SSHKnownHosts`, settings-model password, `SFTPClient.open(...).read()` whole/chunked reads, per-submit `create_process(command, encoding=None)` sessions on the workers REMOTE arm, transient faults retried through `stamina`
- Reject: disabled host-key verification (`known_hosts=None`), inline password/key literals, scattered `connect(...)` keyword soup beside one options object, a second dial spelling past `RemoteEndpoint.dialed`, leaked connections, a second SSH client (`paramiko`), vendoring or modifying the copyleft source, and the unconsumed server/forwarding/SCP/key-mint/agent surfaces
