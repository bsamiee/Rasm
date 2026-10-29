"""Operator and macro classes over a command body, the invoke form protocol, and body answers."""

from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager
import inspect
from itertools import chain
from pathlib import Path
from types import FunctionType, MethodType
from typing import Final, override, Protocol, Self, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Command, filled, Placement
from blender.bpy.data.rna.descriptors import Value
from blender.bpy.data.rna.mirrors import annotated, nested
from blender.bpy.data.rna.snapshots import Applied, assigned
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import rendered
from blender.records.faults import accumulated, Faults, Outcome

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Returns = set[rna_enums.OperatorReturnItems]
type Draw = Callable[[bpy.types.UILayout, bpy.types.Context, bpy.types.OperatorProperties, Identity], None]
type Answer = Done | Unchanged
type Notes = tuple[tuple[rna_enums.WmReportItems, str], ...]


class Invoke[T: bpy.types.PropertyGroup, S](Protocol):
    """Invoke form deriving the registered operator class from the class its command builds."""

    @property
    def asks(self) -> bool:
        """Whether the form asks for input before the body runs, its command's label ending in an ellipsis."""
        ...

    def classed(self, body: "Body[T, S]", base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Builds the operator class the form invokes, with the release scopes it holds.

        Args:
            body: Callbacks over the command and extension identity.
            base: Class carrying the command's names, flags, poll, form properties, and execute.

        Returns:
            Classes and banded scopes, the operator class first, register fault for a host call the form cannot make.
        """
        ...


# --- [MODELS] ---------------------------------------------------------------------------


class Empty(bpy.types.PropertyGroup):
    """Form of operator without properties."""


class Done(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with changes, report entries, and skipped faults."""

    notes: Notes = ()
    skipped: Option[Faults] = Nothing

    @classmethod
    def wrote(cls, paths: Iterable[str | Path], /) -> Self:
        """Reports each written path."""
        return cls(notes=tuple(("INFO", bpy.app.translations.pgettext_rpt("Wrote {}").format(path)) for path in paths))


class Unchanged(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with no change, reason reported as INFO."""

    reason: str


class Call[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Body invocation with context, operator properties under the form group type, and the paths its invoke form chose."""

    identity: Identity
    context: bpy.types.Context
    properties: T
    paths: tuple[Path, ...] = ()


class Operator[T: bpy.types.PropertyGroup, S = None](Command, frozen=True, kw_only=True):
    """Form group and body with invoke form and undo, its key the body function name unless stated and its description the first paragraph of the body docstring.

    `undo` holds Nothing for no undo step, `Some(Nothing)` for one step per run, and `Some(Some(group))` for one step across consecutive runs of the group.
    `describe` computes the tooltip from context and properties in place of the docstring.
    """

    form: type[T]
    run: Callable[[Call[T]], Outcome[Answer]]
    invoke: Option[Invoke[T, S]] = Nothing
    seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]] = Nothing
    undo: Option[Option[str]] = Some(Nothing)
    draw: Option[Draw] = Nothing
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]] = Nothing
    describe: Option[Callable[[bpy.types.Context, T], str]] = Nothing
    presets: bool = False
    primary: Option[str] = Nothing
    named: Option[Key] = Nothing

    def __post_init__(self) -> None:
        """Refuses a body holding no function name or docstring."""
        function = inspect.unwrap(self.run)
        if not (isinstance(function, FunctionType | MethodType) and inspect.getdoc(function) is not None):
            raise TypeError(f"{function!r} has no function name or docstring")

    # --- [TEXT]

    @property
    @override
    def key(self) -> Key:
        """Stated key, else the body function name."""
        return self.named.default_with(lambda: self.run.__name__)  # ty: ignore[unresolved-attribute]

    @property
    @override
    def description(self) -> str:
        """First paragraph of the body docstring."""
        return " ".join(str(inspect.getdoc(inspect.unwrap(self.run))).split("\n\n")[0].split()).removesuffix(".")

    @property
    @override
    def asks(self) -> bool:
        """Whether the invoke form asks for input before the body runs."""
        return any(form.asks for form in self.invoke.to_list())

    # --- [BINDING]

    @property
    def invoked(self) -> bool:
        """Whether placement invokes to open form or seed properties."""
        return self.invoke.is_some() or self.seed.is_some()

    @property
    def flags(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """`REGISTER` with `PRESET` and the undo flag the `undo` declaration names."""
        undo: tuple[rna_enums.OperatorTypeFlagItems, ...] = tuple(self.undo.map(lambda group: "UNDO_GROUPED" if group.is_some() else "UNDO").to_list())
        return frozenset(("REGISTER", *(("PRESET",) if self.presets else ()), *undo))

    @property
    def property(self) -> Option[str]:
        """Primary property a search popup edits and an enum button's tooltip describes, the form's one enum while none is stated."""
        enums = tuple(name for name, definition in annotated(self.form).items() if definition.function is bpy.props.EnumProperty)
        return self.primary.or_else(Some(enums[0]) if len(enums) == 1 else Nothing)

    @override
    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds the form's property groups, then the operator class its invoke form derives with that form's release scopes."""
        body = Body(command=self, identity=identity)

        def classed(name: str) -> Outcome[Acquisitions]:
            base: type[bpy.types.Operator] = type(
                name,
                (bpy.types.Operator,),
                {
                    "__annotations__": annotated(self.form),
                    "execute": body.execute,
                    **self.namespace(name, self.flags),
                    **self.property.map(lambda member: {"bl_property": member}).default_value({}),
                    **self.undo.bind(lambda group: group).map(lambda group: {"bl_undo_group": group}).default_value({}),
                    **self.draw.map(lambda _: {"draw": body.draw}).default_value({}),
                    **self.check.map(lambda callback: {"check": callback}).default_value({}),
                    **self.describe.map(lambda describe: {"description": classmethod(lambda _cls, context, properties: describe(context, properties))}).default_value({}),
                    **self.seed.map(lambda _: {"invoke": body.invoking(lambda operator, context, _event: Ok(body.execute(operator, context)))}).default_value({}),
                },
            )
            return self.invoke.map(lambda form: form.classed(body, base)).default_value(Ok((base,))).map(lambda classes: (*nested(self.form), *classes))

        return identity.name(bpy.types.Operator, self.key).bind(classed)


class Body[T: bpy.types.PropertyGroup, S](msgspec.Struct, frozen=True, kw_only=True):
    """Callbacks one operator class composes over its command."""

    command: Operator[T, S]
    identity: Identity

    # --- [HOST]

    @staticmethod
    def properties(operator: bpy.types.Operator, /) -> T:
        """Returns the operator properties as the form group whose properties its class copies, which no host type states."""
        properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return properties

    @staticmethod
    def layout(owner: bpy.types.Operator | bpy.types.UIPopupMenu | bpy.types.UIPieMenu | bpy.types.UIPopover | bpy.types.Header, /) -> bpy.types.UILayout:
        """Returns the layout an operator, popup, pie, popover, or header draws into, which the stubs type as optional."""
        layout: bpy.types.UILayout = owner.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return layout

    # --- [REPORTS]

    @staticmethod
    def noted(operator: bpy.types.Operator, entries: Iterable[tuple["rna_enums.WmReportItems", str]], outcome: "rna_enums.OperatorReturnItems", /) -> Returns:
        """Reports entries, answering outcome."""
        for severity, text in entries:
            operator.report({severity}, text)
        return {outcome}

    @staticmethod
    def finished(operator: bpy.types.Operator, result: Outcome[Answer], /) -> Returns:
        """Reports body result as notes and skipped faults with FINISHED, a reason with CANCELLED, or rendered faults with CANCELLED."""

        def concluded(outcome: Answer) -> Returns:
            match outcome:
                case Done(notes=notes, skipped=skipped):
                    return Body.noted(operator, (*notes, *(("WARNING", text) for faults in skipped.to_list() for _, text in rendered(faults))), "FINISHED")
                case Unchanged(reason=reason):
                    return Body.noted(operator, (("INFO", reason),), "CANCELLED")

        return result.map(concluded).default_with(lambda faults: Body.noted(operator, rendered(faults), "CANCELLED"))

    # --- [RUNS]

    def performed(self, operator: bpy.types.Operator, context: bpy.types.Context, paths: tuple[Path, ...] = (), /) -> Outcome[Answer]:
        """Runs the command over the operator properties and the paths its invoke form chose."""
        return self.command.run(Call(identity=self.identity, context=context, properties=self.properties(operator), paths=paths))

    @property
    def execute(self) -> Callable[[bpy.types.Operator, bpy.types.Context], Returns]:
        """Class `execute` running the command and reporting its result."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return self.finished(operator, self.performed(operator, context))

        return execute

    def settled(self, operator: bpy.types.Operator, context: bpy.types.Context, release: Callable[[bool], Outcome[None]], /) -> Returns:
        """Runs the command after a modal run, releasing the run kept on Done and rolled back otherwise."""

        def completed(ran: Answer) -> Returns:
            match ran:
                case Done(skipped=skipped) as done:
                    return self.finished(
                        operator, Ok(release(False).map(lambda _: done).default_with(lambda faults: msgspec.structs.replace(done, skipped=Some((*chain.from_iterable(skipped.to_list()), *faults)))))
                    )
                case Unchanged():
                    return self.finished(operator, release(True).map(lambda _: ran))

        return (
            self
            .performed(operator, context)
            .map(completed)
            .default_with(lambda faults: self.noted(operator, (*rendered(faults), *release(True).map(lambda _: ()).default_with(rendered)), "CANCELLED"))
        )

    # --- [DRAW]

    @property
    def draw(self) -> Callable[[bpy.types.Operator, bpy.types.Context], None]:
        """Class `draw` laying out the command's draw over the operator properties."""

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            for drawn in self.command.draw.to_list():
                drawn(self.layout(operator), context, operator.properties, self.identity)

        return draw

    # --- [INVOKE]

    def invoking(self, opening: Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.Event], Outcome[Returns]], /) -> Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.Event], Returns]:
        """Returns invoke writing seed values into unset properties, then opening the form."""

        def invoke(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
            return (
                self.command.seed
                .map(
                    lambda seed: assigned(
                        operator.properties, {(name,): value for name, value in seed(self.properties(operator), event, self.identity).items() if not operator.properties.is_property_set(name)}
                    )
                )
                .default_value(Ok(None))
                .bind(lambda _: opening(operator, context, event))
                .default_with(lambda faults: self.noted(operator, rendered(faults), "CANCELLED"))
            )

        return invoke


class Macro(Command, frozen=True, kw_only=True):
    """Operator steps run in one undo step and redo entry, stating the key and description no body function holds."""

    key: Key = msgspec.field()
    description: str = msgspec.field()
    steps: tuple[Placement, *tuple[Placement, ...]]

    @override
    def acquisitions(self, identity: Identity, /) -> Outcome[Acquisitions]:
        """Builds macro class, defining its steps after class registration."""

        def built(name: str) -> Acquisitions:
            kind: type[bpy.types.Macro] = type(name, (bpy.types.Macro,), self.namespace(name, frozenset({"REGISTER", "UNDO"})))

            @contextmanager
            def defined() -> Generator[Outcome[None]]:
                yield accumulated(step.resolved(identity).bind(lambda placed: filled(placed, kind.define(placed[0]).properties)) for step in self.steps).map(lambda _: None)

            return kind, (Band.OPERATORS, defined())

        return identity.name(bpy.types.Operator, self.key).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def answered(applied: Applied, reason: str, notes: Notes = (), /) -> Outcome[Answer]:
    """Returns Done skipping write faults when a path was written, else write faults, else Unchanged."""
    match applied.written, applied.outcome.swap().to_option():
        case (_, *_), skipped:
            return Ok(Done(notes=notes, skipped=skipped))
        case (), Option(tag="some", some=faults):
            return Error(faults)
        case _:
            return Ok(Unchanged(reason=reason))


def tallied(edits: tuple[Outcome[object], ...], notes: Notes = (), /) -> Outcome[Done]:
    """Returns Done skipping faults when any item changed, else every fault."""
    return accumulated(edits).map(lambda _: Done(notes=notes)).or_else_with(lambda faults: Error(faults) if all(edit.is_error() for edit in edits) else Ok(Done(notes=notes, skipped=Some(faults))))


def dismiss_reports(call: Call[Empty]) -> Outcome[Answer]:
    """Clears the extension's held callback reports."""
    return Ok(Done() if call.identity.reports.cleared() else Unchanged(reason="No reports are held"))


# --- [COMPOSITION] ----------------------------------------------------------------------

DISMISSAL: Final = Operator[Empty](form=Empty, run=dismiss_reports, icon=Some("X"), internal=True, undo=Nothing)

# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["DISMISSAL", "Answer", "Body", "Call", "Done", "Draw", "Empty", "Invoke", "Macro", "Notes", "Operator", "Returns", "Unchanged", "answered", "dismiss_reports", "tallied"]
