"""Run refurb checks over trees mypy re-checks against the cache `typecheck:mypy` writes."""

import sys
lazy from collections.abc import Sequence
lazy import copy
lazy from pathlib import Path

lazy from expression import Result
lazy from expression.extra.result import catch
lazy from mypy.build import build
lazy from mypy.errors import CompileError
lazy from mypy.main import process_options
lazy from mypy.modulefinder import BuildSource
lazy from refurb import types
lazy from refurb.error import Error
lazy from refurb.loader import load_checks
lazy from refurb.main import format_errors, main, should_ignore_error, sort_errors
lazy from refurb.settings import load_settings, Settings
lazy from refurb.visitor import RefurbVisitor

# --- [OPERATIONS] -----------------------------------------------------------------------


def refurbished(settings: Settings) -> Sequence[Error | str]:
    """Type-checks each source module from its text with cached imports and runs checks over its typed tree."""
    sources, options = process_options([*settings.files, *settings.mypy_args])
    options.preserve_asts = True
    options.python_version = settings.get_python_version()
    read = [BuildSource(source.path, source.module, Path(source.path).read_text(encoding="utf-8"), source.base_dir) for source in sources if source.path is not None]
    match catch(build, exception=CompileError)(read, options=options):
        case Result(tag="ok", ok=result):
            types.BUILTINS_MYPY_FILE = result.files["builtins"]
            checks = load_checks(settings)

            def visited(source: BuildSource) -> list[Error]:
                visitor = RefurbVisitor(checks, settings)
                visitor.accept(result.files[source.module])
                return [copy.replace(error, filename=source.path) for error in visitor.errors]

            return sorted((error for source in read for error in visited(source) if not should_ignore_error(error, settings)), key=lambda error: sort_errors(error, settings))
        case Result(error=error):
            return error.messages


# --- [COMPOSITION] ----------------------------------------------------------------------


def _status() -> int:
    """Returns the exit status of a findings run over configured mypy sources, passing every other run to refurb."""
    match catch(load_settings, exception=ValueError)(sys.argv[1:]):
        case Result(tag="ok", ok=Settings(help=False, version=False, explain=None, generate=False, debug=False, timing_stats=None) as settings):
            errors = refurbished(settings)
            if output := format_errors(errors, settings):
                sys.stdout.write(f"{output}\n")
            return 1 if errors else 0
        case _:
            return main(sys.argv[1:])


if __name__ == "__main__":
    sys.exit(_status())
