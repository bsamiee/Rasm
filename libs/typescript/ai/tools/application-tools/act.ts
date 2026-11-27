import type { Source } from '@rasm/applications/description';
import { InterfaceMap } from '@rasm/applications/map';
import { Act as Wire } from '@rasm/applications/server/act';
import type { Capture } from '@rasm/applications/server/capture';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Instance } from '@rasm/applications/server/instance';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Array, ByteSize, Context, Duration, Effect, Equal, Formatter, Function, HashMap, HashSet, Match, Number, Option, Predicate, Record, Ref, Result, Schema, SchemaTransformation, Stream, String, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { Address } from '#/handle/address';
import { Notice } from '#/handle/notice';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';
import { View } from '#/view';

// --- [TYPES] ---------------------------------------------------------------------------

type Current = Effect.Success<typeof Target.current>;
type Sample = (typeof Wire.Input.Type)['pointer'];
type Delivered = (typeof Wire.Outcome.Type)['outcomes'][number];

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROOFS: Partial<Record<Result.Result.Success<Delivered>['proof'], string>> = { answer: 'accepted', change: 'changed', standing: 'already showing' };
const NEXT: Record.ReadonlyRecord<string, Call> = {
    ...Struct.Record(['notOpen', 'absent', 'undelivered'], new Call({ tool: 'inspect', given: {} })),
    ...Struct.Record(['noChangeSeen', 'unansweredMayHaveRun'], new Call({ tool: 'view', given: { of: 'front' } })),
};

// --- [MODELS] --------------------------------------------------------------------------

const Position = Schema.Union([Address.At, Wire.Input.fields.pointer]);
const seconds = (description: string) =>
    Schema.optionalKey(Schema.Finite.annotate({ description }).pipe(Schema.decodeTo(Wire.Step.cases.hold.fields.hold, SchemaTransformation.transform({ decode: (given: number) => Duration.toMillis(Duration.seconds(given)), encode: (millis: number) => Duration.toSeconds(Duration.millis(millis)) }))));
const timing = {
    hold: seconds('Seconds from down to up, a drag or scroll spread across them'),
    every: seconds('Seconds from one press start to the next'),
    jitter: seconds('Seconds each press start varies by'),
    count: Wire.Step.cases.key.fields.count.annotate({ description: 'Presses, 2 a double click' }),
    edge: Wire.Step.cases.key.fields.edge.annotate({ description: 'The one edge posted, a later step posting the other' }),
};
const bodies = {
    key: Wire.Step.cases.key.mapFields(
        (fields) => ({
            ...Struct.omit(fields, ['from', '_tag']),
            key: fields.key.annotate({ description: 'One character, a key name, or a virtual key code' }),
            at: Schema.optionalKey(Address.At.annotate({ description: 'Element the key goes to, the key window when absent' })),
            ...timing,
        }),
        { unsafePreserveChecks: true },
    ),
    pointer: Wire.Step.cases.pointer.mapFields(
        (fields) => ({
            ...Struct.omit(fields, ['from', 'windows', '_tag']),
            pointer: Position.annotate({ description: 'Element, or [x, y] in pixels of a window image view returned, pen pressure 0 to 1 and tilt x, y -1 to 1 following' }),
            path: Schema.optionalKey(Schema.Array(Position).annotate({ description: 'Positions moved through after the first: a drag, stroke, or hover path' })),
            button: fields.button.annotate({ description: 'left when absent, none hovers' }),
            by: fields.by.annotate({ description: 'Scroll distance [horizontal, vertical]' }),
            unit: fields.unit.annotate({ description: 'pixel when absent' }),
            ...timing,
            in: Schema.optionalKey(Address.At.annotate({ description: 'Window whose image the points count pixels of, where view returned several' })),
        }),
        { unsafePreserveChecks: true },
    ),
    press: Wire.Step.cases.press.mapFields(
        (fields) => ({
            ...Struct.omit(fields, ['from', '_tag']),
            at: Address.At,
            action: fields.action.annotate({ description: 'Action as inspect prints it, press when absent' }),
            to: fields.to.annotate({ description: 'Value set: toggle state, number, ISO 8601 date, text, option label, or window frame [[x, y], [width, height]]; text on the clipboard reference copies it' }),
            range: fields.range.annotate({ description: 'Text selected, [location, length] or the text itself, replaced where to is text' }),
        }),
        { unsafePreserveChecks: true },
    ),
    hold: Wire.Step.cases.hold.mapFields((fields) => ({ modifiers: fields.modifiers.annotate({ description: 'Modifiers held for hold, no key or pointer' }), hold: timing.hold.schema })),
};
const Step = Schema.Union([
    bodies.key.pipe(Schema.decodeTo(Schema.toType(Schema.TaggedStruct('key', bodies.key.fields)), SchemaTransformation.transform({ decode: (given) => ({ ...given, _tag: Wire.Step.cases.key.fields._tag.schema.literal }), encode: Struct.omit(['_tag']) }))),
    bodies.pointer.pipe(Schema.decodeTo(Schema.toType(Schema.TaggedStruct('pointer', bodies.pointer.fields)), SchemaTransformation.transform({ decode: (given) => ({ ...given, _tag: Wire.Step.cases.pointer.fields._tag.schema.literal }), encode: Struct.omit(['_tag']) }))),
    bodies.press.pipe(Schema.decodeTo(Schema.toType(Schema.TaggedStruct('press', bodies.press.fields)), SchemaTransformation.transform({ decode: (given) => ({ ...given, _tag: Wire.Step.cases.press.fields._tag.schema.literal }), encode: Struct.omit(['_tag']) }))),
    bodies.hold.pipe(Schema.decodeTo(Schema.toType(Schema.TaggedStruct('hold', bodies.hold.fields)), SchemaTransformation.transform({ decode: (given) => ({ ...given, _tag: Wire.Step.cases.hold.fields._tag.schema.literal }), encode: Struct.omit(['_tag']) }))),
]).pipe(Schema.toTaggedUnion('_tag'));
const tool = Tool.make('act', {
    description:
        'Runs steps on the target in order until one fails, the rest not run. A step is at (press an element, to sets its value, range selects its text), key, pointer (click, drag, hover, or scroll at an element or at a pixel of a window image view returned), or modifiers with hold. An element is a reference or label path as inspect prints it: one in a closed container is reached by pressing what opens it first, and one an earlier step opens resolves once that step ran. Calls on one handle answer in order',
    parameters: Schema.Struct({
        steps: Schema.Array(Step).check(Schema.isMinLength(1)),
        record: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Record the front window, else what the first step aims at, while the steps run, and return key frames' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false), Context.add(Tool.Strict, true), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const print = (value: Schema.Json): string =>
    Match.value(value).pipe(
        Match.when(Match.string, Format.quoted),
        Match.when(Match.number, (amount) => Format.count(amount)),
        Match.when(
            (held: Schema.Json): held is Schema.JsonArray => Array.isArray(held),
            (held) => held.map(print).join(held.some((each) => Array.isArray(each)) ? ' ' : ','),
        ),
        Match.orElse(Formatter.format),
    );
const echo = (written: Record.ReadonlyRecord<string, Schema.Json>): readonly string[] =>
    Record.collect(written, (key, value) =>
        Match.value({ key, value }).pipe(
            Match.when({ value: (held: Schema.Json) => Array.isArray(held) && Array.isReadonlyArrayEmpty(held) }, Array.empty<string>),
            Match.when({ key: Match.is('to', 'range'), value: Match.string }, ({ value: text }) => [key, `text of ${Format.count(text.length, Outline.nouns.character)}`]),
            Match.orElse(() => [key, print(value)]),
        ),
    ).flat();
const aim = Wire.Step.match({
    key: ({ at }) => Option.fromUndefinedOr(at),
    pointer: ({ pointer }) => Option.liftPredicate(pointer, Predicate.isNumber),
    press: ({ at }) => Option.some(at),
    hold: () => Option.none<number>(),
});
const locate = ({ element, containers }: Parameters<typeof Address.open>[1]) => (element._tag === 'control' ? Option.some({ container: Array.lastNonEmpty<{ readonly locator: string }>(containers).locator, control: element.locator }) : Option.none());
const place = (at: number | Sample, windows: readonly number[]) => ({ at, windows });
const ends = (outcome: Delivered) => Result.isFailure(outcome) || outcome.success.proof === 'exit';
const unread =
    (subject: string) =>
    ({ message }: Error) =>
        new Line({ head: [subject], value: 'unread', facts: [message] });
const read = Effect.fnUntraced(function* (target: Current) {
    const held = Option.filter(yield* Handle.use((handle) => handle.get(Address.Held)), ({ instance }) => instance === target.live.instance);
    return yield* Option.match(held, { onNone: () => Address.read(target), onSome: (last) => Effect.succeed(last) });
});
const resolve = (target: Current, state: Instance['state'], { number, step, label }: { readonly number: number; readonly step: typeof Step.Type; readonly label: readonly string[] }) => {
    const { instance } = target.live;
    const plain = { number, label, opener: false, proving: Option.none<Omit<Parameters<InterfaceMap['prove']>[0], 'outcome'>>(), operation: Option.none<Parameters<Source['operate']>[0]>() };
    const entry = (sent: typeof Wire.Step.Type, stated: Partial<typeof plain> = {}) => ({ ...plain, ...stated, step: sent });
    const refer = (at: string) => Effect.flatMap(Address.reach(target, at), (node) => Address.intern(instance, node));
    return Step.match(step, {
        hold: (held) => Effect.succeed([entry(held)]),
        key: ({ at, ...keyed }) => Effect.map(Effect.forEach(Array.fromNullishOr(at), refer), (refs) => [entry({ ...keyed, ...Option.getOrUndefined(Option.map(Array.head(refs), (ref) => ({ at: ref }))) })]),
        pointer: Effect.fnUntraced(function* ({ in: within, _tag, ...pointing }: typeof Step.cases.pointer.Type) {
            const [viewed, found] = yield* Effect.all([Handle.use((handle) => handle.get(View.Viewed)), Effect.forEach(Array.fromNullishOr(within), (at) => Address.reach(target, at))]);
            const point = View.point(viewed, { instance, state, window: Array.head(found) });
            const sample = ([x, y, ...pen]: Sample) => Result.map(point([x, y]), (pixel) => place([...pixel.at, ...pen], pixel.windows));
            const [first, ...rest] = yield* Effect.forEach(Array.prepend(pointing.path ?? [], pointing.pointer), (at) => (Predicate.isString(at) ? Effect.map(refer(at), (ref) => place(ref, [])) : Effect.fromResult(sample(at))), { concurrency: 'unbounded' });
            const held = yield* read(target);
            const regions = Address.list(held).flatMap(({ element }) => Option.toArray(Option.all({ ref: Option.some(element.ref), locator: Address.route(held, element.ref), frame: Option.fromUndefinedOr(element._tag === 'control' && element.kind === 'region' ? element.frame : undefined) })));
            const canvas = Array.findFirst(regions, ({ ref, frame: [[left, top], [width, height]] }) => (Predicate.isNumber(first.at) ? ref === first.at : first.at[0] >= left && first.at[0] < left + width && first.at[1] >= top && first.at[1] < top + height));
            const pointer = Option.match(canvas, { onNone: () => first.at, onSome: ({ frame: [[left, top], [width, height]] }): number | Sample => (Predicate.isNumber(first.at) ? [left + width / 2, top + height / 2] : first.at) });
            const path = rest.map((each) => each.at);
            const [refs, samples] = Array.partition(path, (each) => (Predicate.isNumber(each) ? Result.succeed(each) : Result.fail(each)));
            const windows = Array.dedupe([first, ...rest].flatMap((each) => each.windows));
            const operation = Predicate.isNumber(pointer) || Array.isReadonlyArrayNonEmpty(refs) ? Option.none() : Option.map(canvas, ({ locator }) => ({ _tag, locator, input: { ...pointing, pointer, path: samples } }));
            return [entry({ _tag, ...pointing, pointer, path, ...(Array.isReadonlyArrayEmpty(windows) ? {} : { windows }) }, { operation })];
        }),
        press: Effect.fnUntraced(function* (pressed: typeof Step.cases.press.Type) {
            const node = yield* Address.reach(target, pressed.at);
            const held = yield* read(target);
            const openers = yield* Effect.fromResult(Address.open(held, node));
            const [ref, ...refs] = yield* Effect.forEach(Array.prepend(openers, node), (each) => Address.intern(instance, each), { concurrency: 'unbounded' });
            const last = Option.flatMap(locate(node), (at) => held.map.route(at));
            const passed = Option.filter(last, Schema.is(Wire.Step.cases.press.fields.from.schema));
            const to = Option.map(Option.liftPredicate(pressed.to, Schema.is(Element.Value)), (value) => ({ to: value }));
            return [
                ...Array.zipWith(openers, refs, (opener, at) =>
                    entry({ _tag: pressed._tag, at }, { opener: true, label: ['at', ...Option.toArray(Option.firstSomeOf([opener.token, opener.path])), 'to reach', pressed.at], proving: Option.map(locate(opener), (found) => ({ at: found, from: 'accessibility' as const })) }),
                ),
                entry(
                    { ...pressed, at: ref, ...Option.getOrUndefined(Option.map(passed, (from) => ({ from }))) },
                    {
                        proving: Option.map(locate(node), (found) => ({ at: found, from: Option.getOrElse(passed, () => 'accessibility' as const) })),
                        operation: Option.map(Address.route(held, ref), (locator) => ({ _tag: pressed._tag, locator, ...Option.getOrUndefined(to) })),
                    },
                ),
            ];
        }),
    });
};
const send = Effect.fnUntraced(function* (target: Current, state: Instance['state'], entries: Array.NonEmptyReadonlyArray<Effect.Success<ReturnType<typeof resolve>>[number]>, record: boolean) {
    const [server, side, linked] = yield* Effect.all([ApplicationServer, View.Side, Address.Linked]);
    const { operation, step } = Array.headNonEmpty(entries);
    const drawn = yield* Effect.transposeOption(Option.map(operation, (payload) => Effect.result(Effect.flatMap(linked(target.row), (from) => from.operate(payload)))));
    const source = Option.filter(Option.firstSomeOf([Option.map(View.front(state), (number): Capture['source'] => ({ _tag: 'windows', numbers: [number] })), Option.map(aim(step), (ref): Capture['source'] => ({ _tag: 'element', ref }))]), () => record);
    const acted = yield* Effect.when(
        server.act({ instance: target.live.instance, steps: Array.map(entries, (each) => each.step), ...Option.getOrUndefined(Option.map(source, (from) => ({ record: { source: from, longestSide: Option.some(side), keyFrameLimit: View.limit } }))) }),
        Effect.succeed(Option.match(drawn, { onNone: Function.constTrue, onSome: (answer) => Result.isFailure(answer) && step._tag === 'pointer' })),
    );
    const outcomes: readonly Delivered[] = Option.match(acted, {
        onNone: () => Option.toArray(drawn).map(Result.map((route) => ({ proof: 'answer' as const, ...(Wire.Route.guards.source(route) ? { route } : {}), tried: [], popUps: [] }))),
        onSome: (held) => held.outcomes,
    });
    const frames = Option.map(
        Option.flatMap(acted, (held) => Option.fromUndefinedOr(held.frames)),
        Result.map(Array.map(({ markers = [], ...frame }) => ({ ...frame, markers: markers.map((marker) => (marker._tag === 'step' ? { ...marker, index: (entries[marker.index]?.number ?? marker.index + 1) - 1 } : marker)) }))),
    );
    return { entries, outcomes, acted, recording: Option.all({ source, frames }) };
});
const round = (target: Current, record: boolean) =>
    Effect.fnUntraced(function* ({ state, rest }: { readonly state: Instance['state']; readonly rest: readonly Parameters<typeof resolve>[2][] }) {
        const resolved = yield* Stream.fromIterable(rest).pipe(
            Stream.mapEffect((each) => Effect.result(resolve(target, state, each))),
            Stream.takeUntil(Result.isFailure),
            Stream.runCollect,
        );
        const [ready, refusals] = Array.partition(resolved, Function.identity);
        const left = Array.drop(rest, ready.length);
        const entries = Array.dedupeWith(ready.flat(), (each, other) => each.opener && other.opener && Equal.equals(each.step, other.step));
        const recording = record && entries.every(({ operation }) => Option.isNone(operation));
        const groups = Array.match(entries, { onEmpty: () => [], onNonEmpty: (held) => Array.groupWith(held, (each, next) => Option.isNone(each.operation) && Option.isNone(next.operation)) });
        const ran = yield* Stream.fromIterable(groups).pipe(
            Stream.mapEffect((group) => send(target, state, group, recording)),
            Stream.takeUntil(({ outcomes }) => outcomes.some(ends)),
            Stream.runCollect,
        );
        const refused = Array.zipWith(left, Array.isReadonlyArrayEmpty(entries) ? refusals : [], (each, failure) => Result.fail({ ...each, failure }));
        const acted = Array.last(Array.getSomes(ran.map((each) => each.acted)));
        const next = { state: Option.match(acted, { onNone: () => state, onSome: (held) => held.state }), rest: left };
        const more = Option.liftPredicate(next, () => Array.isReadonlyArrayNonEmpty(left) && Array.isReadonlyArrayEmpty(refused) && !ran.some(({ outcomes }) => outcomes.some(ends)));
        return [[...ran.map((each) => Result.succeed(each)), ...refused], more] as const;
    });
const diagnose = ({ usage, load }: NonNullable<(typeof Wire.Outcome.Type)['diagnosis']>) => [
    Result.match(usage, {
        onFailure: unread('usage'),
        onSuccess: ({ cause, duration, end, ...spent }) => {
            const kept = Record.filter(spent, (held) => (Duration.isDuration(held) ? !Duration.isZero(held) : held > 0));
            const asserted = Array.getSuccesses([end.powerAssertions]).flat();
            return new Line({
                head: ['usage'],
                value: cause,
                facts: [Format.duration(duration), ...Record.collect(kept, (key, held) => `${String.noCase()(key)}: ${Duration.isDuration(held) ? Format.duration(held) : Format.bytes(ByteSize.bytes(held))}`), ...(Array.isReadonlyArrayEmpty(asserted) ? [] : [`power assertions: ${Format.list(asserted)}`])],
            });
        },
    }),
    Result.match(load, {
        onFailure: unread('load'),
        onSuccess: ({ processorIdle, swapins = 0, swapouts = 0 }) =>
            new Line({ head: ['load'], facts: [...Array.fromNullishOr(processorIdle).map((idle) => `processor idle: ${Format.percent(idle)}`), ...Record.collect(Record.filter({ swapins, swapouts }, Number.isGreaterThan(0)), (key, pages) => `${key}: ${Format.count(pages)}`)] }),
    }),
];
const line = (
    { shown, proven, changed, numbered }: { readonly shown: Address; readonly proven: HashSet.HashSet<number>; readonly changed: boolean; readonly numbered: boolean },
    { number, label }: { readonly number: number; readonly label: readonly string[] },
    outcome: Delivered,
    sent: Option.Option<typeof Wire.Step.Type>,
) => {
    const head = numbered ? [`${number}.`, ...label] : label;
    const name = (ref: number) => Option.match(Address.get(shown, ref), { onNone: Array.empty<string>, onSome: (node) => [...Option.toArray(node.token), ...Option.toArray(Option.map(node.name, Format.quoted))] });
    return Result.match(outcome, {
        onFailure: (failure) => {
            const next = Option.flatMap(Option.liftPredicate(failure, Schema.is(ApplicationFailure)), ({ reason }) => Record.get(NEXT, reason));
            return new Line({ head, value: Option.isSome(sent) ? 'failed' : 'not run', facts: [failure.message], ...Option.getOrUndefined(Option.map(next, (call) => ({ call }))) });
        },
        onSuccess: ({ proof, followed, resolved, surface, route, presentation, popUps, scrolled }) => {
            const pointing = Option.filter(sent, Wire.Step.isAnyOf(['pointer']));
            const wheel = Option.exists(pointing, ({ by }) => by !== undefined);
            const told = (proof === 'answer' && changed) || (proof === 'posting' && followed !== undefined && HashSet.has(proven, followed)) ? 'change' : proof;
            const said = Match.value({ resolved, told, hover: !wheel && Option.exists(pointing, ({ button }) => button === 'none') }).pipe(
                Match.when({ resolved: Match.defined }, ({ resolved: found }) => [found.effect, ...name(found.ref)]),
                Match.when({ told: 'posting', hover: true }, Array.empty<string>),
                Match.orElse(() => [PROOFS[told] ?? told, ...Array.fromNullishOr(surface).flatMap(name)]),
            );
            const drawn = Wire.Route.matchOrElse({ source: ({ member, approximated }) => [`drawn by ${member}`, ...(Array.isReadonlyArrayEmpty(approximated) ? [] : [`without ${Format.list(approximated)}`])], focused: () => ['window focused'] }, Array.empty<string>);
            const closed = Record.filter({ closed: Array.countBy(popUps, (each) => each.closed), 'left open': Array.countBy(popUps, (each) => !each.closed) }, Number.isGreaterThan(0));
            return new Line({
                head,
                ...Array.match(said, { onEmpty: () => ({}), onNonEmpty: (words) => ({ value: words.join(' ') }) }),
                facts: [
                    ...Array.fromNullishOr(scrolled).flatMap((held) => Outline.scroll(held, wheel)),
                    ...Array.fromNullishOr(route).flatMap(drawn),
                    ...Array.fromNullishOr(presentation).flatMap(Instance.Presentation.matchOrElse({ restored: ({ back }) => ['brought forward', back ? 'put back' : 'put back refused'] }, Array.empty<string>)),
                    ...Record.collect(closed, (left, amount) => `${Format.count(amount, Outline.nouns.popUp)} ${left}`),
                ],
            });
        },
    });
};

// --- [COMPOSITION] ---------------------------------------------------------------------

const Act = {
    tool,
    run: Effect.fnUntraced(function* ({ steps, record = false }: Tool.Parameters<typeof tool>) {
        const target = yield* Target.current;
        const { row, live } = target;
        const [server, maps, handle, { call, awaitRunning }] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Handle, ToolCall]);
        const proofs = yield* Ref.make(HashSet.empty<number>());
        yield* server.changes.pipe(
            Stream.filterMap(({ instance, change }) => (instance === live.instance && change._tag === 'proven' ? Result.succeed(change.followed) : Result.failVoid)),
            Stream.runForEach((followed) => Ref.update(proofs, HashSet.add(followed))),
            Effect.forkScoped({ startImmediately: true }),
        );
        const before = yield* read(target);
        const numbered = yield* Effect.forEach(steps, (step, at) => Effect.map(Effect.orDie(Schema.encodeEffect(Step)(step)), (written) => ({ number: at + 1, step, label: echo(written) })));
        const rounds = yield* Stream.runCollect(Stream.paginate({ state: live.state, rest: Array.fromIterable(numbered) }, round(target, record)));
        const [runs, refused] = Array.partition(rounds, Function.identity);
        const ran = runs.flatMap(({ entries, outcomes }) => Array.zipWith(entries, outcomes, (entry, outcome) => ({ ...entry, outcome })));
        const acted = Array.getSomes(runs.map((each) => each.acted));
        const recordings = Array.getSomes(runs.map((each) => each.recording));
        const state = Option.match(Array.last(acted), { onNone: () => live.state, onSome: (held) => held.state });
        const delivered = Array.getSuccesses(ran.map((each) => each.outcome));
        const answering = Instance.State.matchOrElse(state, { running: ({ status }) => status._tag === 'responding' }, Function.constFalse);
        const unanswered = delivered.some(({ proof }) => proof === 'unanswered');
        const touched = new Set([...ran.flatMap(({ step }) => Option.toArray(aim(step))), ...delivered.flatMap(({ resolved }) => Array.fromNullishOr(resolved?.ref))]);
        const after = yield* Effect.result(Effect.when(Address.read(target), Effect.succeed(Array.isReadonlyArrayNonEmpty(delivered) && answering)));
        const now = Result.getSuccess(after).pipe(
            Option.flatten,
            Option.getOrElse(() => before),
        );
        const changes = after.pipe(Result.map(Option.map((held) => Outline.changes(before, held, touched))));
        const proven = yield* Ref.get(proofs);
        yield* maps.prove(
            live,
            ran.flatMap(({ proving, operation, outcome }) => (Option.isSome(operation) ? [] : Option.toArray(proving).map((held) => ({ ...held, outcome })))),
        );
        const owed = ran.flatMap(({ number, label, outcome }) => Array.fromNullishOr(Result.getOrUndefined(outcome)?.followed).flatMap((followed) => (HashSet.has(proven, followed) ? [] : [Tuple.make({ instance: live.instance, followed }, { call, step: number, head: label })])));
        yield* handle.update(Notice.Proving, HashMap.union(HashMap.fromIterable(owed)));
        yield* awaitRunning;
        const subject = yield* Outline.subject(row.name, { instance: live.instance, state }, { troubled: unanswered || ran.some(({ outcome }) => Result.isFailure(outcome)), keyed: ran.some(({ step }) => step._tag === 'key' || (step._tag === 'press' && Predicate.isString(step.to))) });
        const clipboard = yield* Effect.forEach(Option.toArray(Array.findLast(acted, ({ pasteboard }) => Option.fromUndefinedOr(pasteboard))), (taken) =>
            Effect.match(
                Effect.flatMap(Effect.fromResult(taken), (pasteboard) => Outline.clipboard(live.instance, pasteboard, false)),
                { onFailure: (failure) => [unread('clipboard')(failure)], onSuccess: Function.identity },
            ),
        );
        const frames = yield* View.attach(recordings.flatMap(({ source, frames: taken }) => Result.match(taken, { onFailure: Array.empty, onSuccess: (held) => View.frames({ instance: live.instance, state }, source, held) })));
        const written = acted.flatMap((held) => Array.fromNullishOr(held.written));
        const seen = { shown: now, proven, changed: Result.getSuccess(changes).pipe(Option.flatten, Option.exists(Array.isReadonlyArrayNonEmpty)), numbered: steps.length > 1 };
        const reached = Array.last([...ran.flatMap(({ opener, number }) => (opener ? [] : [number])), ...refused.map(({ number }) => number)]);
        const unrun = Array.drop(numbered, Option.getOrUndefined(reached) ?? 0);
        return new Report({
            lines: [
                ...subject,
                ...ran.map((each) => line(seen, each, each.outcome, Option.some(each.step))),
                ...refused.map((each) => line(seen, each, Result.fail(each.failure), Option.none())),
                ...Array.match(unrun, { onEmpty: Array.empty, onNonEmpty: (rest) => [new Line({ head: ['not run'], value: Format.list(rest.map(({ number, label }) => ['step', `${number}`, ...label].join(' '))) })] }),
                ...Result.match(changes, {
                    onFailure: (failure) => [unread('changes')(failure)],
                    onSuccess: Option.match({ onNone: Array.empty<Line>, onSome: (lines) => (Array.isReadonlyArrayEmpty(lines) ? [new Line({ head: ['nothing shown changed'] })] : lines) }),
                }),
                ...(answering && !unanswered ? [] : Option.toArray(Option.flatMapNullishOr(Array.last(acted), (held) => held.diagnosis)).flatMap(diagnose)),
                ...Array.dedupe(Array.getFailures(written).map(unread('written'))),
                ...Array.match(Array.getSuccesses(written).flat(), { onEmpty: Array.empty, onNonEmpty: (urls) => [new Line({ head: ['written'], value: Format.list(urls.map((url) => Format.quoted(Format.path(url)))) })] }),
                ...clipboard.flat(),
                ...Array.getFailures(recordings.map((each) => each.frames)).map(unread('recording')),
                ...frames,
                ...(record && Array.isReadonlyArrayNonEmpty(runs) && Array.isReadonlyArrayEmpty(recordings) ? [new Line({ head: ['not recorded'], value: ran.some(({ operation }) => Option.isSome(operation)) ? 'a linked source ran a step' : 'no window shown and no element aimed at' })] : []),
            ],
        });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Act };
