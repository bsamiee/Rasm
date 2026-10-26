"""Procedural textures cut to density over greater frame sides and the light they emit, sampled as nodes and on the CPU through the host texture records."""

from collections.abc import Callable, Mapping
import functools
from math import pi, tau
from typing import Annotated, Literal

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes import textures
from blender.bpy.nodes.graphs import DataType, Expr, math, menu, mixed, NodeGroup, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key, MenuItems
from blender.bpy.nodes.ramps import Ramp
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.colors import COLOR_INPUT
from blender.bpy.render.compositor.coordinates import origin
from blender.bpy.render.compositor.effects import Shell
from blender.bpy.render.compositor.inputs import KeyPath
from blender.bpy.settings.shapes import GraphForm
from blender.imaging.color import observer, processors
from blender.imaging.color.processors import Stops
from blender.imaging.geometry.arithmetic import Field, Namespace
from blender.imaging.geometry.regions import FrameExtent
from blender.records.fields import Case, Finite, NonNegative, Positive, Presentation, Quantity, Rgb, Unit

from ...chain.stages import Effect

# --- [TYPES] ----------------------------------------------------------------------------

type WaveProfile = textures.WaveProfile | Literal["SQUARE"]
type Basis = Perlin | Voronoi | Wave | Gabor | Magic | White
type Spectrum = Tint | Incandescence | Gradient
type Inlet = Callable[..., Expr]

# --- [MODELS] ---------------------------------------------------------------------------

# --- [TEXTURES]


class Perlin(Case, frozen=True, kw_only=True):
    """Fractal Perlin noise over greater frame sides cut to a density."""

    title = "Perlin Noise"
    texture: Annotated[textures.Noise, msgspec.Meta(title="Noise", description="Noise texture, scale counting features per greater frame side")] = textures.Noise(noise_dimensions="4D", scale=1.0)
    stretch: Annotated[Positive, msgspec.Meta(description="Feature height over width"), Presentation(soft=Some((0.25, 4.0)))] = 1.0
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Noise value mapped to density")] = textures.MapRange()


class Voronoi(Case, frozen=True, kw_only=True):
    """Cellular distances over greater frame sides cut to a density."""

    texture: Annotated[textures.Voronoi, msgspec.Meta(title="Cells", description="Voronoi texture, scale counting cells per greater frame side")] = textures.Voronoi(
        voronoi_dimensions="4D", normalize=True, scale=10.0
    )
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Distance mapped to density")] = textures.MapRange(interpolation_type="SMOOTHSTEP")
    cell_color: Annotated[bool, msgspec.Meta(description="Cell red sets tone, green scales density")] = False

    @property
    def colored(self) -> textures.Voronoi:
        """Cells whose `Color` belongs to the nearest feature, F1 Euclidean cells where the feature measures edges."""
        return msgspec.structs.replace(self.texture, feature="F1", distance="EUCLIDEAN") if self.texture.feature == "DISTANCE_TO_EDGE" else self.texture


class Wave(Case, frozen=True, kw_only=True):
    """Bands or rings of a periodic profile over greater frame sides cut to a density."""

    wavelength: Annotated[FrameExtent, msgspec.Meta(description="Band or ring spacing over greater frame side"), Presentation(soft=Some((0.005, 1.0)), precision=Some(4))] = 0.1
    pattern: Annotated[Literal["BANDS", "RINGS"], msgspec.Meta(description="Bands across field or rings about center"), GraphForm.SETTING] = "BANDS"
    profile: Annotated[WaveProfile, msgspec.Meta(description="Shape of each period"), GraphForm.SETTING] = "SIN"
    phase: Annotated[Finite, msgspec.Meta(description="Profile phase offset"), Presentation(quantity=Some(Quantity.ANGLE))] = 0.0
    distortion: Annotated[Finite, msgspec.Meta(description="Distortion noise phase offset in radians"), Presentation(soft=Some((-20.0, 20.0)))] = 0.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0, description="Distortion noise octaves past first"), Presentation(soft=Some((0.0, 8.0)), precision=Some(1))] = 2.0
    detail_scale: Annotated[NonNegative, msgspec.Meta(description="Distortion noise frequency"), Presentation(soft=Some((0.0, 10.0)))] = 1.0
    detail_roughness: Annotated[Unit, msgspec.Meta(description="Distortion octave amplitude over previous")] = 0.5
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Wave value mapped to density")] = textures.MapRange(interpolation_type="SMOOTHSTEP")

    @property
    def texture(self) -> textures.Wave:
        """Wave Texture of one period per wavelength unit, square periods read from its saw profile."""
        return textures.Wave(
            wave_type=self.pattern,
            rings_direction="Z",
            wave_profile="SAW" if self.profile == "SQUARE" else self.profile,
            scale=pi / 10.0,
            distortion=self.distortion,
            detail=self.detail,
            detail_scale=self.detail_scale,
            detail_roughness=self.detail_roughness,
            phase_offset=self.phase,
        )


class Gabor(Case, frozen=True, kw_only=True):
    """Sparse windowed oscillations over greater frame sides cut to a density."""

    texture: Annotated[textures.Gabor, msgspec.Meta(title="Oscillations", description="Gabor texture, scale counting impulse cells per greater frame side")] = textures.Gabor(
        gabor_type="3D", scale=10.0
    )
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Oscillation value mapped to density")] = textures.MapRange(interpolation_type="SMOOTHSTEP")


class Magic(Case, frozen=True, kw_only=True):
    """Nested trigonometric turbulence over greater frame sides cut to a density."""

    texture: Annotated[textures.Magic, msgspec.Meta(title="Turbulence", description="Magic texture, scale counting patterns per greater frame side")] = textures.Magic(scale=10.0)
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Mean color mapped to density")] = textures.MapRange(interpolation_type="SMOOTHSTEP")


class White(Case, frozen=True, kw_only=True):
    """Constant random values on lattice cells cut to a density."""

    title = "White Noise"
    texture: Annotated[textures.White, msgspec.Meta(title="Noise", description="White noise texture")] = textures.White(noise_dimensions="4D")
    spacing: Annotated[FrameExtent, msgspec.Meta(description="Cell width over greater frame side")] = 0.1
    cut: Annotated[textures.MapRange, msgspec.Meta(title="Density", description="Cell value mapped to density")] = textures.MapRange(interpolation_type="SMOOTHSTEP")


# --- [LIGHT]


class Tint(Case, frozen=True, kw_only=True):
    """Color graded from start to end by field tone."""

    start: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear color at tone 0")] = Rgb(red=1.0, green=1.0, blue=1.0)
    end: Annotated[Rgb[Finite], msgspec.Meta(description="Scene-linear color at tone 1")] = Rgb(red=1.0, green=1.0, blue=1.0)


class Incandescence(Case, frozen=True, kw_only=True):
    """Unit-luminance blackbody color graded from cool to hot temperature by field tone."""

    cool: Annotated[float, msgspec.Meta(ge=800.0, le=12000.0, description="Temperature at tone 0"), Presentation(step=Some(1.0), precision=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE))] = 1000.0
    hot: Annotated[float, msgspec.Meta(ge=800.0, le=12000.0, description="Temperature at tone 1"), Presentation(step=Some(1.0), precision=Some(0), quantity=Some(Quantity.COLOR_TEMPERATURE))] = 2000.0


class Gradient(Case, frozen=True, kw_only=True):
    """Color read from a color ramp at field tone."""

    ramp: Annotated[Ramp, msgspec.Meta(description="Scene-linear colors along tone 0 to 1")] = Ramp()


class Emission(msgspec.Struct, frozen=True, kw_only=True):
    """Light a generator adds over its coverage, colored by tone."""

    spectrum: Annotated[Spectrum, msgspec.Meta(description="Color emitted at each tone")] = Incandescence()
    intensity: Annotated[Stops, msgspec.Meta(description="Luminance at full density in stops")] = 2.0


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [PROFILES]


def profiled[V: Field, B](profile: WaveProfile, cycles: V, ns: Namespace[V, B]) -> V:
    """Returns the periodic profile in [0, 1] at a cycle coordinate, the square profile floored from the saw."""
    match profile:
        case "SQUARE":
            return ns.floor(textures.periodic("SAW", cycles, ns) * 2.0)
        case _:
            return textures.periodic(profile, cycles, ns)


def density_range(field: Basis) -> tuple[float, float]:
    """Bounds the cut density, smooth-cell color corrections reaching below zero."""
    match field:
        case Voronoi(cell_color=True, texture=textures.Voronoi(feature="SMOOTH_F1") as cells):
            span = cells.smoothness / 2.0
            return -(5**cells.rank - 1) * span / (4.0 * (1.0 + 3.0 * span)), 1.0
        case _:
            return 0.0, 1.0


# --- [NODES]


def textured[R: Effect](shell: Shell[R], path: KeyPath, point: Expr, time: Scalar, seed: Scalar, *, loss: Scalar = 0.0, streams: tuple[int, *tuple[int, ...]] = (0,)) -> tuple[tuple[Expr, Expr], ...]:
    """Returns density and tone per stream of the basis menu at `path`, every stream switched by the one menu.

    Args:
        shell: Entry inputs and retained basis cases.
        path: Basis union path in the record.
        point: Greater-side frame position.
        time: Signed evolution coordinate.
        seed: Record seed operand.
        loss: Spatial loss subtracted before the cut.
        streams: Draw families in output order, from `textures.stream`.

    Returns:
        Density and tone per stream.
    """

    def drawn(field: Basis, stream: int) -> tuple[Expr, Expr]:
        inlet = functools.partial(shell.input, *path, type(field))
        offset = origin(seed, float(stream))
        density = field.cut.node(_raw(field, point, offset, time, inlet) - loss, linked=Some(_linked(inlet, "cut")))
        match field:
            case Voronoi():
                color = _node(field.colored, point + offset, time, inlet, socket=Some("Color"))
                return switched(inlet("cell_color"), density, density * color["Green"]), switched(inlet("cell_color"), density, color["Red"])
            case _:
                return density, density

    bases = tuple(case for case in shell.cases[path] if isinstance(case, Perlin | Voronoi | Wave | Gabor | Magic | White))
    titles = tuple(type(case).title for case in bases)
    pairs = tuple(tuple(drawn(case, stream) for case in bases) for stream in streams)
    chosen = _selected(
        shell.input(*path),
        shell.items[path],
        {item_key(stream, part): dict(zip(titles, (pair[index] for pair in held))) for stream, held in zip(streams, pairs) for index, part in enumerate(("density", "tone"))},
        "FLOAT",
    )
    return tuple((chosen[item_key(stream, "density")], chosen[item_key(stream, "tone")]) for stream in streams)


def emitted[R: Effect](shell: Shell[R], path: KeyPath, tones: tuple[Scalar, *tuple[Scalar, ...]]) -> BlenderResult[tuple[Expr, ...]]:
    """Returns scene-linear emitted light at each tone through the one spectrum menu of the emission at `path`.

    Args:
        shell: Working config, entry inputs, and retained spectrum cases.
        path: Emission record path.
        tones: Tone operands in output order.

    Returns:
        Light per tone, or the working luminance fault.
    """
    spectrum_field, intensity = Emission.__struct_fields__
    spectral = (*path, spectrum_field)
    start, end = Tint.__struct_fields__
    cool, hot = Incandescence.__struct_fields__
    (ramp,) = Gradient.__struct_fields__
    brightness = math("POWER", 2.0, shell.input(*path, intensity))

    def lit(working: NodeGroup) -> tuple[Expr, ...]:
        def colored(index: int, tone: Scalar) -> dict[str, Expr]:
            body = Expr(
                kind=bpy.types.ShaderNodeBlackbody,
                inputs=(("Temperature", mixed(tone, shell.input(*spectral, Incandescence, cool), shell.input(*spectral, Incandescence, hot))),),
                socket="Color",
                shape="RGBA",
            )
            gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", tone),), content=shell.case(Gradient, *spectral).map(lambda held: held.ramp), socket="Color", shape="RGBA")
            return {
                Tint.title: mixed(tone, shell.input(*spectral, Tint, start), shell.input(*spectral, Tint, end)),
                Incandescence.title: vector("DIVIDE", body, working.placed({COLOR_INPUT.key: body}, masks.LUMINANCE.key)),
                Gradient.title: gradient.named(item_key(*spectral, Gradient, ramp) if index == 0 else item_key(*path, index, ramp)),
            }

        colors = _selected(shell.input(*spectral), shell.items[spectral], {item_key(index): colored(index, tone) for index, tone in enumerate(tones)}, "RGBA")
        return tuple(vector("SCALE", colors[item_key(index)], brightness) for index in range(len(tones)))

    return masks.luminance(shell.config).map(lit)


# --- [CPU]


def evaluated(field: Basis, points: npt.NDArray[np.float64], time: npt.NDArray[np.float64], *, seed: int, stream: int) -> npt.NDArray[np.float64]:
    """Returns density and tone columns at greater-side frame points and evolution times, drawing the field the node path draws.

    Args:
        field: Texture and cut.
        points: `(n, 2)` greater-side frame positions.
        time: `(n,)` evolution coordinates.
        seed: Record seed.
        stream: Draw family from `textures.stream`.

    Returns:
        `(n, 2)` density and tone.
    """
    offset = textures.White(noise_dimensions="4D").evaluated(np.array(((seed, stream, 0.0),)), 0.0)["Color"][0] * 256.0
    planar = np.column_stack((points, np.zeros(len(points))))
    located = planar + offset
    match field:
        case Perlin(texture=noise, stretch=stretch):
            raw = noise.evaluated(*_axes(noise.rank, located / (1.0, stretch, 1.0), time))[noise.primary]
        case Voronoi(texture=cells):
            raw = cells.evaluated(*_axes(cells.rank, located, time))[cells.primary]
        case Wave(wavelength=wavelength, phase=phase, profile=profile) as wave:
            at = planar / (wavelength, wavelength, 1.0) + offset * (0.0, 0.0, 1.0)
            raw = np.empty(len(points), dtype=np.float32)
            for moment in np.unique(time):
                held = time == moment
                fac = msgspec.structs.replace(wave.texture, phase_offset=phase - tau * float(moment)).evaluated(at[held])["Fac"]
                raw[held] = np.floor(fac * 2.0) if profile == "SQUARE" else fac
        case Gabor(texture=oscillations):
            raw = oscillations.evaluated(*_axes(oscillations.rank, located, time))[oscillations.primary]
        case Magic(texture=turbulence):
            raw = turbulence.evaluated(*_axes(turbulence.rank, located, time))[turbulence.primary]
        case White(texture=noise, spacing=spacing):
            cells, w = _axes(noise.rank, located / (spacing, spacing, 1.0), time)
            raw = noise.evaluated(np.floor(cells), np.floor(w))[noise.primary]
    density = np.asarray(field.cut.evaluated(raw), dtype=np.float64)
    match field:
        case Voronoi(cell_color=True) as cellular:
            tint = cellular.colored.evaluated(*_axes(cellular.texture.rank, located, time))["Color"]
            return np.column_stack((density * tint[:, 1], tint[:, 0])).astype(np.float64)
        case _:
            return np.column_stack((density, density))


def spectrum(request: Spectrum, tones: npt.NDArray[np.float64], config: PyOpenColorIO.Config) -> BlenderResult[npt.NDArray[np.float64]]:
    """Evaluates scene-linear RGB of linear tints, host ramps, or unit-luminance blackbodies integrated against the observer at each tone."""
    match request:
        case Tint(start=start, end=end):
            return Ok((1.0 - tones[:, None]) * np.asarray(start) + tones[:, None] * np.asarray(end))
        case Gradient(ramp=ramp):
            return ramp.evaluated(tones.tolist()).map(lambda colors: colors[:, :3])
        case Incandescence(cool=cool, hot=hot):
            tristimulus = observer.blackbody((1.0 - tones) * cool + tones * hot)
            return processors.basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(lambda matrix: np.linalg.solve(matrix, tristimulus.T).T)


# --- [LOWERING]


def _raw(field: Basis, point: Expr, offset: Expr, time: Scalar, inlet: Inlet) -> Expr:
    """Texture value of one basis at a greater-side position decorrelated by a drawn offset."""
    match field:
        case Perlin(texture=noise):
            return _node(noise, (point + offset) / xyz(1.0, inlet("stretch"), 1.0), time, inlet)
        case Voronoi(texture=cells):
            return _node(cells, point + offset, time, inlet)
        case Wave(profile=profile) as wave:
            phase = math("MULTIPLY_ADD", time, -tau, inlet("phase"))
            read = _linked(inlet)
            at = vector("DIVIDE", point, xyz(inlet("wavelength"), inlet("wavelength"), 1.0)) + offset * (0.0, 0.0, 1.0)
            fac = wave.texture.node(at, linked=Some(lambda info: Some(phase) if info.name == "phase_offset" else Nothing if info.name == "scale" else read(info)))
            return math("FLOOR", fac * 2.0) if profile == "SQUARE" else fac
        case Gabor(texture=oscillations):
            return _node(oscillations, point + offset, time, inlet)
        case Magic(texture=turbulence):
            return _node(turbulence, point + offset, time, inlet)
        case White(texture=noise):
            return _node(noise, (point + offset) / xyz(inlet("spacing"), inlet("spacing"), 1.0), time, inlet, floored=True)


def _node(texture: textures.Texture, point: Expr, time: Scalar, inlet: Inlet, *, socket: Option[str] = Nothing, floored: bool = False) -> Expr:
    """Builds a texture over a position, time on Z at rank 3 and on W at rank 4, W reading X at rank 1, both coordinates floored to cells when `floored`."""
    match texture.rank:
        case 1:
            vector_axis, w = point, point["X"]
        case 3:
            vector_axis, w = point + xyz(z=time), time
        case _:
            vector_axis, w = point, time
    coordinates = (vector("FLOOR", vector_axis), math("FLOOR", w)) if floored else (vector_axis, w)
    return texture.node(*coordinates, socket=socket, linked=Some(_linked(inlet, "texture")))


def _linked(inlet: Inlet, *at: Key) -> textures.Linked:
    """Reads each texture or map range field at its group input under `at`."""
    return lambda info: Some(inlet(*at, info.name))


def _axes(rank: int, located: npt.NDArray[np.float64], time: npt.NDArray[np.float64]) -> tuple[npt.NDArray[np.float64], npt.NDArray[np.float64]]:
    """`Vector` and `W` coordinates the node path builds at a rank."""
    match rank:
        case 1:
            return located, located[:, 0]
        case 3:
            return located + np.column_stack((np.zeros((len(time), 2)), time)), time
        case _:
            return located, time


def _selected(selector: Expr, items: MenuItems, outputs: Mapping[Key, Mapping[str, Expr]], kind: DataType) -> dict[Key, Expr]:
    """Selects every output's case through one Menu Switch, the first output at its `Output`, the rest by Switch nodes on its item booleans."""
    first, *rest = outputs
    head = menu(selector, items, outputs[first], kind=kind)

    def chosen(cases: Mapping[str, Expr]) -> Expr:
        (_, initial), *others = cases.items()
        return functools.reduce(lambda held, case: switched(msgspec.structs.replace(head, socket=case[0], shape="FLOAT"), held, case[1]), others, initial)

    return {first: head} | {output: chosen(outputs[output]) for output in rest}


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Basis",
    "Emission",
    "Gabor",
    "Gradient",
    "Incandescence",
    "Magic",
    "Perlin",
    "Spectrum",
    "Tint",
    "Voronoi",
    "Wave",
    "WaveProfile",
    "White",
    "density_range",
    "emitted",
    "evaluated",
    "profiled",
    "spectrum",
    "textured",
]
