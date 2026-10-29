"""Modal invoke form with gesture, preview, timer, rollback, and status hints read from the user's modal keymaps."""

from collections.abc import Callable, Generator
from contextlib import contextmanager, ExitStack
from enum import Enum
from functools import partial
from types import MappingProxyType
from typing import ClassVar, Final, TYPE_CHECKING

import bpy
from expression import Error, Nothing, Ok, Option, Result
from expression.collections import Block
from expression.extra.result import catch
import msgspec

from blender.bpy.commands.operators import Body, Notes, Returns
from blender.bpy.extension.registration import Acquisitions, Band
from blender.bpy.extension.reports import rendered
from blender.records.faults import accumulated, Fault, Faults, Outcome, paired, Reason

if TYPE_CHECKING:
    from bpy.stub_internal import rna_enums

# --- [TYPES] ----------------------------------------------------------------------------


class Gesture(Enum):
    """Pointer movement with the modal cursor it shows and the cursor grab flag it declares."""

    DRAG = ("SCROLL_XY", "GRAB_CURSOR")
    DRAG_X = ("SCROLL_X", "GRAB_CURSOR_X")
    DRAG_Y = ("SCROLL_Y", "GRAB_CURSOR_Y")
    TRACK = ("CROSSHAIR", None)
    TIMED = (None, None)

    def __init__(self, cursor: "rna_enums.WindowCursorItems | None", grab: "rna_enums.OperatorTypeFlagItems | None") -> None:
        """Holds the modal cursor and the grab flag, Nothing where the gesture shows or declares none."""
        self.cursor: Option[rna_enums.WindowCursorItems] = Option.of_optional(cursor)
        self.grab: Option[rna_enums.OperatorTypeFlagItems] = Option.of_optional(grab)

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Blocking and grab operator flags."""
        return frozenset(("BLOCKING", *self.grab.to_list()) if self.cursor.is_some() else self.grab.to_list())


# --- [CONSTANTS] ------------------------------------------------------------------------

_STANDARD: Final = "Standard Modal Map"
_TRANSFORM: Final = "Transform Modal Map"
_MODIFIERS: Final = (("LEFT_SHIFT", "RIGHT_SHIFT"), ("LEFT_CTRL", "RIGHT_CTRL"), ("LEFT_ALT", "RIGHT_ALT"), ("OSKEY",), ("HYPER",))
_BUTTONS: Final = frozenset({"LEFTMOUSE", "MIDDLEMOUSE", "RIGHTMOUSE", "BUTTON4MOUSE", "BUTTON5MOUSE", "BUTTON6MOUSE", "BUTTON7MOUSE"})
_WHEELS: Final = MappingProxyType({
    ("WHEELINMOUSE", False): "WHEELUPMOUSE",
    ("WHEELINMOUSE", True): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", False): "WHEELDOWNMOUSE",
    ("WHEELOUTMOUSE", True): "WHEELUPMOUSE",
})

# --- [MODELS] ---------------------------------------------------------------------------


class Travel(msgspec.Struct, frozen=True, kw_only=True):
    """Pointer travel since press with tablet pressure and tilt, precise while a user precision key is held."""

    x: int
    y: int
    dx: int
    dy: int
    dragged: bool
    precise: bool
    snapped: bool
    pressure: float
    tilt: tuple[float, float]


class Press(msgspec.Struct, frozen=True, kw_only=True):
    """Press position and device drag threshold in window pixels."""

    x: int
    y: int
    threshold: int

    def travel(self, event: bpy.types.Event, *, snapped: bool, precise: bool) -> Travel:
        """Returns travel from press to event."""
        dx, dy = event.mouse_x - self.x, event.mouse_y - self.y
        return Travel(
            x=event.mouse_region_x,
            y=event.mouse_region_y,
            dx=dx,
            dy=dy,
            dragged=max(abs(dx), abs(dy)) > self.threshold,
            precise=precise,
            snapped=snapped,
            pressure=event.pressure,
            tilt=(event.tilt[0], event.tilt[1]),
        )


class Tick[S](msgspec.Struct, frozen=True, kw_only=True):
    """Timer interval and state step, Nothing finishing, with the done fraction of a state the host progress cursor shows."""

    interval: float
    body: Callable[[bpy.types.Context, S, float], Outcome[Option[S]]]
    progress: Option[Callable[[S], float]] = Nothing


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
    """Modal invoke form with preview and rollback, asking for no input before the body, which runs once the run applies."""

    asks: ClassVar[bool] = False
    gesture: Gesture
    begin: Callable[[bpy.types.Context, Travel], Outcome[S]]
    preview: Callable[[bpy.types.Context, S, Travel], Outcome[Option[str]]]
    restore: Callable[[bpy.types.Context, S], Outcome[None]]
    tick: Option[Tick[S]] = Nothing
    status: Option[Callable[[bpy.types.UILayout, S], None]] = Nothing
    priority: bool = False
    event: Option[Callable[[bpy.types.Context, S, bpy.types.Event], Outcome[tuple[S, "rna_enums.OperatorReturnItems"]]]] = Nothing
    release: Option[Callable[[bpy.types.Context, S], Outcome[None]]] = Nothing

    @property
    def options(self) -> frozenset["rna_enums.OperatorTypeFlagItems"]:
        """Gesture flags with `MODAL_PRIORITY` when `priority` is set."""
        return self.gesture.options | frozenset(("MODAL_PRIORITY",) if self.priority else ())

    def classed[T: bpy.types.PropertyGroup](self, body: Body[T], base: type[bpy.types.Operator], /) -> Outcome[Acquisitions]:
        """Derives the class with gesture flags, modal and cancel members, and the run opening, with a release cancelling live runs at unregistration."""
        runs: dict[bpy.types.Operator, Run[S]] = {}

        def proposed(item: bpy.types.KeyMapItem, /) -> str:
            """Returns the modal action a modal keymap item names, which the stubs type as the placeholder item of its per-keymap enum."""
            return item.propvalue

        def modal_map(manager: bpy.types.WindowManager, name: str, /) -> Option[bpy.types.KeyMap]:
            """Returns a modal keymap of the user keyconfig, which Python operators read and cannot replace."""
            user: bpy.types.KeyConfig = manager.keyconfigs.user  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            return Option.of_optional(user.keymaps.get(name))

        def active(manager: bpy.types.WindowManager, name: str, /) -> Block[bpy.types.KeyMapItem]:
            """Returns the active items of a user modal keymap in keymap order."""
            return Block.of_seq(item for keymap in modal_map(manager, name).to_list() for item in keymap.keymap_items if bool(item.active))

        def precision(manager: bpy.types.WindowManager, event: bpy.types.Event, pressed: frozenset[str], /) -> bool:
            """Checks whether a key of the user's transform precision items is held, a modifier key held as its event flag reports."""
            flagged = frozenset(key for down, keys in zip((event.shift, event.ctrl, event.alt, event.oskey, event.hyper), _MODIFIERS, strict=True) if down for key in keys)
            return active(manager, _TRANSFORM).filter(lambda item: proposed(item) == "PRECISION" and item.type in pressed | flagged).try_head().is_some()

        def hinted(operator: bpy.types.Operator, header: bpy.types.Header, _context: bpy.types.Context) -> None:
            """Draws the status bar of a live run: user chords in action order, the user precision chord on a grabbed drag, and run state."""
            run = runs[operator]
            layout: bpy.types.UILayout = header.layout  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            items = (
                *(
                    item
                    for keymap in modal_map(run.manager, _STANDARD).to_list()
                    for action in keymap.modal_event_values
                    if (action.identifier == "CANCEL" if self.gesture is Gesture.TIMED else action.identifier != "SNAP_OFF")
                    for item in active(run.manager, _STANDARD).filter(lambda item: proposed(item) == action.identifier).try_head().to_list()
                ),
                *(active(run.manager, _TRANSFORM).filter(lambda item: proposed(item) == "PRECISION").try_head().to_list() if self.gesture.grab.is_some() else ()),
            )
            for item in items:
                layout.template_event_from_keymap_item(item, text=bpy.types.UILayout.enum_item_name(item, "propvalue", item.propvalue))
            for draw in self.status.to_list():
                draw(layout, run.state)

        def began(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome["rna_enums.OperatorReturnItems"]:
            """Returns RUNNING_MODAL once the host holds the modal handler, faulting before any resource is acquired and restoring what `begin` wrote when the host refuses the handler."""

            def started(manager: bpy.types.WindowManager, window: bpy.types.Window, workspace: bpy.types.WorkSpace, press: Press, state: S) -> Outcome["rna_enums.OperatorReturnItems"]:
                if not manager.modal_handler_add(operator):
                    return paired(Result[None, Faults].Error(Fault.of(Reason.FAILED, "WindowManager.modal_handler_add")), self.restore(context, state)).map(lambda _: "CANCELLED")
                runs[operator] = Run(
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
                for cursor in self.gesture.cursor.to_list():
                    window.cursor_modal_set(cursor)
                for progress in self.tick.bind(lambda tick: tick.progress).to_list():
                    manager.progress_begin(0.0, 1.0)
                    manager.progress_update(progress(state))
                workspace.status_text_set(partial(hinted, operator))
                return Ok("RUNNING_MODAL")

            match context:
                case bpy.types.Context(window_manager=bpy.types.WindowManager() as manager, window=bpy.types.Window(workspace=workspace) as window):
                    preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
                    inputs, system = preferences.inputs, preferences.system
                    threshold = (inputs.drag_threshold_tablet if event.is_tablet else inputs.drag_threshold_mouse) if event.type in _BUTTONS else inputs.drag_threshold
                    x, y = (event.mouse_prev_press_x, event.mouse_prev_press_y) if event.value == "CLICK_DRAG" else (event.mouse_x, event.mouse_y)
                    press = Press(x=x, y=y, threshold=int(threshold * system.ui_scale))
                    return self.begin(context, press.travel(event, snapped=False, precise=precision(manager, event, frozenset()))).bind(partial(started, manager, window, workspace, press))
                case _:
                    return Error(Fault.of(Reason.ABSENT, repr(context), member="window"))

        def ended(operator: bpy.types.Operator, context: bpy.types.Context, *, cancelled: bool) -> Outcome[None]:
            """Releases live run, restoring initial snapshot when cancelled."""

            def released(action: Callable[[], None]) -> Outcome[None]:
                return catch(action, exception=ReferenceError)().map_error(partial(Fault.of, Reason.REFUSED, ended.__qualname__))

            def restored(run: Run[S], live: tuple[bpy.types.Window, ...]) -> Outcome[None]:
                area = run.area.filter(lambda held: any(held == area for window in live for area in window.screen.areas))
                region = run.region.filter(lambda held: any(held == region for found in area.to_list() for region in found.regions))
                with ExitStack() as scope:
                    entered = catch(scope.enter_context, exception=TypeError)(
                        context.temp_override(window=Block.of_seq(live).try_head().to_optional(), area=area.to_optional(), region=region.to_optional())
                    ).map_error(partial(Fault.of, Reason.REFUSED, "Context.temp_override"))
                    return Result[object, Faults].bind(entered, lambda _: self.restore(context, run.initial))

            def closed(run: Run[S]) -> Outcome[None]:
                live = tuple(window for window in run.manager.windows if window == run.window)
                cleanup = accumulated((
                    *(release(context, run.state) for release in self.release.to_list()),
                    *(released(window.cursor_modal_restore) for _ in self.gesture.cursor.to_list() for window in live),
                    released(lambda: run.workspace.status_text_set(None)),
                    *(released(lambda: area.header_text_set(None)) for area in run.area.to_list()),
                    *(released(lambda: run.manager.event_timer_remove(timer)) for _, timer in run.ticking.to_list()),
                    *(released(run.manager.progress_end) for tick, _ in run.ticking.to_list() for _ in tick.progress.to_list()),
                    *(released(lambda: window.cursor_warp(run.press.x, run.press.y)) for window in live if self.gesture.grab.is_some()),
                ))
                return paired(cleanup, restored(run, live) if cancelled else Ok(None)).map(lambda _: None)

            return Option.of_optional(runs.pop(operator, None)).map(closed).default_value(Ok(None))

        def handled(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Outcome["rna_enums.OperatorReturnItems"]:
            """Returns event outcome, matching modal items against the keys held before the event and precision against the keys held after it."""
            prior = runs[operator]
            held = prior.pressed - {event.type} if event.value == "RELEASE" else prior.pressed
            run = runs[operator] = msgspec.structs.replace(prior, pressed=held | {event.type} if event.value == "PRESS" else held)
            precise = precision(run.manager, event, run.pressed)

            def previewed(current: Run[S]) -> Outcome["rna_enums.OperatorReturnItems"]:
                runs[operator] = current
                result = self.preview(context, current.state, current.press.travel(event, snapped=current.snapped, precise=precise))
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
                        lambda following: (
                            following.map(lambda state: progressed(tick, state)).map(lambda state: previewed(msgspec.structs.replace(run, state=state, elapsed=elapsed))).default_value(Ok("FINISHED"))
                        )
                    )
                )

            def progressed(tick: Tick[S], state: S) -> S:
                for progress in tick.progress.to_list():
                    run.manager.progress_update(progress(state))
                return state

            def following(result: tuple[S, "rna_enums.OperatorReturnItems"]) -> "rna_enums.OperatorReturnItems":
                state, outcome = result
                runs[operator] = msgspec.structs.replace(run, state=state)
                return outcome

            if context.window != run.window or any(context.area != area for area in run.area.to_list()) or any(context.region != region for region in run.region.to_list()):
                return Ok("CANCELLED")
            run.workspace.status_text_set(partial(hinted, operator))
            for handle in self.event.to_list():
                return handle(context, run.state, event).map(following)
            preferences: bpy.types.Preferences = context.preferences  # type: ignore[assignment]  # ty: ignore[invalid-assignment]
            action = active(run.manager, _STANDARD).filter(lambda item: matched(item, event, held, inverted=preferences.inputs.invert_zoom_wheel)).try_head().map(proposed)
            match action.to_optional(), event.type:
                case "CANCEL", _:
                    return Ok("CANCELLED")
                case None, "TIMER":
                    return run.ticking.map(lambda ticking: ticked(*ticking)).default_value(Ok("PASS_THROUGH"))
                case _ if self.gesture is Gesture.TIMED:
                    return Ok("PASS_THROUGH")
                case "APPLY", _:
                    return self.preview(context, run.state, run.press.travel(event, snapped=run.snapped, precise=precise)).map(lambda _: "FINISHED")
                case ("SNAP" | "SNAP_OFF") as snap, _:
                    return previewed(msgspec.structs.replace(run, snapped=snap == "SNAP"))
                case None, "MOUSEMOVE" | "INBETWEEN_MOUSEMOVE":
                    return previewed(run)
                case _:
                    return Ok("RUNNING_MODAL")

        def cancelled(operator: bpy.types.Operator, context: bpy.types.Context, entries: Notes) -> Returns:
            return body.noted(operator, (*entries, *ended(operator, context, cancelled=True).map(lambda _: ()).default_with(rendered)), "CANCELLED")

        def returned(operator: bpy.types.Operator, context: bpy.types.Context, outcome: "rna_enums.OperatorReturnItems") -> Returns:
            match outcome:
                case "FINISHED":
                    return body.settled(operator, context, lambda rolled: ended(operator, context, cancelled=rolled))
                case "CANCELLED":
                    return cancelled(operator, context, ())
                case _:
                    return {outcome}

        def stepped(operator: bpy.types.Operator, context: bpy.types.Context, event: bpy.types.Event) -> Returns:
            return handled(operator, context, event).map(lambda outcome: returned(operator, context, outcome)).default_with(lambda faults: cancelled(operator, context, rendered(faults)))

        def cancelling() -> None:
            for operator in tuple(runs):
                cancelled(operator, bpy.context, ())

        @contextmanager
        def released() -> Generator[None]:
            with ExitStack() as stack:
                stack.callback(cancelling)
                yield

        members = {
            "bl_options": base.bl_options | self.options,
            "modal": stepped,
            "cancel": lambda operator, context: cancelled(operator, context, ()),
            "invoke": body.invoking(lambda operator, context, event: began(operator, context, event).map(lambda outcome: {outcome})),
        }
        return Ok((type(base.__name__, (base,), members), (Band.OPERATORS, released())))


# --- [OPERATIONS] -----------------------------------------------------------------------


def matched(item: bpy.types.KeyMapItem, event: bpy.types.Event, held: frozenset[str], /, *, inverted: bool) -> bool:
    """Checks whether active modal item matches event as `wm_eventmatch` does, its key modifier among the keys held and wheel zoom inverted or not."""
    return (
        bool(item.active)
        and (item.repeat or not event.is_repeat)
        and (
            (item.type == "TEXTINPUT" and event.value == "PRESS" and bool(event.unicode))
            or (
                _WHEELS.get((item.type, inverted), item.type) == event.type
                and item.value in {"ANY", event.value}
                and (item.value != "CLICK_DRAG" or item.direction in {"ANY", event.direction})
                and all(
                    state in {-1, int(pressed)} or event.type in keys
                    for state, pressed, keys in zip((item.shift, item.ctrl, item.alt, item.oskey, item.hyper), (event.shift, event.ctrl, event.alt, event.oskey, event.hyper), _MODIFIERS, strict=True)
                )
                and (item.key_modifier == "NONE" or item.key_modifier in held)
            )
        )
    )


# --- [EXPORTS] --------------------------------------------------------------------------

__all__ = ["Gesture", "Modal", "Press", "Run", "Tick", "Travel", "matched"]
