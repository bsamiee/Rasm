"""Source decoding, traversal, and diagnostic ownership across checker runs."""

from textwrap import dedent
lazy from itertools import chain, repeat
lazy from os import devnull
lazy from pathlib import Path
lazy from sys import getrecursionlimit

from mypy.errorcodes import ASSIGNMENT, ErrorCode, SYNTAX
import pytest
from refurb.error import Error
from refurb.settings import load_settings
lazy import msgspec
lazy from mypy.nodes import MypyFile
lazy from refurb.checks.readability.no_copy_with_merge import ErrorInfo as MergeError
lazy from refurb.checks.readability.no_len_cmp import ErrorInfo as LengthError

from tools.refurb.__main__ import checked
from tools.refurb.defaults import ErrorRestatedDefaultError
lazy from tests.python.tools.refurb.conftest import Findings

# --- [OPERATIONS] -----------------------------------------------------------------------


@pytest.mark.parametrize("source", [b"\xef\xbb\xbfvalue = 1\n", b"# coding: latin-1\nvalue = 'caf\xe9'\n"])
def test_source_encoding(tmp_path: Path, source: bytes) -> None:
    """Python encoding declarations and UTF-8 BOMs retain the source body and its diagnostic locations."""
    path = tmp_path / "encoded.py"
    path.write_bytes(source + b"def read(*, width: int = 80) -> int:\n    return width\nread(width=80)\n")
    assert [error.line for error in checked(load_settings([str(path)])) if isinstance(error, ErrorRestatedDefaultError)] == [source.count(b"\n") + 3]


def test_encoded_source_reports_and_honors_ignore_comments(tmp_path: Path) -> None:
    """Decoded source supplies diagnostic lines and ignore comments without another file read."""
    path = tmp_path / "latin.py"
    source = b"# coding: latin-1\nname = 'caf\xe9'\ndef read(*, width: int = 80) -> int:\n    return width\nread(width=80)"
    path.write_bytes(source + b"\n")
    assert [error.line for error in checked(load_settings([str(path)])) if isinstance(error, ErrorRestatedDefaultError)] == [5]
    path.write_bytes(source + b"  # noqa: RASM102\n")
    assert checked(load_settings([str(path)])) == []


def test_template_interpolations_are_visited(findings: Findings) -> None:
    """Template values and format specifications retain diagnostics."""
    source = dedent("""
        def declared(*, width: int = 80) -> int:
            return width

        def render() -> object:
            return t"{declared(width=80):{declared(width=80)}}"
    """)
    assert [(error.line, error.column) for error in findings(ErrorRestatedDefaultError, source)] == [(6, 29), (6, 49)]


def test_findings_keep_source_filename(tmp_path: Path) -> None:
    """One visitor assigns each diagnostic to its source module."""
    paths = tuple(tmp_path / f"source_{index}.py" for index in range(2))
    for path in paths:
        path.write_text("def read(*, width: int = 80) -> int:\n    if len([width]) == 0:\n        return 0\n    return width\nread(width=80)\n", encoding="utf-8")
    found = checked(load_settings(list(map(str, paths))))
    assert [(error.filename, error.line) for error in found if isinstance(error, ErrorRestatedDefaultError)] == [(str(path), 5) for path in paths]
    assert [(error.filename, error.line) for error in found if isinstance(error, LengthError)] == [(str(path), 2) for path in paths]


def test_driver_visits_a_deep_expression_without_losing_its_tail(tmp_path: Path) -> None:
    """A left-deep expression at the interpreter recursion limit retains the diagnostic after its final operand."""
    path = tmp_path / "deep.py"
    expression = " + ".join(chain(repeat("value", getrecursionlimit()), ("declared(width=80)",)))
    path.write_text(f"def declared(*, width: int = 80) -> int:\n    return width\ndef folded(value: int) -> int:\n    return {expression}\n", encoding="utf-8")
    found = checked(load_settings([str(path)]))
    assert not any(isinstance(error, str) for error in found)
    assert [error.line for error in found if isinstance(error, ErrorRestatedDefaultError)] == [4]


def test_ignore_comments_follow_source_changes(tmp_path: Path) -> None:
    """Repeated runs observe changed ignore comments at the same path."""
    path = tmp_path / "changed.py"
    source = "def read(*, width: int = 80) -> int:\n    return width\nread(width=80)"
    path.write_text(f"{source}  # noqa: RASM102\n", encoding="utf-8")
    assert checked(load_settings([str(path)])) == []
    path.write_text(f"{source}\n", encoding="utf-8")
    assert [error.line for error in checked(load_settings([str(path)])) if isinstance(error, Error)] == [3]


def test_repeated_runs_keep_all_nested_merge_findings(tmp_path: Path) -> None:
    """Independent compilations report both union operands without retaining process-global native node identities."""
    path = tmp_path / "merge.py"
    path.write_text("combined = dict(a=1).copy() | {'b': 2} | dict(c=3).copy()\n", encoding="utf-8")
    for _ in range(2):
        assert [(error.line, error.column) for error in checked(load_settings([str(path)])) if isinstance(error, MergeError)] == [(1, 11), (1, 41)]


def test_inline_source_keeps_findings_ignores_and_debug() -> None:
    """Valid inline source retains native filename ownership, source ignores, and its debug tree."""
    source = "def read(*, width: int = 80) -> int:\n    return width\nread(width=80)\nread(width=80)  # noqa: RASM102\n"
    found = checked(load_settings(["--debug", "--", "--config-file", devnull, "--cache-dir", devnull, "-c", source]))
    assert [(type(error), error.filename, error.line) for error in found if isinstance(error, Error)] == [(ErrorRestatedDefaultError, "<string>", 3)]
    assert len(found) == 2
    assert any(isinstance(error, str) and MypyFile.__name__ in error for error in found)


@pytest.mark.parametrize(
    "source,code,lines",
    [pytest.param("def invalid(", SYNTAX, (1,), id=SYNTAX.code), pytest.param("\nfirst: int = 'invalid'\n" + "\n" * 7 + "second: int = 'invalid'", ASSIGNMENT, (2, 10), id=ASSIGNMENT.code)],
)
def test_native_compiler_diagnostics_are_returned(source: str, code: ErrorCode, lines: tuple[int, ...]) -> None:
    """Native syntax and ordinary type diagnostics retain compiler ordering through inline source arguments."""
    found = checked(load_settings(["--", "--config-file", devnull, "--cache-dir", devnull, "-c", source]))
    assert len(found) == len(lines)
    assert all(isinstance(error, str) and error.startswith(f"<string>:{line}:") and f"[{code.code}]" in error for error, line in zip(found, lines, strict=True))


def test_unpacked_comprehension_keeps_value_traversal(findings: Findings) -> None:
    """PEP 798 dictionary unpacking has no key expression while its value still carries diagnostics."""
    source = dedent("""
        def declared(*, width: int = 80) -> dict[str, int]:
            return {"width": width}

        def merged(count: int) -> dict[str, int]:
            return {**declared(width=80) for _ in range(count)}
    """)
    assert [error.line for error in findings(ErrorRestatedDefaultError, source)] == [6]


def test_debug_and_timing_keep_driver_analysis(tmp_path: Path) -> None:
    """Debug trees and native timing output share one cached compiler run."""
    path, output = tmp_path / "timed.py", tmp_path / "timing.json"
    path.write_text("value = 1\n", encoding="utf-8")
    found = checked(load_settings([str(path), "--debug", "--timing-stats", str(output)]))
    metadata = msgspec.json.decode(output.read_bytes(), type=dict[str, int | dict[str, int]])
    held = metadata["refurb_time_spent_checking_file_in_ms"]
    assert isinstance(held, dict)
    assert any(name.endswith(path.stem) for name in held)
    assert any(isinstance(item, str) and MypyFile.__name__ in item for item in found)
