"""Native rendered discontinuities, painted regions, and continuous ink coverage."""

from math import sqrt

import bpy
from expression import Some
import msgspec

from blender.bpy.data.rna.conditions import AllOf, Condition, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, pixels
from blender.bpy.nodes.graphs import End, ended, Fragment, math, menu, node, Operand, Part, switched, vector
from blender.bpy.nodes.interface import item_key
from blender.bpy.render.effects.stack import CompositorEffect, Coverage, Shell
from blender.bpy.render.effects.viewers import inspection
from blender.bpy.render.view_layers.passes import Entity, PassRequest
from blender.bpy.scene.cameras import camera_type
from blender.imaging.effects.stylize.edges import Anisotropic, Classic, Edges, FrameWidth, IdSource, Inner, Outer, Paint, Placement, Taper, WorldWidth
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Gated

# --- [OPERATIONS] -----------------------------------------------------------------------


def read_ids(record: Edges) -> tuple[Entity, ...]:
    """Returns the object and material identity terms enabled by the record."""
    return tuple(entity for entity, enabled in ((Entity.OBJECT, record.outer.value.objects), (Entity.MATERIAL, record.inner.value.materials)) if enabled)


def requested(record: Edges, layer: str) -> BlenderResult[PassRequest]:
    """Resolves render flags, engine support, and precision before graph reconciliation.

    Args:
        record: Entry's rendered guide selection.
        layer: Native view layer supplying the Render Layers node.

    Returns:
        Depth and normal request with selected identity passes, or accumulated
        native descriptor faults.
    """
    entities = read_ids(record)
    flags = ("use_pass_normal", *(entity.flag if record.source is IdSource.CRYPTOMATTE else f"use_pass_{entity.name.lower()}_index" for entity in entities))
    return paired(
        PassRequest.depth(layer, precise=bool(entities)),
        paired(accumulated(Descriptor.resolve(bpy.types.ViewLayer, flag, bpy.types.BoolProperty) for flag in flags), Descriptor.resolve(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)),
    ).map(
        lambda values: msgspec.structs.replace(
            values[0],
            flags=values[0].flags | frozenset(values[1][0]),
            condition=Some(Setting(descriptor=values[1][1], accepted=("CYCLES",) if entities and record.source is IdSource.INDEX else ("CYCLES", "BLENDER_EEVEE"))),
        )
    )


def condition(record: Edges) -> Condition:
    """Returns the camera requirement of world widths."""
    return camera_type("PERSP", "ORTHO") if any(isinstance(line.width, WorldWidth) for line in (record.outer.value, record.inner.value)) else AllOf(conditions=())


def outlined(shell: Shell[Edges]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds painted regions, rendered guide cores, and nearest-width ink coverage.

    Args:
        shell: Entry's image, native parameter endpoints, and retained graph fields.

    Returns:
        Inked associated color and its complete graph, or accumulated native graph faults.
    """
    record, source, socket = shell.record, shell.source, shell.socket
    entities = read_ids(record)
    paint_field, outer_field, inner_field, source_field, taper_field, paper = Edges.__struct_fields__
    _, value = Gated.__struct_fields__
    outer = (outer_field, value)
    inner = (inner_field, value)
    paint = (paint_field, value)
    taper = (taper_field, value)

    def guides(image: Operand, orthographic: Operand) -> BlenderResult[tuple[tuple[Fragment, End], ...]]:
        depth_field, _, _, _, _ = Outer.__struct_fields__
        crease, _, contrast_field, _, _ = Inner.__struct_fields__
        key, depth, normals = "guides", shell.layer("Depth"), shell.layer("Normal")
        depth_gap = socket(*outer, depth_field, value)
        crease_angle = socket(*inner, crease, value)
        contrast = socket(*inner, contrast_field, value)
        inverse = math(f"{key}_inverse", "DIVIDE", 1.0, depth)

        def creases() -> BlenderResult[tuple[Fragment, End]]:
            return Fragment.joined(
                vector(f"{key}_normal_length", "LENGTH", normals),
                node(f"{key}_normal_minimum", bpy.types.CompositorNodeDilateErode, {"Mask": End(node=f"{key}_normal_length"), "Size": -1, "Type": "Steps"}, {}),
                math(f"{key}_normal_valid", "GREATER_THAN", End(node=f"{key}_normal_minimum", socket="Mask"), 0.0),
                node(f"{key}_normal_sobel", bpy.types.CompositorNodeFilter, {"Image": normals, "Fac": 1.0, "Type": "Sobel"}, {}),
                vector(f"{key}_normal_magnitude", "LENGTH", End(node=f"{key}_normal_sobel", socket="Image")),
                math(f"{key}_half_angle", "MULTIPLY", crease_angle, 0.5),
                math(f"{key}_angle_sine", "SINE", End(node=f"{key}_half_angle")),
                math(f"{key}_crease_threshold", "MULTIPLY", End(node=f"{key}_angle_sine"), 8.0),
                math(f"{key}_normal_edge", "GREATER_THAN", End(node=f"{key}_normal_magnitude"), End(node=f"{key}_crease_threshold")),
                math(f"{key}_crease", "MULTIPLY", End(node=f"{key}_normal_valid"), End(node=f"{key}_normal_edge")),
            ).map(ended(f"{key}_crease", "Value"))

        return accumulated((
            inverse
            .bind(lambda reciprocal: switched(f"{key}_coordinate", "FLOAT", orthographic, reciprocal, depth))
            .bind(
                lambda coordinate: node(f"{key}_laplace", bpy.types.CompositorNodeFilter, {"Image": coordinate, "Fac": 1.0, "Type": "Laplace"}, {}).bind(
                    lambda laplace: math(f"{key}_relative", "DIVIDE", ended(f"{key}_laplace", "Image")(laplace), coordinate)
                )
            )
            .bind(lambda relative: math(f"{key}_depth_measure", "MULTIPLY", relative, 8.0 / (3.0 * sqrt(2.0))))
            .bind(lambda measure: math(f"{key}_depth", "GREATER_THAN", measure, depth_gap)),
            creases(),
            node(f"{key}_color_sobel", bpy.types.CompositorNodeFilter, {"Image": image, "Fac": 1.0, "Type": "Sobel"}, {})
            .bind(lambda sobel: math(f"{key}_contrast_measure", "DIVIDE", ended(f"{key}_color_sobel", "Image")(sobel), 4.0))
            .bind(lambda measure: math(f"{key}_contrast", "GREATER_THAN", measure, contrast)),
            inverse.bind(
                lambda reciprocal: node(f"{key}_softened", bpy.types.CompositorNodeFilter, {"Image": reciprocal, "Fac": 1.0, "Type": "Soften"}, {}).bind(
                    lambda softened: (
                        math(f"{key}_near_share", "MULTIPLY_ADD", depth_gap, -0.25, 1.0)
                        .bind(lambda share: math(f"{key}_near_threshold", "MULTIPLY", reciprocal, share))
                        .bind(lambda threshold: math(f"{key}_near", "LESS_THAN", ended(f"{key}_softened", "Image")(softened), threshold))
                    )
                )
            ),
        ))

    def field(key: str, core: Operand, reach: Operand) -> tuple[Part, ...]:
        return (
            node(f"{key}_sdf", bpy.types.CompositorNodeMaskToSDF, {"Mask": core}, {}),
            math(f"{key}_seeded", "MULTIPLY", core, reach),
            node(f"{key}_dimensions", bpy.types.CompositorNodeImageInfo, {"Image": End(node=f"{key}_seeded")}, {}),
            vector(f"{key}_cell", "DIVIDE", (1.0, 1.0, 0.0), End(node=f"{key}_dimensions", socket="Dimensions")),
            vector(f"{key}_offset", "MULTIPLY_ADD", End(node=f"{key}_cell", socket="Vector"), (0.5, 0.5, 0.0), (0.0, 0.0, 1.0)),
            vector(f"{key}_uv", "MULTIPLY_ADD", End(node=f"{key}_sdf", socket="Nearest Pixel"), End(node=f"{key}_cell", socket="Vector"), End(node=f"{key}_offset", socket="Vector")),
            node(
                f"{key}_sample",
                bpy.types.CompositorNodeMapUV,
                {"Image": End(node=f"{key}_seeded"), "UV": End(node=f"{key}_uv", socket="Vector"), "Interpolation": "Nearest", "Extension X": "Clip", "Extension Y": "Clip"},
                {},
            ),
            node(f"{key}_size", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=f"{key}_sample", socket="Image")}, {"mode": "RGB"}),
        )

    def width(key: str, path: tuple[str, ...], frame: FrameSpace) -> tuple[Part, ...]:
        size, limit = WorldWidth.__struct_fields__
        (size_frame,) = FrameWidth.__struct_fields__
        return (
            math(f"{key}_world", "MULTIPLY", socket(*path, WorldWidth, size), End(node="world_scale", socket="Output")),
            math(f"{key}_limit", "MINIMUM", End(node=f"{key}_world"), socket(*path, WorldWidth, limit)),
            menu(f"{key}_width", "FLOAT", socket(*path), shell.items[path], {FrameWidth.title: socket(*path, FrameWidth, size_frame), WorldWidth.title: End(node=f"{key}_limit")}),
            pixels(f"{key}_pixels", frame, End(node=f"{key}_width", socket="Output")),
            math(f"{key}_tapered", "MULTIPLY", End(node=f"{key}_pixels"), End(node="taper_value", socket="Blue")),
            switched(f"{key}_reach", "FLOAT", socket(taper_field), End(node=f"{key}_pixels"), End(node=f"{key}_tapered")),
        )

    def drawn(frame: FrameSpace, terms: tuple[tuple[Fragment, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
        depth_field, objects, ink_outer, width_outer, placement = Outer.__struct_fields__
        crease_field, materials, contrast_field, ink_inner, width_inner = Inner.__struct_fields__
        near_field, ramp = Taper.__struct_fields__
        depth, crease, contrast, near = terms
        return Fragment.joined(
            *(
                node(
                    f"{entity.name.lower()}_id",
                    bpy.types.CompositorNodeSeparateColor,
                    {"Image": shell.layer(f"{entity.pass_name}00" if record.source is IdSource.CRYPTOMATTE else f"{entity.label} Index")},
                    {"mode": "RGB"},
                )
                for entity in entities
            ),
            *(
                node(
                    f"{choice.name.lower()}_ids",
                    bpy.types.ShaderNodeCombineXYZ,
                    {axis: End(node=f"{entity.name.lower()}_id", socket="Red") for axis, entity in zip(("X", "Y"), (Entity.OBJECT, Entity.MATERIAL)) if choice is record.source and entity in entities},
                    {},
                )
                for choice in IdSource
            ),
            menu("ids", "VECTOR", socket(source_field), shell.items[source_field,], {choice.label: End(node=f"{choice.name.lower()}_ids", socket="Vector") for choice in IdSource}),
            node("id", bpy.types.ShaderNodeSeparateXYZ, {"Vector": End(node="ids", socket="Output")}, {}),
            *(
                part
                for key, axis, gate in (("objects", "X", socket(*outer, objects)), ("materials", "Y", socket(*inner, materials)))
                for part in (
                    node(f"{key}_maximum", bpy.types.CompositorNodeDilateErode, {"Mask": End(node="id", socket=axis), "Size": 1, "Type": "Steps"}, {}),
                    node(f"{key}_minimum", bpy.types.CompositorNodeDilateErode, {"Mask": End(node="id", socket=axis), "Size": -1, "Type": "Steps"}, {}),
                    math(f"{key}_mark", "GREATER_THAN", End(node=f"{key}_maximum", socket="Mask"), End(node=f"{key}_minimum", socket="Mask")),
                    switched(key, "FLOAT", gate, 0.0, End(node=f"{key}_mark")),
                )
            ),
            switched("depth", "FLOAT", socket(*outer, depth_field), 0.0, depth),
            switched("crease", "FLOAT", socket(*inner, crease_field), 0.0, crease),
            switched("contrast", "FLOAT", socket(*inner, contrast_field), 0.0, contrast),
            math("outer_core", "MAXIMUM", End(node="depth", socket="Output"), End(node="objects", socket="Output")),
            math("inner_turns", "MAXIMUM", End(node="crease", socket="Output"), End(node="materials", socket="Output")),
            math("inner_terms", "MAXIMUM", End(node="inner_turns"), End(node="contrast", socket="Output")),
            math("inner_core", "SUBTRACT", End(node="inner_terms"), End(node="outer_core"), clamp=True),
            math("inverse", "DIVIDE", 1.0, shell.layer("Depth")),
            node("nearest", bpy.types.CompositorNodeDilateErode, {"Mask": End(node="inverse"), "Size": 1, "Type": "Steps"}, {}),
            node("projection", bpy.types.FunctionNodeSeparateMatrix, {"Matrix": End(node="lens", socket="Projection Matrix")}, {}),
            *(math(f"projection_{axis}", "ABSOLUTE", End(node="projection", socket=f"Column {index} Row {index}")) for axis, index in (("x", 1), ("y", 2))),
            math("projection_greater", "MINIMUM", End(node="projection_x"), End(node="projection_y")),
            math("render_focal", "MULTIPLY", End(node="projection_greater"), 0.5),
            math("focal", "DIVIDE", End(node="render_focal"), shell.scale),
            math("perspective_scale", "MULTIPLY", End(node="focal"), End(node="nearest", socket="Mask")),
            switched("world_scale", "FLOAT", End(node="lens", socket="Is Orthographic"), End(node="perspective_scale"), End(node="focal")),
            math("taper_position", "MULTIPLY", socket(*taper, near_field), End(node="nearest", socket="Mask"), clamp=True),
            node(item_key(*taper, ramp), bpy.types.ShaderNodeValToRGB, {"Fac": End(node="taper_position")}, {}, Some(record.taper.value.ramp)),
            node("taper_value", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=item_key(*taper, ramp), socket="Color")}, {"mode": "HSV"}),
            *width("outer", (*outer, width_outer), frame),
            *width("inner", (*inner, width_inner), frame),
            *(
                part
                for key in ("outer", "inner")
                for part in (
                    *field(key, End(node=f"{key}_core"), End(node=f"{key}_reach", socket="Output")),
                    math(f"{key}_half", "MULTIPLY", End(node=f"{key}_size", socket="Red"), 0.5),
                    math(f"{key}_center", "MULTIPLY_ADD", End(node=f"{key}_sdf", socket="SDF"), -1.0, End(node=f"{key}_half"), clamp=True),
                )
            ),
            math("near_core", "MULTIPLY", End(node="outer_core"), near),
            math("far_core", "SUBTRACT", End(node="outer_core"), End(node="near_core")),
            *field("near", End(node="near_core"), End(node="outer_reach", socket="Output")),
            *field("far", End(node="far_core"), End(node="outer_reach", socket="Output")),
            math("side_delta", "SUBTRACT", End(node="far_sdf", socket="SDF"), End(node="near_sdf", socket="SDF")),
            math("side_weight", "MULTIPLY_ADD", End(node="side_delta"), 0.5, 0.5, clamp=True),
            math("near_present", "GREATER_THAN", End(node="near_size", socket="Red"), 0.0),
            math("claimed", "MULTIPLY", End(node="near_present"), End(node="side_weight")),
            math("unclaimed", "SUBTRACT", 1.0, End(node="claimed")),
            *(
                part
                for key, side in (("near", "claimed"), ("far", "unclaimed"))
                for part in (
                    math(f"{key}_coverage", "SUBTRACT", End(node=f"{key}_size", socket="Red"), End(node=f"{key}_sdf", socket="SDF"), clamp=True),
                    math(f"{key}_sided", "MULTIPLY", End(node=f"{key}_coverage"), End(node=side)),
                )
            ),
            menu(
                "placement",
                "FLOAT",
                socket(*outer, placement),
                shell.items[*outer, placement],
                {Placement.CENTER.label: End(node="outer_center"), Placement.INSIDE.label: End(node="near_sided"), Placement.OUTSIDE.label: End(node="far_sided")},
            ),
            *(
                part
                for key, path, ink, coverage, below in (
                    ("inner", inner, ink_inner, End(node="inner_center"), End(node="canvas", socket="Output")),
                    ("outer", outer, ink_outer, End(node="placement", socket="Output"), End(node="inner_drawn", socket="Output")),
                )
                for part in (
                    node(f"{key}_smooth", bpy.types.CompositorNodeAntiAliasing, {"Image": coverage}, {}),
                    math(f"{key}_shown", "MULTIPLY", End(node=f"{key}_smooth", socket="Image"), socket(path[0])),
                    node(f"{key}_over", bpy.types.CompositorNodeAlphaOver, {"Background": below, "Foreground": socket(*path, ink), "Fac": End(node=f"{key}_smooth", socket="Image")}, {}),
                    switched(f"{key}_drawn", "RGBA", socket(path[0]), below, End(node=f"{key}_over", socket="Image")),
                )
            ),
            math("lines", "MAXIMUM", End(node="inner_shown"), End(node="outer_shown")),
        ).map(ended("outer_drawn", "Output"))

    def painted(frame: FrameSpace) -> BlenderResult[tuple[Fragment, End]]:
        (size,) = Paint.__struct_fields__
        _, uniformity, sharpness, eccentricity = Anisotropic.__struct_fields__
        _, precise = Classic.__struct_fields__
        return Fragment.joined(
            *(
                part
                for case in (Anisotropic, Classic)
                for part in (
                    pixels(f"{case.__name__.lower()}_size", frame, socket(*paint, case, size)),
                    node(
                        case.__name__.lower(),
                        bpy.types.CompositorNodeKuwahara,
                        {
                            "Image": source,
                            "Type": case.title,
                            "Size": End(node=f"{case.__name__.lower()}_size"),
                            **(
                                {"Uniformity": End(node="uniformity"), "Sharpness": socket(*paint, Anisotropic, sharpness), "Eccentricity": socket(*paint, Anisotropic, eccentricity)}
                                if case is Anisotropic
                                else {"High Precision": socket(*paint, Classic, precise)}
                            ),
                        },
                        {},
                    ),
                )
            ),
            pixels("paint_uniformity", frame, socket(*paint, Anisotropic, uniformity)),
            math("uniformity", "ROUND", End(node="paint_uniformity")),
            menu("brush", "RGBA", socket(*paint), shell.items[paint], {case.title: End(node=case.__name__.lower(), socket="Image") for case in (Anisotropic, Classic)}),
            switched("painted", "RGBA", socket(paint_field), source, End(node="brush", socket="Output")),
            switched("canvas", "RGBA", socket(paper), End(node="painted", socket="Output"), socket(paper, value)),
            node("camera", bpy.types.GeometryNodeInputActiveCamera, {}, {}),
            node("lens", bpy.types.GeometryNodeCameraInfo, {"Camera": End(node="camera", socket="Active Camera")}, {}),
        ).bind(lambda base: guides((base, End(node="painted", socket="Output")), (base, End(node="lens", socket="Is Orthographic"))).bind(lambda terms: drawn(frame, terms)))

    return framed("frame", source, shell.aspect, shell.scale).bind(painted)


# --- [COMPOSITION] ----------------------------------------------------------------------

edges: CompositorEffect[Edges] = CompositorEffect(
    record=Edges,
    graph=outlined,
    coverage=Coverage.RESULT,
    requested=Some(requested),
    condition=Some(condition),
    registrables=(inspection(lambda: edges, "lines", "Outline Mask", "Inspect the rendered line coverage", lambda: End(node="lines")),),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["condition", "edges", "outlined", "read_ids", "requested"]
