"""Generic, SUT-agnostic policy meta-tests parameterized over the registered ``SUT_PACKAGES``.

This layer imports NO system-under-test: assay-bound couplings (benchmark-storage URI identity, the
catalog-import surface) live in ``tests.python.tools.assay.test_policy_assay``. Three orthogonal rails:

1. Law-coverage gate: calls ``assert_law_coverage()`` in-session after all laws have been collected
   and registered into ``MANIFEST``. Every public symbol of every package registered via ``register_sut``
   (the ``SUT_PACKAGES`` table) must have at least one law or an explicit exemption in that package's
   conftest ``exempt`` set — the gate is total across all SUTs, not one named package.

2. Marker-policy laws: assert the ``pytest_collection_modifyitems`` hook auto-applies ``network``
   to tests that request ``socket_enabled`` and ``property`` to hypothesis-backed tests. The ``mutation``
   marker is declared in ``pyproject.toml``; its registration is verified structurally.

3. Litter-containment policy: no test tool writes its scratch to the repo root.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from pathlib import Path  # noqa: TC003  # module-level _PYPROJECT assignment prevents deferral
import sys
import tomllib
from unittest.mock import create_autospec

from hypothesis import is_hypothesis_test
import msgspec
import pytest

from tests.python._testkit import laws as laws_mod
from tests.python._testkit.bench import _series_from_storage
from tests.python._testkit.laws import assert_law_coverage, LawRecord, MANIFEST, SUT_PACKAGES
from tests.python._testkit.runtime import REPO_ROOT


# --- [CONSTANTS] ------------------------------------------------------------------------

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

_POLICY_MARKERS: frozenset[str] = frozenset({"benchmark", "mutation", "network", "property"})

# mutants/ at the repo ROOT is litter: mutmut hard-codes Path("mutants") relative to its cwd, and the
# sanctioned mutation posture runs staged (cwd = .artifacts/python/mutmut/work), so root mutants/ can only
# come from a manual card run launched from the repo root — forbidden.
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
    """Return the bare marker names declared under ``[tool.pytest] markers`` (e.g. ``{"benchmark", "network"}``); empty when absent."""
    markers: object = _nav(_pyproject_data(), "tool", "pytest", "markers")
    if not isinstance(markers, list):
        return frozenset()
    return frozenset(str(m).split(":")[0].strip() for m in markers)


def _collect_session_items(pytestconfig: pytest.Config) -> list[pytest.Function]:
    """Return collected ``Function`` items from the current session; empty when ``session.items`` is absent."""
    session: object = pytestconfig.pluginmanager.get_plugin("session")
    raw: object = getattr(session, "items", None)  # B009: session has no public stub; attribute is a fixed string, not computed
    return [item for item in (raw if isinstance(raw, list) else []) if isinstance(item, pytest.Function)]


# --- [LAW_COVERAGE_GATE]


def test_law_coverage_gate() -> None:
    """Every public symbol of every registered SUT package has >=1 law in MANIFEST or is exempt.

    The gate is total across the ``SUT_PACKAGES`` table (each ``register_sut`` call adds one package and
    its exempt set), so a second SUT growing into the monorepo is covered with no edit to this generic
    law. Skips only when no SUT is registered (isolated single-file collection).

    Partial-selection sessions fail by design: when a registering conftest loads but only a subset of
    law files is collected, ``MANIFEST`` misses the deselected modules' laws and the surface walk reports
    their subjects as uncovered. That false failure is intended — the gate is a full-suite invariant,
    not a per-file one; run the full ``tests/python`` collection for an authoritative verdict.

    Falsifiable by: adding a new public symbol to any registered SUT module ``__all__`` without authoring
    a corresponding law — the gate finds it in the surface walk and fails with the symbol name.
    """
    if not SUT_PACKAGES:
        pytest.skip("no SUT registered — register_sut was not called in this collection")
    assert_law_coverage()


def test_law_coverage_is_package_scoped_not_name_global(tmp_path: Path, monkeypatch: pytest.MonkeyPatch, request: pytest.FixtureRequest) -> None:
    """A law on package A's ``thing`` never covers package B's same-named ``thing``.

    Synthetic two-package collision: both packages export ``thing``; only A carries a law (its
    ``subject_module`` resolves inside A). The gate passes with A alone registered and fails the
    moment B joins — under a single global covered-name set (the historical defect) B would
    falsely pass on A's law. Registrations are faked against scoped copies of ``MANIFEST`` /
    ``SUT_PACKAGES`` via monkeypatch; the synthetic modules are purged from ``sys.modules`` on teardown.
    """
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


# --- [MANIFEST_STRUCTURAL_LAWS]


def test_manifest_records_are_typed() -> None:
    """Every entry in MANIFEST is a frozen LawRecord with non-empty subject, law, and module fields.

    Skips in isolated mode (MANIFEST is empty because no law files were collected alongside this one).
    In full-suite mode each collected law file populates MANIFEST via ``@spec`` / ``register_law``.

    Falsifiable by: a corrupt or hand-constructed registration that injects an invalid record type,
    an empty subject name, or an empty law name.
    """
    if not MANIFEST:
        pytest.skip("MANIFEST is empty — no law files collected in this session")
    for rec in MANIFEST:
        assert isinstance(rec, LawRecord), f"non-LawRecord in MANIFEST: {rec!r}"
        assert rec.subject, f"empty subject in {rec!r}"
        assert rec.law, f"empty law name in {rec!r}"
        assert rec.module, f"empty module in {rec!r}"


def test_manifest_has_no_lambda_subjects() -> None:
    """No MANIFEST entry may record ``<lambda>`` as its subject.

    Skips in isolated mode (no law files collected). In full-suite mode any ``@spec(lambda ...)`` call
    would surface here.

    Falsifiable by: passing an anonymous lambda to ``@spec`` or ``register_law`` instead of a named type.
    ``<lambda>`` entries are law-less — the gate keys on simple-names and ``<lambda>`` is not a meaningful
    SUT symbol name.
    """
    if not MANIFEST:
        pytest.skip("MANIFEST is empty — no law files collected in this session")
    lambda_entries = [rec for rec in MANIFEST if "<lambda>" in rec.subject]
    assert not lambda_entries, f"anonymous-subject law records found: {lambda_entries!r}"


# --- [MARKER_POLICY_LAWS]


def test_declared_markers_cover_policy_set() -> None:
    """The four policy markers (benchmark/mutation/network/property) are declared in pyproject.toml.

    Falsifiable by: removing a policy marker from ``[tool.pytest.ini_options] markers`` — pytest would
    warn on an unknown mark and the policy set becomes out-of-sync with the hook's auto-application logic.
    """
    missing = _POLICY_MARKERS - _pytest_ini_marker_names()
    assert not missing, f"Policy markers not declared in ini_options.markers: {missing}"


def test_mutation_marker_is_declared_not_unknown() -> None:
    """The ``mutation`` marker is registered so mutation runs are selectable via ``-m mutation``.

    Falsifiable by: removing the ``mutation`` entry from ``[tool.pytest.ini_options] markers`` in
    pyproject.toml — pytest emits ``PytestUnknownMarkWarning`` (error under ``filterwarnings=["error"]``)
    when any test uses ``@pytest.mark.mutation``.
    """
    assert "mutation" in _pytest_ini_marker_names(), "mutation marker not declared in [tool.pytest.ini_options] markers"


def test_network_marker_auto_applied_to_socket_fixture_items(pytestconfig: pytest.Config) -> None:
    """Tests that request ``socket_enabled`` carry the ``network`` marker after collection.

    Falsifiable by: removing the ``socket_enabled`` branch from ``pytest_collection_modifyitems``
    — socket-gated tests become invisible to ``-m network`` selection.
    """
    socket_items = [item for item in _collect_session_items(pytestconfig) if "socket_enabled" in getattr(item, "fixturenames", ())]
    for item in socket_items:
        assert item.get_closest_marker("network") is not None, (
            f"{item.nodeid!r} requests socket_enabled but lacks the 'network' marker — pytest_collection_modifyitems hook is not applying it"
        )


def test_property_marker_auto_applied_to_hypothesis_items(pytestconfig: pytest.Config) -> None:
    """Tests backed by hypothesis carry the ``property`` marker after collection.

    Falsifiable by: removing the ``is_hypothesis_test`` branch from ``pytest_collection_modifyitems``
    — property-based tests become invisible to ``-m property`` selection.
    """
    hypothesis_items = [item for item in _collect_session_items(pytestconfig) if item.function is not None and is_hypothesis_test(item.function)]
    for item in hypothesis_items:
        assert item.get_closest_marker("property") is not None, (
            f"{item.nodeid!r} is a hypothesis test but lacks the 'property' marker — pytest_collection_modifyitems hook is not applying it"
        )


# --- [BENCHMARK_HOOK_POLICY]


def test_benchmark_regression_hook_is_registered(pytestconfig: pytest.Config) -> None:
    """The bench module's ``pytest_benchmark_update_json`` hookimpl is live on the plugin manager.

    ``runtime.pytest_configure`` registers ``tests.python._testkit.bench`` under the plugin name
    ``testkit-bench``; without that registration the Potts/BIC sustained-regression gate is dead code
    that no session ever calls — this law makes silent hook-death structurally impossible. Skips when
    pytest-benchmark itself is unloaded (``-p no:benchmark`` xdist posture): the hookspec then does
    not exist, so there is nothing to implement.

    Falsifiable by: dropping the ``pytest_configure`` registration from the runtime plugin — the
    hookimpl list loses the ``testkit-bench`` entry.
    """
    hook = getattr(pytestconfig.pluginmanager.hook, "pytest_benchmark_update_json", None)
    if hook is None:
        pytest.skip("pytest-benchmark plugin not loaded — no pytest_benchmark_update_json hookspec")
    impl_names = [impl.plugin_name for impl in hook.get_hookimpls()]
    assert "testkit-bench" in impl_names, f"testkit-bench hookimpl missing; live impls: {impl_names}"


def test_bench_series_keying_is_file_disjoint(tmp_path: Path) -> None:
    """Two stored entries sharing (group, size) but originating from different benchmark files never merge.

    The regression series key is ``(fullname file part, group, size)``: a second SUT package's bench
    file reusing a group label under the one policy-mandated storage root keeps its own median series.

    Falsifiable by: keying on ``(group, size)`` alone (the historical defect) — both medians would
    fold into one two-point series and a step in package B's timing would gate package A.
    """
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


# --- [LITTER_CONTAINMENT_POLICY]


def test_repo_root_has_no_tool_scratch_litter() -> None:
    """No test tool writes its scratch to the repo root; every home routes under .cache/ or .artifacts/.

    Falsifiable by: a tool (hypothesis, pytest-benchmark, coverage, mutmut, approvaltests) regressing its
    store to the repo root instead of its routed home — the offending dir name would surface here.
    """
    present = sorted(name for name in _LITTER_DIRS if (REPO_ROOT / name).exists())
    assert not present, f"repo root collected test-tool litter (route each under .cache/ or .artifacts/): {present}"
