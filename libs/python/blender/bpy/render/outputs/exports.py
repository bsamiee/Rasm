"""Displayed-render exports as Linear Raw DNG, tagged stills, and layered documents, and the Combined plane of saved renders."""

from collections.abc import Callable, Mapping
from functools import cached_property
from types import MappingProxyType
import typing
from typing import Final, Protocol

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Save
from blender.bpy.data.ids.images import read
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.render.color.spaces import viewed
from blender.bpy.render.outputs.finishing import stamped
from blender.bpy.render.outputs.formats import display_space, readouts, sample, writer
from blender.bpy.render.outputs.results import built, Production, production, PRODUCTIONS, saved
from blender.bpy.ui.images.views import RESULT
from blender.bpy.ui.layout.rows import alerted
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.buffers import unassociated
from blender.imaging.files.coding import Depth, Rounded
from blender.imaging.files.documents import Document, LinearRaw, Optics
from blender.imaging.files.images import Compressions, Format, ImageFile, Storage
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired

# --- [TYPES] ----------------------------------------------------------------------------


class Documents(Protocol):
    """Layered document source over a saved render."""

    def document(self, identity: Identity, /, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> Outcome[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds layered document."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = bpy.types.ViewLayer.bl_rna.properties["use_pass_combined"].name
COMPOSITE: Final = "Composite"
_STILLS: Final = ("RENDER_RESULT", "COMPOSITING")
_IDLE: Final = Job(kind="RENDER")
_SAVABLE: Final = RESULT & _IDLE
_COMPRESSIONS: Final = MappingProxyType({form.__name__: form for form in typing.get_args(Compressions.__value__)})
_DEPTHS: Final = tuple((depth.name, depth.name, "", "NONE", depth.value) for depth in Depth)
SOURCES: Final = tuple(
    (item.identifier, item.name, item.description, item.icon, item.value) for item in Descriptor.declared(bpy.types.Image, "type", bpy.types.EnumProperty).prop.enum_items if item.identifier in _STILLS
)
WRITERS: Final = tuple((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in Format.writable())
CODECS: Final = (("DEFAULT", "Default", ""), *((name, name, "") for name in _COMPRESSIONS))

# --- [MODELS] ---------------------------------------------------------------------------


class Still(bpy.types.PropertyGroup):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: bpy.props.EnumProperty(name="Source", description="Image the still writes", items=SOURCES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    format: bpy.props.EnumProperty(name="Format", description="File format the still writes", items=WRITERS, default="png")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each channel", items=_DEPTHS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    display: bpy.props.StringProperty(name="Display", description="Display encoding at selected depth", search=lambda _owner, _context, _edit: PyOpenColorIO.GetCurrentConfig().getDisplays())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    save_as_render: bpy.props.BoolProperty(name="Save as Render", description="Apply view, exposure, and gamma to unformed source", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    compression: bpy.props.EnumProperty(name="Compression", description="Writer compression at its defaults, writer default when unset", items=CODECS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Sheet(bpy.types.PropertyGroup):
    """Channel storage selected for a layered TIFF export."""

    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each layer channel", items=_DEPTHS, default=Depth.UINT16.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Layering(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layered TIFF export of a document source over the saved frame, its command named for the source's kind."""

    source: Documents

    @cached_property
    def command(self) -> Operator[Sheet]:
        """Command writing the source's document of the saved frame as layered TIFF, named for the source's kind and described by its document."""

        def exported(call: Call[Sheet]) -> Outcome[Done]:
            """Write the source's document of the saved frame as layered TIFF."""
            target = str(call.paths[0])
            return (
                call
                .root(bpy.types.Scene)
                .bind(
                    lambda scene: (
                        saved(scene)
                        .bind(Multilayer.read)
                        .bind(lambda frame: self.source.document(call.identity, scene, frame, Depth[call.properties.depth]))
                        .bind(lambda document: document.written(target, PyOpenColorIO.GetCurrentConfig()))
                    )
                )
                .map(lambda _: Done.wrote((target,)))
            )

        exported.__name__ = exported.__qualname__ = f"export_{type(self.source).__name__.lower()}"
        return Operator(form=Sheet, run=exported, condition=Some(_SAVABLE), invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))), undo=False)


class Export(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render and Viewer export actions, the DNG exposure adding `gain` stops to the scene view's exposure."""

    gain: Callable[[Identity, bpy.types.Scene], Outcome[float]] = lambda _identity, _scene: Ok(0.0)
    layering: tuple[Layering, ...] = ()

    # --- [COMMANDS]

    @cached_property
    def linear(self) -> Operator[Empty]:
        """Command writing the scene-linear render as DNG."""
        return Operator(form=Empty, run=self.export_dng, condition=Some(_SAVABLE), invoke=Some(Save[Empty](extensions=lambda _: (".dng",))), undo=False)

    @cached_property
    def still(self) -> Operator[Still]:
        """Command writing the render or Viewer image with chosen encoding, its browser seeded with writer-supported values and its suffixes the writer's."""

        def drawn(layout: bpy.types.UILayout, _context: bpy.types.Context, properties: bpy.types.OperatorProperties, _identity: Identity) -> None:
            for name in Still.__annotations__:
                layout.prop(properties, name)
            verdicts = _chosen(properties.format).map(lambda chosen: readouts(chosen, Depth[properties.depth], properties.display, PyOpenColorIO.GetCurrentConfig()))
            for readings in verdicts.to_option().to_list():
                for row in readings:
                    layout.label(text=row.text)
            alerted(layout, verdicts)

        def seeded(properties: Still, _event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
            supported = writer(properties.format).map(lambda row: (row.depths, row.compressions)).default_value(((), ()))
            depth = Depth[properties.depth]
            return {
                "display": PyOpenColorIO.GetCurrentConfig().getDefaultDisplay(),
                "depth": (depth if depth in supported[0] or not supported[0] else supported[0][0]).name,
                "compression": properties.compression if _COMPRESSIONS.get(properties.compression) in supported[1] else "DEFAULT",
            }

        def dotted(row: Format) -> tuple[str, *tuple[str, ...]]:
            first, *rest = row.extensions
            return (f".{first}", *(f".{suffix}" for suffix in rest))

        def suffixes(properties: Still) -> tuple[str, *tuple[str, ...]]:
            return writer(properties.format).map(dotted).default_value((f".{properties.format}",))

        return Operator(form=Still, run=export_still, condition=Some(_IDLE), invoke=Some(Save[Still](extensions=suffixes)), seed=Some(seeded), draw=Some(drawn), undo=False)

    @property
    def layered(self) -> tuple[Operator[Sheet], ...]:
        """Layered document commands in layering order."""
        return tuple(each.command for each in self.layering)

    # --- [WRITES]

    def export_dng(self, call: Call[Empty]) -> Outcome[Done]:
        """Write scene-linear render as Linear Raw DNG with total exposure and camera optics."""
        target = str(call.paths[0])

        def wrote(scene: bpy.types.Scene, frame: npt.NDArray[np.float32], exposure: float, config: PyOpenColorIO.Config) -> Outcome[Done]:
            match scene.camera:
                case bpy.types.Object(data=bpy.types.Camera(lens=lens, dof=bpy.types.CameraDOFSettings(aperture_fstop=fstop))):
                    optics = Some(Optics(focal_length=lens / 1000.0, f_number=fstop))
                case _:
                    optics = Nothing
            return LinearRaw(make="Blender", model=bpy.app.version_string, exposure=exposure, optics=optics).written(target, frame, config).map(lambda _: Done.wrote((target,)))

        def exported(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            return paired(saved(scene).bind(lambda path: combined(path, Some(layer.name))), self.gain(call.identity, scene).map(lambda gained: viewed(scene).exposure + gained)).bind(
                lambda found: wrote(scene, found[0][1][..., :3], found[1], PyOpenColorIO.GetCurrentConfig())
            )

        return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda roots: exported(*roots))

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns export commands and render production tracking."""
        return Ok((self.linear, self.still, *self.layered, PRODUCTIONS))


# --- [OPERATIONS] -----------------------------------------------------------------------


def export_still(call: Call[Still]) -> Outcome[Done]:
    """Write render or Viewer image with chosen encoding."""
    properties, target, config = call.properties, str(call.paths[0]), PyOpenColorIO.GetCurrentConfig()
    depth: Depth = Depth[properties.depth]
    compressed: Option[Compressions] = Option.of_optional(_COMPRESSIONS.get(properties.compression)).map(lambda form: form())

    def stored(file: ImageFile[np.float32]) -> Outcome[str]:
        match depth:
            case Depth.UINT8 | Depth.UINT16 as integer:
                return ImageFile(pixels=integer.coded(unassociated(file.pixels), Rounded()), colorspace=file.colorspace).write(target, Storage(compression=compressed), stamped(call.identity), config)
            case Depth.HALF | Depth.FLOAT as floated:
                return file.write(target, Storage(depth=16 if floated is Depth.HALF else 32, compression=compressed), stamped(call.identity), config)

    def encoded(file: ImageFile[np.float32]) -> Outcome[ImageFile[np.float32]]:
        return (
            Ok(file)
            if config.getColorSpace(file.colorspace).isData()
            else display_space(config, properties.display).bind(
                lambda space: converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=space), file.pixels).map(lambda pixels: ImageFile(pixels=pixels, colorspace=space))
            )
        )

    def image(native: bpy.types.Image, origin: Production) -> Outcome[ImageFile[np.float32]]:
        pixels = origin.layer.map(lambda layer: origin.scene.resolve().bind(saved).bind(lambda path: combined(path, Some(layer))).map(lambda found: found[1])).default_with(
            lambda: read(native, config).map(lambda file: file.pixels)
        )
        return (
            origin.view
            .filter(lambda _: properties.save_as_render)
            .map(lambda view: pixels.bind(lambda values: formed(view, values, origin.space, config)))
            .default_with(lambda: pixels.map(lambda values: ImageFile(pixels=values, colorspace=origin.space)))
        )

    return (
        _chosen(properties.format)
        .bind(lambda chosen: sample(chosen, depth))
        .bind(lambda _: built(properties.source))
        .bind(lambda native: production(call.identity, native).bind(lambda origin: image(native, origin)))
        .bind(encoded)
        .bind(stored)
        .map(lambda _: Done.wrote((target,)))
    )


def _chosen(name: str) -> Outcome[Format]:
    """Returns the named writer, absent fault for a format OpenImageIO does not write."""
    return writer(name).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject="OpenImageIO.extension_list", member=name),))


def plane(path: str, names: tuple[str, ...], channels: tuple[str, ...]) -> Outcome[npt.NDArray[np.float32]]:
    """Reads first requested layer, a fault per missing channel."""

    def picked(file: Multilayer) -> Outcome[npt.NDArray[np.float32]]:
        def channel(found: tuple[Channel, ...], layer: str, name: str) -> Outcome[Channel]:
            return Option.of_optional(next((held for held in found if held.name == name), None)).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=f"{path}:{layer}", member=name),))

        chosen: Outcome[Layer] = Option.of_optional(next((found for name in names for found in file.layers if found.name == name), None)).to_result_with(
            lambda: (Fault(reason=BlenderReason.ABSENT, subject=path, member=names[-1]),)
        )
        return Result[Layer, Faults].bind(chosen, lambda found: accumulated(channel(found.channels, found.name, name) for name in channels).bind(lambda held: file.plane(found, held)))

    return Multilayer.read(path).bind(picked)


def combined(path: str, layer: Option[str] = Nothing) -> Outcome[tuple[str, npt.NDArray[np.float32]]]:
    """Reads a saved render's Combined RGBA with its owner.

    Args:
        path: Saved multilayer render.
        layer: View layer to read, else the compositor's output, then the unnamed sequencer layer, then the first view layer.

    Returns:
        Owner name and RGBA plane, absent fault without a matching Combined layer.
    """

    def selected(file: Multilayer) -> Outcome[tuple[str, npt.NDArray[np.float32]]]:
        owners = {owner: found for found in file.layers for owner, _, kind in (found.name.rpartition("."),) if kind in {COMBINED, ""}}
        rendered = (*(owner for owner in (COMPOSITE, "") if owner in owners), *tuple(owner for owner in owners if owner not in {COMPOSITE, ""})[:1])
        match layer.map(lambda name: (name,)).default_value(rendered):
            case (owner, *_) if owner in owners:
                return plane(path, (owners[owner].name,), ("R", "G", "B", "A")).map(lambda pixels: (owner, pixels))
            case _:
                return Error((Fault(reason=BlenderReason.ABSENT, subject=path, member=layer.map(lambda name: f"{name}.{COMBINED}").default_value(COMBINED)),))

    return Multilayer.read(path).bind(selected)


def formed(view: DisplayView, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> Outcome[ImageFile[np.float32]]:
    """Forms a plane in `source` through a view into the view's display colorspace."""
    return paired(
        view.form(config, plane, source), view.colorspace(config).to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(config), member=f"{view.display}/{view.view}"),))
    ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["COMBINED", "COMPOSITE", "Documents", "Export", "Layering", "Sheet", "Still", "combined", "export_still", "formed", "plane"]
