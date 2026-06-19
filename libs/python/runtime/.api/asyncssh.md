# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` supplies an asyncio SSHv2 client and server: connection establishment, remote process execution, SFTP/SCP file transfer, port forwarding, a key-import/generation surface, known-hosts and authorized-keys verification, and a complete error taxonomy. It is the runtime owner for SSH/SFTP transport over the companion seam.

## [01]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh`
- import: `asyncssh`
- owner: `runtime`
- rail: transport
- namespaces: `asyncssh`, `asyncssh.connection`, `asyncssh.sftp`, `asyncssh.process`, `asyncssh.public_key`
- capability: SSHv2 client/server, remote process execution, SFTP/SCP transfer, port forwarding, key management, host/key verification

## [02]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and process family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :-------------------- | :------------ | :-------------------------- |
|  [01]   | `SSHClientConnection` | connection    | established client session  |
|  [02]   | `SSHServerConnection` | connection    | established server session  |
|  [03]   | `SSHClient`           | handler       | client connection callbacks |
|  [04]   | `SSHServer`           | handler       | server connection callbacks |
|  [05]   | `SSHClientProcess`    | process       | remote process handle       |
|  [06]   | `SSHCompletedProcess` | result        | finished-process result     |
|  [07]   | `SSHClientChannel`    | channel       | session channel             |
|  [08]   | `SSHListener`         | listener      | forwarded/accept listener   |
|  [09]   | `SSHAcceptor`         | acceptor      | server accept loop          |

[PUBLIC_TYPE_SCOPE]: SFTP and key family
- rail: transport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :------------------ | :------------ | :------------------------ |
|  [01]   | `SFTPClient`        | sftp          | SFTP session client       |
|  [02]   | `SFTPClientFile`    | sftp          | open SFTP file handle     |
|  [03]   | `SFTPName`          | sftp          | directory entry           |
|  [04]   | `SFTPAttrs`         | sftp          | file attributes           |
|  [05]   | `SFTPServer`        | sftp          | SFTP server handler       |
|  [06]   | `SSHKey`            | key           | private/public key object |
|  [07]   | `SSHKeyPair`        | key           | key pair                  |
|  [08]   | `SSHCertificate`    | key           | SSH certificate           |
|  [09]   | `SSHKnownHosts`     | verify        | known-hosts database      |
|  [10]   | `SSHAuthorizedKeys` | verify        | authorized-keys database  |
|  [11]   | `SSHAgentClient`    | agent         | ssh-agent client          |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :--------------------- | :------------ | :---------------------------- |
|  [01]   | `Error`                | fault base    | base SSH error                |
|  [02]   | `DisconnectError`      | fault         | connection disconnect         |
|  [03]   | `ConnectionLost`       | fault         | unexpected connection loss    |
|  [04]   | `PermissionDenied`     | fault         | authentication denial         |
|  [05]   | `HostKeyNotVerifiable` | fault         | host-key verification failure |
|  [06]   | `ProcessError`         | fault         | non-zero remote exit          |
|  [07]   | `SFTPError`            | fault base    | SFTP operation error          |
|  [08]   | `SFTPNoSuchFile`       | fault         | missing remote path           |
|  [09]   | `SFTPPermissionDenied` | fault         | SFTP permission denial        |
|  [10]   | `KeyImportError`       | fault         | key parse failure             |

## [03]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session operations
- rail: transport

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------- |
|  [01]   | `connect`                                     | connect        | establish a client connection      |
|  [02]   | `listen`                                      | serve          | start an SSH server                |
|  [03]   | `SSHClientConnection.run`                     | exec           | run a remote command to completion |
|  [04]   | `SSHClientConnection.create_process`          | exec           | spawn a streamed remote process    |
|  [05]   | `SSHClientConnection.start_sftp_client`       | sftp           | open an SFTP session               |
|  [06]   | `SSHClientConnection.forward_local_port`      | forward        | local→remote port forward          |
|  [07]   | `SSHClientConnection.forward_remote_port`     | forward        | remote→local port forward          |
|  [08]   | `SFTPClient.get` / `.put`                     | transfer       | download/upload files              |
|  [09]   | `SFTPClient.glob` / `.listdir`                | traverse       | remote enumeration                 |
|  [10]   | `import_private_key` / `generate_private_key` | key            | load/create a key                  |
|  [11]   | `read_known_hosts`                            | verify         | load known-hosts database          |

## [04]-[IMPLEMENTATION_LAW]

[TRANSPORT_TOPOLOGY]:
- connection law: SSH sessions are `connect` context managers under the anyio lane; the connection is closed deterministically, never leaked across the event loop.
- verification law: host keys are verified against `known_hosts`/`SSHKnownHosts`; `known_hosts=None` (disabled verification) is never used in production.
- credential law: authentication uses keys imported through `import_private_key` from the caller-owned settings model or an `SSHAgentClient`; passwords are admitted only through the settings model, never inline literals.
- execution law: remote commands use `run` for completion or `create_process` for streaming; a non-zero exit raises `ProcessError`, lifted into `Error(BoundaryFault(...))`.
- transfer law: file movement uses the `SFTPClient` surface with `glob`/recursive options; no shell-pipe `scp` invocation.
- resilience law: transient connection faults (`ConnectionLost`, `DisconnectError`) are retried through the `stamina` owner.

[LOCAL_ADMISSION]:
- The transport surface composes asyncssh for SSH/SFTP companion-seam transport; the runtime owns no second SSH client and no durable remote store.
- Key generation/import is a credential admission step bound to the settings model, never a global key cache.

[RAIL_LAW]:
- Package: `asyncssh`
- Owns: SSHv2 client/server sessions, remote process execution, SFTP/SCP transfer, port forwarding, and key/host verification
- Accept: scoped `connect` sessions, verified host keys, settings-model credentials, `run`/`create_process` execution, `SFTPClient` transfer, retried transient faults
- Reject: disabled host-key verification, inline credentials, shell `scp`, leaked connections, a second SSH client
