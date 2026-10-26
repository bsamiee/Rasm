"""Circular lens maps, independent spectral integration, and iris aberration."""

from collections.abc import Mapping
from typing import Annotated, Final, Literal

import bpy
from expression import Ok, Result, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Operator, Save
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import Expr, grouped, math, rgba, switched, vector, where, xyz
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.coordinates import covered, extended, pixels, shorter
from blender.bpy.render.effects.optics.defocus import iris
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, Named, Shell
from blender.bpy.render.outputs.framing import FrameSize, Framing, Whole
from blender.imaging.compositing.blending import Entry
from blender.imaging.effects.optics.distortion import Aberration, Direction, Distortion
from blender.imaging.faults import paired
from blender.imaging.files.images import ImageFile, Storage
from blender.imaging.geometry.stmaps import StMap
from blender.imaging.parameters import heading

# --- [CONSTANTS] ------------------------------------------------------------------------

RESOLUTION: Final = Descriptor.declared(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty)

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [LENS]


@grouped("radial_lens", heading("radial_lens"))
def radial(point: Annotated[Expr, bpy.types.NodeSocketVector], extent: Annotated[Expr, bpy.types.NodeSocketVector], share: Expr | float, distortion: Expr, scale: Expr | float) -> Mapping[Key, Expr]:
    """Circular forward and inverse radial maps of a displayed point, real-branch coverage in Z."""

    def length(value: Expr) -> Expr:
        folded = abs(value)
        largest = math("MAXIMUM", math("MAXIMUM", math("MAXIMUM", folded["X"], folded["Y"]), folded["Z"]), 1.0)
        return vector("LENGTH", value / largest) * largest

    offset = point * (2.0, 2.0, 0.0) - extent
    unit, strength, negative = vector("SCALE", offset / extent, share), math("SQRT", abs(distortion)), distortion < 0.0
    stretched = vector("SCALE", offset, strength * scale * 2.0)
    reach, hypotenuse = length(stretched), length(vector("ADD", stretched, (0.0, 0.0, 1.0)))
    real = math("MAXIMUM", 1.0 - (reach > 1.0), negative)
    root = switched(negative, math("SQRT", (1.0 - reach) * (1.0 + reach)), hypotenuse)
    lens = length(vector("SCALE", offset, strength))
    squared = lens * lens
    covered = switched(negative, 1.0 - (lens > 1.0), lens < 1.0)
    return {
        "rectilinear": vector("MULTIPLY_ADD", unit, where(real, scale / (root + 1.0), 0.0), xyz(0.5, 0.5, real)),
        "lens": vector("MULTIPLY_ADD", unit, where(covered, 0.5 / ((1.0 + switched(negative, squared, -squared)) * scale), 0.0), xyz(0.5, 0.5, covered)),
    }


def distorted(shell: Shell[Distortion]) -> BlenderResult[Expr]:
    """Samples the stage input through the fitted circular distortion or its covered inverse map."""
    distortion_field, fit, direction, extension = Distortion.__struct_fields__
    frame, coefficient = shell.space, shell.input(distortion_field)
    smaller = shorter(frame.extent)
    anchor = math("MULTIPLY_ADD", shell.input(fit) * (coefficient > 0.0), smaller * smaller, 1.0)
    placed = radial(frame.point, frame.extent, 1.0, coefficient, 1.0 / (1.0 / anchor + coefficient) / anchor)
    position = shell.menu(direction, cases={Direction.DISTORT.label: placed, Direction.UNDISTORT.label: msgspec.structs.replace(placed, socket="lens")}, kind="VECTOR")
    border = (shell.input(extension), shell.items[extension,])
    return Ok(extended(shell.color, covered(frame, position), (border, border), clipped=False))


def aberrated(shell: Shell[Aberration]) -> BlenderResult[Expr]:
    """Integrates independent spectral maps, then gathers red and blue axial light through the iris."""
    dispersion, _, axial, iris_field, extension = Aberration.__struct_fields__
    frame = shell.space
    border = (shell.input(extension), shell.items[extension,])
    samples = tuple(
        (extended(shell.color, covered(frame, radial(frame.point, frame.extent, 1.0, shell.input(dispersion) * coefficient, 1.0)), (border, border), clipped=False), weight)
        for coefficient, weight in shell.record.spectrum
    )
    colors = tuple(vector("MULTIPLY", sample, (weight.red, weight.green, weight.blue)) for sample, weight in samples)
    alphas = tuple(sample["Alpha"] * (1.0 / shell.record.samples) for sample, _ in samples)
    lateral = Expr(
        kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", sum(colors[1:], colors[0])), ("Alpha", sum(alphas[1:], alphas[0])), ("Type", "Replace Alpha")), socket="Image", shape="RGBA"
    )
    bokeh = iris(shell, iris_field)

    def gathered(channel: Literal["Red", "Green", "Blue"], ratio: float) -> Expr:
        return (
            Expr(kind=bpy.types.CompositorNodeBokehBlur, inputs=(("Image", lateral[channel]), ("Bokeh", bokeh), ("Size", pixels(frame, shell.input(axial) * ratio))), socket="Image")
            if ratio > 0.0
            else lateral[channel]
        )

    return Ok(rgba(gathered("Red", Aberration.defocus.red), gathered("Green", Aberration.defocus.green), gathered("Blue", Aberration.defocus.blue), lateral["Alpha"]))


# --- [EXPORT]


def exported(call: Call[Named]) -> BlenderResult[Done]:
    """Writes selected lens map as data RGBA float OpenEXR, or independent entry, framing, configuration, and file faults."""

    def written(config: PyOpenColorIO.Config, effect: Distortion, framing: Framing) -> BlenderResult[Done]:
        def baked(validated: tuple[Framing, tuple[int, int]]) -> BlenderResult[Done]:
            _, (width, height) = validated
            plane = StMap.bake(
                width, height, effect.mapping(width * framing.pixel_aspect_x / (height * framing.pixel_aspect_y)), (effect.extension, effect.extension), source_size=(width, height)
            ).plane
            return ImageFile(pixels=plane, colorspace=PyOpenColorIO.ROLE_DATA).write(str(call.paths[0]), Storage(), {}, config).map(lambda path: Done(notes=(("INFO", path),)))

        return Result[tuple[Framing, tuple[int, int]], BlenderFaults].bind(framing.rendered((RESOLUTION.prop.hard_min, RESOLUTION.prop.hard_max)), baked)

    def ready(scene: bpy.types.Scene) -> BlenderResult[Done]:
        entry = distortion.named(call.identity, scene, call.record.name).bind(lambda held: distortion.read(call.identity, held))
        framing = FrameSize().bound(call.identity).bind(lambda framed: framed.read(scene, Some(Whole())))
        return Result[tuple[tuple[PyOpenColorIO.Config, Entry[Distortion]], Framing], BlenderFaults].bind(
            paired(paired(config_of(), entry), framing), lambda parts: written(parts[0][0], parts[0][1].effect, parts[1])
        )

    return call.root(bpy.types.Scene).bind(ready)


# --- [COMPOSITION] ----------------------------------------------------------------------

st_map_export = Operator(
    key="export_st_map", title="Export ST Map", description="Write lens map as float OpenEXR data", form=Named, run=exported, invoke=Some(Save(extensions=lambda _: (".exr",))), undo=False
)

distortion = CompositorEffect(record=Distortion, graph=distorted, alpha=Alpha.RESULT, registrables=(st_map_export,))
aberration = CompositorEffect(record=Aberration, graph=aberrated, alpha=Alpha.RESULT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["RESOLUTION", "aberrated", "aberration", "distorted", "distortion", "exported", "radial", "st_map_export"]
