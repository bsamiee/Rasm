"""Ordered finishing rows over files written during a native render job."""

from collections import Counter
from collections.abc import Callable, Mapping
import functools
from pathlib import Path
from typing import Protocol, Self

import bpy
from expression import Nothing, Ok, Option
from expression.collections import Map
import msgspec

from blender.bpy.data.ids.images import intermediates
from blender.bpy.extension.events.handlers import SceneHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderImagingFault, BlenderResult
from blender.bpy.render.outputs.formats import compression, depth, Kind, kind
from blender.bpy.render.outputs.framing import FrameSize
from blender.imaging.faults import accumulated
from blender.imaging.files.images import Compressions, Depth
from blender.imaging.files.multilayer import rewritten, Window
from blender.rendering.outputs.conventions import Blender, Conventions
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type Body = Callable[[bpy.types.Scene, Written, Identity], BlenderResult[None]]


class Files(Protocol):
    """Files written by marked compositor outputs for one scene frame."""

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity) -> BlenderResult[tuple["Written", ...]]:
        """Returns the frame's written files or their naming faults."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Written(msgspec.Struct, frozen=True, kw_only=True):
    """Written file and the storage, pass names, and display window its owner supplies."""

    path: str
    output: Output
    kind: Kind
    depth: Depth
    layer: str
    compression: Option[Compressions] = Nothing
    convention: Conventions = Blender()
    passes: Map[str, str] = Map.empty()
    display: Option[Window] = Nothing

    @classmethod
    def of(cls, path: str, settings: bpy.types.ImageFormatSettings, output: Output, convention: Conventions, layer: str, passes: Map[str, str]) -> Option[Self]:
        """Constructs a written file when its native depth has a storage representation."""
        return depth(settings).map(lambda held: cls(path=path, output=output, kind=kind(settings), depth=held, layer=layer, compression=compression(settings), convention=convention, passes=passes))


class Finish(msgspec.Struct, frozen=True, kw_only=True):
    """One finishing operation and the written storage classes it consumes."""

    run: Body
    outputs: frozenset[Output] = frozenset(Output)
    kinds: frozenset[Kind] = frozenset(Kind)
    depths: frozenset[Depth] = frozenset(Depth)

    def accepts(self, file: Written) -> bool:
        """Returns whether the operation consumes the file's class and storage."""
        return file.output in self.outputs and file.kind in self.kinds and file.depth in self.depths


class Finishing(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operations and native handlers owning one render-job bracket."""

    rows: tuple[Finish, ...]
    files: Files

    def finished(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> BlenderResult[None]:
        """Binds operations per file and accumulates independent files' faults."""
        return accumulated(functools.reduce(lambda done, row: done.bind(lambda _: row.run(scene, file, identity)), (row for row in self.rows if row.accepts(file)), Ok(None)) for file in files).map(
            lambda _: None
        )

    def acquisitions(self, identity: Identity) -> BlenderResult[Acquisitions]:
        """Declares handlers that bind registered framing when each event runs."""
        running: set[int] = set()

        def bracket(scene: bpy.types.Scene, *, started: bool) -> BlenderResult[None]:
            (running.add if started else running.discard)(scene.session_uid)
            return Ok(None)

        def windowed(scene: bpy.types.Scene, files: tuple[Written, ...]) -> BlenderResult[None]:
            if scene.session_uid in intermediates:
                return Ok(None)
            return (
                FrameSize()
                .bound(identity)
                .bind(lambda framed: framed.base(scene))
                .bind(
                    lambda held: self.finished(
                        scene,
                        tuple(msgspec.structs.replace(file, display=held.map(lambda base: Window(x=base.padding[0], y=base.padding[1], width=base.size[0], height=base.size[1]))) for file in files),
                        identity,
                    )
                )
            )

        stopped = functools.partial(bracket, started=False)
        return Ok((
            FrameSize.attachment,
            SceneHandler(handlers=bpy.app.handlers.render_init, body=functools.partial(bracket, started=True)),
            SceneHandler(handlers=bpy.app.handlers.render_complete, body=stopped),
            SceneHandler(handlers=bpy.app.handlers.render_cancel, body=stopped),
            SceneHandler(handlers=bpy.app.handlers.render_write, body=lambda scene: windowed(scene, scene_files(scene))),
            SceneHandler(
                handlers=bpy.app.handlers.render_post,
                body=lambda scene: (
                    self.files.written(scene, scene.frame_current, identity).bind(lambda files: windowed(scene, files))
                    if scene.session_uid in running and scene.session_uid not in intermediates
                    else Ok(None)
                ),
            ),
        ))


# --- [OPERATIONS] -----------------------------------------------------------------------


def scene_files(scene: bpy.types.Scene) -> tuple[Written, ...]:
    """Returns the current frame's written native paths per active view, omitting movies, unsupported depths, and still writes `frame_path` does not name."""
    render, settings = scene.render, scene.render.image_settings
    views = tuple(view.name for view in render.views if view.use) if render.use_multiview and settings.views_format == "INDIVIDUAL" else ("",)
    return (
        ()
        if render.is_movie_format
        else tuple(
            file
            for view in views
            for path in (render.frame_path(frame=scene.frame_current, view=view),)
            if Path(path).is_file()
            for file in Written.of(path, settings, Output.PREVIEW, Blender(), scene.view_layers[0].name, Map.empty()).to_list()
        )
    )


def stamped(identity: Identity) -> Mapping[str, str]:
    """Returns the extension's software name and version for exported file metadata."""
    return {"Software": f"{identity.manifest.name} {identity.manifest.version}"}


def rewritten_file(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> BlenderResult[None]:
    """Publishes changed display windows and one-channel pass names through native imaging."""

    def channels(names: tuple[str, ...]) -> tuple[str, ...]:
        counts = Counter(name.rpartition(".")[0] for name in names)
        return tuple(
            file.passes.try_find(layer).bind(file.convention.channel).filter(lambda _: counts[layer] == 1).map(lambda channel: f"{layer}.{channel}" if layer else channel).default_value(name)
            for name in names
            for layer in (name.rpartition(".")[0],)
        )

    return rewritten(file.path, file.display, channels, file.compression).map(lambda _: None).map_error(lambda faults: (BlenderImagingFault(faults=faults),))


# --- [COMPOSITION] ----------------------------------------------------------------------


rewrite = Finish(run=rewritten_file, kinds=frozenset({Kind.LAYERED, Kind.EXR}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Body", "Files", "Finish", "Finishing", "Written", "rewrite", "rewritten_file", "scene_files", "stamped"]
