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
from blender.bpy.nodes.content import Generated
from blender.bpy.nodes.desired import Interface, InterfacePanel, Socket, under
from blender.bpy.nodes.graphs import Expr, mixed, switched, vector
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import dimensions, shorter
from blender.bpy.render.compositor.effects import CompositorEffect
from blender.bpy.render.compositor.inputs import Shell
from blender.imaging.sampling.tiles import Pupil, Specks
from blender.records.faults import Outcome
from blender.records.stores import content_hash

from ...chain.stages import Effect, Stage

# --- [CONSTANTS] ------------------------------------------------------------------------

_WHOLE: Final[tuple[tuple[str, Value], ...]] = (("Quality", "High"), ("Clamp Highlights", False), ("Highlights Threshold", 0.0), ("Highlights Smoothness", 0.0))
kernels: Final = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
dirt_fields: Final = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
_SCATTER: Final = Socket(key="scatter", kind=bpy.types.NodeSocketFloatFactor, description="Share of scene light spread into bloom", min_value=0.0, max_value=1.0, default_value=0.7 * 0.15**1.5)
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketFloatFactor, description="Widest bloom level", min_value=0.0, max_value=1.0, default_value=0.28125)
_LENS_DIRT_STRENGTH: Final = Socket(
    key="lens_dirt_strength", name="Strength", kind=bpy.types.NodeSocketFloat, description="Dirt light multiple of bloom", min_value=0.0, max_value=5.0, default_value=1.0
)
_LENS_DIRT: Final = InterfacePanel(key="lens_dirt", name="Lens Dirt", description="Bloom-lit front-element dirt", toggle=Some(False), inputs=(_LENS_DIRT_STRENGTH,))
_APERTURE: Final = Socket(key="aperture", kind=bpy.types.NodeSocketBool, description="Iris spectral diffraction")
_VEIL_SIZE: Final = Socket(
    key="veil_size", name="Size", kind=bpy.types.NodeSocketFloatFactor, description="Spread from a frame spanning 180 to 10 degrees", min_value=0.0, max_value=1.0, default_value=0.5
)
_VEIL: Final = InterfacePanel(key="veil", name="Veil", description="Spencer photopic veiling glare", toggle=Some(True), inputs=(_VEIL_SIZE,))
_STRENGTH: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloat, description="Ghost light multiple of highlights", min_value=0.0, default_value=1.0)
_ITERATIONS: Final = Socket(key="iterations", kind=bpy.types.NodeSocketInt, description="Two base ghosts, four more per iteration", min_value=2, max_value=5, default_value=3)
_COLOR_MODULATION: Final = Socket(
    key="color_modulation", kind=bpy.types.NodeSocketFloatFactor, description="Spectral modulation between ghost channels", min_value=0.0, max_value=1.0, default_value=0.25
)
_THRESHOLD, _SMOOTHNESS = masks.HIGHLIGHTS.inputs
_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Halo blur radius over greater frame side", min_value=0.0, default_value=0.0067)
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Film re-exposure per channel", default_value=(1.0, 0.1, 0.0, 1.0))

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
    """Scatters toward the mip mean and lights camera-space lens dirt from a coverage field generated once a preview frame sizes it."""
    specks = shell.record.specks
    content = content_hash(Specks, specks, arrays=tuple(np.asarray(preview.framing.size[::-1], dtype=np.int64) for preview in shell.preview.to_list()), providers=(np,))

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return Ok(shell.preview.map(lambda preview: specks.field(preview.framing.size[::-1])).map(lambda coverage: np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))

    field = Expr(
        kind=bpy.types.CompositorNodeImage,
        content=Some(Generated(ownership=dirt_fields, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    ).named(dirt_fields.key)
    mean = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", shell.color), ("Type", "Bloom"), *_WHOLE, ("Size", shell.space.pixels(shell.input(_SIZE)) / shorter(dimensions(shell.color)))),
        socket="Glare",
        shape=bpy.types.NodeSocketColor,
    )
    scatter = shell.input(_SCATTER)
    clean = mixed(scatter, shell.color, mean)
    return Ok(switched(shell.input(_LENS_DIRT), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, field), scatter * shell.input(_LENS_DIRT_STRENGTH)))))


def glared(shell: Shell[Glare]) -> Outcome[Expr]:
    """Applies the photopic veil, then signed spectral diffraction through a pupil kernel generated once a preview frame sizes it."""
    pupil = shell.record.pupil
    content = content_hash(
        Pupil,
        pupil,
        shell.config.getCacheID().encode(),
        arrays=tuple(
            array
            for preview in shell.preview.to_list()
            for array in (np.asarray((preview.size, preview.framing.size[::-1]), dtype=np.int64), np.asarray(preview.framing.pixel_aspect_x / preview.framing.pixel_aspect_y, dtype=np.float64))
        ),
        providers=(np, sp, PyOpenColorIO),
    )

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return shell.preview.map(lambda preview: pupil.kernel(shell.config, preview.size, preview.framing.size[::-1], preview.framing.pixel_aspect_x / preview.framing.pixel_aspect_y)).default_value(
            Ok(Nothing)
        )

    kernel = Generated(ownership=kernels, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))
    spread = Expr(
        kind=bpy.types.CompositorNodeGlare, inputs=(("Image", shell.color), ("Type", "Fog Glow"), *_WHOLE, ("Size", shell.input(_VEIL_SIZE))), socket="Glare", shape=bpy.types.NodeSocketColor
    )
    veil = switched(shell.input(_VEIL), shell.color, spread)
    return Ok(switched(shell.input(_APERTURE), veil, filters.convolved(veil, kernel)))


def ghosted(shell: Shell[Flares]) -> Outcome[Expr]:
    """Adds recursive highlight ghosts to scene light."""
    ghosts = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", shell.color),
            ("Type", "Ghosts"),
            ("Highlights Threshold", shell.input("highlights", _THRESHOLD)),
            ("Highlights Smoothness", shell.input("highlights", _SMOOTHNESS)),
            ("Quality", "High"),
            ("Clamp Highlights", False),
            ("Strength", shell.input(_STRENGTH)),
            ("Iterations", shell.input(_ITERATIONS)),
            ("Color Modulation", shell.input(_COLOR_MODULATION)),
        ),
        socket="Glare",
        shape=bpy.types.NodeSocketColor,
    ).named("ghosts")
    return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


def haloed(shell: Shell[Halation]) -> Outcome[Expr]:
    """Adds the Gaussian highlight halo under per-channel film response."""
    key = masks.highlight(shell.color, shell.input("highlights", _THRESHOLD), shell.input("highlights", _SMOOTHNESS)).named("key")
    halo = Expr(
        kind=bpy.types.CompositorNodeBlur, inputs=(("Image", key), ("Size", shell.space.pixel_size(shell.input(_RADIUS))), ("Type", "Gaussian")), socket="Image", shape=bpy.types.NodeSocketColor
    )
    return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input(_COLOR)), blend="ADD"))


# --- [COMPOSITION] ----------------------------------------------------------------------

bloom: Final = CompositorEffect(bloomed, record=Bloom, interface=Interface(inputs=(_SCATTER, _SIZE), panels=(_LENS_DIRT,)), registers=(dirt_fields,))
glare: Final = CompositorEffect(glared, record=Glare, interface=Interface(inputs=(_APERTURE,), panels=(_VEIL,)), registers=(kernels,))
flares: Final = CompositorEffect(
    ghosted,
    record=Flares,
    interface=Interface(
        inputs=(_STRENGTH, _ITERATIONS, _COLOR_MODULATION),
        panels=(InterfacePanel(key="highlights", name="Highlights", description="Light forming ghosts", inputs=under("highlights", masks.HIGHLIGHTS).inputs),),
    ),
)
halation: Final = CompositorEffect(
    haloed,
    record=Halation,
    interface=Interface(
        inputs=(_RADIUS, _COLOR),
        panels=(
            InterfacePanel(
                key="highlights",
                name="Highlights",
                description="Light reaching film base",
                inputs=under("highlights", Interface(inputs=(msgspec.structs.replace(_THRESHOLD, default_value=2.0), msgspec.structs.replace(_SMOOTHNESS, default_value=1.0)))).inputs,
            ),
        ),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bloom", "Flares", "Glare", "Halation", "bloom", "flares", "glare", "halation"]
