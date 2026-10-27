"""Render results: completed-image facts, intermediate renders, and displayed result export."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack, nullcontext
from pathlib import Path
from typing import Final

import bpy
from expression import Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.data.ids.store import Handle
from blender.bpy.data.rna.descriptors import assigned, Descriptor, Value
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.events.handlers import SceneHandler
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Band, entered
from blender.bpy.extension.scoped import Scoped
from blender.bpy.faults import BlenderReason
from blender.imaging.color.processors import DisplayView
from blender.records.faults import Fault, Outcome, paired
from blender.records.stores import committed

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

    def acquisitions(self, identity: Identity) -> Outcome[Acquisitions]:
        """Returns intermediate cell and render completion handler deferring consumption."""

        def consumed(handle: Handle[bpy.types.Scene]) -> Outcome[Option[str]]:
            return handle.resolve().bind(lambda scene: saved(scene).bind(lambda path: self.consume(scene, path)))

        @contextmanager
        def acquired() -> Generator[Outcome[None]]:
            """Removes completion handler, then retires deferred writes."""
            with ExitStack() as stack:
                deferred = stack.enter_context(identity.reports.deferred(consumed))
                yield (
                    SceneHandler(handlers=bpy.app.handlers.render_complete, body=lambda scene: Ok(None if scene.session_uid in identity.held[INTERMEDIATES] else deferred(Handle.of(scene))))
                    .acquisitions(identity)
                    .bind(lambda pairs: entered(stack, tuple(acquisition for _, acquisition in pairs)))
                )

        return Ok((INTERMEDIATES, (Band.APP_HANDLERS, acquired())))


# --- [SERVICES] -------------------------------------------------------------------------

INTERMEDIATES: Final = Scoped[set[int]](made=lambda _identity: nullcontext(set()))
PRODUCTIONS: Final = Scoped[dict[tuple[int, int | None], Option[Production]]](made=lambda _identity: nullcontext({}), renewed=Some(lambda _held: {}))

# --- [OPERATIONS] -----------------------------------------------------------------------


@contextmanager
def intermediate(identity: Identity, scene: bpy.types.Scene) -> Generator[None]:
    """Hides scene renders from the extension's completed-image consumers during scope."""
    held = identity.held[INTERMEDIATES]
    held.add(scene.session_uid)
    try:
        yield
    finally:
        held.remove(scene.session_uid)


def produced(identity: Identity, image: bpy.types.Image, production: Option[Production]) -> None:
    """Records production facts of displayed slot."""
    identity.held[PRODUCTIONS][image.session_uid, image.render_slots.active_index] = production


def production(identity: Identity, image: bpy.types.Image) -> Outcome[Production]:
    """Returns recorded production facts of displayed slot."""
    return (
        identity.held[PRODUCTIONS]
        .get((image.session_uid, image.render_slots.active_index), Nothing)
        .to_result_with(lambda: (Fault(reason=BlenderReason.ABSENT, subject=repr(image), member="production facts"),))
    )


def built(kind: str) -> Outcome[bpy.types.Image]:
    """Returns Render Result or shared compositor Viewer image, or absent fault before Blender creates it.

    Callers read Viewer completion pixels before deferring.
    """
    return Option.of_optional(next((image for image in bpy.data.images if image.type == kind), None)).to_result_with(
        lambda: (Fault(reason=BlenderReason.ABSENT, subject=type(bpy.data.images).__name__, member=kind),)
    )


def saved(scene: bpy.types.Scene) -> Outcome[str]:
    """Exports displayed render result passes to temporary multilayer EXR, the scene's format restored after."""
    settings, path = scene.render.image_settings, Path(bpy.app.tempdir, f"{scene.session_uid}.exr")
    form: dict[str, Value] = {"media_type": "MULTI_LAYER_IMAGE", "color_depth": "32", "exr_codec": "ZIP", "use_exr_interleave": False, "views_format": "MULTIVIEW", "color_management": "FOLLOW_SCENE"}
    direct = Descriptor.saved(bpy.types.ImageFormatSettings, lambda prop: not isinstance(prop, bpy.types.PointerProperty))

    def exported(image: bpy.types.Image) -> Outcome[str]:
        return assigned(settings, form).bind(
            lambda _: committed(
                path,
                lambda stage: catch(image.save_render, exception=RuntimeError)(str(stage), scene=scene).map_error(
                    lambda error: (Fault(reason=BlenderReason.REFUSED, subject=repr(image), cause=error),)
                ),
            ).map(str)
        )

    return paired(built("RENDER_RESULT"), Snapshot.capture(settings, Some(direct))).bind(lambda held: paired(exported(held[0]), held[1].apply(settings).outcome)).map(lambda pair: pair[0])


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["INTERMEDIATES", "PRODUCTIONS", "Production", "ResultRead", "built", "intermediate", "produced", "production", "saved"]
