"""Declarative environment doubles for SSH, remote filesystems, and object stores."""

# --- [IMPORTS] --------------------------------------------------------------------------

from collections.abc import Awaitable, Callable, Iterator
from contextlib import AbstractContextManager, contextmanager
import os
from pathlib import Path
import socket
from typing import assert_never, overload, override
import uuid

import anyio
import asyncssh
from fsspec import AbstractFileSystem
from fsspec.implementations.dirfs import DirFileSystem
from fsspec.implementations.memory import MemoryFileSystem
import httpx
from moto.server import ThreadedMotoServer
import msgspec
import pytest
import s3fs
import sniffio

# --- [MODELS] ---------------------------------------------------------------------------


def _echo(command: str) -> tuple[str, int]:
    """Return the default ``SshHost`` exec reply, a ``remote-ok:`` stdout line at exit 0."""
    return (f"remote-ok:{command}\n", 0)


class SshHost(msgspec.Struct, frozen=True):
    """In-process SSH exec/SFTP host over a socketpair with optional chrooted SFTP."""

    handler: Callable[[str], tuple[str, int]] = _echo
    sftp_root: Path | None = None
    user: str = "test-user"


class RemoteFS(msgspec.Struct, frozen=True):
    """Remote filesystem double scoped to a per-provision in-memory root."""


class ObjectStore(msgspec.Struct, frozen=True):
    """S3-compatible object-store double over an in-process threaded moto endpoint.

    Endpoints are per-provision but moto account state is process-global, teardown resets the backend and a later provision starts empty.
    """

    bucket: str = "test-support-bucket"
    region: str = "us-east-1"


type EnvironmentSpec = SshHost | RemoteFS | ObjectStore


class Provisioned[C](msgspec.Struct, frozen=True):
    """Provisioned test resource with its URL and client factory, the provisioning scope owns teardown."""

    url: str
    client: Callable[[], C]


# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def _ssh_host(spec: SshHost) -> Iterator[Provisioned[Awaitable[asyncssh.SSHClientConnection]]]:
    """Serve the socketpair SSH exec/SFTP host, each client connection runs beside its own server handshake."""
    key = asyncssh.generate_private_key("ssh-ed25519")

    class _Host(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            return False

    async def _exec(process: asyncssh.SSHServerProcess[str]) -> None:  # ruff:ignore[unused-async]
        text, code = spec.handler(process.command or "")
        process.stdout.write(text)
        process.exit(code)

    def _sftp(chan: asyncssh.SSHServerChannel[bytes]) -> asyncssh.SFTPServer:
        return asyncssh.SFTPServer(chan, chroot=os.fsencode(spec.sftp_root) if spec.sftp_root is not None else None)

    async def _serve(sock: socket.socket) -> None:
        await asyncssh.run_server(sock, server_factory=_Host, server_host_keys=[key], process_factory=_exec, sftp_factory=_sftp if spec.sftp_root is not None else None)

    async def _client(sock: socket.socket) -> asyncssh.SSHClientConnection:
        return await asyncssh.connect("127.0.0.1", 22, sock=sock, username=spec.user, known_hosts=None)

    async def _connect() -> asyncssh.SSHClientConnection:
        if sniffio.current_async_library() != "asyncio":
            pytest.skip("asyncssh double requires the asyncio backend")
        server_sock, client_sock = socket.socketpair()
        _, connection = await anyio.gather(_serve(server_sock), _client(client_sock))
        return connection

    yield Provisioned(url=f"ssh://{spec.user}@127.0.0.1:0", client=_connect)


@contextmanager
def _memory_filesystem() -> Iterator[Provisioned[AbstractFileSystem]]:
    """Scope an in-memory filesystem double to an isolated root removed at exit."""
    root = f"/env-fs/{uuid.uuid4().hex}"
    memory = MemoryFileSystem()
    memory.makedirs(root)
    try:
        yield Provisioned(url=f"memory://{root}", client=lambda: DirFileSystem(path=root, fs=memory))
    finally:
        memory.rm(root, recursive=True)


@contextmanager
def _object_store(spec: ObjectStore) -> Iterator[Provisioned[s3fs.S3FileSystem]]:
    """Serve a moto endpoint with the bucket created, reset the backend and stop the server at exit."""
    server = ThreadedMotoServer(ip_address="127.0.0.1", port=0, verbose=False)
    server.start()
    host, port = server.get_host_and_port()
    endpoint = f"http://{host}:{port}"

    def _store() -> s3fs.S3FileSystem:
        return s3fs.S3FileSystem(
            key="testing",
            secret="testing",  # ruff:ignore[hardcoded-password-func-arg]
            endpoint_url=endpoint,
            client_kwargs={"region_name": spec.region},
            skip_instance_cache=True,
        )

    try:
        _store().call_s3("create_bucket", Bucket=spec.bucket, **({"CreateBucketConfiguration": {"LocationConstraint": spec.region}} if spec.region != "us-east-1" else {}))
        yield Provisioned(url=endpoint, client=_store)
    finally:
        try:
            httpx.post(f"{endpoint}/moto-api/reset")
        finally:
            server.stop()


@overload
def provision(spec: SshHost) -> AbstractContextManager[Provisioned[Awaitable[asyncssh.SSHClientConnection]]]: ...
@overload
def provision(spec: RemoteFS) -> AbstractContextManager[Provisioned[AbstractFileSystem]]: ...
@overload
def provision(spec: ObjectStore) -> AbstractContextManager[Provisioned[s3fs.S3FileSystem]]: ...
def provision(
    spec: EnvironmentSpec,
) -> AbstractContextManager[Provisioned[Awaitable[asyncssh.SSHClientConnection]]] | AbstractContextManager[Provisioned[AbstractFileSystem]] | AbstractContextManager[Provisioned[s3fs.S3FileSystem]]:
    """Return the provisioning scope of the declared environment double, entered with ``with``."""
    match spec:
        case SshHost():
            return _ssh_host(spec)
        case RemoteFS():
            return _memory_filesystem()
        case ObjectStore():
            return _object_store(spec)
        case never:
            assert_never(never)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EnvironmentSpec", "ObjectStore", "Provisioned", "RemoteFS", "SshHost", "provision"]
