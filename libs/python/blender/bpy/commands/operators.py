"""Native operators, invocation forms, property placements, and macros."""

from abc import abstractmethod
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager
from enum import auto, Enum
from itertools import chain
from pathlib import Path
from typing import override, Protocol, Self, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import Operation
from blender.bpy.commands.modal import Modal
from blender.bpy.data.rna.conditions import check, Condition
from blender.bpy.data.rna.descriptors import Descriptor, rooted, Value, written
from blender.bpy.data.rna.snapshots import Applied
from blender.bpy.extension.identity import Identity, Key, Name
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderFaults, BlenderReadFault, BlenderResult
from blender.bpy.settings.attachments import preferred
from blender.bpy.settings.properties import RecordGroup
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Draw = Callable[[bpy.types.UILayout, bpy.types.Context, bpy.types.OperatorProperties], None]
type Writer = Callable[[bpy.types.OperatorProperties], BlenderResult[None]]
type Invoke[T: msgspec.Struct, S] = Confirm[T] | Suppressible[T] | Properties | Floating | InvokePopup | Open[T] | Save[T] | Pick | Modal[S]
type Outcome = Done | Unchanged


class Placed(Protocol):
    """Operator identity and the writer filling its placement properties."""

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns the native identifier and writer or the placement's failures."""
        ...


class InvokePopup(Enum):
    """Native property redo or enum search popup."""

    REDO = auto()
    SEARCH = auto()


# --- [MODELS] ---------------------------------------------------------------------------


class Empty(msgspec.Struct, frozen=True, kw_only=True):
    """Record of an operator without properties."""


class Done(msgspec.Struct, frozen=True, kw_only=True):
    """Body that wrote at least one change, its report entries, and independent failures skipped."""

    notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()
    skipped: Option[BlenderFaults] = Nothing

    @classmethod
    def wrote(cls, paths: Iterable[str | Path]) -> Self:
        """Reports each written path."""
        return cls(notes=tuple(("INFO", f"Wrote {path}") for path in paths))


class Unchanged(msgspec.Struct, frozen=True, kw_only=True):
    """Body whose work set held nothing to change, with the reason reported to the user."""

    reason: str


class Call[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Body invocation with its native context, record, and files."""

    identity: Identity
    context: bpy.types.Context
    record: T
    paths: tuple[Path, ...] = ()

    def root[K: "bpy.types.bpy_struct[object]"](self, kind: type[K]) -> BlenderResult[K]:
        """Returns a required native context root or its typed absence."""
        return rooted(kind, self.context)


class Confirm[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Confirmation before execution, with optional computed text and button label."""

    message: Option[Callable[[bpy.types.Context, T], BlenderResult[str]]] = Nothing
    confirm_text: Option[str] = Nothing
    icon: str = bpy.types.WindowManager.bl_rna.functions["invoke_confirm"].parameters["icon"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    title: Option[str] = Nothing
    text_ctxt: Option[str] = Nothing
    translate: bool = True


class Properties(msgspec.Struct, frozen=True, kw_only=True):
    """Editable dialog executing on confirmation."""

    width: int = bpy.types.WindowManager.bl_rna.functions["invoke_props_dialog"].parameters["width"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    confirm_text: Option[str] = Nothing
    cancel_default: bool = False
    title: Option[str] = Nothing
    text_ctxt: Option[str] = Nothing
    translate: bool = True


class Suppressible[T: msgspec.Struct](Properties, frozen=True, kw_only=True):
    """Confirmation whose successful execution can disable its preference ask."""

    message: Callable[[bpy.types.Context, T], str]
    icon: "rna_enums.IconItems" = "NONE"


class Floating(msgspec.Struct, frozen=True, kw_only=True):
    """Property popup that does not execute the body."""

    width: int = bpy.types.WindowManager.bl_rna.functions["invoke_popup"].parameters["width"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    auto_keymap: bool = False


class Open[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Native import browser choosing files of the record's current formats."""

    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""


class Save[T: msgspec.Struct](msgspec.Struct, frozen=True, kw_only=True):
    """Native export browser with optional final output destinations."""

    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Native pending invocation waiting for a pointer location under its cursor."""

    cursor: "rna_enums.WindowCursorItems"


class Command(msgspec.Struct, frozen=True, kw_only=True):
    """Action's identity, native text, availability, icon, and search visibility."""

    key: Key
    title: str
    description: str
    condition: Option[Condition] = Nothing
    icon: Option["rna_enums.IconItems"] = Nothing
    internal: bool = False

    # --- [REGISTRATION]

    @abstractmethod
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds native registrations for this command under its extension identity."""
        ...

    # --- [PLACEMENT]

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns the command identity without setting placement properties."""
        return identity.name(Name.OPERATOR, self.key).map(lambda name: (name, lambda _: Ok(None)))

    # --- [CLASS_MEMBERS]

    def namespace(self, name: str, flags: frozenset["rna_enums.OperatorTypeFlagItems"]) -> dict[str, object]:
        """Returns native class members and a deferred typed poll failure message."""

        def poll(cls: type[bpy.types.Operator], context: bpy.types.Context) -> bool:
            faults = self.condition.bind(lambda condition: check(condition, context).swap().to_option())
            for failures in faults.to_list():
                cls.poll_message_set(lambda: " ".join(text for _, text in rendered(failures)))
            return faults.is_none()

        return {
            "bl_idname": name,
            "bl_label": self.title,
            "bl_description": self.description,
            "bl_options": flags | (frozenset({"INTERNAL"}) if self.internal else frozenset()),
            **({"poll": classmethod(poll)} if self.condition.is_some() else {}),
        }


class Operator[T: msgspec.Struct, S = None](Command, frozen=True, kw_only=True):
    """Typed record and body registered with its native invocation and undo behavior."""

    form: type[T]
    run: Callable[[Call[T]], BlenderResult[Outcome]]
    invoke: Option[Invoke[T, S]] = Nothing
    seed: Option[Callable[[T, bpy.types.Event], T]] = Nothing
    undo: bool | str = True
    draw: Option[Draw] = Nothing
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]] = Nothing
    presets: bool = False
    primary: Option[msgspec.structs.FieldInfo] = Nothing

    # --- [BINDING]

    @property
    def ask(self) -> str:
        """Preference identifier for this action's suppressible confirmation."""
        return f"ask_{self.key}"

    @property
    def invoked(self) -> bool:
        """Whether a placement needs invocation to open its form or seed properties."""
        return self.invoke.is_some() or self.seed.is_some()

    def bound(self, identity: Identity) -> BlenderResult[tuple[str, RecordGroup[T, bpy.types.Operator]]]:
        """Returns the registered operator and its original acquired record binding."""

        def held(name: str) -> BlenderResult[tuple[str, RecordGroup[T, bpy.types.Operator]]]:
            module, _, operation = name.partition(".")
            native: BlenderResult[bpy.types.Struct] = catch(lambda: getattr(getattr(bpy.ops, module), operation).get_rna_type(), exception=KeyError)().map_error(
                lambda _: (BlenderAbsentFault(owner="bpy.ops", key=name),)
            )
            return native.bind(
                lambda rna: (
                    Option
                    .of_optional(bpy.types.Operator.bl_rna_get_subclass_py(rna.identifier))
                    .map(lambda kind: (name, RecordGroup(record=self.form, struct=kind)))
                    .to_result_with(lambda: (BlenderAbsentFault(owner="bpy.types.Operator", key=rna.identifier),))
                )
            )

        return identity.name(Name.OPERATOR, self.key).bind(held)

    @override
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds property groups, the operator class, and its modal release scope."""
        return paired(identity.name(Name.OPERATOR, self.key), RecordGroup.bound(identity, f"operator_{self.key}", self.form)).map(
            lambda pair: (*((Band.GROUPS, nested) for nested in pair[1].nested), *((Band.OPERATORS, item) for item in self.built(identity, *pair)))
        )

    # --- [NATIVE_CLASS]

    def built(self, identity: Identity, name: str, group: RecordGroup[T]) -> tuple[Acquisition, ...]:
        """Builds the native callbacks and class additions of one invocation form."""
        made = group.struct.__annotations__
        flags: frozenset[rna_enums.OperatorTypeFlagItems] = frozenset({"REGISTER", "PRESET"} if self.presets else {"REGISTER"})
        match self.undo:
            case True:
                flags |= frozenset["rna_enums.OperatorTypeFlagItems"]({"UNDO"})
                grouping: Option[str] = Nothing
            case False:
                grouping = Nothing
            case str(label):
                flags |= frozenset["rna_enums.OperatorTypeFlagItems"]({"UNDO_GROUPED"})
                grouping = Some(label)

        def noted(operator: bpy.types.Operator, entries: Iterable[tuple["rna_enums.WmReportItems", str]], outcome: "rna_enums.OperatorReturnItems") -> set["rna_enums.OperatorReturnItems"]:
            for severity, text in entries:
                operator.report({severity}, text)
            return {outcome}

        def finished(operator: bpy.types.Operator, result: BlenderResult[Outcome]) -> set["rna_enums.OperatorReturnItems"]:
            def concluded(outcome: Outcome) -> set["rna_enums.OperatorReturnItems"]:
                match outcome:
                    case Done(notes=notes, skipped=skipped):
                        return noted(operator, (*notes, *(("WARNING", text) for faults in skipped.to_list() for _, text in rendered(faults))), "FINISHED")
                    case Unchanged(reason=reason):
                        return noted(operator, (("INFO", reason),), "CANCELLED")

            return result.map(concluded).default_with(lambda faults: noted(operator, rendered(faults), "CANCELLED"))

        def prepared(operator: bpy.types.Operator) -> BlenderResult[tuple[T, tuple[Path, ...]]]:
            def projected(record: T) -> tuple[T, tuple[Path, ...]]:
                match self.invoke.to_optional():
                    case Open():
                        paths = tuple(Path(operator.directory, item.name) for item in operator.files) or (Path(operator.filepath),)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                    case Save(destinations=destinations):
                        path = Path(operator.filepath)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                        paths = destinations.map(lambda project: project(path, record)).default_value((path,))
                    case _:
                        paths = ()
                return record, paths

            return binding.read(operator.properties).map(projected)

        def performed(operator: bpy.types.Operator, context: bpy.types.Context, record: T, paths: tuple[Path, ...]) -> BlenderResult[Outcome]:
            result = self.run(Call(identity=identity, context=context, record=record, paths=paths))
            if isinstance(self.invoke.to_optional(), Suppressible) and operator.suppress:  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
                result = result.bind(lambda outcome: preferred(identity, context.preferences, lambda holder: written(holder, self.ask, False).map(lambda _: None)).map(lambda _: outcome))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]
            return result

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
            return finished(operator, prepared(operator).bind(lambda given: performed(operator, context, *given)))

        def shown(properties: bpy.types.OperatorProperties) -> tuple[bpy.types.Property, ...]:
            return tuple(prop for prop in properties.bl_rna.properties if prop.identifier in made and not prop.is_hidden)

        def described(_cls: type[bpy.types.Operator], _context: bpy.types.Context, properties: bpy.types.OperatorProperties) -> str:
            return ". ".join((
                self.description,
                *(
                    f"{prop.name}: {descriptor.shown(descriptor.read(properties))}"
                    for prop in shown(properties)
                    if properties.is_property_set(prop.identifier) and not isinstance(prop, (bpy.types.PointerProperty, bpy.types.CollectionProperty))
                    for descriptor in (Descriptor(root=type(properties), owner="", prop=prop),)
                ),
            ))

        def draw(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            for body in self.draw.to_list():
                for layout in Option.of_optional(operator.layout).to_list():
                    body(layout, context, operator.properties)

        members: dict[str, object] = {
            **RecordGroup.body(group.struct),
            "execute": execute,
            **({"description": classmethod(described)} if made else {}),
            **({"draw": draw} if self.draw.is_some() else {}),
            **self.check.map(lambda callback: {"check": callback}).default_value({}),
            **self.primary.map(lambda member: {"bl_property": member.name}).default_value({}),
            **grouping.map(lambda label: {"bl_undo_group": label}).default_value({}),
        }
        bases: tuple[type, ...] = (bpy.types.Operator,)
        lifetime: tuple[Acquisition, ...] = ()
        opening: Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.WindowManager, bpy.types.Event, T], BlenderResult[set[rna_enums.OperatorReturnItems]]] = (
            lambda operator, context, _manager, _event, _record: Ok(execute(operator, context))
        )

        match self.invoke.to_optional():
            case Pick(cursor=cursor):
                flags |= frozenset["rna_enums.OperatorTypeFlagItems"]({"DEPENDS_ON_CURSOR"})
                members["bl_cursor_pending"] = cursor
            case Confirm(message=message) as confirmation:

                def confirming(
                    operator: bpy.types.Operator, context: bpy.types.Context, manager: bpy.types.WindowManager, event: bpy.types.Event, record: T
                ) -> BlenderResult[set["rna_enums.OperatorReturnItems"]]:
                    return (
                        message
                        .map(lambda body: body(context, record).map(Some))
                        .default_value(Ok(Nothing))
                        .map(
                            lambda text: manager.invoke_confirm(
                                operator,
                                event,
                                **{
                                    key: value
                                    for key, held in (msgspec.structs.asdict(confirmation) | {"message": text}).items()
                                    for value in (held.to_list() if isinstance(held, Option) else (held,))
                                },
                            )
                        )
                    )

                opening = confirming
            case Suppressible() as suppressed:
                members["__annotations__"] = made | {"suppress": bpy.props.BoolProperty(name="Don't ask again", options={"SKIP_SAVE"})}

                def asking(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
                    for layout in Option.of_optional(operator.layout).to_list():
                        layout.use_property_split = True
                        layout.label_multiline(
                            text=binding.read(operator.properties).map(lambda record: suppressed.message(context, record)).default_with(lambda faults: " ".join(text for _, text in rendered(faults))),
                            icon=suppressed.icon,
                        )
                        draw(operator, context)
                        for prop in () if self.draw.is_some() else shown(operator.properties):
                            layout.prop(operator, prop.identifier)
                        layout.prop(operator, "suppress")

                def asked(
                    operator: bpy.types.Operator, context: bpy.types.Context, manager: bpy.types.WindowManager, _event: bpy.types.Event, _record: T
                ) -> BlenderResult[set["rna_enums.OperatorReturnItems"]]:
                    return Ok(
                        manager.invoke_props_dialog(
                            operator,
                            **{
                                key: value
                                for key, held in msgspec.structs.asdict(suppressed).items()
                                if key in Properties.__struct_fields__
                                for value in (held.to_list() if isinstance(held, Option) else (held,))
                            },
                        )
                        if getattr(context.preferences.addons[identity.package].preferences, self.ask)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                        else execute(operator, context)
                    )

                members["draw"] = asking
                opening = asked
            case Properties() as dialog:
                opening = lambda operator, _context, manager, _event, _record: Ok(
                    manager.invoke_props_dialog(operator, **{key: value for key, held in msgspec.structs.asdict(dialog).items() for value in (held.to_list() if isinstance(held, Option) else (held,))})
                )
            case Floating() as floating:
                opening = lambda operator, _context, manager, _event, _record: Ok(manager.invoke_popup(operator, **msgspec.structs.asdict(floating)))
            case InvokePopup.REDO:
                flags |= frozenset["rna_enums.OperatorTypeFlagItems"]({"UNDO"})
                opening = lambda operator, _context, manager, event, _record: Ok(manager.invoke_props_popup(operator, event))
            case InvokePopup.SEARCH:

                def searched(
                    operator: bpy.types.Operator, _context: bpy.types.Context, manager: bpy.types.WindowManager, _event: bpy.types.Event, _record: T
                ) -> BlenderResult[set["rna_enums.OperatorReturnItems"]]:
                    manager.invoke_search_popup(operator)
                    return Ok({"RUNNING_MODAL"})

                opening = searched
            case Open(extensions=extensions) | Save(extensions=extensions) as browser:

                def formats(properties: bpy.types.OperatorProperties) -> tuple[str, ...]:
                    match binding.read(properties):
                        case Result(tag="error", error=faults):
                            raise RuntimeError(" ".join(text for _, text in rendered(faults)))
                        case Result(ok=record):
                            return extensions(record)

                def filter_glob(properties: bpy.types.OperatorProperties) -> str:
                    return ";".join(f"*{extension}" for extension in formats(properties))

                annotations = made | {"filter_glob": bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=filter_glob)}
                if isinstance(browser, Open):
                    bases = (ImportHelper, bpy.types.Operator)
                    annotations |= {
                        "files": bpy.props.CollectionProperty(type=bpy.types.OperatorFileListElement, options={"HIDDEN", "SKIP_SAVE"}),
                        "directory": bpy.props.StringProperty(subtype="DIR_PATH", options={"HIDDEN", "SKIP_SAVE"}),
                    }
                    opening = lambda operator, context, _manager, _event, _record: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=browser.confirm_text))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
                else:
                    bases = (ExportHelper, bpy.types.Operator)
                    members["filename_ext"] = property(lambda operator: formats(operator.properties)[0])
                    members["check"] = lambda operator, context: any((ExportHelper.check(operator, context), *self.check.map(lambda callback: callback(operator, context)).to_list()))
                    opening = lambda operator, context, _manager, event, _record: Ok(ExportHelper.invoke(operator, context, event))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
                    if browser.destinations.is_some():
                        annotations["check_existing"] = bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"})

                        def saving(operator: bpy.types.Operator, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
                            vars(operator)["continuation"] = execute
                            if not operator.options.is_invoke or operator.options.is_repeat:
                                return execute(operator, context)

                            def confirming(given: tuple[T, tuple[Path, ...]]) -> BlenderResult[set["rna_enums.OperatorReturnItems"]]:
                                record, paths = given

                                def accepted(active: bpy.types.Operator, held: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
                                    vars(active)["continuation"] = execute
                                    return finished(active, self.run(Call(identity=identity, context=held, record=record, paths=paths)))

                                vars(operator)["continuation"] = accepted
                                collisions = catch(lambda: tuple(destination for destination in paths if destination.exists()), exception=OSError)().map_error(
                                    lambda error: (BlenderReadFault(request=repr(paths), error=error),)
                                )
                                return rooted(bpy.types.WindowManager, context).map2(
                                    collisions,
                                    lambda manager, existing: (
                                        manager.invoke_confirm(operator, None, title="Replace existing files?", message="\n".join(str(destination) for destination in existing), confirm_text="Replace")
                                        if existing
                                        else accepted(operator, context)
                                    ),
                                )

                            return prepared(operator).bind(confirming).default_with(lambda faults: noted(operator, rendered(faults), "CANCELLED"))

                        def initialize(operator: bpy.types.Operator, *args: object, **kwargs: object) -> None:
                            super(kind, operator).__init__(*args, **kwargs)
                            vars(operator)["continuation"] = saving

                        def resumed(operator: bpy.types.Operator, context: bpy.types.Context) -> set["rna_enums.OperatorReturnItems"]:
                            continuation: Callable[[bpy.types.Operator, bpy.types.Context], set[rna_enums.OperatorReturnItems]] = vars(operator)["continuation"]
                            return continuation(operator, context)

                        members |= {"__init__": initialize, "execute": resumed}
                members["__annotations__"] = annotations
            case Modal() as interactive:
                active: set[bpy.types.Operator] = set()
                flags |= interactive.options

                def cancelled(operator: bpy.types.Operator, context: bpy.types.Context, entries: tuple[tuple["rna_enums.WmReportItems", str], ...]) -> set["rna_enums.OperatorReturnItems"]:
                    active.discard(operator)
                    return noted(operator, (*entries, *interactive.ended(operator, context, cancelled=True).map(lambda _: ()).default_with(rendered)), "CANCELLED")

                def stepped(operator: bpy.types.Operator, context: bpy.types.Context, result: BlenderResult["rna_enums.OperatorReturnItems"]) -> set["rna_enums.OperatorReturnItems"]:
                    def refused(faults: BlenderFaults) -> set["rna_enums.OperatorReturnItems"]:
                        return cancelled(operator, context, rendered(faults))

                    def returned(outcome: "rna_enums.OperatorReturnItems") -> set["rna_enums.OperatorReturnItems"]:
                        match outcome:
                            case "FINISHED":

                                def completed(ran: Outcome) -> set["rna_enums.OperatorReturnItems"]:
                                    active.discard(operator)
                                    match ran:
                                        case Done(skipped=skipped) as done:
                                            released = interactive.ended(operator, context, cancelled=False)
                                            return finished(
                                                operator,
                                                Ok(
                                                    released.map(lambda _: done).default_with(
                                                        lambda faults: msgspec.structs.replace(done, skipped=Some((*chain.from_iterable(skipped.to_list()), *faults)))
                                                    )
                                                ),
                                            )
                                        case Unchanged():
                                            return finished(operator, interactive.ended(operator, context, cancelled=True).map(lambda _: ran))

                                return prepared(operator).bind(lambda given: performed(operator, context, *given)).map(completed).default_with(refused)
                            case "CANCELLED":
                                return cancelled(operator, context, ())
                            case _:
                                return {outcome}

                    return result.map(returned).default_with(refused)

                def began(
                    operator: bpy.types.Operator, context: bpy.types.Context, manager: bpy.types.WindowManager, event: bpy.types.Event, _record: T
                ) -> BlenderResult[set["rna_enums.OperatorReturnItems"]]:
                    def running(outcome: "rna_enums.OperatorReturnItems") -> set["rna_enums.OperatorReturnItems"]:
                        active.add(operator)
                        manager.modal_handler_add(operator)
                        return {outcome}

                    return interactive.began(operator, context, event).map(running)

                def cancel(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
                    cancelled(operator, context, ())

                opening = began
                members |= {"modal": lambda operator, context, event: stepped(operator, context, interactive.handled(operator, context, event)), "cancel": cancel}

                @contextmanager
                def acquired() -> Generator[None]:
                    try:
                        yield
                    finally:
                        for operator in tuple(active):
                            cancelled(operator, bpy.context, ())

                lifetime = (acquired(),)
            case None:
                pass

        def seeded(properties: bpy.types.OperatorProperties, record: T, fresh: T) -> T:
            return msgspec.structs.replace(fresh, **{name: value for name, value in msgspec.structs.asdict(record).items() if properties.is_property_set(name)})

        def invoke(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> set["rna_enums.OperatorReturnItems"]:
            return (
                binding
                .read(operator.properties)
                .bind(
                    lambda record: (
                        self.seed
                        .map(lambda seed: seeded(operator.properties, record, seed(record, event)))
                        .map(lambda fresh: binding.write(operator.properties, fresh).map(lambda _: fresh))
                        .default_value(Ok(record))
                    )
                )
                .bind(lambda record: rooted(bpy.types.WindowManager, context).bind(lambda manager: opening(operator, context, manager, event, record)))
                .default_with(lambda faults: noted(operator, rendered(faults), "CANCELLED"))
            )

        members |= self.namespace(name, flags)
        if self.invoked:
            members["invoke"] = invoke
        kind: type[bpy.types.Operator] = type(name, bases, members)
        binding = RecordGroup(record=self.form, struct=kind)
        return (kind, *lifetime)


class Own[T: msgspec.Struct, S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Placement of an operator with its complete typed property record."""

    command: Operator[T, S]
    values: T

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns the operator identity and writer of its supplied record."""
        return identity.name(Name.OPERATOR, self.command.key).map(lambda name: (name, lambda properties: self.command.bound(identity).bind(lambda pair: pair[1].write(properties, self.values))))


class Acquired(msgspec.Struct, frozen=True, kw_only=True):
    """Placement by the key of an operator built during registration."""

    key: Key

    def invocation(self, identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Returns the extension identifier without setting placement properties."""
        return identity.name(Name.OPERATOR, self.key).map(lambda name: (name, lambda _: Ok(None)))


class Stock(msgspec.Struct, frozen=True, kw_only=True):
    """Placement of an existing operator with native RNA property values."""

    operator: Operation
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)

    def invocation(self, _identity: Identity, /) -> BlenderResult[tuple[str, Writer]]:
        """Resolves native operator properties and returns their write boundary."""
        idname = self.operator.idname_py()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]

        def member(kind: bpy.types.Struct, key: str) -> BlenderResult[Descriptor]:
            return (
                Option
                .of_optional(kind.properties.get(key))
                .map(lambda prop: Descriptor(root=bpy.types.OperatorProperties, owner="", prop=prop))
                .to_result_with(lambda: (BlenderAbsentFault(owner=idname, key=key),))
            )

        try:
            kind = self.operator.get_rna_type()  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
        except KeyError:
            return Error((BlenderAbsentFault(owner="bpy.ops", key=idname),))
        return accumulated(member(kind, key) for key in self.values).map(
            lambda members: (idname, lambda properties: accumulated(descriptor.write(properties, self.values[descriptor.prop.identifier]) for descriptor in members).map(lambda _: None))
        )


class Macro(Command, frozen=True, kw_only=True):
    """Native operator steps executed as one undo step and redo entry."""

    steps: tuple[Placed, *tuple[Placed, ...]]

    @override
    def acquisitions(self, identity: Identity) -> BlenderResult[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the native macro class and defines its steps after class registration."""

        def built(name: str) -> tuple[tuple[Band, Acquisition], ...]:
            kind: type[bpy.types.Macro] = type(name, (bpy.types.Macro,), self.namespace(name, frozenset({"REGISTER", "UNDO"})))

            @contextmanager
            def defined() -> Generator[BlenderResult[None]]:
                yield accumulated(step.invocation(identity).bind(lambda pair: pair[1](kind.define(pair[0]).properties)) for step in self.steps).map(lambda _: None)

            return (Band.OPERATORS, kind), (Band.OPERATORS, defined())

        return identity.name(Name.OPERATOR, self.key).map(built)


# --- [OPERATIONS] -----------------------------------------------------------------------


def answered(applied: Applied, reason: str, notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> BlenderResult[Outcome]:
    """Answers written paths as a change beside the write faults, the faults where nothing was written, or the reason nothing changed."""
    match applied.written, applied.outcome.swap().to_option():
        case (_, *_), skipped:
            return Ok(Done(notes=notes, skipped=skipped))
        case (), Option(tag="some", some=faults):
            return Error(faults)
        case _:
            return Ok(Unchanged(reason=reason))


def tallied(edits: tuple[BlenderResult[object], ...], notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> BlenderResult[Done]:
    """Answers independent item edits as a change holding the refused items' faults, or every fault where no item changed."""
    return accumulated(edits).map(lambda _: Done(notes=notes)).or_else_with(lambda faults: Error(faults) if all(edit.is_error() for edit in edits) else Ok(Done(notes=notes, skipped=Some(faults))))


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Acquired",
    "Call",
    "Command",
    "Confirm",
    "Done",
    "Draw",
    "Empty",
    "Floating",
    "Invoke",
    "InvokePopup",
    "Macro",
    "Open",
    "Operator",
    "Outcome",
    "Own",
    "Pick",
    "Placed",
    "Properties",
    "Save",
    "Stock",
    "Suppressible",
    "Unchanged",
    "Writer",
    "answered",
    "tallied",
]
