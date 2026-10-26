"""Whole-frame scatter of scene light: bloom lighting lens dirt, veiling glare with spectral pupil diffraction, highlight ghosts, and film halation."""

from typing import Annotated, Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import scipy as sp

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Generated, mixed, switched, vector
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import pixels, shorter
from blender.bpy.render.compositor.effects import CompositorEffect, Inspects, Registers, Shell
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.geometry.regions import FrameLength
from blender.imaging.sampling.kernels import Pupil, Specks
from blender.records.fields import Gated, NonNegative, Presentation, Quantity, Rgb, Unit
from blender.records.identity import content_hash

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_WHOLE: Final[tuple[tuple[str, Value], ...]] = (("Quality", "High"), ("Clamp Highlights", False), ("Highlights Threshold", 0.0), ("Highlights Smoothness", 0.0))

# --- [MODELS] ---------------------------------------------------------------------------


class Dirt(msgspec.Struct, frozen=True, kw_only=True):
    """Front-element dirt scattering bloom."""

    strength: Annotated[float, msgspec.Meta(ge=0.0, le=5.0, description="Dirt light multiple of bloom"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    specks: Annotated[Specks, msgspec.Meta(description="Lens dirt coverage"), GraphForm.IMAGE] = Specks()


class Bloom(Effect, frozen=True, kw_only=True):
    """Whole-frame scatter toward a mip-chain mean, lighting lens dirt."""

    stage = Stage.SCATTER
    pointwise = False
    scatter: Annotated[Unit, msgspec.Meta(description="Share of scene light spread into bloom"), Presentation(soft=Some((0.0, 0.25)))] = 0.7 * 0.15**1.5
    size: Annotated[Unit, msgspec.Meta(description="Widest bloom level"), Presentation(quantity=Some(Quantity.FRAME))] = 0.28125
    dirt: Annotated[Gated[Dirt], msgspec.Meta(title="Lens Dirt", description="Bloom-lit front-element dirt")] = Gated(enabled=False, value=Dirt())


class Glare(Effect, frozen=True, kw_only=True):
    """Whole-frame point spread from photopic veiling glare and aperture diffraction."""

    stage = Stage.SCATTER
    pointwise = False
    veil: Annotated[Gated[Annotated[Unit, msgspec.Meta(title="Size", description="Spread from a frame spanning 180 to 10 degrees")]], msgspec.Meta(description="Spencer photopic veiling glare")] = (
        Gated(enabled=True, value=0.5)
    )
    aperture: Annotated[Gated[Annotated[Pupil, GraphForm.IMAGE]], msgspec.Meta(description="Iris spectral diffraction")] = Gated(enabled=False, value=Pupil())


class Flares(Effect, frozen=True, kw_only=True):
    """Internal-reflection highlight ghosts at signed scales through frame center."""

    stage = Stage.SCATTER
    pointwise = False
    highlights: Annotated[masks.Highlights, msgspec.Meta(description="Light forming ghosts")] = masks.Highlights()
    strength: Annotated[NonNegative, msgspec.Meta(description="Ghost light multiple of highlights"), Presentation(soft=Some((0.0, 1.0)))] = 1.0
    iterations: Annotated[int, msgspec.Meta(ge=2, le=5, description="Two base ghosts, four more per iteration")] = 3
    color_modulation: Annotated[Unit, msgspec.Meta(description="Spectral modulation between ghost channels")] = 0.25


class Halation(Effect, frozen=True, kw_only=True):
    """Red-weighted highlight re-exposure reflected by a film base."""

    stage = Stage.HALATION
    pointwise = False
    highlights: Annotated[masks.Highlights, msgspec.Meta(description="Light reaching film base")] = masks.Highlights(threshold=2.0, smoothness=1.0)
    radius: Annotated[FrameLength, msgspec.Meta(description="Halo blur radius"), Presentation(soft=Some((0.0, 0.03)))] = 64.0 * 0.2 / 1920.0
    color: Annotated[Rgb, msgspec.Meta(description="Film re-exposure per channel")] = Rgb(red=1.0, green=0.1, blue=0.0)


# --- [OPERATIONS] -----------------------------------------------------------------------


def bloomed(shell: Shell[Bloom]) -> BlenderResult[Expr]:
    """Scatters toward the mip mean and lights camera-space lens dirt from a generated coverage field."""
    scatter_field, size, dirt = Bloom.__struct_fields__
    strength, _ = Dirt.__struct_fields__
    _, value = Gated.__struct_fields__
    specks = shell.record.dirt.value.specks
    content = content_hash(Specks, shell.record.dirt, arrays=tuple(np.asarray(frame.size[::-1], dtype=np.int64) for _, frame in shell.context.to_list()), providers=(np,))

    def plane() -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        return (
            shell.context.map(lambda context: specks.field(context[1].size[::-1]).map(lambda coverage: Some(np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))).default_with(
                lambda: Error((BlenderAbsentFault(owner=repr(specks), key="consuming_image_context"),))
            )
            if shell.record.dirt.enabled
            else Ok(Nothing)
        )

    field = Expr(
        kind=bpy.types.CompositorNodeImage,
        content=Some(Generated(ownership=dirt_fields, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
        socket="Image",
        shape="RGBA",
    ).named(dirt_fields.key)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR")
    mean = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", shell.color), ("Type", "Bloom"), *_WHOLE, ("Size", pixels(shell.space, shell.input(size)) / shorter(dimensions))),
        socket="Glare",
        shape="RGBA",
    )
    scatter = shell.input(scatter_field)
    clean = mixed(scatter, shell.color, mean)
    return Ok(switched(shell.gate(dirt), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, field), scatter * shell.input(dirt, value, strength)))))


def glared(shell: Shell[Glare]) -> BlenderResult[Expr]:
    """Applies the photopic veil, then signed spectral diffraction through a generated pupil kernel."""
    veil_field, aperture = Glare.__struct_fields__
    _, value = Gated.__struct_fields__
    pupil = shell.record.aperture.value
    content = content_hash(
        Pupil,
        shell.record.aperture,
        arrays=tuple(
            array
            for stored, frame in shell.context.to_list()
            for array in (np.asarray((stored, frame.size[::-1]), dtype=np.int64), np.asarray(frame.pixel_aspect_x / frame.pixel_aspect_y, dtype=np.float64))
        ),
        config=Some(shell.config.getCacheID()),
        providers=(np, sp, PyOpenColorIO),
    )

    def plane() -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        return (
            shell.context.map(lambda context: pupil.kernel(shell.config, context[0], context[1].size[::-1], context[1].pixel_aspect_x / context[1].pixel_aspect_y)).default_with(
                lambda: Error((BlenderAbsentFault(owner=repr(pupil), key="consuming_image_context"),))
            )
            if shell.record.aperture.enabled
            else Ok(Nothing)
        )

    kernel = Generated(ownership=kernels, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))
    spread = Expr(kind=bpy.types.CompositorNodeGlare, inputs=(("Image", shell.color), ("Type", "Fog Glow"), *_WHOLE, ("Size", shell.input(veil_field, value))), socket="Glare", shape="RGBA")
    veil = switched(shell.gate(veil_field), shell.color, spread)
    return Ok(switched(shell.gate(aperture), veil, filters.convolved(veil, kernel)))


def flared(shell: Shell[Flares]) -> BlenderResult[Expr]:
    """Adds recursive highlight ghosts to scene light."""
    highlights, strength, iterations, modulation = Flares.__struct_fields__
    threshold, smoothness = masks.Highlights.__struct_fields__
    ghosts = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", shell.color),
            ("Type", "Ghosts"),
            ("Highlights Threshold", shell.input(highlights, threshold)),
            ("Highlights Smoothness", shell.input(highlights, smoothness)),
            ("Quality", "High"),
            ("Clamp Highlights", False),
            ("Strength", shell.input(strength)),
            ("Iterations", shell.input(iterations)),
            ("Color Modulation", shell.input(modulation)),
        ),
        socket="Glare",
        shape="RGBA",
    ).named("ghosts")
    return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


def halated(shell: Shell[Halation]) -> BlenderResult[Expr]:
    """Adds the Gaussian highlight halo under per-channel film response."""
    highlights, radius, color = Halation.__struct_fields__
    threshold, smoothness = masks.Highlights.__struct_fields__
    key = masks.highlight(shell.color, shell.input(highlights, threshold), shell.input(highlights, smoothness)).named("key")
    halo = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", key), ("Size", pixels(shell.space, shell.input(radius), kind="VECTOR")), ("Type", "Gaussian")), socket="Image", shape="RGBA")
    return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input(color)), blend="ADD"))


# --- [COMPOSITION] ----------------------------------------------------------------------

kernels = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
dirt_fields = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
bloom = CompositorEffect(record=Bloom, graph=bloomed, parts=(Registers(rows=(dirt_fields,)),))
glare = CompositorEffect(record=Glare, graph=glared, parts=(Registers(rows=(kernels,)),))
flares = CompositorEffect(record=Flares, graph=flared, parts=(Inspects(key="flares_key", end=End(node="ghosts", socket="Highlights"), description="Highlights forming internal reflections"),))
halation = CompositorEffect(record=Halation, graph=halated, parts=(Inspects(key="halation_key", end=End(node="key", socket="Highlights"), description="Highlights reaching film base"),))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bloom", "Dirt", "Flares", "Glare", "Halation", "bloom", "flares", "glare", "halation"]
