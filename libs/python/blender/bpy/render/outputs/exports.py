"""Displayed-render exports as Linear Raw DNG, tagged stills, and layered documents, and the Combined plane of saved renders."""

from typing import Annotated, Final, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Save
from blender.bpy.data.ids.images import read
from blender.bpy.data.rna.conditions import AllOf, Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderResult
from blender.bpy.render.color.spaces import config_of
from blender.bpy.render.effects.rows import CompositorEffect, stops
from blender.bpy.render.outputs.finishing import stamped
from blender.bpy.render.outputs.formats import display_space, displays, readouts, sample, writer, writers
from blender.bpy.render.outputs.results import built, Production, production, PRODUCTIONS, saved
from blender.bpy.settings.properties import Choices, RecordGroup
from blender.bpy.ui.layout.rows import alerted, placed, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest
from blender.imaging.color.processors import converted, DisplayView, unassociated
from blender.imaging.effects.tone.exposure import Exposure
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.dng import LinearRaw, Optics
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Compressions, Deflate, Depth, Format, ImageFile, Storage
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.imaging.parameters import Choice, Gated

# --- [TYPES] ----------------------------------------------------------------------------


class Documents(Protocol):
    """Layered document source over a saved render."""

    def document(self, identity: Identity, /, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds layered document."""
        ...


class Source(Choice):
    """Combined plane owner, a saved render's output or a view layer ahead of scene effects."""

    RENDER = 1, "Stored Render"
    LAYER = 2, "View Layer"


# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = bpy.types.ViewLayer.bl_rna.properties["use_pass_combined"].name
COMPOSITE: Final = "Composite"
IMAGE_TYPE: Final = Descriptor.declared(bpy.types.SpaceImageEditor, "image.type", bpy.types.EnumProperty)
STILLS: Final = ("RENDER_RESULT", "COMPOSITING")

# --- [MODELS] ---------------------------------------------------------------------------


class Still(msgspec.Struct, frozen=True, kw_only=True):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: Annotated[str, msgspec.Meta(description="Image the still writes"), Choices.mirrored(IMAGE_TYPE.prop, lambda kind: kind in STILLS)] = "RENDER_RESULT"
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


class Layering(msgspec.Struct, frozen=True, kw_only=True):
    """Document source with its export action."""

    key: Key
    title: str
    description: str
    source: Documents

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


class Export:
    """Render and Viewer export actions."""

    gain: CompositorEffect[Exposure]
    linear: Operator[Empty]
    still: Operator[Still]
    layered: tuple[Operator[Sheet], ...]

    def __init__(self, *, gain: CompositorEffect[Exposure], layering: tuple[Layering, ...] = ()) -> None:
        """Declares linear raw, still, and layered document commands on save browser without undo."""
        self.gain = gain
        idle = Job(kind="RENDER")
        result = Some(AllOf(conditions=(Setting(descriptor=IMAGE_TYPE, accepted=("RENDER_RESULT",)), idle)))
        self.linear = Operator(
            key="export_dng",
            title="Export Linear Raw DNG",
            description="Write scene-linear render as DNG",
            form=Empty,
            run=self.raw,
            condition=result,
            invoke=Some(Save[Empty](extensions=lambda _: (".dng",))),
            undo=False,
        )
        self.still = Operator(
            key="export_still",
            title="Export Still",
            description="Write render or Viewer image with chosen encoding",
            form=Still,
            run=self.exported,
            condition=Some(idle),
            invoke=Some(Save[Still](extensions=Still.extensions)),
            seed=Some(Still.seeded),
            draw=Some(self.drawn),
            undo=False,
        )
        self.layered = tuple(
            Operator(
                key=each.key, title=each.title, description=each.description, form=Sheet, run=each.exported, condition=result, invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))), undo=False
            )
            for each in layering
        )

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
            return paired(
                saved(scene).bind(lambda path: combined(path, Source.LAYER, Some(layer.name))),
                stops(call.identity, scene, self.gain).map(lambda gained: scene.view_settings.exposure + gained),  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                config_of(),
            ).bind(lambda found: wrote(scene, found[0][1][..., :3], found[1], found[2]))

        return paired(call.root(bpy.types.Scene), call.root(bpy.types.ViewLayer)).bind(lambda roots: exported(*roots))

    def exported(self, call: Call[Still]) -> BlenderResult[Done]:
        """Writes render or Viewer image with chosen encoding."""
        record, target = call.record, str(call.paths[0])
        compressed: Option[Compressions] = Some(record.compression.value) if record.compression.enabled else Nothing

        def stored(file: ImageFile[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[str]:
            match record.depth:
                case Depth.UINT8 | Depth.UINT16 as integer:
                    return ImageFile(pixels=integer.coded(unassociated(file.pixels)), colorspace=file.colorspace).write(target, Storage(compression=compressed), stamped(call.identity), config)
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
            pixels = origin.layer.map(lambda layer: origin.scene.resolve().bind(saved).bind(lambda path: combined(path, Source.LAYER, Some(layer))).map(lambda found: found[1])).default_with(
                lambda: read(native, config).map(lambda file: file.pixels)
            )
            match origin.view:
                case Option(tag="some", some=(view, exposure, gamma)) if record.save_as_render:
                    return pixels.bind(
                        lambda values: paired(
                            view.form(config, values, source=origin.space, exposure=exposure, gamma=gamma),
                            view.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),)),
                        )
                    ).map(lambda found: ImageFile(pixels=found[0], colorspace=found[1].getName()))
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

    def acquisitions(self, _identity: Identity) -> BlenderResult[Acquisitions]:
        """Returns export commands and render production tracking."""
        return Ok((self.linear, self.still, *self.layered, PRODUCTIONS))


# --- [OPERATIONS] -----------------------------------------------------------------------


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns image-editor condition over displayed image type."""
    return Setting(descriptor=IMAGE_TYPE, accepted=kinds)


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


def combined(path: str, source: Source, layer: Option[str] = Nothing) -> BlenderResult[tuple[str, npt.NDArray[np.float32]]]:
    """Reads a saved render's Combined RGBA with its owner.

    Args:
        path: Saved multilayer render.
        source: `Source.RENDER` reads the compositor's output, else the unnamed sequencer layer, else the first view layer; `Source.LAYER` the view layer alone.
        layer: View layer `Source.LAYER` reads, the first one without it.

    Returns:
        Owner name and RGBA plane, absent fault without a matching Combined layer.
    """

    def selected(file: Multilayer) -> BlenderResult[tuple[str, npt.NDArray[np.float32]]]:
        owners = {owner: found for found in file.layers for owner, _, kind in (found.name.rpartition("."),) if kind in {COMBINED, ""}}
        views = layer.map(lambda name: (name,)).default_with(lambda: tuple(owner for owner in owners if owner not in {COMPOSITE, ""})[:1])
        match views if source is Source.LAYER else (*(owner for owner in (COMPOSITE, "") if owner in owners), *views):
            case (owner, *_) if owner in owners:
                return plane(path, (owners[owner].name,), ("R", "G", "B", "A")).map(lambda pixels: (owner, pixels))
            case _:
                return Error((BlenderAbsentFault(owner=path, key=layer.map(lambda name: f"{name}.{COMBINED}").default_value(COMBINED)),))

    file: BlenderResult[Multilayer] = Multilayer.read(path)
    return Result[Multilayer, BlenderFaults].bind(file, selected)


def displayed(
    formation: CompositorEffect[DisplayView], identity: Identity, scene: bpy.types.Scene, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config
) -> BlenderResult[ImageFile[np.float32]]:
    """Forms display values through owned formation entry, else scene view with its exposure and gamma."""

    def named(view: DisplayView, pixels: npt.NDArray[np.float32]) -> BlenderResult[ImageFile[np.float32]]:
        return (
            view
            .colorspace(config)
            .map(lambda space: ImageFile(pixels=pixels, colorspace=space.getName()))
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),))
        )

    def formed(shown: Option[DisplayView]) -> BlenderResult[ImageFile[np.float32]]:
        def native() -> BlenderResult[ImageFile[np.float32]]:
            exposure, gamma = scene.view_settings.exposure, scene.view_settings.gamma  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
            return formation.seeded(scene).bind(
                lambda view: Result[npt.NDArray[np.float32], BlenderFaults].bind(view.form(config, plane, source=source, exposure=exposure, gamma=gamma), lambda pixels: named(view, pixels))
            )

        return shown.map(
            lambda view: Result[npt.NDArray[np.float32], BlenderFaults].bind(
                converted(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR), plane), lambda pixels: named(view, pixels)
            )
        ).default_with(native)

    return formation.current(identity, scene).bind(formed)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["COMBINED", "COMPOSITE", "IMAGE_TYPE", "STILLS", "Documents", "Export", "Layering", "Sheet", "Source", "Still", "combined", "displayed", "image_type", "plane"]
