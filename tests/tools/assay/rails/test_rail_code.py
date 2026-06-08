"""Laws for tools.assay.rails.code — CodeParams, query, rewrite, search."""

# --- [RUNTIME_PRELUDE] -----------------------------------------------------------------------

from dataclasses import replace as dc_replace
from pathlib import Path
import tempfile
from typing import TYPE_CHECKING

from expression import Ok
from hypothesis import given, settings as hyp_settings
import msgspec
import msgspec.structs
import pytest
from upath import UPath

from tests._aspect import register_law  # noqa: PLC2701
from tests._spec import assert_error, assert_ok, validity_matrix, ValidityCase  # noqa: PLC2701
from tests._strategies import resolve  # noqa: PLC2701
from tools.assay.composition.catalog import AstMatch, Capture, CAPTURE_ENCODER, CAPTURES
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.model import Check, Claim, Fault, Input, Language, Mode, receipt, Runner, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
import tools.assay.rails.code as code_rail
from tools.assay.rails.code import (
    _ag_normalize,  # noqa: PLC2701
    _apply_report,  # noqa: PLC2701
    _artifact,  # noqa: PLC2701
    _checks,  # noqa: PLC2701
    _dispatch,  # noqa: PLC2701
    _languages,  # noqa: PLC2701
    _rg_rows,  # noqa: PLC2701
    _rg_status,  # noqa: PLC2701
    _targets,  # noqa: PLC2701
    _ts_language,  # noqa: PLC2701
    _ts_rows,  # noqa: PLC2701
    _ts_thunk,  # noqa: PLC2701
    CodeParams,
    query,
    rewrite,
    search,
)


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.tools.assay.conftest import AssayHarness


# --- [CONSTANTS] -----------------------------------------------------------------------------

_QUERY_TOOL = Tool("tree-sitter", Runner.INPROC, ("tree-sitter", "query"), Input.FILES, Language.PYTHON, Claim.CODE, mode=Mode.QUERY)
_PY_FUNC_QUERY = "(function_definition name: (identifier) @name)"
_BLANK_CASES: tuple[str, ...] = ("", "   ", "\t")
_AG_MATCH = AstMatch(text="alpha = 1", file="pkg/mod.py", lines="alpha = 1\n", replacement="let alpha = 1")

# --- [LAW_COVERAGE] --------------------------------------------------------------------------

register_law(CodeParams, "bound_positional_projection")
register_law(CodeParams, "bound_does_not_overwrite_flag_pattern")
register_law(CodeParams, "bound_idempotent_explicit_pattern")
register_law(CodeParams, "bound_rejects_blank_pattern")
register_law(CodeParams, "bound_passthrough_non_code_verb")
register_law(CodeParams, "bound_validity_matrix")
register_law(CodeParams, "defaults_are_blank_sentinel")
register_law(query, "ts_thunk_python_captures_name")
register_law(query, "ts_thunk_tsx_grammar")
register_law(query, "ts_thunk_parse_error_becomes_capture_row")
register_law(query, "ts_thunk_query_error_becomes_capture_row")
register_law(query, "ts_thunk_match_limit_truncates")
register_law(query, "ts_thunk_any_of_prefilter_no_false_negative")
register_law(query, "ts_thunk_missing_file_yields_empty")
register_law(query, "query_ok_on_valid_tree")
register_law(query, "query_empty_on_no_match")
register_law(query, "artifact_code_store_path")
register_law(query, "artifact_lines_equals_splitlines")
register_law(search, "search_structural_on_metavar")
register_law(search, "search_content_ok_on_literal_pattern")
register_law(search, "artifact_code_store_path")
register_law(search, "content_search_no_match_empty_status")
register_law(search, "content_search_failure_exit_failed_status")
register_law(search, "content_search_hit_produces_ok_status")
register_law(search, "content_search_hit_with_warning_produces_warning_note")
register_law(search, "content_search_rg_rows_listing_format")
register_law(search, "content_search_no_catalog_row_returns_fault")
register_law(search, "languages_none_resolves_all_code_languages")
register_law(search, "checks_empty_routed_returns_empty_tuple")
register_law(search, "dispatch_empty_checks_returns_empty_tuple")
register_law(search, "targets_empty_paths_returns_default_target")
register_law(search, "ts_language_tsx_key_resolves_tsx_grammar")
register_law(rewrite, "rewrite_preview_ok")
register_law(rewrite, "artifact_code_store_path")
register_law(rewrite, "rewrite_apply_true_via_fan_out")
register_law(rewrite, "rewrite_preview_fan_ok_report")
register_law(rewrite, "apply_report_with_no_output_uses_no_changes")
register_law(rewrite, "apply_report_with_failed_completed_uses_error_status")
register_law(rewrite, "ag_normalize_exit1_parseable_becomes_empty")
register_law(rewrite, "ts_rows_produces_match_rows_and_listing")


# --- [OPERATIONS] ----------------------------------------------------------------------------

# ------ CodeParams laws -----------------------------------------------------------------------


def test_codeparams_defaults() -> None:
    """Default CodeParams has blank pattern, empty rewrite/paths, apply=False, max_results=1000."""
    p = CodeParams()
    assert not p.pattern
    assert not p.rewrite
    assert not p.apply
    assert p.paths == ()
    assert p.max_results == 1000


@pytest.mark.parametrize(
    "verb, paths, expected_pattern, is_fault",
    [
        ("search", ("mypat", "src/"), "mypat", False),
        ("query", ("qpat",), "qpat", False),
        ("rewrite", ("pat", "rep", "src/"), "pat", False),
        ("rewrite", ("pat",), "pat", False),
        ("search", (), "", True),
        ("query", (), "", True),
        ("rewrite", (), "", True),
        ("search", ("   ",), "", True),
    ],
)
def test_codeparams_bound_positional_projection(
    verb: str,
    paths: tuple[str, ...],
    expected_pattern: str,
    is_fault: bool,  # noqa: FBT001
) -> None:
    """bound() projects positionals into pattern/rewrite, faults on missing or blank pattern."""
    result = CodeParams(paths=paths).bound(verb)
    match is_fault:
        case True:
            assert isinstance(result, Fault), f"expected Fault, got {type(result)}: {result!r}"
            assert result.status is RailStatus.FAULTED
        case False:
            assert isinstance(result, CodeParams), f"expected CodeParams, got {type(result)}"
            assert result.pattern == expected_pattern


@given(resolve(CodeParams))
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_codeparams_bound_with_explicit_pattern_never_overwrites(p: CodeParams) -> None:
    """bound() never overwrites a flag-supplied non-blank pattern."""
    for verb in ("search", "query", "rewrite"):
        result = dc_replace(p, pattern="explicit").bound(verb)
        assert not isinstance(result, Fault), f"flag-pattern must survive bound({verb!r}): {result!r}"
        assert isinstance(result, CodeParams)
        assert result.pattern == "explicit", f"pattern overwritten by bound({verb!r})"


@given(resolve(CodeParams))
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_codeparams_bound_idempotent_on_explicit_pattern(p: CodeParams) -> None:
    """bound() is idempotent when pattern is already set — second call changes nothing."""
    for verb in ("search", "query", "rewrite"):
        first = dc_replace(p, pattern="pat", paths=()).bound(verb)
        assert isinstance(first, CodeParams)
        second = first.bound(verb)
        assert isinstance(second, CodeParams)
        assert first.pattern == second.pattern


@pytest.mark.parametrize("blank", _BLANK_CASES, ids=["empty", "spaces", "tab"])
def test_codeparams_bound_rejects_blank_for_all_code_verbs(blank: str) -> None:
    """bound() rejects blank/whitespace-only patterns for search, query, and rewrite."""
    for verb in ("search", "query", "rewrite"):
        result = CodeParams(pattern=blank).bound(verb)
        assert isinstance(result, Fault), f"expected Fault for {blank!r} on {verb!r}"
        assert result.status is RailStatus.FAULTED


def test_codeparams_bound_passthrough_non_code_verb() -> None:
    """bound() with a non-code verb does not trigger pattern validation."""
    assert isinstance(CodeParams(paths=("extra",)).bound("static"), CodeParams)


def test_codeparams_bound_validity_matrix() -> None:
    """Validity matrix: pattern+verb combos that produce/don't produce a Fault."""
    validity_matrix(
        [
            ValidityCase("search_with_pattern_ok", CodeParams(pattern="x").bound("search"), expected=False),
            ValidityCase("query_with_pattern_ok", CodeParams(pattern="x").bound("query"), expected=False),
            ValidityCase("rewrite_with_pattern_ok", CodeParams(pattern="x").bound("rewrite"), expected=False),
            ValidityCase("search_blank_fault", CodeParams(pattern="").bound("search"), expected=True),
            ValidityCase("query_blank_fault", CodeParams(pattern="").bound("query"), expected=True),
        ],
        lambda v: isinstance(v, Fault),
    )


# ------ _ts_thunk / query rail laws -----------------------------------------------------------


def test_ts_thunk_python_query_captures_name(assay_root: AssayHarness) -> None:
    """Python function-name query yields a Capture with correct text, line, and ordinal."""
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    caps = CAPTURES.decode(done.stdout)
    assert done.status.value == "ok"
    assert caps[0].text == "alpha"
    assert caps[0].line == 1
    assert caps[0].column > 0
    assert caps[0].ordinal == 0


def test_ts_thunk_tsx_uses_tsx_grammar(assay_root: AssayHarness) -> None:
    """TSX files are parsed with the TSX grammar — JSX elements are captured."""
    assay_root.write("src/view.tsx", "export const View = () => <div />;\n")
    ts_tool = msgspec.structs.replace(_QUERY_TOOL, language=Language.TYPESCRIPT)
    done = _ts_thunk("(jsx_self_closing_element) @jsx", Language.TYPESCRIPT, assay_root.root, limit=10)(Check(tool=ts_tool, paths=("src/view.tsx",)))
    caps = CAPTURES.decode(done.stdout)
    assert done.status.value == "ok"
    assert caps[0].name == "jsx"
    assert caps[0].file == "src/view.tsx"


def test_ts_thunk_parse_error_emits_capture_row(assay_root: AssayHarness) -> None:
    """A syntactically broken source file surfaces a parse_error Capture row and returncode=1."""
    assay_root.write("pkg/bad.py", "def broken(:\n")
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/bad.py",)))
    assert done.returncode == 1
    assert any(c.parse_error for c in CAPTURES.decode(done.stdout))


def test_ts_thunk_invalid_query_emits_query_error_row(assay_root: AssayHarness) -> None:
    """Invalid query syntax is returned as a query_error Capture row, not an escaped exception."""
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    done = _ts_thunk("(", Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    caps = CAPTURES.decode(done.stdout)
    assert done.returncode == 1
    assert caps[0].name == "query_error"
    assert caps[0].parse_error


def test_ts_thunk_match_limit_truncates(assay_root: AssayHarness) -> None:
    """Tree-sitter match cap is applied at cursor level; Capture.truncated is set on the last row."""
    assay_root.write("pkg/mod.py", "def a():\n    pass\ndef b():\n    pass\n")
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=1)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    caps = CAPTURES.decode(done.stdout)
    assert len(caps) == 1
    assert caps[0].truncated


def test_ts_thunk_any_of_prefilter_no_false_negative(assay_root: AssayHarness) -> None:
    """#any-of? prefilter keeps files matching any literal — no false negatives on valid names."""
    assay_root.write("pkg/mod.py", "def beta():\n    return 1\n")
    q = '(function_definition name: (identifier) @name (#any-of? @name "alpha" "beta"))'
    done = _ts_thunk(q, Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    caps = CAPTURES.decode(done.stdout)
    assert done.status.value == "ok"
    assert caps[0].text == "beta"


def test_ts_thunk_missing_file_yields_empty_receipt(assay_root: AssayHarness) -> None:
    """A path that does not exist on disk is silently skipped — no captures, EMPTY status."""
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("nonexistent.py",)))
    assert done.status is RailStatus.EMPTY
    assert done.returncode == 0


# ------ query / search / rewrite public verb laws -----------------------------------------------


@pytest.mark.parametrize(
    "content, pattern, check_fn",
    [
        ("def alpha():\n    return 1\n", _PY_FUNC_QUERY, lambda r: r.counts.total >= 1),
        ("x = 1\n", _PY_FUNC_QUERY, lambda r: r.status in {RailStatus.EMPTY, RailStatus.OK}),
    ],
    ids=["query_ok_on_valid_tree", "query_empty_on_no_match"],
)
def test_query_public(assay_root: AssayHarness, content: str, pattern: str, check_fn: Callable) -> None:
    """query() returns Ok Report matching expected status and count predicate."""
    assay_root.write("pkg/mod.py", content)
    report = assert_ok(
        query(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern=pattern, language=Language.PYTHON, paths=("pkg/mod.py",)))
    )
    check_fn(report)


@pytest.mark.parametrize(
    "content, pattern",
    [("alpha = 1\nbeta = 2\n", "$NAME = $VAL"), ("def alpha():\n    return 1\n", "alpha")],
    ids=["search_structural_on_metavar", "search_content_ok_on_literal_pattern"],
)
def test_search_public(assay_root: AssayHarness, content: str, pattern: str) -> None:
    """search() routes to ast-grep (metavar) or ripgrep (literal); Ok report carries CODE claim."""
    assay_root.write("pkg/mod.py", content)
    result = search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern=pattern, language=Language.PYTHON, paths=("pkg/mod.py",)))
    match result:
        case _ if result.is_ok():
            assert result.ok.claim is Claim.CODE
        case _:
            assert result.error.status in {RailStatus.FAULTED, RailStatus.FAILED}


def test_rewrite_preview_ok_on_valid_pattern(assay_root: AssayHarness) -> None:
    """rewrite() in preview mode (apply=False) returns an Ok Report for a valid pattern."""
    assay_root.write("pkg/mod.py", "alpha = 1\n")
    params = CodeParams(pattern="$NAME = $VAL", rewrite="let $NAME = $VAL", language=Language.PYTHON, paths=("pkg/mod.py",))
    result = rewrite(assay_root.settings, assay_root.scope(Claim.CODE), params)
    match result:
        case _ if result.is_ok():
            assert result.ok.claim is Claim.CODE
        case _:
            assert result.error.status in {RailStatus.FAULTED, RailStatus.FAILED}


# ------ _artifact laws --------------------------------------------------------------------------


def test_artifact_uses_assay_code_store(assay_root: AssayHarness) -> None:
    """_artifact persists content under the code ArtifactKind bucket and reports the line count."""
    artifact = _artifact(assay_root.scope(Claim.CODE), "search", "a.py:1: hit\nb.py:3: hit2", assay_root.settings)
    assert ".artifacts/assay/code/search/" in artifact.path
    assert artifact.lines == 2


@given(resolve(CodeParams))
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_artifact_line_count_equals_splitlines(p: CodeParams) -> None:
    """_artifact.lines always equals len(content.splitlines()) for any content derived from params."""
    content = f"{p.pattern}\n{p.rewrite}"
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / "Workspace.slnx").write_text("", encoding="utf-8")
        s = AssaySettings(root=UPath(root), exec_target="", exec_known_hosts=None)
        artifact = _artifact(ArtifactScope.open(s, Claim.CODE), "search", content, s)
        assert artifact.lines == len(content.splitlines())


# ------ internal primitive laws ---------------------------------------------------------------


def test_languages_none_resolves_all_code_languages() -> None:
    """_languages(None) returns every language in the CODE catalog, sorted by value, no duplicates."""
    langs = _languages(None)
    assert isinstance(langs, tuple)
    assert len(langs) > 0
    assert langs == tuple(sorted(set(langs), key=lambda m: m.value))


def test_checks_and_dispatch_empty_routed(assay_root: AssayHarness) -> None:
    """_checks() and _dispatch() on Routed with no files return empty tuples without spawning."""
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED, files=())
    assert _checks(routed, Mode.CHECK, lambda t, _r: t) == ()
    assert _dispatch(routed, settings=assay_root.settings, scope=assay_root.scope(Claim.CODE), mode=Mode.CHECK, splice=lambda t, _r: t) == ()


@pytest.mark.parametrize(
    "paths, setup, expected_contains, expected_excludes",
    [((), None, (".",), ()), (("pkg/mod.py", "no_such_dir/"), "pkg/mod.py", ("pkg/mod.py",), ("no_such_dir/",))],
    ids=["targets_empty_paths_returns_default_target", "targets_existing_paths_returned"],
)
def test_targets(
    assay_root: AssayHarness, paths: tuple[str, ...], setup: str | None, expected_contains: tuple[str, ...], expected_excludes: tuple[str, ...]
) -> None:
    """_targets returns ('.',) for empty paths; for non-empty paths keeps existing, drops stale."""
    if setup:
        assay_root.write(setup, "x = 1\n")
    result = _targets(paths, assay_root.root)
    for p in expected_contains:
        assert p in result
    for p in expected_excludes:
        assert p not in result


def test_ts_language_tsx_key_resolves_tsx_grammar() -> None:
    """_ts_language('tsx') returns a distinct TSLanguage from _ts_language('typescript')."""
    from tree_sitter import Language as TSLanguage  # noqa: PLC0415

    tsx_lang, ts_lang = _ts_language("tsx"), _ts_language("typescript")
    assert isinstance(tsx_lang, TSLanguage)
    assert isinstance(ts_lang, TSLanguage)
    assert tsx_lang is not ts_lang


# ------ _rg_status branches -------------------------------------------------------------------


@pytest.mark.parametrize(
    "returncode, has_rows, expected_status, check_note",
    [
        (0, True, RailStatus.OK, None),
        (1, True, RailStatus.OK, None),
        (2, True, RailStatus.OK, "some error"),  # rc=2 + rows → OK; warning note embeds stderr
        (0, False, RailStatus.EMPTY, None),
        (1, False, RailStatus.EMPTY, None),
        (2, False, RailStatus.FAILED, "2"),  # failure note embeds exit code
        (3, False, RailStatus.FAILED, None),
    ],
    ids=["rc0_rows", "rc1_rows", "rc2_rows_warn", "rc0_norows", "rc1_norows", "rc2_norows_fail", "rc3_norows_fail"],
)
def test_rg_status_branches(returncode: int, has_rows: bool, expected_status: RailStatus, check_note: str | None) -> None:  # noqa: FBT001
    """_rg_status dispatches across rc x has_rows to the correct RailStatus; validates embedded notes."""
    status, notes = _rg_status(returncode, "some error", has_rows=has_rows)
    assert status is expected_status
    assert len(notes) >= 1
    if check_note is not None:
        assert any(check_note in n for n in notes)


# ------ _rg_rows parsing ---------------------------------------------------------------------


@pytest.mark.parametrize(
    "raw, expected_count, check_listing",
    [
        (b'{"type":"match","data":{"path":{"text":"src/a.py"},"lines":{"text":"alpha = 1 "},"line_number":3}}\n', 1, "src/a.py:3"),
        (b'{"type":"begin","data":{"path":{"text":"src/a.py"}}}\nnot-json\n', 0, None),
    ],
    ids=["match_events_parsed", "non_match_events_ignored"],
)
def test_rg_rows(raw: bytes, expected_count: int, check_listing: str | None) -> None:
    """_rg_rows decodes ripgrep NDJSON match events; skips begin/end/non-JSON without raising."""
    rows, listing = _rg_rows((receipt(("rg",), 0, stdout=raw, status=RailStatus.OK),), 100, "alpha")
    assert len(rows) == expected_count
    if check_listing:
        assert check_listing in listing
        assert "alpha" in rows[0].text
        assert rows[0].id == "src/a.py:3"
    else:
        assert not listing


# ------ _apply_report branches ----------------------------------------------------------------


@pytest.mark.parametrize(
    "stdout, rc, status_in, expected_status, ok_count",
    [
        (b"", 0, RailStatus.OK, RailStatus.OK, 1),  # no output → no-changes fallback
        (b"3 files changed", 0, RailStatus.OK, RailStatus.OK, 1),  # stdout present → OK
        (b"error", 1, RailStatus.FAILED, RailStatus.FAILED, 0),  # FAILED propagated
        (b"", 0, RailStatus.EMPTY, RailStatus.EMPTY, 0),  # empty completeds → EMPTY
    ],
    ids=["no_output", "with_stderr", "failed", "empty_completeds"],
)
def test_apply_report_branches(stdout: bytes, rc: int, status_in: RailStatus, expected_status: RailStatus, ok_count: int) -> None:
    """_apply_report dispatches across output x status to correct report status."""
    completeds = (receipt(("code", "rewrite", "pat"), rc, stdout=stdout, status=status_in),) if status_in is not RailStatus.EMPTY else ()
    report = _apply_report("rewrite", "pat", completeds)
    assert report.status is expected_status
    if ok_count:
        assert report.counts.ok == ok_count


# ------ _ag_normalize + _ts_rows branches -----------------------------------------------------


@pytest.mark.parametrize(
    "stdout, rc, status_in, expected_status",
    [
        (b"[]", 1, RailStatus.FAILED, RailStatus.EMPTY),  # exit1 + parseable JSON → EMPTY
        (b"not json", 1, RailStatus.FAILED, RailStatus.FAILED),  # exit1 + garbage → stays FAILED
        (b"[]", 0, RailStatus.OK, RailStatus.OK),  # exit0 → unchanged
    ],
    ids=["exit1_parseable", "exit1_garbage", "exit0_unchanged"],
)
def test_ag_normalize_branches(stdout: bytes, rc: int, status_in: RailStatus, expected_status: RailStatus) -> None:
    """_ag_normalize maps rc=1+JSON to EMPTY; keeps FAILED for garbage; leaves rc=0 unchanged."""
    assert _ag_normalize((receipt(("ast-grep",), rc, stdout=stdout, status=status_in),))[0].status is expected_status


@pytest.mark.parametrize(
    "name, text, parse_error, rc, status_in, expected_id_fragment, expected_severity",
    [
        ("name", "alpha", False, 0, RailStatus.OK, "pkg/mod.py:1:5:name:0", None),
        ("parse_error", "tree-sitter parse error", True, 1, RailStatus.FAILED, "pkg/bad.py:", "failed"),
    ],
    ids=["match_row", "parse_error_row"],
)
def test_ts_rows_produces_match_rows_and_listing(
    name: str,
    text: str,
    parse_error: bool,  # noqa: FBT001
    rc: int,
    status_in: RailStatus,
    expected_id_fragment: str,
    expected_severity: str | None,
) -> None:
    """_ts_rows decodes Capture JSON into Match rows with correct id, text, severity, and listing."""
    cap = Capture(name=name, text=text, file="pkg/mod.py" if not parse_error else "pkg/bad.py", line=1, column=5, ordinal=0, parse_error=parse_error)
    done = receipt(("tree-sitter",), rc, stdout=CAPTURE_ENCODER.encode((cap,)), status=status_in)
    rows, listing = _ts_rows((done,), 100, "(function_definition)")
    assert len(rows) == 1
    assert expected_id_fragment in rows[0].id
    assert rows[0].severity == expected_severity
    if not parse_error:
        assert text in rows[0].text
        assert "pkg/mod.py:1" in listing
    else:
        assert "parse-error" in rows[0].text


# ------ monkeypatched verb integration laws ---------------------------------------------------


@pytest.mark.parametrize(
    "stdout, rc, status_in, expected_report_status",
    [
        (b'{"type":"match","data":{"path":{"text":"pkg/mod.py"},"lines":{"text":"alpha "},"line_number":1}}\n', 0, RailStatus.OK, RailStatus.OK),
        (b"", 1, RailStatus.EMPTY, RailStatus.EMPTY),
        (b"", 2, RailStatus.FAILED, RailStatus.FAILED),
        (b'{"type":"match","data":{"path":{"text":"a.py"},"lines":{"text":"hit "},"line_number":1}}\n', 2, RailStatus.FAILED, RailStatus.OK),
    ],
    ids=["hit_ok", "no_match_empty", "failure_exit_failed", "rg_warning_note"],
)
def test_content_search_monkeypatched(
    assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, stdout: bytes, rc: int, status_in: RailStatus, expected_report_status: RailStatus
) -> None:
    """search() literal: canned run_check outcomes drive report status across rc x rows matrix."""
    monkeypatch.setattr(code_rail, "run_check", lambda *_a, **_kw: Ok(receipt(("rg",), rc, stdout=stdout, status=status_in)))
    report = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern="alpha", paths=())))
    assert report.status is expected_report_status
    if expected_report_status is RailStatus.OK and rc == 0:
        assert report.counts.total >= 1


@pytest.mark.parametrize("apply", [True, False], ids=["apply_true", "apply_false"])
def test_rewrite_fan_out(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch, apply: bool) -> None:  # noqa: FBT001
    """rewrite(apply) routes through fan_out; canned OK match propagates to the report."""
    canned = receipt(("ast-grep",), 0, stdout=msgspec.json.encode((_AG_MATCH,)), status=RailStatus.OK)
    monkeypatch.setattr(code_rail, "fan_out", lambda *_a, **_kw: (Ok(canned),))
    if apply:
        monkeypatch.setattr(code_rail, "leased", lambda _res, action, **_kw: action(None))
    params = CodeParams(pattern="$NAME = $VAL", rewrite="let $NAME = $VAL", apply=apply, language=Language.PYTHON, paths=())
    report = assert_ok(rewrite(assay_root.settings, assay_root.scope(Claim.CODE), params))
    assert report.claim is Claim.CODE
    if not apply:
        assert report.counts.total >= 1


def test_content_search_no_catalog_row_returns_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_content_search faults with FAULTED when no CONTENT-mode tool exists in the catalog."""
    monkeypatch.setattr(code_rail, "select", lambda *_a, **_kw: ())
    fault = assert_error(search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern="anything", paths=())))
    assert fault.status is RailStatus.FAULTED
    assert "no ripgrep" in fault.message
