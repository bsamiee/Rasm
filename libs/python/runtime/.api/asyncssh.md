# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` supplies a pure-Python asyncio SSHv2 client and server over the `cryptography` backend: connection establishment (forward and reverse), remote process execution with streaming and `communicate`, native async SFTP/SCP transfer with glob and recursive multi-file ops, the full port/UNIX/SOCKS forwarding family, a connection-options object, a key/certificate import-generate-export surface, known-hosts/authorized-keys verification, and a complete SSH+SFTP error taxonomy. It is the runtime owner for SSH/SFTP transport over the companion seam; the native async `scp()` is the SCP owner, never a shell-pipe.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh`
- import: `asyncssh`
- owner: `runtime`
- rail: transport
- version: `2.23.1`
- license: `EPL-2.0 OR GPL-2.0-or-later` (copyleft dual-license; redistribution and link-shape are constrained — a load-bearing flag, not the permissive MIT/BSD band)
- namespaces: `asyncssh`, `asyncssh.connection`, `asyncssh.sftp`, `asyncssh.process`, `asyncssh.stream`, `asyncssh.public_key`, `asyncssh.known_hosts`, `asyncssh.auth_keys`, `asyncssh.listener`, `asyncssh.agent`
- capability: SSHv2 client/server (forward + reverse), remote process execution (run/streamed/communicate), native async SFTP + SCP transfer, local/remote/SOCKS/UNIX forwarding, connection-options config, key/cert import-generate-export, host/authorized-keys verification, ssh-agent client

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection, options, and handler family
- rail: transport

| [INDEX] | [SYMBOL]                       | [TYPE_FAMILY]  | [RAIL]                                                       |
| :-----: | :----------------------------- | :------------- | :----------------------------------------------------------- |
|  [01]   | `SSHClientConnection`          | connection     | established client session (exec/sftp/forward entry)         |
|  [02]   | `SSHServerConnection`          | connection     | established server session                                   |
|  [03]   | `SSHClientConnectionOptions`   | options        | client config object (`username`/`client_keys`/`known_hosts`/`config`/...) |
|  [04]   | `SSHServerConnectionOptions`   | options        | server config object (`server_host_keys`/`authorized_client_keys`/...) |
|  [05]   | `SSHClient` / `SSHServer`      | handler        | connection-lifecycle callback handlers                       |
|  [06]   | `SSHAcceptor`                  | acceptor       | `listen`/`listen_reverse` server accept loop (async ctx mgr) |
|  [07]   | `SSHListener`                  | listener       | a forwarded-port/path listener handle                        |

[PUBLIC_TYPE_SCOPE]: process, channel, and stream family
- rail: transport

| [INDEX] | [SYMBOL]                                  | [TYPE_FAMILY] | [RAIL]                                                       |
| :-----: | :---------------------------------------- | :------------ | :----------------------------------------------------------- |
|  [01]   | `SSHClientProcess`                        | process       | streamed remote process (`stdin`/`stdout`/`stderr`, `communicate`, `kill`, `terminate`, `exit_status`) |
|  [02]   | `SSHCompletedProcess`                     | result        | finished-process result namedtuple (`exit_status`/`exit_signal`/`returncode`/`stdout`/`stderr`/`env`/`command`) |
|  [03]   | `SSHClientChannel` / `SSHServerChannel`   | channel       | session channel (PTY/env/signal control)                     |
|  [04]   | `SSHTCPChannel` / `SSHUNIXChannel`        | channel       | forwarded TCP / UNIX-domain channel                          |
|  [05]   | `SSHReader` / `SSHWriter`                 | stream        | asyncio-style stream pair from `open_connection`/process std streams |

[PUBLIC_TYPE_SCOPE]: SFTP family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :-------------------- | :------------ | :------------------------------------------------------ |
|  [01]   | `SFTPClient`          | sftp          | SFTP session client (async ctx mgr; transfer/traverse/attr) |
|  [02]   | `SFTPClientFile`      | sftp          | open remote file handle (`read`/`write`/`seek`/`fstat`) |
|  [03]   | `SFTPName`            | sftp          | directory entry (`filename`/`longname`/`attrs`)         |
|  [04]   | `SFTPAttrs`           | sftp          | file attributes (`size`/`permissions`/`mtime`/`uid`/`gid`/...) |
|  [05]   | `SFTPVFSAttrs`        | sftp          | filesystem-stat result (`statvfs`)                      |
|  [06]   | `SFTPLimits`          | sftp          | negotiated server limits (`max_read_len`/`max_write_len`) |
|  [07]   | `SFTPServer`          | sftp          | SFTP server handler base (override for custom server fs) |

[PUBLIC_TYPE_SCOPE]: key, certificate, and verification family
- rail: transport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                                                  |
| :-----: | :------------------ | :------------ | :------------------------------------------------------ |
|  [01]   | `SSHKey`            | key           | private/public key (`sign`/`verify`/`export_*`/`get_fingerprint`/`generate_*_certificate`) |
|  [02]   | `SSHKeyPair`        | key           | algorithm-tagged key pair for auth                      |
|  [03]   | `SSHCertificate`    | key           | OpenSSH/X.509 certificate object                        |
|  [04]   | `SSHKnownHosts`     | verify        | known-hosts database (host-key trust)                   |
|  [05]   | `SSHAuthorizedKeys` | verify        | authorized-keys database (server-side client trust)     |
|  [06]   | `SSHAgentClient` / `SSHAgentKeyPair` | agent | ssh-agent client + agent-resident key pair         |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport
- `Error(code, reason, lang)` is the disconnect-coded base; `SFTPError(code, reason, lang)` is the SFTP-coded base. Concrete subtypes carry the SSH disconnect code / SFTP status code, so the rail discriminates by exception type, not by reading a numeric field.

| [INDEX] | [SYMBOL]                                                         | [TYPE_FAMILY] | [RAIL]                                          |
| :-----: | :--------------------------------------------------------------- | :------------ | :---------------------------------------------- |
|  [01]   | `Error`                                                          | fault base    | base SSH error (carries disconnect `code`/`reason`) |
|  [02]   | `DisconnectError` / `ConnectionLost`                            | fault         | peer disconnect / unexpected connection loss    |
|  [03]   | `PermissionDenied` / `IllegalUserName`                          | fault         | authentication denial / bad username            |
|  [04]   | `PasswordChangeRequired`                                        | fault         | server demands a password change                |
|  [05]   | `HostKeyNotVerifiable`                                          | fault         | host-key verification failure                   |
|  [06]   | `KeyExchangeFailed` / `MACError` / `CompressionError` / `ProtocolError` / `ProtocolNotSupported` | fault | transport-negotiation faults     |
|  [07]   | `ChannelOpenError` / `ChannelListenError`                       | fault         | channel/forward open rejection                  |
|  [08]   | `ProcessError`                                                  | fault         | non-zero remote exit (`check=True`; carries `exit_status`/`stdout`/`stderr`) |
|  [09]   | `KeyImportError` / `KeyExportError` / `KeyEncryptionError` / `KeyGenerationError` | fault | key parse/serialize/encrypt/generate failure |
|  [10]   | `SFTPError`                                                     | fault base    | SFTP operation error (status-coded base)        |
|  [11]   | `SFTPNoSuchFile` / `SFTPNoSuchPath` / `SFTPPermissionDenied` / `SFTPFailure` / `SFTPEOFError` / `SFTPFileAlreadyExists` / `SFTPNotADirectory` / `SFTPNoSpaceOnFilesystem` | fault | SFTP status subtypes (POSIX-shaped) |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: connection establishment (module-level coroutines)
- rail: transport
- every connector takes a `config=` (ssh_config path) and an `options=SSHClientConnectionOptions`/`SSHServerConnectionOptions`, with all option fields also accepted as `**kwargs`; the connection objects are async context managers.

| [INDEX] | [SURFACE]                                                                                          | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :------------------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `await connect(host='', port=(), *, tunnel=, family=, local_addr=, sock=, config=, options=, **kwargs) -> SSHClientConnection` | connect | establish a client connection (async ctx mgr) |
|  [02]   | `await listen(host='', port=(), *, acceptor=, error_handler=, backlog=100, config=, options=, **kwargs) -> SSHAcceptor` | serve | start an SSH server                          |
|  [03]   | `await connect_reverse(...) -> SSHServerConnection`                                                | reverse        | client socket runs the server role (reverse-direction SSH) |
|  [04]   | `await listen_reverse(...) -> SSHAcceptor`                                                         | reverse        | server socket runs the client role          |
|  [05]   | `await run_client(sock, ...) -> SSHClientConnection` / `await run_server(sock, ...) -> SSHServerConnection` | adopt-socket | run SSH over an already-connected socket  |
|  [06]   | `await get_server_host_key(host, port=(), *, kex_algs=, server_host_key_algs=, ...) -> SSHKey\|None` | probe         | fetch a server's host key without authenticating (TOFU pinning) |

[ENTRYPOINT_SCOPE]: remote execution and forwarding (on `SSHClientConnection`)
- rail: transport
- defined on `SSHClientConnection` (PUBLIC_TYPES connection [01]).

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `await conn.run(*args, check=False, timeout=None, input=, env=, **kwargs) -> SSHCompletedProcess` | exec    | run a remote command to completion (`check=True` raises `ProcessError`) |
|  [02]   | `await conn.create_process(*args, input=, stdin=, stdout=, stderr=, bufsize=, **kwargs) -> SSHClientProcess` | exec | spawn a streamed remote process (`communicate`/`stdin`/`stdout`) |
|  [03]   | `await conn.create_session(session_factory, command=, *, request_pty=, env=, term_type=, ...)` | exec      | low-level channel+session pair with PTY/env control |
|  [04]   | `await conn.open_connection(...) -> (SSHReader, SSHWriter)`                                 | stream         | asyncio stream-pair over a forwarded TCP channel |
|  [05]   | `await conn.forward_local_port(listen_host, listen_port, dest_host, dest_port, accept_handler=) -> SSHListener` | forward | local->remote TCP forward                    |
|  [06]   | `await conn.forward_remote_port(listen_host, listen_port, dest_host, dest_port) -> SSHListener` | forward    | remote->local TCP forward                    |
|  [07]   | `await conn.forward_socks(listen_host, listen_port) -> SSHListener`                         | forward        | dynamic SOCKS proxy over the connection      |
|  [08]   | `forward_local_path` / `forward_remote_path` / `forward_local_port_to_path` / `forward_remote_path_to_port` | forward | UNIX-domain-socket forwarding variants       |

[ENTRYPOINT_SCOPE]: SFTP and SCP transfer
- rail: transport
- `start_sftp_client` on the connection yields `SFTPClient`; the top-level `scp` is the native async SCP owner (no shell `scp` subprocess). Transfer methods take `recurse`/`preserve`/`progress_handler`/`error_handler`/`max_requests` for parallel, resumable movement.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `await conn.start_sftp_client(env=, path_encoding='utf-8', sftp_version=3) -> SFTPClient`  | sftp open      | open an SFTP session (async ctx mgr)         |
|  [02]   | `await sftp.get(remotepaths, localpath=None, *, recurse=, preserve=, follow_symlinks=, block_size=, max_requests=, progress_handler=, error_handler=)` / `.put(...)` | transfer | download / upload (single or sequence) |
|  [03]   | `await sftp.mget(remotepaths, ...)` / `.mput(...)` / `.mcopy(...)`                          | transfer       | glob-expanding multi-file download/upload/server-copy |
|  [04]   | `await sftp.glob(patterns, error_handler=) -> Sequence[bytes\|str]`                        | traverse       | server-side glob enumeration                 |
|  [05]   | `await sftp.listdir(path)` / `.scandir(path)` / `.readdir(path)` / `.walk(...)`            | traverse       | directory enumeration (scandir yields `SFTPName`) |
|  [06]   | `await sftp.open(path, pflags_or_mode=FXF_READ, attrs=, encoding='utf-8', block_size=, max_requests=) -> SFTPClientFile` | sftp open file | open a remote file (`FXF_*` flags or POSIX mode string) |
|  [07]   | `await file.read(size=-1, offset=-1)` / `.write(data, offset=-1)` / `.seek(...)` / `.fstat()` | sftp io     | read/write/seek an open SFTP file            |
|  [08]   | `await sftp.stat(path)` / `.lstat(path)` / `.setstat(path, attrs)` / `.statvfs(path)` / `.chmod` / `.chown` / `.utime` / `.truncate` | attr | inspect/mutate remote attributes |
|  [09]   | `await sftp.makedirs(path, exist_ok=)` / `.rmtree(path)` / `.mkdir` / `.rmdir` / `.remove` / `.rename` / `.posix_rename` / `.symlink` / `.readlink` / `.remote_copy` | fs ops | recursive dir + link + server-side copy ops  |
|  [10]   | `await scp(srcpaths, dstpath, *, recurse=, preserve=, block_size=262144, progress_handler=, error_handler=)` | transfer | native async SCP (paths may be `(conn, path)` tuples for remote-to-remote) |

[ENTRYPOINT_SCOPE]: key, certificate, and verification I/O
- rail: transport
- import/read load from bytes/files; `SSHKey.export_*`/`generate_*_certificate` serialize and mint; `match_known_hosts` is the trust-decision primitive the connection uses internally.

| [INDEX] | [SURFACE]                                                                                  | [ENTRY_FAMILY] | [RAIL]                                       |
| :-----: | :----------------------------------------------------------------------------------------- | :------------- | :------------------------------------------- |
|  [01]   | `import_private_key(data, passphrase=None, ...) -> SSHKey` / `import_public_key(data) -> SSHKey` / `import_certificate(data) -> SSHCertificate` | key import | parse from bytes/str         |
|  [02]   | `read_private_key(filename, passphrase=None)` / `read_public_key` / `read_certificate` / `read_known_hosts(filelist) -> SSHKnownHosts` / `read_authorized_keys(filelist) -> SSHAuthorizedKeys` | key read | load from path(s) |
|  [03]   | `import_known_hosts(data) -> SSHKnownHosts` / `import_authorized_keys(data) -> SSHAuthorizedKeys`            | verify import  | parse trust DBs from string                  |
|  [04]   | `generate_private_key(alg_name, comment=None, **kwargs) -> SSHKey`                         | key gen        | mint a new key (`'ssh-ed25519'`/`'ssh-rsa'`/`'ecdsa-sha2-*'`) |
|  [05]   | `key.export_private_key(...)` / `key.export_public_key(...)` / `key.write_private_key(filename, ...)` / `key.get_fingerprint()` / `key.sign(data)` / `key.verify(data, sig)` | key export | serialize / fingerprint / sign / verify |
|  [06]   | `key.generate_user_certificate(...)` / `key.generate_host_certificate(...)` / `key.generate_x509_user_certificate(...)` | cert mint | mint OpenSSH / X.509 certificates           |
|  [07]   | `match_known_hosts(known_hosts, host, addr, port) -> (host_keys, ca_keys, revoked_keys, ...)` | verify      | resolve trusted/CA/revoked keys for a target |
|  [08]   | `await connect_agent(agent_path=None) -> SSHAgentClient`                                   | agent          | open an ssh-agent client (agent-backed keys) |
|  [09]   | `set_log_level(level)` / `set_sftp_log_level(level)` / `set_debug_level(level)`            | observe        | route the package logger into the host logging surface |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- connection law: SSH sessions are `async with connect(...)`/`listen(...)` context managers under the anyio lane; the `SSHClientConnection`/`SSHAcceptor` is closed deterministically (`close()`+`wait_closed()` on exit), never leaked across the event loop.
- options law: connection configuration is one `SSHClientConnectionOptions`/`SSHServerConnectionOptions` built from the caller-owned settings model (or an `ssh_config` path via `config=`); option fields are not scattered as ad hoc `connect(...)` keyword soup duplicated per call site.
- verification law: host keys verify against `known_hosts`/`SSHKnownHosts` (or `get_server_host_key` for explicit TOFU pinning); `known_hosts=None` (disabled verification) is never used in production. `match_known_hosts` owns the trust decision; server-side, `SSHAuthorizedKeys` owns client-key trust.
- credential law: authentication uses keys imported through `import_private_key`/`read_private_key` from the settings model or an `SSHAgentClient`; passwords are admitted only through the settings model, never inline literals. Encrypted keys carry a passphrase from the settings model (requires the `bcrypt` extra for OpenSSH-format encryption).
- execution law: remote commands use `conn.run(..., check=True)` for completion (a non-zero exit raises `ProcessError`, lifted into `Error(BoundaryFault(...))`) or `create_process` for streaming with `communicate`; the boundary reads `SSHCompletedProcess.exit_status`/`.stdout`/`.stderr`, never parses a shell wrapper.
- transfer law: file movement uses `SFTPClient` (`get`/`put`/`mget`/`mput` with `recurse`/`preserve`/`progress_handler`) or the native async `scp()`; there is no `subprocess` shell-pipe `scp`. `remote_copy`/`mcopy` perform server-side copy without round-tripping bytes through the client.
- forwarding law: tunnels use the `forward_local_port`/`forward_remote_port`/`forward_socks`/UNIX-path family returning an `SSHListener` held for the tunnel lifetime; a forwarded channel is `open_connection`'d as an asyncio stream pair, not a raw socket.
- resilience law: transient connection faults (`ConnectionLost`, `DisconnectError`, `ChannelOpenError`) are retried through the `stamina` owner; `PermissionDenied`/`HostKeyNotVerifiable` are non-retryable terminal faults that surface immediately.

[LOCAL_ADMISSION]:
- The transport surface composes asyncssh for SSH/SFTP companion-seam transport; the runtime owns no second SSH client and no durable remote store. `paramiko` (a blocking-IO peer with no asyncio model) is not admitted.
- Key generation/import is a credential admission step bound to the settings model, never a global key cache. `cryptography` is asyncssh's crypto kernel and is reached only through asyncssh's key surface, not instantiated in parallel.
- The dual EPL-2.0/GPL-2.0-or-later license constrains redistribution: asyncssh is consumed as an unmodified library dependency over its public API; it is never vendored, statically embedded, or modified in-tree.

[STACK_LAW]:
- `async with connect(host, options=SSHClientConnectionOptions(**settings_model_fields)) as conn:` under the anyio runtime -> `await conn.run(cmd, check=True)` -> `ProcessError`/`ConnectionLost` lift to `BoundaryFault` -> the `stamina.retry_context` owner retries the transient subset: one transport rail, configuration flowing from the `pydantic-settings` settings model into one options object.
- An `fsspec` SFTP backend and this owner are distinct: bulk protocol-dispatched resource I/O routes through `fsspec` (`sftp://` via `paramiko` extra) only when the resource abstraction is wanted; direct, typed SSH command execution and streamed forwarding are this owner's exclusive surface.
- `set_log_level`/`set_sftp_log_level` route the package logger into the host structured-logging surface once at composition; per-call OTel spans wrap `connect`/`run`/`get` at the boundary, reading `SSHCompletedProcess`/`SFTPName` fields, not a per-command print.

[RAIL_LAW]:
- Package: `asyncssh`
- Owns: SSHv2 client/server sessions (forward + reverse), remote process execution (run/streamed/communicate), native async SFTP + SCP transfer, the local/remote/SOCKS/UNIX forwarding family, connection-options config, key/certificate import-generate-export, host/authorized-keys verification, and the ssh-agent client
- Accept: scoped `async with connect(...)` sessions, one `SSHClientConnectionOptions` from the settings model, verified host keys (`known_hosts`/`get_server_host_key`/`match_known_hosts`), settings-model credentials or `SSHAgentClient`, `run(check=True)`/`create_process` execution, `SFTPClient`/native `scp()` transfer with `recurse`/`progress_handler`, the forwarding family, retried transient faults through `stamina`
- Reject: disabled host-key verification (`known_hosts=None`), inline password/key literals, shell-pipe `scp`/`subprocess` SSH, a second SSH client (`paramiko` direct), leaked connections, scattered `connect(...)` keyword soup instead of one options object, vendoring/modifying the GPL/EPL source, reading return values inline instead of `SSHCompletedProcess`/the event taxonomy
