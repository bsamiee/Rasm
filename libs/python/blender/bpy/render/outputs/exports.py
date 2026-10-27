"""Displayed-render exports as Linear Raw DNG, tagged stills, and layered documents, and the Combined plane of saved renders."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
import math
from pathlib import Path
import sys
from types import MappingProxyType
import typing
from typing import Annotated, Final, Protocol

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Save
from blender.bpy.data.ids.images import read
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import choices, Descriptor, Value, written
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
from blender.imaging.files.buffers import resampled, unassociated
from blender.imaging.files.coding import Depth, Rounded
from blender.imaging.files.documents import Document, LinearRaw, Optics
from blender.imaging.files.images import Compressions, Format, ImageFile, read_file, Storage
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------


type _Downscale = Scaled | LongestSide


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
_DEPTHS: Final = choices((depth.name, depth.name, "") for depth in Depth)
SOURCES: Final = choices(item for item in Descriptor.declared(bpy.types.Image, "type", bpy.types.EnumProperty).prop.enum_items if item.identifier in _STILLS)
WRITERS: Final = choices((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in Format.writable())
CODECS: Final = choices((("DEFAULT", "Default", ""), *((name, name, "") for name in _COMPRESSIONS)))

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
                        .bind(lambda document: document.saved(target, PyOpenColorIO.GetCurrentConfig()))
                    )
                )
                .map(lambda _: Done.wrote((target,)))
            )

        exported.__name__ = exported.__qualname__ = f"export_{type(self.source).__name__.lower()}"
        return Operator(form=Sheet, run=exported, condition=Some(_SAVABLE), invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))), undo=False)


class Scaled(Case, frozen=True, kw_only=True):
    """Proxy at fraction of each source side."""

    factor: Annotated[float, msgspec.Meta(ge=sys.float_info.min, lt=1.0)] = 0.5

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns each scaled side rounded up to whole pixel."""
        return Some((math.ceil(width * self.factor), math.ceil(height * self.factor)))


class LongestSide(Case, frozen=True, kw_only=True):
    """Longest-side proxy bound."""

    pixels: Annotated[int, msgspec.Meta(ge=1, le=2**31 - 1)] = 2048

    def fitted(self, width: int, height: int) -> Option[tuple[int, int]]:
        """Returns sides rounded up, or Nothing for source within bound."""
        longest = max(width, height)
        return Some((width, height)).filter(lambda _: longest > self.pixels).map(lambda sides: ((sides[0] * self.pixels + longest - 1) // longest, (sides[1] * self.pixels + longest - 1) // longest))


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
                case bpy.types.Object(data=bpy.types.Camera(lens=lens, dof=bpy.types.CameraDOFSettings(aperture_fstop=fstop))) as camera:
                    optics = _optics(lens, fstop).map(Some).map_error(partial(Fault.of, RecordReason.OUTSIDE, repr(camera), member=Optics.__name__))
                case _:
                    optics = Ok(Nothing)
            return optics.bind(lambda held: LinearRaw(make="Blender", model=bpy.app.version_string, exposure=exposure, optics=held).saved(target, frame, config).map(lambda _: Done.wrote((target,))))

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

# --- [STILLS]


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
                return file.write(target, Storage(depth=floated, compression=compressed), stamped(call.identity), config)

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
    return writer(name).to_result_with(partial(Fault.of, BlenderReason.ABSENT, "OpenImageIO.extension_list", member=name))


@catch(exception=msgspec.ValidationError)
def _optics(lens: float, fstop: float) -> Optics:
    """Returns camera optics of a focal length in millimeters and an f-number, refusing a value outside the positive bounds."""
    return msgspec.convert({"focal_length": lens / 1000.0, "f_number": fstop}, Optics)


# --- [PLANES]


def plane(path: str, names: tuple[str, ...], channels: tuple[str, ...]) -> Outcome[npt.NDArray[np.float32]]:
    """Reads first requested layer, a fault per missing channel."""

    def picked(file: Multilayer) -> Outcome[npt.NDArray[np.float32]]:
        def channel(found: tuple[Channel, ...], layer: str, name: str) -> Outcome[Channel]:
            return Block.of_seq(found).filter(lambda held: held.name == name).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, f"{path}:{layer}", member=name))

        chosen: Outcome[Layer] = (
            Block
            .of_seq(names)
            .choose(lambda name: Block.of_seq(file.layers).filter(lambda found: found.name == name).try_head())
            .try_head()
            .to_result_with(partial(Fault.of, BlenderReason.ABSENT, path, member=names[-1]))
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
                return Error(Fault.of(BlenderReason.ABSENT, path, member=layer.map(lambda name: f"{name}.{COMBINED}").default_value(COMBINED)))

    return Multilayer.read(path).bind(selected)


def formed(view: DisplayView, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> Outcome[ImageFile[np.float32]]:
    """Forms a plane in `source` through a view into the view's display colorspace."""
    return paired(view.form(config, plane, source), view.colorspace(config).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, repr(config), member=f"{view.display}/{view.view}"))).map(
        lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName())
    )


# --- [PROXIES]


def proxied(image: bpy.types.Image, identity: Identity, downscale: _Downscale, config: PyOpenColorIO.Config) -> Outcome[Option[str]]:
    """Points image at a linear-light proxy beside its source file, Nothing when the source fits."""

    def chosen(path: str) -> Outcome[Format]:
        return (
            Option
            .of_optional(OpenImageIO.ImageOutput.create(path))
            .to_result_with(lambda: Fault.of(BlenderReason.ABSENT, path, OpenImageIO.geterror()))
            .bind(lambda output: writer(output.format_name()).to_result_with(lambda: Fault.of(BlenderReason.ABSENT, path, member=output.format_name())))
        )

    def linear(file: ImageFile[np.float32], width: int, height: int) -> Outcome[npt.NDArray[np.float32]]:
        def moved(values: npt.NDArray[np.float32], direction: PyOpenColorIO.TransformDirection) -> Outcome[npt.NDArray[np.float32]]:
            transform = PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR, direction=direction)
            return converted(config, transform, values) if values.shape[2] in {3, 4} else Ok(values)

        return moved(file.pixels, PyOpenColorIO.TRANSFORM_DIR_FORWARD).bind(lambda values: resampled(values, (width, height))).bind(lambda values: moved(values, PyOpenColorIO.TRANSFORM_DIR_INVERSE))

    def proxy(key: str, source: str, row: Format, file: ImageFile[np.float32], width: int, height: int) -> Outcome[str]:
        origin = Path(source)
        path, depth = str(origin.with_stem(f"{origin.stem}.{width}x{height}")), row.depths[-1] if image.is_float else Depth.UINT8

        def stored(values: npt.NDArray[np.float32]) -> Outcome[str]:
            match depth:
                case Depth.UINT8 | Depth.UINT16 as integer:
                    return ImageFile(pixels=integer.coded(unassociated(values), Rounded()), colorspace=file.colorspace, header=file.header).write(path, Storage(), {}, config)
                case Depth.HALF | Depth.FLOAT as floated:
                    return ImageFile(pixels=values, colorspace=file.colorspace, header=file.header).write(path, Storage(depth=floated), {}, config)

        return (
            linear(file, width, height)
            .bind(stored)
            .bind(lambda _: catch(image.__setitem__, exception=TypeError)(key, source).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image))))
            .bind(lambda _: written(image, ("filepath",), path).map(lambda _: path))
        )

    def sourced(key: str) -> Outcome[Option[str]]:
        source = Option.of_optional(try_downcast(str, image.get(key))).default_with(lambda: bpy.path.abspath(image.filepath, library=image.library))
        return chosen(source).bind(
            lambda row: read_file(source, config).bind(
                lambda file: downscale.fitted(file.pixels.shape[1], file.pixels.shape[0]).map(lambda size: proxy(key, source, row, file, *size).map(Some)).default_value(Ok(Nothing))
            )
        )

    return identity.identifier("proxy").bind(sourced)


def restored(image: bpy.types.Image, identity: Identity) -> Outcome[None]:
    """Points image back at its stored source path and removes the proxy property."""

    def back(key: str) -> Outcome[None]:
        return (
            Option
            .of_optional(try_downcast(str, image.get(key)))
            .map(
                lambda source: (
                    written(image, ("filepath",), source).bind(lambda _: catch(image.pop, exception=KeyError)(key).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image)))).map(lambda _: None)
                )
            )
            .default_value(Ok(None))
        )

    return identity.identifier("proxy").bind(back)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["COMBINED", "COMPOSITE", "Documents", "Export", "Layering", "LongestSide", "Scaled", "Sheet", "Still", "combined", "export_still", "formed", "plane", "proxied", "restored"]
