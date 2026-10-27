"""Registration bands, class registration in dependency order, and extension resource scope."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import AbstractContextManager, ExitStack
from enum import auto, Enum
from functools import partial, reduce
import graphlib
from types import MappingProxyType
from typing import Final, get_type_hints, NoReturn, Protocol, runtime_checkable

import bpy
from expression import Ok, Option, Result
from expression.extra.result import catch

from blender.bpy.extension.events import Handler, Subscription, Timer
from blender.bpy.extension.identity import Identity, Resource
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderReason
from blender.records.faults import Fault, Faults, Outcome

# --- [TYPES] ----------------------------------------------------------------------------


class Band(Enum):
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


type Acquisition = type | AbstractContextManager[Outcome[None] | None]
type Declaration = Registrable | Resource[object] | Handler | Subscription | Timer
type Acquisitions = tuple[Declaration | tuple[Band, Acquisition], ...]


@runtime_checkable
class Registrable(Protocol):
    """Row building its acquisitions under extension identity."""

    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions] | AbstractContextManager[Outcome[Acquisitions]]:
        """Builds acquisitions, optionally inside a construction scope.

        Args:
            identity: Extension with names, folder, and reports row reads.

        Returns:
            Build result, direct or yielded by its construction context manager.
        """


@runtime_checkable
class Dependent(Protocol):
    """Row whose build reads declarations the scope acquires before every row."""

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations acquired before dependent rows build."""


@runtime_checkable
class Gathering(Protocol):
    """Row building its acquisitions over every declaration the scope built before it."""

    def gathered(self, identity: Identity, declared: "tuple[Declaration | Gathering, ...]", /) -> Outcome[Acquisitions]:
        """Builds acquisitions over declarations of every other row.

        Args:
            identity: Extension with names, folder, and reports row reads.
            declared: Declarations in build order.

        Returns:
            Build result.
        """


# --- [CONSTANTS] ------------------------------------------------------------------------

_SCHEMA: Final = (Band.GROUPS, Band.ATTACHMENTS)
_EVENTS: Final[Mapping[type[Handler | Subscription | Timer], Band]] = MappingProxyType({Handler: Band.APP_HANDLERS, Subscription: Band.SUBSCRIPTIONS, Timer: Band.TIMERS})
_REGISTER: Final[Callable[[type], Result[None, RuntimeError | ValueError]]] = catch(exception=RuntimeError)(catch(bpy.utils.register_class, exception=ValueError))  # ty: ignore[invalid-assignment]

# --- [SERVICES] -------------------------------------------------------------------------


class _Scope:
    """One registration's build state: the stack holding every release, one stack per band, built declarations, and acquisition pairs with whether each was entered."""

    __slots__ = ("declarations", "identity", "pairs", "stack", "stacks")

    def __init__(self, identity: Identity, stack: ExitStack) -> None:
        """Enters the keymap merge, reports, and band stacks onto stack, holding no declaration."""

        def merged() -> None:
            """Merges user keymap edits after release, keeping properties for next enable."""
            manager: bpy.types.WindowManager = bpy.context.window_manager  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            manager.keyconfigs.update(keep_properties=True)

        self.identity, self.stack = identity, stack
        stack.callback(merged)
        stack.enter_context(identity.reports)
        self.stacks = {band: stack.enter_context(ExitStack()) for band in Band}
        self.declarations: list[Declaration | Gathering] = []
        self.pairs: dict[tuple[Band, Acquisition], bool] = {}

    def acquired(self, rows: tuple[Declaration | Gathering, ...]) -> Outcome[None]:
        """Builds every row's prerequisites, then the rows with gathering rows last, then enters the bands after the schema."""
        return _each(self.built, (*(needed for row in rows if isinstance(row, Dependent) for needed in row.prerequisites), *sorted(rows, key=lambda row: isinstance(row, Gathering)))).bind(
            lambda _: _each(self.opened, (band for band in Band if band not in _SCHEMA))
        )

    def built(self, row: Declaration | Gathering | tuple[Band, Acquisition]) -> Outcome[None]:
        """Builds each declaration once after its prerequisites, entering new schema acquisitions after each build."""
        match row:
            case _ if row in self.declarations:
                return Ok(None)
            case Resource() | Handler() | Subscription() | Timer() | Gathering() | Registrable():
                return (
                    _each(self.built, row.prerequisites if isinstance(row, Dependent) else ())
                    .bind(lambda _: _each(self.opened, _SCHEMA))
                    .bind(lambda _: self.declared(row))
                    .bind(partial(_each, self.built))
                    .bind(lambda _: _each(self.opened, _SCHEMA))
                )
            case _:
                self.pairs.setdefault(row, False)
                return Ok(None)

    def declared(self, row: Declaration | Gathering) -> Outcome[Acquisitions]:
        """Builds the acquisitions of one declaration, holding a construction scope on the registration stack."""
        answer: Outcome[Acquisitions] | AbstractContextManager[Outcome[Acquisitions]]
        match row:
            case Resource():
                answer = Ok((
                    (Band.RESOURCES, self.identity.held.holding(row, partial(row.made, self.identity))),
                    *(Handler(handlers=bpy.app.handlers.load_post, body=partial(self.identity.held.renewed, row, renew)) for renew in row.renewed.to_list()),
                ))
            case Handler() | Subscription() | Timer():
                answer = Ok(((_EVENTS[type(row)], row.held(self.identity)),))
            case Gathering():
                answer = row.gathered(self.identity, tuple(self.declarations))
            case _:
                answer = row.acquisitions(self.identity)
        self.declarations.append(row)
        return self.stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer

    def opened(self, band: Band) -> Outcome[None]:
        """Enters the band's pairs not yet entered, marking them entered."""
        fresh = tuple((kind, acquisition) for (kind, acquisition), done in self.pairs.items() if kind is band and not done)
        return entered(self.stacks[band], tuple(acquisition for _, acquisition in fresh)).map(lambda _: self.pairs.update(dict.fromkeys(fresh, True)))


# --- [OPERATIONS] -----------------------------------------------------------------------


def ordered(classes: tuple[type, ...]) -> Outcome[tuple[type, ...]]:
    """Orders classes after their pointer and collection property targets and parent panels.

    Args:
        classes: Distinct classes of one acquisition batch.

    Returns:
        Ok(classes in dependency order), register fault naming the batch for a cycle.
    """
    panels = {panel.bl_idname: panel for panel in classes if issubclass(panel, bpy.types.Panel)}
    targets = {
        cls: {
            *(
                prop.keywords["type"]
                for prop in get_type_hints(cls).values()
                if isinstance(prop, bpy.props._PropertyDeferred) and prop.function in {bpy.props.PointerProperty, bpy.props.CollectionProperty}
            ),
            *Option.of_optional(vars(cls).get("bl_parent_id")).bind(lambda parent: Option.of_optional(panels.get(parent))).to_list(),
        }
        for cls in classes
    }
    return catch(tuple, exception=graphlib.CycleError)(graphlib.TopologicalSorter({cls: held & targets.keys() for cls, held in targets.items()}).static_order()).map_error(
        partial(Fault.of, BlenderReason.UNREGISTERED, ", ".join(cls.__name__ for cls in classes))
    )


def registered(cls: type) -> Outcome[None]:
    """Registers class with the host.

    Args:
        cls: Class `bpy.utils.register_class` takes.

    Returns:
        Ok(None), register fault for a refused class.
    """
    return _REGISTER(cls).map_error(partial(Fault.of, BlenderReason.UNREGISTERED, cls.__name__))


def entered(stack: ExitStack, acquisitions: tuple[Acquisition, ...]) -> Outcome[None]:
    """Registers classes in dependency order, then enters managers, onto stack until first refusal, each class released in reverse whenever the host holds it.

    Args:
        stack: Stack holding every release.
        acquisitions: Classes and context managers of one band.

    Returns:
        Ok(None), the first refusal's faults.
    """
    managers = tuple(acquisition for acquisition in acquisitions if not isinstance(acquisition, type))
    return ordered(tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type))).bind(lambda order: _each(partial(_acquired, stack), (*order, *managers)))


def _acquired(stack: ExitStack, acquisition: Acquisition) -> Outcome[None]:
    """Registers a class after pushing its release, or enters a manager."""
    match acquisition:
        case type() as cls:

            def released() -> None:
                """Unregisters class while the host holds it, a class whose `register` raised included."""
                if "bl_rna" in vars(cls):
                    bpy.utils.unregister_class(cls)  # ty: ignore[invalid-argument-type]

            stack.callback(released)
            return registered(cls)
        case _:
            return Option.of_optional(stack.enter_context(acquisition)).default_value(Ok(None))


def _each[T](step: Callable[[T], Outcome[None]], items: Iterable[T]) -> Outcome[None]:
    """Runs step over items in order until the first refusal."""
    return reduce(lambda result, item: result.bind(lambda _: step(item)), items, Result[None, Faults](ok=None))


# --- [COMPOSITION] ----------------------------------------------------------------------


def scope(package: str, rows: tuple[Declaration | Gathering, ...]) -> tuple[Callable[[], None], Callable[[], None]]:
    """Builds register and unregister callbacks.

    Prerequisites of every row precede the rows, a nested row's prerequisites precede it, and gathering rows build last.
    Equal declarations build once.

    Args:
        package: Extension package with manifest `Identity.read` reads.
        rows: Rows in build order, rebuilt on each registration.

    Returns:
        Callbacks, register raising RuntimeError of rendered faults after unwinding.
    """
    held = ExitStack()

    def refused(faults: Faults) -> NoReturn:
        raise RuntimeError("\n".join(text for _, text in rendered(faults)))

    def register() -> None:
        with ExitStack() as stack:
            Identity.read(package).bind(lambda identity: _Scope(identity, stack).acquired(rows)).map(lambda _: held.push(stack.pop_all())).default_with(refused)

    return register, held.close


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquisition", "Acquisitions", "Band", "Declaration", "Dependent", "Gathering", "Registrable", "entered", "ordered", "registered", "scope"]
