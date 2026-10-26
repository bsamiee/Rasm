"""Modal operator gestures with preview and rollback."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import Enum
from types import MappingProxyType
from typing import Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.forms import Opening, Plan, Returns
from blender.bpy.extension.reports import rendered
from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.records.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

    from blender.bpy.commands.operators import Body

# --- [TYPES] ----------------------------------------------------------------------------


class Gesture(Enum):
    """Pointer movement with modal cursor and grab."""

    DRAG = (Some("SCROLL_XY"), Some("GRAB_CURSOR"))
    DRAG_X = (Some("SCROLL_X"), Some("GRAB_CURSOR_X"))
    DRAG_Y = (Some("SCROLL_Y"), Some("GRAB_CURSOR_Y"))
    TRACK = (Some("CROSSHAIR"), Nothing)
    TIMED = (Nothing, Nothing)

    def __init__(self, cursor: Option["rna_enums.WindowCursorItems"], grab: Option["rna_enums.OperatorTypeFlagItems"]) -> None:
        """Stores cursor and grab flag."""
        self.cursor = cursor
        self.grab = grab

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Blocking and grab operator flags."""
        return frozenset(("BLOCKING", *self.grab.to_list()) if self.cursor.is_some() else self.grab.to_list())

    @property
    def relative(self) -> bool:
        """Whether cursor stays grabbed."""
        return self.grab.is_some()


# --- [CONSTANTS] ------------------------------------------------------------------------

MODAL_KEYMAP = "Standard Modal Map"
_WHEELS: Final = MappingProxyType({
    ("WHEELINMOUSE", False): "WHEELUPMOUSE",
    ("WHEELINMOUSE", True): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", False): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", True): "WHEELUPMOUSE",
})


# --- [MODELS] ---------------------------------------------------------------------------


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
    body: Callable[[bpy.types.Context, S, float], BlenderResult[Option[S]]]


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


class Runs[S](msgspec.Struct, frozen=True, kw_only=True, eq=False):
    """Live runs of one registered operator class by operator instance."""

    held: dict[bpy.types.Operator, Run[S]] = msgspec.field(default_factory=dict)


class Modal[S](msgspec.Struct, frozen=True, kw_only=True):
    """Modal operator body with preview and rollback."""

    gesture: Gesture
    begin: Callable[[bpy.types.Context, Travel], BlenderResult[S]]
    preview: Callable[[bpy.types.Context, S, Travel], BlenderResult[Option[str]]]
    restore: Callable[[bpy.types.Context, S], BlenderResult[None]]
    tick: Option[Tick[S]] = Nothing
    status: Option[Callable[[bpy.types.UILayout, S], None]] = Nothing
    priority: bool = False
    event: Option[Callable[[bpy.types.Context, S, bpy.types.Event], BlenderResult[tuple[S, "rna_enums.OperatorReturnItems"]]]] = Nothing
    release: Option[Callable[[bpy.types.Context, S], BlenderResult[None]]] = Nothing

    # --- [PLAN]

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Gesture flags with `MODAL_PRIORITY` when `priority` is set."""
        return self.gesture.options | frozenset(("MODAL_PRIORITY",) if self.priority else ())

    def planned[T: msgspec.Struct](self, plan: Plan[T], body: "Body[T, S]", /) -> BlenderResult[Plan[T]]:
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

        opening: Opening[T] = lambda operator, context, _manager, event, _record: self.began(runs, operator, context, event).map(lambda outcome: {outcome})

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

    def began(self, runs: Runs[S], operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Returns RUNNING_MODAL once begun, faulting before resource acquisition."""

        def started(run: Run[S]) -> "rna_enums.OperatorReturnItems":
            runs.held[operator] = run
            run.manager.modal_handler_add(operator)
            for cursor in self.gesture.cursor.to_list():
                run.window.cursor_modal_set(cursor)
            run.workspace.status_text_set(self._hinted(runs, operator))
            return "RUNNING_MODAL"

        match context:
            case bpy.types.Context(window_manager=bpy.types.WindowManager() as manager, window=bpy.types.Window(workspace=workspace) as window):
                inputs, system = context.preferences.inputs, context.preferences.system  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
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
                return Error((BlenderAbsentFault(owner=repr(context), key="window"),))

    def ended(self, runs: Runs[S], operator: bpy.types.Operator, context: bpy.types.Context, *, cancelled: bool) -> BlenderResult[None]:
        """Releases live run, restoring initial snapshot when cancelled."""

        def released(action: Callable[[], None]) -> BlenderResult[None]:
            return catch(action, exception=ReferenceError)().map_error(lambda error: (BlenderEditFault(request=self.ended.__qualname__, error=error),))

        def restored(run: Run[S], live: tuple[bpy.types.Window, ...]) -> BlenderResult[None]:
            area = next((area for window in live for area in window.screen.areas if area in run.area.to_list()), None)
            region = next((region for held in Option.of_optional(area).to_list() for region in held.regions if region in run.region.to_list()), None)
            with ExitStack() as scope:
                try:
                    scope.enter_context(context.temp_override(window=next(iter(live), None), area=area, region=region))
                except TypeError as error:
                    return Error((BlenderEditFault(request="Context.temp_override", error=error),))
                return self.restore(context, run.initial)

        def closed(run: Run[S]) -> BlenderResult[None]:
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

        return Option.of_optional(runs.held.pop(operator, None)).map(closed).default_value(Ok(None))

    # --- [EVENTS]

    def handled(self, runs: Runs[S], operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Returns event outcome, matching modal items against the keys held before the event."""
        prior = runs.held[operator]
        held = prior.pressed - {event.type} if event.value == "RELEASE" else prior.pressed
        run = runs.held[operator] = msgspec.structs.replace(prior, pressed=held | {event.type} if event.value == "PRESS" else held)

        def previewed(current: Run[S]) -> BlenderResult["rna_enums.OperatorReturnItems"]:
            runs.held[operator] = current
            result = self.preview(context, current.state, current.press.travel(event, snapped=current.snapped))
            for text in result.to_option().to_list():
                for area in current.area.to_list():
                    area.header_text_set(text.to_optional())
            return result.map(lambda _: "RUNNING_MODAL")

        def ticked(tick: Tick[S], timer: bpy.types.Timer) -> BlenderResult["rna_enums.OperatorReturnItems"]:
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
            runs.held[operator] = msgspec.structs.replace(run, state=state)
            return outcome

        if context.window != run.window or any(context.area != area for area in run.area.to_list()) or any(context.region != region for region in run.region.to_list()):
            return Ok("CANCELLED")
        run.workspace.status_text_set(self._hinted(runs, operator))
        for handle in self.event.to_list():
            return handle(context, run.state, event).map(following)
        action = next((item for item in run.manager.keyconfigs.user.keymaps[MODAL_KEYMAP].keymap_items if matched(item, event, held, inverted=context.preferences.inputs.invert_zoom_wheel)), None)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
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

    def _hinted(self, runs: Runs[S], operator: bpy.types.Operator) -> Callable[[bpy.types.Header, bpy.types.Context], None]:
        """Returns status bar callback drawing user chords and run state."""

        def drawn(header: bpy.types.Header, _context: bpy.types.Context) -> None:
            """Draws user modal items in action order, holding no keymap reference."""
            run = runs.held[operator]
            items = (
                item
                for keymap in (run.manager.keyconfigs.user.keymaps[MODAL_KEYMAP],)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
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


# --- [OPERATIONS] -----------------------------------------------------------------------


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


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gesture", "Modal", "Press", "Run", "Runs", "Tick", "Travel", "matched"]
