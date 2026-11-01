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
from blender.bpy.nodes.graphs import Expr, mixed, Scalar, switched, vector
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import dimensions, shorter
from blender.bpy.render.compositor.effects import CompositorEffect
from blender.bpy.render.compositor.inputs import Shell
from blender.imaging.sampling.tiles import Pupil, Specks
from blender.records.faults import Outcome
from blender.records.fields import Case
from blender.records.stores import content_hash

# --- [CONSTANTS] ------------------------------------------------------------------------

_WHOLE: Final[tuple[tuple[str, Value], ...]] = (("Quality", "High"), ("Highlights Threshold", 0.0), ("Highlights Smoothness", 0.0))
_KERNELS: Final = Ownership(kind=bpy.types.Image, key="pupil_kernel", saved=Saved.USED)
_DIRT_FIELDS: Final = Ownership(kind=bpy.types.Image, key="lens_dirt", saved=Saved.USED)
_THRESHOLD, _SMOOTHNESS = masks.HIGHLIGHTS.inputs
_SCATTER: Final = Socket(key="scatter", kind=bpy.types.NodeSocketFloatFactor, description="Share of scene light spread into bloom", default_value=0.7 * 0.15**1.5)
_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketFloatFactor, description="Widest bloom level over greater frame side", default_value=0.28125)
_DIRT_STRENGTH: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloat, description="Dirt light multiple of bloom", min_value=0.0, max_value=5.0, default_value=1.0)
_LENS_DIRT: Final = InterfacePanel.of("lens_dirt", Interface(inputs=(_DIRT_STRENGTH,)), description="Bloom-lit front-element dirt", toggle=Some(False))
_APERTURE: Final = Socket(key="aperture", kind=bpy.types.NodeSocketBool, description="Iris spectral diffraction")
_VEIL_SIZE: Final = Socket(key="size", kind=bpy.types.NodeSocketFloatFactor, description="Spread over greater frame side, from a frame spanning 180 to 10 degrees", default_value=0.5)
_VEIL: Final = InterfacePanel.of("veil", Interface(inputs=(_VEIL_SIZE,)), description="Spencer photopic veiling glare", toggle=Some(True))
_STRENGTH: Final = Socket(key="strength", kind=bpy.types.NodeSocketFloat, description="Ghost light multiple of highlights", min_value=0.0, default_value=1.0)
_ITERATIONS: Final = Socket(key="iterations", kind=StockInput(node=bpy.types.CompositorNodeGlare, input="Iterations"))
_COLOR_MODULATION: Final = Socket(key="color_modulation", kind=StockInput(node=bpy.types.CompositorNodeGlare, input="Color Modulation"))
_GHOST_HIGHLIGHTS: Final = InterfacePanel.of("highlights", masks.HIGHLIGHTS, description="Light forming ghosts")
_RADIUS: Final = Socket(key="radius", kind=bpy.types.NodeSocketFloat, description="Halo blur radius over greater frame side", min_value=0.0, default_value=0.0067)
_COLOR: Final = Socket(key="color", kind=bpy.types.NodeSocketColor, description="Film re-exposure per channel", default_value=(1.0, 0.1, 0.0, 1.0))
_HALO_HIGHLIGHTS: Final = InterfacePanel.of("highlights", masks.HIGHLIGHTS.defaulted({_THRESHOLD.key: 2.0, _SMOOTHNESS.key: 1.0}), description="Light reaching film base")

# --- [MODELS] ---------------------------------------------------------------------------


class Bloom(Case, frozen=True, kw_only=True):
    """Whole-frame scatter toward a mip-chain mean, lighting lens dirt."""

    specks: Annotated[Specks, msgspec.Meta(description="Lens dirt coverage")] = Specks()


class Glare(Case, frozen=True, kw_only=True):
    """Whole-frame point spread from photopic veiling glare and aperture diffraction."""

    pupil: Annotated[Pupil, msgspec.Meta(description="Iris spectral diffraction")] = Pupil()


class Ghosts(Case, frozen=True, kw_only=True):
    """Internal-reflection highlight ghosts at signed scales through frame center."""


class Halation(Case, frozen=True, kw_only=True):
    """Red-weighted highlight re-exposure reflected by a film base."""


# --- [OPERATIONS] -----------------------------------------------------------------------


def _spread[R: Case](shell: Shell[R], size: Scalar) -> Expr:
    """Glare node size of a spread over the greater frame side, which the node reads over the shorter side of the overscanned image."""
    return shell.space.pixels(size) / shorter(dimensions(shell.color))


def _bloomed(shell: Shell[Bloom]) -> Outcome[Expr]:
    """Scatters toward the mip mean and lights camera-space lens dirt from a coverage field generated once a preview frame sizes it."""
    specks = shell.record.specks
    content = content_hash(Specks, specks, arrays=tuple(np.asarray(preview.framing.size[::-1], dtype=np.int64) for preview in shell.preview.to_list()), providers=(np,))

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return Ok(shell.preview.map(lambda preview: specks.field(preview.framing.size[::-1])).map(lambda coverage: np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))

    field = Expr(
        kind=bpy.types.CompositorNodeImage,
        content=Some(Generated(ownership=_DIRT_FIELDS, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
        socket="Image",
        shape=bpy.types.NodeSocketColor,
    ).named(_DIRT_FIELDS.key)
    mean = Expr(
        kind=bpy.types.CompositorNodeGlare, inputs=(("Image", shell.color), ("Type", "Bloom"), *_WHOLE, ("Size", _spread(shell, shell.input(_SIZE)))), socket="Glare", shape=bpy.types.NodeSocketColor
    )
    scatter = shell.input(_SCATTER)
    clean = mixed(scatter, shell.color, mean)
    return Ok(switched(shell.input(_LENS_DIRT), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, field), scatter * shell.input(_LENS_DIRT, _DIRT_STRENGTH)))))


def _glared(shell: Shell[Glare]) -> Outcome[Expr]:
    """Applies the photopic veil, then signed spectral diffraction through a pupil kernel generated once a preview frame sizes it."""
    pupil = shell.record.pupil
    content = content_hash(
        Pupil,
        pupil,
        shell.config.getCacheID().encode(),
        arrays=tuple(
            array
            for preview in shell.preview.to_list()
            for array in (np.asarray((preview.size, preview.framing.size[::-1]), dtype=np.int64), np.asarray(preview.framing.pixel_aspect, dtype=np.float64))
        ),
        providers=(np, sp, PyOpenColorIO),
    )

    def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
        return shell.preview.map(lambda preview: pupil.kernel(shell.config, preview.size, preview.framing.size[::-1], preview.framing.pixel_aspect)).default_value(Ok(Nothing))

    kernel = Generated(ownership=_KERNELS, content=content, name="Spectral Pupil", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))
    spread = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(("Image", shell.color), ("Type", "Fog Glow"), *_WHOLE, ("Size", _spread(shell, shell.input(_VEIL, _VEIL_SIZE)))),
        socket="Glare",
        shape=bpy.types.NodeSocketColor,
    )
    veil = switched(shell.input(_VEIL), shell.color, spread)
    return Ok(switched(shell.input(_APERTURE), veil, filters.convolved(veil, kernel)))


def _ghosted(shell: Shell[Ghosts]) -> Outcome[Expr]:
    """Adds recursive highlight ghosts to scene light."""
    ghosts = Expr(
        kind=bpy.types.CompositorNodeGlare,
        inputs=(
            ("Image", shell.color),
            ("Type", "Ghosts"),
            ("Highlights Threshold", shell.input(_GHOST_HIGHLIGHTS, _THRESHOLD)),
            ("Highlights Smoothness", shell.input(_GHOST_HIGHLIGHTS, _SMOOTHNESS)),
            ("Quality", "High"),
            ("Strength", shell.input(_STRENGTH)),
            ("Iterations", shell.input(_ITERATIONS)),
            ("Color Modulation", shell.input(_COLOR_MODULATION)),
        ),
        socket="Glare",
        shape=bpy.types.NodeSocketColor,
    ).named("ghosts")
    return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


def _haloed(shell: Shell[Halation]) -> Outcome[Expr]:
    """Adds the Gaussian highlight halo under per-channel film response."""
    key = masks.highlight(shell.color, shell.input(_HALO_HIGHLIGHTS, _THRESHOLD), shell.input(_HALO_HIGHLIGHTS, _SMOOTHNESS)).named("key")
    halo = filters.blurred(key, shell.space.pixel_size(shell.input(_RADIUS)))
    return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input(_COLOR)), blend="ADD"))


# --- [COMPOSITION] ----------------------------------------------------------------------

bloom: Final = CompositorEffect(_bloomed, record=Bloom, interface=Interface(inputs=(_SCATTER, _SIZE), panels=(_LENS_DIRT,)), registers=(_DIRT_FIELDS,))
glare: Final = CompositorEffect(_glared, record=Glare, interface=Interface(inputs=(_APERTURE,), panels=(_VEIL,)), registers=(_KERNELS,))
ghosts: Final = CompositorEffect(_ghosted, record=Ghosts, interface=Interface(inputs=(_STRENGTH, _ITERATIONS, _COLOR_MODULATION), panels=(_GHOST_HIGHLIGHTS,)))
halation: Final = CompositorEffect(_haloed, record=Halation, interface=Interface(inputs=(_RADIUS, _COLOR), panels=(_HALO_HIGHLIGHTS,)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bloom", "Ghosts", "Glare", "Halation", "bloom", "ghosts", "glare", "halation"]
