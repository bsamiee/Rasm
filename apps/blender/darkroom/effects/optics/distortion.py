"""Circular radial lens distortion with corner framing, its ST map export, and lateral spectral aberration with axial iris defocus."""

from collections.abc import Mapping
import sys
from typing import Annotated, ClassVar, Final, Literal, overload

import bpy
from expression import Error, Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Save
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Key
from blender.bpy.nodes.graphs import Expr, math, rgba, vector, xyz
from blender.bpy.nodes.interface import Interface, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import Border, BORDERS, covered, extended, FrameSpace, pixels, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Named, Registers, Shell
from blender.bpy.render.compositor.inputs import captured
from blender.bpy.render.outputs.framing import FrameSize, Framing, Whole
from blender.imaging.files.images import ImageFile, Storage
from blender.imaging.geometry.lenses import Radial
from blender.imaging.geometry.stmaps import StMap
from blender.records.faults import Fault, Outcome, paired, RecordReason
from blender.records.fields import NonNegative

from ...chain.stages import Effect, Stage
from .defocus import IRIS

# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLUTION: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Distortion(Effect, frozen=True, kw_only=True):
    """Barrel or pincushion lens distortion with edge or corner framing."""

    stage = Stage.DISTORTION
    pointwise = False
    distortion: Annotated[float, msgspec.Meta(gt=-1.0, le=sys.float_info.max, description="Barrel above 0, pincushion below 0")] = 0.05

    @overload
    def scale(self, reach: float) -> float: ...
    @overload
    def scale(self, reach: Expr) -> Expr: ...
    def scale(self, reach: float | Expr) -> float | Expr:
        """Zoom keeping the frame edge, or its corner at `reach` past the edge, in place."""
        return 1.0 / (1.0 / reach + self.distortion) / reach


class Aberration(Effect, frozen=True, kw_only=True):
    """Lateral spectral integration about green, then red and blue iris defocus."""

    stage = Stage.DISTORTION
    pointwise = False
    defocus: ClassVar[tuple[float, float, float]] = (0.5, 0.0, 1.0)
    dispersion: Annotated[NonNegative, msgspec.Meta(description="Image height spread from red to blue")] = 0.05
    samples: Annotated[int, msgspec.Meta(ge=2, description="Wavelengths integrated from red to blue")] = 8

    @property
    def spectrum(self) -> tuple[tuple[float, tuple[float, ...]], ...]:
        """Distortion coefficient and channel-normalized midpoint wavelength weights of each sample."""
        across = 2.0 * (np.arange(self.samples, dtype=np.float64) + 0.5) / self.samples - 1.0
        weights = np.stack((np.maximum(-across, 0.0), 1.0 - np.abs(across), np.maximum(across, 0.0)), axis=-1)
        return tuple(zip((-0.25 * self.dispersion * across).tolist(), map(tuple, (weights / weights.sum(axis=0)).tolist())))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LENS]


def lensed(frame: FrameSpace, distortion: float, scale: Expr | float, direction: Literal["rectilinear", "lens"]) -> Expr:
    """Maps displayed frame positions through the radial lens of a distortion and zoom, camera frame fractions out with real-branch coverage in Z.

    The imaging map reads frame fractions at unit aspect, so displayed offsets from the frame center enter as fractions about 0.5; the zoom
    scales offsets entering the rectilinear map and leaving the lens map, which equals the map at that zoom.

    Returns:
        Camera frame fractions with coverage in Z.
    """
    offset = frame.point - frame.extent * 0.5
    radial = Radial(distortion=distortion, scale=1.0, aspect=1.0)
    match direction:
        case "rectilinear":
            held = lenses.mapped(vector("ADD", vector("SCALE", offset, scale), (0.5, 0.5, 1.0)), radial.to_rectilinear)
            across, up = held["X"] - 0.5, held["Y"] - 0.5
        case "lens":
            held = lenses.mapped(vector("ADD", offset, (0.5, 0.5, 1.0)), radial.to_lens)
            across, up = (held["X"] - 0.5) / scale, (held["Y"] - 0.5) / scale
    fraction = vector("DIVIDE", xyz(across, up) + frame.extent * 0.5, frame.extent)
    return xyz(fraction["X"], fraction["Y"], held["Z"])


def distorted(shell: Shell[Distortion]) -> Outcome[Expr]:
    """Samples the stage input through the fitted lens map or its covered inverse."""
    frame, record = shell.space, shell.record
    smaller = shorter(frame.extent)
    zoom = record.scale(math("MULTIPLY_ADD", shell.input("fit") * float(record.distortion > 0.0), smaller * smaller, 1.0))
    border = shell.input("extension")
    return shell.menu("direction", cases={"Distort": lensed(frame, record.distortion, zoom, "rectilinear"), "Undistort": lensed(frame, record.distortion, zoom, "lens")}, kind="VECTOR").map(
        lambda positions: extended(shell.color, covered(frame, positions), (border, border), clipped=False)
    )


def aberrated(shell: Shell[Aberration]) -> Outcome[Expr]:
    """Integrates the spectral lens maps, then gathers red and blue axial light through the iris."""
    frame = shell.space
    border = shell.input("extension")
    samples = tuple((extended(shell.color, covered(frame, lensed(frame, coefficient, 1.0, "rectilinear")), (border, border), clipped=False), weight) for coefficient, weight in shell.record.spectrum)
    colors = tuple(vector("MULTIPLY", sample, weight) for sample, weight in samples)
    alphas = tuple(sample["Alpha"] * (1.0 / shell.record.samples) for sample, _ in samples)
    lateral = Expr(
        kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", sum(colors[1:], colors[0])), ("Alpha", sum(alphas[1:], alphas[0])), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"
    )
    bokeh = lenses.iris(lambda field: shell.input("iris", field.name))

    def gathered(channel: Literal["Red", "Green", "Blue"], ratio: float) -> Expr:
        return (
            Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", lateral[channel]), ("Bokeh", bokeh), ("Size", pixels(frame, shell.input("axial") * ratio))), socket="Image")
            if ratio > 0.0
            else lateral[channel]
        )

    red, green, blue = Aberration.defocus
    return Ok(rgba(gathered("Red", red), gathered("Green", green), gathered("Blue", blue), lateral["Alpha"]))


# --- [EXPORT]


def export_st_map(call: Call[Named]) -> Outcome[Done]:
    """Write lens map as float OpenEXR data."""

    def written(record: Distortion, values: Mapping[Key, Value], framing: Framing, size: tuple[int, int]) -> Outcome[Done]:
        width, height = size
        aspect = width * framing.pixel_aspect_x / (height * framing.pixel_aspect_y)
        match values:
            case {"fit": bool(fit), "direction": str(direction), "extension": str(extension)}:
                smaller = min(aspect, 1.0) / max(aspect, 1.0)
                radial = Radial(distortion=record.distortion, scale=record.scale(1.0 + smaller * smaller if fit and record.distortion > 0.0 else 1.0), aspect=aspect)
                forward = radial.to_rectilinear if direction == "Distort" else radial.to_lens
                wrap = {border.item.name: border for border in Border}[extension].wrap

                def mapping(points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
                    return np.stack(forward((points[..., 0], points[..., 1], points[..., 2]), np), axis=-1)

                plane = StMap.bake(width, height, mapping, (wrap, wrap), source_size=size).plane
                return ImageFile(pixels=plane, colorspace=PyOpenColorIO.ROLE_DATA).write(str(call.paths[0]), Storage(), {}, PyOpenColorIO.GetCurrentConfig()).map(lambda path: Done.wrote((path,)))
            case _:
                return Error((Fault(reason=RecordReason.UNREADABLE, subject=Distortion.__name__, cause=repr(values)),))

    def ready(scene: bpy.types.Scene) -> Outcome[Done]:
        entry = distortion.named(call.identity, scene, call.properties.name)
        held = entry.bind(
            lambda found: paired(
                distortion.read(call.identity, found), captured(call.identity, found).map(lambda snapshot: {key: owner.values.get("value") for key, owner in snapshot.tree.owners.items() if key})
            )
        )
        framing = FrameSize().bound(call.identity).bind(lambda framed: framed.read(scene, Some(Whole()))).bind(lambda found: found.rendered((RESOLUTION.prop.hard_min, RESOLUTION.prop.hard_max)))
        return paired(held, framing).bind(lambda parts: written(parts[0][0].record, parts[0][1], *parts[1]))

    return call.root(bpy.types.Scene).bind(ready)


# --- [COMPOSITION] ----------------------------------------------------------------------

distortion = CompositorEffect(
    record=Distortion,
    graph=distorted,
    interface=Interface(
        inputs=(
            Socket(key="fit", name="Fit", kind=bpy.types.NodeSocketBool, description="Barrel fit to frame corners"),
            Socket(
                key="direction",
                name="Direction",
                kind=bpy.types.NodeSocketMenu,
                description="Distort rectilinear or undistort lens image",
                members={"default_value": "Distort"},
                items=Some(MenuItems(items=(MenuItem(name="Distort", description="Distort a rectilinear image"), MenuItem(name="Undistort", description="Undistort a lens image")))),
            ),
            Socket(
                key="extension", name="Extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", members={"default_value": Border.BLACK.item.name}, items=Some(BORDERS)
            ),
        )
    ),
    alpha=Alpha.RESULT,
    parts=(Registers(rows=(Operator(form=Named, run=export_st_map, invoke=Some(Save(extensions=lambda _: (".exr",))), undo=False),)),),
)
aberration = CompositorEffect(
    record=Aberration,
    graph=aberrated,
    interface=Interface(
        inputs=(
            Socket(key="axial", name="Axial", kind=bpy.types.NodeSocketFloat, description="Blue defocus radius, red at half and green in focus", members={"min_value": 0.0}),
            Socket(
                key="extension", name="Extension", kind=bpy.types.NodeSocketMenu, description="Source sampling past frame edges", members={"default_value": Border.CLAMP.item.name}, items=Some(BORDERS)
            ),
        ),
        panels=(IRIS,),
    ),
    alpha=Alpha.RESULT,
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aberration", "Distortion", "aberration", "distortion"]
