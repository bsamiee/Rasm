"""Layout rows drawn once per extension identity, each declaring what it reads, and the containers rows open."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import AbstractContextManager
from functools import partial
from pathlib import Path
from types import MappingProxyType
from typing import Final, override, Protocol, runtime_checkable

from _bl_ui_utils.layout import operator_context
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.operators import Command, dismiss_reports, filled, idname, Operator, Pairs, Placement, PopupTarget
from blender.bpy.data.rna.conditions import Condition, satisfied
from blender.bpy.data.rna.descriptors import Descriptor, Scalar
from blender.bpy.data.rna.paths import definition, Struct
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Dependent, Registrable
from blender.bpy.extension.reports import alerted, listed
from blender.bpy.ui.layout.members import bound, LocatedMembers, Member
from blender.bpy.ui.resources.icons import Icon, ICONS, Icons
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type RowDraw = Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]
type Keywords = Mapping[str, Scalar]
type ContextMember = Callable[[Struct, bpy.types.Context], Option[Struct | str]]
type LayoutOwner = bpy.types.Panel | bpy.types.Menu | bpy.types.Header | bpy.types.AddonPreferences
type RowContent = Row | RowDraw
type _Scoping = Callable[[bpy.types.UILayout, str], AbstractContextManager[None]]


@runtime_checkable
class Row(Protocol):
    """Layout declaration realizing its draw once per extension identity, registering what it holds when it is `Registrable`."""

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

# --- [MODELS] ---------------------------------------------------------------------------


class ConditionalRow(msgspec.Struct, frozen=True, kw_only=True):
    """Row with an optional availability condition, drawn inactive while it fails."""

    condition: Option[Condition] = Nothing

    def conditioned(self, draw: RowDraw) -> RowDraw:
        """Wraps draw in a column inactive while the condition fails."""
        return partial(self.dimmed, draw) if self.condition.is_some() else draw

    def dimmed(self, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws in a column inactive while the condition fails over the holder."""
        column = layout.column()
        column.active = satisfied(self.condition, context, Some(holder))
        draw(column, holder, context)


class Layout(ConditionalRow, frozen=True, kw_only=True):
    """Rows drawn in the sub-layout `container` opens, the drawing layout while unset, with the context members it publishes, hidden while `visible` fails.

    Rows an identity builder returns realize once the extension registers, the declarations they hold registering as the layout's `prerequisites`.
    """

    rows: tuple[RowContent, ...] | Callable[[Identity], Outcome[tuple[RowContent, ...]]]
    container: Option[Callable[[bpy.types.UILayout], bpy.types.UILayout]] = Nothing
    published: Mapping[str, ContextMember] = MappingProxyType({})
    visible: Option[Condition] = Nothing
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
        """Opens the container with its context members and draws the rows into it while `visible` holds."""
        if satisfied(self.visible, context, Some(holder)):
            inner = self.container.map(lambda container: container(layout)).default_value(layout)
            _published(self.published, inner, holder, context)
            draw(inner, holder, context)


class Iconed(ConditionalRow, frozen=True, kw_only=True):
    """Row drawn under an optional icon with its layout function's `keywords`, declaring the icon set a packaged icon reads."""

    icon: Option[Icon] = Nothing
    keywords: Keywords = MappingProxyType({})

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Icon set a packaged icon reads."""
        return icon_prerequisites(self.icon)


class Property(Iconed, frozen=True, kw_only=True):
    """Bound member drawn through a layout function, `prop` unless stated, `prop_search` over the `search` collection while no other function is stated.

    Function naming a `property` parameter takes the member's owner and name first, any other the struct the member points at, of the type its first parameter declares.
    `keywords` are the function's own parameters, checked against its RNA definition at realization.
    Icon passes as `icon_value`, an element path as `index`, and a member with a scope takes the mixed label while its owners' values differ.
    """

    target: Member
    function: bpy.types.Function = FUNCTIONS["prop"]
    search: Option[Member] = Nothing

    @property
    @override
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Attachments the target and search items read, and the icon set a packaged icon reads."""
        return (*self.target.prerequisites, *(held for member in self.search.to_list() for held in member.prerequisites), *super().prerequisites)

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Locates and binds the target and search items within the struct type a struct-taking function declares, the icon and keywords resolved once for every redraw."""
        function = FUNCTIONS["prop_search"] if self.search.is_some() and self.function == FUNCTIONS["prop"] else self.function
        match function.parameters[0]:
            case bpy.types.PointerProperty(fixed_type=bpy.types.bpy_struct() as target) if "property" not in function.parameters:
                kinds: tuple[type[Struct], ...] = (type(target),)
            case _:
                kinds = ()
        located = accumulated(member.located(identity) for member in (self.target, *self.search.to_list()))
        return paired(_keyworded(identity, function, self.keywords, self.icon), located).bind(lambda parts: bound(parts[1], partial(self.shown, function, parts[0]), kinds))

    def shown(self, function: bpy.types.Function, keywords: Keywords, held: LocatedMembers, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the function at the located owner and search items, or at the located struct, under the scope's label ahead of the stated keywords."""
        (member, _), names = held[0], function.parameters
        index = member.descriptor.index.filter(lambda _: "index" in names).map(lambda at: {"index": at}).default_value({})
        options = self.target.labelled(member.descriptor, context).map(lambda label: {**{key: value for key, value in label.items() if key in names}, **keywords, **index})
        arguments = tuple(part for _, at in held for part in at) if "property" in names else (getattr(*held[0][1]),)
        alerted(layout, options)
        for chosen in options.to_option().to_list():
            layout_call(layout, function, *arguments, **chosen)


class OperatorButton(Iconed, frozen=True, kw_only=True):
    """Operator button over a placement, or a command layout_call with no values, through a layout operator function, `operator` unless stated.

    `keywords` are the function's parameters after the operator, checked at realization, and a command's own icon serves while none is stated.
    Placement values fill the properties the function answers, a function answering none refusing a placement holding values.
    Invoked command raises its form even where the layout's opener sets an `EXEC_*` operator context.
    """

    target: Placement | Command
    function: bpy.types.Function = FUNCTIONS["operator"]
    published: Mapping[str, ContextMember] = MappingProxyType({})

    @property
    def placement(self) -> Placement:
        """Placement the button presses."""
        return self.target if isinstance(self.target, Placement) else Placement(self.target)

    def acquisitions(self, _identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the extension command."""
        return Ok(tuple(command for command in (self.placement.operator,) if isinstance(command, Registrable)))

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the placement, icon, and keywords once into the button draw."""
        placement, function = self.placement, self.function
        operator = placement.operator
        resolved = placement.resolved(identity).filter_with(
            lambda found: _answers(function) or not found[1], lambda found: Fault.of(Reason.OUTSIDE, function.identifier, ", ".join(name for name, _ in found[1]), member="no operator properties")
        )
        icon = self.icon.or_else(operator.icon if isinstance(operator, Command) else Nothing)
        return paired(resolved, _keyworded(identity, function, self.keywords, icon)).map(lambda parts: partial(self.shown, function, *parts))

    def shown(self, function: bpy.types.Function, operator: tuple[str, Pairs], keywords: Keywords, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the button in a row holding the context members it publishes."""
        line = layout.row(align=True) if self.published else layout
        _published(self.published, line, holder, context)
        alerted(line, _button(line, function, operator, keywords, invoked=_invoked(self.placement.operator)))


class Opens(Iconed, frozen=True, kw_only=True):
    """Layout button opening a stock or extension menu, panel, or asset shelf through the button its kind names, `keywords` its parameters after the opened name."""

    target: PopupTarget

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the opened name, button, icon, and keywords once, refusing a kind that opens through its operator alone."""
        kind = self.target.kind
        button = kind.button.to_result_with(lambda: Fault.of(Reason.ABSENT, kind.name, member="layout button"))
        return paired(idname(identity, self.target), button.bind(lambda function: _keyworded(identity, function, self.keywords, self.icon).map(lambda keywords: (function, keywords)))).map(
            lambda parts: partial(self.shown, parts[1][0], parts[0], parts[1][1])
        )

    @staticmethod
    def shown(function: bpy.types.Function, name: str, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the opener button."""
        layout_call(layout, function, name, **keywords)


class Label(Iconed, frozen=True, kw_only=True):
    """Fixed text, or texts and keyword sets read each redraw and drawn untranslated, through a label function, `label_multiline` unless stated.

    Each live text or keyword set draws one item, `progress`, `link`, and `label_markdown` taking live parameters.
    `label_multiline` draws as `label` in a horizontal layout, which gives it no width to wrap in.
    """

    text: str | Callable[[bpy.types.Context], Iterable[str | Keywords]]
    function: bpy.types.Function = FUNCTIONS["label_multiline"]

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the icon and keywords once, returning the draw reading live text each redraw."""
        return _keyworded(identity, self.function, self.keywords, self.icon).map(lambda keywords: partial(self.shown, keywords))

    def shown(self, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws each text, the stated keywords the drawing function takes kept."""
        function = FUNCTIONS["label"] if self.function == FUNCTIONS["label_multiline"] and layout.direction == "HORIZONTAL" else self.function
        chosen = {name: value for name, value in keywords.items() if name in function.parameters}
        parts = ((self.text, {}),) if isinstance(self.text, str) else ((part, {"translate": False}) for part in self.text(context))
        for part, untranslated in parts:
            layout_call(layout, function, **{**chosen, **untranslated, **({"text": part} if isinstance(part, str) else part)})


class Separator(ConditionalRow, frozen=True, kw_only=True):
    """Separator between rows, `keywords` the `separator` parameters (`type`, `factor`) checked at realization."""

    keywords: Keywords = MappingProxyType({})

    def drawn(self, _identity: Identity, /) -> Outcome[RowDraw]:
        """Checks the keywords once into the separator draw."""
        return accepted(FUNCTIONS["separator"], self.keywords).map(lambda keywords: partial(Separator.shown, keywords))

    @staticmethod
    def shown(keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the separator."""
        layout_call(layout, FUNCTIONS["separator"], **keywords)


class Status(ConditionalRow, frozen=True, kw_only=True):
    """Report banner over the extension's held report entries, with the button dismissing them while any is held."""

    @staticmethod
    def acquisitions(_identity: Identity, /) -> Outcome[Acquisitions]:
        """Acquires the dismissal command."""
        return Ok((dismiss_reports,))

    @staticmethod
    def drawn(identity: Identity, /) -> Outcome[RowDraw]:
        """Returns the draw reading held entries each redraw."""
        return OperatorButton(target=dismiss_reports).drawn(identity).map(lambda dismiss: partial(Status.shown, identity, dismiss))

    @staticmethod
    def shown(identity: Identity, dismiss: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the report banner with the dismissal button beside it while entries are held, then the extension's held entries."""
        entries = identity.reports.entries
        line = layout.row()
        line.template_reports_banner()
        if entries:
            dismiss(line, holder, context)
        listed(layout, entries)


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [DRAWING]


def operator_button(layout: bpy.types.UILayout, identity: Identity, placement: Placement, keywords: Keywords = MappingProxyType({}), /) -> Outcome[None]:
    """Draws an operator button over a placement built this redraw, `keywords` its `operator` parameters."""
    operator = FUNCTIONS["operator"]
    return paired(placement.resolved(identity), accepted(operator, keywords)).bind(lambda parts: _button(layout, operator, *parts, invoked=_invoked(placement.operator)))


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


def aligned(layout: bpy.types.UILayout) -> bpy.types.UILayout:
    """Opens a row of aligned buttons."""
    return layout.row(align=True)


def grouped(layout: bpy.types.UILayout) -> bpy.types.UILayout:
    """Opens aligned buttons under a line separator, one group apart from the rows above, in as many columns as the region width fits whole."""
    layout.separator(type="LINE")
    return layout.grid_flow(row_major=True, even_columns=True, align=True)


def even(layout: bpy.types.UILayout) -> bpy.types.UILayout:
    """Opens a grid of even columns filled row by row, each property drawing its own label in its cell."""
    grid = layout.grid_flow(row_major=True, even_columns=True)
    grid.use_property_split = False
    return grid


def accepted(function: bpy.types.Function, keywords: Keywords) -> Outcome[Keywords]:
    """Accepts keywords the layout function names as parameters."""
    names = frozenset(function.parameters.keys())
    return Ok(keywords).filter_with(
        lambda held: held.keys() <= names, lambda held: Fault.of(Reason.OUTSIDE, function.identifier, ", ".join(sorted(held.keys() - names)), member=", ".join(sorted(names)))
    )


def layout_call(layout: bpy.types.UILayout, function: bpy.types.Function, /, *arguments: object, **keywords: object) -> object:
    """Places the item of a layout function by its RNA identifier, the one call of every row passing host keywords."""
    return getattr(layout, function.identifier)(*arguments, **keywords)


def _keyworded(identity: Identity, function: bpy.types.Function, keywords: Keywords, icon: Option[Icon]) -> Outcome[Keywords]:
    """Accepts the keywords with the icon as `icon_value`, none leaving the RNA icon."""
    stated: Outcome[Keywords] = icon.map(lambda held: Icons.value(identity, held).map(lambda value: {**keywords, "icon_value": value})).default_value(Ok(keywords))
    return stated.bind(partial(accepted, function))


def _invoked(operator: object) -> bool:
    """Whether a layout_call operator runs `invoke`, every operator but an extension command without invoke form or seed."""
    return not isinstance(operator, Operator) or operator.invoked


def _answers(function: bpy.types.Function) -> bool:
    """Whether a layout operator function returns the button's operator properties."""
    return any(parameter.is_output for parameter in function.parameters)


def _button(layout: bpy.types.UILayout, function: bpy.types.Function, operator: tuple[str, Pairs], keywords: Keywords, *, invoked: bool) -> Outcome[None]:
    """Draws the button under the invoking counterpart of an `EXEC_*` operator context, then fills the properties the function answers."""
    prior = layout.operator_context
    with _SCOPED(layout, _INVOKING.get(prior, prior) if invoked else prior):
        properties = layout_call(layout, function, operator[0], **keywords)
    return filled(operator, properties) if _answers(function) else Ok(None)


def _published(members: Mapping[str, ContextMember], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Publishes each context member the holder and context reach, a string through `context_string_set` and a struct through `context_pointer_set`."""
    for name, member in members.items():
        for value in member(holder, context).to_list():
            match value:
                case str():
                    layout.context_string_set(name, value)
                case _:
                    layout.context_pointer_set(name, value)


# --- [REALIZATION]


def acquired(rows: Iterable[object]) -> Acquisitions:
    """Returns the prerequisites of rows that register nothing, then the rows that register."""
    held = tuple(rows)
    return (
        *(declaration for row in held if isinstance(row, Dependent) and not isinstance(row, Registrable) for declaration in row.prerequisites),
        *(row for row in held if isinstance(row, Registrable)),
    )


def icon_prerequisites(icon: Option[Icon]) -> tuple[Declaration, ...]:
    """Icon set a packaged icon reads, none for a stock icon or data-block preview."""
    return (ICONS,) if icon.filter(lambda held: isinstance(held, Path)).is_some() else ()


def realized(rows: Iterable[RowContent], identity: Identity) -> Outcome[RowDraw]:
    """Realizes rows once into one draw over the holder in row order, each `ConditionalRow` under its condition and plain draws passed through."""

    def drawn(row: RowContent) -> Outcome[RowDraw]:
        match row:
            case Row():
                found = row.drawn(identity)
                return found.map(row.conditioned) if isinstance(row, ConditionalRow) else found
            case Callable():
                return Ok(row)

    return accumulated(map(drawn, rows)).map(lambda draws: draws[0] if len(draws) == 1 else partial(_joined, draws))


def layout_method(draw: RowDraw) -> Callable[[LayoutOwner, bpy.types.Context], None]:
    """Returns a host draw method drawing over the context as holder in the owner's layout, a function as the host's class check requires."""

    def method(owner: LayoutOwner, context: bpy.types.Context) -> None:
        layout: bpy.types.UILayout = owner.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        draw(layout, context, context)

    return method


def _joined(draws: tuple[RowDraw, ...], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws in row order over one holder."""
    for draw in draws:
        draw(layout, holder, context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "FUNCTIONS",
    "ConditionalRow",
    "Container",
    "ContextMember",
    "Iconed",
    "Keywords",
    "Label",
    "Layout",
    "LayoutOwner",
    "Opens",
    "OperatorButton",
    "Property",
    "Row",
    "RowContent",
    "RowDraw",
    "Separator",
    "Status",
    "accepted",
    "acquired",
    "aligned",
    "even",
    "gridded",
    "grouped",
    "icon_prerequisites",
    "layout_call",
    "layout_method",
    "operator_button",
    "realized",
    "split",
]
