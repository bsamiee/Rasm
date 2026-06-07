"""API rail laws."""

from typing import TYPE_CHECKING

from tools.assay.composition.catalog import CAPTURES
from tools.assay.core.model import ApiResolution, ApiSource, ApiSurface, Check, Claim, Input, Language, Mode, Runner, SourceKind, SymbolShape, Tool
from tools.assay.core.status import RailStatus
from tools.assay.rails import api as api_rail
from tools.assay.rails.api import ApiParams, shape_of


if TYPE_CHECKING:
    from pathlib import Path

    from tests.tools.assay.conftest import AssayHarness, VerbRunner


def test_signature_string_format_captures_forward_refs() -> None:
    """PEP 749 STRING annotations render forward-ref callables for pydist member capture."""

    def forward_handler(value: object) -> object:
        return value

    forward_handler.__annotations__ = {"value": "ForwardType", "return": "ForwardType"}
    sig = api_rail._signature(forward_handler)

    assert "ForwardType" in sig


def test_signature_falls_back_for_non_callable() -> None:
    """Non-callable symbols synthesize annotationlib fallback text instead of an empty capture."""

    class Marker:
        value: str

    sig = api_rail._signature(Marker)

    assert sig
    assert "(" in sig


def test_member_captures_signature_suffix_for_non_callable() -> None:
    """Member capture prefixes the symbol with a non-empty signature suffix."""

    class Marker:
        value: str

    signature, *_ = api_rail._member_captures(Marker, "Marker")

    assert signature.name == "signature"
    assert signature.text != "Marker"
    assert signature.text.startswith("Marker(")


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
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=tmp_path, asset_paths=(good, bad))
    check = Check(tool=Tool("ts-api", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.API, mode=Mode.QUERY))

    roster = CAPTURES.decode(api_rail._tsdecl_thunk(source, "")(check).stdout)
    member = CAPTURES.decode(api_rail._tsdecl_thunk(source, "Bar")(check).stdout)

    assert {"Bar", "Baz", "Thing"} <= {cap.text for cap in roster if cap.name == "type"}
    assert any(cap.parse_error for cap in roster)
    assert member[0].name == "signature"
    assert "Foo as Bar" in member[0].text


def test_tsdecl_member_lookup_prefers_owner_scope(tmp_path: Path) -> None:
    """Qualified TypeScript declaration member lookup resolves owner before simple member names."""
    dts = tmp_path / "index.d.ts"
    dts.write_text("export interface Foo { x: string }\nexport interface Bar { x: number }\n", encoding="utf-8")
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=tmp_path, asset_paths=(dts,))
    check = Check(tool=Tool("ts-api", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.API, mode=Mode.QUERY))

    member = CAPTURES.decode(api_rail._tsdecl_thunk(source, "Foo.x")(check).stdout)

    assert member[0].name == "signature"
    assert "string" in member[0].text
    assert "number" not in member[0].text


def test_resolve_rejects_unknown_kind(assay_root: AssayHarness) -> None:
    """Api resolve --kind typos become typed unsupported reports, not silent all-path lookups."""
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=assay_root.root, asset_paths=())
    report = api_rail._resolve_report(assay_root.settings, source, ApiParams(kind="typo"))

    assert report.status is RailStatus.UNSUPPORTED
    assert isinstance(report.detail, ApiResolution)
    assert ("all", 100) in report.detail.candidates


def test_nuget_source_carries_quality_inventory_facts(assay_root: AssayHarness) -> None:
    """NuGet source evidence preserves package root, frameworks, owners, and selected assets."""
    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Core" Version="1.2.3" /></ItemGroup></Project>')
    assay_root.write("src/App/App.csproj", '<Project><ItemGroup><PackageReference Include="Pkg.Core" /></ItemGroup></Project>')
    package_root = assay_root.root / ".cache/nuget/packages/pkg.core/1.2.3"
    dll = package_root / "lib/net8.0/Pkg.Core.dll"
    xml = dll.with_suffix(".xml")
    nuspec = package_root / "pkg.core.nuspec"
    for path in (dll, xml, nuspec):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text("x", encoding="utf-8")
    (package_root / "lib/netstandard2.0/Pkg.Core.dll").parent.mkdir(parents=True, exist_ok=True)
    (package_root / "lib/netstandard2.0/Pkg.Core.dll").write_text("x", encoding="utf-8")

    source = api_rail._nuget_source(assay_root.settings, "Pkg.Core", "1.2.3")
    detail = api_rail._api_source(source)

    assert source.package_root == package_root
    assert source.frameworks == ("net8.0", "netstandard2.0")
    assert source.owners == ("src/App/App.csproj",)
    assert isinstance(detail, ApiSource)
    assert detail.package_root == str(package_root)
    assert detail.frameworks == ("net8.0", "netstandard2.0")
    assert detail.owners == ("src/App/App.csproj",)
    assert str(dll) in detail.assets
    assert str(xml) in detail.assets


def test_inventory_sources_include_host_nuget_and_lightweight_pydist_rows(assay_root: AssayHarness) -> None:
    """Doctor inventory keeps full source rows while avoiding pydist per-file expansion."""
    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Core" Version="1.2.3" /></ItemGroup></Project>')
    source = api_rail._inventory_sources(assay_root.settings, None, "ilspycmd: x", 0)
    nuget = next(row for row in source if row.source_id == "Pkg.Core")
    pydist = next(row for row in source if row.source_kind is SourceKind.PYDIST and row.source_id != "python-dists")

    assert "rhino-code-remote" in {row.source_id for row in source}
    assert nuget.assets == ()
    assert pydist.package_root
    assert pydist.assets == ()


def test_show_store_report_reads_through_artifact_store(assay_root: AssayHarness) -> None:
    """Api show can preview artifacts through the configured ArtifactStore instead of local Path traversal."""
    store = assay_root.settings.store(protocol="memory", root="api-show-root")
    path = store.write_text("one\ntwo\nthree\n", "scope", "api", "pkg", "surface.txt")
    report = api_rail._show_store_report(store, path, ApiParams(token=path.rsplit("/", 1)[-1], max_lines=2))

    assert report.artifacts[0].path == path
    assert report.artifacts[0].lines == 3
    assert isinstance(report.detail, ApiSurface)
    assert report.detail.preview == "one\ntwo"
    assert report.detail.truncated


def test_api_resolve_cli_preserves_nuget_source_detail(assay_root: AssayHarness, cli: VerbRunner) -> None:
    """Public api resolve envelopes expose NuGet owners/frameworks/package roots, not only path rows."""
    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Core" Version="1.2.3" /></ItemGroup></Project>')
    assay_root.write("src/App/App.csproj", '<Project><ItemGroup><PackageReference Include="Pkg.Core" /></ItemGroup></Project>')
    package_root = assay_root.root / ".cache/nuget/packages/pkg.core/1.2.3"
    dll = package_root / "lib/net8.0/Pkg.Core.dll"
    xml = dll.with_suffix(".xml")
    nuspec = package_root / "pkg.core.nuspec"
    for path in (dll, xml, nuspec):
        path.parent.mkdir(parents=True, exist_ok=True)
        path.write_text("x", encoding="utf-8")

    res = cli("api", "resolve", "--key", "Pkg.Core", "--kind", "all")
    env, code = res.envelope, res.exit_code

    assert code == RailStatus.OK.exit_code
    assert env.report is not None
    assert isinstance(env.report.detail, ApiSurface)
    source = env.report.detail.source
    assert source.source_kind is SourceKind.NUGET
    assert source.package_root == str(package_root)
    assert source.frameworks == ("net8.0",)
    assert source.owners == ("src/App/App.csproj",)
    assert source.nuspec == str(nuspec)
    assert str(dll) in source.assets
    assert str(xml) in source.assets


def test_api_show_latest_cli_prefers_api_scope_artifacts(assay_root: AssayHarness, cli: VerbRunner) -> None:
    """Public api show latest selects API artifacts before newer non-API scope artifacts."""
    store = assay_root.settings.store()
    api_path = store.write_text("api artifact\n", "scope", "api", "pkg", "surface.txt")
    store.write_text("newer generic artifact\n", "scope", "zzz", "surface.txt")

    res = cli("api", "show", "latest", "--max-lines", "1")
    env, code = res.envelope, res.exit_code

    assert code == RailStatus.OK.exit_code
    assert env.report is not None
    assert env.report.artifacts[0].path == api_path
    assert isinstance(env.report.detail, ApiSurface)
    assert env.report.detail.preview == "api artifact"
