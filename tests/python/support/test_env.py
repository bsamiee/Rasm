"""Tests for SSH, SFTP, filesystem, and object-store test resources."""

# --- [IMPORTS] --------------------------------------------------------------------------

from typing import TYPE_CHECKING

import anyio
import pytest
lazy import asyncssh
lazy import httpx

from tests.python.support.env import ObjectStore, provision, RemoteFS, SshHost

if TYPE_CHECKING:
    from pathlib import Path

    from fsspec import AbstractFileSystem

    from tests.python.support.env import EnvironmentSpec


# --- [OPERATIONS] -----------------------------------------------------------------------


def _assert_filesystem_operations(fs: AbstractFileSystem, root: str) -> None:
    """Assert shared write, read, metadata, copy, move, find, and remove behavior."""
    nest, deep, blob, copy, moved = f"{root}nest", f"{root}nest/deep", f"{root}nest/deep/blob.bin", f"{root}nest/copy.bin", f"{root}nest/moved.bin"
    fs.makedirs(deep, exist_ok=True)
    fs.pipe_file(blob, b"content")
    assert fs.cat_file(blob) == b"content", "write/cat round-trip returned the wrong content"
    assert (fs.exists(blob), fs.isdir(deep)) == (True, True), "exists/isdir disagree with the write"
    assert fs.info(blob)["size"] == len(b"content"), "info reported the wrong content size"
    fs.copy(blob, copy)
    assert (fs.cat_file(copy), fs.exists(blob)) == (b"content", True), "copy removed the source"
    fs.mv(copy, moved)
    assert (fs.exists(moved), fs.exists(copy)) == (True, False), "mv left the source behind"
    assert sorted(fs.find(nest)) == [blob, moved], f"find returned unexpected paths: {fs.find(nest)!r}"
    fs.rm(nest, recursive=True)
    assert not fs.exists(blob), "recursive rm left content behind"


# --- [DISPATCH] -------------------------------------------------------------------------


@pytest.mark.usefixtures("socket_enabled")
def test_provision_supports_every_environment_specification() -> None:
    """Every environment specification provides a URL, client factory, and idempotent teardown."""
    specs: tuple[EnvironmentSpec, ...] = (SshHost(), RemoteFS(), ObjectStore())
    for spec in specs:
        provisioned = provision(spec)
        assert provisioned.url, f"{type(spec).__name__} provisioned an empty url"
        assert callable(provisioned.client_factory), f"{type(spec).__name__} factory is not callable"
        assert callable(provisioned.teardown), f"{type(spec).__name__} teardown is not callable"
        provisioned.teardown()
        provisioned.teardown()


# --- [SSH_HOST] -------------------------------------------------------------------------


@pytest.mark.anyio
async def test_ssh_exec_round_trip_without_tcp() -> None:
    """Default SSH exec acknowledges the exact command at exit 0 without TCP."""
    provisioned = provision(SshHost())
    conn = await provisioned.client_factory()
    try:
        done = await conn.run("echo hi", encoding=None, check=False)
        assert (done.stdout, done.exit_status) == (b"remote-ok:echo hi\n", 0), f"exec returned an unexpected result: {done.stdout!r}"
    finally:
        with anyio.CancelScope(shield=True):
            conn.close()
            await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_handler_owns_reply_and_exit_code() -> None:
    """Custom SSH handlers own both stdout text and nonzero exit code."""
    provisioned = provision(SshHost(handler=lambda command: (f"custom:{command}", 17)))
    conn = await provisioned.client_factory()
    try:
        done = await conn.run("input", encoding=None, check=False)
        assert (done.stdout, done.exit_status) == (b"custom:input", 17), (
            f"handler returned an unexpected reply or exit code: {done.stdout!r}/{done.exit_status}"
        )
    finally:
        with anyio.CancelScope(shield=True):
            conn.close()
            await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_streaming_process_returns_bytes_and_exit_code() -> None:
    """The streaming SSH process returns bytes and exit code zero."""
    provisioned = provision(SshHost())
    conn = await provisioned.client_factory()
    try:
        proc = await conn.create_process("stream me", encoding=None, stdin=asyncssh.DEVNULL)
        out = await proc.stdout.read()
        assert (out, proc.exit_status) == (b"remote-ok:stream me\n", 0), f"streaming process returned {out!r} with exit code {proc.exit_status}"
        proc.close()
        await proc.wait_closed()
    finally:
        with anyio.CancelScope(shield=True):
            conn.close()
            await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_factory_yields_fresh_connections() -> None:
    """Each ``client_factory`` call opens an independent socketpair connection from a provision."""
    provisioned = provision(SshHost())
    for connection_index in range(2):
        conn = await provisioned.client_factory()
        try:
            done = await conn.run(f"connection {connection_index}", encoding=None, check=False)
            assert done.stdout == f"remote-ok:connection {connection_index}\n".encode(), f"connection {connection_index} failed: {done.stdout!r}"
        finally:
            with anyio.CancelScope(shield=True):
                conn.close()
                await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_sftp_chroot_serves_and_confines(tmp_path: Path) -> None:
    """``sftp_root`` confines relative reads and absolute writes to the chroot."""
    (tmp_path / "hello.txt").write_text("content", encoding="utf-8")
    provisioned = provision(SshHost(sftp_root=tmp_path))
    conn = await provisioned.client_factory()
    try:
        async with conn.start_sftp_client() as sftp:
            assert "hello.txt" in await sftp.listdir("."), "chroot listing omitted the seeded file"
            async with sftp.open("hello.txt") as handle:
                assert await handle.read() == "content", "chroot read returned the wrong content"
            async with sftp.open("/escape.txt", "w") as handle:
                await handle.write("contained")
        assert (tmp_path / "escape.txt").read_text(encoding="utf-8") == "contained", "absolute sftp path escaped the chroot"
    finally:
        with anyio.CancelScope(shield=True):
            conn.close()
            await conn.wait_closed()


# --- [REMOTE_FS] ------------------------------------------------------------------------


def test_remote_fs_isolates_per_test_roots() -> None:
    """RemoteFS provisions isolate equal keys in disjoint memory roots."""
    first, second = provision(RemoteFS()), provision(RemoteFS())
    fs_first, fs_second = first.client_factory(), second.client_factory()
    assert first.url != second.url, "per-test roots collided"
    fs_first.pipe_file("blob.bin", b"alpha")
    fs_second.pipe_file("blob.bin", b"beta")
    assert (fs_first.cat_file("blob.bin"), fs_second.cat_file("blob.bin")) == (b"alpha", b"beta"), "filesystem roots were not isolated"
    first.teardown()
    first.teardown()
    assert not fs_first.exists("blob.bin"), "teardown left the first root populated"
    assert fs_second.cat_file("blob.bin") == b"beta", "teardown of one root erased its sibling"
    second.teardown()


def test_remote_fs_supports_common_operations_without_presigning() -> None:
    """RemoteFS supports common filesystem operations and raises ``NotImplementedError`` for presigning."""
    provisioned = provision(RemoteFS())
    try:
        fs = provisioned.client_factory()
        _assert_filesystem_operations(fs, "")
        fs.pipe_file("blob.bin", b"content")
        match getattr(fs, "url", None):
            case None:
                pass
            case presign:
                with pytest.raises(NotImplementedError):
                    presign("blob.bin", expires=60)
    finally:
        provisioned.teardown()


# --- [OBJECT_STORE] ---------------------------------------------------------------------


@pytest.mark.usefixtures("socket_enabled")
def test_object_store_supports_common_filesystem_operations() -> None:
    """The S3 resource supports the same filesystem operations as the memory implementation."""
    store = ObjectStore()
    provisioned = provision(store)
    try:
        _assert_filesystem_operations(provisioned.client_factory(), f"{store.bucket}/")
    finally:
        provisioned.teardown()


@pytest.mark.usefixtures("socket_enabled")
def test_object_store_teardown_resets_process_global_state() -> None:
    """Moto state is process-global, teardown removes objects before the next provision."""
    store = ObjectStore()
    residue = f"{store.bucket}/residue.bin"
    first = provision(store)
    first.client_factory().pipe_file(residue, b"stale")
    first.teardown()
    second = provision(store)
    try:
        assert not second.client_factory().exists(residue), "teardown did not remove the prior provision's object"
    finally:
        second.teardown()


@pytest.mark.usefixtures("socket_enabled")
def test_object_store_round_trips_presigns_and_isolates_endpoints() -> None:
    """Endpoints stay disjoint, put/cat/info round-trips with an e-tag, presigned GET serves the exact content over HTTP."""
    store, peer_store = ObjectStore(), ObjectStore(bucket="peer-bucket")
    blob, peer_blob = f"{store.bucket}/nest/blob.bin", f"{peer_store.bucket}/nest/blob.bin"
    first, second = provision(store), provision(peer_store)
    try:
        assert first.url != second.url, "moto endpoints collided"
        fs = first.client_factory()
        fs.pipe_file(blob, b"alpha")
        assert fs.cat_file(blob) == b"alpha", "put/cat round-trip returned the wrong content"
        info = fs.info(blob)
        assert info["size"] == len(b"alpha"), "info reported the wrong content size"
        assert info.get("ETag"), "object metadata did not include an e-tag"
        signed = fs.url(blob, expires=60)
        assert signed.startswith(first.url), f"presigned URL escaped the provisioned endpoint: {signed!r}"
        fetched = httpx.get(signed)
        assert (fetched.status_code, fetched.content) == (200, b"alpha"), "presigned GET did not serve the object"
        peer = second.client_factory()
        peer.pipe_file(peer_blob, b"beta")
        assert (fs.cat_file(blob), peer.cat_file(peer_blob)) == (b"alpha", b"beta"), "object-store endpoints did not remain isolated"
    finally:
        first.teardown()
        first.teardown()
        second.teardown()
