"""Code rail tree-sitter and artifact laws."""

from typing import TYPE_CHECKING

from tools.assay.composition.catalog import CAPTURES
from tools.assay.core.model import Check, Claim, Input, Language, Mode, Runner, Tool
from tools.assay.rails.code import _artifact, _ts_thunk  # noqa: PLC2701


if TYPE_CHECKING:
    from tests.tools.assay.conftest import AssayHarness


def test_tree_sitter_python_query_preserves_capture_metadata(assay_root: AssayHarness) -> None:
    """Python query captures include coordinates and match ordinal metadata."""
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    thunk = _ts_thunk("(function_definition name: (identifier) @name)", Language.PYTHON, assay_root.root, limit=10)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.QUERY), paths=("pkg/mod.py",)))
    captures = CAPTURES.decode(done.stdout)

    assert done.status.value == "ok"
    assert captures[0].text == "alpha"
    assert captures[0].line == 1
    assert captures[0].column > 0
    assert captures[0].ordinal == 0


def test_tree_sitter_tsx_uses_tsx_grammar(assay_root: AssayHarness) -> None:
    """TSX files parse with the TSX grammar instead of the TypeScript grammar."""
    assay_root.write("src/view.tsx", "export const View = () => <div />;\n")
    thunk = _ts_thunk("(jsx_self_closing_element) @jsx", Language.TYPESCRIPT, assay_root.root, limit=10)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.TYPESCRIPT, Claim.CODE, mode=Mode.QUERY), paths=("src/view.tsx",)))
    captures = CAPTURES.decode(done.stdout)

    assert done.status.value == "ok"
    assert captures[0].name == "jsx"
    assert captures[0].file == "src/view.tsx"


def test_tree_sitter_parse_error_becomes_capture_row(assay_root: AssayHarness) -> None:
    """Parser errors are visible in Capture rows and fail the in-process receipt."""
    assay_root.write("pkg/bad.py", "def broken(:\n")
    thunk = _ts_thunk("(function_definition name: (identifier) @name)", Language.PYTHON, assay_root.root, limit=10)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.QUERY), paths=("pkg/bad.py",)))
    captures = CAPTURES.decode(done.stdout)

    assert done.returncode == 1
    assert any(cap.parse_error for cap in captures)


def test_tree_sitter_query_error_becomes_capture_row(assay_root: AssayHarness) -> None:
    """Invalid query syntax is returned as a failed Capture row, not an escaped exception."""
    assay_root.write("pkg/mod.py", "def alpha():\n    return 1\n")
    thunk = _ts_thunk("(", Language.PYTHON, assay_root.root, limit=10)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.QUERY), paths=("pkg/mod.py",)))
    captures = CAPTURES.decode(done.stdout)

    assert done.returncode == 1
    assert captures[0].name == "query_error"
    assert captures[0].parse_error


def test_tree_sitter_match_limit_marks_truncation(assay_root: AssayHarness) -> None:
    """Tree-sitter query caps are applied at cursor level and surfaced on Capture rows."""
    assay_root.write("pkg/mod.py", "def a():\n    pass\ndef b():\n    pass\n")
    thunk = _ts_thunk("(function_definition name: (identifier) @name)", Language.PYTHON, assay_root.root, limit=1)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.QUERY), paths=("pkg/mod.py",)))
    captures = CAPTURES.decode(done.stdout)

    assert len(captures) == 1
    assert captures[0].truncated


def test_tree_sitter_any_of_prefilter_has_no_false_negative(assay_root: AssayHarness) -> None:
    """Grouped #any-of? prefilter keeps files matching any literal in the predicate group."""
    assay_root.write("pkg/mod.py", "def beta():\n    return 1\n")
    query = '(function_definition name: (identifier) @name (#any-of? @name "alpha" "beta"))'
    thunk = _ts_thunk(query, Language.PYTHON, assay_root.root, limit=10)
    done = thunk(Check(tool=Tool("ts", Runner.INPROC, (), Input.NONE, Language.PYTHON, Claim.CODE, mode=Mode.QUERY), paths=("pkg/mod.py",)))
    captures = CAPTURES.decode(done.stdout)

    assert done.status.value == "ok"
    assert captures[0].text == "beta"


def test_code_artifact_uses_assay_store(assay_root: AssayHarness) -> None:
    """Code listings are persisted by ArtifactStore under the assay artifact root."""
    artifact = _artifact(assay_root.scope(Claim.CODE), "search", "a.py:1: hit", assay_root.settings)

    assert ".artifacts/assay/code/search/" in artifact.path
    assert artifact.lines == 1
