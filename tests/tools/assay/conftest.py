"""Typed fixture surface for assay Phase-2 A/B + isolation laws.

Mirrors ``tests/tools/quality/conftest.py`` (``QualityHarness``/``RailProbe``/``PackageShape``):
one tmp-tree settings capsule, one canned-receipt monkeypatch probe, one loopback ssh seam, one
``_BridgeResult`` writer, one fake-yak/fake-msbuild materializer, and one A/B delta projector that
decodes the single stdout Envelope each operator emits.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

import asyncio  # noqa: TID251  # the loopback ssh server needs a persistent run_forever loop on a daemon thread; anyio exposes no such surface, and asyncssh is asyncio-native
from dataclasses import dataclass, field
from pathlib import Path
import threading
from typing import override, TYPE_CHECKING

import asyncssh
from expression import Ok
import msgspec
import pytest
from upath import UPath

from tools.assay._TMP.composition.settings import (  # noqa: PLC2701  # _TMP is the staging package root, mirrored from the source convention
    ArtifactScope,
    AssaySettings,
)
from tools.assay._TMP.core.model import Claim, Envelope, envelope as wrap_envelope, receipt  # noqa: PLC2701  # _TMP staging package root
from tools.assay._TMP.core.status import RailStatus  # noqa: PLC2701  # _TMP staging package root
from tools.assay._TMP.rails import bridge as bridge_rail, package as package_rail  # noqa: PLC2701  # _TMP staging package root
from tools.quality.rails import package as quality_package
from tools.quality.settings import ArtifactScope as QualityScope, QualitySettings


if TYPE_CHECKING:
    from collections.abc import Callable, Generator, Mapping

    from expression import Result

    from tools.assay._TMP.core.model import Completed, Fault, Report  # _TMP staging package root


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class AssayHarness:
    """The isolated tmp-tree capsule: every assay I/O path roots under ``<root>/.artifacts/assay``.

    ``settings`` carries ``exec_target=""``/``exec_known_hosts=None`` (point-and-go local) so a bare
    rail mutates nothing outside ``root``; ``remote`` re-derives the same settings against an ssh
    ``exec_target`` for the one network-marked law.
    """

    root: Path
    settings: AssaySettings

    def write(self, rel: str | Path, text: str = "") -> Path:
        path = self.root / Path(rel)
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text(text, encoding="utf-8")
        return path

    def scope(self, claim: Claim = Claim.PACKAGE) -> ArtifactScope:
        return ArtifactScope.open(self.settings, claim)

    def remote(self, exec_target: str) -> AssaySettings:
        return self.settings.model_copy(update={"exec_target": exec_target, "exec_known_hosts": None})

    @staticmethod
    def envelope_of(payload: Report | Fault, *, claim: Claim, verb: str) -> Envelope:
        return wrap_envelope(payload, claim=claim, verb=verb)


@dataclass(frozen=True, slots=True)
class RailProbe:
    """Socket-free mock host: monkeypatch the rail-module ``run_check``/``fan_out`` to a canned receipt.

    Rails bind ``from ...engine import run_check`` as a module attribute, so the patch target is the
    OWNER rail module (``bridge_rail``/``package_rail``/...), never ``core.engine`` — mirrors the
    legacy ``RailProbe.install``. ``calls`` records every interception for assertion.
    """

    calls: list[tuple[str, tuple[object, ...], dict[str, object]]] = field(default_factory=list)

    def install(self, monkeypatch: pytest.MonkeyPatch, owner: object, member: str, payload: Result[object, Fault]) -> None:
        def replacement(*args: object, **kwargs: object) -> Result[object, Fault]:
            self.calls.append((member, args, kwargs))
            return payload

        monkeypatch.setattr(owner, member, replacement)

    @staticmethod
    def ok(argv: tuple[str, ...] = ("rasm-bridge", "check"), status: RailStatus = RailStatus.OK) -> Result[Completed, Fault]:
        return Ok(receipt(argv, 0, status=status))


@dataclass(frozen=True, slots=True)
class SshLoopback:
    """A live loopback asyncssh server on a dedicated thread-owned event loop.

    The engine's ``_run_remote`` opens its own ``anyio.run`` loop per spawn, so the server cannot
    share it: it runs ``run_forever`` on a daemon thread whose loop survives across the engine's
    transient loops. ``exec_target`` carries a username (``ssh://x@…``) because asyncssh ``saslprep``
    rejects a ``None`` username at connect. Teardown closes the server, drains ``wait_closed`` on the
    server loop, stops the loop, joins the thread, and closes the loop so no ``ResourceWarning`` leaks
    under ``filterwarnings=["error"]``.
    """

    port: int

    @property
    def exec_target(self) -> str:
        return f"ssh://x@127.0.0.1:{self.port}"


@dataclass(frozen=True, slots=True)
class AbDelta:
    """The structural A/B delta: both decoded operator outputs plus the field-name correspondence.

    ``assay`` is the new operator's one-Envelope wire (``report``/``error_context`` rails); ``quality``
    is the predecessor's rail payload (``query``/``packages`` keys). ``mapping`` is the canonical
    ``rail↔claim``/``data↔report``/``evidence↔error_context`` correspondence the migration preserves.
    """

    assay: Envelope
    quality: dict[str, object]
    mapping: Mapping[str, str]


@dataclass(frozen=True, slots=True)
class BridgeResult:
    """A ``_BridgeResult`` JSON writer: one valid camelCase payload plus three adversarial variants.

    The rail decodes ``--result`` JSON with ``strict=False`` and degrades a malformed/partial/missing
    file to a ``FAILED`` row, so the variants exercise the rail's defensive decode boundary.
    """

    directory: Path

    def valid(self, command: str = "scenario.verify.csx") -> Path:
        payload = {
            "command": command,
            "status": "ok",
            "phases": [
                {
                    "phase": "execute",
                    "status": "ok",
                    "data": {"diagnostics": {"commandWindow": [], "exceptionReports": []}},
                    "outputs": [{"source": "stdout", "text": "rasm.rhino-bridge.evidence=facts={}"}],
                }
            ],
        }
        return self._write("valid.json", msgspec.json.encode(payload))

    def malformed(self) -> Path:
        return self._write("malformed.json", b"{not json")

    def partial(self) -> Path:
        return self._write("partial.json", b"{}")

    def missing(self) -> Path:
        return self.directory / "absent.json"

    def _write(self, name: str, raw: bytes) -> Path:
        path = self.directory / name
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_bytes(raw)
        return path


@dataclass(frozen=True, slots=True)
class YakShape:
    """Port of ``PackageShape``: materializes an executable fake-yak + fake-msbuild env over a temp tree.

    ``materialize`` lays a ``YakPackageSlug`` project, a manifest, the primary ``.rhp`` + ``.dll`` under
    ``bin/<config>/<tfm>``, and an executable ``yak`` script so ``package.evaluate_meta`` resolves a
    valid ``YakMeta`` and the stage path runs ``yak build`` against the temp tree. ``msbuild_props``
    answers the ``-getProperty`` envelope with every ``_META_PROPS`` member non-empty except
    ``YakPushSource``.
    """

    slug: str = "rasm-bridge"
    project: Path = Path("apps/bridge/plugin.csproj")
    assembly_name: str = "Rasm"
    target_ext: str = ".rhp"
    target_framework: str = "net10.0"
    package_pattern: str = "rasm-rh9_1-mac.yak"

    def props(self, meta: package_rail.YakMeta) -> dict[str, str]:
        return {
            "AssemblyName": meta.assembly_name,
            "MSBuildProjectDirectory": str(meta.project_dir),
            "TargetDir": str(meta.target_dir),
            "TargetExt": meta.target_ext,
            "TargetFramework": meta.target_framework,
            "YakManifestDirectory": str(meta.manifest_dir),
            "YakPackageDirectory": str(meta.package_dir),
            "YakPackagePattern": meta.package_pattern,
            "YakPackageSlug": self.slug,
            "YakPath": str(meta.yak_path),
            "YakPlatform": meta.yak_platform,
            "YakPushSource": meta.yak_push_source,
        }

    def materialize(self, harness: AssayHarness) -> package_rail.YakMeta:
        yak = harness.write("yak", "#!/bin/sh\nexit 0\n")
        yak.chmod(0o755)
        project = harness.write(self.project, f"<Project><PropertyGroup><YakPackageSlug>{self.slug}</YakPackageSlug></PropertyGroup></Project>")
        target = project.parent / "bin" / harness.settings.configuration.value / self.target_framework
        target.mkdir(parents=True, exist_ok=True)
        harness.write(project.parent.relative_to(harness.root) / "manifest.yml", f"name: {self.slug}\n")
        harness.write(target.relative_to(harness.root) / f"{self.assembly_name}{self.target_ext}")
        harness.write(target.relative_to(harness.root) / f"{self.assembly_name}.dll")
        return package_rail.YakMeta(
            project=str(project.relative_to(harness.root)),
            manifest_dir=project.parent,
            target_dir=target,
            assembly_name=self.assembly_name,
            target_ext=self.target_ext,
            yak_path=yak,
            yak_platform="mac",
            yak_push_source="",
            package_dir=project.parent / "yak",
            package_pattern=self.package_pattern,
            target_framework=self.target_framework,
            project_dir=project.parent,
        )


# --- [OPERATIONS] ----------------------------------------------------------------------


def _serve_loopback(reply: str) -> tuple[asyncio.AbstractEventLoop, threading.Thread, asyncssh.SSHAcceptor, int]:
    """Stand a loopback asyncssh server on a dedicated thread-owned loop; return ``(loop, thread, acceptor, port)``.

    The handler ignores the resolved remote command (``cd … && argv``) and replies a fixed line so the
    engine's ``_run_remote`` round-trip is deterministic; ``begin_auth -> False`` admits the no-key
    ephemeral connect the engine opens with ``known_hosts=None``.
    """
    loop = asyncio.new_event_loop()
    ready = threading.Event()
    box: dict[str, object] = {}

    class _Server(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:  # asyncssh SSHServer override contract: instance method, admits the no-key ephemeral connect
            return False

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029  # asyncssh process_factory contract requires a coroutine even when the body never awaits
        process.stdout.write(f"{reply}\n")
        process.exit(0)

    def _run() -> None:
        asyncio.set_event_loop(loop)

        async def _boot() -> None:
            key = asyncssh.generate_private_key("ssh-ed25519")
            box["acceptor"] = await asyncssh.create_server(_Server, "127.0.0.1", 0, server_host_keys=[key], process_factory=_handler)
            ready.set()

        loop.run_until_complete(_boot())
        loop.run_forever()

    thread = threading.Thread(target=_run, daemon=True)
    thread.start()
    ready.wait(timeout=10)
    acceptor = box["acceptor"]
    assert isinstance(acceptor, asyncssh.SSHAcceptor)
    return loop, thread, acceptor, acceptor.get_port()


def _teardown_loopback(loop: asyncio.AbstractEventLoop, thread: threading.Thread, acceptor: asyncssh.SSHAcceptor) -> None:
    """Close the server + drain its loop with no ``ResourceWarning`` (which ``filterwarnings=["error"]`` would fail)."""

    async def _shut() -> None:
        acceptor.close()
        await acceptor.wait_closed()

    asyncio.run_coroutine_threadsafe(_shut(), loop).result(timeout=10)
    loop.call_soon_threadsafe(loop.stop)
    thread.join(timeout=10)
    loop.close()


# --- [COMPOSITION] ---------------------------------------------------------------------


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """An isolated tmp-tree ``AssaySettings`` — all I/O redirects under ``<tmp>/.artifacts/assay``."""
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), exec_target="", exec_known_hosts=None)
    return AssayHarness(tmp_path, settings)


@pytest.fixture
def envelope() -> Callable[[pytest.CaptureFixture[str]], Envelope]:
    """Decode the single stdout Envelope line the CLI ``_emit`` writes; structlog rides stderr."""

    def decode(capsys: pytest.CaptureFixture[str]) -> Envelope:
        captured = capsys.readouterr()
        rows = captured.out.splitlines()
        assert len(rows) == 1, f"expected exactly one stdout Envelope line, got {len(rows)}"
        return msgspec.json.decode(rows[0].encode(), type=Envelope)

    return decode


@pytest.fixture
def rail_probe() -> RailProbe:
    """The socket-free mock host: install canned ``Completed`` receipts on a rail's ``run_check``/``fan_out``."""
    return RailProbe()


@pytest.fixture
def ssh_loopback(socket_enabled: None) -> Generator[SshLoopback]:
    """A live loopback asyncssh server (network-marked usage): yields the ssh ``exec_target`` capsule.

    Depends on ``socket_enabled`` so the server boot ALWAYS runs with ``--disable-socket`` lifted (the
    boot ``create_server`` binds a TCP listener); consumers still carry ``@pytest.mark.network`` to
    classify the law. The lift is request-scoped, so the suite's default socket block stands everywhere else.

    Yields:
        ``SshLoopback`` carrying the bound port and the ``ssh://x@127.0.0.1:<port>`` ``exec_target``.
    """
    _ = socket_enabled
    loop, thread, acceptor, port = _serve_loopback("remote-ok")
    yield SshLoopback(port=port)
    _teardown_loopback(loop, thread, acceptor)


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """Write a ``_BridgeResult`` JSON (valid + malformed/partial/missing) the bridge rail decodes defensively."""
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """The fake-yak + fake-msbuild materializer (port of ``PackageShape``)."""
    return YakShape()


@pytest.fixture
def ab_diff(assay_root: AssayHarness) -> Callable[[Claim, str], AbDelta]:
    """Run a read-only ``_TMP`` rail and the matching ``tools.quality`` rail; decode both + the field delta.

    Both operators run in-process under isolated settings (zero host mutation): the ``_TMP`` rail folds a
    ``Result[Report, Fault]`` wrapped to one ``Envelope`` (``report``/``error_context`` rails), and the
    quality rail payload decodes to its ``query``/``packages`` dict. ``mapping`` records the canonical
    ``rail↔claim``/``data↔report``/``evidence↔error_context`` correspondence the migration preserves.
    """
    mapping = {"rail": "claim", "data": "report", "evidence": "error_context"}

    def diff(claim: Claim, verb: str) -> AbDelta:
        scope = assay_root.scope(claim)
        outcome: Result[Report, Fault] = package_rail.list(assay_root.settings, scope, package_rail.PackageParams())
        payload = outcome.ok if outcome.is_ok() else outcome.error
        assay_env = wrap_envelope(payload, claim=claim, verb=verb)
        quality_settings = QualitySettings(root=assay_root.root, rhino_app=None, run_id="ab-diff")
        quality_scope = QualityScope(root=assay_root.root, rail=claim.value, scope_path=assay_root.root, dotnet_env={})
        quality_payload = quality_package.package_list_payload(quality_settings, quality_scope)
        quality_decoded = _json_object(msgspec.json.decode(quality_payload.ok if quality_payload.is_ok() else b"{}"))
        return AbDelta(assay=assay_env, quality=quality_decoded, mapping=mapping)

    return diff


def _json_object(value: object) -> dict[str, object]:
    match value:
        case dict() as decoded:
            return {str(key): item for key, item in decoded.items()}
        case _:
            raise AssertionError(f"expected JSON object payload, got {value!r}")
