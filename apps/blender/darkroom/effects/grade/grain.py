"""Film grain multiplying light by a lognormal factor over tiled normal scores, and sensor noise of shot and read deviation on scene-linear light."""

import functools
from pathlib import Path
from typing import Final, override

import bpy
from expression import Option, Some
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.data import normalized, Ownership, Saved, Tag
from rasm_bpy.nodes import equal, Expr, EXPRESSIONS, Generated, Interface, InterfacePanel, MapRange, math, MenuItem, mixed, Socket, vector, where, WhiteNoise, xyz
from rasm_bpy.render import CLOCK, Clock, clocked, CompositorEffect, CompositorRecord, FilePath, GraphInputs, luminance, Mix, motion, origin, PixelGrid, precise, SEED, space_item, tiled
from rasm_imaging import gaussian, GRAY, read_file, rng, stream, tiles
from rasm_records import content_hash, ContentHash, Nothing, Ok, Outcome, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

_TILES: Final = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)
_SIDE: Final = 1024
_PROCEDURAL: Final = MenuItem(name="Procedural", description="Seeded Gaussian grain of two texel diameters baked into a seamless tile")
_SCAN: Final = MenuItem(name="Scan", description="Scanned grain image tiled over the frame")
_FILM: Final = InterfacePanel.of(
    "film",
    Interface(
        inputs=(
            Socket(key="field", kind=bpy.types.NodeSocketMenu, description="Procedural grain or scanned image", default_value=_PROCEDURAL.name, items=(_PROCEDURAL, _SCAN)),
            Socket(key="size", kind=bpy.types.NodeSocketFloat, description="Grain diameter over greater frame side", min_value=0.0, default_value=0.00104),
            Socket(key="span", kind=bpy.types.NodeSocketFloat, description="Tile width over greater frame side", min_value=0.0, default_value=1.0),
            Socket(key="granularity", kind=bpy.types.NodeSocketFloat, description="Deviation of the log grain factor", min_value=0.0),
            Socket(key="shadows", kind=bpy.types.NodeSocketFloatFactor, description="Grain weight below the shadow edge", default_value=1.0),
            Socket(key="midtones", kind=bpy.types.NodeSocketFloatFactor, description="Grain weight between the shadow and highlight bands", default_value=1.0),
            Socket(key="highlights", kind=bpy.types.NodeSocketFloatFactor, description="Grain weight above the highlight band", default_value=1.0),
            Socket(key="shadow_edge", kind=bpy.types.NodeSocketFloat, description="Luminance zeroing the shadow weight", min_value=0.0, default_value=0.09),
            Socket(key="midtone_span", kind=bpy.types.NodeSocketFloat, description="Luminance from the shadow edge to the highlight band", min_value=0.0, default_value=0.41),
            Socket(key="highlight_span", kind=bpy.types.NodeSocketFloat, description="Luminance from the highlight start to full weight", min_value=0.0, default_value=0.5),
        )
    ),
    description="Grain multiplying light by a lognormal factor, weighted by tone",
)
_SENSOR: Final = InterfacePanel.of(
    "sensor",
    Interface(
        inputs=(
            Socket(key="shot", kind=bpy.types.NodeSocketFloat, description="Photon noise deviation at scene-linear gray", min_value=0.0, default_value=0.046875 * float(np.sqrt(GRAY))),
            Socket(key="read", kind=bpy.types.NodeSocketFloat, description="Signal-independent deviation", min_value=0.0),
        )
    ),
    description="Shot noise scaling with signal and constant read noise",
)
_MEDIUM: Final = Socket(key="medium", kind=bpy.types.NodeSocketMenu, description="Film grain or sensor noise", default_value=_FILM.item.name, items=(_FILM.item, _SENSOR.item))
_CHROMA: Final = Socket(key="chroma", kind=bpy.types.NodeSocketFloatFactor, description="Per-channel noise share against shared noise")
_CLOCK: Final = InterfacePanel.of(CLOCK.key, Clock(speed=24.0))

# --- [MODELS] ---------------------------------------------------------------------------


class Grain(CompositorRecord, frozen=True, kw_only=True):
    """Multiplicative film grain or additive sensor noise, the scanned grain image tiled over the frame when one is held."""

    scan: FilePath | None = None

    @override
    def graph(self, shell: GraphInputs) -> Outcome[Expr]:
        """Builds the selected medium on straight scene-linear color, film over the tiled field and sensor over hashed normal draws."""
        field, size, span, granularity, shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span = _FILM.inputs
        shot, read_noise = map(shell.input, _SENSOR.inputs)
        source, share = shell.color, shell.input(_CHROMA)
        own, shared = math("SQRT", share), math("SQRT", 1.0 - share)
        deviation = shell.input(granularity)
        pattern = math("FLOOR", clocked(shell.input, _CLOCK))

        def developed(content: Generated, texels: int, extent: Expr, correlation: npt.NDArray[np.float64]) -> Expr:
            image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(content), socket="Image", socket_type=bpy.types.NodeSocketColor)
            shift = vector("SCALE", vector("FRACTION", origin(shell.input(SEED), float(stream(Grain, _FILM.key)), pattern)), extent)
            scores = tiled(image, vector("MULTIPLY_ADD", shift, (1.0, 1.0, 0.0), shell.space.point), texels / extent, correlation)
            normal = vector("MULTIPLY_ADD", scores, own, scores["Alpha"] * shared)
            return vector("POWER", (np.e,) * 3, vector("MULTIPLY_ADD", normal, deviation, deviation * deviation * -0.5))

        def fielded(space: str) -> Outcome[Expr]:
            tag = Tag(space=Some(space), alpha=Some("CHANNEL_PACKED"))
            tile, key, correlation = _procedural()
            procedural = Generated(ownership=_TILES, content=key, name="Grain Procedural", plane=lambda: Ok(Some(tile)), tag=tag)

            def scanned(path: str) -> Outcome[Expr]:
                held = content_hash(Grain, path.encode())

                def scores(found: npt.NDArray[np.float32]) -> npt.NDArray[np.float32]:
                    color = found[..., (0, 1, 2) if found.shape[2] >= 3 else (0, 0, 0)]
                    return tiles.normal_scores(np.dstack((color, color.mean(axis=2, dtype=np.float64))))

                plane = (
                    _TILES
                    .found(shell.identity, content=Some(held))
                    .map(lambda image: normalized(image, shell.config).map(lambda file: file.pixels))
                    .default_with(lambda: read_file(Path(bpy.path.abspath(path)), shell.config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(lambda image: scores(image.pixels)))
                )
                return plane.map(
                    lambda values: developed(
                        Generated(ownership=_TILES, content=held, name=f"Grain {Path(path).name}", plane=lambda: Ok(Some(values)), tag=tag),
                        values.shape[1],
                        shell.input(span),
                        tiles.correlated(values),
                    )
                )

            scan: Outcome[Option[Expr]] = Option.of_optional(self.scan).map(lambda path: scanned(path).map(Some)).default_value(Ok(Nothing))
            return scan.map(
                lambda held: shell.menu(field, cases={_PROCEDURAL: developed(procedural, _SIDE, shell.input(size) * (_SIDE / 2.0), correlation), _SCAN: held.default_value((1.0, 1.0, 1.0))}).keyed(
                    "field"
                )
            )

        def filmed(factor: Expr, level: Expr) -> Expr:
            def band(distance: Expr, width: Expr) -> Expr:
                return where(width > 0.0, MapRange(interpolation_type="SMOOTHSTEP").node(distance / width), distance > 0.0)

            low, middle, high, edge, middle_extent, high_extent = map(shell.input, (shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span))
            toned = math("MULTIPLY_ADD", 1.0 - band(level, edge), low - middle, middle)
            weight = math("MULTIPLY_ADD", band(level - edge - middle_extent, high_extent), high - middle, toned).keyed("weight")
            return mixed(weight, source, factor, blend="MULTIPLY")

        def sensored(level: Expr) -> Expr:
            grid = PixelGrid.of(source)
            sited = vector("FLOOR", grid.base / grid.inverse) + origin(shell.input(SEED), float(stream(Grain, _SENSOR.key)), pattern)
            draws = tuple(WhiteNoise(noise_dimensions="4D").node(sited, float(index)) for index in range(4))
            (common, red), (green, blue) = gaussian(draws[0], draws[1], EXPRESSIONS), gaussian(draws[2], draws[3], EXPRESSIONS)
            gain, floor = shot * shot / GRAY, read_noise * read_noise
            spread = vector("POWER", vector("MULTIPLY_ADD", vector("MAXIMUM", source, (0.0, 0.0, 0.0)), gain, floor), (0.5, 0.5, 0.5)).keyed("deviation")
            scatter = vector("SCALE", xyz(red, green, blue), own) * spread + common * shared * math("SQRT", math("MULTIPLY_ADD", math("MAXIMUM", level, 0.0), gain, floor))
            return where(equal(source["Alpha"], 0.0), source, vector("ADD", source, scatter))

        def built(level: Expr, factor: Expr) -> Expr:
            return shell.menu(_MEDIUM, cases={_FILM.item: filmed(factor, level), _SENSOR.item: sensored(level)})

        return paired(luminance(shell.config, source), space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(fielded)).map(lambda parts: built(*parts))

    requested = staticmethod(precise)


# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def _procedural() -> tuple[npt.NDArray[np.float32], ContentHash, npt.NDArray[np.float64]]:
    """Seamless tile of four standardized normal channels low-pass filtered to unit texel deviation, the content key naming its packed image, and its neighbor correlations."""
    tile = tiles.gaussian_tile(rng(content_hash(Grain, _PROCEDURAL.name.encode(), providers=(np,))), _SIDE, 4)
    return tile, content_hash(Grain, arrays=(tile,)), tiles.correlated(tile)


# --- [COMPOSITION] ----------------------------------------------------------------------

grain: Final = CompositorEffect(record=Grain, mix=Mix.STRAIGHT, interface=Interface(inputs=(_MEDIUM, _CHROMA, SEED), panels=(_FILM, _SENSOR, _CLOCK)), derives=(motion.timing,), registers=(_TILES,))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Grain", "grain"]
