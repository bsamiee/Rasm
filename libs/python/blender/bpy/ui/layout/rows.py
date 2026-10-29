"""Member bindings rooted at an owner type, and the layout rows realized over them once per extension identity: bound members, operator placements, registered popups, sub-layouts, live labels, and reports."""

from collections.abc import Callable, Iterable, Mapping
from contextlib import AbstractContextManager
from functools import partial
from itertools import pairwise
from types import MappingProxyType
from typing import Final, overload, Protocol, runtime_checkable, Self

from _bl_ui_utils.layout import operator_context
import bpy
from expression import Nothing, Ok, Option, Some
import msgspec

from blender.bpy.commands.calls import Command, filled, idname, Opened, Pairs, Placement
from blender.bpy.commands.operators import dismiss_reports, Operator
from blender.bpy.data.rna.conditions import admits, Condition, Scoped
from blender.bpy.data.rna.descriptors import Descriptor, Scalar
from blender.bpy.data.rna.paths import definition, Native, rna, RnaPath, Struct
from blender.bpy.data.rna.snapshots import Snapshot
from blender.bpy.extension.identity import Identity
from blender.bpy.extension.registration import Acquisitions, Declaration, Registrable
from blender.bpy.extension.reports import alerted, listed
from blender.bpy.ui.resources.icons import Glyph, Icons
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason

# --- [TYPES] ----------------------------------------------------------------------------

type At = tuple[Native, str]
type Reach = Callable[[bpy.types.Context], Option[Struct]]
type RowDraw = Callable[[bpy.types.UILayout, Struct, bpy.types.Context], None]
type Keywords = Mapping[str, Scalar]
type ContextMember = Callable[[Struct, bpy.types.Context], Option[Struct | str]]
type Painted = bpy.types.Panel | bpy.types.Menu | bpy.types.Header | bpy.types.AddonPreferences
type RowContent = Row | RowDraw
type _Kinds = tuple[type[Struct], ...]
type _Located = tuple[tuple[ResolvedMember, At], ...]
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

# --- [MODELS] ---------------------------------------------------------------------------

# --- [MEMBERS]


class ResolvedMember[P: bpy.types.Property = bpy.types.Property](msgspec.Struct, frozen=True, kw_only=True):
    """RNA definition with the reach of its owner's root, the drawing holder or the context root while no reach is stated."""

    descriptor: Descriptor[P]
    reach: Option[Reach] = Nothing

    def at(self, holder: Struct, context: bpy.types.Context) -> Option[At]:
        """Locates the property owner through the reach, the drawing holder, or the context root."""
        descriptor = self.descriptor
        owner = self.reach.map(lambda reach: reach(context).bind(descriptor.owner_of)).default_with(lambda: descriptor.owner_of(context, Some(holder)))
        return owner.map(lambda found: (found, descriptor.prop.identifier))

    def accepts(self, kinds: _Kinds) -> Outcome[Self]:
        """Accepts a property or pointer target of the kinds, every kind while none is given."""
        prop = self.descriptor.prop
        target = prop.fixed_type if isinstance(prop, bpy.types.PointerProperty) else prop
        return Ok(self).filter_with(
            lambda _: not kinds or isinstance(prop, kinds) or isinstance(target, kinds),
            lambda _: Fault.of(Reason.OUTSIDE, rna(self.descriptor.path), type(target).__name__, member=", ".join(kind.__name__ for kind in kinds)),
        )

    def current(self, _kinds: _Kinds, _context: bpy.types.Context, /) -> Outcome[Option[Self]]:
        """Member resolved once at realization, the same at every draw."""
        return Ok(Some(self))


class Member(msgspec.Struct, frozen=True, kw_only=True):
    """Property path rooted at its owner type, reached through context when the owner is no drawing holder, labelled as mixed while the owners `scope` selects differ."""

    root: type[Struct]
    path: RnaPath
    reach: Option[Reach] = Nothing
    scope: Option[Scoped] = Nothing

    @overload
    def resolved(self, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]: ...

    @overload
    def resolved[P: bpy.types.Property](self, kind: type[P], *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember[P]]: ...

    def resolved(self, kind: type[bpy.types.Property] = bpy.types.Property, *, root: Option[Struct] = Nothing) -> Outcome[ResolvedMember]:
        """Resolves metadata on the root type, or on a live `root` of it whose refined definition answers."""
        held: Outcome[type[Struct] | Struct] = root.map(
            lambda actual: Ok(actual).filter_with(lambda found: isinstance(found, self.root), lambda found: Fault.of(Reason.OUTSIDE, rna(self.path), type(found).__name__, member=self.root.__name__))
        ).default_value(Ok(self.root))
        return held.bind(lambda found: Descriptor.resolve(found, self.path, kind)).map(lambda descriptor: ResolvedMember(descriptor=descriptor, reach=self.reach))

    def bound(self, kinds: _Kinds = (), /) -> Outcome[ResolvedMember | Self]:
        """Resolves a member with no reach once within the kinds, a reached member staying to resolve on its live root each draw."""
        match self.reach:
            case Option(tag="some"):
                return Ok(self)
            case _:
                return self.resolved().bind(lambda held: held.accepts(kinds))

    def current(self, kinds: _Kinds, context: bpy.types.Context, /) -> Outcome[Option[ResolvedMember]]:
        """Resolves on the root the reach finds this draw within the kinds, `Nothing` while it finds none."""
        return self.reach.bind(lambda reach: reach(context)).map(lambda root: self.resolved(root=Some(root)).bind(lambda held: held.accepts(kinds)).map(Some)).default_value(Ok(Nothing))

    def labelled(self, setting: Descriptor, context: bpy.types.Context) -> Outcome[Mapping[str, str | bool]]:
        """Mixed label keywords of the setting across the scope's owners, none without a scope or while the owners agree so the host labels the property itself."""
        return (
            self.scope
            .map(lambda scope: scope.owners(context).bind(partial(marked, setting)))
            .default_value(Ok(Nothing))
            .map(lambda mixed: {"text": mixed.default_value(""), "translate": False} if mixed.is_some() else {})
        )


# --- [ROWS]


class Line(msgspec.Struct, frozen=True, kw_only=True):
    """Row with an optional availability condition, drawn inactive while it fails."""

    condition: Option[Condition] = Nothing

    def conditioned(self, draw: RowDraw) -> RowDraw:
        """Wraps draw in a column inactive while the condition fails."""
        return partial(self.dimmed, draw) if self.condition.is_some() else draw

    def dimmed(self, draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws in a column inactive while the condition fails over the holder."""
        column = layout.column()
        column.active = admits(self.condition, context, Some(holder))
        draw(column, holder, context)


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
    """Bound member drawn through a layout function, `prop` unless stated, `prop_search` over the `search` collection while no other function is stated.

    A function naming a `property` parameter takes the member's owner and name first, any other the struct the member points at, of the type its first parameter declares.
    `keywords` are the function's own parameters, checked against its RNA definition at realization.
    The glyph passes as `icon_value`, an element path as `index`, and a member with a scope takes the mixed label while its owners' values differ.
    """

    target: Member
    function: bpy.types.Function = FUNCTIONS["prop"]
    keywords: Keywords = MappingProxyType({})
    search: Option[Member] = Nothing
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Binds the target and search items within the struct type a struct-taking function declares, the glyph and keywords resolved once for every redraw."""
        function = FUNCTIONS["prop_search"] if self.search.is_some() and self.function == FUNCTIONS["prop"] else self.function
        match function.parameters[0]:
            case bpy.types.PointerProperty(fixed_type=bpy.types.bpy_struct() as target) if "property" not in function.parameters:
                kinds: _Kinds = (type(target),)
            case _:
                kinds = ()
        return _keyworded(identity, function, self.keywords, self.icon).bind(lambda keywords: bound((self.target, *self.search.to_list()), partial(self.shown, function, keywords), kinds))

    def shown(self, function: bpy.types.Function, keywords: Keywords, held: _Located, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws the function at the located owner and search items, or at the located struct, under the scope's label ahead of the stated keywords."""
        (member, _), names = held[0], function.parameters
        index = member.descriptor.index.filter(lambda _: "index" in names).map(lambda at: {"index": at}).default_value({})
        options = self.target.labelled(member.descriptor, context).map(lambda label: {**{key: value for key, value in label.items() if key in names}, **keywords, **index})
        arguments = tuple(part for _, at in held for part in at) if "property" in names else (getattr(*held[0][1]),)
        alerted(layout, options)
        for chosen in options.to_option().to_list():
            placed(layout, function, *arguments, **chosen)


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
        resolved = placement.resolved(identity).filter_with(
            lambda found: _answers(function) or not found[1],
            lambda found: Fault.of(Reason.OUTSIDE, function.identifier, ", ".join(name for name, _ in found[1]), member="an operator function answering properties"),
        )
        glyph = self.icon.or_else(operator.icon if isinstance(operator, Command) else Nothing)
        return paired(resolved, _keyworded(identity, function, self.keywords, glyph)).map(lambda parts: partial(self.shown, function, *parts))

    def shown(self, function: bpy.types.Function, operator: tuple[str, Pairs], keywords: Keywords, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the button in a row holding the context members it publishes."""
        line = layout.row(align=True) if self.published else layout
        _published(self.published, line, holder, context)
        alerted(line, _pressed(line, function, operator, keywords, invoked=_invoked(self.placement.operator)))


class Opens(Line, frozen=True, kw_only=True):
    """Layout button opening a stock or extension menu, panel, or asset shelf through the button its kind names, `keywords` its parameters after the opened name."""

    target: Opened
    keywords: Keywords = MappingProxyType({})
    icon: Option[Glyph] = Nothing

    def drawn(self, identity: Identity, /) -> Outcome[RowDraw]:
        """Resolves the opened name, button, glyph, and keywords once, refusing a kind that opens through its operator alone."""
        kind = self.target.kind
        button = kind.button.to_result_with(lambda: Fault.of(Reason.ABSENT, kind.name, member="layout button"))
        return paired(idname(identity, self.target), button.bind(lambda function: _keyworded(identity, function, self.keywords, self.icon).map(lambda keywords: (function, keywords)))).map(
            lambda parts: partial(self.shown, parts[1][0], parts[0], parts[1][1])
        )

    @staticmethod
    def shown(function: bpy.types.Function, name: str, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the opener button."""
        placed(layout, function, name, **keywords)


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
        return _keyworded(identity, self.function, self.keywords, self.icon).map(lambda keywords: partial(self.shown, keywords))

    def shown(self, keywords: Keywords, layout: bpy.types.UILayout, _holder: Struct, context: bpy.types.Context) -> None:
        """Draws each text, the stated keywords the drawing function takes kept."""
        function = FUNCTIONS["label"] if self.function == FUNCTIONS["label_multiline"] and layout.direction == "HORIZONTAL" else self.function
        chosen = {name: value for name, value in keywords.items() if name in function.parameters}
        parts = ((self.text, {}),) if isinstance(self.text, str) else ((part, {"translate": False}) for part in self.text(context))
        for part, untranslated in parts:
            placed(layout, function, **{**chosen, **untranslated, **({"text": part} if isinstance(part, str) else part)})


class Status(Line, frozen=True, kw_only=True):
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

# --- [LABELS]


def marked[P: bpy.types.Property](setting: Descriptor[P], owners: tuple[Struct, ...], /) -> Outcome[Option[str]]:
    """Translated name of a setting marked as mixed while owner values differ, nothing while they agree so the host labels it."""
    prop = setting.prop
    return accumulated(Snapshot.capture(owner, Some((setting,))) for owner in owners).map(
        lambda values: Some(f"{bpy.app.translations.pgettext_iface(prop.name, prop.translation_context)} —").filter(lambda _: any(left != right for left, right in pairwise(values)))
    )


# --- [BINDING]


def bound(members: tuple[Member, ...], place: Callable[[_Located, bpy.types.UILayout, Struct, bpy.types.Context], None], /, kinds: _Kinds = ()) -> Outcome[RowDraw]:
    """Resolves fixed members once and reached members each draw, each within the kinds, drawing over the located owners while every one is found."""
    return accumulated(member.bound(kinds) for member in members).map(lambda held: partial(_bound, place, kinds, held))


def _bound(
    place: Callable[[_Located, bpy.types.UILayout, Struct, bpy.types.Context], None],
    kinds: _Kinds,
    held: tuple[ResolvedMember | Member, ...],
    layout: bpy.types.UILayout,
    holder: Struct,
    context: bpy.types.Context,
) -> None:
    """Draws over the members and owners this redraw locates, alerting refusals."""
    found = accumulated(each.current(kinds, context) for each in held)
    alerted(layout, found)
    for members in found.to_option().to_list():
        located = tuple((member, at) for each in members for member in each.to_list() for at in member.at(holder, context).to_list())
        if len(located) == len(held):
            place(located, layout, holder, context)


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


def accepted(function: bpy.types.Function, keywords: Keywords) -> Outcome[Keywords]:
    """Accepts keywords the layout function names as parameters."""
    names = frozenset(function.parameters.keys())
    return Ok(keywords).filter_with(
        lambda held: held.keys() <= names, lambda held: Fault.of(Reason.OUTSIDE, function.identifier, ", ".join(sorted(held.keys() - names)), member=", ".join(sorted(names)))
    )


def placed(layout: bpy.types.UILayout, function: bpy.types.Function, /, *arguments: object, **keywords: object) -> object:
    """Places the item of a layout function by its RNA identifier, the one call of every row carrying host keywords."""
    return getattr(layout, function.identifier)(*arguments, **keywords)


def _keyworded(identity: Identity, function: bpy.types.Function, keywords: Keywords, icon: Option[Glyph]) -> Outcome[Keywords]:
    """Accepts the keywords with the glyph as `icon_value`, no glyph leaving the RNA icon."""
    glyphed: Outcome[Keywords] = icon.map(lambda glyph: Icons.value(identity, glyph).map(lambda value: {**keywords, "icon_value": value})).default_value(Ok(keywords))
    return glyphed.bind(partial(accepted, function))


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


def layout_method(draw: RowDraw) -> Callable[[Painted, bpy.types.Context], None]:
    """Returns a host draw method drawing over the context as holder in the owner's layout, a function as the host's class check requires."""

    def method(owner: Painted, context: bpy.types.Context) -> None:
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
    "At",
    "Container",
    "ContextMember",
    "Keywords",
    "Label",
    "Layout",
    "Line",
    "Member",
    "Opens",
    "OperatorButton",
    "Painted",
    "Property",
    "Reach",
    "ResolvedMember",
    "Row",
    "RowContent",
    "RowDraw",
    "Status",
    "accepted",
    "acquired",
    "bound",
    "gridded",
    "layout_method",
    "marked",
    "placed",
    "pressed",
    "realized",
    "split",
]
