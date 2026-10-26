"""Circular radial lens distortion with corner framing, its ST map export, and lateral spectral aberration with axial iris defocus."""

import sys
from typing import Annotated, ClassVar, Final, Literal, overload

import bpy
from expression import Ok, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Save
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import Expr, math, rgba, vector, xyz
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.compositor import lenses
from blender.bpy.render.compositor.coordinates import Border, covered, extended, FrameSpace, pixels, shorter
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Named, Registers, Shell
from blender.bpy.render.outputs.framing import FrameSize, Framing, Whole
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.files.images import ImageFile, Storage
from blender.imaging.geometry.lenses import Aperture, Radial
from blender.imaging.geometry.regions import FrameLength
from blender.imaging.geometry.stmaps import StMap
from blender.records.faults import paired
from blender.records.fields import Choice, NonNegative, Presentation, Rgb

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------


class Direction(Choice):
    """Whether a map distorts a rectilinear image or undistorts a lens image."""

    DISTORT = 1
    UNDISTORT = 2


# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLUTION: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Distortion(Effect, frozen=True, kw_only=True):
    """Barrel or pincushion lens distortion with edge or corner framing."""

    stage = Stage.DISTORTION
    title = "Lens Distortion"
    pointwise = False
    distortion: Annotated[float, msgspec.Meta(gt=-1.0, le=sys.float_info.max, description="Barrel above 0, pincushion below 0"), Presentation(soft=Some((-0.999, 1.0))), GraphForm.SETTING] = 0.05
    fit: Annotated[bool, msgspec.Meta(description="Barrel fit to frame corners")] = False
    direction: Annotated[Direction, msgspec.Meta(description="Distort rectilinear or undistort lens image")] = Direction.DISTORT
    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.BLACK

    @overload
    def scale(self, reach: float) -> float: ...
    @overload
    def scale(self, reach: Expr) -> Expr: ...
    def scale(self, reach: float | Expr) -> float | Expr:
        """Zoom keeping the frame edge, or its corner at `reach` past the edge, in place."""
        return 1.0 / (1.0 / reach + self.distortion) / reach

    def reach(self, smaller: float) -> float:
        """Squared corner reach over the edge for a barrel fit to corners at shorter over greater side `smaller`, else 1."""
        return 1.0 + smaller * smaller if self.fit and self.distortion > 0.0 else 1.0


class Aberration(Effect, frozen=True, kw_only=True):
    """Lateral spectral integration about green, then red and blue iris defocus."""

    stage = Stage.DISTORTION
    title = "Chromatic Aberration"
    pointwise = False
    defocus: ClassVar[Rgb] = Rgb(red=0.5, green=0.0, blue=1.0)
    dispersion: Annotated[NonNegative, msgspec.Meta(description="Image height spread from red to blue"), Presentation(soft=Some((0.0, 1.0))), GraphForm.SETTING] = 0.05
    samples: Annotated[int, msgspec.Meta(ge=2, description="Wavelengths integrated from red to blue"), GraphForm.SETTING] = 8
    axial: Annotated[FrameLength, msgspec.Meta(description="Blue defocus radius, red at half and green in focus"), Presentation(soft=Some((0.0, 0.05)))] = 0.0
    iris: Annotated[Aperture, msgspec.Meta(description="Iris outline of red and blue defocus")] = Aperture()
    extension: Annotated[Border, msgspec.Meta(description="Source sampling past frame edges")] = Border.CLAMP

    @property
    def spectrum(self) -> tuple[tuple[float, Rgb], ...]:
        """Distortion coefficient and channel-normalized midpoint wavelength weights of each sample."""
        across = 2.0 * (np.arange(self.samples, dtype=np.float64) + 0.5) / self.samples - 1.0
        weights = np.stack((np.maximum(-across, 0.0), 1.0 - np.abs(across), np.maximum(across, 0.0)), axis=-1)
        return tuple(zip((-0.25 * self.dispersion * across).tolist(), msgspec.convert((weights / weights.sum(axis=0)).tolist(), tuple[Rgb, ...])))


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


def distorted(shell: Shell[Distortion]) -> BlenderResult[Expr]:
    """Samples the stage input through the fitted lens map or its covered inverse."""
    _, fit, direction, extension = Distortion.__struct_fields__
    frame, record = shell.space, shell.record
    smaller = shorter(frame.extent)
    zoom = record.scale(math("MULTIPLY_ADD", shell.input(fit) * float(record.distortion > 0.0), smaller * smaller, 1.0))
    positions = {member.label: lensed(frame, record.distortion, zoom, side) for member, side in ((Direction.DISTORT, "rectilinear"), (Direction.UNDISTORT, "lens"))}
    border = shell.input(extension)
    return Ok(extended(shell.color, covered(frame, shell.menu(direction, cases=positions, kind="VECTOR")), (border, border), clipped=False))


def aberrated(shell: Shell[Aberration]) -> BlenderResult[Expr]:
    """Integrates the spectral lens maps, then gathers red and blue axial light through the iris."""
    _, _, axial, iris_field, extension = Aberration.__struct_fields__
    frame = shell.space
    border = shell.input(extension)
    samples = tuple((extended(shell.color, covered(frame, lensed(frame, coefficient, 1.0, "rectilinear")), (border, border), clipped=False), weight) for coefficient, weight in shell.record.spectrum)
    colors = tuple(vector("MULTIPLY", sample, (weight.red, weight.green, weight.blue)) for sample, weight in samples)
    alphas = tuple(sample["Alpha"] * (1.0 / shell.record.samples) for sample, _ in samples)
    lateral = Expr(
        kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", sum(colors[1:], colors[0])), ("Alpha", sum(alphas[1:], alphas[0])), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"
    )
    bokeh = lenses.iris(lambda field: shell.input(iris_field, field.name))

    def gathered(channel: Literal["Red", "Green", "Blue"], ratio: float) -> Expr:
        return (
            Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", lateral[channel]), ("Bokeh", bokeh), ("Size", pixels(frame, shell.input(axial) * ratio))), socket="Image")
            if ratio > 0.0
            else lateral[channel]
        )

    return Ok(rgba(gathered("Red", Aberration.defocus.red), gathered("Green", Aberration.defocus.green), gathered("Blue", Aberration.defocus.blue), lateral["Alpha"]))


# --- [EXPORT]


def exported(call: Call[Named]) -> BlenderResult[Done]:
    """Writes the entry's lens map at render size as data RGBA float OpenEXR."""

    def written(config: PyOpenColorIO.Config, record: Distortion, framing: Framing, size: tuple[int, int]) -> BlenderResult[Done]:
        width, height = size
        aspect = width * framing.pixel_aspect_x / (height * framing.pixel_aspect_y)
        radial = Radial(distortion=record.distortion, scale=record.scale(record.reach(min(aspect, 1.0) / max(aspect, 1.0))), aspect=aspect)
        forward = radial.to_rectilinear if record.direction is Direction.DISTORT else radial.to_lens

        def mapping(points: npt.NDArray[np.float64]) -> npt.NDArray[np.float64]:
            return np.stack(forward((points[..., 0], points[..., 1], points[..., 2]), np), axis=-1)

        plane = StMap.bake(width, height, mapping, (record.extension.wrap, record.extension.wrap), source_size=size).plane
        return ImageFile(pixels=plane, colorspace=PyOpenColorIO.ROLE_DATA).write(str(call.paths[0]), Storage(), {}, config).map(lambda path: Done.wrote((path,)))

    def ready(scene: bpy.types.Scene) -> BlenderResult[Done]:
        entry = distortion.named(call.identity, scene, call.record.name).bind(lambda held: distortion.read(call.identity, held))
        framing = FrameSize().bound(call.identity).bind(lambda framed: framed.read(scene, Some(Whole()))).bind(lambda held: held.rendered((RESOLUTION.prop.hard_min, RESOLUTION.prop.hard_max)))
        return paired(config_of(), entry, framing).bind(lambda parts: written(parts[0], parts[1].record, *parts[2]))

    return call.root(bpy.types.Scene).bind(ready)


# --- [COMPOSITION] ----------------------------------------------------------------------

st_map_export = Operator(
    key="export_st_map", title="Export ST Map", description="Write lens map as float OpenEXR data", form=Named, run=exported, invoke=Some(Save(extensions=lambda _: (".exr",))), undo=False
)
distortion = CompositorEffect(record=Distortion, graph=distorted, alpha=Alpha.RESULT, parts=(Registers(rows=(st_map_export,)),))
aberration = CompositorEffect(record=Aberration, graph=aberrated, alpha=Alpha.RESULT)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Aberration", "Direction", "Distortion", "aberration", "distortion", "st_map_export"]
