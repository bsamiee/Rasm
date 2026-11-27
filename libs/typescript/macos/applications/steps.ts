import { Predicate, Schema, Struct } from 'effect';
import { Recording } from '#/capture';
import { InstanceState, Pasteboard, Written } from '#/instances';
import { Frame, KeyStep, Point, Presentation, Ref, Value, WindowID } from '#/interface';
import { Load, UsageInterval } from '#/processes';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPLY_DEADLINE = 120_000;
const PLANE = 2_147_483_647;
const LIST = new Intl.ListFormat('en', { type: 'conjunction' });

// --- [MODELS] --------------------------------------------------------------------------

const Milliseconds = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: REPLY_DEADLINE })).annotate({ description: 'Milliseconds up to the 120 s reply deadline' });
const PenPressure = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 })).annotate({ description: 'Pressure' });
const Tilt = Schema.Finite.check(Schema.isBetween({ minimum: -1, maximum: 1 }));
const Sample = Schema.Union([Point, Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure]), Schema.Tuple([Schema.Finite, Schema.Finite, PenPressure, Tilt.annotate({ description: 'Tilt x' }), Tilt.annotate({ description: 'Tilt y' })])]).annotate({
    description: 'Point with the pressure from 0 to 1 and tilt from -1 to 1 of a pen stroke',
});
const paced = Schema.makeFilter(
    ({ hold = 0, every, jitter = 0, count = 1 }: { readonly hold?: number; readonly every?: number; readonly jitter?: number; readonly count?: number }) => (every ?? hold + jitter) >= hold + jitter && (every !== undefined || jitter === 0) && (count - 1) * (every ?? 0) + hold <= REPLY_DEADLINE,
    { expected: 'every at least the hold plus jitter, jitter only with every, and every press inside the 120 s reply deadline' },
);
const pointed = Schema.makeFilter(
    ({ pointer, path = [], button, by, count = 1 }: { readonly pointer?: unknown; readonly path?: readonly unknown[]; readonly button?: string; readonly by?: readonly [number, number]; readonly count?: number }) => {
        const points = [pointer, ...path].filter((position): position is readonly number[] => globalThis.Array.isArray(position));
        const pens = new Set(points.map((point) => point.length > 2));
        const planar = points.every(([x = 0, y = 0]) => Math.abs(x) <= PLANE && Math.abs(y) <= PLANE);
        const reach = by === undefined ? 1 : Math.round(Math.max(Math.abs(by[0]), Math.abs(by[1])));
        return planar && reach >= 1 && reach < PLANE && (path.length === 0 || (by === undefined && count === 1)) && (!pens.has(true) || (points.length === path.length + 1 && !pens.has(false) && by === undefined && (button ?? 'left') === 'left'));
    },
    { expected: 'points and a by of at least one whole unit inside ±2³¹, a path only on one press with no by, and pressure or tilt on every point of a left-button stroke or on none' },
);
const ranged = Schema.makeFilter(({ to, range }: { readonly to?: unknown; readonly range?: unknown }) => range === undefined || to === undefined || Predicate.isString(to), { expected: 'a range replaced by text alone' });
const timing = {
    hold: Schema.optionalKey(Milliseconds.annotate({ description: 'Held between down and up, a drag or scroll spread across it, a key repeating past the key repeat delay' })),
    count: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: REPLY_DEADLINE })).annotate({ description: 'Presses, the click state counting up where they fall inside the double-click interval' })),
    every: Schema.optionalKey(Milliseconds.annotate({ description: 'Start to start of presses, when absent the hold with the double-click interval for a pointer or the key repeat interval for a key' })),
    jitter: Schema.optionalKey(Milliseconds.annotate({ description: 'Random spread of each press start around every' })),
    edge: Schema.optionalKey(Schema.Literals(['down', 'up']).annotate({ description: 'The one edge posted, a later step posting the other: a press held across steps' })),
};
const taking =
    (step: string) =>
    <S extends Schema.Top & { readonly fields: Schema.Struct.Fields }>(struct: S) =>
        struct.annotate({ messageUnexpectedKey: `is not a key of ${step}, which takes ${LIST.format(Struct.keys(struct.fields))}` });
const input = <Position extends Schema.Top>(position: Position) =>
    Schema.Struct({
        pointer: position.annotate({ description: 'Element pressed at its activation point or center, or a point' }),
        path: Schema.optionalKey(Schema.Array(position).annotate({ description: 'Points the pointer moves through after the first: a drag, a stroke, or a hover path' })),
        button: Schema.optionalKey(Schema.Literals(['left', 'right', 'middle', 'none']).annotate({ description: 'left when absent, none a hover' })),
        by: Schema.optionalKey(Point.annotate({ description: 'Scroll by horizontal and vertical distance, at least one whole unit' })),
        unit: Schema.optionalKey(Schema.Literals(['pixel', 'line']).annotate({ description: 'Unit of by, pixel when absent' })),
        modifiers: KeyStep.fields.modifiers,
        ...timing,
    })
        .check(paced, pointed)
        .annotate({ description: 'Pointer step: a press, drag, stroke, hover, or scroll' })
        .pipe(taking('a pointer step'));
const steps = <Target extends Schema.Top, Fields extends Schema.Struct.Fields, Pointing extends Schema.Struct.Fields>(target: Target, fields: Fields, pointing: Pointing) =>
    Schema.NonEmptyArray(
        Schema.Union([
            Schema.Struct({ ...KeyStep.fields, at: Schema.optionalKey(target.annotate({ description: 'Element the key goes to, focused through Accessibility or a click, else the key window' })), ...timing, ...fields })
                .check(paced)
                .pipe(taking('a key step')),
            input(Schema.Union([target, Sample]))
                .mapFields((held) => ({ ...held, ...pointing, ...fields }), { unsafePreserveChecks: true })
                .pipe(taking('a pointer step')),
            Schema.Struct({
                at: target,
                action: Schema.optionalKey(
                    Schema.String.check(Schema.makeFilter((named: string) => named !== 'AXRaise', { expected: 'an action other than AXRaise, which takes focus from the frontmost application' })).annotate({
                        description: 'Element action by name, AXPress where no to or range is given',
                    }),
                ),
                to: Schema.optionalKey(
                    Schema.Union([Value, Frame]).annotate({
                        description: 'Value the control takes by its kind, read back once set: a toggle state, number, ISO 8601 date, text, option label of a choice, or frame of a window',
                    }),
                ),
                range: Schema.optionalKey(Schema.Union([Schema.Tuple([Schema.Natural, Schema.Natural]), Schema.String]).annotate({ description: 'Text selected, by location and length or by the text it holds, replaced by to where to is text' })),
                ...fields,
            })
                .check(ranged)
                .pipe(taking('a step at an element')),
        ]),
    ).annotate({ description: 'Run in order until one fails, every step refused before any runs where one is malformed' });
const Route = Schema.TaggedUnion({
    source: {
        member: Schema.String.annotate({ description: 'Host member that drew the step on its canvas, with the tool it drew with' }),
        approximated: Schema.Array(Schema.String).annotate({ description: 'What of the step the host member does not take, such as pressure drawn as a taper, tilt, timing, or modifiers' }),
    },
    process: {},
    window: { number: WindowID },
    focused: { number: WindowID },
    system: {},
}).annotate({
    description:
        'Route the events of the step took: source, a host member drew it on the canvas of a linked source with no event, pointer, or activation; process, posted to the process, which sends keys to its key window; window, posted to the window with its number and window-local location; focused, the window route between focus and defocus records; system, posted at the HID tap to the frontmost process, the pointer put back after',
});
const Rung = Schema.Literals(['source', 'accessibility', 'process', 'window', 'focused', 'system']).annotate({
    description:
        'Route of a step in the order a step tries them: the host member of a linked source drawing on its canvas, an Accessibility call, events posted to the process or to the window, the window events between focus and defocus records, then events at the HID tap, which bring the target forward and put the application in front before it back; a step moves on only when a route refuses',
});
const Scrolled = Schema.Struct({
    horizontal: Schema.optionalKey(Schema.Finite.annotate({ description: 'Horizontal scroll bar value after the step, 0 at the start of the content and 1 at its end' })),
    vertical: Schema.optionalKey(Schema.Finite.annotate({ description: 'Vertical scroll bar value after the step, 0 at the top of the content and 1 at its end' })),
    rows: Schema.optionalKey(Schema.Int.annotate({ description: 'Rows the scrolled list holds' })),
    showing: Schema.Array(Schema.Struct({ index: Schema.Int, label: Schema.optionalKey(Schema.String) })).annotate({
        description: 'Rows now in view, each by its index from 0 and the first text it shows',
    }),
});
const Delivery = Schema.Struct({
    proof: Schema.Literals(['answer', 'change', 'value', 'limit', 'posting', 'unanswered', 'exit']).annotate({
        description:
            "What proved the step: answer, an Accessibility call answered success; change, an event of the application, the aimed window's image changing, or a new window of the target after the posted events; value, the attribute or the scroll position read back; limit, the scroll area already at the end of its range that way, so nothing ran; posting, events delivered with nothing to read back (a hover, a held edge) or a route that delivered while nothing shown changed, so no more intrusive route ran; unanswered, no answer within the messaging timeout, so the step can have run; exit, the instance exited during the step, which ends the run",
    }),
    window: Schema.optionalKey(WindowID.annotate({ description: 'Window the step reached' })),
    route: Schema.optionalKey(Route.annotate({ description: 'Route its events took, absent where an Accessibility call alone proved the step' })),
    tried: Schema.Array(Schema.Struct({ rung: Rung, failure: Schema.String })).annotate({ description: 'Routes tried before the proving one, each with why it refused' }),
    presentation: Schema.optionalKey(Presentation.annotate({ description: 'How the step presented the menu it opened or the target it reached at the HID tap, absent where it stayed behind with no menu' })),
    pointer: Schema.optionalKey(Point.annotate({ description: 'Screen point the events of the step left the posted pointer at' })),
    popUps: Schema.Array(Schema.Struct({ window: WindowID, closed: Schema.Boolean })).annotate({
        description: 'Windows at the pop-up menu level or above the step opened behind the frontmost application, each closed before act returned and read back gone, else left open; a call whose last step is inside one leaves it open',
    }),
    scrolled: Schema.optionalKey(Scrolled.annotate({ description: 'Where a scroll left the content: its scroll bar values and the rows now in view' })),
    resolved: Schema.optionalKey(Schema.Struct({ ref: Ref, effect: Schema.Literals(['pressed', 'selected', 'focused']) }).annotate({ description: 'Element a pointer step at a point resolved to through Accessibility, with what the Accessibility call did to it' })),
    followed: Schema.optionalKey(Schema.Int.annotate({ description: 'Number the watch follows a step by after it ended as posted; a later proven event carrying it says the step took effect' })),
});
const Act = {
    Rung,
    Route,
    Input: input(Sample),
    steps,
    Result: Schema.Struct({
        outcomes: Schema.Array(Schema.Result(Delivery, Schema.ErrorInstance())).annotate({ description: 'One per step run, the first failure ending the run' }),
        state: InstanceState.annotate({ description: 'State once the instance answered: responding, unresponsive with its main thread state (stopped, traced, or leaving an Accessibility read unanswered within the messaging timeout), or quiet for one beat' }),
        usage: Schema.optionalKey(Schema.Result(UsageInterval, Schema.ErrorInstance()).annotate({ description: 'Growth of the instance usage over the steps and its cause, else the read failure; present only where the instance has not answered or a step failed or went unanswered' })),
        load: Schema.optionalKey(Schema.Result(Load, Schema.ErrorInstance()).annotate({ description: 'System load over the steps, else the read failure; present only where the instance has not answered or a step failed or went unanswered' })),
        written: Schema.optionalKey(Written.annotate({ description: 'Documents the instance holds open, by AXDocument and the open command, that FSEvents saw written during the steps, else the watch failure; absent for steps a linked source runs' })),
        pasteboard: Schema.optionalKey(
            Schema.Result(Pasteboard, Schema.ErrorInstance()).annotate({
                description:
                    'General pasteboard whose change count rose during the steps, where this server wrote the contents (a step with to on the clipboard reference) or their plain text is the text the instance shows selected, else the read failure; absent where nothing changed or neither holds, as the pasteboard server names the writer of its contents only to a process holding an Apple entitlement',
            }),
        ),
        recording: Schema.optionalKey(Schema.Result(Recording.Result, Schema.ErrorInstance()).annotate({ description: 'Key frames of the recorded source from before the first step until the instance answered, else why recording failed while the steps ran; absent when not asked' })),
    }),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Act };
