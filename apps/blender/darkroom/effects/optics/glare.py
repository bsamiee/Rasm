"""Whole-frame scatter of scene light: bloom lighting lens dirt, veiling glare with spectral pupil diffraction, highlight ghosts, and film halation."""

from typing import Annotated, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import scipy as sp

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.nodes.graphs import Expr, Generated, mixed, switched, vector
from blender.bpy.nodes.interface import Interface, InterfacePanel, Socket, under
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import pixels, shorter
from blender.bpy.render.compositor.effects import CompositorEffect, Registers, Shell
from blender.imaging.sampling.kernels import Pupil, Specks
from blender.records.faults import Outcome
from blender.records.identity import content_hash

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_WHOLE: Final[tuple[tuple[str, Value], ...]] = (("Quality", "High"), ("Clamp Highlights", False), ("Highlights Threshold", 0.0), ("Highlights Smoothness", 0.0))

# --- [MODELS] ---------------------------------------------------------------------------


class Bloom(Effect, frozen=True, kw_only=True):
    """Whole-frame scatter toward a mip-chain mean, lighting lens dirt."""

    stage = Stage.SCATTER
    pointwise = False
    specks: Annotated[Specks, msgspec.Meta(description="Lens dirt coverage")] = Specks()


class Glare(Effect, frozen=True, kw_only=True):
    """Whole-frame point spread from photopic veiling glare and aperture diffraction."""

    stage = Stage.SCATTER
    pointwise = False
    pupil: Annotated[Pupil, msgspec.Meta(description="Iris spectral diffraction")] = Pupil()


class Flares(Effect, frozen=True, kw_only=True):
    """Internal-reflection highlight ghosts at signed scales through frame center."""

    stage = Stage.SCATTER
    pointwise = False


class Halation(Effect, frozen=True, kw_only=True):
    """Red-weighted highlight re-exposure reflected by a film base."""

    stage = Stage.HALATION
    pointwise = False


# --- [OPERATIONS] -----------------------------------------------------------------------


def bloomed(shell: Shell[Bloom]) -> Outcome[Expr]:
    """Scatters toward the mip mean and lights camera-space lens dirt from a coverage field generated once an image context sizes it."""
    specks = shell.record.specks
    content = content_hash(Specks, specks, arrays=tuple(np.asarray(frame.size[::-1], dtype=np.int64) for _, frame in shell.context.to_list()), providers=(np,))

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return Ok(shell.context.map(lambda context: specks.field(context[1].size[::-1])).map(lambda coverage: np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))

    field = Expr(
        kind=bpy.types.CompositorNodeImage,
        content=Some(Generated(ownership=dirt_fields, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
        socket="Image",
        shape="RGBA",
    ).named(dirt_fields.key)
    dimensions = Expr(kind=bpy.types.CompositorNodeImageInfo, inputs=(("Image", shell.color),), socket="Dimensions", shape="VECTOR")
    mean = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", shell.color), ("Type", "Bloom"), *_WHOLE, ("Size", pixels(shell.space, shell.input("size")) / shorter(dimensions))),
        socket="Glare",
        shape="RGBA",
    )
    scatter = shell.input("scatter")
    clean = mixed(scatter, shell.color, mean)
    return Ok(switched(shell.input("lens_dirt"), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, field), scatter * shell.input("lens_dirt_strength")))))


def glared(shell: Shell[Glare]) -> Outcome[Expr]:
    """Applies the photopic veil, then signed spectral diffraction through a pupil kernel generated once an image context sizes it."""
    pupil = shell.record.pupil
    content = content_hash(
        Pupil,
        pupil,
        arrays=tuple(
            array
            for stored, frame in shell.context.to_list()
            for array in (np.asarray((stored, frame.size[::-1]), dtype=np.int64), np.asarray(frame.pixel_aspect_x / frame.pixel_aspect_y, dtype=np.float64))
        ),
        config=Some(shell.config.getCacheID()),
        providers=(np, sp, PyOpenColorIO),
    )

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return shell.context.map(lambda context: pupil.kernel(shell.config, context[0], context[1].size[::-1], context[1].pixel_aspect_x / context[1].pixel_aspect_y)).default_value(Ok(Nothing))

    kernel = Generated(ownership=kernels, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))
    spread = Expr(kind=bpy.types.CompositorNodeGlare, inputs=(("Image", shell.color), ("Type", "Fog Glow"), *_WHOLE, ("Size", shell.input("veil_size"))), socket="Glare", shape="RGBA")
    veil = switched(shell.input("veil"), shell.color, spread)
    return Ok(switched(shell.input("aperture"), veil, filters.convolved(veil, kernel)))


def flared(shell: Shell[Flares]) -> Outcome[Expr]:
    """Adds recursive highlight ghosts to scene light."""
    ghosts = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", shell.color),
            ("Type", "Ghosts"),
            ("Highlights Threshold", shell.input("highlights", "threshold")),
            ("Highlights Smoothness", shell.input("highlights", "smoothness")),
            ("Quality", "High"),
            ("Clamp Highlights", False),
            ("Strength", shell.input("strength")),
            ("Iterations", shell.input("iterations")),
            ("Color Modulation", shell.input("color_modulation")),
        ),
        socket="Glare",
        shape="RGBA",
    ).named("ghosts")
    return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


def halated(shell: Shell[Halation]) -> Outcome[Expr]:
    """Adds the Gaussian highlight halo under per-channel film response."""
    key = masks.highlight(shell.color, shell.input("highlights", "threshold"), shell.input("highlights", "smoothness")).named("key")
    halo = Expr(kind=bpy.types.CompositorNodeBlur, inputs=(("Image", key), ("Size", pixels(shell.space, shell.input("radius"), kind="VECTOR")), ("Type", "Gaussian")), socket="Image", shape="RGBA")
    return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input("color")), blend="ADD"))


# --- [COMPOSITION] ----------------------------------------------------------------------

kernels = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
dirt_fields = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
bloom = CompositorEffect(
    record=Bloom,
    graph=bloomed,
    interface=Interface(
        inputs=tuple(
            Socket(
                key=key,
                name=bpy.path.display_name(key),
                kind=bpy.types.NodeSocketFloat,
                description=description,
                members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": default},
            )
            for key, description, default in (("scatter", "Share of scene light spread into bloom", 0.7 * 0.15**1.5), ("size", "Widest bloom level", 0.28125))
        ),
        panels=(
            InterfacePanel(
                key="lens_dirt",
                name="Lens Dirt",
                description="Bloom-lit front-element dirt",
                toggle=Some(False),
                inputs=(
                    Socket(
                        key="lens_dirt_strength",
                        name="Strength",
                        kind=bpy.types.NodeSocketFloat,
                        description="Dirt light multiple of bloom",
                        members={"min_value": 0.0, "max_value": 5.0, "default_value": 1.0},
                    ),
                ),
            ),
        ),
    ),
    parts=(Registers(rows=(dirt_fields,)),),
)
glare = CompositorEffect(
    record=Glare,
    graph=glared,
    interface=Interface(
        inputs=(Socket(key="aperture", name="Aperture", kind=bpy.types.NodeSocketBool, description="Iris spectral diffraction"),),
        panels=(
            InterfacePanel(
                key="veil",
                name="Veil",
                description="Spencer photopic veiling glare",
                toggle=Some(True),
                inputs=(
                    Socket(
                        key="veil_size",
                        name="Size",
                        kind=bpy.types.NodeSocketFloat,
                        description="Spread from a frame spanning 180 to 10 degrees",
                        members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0, "default_value": 0.5},
                    ),
                ),
            ),
        ),
    ),
    parts=(Registers(rows=(kernels,)),),
)
flares = CompositorEffect(
    record=Flares,
    graph=flared,
    interface=Interface(
        inputs=tuple(
            Socket(key=key, name=bpy.path.display_name(key), kind=kind, description=description, members={"subtype": subtype, **numbers})
            for key, kind, subtype, description, numbers in (
                ("strength", bpy.types.NodeSocketFloat, "NONE", "Ghost light multiple of highlights", {"min_value": 0.0, "default_value": 1.0}),
                ("iterations", bpy.types.NodeSocketInt, "NONE", "Two base ghosts, four more per iteration", {"min_value": 2, "max_value": 5, "default_value": 3}),
                ("color_modulation", bpy.types.NodeSocketFloat, "FACTOR", "Spectral modulation between ghost channels", {"min_value": 0.0, "max_value": 1.0, "default_value": 0.25}),
            )
        ),
        panels=(InterfacePanel(key="highlights", name="Highlights", description="Light forming ghosts", inputs=under("highlights", masks.HIGHLIGHTS).inputs),),
    ),
)
halation = CompositorEffect(
    record=Halation,
    graph=halated,
    interface=Interface(
        inputs=(
            Socket(key="radius", name="Radius", kind=bpy.types.NodeSocketFloat, description="Halo blur radius over greater frame side", members={"min_value": 0.0, "default_value": 0.0067}),
            Socket(key="color", name="Color", kind=bpy.types.NodeSocketColor, description="Film re-exposure per channel", members={"default_value": (1.0, 0.1, 0.0, 1.0)}),
        ),
        panels=(
            InterfacePanel(
                key="highlights",
                name="Highlights",
                description="Light reaching film base",
                inputs=tuple(
                    msgspec.structs.replace(socket, members={**socket.members, "default_value": 2.0 if socket.key == "highlights_threshold" else 1.0})
                    for socket in under("highlights", masks.HIGHLIGHTS).inputs
                ),
            ),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bloom", "Flares", "Glare", "Halation", "bloom", "flares", "glare", "halation"]
