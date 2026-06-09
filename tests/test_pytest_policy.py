"""Policy meta-tests: law-coverage gate, marker-policy laws, benchmark-storage single-owner policy.

Three orthogonal rails:

1. **Law-coverage gate** — calls ``assert_law_coverage()`` in-session after all laws have been collected
   and registered into ``MANIFEST``.  Every public ``tools.assay`` symbol must have at least one law or
   an explicit exemption recorded in the assay conftest's ``_EXEMPT`` frozenset.

2. **Marker-policy laws** — assert the ``pytest_collection_modifyitems`` hook auto-applies ``network``
   to tests that request ``socket_enabled`` and ``property`` to hypothesis-backed tests.  The ``mutation``
   marker is declared in ``pyproject.toml``; its registration is verified structurally.

3. **Benchmark-storage single-owner policy** — the ``--benchmark-storage`` URI must appear in exactly
   one place (``[tool.pytest.ini_options] addopts``) and must match the constant declared in
   ``tools.assay.composition.catalog``.  Duplication across ``conftest.py`` files or per-session
   ``benchmark.pedantic`` calls is forbidden.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import importlib
from pathlib import Path  # noqa: TC003  # used at module level for _PYPROJECT constant; cannot defer to TYPE_CHECKING
import tomllib

from _pytest.python import Function  # noqa: PLC2701  # internal pytest type; no public re-export; required for isinstance check
from hypothesis import is_hypothesis_test
import pytest

from tests._aspect import assert_law_coverage, LawRecord, MANIFEST  # noqa: PLC2701  # sibling test-internal module; _-named by S1 design
from tests.conftest import REPO_ROOT
from tools.assay.composition.catalog import BENCHMARK_STORAGE_URI


# --- [CONSTANTS] ------------------------------------------------------------------------

_PYPROJECT: Path = REPO_ROOT / "pyproject.toml"

# Policy markers that must be declared in [tool.pytest.ini_options] markers.
_POLICY_MARKERS: frozenset[str] = frozenset({"benchmark", "mutation", "network", "property"})

# Tool scratch dirs that must never collect at the repo root: each tool home is routed under .cache/ or
# .artifacts/ (hypothesis → .cache/hypothesis, pytest-benchmark → .artifacts/python/benchmarks, coverage →
# .cache/coverage/.coverage, inline-snapshot → .cache/inline-snapshot).
_LITTER_DIRS: frozenset[str] = frozenset({".hypothesis", ".benchmarks", ".mutants", "mutants", ".coverage", ".approval_tests_temp"})


# --- [OPERATIONS] -----------------------------------------------------------------------


def _nav(node: dict[str, object], *keys: str) -> object:
    """Walk a nested TOML mapping by successive string keys; returns None on any miss or type mismatch.

    Returns:
        The value found at the final key, or ``None`` if any key is absent or an intermediate value
        is not a mapping.
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
    """Load the root pyproject.toml as a plain string-keyed mapping.

    Returns:
        TOML document as a ``dict[str, object]`` (toml values are always str-keyed at top level).
    """
    # tomllib returns dict[str, Any] at runtime; the annotation is intentionally widened to object
    # so callers never rely on implicit Any propagation.
    data: dict[str, object] = tomllib.loads(_PYPROJECT.read_text(encoding="utf-8"))
    return data


def _pytest_ini_addopts() -> list[str]:
    """Return the ``[tool.pytest.ini_options] addopts`` list as a flat string list.

    The TOML section ``[tool.pytest.ini_options]`` maps to the key path ``tool.pytest`` in the parsed
    document; ``ini_options`` is the TOML section suffix, not a nested key.

    Returns:
        List of addopts strings; empty if the key is absent.
    """
    addopts: object = _nav(_pyproject_data(), "tool", "pytest", "addopts")
    return [str(o) for o in addopts] if isinstance(addopts, list) else []


def _pytest_ini_marker_names() -> frozenset[str]:
    """Return the simple marker names declared under ``[tool.pytest.ini_options] markers``.

    Returns:
        Frozenset of bare marker names (e.g. ``{"benchmark", "mutation", "network", "property"}``).
    """
    markers: object = _nav(_pyproject_data(), "tool", "pytest", "markers")
    if not isinstance(markers, list):
        return frozenset()
    return frozenset(str(m).split(":")[0].strip() for m in markers)


def _benchmark_storage_flags(addopts: list[str]) -> list[str]:
    """Filter addopts to only ``--benchmark-storage`` flags.

    Returns:
        Sublist of entries that start with ``--benchmark-storage``.
    """
    return [o for o in addopts if o.startswith("--benchmark-storage")]


def _collect_session_items(pytestconfig: pytest.Config) -> list[Function]:
    """Return collected ``Function`` items from the current session if available.

    Returns:
        List of ``_pytest.python.Function`` items; empty when the session has no ``items`` attr.
    """
    session: object = pytestconfig.pluginmanager.get_plugin("session")
    # _pytest.main.Session has no public stub; access .items via getattr to avoid ty attribute errors.
    raw: object = getattr(session, "items", None)  # constant attr name; B009 suppressed: no stub on session type forces getattr over direct access
    return [item for item in (raw if isinstance(raw, list) else []) if isinstance(item, Function)]


# --- [LAW_COVERAGE_GATE] ----------------------------------------------------------------


def test_law_coverage_gate() -> None:
    """Every public tools.assay symbol has >=1 law in MANIFEST or is in the exempt set.

    Falsifiable by: adding a new public symbol to any tools.assay module ``__all__`` without
    authoring a corresponding law (the gate finds it in the surface walk and fails with the symbol name).
    """
    assert_law_coverage()


# --- [MANIFEST_STRUCTURAL_LAWS] ---------------------------------------------------------


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


# --- [MARKER_POLICY_LAWS] ---------------------------------------------------------------


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


# --- [BENCHMARK_STORAGE_POLICY] ---------------------------------------------------------


def test_benchmark_storage_single_owner_in_addopts() -> None:
    """``--benchmark-storage`` appears in addopts exactly once.

    Falsifiable by: adding a second ``--benchmark-storage=...`` flag in addopts or a duplicate inline
    pytest invocation — the single-owner policy breaks and the storage path becomes ambiguous.
    """
    flags = _benchmark_storage_flags(_pytest_ini_addopts())
    assert len(flags) == 1, f"Expected exactly one --benchmark-storage in addopts, found {len(flags)}: {flags!r}"


def test_benchmark_storage_uri_matches_catalog_constant() -> None:
    """The ``--benchmark-storage`` URI in addopts equals ``catalog.BENCHMARK_STORAGE_URI``.

    Falsifiable by: updating the catalog constant without updating pyproject.toml, or vice versa —
    the benchmark runner would write to a different directory than the catalog's storage declaration.
    """
    flags = _benchmark_storage_flags(_pytest_ini_addopts())
    assert flags, "--benchmark-storage not found in addopts (prerequisite for URI-match law)"
    _, _, uri = flags[0].partition("=")
    assert uri == BENCHMARK_STORAGE_URI, f"addopts URI {uri!r} != catalog.BENCHMARK_STORAGE_URI {BENCHMARK_STORAGE_URI!r}"


def test_benchmark_storage_not_duplicated_in_conftest_files() -> None:
    """No conftest.py file under ``tests/`` carries a ``--benchmark-storage`` flag.

    Falsifiable by: adding a per-suite ``pytest.ini_options`` or ``addopts`` override in a sub-conftest
    that duplicates or overrides the benchmark storage path — single-owner policy breaks silently.
    """
    violators: list[str] = [
        str(cf.relative_to(REPO_ROOT)) for cf in (REPO_ROOT / "tests").rglob("conftest.py") if "--benchmark-storage" in cf.read_text(encoding="utf-8")
    ]
    assert not violators, f"--benchmark-storage found in conftest file(s) outside addopts: {violators!r}"


def test_catalog_module_exposes_benchmark_storage_uri() -> None:
    """``tools.assay.composition.catalog`` is importable and ``BENCHMARK_STORAGE_URI`` is a non-empty str.

    Falsifiable by: renaming or removing ``BENCHMARK_STORAGE_URI`` from catalog.py — the single-owner
    policy tests above would also fail, but this law isolates the catalog as the canonical declaration.
    """
    mod = importlib.import_module("tools.assay.composition.catalog")
    uri: object = getattr(mod, "BENCHMARK_STORAGE_URI", None)
    assert isinstance(uri, str), f"BENCHMARK_STORAGE_URI is absent or not str: {uri!r}"
    assert uri, f"BENCHMARK_STORAGE_URI is empty: {uri!r}"


# --- [LITTER_CONTAINMENT_POLICY] --------------------------------------------------------


def test_repo_root_has_no_tool_scratch_litter() -> None:
    """No test tool writes its scratch to the repo root; every home routes under .cache/ or .artifacts/.

    Falsifiable by: a tool (hypothesis, pytest-benchmark, coverage, mutmut, approvaltests) regressing its
    store to the repo root instead of its routed home — the offending dir name would surface here.
    """
    present = sorted(name for name in _LITTER_DIRS if (REPO_ROOT / name).exists())
    assert not present, f"repo root collected test-tool litter (route each under .cache/ or .artifacts/): {present}"
