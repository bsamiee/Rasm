"""Displayed-render exports as Linear Raw DNG, tagged stills, layered documents, and linear-light review proxies."""

from collections.abc import Callable, Mapping
from functools import cached_property, partial
import math
from pathlib import Path
import sys
from types import MappingProxyType
from typing import Annotated, Final, get_args

import bpy
from expression import Option, Some, try_downcast
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import PyOpenColorIO

from rasm_bpy.commands.invocations import Save
from rasm_bpy.commands.operators import Call, Done, Operator
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import Acquisitions
from rasm_bpy.data._ids.images import _read
from rasm_bpy.data._rna.conditions import Job
from rasm_bpy.data._rna.descriptors import Value, written
from rasm_bpy.data._rna.paths import rooted
from rasm_bpy.data._rna.properties import case_choices, choices
from rasm_bpy.render._color.spaces import viewed
from rasm_bpy.render._outputs.results import _built, _RENDER_ORIGINS, _ResultKind, CompletedRender
from rasm_imaging import basis, captured_scene_source, Compression, converted, converted_image, Depth, DisplayView, Document, fitted, Format, ImageFile, LinearRaw, Multilayer, read_file, Storage
from rasm_records import Case, Error, Fault, Nothing, Ok, Outcome, paired, Reason

# --- [CONSTANTS] ------------------------------------------------------------------------

_IDLE: Final = Job(kind="RENDER")
_DEPTHS: Final = choices((depth.name, depth.name, "") for depth in Depth)
_SOURCES: Final = choices(item for item in bpy.types.Image.bl_rna.properties["type"].enum_items if item.identifier in get_args(_ResultKind.__value__))  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
_WRITERS: Final = MappingProxyType({row.name: row for row in Format.writable()})

# --- [MODELS] ---------------------------------------------------------------------------


class CameraView(bpy.types.PropertyGroup):
    """Optional camera-view selection for a saved multilayer render."""

    use_camera_view: bpy.props.BoolProperty(name="Camera View", description="Select one camera view from a multi-view render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    camera_view: bpy.props.StringProperty(name="View", description="Camera view name in saved render")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]

    @property
    def selected_view(self) -> Option[str]:
        """Explicit camera-view request, absent while automatic selection stands."""
        if not self.use_camera_view:
            return Nothing
        name: str = self.camera_view  # ty: ignore[unsound-assignment]
        return Some(name)


class Still(CameraView):
    """Source, writer, stored depth, display encoding, and formation policy."""

    source: bpy.props.EnumProperty(name="Source", description="Image the still writes", items=_SOURCES)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    format: bpy.props.EnumProperty(  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
        name="Format", description="File format the still writes", items=choices((row.name, f"{row.name} (.{row.extensions[0]})", "") for row in _WRITERS.values()), default="png"
    )
    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each channel", items=_DEPTHS)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    display: bpy.props.StringProperty(name="Display", description="Display encoding at selected depth", search=lambda _owner, _context, _edit: PyOpenColorIO.GetCurrentConfig().getDisplays())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    save_as_render: bpy.props.BoolProperty(name="Save as Render", description="Apply view, exposure, and gamma to unformed source", default=True)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    use_compression: bpy.props.BoolProperty(name="Use Compression", description="Write the chosen compression, writer default when off")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    compression: bpy.props.EnumProperty(name="Compression", description="Writer compression at its defaults", items=case_choices(Compression))  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Sheet(bpy.types.PropertyGroup):
    """Channel storage selected for a layered TIFF export."""

    depth: bpy.props.EnumProperty(name="Depth", description="Storage of each layer channel", items=_DEPTHS, default=Depth.UINT16.name)  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


class Layering(msgspec.Struct, frozen=True, kw_only=True):
    """Layered TIFF export of the document a builder makes of the saved frame, its command keyed `export_<key>`."""

    key: Key
    document: Callable[[Identity, bpy.types.Scene, Multilayer, Depth], Outcome[Document]]

    def command(self, selected: Callable[[Identity, bpy.types.Context], Outcome[CompletedRender]]) -> Operator[Sheet]:
        """Command writing the built document of the saved frame as layered TIFF, keyed by the layering."""

        def exported(call: Call[Sheet]) -> Outcome[Done]:
            """Write the built document of the saved frame as layered TIFF."""
            target = call.paths[0]
            return (
                selected(call.identity, call.context)
                .bind(
                    lambda source: (
                        rooted(bpy.types.Scene, call.context)
                        .bind(lambda scene: self.document(call.identity, scene, source.file, Depth[call.properties.depth]))
                        .bind(lambda document: document.written(target, PyOpenColorIO.GetCurrentConfig()))
                    )
                )
                .map(lambda _: Done.wrote((target,)))
            )

        return Operator.of(Sheet, condition=Some(_IDLE), invoke=Some(Save[Sheet](extensions=lambda _: (".tif",))), options=frozenset(), named=Some(f"export_{self.key}"))(exported)


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
    """Exports of an explicitly selected owned render and native Viewer pixels."""

    # --- [STATE]

    selected: Callable[[Identity, bpy.types.Context], Outcome[CompletedRender]]
    layering: tuple[Layering, ...] = ()

    # --- [COMMANDS]

    @cached_property
    def linear(self) -> Operator[CameraView]:
        """Command writing the scene-linear render as DNG."""
        return Operator.of(CameraView, condition=Some(_IDLE), invoke=Some(Save[CameraView](extensions=lambda _: (".dng",))), options=frozenset())(self.export_dng)

    @cached_property
    def still(self) -> Operator[Still]:
        """Command writing the render or Viewer image with chosen encoding."""

        def seeded(properties: Still, _event: bpy.types.Event, _identity: Identity) -> Mapping[str, Value]:
            writer = _WRITERS[properties.format]
            depth = Depth[properties.depth]
            return {
                "display": PyOpenColorIO.GetCurrentConfig().getDefaultDisplay(),
                "depth": (depth if depth in writer.depths else writer.depths[0]).name,
                "use_compression": _compression(properties).filter(lambda held: held in writer.compressions).is_some(),
            }

        return Operator.of(Still, condition=Some(_IDLE), invoke=Some(Save[Still](extensions=lambda properties: _WRITERS[properties.format].suffixes)), seed=Some(seeded), options=frozenset())(
            self.export_still
        )

    @cached_property
    def layered(self) -> tuple[Operator[Sheet], ...]:
        """Layered document commands in layering order."""
        return tuple(each.command(self.selected) for each in self.layering)

    # --- [WRITES]

    def export_dng(self, call: Call[CameraView]) -> Outcome[Done]:
        """Write scene-linear render as Linear Raw DNG with total exposure and camera optics."""
        target = call.paths[0]

        def exported(source: CompletedRender) -> Outcome[Done]:
            return (
                source.raw
                .to_result_with(partial(Fault.of, Reason.ABSENT, str(source.file.path), member="raw render source"))
                .bind(
                    lambda raw: paired(
                        source.source(view=call.properties.selected_view).bind(
                            lambda file: source.source_encoding.filter_with(Option.is_some, lambda _: Fault.of(Reason.REQUIRED, "color encoding for Linear Raw DNG")).bind(
                                lambda program: converted_image(source.config, PyOpenColorIO.ROLE_INTERCHANGE_SCENE, file, source=program)
                            )
                        ),
                        Option.of_optional(raw.view).to_result_with(partial(Fault.of, Reason.ABSENT, str(source.file.path), member="captured source exposure")),
                        raw.optics,
                        basis(source.config, PyOpenColorIO.ROLE_INTERCHANGE_SCENE),
                    ).bind(lambda facts: LinearRaw(make="Blender", model=bpy.app.version_string, exposure=facts[1].exposure, optics=facts[2]).written(target, facts[0].pixels[..., :3], facts[3]))
                )
                .map(lambda _: Done.wrote((target,)))
            )

        return self.selected(call.identity, call.context).bind(exported)

    def export_still(self, call: Call[Still]) -> Outcome[Done]:
        """Write render or Viewer image with chosen encoding."""
        properties, target, config = call.properties, call.paths[0], PyOpenColorIO.GetCurrentConfig()
        storage = Storage(depth=Depth[properties.depth], compression=_compression(properties))

        def encoded(file: ImageFile) -> Outcome[ImageFile]:
            return (
                Ok(file)
                if config.getColorSpace(file.colorspace).isData()
                else DisplayView.encoding(config, properties.display).bind(
                    lambda space: converted(config, PyOpenColorIO.ColorSpaceTransform(src=file.colorspace, dst=space), file.pixels).map(
                        lambda pixels: msgspec.structs.replace(file, pixels=pixels, colorspace=space)
                    )
                )
            )

        def image(file: ImageFile, program: Option[PyOpenColorIO.Processor], formation: Option[DisplayView]) -> Outcome[ImageFile]:
            if program.is_none():
                return Ok(file)
            return (
                formation
                .filter(lambda _: properties.save_as_render)
                .map(
                    lambda view: (
                        converted_image(config, PyOpenColorIO.ROLE_SCENE_LINEAR, file, source=program)
                        .bind(lambda linear: view.image(config, linear.pixels, linear.colorspace))
                        .map(lambda formed: msgspec.structs.replace(formed, header=file.header))
                        .bind(encoded)
                    )
                )
                .default_with(lambda: DisplayView.encoding(config, properties.display).bind(lambda space: converted_image(config, space, file, source=program)))
            )

        def rendered(source: CompletedRender) -> Outcome[ImageFile]:
            def selected(found: tuple[ImageFile, Option[PyOpenColorIO.Processor]]) -> Outcome[ImageFile]:
                file, program = found
                formation = (
                    Ok(source.view)
                    if source.scene.is_some()
                    else rooted(bpy.types.Scene, call.context).map(lambda scene: Some(viewed(scene)))
                    if program.is_some()
                    and source.config.getCanonicalName(file.colorspace) in source.config.getColorSpaceNames(PyOpenColorIO.SEARCH_REFERENCE_SPACE_SCENE, PyOpenColorIO.COLORSPACE_ALL)
                    else Ok(Nothing)
                )
                return formation.bind(lambda view: image(file, program, view))

            return paired(source.read(view=properties.selected_view), source.encoding).bind(selected)

        def native(native: bpy.types.Image) -> Outcome[ImageFile]:
            space = config.getColorSpace(PyOpenColorIO.ROLE_SCENE_LINEAR if native.use_view_as_render else PyOpenColorIO.ROLE_DATA).getName()
            formation = rooted(bpy.types.Scene, call.context).map(lambda scene: Some(viewed(scene))) if native.use_view_as_render else Ok(Nothing)
            return paired(_read(native, space), captured_scene_source(config, space), formation).bind(lambda found: image(*found))

        return (
            (
                self.selected(call.identity, call.context).bind(rendered)
                if properties.source == "RENDER_RESULT"
                else _built("COMPOSITING").bind(native)
                if properties.selected_view.is_none()
                else Error(Fault.of(Reason.OUTSIDE, properties.source, properties.selected_view, member="owned render for camera view selection"))
            )
            .bind(lambda file: file.written(target, storage, call.identity.manifest.stamp, config))
            .map(lambda _: Done.wrote((target,)))
        )

    # --- [REGISTRATION]

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns export commands and render production tracking."""
        return self.linear, self.still, *self.layered, _RENDER_ORIGINS


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [COMPRESSION]


def _compression(properties: Still) -> Option[Compression]:
    """Returns the chosen compression case at its defaults, Nothing while the writer default stands."""
    if not properties.use_compression:
        return Nothing
    selected: Compression = msgspec.convert({"kind": properties.compression}, Compression.__value__)  # ty: ignore[unsound-assignment]
    return Some(selected)


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
            lambda row: read_file(source, config).bind(
                lambda file: downscale.fitted(file.pixels.shape[1], file.pixels.shape[0]).map(lambda size: proxy(key, source, row, file, *size).map(Some)).default_value(Ok(Nothing))
            )
        )

    return sourced(identity.identifier("proxy"))


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

    return back(identity.identifier("proxy"))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["CameraView", "Export", "Layering", "LongestSide", "Scaled", "Sheet", "Still", "proxied", "restored"]
