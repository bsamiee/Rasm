"""Declarative environment doubles for SSH, remote filesystems, and object stores."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Callable
import os
from pathlib import Path
import socket
from typing import assert_never, overload, override, TYPE_CHECKING
import uuid

import anyio
import msgspec
import pytest
import sniffio
lazy import asyncssh
lazy from fsspec.implementations.dirfs import DirFileSystem
lazy from fsspec.implementations.memory import MemoryFileSystem
lazy import httpx
lazy from moto.server import ThreadedMotoServer
lazy import s3fs

if TYPE_CHECKING:
    from collections.abc import Awaitable

    from fsspec import AbstractFileSystem


# --- [TYPES] ----------------------------------------------------------------------------

type EnvironmentSpec = SshHost | RemoteFS | ObjectStore


# --- [MODELS] ---------------------------------------------------------------------------


def _echo(command: str) -> tuple[str, int]:
    """Return the default ``SshHost`` exec reply, a ``remote-ok:`` stdout line at exit 0."""
    return (f"remote-ok:{command}\n", 0)


class SshHost(msgspec.Struct, frozen=True, gc=False):
    """In-process SSH exec/SFTP host over a socketpair with optional chrooted SFTP."""

    handler: Callable[[str], tuple[str, int]] = _echo
    sftp_root: Path | None = None
    user: str = "test-user"


class RemoteFS(msgspec.Struct, frozen=True, gc=False):
    """Remote filesystem double scoped to a per-test in-memory root."""

    root: str = ""


class ObjectStore(msgspec.Struct, frozen=True, gc=False):
    """S3-compatible object-store double over an in-process threaded moto endpoint.

    Endpoints are per-provision but moto account state is process-global, teardown resets the backend and a later provision starts empty.
    """

    bucket: str = "test-support-bucket"
    region: str = "us-east-1"


class Provisioned[C](msgspec.Struct, frozen=True, gc=False):
    """Provisioned test resource with its URL, client factory, and idempotent teardown."""

    url: str
    client_factory: Callable[[], C]
    teardown: Callable[[], None]


# --- [OPERATIONS] -----------------------------------------------------------------------


def _provision_ssh(spec: SshHost) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]:
    """Create the socketpair SSH exec/SFTP host."""
    key = asyncssh.generate_private_key("ssh-ed25519")

    class _Host(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            _ = username
            return False

    async def _exec(process: asyncssh.SSHServerProcess[str]) -> None:  # ruff:ignore[unused-async]
        text, code = spec.handler(process.command or "")
        process.stdout.write(text)
        process.exit(code)

    def _sftp(chan: asyncssh.SSHServerChannel[bytes]) -> asyncssh.SFTPServer:
        return asyncssh.SFTPServer(chan, chroot=os.fsencode(spec.sftp_root) if spec.sftp_root is not None else None)

    async def _serve(sock: socket.socket) -> None:  # ruff:ignore[banned-api]
        await asyncssh.run_server(
            sock, server_factory=_Host, server_host_keys=[key], process_factory=_exec, sftp_factory=_sftp if spec.sftp_root is not None else None
        )

    async def _connect() -> asyncssh.SSHClientConnection:
        if sniffio.current_async_library() != "asyncio":
            pytest.skip("asyncssh double requires the asyncio backend")
        server_sock, client_sock = socket.socketpair()
        async with anyio.create_task_group() as task_group:
            _ = task_group.start_soon(_serve, server_sock)
            return await asyncssh.connect("127.0.0.1", 22, sock=client_sock, username=spec.user, known_hosts=None)
        pytest.fail("the SSH task group exited without a connection")  # The checkers read the task group exit as able to suppress the error

    return Provisioned(url=f"ssh://{spec.user}@127.0.0.1:0", client_factory=_connect, teardown=lambda: None)


def _provision_filesystem(spec: RemoteFS) -> Provisioned[AbstractFileSystem]:
    """Scope an in-memory filesystem double to an isolated root."""
    scoped = spec.root or f"/env-fs/{uuid.uuid4().hex}"
    memory = MemoryFileSystem()
    memory.makedirs(scoped, exist_ok=True)

    def _teardown() -> None:
        memory.rm(scoped, recursive=True) if memory.exists(scoped) else None

    return Provisioned(url=f"memory://{scoped}", client_factory=lambda: DirFileSystem(path=scoped, fs=MemoryFileSystem()), teardown=_teardown)


def _provision_object_store(spec: ObjectStore) -> Provisioned[s3fs.S3FileSystem]:
    """Serve a moto endpoint through ``s3fs`` with object metadata and presigned URLs."""
    server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False)
    server.start()
    host, port = server.get_host_and_port()
    endpoint = f"http://{host}:{port}"
    live: list[ThreadedMotoServer] = [server]

    def _store() -> s3fs.S3FileSystem:
        filesystem = s3fs.S3FileSystem(
            key="testing",
            secret="testing",  # ruff:ignore[hardcoded-password-func-arg]
            endpoint_url=endpoint,
            client_kwargs={"region_name": spec.region},
            skip_instance_cache=True,
        )
        constraint = {"CreateBucketConfiguration": {"LocationConstraint": spec.region}} if spec.region != "us-east-1" else {}
        filesystem.exists(spec.bucket) or filesystem.call_s3("create_bucket", Bucket=spec.bucket, **constraint)
        return filesystem

    def _stop() -> None:
        if not live:
            return
        server_handle = live.pop()
        try:
            httpx.post(f"{endpoint}/moto-api/reset", timeout=5.0)
        except httpx.HTTPError:
            server_handle.stop()
            return
        server_handle.stop()

    return Provisioned(url=endpoint, client_factory=_store, teardown=_stop)


@overload
def provision(spec: SshHost) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]]: ...
@overload
def provision(spec: RemoteFS) -> Provisioned[AbstractFileSystem]: ...
@overload
def provision(spec: ObjectStore) -> Provisioned[s3fs.S3FileSystem]: ...
def provision(
    spec: EnvironmentSpec,
) -> Provisioned[Awaitable[asyncssh.SSHClientConnection]] | Provisioned[AbstractFileSystem] | Provisioned[s3fs.S3FileSystem]:
    """Provision the declared environment double."""
    match spec:
        case SshHost():
            return _provision_ssh(spec)
        case RemoteFS():
            return _provision_filesystem(spec)
        case ObjectStore():
            return _provision_object_store(spec)
        case never:
            assert_never(never)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EnvironmentSpec", "ObjectStore", "Provisioned", "RemoteFS", "SshHost", "provision"]
