"""Environment-double laws for dispatch, SSH/SFTP, bucket, and filesystem behavior."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import pytest

from tests.python._testkit.env import Bucket, provision, RemoteFS, SshHost


if TYPE_CHECKING:
    from pathlib import Path

    from tests.python._testkit.env import EnvSpec


# --- [CONSTANTS] ------------------------------------------------------------------------


def _moto_imports() -> bool:
    """Import-smoke the moto server entrypoint.

    Returns:
        True when bucket laws may run; False downgrades them to skips.
    """
    try:
        import moto.server  # noqa: F401, PLC0415  # lazy import smoke
    except Exception:  # noqa: BLE001  # import smoke: any interpreter incompatibility downgrades to skip, never errors collection
        return False
    return True


requires_moto = pytest.mark.skipif(not _moto_imports(), reason="moto[server] does not import on this interpreter")

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISPATCH]


def test_provision_is_total_over_the_spec_union() -> None:
    """Every ``EnvSpec`` variant provisions a URL, factory, and teardown."""
    specs: tuple[EnvSpec, ...] = (SshHost(), RemoteFS())
    for spec, provisioned in zip(specs, map(provision, specs), strict=True):
        assert provisioned.url, f"{type(spec).__name__} provisioned an empty url"
        assert callable(provisioned.client_factory), f"{type(spec).__name__} factory is not callable"
        assert callable(provisioned.teardown), f"{type(spec).__name__} teardown is not callable"
        provisioned.teardown()
        provisioned.teardown()  # idempotent by contract


# --- [SSH_HOST]


@pytest.mark.anyio
async def test_ssh_exec_round_trip_without_tcp() -> None:
    """Default SSH exec acknowledges the exact command at exit 0 without TCP."""
    provisioned = provision(SshHost())
    conn = await provisioned.client_factory()
    try:
        done = await conn.run("echo hi", encoding=None, check=False)
        assert (done.stdout, done.exit_status) == (b"remote-ok:echo hi\n", 0), f"exec round-trip broke: {done.stdout!r}"
    finally:
        conn.close()
        await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_handler_owns_reply_and_exit_code() -> None:
    """Custom SSH handlers own both stdout text and nonzero exit code."""
    provisioned = provision(SshHost(handler=lambda command: (f"custom:{command}", 17)))
    conn = await provisioned.client_factory()
    try:
        done = await conn.run("payload", encoding=None, check=False)
        assert (done.stdout, done.exit_status) == (b"custom:payload", 17), f"handler reply/exit lost: {done.stdout!r}/{done.exit_status}"
    finally:
        conn.close()
        await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_streaming_process_arm_round_trips() -> None:
    """The streaming SSH process shape yields bytes and exit 0."""
    import asyncssh  # noqa: PLC0415  # lazy: only the streaming law needs the DEVNULL sentinel

    provisioned = provision(SshHost())
    conn = await provisioned.client_factory()
    try:
        proc = await conn.create_process("stream me", encoding=None, stdin=asyncssh.DEVNULL)
        out = await proc.stdout.read()
        assert (out, proc.exit_status) == (b"remote-ok:stream me\n", 0), f"streaming arm broke: {out!r}"
        proc.close()
        await proc.wait_closed()
    finally:
        conn.close()
        await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_factory_yields_fresh_connections() -> None:
    """Each ``client_factory`` call opens an independent socketpair connection from one provision."""
    provisioned = provision(SshHost())
    for attempt in range(2):
        conn = await provisioned.client_factory()
        try:
            done = await conn.run(f"slot {attempt}", encoding=None, check=False)
            assert done.stdout == f"remote-ok:slot {attempt}\n".encode(), f"connection {attempt} broke: {done.stdout!r}"
        finally:
            conn.close()
            await conn.wait_closed()


@pytest.mark.anyio
async def test_ssh_sftp_chroot_serves_and_confines(tmp_path: Path) -> None:
    """``sftp_root`` confines relative reads and absolute writes to the chroot."""
    (tmp_path / "hello.txt").write_text("payload", encoding="utf-8")
    provisioned = provision(SshHost(sftp_root=tmp_path))
    conn = await provisioned.client_factory()
    try:
        async with conn.start_sftp_client() as sftp:
            assert "hello.txt" in await sftp.listdir("."), "chroot listing lost the seeded file"
            async with sftp.open("hello.txt") as handle:
                assert await handle.read() == "payload", "chroot read broke"
            async with sftp.open("/escape.txt", "w") as handle:
                await handle.write("contained")
        assert (tmp_path / "escape.txt").read_text(encoding="utf-8") == "contained", "absolute sftp path escaped the chroot"
    finally:
        conn.close()
        await conn.wait_closed()


# --- [BUCKET]


@requires_moto
def test_bucket_round_trips_through_loopback_server(socket_enabled: None) -> None:
    """Provision pre-creates the moto bucket and preserves object bytes."""
    _ = socket_enabled
    provisioned = provision(Bucket())
    try:
        assert provisioned.url.startswith("http://127.0.0.1:"), f"unexpected endpoint: {provisioned.url}"
        client = provisioned.client_factory()
        client.head_bucket(Bucket="env-bucket")
        client.put_object(Bucket="env-bucket", Key="laws/blob", Body=b"payload")
        body = client.get_object(Bucket="env-bucket", Key="laws/blob")["Body"]
        assert body.read() == b"payload", "bucket round-trip lost the payload"  # ty: ignore[unresolved-attribute]  # untyped StreamingBody
    finally:
        provisioned.teardown()


@requires_moto
def test_bucket_spec_owns_name_and_region(socket_enabled: None) -> None:
    """Non-default bucket regions drive the location-constraint creation arm."""
    _ = socket_enabled
    provisioned = provision(Bucket(name="env-regional", region="eu-west-1"))
    try:
        provisioned.client_factory().head_bucket(Bucket="env-regional")
    finally:
        provisioned.teardown()


# --- [REMOTE_FS]


def test_remote_fs_isolates_per_test_roots() -> None:
    """RemoteFS provisions isolate equal keys in disjoint memory roots."""
    first, second = provision(RemoteFS()), provision(RemoteFS())
    fs_first, fs_second = first.client_factory(), second.client_factory()
    assert first.url != second.url, "per-test roots collided"
    fs_first.pipe_file("blob.bin", b"alpha")
    fs_second.pipe_file("blob.bin", b"beta")
    assert (fs_first.cat_file("blob.bin"), fs_second.cat_file("blob.bin")) == (b"alpha", b"beta"), "cross-root bleed"
    first.teardown()
    first.teardown()  # idempotent: a second teardown of an absent root is a no-op
    assert not fs_first.exists("blob.bin"), "teardown left the first root populated"
    assert fs_second.cat_file("blob.bin") == b"beta", "teardown of one root erased its sibling"
    second.teardown()


def test_remote_fs_obeys_filesystem_algebra() -> None:
    """RemoteFS satisfies write/cat, info, cp/mv/rm, and find laws."""
    provisioned = provision(RemoteFS())
    fs = provisioned.client_factory()
    try:
        fs.makedirs("nest/deep", exist_ok=True)
        fs.pipe_file("nest/deep/blob.bin", b"payload")
        assert fs.cat_file("nest/deep/blob.bin") == b"payload", "write/cat round-trip broke"
        assert (fs.exists("nest/deep/blob.bin"), fs.isdir("nest/deep")) == (True, True), "exists/isdir disagree with the write"
        assert fs.info("nest/deep/blob.bin")["size"] == len(b"payload"), "info size drifted from the payload"
        fs.copy("nest/deep/blob.bin", "nest/copy.bin")
        assert (fs.cat_file("nest/copy.bin"), fs.exists("nest/deep/blob.bin")) == (b"payload", True), "copy moved instead of duplicating"
        fs.mv("nest/copy.bin", "nest/moved.bin")
        assert (fs.exists("nest/moved.bin"), fs.exists("nest/copy.bin")) == (True, False), "mv left the source behind"
        assert sorted(fs.find("nest")) == ["nest/deep/blob.bin", "nest/moved.bin"], f"find enumeration drifted: {fs.find('nest')!r}"
        fs.rm("nest", recursive=True)
        assert not fs.exists("nest/deep/blob.bin"), "recursive rm left content behind"
    finally:
        provisioned.teardown()
