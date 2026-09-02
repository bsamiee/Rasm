"""Tests for package coverage registration, pytest markers, hooks, and generated paths."""

# --- [IMPORTS] --------------------------------------------------------------------------

from contextvars import ContextVar
from datetime import datetime, timedelta, UTC
import enum
import fnmatch
import functools
import os
from pathlib import Path
import sys
import tomllib
from types import ModuleType, SimpleNamespace
from typing import TYPE_CHECKING
from unittest.mock import create_autospec

import anyio
from hypothesis import is_hypothesis_test, Phase, settings as hyp_settings
import msgspec
import pytest

from tests.python.support import properties as properties_module
from tests.python.support.assertions import capability_matrix
from tests.python.support.bench import _series_from_storage, pytest_benchmark_update_json
from tests.python.support.properties import assert_property_coverage, is_automatically_exempt, PackageUnderTest, property_test, PROPERTY_TESTS, PropertyRecord, record_coverage_declarations, register_package_tree, uncollected_test_modules
from tests.python.support.runtime import PROFILE_DEFAULT, PROFILE_STATEFUL, REPO_ROOT

if TYPE_CHECKING:
    from unittest.mock import Mock


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (record_coverage_declarations,)

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

_POLICY_MARKERS: frozenset[str] = frozenset({"benchmark", "network", "property", "subprocess"})

# --- [OPERATIONS] -----------------------------------------------------------------------


def _nav(node: dict[str, object], *keys: str) -> object:
    """Walk a nested TOML mapping by successive keys, ``None`` when a key is absent or an intermediate is not a mapping."""
    current: object = node
    for k in keys:
        match current:
            case {**mapping}:
                current = mapping.get(k)
            case _:
                return None
    return current


def _pyproject_data() -> dict[str, object]:
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    return data


def _pytest_ini_marker_names() -> frozenset[str]:
    markers: object = _nav(_pyproject_data(), "tool", "pytest", "markers")
    if not isinstance(markers, list):
        return frozenset()
    return frozenset(str(m).split(":")[0].strip() for m in markers)


def _collect_session_items(pytestconfig: pytest.Config) -> list[pytest.Function]:
    session: object = pytestconfig.pluginmanager.get_plugin("session")
    raw: object = getattr(session, "items", None)
    return [item for item in (raw if isinstance(raw, list) else []) if isinstance(item, pytest.Function)]


# --- [PROPERTY_TEST_COVERAGE]


def test_property_test_coverage() -> None:
    """Registered public APIs require a property test or explicit exemption, partially collected packages are skipped."""
    if not properties_module.PACKAGES_UNDER_TEST:
        pytest.skip("no package registered for property-test coverage")
    partial = uncollected_test_modules()
    assert_property_coverage(only=frozenset(properties_module.PACKAGES_UNDER_TEST) - frozenset(partial))
    if partial:
        detail = "; ".join(f"{package}: {', '.join(missing)}" for package, missing in sorted(partial.items()))
        pytest.skip(f"property-test coverage incomplete because test modules were not collected ({detail})")


def test_property_coverage_is_scoped_by_package(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """Property tests for a package do not cover another package's same-named symbol."""
    names = ("propertypkg_alpha", "propertypkg_beta")
    for name in names:
        pkg = tmp_path / name
        pkg.mkdir()
        (pkg / "__init__.py").write_text('__all__ = ["thing"]\n\n\ndef thing() -> None: ...\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))

    def _purge() -> None:
        [sys.modules.pop(name, None) for name in names]

    request.addfinalizer(_purge)

    record = PropertyRecord(subject="thing", property_name="alpha_thing_property", module=__name__, subject_module="propertypkg_alpha")
    monkeypatch.setattr(properties_module, "PROPERTY_TESTS", [record])
    monkeypatch.setattr(properties_module, "PACKAGES_UNDER_TEST", {"propertypkg_alpha": PackageUnderTest()})
    assert_property_coverage()

    monkeypatch.setattr(properties_module, "PACKAGES_UNDER_TEST", {"propertypkg_alpha": PackageUnderTest(), "propertypkg_beta": PackageUnderTest()})
    with pytest.raises(AssertionError, match="propertypkg_beta"):
        assert_property_coverage()


def test_property_coverage_detects_partial_collection(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """Uncollected test modules defer coverage checks, a real API gap still fails after full collection."""
    suite = tmp_path / "suite"
    suite.mkdir()
    (suite / "test_ghost.py").write_text("", encoding="utf-8")
    pkg = tmp_path / "propertypkg_partial"
    pkg.mkdir()
    (pkg / "__init__.py").write_text('__all__ = ["thing"]\n\n\ndef thing() -> None: ...\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))
    request.addfinalizer(lambda: sys.modules.pop("propertypkg_partial", None))

    monkeypatch.setattr(properties_module, "PROPERTY_TESTS", [])
    monkeypatch.setattr(properties_module, "PACKAGES_UNDER_TEST", {"propertypkg_partial": PackageUnderTest(suite=suite)})
    missing = uncollected_test_modules()["propertypkg_partial"]
    assert missing, "on-disk test module not reported as uncollected"
    with pytest.raises(pytest.skip.Exception, match="coverage incomplete"):
        test_property_test_coverage()

    for name in missing:
        monkeypatch.setitem(sys.modules, name, ModuleType(name))
    assert "propertypkg_partial" not in uncollected_test_modules(), "coverage remained partial after every test module was imported"
    with pytest.raises(AssertionError, match="propertypkg_partial"):
        test_property_test_coverage()


def test_register_package_tree_registers_only_python_packages(tmp_path: Path, monkeypatch: pytest.MonkeyPatch) -> None:
    """Package discovery follows Python source layouts and ignores directories without Python modules."""
    source = tmp_path / "src"
    (source / "alpha").mkdir(parents=True)
    (source / "alpha" / "__init__.py").write_text("", encoding="utf-8")
    (source / "planning_only").mkdir()
    (source / "unregistered.py").write_text("", encoding="utf-8")
    (source / "packaged" / "src" / "ns" / "pkg").mkdir(parents=True)
    (source / "packaged" / "src" / "ns" / "pkg" / "__init__.py").write_text("", encoding="utf-8")
    suites = tmp_path / "suites"
    monkeypatch.setattr(properties_module, "PACKAGES_UNDER_TEST", {})

    assert register_package_tree(source, suites) == ("alpha", "ns.pkg"), "package registration did not match the source layout"
    assert properties_module.PACKAGES_UNDER_TEST["alpha"].suite == suites / "alpha", "test-directory derivation failed"
    assert properties_module.PACKAGES_UNDER_TEST["ns.pkg"].suite == suites / "packaged", "src layout registered the wrong package test directory"
    assert register_package_tree(tmp_path / "absent", suites) == (), "a missing source root must register nothing"


def test_registered_packages_have_test_directories() -> None:
    """Each package registration identifies an existing test directory."""
    if not properties_module.PACKAGES_UNDER_TEST:
        pytest.skip("no package registered for property-test coverage")
    for package, registration in properties_module.PACKAGES_UNDER_TEST.items():
        assert registration.suite is not None and registration.suite.is_dir(), f"{package} registered without an existing test directory: {registration.suite!r}"


def test_test_module_names_match_live_session_imports(pytestconfig: pytest.Config) -> None:
    """Derived module names match pytest importlib-mode names used by the live session."""
    test_files = {path for item in _collect_session_items(pytestconfig) if (path := Path(item.path)).is_relative_to(REPO_ROOT) and any(fnmatch.fnmatch(path.name, pattern) for pattern in properties_module._TEST_FILE_GLOBS)}
    assert test_files, "no test modules collected in this session"
    unloaded_modules = sorted(str(path) for path in test_files if properties_module._module_name(path) not in sys.modules)
    assert not unloaded_modules, f"derived test module names absent from sys.modules, naming differs from pytest importlib mode: {unloaded_modules}"


def test_phantom_export_fails_the_census_not_silently_exempt(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """Undefined ``__all__`` entries are public-API inspection failures, not automatic exemptions."""
    pkg = tmp_path / "propertypkg_phantom"
    pkg.mkdir()
    (pkg / "__init__.py").write_text('__all__ = ["ghost"]\n', encoding="utf-8")
    monkeypatch.syspath_prepend(str(tmp_path))
    request.addfinalizer(lambda: sys.modules.pop("propertypkg_phantom", None))
    monkeypatch.setattr(properties_module, "PROPERTY_TESTS", [])
    monkeypatch.setattr(properties_module, "PACKAGES_UNDER_TEST", {"propertypkg_phantom": PackageUnderTest()})
    with pytest.raises(AssertionError, match="ghost"):
        assert_property_coverage()


# --- [PROPERTY_TEST_SETTINGS]


def test_property_test_uses_profiles_timeout_and_event_labels(monkeypatch: pytest.MonkeyPatch) -> None:
    """Property tests use the active or named profile, interpret timeout as seconds, and label generated examples."""
    monkeypatch.setattr(properties_module, "PROPERTY_TESTS", [])
    runs: list[int] = []
    pinned_runs: list[int] = []
    tagged: list[int] = []

    def _tag(drawn: object) -> str:
        tagged.append(drawn) if isinstance(drawn, int) else None
        return f"n={drawn}"

    hyp_settings.register_profile("test-support-probe", max_examples=3, deadline=None, database=None, derandomize=True)
    prior = hyp_settings.get_current_profile_name()
    hyp_settings.load_profile("test-support-probe")
    try:

        @property_test(int, property_name="probe-follows", events=(_tag,))
        def probe(n: int) -> None:
            runs.append(n)

        @property_test(int, profile="rasm-parity", property_name="probe-pinned")
        def pinned(n: int) -> None:
            pinned_runs.append(n)

        probe()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
        pinned()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
    finally:
        hyp_settings.load_profile(prior)
    assert len(runs) == 3, f"property test ignored the active profile: {len(runs)} examples"
    assert tagged == runs, f"events tagger missed drawn examples: tagged {tagged}, ran {runs}"
    assert len(pinned_runs) == hyp_settings.get_profile("rasm-parity").max_examples, f"named profile used the wrong example count: {len(pinned_runs)}"

    @property_test(int, given=False, timeout=2.5, property_name="probe-deadline")
    def bounded() -> None: ...

    resolved: object = getattr(bounded, "_hypothesis_internal_use_settings", None)
    assert getattr(resolved, "deadline", None) == timedelta(seconds=2.5), f"timeout=2.5 must mean seconds, resolved {resolved!r}"

    with pytest.raises(TypeError, match="applied twice"):
        property_test(int, property_name="probe-duplicate")(probe)

    type Pair = tuple[int, int]
    alias_runs: list[tuple[int, int]] = []

    @property_test(Pair, profile="test-support-probe", property_name="probe-alias")
    def alias_property(pair: tuple[int, int]) -> None:
        alias_runs.append(pair)

    alias_property()  # type: ignore[call-arg]  # ty: ignore[missing-argument]
    assert len(alias_runs) == 3, f"PEP 695 alias subject did not receive a generated strategy: {len(alias_runs)} examples"
    with pytest.raises(TypeError, match="resolvable type form"):
        property_test(record_coverage_declarations, property_name="probe-callable")(lambda: None)


def test_hypothesis_profiles_preserve_required_settings() -> None:
    """Each registered profile retains the settings required by its use case."""
    profile_names = (PROFILE_DEFAULT, "rasm-ci", "rasm-stress", "rasm-debug", PROFILE_STATEFUL, "rasm-parity", "rasm-adversarial")
    profiles = {name: hyp_settings.get_profile(name) for name in profile_names}
    capability_matrix(
        ("parity-byte-stable", lambda: profiles["rasm-parity"].derandomize and profiles["rasm-parity"].database is None, True),
        ("stress-hill-climbs", lambda: Phase.target in profiles["rasm-stress"].phases, True),
        ("adversarial-outbudgets-ci", lambda: profiles["rasm-adversarial"].max_examples > profiles["rasm-ci"].max_examples, True),
        ("default-replays-examples", lambda: profiles[PROFILE_DEFAULT].database is not None, True),
    )


# --- [COVERS_AND_AUTO_EXEMPTION]


class _Role(enum.StrEnum):
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
        """Validate construction when the field is present."""


class _MutableRow(msgspec.Struct):
    field: int = 0


class _Plain:
    pass


def test_covers_tuple_recorded_at_collection() -> None:
    """The runtime plugin records this module's COVERS declaration during collection."""
    assert any(record.property_name == "covers" and record.module == __name__ and record.subject == "record_coverage_declarations" for record in PROPERTY_TESTS), "COVERS declaration was not recorded during collection"


@pytest.mark.parametrize(
    "subject, exempt",
    [
        pytest.param(_Role, True, id="strenum"),
        pytest.param(_FrozenRow, True, id="frozen-struct-method-free"),
        pytest.param(42, True, id="value-int"),
        pytest.param((1, 2), True, id="value-tuple"),
        pytest.param(ContextVar("probe"), True, id="value-contextvar"),
        pytest.param(msgspec.json.Decoder(int), True, id="value-codec"),
        pytest.param(_FrozenOwner, False, id="frozen-struct-with-method"),
        pytest.param(_ValidatedRow, False, id="frozen-struct-with-post-init"),
        pytest.param(_MutableRow, False, id="mutable-struct"),
        pytest.param(_Plain, False, id="plain-class"),
        pytest.param(record_coverage_declarations, False, id="function"),
    ],
)
def test_automatic_exemption_classifies_public_symbols(subject: object, *, exempt: bool) -> None:
    """StrEnums, method-free frozen structs, and value-only objects are exempt, behavior-bearing symbols never are."""
    assert is_automatically_exempt(subject) is exempt, f"is_automatically_exempt({subject!r}) != {exempt}"


def test_record_coverage_declarations_is_idempotent_and_rejects_values(monkeypatch: pytest.MonkeyPatch) -> None:
    """COVERS consumption is idempotent per module and rejects value-only entries."""
    monkeypatch.setattr(properties_module, "PROPERTY_TESTS", [])
    monkeypatch.setattr(properties_module, "_CONSUMED", set())
    module = SimpleNamespace(__name__="covers_probe", COVERS=(_FrozenOwner, record_coverage_declarations))
    record_coverage_declarations(module)
    record_coverage_declarations(module)
    assert [(record.subject, record.property_name) for record in properties_module.PROPERTY_TESTS] == [("_FrozenOwner", "covers"), ("record_coverage_declarations", "covers")]

    monkeypatch.setattr(properties_module, "_CONSUMED", set())
    with pytest.raises(TypeError, match="types or callables"):
        record_coverage_declarations(SimpleNamespace(__name__="covers_bad", COVERS=(42,)))


# --- [PROPERTY_RECORDS]


def test_property_records_have_named_subjects_and_modules() -> None:
    """Every property record has a subject, property name, and module, subjects cannot be anonymous lambdas."""
    if not PROPERTY_TESTS:
        pytest.skip("no property tests were recorded in this session")
    for record in PROPERTY_TESTS:
        assert record.subject and record.property_name and record.module, f"empty property record field in {record!r}"
        assert "<lambda>" not in record.subject, f"anonymous property-test subject: {record!r}"


# --- [MARKER_POLICY]


def test_declared_markers_cover_policy_set() -> None:
    """The policy marker set is declared in pyproject."""
    missing = _POLICY_MARKERS - _pytest_ini_marker_names()
    assert not missing, f"Policy markers not declared in the [tool.pytest] markers table: {missing}"


def test_network_marker_auto_applied_to_socket_fixture_items(pytestconfig: pytest.Config) -> None:
    """Tests requesting ``socket_enabled`` receive the ``network`` marker during collection."""
    socket_items = [item for item in _collect_session_items(pytestconfig) if "socket_enabled" in getattr(item, "fixturenames", ())]
    for item in socket_items:
        assert item.get_closest_marker("network") is not None, f"{item.nodeid!r} requests socket_enabled but lacks the 'network' marker, pytest_collection_modifyitems hook is not applying it"


def test_property_marker_auto_applied_to_hypothesis_items(pytestconfig: pytest.Config) -> None:
    """Hypothesis-backed tests receive the ``property`` marker during collection."""
    hypothesis_items = [item for item in _collect_session_items(pytestconfig) if item.function is not None and is_hypothesis_test(item.function)]
    for item in hypothesis_items:
        assert item.get_closest_marker("property") is not None, f"{item.nodeid!r} is a hypothesis test but lacks the 'property' marker, pytest_collection_modifyitems hook is not applying it"


# --- [BENCHMARK_HOOK_POLICY]


def test_benchmark_regression_hook_is_registered(pytestconfig: pytest.Config) -> None:
    """The benchmark regression hook is live when pytest-benchmark is loaded."""
    hook = getattr(pytestconfig.pluginmanager.hook, "pytest_benchmark_update_json", None)
    if hook is None:
        pytest.skip("pytest-benchmark plugin not loaded, no pytest_benchmark_update_json hookspec")
    impl_names = [impl.plugin_name for impl in hook.get_hookimpls()]
    assert "test-support-bench" in impl_names, f"test-support-bench hook implementation missing, registered implementations: {impl_names}"


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


def test_sustained_regression_check_fails_on_step_change_and_accepts_flat_history(tmp_path: Path) -> None:
    """Regression detection fails a sustained final-segment increase and accepts a flat history."""

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


# --- [OBSERVABILITY_OUTPUT]


@pytest.mark.subprocess
def test_observability_flag_writes_hypothesis_observations_to_artifacts() -> None:
    """``TESTS_OBSERVABILITY`` writes decodable observations, without it the artifact is unchanged."""
    test_node = "tests/python/support/test_strategies.py::test_literal_form_generates_only_declared_values"
    artifact = REPO_ROOT / ".artifacts" / "python" / "hypothesis" / f"{datetime.now(tz=UTC).date().isoformat()}_testcases.jsonl"

    def child(*, observed: bool) -> int:
        base = {name: value for name, value in os.environ.items() if name != "TESTS_OBSERVABILITY"}  # ruff:ignore[banned-api]
        env = {**base, **({"TESTS_OBSERVABILITY": "1"} if observed else {})}
        spawn = functools.partial(anyio.run_process, env=env, cwd=str(REPO_ROOT), check=False)
        result = anyio.run(spawn, [sys.executable, "-m", "pytest", test_node, "-q"])
        assert result.returncode == 0, f"observability child failed: {result.stdout!r} {result.stderr!r}"
        return artifact.stat().st_size if artifact.exists() else 0

    initial = artifact.stat().st_size if artifact.exists() else 0
    assert child(observed=False) == initial, "testcase observations written while TESTS_OBSERVABILITY was disabled"
    assert child(observed=True) > initial, "enabled child wrote no testcase observations"
    decoded: object = msgspec.json.decode(artifact.read_bytes().splitlines()[-1])
    assert isinstance(decoded, dict) and decoded, f"artifact row is not a JSON object: {decoded!r}"


# --- [GENERATED_STORAGE]


def test_package_manager_and_type_checker_caches_route_under_owned_roots() -> None:
    """Root package-manager and type-checker configuration routes generated data to configured cache directories."""
    workspace = (REPO_ROOT / "pnpm-workspace.yaml").read_text(encoding="utf-8")
    mypy = _nav(_pyproject_data(), "tool", "mypy")
    assert isinstance(mypy, dict), "[tool.mypy] must define native mypy cache routing"
    assert mypy.get("cache_dir") == ".cache/mypy", "native mypy must never write .mypy_cache at repo root"
    assert "\nstoreDir: .cache/pnpm/store\n" in workspace, "native pnpm must never write .pnpm-store at repo root"
    assert "\ncacheDir: .cache/pnpm/cache\n" in workspace, "pnpm metadata cache must stay under .cache"
    assert "\nstateDir:" not in workspace, "pnpm stateDir is retired, pnpm has no supported state-directory setting"
