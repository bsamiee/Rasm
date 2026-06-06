"""API rail laws."""

from typing import TYPE_CHECKING

from tools.assay.composition.catalog import CAPTURES
from tools.assay.core.model import ApiResolution, ApiSurface, Check, Claim, Input, Language, Mode, Runner, SourceKind, SymbolShape, Tool
from tools.assay.core.status import RailStatus
from tools.assay.rails.api import _resolve_report, _show_report, _Source, _tsdecl_thunk, ApiParams, shape_of  # noqa: PLC2701


if TYPE_CHECKING:
    from pathlib import Path

    from tests.tools.assay.conftest import AssayHarness


def test_shape_of_classifies_symbols() -> None:
    """API query dispatch shape follows the public symbol string."""
    assert shape_of("") is SymbolShape.INDEX
    assert shape_of("System") is SymbolShape.NAMESPACE
    assert shape_of("System.String") is SymbolShape.TYPE
    assert shape_of("System.String.Contains()") is SymbolShape.MEMBER


def test_tsdecl_thunk_exports_aliases_and_parse_errors(tmp_path: Path) -> None:
    """TypeScript declaration thunks capture export aliases and parser failures."""
    good = tmp_path / "index.d.ts"
    bad = tmp_path / "bad.d.ts"
    good.write_text('export { Foo as Bar, Baz } from "./x";\nexport interface Thing { value: string }\n', encoding="utf-8")
    bad.write_text("export interface Broken {", encoding="utf-8")
    source = _Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=tmp_path, asset_paths=(good, bad))
    check = Check(tool=Tool("ts-api", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.API, mode=Mode.QUERY))

    roster = CAPTURES.decode(_tsdecl_thunk(source, "")(check).stdout)
    member = CAPTURES.decode(_tsdecl_thunk(source, "Bar")(check).stdout)

    assert {"Bar", "Baz", "Thing"} <= {cap.text for cap in roster if cap.name == "type"}
    assert any(cap.parse_error for cap in roster)
    assert member[0].name == "signature"
    assert "Foo as Bar" in member[0].text


def test_resolve_rejects_unknown_kind(assay_root: AssayHarness) -> None:
    """Api resolve --kind typos become typed unsupported reports, not silent all-path lookups."""
    source = _Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=assay_root.root, asset_paths=())
    report = _resolve_report(assay_root.settings, source, ApiParams(kind="typo"))

    assert report.status is RailStatus.UNSUPPORTED
    assert isinstance(report.detail, ApiResolution)
    assert ("all", 100) in report.detail.candidates


def test_show_report_marks_preview_truncation(tmp_path: Path) -> None:
    """Api show records when its preview is a bounded window over a larger artifact."""
    path = tmp_path / "surface.txt"
    path.write_text("one\ntwo\nthree\n", encoding="utf-8")
    report = _show_report(path, ApiParams(token=path.name, max_lines=1))

    assert report.artifacts[0].lines == 3
    assert isinstance(report.detail, ApiSurface)
    assert report.detail.truncated
    assert report.detail.lines == 3
