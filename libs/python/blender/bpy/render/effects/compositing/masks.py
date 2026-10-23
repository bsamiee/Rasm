"""Displayed-frame mask keys, working luminance, and composited coverage."""

from collections.abc import Callable
import functools
from math import pi, tau

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import PyOpenColorIO

from blender.bpy.extension.identity import Key
from blender.bpy.faults import BlenderResult
from blender.bpy.nodes.coordinates import framed, FrameSpace, pixels
from blender.bpy.nodes.graphs import compared, End, ended, Fragment, INPUTS, math, menu, nested, node, NodeGroup, Operand, switched, vector
from blender.bpy.nodes.interface import DesiredItem, Interface, InterfacePanel, item_key, MenuItems, Socket
from blender.bpy.render.color.spaces import COLOR_INPUT, COLOR_RESULT, converted, hsy, logarithmic
from blender.imaging.compositing.masks import Band, HueRange, Mask, MaskOperation, MatteChannel, Outline, Qualifier, Shape, TrackMatte, Transition
from blender.imaging.faults import accumulated, paired
from blender.imaging.parameters import Case, Gated

# --- [OPERATIONS] -----------------------------------------------------------------------


def luminance(config: PyOpenColorIO.Config) -> BlenderResult[NodeGroup]:
    """Returns working-space Y group from scene-linear conversion."""
    return (
        converted("xyz", config, PyOpenColorIO.ROLE_SCENE_LINEAR, PyOpenColorIO.ROLE_INTERCHANGE_DISPLAY, End(node=INPUTS, socket="image"))
        .bind(lambda xyz: node("xyz_split", bpy.types.CompositorNodeSeparateColor, {"Image": xyz}, {"mode": "RGB"}))
        .bind(
            lambda split: NodeGroup.built(
                "working_luminance",
                "Working Luminance",
                Interface(inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),), outputs=(Socket(key="luminance", name="Luminance", kind=bpy.types.NodeSocketFloat),)),
                {"luminance": ended("xyz_split", "Green")(split)},
            )
        )
    )


@functools.cache
def edge() -> BlenderResult[NodeGroup]:
    """Returns smoothstep edge group, inclusive step at zero softness."""
    distance, softness = End(node=INPUTS, socket="distance"), End(node=INPUTS, socket="softness")
    soft = (
        math("negative", "MULTIPLY", softness, -1.0)
        .bind(
            lambda negative: node(
                "ramp", bpy.types.ShaderNodeMapRange, {"Value": distance, "From Min": negative, "From Max": 0.0, "To Min": 0.0, "To Max": 1.0}, {"interpolation_type": "SMOOTHSTEP", "clamp": True}
            )
        )
        .map(ended("ramp", "Result"))
    )
    hard = math("below", "LESS_THAN", distance, 0.0).bind(lambda below: math("step", "SUBTRACT", 1.0, below))
    return (
        paired(soft, hard)
        .bind(lambda ends: math("soft", "GREATER_THAN", softness, 0.0).bind(lambda enabled: switched("key", "FLOAT", enabled, ends[1], ends[0])))
        .bind(
            lambda key: NodeGroup.built(
                "soft_edge",
                "Soft Edge",
                Interface(
                    inputs=(Socket(key="distance", name="Distance", kind=bpy.types.NodeSocketFloat), Socket(key="softness", name="Softness", kind=bpy.types.NodeSocketFloat)),
                    outputs=(Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat),),
                ),
                {"key": key},
            )
        )
    )


@functools.cache
def band() -> BlenderResult[NodeGroup]:
    """Returns feathered interval group, empty when high is below low."""
    value, low, high, softness = (End(node=INPUTS, socket=name) for name in ("value", "low", "high", "softness"))
    return (
        edge()
        .bind(
            lambda group: paired(
                accumulated(
                    math(name, "SUBTRACT", left, right).bind(lambda distance: nested(f"{name}_edge", group, {"distance": distance, "softness": softness}, "key"))
                    for name, left, right in (("low", value, low), ("high", high, value))
                ).bind(lambda keys: math("inside", "MULTIPLY", *keys)),
                compared("ordered", "GREATER_EQUAL", high, low),
            )
        )
        .bind(lambda ends: math("key", "MULTIPLY", *ends))
        .bind(
            lambda key: NodeGroup.built(
                "band",
                "Band",
                Interface(
                    inputs=tuple(Socket(key=name, name=name.title(), kind=bpy.types.NodeSocketFloat) for name in ("value", "low", "high", "softness")),
                    outputs=(Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat),),
                ),
                {"key": key},
            )
        )
    )


@functools.cache
def shape() -> BlenderResult[NodeGroup]:
    """Returns rotated box and ellipse key group over displayed frame."""
    outline, position_shape, size_shape, rotation, feather_shape = msgspec.structs.fields(Shape)
    position, size, angle, feather = (End(node=INPUTS, socket=field.name) for field in (position_shape, size_shape, rotation, feather_shape))

    def shaped(space: FrameSpace) -> BlenderResult[tuple[Fragment, End]]:
        offset = (
            vector("centre", "MULTIPLY", position, space.extent)
            .bind(lambda centre: vector("offset", "SUBTRACT", space.point, centre))
            .bind(lambda shifted: node("offset_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": shifted}, {}))
        )
        turn = paired(math("cosine", "COSINE", angle), math("sine", "SINE", angle))

        def rotated(parts: tuple[Fragment, tuple[tuple[Fragment, End], tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
            axes, (cosine, sine) = parts
            x, y = ended("offset_axes", "X")(axes), ended("offset_axes", "Y")(axes)
            return (
                paired(
                    paired(math("x_cosine", "MULTIPLY", x, cosine), math("y_sine", "MULTIPLY", y, sine)).bind(lambda terms: math("turned_x", "ADD", *terms)),
                    paired(math("y_cosine", "MULTIPLY", y, cosine), math("x_sine", "MULTIPLY", x, sine)).bind(lambda terms: math("turned_y", "SUBTRACT", *terms)),
                )
                .bind(lambda ends: node("turned", bpy.types.ShaderNodeCombineXYZ, {"X": ends[0], "Y": ends[1]}, {}))
                .map(ended("turned", "Vector"))
            )

        normalized = paired(offset, turn).bind(rotated).bind(lambda turned: vector("normalized", "DIVIDE", turned, size))

        def keyed(plane: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            box = (
                vector("absolute", "ABSOLUTE", plane)
                .bind(lambda absolute: node("absolute_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": absolute}, {}))
                .bind(lambda axes: math("maximum", "MAXIMUM", ended("absolute_axes", "X")(axes), ended("absolute_axes", "Y")(axes)))
                .bind(lambda maximum: math("box", "LESS_THAN", maximum, 1.0))
            )
            ellipse = vector("squared", "DOT_PRODUCT", plane, plane).bind(lambda squared: math("ellipse", "LESS_THAN", squared, 1.0))
            positive = (
                node("size_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": size}, {})
                .bind(lambda axes: math("minimum_size", "MINIMUM", ended("size_axes", "X")(axes), ended("size_axes", "Y")(axes)))
                .bind(lambda minimum: math("positive", "GREATER_THAN", minimum, 0.0))
            )
            return paired(
                paired(box, ellipse).bind(
                    lambda keys: menu("outline", "FLOAT", End(node=INPUTS, socket=outline.name), MenuItems.of(Outline), {Outline.BOX.label: keys[0], Outline.ELLIPSE.label: keys[1]})
                ),
                positive,
            ).bind(lambda ends: math("inside", "MULTIPLY", *ends))

        radius = node("feather_axes", bpy.types.ShaderNodeCombineXYZ, {"X": feather, "Y": feather}, {}).bind(lambda axes: pixels("feather", space, ended("feather_axes", "Vector")(axes), "VECTOR"))
        return (
            paired(normalized.bind(keyed), radius)
            .bind(lambda parts: node("blur", bpy.types.CompositorNodeBlur, {"Image": parts[0], "Size": parts[1], "Type": "Fast Gaussian"}, {}))
            .map(ended("blur", "Image"))
        )

    return _framed_key("shape", "Shape", Shape, shaped)


@functools.cache
def wipe() -> BlenderResult[NodeGroup]:
    """Returns corner-normalized wipe group, exact at both ends."""
    completion, angle, softness = (End(node=INPUTS, socket=name) for name in Transition.__struct_fields__)

    def traveled(space: FrameSpace) -> BlenderResult[tuple[Fragment, End]]:
        direction = (
            paired(math("cosine", "COSINE", angle), math("sine", "SINE", angle))
            .bind(lambda ends: node("direction", bpy.types.ShaderNodeCombineXYZ, {"X": ends[0], "Y": ends[1]}, {}))
            .map(ended("direction", "Vector"))
        )

        def projected(unit: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            corners = vector("corner", "MULTIPLY", space.extent, unit).bind(lambda corner: node("corner_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": corner}, {}))

            def ranged(axes: Fragment) -> BlenderResult[tuple[Fragment, End]]:
                x, y = ended("corner_axes", "X")(axes), ended("corner_axes", "Y")(axes)
                return (
                    accumulated(
                        paired(math(f"{name}_x", operation, x, 0.0), math(f"{name}_y", operation, y, 0.0)).bind(lambda ends: math(name, "ADD", *ends))
                        for name, operation in (("minimum", "MINIMUM"), ("maximum", "MAXIMUM"))
                    )
                    .bind(
                        lambda ends: paired(
                            vector("projection", "DOT_PRODUCT", space.point, unit).bind(lambda point: math("distance", "SUBTRACT", point, ends[0])), math("span", "SUBTRACT", ends[1], ends[0])
                        )
                    )
                    .bind(lambda ends: math("travel", "DIVIDE", *ends))
                )

            return corners.bind(ranged)

        def revealed(travel: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            soft = (
                math("reach", "ADD", softness, 1.0)
                .bind(lambda reach: math("progress", "MULTIPLY", completion, reach))
                .bind(lambda progress: math("ahead", "SUBTRACT", progress, travel))
                .bind(
                    lambda ahead: node(
                        "soft", bpy.types.ShaderNodeMapRange, {"Value": ahead, "From Min": 0.0, "From Max": softness, "To Min": 0.0, "To Max": 1.0}, {"interpolation_type": "LINEAR", "clamp": True}
                    )
                )
                .map(ended("soft", "Result"))
            )
            hard = math("beyond", "GREATER_THAN", travel, completion).bind(lambda beyond: math("hard", "SUBTRACT", 1.0, beyond))
            return (
                paired(soft, hard)
                .bind(lambda ends: math("softened", "GREATER_THAN", softness, 0.0).bind(lambda gate: switched("interior", "FLOAT", gate, ends[1], ends[0])))
                .bind(lambda interior: math("begun", "GREATER_THAN", completion, 0.0).bind(lambda begun: switched("opening", "FLOAT", begun, 0.0, interior)))
                .bind(lambda opening: math("unfinished", "LESS_THAN", completion, 1.0).bind(lambda unfinished: switched("key", "FLOAT", unfinished, 1.0, opening)))
            )

        return direction.bind(projected).bind(revealed)

    return _framed_key("wipe", "Wipe", Transition, traveled)


@functools.cache
def qualifier() -> BlenderResult[NodeGroup]:
    """Returns qualifier group over HSY hue, saturation, and log luminance."""
    entering = End(node=INPUTS, socket="image")
    forward = PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def qualified(groups: tuple[NodeGroup, NodeGroup, NodeGroup, NodeGroup]) -> BlenderResult[tuple[Fragment, End]]:
        coordinates, logarithm, feather, interval = groups

        def keyed(hsy_color: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
            channels = node("hsy_split", bpy.types.CompositorNodeSeparateColor, {"Image": hsy_color}, {"mode": "RGB"})
            stops = nested("logarithmic", logarithm, {COLOR_INPUT.key: hsy_color}, COLOR_RESULT.key).bind(
                lambda logarithmic_color: node("log_split", bpy.types.CompositorNodeSeparateColor, {"Image": logarithmic_color}, {"mode": "RGB"})
            )

            def ranged(parts: tuple[Fragment, Fragment]) -> BlenderResult[tuple[Fragment, End]]:
                split, logged = parts
                hue, saturation, luminance = ended("hsy_split", "Red")(split), ended("hsy_split", "Green")(split), ended("log_split", "Blue")(logged)
                hue_qualifier, saturation_qualifier, luminance_qualifier = Qualifier.__struct_fields__
                _, value_gated = Gated.__struct_fields__
                center, width, softness = HueRange.__struct_fields__
                hue_path = (hue_qualifier, value_gated)
                hue_key = (
                    math("hue_radians", "MULTIPLY", hue, tau)
                    .bind(lambda radians: math("hue_offset", "SUBTRACT", radians, End(node=INPUTS, socket=item_key(*hue_path, center))))
                    .bind(lambda offset: math("hue_absolute", "ABSOLUTE", offset))
                    .bind(lambda absolute: math("hue_wrapped", "FLOORED_MODULO", absolute, tau))
                    .bind(lambda wrapped: math("hue_halfturn", "SUBTRACT", wrapped, pi))
                    .bind(lambda halfturn: math("hue_fold", "ABSOLUTE", halfturn))
                    .bind(lambda folded: math("hue_distance", "SUBTRACT", pi, folded))
                )
                hue_band = (
                    paired(hue_key, math("hue_radius", "MULTIPLY", End(node=INPUTS, socket=item_key(*hue_path, width)), 0.5))
                    .bind(lambda ends: math("hue_inside", "SUBTRACT", ends[1], ends[0]))
                    .bind(lambda distance: nested("hue_edge", feather, {"distance": distance, "softness": End(node=INPUTS, socket=item_key(*hue_path, softness))}, "key"))
                )
                bands = accumulated(
                    nested(name, interval, {"value": value, **{field.name: End(node=INPUTS, socket=item_key(name, value_gated, field.name)) for field in msgspec.structs.fields(Band)}}, "key")
                    for name, value in ((saturation_qualifier, saturation), (luminance_qualifier, luminance))
                )
                return (
                    paired(hue_band, bands)
                    .bind(
                        lambda keys: accumulated(
                            switched(f"{name}_gate", "FLOAT", End(node=INPUTS, socket=name), 1.0, key)
                            for name, key in zip((hue_qualifier, saturation_qualifier, luminance_qualifier), (keys[0], *keys[1]))
                        )
                    )
                    .bind(lambda keys: math("hue_saturation", "MULTIPLY", keys[0], keys[1]).bind(lambda first: math("key", "MULTIPLY", first, keys[2])))
                )

            return paired(channels, stops).bind(ranged)

        return (
            node("straight", bpy.types.CompositorNodePremulKey, {"Image": entering, "Type": "To Straight"}, {})
            .map(ended("straight", "Image"))
            .bind(lambda straight: nested("hsy", coordinates, {COLOR_INPUT.key: straight}, COLOR_RESULT.key))
            .bind(keyed)
        )

    return paired(Interface.of(Qualifier), paired(paired(hsy(forward), logarithmic(forward)), paired(edge(), band())).bind(lambda groups: qualified((*groups[0], *groups[1])))).bind(
        lambda parts: NodeGroup.built(
            "qualifier",
            "Qualifier",
            msgspec.structs.replace(
                parts[0], inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor), *parts[0].inputs), outputs=(Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat),)
            ),
            {"key": parts[1]},
        )
    )


def matte(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Returns track matte group keying clamped alpha or associated working-space Y."""
    image = End(node=INPUTS, socket="image")
    channel, invert = msgspec.structs.fields(TrackMatte)
    return (
        paired(node("alpha", bpy.types.CompositorNodeSeparateColor, {"Image": image}, {"mode": "RGB"}).map(ended("alpha", "Alpha")), nested("luminance", working, {"image": image}, "luminance"))
        .bind(lambda ends: menu("channel", "FLOAT", End(node=INPUTS, socket=channel.name), MenuItems.of(MatteChannel), {MatteChannel.ALPHA.label: ends[0], MatteChannel.LUMINANCE.label: ends[1]}))
        .bind(lambda selected: node("clamped", bpy.types.ShaderNodeClamp, {"Value": selected, "Min": 0.0, "Max": 1.0}, {}))
        .map(ended("clamped", "Result"))
        .bind(lambda coverage: math("flip", "SUBTRACT", End(node=INPUTS, socket=invert.name), coverage))
        .bind(lambda flipped: math("key", "ABSOLUTE", flipped))
        .bind(
            lambda key: Interface.of(TrackMatte).bind(
                lambda face: NodeGroup.built(
                    "track_matte",
                    "Track Matte",
                    msgspec.structs.replace(
                        face, inputs=(Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor), *face.inputs), outputs=(Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat),)
                    ),
                    {"key": key},
                )
            )
        )
    )


def relayed(outer: Callable[[Key], Operand], interface: Interface) -> dict[Key, Operand]:
    """Returns operands for group input sockets and panel toggles."""

    def entered(item: DesiredItem) -> bool:
        match item:
            case InterfacePanel(toggle=toggle):
                return toggle.is_some()
            case (_, side):
                return side == "INPUT"

    return {key: outer(key) for key, item, _, _ in interface.walked() if entered(item)}


@functools.cache
def joined(*, opening: bool) -> BlenderResult[NodeGroup]:
    """Returns mask join group, opening mask joining onto operation identity."""
    key_mask, operation_mask, invert = msgspec.structs.fields(Mask)
    heads = (
        Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),
        Socket(key="aspect", name="Aspect", kind=bpy.types.NodeSocketFloat),
        Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat),
        *((Socket(key="running", name="Running", kind=bpy.types.NodeSocketFloat),) if not opening else ()),
    )
    shared = frozenset(socket.key for socket in heads)
    cases: tuple[type[Case], ...] = (Shape, Qualifier, Transition)
    joins = {MaskOperation.ADD: ("MAXIMUM", False, 0.0), MaskOperation.SUBTRACT: ("SUBTRACT", True, 1.0), MaskOperation.MULTIPLY: ("MULTIPLY", False, 1.0)}

    def keyed(groups: tuple[NodeGroup, ...]) -> BlenderResult[tuple[Fragment, End]]:
        return accumulated(
            nested(item_key(case), group, relayed(lambda key: End(node=INPUTS, socket=key if key in shared else item_key(key_mask.name, case, key)), group.interface), "key")
            for case, group in zip(cases, groups)
        ).bind(lambda keys: menu("key", "FLOAT", End(node=INPUTS, socket=key_mask.name), MenuItems.of(cases), dict(zip((case.title for case in cases), keys))))

    def joining(key: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        return (
            math("flip", "SUBTRACT", End(node=INPUTS, socket=invert.name), key)
            .bind(lambda flipped: math("keyed", "ABSOLUTE", flipped))
            .bind(
                lambda keyed: accumulated(
                    math(f"join_{operation.name.lower()}", name, start if opening else End(node=INPUTS, socket="running"), keyed, clamp=clamp) for operation, (name, clamp, start) in joins.items()
                )
            )
            .bind(lambda ends: menu("join", "FLOAT", End(node=INPUTS, socket=operation_mask.name), MenuItems.of(MaskOperation), dict(zip((operation.label for operation in joins), ends))))
        )

    return paired(Interface.of(Mask), accumulated((shape(), qualifier(), wipe())).bind(keyed).bind(joining)).bind(
        lambda parts: NodeGroup.built(
            "first_mask" if opening else "mask",
            "First Mask" if opening else "Mask",
            msgspec.structs.replace(parts[0], inputs=(*heads, *parts[0].inputs), outputs=(Socket(key="coverage", name="Coverage", kind=bpy.types.NodeSocketFloat),)),
            {"coverage": parts[1]},
        )
    )


def masked(key: Key, count: int, entering: Operand, aspect: Operand, scale: Operand, outer: Callable[[Key], Operand]) -> BlenderResult[Option[tuple[Fragment, End]]]:
    """Returns chained mask coverage, Nothing at zero count."""

    def joined_at(index: int, group: NodeGroup, running: Option[tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        heads: dict[Key, Operand] = {"image": entering, "aspect": aspect, "scale": scale, **running.map(lambda pair: {"running": pair}).default_value({})}
        return nested(f"{key}_{index}", group, relayed(lambda inner: heads[inner] if inner in heads else outer(item_key(index, inner)), group.interface), "coverage")

    def chained(groups: tuple[NodeGroup, NodeGroup]) -> BlenderResult[Option[tuple[Fragment, End]]]:
        first, rest = groups
        return functools.reduce(lambda done, index: done.bind(lambda pair: joined_at(index, rest, Some(pair))), range(2, count + 1), joined_at(1, first, Nothing)).map(Some)

    return paired(joined(opening=True), joined(opening=False)).bind(chained) if count else Ok(Nothing)


def _framed_key(key: Key, name: str, form: type[msgspec.Struct], body: Callable[[FrameSpace], BlenderResult[tuple[Fragment, End]]]) -> BlenderResult[NodeGroup]:
    """Builds key group over displayed frame of its image.

    Args:
        key: Group key.
        name: Group name.
        form: Settings record with fields after image, aspect, and scale inputs.
        body: Key graph over image frame space.

    Returns:
        Group with one float `key` output, or accumulated interface, frame, or body faults.
    """
    image, aspect, scale = (End(node=INPUTS, socket=socket) for socket in ("image", "aspect", "scale"))
    return paired(Interface.of(form), framed("frame", image, aspect, scale).bind(body)).bind(
        lambda parts: NodeGroup.built(
            key,
            name,
            msgspec.structs.replace(
                parts[0],
                inputs=(
                    Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor),
                    Socket(key="aspect", name="Aspect", kind=bpy.types.NodeSocketFloat),
                    Socket(key="scale", name="Scale", kind=bpy.types.NodeSocketFloat),
                    *parts[0].inputs,
                ),
                outputs=(Socket(key="key", name="Key", kind=bpy.types.NodeSocketFloat),),
            ),
            {"key": parts[1]},
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["band", "edge", "joined", "luminance", "masked", "matte", "qualifier", "relayed", "shape", "wipe"]
