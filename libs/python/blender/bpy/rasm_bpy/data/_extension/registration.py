"""Registration bands, callback lifetimes, typed resources, class dependencies, and extension registries."""

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack, nullcontext
from dataclasses import dataclass, field
from enum import auto, Enum
from functools import partial, reduce
import graphlib
import inspect
from itertools import chain
import os
import sys
from threading import Event
from types import MappingProxyType
from typing import ClassVar, Final, NoReturn, Protocol, runtime_checkable

import bpy
from expression import Option, Result, Some
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from rasm_bpy.data._extension.events import runtime
from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.reports import Refusal, rendered
from rasm_bpy.data._rna.paths import rna_definition, root_in, Struct
from rasm_bpy.data._rna.properties import annotated, hosting
from rasm_records import Error, Fault, Faults, Nothing, Ok, Outcome, Reason
from rasm_rendering import argued

# --- [TYPES] ----------------------------------------------------------------------------


class Tier(Enum):
    """Acquisition order, with resources released in reverse."""

    GROUPS = auto()
    ATTACHMENTS = auto()
    RESOURCES = auto()
    PREFERENCES = auto()
    OPERATORS = auto()
    INTERFACE = auto()
    APPENDS = auto()
    KEYMAP_ITEMS = auto()
    DRAW_HANDLERS = auto()
    APP_HANDLERS = auto()
    SUBSCRIPTIONS = auto()
    TIMERS = auto()
    REGISTRIES = auto()


type _Acquisition = type | AbstractContextManager[Outcome[None]] | Classes
type Declaration = Registrable | Handler | Subscription | Timer
type Acquisitions = tuple[Declaration | type | tuple[Tier, _Acquisition], ...]
type Declarations = tuple[Declaration | Gathering | type | tuple[Tier, _Acquisition], ...]
type _Answer = Acquisitions | Outcome[Acquisitions] | AbstractContextManager[Acquisitions | Outcome[Acquisitions]]


type _Dispatcher = Callable[[*tuple[object, ...]], None]


class Handlers(Protocol):
    """`bpy.app.handlers` list a dispatcher accepting every host argument joins and leaves."""

    def append(self, dispatcher: _Dispatcher, /) -> None:
        """Joins the dispatcher to the host's calls."""

    def remove(self, dispatcher: _Dispatcher, /) -> None:
        """Drops the dispatcher from the host's calls."""


@runtime_checkable
class Registrable(Protocol):
    """Row building its acquisitions under extension identity."""

    def acquisitions(self, identity: Identity, /) -> _Answer:
        """Builds acquisitions under the extension identity, directly or yielded by a construction scope."""


@runtime_checkable
class _Dependent(Protocol):
    """Row whose build reads declarations the scope builds before every row."""

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations built before dependent rows build."""


@runtime_checkable
class Gathering(Protocol):
    """Row building its acquisitions over every declaration the scope built before it."""

    def gathered(self, identity: Identity, declared: "tuple[Declaration | Gathering, ...]", /) -> _Answer:
        """Builds acquisitions over the declarations of every other row, in build order."""


# --- [CONSTANTS] ------------------------------------------------------------------------

_EAGER: Final = (Tier.GROUPS, Tier.ATTACHMENTS, Tier.RESOURCES)
_BANDS: Final[Mapping[type, Tier]] = MappingProxyType({
    bpy.types.PropertyGroup: Tier.GROUPS,
    bpy.types.AddonPreferences: Tier.PREFERENCES,
    bpy.types.KeyConfigPreferences: Tier.PREFERENCES,
    bpy.types.Operator: Tier.OPERATORS,
    bpy.types.Macro: Tier.OPERATORS,
    bpy.types.bpy_struct: Tier.INTERFACE,
})

# --- [MODELS] ---------------------------------------------------------------------------


@dataclass(frozen=True, slots=True, kw_only=True)
class Classes:
    """Deferred native classes, invoked in their acquisition band even when their factory also implements a context manager."""

    build: Callable[[], Outcome[tuple[type, ...]]]


class Handler(msgspec.Struct, frozen=True, kw_only=True, eq=False):  # type: ignore[explicit-any]
    """Callback on one `bpy.app.handlers` list, each row distinct."""

    tier: ClassVar[Tier] = Tier.APP_HANDLERS
    handlers: Handlers
    body: Callable[..., Outcome[None]]  # type: ignore[explicit-any]
    scenes: Callable[[Identity, bpy.types.Scene], bool] = lambda identity, scene: not runtime(identity, scene)

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[Outcome[None]]:
        """Joins the persistent dispatcher until scope exit, applying the scene policy and passing the host arguments body takes."""
        body = identity.reports.reported(self.body)
        parameters = tuple(inspect.signature(self.body).parameters.values())
        count = (
            None
            if any(parameter.kind is inspect.Parameter.VAR_POSITIONAL for parameter in parameters)
            else sum(parameter.kind in {inspect.Parameter.POSITIONAL_ONLY, inspect.Parameter.POSITIONAL_OR_KEYWORD} for parameter in parameters)
        )

        def called(*arguments: object) -> None:
            """Runs body over the leading arguments it takes."""
            match arguments:
                case (bpy.types.Scene() as scene, *_) if not self.scenes(identity, scene):
                    return
                case _:
                    _ = body(*arguments[:count])

        lists, key = DISPATCHERS[identity], id(self.handlers)
        if key not in lists:
            joined = list[_Dispatcher]()

            def dispatcher(*arguments: object) -> None:
                """Runs each joined call in join order, a call leaving during the pass skipped."""
                for each in (call for call in tuple(joined) if call in joined):
                    each(*arguments)

            bpy.app.handlers.persistent(dispatcher)
            lists[key] = (dispatcher, joined)
            self.handlers.append(dispatcher)
        dispatched, calls = lists[key]

        def left() -> None:
            """Drops the call, and the dispatcher with the list's last call."""
            calls.remove(called)
            if not calls:
                self.handlers.remove(dispatched)
                del lists[key]

        with ExitStack() as stack:
            calls.append(called)
            stack.callback(left)
            yield Ok(None)


class Subscription(msgspec.Struct, frozen=True, kw_only=True):
    """Msgbus callback on one owner type property, subscribed again after every file load clears the bus."""

    tier: ClassVar[Tier] = Tier.SUBSCRIPTIONS
    owner: type[Struct]
    setting: bpy.types.Property
    body: Callable[[], Outcome[None]]

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[Outcome[None]]:
        """Holds the subscription under its own msgbus owner until scope exit, notification faults logged by the extension's reports."""
        owner, body = object(), identity.reports.reported(self.body)

        def notified() -> None:
            """Runs body through reports, the host printing a `ValueError` for any notify return other than None."""
            _ = body()

        def subscribed() -> Outcome[None]:
            """Subscribes the key under owner."""
            bpy.msgbus.subscribe_rna(key=(self.owner, self.setting.identifier), owner=owner, args=(), notify=notified)  # ty: ignore[invalid-argument-type]
            return Ok(None)

        with ExitStack() as stack:
            stack.callback(bpy.msgbus.clear_by_owner, owner)
            _ = subscribed()
            yield stack.enter_context(Handler(handlers=bpy.app.handlers.load_post, body=subscribed).held(identity))


class Timer(msgspec.Struct, frozen=True, kw_only=True):
    """Main-loop work returning next interval, Nothing to stop, idle in background mode."""

    tier: ClassVar[Tier] = Tier.TIMERS
    body: Callable[[], Outcome[Option[float]]]
    first_interval: float = 0.0

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[Outcome[None]]:
        """Holds the persistent timer until it stops or scope exits, body faults logged by the extension's reports."""
        body = identity.reports.reported(self.body)

        def ticked() -> float | None:
            """Returns the next interval, `None` to stop the timer."""
            return body().bind(lambda interval: interval).to_optional()

        def stopped() -> None:
            """Unregisters the timer while the host holds it, one that returned `None` being gone."""
            if bpy.app.timers.is_registered(ticked):
                bpy.app.timers.unregister(ticked)

        with ExitStack() as stack:
            bpy.app.timers.register(ticked, first_interval=self.first_interval, persistent=True)
            stack.callback(stopped)
            yield Ok(None)


class Startup(msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Work run after every file load, and on the first main-loop pass after each registration unless a load ran it first.

    Blender startup registers extensions before load handlers and the first main-loop pass, and the work runs once there, the timer idle in background.
    """

    body: Callable[[], Outcome[None]]

    def acquisitions(self, _identity: Identity, /) -> tuple[Handler | Timer, ...]:
        """Builds the `load_post` handler and the one-shot timer sharing this registration's load mark."""
        loaded = Event()

        def reloaded() -> Outcome[None]:
            """Marks the load, then runs the work."""
            loaded.set()
            return self.body()

        def once() -> Outcome[Option[float]]:
            """Runs the work unless a file load ran it first, stopping the timer."""
            return (Ok(None) if loaded.is_set() else self.body()).map(lambda _: Nothing)

        return Handler(handlers=bpy.app.handlers.load_post, body=reloaded), Timer(body=once)


class Translations(msgspec.Struct, frozen=True, kw_only=True):
    """Interface text translations registered under extension package, in the host form keyed by locale, then by translation context and message."""

    table: Mapping[str, Mapping[tuple[str, str], str]]

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds the registry acquisition holding the table under the extension package."""
        table = {locale: dict(entries) for locale, entries in self.table.items()}
        return ((Tier.REGISTRIES, scoped(identity.package, partial(bpy.app.translations.register, identity.package, table), lambda _: bpy.app.translations.unregister(identity.package))),)


class CliCommand[A](msgspec.Struct, frozen=True, kw_only=True):
    """Command-line command with base64 JSON first argument, exiting with the code its run returns, 1 after faults."""

    key: Key
    decoder: msgspec.json.Decoder[A]
    run: Callable[[A, tuple[str, ...]], Outcome[int]]

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds the prefixed command acquisition, stderr holding command faults."""
        command = identity.identifier(self.key)
        return ((Tier.REGISTRIES, scoped(command, partial(bpy.utils.register_cli_command, command, partial(self.traced, identity, command)), bpy.utils.unregister_cli_command)),)

    def executed(self, command: str, arguments: list[str], /) -> Outcome[int]:
        """Runs over the decoded first argument and the rest unchanged, unreadable fault for an undecodable document, absent fault for no argument."""
        match arguments:
            case [document, *trailing]:
                return argued(self.decoder, command, document).bind(lambda value: self.run(value, tuple(trailing)))
            case _:
                return Error(Fault.of(Reason.ABSENT, command, member="arguments"))

    def traced(self, identity: Identity, command: str, arguments: list[str], /) -> int:
        """Runs the command as the program of its process: recorded under the command's name, inside one span continuing the trace its launcher wrote into the environment, its faults and the one `INFO` entry of the run logged, returning its exit code."""
        with identity.reports.commanded(command, os.environ):
            answer = identity.reports.spanned(command, partial(self.executed, command, arguments))
            identity.reports.faulted(answer)
            status = answer.default_with(_refused)
            identity.reports.logger.info("Command ended", status=status)
            return status


# --- [SERVICES] -------------------------------------------------------------------------


@dataclass(eq=False, slots=True, kw_only=True)
class Resource[T]:
    """Value each extension holds while registered, made after its prerequisites and optionally renewed after every file load."""

    # --- [STATE]

    made: Callable[[Identity], AbstractContextManager[T]]
    renewed: Option[Callable[[T], T]] = Nothing
    prerequisites: tuple[Declaration, ...] = ()
    _values: dict[Identity, T] = field(default_factory=dict, init=False)

    # --- [VALUES]

    def __getitem__(self, identity: Identity, /) -> T:
        """Returns the value held by this registration."""
        return self._values[identity]

    def get(self, identity: Identity, /) -> Option[T]:
        """Returns the registration's value, Nothing outside its resource scope."""
        try:
            return Some(self._values[identity])
        except KeyError:
            return Nothing

    # --- [LIFECYCLE]

    @contextmanager
    def held(self, identity: Identity, /) -> Generator[Outcome[None]]:
        """Holds the value made for this registration until scope exit."""
        with ExitStack() as stack:
            self._values[identity] = stack.enter_context(self.made(identity))
            stack.callback(self._values.pop, identity)
            yield Ok(None)

    def updated(self, identity: Identity, renew: Callable[[T], T], /) -> Outcome[None]:
        """Replaces the registration's value with its renewal after a file load."""
        self._values[identity] = renew(self[identity])
        return Ok(None)

    # --- [REGISTRATION]

    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds the resource scope and native renewal handlers."""
        return ((Tier.RESOURCES, self.held(identity)), *(Handler(handlers=bpy.app.handlers.load_post, body=partial(self.updated, identity, renew)) for renew in self.renewed.to_list()))


DISPATCHERS: Final = Resource[dict[int, tuple[_Dispatcher, list[_Dispatcher]]]](made=lambda _identity: nullcontext({}))


class Registration:
    """Register and unregister callbacks of one extension over its rows, rebuilt on each registration.

    Handler dispatcher builds first, prerequisites precede their rows, resources are held once built, and gathering rows build last.
    Equal declarations build once, each class acquires in the band of its host base, and a manifest website holds Online Manual links over the extension.
    Host registration raises rendered faults after partial acquisitions release; scoped registration returns typed faults.
    """

    __slots__ = ("_held", "package", "rows", "unregister")

    def __init__(self, package: str, rows: Declarations, /) -> None:
        """Holds the package `Identity.read` reads and the rows, with nothing registered, `unregister` releasing every registration in reverse."""
        self.package, self.rows, self._held = package, rows, ExitStack()
        self.unregister: Callable[[], None] = self._held.close

    @contextmanager
    def opened(self) -> Generator[Outcome[Identity]]:
        """Yields the acquired identity or registration faults, releasing acquisitions at scope exit."""

        def merged() -> None:
            for manager in root_in(bpy.types.WindowManager, bpy.context).to_list():
                manager.keyconfigs.update(keep_properties=True)

        def registered(stack: ExitStack, identity: Identity) -> Outcome[None]:
            """Enters keymap merge and band stacks with the manual map hook of a manifest website, builds rows, and enters every band."""

            def manual(website: str) -> AbstractContextManager[Outcome[None]]:
                hook = partial(_mapped, identity, website)
                return scoped(website, partial(bpy.utils.register_manual_map, hook), lambda _: bpy.utils.unregister_manual_map(hook))

            def opened(band: Tier) -> Outcome[None]:
                fresh = tuple((kind, acquisition) for (kind, acquisition), done in pairs.items() if kind is band and not done)
                return entered(identity, stacks[band], tuple(acquisition for _, acquisition in fresh)).map(lambda _: pairs.update(dict.fromkeys(fresh, True)))

            def declared(declaration: Declaration | Gathering, answer: _Answer) -> Outcome[Acquisitions]:
                declarations.append(declaration)
                held = stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer
                return held if isinstance(held, Result) else Ok(held)

            def built(row: Declaration | Gathering | type | tuple[Tier, _Acquisition]) -> Outcome[None]:
                answer: Callable[[], _Answer]
                match row:
                    case type() as cls:
                        return (
                            Block
                            .of_seq(cls.__mro__)
                            .choose(lambda base: Option.of_optional(_BANDS.get(base)))
                            .try_head()
                            .to_result_with(partial(Fault.of, Reason.ABSENT, cls.__name__, member="registrable host base"))
                            .map(lambda band: pairs.setdefault((band, cls), False))
                            .map(lambda _: None)
                        )
                    case _ if row in declarations:
                        return Ok(None)
                    case Handler() | Subscription() | Timer() as event:
                        answer = lambda: ((event.tier, event.held(identity)),)
                    case Gathering() as gathering:
                        answer = lambda: gathering.gathered(identity, tuple(declarations))
                    case Registrable() as registrable:
                        answer = partial(registrable.acquisitions, identity)
                    case _:
                        pairs.setdefault(row, False)
                        return Ok(None)
                return _each(built, _prerequisites(row)).bind(lambda _: _each(opened, _EAGER)).bind(lambda _: declared(row, answer())).bind(partial(_each, built)).bind(lambda _: _each(opened, _EAGER))

            stack.callback(merged)
            stacks = {band: stack.enter_context(ExitStack()) for band in Tier}
            declarations: list[Declaration | Gathering] = []
            pairs: dict[tuple[Tier, _Acquisition], bool] = dict.fromkeys(((Tier.REGISTRIES, manual(website)) for website in Option.of_optional(identity.manifest.website).to_list()), False)
            prerequisites = chain.from_iterable(map(_prerequisites, self.rows))
            return _each(built, (DISPATCHERS, *prerequisites, *sorted(self.rows, key=lambda row: isinstance(row, Gathering)))).bind(lambda _: _each(opened, Tier))

        with ExitStack() as stack:
            yield Identity.read(self.package).bind(
                lambda identity: (
                    catch(stack.enter_context, exception=OSError)(identity.reports)
                    .map_error(partial(Fault.of, Reason.UNWRITABLE, str(identity.folder)))
                    .bind(lambda _: registered(stack, identity))
                    .map(lambda _: identity)
                )
            )

    def register(self) -> None:
        """Holds the acquired scope until unregister, raising rendered registration faults after cleanup."""

        def refused(faults: Faults) -> NoReturn:
            raise RuntimeError("\n".join(text for _, text in rendered(faults)))

        with ExitStack() as stack:
            stack.enter_context(self.opened()).map(lambda _: self._held.push(stack.pop_all())).default_with(refused)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [HANDLERS]


def _refused(faults: tuple[Fault, ...], /) -> int:
    """Writes command faults to stderr, returning the failing exit code."""
    sys.stderr.write("".join(f"{fault.message}\n" for fault in faults))
    return 1


# --- [ROWS]


def acquired_rows(rows: Iterable[object]) -> tuple[Declaration, ...]:
    """Returns prerequisites of rows that register nothing, then the rows that register."""
    held = tuple(rows)
    return (
        *(declaration for row in held if isinstance(row, _Dependent) and not isinstance(row, Registrable) for declaration in row.prerequisites),
        *(row for row in held if isinstance(row, Registrable)),
    )


def _prerequisites(row: object, /) -> tuple[Declaration, ...]:
    """Declarations a dependent row reads, none for any other row."""
    return row.prerequisites if isinstance(row, _Dependent) else ()


# --- [CLASSES]


def entered(identity: Identity, stack: ExitStack, acquisitions: tuple[_Acquisition, ...], /) -> Outcome[None]:
    """Registers a band's classes after the pointer targets and parent panels they name, then its managers and deferred builds, onto stack until the first refusal or dependency cycle."""
    classes = tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type) and issubclass(acquisition, bpy.types.bpy_struct))
    declared = {cls: {name: value for base in reversed(cls.__mro__) for name, value in vars(base).items()} for cls in classes}
    panels: dict[object, type[Struct]] = {declared[panel].get("bl_idname", panel.__name__): panel for panel in classes if issubclass(panel, bpy.types.Panel)}
    targets = {
        cls: {
            *(prop.keywords["type"] for prop in annotated(cls).values() if prop.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}),
            *(panels[name] for name in panels.keys() & {declared[cls].get("bl_parent_id")}),
        }
        for cls in classes
    }
    return (
        catch(tuple, exception=graphlib.CycleError)(graphlib.TopologicalSorter({cls: held & targets.keys() for cls, held in targets.items()}).static_order())
        .map_error(partial(Fault.of, Refusal.UNREGISTERED, ", ".join(cls.__name__ for cls in classes)))
        .bind(lambda order: _each(partial(_registered, identity, stack), (*order, *(acquisition for acquisition in acquisitions if acquisition not in classes))))
    )


def _registered(identity: Identity, stack: ExitStack, acquisition: _Acquisition, /) -> Outcome[None]:
    """Registers a class and pushes its release, enters a manager, or registers the classes a deferred build returns.

    A property group registers under the extension's group name, the host taking its RNA identifier from the class name at registration, and keeps its own name after.

    Returns:
        Ok, or the `UNREGISTERED` fault of a class the host refused, or the faults of a manager or deferred build.
    """
    match acquisition:
        case type() as cls:
            named = cls.__name__

            def released() -> None:
                """Unregisters class while the host holds it, a class whose property or `register` raised after the host took it included."""
                if "bl_rna" in vars(cls):
                    bpy.utils.unregister_class(cls)  # ty: ignore[invalid-argument-type]

            stack.callback(released)
            try:
                cls.__name__ = identity.name(bpy.types.PropertyGroup, named) if issubclass(cls, bpy.types.PropertyGroup) else named
                with hosting(cls):
                    bpy.utils.register_class(cls)  # ty: ignore[invalid-argument-type]
            except (ValueError, RuntimeError, AttributeError) as error:
                return Error(Fault.of(Refusal.UNREGISTERED, named, error))
            finally:
                cls.__name__ = named
            return Ok(None)
        case Classes(build=build):
            return build().bind(partial(entered, identity, stack))
        case manager:
            return stack.enter_context(manager)


def _each[T](step: Callable[[T], Outcome[None]], items: Iterable[T], /) -> Outcome[None]:
    """Runs step over items in order until the first refusal."""
    return reduce(lambda result, item: result.bind(lambda _: step(item)), items, Result[None, Faults](ok=None))


# --- [REGISTRIES]


@contextmanager
def scoped[H](subject: str, acquired: Callable[[], H], released: Callable[[H], object], /, *, exception: type[Exception] = ValueError) -> Generator[Outcome[None]]:
    """Runs a host acquisition and holds its handle's release until scope exit, yielding the unregistered fault on subject when it raises exception."""
    with ExitStack() as stack:
        yield catch(acquired, exception=exception)().map(lambda handle: stack.callback(released, handle)).map(lambda _: None).map_error(partial(Fault.of, Refusal.UNREGISTERED, subject))


def _mapped(identity: Identity, website: str, /) -> tuple[str, list[tuple[str, str]]]:
    """Returns website with extension operator, type, and property patterns, and one per preferences struct and property group a property reaches."""
    category, prefix, group = identity.manifest.id.lower(), identity.manifest.prefix, rna_definition(bpy.types.PropertyGroup)

    def grouped(struct: bpy.types.Struct) -> bool:
        return struct.base == group

    attached = tuple(target for identifier, target in _walked((rna_definition(bpy.types.BlendData),), lambda _: True) if identity.owns(identifier) and grouped(target))
    owned = identity.preferences(bpy.types.AddonPreferences).map(rna_definition).to_list()
    nested = tuple(target for _, target in _walked((*attached, *owned), grouped) if grouped(target))
    return website, [
        *((f"bpy.ops.{category}.{key}", f"#{key}") for key in dir(getattr(bpy.ops, category))),
        (f"bpy.types.{prefix}*", ""),
        (f"bpy.types.*.{prefix}*", ""),
        *((f"bpy.types.{identifier.lower()}.*", "") for identifier in dict.fromkeys(struct.identifier for struct in (*owned, *attached, *nested))),
    ]


def _walked(starts: tuple[bpy.types.Struct, ...], followed: Callable[[bpy.types.Struct], bool], /) -> tuple[tuple[str, bpy.types.Struct], ...]:
    """Returns identifier and fixed target of each pointer and collection property breadth-first, entering each target `followed` accepts once."""
    seen, frontier, edges = {start.identifier for start in starts}, deque(starts), list[tuple[str, bpy.types.Struct]]()
    while frontier:
        properties = frontier.popleft().properties
        for identifier, target in (
            (prop.identifier, target) for prop in properties if isinstance(prop, bpy.types.PointerProperty | bpy.types.CollectionProperty) for target in Option.of_optional(prop.fixed_type).to_list()
        ):
            edges.append((identifier, target))
            if target.identifier not in seen and followed(target):
                seen.add(target.identifier)
                frontier.append(target)
    return tuple(edges)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "DISPATCHERS",
    "Acquisitions",
    "Classes",
    "CliCommand",
    "Declaration",
    "Declarations",
    "Gathering",
    "Handler",
    "Handlers",
    "Registrable",
    "Registration",
    "Resource",
    "Startup",
    "Subscription",
    "Tier",
    "Timer",
    "Translations",
    "acquired_rows",
    "entered",
    "scoped",
]
