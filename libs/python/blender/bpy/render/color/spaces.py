"""Native working-space changes, compositor color coordinates, and OCIO installation."""

from collections.abc import Callable, Iterable
import functools
import math as numeric
from pathlib import Path
from typing import Annotated, ClassVar, Self

import bpy
from expression import Ok, Option, Some
import msgspec
import numpy as np
import PyOpenColorIO

from blender.bpy.commands.calls import called
from blender.bpy.commands.operators import Call, Done, Empty, Operator, Outcome, Properties, Unchanged
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, struct_at
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import compared, End, ended, Fragment, INPUTS, masked, math, mixed, node, NodeGroup, Operand
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.settings.properties import Choices
from blender.imaging.color.authoring import ConfigBuild, Formation
from blender.imaging.color.config import admitted, ConfigFile, opened
from blender.imaging.color.processors import lin_to_log, processor
from blender.imaging.effects.tone.film import Film
from blender.imaging.faults import accumulated, paired

# --- [CHOICES] --------------------------------------------------------------------------

working = Choices(
    lambda _identity, _owner, _context: (
        (name, name, space.getDescription())
        for space in PyOpenColorIO.GetCurrentConfig().getColorSpaces()
        if bpy.types.UILayout.enum_item_name(bpy.data.colorspace, "working_space", name := space.getName())
    )
)

# --- [MODELS] ---------------------------------------------------------------------------

COLOR_INPUT = Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor)
COLOR_RESULT = Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor)


class WorkingSpace(msgspec.Struct, frozen=True, kw_only=True):
    """File's new scene-linear space and stored-color conversion policy."""

    space: Annotated[str, msgspec.Meta(title="Working Space", description="Scene-linear space of shading and compositing"), working]
    convert: Annotated[bool, msgspec.Meta(title="Convert Colors", description="Convert stored colors into the working space")] = True


class FilmView(msgspec.Struct, frozen=True, kw_only=True):
    """Photographic formation named by its application."""

    name: str
    description: str
    film: Film

    @property
    def formation(self) -> BlenderResult[Formation]:
        """Developed native formation or its complete imaging failures."""
        return self.film.transform().map(lambda transform: Formation.of(self.name, self.description, transform, self.film.space)).map_error(BlenderImagingFault.lifted)


class ConfigInstall(msgspec.Struct, frozen=True, kw_only=True):
    """Application OCIO rows installed over Blender's bundled configuration."""

    build: ConfigBuild
    films: tuple[FilmView, ...] = ()
    key: ClassVar[Key] = "install_config"

    @property
    def merged(self) -> BlenderResult[ConfigBuild]:
        """All film formations accumulated into the application build."""
        return accumulated(view.formation for view in self.films).map(lambda formations: msgspec.structs.replace(self.build, formations=(*self.build.formations, *formations)))

    @property
    def base(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Bundled base configuration independent of a user installation."""
        return opened(ConfigFile(path=str(Path(bpy.utils.system_resource("DATAFILES", path="colormanagement"), "config.ocio"))), ()).map_error(BlenderImagingFault.lifted)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def staged(self, stage: Path) -> BlenderResult[PyOpenColorIO.Config]:
        """Stages the full build in the caller's resource scope."""
        return paired(self.base, self.merged).bind(lambda pair: pair[1].staged(pair[0], stage).map_error(BlenderImagingFault.lifted))

    def extended(self, formation: Formation) -> Self:
        """Returns an installation containing an additional formation."""
        return msgspec.structs.replace(self, build=msgspec.structs.replace(self.build, formations=(*self.build.formations, formation)))

    def written(self) -> BlenderResult[Path]:
        """Installs the complete configuration for Blender's next start."""
        return paired(self.base, self.merged).bind(lambda pair: pair[1].installed(pair[0], Path(bpy.utils.user_resource("DATAFILES", path="colormanagement"))).map_error(BlenderImagingFault.lifted))

    def installed(self, _call: Call[Empty]) -> BlenderResult[Done]:
        """Installs the build and reports its next-start activation."""
        return self.written().map(lambda path: Done(notes=(("INFO", f"Installed {path}, read at the next Blender start"),)))

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns the original operator declaration for central registration."""
        return Ok((Operator(key=self.key, title="Install Color Config", description="Installs the extension's formations for the next Blender start", form=Empty, run=self.installed, undo=False),))


# --- [OPERATIONS] -----------------------------------------------------------------------


def config_of(spaces: Iterable[str] = ()) -> BlenderResult[PyOpenColorIO.Config]:
    """Returns Blender's live OCIO configuration once it resolves the required roles and a caller's spaces, or the imaging fault naming each unresolved one."""
    return admitted(PyOpenColorIO.GetCurrentConfig(), spaces).map_error(BlenderImagingFault.lifted)


def changed(call: Call[WorkingSpace]) -> BlenderResult[Outcome]:
    """Changes native working space, then converts compositor effect color inputs the native conversion leaves, or answers the space already current."""
    source, record = bpy.data.colorspace.working_space, call.record
    if source == record.space:
        return Ok(Unchanged(reason=f"The working space is already {source}"))
    inputs = (
        tuple(
            (item, descriptor, np.asarray(descriptor.read(item), dtype=np.float32).copy())
            for scene in bpy.data.scenes
            for effect in scene.compositor_effects
            for properties in Option.of_optional(effect.properties).to_list()
            for inputs_prop in properties.bl_rna.properties
            if isinstance(inputs_prop, bpy.types.PointerProperty) and inputs_prop.identifier == "inputs"
            for group in struct_at(properties, inputs_prop.identifier).to_list()
            for sockets in Option.of_optional(inputs_prop.fixed_type).to_list()
            for prop in sockets.properties
            if isinstance(prop, bpy.types.PointerProperty)
            for item in struct_at(group, prop.identifier).to_list()
            for socket in Option.of_optional(prop.fixed_type).to_list()
            for color in socket.properties
            if isinstance(color, bpy.types.FloatProperty) and color.identifier == "value" and color.subtype == "COLOR"
            for descriptor in (Descriptor(root=type(item), owner="", prop=color),)
        )
        if record.convert
        else ()
    )

    def recolored(cpu: PyOpenColorIO.CPUProcessor) -> BlenderResult[int]:
        for _item, _descriptor, values in inputs:
            cpu.applyRGB(values[:3])
        return accumulated(descriptor.write(item, tuple(values.tolist())) for item, descriptor, values in inputs).map(lambda priors: sum(prior.is_some() for prior in priors))

    return (
        config_of((source, record.space))
        .bind(
            lambda config: processor(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=record.space)).map(PyOpenColorIO.Processor.getDefaultCPUProcessor).map_error(BlenderImagingFault.lifted)
        )
        .bind(lambda held: called(bpy.ops.wm.set_working_color_space, {"working_space": record.space, "convert_colors": record.convert}).bind(lambda _: recolored(held)))
        .map(lambda count: Done(notes=(("INFO", f"Working space {record.space}, {count} effect colors converted"),)))
    )


def space_item(config: PyOpenColorIO.Config, name: str) -> BlenderResult[str]:
    """Returns the native conversion-node item of a role or colorspace."""
    return (
        Ok(name)
        if name == PyOpenColorIO.ROLE_SCENE_LINEAR
        else Option.of_optional(config.getColorSpace(name)).map(PyOpenColorIO.ColorSpace.getName).to_result_with(lambda: (BlenderAbsentFault(owner=config.getName(), key=name),))
    )


def converted(key: Key, config: PyOpenColorIO.Config, source: str, target: str, entering: Operand) -> BlenderResult[tuple[Fragment, End]]:
    """Converts straight RGB through native colorspaces with the scene-linear role kept live."""
    return (
        paired(space_item(config, source), space_item(config, target))
        .bind(lambda pair: node(key, bpy.types.CompositorNodeConvertColorSpace, {"Image": entering}, {"from_color_space": pair[0], "to_color_space": pair[1]}))
        .map(ended(key, "Image"))
    )


def pointwise(key: Key, entering: Operand, operation: Callable[[tuple[Fragment, End]], BlenderResult[tuple[Fragment, End]]]) -> BlenderResult[tuple[Fragment, End]]:
    """Applies a straight-color graph to associated RGB, retaining transformed RGB at zero alpha."""
    alpha = node(f"{key}_alpha", bpy.types.CompositorNodeSeparateColor, {"Image": entering}, {"mode": "RGB"}).map(ended(f"{key}_alpha", "Alpha"))
    straight = node(f"{key}_straight", bpy.types.CompositorNodePremulKey, {"Image": entering, "Type": "To Straight"}, {}).map(ended(f"{key}_straight", "Image"))

    def associated(inputs: tuple[tuple[Fragment, End], tuple[Fragment, End]]) -> BlenderResult[tuple[Fragment, End]]:
        alpha, straight = inputs
        return (
            operation(straight)
            .bind(lambda adjusted: node(f"{key}_restored", bpy.types.CompositorNodeSetAlpha, {"Image": adjusted, "Alpha": alpha, "Type": "Replace Alpha"}, {}).map(ended(f"{key}_restored", "Image")))
            .bind(
                lambda restored: paired(
                    node(f"{key}_associated", bpy.types.CompositorNodePremulKey, {"Image": restored, "Type": "To Premultiplied"}, {}).map(ended(f"{key}_associated", "Image")),
                    compared(f"{key}_transparent", "EQUAL", alpha, 0.0, epsilon=Some(0.0)),
                ).bind(lambda branches: masked(key, "RGBA", branches[1], branches[0], restored))
            )
        )

    return paired(alpha, straight).bind(associated)


def _converter(key: Key, name: str, body: Callable[[End, End, End], BlenderResult[tuple[Operand, ...]]]) -> BlenderResult[NodeGroup]:
    """Converts the caller's channels while carrying native alpha unchanged."""
    sockets = ("Red", "Green", "Blue")
    return (
        node("split", bpy.types.CompositorNodeSeparateColor, {"Image": End(node=INPUTS, socket=COLOR_INPUT.key)}, {"mode": "RGB"})
        .bind(
            lambda split: body(*(End(node="split", socket=socket) for socket in sockets)).bind(
                lambda channels: node("join", bpy.types.CompositorNodeCombineColor, dict(zip(sockets, channels)) | {"Alpha": ended("split", "Alpha")(split)}, {"mode": "RGB"})
            )
        )
        .bind(lambda joined: NodeGroup.built(key, name, Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: ended("join", "Image")(joined)}))
    )


@functools.cache
def logarithmic(direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO's channel stop coordinates from the native LogCamera parameters."""
    transform = lin_to_log()
    base, forward = transform.getBase(), direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def channel(value: End, log_slope: float, log_offset: float, lin_slope: float, lin_offset: float, lin_break: float, linear_slope: float) -> BlenderResult[tuple[Fragment, End]]:
        key = value.socket.lower()
        log_break = log_slope * numeric.log(lin_slope * lin_break + lin_offset, base) + log_offset
        linear_offset = log_break - linear_slope * lin_break
        sides = (
            (
                math(f"{key}_toe", "LESS_THAN", value, lin_break),
                math(f"{key}_scaled", "MULTIPLY_ADD", value, lin_slope, lin_offset)
                .bind(lambda scaled: math(f"{key}_logarithm", "LOGARITHM", scaled, base))
                .bind(lambda log: math(f"{key}_log", "MULTIPLY_ADD", log, log_slope, log_offset)),
                math(f"{key}_linear", "MULTIPLY_ADD", value, linear_slope, linear_offset),
            )
            if forward
            else (
                math(f"{key}_toe", "LESS_THAN", value, log_break),
                math(f"{key}_scaled", "MULTIPLY_ADD", value, 1.0 / log_slope, -log_offset / log_slope)
                .bind(lambda scaled: math(f"{key}_power", "POWER", base, scaled))
                .bind(lambda power: math(f"{key}_log", "MULTIPLY_ADD", power, 1.0 / lin_slope, -lin_offset / lin_slope)),
                math(f"{key}_linear", "MULTIPLY_ADD", value, 1.0 / linear_slope, -linear_offset / linear_slope),
            )
        )
        return accumulated(sides).bind(lambda ends: masked(key, "FLOAT", ends[0], ends[1], ends[2]))

    parameters = (
        transform.getLogSideSlopeValue(),
        transform.getLogSideOffsetValue(),
        transform.getLinSideSlopeValue(),
        transform.getLinSideOffsetValue(),
        transform.getLinSideBreakValue(),
        transform.getLinearSlopeValue(),
    )
    return _converter("lin_to_log" if forward else "log_to_lin", "Lin to Log" if forward else "Log to Lin", lambda *channels: accumulated(map(channel, channels, *parameters)))


@functools.cache
def hsy(direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO's native linear HSY coordinates as compositor math nodes."""
    names, weights = ("red", "green", "blue"), (0.2126, 0.7152, 0.0722)
    maximum_luma, minimum_luma = 0.01, 0.001
    forward = direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def body(first: End, second: End, third: End) -> BlenderResult[tuple[Operand, ...]]:
        steps: list[BlenderResult[tuple[Fragment, End]]] = []
        ends: tuple[End, ...]

        def calculated(key: Key, operation: str, *values: Operand, clamp: bool = False) -> End:
            steps.append(math(key, operation, *values, clamp=clamp))
            return End(node=key)

        def chosen(key: Key, factor: Operand, left: Operand, right: Operand) -> End:
            steps.append(masked(key, "FLOAT", factor, left, right))
            return End(node=key, socket="Value")

        def sum_of(key: Key, values: tuple[Operand, ...]) -> End:
            return calculated(key, "ADD", calculated(f"{key}_pair", "ADD", values[0], values[1]), values[2])

        def luminance(values: tuple[Operand, ...]) -> End:
            return sum_of("luma" if forward else "unit_luma", tuple(calculated(f"{name}_weighted", "MULTIPLY", value, weight) for name, value, weight in zip(names, values, weights)))

        if forward:
            channels = first, second, third
            luma = luminance(channels)
            distance = sum_of("distance", tuple(calculated(f"{name}_distance", "ABSOLUTE", calculated(f"{name}_offset", "SUBTRACT", value, luma)) for name, value in zip(names, channels)))
            total = sum_of("total", channels)
            high = calculated("high", "MAXIMUM", first, calculated("high_pair", "MAXIMUM", second, third))
            low = calculated("low", "MINIMUM", first, calculated("low_pair", "MINIMUM", second, third))
            delta = calculated("delta", "SUBTRACT", high, low)
            sectors = tuple(
                calculated(
                    f"{name}_sector",
                    "ADD",
                    calculated(f"{name}_ratio", "DIVIDE", calculated(f"{name}_span", "SUBTRACT", channels[(index + 1) % 3], channels[(index + 2) % 3]), delta),
                    float(2 * index + 1),
                )
                for index, name in enumerate(names)
            )
            sector = chosen(
                "peak_sector", calculated("red_below", "LESS_THAN", first, high), sectors[0], chosen("green_blue_sector", calculated("green_below", "LESS_THAN", second, high), sectors[1], sectors[2])
            )
            hue = calculated("hue", "DIVIDE", chosen("sector", calculated("chromatic", "LESS_THAN", low, high), 0.0, sector), 6.0)
            low_saturation = calculated("saturation_low", "MULTIPLY", distance, 5.0)
            high_saturation = calculated(
                "saturation_high",
                "DIVIDE",
                distance,
                calculated("high_denominator", "MAXIMUM", calculated("high_floor", "MULTIPLY_ADD", distance, 0.07, 1e-6), calculated("high_base", "ADD", total, 0.15)),
            )
            blend = calculated("blend", "MULTIPLY_ADD", luma, 1.0 / (maximum_luma - minimum_luma), -minimum_luma / (maximum_luma - minimum_luma), clamp=True)
            steps.append(mixed("saturation_mixed", "FLOAT", blend, low_saturation, high_saturation))
            saturation = calculated("saturation", "MULTIPLY", End(node="saturation_mixed", socket="Result_Float"), 1.4)
            ends = hue, saturation, luma
        else:
            hue, saturation, luma = first, second, third
            rotated = calculated("turned", "MULTIPLY_ADD", calculated("negative", "LESS_THAN", luma, 0.0), 0.5, hue)
            sector = calculated("sector", "MULTIPLY", calculated("wrapped", "FRACT", calculated("shifted", "SUBTRACT", rotated, 1.0 / 6.0)), 6.0)
            unit = (
                calculated("red_unit", "SUBTRACT", calculated("red_reach", "ABSOLUTE", calculated("red_far", "SUBTRACT", sector, 3.0)), 1.0, clamp=True),
                *(
                    calculated(f"{name}_unit", "SUBTRACT", 2.0, calculated(f"{name}_reach", "ABSOLUTE", calculated(f"{name}_far", "SUBTRACT", sector, center)), clamp=True)
                    for name, center in zip(names[1:], (2.0, 4.0))
                ),
            )
            scale = calculated("scale", "DIVIDE", luma, luminance(unit))
            offsets = tuple(calculated(f"{name}_offset", "SUBTRACT", calculated(f"{name}_base", "MULTIPLY", value, scale), luma) for name, value in zip(names, unit))
            distance = sum_of("distance", tuple(calculated(f"{name}_distance", "ABSOLUTE", offset) for name, offset in zip(names, offsets)))
            total = sum_of("total", tuple(End(node=f"{name}_base") for name in names))
            chroma = calculated("chroma", "DIVIDE", saturation, 1.4)
            triple = calculated("luma_triple", "MULTIPLY", luma, 3.0)
            excess = calculated("excess", "SUBTRACT", total, triple)
            reach = calculated("reach", "ADD", triple, 0.15)
            chroma_excess = calculated("chroma_excess", "MULTIPLY", chroma, excess)
            chroma_reach = calculated("chroma_reach", "MULTIPLY", chroma, reach)
            high_gain = calculated(
                "gain_high",
                "MINIMUM",
                calculated("gain_ratio", "DIVIDE", chroma_reach, calculated("remainder_floor", "MAXIMUM", calculated("remainder", "SUBTRACT", distance, chroma_excess), 1e-6)),
                50.0,
            )
            low_distance = calculated("distance_low", "MULTIPLY", distance, 5.0)
            low_gain = calculated("gain_low", "DIVIDE", chroma, calculated("distance_floor", "MAXIMUM", low_distance, 1e-10))
            blend = calculated("blend", "MULTIPLY_ADD", luma, 1.0 / (maximum_luma - minimum_luma), -minimum_luma / (maximum_luma - minimum_luma), clamp=True)
            low_weight = calculated("weight_low", "MULTIPLY", low_distance, calculated("shade", "SUBTRACT", 1.0, blend))
            a = calculated("quadratic_a", "MULTIPLY", low_weight, excess)
            minus_b = calculated("negated_b", "SUBTRACT", chroma_excess, calculated("lit_distance", "MULTIPLY_ADD", distance, blend, calculated("weight_reach", "MULTIPLY", low_weight, reach)))
            discriminant = calculated(
                "discriminant",
                "SQRT",
                calculated("discriminant_square", "MULTIPLY_ADD", calculated("a_reach", "MULTIPLY", a, chroma_reach), 4.0, calculated("b_squared", "MULTIPLY", minus_b, minus_b)),
            )
            denominator = calculated("denominator", "SUBTRACT", minus_b, discriminant)
            twice_c = calculated("twice_c", "MULTIPLY", chroma_reach, -2.0)
            near = calculated("root_near", "DIVIDE", twice_c, denominator)
            far = calculated("root_far", "DIVIDE", twice_c, calculated("denominator_far", "MULTIPLY_ADD", discriminant, 2.0, denominator))
            middle = chosen("gain_mid", calculated("root_negative", "LESS_THAN", near, 0.0), near, far)
            shadow = chosen("gain_shadow", calculated("lifted", "GREATER_THAN", blend, 0.0), low_gain, middle)
            gain = chosen("gain", calculated("unlit", "LESS_THAN", blend, 1.0), high_gain, shadow)
            ends = tuple(calculated(name, "MULTIPLY_ADD", offset, gain, luma) for name, offset in zip(names, offsets))
        return Fragment.joined(*steps).map(lambda fragment: tuple((fragment, end) for end in ends))

    return _converter("rgb_to_hsy" if forward else "hsy_to_rgb", "RGB to HSY" if forward else "HSY to RGB", body)


def configured() -> BlenderResult[Setting]:
    """Returns the native condition that a file's OCIO configuration is present."""
    return Descriptor.resolve(bpy.types.BlendData, "colorspace.is_missing_opencolorio_config", bpy.types.BoolProperty).map(lambda descriptor: Setting(descriptor=descriptor, accepted=(False,)))


# --- [COMPOSITION] ----------------------------------------------------------------------


change = Operator(
    key="working_space",
    title="Set Working Space",
    description="Changes scene-linear working space and converts stored colors",
    form=WorkingSpace,
    run=changed,
    invoke=Some(Properties()),
    seed=Some(lambda record, _event: msgspec.structs.replace(record, space=bpy.data.colorspace.working_space)),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLOR_INPUT",
    "COLOR_RESULT",
    "ConfigInstall",
    "FilmView",
    "WorkingSpace",
    "change",
    "changed",
    "config_of",
    "configured",
    "converted",
    "hsy",
    "logarithmic",
    "pointwise",
    "space_item",
    "working",
]
