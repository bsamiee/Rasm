"""Modal operator gestures with preview and rollback."""

from collections.abc import Callable
from contextlib import ExitStack
from enum import Enum
from typing import TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Some
from expression.extra.result import catch
import msgspec

from blender.bpy.faults import BlenderAbsentFault, BlenderEditFault, BlenderResult
from blender.imaging.faults import accumulated, paired

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

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
        return frozenset["rna_enums.OperatorTypeFlagItems"]({"BLOCKING"} if self.cursor.is_some() else ()) | frozenset(self.grab.to_list())

    @property
    def relative(self) -> bool:
        """Whether cursor stays grabbed."""
        return self.grab.is_some()


# --- [CONSTANTS] ------------------------------------------------------------------------

MODAL_KEYMAP = "Standard Modal Map"


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
    """Live run with rollback snapshot and held resources."""

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

    # --- [LIFECYCLE]

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Gesture flags with `MODAL_PRIORITY` when `priority` is set."""
        return self.gesture.options | (frozenset["rna_enums.OperatorTypeFlagItems"]({"MODAL_PRIORITY"}) if self.priority else frozenset())

    def began(self, operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Returns RUNNING_MODAL once begun, faulting before resource acquisition."""

        def started(run: Run[S]) -> "rna_enums.OperatorReturnItems":
            vars(operator)["running"] = run
            for cursor in self.gesture.cursor.to_list():
                run.window.cursor_modal_set(cursor)
            run.workspace.status_text_set(self._hinted(operator))
            return "RUNNING_MODAL"

        match context:
            case bpy.types.Context(window_manager=bpy.types.WindowManager() as manager, window=bpy.types.Window(workspace=workspace) as window):
                inputs, system = context.preferences.inputs, context.preferences.system  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
                buttons = ("LEFTMOUSE", "MIDDLEMOUSE", "RIGHTMOUSE", "BUTTON4MOUSE", "BUTTON5MOUSE", "BUTTON6MOUSE", "BUTTON7MOUSE")
                threshold = (inputs.drag_threshold_tablet if event.is_tablet else inputs.drag_threshold_mouse) if event.type in buttons else inputs.drag_threshold
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

    def ended(self, operator: bpy.types.Operator, context: bpy.types.Context, *, cancelled: bool) -> BlenderResult[None]:
        """Releases live run, restoring initial snapshot when cancelled."""
        ending: Option[Run[S]] = Option.of_optional(vars(operator).pop("running", None))

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

        for run in ending.to_list():
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
        return Ok(None)

    # --- [EVENTS]

    def handled(self, operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> BlenderResult["rna_enums.OperatorReturnItems"]:
        """Returns event outcome."""
        run: Run[S] = vars(operator)["running"]

        def previewed(current: Run[S]) -> BlenderResult["rna_enums.OperatorReturnItems"]:
            vars(operator)["running"] = current
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

        if context.window != run.window or any(context.area != area for area in run.area.to_list()) or any(context.region != region for region in run.region.to_list()):
            return Ok("CANCELLED")
        run.workspace.status_text_set(self._hinted(operator))
        for handle in self.event.to_list():

            def following(result: tuple[S, "rna_enums.OperatorReturnItems"]) -> "rna_enums.OperatorReturnItems":
                state, outcome = result
                vars(operator)["running"] = msgspec.structs.replace(run, state=state)
                return outcome

            return handle(context, run.state, event).map(following)
        action = next((item for item in run.manager.keyconfigs.user.keymaps[MODAL_KEYMAP].keymap_items if matched(item, event)), None)  # type: ignore[union-attr]  # ty: ignore[unresolved-attribute]
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

    def _hinted(self, operator: bpy.types.Operator) -> Callable[[bpy.types.Header, bpy.types.Context], None]:
        """Returns status bar callback drawing user chords and run state."""

        def drawn(header: bpy.types.Header, _context: bpy.types.Context) -> None:
            """Draws user modal items in action order, holding no keymap reference."""
            run: Run[S] = vars(operator)["running"]
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


def matched(item: bpy.types.KeyMapItem, event: bpy.types.Event) -> bool:
    """Checks whether active modal item matches event."""
    return (
        item.active  # type: ignore[return-value]  # ty: ignore[invalid-return-type]
        and (item.repeat or not event.is_repeat)
        and item.type == event.type
        and item.value in {"ANY", event.value}
        and (item.value != "CLICK_DRAG" or item.direction in {"ANY", event.direction})
        and all(
            held in {-1, int(pressed)} or event.type in keys
            for held, pressed, keys in (
                (item.shift, event.shift, ("LEFT_SHIFT", "RIGHT_SHIFT")),
                (item.ctrl, event.ctrl, ("LEFT_CTRL", "RIGHT_CTRL")),
                (item.alt, event.alt, ("LEFT_ALT", "RIGHT_ALT")),
                (item.oskey, event.oskey, ("OSKEY",)),
                (item.hyper, event.hyper, ("HYPER",)),
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gesture", "Modal", "Press", "Run", "Tick", "Travel", "matched"]
