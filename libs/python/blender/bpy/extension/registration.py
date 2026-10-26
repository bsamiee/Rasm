"""Registration bands, class registration in dependency order, and extension resource scope."""

from collections.abc import Callable, Generator, Iterable
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import auto, Enum
from functools import reduce
import graphlib
from typing import get_type_hints, NoReturn, Protocol, runtime_checkable

import bpy
from expression import Error, Ok, Option, Result

from blender.bpy.extension.identity import Identity
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderFaults, BlenderRegisterFault, BlenderResult

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


type Acquisition = type | AbstractContextManager[BlenderResult[None] | None]
type Acquisitions = tuple[Registrable | tuple[Band, Acquisition], ...]


@runtime_checkable
class Registrable(Protocol):
    """Row building its acquisitions under extension identity."""

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions] | AbstractContextManager[BlenderResult[Acquisitions]]:
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
    def prerequisites(self) -> tuple[Registrable, ...]:
        """Declarations acquired before dependent rows build."""


@runtime_checkable
class Gathering(Protocol):
    """Row building its acquisitions over every declaration the scope built before it."""

    def gathered(self, identity: Identity, declared: "tuple[Registrable | Gathering, ...]", /) -> BlenderResult[Acquisitions]:
        """Builds acquisitions over declarations of every other row.

        Args:
            identity: Extension with names, folder, and reports row reads.
            declared: Declarations in build order.

        Returns:
            Build result.
        """


# --- [OPERATIONS] -----------------------------------------------------------------------


def ordered(classes: tuple[type, ...]) -> BlenderResult[tuple[type, ...]]:
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
    try:
        return Ok(tuple(graphlib.TopologicalSorter({cls: held & targets.keys() for cls, held in targets.items()}).static_order()))
    except graphlib.CycleError as error:
        return Error((BlenderRegisterFault(request=tuple(cls.__name__ for cls in classes), error=error),))


@contextmanager
def registered(cls: type) -> Generator[BlenderResult[None]]:
    """Registers class, unregistering it at exit whenever the host holds it, a refused registration included.

    Args:
        cls: Class `bpy.utils.register_class` takes.

    Yields:
        Ok(None), register fault for a refused class.
    """
    register, unregister = bpy.utils.register_classes_factory((cls,))
    try:
        register()
    except (ValueError, RuntimeError) as error:
        answer = Result[None, BlenderFaults](error=(BlenderRegisterFault(request=(cls.__name__,), error=error),))
    else:
        answer = Result[None, BlenderFaults](ok=None)
    try:
        yield answer
    finally:
        if "bl_rna" in vars(cls):
            unregister()


def entered(band_stack: ExitStack, acquisitions: tuple[Acquisition, ...]) -> BlenderResult[None]:
    """Enters classes in dependency order, then managers, onto stack until first refusal."""
    managers = tuple(acquisition for acquisition in acquisitions if not isinstance(acquisition, type))
    return ordered(tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type))).bind(
        lambda order: reduce(
            lambda result, manager: result.bind(lambda _: Option.of_optional(band_stack.enter_context(manager)).default_value(Ok(None))),
            (*map(registered, order), *managers),
            Result[None, BlenderFaults](ok=None),
        )
    )


# --- [COMPOSITION] ----------------------------------------------------------------------


def scope(package: str, rows: tuple[Registrable | Gathering, ...]) -> tuple[Callable[[], None], Callable[[], None]]:
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

    def merged() -> None:
        """Merges user keymap edits after release, keeping properties for next enable."""
        for manager in Option.of_optional(bpy.context.window_manager).to_list():
            manager.keyconfigs.update(keep_properties=True)

    def acquired(stack: ExitStack, identity: Identity) -> BlenderResult[None]:
        """Builds rows, schema acquisitions entered before dependents."""
        stack.callback(merged)
        stack.enter_context(identity.reports)
        stacks = {band: stack.enter_context(ExitStack()) for band in Band}
        schema = (Band.GROUPS, Band.ATTACHMENTS)
        declarations: list[Registrable | Gathering] = []
        pairs: dict[tuple[Band, Acquisition], bool] = {}

        def walked(bands: Iterable[Band]) -> BlenderResult[None]:
            """Enters each band's pairs not yet entered, in band order until first refusal."""

            def band_entered(result: BlenderResult[None], band: Band) -> BlenderResult[None]:
                fresh = tuple(pair for pair, done in pairs.items() if pair[0] is band and not done)
                return result.bind(lambda _: entered(stacks[band], tuple(acquisition for _, acquisition in fresh))).map(lambda _: pairs.update(dict.fromkeys(fresh, True)))

            return reduce(band_entered, bands, Result[None, BlenderFaults](ok=None))

        def built(row: Registrable | Gathering | tuple[Band, Acquisition]) -> BlenderResult[None]:
            """Builds each declaration once after its prerequisites, entering new schema acquisitions."""
            if not isinstance(row, Registrable | Gathering):
                pairs.setdefault(row, False)
                return Ok(None)
            if row in declarations:
                return Ok(None)

            def answered(_: None) -> BlenderResult[None]:
                answer = row.gathered(identity, tuple(declarations)) if isinstance(row, Gathering) else row.acquisitions(identity)
                declarations.append(row)
                acquisitions = stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer
                return acquisitions.bind(folded).bind(lambda _: walked(schema))

            return folded(row.prerequisites if isinstance(row, Dependent) else ()).bind(lambda _: walked(schema)).bind(answered)

        def folded(rows: Iterable[Registrable | Gathering | tuple[Band, Acquisition]]) -> BlenderResult[None]:
            """Builds rows in order."""
            return reduce(lambda result, row: result.bind(lambda _: built(row)), rows, Result[None, BlenderFaults](ok=None))

        return folded((*(needed for row in rows if isinstance(row, Dependent) for needed in row.prerequisites), *sorted(rows, key=lambda row: isinstance(row, Gathering)))).bind(
            lambda _: walked(band for band in Band if band not in schema)
        )

    def refused(faults: BlenderFaults) -> NoReturn:
        """Raises rendered faults as RuntimeError."""
        raise RuntimeError("\n".join(text for _, text in rendered(faults)))

    def register() -> None:
        """Acquires rows, holding releases once every row succeeds."""
        with ExitStack() as stack:
            Identity.read(package).bind(lambda identity: acquired(stack, identity)).map(lambda _: held.push(stack.pop_all())).default_with(refused)

    return register, held.close


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquisition", "Acquisitions", "Band", "Dependent", "Gathering", "Registrable", "entered", "ordered", "registered", "scope"]
