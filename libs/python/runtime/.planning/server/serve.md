# [PY_RUNTIME_SERVE]

The inbound companion server-runtime, credential axis, and private daemon entrypoint. `ServerHost` owns the `grpc.aio` server lifecycle (bind, request lifecycle, graceful drain) under the `anyio` runner, hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`ArtifactSync` gRPC contract over the UDS/InProcess leg. `Credential` is the Python half of the C# `CredentialPolicy` axis. `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon — a PRIVATE entry only, co-located with the host it boots. The package owns the companion's inbound serve only, never the C# host lifecycle.

## [1]-[INDEX]

- `[2]-[SERVE]` — the inbound `grpc.aio` server lifecycle, the credential axis.
- `[3]-[ENTRY]` — the private companion entrypoint grammar.

## [2]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio.server` hosting servicers generated from the C# proto descriptors; `Credential` the tagged union over token/keyring resolution and `InsecureLoopback` for the UDS leg.
- Cases: `Credential` cases `Token(pem)` · `Keyring(service, account)` · `InsecureLoopback` — matched by `match`/`case`; `InsecureLoopback` binds `add_insecure_port` for the UDS/test leg, `Token` and `Keyring` resolve a `(private-key, certificate-chain)` PEM pair into `ssl_server_credentials`, `Keyring` reading the bundle through the OS-native `keyring` backend.
- Entry: `ServerHost.serve` binds the port (or UDS socket), starts the server, and awaits termination under the `anyio` runner; `ServerHost.drain` calls `Server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period.
- Auto: outcomes map to `grpc.StatusCode` and a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` converts through `ServicerContext.abort`, never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors — the runtime implements servicers over them and converts to canonical shapes at the seam; the `Token` and `Keyring` cases resolve a PEM key/cert bundle (`keyring.get_password` for the keyring case) and never read an environment variable; `_secure` is the one `ssl_server_credentials` call both secure cases share.
- Packages: `grpcio` (`grpc.aio.server`/`add_secure_port`/`add_insecure_port`/`ssl_server_credentials`/`StatusCode`/`ServicerContext.abort`/`Compression`), `grpcio-tools`, `protobuf`, `keyring` (`get_password`), `anyio`.
- Growth: a new servicer is one generated stub implementation; a new credential mode is one `Credential` case; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a bare exception across the wire, an insecure port in production, and a second RPC server are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned. The owner rides the sub-3.15 companion floor: the floor/lock-scope decision admits the companion environment before the `grpc.aio` server boots against the descriptors.

```python signature
from typing import Literal

import anyio
import grpc
import keyring
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

    def _secure(self, key_pem: str, cert_pem: str) -> None:
        self._server.add_secure_port(self._bind, grpc.ssl_server_credentials([(key_pem.encode(), cert_pem.encode())]))

    def _listen(self) -> None:
        match self._credential:
            case Credential(tag="insecure_loopback"):
                self._server.add_insecure_port(self._bind)
            case Credential(tag="token", token=pem):
                key_pem, cert_pem = pem.split("\n--SEP--\n", 1)
                self._secure(key_pem, cert_pem)
            case Credential(tag="keyring", keyring=(service, account)):
                bundle = keyring.get_password(service, account) or ""
                key_pem, cert_pem = bundle.split("\n--SEP--\n", 1)
                self._secure(key_pem, cert_pem)

    async def serve(self) -> None:
        self._listen()
        await self._server.start()
        async with anyio.create_task_group() as group:
            group.start_soon(self._server.wait_for_termination)

    async def drain(self) -> None:
        await self._server.stop(self._grace)
```

## [3]-[ENTRY]

- Owner: `Entrypoint` — the type-hint-driven `cyclopts` command axis backing the companion daemon's PRIVATE entry and package-internal entrypoints only; co-located with `ServerHost` because `serve` composes the host it launches.
- Entry: `companion_app` returns the `cyclopts.App` whose commands bind to the companion serve and drain; arguments bind from type hints, never from a hand-parsed `argv`.
- Packages: `cyclopts` (`App`/`Parameter`).
- Growth: a new private command is one `@app.command` method on the existing app; zero new surface.
- Boundary: never a new public command surface — public commands are reserved to the suite Assay command surface; a public-facing CLI axis and a hand-parsed argument loop are the deleted forms.

```python signature
from cyclopts import App


def companion_app() -> App:
    app = App(name="rasm-companion", help="private companion daemon entry")

    @app.command
    async def serve(bind: str, *, grace: float = 5.0) -> None:
        host = ServerHost(bind, Credential.InsecureLoopback(), grace)
        await host.serve()

    return app
```

## [4]-[RESEARCH]

- [COMPANION_FLOOR]: `grpcio`/`grpcio-tools`/`protobuf` ride the sub-3.15 companion floor; under the suite `requires-python` ceiling they are pruned from the lock until the floor/lock-scope decision admits the companion environment. The `grpc.aio.server` boot and the generated-stub servicer shape are proven against the C# `ComputeService`/`ArtifactSync` descriptors once the companion environment is admitted; the `ssl_server_credentials` and `add_secure_port` spellings confirm against the `grpcio` catalogue.
- [KEYRING_ADMISSION]: `keyring` is the OS-native credential backend the `Keyring` case resolves through; admission of `keyring` to the manifest lands at the credential-backend task, and the `get_password` spelling confirms at fence transcription.
- [CREDENTIAL_PEM]: the exact PEM-bundle encoding the `Token`/`Keyring` secret carries — whether a single concatenated key+cert blob or a structured pair — is fixed against the C# `CredentialPolicy` axis once the companion environment is admitted; the `\n--SEP--\n` split is the placeholder the seam decision replaces.
