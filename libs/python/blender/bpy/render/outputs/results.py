"""Render products: the Render Result and Viewer images, production facts per slot, intermediate renders, the saved multilayer render, and finishing rows over the files a render job writes."""

from collections import Counter
from collections.abc import Callable, Generator, Mapping
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial, reduce
from pathlib import Path
from types import MappingProxyType
from typing import Final, Literal, Self

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.collections import Block, Map
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt
import OpenImageIO

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.outputs.formats import depth, FILE_FORMATS, OutputFormat
from blender.bpy.render.outputs.framing import FrameSize
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.buffers import Depth
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Fault, Outcome, Reason
from blender.records.stores import committed
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type ResultKind = Literal["RENDER_RESULT", "COMPOSITING"]

# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = bpy.types.ViewLayer.bl_rna.properties["use_pass_combined"].name
COMPOSITE: Final = "Composite"
_SAVED: Final = OutputFormat(
    media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", color_depth="32", exr_codec="ZIP", use_exr_interleave=False, views_format="MULTIVIEW", color_management="FOLLOW_SCENE"
)

# --- [MODELS] ---------------------------------------------------------------------------


class Production(msgspec.Struct, frozen=True, kw_only=True):
    """Completed image origin and encoding colorspace, recorded per image and render slot."""

    scene: Handle[bpy.types.Scene]
    space: str
    layer: Option[str] = Nothing
    view: Option[DisplayView] = Nothing

    @staticmethod
    def recorded(identity: Identity, image: bpy.types.Image, production: Option["Production"], /) -> None:
        """Records the production facts of the image's displayed slot, Nothing clearing them."""
        identity.held[PRODUCTIONS][image.session_uid, image.render_slots.active_index] = production

    @classmethod
    def shown(cls, identity: Identity, image: bpy.types.Image, /) -> Outcome[Self]:
        """Returns the recorded production facts of the image's displayed slot."""
        return identity.held[PRODUCTIONS].get((image.session_uid, image.render_slots.active_index), Nothing).to_result_with(partial(Fault.of, Reason.ABSENT, repr(image), member="production facts"))


class ResultRead(msgspec.Struct, frozen=True, kw_only=True):
    """Completed-render consumer receiving the saved multilayer file on the main thread."""

    consume: Callable[[bpy.types.Scene, str], Outcome[Option[str]]]

    @contextmanager
    def acquisitions(self, identity: Identity) -> Generator[Outcome[Acquisitions]]:
        """Yields the intermediate cell and the render completion handler deferring consumption."""

        def consumed(handle: Handle[bpy.types.Scene]) -> Outcome[Option[str]]:
            return handle.resolve().bind(lambda scene: saved(scene).bind(lambda path: self.consume(scene, path)))

        with identity.reports.deferred(consumed) as deferred:
            yield Ok((
                INTERMEDIATES,
                Handler(handlers=bpy.app.handlers.render_complete, body=lambda scene: Ok(None if scene.session_uid in identity.held[INTERMEDIATES] else deferred(Handle.of(scene)))),
            ))


class Written(msgspec.Struct, frozen=True, kw_only=True):
    """Written file with its stored format and depth, pass names, and display window."""

    path: str
    output: Output
    format: OutputFormat
    depth: Depth
    layer: str
    channels: Mapping[str, str] = MappingProxyType({})
    passes: Map[str, str] = Map.empty()
    display: Option[OpenImageIO.ROI] = Nothing

    @classmethod
    def of(cls, path: str, settings: bpy.types.ImageFormatSettings, output: Output, channels: Mapping[str, str], layer: str, passes: Map[str, str]) -> Option[Self]:
        """Returns written file when native depth maps to a storage depth, `channels` renaming one-channel source passes for a downstream reader."""
        return depth(settings).map(lambda held: cls(path=path, output=output, format=OutputFormat.stored(settings), depth=held, layer=layer, channels=channels, passes=passes))


class Finish(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operation and written storage classes it consumes."""

    run: Callable[[bpy.types.Scene, Written, Identity], Outcome[None]]
    outputs: frozenset[Output] = frozenset(Output)
    formats: frozenset[str] = FILE_FORMATS
    depths: frozenset[Depth] = frozenset(Depth)

    def accepts(self, file: Written) -> bool:
        """Returns whether operation consumes file."""
        return file.output in self.outputs and file.format.file_format in self.formats and file.depth in self.depths


class Finishing(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operations with handlers spanning one render job, `files` reading the files marked compositor outputs wrote for a scene frame."""

    rows: tuple[Finish, ...]
    files: Callable[[bpy.types.Scene, int, Identity], Outcome[tuple[Written, ...]]]

    def finished(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> Outcome[None]:
        """Chains operations per file, faults accumulating across files."""
        return accumulated(reduce(lambda done, row: done.bind(lambda _: row.run(scene, file, identity)), (row for row in self.rows if row.accepts(file)), Ok(None)) for file in files).map(
            lambda _: None
        )

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Declares handlers binding registered framing per event."""

        def bracket(scene: bpy.types.Scene, *, started: bool) -> Outcome[None]:
            running = identity.held[_RUNNING]
            (running.add if started else running.discard)(scene.session_uid)
            return Ok(None)

        def windowed(scene: bpy.types.Scene, files: tuple[Written, ...]) -> Outcome[None]:
            if scene.session_uid in identity.held[INTERMEDIATES]:
                return Ok(None)
            return (
                FrameSize()
                .bound(identity)
                .bind(lambda framed: framed.base(scene))
                .bind(lambda held: self.finished(scene, tuple(msgspec.structs.replace(file, display=held.map(lambda base: base.bounds[1])) for file in files), identity))
            )

        def scene_files(scene: bpy.types.Scene) -> tuple[Written, ...]:
            render, settings = scene.render, scene.render.image_settings
            active: bpy.types.ViewLayer = bpy.context.view_layer  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            views = tuple(view.name for view in render.views if view.use) if render.use_multiview and settings.views_format == "INDIVIDUAL" else ("",)
            return (
                ()
                if render.is_movie_format
                else tuple(
                    file
                    for view in views
                    for path in (render.frame_path(frame=scene.frame_current, view=view),)
                    if Path(path).is_file()
                    for file in Written.of(path, settings, Output.PREVIEW, MappingProxyType({}), active.name, Map.empty()).to_list()
                )
            )

        stopped = partial(bracket, started=False)
        return Ok((
            FrameSize.attachment,
            INTERMEDIATES,
            _RUNNING,
            Handler(handlers=bpy.app.handlers.render_init, body=partial(bracket, started=True)),
            Handler(handlers=bpy.app.handlers.render_complete, body=stopped),
            Handler(handlers=bpy.app.handlers.render_cancel, body=stopped),
            Handler(handlers=bpy.app.handlers.render_write, body=lambda scene: windowed(scene, scene_files(scene))),
            Handler(
                handlers=bpy.app.handlers.render_post,
                body=lambda scene: (
                    self.files(scene, scene.frame_current, identity).bind(lambda files: windowed(scene, files))
                    if scene.session_uid in identity.held[_RUNNING] and scene.session_uid not in identity.held[INTERMEDIATES]
                    else Ok(None)
                ),
            ),
        ))


# --- [SERVICES] -------------------------------------------------------------------------

INTERMEDIATES: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))
PRODUCTIONS: Final = Resource[dict[tuple[int, int | None], Option[Production]]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {}))
_RUNNING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [RESULTS]


@contextmanager
def intermediate(identity: Identity, scene: bpy.types.Scene) -> Generator[None]:
    """Hides scene renders from the extension's completed-image consumers during scope."""
    held = identity.held[INTERMEDIATES]
    with ExitStack() as stack:
        held.add(scene.session_uid)
        stack.callback(held.remove, scene.session_uid)
        yield


def built(kind: ResultKind) -> Outcome[bpy.types.Image]:
    """Returns Render Result or shared compositor Viewer image, or absent fault before Blender creates it.

    Callers read Viewer completion pixels before deferring.
    """
    return Block.of_seq(bpy.data.images).filter(lambda image: image.type == kind).try_head().to_result_with(partial(Fault.of, Reason.ABSENT, type(bpy.data.images).__name__, member=kind))


def saved(scene: bpy.types.Scene) -> Outcome[str]:
    """Exports displayed render result passes to temporary multilayer EXR, the scene's format restored after on every path."""
    path = Path(bpy.app.tempdir, f"{scene.session_uid}.exr")

    def exported(image: bpy.types.Image) -> Outcome[str]:
        with _SAVED.held(scene.render.image_settings) as formatted:
            return formatted.bind(
                lambda _: committed(path, lambda stage: catch(image.save_render, exception=RuntimeError)(str(stage), scene=scene).map_error(partial(Fault.of, Reason.REFUSED, repr(image)))).map(str)
            )

    return built("RENDER_RESULT").bind(exported)


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
                return file.plane(owners[owner], ("R", "G", "B", "A")).map(lambda pixels: (owner, pixels))
            case _:
                return Error(Fault.of(Reason.ABSENT, path, member=layer.map(lambda name: f"{name}.{COMBINED}").default_value(COMBINED)))

    return Multilayer.read(Path(path)).bind(selected)


# --- [FINISHING]


def _rewritten(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
    """Rewrites file display window and the one-channel pass names its channel renames hold."""

    def channels(names: tuple[str, ...]) -> tuple[str, ...]:
        counts = Counter(name.rpartition(".")[0] for name in names)
        return tuple(
            file.passes
            .try_find(layer)
            .bind(lambda source: Option.of_optional(file.channels.get(source)))
            .filter(lambda _: counts[layer] == 1)
            .map(lambda channel: f"{layer}.{channel}" if layer else channel)
            .default_value(name)
            for name in names
            for layer in (name.rpartition(".")[0],)
        )

    return Multilayer.rewritten(Path(file.path), file.display, channels, file.format.codec).map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------

rewrite: Final = Finish(run=_rewritten, formats=frozenset({"OPEN_EXR", "OPEN_EXR_MULTILAYER"}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMBINED", "COMPOSITE", "INTERMEDIATES", "PRODUCTIONS", "Finish", "Finishing", "Production", "ResultKind", "ResultRead", "Written", "built", "combined", "intermediate", "rewrite", "saved"]
