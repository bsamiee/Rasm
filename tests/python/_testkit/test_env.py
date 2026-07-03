"""Environment-double laws for dispatch, SSH/SFTP, filesystem, object-store, and loopback behavior."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from typing import TYPE_CHECKING

import pytest
lazy import asyncssh
lazy import grpc

from tests.python._testkit.env import ObjectStore, provision, RemoteFS, SshHost
from tests.python._testkit.seams import grpc_loopback


if TYPE_CHECKING:
    from pathlib import Path

    from fsspec import AbstractFileSystem

    from tests.python._testkit.env import EnvSpec


# --- [OPERATIONS] -----------------------------------------------------------------------


def _fs_algebra(fs: AbstractFileSystem, root: str) -> None:
    """One filesystem algebra every provisioned backend satisfies: write/cat, info, cp, mv, find, rm."""
    fs.makedirs(f"{root}nest/deep", exist_ok=True)
    fs.pipe_file(f"{root}nest/deep/blob.bin", b"payload")
    assert fs.cat_file(f"{root}nest/deep/blob.bin") == b"payload", "write/cat round-trip broke"
    assert (fs.exists(f"{root}nest/deep/blob.bin"), fs.isdir(f"{root}nest/deep")) == (True, True), "exists/isdir disagree with the write"
    assert fs.info(f"{root}nest/deep/blob.bin")["size"] == len(b"payload"), "info size drifted from the payload"
    fs.copy(f"{root}nest/deep/blob.bin", f"{root}nest/copy.bin")
    assert (fs.cat_file(f"{root}nest/copy.bin"), fs.exists(f"{root}nest/deep/blob.bin")) == (b"payload", True), "copy moved instead of duplicating"
    fs.mv(f"{root}nest/copy.bin", f"{root}nest/moved.bin")
    assert (fs.exists(f"{root}nest/moved.bin"), fs.exists(f"{root}nest/copy.bin")) == (True, False), "mv left the source behind"
    assert sorted(fs.find(f"{root}nest")) == [f"{root}nest/deep/blob.bin", f"{root}nest/moved.bin"], f"find drifted: {fs.find(f'{root}nest')!r}"
    fs.rm(f"{root}nest", recursive=True)
    assert not fs.exists(f"{root}nest/deep/blob.bin"), "recursive rm left content behind"


# --- [DISPATCH]


def test_provision_is_total_over_the_spec_union(socket_enabled: None) -> None:
    """Every ``EnvSpec`` variant provisions a URL, factory, and idempotent teardown."""
    _ = socket_enabled  # ObjectStore binds a real loopback endpoint at provision time
    specs: tuple[EnvSpec, ...] = (SshHost(), RemoteFS(), ObjectStore())
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
    """RemoteFS satisfies the shared filesystem algebra over its scoped memory root."""
    provisioned = provision(RemoteFS())
    try:
        _fs_algebra(provisioned.client_factory(), "")
    finally:
        provisioned.teardown()


# --- [OBJECT_STORE]


def test_object_store_obeys_the_same_filesystem_algebra(socket_enabled: None) -> None:
    """The S3 double satisfies the identical algebra the memory backend does, rooted at its bucket."""
    _ = socket_enabled
    provisioned = provision(ObjectStore())
    try:
        _fs_algebra(provisioned.client_factory(), "kit-bucket/")
    finally:
        provisioned.teardown()


def test_object_store_teardown_resets_process_global_state(socket_enabled: None) -> None:
    """Moto state is process-global: teardown resets it, so a later provision never sees residue keys."""
    _ = socket_enabled
    first = provision(ObjectStore())
    first.client_factory().pipe_file("kit-bucket/residue.bin", b"stale")
    first.teardown()
    second = provision(ObjectStore())
    try:
        assert not second.client_factory().exists("kit-bucket/residue.bin"), "prior provision's key leaked through the shared moto backend"
    finally:
        second.teardown()


def test_object_store_round_trips_and_isolates_endpoints(socket_enabled: None) -> None:
    """Two ObjectStore provisions serve disjoint endpoints; put/cat/info round-trips with an e-tag."""
    _ = socket_enabled
    first, second = provision(ObjectStore()), provision(ObjectStore(bucket="peer-bucket"))
    try:
        assert first.url != second.url, "moto endpoints collided"
        fs = first.client_factory()
        fs.pipe_file("kit-bucket/nest/blob.bin", b"alpha")
        assert fs.cat_file("kit-bucket/nest/blob.bin") == b"alpha", "put/cat round-trip broke"
        info = fs.info("kit-bucket/nest/blob.bin")
        assert info["size"] == len(b"alpha"), "info size drifted from the payload"
        assert info.get("ETag"), "object store lost the e-tag the egress content-key contract reads"
        peer = second.client_factory()
        peer.pipe_file("peer-bucket/nest/blob.bin", b"beta")
        assert (fs.cat_file("kit-bucket/nest/blob.bin"), peer.cat_file("peer-bucket/nest/blob.bin")) == (b"alpha", b"beta"), "cross-endpoint bleed"
    finally:
        first.teardown()
        first.teardown()  # idempotent: a second stop of a dead server is a no-op
        second.teardown()


# --- [GRPC_LOOPBACK]


@pytest.mark.anyio
async def test_grpc_loopback_serves_generic_unary_and_tears_down(socket_enabled: None) -> None:
    """The grpc.aio capsule binds an ephemeral port and serves a raw-bytes unary handler."""
    _ = socket_enabled

    async def _reverse(request: bytes, context: object) -> bytes:  # noqa: RUF029  # no await; grpc.aio drives the handler coroutine
        _ = context
        return bytes(reversed(request))

    def _bind(server: grpc.aio.Server) -> None:
        server.add_generic_rpc_handlers((
            grpc.method_handlers_generic_handler("kit.Echo", {"Reverse": grpc.unary_unary_rpc_method_handler(_reverse)}),
        ))

    async with grpc_loopback(_bind) as (endpoint, channel):
        assert endpoint.port > 0, "capsule failed to bind an ephemeral loopback port"
        reply: bytes = await channel.unary_unary("/kit.Echo/Reverse")(b"abc")
        assert reply == b"cba", f"unary echo broke: {reply!r}"
