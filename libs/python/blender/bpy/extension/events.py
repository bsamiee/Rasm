"""Callbacks on host events: app handler lists, msgbus properties, and main-loop timers."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import auto, Flag
from functools import reduce
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
        """Reads the change kinds an update reports during its handler call, `Change(0)` for none."""
        flags = (update.is_updated_geometry, update.is_updated_transform, update.is_updated_shading)
        return reduce(or_, (kind for kind, flagged in zip(cls, flags, strict=True) if flagged), cls(0))


# --- [MODELS] ---------------------------------------------------------------------------


class Handler(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Callback on one `bpy.app.handlers` list, each row distinct since equal lists name different host events."""

    handlers: list[Callable[..., None]]
    body: Callable[..., Outcome[None]]

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[None]:
        """Holds one persistent callback on the handler list until scope exit, running body over the leading host arguments it takes, skipping the extension's runtime scenes."""
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
        """Holds the subscription under its own msgbus owner until scope exit, notification faults held in the extension's reports."""
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
        """Holds the persistent timer until it stops or scope exits, body faults held in the extension's reports."""
        body = identity.reports.reported(self.body)

        def ticked() -> float | None:
            """Returns the next interval, `None` to stop the timer."""
            return body().bind(lambda interval: interval).to_optional()

        def stopped() -> None:
            """Unregisters the timer while the host holds it, one that answered `None` being gone."""
            if bpy.app.timers.is_registered(ticked):
                bpy.app.timers.unregister(ticked)

        with ExitStack() as stack:
            bpy.app.timers.register(ticked, first_interval=self.first_interval, persistent=True)
            stack.callback(stopped)
            yield


class Startup(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Work run after every file load, and on the first main-loop pass after each registration unless a load ran it first.

    Blender startup registers extensions before the startup file's load handlers run and before the first main-loop pass, so the work runs once there, the timer idle in background mode.
    """

    body: Callable[[], Outcome[None]]

    def acquisitions(self, _identity: Identity, /) -> Outcome[tuple[Handler | Timer, ...]]:
        """Builds the `load_post` handler and the one-shot timer sharing this registration's load mark."""
        loaded = Event()

        def reloaded() -> Outcome[None]:
            """Marks the load, then runs the work."""
            loaded.set()
            return self.body()

        def once() -> Outcome[Option[float]]:
            """Runs the work unless a file load ran it first, stopping the timer."""
            return (Ok(None) if loaded.is_set() else self.body()).map(lambda _: Nothing)

        return Ok((Handler(handlers=bpy.app.handlers.load_post, body=reloaded), Timer(body=once)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def runtime(identity: Identity, scene: bpy.types.Scene, /) -> bool:
    """Answers whether a scene a callback received is a runtime scene holding the extension's manifest ID as its mark."""
    return scene.is_runtime_data and identity.manifest.id in scene  # type: ignore[operator]


def located(body: Callable[[Option[Path]], Outcome[None]], path: str, /) -> Outcome[None]:
    """Passes the path a load or save handler receives to body, Nothing for the startup file's empty path."""
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
    """Passes the scene and this pass's depsgraph updates of the ID types reporting one of the change kinds (any kind for Nothing) to body, Ok(None) when none matches."""
    updates = tuple(item for item in depsgraph.updates if isinstance(item.id, ids) and changes.map(lambda kinds: bool(Change.of(item) & kinds)).default_value(True))
    return body(scene, updates) if updates else Ok(None)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Change", "Handler", "Startup", "Subscription", "Timer", "located", "runtime", "updated"]
