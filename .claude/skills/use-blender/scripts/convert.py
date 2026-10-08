# ty: ignore[invalid-assignment]
# mypy: disable-error-code=union-attr
"""Headless batch of files through importers and one exporter into `.artifacts/blender/convert/<name>/`."""

from collections.abc import Mapping
import contextlib
from pathlib import Path
import shutil
import sys
from types import MappingProxyType
from typing import Protocol, runtime_checkable, TextIO

import attrs
import bpy
import numpy as np

from results import artifacts, collect_faults, Fault, Faults, JSON, Resolved
from rna import DIGITS, operators
from scene import bounds

# --- [TYPES] ----------------------------------------------------------------------------


@runtime_checkable
class Executing(Protocol):
    """Python operator class that runs from code through its own `execute`."""

    def execute(self, context: bpy.types.Context) -> set[str]:
        """Return set of one run from code."""
        ...


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
    files: tuple[Resolved[Converted], ...]


# --- [OPERATIONS] -----------------------------------------------------------------------


def convert(name: str, exporter: str, sources: tuple[str, ...], options: Mapping[str, Mapping[str, object]] = MappingProxyType({})) -> Resolved[Batch]:
    """Return the batch of each source imported into an empty factory scene and exported to a cleared `.artifacts/blender/convert/<name>/` as `<stem><ext>`, `options` holding keyword arguments per operator id and naming importers, refused in a process with a window."""
    if not bpy.app.background:
        return Faults.of(Fault(bpy.ops.wm.read_homefile, bpy.data.filepath or None))
    functions = {op.idname_py(): op for op in operators()}
    properties = {key: op.get_rna_type().properties for key, op in functions.items()}
    globs = {
        key: patterns
        for key, props in properties.items()
        if "filter_glob" in props and isinstance(glob := props["filter_glob"], bpy.types.StringProperty) and (patterns := tuple(filter(None, glob.default.split(";"))))
    }
    writers = {key for key in globs if "check_existing" in (props := properties[key]) and isinstance(flag := props["check_existing"], bpy.types.BoolProperty) and flag.default}
    importers = {key: patterns for key, patterns in globs.items() if key not in writers}
    if faults := collect_faults(
        *(Fault(bpy.types.Operator, op, tuple(sorted(table))) for op, table in ((exporter, writers), *((op, importers.keys() | writers) for op in options)) if op not in table),
        *(Fault(functions[op], key, tuple(properties[op].keys())) for op, values in options.items() if op in properties for key in values if key not in properties[op]),
    ):
        return faults
    ids = {op.idname(): key for key, op in functions.items()} | {key: key for key in functions}
    classes = {key: bpy.types.Operator.bl_rna_get_subclass_py(op.idname()) for key, op in functions.items()}
    native = {key for key in importers if classes[key] is None}
    handled = {
        ids[op]: tuple(f"*{suffix}" for suffix in handler.bl_file_extensions.split(";"))
        for handler in bpy.types.FileHandler.__subclasses__()
        if (op := handler.bl_import_operator) in ids and (classes[ids[op]] is None or issubclass(classes[ids[op]], Executing))
    }
    named = tuple(op for op in options if op in importers)
    readers = {key: importers[key] for key in (*native, *named)} | handled
    shutil.rmtree(artifacts("convert", name))
    out, paths = artifacts("convert", name), tuple(Path(s).resolve() for s in sources)
    matches = {path: tuple(sorted(op for op, patterns in readers.items() if any(path.match(p, case_sensitive=False) for p in patterns))) for path in paths}
    spare = tuple(op for op in named if not any(op in found for found in matches.values()))

    def resolve(source: Path) -> str | Fault:
        """Named importer whose filter matches the file, or the named importer no file's filter matches when none does, else the file's lone C reader, else its lone reader, a fault naming the importers that can read it."""
        candidates = matches[source]
        chosen = tuple(op for op in named if op in candidates) if candidates else spare
        match chosen, tuple(op for op in candidates if op in native), candidates:
            case ((one,), _, _) | ((), (one,), _) | ((), (), (one,)):
                return one
            case _:
                return Fault(bpy.types.Operator, str(source), candidates or spare or tuple(sorted(importers)))

    def run(op: str, source: Path, **arguments: object) -> Fault | None:
        """Fault of one operator call on the source with its message or return status on the log, `None` when it finishes."""
        try:
            status = functions[op](**arguments)
        except (RuntimeError, TypeError) as error:
            sys.stdout.write(f"{op} raised {str(error).strip()}\n")
            return Fault(functions[op], str(source))
        if "FINISHED" in status:
            return None
        sys.stdout.write(f"{op} returned {sorted(status)}\n")
        return Fault(functions[op], str(source))

    def one(source: Path, sink: TextIO) -> Resolved[Converted]:
        """Outcome of importing one source into an empty scene with a world and exporting it, its operators' output in the sink under a line naming it."""
        shared = tuple(str(p) for p in paths if p.stem == source.stem and p != source)
        if isinstance(chosen := resolve(source), Fault) or shared:
            return Faults.of(chosen, Fault(Path, str(source), shared) if shared else None)
        sink.write(f"--- {source}\n")
        bpy.ops.wm.read_homefile(use_empty=True, use_factory_startup=True)
        bpy.context.scene.world = bpy.data.worlds.new("World")
        location = {"filepath": str(source), "directory": str(source.parent), "files": [{"name": source.name}]}
        if failed := run(chosen, source, **options.get(chosen, {}), **{key: value for key, value in location.items() if key in properties[chosen]}):
            return Faults.of(failed)
        corners = tuple(bounds(bpy.context.evaluated_depsgraph_get(), drawn=False).values())
        extent = tuple(np.ptp(np.concatenate(corners), axis=0).round(DIGITS).tolist()) if corners else None
        empty = tuple(sorted(o.name for o in bpy.data.objects if isinstance(o.data, bpy.types.Mesh) and not o.data.vertices))
        before = set(out.iterdir())
        if failed := run(exporter, source, **options.get(exporter, {}), filepath=str(out / f"{source.stem}{globs[exporter][0].removeprefix('*')}")):
            return Faults.of(failed)
        return Converted(str(source), chosen, len(bpy.data.objects), empty, extent, tuple(sorted(map(str, set(out.iterdir()) - before))))

    report, log = out / "convert.json", out / "convert.log"
    with log.open("w", encoding="utf-8") as sink, contextlib.redirect_stdout(sink), contextlib.redirect_stderr(sink):
        batch = Batch(str(report), str(log), tuple(one(path, sink) for path in paths))
    report.write_text(JSON.dumps(batch, indent=1), encoding="utf-8")
    return batch


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Batch", "Converted", "Executing", "convert"]
