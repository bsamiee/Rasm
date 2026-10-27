"""Operators, invocation forms, property placements, and macros."""

from abc import abstractmethod
from collections.abc import Callable, Generator, Iterable, Mapping
from contextlib import contextmanager, ExitStack
from enum import auto, Enum
import inspect
from itertools import chain
from pathlib import Path
from types import FunctionType, MappingProxyType, MethodType
from typing import Final, override, Self, TYPE_CHECKING

import bpy
from bpy_extras.io_utils import ExportHelper, ImportHelper
from expression import Error, Nothing, Ok, Option, Result, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.calls import given, Placed, Writer
from blender.bpy.data.rna.conditions import Condition
from blender.bpy.data.rna.descriptors import assigned, rooted, Struct, Value, written
from blender.bpy.data.rna.snapshots import Applied
from blender.bpy.extension.identity import Identity, Key
from blender.bpy.extension.registration import Acquisition, Band
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderReason
from blender.bpy.settings.attachments import annotated, nested, preferred
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, RecordReason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------

type Returns = set[rna_enums.OperatorReturnItems]
type Executed = Callable[[bpy.types.Operator, bpy.types.Context], Returns]
type Opening[T] = Callable[[bpy.types.Operator, bpy.types.Context, bpy.types.WindowManager, bpy.types.Event, T], Outcome[Returns]]
type Draw = Callable[[bpy.types.UILayout, bpy.types.Context, bpy.types.OperatorProperties, Identity], None]
type Invoke[T: bpy.types.PropertyGroup, S] = Confirm[T] | Suppressible[T] | Properties | Floating | InvokePopup | Open[T] | Save[T] | Pick | Modal[S]
type Answer = Done | Unchanged
type Located[T] = Callable[[bpy.types.Operator, T], Outcome[tuple[Path, ...]]]


class InvokePopup(Enum):
    """Redo properties popup or enum search popup."""

    REDO = auto()
    SEARCH = auto()

    def planned[T: bpy.types.PropertyGroup, S](self, plan: "Plan[T]", body: "Body[T, S]", /) -> "Outcome[Plan[T]]":
        """Opens the redo popup over an undoable command, or the search popup over its primary enum."""
        redone: Opening[T] = lambda operator, _context, manager, event, _properties: Ok(manager.invoke_props_popup(operator, event))

        def searched(operator: bpy.types.Operator, _context: bpy.types.Context, manager: bpy.types.WindowManager, _event: bpy.types.Event, _properties: T) -> Outcome[Returns]:
            manager.invoke_search_popup(operator)
            return Ok({"RUNNING_MODAL"})

        match self, body.command.undo:
            case InvokePopup.REDO, True:
                return Ok(msgspec.structs.replace(plan, opening=redone))
            case InvokePopup.REDO, _:
                return Error((Fault(reason=BlenderReason.UNREGISTERED, subject=body.command.key, cause="the redo popup repeats an undo step, so undo must be True"),))
            case InvokePopup.SEARCH, _:
                return Ok(msgspec.structs.replace(plan, opening=searched))


class Gesture(Enum):
    """Pointer movement with modal cursor and grab."""

    DRAG = auto()
    DRAG_X = auto()
    DRAG_Y = auto()
    TRACK = auto()
    TIMED = auto()

    @property
    def cursor(self) -> Option["rna_enums.WindowCursorItems"]:
        """Modal cursor the gesture shows, none for a timed run."""
        match self:
            case Gesture.DRAG:
                return Some("SCROLL_XY")
            case Gesture.DRAG_X:
                return Some("SCROLL_X")
            case Gesture.DRAG_Y:
                return Some("SCROLL_Y")
            case Gesture.TRACK:
                return Some("CROSSHAIR")
            case Gesture.TIMED:
                return Nothing

    @property
    def grab(self) -> Option["rna_enums.OperatorTypeFlagItems"]:
        """Cursor grab flag of a drag along its axes."""
        match self:
            case Gesture.DRAG:
                return Some("GRAB_CURSOR")
            case Gesture.DRAG_X:
                return Some("GRAB_CURSOR_X")
            case Gesture.DRAG_Y:
                return Some("GRAB_CURSOR_Y")
            case Gesture.TRACK | Gesture.TIMED:
                return Nothing

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Blocking and grab operator flags."""
        return frozenset(("BLOCKING", *self.grab.to_list()) if self.cursor.is_some() else self.grab.to_list())

    @property
    def relative(self) -> bool:
        """Whether cursor stays grabbed."""
        return self.grab.is_some()


# --- [CONSTANTS] ------------------------------------------------------------------------

_WHEELS: Final = MappingProxyType({
    ("WHEELINMOUSE", False): "WHEELUPMOUSE",
    ("WHEELINMOUSE", True): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", False): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", True): "WHEELUPMOUSE",
})


# --- [MODELS] ---------------------------------------------------------------------------


class Plan[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Operator class under construction: flags, property definitions, members, bases, release scopes, execute, and invoke opening."""

    flags: frozenset["rna_enums.OperatorTypeFlagItems"]
    annotations: Mapping[str, object]
    members: Mapping[str, object]
    execute: Executed
    opening: Opening[T]
    bases: tuple[type, ...] = (bpy.types.Operator,)
    lifetime: tuple[Acquisition, ...] = ()


class Empty(bpy.types.PropertyGroup):
    """Form of operator without properties."""


class Done(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with changes, report entries, and skipped faults."""

    notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()
    skipped: Option[Faults] = Nothing

    @classmethod
    def wrote(cls, paths: Iterable[str | Path]) -> Self:
        """Reports each written path."""
        return cls(notes=tuple(("INFO", f"Wrote {path}") for path in paths))


class Unchanged(msgspec.Struct, frozen=True, kw_only=True):
    """Body result with no change, reason reported as INFO."""

    reason: str


class Call[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Body invocation with context, operator properties under the form group type, and file paths."""

    identity: Identity
    context: bpy.types.Context
    properties: T
    paths: tuple[Path, ...] = ()

    def root[K: Struct](self, kind: type[K]) -> Outcome[K]:
        """Returns required context root."""
        return rooted(kind, self.context)


class Confirm[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Confirmation before execution with computed message."""

    message: Option[Callable[[bpy.types.Context, T], Outcome[str]]] = Nothing
    confirm_text: Option[str] = Nothing
    icon: str = bpy.types.WindowManager.bl_rna.functions["invoke_confirm"].parameters["icon"].default  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
    title: Option[str] = Nothing
    text_ctxt: Option[str] = Nothing
    translate: bool = True

    def planned[S](self, plan: Plan[T], _body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Opens `invoke_confirm` with the computed message."""
        keywords = given(msgspec.structs.asdict(self))

        opening: Opening[T] = lambda operator, context, manager, event, properties: (
            self.message
            .map(lambda compute: compute(context, properties).map(lambda text: keywords | {"message": text}))
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

    def planned[T: bpy.types.PropertyGroup, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Opens `invoke_props_dialog`."""
        opening: Opening[T] = lambda operator, _context, manager, _event, _properties: Ok(self.opened(operator, manager))
        return Ok(msgspec.structs.replace(plan, opening=opening))


class Suppressible[T: bpy.types.PropertyGroup](Dialog, frozen=True, kw_only=True):
    """Confirmation dialog with checkbox clearing `ask_<key>` preference on success."""

    message: Callable[[bpy.types.Context, T], str]
    icon: "rna_enums.IconItems" = "NONE"

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Opens the dialog while `ask_<key>` holds, clearing it after a run with the checkbox set."""
        ask = body.command.ask

        def asked(context: bpy.types.Context) -> Outcome[bool]:
            return (
                rooted(bpy.types.Preferences, context)
                .map(lambda preferences: Option.of_optional(preferences.addons.get(body.identity.package)).bind(lambda addon: Option.of_optional(addon.preferences)))
                .bind(
                    lambda holder: holder.bind(lambda held: Option.of_optional(held.bl_rna.properties.get(ask)).map(lambda prop: bool(getattr(held, prop.identifier)))).to_result_with(
                        lambda: (Fault(reason=BlenderReason.ABSENT, subject=body.identity.package, member=ask),)
                    )
                )
            )

        def cleared(operator: bpy.types.Operator, context: bpy.types.Context) -> Outcome[None]:
            suppressed: bool = operator.suppress  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return (
                rooted(bpy.types.Preferences, context).bind(lambda preferences: preferred(body.identity, preferences, lambda holder: written(holder, ask, False).map(lambda _: None)))
                if suppressed
                else Ok(None)
            )

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return body.finished(operator, body.performed(operator, context).bind(lambda outcome: cleared(operator, context).map(lambda _: outcome)))

        def asking(operator: bpy.types.Operator, context: bpy.types.Context) -> None:
            properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            for layout in Option.of_optional(operator.layout).to_list():
                layout.use_property_split = True
                layout.label_multiline(text=self.message(context, properties), icon=self.icon)
                body.draw(operator, context)
                layout.prop(operator, "suppress")

        opening: Opening[T] = lambda operator, context, manager, _event, _properties: asked(context).map(lambda shown: self.opened(operator, manager) if shown else execute(operator, context))
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

    def planned[T: bpy.types.PropertyGroup, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Opens `invoke_popup`."""
        keywords = msgspec.structs.asdict(self)
        opening: Opening[T] = lambda operator, _context, manager, _event, _properties: Ok(manager.invoke_popup(operator, **keywords))
        return Ok(msgspec.structs.replace(plan, opening=opening))


class Open[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Import file browser filtering by the formats the properties choose."""

    extensions: Callable[[T], tuple[str, ...]]
    confirm_text: str = ""

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Bases the class on `ImportHelper`, running the body over the chosen files."""

        def chosen(operator: bpy.types.Operator, _properties: T) -> Outcome[tuple[Path, ...]]:
            paths = tuple(Path(operator.directory, item.name) for item in operator.files if item.name) or tuple(Path(path) for path in (operator.filepath,) if path)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Ok(paths) if paths else Error((Fault(reason=BlenderReason.UNMET, subject=Unopened(title=body.command.title).message),))

        execute: Executed = lambda operator, context: body.finished(operator, body.performed(operator, context, Some(chosen)))
        opening: Opening[T] = lambda operator, context, _manager, _event, _properties: Ok(ImportHelper.invoke_popup(operator, context, confirm_text=self.confirm_text))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
        return Ok(
            msgspec.structs.replace(
                plan,
                bases=(ImportHelper, bpy.types.Operator),
                annotations={
                    **plan.annotations,
                    "filter_glob": globbed(self.extensions),
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


class Save[T: bpy.types.PropertyGroup](msgspec.Struct, frozen=True, kw_only=True):
    """Export file browser with optional output destinations."""

    extensions: Callable[[T], tuple[str, *tuple[str, ...]]]
    destinations: Option[Callable[[Path, T], tuple[Path, ...]]] = Nothing

    def planned[S](self, plan: Plan[T], body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Bases the class on `ExportHelper`, confirming replacement of existing destinations once per invocation."""

        def located(operator: bpy.types.Operator, properties: T) -> Outcome[tuple[Path, ...]]:
            path = Path(operator.filepath)  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            return Ok(self.destinations.map(lambda project: project(path, properties)).default_value((path,)))

        def existing(operator: bpy.types.Operator, properties: T) -> Outcome[tuple[Path, ...]]:
            return located(operator, properties).bind(
                lambda paths: catch(lambda: tuple(path for path in paths if path.exists()), exception=OSError)().map_error(
                    lambda error: (Fault(reason=RecordReason.UNREADABLE, subject=repr(paths), cause=error),)
                )
            )

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            confirming: bool = self.destinations.is_some() and operator.options.is_invoke and not operator.options.is_repeat and not operator.replacing  # type: ignore[attr-defined]  # ty: ignore[unresolved-attribute]
            properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return (
                (existing(operator, properties) if confirming else Ok(()))
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

        def extension(operator: bpy.types.Operator) -> str:
            properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return self.extensions(properties)[0]

        def checked(operator: bpy.types.Operator, context: bpy.types.Context) -> bool:
            return any((ExportHelper.check(operator, context), *body.command.check.map(lambda callback: callback(operator, context)).to_list()))  # type: ignore[arg-type]  # ty: ignore[invalid-argument-type]

        opening: Opening[T] = lambda operator, context, _manager, event, _properties: Ok(ExportHelper.invoke(operator, context, event))  # type: ignore[arg-type]  # ty: ignore[invalid-assignment, invalid-argument-type]
        destined = {"check_existing": bpy.props.BoolProperty(default=False, options={"HIDDEN", "SKIP_SAVE"}), "replacing": bpy.props.BoolProperty(options={"HIDDEN", "SKIP_SAVE"})}
        return Ok(
            msgspec.structs.replace(
                plan,
                bases=(ExportHelper, bpy.types.Operator),
                annotations={**plan.annotations, "filter_glob": globbed(self.extensions), **(destined if self.destinations.is_some() else {})},
                members={**plan.members, "filename_ext": property(extension), "check": checked},
                execute=execute,
                opening=opening,
            )
        )


class Pick(msgspec.Struct, frozen=True, kw_only=True):
    """Invocation waiting for pointer location under cursor."""

    cursor: "rna_enums.WindowCursorItems"

    def planned[T: bpy.types.PropertyGroup, S](self, plan: Plan[T], _body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Declares `DEPENDS_ON_CURSOR` with the pending cursor."""
        return Ok(msgspec.structs.replace(plan, flags=plan.flags | {"DEPENDS_ON_CURSOR"}, members={**plan.members, "bl_cursor_pending": self.cursor}))


class Travel(msgspec.Struct, frozen=True, kw_only=True):
    """Pointer travel since press."""

    x: int
    y: int
    dx: int
    dy: int
    dragged: bool
    precise: bool
    snapped: bool


class Press(msgspec.Struct, frozen=True, kw_only=True):
    """Press position and device drag threshold in window pixels."""

    x: int
    y: int
    threshold: int

    def travel(self, event: bpy.types.Event, *, snapped: bool) -> Travel:
        """Returns travel from press to event."""
        dx, dy = event.mouse_x - self.x, event.mouse_y - self.y
        return Travel(x=event.mouse_region_x, y=event.mouse_region_y, dx=dx, dy=dy, dragged=max(abs(dx), abs(dy)) > self.threshold, precise=event.shift, snapped=snapped)


class Tick[S](msgspec.Struct, frozen=True, kw_only=True):
    """Timer interval and state step, Nothing finishing."""

    interval: float
    body: Callable[[bpy.types.Context, S, float], Outcome[Option[S]]]


class Run[S](msgspec.Struct, frozen=True, kw_only=True):
    """Live run with rollback snapshot, held resources, and event types pressed since it began."""

    initial: S
    state: S
    manager: bpy.types.WindowManager
    window: bpy.types.Window
    workspace: bpy.types.WorkSpace
    area: Option[bpy.types.Area]
    region: Option[bpy.types.Region]
    press: Press
    ticking: Option[tuple[Tick[S], bpy.types.Timer]]
    elapsed: float = 0.0
    snapped: bool = False
    pressed: frozenset[str] = frozenset()


class Modal[S](msgspec.Struct, frozen=True, kw_only=True):
    """Modal operator body with preview and rollback."""

    gesture: Gesture
    begin: Callable[[bpy.types.Context, Travel], Outcome[S]]
    preview: Callable[[bpy.types.Context, S, Travel], Outcome[Option[str]]]
    restore: Callable[[bpy.types.Context, S], Outcome[None]]
    tick: Option[Tick[S]] = Nothing
    status: Option[Callable[[bpy.types.UILayout, S], None]] = Nothing
    priority: bool = False
    event: Option[Callable[[bpy.types.Context, S, bpy.types.Event], Outcome[tuple[S, "rna_enums.OperatorReturnItems"]]]] = Nothing
    release: Option[Callable[[bpy.types.Context, S], Outcome[None]]] = Nothing

    # --- [PLAN]

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Gesture flags with `MODAL_PRIORITY` when `priority` is set."""
        return self.gesture.options | frozenset(("MODAL_PRIORITY",) if self.priority else ())

    def planned[T: bpy.types.PropertyGroup](self, plan: Plan[T], body: "Body[T, S]", /) -> Outcome[Plan[T]]:
        """Adds gesture flags, modal and cancel members, the run opening, and a release cancelling live runs at unregistration."""
        runs = Runs[S]()

        def cancelled(operator: bpy.types.Operator, context: bpy.types.Context, entries: tuple[tuple["rna_enums.WmReportItems", str], ...]) -> Returns:
            return body.noted(operator, (*entries, *self.ended(runs, operator, context, cancelled=True).map(lambda _: ()).default_with(rendered)), "CANCELLED")

        def returned(operator: bpy.types.Operator, context: bpy.types.Context, outcome: "rna_enums.OperatorReturnItems") -> Returns:
            match outcome:
                case "FINISHED":
                    return body.settled(operator, context, lambda rolled: self.ended(runs, operator, context, cancelled=rolled))
                case "CANCELLED":
                    return cancelled(operator, context, ())
                case _:
                    return {outcome}

        def stepped(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
            return self.handled(runs, operator, context, event).map(lambda outcome: returned(operator, context, outcome)).default_with(lambda faults: cancelled(operator, context, rendered(faults)))

        opening: Opening[T] = lambda operator, context, _manager, event, _properties: self.began(runs, operator, context, event).map(lambda outcome: {outcome})

        @contextmanager
        def released() -> Generator[None]:
            try:
                yield
            finally:
                for operator in tuple(runs.held):
                    cancelled(operator, bpy.context, ())

        return Ok(
            msgspec.structs.replace(
                plan,
                flags=plan.flags | self.options,
                members={**plan.members, "modal": stepped, "cancel": lambda operator, context: cancelled(operator, context, ())},
                opening=opening,
                lifetime=(*plan.lifetime, released()),
            )
        )

    # --- [LIFECYCLE]

    def began(self, runs: "Runs[S]", operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome["rna_enums.OperatorReturnItems"]:
        """Returns RUNNING_MODAL once begun, faulting before resource acquisition."""

        def started(run: Run[S]) -> "rna_enums.OperatorReturnItems":
            runs.kept(operator, run)
            run.manager.modal_handler_add(operator)
            for cursor in self.gesture.cursor.to_list():
                run.window.cursor_modal_set(cursor)
            run.workspace.status_text_set(self._hinted(runs, operator))
            return "RUNNING_MODAL"

        match context:
            case bpy.types.Context(window_manager=bpy.types.WindowManager() as manager, window=bpy.types.Window(workspace=workspace) as window):
                preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                inputs, system = preferences.inputs, preferences.system
                threshold = (
                    (inputs.drag_threshold_tablet if event.is_tablet else inputs.drag_threshold_mouse)
                    if event.type in {"LEFTMOUSE", "MIDDLEMOUSE", "RIGHTMOUSE", "BUTTON4MOUSE", "BUTTON5MOUSE", "BUTTON6MOUSE", "BUTTON7MOUSE"}
                    else inputs.drag_threshold
                )
                x, y = (event.mouse_prev_press_x, event.mouse_prev_press_y) if event.value == "CLICK_DRAG" else (event.mouse_x, event.mouse_y)
                press = Press(x=x, y=y, threshold=int(threshold * system.ui_scale))
                return self.begin(context, press.travel(event, snapped=False)).map(
                    lambda state: started(
                        Run(
                            initial=state,
                            state=state,
                            manager=manager,
                            window=window,
                            workspace=workspace,
                            area=Option.of_optional(context.area),
                            region=Option.of_optional(context.region),
                            press=press,
                            ticking=self.tick.map(lambda tick: (tick, manager.event_timer_add(tick.interval, window=window))),
                        )
                    )
                )
            case _:
                return Error((Fault(reason=BlenderReason.ABSENT, subject=repr(context), member="window"),))

    def ended(self, runs: "Runs[S]", operator: bpy.types.Operator, context: bpy.types.Context, *, cancelled: bool) -> Outcome[None]:
        """Releases live run, restoring initial snapshot when cancelled."""

        def released(action: Callable[[], None]) -> Outcome[None]:
            return catch(action, exception=ReferenceError)().map_error(lambda error: (Fault(reason=BlenderReason.REFUSED, subject=self.ended.__qualname__, cause=error),))

        def restored(run: Run[S], live: tuple[bpy.types.Window, ...]) -> Outcome[None]:
            area = next((area for window in live for area in window.screen.areas if area in run.area.to_list()), None)
            region = next((region for held in Option.of_optional(area).to_list() for region in held.regions if region in run.region.to_list()), None)
            with ExitStack() as scope:
                entered = catch(scope.enter_context, exception=TypeError)(context.temp_override(window=next(iter(live), None), area=area, region=region)).map_error(
                    lambda error: (Fault(reason=BlenderReason.REFUSED, subject="Context.temp_override", cause=error),)
                )
                return Result[object, Faults].bind(entered, lambda _: self.restore(context, run.initial))

        def closed(run: Run[S]) -> Outcome[None]:
            live = tuple(window for window in run.manager.windows if window == run.window)
            cleanup = accumulated((
                *(release(context, run.state) for release in self.release.to_list()),
                *(released(window.cursor_modal_restore) for _ in self.gesture.cursor.to_list() for window in live),
                released(lambda: run.workspace.status_text_set(None)),
                *(released(lambda: area.header_text_set(None)) for area in run.area.to_list()),
                *(released(lambda: run.manager.event_timer_remove(timer)) for _, timer in run.ticking.to_list()),
                *(released(lambda: window.cursor_warp(run.press.x, run.press.y)) for window in live if self.gesture.relative),
            ))
            return paired(cleanup, restored(run, live) if cancelled else Ok(None)).map(lambda _: None)

        return runs.released(operator).map(closed).default_value(Ok(None))

    # --- [EVENTS]

    def handled(self, runs: "Runs[S]", operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome["rna_enums.OperatorReturnItems"]:
        """Returns event outcome, matching modal items against the keys held before the event."""
        prior = runs.held[operator]
        held = prior.pressed - {event.type} if event.value == "RELEASE" else prior.pressed
        run = runs.kept(operator, msgspec.structs.replace(prior, pressed=held | {event.type} if event.value == "PRESS" else held))

        def previewed(current: Run[S]) -> Outcome["rna_enums.OperatorReturnItems"]:
            runs.kept(operator, current)
            result = self.preview(context, current.state, current.press.travel(event, snapped=current.snapped))
            for text in result.to_option().to_list():
                for area in current.area.to_list():
                    area.header_text_set(text.to_optional())
            return result.map(lambda _: "RUNNING_MODAL")

        def ticked(tick: Tick[S], timer: bpy.types.Timer) -> Outcome["rna_enums.OperatorReturnItems"]:
            elapsed = timer.time_duration
            return (
                Ok("PASS_THROUGH")
                if elapsed == run.elapsed
                else tick.body(context, run.state, elapsed).bind(
                    lambda following: following.map(lambda state: previewed(msgspec.structs.replace(run, state=state, elapsed=elapsed))).default_value(Ok("FINISHED"))
                )
            )

        def following(result: tuple[S, "rna_enums.OperatorReturnItems"]) -> "rna_enums.OperatorReturnItems":
            state, outcome = result
            runs.kept(operator, msgspec.structs.replace(run, state=state))
            return outcome

        if context.window != run.window or any(context.area != area for area in run.area.to_list()) or any(context.region != region for region in run.region.to_list()):
            return Ok("CANCELLED")
        run.workspace.status_text_set(self._hinted(runs, operator))
        for handle in self.event.to_list():
            return handle(context, run.state, event).map(following)
        preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        action = next((item for item in _modal(run.manager).keymap_items if matched(item, event, held, inverted=preferences.inputs.invert_zoom_wheel)), None)
        match action, event.type:
            case bpy.types.KeyMapItem(propvalue="CANCEL"), _:
                return Ok("CANCELLED")  # type: ignore[unreachable]
            case None, "TIMER":
                return run.ticking.map(lambda ticking: ticked(*ticking)).default_value(Ok("PASS_THROUGH"))
            case _ if self.gesture is Gesture.TIMED:
                return Ok("PASS_THROUGH")
            case bpy.types.KeyMapItem(propvalue="APPLY"), _:
                return self.preview(context, run.state, run.press.travel(event, snapped=run.snapped)).map(lambda _: "FINISHED")  # type: ignore[unreachable]
            case bpy.types.KeyMapItem(propvalue="SNAP" | "SNAP_OFF") as item, _:
                return previewed(msgspec.structs.replace(run, snapped=item.propvalue == "SNAP"))  # type: ignore[unreachable]
            case None, "MOUSEMOVE" | "INBETWEEN_MOUSEMOVE":
                return previewed(run)
            case _:
                return Ok("RUNNING_MODAL")

    # --- [STATUS]

    def _hinted(self, runs: "Runs[S]", operator: bpy.types.Operator) -> Callable[[bpy.types.Header, bpy.types.Context], None]:
        """Returns status bar callback drawing user chords and run state."""

        def drawn(header: bpy.types.Header, _context: bpy.types.Context) -> None:
            """Draws user modal items in action order, holding no keymap reference."""
            run = runs.held[operator]
            items = (
                item
                for keymap in (_modal(run.manager),)
                for action in keymap.modal_event_values
                if (action.identifier == "CANCEL" if self.gesture is Gesture.TIMED else action.identifier != "SNAP_OFF")
                for item in Option.of_optional(next((item for item in keymap.keymap_items if item.active and item.propvalue == action.identifier), None)).to_list()  # ty: ignore[truthiness-test-of-none-union]
            )
            for layout in Option.of_optional(header.layout).to_list():
                for item in items:
                    layout.template_event_from_keymap_item(item, text=bpy.types.UILayout.enum_item_name(item, "propvalue", item.propvalue))
                if self.gesture.relative:
                    layout.label(text="Precision", icon="EVENT_SHIFT")
                for draw in self.status.to_list():
                    draw(layout, run.state)

        return drawn


class Command(msgspec.Struct, frozen=True, kw_only=True):
    """Command with poll condition and search visibility, its title derived from its key."""

    condition: Option[Condition] = Nothing
    icon: Option["rna_enums.IconItems"] = Nothing
    internal: bool = False

    # --- [TEXT]

    @property
    @abstractmethod
    def key(self) -> Key:
        """Identifier the host name derives from."""
        ...

    @property
    @abstractmethod
    def description(self) -> str:
        """Tooltip text."""
        ...

    @property
    def title(self) -> str:
        """Label the key displays as."""
        return bpy.path.display_name(self.key)

    # --- [REGISTRATION]

    @abstractmethod
    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds command registrations."""
        ...

    # --- [PLACEMENT]

    def invocation(self, identity: Identity, /) -> Outcome[tuple[str, Writer]]:
        """Returns operator name with no-op writer."""
        return identity.name(bpy.types.Operator, self.key).map(lambda name: (name, lambda _: Ok(None)))

    # --- [CLASS_MEMBERS]

    def namespace(self, name: str, flags: frozenset["rna_enums.OperatorTypeFlagItems"]) -> dict[str, object]:
        """Returns class members with poll message of condition faults."""

        def poll(cls: type[bpy.types.Operator], context: bpy.types.Context) -> bool:
            faults = self.condition.bind(lambda condition: condition.check(context).swap().to_option())
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


class Operator[T: bpy.types.PropertyGroup, S = None](Command, frozen=True, kw_only=True):
    """Form group and body with invocation form and undo, its key the body function name and its description the first paragraph of the body docstring."""

    form: type[T]
    run: Callable[[Call[T]], Outcome[Answer]]
    invoke: Option[Invoke[T, S]] = Nothing
    seed: Option[Callable[[T, bpy.types.Event, Identity], Mapping[str, Value]]] = Nothing
    undo: bool | str = True
    draw: Option[Draw] = Nothing
    check: Option[Callable[[bpy.types.Operator, bpy.types.Context], bool]] = Nothing
    presets: bool = False
    primary: Option[str] = Nothing

    def __post_init__(self) -> None:
        """Refuses a body holding no function name or docstring."""
        function = inspect.unwrap(self.run)
        if not (isinstance(function, FunctionType | MethodType) and function.__name__.isidentifier() and inspect.getdoc(function) is not None):
            raise TypeError(f"{function!r} has no function name or docstring")

    # --- [TEXT]

    @property
    @override
    def key(self) -> Key:
        """Body function name."""
        return self.run.__name__  # ty: ignore[unresolved-attribute]

    @property
    @override
    def description(self) -> str:
        """First paragraph of the body docstring."""
        return " ".join(str(inspect.getdoc(inspect.unwrap(self.run))).split("\n\n")[0].split()).removesuffix(".")

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

    @override
    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds the form's property groups, operator class, and the release scopes of its invocation form."""
        return identity.name(bpy.types.Operator, self.key).bind(
            lambda name: self.built(Body(command=self, identity=identity), name).map(
                lambda acquired: (*((Band.GROUPS, group) for group in nested(self.form)), *((Band.OPERATORS, item) for item in acquired))
            )
        )

    # --- [OPERATOR_CLASS]

    def built(self, body: "Body[T, S]", name: str) -> Outcome[tuple[Acquisition, ...]]:
        """Builds operator class from the plan its invocation form folds, with the form's release scopes."""
        annotations = annotated(self.form)

        def classed(plan: Plan[T]) -> tuple[Acquisition, ...]:
            members = {**plan.members, "__annotations__": dict(plan.annotations), "execute": plan.execute, **self.namespace(name, plan.flags)}
            return (type(name, plan.bases, members | ({"invoke": body.invoking(plan.opening)} if self.invoked else {})), *plan.lifetime)

        initial = Plan[T](
            flags=self.flags,
            annotations=annotations,
            members={
                **(
                    {"description": classmethod(lambda _cls, _context, properties: body.described(properties))} if any(prop.function is bpy.props.EnumProperty for prop in annotations.values()) else {}
                ),
                **({"draw": body.draw} if self.draw.is_some() else {}),
                **self.check.map(lambda callback: {"check": callback}).default_value({}),
                **self.primary.map(lambda member: {"bl_property": member}).default_value({}),
                **({"bl_undo_group": self.undo} if isinstance(self.undo, str) else {}),
            },
            execute=body.execute,
            opening=lambda operator, context, _manager, _event, _properties: Ok(body.execute(operator, context)),
        )
        return self.invoke.map(lambda form: form.planned(initial, body)).default_value(Ok(initial)).map(classed)


class Body[T: bpy.types.PropertyGroup, S](msgspec.Struct, frozen=True, kw_only=True):
    """Callbacks one operator class composes over its command."""

    command: Operator[T, S]
    identity: Identity

    # --- [REPORTS]

    @staticmethod
    def noted(operator: bpy.types.Operator, entries: Iterable[tuple["rna_enums.WmReportItems", str]], outcome: "rna_enums.OperatorReturnItems") -> Returns:
        """Reports entries, answering outcome."""
        for severity, text in entries:
            operator.report({severity}, text)
        return {outcome}

    @staticmethod
    def finished(operator: bpy.types.Operator, result: Outcome[Answer]) -> Returns:
        """Reports body result as notes and skipped faults with FINISHED, a reason with CANCELLED, or rendered faults with CANCELLED."""

        def concluded(outcome: Answer) -> Returns:
            match outcome:
                case Done(notes=notes, skipped=skipped):
                    return Body.noted(operator, (*notes, *(("WARNING", fault.message) for faults in skipped.to_list() for fault in faults)), "FINISHED")
                case Unchanged(reason=reason):
                    return Body.noted(operator, (("INFO", reason),), "CANCELLED")

        return result.map(concluded).default_with(lambda faults: Body.noted(operator, rendered(faults), "CANCELLED"))

    # --- [RUNS]

    def performed(self, operator: bpy.types.Operator, context: bpy.types.Context, located: Option[Located[T]] = Nothing) -> Outcome[Answer]:
        """Runs the command over the operator properties and the paths the form locates."""
        properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
        return (
            located
            .map(lambda locate: locate(operator, properties))
            .default_value(Ok(()))
            .bind(lambda paths: self.command.run(Call(identity=self.identity, context=context, properties=properties, paths=paths)))
        )

    @property
    def execute(self) -> Executed:
        """Class `execute` running the command and reporting its result."""

        def execute(operator: bpy.types.Operator, context: bpy.types.Context) -> Returns:
            return self.finished(operator, self.performed(operator, context))

        return execute

    def settled(self, operator: bpy.types.Operator, context: bpy.types.Context, release: Callable[[bool], Outcome[None]]) -> Returns:
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

    def shown(self, properties: bpy.types.OperatorProperties) -> tuple[bpy.types.Property, ...]:
        """Visible form properties."""
        return tuple(prop for prop in properties.bl_rna.properties if prop.identifier in annotated(self.command.form) and not prop.is_hidden)

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
        """Class `draw` drawing the command's layout, else every visible form property."""

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
        """Returns invoke writing seed values into unset properties, then opening the form."""

        def invoke(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
            properties: T = operator.properties  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return (
                self.command.seed
                .map(lambda seed: seed(properties, event, self.identity))
                .map(lambda values: assigned(operator.properties, {name: value for name, value in values.items() if not operator.properties.is_property_set(name)}))
                .default_value(Ok(None))
                .bind(lambda _: rooted(bpy.types.WindowManager, context).bind(lambda manager: opening(operator, context, manager, event, properties)))
                .default_with(lambda faults: self.noted(operator, rendered(faults), "CANCELLED"))
            )

        return invoke


class Own[T: bpy.types.PropertyGroup, S = None](msgspec.Struct, frozen=True, kw_only=True):
    """Placement of owned operator with property values."""

    command: Operator[T, S]
    values: Mapping[str, Value] = msgspec.field(default_factory=dict)

    def invocation(self, identity: Identity, /) -> Outcome[tuple[str, Writer]]:
        """Returns operator name and writer of supplied values."""
        return identity.name(bpy.types.Operator, self.command.key).map(lambda name: (name, lambda properties: assigned(properties, self.values)))


class Macro(Command, frozen=True, kw_only=True):
    """Operator steps run in one undo step and redo entry, stating the key and description no body function holds."""

    key: Key
    description: str
    steps: tuple[Placed, *tuple[Placed, ...]]

    @override
    def acquisitions(self, identity: Identity) -> Outcome[tuple[tuple[Band, Acquisition], ...]]:
        """Builds macro class, defining its steps after class registration."""

        def built(name: str) -> tuple[tuple[Band, Acquisition], ...]:
            kind: type[bpy.types.Macro] = type(name, (bpy.types.Macro,), self.namespace(name, frozenset({"REGISTER", "UNDO"})))

            @contextmanager
            def defined() -> Generator[Outcome[None]]:
                yield accumulated(step.invocation(identity).bind(lambda pair: pair[1](kind.define(pair[0]).properties)) for step in self.steps).map(lambda _: None)

            return (Band.OPERATORS, kind), (Band.OPERATORS, defined())

        return identity.name(bpy.types.Operator, self.key).map(built)


# --- [SERVICES] -------------------------------------------------------------------------


class Runs[S]:
    """Live runs of one registered operator class by operator instance, each released when its run ends."""

    __slots__ = ("held",)

    def __init__(self) -> None:
        """Starts with no live run."""
        self.held: dict[bpy.types.Operator, Run[S]] = {}

    def kept(self, operator: bpy.types.Operator, run: Run[S]) -> Run[S]:
        """Holds the operator's current run, answering it."""
        self.held[operator] = run
        return run

    def released(self, operator: bpy.types.Operator) -> Option[Run[S]]:
        """Releases the operator's run, `Nothing` when none was live."""
        return Option.of_optional(self.held.pop(operator, None))


# --- [OPERATIONS] -----------------------------------------------------------------------


def globbed[T: bpy.types.PropertyGroup](extensions: Callable[[T], tuple[str, ...]]) -> bpy.props._PropertyDeferred:
    """File browser `filter_glob` property over the extensions the properties choose."""

    def glob(properties: T) -> str:
        return ";".join(f"*{extension}" for extension in extensions(properties))

    return bpy.props.StringProperty(options={"HIDDEN", "SKIP_SAVE"}, get=glob)


def answered(applied: Applied, reason: str, notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> Outcome[Answer]:
    """Returns Done skipping write faults when a path was written, else write faults, else Unchanged."""
    match applied.written, applied.outcome.swap().to_option():
        case (_, *_), skipped:
            return Ok(Done(notes=notes, skipped=skipped))
        case (), Option(tag="some", some=faults):
            return Error(faults)
        case _:
            return Ok(Unchanged(reason=reason))


def tallied(edits: tuple[Outcome[object], ...], notes: tuple[tuple["rna_enums.WmReportItems", str], ...] = ()) -> Outcome[Done]:
    """Returns Done skipping faults when any item changed, else every fault."""
    return accumulated(edits).map(lambda _: Done(notes=notes)).or_else_with(lambda faults: Error(faults) if all(edit.is_error() for edit in edits) else Ok(Done(notes=notes, skipped=Some(faults))))


def matched(item: bpy.types.KeyMapItem, event: bpy.types.Event, held: frozenset[str], *, inverted: bool) -> bool:
    """Checks whether active modal item matches event as `wm_eventmatch` does, its key modifier among the keys held and wheel zoom inverted or not."""
    return (
        item.active  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
        and (item.repeat or not event.is_repeat)
        and (
            (item.type == "TEXTINPUT" and event.value == "PRESS" and bool(event.unicode))
            or (
                _WHEELS.get((item.type, inverted), item.type) == event.type
                and item.value in {"ANY", event.value}
                and (item.value != "CLICK_DRAG" or item.direction in {"ANY", event.direction})
                and all(
                    state in {-1, int(pressed)} or event.type in keys
                    for state, pressed, keys in (
                        (item.shift, event.shift, ("LEFT_SHIFT", "RIGHT_SHIFT")),
                        (item.ctrl, event.ctrl, ("LEFT_CTRL", "RIGHT_CTRL")),
                        (item.alt, event.alt, ("LEFT_ALT", "RIGHT_ALT")),
                        (item.oskey, event.oskey, ("OSKEY",)),
                        (item.hyper, event.hyper, ("HYPER",)),
                    )
                )
                and (item.key_modifier == "NONE" or item.key_modifier in held)
            )
        )
    )


def _modal(manager: bpy.types.WindowManager) -> bpy.types.KeyMap:
    """Returns the user keyconfig's standard modal keymap, which Python operators cannot replace, a keyconfig the stubs type as optional."""
    user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
    return user.keymaps["Standard Modal Map"]


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = [
    "Answer",
    "Body",
    "Call",
    "Command",
    "Confirm",
    "Dialog",
    "Done",
    "Draw",
    "Empty",
    "Executed",
    "Floating",
    "Gesture",
    "Invoke",
    "InvokePopup",
    "Located",
    "Macro",
    "Modal",
    "Open",
    "Opening",
    "Operator",
    "Own",
    "Pick",
    "Plan",
    "Press",
    "Properties",
    "Returns",
    "Run",
    "Runs",
    "Save",
    "Suppressible",
    "Tick",
    "Travel",
    "Unchanged",
    "Unopened",
    "answered",
    "matched",
    "tallied",
]
