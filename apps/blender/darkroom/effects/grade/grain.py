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
from blender.bpy.nodes.graphs import Expr, Generated, math, mixed, NodeGroup, vector, where, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.nodes.textures import gaussian, MapRange, stream, White
from blender.bpy.render.compositor.colors import COLOR_INPUT, space_item
from blender.bpy.render.compositor.coordinates import clocked, gridded, origin
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, precise, Registers, Requests, Shell
from blender.bpy.render.compositor.filters import tiled
from blender.bpy.render.compositor.masks import LUMINANCE, luminance
from blender.imaging.color.processors import GREY
from blender.imaging.files.images import read_file
from blender.imaging.sampling import tiles
from blender.records.faults import Outcome, paired
from blender.records.identity import content_hash, ContentHash, rng

from ...chain.stages import Effect, Stage
from ..optics.procedural import CLOCK

# --- [CONSTANTS] ------------------------------------------------------------------------

TILES: Final = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)

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


def grained(shell: Shell[Grain]) -> Outcome[Expr]:
    """Builds the selected medium on straight scene-linear color, film over the tiled field and sensor over hashed normal draws."""
    source, share = shell.color, shell.input("chroma")
    own, shared = math("SQRT", share), math("SQRT", 1.0 - share)
    deviation = shell.input("film", "granularity")
    pattern = math("FLOOR", clocked(shell.input("clock_speed"), shell.input("clock_hold")))

    def developed(content: Generated, texels: int, extent: Expr, correlation: npt.NDArray[np.float64]) -> Expr:
        image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(content), socket="Image", shape="RGBA")
        shift = vector("SCALE", vector("FRACTION", origin(shell.input("seed"), float(stream(Grain, "film")), pattern)), extent)
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
                .default_with(lambda: read_file(bpy.path.abspath(path), shell.config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(lambda image: scores(image.pixels)))
            )
            return plane.map(
                lambda values: developed(
                    Generated(ownership=TILES, content=key, name=f"Grain {Path(path).name}", plane=lambda: Ok(Some(values)), tag=tag),
                    values.shape[1],
                    shell.input("film", "span"),
                    tiles.correlated(values),
                )
            )

        held: Outcome[Option[Expr]] = Option.of_optional(shell.record.scan).map(lambda path: scanned(path).map(Some)).default_value(Ok(Nothing))
        return held.bind(
            lambda scan: shell.menu(
                "film_field",
                cases={
                    "Procedural": developed(procedural, Procedural.side, shell.input("film", "size") * (Procedural.side / 2.0), Procedural.correlation()),
                    "Scan": scan.default_value((1.0, 1.0, 1.0, 1.0)),
                },
                kind="RGBA",
            ).map(lambda field: field.named("field"))
        )

    def filmed(factor: Expr, level: Expr) -> Expr:
        def band(distance: Expr, width: Expr) -> Expr:
            return where(width > 0.0, MapRange(interpolation_type="SMOOTHSTEP").node(distance / width), distance > 0.0)

        low, middle, high, edge, middle_extent, high_extent = (shell.input("film", name) for name in ("shadows", "midtones", "highlights", "shadow_edge", "midtone_span", "highlight_span"))
        toned = math("MULTIPLY_ADD", 1.0 - band(level, edge), low - middle, middle)
        weight = math("MULTIPLY_ADD", band(level - edge - middle_extent, high_extent), high - middle, toned).named("weight")
        return mixed(weight, source, factor, blend="MULTIPLY")

    def sensored(level: Expr) -> Expr:
        grid = gridded(source)
        sited = vector("FLOOR", grid.base / grid.inverse) + origin(shell.input("seed"), float(stream(Grain, "sensor")), pattern)
        draws = tuple(White(noise_dimensions="4D").node(sited, float(index)) for index in range(4))
        (common, red), (green, blue) = gaussian(draws[0], draws[1], Expr), gaussian(draws[2], draws[3], Expr)
        photon, read_noise = shell.input("sensor", "shot"), shell.input("sensor", "read")
        gain, floor = photon * photon / GREY, read_noise * read_noise
        spread = vector("POWER", vector("MULTIPLY_ADD", vector("MAXIMUM", source, (0.0, 0.0, 0.0)), gain, floor), (0.5, 0.5, 0.5)).named("deviation")
        scatter = vector("SCALE", xyz(red, green, blue), own) * spread + common * shared * math("SQRT", math("MULTIPLY_ADD", math("MAXIMUM", level, 0.0), gain, floor))
        empty = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", source["Alpha"]), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
        return where(empty, source, vector("ADD", source, scatter))

    def built(working: NodeGroup, factor: Expr) -> Outcome[Expr]:
        level = working.placed({COLOR_INPUT.key: source}, LUMINANCE.key)
        return shell.menu("medium", cases={"Film": filmed(factor, level), "Sensor": sensored(level)}, kind="RGBA")

    return paired(luminance(shell.config), space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(fielded)).bind(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

grain = CompositorEffect(
    record=Grain,
    graph=grained,
    alpha=Alpha.STRAIGHT,
    interface=Interface(
        inputs=(
            Socket(
                key="medium",
                name="Medium",
                kind=bpy.types.NodeSocketMenu,
                description="Film grain or sensor noise",
                members={"default_value": "Film"},
                items=Some(
                    MenuItems(
                        items=(
                            MenuItem(name="Film", description="Grain multiplying light by a lognormal factor, weighted by tone"),
                            MenuItem(name="Sensor", description="Shot noise scaling with signal and constant read noise"),
                        )
                    )
                ),
            ),
            Socket(
                key="chroma",
                name="Chroma",
                kind=bpy.types.NodeSocketFloat,
                description="Per-channel noise share against shared noise",
                members={"subtype": "FACTOR", "min_value": 0.0, "max_value": 1.0},
            ),
            Socket(key="seed", name="Seed", kind=bpy.types.NodeSocketInt, description="Number selecting the pattern", members={"min_value": 0, "max_value": 2**24}),
        ),
        panels=(
            InterfacePanel(
                key="film",
                name="Film",
                description="Grain multiplying light by a lognormal factor, weighted by tone",
                inputs=(
                    Socket(
                        key="film_field",
                        name="Field",
                        kind=bpy.types.NodeSocketMenu,
                        description="Procedural grain or scanned image",
                        members={"default_value": "Procedural"},
                        items=Some(
                            MenuItems(
                                items=(
                                    MenuItem(name="Procedural", description="Seeded Gaussian grain of two texel diameters baked into a seamless tile"),
                                    MenuItem(name="Scan", description="Scanned grain image tiled over the frame"),
                                )
                            )
                        ),
                    ),
                    *(
                        Socket(key=item_key("film", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members={"subtype": subtype, **members})
                        for field, text, subtype, members in (
                            ("size", "Grain diameter over greater frame side", "NONE", {"min_value": 0.0, "default_value": 0.00104}),
                            ("span", "Tile width over greater frame side", "NONE", {"min_value": 0.0, "default_value": 1.0}),
                            ("granularity", "Deviation of the log grain factor", "NONE", {"min_value": 0.0}),
                            ("shadows", "Grain weight below the shadow edge", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                            ("midtones", "Grain weight between the shadow and highlight bands", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                            ("highlights", "Grain weight above the highlight band", "FACTOR", {"min_value": 0.0, "max_value": 1.0, "default_value": 1.0}),
                            ("shadow_edge", "Luminance zeroing the shadow weight", "NONE", {"min_value": 0.0, "default_value": 0.09}),
                            ("midtone_span", "Luminance from the shadow edge to the highlight band", "NONE", {"min_value": 0.0, "default_value": 0.41}),
                            ("highlight_span", "Luminance from the highlight start to full weight", "NONE", {"min_value": 0.0, "default_value": 0.5}),
                        )
                    ),
                ),
            ),
            InterfacePanel(
                key="sensor",
                name="Sensor",
                description="Shot noise scaling with signal and constant read noise",
                inputs=tuple(
                    Socket(key=item_key("sensor", field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloat, description=text, members=members)
                    for field, text, members in (
                        ("shot", "Photon noise deviation at scene-linear gray", {"min_value": 0.0, "default_value": 0.046875 * float(np.sqrt(GREY))}),
                        ("read", "Signal-independent deviation", {"min_value": 0.0}),
                    )
                ),
            ),
            CLOCK,
        ),
    ),
    parts=(Requests[Grain](request=precise), Registers(rows=(TILES,))),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grain", "Procedural", "grain"]
