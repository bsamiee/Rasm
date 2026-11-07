"""Active compositor entry editor beside the stock effect list: its commands and reset, grading wheels, curve and ramp editors, record fields no socket holds, scene settings, effect rows, and Viewer buttons, with the field commands."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from functools import cached_property, partial
from types import MappingProxyType, MemberDescriptorType
from typing import Annotated, Final

from bl_ui.properties_compositor import SCENE_PT_compositor_effects
import bpy
from expression import Option, Some, try_downcast
from expression.collections import Block
import msgspec
import msgspec.inspect
from typing_extensions import TypeForm
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.commands.invocations import Open, Popup
from blender.bpy.commands.operators import Answer, Call, Command, Done, Operator, Placement, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.bpy.data.rna.properties import Callbacks, choices, EnumItem, rebound
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.desired import Content, item_key, MenuItem
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor.effects import CompositorEffect, CompositorRecord, Named
from blender.bpy.render.compositor.entries import composites
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.offered import OfferedEffects
from blender.bpy.render.compositor.viewers import Viewers
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.elements.panels import Beside, Panel
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import even, FUNCTIONS, gridded, Keywords, Label, Layout, operator_button, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import fields, Section
from blender.bpy.ui.layout.templates import wheel
from blender.records.faults import accumulated, Error, Fault, Nothing, Ok, Outcome, Reason
from blender.records.fields import amended

# --- [TYPES] ----------------------------------------------------------------------------

type FilePath = Annotated[str, msgspec.Meta(min_length=1)]
type _Choices = Mapping[str, tuple[str, object]]
type _Edited = tuple[Key, tuple[str, ...], Operator[FieldEdit], Keywords, Mapping[str, float | str]]

# --- [CONSTANTS] ------------------------------------------------------------------------

EDITOR: Final[Key] = "active_effect"
_PICKER: Final[Keywords] = {"value_slider": True, "lock_luminosity": True, "cubic": True}

# --- [MODELS] ---------------------------------------------------------------------------


class _Field(msgspec.Struct, frozen=True, kw_only=True):
    """Native member path and declared type of one current field."""

    lens: tuple[MemberDescriptorType, ...]
    annotation: TypeForm[object]

    @property
    def names(self) -> tuple[str, ...]:
        """Field names along the native member path."""
        return tuple(member.__name__ for member in self.lens)


class FieldEdit(Named):
    """Record field of a named entry and the value a pick, a number, a text, or a file writes it to."""

    kind: bpy.props.StringProperty(name="Effect", description="Record key of the entry's effect", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    field: bpy.props.StringProperty(name="Field", description="Item key of the record field the edit writes, as the effect's interface keys it", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    choices: bpy.props.StringProperty(name="Choices", description="Choice labels for the field picker", default="{}", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    choice: bpy.props.EnumProperty(name="Value", description="Value the field takes", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    number: bpy.props.FloatProperty(name="Value", description="Number the field takes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    text: bpy.props.StringProperty(name="Value", description="Text the field takes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class EntryEditor(msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Editor of the scene's active compositor entry beside the stock effect list, `header` rows ahead of the entry's commands and `leading` and `trailing` rows around its body, each over the entry."""

    # --- [STATE]

    rows: OfferedEffects
    header: tuple[RowContent, ...] = ()
    leading: tuple[RowContent, ...] = ()
    trailing: tuple[RowContent, ...] = ()

    # --- [COMMANDS]

    @cached_property
    def form(self) -> type[FieldEdit]:
        """Field edit form, its choices the values the named field's type offers."""
        return rebound(FieldEdit, choice=Callbacks[FieldEdit](items=self.offered))

    @cached_property
    def choosing(self) -> Operator[FieldEdit]:
        """Command picking a field value through a search popup."""
        return Operator(form=self.form, run=self.choose_field_value, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("choice"))

    @cached_property
    def toggling(self) -> Operator[FieldEdit]:
        """Command setting a flag field to the state its button seeds."""
        return Operator(form=self.form, run=self.choose_field_value, named=Some("toggle_field_value"))

    def _dialog(self, run: Callable[[Call[FieldEdit]], Outcome[Answer]], member: str, /) -> Operator[FieldEdit]:
        """Command entering one form member in a dialog."""
        return Operator(
            form=self.form,
            run=run,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(lambda layout, _context, properties, _identity: layout.prop(properties, member)),
        )

    @cached_property
    def numbering(self) -> Operator[FieldEdit]:
        """Command entering a field number in a dialog."""
        return self._dialog(self.enter_field_number, "number")

    @cached_property
    def writing(self) -> Operator[FieldEdit]:
        """Command entering field text in a dialog."""
        return self._dialog(self.enter_field_text, "text")

    @cached_property
    def opening(self) -> Operator[FieldEdit]:
        """Command choosing a field file in the file browser."""
        return Operator(form=self.form, run=self.open_field_file, invoke=Some(Open(extensions=lambda _: ())))

    @cached_property
    def viewers(self) -> Viewers:
        """Viewer commands and buttons over the offered and pinned entries."""
        return Viewers(rows=self.rows)

    @staticmethod
    def offered(owner: FieldEdit, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
        """Choice labels supplied by the button, valued in snapshot order."""
        labels = msgspec.json.decode(owner.choices, type=dict[str, str])
        return choices((identifier, label, "", "NONE", index) for index, (identifier, label) in enumerate(labels.items()))

    def choose_field_value(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set record field to picked value."""
        chosen = call.properties
        return self.revised(
            call,
            lambda field: (
                _choices(field.annotation).bind(lambda found: Option.of_optional(found.get(chosen.choice))).to_result_with(partial(Fault.of, Reason.ABSENT, chosen.field, member=chosen.choice))
            ).map(lambda held: held[1]),
        )

    def enter_field_number(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set record field to entered number."""
        chosen = call.properties
        return self.revised(
            call, lambda field: _numeric(field.annotation).to_result_with(partial(Fault.of, Reason.ABSENT, chosen.kind, member=chosen.field)).map(lambda conversion: conversion(chosen.number))
        )

    def enter_field_text(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set record field to entered text."""
        return self.revised(call, lambda _field: Ok(call.properties.text))

    def open_field_file(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set record field to chosen file."""
        return (
            Block
            .of_seq(call.paths)
            .try_head()
            .to_result_with(partial(Fault.of, Reason.ABSENT, call.properties.field, member="file"))
            .bind(lambda path: self.revised(call, lambda _field: Ok(bpy.path.relpath(str(path)))))
        )

    @staticmethod
    def located(edit: FieldEdit, record: msgspec.Struct, /) -> Outcome[_Field]:
        """Resolves an edited field on the current record, absent after its case changes."""
        return (
            Option
            .of_optional(_declared(record).get(edit.field) if item_key(type(record)) == edit.kind else None)
            .map(lambda held: held[0])
            .to_result_with(partial(Fault.of, Reason.ABSENT, edit.kind, member=edit.field))
        )

    def revised(self, call: Call[FieldEdit], read: Callable[[_Field], Outcome[object]]) -> Outcome[Answer]:
        """Reads the edited value against the current field and rebuilds its immutable ancestors."""
        chosen = call.properties

        def changed(record: CompositorRecord) -> Outcome[CompositorRecord]:
            return self.located(chosen, record).bind(lambda field: read(field).bind(lambda value: _replaced(field, value, record)))

        return rooted(bpy.types.Scene, call.context).bind(
            lambda scene: self.rows.found(call.identity, scene, chosen.name).bind(
                lambda held: held.row.revised(call.identity, scene, held.entry, changed, form=held.row.record).map(
                    lambda wrote: Done() if wrote else Unchanged(reason=f"{held.entry.name}.{chosen.field} unchanged")
                )
            )
        )

    # --- [DRAWING]

    def parts(self, identity: Identity) -> Outcome[RowDraw]:
        """Realizes each offered effect's body once: its grading wheels, record field buttons, registered scene settings, and own rows."""

        def settings[G: bpy.types.PropertyGroup](attachment: Attachment[G]) -> Outcome[RowDraw]:
            return attachment.bound(identity).bind(lambda attached: realized(fields(attached.form, attachment.key), identity).map(lambda draw: partial(_scened, attached.struct, draw)))

        def body(row: CompositorEffect) -> Outcome[tuple[Key, RowDraw]]:
            scened = accumulated(settings(held) for held in row.registers if isinstance(held, Attachment) and held.owner is bpy.types.Scene)
            return scened.bind(
                lambda drawn: realized((wheels(row.interface, partial(_reached, identity)), partial(self.fielded, identity, row), *drawn, *row.rows), identity).map(lambda draw: (row.kind, draw))
            )

        return accumulated(map(body, self.rows.rows)).map(lambda found: partial(self.shown, identity, dict(found)))

    def editing(self, annotation: object, choices: Option[_Choices], /) -> Option[Operator[FieldEdit]]:
        """Command editing a field of the type: a flag toggle, a pick over choices, a number, a file, required or optional, or text, `Nothing` for a type none edits."""
        return (
            Some(self.toggling)
            .filter(lambda _: isinstance(_form(annotation), msgspec.inspect.BoolType))
            .or_else_with(lambda: choices.map(lambda _: self.choosing))
            .or_else_with(lambda: _numeric(annotation).map(lambda _: self.numbering))
            .or_else_with(lambda: Some(self.opening) if annotation in {FilePath, FilePath | None} else Nothing)
            .or_else_with(lambda: Some(self.writing) if isinstance(_form(annotation), msgspec.inspect.StrType) else Nothing)
        )

    def edited(self, interface: Interface, record: msgspec.Struct, /) -> tuple[_Edited, ...]:
        """Reads the edited fields of a record and the records it holds whose item key names no socket of the interface.

        Flag field's button is a checkbox under the field's name seeding the other state, every other field's button is labelled by its current value.

        Returns:
            Item key, field names along the path, editing command, button keywords, and seeded form values of each field.
        """
        declared = frozenset(key for key, item, _, _ in interface.walked() if isinstance(item, tuple))

        def shown(names: tuple[str, ...], current: object, command: Operator[FieldEdit], choices: Option[_Choices]) -> Keywords:
            match current:
                case bool() if command is self.toggling:
                    return {"text": bpy.path.display_name(names[-1]), "icon": "CHECKBOX_HLT" if current else "CHECKBOX_DEHLT", "emboss": False}
                case msgspec.Struct():
                    return {"text": MenuItem.of(type(current)).name}
                case _:
                    chosen = choices.bind(lambda found: Block.of_seq(found.values()).filter(lambda pair: pair[1] == current).try_head().map(lambda pair: pair[0]))
                    return {"text": chosen.default_with(lambda: f"{current:g}" if isinstance(current, int | float) else bpy.path.basename(str(current)) if command is self.opening else str(current))}

        return tuple(
            (
                key,
                names,
                command,
                shown(names, value, command, choices),
                {
                    **choices.map(lambda found: {"choices": msgspec.json.encode({identifier: label for identifier, (label, _) in found.items()}).decode()}).default_value(dict[str, str]()),
                    **dict(
                        (("number", float(value)),)
                        if command is self.numbering and isinstance(value, int | float)
                        else (("text", str(value)),)
                        if command is self.writing
                        else (("choice", str(not value)),)
                        if command is self.toggling
                        else ()
                    ),
                },
            )
            for key, (field, value) in _declared(record).items()
            if key not in declared
            for names in (field.names,)
            for choices in (_choices(field.annotation),)
            for command in self.editing(field.annotation, choices).to_list()
        )

    def fielded(self, identity: Identity, row: CompositorEffect, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws one button per edited record field at any depth, each run of nested fields headed by the names along their path.

        Flag draws as a checkbox under its name in the value cell, any other field under its name in the label cell, labelled by its current value: a choice's or case's label, a number, a file's name, or the text.
        """
        held = Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).filter(lambda _: bool(msgspec.structs.fields(row.record)))
        records = tuple((entry, identity.reports.settled(row.read(identity, entry))) for entry in held.to_list())
        edits = tuple((entry, edit) for entry, record in records for found in record.to_list() for edit in self.edited(row.interface, found.record))
        for (entry, (key, names, command, keywords, seeded)), prior in zip(edits, ((), *(edit[1] for _, edit in edits)), strict=False):
            if names[:-1] and names[:-1] != prior[:-1]:
                layout.label(text=" ".join(map(bpy.path.display_name, names[:-1])))
            values = gridded("" if command is self.toggling else bpy.path.display_name(names[-1]), layout)
            values.alignment = "LEFT" if command is self.toggling else "EXPAND"
            placement = Placement(command, {"name": entry.name, "kind": row.kind, "field": key, **seeded})
            identity.reports.faulted(operator_button(values, identity, placement, {**keywords, "translate": False}))

    @cached_property
    def headed(self) -> Mapping[Key, tuple[tuple[Command, Keywords], ...]]:
        """Header commands of each offered effect by record key: its commands selecting an entry by name, then reset, each under its icon alone where it states one, else its title."""
        return MappingProxyType({
            row.kind: tuple(
                (command, command.icon.map(lambda icon: {"text": "", "icon": icon}).default_value(dict[str, str]()))
                for command in (*(held for held in row.registers if isinstance(held, Operator) and issubclass(held.form, Named)), self.rows.resetting)
            )
            for row in self.rows.rows
        })

    def commanded(self, identity: Identity, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the owned entry's header commands."""
        held = tuple((entry, row) for entry in Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).to_list() for row in self.rows.row(identity, entry).to_list())
        for entry, row in held:
            identity.reports.faulted(accumulated(operator_button(layout, identity, Placement(command, {"name": entry.name}), keywords) for command, keywords in self.headed[row.kind]))

    def shown(self, identity: Identity, parts: Mapping[Key, RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the owned entry's body and its curve and ramp editors, logging the conditions an unowned pinned entry misses."""
        match holder:
            case bpy.types.SceneCompositorEffect(node_group=bpy.types.NodeTree() as group) as entry:
                row = self.rows.row(identity, entry)
                for offered in row.to_list():
                    parts[offered.kind](layout, entry, context)
                    node_editors(layout, owned(group, identity).values())
                identity.reports.faulted(accumulated(pinned.checked(context, entry) for pinned in self.rows.pinned if row.is_none() and pinned.ownership.marked(identity, group).is_some()))
            case _:
                pass

    # --- [SECTIONS]

    @cached_property
    def section(self) -> Section:
        """Section over the active entry, open by default and dimmed while the scene renders without compositing."""
        header = Layout(builder=Some(lambda identity: Ok((partial(self.commanded, identity),))), container=Some(lambda layout: layout.row(align=True)))
        body = Layout(builder=Some(lambda identity: self.parts(identity).map(lambda draw: (draw,))))
        return Section(
            key=EDITOR,
            holder=Some(Member(root=bpy.types.Scene, path=("compositor_effects", "active"))),
            condition=Some(composites),
            rows=(*self.leading, body, self.viewers, *self.trailing),
            header=(*self.header, header),
            opened=True,
        )

    # --- [REGISTRATION]

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Offered effects the editor draws, with the scene settings and commands they register."""
        return (self.rows,)

    def acquisitions(self, _identity: Identity) -> Acquisitions:
        """Returns the field commands and the panel beside the stock effect list."""
        return self.toggling, self.choosing, self.numbering, self.writing, self.opening, Panel(section=self.section, at=Beside(SCENE_PT_compositor_effects))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FIELDS]


def _declared(record: msgspec.Struct, /) -> Mapping[Key, tuple[_Field, object]]:
    """Reads current fields and values in preorder, retaining declared union keys and excluding node content."""
    pending: list[tuple[msgspec.Struct, tuple[MemberDescriptorType, ...], tuple[object, ...], Iterator[msgspec.structs.FieldInfo]]] = [(record, (), (), iter(msgspec.structs.fields(type(record))))]
    found: dict[Key, tuple[_Field, object]] = {}
    while pending:
        held, lens, parts, fields = pending[-1]
        if (field := next(fields, None)) is None:
            pending.pop()
            continue
        name, annotation = field.name, field.type
        focused, keyed, value = (*lens, getattr(type(held), name)), (*parts, name), getattr(held, name)
        found[item_key(*keyed)] = _Field(lens=focused, annotation=annotation), value
        if isinstance(value, msgspec.Struct) and not isinstance(value, Content):
            inner = tuple(case for case in _cases(annotation) if not issubclass(case, Content))
            kind = type(value)
            pending.append((value, focused, (*keyed, kind) if len(inner) > 1 else keyed, iter(msgspec.structs.fields(kind))))
    return MappingProxyType(found)


def _cases(annotation: object, /) -> tuple[type[msgspec.Struct], ...]:
    """Record classes a field type holds: its record class, or the cases of its union."""
    match _form(annotation):
        case msgspec.inspect.StructType(cls=kind):
            return (kind,)
        case msgspec.inspect.UnionType(types=types):
            return tuple(member.cls for member in types if isinstance(member, msgspec.inspect.StructType))
        case _:
            return ()


def _form(annotation: object, /) -> msgspec.inspect.Type:
    """Type node of a field type beneath the description metadata wrapping it."""
    match msgspec.inspect.type_info(annotation):
        case msgspec.inspect.Metadata(type=inner):
            return inner
        case info:
            return info


def _choices(annotation: object, /) -> Option[_Choices]:
    """Values a field type offers by item identifier with their labels: the colorspaces Blender reads, the items of a host enum its `Annotated` metadata holds, an enum's members, a flag's two states, a literal's values, or the default record of each case of a union."""
    metadata = inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
    hosted = next((held.prop for held in metadata if isinstance(held, Descriptor) and isinstance(held.prop, bpy.types.EnumProperty)), None)
    match hosted, _form(annotation):
        case bpy.types.EnumProperty(enum_items=items), _:
            return Some({item.identifier: (item.name, item.identifier) for item in items})
        case _, msgspec.inspect.EnumType(cls=kind):
            return Some({member.name: (bpy.path.display_name(member.name.lower()), member) for member in kind})
        case _, msgspec.inspect.BoolType():
            return Some({str(value): (str(value), value) for value in (False, True)})
        case _, msgspec.inspect.LiteralType(values=values):
            return Some({str(held): (str(held), held) for held in values})
        case _, msgspec.inspect.UnionType(types=types) if len(kinds := _cases(annotation)) > 1:
            none = (("None", ("None", None)),) if any(isinstance(member, msgspec.inspect.NoneType) for member in types) else ()
            return Some(dict((*none, *((kind.__name__, (MenuItem.of(kind).name, kind())) for kind in kinds))))
        case _, msgspec.inspect.UnionType(types=(msgspec.inspect.LiteralType(values=values), msgspec.inspect.NoneType())):
            return Some({"None": ("None", None), **{str(held): (str(held), held) for held in values}})
        case _:
            return Nothing


def _numeric(annotation: object, /) -> Option[Callable[[float], Value]]:
    """Conversion of an entered number into a field of an integer or float type."""
    match _form(annotation):
        case msgspec.inspect.IntType():
            return Option[Callable[[float], int]](some=round)
        case msgspec.inspect.FloatType():
            return Some(float)
        case _:
            return Nothing


def _replaced[T: msgspec.Struct](field: _Field, value: object, record: T, /) -> Outcome[T]:
    """Decodes the edited value and rebuilds its immutable ancestors."""
    subject = ".".join((type(record).__name__, *field.names))
    try:
        decoded = msgspec.convert(value, field.annotation)
        rebuilt = amended(object, record, field.lens, lambda _: decoded)
    except (TypeError, ValueError) as error:
        return Error(Fault.of(Reason.INVALID, subject, error))
    return rebuilt.to_result_with(partial(Fault.of, Reason.ABSENT, subject))


# --- [DRAWING]


def node_editors(layout: bpy.types.UILayout, nodes: Iterable[bpy.types.Node], /) -> None:
    """Draws the curve and ramp editors of nodes holding a curve mapping or color ramp, each under its label through the node's own input template."""
    for node in nodes:
        if any(isinstance(prop, bpy.types.PointerProperty) and isinstance(prop.fixed_type, bpy.types.CurveMapping | bpy.types.ColorRamp) for prop in node.bl_rna.properties):
            column = layout.column()
            column.use_property_split = False
            column.label(text=node.label or node.bl_label)
            column.template_node_inputs(node)


def wheels(interface: Interface, reach: Callable[[Key, bpy.types.Context], Option[Struct]], /) -> Layout:
    """Grading wheel of each interface panel holding exactly one stated color socket then one stated float socket, under the panel's title in even columns, over the inputs `reach` finds by socket key, since a stock input declares its class only through a data-block registration cannot create."""

    def member(socket: Socket) -> Member:
        return Member(root=bpy.types.bpy_struct, path=("value",), reach=Some(partial(reach, socket.key)))

    def graded(panel: InterfacePanel) -> Option[Layout]:
        match panel.inputs:
            case (hue, luma) if not panel.panels and hue.stock.is_none() and luma.stock.is_none() and hue.base is bpy.types.NodeSocketColor and luma.base is bpy.types.NodeSocketFloat:
                return Some(Layout(rows=(Label(text=panel.title, function=FUNCTIONS["label"]), wheel(member(hue), Some(member(luma)), _PICKER)), container=Some(lambda layout: layout.column())))
            case _:
                return Nothing

    panels = tuple(item for _, item, _, _ in interface.walked() if isinstance(item, InterfacePanel))
    return Layout(rows=tuple(found for panel in panels for found in graded(panel).to_list()), container=Some(even))


def _reached(identity: Identity, key: Key, context: bpy.types.Context) -> Option[Struct]:
    """Input of the scene's active entry at an item key."""
    return root_in(bpy.types.Scene, context).bind(lambda scene: Option.of_optional(scene.compositor_effects.active)).bind(lambda entry: entry_input(identity, entry, key).to_option())


def _scened(struct: Callable[[bpy.types.Scene], Struct], draw: RowDraw, layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
    """Draws over the attachment group the entry's scene holds."""
    for scene in Option.of_optional(try_downcast(bpy.types.Scene, holder.id_data)).to_list():
        draw(layout, struct(scene), context)


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["EDITOR", "EntryEditor", "FieldEdit", "FilePath", "node_editors", "wheels"]
