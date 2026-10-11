"""Circular lens maps, independent spectral integration, and iris aberration."""

import functools

import bpy
from expression import Ok, Result, Some
import PyOpenColorIO

from rasm.blender.commands.operators import Call, Done, Operator, Save
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderFaults, BlenderImagingFault, BlenderRenderingFault, BlenderResult
from rasm.blender.nodes import graphs
from rasm.blender.nodes.graphs import (
    compared,
    End,
    ended,
    Fragment,
    framed,
    FrameSpace,
    INPUTS,
    Interface,
    masked,
    math,
    menu,
    mixed,
    nested,
    node,
    NodeGroup,
    Operand,
    pixels,
    Socket,
    summed,
    switched,
    vector,
)
from rasm.blender.render.effects.optics.defocus import iris
from rasm.blender.render.effects.stack import CompositorEffect, Coverage, Named, Shell
from rasm.blender.render.outputs.framing import FrameSize
from rasm.imaging.color.config import admitted
from rasm.imaging.compositing.blending import Entry
from rasm.imaging.effects.optics.distortion import Aberration, Direction, Distortion
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.files.images import ImageFile, Storage
from rasm.imaging.geometry.stmaps import Extension, StMap
from rasm.imaging.parameters import Rgb
from rasm.rendering.outputs.framing import Framing, Whole

# --- [OPERATIONS] -----------------------------------------------------------------------


@functools.cache
def radial() -> BlenderResult[NodeGroup]:
    """Returns circular forward and inverse radial maps with real-branch coverage."""
    point, extent, share, distortion, scale = (End(node=INPUTS, socket=key) for key in ("point", "extent", "share", "distortion", "scale"))

    def length(key: Key, value: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return (
            vector(f"{key}_absolute", "ABSOLUTE", value)
            .bind(lambda absolute: node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": absolute}, {}))
            .bind(
                lambda axes: math(f"{key}_xy", "MAXIMUM", ended(f"{key}_axes", "X")(axes), End(node=f"{key}_axes", socket="Y")).bind(
                    lambda xy: math(f"{key}_xyz", "MAXIMUM", xy, End(node=f"{key}_axes", socket="Z"))
                )
            )
            .bind(lambda largest: math(f"{key}_scale", "MAXIMUM", largest, 1.0))
            .bind(
                lambda largest: (
                    vector(f"{key}_normalized", "DIVIDE", value, largest)
                    .bind(lambda normalized: vector(f"{key}_unit_length", "LENGTH", normalized))
                    .bind(lambda unit: math(key, "MULTIPLY", unit, largest))
                )
            )
        )

    negative = compared("negative", "LESS_THAN", distortion, 0.0)

    def rectified(unit: Operand, radius: Operand, hypotenuse: Operand, negative: Operand) -> BlenderResult[tuple[Fragment, End]]:
        root = (
            paired(math("rect_below", "SUBTRACT", 1.0, radius), math("rect_above", "ADD", 1.0, radius))
            .bind(lambda parts: math("rect_discriminant", "MULTIPLY", *parts))
            .bind(lambda discriminant: math("rect_root", "SQRT", discriminant))
            .bind(lambda root: switched("rect_root_signed", "FLOAT", negative, root, hypotenuse))
        )
        covered = (
            compared("rect_real", "LESS_EQUAL", radius, 1.0)
            .bind(lambda real: node("rect_covered", bpy.types.FunctionNodeBooleanMath, {"Boolean": real, "Boolean_001": negative}, {"operation": "OR"}))
            .map(ended("rect_covered", "Boolean"))
        )
        factor = paired(root, covered).bind(
            lambda parts: (
                math("rect_denominator", "ADD", parts[0], 1.0)
                .bind(lambda denominator: math("rect_factor", "DIVIDE", scale, denominator))
                .bind(lambda factor: masked("rect_branch_factor", "FLOAT", parts[1], 0.0, factor))
            )
        )
        center = covered.bind(lambda covered: node("rect_center", bpy.types.ShaderNodeCombineXYZ, {"X": 0.5, "Y": 0.5, "Z": covered}, {})).map(ended("rect_center", "Vector"))
        return paired(factor, center).bind(lambda parts: vector("rectilinear", "MULTIPLY_ADD", unit, *parts))

    def inverted(unit: Operand, radius: Operand, negative: Operand) -> BlenderResult[tuple[Fragment, End]]:
        covered = paired(compared("lens_closed", "LESS_EQUAL", radius, 1.0), compared("lens_open", "LESS_THAN", radius, 1.0)).bind(lambda parts: switched("lens_covered", "FLOAT", negative, *parts))
        denominator = (
            math("lens_squared", "MULTIPLY", radius, radius)
            .bind(lambda squared: math("lens_negative", "MULTIPLY", squared, -1.0).bind(lambda negated: switched("lens_signed", "FLOAT", negative, squared, negated)))
            .bind(lambda signed: math("lens_sum", "ADD", 1.0, signed))
            .bind(lambda total: math("lens_denominator", "MULTIPLY", total, scale))
        )
        factor = paired(covered, denominator).bind(lambda parts: math("lens_factor", "DIVIDE", 0.5, parts[1]).bind(lambda factor: masked("lens_branch_factor", "FLOAT", parts[0], 0.0, factor)))
        center = covered.bind(lambda covered: node("lens_center", bpy.types.ShaderNodeCombineXYZ, {"X": 0.5, "Y": 0.5, "Z": covered}, {})).map(ended("lens_center", "Vector"))
        return paired(factor, center).bind(lambda parts: vector("lens", "MULTIPLY_ADD", unit, *parts))

    def mapped(offset: Operand, unit: Operand, strength: Operand) -> BlenderResult[NodeGroup]:
        lens_radius = vector("lens_radial", "SCALE", offset, strength).bind(lambda radial: length("lens_radius", radial))
        rect_radial = (
            math("rect_strength", "MULTIPLY", strength, scale)
            .bind(lambda strength: math("rect_diameter", "MULTIPLY", strength, 2.0))
            .bind(lambda strength: vector("rect_radial", "SCALE", offset, strength))
        )
        rectified_map = rect_radial.bind(
            lambda radial: paired(length("rect_radius", radial), vector("rect_hypotenuse_vector", "ADD", radial, (0.0, 0.0, 1.0)).bind(lambda value: length("rect_hypotenuse", value)))
        ).bind(lambda parts: negative.bind(lambda negative: rectified(unit, *parts, negative)))
        inverted_map = paired(lens_radius, negative).bind(lambda parts: inverted(unit, *parts))
        return paired(rectified_map, inverted_map).bind(
            lambda maps: NodeGroup.built(
                "radial_lens",
                "Radial Lens",
                Interface(
                    outputs=tuple(Socket(key=key, name=name, kind=bpy.types.NodeSocketVector) for key, name in (("rectilinear", "Rectilinear"), ("lens", "Lens"))),
                    inputs=tuple(
                        Socket(key=key, name=name, kind=kind)
                        for key, name, kind in (
                            ("point", "Point", bpy.types.NodeSocketVector),
                            ("extent", "Extent", bpy.types.NodeSocketVector),
                            ("share", "Share", bpy.types.NodeSocketFloat),
                            ("distortion", "Distortion", bpy.types.NodeSocketFloat),
                            ("scale", "Scale", bpy.types.NodeSocketFloat),
                        )
                    ),
                ),
                {"rectilinear": maps[0], "lens": maps[1]},
            )
        )

    return (
        vector("doubled", "MULTIPLY", point, (2.0, 2.0, 0.0))
        .bind(lambda doubled: vector("offset", "SUBTRACT", doubled, extent))
        .bind(
            lambda offset: paired(
                vector("ratio", "DIVIDE", offset, extent).bind(lambda ratio: vector("unit", "SCALE", ratio, share)),
                math("absolute_distortion", "ABSOLUTE", distortion).bind(lambda absolute: math("strength", "SQRT", absolute)),
            ).bind(lambda parts: mapped(offset, *parts))
        )
    )


def sampled(key: Key, source: Operand, frame: FrameSpace, extension: tuple[Fragment, End], position: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Samples a covered map through the once-built extension menu.

    Args:
        key: Prefix of the sampler's native node keys.
        source: Associated source image.
        frame: Displayed camera placement within the source's data window.
        extension: Shared menu mapping Mirror to native Extend, with item booleans.
        position: Camera-frame fractions and geometric coverage in Z.

    Returns:
        Bilinear source light and graph, or native construction faults.
    """
    fragment, selector = extension
    coverage = (
        node(f"{key}_channels", bpy.types.ShaderNodeSeparateXYZ, {"Vector": position}, {})
        .bind(lambda channels: node(f"{key}_coverage", bpy.types.ShaderNodeCombineXYZ, {"X": 1.0, "Y": 1.0, "Z": ended(f"{key}_channels", "Z")(channels)}, {}))
        .map(ended(f"{key}_coverage", "Vector"))
    )
    coordinate = paired(graphs.sampled(f"{key}_data", frame, position), coverage).bind(lambda parts: vector(f"{key}_covered", "MULTIPLY", *parts))
    return (
        coordinate
        .bind(
            lambda position: (
                vector(f"{key}_wrap", "WRAP", position, (1.0, 1.0, 1.0), (-1.0, -1.0, -1.0))
                .bind(lambda wrapped: vector(f"{key}_fold", "ABSOLUTE", wrapped))
                .bind(lambda folded: mixed(f"{key}_at", "VECTOR", (fragment, End(node=selector.node, socket=Extension.MIRROR.label)), position, folded))
            )
        )
        .bind(lambda at: node(key, bpy.types.CompositorNodeMapUV, {"Image": source, "UV": at, "Interpolation": "Bilinear", "Extension X": extension, "Extension Y": extension}, {}))
        .map(ended(key, "Image"))
    )


def distorted(shell: Shell[Distortion]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds fitted circular distortion or its covered inverse map.

    Args:
        shell: Source image, displayed frame, lens sockets, and menu items.

    Returns:
        The sampled image and graph, or accumulated native construction faults.
    """
    coefficient = shell.socket(Distortion.distortion.__name__)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def built(lens: NodeGroup, frame: FrameSpace, extension: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        corner = (
            node("sides", bpy.types.ShaderNodeSeparateXYZ, {"Vector": frame.extent}, {})
            .bind(lambda sides: math("smaller", "MINIMUM", ended("sides", "X")(sides), End(node="sides", socket="Y")))
            .bind(lambda smaller: math("corner", "MULTIPLY", smaller, smaller))
        )
        fitted = (
            compared("positive", "GREATER_THAN", coefficient, 0.0)
            .bind(lambda positive: node("fitted", bpy.types.FunctionNodeBooleanMath, {"Boolean": shell.socket(Distortion.fit.__name__), "Boolean_001": positive}, {"operation": "AND"}))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .map(ended("fitted", "Boolean"))
        )
        return (
            paired(fitted, corner)
            .bind(lambda parts: math("anchor", "MULTIPLY_ADD", *parts, 1.0))
            .bind(
                lambda anchor: (
                    math("inverse_anchor", "DIVIDE", 1.0, anchor)
                    .bind(lambda inverse: math("scale_sum", "ADD", inverse, coefficient))
                    .bind(lambda total: math("inverse_scale", "DIVIDE", 1.0, total))
                    .bind(lambda scale: math("scale", "DIVIDE", scale, anchor))
                )
            )
            .bind(lambda scale: nested("radial", lens, {"point": frame.point, "extent": frame.extent, "share": 1.0, "distortion": coefficient, "scale": scale}, "rectilinear"))
            .bind(
                lambda placed: menu(
                    "direction",
                    "VECTOR",
                    shell.socket(Distortion.direction.__name__),  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    shell.items[Distortion.direction.__name__,],  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    {Direction.DISTORT.label: placed, Direction.UNDISTORT.label: (placed[0], End(node="radial", socket="lens"))},
                )
            )
            .bind(lambda position: sampled("map", shell.source, frame, extension, position))
        )

    extension = Distortion.extension.__name__  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return paired(
        paired(radial(), framed("frame", shell.source, shell.aspect, shell.scale)),
        menu("extension", "MENU", shell.socket(extension), shell.items[extension,], {member.label: (Extension.EXTEND if member is Extension.MIRROR else member).label for member in Extension}),
    ).bind(lambda parts: built(*parts[0], parts[1]))


def aberrated(shell: Shell[Aberration]) -> BlenderResult[tuple[Fragment, End]]:
    """Integrates independent spectral maps and gathers red and blue axial light.

    Args:
        shell: Spectral settings, displayed frame, and native lens inputs.

    Returns:
        Associated spectral light and graph, or every independent graph fault.
    """

    def gathered(frame: FrameSpace, bokeh: tuple[Fragment, End], split: Fragment, channel: str, ratio: float) -> BlenderResult[tuple[Fragment, End]]:
        key = channel.lower()
        return (
            math(f"{key}_radius", "MULTIPLY", shell.socket(Aberration.axial.__name__), ratio)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            .bind(lambda radius: pixels(f"{key}_size", frame, radius))
            .bind(lambda size: node(f"{key}_gather", bpy.types.CompositorNodeBokehBlur, {"Image": ended("split", channel)(split), "Bokeh": bokeh, "Size": size}, {}))
            .map(ended(f"{key}_gather", "Image"))
        )

    def defocused(frame: FrameSpace, lateral: tuple[Fragment, End], bokeh: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        ratios = dict(zip(("Red", "Green", "Blue"), (Aberration.defocus.red, Aberration.defocus.green, Aberration.defocus.blue)))
        split = node("split", bpy.types.CompositorNodeSeparateColor, {"Image": lateral}, {"mode": "RGB"})
        channels = accumulated(split.bind(lambda split: gathered(frame, bokeh, split, channel, ratio) if ratio > 0.0 else Ok(ended("split", channel)(split))) for channel, ratio in ratios.items())
        return (
            paired(channels, split)
            .bind(lambda parts: node("combined", bpy.types.CompositorNodeCombineColor, {**dict(zip(ratios, parts[0])), "Alpha": ended("split", "Alpha")(parts[1])}, {"mode": "RGB"}))
            .map(ended("combined", "Image"))
        )

    def dispersed(lens: NodeGroup, frame: FrameSpace, extension: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        def waved(index: int, coefficient: float, weight: Rgb) -> BlenderResult[tuple[tuple[Fragment, End], tuple[Fragment, End]]]:
            sample = (
                math(f"wave_{index}", "MULTIPLY", shell.socket(Aberration.dispersion.__name__), coefficient)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                .bind(lambda spread: nested(f"radial_{index}", lens, {"point": frame.point, "extent": frame.extent, "share": 1.0, "distortion": spread, "scale": 1.0}, "rectilinear"))
                .bind(lambda placed: sampled(f"sample_{index}", shell.source, frame, extension, placed))
            )
            color = sample.bind(lambda sample: vector(f"color_{index}", "MULTIPLY", sample, (weight.red, weight.green, weight.blue)))
            alpha = sample.bind(lambda sample: node(f"channels_{index}", bpy.types.CompositorNodeSeparateColor, {"Image": sample}, {"mode": "RGB"})).bind(
                lambda channels: math(f"alpha_{index}", "MULTIPLY", ended(f"channels_{index}", "Alpha")(channels), 1.0 / shell.record.samples)
            )
            return paired(color, alpha)

        waves = accumulated(waved(index, *wave) for index, wave in enumerate(shell.record.spectrum)).map(lambda waves: tuple(zip(*waves)))
        lateral = (
            waves
            .bind(lambda channels: paired(summed("color_sum", channels[0], kind="VECTOR"), summed("alpha_sum", channels[1], kind="FLOAT")))
            .bind(lambda sums: node("lateral", bpy.types.CompositorNodeSetAlpha, {"Image": sums[0], "Alpha": sums[1], "Type": "Replace Alpha"}, {}))
            .map(ended("lateral", "Image"))
        )
        return paired(lateral, iris("iris", shell, Aberration.iris.__name__)).bind(lambda parts: defocused(frame, *parts))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    extension = Aberration.extension.__name__  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    return paired(
        paired(radial(), framed("frame", shell.source, shell.aspect, shell.scale)),
        menu("extension", "MENU", shell.socket(extension), shell.items[extension,], {member.label: (Extension.EXTEND if member is Extension.MIRROR else member).label for member in Extension}),
    ).bind(lambda parts: dispersed(*parts[0], parts[1]))


def exported(call: Call[Named]) -> BlenderResult[Done]:
    """Writes the selected lens map as a data RGBA float OpenEXR.

    Args:
        call: Selected entry and native export browser path.

    Returns:
        Written path, or independent entry, framing, configuration, and file faults.
    """

    def written(config: PyOpenColorIO.Config, effect: Distortion, framing: Framing) -> BlenderResult[Done]:
        def baked(admitted: tuple[Framing, tuple[int, int]]) -> BlenderResult[Done]:
            _, (width, height) = admitted
            plane = StMap.bake(
                width, height, effect.mapping(width * framing.pixel_aspect_x / (height * framing.pixel_aspect_y)), (effect.extension, effect.extension), source_size=(width, height)
            ).plane
            return (
                ImageFile(pixels=plane, colorspace=PyOpenColorIO.ROLE_DATA)
                .write(str(call.paths[0]), Storage(), {}, config)
                .map_error(lambda faults: (BlenderImagingFault(faults=faults),))
                .map(lambda path: Done(notes=(("INFO", path),)))
            )

        def sized(bounds: Descriptor[bpy.types.IntProperty]) -> BlenderResult[Done]:
            ready: BlenderResult[tuple[Framing, tuple[int, int]]] = framing.rendered((bounds.prop.hard_min, bounds.prop.hard_max)).map_error(lambda faults: (BlenderRenderingFault(faults=faults),))
            return Result[tuple[Framing, tuple[int, int]], BlenderFaults].bind(ready, baked)

        return Descriptor.resolve(bpy.types.RenderSettings, "resolution_x", bpy.types.IntProperty).bind(sized)

    def ready(scene: bpy.types.Scene) -> BlenderResult[Done]:
        entry = distortion.named(call.identity, scene, call.record.name).bind(lambda held: distortion.read(call.identity, held))
        config: BlenderResult[PyOpenColorIO.Config] = admitted(PyOpenColorIO.GetCurrentConfig(), (PyOpenColorIO.ROLE_DATA,)).map_error(lambda faults: (BlenderImagingFault(faults=faults),))
        framing = FrameSize().bound(call.identity).bind(lambda framed: framed.read(scene, Some(Whole())))
        return Result[tuple[tuple[PyOpenColorIO.Config, Entry[Distortion]], Framing], BlenderFaults].bind(
            paired(paired(config, entry), framing), lambda parts: written(parts[0][0], parts[0][1].effect, parts[1])
        )

    return call.root(bpy.types.Scene).bind(ready)


# --- [COMPOSITION] ----------------------------------------------------------------------

st_map_export = Operator(
    key="export_st_map",
    title="Export ST Map",
    description="Writes the lens mapping as a float OpenEXR data image",
    form=Named,
    run=exported,
    invoke=Some(Save(extensions=lambda _: (".exr",))),
    undo=False,
)

distortion = CompositorEffect(record=Distortion, graph=distorted, coverage=Coverage.RESULT, registrables=(st_map_export,))
aberration = CompositorEffect(record=Aberration, graph=aberrated, coverage=Coverage.RESULT)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["aberrated", "aberration", "distorted", "distortion", "exported", "radial", "sampled", "st_map_export"]
