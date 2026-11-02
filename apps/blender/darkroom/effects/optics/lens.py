"""Lens optics of the frame: variable-radius bokeh through the iris, circular radial distortion with its ST map export, lateral spectral aberration with axial iris defocus, and edge falloff."""

from collections.abc import Mapping
from enum import auto, Enum
from functools import partial
from itertools import starmap
from math import pi
import sys
from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Save
from blender.bpy.commands.operators import Answer, Call, Done, NoUndo, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.paths import rooted
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.desired import item_key, MenuItem, MenuItems
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import Component, Expr, math, mixed, rgba, switched, vector
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor import lenses, masks
from blender.bpy.render.compositor.colors import with_alpha
from blender.bpy.render.compositor.coordinates import Border, BORDERS, extended, shorter
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, depth_precise, Named
from blender.bpy.render.compositor.inputs import captured, chosen, entered, FRAME_ASPECT, FRAME_METERS, FRAME_SCALE, GraphInputs, Holds, path_key, value, valued
from blender.bpy.render.compositor.shapes import turned
from blender.bpy.render.outputs.framing import FrameSize, Framing, Whole
from blender.imaging.files.images import ImageFile, Storage
from blender.imaging.geometry.arithmetic import Elementwise, Namespace
from blender.imaging.geometry.frames import StMap
from blender.imaging.geometry.lenses import Aperture, Radial
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type _Coefficient = Annotated[float, msgspec.Meta(gt=-1.0, le=sys.float_info.max, description="Barrel above 0, pincushion below 0")]


class _Direction(Enum):
    """Distort a rectilinear image, or undistort a lens image."""

    DISTORT = auto()
    UNDISTORT = auto()


# --- [CONSTANTS] ------------------------------------------------------------------------

# --- [DEFOCUS]

_F_STOP: Final = Socket(key="f_stop", name="F-Stop", kind=bpy.types.NodeSocketFloat, description="Focal length over aperture diameter", min_value=0.0, default_value=2.8)
_ELLIPSE_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Sharp ellipse center", default_value=(0.5, 0.5))
_ELLIPSE_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Ellipse turn from horizontal", min_value=-pi / 2.0, max_value=pi / 2.0)
_ELLIPSE_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketVector2D, description="Ellipse half extents along rotated axes", min_value=0.0, default_value=(0.4, 0.4))
_ELLIPSE_FEATHER: Final = Socket(key="feather", kind=bpy.types.NodeSocketFloat, description="Outward distance to full defocus", min_value=0.0, default_value=0.2)
_BAND_CENTER: Final = Socket(key="center", kind=bpy.types.NodeSocketVector2D, description="Point sharp band passes through", default_value=(0.5, 0.5))
_BAND_ANGLE: Final = Socket(key="angle", kind=bpy.types.NodeSocketFloatAngle, description="Band turn from horizontal", min_value=-pi / 2.0, max_value=pi / 2.0)
_BAND_WIDTH: Final = Socket(key="width", kind=bpy.types.NodeSocketFloat, description="Sharp band width", min_value=0.0, default_value=0.16875)
_BAND_FEATHER: Final = Socket(key="feather", kind=bpy.types.NodeSocketFloat, description="Outward distance to full defocus", min_value=0.0, default_value=0.140625)
_DEPTH_OF_FIELD: Final = InterfacePanel.of("depth_of_field", Interface(inputs=(_F_STOP,)), description="Thin-lens circle of confusion from camera and depth pass")
_RADIAL: Final = InterfacePanel.of("radial", Interface(inputs=(_ELLIPSE_CENTER, _ELLIPSE_ANGLE, _ELLIPSE_SIZE, _ELLIPSE_FEATHER)), description="Defocus growing quadratically beyond a sharp ellipse")
_TILT_SHIFT: Final = InterfacePanel.of("tilt_shift", Interface(inputs=(_BAND_CENTER, _BAND_ANGLE, _BAND_WIDTH, _BAND_FEATHER)), description="Defocus growing linearly beyond a tilted sharp band")
_FOCI: Final = Interface(panels=(_DEPTH_OF_FIELD, _RADIAL, _TILT_SHIFT))
_FOCUS: Final = Socket(key="focus", kind=bpy.types.NodeSocketMenu, description="Bokeh size source", default_value=_DEPTH_OF_FIELD.title, items=Some(_FOCI.menu))
_MAX_BLUR: Final = Socket(key="max_blur", kind=bpy.types.NodeSocketFloat, description="Largest bokeh radius", min_value=0.0, default_value=0.02)
_FRINGE: Final = Socket(key="fringe", kind=bpy.types.NodeSocketFloat, description="Bokeh rim color shift, toward blue above 0", min_value=-1.0, max_value=1.0)

# --- [DISTORTION]

_DISTORTION: Final = Socket.of("distortion", _Coefficient, 0.05).value
_FIT: Final = Socket(key="fit", kind=bpy.types.NodeSocketBool, description="Barrel fit to frame corners")
_DIRECTION: Final = Socket(
    key="direction", kind=bpy.types.NodeSocketMenu, description="Distort rectilinear or undistort lens image", default_value=MenuItem.of(_Direction.DISTORT).name, items=Some(MenuItems.of(_Direction))
)
_EXTENSION: Final = Socket(key="extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", default_value=MenuItem.of(Border.BLACK).name, items=Some(BORDERS))
_DISPERSION: Final = Socket(key="dispersion", kind=bpy.types.NodeSocketFloat, description="Image height spread from red to blue", min_value=0.0, default_value=0.05)
_AXIAL: Final = Socket(key="axial", kind=bpy.types.NodeSocketFloat, description="Blue defocus radius, red at half and green in focus", min_value=0.0)
_AXIAL_SHARES: Final[tuple[tuple[Component, float], ...]] = (("Red", 0.5), ("Green", 0.0), ("Blue", 1.0))
_SPECTRAL_EXTENSION: Final = msgspec.structs.replace(_EXTENSION, default_value=MenuItem.of(Border.CLAMP).name)

# --- [VIGNETTE]

_NATURAL: Final = MenuItem(name="Natural", description="Relative illumination as fourth power of chief-ray cosine")
_SHAPE_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Scene-linear color outside shape", default_value=(0.0, 0.0, 0.0, 1.0))
_, _, _SHAPE_SIZE, _, _SHAPE_FEATHER = masks.SHAPE.inputs
_SHAPED: Final = InterfacePanel.of(
    "shaped",
    Interface(inputs=(*masks.SHAPE.defaulted({_SHAPE_SIZE.key: (0.8, 0.45), _SHAPE_FEATHER.key: 0.2}).inputs, _SHAPE_COLOR)),
    description="Light kept inside a feathered outline, falling to a color outside",
)
_FALLOFF: Final = Socket(key="falloff", kind=bpy.types.NodeSocketMenu, description="Lens falloff or drawn outline", default_value=_NATURAL.name, items=Some(MenuItems(items=(_NATURAL, _SHAPED.item))))

# --- [MODELS] ---------------------------------------------------------------------------


class Defocus(Case, frozen=True, kw_only=True):
    """Variable-radius lens blur through the iris."""


class Distortion(Case, frozen=True, kw_only=True):
    """Barrel or pincushion lens distortion with edge or corner framing."""


class Aberration(Case, frozen=True, kw_only=True):
    """Lateral spectral integration about green, then red and blue iris defocus."""

    samples: Annotated[int, msgspec.Meta(ge=2, description="Wavelengths integrated from red to blue")] = 8

    @property
    def spectrum(self) -> tuple[tuple[float, tuple[float, ...]], ...]:
        """Distortion coefficient per unit dispersion and channel-normalized midpoint wavelength weights of each sample."""
        across = 2.0 * (np.arange(self.samples, dtype=np.float64) + 0.5) / self.samples - 1.0
        weights = np.stack((np.maximum(-across, 0.0), 1.0 - np.abs(across), np.maximum(across, 0.0)), axis=-1)
        return tuple(zip((-0.25 * across).tolist(), map(tuple, (weights / weights.sum(axis=0)).tolist())))


class Vignette(Case, frozen=True, kw_only=True):
    """Frame-edge falloff from camera chief rays or a feathered outline."""


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DEFOCUS]


def match_camera_iris(call: Call[Named]) -> Outcome[Answer]:
    """Set f-stop and iris from scene camera."""
    iris = partial(item_key, lenses.IRIS.key)

    def copied(camera: tuple[str, bpy.types.CameraDOFSettings], entry: bpy.types.SceneCompositorEffect) -> Outcome[Answer]:
        name, dof = camera
        common: dict[Key, Value] = {_FOCUS.key: _DEPTH_OF_FIELD.title, path_key(_DEPTH_OF_FIELD, _F_STOP): dof.aperture_fstop, iris(Aperture.rotation): dof.aperture_rotation}
        outline: dict[Key, Value] = {iris(Aperture.roundness): 1.0} if dof.aperture_blades == 0 else {iris(Aperture.blades): dof.aperture_blades, iris(Aperture.roundness): 0.0}
        written = entered(call.identity, entry, common | outline) if dof.aperture_fstop > 0.0 else Error(Fault.of(Reason.OUTSIDE, _F_STOP.key, str(dof.aperture_fstop), member="positive f-numbers"))
        return written.map(lambda changed: Done(notes=(("INFO", f"Copied f-stop and iris from {name}"),)) if changed else Unchanged(reason=f"{entry.name} matches {name} f-stop and iris"))

    def matched(scene: bpy.types.Scene) -> Outcome[Answer]:
        camera: Option[tuple[str, bpy.types.CameraDOFSettings]] = Option.of_optional(scene.camera).bind(
            lambda held: Option.of_optional(held.data.dof).map(lambda dof: (held.name, dof)) if isinstance(held.data, bpy.types.Camera) else Nothing
        )
        return paired(camera.to_result_with(partial(Fault.of, Reason.ABSENT, repr(scene), member="camera")), defocus.named(call.identity, scene, call.properties.name)).bind(
            lambda found: copied(*found)
        )

    return rooted(bpy.types.Scene, call.context).bind(matched)


def _defocused(shell: GraphInputs[Defocus]) -> Outcome[Expr]:
    """Gathers the stage input through the iris at the selected radius field."""
    read, at = shell.input, shell.space
    cap = read(_MAX_BLUR)
    projected = lenses.confusion(shell.layer("Depth"), read(_DEPTH_OF_FIELD, _F_STOP), read(FRAME_METERS), cap)
    spread = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", projected), ("Size", at.pixels(cap))), socket="Image")
    size = read(_RADIAL, _ELLIPSE_SIZE)
    local = turned(at, read(_RADIAL, _ELLIPSE_CENTER), read(_RADIAL, _ELLIPSE_ANGLE))
    curvature = math("DIVIDE", size["Y"] * math("MAXIMUM", vector("LENGTH", local / size) - 1.0, 0.0), read(_RADIAL, _ELLIPSE_FEATHER), clamp=True)
    across = abs(turned(at, read(_TILT_SHIFT, _BAND_CENTER), read(_TILT_SHIFT, _BAND_ANGLE))["Y"])
    tilt = math("DIVIDE", math("MULTIPLY_ADD", read(_TILT_SHIFT, _BAND_WIDTH), -0.5, across), read(_TILT_SHIFT, _BAND_FEATHER), clamp=True)
    radius = shell.menu(_FOCUS, cases={_DEPTH_OF_FIELD.item: math("MINIMUM", spread, projected), _RADIAL.item: curvature * curvature * cap, _TILT_SHIFT.item: tilt * cap})
    bokeh = lenses.iris(read, shift=read(_FRINGE))
    return Ok(
        Expr(
            kind=bpy.types.CompositorNodeBokehBlur,
            inputs=(("Image", shell.color), ("Bokeh", bokeh), ("Size", at.pixels(radius.keyed("select")))),
            socket="Image",
            socket_type=bpy.types.NodeSocketColor,
        )
    )


# --- [DISTORTION]


def _zoom[V: Elementwise, B](distortion: V, fit: V | float, smaller: V | float, ns: Namespace[V, B]) -> V:
    """Zoom keeping the frame edge in place, or a barrel's frame corner while `fit` holds, `smaller` the shorter frame side over the greater."""
    return 1.0 / (distortion * (ns.where(ns.greater(distortion, 0.0), fit * smaller * smaller, 0.0) + 1.0) + 1.0)


def export_st_map(call: Call[Named]) -> Outcome[Done]:
    """Write lens map as float OpenEXR data."""

    def baked(read: tuple[float, bool, _Direction, Border], framing: Framing, size: tuple[int, int]) -> Outcome[Done]:
        coefficient, fit, direction, border = read
        smaller = min(framing.aspect, 1.0) / max(framing.aspect, 1.0)
        held = np.asarray(coefficient)
        lens = Radial(distortion=held, scale=_zoom(held, float(fit), smaller, np), aspect=framing.aspect)
        forward = lens.to_rectilinear if direction is _Direction.DISTORT else lens.to_lens

        def mapping(points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return np.stack(forward((points[..., 0], points[..., 1], points[..., 2]), np), axis=-1)

        plane = StMap.bake(*size, mapping, (border.wrap, border.wrap), source_size=size).plane
        return ImageFile(pixels=plane, colorspace=PyOpenColorIO.ROLE_DATA).written(call.paths[0], Storage(), {}, PyOpenColorIO.GetCurrentConfig()).map(lambda path: Done.wrote((path,)))

    def read(values: Mapping[Key, Value]) -> Outcome[tuple[float, bool, _Direction, Border]]:
        return paired(value(values, _Coefficient, _DISTORTION), value(values, bool, _FIT), chosen(values, _Direction, _DIRECTION), chosen(values, Border, _EXTENSION))

    def ready(scene: bpy.types.Scene) -> Outcome[Done]:
        held = distortion.named(call.identity, scene, call.properties.name).bind(lambda entry: captured(call.identity, entry)).map(valued).bind(read)
        framing = FrameSize().bound(call.identity).map(lambda framed: framed.read(scene, Some(Whole()))).bind(lambda found: found.rendered())
        return paired(held, framing).bind(lambda parts: baked(parts[0], *parts[1]))

    return rooted(bpy.types.Scene, call.context).bind(ready)


def _distorted(shell: GraphInputs[Distortion]) -> Outcome[Expr]:
    """Samples the stage input through the fitted lens map or its covered inverse."""
    frame, coefficient = shell.space, shell.input(_DISTORTION)
    lens = Radial(distortion=coefficient, scale=_zoom(coefficient, shell.input(_FIT), shorter(frame.extent), EXPRESSIONS), aspect=1.0)
    positions = shell.menu(_DIRECTION, cases={MenuItem.of(_Direction.DISTORT): lenses.radial(frame, lens.to_rectilinear), MenuItem.of(_Direction.UNDISTORT): lenses.radial(frame, lens.to_lens)})
    return Ok(extended(shell.color, frame.covered(positions), shell.input(_EXTENSION), clipped=False))


def _aberrated(shell: GraphInputs[Aberration]) -> Outcome[Expr]:
    """Integrates the spectral lens maps, then gathers red and blue axial light through the iris."""
    frame, dispersion, border = shell.space, shell.input(_DISPERSION), shell.input(_SPECTRAL_EXTENSION)
    samples = tuple(
        (extended(shell.color, frame.covered(lenses.radial(frame, Radial(distortion=dispersion * factor, scale=1.0, aspect=1.0).to_rectilinear)), border, clipped=False), weight)
        for factor, weight in shell.record.spectrum
    )
    colors = tuple(vector("MULTIPLY", sample, weight) for sample, weight in samples)
    alphas = tuple(sample["Alpha"] * (1.0 / shell.record.samples) for sample, _ in samples)
    lateral = with_alpha(sum(colors[1:], colors[0]), sum(alphas[1:], alphas[0]))
    bokeh = lenses.iris(shell.input)

    def gathered(channel: Component, share: float) -> Expr:
        size = frame.pixels(shell.input(_AXIAL) * share)
        return Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", lateral[channel]), ("Bokeh", bokeh), ("Size", size)), socket="Image") if share > 0.0 else lateral[channel]

    red, green, blue = starmap(gathered, _AXIAL_SHARES)
    return Ok(rgba(red, green, blue, lateral["Alpha"]))


# --- [VIGNETTE]


def _vignetted(shell: GraphInputs[Vignette]) -> Outcome[Expr]:
    """Mixes the stage input toward the pull color by the selected falloff weight."""
    source = shell.color

    def built(outline: Expr) -> Expr:
        weight = shell.menu(_FALLOFF, cases={_NATURAL: lenses.chief_ray(source) ** 4.0, _SHAPED.item: outline})
        pull = switched(weight.at(_SHAPED.title), (0.0, 0.0, 0.0, 1.0), shell.input(_SHAPED, _SHAPE_COLOR))
        return mixed(weight, with_alpha(pull, source["Alpha"], masked=True), source)

    return masks.outlined(source, shell.input(FRAME_ASPECT), shell.input(FRAME_SCALE), lambda socket: shell.input(_SHAPED, socket)).map(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

defocus: Final = CompositorEffect(
    _defocused,
    record=Defocus,
    interface=Interface(inputs=(_FOCUS, _MAX_BLUR, _FRINGE), panels=(*_FOCI.panels, lenses.IRIS)),
    mix=Blended(alpha=Alpha.RESULT),
    request=Some(depth_precise),
    holds=Holds(cameras=Snapshot.of(bpy.types.Camera, {("dof", "use_dof"): False})),
    registers=(Operator(form=Named, run=match_camera_iris, icon=Some("CAMERA_DATA")),),
)
distortion: Final = CompositorEffect(
    _distorted,
    record=Distortion,
    interface=Interface(inputs=(_DISTORTION, _FIT, _DIRECTION, _EXTENSION)),
    mix=Blended(alpha=Alpha.RESULT),
    registers=(Operator(form=Named, run=export_st_map, invoke=Some(Save(extensions=lambda _: (".exr",))), undo=NoUndo()),),
)
aberration: Final = CompositorEffect(_aberrated, record=Aberration, interface=Interface(inputs=(_DISPERSION, _AXIAL, _SPECTRAL_EXTENSION), panels=(lenses.IRIS,)), mix=Blended(alpha=Alpha.RESULT))
vignette: Final = CompositorEffect(_vignetted, record=Vignette, interface=Interface(inputs=(_FALLOFF,), panels=(_SHAPED,)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aberration", "Defocus", "Distortion", "Vignette", "aberration", "defocus", "distortion", "export_st_map", "match_camera_iris", "vignette"]
