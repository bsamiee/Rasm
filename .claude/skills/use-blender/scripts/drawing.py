# mypy: disable-error-code="index, union-attr"
# ty: ignore[unresolved-attribute]
# ruff: file-ignore[suspicious-xml-etree-import, subprocess-without-shell-equals-true]
"""Grease Pencil strokes an orthographic camera sees as an SVG and PDF sheet at scale in `.artifacts/blender/`, screen ink written as document ink."""

from enum import StrEnum
from itertools import pairwise
import shutil
import subprocess
import xml.etree.ElementTree as ET

import attrs
import bpy
from mathutils import Color
import numpy as np
from results import artifacts, unknown, UnknownObjects

from interface.roles import Ink
from interface.units import Length

# --- [TYPES] ----------------------------------------------------------------------------


class Rejection(StrEnum):
    """Scene state no sheet draws from, each value the `kind` its result reports."""

    NO_CAMERA = "NoCamera"
    NOT_CAMERA = "NotCamera"
    NOT_ORTHOGRAPHIC = "NotOrthographic"
    NOT_GREASE_PENCIL = "NotGreasePencil"
    NO_STROKES = "NoStrokes"


# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Sheet:
    """Written SVG and PDF, paper size in meters, scale denominator, camera, and drawn stroke count per Grease Pencil object."""

    svg: str
    pdf: str
    paper: tuple[float, float]
    scale: int
    camera: str
    strokes: dict[str, int]


# --- [ERRORS] ---------------------------------------------------------------------------


@attrs.frozen
class Rejected:
    """Scene state the sheet refused, with the object, camera, or scene names it concerns."""

    kind: Rejection
    names: tuple[str, ...]


@attrs.frozen
class NoPdf:
    """SVG written without its PDF, with the diagnostics of the failed `typst compile`, `None` when the process's `PATH` holds no `typst`."""

    svg: str
    diagnostics: str | None


# --- [OPERATIONS] -----------------------------------------------------------------------


def sheet(name: str, scale: int, camera: str | None = None, objects: tuple[str, ...] = ()) -> Sheet | Rejected | NoPdf | UnknownObjects:
    """Project the strokes of the named Grease Pencil objects, or of every visible one, through the named or scene camera onto a 1:`scale` sheet."""
    depsgraph = bpy.context.evaluated_depsgraph_get()
    scene = depsgraph.scene
    if (absent := unknown(scene.objects, objects if camera is None else (camera, *objects))) is not None:
        return absent
    if (view := scene.camera if camera is None else scene.objects[camera]) is None:
        return Rejected(Rejection.NO_CAMERA, (scene.name,))
    if not isinstance(lens := view.data, bpy.types.Camera):
        return Rejected(Rejection.NOT_CAMERA, (view.name,))
    if lens.type != "ORTHO":
        return Rejected(Rejection.NOT_ORTHOGRAPHIC, (view.name,))
    drawn = [scene.objects[n] for n in objects] or [o for o in scene.objects if isinstance(o.data, bpy.types.GreasePencil) and o.visible_get()]
    if foreign := tuple(o.name for o in drawn if not isinstance(o.data, bpy.types.GreasePencil)):
        return Rejected(Rejection.NOT_GREASE_PENCIL, foreign)
    to_view = view.matrix_world.normalized().inverted()
    frame = np.array([corner.xy for corner in lens.view_frame(scene=scene)])
    low, high = frame.min(axis=0), frame.max(axis=0)
    inches = scene.unit_settings.scale_length / scale / Length.INCHES
    digits = int(-np.log10(np.spacing(np.float32(1))))

    def number(value: float) -> str:
        """Text of the value to the decimal digits single precision holds, the precision of Grease Pencil positions and camera frames."""
        return np.format_float_positional(value, precision=digits, unique=False, fractional=False, trim="-")

    def ink(style: bpy.types.MaterialGPencilStyle) -> str:
        """Hex of the style's scene-linear stroke color in sRGB bytes, screen ink written as document ink."""
        srgb = (np.array(Color(style.color[:3]).from_scene_linear_to_srgb()).clip(0, 1) * 255).round().astype(np.uint8)
        return "#" + (bytes(Ink.DOCUMENT) if (srgb == Ink.SCREEN).all() else srgb.tobytes()).hex()

    def shown(node: bpy.types.GreasePencilLayer | bpy.types.GreasePencilLayerGroup | None) -> bool:
        """Whether neither the node nor a group holding it is hidden."""
        return node is None or (not node.hide and shown(node.parent_group))

    def strokes(owner: bpy.types.Object, layer: bpy.types.GreasePencilLayer) -> list[ET.Element]:
        """Element per stroke of the layer's current drawing with two or more points and a shown material, in paper inches."""
        if (frame := layer.current_frame()) is None or (drawing := frame.drawing) is None or not len(found := drawing.strokes):
            return []
        match layer.parent:
            case None:
                parent_to_world = owner.matrix_world
            case bpy.types.Object(pose=bpy.types.Pose(bones=bones)) as parent if layer.parent_bone in bones:
                parent_to_world = parent.matrix_world @ bones[layer.parent_bone].matrix @ layer.matrix_parent_inverse
            case parent:
                parent_to_world = parent.matrix_world @ layer.matrix_parent_inverse
        offsets = np.empty(len(drawing.curve_offsets), np.int32)
        drawing.curve_offsets.foreach_get("value", offsets)
        positions = np.empty(offsets[-1] * 3, np.float32)
        point = found[0].points[0]
        columns = {"radius": np.full(offsets[-1], point.radius, np.float32), "opacity": np.full(offsets[-1], point.opacity, np.float32)}
        for attribute in drawing.attributes:
            match attribute:
                case bpy.types.FloatVectorAttribute(name="position"):
                    attribute.data.foreach_get("vector", positions)
                case bpy.types.FloatAttribute(name=column) if column in columns:
                    attribute.data.foreach_get("value", columns[column])
        projection = np.array(to_view @ parent_to_world @ layer.matrix_local)
        xy = positions.reshape(-1, 3) @ projection[:2, :3].T + projection[:2, 3]
        points = np.column_stack(((xy[:, 0] - low[0]) * inches, (high[1] - xy[:, 1]) * inches))
        widths = 2 * columns["radius"] * np.linalg.norm(projection[:3, :3] @ np.full(3, np.sqrt(1 / 3))) * inches
        styles = {i: (ink(style), style.color[3]) for i, slot in enumerate(owner.material_slots) if slot.material is not None and (style := slot.material.grease_pencil) is not None and not style.hide}
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
            if end - start > 1 and (pen := styles.get(stroke.material_index)) is not None
            for color, alpha in (pen,)
        ]

    layers = {owner.name: {layer.name: strokes(owner, layer) for layer in owner.data.layers if shown(layer)} for owner in (o.evaluated_get(depsgraph) for o in drawn)}
    counts = {owner: sum(map(len, by_layer.values())) for owner, by_layer in layers.items()}
    if not any(counts.values()):
        return Rejected(Rejection.NO_STROKES, tuple(counts))
    across, down = (high - low) * inches
    width, height = number(across), number(down)
    root = ET.Element("svg", {"xmlns": "http://www.w3.org/2000/svg", "width": f"{width}in", "height": f"{height}in", "viewBox": f"0 0 {width} {height}"})
    linework = ET.SubElement(root, "g", {"fill": "none", "stroke-linecap": "round", "stroke-linejoin": "round"})
    for owner, by_layer in layers.items():
        for layer, elements in by_layer.items():
            ET.SubElement(linework, "g", {"id": f"{owner}/{layer}"}).extend(elements)
    path = artifacts() / f"{name}.svg"
    ET.ElementTree(root).write(path, encoding="utf-8")
    svg, pdf = str(path), str(path.with_suffix(".pdf"))
    if (typst := shutil.which("typst")) is None:
        return NoPdf(svg, None)
    page = "#set page(width: auto, height: auto, margin: 0pt)\n#image(sys.inputs.svg)\n"
    compiled = subprocess.run((typst, "compile", "--root", path.anchor, "--input", f"svg={svg}", "-", pdf), input=page, capture_output=True, text=True, check=False)
    paper = (float(number(across * Length.INCHES)), float(number(down * Length.INCHES)))
    return NoPdf(svg, compiled.stderr.strip()) if compiled.returncode else Sheet(svg, pdf, paper, scale, view.name, counts)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["NoPdf", "Rejected", "Rejection", "Sheet", "sheet"]
