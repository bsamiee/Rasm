"""Active compositor entry editor beside the stock effect list: its commands and reset, grading wheels, curve and ramp editors, record fields no socket holds, scene settings, effect rows, and Viewer buttons, with the field commands."""

from collections.abc import Callable, Iterable, Iterator, Mapping
from functools import cache, cached_property, partial, reduce
from types import MappingProxyType
from typing import Annotated, Final

from bl_ui.properties_compositor import SCENE_PT_compositor_effects
import bpy
from expression import Nothing, Ok, Option, Some, try_downcast
from expression.collections import Block
from expression.extra.result import catch
import msgspec
import msgspec.inspect
from typing_inspection.introspection import AnnotationSource, inspect_annotation

from blender.bpy.commands.calls import Placement
from blender.bpy.commands.invocations import Open, Popup
from blender.bpy.commands.operators import Answer, Call, Done, Operator, Unchanged
from blender.bpy.data.rna.descriptors import Descriptor, Value
from blender.bpy.data.rna.mirrors import Callbacks, EnumItem, held_choices, rebound
from blender.bpy.data.rna.paths import root_in, rooted, Struct
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisitions, Declaration
from blender.bpy.extension.reports import alerted
from blender.bpy.nodes.addressing import owned
from blender.bpy.nodes.desired import Content, item_key, MenuItem
from blender.bpy.nodes.interfaces import Interface, InterfacePanel, Socket
from blender.bpy.render.compositor.effects import CompositorEffect, Named
from blender.bpy.render.compositor.entries import EffectRows
from blender.bpy.render.compositor.inputs import entry_input
from blender.bpy.render.compositor.viewers import composites, Viewers
from blender.bpy.settings.attachments import Attachment
from blender.bpy.ui.layout.members import Member
from blender.bpy.ui.layout.rows import even, FUNCTIONS, gridded, Keywords, Label, Layout, pressed, realized, RowContent, RowDraw
from blender.bpy.ui.layout.sections import fields, Section
from blender.bpy.ui.layout.templates import wheel
from blender.bpy.ui.surfaces.panels import Beside, Panel
from blender.imaging.color.processors import ColorSpace
from blender.records.faults import accumulated, Fault, Outcome, paired, Reason
from blender.records.fields import Case

# --- [TYPES] ----------------------------------------------------------------------------

type FilePath = Annotated[str, msgspec.Meta(min_length=1)]
type _Choices = Mapping[str, tuple[str, object]]
type _Field = tuple[tuple[str, ...], tuple[type[msgspec.Struct], ...], object]
type _Edited = tuple[Key, tuple[str, ...], Operator[FieldEdit], Keywords, Mapping[str, float | str]]

# --- [CONSTANTS] ------------------------------------------------------------------------

EDITOR: Final[Key] = "active_effect"
_SPACES: Final = Descriptor.declared(bpy.types.ColorManagedInputColorspaceSettings, ("name",), bpy.types.EnumProperty)
_PICKER: Final[Keywords] = {"value_slider": True, "lock_luminosity": True, "cubic": True}

# --- [MODELS] ---------------------------------------------------------------------------


class FieldEdit(Named):
    """Record field of a named entry and the value a pick, a number, a text, or a file writes it to."""

    kind: bpy.props.StringProperty(name="Effect", description="Record key of the entry's effect", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    field: bpy.props.StringProperty(name="Field", description="Item key of the record field the edit writes, as the effect's interface keys it", options={"HIDDEN"})  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    choice: bpy.props.EnumProperty(name="Value", description="Value the field takes", items=())  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    number: bpy.props.FloatProperty(name="Value", description="Number the field takes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]
    text: bpy.props.StringProperty(name="Value", description="Text the field takes")  # type: ignore[valid-type]  # ty: ignore[invalid-type-form]


# --- [SERVICES] -------------------------------------------------------------------------


class EntryEditor[R: Case](msgspec.Struct, frozen=True, kw_only=True, dict=True):
    """Editor of the scene's active compositor entry beside the stock effect list, `header` rows ahead of the entry's commands and `leading` and `trailing` rows around its body, each over the entry."""

    rows: EffectRows[R]
    header: tuple[RowContent, ...] = ()
    leading: tuple[RowContent, ...] = ()
    trailing: tuple[RowContent, ...] = ()

    # --- [COMMANDS]

    @cached_property
    def form(self) -> type[FieldEdit]:
        """Field edit form, its choices the values the named field's type offers."""
        return rebound(FieldEdit, choice=Callbacks(items=self.offered))

    @cached_property
    def choosing(self) -> Operator[FieldEdit]:
        """Command picking a field value through a search popup."""
        return Operator(form=self.form, run=self.choose_field_value, invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_search_popup))), primary=Some("choice"))

    @cached_property
    def toggling(self) -> Operator[FieldEdit]:
        """Command setting a flag field to the state its button seeds."""
        return Operator(form=self.form, run=self.choose_field_value, named=Some("toggle_field_value"))

    @cached_property
    def numbering(self) -> Operator[FieldEdit]:
        """Command entering a field number in a dialog."""
        return Operator(
            form=self.form,
            run=self.enter_field_number,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(lambda layout, _context, properties, _identity: layout.prop(properties, "number")),
        )

    @cached_property
    def writing(self) -> Operator[FieldEdit]:
        """Command entering field text in a dialog."""
        return Operator(
            form=self.form,
            run=self.enter_field_text,
            invoke=Some(Popup(partial(bpy.types.WindowManager.invoke_props_dialog))),
            draw=Some(lambda layout, _context, properties, _identity: layout.prop(properties, "text")),
        )

    @cached_property
    def opening(self) -> Operator[FieldEdit]:
        """Command choosing a field file in the file browser."""
        return Operator(form=self.form, run=self.open_field_file, invoke=Some(Open(extensions=lambda _: ())))

    @cached_property
    def viewers(self) -> Viewers[R]:
        """Viewer commands and buttons over the offered and pinned entries."""
        return Viewers(rows=self.rows)

    def offered(self, owner: Struct, _context: bpy.types.Context | None) -> tuple[EnumItem, ...]:
        """Items of the values the edited field's type offers, valued by position so the unset pick names the first."""
        edit: FieldEdit = owner  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        rows = self.rows.offering(edit.kind).to_option().bind(lambda row: Option.of_optional(_declared(row.record).get(edit.field))).bind(lambda field: _choices(field[2]))
        return held_choices(tuple((identifier, label, "", "NONE", index) for found in rows.to_list() for index, (identifier, (label, _)) in enumerate(found.items())))

    def choose_field_value(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set the record field to the picked value."""
        chosen = call.properties
        value = self.located(chosen).bind(
            lambda field: _choices(field[2]).bind(lambda found: Option.of_optional(found.get(chosen.choice))).to_result_with(partial(Fault.of, Reason.ABSENT, chosen.field, member=chosen.choice))
        )
        return value.bind(lambda held: self.revised(call, held[1]))

    def enter_field_number(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set the record field to the entered number."""
        chosen = call.properties
        numeric = self.located(chosen).bind(lambda field: _numeric(field[2]).to_result_with(partial(Fault.of, Reason.ABSENT, chosen.kind, member=chosen.field)))
        return numeric.bind(lambda conversion: self.revised(call, conversion(chosen.number)))

    def enter_field_text(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set the record field to the entered text."""
        return self.revised(call, call.properties.text)

    def open_field_file(self, call: Call[FieldEdit]) -> Outcome[Answer]:
        """Set the record field to the chosen file."""
        return (
            Block
            .of_seq(call.paths)
            .try_head()
            .to_result_with(partial(Fault.of, Reason.ABSENT, call.properties.field, member="file"))
            .bind(lambda path: self.revised(call, bpy.path.relpath(str(path))))
        )

    def located(self, edit: FieldEdit, /) -> Outcome[_Field]:
        """Field names along the path, record classes along it, and declared type of the field an edit's item key names in its effect's record."""
        return self.rows.offering(edit.kind).bind(lambda row: Option.of_optional(_declared(row.record).get(edit.field)).to_result_with(partial(Fault.of, Reason.ABSENT, row.kind, member=edit.field)))

    def revised(self, call: Call[FieldEdit], value: object) -> Outcome[Answer]:
        """Writes the value into the named entry's record field, refusing a value outside the field's bounds."""
        chosen = call.properties
        return paired(rooted(bpy.types.Scene, call.context), self.located(chosen)).bind(
            lambda parts: self.rows.found(call.identity, parts[0], chosen.name).bind(
                lambda held: held.row.revised(call.identity, parts[0], held.entry, partial(_replaced, parts[1][0], value)).map(
                    lambda wrote: Done() if wrote else Unchanged(reason=f"{held.entry.name} holds {value}")
                )
            )
        )

    # --- [DRAWING]

    def parts(self, identity: Identity) -> Outcome[RowDraw]:
        """Realizes each offered effect's body once: its grading wheels, record field buttons, registered scene settings, and own rows."""

        def settings[G: bpy.types.PropertyGroup](attachment: Attachment[G]) -> Outcome[RowDraw]:
            return attachment.bound(identity).bind(lambda attached: realized(fields(attached.form, attachment.key), identity).map(lambda draw: partial(_scened, attached.struct, draw)))

        def body(row: CompositorEffect[R]) -> Outcome[tuple[Key, RowDraw]]:
            scened = accumulated(settings(held) for held in row.registers if isinstance(held, Attachment) and held.owner is bpy.types.Scene)
            return scened.bind(
                lambda drawn: realized((wheels(row.interface, partial(_reached, identity)), partial(self.fielded, identity, row), *drawn, *row.rows), identity).map(lambda draw: (row.kind, draw))
            )

        return accumulated(map(body, self.rows.rows)).map(lambda found: partial(self.shown, identity, dict(found)))

    def editing(self, annotation: object, /) -> Option[Operator[FieldEdit]]:
        """Command editing a field of the type: a flag toggle, a pick over choices, a number, a file, required or optional, or text, `Nothing` for a type none edits."""
        return (
            Some(self.toggling)
            .filter(lambda _: isinstance(_form(annotation), msgspec.inspect.BoolType))
            .or_else_with(lambda: _choices(annotation).map(lambda _: self.choosing))
            .or_else_with(lambda: _numeric(annotation).map(lambda _: self.numbering))
            .or_else_with(lambda: Some(self.opening) if annotation in {FilePath, FilePath | None} else Nothing)
            .or_else_with(lambda: Some(self.writing) if isinstance(_form(annotation), msgspec.inspect.StrType) else Nothing)
        )

    def edited(self, interface: Interface, record: msgspec.Struct, /) -> tuple[_Edited, ...]:
        """Reads the edited fields of a record and the records it holds whose item key names no socket of the interface.

        A flag field's button is a checkbox under the field's name seeding the other state, every other field's button is labelled by its current value.

        Returns:
            Item key, field names along the path, editing command, button keywords, and seeded form values of each field.
        """
        declared = frozenset(key for key, item, _, _ in interface.walked() if isinstance(item, Socket))

        def current(names: tuple[str, ...], cases: tuple[type[msgspec.Struct], ...]) -> Option[object]:
            start: Option[object] = Some(record)
            return reduce(lambda held, step: held.filter(lambda value: type(value) is step[1]).map(lambda value: getattr(value, step[0])), zip(names, (type(record), *cases), strict=True), start)

        def shown(names: tuple[str, ...], annotation: object, current: object, command: Operator[FieldEdit]) -> Keywords:
            chosen = _choices(annotation).bind(lambda found: Block.of_seq(found.values()).filter(lambda pair: pair[1] == msgspec.to_builtins(current)).try_head().map(lambda pair: pair[0]))
            match current:
                case bool() if command is self.toggling:
                    return {"text": bpy.path.display_name(names[-1]), "icon": "CHECKBOX_HLT" if current else "CHECKBOX_DEHLT", "emboss": False}
                case msgspec.Struct():
                    return {"text": MenuItem.of(type(current)).name}
                case int() | float():
                    return {"text": chosen.default_value(f"{current:g}")}
                case _:
                    return {"text": chosen.default_with(lambda: bpy.path.basename(str(current)) if command is self.opening else str(current))}

        return tuple(
            (
                key,
                names,
                command,
                shown(names, annotation, value, command),
                {"number": float(value)}
                if command is self.numbering and isinstance(value, int | float)
                else {"text": str(value)}
                if command is self.writing
                else {"choice": "OFF" if value else "ON"}
                if command is self.toggling
                else {},
            )
            for key, (names, cases, annotation) in _declared(type(record)).items()
            if key not in declared
            for value in current(names, cases).to_list()
            for command in self.editing(annotation).to_list()
        )

    def fielded(self, identity: Identity, row: CompositorEffect[R], layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws one button per edited record field at any depth, each run of nested fields headed by the names along their path.

        A flag draws as a checkbox under its name in the value cell, any other field under its name in the label cell, labelled by its current value: a choice's or case's label, a number, a file's name, or the text.
        """
        held = Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).filter(lambda _: bool(msgspec.structs.fields(row.record)))
        records = tuple((entry, row.read(identity, entry)) for entry in held.to_list())
        for _, record in records:
            alerted(layout, record)
        edits = tuple((entry, edit) for entry, record in records for found in record.to_option().to_list() for edit in self.edited(row.interface, found.record))
        for (entry, (key, names, command, keywords, seeded)), prior in zip(edits, ((), *(edit[1] for _, edit in edits)), strict=False):
            if names[:-1] and names[:-1] != prior[:-1]:
                layout.label(text=" ".join(map(bpy.path.display_name, names[:-1])))
            values = gridded("" if command is self.toggling else bpy.path.display_name(names[-1]), layout)
            values.alignment = "LEFT" if command is self.toggling else "EXPAND"
            placement = Placement(command, {"name": entry.name, "kind": row.kind, "field": key, **seeded})
            alerted(values, pressed(values, identity, placement, {**keywords, "translate": False}))

    @cached_property
    def headed(self) -> Mapping[Key, tuple[tuple[Operator[Named], Keywords], ...]]:
        """Header commands of each offered effect by record key: its commands selecting an entry by name, then reset, each under its icon alone where it states one, else its title."""
        return MappingProxyType({
            row.kind: tuple(
                (command, command.icon.map(lambda icon: {"text": "", "icon": icon}).default_value({}))
                for command in (*(held for held in row.registers if isinstance(held, Operator) and issubclass(held.form, Named)), self.rows.resetting)
            )
            for row in self.rows.rows
        })

    def commanded(self, identity: Identity, layout: bpy.types.UILayout, holder: Struct, _context: bpy.types.Context) -> None:
        """Draws the owned entry's header commands."""
        held = tuple((entry, row) for entry in Option.of_optional(try_downcast(bpy.types.SceneCompositorEffect, holder)).to_list() for row in self.rows.row(identity, entry).to_list())
        for entry, row in held:
            alerted(layout, accumulated(pressed(layout, identity, Placement(command, {"name": entry.name}), keywords) for command, keywords in self.headed[row.kind]))

    def shown(self, identity: Identity, parts: Mapping[Key, RowDraw], layout: bpy.types.UILayout, holder: Struct, context: bpy.types.Context) -> None:
        """Draws the owned entry's body and its curve and ramp editors, or the conditions an unowned pinned entry misses."""
        match holder:
            case bpy.types.SceneCompositorEffect(node_group=bpy.types.NodeTree() as group) as entry:
                row = self.rows.row(identity, entry)
                for offered in row.to_list():
                    parts[offered.kind](layout, entry, context)
                    node_editors(layout, owned(group, identity).values())
                alerted(layout, accumulated(pinned.checked(context, entry) for pinned in self.rows.pinned if row.is_none() and pinned.ownership.marked(identity, group).is_some()).map(lambda _: None))
            case _:
                pass

    # --- [SECTION]

    @cached_property
    def section(self) -> Section:
        """Section over the active entry, open by default and dimmed while the scene renders without compositing."""
        header = Layout(rows=lambda identity: Ok((partial(self.commanded, identity),)), container=Some(lambda layout: layout.row(align=True)))
        body = Layout(rows=lambda identity: self.parts(identity).map(lambda draw: (draw,)))
        return Section(
            key=EDITOR,
            holder=Some(Member(root=bpy.types.Scene, path=("compositor_effects", "active"))),
            condition=Some(composites),
            rows=(*self.leading, body, self.viewers, *self.trailing),
            header=(*self.header, header),
            opened=True,
        )

    @property
    def prerequisites(self) -> tuple[Declaration, ...]:
        """Offered effects the editor draws, with the scene settings and commands they register."""
        return (self.rows,)

    def acquisitions(self, _identity: Identity) -> Outcome[Acquisitions]:
        """Returns the field commands and the panel beside the stock effect list."""
        return Ok((self.toggling, self.choosing, self.numbering, self.writing, self.opening, Panel(section=self.section, at=Beside(SCENE_PT_compositor_effects))))


# --- [OPERATIONS] -----------------------------------------------------------------------

# --- [FIELDS]


@cache
def _declared(kind: type[msgspec.Struct], /) -> Mapping[Key, _Field]:
    """Reads each field a record class and the record classes its fields hold declare, a union field's cases each keying their fields by the case class as `InterfacePanel.of` keys the sockets of a record.

    Returns:
        Field names along the path, the record class each value along it holds, and declared type of each field by item key, node content left to the node editors drawing it.
    """

    def walked(held: type[msgspec.Struct], names: tuple[str, ...], cases: tuple[type[msgspec.Struct], ...], parts: tuple[object, ...]) -> Iterator[tuple[Key, _Field]]:
        for field in msgspec.structs.fields(held):
            at, keyed, inner = (*names, field.name), (*parts, field.name), tuple(case for case in _cases(field.type) if not issubclass(case, Content))
            yield item_key(*keyed), (at, cases, field.type)
            for case in inner:
                yield from walked(case, at, (*cases, case), (*keyed, case) if len(inner) > 1 else keyed)

    return MappingProxyType(dict(walked(kind, (), (), ())))


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
    """Values a field type offers by item identifier with their labels: the colorspaces Blender reads, the items of a host enum its `Annotated` metadata carries, an enum's members, a flag's two states, a literal's values, or the default record of each case of a union."""
    carried = inspect_annotation(annotation, annotation_source=AnnotationSource.ANY, unpack_type_aliases="eager").metadata
    hosted = _SPACES.prop if annotation is ColorSpace else next((held.prop for held in carried if isinstance(held, Descriptor) and isinstance(held.prop, bpy.types.EnumProperty)), None)
    match hosted, _form(annotation):
        case bpy.types.EnumProperty(enum_items=items), _:
            return Some({item.identifier: (item.name, item.identifier) for item in items})
        case _, msgspec.inspect.EnumType(cls=kind):
            return Some({member.name: (bpy.path.display_name(member.name.lower()), msgspec.to_builtins(member)) for member in kind})
        case _, msgspec.inspect.BoolType():
            return Some({"OFF": ("Off", False), "ON": ("On", True)})
        case _, msgspec.inspect.LiteralType(values=values):
            return Some({str(held): (str(held), held) for held in values})
        case _, msgspec.inspect.UnionType() if len(kinds := _cases(annotation)) > 1:
            return Some({kind.__name__: (MenuItem.of(kind).name, msgspec.to_builtins(kind())) for kind in kinds})
        case _, msgspec.inspect.UnionType(types=(msgspec.inspect.LiteralType(values=values), msgspec.inspect.NoneType())):
            return Some({"None": ("None", None), **{str(held): (str(held), held) for held in values}})
        case _:
            return Nothing


def _numeric(annotation: object, /) -> Option[Callable[[float], Value]]:
    """Conversion of an entered number into a field of an integer or float type."""
    match _form(annotation):
        case msgspec.inspect.IntType():
            return Some(round)
        case msgspec.inspect.FloatType():
            return Some(float)
        case _:
            return Nothing


def _replaced[T: msgspec.Struct](names: tuple[str, ...], value: object, record: T, /) -> Outcome[T]:
    """Record with the field at a path of field names replaced, validated against the bounds of every record it holds."""

    def placed(tree: object, names: tuple[str, ...]) -> object:
        match names, tree:
            case (name, *rest), Mapping():
                return {**tree, name: placed(tree.get(name), tuple(rest))}
            case _:
                return value

    return _rebuilt(record, placed(msgspec.to_builtins(record), names)).map_error(partial(Fault.of, Reason.INVALID, ".".join((type(record).__name__, *names))))


@catch(exception=msgspec.ValidationError)
def _rebuilt[T: msgspec.Struct](record: T, values: Mapping[str, object], /) -> T:
    """Converts builtin values back into the record's type."""
    rebuilt: T = msgspec.convert(values, type(record))
    return rebuilt


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
    """Grading wheel of each interface panel holding exactly one color socket then one float socket, under the panel's title in even columns, over the inputs `reach` finds by socket key."""

    def member(socket: Socket) -> Member:
        return Member(root=bpy.types.bpy_struct, path=("value",), reach=Some(partial(reach, socket.key)))

    def graded(panel: InterfacePanel) -> Option[Layout]:
        match panel.inputs:
            case (Socket(kind=type() as color) as hue, Socket(kind=type() as level) as luma) if (
                not panel.panels and issubclass(color, bpy.types.NodeSocketColor) and issubclass(level, bpy.types.NodeSocketFloat)
            ):
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
