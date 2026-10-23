"""Registration bands and extension resource scope."""

from collections.abc import Callable, Iterable
from contextlib import AbstractContextManager, ExitStack
from enum import auto, Enum
from functools import reduce
from itertools import chain
from typing import NoReturn, Protocol, runtime_checkable

import bpy
from expression import Ok, Option, Result

from blender.bpy.extension.classes import Classes
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderFaults, BlenderResult

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


def entered(band_stack: ExitStack, acquisitions: tuple[Acquisition, ...]) -> BlenderResult[None]:
    """Enters classes, then managers, onto stack until first refusal."""
    classes = Classes(members=tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type)))
    managers: Iterable[AbstractContextManager[BlenderResult[None] | None]] = (acquisition for acquisition in acquisitions if not isinstance(acquisition, type))
    succeeded = Result[None, BlenderFaults](ok=None)
    return reduce(lambda result, manager: result.bind(lambda _: Option.of_optional(band_stack.enter_context(manager)).default_value(succeeded)), chain(classes.registrations(), managers), succeeded)


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
        acquired_pairs: list[tuple[Band, Acquisition]] = []

        def walked(bands: Iterable[Band], pairs: tuple[tuple[Band, Acquisition], ...]) -> BlenderResult[None]:
            """Enters bands in order until first refusal."""
            for band in bands:
                fresh = tuple((kind, acquisition) for kind, acquisition in pairs if kind is band and (kind, acquisition) not in acquired_pairs)
                if (answer := entered(stacks[band], tuple(acquisition for _, acquisition in fresh))).is_error():
                    return answer
                acquired_pairs.extend(fresh)
            return Ok(None)

        def built(pairs: tuple[tuple[Band, Acquisition], ...], row: Registrable | Gathering | tuple[Band, Acquisition]) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            """Builds each declaration once after its prerequisites, entering new schema acquisitions."""
            if not isinstance(row, Registrable | Gathering):
                return Ok(pairs if row in pairs else (*pairs, row))
            if row in declarations:
                return Ok(pairs)

            def answered(held: tuple[tuple[Band, Acquisition], ...]) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
                answer = row.gathered(identity, tuple(declarations)) if isinstance(row, Gathering) else row.acquisitions(identity)
                declarations.append(row)
                acquired = stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer
                return acquired.bind(lambda members: folded(held, members)).bind(lambda members: walked(schema, members).map(lambda _: members))

            return folded(pairs, row.prerequisites if isinstance(row, Dependent) else ()).bind(lambda held: walked(schema, held).map(lambda _: held)).bind(answered)

        def folded(pairs: tuple[tuple[Band, Acquisition], ...], rows: Iterable[Registrable | Gathering | tuple[Band, Acquisition]]) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            """Builds rows in order from acquired pairs."""
            return reduce(lambda result, row: result.bind(lambda held: built(held, row)), rows, Result[tuple[tuple[Band, Acquisition], ...], BlenderFaults](ok=pairs))

        ordered = (*(needed for row in rows if isinstance(row, Dependent) for needed in row.prerequisites), *sorted(rows, key=lambda row: isinstance(row, Gathering)))
        return folded((), ordered).bind(lambda pairs: walked((band for band in Band if band not in schema), pairs))

    def refused(faults: BlenderFaults) -> NoReturn:
        """Raises rendered faults as RuntimeError."""
        raise RuntimeError("\n".join(text for _, text in rendered(faults)))

    def register() -> None:
        """Acquires rows, holding releases once every row succeeds."""
        with ExitStack() as stack:
            Identity.read(package).bind(lambda identity: acquired(stack, identity)).map(lambda _: held.push(stack.pop_all())).default_with(refused)

    return register, held.close


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquisition", "Acquisitions", "Band", "Dependent", "Gathering", "Registrable", "entered", "scope"]
