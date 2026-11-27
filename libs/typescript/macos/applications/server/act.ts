import { Predicate, Schema, Struct } from 'effect';
import { Capture } from '#/server/capture';
import { Element } from '#/server/element';
import { ApplicationFailure } from '#/server/failure';
import { Instance } from '#/server/instance';

// --- [CONSTANTS] -----------------------------------------------------------------------

const REPLY_DEADLINE = 120_000;
const PLANE = 2_147_483_647;

// --- [MODELS] --------------------------------------------------------------------------

const Milliseconds = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: REPLY_DEADLINE }));
const Pressure = Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 1 }));
const Tilt = Schema.Finite.check(Schema.isBetween({ minimum: -1, maximum: 1 }));
const Sample = Schema.Union([Schema.Tuple([Schema.Finite, Schema.Finite]), Schema.Tuple([Schema.Finite, Schema.Finite, Pressure]), Schema.Tuple([Schema.Finite, Schema.Finite, Pressure, Tilt, Tilt])]);
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
    hold: Schema.optionalKey(Milliseconds),
    count: Schema.optionalKey(Schema.Int.check(Schema.isBetween({ minimum: 1, maximum: REPLY_DEADLINE }))),
    every: Schema.optionalKey(Milliseconds),
    jitter: Schema.optionalKey(Milliseconds),
    edge: Schema.optionalKey(Schema.Literals(['down', 'up'])),
};
const input = <Position extends Schema.Top>(position: Position) =>
    Schema.Struct({
        pointer: position,
        path: position.pipe(Schema.Array, Schema.optionalKey),
        button: Schema.optionalKey(Schema.Literals(['left', 'right', 'middle', 'none'])),
        by: Schema.optionalKey(Schema.Tuple([Schema.Finite, Schema.Finite])),
        unit: Schema.optionalKey(Schema.Literals(['pixel', 'line'])),
        modifiers: Element.Shortcut.fields.modifiers,
        ...timing,
    }).check(paced, pointed);
const Kind = Schema.Literals(['source', 'accessibility', 'process', 'window', 'focused', 'system']);
const Route = Object.assign(Schema.TaggedUnion({ source: { member: Schema.String, approximated: Schema.Array(Schema.String) }, process: {}, window: { number: Element.WindowNumber }, focused: { number: Element.WindowNumber }, system: {} }), { Kind });
const from = Schema.optionalKey(Kind.pick(['accessibility', 'process', 'window', 'focused', 'system']));
const Step = Schema.Union([
    Schema.TaggedStruct('key', { ...Element.Shortcut.fields, at: Schema.optionalKey(Element.Ref), ...timing, from }).check(paced),
    Schema.TaggedStruct('pointer', { ...input(Schema.Union([Element.Ref, Sample])).fields, windows: Element.WindowNumber.pipe(Schema.Array, Schema.optionalKey), from }).check(paced, pointed),
    Schema.TaggedStruct('press', {
        at: Element.Ref,
        action: Schema.optionalKey(Schema.String.check(Schema.makeFilter((named: string) => named !== 'AXRaise', { expected: 'an action other than AXRaise, which takes focus from the frontmost application' }))),
        to: Schema.optionalKey(Schema.Union([Element.Value, Element.Frame])),
        range: Schema.optionalKey(Schema.Union([Schema.Tuple([Schema.Natural, Schema.Natural]), Schema.String])),
        from,
    }).check(ranged),
    Schema.TaggedStruct('hold', { modifiers: Schema.NonEmptyArray(Element.Modifier), hold: Milliseconds }),
]).pipe(Schema.toTaggedUnion('_tag'));
const Delivery = Schema.Struct({
    proof: Schema.Literals(['answer', 'change', 'value', 'limit', 'posting', 'unanswered', 'exit', 'standing']),
    route: Schema.optionalKey(Route),
    tried: Schema.Array(Schema.Struct({ route: Kind, failure: ApplicationFailure.Stated })),
    presentation: Schema.optionalKey(Instance.Presentation),
    popUps: Schema.Array(Schema.Struct({ window: Element.WindowNumber, closed: Schema.Boolean })),
    scrolled: Schema.optionalKey(
        Schema.Struct({ horizontal: Schema.optionalKey(Schema.Finite), vertical: Schema.optionalKey(Schema.Finite), rows: Schema.optionalKey(Schema.Int), showing: Schema.Array(Schema.Struct({ index: Schema.Int, label: Schema.optionalKey(Schema.String) })) }),
    ),
    resolved: Schema.optionalKey(Schema.Struct({ ref: Element.Ref, effect: Schema.Literals(['pressed', 'selected', 'focused']) })),
    followed: Schema.optionalKey(Schema.Int),
    surface: Schema.optionalKey(Element.Ref),
});
const Pasteboard = Schema.Struct({
    locator: Element.Locator,
    types: Schema.Array(Schema.String),
    accessBehavior: Schema.Union([Schema.Literals(['default', 'ask', 'alwaysAllow', 'alwaysDeny']), Schema.Int]),
    detectedPatterns: Schema.Array(Schema.String),
    contentType: Schema.optionalKey(Schema.String),
    content: Schema.optionalKey(Schema.TaggedUnion({ text: { value: Schema.String }, withheld: {}, concealed: {} })),
});
const Written = ApplicationFailure.read(Schema.Array(Schema.URL));
const Act = {
    Route,
    Step,
    Input: input(Sample),
    Pasteboard,
    Written,
    Record: Schema.Struct({ ...Struct.pick(Capture.fields, ['source', 'longestSide']), ...Struct.pick(Capture.fields.mode.cases.recording.fields, ['keyFrameLimit']) }),
    Outcome: Schema.Struct({
        outcomes: Schema.Array(ApplicationFailure.read(Delivery)),
        state: Instance.State,
        diagnosis: Schema.optionalKey(
            Schema.Struct({
                usage: Instance.State.cases.running.fields.usage.schema,
                load: ApplicationFailure.read(Schema.Struct({ processorIdle: Schema.optionalKey(Schema.Finite), swapins: Schema.optionalKey(Schema.Finite), swapouts: Schema.optionalKey(Schema.Finite) })),
            }),
        ),
        written: Schema.optionalKey(Written),
        pasteboard: Schema.optionalKey(ApplicationFailure.read(Pasteboard)),
        frames: Schema.optionalKey(Capture.Frame.pipe(Schema.Array, ApplicationFailure.read)),
    }),
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { Act };
