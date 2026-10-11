"""Registration bands, row acquisitions, and an extension's resource scope."""

from collections.abc import Callable, Generator, Iterable
from contextlib import AbstractContextManager, contextmanager, ExitStack
from enum import auto, Enum
from functools import reduce
from itertools import chain
from typing import NoReturn, Protocol, runtime_checkable

import bpy
from expression import Option, Result

from rasm.blender.extension.classes import Classes
from rasm.blender.extension.identity import Identity
from rasm.blender.extension.reports import rendered
from rasm.blender.faults import BlenderFaults, BlenderResult

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
    PREFERENCE_ROWS = auto()


type Acquisition = type | AbstractContextManager[BlenderResult[None] | None]
type Acquisitions = tuple[Registrable | tuple[Band, Acquisition], ...]


@runtime_checkable
class Registrable(Protocol):
    """Row building its native acquisitions under an extension identity."""

    def acquisitions(self, identity: Identity, /) -> BlenderResult[Acquisitions] | AbstractContextManager[BlenderResult[Acquisitions]]:
        """Builds native acquisitions with an optional scoped construction lifetime.

        Args:
            identity: Extension whose names, folder, and reports the row consumes.

        Returns:
            A typed build result, directly or held by its construction context.
        """


# --- [OPERATIONS] -----------------------------------------------------------------------


def entered(band_stack: ExitStack, acquisitions: tuple[Acquisition, ...]) -> BlenderResult[None]:
    """Enters distinct classes before managers, retaining every release through the first refusal."""
    classes = Classes(members=tuple(acquisition for acquisition in acquisitions if isinstance(acquisition, type)))
    managers: Iterable[AbstractContextManager[BlenderResult[None] | None]] = (acquisition for acquisition in acquisitions if not isinstance(acquisition, type))
    succeeded = Result[None, BlenderFaults](ok=None)
    return reduce(lambda result, manager: result.bind(lambda _: Option.of_optional(band_stack.enter_context(manager)).default_value(succeeded)), chain(classes.registrations(), managers), succeeded)


# --- [COMPOSITION] ----------------------------------------------------------------------


def scope(package: str, rows: tuple[Registrable, ...]) -> tuple[Callable[[], None], Callable[[], None]]:
    """Builds registration and release callbacks over one extension stack.

    Args:
        package: Extension package whose manifest Identity.read consumes.
        rows: Rows in prerequisite order, rebuilt on each registration.

    Returns:
        Register and unregister callbacks; registration faults raise RuntimeError while the stack unwinds.
    """
    held = ExitStack()

    @contextmanager
    def released(preference_rows: ExitStack) -> Generator[None]:
        """Releases preference rows before Blender's quit preference save."""

        def closed(_: object) -> None:
            """Closes the preference-row band at the host exit boundary."""
            preference_rows.close()

        bpy.app.handlers.exit_pre.append(bpy.app.handlers.persistent(closed))
        try:
            yield
        finally:
            bpy.app.handlers.exit_pre.remove(closed)

    def acquired(stack: ExitStack, identity: Identity) -> BlenderResult[None]:
        """Builds dependent rows after immediately entering their schema acquisitions."""
        stack.enter_context(identity.reports)
        stacks = {band: stack.enter_context(ExitStack()) for band in Band}
        stack.enter_context(released(stacks[Band.PREFERENCE_ROWS]))
        schema = (Band.GROUPS, Band.ATTACHMENTS)
        declarations: list[Registrable] = []
        acquired_pairs: list[tuple[Band, Acquisition]] = []

        def walked(bands: Iterable[Band], pairs: tuple[tuple[Band, Acquisition], ...]) -> BlenderResult[None]:
            """Enters bands in order through the first acquisition refusal."""
            for band in bands:
                fresh = tuple((kind, acquisition) for kind, acquisition in pairs if kind is band and (kind, acquisition) not in acquired_pairs)
                if (answer := entered(stacks[band], tuple(acquisition for _, acquisition in fresh))).is_error():
                    return answer
                acquired_pairs.extend(fresh)
            return Result(ok=None)

        def built(pairs: tuple[tuple[Band, Acquisition], ...], row: Registrable | tuple[Band, Acquisition]) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
            """Constructs each declaration once and enters each new schema at its first use."""
            if not isinstance(row, Registrable):
                return Result(ok=pairs if row in pairs else (*pairs, row))
            if any(row is declaration for declaration in declarations):
                return Result(ok=pairs)
            if (prerequisite := walked(schema, pairs)).is_error():
                return Result(error=prerequisite.error)
            declarations.append(row)
            answer = row.acquisitions(identity)
            acquired = stack.enter_context(answer) if isinstance(answer, AbstractContextManager) else answer
            return acquired.bind(
                lambda members: reduce(lambda result, member: result.bind(lambda held: built(held, member)), members, Result[tuple[tuple[Band, Acquisition], ...], BlenderFaults](ok=pairs))
            ).bind(lambda held: walked(schema, held).map(lambda _: held))

        answered = reduce(lambda result, row: result.bind(lambda pairs: built(pairs, row)), rows, Result[tuple[tuple[Band, Acquisition], ...], BlenderFaults](ok=()))
        return answered.bind(lambda pairs: walked((band for band in Band if band not in schema), pairs))

    def refused(faults: BlenderFaults) -> NoReturn:
        """Renders typed failures at Blender's registration exception boundary."""
        raise RuntimeError("\n".join(text for _, text in rendered(faults)))

    def register() -> None:
        """Acquires rows and transfers their releases only after complete success."""
        with ExitStack() as stack:
            Identity.read(package).bind(lambda identity: acquired(stack, identity)).map(lambda _: held.push(stack.pop_all())).default_with(refused)

    return register, held.close


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Acquisition", "Acquisitions", "Band", "Registrable", "entered", "scope"]
