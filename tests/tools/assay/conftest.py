"""Typed fixture DNA for the assay test suite.

A ``msgspec.inspect``-driven strategy resolver registers every wire struct with bounded leaf strategies
(reading ``Meta`` constraints generically, so ``from_type(Fault)`` yields non-empty bounded messages and
adding a model field over the supported JSON leaf taxonomy needs zero conftest change), a single
``pytest_generate_tests`` axis-matrix hook
composes the protocol/exec/clock/census ``Capsule`` over an ``@axes`` marker, the polymorphic ``cli``
fixture runs the CLI in-process (or as a real subprocess under ``isolate=True``), the ``AssayMachine``
base wires RBSM scaffolding for W3's lease/tick/history machines, and the promoted oracles
(``assert_wire_roundtrip``/``expect_ok``/``assert_counts_consistent``/``make_history_envelope``) let
laws read as one-liners. Plus the socket-free ``RailProbe`` host, the loopback ``SshLoopback`` network
seam, the ``BridgeResult``/``YakShape`` materializers, and the table-driven ``ab_delta`` A/B fixture.

W3 discipline: prefer ``@precondition`` over ``assume``, ``data()`` over nested ``@given``, and the
shared oracles over hand-rolled ``assert x.is_ok()``.
"""

# --- [IMPORTS] ------------------------------------------------------------------------

import contextlib
from dataclasses import dataclass, field
import datetime as dt
import functools
import importlib.util
from itertools import product
import os
from pathlib import Path
import shutil
import tempfile
from types import SimpleNamespace
from typing import get_args, override, Protocol, TYPE_CHECKING
from unittest.mock import create_autospec, MagicMock

import anyio
from expression import Ok, Result  # noqa: TC002  # Result used as runtime annotation in diff() closure
from hypothesis import given, settings as hyp_settings, strategies as st
from hypothesis.stateful import initialize, invariant, RuleBasedStateMachine
import msgspec
import msgspec.inspect as msgspec_inspect
import psutil as _psutil
import pytest
import time_machine
from upath import UPath

from tools.assay.composition.registry import REGISTRY
from tools.assay.composition.settings import ArtifactScope, ArtifactStore, AssaySettings  # noqa: TC001
from tools.assay.core import engine as engine_mod
from tools.assay.core.engine import _LeaseOwner  # noqa: PLC2701  # RBSM scaffolding draws the engine lease block
from tools.assay.core.model import (
    AnyDetail,
    ApiResolution,
    ApiSurface,
    Artifact,
    ArtifactKind,
    Check,
    Claim,
    Completed,
    Counts,
    Diagnostic,
    Envelope,
    envelope as wrap_envelope,
    Fault,
    fold,
    Match,
    PackageRun,
    receipt,
    Report,
    RunDelta,
    RunSnapshot,
    TestRun,
    Tool,
    VerifySummary,
)
from tools.assay.core.status import RailStatus
from tools.assay.rails import bridge as bridge_rail, package as package_rail
from tools.quality.rails import package as quality_package
from tools.quality.settings import ArtifactScope as QualityScope, QualitySettings


if TYPE_CHECKING:
    from collections.abc import AsyncGenerator, Callable, Generator, Mapping


# --- [CONSTANTS] -----------------------------------------------------------------------

# Host-gated skipif markers — computed once at collection so xdist workers share the result.
_DOTNET = shutil.which("dotnet")
_YAK = shutil.which("yak")
_ILSPYCMD = shutil.which("ilspycmd")
_UV = shutil.which("uv")
_RHINOWIP = next(
    (p for p in ("/Applications/RhinoWIP.app", "/Applications/Rhino WIP.app", "/Applications/Rhino 8 WIP.app") if Path(p).is_dir()), None
)
_TREE_SITTER_PY: bool = importlib.util.find_spec("tree_sitter_python") is not None
_EPOCH = dt.datetime(2026, 1, 1, tzinfo=dt.UTC)
_REPO_ROOT = Path(__file__).resolve().parents[3]  # subprocess CWD so `python -m tools.assay` resolves the package

skip_no_dotnet = pytest.mark.skipif(_DOTNET is None, reason="dotnet not on PATH")
skip_no_rhino = pytest.mark.skipif(_RHINOWIP is None, reason="RhinoWIP.app not installed")
skip_no_yak = pytest.mark.skipif(_YAK is None, reason="yak not on PATH")
skip_no_ilspycmd = pytest.mark.skipif(_ILSPYCMD is None, reason="ilspycmd not on PATH")
skip_no_tree_sitter_py = pytest.mark.skipif(not _TREE_SITTER_PY, reason="tree-sitter-python grammar absent")

# Deterministic codec pair shared by the wire-roundtrip oracle (mirrors model._ENCODER order policy).
_ENCODER = msgspec.json.Encoder(order="deterministic")

# Axis vocabulary for the capsule matrix — the (proto, exec_mode, clock, census) cartesian.
_AXES: dict[str, tuple[str, ...]] = {
    "proto": ("file", "memory"),
    "exec_mode": ("local", "remote"),
    "clock": ("real", "frozen"),
    "census": ("live", "double"),
}
_AXIS_DEFAULT = ("file", "local", "real", "live")


# --- [TYPES] ----------------------------------------------------------------------------


class VerbRunner(Protocol):
    """In-process / subprocess CLI runner fixture surface.

    Synchronous so the in-process default (which spawns its own ``anyio`` event loop inside ``main``)
    is callable from any test; ``isolate=True`` drives the subprocess via ``anyio.run`` internally.
    """

    def __call__(self, *argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> tuple[Envelope, int]: ...


# --- [GENERATORS] -----------------------------------------------------------------------
# A msgspec.inspect-driven resolver maps each wire struct field to a bounded leaf strategy reading its
# Meta constraints, so from_type(X) round-trips through the deterministic codec and yields non-empty
# bounded strings. register_type_strategy each struct; StrEnum resolves natively (no enum registration).


def _leaf(node: msgspec_inspect.Type) -> st.SearchStrategy[object]:  # noqa: C901, PLR0911, PLR0912, PLR0914  # closed-taxonomy match dispatch: one polymorphic surface over the msgspec node algebra
    """Map one ``msgspec.inspect`` field node to a codec-bounded Hypothesis strategy.

    Reads ``ge``/``gt``/``le``/``lt``/``max_length`` so generated values satisfy the wire ``Meta`` and
    survive an encode/decode round-trip; strings draw ``min_size=1`` so faults carry real messages.
    ``CustomType`` (``Parser``/``InprocThunk``/``Path``) forces ``None`` — the registered resolver routes
    every ``Tool``/``Check`` callable + ``cwd`` field through here, so drawn instances are encode-clean.
    Covers the JSON-codec leaf taxonomy a future ``Detail``/``Report`` field could introduce
    (datetime/date/dict/list/set/literal); only msgspec nodes with no JSON-stable projection raise.

    Returns:
        A bounded strategy for the node, recursing through unions, collections, and nested structs.

    Raises:
        AssertionError: When the node type is absent from the supported JSON leaf taxonomy.
    """
    match node:
        case msgspec_inspect.IntType(ge=ge, gt=gt, le=le, lt=lt):
            lo = ge if ge is not None else (gt + 1 if gt is not None else 0)
            hi = le if le is not None else (lt - 1 if lt is not None else 1_000_000)
            return st.integers(min_value=lo, max_value=hi)
        case msgspec_inspect.FloatType(ge=ge, gt=gt, le=le, lt=lt):
            lo_f = ge if ge is not None else (gt if gt is not None else 0.0)
            hi_f = le if le is not None else (lt if lt is not None else 1_000_000.0)
            exclude_lo = ge is None and gt is not None  # Meta(gt=0): timeout must be strictly positive
            exclude_hi = le is None and lt is not None
            return st.floats(min_value=lo_f, max_value=hi_f, exclude_min=exclude_lo, exclude_max=exclude_hi, allow_nan=False, allow_infinity=False)
        case msgspec_inspect.StrType(max_length=cap):
            return st.text(min_size=1, max_size=min(cap or 64, 64))
        case msgspec_inspect.BoolType():
            return st.booleans()
        case msgspec_inspect.BytesType():
            return st.binary(max_size=256)
        case msgspec_inspect.EnumType(cls=cls):
            return st.sampled_from(list(cls))
        case msgspec_inspect.LiteralType(values=values):
            return st.sampled_from(list(values))
        case msgspec_inspect.DateTimeType():
            return st.datetimes(timezones=st.just(dt.UTC))  # msgspec JSON requires tz-aware (RFC3339) datetimes
        case msgspec_inspect.DateType():
            return st.dates()
        case msgspec_inspect.NoneType():
            return st.none()
        case msgspec_inspect.UnionType(types=types):
            return st.one_of(*(_leaf(t) for t in types))
        case msgspec_inspect.VarTupleType(item_type=item):
            return st.lists(_leaf(item), max_size=3).map(tuple)
        case msgspec_inspect.TupleType(item_types=items):
            return st.tuples(*(_leaf(t) for t in items))
        case msgspec_inspect.ListType(item_type=item):
            return st.lists(_leaf(item), max_size=3)
        case msgspec_inspect.SetType(item_type=item) | msgspec_inspect.FrozenSetType(item_type=item):
            return st.frozensets(_leaf(item), max_size=3)
        case msgspec_inspect.DictType(key_type=key, value_type=val):
            return st.dictionaries(_leaf(key), _leaf(val), max_size=3)
        case msgspec_inspect.StructType(cls=cls):
            return _resolver(cls)
        case msgspec_inspect.CustomType():
            return st.none()  # Parser / InprocThunk / Path: no JSON-stable projection -> draw None (round-trip clean)
        case _:
            raise AssertionError(f"unhandled msgspec node {type(node).__name__}")


def _resolver(cls: type[msgspec.Struct]) -> st.SearchStrategy[object]:
    """Build a ``builds`` strategy for a struct from its inspected fields.

    Returns:
        A strategy emitting fully-populated instances with every optional field overridden.
    """
    info = msgspec_inspect.type_info(cls)
    assert isinstance(info, msgspec_inspect.StructType)
    return st.builds(cls, **{f.name: _leaf(f.type) for f in info.fields})


# Every struct W3 draws under @given (incl. Tool/Check). Registering Tool/Check via _resolver routes
# their CustomType fields (parser/thunk callables, cwd: Path) through _leaf -> st.none(), so from_type
# yields encode-clean instances; OMITTING them lets hypothesis resolve the union natively, which both
# emits SmallSearchSpaceWarning (hard error under filterwarnings=['error']) for Path and generates live
# lambdas that crash msgspec.encode. Order is irrelevant: detail_st derives from AnyDetail, not a slice.
_WIRE_STRUCTS: tuple[type[msgspec.Struct], ...] = (
    Completed, Fault, Counts, Artifact, Match, RunSnapshot,
    ApiSurface, VerifySummary, TestRun, PackageRun, ApiResolution, Diagnostic, RunDelta, Report,
    Tool, Check,
)  # fmt: skip
for _struct in _WIRE_STRUCTS:
    st.register_type_strategy(_struct, _resolver(_struct))

# Module-level NAMES the foundation tests + W3 reference; thin aliases over from_type / the resolver.
rail_status_st: st.SearchStrategy[RailStatus] = st.from_type(RailStatus)
binds_st = st.sampled_from(REGISTRY)
completed_st: st.SearchStrategy[Completed] = st.from_type(Completed)
fault_st: st.SearchStrategy[Fault] = st.from_type(Fault)
counts_st: st.SearchStrategy[Counts] = st.from_type(Counts)
artifact_st: st.SearchStrategy[Artifact] = st.from_type(Artifact)
match_st: st.SearchStrategy[Match] = st.from_type(Match)
run_snapshot_st: st.SearchStrategy[RunSnapshot] = st.from_type(RunSnapshot)
report_st: st.SearchStrategy[Report] = st.from_type(Report)
# Derive the Detail variants from the AnyDetail alias itself — self-tracking, no positional [6:13] coupling.
detail_st: st.SearchStrategy[AnyDetail] = st.one_of(*(st.from_type(v) for v in get_args(AnyDetail.__value__)))
# The resolver forces Tool.parser/Tool.thunk (Callable) and Check.cwd (Path) to None via _leaf(CustomType),
# so from_type sweeps the engine Runner x Input x Mode x Language cartesian with encode-clean instances.
tool_st: st.SearchStrategy[Tool] = st.from_type(Tool)
check_st: st.SearchStrategy[Check] = st.from_type(Check)

# RBSM entity strategies the W3 machines draw from.
lease_owner_st: st.SearchStrategy[_LeaseOwner] = st.builds(
    _LeaseOwner,
    resource=st.text(min_size=1, max_size=32),
    run_id=st.text(min_size=1, max_size=32),
    pid=st.sampled_from((os.getpid(), -1, 0, 99999)),  # live self + dead/adversarial sentinels for _stale laws
    create_time=st.floats(min_value=0.0, max_value=2e9, allow_nan=False, allow_infinity=False),
)
# ISO-8601 timestamps are lexically monotonic, so run_id sorting == chronology for delta/retention order.
_ISO_RUN_ID = r"\A20[2-9][0-9]-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9])T([01][0-9]|2[0-3]):[0-5][0-9]:[0-5][0-9]\.[0-9]{6}Z\Z"
run_id_st: st.SearchStrategy[str] = st.from_regex(_ISO_RUN_ID, fullmatch=True)
tick_delta_st: st.SearchStrategy[float] = st.floats(min_value=0.0, max_value=5_000.0, allow_nan=False, allow_infinity=False)


@st.composite
def _envelopes(draw: st.DrawFn) -> Envelope:
    """Draw a wire-consistent Envelope from a Report or Fault via the canonical ``envelope`` projection.

    Returns:
        Envelope whose status/exit_code derive from the wrapped payload.
    """
    payload: Report | Fault = draw(st.one_of(report_st, fault_st))
    return wrap_envelope(payload, claim=draw(st.from_type(Claim)), verb=draw(st.text(max_size=32)), run_id=draw(st.text(max_size=32)))


envelope_st: st.SearchStrategy[Envelope] = _envelopes()
st.register_type_strategy(Envelope, envelope_st)

# Every module-level strategy name a foundation/W3 law draws under @given; validated once post-init (see
# pytest_configure). .validate() is the path that raises SmallSearchSpaceWarning (-> hard error under
# filterwarnings=['error']) which .example() silently skips — so a future wire field whose Path/Callable
# escapes the resolver fails at session start, not deep in a W3 law. hypothesis forbids running it during
# conftest import (HypothesisSideeffectWarning), hence the deferral to the configure hook.
_VALIDATED_STRATEGIES: tuple[st.SearchStrategy[object], ...] = (
    rail_status_st, completed_st, fault_st, counts_st, artifact_st, match_st, run_snapshot_st,
    report_st, detail_st, tool_st, check_st, lease_owner_st, run_id_st, tick_delta_st, envelope_st,
)  # fmt: skip
# Wire strategies whose drawn instance must survive the deterministic codec — the encode step is what
# catches a live Tool.parser/thunk Callable (a TypeError validate() alone never reaches). Tool/Check
# encode-clean only because the resolver forced their callable/Path fields to None.
_ENCODE_PROBE: tuple[tuple[st.SearchStrategy[object], type[object]], ...] = (
    (completed_st, Completed), (fault_st, Fault), (counts_st, Counts), (artifact_st, Artifact),
    (match_st, Match), (run_snapshot_st, RunSnapshot), (report_st, Report),
    (tool_st, Tool), (check_st, Check), (envelope_st, Envelope),
)  # fmt: skip


# --- [MODELS] --------------------------------------------------------------------------


@dataclass(frozen=True, slots=True)
class AssayHarness:
    """Isolated tmp-tree capsule: all assay I/O roots under ``<root>/.artifacts/assay``.

    ``settings`` carries ``exec_target=""``/``exec_known_hosts=None`` for point-and-go local operation
    that mutates nothing outside ``root``. ``remote`` re-derives the same settings against an ssh
    ``exec_target`` for the network-marked laws.
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
class Capsule:
    """Composed capability over the four axes; delegates tmp-tree ops to the harness.

    Built by the ``capsule`` fixture from a ``(proto, exec_mode, clock, census)`` tuple the
    ``pytest_generate_tests`` hook parametrizes via the ``@axes`` marker. ``store`` is
    protocol-dispatched, ``settings`` is local or remote, ``census`` is the patched psutil
    module-double (or ``None`` under live census) and ``clock`` records the active wall-clock mode.
    ``clock='frozen'`` freezes the wall clock at ``_EPOCH``; ``time_machine`` cannot freeze
    ``time.monotonic`` (automation jitter/governor stay live).
    """

    harness: AssayHarness
    store: ArtifactStore
    settings: AssaySettings
    proto: str
    exec_mode: str
    clock: str
    census: MagicMock | None

    def write(self, rel: str | Path, text: str = "") -> Path:
        return self.harness.write(rel, text)

    def scope(self, claim: Claim = Claim.PACKAGE) -> ArtifactScope:
        return ArtifactScope.open(self.settings, claim)

    @property
    def root(self) -> Path:
        return self.harness.root


@dataclass(frozen=True, slots=True)
class RailProbe:
    """Socket-free mock host: monkeypatch a rail-module member to a canned receipt.

    Patch target is the OWNER rail module, never ``core.engine``, mirroring production binding.
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
    """A live loopback asyncssh server bound via ``asyncssh.listen``.

    ``exec_target`` includes a username because asyncssh ``saslprep`` rejects ``None`` at connect.
    """

    port: int

    @property
    def exec_target(self) -> str:
        return f"ssh://x@127.0.0.1:{self.port}"


@dataclass(frozen=True, slots=True)
class AbDelta:
    """The A/B delta: both decoded operator outputs plus the field-name correspondence."""

    assay: Envelope
    quality: dict[str, object]
    mapping: Mapping[str, str]


@dataclass(frozen=True, slots=True)
class Verdict:
    """Per-field A/B verdict: the assay value, the predecessor value, and whether assay subsumes it."""

    field: str
    assay: object
    quality: object
    subsumes: bool


@dataclass(frozen=True, slots=True)
class AbCell:
    """One overlapping verb: bound assay + predecessor thunks plus the field-name correspondence.

    The thunks absorb the non-uniform predecessor signatures (``package_list_payload(settings, scope)``
    vs ``package_plan_payload(settings, scope, slug, version)`` vs ``plan_payload(settings, scope,
    paths)``) so the law table iterates one uniform shape.
    """

    claim: Claim
    verb: str
    assay: Callable[[AssayHarness], Result[Report, Fault]]
    quality: Callable[[AssayHarness], Result[bytes, object]]
    mapping: Mapping[str, str]


@dataclass(frozen=True, slots=True)
class BridgeResult:
    """``_BridgeResult`` JSON writer: one valid camelCase payload plus three adversarial variants."""

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
    """Fake-yak + fake-msbuild materializer."""

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


# --- [PSUTIL DOUBLES] ------------------------------------------------------------------
# ``_proc`` builds a create_autospec(psutil.Process) double from keyword fields — no hand-rolled API.
# ``_make_psutil_module`` wraps N pid->proc mappings into a MagicMock(spec=psutil) module double.


def _proc(
    *,
    pid: int = 12345,
    rss: int = 4096,
    fds: int = 8,
    cpu: float = 0.0,
    running: bool = True,
    create_time: float = 1_700_000_000.0,
    raise_no_such: bool = False,
) -> MagicMock:
    """Return a ``create_autospec(psutil.Process)`` double with the given field values.

    ``raise_no_such=True`` marks the double as a dead-process sentinel: the module-level factory
    (``_make_psutil_module``) raises ``NoSuchProcess`` instead of returning it.

    Returns:
        A ``psutil.Process`` autospec double carrying the requested field values.
    """
    p: MagicMock = create_autospec(_psutil.Process, instance=True)
    p.pid = pid
    p.memory_info.return_value = SimpleNamespace(rss=rss)
    p.num_fds.return_value = fds
    p.cpu_percent.return_value = cpu
    p.is_running.return_value = running
    p.create_time.return_value = create_time
    p._raise_no_such = raise_no_such  # private sentinel flag; consumed by _make_psutil_module
    return p


def _make_psutil_module(procs: dict[int | None, MagicMock], *, cpu_count: int = 4) -> MagicMock:
    """Return a ``MagicMock(spec=psutil)`` module double that dispatches ``Process(pid)`` via ``procs``.

    ``procs[None]`` is the self-process (``Process()`` / ``Process(os.getpid())``); integer keys are
    specific pids. An unregistered pid causes ``NoSuchProcess`` — no silent fallback.
    ``raise_no_such=True`` on a double causes the factory to raise ``NoSuchProcess(pid)`` on lookup.

    Returns:
        A ``psutil`` module double with a pid-dispatching ``Process`` factory.
    """
    fake = MagicMock(spec=_psutil)
    fake.NoSuchProcess = _psutil.NoSuchProcess
    fake.AccessDenied = _psutil.AccessDenied
    fake.cpu_count.return_value = cpu_count

    def _process_factory(pid: int | None = None) -> MagicMock:
        proc = procs.get(pid)
        if proc is None:
            raise _psutil.NoSuchProcess(pid if isinstance(pid, int) else 0)
        if getattr(proc, "_raise_no_such", False):
            raise _psutil.NoSuchProcess(int(proc.pid))
        return proc

    fake.Process.side_effect = _process_factory
    return fake


# --- [ORACLES] -------------------------------------------------------------------------
# >=2-consumer oracle helpers the foundation tests + W3 share; single-file helpers stay in their file.


def expect_ok[T](result: Result[T, object]) -> T:
    """Assert a Result is Ok and return its value (mirrors tools/quality conftest).

    Returns:
        The unwrapped success value.
    """
    assert result.is_ok(), f"expected Ok, got Error: {result}"
    return result.ok


def assert_wire_roundtrip[T](value: T, typ: type[T]) -> T:
    """Assert deterministic encode/decode/re-encode byte-identity for a wire struct.

    Returns:
        The decoded value (equal to ``value``).
    """
    raw = _ENCODER.encode(value)
    decoded = msgspec.json.decode(raw, type=typ)
    assert decoded == value, f"decode mismatch for {typ.__name__}"
    assert _ENCODER.encode(decoded) == raw, f"re-encode not byte-identical for {typ.__name__}"
    return decoded


def assert_counts_consistent(report: Report) -> None:
    """Assert the report fold invariant: ``total == ok + failed`` and ``len(results) == failed``."""
    assert report.counts.total == report.counts.ok + report.counts.failed, f"counts arithmetic broken: {report.counts}"
    assert len(report.results) == report.counts.failed, f"defect rows != failed count: {len(report.results)} != {report.counts.failed}"


def make_history_envelope(run_id: str, *, claim: Claim = Claim.STATIC, status: RailStatus = RailStatus.OK) -> Envelope:
    """Build a deterministic persistable Envelope for a run_id (history/retention laws).

    Returns:
        An Envelope carrying a single-receipt OK report stamped with ``run_id``.
    """
    report = fold(claim, "check", (receipt(("tool",), 0, status=status),))
    return msgspec.structs.replace(wrap_envelope(report, claim=claim, verb="check"), run_id=run_id)


def pipe_history(store: ArtifactStore, run_ids: tuple[str, ...]) -> None:
    """Write a canned Envelope into the history tree for each run_id (delta/retention setup)."""
    for run_id in run_ids:
        directory = store.ensure("history", run_id)
        store.fs.pipe_file(f"{directory}/envelope.json", _ENCODER.encode(make_history_envelope(run_id)))


# --- [TABLES] ---------------------------------------------------------------------------
# VERB_MATRIX drives the A/B closure: one bound (assay, predecessor) pair per overlapping read-only verb.
# W2-A grows this to the full overlapping-verb set; the machinery (ab_delta + Verdict) is verb-agnostic.


def _qsettings(harness: AssayHarness) -> QualitySettings:
    return QualitySettings(root=harness.root, rhino_app=None, run_id="ab-delta")


def _qscope(harness: AssayHarness, claim: Claim) -> QualityScope:
    return QualityScope(root=harness.root, rail=claim.value, scope_path=harness.root, dotnet_env={})


VERB_MATRIX: tuple[AbCell, ...] = (
    AbCell(
        Claim.PACKAGE,
        "list",
        assay=lambda h: package_rail.list(h.settings, ArtifactScope.open(h.settings, Claim.PACKAGE), package_rail.PackageParams()),
        quality=lambda h: quality_package.package_list_payload(_qsettings(h), _qscope(h, Claim.PACKAGE)),
        mapping={"rail": "claim", "data": "report", "evidence": "error_context"},
    ),
)

_CELLS: dict[tuple[Claim, str], AbCell] = {(c.claim, c.verb): c for c in VERB_MATRIX}


# --- [RBSM] -----------------------------------------------------------------------------
# Shared RuleBasedStateMachine base so W3's LeaseMachine/TickMachine/HistoryMachine are trivial: an
# @initialize wires the tmp ArtifactStore; subclasses bind the rasm-stateful profile via
# ``TestCase = AssayMachine.TestCase`` then ``settings`` on the class (never assign Machine.settings).


class AssayMachine(RuleBasedStateMachine):
    """Base RBSM wiring an isolated tmp ``ArtifactStore`` and shared invariant helpers for W3 machines.

    Subclasses add ``@rule``/``@invariant``/``@precondition`` and select the profile with
    ``@hyp_settings(hyp_settings.get_profile('rasm-stateful'))`` on the generated ``TestCase``.
    """

    @initialize()
    def _open_store(self) -> None:
        base = Path(tempfile.mkdtemp(prefix="assay-rbsm-"))
        (base / "Workspace.slnx").write_text("", encoding="utf-8")
        self.settings = AssaySettings(root=UPath(base), exec_target="", exec_known_hosts=None)
        self.store = self.settings.store(protocol="file")

    @invariant()
    def _store_namespaced(self) -> None:
        if hasattr(self, "store"):
            assert self.store.root, "ArtifactStore root must be namespaced"


# Bind the rasm-stateful profile (200-step traces, shared example DB) onto the base TestCase so every
# W3 subclass inherits it through ``class LeaseMachine(AssayMachine)`` without re-declaring settings.
AssayMachine.TestCase.settings = hyp_settings.get_profile("rasm-stateful")


def at_most_one_holder(store: ArtifactStore, resource: str) -> bool:
    """Lease invariant: at most one live owner block exists for a resource in the store.

    Returns:
        True when zero or one non-empty owner block is present.
    """
    held = tuple(p for p in store.fs.glob(f"/{store.root}/locks/{resource}*.lock") if store.fs.cat_file(p))
    return len(held) <= 1


# --- [COMPOSITION] ---------------------------------------------------------------------


def pytest_generate_tests(metafunc: pytest.Metafunc) -> None:
    """Parametrize the ``capsule`` fixture from an ``@pytest.mark.axes(...)`` marker.

    ``@axes('proto', 'census')`` sweeps the cartesian of the named axes (others pinned to canonical
    default); ``@axes()`` sweeps all four. A test that requests ``capsule`` with no marker runs the lone
    canonical ``(file, local, real, live)`` cell. Densest native axis-matrix idiom — one hook + one
    fixture replaces four indirect-parametrize axis fixtures.
    """
    marker = metafunc.definition.get_closest_marker("axes")
    if "capsule" not in metafunc.fixturenames or marker is None:
        return
    names = marker.args or tuple(_AXES)
    choices = tuple(_AXES[n] if n in names else (_AXIS_DEFAULT[i],) for i, n in enumerate(_AXES))
    cells = tuple(product(*choices))
    metafunc.parametrize("capsule", cells, indirect=True, ids=["-".join(c) for c in cells])


@pytest.fixture
def anyio_backend() -> str:
    """Pin the single asyncio backend (function scope) so async fixtures avoid the module-scope default.

    Returns:
        The asyncio backend name.
    """
    return "asyncio"


@pytest.fixture
def assay_root(tmp_path: Path) -> AssayHarness:
    """Build isolated tmp-tree settings with redirected artifact I/O.

    Returns:
        Assay harness rooted under the pytest temporary directory.
    """
    (tmp_path / "Workspace.slnx").write_text("", encoding="utf-8")
    settings = AssaySettings(root=UPath(tmp_path), exec_target="", exec_known_hosts=None)
    return AssayHarness(tmp_path, settings)


@pytest.fixture
def capsule(request: pytest.FixtureRequest, assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> Generator[Capsule]:
    """Compose the four axes (parametrized by the ``@axes`` marker) into one capability.

    ``request.param`` is the ``(proto, exec_mode, clock, census)`` tuple; unmarked tests get the
    canonical default. Performs the protocol store dispatch, local/remote settings derivation, frozen
    wall-clock travel, and psutil module-double monkeypatch inline, then tears the memory namespace down.

    Yields:
        A ``Capsule`` carrying the protocol store, local/remote settings, clock mode, and census double.
    """
    proto, exec_mode, clock, census = getattr(request, "param", _AXIS_DEFAULT)
    store = assay_root.settings.store(protocol=proto)
    settings = assay_root.remote("ssh://probe@127.0.0.1:22") if exec_mode == "remote" else assay_root.settings
    double: MagicMock | None = None
    if census == "double":
        self_proc = _proc(pid=os.getpid())
        double = _make_psutil_module({None: self_proc, os.getpid(): self_proc, 99999: _proc(pid=99999, raise_no_such=True)})
        monkeypatch.setattr(engine_mod, "psutil", double)
    cell = Capsule(harness=assay_root, store=store, settings=settings, proto=proto, exec_mode=exec_mode, clock=clock, census=double)
    with time_machine.travel(_EPOCH, tick=False) if clock == "frozen" else contextlib.nullcontext():
        yield cell
    if proto == "memory" and store.fs.exists(f"/{store.root}"):
        store.fs.rm(f"/{store.root}", recursive=True)


@pytest.fixture
def mem_store(assay_root: AssayHarness) -> Generator[ArtifactStore]:
    """Isolated fsspec ``memory://`` ArtifactStore partitioned by ``run_id`` (direct store-function seam).

    Yields:
        An ``ArtifactStore`` backed by ``fsspec.MemoryFileSystem`` rooted at a ``run_id``-keyed prefix.
    """
    store = assay_root.settings.store(protocol="memory")
    yield store
    if store.fs.exists(f"/{store.root}"):
        store.fs.rm(f"/{store.root}", recursive=True)


@pytest.fixture
def envelope() -> Callable[[pytest.CaptureFixture[str]], Envelope]:
    """Decode the single stdout Envelope line the CLI ``_emit`` writes (synchronous capsys seam).

    Returns:
        Callable that reads captured stdout and decodes one Envelope.
    """

    def decode(capsys: pytest.CaptureFixture[str]) -> Envelope:
        rows = capsys.readouterr().out.splitlines()
        assert len(rows) == 1, f"expected exactly one stdout Envelope line, got {len(rows)}"
        return msgspec.json.decode(rows[0].encode(), type=Envelope)

    return decode


@pytest.fixture
def cli(assay_root: AssayHarness, capsysbinary: pytest.CaptureFixture[bytes], monkeypatch: pytest.MonkeyPatch) -> VerbRunner:
    """Run the assay CLI and decode its single stdout Envelope plus exit code.

    Default (``isolate=False``): drives ``main([*argv])`` in-process under ``ASSAY_ROOT=<root>``
    (~0.001s vs ~0.65s subprocess), reading the one Envelope from ``capsysbinary`` and the returned exit
    code. ``isolate=True`` spawns the real ``uv run python -m tools.assay`` subprocess via ``anyio.run``
    for the few argv/exit-code isolation laws. Synchronous: ``main`` opens its own ``anyio`` loop, so an
    async wrapper would raise "Already running asyncio". Structlog writes to stderr; one Envelope reaches
    stdout.

    Returns:
        Synchronous callable ``run(*argv, isolate=False, extra_env=None) -> (Envelope, int)``.
    """
    monkeypatch.setenv("ASSAY_ROOT", str(assay_root.root))

    def run(*argv: str, isolate: bool = False, extra_env: dict[str, str] | None = None) -> tuple[Envelope, int]:
        for key, value in (extra_env or {}).items():
            monkeypatch.setenv(key, value)
        match isolate:
            case False:
                from tools.assay.__main__ import main  # noqa: PLC0415  # in-proc import keeps the subprocess path import-clean

                code = main([*argv])
                rows = capsysbinary.readouterr().out.splitlines()
                assert len(rows) == 1, f"expected exactly one stdout Envelope line, got {len(rows)}"
                return msgspec.json.decode(rows[0], type=Envelope), code
            case True:
                if _UV is None:
                    pytest.skip("uv not on PATH")
                spawn_env = {**os.environ, "ASSAY_ROOT": str(assay_root.root), **(extra_env or {})}  # noqa: TID251  # test boundary: clone + override env for subprocess isolation
                # CWD is the repo root (so `python -m tools.assay` resolves the package); ASSAY_ROOT isolates I/O to tmp.
                spawn = functools.partial(anyio.run_process, env=spawn_env, cwd=str(_REPO_ROOT), check=False)
                result = anyio.run(spawn, ["uv", "run", "python", "-m", "tools.assay", *argv])
                rows = result.stdout.splitlines()
                assert len(rows) == 1, f"expected 1 Envelope line, got {len(rows)}: {result.stdout[:300]!r}"
                return msgspec.json.decode(rows[0], type=Envelope), result.returncode

    return run


@pytest.fixture
def rail_probe() -> RailProbe:
    """Build a socket-free rail host with canned receipts.

    Returns:
        Probe that installs fake ``run_check`` and ``fan_out`` responses.
    """
    return RailProbe()


@pytest.fixture
async def ssh_loopback(socket_enabled: None) -> AsyncGenerator[SshLoopback]:
    """Live loopback asyncssh server (network-marked): yields the ssh ``exec_target`` capsule.

    Uses ``asyncssh.listen`` as an async context manager — no daemon threads, no manual teardown, no
    ResourceWarning under ``filterwarnings=["error"]``. ``asyncssh`` is imported lazily here so the
    ~194ms import tax is paid only on network runs.

    Yields:
        ``SshLoopback`` carrying the bound port and ``ssh://x@127.0.0.1:<port>`` ``exec_target``.
    """
    _ = socket_enabled
    import asyncssh  # noqa: PLC0415  # lazy: keep the import off the non-network collection path

    class _Server(asyncssh.SSHServer):
        @override
        def begin_auth(self, username: str) -> bool:
            _ = username
            return False

    async def _handler(process: asyncssh.SSHServerProcess[str]) -> None:  # noqa: RUF029
        # Echo a fixed token PLUS the received command so callers can assert the command transited.
        command = process.command or ""
        process.stdout.write(f"remote-ok:{command}\n")
        process.exit(0)

    key = asyncssh.generate_private_key("ssh-ed25519")
    server = await asyncssh.listen("127.0.0.1", 0, server_host_keys=[key], server_factory=_Server, process_factory=_handler)
    async with server:
        yield SshLoopback(port=server.get_port())


@pytest.fixture
def bridge_result(tmp_path: Path) -> BridgeResult:
    """Build bridge-result fixtures for defensive-decode laws.

    Returns:
        Bridge result writer rooted under the pytest temporary directory.
    """
    return BridgeResult(tmp_path / "verify")


@pytest.fixture
def yak_shape() -> YakShape:
    """Build the fake yak and fake MSBuild materializer.

    Returns:
        Package-shape materializer for yak rail tests.
    """
    return YakShape()


@pytest.fixture
def ab_delta(assay_root: AssayHarness) -> Callable[[Claim, str], AbDelta]:
    """Run an overlapping verb on OURS and the ``tools.quality`` predecessor; decode both + the field map.

    Dispatches on ``(claim, verb)`` through ``VERB_MATRIX``. Fault-transparent: asserts the predecessor
    is ``is_ok()`` before decoding (silently degrading to empty-dict on fault was the prior bug).

    Returns:
        Callable that compares an assay Envelope with the predecessor payload for one overlapping verb.
    """

    def run(claim: Claim, verb: str) -> AbDelta:
        cell = _CELLS[claim, verb]
        outcome = cell.assay(assay_root)
        payload = outcome.ok if outcome.is_ok() else outcome.error
        assay_env = wrap_envelope(payload, claim=claim, verb=verb)
        quality = cell.quality(assay_root)
        assert quality.is_ok(), f"ab_delta: predecessor {claim.value} {verb} faulted: {quality}"
        quality_decoded = msgspec.json.decode(quality.ok, type=dict[str, object])
        return AbDelta(assay=assay_env, quality=quality_decoded, mapping=cell.mapping)

    return run


def pytest_configure(config: pytest.Config) -> None:
    """Register the ``axes`` marker for the capsule matrix hook.

    Keeps ``--strict-markers`` green and documents the axis-sweep contract W3 inherits.
    """
    config.addinivalue_line("markers", "axes(*names): sweep the named capsule axes (proto/exec_mode/clock/census); empty sweeps all four")


@pytest.fixture(scope="session", autouse=True)
def _strategy_validation_gate() -> None:
    """Validate + encode-probe every module-level strategy once per session, post-init.

    Closes the methodology hole that hid the Path/Callable defects: ``.example()`` skips both
    ``strategy.validate()`` (which raises ``SmallSearchSpaceWarning`` — a hard error under the suite's
    ``filterwarnings=['error']`` — when a field like ``Check.cwd`` escapes the resolver to native
    ``from_type(Path)``) AND the ``@given`` draw path. The inner ``@given`` law draws every encode-bearing
    strategy through ``data()`` and round-trips the deterministic codec, catching a live
    ``Tool.parser``/``thunk`` ``Callable`` that ``msgspec.encode`` rejects. A session-scoped autouse
    fixture runs after collection, when hypothesis lifts the ``HypothesisSideeffectWarning`` init guard
    that bans strategy materialization at conftest import.
    """
    for strategy in _VALIDATED_STRATEGIES:
        strategy.validate()

    @hyp_settings(max_examples=25, deadline=None)
    @given(data=st.data())
    def _encode_law(data: st.DataObject) -> None:
        for strategy, typ in _ENCODE_PROBE:
            assert_wire_roundtrip(data.draw(strategy), typ)

    _encode_law()
