"""Procedural textures cut to density over greater frame sides and the light they emit: their menus and panels, their node forms, their CPU evaluation, and entry input decoding."""

from collections.abc import Callable, Mapping
import functools
import inspect
from itertools import starmap
from math import pi, tau
from types import MemberDescriptorType
import typing
from typing import Annotated, Final, Literal

import bpy
from expression import Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec
import msgspec.inspect
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.data.rna.descriptors import Value
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.nodes import textures
from blender.bpy.nodes.content import Ramp
from blender.bpy.nodes.evaluation import column, sampled
from blender.bpy.nodes.graphs import Expr, math, menu, mixed, Scalar, Shape, switched, vector, xyz
from blender.bpy.nodes.interface import Interface, InterfacePanel, item_key, MenuItem, MenuItems, Socket
from blender.bpy.render.compositor import masks
from blender.bpy.render.compositor.motion import origin
from blender.bpy.render.compositor.shells import Shell
from blender.imaging.color import colorimetry, processors
from blender.imaging.geometry import arithmetic
from blender.imaging.geometry.arithmetic import Elementwise, Namespace
from blender.records.faults import accumulated, Fault, Outcome, RecordReason
from blender.records.fields import Case, Finite, member, NonNegative, Positive, Unit

from ...chain.stages import Effect

# --- [TYPES] ----------------------------------------------------------------------------

type WaveProfile = arithmetic.WaveProfile | Literal["SQUARE"]
type Basis = Perlin | Voronoi | Wave | Gabor | Magic | White
type Spectrum = Tint | Incandescence | Gradient
type Inlet = Callable[..., Expr]

# --- [CONSTANTS] ------------------------------------------------------------------------

POSITION: Final = column("position", bpy.types.NodeSocketVector)
W: Final = column("w")

# --- [MODELS] ---------------------------------------------------------------------------

# --- [TEXTURES]


class Perlin(Case, frozen=True, kw_only=True):
    """Fractal Perlin noise over greater frame sides cut to a density."""

    texture: textures.Noise = textures.Noise(noise_dimensions="4D", scale=1.0)
    stretch: Positive = 1.0
    cut: textures.MapRange = textures.MapRange()


class Voronoi(Case, frozen=True, kw_only=True):
    """Cellular distances over greater frame sides cut to a density."""

    texture: textures.Voronoi = textures.Voronoi(voronoi_dimensions="4D", normalize=True, scale=10.0)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")
    cell_color: bool = False

    @property
    def feature_cells(self) -> textures.Voronoi:
        """Cells whose `Color` belongs to the nearest feature, F1 Euclidean cells where the feature measures edges."""
        return msgspec.structs.replace(self.texture, feature="F1", distance="EUCLIDEAN") if self.texture.feature == "DISTANCE_TO_EDGE" else self.texture


class Wave(Case, frozen=True, kw_only=True):
    """Bands or rings of a periodic profile over greater frame sides cut to a density."""

    wavelength: Positive = 0.1
    pattern: Literal["BANDS", "RINGS"] = "BANDS"
    profile: WaveProfile = "SIN"
    phase: Finite = 0.0
    distortion: Finite = 0.0
    detail: Annotated[float, msgspec.Meta(ge=0.0, le=15.0)] = 2.0
    detail_scale: NonNegative = 1.0
    detail_roughness: Unit = 0.5
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")

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

    texture: textures.Gabor = textures.Gabor(gabor_type="3D", scale=10.0)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


class Magic(Case, frozen=True, kw_only=True):
    """Nested trigonometric turbulence over greater frame sides cut to a density."""

    texture: textures.Magic = textures.Magic(scale=10.0)
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


class White(Case, frozen=True, kw_only=True):
    """Constant random values on lattice cells cut to a density."""

    texture: textures.White = textures.White(noise_dimensions="4D")
    spacing: Positive = 0.1
    cut: textures.MapRange = textures.MapRange(interpolation_type="SMOOTHSTEP")


# --- [LIGHT]


class Tint(Case, frozen=True, kw_only=True):
    """Color graded from start to end by field tone."""

    start: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)
    end: tuple[Finite, Finite, Finite] = (1.0, 1.0, 1.0)


class Incandescence(Case, frozen=True, kw_only=True):
    """Unit-luminance blackbody color graded from cool to hot temperature by field tone."""

    cool: Annotated[float, msgspec.Meta(ge=800.0, le=12000.0)] = 1000.0
    hot: Annotated[float, msgspec.Meta(ge=800.0, le=12000.0)] = 2000.0


class Gradient(Case, frozen=True, kw_only=True):
    """Color read from a color ramp at field tone."""

    ramp: Ramp = Ramp()


class Emission(msgspec.Struct, frozen=True, kw_only=True):
    """Light a generator adds over its coverage, colored by tone, its luminance at full density in stops."""

    spectrum: Spectrum = Incandescence()
    intensity: Annotated[float, msgspec.Meta(ge=-32.0, le=32.0)] = 2.0


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [INTERFACES]


def fields(prefix: Key, *sockets: Socket) -> tuple[Socket, ...]:
    """Sockets keyed under `prefix` by their own keys, each named by its own key's display name."""
    return tuple(msgspec.structs.replace(socket, key=item_key(prefix, socket.key), name=socket.title) for socket in sockets)


def clock(key: Key, speed: float = 24.0) -> InterfacePanel:
    """Clock panel at `key` holding the speed and frame hold of animated parameters, the speed defaulting to `speed`."""
    return InterfacePanel(
        key=key,
        name=bpy.path.display_name(key),
        description="Clock speed and frame hold of animated parameters",
        inputs=(
            Socket(key=item_key(key, "speed"), name="Speed", kind=bpy.types.NodeSocketFloat, description="Clock units per second, still at 0", min_value=0.0, default_value=speed),
            Socket(key=item_key(key, "hold"), name="Hold", kind=bpy.types.NodeSocketInt, description="Scene frames each clock value holds", min_value=1, default_value=1),
        ),
    )


def bases(key: Key, chosen: Basis, description: str, name: str = "Basis") -> InterfacePanel:
    """Basis panel at `key`: its case menu at `<key>_kind` defaulting to the chosen case, then one panel per case under `<key>_<case>` holding its texture inputs, its own sockets, and its density cut, each other case at its defaults."""

    def floats(prefix: Key, settings: textures.NodeSettings, title: str) -> tuple[Socket, ...]:
        return tuple(
            Socket(
                key=item_key(prefix, field),
                name=f"{title}{bpy.path.display_name(field)}",
                kind=bpy.types.NodeSocketVector if isinstance(value, tuple) else bpy.types.NodeSocketFloat,
                default_value=value,
            )
            for field, value in msgspec.structs.asdict(settings).items()
            if field not in settings.kind.bl_rna.properties
        )

    def panel(case: Basis) -> InterfacePanel:
        prefix = item_key(key, type(case))
        match case:
            case Perlin(stretch=stretch):
                own = fields(prefix, Socket(key="stretch", kind=bpy.types.NodeSocketFloat, description="Feature height over width", min_value=0.0, default_value=stretch))
            case Voronoi(cell_color=cell_color):
                own = fields(prefix, Socket(key="cell_color", kind=bpy.types.NodeSocketBool, description="Cell red sets tone, green scales density", default_value=cell_color))
            case Wave():
                own = fields(
                    prefix,
                    Socket(key="wavelength", kind=bpy.types.NodeSocketFloat, description="Band or ring spacing over greater frame side", min_value=0.0, default_value=case.wavelength),
                    Socket(key="phase", kind=bpy.types.NodeSocketFloatAngle, description="Profile phase offset", default_value=case.phase),
                    Socket(key="distortion", kind=bpy.types.NodeSocketFloat, description="Distortion noise phase offset in radians", default_value=case.distortion),
                    Socket(key="detail", kind=bpy.types.NodeSocketFloat, description="Distortion noise octaves past first", min_value=0.0, max_value=15.0, default_value=case.detail),
                    Socket(key="detail_scale", kind=bpy.types.NodeSocketFloat, description="Distortion noise frequency", min_value=0.0, default_value=case.detail_scale),
                    Socket(key="detail_roughness", kind=bpy.types.NodeSocketFloatFactor, description="Distortion octave amplitude over previous", default_value=case.detail_roughness),
                )
            case White(spacing=spacing):
                own = fields(prefix, Socket(key="spacing", kind=bpy.types.NodeSocketFloat, description="Cell width over greater frame side", min_value=0.0, default_value=spacing))
            case Gabor() | Magic():
                own = ()
        texture = () if isinstance(case, Wave) else floats(item_key(prefix, "texture"), case.texture, "")
        return InterfacePanel(
            key=prefix, name=bpy.path.display_name(item_key(type(case))), description=str(inspect.getdoc(type(case))), inputs=(*texture, *own, *floats(item_key(prefix, "cut"), case.cut, "Density "))
        )

    cases = tuple(chosen if isinstance(chosen, kind) else kind() for kind in typing.get_args(Basis.__value__))
    return InterfacePanel(
        key=key,
        name=name,
        description=description,
        inputs=(
            Socket(
                key=item_key(key, "kind"),
                name="Texture",
                kind=bpy.types.NodeSocketMenu,
                description="Texture case",
                default_value=bpy.path.display_name(item_key(type(chosen))),
                items=Some(_items(cases)),
            ),
        ),
        panels=tuple(map(panel, cases)),
    )


def incandescence(key: Key, case: Incandescence, description: str) -> InterfacePanel:
    """Blackbody panel at `key` holding the temperatures at tone 0 and 1."""
    return InterfacePanel(
        key=key,
        name=bpy.path.display_name(key),
        description=description,
        inputs=tuple(
            Socket(
                key=item_key(key, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketFloatColorTemperature, description=text, min_value=800.0, max_value=12000.0, default_value=kelvin
            )
            for field, text, kelvin in (("cool", "Temperature at tone 0", case.cool), ("hot", "Temperature at tone 1", case.hot))
        ),
    )


def spectra(key: Key, chosen: Spectrum, description: str, toggle: Option[bool] = Nothing) -> InterfacePanel:
    """Spectrum panel at `key`: its case menu at `<key>_kind` defaulting to the chosen case, then the tint and blackbody panels, the ramp held by the record."""
    cases = tuple(chosen if isinstance(chosen, kind) else kind() for kind in typing.get_args(Spectrum.__value__))
    tint = next(case for case in cases if isinstance(case, Tint))
    return InterfacePanel(
        key=key,
        name=bpy.path.display_name(key.rsplit("_", 1)[-1]),
        description=description,
        toggle=toggle,
        inputs=(
            Socket(
                key=item_key(key, "kind"),
                name="Spectrum",
                kind=bpy.types.NodeSocketMenu,
                description="Spectrum case",
                default_value=bpy.path.display_name(item_key(type(chosen))),
                items=Some(_items(cases)),
            ),
        ),
        panels=(
            InterfacePanel(
                key=item_key(key, Tint),
                name="Tint",
                description=str(inspect.getdoc(Tint)),
                inputs=tuple(
                    Socket(key=item_key(key, Tint, field), name=bpy.path.display_name(field), kind=bpy.types.NodeSocketColor, description=text, default_value=(*color, 1.0))
                    for field, text, color in (("start", "Scene-linear color at tone 0", tint.start), ("end", "Scene-linear color at tone 1", tint.end))
                ),
            ),
            incandescence(item_key(key, Incandescence), next(case for case in cases if isinstance(case, Incandescence)), str(inspect.getdoc(Incandescence))),
        ),
    )


def emission(key: Key, light: Emission, description: str, toggle: Option[bool] = Nothing) -> InterfacePanel:
    """Emission panel at `key`: luminance in stops, then the spectrum panel defaulting to the light's case."""
    return InterfacePanel(
        key=key,
        name=bpy.path.display_name(key.rsplit("_", 1)[-1]),
        description=description,
        toggle=toggle,
        inputs=(
            Socket(
                key=item_key(key, "intensity"),
                name="Intensity",
                kind=bpy.types.NodeSocketFloat,
                description="Luminance at full density in stops",
                min_value=-32.0,
                max_value=32.0,
                default_value=light.intensity,
            ),
        ),
        panels=(spectra(item_key(key, "spectrum"), light.spectrum, "Color emitted at each tone"),),
    )


# --- [PROFILES]


def profiled[V: Elementwise, B](profile: WaveProfile, cycles: V, ns: Namespace[V, B]) -> V:
    """Returns the periodic profile in [0, 1] at a cycle coordinate, the square profile floored from the saw."""
    match profile:
        case "SQUARE":
            return ns.floor(arithmetic.periodic("SAW", cycles, ns) * 2.0)
        case _:
            return arithmetic.periodic(profile, cycles, ns)


def rank(texture: textures.Texture) -> int:
    """Coordinate axes the texture reads by its dimensions member, Wave and Magic reading three."""
    match texture:
        case textures.Noise(noise_dimensions=held) | textures.White(noise_dimensions=held) | textures.Voronoi(voronoi_dimensions=held) | textures.Gabor(gabor_type=held):
            return int(held[0])
        case _:
            return 3


def density_range(field: Basis) -> tuple[float, float]:
    """Bounds the cut density, smooth-cell color corrections reaching below zero."""
    match field:
        case Voronoi(cell_color=True, texture=textures.Voronoi(feature="SMOOTH_F1") as cells):
            span = cells.smoothness / 2.0
            return -(5 ** rank(cells) - 1) * span / (4.0 * (1.0 + 3.0 * span)), 1.0
        case _:
            return 0.0, 1.0


# --- [NODES]


def textured[R: Effect](
    shell: Shell[R], key: Key, chosen: Basis, point: Expr, time: Scalar, seed: Scalar, *, loss: Scalar = 0.0, streams: tuple[int, *tuple[int, ...]] = (0,)
) -> tuple[tuple[Expr, Expr], ...]:
    """Returns density and tone per stream of the basis menu at `key`, every stream switched by the one menu.

    Args:
        shell: Entry inputs.
        key: Basis menu key.
        chosen: Record basis holding the node members of its case, every other case at its defaults.
        point: Greater-side frame position.
        time: Signed evolution coordinate.
        seed: Record seed operand.
        loss: Spatial loss subtracted before the cut.
        streams: Draw families in output order, from `tiles.stream`.

    Returns:
        Density and tone per stream.
    """
    face = _declared(bases(key, chosen, ""))

    def read(*path: Key | type[Case]) -> Expr:
        return shell.input(face[item_key(*path)])

    def linked(inlet: Inlet, at: tuple[Key, ...], field: MemberDescriptorType) -> Option[Scalar]:
        return Some(inlet(*at, field.__name__))

    def node(texture: textures.Texture, at: Expr, inlet: Inlet, *, socket: str | int = 0, shape: Shape = bpy.types.NodeSocketFloat, floored: bool = False) -> Expr:
        w: Scalar
        match rank(texture):
            case 1:
                axis, w = at, at["X"]
            case 3:
                axis, w = at + xyz(z=time), time
            case _:
                axis, w = at, time
        coordinates = (vector("FLOOR", axis), math("FLOOR", w)) if floored else (axis, w)
        return texture.node(*coordinates, socket=socket, shape=shape, linked=Some(functools.partial(linked, inlet, ("texture",))))

    def raw(field: Basis, offset: Expr, inlet: Inlet) -> Expr:
        match field:
            case Perlin(texture=noise):
                return node(noise, (point + offset) / xyz(1.0, inlet("stretch"), 1.0), inlet)
            case Voronoi(texture=cells):
                return node(cells, point + offset, inlet)
            case Wave(profile=profile) as wave:
                phase = math("MULTIPLY_ADD", time, -tau, inlet("phase"))
                at = vector("DIVIDE", point, xyz(inlet("wavelength"), inlet("wavelength"), 1.0)) + offset * (0.0, 0.0, 1.0)
                fac = wave.texture.node(
                    at,
                    linked=Some(
                        lambda field: (
                            Some(phase)
                            if field is member(textures.Wave, lambda kind: kind.phase_offset)
                            else Nothing
                            if field is member(textures.Wave, lambda kind: kind.scale)
                            else linked(inlet, (), field)
                        )
                    ),
                )
                return math("FLOOR", fac * 2.0) if profile == "SQUARE" else fac
            case Gabor(texture=oscillations):
                return node(oscillations, point + offset, inlet)
            case Magic(texture=turbulence):
                return node(turbulence, point + offset, inlet)
            case White(texture=noise):
                return node(noise, (point + offset) / xyz(inlet("spacing"), inlet("spacing"), 1.0), inlet, floored=True)

    def drawn(field: Basis, stream: int) -> tuple[Expr, Expr]:
        inlet = functools.partial(read, key, type(field))
        offset = origin(seed, float(stream))
        density = field.cut.node(raw(field, offset, inlet) - loss, linked=Some(functools.partial(linked, inlet, ("cut",))))
        match field:
            case Voronoi():
                color = node(field.feature_cells, point + offset, inlet, socket="Color", shape=bpy.types.NodeSocketColor)
                return switched(inlet("cell_color"), density, density * color["Green"]), switched(inlet("cell_color"), density, color["Red"])
            case _:
                return density, density

    cases = tuple(chosen if isinstance(chosen, kind) else kind() for kind in typing.get_args(Basis.__value__))
    items = _items(cases).items
    pairs = tuple(tuple(drawn(case, stream) for case in cases) for stream in streams)
    selected = _selected(
        read(key, "kind"),
        {item_key(stream, part): dict(zip(items, (pair[index] for pair in held), strict=True)) for stream, held in zip(streams, pairs) for index, part in enumerate(("density", "tone"))},
    )
    return tuple((selected[item_key(stream, "density")], selected[item_key(stream, "tone")]) for stream in streams)


def emitted[R: Effect](shell: Shell[R], key: Key, light: Emission, tones: tuple[Scalar, *tuple[Scalar, ...]]) -> Outcome[tuple[Expr, ...]]:
    """Returns scene-linear emitted light at each tone through the one spectrum menu of the emission panel at `key`.

    Args:
        shell: Working config and entry inputs.
        key: Emission panel key.
        light: Record emission holding the ramp of its gradient case.
        tones: Tone operands in output order.

    Returns:
        Light per tone, or the working luminance fault.
    """
    face = _declared(emission(key, light, ""))

    def read(*path: Key | type[Case]) -> Expr:
        return shell.input(face[item_key(*path)])

    spectral = item_key(key, "spectrum")
    ramp = light.spectrum.ramp if isinstance(light.spectrum, Gradient) else Gradient().ramp
    brightness = math("POWER", 2.0, read(key, "intensity"))
    spectra = tuple(light.spectrum if isinstance(light.spectrum, kind) else kind() for kind in typing.get_args(Spectrum.__value__))

    def tinted(index: int, tone: Scalar) -> Outcome[dict[MenuItem, Expr]]:
        body = Expr(
            kind=bpy.types.ShaderNodeBlackbody,
            inputs=(("Temperature", mixed(tone, read(spectral, Incandescence, "cool"), read(spectral, Incandescence, "hot"))),),
            socket="Color",
            shape=bpy.types.NodeSocketColor,
        )
        gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", tone),), content=Some(ramp), socket="Color", shape=bpy.types.NodeSocketColor)
        return masks.luminance(shell.config, body).map(
            lambda level: dict(
                zip(
                    _items(spectra).items,
                    (
                        mixed(tone, read(spectral, Tint, "start"), read(spectral, Tint, "end")),
                        vector("DIVIDE", body, level),
                        gradient.named(item_key(spectral, Gradient, "ramp") if index == 0 else item_key(key, index, "ramp")),
                    ),
                    strict=True,
                )
            )
        )

    def lit(cases: tuple[dict[MenuItem, Expr], ...]) -> tuple[Expr, ...]:
        colors = _selected(read(spectral, "kind"), {item_key(index): case for index, case in enumerate(cases)})
        return tuple(vector("SCALE", colors[item_key(index)], brightness) for index in range(len(tones)))

    return accumulated(starmap(tinted, enumerate(tones))).map(lit)


# --- [CPU]


def decoded[T: msgspec.Struct](record: T, values: Mapping[Key, Value], prefix: Key = "") -> Outcome[T]:
    """Decodes the record its entry inputs override, each field at its item key under `prefix`.

    Nested records sit under their field key, an optional value holds while its panel toggle is on with a plain value at `<key>_value`, a case union takes the case its `<key>_kind` menu names, an enum the member its menu item names, and RGBA colors cut to RGB.

    Args:
        record: Stored record whose fields no input holds.
        values: Entry input values by interface item key.
        prefix: Key the record's fields sit under.

    Returns:
        Record of the input values, or the refused value fault.
    """

    def raw(info: msgspec.inspect.Type, held: object, key: Key) -> object:
        match info, values.get(key):
            case msgspec.inspect.StructType(cls=kind, fields=fields), _:
                base = held if isinstance(held, dict) else msgspec.to_builtins(kind())
                return {**base, **{field.encode_name: raw(field.type, base.get(field.encode_name), item_key(key, field.name) if key else field.name) for field in fields}}
            case msgspec.inspect.UnionType(types=types), bool(on) if any(isinstance(option, msgspec.inspect.NoneType) for option in types):
                present = tuple(option for option in types if not isinstance(option, msgspec.inspect.NoneType))
                inner = present[0] if len(present) == 1 else msgspec.inspect.UnionType(types=present)
                return (raw(inner, held, key) if isinstance(inner, msgspec.inspect.StructType | msgspec.inspect.UnionType) else raw(inner, held, item_key(key, "value"))) if on else None
            case msgspec.inspect.UnionType(types=types), _ if isinstance(name := values.get(item_key(key, "kind")), str):
                cases = {kind: option for option in types if isinstance(option, msgspec.inspect.StructType) and issubclass(kind := option.cls, Case)}
                match next((kind for kind in cases if bpy.path.display_name(item_key(kind)) == name), None):
                    case None:
                        return held
                    case kind:
                        return raw(cases[kind], held if isinstance(held, dict) and held.get("kind") == kind.__name__ else msgspec.to_builtins(kind()), item_key(key, kind))
            case msgspec.inspect.EnumType(cls=kind), str(name):
                return next((item.value for item in kind if bpy.path.display_name(item.name.lower()) == name), held)
            case msgspec.inspect.TupleType(item_types=items), tuple(found) if len(items) == 3 and len(found) == 4:
                return found[:3]
            case _, None:
                return held
            case _, found:
                return found

    return catch(msgspec.convert, exception=msgspec.ValidationError)(raw(msgspec.inspect.type_info(type(record)), msgspec.to_builtins(record), prefix), type(record)).map_error(
        functools.partial(Fault.of, RecordReason.UNREADABLE, type(record).__name__)
    )


def evaluated(identity: Identity, field: Basis, points: npt.NDArray[np.float64], time: npt.NDArray[np.float64], *, seed: int, stream: int) -> npt.NDArray[np.float64]:
    """Returns density and tone columns at greater-side frame points and evolution times, the stock texture nodes evaluating the field the node path draws in one pass.

    Args:
        identity: Extension owning the scratch evaluation tree.
        field: Texture and cut.
        points: `(n, 2)` greater-side frame positions.
        time: `(n,)` evolution coordinates.
        seed: Record seed.
        stream: Draw family from `tiles.stream`.

    Returns:
        `(n, 2)` density and tone.
    """
    offset = origin(float(seed), float(stream))
    located = POSITION + offset

    def coordinates(texture: textures.Texture, at: Expr) -> tuple[Expr, Scalar]:
        match rank(texture):
            case 1:
                return at, at["X"]
            case 3:
                return at + xyz(z=W), W
            case _:
                return at, W

    raw: Expr
    match field:
        case Perlin(texture=noise, stretch=stretch):
            raw = noise.node(*coordinates(noise, located / (1.0, stretch, 1.0)))
        case Voronoi(texture=cells):
            raw = cells.node(*coordinates(cells, located))
        case Wave(wavelength=wavelength, phase=phase, profile=profile) as wave:
            at = POSITION / (wavelength, wavelength, 1.0) + offset * (0.0, 0.0, 1.0)
            fac = wave.texture.node(at, linked=Some(lambda field: Some(math("MULTIPLY_ADD", W, -tau, phase)) if field is member(textures.Wave, lambda kind: kind.phase_offset) else Nothing))
            raw = math("FLOOR", fac * 2.0) if profile == "SQUARE" else fac
        case Gabor(texture=oscillations):
            raw = oscillations.node(*coordinates(oscillations, located))
        case Magic(texture=turbulence):
            raw = turbulence.node(*coordinates(turbulence, located))
        case White(texture=noise, spacing=spacing):
            lattice, w = coordinates(noise, located / (spacing, spacing, 1.0))
            raw = noise.node(vector("FLOOR", lattice), math("FLOOR", w))
    density = field.cut.node(raw)
    columns = {"position": np.column_stack((points, np.zeros(len(points)))), "w": time}
    match field:
        case Voronoi(cell_color=True) as cellular:
            cut, tint = evaluated_nodes(identity, columns, density, cellular.feature_cells.node(*coordinates(cellular.texture, located), socket="Color", shape=bpy.types.NodeSocketColor))
            return np.column_stack((cut * tint[:, 1], tint[:, 0])).astype(np.float64)
        case _:
            (cut,) = evaluated_nodes(identity, columns, density)
            return np.column_stack((cut, cut)).astype(np.float64)


def evaluated_nodes(identity: Identity, columns: Mapping[str, npt.ArrayLike], *roots: Expr) -> tuple[npt.NDArray[np.float32], ...]:
    """Values of stock-node roots over named per-point columns, a host refusal raised as the declaration defect it is."""
    match sampled(identity, columns, *roots):
        case Result(tag="ok", ok=values):
            return values
        case Result(error=faults):
            raise RuntimeError(*(fault.message for fault in faults))


def spectrum(identity: Identity, request: Spectrum, tones: npt.NDArray[np.float64], config: PyOpenColorIO.Config) -> Outcome[npt.NDArray[np.float64]]:
    """Evaluates scene-linear RGB of linear tints, host ramps, or unit-luminance blackbodies integrated against the observer at each tone."""
    match request:
        case Tint(start=start, end=end):
            return Ok((1.0 - tones[:, None]) * np.asarray(start) + tones[:, None] * np.asarray(end))
        case Gradient(ramp=ramp):
            gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", W),), content=Some(ramp), socket="Color", shape=bpy.types.NodeSocketColor)
            return sampled(identity, {"w": tones}, gradient).map(lambda values: values[0][:, :3].astype(np.float64))
        case Incandescence(cool=cool, hot=hot):
            tristimulus = colorimetry.blackbody((1.0 - tones) * cool + tones * hot)
            return processors.basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR).map(lambda matrix: np.asarray(np.linalg.solve(matrix, tristimulus.T).T, dtype=np.float64))


# --- [LOWERING]


def _declared(face: Interface) -> dict[Key, Socket | InterfacePanel]:
    """Sockets and panels a declared interface holds by item key, the declarations reads go through."""
    return {key: item if isinstance(item, InterfacePanel) else item[0] for key, item, _, _ in face.walked()}


def _items(cases: tuple[Case, ...]) -> MenuItems:
    """Menu items of union cases, each named by its key and described by its docstring."""
    return MenuItems(items=tuple(MenuItem(name=bpy.path.display_name(item_key(type(case))), description=str(inspect.getdoc(type(case)))) for case in cases))


def _selected(selector: Expr, outputs: Mapping[Key, Mapping[MenuItem, Expr]]) -> dict[Key, Expr]:
    """Selects every output's case through one Menu Switch, the first output at its `Output`, the rest by Switch nodes on its item booleans."""
    first, *rest = outputs
    head = menu(selector, outputs[first])

    def chosen(cases: Mapping[MenuItem, Expr]) -> Expr:
        (_, initial), *others = cases.items()
        return functools.reduce(lambda held, case: switched(msgspec.structs.replace(head, socket=case[0].name, shape=bpy.types.NodeSocketFloat), held, case[1]), others, initial)

    return {first: head} | {output: chosen(outputs[output]) for output in rest}


# --- [COMPOSITION] ----------------------------------------------------------------------

CLOCK: Final = clock("clock")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "CLOCK",
    "POSITION",
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
    "W",
    "Wave",
    "WaveProfile",
    "White",
    "bases",
    "clock",
    "decoded",
    "density_range",
    "emission",
    "emitted",
    "evaluated",
    "evaluated_nodes",
    "fields",
    "incandescence",
    "profiled",
    "rank",
    "spectra",
    "spectrum",
    "textured",
]
