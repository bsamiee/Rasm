# mypy: disable-error-code="attr-defined"
# ty: ignore[unresolved-attribute]
"""Convert interchange files through Blender importers and one exporter in a headless process, one empty factory scene per file."""

from collections.abc import Mapping
from pathlib import Path
from types import MappingProxyType

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
    """Source with its importer, imported object names, world extent in meters, and the files the exporter wrote."""

    source: str
    importer: str
    objects: tuple[str, ...]
    extent: tuple[float, float, float] | None
    outputs: tuple[str, ...]


@attrs.frozen
class Batch:
    """Report file and one outcome per source in input order."""

    report: str
    files: tuple[Outcome, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class NoImporter:
    """Source that no importer's file filter or file handler names, with no importer named."""

    source: str
    suffix: str


@attrs.frozen
class AmbiguousImporter:
    """Source more than one importer reads with no named or lone C importer among them, `importer=` picking one."""

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
    """Exporter, importer, or option ids naming no operator of their role."""

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


def convert(
    name: str, exporter: str, sources: tuple[str, ...], importer: str | None = None, options: Mapping[str, Mapping[str, object]] = MappingProxyType({})
) -> Batch | UnknownOperator | UnknownOptions | LiveSession:
    """Batch of each source imported into an empty factory scene and exported to `.artifacts/blender/<name>/<stem><ext>`, `options` holding keyword arguments per operator id."""
    if not bpy.app.background:
        return LiveSession(bpy.data.filepath)
    functions = {op.idname_py(): op for op in operators()}
    properties = {key: op.get_rna_type().properties for key, op in functions.items()}
    ids = {op.idname(): key for key, op in functions.items()} | {key: key for key in functions}
    globs = {key: patterns for key, props in properties.items() if "filter_glob" in props and (patterns := tuple(filter(None, props["filter_glob"].default.split(";"))))}
    writers = {key for key in globs if "check_existing" in (props := properties[key]) and props["check_existing"].default}
    handled = {ids[op]: tuple(f"*{e}" for e in handler.bl_file_extensions.split(";")) for handler in bpy.types.FileHandler.__subclasses__() if (op := handler.bl_import_operator) in ids}
    opens = {key: globs[key] for key in globs.keys() - writers} | handled
    native = {key for key in opens if bpy.types.Operator.bl_rna_get_subclass_py(functions[key].idname()) is None}
    if unknown := tuple(op for op, table in ((exporter, writers), (importer, opens.keys()), *((op, opens.keys() | writers) for op in options)) if op is not None and op not in table):
        return UnknownOperator(unknown)
    if undeclared := tuple(f"{op}.{key}" for op, values in options.items() for key in values if key not in properties[op]):
        return UnknownOptions(undeclared)
    readers = {key: patterns for key, patterns in opens.items() if key in native or key in handled or key == importer}
    out = artifacts(name)
    paths = tuple(Path(s).resolve() for s in sources)

    def resolve(source: Path) -> str | NoImporter | AmbiguousImporter:
        """Named importer when its filter matches the file or no filter does, else the file's lone C reader, else its lone reader."""
        candidates = tuple(sorted(op for op, patterns in readers.items() if any(source.match(p, case_sensitive=False) for p in patterns)))
        named = (importer,) if importer is not None and (importer in candidates or not candidates) else ()
        match named, tuple(op for op in candidates if op in native), candidates:
            case ((chosen,), _, _) | ((), (chosen,), _) | ((), (), (chosen,)):
                return chosen
            case (), (), ():
                return NoImporter(str(source), source.suffix)
            case _:
                return AmbiguousImporter(str(source), candidates)

    def run(op: str, source: Path, **arguments: object) -> Failed | None:
        """Failure of one operator call, `None` when it finishes."""
        try:
            status = functions[op](**arguments)
        except (RuntimeError, TypeError) as error:
            return Failed(str(source), op, str(error).strip())
        return None if "FINISHED" in status else Failed(str(source), op, " ".join(sorted(status)))

    def stamps() -> frozenset[tuple[str, int]]:
        """Name and modification time of every file in the output folder."""
        return frozenset((p.name, p.stat().st_mtime_ns) for p in out.iterdir())

    def one(source: Path) -> Outcome:
        """Outcome of importing one source into an empty scene and exporting it."""
        if not isinstance(chosen := resolve(source), str):
            return chosen
        if len(shared := tuple(str(p) for p in paths if p.stem == source.stem)) > 1:
            return SharedStem(str(source), shared)
        bpy.ops.wm.read_homefile(use_empty=True, use_factory_startup=True)
        location = {"filepath": str(source), "directory": str(source.parent), "files": [{"name": source.name}]}
        if failed := run(chosen, source, **options.get(chosen, {}), **{key: value for key, value in location.items() if key in properties[chosen]}):
            return failed
        corners = tuple(bounds(bpy.context.evaluated_depsgraph_get()).values())
        extent = tuple(np.ptp(np.concatenate(corners), axis=0).round(DIGITS).tolist()) if corners else None
        before = stamps()
        if failed := run(exporter, source, **options.get(exporter, {}), filepath=str(out / f"{source.stem}{globs[exporter][0].removeprefix('*')}")):
            return failed
        return Converted(str(source), chosen, tuple(sorted(o.name for o in bpy.data.objects)), extent, tuple(sorted(str(out / n) for n, _ in stamps() - before)))

    report = out / "convert.json"
    batch = Batch(str(report), tuple(one(p) for p in paths))
    report.write_text(JSON.dumps(batch, indent=1), encoding="utf-8")
    return batch


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AmbiguousImporter", "Batch", "Converted", "Failed", "LiveSession", "NoImporter", "Outcome", "SharedStem", "UnknownOperator", "UnknownOptions", "convert"]
