# [PY_RUNTIME_API_ASYNCSSH]

`asyncssh` supplies an asyncio SSHv2 client and server: connection establishment, remote process execution, SFTP/SCP file transfer, port forwarding, a key-import/generation surface, known-hosts and authorized-keys verification, and a complete error taxonomy. It is the runtime owner for SSH/SFTP transport over the companion seam.

## [1]-[PACKAGE_SURFACE]

[PACKAGE_SURFACE]: `asyncssh`
- package: `asyncssh`
- import: `asyncssh`
- owner: `runtime`
- rail: transport
- namespaces: `asyncssh`, `asyncssh.connection`, `asyncssh.sftp`, `asyncssh.process`, `asyncssh.public_key`
- capability: SSHv2 client/server, remote process execution, SFTP/SCP transfer, port forwarding, key management, host/key verification

## [2]-[PUBLIC_TYPES]

[PUBLIC_TYPE_SCOPE]: connection and process family
- rail: transport

| [INDEX] | [SYMBOL]              | [TYPE_FAMILY] | [RAIL]                      |
| :-----: | :-------------------- | :------------ | :-------------------------- |
|   [1]   | `SSHClientConnection` | connection    | established client session  |
|   [2]   | `SSHServerConnection` | connection    | established server session  |
|   [3]   | `SSHClient`           | handler       | client connection callbacks |
|   [4]   | `SSHServer`           | handler       | server connection callbacks |
|   [5]   | `SSHClientProcess`    | process       | remote process handle       |
|   [6]   | `SSHCompletedProcess` | result        | finished-process result     |
|   [7]   | `SSHClientChannel`    | channel       | session channel             |
|   [8]   | `SSHListener`         | listener      | forwarded/accept listener   |
|   [9]   | `SSHAcceptor`         | acceptor      | server accept loop          |

[PUBLIC_TYPE_SCOPE]: SFTP and key family
- rail: transport

| [INDEX] | [SYMBOL]            | [TYPE_FAMILY] | [RAIL]                    |
| :-----: | :------------------ | :------------ | :------------------------ |
|   [1]   | `SFTPClient`        | sftp          | SFTP session client       |
|   [2]   | `SFTPClientFile`    | sftp          | open SFTP file handle     |
|   [3]   | `SFTPName`          | sftp          | directory entry           |
|   [4]   | `SFTPAttrs`         | sftp          | file attributes           |
|   [5]   | `SFTPServer`        | sftp          | SFTP server handler       |
|   [6]   | `SSHKey`            | key           | private/public key object |
|   [7]   | `SSHKeyPair`        | key           | key pair                  |
|   [8]   | `SSHCertificate`    | key           | SSH certificate           |
|   [9]   | `SSHKnownHosts`     | verify        | known-hosts database      |
|  [10]   | `SSHAuthorizedKeys` | verify        | authorized-keys database  |
|  [11]   | `SSHAgentClient`    | agent         | ssh-agent client          |

[PUBLIC_TYPE_SCOPE]: fault family
- rail: transport

| [INDEX] | [SYMBOL]               | [TYPE_FAMILY] | [RAIL]                        |
| :-----: | :--------------------- | :------------ | :---------------------------- |
|   [1]   | `Error`                | fault base    | base SSH error                |
|   [2]   | `DisconnectError`      | fault         | connection disconnect         |
|   [3]   | `ConnectionLost`       | fault         | unexpected connection loss    |
|   [4]   | `PermissionDenied`     | fault         | authentication denial         |
|   [5]   | `HostKeyNotVerifiable` | fault         | host-key verification failure |
|   [6]   | `ProcessError`         | fault         | non-zero remote exit          |
|   [7]   | `SFTPError`            | fault base    | SFTP operation error          |
|   [8]   | `SFTPNoSuchFile`       | fault         | missing remote path           |
|   [9]   | `SFTPPermissionDenied` | fault         | SFTP permission denial        |
|  [10]   | `KeyImportError`       | fault         | key parse failure             |

## [3]-[ENTRYPOINTS]

[ENTRYPOINT_SCOPE]: session operations
- rail: transport

| [INDEX] | [SURFACE]                                     | [ENTRY_FAMILY] | [RAIL]                             |
| :-----: | :-------------------------------------------- | :------------- | :--------------------------------- |
|   [1]   | `connect`                                     | connect        | establish a client connection      |
|   [2]   | `listen`                                      | serve          | start an SSH server                |
|   [3]   | `SSHClientConnection.run`                     | exec           | run a remote command to completion |
|   [4]   | `SSHClientConnection.create_process`          | exec           | spawn a streamed remote process    |
|   [5]   | `SSHClientConnection.start_sftp_client`       | sftp           | open an SFTP session               |
|   [6]   | `SSHClientConnection.forward_local_port`      | forward        | local→remote port forward          |
|   [7]   | `SSHClientConnection.forward_remote_port`     | forward        | remote→local port forward          |
|   [8]   | `SFTPClient.get` / `.put`                     | transfer       | download/upload files              |
|   [9]   | `SFTPClient.glob` / `.listdir`                | traverse       | remote enumeration                 |
|  [10]   | `import_private_key` / `generate_private_key` | key            | load/create a key                  |
|  [11]   | `read_known_hosts`                            | verify         | load known-hosts database          |

## [4]-[IMPLEMENTATION_LAW]

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
