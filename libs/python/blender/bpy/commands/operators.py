"""Operators, invocation forms, property placements, and macros."""

from abc import abstractmethod
from collections.abc import Callable, Generator, Iterable
from contextlib import contextmanager
from enum import auto, Enum
from itertools import chain
from pathlib import Path
from typing import override, Self, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import given, Placed, Writer
from blender.bpy.commands.forms import Executed, Opening, Plan, Returns
from blender.bpy.commands.modal import Modal
from blender.bpy.data.rna.conditions import check, Condition
from blender.bpy.data.rna.descriptors import rooted, subclass, written
from blender.bpy.data.rna.snapshots import Applied
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderRegisterFault, BlenderResult, BlenderUnmetFault
from blender.bpy.settings.attachments import preferred
from blender.bpy.settings.properties import RecordGroup
from blender.records.faults import accumulated, FileFault, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Draw = Callable[[bpy.types.UILayout, bpy.types.Context, bpy.types.OperatorProperties, Identity], None]
type Invoke[T: msgspec.Struct, S] = Confirm[T] | Suppressible[T] | Properties | Floating | InvokePopup | Open[T] | Save[T] | Pick | Modal[S]
type Outcome = Done | Unchanged
type Located[T] = Callable[[bpy.types.Operator, T], BlenderResult[tuple[Path, ...]]]


class InvokePopup(Enum):
    """Redo properties popup or enum search popup."""

    REDO = auto()
    SEARCH = auto()

    def planned[T: msgspec.Struct, S](self, plan: Plan[T], body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Opens the redo popup over an undoable command, or the search popup over its primary enum."""
        redone: Opening[T] = lambda operator, _context, manager, event, _record: Ok(manager.invoke_props_popup(operator, event))

        def searched(operator: bpy.types.Operator, _context: bpy.types.Context, manager: bpy.types.WindowManager, _event: bpy.types.Event, _record: T) -> BlenderResult[Returns]:
            manager.invoke_search_popup(operator)
            return Ok({"RUNNING_MODAL"})

        match self, body.command.undo:
            case InvokePopup.REDO, True:
                return Ok(msgspec.structs.replace(plan, opening=redone))
            case InvokePopup.REDO, _:
                return Error((BlenderRegisterFault(request=(body.command.key,), error=ValueError("the redo popup repeats an undo step, so undo must be True")),))
            case InvokePopup.SEARCH, _:
                return Ok(msgspec.structs.replace(plan, opening=searched))


# --- [MODELS] ---------------------------------------------------------------------------


class Empty(msgspec.Struct, frozen=True, kw_only=True):
    """Record of operator without properties."""


class Done(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with changes, report entries, and skipped faults."""

    notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()
    skipped: Option[BlenderFaults] = Nothing

    @classmethod
    def wrote(cls, paths: Iterable[str | Path]) -> Self:
        """Reports each written path."""
        return cls(notes=tuple(("INFO", f"Wrote {path}") for path in paths))


class Unchanged(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with no change, reason reported as INFO."""

    reason: str


class Call[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Body invocation with context, record, and file paths."""

    identity: Identity
    context: bpy.types.Context
    record: T
    paths: tuple[Path, ...] = ()

    def root[K: "bpy.types.bpy_struct[object]"](self, kind: type[K]) -> BlenderResult[K]:
        """Returns required context root."""
        return rooted(kind, self.context)


class Confirm[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Confirmation before execution with computed message."""

    message: Option[Callable[[bpy.types.Context, T], BlenderResult[str]]] = Nothing
    confirm_text: Option[str] = Nothing
    icon: str = bpy.types.WindowManager.bl_rna.functions["invoke_confirm"].parameters["icon"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    title: Option[str] = Nothing
    text_ctxt: Option[str] = Nothing
    translate: bool = True

    def planned[S](self, plan: Plan[T], _body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Opens `invoke_confirm` with the computed message."""
        keywords = given(msgspec.structs.asdict(self))

        opening: Opening[T] = lambda operator, context, manager, event, record: (
            self.message
            .map(lambda compute: compute(context, record).map(lambda text: keywords | {"message": text}))
            .default_value(Ok(keywords))
            .map(lambda chosen: manager.invoke_confirm(operator, event, **chosen))
        )

        return Ok(msgspec.structs.replace(plan, opening=opening))


class Dialog(msgspec.Struct, frozen=True, kw_only=True):
    """Keywords of `invoke_props_dialog`."""

    width: int = bpy.types.WindowManager.bl_rna.functions["invoke_props_dialog"].parameters["width"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    confirm_text: Option[str] = Nothing
    cancel_default: bool = False
    title: Option[str] = Nothing
    text_ctxt: Option[str] = Nothing
    translate: bool = True

    def opened(self, operator: bpy.types.Operator, manager: bpy.types.WindowManager) -> Returns:
        """Opens `invoke_props_dialog` with the dialog keywords."""
        return manager.invoke_props_dialog(operator, **given({key: value for key, value in msgspec.structs.asdict(self).items() if key in Dialog.__struct_fields__}))


class Properties(Dialog, frozen=True, kw_only=True):
    """Editable dialog executing on confirmation."""

    def planned[T: msgspec.Struct, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Opens `invoke_props_dialog`."""
        opening: Opening[T] = lambda operator, _context, manager, _event, _record: Ok(self.opened(operator, manager))
        return Ok(msgspec.structs.replace(plan, opening=opening))


class Suppressible[T: msgspec.Struct](Dialog, frozen=True, kw_only=True):
    """Confirmation dialog with checkbox clearing `ask_<key>` preference on success."""

    message: Callable[[bpy.types.Context, T], str]
    icon: "rna_enums.IconItems" = "NONE"

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Opens the dialog while `ask_<key>` holds, clearing it after a run with the checkbox set."""
        ask = body.command.ask

        def asked(context: bpy.types.Context) -> BlenderResult[bool]:
            return (
                rooted(bpy.types.Preferences, context)
                .map(lambda preferences: Option.of_optional(preferences.addons.get(body.identity.package)).bind(lambda addon: Option.of_optional(addon.preferences)))
                .bind(
                    lambda holder: holder.bind(lambda held: Option.of_optional(held.bl_rna.properties.get(ask)).map(lambda prop: bool(getattr(held, prop.identifier)))).to_result_with(
                        lambda: (BlenderAbsentFault(owner=body.identity.package, key=ask),)
                    )
                )
            )

        def cleared(operator: bpy.types.Operator, context: bpy.types.Context) -> BlenderResult[None]:
            suppressed: bool = operator.suppress  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return (
                rooted(bpy.types.Preferences, context).bind(lambda preferences: preferred(body.identity, preferences, lambda holder: written(holder, ask, False).map(lambda _: None)))
                if suppressed
                else Ok(None)
            )

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return body.finished(operator, body.performed(operator, context).bind(lambda outcome: cleared(operator, context).map(lambda _: outcome)))

        def asking(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(operator.layout).to_list():
                layout.use_property_split = True
                layout.label_multiline(
                    text=body.group.read(operator.properties).map(lambda record: self.message(context, record)).default_with(lambda faults: " ".join(fault.message for fault in faults)), icon=self.icon
                )
                body.draw(operator, context)
                layout.prop(operator, "suppress")

        opening: Opening[T] = lambda operator, context, manager, _event, _record: asked(context).map(lambda shown: self.opened(operator, manager) if shown else execute(operator, context))
        return Ok(
            msgspec.structs.replace(
                plan,
                annotations={**plan.annotations, "suppress": bpy.props.BoolProperty(name="Do not show again", options={"SKIP_SAVE"})},
                members={**plan.members, "draw": asking},
                execute=execute,
                opening=opening,
            )
        )


class Floating(msgspec.Struct, frozen=True, kw_only=True):
    """Property popup running no body."""

    width: int = bpy.types.WindowManager.bl_rna.functions["invoke_popup"].parameters["width"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    auto_keymap: bool = False

    def planned[T: msgspec.Struct, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Opens `invoke_popup`."""
        keywords = msgspec.structs.asdict(self)
        opening: Opening[T] = lambda operator, _context, manager, _event, _record: Ok(manager.invoke_popup(operator, **keywords))
        return Ok(msgspec.structs.replace(plan, opening=opening))


class Open[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Import file browser filtering by record formats."""

    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Bases the class on `ImportHelper`, running the body over the chosen files."""

        def chosen(operator: bpy.types.Operator, _record: T) -> BlenderResult[tuple[Path, ...]]:
            paths = tuple(Path(operator.directory, item.name) for item in operator.files if item.name) or tuple(Path(path) for path in (operator.filepath,) if path)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Ok(paths) if paths else Error((BlenderUnmetFault(requirement=Unopened(title=body.command.title)),))

        execute: Executed = lambda operator, context: body.finished(operator, body.performed(operator, context, Some(chosen)))
        opening: Opening[T] = lambda operator, context, _manager, _event, _record: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=self.confirm_text))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
        return Ok(
            msgspec.structs.replace(
                plan,
                bases=(ImportHelper, bpy.types.Operator),
                annotations={
                    **plan.annotations,
                    "filter_glob": globbed(body, self.extensions),
                    "files": bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"HIDDEN", "SKIP_SAVE"}),
                    "directory": bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"}),
                },
                execute=execute,
                opening=opening,
            )
        )


class Unopened(msgspec.Struct, frozen=True, kw_only=True):
    """File browser call holding no chosen file."""

    title: str

    @property
    def message(self) -> str:
        """Report naming the operator."""
        return f"{self.title} requires a chosen file"


class Save[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Export file browser with optional output destinations."""

    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Bases the class on `ExportHelper`, confirming replacement of existing destinations once per invocation."""

        def located(operator: bpy.types.Operator, record: T) -> BlenderResult[tuple[Path, ...]]:
            path = Path(operator.filepath)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Ok(self.destinations.map(lambda project: project(path, record)).default_value((path,)))

        def existing(operator: bpy.types.Operator, record: T) -> BlenderResult[tuple[Path, ...]]:
            return located(operator, record).bind(
                lambda paths: catch(lambda: tuple(path for path in paths if path.exists()), exception=OSError)().map_error(lambda error: (FileFault(access="read", request=repr(paths), error=error),))
            )

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            confirming: bool = self.destinations.is_some() and operator.options.is_invoke and not operator.options.is_repeat and not operator.replacing  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return (
                (body.group.read(operator.properties).bind(lambda record: existing(operator, record)) if confirming else Ok(()))
                .bind(
                    lambda found: (
                        rooted(bpy.types.WindowManager, context).map(lambda manager: replaced(operator, manager, found))
                        if found
                        else Ok(body.finished(operator, body.performed(operator, context, Some(located))))
                    )
                )
                .default_with(lambda faults: body.noted(operator, rendered(faults), "CANCELLED"))
            )

        def replaced(operator: bpy.types.Operator, manager: bpy.types.WindowManager, found: tuple[Path, ...]) -> Returns:
            operator.replacing = True  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return manager.invoke_confirm(operator, None, title="Replace existing files?", message="\n".join(map(str, found)), confirm_text="Replace")

        def checked(operator: bpy.types.Operator, context: bpy.types.Context) -> bool:
            return any((ExportHelper.check(operator, context), *body.command.check.map(lambda callback: callback(operator, context)).to_list()))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        opening: Opening[T] = lambda operator, context, _manager, event, _record: Ok(ExportHelper.invoke(operator, context, event))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
        destined = {"check_existing": bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"}), "replacing": bpy.props.BoolProperty(options={"HIDDEN", "SKIP_SAVE"})}
        return Ok(
            msgspec.structs.replace(
                plan,
                bases=(ExportHelper, bpy.types.Operator),
                annotations={**plan.annotations, "filter_glob": globbed(body, self.extensions), **(destined if self.destinations.is_some() else {})},
                members={**plan.members, "filename_ext": property(lambda operator: formats(body, self.extensions, operator.properties)[0]), "check": checked},
                execute=execute,
                opening=opening,
            )
        )


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Invocation waiting for pointer location under cursor."""

    cursor: "rna_enums.WindowCursorItems"

    def planned[T: msgspec.Struct, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
        """Declares `DEPENDS_ON_CURSOR` with the pending cursor."""
        return Ok(msgspec.structs.replace(plan, flags=plan.flags | {"DEPENDS_ON_CURSOR"}, members={**plan.members, "bl_cursor_pending": self.cursor}))


class Command(msgspec.Struct, frozen=True, kw_only=True):
    """Command with poll condition and search visibility."""

    key: Key
    title: str
    description: str
    condition: Option[Condition] = Nothing
    icon: Option["rna_enums.IconItems"] = Nothing
    internal: bool = False

    # --- [REGISTRATION]

    @abstractmethod
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds command registrations."""
        ...

    # --- [PLACEMENT]

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns operator name with no-op writer."""
        return identity.name(Name.OPERATOR, self.key).map(lambda name: (name, lambda _: Ok(None)))

    # --- [CLASS_MEMBERS]

    def namespace(self, name: str, flags: frozenset["rna_enums.OperatorTypeFlagItems"]) -> dict[str, object]:
        """Returns class members with poll message of condition faults."""

        def poll(cls: type[bpy.types.Operator], context: bpy.types.Context) -> bool:
            faults = self.condition.bind(lambda condition: check(condition, context).swap().to_option())
            for failures in faults.to_list():
                cls.poll_message_set(lambda: " ".join(fault.message for fault in failures))
            return faults.is_none()

        return {
            "bl_idname": name,
            "bl_label": self.title,
            "bl_description": self.description,
            "bl_options": flags | (frozenset({"INTERNAL"}) if self.internal else frozenset()),
            **({"poll": classmethod(poll)} if self.condition.is_some() else {}),
        }


class Operator[T: msgspec.Struct, S = None](Command, frozen=True, kw_only=True):
    """Typed record and body with invocation form and undo."""

    form: type[T]
    run: Callable[[Call[T]], BlenderResult[Outcome]]
    invoke: Option[Invoke[T, S]] = Nothing
    seed: Option[Callable[[T, bpy.types.Event, Identity], T]] = Nothing
    undo: bool | str = True
    draw: Option[Draw] = Nothing
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]] = Nothing
    presets: bool = False
    primary: Option[msgspec.structs.FieldInfo] = Nothing

    # --- [BINDING]

    @property
    def ask(self) -> str:
        """Preference identifier of suppressible confirmation."""
        return f"ask_{self.key}"

    @property
    def invoked(self) -> bool:
        """Whether placement invokes to open form or seed properties."""
        return self.invoke.is_some() or self.seed.is_some()

    @property
    def flags(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """`REGISTER` with `PRESET` and the undo flag the `undo` declaration names."""
        undo: tuple[rna_enums.OperatorTypeFlagItems, ...] = ("UNDO",) if self.undo is True else ("UNDO_GROUPED",) if isinstance(self.undo, str) else ()
        return frozenset(("REGISTER", *(("PRESET",) if self.presets else ()), *undo))

    def bound(self, identity: Identity) -> BlenderResult[tuple[str, RecordGroup[T, bpy.types.Operator]]]:
        """Returns operator name and class record binding."""

        def held(name: str) -> BlenderResult[tuple[str, RecordGroup[T, bpy.types.Operator]]]:
            module, _, operation = name.partition(".")
            native: BlenderResult[bpy.types.Struct] = catch(lambda: getattr(getattr(bpy.ops, module), operation).get_rna_type(), exception=KeyError)().map_error(
                lambda _: (BlenderAbsentFault(owner="bpy.ops", key=name),)
            )
            return Result[bpy.types.Struct, BlenderFaults].bind(native, lambda rna: subclass(bpy.types.Operator, rna.identifier)).map(lambda kind: (name, RecordGroup(record=self.form, struct=kind)))

        return identity.name(Name.OPERATOR, self.key).bind(held)

    @override
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds property groups, operator class, and the release scopes of its invocation form."""
        return paired(identity.name(Name.OPERATOR, self.key), RecordGroup.bound(identity, f"operator_{self.key}", self.form)).bind(
            lambda pair: self.built(Body(command=self, identity=identity, group=pair[1]), pair[0]).map(
                lambda acquired: (*((Band.GROUPS, nested) for nested in pair[1].nested), *((Band.OPERATORS, item) for item in acquired))
            )
        )

    # --- [OPERATOR_CLASS]

    def built(self, body: "Body[T, S]", name: str) -> BlenderResult[tuple[Acquisition, ...]]:
        """Builds operator class from the plan its invocation form folds, with the form's release scopes."""
        annotations = body.group.struct.__annotations__

        def classed(plan: Plan[T]) -> tuple[Acquisition, ...]:
            members = {**plan.members, "__annotations__": dict(plan.annotations), "execute": plan.execute, **self.namespace(name, plan.flags)}
            return (type(name, plan.bases, members | ({"invoke": body.invoking(plan.opening)} if self.invoked else {})), *plan.lifetime)

        initial = Plan[T](
            flags=self.flags,
            annotations=annotations,
            members={
                **RecordGroup.body(body.group.struct),
                **(
                    {"description": classmethod(lambda _cls, _context, properties: body.described(properties))} if any(prop.function is bpy.props.EnumProperty for prop in annotations.values()) else {}
                ),
                **({"draw": body.draw} if self.draw.is_some() else {}),
                **self.check.map(lambda callback: {"check": callback}).default_value({}),
                **self.primary.map(lambda member: {"bl_property": member.name}).default_value({}),
                **({"bl_undo_group": self.undo} if isinstance(self.undo, str) else {}),
            },
            execute=body.execute,
            opening=lambda operator, context, _manager, _event, _record: Ok(body.execute(operator, context)),
        )
        return self.invoke.map(lambda form: form.planned(initial, body)).default_value(Ok(initial)).map(classed)


class Body[T: msgspec.Struct, S](msgspec.Struct, frozen=True, kw_only=True):
    """Callbacks one operator class composes over its command and record group."""

    command: Operator[T, S]
    identity: Identity
    group: RecordGroup[T]

    # --- [REPORTS]

    @staticmethod
    def noted(operator: bpy.types.Operator, entries: Iterable[tuple["rna_enums.WmReportItems", str]], outcome: "rna_enums.OperatorReturnItems") -> Returns:
        """Reports entries, answering outcome."""
        for severity, text in entries:
            operator.report({severity}, text)
        return {outcome}

    @staticmethod
    def finished(operator: bpy.types.Operator, result: BlenderResult[Outcome]) -> Returns:
        """Reports body result as notes and skipped faults with FINISHED, a reason with CANCELLED, or rendered faults with CANCELLED."""

        def concluded(outcome: Outcome) -> Returns:
            match outcome:
                case Done(notes=notes, skipped=skipped):
                    return Body.noted(operator, (*notes, *(("WARNING", fault.message) for faults in skipped.to_list() for fault in faults)), "FINISHED")
                case Unchanged(reason=reason):
                    return Body.noted(operator, (("INFO", reason),), "CANCELLED")

        return result.map(concluded).default_with(lambda faults: Body.noted(operator, rendered(faults), "CANCELLED"))

    # --- [RUNS]

    def performed(self, operator: bpy.types.Operator, context: bpy.types.Context, located: Option[Located[T]] = Nothing) -> BlenderResult[Outcome]:
        """Runs the command over the record its properties hold and the paths the form locates."""
        return self.group.read(operator.properties).bind(
            lambda record: (
                located
                .map(lambda locate: locate(operator, record))
                .default_value(Ok(()))
                .bind(lambda paths: self.command.run(Call(identity=self.identity, context=context, record=record, paths=paths)))
            )
        )

    @property
    def execute(self) -> Executed:
        """Class `execute` running the command and reporting its result."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return self.finished(operator, self.performed(operator, context))

        return execute

    def settled(self, operator: bpy.types.Operator, context: bpy.types.Context, release: Callable[[bool], BlenderResult[None]]) -> Returns:
        """Runs the command after a modal run, releasing the run kept on Done and rolled back otherwise."""

        def completed(ran: Outcome) -> Returns:
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

    def shown(self, properties: bpy.types.OperatorProperties) -> tuple[bpy.types.Property, ...]:
        """Visible record properties."""
        return tuple(prop for prop in properties.bl_rna.properties if prop.identifier in self.command.form.__struct_fields__ and not prop.is_hidden)

    def described(self, properties: bpy.types.OperatorProperties) -> str:
        """Description of the set enum choice, else the command description."""
        return next(
            (
                item.description
                for prop in self.shown(properties)
                if isinstance(prop, bpy.types.EnumProperty) and not prop.is_enum_flag and properties.is_property_set(prop.identifier)
                for item in prop.enum_items
                if item.identifier == getattr(properties, prop.identifier) and item.description
            ),
            self.command.description,
        )

    @property
    def draw(self) -> Callable[[bpy.types.Operator, bpy.types.Context], None]:
        """Class `draw` drawing the command's layout, else every visible record property."""

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            for layout in Option.of_optional(operator.layout).to_list():
                match self.command.draw.to_optional():
                    case None:
                        for prop in self.shown(operator.properties):
                            layout.prop(operator, prop.identifier)
                    case body:
                        body(layout, context, operator.properties, self.identity)

        return draw

    # --- [INVOKE]

    def invoking(self, opening: Opening[T]) -> Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.Event], Returns]:
        """Returns invoke seeding unset properties, then opening the form."""

        def seeded(properties: bpy.types.OperatorProperties, record: T, fresh: T) -> T:
            return msgspec.structs.replace(fresh, **{name: value for name, value in msgspec.structs.asdict(record).items() if properties.is_property_set(name)})

        def invoke(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
            return (
                self.group
                .read(operator.properties)
                .bind(
                    lambda record: (
                        self.command.seed
                        .map(lambda seed: seeded(operator.properties, record, seed(record, event, self.identity)))
                        .map(lambda fresh: self.group.write(operator.properties, fresh).map(lambda _: fresh))
                        .default_value(Ok(record))
                    )
                )
                .bind(lambda record: rooted(bpy.types.WindowManager, context).bind(lambda manager: opening(operator, context, manager, event, record)))
                .default_with(lambda faults: self.noted(operator, rendered(faults), "CANCELLED"))
            )

        return invoke


class Own[T: msgspec.Struct, S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Placement of owned operator with typed record."""

    command: Operator[T, S]
    values: T

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns operator name and writer of supplied record."""
        return identity.name(Name.OPERATOR, self.command.key).map(lambda name: (name, lambda properties: self.command.bound(identity).bind(lambda pair: pair[1].write(properties, self.values))))


class Macro(Command, frozen=True, kw_only=True):
    """Operator steps run in one undo step and redo entry."""

    steps: tuple[Placed, *tuple[Placed, ...]]

    @override
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds macro class, defining its steps after class registration."""

        def built(name: str) -> tuple[tuple[Band, Acquisition], ...]:
            kind: type[bpy.types.Macro] = type(name, (bpy.types.Macro,), self.namespace(name, frozenset({"REGISTER", "UNDO"})))

            @contextmanager
            def defined() -> Generator[BlenderResult[None]]:
                yield accumulated(step.invocation(identity).bind(lambda pair: pair[1](kind.define(pair[0]).properties)) for step in self.steps).map(lambda _: None)

            return (Band.OPERATORS, kind), (Band.OPERATORS, defined())

        return identity.name(Name.OPERATOR, self.key).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def formats[T: msgspec.Struct, S](body: Body[T, S], extensions: Callable[[T], tuple[str, ...]], properties: bpy.types.OperatorProperties) -> tuple[str, ...]:
    """File extensions of the record the properties hold, a refused read raising inside the host's property callback."""
    match body.group.read(properties):
        case Result(tag="error", error=faults):
            raise RuntimeError(" ".join(fault.message for fault in faults))
        case Result(ok=record):
            return extensions(record)


def globbed[T: msgspec.Struct, S](body: Body[T, S], extensions: Callable[[T], tuple[str, ...]]) -> bpy.props._PropertyDeferred:
    """File browser `filter_glob` property over the record's extensions."""

    def glob(properties: bpy.types.OperatorProperties) -> str:
        return ";".join(f"*{extension}" for extension in formats(body, extensions, properties))

    return bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=glob)


def answered(applied: Applied, reason: str, notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> BlenderResult[Outcome]:
    """Returns Done skipping write faults when a path was written, else write faults, else Unchanged."""
    match applied.written, applied.outcome.swap().to_option():
        case (_, *_), skipped:
            return Ok(Done(notes=notes, skipped=skipped))
        case (), Option(tag="some", some=faults):
            return Error(faults)
        case _:
            return Ok(Unchanged(reason=reason))


def tallied(edits: tuple[BlenderResult[object], ...], notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> BlenderResult[Done]:
    """Returns Done skipping faults when any item changed, else every fault."""
    return accumulated(edits).map(lambda _: Done(notes=notes)).or_else_with(lambda faults: Error(faults) if all(edit.is_error() for edit in edits) else Ok(Done(notes=notes, skipped=Some(faults))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Body",
    "Call",
    "Command",
    "Confirm",
    "Dialog",
    "Done",
    "Draw",
    "Empty",
    "Floating",
    "Invoke",
    "InvokePopup",
    "Located",
    "Macro",
    "Open",
    "Operator",
    "Outcome",
    "Own",
    "Pick",
    "Properties",
    "Save",
    "Suppressible",
    "Unchanged",
    "Unopened",
    "answered",
    "tallied",
]
