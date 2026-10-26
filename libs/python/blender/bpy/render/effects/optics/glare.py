"""Whole-frame scatter, spectral diffraction, ghosts, and film halation."""

from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import scipy as sp

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Generated, mixed, Scalar, switched, vector
from blender.bpy.render.effects.coordinates import pixels, shorter
from blender.bpy.render.effects.rows import CompositorEffect, Inspected, Shell
from blender.imaging.effects.optics.glare import Bloom, Dirt, Flares, Glare, Halation, Highlights, Pupil, Specks
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Gated

# --- [CONSTANTS] ------------------------------------------------------------------------

HIGH: Final[tuple[tuple[str, Value], ...]] = (("Quality", "High"), ("Clamp Highlights", False))
WHOLE: Final[tuple[tuple[str, Value], ...]] = (*HIGH, ("Highlights Threshold", 0.0), ("Highlights Smoothness", 0.0))

# --- [OPERATIONS] -----------------------------------------------------------------------


def keyed(source: Scalar, threshold: Scalar, smoothness: Scalar) -> Expr:
    """Extracts unclamped scene-linear highlights at full quality, read at the Glare node's `Highlights`."""
    return Expr(kind=bpy.types.CompositorNodeGlare, inputs=(("Image", source), ("Highlights Threshold", threshold), ("Highlights Smoothness", smoothness), *HIGH), socket="Highlights", shape="RGBA")


def bloomed(shell: Shell[Bloom]) -> BlenderResult[Expr]:
    """Scatters toward mip mean and lights camera-space lens dirt from a generated coverage field."""
    scatter_field, size, dirt = Bloom.__struct_fields__
    strength, _ = Dirt.__struct_fields__
    _, value = Gated.__struct_fields__
    specks = shell.record.dirt.value.specks
    content = content_hash(Specks, shell.record.dirt, arrays=tuple(np.asarray(frame.size[::-1], dtype=np.int64) for _, frame in shell.context.to_list()), providers=(np,))

    def plane() -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        if not shell.record.dirt.enabled:
            return Ok(Nothing)
        return shell.context.map(lambda context: specks.field(context[1].size[::-1]).map(lambda coverage: Some(np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))).default_with(
            lambda: Error((BlenderAbsentFault(owner=repr(specks), key="consuming_image_context"),))
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
        inputs=(("Image", shell.color), ("Type", "Bloom"), *WHOLE, ("Size", pixels(shell.space, shell.input(size)) / shorter(dimensions))),
        socket="Glare",
        shape="RGBA",
    )
    scatter = shell.input(scatter_field)
    clean = mixed(scatter, shell.color, mean)
    return Ok(switched(shell.gate(dirt), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, field), scatter * shell.input(dirt, value, strength)))))


def glared(shell: Shell[Glare]) -> BlenderResult[Expr]:
    """Applies photopic veil, then optional signed spectral diffraction through a generated pupil kernel."""
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
        config=Some(shell.config),
        providers=(np, sp, PyOpenColorIO),
    )

    def plane() -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        if not shell.record.aperture.enabled:
            return Ok(Nothing)
        return pupil.kernel.map(
            lambda compute: shell.context.map(lambda context: compute(shell.config, context[0], context[1].size[::-1], context[1].pixel_aspect_x / context[1].pixel_aspect_y).map(Some)).default_with(
                lambda: Error((BlenderAbsentFault(owner=repr(pupil), key="consuming_image_context"),))
            )
        ).default_value(Ok(Nothing))

    kernel = Expr(
        kind=bpy.types.CompositorNodeImage,
        content=Some(Generated(ownership=kernels, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
        socket="Image",
        shape="RGBA",
    ).named(kernels.key)
    spread = Expr(kind=bpy.types.CompositorNodeGlare, inputs=(("Image", shell.color), ("Type", "Fog Glow"), *WHOLE, ("Size", shell.input(veil_field, value))), socket="Glare", shape="RGBA")
    veil = switched(shell.gate(veil_field), shell.color, spread)
    diffraction = Expr(
        kind=bpy.types.CompositorNodeConvolve, inputs=(("Image", veil), ("Kernel Data Type", "Color"), ("Color Kernel", kernel), ("Normalize Kernel", False)), socket="Image", shape="RGBA"
    )
    return Ok(switched(shell.gate(aperture), veil, diffraction))


def flared(shell: Shell[Flares]) -> BlenderResult[Expr]:
    """Adds recursive highlight ghosts to associated scene light."""
    highlights, strength, iterations, modulation = Flares.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    ghosts = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", shell.color),
            ("Type", "Ghosts"),
            ("Highlights Threshold", shell.input(highlights, threshold)),
            ("Highlights Smoothness", shell.input(highlights, smoothness)),
            *HIGH,
            ("Strength", shell.input(strength)),
            ("Iterations", shell.input(iterations)),
            ("Color Modulation", shell.input(modulation)),
        ),
        socket="Glare",
        shape="RGBA",
    ).named("ghosts")
    return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


def halated(shell: Shell[Halation]) -> BlenderResult[Expr]:
    """Adds Gaussian highlight halo with per-channel film response."""
    highlights, radius, color = Halation.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    key = keyed(shell.color, shell.input(highlights, threshold), shell.input(highlights, smoothness)).named("key")
    halo = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", key), ("Size", pixels(shell.space, shell.input(radius), kind="VECTOR")), ("Type", "Gaussian")), socket="Image", shape="RGBA")
    return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input(color)), blend="ADD"))


# --- [COMPOSITION] ----------------------------------------------------------------------

kernels = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
dirt_fields = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
bloom = CompositorEffect(record=Bloom, graph=bloomed, registrables=(dirt_fields,))
glare = CompositorEffect(record=Glare, graph=glared, registrables=(kernels,))
flares = CompositorEffect(record=Flares, graph=flared, inspected=(Inspected(key="flares_key", end=End(node="ghosts", socket="Highlights"), description="Highlights forming internal reflections"),))
halation = CompositorEffect(record=Halation, graph=halated, inspected=(Inspected(key="halation_key", end=End(node="key", socket="Highlights"), description="Highlights reaching film base"),))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["HIGH", "bloom", "bloomed", "dirt_fields", "flared", "flares", "glare", "glared", "halated", "halation", "kernels", "keyed"]
