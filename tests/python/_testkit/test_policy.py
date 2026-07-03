"""SUT-agnostic policy meta-tests over registered packages, markers, hooks, and scratch paths."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from contextvars import ContextVar
import enum
from pathlib import Path  # noqa: TC003  # module-level _PYPROJECT assignment prevents deferral
import sys
import tomllib
from types import SimpleNamespace
from typing import TYPE_CHECKING
from unittest.mock import create_autospec

from hypothesis import is_hypothesis_test
import msgspec
import pytest

from tests.python._testkit import laws as laws_mod
from tests.python._testkit.bench import _series_from_storage, pytest_benchmark_update_json
from tests.python._testkit.laws import assert_law_coverage, auto_exempt, consume_covers, LawRecord, MANIFEST, SUT_PACKAGES
from tests.python._testkit.runtime import REPO_ROOT


if TYPE_CHECKING:
    from unittest.mock import Mock


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (consume_covers,)

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

_POLICY_MARKERS: frozenset[str] = frozenset({"benchmark", "mutation", "network", "property"})

# Root mutants/ means mutmut ran from the repository root instead of the staged mutation workdir.
_LITTER_DIRS: frozenset[str] = frozenset({".hypothesis", ".benchmarks", ".coverage", ".approval_tests_temp", "mutants", ".mutants"})

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
    """Registered SUT public symbols must have a law or an explicit exemption."""
    if not SUT_PACKAGES:
        pytest.skip("no SUT registered — register_sut was not called in this collection")
    assert_law_coverage()


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
    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_alpha": frozenset()})
    assert_law_coverage()

    monkeypatch.setattr(laws_mod, "SUT_PACKAGES", {"lawpkg_alpha": frozenset(), "lawpkg_beta": frozenset()})
    with pytest.raises(AssertionError, match="lawpkg_beta"):
        assert_law_coverage()


# --- [COVERS_AND_AUTO_EXEMPTION]


class _Vocabulary(enum.StrEnum):
    PRIMARY = "primary"


class _FrozenRow(msgspec.Struct, frozen=True):
    field: int = 0


class _FrozenOwner(msgspec.Struct, frozen=True):
    field: int = 0

    def doubled(self) -> int:
        return self.field * 2


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
    assert not missing, f"Policy markers not declared in ini_options.markers: {missing}"


def test_mutation_marker_is_declared_not_unknown() -> None:
    """The ``mutation`` marker is registered for ``-m mutation`` selection."""
    assert "mutation" in _pytest_ini_marker_names(), "mutation marker not declared in [tool.pytest.ini_options] markers"


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


# --- [LITTER_CONTAINMENT_POLICY]


def test_repo_root_has_no_tool_scratch_litter() -> None:
    """Test tools must route scratch directories under configured cache/artifact homes."""
    present = sorted(name for name in _LITTER_DIRS if (REPO_ROOT / name).exists())
    assert not present, f"repo root collected test-tool litter (route each under .cache/ or .artifacts/): {present}"
