"""Runs native mypy diagnostics and Refurb checks over one typed build."""

import sys
lazy from collections.abc import Mapping, Sequence
lazy from dataclasses import replace
lazy from hashlib import sha256
lazy from importlib import metadata
lazy import os
lazy from pathlib import Path
lazy import re
lazy from time import perf_counter

import msgspec
from mypy.nodes import Expression, TemplateStrExpr, TypeFormExpr
from refurb.visitor import TraverserVisitor
from refurb.visitor.traverser import accept
lazy from expression import Result
lazy from expression.extra.result import catch
lazy from mypy.build import build, BuildResult
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

from tools.refurb.rewrites import RewriteVisitor

# --- [TYPES] ----------------------------------------------------------------------------

type Finding = tuple[str, int, int, str, int | None, int | None]

# --- [MODELS] ---------------------------------------------------------------------------


class Entry(msgspec.Struct, frozen=True, kw_only=True):
    """Findings of one checked module, the source modules whose class bodies its checks read, and the digest of its dependencies' mypy interfaces."""

    reads: tuple[str, ...] = ()
    interfaces: str = ""
    findings: tuple[Finding, ...] = ()


class Held(msgspec.Struct, frozen=True, kw_only=True):
    """Entry of each checked module under the key of the settings and checker sources that produced them, beside the digest of each source."""

    key: str
    digests: Mapping[str, str]
    modules: Mapping[str, Entry]


# --- [OPERATIONS] -----------------------------------------------------------------------


def checked(settings: Settings) -> Sequence[Error | str]:
    """Runs mypy diagnostics and Refurb checks over typed source trees, replaying the held findings of each module whose source and the class bodies its checks read are unchanged."""
    filesystem = FileSystemCache()

    def content(path: str) -> str:
        return decode_python_encoding(filesystem.read(path))

    version = ["--python-version", ".".join(map(str, settings.python_version))] if settings.python_version is not None else []
    sources, options = process_options([*settings.files, *version, *settings.mypy_args], fscache=filesystem)
    settings = replace(settings, python_version=options.python_version)
    checks = load_checks(settings)
    options.preserve_asts = True
    options.export_types = True
    options.num_workers = 0
    if settings.timing_stats is not None:
        options.timing_stats = str(settings.timing_stats)
    options.cache_dir = os.devnull if options.shadow_file is not None else str(Path(options.cache_dir, Path(__file__).parent.name))
    store = Path(options.cache_dir, ".".join(map(str, options.python_version)), "findings.msgpack") if options.incremental and options.cache_dir != os.devnull else None
    stated = (*sys.argv[1:], metadata.version("refurb"))
    key = sha256(b"\0".join((*map(str.encode, stated), *map(Path.read_bytes, sorted({*Path(__file__).parent.glob("*.py"), *map(Path, filter(None, [options.config_file]))}))))).hexdigest()
    digests = {source.module: filesystem.hash_digest(source.path) for source in sources if source.path is not None}
    raw = catch(Path.read_bytes, exception=FileNotFoundError)(store).default_value(b"") if store is not None else b""
    stored = (
        catch(msgspec.msgpack.Decoder(Held).decode, exception=msgspec.DecodeError)(raw)
        .to_option()
        .filter(lambda found: found.key == key)
        .default_value(Held(key=key, digests=frozendict(), modules=frozendict()))
    )
    held = {module: entry for module, entry in stored.modules.items() if all(stored.digests.get(name) == digests.get(name) for name in (module, *entry.reads))}
    edited = frozenset(digests.keys() - held.keys())
    classes = {str(ErrorCode.from_error(error)): error for error in {get_error_class(sys.modules[check.__module__]) for found in checks.values() for check in found} if error is not None}
    noqa = re.compile(r"# noqa(: [^'\"]*)?$")
    timings: dict[str, int] = {}

    def read(modules: frozenset[str], /) -> frozenset[str]:
        return frozenset(name for module in modules for name in stored.modules.get(module, Entry()).reads if name in digests)

    def built(forced: frozenset[str], /) -> Result[BuildResult, CompileError]:
        return catch(build, exception=CompileError)(
            [BuildSource(source.path, source.module, content(source.path), source.base_dir) if source.path is not None and source.module in forced else source for source in sources],
            options=options,
            fscache=filesystem,
        )

    def interfaces(result: BuildResult, module: str, /) -> str:
        return sha256(b"\0".join(name.encode() + result.graph[name].interface_hash for name in sorted(result.graph[module].dependencies) if name in result.graph)).hexdigest()

    def parsed(result: BuildResult, /) -> frozenset[str]:
        return frozenset(module for module in digests if (tree := result.files.get(module)) is not None and not tree.is_cache_skeleton)

    def visited(result: BuildResult, modules: frozenset[str], /) -> dict[str, Entry]:
        if modules:
            types.BUILTINS_MYPY_FILE = result.files["builtins"]
        visitor = RewriteVisitor(checks, settings, types=result.types, sources=frozenset(digests))
        entries: dict[str, Entry] = {}
        for module in modules:
            state = result.graph[module]
            visitor.errors.clear()
            started = perf_counter()
            visitor.accept(result.files[module])
            timings[module] = int((perf_counter() - started) * 1000)
            lines = content(result.manager.maybe_swap_for_shadow_path(state.xpath)).splitlines() if visitor.errors else ()
            for finding in visitor.errors:
                finding.filename = state.xpath
            entries[module] = Entry(
                reads=tuple(sorted(visitor.reads.taken() - {module})),
                interfaces=interfaces(result, module),
                findings=tuple(
                    (str(ErrorCode.from_error(type(finding))), finding.line, finding.column, finding.msg, finding.line_end, finding.column_end)
                    for finding in visitor.errors
                    if not is_ignored_via_amend(finding, settings)
                    and not (
                        (comment := noqa.search(lines[finding.line - 1].rstrip())) is not None
                        and (comment[1] is None or str(ErrorCode.from_error(type(finding))) in comment[1][2:].replace(",", " ").split(" "))
                    )
                ),
            )
        return entries

    def inspected(result: BuildResult, chosen: frozenset[str], targets: frozenset[str], /) -> Result[tuple[BuildResult, dict[str, Entry]], CompileError]:
        outcome = (built(forced | chosen | targets) if (chosen | targets) - parsed(result) else Result[BuildResult, CompileError].Ok(result)).map(lambda found: (found, visited(found, chosen)))
        match outcome:
            case Result(tag="ok", ok=(found, entries)) if missing := frozenset(*entry.reads for entry in entries.values()) - parsed(found):
                return inspected(result, chosen, targets | missing)
            case _:
                return outcome

    started = perf_counter()
    forced = edited | read(edited)
    match built(forced).bind(
        lambda result: inspected(result, chosen := edited | frozenset(module for module in digests if interfaces(result, module) != stored.modules.get(module, Entry()).interfaces), read(chosen))
    ):
        case Result(tag="ok", ok=(result, entries)):
            elapsed = perf_counter() - started - sum(timings.values()) / 1000
            modules = held | entries
            if store is not None:
                written = store.with_name(f"{store.name}.{os.getpid()}")
                written.write_bytes(msgspec.msgpack.encode(Held(key=key, digests=digests, modules={module: modules[module] for module in digests})))
                written.replace(store)
            output_timing_stats(settings, elapsed, settings.timing_stats, timings)
            errors: list[Error | str] = [
                *((str(result.files[module]) for module in entries) if settings.debug else ()),
                *(classes[code](line, column, msg, source.path, line_end, column_end) for source in sources for code, line, column, msg, line_end, column_end in modules[source.module].findings),
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
