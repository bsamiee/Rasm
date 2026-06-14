"""Law matrix for tools.assay.rails.api public surface.

Covers: ApiParams, doctor, query, resolve, shape_of, show.

Laws:
    shape_of: parametrized case-table over all SymbolShape arms; every arm is falsifiable.
    ApiParams.bound: parametrized verb-x-positional case-table; surplus tokens produce Fault.
    ApiParams.sources: tuple default; prefix filter is monotone.
    doctor: prefix-subset monotone + strict-mode Fault promotion.
    resolve: miss yields UNSUPPORTED + ApiResolution; bad kind yields kind candidates.
    show: token cap / truncation; absent token yields EMPTY; latest prefers API scope.
    query: known pydist key produces Ok report.
    spec laws: ApiResolution / ApiSource / ApiSurface field roundtrip identity.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from dataclasses import replace
import re
from typing import override, TYPE_CHECKING, TypeAliasType

from dirty_equals import IsInt, IsList, IsStr
from expression import Error, Ok, Result  # runtime: canned run_check replacements return Result instances
from hypothesis import given as hyp_given, settings as hyp_settings, strategies as st
from inline_snapshot import external, outsource  # external-file goldens for the large rendered C# decompile surface
import pytest

from tests.python._testkit.laws import register_law, spec
from tests.python._testkit.spec import assert_error, assert_ok, assert_roundtrip, refutes, support_matrix, validity_matrix, ValidityCase
from tests.python._testkit.strategies import resolve as st_resolve  # aliased to avoid collision with tools.assay.rails.api.resolve verb
from tests.python.tools.assay.kit import RailProbe
from tools.assay.composition.catalog import CAPTURES, select
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
    Runner,
    SourceKind,
    SymbolShape,
    Tool,
)
from tools.assay.core.status import RailStatus
from tools.assay.rails import api as api_rail
from tools.assay.rails.api import ApiParams, doctor, query, resolve, shape_of, show
from tools.assay.rails.code import ts_language  # shared tree-sitter primitive owned by code.py, re-bound in api.py


if TYPE_CHECKING:
    from collections.abc import Callable
    from pathlib import Path

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import ArtifactScope, AssaySettings
    from tools.assay.core.model import Report

    type Verb = Callable[[AssaySettings, ArtifactScope, ApiParams], Result[Report, Fault]]  # query/resolve/show/doctor share this shape


# --- [CONSTANTS] ------------------------------------------------------------------------

# All valid _PathKind tokens per _PATH_KINDS constant in api.py.
_VALID_KINDS: tuple[str, ...] = ("all", "assembly", "xml", "nuspec", "deps", "package-root")

# Canned ilspycmd `-l cisde` type listing: `Kind FullyQualifiedName` per line (parsed by _parse_surface;
# parts[0] must be in _SURFACE_KINDS). Four kinds across one namespace so namespace/type/member dispatch fires.
_ILSPY_TYPES: bytes = b"Class Acme.Widget\nStruct Acme.Point\nEnum Acme.Color\nInterface Acme.IThing\n"

# Canned ilspycmd `-t` decompile window. The Spin member sits at a non-zero offset so the anchor search runs;
# the trailing distinct lines drive max_lines truncation and the grep filter selects the Spin line only.
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

# Canned sidecar XMLDoc keyed by member element name (T:/M: prefix stripped, dotted-suffix matched by _xml_doc).
_ILSPY_XML: str = (
    "<doc><members>"
    '<member name="T:Acme.Widget"><summary>A widget that spins.</summary></member>'
    '<member name="M:Acme.Widget.Spin(System.Int32)"><summary>Spins the widget.</summary></member>'
    "</members></doc>"
)

_INPROC_CHECK: Check = Check(tool=Tool("py-api", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.API, mode=Mode.QUERY))
_TS_CHECK: Check = Check(tool=Tool("ts-api", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.API, mode=Mode.QUERY))

_TEN_LINES: str = "\n".join(f"line{i}" for i in range(1, 11))

# deps excluded: the fixture lays down no build/analyzers/runtimes dirs, so its target set is empty → EMPTY not OK.
_PRESENT_KINDS: tuple[str, ...] = ("all", "assembly", "xml", "nuspec", "package-root")

# --- [MODELS] ---------------------------------------------------------------------------


class _DistDouble:
    """Realistic importlib.metadata.Distribution double driving the pydist source/inventory edge arms.

    ``files`` is None so the file-manifest-absent arm of _pydist_source is exercised; the OSError
    fold-to-empty-root arm of the inventory loop is driven by the _OsErrorDist subclass below.
    """

    def __init__(self, name: str, *, root: str) -> None:
        self.metadata: dict[str, str] = {"Name": name}
        self.version = "1.0"
        self.files = None
        self._root = root

    def locate_file(self, _rel: str) -> str:
        return self._root


# --- [OPERATIONS] -----------------------------------------------------------------------


def _run(verb: Verb, assay_root: AssayHarness, **params: object) -> Result[Report, Fault]:
    """Run an api verb over ApiParams(**params) in a fresh Claim.API scope.

    Collapses the verb(settings, scope, ApiParams(...)) triple so no test needs an inline scope binding.

    Returns:
        The verb's Result[Report, Fault] over the isolated tmp tree.
    """
    return verb(
        assay_root.settings,
        assay_root.scope(Claim.API),
        ApiParams(**params),  # ty: ignore[invalid-argument-type]  # **params is the open keyword set forwarded into the typed ApiParams ctor
    )


def _install_ilspy(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    *,
    types: bytes = _ILSPY_TYPES,
    decompile: bytes = _ILSPY_DECOMPILE,
    returncode: int = 0,
    xmls: bool = False,
) -> None:
    """Pin api_rail.run_check and _source to canned ilspycmd output over a fabricated RhinoCommon assembly.

    The replacement discriminates on -t (decompile) vs the type-listing surface call, mirroring real
    ilspycmd invocation. Both branches build their receipt through RailProbe.receipt.
    """
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    xml_paths = (assay_root.write("RhinoCommon.xml", _ILSPY_XML),) if xmls else ()
    source = api_rail._Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,), xmls=xml_paths)
    status = RailStatus.OK if returncode == 0 else RailStatus.FAULTED

    def _canned(check: Check, **_kw: object) -> Result[object, Fault]:
        payload = decompile if "-t" in check.tool.command else types
        return RailProbe.receipt(
            ("ilspycmd",), returncode, status=status, stdout=payload if returncode == 0 else b"", stderr=b"" if returncode == 0 else b"ilspy boom"
        )

    monkeypatch.setattr(api_rail, "run_check", _canned)
    monkeypatch.setattr(api_rail, "_source", lambda _settings, _key: Ok(source))


def _cs_surface(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, symbol: str, *, install: bool = True, **kw: object) -> ApiSurface:
    """Query symbol over canned ilspycmd output and return the asserted ApiSurface detail.

    Returns:
        The ApiSurface detail of the Ok query report over the canned C# source.
    """
    match install:
        case True:
            _install_ilspy(assay_root, monkeypatch)
        case False:
            pass
    detail = assert_ok(_run(query, assay_root, key="rhino-common", symbol=symbol, **kw)).detail
    assert isinstance(detail, ApiSurface)
    return detail


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
    """shape_of dispatches every SymbolShape arm and is idempotent on a canonical representative.

    Falsified by a mutant that collapses any arm onto another (e.g. drops the dotted-vs-cased
    discriminant): the refutes witness below proves the dispatch table is not a tautology — a
    deliberately wrong (symbol, expected) pair makes the case-table law raise.
    """
    assert shape_of(symbol) is expected
    assert shape_of(symbol) is shape_of(symbol)


def _shape_law(symbol: str, expected: SymbolShape) -> None:
    """Oracle: shape_of(symbol) must be ``expected``; raises AssertionError otherwise.

    Driven both as a positive law (every parametrize row) and inverted by ``refutes`` with a
    deliberately wrong expected, proving the dispatch table discriminates rather than returning a constant.
    """
    assert shape_of(symbol) is expected, f"shape_of({symbol!r}) = {shape_of(symbol)} != {expected}"


def test_shape_of_dispatch_is_falsifiable() -> None:
    """``_shape_law`` raises on a wrong (symbol, shape) pair — the dispatch table is not vacuous.

    Without a witness, a mutant collapsing shape_of to a constant SymbolShape could survive whenever a
    parametrize row happens to expect that constant. refutes pins each shape against a deliberately wrong
    expectation, so a non-discriminating shape_of cannot pass: every arm must reject the other arms' values.
    """
    refutes("System", _shape_law, SymbolShape.MEMBER)  # NAMESPACE-shaped, MEMBER expected → must raise
    refutes("System.String", _shape_law, SymbolShape.INDEX)  # TYPE-shaped, INDEX expected → must raise
    refutes("System.String.Contains()", _shape_law, SymbolShape.TYPE)  # MEMBER-shaped, TYPE expected → must raise
    _shape_law("", SymbolShape.INDEX)  # the same oracle proves the positive arm holds (empty → INDEX)


def test_inspect_kinds_roster_pins_pep695_alias_row() -> None:
    """_INSPECT_KINDS keeps the PEP 695 ``("type", TypeAliasType)`` row that rosters ``type X = ...`` aliases.

    The api rail folds every (cap, predicate) row in _INSPECT_KINDS to roster a pydist's public surface;
    dropping the TypeAliasType predicate silently stops type-alias members from appearing in a surface
    query. This law fails if the alias row leaves the roster — proven by sampling the row's predicate
    against a real ``type`` alias (True) and a plain class (False), so it cannot pass on a renamed/empty row.
    """
    type Alias = int  # a real PEP 695 alias object; isinstance(Alias, TypeAliasType) is True
    assert isinstance(Alias, TypeAliasType)  # anchor: the alias row must accept exactly this object class

    alias_predicates = tuple(pred for cap, pred in api_rail._INSPECT_KINDS if cap == "type" and pred(Alias) and not pred(int))
    assert len(alias_predicates) == 1, "exactly one _INSPECT_KINDS row must accept a TypeAliasType and reject a plain class"
    assert [cap for cap, _ in api_rail._INSPECT_KINDS] == IsList("type", "type", "type")  # every roster row caps under @type


register_law(shape_of, "shape_of_dispatch")
register_law(shape_of, "shape_of_idempotent")
register_law(shape_of, "shape_of_dispatch_falsifiable")
register_law(query, "inspect_kinds_pep695_alias_row")

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


register_law(ApiParams, "api_params_bound_dispatch")


def test_api_params_bound_surplus_is_fault() -> None:
    """ApiParams.bound with surplus positional tokens produces a Fault, not a params object."""
    result = ApiParams(paths=("a", "b", "c")).bound("query")  # query arity is 1; b,c are surplus
    assert isinstance(result, Fault)
    assert "query" in result.message


register_law(ApiParams, "api_params_bound_surplus")


def test_api_params_bound_unknown_verb_passthrough() -> None:
    """ApiParams.bound for a verb with no positional slots (e.g. doctor) returns self unchanged."""
    p = ApiParams()
    assert p.bound("doctor") is p  # doctor arity 0, no paths → fall-through case _


register_law(ApiParams, "api_params_bound_passthrough")


def test_api_params_sources_default_empty() -> None:
    """ApiParams.sources defaults to an empty tuple (no prefix restriction)."""
    assert ApiParams().sources == ()


register_law(ApiParams, "api_params_sources_default")


@spec(ApiParams, law="api_params_field_identity")
def test_api_params_field_identity(p: ApiParams) -> None:
    """ApiParams fields survive a copy-replace round-trip (struct identity)."""
    assert replace(p, key=p.key) == p


# --- [DOCTOR]


def test_doctor_returns_ok_report(assay_root: AssayHarness) -> None:
    """Doctor always returns an Ok Result in the absence of Rhino bundles."""
    r = assert_ok(_run(doctor, assay_root))
    assert r.verb == "doctor"
    assert r.counts.total == IsInt(ge=1)


register_law(doctor, "doctor_returns_ok_report")


def _doctor_lines(assay_root: AssayHarness, sources: tuple[str, ...]) -> int:
    detail = assert_ok(_run(doctor, assay_root, sources=sources)).detail
    return detail.lines if isinstance(detail, ApiSurface) else -1


@pytest.mark.parametrize("prefix", ["rhino-", "eto", "ilspy", "python-dists", "ts-decls"], ids=["rhino", "eto", "ilspy", "py-dists", "ts-decls"])
def test_doctor_sources_prefix_filters(assay_root: AssayHarness, prefix: str) -> None:
    """Doctor with a sources prefix reduces the inventory monotonically (filtered lines <= unfiltered)."""
    lines_all = _doctor_lines(assay_root, ())
    lines_flt = _doctor_lines(assay_root, (prefix,))
    assert lines_flt == IsInt(ge=0)
    assert lines_flt <= lines_all, f"prefix {prefix!r}: {lines_flt} > {lines_all}"


def test_doctor_sources_validity_matrix(assay_root: AssayHarness) -> None:
    """Validity matrix over the doctor sources prefix filter: all >= matching >= non-matching == 0.

    Subsumes the nomatch-prefix-zero law (none == 0) and catches a mutant that breaks _filtered_sources.
    """
    lines_all = _doctor_lines(assay_root, ())
    lines_rhino = _doctor_lines(assay_root, ("rhino-",))
    lines_none = _doctor_lines(assay_root, ("xyz-no-match-xyzzy-",))
    validity_matrix(
        [
            ValidityCase("all>=rhino", lines_all, lines_all >= lines_rhino),
            ValidityCase("rhino>=none", lines_rhino, lines_rhino >= lines_none),
            ValidityCase("none==0", lines_none, lines_none == 0),
        ],
        lambda v: v >= 0,  # predicate: a valid line count is non-negative
    )
    assert lines_all >= lines_rhino >= lines_none == 0  # falsifiable by a defect in _filtered_sources
    # validity_matrix is non-vacuous: a case whose expected flag contradicts the predicate must raise.
    # This pins that the matrix oracle truly compares (it would silently pass any input if it ignored `expected`).
    refutes([ValidityCase("contradiction", lines_none, expected=True)], validity_matrix, lambda v: v > 0)  # 0 > 0 is False ≠ True


register_law(doctor, "doctor_sources_prefix_monotone")
register_law(doctor, "doctor_nomatch_prefix_zero")
register_law(doctor, "doctor_sources_validity_matrix")


def test_doctor_strict_promotes_fault_when_not_all_ok(assay_root: AssayHarness) -> None:
    """Doctor strict=True promotes an incomplete inventory to a FAULTED error rail."""
    result = _run(doctor, assay_root, strict=True)
    # In a minimal tmp-tree without Rhino bundles, doctor status is EMPTY → strict faults.
    match result.tag:
        case "error":
            assert assert_error(result).status is RailStatus.FAULTED
        case "ok":
            assert result.ok.status is RailStatus.OK  # unexpectedly fully-OK environment → strict passes through


register_law(doctor, "doctor_strict_promotes_fault")


def test_doctor_inventory_includes_nuget_and_polyglot_rows(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Doctor folds a populated NuGet package plus polyglot summary rows into a single inventory report."""
    _nuget_fixture(assay_root)
    # Pin the version probe so the ilspycmd row reports a concrete version instead of 'unavailable'.
    monkeypatch.setattr(api_rail, "run_check", lambda *_a, **_kw: RailProbe.receipt(("ilspycmd",), 0, stdout=b"ilspycmd: 9.1.0.7988\n"))

    r = assert_ok(_run(doctor, assay_root))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert "ilspycmd" in detail.preview  # canned version row surfaced
    assert "python-dists" in detail.preview  # polyglot summary row present
    assert r.artifacts  # inventory json + tsv artifacts written
    assert detail.lines == IsInt(ge=3)  # rhino-app + ilspycmd + at least the NuGet package


register_law(doctor, "doctor_inventory_population")


def test_doctor_inventory_artifacts_live_under_retained_api_claim_root(assay_root: AssayHarness) -> None:
    """Doctor inventory artifacts live under the API claim root so retain_scopes(Claim.API) owns them."""
    r = assert_ok(_run(doctor, assay_root, sources=("ilspy",)))
    store = assay_root.settings.store()
    run_id = assay_root.settings.run_id
    expected = (store.path(Claim.API.value, run_id, "doctor-inventory.json"), store.path(Claim.API.value, run_id, "doctor-inventory.tsv"))
    assert tuple(a.path for a in r.artifacts) == expected
    assert all(a.kind is ArtifactKind.SCOPE for a in r.artifacts)
    assert store.exists(Claim.API.value, run_id, "doctor-inventory.json")
    assert store.exists(Claim.API.value, run_id, "doctor-inventory.tsv")
    assert not store.exists(ArtifactKind.SCOPE.value, Claim.API.value, run_id, "doctor-inventory.json")
    assert not store.exists(ArtifactKind.SCOPE.value, Claim.API.value, run_id, "doctor-inventory.tsv")


register_law(doctor, "doctor_inventory_artifacts_retained_api_claim_root")


def test_inventory_sources_keep_full_rows_without_pydist_file_expansion(assay_root: AssayHarness) -> None:
    """_inventory_sources keeps full host/NuGet rows yet lists pydist rows without per-file asset expansion."""
    _nuget_fixture(assay_root, owner=False)
    sources = api_rail._inventory_sources(assay_root.settings, None, "ilspycmd: 9.1", 0)
    by_id = {row.source_id for row in sources}
    nuget = next(row for row in sources if row.source_id == "Pkg.Core")
    pydist = next(row for row in sources if row.source_kind is SourceKind.PYDIST and row.source_id != "python-dists")
    assert {"rhino-app", "ilspycmd", "rhino-code-remote", "python-dists", "ts-decls"} <= by_id
    assert nuget.assets == ()  # doctor inventory skips NuGet asset expansion (include_assets=False)
    assert pydist.package_root  # pydist row keeps its root...
    assert pydist.assets == ()  # ...but not its per-file asset list


register_law(doctor, "inventory_sources_full_rows")


def test_doctor_result_ids_are_identity_shaped(assay_root: AssayHarness) -> None:
    """Doctor inventory results are keyed inventory:<source_id> — stable across runs so delta's (id, line) compare holds."""
    first = assert_ok(_run(doctor, assay_root))
    second = assert_ok(_run(doctor, assay_root))
    assert first.results
    assert all(m.id.startswith("inventory:") for m in first.results)
    assert tuple(m.id for m in first.results) == tuple(m.id for m in second.results)


register_law(doctor, "doctor_result_ids_identity_shaped")


_DOCTOR_ROW = re.compile(r"^\S+ status=\S+ assembly=(?:present|missing) xml=(?:present|missing) version=\S+$")


def test_doctor_row_text_is_stable_key_value_grammar(assay_root: AssayHarness) -> None:
    """Each doctor inventory ``Match.text`` follows the fixed ``key=value`` health grammar parsers key off.

    Pins ``<source_id> status=<status> assembly=present|missing xml=present|missing version=<version|->`` —
    fixed key order, single-space-separated — so a downstream parser reads named keys, not positions. A
    presence-token flip (``present`` <-> ``missing``) or a dropped key fails the per-row fullmatch.
    """
    report = assert_ok(_run(doctor, assay_root))
    assert report.results
    for match in report.results:
        assert _DOCTOR_ROW.fullmatch(match.text), f"doctor row text broke the key=value grammar: {match.text!r}"
        keys = tuple(token.split("=", 1)[0] for token in match.text.split(" ")[1:])
        assert keys == ("status", "assembly", "xml", "version"), f"doctor row key order drifted: {match.text!r}"


register_law(doctor, "doctor_row_text_is_stable_key_value_grammar")

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
    """Resolve misses stay UNSUPPORTED with a typed ApiResolution (reason + candidates); strict faults the rail.

    One case-table covers unknown-key, bad-kind, NUL-codec-boundary, and strict-promotion in a single
    falsifiable law.
    """
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


register_law(resolve, "resolve_unknown_key_unsupported")
register_law(resolve, "resolve_bad_kind_unsupported_with_candidates")
register_law(resolve, "resolve_codec_boundary_miss")
register_law(resolve, "resolve_strict_fault_on_miss")


def _kind_not_rejected(assay_root: AssayHarness, kind: str) -> bool:
    # pytest is always a resolvable pydist key in the test environment; valid kinds never emit unknown-kind.
    r = assert_ok(_run(resolve, assay_root, key="pytest", kind=kind))
    return not isinstance(r.detail, ApiResolution) or r.detail.reason != "unknown-kind"


@pytest.mark.parametrize("kind", _VALID_KINDS, ids=list(_VALID_KINDS))
def test_resolve_valid_kinds_accepted(assay_root: AssayHarness, kind: str) -> None:
    """Resolve accepts all documented kind tokens without an UNSUPPORTED kind error (may be EMPTY)."""
    assert _kind_not_rejected(assay_root, kind), f"kind={kind!r} unexpectedly rejected"


register_law(resolve, "resolve_valid_kinds_all_accepted")


def test_resolve_kind_support_matrix(assay_root: AssayHarness) -> None:
    """Support matrix: every documented kind token is accepted (no unknown-kind rejection)."""
    support_matrix(*((kind, lambda k=kind: _kind_not_rejected(assay_root, k), True) for kind in _VALID_KINDS))


register_law(resolve, "resolve_kind_support_matrix")


def test_resolve_pydist_key_ok(assay_root: AssayHarness) -> None:
    """Resolve with a live pydist key returns Ok with ApiSurface detail (not ApiResolution)."""
    r = assert_ok(_run(resolve, assay_root, key="pytest", kind="all"))
    assert r.status in {RailStatus.OK, RailStatus.EMPTY}
    assert isinstance(r.detail, ApiSurface)
    assert r.detail.source.source_kind is SourceKind.PYDIST


register_law(resolve, "resolve_pydist_key_ok")

# --- [SHOW]


def _show_detail(assay_root: AssayHarness, content: str, **params: object) -> ApiSurface:
    token = assay_root.settings.store().write_text(content, "scope", "api", "pkg", "data.txt").rsplit("/", 1)[-1]
    detail = assert_ok(_run(show, assay_root, token=token, **params)).detail
    assert isinstance(detail, ApiSurface)
    return detail


def test_show_absent_token_yields_empty(assay_root: AssayHarness) -> None:
    """Show with a token that matches no artifact returns Ok(EMPTY)."""
    absent = "not-found-abc-xyz-123"  # an artifact token guaranteed to match nothing
    r = assert_ok(_run(show, assay_root, token=absent))
    assert r.status is RailStatus.EMPTY


register_law(show, "show_absent_empty")


@pytest.mark.parametrize(
    "content, params, expect_truncated, expect_lines, check_preview",
    [
        (_TEN_LINES, {"max_lines": 3}, True, 10, lambda pv: pv.count("\n") < 9),  # cap truncates below all 10
        ("line1\nline2\nline3", {"max_lines": 120}, False, 3, lambda _pv: True),  # within cap → no truncation
        (
            _TEN_LINES,
            {"lines": "3:5", "max_lines": 1},
            True,
            10,
            lambda pv: pv == "line3\nline4\nline5",
        ),  # explicit window beats cap (3 of 10 → truncated)
        (
            "alpha 1\nbeta 2\nalpha 3\ngamma 4\nalpha 5",
            {"grep": "alpha", "max_lines": 120},
            False,
            3,
            lambda pv: "beta" not in pv and "gamma" not in pv,
        ),
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
    """Show windows a stored artifact via --max-lines / --lines / --grep / --full, with the full selected count on lines."""
    detail = _show_detail(assay_root, content, **params)
    assert detail.truncated is expect_truncated
    assert detail.lines == expect_lines
    assert check_preview(detail.preview)


register_law(show, "show_max_lines_cap")
register_law(show, "show_no_truncation_within_cap")
register_law(show, "show_lines_window")
register_law(show, "show_grep_filter")
register_law(show, "show_full_flag")


def test_show_artifact_lines_match_content(assay_root: AssayHarness) -> None:
    """Show records the exact line count of the stored artifact in the Artifact.lines field."""
    store = assay_root.settings.store()
    path = store.write_text("a\nb\nc\nd", "scope", "api", "data", "file.txt")  # 4 lines
    token = path.rsplit("/", 1)[-1]
    r = assert_ok(_run(show, assay_root, token=token, max_lines=120))
    assert r.artifacts
    assert r.artifacts[0].lines == 4


register_law(show, "show_artifact_lines_match_content")


def test_show_latest_prefers_api_scope_artifact(assay_root: AssayHarness) -> None:
    """Show latest selects the API-scoped artifact even when a newer non-API artifact exists."""
    store = assay_root.settings.store()
    api_path = store.write_text("api artifact\n", "scope", "api", "pkg", "surface.txt")
    store.write_text("newer generic artifact\n", "scope", "zzz", "surface.txt")

    latest_token = "latest"  # noqa: S105  # not a password — artifact lookup keyword per api.py _LATEST_ARTIFACT
    r = assert_ok(_run(show, assay_root, token=latest_token, max_lines=50))
    assert r.status is RailStatus.OK
    assert r.artifacts
    assert r.artifacts[0].path == api_path


register_law(show, "show_latest_api_scope_preference")


def test_show_latest_prefers_api_claim_root_before_api_scope_cache(assay_root: AssayHarness) -> None:
    """Show latest selects retained API-claim evidence before reusable API scope cache artifacts."""
    store = assay_root.settings.store()
    api_path = store.write_text("api claim artifact\n", Claim.API.value, "run-a", "doctor-inventory.json")
    store.write_text("api scope cache artifact\n", "scope", "api", "pkg", "surface.txt")
    store.write_text("newer generic artifact\n", "scope", "zzz", "surface.txt")

    latest_token = "latest"  # noqa: S105  # not a password — artifact lookup keyword per api.py _LATEST_ARTIFACT
    r = assert_ok(_run(show, assay_root, token=latest_token, max_lines=50))
    assert r.status is RailStatus.OK
    assert r.artifacts
    assert r.artifacts[0].path == api_path


register_law(show, "show_latest_api_claim_root_preference")


# --- [QUERY]


@pytest.mark.parametrize(
    "key, unsupported",
    [
        ("pytest", False),  # live pydist key → Ok surface/miss, never FAULTED
        ("totally-unknown-xxxxxxx456", True),  # unknown key → UNSUPPORTED + ApiResolution
    ],
    ids=["pydist-ok", "unknown-unsupported"],
)
def test_query_key_resolution(
    assay_root: AssayHarness,
    key: str,
    unsupported: bool,  # noqa: FBT001  # parametrized bool flag
) -> None:
    """Query resolves a live key to an Ok report (never FAULTED) and an unknown key to UNSUPPORTED+ApiResolution."""
    r = assert_ok(_run(query, assay_root, key=key, symbol=""))
    assert r.verb == "query"
    assert r.status is not RailStatus.FAULTED  # both arms ride the Ok rail
    assert not unsupported or (r.status is RailStatus.UNSUPPORTED and isinstance(r.detail, ApiResolution))


register_law(query, "query_pydist_ok")
register_law(query, "query_unknown_key_unsupported")

# --- [WIRE_TYPES]


# Three wire types share identical roundtrip semantics; collapsed into one parametrized law.
# register_law entries below keep law-coverage gate total for all three subjects.
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


register_law(ApiResolution, "api_resolution_roundtrip")
register_law(ApiSource, "api_source_roundtrip")
register_law(ApiSurface, "api_surface_roundtrip")

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
    """C# query parses canned ilspycmd output into the correct ApiSurface shape and anchor text across all arms.

    Drives the full C# surface and decompile pipeline through a monkeypatched run_check. Covers the
    four SymbolShape arms plus the SEARCH branch reached via substring match and empty decompile fallback.
    """
    # The substring-search arm needs two same-prefix types so 'wid' substring-matches both; other arms use the default roster.
    types = {"wid": b"Class Acme.Widget\nClass Acme.WidgetFactory\nStruct Acme.Point\n"}.get(symbol, _ILSPY_TYPES)
    _install_ilspy(assay_root, monkeypatch, types=types, decompile=decompile)
    detail = _cs_surface(assay_root, monkeypatch, symbol, install=False)
    assert detail.shape is shape, f"symbol {symbol!r}: shape {detail.shape} != {shape}"
    assert anchor in f"{detail.preview}\n{detail.signature}" or anchor in detail.preview.splitlines(), f"symbol {symbol!r}: {anchor!r} missing"


register_law(query, "cs_query_dispatches_every_shape")
register_law(query, "cs_query_search_hits")
register_law(query, "cs_query_empty_decompile_search")


def test_cs_query_type_window_renders_full_golden_surface(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """The rendered C# type-decompile window reproduces the full anchored surface byte-for-byte.

    Where the dispatch sweep only spot-checks ``anchor in preview``, this pins the entire rendered
    window (class declaration + every member line, anchored and dedented) against an external golden.
    Falsified by any decompile-window regression — line reordering, dropped members, anchor drift —
    that a single-substring assertion would let survive. The golden lives in the external store so the
    multi-line surface never bloats the test source.
    """
    detail = _cs_surface(assay_root, monkeypatch, "Widget")
    assert outsource(detail.preview, suffix=".txt") == external("uuid:be213712-2b2b-44e5-973c-ec6ff40ee8b9.txt")


register_law(query, "cs_query_type_window_golden")


def test_cs_query_member_carries_xml_doc(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """C# member decompile pulls the sidecar XMLDoc summary into the ApiSurface.doc field."""
    _install_ilspy(assay_root, monkeypatch, xmls=True)
    detail = _cs_surface(assay_root, monkeypatch, "Acme.Widget.Spin", install=False)
    assert detail.doc == "Spins the widget."
    assert detail.member == "Spin"


register_law(query, "cs_query_member_xml_doc")


def test_cs_query_grep_filters_decompile_window(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """C# decompile with grep keeps only matching lines and recomputes the selected count."""
    detail = _cs_surface(assay_root, monkeypatch, "Widget", grep="Spin")
    assert "Spin" in detail.preview
    assert "Count" not in detail.preview  # grep dropped the non-matching member lines
    assert detail.lines == 1  # exactly one line survived the grep filter


register_law(query, "cs_query_grep_filter")


@pytest.mark.parametrize(
    "max_lines, full, expect_truncated", [(2, False, True), (1, True, False)], ids=["small-cap-truncates", "full-flag-never-truncates"]
)
def test_cs_query_truncation_matrix(
    assay_root: AssayHarness,
    monkeypatch: pytest.MonkeyPatch,
    max_lines: int,
    full: bool,  # noqa: FBT001  # parametrized bool flag
    expect_truncated: bool,  # noqa: FBT001  # parametrized bool flag
) -> None:
    """C# decompile truncation: a capped window truncates (anchor-relative); --full returns the whole file.

    lines always carries the full source count; the window is anchored at the member declaration so a
    capped (non-full) request never spans the whole file, while --full ignores the cap entirely.
    """
    detail = _cs_surface(assay_root, monkeypatch, "Widget", max_lines=max_lines, full=full)
    assert detail.truncated is expect_truncated
    assert detail.lines == _ILSPY_DECOMPILE_LINES  # full count regardless of window


register_law(query, "cs_query_truncation_matrix")


def test_cs_decompile_window_note_surfaces_truncation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A capped decompile window notes 'window: N of M lines (--full or --max-lines to widen)' on the report."""
    _install_ilspy(assay_root, monkeypatch)
    r = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Widget", max_lines=2))
    assert f"window: 2 of {_ILSPY_DECOMPILE_LINES} lines (--full or --max-lines to widen)" in r.notes


register_law(query, "cs_decompile_window_note")


def test_roster_forced_cap_emits_results_note(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A roster larger than _RESULT_CAP caps results and notes 'results: N of M (cap=K); full listing in artifact'."""
    _install_ilspy(assay_root, monkeypatch)
    monkeypatch.setattr(api_rail, "_RESULT_CAP", 1)
    r = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Acme"))  # namespace roster of 4 owned types
    assert len(r.results) == 1
    assert "results: 1 of 4 (cap=1); full listing in artifact" in r.notes


register_law(query, "roster_forced_cap_results_note")


def test_api_result_ids_are_identity_shaped(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Roster and decompile results carry identity ids (kind:identity), not run-ordinal ids."""
    _install_ilspy(assay_root, monkeypatch)
    decompiled = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Widget"))
    roster = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Acme"))
    assert decompiled.results[0].id == "type:Acme.Widget"
    assert roster.results
    assert all(m.id == f"scope:{m.text}" for m in roster.results)


register_law(query, "api_result_ids_identity_shaped")


@pytest.mark.parametrize("symbol", ["Nonexistent", "Other.Missing"], ids=["search-miss", "unranked-fqn-miss"])
def test_cs_query_resolution_miss(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, symbol: str) -> None:
    """C# query for an absent (search miss) or unranked-TYPE-shaped symbol folds to a partial ApiResolution (UNSUPPORTED)."""
    _install_ilspy(assay_root, monkeypatch)
    r = assert_ok(_run(query, assay_root, key="rhino-common", symbol=symbol))
    detail = r.detail
    assert isinstance(detail, ApiResolution)
    assert detail.reason == "partial"
    assert r.status is RailStatus.UNSUPPORTED


register_law(query, "cs_query_search_miss_partial")
register_law(query, "cs_query_unranked_fqn_miss")


def test_cs_surface_all_attempts_fail_faults_rail(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """C# surface with every ilspycmd attempt non-zero promotes the rail to a FAULTED error."""
    _install_ilspy(assay_root, monkeypatch, returncode=1)
    e = assert_error(_run(query, assay_root, key="rhino-common", symbol=""))
    assert e.status is RailStatus.FAULTED
    assert "ilspy" in e.message.casefold()


register_law(query, "cs_surface_all_fail_faults")


def test_cs_surface_cache_hit_skips_reinvocation(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A second C# surface query reads the fingerprint cache instead of re-invoking ilspycmd.

    Regression guard: the first query writes the roster cache; a follow-up that would FAULT on a fresh
    invocation must still succeed off the cache, proving the cache-hit branch (api.py:884-886) is taken.
    """
    first = _cs_surface(assay_root, monkeypatch, "")
    # Re-pin run_check to a hard fault: only a cache hit can keep the second surface query green.
    monkeypatch.setattr(api_rail, "run_check", lambda *_a, **_kw: RailProbe.error(("api",), "must-not-run"))
    second = _cs_surface(assay_root, monkeypatch, "", install=False)
    assert second.preview == first.preview


register_law(query, "cs_surface_cache_hit")


def test_cs_surface_empty_assemblies_is_empty_not_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A C# source with no assemblies yields an EMPTY surface (search miss), never a FAULT."""
    empty_src = api_rail._Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=())
    monkeypatch.setattr(api_rail, "_source", lambda _settings, _key: Ok(empty_src))
    r = assert_ok(_run(query, assay_root, key="rhino-common", symbol="Anything"))
    assert r.status is not RailStatus.FAULTED  # no types → search miss → UNSUPPORTED ApiResolution, never a fault rail


register_law(query, "cs_surface_empty_assemblies_empty")

# --- [PYDIST_QUERY]


def test_pydist_member_query_captures_real_signature(assay_root: AssayHarness) -> None:
    """A pydist member query introspects the live symbol: signature + doc + source ride ApiSurface."""
    r = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture"))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.source_kind is SourceKind.PYDIST
    assert "fixture" in detail.signature  # real inspect.signature of pytest.fixture
    assert detail.shape is SymbolShape.TYPE


register_law(query, "pydist_member_real_signature")


def test_pydist_surface_query_rosters_real_types(assay_root: AssayHarness) -> None:
    """A pydist surface query rosters the distribution's public classes/functions via INPROC inspect."""
    r = assert_ok(_run(query, assay_root, key="msgspec", symbol=""))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.source_kind is SourceKind.PYDIST
    assert detail.lines == IsInt(ge=1)  # msgspec exposes Struct/Meta/Raw etc. as type captures


register_law(query, "pydist_surface_real_roster")


def test_pydist_member_grep_and_full_window(assay_root: AssayHarness) -> None:
    """Pydist decompile honors grep filtering and the --full window over the captured source."""
    full_r = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture", full=True))
    grep_r = assert_ok(_run(query, assay_root, key="pytest", symbol="fixture", grep="def"))
    full_detail, grep_detail = full_r.detail, grep_r.detail
    assert isinstance(full_detail, ApiSurface)
    assert isinstance(grep_detail, ApiSurface)
    assert not full_detail.truncated  # full flag never truncates
    assert grep_detail.lines <= full_detail.lines  # grep is a monotone filter over the same source


register_law(query, "pydist_member_grep_full")

# --- [PYDIST_THUNK]


def test_member_captures_emits_signature_doc_full_triple() -> None:
    """_member_captures projects exactly the signature/doc/full capture triple for a resolved object."""

    class Marker:
        """A marker class."""

        value: str

    signature, doc, full = api_rail._member_captures(Marker, "Marker")
    assert (signature.name, doc.name, full.name) == ("signature", "doc", "full")
    assert signature.text.startswith("Marker(")
    assert doc.text == "A marker class."


def test_object_source_swallows_builtin_typeerror() -> None:
    """_object_source returns empty for a C-builtin whose source is unreadable (TypeError arm)."""
    assert not api_rail._object_source(len)


def test_pydist_thunk_roster_and_member_branches() -> None:
    """_pydist_thunk: empty symbol rosters @type captures; a member symbol anchors the capture triple."""
    roster = CAPTURES.decode(api_rail._pydist_thunk("msgspec", "")(_INPROC_CHECK).stdout)
    member = CAPTURES.decode(api_rail._pydist_thunk("msgspec", "Struct")(_INPROC_CHECK).stdout)
    assert any(cap.name == "type" and cap.text for cap in roster)
    assert [cap.name for cap in member] == ["signature", "doc", "full"]
    assert member[0].text.startswith("Struct")


def test_pydist_modules_recovers_import_roots() -> None:
    """_pydist_modules maps a distribution key to its real import roots (top_level or inverted map)."""
    assert "msgspec" in api_rail._pydist_modules("msgspec")


def test_resolve_symbol_walks_longest_importable_prefix() -> None:
    """_resolve_symbol resolves a dotted member against the longest importable distribution prefix."""
    resolved = api_rail._resolve_symbol("msgspec", "Struct")
    assert resolved is not None
    assert getattr(resolved, "__name__", "") == "Struct"


def _signature_fallback_probes() -> tuple[tuple[str, object, object], ...]:
    import types  # noqa: PLC0415  # local: a module object is the cleanest unsignable-but-annotated probe

    class ForwardType:
        pass

    def forward_handler(value: ForwardType) -> ForwardType:
        return value

    unsignable = types.ModuleType("fake_with_annotations")
    vars(unsignable)["__annotations__"] = {"cfg": "int", "name": "str"}

    class Holder:
        a: int

    return (
        ("typed-callable", forward_handler, IsStr(regex=r".*ForwardType.*")),  # annotationlib renders real annotations as strings
        ("synthesized-params", unsignable, "(cfg: int, name: str)"),  # annotationlib synthesis, not inspect.signature
        ("class-parens", Holder, IsStr(regex=r"\(.*\)")),  # non-callable class → parenthesized synthetic signature
        ("sentinel-no-annotations", Holder(), "(...)"),  # instance: class-level annotations are not on the instance
    )


@pytest.mark.parametrize(
    "obj, expected", [(o, e) for _id, o, e in _signature_fallback_probes()], ids=[i for i, _o, _e in _signature_fallback_probes()]
)
def test_signature_fallback_cases(obj: object, expected: object) -> None:
    """_signature renders annotations, synthesizes annotationlib params, or yields the '(...)' sentinel."""
    assert api_rail._signature(obj) == expected


def test_annotations_empty_for_unannotated_object() -> None:
    """_annotations returns an empty mapping for an object that exposes no annotation namespace."""
    assert api_rail._annotations(42) == {}


def test_malformed_xml_or_json_inputs_degrade_to_empty(assay_root: AssayHarness) -> None:
    """Every codec boundary (props/csproj/package.json/sidecar-xml) folds malformed input to an empty result, never raising."""
    props = assay_root.write("Directory.Packages.props", "<Project><PackageVersion Include='A' Version='1'/>")  # unterminated XML
    csproj = assay_root.write("src/App/App.csproj", "<bad")  # unparseable XML
    manifest = assay_root.write("node_modules/pkg/package.json", "{not json")
    bad_xml = assay_root.write("RhinoCommon.xml", "<doc><member")
    _ = props  # _packages reads via settings, not the path directly
    assert api_rail._packages(assay_root.settings) == {}
    assert api_rail._project_references(csproj) == ()
    assert not api_rail._json_field(manifest, "version")
    assert api_rail._xml_members(bad_xml) == ()


def test_pydist_source_handles_files_none() -> None:
    """_pydist_source yields a source even when the distribution exposes no file manifest (files=None arm)."""
    source = api_rail._pydist_source("pytest")
    assert source is not None
    assert source.kind is SourceKind.PYDIST


def test_pydist_source_files_none_yields_empty_assets(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_pydist_source over a distribution whose .files is None resolves to a source with empty assets."""
    import importlib.metadata as importlib_metadata  # noqa: PLC0415  # local: drive the stdlib metadata boundary

    monkeypatch.setattr(importlib_metadata, "distribution", lambda _key: _DistDouble("FilesNone", root=str(assay_root.root)))
    source = api_rail._pydist_source("filesnone")
    assert source is not None
    assert source.kind is SourceKind.PYDIST
    assert source.asset_paths == ()  # files=None → no per-file assets


def test_pydist_inventory_skips_nameless_and_survives_locate_oserror(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_pydist_inventory_sources skips a nameless dist and folds a locate_file OSError to an empty root."""
    import importlib.metadata as importlib_metadata  # noqa: PLC0415  # local: drive the stdlib metadata boundary

    class _OsErrorDist(_DistDouble):
        @override
        def locate_file(self, _rel: str) -> str:
            raise OSError("no root")

    root = str(assay_root.root)
    dists = (_DistDouble("", root=root), _DistDouble("Good", root=root), _OsErrorDist("Broken", root=root))
    monkeypatch.setattr(importlib_metadata, "distributions", lambda: iter(dists))
    rows = api_rail._pydist_inventory_sources()
    assert {row.source_id for row in rows} == {"Good", "Broken"}  # the nameless dist was skipped (the continue arm)
    assert all(row.source_kind is SourceKind.PYDIST for row in rows)  # OSError dist still produced a row


def test_pydist_modules_unknown_key_empty() -> None:
    """_pydist_modules returns empty for a key with no installed distribution (PackageNotFoundError arm)."""
    assert api_rail._pydist_modules("totally-not-installed-xyz") == ()


# --- [TSDECL]


def _ts_package(assay_root: AssayHarness, body: str, *, name: str = "mypkg", version: str = "2.1.0") -> None:
    assay_root.write(f"node_modules/{name}/package.json", f'{{"name":"{name}","version":"{version}","types":"index.d.ts"}}')
    assay_root.write(f"node_modules/{name}/index.d.ts", body)


def test_tsdecl_surface_rosters_exported_declarations(assay_root: AssayHarness) -> None:
    """A TS surface query rosters every exported .d.ts declaration name via the tree-sitter thunk."""
    _ts_package(assay_root, "export interface Foo { x: string }\nexport declare class Bar { y: number }\nexport type Baz = string;\n")
    r = assert_ok(_run(query, assay_root, key="mypkg", symbol=""))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert detail.source.source_kind is SourceKind.TSDECL
    assert {"Foo", "Bar", "Baz"} <= set(detail.preview.splitlines())


register_law(query, "tsdecl_surface_roster")


def test_tsdecl_member_query_anchors_signature(assay_root: AssayHarness) -> None:
    """A TS member query anchors the declaration's first-line signature from the .d.ts source."""
    _ts_package(assay_root, "export interface Foo { x: string }\nexport interface Bar { y: number }\n")
    r = assert_ok(_run(query, assay_root, key="mypkg", symbol="Foo"))
    detail = r.detail
    assert isinstance(detail, ApiSurface)
    assert "interface Foo" in detail.signature
    assert "number" not in detail.signature  # Bar's member must not leak into Foo's window


register_law(query, "tsdecl_member_signature")


def test_tsdecl_thunk_captures_export_aliases_and_parse_errors(assay_root: AssayHarness) -> None:
    """_tsdecl_thunk surfaces re-export aliases as @type captures and flags malformed .d.ts as parse errors."""
    good = assay_root.write("pkg/index.d.ts", 'export { Foo as Bar, Baz } from "./x";\nexport interface Thing { value: string }\n')
    bad = assay_root.write("pkg/bad.d.ts", "export interface Broken {")
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=good.parent, asset_paths=(good, bad))

    roster = CAPTURES.decode(api_rail._tsdecl_thunk(source, "")(_TS_CHECK).stdout)
    member = CAPTURES.decode(api_rail._tsdecl_thunk(source, "Bar")(_TS_CHECK).stdout)
    assert {"Bar", "Baz", "Thing"} <= {cap.text for cap in roster if cap.name == "type"}
    assert any(cap.parse_error for cap in roster)
    assert member[0].name == "signature"
    assert "Foo as Bar" in member[0].text


def test_tsdecl_member_lookup_prefers_owner_scope(assay_root: AssayHarness) -> None:
    """Owner-qualified TS member lookup resolves Foo.x within Foo before any same-named sibling member."""
    dts = assay_root.write("pkg/index.d.ts", "export interface Foo { x: string }\nexport interface Bar { x: number }\n")
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=dts.parent, asset_paths=(dts,))
    member = CAPTURES.decode(api_rail._tsdecl_thunk(source, "Foo.x")(_TS_CHECK).stdout)
    assert member[0].name == "signature"
    assert "string" in member[0].text
    assert "number" not in member[0].text


def test_tsdecl_names_scans_hoisted_and_scoped(assay_root: AssayHarness) -> None:
    """_tsdecl_names rosters both hoisted and @scope/pkg npm packages, sorted, dot-prefix excluded."""
    assay_root.write("node_modules/alpha/package.json", "{}")
    assay_root.write("node_modules/@scope/beta/package.json", "{}")
    assay_root.write("node_modules/.bin/placeholder", "")  # dot-prefixed → excluded
    assert api_rail._tsdecl_names(assay_root.settings) == ("@scope/beta", "alpha")


def test_tsdecl_thunk_query_error_surfaces_capture(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """A malformed roster query surfaces as a single query_error capture, mirroring the code rail."""
    from tree_sitter import QueryError  # noqa: PLC0415  # local: only this law needs the tree-sitter error type

    dts = assay_root.write("pkg/index.d.ts", "export interface Foo { x: string }\n")
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=dts.parent, asset_paths=(dts,))
    monkeypatch.setattr(api_rail, "ts_query", lambda *_a, **_kw: Error(QueryError("bad roster query")))

    roster = CAPTURES.decode(api_rail._tsdecl_thunk(source, "")(_TS_CHECK).stdout)
    assert any(cap.name == "query_error" and cap.parse_error for cap in roster)


def test_ts_declared_match_limit_saturation_marks_truncated(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_ts_declared mirrors the code rail: roster overflow past _RESULT_CAP caps the rows and marks every kept row truncated."""
    dts = assay_root.write("pkg/index.d.ts", "export interface Foo { x: string }\nexport interface Bar { y: number }\n")
    source = api_rail._Source("pkg", SourceKind.TSDECL, "1.0.0", package_root=dts.parent, asset_paths=(dts,))
    monkeypatch.setattr(api_rail, "_RESULT_CAP", 1)
    roster = CAPTURES.decode(api_rail._tsdecl_thunk(source, "")(_TS_CHECK).stdout)
    types = tuple(cap for cap in roster if cap.name == "type")
    assert len(types) == 1  # capped at the patched _RESULT_CAP
    assert all(cap.truncated for cap in types)  # saturation surfaces the same way code.py surfaces truncated


register_law(query, "ts_declared_match_limit_saturation")


def test_ts_captures_unreadable_path_returns_empty(assay_root: AssayHarness) -> None:
    """_ts_captures returns empty for an asset path that cannot be read (OSError arm)."""
    from tree_sitter import Parser as TSParser  # noqa: PLC0415  # local: construct the real parser for the read-fail probe

    parser = TSParser(ts_language(api_rail._TS_GRAMMAR))
    missing = assay_root.root / "node_modules" / "ghost" / "absent.d.ts"
    assert api_rail._ts_captures(parser, missing, "") == ()


# --- [NUGET]


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


def test_nuget_source_carries_inventory_facts(assay_root: AssayHarness) -> None:
    """_nuget_source resolves package root, ranked frameworks, owners, and primary-stem asset ordering."""
    package_root = _nuget_fixture(assay_root)
    source = api_rail._nuget_source(assay_root.settings, "Pkg.Core", "1.2.3")
    detail = api_rail._api_source(source)
    assert source.package_root == package_root
    assert source.frameworks == ("net8.0", "netstandard2.0")  # _FRAMEWORK_RANK ordering
    assert source.owners == ("src/App/App.csproj",)
    assert isinstance(detail, ApiSource)
    assert detail.restore == "restored"
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
    assert r.artifacts  # full path listing rides an artifact


register_law(resolve, "resolve_nuget_kind_path_rows")


@pytest.mark.parametrize(
    "key, expect_ok, expect_reason",
    [
        ("Pkg.Core", True, ""),  # exact → resolved
        ("Pkg.C", False, "ambiguous"),  # prefix-matches Core + Cache → ambiguous, never auto-picks one
        ("zzz-nope", False, "unknown"),  # no match → unknown with nearest candidates
    ],
    ids=["exact", "ambiguous", "unknown"],
)
def test_resolve_key_fuzzy_dispatch(key: str, expect_ok: bool, expect_reason: str) -> None:  # noqa: FBT001  # parametrized bool flag
    """_resolve_key discriminates exact / ambiguous / unknown over the NuGet package map."""
    packages = {"Pkg.Core": "1.0.0", "Pkg.Cache": "2.0.0", "Other": "3.0.0"}
    result = api_rail._resolve_key(packages, key)
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
    """Resolve over an ambiguous NuGet key folds the ambiguity to None in _source, surfacing unknown.

    Regression guard for the _source fold contract: an ambiguous NuGet resolution does not short-circuit
    the resolver chain — it degrades to None and the aggregate miss reports the union-of-roster reason.
    """
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


register_law(resolve, "resolve_nuget_ambiguous")

# --- [ENGINE_BOUNDARY]


def test_invoke_error_rail_yields_nonzero_completed(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_invoke maps a run_check Error fault into a non-zero Completed carrying the fault message on stderr."""
    monkeypatch.setattr(api_rail, "run_check", lambda *_a, **_kw: RailProbe.error(("api",), "spawn boom"))
    surface_tool = next(t for t in select(Claim.API, Language.CSHARP))
    done = api_rail._invoke(assay_root.settings, assay_root.scope(Claim.API), surface_tool, "--version")
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
    """A corrupted catalog (no api row) faults the surface rail with a precise diagnostic, across C# and INPROC."""
    asm = assay_root.write("RhinoCommon.dll" if kind is SourceKind.ASSEMBLY else "dummy.py", "MZ")
    source = api_rail._Source(
        key=key, kind=kind, assemblies=(asm,) if kind is SourceKind.ASSEMBLY else (), asset_paths=() if kind is SourceKind.ASSEMBLY else (asm,)
    )
    monkeypatch.setattr(api_rail, "_source", lambda _settings, _key: Ok(source))
    monkeypatch.setattr(api_rail, "select", lambda _claim, _lang: iter(()))

    e = assert_error(_run(query, assay_root, key=key, symbol=symbol))
    assert e.status is RailStatus.FAULTED
    assert message in e.message


register_law(query, "cs_surface_no_catalog_fault")
register_law(query, "inproc_no_catalog_fault")


def test_cs_decompile_faults_when_catalog_row_missing(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """When the catalog drops the ilspycmd row between surface listing and decompile, the decompile arm faults.

    A stateful select double returns the real row for the surface listing (call 1) and an empty catalog for the
    decompile lookup (call 2+), driving the _cs_decompile select-None Fault arm with the real verb pipeline.
    """
    asm = assay_root.write("RhinoCommon.dll", "MZ")
    source = api_rail._Source(key="rhino-common", kind=SourceKind.ASSEMBLY, assemblies=(asm,))
    calls = {"n": 0}

    def _staged(_claim: Claim, _lang: Language) -> object:
        calls["n"] += 1
        return iter(select(Claim.API, Language.CSHARP)) if calls["n"] == 1 else iter(())

    monkeypatch.setattr(api_rail, "_source", lambda _settings, _key: Ok(source))
    monkeypatch.setattr(api_rail, "run_check", lambda *_a, **_kw: RailProbe.receipt(("ilspycmd",), 0, stdout=_ILSPY_TYPES))
    monkeypatch.setattr(api_rail, "select", _staged)

    e = assert_error(_run(query, assay_root, key="rhino-common", symbol="Acme.Widget"))
    assert e.status is RailStatus.FAULTED
    assert "no ilspycmd catalog row" in e.message


register_law(query, "cs_decompile_no_catalog_fault")
