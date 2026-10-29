"""Callbacks on host events: app handler lists, msgbus properties, and main-loop timers."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import auto, Flag
from functools import partial, reduce
import inspect
from operator import or_
from pathlib import Path
from threading import Event
from typing import Self

import bpy
from expression import Nothing, Ok, Option
import msgspec

from blender.bpy.data.rna.paths import Struct
from blender.bpy.extension.identity import Identity
from blender.records.faults import Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Change(Flag):
    """Change kinds depsgraph update reports, in the order of the update flags reading them."""

    GEOMETRY = auto()
    TRANSFORM = auto()
    SHADING = auto()

    @classmethod
    def of(cls, update: bpy.types.DepsgraphUpdate, /) -> Self:
        """Reads change flags of update during its handler call.

        Args:
            update: Update holding geometry, transform, and shading flags.

        Returns:
            Reported change kinds, `Change(0)` when none.
        """
        flags = (update.is_updated_geometry, update.is_updated_transform, update.is_updated_shading)
        return reduce(or_, (kind for kind, flagged in zip(cls, flags, strict=True) if flagged), cls(0))


# --- [MODELS] ---------------------------------------------------------------------------


class Handler(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Callback on one `bpy.app.handlers` list, each row distinct since equal lists name different host events."""

    handlers: list[Callable[..., None]]
    body: Callable[..., Outcome[None]]

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[None]:
        """Holds one persistent callback on the handler list until scope exit, file loads keeping it.

        Args:
            identity: Extension whose runtime scenes the callback skips and whose reports hold body faults.

        Yields:
            None while the callback runs body with the leading host arguments it takes positionally.
        """
        body = identity.reports.reported(self.body)
        count = sum(parameter.kind in {inspect.Parameter.POSITIONAL_ONLY, inspect.Parameter.POSITIONAL_OR_KEYWORD} for parameter in inspect.signature(self.body).parameters.values())

        @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
        def called(*arguments: object) -> None:
            """Runs body over the arguments it takes, the host passing both to a callback declaring no single positional parameter."""
            match arguments:
                case (bpy.types.Scene() as scene, *_) if runtime(identity, scene):
                    return
                case _:
                    _ = body(*arguments[:count])

        with ExitStack() as stack:
            self.handlers.append(called)
            stack.callback(self.handlers.remove, called)
            yield


class Subscription(msgspec.Struct, frozen=True, kw_only=True):
    """Msgbus callback on one owner type property, subscribed again after every file load clears the bus."""

    owner: type[Struct]
    setting: bpy.types.Property
    body: Callable[[], Outcome[None]]

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[None]:
        """Holds the subscription under its own msgbus owner until scope exit.

        Args:
            identity: Extension with reports holding notification faults.

        Yields:
            None while the bus holds the key.
        """
        owner, body = object(), identity.reports.reported(self.body)

        def notified() -> None:
            """Runs body through reports, the host taking a plain function answering None."""
            _ = body()

        def subscribed() -> Outcome[None]:
            """Subscribes the key under owner."""
            bpy.msgbus.subscribe_rna(key=(self.owner, self.setting.identifier), owner=owner, args=(), notify=notified)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            return Ok(None)

        with ExitStack() as stack:
            stack.callback(bpy.msgbus.clear_by_owner, owner)
            _ = subscribed()
            stack.enter_context(Handler(handlers=bpy.app.handlers.load_post, body=subscribed).held(identity))
            yield


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work returning next interval, Nothing to stop, idle in background mode."""

    body: Callable[[], Outcome[Option[float]]]
    first_interval: float = 0.0

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[None]:
        """Holds persistent timer until it stops or scope exits.

        Args:
            identity: Extension with reports holding body faults.

        Yields:
            None while the host holds the timer.
        """
        ticked = partial(_ticked, identity.reports.reported(self.body))

        def stopped() -> None:
            """Unregisters the timer while the host holds it, one that answered `None` being gone."""
            if bpy.app.timers.is_registered(ticked):
                bpy.app.timers.unregister(ticked)

        with ExitStack() as stack:
            bpy.app.timers.register(ticked, first_interval=self.first_interval, persistent=True)
            stack.callback(stopped)
            yield


# --- [OPERATIONS] -----------------------------------------------------------------------


def runtime(identity: Identity, scene: bpy.types.Scene, /) -> bool:
    """Answers whether scene is one of extension's runtime scenes.

    Args:
        identity: Extension whose manifest ID marks its runtime scenes.
        scene: Scene a callback received.

    Returns:
        True for a runtime scene holding the extension's mark.
    """
    return scene.is_runtime_data and identity.manifest.id in scene  # type: ignore[operator]


def located(body: Callable[[Option[Path]], Outcome[None]], path: str, /) -> Outcome[None]:
    """Passes the loaded or saved file path to body, Nothing for the startup file.

    Args:
        body: Callback reading the file path.
        path: Path a load or save handler receives, empty for the startup file.

    Returns:
        Body result.
    """
    return body(Option.of_optional(path or None).map(Path))


def updated(
    ids: tuple[type[bpy.types.ID], ...],
    body: Callable[[bpy.types.Scene, tuple[bpy.types.DepsgraphUpdate, ...]], Outcome[None]],
    scene: bpy.types.Scene,
    depsgraph: bpy.types.Depsgraph,
    /,
    *,
    changes: Option[Change] = Nothing,
) -> Outcome[None]:
    """Passes the depsgraph updates of ID types and optional change kinds to body.

    Args:
        ids: ID types whose updates body reads.
        body: Callback receiving the scene and matching updates.
        scene: Scene the handler received.
        depsgraph: Evaluated depsgraph holding this pass's updates.
        changes: Change kinds an update must report, any kind when Nothing.

    Returns:
        Body result, Ok(None) when no update matches.
    """
    updates = tuple(item for item in depsgraph.updates if isinstance(item.id, ids) and changes.map(lambda kinds: bool(Change.of(item) & kinds)).default_value(True))
    return body(scene, updates) if updates else Ok(None)


def startup(body: Callable[[], Outcome[None]], /) -> tuple[Handler, Timer]:
    """Runs body after every file load, and on the first main-loop pass after registration unless a load ran it first.

    Blender startup registers extensions before the startup file's load handlers run and before the first main-loop pass, so body runs once there.
    Rows are built per registration, each pair sharing its own load mark.

    Args:
        body: Work taking no argument.

    Returns:
        The `load_post` handler and the one-shot timer running body, the timer idle in background mode.
    """
    loaded = Event()

    def reloaded() -> Outcome[None]:
        """Marks the load, then runs body."""
        loaded.set()
        return body()

    return Handler(handlers=bpy.app.handlers.load_post, body=reloaded), Timer(body=partial(_once, body, loaded))


def _once(body: Callable[[], Outcome[None]], loaded: Event, /) -> Outcome[Option[float]]:
    """Runs body unless a file load ran it first, stopping the timer."""
    return (Ok(None) if loaded.is_set() else body()).map(lambda _: Nothing)


def _ticked(body: Callable[[], Option[Option[float]]], /) -> float | None:
    """Returns the next interval, `None` to stop the timer."""
    return body().bind(lambda interval: interval).to_optional()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Change", "Handler", "Subscription", "Timer", "located", "runtime", "startup", "updated"]
