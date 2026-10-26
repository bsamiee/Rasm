"""Film grain multiplying light by a lognormal factor over tiled normal scores, and sensor noise of shot and read deviation on scene-linear light."""

import functools
from pathlib import Path
from typing import Annotated, ClassVar, Final

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.ids.images import read, Tag
from blender.bpy.data.ids.ownership import Ownership, Saved
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import End, Expr, Generated, math, mixed, NodeGroup, NODES, vector, where, xyz
from blender.bpy.nodes.interface import item_key
from blender.bpy.nodes.textures import gaussian, MapRange, stream, White
from blender.bpy.render.compositor.colors import COLOR_INPUT, space_item
from blender.bpy.render.compositor.coordinates import Clock, clocked, gridded, origin
from blender.bpy.render.compositor.effects import Alpha, CompositorEffect, Inspects, precise, Registers, Requests, Shell
from blender.bpy.render.compositor.filters import tiled
from blender.bpy.render.compositor.masks import LUMINANCE, luminance
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color.processors import GREY
from blender.imaging.faults import ImagingResult
from blender.imaging.files.images import read_file
from blender.imaging.geometry.regions import FrameExtent
from blender.imaging.sampling import tiles
from blender.records.faults import paired
from blender.records.fields import Case, NonNegative, Presentation, Seed, Unit
from blender.records.identity import content_hash, ContentHash, rng

from ...chain.stages import Effect, Stage

# --- [TYPES] ----------------------------------------------------------------------------

type Source = Procedural | Scan
type Medium = Film | Sensor
type Deviation = Annotated[float, msgspec.Meta(ge=0.0, le=float(np.sqrt(np.finfo(np.float32).max)))]

# --- [MODELS] ---------------------------------------------------------------------------


class Procedural(Case, frozen=True, kw_only=True):
    """Seeded Gaussian grain of two texel diameters baked into a seamless tile."""

    side: ClassVar[int] = 1024
    size: Annotated[FrameExtent, msgspec.Meta(description="Grain diameter over greater frame side"), Presentation(soft=Some((0.0002, 0.005)), precision=Some(5))] = 2.0 / 1920.0

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


class Scan(Case, frozen=True, kw_only=True):
    """Scanned grain image tiled over the frame."""

    path: Annotated[str, msgspec.Meta(title="File", description="Image OpenImageIO reads"), GraphForm.IMAGE]
    span: Annotated[FrameExtent, msgspec.Meta(description="Tile width over greater frame side"), Presentation(soft=Some((0.05, 1.0)))] = 1.0

    def tile(self, config: PyOpenColorIO.Config) -> ImagingResult[npt.NDArray[np.float32]]:
        """Reads data samples into normal scores of their color channels and their mean."""
        return read_file(self.path, config, colorspace=Some(PyOpenColorIO.ROLE_DATA)).map(
            lambda image: tiles.ranked(np.dstack(((color := image.pixels[..., (0, 1, 2) if image.pixels.shape[2] >= 3 else (0, 0, 0)]), color.mean(axis=2, dtype=np.float64))))
        )


class Film(Case, frozen=True, kw_only=True):
    """Grain multiplying light by a lognormal factor, weighted by tone."""

    field: Annotated[Source, msgspec.Meta(description="Procedural grain or scanned image")] = Procedural()
    granularity: Annotated[Deviation, msgspec.Meta(description="Deviation of the log grain factor"), Presentation(soft=Some((0.0, 0.5)))] = 0.0
    shadows: Annotated[Unit, msgspec.Meta(description="Grain weight below the shadow edge")] = 1.0
    midtones: Annotated[Unit, msgspec.Meta(description="Grain weight between the shadow and highlight bands")] = 1.0
    highlights: Annotated[Unit, msgspec.Meta(description="Grain weight above the highlight band")] = 1.0
    shadow_edge: Annotated[NonNegative, msgspec.Meta(description="Luminance zeroing the shadow weight"), Presentation(soft=Some((0.0, 0.5)), precision=Some(3))] = 0.09
    midtone_span: Annotated[NonNegative, msgspec.Meta(description="Luminance from the shadow edge to the highlight band"), Presentation(soft=Some((0.0, 2.0)), precision=Some(3))] = 0.41
    highlight_span: Annotated[NonNegative, msgspec.Meta(description="Luminance from the highlight start to full weight"), Presentation(soft=Some((0.0, 4.0)), precision=Some(3))] = 0.5


class Sensor(Case, frozen=True, kw_only=True):
    """Shot noise scaling with signal and constant read noise."""

    shot: Annotated[
        float,
        msgspec.Meta(ge=0.0, le=float(np.sqrt(np.float32(np.finfo(np.float32).max) * GREY)), description="Photon noise deviation at scene-linear gray"),
        Presentation(soft=Some((0.0, 0.1)), precision=Some(4)),
    ] = 0.046875 * float(np.sqrt(GREY))
    read: Annotated[Deviation, msgspec.Meta(description="Signal-independent deviation"), Presentation(soft=Some((0.0, 0.05)), precision=Some(4))] = 0.0


class Grain(Effect, frozen=True, kw_only=True):
    """Multiplicative film grain or additive sensor noise."""

    stage = Stage.GRAIN
    pointwise = False
    medium: Annotated[Medium, msgspec.Meta(description="Film grain or sensor noise")] = Film()
    chroma: Annotated[Unit, msgspec.Meta(description="Per-channel noise share against shared noise")] = 0.0
    clock: Annotated[Clock, msgspec.Meta(description="Pattern change clock")] = Clock()
    seed: Annotated[Seed, msgspec.Meta(description="Number selecting the pattern")] = 0


# --- [OPERATIONS] -----------------------------------------------------------------------


def grained(shell: Shell[Grain]) -> BlenderResult[Expr]:
    """Builds the selected medium on straight scene-linear color, film over the tiled field and sensor over hashed normal draws."""
    medium, chroma, clock, seed = Grain.__struct_fields__
    field, granularity, shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span = Film.__struct_fields__
    (size,) = Procedural.__struct_fields__
    _, span = Scan.__struct_fields__
    shot, readout = Sensor.__struct_fields__
    speed, hold = Clock.__struct_fields__
    source, share = shell.color, shell.input(chroma)
    own, shared = math("SQRT", share), math("SQRT", 1.0 - share)
    deviation = shell.input(medium, Film, granularity)
    pattern = math("FLOOR", clocked(shell.input(clock, speed), shell.input(clock, hold)))

    def developed(content: Generated, texels: int, extent: Expr, correlation: npt.NDArray[np.float64]) -> Expr:
        image = Expr(kind=bpy.types.CompositorNodeImage, content=Some(content), socket="Image", shape="RGBA")
        shift = vector("SCALE", vector("FRACTION", origin(shell.input(seed), float(stream(Film)), pattern)), extent)
        scores = tiled(image, vector("MULTIPLY_ADD", shift, (1.0, 1.0, 0.0), shell.space.point), texels / extent, correlation)
        normal = vector("MULTIPLY_ADD", scores, own, scores["Alpha"] * shared)
        return vector("POWER", (float(np.e),) * 3, vector("MULTIPLY_ADD", normal, deviation, deviation * deviation * -0.5))

    def fielded(space: str) -> BlenderResult[Expr]:
        tag = Tag(space=Some(space), alpha=Some("CHANNEL_PACKED"))
        procedural = Generated(ownership=TILES, content=Procedural.key(), name=f"{Grain.title} {Procedural.title}", plane=lambda: Ok(Some(Procedural.tile())), tag=tag)

        def scanned(scan: Scan) -> BlenderResult[Expr]:
            key = content_hash(Scan, Scan(path=scan.path))
            plane = (
                TILES
                .found(shell.identity, content=Some(key))
                .map(lambda image: read(image, shell.config).map(lambda file: file.pixels))
                .default_with(lambda: Scan(path=bpy.path.abspath(scan.path)).tile(shell.config))
            )
            return plane.map(
                lambda values: developed(
                    Generated(ownership=TILES, content=key, name=f"{Grain.title} {Path(scan.path).name}", plane=lambda: Ok(Some(values)), tag=tag),
                    values.shape[1],
                    shell.input(medium, Film, field, Scan, span),
                    tiles.correlated(values),
                )
            )

        held: BlenderResult[Option[Expr]] = shell.case(Scan, medium, Film, field).map(lambda scan: scanned(scan).map(Some)).default_value(Ok(Nothing))
        return held.map(
            lambda scan: shell.menu(
                medium,
                Film,
                field,
                cases={
                    Procedural.title: developed(procedural, Procedural.side, shell.input(medium, Film, field, Procedural, size) * (Procedural.side / 2.0), Procedural.correlation()),
                    Scan.title: scan.default_value((1.0, 1.0, 1.0, 1.0)),
                },
                kind="RGBA",
            ).named("field")
        )

    def filmed(factor: Expr, level: Expr) -> Expr:
        def band(distance: Expr, width: Expr) -> Expr:
            return where(width > 0.0, MapRange(interpolation_type="SMOOTHSTEP").node(distance / width), distance > 0.0)

        low, middle, high, edge, middle_extent, high_extent = (shell.input(medium, Film, name) for name in (shadows, midtones, highlights, shadow_edge, midtone_span, highlight_span))
        toned = math("MULTIPLY_ADD", 1.0 - band(level, edge), low - middle, middle)
        weight = math("MULTIPLY_ADD", band(level - edge - middle_extent, high_extent), high - middle, toned).named("weight")
        return mixed(weight, source, factor, blend="MULTIPLY")

    def sensored(level: Expr) -> Expr:
        grid = gridded(source)
        sited = vector("FLOOR", grid.base / grid.inverse) + origin(shell.input(seed), float(stream(Sensor)), pattern)
        draws = tuple(White(noise_dimensions="4D").node(sited, float(index)) for index in range(4))
        (common, red), (green, blue) = gaussian(draws[0], draws[1], NODES), gaussian(draws[2], draws[3], NODES)
        photon, read_noise = shell.input(medium, Sensor, shot), shell.input(medium, Sensor, readout)
        gain, floor = photon * photon / GREY, read_noise * read_noise
        spread = vector("POWER", vector("MULTIPLY_ADD", vector("MAXIMUM", source, (0.0, 0.0, 0.0)), gain, floor), (0.5, 0.5, 0.5)).named("deviation")
        scatter = vector("SCALE", xyz(red, green, blue), own) * spread + common * shared * math("SQRT", math("MULTIPLY_ADD", math("MAXIMUM", level, 0.0), gain, floor))
        empty = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", source["Alpha"]), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
        return where(empty, source, vector("ADD", source, scatter))

    def built(working: NodeGroup, factor: Expr) -> Expr:
        level = working.placed({COLOR_INPUT.key: source}, LUMINANCE.key)
        return shell.menu(medium, cases={Film.title: filmed(factor, level), Sensor.title: sensored(level)}, kind="RGBA")

    return paired(luminance(shell.config), space_item(shell.config, PyOpenColorIO.ROLE_DATA).bind(fielded)).map(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

TILES: Final = Ownership(kind=bpy.types.Image, key="grain_tile", saved=Saved.USED)
grain = CompositorEffect(
    record=Grain,
    graph=grained,
    alpha=Alpha.STRAIGHT,
    parts=(
        Requests[Grain](request=precise),
        *(
            Inspects(key=item_key(Grain, node), end=End(node=node, socket=socket), description=description)
            for node, socket, description in (
                ("weight", "Value", "Film grain tonal weight"),
                ("field", "Output", "Developed film grain factor"),
                ("deviation", "Vector", "Signal-dependent channel deviation before the noise draw"),
            )
        ),
        Registers(rows=(TILES,)),
    ),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Film", "Grain", "Procedural", "Scan", "Sensor", "grain"]
