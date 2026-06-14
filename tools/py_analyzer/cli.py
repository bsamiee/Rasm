"""Command-line boundary for the Python analyzer."""

import argparse
from pathlib import Path
import sys
from typing import assert_never, TYPE_CHECKING

import msgspec

from tools.py_analyzer.analyzer import analyze_paths
from tools.py_analyzer.rules import OutputFormat


if TYPE_CHECKING:
    from collections.abc import Sequence

    from tools.py_analyzer.rules import Diagnostic


# --- [OPERATIONS] -----------------------------------------------------------------------


def emit(diagnostics: Sequence[Diagnostic], root: Path, output_format: OutputFormat) -> None:
    """Emit diagnostics using the selected output contract."""
    resolved_root = root.resolve()
    match output_format:
        case OutputFormat.text:
            sys.stdout.write(
                "".join(
                    f"{diagnostic.relative_path(resolved_root)}:{diagnostic.line}:{diagnostic.column}: "
                    f"{diagnostic.rule_id.value} {diagnostic.title}: {diagnostic.message}\n"
                    for diagnostic in diagnostics
                )
            )
        case OutputFormat.json:
            sys.stdout.buffer.write(msgspec.json.encode([diagnostic.as_json(resolved_root) for diagnostic in diagnostics]) + b"\n")
        case OutputFormat.github:

            def escape(value: str, *, is_property: bool = False) -> str:
                data = value.replace("%", "%25").replace("\r", "%0D").replace("\n", "%0A")
                return data.replace(":", "%3A").replace(",", "%2C") if is_property else data

            sys.stdout.write(
                "".join(
                    f"::error file={escape(diagnostic.relative_path(resolved_root), is_property=True)},"
                    f"line={diagnostic.line},col={diagnostic.column},"
                    f"title={escape(f'{diagnostic.rule_id.value} {diagnostic.title}', is_property=True)}::"
                    f"{escape(diagnostic.message)}\n"
                    for diagnostic in diagnostics
                )
            )
        case unreachable:
            assert_never(unreachable)


def _parser() -> argparse.ArgumentParser:
    root_parser = argparse.ArgumentParser(prog="py-analyzer")
    subcommands = root_parser.add_subparsers(dest="command", required=True)
    check = subcommands.add_parser("check")
    check.add_argument("paths", nargs="*")
    check.add_argument("--root", default=".")
    check.add_argument("--format", choices=tuple(OutputFormat), default=OutputFormat.text.value)
    return root_parser


def _invocation_errors(root: Path, paths: Sequence[Path]) -> tuple[str, ...]:
    root_errors = (
        f"py-analyzer: --root does not exist: {root}" if not root.exists() else "",
        f"py-analyzer: --root is not a directory: {root}" if root.exists() and not root.is_dir() else "",
    )
    path_errors = tuple(f"py-analyzer: path does not exist: {path}" for path in (_anchor(root, path) for path in paths) if not path.exists())
    return tuple(error for error in (*root_errors, *path_errors) if error)


def _anchor(root: Path, path: Path) -> Path:
    return path.resolve() if path.is_absolute() else (root / path).resolve()


# --- [ENTRY] ----------------------------------------------------------------------------


def main(argv: Sequence[str] | None = None) -> int:
    """Run the analyzer command.

    Returns:
        Process exit code for invocation validation and diagnostics.
    """
    args = _parser().parse_args(argv)
    match args.command:
        case "check":
            root = Path(args.root).resolve()
            paths = tuple(Path(path) for path in args.paths)
            errors = _invocation_errors(root, paths)
            match errors:
                case ():
                    diagnostics = analyze_paths(root, paths)
                    emit(diagnostics, root, OutputFormat(args.format))
                    return int(bool(diagnostics))
                case _:
                    sys.stderr.write("".join(f"{error}\n" for error in errors))
                    return 2
        case _:
            return 2


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["emit", "main"]
