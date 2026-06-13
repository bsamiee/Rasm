"""Declarative environment doubles: provision an SSH host, an object bucket, or a remote filesystem by spec.

The closed ``EnvSpec`` union declares WHAT environment a test needs; ``provision`` dispatches structurally
on the variant and returns a ``Provisioned`` capsule — connect ``url``, a ``client_factory``, an idempotent
``teardown``. ``SshHost`` answers real asyncssh exec/SFTP traffic over a ``socket.socketpair`` created inside
whichever event loop awaits the factory — no TCP, no ``socket_enabled`` lift, so its laws ride the default
and mutation lanes and the factory is drop-in compatible with an engine's own loop. ``Bucket`` starts a
loopback ``ThreadedMotoServer`` (real TCP — callers hold ``socket_enabled``). ``RemoteFS`` scopes an fsspec
``DirFileSystem`` root inside the process-global ``MemoryFileSystem`` so parallel tests never collide.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # Provisioned msgspec fields are runtime-annotated
import os
from pathlib import Path  # SshHost msgspec field type requires runtime presence
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
    """Structural subset of an S3-compatible client the bucket laws drive (AWS wire casing preserved)."""

    def create_bucket(self, *, Bucket: str, **config: object) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def head_bucket(self, *, Bucket: str) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def put_object(self, *, Bucket: str, Key: str, Body: bytes) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing
    def get_object(self, *, Bucket: str, Key: str) -> Mapping[str, object]: ...  # noqa: N803  # AWS wire casing


# --- [MODELS] ---------------------------------------------------------------------------


def _echo(command: str) -> tuple[str, int]:
    """Default ``SshHost`` exec reply.

    Returns:
        The exact command line acknowledged as one ``remote-ok:``-prefixed stdout line at exit 0.
    """
    return (f"remote-ok:{command}\n", 0)


class SshHost(msgspec.Struct, frozen=True, gc=False):
    """An in-process SSH exec/SFTP host over a socketpair — no TCP, default-lane and mutation-eligible.

    ``handler`` maps the requested command line to ``(stdout_text, exit_code)``; ``sftp_root`` additionally
    serves a chroot-confined SFTP subsystem rooted there.
    """

    handler: Callable[[str], tuple[str, int]] = _echo
    sftp_root: Path | None = None
    user: str = "x"


class Bucket(msgspec.Struct, frozen=True, gc=False):
    """A loopback moto S3 bucket: the server is started and the bucket created at provision time."""

    name: str = "env-bucket"
    region: str = "us-east-1"


class RemoteFS(msgspec.Struct, frozen=True, gc=False):
    """A remote-filesystem double: a ``DirFileSystem`` view over a per-test memory root (fresh uuid when empty)."""

    root: str = ""


class Provisioned[C](msgspec.Struct, frozen=True, gc=False):
    """A provisioned environment double: connect ``url``, per-call ``client_factory``, idempotent ``teardown``."""

    url: str
    client_factory: Callable[[], C]
    teardown: Callable[[], None]


# --- [OPERATIONS] -------------------------------------------------------------------------


@overload
def provision(spec: SshHost) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]: ...
@overload
def provision(spec: Bucket) -> Provisioned[ObjectClient]: ...
@overload
def provision(spec: RemoteFS) -> Provisioned[AbstractFileSystem]: ...
def provision(  # noqa: PLR0914, PLR0915  # one dispatch surface owns all three provision arms; splitting fragments the closed union
    spec: EnvSpec,
) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]] | Provisioned[ObjectClient] | Provisioned[AbstractFileSystem]:
    """Materialize the declared environment double and return its ``Provisioned`` capsule.

    Dispatch is structural on the closed ``EnvSpec`` union. ``SshHost`` defers ALL I/O to the factory so the
    handshake runs inside the awaiting loop; ``Bucket`` starts its loopback server and pre-creates the bucket
    here; ``RemoteFS`` claims its memory root here.

    Returns:
        ``Provisioned`` whose factory yields an awaited ``SSHClientConnection``, an S3-compatible client,
        or an ``AbstractFileSystem`` respectively.
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

            async def _serve(sock: socket.socket) -> None:  # noqa: TID251  # type reference only: the pair half asyncssh adopts; no raw socket I/O here
                # run_server blocks on auth completion; its result (the server-side connection) needs no
                # handle — the server end closes itself when the client disconnects (EOF on its pair half).
                await asyncssh.run_server(
                    sock, server_factory=_Host, server_host_keys=[key], process_factory=_exec, sftp_factory=_sftp if sftp_root is not None else None
                )

            async def _connect() -> asyncssh.SSHClientConnection:
                server_sock, client_sock = socket.socketpair()
                # Both handshake halves block on auth completion, so they must be driven concurrently in THIS
                # loop. The host label on connect skips asyncssh's AF_UNIX peername unpack; known_hosts=None
                # skips host-key validation against the generated key.
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

                # moto ignores credentials; static values keep ambient AWS env/config out of the double.
                # boto3 ships no py.typed (mypy override); structural narrowing is the typed admission.
                client = boto3.client("s3", endpoint_url=url, region_name=region, aws_access_key_id="env-double", aws_secret_access_key="env-double")  # noqa: S106  # not a secret: moto double placeholder
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
