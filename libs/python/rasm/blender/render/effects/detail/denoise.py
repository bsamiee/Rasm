# mypy: disable-error-code="attr-defined,union-attr,misc"
# ty: ignore[unresolved-attribute,invalid-attribute-access]

"""Spatial denoiser units and motion-aligned temporal reconstruction."""

from collections.abc import Callable
import functools
from itertools import starmap
from operator import or_
from typing import ClassVar, Self

import bpy
from expression import Nothing, Option, Some
import msgspec
from msgspec import inspect, structs

from rasm.blender.data.ids.images import ImageFrames, Tag
from rasm.blender.data.ids.ownership import Ownership, Saved
from rasm.blender.data.rna.conditions import Setting
from rasm.blender.data.rna.descriptors import Descriptor
from rasm.blender.extension.identity import Key
from rasm.blender.faults import BlenderResult
from rasm.blender.nodes.graphs import (
    compared,
    End,
    ended,
    Fragment,
    gridded,
    INPUTS,
    Interface,
    item_key,
    Loaded,
    masked,
    math,
    menu,
    MenuItems,
    mixed,
    nested,
    node,
    NodeGroup,
    Operand,
    PixelGrid,
    Socket,
    switched,
    vector,
)
from rasm.blender.render.color.spaces import COLOR_INPUT, COLOR_RESULT
from rasm.blender.render.effects.compositing.masks import luminance
from rasm.blender.render.effects.stack import CompositorEffect, KeyPath, Shell
from rasm.blender.render.effects.viewers import inspection
from rasm.blender.render.outputs.formats import Kind
from rasm.blender.render.view_layers.passes import Family, Filing, PassRequest, Source
from rasm.imaging.compositing.recombination import Terms
from rasm.imaging.effects.detail.denoise import Filter, Frame, Groups, Guides, Passes, SpatialDenoise, TemporalDenoise
from rasm.imaging.faults import accumulated, paired
from rasm.imaging.parameters import Gated
from rasm.rendering.outputs.naming import Output

# --- [CONSTANTS] ------------------------------------------------------------------------

_FRAME_BASE = End(node="frame_base", socket="Output")


# --- [MODELS] ---------------------------------------------------------------------------


class Pair(msgspec.Struct, frozen=True, kw_only=True):
    """Albedo and normal guide ends of one denoised image."""

    albedo: End
    normal: End

    @classmethod
    def of(cls, layer: Callable[[str], End], family: Option[Family]) -> Self:
        """Uses a family's color pass when present, and Cycles' guides otherwise."""
        albedo = family.filter(lambda held: bool(held.color)).map(lambda held: held.socket("color"))
        return cls(albedo=layer(albedo.default_value("Denoising Albedo")), normal=layer("Denoising Normal"))


class Gate(msgspec.Struct, frozen=True, kw_only=True):
    """Enable and amount operands of a denoised term."""

    enabled: Operand
    amount: Operand


class Filters(msgspec.Struct, frozen=True, kw_only=True):
    """Native parameter paths of every spatial filter and its target."""

    target: KeyPath
    fireflies: KeyPath
    frame: KeyPath
    terms: Terms[KeyPath]
    occlusion: KeyPath
    groups: KeyPath

    @classmethod
    def of(cls) -> Self:
        """Derives paths from the record's native slot and term metadata."""
        target = (SpatialDenoise.target.__name__,)
        return cls(
            target=target,
            fireflies=(SpatialDenoise.fireflies.__name__,),
            frame=(*target, Frame, Frame.filter.__name__),
            terms=Terms(**{field.name: (*target, Passes, Passes.terms.__name__, field.name, Gated.value.__name__) for field in structs.fields(Terms[Gated[Filter]])}),
            occlusion=(*target, Passes, Passes.occlusion.__name__, Gated.value.__name__),
            groups=(*target, Groups, Groups.filter.__name__),
        )

    @property
    def filters(self) -> tuple[KeyPath, ...]:
        """Frame path followed by every term, occlusion, and group path."""
        return self.frame, *structs.astuple(self.terms), self.occlusion, self.groups


class Plane(msgspec.Struct, frozen=True, kw_only=True):
    """Written sequence path and the image output holding its plane."""

    path: str
    name: str


class Rendered(msgspec.Struct, frozen=True, kw_only=True):
    """Written color and pixel-motion sequences over one inclusive frame range."""

    color: Plane
    motion: Plane
    first: int
    last: int
    ownership: ClassVar[Ownership[bpy.types.CompositorNodeTree]] = Ownership(kind=bpy.types.CompositorNodeTree, key=item_key(TemporalDenoise), saved=Saved.USED)

    def group(self, record: TemporalDenoise) -> BlenderResult[NodeGroup]:
        """Builds temporal reconstruction from current, previous, and next sequence frames.

        Args:
            record: Color thresholds, reverse-motion tolerance, and neighbor weight bound.

        Returns:
            The compositor-only sequence group, or accumulated native graph faults.
        """

        def loaded(key: Key, plane: Plane, offset: int) -> BlenderResult[tuple[Fragment, End]]:
            frames = ImageFrames(frame_start=self.first, frame_duration=self.last - self.first + 1, frame_offset=self.first - 1 + offset)
            return node(key, bpy.types.CompositorNodeImage, {}, {}, Some(Loaded(path=plane.path, tag=Tag(sequence=True), frames=Some(frames)))).map(ended(key, plane.name))

        color = End(node="current_color", socket=self.color.name)
        current_hue = End(node="current_hsv", socket="Red")
        current_value = End(node="current_hsv", socket="Blue")

        def side(grid: PixelGrid, key: Key, offset: int, channels: tuple[str, str], reverse: tuple[str, str], scale: Operand, comparison: str, bound: int) -> BlenderResult[Fragment]:
            uv = End(node=f"{key}_uv", socket="Vector")
            warped = End(node=f"{key}_warped", socket="Image")
            hue, saturation, value = (End(node=f"{key}_hsv", socket=channel) for channel in ("Red", "Green", "Blue"))
            hue_spread = End(node=f"{key}_hue_spread")
            current_motion = {channel: End(node="motion", socket=channel) for channel in channels}
            within = End(node=f"{key}_inside")
            extensions = {"Extension X": "Clip", "Extension Y": "Clip"}
            return Fragment.joined(
                loaded(f"{key}_color", self.color, offset),
                loaded(f"{key}_motion", self.motion, offset),
                node(f"{key}_offset", bpy.types.ShaderNodeCombineXYZ, dict(zip(("X", "Y"), current_motion.values())), {}),
                grid.at(f"{key}_uv", End(node=f"{key}_offset", socket="Vector"), scale),
                node(f"{key}_warped", bpy.types.CompositorNodeMapUV, {"Image": End(node=f"{key}_color", socket=self.color.name), "UV": uv, "Interpolation": "Bilinear", **extensions}, {}),
                node(f"{key}_moved", bpy.types.CompositorNodeMapUV, {"Image": End(node=f"{key}_motion", socket=self.motion.name), "UV": uv, "Interpolation": "Nearest", **extensions}, {}),
                node(f"{key}_hsv", bpy.types.CompositorNodeSeparateColor, {"Image": warped}, {"mode": "HSV"}),
                node(f"{key}_vector", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=f"{key}_moved", socket="Image")}, {"mode": "RGB"}),
                *(
                    Fragment.joined(
                        math(f"{key}_drift_{axis}", "SUBTRACT", End(node=f"{key}_vector", socket=back), toward),
                        math(f"{key}_spread_{axis}", "ABSOLUTE", End(node=f"{key}_drift_{axis}")),
                        compared(f"{key}_agree_{axis}", "LESS_EQUAL", End(node=f"{key}_spread_{axis}"), record.tolerance),
                    )
                    for axis, back, toward in zip(("x", "y"), reverse, current_motion.values())
                ),
                math(f"{key}_agree", "MULTIPLY", End(node=f"{key}_agree_x", socket="Result"), End(node=f"{key}_agree_y", socket="Result")),
                math(f"{key}_hue_gap", "SUBTRACT", current_hue, hue),
                math(f"{key}_hue_spread", "ABSOLUTE", End(node=f"{key}_hue_gap")),
                math(f"{key}_hue_wrap", "SUBTRACT", 1.0, hue_spread),
                math(f"{key}_hue", "MINIMUM", hue_spread, End(node=f"{key}_hue_wrap")),
                math(f"{key}_chroma", "MULTIPLY", saturation, value),
                math(f"{key}_saturation_gap", "SUBTRACT", End(node="current_chroma"), End(node=f"{key}_chroma")),
                math(f"{key}_saturation", "ABSOLUTE", End(node=f"{key}_saturation_gap")),
                math(f"{key}_value_gap", "SUBTRACT", current_value, value),
                math(f"{key}_value", "ABSOLUTE", End(node=f"{key}_value_gap")),
                *(
                    Fragment.joined(
                        math(f"{key}_{channel}_room", "SUBTRACT", threshold, End(node=f"{key}_{channel}")),
                        math(f"{key}_{channel}_weight", "DIVIDE", End(node=f"{key}_{channel}_room"), threshold, clamp=True),
                    )
                    for channel, threshold in (("hue", record.hue), ("saturation", record.saturation), ("value", record.value))
                ),
                math(f"{key}_like", "MULTIPLY", End(node=f"{key}_hue_weight"), End(node=f"{key}_saturation_weight")),
                math(f"{key}_alike", "MULTIPLY", End(node=f"{key}_like"), End(node=f"{key}_value_weight")),
                masked(f"{key}_kept", "FLOAT", End(node=f"{key}_agree"), 0.0, End(node=f"{key}_alike")),
                math(f"{key}_maximum", "MULTIPLY", End(node=f"{key}_kept"), record.max_weight),
                math(f"{key}_inside", comparison, End(node="time", socket="Frame"), float(bound)),
                switched(f"{key}_weight", "FLOAT", within, 0.0, End(node=f"{key}_maximum")),
                switched(f"{key}_image", "RGBA", within, color, warped),
            )

        def built(grid: PixelGrid) -> BlenderResult[NodeGroup]:
            next_weight, total = End(node="next_weight", socket="Output"), End(node="total")
            return Fragment.joined(
                loaded("current_color", self.color, 0),
                loaded("current_motion", self.motion, 0),
                node("motion", bpy.types.CompositorNodeSeparateColor, {"Image": End(node="current_motion", socket=self.motion.name)}, {"mode": "RGB"}),
                node("current_hsv", bpy.types.CompositorNodeSeparateColor, {"Image": color}, {"mode": "HSV"}),
                math("current_chroma", "MULTIPLY", End(node="current_hsv", socket="Green"), current_value),
                vector("backward", "MULTIPLY", grid.inverse, (-1.0, -1.0, -1.0)),
                node("time", bpy.types.CompositorNodeSceneTime, {}, {}),
                side(grid, "previous", -1, ("Red", "Green"), ("Blue", "Alpha"), grid.inverse, "GREATER_THAN", self.first),
                side(grid, "next", 1, ("Blue", "Alpha"), ("Red", "Green"), End(node="backward", socket="Vector"), "LESS_THAN", self.last),
                math("total", "ADD", End(node="previous_weight", socket="Output"), next_weight),
                math("share", "DIVIDE", next_weight, total),
                mixed("neighbours", "RGBA", End(node="share"), End(node="previous_image", socket="Output"), End(node="next_image", socket="Output")),
                mixed("blend", "RGBA", total, color, End(node="neighbours", socket="Result_Color")),
                node("restored", bpy.types.CompositorNodeSetAlpha, {"Image": End(node="blend", socket="Result_Color"), "Alpha": End(node="current_hsv", socket="Alpha"), "Type": "Replace Alpha"}, {}),
            ).bind(
                lambda body: NodeGroup.built(
                    key=self.ownership.key, name=TemporalDenoise.title, interface=Interface(outputs=(COLOR_RESULT,)), outputs={COLOR_RESULT.key: (body, End(node="restored", socket="Image"))}
                )
            )

        return gridded("grid", color).bind(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def guided(layer: str) -> BlenderResult[PassRequest]:
    """Requests Cycles' denoising guides on a view layer."""

    def requested(flag: Descriptor[bpy.types.BoolProperty], engine: Descriptor[bpy.types.EnumProperty]) -> PassRequest:
        return PassRequest(layer=layer, flags=frozenset({flag}), condition=Some(Setting(descriptor=engine, accepted=("CYCLES",))))

    return paired(Descriptor.resolve(bpy.types.ViewLayer, "cycles.denoising_store_passes", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.Scene, "render.engine", bpy.types.EnumProperty)).map(
        lambda descriptors: requested(*descriptors)
    )


def motioned(layer: str) -> BlenderResult[tuple[PassRequest, Setting]]:
    """Requests pixel motion and disables motion blur for the sequence render."""

    def requested(flag: Descriptor[bpy.types.BoolProperty], blur: Descriptor[bpy.types.BoolProperty]) -> tuple[PassRequest, Setting]:
        return PassRequest(layer=layer, flags=frozenset({flag})), Setting(descriptor=blur, accepted=(False,))

    return paired(Descriptor.resolve(bpy.types.ViewLayer, "use_pass_vector", bpy.types.BoolProperty), Descriptor.resolve(bpy.types.Scene, "render.use_motion_blur", bpy.types.BoolProperty)).map(
        lambda descriptors: requested(*descriptors)
    )


def settings() -> BlenderResult[tuple[Descriptor, ...]]:
    """Resolves the native compositor denoise device and quality settings."""
    return accumulated(
        Descriptor.resolve(bpy.types.Scene, f"render.{member}", bpy.types.EnumProperty)
        for member in ("compositor_denoise_device", "compositor_denoise_preview_quality", "compositor_denoise_final_quality")
    )


def denoiser(working: NodeGroup) -> BlenderResult[NodeGroup]:
    """Builds the shared firefly suppression and HDR denoise group.

    Args:
        working: Working-space luminance group with image input and luminance output.

    Returns:
        The unit group with filtered and noisy outputs, or accumulated graph faults.
    """
    image = End(node=INPUTS, socket=COLOR_INPUT.key)
    fireflies = item_key(SpatialDenoise.fireflies.__name__)
    guides = next(field for field in structs.fields(Filter) if field.name == Filter.guides.__name__)
    joint = Guides.ALBEDO_NORMAL.label
    ring = tuple((float(dx), float(dy), 0.0) for dy in range(-2, 3) for dx in range(-2, 3) if max(abs(dx), abs(dy)) == 2)
    rank = 4

    def ranked(values: tuple[tuple[Fragment, End, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
        key = "rank"
        count = len(values)
        selected = count - rank
        pairs = tuple(
            (i + j, i + j + k)
            for p in (1 << exponent for exponent in range(count.bit_length() - 1))
            for k in (p >> shift for shift in range(p.bit_length()))
            for j in range(k % p, count - k, 2 * k)
            for i in range(min(k, count - j - k))
            if (i + j) // (2 * p) == (i + j + k) // (2 * p)
        )

        def marked(state: tuple[frozenset[int], frozenset[int]], indexed: tuple[int, tuple[int, int]]) -> tuple[frozenset[int], frozenset[int]]:
            (read, kept), (index, wires) = state, indexed
            return (read | frozenset(wires), kept | {index}) if read.intersection(wires) else state

        def connected(
            state: tuple[tuple[tuple[End, End], ...], tuple[tuple[int, tuple[End, End], tuple[End, End]], ...]], indexed: tuple[int, tuple[int, int]]
        ) -> tuple[tuple[tuple[End, End], ...], tuple[tuple[int, tuple[End, End], tuple[End, End]], ...]]:
            (wires, made), (index, (low, high)) = state, indexed
            moved = {wire: (End(node=f"{key}_{name}_{index}"), End(node=f"{key}_{name}_valid_{index}")) for wire, name in ((low, "minimum"), (high, "maximum"))}
            return tuple(starmap(moved.get, enumerate(wires))), (*made, (index, wires[low], wires[high]))

        def comparator(index: int, left: tuple[End, End], right: tuple[End, End]) -> BlenderResult[Fragment]:
            left_value, left_valid = left
            right_value, right_valid = right
            return (
                paired(
                    math(f"{key}_valid_greater_{index}", "GREATER_THAN", left_valid, right_valid),
                    paired(
                        compared(f"{key}_valid_equal_{index}", "EQUAL", left_valid, right_valid, epsilon=Some(0.0)), math(f"{key}_value_greater_{index}", "GREATER_THAN", left_value, right_value)
                    ).bind(lambda parts: math(f"{key}_equal_greater_{index}", "MULTIPLY", *parts)),
                )
                .bind(lambda parts: math(f"{key}_choose_{index}", "ADD", *parts))
                .bind(
                    lambda choose: Fragment.joined(
                        masked(f"{key}_minimum_{index}", "FLOAT", choose, left_value, right_value),
                        masked(f"{key}_maximum_{index}", "FLOAT", choose, right_value, left_value),
                        math(f"{key}_minimum_valid_{index}", "MINIMUM", left_valid, right_valid),
                        math(f"{key}_maximum_valid_{index}", "MAXIMUM", left_valid, right_valid),
                    )
                )
            )

        initial: tuple[frozenset[int], frozenset[int]] = frozenset({selected}), frozenset()
        _, needed = functools.reduce(marked, reversed(tuple(enumerate(pairs))), initial)
        inputs = functools.reduce(or_, (body for body, _, _ in values), Fragment())
        ends = tuple((value, valid) for _, value, valid in values)
        made: tuple[tuple[int, tuple[End, End], tuple[End, End]], ...] = ()
        wires, made = functools.reduce(connected, enumerate(pairs), (ends, made))
        value, _ = wires[selected]
        return accumulated(comparator(index, low, high) for index, low, high in made if index in needed).map(lambda parts: (functools.reduce(or_, parts, inputs), value))

    def sample(grid: PixelGrid, luminance: Operand, index: int, offset: tuple[float, float, float]) -> BlenderResult[tuple[Fragment, End, End]]:
        key = f"ring_{index}"
        uv = End(node=f"{key}_uv", socket="Vector")
        axes = {axis: End(node=f"{key}_axes", socket=axis) for axis in ("X", "Y")}
        return Fragment.joined(
            grid.at(f"{key}_uv", offset, grid.inverse),
            node(key, bpy.types.CompositorNodeMapUV, {"Image": luminance, "UV": uv, "Interpolation": "Nearest", "Extension X": "Clip", "Extension Y": "Clip"}, {}),
            node(f"{key}_axes", bpy.types.ShaderNodeSeparateXYZ, {"Vector": uv}, {}),
            *(
                Fragment.joined(
                    math(f"{key}_{axis}_lower", "GREATER_THAN", end, 0.0),
                    math(f"{key}_{axis}_upper", "LESS_THAN", end, 1.0),
                    math(f"{key}_{axis}_inside", "MULTIPLY", End(node=f"{key}_{axis}_lower"), End(node=f"{key}_{axis}_upper")),
                )
                for axis, end in axes.items()
            ),
            math(f"{key}_valid", "MULTIPLY", *(End(node=f"{key}_{axis}_inside") for axis in axes)),
        ).map(lambda body: (body, End(node=key, socket="Image"), End(node=f"{key}_valid")))

    def limited(luminance: Operand, fourth: Operand, enough: Operand) -> BlenderResult[tuple[Fragment, End]]:
        return Fragment.joined(
            math("ratio", "DIVIDE", fourth, luminance),
            paired(
                math("limit", "MULTIPLY", fourth, End(node=INPUTS, socket=item_key(fireflies, Gated.value.__name__))).bind(lambda limit: math("spike", "GREATER_THAN", luminance, limit)),
                compared("lit", "GREATER_EQUAL", luminance, 0.0001),
            )
            .bind(lambda flags: math("positive_spike", "MULTIPLY", *flags))
            .bind(lambda spike: math("marked", "MULTIPLY", spike, enough)),
            masked("scale", "FLOAT", End(node="marked"), 1.0, End(node="ratio")),
        ).map(ended("scale", "Value"))

    def gathered(grid: PixelGrid, luminance: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        def scaled(samples: tuple[tuple[Fragment, End, End], ...]) -> BlenderResult[tuple[Fragment, End]]:
            count = Fragment.joined(*(math(f"count_{index}", "ADD", End(node=f"count_{index - 1}") if index else 0.0, valid) for index, (_, _, valid) in enumerate(samples))).map(
                ended(f"count_{len(samples) - 1}", "Value")
            )
            return paired(ranked(samples), count.bind(lambda total: math("enough", "GREATER_THAN", total, float(rank)))).bind(lambda found: limited(luminance, *found))

        return accumulated(sample(grid, luminance, index, offset) for index, offset in enumerate(ring)).bind(scaled)

    def cleaned(scale: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        routes = {
            "Image": mixed("despeckled", "RGBA", 1.0, image, scale, "MULTIPLY").bind(lambda despeckled: switched("fireflies", "RGBA", End(node=INPUTS, socket=fireflies), image, despeckled)),
            "Albedo": menu(
                "albedo",
                "RGBA",
                End(node=INPUTS, socket=guides.name),
                MenuItems.of(inspect.type_info(guides.type)),
                {Guides.ALBEDO.label: End(node=INPUTS, socket="albedo"), joint: End(node=INPUTS, socket="albedo")},
            ),
            "Normal": switched("normal", "VECTOR", End(node="albedo", socket=joint), (0.0, 0.0, 0.0), End(node=INPUTS, socket="normal")),
        }
        return (
            accumulated(routes.values())
            .bind(
                lambda ends: node(
                    "denoise",
                    bpy.types.CompositorNodeDenoise,
                    {**dict(zip(routes, ends)), "HDR": True, "Prefilter": End(node=INPUTS, socket=Filter.prefilter.__name__), "Quality": End(node=INPUTS, socket=Filter.quality.__name__)},
                    {},
                )
            )
            .map(ended("denoise", "Image"))
        )

    def assembled(parts: tuple[tuple[Interface, Interface], tuple[Fragment, End]]) -> BlenderResult[NodeGroup]:
        (filters, entry), result = parts
        body, _ = result
        return NodeGroup.built(
            key="denoiser",
            name="Denoiser",
            interface=Interface(
                outputs=(COLOR_RESULT, Socket(key="noisy", name="Filter Input", kind=bpy.types.NodeSocketColor)),
                inputs=(COLOR_INPUT, Socket(key="albedo", name="Albedo", kind=bpy.types.NodeSocketColor), Socket(key="normal", name="Normal", kind=bpy.types.NodeSocketVector), *filters.inputs),
                panels=tuple(panel for panel in entry.panels if panel.key == fireflies),
            ),
            outputs={COLOR_RESULT.key: result, "noisy": (body, End(node="fireflies", socket="Output"))},
        )

    return paired(
        paired(Interface.of(Filter), Interface.of(SpatialDenoise)),
        paired(gridded("grid", image), nested("luminance", working, {COLOR_INPUT.key: image}, "luminance")).bind(lambda found: gathered(*found)).bind(cleaned),
    ).bind(assembled)


def unit(key: Key, group: NodeGroup, path: KeyPath, socket: Callable[[Key], End], source: Operand, pair: Pair) -> BlenderResult[tuple[Fragment, End]]:
    """Builds one unit from its filter path, guide ends, and firefly controls."""
    fireflies = SpatialDenoise.fireflies.__name__
    controls = {
        **{field.name: socket(item_key(*path, field.name)) for field in structs.fields(Filter)},
        **{held: socket(held) for held in (item_key(fireflies), item_key(fireflies, Gated.value.__name__))},
    }
    return nested(key, group, {COLOR_INPUT.key: source, "albedo": pair.albedo, "normal": pair.normal, **controls}, COLOR_RESULT.key)


def gated(key: Key, source: Operand, filtered: tuple[Fragment, End], gate: Gate) -> BlenderResult[tuple[Fragment, End]]:
    """Blends enabled denoise against its firefly-cleaned input, retaining disabled source."""
    body, result = filtered
    return mixed(f"{key}_amount", "RGBA", gate.amount, (body, End(node=result.node, socket="noisy")), filtered).bind(lambda blended: switched(key, "RGBA", gate.enabled, source, blended))


def filing(terms: tuple[tuple[str, End], ...]) -> Filing:
    """Routes denoised terms to the layered beauty output."""
    return Filing(output=Output.BEAUTY, kind=Kind.LAYERED, sources=tuple(Source(name=f"{label} Denoised", end=end, kind="RGBA", dimensions=Nothing, output=Output.BEAUTY) for label, end in terms))


def denoised(shell: Shell[SpatialDenoise]) -> BlenderResult[tuple[Fragment, End]]:
    """Builds all filter units and the selected spatial target.

    Args:
        shell: Entry inputs, target menus, working config, and render-layer endpoints.

    Returns:
        The target image with its cleaned frame baseline, or accumulated graph faults.
    """
    paths, pair = Filters.of(), Pair.of(shell.layer, Nothing)

    def joined(parts: tuple[tuple[Fragment, End], Fragment]) -> tuple[Fragment, End]:
        (body, end), others = parts
        return body | others, end

    def targeted(frame: tuple[Fragment, End]) -> BlenderResult[tuple[Fragment, End]]:
        body, result = frame
        target = item_key(*paths.target)
        return menu(target, "RGBA", shell.socket(*paths.target), shell.items[paths.target], {Frame.title: frame, Passes.title: shell.source, Groups.title: shell.source}).bind(
            lambda selected: Fragment.joined(switched(_FRAME_BASE.node, "RGBA", End(node=target, socket=Frame.title), shell.source, (body, End(node=result.node, socket="noisy")))).map(
                lambda baseline: joined((selected, baseline))
            )
        )

    def built(group: NodeGroup) -> BlenderResult[tuple[Fragment, End]]:
        frame, *rest = (unit(item_key(*path), group, path, shell.socket, shell.source, pair) for path in paths.filters)
        return paired(frame.bind(targeted), Fragment.joined(*rest)).map(joined)

    return luminance(shell.config).bind(denoiser).bind(built)


# --- [COMPOSITION] ----------------------------------------------------------------------

spatial_denoise: CompositorEffect[SpatialDenoise] = CompositorEffect(
    record=SpatialDenoise,
    graph=denoised,
    mix_base=Some(_FRAME_BASE),
    requested=Some(lambda _record, layer: guided(layer)),
    holds={"cycles.use_denoising": False},
    registrables=(
        inspection(
            row=lambda: spatial_denoise,
            key=item_key(SpatialDenoise, "frame"),
            title="View Denoised Frame",
            description="Shows the frame filter output before the target selection and entry blend",
            target=lambda: End(node=item_key(*Filters.of().frame), socket=COLOR_RESULT.key),
        ),
    ),
)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Filters", "Gate", "Pair", "Plane", "Rendered", "denoised", "denoiser", "filing", "gated", "guided", "motioned", "settings", "spatial_denoise", "unit"]
