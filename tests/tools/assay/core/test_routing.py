"""Routing and placement laws."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------------

from typing import override, TYPE_CHECKING

from expression import Error, Ok
import pytest

from tests.tools.assay.conftest import assert_result_status
from tools.assay.core.model import Claim, Fault, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import place, route, Routed, Scope, Source
from tools.assay.core.status import RailStatus


if TYPE_CHECKING:
    from expression import Result

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.routing import RoutePaths


# --- [MODELS] ---------------------------------------------------------------------------


class StubSource(Source):
    """Source double for route laws; enumerate falls back to the requested paths when no universe is set."""

    def __init__(self, changed: tuple[str, ...], universe: tuple[str, ...] = ()) -> None:
        """Store changed and enumerated path fixtures."""
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


class FaultingSource(Source):
    """Source double whose changed/enumerate rails fault so the route bind-chain propagates Error."""

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

_PY_TOOL = Tool(
    name="py-check", runner=Runner.UV, command=("ruff", "check"), input=Input.FILES, language=Language.PYTHON, claim=Claim.CODE, mode=Mode.CHECK
)


# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize(
    "language,changed,universe,expected_scope,n_files,full_triggers",
    [
        (Language.PYTHON, ("a.py", "b.ts"), (), Scope.CHANGED, 1, ()),
        (Language.TYPESCRIPT, ("a.py", "b.ts"), (), Scope.CHANGED, 1, ()),
        (Language.CSHARP, ("Workspace.slnx",), ("src/A.csproj",), Scope.FULL, 1, ("Workspace.slnx",)),
        (Language.CSHARP, ("src/A.cs",), ("src/A.csproj",), Scope.CHANGED, 1, ()),
    ],
)
def test_route_table(
    language: Language,
    changed: tuple[str, ...],
    universe: tuple[str, ...],
    expected_scope: Scope,
    n_files: int,
    full_triggers: tuple[str, ...],
    assay_root: AssayHarness,
) -> None:
    """_glob suffix-filters by language and _closure escalates trigger files to FULL scope."""
    routed = route(language, source=StubSource(changed, universe=universe), settings=assay_root.settings)

    assert routed.is_ok()
    assert routed.ok.scope is expected_scope
    assert len(routed.ok.files) == n_files
    assert routed.ok.full_triggers == full_triggers


def test_route_propagates_source_fault(assay_root: AssayHarness) -> None:
    """A faulting Source short-circuits the route bind-chain into a FAULTED Error."""
    assert_result_status(route(Language.PYTHON, source=FaultingSource(), settings=assay_root.settings), RailStatus.FAULTED)


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
)
def test_place_table(input_mode: Input, mode: Mode, routed: Routed, expected: tuple[tuple[str, ...], ...], assay_root: AssayHarness) -> None:
    """Each Input arm projects routed data into a stable argv tail."""
    tool = Tool("t", Runner.DIRECT, ("tool",), input_mode, routed.language, Claim.STATIC, mode=mode)

    assert place(routed, tool, settings=assay_root.settings) == expected


def test_place_strips_typecheck_probe_fixtures_from_files_argv(assay_root: AssayHarness) -> None:
    """Explicit FILE tails skip ast-grep/py_analyzer probes so ty/mypy excludes are not bypassed."""
    routed = Routed(
        language=Language.PYTHON,
        scope=Scope.CHANGED,
        files=("tools/assay/core/model.py", "tests/tools/ast-grep/fail/helper_import.py", "tests/tools/py_analyzer/sample.py"),
    )

    assert place(routed, _PY_TOOL, settings=assay_root.settings) == (("tools/assay/core/model.py",),)


def test_place_solution_arm(assay_root: AssayHarness) -> None:
    """The SOLUTION arm projects the settings solution path, independent of routed inputs."""
    tool = Tool("sln", Runner.DOTNET, ("dotnet",), Input.SOLUTION, Language.CSHARP, Claim.STATIC, mode=Mode.BUILD)
    routed = Routed(Language.CSHARP, Scope.FULL)

    assert place(routed, tool, settings=assay_root.settings) == ((str(assay_root.settings.solution),),)


@pytest.mark.parametrize("k", [10, 20])
def test_place_is_proportional_by_inspection(k: int, assay_root: AssayHarness) -> None:
    """place() projects all k files into one argv tail — O(N) by construction, not a wall-clock ratio."""
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=tuple(f"pkg/mod_{i}.py" for i in range(k)))

    assert place(routed, _PY_TOOL, settings=assay_root.settings) == (routed.files,)
