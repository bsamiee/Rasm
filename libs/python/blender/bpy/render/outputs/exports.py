"""Displayed-render exports as Linear Raw DNG, tagged stills, layered documents, and linear-light review proxies."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
import math
from pathlib import Path
import sys
from types import MappingProxyType
import typing
from typing import Annotated, Final

import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.invocations import Save
from blender.bpy.commands.operators import Call, Done, Empty, Operator
from blender.bpy.data.ids.images import read
from blender.bpy.data.rna.conditions import Job
from blender.bpy.data.rna.descriptors import Descriptor, Value, written
from blender.bpy.data.rna.mirrors import choices
from blender.bpy.data.rna.paths import rooted
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.color.spaces import UNTAGGED, viewed
from blender.bpy.render.outputs.results import built, combined, Production, PRODUCTIONS, ResultKind, saved
from blender.bpy.ui.images.views import RESULT
from blender.imaging.color.processors import basis, converted, DisplayView
from blender.imaging.files.buffers import Depth, fitted
from blender.imaging.files.documents import Document, LinearRaw, Optics
from blender.imaging.files.images import Compressions, Format, ImageFile, read_file, Storage
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [CONSTANTS] ------------------------------------------------------------------------

_IDLE: Final = Job(kind="RENDER")
_SAVABLE: Final = RESULT & _IDLE
_COMPRESSIONS: Final = MappingProxyType({form.__name__: form() for form in typing.get_args(Compressions.__value__)})
_DEPTHS: Final = choices((depth.name, depth.name, "") for depth in Depth)
_SOURCES: Final = choices(item for item in Descriptor.declared(bpy.types.Image, ("type",), bpy.types.EnumProperty).prop.enum_items if item.identifier in typing.get_args(ResultKind.__value__))
_WRITERS: Final = choices((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in Format.writable())
_CODECS: Final = choices((("DEFAULT", "Default", ""), *((name, name, "") for name in _COMPRESSIONS)))

# --- [MODELS] ---------------------------------------------------------------------------


class Still(bpy.types.PropertyGroup):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: bpy.props.EnumProperty(name="Source", description="Image the still writes", items=_SOURCES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    format: bpy.props.EnumProperty(name="Format", description="File format the still writes", items=_WRITERS, default="png")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each channel", items=_DEPTHS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    display: bpy.props.StringProperty(name="Display", description="Display encoding at selected depth", search=lambda _owner, _context, _edit: PyOpenColorIO.GetCurrentConfig().getDisplays())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    save_as_render: bpy.props.BoolProperty(name="Save as Render", description="Apply view, exposure, and gamma to unformed source", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    compression: bpy.props.EnumProperty(name="Compression", description="Writer compression at its defaults, writer default when unset", items=_CODECS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Sheet(bpy.types.PropertyGroup):
    """Channel storage selected for a layered TIFF export."""

    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each layer channel", items=_DEPTHS, default=Depth.UINT16.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Layering(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layered TIFF export of the document a builder makes of the saved frame, its command keyed `export_<key>`."""

    key: Key
    document: Callable[[Identity, bpy.types.Scene, Multilayer, Depth], Outcome[Document[np.uint8 | np.uint16 | np.float32]]]

    @cached_property
    def command(self) -> Operator[Sheet]:
        """Command writing the built document of the saved frame as layered TIFF, keyed by the layering."""

        def exported(call: Call[Sheet]) -> Outcome[Done]:
            """Write the built document of the saved frame as layered TIFF."""
            target = call.paths[0]
            return (
                rooted(bpy.types.Scene, call.context)
                .bind(
                    lambda scene: (
                        saved(scene)
                        .bind(lambda path: Multilayer.read(Path(path)))
                        .bind(lambda frame: self.document(call.identity, scene, frame, Depth[call.properties.depth]))
                        .bind(lambda document: document.written(target, PyOpenColorIO.GetCurrentConfig()))
                    )
                )
                .map(lambda _: Done.wrote((target,)))
            )

        return Operator.of(Sheet, condition=Some(_SAVABLE), invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))), undo=Nothing, named=Some(f"export_{self.key}"))(exported)


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
        return Operator.of(Empty, condition=Some(_SAVABLE), invoke=Some(Save[Empty](extensions=lambda _: (".dng",))), undo=Nothing)(self.export_dng)

    @cached_property
    def still(self) -> Operator[Still]:
        """Command writing the render or Viewer image with chosen encoding, its browser seeded with writer-supported values and its suffixes the writer's, the host drawing its properties."""

        def seeded(properties: Still, _event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
            supported = Format.writer(properties.format).map(lambda row: (row.depths, row.compressions)).default_value(((), ()))
            depth = Depth[properties.depth]
            return {
                "display": PyOpenColorIO.GetCurrentConfig().getDefaultDisplay(),
                "depth": (depth if depth in supported[0] or not supported[0] else supported[0][0]).name,
                "compression": properties.compression if _COMPRESSIONS.get(properties.compression) in supported[1] else "DEFAULT",
            }

        return Operator.of(
            Still,
            condition=Some(_IDLE),
            invoke=Some(Save[Still](extensions=lambda properties: Format.writer(properties.format).map(lambda row: row.suffixes).default_value((f".{properties.format}",)))),
            seed=Some(seeded),
            undo=Nothing,
        )(self.export_still)

    @property
    def layered(self) -> tuple[Operator[Sheet], ...]:
        """Layered document commands in layering order."""
        return tuple(each.command for each in self.layering)

    # --- [WRITES]

    def export_dng(self, call: Call[Empty]) -> Outcome[Done]:
        """Write scene-linear render as Linear Raw DNG with total exposure and camera optics."""
        target = call.paths[0]

        def wrote(scene: bpy.types.Scene, frame: npt.NDArray[np.float32], exposure: float, config: PyOpenColorIO.Config) -> Outcome[Done]:
            match scene.camera:
                case bpy.types.Object(data=bpy.types.Camera(lens=lens, dof=bpy.types.CameraDOFSettings(aperture_fstop=fstop))) as camera:
                    optics = _optics(lens, fstop).map(Some).map_error(partial(Fault.of, Reason.OUTSIDE, repr(camera), member=Optics.__name__))
                case _:
                    optics = Ok(Nothing)
            return paired(optics, basis(config, PyOpenColorIO.ROLE_SCENE_LINEAR)).bind(
                lambda found: LinearRaw(make="Blender", model=bpy.app.version_string, exposure=exposure, optics=found[0]).written(target, frame, found[1]).map(lambda _: Done.wrote((target,)))
            )

        def exported(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> Outcome[Done]:
            return paired(saved(scene).bind(lambda path: combined(path, Some(layer.name))), self.gain(call.identity, scene).map(lambda gained: viewed(scene).exposure + gained)).bind(
                lambda found: wrote(scene, found[0][1][..., :3], found[1], PyOpenColorIO.GetCurrentConfig())
            )

        return paired(rooted(bpy.types.Scene, call.context), rooted(bpy.types.ViewLayer, call.context)).bind(lambda roots: exported(*roots))

    @staticmethod
    def export_still(call: Call[Still]) -> Outcome[Done]:
        """Write render or Viewer image with chosen encoding."""
        properties, target, config = call.properties, call.paths[0], PyOpenColorIO.GetCurrentConfig()
        storage = Storage(depth=Depth[properties.depth], compression=Option.of_optional(_COMPRESSIONS.get(properties.compression)))

        def encoded(file: ImageFile) -> Outcome[ImageFile]:
            return (
                Ok(file)
                if config.getColorSpace(file.colorspace).isData()
                else DisplayView.encoding(config, properties.display).bind(
                    lambda space: converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=space), file.pixels).map(lambda pixels: ImageFile(pixels=pixels, colorspace=space))
                )
            )

        def image(native: bpy.types.Image, origin: Production) -> Outcome[ImageFile]:
            pixels = origin.layer.map(lambda layer: origin.scene.resolve().bind(saved).bind(lambda path: combined(path, Some(layer))).map(lambda found: found[1])).default_with(
                lambda: read(native, config).map(lambda file: file.pixels)
            )
            return (
                origin.view
                .filter(lambda _: properties.save_as_render)
                .map(lambda view: pixels.bind(lambda values: view.formed(config, values, origin.space)))
                .default_with(lambda: pixels.map(lambda values: ImageFile(pixels=values, colorspace=origin.space)))
            )

        return (
            Format
            .writer(properties.format)
            .to_result_with(partial(Fault.of, Reason.ABSENT, "OpenImageIO.extension_list", member=properties.format))
            .bind(lambda chosen: chosen.stored(storage.depth))
            .bind(lambda _: built(properties.source))
            .bind(lambda native: Production.shown(call.identity, native).bind(lambda origin: image(native, origin)))
            .bind(encoded)
            .bind(lambda file: file.written(target, storage, call.identity.manifest.stamp, config))
            .map(lambda _: Done.wrote((target,)))
        )

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns export commands and render production tracking."""
        return Ok((self.linear, self.still, *self.layered, PRODUCTIONS))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [OPTICS]


@catch(exception=msgspec.ValidationError)
def _optics(lens: float, fstop: float) -> Optics:
    """Returns camera optics of a focal length in millimeters and an f-number, refusing a value outside the positive bounds."""
    return msgspec.convert({"focal_length": lens / 1000.0, "f_number": fstop}, Optics)


# --- [PROXIES]


def proxied(image: bpy.types.Image, identity: Identity, downscale: Scaled | LongestSide, config: PyOpenColorIO.Config) -> Outcome[Option[str]]:
    """Points image at a linear-light proxy beside its source file, Nothing when the source fits."""

    def linear(file: ImageFile, width: int, height: int) -> Outcome[npt.NDArray[np.float32]]:
        def moved(values: npt.NDArray[np.float32], direction: PyOpenColorIO.TransformDirection) -> Outcome[npt.NDArray[np.float32]]:
            transform = PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=PyOpenColorIO.ROLE_SCENE_LINEAR, direction=direction)
            return converted(config, transform, values) if values.shape[2] in {3, 4} else Ok(values)

        return (
            moved(file.pixels, PyOpenColorIO.TRANSFORM_DIR_FORWARD)
            .bind(lambda values: fitted(values, (width, height), exact=True))
            .bind(lambda values: moved(values, PyOpenColorIO.TRANSFORM_DIR_INVERSE))
        )

    def proxy(key: str, source: Path, row: Format, file: ImageFile, width: int, height: int) -> Outcome[str]:
        target = source.with_stem(f"{source.stem}.{width}x{height}")
        depth = row.depths[-1] if image.is_float else Depth.UINT8
        return (
            linear(file, width, height)
            .bind(lambda values: ImageFile(pixels=values, colorspace=file.colorspace, header=file.header).written(target, Storage(depth=depth), {}, config))
            .bind(lambda _: catch(image.__setitem__, exception=TypeError)(key, str(source)).map_error(partial(Fault.of, Reason.REFUSED, repr(image))))
            .bind(lambda _: written(image, ("filepath",), str(target)).map(lambda _: str(target)))
        )

    def sourced(key: str) -> Outcome[Option[str]]:
        source = Path(Option.of_optional(try_downcast(str, image.get(key))).default_with(lambda: bpy.path.abspath(image.filepath, library=image.library)))
        return Format.of(source).bind(
            lambda row: read_file(source, config, untagged=Some(UNTAGGED)).bind(
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
                    written(image, ("filepath",), source).bind(lambda _: catch(image.pop, exception=KeyError)(key).map_error(partial(Fault.of, Reason.REFUSED, repr(image)))).map(lambda _: None)
                )
            )
            .default_value(Ok(None))
        )

    return identity.identifier("proxy").bind(back)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Export", "Layering", "LongestSide", "Scaled", "Sheet", "Still", "proxied", "restored"]
