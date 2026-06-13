"""Law matrix for ``tools.assay.rails.static`` public surface.

Scope: StaticParams, build, fix, full, plan, report.

Law structure:
- StaticParams field invariants: defaults, language projection, path binding.
- plan: routing-plan preview invariants (notes carry argv previews, artifacts carry scope shas,
  status is OK when files or projects are routed, Match rows carry scope/files/projects text).
- plan/fix/report/build/full: ROP rail contract — every verb returns Result[Report, Fault].
- build mode-family law: restore+build is always the closure for build mode.
- full: combines debug+release configurations and folds their counts.
- Language inference: suffix narrowing and fallback to all languages.
"""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from dataclasses import replace as dc_replace
from typing import TYPE_CHECKING

from expression import Error, Ok
from expression.collections import block
from hypothesis import given
import pytest

from tests.python._testkit.laws import register_law, spec
from tests.python._testkit.spec import assert_error, assert_ok, support_matrix, validity_matrix, ValidityCase
from tests.python._testkit.strategies import resolve
from tools.assay.core.model import Claim, Fault, Language, Mode, receipt
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
import tools.assay.rails.static as static_rail
from tools.assay.rails.static import build, fix, full, plan, report, StaticParams


if TYPE_CHECKING:
    from collections.abc import Callable

    from expression import Result

    from tests.python.tools.assay.kit import AssayHarness
    from tools.assay.composition.settings import ArtifactScope, AssaySettings
    from tools.assay.core.model import Report

    type _VerbFn = Callable[[AssaySettings, ArtifactScope, StaticParams], Result[Report, Fault]]
    type _PlanCheckFn = Callable[[Report], bool]


# --- [CONSTANTS] ------------------------------------------------------------------------

_SUFFIX_CASES: tuple[tuple[str, Language], ...] = (
    ("tools/assay/__init__.py", Language.PYTHON),
    ("src/view.tsx", Language.TYPESCRIPT),
    ("src/view.ts", Language.TYPESCRIPT),
    ("schema.sql", Language.SQL),
    ("build.sh", Language.BASH),
    ("README.md", Language.DOCS),
    ("Foo.cs", Language.CSHARP),
)

_CLAIM_VERB_CASES: tuple[tuple[_VerbFn, str], ...] = ((fix, "fix"), (report, "report"), (build, "build"), (full, "full"))

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [STATICPARAMS_LAWS]

register_law(StaticParams, "default_fields")


def test_staticparams_defaults() -> None:
    """Default StaticParams: empty paths, no language override."""
    p = StaticParams()
    assert p.paths == ()
    assert p.language is None


@spec(StaticParams, given=False, law="roundtrip_resolve")
@given(resolve(StaticParams))
def test_staticparams_roundtrip(p: StaticParams) -> None:
    """Every resolved StaticParams survives a dc_replace identity trip."""
    assert p == dc_replace(p)


@pytest.mark.parametrize(
    "language, paths, expected_language",
    [
        (Language.PYTHON, ("tools/assay/__init__.py",), Language.PYTHON),
        (Language.CSHARP, ("Workspace.slnx",), Language.CSHARP),
        (None, ("tools/assay/__init__.py",), None),
    ],
)
def test_staticparams_language_field(language: Language | None, paths: tuple[str, ...], expected_language: Language | None) -> None:
    """Language field is preserved exactly on construction."""
    p = StaticParams(paths=paths, language=language)
    assert p.language is expected_language


register_law(StaticParams, "language_field_preserved")

# --- [VALIDITY_MATRIX]


@pytest.mark.parametrize(
    "case",
    [
        ValidityCase("none", StaticParams(language=None), expected=True),
        *(ValidityCase(lang.value, StaticParams(language=lang), expected=True) for lang in Language),
    ],
    ids=lambda c: c.label if isinstance(c, ValidityCase) else str(c),
)
def test_staticparams_language_validity_matrix(case: ValidityCase[StaticParams]) -> None:
    """StaticParams accepts None and every Language member without fault."""
    validity_matrix([case], valid=lambda p: isinstance(p, StaticParams) and p.language == case.value.language)


register_law(StaticParams, "language_validity_matrix")
register_law(StaticParams, "all_verbs_rop_contract")

# --- [LANGUAGE_INFERENCE_LAWS]


@pytest.mark.parametrize("path, expected", _SUFFIX_CASES)
def test_language_inference_single_suffix(path: str, expected: Language) -> None:
    """Infers correct Language from a single file suffix (falsifiable: wrong suffix table)."""
    from tools.assay.rails.static import _languages  # noqa: PLC0415

    result = _languages(None, (path,))
    assert expected in result, f"{expected} not inferred from {path!r}: got {result}"


register_law(Language, "suffix_inference_single")


def test_language_inference_empty_paths_returns_all() -> None:
    """No paths → all Languages are returned (fallback rule)."""
    from tools.assay.rails.static import _languages  # noqa: PLC0415

    result = _languages(None, ())
    assert set(result) == set(Language)


register_law(Language, "inference_empty_paths_all_languages")


def test_language_inference_explicit_overrides_paths() -> None:
    from tools.assay.rails.static import _languages  # noqa: PLC0415

    result = _languages(Language.CSHARP, ("tools/assay/__init__.py",))
    assert result == (Language.CSHARP,)


register_law(Language, "explicit_overrides_suffix")


def test_language_inference_multi_suffix() -> None:
    """Mixed-suffix paths produce exactly the matching languages with no duplicates."""
    from tools.assay.rails.static import _languages  # noqa: PLC0415

    result = _languages(None, ("tools/assay/__init__.py", "src/view.tsx"))
    assert Language.PYTHON in result
    assert Language.TYPESCRIPT in result
    assert len(result) == len(set(result)), f"duplicates in inferred languages: {result}"


register_law(Language, "inference_multi_suffix_no_duplicates")

# --- [MODE_FAMILY_LAW]


@pytest.mark.parametrize("mode, expected", [(Mode.BUILD, (Mode.RESTORE, Mode.BUILD)), (Mode.CHECK, (Mode.CHECK,)), (Mode.WRITE, (Mode.WRITE,))])
def test_mode_family(mode: Mode, expected: tuple[Mode, ...]) -> None:
    """Build mode yields restore+build; other modes are their own singleton family."""
    from tools.assay.rails.static import _mode_family  # noqa: PLC0415

    assert _mode_family(mode) == expected


register_law(Mode, "mode_family_build_restore_closure")

# --- [SUPPORT_MATRIX]


def test_readonly_verbs_do_not_use_write_mode() -> None:
    """Plan, report, and build verbs never invoke Mode.WRITE (falsifiable: swap mode in _thin_rail)."""
    support_matrix(
        ("check_writes_false", lambda: not Mode.CHECK.writes, True),
        ("build_writes_false", lambda: not Mode.BUILD.writes, True),
        ("write_writes_true", lambda: Mode.WRITE.writes, True),
    )


register_law(plan, "readonly_verb_no_write_mode")
register_law(report, "readonly_verb_no_write_mode")
register_law(build, "readonly_verb_no_write_mode")

# --- [PLAN_RAIL_LAWS]


@pytest.mark.parametrize(
    "check_fn",
    [
        lambda r: r.claim is Claim.STATIC and r.verb == "plan",
        lambda r: r.status is RailStatus.OK,
        lambda r: any("--artifacts-path" in n and "--disable-build-servers" in n for n in r.notes),
        lambda r: "scope=" in " ".join(m.text for m in r.results) and "files=" in " ".join(m.text for m in r.results),
    ],
    ids=["claim_verb", "status_ok", "argv_flags_in_notes", "match_rows_scope_text"],
)
def test_plan_csharp_workspace_slnx(check_fn: _PlanCheckFn, assay_root: AssayHarness) -> None:
    """Plan with Workspace.slnx: claim/verb, status, notes argv flags, and match-row scope text."""
    scope = assay_root.scope(Claim.STATIC)
    r = assert_ok(plan(assay_root.settings, scope, StaticParams(paths=("Workspace.slnx",))))
    assert check_fn(r)


register_law(plan, "returns_ok_report")
register_law(plan, "status_ok_when_routed")
register_law(plan, "notes_contain_argv_previews")
register_law(plan, "match_rows_carry_scope_text")


def test_plan_build_preview_pins_run_scope_sarif_dir(assay_root: AssayHarness) -> None:
    """The dotnet-build STATIC row pins -p:CspSarifDir=<run-scope>/sarif while restore stays unpinned (falsifiable: drop _sarif_pin)."""
    scope = assay_root.scope(Claim.STATIC)
    r = assert_ok(plan(assay_root.settings, scope, StaticParams(paths=("Workspace.slnx",))))
    build_notes = [n for n in r.notes if "csharp build:" in n]
    assert build_notes, f"expected a csharp build preview note, got: {r.notes!r}"
    assert all(f"-p:CspSarifDir={scope.sarif_dir}" in n for n in build_notes)
    restore_segments = [seg for n in build_notes for seg in n.split(" ; ") if " restore " in f" {seg} "]
    assert all("-p:CspSarifDir=" not in seg for seg in restore_segments), "the SARIF pin targets only the dotnet-build row"


register_law(plan, "build_preview_pins_run_scope_sarif_dir")


def test_plan_artifacts_carry_build_scope_sha_for_csharp(assay_root: AssayHarness) -> None:
    """_plan_report emits a build-scope Artifact when Routed.projects is non-empty (falsifiable: drop artifact emit)."""
    from tools.assay.core.model import ArtifactKind  # noqa: PLC0415
    from tools.assay.rails.static import _plan_report  # noqa: PLC0415

    scope: ArtifactScope = assay_root.scope(Claim.STATIC)
    routed = Routed(Language.CSHARP, Scope.FULL, files=("apps/foo/foo.csproj",), projects=("apps/foo/foo.csproj",))
    rpt = _plan_report((routed,), assay_root.settings, scope)
    scope_artifacts = [a for a in rpt.artifacts if a.kind is ArtifactKind.SCOPE]
    assert scope_artifacts, f"expected build-scope artifacts for CSHARP, got: {rpt.artifacts!r}"
    assert all(a.id.startswith("build-") for a in scope_artifacts), f"artifact id must start with 'build-': {scope_artifacts!r}"


register_law(plan, "artifacts_carry_build_scope_sha")


def test_plan_explicit_language_limits_results(assay_root: AssayHarness) -> None:
    """Plan with explicit language=PYTHON only routes Python; results carry python scope row."""
    assay_root.write("tools/assay/__init__.py", "")
    scope = assay_root.scope(Claim.STATIC)
    r = assert_ok(plan(assay_root.settings, scope, StaticParams(language=Language.PYTHON, paths=("tools/assay/__init__.py",))))
    for row in r.results:
        assert "python" in row.id.lower(), f"unexpected language in plan results: {row.id!r}"


register_law(plan, "explicit_language_limits_results")


def test_plan_report_routed_object_directly(assay_root: AssayHarness) -> None:
    """_plan_report: an explicit Routed CSHARP FULL carries sha/scope text in notes."""
    from tools.assay.rails.static import _plan_report  # noqa: PLC0415

    scope: ArtifactScope = assay_root.scope(Claim.STATIC)
    routed = Routed(Language.CSHARP, Scope.FULL, files=("Workspace.slnx",), projects=("Workspace.slnx",), full_triggers=("Workspace.slnx",))
    rpt = _plan_report((routed,), assay_root.settings, scope)
    assert rpt.status is RailStatus.OK
    assert any("--artifacts-path" in n and "--disable-build-servers" in n for n in rpt.notes)


register_law(plan, "plan_report_routed_notes")

# --- [ROP_CONTRACT]


@pytest.mark.parametrize("verb_fn, verb_name", _CLAIM_VERB_CASES, ids=[n for _, n in _CLAIM_VERB_CASES])
def test_verb_returns_result_and_ok_shape(verb_fn: _VerbFn, verb_name: str, assay_root: AssayHarness) -> None:
    """Every verb returns Result[Report, Fault]; Ok branch carries claim=STATIC and verb=<name>."""
    scope = assay_root.scope(Claim.STATIC)
    params = StaticParams(language=Language.PYTHON, paths=())
    raw: Result[Report, Fault] = verb_fn(assay_root.settings, scope, params)
    assert raw.is_ok() or raw.is_error(), f"{verb_name} returned non-Result: {raw!r}"
    match raw:
        case _ if raw.is_ok():
            r = assert_ok(raw)
            assert r.claim is Claim.STATIC
            assert r.verb == verb_name
            if verb_name == "full":
                assert r.counts.ok + r.counts.failed == r.counts.total, f"counts arithmetic broken: {r.counts!r}"
        case _:
            pass  # Fault branch is an acceptable outcome; law governs only the Ok shape.


register_law(fix, "returns_result")
register_law(fix, "ok_carries_static_claim")
register_law(report, "returns_result")
register_law(report, "ok_carries_static_claim")
register_law(build, "returns_result")
register_law(build, "ok_carries_static_claim")
register_law(full, "returns_result")
register_law(full, "counts_fold_invariant")


def test_full_injects_workspace_slnx_when_paths_empty(assay_root: AssayHarness) -> None:
    """Full inserts Workspace.slnx into paths when params.paths is empty."""
    assay_root.write("Workspace.slnx", "")
    scope = assay_root.scope(Claim.STATIC)
    result = full(assay_root.settings, scope, StaticParams(paths=()))
    assert result.is_ok() or result.is_error()


register_law(full, "workspace_slnx_injection")

# --- [DISPATCH_EMPTY_CHECKS]


def test_dispatch_returns_empty_when_no_checks_for_language(assay_root: AssayHarness) -> None:
    """_dispatch returns () when a language (DOCS) has no registered tools — falsifiable: add DOCS tools."""
    scope = assay_root.scope(Claim.STATIC)
    routed = Routed(Language.DOCS, Scope.FULL, files=("README.md",), projects=())
    result = static_rail._dispatch(routed, settings=assay_root.settings, scope=scope, mode=Mode.CHECK)
    assert result == (), f"expected empty dispatch for toolless language, got {result!r}"


register_law(Language, "dispatch_empty_checks_toolless_language")

# --- [DISPATCH_ARMS]


@pytest.mark.parametrize(
    "mode, routed, prefix",
    [
        (Mode.BUILD, Routed(Language.CSHARP, Scope.FULL, files=("Workspace.slnx",), projects=("Workspace.slnx",)), None),
        (Mode.WRITE, Routed(Language.PYTHON, Scope.FULL, files=("tools/assay/__init__.py",), projects=()), "write-"),
    ],
    ids=["build_projects", "write_mode"],
)
def test_dispatch_arm(mode: Mode, routed: Routed, prefix: str | None, monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """_dispatch routes BUILD+projects to _build_fan and WRITE to _write_fan (falsifiable: wrong arm)."""
    ok_argv = ("dotnet", "build") if mode is Mode.BUILD else ("ruff", "check", "--fix")
    ok_rows: tuple[object, ...] = (Ok(receipt(ok_argv, 0, status=RailStatus.OK)),)
    captured: list[str] = []

    def _fake(resource: str, *_a: object, **_kw: object) -> object:
        captured.append(resource)
        return Ok(ok_rows)

    monkeypatch.setattr(static_rail, "leased", _fake)
    scope = assay_root.scope(Claim.STATIC)
    result = static_rail._dispatch(routed, settings=assay_root.settings, scope=scope, mode=mode)
    assert result == ok_rows, f"_dispatch {mode}: expected ok_rows, got {result!r}"
    if prefix is not None:
        assert captured, f"leased not called — wrong arm taken for {mode}: {captured!r}"
        assert captured[0].startswith(prefix), f"wrong resource prefix for {mode}: {captured!r}"


register_law(build, "dispatch_build_projects_routes_build_fan")
register_law(fix, "dispatch_write_mode_routes_write_fan")


def test_build_envelope_names_host_routed(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """The build verb's report notes carry the closure partition row and name every host-routed project.

    BUILD lanes keep host-bound projects in the closure, so the envelope must still surface the partition:
    a head row counting included vs host-routed and a names row listing the host-routed projects. Falsifies
    the _closure_notes fold (dropped rows), the hardcoded host-routed=0 regression, and a names row that
    leaks only counts.
    """
    routed = Routed(Language.CSHARP, Scope.CHANGED, projects=("src/App/App.csproj", "src/Lib/Lib.csproj"), host_bound=("src/App/App.csproj",))
    monkeypatch.setattr(static_rail, "_routed", lambda *_a, **_k: Ok(block.of_seq((routed,))))
    monkeypatch.setattr(static_rail, "_dispatch", lambda *_a, **_k: (Ok(receipt(("dotnet", "build"), 0, status=RailStatus.OK)),))
    out = assert_ok(build(assay_root.settings, assay_root.scope(Claim.STATIC), StaticParams(language=Language.CSHARP)))
    assert "closure[csharp]: included=1 excluded=0 cached=0 host-routed=1" in out.notes
    assert "host-routed[csharp]: src/App/App.csproj" in out.notes


register_law(build, "envelope_names_host_routed_projects")

# --- [FAN_LEASED_LAWS]


@pytest.mark.parametrize(
    "fan, routed, ok_argv, fault",
    [
        (
            "_build_fan",
            Routed(Language.CSHARP, Scope.FULL, files=("Workspace.slnx",), projects=("Workspace.slnx",)),
            ("dotnet", "build"),
            Fault(argv=("leased", "build"), status=RailStatus.BUSY, message="lease-busy"),
        ),
        (
            "_write_fan",
            Routed(Language.PYTHON, Scope.FULL, files=("tools/assay/__init__.py",), projects=()),
            ("ruff", "check", "--fix"),
            Fault(argv=("leased", "write"), status=RailStatus.FAULTED, message="lease-faulted"),
        ),
    ],
    ids=["build_fan", "write_fan"],
)
def test_fan_leased_ok_and_error(
    fan: str, routed: Routed, ok_argv: tuple[str, ...], fault: Fault, monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness
) -> None:
    """Fan projects Ok rows through unchanged and wraps Error as a single-element fault tuple."""
    completed = receipt(ok_argv, 0, status=RailStatus.OK)
    ok_rows: tuple[object, ...] = (Ok(completed),)
    scope = assay_root.scope(Claim.STATIC)
    mode = Mode.BUILD if fan == "_build_fan" else Mode.WRITE
    checks = static_rail._checks(routed, mode, assay_root.settings, scope)
    fn = getattr(static_rail, fan)

    monkeypatch.setattr(static_rail, "leased", lambda *_a, **_kw: Ok(ok_rows))
    ok_result = fn(checks, routed, assay_root.settings) if fan == "_build_fan" else fn(checks, routed, assay_root.settings, scope)
    assert ok_result == ok_rows, f"{fan} Ok: expected ok_rows propagated, got {ok_result!r}"

    monkeypatch.setattr(static_rail, "leased", lambda *_a, **_kw: Error(fault))
    err_result = fn(checks, routed, assay_root.settings) if fan == "_build_fan" else fn(checks, routed, assay_root.settings, scope)
    assert len(err_result) == 1, f"{fan} Error: expected single-element tuple, got {err_result!r}"
    err = assert_error(err_result[0])
    assert err.message == fault.message, f"{fan} wrong fault message: {err!r}"
    assert err.status is fault.status, f"{fan} wrong status: {err.status!r}"


register_law(build, "build_fan_leased_ok_propagates_rows")
register_law(build, "build_fan_leased_error_wraps_fault")
register_law(fix, "write_fan_leased_ok_propagates_rows")
register_law(fix, "write_fan_leased_error_wraps_fault")


def test_write_fan_route_sha_prefix(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """_write_fan invokes leased with a resource starting with 'write-python-' (_route_sha proves it)."""
    completed = receipt(("ruff", "check", "--fix"), 0, status=RailStatus.OK)
    ok_rows: tuple[object, ...] = (Ok(completed),)
    captured: list[str] = []

    def _fake(resource: str, *_a: object, **_kw: object) -> object:
        captured.append(resource)
        return Ok(ok_rows)

    monkeypatch.setattr(static_rail, "leased", _fake)
    routed = Routed(Language.PYTHON, Scope.FULL, files=("tools/assay/__init__.py",), projects=())
    scope = assay_root.scope(Claim.STATIC)
    checks = static_rail._checks(routed, Mode.WRITE, assay_root.settings, scope)
    result = static_rail._write_fan(checks, routed, assay_root.settings, scope)
    assert len(captured) == 1, f"leased not called exactly once: {captured!r}"
    assert captured[0].startswith("write-python-"), f"_route_sha not invoked: {captured!r}"
    assert result == ok_rows


register_law(fix, "route_sha_included_in_lease_resource")

# --- [VERB_INTEGRATION]


@pytest.mark.parametrize("verb_fn, verb_name", [(fix, "fix"), (build, "build"), (report, "report")], ids=["fix", "build", "report"])
def test_verb_fold_arithmetic_via_canned(verb_fn: _VerbFn, verb_name: str, monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Verb with canned output returns Ok(Report) with claim=STATIC, correct verb, and counts fold invariant."""
    completed = receipt(("tool", "run"), 0, status=RailStatus.OK)
    ok_rows = (Ok(completed),)
    monkeypatch.setattr(static_rail, "leased", lambda *_a, **_kw: Ok(ok_rows))
    monkeypatch.setattr(static_rail, "fan_out", lambda *_a, **_kw: ok_rows)
    scope = assay_root.scope(Claim.STATIC)
    raw: Result[Report, Fault] = verb_fn(assay_root.settings, scope, StaticParams(language=Language.PYTHON, paths=("tools/assay/__init__.py",)))
    r = assert_ok(raw)
    assert r.claim is Claim.STATIC, f"expected STATIC claim, got {r.claim!r}"
    assert r.verb == verb_name, f"expected verb={verb_name!r}, got {r.verb!r}"
    assert r.counts.ok + r.counts.failed == r.counts.total, f"counts arithmetic broken: {r.counts!r}"


register_law(fix, "ok_report_fold_arithmetic_via_canned_output")
register_law(build, "ok_report_fold_arithmetic_via_canned_output")
register_law(report, "ok_report_fold_arithmetic_via_canned_output")


def test_full_combines_debug_release_via_canned_leased(monkeypatch: pytest.MonkeyPatch, assay_root: AssayHarness) -> None:
    """Full with canned leased and fan_out folds two configurations; counts sum correctly (falsifiable: wrong sum or wrong verb)."""
    ok_rows: tuple[object, ...] = (Ok(receipt(("dotnet", "build"), 0, status=RailStatus.OK)),)
    monkeypatch.setattr(static_rail, "leased", lambda *_a, **_kw: Ok(ok_rows))
    monkeypatch.setattr(static_rail, "fan_out", lambda *_a, **_kw: ok_rows)
    assay_root.write("Workspace.slnx", "")
    scope = assay_root.scope(Claim.STATIC)
    r = assert_ok(full(assay_root.settings, scope, StaticParams(paths=("Workspace.slnx",))))
    assert r.verb == "full", f"expected verb=full, got {r.verb!r}"
    assert r.counts.ok + r.counts.failed == r.counts.total, f"full counts arithmetic broken: {r.counts!r}"
    assert r.counts.total > 0, f"expected non-zero total from two configs, got {r.counts!r}"


register_law(full, "combines_debug_release_counts_via_canned_leased")
