"""Declarative environment doubles for SSH, object storage, and remote filesystems."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # msgspec resolves Provisioned field annotations at runtime
import os
from pathlib import Path  # msgspec resolves SshHost field annotations at runtime
import socket
from typing import overload, override, Protocol, runtime_checkable, TYPE_CHECKING
import uuid

import anyio
import msgspec


if TYPE_CHECKING:
    from collections.abc import Awaitable, Mapping

    import asyncssh
    from fsspec import AbstractFileSystem


# --- [TYPES] ----------------------------------------------------------------------------

type EnvSpec = SshHost | Bucket | RemoteFS


@runtime_checkable
class ObjectClient(Protocol):
    """S3-compatible client subset driven by bucket laws."""

    def create_bucket(self, *, Bucket: str, **config: object) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def head_bucket(self, *, Bucket: str) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def put_object(self, *, Bucket: str, Key: str, Body: bytes) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def get_object(self, *, Bucket: str, Key: str) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing


# --- [MODELS] ---------------------------------------------------------------------------


def _echo(command: str) -> tuple[str, int]:
    """Build the default ``SshHost`` exec reply.

    Returns:
        ``remote-ok:`` stdout line plus exit status 0.
    """
    return (f"remote-ok:{command}\n", 0)


class SshHost(msgspec.Struct, frozen=True, gc=False):
    """In-process SSH exec/SFTP host over a socketpair with optional chrooted SFTP."""

    handler: Callable[[str], tuple[str, int]] = _echo
    sftp_root: Path | None = None
    user: str = "x"


class Bucket(msgspec.Struct, frozen=True, gc=False):
    """Loopback moto S3 bucket created at provision time."""

    name: str = "env-bucket"
    region: str = "us-east-1"


class RemoteFS(msgspec.Struct, frozen=True, gc=False):
    """Remote filesystem double scoped to a per-test in-memory root."""

    root: str = ""


class Provisioned[C](msgspec.Struct, frozen=True, gc=False):
    """Provisioned double capsule with connection URL, factory, and idempotent teardown."""

    url: str
    client_factory: Callable[[], C]
    teardown: Callable[[], None]


# --- [OPERATIONS] -----------------------------------------------------------------------


@overload
def provision(spec: SshHost) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]: ...
@overload
def provision(spec: Bucket) -> Provisioned[ObjectClient]: ...
@overload
def provision(spec: RemoteFS) -> Provisioned[AbstractFileSystem]: ...
def provision(  # noqa: PLR0914, PLR0915  # one dispatch surface owns all three provision arms; splitting fragments the closed union
    spec: EnvSpec,
) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]] | Provisioned[ObjectClient] | Provisioned[AbstractFileSystem]:
    """Materialize the declared environment double.

    Returns:
        Provisioned SSH connection factory, S3-compatible client, or filesystem view.
    """
    match spec:
        case SshHost(handler=handler, sftp_root=sftp_root, user=user):
            import asyncssh  # noqa: PLC0415  # lazy: the ~194ms import tax is paid only by suites that provision an SSH double

            key = asyncssh.generate_private_key("ssh-ed25519")

            class _Host(asyncssh.SSHServer):
                @override
                def begin_auth(self, username: str) -> bool:
                    _ = username
                    return False

            async def _exec(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029  # no await; asyncssh drives the handler synchronously
                text, code = handler(process.command or "")
                process.stdout.write(text)
                process.exit(code)

            def _sftp(chan: asyncssh.SSHServerChannel[bytes]) -> asyncssh.SFTPServer:
                return asyncssh.SFTPServer(chan, chroot=os.fsencode(sftp_root) if sftp_root is not None else None)

            async def _serve(sock: socket.socket) -> None:  # noqa: TID251  # asyncssh adopts the pair half; no raw socket I/O
                # No retained handle: asyncssh closes the server side when the client half reaches EOF.
                await asyncssh.run_server(
                    sock, server_factory=_Host, server_host_keys=[key], process_factory=_exec, sftp_factory=_sftp if sftp_root is not None else None
                )

            async def _connect() -> asyncssh.SSHClientConnection:
                server_sock, client_sock = socket.socketpair()
                # Both handshake halves block on auth, so the awaiting loop must drive them concurrently.
                async with anyio.create_task_group() as tg:
                    tg.start_soon(_serve, server_sock)
                    client = await asyncssh.connect("127.0.0.1", 22, sock=client_sock, username=user, known_hosts=None)
                return client

            return Provisioned(url=f"ssh://{user}@127.0.0.1:0", client_factory=_connect, teardown=lambda: None)
        case Bucket(name=name, region=region):
            from moto.server import ThreadedMotoServer  # noqa: PLC0415  # lazy: moto/werkzeug import tax paid only by bucket suites

            server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False)
            server.start()
            host, port = server.get_host_and_port()
            url = f"http://{host}:{port}"

            def _client() -> ObjectClient:
                import boto3  # noqa: PLC0415  # lazy: boto3 rides moto[server]'s dependency closure

                # Static credentials isolate the moto double from ambient AWS config.
                client = boto3.client(
                    "s3",
                    endpoint_url=url,
                    region_name=region,
                    aws_access_key_id="env-double",
                    aws_secret_access_key="env-double",  # noqa: S106  # moto placeholder
                )
                match client:
                    case ObjectClient():
                        return client
                    case _:
                        msg = "boto3 S3 client no longer satisfies the ObjectClient protocol"
                        raise TypeError(msg)

            match region:
                case "us-east-1":
                    _client().create_bucket(Bucket=name)
                case _:
                    _client().create_bucket(Bucket=name, CreateBucketConfiguration={"LocationConstraint": region})
            return Provisioned(url=url, client_factory=_client, teardown=server.stop)
        case RemoteFS(root=root):
            from fsspec.implementations.dirfs import DirFileSystem  # noqa: PLC0415  # lazy: keep fsspec impl imports off non-fs suites
            from fsspec.implementations.memory import MemoryFileSystem  # noqa: PLC0415  # lazy: keep fsspec impl imports off non-fs suites

            scoped = root or f"/env-fs/{uuid.uuid4().hex}"
            memory = MemoryFileSystem()
            memory.makedirs(scoped, exist_ok=True)

            def _teardown() -> None:
                memory.rm(scoped, recursive=True) if memory.exists(scoped) else None

            return Provisioned(url=f"memory://{scoped}", client_factory=lambda: DirFileSystem(path=scoped, fs=MemoryFileSystem()), teardown=_teardown)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bucket", "EnvSpec", "ObjectClient", "Provisioned", "RemoteFS", "SshHost", "provision"]
