"""Declarative environment doubles for SSH and remote filesystems."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from collections.abc import Callable  # msgspec resolves Provisioned field annotations at runtime
import os
from pathlib import Path  # msgspec resolves SshHost field annotations at runtime
import socket
from typing import overload, override, TYPE_CHECKING
import uuid

import anyio
import msgspec


if TYPE_CHECKING:
    from collections.abc import Awaitable

    import asyncssh
    from fsspec import AbstractFileSystem


# --- [TYPES] ----------------------------------------------------------------------------

type EnvSpec = SshHost | RemoteFS


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
def provision(spec: RemoteFS) -> Provisioned[AbstractFileSystem]: ...
def provision(  # noqa: PLR0914, PLR0915  # one dispatch surface owns both provision arms; splitting fragments the closed union
    spec: EnvSpec,
) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]] | Provisioned[AbstractFileSystem]:
    """Materialize the declared environment double.

    Returns:
        Provisioned SSH connection factory or filesystem view.
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

__all__ = ["EnvSpec", "Provisioned", "RemoteFS", "SshHost", "provision"]
