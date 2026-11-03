"""Registration bands, class registration in dependency order, translation, manual, and CLI registries, and the extension service."""

from collections import deque
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import auto, Enum
from functools import partial, reduce
import graphlib
from itertools import chain
from types import MappingProxyType
from typing import Any, Final, NoReturn, Protocol, runtime_checkable

import bpy
from expression import Error, Ok, Option, Result
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.data.rna.properties import annotated
from blender.bpy.extension.events import DISPATCHERS, Handler, Subscription, Timer
from blender.bpy.extension.identity import Identity, Key, Resource
from blender.bpy.extension.reports import rendered
from blender.records.faults import Fault, Faults, Outcome, Reason
from blender.records.stores import argued

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


type Acquisition = type | AbstractContextManager[Outcome[None]] | Callable[[], Outcome[tuple[type, ...]]]
type Declaration = Registrable | Resource[Any] | Handler | Subscription | Timer
type _Banded = tuple[Tier, Acquisition]
type Acquisitions = tuple[Declaration | type | _Banded, ...]
type _Answer = Outcome[Acquisitions] | AbstractContextManager[Outcome[Acquisitions]]


@runtime_checkable
class Registrable(Protocol):
    """Row building its acquisitions under extension identity."""

    def acquisitions(self, identity: Identity, /) -> _Answer:
        """Builds acquisitions under the extension identity, directly or yielded by a construction scope."""


@runtime_checkable
class Dependent(Protocol):
    """Row whose build reads declarations the scope builds before every row."""

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations built before dependent rows build."""


@runtime_checkable
class Gathering(Protocol):
    """Row building its acquisitions over every declaration the scope built before it."""

    def gathered(self, identity: Identity, declared: "tuple[Declaration | Gathering, ...]", /) -> Outcome[Acquisitions]:
        """Builds acquisitions over the declarations of every other row, in build order."""


# --- [CONSTANTS] ------------------------------------------------------------------------

_EAGER: Final = (Tier.GROUPS, Tier.ATTACHMENTS, Tier.RESOURCES)
_EVENTS: Final[Mapping[type, Tier]] = MappingProxyType({Handler: Tier.APP_HANDLERS, Subscription: Tier.SUBSCRIPTIONS, Timer: Tier.TIMERS})
_BANDS: Final[Mapping[type, Tier]] = MappingProxyType({
    bpy.types.PropertyGroup: Tier.GROUPS,
    bpy.types.AddonPreferences: Tier.PREFERENCES,
    bpy.types.KeyConfigPreferences: Tier.PREFERENCES,
    bpy.types.Operator: Tier.OPERATORS,
    bpy.types.Macro: Tier.OPERATORS,
    bpy.types.bpy_struct: Tier.INTERFACE,
})

# --- [MODELS] ---------------------------------------------------------------------------


class Translations(msgspec.Struct, frozen=True, kw_only=True):
    """Interface text translations registered under extension package, in the host form keyed by locale, then by translation context and message."""

    table: Mapping[str, Mapping[tuple[str, str], str]]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds the registry acquisition holding the table under the extension package."""
        table = {locale: dict(entries) for locale, entries in self.table.items()}
        return Ok(((Tier.REGISTRIES, scoped(identity.package, partial(bpy.app.translations.register, identity.package, table), lambda _: bpy.app.translations.unregister(identity.package))),))


class CliCommand[A](msgspec.Struct, frozen=True, kw_only=True):
    """Command-line command with base64 JSON first argument, exiting with the code its run returns, 1 after faults."""

    key: Key
    decoder: msgspec.json.Decoder[A]
    run: Callable[[A, tuple[str, ...]], Outcome[int]]

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds the registry acquisition holding the command under its prefixed name, reports holding its faults, unregistered fault for a refused key."""

        def registered(command: str) -> AbstractContextManager[Outcome[None]]:
            executed = identity.reports.reported(partial(self.executed, command))
            return scoped(command, partial(bpy.utils.register_cli_command, command, lambda arguments: executed(arguments).default_value(1)), bpy.utils.unregister_cli_command)

        return identity.identifier(self.key).map(lambda command: ((Tier.REGISTRIES, registered(command)),))

    def executed(self, command: str, arguments: list[str], /) -> Outcome[int]:
        """Runs over the decoded first argument and the rest unchanged, unreadable fault for an undecodable document, absent fault for no argument."""
        match arguments:
            case [document, *trailing]:
                return argued(self.decoder, command, document).bind(lambda value: self.run(value, tuple(trailing)))
            case _:
                return Error(Fault.of(Reason.ABSENT, command, member="arguments"))


# --- [SERVICES] -------------------------------------------------------------------------


class Registration:
    """Register and unregister callbacks of one extension over its rows, rebuilt on each registration.

    Handler dispatcher builds first, prerequisites precede their rows, resources are held once built, and gathering rows build last.
    Equal declarations build once, each class acquires in the band of its host base, and a manifest website holds Online Manual links over the extension.
    Refused registration raises one `RuntimeError` of rendered faults after every acquisition before it released.
    """

    __slots__ = ("_held", "package", "rows", "unregister")

    def __init__(self, package: str, rows: tuple[Declaration | Gathering | type | _Banded, ...], /) -> None:
        """Holds the package `Identity.read` reads and the rows, with nothing registered, `unregister` releasing every registration in reverse."""
        self.package, self.rows, self._held = package, rows, ExitStack()
        self.unregister: Callable[[], None] = self._held.close

    def register(self) -> None:
        """Acquires every row's registrations, holding their releases until `unregister`, a refusal raising the `RuntimeError` of every rendered fault."""

        def refused(faults: Faults) -> NoReturn:
            raise RuntimeError("\n".join(text for _, text in rendered(faults)))

        def merged() -> None:
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            manager.keyconfigs.update(keep_properties=True)

        def registered(stack: ExitStack, identity: Identity) -> Outcome[None]:
            """Enters keymap merge, reports, and band stacks with the manual map hook of a manifest website, builds rows, and enters every band."""

            def manual(website: str) -> AbstractContextManager[Outcome[None]]:
                hook = partial(_mapped, identity, website)
                return scoped(website, partial(bpy.utils.register_manual_map, hook), lambda _: bpy.utils.unregister_manual_map(hook))

            def opened(band: Tier) -> Outcome[None]:
                fresh = tuple((kind, acquisition) for (kind, acquisition), done in pairs.items() if kind is band and not done)
                return entered(stacks[band], tuple(acquisition for _, acquisition in fresh)).map(lambda _: pairs.update(dict.fromkeys(fresh, True)))

            def declared(declaration: Declaration | Gathering, answer: _Answer) -> Outcome[Acquisitions]:
                declarations.append(declaration)
                return stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer

            def built(row: Declaration | Gathering | type | _Banded) -> Outcome[None]:
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
                    case Resource() as resource:
                        answer = lambda: Ok((
                            (Tier.RESOURCES, identity.resources.holding(resource, partial(resource.made, identity))),
                            *(Handler(handlers=bpy.app.handlers.load_post, body=partial(identity.resources.renewed, resource, renew)) for renew in resource.renewed.to_list()),
                        ))
                    case Handler() | Subscription() | Timer() as event:
                        answer = lambda: Ok(((_EVENTS[type(event)], event.held(identity)),))
                    case Gathering() as gathering:
                        answer = lambda: gathering.gathered(identity, tuple(declarations))
                    case Registrable() as registrable:
                        answer = partial(registrable.acquisitions, identity)
                    case _:
                        pairs.setdefault(row, False)
                        return Ok(None)
                return _each(built, _prerequisites(row)).bind(lambda _: _each(opened, _EAGER)).bind(lambda _: declared(row, answer())).bind(partial(_each, built)).bind(lambda _: _each(opened, _EAGER))

            stack.callback(merged)
            stack.enter_context(identity.reports)
            stacks = {band: stack.enter_context(ExitStack()) for band in Tier}
            declarations: list[Declaration | Gathering] = []
            pairs: dict[tuple[Tier, Acquisition], bool] = dict.fromkeys(((Tier.REGISTRIES, manual(website)) for website in Option.of_optional(identity.manifest.website).to_list()), False)
            prerequisites = chain.from_iterable(map(_prerequisites, self.rows))
            return _each(built, (DISPATCHERS, *prerequisites, *sorted(self.rows, key=lambda row: isinstance(row, Gathering)))).bind(lambda _: _each(opened, Tier))

        with ExitStack() as stack:
            Identity.read(self.package).bind(partial(registered, stack)).map(lambda _: self._held.push(stack.pop_all())).default_with(refused)


# --- [OPERATIONS] -----------------------------------------------------------------------
# --- [ROWS]


def _prerequisites(row: object, /) -> tuple[Declaration, ...]:
    """Declarations a dependent row reads, none for any other row."""
    return row.prerequisites if isinstance(row, Dependent) else ()


# --- [CLASSES]


def entered(stack: ExitStack, acquisitions: tuple[Acquisition, ...], /) -> Outcome[None]:
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
        .map_error(partial(Fault.of, Reason.UNREGISTERED, ", ".join(cls.__name__ for cls in classes)))
        .bind(lambda order: _each(partial(_acquired, stack), (*order, *(acquisition for acquisition in acquisitions if acquisition not in classes))))
    )


def _acquired(stack: ExitStack, acquisition: Acquisition, /) -> Outcome[None]:
    """Registers a class and pushes its release, enters a manager, or registers the classes a deferred build returns."""
    match acquisition:
        case type() as cls:

            def released() -> None:
                """Unregisters class while the host holds it, a class whose property or `register` raised after the host took it included."""
                if "bl_rna" in vars(cls):
                    bpy.utils.unregister_class(cls)  # ty: ignore[invalid-argument-type]

            stack.callback(released)
            try:
                bpy.utils.register_class(cls)  # ty: ignore[invalid-argument-type]
            except (ValueError, RuntimeError, AttributeError) as error:
                return Error(Fault.of(Reason.UNREGISTERED, cls.__name__, error))
            return Ok(None)
        case AbstractContextManager() as manager:
            return stack.enter_context(manager)
        case build:
            return build().bind(partial(entered, stack))


def _each[T](step: Callable[[T], Outcome[None]], items: Iterable[T], /) -> Outcome[None]:
    """Runs step over items in order until the first refusal."""
    return reduce(lambda result, item: result.bind(lambda _: step(item)), items, Result[None, Faults](ok=None))


# --- [REGISTRIES]


@contextmanager
def scoped[H](subject: str, acquired: Callable[[], H], released: Callable[[H], object], /, *, exception: type[Exception] = ValueError) -> Generator[Outcome[None]]:
    """Runs a host acquisition and holds its handle's release until scope exit, yielding the unregistered fault on subject when it raises exception."""
    with ExitStack() as stack:
        yield catch(acquired, exception=exception)().map(lambda handle: stack.callback(released, handle)).map(lambda _: None).map_error(partial(Fault.of, Reason.UNREGISTERED, subject))


def _mapped(identity: Identity, website: str, /) -> tuple[str, list[tuple[str, str]]]:
    """Returns website with extension operator, type, and property patterns, and one per preferences struct and property group a property reaches."""
    category, prefix, group = identity.manifest.id.lower(), identity.manifest.prefix, definition(bpy.types.PropertyGroup)

    def grouped(struct: bpy.types.Struct) -> bool:
        return struct.base == group

    attached = tuple(target for identifier, target in _walked((definition(bpy.types.BlendData),), lambda _: True) if identity.owns(identifier) and grouped(target))
    owned = identity.preferences(bpy.types.AddonPreferences).map(definition).to_list()
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

__all__ = ["Acquisition", "Acquisitions", "CliCommand", "Declaration", "Dependent", "Gathering", "Registrable", "Registration", "Tier", "Translations", "entered", "scoped"]
