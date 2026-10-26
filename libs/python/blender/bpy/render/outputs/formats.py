"""Output format value sets and written file storage."""

from collections.abc import Callable
from enum import Enum
from typing import Final, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import PyOpenColorIO

from blender.bpy.commands.operators import answered
from blender.bpy.data.rna.descriptors import Descriptor, rooted
from blender.bpy.data.rna.snapshots import Applied, Snapshot
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.nodes.graphs import DataType
from blender.bpy.nodes.trees import nested
from blender.bpy.settings.presets import Preset
from blender.bpy.settings.properties import Choices
from blender.bpy.ui.layout.values import Listing
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.images import Codec, Compressions, Depth, Format, Plain
from blender.records.faults import DomainFault, paired
from blender.records.fields import Choice
from blender.records.trees import transfer
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------


class Kind(Choice):
    """File layout written by a native output owner."""

    LAYERED = 1, "Multilayer EXR"
    EXR = 2, "OpenEXR"
    IMAGE = 3, "Image"


class Verdict(Enum):
    """How a writer uses an output choice."""

    HONORED = "Honored"
    COLORSPACE = "No display transform"
    UNSUPPORTED = "Unsupported"


# --- [CONSTANTS] ------------------------------------------------------------------------

COLOR_DEPTH: Final = Descriptor.declared(bpy.types.ImageFormatSettings, "color_depth", bpy.types.EnumProperty)

# --- [MODELS] ---------------------------------------------------------------------------


class Readout(msgspec.Struct, frozen=True, kw_only=True):
    """Output choice and writer outcome shown as one label."""

    caption: str
    value: str
    verdict: Verdict

    @property
    def text(self) -> str:
        """Choice and outcome shown beside output controls."""
        return f"{self.caption} · {self.value} · {self.verdict.value}"

    @classmethod
    def rows(cls, values: tuple[str, str, str], verdicts: tuple[Verdict, Verdict, Verdict]) -> tuple[Self, Self, Self]:
        """Returns format, depth, and display readouts."""
        (format_name, depth_name, display), (shown, stored, encoded) = values, verdicts
        return cls(caption="Format", value=format_name, verdict=shown), cls(caption="Depth", value=depth_name, verdict=stored), cls(caption="Display", value=display, verdict=encoded)


class OutputFormat(msgspec.Struct, frozen=True, kw_only=True):
    """Owner settings and its image format applied in native dependency order."""

    own: Snapshot
    format: Snapshot

    @classmethod
    def of(cls, output: Output, kind: Kind) -> BlenderResult[Self]:
        """Returns node format, choice faults for an unsupported class or layout."""
        match kind:
            case Kind.LAYERED:
                container = Ok({"media_type": "MULTI_LAYER_IMAGE", "file_format": "OPEN_EXR_MULTILAYER"})
            case Kind.EXR:
                container = Ok({"media_type": "IMAGE", "file_format": "OPEN_EXR"})
            case Kind.IMAGE:
                container = Error((DomainFault(subject="File Output format", value=kind.name, accepted=f"{Kind.LAYERED.name}, {Kind.EXR.name}"),))
        match output:
            case Output.BEAUTY:
                stored = Ok({"color_depth": "16", "exr_codec": "DWAB"})
            case Output.DATA | Output.MATTE:
                stored = Ok({"color_depth": "32", "exr_codec": "ZIP"})
            case Output.PREVIEW:
                stored = Error((DomainFault(subject="File Output class", value=output.name, accepted=", ".join(member.name for member in Output if member is not Output.PREVIEW)),))
        return paired(container, stored).map(lambda pair: cls(own=Snapshot.of(bpy.types.CompositorNodeOutputFile, {}), format=Snapshot.of(bpy.types.ImageFormatSettings, pair[0] | pair[1])))

    @classmethod
    def typed(cls, kind: DataType) -> BlenderResult[Option[Self]]:
        """Returns full-float item override for data sockets, Nothing for color."""
        return (
            cls.of(Output.DATA, Kind.EXR).map(
                lambda settings: Some(msgspec.structs.replace(settings, own=Snapshot.of(bpy.types.NodeCompositorFileOutputItem, {"override_node_format": True, "save_as_render": False})))
            )
            if kind in {"FLOAT", "VECTOR"}
            else Ok(Nothing)
        )

    def applied(self, target: bpy.types.Node | bpy.types.NodeCompositorFileOutputItem) -> BlenderResult[Applied]:
        """Applies owner settings before format they constrain."""
        own = self.own.apply(target)

        def formatted() -> BlenderResult[Applied]:
            written = self.format.apply(getattr(target, pointer(type(target))))
            return written.outcome.map(lambda _: Applied.join((own, written)))

        return own.outcome.bind(lambda _: formatted())


# --- [OPERATIONS] -----------------------------------------------------------------------


def pointer(struct: "type[bpy.types.bpy_struct[object]]") -> str:
    """Returns the member holding the struct's image format."""
    (identifier,) = (prop.identifier for prop in struct.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.ImageFormatSettings))
    return identifier


def writer(name: str) -> Option[Format]:
    """Returns named writer."""
    return Option.of_optional(next((row for row in Format.writable() if row.name == name), None))


def sample(selected: Format, depth: Depth) -> BlenderResult[Depth]:
    """Accepts a depth the writer stores."""
    accepted = selected.depths
    return Ok(depth) if depth in accepted else Error((DomainFault(subject=selected.name, value=depth.name, accepted=", ".join(member.name for member in accepted)),))


def display_space(config: PyOpenColorIO.Config, display: str) -> BlenderResult[str]:
    """Resolves a certified display baseline encoding in native view order."""
    found: BlenderResult[Option[tuple[DisplayView, PyOpenColorIO.Processor]]] = DisplayView.for_encoding(config, display)
    return Result[Option[tuple[DisplayView, PyOpenColorIO.Processor]], BlenderFaults].bind(
        found,
        lambda choice: (
            choice
            .bind(lambda pair: pair[0].colorspace(config))
            .map(PyOpenColorIO.ColorSpace.getName)
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"display encoding {display}"),))
        ),
    )


def readouts(selected: Format, depth: Depth, display: str, config: PyOpenColorIO.Config) -> tuple[Readout, Readout, Readout]:
    """Describes writer depth compatibility and certified display encoding."""
    return Readout.rows(
        (selected.name, depth.label, display),
        (
            Verdict.HONORED,
            sample(selected, depth).map(lambda _: Verdict.HONORED).default_value(Verdict.UNSUPPORTED),
            display_space(config, display).map(lambda _: Verdict.HONORED).default_value(Verdict.UNSUPPORTED),
        ),
    )


def native_readouts(settings: bpy.types.ImageFormatSettings, display: str) -> tuple[Readout, Readout, Readout]:
    """Describes native settings and whether writes apply display transform."""
    stored = depth(settings).map(lambda value: value.label).default_with(lambda: COLOR_DEPTH.prop.enum_items[settings.color_depth].description)
    return Readout.rows((settings.file_format, stored, display), (Verdict.HONORED, Verdict.HONORED, Verdict.COLORSPACE if settings.has_linear_colorspace else Verdict.HONORED))


def output_rows(layout: bpy.types.UILayout, _holder: "bpy.types.bpy_struct[object]", context: bpy.types.Context) -> None:
    """Draws native output format, depth, and display policy."""
    for scene in Option.of_optional(context.scene).to_list():
        settings = scene.render.image_settings
        display = (settings.display_settings if settings.color_management == "OVERRIDE" else scene.display_settings).display_device  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
        for row in native_readouts(settings, display):
            layout.label(text=row.text)


def kind(settings: bpy.types.ImageFormatSettings) -> Kind:
    """Classifies image container."""
    return Kind.LAYERED if settings.media_type == "MULTI_LAYER_IMAGE" else Kind.EXR if settings.file_format == "OPEN_EXR" else Kind.IMAGE


def depth(settings: bpy.types.ImageFormatSettings) -> Option[Depth]:
    """Returns storage depth, Nothing for packed 10-bit and 12-bit files."""
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
    """Supplies HTJ2K codec absent from reader's header metadata."""
    return Some(Plain(codec=Codec.HTJ2K32)) if kind(settings) is not Kind.IMAGE and settings.exr_codec == "HTJ2K" else Nothing


def extension(settings: bpy.types.ImageFormatSettings) -> Option[str]:
    """Returns native still suffix, `.jp2` or `.j2c` by JPEG2000 codec."""
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
    nodes = tuple(
        node
        for tree in nested(effect.node_group for effect in scene.compositor_effects if isinstance(effect.node_group, bpy.types.CompositorNodeTree))
        for node in tree.nodes
        if isinstance(node, bpy.types.CompositorNodeOutputFile)
    )
    return (
        scene.render.image_settings,
        *(held for node in nodes for held in Option.of_optional(node.format).to_list()),
        *(held for node in nodes for item in node.file_output_items if item.override_node_format for held in Option.of_optional(item.format).to_list()),
    )


def struct_listing(preset: Preset[Snapshot], struct: Callable[[bpy.types.Scene], "bpy.types.bpy_struct[object]"], unchanged: str) -> Listing[Snapshot]:
    """Returns preset actions over one struct of the context scene."""

    def captured(context: bpy.types.Context) -> BlenderResult[Snapshot]:
        return rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(struct(scene)))

    return Listing(
        preset=preset,
        current=captured,
        matches=lambda context, values: captured(context).map(lambda held: tuple(not transfer(held.tree, value.tree) for value in values)),
        applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: answered(value.apply(struct(scene)), unchanged)),
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


writers = Choices(lambda _identity, _owner, _context: ((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in Format.writable()))
displays = Choices.named(lambda _identity, _owner, _context: PyOpenColorIO.GetCurrentConfig().getDisplays())
presets = Preset(key="image_formats", form=Snapshot)
format_listing = struct_listing(presets, lambda scene: scene.render.image_settings, "Output format already holds preset")

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "COLOR_DEPTH",
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
    "pointer",
    "presets",
    "readouts",
    "sample",
    "struct_listing",
    "writer",
    "writers",
]
