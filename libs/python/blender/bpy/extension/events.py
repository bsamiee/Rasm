"""Callbacks on host events: app handler lists, msgbus properties, and main-loop timers."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import auto, Flag
from functools import partial
import inspect
from pathlib import Path
from types import MethodType
from typing import Final, Self

import bpy
from expression import identity, Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
import msgspec

from blender.bpy.data.rna.descriptors import Struct
from blender.bpy.extension.identity import Identity, Resource
from blender.records.faults import Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Change(Flag):
    """Change kinds depsgraph update reports."""

    GEOMETRY = auto()
    TRANSFORM = auto()
    SHADING = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Handler(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Callback on one `bpy.app.handlers` list, held on the extension's one dispatcher there, each row distinct since equal lists name different host events."""

    handlers: list[Callable[..., None]]
    body: Callable[..., Outcome[None]]

    @contextmanager
    def held(self, identity: Identity) -> Generator[None]:
        """Holds body on the extension's dispatcher until scope exit.

        Args:
            identity: Extension owning the dispatcher and reports holding body faults.

        Yields:
            None while the dispatcher runs body with the leading host arguments it takes positionally.
        """
        dispatcher = _Dispatcher.found(self.handlers, identity)
        count = sum(parameter.kind in {inspect.Parameter.POSITIONAL_ONLY, inspect.Parameter.POSITIONAL_OR_KEYWORD} for parameter in inspect.signature(self.body).parameters.values())
        entry = (identity.reports.reported(self.body), count)
        with ExitStack() as stack:
            dispatcher.entries.append(entry)
            stack.callback(dispatcher.released, entry)
            yield


class Subscription(msgspec.Struct, frozen=True, kw_only=True):
    """Msgbus callback on one owner type property."""

    owner: type[Struct]
    setting: bpy.types.Property
    body: Callable[[], Outcome[None]]

    @property
    def prerequisites(self) -> tuple[Resource[object], ...]:
        """Extension bus the subscription holds its key on."""
        return (_BUS,)

    @contextmanager
    def held(self, identity: Identity) -> Generator[None]:
        """Holds key on the extension's bus until scope exit.

        Args:
            identity: Extension with the bus and reports holding notification faults.

        Yields:
            None while the bus holds the key.
        """
        bus, body = identity.held[_BUS], identity.reports.reported(self.body)

        def notified() -> None:
            """Runs body through reports, the host taking a plain function answering None."""
            return body().to_optional()

        key = ((self.owner, self.setting.identifier), notified)
        with ExitStack() as stack:
            bus.keys.append(key)
            stack.callback(bus.keys.remove, key)
            bus.renewed()
            yield


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work returning next interval, Nothing to stop, idle in background mode."""

    body: Callable[[], Outcome[Option[float]]]

    @contextmanager
    def held(self, identity: Identity) -> Generator[None]:
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
            bpy.app.timers.register(ticked, persistent=True)
            stack.callback(stopped)
            yield


# --- [SERVICES] -------------------------------------------------------------------------


class _Dispatcher:
    """One persistent callback an extension keeps on a handler list, skipping its runtime scenes and running each held callback over the arguments it takes."""

    __slots__ = ("entries", "handlers", "identity")

    def __init__(self, identity: Identity, handlers: list[Callable[..., None]]) -> None:
        """Appends the dispatcher to handler list, holding no callback of identity."""
        self.identity, self.handlers = identity, handlers
        self.entries: list[tuple[Callable[..., Option[None]], int]] = []
        self.handlers.append(self.dispatched)

    @classmethod
    def found(cls, handlers: list[Callable[..., None]], identity: Identity) -> Self:
        """Returns the dispatcher of identity's package on handler list, appending a new one when absent."""
        return (
            Block
            .of_seq(handlers)
            .choose(lambda held: Option.of_optional(try_downcast(MethodType, held)))
            .choose(lambda method: Option.of_optional(try_downcast(cls, method.__self__)))
            .filter(lambda owner: owner.identity.package == identity.package)
            .try_head()
            .default_with(lambda: cls(identity, handlers))
        )

    @bpy.app.handlers.persistent  # type: ignore[untyped-decorator]  # ty: ignore[dynamic-function-decorator-return]
    def dispatched(self, *arguments: object) -> None:
        """Runs every held callback over the arguments it takes, skipping the extension's runtime scenes."""
        match arguments:
            case (bpy.types.Scene() as scene, *_) if staged(self.identity, scene):
                return
            case _:
                for entry, count in tuple(self.entries):
                    _ = entry(*arguments[:count])

    def released(self, entry: tuple[Callable[..., Option[None]], int]) -> None:
        """Drops entry, removing the dispatcher from its handler list once it holds none."""
        self.entries.remove(entry)
        if not self.entries:
            self.handlers.remove(self.dispatched)


class _Bus:
    """Msgbus owner of one extension with its keys, each subscribed again after every file load."""

    __slots__ = ("keys",)

    def __init__(self) -> None:
        """Holds no key."""
        self.keys: list[tuple[tuple[type[Struct], str], Callable[[], None]]] = []

    @classmethod
    @contextmanager
    def opened(cls, _identity: Identity) -> Generator[Self]:
        """Holds a new bus, clearing its subscriptions at exit."""
        bus = cls()
        with ExitStack() as stack:
            stack.callback(bpy.msgbus.clear_by_owner, bus)
            yield bus

    def renewed(self) -> Self:
        """Subscribes every held key, clearing earlier subscriptions first."""
        bpy.msgbus.clear_by_owner(self)
        for key, notify in self.keys:
            bpy.msgbus.subscribe_rna(key=key, owner=self, args=(), notify=notify)  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
        return self


_BUS: Final = Resource[_Bus](made=_Bus.opened, renewed=Some(_Bus.renewed))

# --- [OPERATIONS] -----------------------------------------------------------------------


def staged(identity: Identity, scene: bpy.types.Scene) -> bool:
    """Answers whether scene is one of extension's runtime scenes.

    Args:
        identity: Extension whose manifest ID marks its runtime scenes.
        scene: Scene a callback received.

    Returns:
        True for a runtime scene holding the extension's mark.
    """
    return scene.is_runtime_data and identity.manifest.id in scene  # type: ignore[operator]


def changed(update: bpy.types.DepsgraphUpdate) -> Change:
    """Reads change flags of update during its handler call.

    Args:
        update: Update holding geometry, transform, and shading flags.

    Returns:
        Reported change kinds, `Change(0)` when none.
    """
    return (
        (Change.GEOMETRY if update.is_updated_geometry else Change(0)) | (Change.TRANSFORM if update.is_updated_transform else Change(0)) | (Change.SHADING if update.is_updated_shading else Change(0))
    )


def located(body: Callable[[Option[Path]], Outcome[None]], path: str) -> Outcome[None]:
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
    updates = tuple(item for item in depsgraph.updates if isinstance(item.id, ids) and changes.map(lambda kinds: bool(changed(item) & kinds)).default_value(True))
    return body(scene, updates) if updates else Ok(None)


def startup(body: Callable[[], Outcome[None]]) -> tuple[Handler, Timer]:
    """Runs body on the first main-loop pass after registration and after every file load.

    Args:
        body: Work taking no argument.

    Returns:
        The `load_post` handler and the one-shot timer running body, the timer idle in background mode.
    """
    return Handler(handlers=bpy.app.handlers.load_post, body=body), Timer(body=partial(_once, body))


def _once(body: Callable[[], Outcome[None]]) -> Outcome[Option[float]]:
    """Runs body, stopping the timer."""
    return body().map(lambda _: Nothing)


def _ticked(body: Callable[[], Option[Option[float]]]) -> float | None:
    """Returns the next interval, `None` to stop the timer."""
    return body().bind(identity).to_optional()


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Change", "Handler", "Subscription", "Timer", "changed", "located", "staged", "startup", "updated"]
