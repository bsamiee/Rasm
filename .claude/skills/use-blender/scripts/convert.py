# ast-grep-ignore: no-json-codec, no-stdlib-record

"""Convert interchange files one empty scene per file through Blender's importers and one exporter, run in a headless session through `runpy.run_path`."""

from dataclasses import asdict, dataclass
import json
from pathlib import Path

import bpy
import numpy as np
from numpy.typing import NDArray

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Converted | NoImporter | AmbiguousImporter | SharedStem | Failed | EmptyImport
type Result = Batch | UnknownOperator | LiveSession

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Converted:
    """Written file, importer used, imported object names, and the world extent in meters of their evaluated vertices."""

    source: str
    output: str
    importer: str
    objects: tuple[str, ...]
    extent_m: tuple[float, float, float]


@dataclass(frozen=True)
class Batch:
    """One outcome per input in input order, and the report file holding them."""

    report: str
    files: tuple[Outcome, ...]


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class NoImporter:
    """No C importer's file filter, file handler, or named importer's filter names the file."""

    source: str
    suffix: str


@dataclass(frozen=True)
class AmbiguousImporter:
    """Several importers read the file and neither a named importer nor a lone C importer among them settles it, `importer=` picks one."""

    source: str
    candidates: tuple[str, ...]


@dataclass(frozen=True)
class SharedStem:
    """Sources with one stem that would write one output file."""

    source: str
    sources: tuple[str, ...]


@dataclass(frozen=True)
class Failed:
    """Operator that raised or did not finish on a file, with its message or return set."""

    source: str
    operator: str
    error: str


@dataclass(frozen=True)
class EmptyImport:
    """Importer finished and added no object with vertices."""

    source: str
    importer: str


@dataclass(frozen=True)
class UnknownOperator:
    """Exporter or importer ids naming no operator that writes or reads files through a file filter."""

    operators: tuple[str, ...]


@dataclass(frozen=True)
class LiveSession:
    """Call inside the live session, where emptying the scene per file would replace the user's open document."""

    document: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def convert(name: str, exporter: str, sources: tuple[str, ...], importer: str | None = None) -> Result:
    """Import each source into an empty scene and export it to `.artifacts/blender/<name>/<stem><ext>`, `importer` for the files its filter names."""
    if not bpy.app.background:
        return LiveSession(bpy.data.filepath)
    create = vars(bpy.ops)["_ops_module"].create_function
    operators = {f"{c}.{n}": create(c, n) for c in dir(bpy.ops) for n in dir(getattr(bpy.ops, c))}
    properties = {key: op.get_rna_type().properties for key, op in operators.items()}
    ids = {op.idname(): key for key, op in operators.items()} | {key: key for key in operators}
    globs = {key: tuple(p for p in props["filter_glob"].default.split(";") if p) for key, props in properties.items() if "filter_glob" in props and props["filter_glob"].default}
    saves = {key for key, props in properties.items() if "check_existing" in props and props["check_existing"].default}
    handled = {
        ids[op]: tuple(f"*{e}" for e in handler.bl_file_extensions.split(";")) for handler in bpy.types.FileHandler.__subclasses__() if (op := getattr(handler, "bl_import_operator", "")) in ids
    }
    opens = {key: globs[key] for key in globs.keys() - saves} | handled
    native = {key for key in opens if bpy.types.Operator.bl_rna_get_subclass_py(operators[key].idname()) is None}
    if unknown := tuple(op for op, table in ((exporter, globs.keys() & saves), (importer, opens)) if op is not None and op not in table):
        return UnknownOperator(unknown)
    readers = {key: patterns for key, patterns in opens.items() if key in native or key in handled or key == importer}
    out = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / name
    out.mkdir(parents=True, exist_ok=True)
    paths = tuple(Path(s).resolve() for s in sources)

    def resolve(source: Path) -> str | NoImporter | AmbiguousImporter:
        """Named importer that reads the file, else its one C reader, else its one reader."""
        candidates = tuple(sorted(op for op, patterns in readers.items() if any(source.match(p, case_sensitive=False) for p in patterns)))
        match (importer,) if importer in candidates else (), tuple(op for op in candidates if op in native), candidates:
            case ((chosen,), _, _) | ((), (chosen,), _) | ((), (), (chosen,)):
                return chosen
            case _, _, ():
                return NoImporter(str(source), source.suffix)
            case _:
                return AmbiguousImporter(str(source), candidates)

    def run(op: str, source: Path, **arguments: object) -> Failed | None:
        """Failure of one operator call, an unreadable file raising `RuntimeError` and a cancelled call answering without `FINISHED`."""
        try:
            status = operators[op](**arguments)
        except RuntimeError as error:
            return Failed(str(source), op, str(error))
        return None if "FINISHED" in status else Failed(str(source), op, str(sorted(status)))

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
        bpy.ops.wm.read_homefile(use_empty=True)
        location = {"filepath": str(source), "directory": str(source.parent), "files": [{"name": source.name}]}
        if failed := run(chosen, source, **{key: value for key, value in location.items() if key in properties[chosen]}):
            return failed
        depsgraph = bpy.context.evaluated_depsgraph_get()
        clouds = [points(i) for i in depsgraph.object_instances]
        if not any(cloud.size for cloud in clouds):
            return EmptyImport(str(source), chosen)
        x, y, z = (round(extent, 6) for extent in np.ptp(np.concatenate(clouds), axis=0).tolist())
        target = out / f"{source.stem}{globs[exporter][0].removeprefix('*')}"
        if failed := run(exporter, source, filepath=str(target)):
            return failed
        return Converted(str(source), str(target), chosen, tuple(sorted(o.name for o in bpy.data.objects)), (x, y, z))

    report = out / "convert.json"
    batch = Batch(str(report), tuple(one(p) for p in paths))
    report.write_text(json.dumps(as_result(batch), indent=1), encoding="utf-8")
    return batch


def as_result(value: Result | Outcome) -> dict[str, object]:
    """`result` dict, the case name under `kind` for the batch and for every file."""
    match value:
        case Batch(files=files):
            return {"kind": type(value).__name__, **asdict(value), "files": [as_result(f) for f in files]}
        case _:
            return {"kind": type(value).__name__, **asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["AmbiguousImporter", "Batch", "Converted", "EmptyImport", "Failed", "LiveSession", "NoImporter", "Outcome", "Result", "SharedStem", "UnknownOperator", "as_result", "convert"]
