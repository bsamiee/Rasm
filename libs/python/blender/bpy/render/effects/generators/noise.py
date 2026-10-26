"""Procedural fields, generated light, and Gaussian distributions."""

from collections.abc import Mapping
import functools
from math import exp, pi, tau

import bpy
from expression import Some
import msgspec
import msgspec.inspect as native
import numpy as np

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.graphs import DataType, Expr, math, menu, mixed, NodeGroup, Scalar, switched, vector, xyz
from blender.bpy.nodes.interface import item_key, MenuItems
from blender.bpy.render.color.spaces import COLOR_INPUT
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.coordinates import clocked, origin
from blender.bpy.render.effects.records import Field, KeyPath
from blender.bpy.render.effects.rows import Alpha, CompositorEffect, precise, Shell
from blender.imaging.chain import Effect
from blender.imaging.effects.generators.noise import (
    Basis,
    Cut,
    Emission,
    Envelope,
    Extinction,
    Feature,
    Fractal,
    Gabor,
    Gradient,
    Incandescence,
    Magic,
    Metric,
    NoiseField,
    Octaves,
    Perlin,
    Profile,
    Tint,
    Voronoi,
    Vortex,
    Wave,
    White,
)
from blender.imaging.parameters import Gated, Motion

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [TEXTURES]


def periodic(profile: Profile, cycles: Expr) -> Expr:
    """Returns selected unit profile at physical cycle coordinate."""
    match profile:
        case Profile.SIN:
            return math("MULTIPLY_ADD", math("COSINE", cycles * tau), -0.5, 0.5)
        case Profile.SAW:
            return math("FRACT", cycles)
        case Profile.TRI:
            return abs(cycles - math("FLOOR", cycles + 0.5)) * 2.0
        case Profile.SQUARE:
            return math("FLOOR", math("FRACT", cycles) * 2.0)


def sample(basis: Basis, point: Scalar, time: Scalar, seed: Scalar, stream: Scalar, inputs: Mapping[KeyPath, Scalar] = {}) -> Expr:
    """Returns raw scalar texture with input overrides, a Voronoi read at the texture node's `Distance`.

    Args:
        basis: Selected texture settings and defaults.
        point: Spatial sample coordinate, X on W for one-dimensional samples.
        time: Evolution coordinate, on W in four dimensions and Z in three.
        seed: Decorrelation origin seed.
        stream: Decorrelation stream as literal or linked index.
        inputs: Record member paths with values replacing defaults.

    Returns:
        Scalar texture expression.
    """
    dimensions = basis.dimensions if isinstance(basis, Perlin | Voronoi | White | Gabor) else 3
    detail = tuple((field.name.title(), field.name) for field in msgspec.structs.fields(Octaves))

    def textured(position: Expr, fourth: Scalar) -> Expr:
        common = (*((("Vector", position),) if dimensions > 1 else ()), *((("W", fourth),) if dimensions in {1, 4} else ()), ("Scale", 1.0))
        match basis:
            case Perlin(octaves=octaves, fractal=fractal):
                _, _, _, _, octaves_field, offset, gain, distortion, _ = Perlin.__struct_fields__
                return Expr(
                    kind=bpy.types.ShaderNodeTexNoise,
                    inputs=(
                        *common,
                        *((socket, inputs.get((octaves_field, name), getattr(octaves, name))) for socket, name in detail),
                        *((("Offset", inputs.get((offset,), basis.offset)),) if fractal in {Fractal.RIDGED_MULTIFRACTAL, Fractal.HYBRID_MULTIFRACTAL, Fractal.HETERO_TERRAIN} else ()),
                        *((("Gain", inputs.get((gain,), basis.gain)),) if fractal in {Fractal.RIDGED_MULTIFRACTAL, Fractal.HYBRID_MULTIFRACTAL} else ()),
                        ("Distortion", inputs.get((distortion,), basis.distortion)),
                    ),
                    members=(("noise_dimensions", f"{dimensions}D"), ("noise_type", fractal.name), ("normalize", True)),
                    socket="Fac",
                )
            case Voronoi(octaves=octaves):
                _, _, _, _, exponent, smoothness, randomness, octaves_field, _, _ = Voronoi.__struct_fields__
                minkowski = basis.metric is Metric.MINKOWSKI and basis.feature is not Feature.DISTANCE_TO_EDGE and dimensions > 1
                return Expr(
                    kind=bpy.types.ShaderNodeTexVoronoi,
                    inputs=(
                        *common,
                        *((socket, inputs.get((octaves_field, name), getattr(octaves, name))) for socket, name in detail),
                        *((("Exponent", inputs.get((exponent,), basis.exponent)),) if minkowski else ()),
                        *((("Smoothness", inputs.get((smoothness,), basis.smoothness)),) if basis.feature is Feature.SMOOTH_F1 else ()),
                        ("Randomness", inputs.get((randomness,), basis.randomness)),
                    ),
                    members=(("voronoi_dimensions", f"{dimensions}D"), ("feature", basis.feature.name), ("distance", basis.metric.name), ("normalize", True)),
                    socket="Distance",
                )
            case White():
                cells = (*((("Vector", vector("FLOOR", position)),) if dimensions > 1 else ()), *((("W", math("FLOOR", fourth)),) if dimensions in {1, 4} else ()))
                return Expr(kind=bpy.types.ShaderNodeTexWhiteNoise, inputs=cells, members=(("noise_dimensions", f"{dimensions}D"),))
            case Magic():
                _, _, distortion, _ = Magic.__struct_fields__
                return Expr(
                    kind=bpy.types.ShaderNodeTexMagic,
                    inputs=(("Vector", position), ("Scale", 1.0), ("Distortion", inputs.get((distortion,), basis.distortion))),
                    members=(("turbulence_depth", basis.depth),),
                    socket="Fac",
                )
            case Gabor():
                _, _, frequency, anisotropy, orientation, _ = Gabor.__struct_fields__
                angle = inputs.get((orientation,), basis.orientation)
                return Expr(
                    kind=bpy.types.ShaderNodeTexGabor,
                    inputs=(
                        ("Vector", position),
                        ("Scale", 1.0),
                        ("Frequency", inputs.get((frequency,), basis.frequency)),
                        ("Anisotropy", inputs.get((anisotropy,), basis.anisotropy)),
                        (f"Orientation {dimensions}D", angle if dimensions == 2 else xyz(math("COSINE", angle), math("SINE", angle))),
                    ),
                    members=(("gabor_type", f"{dimensions}D"),),
                )
            case Wave():
                _, _, _, phase, distortion, detail_field, detail_scale, detail_roughness, _ = Wave.__struct_fields__
                wave = Expr(
                    kind=bpy.types.ShaderNodeTexWave,
                    inputs=(
                        ("Vector", position),
                        ("Scale", pi / 10.0),
                        ("Phase Offset", math("MULTIPLY_ADD", time, -tau, inputs.get((phase,), basis.phase))),
                        ("Distortion", inputs.get((distortion,), basis.distortion)),
                        ("Detail", inputs.get((detail_field,), basis.detail)),
                        ("Detail Scale", inputs.get((detail_scale,), basis.detail_scale)),
                        ("Detail Roughness", inputs.get((detail_roughness,), basis.detail_roughness)),
                    ),
                    members=(
                        ("wave_type", basis.pattern.name),
                        ("wave_profile", Profile.SAW.name if basis.profile is Profile.SQUARE else basis.profile.name),
                        ("bands_direction", "X"),
                        ("rings_direction", "Z"),
                    ),
                    socket="Fac",
                )
                return math("FLOOR", wave * 2.0) if basis.profile is Profile.SQUARE else wave

    match basis:
        case Perlin():
            _, size, stretch, *_ = Perlin.__struct_fields__
            extent = inputs.get((size,), basis.size)
            scale: Scalar = xyz(extent, math("MULTIPLY", extent, inputs.get((stretch,), basis.stretch)), extent)
        case Voronoi() | White():
            _, spacing, *_ = type(basis).__struct_fields__
            scale = inputs.get((spacing,), basis.spacing)
        case Wave():
            wavelength, *_ = Wave.__struct_fields__
            scale = inputs.get((wavelength,), basis.wavelength)
        case Gabor():
            _, size, *_ = Gabor.__struct_fields__
            scale = inputs.get((size,), basis.size)
        case Magic():
            size, *_ = Magic.__struct_fields__
            scale = inputs.get((size,), basis.size)
    offset = origin(seed, stream)
    located = vector("DIVIDE", point, (scale,) * 3 if isinstance(scale, int | float) else scale) + (offset * (0.0, 0.0, 1.0) if isinstance(basis, Wave) else offset)
    match dimensions:
        case 1:
            return textured(located, located["X"])
        case 3 if not isinstance(basis, Wave):
            return textured(located + xyz(z=time), time)
        case _:
            return textured(located, time)


def sampled[R: Effect](shell: Shell[R], path: KeyPath, point: Scalar, time: Scalar, seed: Scalar, loss: Scalar = 0.0, streams: tuple[int, *tuple[int, ...]] = (0,)) -> tuple[tuple[Expr, Expr], ...]:
    """Returns selected cut density and tone streams from one Basis menu.

    Args:
        shell: Entry inputs and retained texture settings.
        path: Basis union path in record.
        point: Displayed-frame sample position.
        time: Signed evolution coordinate.
        seed: Caller-owned field seed.
        loss: Spatial loss subtracted before selected cut.
        streams: Caller-owned decorrelation streams in output order.

    Returns:
        Density and tone per stream, every stream switched by the one Basis menu.
    """
    threshold, softness, invert, _ = Cut.__struct_fields__

    def cut(basis: Basis, stream: int) -> tuple[Expr, Expr]:
        kind = type(basis)
        fields = Field.of(kind, prefix=(*path, kind))
        values: dict[KeyPath, Scalar] = {field.path[len(path) + 1 :]: shell.input(*field.path) for field in fields}
        mapping = next(field.path for field in fields if isinstance(field.shape, native.StructType) and field.shape.cls is Cut)
        raw = sample(basis, point, time, seed, stream, values)
        flipped = shell.input(*mapping, invert)
        density = Expr(
            kind=bpy.types.ShaderNodeMapRange,
            inputs=(("Value", raw - loss - shell.input(*mapping, threshold)), ("From Min", 0.0), ("From Max", shell.input(*mapping, softness)), ("To Min", flipped), ("To Max", 1.0 - flipped)),
            members=(("interpolation_type", basis.cut.interpolation.name), ("clamp", True)),
            socket="Result",
        )
        match basis:
            case Voronoi(feature=Feature.DISTANCE_TO_EDGE):
                color = msgspec.structs.replace(sample(msgspec.structs.replace(basis, feature=Feature.F1, metric=Metric.EUCLIDEAN), point, time, seed, stream, values), socket="Color", shape="RGBA")
            case Voronoi():
                color = msgspec.structs.replace(raw, socket="Color", shape="RGBA")
            case _:
                return density, density
        *_, cell_color = Voronoi.__struct_fields__
        gate = shell.input(*path, kind, cell_color)
        return switched(gate, density, density * color["Green"]), switched(gate, density, color["Red"])

    bases = tuple(basis for basis in shell.cases[path] if isinstance(basis, Perlin | Voronoi | Wave | Gabor | Magic | White))
    fields = tuple(tuple(cut(basis, stream) for basis in bases) for stream in streams)
    titles = tuple(type(basis).title for basis in bases)
    chosen = _selected(
        shell.input(*path),
        shell.items[path],
        {item_key(stream, component): dict(zip(titles, (pair[index] for pair in samples))) for stream, samples in zip(streams, fields) for index, component in enumerate(("density", "tone"))},
        "FLOAT",
    )
    return tuple((chosen[item_key(stream, "density")], chosen[item_key(stream, "tone")]) for stream in streams)


def gaussian(radius: Scalar, angle: Scalar) -> tuple[Expr, Expr]:
    """Builds cosine and sine Box-Muller pair from distinct scalar uniform draws, callers supplying full compositor precision.

    Args:
        radius: Caller-owned White Noise Value draw for radial sampling.
        angle: Distinct caller-owned White Noise Value draw for angular sampling.

    Returns:
        Cosine and sine normals over one shared radius and phase.
    """
    epsilon = float(np.finfo(np.float32).eps)
    spread = math("SQRT", math("LOGARITHM", math("DIVIDE", math("ADD", radius, epsilon), 1.0 + 2.0 * epsilon), exp(-0.5)))
    phase = math("MULTIPLY", angle, tau)
    return spread * math("COSINE", phase), spread * math("SINE", phase)


# --- [LIGHT]


def emitted[R: Effect](shell: Shell[R], path: KeyPath, tones: tuple[Scalar, *tuple[Scalar, ...]]) -> BlenderResult[tuple[Expr, ...]]:
    """Returns scene-linear emitted light at each tone with one spectrum selector.

    Args:
        shell: Working config, entry inputs, and retained spectrum cases.
        path: Emission record path from effect root.
        tones: Tone operands in output order.

    Returns:
        Light per tone, or the working luminance fault.
    """
    key = item_key(*path)
    spectrum_field, intensity = Emission.__struct_fields__
    spectrum = (*path, spectrum_field)
    start, end = Tint.__struct_fields__
    cool, hot = Incandescence.__struct_fields__
    (ramp,) = Gradient.__struct_fields__
    brightness = math("POWER", 2.0, shell.input(*path, intensity))

    def lit(working: NodeGroup) -> tuple[Expr, ...]:
        def colored(index: int, tone: Scalar) -> dict[str, Expr]:
            body = Expr(
                kind=bpy.types.ShaderNodeBlackbody,
                inputs=(("Temperature", mixed(tone, shell.input(*spectrum, Incandescence, cool), shell.input(*spectrum, Incandescence, hot))),),
                socket="Color",
                shape="RGBA",
            )
            gradient = Expr(kind=bpy.types.ShaderNodeValToRGB, inputs=(("Fac", tone),), content=shell.case(Gradient, *spectrum).map(lambda held: held.ramp), socket="Color", shape="RGBA")
            return {
                Tint.title: mixed(tone, shell.input(*spectrum, Tint, start), shell.input(*spectrum, Tint, end)),
                Incandescence.title: vector("DIVIDE", body, working.placed({COLOR_INPUT.key: body}, masks.LUMINANCE.key)),
                Gradient.title: gradient.named(item_key(*spectrum, Gradient, ramp) if index == 0 else item_key(key, index, "gradient")),
            }

        colors = _selected(shell.input(*spectrum), shell.items[spectrum], {item_key(index): colored(index, tone) for index, tone in enumerate(tones)}, "RGBA")
        return tuple(vector("SCALE", colors[item_key(index)], brightness) for index in range(len(tones)))

    return masks.luminance(shell.config).map(lit)


def noised(shell: Shell[NoiseField]) -> BlenderResult[Expr]:
    """Returns emission and extinction coverage over field coordinates."""
    basis, emission, extinction, motion, drift, evolution, vortex, envelope, center, angle, seed = NoiseField.__struct_fields__
    _, value = Gated.__struct_fields__
    speed, hold = Motion.__struct_fields__
    radius_field, twirl, arms, spin, tunnel = Vortex.__struct_fields__
    width, height = Envelope.__struct_fields__
    depth, color = Extinction.__struct_fields__
    inlet, at = shell.input, shell.space
    clock = clocked(inlet(motion, speed), inlet(motion, hold)).named("clock")
    axis = Expr(
        kind=bpy.types.ShaderNodeVectorRotate,
        inputs=(("Vector", at.point - inlet(center) * at.extent), ("Angle", inlet(angle))),
        members=(("rotation_type", "Z_AXIS"), ("invert", True)),
        socket="Vector",
        shape="VECTOR",
    )
    radius = inlet(vortex, value, radius_field)
    rho = vector("LENGTH", axis) / radius
    phase = math("MULTIPLY_ADD", math("ARCTAN2", axis["Y"], axis["X"]), inlet(vortex, value, arms), math("MULTIPLY_ADD", 1.0 - rho, inlet(vortex, value, twirl), clock * inlet(vortex, value, spin)))
    cone = vector("SCALE", xyz(math("COSINE", phase), math("SINE", phase), inlet(vortex, value, tunnel)), math("MINIMUM", rho, 1.0) * radius)
    reach = math("MAXIMUM", 1.0 - vector("LENGTH", axis / xyz(inlet(envelope, value, width), inlet(envelope, value, height))), 0.0)
    ((density, tone),) = sampled(
        shell,
        (basis,),
        switched(shell.gate(vortex), axis, cone) - vector("SCALE", inlet(drift), clock),
        clock * inlet(evolution),
        inlet(seed),
        switched(shell.gate(envelope), 0.0, 1.0 - reach * reach),
    )
    coverage = 1.0 - switched(shell.gate(extinction), 1.0, math("POWER", exp(-1.0), density * inlet(extinction, value, depth)))

    def generated(lights: tuple[Expr, ...]) -> Expr:
        light = switched(shell.gate(emission), (0.0, 0.0, 0.0), vector("SCALE", lights[0], density)) + vector("SCALE", inlet(extinction, value, color), coverage)
        return Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", light), ("Alpha", coverage), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")

    return emitted(shell, (emission, value), (tone,)).map(generated)


def _selected(selector: Expr, items: MenuItems, outputs: Mapping[Key, Mapping[str, Expr]], kind: DataType) -> dict[Key, Expr]:
    """Selects every output's case through one Menu Switch: the first output at its `Output`, the rest by Switch nodes on its item booleans."""
    first, *rest = outputs
    head = menu(selector, items, outputs[first], kind=kind)

    def chosen(cases: Mapping[str, Expr]) -> Expr:
        (_, initial), *others = cases.items()
        return functools.reduce(lambda held, case: switched(msgspec.structs.replace(head, socket=case[0], shape="FLOAT"), held, case[1]), others, initial)

    return {first: head} | {output: chosen(outputs[output]) for output in rest}


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field = CompositorEffect(record=NoiseField, graph=noised, alpha=Alpha.RESULT, requested=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["emitted", "gaussian", "noise_field", "noised", "periodic", "sample", "sampled"]
