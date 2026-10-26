"""Working-space effect color follow-through, compositor color coordinates, and OCIO installation."""

from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager, ExitStack
import functools
from math import log
from pathlib import Path
from typing import ClassVar, Final, Self

import bpy
from expression import Nothing, Ok, Option, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.rna.conditions import Setting
from blender.bpy.data.rna.descriptors import Descriptor, Native, struct_at
from blender.bpy.extension.events.handlers import UpdateHandler
from blender.bpy.extension.events.timers import Startup
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderResult
from blender.bpy.nodes.graphs import Expr, INPUTS, math, mixed, NodeGroup, rgba, Term, where
from blender.bpy.nodes.interface import Interface, Socket
from blender.bpy.render.effects.records import inputs_of
from blender.bpy.settings.attachments import Stored
from blender.imaging.color.authoring import ConfigBuild, Formation
from blender.imaging.color.config import ConfigFile, opened, resolved
from blender.imaging.color.processors import lin_to_log, processor
from blender.imaging.effects.tone.film import Film
from blender.imaging.faults import accumulated, paired

# --- [CONSTANTS] ------------------------------------------------------------------------

COLOR_INPUT: Final = Socket(key="image", name="Image", kind=bpy.types.NodeSocketColor)
COLOR_RESULT: Final = Socket(key="result", name="Image", kind=bpy.types.NodeSocketColor)
CONFIGURED: Final = Setting(descriptor=Descriptor.declared(bpy.types.BlendData, "colorspace.is_missing_opencolorio_config", bpy.types.BoolProperty), accepted=(False,))
WEIGHTS: Final = (0.2126, 0.7152, 0.0722)
LUMA: Final = (0.001, 0.01)

# --- [MODELS] ---------------------------------------------------------------------------


class WorkingColors(msgspec.Struct, frozen=True, kw_only=True):
    """Effect color inputs native working space conversion skips, carried into each new working space from the space each scene last held them in."""

    stamp: ClassVar[Key] = "working_space"

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[BlenderResult[Acquisitions]]:
        """Holds depsgraph check converting effect colors in one undo step after a working space change, which publishes no property notification."""
        previous: Option[str] = Nothing

        def seeded() -> BlenderResult[None]:
            nonlocal previous
            previous = Some(bpy.data.colorspace.working_space)
            return Ok(None)

        def source(stamped: Stored[str], scene: bpy.types.Scene, target: str) -> BlenderResult[str]:
            return stamped.read(scene).map(lambda held: held.or_else(previous).default_value(target))

        def converted(stamped: Stored[str]) -> BlenderResult[Option[str]]:
            target = bpy.data.colorspace.working_space

            def scene_converted(scene: bpy.types.Scene, start: str) -> BlenderResult[int]:
                inputs = effect_colors(scene)

                def recolored(cpu: PyOpenColorIO.CPUProcessor) -> BlenderResult[int]:
                    for _item, _descriptor, values in inputs:
                        cpu.applyRGB(values[:3])
                    return (
                        accumulated(descriptor.write(item, tuple(values.tolist())) for item, descriptor, values in inputs)
                        .map(lambda priors: sum(prior.is_some() for prior in priors))
                        .bind(lambda count: stamped.write(scene, target).map(lambda _: count))
                    )

                return (
                    config_of((start, target))
                    .bind(lambda config: processor(config, PyOpenColorIO.ColorSpaceTransform(src=start, dst=target)).map(PyOpenColorIO.Processor.getDefaultCPUProcessor))
                    .bind(recolored)
                )

            def each(scene: bpy.types.Scene) -> BlenderResult[Option[int]]:
                return source(stamped, scene, target).bind(lambda start: Ok(Nothing) if start == target else scene_converted(scene, start).map(Some))

            counted = accumulated(each(scene) for scene in bpy.data.scenes)
            return seeded().bind(lambda _: counted).map(lambda counts: Some("Convert Effect Colors") if any(count.is_some() for count in counts) else Nothing)

        with ExitStack() as scope:

            def followed(stamped: Stored[str]) -> Acquisitions:
                later = scope.enter_context(identity.reports.deferred(lambda: converted(stamped)))

                def checked(scene: bpy.types.Scene, _updates: tuple[bpy.types.DepsgraphUpdate, ...]) -> BlenderResult[None]:
                    target = bpy.data.colorspace.working_space
                    return source(stamped, scene, target).map(lambda start: None if start == target else later())

                return Startup(body=seeded), UpdateHandler(handlers=bpy.app.handlers.depsgraph_update_post, ids=(bpy.types.ID,), body=checked)

            yield identity.name(Name.ATTACHMENT, self.stamp).map(lambda name: followed(Stored(name=name, decoder=msgspec.msgpack.Decoder(str))))


class FilmView(msgspec.Struct, frozen=True, kw_only=True):
    """Photographic formation named by its application."""

    name: str
    description: str
    film: Film

    @property
    def formation(self) -> BlenderResult[Formation]:
        """Developed native formation."""
        return self.film.transform().map(lambda transform: Formation.of(self.name, self.description, transform, self.film.space))


class ConfigInstall(msgspec.Struct, frozen=True, kw_only=True):
    """Application OCIO rows installed over Blender's bundled configuration."""

    build: ConfigBuild
    films: tuple[FilmView, ...] = ()

    @property
    def merged(self) -> BlenderResult[ConfigBuild]:
        """Application build with film formations added."""
        return accumulated(view.formation for view in self.films).map(lambda formations: msgspec.structs.replace(self.build, formations=(*self.build.formations, *formations)))

    @property
    def base(self) -> BlenderResult[PyOpenColorIO.Config]:
        """Bundled base configuration independent of a user installation."""
        return opened(ConfigFile(path=str(Path(bpy.utils.system_resource("DATAFILES", path="colormanagement"), "config.ocio"))), ())  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

    def staged(self, stage: Path) -> BlenderResult[PyOpenColorIO.Config]:
        """Stages full build in caller's resource scope."""
        return paired(self.base, self.merged).bind(lambda pair: pair[1].staged(pair[0], stage))

    def extended(self, formation: Formation) -> Self:
        """Returns installation with formation appended."""
        return msgspec.structs.replace(self, build=msgspec.structs.replace(self.build, formations=(*self.build.formations, formation)))

    def written(self) -> BlenderResult[Path]:
        """Installs complete configuration read on restart."""
        return paired(self.base, self.merged).bind(lambda pair: pair[1].installed(pair[0], Path(bpy.utils.user_resource("DATAFILES", path="colormanagement"))))

    def installed(self, _call: Call[Empty]) -> BlenderResult[Done]:
        """Installs build with a restart note."""
        return self.written().map(lambda path: Done(notes=(("INFO", f"Installed {path}, restart Blender to read it"),)))

    @property
    def installing(self) -> Operator[Empty]:
        """Command installing the build."""
        return Operator(key="install_config", title="Install Color Config", description="Install extension formations read on restart", form=Empty, run=self.installed, undo=False)

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns install command."""
        return Ok((self.installing,))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COLORS]


def config_of(spaces: Iterable[str] = ()) -> BlenderResult[PyOpenColorIO.Config]:
    """Returns live OCIO config, a fault per unresolved required role or given space."""
    return resolved(PyOpenColorIO.GetCurrentConfig(), spaces)


def effect_colors(scene: bpy.types.Scene) -> tuple[tuple[Native, Descriptor[bpy.types.FloatProperty], npt.NDArray[np.float32]], ...]:
    """Returns color socket values of every scene effect group input with its writer."""
    return tuple(
        (item, descriptor, np.asarray(descriptor.read(item), dtype=np.float32).copy())
        for effect in scene.compositor_effects
        for inputs in inputs_of(effect).to_option().to_list()
        if isinstance(inputs, bpy.types.bpy_struct)
        for prop in inputs.bl_rna.properties
        if isinstance(prop, bpy.types.PointerProperty)
        for item in struct_at(inputs, prop.identifier).to_list()
        if isinstance(item, bpy.types.bpy_struct)
        if "value" in item.bl_rna.properties
        for color in (item.bl_rna.properties["value"],)
        if isinstance(color, bpy.types.FloatProperty) and color.subtype == "COLOR"
        for descriptor in (Descriptor(root=type(item), owner="", prop=color),)
    )


def space_item(config: PyOpenColorIO.Config, name: str) -> BlenderResult[str]:
    """Returns conversion-node item of a role or colorspace."""
    return (
        Ok(name)
        if name == PyOpenColorIO.ROLE_SCENE_LINEAR
        else Option.of_optional(config.getColorSpace(name)).map(PyOpenColorIO.ColorSpace.getName).to_result_with(lambda: (BlenderAbsentFault(owner=config.getName(), key=name),))
    )


# --- [GRAPHS]


def converted(key: Key, config: PyOpenColorIO.Config, source: str, target: str, entering: Term) -> BlenderResult[Expr]:
    """Converts straight RGB between colorspaces, scene-linear role resolved live."""
    return paired(space_item(config, source), space_item(config, target)).map(
        lambda pair: Expr(
            kind=bpy.types.CompositorNodeConvertColorSpace, inputs=(("Image", entering),), members=(("from_color_space", pair[0]), ("to_color_space", pair[1])), socket="Image", shape="RGBA"
        ).named(key)
    )


def pointwise(key: Key, entering: Term, operation: Callable[[Expr], BlenderResult[Expr]]) -> BlenderResult[Expr]:
    """Applies straight-color graph to associated RGB, zero alpha keeping transformed RGB."""
    alpha = Expr(kind=bpy.types.CompositorNodeSeparateColor, inputs=(("Image", entering),), members=(("mode", "RGB"),), socket="Alpha")

    def associated(adjusted: Expr) -> Expr:
        restored = Expr(kind=bpy.types.CompositorNodeSetAlpha, inputs=(("Image", adjusted), ("Alpha", alpha), ("Type", "Replace Alpha")), socket="Image", shape="RGBA")
        transparent = Expr(kind=bpy.types.FunctionNodeCompare, inputs=(("A", alpha), ("B", 0.0), ("Epsilon", 0.0)), members=(("data_type", "FLOAT"), ("operation", "EQUAL")), socket="Result")
        premultiplied = Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", restored), ("Type", "To Premultiplied")), socket="Image", shape="RGBA")
        return where(transparent, restored, premultiplied).named(key)

    return operation(Expr(kind=bpy.types.CompositorNodePremulKey, inputs=(("Image", entering), ("Type", "To Straight")), socket="Image", shape="RGBA")).map(associated)


def _converter(key: Key, name: str, body: Callable[[Expr, Expr, Expr], tuple[Expr, Expr, Expr]]) -> BlenderResult[NodeGroup]:
    """Converts RGB channels and passes alpha unchanged."""
    image = Expr(kind=bpy.types.NodeGroupInput, socket=COLOR_INPUT.key, shape="RGBA", label=Some(INPUTS))
    return NodeGroup.built(key, name, Interface(inputs=(COLOR_INPUT,), outputs=(COLOR_RESULT,)), {COLOR_RESULT.key: rgba(*body(image["Red"], image["Green"], image["Blue"]), image["Alpha"])})


@functools.cache
def logarithmic(direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO channel stop coordinates from LogCamera parameters."""
    transform = lin_to_log()
    base, forward = transform.getBase(), direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD
    log_slope, log_offset, lin_slope, lin_offset, lin_break, linear_slope = (
        transform.getLogSideSlopeValue(),
        transform.getLogSideOffsetValue(),
        transform.getLinSideSlopeValue(),
        transform.getLinSideOffsetValue(),
        transform.getLinSideBreakValue(),
        transform.getLinearSlopeValue(),
    )
    log_break = tuple(side * log(slope * point + offset, base) + shift for side, shift, slope, offset, point in zip(log_slope, log_offset, lin_slope, lin_offset, lin_break))
    linear_offset = tuple(held - slope * point for held, slope, point in zip(log_break, linear_slope, lin_break))

    def channel(value: Expr, at: int) -> Expr:
        if forward:
            logged = math("MULTIPLY_ADD", math("LOGARITHM", math("MULTIPLY_ADD", value, lin_slope[at], lin_offset[at]), base), log_slope[at], log_offset[at])
            return where(value < lin_break[at], math("MULTIPLY_ADD", value, linear_slope[at], linear_offset[at]), logged)
        powered = math("POWER", base, math("MULTIPLY_ADD", value, 1.0 / log_slope[at], -log_offset[at] / log_slope[at]))
        linear = math("MULTIPLY_ADD", value, 1.0 / linear_slope[at], -linear_offset[at] / linear_slope[at])
        return where(value < log_break[at], linear, math("MULTIPLY_ADD", powered, 1.0 / lin_slope[at], -lin_offset[at] / lin_slope[at]))

    return _converter(
        "lin_to_log" if forward else "log_to_lin", "Lin to Log" if forward else "Log to Lin", lambda *channels: (channel(channels[0], 0), channel(channels[1], 1), channel(channels[2], 2))
    )


@functools.cache
def hsy(direction: PyOpenColorIO.TransformDirection) -> BlenderResult[NodeGroup]:
    """Builds OCIO linear HSY coordinates as compositor math nodes."""
    low_luma, high_luma = LUMA
    forward = direction == PyOpenColorIO.TRANSFORM_DIR_FORWARD

    def luminance(values: tuple[Expr, Expr, Expr]) -> Expr:
        red, green, blue = (value * weight for value, weight in zip(values, WEIGHTS))
        return red + green + blue

    def lit(luma: Expr) -> Expr:
        return math("MULTIPLY_ADD", luma, 1.0 / (high_luma - low_luma), -low_luma / (high_luma - low_luma), clamp=True)

    def converted(first: Expr, second: Expr, third: Expr) -> tuple[Expr, Expr, Expr]:
        channels = first, second, third
        luma = luminance(channels)
        distance = abs(first - luma) + abs(second - luma) + abs(third - luma)
        high = math("MAXIMUM", first, math("MAXIMUM", second, third))
        low = math("MINIMUM", first, math("MINIMUM", second, third))
        delta = high - low
        sectors = tuple((channels[(index + 1) % 3] - channels[(index + 2) % 3]) / delta + float(2 * index + 1) for index in range(3))
        hue = where(low < high, where(first < high, where(second < high, sectors[2], sectors[1]), sectors[0]), 0.0) / 6.0
        high_saturation = distance / math("MAXIMUM", math("MULTIPLY_ADD", distance, 0.07, 1e-6), first + second + third + 0.15)
        return hue, mixed(lit(luma), distance * 5.0, high_saturation) * 1.4, luma

    def inverted(hue: Expr, saturation: Expr, luma: Expr) -> tuple[Expr, Expr, Expr]:
        sector = math("FRACT", math("MULTIPLY_ADD", luma < 0.0, 0.5, hue) - 1.0 / 6.0) * 6.0
        unit = (math("SUBTRACT", abs(sector - 3.0), 1.0, clamp=True), math("SUBTRACT", 2.0, abs(sector - 2.0), clamp=True), math("SUBTRACT", 2.0, abs(sector - 4.0), clamp=True))
        scale = luma / luminance(unit)
        bases = tuple(value * scale for value in unit)
        offsets = tuple(value - luma for value in bases)
        distance = abs(offsets[0]) + abs(offsets[1]) + abs(offsets[2])
        chroma, triple = saturation / 1.4, luma * 3.0
        excess, reach = bases[0] + bases[1] + bases[2] - triple, triple + 0.15
        chroma_excess, chroma_reach = chroma * excess, chroma * reach
        high_gain = math("MINIMUM", chroma_reach / math("MAXIMUM", distance - chroma_excess, 1e-6), 50.0)
        low_distance = distance * 5.0
        blend = lit(luma)
        low_weight = low_distance * (1.0 - blend)
        minus_b = chroma_excess - math("MULTIPLY_ADD", distance, blend, low_weight * reach)
        discriminant = math("SQRT", math("MULTIPLY_ADD", low_weight * excess * chroma_reach, 4.0, minus_b * minus_b))
        twice_c = chroma_reach * -2.0
        near = twice_c / (minus_b - discriminant)
        far = twice_c / math("MULTIPLY_ADD", discriminant, 2.0, minus_b - discriminant)
        gain = where(blend < 1.0, where(blend > 0.0, where(near < 0.0, far, near), chroma / math("MAXIMUM", low_distance, 1e-10)), high_gain)
        red, green, blue = (math("MULTIPLY_ADD", offset, gain, luma) for offset in offsets)
        return red, green, blue

    return _converter("rgb_to_hsy" if forward else "hsy_to_rgb", "RGB to HSY" if forward else "HSY to RGB", converted if forward else inverted)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLOR_INPUT",
    "COLOR_RESULT",
    "CONFIGURED",
    "LUMA",
    "WEIGHTS",
    "ConfigInstall",
    "FilmView",
    "WorkingColors",
    "config_of",
    "converted",
    "effect_colors",
    "hsy",
    "logarithmic",
    "pointwise",
    "space_item",
]
