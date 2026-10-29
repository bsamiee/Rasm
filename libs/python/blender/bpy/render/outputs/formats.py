"""Image format settings records, File Output node items with the names the host stores, and written file storage."""

from collections.abc import Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from functools import reduce
from itertools import count
import re
from typing import ClassVar, Final, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
import msgspec
from msgspec import UNSET, UnsetType

from blender.bpy.commands.operators import answered
from blender.bpy.data.rna.descriptors import Descriptor, differing
from blender.bpy.data.rna.paths import rooted, Struct
from blender.bpy.data.rna.snapshots import assigned, Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.faults import BlenderReason
from blender.bpy.nodes.desired import member_paths
from blender.bpy.nodes.trees import arranged, nested
from blender.bpy.render.color.spaces import DisplaySettings, Settings, ViewSettings
from blender.bpy.settings.presets import Preset
from blender.bpy.ui.layout.presets import Listing
from blender.imaging.files.buffers import Depth
from blender.imaging.files.images import Codec, Compressions, Plain
from blender.records.faults import accumulated, Fault, Faults, Outcome, RecordReason
from blender.records.trees import transfer
from blender.rendering.outputs.naming import Output, RESERVED

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [CONSTANTS] ------------------------------------------------------------------------

FILE_FORMATS: Final = frozenset(item.identifier for item in Descriptor.declared(bpy.types.ImageFormatSettings, ("file_format",), bpy.types.EnumProperty).prop.enum_items)
_EXR: Final = frozenset({"OPEN_EXR", "OPEN_EXR_MULTILAYER"})

# --- [MODELS] ---------------------------------------------------------------------------


class Colorspace(Settings, frozen=True, kw_only=True):
    """`ColorManagedInputColorspaceSettings` members."""

    name: str | UnsetType = UNSET


class Stereo3dFormat(Settings, frozen=True, kw_only=True):
    """`Stereo3dFormat` members."""

    display_mode: str | UnsetType = UNSET
    anaglyph_type: str | UnsetType = UNSET
    interlace_type: str | UnsetType = UNSET
    use_interlace_swap: bool | UnsetType = UNSET
    use_sidebyside_crosseyed: bool | UnsetType = UNSET
    use_squeezed_frame: bool | UnsetType = UNSET


class OutputFormat(Settings, frozen=True, kw_only=True):
    """`ImageFormatSettings` members, the snapshot writer ordering governing members before those their writes reset."""

    media_type: str | UnsetType = UNSET
    file_format: str | UnsetType = UNSET
    color_mode: str | UnsetType = UNSET
    color_depth: str | UnsetType = UNSET
    quality: int | UnsetType = UNSET
    compression: int | UnsetType = UNSET
    use_preview: bool | UnsetType = UNSET
    exr_codec: str | UnsetType = UNSET
    use_exr_interleave: bool | UnsetType = UNSET
    use_jpeg2k_ycc: bool | UnsetType = UNSET
    use_jpeg2k_cinema_preset: bool | UnsetType = UNSET
    use_jpeg2k_cinema_48: bool | UnsetType = UNSET
    jpeg2k_codec: str | UnsetType = UNSET
    tiff_codec: str | UnsetType = UNSET
    use_cineon_log: bool | UnsetType = UNSET
    cineon_black: int | UnsetType = UNSET
    cineon_white: int | UnsetType = UNSET
    cineon_gamma: float | UnsetType = UNSET
    views_format: str | UnsetType = UNSET
    stereo_3d_format: Stereo3dFormat | UnsetType = UNSET
    color_management: str | UnsetType = UNSET
    view_settings: ViewSettings | UnsetType = UNSET
    display_settings: DisplaySettings | UnsetType = UNSET
    linear_colorspace_settings: Colorspace | UnsetType = UNSET

    @classmethod
    def exr(cls, output: Output, *, layered: bool) -> Outcome[Self]:
        """Returns the EXR an output class writes, half-float DWAB for beauty and full-float ZIP for data and mattes, refusing the preview class."""
        container = cls(media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER") if layered else cls(media_type="IMAGE", file_format="OPEN_EXR")
        match output:
            case Output.BEAUTY:
                return Ok(msgspec.structs.replace(container, color_depth="16", exr_codec="DWAB"))
            case Output.DATA | Output.MATTE:
                return Ok(msgspec.structs.replace(container, color_depth="32", exr_codec="ZIP"))
            case Output.PREVIEW:
                return Error(Fault.of(RecordReason.OUTSIDE, "File Output class", output.name, member=", ".join(member.name for member in Output if member is not Output.PREVIEW)))

    @classmethod
    def stored(cls, settings: bpy.types.ImageFormatSettings) -> Self:
        """Reads every member image format settings hold."""
        return msgspec.convert(settings, cls, from_attributes=True)

    @property
    def layered(self) -> bool:
        """Whether the format writes one multilayer file."""
        return self.media_type == "MULTI_LAYER_IMAGE"

    @property
    def codec(self) -> Option[Compressions]:
        """HTJ2K codec a reader's header metadata lacks, Nothing for every other storage."""
        return Some(Plain(codec=Codec.HTJ2K32)) if self.file_format in _EXR and self.exr_codec == "HTJ2K" else Nothing

    def applied(self, settings: bpy.types.ImageFormatSettings) -> Outcome[None]:
        """Writes the set members onto image format settings."""
        return assigned(settings, member_paths(self.members()))

    @contextmanager
    def held(self, settings: bpy.types.ImageFormatSettings) -> Generator[Outcome[None]]:
        """Writes the format for the scope, the whole prior struct, pointer members included, restored on exit on every path."""
        prior: Outcome[Snapshot] = Snapshot.capture(settings)
        with ExitStack() as restore:
            for snapshot in prior.to_option().to_list():
                restore.callback(snapshot.apply, settings)
            yield Result[Snapshot, Faults].bind(prior, lambda _: self.applied(settings))


class FileItem(Settings, frozen=True, kw_only=True):
    """`NodeCompositorFileOutputItem` members, `format` overriding the node's when `override_node_format` is set."""

    name: str
    socket_type: "rna_enums.NodeSocketDataTypeItems"
    vector_socket_dimensions: int | UnsetType = UNSET
    override_node_format: bool | UnsetType = UNSET
    save_as_render: bool | UnsetType = UNSET
    format: OutputFormat | UnsetType = UNSET


class FileItems(msgspec.Struct, frozen=True, kw_only=True):
    """Ordered File Output node items, the content a File Output node holds, each name the spelling the host stores."""

    items: tuple[FileItem, ...]

    nested: ClassVar[tuple[()]] = ()

    def __post_init__(self) -> None:
        """Respells each item name as the host stores it."""
        msgspec.structs.force_setattr(self, "items", tuple(msgspec.structs.replace(item, name=name) for item, name in zip(self.items, self.stored(item.name for item in self.items))))

    @staticmethod
    def stored(requested: Iterable[str], held: tuple[str, ...] = ()) -> tuple[str, ...]:
        """Predicts the host's truncation, sanitization, and uniqueness of item names in item order, kept spellings of `held` items staying, the names no host member states before a write."""
        asked = tuple(requested)

        def spelled(done: tuple[str, ...], name: str) -> tuple[str, ...]:
            index, safe = len(done), re.sub(f"[{RESERVED}]", "_", name.encode()[:1023].decode(errors="ignore"))
            taken = {*done, *(kept for later, kept in enumerate(held) if later > index and asked[later] == kept)}
            base, number = (found[1], int(found[2])) if (found := re.fullmatch(r"(.*)_(\d+)", safe)) else (safe, 0)
            return (*done, safe if held[index : index + 1] == (name,) or safe not in taken else next(f"{base}_{number:03d}" for number in count(number + 1) if f"{base}_{number:03d}" not in taken))

        return reduce(spelled, asked, ())

    @property
    def keyed(self) -> object:
        """Builtin form a node graph digest reads."""
        return tuple(msgspec.to_builtins(item) for item in self.items)

    def holds(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[bool]:
        """Whether the File Output node holds these items in order with these members."""
        return _output(live).map(
            lambda output: len(output.file_output_items) == len(self.items) and all(not differing(held, member_paths(item.members())) for held, item in zip(output.file_output_items, self.items))
        )

    def write(self, live: bpy.types.Node, _identity: Identity, /) -> Outcome[None]:
        """Arranges the File Output items by name, then writes each item's members."""
        kinds = {item.name: item.socket_type for item in self.items}
        return _output(live).bind(
            lambda output: accumulated(
                assigned(held, member_paths(item.members()))
                for held, item in zip(arranged(output.file_output_items, tuple(kinds), lambda name: output.file_output_items.new(kinds[name], name), output.file_output_items.remove), self.items)
            ).map(lambda _: None)
        )

    def sockets(self, live: bpy.types.Node, _identity: Identity, _slot: str, /) -> Mapping[str, str]:
        """Item socket identifiers by item name, the node inputs following these items in order."""
        return {item.name: socket.identifier for item, socket in zip(self.items, live.inputs)}


# --- [OPERATIONS] -----------------------------------------------------------------------


def pointer(struct: type[Struct]) -> str:
    """Returns the member holding the struct's image format."""
    (identifier,) = (prop.identifier for prop in struct.bl_rna.properties if isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.ImageFormatSettings))
    return identifier


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


def extension(settings: bpy.types.ImageFormatSettings) -> Option[str]:
    """Returns the still suffix a format writes, `.jp2` or `.j2c` by JPEG2000 codec, which no image format member states."""
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


def _captured(context: bpy.types.Context) -> Outcome[Snapshot]:
    """Context scene's output format settings."""
    return rooted(bpy.types.Scene, context).bind(lambda scene: Snapshot.capture(scene.render.image_settings))


def _output(live: bpy.types.Node) -> Outcome[bpy.types.CompositorNodeOutputFile]:
    """Node as File Output, else `absent` fault naming its items."""
    return Option.of_optional(try_downcast(bpy.types.CompositorNodeOutputFile, live)).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(live), member="file_output_items"))


# --- [COMPOSITION] ----------------------------------------------------------------------

format_listing: Final = Listing(
    preset=Preset(key="image_formats", form=Snapshot),
    current=_captured,
    matches=lambda context, values: _captured(context).map(lambda held: tuple(not transfer(held.tree, value.tree) for value in values)),
    applied=lambda context, value: rooted(bpy.types.Scene, context).bind(lambda scene: answered(value.apply(scene.render.image_settings), "Output format already holds preset")),
)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["FILE_FORMATS", "Colorspace", "FileItem", "FileItems", "OutputFormat", "Stereo3dFormat", "depth", "extension", "format_listing", "owners", "pointer"]
