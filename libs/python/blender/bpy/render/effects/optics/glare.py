"""Whole-frame scatter, spectral diffraction, ghosts, and film halation."""

import bpy
from expression import Error, Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import scipy as sp

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderAbsentFault, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, pixels, shorter
from blender.bpy.nodes.graphs import End, ended, Fragment, Generated, math, mixed, node, Operand, switched, vector
from blender.bpy.render.effects.stack import CompositorEffect, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.imaging.effects.optics.glare import Bloom, Dirt, Flares, Glare, Halation, Highlights, Pupil, Specks
from blender.imaging.faults import paired
from blender.imaging.hashing import content_hash
from blender.imaging.parameters import Gated

# --- [OPERATIONS] -----------------------------------------------------------------------


def keyed(key: Key, source: Operand, threshold: Operand, smoothness: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Extracts unclamped scene-linear highlights at full quality.

    Args:
        key: Extraction node key.
        source: Associated scene-linear light.
        threshold: Highlight threshold in scene-linear light.
        smoothness: Continuous width of highlight knee.

    Returns:
        Highlight field or graph faults.
    """
    return node(key, bpy.types.CompositorNodeGlare, {"Image": source, "Quality": "High", "Highlights Threshold": threshold, "Highlights Smoothness": smoothness, "Clamp Highlights": False}, {}).map(
        ended(key, "Highlights")
    )


def bloomed(shell: Shell[Bloom]) -> BlenderResult[tuple[Fragment, End]]:
    """Scatters toward mip mean and lights camera-space lens dirt.

    Args:
        shell: Source, frame geometry, dirt graph field, and consuming context.

    Returns:
        Bloomed light and graph, or graph and acquisition faults.
    """
    specks = shell.record.dirt.value.specks
    content = content_hash(Specks, shell.record.dirt, arrays=tuple(np.asarray(frame.size[::-1], dtype=np.int64) for _, frame in shell.context.to_list()), providers=(np,))

    def plane() -> BlenderResult[Option[npt.NDArray[np.float32]]]:
        if not shell.record.dirt.enabled:
            return Ok(Nothing)
        return shell.context.map(
            lambda context: specks.field(context[1].size[::-1]).map_error(BlenderImagingFault.lifted).map(lambda coverage: Some(np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))
        ).default_with(lambda: Error((BlenderAbsentFault(owner=repr(specks), key="consuming_image_context"),)))

    field = node(
        "dirt_field", bpy.types.CompositorNodeImage, {}, {}, Some(Generated(ownership=dirt_fields, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA))))
    ).map(ended("dirt_field", "Image"))

    def scattered(frame: FrameSpace, field: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        scatter_field, size_field, dirt = Bloom.__struct_fields__
        strength, _ = Dirt.__struct_fields__
        _, value = Gated.__struct_fields__
        smaller = node("dimensions", bpy.types.CompositorNodeImageInfo, {"Image": shell.source}, {}).bind(lambda info: shorter("smaller", ended("dimensions", "Dimensions")(info)))
        size = paired(pixels("span", frame, shell.socket(size_field)), smaller).bind(lambda spans: math("size", "DIVIDE", *spans))
        mean = size.bind(
            lambda size: node(
                "mean",
                bpy.types.CompositorNodeGlare,
                {"Image": shell.source, "Type": "Bloom", "Quality": "High", "Highlights Threshold": 0.0, "Highlights Smoothness": 0.0, "Clamp Highlights": False, "Size": size},
                {},
            )
        ).map(ended("mean", "Glare"))

        scatter = shell.socket(scatter_field)
        clean = mean.bind(lambda mean: mixed("scatter", "VECTOR", scatter, shell.source, mean))
        gain = math("dirt_gain", "MULTIPLY", scatter, shell.socket(dirt, value, strength))
        light = mean.bind(lambda mean: vector("dirt_light", "MULTIPLY", mean, field))
        gained = paired(light, gain).bind(lambda parts: vector("dirt_gained", "SCALE", *parts))
        dirtied = paired(clean, gained).bind(lambda parts: vector("dirt_added", "ADD", *parts))
        return paired(clean, dirtied).bind(lambda parts: switched("result", "VECTOR", shell.socket(dirt), *parts))

    return paired(framed("frame", shell.source, shell.aspect, shell.scale), field).bind(lambda parts: scattered(*parts))


def glared(shell: Shell[Glare]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies photopic veil, then optional signed spectral diffraction.

    Args:
        shell: Source light, graph-owned pupil, gates, and consuming image context.

    Returns:
        Spread light and graph, or graph and acquisition faults.
    """
    veil_field, aperture = Glare.__struct_fields__
    _, value = Gated.__struct_fields__
    pupil = shell.record.aperture.value
    operation = pupil.kernel
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
        return operation.map(
            lambda compute: shell.context.map(
                lambda context: compute(shell.config, context[0], context[1].size[::-1], context[1].pixel_aspect_x / context[1].pixel_aspect_y).map_error(BlenderImagingFault.lifted).map(Some)
            ).default_with(lambda: Error((BlenderAbsentFault(owner=repr(pupil), key="consuming_image_context"),)))
        ).default_value(Ok(Nothing))

    kernel = node(
        "kernel", bpy.types.CompositorNodeImage, {}, {}, Some(Generated(ownership=kernels, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA))))
    ).map(ended("kernel", "Image"))
    veil = (
        node(
            "veil",
            bpy.types.CompositorNodeGlare,
            {
                "Image": shell.source,
                "Type": "Fog Glow",
                "Quality": "High",
                "Highlights Threshold": 0.0,
                "Highlights Smoothness": 0.0,
                "Clamp Highlights": False,
                "Size": shell.socket(veil_field, value),
            },
            {},
        )
        .map(ended("veil", "Glare"))
        .bind(lambda spread: switched("veiled", "RGBA", shell.socket(veil_field), shell.source, spread))
    )
    return paired(veil, kernel).bind(
        lambda parts: (
            node("diffraction", bpy.types.CompositorNodeConvolve, {"Image": parts[0], "Kernel Data Type": "Color", "Color Kernel": parts[1], "Normalize Kernel": False}, {})
            .map(ended("diffraction", "Image"))
            .bind(lambda spread: switched("result", "RGBA", shell.socket(aperture), parts[0], spread))
        )
    )


def flared(shell: Shell[Flares]) -> BlenderResult[tuple[Fragment, End]]:
    """Adds recursive highlight ghosts to associated scene light.

    Args:
        shell: Source image and highlight, gain, and ghost fields.

    Returns:
        Combined image and graph, or graph faults.
    """
    highlights, strength, iterations, color_modulation = Flares.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    return (
        node(
            "ghosts",
            bpy.types.CompositorNodeGlare,
            {
                "Image": shell.source,
                "Type": "Ghosts",
                "Quality": "High",
                "Highlights Threshold": shell.socket(highlights, threshold),
                "Highlights Smoothness": shell.socket(highlights, smoothness),
                "Clamp Highlights": False,
                "Strength": shell.socket(strength),
                "Iterations": shell.socket(iterations),
                "Color Modulation": shell.socket(color_modulation),
            },
            {},
        )
        .map(ended("ghosts", "Glare"))
        .bind(lambda ghosts: mixed("result", "RGBA", 1.0, shell.source, ghosts, "ADD"))
    )


def halated(shell: Shell[Halation]) -> BlenderResult[tuple[Fragment, End]]:
    """Adds Gaussian highlight halo with per-channel film response.

    Args:
        shell: Source image, displayed frame measures, and halation fields.

    Returns:
        Re-exposed image and graph, or accumulated graph faults.
    """
    highlights_field, radius, color = Halation.__struct_fields__
    threshold, smoothness = Highlights.__struct_fields__
    highlights = keyed("key", shell.source, shell.socket(highlights_field, threshold), shell.socket(highlights_field, smoothness))
    size = framed("frame", shell.source, shell.aspect, shell.scale).bind(lambda frame: pixels("radius", frame, shell.socket(radius), kind="VECTOR"))
    return (
        paired(highlights, size)
        .bind(lambda parts: node("halo", bpy.types.CompositorNodeBlur, {"Image": parts[0], "Size": parts[1], "Type": "Gaussian"}, {}))
        .map(ended("halo", "Image"))
        .bind(lambda halo: vector("film_response", "MULTIPLY", halo, shell.socket(color)))
        .bind(lambda response: mixed("result", "RGBA", 1.0, shell.source, response, "ADD"))
    )


# --- [COMPOSITION] ----------------------------------------------------------------------

kernels = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
dirt_fields = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
bloom = CompositorEffect(record=Bloom, graph=bloomed, registrables=(dirt_fields,))
glare = CompositorEffect(record=Glare, graph=glared, registrables=(kernels,))
flares: CompositorEffect[Flares] = CompositorEffect(
    record=Flares,
    graph=flared,
    registrables=(
        inspection(row=lambda: flares, key="flares_key", title="View Flares Key", description="Highlights forming internal reflections", target=lambda: End(node="ghosts", socket="Highlights")),
    ),
)
halation: CompositorEffect[Halation] = CompositorEffect(
    record=Halation,
    graph=halated,
    registrables=(inspection(row=lambda: halation, key="halation_key", title="View Halation Key", description="Highlights reaching film base", target=lambda: End(node="key", socket="Highlights")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["bloom", "bloomed", "dirt_fields", "flared", "flares", "glare", "glared", "halated", "halation", "kernels", "keyed"]
