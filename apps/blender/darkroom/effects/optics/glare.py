"""Whole-frame scatter of scene light: bloom lighting lens dirt, veiling glare with spectral pupil diffraction, highlight ghosts, and film halation."""

from collections.abc import Callable
from functools import partial
from typing import Annotated, ClassVar, Final, override

import bpy
from expression import Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO
import scipy as sp

from blender.bpy.data.ids.images import Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.nodes.content import Generated
from blender.bpy.nodes.graphs import Expr, mixed, Scalar, switched, vector, xyz
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket, StockInput
from blender.bpy.render.compositor import filters, masks
from blender.bpy.render.compositor.coordinates import dimensions, shorter, uv_mapped
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Prepared, Stage
from blender.bpy.render.compositor.inputs import GraphInputs
from blender.imaging.geometry.frames import Raster
from blender.imaging.sampling.tiles import Pupil, Specks
from blender.records.faults import Ok, Outcome
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


class Bloom(CompositorRecord, frozen=True, kw_only=True):
    """Whole-frame scatter toward a mip-chain mean, lighting lens dirt."""

    specks: Annotated[Specks, msgspec.Meta(description="Lens dirt coverage")] = Specks()

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Scatters toward the mip mean and lights camera-space lens dirt from a coverage field generated at camera resolution."""
        specks = self.specks

        def built(camera: Raster) -> Expr:
            content = content_hash(Specks, specks, camera.shape, providers=(np,))

            def plane() -> Outcome[Option[npt.NDArray[np.float32]]]:
                coverage = specks.field(camera.shape)
                return Ok(Some(np.broadcast_to(coverage[..., None], (*coverage.shape, 3))))

            field = Expr(
                kind=bpy.types.CompositorNodeImage,
                content=Some(Generated(ownership=_DIRT_FIELDS, content=content, name="Lens Dirt", plane=plane, tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))),
                socket="Image",
                socket_type=bpy.types.NodeSocketColor,
            ).keyed(_DIRT_FIELDS.key)
            fraction = shell.space.fraction
            dirt = uv_mapped(field, xyz(fraction["X"], fraction["Y"], 1.0))
            mean = Expr(
                kind=bpy.types.CompositorNodeGlare,
                inputs=(("Image", shell.color), ("Type", "Bloom"), *_WHOLE, ("Size", _spread(shell, shell.input(_SIZE)))),
                socket="Glare",
                socket_type=bpy.types.NodeSocketColor,
            )
            scatter = shell.input(_SCATTER)
            clean = mixed(scatter, shell.color, mean)
            return switched(shell.input(_LENS_DIRT), clean, vector("ADD", clean, vector("SCALE", vector("MULTIPLY", mean, dirt), scatter * shell.input(_LENS_DIRT, _DIRT_STRENGTH))))

        return shell.camera().map(built)


class Glare(CompositorRecord, frozen=True, kw_only=True):
    """Whole-frame point spread from photopic veiling glare and aperture diffraction."""

    pupil: Annotated[Pupil, msgspec.Meta(description="Iris spectral diffraction")] = Pupil()
    prepared_when: ClassVar[Socket | None] = _APERTURE

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Applies the photopic veil, then signed spectral diffraction through a pupil kernel over the actual data and camera rasters."""
        pupil = self.pupil

        spread = Expr(
            kind=bpy.types.CompositorNodeGlare,
            inputs=(("Image", shell.color), ("Type", "Fog Glow"), *_WHOLE, ("Size", _spread(shell, shell.input(_VEIL, _VEIL_SIZE)))),
            socket="Glare",
            socket_type=bpy.types.NodeSocketColor,
        )
        veil = switched(shell.input(_VEIL), shell.color, spread)

        def built(rasters: tuple[Raster, Raster]) -> Expr:
            data, camera = rasters
            content = content_hash(Pupil, pupil, shell.config.getCacheID(), data, camera, providers=(np, sp, PyOpenColorIO))
            kernel = Generated(ownership=_KERNELS, content=content, name="Spectral Pupil", plane=partial(pupil.kernel, shell.config, data, camera=camera), tag=Tag(space=Some(PyOpenColorIO.ROLE_DATA)))
            return switched(shell.input(_APERTURE), veil, filters.convolved(veil, kernel))

        return Ok(shell.rasters.map(built).default_value(veil))

    @property
    @override
    def prepared(self) -> Callable[[Stage], Outcome[Option[Prepared]]]:
        """Builds the pupil from the evaluated input geometry without rewriting its pixels."""
        return lambda stage: Ok(Some(stage.rasters))


class Ghosts(CompositorRecord, frozen=True, kw_only=True):
    """Internal-reflection highlight ghosts at signed scales through frame center."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
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
            socket_type=bpy.types.NodeSocketColor,
        ).keyed("ghosts")
        return Ok(mixed(1.0, shell.color, ghosts, blend="ADD"))


class Halation(CompositorRecord, frozen=True, kw_only=True):
    """Red-weighted highlight re-exposure reflected by a film base."""

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Adds the Gaussian highlight halo under per-channel film response."""
        key = masks.highlight(shell.color, shell.input(_HALO_HIGHLIGHTS, _THRESHOLD), shell.input(_HALO_HIGHLIGHTS, _SMOOTHNESS)).keyed("key")
        halo = filters.blurred(key, shell.space.pixel_size(shell.input(_RADIUS)))
        return Ok(mixed(1.0, shell.color, vector("MULTIPLY", halo, shell.input(_COLOR)), blend="ADD"))


# --- [OPERATIONS] -----------------------------------------------------------------------


def _spread(shell: GraphInputs, size: Scalar) -> Expr:
    """Glare node size of a spread over the greater frame side, which the node reads over the shorter side of the overscanned image."""
    return shell.space.pixels(size) / shorter(dimensions(shell.color))


# --- [COMPOSITION] ----------------------------------------------------------------------

bloom: Final = CompositorEffect(record=Bloom, interface=Interface(inputs=(_SCATTER, _SIZE), panels=(_LENS_DIRT,)), registers=(_DIRT_FIELDS,))
glare: Final = CompositorEffect(record=Glare, interface=Interface(inputs=(_APERTURE,), panels=(_VEIL,)), registers=(_KERNELS,))
ghosts: Final = CompositorEffect(record=Ghosts, interface=Interface(inputs=(_STRENGTH, _ITERATIONS, _COLOR_MODULATION), panels=(_GHOST_HIGHLIGHTS,)))
halation: Final = CompositorEffect(record=Halation, interface=Interface(inputs=(_RADIUS, _COLOR), panels=(_HALO_HIGHLIGHTS,)))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Bloom", "Ghosts", "Glare", "Halation", "bloom", "ghosts", "glare", "halation"]
