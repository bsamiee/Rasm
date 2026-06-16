# [PY_RUNTIME_SERVER_HOST]

The inbound companion server-runtime and credential owner. `ServerHost` owns the `grpc.aio` server lifecycle (bind, request lifecycle, graceful drain) under the anyio runner, hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`ArtifactSync` gRPC contract over the remote-lane `TRANSPORT_AXIS` UDS/InProcess leg. `Credential` is the Python half of the C# `CredentialPolicy` axis. The host owns the companion's inbound serve only, never the C# host lifecycle.

## [1]-[INDEX]

| [INDEX] | [CLUSTER] | [OWNS]                                                          |
| :-----: | :-------- | :------------------------------------------------------------- |
|   [1]   | SERVE     | the inbound gRPC server lifecycle, the credential axis          |

## [2]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio.server` hosting servicers generated from the C# proto descriptors; `Credential` the tagged union over token/keyring resolution and `InsecureLoopback` for the UDS leg.
- Cases: `Credential` cases `Token(value)` · `Keyring(service, account)` · `InsecureLoopback` — matched by `match`/`case`; `InsecureLoopback` binds `add_insecure_port` for the UDS/test leg, the others bind TLS `ServerCredentials`.
- Entry: `ServerHost.serve` binds the port (or UDS socket), starts the server, and awaits termination; `ServerHost.drain` calls `Server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period.
- Auto: outcomes map to `grpc.StatusCode` and a domain `Error(BoundaryFault)` converts through `ServicerContext.abort`, never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors — the runtime implements servicers over them and converts to canonical shapes at the seam.
- Packages: `grpcio` (`grpc.aio.server`/`add_secure_port`/`add_insecure_port`/`ssl_server_credentials`/`StatusCode`/`ServicerContext.abort`/`Compression`), `grpcio-tools`, `protobuf`, `anyio`.
- Growth: a new servicer is one generated stub implementation; a new credential mode is one `Credential` case; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a bare exception across the wire, an insecure port in production, and a second RPC server are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned. This owner is `SPIKE`: the floor/lock-scope decision must admit the sub-3.15 companion environment before the `grpc.aio` server boots against the descriptors.

```python signature
from typing import Literal

import anyio
import grpc
from expression import case, tag, tagged_union


@tagged_union(frozen=True)
class Credential:
    tag: Literal["token", "keyring", "insecure_loopback"] = tag()
    token: str = case()
    keyring: tuple[str, str] = case()
    insecure_loopback: bool = case()

    @staticmethod
    def Token(value: str) -> "Credential":
        return Credential(token=value)

    @staticmethod
    def Keyring(service: str, account: str) -> "Credential":
        return Credential(keyring=(service, account))

    @staticmethod
    def InsecureLoopback() -> "Credential":
        return Credential(insecure_loopback=True)


class ServerHost:
    def __init__(self, bind: str, credential: Credential, grace: float = 5.0) -> None:
        self._bind, self._credential, self._grace = bind, credential, grace
        self._server: grpc.aio.Server = grpc.aio.server(compression=grpc.Compression.Gzip)

    def _listen(self) -> None:
        match self._credential:
            case Credential(tag="insecure_loopback"):
                self._server.add_insecure_port(self._bind)
            case Credential(tag="token", token=value):
                self._server.add_secure_port(self._bind, grpc.ssl_server_credentials([(value.encode(), value.encode())]))
            case _:
                self._server.add_secure_port(self._bind, grpc.ssl_server_credentials([]))

    async def serve(self) -> None:
        self._listen()
        await self._server.start()
        async with anyio.create_task_group() as group:
            group.start_soon(self._server.wait_for_termination)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [3]-[RESEARCH]

- [COMPANION_FLOOR]: `grpcio`/`grpcio-tools`/`protobuf` ride the `python_version<'3.13'` companion floor; under `requires-python='>=3.15'` they are pruned from the lock. The `grpc.aio.server` boot and the generated-stub servicer shape are proven against the C# `ComputeService`/`ArtifactSync` descriptors once the floor/lock-scope decision admits the sub-3.15 environment (suite TASKLOG); the `ssl_server_credentials` and `add_secure_port` spellings confirm against `.api/api-grpcio.md`.
