"""Environment-double laws: provision dispatch totality, SSH exec/SFTP round-trips, bucket round-trips, fs algebra.

The ``SshHost`` laws run real asyncssh handshakes over AF_UNIX socketpairs — no TCP, no ``socket_enabled``
lift — so they ride the default and mutation lanes. The ``Bucket`` laws drive a loopback moto server over
real TCP (``socket_enabled``) and skip wholesale when moto does not import on this interpreter (moto
publishes no cp315 support claim). The ``RemoteFS`` laws prove the AbstractFileSystem algebra plus per-test
root isolation over the process-global memory store.
"""

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
        True when ``moto.server`` imports cleanly; False downgrades the bucket laws to skips.
    """
    try:
        import moto.server  # noqa: F401, PLC0415  # lazy: the smoke IS the probe; moto publishes no cp315 classifier
    except Exception:  # noqa: BLE001  # import smoke: any interpreter incompatibility downgrades to skip, never errors collection
        return False
    return True


requires_moto = pytest.mark.skipif(not _moto_imports(), reason="moto[server] does not import on this interpreter")

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DISPATCH]


def test_provision_is_total_over_the_spec_union() -> None:
    """Every ``EnvSpec`` variant provisions to a non-empty url plus callable factory and teardown."""
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
    """The default handler acknowledges the exact command at exit 0 over the non-streaming ``conn.run`` arm."""
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
    """A custom handler's stdout text AND nonzero exit code both cross the exec boundary."""
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
    """``create_process(encoding=None, stdin=DEVNULL)`` — the engine's streaming shape — yields bytes and exit 0."""
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
    """``sftp_root`` serves a chroot: reads resolve inside it and absolute writes land inside it."""
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
    """Provision starts a loopback moto server with the bucket pre-created; bytes round-trip through the client."""
    _ = socket_enabled
    provisioned = provision(Bucket())
    try:
        assert provisioned.url.startswith("http://127.0.0.1:"), f"unexpected endpoint: {provisioned.url}"
        client = provisioned.client_factory()
        client.head_bucket(Bucket="env-bucket")
        client.put_object(Bucket="env-bucket", Key="laws/blob", Body=b"payload")
        body = client.get_object(Bucket="env-bucket", Key="laws/blob")["Body"]
        assert body.read() == b"payload", "bucket round-trip lost the payload"  # ty: ignore[unresolved-attribute]  # botocore StreamingBody publishes no types
    finally:
        provisioned.teardown()


@requires_moto
def test_bucket_spec_owns_name_and_region(socket_enabled: None) -> None:
    """A non-default name and a non-us-east-1 region drive the location-constraint arm of bucket creation."""
    _ = socket_enabled
    provisioned = provision(Bucket(name="env-regional", region="eu-west-1"))
    try:
        provisioned.client_factory().head_bucket(Bucket="env-regional")
    finally:
        provisioned.teardown()


# --- [REMOTE_FS]


def test_remote_fs_isolates_per_test_roots() -> None:
    """Two provisions write the same key into disjoint memory roots; teardown erases only its own root."""
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
    """The DirFileSystem view satisfies the AbstractFileSystem law battery: write/cat, info, cp/mv/rm, find."""
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
