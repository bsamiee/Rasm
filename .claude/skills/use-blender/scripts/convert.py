"""Convert interchange files one empty scene per file through Blender's importers and one exporter, run in a headless session through `runpy.run_path`."""

from collections.abc import Mapping
from pathlib import Path
from typing import Final

import attrs
import bpy
from cattrs.preconf.json import make_converter
import numpy as np
from numpy.typing import NDArray

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Converted | NoImporter | AmbiguousImporter | SharedStem | Failed | EmptyImport
type Result = Batch | UnknownOperator | UnknownOptions | LiveSession

# --- [CONSTANTS] ------------------------------------------------------------------------

JSON: Final = make_converter()

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Converted:
    """Files the exporter wrote, importer used, imported object names, and the world extent in meters of their evaluated vertices."""

    source: str
    outputs: tuple[str, ...]
    importer: str
    objects: tuple[str, ...]
    extent_m: tuple[float, float, float]


@attrs.frozen
class Batch:
    """One outcome per input in input order, and the report file holding them."""

    report: str
    files: tuple[Outcome, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class NoImporter:
    """No C importer's file filter or file handler names the file, and no importer is named."""

    source: str
    suffix: str


@attrs.frozen
class AmbiguousImporter:
    """More than one importer reads the file with no named importer or lone C importer among them, `importer=` picks one."""

    source: str
    candidates: tuple[str, ...]


@attrs.frozen
class SharedStem:
    """Sources sharing one stem and its output file."""

    source: str
    sources: tuple[str, ...]


@attrs.frozen
class Failed:
    """Operator that raised or did not finish on a file, with its message or return set."""

    source: str
    operator: str
    error: str


@attrs.frozen
class EmptyImport:
    """Importer finished and added no object with vertices."""

    source: str
    importer: str


@attrs.frozen
class UnknownOperator:
    """Exporter, importer, or option ids naming no operator that writes or reads files through a file filter."""

    operators: tuple[str, ...]


@attrs.frozen
class UnknownOptions:
    """Option names as `<operator>.<property>` that the operator does not declare."""

    options: tuple[str, ...]


@attrs.frozen
class LiveSession:
    """Call inside the live session, where emptying the scene per file replaces the user's open document."""

    document: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def as_result(value: Result | Outcome) -> dict[str, object]:
    """`result` dict, the case name under `kind` for the batch and for every file."""
    match value:
        case Batch(files=files):
            return {"kind": type(value).__name__, **attrs.asdict(value), "files": [as_result(f) for f in files]}
        case _:
            return {"kind": type(value).__name__, **attrs.asdict(value)}


def convert(name: str, exporter: str, sources: tuple[str, ...], importer: str | None = None, options: Mapping[str, Mapping[str, object]] | None = None) -> Result:
    """Import each source into an empty scene and export it to `.artifacts/blender/<name>/<stem><ext>`, `importer` for the files its filter names and the files no filter names, `options` the keyword arguments per importer or exporter id."""
    if not bpy.app.background:
        return LiveSession(bpy.data.filepath)
    settings = {} if options is None else options
    categories = {c: getattr(bpy.ops, c) for c in dir(bpy.ops)}
    operators = {f"{c}.{n}": getattr(category, n) for c, category in categories.items() for n in dir(category)}
    properties = {key: op.get_rna_type().properties for key, op in operators.items()}
    ids = {op.idname(): key for key, op in operators.items()} | {key: key for key in operators}
    globs = {key: patterns for key, props in properties.items() if (glob := props.get("filter_glob")) is not None and (patterns := tuple(p for p in glob.default.split(";") if p))}
    saves = {key for key, props in properties.items() if (check := props.get("check_existing")) is not None and check.default}
    handled = {ids[op]: tuple(f"*{e}" for e in handler.bl_file_extensions.split(";")) for handler in bpy.types.FileHandler.__subclasses__() if (op := handler.bl_import_operator) in ids}
    opens = {key: globs[key] for key in globs.keys() - saves} | handled
    native = {key for key in opens if bpy.types.Operator.bl_rna_get_subclass_py(operators[key].idname()) is None}
    writers = globs.keys() & saves
    if unknown := tuple(op for op, table in ((exporter, writers), (importer, opens.keys()), *((op, opens.keys() | writers) for op in settings)) if op is not None and op not in table):
        return UnknownOperator(unknown)
    if undeclared := tuple(f"{op}.{key}" for op, values in settings.items() for key in values if key not in properties[op]):
        return UnknownOptions(undeclared)
    readers = {key: patterns for key, patterns in opens.items() if key in native or key in handled or key == importer}
    out = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / name
    out.mkdir(parents=True, exist_ok=True)
    paths = tuple(Path(s).resolve() for s in sources)

    def resolve(source: Path) -> str | NoImporter | AmbiguousImporter:
        """Named importer when its filter names the file or no filter does, else the file's one C reader, else its one reader."""
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
        """Failure of one operator call, an unreadable file raising `RuntimeError`, an argument outside its property raising `TypeError`, and a cancelled call answering without `FINISHED`."""
        try:
            status = operators[op](**arguments)
        except (RuntimeError, TypeError) as error:
            return Failed(str(source), op, str(error))
        return None if "FINISHED" in status else Failed(str(source), op, str(sorted(status)))

    def stamps() -> frozenset[tuple[str, int]]:
        """Name and modification time of every file in the output folder."""
        return frozenset((p.name, p.stat().st_mtime_ns) for p in out.iterdir())

    def points(instance: bpy.types.DepsgraphObjectInstance) -> NDArray[np.float64]:
        """World positions of one object instance's evaluated vertices, none for the object types `to_mesh` refuses."""
        match instance.object:
            case bpy.types.Object(type="MESH" | "CURVE" | "SURFACE" | "FONT" | "META") as evaluated:
                mesh = evaluated.to_mesh()
                local = np.empty(len(mesh.vertices) * 3, dtype=np.float32)
                mesh.vertices.foreach_get("co", local)
                evaluated.to_mesh_clear()
                matrix = np.array(instance.matrix_world, dtype=np.float64)
                return local.reshape(-1, 3) @ matrix[:3, :3].T + matrix[:3, 3]
            case _:
                return np.empty((0, 3), dtype=np.float64)

    def one(source: Path) -> Outcome:
        """Outcome of importing one source into an empty scene and exporting it."""
        if not isinstance(chosen := resolve(source), str):
            return chosen
        if len(shared := tuple(str(p) for p in paths if p.stem == source.stem)) > 1:
            return SharedStem(str(source), shared)
        bpy.ops.wm.read_homefile(use_empty=True, use_factory_startup=True)
        location = {"filepath": str(source), "directory": str(source.parent), "files": [{"name": source.name}]}
        if failed := run(chosen, source, **settings.get(chosen, {}), **{key: value for key, value in location.items() if key in properties[chosen]}):
            return failed
        depsgraph = bpy.context.evaluated_depsgraph_get()
        clouds = [points(i) for i in depsgraph.object_instances]
        if not any(cloud.size for cloud in clouds):
            return EmptyImport(str(source), chosen)
        x, y, z = (round(extent, 6) for extent in np.ptp(np.concatenate(clouds), axis=0).tolist())
        target = out / f"{source.stem}{globs[exporter][0].removeprefix('*')}"
        before = stamps()
        if failed := run(exporter, source, **settings.get(exporter, {}), filepath=str(target)):
            return failed
        outputs = tuple(sorted(str(out / name) for name, _ in stamps() - before))
        return Converted(str(source), outputs, chosen, tuple(sorted(o.name for o in bpy.data.objects)), (x, y, z))

    report = out / "convert.json"
    batch = Batch(str(report), tuple(one(p) for p in paths))
    report.write_text(JSON.dumps(as_result(batch), indent=1), encoding="utf-8")
    return batch


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "AmbiguousImporter",
    "Batch",
    "Converted",
    "EmptyImport",
    "Failed",
    "LiveSession",
    "NoImporter",
    "Outcome",
    "Result",
    "SharedStem",
    "UnknownOperator",
    "UnknownOptions",
    "as_result",
    "convert",
]
