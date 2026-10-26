"""Displayed-render exports as Linear Raw DNG, tagged stills, and layered documents, and the Combined plane of saved renders."""

from collections.abc import Callable
from functools import cached_property
from typing import Annotated, Final, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Save
from blender.bpy.data.ids.images import read
from blender.bpy.data.rna.conditions import AllOf, Job
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.render.color.spaces import config_of, viewed
from blender.bpy.render.outputs.finishing import stamped
from blender.bpy.render.outputs.formats import display_space, displays, readouts, sample, writer, writers
from blender.bpy.render.outputs.results import built, Production, production, PRODUCTIONS, saved
from blender.bpy.settings.properties import Choices, RecordGroup
from blender.bpy.ui.images.views import RESULT
from blender.bpy.ui.layout.rows import alerted, placed, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest
from blender.imaging.color.processors import converted, DisplayView
from blender.imaging.files.dng import LinearRaw, Optics
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Compressions, Deflate, Depth, Format, ImageFile, Rounded, Storage, unassociated
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.records.faults import accumulated, paired
from blender.records.fields import Gated, heading

# --- [TYPES] ----------------------------------------------------------------------------


class Documents(Protocol):
    """Layered document source over a saved render."""

    def document(self, identity: Identity, /, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds layered document."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = bpy.types.ViewLayer.bl_rna.properties["use_pass_combined"].name
COMPOSITE: Final = "Composite"
STILLS: Final = ("RENDER_RESULT", "COMPOSITING")
IDLE: Final = Job(kind="RENDER")
SAVABLE: Final = AllOf(conditions=(RESULT, IDLE))

# --- [MODELS] ---------------------------------------------------------------------------


class Still(msgspec.Struct, frozen=True, kw_only=True):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: Annotated[
        str, msgspec.Meta(description="Image the still writes"), Choices.mirrored(Descriptor.declared(bpy.types.Image, "type", bpy.types.EnumProperty).prop, lambda kind: kind in STILLS)
    ] = "RENDER_RESULT"
    format: Annotated[str, msgspec.Meta(description="File format the still writes"), writers] = "png"
    depth: Annotated[Depth, msgspec.Meta(description="Storage of each channel")] = Depth.UINT8
    display: Annotated[str, msgspec.Meta(description="Display encoding at selected depth"), displays] = msgspec.field(default_factory=lambda: PyOpenColorIO.GetCurrentConfig().getDefaultDisplay())
    save_as_render: Annotated[bool, msgspec.Meta(description="Apply view, exposure, and gamma to unformed source")] = True
    compression: Annotated[Gated[Compressions], msgspec.Meta(description="Writer compression, writer default when off")] = Gated(enabled=False, value=Deflate())

    def seeded(self, _event: bpy.types.Event, _identity: Identity) -> Self:
        """Keeps writer-supported depth and compression on browser open."""
        return (
            writer(self.format)
            .map(
                lambda row: msgspec.structs.replace(
                    self,
                    depth=self.depth if self.depth in row.depths else row.depths[0],
                    compression=msgspec.structs.replace(self.compression, enabled=self.compression.enabled and type(self.compression.value) in row.compressions),
                )
            )
            .default_value(self)
        )

    def extensions(self) -> tuple[str, *tuple[str, ...]]:
        """Returns chosen writer's suffixes, or unknown writer name as suffix."""

        def suffixes(row: Format) -> tuple[str, *tuple[str, ...]]:
            first, *rest = row.extensions
            return (f".{first}", *(f".{suffix}" for suffix in rest))

        return writer(self.format).map(suffixes).default_value((f".{self.format}",))


class Sheet(msgspec.Struct, frozen=True, kw_only=True):
    """Channel storage selected for a layered TIFF export."""

    depth: Annotated[Depth, msgspec.Meta(description="Storage of each layer channel")] = Depth.UINT16


class Layering(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Layered TIFF export of a document source over the saved frame, its command named for the source's kind."""

    source: Documents

    @cached_property
    def command(self) -> Operator[Sheet]:
        """Command writing the source's document of the displayed render."""
        kind = heading(type(self.source).__name__)
        return Operator(
            key=f"export_{'_'.join(kind.lower().split())}",
            title=f"Export {kind} Document",
            description=f"Write {kind.lower()} document of displayed render as layered TIFF",
            form=Sheet,
            run=self.exported,
            condition=Some(SAVABLE),
            invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))),
            undo=False,
        )

    def exported(self, call: Call[Sheet]) -> BlenderResult[Done]:
        """Writes layered TIFF from saved frame."""
        target = str(call.paths[0])
        return (
            call
            .root(bpy.types.Scene)
            .bind(
                lambda scene: paired(saved(scene).bind(Multilayer.read), config_of()).bind(
                    lambda pair: self.source.document(call.identity, scene, pair[0], call.record.depth).bind(lambda document: document.written(target, pair[1]))
                )
            )
            .map(lambda _: Done.wrote((target,)))
        )


class Export(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Render and Viewer export actions, the DNG exposure adding `gain` stops to the scene view's exposure."""

    gain: Callable[[Identity, bpy.types.Scene], BlenderResult[float]] = lambda _identity, _scene: Ok(0.0)
    layering: tuple[Layering, ...] = ()

    # --- [COMMANDS]

    @cached_property
    def linear(self) -> Operator[Empty]:
        """Command writing the scene-linear render as DNG."""
        return Operator(
            key="export_dng",
            title="Export Linear Raw DNG",
            description="Write scene-linear render as DNG",
            form=Empty,
            run=self.raw,
            condition=Some(SAVABLE),
            invoke=Some(Save[Empty](extensions=lambda _: (".dng",))),
            undo=False,
        )

    @cached_property
    def still(self) -> Operator[Still]:
        """Command writing the render or Viewer image with chosen encoding."""
        return Operator(
            key="export_still",
            title="Export Still",
            description="Write render or Viewer image with chosen encoding",
            form=Still,
            run=self.exported,
            condition=Some(IDLE),
            invoke=Some(Save[Still](extensions=Still.extensions)),
            seed=Some(Still.seeded),
            draw=Some(self.drawn),
            undo=False,
        )

    @property
    def layered(self) -> tuple[Operator[Sheet], ...]:
        """Layered document commands in layering order."""
        return tuple(each.command for each in self.layering)

    # --- [WRITES]

    def raw(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Writes scene-linear Combined RGB with total exposure and camera optics."""
        target = str(call.paths[0])

        def wrote(scene: bpy.types.Scene, frame: npt.NDArray[np.float32], exposure: float, config: PyOpenColorIO.Config) -> BlenderResult[Done]:
            match scene.camera:
                case bpy.types.Object(data=bpy.types.Camera(lens=lens, dof=bpy.types.CameraDOFSettings(aperture_fstop=fstop))):
                    optics = Some(Optics(focal_length=lens / 1000.0, f_number=fstop))
                case _:
                    optics = Nothing
            return LinearRaw(make="Blender", model=bpy.app.version_string, exposure=exposure, optics=optics).written(target, frame, config).map(lambda _: Done.wrote((target,)))

        def exported(scene: bpy.types.Scene, layer: bpy.types.ViewLayer) -> BlenderResult[Done]:
            return paired(saved(scene).bind(lambda path: combined(path, Some(layer.name))), self.gain(call.identity, scene).map(lambda gained: viewed(scene).exposure + gained), config_of()).bind(
                lambda found: wrote(scene, found[0][1][..., :3], found[1], found[2])
            )

        return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda roots: exported(*roots))

    def exported(self, call: Call[Still]) -> BlenderResult[Done]:
        """Writes render or Viewer image with chosen encoding."""
        record, target = call.record, str(call.paths[0])
        compressed: Option[Compressions] = Some(record.compression.value) if record.compression.enabled else Nothing

        def stored(file: ImageFile[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[str]:
            match record.depth:
                case Depth.UINT8 | Depth.UINT16 as integer:
                    return ImageFile(pixels=integer.coded(unassociated(file.pixels), Rounded()), colorspace=file.colorspace).write(
                        target, Storage(compression=compressed), stamped(call.identity), config
                    )
                case Depth.HALF | Depth.FLOAT as floated:
                    return file.write(target, Storage(depth=16 if floated is Depth.HALF else 32, compression=compressed), stamped(call.identity), config)

        def encoded(file: ImageFile[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
            return (
                Ok(file)
                if config.getColorSpace(file.colorspace).isData()
                else display_space(config, record.display).bind(
                    lambda space: converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=space), file.pixels).map(lambda pixels: ImageFile(pixels=pixels, colorspace=space))
                )
            )

        def image(native: bpy.types.Image, origin: Production, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
            pixels = origin.layer.map(lambda layer: origin.scene.resolve().bind(saved).bind(lambda path: combined(path, Some(layer))).map(lambda found: found[1])).default_with(
                lambda: read(native, config).map(lambda file: file.pixels)
            )
            match origin.view:
                case Option(tag="some", some=view) if record.save_as_render:
                    return pixels.bind(lambda values: formed(view, values, origin.space, config))
                case _:
                    return pixels.map(lambda values: ImageFile(pixels=values, colorspace=origin.space))

        selected: BlenderResult[Format] = writer(record.format).to_result_with(lambda: (BlenderAbsentFault(owner="OpenImageIO.extension_list", key=record.format),))
        return (
            Result[Format, BlenderFaults]
            .bind(selected, lambda chosen: sample(chosen, record.depth))
            .bind(lambda _: config_of())
            .bind(
                lambda config: (
                    built(record.source)
                    .bind(lambda native: production(call.identity, native).bind(lambda origin: image(native, origin, config)))
                    .bind(lambda file: encoded(file, config))
                    .bind(lambda file: stored(file, config))
                )
            )
            .map(lambda _: Done.wrote((target,)))
        )

    def drawn(self, layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties, identity: Identity) -> None:
        """Draws still encoding fields with writer readouts."""

        def controls(binding: RecordGroup[Still, bpy.types.Operator]) -> BlenderResult[tuple[tuple[RowDraw, ...], Nest]]:
            return fields(binding, identity).bind(lambda parts: placed(parts[0], identity).map(lambda rows: (rows, parts[1])))

        bound = self.still.bound(identity).map(lambda pair: pair[1])
        form = bound.bind(controls)
        for rows, nested in form.to_option().to_list():
            for draw in rows:
                draw(layout, properties, context)
            nested(layout, properties, context, self.still.key)
        verdicts = bound.bind(lambda binding: binding.read(properties)).bind(
            lambda record: paired(writer(record.format).to_result_with(lambda: (BlenderAbsentFault(owner="OpenImageIO.extension_list", key=record.format),)), config_of()).map(
                lambda pair: readouts(pair[0], record.depth, record.display, pair[1])
            )
        )
        for readings in verdicts.to_option().to_list():
            for row in readings:
                layout.label(text=row.text)
        alerted(layout, paired(form, verdicts))

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns export commands and render production tracking."""
        return Ok((self.linear, self.still, *self.layered, PRODUCTIONS))


# --- [OPERATIONS] -----------------------------------------------------------------------


def plane(path: str, names: tuple[str, ...], channels: tuple[str, ...]) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads first requested layer, a fault per missing channel."""

    def picked(file: Multilayer) -> BlenderResult[npt.NDArray[np.float32]]:
        def channel(found: tuple[Channel, ...], layer: str, name: str) -> BlenderResult[Channel]:
            return Option.of_optional(next((held for held in found if held.name == name), None)).to_result_with(lambda: (BlenderAbsentFault(owner=f"{path}:{layer}", key=name),))

        chosen: BlenderResult[Layer] = Option.of_optional(next((found for name in names for found in file.layers if found.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=path, key=names[-1]),)
        )
        return Result[Layer, BlenderFaults].bind(
            chosen, lambda found: Result[tuple[Channel, ...], BlenderFaults].bind(accumulated(channel(found.channels, found.name, name) for name in channels), lambda chosen: file.plane(found, chosen))
        )

    return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path), picked)


def combined(path: str, layer: Option[str] = Nothing) -> BlenderResult[tuple[str, npt.NDArray[np.float32]]]:
    """Reads a saved render's Combined RGBA with its owner.

    Args:
        path: Saved multilayer render.
        layer: View layer to read, else the compositor's output, then the unnamed sequencer layer, then the first view layer.

    Returns:
        Owner name and RGBA plane, absent fault without a matching Combined layer.
    """

    def selected(file: Multilayer) -> BlenderResult[tuple[str, npt.NDArray[np.float32]]]:
        owners = {owner: found for found in file.layers for owner, _, kind in (found.name.rpartition("."),) if kind in {COMBINED, ""}}
        rendered = (*(owner for owner in (COMPOSITE, "") if owner in owners), *tuple(owner for owner in owners if owner not in {COMPOSITE, ""})[:1])
        match layer.map(lambda name: (name,)).default_value(rendered):
            case (owner, *_) if owner in owners:
                return plane(path, (owners[owner].name,), ("R", "G", "B", "A")).map(lambda pixels: (owner, pixels))
            case _:
                return Error((BlenderAbsentFault(owner=path, key=layer.map(lambda name: f"{name}.{COMBINED}").default_value(COMBINED)),))

    file: BlenderResult[Multilayer] = Multilayer.read(path)
    return Result[Multilayer, BlenderFaults].bind(file, selected)


def formed(view: DisplayView, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
    """Forms a plane in `source` through a view into the view's display colorspace."""
    return paired(view.form(config, plane, source), view.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),))).map(
        lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName())
    )


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["COMBINED", "COMPOSITE", "IDLE", "SAVABLE", "STILLS", "Documents", "Export", "Layering", "Sheet", "Still", "combined", "formed", "plane"]
