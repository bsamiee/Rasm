"""Extension commands, their operator and macro classes over a body, placements in the host keymap-data form, operator calls, and the popups window manager operators open."""

from abc import abstractmethod
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, nullcontext
from enum import Enum
from functools import partial
import inspect
from pathlib import Path
from types import FunctionType, MappingProxyType, MethodType
from typing import ClassVar, override, Protocol, Self, TYPE_CHECKING, TypedDict, Unpack

import bpy
from expression import Option, Result, Some, try_downcast
import msgspec

from rasm_bpy.data._extension.identity import Identity, Key
from rasm_bpy.data._extension.registration import acquired_rows, Acquisitions, Declaration, Tier
from rasm_bpy.data._extension.reports import Refusal, rendered
from rasm_bpy.data._rna.conditions import Condition, polled
from rasm_bpy.data._rna.descriptors import Scalar, Value
from rasm_bpy.data._rna.paths import rna, rna_definition, RnaPath, Struct
from rasm_bpy.data._rna.properties import annotated, Callbacks, nested, rebound
from rasm_bpy.data._rna.snapshots import Applied, assigned
from rasm_records import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason

if TYPE_CHECKING:
    import _bpy_types
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Operation = Callable[[rna_enums.OperatorContextItems, bool], set[rna_enums.OperatorReturnItems]]
type Setting = Scalar | tuple[Scalar, ...]
type Pairs = tuple[tuple[str, Setting], ...]
type Draw[T] = Callable[[bpy.types.UILayout, bpy.types.Context, T, Identity], None]
type Answer = Done | Unchanged
type Notes = tuple[tuple[rna_enums.WmReportItems, str], ...]


class PopupKind(Enum):
    """Registered popup kind with its host base, window manager call operator, and layout button, a pie opening through its operator alone."""

    MENU = (bpy.types.Menu, bpy.ops.wm.call_menu, "menu")
    PIE = (bpy.types.Menu, bpy.ops.wm.call_menu_pie, None)
    PANEL = (bpy.types.Panel, bpy.ops.wm.call_panel, "popover")
    SHELF = (bpy.types.AssetShelf, bpy.ops.wm.call_asset_shelf_popover, "template_asset_shelf_popover")

    def __init__(self, base: type[Struct], operator: Operation, button: str | None) -> None:
        """Holds the host base, the call operator, and the `UILayout` function the button draws through."""
        self.base, self.operator = base, operator
        self.button: Option[bpy.types.Function] = Option.of_optional(button).map(rna_definition(bpy.types.UILayout).functions.__getitem__)


class Invoke[T: bpy.types.PropertyGroup](Protocol):
    """Invoke form deriving the operator class from its command's class, `asks` stating whether it asks for input before the body."""

    asks: ClassVar[bool]

    def classed(self, body: "Body[T]", base: type[bpy.types.Operator], /) -> Acquisitions:
        """Builds the invoked class first among the acquisitions it holds."""
        ...


class Options[T: bpy.types.PropertyGroup](TypedDict, total=False):
    """Operator fields besides the form and body, as `Operator.of` takes them."""

    condition: Option[Condition]
    callbacks: Option[Callable[[Identity], Mapping[str, Callbacks[T]]]]
    icon: Option["rna_enums.IconItems"]
    options: frozenset["rna_enums.OperatorTypeFlagItems"]
    undo_group: Option[str]
    invoke: Option[Invoke[T]]
    seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]]
    draw: Option[Draw[T]]
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]]
    describe: Option[Callable[[bpy.types.Context, T], str]]
    primary: Option[str]
    named: Option[Key]


# --- [MODELS] ---------------------------------------------------------------------------


class Empty(bpy.types.PropertyGroup):
    """Form of operator without properties."""


class Command(msgspec.Struct, frozen=True, kw_only=True):
    """Extension operator with poll condition, button icon, and native flags, its host name and title derived from its key.

    Commands of one kind and key are one host operator and equal whatever callables they hold, a registration acquiring one twice registering it once.
    """

    # --- [STATE]

    condition: Option[Condition] = Nothing
    icon: Option["rna_enums.IconItems"] = Nothing
    options: frozenset["rna_enums.OperatorTypeFlagItems"] = frozenset({"UNDO"})
    undo_group: Option[str] = Nothing

    # --- [IDENTITY]

    @override
    def __eq__(self, other: object) -> bool:
        """Whether other is a command of the same kind under the same key."""
        return type(other) is type(self) and other.key == self.key

    @override
    def __hash__(self) -> int:
        """Hash of the kind and key."""
        return hash((type(self), self.key))

    # --- [TEXT]

    @property
    @abstractmethod
    def key(self) -> Key:
        """Identifier the host name derives from."""

    @property
    @abstractmethod
    def description(self) -> str:
        """Tooltip text."""

    @property
    def title(self) -> str:
        """Label the key displays as."""
        return bpy.path.display_name(self.key)

    @property
    def asks(self) -> bool:
        """Whether the command asks for input before it runs, its button label ending in an ellipsis."""
        return False

    # --- [REGISTRATION]

    @abstractmethod
    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds command registrations."""

    # --- [CLASS_MEMBERS]

    def namespace(self, name: str, /) -> dict[str, object]:
        """Returns class members with native flags, undo group, and poll messages."""
        return {
            "bl_idname": name,
            "bl_label": f"{self.title}..." if self.asks else self.title,
            "bl_description": self.description,
            "bl_options": self.options | {"REGISTER"},
            **self.undo_group.map(lambda group: {"bl_undo_group": group}).default_with(dict[str, str]),
            **polled(self.condition),
        }


class PopupTarget(msgspec.Struct, frozen=True):
    """Registered menu, panel, or asset shelf by class or extension key, opened as its kind, a class an extension or the stock interface declares."""

    target: "type[bpy.types.Menu | bpy.types.Panel | bpy.types.AssetShelf | _bpy_types.Menu | _bpy_types.Panel | _bpy_types.AssetShelf] | Key"
    kind: PopupKind


class Placement(msgspec.Struct, frozen=True):
    """Operator with property values in the host keymap-data form."""

    operator: Command | Operation | PopupTarget
    values: Mapping[str, Setting] = MappingProxyType({})

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations the placed operator registers."""
        return acquired_rows((self.operator,))

    @classmethod
    def context(cls, operator: Operation, path: RnaPath, /, **values: Setting) -> Self:
        """Places a stock `wm.context_*` operator over a context path with its other property values."""
        return cls(operator, MappingProxyType({"data_path": rna(path), **values}))

    def resolved(self, identity: Identity, /) -> tuple[str, Pairs]:
        """Resolves operator idname and property pairs, an opened popup's name leading its call operator's values."""
        values = tuple(self.values.items())
        match self.operator:
            case PopupTarget(kind=kind) as opened:
                return _dotted(kind.operator), (("name", idname(identity, opened)), *values)
            case operator:
                return idname(identity, operator), values


class Done(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with changes, report entries, and skipped faults."""

    notes: Notes = ()
    skipped: tuple[Fault, ...] = ()

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


class BoundBody[S, T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True):
    """Body running a step over what the call's identity binds, keyed and described by the step it wraps."""

    binding: Callable[[Identity], Outcome[S]]
    step: Callable[[S, Call[T]], Outcome[Answer]]

    def __call__(self, call: Call[T], /) -> Outcome[Answer]:
        """Runs the step over the binding of the calling extension."""
        return self.binding(call.identity).bind(lambda bound: self.step(bound, call))


class Operator[T: bpy.types.PropertyGroup](Command, frozen=True, kw_only=True):
    """Form group, identity-bound callbacks, body, and invoke form, keyed by the body function name unless stated, described by its docstring's first paragraph.

    Body function is the def `run` wraps, read through `__wrapped__`, `functools.partial`, and the step of a `BoundBody`.
    `describe` computes the tooltip from context and properties in place of the docstring.
    """

    # --- [STATE]

    form: type[T]
    run: Callable[[Call[T]], Outcome[Answer]]
    callbacks: Option[Callable[[Identity], Mapping[str, Callbacks[T]]]] = Nothing
    invoke: Option[Invoke[T]] = Nothing
    seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]] = Nothing
    draw: Option[Draw[T]] = Nothing
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]] = Nothing
    describe: Option[Callable[[bpy.types.Context, T], str]] = Nothing
    primary: Option[str] = Nothing
    named: Option[Key] = Nothing

    @staticmethod
    def of[U: bpy.types.PropertyGroup](form: type[U], /, **options: Unpack[Options[U]]) -> Callable[[Callable[[Call[U]], Outcome[Answer]]], "Operator[U]"]:
        """Returns the decorator building the operator of a body over the form, every other field a keyword."""
        return lambda run: Operator(form=form, run=run, **options)

    # --- [TEXT]

    @property
    @override
    def key(self) -> Key:
        """Stated key, else the body function name."""
        return self.named.default_with(lambda: _documented(self.run)[0])

    @property
    @override
    def description(self) -> str:
        """First paragraph of the body docstring."""
        return " ".join(_documented(self.run)[1].split("\n\n")[0].split()).removesuffix(".")

    @property
    @override
    def asks(self) -> bool:
        """Whether the invoke form asks for input before the body runs."""
        return any(form.asks for form in self.invoke.to_list())

    # --- [REGISTRATION]

    @property
    def invoked(self) -> bool:
        """Whether placement invokes to open form or seed properties."""
        return self.invoke.is_some() or self.seed.is_some()

    @property
    def bl_property(self) -> Option[str]:
        """Primary property a search popup edits and an enum button's tooltip describes, the form's one enum while none is stated."""
        enums = tuple(name for name, definition in annotated(self.form).items() if definition.function is bpy.props.EnumProperty)
        return self.primary.or_else(Some(enums[0]) if len(enums) == 1 else Nothing)

    @override
    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds the form's property groups, then the operator class its invoke form derives with that form's release scopes."""
        name, body = identity.name(bpy.types.Operator, self.key), Body(command=self, identity=identity)
        form = self.callbacks.map(lambda callbacks: rebound(self.form, **callbacks(identity))).default_value(self.form)
        base: type[bpy.types.Operator] = type(
            name,
            (bpy.types.Operator,),
            {
                "__annotations__": annotated(form),
                "execute": body.execute,
                **self.namespace(name),
                **self.bl_property.map(lambda member: {"bl_property": member}).default_value({}),
                **self.draw.map(lambda _: {"draw": body.draw}).default_value({}),
                **self.check.map(lambda callback: {"check": callback}).default_value({}),
                **self.describe.map(lambda describe: {"description": classmethod(lambda _cls, context, properties: describe(context, properties))}).default_value({}),
                **self.seed.map(lambda _: {"invoke": body.invoking(lambda operator, context, _event: Ok(body.execute(operator, context)))}).default_value({}),
            },
        )
        return (*nested(form), *self.invoke.map(lambda invoked: invoked.classed(body, base)).default_value((base,)))


class Body[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Callbacks one operator class composes over its command."""

    # --- [STATE]

    command: Operator[T]
    identity: Identity

    # --- [HOSTS]

    @staticmethod
    def properties(operator: bpy.types.Operator, /) -> T:
        """Returns operator properties typed as the form group."""
        properties: T = operator.properties  # ty: ignore[invalid-assignment]
        return properties

    def derived(self, base: type[bpy.types.Operator], namespace: Mapping[str, object], /, *mixins: type) -> type:
        """Returns the class an invoke form registers for base after the mixins, restating `bl_property` Blender reads from a class's own namespace."""
        return type(base.__name__, (*mixins, base), {**self.command.bl_property.map(lambda member: {"bl_property": member}).default_value({}), **namespace})

    # --- [REPORTS]

    @staticmethod
    def noted(operator: bpy.types.Operator, entries: Iterable[tuple["rna_enums.WmReportItems", str]], outcome: "rna_enums.OperatorReturnItems", /) -> set["rna_enums.OperatorReturnItems"]:
        """Reports entries, returning outcome."""
        for severity, text in entries:
            operator.report({severity}, text)
        return {outcome}

    @staticmethod
    def finished(operator: bpy.types.Operator, result: Outcome[Answer], /) -> set["rna_enums.OperatorReturnItems"]:
        """Reports body result as notes and skipped faults with FINISHED, a reason with CANCELLED, or rendered faults with CANCELLED."""
        match result:
            case Result(tag="ok", ok=Done(notes=notes, skipped=skipped)):
                return Body.noted(operator, (*notes, *(("WARNING", text) for _, text in rendered(skipped))), "FINISHED")
            case Result(tag="ok", ok=Unchanged(reason=reason)):
                return Body.noted(operator, (("INFO", reason),), "CANCELLED")
            case Result(error=faults):
                return Body.noted(operator, rendered(faults), "CANCELLED")

    # --- [EXECUTION]

    def performed(self, operator: bpy.types.Operator, context: bpy.types.Context, paths: tuple[Path, ...] = (), /) -> Outcome[Answer]:
        """Runs the command over the operator properties and the paths its invoke form chose."""
        return self.command.run(Call(identity=self.identity, context=context, properties=self.properties(operator), paths=paths))

    def chose(self, operator: bpy.types.Operator, context: bpy.types.Context, paths: tuple[Path, ...], /) -> set["rna_enums.OperatorReturnItems"]:
        """Runs the command over the paths a browser chose and reports its result, required fault when no path was chosen."""
        return self.finished(operator, Ok(paths).filter_with(bool, lambda _: Fault.of(Refusal.REQUIRED, f"chosen file for {self.command.title}")).bind(partial(self.performed, operator, context)))

    @property
    def execute(self) -> Callable[[bpy.types.Operator, bpy.types.Context], set["rna_enums.OperatorReturnItems"]]:
        """Class `execute` running the command and reporting its result."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            return self.finished(operator, self.performed(operator, context))

        return execute

    def modal_finished(self, operator: bpy.types.Operator, context: bpy.types.Context, release: Callable[[Option[bpy.types.Context]], Outcome[None]], /) -> set["rna_enums.OperatorReturnItems"]:
        """Runs the command after a modal run, releasing the run kept on Done and rolled back otherwise."""
        match self.performed(operator, context):
            case Result(tag="ok", ok=Done() as done):
                return self.finished(operator, Ok(release(Nothing).map(lambda _: done).default_with(lambda faults: msgspec.structs.replace(done, skipped=(*done.skipped, *faults)))))
            case Result(tag="ok", ok=Unchanged() as unchanged):
                return self.finished(operator, release(Some(context)).map(lambda _: unchanged))
            case Result(error=faults):
                return self.noted(operator, (*rendered(faults), *release(Some(context)).map(lambda _: ()).default_with(rendered)), "CANCELLED")

    # --- [DRAWING]

    @property
    def draw(self) -> Callable[[bpy.types.Operator, bpy.types.Context], None]:
        """Class `draw` laying out the command's draw over the operator properties."""

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(operator.layout).to_list():
                for drawn in self.command.draw.to_list():
                    drawn(layout, context, self.properties(operator), self.identity)

        return draw

    # --- [INVOCATION]

    def invoking[O: bpy.types.Operator](
        self, opening: Callable[[O, bpy.types.Context, bpy.types.Event], Outcome[set["rna_enums.OperatorReturnItems"]]], /
    ) -> Callable[[O, bpy.types.Context, bpy.types.Event], set["rna_enums.OperatorReturnItems"]]:
        """Returns invoke writing seed values into unset properties, then opening the form."""

        def invoke(operator: O, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
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

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Declarations the macro's steps register."""
        return acquired_rows(self.steps)

    @override
    def acquisitions(self, identity: Identity, /) -> Acquisitions:
        """Builds macro class, defining its steps after class registration."""
        name = identity.name(bpy.types.Operator, self.key)
        kind: type[bpy.types.Macro] = type(name, (bpy.types.Macro,), self.namespace(name))

        @contextmanager
        def defined() -> Generator[Outcome[None]]:
            yield accumulated(filled(placed, kind.define(placed[0]).properties) for step in self.steps for placed in (step.resolved(identity),)).map(lambda _: None)

        return kind, (Tier.OPERATORS, defined())


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [CALLS]


def idname(identity: Identity, target: Command | Operation | PopupTarget, /) -> str:
    """Resolves an extension key under its host name form, a class's RNA identifier, or a stock operator's `bpy.ops` path."""
    match target:
        case PopupTarget(target=str(key), kind=kind):
            return identity.name(kind.base, key)
        case PopupTarget(target=type() as opened) if issubclass(opened, bpy.types.bpy_struct):
            return rna_definition(opened).identifier
        case PopupTarget():
            raise TypeError(target)
        case Command() as command:
            return identity.name(bpy.types.Operator, command.key)
        case _:
            return _dotted(target)


def filled(placed: tuple[str, Pairs], properties: object, /) -> Outcome[None]:
    """Writes placed pairs onto the operator properties of a button, keymap item, gizmo, or macro step."""
    operator, pairs = placed
    return (
        Option
        .of_optional(try_downcast(bpy.types.OperatorProperties, properties))
        .to_result_with(lambda: Fault.of(Reason.ABSENT, operator, member="properties"))
        .bind(lambda held: assigned(held, {(name,): value for name, value in pairs}))
    )


def called(
    operator: Operation,
    properties: Mapping[str, Setting | Option[Setting]],
    /,
    *,
    context: "rna_enums.OperatorContextItems" = "EXEC_DEFAULT",
    undo: bool = False,
    override: Option[bpy.types.ContextTempOverride] = Nothing,
) -> Outcome[frozenset["rna_enums.OperatorReturnItems"]]:
    """Calls operator, leaving Nothing-valued properties unset."""
    keywords = {key: value for key, held in properties.items() for value in (held.to_list() if isinstance(held, Option) else (held,))}
    identifier = _dotted(operator)
    request = f"bpy.ops.{identifier}({', '.join(f'{key}={value!r}' for key, value in keywords.items())})"

    try:
        with override.default_with(nullcontext):
            answered: frozenset[rna_enums.OperatorReturnItems] = frozenset(operator(context, undo, **keywords))
    except AttributeError as error:
        return Error(Fault.of(Reason.ABSENT, "bpy.ops", error, member=identifier))
    except (RuntimeError, TypeError) as error:
        return Error(Fault.of(Reason.FAILED, request, error))
    return Ok(answered).filter_with(lambda answer: not answer.isdisjoint(("FINISHED", "RUNNING_MODAL", "INTERFACE")), lambda answer: Fault.of(Reason.FAILED, request, ", ".join(sorted(answer))))


def _dotted(operator: Operation, /) -> str:
    """Returns the `bpy.ops` path of an operator, which the stubs type as its own class over an unbound `idname_py`."""
    identifier: str = operator.idname_py()  # ty: ignore[missing-attribute, unsound-assignment]
    return identifier


# --- [ANSWERS]


def answered(applied: Applied, reason: str, notes: Notes = (), /) -> Outcome[Answer]:
    """Returns Done skipping write faults when a path was written, else write faults, else Unchanged."""
    match applied.written, applied.outcome.swap().default_value(()):
        case (_, *_), skipped:
            return Ok(Done(notes=notes, skipped=skipped))
        case (), (first, *rest):
            return Error((first, *rest))
        case _:
            return Ok(Unchanged(reason=reason))


def tallied(edits: tuple[Outcome[object], ...], notes: Notes = (), /) -> Outcome[Done]:
    """Returns Done skipping faults when any item changed, else every fault."""
    return accumulated(edits).map(lambda _: Done(notes=notes)).or_else_with(lambda faults: Error(faults) if all(edit.is_error() for edit in edits) else Ok(Done(notes=notes, skipped=faults)))


# --- [DRAWING]


def confirmed[S, T: bpy.types.PropertyGroup](
    binding: Callable[[Identity], Outcome[S]],
    message: Callable[[S, bpy.types.Context, T], Outcome[tuple[str, ...]]],
    fields: tuple[str, ...],
    layout: bpy.types.UILayout,
    context: bpy.types.Context,
    properties: T,
    identity: Identity,
    /,
) -> None:
    """Draws a confirmation dialog's message lines over what the identity binds, logging their faults, then the form fields it names."""
    for line in identity.reports.settled(binding(identity).bind(lambda bound: message(bound, context, properties))).default_value(()):
        layout.label(text=line, translate=False)
    for name in fields:
        layout.prop(properties, name)


# --- [BODIES]


def _documented[**P, R](run: Callable[P, R], /) -> tuple[str, str]:
    """Returns callable metadata through wrappers, partials, and bound bodies."""
    unwrapped: object = inspect.unwrap(run)
    match unwrapped:
        case partial(func=func) | BoundBody(step=func):
            return _documented(func)
        case FunctionType() | MethodType() as found:
            return found.__name__, inspect.getdoc(found) or ""
        case _:
            return type(unwrapped).__name__, inspect.getdoc(unwrapped) or ""


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Answer",
    "Body",
    "BoundBody",
    "Call",
    "Command",
    "Done",
    "Draw",
    "Empty",
    "Invoke",
    "Macro",
    "Notes",
    "Operation",
    "Operator",
    "Options",
    "Pairs",
    "Placement",
    "PopupKind",
    "PopupTarget",
    "Setting",
    "Unchanged",
    "answered",
    "called",
    "confirmed",
    "filled",
    "idname",
    "tallied",
]
