"""Static rail laws."""

from typing import TYPE_CHECKING

from tools.assay.core.model import Claim, Language
from tools.assay.core.routing import Routed, Scope
from tools.assay.rails.static import _languages, _plan_report  # noqa: PLC2701


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_language_inference_from_suffixes() -> None:
    """Explicit file suffixes narrow the static fan to the matching language set."""
    assert _languages(None, ("tools/assay/__init__.py",)) == (Language.PYTHON,)
    assert _languages(None, ("tools/assay/__init__.py", "src/view.tsx")) == (Language.PYTHON, Language.TYPESCRIPT)
    assert Language.CSHARP in _languages(None, ())


def test_plan_preview_uses_engine_argv(assay_root: AssayHarness) -> None:
    """Static plan previews include the same dotnet artifact flags the engine splices."""
    scope = assay_root.scope(Claim.STATIC)
    routed = Routed(Language.CSHARP, Scope.FULL, files=("Workspace.slnx",), projects=("Workspace.slnx",), full_triggers=("Workspace.slnx",))
    report = _plan_report((routed,), assay_root.settings, scope)

    assert report.status.value == "ok"
    assert any("--artifacts-path" in note and "--disable-build-servers" in note for note in report.notes)
