"""Rendered discontinuities, painted regions, and continuous ink coverage."""

from math import sqrt
from types import MappingProxyType
from typing import Final, Literal

import bpy
from expression import Ok, Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, math, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.coordinates import lensed, pixels
from blender.bpy.render.effects.rows import Alpha, CameraFrame, CompositorEffect, Inspected, Shell
from blender.bpy.render.view_layers.passes import Entity, PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.effects.stylize.edges import Anisotropic, Classic, Edges, FrameWidth, IdSource, Inner, Outer, Paint, Placement, Taper, WorldWidth
from blender.imaging.parameters import Gated

# --- [CONSTANTS] ------------------------------------------------------------------------

_ENGINE: Final = Descriptor.declared(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)
_NORMAL: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_normal", bpy.types.BoolProperty)
_ID_PASSES: Final = MappingProxyType({
    IdSource.CRYPTOMATTE: {entity: (entity.flag, f"{entity.pass_name}00") for entity in (Entity.OBJECT, Entity.MATERIAL)},
    IdSource.INDEX: {
        entity: (Descriptor.declared(bpy.types.ViewLayer, flag, bpy.types.BoolProperty), f"{entity.label} Index")
        for entity, flag in ((Entity.OBJECT, "use_pass_object_index"), (Entity.MATERIAL, "use_pass_material_index"))
    },
})

# --- [OPERATIONS] -----------------------------------------------------------------------


def read_ids(record: Edges) -> tuple[Entity, ...]:
    """Returns object and material ID terms the record enables."""
    return tuple(entity for entity, enabled in ((Entity.OBJECT, record.outer.value.objects), (Entity.MATERIAL, record.inner.value.materials)) if enabled)


def requested(record: Edges, layer: str) -> BlenderResult[PassRequest]:
    """Requests depth, normal, and the enabled ID passes with the engines that write them.

    Args:
        record: Entry's rendered guide selection.
        layer: View layer of Render Layers node.

    Returns:
        Depth and normal request with selected ID passes, or the Depth descriptor fault.
    """
    entities = read_ids(record)
    return PassRequest.depth(layer, precise=bool(entities)).map(
        lambda request: msgspec.structs.replace(
            request,
            flags=request.flags | {_NORMAL, *(_ID_PASSES[record.source][entity][0] for entity in entities)},
            condition=Some(Setting(descriptor=_ENGINE, accepted=("CYCLES",) if entities and record.source is IdSource.INDEX else ("CYCLES", "BLENDER_EEVEE"))),
        )
    )


def condition(record: Edges) -> Condition:
    """Returns camera requirement of world widths."""
    return camera_type("PERSP", "ORTHO") if any(isinstance(line.width, WorldWidth) for line in (record.outer.value, record.inner.value)) else AllOf(conditions=())


def outlined(shell: Shell[Edges]) -> BlenderResult[Expr]:
    """Builds painted regions, rendered guide cores, and nearest-width ink coverage.

    Args:
        shell: Entry image, parameter inputs, render layer, and retained graph fields.

    Returns:
        Inked associated color keeping its line coverage.
    """
    record, frame, source = shell.record, shell.space, shell.color
    paint, outer, inner, ids, taper, paper = Edges.__struct_fields__
    gap, objects, outer_ink, outer_width, placement = Outer.__struct_fields__
    crease, materials, contrast, inner_ink, inner_width = Inner.__struct_fields__
    near, ramp = Taper.__struct_fields__
    (size,) = Paint.__struct_fields__
    _, uniformity, sharpness, eccentricity = Anisotropic.__struct_fields__
    _, high = Classic.__struct_fields__
    _, value = Gated.__struct_fields__
    _, scale, _ = CameraFrame.__struct_fields__
    depth, normals = msgspec.structs.replace(shell.layer("Depth"), shape="FLOAT"), msgspec.structs.replace(shell.layer("Normal"), shape="VECTOR")

    def filtered(image: Scalar, kind: Literal["Laplace", "Sobel", "Soften"]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Fac", 1.0), ("Type", kind)), socket="Image", shape="RGBA")

    def dilated(mask: Scalar, steps: int) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDilateErode, inputs=(("Mask", mask), ("Size", steps), ("Type", "Steps")), socket="Mask")

    def kuwahara(case: type[Anisotropic | Classic], *settings: tuple[str, Scalar]) -> Expr:
        sized = pixels(frame, shell.input(paint, value, case, size))
        return Expr(kind=bpy.types.CompositorNodeKuwahara, inputs=(("Image", source), ("Type", case.title), ("Size", sized), *settings), socket="Image", shape="RGBA")

    brushes = {
        Anisotropic.title: kuwahara(
            Anisotropic,
            ("Uniformity", math("ROUND", pixels(frame, shell.input(paint, value, Anisotropic, uniformity)))),
            ("Sharpness", shell.input(paint, value, Anisotropic, sharpness)),
            ("Eccentricity", shell.input(paint, value, Anisotropic, eccentricity)),
        ),
        Classic.title: kuwahara(Classic, ("High Precision", shell.input(paint, value, Classic, high))),
    }
    painted = switched(shell.gate(paint), source, shell.menu(paint, value, cases=brushes, kind="RGBA"))
    canvas = switched(shell.gate(paper), painted, shell.input(paper))
    orthographic, inverse, depth_gap = lensed("Is Orthographic"), math("DIVIDE", 1.0, depth), shell.input(outer, value, gap)
    coordinate = switched(orthographic, inverse, depth)
    occluding = math("MULTIPLY", math("DIVIDE", filtered(coordinate, "Laplace"), coordinate), 8.0 / (3.0 * sqrt(2.0))) > depth_gap
    turning = (math("GREATER_THAN", dilated(vector("LENGTH", normals), -1), 0.0)) * (vector("LENGTH", filtered(normals, "Sobel")) > math("SINE", shell.input(inner, value, crease) * 0.5) * 8.0)
    contrasting = math("DIVIDE", filtered(painted, "Sobel"), 4.0) > shell.input(inner, value, contrast)
    nearer = math("LESS_THAN", filtered(inverse, "Soften"), inverse * math("MULTIPLY_ADD", depth_gap, -0.25, 1.0))
    passes = {entity: shell.layer(_ID_PASSES[record.source][entity][1])["Red"] for entity in read_ids(record)}
    sources = {choice.label: xyz(**{axis: passes[entity] for axis, entity in (("x", Entity.OBJECT), ("y", Entity.MATERIAL)) if choice is record.source and entity in passes}) for choice in IdSource}
    identities = shell.menu(ids, cases=sources, kind="VECTOR")

    def changed(axis: Literal["X", "Y"], gate: Expr) -> Expr:
        return switched(gate, 0.0, dilated(identities[axis], 1) > dilated(identities[axis], -1))

    outer_core = math("MAXIMUM", switched(shell.gate(outer, value, gap), 0.0, occluding), changed("X", shell.input(outer, value, objects)))
    inner_terms = math(
        "MAXIMUM",
        math("MAXIMUM", switched(shell.gate(inner, value, crease), 0.0, turning), changed("Y", shell.input(inner, value, materials))),
        switched(shell.gate(inner, value, contrast), 0.0, contrasting),
    )
    inner_core = math("SUBTRACT", inner_terms, outer_core, clamp=True)
    nearest = dilated(inverse, 1)
    projection = Expr(kind=bpy.types.FunctionNodeSeparateMatrix, inputs=(("Matrix", lensed("Projection Matrix")),))
    focal = math("MINIMUM", *(abs(msgspec.structs.replace(projection, socket=f"Column {index} Row {index}")) for index in (1, 2))) * 0.5 / shell.input(Shell.frame, scale)
    world_scale = switched(orthographic, focal * nearest, focal)
    tapering = Expr(
        kind=bpy.types.ShaderNodeValToRGB,
        inputs=(("Fac", math("MULTIPLY", shell.input(taper, value, near), nearest, clamp=True)),),
        content=Some(record.taper.value.ramp),
        socket="Color",
        shape="RGBA",
    ).named(item_key(taper, value, ramp))
    tapered = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", tapering),), members=(("mode", "HSV"),), socket="Blue")

    def reach(*path: str) -> Expr:
        world, limit = WorldWidth.__struct_fields__
        (width,) = FrameWidth.__struct_fields__
        reached = pixels(
            frame,
            shell.menu(
                *path,
                cases={
                    FrameWidth.title: shell.input(*path, FrameWidth, width),
                    WorldWidth.title: math("MINIMUM", shell.input(*path, WorldWidth, world) * world_scale, shell.input(*path, WorldWidth, limit)),
                },
                kind="FLOAT",
            ),
        )
        return switched(shell.gate(taper), reached, reached * tapered)

    def field(core: Expr, extent: Expr) -> tuple[Expr, Expr]:
        signed = Expr(kind=bpy.types.CompositorNodeMaskToSDF, inputs=(("Mask", core),), socket="SDF")
        seeded = core * extent
        cell = vector("DIVIDE", (1.0, 1.0, 0.0), Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", seeded),), socket="Dimensions", shape="VECTOR"))
        uv = vector("MULTIPLY_ADD", msgspec.structs.replace(signed, socket="Nearest Pixel", shape="VECTOR"), cell, vector("MULTIPLY_ADD", cell, (0.5, 0.5, 0.0), (0.0, 0.0, 1.0)))
        sample = Expr(
            kind=bpy.types.CompositorNodeMapUV, inputs=(("Image", seeded), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")), socket="Image", shape="RGBA"
        )
        return signed, sample["Red"]

    def centered(core: Expr, extent: Expr) -> Expr:
        signed, width = field(core, extent)
        return math("MULTIPLY_ADD", signed, -1.0, width * 0.5, clamp=True)

    outer_reach = reach(outer, value, outer_width)
    near_core = outer_core * nearer
    (near_sdf, near_size), (far_sdf, far_size) = field(near_core, outer_reach), field(outer_core - near_core, outer_reach)
    claimed = (near_size > 0.0) * math("MULTIPLY_ADD", far_sdf - near_sdf, 0.5, 0.5, clamp=True)
    placed = shell.menu(
        outer,
        value,
        placement,
        cases={
            Placement.CENTER.label: centered(outer_core, outer_reach),
            Placement.INSIDE.label: math("SUBTRACT", near_size, near_sdf, clamp=True) * claimed,
            Placement.OUTSIDE.label: math("SUBTRACT", far_size, far_sdf, clamp=True) * (1.0 - claimed),
        },
        kind="FLOAT",
    )

    def inked(line: str, ink: str, coverage: Expr, below: Expr) -> tuple[Expr, Expr]:
        smooth = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", coverage),), socket="Image", shape="RGBA")
        over = Expr(kind=bpy.types.CompositorNodeAlphaOver, inputs=(("Background", below), ("Foreground", shell.input(line, value, ink)), ("Fac", smooth)), socket="Image", shape="RGBA")
        return switched(shell.gate(line), below, over), math("MULTIPLY", smooth, shell.gate(line))

    inner_drawn, inner_shown = inked(inner, inner_ink, centered(inner_core, reach(inner, value, inner_width)), canvas)
    outer_drawn, outer_shown = inked(outer, outer_ink, placed, inner_drawn)
    return Ok(outer_drawn.keeping(math("MAXIMUM", inner_shown, outer_shown).named("lines")))


# --- [COMPOSITION] ----------------------------------------------------------------------

edges = CompositorEffect(
    record=Edges,
    graph=outlined,
    alpha=Alpha.RESULT,
    requested=Some(requested),
    condition=Some(condition),
    inspected=(Inspected(key="lines", end=End(node="lines"), description="Rendered line coverage"),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["condition", "edges", "outlined", "read_ids", "requested"]
