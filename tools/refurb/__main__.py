"""Runs native mypy diagnostics and Refurb checks over one typed build."""

import sys
lazy from collections.abc import Sequence
lazy from dataclasses import replace
lazy from os import devnull
lazy import re
lazy from time import perf_counter

from mypy.nodes import Expression, MypyFile, TemplateStrExpr, TypeFormExpr
from refurb.visitor import TraverserVisitor
from refurb.visitor.traverser import accept
lazy from expression import Result
lazy from expression.extra.result import catch
lazy from mypy.build import build
lazy from mypy.errors import CompileError
lazy from mypy.fscache import FileSystemCache
lazy from mypy.main import process_options
lazy from mypy.modulefinder import BuildSource
lazy from mypy.util import decode_python_encoding
lazy from refurb import types
lazy from refurb.error import Error, ErrorCode
lazy from refurb.loader import load_checks
lazy from refurb.main import format_errors, is_ignored_via_amend, main, output_timing_stats, sort_errors
lazy from refurb.settings import load_settings, Settings

from tools.refurb import defaults
from tools.refurb.rewrites import RewriteVisitor

# --- [OPERATIONS] -----------------------------------------------------------------------


def checked(settings: Settings) -> Sequence[Error | str]:
    """Runs mypy diagnostics and Refurb checks over fresh typed source trees."""
    filesystem = FileSystemCache()

    def content(path: str) -> str:
        return decode_python_encoding(filesystem.read(path))

    version = ["--python-version", ".".join(map(str, settings.python_version))] if settings.python_version is not None else []
    sources, options = process_options([*settings.files, *version, *settings.mypy_args], fscache=filesystem)
    settings = replace(settings, python_version=options.python_version)
    checks = load_checks(settings)
    options.preserve_asts = True
    options.export_types = defaults.check in checks[MypyFile]
    options.num_workers = 0
    if settings.timing_stats is not None:
        options.timing_stats = str(settings.timing_stats)
    if options.shadow_file is not None:
        options.cache_dir = devnull
    read = (
        [BuildSource(source.path, source.module, content(source.path), source.base_dir) if source.path is not None else source for source in sources]
        if options.incremental and options.cache_dir != devnull
        else sources
    )
    started = perf_counter()
    match catch(build, exception=CompileError)(read, options=options, fscache=filesystem):
        case Result(tag="ok", ok=result):
            compiled = perf_counter() - started
            types.BUILTINS_MYPY_FILE = result.files["builtins"]
            visitor = RewriteVisitor(checks, settings, default_check=defaults.configured(result.types.get))
            noqa = re.compile(r"# noqa(: [^'\"]*)?$")
            errors: list[Error | str] = []
            timings: dict[str, int] = {}
            for source in read:
                state = result.graph[source.module]
                tree = result.files[source.module]
                errors.extend((str(tree),) if settings.debug else ())
                visitor.errors.clear()
                started = perf_counter()
                visitor.accept(tree)
                timings[source.module] = int((perf_counter() - started) * 1000)
                lines = (source.text if source.text is not None else content(result.manager.maybe_swap_for_shadow_path(state.xpath))).splitlines() if visitor.errors else ()
                for finding in visitor.errors:
                    finding.filename = state.xpath
                errors.extend(
                    finding
                    for finding in visitor.errors
                    if not is_ignored_via_amend(finding, settings)
                    and not (
                        (comment := noqa.search(lines[finding.line - 1].rstrip())) is not None
                        and (comment[1] is None or str(ErrorCode.from_error(type(finding))) in comment[1][2:].replace(",", " ").split(" "))
                    )
                )
            output_timing_stats(settings, compiled, settings.timing_stats, timings)
            return [*result.errors, *sorted(errors, key=lambda error: sort_errors(error, settings))]
        case Result(error=error):
            return error.messages


# --- [COMPOSITION] ----------------------------------------------------------------------


@accept.register
def _template(node: TemplateStrExpr, visitor: TraverserVisitor) -> None:
    for item in node.items:
        match item:
            case tuple():
                value, _, _, spec = item
                visitor.accept(value)
                if spec is not None:
                    visitor.accept(spec)
            case Expression():
                visitor.accept(item)


@accept.register
def _type_form(node: TypeFormExpr, visitor: TraverserVisitor) -> None:
    pass


def _status() -> int:
    """Returns the exit status of a findings run over configured mypy sources, passing every other run to refurb."""
    match catch(load_settings, exception=ValueError)(sys.argv[1:]):
        case Result(tag="ok", ok=Settings(help=False, version=False, explain=None, generate=False) as settings):
            errors = checked(settings)
            if output := format_errors(errors, settings):
                sys.stdout.write(f"{output}\n")
            return 1 if errors else 0
        case _:
            return main(sys.argv[1:])


if __name__ == "__main__":
    sys.exit(_status())
