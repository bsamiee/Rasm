"""Routing and placement laws."""

from typing import override, TYPE_CHECKING

from expression import Ok

from tools.assay.core.model import Claim, Input, Language, Mode, Runner, Tool
from tools.assay.core.routing import place, route, Routed, Scope, Source


if TYPE_CHECKING:
    from expression import Result

    from tests.tools.assay.conftest import AssayHarness
    from tools.assay.core.model import Fault
    from tools.assay.core.routing import RoutePaths


class StubSource(Source):
    """Source double for route laws."""

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


def test_trigger_file_escalates_csharp_to_full_scope(assay_root: AssayHarness) -> None:
    """Workspace trigger files skip project graph resolution and route FULL."""
    routed = route(Language.CSHARP, source=StubSource(("Workspace.slnx",)), settings=assay_root.settings)

    assert routed.is_ok()
    assert routed.ok.scope is Scope.FULL
    assert routed.ok.full_triggers == ("Workspace.slnx",)


def test_place_projects_and_files(assay_root: AssayHarness) -> None:
    """Input modes project routed data into stable argv tails."""
    files = Tool("files", Runner.DIRECT, ("tool",), Input.FILES, Language.PYTHON, Claim.STATIC)
    project = Tool("proj", Runner.DIRECT, ("tool",), Input.PROJECT, Language.CSHARP, Claim.TEST, mode=Mode.RUN)
    routed = Routed(Language.CSHARP, Scope.CHANGED, files=("a.cs",), projects=("A.csproj",))

    assert place(routed, files, settings=assay_root.settings) == (("a.cs",),)
    assert place(routed, project, settings=assay_root.settings) == (("A.csproj",),)
