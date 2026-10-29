"""Ink lines from rendered depth, identity, normal, and image discontinuities over optional edge-preserving paint."""

from enum import auto, Enum
from types import MappingProxyType
from typing import Annotated, Final, Literal

import bpy
from expression import Ok, Some
import msgspec
import numpy as np

from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.graphs import cell, Expr, math, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor.coordinates import dimensions, pixels
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.inputs import FRAME_SCALE
from blender.bpy.render.compositor.lenses import lensed
from blender.bpy.render.compositor.shells import Shell
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

_ANISOTROPIC: Final = MenuItem(name="Anisotropic", description="Kuwahara filtering over elliptical sectors along local structure")
_CLASSIC: Final = MenuItem(name="Classic", description="Kuwahara filtering by least varied quadrant mean")
_FRAME_WIDTH: Final = MenuItem(name="Frame", description="Line width as a frame share")
_WORLD_WIDTH: Final = MenuItem(name="World", description="Line width as a surface length, narrowing with camera distance")
_CENTER: Final = MenuItem(name="Center")
_INSIDE: Final = MenuItem(name="Inside")
_OUTSIDE: Final = MenuItem(name="Outside")
_ENGINE: Final = Descriptor.declared(bpy.types.Scene, ("render", "engine"), bpy.types.EnumProperty)
_NORMAL: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_normal",), bpy.types.BoolProperty)
_ID_PASSES: Final = MappingProxyType({
    IdSource.CRYPTOMATTE: {entity: (ENTITIES[entity].flag, f"{ENTITIES[entity].pass_name}00") for entity in (Entity.OBJECT, Entity.MATERIAL)},
    IdSource.INDEX: {
        entity: (Descriptor.declared(bpy.types.ViewLayer, (flag,), bpy.types.BoolProperty), f"{ENTITIES[entity].name} Index")
        for entity, flag in ((Entity.OBJECT, "use_pass_object_index"), (Entity.MATERIAL, "use_pass_material_index"))
    },
})
_PAINT_BRUSH: Final = Socket(
    key="paint_brush",
    name="Brush",
    kind=bpy.types.NodeSocketMenu,
    description="Kuwahara filter flattening texture into painted regions",
    default_value=_ANISOTROPIC.name,
    items=Some(MenuItems(items=(_ANISOTROPIC, _CLASSIC))),
)
_PAINT_SIZE: Final = Socket(key="paint_size", name="Size", kind=bpy.types.NodeSocketFloat, description="Filter window radius over greater frame side", min_value=0.0, default_value=1 / 320)
_UNIFORMITY: Final = Socket(
    key="paint_anisotropic_uniformity", name="Uniformity", kind=bpy.types.NodeSocketFloat, description="Stroke direction smoothing radius", min_value=0.0, default_value=1 / 480
)
_SHARPNESS: Final = Socket(
    key="paint_anisotropic_sharpness",
    name="Sharpness",
    kind=bpy.types.NodeSocketFloatFactor,
    description="Weight toward least varied sectors, sharpest at 1",
    min_value=0.0,
    max_value=1.0,
    default_value=1.0,
)
_ECCENTRICITY: Final = Socket(
    key="paint_anisotropic_eccentricity",
    name="Eccentricity",
    kind=bpy.types.NodeSocketFloat,
    description="Sector elongation along edges, longest at 2",
    min_value=0.0,
    max_value=2.0,
    default_value=1.0,
)
_HIGH_PRECISION: Final = Socket(key="paint_classic_high_precision", name="High Precision", kind=bpy.types.NodeSocketBool, description="Direct quadrant sums instead of summed-area tables")
_PAINT: Final = InterfacePanel(
    key="paint",
    name="Paint",
    description="Edge-preserving smoothing before lines",
    toggle=Some(False),
    inputs=(_PAINT_BRUSH, _PAINT_SIZE),
    panels=(
        InterfacePanel(key="paint_anisotropic", name="Anisotropic", inputs=(_UNIFORMITY, _SHARPNESS, _ECCENTRICITY)),
        InterfacePanel(key="paint_classic", name="Classic", inputs=(_HIGH_PRECISION,)),
    ),
)
_LINES: Final = ("outer", "inner")
_OUTER_INK, _INNER_INK = (
    Socket(key=item_key(line, "ink"), name="Ink", kind=bpy.types.NodeSocketColor, description=f"{bpy.path.display_name(line)} line display color", default_value=(0.0, 0.0, 0.0, 1.0))
    for line in _LINES
)
_OUTER_WIDTH, _INNER_WIDTH = (
    Socket(
        key=item_key(line, "width"),
        name="Width",
        kind=bpy.types.NodeSocketMenu,
        description=f"{bpy.path.display_name(line)} line width as a frame share or a surface length",
        default_value=_FRAME_WIDTH.name,
        items=Some(MenuItems(items=(_FRAME_WIDTH, _WORLD_WIDTH))),
    )
    for line in _LINES
)
_OUTER_FRAME_WIDTH, _INNER_FRAME_WIDTH = (
    Socket(key=item_key(line, "frame_width"), name="Frame Width", kind=bpy.types.NodeSocketFloat, description="Line width over greater frame side", min_value=0.0, default_value=default)
    for line, default in zip(_LINES, (1 / 640, 1 / 1280), strict=True)
)
_OUTER_WORLD_WIDTH, _INNER_WORLD_WIDTH = (
    Socket(key=item_key(line, "world_width"), name="World Width", kind=bpy.types.NodeSocketFloatDistance, description="Line width on surface", min_value=0.0, default_value=0.00635) for line in _LINES
)
_OUTER_LIMIT, _INNER_LIMIT = (
    Socket(key=item_key(line, "limit"), name="Limit", kind=bpy.types.NodeSocketFloat, description="Width limit near camera", min_value=0.0, default_value=1 / 240) for line in _LINES
)
_OUTER_OBJECTS: Final = Socket(key="outer_objects", name="Object Changes", kind=bpy.types.NodeSocketBool, description="Lines at object boundaries")
_INNER_MATERIALS: Final = Socket(key="inner_materials", name="Material Changes", kind=bpy.types.NodeSocketBool, description="Lines at material boundaries")
_OUTER_PLACEMENT: Final = Socket(
    key="outer_placement",
    name="Placement",
    kind=bpy.types.NodeSocketMenu,
    description="Contour side outer lines cover, inside on nearer surface",
    default_value=_CENTER.name,
    items=Some(MenuItems(items=(_CENTER, _INSIDE, _OUTSIDE))),
)
_DEPTH_GAP_VALUE: Final = Socket(key="outer_depth_gap_value", name="Depth Gap", kind=bpy.types.NodeSocketFloatFactor, min_value=0.0, max_value=1.0, default_value=0.05)
_DEPTH_GAP: Final = InterfacePanel(key="outer_depth_gap", name="Depth Gap", description="Least relative gap (1 - near/far) at occluding surface", toggle=Some(True), inputs=(_DEPTH_GAP_VALUE,))
_CREASE_ANGLE_VALUE: Final = Socket(
    key="inner_crease_angle_value", name="Crease Angle", kind=bpy.types.NodeSocketFloatAngle, min_value=0.0, max_value=np.pi, default_value=float(np.pi - np.radians(134.4))
)
_CREASE_ANGLE: Final = InterfacePanel(key="inner_crease_angle", name="Crease Angle", description="Least neighboring normal angle drawing a line", toggle=Some(True), inputs=(_CREASE_ANGLE_VALUE,))
_CONTRAST_VALUE: Final = Socket(key="inner_contrast_value", name="Contrast", kind=bpy.types.NodeSocketFloatFactor, min_value=0.0, max_value=1.0, default_value=0.1)
_CONTRAST: Final = InterfacePanel(key="inner_contrast", name="Contrast", description="Least luminance-weighted display change, 1 from black to white", toggle=Some(False), inputs=(_CONTRAST_VALUE,))
_OUTER: Final = InterfacePanel(
    key="outer",
    name="Outer Lines",
    description="Lines at occluding surfaces and object boundaries",
    toggle=Some(True),
    inputs=(_OUTER_OBJECTS, _OUTER_INK, _OUTER_WIDTH, _OUTER_FRAME_WIDTH, _OUTER_WORLD_WIDTH, _OUTER_LIMIT, _OUTER_PLACEMENT),
    panels=(_DEPTH_GAP,),
)
_INNER: Final = InterfacePanel(
    key="inner",
    name="Inner Lines",
    description="Lines at surface turns, material boundaries, and image contrast",
    toggle=Some(True),
    inputs=(_INNER_MATERIALS, _INNER_INK, _INNER_WIDTH, _INNER_FRAME_WIDTH, _INNER_WORLD_WIDTH, _INNER_LIMIT),
    panels=(_CREASE_ANGLE, _CONTRAST),
)
_TAPER_NEAR: Final = Socket(
    key="taper_near", name="Near", kind=bpy.types.NodeSocketFloatDistance, description="Distance at ramp position 1, depth d reading near / d", min_value=1e-6, default_value=3.048
)
_TAPER: Final = InterfacePanel(key="taper", name="Depth Taper", description="Width scaling with line depth", toggle=Some(False), inputs=(_TAPER_NEAR,))
_PAPER_VALUE: Final = Socket(key="paper_value", name="Paper", kind=bpy.types.NodeSocketColor, default_value=(1.0, 1.0, 1.0, 1.0))
_PAPER: Final = InterfacePanel(key="paper", name="Paper", description="Display color beneath lines, replacing image", toggle=Some(False), inputs=(_PAPER_VALUE,))

# --- [MODELS] ---------------------------------------------------------------------------


class Edges(Effect, frozen=True, kw_only=True):
    """Ink lines from depth, identity, normal, and image changes over optional paint, identity read from the source passes and widths tapered through a ramp over line depth."""

    stage = Stage.EDGES
    pointwise = False
    domain = Domain.DISPLAY
    source: Annotated[IdSource, msgspec.Meta(description="Pass identifying objects and materials")] = IdSource.CRYPTOMATTE
    taper: Annotated[Ramp, msgspec.Meta(description="Width factor over near / depth, read as each color value")] = Ramp()


# --- [OPERATIONS] -----------------------------------------------------------------------


def requested(record: Edges, layer: str) -> Outcome[PassRequest]:
    """Requests depth, normal, and the source's identity passes with the engines that write them."""
    request = PassRequest.depth(layer, precise=True)
    return Ok(
        msgspec.structs.replace(
            request,
            flags=request.flags | {_NORMAL, *(flag for flag, _ in _ID_PASSES[record.source].values())},
            condition=Some(Setting(descriptor=_ENGINE, accepted=("CYCLES",) if record.source is IdSource.INDEX else ("CYCLES", "BLENDER_EEVEE"))),
        )
    )


def _edges(shell: Shell[Edges]) -> Outcome[Expr]:
    """Builds painted regions, rendered guide cores, and nearest-width ink coverage.

    Args:
        shell: Entry image, parameter inputs, render layer, and record taper and source.

    Returns:
        Inked associated color keeping its line coverage, or a menu fault.
    """
    record, frame, source = shell.record, shell.space, shell.color
    depth, normals = msgspec.structs.replace(shell.layer("Depth"), shape=bpy.types.NodeSocketFloat), msgspec.structs.replace(shell.layer("Normal"), shape=bpy.types.NodeSocketVector)

    def filtered(image: Scalar, kind: Literal["Laplace", "Sobel", "Soften"]) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeFilter, inputs=(("Image", image), ("Fac", 1.0), ("Type", kind)), socket="Image", shape=bpy.types.NodeSocketColor)

    def dilated(mask: Scalar, steps: int) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDilateErode, inputs=(("Mask", mask), ("Size", steps), ("Type", "Steps")), socket="Mask")

    def kuwahara(kind: str, *settings: tuple[str, Scalar]) -> Expr:
        return Expr(
            kind=bpy.types.CompositorNodeKuwahara,
            inputs=(("Image", source), ("Type", kind), ("Size", pixels(frame, shell.input(_PAINT_SIZE))), *settings),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )

    brushes = {
        _ANISOTROPIC: kuwahara(
            "Anisotropic", ("Uniformity", math("ROUND", pixels(frame, shell.input(_UNIFORMITY)))), ("Sharpness", shell.input(_SHARPNESS)), ("Eccentricity", shell.input(_ECCENTRICITY))
        ),
        _CLASSIC: kuwahara("Classic", ("High Precision", shell.input(_HIGH_PRECISION))),
    }
    orthographic, inverse, depth_gap = lensed("Is Orthographic"), math("DIVIDE", 1.0, depth), shell.input(_DEPTH_GAP_VALUE)
    coordinate = switched(orthographic, inverse, depth)
    occluding = math("MULTIPLY", math("DIVIDE", filtered(coordinate, "Laplace"), coordinate), 8.0 / (3.0 * float(np.sqrt(2.0)))) > depth_gap
    turning = (math("GREATER_THAN", dilated(vector("LENGTH", normals), -1), 0.0)) * (vector("LENGTH", filtered(normals, "Sobel")) > math("SINE", shell.input(_CREASE_ANGLE_VALUE) * 0.5) * 8.0)
    nearer = math("LESS_THAN", filtered(inverse, "Soften"), inverse * math("MULTIPLY_ADD", depth_gap, -0.25, 1.0))
    passes = {entity: shell.layer(name)["Red"] for entity, (_, name) in _ID_PASSES[record.source].items()}
    identities = xyz(passes[Entity.OBJECT], passes[Entity.MATERIAL])

    def changed(axis: Literal["X", "Y"], gate: Expr) -> Expr:
        return switched(gate, 0.0, dilated(identities[axis], 1) > dilated(identities[axis], -1))

    outer_core = math("MAXIMUM", switched(shell.input(_DEPTH_GAP), 0.0, occluding), changed("X", shell.input(_OUTER_OBJECTS)))
    nearest = dilated(inverse, 1)
    projection = lensed("Projection Matrix")
    focal = math("MINIMUM", *(abs(cell(projection, index, index)) for index in (1, 2))) * 0.5 / shell.input(FRAME_SCALE)
    world_scale = switched(orthographic, focal * nearest, focal)
    tapering = Expr(
        kind=bpy.types.ShaderNodeValToRGB,
        inputs=(("Fac", math("MULTIPLY", shell.input(_TAPER_NEAR), nearest, clamp=True)),),
        content=Some(record.taper),
        socket="Color",
        shape=bpy.types.NodeSocketColor,
    ).named("taper_ramp")
    tapered = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", tapering),), members={"mode": "HSV"}, socket="Blue")

    def reach(width: Socket, frame_width: Socket, world_width: Socket, limit: Socket) -> Expr:
        chosen = shell.menu(width, cases={_FRAME_WIDTH: shell.input(frame_width), _WORLD_WIDTH: math("MINIMUM", shell.input(world_width) * world_scale, shell.input(limit))})
        return switched(shell.input(_TAPER), pixels(frame, chosen), pixels(frame, chosen) * tapered)

    def field(core: Expr, extent: Expr) -> tuple[Expr, Expr]:
        signed = Expr(kind=bpy.types.CompositorNodeMaskToSDF, inputs=(("Mask", core),), socket="SDF")
        seeded = core * extent
        step = vector("DIVIDE", (1.0, 1.0, 0.0), dimensions(seeded))
        uv = vector("MULTIPLY_ADD", msgspec.structs.replace(signed, socket="Nearest Pixel", shape=bpy.types.NodeSocketVector), step, vector("MULTIPLY_ADD", step, (0.5, 0.5, 0.0), (0.0, 0.0, 1.0)))
        sample = Expr(
            kind=bpy.types.CompositorNodeMapUV,
            inputs=(("Image", seeded), ("UV", uv), ("Interpolation", "Nearest"), ("Extension X", "Clip"), ("Extension Y", "Clip")),
            socket="Image",
            shape=bpy.types.NodeSocketColor,
        )
        return signed, sample["Red"]

    def centered(core: Expr, extent: Expr) -> Expr:
        signed, width = field(core, extent)
        return math("MULTIPLY_ADD", signed, -1.0, width * 0.5, clamp=True)

    def inked(line: InterfacePanel, ink: Socket, coverage: Expr, below: Expr) -> tuple[Expr, Expr]:
        smooth = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", coverage),), socket="Image", shape=bpy.types.NodeSocketColor)
        over = Expr(kind=bpy.types.CompositorNodeAlphaOver, inputs=(("Background", below), ("Foreground", shell.input(ink)), ("Fac", smooth)), socket="Image", shape=bpy.types.NodeSocketColor)
        return switched(shell.input(line), below, over), math("MULTIPLY", smooth, shell.input(line))

    def drawn(painted: Expr, outer_reach: Expr, inner_reach: Expr) -> Expr:
        canvas = switched(shell.input(_PAPER), painted, shell.input(_PAPER_VALUE))
        contrasting = math("DIVIDE", filtered(painted, "Sobel"), 4.0) > shell.input(_CONTRAST_VALUE)
        inner_terms = math(
            "MAXIMUM", math("MAXIMUM", switched(shell.input(_CREASE_ANGLE), 0.0, turning), changed("Y", shell.input(_INNER_MATERIALS))), switched(shell.input(_CONTRAST), 0.0, contrasting)
        )
        inner_core = math("SUBTRACT", inner_terms, outer_core, clamp=True)
        near_core = outer_core * nearer
        (near_sdf, near_size), (far_sdf, far_size) = field(near_core, outer_reach), field(outer_core - near_core, outer_reach)
        claimed = (near_size > 0.0) * math("MULTIPLY_ADD", far_sdf - near_sdf, 0.5, 0.5, clamp=True)
        placements = {
            _CENTER: centered(outer_core, outer_reach),
            _INSIDE: math("SUBTRACT", near_size, near_sdf, clamp=True) * claimed,
            _OUTSIDE: math("SUBTRACT", far_size, far_sdf, clamp=True) * (1.0 - claimed),
        }
        inner_drawn, inner_shown = inked(_INNER, _INNER_INK, centered(inner_core, inner_reach), canvas)
        outer_drawn, outer_shown = inked(_OUTER, _OUTER_INK, shell.menu(_OUTER_PLACEMENT, cases=placements), inner_drawn)
        return outer_drawn.keeping(math("MAXIMUM", inner_shown, outer_shown).named("lines"))

    brushed = shell.menu(_PAINT_BRUSH, cases=brushes)
    outer_reach, inner_reach = reach(_OUTER_WIDTH, _OUTER_FRAME_WIDTH, _OUTER_WORLD_WIDTH, _OUTER_LIMIT), reach(_INNER_WIDTH, _INNER_FRAME_WIDTH, _INNER_WORLD_WIDTH, _INNER_LIMIT)
    return Ok(drawn(switched(shell.input(_PAINT), source, brushed), outer_reach, inner_reach))


edges: Final = CompositorEffect(graph=_edges, record=Edges, interface=Interface(panels=(_PAINT, _OUTER, _INNER, _TAPER, _PAPER)), mix=Blended(alpha=Alpha.RESULT), request=Some(requested))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Edges", "IdSource", "edges"]
