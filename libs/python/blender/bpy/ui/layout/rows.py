"""Layout rows over bound members, operator placements, registered popups, sub-layouts, live labels, and reports, realized once per extension identity."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import AbstractContextManager
from functools import partial
from itertools import product
from types import MappingProxyType
import typing
from typing import Final, Protocol, runtime_checkable

from _bl_ui_utils.layout import operator_context
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Command, filled, idname, Opened, Pairs, Placement, POPUP_CALLS
from blender.bpy.commands.operators import DISMISSAL, Operator
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import Descriptor, Scalar
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Registrable
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderReason
from blender.bpy.ui.layout.members import admits, Member, Read, reader, ResolvedMember
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.records.faults import accumulated, Fault, Outcome, paired, RecordReason

if typing.TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type RowDraw = Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]
type SurfaceDraw = Callable[[bpy.types.UILayout, bpy.types.Context], None]
type Keywords = Mapping[str, Scalar]
type ContextMember = Callable[[Struct, bpy.types.Context], Option[Struct | str]]
type Painted = bpy.types.Panel | bpy.types.Menu | bpy.types.Header | bpy.types.AddonPreferences
type RowContent = Row | RowDraw
type _Scoping = Callable[[bpy.types.UILayout, str], AbstractContextManager[None]]


@runtime_checkable
class Row(Protocol):
    """Layout declaration realizing its draw once per extension identity, registering what it holds when it is also `Registrable`."""

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the draw or every refusal."""
        ...


@runtime_checkable
class Container(Protocol):
    """Layout declaration holding rows of its own."""

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Rows the declaration holds."""
        ...


# --- [CONSTANTS] ------------------------------------------------------------------------

FUNCTIONS: Final = definition(bpy.types.UILayout).functions
_INVOKING: Final[Mapping[str, str]] = MappingProxyType({
    item.identifier: item.identifier.replace("EXEC", "INVOKE", 1)
    for item in Descriptor.declared(bpy.types.UILayout, ("operator_context",), bpy.types.EnumProperty).prop.enum_items
    if item.identifier.startswith("EXEC")
})
_SCOPED: Final[_Scoping] = operator_context  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
_REPORTED: Final[Mapping[str, "rna_enums.IconItems"]] = MappingProxyType({
    "ERROR": "STATUS_ERROR_FILLED",
    "WARNING": "STATUS_WARNING_FILLED",
    "INFO": "STATUS_INFO_FILLED",
    "DEBUG": "SYSTEM",
    "PROPERTY": "OPTIONS",
    "OPERATOR": "CHECKMARK",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Line(msgspec.Struct, frozen=True, kw_only=True):
    """Row with an optional availability condition, drawn inactive while it fails."""

    condition: Option[Condition] = Nothing

    def conditioned(self, draw: RowDraw) -> RowDraw:
        """Wraps draw in a column inactive while the condition fails."""
        return self.condition.map(lambda required: partial(_dimmed, required, draw)).default_value(draw)


class Layout(Line, frozen=True, kw_only=True):
    """Rows drawn in the sub-layout `container` opens, the drawing layout while unset, with the context members it publishes, hidden while `shown` fails.

    Rows an identity builder returns realize once the extension registers, the declarations they hold registering as the layout's `prerequisites`.
    """

    rows: tuple[RowContent, ...] | Callable[[Identity], Outcome[tuple[RowContent, ...]]]
    container: Option[Callable[[bpy.types.UILayout], bpy.types.UILayout]] = Nothing
    published: Mapping[str, ContextMember] = MappingProxyType({})
    shown: Option[Condition] = Nothing
    prerequisites: tuple[Declaration, ...] = ()

    @property
    def children(self) -> tuple[RowContent, ...]:
        """Rows declared as a tuple."""
        return self.rows if isinstance(self.rows, tuple) else ()

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the declared rows that register what they hold."""
        return Ok(acquired(self.children))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Realizes the rows once into the container draw."""
        rows = Ok(self.rows) if isinstance(self.rows, tuple) else self.rows(identity)
        return rows.bind(lambda found: realized(found, identity)).map(lambda draw: partial(self.opened, draw))

    def opened(self, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Opens the container with its context members and draws the rows into it while `shown` holds."""
        if admits(self.shown, context, Some(holder)):
            inner = self.container.map(lambda container: container(layout)).default_value(layout)
            _published(self.published, inner, holder, context)
            draw(inner, holder, context)


class Property(Line, frozen=True, kw_only=True):
    """Bound member drawn through a layout function taking its owner and property name first, `prop` unless stated, `prop_search` over the `search` collection.

    `keywords` are the function's own parameters, checked against its RNA definition at realization.
    The glyph passes as `icon_value`, an element path as `index`, and a member with a scope takes the label its owners give it.
    """

    target: Member
    function: bpy.types.Function = FUNCTIONS["prop"]
    keywords: Keywords = MappingProxyType({})
    search: Option[Member] = Nothing
    icon: Option[Glyph] = Nothing

    @property
    def drawing(self) -> bpy.types.Function:
        """Layout function the row calls."""
        return self.search.map(lambda _: FUNCTIONS["prop_search"]).default_value(self.function)

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Binds the target and search items, the glyph and keywords resolved once for every redraw."""
        function = self.drawing
        bindings = tuple((binding, ()) for binding in (self.target, *self.search.to_list()))
        return (
            _iconed(identity, self.icon)
            .bind(lambda icon: accepted(function, {**self.keywords, **icon}))
            .bind(lambda keywords: bound(bindings, lambda held: partial(self.located, function, keywords, held)))
        )

    def located(self, function: bpy.types.Function, keywords: Keywords, held: tuple[ResolvedMember, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the function at each located owner and search item pair, under the scope's label ahead of the stated keywords."""
        member, names = held[0], function.parameters
        index = member.descriptor.index.filter(lambda _: "index" in names).map(lambda at: {"index": at}).default_value({})
        options = self.target.labelled(member.descriptor, context).map(lambda label: {**{key: value for key, value in label.items() if key in names}, **keywords, **index})
        alerted(layout, options)
        for chosen, found in product(options.to_option().to_list(), product(*(each.at(holder, context).to_list() for each in held))):
            placed(layout, function, *(part for at in found for part in at), **chosen)


class OperatorButton(Line, frozen=True, kw_only=True):
    """Operator button over a placement, or a command placed with no values, through a layout operator function, `operator` unless stated.

    `keywords` are the function's parameters after the operator, checked at realization, and a command's own icon serves while no glyph is stated.
    Placement values fill the properties the function answers, a function answering none refusing a placement holding values.
    An invoked command raises its form even where the layout's opener sets an `EXEC_*` operator context.
    """

    target: Placement | Command
    function: bpy.types.Function = FUNCTIONS["operator"]
    keywords: Keywords = MappingProxyType({})
    icon: Option[Glyph] = Nothing
    published: Mapping[str, ContextMember] = MappingProxyType({})

    @property
    def placement(self) -> Placement:
        """Placement the button presses."""
        return self.target if isinstance(self.target, Placement) else Placement(self.target)

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the extension command."""
        return Ok(tuple(command for command in (self.placement.operator,) if isinstance(command, Registrable)))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the placement, glyph, and keywords once into the button draw."""
        placement, function = self.placement, self.function
        operator = placement.operator
        glyph = self.icon.or_else(operator.icon if isinstance(operator, Command) else Nothing)
        resolved = placement.resolved(identity).filter_with(
            lambda found: _answers(function) or not found[1],
            lambda found: Fault.of(RecordReason.OUTSIDE, function.identifier, ", ".join(name for name, _ in found[1]), member="an operator function answering properties"),
        )
        return paired(resolved, _iconed(identity, glyph).bind(lambda icon: accepted(function, {**self.keywords, **icon}))).map(
            lambda parts: partial(_button, self.published, function, *parts, invoked=_invoked(operator))
        )


class Opens(Line, frozen=True, kw_only=True):
    """Layout button opening a stock or extension menu, panel, or asset shelf through the button its kind names, `keywords` its parameters after the opened name."""

    target: Opened
    keywords: Keywords = MappingProxyType({})
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the opened name, button, glyph, and keywords once, refusing a kind that opens through its operator alone."""
        kind = self.target.kind
        button = POPUP_CALLS[kind].button.to_result_with(lambda: Fault.of(BlenderReason.ABSENT, kind.name, member="layout button"))
        return paired(idname(identity, self.target), button, _iconed(identity, self.icon)).bind(
            lambda parts: accepted(parts[1], {**self.keywords, **parts[2]}).map(lambda keywords: partial(_opened, parts[1], parts[0], keywords))
        )


class Label(Line, frozen=True, kw_only=True):
    """Fixed text, or texts and keyword sets read each redraw and drawn untranslated, through a label function, `label_multiline` unless stated.

    Each live text or keyword set draws one item, so `progress`, `link`, and `label_markdown` take live parameters.
    `label_multiline` draws as `label` in a horizontal layout, which gives it no width to wrap in.
    """

    text: str | Callable[[bpy.types.Context], Iterable[str | Keywords]]
    function: bpy.types.Function = FUNCTIONS["label_multiline"]
    keywords: Keywords = MappingProxyType({})
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the glyph and keywords once, returning the draw reading live text each redraw."""
        return _iconed(identity, self.icon).bind(lambda icon: accepted(self.function, {**self.keywords, **icon})).map(lambda keywords: partial(self.shown, keywords))

    def shown(self, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws each text, the stated keywords the drawing function takes kept."""
        function = FUNCTIONS["label"] if self.function == FUNCTIONS["label_multiline"] and layout.direction == "HORIZONTAL" else self.function
        chosen = {name: value for name, value in keywords.items() if name in function.parameters}
        fixed = isinstance(self.text, str)
        for part in (self.text,) if isinstance(self.text, str) else self.text(context):
            placed(layout, function, **{**chosen, **({} if fixed else {"translate": False}), **({"text": part} if isinstance(part, str) else part)})


class Status(Line, frozen=True, kw_only=True):
    """Report banner over the extension's held report entries, with the button dismissing them while any is held."""

    @staticmethod
    def acquisitions(identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the dismissal command."""
        return OperatorButton(target=DISMISSAL).acquisitions(identity)

    @staticmethod
    def drawn(identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the draw reading held entries each redraw."""
        return OperatorButton(target=DISMISSAL).drawn(identity).map(lambda dismiss: partial(_reported, identity, dismiss))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [BINDING]


def bound(bindings: Iterable[tuple[Member, tuple[type[Struct], ...]]], place: Callable[[tuple[ResolvedMember, ...]], RowDraw]) -> Outcome[RowDraw]:
    """Resolves fixed members once and reached members on each draw, each within its accepted kinds, drawing nothing while a reached owner is absent."""
    return reader(bindings).map(lambda read: partial(_bound, place, read))


def _bound(place: Callable[[tuple[ResolvedMember, ...]], RowDraw], read: Read, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws over the members this redraw locates, alerting refusals."""
    found = read(context)
    alerted(layout, found)
    for held in (present for each in found.to_option().to_list() for present in each.to_list()):
        place(held)(layout, holder, context)


# --- [DRAWING]


def pressed(layout: bpy.types.UILayout, identity: Identity, placement: Placement, keywords: Keywords = MappingProxyType({}), /) -> Outcome[None]:
    """Draws an operator button over a placement built this redraw, `keywords` its `operator` parameters."""
    operator = FUNCTIONS["operator"]
    return paired(placement.resolved(identity), accepted(operator, keywords)).bind(lambda parts: _pressed(layout, operator, *parts, invoked=_invoked(placement.operator)))


def split(layout: bpy.types.UILayout) -> tuple[bpy.types.UILayout, bpy.types.UILayout]:
    """Places label and value cells at the layout's split factor beside the decorator cell it reserves."""
    line = layout.row(align=True)
    halves = line.split(factor=layout.property_split_factor, align=True)
    names, values = halves.column(align=True), halves.row(align=True)
    values.use_property_split = False
    values.use_property_decorate = False
    if layout.use_property_decorate:
        line.label(text="", icon="BLANK1")
    return names, values


def gridded(text: str, layout: bpy.types.UILayout) -> bpy.types.UILayout:
    """Places text in the label cell, returning the value cell, the container of a layout function that draws no label of its own."""
    names, values = split(layout)
    names.alignment = "RIGHT"
    names.label(text=text)
    return values


def alerted[T](layout: bpy.types.UILayout, result: Outcome[T]) -> None:
    """Draws the report entries of a failed result."""
    for faults in result.swap().to_option().to_list():
        _listed(layout.column(align=True), rendered(faults))


def accepted(function: bpy.types.Function, keywords: Keywords) -> Outcome[Keywords]:
    """Accepts keywords the layout function names as parameters."""
    names = frozenset(function.parameters.keys())
    return Ok(keywords).filter_with(
        lambda held: held.keys() <= names, lambda held: Fault.of(RecordReason.OUTSIDE, function.identifier, ", ".join(sorted(held.keys() - names)), member=", ".join(sorted(names)))
    )


def placed(layout: bpy.types.UILayout, function: bpy.types.Function, /, *arguments: object, **keywords: object) -> object:
    """Places the item of a layout function by its RNA identifier, the one call of every row carrying host keywords."""
    return getattr(layout, function.identifier)(*arguments, **keywords)


def _iconed(identity: Identity, icon: Option[Glyph]) -> Outcome[Keywords]:
    """Resolves a glyph into the `icon_value` keyword, no keyword leaving the RNA icon."""
    return icon.map(lambda glyph: Icons.value(identity, glyph).map(lambda value: {"icon_value": value})).default_with(lambda: Ok({}))


def _invoked(operator: object) -> bool:
    """Whether a placed operator runs `invoke`, every operator but an extension command without invoke form or seed."""
    return not isinstance(operator, Operator) or operator.invoked


def _answers(function: bpy.types.Function) -> bool:
    """Whether a layout operator function returns the button's operator properties."""
    return any(parameter.is_output for parameter in function.parameters)


def _pressed(layout: bpy.types.UILayout, function: bpy.types.Function, operator: tuple[str, Pairs], keywords: Keywords, *, invoked: bool) -> Outcome[None]:
    """Draws the button under the invoking counterpart of an `EXEC_*` operator context, then fills the properties the function answers."""
    prior = layout.operator_context
    with _SCOPED(layout, _INVOKING.get(prior, prior) if invoked else prior):
        properties = placed(layout, function, operator[0], **keywords)
    return filled(operator, properties) if _answers(function) else Ok(None)


def _button(
    published: Mapping[str, ContextMember],
    function: bpy.types.Function,
    operator: tuple[str, Pairs],
    keywords: Keywords,
    layout: bpy.types.UILayout,
    holder: Struct,
    context: bpy.types.Context,
    *,
    invoked: bool,
) -> None:
    """Draws the button in a row holding the context members it publishes."""
    line = layout.row(align=True) if published else layout
    _published(published, line, holder, context)
    alerted(line, _pressed(line, function, operator, keywords, invoked=invoked))


def _published(members: Mapping[str, ContextMember], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Publishes each context member the holder and context reach, a string through `context_string_set` and a struct through `context_pointer_set`."""
    for name, member in members.items():
        for value in member(holder, context).to_list():
            match value:
                case str():
                    layout.context_string_set(name, value)
                case _:
                    layout.context_pointer_set(name, value)


def _opened(function: bpy.types.Function, name: str, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
    """Draws the opener button."""
    placed(layout, function, name, **keywords)


def _dimmed(required: Condition, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws in a column inactive while the condition fails over the holder."""
    column = layout.column()
    column.active = required.holds(context, Some(holder))
    draw(column, holder, context)


def _listed(layout: bpy.types.UILayout, entries: Iterable[tuple["rna_enums.WmReportItems", str]]) -> None:
    """Draws report entries untranslated under the icon their severity takes in the host's report drawing, errors on alert rows."""
    for kind, text in entries:
        severity = "ERROR" if kind.startswith("ERROR") else kind
        line = layout.row()
        line.alert = severity == "ERROR"
        line.label(text=text, icon=_REPORTED[severity], translate=False)


def _reported(identity: Identity, dismiss: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws the report banner with the dismissal button beside it while entries are held, then the extension's held entries."""
    entries = identity.reports.entries
    line = layout.row()
    line.template_reports_banner()
    if entries:
        dismiss(line, holder, context)
    _listed(layout, entries)


# --- [REALIZATION]


def acquired(rows: Iterable[object]) -> Acquisitions:
    """Rows that register what they hold."""
    return tuple(row for row in rows if isinstance(row, Registrable))


def realized(rows: Iterable[RowContent], identity: Identity) -> Outcome[RowDraw]:
    """Realizes rows once into one draw over the holder in row order, each `Line` under its condition and plain draws passed through."""

    def drawn(row: RowContent) -> Outcome[RowDraw]:
        match row:
            case Row():
                found = row.drawn(identity)
                return found.map(row.conditioned) if isinstance(row, Line) else found
            case Callable():
                return Ok(row)

    return accumulated(map(drawn, rows)).map(lambda draws: draws[0] if len(draws) == 1 else partial(_joined, draws))


def surfaced(rows: Iterable[RowContent], identity: Identity) -> Outcome[SurfaceDraw]:
    """Realizes rows once into one draw over the context as holder."""
    return realized(rows, identity).map(painted)


def painted(draw: RowDraw) -> SurfaceDraw:
    """Draws over the context as holder."""
    return partial(_painted, draw)


def layout_method(paint: SurfaceDraw) -> Callable[[Painted, bpy.types.Context], None]:
    """Returns a host draw method painting the owner's layout, a function as the host's class check requires."""

    def draw(owner: Painted, context: bpy.types.Context) -> None:
        layout: bpy.types.UILayout = owner.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        paint(layout, context)

    return draw


def _painted(draw: RowDraw, layout: bpy.types.UILayout, context: bpy.types.Context) -> None:
    """Draws over the context as holder."""
    draw(layout, context, context)


def _joined(draws: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws in row order over one holder."""
    for draw in draws:
        draw(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "FUNCTIONS",
    "Container",
    "ContextMember",
    "Keywords",
    "Label",
    "Layout",
    "Line",
    "Opens",
    "OperatorButton",
    "Painted",
    "Property",
    "Row",
    "RowContent",
    "RowDraw",
    "Status",
    "SurfaceDraw",
    "accepted",
    "acquired",
    "alerted",
    "bound",
    "gridded",
    "layout_method",
    "painted",
    "placed",
    "pressed",
    "realized",
    "split",
    "surfaced",
]
