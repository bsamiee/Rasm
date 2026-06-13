"""Law suite for tools.assay.core.routing.

Covers Scope StrEnum identity and validity, Source Protocol runtime-checkability,
Routed structural invariants and scope-consistency, ProjectIndex and RoutePaths
type contracts, route() language-table dispatch / determinism / fault propagation /
normalisation, target_files() partitioning, place() Input-arm projection / solution arm /
determinism / proportionality / probe-fixture stripping, and routable_files() prefix filtering.

Symbols under test: route, place, routable_files, Routed, RoutePaths, ProjectIndex,
Scope, Source, TargetFiles, target_files.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from __future__ import annotations

from typing import ClassVar, override, TYPE_CHECKING

from expression import Error, Ok
from hypothesis import given, HealthCheck, settings as h_settings, strategies as st, target
import msgspec.structs
import pytest
from upath import UPath

from tests.python._testkit.laws import register_law, spec
from tests.python._testkit.spec import assert_error_status, assert_ok, support_matrix, validity_matrix, ValidityCase
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import (  # private probes: read/parse degradation arms are unreachable through public route fixtures
    _LocalSource,
    _owner,
    _refs,
    expand,
    infer_languages,
    place,
    routable_files,
    route,
    Routed,
    Scope,
    Source,
    target_files,
    TargetFiles,
)
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from pathlib import Path

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
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


class _GraphSource(Source):
    """Source double over a fixed three-project graph App -> Lib -> Core (backslash-style Include).

    ``read`` returns real ``.csproj`` bytes carrying ``ProjectReference Include`` entries so that
    ``_refs`` must parse, ``_dependents`` must walk the transitive fixed-point, and ``_resolve`` must
    project closure/groups; ``enumerate`` yields the full universe regardless of the requested paths.
    """

    _UNIVERSE: ClassVar[tuple[str, ...]] = ("src/Core/Core.csproj", "src/Lib/Lib.csproj", "src/App/App.csproj", "src/Core/c.cs", "src/App/a.cs")
    _CSPROJ: ClassVar[dict[str, bytes]] = {
        "src/Core/Core.csproj": b"<Project></Project>",
        "src/Lib/Lib.csproj": b'<Project><ItemGroup><ProjectReference Include="..\\Core\\Core.csproj" /></ItemGroup></Project>',
        "src/App/App.csproj": b'<Project><ItemGroup><ProjectReference Include="..\\Lib\\Lib.csproj" /></ItemGroup></Project>',
    }

    def __init__(self, changed: tuple[str, ...]) -> None:
        self._changed = changed

    @override
    def changed(self) -> Result[tuple[str, ...], Fault]:
        return Ok(self._changed)

    @override
    def enumerate(self, paths: RoutePaths) -> Result[tuple[str, ...], Fault]:
        _ = paths
        return Ok(self._UNIVERSE)

    @override
    def read(self, rel: str) -> Result[bytes, Fault]:
        return Ok(self._CSPROJ.get(rel, b"<Project />"))


class _HostGraphSource(_GraphSource):
    """_GraphSource variant where App alone carries the explicit AssayHostBound marker."""

    _CSPROJ: ClassVar[dict[str, bytes]] = {
        **_GraphSource._CSPROJ,
        "src/App/App.csproj": (
            b"<Project><PropertyGroup><AssayHostBound>true</AssayHostBound></PropertyGroup>"
            b'<ItemGroup><ProjectReference Include="..\\Lib\\Lib.csproj" /></ItemGroup></Project>'
        ),
    }


# --- [CONSTANTS] ------------------------------------------------------------------------

_HOST_ROUTED = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj", "src/Lib/Lib.csproj"), host_bound=("src/App/App.csproj",))

_PY_TOOL = Tool(
    name="py-check", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)

# Mirrors AssaySettings.probe_fixture_prefixes — keeps prefix-stripping law parametrization in sync with settings.
_DEFAULT_PREFIXES: tuple[str, ...] = ("tests/ast-grep/", "tests/python/tools/py_analyzer/")

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [SCOPE_SOURCE_SWEEPS]

register_law(Scope, "scope_enum_sweep")


@pytest.mark.parametrize("member", list(Scope))
def test_scope_enum_sweep(member: Scope) -> None:
    """Every Scope member round-trips through its string value (StrEnum identity law)."""
    assert Scope(member.value) is member


register_law(Scope, "scope_validity_matrix")


def _try_scope(v: str) -> bool:
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


register_law(Source, "source_protocol_runtime_checkable")


def test_source_is_runtime_checkable() -> None:
    """Source is @runtime_checkable — isinstance checks are supported for Protocol conformance."""
    assert isinstance(_StubSource(("a.py",)), Source)
    assert not isinstance(object(), Source)


register_law(Source, "source_protocol_support_matrix")


def test_source_protocol_support_matrix() -> None:
    """Support matrix: conforming/non-conforming Source implementations are distinguished at runtime."""
    support_matrix(
        ("stub_source_conforms", lambda: isinstance(_StubSource(()), Source), True),
        ("faulting_source_conforms", lambda: isinstance(_FaultingSource(), Source), True),
        ("plain_object_rejected", lambda: isinstance(object(), Source), False),
        ("dict_rejected", lambda: isinstance({}, Source), False),
    )


# --- [TARGETFILES_LAWS]

register_law(TargetFiles, "targetfiles_defaults_are_empty_tuples")


def test_targetfiles_defaults_are_empty_tuples() -> None:
    """TargetFiles defaults are tuple-shaped so static detail can serialize without None repair."""
    targets = TargetFiles()
    assert targets.targets == ()
    assert targets.files == ()
    assert targets.rejected == ()
    assert targets.changed is False


register_law(target_files, "target_files_partitions_unsupported_inputs")


def test_target_files_partitions_unsupported_inputs(assay_root: AssayHarness) -> None:
    """target_files keeps supported source rows and rejects project, solution, and root-trigger rows."""
    source = _StubSource((), universe=("src/App/App.csproj", "src/App/a.cs", "Directory.Build.props"))
    targets = assert_ok(target_files(("src/App",), ("Workspace.slnx", "single.py"), source=source, settings=assay_root.settings))
    assert targets.targets == (("folder", "src/App"), ("file", "Workspace.slnx"), ("file", "single.py"))
    assert targets.files == ("single.py", "src/App/a.cs")
    assert targets.rejected == (
        ("folder", "Directory.Build.props", "full-trigger"),
        ("folder", "src/App/App.csproj", "project-or-solution"),
        ("file", "Workspace.slnx", "project-or-solution"),
    )


# --- [ROUTED_INVARIANTS]


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


# --- [PROJECTINDEX_ROUTEPATHS]

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


# --- [ROUTE_LAWS]

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


# --- [PLACE_LAWS]

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
        (Input.OWNED, Mode.CHECK, Routed(Language.DOCS, Scope.CHANGED, files=("a.md", "b.md")), ((),)),
        (Input.OWNED, Mode.CHECK, Routed(Language.DOCS, Scope.CHANGED), ((),)),
    ],
    ids=["files-present", "files-empty", "include-arm", "project-arm", "none-with-files", "none-empty", "owned-with-files", "owned-empty"],
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


# --- [PLACE_PROPORTIONAL]

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


# --- [ROUTABLE_FILES_LAWS]

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


# --- [LAWS_INFER_LANGUAGES]

register_law(infer_languages, "infer_languages_narrows_by_suffix")


def test_infer_languages_narrows_by_suffix() -> None:
    """infer_languages keeps only available languages whose suffix sets intersect the given paths."""
    assert infer_languages(("tests/python/tools/assay/core/test_routing.py",), tuple(Language)) == (Language.PYTHON,)


register_law(infer_languages, "infer_languages_fallback_all_available")


def test_infer_languages_fallback_all_available() -> None:
    """Empty paths and suffixless paths (directories) both fall back to the full available tuple."""
    available = (Language.PYTHON, Language.TYPESCRIPT)
    support_matrix(
        ("empty paths → all available", lambda: infer_languages((), available) == available, True),
        ("directory path → all available", lambda: infer_languages(("tests",), available) == available, True),
        ("unmatched suffix → all available", lambda: infer_languages(("src/App.cs",), available) == available, True),
    )


register_law(infer_languages, "infer_languages_preserves_available_order")


def test_infer_languages_preserves_available_order() -> None:
    """Multi-suffix paths yield matched languages in available order, never path order."""
    result = infer_languages(("src/view.tsx", "tools/assay/__init__.py"), tuple(Language))
    assert result == (Language.PYTHON, Language.TYPESCRIPT)


register_law(infer_languages, "infer_languages_never_exceeds_available")


@given(st.lists(st.sampled_from([".py", ".ts", ".cs", ".sh", ".sql", ".md", ""]), max_size=8))
def test_infer_languages_never_exceeds_available(suffixes: list[str]) -> None:
    """For any path multiset the result is a non-empty, order-preserving subsequence of available."""
    paths = tuple(f"dir/file{i}{suffix}" for i, suffix in enumerate(suffixes))
    available = tuple(Language)
    result = infer_languages(paths, available)
    assert result
    assert tuple(language for language in available if language in result) == result


register_law(route, "local_source_read_and_refs_degrade_isolated")


def test_local_source_read_and_refs_degrade_isolated(tmp_path: Path) -> None:
    """The closure-graph degradation ladder: read faults and malformed .csproj XML collapse to isolated nodes.

    ``_LocalSource.read`` returns Ok(bytes) for a present file and a FAULTED Fault (never a raised OSError)
    for a missing one; ``_refs`` folds both an unreadable and a malformed .csproj to an empty reference set.
    Falsified by a read that raises through, a Fault carrying the wrong argv provenance, or a ParseError
    escaping ``_parse`` instead of degrading to an empty Project element.
    """
    src = _LocalSource(root=UPath(tmp_path))
    (tmp_path / "good.bin").write_bytes(b"payload")
    assert assert_ok(src.read("good.bin")) == b"payload"
    fault = assert_error_status(src.read("missing.csproj"), RailStatus.FAULTED)
    assert fault.argv == ("read", "missing.csproj")
    assert _refs("missing.csproj", src) == frozenset()
    (tmp_path / "bad.csproj").write_bytes(b"<not-xml")
    assert _refs("bad.csproj", src) == frozenset()


# --- [CLOSURE_GRAPH_LAWS]

register_law(_refs, "refs_resolves_project_references")


@pytest.mark.parametrize(
    "csproj,expected",
    [
        ("src/Lib/Lib.csproj", frozenset({"src/Core/Core.csproj"})),
        ("src/App/App.csproj", frozenset({"src/Lib/Lib.csproj"})),
        ("src/Core/Core.csproj", frozenset()),
    ],
    ids=["lib-refs-core", "app-refs-lib", "core-leaf"],
)
def test_refs_resolves_project_references(csproj: str, expected: frozenset[str]) -> None:
    r"""_refs parses ProjectReference Include attributes, normalising backslash-relative paths to project-rooted refs.

    Each ``.csproj`` carries a single Windows-style ``Include="..\\X\\X.csproj"``; _refs must read the file, locate
    every ``ProjectReference`` element, pull its ``Include`` attribute, swap backslash for slash, join against the
    declaring project's parent directory, and ``normpath`` the join to the exact root-relative reference path.
    Falsifies the entire _refs happy-path cluster: deleting the ok-arm (always empty), nulling ``base`` (TypeError or
    wrong root), nulling/garbling ``normpath``/``str`` args, mutating the ``"\\"``→``"/"`` replace literals, renaming
    the ``"Include"`` attribute key (case-flips, XX-markers), or inverting the ``is not None`` guard — every one alters
    which project edges the closure graph carries, so each produces a reference set that diverges from ``expected``.
    """
    assert _refs(csproj, _GraphSource(())) == expected


register_law(_owner, "owner_selects_deepest_project")


@pytest.mark.parametrize(
    "rel,expected",
    [("src/Lib/Sub/x.cs", "src/Lib/Sub"), ("src/Lib/y.cs", "src/Lib"), ("src/Other/z.cs", None)],
    ids=["nested-deepest-wins", "parent-only", "unowned"],
)
def test_owner_selects_deepest_project(rel: str, expected: str | None) -> None:
    """_owner returns the longest (deepest) owning project directory, not an arbitrary or insertion-order match.

    With nested index keys ``src/Lib`` and ``src/Lib/Sub`` both relative-to a file under ``src/Lib/Sub``, the file is
    owned by the deeper project. Falsifies the _owner cluster: dropping ``key=str.__len__`` (or nulling it) makes
    ``max`` compare the directory strings lexicographically rather than by length, so ``src/Lib`` can win over
    ``src/Lib/Sub`` and a changed file is attributed to the wrong project — the seed and group projection both shift.
    """
    index: ProjectIndex = {"src/Lib": "src/Lib/Lib.csproj", "src/Lib/Sub": "src/Lib/Sub/Sub.csproj"}
    assert _owner(rel, index) == expected


register_law(route, "route_closure_transitive_dependents")


def test_route_closure_transitive_dependents(assay_root: AssayHarness) -> None:
    """A change to the deepest project pulls its full transitive reverse-dependency closure into ``projects``.

    The graph is App -> Lib -> Core; changing ``src/Core/c.cs`` must rebuild Core, Lib, AND App because both
    transitively reference Core. Falsifies the _dependents monotone-fixed-point cluster: union->intersection
    (closure shrinks to the seed), ``and``->``or`` (unrelated nodes leak in), ``p not in current``->``p in current``
    (no node is ever added), ``bool(current & refs)``->``bool(None)`` (no dependents pulled), and
    ``current & refs``->``current | refs`` (every node pulled regardless of edges) each yield a ``projects`` set that
    differs from the true ``{Core, Lib, App}`` closure.
    """
    routed = assert_ok(route(Language.CSHARP, source=_GraphSource(("src/Core/c.cs",)), settings=assay_root.settings))
    assert routed.scope is Scope.CHANGED
    assert routed.projects == ("src/App/App.csproj", "src/Core/Core.csproj", "src/Lib/Lib.csproj")


register_law(route, "route_closure_groups_pair_seed_with_owned_files")


def test_route_closure_groups_pair_seed_with_owned_files(assay_root: AssayHarness) -> None:
    """_resolve pairs each seed project with exactly the changed files it owns, sorted, and threads language through.

    Two changed files land in two different seed projects; ``groups`` must associate each seed ``.csproj`` with the
    files whose ``_owner`` resolves to that project, and ``files`` is the normalised owned set. Falsifies the _resolve
    cluster: ``groups=None`` / ``groups=`` default (groups vanish), the ``index[owner] == owner_proj`` filter flipped
    to ``!=`` (each seed paired with every OTHER seed's files), ``language=None`` / ``projects=None`` / ``projects=``
    default (the carried projection fields drop or null out).
    """
    routed = assert_ok(route(Language.CSHARP, source=_GraphSource(("src/Core/c.cs", "src/App/a.cs")), settings=assay_root.settings))
    assert routed.language is Language.CSHARP
    assert routed.files == ("src/App/a.cs", "src/Core/c.cs")
    assert routed.groups == (("src/App/App.csproj", ("src/App/a.cs",)), ("src/Core/Core.csproj", ("src/Core/c.cs",)))


register_law(route, "route_escalation_honours_settings_triggers")


@pytest.mark.parametrize(
    "update,changed,expected_scope",
    [
        ({"trigger_files": frozenset({"custom-trigger.txt"}), "trigger_prefixes": ()}, ("custom-trigger.txt",), Scope.FULL),
        ({"trigger_files": frozenset({"custom-trigger.txt"}), "trigger_prefixes": ()}, ("Directory.Build.props",), Scope.CHANGED),
        ({"trigger_files": frozenset(), "trigger_prefixes": ("custom/gen/",)}, ("custom/gen/build.cs",), Scope.FULL),
        ({"trigger_files": frozenset(), "trigger_prefixes": ("custom/gen/",)}, ("tools/cs-analyzer/Rule.cs",), Scope.CHANGED),
    ],
    ids=["custom-file-escalates", "default-file-ignored", "custom-prefix-escalates", "default-prefix-ignored"],
)
def test_route_escalation_honours_settings_triggers(
    update: dict[str, object], changed: tuple[str, ...], expected_scope: Scope, assay_root: AssayHarness
) -> None:
    """FULL-scope escalation consults the active settings' trigger_files/trigger_prefixes, never the module constants.

    Custom triggers escalate the matching change to FULL; a module-default trigger that the custom settings omit does
    NOT escalate. Falsifies the settings-threading cluster: _escalate flipping ``settings is not None``->``is None``
    (module defaults used instead of settings), _closure passing ``_escalate(files, None)``, and route passing
    ``_closure(..., None)`` all swap the consulted trigger set, inverting at least one row of this matrix.
    """
    settings = assay_root.settings.model_copy(update=update)
    routed = assert_ok(route(Language.CSHARP, source=_GraphSource(changed), settings=settings))
    assert routed.scope is expected_scope


# --- [HOST_BOUND_LAWS]

register_law(route, "route_classifies_host_bound_by_marker_only")


def test_route_classifies_host_bound_by_marker_only(assay_root: AssayHarness) -> None:
    """ONLY the explicit AssayHostBound marker classifies; the closure itself is unaffected (BUILD-lane keep).

    Core changes; the closure is {Core, Lib, App}; App alone carries ``<AssayHostBound>true</AssayHostBound>``.
    ``host_bound`` must be exactly (App,) while ``projects`` still carries the full closure, and the unmarked
    graph yields an empty partition. Falsifies: deriving the partition from path shape or host-assembly
    awareness (no path or reference in the graph distinguishes App), dropping host projects from ``projects``
    (BUILD lanes would lose them), and a marker read that is case-sensitive or matches non-``true`` text.
    """
    marked = assert_ok(route(Language.CSHARP, source=_HostGraphSource(("src/Core/c.cs",)), settings=assay_root.settings))
    assert marked.projects == ("src/App/App.csproj", "src/Core/Core.csproj", "src/Lib/Lib.csproj")
    assert marked.host_bound == ("src/App/App.csproj",)
    unmarked = assert_ok(route(Language.CSHARP, source=_GraphSource(("src/Core/c.cs",)), settings=assay_root.settings))
    assert unmarked.host_bound == ()


register_law(place, "place_host_bound_lane_partition")


@pytest.mark.parametrize(
    "mode,expected",
    [
        (Mode.RUN, (("src/Lib/Lib.csproj",),)),
        (Mode.LIST, (("src/Lib/Lib.csproj",),)),
        (Mode.RESTORE, (("src/App/App.csproj",), ("src/Lib/Lib.csproj",))),
        (Mode.BUILD, (("src/App/App.csproj",), ("src/Lib/Lib.csproj",))),
    ],
    ids=["run-drops", "list-drops", "restore-keeps", "build-keeps"],
)
def test_place_host_bound_lane_partition(mode: Mode, expected: tuple[tuple[str, ...], ...], assay_root: AssayHarness) -> None:
    """TEST lanes (RUN/LIST) drop host-bound projects from PROJECT placement; RESTORE/BUILD lanes keep them.

    Falsifies the lane discriminant cluster: filtering on RESTORE/BUILD (host projects stop compiling),
    keeping on RUN/LIST (managed ``dotnet test`` executes a native P/Invoke and dies at runtime), and
    membership tested against ``projects`` instead of ``host_bound``.
    """
    tool = Tool("t", Runner.DOTNET, ("test",), Input.PROJECT, Language.CSHARP, Claim.TEST, mode=mode)
    assert place(_HOST_ROUTED, tool, settings=assay_root.settings) == expected


register_law(place, "place_host_emptied_never_resurrects_fallback")
register_law(expand, "expand_drops_host_emptied_project_check")


def test_host_emptied_placement_drops_check(assay_root: AssayHarness) -> None:
    """An all-host-bound closure yields zero TEST invocations: no fallback resurrection, no bare unpinned check.

    place(LIST) must NOT fall back to ``settings.test_target`` when the route HAD projects — the fallback
    exists for empty routes only; expand must remove the RUN check entirely, because an unpinned zero-tail
    PROJECT check composes a bare ``dotnet test`` against the whole tree. The BUILD check survives untouched,
    and the empty-route fallback (pinned by place_project_list_fallback_to_test_target) stays intact.
    """
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj",), host_bound=("src/App/App.csproj",))
    run_tool = Tool("t", Runner.DOTNET, ("test",), Input.PROJECT, Language.CSHARP, Claim.TEST, mode=Mode.RUN)
    list_tool = msgspec.structs.replace(run_tool, mode=Mode.LIST)
    build_tool = msgspec.structs.replace(run_tool, mode=Mode.BUILD)
    assert place(routed, list_tool, settings=assay_root.settings) == ()
    assert expand((Check(tool=run_tool), Check(tool=build_tool)), routed, settings=assay_root.settings) == (Check(tool=build_tool),)


register_law(Routed, "closure_note_names_host_routed")


def test_closure_note_names_host_routed() -> None:
    """closure_note emits a partition head row, plus a names row exactly when host-routed projects exist."""
    support_matrix(
        ("no projects yields empty", lambda: Routed(Language.CSHARP, Scope.CHANGED).closure_note() == (), True),
        (
            "managed-only yields single head row",
            lambda: (
                Routed(Language.CSHARP, Scope.CHANGED, projects=("a.csproj",)).closure_note()
                == ("closure[csharp]: included=1 excluded=0 cached=0 host-routed=0",)
            ),
            True,
        ),
        (
            "host subset yields head plus names rows",
            lambda: (
                _HOST_ROUTED.closure_note()
                == ("closure[csharp]: included=1 excluded=0 cached=0 host-routed=1", "host-routed[csharp]: src/App/App.csproj")
            ),
            True,
        ),
    )


register_law(place, "place_project_list_fallback_to_test_target")


@pytest.mark.parametrize(
    "mode,projects,expected_fn",
    [(Mode.LIST, (), "test_target"), (Mode.RUN, (), "empty"), (Mode.LIST, ("src/A/A.csproj",), "projects")],
    ids=["list-empty-falls-back", "non-list-empty-yields-nothing", "list-with-projects-passthrough"],
)
def test_place_project_list_fallback_to_test_target(mode: Mode, projects: tuple[str, ...], expected_fn: str, assay_root: AssayHarness) -> None:
    """Input.PROJECT with no routed projects falls back to settings.test_target only when tool.mode is Mode.LIST.

    Falsifies the place PROJECT-arm cluster: ``str(settings.test_target)``->``str(None)`` (the fallback projects the
    literal ``"None"`` instead of the configured target) and ``tool.mode is Mode.LIST``->``is not Mode.LIST`` (the
    fallback fires for the wrong modes); the non-LIST empty case yields no invocations, pinning the discriminant.
    """
    tool = Tool("t", Runner.DOTNET, ("dotnet",), Input.PROJECT, Language.CSHARP, Claim.STATIC, mode=mode)
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=projects)
    result = place(routed, tool, settings=assay_root.settings)
    expected = {"test_target": ((str(assay_root.settings.test_target),),), "empty": (), "projects": tuple((p,) for p in projects)}[expected_fn]
    assert result == expected
