"""Render results: completed-image facts, intermediate renders, the saved render result, and its planes."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack, nullcontext
from functools import partial
from pathlib import Path
from typing import Final

import bpy
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import numpy as np
import numpy.typing as npt

from blender.bpy.data.ids.store import Handle
from blender.bpy.extension.events import Handler
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.registration import Acquisitions
from blender.bpy.faults import BlenderReason
from blender.bpy.render.outputs.formats import OutputFormat
from blender.imaging.color.processors import DisplayView
from blender.imaging.files.multilayer import Channel, Layer, Multilayer
from blender.records.faults import accumulated, Fault, Faults, Outcome
from blender.records.stores import committed

# --- [CONSTANTS] ------------------------------------------------------------------------

COMBINED: Final = bpy.types.ViewLayer.bl_rna.properties["use_pass_combined"].name
COMPOSITE: Final = "Composite"
_SAVED: Final = OutputFormat(
    media_type="MULTI_LAYER_IMAGE", file_format="OPEN_EXR_MULTILAYER", color_depth="32", exr_codec="ZIP", use_exr_interleave=False, views_format="MULTIVIEW", color_management="FOLLOW_SCENE"
)

# --- [MODELS] ---------------------------------------------------------------------------


class Production(msgspec.Struct, frozen=True, kw_only=True):
    """Completed image origin and encoding colorspace."""

    scene: Handle[bpy.types.Scene]
    space: str
    layer: Option[str] = Nothing
    view: Option[DisplayView] = Nothing


class ResultRead(msgspec.Struct, frozen=True, kw_only=True):
    """Completed-render consumer receiving multilayer file on main thread."""

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


# --- [SERVICES] -------------------------------------------------------------------------

INTERMEDIATES: Final = Resource[set[int]](made=lambda _identity: nullcontext(set()))
PRODUCTIONS: Final = Resource[dict[tuple[int, int | None], Option[Production]]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {}))

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


def produced(identity: Identity, image: bpy.types.Image, production: Option[Production]) -> None:
    """Records production facts of displayed slot."""
    identity.held[PRODUCTIONS][image.session_uid, image.render_slots.active_index] = production


def production(identity: Identity, image: bpy.types.Image) -> Outcome[Production]:
    """Returns recorded production facts of displayed slot."""
    return identity.held[PRODUCTIONS].get((image.session_uid, image.render_slots.active_index), Nothing).to_result_with(partial(Fault.of, BlenderReason.ABSENT, repr(image), member="production facts"))


def built(kind: str) -> Outcome[bpy.types.Image]:
    """Returns Render Result or shared compositor Viewer image, or absent fault before Blender creates it.

    Callers read Viewer completion pixels before deferring.
    """
    return Block.of_seq(bpy.data.images).filter(lambda image: image.type == kind).try_head().to_result_with(partial(Fault.of, BlenderReason.ABSENT, type(bpy.data.images).__name__, member=kind))


def saved(scene: bpy.types.Scene) -> Outcome[str]:
    """Exports displayed render result passes to temporary multilayer EXR, the scene's format restored after on every path."""
    path = Path(bpy.app.tempdir, f"{scene.session_uid}.exr")

    def exported(image: bpy.types.Image) -> Outcome[str]:
        with _SAVED.held(scene.render.image_settings) as formatted:
            return formatted.bind(
                lambda _: committed(path, lambda stage: catch(image.save_render, exception=RuntimeError)(str(stage), scene=scene).map_error(partial(Fault.of, BlenderReason.REFUSED, repr(image)))).map(
                    str
                )
            )

    return built("RENDER_RESULT").bind(exported)


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

    return Multilayer.read(Path(path)).bind(picked)


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

    return Multilayer.read(Path(path)).bind(selected)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["COMBINED", "COMPOSITE", "INTERMEDIATES", "PRODUCTIONS", "Production", "ResultRead", "built", "combined", "intermediate", "plane", "produced", "production", "saved"]
