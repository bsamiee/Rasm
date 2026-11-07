"""Runs native mypy diagnostics and Refurb checks over one typed build."""

import sys
lazy from collections.abc import Mapping, Sequence
lazy from dataclasses import replace
lazy from hashlib import sha256
lazy from importlib import metadata
lazy from os import devnull
lazy from pathlib import Path
lazy import re
lazy from time import perf_counter

import msgspec
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
lazy from refurb.loader import get_error_class, load_checks
lazy from refurb.main import format_errors, is_ignored_via_amend, main, output_timing_stats, sort_errors
lazy from refurb.settings import load_settings, Settings

from tools.refurb import defaults
from tools.refurb.rewrites import RewriteVisitor

# --- [TYPES] ----------------------------------------------------------------------------

type Finding = tuple[str, int, int, str, int | None, int | None]

# --- [MODELS] ---------------------------------------------------------------------------


class Held(msgspec.Struct, frozen=True, kw_only=True):
    """Findings of each module one run checked, under the key of the settings and checker sources that produced them."""

    key: str
    modules: Mapping[str, tuple[str, tuple[Finding, ...]]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def checked(settings: Settings) -> Sequence[Error | str]:
    """Runs mypy diagnostics and Refurb checks over typed source trees, replaying the held findings of each unchanged module mypy loads fresh from its cache."""
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
    options.cache_dir = devnull if options.shadow_file is not None else str(Path(options.cache_dir, Path(__file__).parent.name))
    store = Path(options.cache_dir, ".".join(map(str, options.python_version)), "findings.msgpack") if options.incremental and options.cache_dir != devnull else None
    stated = (*sys.argv[1:], metadata.version("refurb"))
    key = sha256(b"\0".join((*map(str.encode, stated), *map(Path.read_bytes, sorted({*Path(__file__).parent.glob("*.py"), *map(Path, filter(None, [options.config_file]))}))))).hexdigest()
    raw = catch(Path.read_bytes, exception=FileNotFoundError)(store).default_value(b"") if store is not None else b""
    match catch(msgspec.msgpack.decode, exception=msgspec.DecodeError)(raw, type=Held):
        case Result(tag="ok", ok=Held(key=held_key, modules=modules)) if held_key == key:
            held = modules
        case _:
            held = {}
    if store is not None:
        store.unlink(missing_ok=True)
    read = (
        [
            BuildSource(source.path, source.module, content(source.path), source.base_dir)
            if source.path is not None and held.get(source.module, ("", ()))[0] != filesystem.hash_digest(source.path)
            else source
            for source in sources
        ]
        if store is not None
        else sources
    )
    started = perf_counter()
    match catch(build, exception=CompileError)(read, options=options, fscache=filesystem):
        case Result(tag="ok", ok=result):
            compiled = perf_counter() - started
            if stale := {source: tree for source in read if (tree := result.files.get(source.module)) is not None and not tree.is_cache_skeleton}:
                types.BUILTINS_MYPY_FILE = result.files["builtins"]
            visitor = RewriteVisitor(checks, settings, default_check=defaults.configured(result.types.get))
            classes = {str(ErrorCode.from_error(error)): error for error in {get_error_class(sys.modules[check.__module__]) for found in checks.values() for check in found} if error is not None}
            noqa = re.compile(r"# noqa(: [^'\"]*)?$")
            findings: dict[str, tuple[Finding, ...]] = {source.module: held[source.module][1] for source in read if source not in stale}
            timings: dict[str, int] = {}
            for source, tree in stale.items():
                state = result.graph[source.module]
                visitor.errors.clear()
                started = perf_counter()
                visitor.accept(tree)
                timings[source.module] = int((perf_counter() - started) * 1000)
                lines = (source.text if source.text is not None else content(result.manager.maybe_swap_for_shadow_path(state.xpath))).splitlines() if visitor.errors else ()
                for finding in visitor.errors:
                    finding.filename = state.xpath
                findings[source.module] = tuple(
                    (str(ErrorCode.from_error(type(finding))), finding.line, finding.column, finding.msg, finding.line_end, finding.column_end)
                    for finding in visitor.errors
                    if not is_ignored_via_amend(finding, settings)
                    and not (
                        (comment := noqa.search(lines[finding.line - 1].rstrip())) is not None
                        and (comment[1] is None or str(ErrorCode.from_error(type(finding))) in comment[1][2:].replace(",", " ").split(" "))
                    )
                )
            if store is not None:
                store.write_bytes(
                    msgspec.msgpack.encode(Held(key=key, modules={source.module: (filesystem.hash_digest(source.path), findings[source.module]) for source in read if source.path is not None}))
                )
            output_timing_stats(settings, compiled, settings.timing_stats, timings)
            errors: list[Error | str] = [
                *((str(tree) for tree in stale.values()) if settings.debug else ()),
                *(classes[code](line, column, msg, source.path, line_end, column_end) for source in read for code, line, column, msg, line_end, column_end in findings[source.module]),
            ]
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
