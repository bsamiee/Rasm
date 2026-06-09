"""Laws for tools.assay.core.routing: route, place, routable_files, Routed, RoutePaths, ProjectIndex, Scope, Source."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from __future__ import annotations

from typing import override, TYPE_CHECKING

from expression import Error, Ok
from hypothesis import given, HealthCheck, settings as h_settings, strategies as st, target
import msgspec.structs
import pytest

from tests._aspect import register_law, spec  # noqa: PLC2701  # sibling test-internal module; `_`-named by S1 design
from tests._spec import assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase  # noqa: PLC2701  # sibling test-internal module
from tools.assay.core.model import Claim, Fault, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import place, routable_files, route, Routed, Scope, Source
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression import Result

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.routing import ProjectIndex, RoutePaths


# --- [TYPES] ----------------------------------------------------------------------------


class _StubSource(Source):
    """Source double: returns the injected changed/universe sets; read yields a valid empty project."""

    def __init__(self, changed: tuple[str, ...], universe: tuple[str, ...] = ()) -> None:
        self._changed = changed
        self._universe = universe

    @override
    def changed(self) -> Result[tuple[str, ...], Fault]:
        return Ok(self._changed)

    @override
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        return Ok(self._universe or paths)

    @override
    def read(self, rel: str) -> Result[bytes, Fault]:
        _ = rel
        return Ok(b"<Project />")


class _FaultingSource(Source):
    """Source double whose changed/enumerate rails always fault — propagates Error through route."""

    @override
    def changed(self) -> Result[tuple[str, ...], Fault]:
        return Error(Fault(("git",), RailStatus.FAULTED, "git missing"))

    @override
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        _ = paths
        return Error(Fault(("fd",), RailStatus.FAULTED, "fd missing"))

    @override
    def read(self, rel: str) -> Result[bytes, Fault]:
        _ = rel
        return Ok(b"")


# --- [CONSTANTS] ------------------------------------------------------------------------

# Shared Python-language tool fixture: FILES/CHECK — covers routable_files filtering via place.
_PY_TOOL = Tool(
    name="py-check", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)

# Probe-fixture prefixes declared by AssaySettings — drives the S2 seam parametrize law.
_DEFAULT_PREFIXES: tuple[str, ...] = ("tests/tools/ast-grep/", "tests/tools/py_analyzer/")


# --- [OPERATIONS] -----------------------------------------------------------------------


# --- Scope / Source enum sweeps ---


register_law(Scope, "scope_enum_sweep")


@pytest.mark.parametrize("member", list(Scope))
def test_scope_enum_sweep(member: Scope) -> None:
    """Every Scope member round-trips through its string value (StrEnum identity law)."""
    assert Scope(member.value) is member


register_law(Source, "source_protocol_runtime_checkable")


def test_source_is_runtime_checkable() -> None:
    """Source is @runtime_checkable — isinstance checks are supported for Protocol conformance."""
    assert isinstance(_StubSource(("a.py",)), Source)
    assert not isinstance(object(), Source)


# --- Routed structural invariants ---


@spec(Routed)
def test_routed_defaults(instance: Routed) -> None:
    """Routed is frozen; files/projects/groups/full_triggers are always tuples of the right element type."""
    assert isinstance(instance.language, Language)
    assert isinstance(instance.scope, Scope)
    assert isinstance(instance.files, tuple)
    assert isinstance(instance.projects, tuple)
    assert isinstance(instance.groups, tuple)
    assert isinstance(instance.full_triggers, tuple)


register_law(Routed, "routed_replace_yields_new_instance")


def test_routed_replace_yields_new_instance() -> None:
    """msgspec.structs.replace on a frozen Routed produces a distinct object — structural copy semantics."""
    r = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=("a.py",))
    r2 = msgspec.structs.replace(r, files=("b.py",))
    assert r2 is not r
    assert r2.files == ("b.py",)
    assert r.files == ("a.py",)


register_law(Routed, "routed_full_triggers_scope_consistency")


@pytest.mark.parametrize("scope,full_triggers,files", [(Scope.FULL, ("Workspace.slnx",), ("Workspace.slnx",)), (Scope.CHANGED, (), ("src/A.cs",))])
def test_routed_full_triggers_scope_consistency(scope: Scope, full_triggers: tuple[str, ...], files: tuple[str, ...]) -> None:
    """full_triggers is non-empty iff scope is FULL — the escalation invariant."""
    r = Routed(language=Language.CSHARP, scope=scope, files=files, full_triggers=full_triggers)
    assert (r.scope is Scope.FULL) == (len(r.full_triggers) > 0)


# --- ProjectIndex / RoutePaths structural laws ---


register_law("ProjectIndex", "project_index_mapping")


def test_project_index_is_mapping() -> None:
    """ProjectIndex is a Mapping[str, str] — dict satisfies the contract."""
    index: ProjectIndex = {"src/Lib": "src/Lib/Lib.csproj", "src/App": "src/App/App.csproj"}
    assert isinstance(index["src/Lib"], str)
    assert len(index) == 2


register_law("RoutePaths", "route_paths_tuple_of_str")


def test_route_paths_is_tuple_of_str() -> None:
    """RoutePaths is a tuple[str, ...] — str membership is structurally enforced."""
    paths: RoutePaths = ("a.py", "b.ts", "src/App.csproj")
    assert all(isinstance(p, str) for p in paths)


# --- route: determinism + scope escalation ---


register_law(route, "route_language_table")


@pytest.mark.parametrize(
    "language,changed,universe,expected_scope,n_files,full_triggers",
    [
        (Language.PYTHON, ("a.py", "b.ts"), (), Scope.CHANGED, 1, ()),
        (Language.TYPESCRIPT, ("a.py", "b.ts"), (), Scope.CHANGED, 1, ()),
        (Language.CSHARP, ("Workspace.slnx",), ("src/A.csproj",), Scope.FULL, 1, ("Workspace.slnx",)),
        (Language.CSHARP, ("src/A.cs",), ("src/A.csproj",), Scope.CHANGED, 1, ()),
        (Language.BASH, ("deploy.sh", "notes.md"), (), Scope.CHANGED, 1, ()),
        (Language.SQL, ("schema.sql", "schema.py"), (), Scope.CHANGED, 1, ()),
        (Language.DOCS, ("CHANGELOG.md", "schema.py"), (), Scope.CHANGED, 1, ()),
    ],
    ids=["py-glob", "ts-glob", "csharp-full-trigger", "csharp-changed", "bash-glob", "sql-glob", "docs-glob"],
)
def test_route_language_table(
    language: Language,
    changed: tuple[str, ...],
    universe: tuple[str, ...],
    expected_scope: Scope,
    n_files: int,
    full_triggers: tuple[str, ...],
    assay_root: AssayHarness,
) -> None:
    """route() suffix-filters by language and escalates trigger files to FULL scope."""
    result = route(language, source=_StubSource(changed, universe=universe), settings=assay_root.settings)
    routed = assert_ok(result)
    assert routed.scope is expected_scope
    assert len(routed.files) == n_files
    assert routed.full_triggers == full_triggers


register_law(route, "route_determinism")


def test_route_determinism(assay_root: AssayHarness) -> None:
    """route() is pure: two calls with identical inputs produce structurally equal Routed outputs."""
    source = _StubSource(("a.py", "b.py"))
    r1 = assert_ok(route(Language.PYTHON, source=source, settings=assay_root.settings))
    r2 = assert_ok(route(Language.PYTHON, source=source, settings=assay_root.settings))
    assert r1 == r2


register_law(route, "route_propagates_source_fault")


def test_route_propagates_source_fault(assay_root: AssayHarness) -> None:
    """A faulting Source short-circuits the route bind-chain into a FAULTED Error."""
    result = route(Language.PYTHON, source=_FaultingSource(), settings=assay_root.settings)
    assert_error_status(result, RailStatus.FAULTED)


register_law(route, "route_empty_changeset_returns_ok")


@pytest.mark.parametrize("language", list(Language))
def test_route_empty_changeset_returns_ok(language: Language, assay_root: AssayHarness) -> None:
    """An empty changeset produces an Ok Routed (never an error) for every Language."""
    result = route(language, source=_StubSource(()), settings=assay_root.settings)
    assert_ok(result)


register_law(route, "route_files_are_normalized")


def test_route_files_are_normalized(assay_root: AssayHarness) -> None:
    """route() normalises and deduplicates paths — duplicate entries collapse to one."""
    source = _StubSource(("./a.py", "a.py", "b.py"))
    routed = assert_ok(route(Language.PYTHON, source=source, settings=assay_root.settings))
    assert len(routed.files) == len(set(routed.files)), "duplicate paths leaked through normalisation"


# --- place: input-arm table ---


register_law(place, "place_input_arm_table")


@pytest.mark.parametrize(
    "input_mode,mode,routed,expected",
    [
        (Input.FILES, Mode.CHECK, Routed(Language.PYTHON, Scope.CHANGED, files=("a.py",)), (("a.py",),)),
        (Input.FILES, Mode.CHECK, Routed(Language.PYTHON, Scope.CHANGED), ()),
        (Input.INCLUDE, Mode.WRITE, Routed(Language.CSHARP, Scope.CHANGED, groups=(("A.csproj", ("a.cs",)),)), (("A.csproj", "--include", "a.cs"),)),
        (Input.PROJECT, Mode.RUN, Routed(Language.CSHARP, Scope.CHANGED, projects=("A.csproj",)), (("A.csproj",),)),
        (Input.NONE, Mode.CHECK, Routed(Language.PYTHON, Scope.CHANGED, files=("a.py",)), (("a.py",),)),
        (Input.NONE, Mode.CHECK, Routed(Language.PYTHON, Scope.CHANGED), ((),)),
    ],
    ids=["files-present", "files-empty", "include-arm", "project-arm", "none-with-files", "none-empty"],
)
def test_place_input_arm_table(
    input_mode: Input, mode: Mode, routed: Routed, expected: tuple[tuple[str, ...], ...], assay_root: AssayHarness
) -> None:
    """Each Input arm projects routed data into a stable argv tail (full matrix, 6 arms)."""
    tool = Tool("t", Runner.DIRECT, ("tool",), input_mode, routed.language, Claim.STATIC, mode=mode)
    assert place(routed, tool, settings=assay_root.settings) == expected


register_law(place, "place_solution_arm")


def test_place_solution_arm(assay_root: AssayHarness) -> None:
    """The SOLUTION arm projects the settings solution path, independent of routed inputs."""
    tool = Tool("sln", Runner.DOTNET, ("dotnet",), Input.SOLUTION, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    routed = Routed(Language.CSHARP, Scope.FULL)
    assert place(routed, tool, settings=assay_root.settings) == ((str(assay_root.settings.solution),),)


register_law(place, "place_determinism")


def test_place_determinism(assay_root: AssayHarness) -> None:
    """place() is pure — two calls with equal inputs produce equal argv tails."""
    routed = Routed(Language.PYTHON, Scope.CHANGED, files=("a.py", "b.py"))
    assert place(routed, _PY_TOOL, settings=assay_root.settings) == place(routed, _PY_TOOL, settings=assay_root.settings)


# --- place: proportional placement (O(N) projection law) ---


register_law(place, "place_proportional_by_inspection")


@h_settings(max_examples=200, suppress_health_check=[HealthCheck.function_scoped_fixture])
@given(k=st.integers(min_value=0, max_value=2000))
def test_place_proportional_by_inspection(k: int, assay_root: AssayHarness) -> None:
    """place() projects all k files into exactly one argv tail — O(N) by construction, not chunked."""
    files = tuple(f"pkg/mod_{i}.py" for i in range(k))
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=files)
    result = place(routed, _PY_TOOL, settings=assay_root.settings)
    target(float(k), label="placed_file_count")
    assert result == (files,) if k else result == (), f"expected one argv tail with {k} files, got {result!r}"


# --- routable_files: probe-fixture prefix stripping ---


register_law(routable_files, "routable_files_strips_prefix")


@pytest.mark.parametrize("prefix", _DEFAULT_PREFIXES)
def test_routable_files_strips_prefix(prefix: str, assay_root: AssayHarness) -> None:
    """routable_files removes every file whose path starts with a configured probe_fixture_prefix."""
    probe = f"{prefix}fail/helper_import.py"
    legitimate = "tools/assay/core/model.py"
    result = routable_files((probe, legitimate), assay_root.settings)
    assert probe not in result
    assert legitimate in result


register_law(routable_files, "routable_files_strips_all_default_prefixes")


def test_routable_files_strips_all_default_prefixes(assay_root: AssayHarness) -> None:
    """routable_files removes every file matching any of the default probe_fixture_prefixes."""
    probes = tuple(f"{pfx}sample.py" for pfx in _DEFAULT_PREFIXES)
    legit: tuple[str, ...] = ("tools/assay/core/routing.py",)
    result = routable_files(probes + legit, assay_root.settings)
    assert result == legit


register_law(routable_files, "routable_files_passthrough_when_no_probes")


def test_routable_files_passthrough_when_no_probes(assay_root: AssayHarness) -> None:
    """routable_files is identity when no file matches any probe_fixture_prefix."""
    files: tuple[str, ...] = ("tools/assay/core/model.py", "tests/csharp/libs/Rasm/ModelTests.cs")
    assert routable_files(files, assay_root.settings) == files


register_law(routable_files, "routable_files_custom_prefixes")


def test_routable_files_custom_prefixes(assay_root: AssayHarness) -> None:
    """routable_files honours settings.probe_fixture_prefixes, not a hardcoded constant (S2 seam)."""
    custom_settings = assay_root.settings.model_copy(update={"probe_fixture_prefixes": ("custom/probe/",)})
    files: tuple[str, ...] = ("custom/probe/fail.py", "tools/assay/core/routing.py")
    result = routable_files(files, custom_settings)
    assert "custom/probe/fail.py" not in result
    assert "tools/assay/core/routing.py" in result


register_law(place, "place_strips_probe_fixtures_via_files_input")


@pytest.mark.parametrize("extra_prefix", list(_DEFAULT_PREFIXES))
def test_place_strips_probe_fixtures_via_files_input(extra_prefix: str, assay_root: AssayHarness) -> None:
    """place(INPUT.FILES) delegates to routable_files — probe-fixture paths are absent from argv."""
    probe = f"{extra_prefix}fail/helper_import.py"
    legit = "tools/assay/core/model.py"
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=(probe, legit))
    result = place(routed, _PY_TOOL, settings=assay_root.settings)
    argv_flat = tuple(arg for tail in result for arg in tail)
    assert probe not in argv_flat
    assert legit in argv_flat


# --- validity matrix: Scope values ---


register_law(Scope, "scope_validity_matrix")


def _try_scope(v: str) -> bool:
    # Inline predicate — only called inline in the validity_matrix oracle below.
    try:
        Scope(v)
    except ValueError:
        return False
    return True


def test_scope_validity_matrix() -> None:
    """All Scope members are valid StrEnum instances; non-members raise ValueError."""
    validity_matrix(
        [
            ValidityCase(label="changed", value="changed", expected=True),
            ValidityCase(label="full", value="full", expected=True),
            ValidityCase(label="invalid", value="INVALID", expected=False),
            ValidityCase(label="empty", value="", expected=False),
        ],
        _try_scope,
    )


# --- support matrix: Source protocol conformance ---


register_law(Source, "source_protocol_support_matrix")


def test_source_protocol_support_matrix() -> None:
    """Support matrix: conforming/non-conforming Source implementations are distinguished at runtime."""
    support_matrix(
        ("stub_source_conforms", lambda: isinstance(_StubSource(()), Source), True),
        ("faulting_source_conforms", lambda: isinstance(_FaultingSource(), Source), True),
        ("plain_object_rejected", lambda: isinstance(object(), Source), False),
        ("dict_rejected", lambda: isinstance({}, Source), False),
    )
