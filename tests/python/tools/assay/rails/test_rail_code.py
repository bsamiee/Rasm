"""Laws for tools.assay.rails.code — CodeParams, query, search."""

# --- [RUNTIME_PRELUDE] ------------------------------------------------------------------

from dataclasses import replace as dc_replace
from hashlib import sha256
from pathlib import Path
import shutil
import tempfile
from typing import TYPE_CHECKING

from expression import Ok
from hypothesis import given, settings as hyp_settings
import msgspec
import msgspec.structs
import pytest
from upath import UPath

from tests.python._testkit.laws import register_law
from tests.python._testkit.spec import assert_error, assert_ok, validity_matrix, ValidityCase
from tests.python._testkit.strategies import resolve
from tools.assay.composition.catalog import AstMatch, Capture, CAPTURE_ENCODER, CAPTURES
from tools.assay.composition.settings import ArtifactScope, AssaySettings
from tools.assay.core.model import ArtifactKind, Check, Claim, Fault, Input, Language, Mode, receipt, Runner, Tool
from tools.assay.core.routing import Routed, Scope
from tools.assay.core.status import RailStatus
import tools.assay.rails.code as code_rail
from tools.assay.rails.code import (
    _ag_normalize,  # private primitives under direct law coverage
    _AG_SPEC,  # private primitives under direct law coverage
    _artifact,  # private primitives under direct law coverage
    _checks,  # private primitives under direct law coverage
    _content_splice,  # private primitives under direct law coverage
    _dispatch,  # private primitives under direct law coverage
    _eq_needles,  # private primitives under direct law coverage
    _languages,  # private primitives under direct law coverage
    _project_rows,  # private primitives under direct law coverage
    _RG_SPEC,  # private primitives under direct law coverage
    _rg_status,  # private primitives under direct law coverage
    _search_splice,  # private primitives under direct law coverage
    _targets,  # private primitives under direct law coverage
    _top_level_patterns,  # private primitives under direct law coverage
    _ts_grammar,  # private primitives under direct law coverage
    _TS_SPEC,  # private primitives under direct law coverage
    _ts_thunk,  # private primitives under direct law coverage
    CodeParams,
    query,
    search,
    ts_language,
)


if TYPE_CHECKING:
    from collections.abc import Callable

    from tests.python.tools.assay.kit import AssayHarness


# --- [CONSTANTS] ------------------------------------------------------------------------

_QUERY_TOOL = Tool("tree-sitter", Runner.INPROC, ("tree-sitter", "query"), Input.FILES, Language.PYTHON, Claim.CODE, mode=Mode.QUERY)
_PY_FUNC_QUERY = "(function_definition name: (identifier) @name)"
_BLANK_CASES: tuple[str, ...] = ("", "   ", "\t")
_AG_MATCH = AstMatch(text="alpha = 1", file="pkg/mod.py", lines="alpha = 1\n", replacement="let alpha = 1")

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CODEPARAMS_LAWS]


def test_codeparams_defaults() -> None:
    """Default CodeParams has blank pattern, empty paths, max_results=1000."""
    p = CodeParams()
    assert not p.pattern
    assert p.paths == ()
    assert p.max_results == 1000


@pytest.mark.parametrize(
    "verb, paths, expected_pattern, is_fault",
    [
        ("search", ("mypat", "src/"), "mypat", False),
        ("query", ("qpat",), "qpat", False),
        ("search", (), "", True),
        ("query", (), "", True),
        ("search", ("   ",), "", True),
    ],
)
def test_codeparams_bound_positional_projection(
    verb: str,
    paths: tuple[str, ...],
    expected_pattern: str,
    is_fault: bool,  # noqa: FBT001  # parametrized bool flag
) -> None:
    """bound() projects the leading positional into pattern, faults on missing or blank pattern."""
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
    for verb in ("search", "query"):
        result = dc_replace(p, pattern="explicit").bound(verb)
        assert not isinstance(result, Fault), f"flag-pattern must survive bound({verb!r}): {result!r}"
        assert isinstance(result, CodeParams)
        assert result.pattern == "explicit", f"pattern overwritten by bound({verb!r})"


@given(resolve(CodeParams))
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_codeparams_bound_idempotent_on_explicit_pattern(p: CodeParams) -> None:
    """bound() is idempotent when pattern is already set — second call changes nothing."""
    for verb in ("search", "query"):
        first = dc_replace(p, pattern="pat", paths=()).bound(verb)
        assert isinstance(first, CodeParams)
        second = first.bound(verb)
        assert isinstance(second, CodeParams)
        assert first.pattern == second.pattern


@pytest.mark.parametrize("blank", _BLANK_CASES, ids=["empty", "spaces", "tab"])
def test_codeparams_bound_rejects_blank_for_all_code_verbs(blank: str) -> None:
    """bound() rejects blank/whitespace-only patterns for search and query."""
    for verb in ("search", "query"):
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
            ValidityCase("search_blank_fault", CodeParams(pattern="").bound("search"), expected=True),
            ValidityCase("query_blank_fault", CodeParams(pattern="").bound("query"), expected=True),
        ],
        lambda v: isinstance(v, Fault),
    )


# --- [TS_THUNK_QUERY_LAWS]


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


# --- [ARTIFACT_LAWS]


def test_artifact_uses_assay_code_store(assay_root: AssayHarness) -> None:
    """_artifact persists content under the code ArtifactKind bucket and reports the line count."""
    artifact = _artifact(assay_root.scope(Claim.CODE), "search", "a.py:1: hit\nb.py:3: hit2", assay_root.settings)
    assert ".artifacts/assay/code/search/" in artifact.path
    assert artifact.lines == 2


@given(resolve(CodeParams))
@hyp_settings(parent=hyp_settings.get_profile("rasm"))
def test_artifact_line_count_equals_splitlines(p: CodeParams) -> None:
    """_artifact.lines always equals len(content.splitlines()) for any content derived from params."""
    content = "\n".join((p.pattern, *p.paths))
    with tempfile.TemporaryDirectory() as tmp:
        root = Path(tmp)
        (root / "Workspace.slnx").write_text("", encoding="utf-8")
        s = AssaySettings(root=UPath(root), exec_target="", exec_known_hosts=None)
        artifact = _artifact(ArtifactScope.open(s, Claim.CODE), "search", content, s)
        assert artifact.lines == len(content.splitlines())


# --- [INTERNAL_PRIMITIVE_LAWS]


def test_languages_none_resolves_all_code_languages() -> None:
    """_languages(None, ()) returns every language in the CODE catalog, sorted by value, no duplicates."""
    langs = _languages(None, ())
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
    """ts_language composed over the _ts_grammar resolver yields distinct TSLanguages for tsx vs typescript."""
    from tree_sitter import (  # noqa: PLC0415  # local import avoids aliasing `Language` from tools.assay.core.model at module scope
        Language as TSLanguage,
    )

    tsx_lang, ts_lang = ts_language(_ts_grammar(Language.TYPESCRIPT, is_tsx=True)), ts_language(_ts_grammar(Language.TYPESCRIPT, is_tsx=False))
    assert isinstance(tsx_lang, TSLanguage)
    assert isinstance(ts_lang, TSLanguage)
    assert tsx_lang is not ts_lang


# --- [RG_STATUS_LAWS]


@pytest.mark.parametrize(
    "returncode, has_rows, expected_status, check_note",
    [
        (0, True, RailStatus.OK, None),
        (1, True, RailStatus.OK, None),
        (2, True, RailStatus.OK, "some error"),  # rc=2 with rows → OK; stderr surfaced as warning note
        (0, False, RailStatus.EMPTY, None),
        (1, False, RailStatus.EMPTY, None),
        (2, False, RailStatus.FAILED, "2"),  # rc=2 without rows → FAILED; exit code embedded in note
        (3, False, RailStatus.FAILED, None),
    ],
    ids=["rc0_rows", "rc1_rows", "rc2_rows_warn", "rc0_norows", "rc1_norows", "rc2_norows_fail", "rc3_norows_fail"],
)
def test_rg_status_branches(
    returncode: int,
    has_rows: bool,  # noqa: FBT001  # parametrized bool flag
    expected_status: RailStatus,
    check_note: str | None,
) -> None:
    """_rg_status dispatches across rc x has_rows to the correct RailStatus; validates embedded notes."""
    status, notes = _rg_status(returncode, "some error", has_rows=has_rows)
    assert status is expected_status
    assert len(notes) >= 1
    if check_note is not None:
        assert any(check_note in n for n in notes)


# --- [RG_ROWS_LAWS]


@pytest.mark.parametrize(
    "raw, expected_count, check_listing",
    [
        (b'{"type":"match","data":{"path":{"text":"src/a.py"},"lines":{"text":"alpha = 1 "},"line_number":3}}\n', 1, "src/a.py:3"),
        (b'{"type":"begin","data":{"path":{"text":"src/a.py"}}}\nnot-json\n', 0, None),
    ],
    ids=["match_events_parsed", "non_match_events_ignored"],
)
def test_rg_rows(raw: bytes, expected_count: int, check_listing: str | None) -> None:
    """_project_rows over _RG_SPEC decodes ripgrep NDJSON match events; skips begin/end/non-JSON without raising."""
    rows, listing, notes = _project_rows((receipt(("rg",), 0, stdout=raw, status=RailStatus.OK),), 100, "alpha", spec=_RG_SPEC)
    assert len(rows) == expected_count
    assert notes == ()  # uncapped row sets carry no truncation note
    if check_listing:
        assert check_listing in listing
        assert "alpha" in rows[0].text
        assert rows[0].id == "ripgrep:src/a.py:3"
    else:
        assert not listing


# --- [AG_NORMALIZE_TS_ROWS_LAWS]


@pytest.mark.parametrize(
    "stdout, rc, status_in, expected_status",
    [
        (b"[]", 1, RailStatus.FAILED, RailStatus.EMPTY),  # rc=1 + parseable JSON → no-match, not failure
        (b"not json", 1, RailStatus.FAILED, RailStatus.FAILED),  # rc=1 + garbage → genuine failure
        (b"[]", 0, RailStatus.OK, RailStatus.OK),
    ],
    ids=["exit1_parseable", "exit1_garbage", "exit0_unchanged"],
)
def test_ag_normalize_branches(stdout: bytes, rc: int, status_in: RailStatus, expected_status: RailStatus) -> None:
    """_ag_normalize maps rc=1+JSON to EMPTY; keeps FAILED for garbage; leaves rc=0 unchanged."""
    assert _ag_normalize((receipt(("ast-grep",), rc, stdout=stdout, status=status_in),))[0].status is expected_status


@pytest.mark.parametrize(
    "name, text, parse_error, rc, status_in, expected_id_fragment, expected_severity",
    [
        ("name", "alpha", False, 0, RailStatus.OK, "tree-sitter:pkg/mod.py:1:name", None),
        ("parse_error", "tree-sitter parse error", True, 1, RailStatus.FAILED, "tree-sitter:pkg/bad.py:", "failed"),
    ],
    ids=["match_row", "parse_error_row"],
)
def test_ts_rows_produces_match_rows_and_listing(
    name: str,
    text: str,
    parse_error: bool,  # noqa: FBT001  # parametrized bool flag
    rc: int,
    status_in: RailStatus,
    expected_id_fragment: str,
    expected_severity: str | None,
) -> None:
    """_project_rows over _TS_SPEC decodes Capture JSON into Match rows with correct id, text, severity, and listing."""
    cap = Capture(name=name, text=text, file="pkg/mod.py" if not parse_error else "pkg/bad.py", line=1, column=5, ordinal=0, parse_error=parse_error)
    done = receipt(("tree-sitter",), rc, stdout=CAPTURE_ENCODER.encode((cap,)), status=status_in)
    rows, listing, _notes = _project_rows((done,), 100, "(function_definition)", spec=_TS_SPEC)
    assert len(rows) == 1
    assert expected_id_fragment in rows[0].id
    assert rows[0].severity == expected_severity
    if not parse_error:
        assert text in rows[0].text
        assert "pkg/mod.py:1" in listing
    else:
        assert "parse-error" in rows[0].text


# --- [MATCH_ID_AND_CAP_NOTE_LAWS]


def test_match_ids_carry_source_prefixes() -> None:
    """Every spec row emits identity ids shaped source:file:line[:capture] with its tool prefix."""
    ts_cap = Capture(name="name", text="alpha", file="pkg/mod.py", line=1, column=5)
    rg_event = b'{"type":"match","data":{"path":{"text":"src/a.py"},"lines":{"text":"alpha"},"line_number":3}}\n'
    ag = receipt(("ast-grep",), 0, stdout=msgspec.json.encode((_AG_MATCH,)), status=RailStatus.OK)
    ts = receipt(("tree-sitter",), 0, stdout=CAPTURE_ENCODER.encode((ts_cap,)), status=RailStatus.OK)
    rg = receipt(("rg",), 0, stdout=rg_event, status=RailStatus.OK)
    assert _project_rows((ag,), 10, "$X", spec=_AG_SPEC)[0][0].id == "ast-grep:pkg/mod.py:1"
    assert _project_rows((ts,), 10, "(q)", spec=_TS_SPEC)[0][0].id == "tree-sitter:pkg/mod.py:1:name"
    assert _project_rows((rg,), 10, "alpha", spec=_RG_SPEC)[0][0].id == "ripgrep:src/a.py:3"


def test_project_rows_shape_parity() -> None:
    """Each spec row reproduces its per-family projector shape byte-for-byte: id, text, listing line, and empty uncapped note."""
    ts_cap = Capture(name="name", text="alpha", file="pkg/mod.py", line=1, column=5, ordinal=0, pattern=0)
    rg_line = b'{"type":"match","data":{"path":{"text":"src/a.py"},"lines":{"text":"alpha = 1 \\n"},"line_number":3}}\n'
    ag = _project_rows((receipt(("ast-grep",), 0, stdout=msgspec.json.encode((_AG_MATCH,)), status=RailStatus.OK),), 10, "$X", spec=_AG_SPEC)
    ts = _project_rows((receipt(("tree-sitter",), 0, stdout=CAPTURE_ENCODER.encode((ts_cap,)), status=RailStatus.OK),), 10, "(q)", spec=_TS_SPEC)
    rg = _project_rows((receipt(("rg",), 0, stdout=rg_line, status=RailStatus.OK),), 10, "alpha", spec=_RG_SPEC)
    assert (ag[0][0].id, ag[0][0].text, ag[1], ag[2]) == (
        "ast-grep:pkg/mod.py:1",
        "alpha = 1 => let alpha = 1",
        "pkg/mod.py:1: alpha = 1 => let alpha = 1",
        (),
    )
    assert (ts[0][0].id, ts[0][0].text, ts[0][0].severity, ts[1], ts[2]) == (
        "tree-sitter:pkg/mod.py:1:name",
        "@name alpha",
        None,
        "pkg/mod.py:1:5: @name#0/0 alpha",
        (),
    )
    assert (rg[0][0].id, rg[0][0].text, rg[1], rg[2]) == ("ripgrep:src/a.py:3", "alpha = 1", "src/a.py:3: alpha = 1", ())


def test_content_search_forced_cap_emits_results_note(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """search() with max_results=1 over 2 content hits notes 'results: 1 of 2 (cap=1); full listing in artifact'."""
    two = (
        b'{"type":"match","data":{"path":{"text":"a.py"},"lines":{"text":"alpha 1"},"line_number":1}}\n'
        b'{"type":"match","data":{"path":{"text":"a.py"},"lines":{"text":"alpha 2"},"line_number":2}}\n'
    )
    monkeypatch.setattr(code_rail, "run_check", lambda *_a, **_kw: Ok(receipt(("rg",), 0, stdout=two, status=RailStatus.OK)))
    report = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern="alpha", paths=(), max_results=1)))
    assert "results: 1 of 2 (cap=1); full listing in artifact" in report.notes


def test_structural_search_forced_cap_emits_results_note(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Structural (ast-grep) search with max_results=1 over 2 matches emits the exact N-of-M cap note."""
    assay_root.write("pkg/mod.py", "alpha = 1\nbeta = 2\n")
    two = msgspec.json.encode((_AG_MATCH, AstMatch(text="beta = 2", file="pkg/mod.py", lines="beta = 2\n")))
    monkeypatch.setattr(code_rail, "fan_out", lambda *_a, **_kw: (Ok(receipt(("ast-grep",), 0, stdout=two, status=RailStatus.OK)),))
    params = CodeParams(pattern="$NAME = $VAL", language=Language.PYTHON, paths=(), max_results=1)
    report = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), params))
    assert "results: 1 of 2 (cap=1); full listing in artifact" in report.notes


def test_query_forced_cap_emits_saturation_note(assay_root: AssayHarness) -> None:
    """query() with max_results=1 over 2 defs surfaces the match-limit-saturated cap note in the report."""
    assay_root.write("pkg/mod.py", "def a():\n    pass\n\ndef b():\n    pass\n")
    params = CodeParams(pattern=_PY_FUNC_QUERY, language=Language.PYTHON, paths=("pkg/mod.py",), max_results=1)
    report = assert_ok(query(assay_root.settings, assay_root.scope(Claim.CODE), params))
    assert any("(cap=1, match-limit saturated); full listing in artifact" in n for n in report.notes), report.notes


# --- [PUBLIC_VERB_LAWS]


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
    "content, pattern, binary",
    [("alpha = 1\nbeta = 2\n", "$NAME = $VAL", "ast-grep"), ("def alpha():\n    return 1\n", "alpha", "rg")],
    ids=["search_structural_on_metavar", "search_content_ok_on_literal_pattern"],
)
def test_search_public(assay_root: AssayHarness, content: str, pattern: str, binary: str) -> None:
    """search() routes to ast-grep (metavar) or ripgrep (literal); when the spawn binary resolves, the Ok report carries CODE claim.

    The both-rails fallback survives only when the routed binary is absent from PATH (a CI without ast-grep/rg);
    a resolvable binary must reach the Ok rail, so a real routing/spawn regression cannot hide behind the Error arm.
    """
    assay_root.write("pkg/mod.py", content)
    result = search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern=pattern, language=Language.PYTHON, paths=("pkg/mod.py",)))
    match (shutil.which(binary), result.is_ok()):
        case (str(), _):
            assert result.is_ok(), f"resolvable {binary!r} must reach the Ok rail, not Error: {result!r}"
            assert assert_ok(result).claim is Claim.CODE
        case (None, True):
            assert assert_ok(result).claim is Claim.CODE
        case (None, False):
            assert assert_error(result).status in {RailStatus.FAULTED, RailStatus.FAILED}


# --- [MONKEYPATCHED_INTEGRATION_LAWS]


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


def test_content_search_no_catalog_row_returns_fault(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """_content_search faults with FAULTED when no CONTENT-mode tool exists in the catalog."""
    monkeypatch.setattr(code_rail, "select", lambda *_a, **_kw: ())
    fault = assert_error(search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern="anything", paths=())))
    assert fault.status is RailStatus.FAULTED
    assert "no ripgrep" in fault.message


# --- [QUERY_KERNEL_BYTE_LAWS]


@pytest.mark.parametrize(
    "query_src, expected",
    [("(a)", 1), ("(a) (b)", 2), ("(a (b))", 1), ('(a (#eq? @x "(y)"))', 1), ("; note (c)\n(a)", 1), (")(a)", 1), ("", 0)],
    ids=["single", "two_top_level", "nested_uncounted", "string_masked_paren", "comment_masked_paren", "unbalanced_close_clamped", "empty"],
)
def test_top_level_patterns_masked_depth_count(query_src: str, expected: int) -> None:
    """Kills _top_level_patterns fold mutations.

    String/comment mask-regex corruption, '(' arm removal, count sign flips, and depth-clamp/seed-state
    perturbations all shift the top-level pattern count.
    """
    assert _top_level_patterns(query_src) == expected


@pytest.mark.parametrize(
    "query_src, expected",
    [
        ('(function_definition name: (identifier) @name (#eq? @name "alpha"))', (frozenset((b"alpha",)),)),
        ('(function_definition name: (identifier) @name (#any-of? @name "alpha" "beta"))', (frozenset((b"alpha", b"beta")),)),
        (
            '(call function: (identifier) @f (#eq? @f "go") arguments: (argument_list (string) @s (#any-of? @s "x" "y")))',
            (frozenset((b"go",)), frozenset((b"x", b"y"))),
        ),
        ("(a) (b)", None),
        ('(a (#match? @x "re"))', None),
        ("(function_definition) @def", None),
    ],
    ids=["eq_single", "any_of_group", "eq_plus_any_of", "two_patterns_refused", "regex_predicate_refused", "predicate_free_refused"],
)
def test_eq_needles_prefilter_byte_sets(query_src: str, expected: tuple[frozenset[bytes], ...] | None) -> None:
    """Kills _eq_needles widen/narrow mutations.

    Predicate- or literal-regex corruption, pattern-count gate flips, subset-operator narrowing, and group
    deletion all change the needle groups or wrongly toggle prefiltering.
    """
    assert _eq_needles(query_src) == expected


def test_ts_thunk_eq_prefilter_skip_and_admit(assay_root: AssayHarness) -> None:
    """Kills _ts_thunk prefilter-gate mutations.

    Disabling needles parses needle-free broken files into FAILED instead of EMPTY; inverting membership
    skips needle-bearing valid files into EMPTY instead of OK.
    """
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    assay_root.write("pkg/broken_miss.py", "def broken(:\n")
    thunk = _ts_thunk('(function_definition name: (identifier) @name (#eq? @name "alpha"))', Language.PYTHON, assay_root.root, limit=10)
    admitted = thunk(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    skipped = thunk(Check(tool=_QUERY_TOOL, paths=("pkg/broken_miss.py",)))
    assert (admitted.status, CAPTURES.decode(admitted.stdout)[0].text) == (RailStatus.OK, "alpha")
    assert (skipped.status, skipped.returncode, tuple(CAPTURES.decode(skipped.stdout))) == (RailStatus.EMPTY, 0, ())


def test_ts_thunk_receipt_argv_and_cap_fallback(assay_root: AssayHarness) -> None:
    """Kills _ts_thunk receipt mutations.

    Argv slot corruption on the tree-sitter provenance tuple and the limit>0 fallback flip that turns
    limit=0 into cap=0 (EMPTY receipt) instead of the _RESULT_CAP fallback both diverge here.
    """
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=0)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    assert done.argv == ("tree-sitter", "query", Language.PYTHON, "pkg/mod.py")
    assert done.status is RailStatus.OK
    assert len(CAPTURES.decode(done.stdout)) == 1


def test_ts_capture_full_byte_shape(assay_root: AssayHarness) -> None:
    """Kills _ts_file_captures projection mutations.

    Any dropped or offset capture field (column/end_line/end_column/start_byte/end_byte/pattern/ordinal)
    and the >=-truncation flip at exact cap diverge from the exact Capture rows.
    """
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    done = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",)))
    expected = Capture(name="name", text="alpha", file="pkg/mod.py", line=1, column=5, end_line=1, end_column=10, start_byte=4, end_byte=9)
    assert tuple(CAPTURES.decode(done.stdout)) == (expected,)
    assay_root.write("pkg/two.py", "def a():\n    pass\ndef b():\n    pass\n")
    exact = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=2)(Check(tool=_QUERY_TOOL, paths=("pkg/two.py",)))
    assert [(c.ordinal, c.truncated) for c in CAPTURES.decode(exact.stdout)] == [(0, False), (1, False)]


def test_ts_capture_error_row_identity(assay_root: AssayHarness) -> None:
    """Kills _ts_file_captures error-row mutations.

    The query_error row must carry the compiler message and file; the parse_error row must keep exact
    name/text/file and the saturation-derived truncated flag.
    """
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    qerr = CAPTURES.decode(_ts_thunk("(", Language.PYTHON, assay_root.root, limit=10)(Check(tool=_QUERY_TOOL, paths=("pkg/mod.py",))).stdout)
    assert (qerr[0].name, qerr[0].file, qerr[0].line, qerr[0].parse_error) == ("query_error", "pkg/mod.py", 1, True)
    assert "EOF" in qerr[0].text
    assay_root.write("pkg/broken_sat.py", "def a():\n    pass\ndef b():\n    pass\ndef oops(:\n")
    saturated = _ts_thunk(_PY_FUNC_QUERY, Language.PYTHON, assay_root.root, limit=1)(Check(tool=_QUERY_TOOL, paths=("pkg/broken_sat.py",)))
    expected = Capture(name="parse_error", text="tree-sitter parse error", file="pkg/broken_sat.py", line=1, parse_error=True, truncated=True)
    assert tuple(CAPTURES.decode(saturated.stdout)) == (expected,)


def test_splice_argv_shapes(assay_root: AssayHarness) -> None:
    """Kills _search_splice/_content_splice argv mutations.

    Flag-string corruption, dropped command splice, and glob/pattern/target tail reordering all diverge
    from the exact spawn argv.
    """
    routed = Routed(language=Language.PYTHON, scope=Scope.CHANGED)
    ag = _search_splice(CodeParams(pattern="$X = $Y"), assay_root.root)(_QUERY_TOOL, routed)
    assert ag.command == (*_QUERY_TOOL.command, "-p", "$X = $Y", "-l", "python", "--json=compact", "--no-ignore", "hidden", ".")
    rg = _content_splice(_QUERY_TOOL, CodeParams(pattern="alpha", language=Language.PYTHON), assay_root.root)
    globs, tail = rg.command[len(_QUERY_TOOL.command) : -4], rg.command[-4:]
    assert tail == ("-e", "alpha", "--", ".")
    assert (globs[::2], set(globs[1::2])) == (("--glob", "--glob"), {"*.py", "*.pyi"})
    assert _content_splice(_QUERY_TOOL, CodeParams(pattern="alpha"), assay_root.root).command == (*_QUERY_TOOL.command, "-e", "alpha", "--", ".")


@pytest.mark.parametrize(
    "returncode, stderr, has_rows, expected",
    [
        (0, "boom", True, (RailStatus.OK, ("content match",))),
        (2, "x" * 250, True, (RailStatus.OK, ("content match; ripgrep warning: " + "x" * 200,))),
        (1, "boom", False, (RailStatus.EMPTY, ("no content matches",))),
        (2, "boom", False, (RailStatus.FAILED, ("ripgrep failed (exit 2): boom",))),
        (3, "", False, (RailStatus.FAILED, ("ripgrep failed (exit 3): error",))),
    ],
    ids=["rc0_rows_no_warning", "rc2_rows_warning_truncated_200", "rc1_norows_exact_note", "rc2_norows_exact_message", "rc3_empty_stderr_fallback"],
)
def test_rg_status_exact_note_bytes(
    returncode: int,
    stderr: str,
    has_rows: bool,  # noqa: FBT001  # parametrized bool flag
    expected: tuple[RailStatus, tuple[str, ...]],
) -> None:
    """Kills _rg_status note mutations.

    Warnings on clean exits, corrupted note prose, 200-char stderr truncation off-by-one, and and/or
    fallback flips all diverge from the exact (status, notes) tuples.
    """
    assert _rg_status(returncode, stderr, has_rows=has_rows) == expected


def test_content_report_byte_shape(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Kills _content_report mutations.

    The stderr rail must decode with errors=replace (an and-flip drops the warning), verb stays 'search',
    rows ride Report.results with pinned ids/scores, and the artifact pins digest/kind/bytes/run_id path.
    """
    two = (
        b'{"type":"match","data":{"path":{"text":"a.py"},"lines":{"text":"alpha 1"},"line_number":1}}\n'
        b'{"type":"match","data":{"path":{"text":"a.py"},"lines":{"text":"alpha 2"},"line_number":2}}\n'
    )
    canned = receipt(("rg",), 2, stdout=two, stderr=b"warn \xff tail", status=RailStatus.FAILED)
    monkeypatch.setattr(code_rail, "run_check", lambda *_a, **_kw: Ok(canned))
    report = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), CodeParams(pattern="alpha", paths=())))
    assert (report.verb, report.status) == ("search", RailStatus.OK)
    assert "content match; ripgrep warning: warn � tail" in report.notes
    assert tuple((r.id, r.score) for r in report.results) == (("ripgrep:a.py:1", 71), ("ripgrep:a.py:2", 71))
    listing = "a.py:1: alpha 1\na.py:2: alpha 2"
    artifact = report.artifacts[0]
    expected_artifact = (sha256(listing.encode()).hexdigest()[:12], ArtifactKind.CODE, len(listing), 2)
    assert (artifact.id, artifact.kind, artifact.bytes, artifact.lines) == expected_artifact
    assert artifact.path.endswith(f"code/search/{assay_root.settings.run_id}/matches.txt")


def test_structural_report_promotion_and_defect_rows(assay_root: AssayHarness, monkeypatch: pytest.MonkeyPatch) -> None:
    """Kills _report fold mutations.

    An EMPTY base with projected rows must promote to OK with rows in Report.results; FAILED with no rows
    must fall back to fold defect rows carrying tool argv provenance; verb stays 'search'.
    """
    assay_root.write("pkg/mod.py", "alpha = 1\n")
    params = CodeParams(pattern="$NAME = $VAL", language=Language.PYTHON, paths=())
    hit = msgspec.json.encode((_AG_MATCH,))
    monkeypatch.setattr(code_rail, "fan_out", lambda *_a, **_kw: (Ok(receipt(("ast-grep", "run"), 0, stdout=hit, status=RailStatus.EMPTY)),))
    promoted = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), params))
    assert (promoted.verb, promoted.status) == ("search", RailStatus.OK)
    assert tuple((r.id, r.score) for r in promoted.results) == (("ast-grep:pkg/mod.py:1", 100),)
    assert promoted.artifacts[0].id == sha256(b"pkg/mod.py:1: alpha = 1 => let alpha = 1").hexdigest()[:12]
    panic = receipt(("ast-grep", "run"), 2, stdout=b"ast-grep panicked", status=RailStatus.FAILED)
    monkeypatch.setattr(code_rail, "fan_out", lambda *_a, **_kw: (Ok(panic),))
    failed = assert_ok(search(assay_root.settings, assay_root.scope(Claim.CODE), params))
    assert (failed.verb, failed.status, failed.artifacts) == ("search", RailStatus.FAILED, ())
    assert [(r.id, "panicked" in r.text) for r in failed.results] == [("ast-grep", True)]


# --- [COMPOSITION] ----------------------------------------------------------------------

# --- [LAW_COVERAGE]

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
register_law(search, "match_ids_carry_source_prefixes")
register_law(search, "project_rows_ag_shape_parity")
register_law(search, "project_rows_rg_shape_parity")
register_law(query, "project_rows_ts_shape_parity")
register_law(search, "content_search_forced_cap_results_note")
register_law(search, "structural_search_forced_cap_results_note")
register_law(query, "query_forced_cap_saturation_note")
register_law(search, "languages_none_resolves_all_code_languages")
register_law(search, "checks_empty_routed_returns_empty_tuple")
register_law(search, "dispatch_empty_checks_returns_empty_tuple")
register_law(search, "targets_empty_paths_returns_default_target")
register_law(search, "ts_language_tsx_key_resolves_tsx_grammar")
register_law(search, "ag_normalize_exit1_parseable_becomes_empty")
register_law(query, "ts_rows_produces_match_rows_and_listing")
register_law(query, "top_level_patterns_masked_depth_count")
register_law(query, "eq_needles_prefilter_byte_sets")
register_law(query, "ts_thunk_eq_prefilter_skip_and_admit")
register_law(query, "ts_thunk_receipt_argv_and_cap_fallback")
register_law(query, "ts_capture_full_byte_shape")
register_law(query, "ts_capture_error_row_identity")
register_law(search, "splice_argv_shapes")
register_law(search, "rg_status_exact_note_bytes")
register_law(search, "content_report_byte_shape")
register_law(search, "structural_report_promotion_and_defect_rows")
