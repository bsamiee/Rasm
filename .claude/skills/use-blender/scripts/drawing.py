# ast-grep-ignore: no-stdlib-record
# mypy: disable-error-code="arg-type, index"
# ruff: file-ignore[suspicious-xml-etree-import, subprocess-without-shell-equals-true]
"""Write the Line Art strokes seen through an orthographic camera as an SVG and PDF sheet at scale to `.artifacts/blender/<name>.svg` and `.pdf`, run inside Blender through `runpy.run_path`."""

from dataclasses import asdict, dataclass
from enum import StrEnum
from itertools import pairwise
from pathlib import Path
import shutil
import subprocess
from typing import cast
import xml.etree.ElementTree as ET

import bpy
from mathutils import Color
import numpy as np

# --- [TYPES] ----------------------------------------------------------------------------

type Outcome = Sheet | Rejected | NoPdf


class Rejection(StrEnum):
    """Scene state no sheet draws from, each value the `kind` its result reports."""

    UNKNOWN_OBJECTS = "UnknownObjects"
    NO_CAMERA = "NoCamera"
    NOT_CAMERA = "NotCamera"
    NOT_ORTHOGRAPHIC = "NotOrthographic"
    NOT_GREASE_PENCIL = "NotGreasePencil"
    NO_STROKES = "NoStrokes"


# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Sheet:
    """Written SVG and PDF, paper size in millimeters, scale denominator, camera, and drawn stroke count per Grease Pencil object."""

    path: str
    pdf: str
    paper_mm: tuple[float, float]
    scale: int
    camera: str
    strokes: dict[str, int]


# --- [ERRORS] ---------------------------------------------------------------------------


@dataclass(frozen=True)
class Rejected:
    """Scene state the sheet refused, with the object, camera, or scene names it concerns."""

    kind: Rejection
    names: tuple[str, ...]


@dataclass(frozen=True)
class NoPdf:
    """SVG written, `typst` absent from the process's `PATH` or `typst compile` failed with its diagnostics."""

    path: str
    error: str


# --- [OPERATIONS] -----------------------------------------------------------------------


def sheet(name: str, scale: int, camera: str | None = None, objects: tuple[str, ...] = ()) -> Outcome:
    """Project the strokes of the named Grease Pencil objects, or of every one with a Line Art modifier, through the scene camera onto a 1:`scale` sheet."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    scene = cast("bpy.types.Scene", depsgraph.scene)
    if missing := tuple(n for n in (objects if camera is None else (camera, *objects)) if n not in scene.objects):
        return Rejected(Rejection.UNKNOWN_OBJECTS, missing)
    match scene.camera if camera is None else scene.objects[camera]:
        case None:
            return Rejected(Rejection.NO_CAMERA, (scene.name,))
        case bpy.types.Object(data=bpy.types.Camera(type="ORTHO") as lens) as view:
            pass
        case bpy.types.Object(data=bpy.types.Camera()) as view:
            return Rejected(Rejection.NOT_ORTHOGRAPHIC, (view.name,))
        case view:
            return Rejected(Rejection.NOT_CAMERA, (view.name,))
    drawn = [scene.objects[n] for n in objects] or [
        o for o in scene.objects if isinstance(o.data, bpy.types.GreasePencil) and any(isinstance(m, bpy.types.GreasePencilLineartModifier) for m in o.modifiers)
    ]
    if foreign := tuple(o.name for o in drawn if not isinstance(o.data, bpy.types.GreasePencil)):
        return Rejected(Rejection.NOT_GREASE_PENCIL, foreign)
    to_view = view.matrix_world.normalized().inverted()
    frame = np.array([(to_view @ view.matrix_world @ corner).xy for corner in lens.view_frame(scene=scene)])
    low, high = frame.min(axis=0), frame.max(axis=0)
    mm = scene.unit_settings.scale_length / bpy.utils.units.to_value("METRIC", "LENGTH", "1mm") / scale

    def number(value: float) -> str:
        """Text of the value to the significant digits single precision holds, the precision of Grease Pencil positions and camera frames."""
        return np.format_float_positional(value, precision=np.finfo(np.float32).precision, unique=False, fractional=False, trim="-")

    def strokes(owner: bpy.types.Object, layer: bpy.types.GreasePencilLayer) -> list[ET.Element]:
        """One element per stroke of the layer's current drawing with two or more points and a visible material, in sheet millimeters with its own color, opacity, and width."""
        match layer.current_frame():
            case bpy.types.GreasePencilFrame(drawing=bpy.types.GreasePencilDrawing(strokes=found) as drawing) if len(found):
                pass
            case _:
                return []
        offsets = np.empty(len(drawing.curve_offsets), np.int32)
        drawing.curve_offsets.foreach_get("value", offsets)
        positions = np.empty(offsets[-1] * 3, np.float32)
        columns = {column: np.full(offsets[-1], getattr(found[0].points[0], column), np.float32) for column in ("radius", "opacity")}
        for attribute in drawing.attributes:
            match attribute:
                case bpy.types.FloatVectorAttribute(name="position"):
                    attribute.data.foreach_get("vector", positions)
                case bpy.types.FloatAttribute(name=column) if column in columns:
                    attribute.data.foreach_get("value", columns[column])
        to_world = (layer.parent.matrix_world @ layer.matrix_parent_inverse if layer.parent else owner.matrix_world) @ layer.matrix_local
        projection = np.array(to_view @ to_world)
        xy = positions.reshape(-1, 3) @ projection[:2, :3].T + projection[:2, 3]
        points = np.column_stack(((xy[:, 0] - low[0]) * mm, (high[1] - xy[:, 1]) * mm))
        widths = 2 * (columns["radius"] + layer.radius_offset) * to_world.median_scale * mm
        styles = {
            i: ("#" + (np.clip(Color(style.color[:3]).from_scene_linear_to_srgb()[:], 0, 1) * 255).round().astype(np.uint8).tobytes().hex(), style.color[3])
            for i, slot in enumerate(owner.material_slots)
            if slot.material is not None and (style := slot.material.grease_pencil) is not None and not style.hide
        }
        return [
            ET.Element(
                "polygon" if stroke.cyclic else "polyline",
                {
                    "points": " ".join(f"{number(x)},{number(y)}" for x, y in points[start:end]),
                    "stroke": color,
                    "stroke-opacity": number(alpha * layer.opacity * columns["opacity"][start:end].mean()),
                    "stroke-width": number(widths[start:end].mean()),
                },
            )
            for stroke, (start, end) in zip(found, pairwise(offsets), strict=True)
            if end - start > 1 and stroke.material_index in styles
            for color, alpha in (styles[stroke.material_index],)
        ]

    pencils = [(o, data) for o in (d.evaluated_get(depsgraph) for d in drawn) if isinstance(data := o.data, bpy.types.GreasePencil)]
    layers = {owner.name: {layer.name: strokes(owner, layer) for layer in data.layers if not layer.hide} for owner, data in pencils}
    counts = {owner: sum(map(len, by_layer.values())) for owner, by_layer in layers.items()}
    if not any(counts.values()):
        return Rejected(Rejection.NO_STROKES, tuple(counts))
    width, height = map(number, (high - low) * mm)
    svg = ET.Element("svg", {"xmlns": "http://www.w3.org/2000/svg", "width": f"{width}mm", "height": f"{height}mm", "viewBox": f"0 0 {width} {height}"})
    linework = ET.SubElement(svg, "g", {"fill": "none", "stroke-linecap": "round", "stroke-linejoin": "round"})
    for owner, by_layer in layers.items():
        for layer, elements in by_layer.items():
            ET.SubElement(linework, "g", {"id": f"{owner}/{layer}"}).extend(elements)
    path = next(p for p in Path(__file__).resolve().parents if (p / ".git").exists()) / ".artifacts" / "blender" / f"{name}.svg"
    path.parent.mkdir(parents=True, exist_ok=True)
    ET.ElementTree(svg).write(path, encoding="utf-8")
    pdf = path.with_suffix(".pdf")
    match shutil.which("typst"):
        case None:
            return NoPdf(str(path), "typst is not on the PATH of this Blender process")
        case typst:
            page = "#set page(width: auto, height: auto, margin: 0pt)\n#image(sys.inputs.svg)\n"
            compiled = subprocess.run((typst, "compile", "--root", path.anchor, "--input", f"svg={path}", "-", str(pdf)), input=page, capture_output=True, text=True, check=False)
    return NoPdf(str(path), compiled.stderr.strip()) if compiled.returncode else Sheet(str(path), str(pdf), (float(width), float(height)), scale, view.name, counts)


def as_result(value: Outcome) -> dict[str, object]:
    """`result` dict for `execute_blender_code`, the case name, or a rejection's own kind, under `kind`."""
    return {"kind": type(value).__name__, **asdict(value)}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NoPdf", "Outcome", "Rejected", "Rejection", "Sheet", "as_result", "sheet"]
