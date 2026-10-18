"""Native output format value sets and the storage facts of written files."""

from enum import Enum
from typing import Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import answered
from blender.bpy.data.rna.descriptors import rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.faults import BlenderAbsentFault, BlenderChoiceFault, BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.nodes.graphs import DataType
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.sections import Listing
from blender.imaging.color.processors import DisplayView
from blender.imaging.faults import paired
from blender.imaging.files.images import Codec, Compressions, Depth, Format, Plain
from blender.imaging.parameters import Choice
from blender.rendering.edits import transfer
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class Holder(Enum):
    """Native format owner and its format-pointer identifier."""

    SCENE = bpy.types.RenderSettings, "image_settings"
    BAKE = bpy.types.BakeSettings, "image_settings"
    NODE = bpy.types.CompositorNodeOutputFile, "format"
    ITEM = bpy.types.NodeCompositorFileOutputItem, "format"

    def __init__(self, struct: "type[bpy.types.bpy_struct[object]]", pointer: str) -> None:
        """Retains the native owner and its pointer."""
        self.struct, self.pointer = struct, pointer


class Kind(Choice):
    """File layout written by a native output owner."""

    LAYERED = 1, "Multilayer EXR"
    EXR = 2, "OpenEXR"
    IMAGE = 3, "Image"


class Verdict(Enum):
    """How a writer uses an output choice."""

    HONORED = "Honored"
    COLORSPACE = "Color-space conversion without the display transform"
    UNSUPPORTED = "Unsupported"


# --- [MODELS] ---------------------------------------------------------------------------


class Readout(msgspec.Struct, frozen=True, kw_only=True):
    """One output choice and the writer's outcome, rendered by the library."""

    caption: str
    value: str
    verdict: Verdict

    @property
    def text(self) -> str:
        """Choice and outcome shown beside output controls."""
        return f"{self.caption}: {self.value} — {self.verdict.value}"


class OutputFormat(msgspec.Struct, frozen=True, kw_only=True):
    """Owner settings and its image format applied in native dependency order."""

    holder: Holder
    own: Snapshot
    format: Snapshot

    @classmethod
    def of(cls, output: Output, kind: Kind) -> BlenderResult[Self]:
        """Returns the node format or the unsupported scene-output class or image layout."""
        match kind:
            case Kind.LAYERED:
                container = Ok({"media_type": "MULTI_LAYER_IMAGE", "file_format": "OPEN_EXR_MULTILAYER"})
            case Kind.EXR:
                container = Ok({"media_type": "IMAGE", "file_format": "OPEN_EXR"})
            case Kind.IMAGE:
                container = Error((BlenderChoiceFault(path="File Output format", identifier=kind.name, accepted=(Kind.LAYERED.name, Kind.EXR.name)),))
        match output:
            case Output.BEAUTY:
                stored = Ok({"color_depth": "16", "exr_codec": "DWAB"})
            case Output.DATA | Output.MATTE:
                stored = Ok({"color_depth": "32", "exr_codec": "ZIP"})
            case Output.PREVIEW:
                stored = Error((BlenderChoiceFault(path="File Output class", identifier=output.name, accepted=tuple(member.name for member in Output if member is not Output.PREVIEW)),))
        return paired(container, stored).map(lambda pair: cls(holder=Holder.NODE, own=Snapshot.of(Holder.NODE.struct, {}), format=Snapshot.of(bpy.types.ImageFormatSettings, pair[0] | pair[1])))

    @classmethod
    def typed(cls, kind: DataType) -> BlenderResult[Option[Self]]:
        """Returns a full-float item override for data sockets and no color override."""
        return (
            cls.of(Output.DATA, Kind.EXR).map(
                lambda settings: Some(msgspec.structs.replace(settings, holder=Holder.ITEM, own=Snapshot.of(Holder.ITEM.struct, {"override_node_format": True, "save_as_render": False})))
            )
            if kind in {"FLOAT", "VECTOR"}
            else Ok(Nothing)
        )

    def applied(self, target: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem) -> BlenderResult[Applied]:
        """Applies owner settings before the format they constrain, returning native outcomes."""
        own = self.own.apply(target)

        def formatted() -> BlenderResult[Applied]:
            written = self.format.apply(getattr(target, self.holder.pointer))
            return written.outcome.map(lambda _: Applied.join((own, written)))

        return own.outcome.bind(lambda _: formatted())


# --- [OPERATIONS] -----------------------------------------------------------------------


def writer(name: str) -> Option[Format]:
    """Returns the provider's named writer or absence."""
    return Option.of_optional(next((row for row in Format.writable() if row.name == name), None))


def sample(selected: Format, depth: Depth) -> BlenderResult[Depth]:
    """Admits a requested channel type against the owning writer's stored depths."""
    accepted = selected.depths
    return Ok(depth) if depth in accepted else Error((BlenderChoiceFault(path=selected.name, identifier=depth.name, accepted=tuple(member.name for member in accepted)),))


def display_space(config: PyOpenColorIO.Config, display: str) -> BlenderResult[str]:
    """Resolves a certified display baseline encoding in native view order."""
    found: BlenderResult[Option[tuple[DisplayView, PyOpenColorIO.Processor]]] = DisplayView.for_encoding(config, display).map_error(BlenderImagingFault.lifted)
    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
        found,
        lambda choice: (
            choice
            .bind(lambda pair: pair[0].colorspace(config))
            .map(PyOpenColorIO.ColorSpace.getName)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"display encoding: {display}"),))
        ),
    )


def readouts(selected: Format, depth: Depth, display: str, config: PyOpenColorIO.Config) -> tuple[Readout, Readout, Readout]:
    """Describes writer depth compatibility and certified display encoding."""
    return (
        Readout(caption="Format", value=selected.name, verdict=Verdict.HONORED),
        Readout(caption="Depth", value=depth.label, verdict=sample(selected, depth).map(lambda _: Verdict.HONORED).default_value(Verdict.UNSUPPORTED)),
        Readout(caption="Display", value=display, verdict=display_space(config, display).map(lambda _: Verdict.HONORED).default_value(Verdict.UNSUPPORTED)),
    )


def native_readouts(settings: bpy.types.ImageFormatSettings, display: str, *, save_as_render: bool) -> tuple[Readout, Readout, Readout]:
    """Describes native settings and whether their write applies the display transform."""
    return (
        Readout(caption="Format", value=settings.file_format, verdict=Verdict.HONORED),
        Readout(caption="Depth", value=depth(settings).map(lambda value: value.label).default_value(f"{settings.color_depth}-bit"), verdict=Verdict.HONORED),
        Readout(caption="Display", value=display, verdict=Verdict.HONORED if save_as_render and not settings.has_linear_colorspace else Verdict.COLORSPACE),
    )


def output_rows(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
    """Draws the actual native output format, depth, and display policy."""
    for scene in Option.of_optional(context.scene).to_list():
        settings = scene.render.image_settings
        display = (settings.display_settings if settings.color_management == "OVERRIDE" else scene.display_settings).display_device  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        for row in native_readouts(settings, display, save_as_render=True):
            layout.label(text=row.text)


def kind(settings: bpy.types.ImageFormatSettings) -> Kind:
    """Classifies the native image container."""
    return Kind.LAYERED if settings.media_type == "MULTI_LAYER_IMAGE" else Kind.EXR if settings.file_format == "OPEN_EXR" else Kind.IMAGE


def depth(settings: bpy.types.ImageFormatSettings) -> Option[Depth]:
    """Returns rasm storage for supported native depths, omitting packed 10/12-bit files."""
    match settings.color_depth:
        case "8":
            return Some(Depth.UINT8)
        case "16":
            return Some(Depth.HALF if settings.has_linear_colorspace else Depth.UINT16)
        case "32":
            return Some(Depth.FLOAT)
        case _:
            return Nothing


def compression(settings: bpy.types.ImageFormatSettings) -> Option[Compressions]:
    """Supplies the HTJ2K codec absent from the reader's header metadata."""
    return Some(Plain(codec=Codec.HTJ2K32)) if kind(settings) is not Kind.IMAGE and settings.exr_codec == "HTJ2K" else Nothing


def extension(settings: bpy.types.ImageFormatSettings) -> Option[str]:
    """Returns the native still suffix, including the JPEG2000 codec distinction."""
    named = {
        "BMP": ".bmp",
        "IRIS": ".rgb",
        "PNG": ".png",
        "JPEG": ".jpg",
        "TARGA": ".tga",
        "TARGA_RAW": ".tga",
        "CINEON": ".cin",
        "DPX": ".dpx",
        "OPEN_EXR": ".exr",
        "OPEN_EXR_MULTILAYER": ".exr",
        "HDR": ".hdr",
        "TIFF": ".tif",
        "WEBP": ".webp",
        "AVIF": ".avif",
        "JPEG2000": ".jp2" if settings.jpeg2k_codec == "JP2" else ".j2c",
    }
    return Option.of_optional(named.get(settings.file_format))


def owners(scene: bpy.types.Scene) -> tuple[bpy.types.ImageFormatSettings, ...]:
    """Returns scene, nested File Output, and overriding item formats once per tree."""

    def reached(trees: tuple[bpy.types.NodeTree, ...], seen: frozenset[bpy.types.NodeTree]) -> tuple[bpy.types.NodeTree, ...]:
        fresh = tuple(dict.fromkeys(tree for tree in trees if tree not in seen))
        nested = tuple(node.node_tree for tree in fresh for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeGroup) and node.node_tree is not None)
        return (*fresh, *reached(nested, seen | frozenset(fresh))) if fresh else ()

    trees = reached(tuple(effect.node_group for effect in scene.compositor_effects if effect.node_group is not None), frozenset())
    nodes = tuple(node for tree in trees for node in tree.nodes if isinstance(node, bpy.types.CompositorNodeOutputFile))
    items = tuple(item for node in nodes for item in node.file_output_items if item.override_node_format)
    return (
        scene.render.image_settings,
        *(held for node in nodes for held in Option.of_optional(node.format).to_list()),
        *(held for item in items for held in Option.of_optional(item.format).to_list()),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


writers = Choices(lambda _identity, _owner, _context: ((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in Format.writable()))
displays = Choices(lambda _identity, _owner, _context: ((name, name, "Display encoding of the written still") for name in PyOpenColorIO.GetCurrentConfig().getDisplays()))
presets = Preset(key="image_formats", form=Snapshot)
format_listing = Listing(
    preset=presets,
    current=lambda context: rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(scene.render.image_settings)),
    matches=lambda context, values: (
        rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(scene.render.image_settings)).map(lambda held: tuple(not transfer(held.tree, value.tree) for value in values))
    ),
    applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: answered(value.apply(scene.render.image_settings), "Output format already holds this preset")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Holder",
    "Kind",
    "OutputFormat",
    "Readout",
    "Verdict",
    "compression",
    "depth",
    "display_space",
    "displays",
    "extension",
    "format_listing",
    "kind",
    "native_readouts",
    "output_rows",
    "owners",
    "presets",
    "readouts",
    "sample",
    "writer",
    "writers",
]
