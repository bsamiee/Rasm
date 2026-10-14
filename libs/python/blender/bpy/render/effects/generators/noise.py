"""Native procedural fields, generated light, and Gaussian distributions."""

from collections.abc import Mapping
from itertools import starmap
from math import exp, pi, tau

import bpy
from expression import Ok, Some
import msgspec
import numpy as np

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace
from blender.bpy.nodes.graphs import Clock, End, ended, Fragment, math, mixed, nested, node, Operand, origin, selected, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.compositing import masks
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, KeyPath, precise, rows, Shell
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
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated, Motion

# --- [OPERATIONS] -----------------------------------------------------------------------


def periodic(key: Key, profile: Profile, cycles: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Returns the selected unit profile at a physical cycle coordinate."""
    match profile:
        case Profile.SIN:
            return math(f"{key}_phase", "MULTIPLY", cycles, tau).bind(lambda phase: math(f"{key}_cosine", "COSINE", phase)).bind(lambda cosine: math(key, "MULTIPLY_ADD", cosine, -0.5, 0.5))
        case Profile.SAW:
            return math(key, "FRACT", cycles)
        case Profile.TRI:
            return (
                math(f"{key}_shift", "ADD", cycles, 0.5)
                .bind(lambda shifted: math(f"{key}_floor", "FLOOR", shifted))
                .bind(lambda floor: math(f"{key}_offset", "SUBTRACT", cycles, floor))
                .bind(lambda offset: math(f"{key}_fold", "ABSOLUTE", offset))
                .bind(lambda folded: math(key, "MULTIPLY", folded, 2.0))
            )
        case Profile.SQUARE:
            return math(f"{key}_fraction", "FRACT", cycles).bind(lambda fraction: math(f"{key}_double", "MULTIPLY", fraction, 2.0)).bind(lambda doubled: math(key, "FLOOR", doubled))


def sample(key: Key, basis: Basis, point: Operand, time: Operand, seed: Operand, stream: Operand, inputs: Mapping[KeyPath, Operand]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns one native raw scalar texture with caller-owned input overrides.

    Args:
        key: Prefix of the sample's nodes.
        basis: Native settings and defaults of the selected texture.
        point: Spatial sample coordinate; a one-dimensional sample reads X on W.
        time: Evolution coordinate, on W in four dimensions and Z in three.
        seed: Seed of the decorrelation origin.
        stream: Caller-owned decorrelation stream, a literal or a linked index.
        inputs: Native record-member paths whose values replace declared defaults.

    Returns:
        The scalar texture and its complete fragment, or native graph faults.
    """
    texture = f"{key}_texture"
    dimensions = basis.dimensions if isinstance(basis, Perlin | Voronoi | White | Gabor) else 3

    def coordinates[T](position: T, fourth: T) -> dict[str, T]:
        return {**({"Vector": position} if dimensions > 1 else {}), **({"W": fourth} if dimensions in {1, 4} else {})}

    def textured(position: tuple[Fragment, End], fourth: Operand) -> BlenderResult[tuple[Fragment, End]]:
        common = {**coordinates(position, fourth), "Scale": 1.0}
        match basis:
            case Perlin():
                _, _, _, _, octaves, offset, gain, distortion, _ = Perlin.__struct_fields__
                return node(
                    texture,
                    bpy.types.ShaderNodeTexNoise,
                    {
                        **common,
                        **{field.name.title(): inputs.get((octaves, field.name), getattr(basis.octaves, field.name)) for field in msgspec.structs.fields(Octaves)},
                        **({"Offset": inputs.get((offset,), basis.offset)} if basis.fractal in {Fractal.RIDGED_MULTIFRACTAL, Fractal.HYBRID_MULTIFRACTAL, Fractal.HETERO_TERRAIN} else {}),
                        **({"Gain": inputs.get((gain,), basis.gain)} if basis.fractal in {Fractal.RIDGED_MULTIFRACTAL, Fractal.HYBRID_MULTIFRACTAL} else {}),
                        "Distortion": inputs.get((distortion,), basis.distortion),
                    },
                    {"noise_dimensions": f"{basis.dimensions}D", "noise_type": basis.fractal.name, "normalize": True},
                ).map(ended(texture, "Fac"))
            case Voronoi():
                _, _, _, _, exponent, smoothness, randomness, octaves, _, _ = Voronoi.__struct_fields__
                minkowski = basis.metric is Metric.MINKOWSKI and basis.feature is not Feature.DISTANCE_TO_EDGE and dimensions > 1
                return node(
                    texture,
                    bpy.types.ShaderNodeTexVoronoi,
                    {
                        **common,
                        **{field.name.title(): inputs.get((octaves, field.name), getattr(basis.octaves, field.name)) for field in msgspec.structs.fields(Octaves)},
                        **({"Exponent": inputs.get((exponent,), basis.exponent)} if minkowski else {}),
                        **({"Smoothness": inputs.get((smoothness,), basis.smoothness)} if basis.feature is Feature.SMOOTH_F1 else {}),
                        "Randomness": inputs.get((randomness,), basis.randomness),
                    },
                    {"voronoi_dimensions": f"{basis.dimensions}D", "feature": basis.feature.name, "distance": basis.metric.name, "normalize": True},
                ).map(ended(texture, "Distance"))
            case White():
                cells = coordinates(vector(f"{key}_cells", "FLOOR", position), math(f"{key}_fourth_cell", "FLOOR", fourth))
                return (
                    accumulated(cells.values())
                    .bind(lambda floored: node(texture, bpy.types.ShaderNodeTexWhiteNoise, dict(zip(cells, floored)), {"noise_dimensions": f"{basis.dimensions}D"}))
                    .map(ended(texture, "Value"))
                )
            case Magic():
                _, _, distortion, _ = Magic.__struct_fields__
                return node(
                    texture, bpy.types.ShaderNodeTexMagic, {"Vector": position, "Scale": 1.0, "Distortion": inputs.get((distortion,), basis.distortion)}, {"turbulence_depth": basis.depth}
                ).map(ended(texture, "Fac"))
            case Gabor():
                _, _, frequency, anisotropy, orientation_field, _ = Gabor.__struct_fields__
                angle = inputs.get((orientation_field,), basis.orientation)
                orientation: BlenderResult[Operand] = (
                    Ok(angle)
                    if basis.dimensions == 2
                    else paired(math(f"{key}_cosine", "COSINE", angle), math(f"{key}_sine", "SINE", angle))
                    .bind(lambda axes: node(f"{key}_orientation", bpy.types.ShaderNodeCombineXYZ, {"X": axes[0], "Y": axes[1], "Z": 0.0}, {}))
                    .map(ended(f"{key}_orientation", "Vector"))
                )
                return orientation.bind(
                    lambda direction: node(
                        texture,
                        bpy.types.ShaderNodeTexGabor,
                        {
                            "Vector": position,
                            "Scale": 1.0,
                            "Frequency": inputs.get((frequency,), basis.frequency),
                            "Anisotropy": inputs.get((anisotropy,), basis.anisotropy),
                            f"Orientation {basis.dimensions}D": direction,
                        },
                        {"gabor_type": f"{basis.dimensions}D"},
                    )
                ).map(ended(texture, "Value"))
            case Wave():
                _, _, _, phase_field, distortion, detail, detail_scale, detail_roughness, _ = Wave.__struct_fields__
                return (
                    math(f"{key}_evolution", "MULTIPLY_ADD", time, -tau, inputs.get((phase_field,), basis.phase))
                    .bind(
                        lambda phase: node(
                            texture,
                            bpy.types.ShaderNodeTexWave,
                            {
                                "Vector": position,
                                "Scale": pi / 10.0,
                                "Phase Offset": phase,
                                "Distortion": inputs.get((distortion,), basis.distortion),
                                "Detail": inputs.get((detail,), basis.detail),
                                "Detail Scale": inputs.get((detail_scale,), basis.detail_scale),
                                "Detail Roughness": inputs.get((detail_roughness,), basis.detail_roughness),
                            },
                            {
                                "wave_type": basis.pattern.name,
                                "wave_profile": Profile.SAW.name if basis.profile is Profile.SQUARE else basis.profile.name,
                                "bands_direction": "X",
                                "rings_direction": "Z",
                            },
                        )
                    )
                    .map(ended(texture, "Fac"))
                    .bind(lambda wave: math(f"{key}_square_double", "MULTIPLY", wave, 2.0).bind(lambda doubled: math(key, "FLOOR", doubled)) if basis.profile is Profile.SQUARE else Ok(wave))
                )

    def shifted(position: tuple[Fragment, End], offset: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        if isinstance(basis, Wave):
            return vector(f"{key}_seed_axis", "MULTIPLY", offset, (0.0, 0.0, 1.0)).bind(lambda axis: vector(f"{key}_position", "ADD", position, axis)).bind(lambda located: textured(located, time))
        shifted = vector(f"{key}_position", "ADD", position, offset)
        if dimensions == 1:
            return shifted.bind(lambda located: node(f"{key}_axis", bpy.types.ShaderNodeSeparateXYZ, {"Vector": located}, {}).bind(lambda axis: textured(located, ended(f"{key}_axis", "X")(axis))))
        if dimensions == 3:
            return (
                paired(shifted, node(f"{key}_evolution_axis", bpy.types.ShaderNodeCombineXYZ, {"Z": time}, {}).map(ended(f"{key}_evolution_axis", "Vector")))
                .bind(lambda terms: vector(f"{key}_evolved", "ADD", *terms))
                .bind(lambda evolved: textured(evolved, time))
            )
        return shifted.bind(lambda located: textured(located, time))

    scale: BlenderResult[Operand]
    match basis:
        case Perlin():
            _, size_field, stretch, _, _, _, _, _, _ = Perlin.__struct_fields__
            size = inputs.get((size_field,), basis.size)
            scale = (
                math(f"{key}_stretched", "MULTIPLY", size, inputs.get((stretch,), basis.stretch))
                .bind(lambda stretched: node(f"{key}_scale", bpy.types.ShaderNodeCombineXYZ, {"X": size, "Y": stretched, "Z": size}, {}))
                .map(ended(f"{key}_scale", "Vector"))
            )
        case Voronoi():
            _, spacing, _, _, _, _, _, _, _, _ = Voronoi.__struct_fields__
            scale = Ok(inputs.get((spacing,), basis.spacing))
        case White():
            _, spacing, _ = White.__struct_fields__
            scale = Ok(inputs.get((spacing,), basis.spacing))
        case Wave():
            wavelength, _, _, _, _, _, _, _, _ = Wave.__struct_fields__
            scale = Ok(inputs.get((wavelength,), basis.wavelength))
        case Gabor():
            _, size_field, _, _, _, _ = Gabor.__struct_fields__
            scale = Ok(inputs.get((size_field,), basis.size))
        case Magic():
            size_field, _, _, _ = Magic.__struct_fields__
            scale = Ok(inputs.get((size_field,), basis.size))
    return paired(scale.bind(lambda scale: vector(f"{key}_scaled", "DIVIDE", point, (scale,) * 3 if isinstance(scale, float | int) else scale)), origin(f"{key}_origin", seed, stream)).bind(
        lambda parts: shifted(*parts)
    )


def sampled[R: Effect](
    shell: Shell[R], path: KeyPath, point: Operand, time: Operand, seed: Operand, loss: Operand = 0.0, streams: tuple[int, *tuple[int, ...]] = (0,)
) -> BlenderResult[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]]:
    """Returns selected cut density and tone streams from one Basis menu.

    Args:
        shell: Native effect inputs and retained texture settings.
        path: Basis union's path in the record.
        point: Displayed-frame sample position.
        time: Signed evolution coordinate.
        seed: Caller-owned field seed.
        loss: Spatial loss subtracted before the selected cut.
        streams: Caller-owned decorrelation streams in output order.

    Returns:
        Density and tone pairs per stream, sharing the whole graph, or native faults.
    """
    key = item_key(*path)

    def cut(basis: Basis, stream: int) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
        kind = type(basis)
        prefix = f"{key}_{stream}_{item_key(kind)}"
        values = {at[len(path) + 1 :]: shell.socket(*at) for at, _, _ in rows(kind, prefix=(*path, kind))}
        raw = sample(prefix, basis, point, time, seed, stream, values)

        def mapped(cut_field: Key) -> BlenderResult[tuple[Fragment, End]]:
            threshold, softness, invert_field, _ = Cut.__struct_fields__
            density = raw.bind(lambda raw: math(f"{prefix}_loss", "SUBTRACT", raw, loss)).bind(
                lambda reduced: math(f"{prefix}_threshold", "SUBTRACT", reduced, shell.socket(*path, kind, cut_field, threshold))
            )
            invert = shell.socket(*path, kind, cut_field, invert_field)
            return (
                paired(density, math(f"{prefix}_high", "SUBTRACT", 1.0, invert))
                .bind(
                    lambda ends: node(
                        f"{prefix}_cut",
                        bpy.types.ShaderNodeMapRange,
                        {"Value": ends[0], "From Min": 0.0, "From Max": shell.socket(*path, kind, cut_field, softness), "To Min": invert, "To Max": ends[1]},
                        {"interpolation_type": basis.cut.interpolation.name, "clamp": True},
                    )
                )
                .map(ended(f"{prefix}_cut", "Result"))
            )

        match basis:
            case Voronoi():
                _, _, _, _, _, _, _, _, cut_field, cell_color_field = Voronoi.__struct_fields__
                density = mapped(cut_field)
                color = (
                    sample(f"{prefix}_color", msgspec.structs.replace(basis, feature=Feature.F1, metric=Metric.EUCLIDEAN), point, time, seed, stream, values).map(
                        lambda raw: (raw[0], End(node=f"{prefix}_color_texture", socket="Color"))
                    )
                    if basis.feature is Feature.DISTANCE_TO_EDGE
                    else raw.map(lambda raw: (raw[0], End(node=f"{prefix}_texture", socket="Color")))
                )
                channels = color.bind(lambda color: node(f"{prefix}_channels", bpy.types.CompositorNodeSeparateColor, {"Image": color}, {"mode": "RGB"}))
                cell_color = shell.socket(*path, kind, cell_color_field)
                weighted = paired(density, channels).bind(lambda parts: math(f"{prefix}_brightness", "MULTIPLY", parts[0], ended(f"{prefix}_channels", "Green")(parts[1])))
                return paired(paired(density, channels), weighted).bind(
                    lambda parts: paired(
                        switched(f"{prefix}_density", "FLOAT", cell_color, parts[0][0], parts[1]),
                        switched(f"{prefix}_tone", "FLOAT", cell_color, parts[0][0], ended(f"{prefix}_channels", "Red")(parts[0][1])),
                    )
                )
            case Perlin():
                _, _, _, _, _, _, _, _, cut_field = Perlin.__struct_fields__
            case Wave():
                _, _, _, _, _, _, _, _, cut_field = Wave.__struct_fields__
            case Gabor():
                _, _, _, _, _, cut_field = Gabor.__struct_fields__
            case Magic():
                _, _, _, cut_field = Magic.__struct_fields__
            case White():
                _, _, cut_field = White.__struct_fields__
        return mapped(cut_field).map(lambda density: (density, density))

    bases = tuple(basis for basis in shell.cases[path] if isinstance(basis, Perlin | Voronoi | Wave | Gabor | Magic | White))

    def chosen(fields: tuple[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...], ...]) -> BlenderResult[tuple[tuple[tuple[Fragment, End], tuple[Fragment, End]], ...]]:
        return selected(
            f"{key}_select",
            shell.socket(*path),
            shell.items[path],
            {
                item_key(str(stream), component): ("FLOAT", {type(basis).title: field[index] for basis, field in zip(bases, samples)})
                for stream, samples in zip(streams, fields)
                for index, component in enumerate(("density", "tone"))
            },
        ).map(lambda outputs: tuple((outputs[item_key(label, "density")], outputs[item_key(label, "tone")]) for stream in streams for label in (str(stream),)))

    return accumulated(accumulated(cut(basis, stream) for basis in bases) for stream in streams).bind(chosen)


def gaussian(key: str, radius: Operand, angle: Operand) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
    """Builds a cosine/sine Box-Muller pair from distinct scalar uniform draws.

    Args:
        key: Prefix of the distribution's native node keys.
        radius: Caller-owned White Noise Value draw for radial sampling.
        angle: Distinct caller-owned White Noise Value draw for angular sampling.

    Returns:
        Both normal ends with the complete shared fragment, or native graph faults.
        The caller supplies full compositor precision.
    """
    epsilon = float(np.finfo(np.float32).eps)
    return (
        paired(
            math(f"{key}_open_shift", "ADD", radius, epsilon)
            .bind(lambda shifted: math(f"{key}_open_radius", "DIVIDE", shifted, 1.0 + 2.0 * epsilon))
            .bind(lambda uniform: math(f"{key}_logarithm", "LOGARITHM", uniform, exp(-0.5)))
            .bind(lambda logarithm: math(f"{key}_radius", "SQRT", logarithm)),
            math(f"{key}_phase", "MULTIPLY", angle, tau),
        )
        .bind(
            lambda polar: paired(
                math(f"{key}_cosine", "COSINE", polar[1]).bind(lambda cosine: math(f"{key}_normal_cosine", "MULTIPLY", polar[0], cosine)),
                math(f"{key}_sine", "SINE", polar[1]).bind(lambda sine: math(f"{key}_normal_sine", "MULTIPLY", polar[0], sine)),
            )
        )
        .bind(lambda normals: Fragment.merged((normals[0][0], normals[1][0])).map(lambda joined: (joined, normals[0][1], normals[1][1])))
        .map(lambda built: ((built[0], built[1]), (built[0], built[2])))
    )


def emitted[R: Effect](shell: Shell[R], path: KeyPath, tones: tuple[Operand, *tuple[Operand, ...]]) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
    """Returns scene-linear emitted light at each tone with one spectrum selector.

    Args:
        shell: Working config, native inputs, and retained spectrum cases.
        path: Emission record's path from the effect root.
        tones: Tone operands in their required output order.

    Returns:
        Light pairs sharing the call's complete graph, or native graph faults.
    """
    key = item_key(*path)
    spectrum_field, intensity = Emission.__struct_fields__
    spectrum = (*path, spectrum_field)
    brightness = math(f"{key}_brightness", "POWER", 2.0, shell.socket(*path, intensity))
    working = masks.luminance(shell.config)

    def colored(index: int, tone: Operand) -> BlenderResult[dict[str, tuple[Fragment, End]]]:
        start, end = Tint.__struct_fields__
        cool, hot = Incandescence.__struct_fields__
        (ramp_field,) = Gradient.__struct_fields__
        prefix = f"{key}_{index}"
        tint = mixed(f"{prefix}_tint", "RGBA", tone, shell.socket(*spectrum, Tint, start), shell.socket(*spectrum, Tint, end))
        body = (
            mixed(f"{prefix}_temperature", "FLOAT", tone, shell.socket(*spectrum, Incandescence, cool), shell.socket(*spectrum, Incandescence, hot))
            .bind(lambda temperature: node(f"{prefix}_blackbody", bpy.types.ShaderNodeBlackbody, {"Temperature": temperature}, {}))
            .map(ended(f"{prefix}_blackbody", "Color"))
        )
        thermal = paired(body, working).bind(
            lambda parts: nested(f"{prefix}_luminance", parts[1], {"image": parts[0]}, "luminance").bind(lambda luminance: vector(f"{prefix}_unit", "DIVIDE", parts[0], luminance))
        )
        ramp = item_key(*spectrum, Gradient, ramp_field) if index == 0 else f"{prefix}_gradient"
        gradient = node(ramp, bpy.types.ShaderNodeValToRGB, {"Fac": tone}, {}, shell.case(Gradient, *spectrum).map(lambda gradient: gradient.ramp)).map(ended(ramp, "Color"))
        return accumulated(light.map(lambda light: (title, light)) for title, light in {Tint.title: tint, Incandescence.title: thermal, Gradient.title: gradient}.items()).map(dict)

    def brightened(parts: tuple[tuple[tuple[Fragment, End], ...], tuple[Fragment, End]]) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
        colors, gain = parts
        return accumulated(vector(f"{key}_{index}", "SCALE", color, gain) for index, color in enumerate(colors)).bind(
            lambda lights: Fragment.merged(fragment for fragment, _ in lights).map(lambda shared: tuple((shared, end) for _, end in lights))
        )

    colors = (
        accumulated(starmap(colored, enumerate(tones)))
        .bind(lambda cases: selected(f"{key}_spectrum", shell.socket(*spectrum), shell.items[spectrum], {item_key(str(index)): ("RGBA", colors) for index, colors in enumerate(cases)}))
        .map(lambda outputs: tuple(outputs[item_key(str(index))] for index in range(len(tones))))
    )
    return paired(colors, brightness).bind(brightened)


def noised(shell: Shell[NoiseField]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns generated emission and extinctive coverage over the field's coordinates."""
    inlet = shell.socket
    basis, emission_field, extinction_field, motion, drift, evolution, vortex_field, envelope_field, center, angle, seed = NoiseField.__struct_fields__
    _, value = Gated.__struct_fields__
    speed, hold = Motion.__struct_fields__
    clock = Clock(key="clock", speed=inlet(motion, speed), hold=inlet(motion, hold)).value

    def lit(fields: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        density_field, color = Extinction.__struct_fields__
        density, tone = fields
        emission = (
            emitted(shell, (emission_field, value), (tone,))
            .bind(lambda lights: vector("radiance", "SCALE", lights[0], density))
            .bind(lambda radiance: switched("emission", "VECTOR", inlet(emission_field), (0.0, 0.0, 0.0), radiance))
        )
        extinction = (extinction_field, value)
        transmission = (
            math("optical_depth", "MULTIPLY", density, inlet(*extinction, density_field))
            .bind(lambda depth: math("absorbed", "POWER", exp(-1.0), depth))
            .bind(lambda transmittance: switched("transmittance", "FLOAT", inlet(extinction_field), 1.0, transmittance))
        )
        coverage = transmission.bind(lambda transmission: math("coverage", "SUBTRACT", 1.0, transmission))
        scatter = coverage.bind(lambda cover: vector("scatter", "SCALE", inlet(*extinction, color), cover))
        return (
            paired(paired(emission, scatter).bind(lambda parts: vector("light", "ADD", *parts)), coverage)
            .bind(lambda parts: node("generated", bpy.types.CompositorNodeSetAlpha, {"Image": parts[0], "Alpha": parts[1], "Type": "Replace Alpha"}, {}))
            .map(ended("generated", "Image"))
        )

    def built(at: FrameSpace, clock: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        axis = (
            vector("anchor", "MULTIPLY", inlet(center), at.extent)
            .bind(lambda anchor: vector("offset", "SUBTRACT", at.point, anchor))
            .bind(lambda offset: node("axes", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": inlet(angle)}, {"rotation_type": "Z_AXIS", "invert": True}))
            .map(ended("axes", "Vector"))
        )
        radius_field, twirl, arms, spin, tunnel = Vortex.__struct_fields__
        width, height = Envelope.__struct_fields__
        vortex = (vortex_field, value)
        envelope = (envelope_field, value)
        radius = inlet(*vortex, radius_field)
        rho = axis.bind(lambda axes: vector("distance", "LENGTH", axes)).bind(lambda distance: math("rho", "DIVIDE", distance, radius))
        theta = axis.bind(lambda axes: node("polar", bpy.types.ShaderNodeSeparateXYZ, {"Vector": axes}, {})).bind(
            lambda polar: math("theta", "ARCTAN2", ended("polar", "Y")(polar), End(node="polar", socket="X"))
        )
        turn = paired(rho.bind(lambda rho: math("remaining", "SUBTRACT", 1.0, rho)), math("spin", "MULTIPLY", clock, inlet(*vortex, spin))).bind(
            lambda parts: math("turn", "MULTIPLY_ADD", parts[0], inlet(*vortex, twirl), parts[1])
        )
        phase = paired(theta, turn).bind(lambda parts: math("phase", "MULTIPLY_ADD", parts[0], inlet(*vortex, arms), parts[1]))
        circle = (
            phase
            .bind(lambda phase: paired(math("cosine", "COSINE", phase), math("sine", "SINE", phase)))
            .bind(lambda trigonometry: node("ring", bpy.types.ShaderNodeCombineXYZ, {"X": trigonometry[0], "Y": trigonometry[1], "Z": inlet(*vortex, tunnel)}, {}))
            .map(ended("ring", "Vector"))
        )
        span = rho.bind(lambda rho: math("bounded_radius", "MINIMUM", rho, 1.0)).bind(lambda bounded: math("span", "MULTIPLY", bounded, radius))
        mapped = paired(axis, paired(circle, span).bind(lambda parts: vector("cone", "SCALE", *parts))).bind(lambda parts: switched("mapped", "VECTOR", inlet(vortex_field), *parts))
        flow = vector("flow", "SCALE", inlet(drift), clock)
        point = paired(mapped, flow).bind(lambda parts: vector("sample", "SUBTRACT", *parts))
        extent = node("envelope_extent", bpy.types.ShaderNodeCombineXYZ, {"X": inlet(*envelope, width), "Y": inlet(*envelope, height)}, {}).map(ended("envelope_extent", "Vector"))
        loss = (
            paired(axis, extent)
            .bind(lambda parts: vector("envelope_position", "DIVIDE", *parts))
            .bind(lambda position: vector("envelope_distance", "LENGTH", position))
            .bind(lambda distance: math("envelope_edge", "SUBTRACT", 1.0, distance))
            .bind(lambda edge: math("envelope_reach", "MAXIMUM", edge, 0.0))
            .bind(lambda reach: math("envelope_squared", "MULTIPLY", reach, reach))
            .bind(lambda square: math("envelope_loss", "SUBTRACT", 1.0, square))
            .bind(lambda loss: switched("loss", "FLOAT", inlet(envelope_field), 0.0, loss))
        )
        time = math("evolution", "MULTIPLY", clock, inlet(evolution))
        return paired(paired(point, time), loss).bind(lambda parts: sampled(shell, (basis,), *parts[0], inlet(seed), parts[1])).bind(lambda fields: lit(fields[0]))

    return paired(framed("frame", shell.source, shell.aspect, shell.scale), clock).bind(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field = CompositorEffect(record=NoiseField, graph=noised, coverage=Coverage.RESULT, requested=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["emitted", "gaussian", "noise_field", "noised", "periodic", "sample", "sampled"]
