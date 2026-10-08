# ty: ignore[unresolved-attribute]
# mypy: disable-error-code="index, union-attr"
# ruff: file-ignore[suspicious-xml-etree-import]
"""Grease Pencil strokes an orthographic camera sees as SVG, PDF, and PNG sheets at scale."""

from collections import Counter
from itertools import pairwise
from pathlib import Path
import shutil
import subprocess
import sys
import xml.etree.ElementTree as ET

import attrs
import bpy
import numpy as np
from numpy.typing import NDArray

from results import artifacts, collect_faults, Fault, Faults, Resolved, unknown

# --- [MODELS] ---------------------------------------------------------------------------


@attrs.frozen
class Sheet:
    """Written SVG, PDF, and PNG, paper size in meters, the paper window the PNG shows as left, top, width, and height in meters from the paper's top-left corner, scale denominator, camera, and stroke count by paper pen width in millimeters per Grease Pencil object."""

    svg: str
    pdf: str
    png: str
    paper: tuple[float, float]
    window: tuple[float, float, float, float]
    scale: int
    camera: str
    strokes: dict[str, dict[float, int]]


# --- [OPERATIONS] -----------------------------------------------------------------------


def sheet(name: str, scale: int, objects: tuple[str, ...] = ()) -> Resolved[Sheet]:
    """Return the 1:`scale` sheet of the named or every visible Grease Pencil object's strokes through the orthographic scene camera at the render resolution, Line Art recomputed first and the PNG showing the inked window 2000 px on its long side."""
    scene = bpy.context.scene
    pencils = tuple(o.name for o in scene.objects if isinstance(o.data, bpy.types.GreasePencil))
    orthographic = tuple(o.name for o in scene.objects if isinstance(o.data, bpy.types.Camera) and o.data.type == "ORTHO")
    match (
        unknown(scene.objects, objects),
        scene.camera if scene.camera is not None and scene.camera.name in orthographic else Fault(bpy.types.Camera, scene.camera and scene.camera.name, orthographic),
        collect_faults(*(Fault(bpy.types.GreasePencil, n, pencils) for n in objects if n in scene.objects and n not in pencils)),
        shutil.which("typst") or Fault(Path, "typst"),
    ):
        case None, bpy.types.Object(data=bpy.types.Camera() as lens) as view, None, str() as typst:
            drawn = [scene.objects[n] for n in objects] or [scene.objects[n] for n in pencils if scene.objects[n].visible_get()]
        case failed:
            return Faults.of(*failed)
    for owner in drawn:
        owner.update_tag()
    depsgraph = bpy.context.evaluated_depsgraph_get()
    to_view = view.matrix_world.normalized().inverted()
    frame = np.array([corner.xy for corner in lens.view_frame(scene=scene)])
    low, high = frame.min(axis=0), frame.max(axis=0)
    inch = 0.0254
    inches = scene.unit_settings.scale_length / scale / inch
    digits = int(-np.log10(np.spacing(np.float32(1))))

    def number(value: float) -> str:
        """Text of the value to the decimal digits single precision holds, the precision of Grease Pencil positions and camera frames."""
        return np.format_float_positional(value, precision=digits, unique=False, fractional=False, trim="-")

    def shown(node: bpy.types.GreasePencilLayer | bpy.types.GreasePencilLayerGroup | None) -> bool:
        """Whether neither the node nor a group holding it is hidden."""
        return node is None or (not node.hide and shown(node.parent_group))

    def strokes(owner: bpy.types.Object, layer: bpy.types.GreasePencilLayer) -> list[tuple[float, ET.Element, NDArray[np.float64]]]:
        """Paper pen width in millimeters, element, and points in paper inches per stroke of the layer's current drawing with two or more points and a shown material."""
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
        alphas = {i: style.color[3] for i, slot in enumerate(owner.material_slots) if slot.material is not None and (style := slot.material.grease_pencil) is not None and not style.hide}
        return [
            (
                round(float(width) * inch * 1000, 3),
                ET.Element(
                    "polygon" if stroke.cyclic else "polyline",
                    {
                        "points": " ".join(f"{number(x)},{number(y)}" for x, y in points[start:end]),
                        "stroke-opacity": number(alpha * layer.opacity * columns["opacity"][start:end].mean()),
                        "stroke-width": number(width),
                    },
                ),
                points[start:end],
            )
            for stroke, (start, end) in zip(found, pairwise(offsets), strict=True)
            if end - start > 1 and (alpha := alphas.get(stroke.material_index)) is not None
            for width in (widths[start:end].mean(),)
        ]

    layers = {owner.name: {layer.name: strokes(owner, layer) for layer in owner.data.layers if shown(layer)} for owner in (o.evaluated_get(depsgraph) for o in drawn)}
    counts = {owner: dict(Counter(pen for placed in by_layer.values() for pen, _, _ in placed)) for owner, by_layer in layers.items()}
    if not any(counts.values()):
        return Faults.of(Fault(bpy.types.GreasePencilDrawing, tuple(counts)))
    across, down = (high - low) * inches
    width, height = number(across), number(down)
    root = ET.Element("svg", {"xmlns": "http://www.w3.org/2000/svg", "width": f"{width}in", "height": f"{height}in", "viewBox": f"0 0 {width} {height}"})
    linework = ET.SubElement(root, "g", {"fill": "none", "stroke": "#000000", "stroke-linecap": "round", "stroke-linejoin": "round"})
    for owner, by_layer in layers.items():
        for layer, placed in by_layer.items():
            ET.SubElement(linework, "g", {"id": f"{owner}/{layer}"}).extend(element for _, element, _ in placed)
    path = artifacts("sheets") / f"{name}.svg"
    ET.ElementTree(root).write(path, encoding="utf-8")
    svg, pdf, png = (str(path.with_suffix(suffix)) for suffix in (".svg", ".pdf", ".png"))
    inked = np.concatenate([drawn for by_layer in layers.values() for placed in by_layer.values() for _, _, drawn in placed])
    pad, readable = 0.025 * float(np.ptp(inked, axis=0).max()), 2000
    corner, far = np.maximum(inked.min(axis=0) - pad, 0.0), np.minimum(inked.max(axis=0) + pad, (across, down))
    wide, tall = far - corner
    runs = [
        subprocess.run((typst, "compile", "--root", path.anchor, "--input", f"svg={svg}", *options, "-", output), input=page, capture_output=True, text=True, check=False)
        for output, page, options in (
            (pdf, "#set page(width: auto, height: auto, margin: 0pt)\n#image(sys.inputs.svg)\n", ()),
            (
                png,
                f"#set page(width: {number(wide)}in, height: {number(tall)}in, margin: 0pt)\n#place(dx: {number(-corner[0])}in, dy: {number(-corner[1])}in, box(width: {width}in, height: {height}in, image(sys.inputs.svg)))\n",
                ("--ppi", number(readable / max(wide, tall))),
            ),
        )
    ]
    if diagnostics := "\n".join(run.stderr.strip() for run in runs if run.returncode):
        sys.stderr.write(f"{diagnostics}\n")
        return Faults.of(Fault(Sheet, svg))
    paper = (float(number(across * inch)), float(number(down * inch)))
    left, top, shown_across, shown_down = (float(number(value * inch)) for value in (*corner, wide, tall))
    return Sheet(svg, pdf, png, paper, (left, top, shown_across, shown_down), scale, view.name, counts)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Sheet", "sheet"]
