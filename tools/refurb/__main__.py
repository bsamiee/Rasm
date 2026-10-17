"""Run refurb checks over trees mypy re-checks against the cache `typecheck:mypy` writes."""

from collections.abc import Sequence
from dataclasses import replace
from pathlib import Path
import sys

from mypy.build import build, State
from mypy.errors import CompileError
from mypy.main import process_options
from mypy.modulefinder import BuildSource
from mypy.nodes import MypyFile
from refurb import types
from refurb.error import Error
from refurb.loader import load_checks
from refurb.main import format_errors, main as refurb, should_ignore_error, sort_errors
from refurb.settings import load_settings, Settings
from refurb.types import Checks
from refurb.visitor import RefurbVisitor

# --- [OPERATIONS] -----------------------------------------------------------------------


def _tree(state: State) -> MypyFile:
    """Return the tree mypy holds for a module it checked or loaded from cache."""
    match state.tree:
        case MypyFile() as tree:
            return tree
        case None:
            raise LookupError(state.id)


def _visited(tree: MypyFile, path: str, checks: Checks, settings: Settings) -> list[Error]:
    """Return the findings refurb's visitor reports over one tree."""
    visitor = RefurbVisitor(checks, settings)
    visitor.accept(tree)
    return [replace(error, filename=path) for error in visitor.errors]


def _refurbished(settings: Settings) -> Sequence[Error | str]:
    """Check each source module from its text, keeping its typed tree, while its imports load from cache, then visit it."""
    sources, options = process_options([*settings.files, *settings.mypy_args])
    options.preserve_asts = True
    options.python_version = settings.get_python_version()
    read = [
        BuildSource(source.path, source.module, Path(source.path).read_text(encoding="utf-8"), source.base_dir) if source.path is not None and source.path.endswith(".py") else source
        for source in sources
    ]
    try:
        result = build(read, options=options)
    except CompileError as error:
        return error.messages
    checks = load_checks(settings)
    types.BUILTINS_MYPY_FILE = _tree(result.graph["builtins"])
    errors = [error for source in read if source.text is not None and source.path is not None for error in _visited(_tree(result.graph[source.module]), source.path, checks, settings)]
    return sorted((error for error in errors if not should_ignore_error(error, settings)), key=lambda error: sort_errors(error, settings))


# --- [COMPOSITION] ----------------------------------------------------------------------


def main() -> int:
    """Print refurb findings over the configured mypy sources, handing usage, version, explanation, and generation to refurb, and return the exit status."""
    arguments = sys.argv[1:]
    try:
        settings = load_settings(arguments)
    except ValueError:
        return refurb(arguments)
    match settings:
        case Settings(help=False, version=False, explain=None, generate=False):
            errors = _refurbished(settings)
            if output := format_errors(errors, settings):
                sys.stdout.write(f"{output}\n")
            return 1 if errors else 0
        case _:
            return refurb(arguments)


if __name__ == "__main__":
    sys.exit(main())


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["main"]
