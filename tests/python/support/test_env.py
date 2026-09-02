"""Tests for SSH, SFTP, filesystem, and object-store test resources."""

# --- [IMPORTS] --------------------------------------------------------------------------

from typing import TYPE_CHECKING

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
    fs.makedirs(f"{root}nest/deep", exist_ok=True)
    fs.pipe_file(f"{root}nest/deep/blob.bin", b"content")
    assert fs.cat_file(f"{root}nest/deep/blob.bin") == b"content", "write/cat round-trip returned the wrong content"
    assert (fs.exists(f"{root}nest/deep/blob.bin"), fs.isdir(f"{root}nest/deep")) == (True, True), "exists/isdir disagree with the write"
    assert fs.info(f"{root}nest/deep/blob.bin")["size"] == len(b"content"), "info reported the wrong content size"
    fs.copy(f"{root}nest/deep/blob.bin", f"{root}nest/copy.bin")
    assert (fs.cat_file(f"{root}nest/copy.bin"), fs.exists(f"{root}nest/deep/blob.bin")) == (b"content", True), "copy moved instead of duplicating"
    fs.mv(f"{root}nest/copy.bin", f"{root}nest/moved.bin")
    assert (fs.exists(f"{root}nest/moved.bin"), fs.exists(f"{root}nest/copy.bin")) == (True, False), "mv left the source behind"
    assert sorted(fs.find(f"{root}nest")) == [f"{root}nest/deep/blob.bin", f"{root}nest/moved.bin"], (
        f"find returned unexpected paths: {fs.find(f'{root}nest')!r}"
    )
    fs.rm(f"{root}nest", recursive=True)
    assert not fs.exists(f"{root}nest/deep/blob.bin"), "recursive rm left content behind"


# --- [DISPATCH]


def test_provision_supports_every_environment_specification(socket_enabled: None) -> None:
    """Every environment specification provides a URL, client factory, and idempotent teardown."""
    _ = socket_enabled
    specs: tuple[EnvironmentSpec, ...] = (SshHost(), RemoteFS(), ObjectStore())
    for spec in specs:
        provisioned = provision(spec)
        assert provisioned.url, f"{type(spec).__name__} provisioned an empty url"
        assert callable(provisioned.client_factory), f"{type(spec).__name__} factory is not callable"
        assert callable(provisioned.teardown), f"{type(spec).__name__} teardown is not callable"
        provisioned.teardown()
        provisioned.teardown()


# --- [SSH_HOST]


@pytest.mark.anyio
async def test_ssh_exec_round_trip_without_tcp() -> None:
    """Default SSH exec acknowledges the exact command at exit 0 without TCP."""
    provisioned = provision(SshHost())
    conn = await provisioned.client_factory()
    try:
        done = await conn.run("echo hi", encoding=None, check=False)
        assert (done.stdout, done.exit_status) == (b"remote-ok:echo hi\n", 0), f"exec returned an unexpected result: {done.stdout!r}"
    finally:
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
        conn.close()
        await conn.wait_closed()


# --- [REMOTE_FS]


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
    """RemoteFS supports common filesystem operations but not object-store presigning."""
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


# --- [OBJECT_STORE]


def test_object_store_supports_common_filesystem_operations(socket_enabled: None) -> None:
    """The S3 resource supports the same filesystem operations as the memory implementation."""
    _ = socket_enabled
    provisioned = provision(ObjectStore())
    try:
        _assert_filesystem_operations(provisioned.client_factory(), "test-support-bucket/")
    finally:
        provisioned.teardown()


def test_object_store_teardown_resets_process_global_state(socket_enabled: None) -> None:
    """Moto state is process-global, teardown removes objects before the next provision."""
    _ = socket_enabled
    first = provision(ObjectStore())
    first.client_factory().pipe_file("test-support-bucket/residue.bin", b"stale")
    first.teardown()
    second = provision(ObjectStore())
    try:
        assert not second.client_factory().exists("test-support-bucket/residue.bin"), "teardown did not remove the prior provision's object"
    finally:
        second.teardown()


def test_object_store_round_trips_presigns_and_isolates_endpoints(socket_enabled: None) -> None:
    """Endpoints stay disjoint, put/cat/info round-trips with an e-tag, presigned GET serves the exact content over HTTP."""
    _ = socket_enabled
    first, second = provision(ObjectStore()), provision(ObjectStore(bucket="peer-bucket"))
    try:
        assert first.url != second.url, "moto endpoints collided"
        fs = first.client_factory()
        fs.pipe_file("test-support-bucket/nest/blob.bin", b"alpha")
        assert fs.cat_file("test-support-bucket/nest/blob.bin") == b"alpha", "put/cat round-trip returned the wrong content"
        info = fs.info("test-support-bucket/nest/blob.bin")
        assert info["size"] == len(b"alpha"), "info reported the wrong content size"
        assert info.get("ETag"), "object metadata did not include an e-tag"
        signed = fs.url("test-support-bucket/nest/blob.bin", expires=60)
        assert signed.startswith(first.url), f"presigned URL escaped the provisioned endpoint: {signed!r}"
        fetched = httpx.get(signed, timeout=5.0)
        assert (fetched.status_code, fetched.content) == (200, b"alpha"), "presigned GET did not serve the object"
        peer = second.client_factory()
        peer.pipe_file("peer-bucket/nest/blob.bin", b"beta")
        assert (fs.cat_file("test-support-bucket/nest/blob.bin"), peer.cat_file("peer-bucket/nest/blob.bin")) == (b"alpha", b"beta"), (
            "object-store endpoints did not remain isolated"
        )
    finally:
        first.teardown()
        first.teardown()
        second.teardown()
