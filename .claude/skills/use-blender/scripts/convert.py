# mypy: disable-error-code="attr-defined, union-attr"
# ty: ignore[unresolved-attribute, invalid-assignment]
"""Convert interchange files through Blender importers and one exporter in a headless process, one empty factory scene with a world per file."""

from collections.abc import Mapping
import contextlib
from pathlib import Path
import shutil
from types import MappingProxyType
from typing import TextIO

import attrs
import bpy
import numpy as np
from results import artifacts, JSON
from rna import DIGITS, operators
from scene import bounds

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Converted | NoImporter | AmbiguousImporter | SharedStem | Failed

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Converted:
    """Source with its importer, imported object count, mesh objects holding no vertices the exporter omits, world extent in meters, and the files the exporter wrote."""

    source: str
    importer: str
    objects: int
    empty: tuple[str, ...]
    extent: tuple[float, float, float] | None
    outputs: tuple[str, ...]


@attrs.frozen
class Batch:
    """Report file, the log holding every operator's output under a line naming its source, and one outcome per source in input order."""

    report: str
    log: str
    files: tuple[Outcome, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class NoImporter:
    """Source that no reader's file filter names, with no importer `options` names left for it."""

    source: str
    suffix: str


@attrs.frozen
class AmbiguousImporter:
    """Source more than one importer reads with no named or lone C importer among them, an importer named in `options` picking one."""

    source: str
    candidates: tuple[str, ...]


@attrs.frozen
class SharedStem:
    """Sources sharing one stem and its output file."""

    source: str
    sources: tuple[str, ...]


@attrs.frozen
class Failed:
    """Operator that raised or returned without `FINISHED` on a source, with its message or return status."""

    source: str
    operator: str
    error: str


@attrs.frozen
class UnknownOperator:
    """Exporter or option ids naming no operator of their role."""

    operators: tuple[str, ...]


@attrs.frozen
class UnknownOptions:
    """Options as `<operator>.<property>` that the operator does not declare."""

    options: tuple[str, ...]


@attrs.frozen
class LiveSession:
    """Live session document a conversion's empty scene per file replaces."""

    document: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def convert(name: str, exporter: str, sources: tuple[str, ...], options: Mapping[str, Mapping[str, object]] = MappingProxyType({})) -> Batch | UnknownOperator | UnknownOptions | LiveSession:
    """Batch of each source imported into an empty factory scene and exported to a cleared `.artifacts/blender/convert/<name>/` as `<stem><ext>`, `options` holding keyword arguments per operator id and naming importers."""
    if not bpy.app.background:
        return LiveSession(bpy.data.filepath)
    functions = {op.idname_py(): op for op in operators()}
    properties = {key: op.get_rna_type().properties for key, op in functions.items()}
    globs = {key: patterns for key, props in properties.items() if "filter_glob" in props and (patterns := tuple(filter(None, props["filter_glob"].default.split(";"))))}
    writers = {key for key in globs if "check_existing" in (props := properties[key]) and props["check_existing"].default}
    importers = {key: patterns for key, patterns in globs.items() if key not in writers}
    if unknown := tuple(op for op, table in ((exporter, writers), *((op, importers.keys() | writers) for op in options)) if op not in table):
        return UnknownOperator(unknown)
    if undeclared := tuple(f"{op}.{key}" for op, values in options.items() for key in values if key not in properties[op]):
        return UnknownOptions(undeclared)
    ids = {op.idname(): key for key, op in functions.items()} | {key: key for key in functions}
    classes = {key: bpy.types.Operator.bl_rna_get_subclass_py(op.idname()) for key, op in functions.items()}
    native = {key for key in importers if classes[key] is None}
    handled = {
        ids[op]: tuple(f"*{suffix}" for suffix in handler.bl_file_extensions.split(";"))
        for handler in bpy.types.FileHandler.__subclasses__()
        if (op := handler.bl_import_operator) in ids and (classes[ids[op]] is None or hasattr(classes[ids[op]], "execute"))
    }
    named = tuple(op for op in options if op in importers)
    readers = {key: importers[key] for key in (*native, *named)} | handled
    shutil.rmtree(artifacts("convert", name))
    out, paths = artifacts("convert", name), tuple(Path(s).resolve() for s in sources)
    matches = {path: tuple(sorted(op for op, patterns in readers.items() if any(path.match(p, case_sensitive=False) for p in patterns))) for path in paths}
    spare = tuple(op for op in named if not any(op in found for found in matches.values()))

    def resolve(source: Path) -> str | NoImporter | AmbiguousImporter:
        """Named importer whose filter matches the file, or the named importer no file's filter matches when none does, else the file's lone C reader, else its lone reader."""
        candidates = matches[source]
        chosen = tuple(op for op in named if op in candidates) if candidates else spare
        match chosen, tuple(op for op in candidates if op in native), candidates:
            case ((one,), _, _) | ((), (one,), _) | ((), (), (one,)):
                return one
            case (), (), ():
                return NoImporter(str(source), source.suffix)
            case _:
                return AmbiguousImporter(str(source), candidates or spare)

    def run(op: str, source: Path, **arguments: object) -> Failed | None:
        """Failure of one operator call, `None` when it finishes."""
        try:
            status = functions[op](**arguments)
        except (RuntimeError, TypeError) as error:
            return Failed(str(source), op, str(error).strip())
        return None if "FINISHED" in status else Failed(str(source), op, " ".join(sorted(status)))

    def one(source: Path, sink: TextIO) -> Outcome:
        """Outcome of importing one source into an empty scene with a world and exporting it, its operators' output in the sink under a line naming it."""
        if not isinstance(chosen := resolve(source), str):
            return chosen
        if len(shared := tuple(str(p) for p in paths if p.stem == source.stem)) > 1:
            return SharedStem(str(source), shared)
        sink.write(f"--- {source}\n")
        bpy.ops.wm.read_homefile(use_empty=True, use_factory_startup=True)
        bpy.context.scene.world = bpy.data.worlds.new("World")
        location = {"filepath": str(source), "directory": str(source.parent), "files": [{"name": source.name}]}
        if failed := run(chosen, source, **options.get(chosen, {}), **{key: value for key, value in location.items() if key in properties[chosen]}):
            return failed
        corners = tuple(bounds(bpy.context.evaluated_depsgraph_get(), drawn=False).values())
        extent = tuple(np.ptp(np.concatenate(corners), axis=0).round(DIGITS).tolist()) if corners else None
        empty = tuple(sorted(o.name for o in bpy.data.objects if isinstance(o.data, bpy.types.Mesh) and not o.data.vertices))
        before = set(out.iterdir())
        if failed := run(exporter, source, **options.get(exporter, {}), filepath=str(out / f"{source.stem}{globs[exporter][0].removeprefix('*')}")):
            return failed
        return Converted(str(source), chosen, len(bpy.data.objects), empty, extent, tuple(sorted(map(str, set(out.iterdir()) - before))))

    report, log = out / "convert.json", out / "convert.log"
    with log.open("w", encoding="utf-8") as sink, contextlib.redirect_stdout(sink), contextlib.redirect_stderr(sink):
        batch = Batch(str(report), str(log), tuple(one(path, sink) for path in paths))
    report.write_text(JSON.dumps(batch, indent=1), encoding="utf-8")
    return batch


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AmbiguousImporter", "Batch", "Converted", "Failed", "LiveSession", "NoImporter", "Outcome", "SharedStem", "UnknownOperator", "UnknownOptions", "convert"]
