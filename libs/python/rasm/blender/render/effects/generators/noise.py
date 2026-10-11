# mypy: disable-error-code="attr-defined,misc,union-attr"
# ty: ignore[invalid-attribute-access,unresolved-attribute]
"""Native procedural fields, generated light, and Gaussian distributions."""

from collections.abc import Mapping
from functools import reduce
from itertools import starmap
from math import exp, pi, tau
from operator import or_

import bpy
from expression import Ok, Some
import msgspec
import numpy as np

from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import Clock, End, ended, Fragment, framed, FrameSpace, item_key, math, mixed, nested, node, Operand, origin, selected, switched, vector
from rasm.blender.render.effects.compositing import masks
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, KeyPath, precise, rows, Shell
from rasm.imaging.chain import Effect
from rasm.imaging.effects.generators.noise import (
    Basis,
    Cut,
    Emission,
    Envelope,
    Extinction,
    Feature,
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
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Gated, Motion

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


def sample(key: Key, basis: Basis, point: Operand, time: Operand, seed: Operand, stream: int, inputs: Mapping[KeyPath, Operand]) -> BlenderResult[tuple[Fragment, End]]:
    """Returns one native raw scalar texture with caller-owned input overrides.

    Args:
        key: Prefix of the sample's nodes.
        basis: Native settings and defaults of the selected texture.
        point: Spatial sample coordinate; a one-dimensional sample reads X on W.
        time: Evolution coordinate, on W in four dimensions and Z in three.
        seed: Seed of the decorrelation origin.
        stream: Caller-owned decorrelation stream.
        inputs: Native record-member paths whose values replace declared defaults.

    Returns:
        The scalar texture and its complete fragment, or native graph faults.
    """
    texture = f"{key}_texture"

    def textured(position: tuple[Fragment, End], fourth: Operand) -> BlenderResult[tuple[Fragment, End]]:
        common = {"Vector": position, "W": fourth, "Scale": 1.0}
        match basis:
            case Perlin():
                octaves = {field.name.title(): inputs.get((Perlin.octaves.__name__, field.name), getattr(basis.octaves, field.name)) for field in msgspec.structs.fields(Octaves)}
                return node(
                    texture,
                    bpy.types.ShaderNodeTexNoise,
                    {
                        **common,
                        **octaves,
                        "Offset": inputs.get((Perlin.offset.__name__,), basis.offset),
                        "Gain": inputs.get((Perlin.gain.__name__,), basis.gain),
                        "Distortion": inputs.get((Perlin.distortion.__name__,), basis.distortion),
                    },
                    {"noise_dimensions": f"{basis.dimensions}D", "noise_type": basis.fractal.name, "normalize": True},
                ).map(ended(texture, "Fac"))
            case Voronoi():
                octaves = {field.name.title(): inputs.get((Voronoi.octaves.__name__, field.name), getattr(basis.octaves, field.name)) for field in msgspec.structs.fields(Octaves)}
                return node(
                    texture,
                    bpy.types.ShaderNodeTexVoronoi,
                    {
                        **common,
                        **octaves,
                        "Exponent": inputs.get((Voronoi.exponent.__name__,), basis.exponent),
                        "Smoothness": inputs.get((Voronoi.smoothness.__name__,), basis.smoothness),
                        "Randomness": inputs.get((Voronoi.randomness.__name__,), basis.randomness),
                    },
                    {"voronoi_dimensions": f"{basis.dimensions}D", "feature": basis.feature.name, "distance": basis.metric.name, "normalize": True},
                ).map(ended(texture, "Distance"))
            case White():
                return (
                    paired(vector(f"{key}_cells", "FLOOR", position), math(f"{key}_fourth_cell", "FLOOR", fourth))
                    .bind(lambda cells: node(texture, bpy.types.ShaderNodeTexWhiteNoise, {"Vector": cells[0], "W": cells[1]}, {"noise_dimensions": f"{basis.dimensions}D"}))
                    .map(ended(texture, "Value"))
                )
            case Magic():
                return node(
                    texture,
                    bpy.types.ShaderNodeTexMagic,
                    {"Vector": position, "Scale": 1.0, "Distortion": inputs.get((Magic.distortion.__name__,), basis.distortion)},
                    {"turbulence_depth": basis.depth},
                ).map(ended(texture, "Fac"))
            case Gabor():
                angle = inputs.get((Gabor.orientation.__name__,), basis.orientation)
                orientation = (
                    paired(math(f"{key}_cosine", "COSINE", angle), math(f"{key}_sine", "SINE", angle))
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
                            "Frequency": inputs.get((Gabor.frequency.__name__,), basis.frequency),
                            "Anisotropy": inputs.get((Gabor.anisotropy.__name__,), basis.anisotropy),
                            "Orientation 2D": angle,
                            "Orientation 3D": direction,
                        },
                        {"gabor_type": f"{basis.dimensions}D"},
                    )
                ).map(ended(texture, "Value"))
            case Wave():
                return (
                    math(f"{key}_evolution", "MULTIPLY_ADD", time, -tau, inputs.get((Wave.phase.__name__,), basis.phase))
                    .bind(
                        lambda phase: node(
                            texture,
                            bpy.types.ShaderNodeTexWave,
                            {
                                "Vector": position,
                                "Scale": pi / 10.0,
                                "Phase Offset": phase,
                                "Distortion": inputs.get((Wave.distortion.__name__,), basis.distortion),
                                "Detail": inputs.get((Wave.detail.__name__,), basis.detail),
                                "Detail Scale": inputs.get((Wave.detail_scale.__name__,), basis.detail_scale),
                                "Detail Roughness": inputs.get((Wave.detail_roughness.__name__,), basis.detail_roughness),
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
        dimensions = basis.dimensions if isinstance(basis, Perlin | Voronoi | White | Gabor) else 3
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
            size = inputs.get((Perlin.size.__name__,), basis.size)
            scale = (
                math(f"{key}_stretched", "MULTIPLY", size, inputs.get((Perlin.stretch.__name__,), basis.stretch))
                .bind(lambda stretched: node(f"{key}_scale", bpy.types.ShaderNodeCombineXYZ, {"X": size, "Y": stretched, "Z": size}, {}))
                .map(ended(f"{key}_scale", "Vector"))
            )
        case Voronoi() | White():
            spacing = Voronoi.spacing.__name__ if isinstance(basis, Voronoi) else White.spacing.__name__
            scale = Ok(inputs.get((spacing,), basis.spacing))
        case Wave():
            scale = Ok(inputs.get((Wave.wavelength.__name__,), basis.wavelength))
        case Gabor() | Magic():
            size = Gabor.size.__name__ if isinstance(basis, Gabor) else Magic.size.__name__
            scale = Ok(inputs.get((size,), basis.size))
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
        density = raw.bind(lambda raw: math(f"{prefix}_loss", "SUBTRACT", raw, loss)).bind(
            lambda reduced: math(f"{prefix}_threshold", "SUBTRACT", reduced, shell.socket(*path, kind, kind.cut.__name__, Cut.threshold.__name__))
        )
        invert = shell.socket(*path, kind, kind.cut.__name__, Cut.invert.__name__)
        high = math(f"{prefix}_high", "SUBTRACT", 1.0, invert)
        mapped = (
            paired(density, high)
            .bind(
                lambda ends: node(
                    f"{prefix}_cut",
                    bpy.types.ShaderNodeMapRange,
                    {"Value": ends[0], "From Min": 0.0, "From Max": shell.socket(*path, kind, kind.cut.__name__, Cut.softness.__name__), "To Min": invert, "To Max": ends[1]},
                    {"interpolation_type": basis.cut.interpolation.name, "clamp": True},
                )
            )
            .map(ended(f"{prefix}_cut", "Result"))
        )
        if not isinstance(basis, Voronoi):
            return mapped.map(lambda density: (density, density))
        color = (
            sample(f"{prefix}_color", msgspec.structs.replace(basis, feature=Feature.F1, metric=Metric.EUCLIDEAN), point, time, seed, stream, values).map(
                lambda raw: (raw[0], End(node=f"{prefix}_color_texture", socket="Color"))
            )
            if basis.feature is Feature.DISTANCE_TO_EDGE
            else raw.map(lambda raw: (raw[0], End(node=f"{prefix}_texture", socket="Color")))
        )
        channels = color.bind(lambda color: node(f"{prefix}_channels", bpy.types.CompositorNodeSeparateColor, {"Image": color}, {"mode": "RGB"}))
        cell_color = shell.socket(*path, kind, Voronoi.cell_color.__name__)
        weighted = paired(mapped, channels).bind(lambda parts: math(f"{prefix}_brightness", "MULTIPLY", parts[0], ended(f"{prefix}_channels", "Green")(parts[1])))
        return paired(paired(mapped, channels), weighted).bind(
            lambda parts: paired(
                switched(f"{prefix}_density", "FLOAT", cell_color, parts[0][0], parts[1]), switched(f"{prefix}_tone", "FLOAT", cell_color, parts[0][0], ended(f"{prefix}_channels", "Red")(parts[0][1]))
            )
        )

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
        .map(lambda normals: (normals[0][0] | normals[1][0], normals[0][1], normals[1][1]))
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
    spectrum = (*path, Emission.spectrum.__name__)
    brightness = math(f"{key}_brightness", "POWER", 2.0, shell.socket(*path, Emission.intensity.__name__))
    working = masks.luminance(shell.config)

    def colored(index: int, tone: Operand) -> BlenderResult[dict[str, tuple[Fragment, End]]]:
        prefix = f"{key}_{index}"
        tint = mixed(f"{prefix}_tint", "RGBA", tone, shell.socket(*spectrum, Tint, Tint.start.__name__), shell.socket(*spectrum, Tint, Tint.end.__name__))
        body = (
            mixed(f"{prefix}_temperature", "FLOAT", tone, shell.socket(*spectrum, Incandescence, Incandescence.cool.__name__), shell.socket(*spectrum, Incandescence, Incandescence.hot.__name__))
            .bind(lambda temperature: node(f"{prefix}_blackbody", bpy.types.ShaderNodeBlackbody, {"Temperature": temperature}, {}))
            .map(ended(f"{prefix}_blackbody", "Color"))
        )
        thermal = paired(body, working).bind(
            lambda parts: nested(f"{prefix}_luminance", parts[1], {"image": parts[0]}, "luminance").bind(lambda luminance: vector(f"{prefix}_unit", "DIVIDE", parts[0], luminance))
        )
        ramp = item_key(*spectrum, Gradient, Gradient.ramp.__name__) if index == 0 else f"{prefix}_gradient"
        gradient = node(ramp, bpy.types.ShaderNodeValToRGB, {"Fac": tone}, {}, shell.case(Gradient, *spectrum).map(lambda gradient: gradient.ramp)).map(ended(ramp, "Color"))
        return accumulated(light.map(lambda light: (title, light)) for title, light in {Tint.title: tint, Incandescence.title: thermal, Gradient.title: gradient}.items()).map(dict)

    def brightened(parts: tuple[tuple[tuple[Fragment, End], ...], tuple[Fragment, End]]) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
        colors, gain = parts
        return accumulated(vector(f"{key}_{index}", "SCALE", color, gain) for index, color in enumerate(colors)).map(
            lambda lights: tuple((shared, end) for shared in (reduce(or_, (fragment for fragment, _ in lights), Fragment()),) for _, end in lights)
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
    motion = NoiseField.motion.__name__
    clock = Clock(key="clock", speed=inlet(motion, Motion.speed.__name__), hold=inlet(motion, Motion.hold.__name__)).value

    def lit(fields: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        density, tone = fields
        emission = (
            emitted(shell, (NoiseField.emission.__name__, Gated.value.__name__), (tone,))
            .bind(lambda lights: vector("radiance", "SCALE", lights[0], density))
            .bind(lambda radiance: switched("emission", "VECTOR", inlet(NoiseField.emission.__name__), (0.0, 0.0, 0.0), radiance))
        )
        extinction = (NoiseField.extinction.__name__, Gated.value.__name__)
        transmission = (
            math("optical_depth", "MULTIPLY", density, inlet(*extinction, Extinction.density.__name__))
            .bind(lambda depth: math("absorbed", "POWER", exp(-1.0), depth))
            .bind(lambda transmittance: switched("transmittance", "FLOAT", inlet(NoiseField.extinction.__name__), 1.0, transmittance))
        )
        coverage = transmission.bind(lambda transmission: math("coverage", "SUBTRACT", 1.0, transmission))
        scatter = coverage.bind(lambda cover: vector("scatter", "SCALE", inlet(*extinction, Extinction.color.__name__), cover))
        return (
            paired(paired(emission, scatter).bind(lambda parts: vector("light", "ADD", *parts)), coverage)
            .bind(lambda parts: node("generated", bpy.types.CompositorNodeSetAlpha, {"Image": parts[0], "Alpha": parts[1], "Type": "Replace Alpha"}, {}))
            .map(ended("generated", "Image"))
        )

    def built(at: FrameSpace, clock: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        axis = (
            vector("anchor", "MULTIPLY", inlet(NoiseField.center.__name__), at.extent)
            .bind(lambda anchor: vector("offset", "SUBTRACT", at.point, anchor))
            .bind(lambda offset: node("axes", bpy.types.ShaderNodeVectorRotate, {"Vector": offset, "Angle": inlet(NoiseField.angle.__name__)}, {"rotation_type": "Z_AXIS", "invert": True}))
            .map(ended("axes", "Vector"))
        )
        vortex = (NoiseField.vortex.__name__, Gated.value.__name__)
        envelope = (NoiseField.envelope.__name__, Gated.value.__name__)

        radius = inlet(*vortex, Vortex.radius.__name__)
        rho = axis.bind(lambda axes: vector("distance", "LENGTH", axes)).bind(lambda distance: math("rho", "DIVIDE", distance, radius))
        theta = axis.bind(lambda axes: node("polar", bpy.types.ShaderNodeSeparateXYZ, {"Vector": axes}, {})).bind(
            lambda polar: math("theta", "ARCTAN2", ended("polar", "Y")(polar), End(node="polar", socket="X"))
        )
        turn = paired(rho.bind(lambda rho: math("remaining", "SUBTRACT", 1.0, rho)), math("spin", "MULTIPLY", clock, inlet(*vortex, Vortex.spin.__name__))).bind(
            lambda parts: math("turn", "MULTIPLY_ADD", parts[0], inlet(*vortex, Vortex.twirl.__name__), parts[1])
        )
        phase = paired(theta, turn).bind(lambda parts: math("phase", "MULTIPLY_ADD", parts[0], inlet(*vortex, Vortex.arms.__name__), parts[1]))
        circle = (
            phase
            .bind(lambda phase: paired(math("cosine", "COSINE", phase), math("sine", "SINE", phase)))
            .bind(lambda trigonometry: node("ring", bpy.types.ShaderNodeCombineXYZ, {"X": trigonometry[0], "Y": trigonometry[1], "Z": inlet(*vortex, Vortex.tunnel.__name__)}, {}))
            .map(ended("ring", "Vector"))
        )
        span = rho.bind(lambda rho: math("bounded_radius", "MINIMUM", rho, 1.0)).bind(lambda bounded: math("span", "MULTIPLY", bounded, radius))
        mapped = paired(axis, paired(circle, span).bind(lambda parts: vector("cone", "SCALE", *parts))).bind(lambda parts: switched("mapped", "VECTOR", inlet(NoiseField.vortex.__name__), *parts))
        flow = vector("flow", "SCALE", inlet(NoiseField.drift.__name__), clock)
        point = paired(mapped, flow).bind(lambda parts: vector("sample", "SUBTRACT", *parts))
        extent = node("envelope_extent", bpy.types.ShaderNodeCombineXYZ, {"X": inlet(*envelope, Envelope.width.__name__), "Y": inlet(*envelope, Envelope.height.__name__)}, {}).map(
            ended("envelope_extent", "Vector")
        )
        loss = (
            paired(axis, extent)
            .bind(lambda parts: vector("envelope_position", "DIVIDE", *parts))
            .bind(lambda position: vector("envelope_distance", "LENGTH", position))
            .bind(lambda distance: math("envelope_edge", "SUBTRACT", 1.0, distance))
            .bind(lambda edge: math("envelope_reach", "MAXIMUM", edge, 0.0))
            .bind(lambda reach: math("envelope_squared", "MULTIPLY", reach, reach))
            .bind(lambda square: math("envelope_loss", "SUBTRACT", 1.0, square))
            .bind(lambda loss: switched("loss", "FLOAT", inlet(NoiseField.envelope.__name__), 0.0, loss))
        )
        time = math("evolution", "MULTIPLY", clock, inlet(NoiseField.evolution.__name__))
        return (
            paired(paired(point, time), loss).bind(lambda parts: sampled(shell, (NoiseField.basis.__name__,), *parts[0], inlet(NoiseField.seed.__name__), parts[1])).bind(lambda fields: lit(fields[0]))
        )

    return paired(framed("frame", shell.source, shell.aspect, shell.scale), clock).bind(lambda parts: built(*parts))


# --- [COMPOSITION] ----------------------------------------------------------------------

noise_field = CompositorEffect(record=NoiseField, graph=noised, coverage=Coverage.RESULT, requested=Some(precise))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["emitted", "gaussian", "noise_field", "noised", "periodic", "sample", "sampled"]
