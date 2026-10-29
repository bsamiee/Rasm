"""Film grain multiplying light by a lognormal factor over tiled normal scores, and sensor noise of shot and read deviation on scene-linear light."""

import functools
from pathlib import Path
from typing import ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import read, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.nodes.content import Generated
from blender.bpy.nodes.desired import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.functions import EXPRESSIONS
from blender.bpy.nodes.graphs import equal, Expr, math, mixed, vector, where, xyz
from blender.bpy.nodes.textures import MapRange, White
from blender.bpy.render.compositor.colors import space_item
from blender.bpy.render.compositor.coordinates import PixelGrid
from blender.bpy.render.compositor.effects import Alpha, Blended, CompositorEffect, precise
from blender.bpy.render.compositor.filters import tiled
from blender.bpy.render.compositor.inputs import Shell
from blender.bpy.render.compositor.masks import luminance
from blender.bpy.render.compositor.motion import clocked, origin
from blender.imaging.color.processors import GREY
from blender.imaging.files.images import read_file
from blender.imaging.geometry.arithmetic import gaussian
from blender.imaging.sampling import tiles
from blender.imaging.sampling.tiles import stream
from blender.records.faults import Outcome, paired
from blender.records.stores import content_hash, ContentHash, rng

from ...chain.stages import Effect, Stage
from ..optics.procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

_PROCEDURAL: Final = MenuItem(name="Procedural", description="Seeded Gaussian grain of two texel diameters baked into a seamless tile")
_SCAN: Final = MenuItem(name="Scan", description="Scanned grain image tiled over the frame")
_FILM: Final = MenuItem(name="Film", description="Grain multiplying light by a lognormal factor, weighted by tone")
_SENSOR: Final = MenuItem(name="Sensor", description="Shot noise scaling with signal and constant read noise")
TILES: Final = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)
_MEDIUM: Final = Socket(key="medium", kind=bpy.types.NodeSocketMenu, description="Film grain or sensor noise", default_value=_FILM.name, items=Some(MenuItems(items=(_FILM, _SENSOR))))
_CHROMA: Final = Socket(key="chroma", kind=bpy.types.NodeSocketFloatFactor, description="Per-channel noise share against shared noise", min_value=0.0, max_value=1.0)
_SEED: Final = Socket(key="seed", kind=bpy.types.NodeSocketInt, description="Number selecting the pattern", min_value=0, max_value=2**24)
_FIELD: Final = Socket(
    key="film_field", name="Field", kind=bpy.types.NodeSocketMenu, description="Procedural grain or scanned image", default_value=_PROCEDURAL.name, items=Some(MenuItems(items=(_PROCEDURAL, _SCAN)))
)
_SIZE: Final = Socket(key=item_key("film", "size"), name="Size", kind=bpy.types.NodeSocketFloat, description="Grain diameter over greater frame side", min_value=0.0, default_value=0.00104)
_SPAN: Final = Socket(key=item_key("film", "span"), name="Span", kind=bpy.types.NodeSocketFloat, description="Tile width over greater frame side", min_value=0.0, default_value=1.0)
_GRANULARITY: Final = Socket(key=item_key("film", "granularity"), name="Granularity", kind=bpy.types.NodeSocketFloat, description="Deviation of the log grain factor", min_value=0.0)
_SHADOWS: Final = Socket(
    key=item_key("film", "shadows"), name="Shadows", kind=bpy.types.NodeSocketFloatFactor, description="Grain weight below the shadow edge", min_value=0.0, max_value=1.0, default_value=1.0
)
_MIDTONES: Final = Socket(
    key=item_key("film", "midtones"),
    name="Midtones",
    kind=bpy.types.NodeSocketFloatFactor,
    description="Grain weight between the shadow and highlight bands",
    min_value=0.0,
    max_value=1.0,
    default_value=1.0,
)
_HIGHLIGHTS: Final = Socket(
    key=item_key("film", "highlights"), name="Highlights", kind=bpy.types.NodeSocketFloatFactor, description="Grain weight above the highlight band", min_value=0.0, max_value=1.0, default_value=1.0
)
_SHADOW_EDGE: Final = Socket(
    key=item_key("film", "shadow_edge"), name="Shadow Edge", kind=bpy.types.NodeSocketFloat, description="Luminance zeroing the shadow weight", min_value=0.0, default_value=0.09
)
_MIDTONE_SPAN: Final = Socket(
    key=item_key("film", "midtone_span"), name="Midtone Span", kind=bpy.types.NodeSocketFloat, description="Luminance from the shadow edge to the highlight band", min_value=0.0, default_value=0.41
)
_HIGHLIGHT_SPAN: Final = Socket(
    key=item_key("film", "highlight_span"), name="Highlight Span", kind=bpy.types.NodeSocketFloat, description="Luminance from the highlight start to full weight", min_value=0.0, default_value=0.5
)
_SHOT: Final = Socket(
    key=item_key("sensor", "shot"), name="Shot", kind=bpy.types.NodeSocketFloat, description="Photon noise deviation at scene-linear gray", min_value=0.0, default_value=0.046875 * float(np.sqrt(GREY))
)
_READ: Final = Socket(key=item_key("sensor", "read"), name="Read", kind=bpy.types.NodeSocketFloat, description="Signal-independent deviation", min_value=0.0)
_FILM_PANEL: Final = InterfacePanel(
    key="film", name="Film", description=_FILM.description, inputs=(_FIELD, _SIZE, _SPAN, _GRANULARITY, _SHADOWS, _MIDTONES, _HIGHLIGHTS, _SHADOW_EDGE, _MIDTONE_SPAN, _HIGHLIGHT_SPAN)
)
_SENSOR_PANEL: Final = InterfacePanel(key="sensor", name="Sensor", description=_SENSOR.description, inputs=(_SHOT, _READ))

# --- [MODELS] ---------------------------------------------------------------------------


class Procedural:
    """Seeded Gaussian grain of two texel diameters baked into a seamless tile."""

    side: ClassVar[int] = 1024

    @staticmethod
    @functools.cache
    def tile() -> npt.NDArray[np.float32]:
        """Bakes the seamless tile of four standardized normal channels low-pass filtered to unit texel deviation."""
        return tiles.gaussian(rng(content_hash(Procedural, providers=(np,))), Procedural.side, 4)

    @staticmethod
    @functools.cache
    def key() -> ContentHash:
        """Returns the content key naming the packed tile."""
        return content_hash(Procedural, arrays=(Procedural.tile(),))

    @staticmethod
    @functools.cache
    def correlation() -> npt.NDArray[np.float64]:
        """Returns the baked tile's neighbor correlations."""
        return tiles.correlated(Procedural.tile())


class Grain(Effect, frozen=True, kw_only=True):
    """Multiplicative film grain or additive sensor noise, the scanned grain image tiled over the frame when one is held."""

    stage = Stage.GRAIN
    pointwise = False
    scan: str | None = None


# --- [OPERATIONS] -----------------------------------------------------------------------


def _grain(shell: Shell[Grain]) -> Outcome[Expr]:
    """Builds the selected medium on straight scene-linear color, film over the tiled field and sensor over hashed normal draws."""
    source, share = shell.color, shell.input(_CHROMA)
    own, shared = math("SQRT", share), math("SQRT", 1.0 - share)
    deviation = shell.input(_GRANULARITY)
    pattern = math("FLOOR", clocked(*(shell.input(socket) for socket in CLOCK.inputs)))

    def developed(content: Generated, texels: int, extent: Expr, correlation: npt.NDArray[np.float64]) -> Expr:
        image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(content), socket="Image", shape=bpy.types.NodeSocketColor)
        shift = vector("SCALE", vector("FRACTION", origin(shell.input(_SEED), float(stream(Grain, "film")), pattern)), extent)
        scores = tiled(image, vector("MULTIPLY_ADD", shift, (1.0, 1.0, 0.0), shell.space.point), texels / extent, correlation)
        normal = vector("MULTIPLY_ADD", scores, own, scores["Alpha"] * shared)
        return vector("POWER", (np.e,) * 3, vector("MULTIPLY_ADD", normal, deviation, deviation * deviation * -0.5))

    def fielded(space: str) -> Outcome[Expr]:
        tag = Tag(space=Some(space), alpha=Some("CHANNEL_PACKED"))
        procedural = Generated(ownership=TILES, content=Procedural.key(), name="Grain Procedural", plane=lambda: Ok(Some(Procedural.tile())), tag=tag)

        def scanned(path: str) -> Outcome[Expr]:
            key = content_hash(Grain, path.encode())

            def scores(found: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
                color = found[..., (0, 1, 2) if found.shape[2] >= 3 else (0, 0, 0)]
                return tiles.ranked(np.dstack((color, color.mean(axis=2, dtype=np.float64))))

            plane = (
                TILES
                .found(shell.identity, content=Some(key))
                .map(lambda image: read(image, shell.config).map(lambda file: file.pixels))
                .default_with(lambda: read_file(Path(bpy.path.abspath(path)), shell.config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(lambda image: scores(image.pixels)))
            )
            return plane.map(
                lambda values: developed(
                    Generated(ownership=TILES, content=key, name=f"Grain {Path(path).name}", plane=lambda: Ok(Some(values)), tag=tag), values.shape[1], shell.input(_SPAN), tiles.correlated(values)
                )
            )

        held: Outcome[Option[Expr]] = Option.of_optional(shell.record.scan).map(lambda path: scanned(path).map(Some)).default_value(Ok(Nothing))
        return held.map(
            lambda scan: shell.menu(
                _FIELD,
                cases={_PROCEDURAL: developed(procedural, Procedural.side, shell.input(_SIZE) * (Procedural.side / 2.0), Procedural.correlation()), _SCAN: scan.default_value((1.0, 1.0, 1.0, 1.0))},
            ).named("field")
        )

    def filmed(factor: Expr, level: Expr) -> Expr:
        def band(distance: Expr, width: Expr) -> Expr:
            return where(width > 0.0, MapRange(interpolation_type="SMOOTHSTEP").node(distance / width), distance > 0.0)

        low, middle, high, edge, middle_extent, high_extent = map(shell.input, (_SHADOWS, _MIDTONES, _HIGHLIGHTS, _SHADOW_EDGE, _MIDTONE_SPAN, _HIGHLIGHT_SPAN))
        toned = math("MULTIPLY_ADD", 1.0 - band(level, edge), low - middle, middle)
        weight = math("MULTIPLY_ADD", band(level - edge - middle_extent, high_extent), high - middle, toned).named("weight")
        return mixed(weight, source, factor, blend="MULTIPLY")

    def sensored(level: Expr) -> Expr:
        grid = PixelGrid.of(source)
        sited = vector("FLOOR", grid.base / grid.inverse) + origin(shell.input(_SEED), float(stream(Grain, "sensor")), pattern)
        draws = tuple(White(noise_dimensions="4D").node(sited, float(index)) for index in range(4))
        (common, red), (green, blue) = gaussian(draws[0], draws[1], EXPRESSIONS), gaussian(draws[2], draws[3], EXPRESSIONS)
        photon, read_noise = shell.input(_SHOT), shell.input(_READ)
        gain, floor = photon * photon / GREY, read_noise * read_noise
        spread = vector("POWER", vector("MULTIPLY_ADD", vector("MAXIMUM", source, (0.0, 0.0, 0.0)), gain, floor), (0.5, 0.5, 0.5)).named("deviation")
        scatter = vector("SCALE", xyz(red, green, blue), own) * spread + common * shared * math("SQRT", math("MULTIPLY_ADD", math("MAXIMUM", level, 0.0), gain, floor))
        return where(equal(source["Alpha"], 0.0), source, vector("ADD", source, scatter))

    def built(level: Expr, factor: Expr) -> Expr:
        return shell.menu(_MEDIUM, cases={_FILM: filmed(factor, level), _SENSOR: sensored(level)})

    return paired(luminance(shell.config, source), space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(fielded)).map(lambda parts: built(*parts))


grain: Final = CompositorEffect(
    graph=_grain,
    record=Grain,
    mix=Blended(alpha=Alpha.STRAIGHT),
    interface=Interface(inputs=(_MEDIUM, _CHROMA, _SEED), panels=(_FILM_PANEL, _SENSOR_PANEL, CLOCK)),
    request=Some(precise),
    registers=(TILES,),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grain", "Procedural", "grain"]
