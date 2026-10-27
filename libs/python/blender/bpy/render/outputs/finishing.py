"""Ordered finishing rows over files written during a native render job."""

from collections import Counter
from collections.abc import Callable, Mapping
from contextlib import nullcontext
import functools
from pathlib import Path
from typing import Final, Protocol, Self

import bpy
from expression import Nothing, Ok, Option
from expression.collections import Map
import msgspec
import OpenImageIO

from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.render.outputs.formats import compression, depth, Kind, kind
from blender.bpy.render.outputs.framing import FrameSize
from blender.bpy.render.outputs.results import INTERMEDIATES
from blender.imaging.files.coding import Depth
from blender.imaging.files.images import Compressions
from blender.imaging.files.multilayer import Multilayer
from blender.records.faults import accumulated, Outcome
from blender.rendering.outputs.conventions import Nuke
from blender.rendering.outputs.naming import Output

# --- [TYPES] ----------------------------------------------------------------------------

type Body = Callable[[bpy.types.Scene, Written, Identity], Outcome[None]]


class Files(Protocol):
    """Files written by marked compositor outputs for one scene frame."""

    def written(self, scene: bpy.types.Scene, frame: int, identity: Identity) -> Outcome[tuple["Written", ...]]:
        """Returns frame's written files."""
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Written(msgspec.Struct, frozen=True, kw_only=True):
    """Written file with storage, pass names, and display window."""

    path: str
    output: Output
    kind: Kind
    depth: Depth
    layer: str
    compression: Option[Compressions] = Nothing
    convention: Option[Nuke] = Nothing
    passes: Map[str, str] = Map.empty()
    display: Option[OpenImageIO.ROI] = Nothing

    @classmethod
    def of(cls, path: str, settings: bpy.types.ImageFormatSettings, output: Output, convention: Option[Nuke], layer: str, passes: Map[str, str]) -> Option[Self]:
        """Returns written file when native depth maps to a storage depth."""
        return depth(settings).map(lambda held: cls(path=path, output=output, kind=kind(settings), depth=held, layer=layer, compression=compression(settings), convention=convention, passes=passes))


class Finish(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operation and written storage classes it consumes."""

    run: Body
    outputs: frozenset[Output] = frozenset(Output)
    kinds: frozenset[Kind] = frozenset(Kind)
    depths: frozenset[Depth] = frozenset(Depth)

    def accepts(self, file: Written) -> bool:
        """Returns whether operation consumes file."""
        return file.output in self.outputs and file.kind in self.kinds and file.depth in self.depths


class Finishing(msgspec.Struct, frozen=True, kw_only=True):
    """Finishing operations with handlers spanning one render job."""

    rows: tuple[Finish, ...]
    files: Files

    def finished(self, scene: bpy.types.Scene, files: tuple[Written, ...], identity: Identity) -> Outcome[None]:
        """Chains operations per file, faults accumulating across files."""
        return accumulated(functools.reduce(lambda done, row: done.bind(lambda _: row.run(scene, file, identity)), (row for row in self.rows if row.accepts(file)), Ok(None)) for file in files).map(
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
                .bind(
                    lambda held: self.finished(
                        scene,
                        tuple(
                            msgspec.structs.replace(
                                file, display=held.map(lambda base: OpenImageIO.ROI(base.padding[0], base.padding[0] + base.size[0], base.padding[1], base.padding[1] + base.size[1]))
                            )
                            for file in files
                        ),
                        identity,
                    )
                )
            )

        stopped = functools.partial(bracket, started=False)
        return Ok((
            FrameSize.attachment,
            INTERMEDIATES,
            _RUNNING,
            Handler(handlers=bpy.app.handlers.render_init, body=functools.partial(bracket, started=True)),
            Handler(handlers=bpy.app.handlers.render_complete, body=stopped),
            Handler(handlers=bpy.app.handlers.render_cancel, body=stopped),
            Handler(handlers=bpy.app.handlers.render_write, body=lambda scene: windowed(scene, scene_files(scene))),
            Handler(
                handlers=bpy.app.handlers.render_post,
                body=lambda scene: (
                    self.files.written(scene, scene.frame_current, identity).bind(lambda files: windowed(scene, files))
                    if scene.session_uid in identity.held[_RUNNING] and scene.session_uid not in identity.held[INTERMEDIATES]
                    else Ok(None)
                ),
            ),
        ))


# --- [SERVICES] -------------------------------------------------------------------------

_RUNNING: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))

# --- [OPERATIONS] -----------------------------------------------------------------------


def scene_files(scene: bpy.types.Scene) -> tuple[Written, ...]:
    """Returns current frame's existing `frame_path` files per active view, omitting movies and unsupported depths."""
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
            for file in Written.of(path, settings, Output.PREVIEW, Nothing, active.name, Map.empty()).to_list()
        )
    )


def stamped(identity: Identity) -> Mapping[str, str]:
    """Returns extension name and version as exported file metadata."""
    return {"Software": f"{identity.manifest.name} {identity.manifest.version}"}


def rewritten_file(_scene: bpy.types.Scene, file: Written, _identity: Identity) -> Outcome[None]:
    """Rewrites file display window and the one-channel pass names its Nuke convention renames."""

    def channels(names: tuple[str, ...]) -> tuple[str, ...]:
        counts = Counter(name.rpartition(".")[0] for name in names)
        return tuple(
            file.passes
            .try_find(layer)
            .bind(lambda source: file.convention.bind(lambda nuke: nuke.channel(source)))
            .filter(lambda _: counts[layer] == 1)
            .map(lambda channel: f"{layer}.{channel}" if layer else channel)
            .default_value(name)
            for name in names
            for layer in (name.rpartition(".")[0],)
        )

    return Multilayer.rewritten(file.path, file.display, channels, file.compression).map(lambda _: None)


# --- [COMPOSITION] ----------------------------------------------------------------------


rewrite = Finish(run=rewritten_file, kinds=frozenset({Kind.LAYERED, Kind.EXR}), depths=frozenset({Depth.HALF, Depth.FLOAT}))

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Body", "Files", "Finish", "Finishing", "Written", "rewrite", "rewritten_file", "scene_files", "stamped"]
