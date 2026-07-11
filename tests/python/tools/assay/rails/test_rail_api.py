"""Law matrix for API params, inventory, resolution, and surface queries."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

import re
from typing import override, TYPE_CHECKING, TypeAliasType

from dirty_equals import IsInt, IsStr
from expression import Error, Ok, Result  # runtime: canned executor lanes return Result instances
from hypothesis import given as hyp_given, settings as hyp_settings, strategies as st
import msgspec
import pytest

from tests.python._testkit.spec import assert_error, assert_ok, assert_roundtrip
from tests.python._testkit.strategies import resolve as st_resolve  # aliased to avoid collision with tools.assay.rails.api.resolve verb
from tests.python.tools.assay.kit import RailProbe, SeamExecutor
from tools.assay import oracle as oracle_mod
from tools.assay.composition.catalog import select
from tools.assay.core.exec import EngineExecutor
from tools.assay.core.model import (
    ApiResolution,
    ApiSource,
    ApiSurface,
    ArtifactKind,
    Check,
    Claim,
    Fault,
    Input,
    Language,
    Mode,
    RailStatus,
    Runner,
    SourceKind,
    SymbolShape,
    Tool,
    ToolArgs,
)
from tools.assay.diagnostics import CAPTURES, ts_language
from tools.assay.rails import api as api_rail
from tools.assay.rails.api import ApiParams, query, resolve, shape_of, show, status


if TYPE_CHECKING:
    from collections.abc import Callable
    from pathlib import Path

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import AssaySettings
    from tools.assay.composition.store import ArtifactScope
    from tools.assay.core.exec import Executor
    from tools.assay.core.model import Report

    type Verb = Callable[[AssaySettings, ArtifactScope, ApiParams, Executor], Result[Report, Fault]]  # query/resolve/show/status share this shape


# --- [CONSTANTS] ------------------------------------------------------------------------

COVERS: tuple[object, ...] = (
    ApiParams, query, resolve, show, status, shape_of,
    oracle_mod.Oracle, oracle_mod.oracle_for,
    oracle_mod.HostBundleOracle, oracle_mod.NugetOracle, oracle_mod.PydistOracle, oracle_mod.TsdeclOracle,
    oracle_mod.Source, oracle_mod.Surface, oracle_mod.CacheEntry,
    oracle_mod.probe_ilspy, oracle_mod.host_sources, oracle_mod.rhino_app,
    oracle_mod.packages, oracle_mod.package_owner_index, oracle_mod.resolve_key,
    oracle_mod.nuget_source, oracle_mod.tfm_rank, oracle_mod.consumer_tfm_floor, oracle_mod.to_api_source,
    oracle_mod.pydist_inventory_sources, oracle_mod.tsdecl_names, oracle_mod.tsdecl_source,
    oracle_mod.rank_candidates, oracle_mod.rank_type, oracle_mod.rank_namespace,
    oracle_mod.fidelity_note, oracle_mod.safe_key, oracle_mod.xml_doc,
)  # fmt: skip

# Valid _PathKind tokens mirrored from api.py.
_VALID_KINDS: tuple[str, ...] = ("all", "assembly", "xml", "nuspec", "deps", "package-root")

# ilspy `Kind FullyQualifiedName` rows exercise namespace, type, and member dispatch.
_ILSPY_TYPES: bytes = b"Class Acme.Widget\nStruct Acme.Point\nEnum Acme.Color\nInterface Acme.IThing\n"

# Spin's non-zero offset drives anchor search; trailing members drive cap and grep laws.
_ILSPY_DECOMPILE: bytes = (
    b"namespace Acme\n"
    b"{\n"
    b"    public sealed class Widget\n"
    b"    {\n"
    b"        public void Spin(int turns) { }\n"
    b"        public int Count => 3;\n"
    b"        public double Ratio => 1.5;\n"
    b'        public string Tag => "x";\n'
    b"    }\n"
    b"}\n"
)
_ILSPY_DECOMPILE_LINES: int = len(_ILSPY_DECOMPILE.decode().splitlines())

# XMLDoc keys match the sidecar member-name suffix lookup.
_ILSPY_XML: str = (
    "<doc><members>"
    '<member name="T:Acme.Widget"><summary>A widget that spins.</summary></member>'
    '<member name="M:Acme.Widget.Spin(System.Int32)"><summary>Spins the widget.</summary></member>'
    "</members></doc>"
)

_INPROC_CHECK: Check = Check(tool=Tool("py-api", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.API, mode=Mode.QUERY))
_TS_CHECK: Check = Check(tool=Tool("ts-api", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.API, mode=Mode.QUERY))

_TEN_LINES: str = "\n".join(f"line{i}" for i in range(1, 11))

# The fixture has no deps target dirs, so that kind stays EMPTY.
_PRESENT_KINDS: tuple[str, ...] = ("all", "assembly", "xml", "nuspec", "package-root")

_STATUS_ROW = re.compile(r"^\S+ status=\S+ assembly=(?:present|missing) xml=(?:present|missing) version=\S+$")

# --- [MODELS] ---------------------------------------------------------------------------


class _DistDouble:
    """Distribution double with absent files and overridable root lookup."""

    def __init__(self, name: str, *, root: str) -> None:
        self.metadata: dict[str, str] = {"Name": name}
        self.version = "1.0"
        self.files = None
        self._root = root

    def locate_file(self, _rel: str) -> str:
        return self._root


# --- [OPERATIONS] -----------------------------------------------------------------------


def _run(verb: Verb, assay_root: AssayHarness, executor: Executor | None = None, /, **params: object) -> Result[Report, Fault]:
    """Run an API verb in a fresh Claim.API scope, defaulting to the production executor.

    Returns:
        Verb result over a fresh API scope rooted at the harness.
    """
    return verb(
        assay_root.settings,
        assay_root.scope(Claim.API),
        ApiParams(**params),  # ty: ignore[invalid-argument-type]  # **params is the open keyword set forwarded into the typed ApiParams ctor
        executor if executor is not None else EngineExecutor(),
    )


def _install_ilspy(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    *,
    types: bytes = _ILSPY_TYPES,
    decompile: bytes = _ILSPY_DECOMPILE,
    returncode: int = 0,
    xmls: bool = False,
) -> SeamExecutor:
    """Pin ilspy source resolution and return a canned executor replaying list/decompile receipts.

    Returns:
        Canned executor whose run lane discriminates list vs decompile on the ``-t`` flag.
    """
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    xml_paths = (assay_root.write("RhinoCommon.xml", _ILSPY_XML),) if xmls else ()
    source = oracle_mod.Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,), xmls=xml_paths)
    rail_status = RailStatus.OK if returncode == 0 else RailStatus.FAULTED

    def _canned(check: Check, **_kw: object) -> Result[object, Fault]:
        payload = decompile if "-t" in check.tool.command else types
        stderr = b"" if returncode == 0 else b"ilspy boom"
        return RailProbe.receipt(("ilspycmd",), returncode, status=rail_status, stdout=payload if returncode == 0 else b"", stderr=stderr)

    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(source))
    return SeamExecutor(run_fn=_canned)


def _cs_surface(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, symbol: str, *, executor: SeamExecutor | None = None, **kw: object
) -> ApiSurface:
    """Return the C# ApiSurface detail over the canned ilspy source."""
    canned = executor if executor is not None else _install_ilspy(assay_root, monkeypatch)
    detail = assert_ok(_run(query, assay_root, canned, key="rhino-common", symbol=symbol, **kw)).detail
    assert isinstance(detail, ApiSurface)
    return detail


def _nuget_fixture(assay_root: AssayHarness, *, owner: bool = True) -> Path:
    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Core" Version="1.2.3" /></ItemGroup></Project>')
    if owner:
        assay_root.write("src/App/App.csproj", '<Project><ItemGroup><PackageReference Include="Pkg.Core" /></ItemGroup></Project>')
    package_root = assay_root.root / ".cache/nuget/packages/pkg.core/1.2.3"
    for rel in ("lib/net8.0/Pkg.Core.dll", "lib/net8.0/Pkg.Core.xml", "lib/netstandard2.0/Pkg.Core.dll", "pkg.core.nuspec"):
        target = package_root / rel
        target.parent.mkdir(parents=True, exist_ok=True)
        target.write_text("x", encoding="utf-8")
    return package_root


def _ts_package(assay_root: AssayHarness, body: str, *, name: str = "mypkg", version: str = "2.1.0") -> None:
    assay_root.write(f"node_modules/{name}/package.json", f'{{"name":"{name}","version":"{version}","types":"index.d.ts"}}')
    assay_root.write(f"node_modules/{name}/index.d.ts", body)


# --- [SHAPE_OF]


@pytest.mark.parametrize(
    "symbol, expected",
    [
        ("", SymbolShape.INDEX),
        ("   ", SymbolShape.INDEX),  # strips to empty
        ("System", SymbolShape.NAMESPACE),
        ("lowercase", SymbolShape.NAMESPACE),
        ("System.String", SymbolShape.TYPE),
        ("rhino.Mesh", SymbolShape.TYPE),
        ("foo.bar.Baz", SymbolShape.TYPE),  # last segment uppercase → TYPE
        ("System.String.Contains()", SymbolShape.MEMBER),
        ("rhino.Mesh.AddVertex()", SymbolShape.MEMBER),
        ("foo.bar.baz", SymbolShape.MEMBER),  # last segment lowercase with dot → MEMBER
    ],
    ids=["empty", "ws", "1-up", "1-low", "2-up", "rhino-type", "3-up", "parens", "rhino-mem", "3-low"],
)
def test_shape_of_dispatch(symbol: str, expected: SymbolShape) -> None:
    """shape_of dispatches every SymbolShape arm and remains idempotent."""
    assert shape_of(symbol) is expected
    assert shape_of(symbol) is shape_of(symbol)


# --- [API_PARAMS]


@pytest.mark.parametrize(
    "verb, paths, key, symbol, kind, token",
    [
        ("resolve", ("MyPkg", "assembly"), "MyPkg", "", "assembly", ""),  # 1st→key, 2nd→kind
        ("resolve", ("OnlyKey",), "OnlyKey", "", "all", ""),  # single→key, kind default
        ("query", ("MySymbol",), "rhino-common", "MySymbol", "all", ""),  # 1st→symbol
        ("show", ("my-token",), "rhino-common", "", "all", "my-token"),  # 1st→token
        ("resolve", (), "rhino-common", "", "all", ""),  # empty→defaults
        ("query", (), "rhino-common", "", "all", ""),
        ("show", (), "rhino-common", "", "all", ""),
    ],
    ids=["resolve-2", "resolve-1", "query-1", "show-1", "resolve-0", "query-0", "show-0"],
)
def test_api_params_bound_dispatch(verb: str, paths: tuple[str, ...], key: str, symbol: str, kind: str, token: str) -> None:
    """ApiParams.bound routes positional tokens into the correct verb slots and consumes paths."""
    result = ApiParams(paths=paths).bound(verb)
    assert result == ApiParams(key=key, symbol=symbol, kind=kind, token=token, paths=())
    p = ApiParams()
    assert p.bound("status") is p  # zero-slot verb with no paths → identity passthrough


@pytest.mark.parametrize(
    "verb, paths, expected_flags",
    [
        ("query", ("a", "b"), "--symbol"),  # query arity 1; b is surplus
        ("resolve", ("rhino-common", "all", "extra"), "--key --kind"),  # resolve arity 2; extra is surplus
        ("show", ("x", "y"), "--token"),  # show arity 1; y is surplus
    ],
    ids=["query-surplus", "resolve-surplus", "show-surplus"],
)
def test_api_params_bound_surplus_faults_naming_flags_and_arity(verb: str, paths: tuple[str, ...], expected_flags: str) -> None:
    """A surplus-positional api fault names the exact flags and the resolved arity so an agent reads the corrected form."""
    result = ApiParams(paths=paths).bound(verb)
    assert isinstance(result, Fault)
    arity = ApiParams.VERB_SLOTS[verb][0]
    assert f"{verb}: unexpected positional(s)" in result.message
    assert f"accepts at most {arity} positional(s) — use flags: {expected_flags}" in result.message


# --- [STATUS]


def _status_lines(assay_root: AssayHarness, sources: tuple[str, ...]) -> int:
    detail = assert_ok(_run(status, assay_root, sources=sources)).detail
    return detail.lines if isinstance(detail, ApiSurface) else -1


def test_status_sources_prefix_filters_monotone(assay_root: AssayHarness) -> None:
    """Status prefix filtering is monotone across every real prefix: all >= matching >= non-matching == 0."""
    lines_all = _status_lines(assay_root, ())
    for prefix in ("rhino-", "eto", "ilspy", "python-dists", "ts-decls"):
        assert 0 <= _status_lines(assay_root, (prefix,)) <= lines_all, f"prefix {prefix!r} broke monotonicity"
    assert _status_lines(assay_root, ("xyz-no-match-xyzzy-",)) == 0


def test_status_strict_faults_when_core_bundle_absent(assay_root: AssayHarness) -> None:
    """Status strict=True faults when a required core bundle is absent (a minimal tmp-tree has no Rhino bundles)."""
    result = _run(status, assay_root, strict=True)
    match result.tag:
        case "error":
            assert assert_error(result).status is RailStatus.FAULTED
        case "ok":
            assert result.ok.status is RailStatus.OK  # unexpectedly fully-OK environment → strict passes through


def test_status_strict_ignores_absent_non_core_source(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Strict status faults only on an absent core bundle, never an absent transitive package (System.IO.Pipelines)."""
    core_ok = tuple(ApiSource(source_kind=SourceKind.ASSEMBLY, source_id=sid, status=RailStatus.OK) for sid in api_rail._REQUIRED_SOURCE_IDS)
    transitive_absent = ApiSource(source_kind=SourceKind.NUGET, source_id="System.IO.Pipelines", status=RailStatus.EMPTY)
    executor = SeamExecutor(run_fn=lambda *_a, **_k: RailProbe.receipt(("ilspycmd",), 0, stdout=b"ilspycmd: 9.1.0.7988\n"))
    monkeypatch.setattr(api_rail, "_inventory_sources", lambda *_a, **_k: (*core_ok, transitive_absent))
    assert assert_ok(_run(status, assay_root, executor, strict=True)).status is RailStatus.OK


def test_status_inventory_projections(assay_root: AssayHarness) -> None:
    """One status run folds NuGet + polyglot rows, retains the artifact under the API claim root, and keeps stable row identity/grammar."""
    _nuget_fixture(assay_root)
    executor = SeamExecutor(run_fn=lambda *_a, **_kw: RailProbe.receipt(("ilspycmd",), 0, stdout=b"9.1.0.7988\n"))  # space-free version token

    first = assert_ok(_run(status, assay_root, executor))
    detail = first.detail
    assert isinstance(detail, ApiSurface)
    assert "ilspycmd" in detail.preview  # canned version row surfaced
    assert "python-dists" in detail.preview  # polyglot summary row present
    assert detail.lines == IsInt(ge=3)  # rhino-app + ilspycmd + at least the NuGet package

    store = assay_root.settings.store()
    run_id = assay_root.settings.run_id
    assert tuple(a.path for a in first.artifacts) == (store.path(Claim.API.value, run_id, "status-inventory.json"),)
    assert all(a.kind is ArtifactKind.SCOPE for a in first.artifacts)
    assert store.exists(Claim.API.value, run_id, "status-inventory.json")  # retain_scopes(Claim.API) owns this root
    assert not store.exists(ArtifactKind.SCOPE.value, Claim.API.value, run_id, "status-inventory.json")

    assert first.results
    assert all(m.id.startswith("inventory:") for m in first.results)
    for match in first.results:
        assert _STATUS_ROW.fullmatch(match.text), f"status row text broke the key=value grammar: {match.text!r}"
        assert tuple(token.split("=", 1)[0] for token in match.text.split(" ")[1:]) == ("status", "assembly", "xml", "version")

    second = assert_ok(_run(status, assay_root, executor))
    assert tuple(m.id for m in first.results) == tuple(m.id for m in second.results)  # ids stable across runs


def test_inventory_sources_keep_full_rows_without_pydist_file_expansion(assay_root: AssayHarness) -> None:
    """_inventory_sources keeps host/NuGet rows and omits pydist asset expansion."""
    # White-box: asset suppression (include_assets=False) is invisible in the summary preview; only the row objects prove it.
    _nuget_fixture(assay_root, owner=False)
    sources = api_rail._inventory_sources(assay_root.settings, None, "ilspycmd: 9.1", 0)
    by_id = {row.source_id for row in sources}
    nuget = next(row for row in sources if row.source_id == "Pkg.Core")
    pydist = next(row for row in sources if row.source_kind is SourceKind.PYDIST and row.source_id != "python-dists")
    assert {"rhino-app", "ilspycmd", "rhino-code-remote", "python-dists", "ts-decls"} <= by_id
    assert nuget.assets == ()  # status inventory skips NuGet asset expansion
    assert pydist.package_root  # pydist row keeps its root...
    assert pydist.assets == ()  # ...but not its per-file asset list


# --- [RESOLVE]


@pytest.mark.parametrize(
    "key, kind, strict, reason",
    [
        ("totally-unknown-xxxxxxx123", "all", False, "unknown"),  # unknown key → nearest candidates
        ("pytest", "typo", False, "unknown-kind"),  # valid key, invalid kind → kind candidates
        ("bad\x00key", "all", False, "unknown"),  # NUL key raises at Distribution/glob boundary → graceful miss
        ("totally-unknown-xxxxxxx123", "all", True, "unknown"),  # strict promotes the miss to a FAULTED rail
    ],
    ids=["unknown-key", "bad-kind", "codec-boundary", "strict-fault"],
)
def test_resolve_miss_family(
    assay_root: AssayHarness,
    key: str,
    kind: str,
    strict: bool,  # noqa: FBT001  # parametrized bool flag
    reason: str,
) -> None:
    """Resolve misses return ApiResolution candidates, while strict promotes to FAULTED."""
    result = _run(resolve, assay_root, key=key, kind=kind, strict=strict)
    match strict:
        case True:
            assert assert_error(result).status is RailStatus.FAULTED
        case False:
            r = assert_ok(result)
            assert r.status is RailStatus.UNSUPPORTED
            assert isinstance(r.detail, ApiResolution)
            assert r.detail.reason == reason
            assert r.detail.candidates  # every miss carries nearest candidates
            assert reason != "unknown-kind" or {name for name, _ in r.detail.candidates} >= set(_VALID_KINDS)


@pytest.mark.parametrize("kind", _VALID_KINDS, ids=list(_VALID_KINDS))
def test_resolve_valid_kinds_accepted(assay_root: AssayHarness, kind: str) -> None:
    """Resolve accepts all documented kind tokens without an UNSUPPORTED kind error (may be EMPTY)."""
    r = assert_ok(_run(resolve, assay_root, key="pytest", kind=kind))  # pytest anchors kind validation to a resolvable pydist key
    assert not isinstance(r.detail, ApiResolution) or r.detail.reason != "unknown-kind", f"kind={kind!r} unexpectedly rejected"


def test_resolve_pydist_key_ok(assay_root: AssayHarness) -> None:
    """Resolve with a live pydist key returns Ok with ApiSurface detail (not ApiResolution)."""
    r = assert_ok(_run(resolve, assay_root, key="pytest", kind="all"))
    assert r.status in {RailStatus.OK, RailStatus.EMPTY}
    assert isinstance(r.detail, ApiSurface)
    assert r.detail.source.source_kind is SourceKind.PYDIST


# --- [SHOW]


def _show(assay_root: AssayHarness, content: str, **params: object) -> Report:
    token = assay_root.settings.store().write_text(content, "scope", "api", "pkg", "data.txt").rsplit("/", 1)[-1]
    return assert_ok(_run(show, assay_root, token=token, **params))


def test_show_absent_token_yields_empty(assay_root: AssayHarness) -> None:
    """Show with a token that matches no artifact returns Ok(EMPTY)."""
    absent = "not-found-abc-xyz-123"  # not a password — artifact lookup token per api.py
    assert assert_ok(_run(show, assay_root, token=absent)).status is RailStatus.EMPTY


@pytest.mark.parametrize(
    "content, params, expect_truncated, expect_lines, check_preview",
    [
        (_TEN_LINES, {"max_lines": 3}, True, 10, lambda pv: pv.count("\n") < 9),  # cap truncates below all 10
        ("line1\nline2\nline3", {"max_lines": 120}, False, 3, lambda _pv: True),  # within cap → no truncation
        (_TEN_LINES, {"lines": "3:5", "max_lines": 1}, True, 10, lambda pv: pv == "line3\nline4\nline5"),  # explicit window beats cap
        ("alpha 1\nbeta 2\nalpha 3\ngamma 4\nalpha 5", {"grep": "alpha", "max_lines": 120}, False, 3, lambda pv: "beta" not in pv),
        ("\n".join(f"row{i}" for i in range(1, 31)), {"full": True, "max_lines": 2}, False, 30, lambda pv: pv.count("\n") == 29),  # --full → whole
    ],
    ids=["max-lines-cap", "within-cap", "explicit-window", "grep-filter", "full-flag"],
)
def test_show_store_windowing(
    assay_root: AssayHarness,
    content: str,
    params: dict[str, object],
    expect_truncated: bool,  # noqa: FBT001  # parametrized bool flag
    expect_lines: int,
    check_preview: Callable[[str], bool],
) -> None:
    """Show applies max-lines, explicit windows, grep, and full-view selection; the Artifact row records the true line count."""
    report = _show(assay_root, content, **params)
    detail = report.detail
    assert isinstance(detail, ApiSurface)
    assert detail.truncated is expect_truncated
    assert detail.lines == expect_lines
    assert check_preview(detail.preview)
    assert report.artifacts
    assert report.artifacts[0].lines == len(content.splitlines())  # Artifact.lines is the stored content's count, never the window's


@pytest.mark.parametrize(
    "writes, winner",
    [
        ((("scope-api", ("scope", "api", "pkg", "surface.txt")), ("generic", ("scope", "zzz", "surface.txt"))), "scope-api"),
        (
            (
                ("claim-api", (Claim.API.value, "run-a", "status-inventory.json")),
                ("scope-api", ("scope", "api", "pkg", "surface.txt")),
                ("generic", ("scope", "zzz", "surface.txt")),
            ),
            "claim-api",
        ),
    ],
    ids=["api-scope-beats-generic", "claim-root-beats-scope-cache"],
)
def test_show_latest_preference_chain(assay_root: AssayHarness, writes: tuple[tuple[str, tuple[str, ...]], ...], winner: str) -> None:
    """Show latest prefers retained API-claim evidence, then API scope cache, before any newer generic artifact."""
    store = assay_root.settings.store()
    paths = {label: store.write_text(f"{label} artifact\n", *parts) for label, parts in writes}
    latest_token = "latest"  # noqa: S105  # not a password — artifact lookup keyword per api.py _LATEST_ARTIFACT
    r = assert_ok(_run(show, assay_root, token=latest_token, max_lines=50))
    assert r.status is RailStatus.OK
    assert r.artifacts
    assert r.artifacts[0].path == paths[winner]


# --- [QUERY]


@pytest.mark.parametrize("key, unsupported", [("pytest", False), ("totally-unknown-xxxxxxx456", True)], ids=["pydist-ok", "unknown-unsupported"])
def test_query_key_resolution(
    assay_root: AssayHarness,
    key: str,
    unsupported: bool,  # noqa: FBT001  # parametrized bool flag
) -> None:
    """Query keeps live and unknown keys on the Ok rail."""
    r = assert_ok(_run(query, assay_root, key=key, symbol=""))
    assert r.verb == "query"
    assert r.status is not RailStatus.FAULTED  # both arms ride the Ok rail
    assert not unsupported or (r.status is RailStatus.UNSUPPORTED and isinstance(r.detail, ApiResolution))


# --- [WIRE_TYPES]


@pytest.mark.parametrize(
    "type_, strategy",
    [(ApiResolution, st_resolve(ApiResolution)), (ApiSource, st_resolve(ApiSource)), (ApiSurface, st_resolve(ApiSurface))],
    ids=["resolution", "source", "surface"],
)
@hyp_given(st.data())
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_wire_roundtrip(type_: type, strategy: st.SearchStrategy[object], data: st.DataObject) -> None:
    """ApiResolution/ApiSource/ApiSurface each survive a deterministic JSON encode/decode round-trip."""
    assert_roundtrip(data.draw(strategy), type_)


# --- [CS_QUERY]


@pytest.mark.parametrize(
    "symbol, decompile, shape, anchor",
    [
        ("", _ILSPY_DECOMPILE, SymbolShape.INDEX, "Acme"),  # roster of namespaces
        ("Acme", _ILSPY_DECOMPILE, SymbolShape.NAMESPACE, "Acme.Widget"),  # namespace roster of owned types
        ("Widget", _ILSPY_DECOMPILE, SymbolShape.TYPE, "public sealed class Widget"),  # type decompile window
        ("Acme.Widget.Spin", _ILSPY_DECOMPILE, SymbolShape.MEMBER, "public void Spin(int turns)"),  # member decompile signature
        ("wid", _ILSPY_DECOMPILE, SymbolShape.SEARCH, "Acme.Widget"),  # lc namespace-shaped, substring-matches → search hits
        ("Acme.Widget", b"", SymbolShape.SEARCH, "Acme.Widget"),  # empty decompile → namespace fallback → search
    ],
    ids=["index", "namespace", "type", "member", "search-substring", "search-empty-decompile"],
)
def test_cs_query_dispatches_every_shape(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, symbol: str, decompile: bytes, shape: SymbolShape, anchor: str
) -> None:
    """C# query dispatches every canned ilspy shape and search fallback."""
    # Two same-prefix types make the substring-search arm non-vacuous.
    types = {"wid": b"Class Acme.Widget\nClass Acme.WidgetFactory\nStruct Acme.Point\n"}.get(symbol, _ILSPY_TYPES)
    executor = _install_ilspy(assay_root, monkeypatch, types=types, decompile=decompile)
    detail = _cs_surface(assay_root, monkeypatch, symbol, executor=executor)
    assert detail.shape is shape, f"symbol {symbol!r}: shape {detail.shape} != {shape}"
    assert anchor in f"{detail.preview}\n{detail.signature}" or anchor in detail.preview.splitlines(), f"symbol {symbol!r}: {anchor!r} missing"


def test_cs_query_decompile_projections_window_grep_and_xml_doc(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The type window is byte-exact, grep recomputes the selected count, and member decompile pulls the sidecar XMLDoc."""
    executor = _install_ilspy(assay_root, monkeypatch, xmls=True)
    window = _cs_surface(assay_root, monkeypatch, "Widget", executor=executor)
    assert window.preview == "\n".join(_ILSPY_DECOMPILE.decode().splitlines()[2:])  # namespace wrapper dropped, class block verbatim

    grepped = _cs_surface(assay_root, monkeypatch, "Widget", executor=executor, grep="Spin")
    assert "Spin" in grepped.preview
    assert "Count" not in grepped.preview  # grep dropped the non-matching member lines
    assert grepped.lines == 1

    member = _cs_surface(assay_root, monkeypatch, "Acme.Widget.Spin", executor=executor)
    assert member.doc == "Spins the widget."
    assert member.member == "Spin"


def _large_cisde_listing(types: int) -> tuple[bytes, int]:
    # A real-shaped cisde roster exceeding capture_spill_bytes, plus one compiler synthetic and one generic-by-bare-name row.
    rows = [f"Class Acme.Big.Type{i:06d}WithAVeryLongTrailingNameToInflateBytes" for i in range(types)]
    payload = ("\n".join(rows) + "\nClass Acme.Big.GenericDictionary\nClass Acme.Big.<>c__DisplayClass0_0\n").encode()
    return payload, types + 1  # +1 generic survives; the <>-synthetic is filtered


def test_cs_query_index_count_survives_capture_spill_truncation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A cisde listing exceeding capture_spill_bytes rosters every real type, and the cache-hit read replays the full count.

    Defect #3 (api stale cache) cached a 4 KB byte-tail, so a real type became false-absent. A >1 MB listing whose full
    count survives both the first parse and the second cache-backed parse proves the receipt and cache now carry the whole payload.
    """
    payload, expected = _large_cisde_listing(40_000)
    assert len(payload) > assay_root.settings.capture_spill_bytes  # the listing genuinely crosses the 1 MB spill ceiling
    executor = _install_ilspy(assay_root, monkeypatch, types=payload)

    first = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol=""))
    assert any(note == f"{expected} types across 1 namespaces" for note in first.notes)

    # A hard-faulting executor on the second call proves the second count is cache-backed, not a re-listing.
    relist_guard = SeamExecutor(run_fn=lambda *_a, **_kw: RailProbe.error(("api",), "must-not-relist"))
    second = assert_ok(_run(query, assay_root, relist_guard, key="rhino-common", symbol=""))
    assert second.notes == first.notes  # the full type count round-trips through the persisted cache, never truncated


def test_cs_query_roster_filters_synthetics_keeps_generics(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The C# roster drops every <>-mangled compiler synthetic but keeps generic types rendered by bare name."""
    listing = (
        b"Class Acme.Mesh\n"
        b"Class <Module>\n"
        b"Class Acme.<>c__DisplayClass4_0\n"
        b"Class Acme.Mesh.<GetEnumerator>d__84\n"
        b"Class <PrivateImplementationDetails>\n"
        b"Class Acme.AgnosticDictionary\n"  # cisde renders an open generic by bare name, no angle bracket
        b"Struct Acme.Point\n"
    )
    executor = _install_ilspy(assay_root, monkeypatch, types=listing)
    detail = _cs_surface(assay_root, monkeypatch, "Acme", executor=executor)  # namespace roster surfaces the owned types directly
    rostered = set(detail.preview.splitlines())
    assert {"Acme.Mesh", "Acme.AgnosticDictionary", "Acme.Point"} <= rostered  # real types and the generic survive
    assert not any("<" in row for row in rostered)  # every angle-bracket synthetic is filtered


def test_cs_query_projections_fidelity_note_and_identity_ids(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Decompile and roster reports carry the SourceKind fidelity note and identity-shaped result ids."""
    executor = _install_ilspy(assay_root, monkeypatch)
    decompiled = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="Widget"))
    roster = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="Acme"))
    assert "fidelity: decompiled" in decompiled.notes
    assert decompiled.results[0].id == "type:Acme.Widget"
    assert roster.results
    assert all(m.id == f"scope:{m.text}" for m in roster.results)


def test_cs_query_grep_member_fans_out_to_decompile(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A --grep member needle with no type hit fans out ilspycmd -t over candidate types and finds the member."""
    executor = _install_ilspy(assay_root, monkeypatch)  # decompile receipt declares Widget.Spin(int turns)
    r = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="zzz-no-such-type", grep="Spin"))
    assert r.status is RailStatus.OK
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.shape is SymbolShape.SEARCH
    assert "Spin" in detail.preview
    assert any("member hits" in note for note in r.notes)


def test_cs_query_grep_member_caps_candidate_fanout(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A broad --grep needle decompiles at most _CANDIDATE_CAP candidate types, never an N-type explosion."""
    listing = ("\n".join(f"Class Acme.Type{i:03d}" for i in range(50))).encode()
    decompiled = {"count": 0}
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    source = oracle_mod.Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,))

    def _canned(check: Check, **_kw: object) -> Result[object, Fault]:
        if "-t" in check.tool.command:
            decompiled["count"] += 1
            return RailProbe.receipt(("ilspycmd",), 0, stdout=b"// no Spin here\n")
        return RailProbe.receipt(("ilspycmd",), 0, stdout=listing)

    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(source))

    assert_ok(_run(query, assay_root, SeamExecutor(run_fn=_canned), key="rhino-common", symbol="nomatch", grep="Type"))
    assert decompiled["count"] <= api_rail._CANDIDATE_CAP  # the cap is the explosion guard


@pytest.mark.parametrize(
    "max_lines, full, expect_truncated, note",
    [(2, False, True, f"window: 2 of {_ILSPY_DECOMPILE_LINES} lines (--full or --max-lines to widen)"), (1, True, False, "")],
    ids=["small-cap-truncates-with-window-note", "full-flag-never-truncates"],
)
def test_cs_query_truncation_matrix(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    max_lines: int,
    full: bool,  # noqa: FBT001  # parametrized bool flag
    expect_truncated: bool,  # noqa: FBT001  # parametrized bool flag
    note: str,
) -> None:
    """C# decompile lines keep full count while preview honors cap/full, surfacing the window note when capped."""
    executor = _install_ilspy(assay_root, monkeypatch)
    r = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="Widget", max_lines=max_lines, full=full))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.truncated is expect_truncated
    assert detail.lines == _ILSPY_DECOMPILE_LINES  # full count regardless of window
    assert not note or note in r.notes


def test_roster_forced_cap_emits_results_note(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Roster overflow emits the capped-results artifact note."""
    executor = _install_ilspy(assay_root, monkeypatch)
    monkeypatch.setattr(api_rail, "RESULT_CAP", 1)
    r = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="Acme"))  # namespace roster of 4 owned types
    assert len(r.results) == 1
    assert "results: 1 of 4 (cap=1); full listing in artifact" in r.notes


@pytest.mark.parametrize("symbol", ["Nonexistent", "Other.Missing"], ids=["search-miss", "unranked-fqn-miss"])
def test_cs_query_resolution_miss(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, symbol: str) -> None:
    """C# search and unranked-type misses fold to partial ApiResolution."""
    executor = _install_ilspy(assay_root, monkeypatch)
    r = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol=symbol))
    detail = r.detail
    assert isinstance(detail, ApiResolution)
    assert detail.reason == "partial"
    assert r.status is RailStatus.UNSUPPORTED


def test_cs_surface_all_attempts_fail_faults_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """C# surface with every ilspycmd attempt non-zero promotes the rail to a FAULTED error."""
    executor = _install_ilspy(assay_root, monkeypatch, returncode=1)
    e = assert_error(_run(query, assay_root, executor, key="rhino-common", symbol=""))
    assert e.status is RailStatus.FAULTED
    assert "ilspy" in e.message.casefold()


def test_cs_decompile_all_attempts_fail_faults_with_cause(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A roster-resolved decompile whose every attempt exits non-zero faults with the tool's stderr, never a silent search."""
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    source = oracle_mod.Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,))
    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(source))

    def _canned(check: Check, **_kw: object) -> Result[object, Fault]:
        broken = "-t" in check.tool.command
        return RailProbe.receipt(
            ("ilspycmd",),
            1 if broken else 0,
            status=RailStatus.FAULTED if broken else RailStatus.OK,
            stdout=b"" if broken else _ILSPY_TYPES,
            stderr=b"Cannot find a tool in the manifest file that has a command named 'ilspycmd'." if broken else b"",
        )

    e = assert_error(_run(query, assay_root, SeamExecutor(run_fn=_canned), key="rhino-common", symbol="Acme.Widget"))
    assert e.status is RailStatus.FAULTED
    assert "Cannot find a tool" in e.message


def test_cs_member_body_mention_misses_to_candidates(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A parameter or body identifier never anchors a member window; the miss reports ranked candidates instead."""
    body = (
        b"namespace Acme\n{\n    public sealed class Widget\n    {\n"
        b"        public static IThing ReadNode(BinaryReader reader, int depth) { }\n    }\n}\n"
    )
    executor = _install_ilspy(assay_root, monkeypatch, decompile=body)
    r = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol="Acme.Widget.Reader"))
    assert isinstance(r.detail, ApiResolution)
    assert r.status is RailStatus.UNSUPPORTED


def test_cs_member_case_drifted_needle_anchors_declaration(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A case-drifted member query anchors the real declaration through the insensitive declaration tier."""
    detail = _cs_surface(assay_root, monkeypatch, "Acme.Widget.spin")
    assert "public void Spin(int turns)" in detail.signature


def test_probe_ilspy_failure_carries_stderr_reason(assay_root: AssayHarness) -> None:
    """A failed version probe reports its first stderr line instead of a bare empty version."""
    executor = SeamExecutor(
        run_fn=lambda *_a, **_kw: RailProbe.receipt(
            ("ilspycmd",), 1, status=RailStatus.FAULTED, stderr=b"Cannot find a tool in the manifest file that has a command named 'ilspycmd'.\n"
        )
    )
    version, rc = oracle_mod.probe_ilspy(assay_root.settings, executor)
    assert rc == 1
    assert version.startswith("unavailable: Cannot find a tool")


def test_cs_surface_cache_hit_skips_reinvocation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A second C# surface query reads the fingerprint cache."""
    first = _cs_surface(assay_root, monkeypatch, "")
    # A hard-faulting executor on the second call proves the second result is cache-backed.
    rerun_guard = SeamExecutor(run_fn=lambda *_a, **_kw: RailProbe.error(("api",), "must-not-run"))
    second = _cs_surface(assay_root, monkeypatch, "", executor=rerun_guard)
    assert second.preview == first.preview


def test_cs_surface_empty_assemblies_is_empty_not_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A C# source with no assemblies stays off the fault rail."""
    empty_src = oracle_mod.Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=())
    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(empty_src))
    r = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Anything"))
    assert r.status is not RailStatus.FAULTED


# --- [SURFACE_CACHE]


@pytest.mark.parametrize("corruption", ["foreign-producer", "garbage-bytes"], ids=["foreign-producer", "garbage-bytes"])
def test_cs_surface_cache_corruption_is_miss_never_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, corruption: str) -> None:
    """A forged-producer or undecodable cache entry is a miss: the surface re-lists and re-persists, never replays or faults."""
    executor = _install_ilspy(assay_root, monkeypatch)
    first = assert_ok(_run(query, assay_root, executor, key="rhino-common", symbol=""))
    cache_paths = tuple(a.path for a in first.artifacts if a.path.endswith(".json"))
    assert cache_paths, "surface cache artifact missing from the INDEX report"
    store = assay_root.settings.store()
    match corruption:
        case "foreign-producer":
            entry = msgspec.json.decode(store.read_path(cache_paths[0]), type=oracle_mod.CacheEntry)
            store.write_bytes_path(msgspec.json.encode(msgspec.structs.replace(entry, producer="not-ilspycmd")), cache_paths[0])
        case _:
            store.write_bytes_path(b"\x00 not json", cache_paths[0])

    runs = {"n": 0}

    def _counting(_check: Check, **_kw: object) -> Result[object, Fault]:
        runs["n"] += 1
        return RailProbe.receipt(("ilspycmd",), 0, stdout=_ILSPY_TYPES)

    second = assert_ok(_run(query, assay_root, SeamExecutor(run_fn=_counting), key="rhino-common", symbol=""))
    assert runs["n"] >= 1  # the corrupted entry was rejected, so the roster re-listed
    assert second.notes == first.notes


# --- [PYDIST]


def test_pydist_query_roster_signature_and_window(assay_root: AssayHarness) -> None:
    """Pydist surface rosters live types via INPROC inspect; member queries introspect the real symbol with monotone grep and --full."""
    roster = assert_ok(_run(query, assay_root, key="msgspec", symbol="")).detail
    assert isinstance(roster, ApiSurface)
    assert roster.source.source_kind is SourceKind.PYDIST
    assert roster.lines == IsInt(ge=1)  # msgspec exposes Struct/Meta/Raw etc. as type captures

    detail = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture")).detail
    assert isinstance(detail, ApiSurface)
    assert "fixture" in detail.signature  # real inspect.signature of pytest.fixture
    assert detail.shape is SymbolShape.TYPE

    full_detail = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture", full=True)).detail
    grep_detail = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture", grep="def")).detail
    assert isinstance(full_detail, ApiSurface)
    assert isinstance(grep_detail, ApiSurface)
    assert not full_detail.truncated
    assert grep_detail.lines <= full_detail.lines  # grep is a monotone filter over the same source


# White-box block: the INPROC pydist thunk runs in-process with no subprocess boundary to observe publicly.
def test_pydist_thunk_introspection_roster_member_and_resolution() -> None:
    """_pydist_thunk rosters @type captures, anchors the member triple, and resolves symbols over importable prefixes."""
    roster = CAPTURES.decode(oracle_mod._pydist_thunk("msgspec", "")(_INPROC_CHECK).stdout)
    member = CAPTURES.decode(oracle_mod._pydist_thunk("msgspec", "Struct")(_INPROC_CHECK).stdout)
    assert any(cap.name == "type" and cap.text for cap in roster)
    assert [cap.name for cap in member] == ["signature", "doc", "full"]
    assert member[0].text.startswith("Struct")

    signature, doc, full = oracle_mod._member_captures(_DistDouble, "_DistDouble")
    assert (signature.name, doc.name, full.name) == ("signature", "doc", "full")
    assert doc.text.startswith("Distribution double")
    assert "msgspec" in oracle_mod._pydist_modules("msgspec")
    assert oracle_mod._pydist_modules("totally-not-installed-xyz") == ()
    resolved = oracle_mod._resolve_py_symbol("msgspec", "Struct")
    assert getattr(resolved, "__name__", "") == "Struct"
    assert not oracle_mod._object_source(len)  # C-builtin source is unreadable → empty, never a raise
    assert oracle_mod._annotations(42) == {}  # no annotation namespace → empty mapping


def test_inspect_kinds_roster_pins_pep695_alias_row() -> None:
    """_INSPECT_KINDS keeps the TypeAliasType row for PEP 695 aliases."""
    type Alias = int
    assert isinstance(Alias, TypeAliasType)
    alias_predicates = tuple(pred for cap, pred in oracle_mod._INSPECT_KINDS if cap == "type" and pred(Alias) and not pred(int))
    assert len(alias_predicates) == 1, "exactly one _INSPECT_KINDS row must accept a TypeAliasType and reject a plain class"
    assert all(cap == "type" for cap, _ in oracle_mod._INSPECT_KINDS)  # every roster row caps under @type


def test_signature_fallback_cases() -> None:
    """_signature renders annotations, synthetic params, or sentinel."""
    import types  # noqa: PLC0415  # local: a module object is the cleanest unsignable-but-annotated probe

    class ForwardType:
        pass

    def forward_handler(value: ForwardType) -> ForwardType:
        return value

    unsignable = types.ModuleType("fake_with_annotations")
    vars(unsignable)["__annotations__"] = {"cfg": "int", "name": "str"}

    class Holder:
        a: int

    for label, obj, expected in (
        ("typed-callable", forward_handler, IsStr(regex=r".*ForwardType.*")),  # annotationlib renders real annotations as strings
        ("synthesized-params", unsignable, "(cfg: int, name: str)"),  # annotationlib synthesis, not inspect.signature
        ("class-parens", Holder, IsStr(regex=r"\(.*\)")),  # non-callable class → parenthesized synthetic signature
        ("sentinel-no-annotations", Holder(), "(...)"),  # instance: class-level annotations are not on the instance
    ):
        assert oracle_mod._signature(obj) == expected, f"{label}: {oracle_mod._signature(obj)!r}"


def test_malformed_boundary_inputs_degrade_to_empty(assay_root: AssayHarness) -> None:
    """Malformed props, csproj, package.json, and XMLDoc inputs fold empty; a bad JSON field folds only itself."""
    assay_root.write("Directory.Packages.props", "<Project><PackageVersion Include='A' Version='1'/>")  # unterminated XML
    csproj = assay_root.write("src/App/App.csproj", "<bad")  # unparseable XML
    manifest = assay_root.write("node_modules/pkg/package.json", "{not json")
    bad_xml = assay_root.write("RhinoCommon.xml", "<doc><member")
    assert oracle_mod.packages(assay_root.settings) == {}
    assert oracle_mod._project_references(csproj) == ()
    assert not oracle_mod._json_fields(manifest, "version")["version"]
    assert oracle_mod._xml_members(bad_xml) == ()

    sibling = assay_root.write("node_modules/pkg2/package.json", '{"types": 5, "version": "2.1.0"}')
    assert oracle_mod._json_fields(sibling, "types", "typings", "version") == {"types": "", "typings": "", "version": "2.1.0"}


def test_pydist_metadata_boundary_degradation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_pydist_source handles files=None dists; inventory skips nameless dists and folds root-lookup OSErrors."""
    import importlib.metadata as importlib_metadata  # noqa: PLC0415  # local: drive the stdlib metadata boundary

    assert oracle_mod._pydist_source("pytest") is not None  # live dist resolves without a file manifest requirement

    class _OsErrorDist(_DistDouble):
        @override
        def locate_file(self, _rel: str) -> str:
            raise OSError("no root")

    root = str(assay_root.root)
    monkeypatch.setattr(importlib_metadata, "distribution", lambda _key: _DistDouble("FilesNone", root=root))
    source = oracle_mod._pydist_source("filesnone")
    assert source is not None
    assert source.kind is SourceKind.PYDIST
    assert source.asset_paths == ()  # files=None → no per-file assets

    dists = (_DistDouble("", root=root), _DistDouble("Good", root=root), _OsErrorDist("Broken", root=root))
    monkeypatch.setattr(importlib_metadata, "distributions", lambda: iter(dists))
    rows = oracle_mod.pydist_inventory_sources()
    assert {row.source_id for row in rows} == {"Good", "Broken"}  # the nameless dist was skipped (the continue arm)
    assert all(row.source_kind is SourceKind.PYDIST for row in rows)  # OSError dist still produced a row


# --- [TSDECL]


def test_tsdecl_query_rosters_declarations_and_anchors_member_signature(assay_root: AssayHarness) -> None:
    """A TS surface query rosters every exported .d.ts declaration; a member query anchors its first-line signature."""
    _ts_package(assay_root, "export interface Foo { x: string }\nexport declare class Bar { y: number }\nexport type Baz = string;\n")
    r = assert_ok(_run(query, assay_root, key="mypkg", symbol=""))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.source_kind is SourceKind.TSDECL
    assert {"Foo", "Bar", "Baz"} <= set(detail.preview.splitlines())

    member_detail = assert_ok(_run(query, assay_root, key="mypkg", symbol="Foo")).detail
    assert isinstance(member_detail, ApiSurface)
    assert "interface Foo" in member_detail.signature
    assert "number" not in member_detail.signature, "Foo lookup must not leak Bar members"


# White-box block: the tsdecl thunk's alias capture, owner scoping, saturation, and read-failure arms have no CLI discriminator.
def test_tsdecl_thunk_capture_matrix(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_tsdecl_thunk captures aliases and parse errors, scopes owner-qualified members, saturates at RESULT_CAP, and folds unreadable paths."""
    good = assay_root.write("pkg/index.d.ts", 'export { Foo as Bar, Baz } from "./x";\nexport interface Thing { value: string }\n')
    bad = assay_root.write("pkg/bad.d.ts", "export interface Broken {")
    source = oracle_mod.Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=good.parent, asset_paths=(good, bad))
    roster = CAPTURES.decode(oracle_mod._tsdecl_thunk(source, "")(_TS_CHECK).stdout)
    member = CAPTURES.decode(oracle_mod._tsdecl_thunk(source, "Bar")(_TS_CHECK).stdout)
    assert {"Bar", "Baz", "Thing"} <= {cap.text for cap in roster if cap.name == "type"}
    assert any(cap.parse_error for cap in roster)
    assert member[0].name == "signature"
    assert "Foo as Bar" in member[0].text

    scoped = assay_root.write("pkg2/index.d.ts", "export interface Foo { x: string }\nexport interface Bar { x: number }\n")
    scoped_source = oracle_mod.Source("pkg2", SourceKind.TSDECL, "1.0.0", package_root=scoped.parent, asset_paths=(scoped,))
    owner_member = CAPTURES.decode(oracle_mod._tsdecl_thunk(scoped_source, "Foo.x")(_TS_CHECK).stdout)
    assert "string" in owner_member[0].text
    assert "number" not in owner_member[0].text  # owner-qualified lookup never leaks the same-named sibling member

    monkeypatch.setattr(oracle_mod, "RESULT_CAP", 1)
    capped = tuple(cap for cap in CAPTURES.decode(oracle_mod._tsdecl_thunk(scoped_source, "")(_TS_CHECK).stdout) if cap.name == "type")
    assert len(capped) == 1  # capped at the patched RESULT_CAP
    assert all(cap.truncated for cap in capped)  # saturation surfaces the same way code.py surfaces truncated

    from tree_sitter import Parser as TSParser  # noqa: PLC0415  # local: construct the real parser for the read-fail probe

    parser = TSParser(ts_language(oracle_mod._TS_GRAMMAR))
    assert oracle_mod._ts_captures(parser, assay_root.root / "node_modules" / "ghost" / "absent.d.ts", "") == ()

    assay_root.write("node_modules/alpha/package.json", "{}")
    assay_root.write("node_modules/@scope/beta/package.json", "{}")
    assay_root.write("node_modules/.bin/placeholder", "")  # dot-prefixed → excluded
    assert oracle_mod.tsdecl_names(assay_root.settings) == ("@scope/beta", "alpha")  # hoisted + scoped, sorted


def test_tsdecl_thunk_query_error_surfaces_capture(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A malformed roster query surfaces as a single query_error capture, mirroring the code rail."""
    from tree_sitter import QueryError  # noqa: PLC0415  # local: only this law needs the tree-sitter error type

    dts = assay_root.write("pkg/index.d.ts", "export interface Foo { x: string }\n")
    source = oracle_mod.Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=dts.parent, asset_paths=(dts,))
    monkeypatch.setattr(oracle_mod, "ts_query", lambda *_a, **_kw: Error(QueryError("bad roster query")))
    roster = CAPTURES.decode(oracle_mod._tsdecl_thunk(source, "")(_TS_CHECK).stdout)
    assert any(cap.name == "query_error" and cap.parse_error for cap in roster)


# --- [NUGET]


def test_nuget_source_carries_inventory_facts(assay_root: AssayHarness) -> None:
    """nuget_source resolves package root, ranked frameworks, owners, and primary-stem asset ordering."""
    package_root = _nuget_fixture(assay_root)
    source = oracle_mod.nuget_source(assay_root.settings, "Pkg.Core", "1.2.3")
    detail = oracle_mod.to_api_source(source)
    assert source.package_root == package_root
    assert source.frameworks == ("net8.0", "netstandard2.0")  # tfm_rank ordering against the workspace floor
    assert source.tfm == "net8.0"  # the consumer-bound framework is stamped on the source
    assert source.owners == ("src/App/App.csproj",)
    assert isinstance(detail, ApiSource)
    assert detail.restore == "restored"
    assert detail.tfm == "net8.0"  # the chosen TFM rides the wire detail
    assert any(p.endswith("Pkg.Core.dll") for p in detail.assets)


@pytest.mark.parametrize("kind", _PRESENT_KINDS, ids=list(_PRESENT_KINDS))
def test_resolve_nuget_kind_path_rows(assay_root: AssayHarness, kind: str) -> None:
    """Resolve over a restored NuGet package emits path rows per concrete present kind with OK status."""
    _nuget_fixture(assay_root)
    r = assert_ok(_run(resolve, assay_root, key="Pkg.Core", kind=kind))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.source_kind is SourceKind.NUGET
    assert r.status is RailStatus.OK  # at least one path present for every concrete kind
    assert r.results  # path rows ride inline; the full-listing artifact appears only when results saturate the cap


@pytest.mark.parametrize(
    "key, expect_ok, expect_reason",
    [
        ("Pkg.Core", True, ""),  # exact → resolved
        ("Pkg.C", False, "ambiguous"),  # prefix matches Core + Cache, so ambiguity is explicit
        ("zzz-nope", False, "unknown"),  # no match → unknown with nearest candidates
    ],
    ids=["exact", "ambiguous", "unknown"],
)
def test_resolve_key_fuzzy_dispatch(key: str, expect_ok: bool, expect_reason: str) -> None:  # noqa: FBT001  # parametrized bool flag
    """resolve_key discriminates exact / ambiguous / unknown over the NuGet package map."""
    packages = {"Pkg.Core": "1.0.0", "Pkg.Cache": "2.0.0", "Other": "3.0.0"}
    result = oracle_mod.resolve_key(packages, key)
    match result:
        case Result(tag="ok", ok=hit):
            assert expect_ok
            assert hit == "Pkg.Core"
        case Result(tag="error", error=resolution):
            assert not expect_ok
            assert resolution.reason == expect_reason
            assert resolution.candidates  # every miss carries nearest candidates
        case _:  # pragma: no cover  # Result is a closed two-tag union; the ok/error arms are exhaustive
            pytest.fail(f"unexpected Result shape: {result!r}")


def test_resolve_ambiguous_nuget_falls_through_to_unknown(assay_root: AssayHarness) -> None:
    """Ambiguous NuGet keys degrade to the aggregate unknown-source miss."""
    assay_root.write(
        "Directory.Packages.props",
        "<Project><ItemGroup>"
        '<PackageVersion Include="Pkg.Core" Version="1.0.0" />'
        '<PackageVersion Include="Pkg.Cache" Version="2.0.0" />'
        "</ItemGroup></Project>",
    )
    r = assert_ok(_run(resolve, assay_root, key="Pkg.C"))
    assert r.status is RailStatus.UNSUPPORTED
    assert isinstance(r.detail, ApiResolution)
    assert r.detail.reason == "unknown"
    assert r.detail.candidates  # nearest NuGet/pydist/tsdecl names


# --- [TFM_POLICY]


@pytest.mark.parametrize(
    "tfms, expected",
    [
        (("netstandard2.0", "net5.0"), "net5.0"),  # the RectpackSharp shape: the consumer binds net5.0, not netstandard2.0
        (("netstandard2.0", "netstandard2.1"), "netstandard2.1"),
        (("net8.0", "net10.0", "netstandard2.0"), "net10.0"),
        (("net11.0", "netstandard2.0"), "netstandard2.0"),  # above-floor modern TFM is incompatible with the consumer
        (("net48", "netcoreapp3.1"), "netcoreapp3.1"),
    ],
    ids=["net5-beats-netstandard", "netstandard-ordering", "floor-exact-wins", "above-floor-excluded", "coreapp-beats-framework"],
)
def test_nuget_source_binds_consumer_tfm(assay_root: AssayHarness, tfms: tuple[str, ...], expected: str) -> None:
    """NuGet resolution ranks lib/<tfm> against the workspace floor so primary_assembly is the bound asset."""
    assay_root.write("Directory.Build.props", "<Project><PropertyGroup><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>")
    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Multi" Version="1.0.0" /></ItemGroup></Project>')
    for tfm in tfms:
        assay_root.write(f".cache/nuget/packages/pkg.multi/1.0.0/lib/{tfm}/Pkg.Multi.dll", "MZ")
    source = oracle_mod.nuget_source(assay_root.settings, "Pkg.Multi", "1.0.0")
    assert source.tfm == expected
    assert f"/lib/{expected}/" in str(source.assemblies[0])
    assert source.frameworks[0] == expected  # compatible candidates rank first, best first
    assert oracle_mod.to_api_source(source).tfm == expected  # the choice is stamped on the wire detail


def test_consumer_tfm_floor_and_resolve_detail_carry_bound_tfm(assay_root: AssayHarness) -> None:
    """The TFM floor reads Directory.Build.props (net10.0 fallback), and the resolve verb stamps the consumer-bound TFM."""
    assert oracle_mod.consumer_tfm_floor(assay_root.settings) == (10, 0)  # fallback: tmp tree has no props
    assay_root.write("Directory.Build.props", "<Project><PropertyGroup><TargetFramework>net9.0</TargetFramework></PropertyGroup></Project>")
    assert oracle_mod.consumer_tfm_floor(assay_root.settings) == (9, 0)

    assay_root.write("Directory.Packages.props", '<Project><ItemGroup><PackageVersion Include="Pkg.Span" Version="1.0.0" /></ItemGroup></Project>')
    for tfm in ("netstandard2.0", "net5.0"):
        assay_root.write(f".cache/nuget/packages/pkg.span/1.0.0/lib/{tfm}/Pkg.Span.dll", "MZ")
    detail = assert_ok(_run(resolve, assay_root, key="Pkg.Span", kind="assembly")).detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.tfm == "net5.0"  # provable absence speaks for the CONSUMED framework, not a fallback


# --- [ENGINE_BOUNDARY]


def test_invoke_error_rail_yields_nonzero_completed(assay_root: AssayHarness) -> None:
    """_invoke maps an executor Error to non-zero Completed stderr."""
    # White-box: the Error→Completed mapping happens before any report fold, so no public verb can isolate it.
    executor = SeamExecutor(run_fn=lambda *_a, **_kw: RailProbe.error(("api",), "spawn boom"))
    surface_tool = next(t for t in select(Claim.API, Language.CSHARP))
    done = oracle_mod._invoke(assay_root.settings, executor, surface_tool, ToolArgs())
    assert done.returncode == 1
    assert b"spawn boom" in done.stderr


@pytest.mark.parametrize(
    "key, kind, symbol, message",
    [
        ("rhino-common", SourceKind.ASSEMBLY, "", "no ilspycmd catalog row"),  # C# surface select-None
        ("pytest", SourceKind.PYDIST, "", "no python INPROC api row"),  # pydist INPROC select-None
    ],
    ids=["cs-surface", "inproc-surface"],
)
def test_surface_faults_when_catalog_row_missing(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, key: str, kind: SourceKind, symbol: str, message: str
) -> None:
    """Missing API catalog rows fault C# and INPROC surfaces precisely."""
    asm = assay_root.write("RhinoCommon.dll" if kind is SourceKind.ASSEMBLY else "dummy.py", "MZ")
    source = oracle_mod.Source(
        key=key, kind=kind, assemblies=(asm,) if kind is SourceKind.ASSEMBLY else (), asset_paths=() if kind is SourceKind.ASSEMBLY else (asm,)
    )
    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(source))
    monkeypatch.setattr(oracle_mod, "select", lambda _claim, _lang: iter(()))

    e = assert_error(_run(query, assay_root, SeamExecutor(), key=key, symbol=symbol))  # lane-less: the fault lands before any spawn
    assert e.status is RailStatus.FAULTED
    assert message in e.message


def test_cs_decompile_faults_when_catalog_row_missing(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Catalog loss between C# listing and decompile faults the decompile arm."""
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    source = oracle_mod.Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,))
    calls = {"n": 0}

    def _staged(_claim: Claim, _lang: Language) -> object:
        calls["n"] += 1
        return iter(select(Claim.API, Language.CSHARP)) if calls["n"] == 1 else iter(())

    monkeypatch.setattr(oracle_mod, "_resolve_source", lambda _settings, _key: Ok(source))
    monkeypatch.setattr(oracle_mod, "select", _staged)
    executor = SeamExecutor(run_fn=lambda *_a, **_kw: RailProbe.receipt(("ilspycmd",), 0, stdout=_ILSPY_TYPES))

    e = assert_error(_run(query, assay_root, executor, key="rhino-common", symbol="Acme.Widget"))
    assert e.status is RailStatus.FAULTED
    assert "no ilspycmd catalog row" in e.message
