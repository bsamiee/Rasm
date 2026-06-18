# [PY_RUNTIME_SERVE]

The inbound companion server-runtime, credential axis, and private daemon entrypoint. `ServerHost` owns the `grpc.aio` server lifecycle (bind, request lifecycle, graceful drain) under the `anyio` runner, hosting the geometry companion daemon that speaks the EXISTING C# `ComputeService`/`ArtifactSync` gRPC contract over the UDS/InProcess leg. `Credential` is the Python half of the C# `CredentialPolicy` axis. `Entrypoint` is the type-hint-driven `cyclopts` command grammar that launches the daemon — a PRIVATE entry only, co-located with the host it boots. The package owns the companion's inbound serve only, never the C# host lifecycle.

## [1]-[INDEX]

- `[2]-[SERVE]` — the inbound `grpc.aio` server lifecycle, the credential axis.
- `[3]-[ENTRY]` — the private companion entrypoint grammar.

## [2]-[SERVE]

- Owner: `ServerHost` — the boundary capsule over one `grpc.aio.server` hosting servicers generated from the C# proto descriptors; `Credential` the tagged union over `token`/`keyring` resolution and the `insecure_loopback` UDS leg, its `bundle` fold collapsing all three to one optional PEM string.
- Cases: `Credential` cases `token=pem` · `keyring=(service, account)` · `insecure_loopback=True` — keyword-constructed and matched by `match`/`case`; `Credential.bundle` folds all three into one `str | None` — `None` for the loopback leg, the inline PEM for `token`, the OS-keyring lookup for `keyring` — so `_listen` dispatches once on presence, binding `add_insecure_port` when absent and `ssl_server_credentials` over the resolved `(private-key, certificate-chain)` pair when present.
- Entry: `ServerHost.serve` binds the port (or UDS socket), starts the server, and awaits termination under the `anyio` runner; `ServerHost.drain` calls `Server.stop(grace)` participating in the host drain choreography so in-flight calls complete within the grace period.
- Auto: outcomes map to `grpc.StatusCode` and a domain `Error(BoundaryFault)` from `reliability/faults#FAULT` converts through `ServicerContext.abort`, never a bare exception across the wire; the generated stubs arrive from `grpcio-tools` compiling the C# `.proto` descriptors — the runtime implements servicers over them and converts to canonical shapes at the seam; `Credential.bundle` is the one credential-resolution fold the two secure cases share, reading the OS-native `keyring` backend for the keyring case and never an environment variable.
- Packages: `grpcio` (`grpc.aio.server`/`add_secure_port`/`add_insecure_port`/`ssl_server_credentials`/`StatusCode`/`ServicerContext.abort`/`Compression`), `grpcio-tools`, `protobuf`, `keyring` (`get_password`), `anyio`.
- Growth: a new servicer is one generated stub implementation; a new credential mode is one `Credential` case; zero new surface, no second RPC server.
- Boundary: the wire contract is the existing C# `ComputeService`/`ArtifactSync` proto — the runtime mints no transport, no channel, and no second wire vocabulary; a hand-rolled message loop, a divergent message shape, a bare exception across the wire, an insecure port in production, and a second RPC server are the deleted forms; the C# host lifecycle, global health, and product telemetry export stay AppHost-owned. The owner rides the sub-3.15 companion floor: the floor/lock-scope decision admits the companion environment before the `grpc.aio` server boots against the descriptors.

```python signature
from typing import Literal, assert_never

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

    def bundle(self) -> str | None:
        match self:
            case Credential(tag="insecure_loopback"):
                return None
            case Credential(tag="token", token=pem):
                return pem
            case Credential(tag="keyring", keyring=(service, account)):
                return keyring.get_password(service, account) or ""
            case _ as unreachable:
                assert_never(unreachable)


class ServerHost:
    def __init__(self, bind: str, credential: Credential, grace: float = 5.0) -> None:
        self._bind, self._credential, self._grace = bind, credential, grace
        self._server: grpc.aio.Server = grpc.aio.server(compression=grpc.Compression.Gzip)

    def _listen(self) -> None:
        match self._credential.bundle():
            case None:
                self._server.add_insecure_port(self._bind)
            case bundle:
                key_pem, cert_pem = bundle.split("\n--SEP--\n", 1)
                creds = grpc.ssl_server_credentials([(key_pem.encode(), cert_pem.encode())])
                self._server.add_secure_port(self._bind, creds)

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
        host = ServerHost(bind, Credential(insecure_loopback=True), grace)
        await host.serve()

    return app
```

## [4]-[RESEARCH]

- [COMPANION_FLOOR]: `grpcio`/`grpcio-tools`/`protobuf` ride the Forge companion lane (`forge-companion-env`, python312, `<'3.13'`), not the cp315 core manifest. The `grpc.aio.server` boot and the generated-stub servicer shape are proven against the C# `ComputeService`/`ArtifactSync` descriptors once the upstream descriptors land; the `ssl_server_credentials` and `add_secure_port` spellings confirm against the `grpcio` catalogue.
- [KEYRING_CATALOGUE]: `keyring` is the OS-native credential backend the `keyring` case resolves through; `keyring` is manifest-declared (cp315-clean) and resolves on the core, but carries no `.api/` catalogue; the `keyring.get_password(service, account)` spelling confirms against the `keyring` catalogue at capture.
- [CREDENTIAL_PEM]: the exact PEM-bundle encoding the `token`/`keyring` secret carries — whether a single concatenated key+cert blob or a structured pair — is fixed against the C# `CredentialPolicy` axis at the wire seam; the `\n--SEP--\n` split is the placeholder the seam decision replaces.
