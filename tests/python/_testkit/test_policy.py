"""SUT-agnostic policy meta-tests over registered packages, markers, hooks, and scratch paths."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from contextvars import ContextVar
from datetime import datetime, timedelta, UTC
import enum
import fnmatch
import functools
import os
from pathlib import Path  # module-level _PYPROJECT assignment prevents deferral
import shutil
import sys
import tomllib
from types import ModuleType, SimpleNamespace
from typing import TYPE_CHECKING
from unittest.mock import create_autospec
import uuid

import anyio
from hypothesis import is_hypothesis_test, Phase, settings as hyp_settings
import msgspec
import pytest

from tests.python._testkit import laws as laws_mod
from tests.python._testkit.bench import _series_from_storage, pytest_benchmark_update_json
from tests.python._testkit.laws import (
    assert_law_coverage,
    auto_exempt,
    consume_covers,
    LawRecord,
    MANIFEST,
    register_tree,
    spec,
    Sut,
    uncollected_laws,
)
from tests.python._testkit.runtime import PROFILE_DEFAULT, PROFILE_MUTATION, PROFILE_STATEFUL, REPO_ROOT
from tests.python._testkit.spec import support_matrix


if TYPE_CHECKING:
    from unittest.mock import Mock


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (consume_covers,)

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

_POLICY_MARKERS: frozenset[str] = frozenset({"benchmark", "mutation", "network", "property", "subprocess"})

# Repo-root residency is a closed allowlist: an unlisted entry is tool litter (a tool ran unrouted from root)
# or an unreviewed surface — route its output under .cache/ or .artifacts/, or review it and extend the roster.
_ROOT_ALLOWLIST: frozenset[str] = frozenset({
    ".DS_Store",
    ".archive",
    ".artifacts",
    ".cache",
    ".claude",
    ".config",
    ".editorconfig",
    ".git",
    ".gitattributes",
    ".gitignore",
    ".nx",
    ".venv",
    ".vscode",
    "AGENTS.md",
    "CLAUDE.md",
    "Directory.Build.props",
    "Directory.Build.targets",
    "Directory.Packages.props",
    "LICENSE",
    "NuGet.config",
    "README.md",
    "Workspace.slnx",
    "apps",
    "biome.json",
    "docs",
    "doppler.yaml",
    "global.json",
    "libs",
    "node_modules",
    "nx.json",
    "package.json",
    "playwright.config.ts",
    "pnpm-lock.yaml",
    "pnpm-workspace.yaml",
    "pyproject.toml",
    "stryker-config.json",
    "stryker.config.json",
    "tests",
    "tools",
    "tsconfig.base.json",
    "tsconfig.json",
    "uv.lock",
    "vite.config.ts",
    "vite.factory.ts",
    "vitest.config.ts",
})

# Campaign briefs, decisions, and specs land at root by workflow law.
_ROOT_PATTERNS: tuple[str, ...] = ("RASM-*.md",)

# --- [OPERATIONS] -----------------------------------------------------------------------


def _nav(node: dict[str, object], *keys: str) -> object:
    """Walk a nested TOML mapping by successive string keys.

    Returns:
        Value at the final key, or ``None`` when any key is absent or an intermediate value is not a mapping.
    """
    current: object = node
    for k in keys:
        match current:
            case {**mapping}:
                current = mapping.get(k)
            case _:
                return None
    return current


def _pyproject_data() -> dict[str, object]:
    """Load the root ``pyproject.toml`` as a plain string-keyed mapping.

    Returns:
        TOML document as ``dict[str, object]``.
    """
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    return data


def _pytest_ini_marker_names() -> frozenset[str]:
    """Return declared pytest marker names, or an empty set when absent."""
    markers: object = _nav(_pyproject_data(), "tool", "pytest", "markers")
    if not isinstance(markers, list):
        return frozenset()
    return frozenset(str(m).split(":")[0].strip() for m in markers)


def _collect_session_items(pytestconfig: pytest.Config) -> list[pytest.Function]:
    """Return collected ``Function`` items from the current session."""
    session: object = pytestconfig.pluginmanager.get_plugin("session")
    raw: object = getattr(session, "items", None)  # B009: session has no public stub; attribute is a fixed string, not computed
    return [item for item in (raw if isinstance(raw, list) else []) if isinstance(item, pytest.Function)]


# --- [LAW_COVERAGE_GATE]


def test_law_coverage_gate() -> None:
    """Registered SUT public symbols must have a law or an explicit exemption.

    Subset-aware: COVERS ledgers and ``@spec`` records aggregate across collected modules, so a
    package whose on-disk law modules were not all imported is skipped by name instead of censused
    against a partial manifest; every fully-collected package still gates.
    """
    if not laws_mod.SUT_PACKAGES:
        pytest.skip("no SUT registered — register_sut was not called in this collection")
    partial = uncollected_laws()
    assert_law_coverage(only=frozenset(laws_mod.SUT_PACKAGES) - frozenset(partial))
    if partial:
        detail = "; ".join(f"{package}: {', '.join(missing)}" for package, missing in sorted(partial.items()))
        pytest.skip(f"law census partial — uncollected law modules ({detail}); activation: full tests/python collection")


def test_law_coverage_is_package_scoped_not_name_global(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """A package-scoped law never covers another package's same-named symbol."""
    names = ("lawpkg_alpha", "lawpkg_beta")
    for name in names:
        pkg = tmp_path / name
        pkg.mkdir()
        (pkg / "__init__.py").write_text('__all__ = ["thing"]\n\n\ndef thing() -> None: ...\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))

    def _purge() -> None:
        [sys.modules.pop(name, None) for name in names]

    request.addfinalizer(_purge)

    record = LawRecord(subject="thing", law="alpha_thing_law", module=__name__, subject_module="lawpkg_alpha")
    monkeypatch.setattr(laws_mod, "MANIFEST", [record])
    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_alpha": Sut()})
    assert_law_coverage()

    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_alpha": Sut(), "lawpkg_beta": Sut()})
    with pytest.raises(AssertionError, match="lawpkg_beta"):
        assert_law_coverage()


def test_law_census_detects_partial_collection_and_gate_self_skips(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest
) -> None:
    """An uncollected on-disk law module suppresses the census by skip; a whole census still fails on real gaps.

    The pair is the gate's falsification: partial collection can be SEEN to skip, and the skip arm
    cannot mask a genuine gap once every law module is imported.
    """
    suite = tmp_path / "suite"
    suite.mkdir()
    (suite / "test_ghost.py").write_text("", encoding="utf-8")
    pkg = tmp_path / "lawpkg_partial"
    pkg.mkdir()
    (pkg / "__init__.py").write_text('__all__ = ["thing"]\n\n\ndef thing() -> None: ...\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))
    request.addfinalizer(lambda: sys.modules.pop("lawpkg_partial", None))

    monkeypatch.setattr(laws_mod, "MANIFEST", [])
    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_partial": Sut(suite=suite)})
    missing = uncollected_laws()["lawpkg_partial"]
    assert missing, "on-disk law module not reported as uncollected"
    with pytest.raises(pytest.skip.Exception, match="law census partial"):
        test_law_coverage_gate()

    for name in missing:
        monkeypatch.setitem(sys.modules, name, ModuleType(name))
    assert "lawpkg_partial" not in uncollected_laws(), "census still partial after every law module imported"
    with pytest.raises(AssertionError, match="lawpkg_partial"):
        test_law_coverage_gate()


def test_register_tree_registers_only_source_bearing_folders(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """Disk shape drives registration: source-bearing folders register with derived suites; sourceless folders and loose files never do.

    The live lane proves the repo derivation: every ``libs/python`` registration carries the
    repo-relative dotted prefix and a suite under ``tests/python/libs``.
    """
    source = tmp_path / "src"
    (source / "alpha").mkdir(parents=True)
    (source / "alpha" / "__init__.py").write_text("", encoding="utf-8")
    (source / "planning_only").mkdir()
    (source / "stray.py").write_text("", encoding="utf-8")
    suites = tmp_path / "suites"
    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {})

    assert register_tree(source, suites) == ("alpha",), "registration drifted from disk shape"
    assert laws_mod.SUT_PACKAGES["alpha"].suite == suites / "alpha", "suite derivation broke"
    assert register_tree(tmp_path / "absent", suites) == (), "a missing source root must register nothing"

    live = register_tree(REPO_ROOT / "libs" / "python", REPO_ROOT / "tests" / "python" / "libs")
    assert all(name.startswith("libs.python.") for name in live), f"repo-relative dotted derivation drifted: {live}"


def test_registered_suts_carry_their_suite_roots() -> None:
    """Live registrations derive a real suite directory, so subset detection is armed for every SUT."""
    if not laws_mod.SUT_PACKAGES:
        pytest.skip("no SUT registered — register_sut was not called in this collection")
    for package, sut in laws_mod.SUT_PACKAGES.items():
        assert sut.suite is not None and sut.suite.is_dir(), f"{package} registered without a live suite root: {sut.suite!r}"


def test_law_module_naming_matches_live_session_imports(pytestconfig: pytest.Config) -> None:
    """The census dotted-name derivation matches pytest's live import-mode module naming.

    Collection imported every collected law module, so its derived name must be in ``sys.modules``;
    a naming drift would make every census silently partial — a gate that could never gate — and
    this law is the in-session witness against that.
    """
    law_files = {
        path
        for item in _collect_session_items(pytestconfig)
        if (path := Path(item.path)).is_relative_to(REPO_ROOT) and any(fnmatch.fnmatch(path.name, pattern) for pattern in laws_mod._LAW_GLOBS)
    }
    assert law_files, "no law modules collected in this session"
    drifted = sorted(str(path) for path in law_files if laws_mod._module_name(path) not in sys.modules)
    assert not drifted, f"derived census names absent from sys.modules — the naming scheme drifted from pytest's import mode: {drifted}"


def test_phantom_export_fails_the_census_not_silently_exempt(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """An ``__all__`` name the module never defines is a census failure, never a value-only exemption."""
    pkg = tmp_path / "lawpkg_phantom"
    pkg.mkdir()
    (pkg / "__init__.py").write_text('__all__ = ["ghost"]\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))
    request.addfinalizer(lambda: sys.modules.pop("lawpkg_phantom", None))
    monkeypatch.setattr(laws_mod, "MANIFEST", [])
    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_phantom": Sut()})
    with pytest.raises(AssertionError, match="ghost"):
        assert_law_coverage()


# --- [SPEC_SETTINGS_POLICY]


def test_spec_follows_active_profile_pins_named_and_scales_timeout(monkeypatch: pytest.MonkeyPatch) -> None:
    """Unpinned laws track the session profile, a named profile pins, timeout is seconds, events tag every draw, and double-decoration refuses."""
    monkeypatch.setattr(laws_mod, "MANIFEST", [])
    runs: list[int] = []
    pinned_runs: list[int] = []
    tagged: list[int] = []

    def _tag(drawn: object) -> str:
        tagged.append(drawn) if isinstance(drawn, int) else None
        return f"n={drawn}"

    hyp_settings.register_profile("kit-probe", max_examples=3, deadline=None, database=None, derandomize=True)
    prior = hyp_settings.get_current_profile_name()
    hyp_settings.load_profile("kit-probe")
    try:
        # Collection order mirror: the CLI profile loads before law modules import, so decoration binds it.
        @spec(int, law="probe-follows", events=(_tag,))
        def probe(n: int) -> None:
            runs.append(n)

        @spec(int, profile=PROFILE_MUTATION, law="probe-pinned")
        def pinned(n: int) -> None:
            pinned_runs.append(n)

        # @given injects the drawn argument; the bare call is the runtime contract the checker cannot see.
        probe()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
        pinned()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
    finally:
        hyp_settings.load_profile(prior)
    assert len(runs) == 3, f"unpinned law ignored the active profile: {len(runs)} examples"
    assert tagged == runs, f"events tagger missed drawn examples: tagged {tagged}, ran {runs}"
    assert len(pinned_runs) == hyp_settings.get_profile(PROFILE_MUTATION).max_examples, f"named pin lost: {len(pinned_runs)} examples"

    @spec(int, given=False, timeout=2.5, law="probe-deadline")
    def bounded() -> None: ...

    resolved: object = getattr(bounded, "_hypothesis_internal_use_settings", None)
    assert getattr(resolved, "deadline", None) == timedelta(seconds=2.5), f"timeout=2.5 must mean seconds, resolved {resolved!r}"

    with pytest.raises(TypeError, match="double-decoration"):
        spec(int, law="probe-duplicate")(probe)

    # The subject algebra matches the resolver's: a PEP 695 alias injects, a bare callable refuses.
    type Pair = tuple[int, int]
    alias_runs: list[tuple[int, int]] = []

    @spec(Pair, profile="kit-probe", law="probe-alias")
    def alias_law(pair: tuple[int, int]) -> None:
        alias_runs.append(pair)

    alias_law()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
    assert len(alias_runs) == 3, f"PEP 695 alias subject did not inject through resolve: {len(alias_runs)} examples"
    with pytest.raises(TypeError, match="resolvable type form"):
        spec(consume_covers, law="probe-callable")(lambda: None)


def test_hypothesis_profile_registry_carries_lane_invariants() -> None:
    """Each registered lane profile carries the semantics its consumers budget against; a drifted row fails by name."""
    lanes = (PROFILE_DEFAULT, "rasm-ci", "rasm-stress", "rasm-debug", PROFILE_MUTATION, PROFILE_STATEFUL, "rasm-parity", "rasm-adversarial")
    profiles = {name: hyp_settings.get_profile(name) for name in lanes}
    support_matrix(
        ("mutation-derandomized-cache-free", lambda: profiles[PROFILE_MUTATION].derandomize and profiles[PROFILE_MUTATION].database is None, True),
        ("mutation-short-traces", lambda: profiles[PROFILE_MUTATION].stateful_step_count < profiles[PROFILE_STATEFUL].stateful_step_count, True),
        ("mutation-skips-shrink", lambda: Phase.shrink in profiles[PROFILE_MUTATION].phases, False),
        ("parity-byte-stable", lambda: profiles["rasm-parity"].derandomize and profiles["rasm-parity"].database is None, True),
        ("stress-hill-climbs", lambda: Phase.target in profiles["rasm-stress"].phases, True),
        ("adversarial-outbudgets-ci", lambda: profiles["rasm-adversarial"].max_examples > profiles["rasm-ci"].max_examples, True),
        ("default-replays-examples", lambda: profiles[PROFILE_DEFAULT].database is not None, True),
    )


# --- [COVERS_AND_AUTO_EXEMPTION]


class _Vocabulary(enum.StrEnum):
    PRIMARY = "primary"


class _FrozenRow(msgspec.Struct, frozen=True):
    field: int = 0


class _FrozenOwner(msgspec.Struct, frozen=True):
    field: int = 0

    def doubled(self) -> int:
        return self.field * 2


class _ValidatedRow(msgspec.Struct, frozen=True):
    field: int = 0

    def __post_init__(self) -> None:
        """Admission behavior: presence alone demands a law."""


class _MutableRow(msgspec.Struct):
    field: int = 0


class _Plain:
    pass


def test_covers_tuple_consumed_at_collection() -> None:
    """The runtime plugin folds this module's COVERS tuple into the manifest during collection."""
    assert any(rec.law == "covers" and rec.module == __name__ and rec.subject == "consume_covers" for rec in MANIFEST), (
        "COVERS declared above was not consumed — pytest_collection_modifyitems is not folding module COVERS tuples"
    )


@pytest.mark.parametrize(
    "subject, exempt",
    [
        pytest.param(_Vocabulary, True, id="strenum"),
        pytest.param(_FrozenRow, True, id="frozen-struct-method-free"),
        pytest.param(42, True, id="value-int"),
        pytest.param((1, 2), True, id="value-tuple"),
        pytest.param(ContextVar("seam"), True, id="value-contextvar"),
        pytest.param(msgspec.json.Decoder(int), True, id="value-codec"),
        pytest.param(_FrozenOwner, False, id="frozen-struct-with-method"),
        pytest.param(_ValidatedRow, False, id="frozen-struct-with-post-init"),
        pytest.param(_MutableRow, False, id="mutable-struct"),
        pytest.param(_Plain, False, id="plain-class"),
        pytest.param(consume_covers, False, id="function"),
    ],
)
def test_auto_exempt_partitions_symbol_kinds(subject: object, *, exempt: bool) -> None:
    """StrEnums, method-free frozen structs, and value-only objects are exempt; behavior-bearing symbols never are."""
    assert auto_exempt(subject) is exempt, f"auto_exempt({subject!r}) != {exempt}"


def test_consume_covers_registers_once_and_rejects_value_only(monkeypatch: pytest.MonkeyPatch) -> None:
    """COVERS consumption is idempotent per module and refuses value-only entries."""
    monkeypatch.setattr(laws_mod, "MANIFEST", [])
    monkeypatch.setattr(laws_mod, "_CONSUMED", set())
    module = SimpleNamespace(__name__="covers_probe", COVERS=(_FrozenOwner, consume_covers))
    consume_covers(module)
    consume_covers(module)
    assert [(rec.subject, rec.law) for rec in laws_mod.MANIFEST] == [("_FrozenOwner", "covers"), ("consume_covers", "covers")]

    monkeypatch.setattr(laws_mod, "_CONSUMED", set())
    with pytest.raises(TypeError, match="value-only"):
        consume_covers(SimpleNamespace(__name__="covers_bad", COVERS=(42,)))


# --- [MANIFEST_STRUCTURAL_LAWS]


def test_manifest_records_are_typed() -> None:
    """Every manifest entry is a ``LawRecord`` with non-empty identity fields."""
    if not MANIFEST:
        pytest.skip("MANIFEST is empty — no law files collected in this session")
    for rec in MANIFEST:
        assert isinstance(rec, LawRecord), f"non-LawRecord in MANIFEST: {rec!r}"
        assert rec.subject, f"empty subject in {rec!r}"
        assert rec.law, f"empty law name in {rec!r}"
        assert rec.module, f"empty module in {rec!r}"


def test_manifest_has_no_lambda_subjects() -> None:
    """No manifest record may use ``<lambda>`` as its subject."""
    if not MANIFEST:
        pytest.skip("MANIFEST is empty — no law files collected in this session")
    lambda_entries = [rec for rec in MANIFEST if "<lambda>" in rec.subject]
    assert not lambda_entries, f"anonymous-subject law records found: {lambda_entries!r}"


# --- [MARKER_POLICY_LAWS]


def test_declared_markers_cover_policy_set() -> None:
    """The policy marker set is declared in pyproject."""
    missing = _POLICY_MARKERS - _pytest_ini_marker_names()
    assert not missing, f"Policy markers not declared in the [tool.pytest] markers table: {missing}"


def test_mutation_marker_is_declared_not_unknown() -> None:
    """The ``mutation`` marker is registered for ``-m mutation`` selection."""
    assert "mutation" in _pytest_ini_marker_names(), "mutation marker not declared in the [tool.pytest] markers table"


def test_network_marker_auto_applied_to_socket_fixture_items(pytestconfig: pytest.Config) -> None:
    """Tests requesting ``socket_enabled`` carry the ``network`` marker after collection."""
    socket_items = [item for item in _collect_session_items(pytestconfig) if "socket_enabled" in getattr(item, "fixturenames", ())]
    for item in socket_items:
        assert item.get_closest_marker("network") is not None, (
            f"{item.nodeid!r} requests socket_enabled but lacks the 'network' marker — pytest_collection_modifyitems hook is not applying it"
        )


def test_property_marker_auto_applied_to_hypothesis_items(pytestconfig: pytest.Config) -> None:
    """Hypothesis-backed tests carry the ``property`` marker after collection."""
    hypothesis_items = [item for item in _collect_session_items(pytestconfig) if item.function is not None and is_hypothesis_test(item.function)]
    for item in hypothesis_items:
        assert item.get_closest_marker("property") is not None, (
            f"{item.nodeid!r} is a hypothesis test but lacks the 'property' marker — pytest_collection_modifyitems hook is not applying it"
        )


# --- [BENCHMARK_HOOK_POLICY]


def test_benchmark_regression_hook_is_registered(pytestconfig: pytest.Config) -> None:
    """The benchmark regression hook is live when pytest-benchmark is loaded."""
    hook = getattr(pytestconfig.pluginmanager.hook, "pytest_benchmark_update_json", None)
    if hook is None:
        pytest.skip("pytest-benchmark plugin not loaded — no pytest_benchmark_update_json hookspec")
    impl_names = [impl.plugin_name for impl in hook.get_hookimpls()]
    assert "testkit-bench" in impl_names, f"testkit-bench hookimpl missing; live impls: {impl_names}"


def test_bench_series_keying_is_file_disjoint(tmp_path: Path) -> None:
    """Stored entries with the same group and size remain disjoint by benchmark file."""
    machine = tmp_path / "store" / "machine"
    machine.mkdir(parents=True)
    entry_a = {"fullname": "tests/python/a/bench_a.py::bench_a[g-10]", "group": "g", "extra_info": {"size": 10}, "stats": {"median": 1.0}}
    entry_b = {"fullname": "tests/python/b/bench_b.py::bench_b[g-10]", "group": "g", "extra_info": {"size": 10}, "stats": {"median": 9.0}}
    (machine / "0001_run.json").write_bytes(msgspec.json.encode({"benchmarks": [entry_a]}))
    (machine / "0002_run.json").write_bytes(msgspec.json.encode({"benchmarks": [entry_b]}))
    config = create_autospec(pytest.Config, instance=True)
    config.getoption.return_value = "file://store"
    config.rootpath = tmp_path

    series = _series_from_storage(config, {"benchmarks": []})

    assert len(series) == 2, f"cross-file series merged: {series!r}"
    assert all(len(points) == 1 for points in series.values()), f"a series absorbed a foreign median: {series!r}"
    assert {key[0] for key in series} == {"tests/python/a/bench_a.py", "tests/python/b/bench_b.py"}


def test_sustained_regression_gate_fires_and_stays_silent(tmp_path: Path) -> None:
    """The stored-series fold fails a sustained final-segment jump and passes a flat history."""

    def doc(median: float) -> bytes:
        entry = {"fullname": "tests/python/x/bench_x.py::bench_x[g-10]", "group": "g", "extra_info": {"size": 10}, "stats": {"median": median}}
        return msgspec.json.encode({"benchmarks": [entry]})

    def storage(root: Path, medians: tuple[float, ...]) -> Mock:
        machine = root / "store" / "machine"
        machine.mkdir(parents=True)
        for index, median in enumerate(medians, start=1):
            (machine / f"{index:04}_run.json").write_bytes(doc(median))
        config: Mock = create_autospec(pytest.Config, instance=True)
        config.getoption.return_value = "file://store"
        config.rootpath = root
        return config

    stepped = storage(tmp_path / "stepped", (1.0, 1.0, 1.0, 1.0, 1.0, 1.0, 3.0, 3.0, 3.0))
    with pytest.raises(pytest.fail.Exception, match="sustained benchmark regression"):
        pytest_benchmark_update_json(stepped, None, msgspec.json.decode(doc(3.0)))

    flat = storage(tmp_path / "flat", (1.0,) * 9)
    pytest_benchmark_update_json(flat, None, msgspec.json.decode(doc(1.0)))


# --- [OBSERVABILITY_ROUTING_POLICY]


@pytest.mark.subprocess
def test_observability_gate_routes_hypothesis_observations_to_artifacts() -> None:
    """``TESTS_OBSERVABILITY`` grows the dated testcase JSONL with decodable rows; the unset gate writes nothing.

    The child pair is the wiring's falsification: an always-on callback fails the ungated arm, and a
    dead callback (or a broken internal hypothesis import path) fails the gated arm.
    """
    law = "tests/python/_testkit/test_strategies.py::test_literal_form_stays_inside_its_vocabulary"
    artifact = REPO_ROOT / ".artifacts" / "python" / "hypothesis" / f"{datetime.now(tz=UTC).date().isoformat()}_testcases.jsonl"

    def child(*, observed: bool) -> int:
        base = {name: value for name, value in os.environ.items() if name != "TESTS_OBSERVABILITY"}  # noqa: TID251  # subprocess env clone
        env = {**base, **({"TESTS_OBSERVABILITY": "1"} if observed else {})}
        spawn = functools.partial(anyio.run_process, env=env, cwd=str(REPO_ROOT), check=False)
        result = anyio.run(spawn, [sys.executable, "-m", "pytest", law, "-q"])
        assert result.returncode == 0, f"observability child failed: {result.stdout!r} {result.stderr!r}"
        return artifact.stat().st_size if artifact.exists() else 0

    initial = artifact.stat().st_size if artifact.exists() else 0
    assert child(observed=False) == initial, "testcase observations written without the TESTS_OBSERVABILITY gate"
    assert child(observed=True) > initial, "gated child wrote no testcase observations"
    decoded: object = msgspec.json.decode(artifact.read_bytes().splitlines()[-1])
    assert isinstance(decoded, dict) and decoded, f"artifact row is not a JSON object: {decoded!r}"


# --- [SNAPSHOT_RAIL_POLICY]


@pytest.mark.subprocess
def test_inline_snapshot_default_reports_without_mutating_and_fix_rewrites() -> None:
    """Default flags fail a stale snapshot without touching source; ``--inline-snapshot=fix`` rewrites it to green.

    The triple is the config's falsification: an auto-fixing default fails the report arm, a dead
    plugin fails the fix arm, and a cosmetic rewrite fails the green rerun.
    """
    probe_dir = REPO_ROOT / ".cache" / "kit-snapshot-probe" / uuid.uuid4().hex
    probe = probe_dir / "test_snapshot_probe.py"
    probe.parent.mkdir(parents=True, exist_ok=True)
    source = "from inline_snapshot import snapshot\n\n\ndef test_probe() -> None:\n    assert 1 + 1 == snapshot(3)\n"
    probe.write_text(source, encoding="utf-8")

    def child(*flags: str) -> int:
        spawn = functools.partial(anyio.run_process, cwd=str(REPO_ROOT), check=False)
        result = anyio.run(spawn, [sys.executable, "-m", "pytest", str(probe), "-q", *flags])
        return result.returncode

    try:
        assert child() != 0, "a stale snapshot must fail under the default report-only flags"
        assert probe.read_text(encoding="utf-8") == source, "report-only flags mutated the snapshot source"
        fix_rc = child("--inline-snapshot=fix")
        rewritten = probe.read_text(encoding="utf-8")
        assert "snapshot(2)" in rewritten, f"fix run (rc={fix_rc}) did not rewrite the stale snapshot literal: {rewritten!r}"
        assert child() == 0, "rewritten snapshot must pass under the default flags"
    finally:
        shutil.rmtree(probe_dir, ignore_errors=True)


# --- [LITTER_CONTAINMENT_POLICY]


def test_repo_root_has_only_allowlisted_entries() -> None:
    """Every repo-root entry is allowlisted by name or pattern, so any rogue tool write fails here by name."""
    unexpected = sorted(
        entry.name
        for entry in REPO_ROOT.iterdir()
        if entry.name not in _ROOT_ALLOWLIST and not any(fnmatch.fnmatch(entry.name, pattern) for pattern in _ROOT_PATTERNS)
    )
    assert not unexpected, f"unexpected repo-root entries (route tool output under .cache/ or .artifacts/, or review and allowlist): {unexpected}"


def test_package_manager_and_type_checker_caches_route_under_owned_roots() -> None:
    """Root-native package-manager and type-checker config routes generated storage under owned cache roots."""
    workspace = (REPO_ROOT / "pnpm-workspace.yaml").read_text(encoding="utf-8")
    mypy = _nav(_pyproject_data(), "tool", "mypy")
    assert isinstance(mypy, dict), "[tool.mypy] must own native mypy cache routing"
    assert mypy.get("cache_dir") == ".cache/mypy", "native mypy must never write .mypy_cache at repo root"
    assert "\nstoreDir: .cache/pnpm/store\n" in workspace, "native pnpm must never write .pnpm-store at repo root"
    assert "\ncacheDir: .cache/pnpm/cache\n" in workspace, "pnpm metadata cache must stay under .cache"
    assert "\nstateDir: .cache/pnpm/state\n" in workspace, "pnpm state must stay under .cache"
