# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` is the pure-Python asyncio SSHv2/SFTP client over the `cryptography` backend; runtime consumes two slices of it through one dial. Roots' `RemoteEndpoint.dialed` is the single `connect` site: the SFTP chain (`roots.md` `_sftp_session`/`_sftp_read`/`_sftp_chunks`) opens `start_sftp_client` -> `SFTPClient.open` for whole-fetch and streamed reads, and the workers REMOTE arm (`workers.md` `WorkerPool._remote`) opens per-submit `create_process` sessions carrying the sealed kernel to a fleet host. Both ride one `SSHClientConnectionOptions` (password + verified `known_hosts`), and admission's `SecretBoundary.known_hosts` loads the host-key database via `read_known_hosts`. Server sessions, the forwarding family, SCP, key/certificate mint-export, and the ssh-agent are unconsumed and out of runtime scope.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh`
- import: `asyncssh`
- owner: `runtime`
- rail: transport
- license: `EPL-2.0 OR GPL-2.0-or-later` (copyleft dual-license; redistribution and link-shape are constrained — a load-bearing flag, not the permissive MIT/BSD band; consumed as an unmodified library dependency, never vendored or modified in-tree)
- namespaces: `asyncssh`, `asyncssh.connection`, `asyncssh.sftp`, `asyncssh.known_hosts`
- capability: async SFTP read transport — `connect`/`start_sftp_client`/`SFTPClient.open` read chain, one `SSHClientConnectionOptions` (password + verified host keys + connect-timeout and keepalive policy), `read_known_hosts` host-key-database loading — plus the workers remote-exec crossing: per-session `create_process` over binary streams with `terminate`/`kill` deadline escalation

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection, options, and SFTP-read family
- rail: transport

| [INDEX] | [SYMBOL]                     | [TYPE_FAMILY] | [RAIL]                                                             |
| :-----: | :--------------------------- | :------------ | :----------------------------------------------------------------- |
|  [01]   | `SSHClientConnection`        | connection    | established client session (SFTP-open entry; async ctx mgr)        |
|  [02]   | `SSHClientConnectionOptions` | options       | client config object — runtime binds `password` + `known_hosts`    |
|  [03]   | `SFTPClient`                 | sftp          | SFTP session client (async ctx mgr; `open`/`read`)                 |
|  [04]   | `SFTPClientFile`             | sftp          | open remote file handle (`read`/`seek`/`fstat`)                    |
|  [05]   | `SSHKnownHosts`              | verify        | known-hosts database (host-key trust) loaded by `read_known_hosts` |
|  [06]   | `SSHClientProcess`           | exec          | remote process session (async ctx mgr; binary stdio, signal kill)  |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport
- the rail discriminates by exception type, not a numeric field; `ConnectionLost`/`DisconnectError` are the transient retry class, `HostKeyNotVerifiable`/`PermissionDenied` are terminal.

| [INDEX] | [SYMBOL]                                                     | [TYPE_FAMILY] | [RAIL]                                                   |
| :-----: | :----------------------------------------------------------- | :------------ | :------------------------------------------------------- |
|  [01]   | `Error`                                                      | fault base    | base SSH error (carries disconnect `code`/`reason`)      |
|  [02]   | `DisconnectError` / `ConnectionLost`                         | fault         | peer disconnect / unexpected connection loss (transient) |
|  [03]   | `HostKeyNotVerifiable` / `PermissionDenied`                  | fault         | host-key verification failure / auth denial (terminal)   |
|  [04]   | `SFTPError`                                                  | fault base    | SFTP operation error (status-coded base)                 |
|  [05]   | `SFTPNoSuchFile` / `SFTPNoSuchPath` / `SFTPPermissionDenied` | fault         | SFTP status subtypes for a read (POSIX-shaped)           |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection and SFTP-read chain
- rail: transport
- `connect` takes `options=SSHClientConnectionOptions`; the connection and SFTP client are async context managers registered on one `AsyncExitStack` (`roots.md` `_sftp_session`).

Return types are the PUBLIC_TYPES rows; the SURFACE cells drop the `-> Type` tail and the `(async ctx mgr)` note the lead already carries.

| [INDEX] | [SURFACE]                                                                | [ENTRY_FAMILY] | [RAIL]                                     |
| :-----: | :----------------------------------------------------------------------- | :------------- | :----------------------------------------- |
|  [01]   | `await connect(host='', port=(), *, options=SSHClientConnectionOptions)` | connect        | establish a client connection              |
|  [02]   | `await conn.start_sftp_client()`                                         | sftp open      | open an SFTP session                       |
|  [03]   | `await sftp.open(path, FXF_READ, *, block_size=, max_requests=)`         | sftp open file | open a remote file for reading             |
|  [04]   | `await file.read(size=-1, offset=-1)`                                    | sftp io        | whole-fetch or offset-bounded chunked read |

[ENTRYPOINT_SCOPE]: connection-options and host-key verification
- rail: transport
- `SSHClientConnectionOptions` is built once from the settings model (`roots.md` `_ssh_options`); `read_known_hosts` loads the trust database at admission (`admission.md` `SecretBoundary.known_hosts`, ENTRYPOINTS [02]).

| [INDEX] | [SURFACE]                                                                                                             | [ENTRY_FAMILY] | [RAIL]                                               |
| :-----: | :-------------------------------------------------------------------------------------------------------------------- | :------------- | :--------------------------------------------------- |
|  [01]   | `SSHClientConnectionOptions(*, password=, known_hosts=, connect_timeout=, keepalive_interval=, keepalive_count_max=)` | options        | credentials, trust, and channel policy in one object |
|  [02]   | `read_known_hosts(filelist)`                                                                                          | verify         | load the host-key trust database from path(s)        |

[ENTRYPOINT_SCOPE]: remote-exec crossing
- rail: worker fabric
- `create_process` with `encoding=None` (forwarded through `**kwargs` to the session) keeps both stdio streams binary for the workers sealed-blob round trip; the session is an async context manager whose exit closes the channel, so an abandoned session HUP-reaps the far process. `terminate()`/`kill()` send the signal escalation, `exit_status` carries the death evidence, and `SSHClientConnection.is_closed()`/`close()`/`abort()` are the channel-liveness and teardown reads the pool arm consumes.

| [INDEX] | [SURFACE]                                           | [ENTRY_FAMILY] | [RAIL]                                           |
| :-----: | :-------------------------------------------------- | :------------- | :----------------------------------------------- |
|  [01]   | `await conn.create_process(command, encoding=None)` | exec open      | open a remote process session (binary stdio)     |
|  [02]   | `process.stdin.write(data)` / `write_eof()`         | exec io        | send the sealed blob, then EOF the request side  |
|  [03]   | `await process.stdout.read()`                       | exec io        | read the pickled verdict to EOF                  |
|  [04]   | `process.terminate()` / `process.kill()`            | exec kill      | signal escalation on a tripped deadline          |
|  [05]   | `process.exit_status`                               | evidence       | far-process exit code on a torn verdict          |
|  [06]   | `conn.is_closed()` / `close()` / `abort()`          | lifecycle      | channel liveness read, graceful close, hard tear |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- connection law: SFTP reads run `async with connect(..., options=...)` -> `start_sftp_client()` under the anyio lane, both contexts on one `AsyncExitStack` so the connection and SFTP client close deterministically on exit, never leaked across the event loop (`roots.md` `_sftp_session`).
- options law: connection configuration is one `SSHClientConnectionOptions` built from the settings-model password, the admission-supplied `known_hosts`, and the roots connect-timeout/keepalive constants — keepalive probes keep the workers arm's `is_closed` liveness read honest between submits — never scattered `connect(host, port=, password=, known_hosts=)` keyword soup duplicated per call.
- verification law: host keys verify against the `SSHKnownHosts` database `read_known_hosts` loads at admission; disabled verification (`known_hosts=None`) is never used. The `password.get_secret_value()` un-mask happens only inside `_ssh_options`.
- read law: file movement is SFTP read only — `SFTPClient.open(...).read()` for whole fetch and an offset-driven loop for streamed chunks (`roots.md` `_sftp_read`/`_sftp_chunks`); there is no `subprocess` shell-pipe, no SCP, no write leg.
- resilience law: transient connection faults (`ConnectionLost`, `DisconnectError`) retry through the `stamina` owner keyed by the `TransferPlan.retry_class`; both subclass `asyncssh.Error`, never builtin `ConnectionError`, so the `RetryClass.SSH` row name-matches them at the BASE tier; `HostKeyNotVerifiable`/`PermissionDenied` and the `SFTPNoSuchFile`/`SFTPPermissionDenied` read faults are terminal, lifted to a `BoundaryFault` immediately.
- exec law: remote process execution is consumed through exactly one arm — `WorkerPool._remote` opens per-submit `create_process(command, encoding=None)` sessions over the memoized connection `RemoteEndpoint.dialed` supplies, writes one cloudpickle-sealed blob, EOFs, and reads one pickled verdict to EOF; the session context exit closes the channel, so a cooperative abandon reaps the far process and a `TERMINAL` deadline escalates through `kill()`. `run()` and the high-level exec conveniences stay unconsumed — the one-blob stdio contract needs no shell quoting, capture policy, or check semantics.
- scope law: server sessions (`listen`/`SSHServer`), the local/remote/SOCKS/UNIX forwarding family, native `scp()`, key/certificate import-generate-export, and the ssh-agent client are UNCONSUMED in runtime — this catalog carries none of them; any future need is a live fence, not a speculative re-catalog.

[LOCAL_ADMISSION]:
- The transport surface composes asyncssh for the SFTP-read companion seam only; the runtime owns no second SSH client (`paramiko`, a blocking-IO peer, is not admitted) and no durable remote store.
- `cryptography` is asyncssh's crypto kernel, reached only through asyncssh's surface, never instantiated in parallel.
- The dual EPL-2.0/GPL-2.0-or-later license constrains redistribution: asyncssh is consumed as an unmodified library dependency over its public API — never vendored, statically embedded, or modified in-tree.

[STACK_LAW]:
- `SSHClientConnectionOptions(password=<settings SecretStr>, known_hosts=<admission SSHKnownHosts>)` (`roots.md` `_ssh_options`) -> `RemoteEndpoint.dialed()` -> `start_sftp_client()` -> `SFTPClient.open(relative).read()` (`roots.md` `_sftp_session`/`_sftp_read`): one options object, one exit stack, configuration flowing from the `pydantic-settings` settings model and the admission host-key loader.
- `RemoteEndpoint.dialed()` -> memoized `SSHClientConnection` per `WorkerPool` REMOTE arm -> per-submit `create_process(f"{endpoint.python} -m rasm.runtime.workers", encoding=None)` -> sealed blob on `stdin` -> pickled verdict off `stdout` (`workers.md` `WorkerPool._remote`/`remote_floor`): cloudpickle seals the request, tblib carries the fault frames home, and the `RetryClass.SSH` row re-drives channel transients under the kernel's `idempotent` declaration.

[RAIL_LAW]:
- Package: `asyncssh`
- Owns: the SFTP-read companion-seam slice — the `connect`/`start_sftp_client`/`SFTPClient.open` read chain, one `SSHClientConnectionOptions` (password + verified host keys), `read_known_hosts` host-key-database loading — and the workers remote-exec crossing's `create_process` sessions
- Accept: `RemoteEndpoint.dialed` as the one `connect` site, verified host keys via `read_known_hosts`/`SSHKnownHosts`, settings-model password, `SFTPClient.open(...).read()` whole/chunked reads, per-submit `create_process(command, encoding=None)` sessions on the workers REMOTE arm, transient connection faults retried through `stamina`
- Reject: disabled host-key verification (`known_hosts=None`), inline password/key literals, scattered `connect(...)` keyword soup instead of one options object, a second dial spelling beside `RemoteEndpoint.dialed`, leaked connections, a second SSH client (`paramiko`), vendoring/modifying the GPL/EPL source, and cataloguing the unconsumed server/forwarding/SCP/key-mint/agent surfaces
