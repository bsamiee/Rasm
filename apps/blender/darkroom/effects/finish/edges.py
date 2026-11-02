"""Lines from rendered depth, identity, normal, and image discontinuities over optional edge-preserving paint."""

from enum import auto, Enum
from functools import partial
from typing import Annotated, Final

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np

from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.desired import MenuItem, MenuItems
from blender.bpy.nodes.graphs import cell, Component, Expr, math, Scalar, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor.coordinates import dimensions, uv_mapped
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect
from blender.bpy.render.compositor.filters import filtered
from blender.bpy.render.compositor.inputs import FRAME_SCALE, GraphInputs
from blender.bpy.render.compositor.lenses import camera_info
from blender.bpy.render.view_layers.passes import CryptomatteKind, PassRequest
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


class IdSource(Enum):
    """Passes object and material terms compare."""

    CRYPTOMATTE = auto()
    INDEX = auto()

    def requested(self, layer: str) -> PassRequest:
        """Requests the object and material passes of the source with the engines writing them."""
        kinds = CryptomatteKind.OBJECT | CryptomatteKind.MATERIAL
        return PassRequest.indexed(layer, kinds) if self is IdSource.INDEX else PassRequest.matted(layer, kinds)

    def pass_name(self, entity: CryptomatteKind) -> Option[str]:
        """Render pass holding the entity's identities: its index pass, or its first Cryptomatte rank."""
        return entity.index.map(lambda held: held[1]) if self is IdSource.INDEX else Some(f"{entity.pass_name}00")


# --- [CONSTANTS] ------------------------------------------------------------------------

_KUWAHARA: Final = bpy.types.CompositorNodeKuwahara
_NORMAL: Final = Descriptor.declared(bpy.types.ViewLayer, ("use_pass_normal",), bpy.types.BoolProperty)
_FRAME_WIDTH: Final = MenuItem(name="Frame", description="Line width as a frame share")
_WORLD_WIDTH: Final = MenuItem(name="World", description="Line width as a surface length, narrowing with camera distance")
_CENTER: Final = MenuItem(name="Center")
_INSIDE: Final = MenuItem(name="Inside")
_OUTSIDE: Final = MenuItem(name="Outside")
_TYPE: Final = Socket(key="type", kind=StockInput(node=_KUWAHARA, input="Type"))
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Filter window radius over greater frame side", min_value=0.0, default_value=1 / 320)
_UNIFORMITY: Final = Socket(key="uniformity", kind=bpy.types.NodeSocketFloat, description="Stroke direction smoothing radius over greater frame side", min_value=0.0, default_value=1 / 480)
_SHARPNESS: Final = Socket(key="sharpness", kind=StockInput(node=_KUWAHARA, input="Sharpness"))
_ECCENTRICITY: Final = Socket(key="eccentricity", kind=StockInput(node=_KUWAHARA, input="Eccentricity"))
_HIGH_PRECISION: Final = Socket(key="high_precision", kind=StockInput(node=_KUWAHARA, input="High Precision"))
_ANISOTROPIC: Final = InterfacePanel.of("anisotropic", Interface(inputs=(_UNIFORMITY, _SHARPNESS, _ECCENTRICITY)))
_CLASSIC: Final = InterfacePanel.of("classic", Interface(inputs=(_HIGH_PRECISION,)))
_PAINT: Final = InterfacePanel.of("paint", Interface(inputs=(_TYPE, _SIZE), panels=(_ANISOTROPIC, _CLASSIC)), description="Kuwahara smoothing preserving edges before lines", toggle=Some(False))
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Line display color", default_value=(0.0, 0.0, 0.0, 1.0))
_WIDTH: Final = Socket(
    key="width",
    kind=bpy.types.NodeSocketMenu,
    description="Line width as a frame share or a surface length",
    default_value=_FRAME_WIDTH.name,
    items=Some(MenuItems(items=(_FRAME_WIDTH, _WORLD_WIDTH))),
)
_FRAME_SHARE: Final = Socket(key="frame_width", kind=bpy.types.NodeSocketFloat, description="Line width over greater frame side", min_value=0.0, default_value=1 / 640)
_WORLD_LENGTH: Final = Socket(key="world_width", kind=bpy.types.NodeSocketFloatDistance, description="Line width on surface", min_value=0.0, default_value=0.00635)
_LIMIT: Final = Socket(key="limit", kind=bpy.types.NodeSocketFloat, description="Width limit near camera over greater frame side", min_value=0.0, default_value=1 / 240)
_LINE: Final = (_COLOR, _WIDTH, _FRAME_SHARE, _WORLD_LENGTH, _LIMIT)
_OBJECTS: Final = Socket(key="objects", name="Object Changes", kind=bpy.types.NodeSocketBool, description="Lines at object boundaries")
_MATERIALS: Final = Socket(key="materials", name="Material Changes", kind=bpy.types.NodeSocketBool, description="Lines at material boundaries")
_PLACEMENT: Final = Socket(
    key="placement",
    kind=bpy.types.NodeSocketMenu,
    description="Contour side outer lines cover, inside on nearer surface",
    default_value=_CENTER.name,
    items=Some(MenuItems(items=(_CENTER, _INSIDE, _OUTSIDE))),
)
_GAP: Final = Socket(key="value", name="Depth Gap", kind=bpy.types.NodeSocketFloatFactor, default_value=0.05)
_DEPTH_GAP: Final = InterfacePanel.of("depth_gap", Interface(inputs=(_GAP,)), description="Least relative gap (1 - near/far) at occluding surface", toggle=Some(True))
_ANGLE: Final = Socket(key="value", name="Crease Angle", kind=bpy.types.NodeSocketFloatAngle, min_value=0.0, max_value=np.pi, default_value=float(np.pi - np.radians(134.4)))
_CREASE_ANGLE: Final = InterfacePanel.of("crease_angle", Interface(inputs=(_ANGLE,)), description="Least neighboring normal angle drawing a line", toggle=Some(True))
_CHANGE: Final = Socket(key="value", name="Contrast", kind=bpy.types.NodeSocketFloatFactor, default_value=0.1)
_CONTRAST: Final = InterfacePanel.of("contrast", Interface(inputs=(_CHANGE,)), description="Least luminance-weighted display change, 1 from black to white", toggle=Some(False))
_OUTER: Final = InterfacePanel.of(
    "outer", Interface(inputs=(_OBJECTS, *_LINE, _PLACEMENT), panels=(_DEPTH_GAP,)), name="Outer Lines", description="Lines at occluding surfaces and object boundaries", toggle=Some(True)
)
_INNER: Final = InterfacePanel.of(
    "inner",
    Interface(inputs=(_MATERIALS, *_LINE), panels=(_CREASE_ANGLE, _CONTRAST)).defaulted({_FRAME_SHARE.key: 1 / 1280}),
    name="Inner Lines",
    description="Lines at surface turns, material boundaries, and image contrast",
    toggle=Some(True),
)
_NEAR: Final = Socket(key="near", kind=bpy.types.NodeSocketFloatDistance, description="Distance at ramp position 1, depth d reading near / d", min_value=1e-6, default_value=3.048)
_TAPER: Final = InterfacePanel.of("taper", Interface(inputs=(_NEAR,)), name="Depth Taper", description="Width scaling with line depth", toggle=Some(False))
_PAPER_COLOR: Final = Socket(key="value", name="Paper", kind=bpy.types.NodeSocketColor, default_value=(1.0, 1.0, 1.0, 1.0))
_PAPER: Final = InterfacePanel.of("paper", Interface(inputs=(_PAPER_COLOR,)), description="Display color beneath lines, replacing image", toggle=Some(False))

# --- [MODELS] ---------------------------------------------------------------------------


class Edges(Case, frozen=True, kw_only=True):
    """Lines from depth, identity, normal, and image changes over optional paint, identity read from the source passes and widths tapered through a ramp over line depth."""

    source: Annotated[IdSource, msgspec.Meta(description="Pass identifying objects and materials")] = IdSource.CRYPTOMATTE
    taper: Annotated[Ramp, msgspec.Meta(description="Width factor over near / depth, read as each color value")] = Ramp()


# --- [OPERATIONS] -----------------------------------------------------------------------


def _requested(record: Edges, layer: str) -> Outcome[PassRequest]:
    """Requests depth, normal, and the source's identity passes with the engines that write them, at full precision."""
    identities = record.source.requested(layer)
    return Ok(msgspec.structs.replace(identities, flags=identities.flags | PassRequest.depth(layer, precise=True).flags | {_NORMAL}, precise=True))


def _edges(shell: GraphInputs[Edges]) -> Outcome[Expr]:
    """Builds painted regions, rendered guide cores, and nearest-width line coverage.

    Args:
        shell: Entry image, parameter inputs, render layer, and record taper and source.

    Returns:
        Lined associated color keeping its line coverage, or the fault of an identity pass the source lacks.
    """
    record, frame, source = shell.record, shell.space, shell.color
    depth, normals = shell.layer("Depth", bpy.types.NodeSocketFloat), shell.layer("Normal", bpy.types.NodeSocketVector)
    paint = partial(shell.input, _PAINT)

    def dilated(mask: Scalar, steps: int) -> Expr:
        return Expr(kind=bpy.types.CompositorNodeDilateErode, inputs=(("Mask", mask), ("Size", steps), ("Type", "Steps")), socket="Mask")

    kuwahara = Expr(
        kind=_KUWAHARA,
        inputs=(
            ("Image", source),
            ("Type", paint(_TYPE)),
            ("Size", frame.pixels(paint(_SIZE))),
            ("Uniformity", math("ROUND", frame.pixels(paint(_ANISOTROPIC, _UNIFORMITY)))),
            ("Sharpness", paint(_ANISOTROPIC, _SHARPNESS)),
            ("Eccentricity", paint(_ANISOTROPIC, _ECCENTRICITY)),
            ("High Precision", paint(_CLASSIC, _HIGH_PRECISION)),
        ),
        socket="Image",
        socket_type=bpy.types.NodeSocketColor,
    )
    painted = switched(shell.input(_PAINT), source, kuwahara)
    orthographic, inverse, depth_gap = camera_info("Is Orthographic"), math("DIVIDE", 1.0, depth), shell.input(_OUTER, _DEPTH_GAP, _GAP)
    coordinate = switched(orthographic, inverse, depth)
    occluding = math("MULTIPLY", math("DIVIDE", filtered(coordinate, "Laplace"), coordinate), 8.0 / (3.0 * float(np.sqrt(2.0)))) > depth_gap
    turning = (math("GREATER_THAN", dilated(vector("LENGTH", normals), -1), 0.0)) * (
        vector("LENGTH", filtered(normals, "Sobel")) > math("SINE", shell.input(_INNER, _CREASE_ANGLE, _ANGLE) * 0.5) * 8.0
    )
    nearer = math("LESS_THAN", filtered(inverse, "Soften"), inverse * math("MULTIPLY_ADD", depth_gap, -0.25, 1.0))
    nearest = dilated(inverse, 1)
    focal = math("MINIMUM", *(abs(cell(camera_info("Projection Matrix"), index, index)) for index in (1, 2))) * 0.5 / shell.input(FRAME_SCALE)
    world_scale = switched(orthographic, focal * nearest, focal)
    tapering = Expr(
        kind=bpy.types.ShaderNodeValToRGB,
        inputs=(("Fac", math("MULTIPLY", shell.input(_TAPER, _NEAR), nearest, clamp=True)),),
        content=Some(record.taper),
        socket="Color",
        socket_type=bpy.types.NodeSocketColor,
    ).keyed("taper_ramp")
    tapered = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", tapering),), members={"mode": "HSV"}, socket="Blue")

    def reach(line: InterfacePanel) -> Expr:
        chosen = shell.menu(
            line, _WIDTH, cases={_FRAME_WIDTH: shell.input(line, _FRAME_SHARE), _WORLD_WIDTH: math("MINIMUM", shell.input(line, _WORLD_LENGTH) * world_scale, shell.input(line, _LIMIT))}
        )
        return switched(shell.input(_TAPER), frame.pixels(chosen), frame.pixels(chosen) * tapered)

    def field(core: Expr, extent: Expr) -> tuple[Expr, Expr]:
        signed = Expr(kind=bpy.types.CompositorNodeMaskToSDF, inputs=(("Mask", core),), socket="SDF")
        seeded = core * extent
        step = vector("DIVIDE", (1.0, 1.0, 0.0), dimensions(seeded))
        uv = vector("MULTIPLY_ADD", signed.at("Nearest Pixel", bpy.types.NodeSocketVector), step, vector("MULTIPLY_ADD", step, (0.5, 0.5, 0.0), (0.0, 0.0, 1.0)))
        return signed, uv_mapped(seeded, uv, interpolation="Nearest")["Red"]

    def centered(core: Expr, extent: Expr) -> Expr:
        signed, width = field(core, extent)
        return math("MULTIPLY_ADD", signed, -1.0, width * 0.5, clamp=True)

    def overlaid(line: InterfacePanel, coverage: Expr, below: Expr) -> tuple[Expr, Expr]:
        smooth = Expr(kind=bpy.types.CompositorNodeAntiAliasing, inputs=(("Image", coverage),), socket="Image", socket_type=bpy.types.NodeSocketColor)
        over = Expr(
            kind=bpy.types.CompositorNodeAlphaOver, inputs=(("Background", below), ("Foreground", shell.input(line, _COLOR)), ("Fac", smooth)), socket="Image", socket_type=bpy.types.NodeSocketColor
        )
        return switched(shell.input(line), below, over), math("MULTIPLY", smooth, shell.input(line))

    def drawn(passes: tuple[str, ...]) -> Expr:
        identities = xyz(*(shell.layer(name)["Red"] for name in passes))

        def changed(axis: Component, gate: Expr) -> Expr:
            return switched(gate, 0.0, dilated(identities[axis], 1) > dilated(identities[axis], -1))

        canvas = switched(shell.input(_PAPER), painted, shell.input(_PAPER, _PAPER_COLOR))
        outer_core = math("MAXIMUM", switched(shell.input(_OUTER, _DEPTH_GAP), 0.0, occluding), changed("X", shell.input(_OUTER, _OBJECTS)))
        contrasting = math("DIVIDE", filtered(painted, "Sobel"), 4.0) > shell.input(_INNER, _CONTRAST, _CHANGE)
        inner_terms = math(
            "MAXIMUM",
            math("MAXIMUM", switched(shell.input(_INNER, _CREASE_ANGLE), 0.0, turning), changed("Y", shell.input(_INNER, _MATERIALS))),
            switched(shell.input(_INNER, _CONTRAST), 0.0, contrasting),
        )
        inner_core = math("SUBTRACT", inner_terms, outer_core, clamp=True)
        near_core = outer_core * nearer
        outer_reach = reach(_OUTER)
        (near_sdf, near_size), (far_sdf, far_size) = field(near_core, outer_reach), field(outer_core - near_core, outer_reach)
        claimed = (near_size > 0.0) * math("MULTIPLY_ADD", far_sdf - near_sdf, 0.5, 0.5, clamp=True)
        placements = {
            _CENTER: centered(outer_core, outer_reach),
            _INSIDE: math("SUBTRACT", near_size, near_sdf, clamp=True) * claimed,
            _OUTSIDE: math("SUBTRACT", far_size, far_sdf, clamp=True) * (1.0 - claimed),
        }
        inner_drawn, inner_shown = overlaid(_INNER, centered(inner_core, reach(_INNER)), canvas)
        outer_drawn, outer_shown = overlaid(_OUTER, shell.menu(_OUTER, _PLACEMENT, cases=placements), inner_drawn)
        return outer_drawn.keeping(math("MAXIMUM", inner_shown, outer_shown).keyed("lines"))

    return accumulated(
        record.source.pass_name(entity).to_result_with(partial(Fault.of, Reason.ABSENT, entity.label, member=record.source.name)) for entity in (CryptomatteKind.OBJECT, CryptomatteKind.MATERIAL)
    ).map(drawn)


# --- [COMPOSITION] ----------------------------------------------------------------------

edges: Final = CompositorEffect(_edges, record=Edges, interface=Interface(panels=(_PAINT, _OUTER, _INNER, _TAPER, _PAPER)), mix=Blended(alpha=Alpha.RESULT), request=Some(_requested))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Edges", "IdSource", "edges"]
