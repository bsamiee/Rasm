"""Ink lines from rendered depth, identity, normal, and image discontinuities over optional edge-preserving paint."""

from enum import auto, Enum
from types import MappingProxyType
from typing import Annotated, Final, Literal

import bpy
from expression import Some
import msgspec
import numpy as np

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.nodes.graphs import Expr, math, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor.coordinates import lensed, pixels
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Requests, Shell
from blender.bpy.render.view_layers.passes import ENTITIES, Entity, PassRequest
from blender.imaging.color.processors import Domain
from blender.records.faults import Outcome

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class IdSource(Enum):
    """Passes object and material terms compare."""

    CRYPTOMATTE = auto()
    INDEX = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

_ENGINE: Final = Descriptor.declared(bpy.types.Scene, ("render", "engine"), bpy.types.EnumProperty)
_NORMAL: Final = Descriptor.declared(bpy.types.ViewLayer, "use_pass_normal", bpy.types.BoolProperty)
_ID_PASSES: Final = MappingProxyType({
    IdSource.CRYPTOMATTE: {entity: (ENTITIES[entity].flag, f"{ENTITIES[entity].pass_name}00") for entity in (Entity.OBJECT, Entity.MATERIAL)},
    IdSource.INDEX: {
        entity: (Descriptor.declared(bpy.types.ViewLayer, flag, bpy.types.BoolProperty), f"{ENTITIES[entity].name} Index")
        for entity, flag in ((Entity.OBJECT, "use_pass_object_index"), (Entity.MATERIAL, "use_pass_material_index"))
    },
})

# --- [MODELS] ---------------------------------------------------------------------------


class Edges(Effect, frozen=True, kw_only=True):
    """Ink lines from depth, identity, normal, and image changes over optional paint, identity read from the source passes and widths tapered through a ramp over line depth."""

    stage = Stage.EDGES
    pointwise = False
    source: Annotated[IdSource, msgspec.Meta(description="Pass identifying objects and materials")] = IdSource.CRYPTOMATTE
    taper: Annotated[Ramp, msgspec.Meta(description="Width factor over near / depth, read as each color value")] = Ramp()


# --- [OPERATIONS] -----------------------------------------------------------------------


def requested(record: Edges, layer: str) -> Outcome[PassRequest]:
    """Requests depth, normal, and the source's identity passes with the engines that write them."""
    return PassRequest.depth(layer, precise=True).map(
        lambda request: msgspec.structs.replace(
            request,
            flags=request.flags | {_NORMAL, *(flag for flag, _ in _ID_PASSES[record.source].values())},
            condition=Some(Setting(descriptor=_ENGINE, accepted=("CYCLES",) if record.source is IdSource.INDEX else ("CYCLES", "BLENDER_EEVEE"))),
        )
    )


def outlined(shell: Shell[Edges]) -> Outcome[Expr]:
    """Builds painted regions, rendered guide cores, and nearest-width ink coverage.

    Args:
        shell: Entry image, parameter inputs, render layer, and record taper and source.

    Returns:
        Inked associated color keeping its line coverage, or a menu fault.
    """
    record, frame, source = shell.record, shell.space, shell.color
    depth, normals = msgspec.structs.replace(shell.layer("Depth"), shape="FLOAT"), msgspec.structs.replace(shell.layer("Normal"), shape="VECTOR")

    def filtered(image: Scalar, kind: Literal["Laplace", "Sobel", "Soften"]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Fac", 1.0), ("Type", kind)), socket="Image", shape="RGBA")

    def dilated(mask: Scalar, steps: int) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDilateErode, inputs=(("Mask", mask), ("Size", steps), ("Type", "Steps")), socket="Mask")

    def kuwahara(kind: str, *settings: tuple[str, Scalar]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeKuwahara, inputs=(("Image", source), ("Type", kind), ("Size", pixels(frame, shell.input("paint", "size"))), *settings), socket="Image", shape="RGBA")

    brushes = {
        "Anisotropic": kuwahara(
            "Anisotropic",
            ("Uniformity", math("ROUND", pixels(frame, shell.input("paint", "anisotropic", "uniformity")))),
            ("Sharpness", shell.input("paint", "anisotropic", "sharpness")),
            ("Eccentricity", shell.input("paint", "anisotropic", "eccentricity")),
        ),
        "Classic": kuwahara("Classic", ("High Precision", shell.input("paint", "classic", "high_precision"))),
    }
    orthographic, inverse, depth_gap = lensed("Is Orthographic"), math("DIVIDE", 1.0, depth), shell.input("outer", "depth_gap", "value")
    coordinate = switched(orthographic, inverse, depth)
    occluding = math("MULTIPLY", math("DIVIDE", filtered(coordinate, "Laplace"), coordinate), 8.0 / (3.0 * float(np.sqrt(2.0)))) > depth_gap
    turning = (math("GREATER_THAN", dilated(vector("LENGTH", normals), -1), 0.0)) * (
        vector("LENGTH", filtered(normals, "Sobel")) > math("SINE", shell.input("inner", "crease_angle", "value") * 0.5) * 8.0
    )
    nearer = math("LESS_THAN", filtered(inverse, "Soften"), inverse * math("MULTIPLY_ADD", depth_gap, -0.25, 1.0))
    passes = {entity: shell.layer(name)["Red"] for entity, (_, name) in _ID_PASSES[record.source].items()}
    identities = xyz(passes[Entity.OBJECT], passes[Entity.MATERIAL])

    def changed(axis: Literal["X", "Y"], gate: Expr) -> Expr:
        return switched(gate, 0.0, dilated(identities[axis], 1) > dilated(identities[axis], -1))

    outer_core = math("MAXIMUM", switched(shell.input("outer", "depth_gap"), 0.0, occluding), changed("X", shell.input("outer", "objects")))
    nearest = dilated(inverse, 1)
    projection = Expr(kind=bpy.types.FunctionNodeSeparateMatrix, inputs=(("Matrix", lensed("Projection Matrix")),))
    focal = math("MINIMUM", *(abs(msgspec.structs.replace(projection, socket=f"Column {index} Row {index}")) for index in (1, 2))) * 0.5 / shell.input(Shell.frame, "scale")
    world_scale = switched(orthographic, focal * nearest, focal)
    tapering = Expr(
        kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", math("MULTIPLY", shell.input("taper", "near"), nearest, clamp=True)),), content=Some(record.taper), socket="Color", shape="RGBA"
    ).named("taper_ramp")
    tapered = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", tapering),), members=(("mode", "HSV"),), socket="Blue")

    def reach(line: str) -> Outcome[Expr]:
        cases = {"Frame": shell.input(line, "frame_width"), "World": math("MINIMUM", shell.input(line, "world_width") * world_scale, shell.input(line, "limit"))}
        return shell.menu(line, "width", cases=cases, kind="FLOAT").map(lambda width: switched(shell.input("taper"), pixels(frame, width), pixels(frame, width) * tapered))

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

    def inked(line: str, coverage: Expr, below: Expr) -> tuple[Expr, Expr]:
        smooth = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", coverage),), socket="Image", shape="RGBA")
        over = Expr(kind=bpy.types.CompositorNodeAlphaOver, inputs=(("Background", below), ("Foreground", shell.input(line, "ink")), ("Fac", smooth)), socket="Image", shape="RGBA")
        return switched(shell.input(line), below, over), math("MULTIPLY", smooth, shell.input(line))

    def drawn(painted: Expr, outer_reach: Expr, inner_reach: Expr) -> Outcome[Expr]:
        canvas = switched(shell.input("paper"), painted, shell.input("paper", "value"))
        contrasting = math("DIVIDE", filtered(painted, "Sobel"), 4.0) > shell.input("inner", "contrast", "value")
        inner_terms = math(
            "MAXIMUM",
            math("MAXIMUM", switched(shell.input("inner", "crease_angle"), 0.0, turning), changed("Y", shell.input("inner", "materials"))),
            switched(shell.input("inner", "contrast"), 0.0, contrasting),
        )
        inner_core = math("SUBTRACT", inner_terms, outer_core, clamp=True)
        near_core = outer_core * nearer
        (near_sdf, near_size), (far_sdf, far_size) = field(near_core, outer_reach), field(outer_core - near_core, outer_reach)
        claimed = (near_size > 0.0) * math("MULTIPLY_ADD", far_sdf - near_sdf, 0.5, 0.5, clamp=True)
        placements = {
            "Center": centered(outer_core, outer_reach),
            "Inside": math("SUBTRACT", near_size, near_sdf, clamp=True) * claimed,
            "Outside": math("SUBTRACT", far_size, far_sdf, clamp=True) * (1.0 - claimed),
        }

        def lined(placed: Expr) -> Expr:
            inner_drawn, inner_shown = inked("inner", centered(inner_core, inner_reach), canvas)
            outer_drawn, outer_shown = inked("outer", placed, inner_drawn)
            return outer_drawn.keeping(math("MAXIMUM", inner_shown, outer_shown).named("lines"))

        return shell.menu("outer", "placement", cases=placements, kind="FLOAT").map(lined)

    return shell.menu("paint", "brush", cases=brushes, kind="RGBA").bind(
        lambda brushed: reach("outer").bind(lambda outer_reach: reach("inner").bind(lambda inner_reach: drawn(switched(shell.input("paint"), source, brushed), outer_reach, inner_reach)))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

edges: Final = CompositorEffect(
    record=Edges,
    graph=outlined,
    interface=Interface(
        panels=(
            InterfacePanel(
                key="paint",
                name="Paint",
                description="Edge-preserving smoothing before lines",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="paint_brush",
                        name="Brush",
                        kind=bpy.types.NodeSocketMenu,
                        description="Kuwahara filter flattening texture into painted regions",
                        members={"default_value": "Anisotropic"},
                        items=Some(
                            MenuItems(
                                items=(
                                    MenuItem(name="Anisotropic", description="Kuwahara filtering over elliptical sectors along local structure"),
                                    MenuItem(name="Classic", description="Kuwahara filtering by least varied quadrant mean"),
                                )
                            )
                        ),
                    ),
                    Socket(
                        key="paint_size", name="Size", kind=bpy.types.NodeSocketFloat, description="Filter window radius over greater frame side", members={"min_value": 0.0, "default_value": 1 / 320}
                    ),
                ),
                panels=(
                    InterfacePanel(
                        key="paint_anisotropic",
                        name="Anisotropic",
                        inputs=tuple(
                            Socket(
                                key=item_key("paint_anisotropic", field),
                                name=bpy.path.display_name(field),
                                kind=bpy.types.NodeSocketFloat,
                                description=description,
                                members={"subtype": subtype, **members},
                            )
                            for field, subtype, description, members in (
                                ("uniformity", "NONE", "Stroke direction smoothing radius", {"min_value": 0.0, "default_value": 1 / 480}),
                                ("sharpness", "FACTOR", "Weight toward least varied sectors, sharpest at 1", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                                ("eccentricity", "NONE", "Sector elongation along edges, longest at 2", {"min_value": 0.0, "max_value": 2.0, "default_value": 1.0}),
                            )
                        ),
                    ),
                    InterfacePanel(
                        key="paint_classic",
                        name="Classic",
                        inputs=(Socket(key="paint_classic_high_precision", name="High Precision", kind=bpy.types.NodeSocketBool, description="Direct quadrant sums instead of summed-area tables"),),
                    ),
                ),
            ),
            *(
                InterfacePanel(
                    key=line,
                    name=f"{bpy.path.display_name(line)} Lines",
                    description=description,
                    toggle=Some(True),
                    inputs=(
                        changes,
                        Socket(
                            key=item_key(line, "ink"),
                            name="Ink",
                            kind=bpy.types.NodeSocketColor,
                            description=f"{bpy.path.display_name(line)} line display color",
                            members={"default_value": (0.0, 0.0, 0.0, 1.0)},
                        ),
                        Socket(
                            key=item_key(line, "width"),
                            name="Width",
                            kind=bpy.types.NodeSocketMenu,
                            description=f"{bpy.path.display_name(line)} line width as a frame share or a surface length",
                            members={"default_value": "Frame"},
                            items=Some(
                                MenuItems(
                                    items=(
                                        MenuItem(name="Frame", description="Line width as a frame share"),
                                        MenuItem(name="World", description="Line width as a surface length, narrowing with camera distance"),
                                    )
                                )
                            ),
                        ),
                        *(
                            Socket(
                                key=item_key(line, field),
                                name=bpy.path.display_name(field),
                                kind=bpy.types.NodeSocketFloat,
                                description=summary,
                                members={"subtype": subtype, "min_value": 0.0, "default_value": default},
                            )
                            for field, subtype, summary, default in (
                                ("frame_width", "NONE", "Line width over greater frame side", frame),
                                ("world_width", "DISTANCE", "Line width on surface", 0.00635),
                                ("limit", "NONE", "Width limit near camera", 1 / 240),
                            )
                        ),
                        *placement,
                    ),
                    panels=tuple(
                        InterfacePanel(
                            key=item_key(line, field),
                            name=bpy.path.display_name(field),
                            description=summary,
                            toggle=Some(enabled),
                            inputs=(
                                Socket(
                                    key=item_key(line, field, "value"),
                                    name=bpy.path.display_name(field),
                                    kind=bpy.types.NodeSocketFloat,
                                    members={"subtype": subtype, "min_value": 0.0, "max_value": high, "default_value": default},
                                ),
                            ),
                        )
                        for field, summary, enabled, subtype, high, default in gates
                    ),
                )
                for line, description, frame, changes, placement, gates in (
                    (
                        "outer",
                        "Lines at occluding surfaces and object boundaries",
                        1 / 640,
                        Socket(key="outer_objects", name="Object Changes", kind=bpy.types.NodeSocketBool, description="Lines at object boundaries"),
                        (
                            Socket(
                                key="outer_placement",
                                name="Placement",
                                kind=bpy.types.NodeSocketMenu,
                                description="Contour side outer lines cover, inside on nearer surface",
                                members={"default_value": "Center"},
                                items=Some(MenuItems(items=(MenuItem(name="Center"), MenuItem(name="Inside"), MenuItem(name="Outside")))),
                            ),
                        ),
                        (("depth_gap", "Least relative gap (1 - near/far) at occluding surface", True, "FACTOR", 1.0, 0.05),),
                    ),
                    (
                        "inner",
                        "Lines at surface turns, material boundaries, and image contrast",
                        1 / 1280,
                        Socket(key="inner_materials", name="Material Changes", kind=bpy.types.NodeSocketBool, description="Lines at material boundaries"),
                        (),
                        (
                            ("crease_angle", "Least neighboring normal angle drawing a line", True, "ANGLE", np.pi, float(np.pi - np.radians(134.4))),
                            ("contrast", "Least luminance-weighted display change, 1 from black to white", False, "FACTOR", 1.0, 0.1),
                        ),
                    ),
                )
            ),
            InterfacePanel(
                key="taper",
                name="Depth Taper",
                description="Width scaling with line depth",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="taper_near",
                        name="Near",
                        kind=bpy.types.NodeSocketFloat,
                        description="Distance at ramp position 1, depth d reading near / d",
                        members={"subtype": "DISTANCE", "min_value": 1e-6, "default_value": 3.048},
                    ),
                ),
            ),
            InterfacePanel(
                key="paper",
                name="Paper",
                description="Display color beneath lines, replacing image",
                toggle=Some(False),
                inputs=(Socket(key="paper_value", name="Paper", kind=bpy.types.NodeSocketColor, members={"default_value": (1.0, 1.0, 1.0, 1.0)}),),
            ),
        )
    ),
    domain=Domain.DISPLAY,
    alpha=Alpha.RESULT,
    parts=(Requests(request=requested),),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Edges", "IdSource", "edges"]
