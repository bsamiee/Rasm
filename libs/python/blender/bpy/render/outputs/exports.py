"""Displayed-render exports as Linear Raw DNG, tagged stills, and layered documents."""

from typing import Annotated, ClassVar, Protocol, Self

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from blender.bpy.commands.operators import Call, Done, Empty, Operator, Save
from blender.bpy.data.ids.images import built, Production, production, productions, read, saved
from blender.bpy.data.rna.conditions import AllOf, Job, Setting
from blender.bpy.data.rna.descriptors import Descriptor
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderImagingFault, BlenderResult
from blender.bpy.render.color.exposure import Gain
from blender.bpy.render.outputs.finishing import stamped
from blender.bpy.render.outputs.formats import display_space, displays, readouts, sample, writer, writers
from blender.bpy.settings.properties import RecordGroup
from blender.bpy.ui.layout.rows import alerted, placed, RowDraw
from blender.bpy.ui.layout.sections import fields, Nest
from blender.imaging.color.config import admitted
from blender.imaging.color.processors import applied, DisplayView, processor, unassociated
from blender.imaging.effects.quantization import codes
from blender.imaging.faults import accumulated, paired
from blender.imaging.files.dng import LinearRaw, Optics
from blender.imaging.files.documents import Document
from blender.imaging.files.images import Compressions, Deflate, Depth, Format, ImageFile, Storage
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.imaging.parameters import Choice, Gated

# --- [TYPES] ----------------------------------------------------------------------------


class Viewing(Protocol):
    """Formation entry's view and the native scene view used without an entry."""

    def current(self, identity: Identity, scene: bpy.types.Scene) -> BlenderResult[Option[DisplayView]]:
        """Returns the owned formation entry's view, absence, or its read faults."""
        ...

    def seeded(self, scene: bpy.types.Scene) -> BlenderResult[DisplayView]:
        """Returns the native scene's display and view."""
        ...


class Documents(Protocol):
    """Layered document source reading a saved render under the caller's identity."""

    def document(self, identity: Identity, scene: bpy.types.Scene, frame: Multilayer, depth: Depth) -> BlenderResult[Document[np.uint8 | np.uint16 | np.float32]]:
        """Builds the source's layered document or returns its read and formation faults."""
        ...


class Source(Choice):
    """Native image selected for a still export."""

    RESULT = 1, "Render Result"
    VIEWER = 2, "Viewer"


# --- [MODELS] ---------------------------------------------------------------------------


class Still(msgspec.Struct, frozen=True, kw_only=True):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: Annotated[Source, msgspec.Meta(description="Image the still writes")] = Source.RESULT
    format: Annotated[str, msgspec.Meta(description="File format the still writes"), writers] = "png"
    depth: Annotated[Depth, msgspec.Meta(description="Storage of each channel")] = Depth.UINT8
    display: Annotated[str, msgspec.Meta(description="Display encoding written at the selected channel depth"), displays] = msgspec.field(
        default_factory=lambda: PyOpenColorIO.GetCurrentConfig().getDefaultDisplay()
    )
    save_as_render: Annotated[bool, msgspec.Meta(description="Apply the view, exposure, and gamma to an unformed scene-linear source")] = True
    compression: Annotated[Gated[Compressions], msgspec.Meta(description="Compression the format's writer applies, the writer's own default while disabled")] = Gated(enabled=False, value=Deflate())

    def seeded(self, _event: bpy.types.Event) -> Self:
        """Retains writer-supported depth and compression when the browser opens."""
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
        """Returns the chosen writer's suffixes or the unknown writer name as its suffix."""

        def suffixes(row: Format) -> tuple[str, *tuple[str, ...]]:
            first, *rest = row.extensions
            return (f".{first}", *(f".{suffix}" for suffix in rest))

        return writer(self.format).map(suffixes).default_value((f".{self.format}",))


class Sheet(msgspec.Struct, frozen=True, kw_only=True):
    """Channel storage selected for a layered TIFF export."""

    depth: Annotated[Depth, msgspec.Meta(description="Storage of each layer channel")] = Depth.UINT16


class Layering(msgspec.Struct, frozen=True, kw_only=True):
    """One document source and the native action presenting its export."""

    key: Key
    title: str
    description: str
    source: Documents

    def exported(self, call: Call[Sheet]) -> BlenderResult[Done]:
        """Writes a layered TIFF after the saved frame and color configuration resolve."""
        scene, target = call.context.scene, str(call.paths[0])
        if scene is None:
            return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))
        return (
            paired(saved(scene).bind(lambda path: Multilayer.read(path).map_error(BlenderImagingFault.lifted)), config_of())
            .bind(lambda pair: self.source.document(call.identity, scene, pair[0], call.record.depth).bind(lambda document: document.written(target, pair[1]).map_error(BlenderImagingFault.lifted)))
            .map(lambda _: Done.wrote((target,)))
        )


class Export(msgspec.Struct, frozen=True, kw_only=True):
    """Render and Viewer export actions with the stack's view and exposure providers."""

    dng: ClassVar[Key] = "export_dng"
    image: ClassVar[Key] = "export_still"
    viewing: Viewing
    gain: Gain
    layering: tuple[Layering, ...] = ()

    def raw(self, call: Call[Empty]) -> BlenderResult[Done]:
        """Writes scene-linear Combined RGB with total exposure and the scene camera's optics."""
        scene, target = call.context.scene, str(call.paths[0])
        layer = call.context.view_layer
        match scene, layer:
            case bpy.types.Scene(), bpy.types.ViewLayer():
                pass
            case _:
                faults = tuple(BlenderAbsentFault(owner=repr(call.context), key=key) for key, value in (("scene", scene), ("view_layer", layer)) if value is None)
                first, *rest = faults
                return Error((first, *rest))
        match scene.camera:
            case bpy.types.Object(data=bpy.types.Camera(lens=lens, dof=bpy.types.CameraDOFSettings(aperture_fstop=fstop))):
                optics = Some(Optics(focal_length=lens / 1000.0, f_number=fstop))
            case _:
                optics = Nothing

        def wrote(frame: npt.NDArray[np.float32], exposure: float, config: PyOpenColorIO.Config) -> BlenderResult[Done]:
            return (
                LinearRaw(make="Blender", model=bpy.app.version_string, exposure=exposure, optics=optics)
                .written(target, frame, config)
                .map_error(BlenderImagingFault.lifted)
                .map(lambda _: Done.wrote((target,)))
            )

        view: BlenderResult[bpy.types.ColorManagedViewSettings] = Option.of_optional(scene.view_settings).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="view_settings"),))
        exposure = paired(view, self.gain.stops(call.identity, scene)).map(lambda pair: pair[0].exposure + pair[1])
        return paired(paired(saved(scene).bind(lambda path: plane(path, (f"{layer.name}.Combined",), ("R", "G", "B"))), exposure), config_of()).bind(
            lambda found: wrote(found[0][0], found[0][1], found[1])
        )

    def still(self, call: Call[Still]) -> BlenderResult[Done]:
        """Writes completed source pixels through their recorded formation and encoding."""
        scene, record, target = call.context.scene, call.record, str(call.paths[0])
        if scene is None:
            return Error((BlenderAbsentFault(owner=repr(call.context), key="scene"),))

        def stored(file: ImageFile[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[str]:
            compressed: Option[Compressions] = Some(record.compression.value) if record.compression.enabled else Nothing
            metadata = stamped(call.identity)
            match record.depth:
                case Depth.UINT8:
                    written = ImageFile(pixels=codes(unassociated(file.pixels), 8), colorspace=file.colorspace).write(target, Storage(compression=compressed), metadata, config)
                case Depth.UINT16:
                    written = ImageFile(pixels=codes(unassociated(file.pixels), 16), colorspace=file.colorspace).write(target, Storage(compression=compressed), metadata, config)
                case Depth.HALF | Depth.FLOAT as floated:
                    written = file.write(target, Storage(depth=16 if floated is Depth.HALF else 32, compression=compressed), metadata, config)
            return written.map_error(BlenderImagingFault.lifted)

        def encoded(file: ImageFile[np.float32], config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
            def converted(space: str) -> BlenderResult[ImageFile[np.float32]]:
                return (
                    processor(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=space))
                    .map(lambda built: ImageFile(pixels=applied(built.getDefaultCPUProcessor(), file.pixels), colorspace=space))
                    .map_error(BlenderImagingFault.lifted)
                )

            return Ok(file) if config.getColorSpace(file.colorspace).isData() else display_space(config, record.display).bind(converted)

        def image(config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
            def formed(pixels: npt.NDArray[np.float32], origin: Production) -> BlenderResult[ImageFile[np.float32]]:
                def viewed(settings: tuple[DisplayView, float, float]) -> BlenderResult[ImageFile[np.float32]]:
                    view, exposure, gamma = settings
                    return paired(
                        view.form(config, pixels, source=origin.space, exposure=exposure, gamma=gamma).map_error(BlenderImagingFault.lifted),
                        view.colorspace(config).to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),)),
                    ).map(lambda values: ImageFile(pixels=values[0], colorspace=values[1].getName()))

                return origin.view.map(viewed).default_value(Ok(ImageFile(pixels=pixels, colorspace=origin.space))) if record.save_as_render else Ok(ImageFile(pixels=pixels, colorspace=origin.space))

            def sourced(native: bpy.types.Image, origin: Production) -> BlenderResult[ImageFile[np.float32]]:
                return (
                    origin.layer
                    .map(lambda layer: origin.scene.resolve().bind(saved).bind(lambda path: plane(path, (layer,), ("R", "G", "B", "A"))))
                    .default_with(lambda: read(native, config).map(lambda file: file.pixels))
                    .bind(lambda values: formed(values, origin))
                )

            return built("RENDER_RESULT" if record.source is Source.RESULT else "COMPOSITING").bind(lambda native: production(native).bind(lambda origin: sourced(native, origin)))

        selected: BlenderResult[Format] = writer(record.format).to_result_with(lambda: (BlenderAbsentFault(owner="OpenImageIO.extension_list", key=record.format),))
        return (
            Result[Format, BlenderFaults]
            .bind(selected, lambda chosen: sample(chosen, record.depth))
            .bind(lambda _: config_of())
            .bind(lambda config: image(config).bind(lambda file: encoded(file, config)).bind(lambda file: stored(file, config)))
            .map(lambda _: Done.wrote((target,)))
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares nested export actions using the native save browser without data undo."""
        idle = Job(kind="RENDER")
        result = Some(AllOf(conditions=(image_type(("RENDER_RESULT",)), idle)))

        def drawn(layout: bpy.types.UILayout, context: bpy.types.Context, properties: bpy.types.OperatorProperties) -> None:
            def controls(binding: RecordGroup[Still, bpy.types.Operator]) -> BlenderResult[tuple[tuple[RowDraw, ...], Nest]]:
                return fields(binding, identity).bind(lambda parts: placed(parts[0], identity).map(lambda rows: (rows, parts[1])))

            bound = still.bound(identity).map(lambda pair: pair[1])
            form = bound.bind(controls)
            for rows, nested in form.to_option().to_list():
                for draw in rows:
                    draw(layout, properties, context)
                nested(layout, properties, context, self.image)
            verdicts = bound.bind(lambda binding: binding.read(properties)).bind(
                lambda record: paired(writer(record.format).to_result_with(lambda: (BlenderAbsentFault(owner="OpenImageIO.extension_list", key=record.format),)), config_of()).map(
                    lambda pair: readouts(pair[0], record.depth, record.display, pair[1])
                )
            )
            for readings in verdicts.to_option().to_list():
                for row in readings:
                    layout.label(text=row.text)
            alerted(layout, paired(form, verdicts))

        still = Operator(
            key=self.image,
            title="Export Still",
            description="Writes the displayed render or Viewer with its color encoding",
            form=Still,
            run=self.still,
            condition=Some(idle),
            invoke=Some(Save[Still](extensions=Still.extensions)),
            seed=Some(Still.seeded),
            draw=Some(drawn),
            undo=False,
        )
        return Ok((
            Operator(
                key=self.dng,
                title="Export Linear Raw DNG",
                description="Writes the scene-linear render as a Linear Raw DNG",
                form=Empty,
                run=self.raw,
                condition=result,
                invoke=Some(Save[Empty](extensions=lambda _: (".dng",))),
                undo=False,
            ),
            still,
            *(
                Operator(
                    key=layering.key,
                    title=layering.title,
                    description=layering.description,
                    form=Sheet,
                    run=layering.exported,
                    condition=result,
                    invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))),
                    undo=False,
                )
                for layering in self.layering
            ),
            productions,
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def image_type(kinds: tuple[str, ...]) -> Setting:
    """Returns the shared native image-editor condition over the displayed image's type."""
    return Setting(descriptor=Descriptor(root=bpy.types.SpaceImageEditor, owner="image", prop=bpy.types.Image.bl_rna.properties["type"]), accepted=kinds)


def config_of() -> BlenderResult[PyOpenColorIO.Config]:
    """Returns Blender's admitted OCIO configuration or its complete imaging faults."""
    return admitted(PyOpenColorIO.GetCurrentConfig(), ()).map_error(BlenderImagingFault.lifted)


def plane(path: str, names: tuple[str, ...], channels: tuple[str, ...]) -> BlenderResult[npt.NDArray[np.float32]]:
    """Reads the first requested layer and accumulates every missing requested channel."""

    def picked(file: Multilayer) -> BlenderResult[npt.NDArray[np.float32]]:
        def channel(found: tuple[Channel, ...], layer: str, name: str) -> BlenderResult[Channel]:
            return Option.of_optional(next((held for held in found if held.name == name), None)).to_result_with(lambda: (BlenderAbsentFault(owner=f"{path}:{layer}", key=name),))

        chosen: BlenderResult[Layer] = Option.of_optional(next((found for name in names for found in file.layers if found.name == name), None)).to_result_with(
            lambda: (BlenderAbsentFault(owner=path, key=names[-1]),)
        )
        return Result[Layer, BlenderFaults].bind(
            chosen,
            lambda found: Result[tuple[Channel, ...], BlenderFaults].bind(
                accumulated(channel(found.channels, found.name, name) for name in channels), lambda chosen: file.plane(found, chosen).map_error(BlenderImagingFault.lifted)
            ),
        )

    return Result[Multilayer, BlenderFaults].bind(Multilayer.read(path).map_error(BlenderImagingFault.lifted), picked)


def displayed(viewing: Viewing, identity: Identity, scene: bpy.types.Scene, plane: npt.NDArray[np.float32], source: str, config: PyOpenColorIO.Config) -> BlenderResult[ImageFile[np.float32]]:
    """Forms display values through the owned entry or native scene exposure and gamma."""

    def named(view: DisplayView, pixels: npt.NDArray[np.float32]) -> BlenderResult[ImageFile[np.float32]]:
        return (
            view
            .colorspace(config)
            .map(lambda space: ImageFile(pixels=pixels, colorspace=space.getName()))
            .to_result_with(lambda: (BlenderAbsentFault(owner=repr(config), key=f"{view.display}/{view.view}"),))
        )

    def formed(shown: Option[DisplayView]) -> BlenderResult[ImageFile[np.float32]]:
        def native() -> BlenderResult[ImageFile[np.float32]]:
            def framed(view: DisplayView) -> BlenderResult[ImageFile[np.float32]]:
                return (
                    Result[bpy.types.ColorManagedViewSettings, BlenderFaults]
                    .bind(
                        Option.of_optional(scene.view_settings).to_result_with(lambda: (BlenderAbsentFault(owner=repr(scene), key="view_settings"),)),
                        lambda held: view.form(config, plane, source=source, exposure=held.exposure, gamma=held.gamma).map_error(BlenderImagingFault.lifted),
                    )
                    .bind(lambda pixels: named(view, pixels))
                )

            return viewing.seeded(scene).bind(framed)

        return shown.map(
            lambda view: Result[npt.NDArray[np.float32], BlenderFaults].bind(
                processor(config, PyOpenColorIO.ColorSpaceTransform(src=source, dst=PyOpenColorIO.ROLE_SCENE_LINEAR))
                .map(lambda built: applied(built.getDefaultCPUProcessor(), plane))
                .map_error(BlenderImagingFault.lifted),
                lambda pixels: named(view, pixels),
            )
        ).default_with(native)

    return viewing.current(identity, scene).bind(formed)


# --- [EXPORTS] --------------------------------------------------------------------------


__all__ = ["Documents", "Export", "Layering", "Sheet", "Source", "Still", "Viewing", "config_of", "displayed", "image_type", "plane"]
