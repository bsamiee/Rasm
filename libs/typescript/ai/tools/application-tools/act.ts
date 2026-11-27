import type { Description } from '@rasm/applications/description';
import { InterfaceMap } from '@rasm/applications/map';
import { Act as Wire } from '@rasm/applications/server/act';
import type { Capture } from '@rasm/applications/server/capture';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Instance } from '@rasm/applications/server/instance';
import { Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, ByteSize, Cause, Context, Duration, Effect, Equal, Equivalence, Formatter, Function, HashMap, HashSet, Match, Number, Option, Predicate, Record, Ref, Result, Schema, SchemaTransformation, Stream, Struct, Tuple } from 'effect';
import { Tool } from 'effect/ai';
import { Address, type Index, Indexed, type Item, read, Source } from '#/handle/address';
import { Proving } from '#/handle/notice';
import { Outline } from '#/handle/outline';
import { current } from '#/handle/target';
import { attach, FRAMES, frontOf, keyFramesOf, pointOf, Side, Viewed } from '#/view';

// --- [TYPES] ---------------------------------------------------------------------------

type Step = Tool.Parameters<typeof tool>['steps'][number];
type Acted = typeof Wire.Outcome.Type;
type Delivery = Result.Result.Success<Acted['outcomes'][number]>;
type Entry = Effect.Success<ReturnType<ReturnType<typeof prepared>>>[number];
interface Piece {
    readonly entries: readonly Entry[];
    readonly outcomes: Acted['outcomes'];
    readonly acted: Option.Option<Acted>;
    readonly source: Option.Option<Capture['source']>;
    readonly notes: readonly Line[];
    readonly refused: readonly number[];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const PROOFS = { answer: 'accepted', change: 'changed', value: 'value read back', limit: 'at end of range', posting: 'posted, no change seen yet', unanswered: 'unanswered, may have run', exit: 'application exited', standing: 'already showing' } as const satisfies Record<Delivery['proof'], string>;
const CAUSE = { running: 'computing', runnable: 'waiting for a processor', paging: 'paging', reading: 'reading disk', writing: 'writing disk', idle: 'idle' } as const satisfies Record.ReadonlyRecord<NonNullable<(typeof Instance.State.cases.running.Type)['usage']>['cause'], string>;
const TEXTUAL: ReadonlySet<(typeof Element.Control.Type)['kind']> = new Set(['measure', 'text', 'date', 'choice', 'multi', 'color']);
const ALIKE = Array.makeEquivalence(Equivalence.String);

// --- [MODELS] --------------------------------------------------------------------------

const [Keyed, Pointed, Aimed, Held] = Wire.Step.members;
const [, Sample] = Pointed.fields.pointer.members;
const Position = Schema.Union([Address.At, Sample]);
const seconds = (description: string) =>
    Schema.optionalKey(Schema.Finite.annotate({ description }).pipe(Schema.decodeTo(Keyed.fields.hold.schema, SchemaTransformation.transform({ decode: (given: number) => Duration.toMillis(Duration.seconds(given)), encode: (millis: number) => Duration.toSeconds(Duration.millis(millis)) }))));
const paces = {
    hold: seconds('Seconds held between down and up, a drag or scroll spread across them'),
    every: seconds('Seconds from the start of one press to the next'),
    jitter: seconds('Seconds each press start varies around every'),
};
const pressing = <Fields extends Pick<typeof Keyed.fields, 'count' | 'edge'>>(fields: Fields) => ({
    ...paces,
    count: fields.count.annotate({ description: 'Presses, 2 a double click' }),
    edge: fields.edge.annotate({ description: 'The one edge posted, a later step posting the other: a press held across steps' }),
});
const tool = Tool.make('act', {
    description:
        'Runs steps on the target in order until one fails. A step is at (press an element, with to set its value, with range select its text), key, pointer (click, drag, hover, or scroll at an element or at a pixel of a window image view returned), or modifiers with hold. An element is a reference or label path as inspect prints it: one in a closed container runs after the item that opens it, and one an earlier step opens resolves once that step ran. Calls on one handle answer in order',
    parameters: Schema.Struct({
        steps: Schema.NonEmptyArray(
            Schema.Union([
                Keyed.mapFields(
                    (fields) => ({
                        ...Struct.omit(fields, ['from']),
                        key: fields.key.annotate({ description: 'A character, a listed key name, or a virtual key code' }),
                        at: Schema.optionalKey(Address.At.annotate({ description: 'Element the key goes to, the key window when absent' })),
                        ...pressing(fields),
                    }),
                    { unsafePreserveChecks: true },
                ),
                Pointed.mapFields(
                    (fields) => ({
                        ...Struct.omit(fields, ['from', 'windows']),
                        pointer: Position.annotate({ description: 'Element, or point [x, y] in pixels of a window image view returned, pen pressure from 0 to 1 and tilt x and y from -1 to 1 following' }),
                        path: Schema.optionalKey(Schema.Array(Position).annotate({ description: 'Positions the pointer moves through after the first: a drag, a stroke, or a hover path' })),
                        button: fields.button.annotate({ description: 'left when absent, none hovers' }),
                        by: fields.by.annotate({ description: 'Scroll by horizontal and vertical distance, at least one whole unit' }),
                        unit: fields.unit.annotate({ description: 'Unit of by, pixel when absent' }),
                        ...pressing(fields),
                        in: Schema.optionalKey(Address.At.annotate({ description: 'Window whose image a point counts pixels of, named where view returned images of more than one' })),
                    }),
                    { unsafePreserveChecks: true },
                ),
                Aimed.mapFields(
                    (fields) => ({
                        ...Struct.omit(fields, ['from']),
                        at: Address.At,
                        action: fields.action.annotate({ description: 'Action of the element as inspect prints it, a press when absent' }),
                        to: fields.to.annotate({ description: 'Element.Value by the kind of the element: toggle state, number, ISO 8601 date, text, option label of a choice, or frame [[x, y], [width, height]] of a window; text on the clipboard reference copies it' }),
                        range: fields.range.annotate({ description: 'Text selected, by [location, length] or by the text it holds, replaced where to is text' }),
                    }),
                    { unsafePreserveChecks: true },
                ),
                Held.mapFields((fields) => ({ modifiers: fields.modifiers.annotate({ description: 'Modifiers held down for hold with no key or pointer' }), hold: paces.hold.schema })),
            ]),
        ),
        record: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Record the front window, else the element the first step aims at, while the steps run, returning key frames as view with seconds does' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false), Context.add(Tool.Strict, true), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [OPERATIONS] ----------------------------------------------------------------------

const printed = (value: Schema.Json): string =>
    Match.value(value).pipe(
        Match.when(Match.string, Format.quoted),
        Match.when(Match.number, (amount) => Format.count(amount)),
        Match.when(
            (held: Schema.Json): held is Schema.JsonArray => Array.isArray(held),
            (held) => held.map(printed).join(held.some((each) => Array.isArray(each)) ? ' ' : ','),
        ),
        Match.orElse(Formatter.format),
    );
const labelOf = (step: Record.ReadonlyRecord<string, Schema.Json>): readonly string[] =>
    Record.collect(step, (key, value) =>
        Match.value({ key, value }).pipe(
            Match.when({ value: (held: Schema.Json) => Array.isArray(held) && Array.isReadonlyArrayEmpty(held) }, Array.empty<string>),
            Match.when({ key: (name) => Predicate.hasProperty(paces, name), value: Match.number }, ({ value: millis }) => [key, Format.duration(Duration.millis(millis))]),
            Match.when({ key: Match.is('to', 'range'), value: Match.string }, ({ value: text }) => [key, `text of ${Format.count(text.length, Outline.nouns.character)}`]),
            Match.orElse(() => [key, printed(value)]),
        ),
    ).flat();
const aimOf = (step: object) => Option.filter(Option.firstSomeOf(['pointer' in step ? Option.some(step.pointer) : Option.none(), 'at' in step ? Option.fromUndefinedOr(step.at) : Option.none()]), Predicate.isNumber);
const [unmatched, failed] = [Schema.is(Unmatched), Schema.is(ApplicationFailure)];
const sharing = (left: Error) => (right: Error) =>
    (unmatched(left) && unmatched(right) && left.subject === right.subject && ALIKE(left.candidates, right.candidates)) || (failed(left) && failed(right) && left.reason === right.reason && left.message.slice(left.subject.length) === right.message.slice(right.subject.length));
const ending = (outcomes: Acted['outcomes']) => outcomes.some((outcome) => Result.isFailure(outcome) || outcome.success.proof === 'exit');
const piece = (ran: Pick<Piece, 'entries' | 'outcomes'> & Partial<Piece>): Piece => ({ acted: Option.none(), source: Option.none(), notes: [], refused: [], ...ran });
const aimedAt = (at: number | typeof Sample.Type, windows: readonly number[]) => ({ at, windows });
const indexOf = Effect.fnUntraced(function* (aimed: Effect.Success<typeof current>) {
    const last = yield* Handle.use((handle) => handle.get(Indexed));
    return yield* Option.match(
        Option.filter(last, ({ instance }) => instance === aimed.live.instance),
        { onNone: () => Effect.map(read(aimed), Struct.get('base')), onSome: ({ index }) => Effect.succeed(index) },
    );
});
const renumbered = (entries: readonly { readonly number: number }[], { keyFrames, ...taken }: typeof Capture.Recorded.Type) => ({
    ...taken,
    keyFrames: keyFrames.map((frame) => ({ ...frame, markers: frame.markers.map((marker) => (marker._tag === 'step' ? { ...marker, index: (entries[marker.index]?.number ?? marker.index + 1) - 1 } : marker)) })),
});
const diagnose = ({ usage, load }: Pick<Acted, 'usage' | 'load'>) => {
    const spent = Array.getSuccesses(Array.fromNullishOr(usage)).flatMap(({ cause, duration, diskBytesRead, diskBytesWritten, pageWait, runnable, end }) => [
        `${CAUSE[cause]} over ${Format.duration(Duration.seconds(duration))}`,
        ...Record.collect(Record.filter({ read: diskBytesRead, wrote: diskBytesWritten }, Number.isGreaterThan(0)), (did, bytes) => `${did} ${Format.bytes(ByteSize.bytes(bytes))}`),
        ...Record.collect(Record.filter({ 'on page faults': pageWait, 'for a processor': runnable }, Number.isGreaterThan(0)), (on, waited) => `waited ${Format.duration(Duration.seconds(waited))} ${on}`),
        ...Array.match(end.assertions ?? [], { onEmpty: Array.empty, onNonEmpty: (names) => [`declares ${Format.list(names)}`] }),
    ]);
    const loaded = Array.getSuccesses(Array.fromNullishOr(load)).flatMap(({ idle, swapins = 0, swapouts = 0 }) => [...Array.fromNullishOr(idle).map((share) => `processors ${Format.percent(1 - share)} busy`), ...(swapins + swapouts > 0 ? [`${Format.count(swapins + swapouts)} pages swapped`] : [])]);
    return Array.match([...spent, ...loaded], { onEmpty: Array.empty, onNonEmpty: (parts) => [new Line({ head: ['usage'], value: Format.list(parts) })] });
};
const accepts = (item: Item, { to, range }: { readonly to?: typeof Element.Value.Type | readonly unknown[]; readonly range?: unknown }): Result.Result<Item, Cause.IllegalArgumentError> => {
    const options = Match.value(item.target).pipe(
        Match.tag('Control', ({ control }) => Option.map(Option.fromUndefinedOr(control.options), Array.map(Struct.get('label')))),
        Match.tag('RememberedControl', ({ control }) => Option.filter(control.options, (listed): listed is readonly string[] => Array.isArray(listed))),
        Match.orElse(() => Option.none<readonly string[]>()),
    );
    const kind = Option.getOrUndefined(item.kind);
    const taken = Match.value(to).pipe(
        Match.when(Match.undefined, () => range === undefined || kind === 'text'),
        Match.when(Match.boolean, () => kind === 'toggle' || kind === 'row'),
        Match.when(Match.number, () => kind === 'number' || kind === 'measure' || kind === 'text'),
        Match.when(Match.string, (label) => kind !== undefined && TEXTUAL.has(kind) && Option.match(options, { onNone: Function.constTrue, onSome: Array.contains(label) })),
        Match.orElse(() => kind === undefined && item.target._tag !== 'Option'),
    );
    const listed = Option.filter(options, () => Predicate.isString(to));
    const takes = Option.match(listed, { onNone: () => (to === undefined ? 'no range' : 'no such value'), onSome: (labels) => Format.list(labels.map(Format.quoted)) });
    return taken ? Result.succeed(item) : Result.fail(new Cause.IllegalArgumentError([...Option.toArray(item.token), ...Option.toArray(item.name), ...Option.toArray(item.kind), 'takes', takes].join(' ')));
};
const prepared =
    <E, R>(aimed: Effect.Success<typeof current>, map: InterfaceMap, state: Acted['state'], find: (at: string) => Effect.Effect<Item, E, R>) =>
    ({ number, step, label }: { readonly number: number; readonly step: Step; readonly label: readonly string[] }) => {
        const { instance } = aimed.live;
        const refOf = Address.refOf(instance);
        const reach = (at: string) => Effect.flatMap(find(at), refOf);
        const plain = { number, label, opener: Option.none<number>(), at: Option.none<Option.Option.Value<ReturnType<typeof Address.locate>>>(), start: 'accessibility' as typeof Wire.Rung.Type, facet: Option.none<typeof Description.Operation.Type>() };
        return Match.value(step).pipe(
            Match.when(
                { pointer: Match.defined },
                Effect.fnUntraced(function* ({ in: within, ...pointing }: Extract<Step, { readonly pointer: unknown }>) {
                    const [viewed, window] = yield* Effect.all([Handle.use((handle) => handle.get(Viewed)), Effect.transposeOption(Option.map(Option.fromUndefinedOr(within), find))]);
                    const placed = pointOf(viewed, { instance, state, window });
                    const sampled = ([x, y, ...rest]: typeof Sample.Type) => Result.map(placed([x, y]), ({ at: [left, top], windows }) => aimedAt([left, top, ...rest], windows));
                    const aim = (at: typeof Position.Type) => (Predicate.isString(at) ? Effect.map(reach(at), (ref) => aimedAt(ref, [])) : Effect.fromResult(sampled(at)));
                    const [first, ...path] = yield* Effect.forEach(Array.prepend(pointing.path ?? [], pointing.pointer), aim, { concurrency: 'unbounded' });
                    const index = yield* indexOf(aimed);
                    const route = Address.route(index);
                    const canvases = Array.filterMap(index.items, ({ target }) =>
                        target._tag === 'Control' && target.control.kind === 'region' ? Result.fromOption(Option.all({ ref: Option.some(target.control.ref), locator: route(target.control.ref), frame: Option.fromUndefinedOr(target.control.frame) }), Function.constVoid) : Result.failVoid,
                    );
                    const landed = Array.findFirst(canvases, ({ ref, frame: [[left, top], [width, height]] }) => (Predicate.isNumber(first.at) ? ref === first.at : first.at[0] >= left && first.at[0] < left + width && first.at[1] >= top && first.at[1] < top + height));
                    const pointer = Option.match(landed, { onNone: () => first.at, onSome: ({ frame: [[left, top], [width, height]] }): number | typeof Sample.Type => (Predicate.isNumber(first.at) ? [left + width / 2, top + height / 2] : first.at) });
                    const points = path.map(Struct.get('at'));
                    const windows = Array.dedupe([first, ...path].flatMap(Struct.get('windows')));
                    const samples = Array.filterMap(points, (point) => (Predicate.isNumber(point) ? Result.failVoid : Result.succeed(point)));
                    const drawn = Predicate.isNumber(pointer) || samples.length < points.length ? Option.none() : Option.map(landed, ({ locator }): typeof Description.Operation.Type => ({ locator, input: { ...pointing, pointer, path: samples } }));
                    return [{ ...plain, facet: drawn, step: { ...pointing, pointer, path: points, ...(Array.isReadonlyArrayEmpty(windows) ? {} : { windows }) } }];
                }),
            ),
            Match.when({ key: Match.defined }, ({ at, ...keyed }) => Effect.map(Effect.transposeOption(Option.map(Option.fromUndefinedOr(at), reach)), (ref) => [{ ...plain, step: { ...keyed, ...Option.getOrUndefined(Option.map(ref, (held) => ({ at: held }))) } }])),
            Match.when(
                { at: Match.defined },
                Effect.fnUntraced(function* (wanted: Extract<Step, { readonly at: string }>) {
                    const item = yield* Effect.flatMap(find(wanted.at), (found) => Effect.fromResult(accepts(found, wanted)));
                    const index = yield* indexOf(aimed);
                    const opening = item.live ? [] : yield* Effect.fromResult(Address.opening(Address.recall(index)(map), item));
                    const [ref, refs] = yield* Effect.all([refOf(item), Effect.forEach(opening, refOf, { concurrency: 'unbounded' })], { concurrency: 'unbounded' });
                    const located = Address.locate(item);
                    const from = Option.filter(Option.flatMap(located, map.route.bind(map)), (rung): rung is Exclude<typeof Wire.Rung.Type, 'source'> => rung !== 'source');
                    const to = Option.map(Option.filter(Option.fromUndefinedOr(wanted.to), Schema.is(Element.Value)), (value) => ({ to: value }));
                    return [
                        ...Array.zipWith(opening, refs, (opener, at) => ({ ...plain, label: ['at', Option.getOrElse(opener.token, () => Address.path(opener).join(Address.separator)), 'to reach', wanted.at], opener: Option.some(at), at: Address.locate(opener), step: { at } })),
                        {
                            ...plain,
                            at: located,
                            start: Option.getOrElse(from, () => plain.start),
                            facet: Option.map(Address.route(index)(ref), (locator): typeof Description.Operation.Type => ({ locator, ...Option.getOrUndefined(to) })),
                            step: { ...wanted, at: ref, ...Option.getOrUndefined(Option.map(from, (rung) => ({ from: rung }))) },
                        },
                    ];
                }),
            ),
            Match.orElse((held) => Effect.succeed([{ ...plain, step: held }])),
        );
    };
const rowOf = (index: Index, proven: HashSet.HashSet<number>, changed: boolean) => {
    const named = (ref: number) => Option.match(Record.get(index.live, `${ref}`), { onNone: Array.empty<string>, onSome: (item) => [...Option.toArray(item.token), ...Option.toArray(Option.map(item.name, Format.quoted))] });
    return (head: readonly string[], step: object, outcome: Acted['outcomes'][number]) => {
        const wheel = 'by' in step;
        const hover = 'button' in step && step.button === 'none' && !wheel;
        return Result.match(outcome, {
            onFailure: ({ message }) => new Line({ head, value: 'failed', facts: [message] }),
            onSuccess: (delivery) => {
                const { proof, route, presentation, popUps, scrolled } = delivery;
                const word = Match.value(delivery).pipe(
                    Match.when({ resolved: Match.defined }, ({ resolved: { ref, effect } }) => Option.some([effect, ...named(ref)].join(' '))),
                    Match.when({ proof: 'standing' }, ({ surface }) => Option.some([PROOFS.standing, ...Array.fromNullishOr(surface).flatMap(named)].join(' '))),
                    Match.when({ proof: 'answer' }, () => Option.some<string>(changed ? PROOFS.change : PROOFS.answer)),
                    Match.when({ proof: 'posting', followed: (promise) => HashSet.has(proven, promise) }, () => Option.some<string>(PROOFS.change)),
                    Match.when({ proof: 'posting' }, () => Option.liftPredicate<string>(PROOFS.posting, () => !hover)),
                    Match.orElse(() => Option.some<string>(PROOFS[proof])),
                );
                const moved = Array.fromNullishOr(scrolled).flatMap((held) => (wheel ? [...Outline.scrolled(held.vertical, held.horizontal), ...Outline.showing(held)] : Outline.scrolled(held.vertical, held.horizontal).map((first) => `${first} first`)));
                return new Line({
                    head,
                    ...Option.getOrUndefined(Option.map(word, (value) => ({ value }))),
                    facts: [
                        ...moved,
                        ...(route?._tag === 'source' ? [`drawn by ${route.member}`, ...Array.match(route.approximated, { onEmpty: Array.empty, onNonEmpty: (lost) => [`without ${Format.list(lost)}`] })] : []),
                        ...(route?._tag === 'focused' ? ['window focused'] : []),
                        ...(presentation?._tag === 'restored' ? ['brought forward', presentation.back ? 'put back' : 'put back refused'] : []),
                        ...Record.collect(
                            Record.filter({ closed: Array.countBy(popUps, Struct.get('closed')), 'left open': Array.countBy(popUps, ({ closed }) => !closed) }, (amount) => amount > 0),
                            (left, amount) => `${Format.count(amount, Outline.nouns.popUp)} ${left}`,
                        ),
                    ],
                });
            },
        });
    };
};
const Act = {
    tool,
    handle: Effect.fnUntraced(function* ({ steps, record = false }: Tool.Parameters<typeof tool>) {
        const aimed = yield* current;
        const { row, live } = aimed;
        const [server, maps, handle, side, source, { call, awaitRunning }] = yield* Effect.all([ApplicationServer, InterfaceMap.Store, Handle, Side, Source, ToolCall]);
        const { changes } = server;
        const proofs = yield* Ref.make(HashSet.empty<number>());
        yield* changes.pipe(
            Stream.filterMap(({ instance, change }) => (instance === live.instance && change._tag === 'proven' ? Result.succeed(change.followed) : Result.failVoid)),
            Stream.runForEach((followed) => Ref.update(proofs, HashSet.add(followed))),
            Effect.forkScoped({ startImmediately: true }),
        );
        const numbered = Array.map(steps, (step, at) => ({ number: at + 1, step, label: labelOf(step) }));
        const head = ({ number, label }: { readonly number: number; readonly label: readonly string[] }) => (steps.length > 1 ? [`${number}.`, ...label] : label);
        const [begun, map] = yield* Effect.all([indexOf(aimed), maps.recall(live)]);
        const settled = (at: string) => Effect.flatMap(indexOf(aimed), (index) => Effect.fromResult(Result.orElse(Address.resolve(index, at), () => Address.resolve(Address.recall(index)(map), at))));
        const client = yield* Effect.cached(source(row));
        const sent = Effect.fnUntraced(function* (entries: Array.NonEmptyReadonlyArray<Entry>, state: Acted['state'], recording: boolean) {
            const around = Option.firstSomeOf([Option.map(frontOf(state), (number): Capture['source'] => ({ _tag: 'windows', numbers: [number] })), Option.map(aimOf(Array.headNonEmpty(entries).step), (ref): Capture['source'] => ({ _tag: 'element', ref }))]);
            const taken = Option.filter(around, () => recording);
            const acted = yield* server.act({ instance: live.instance, steps: Array.map(entries, Struct.get('step')), ...Option.getOrUndefined(Option.map(taken, (from) => ({ record: { source: from, longestSide: Option.some(side), frames: FRAMES } }))) });
            return piece({ entries, outcomes: acted.outcomes, acted: Option.some(acted), source: taken, notes: recording && Option.isNone(around) ? [new Line({ head: ['not recorded'], value: 'no window shown and no element aimed at' })] : [] });
        });
        const operate = (state: Acted['state']) =>
            Effect.fnUntraced(function* ([entry, facet]: readonly [Entry, typeof Description.Operation.Type]) {
                const answered = yield* Effect.result(Effect.flatMap(client, (from) => from.operate(facet)));
                const refused = Result.match(answered, { onSuccess: Array.empty, onFailure: ({ message }) => [{ rung: 'source' as const, failure: message }] });
                const through = Result.isFailure(answered) && 'input' in facet ? Option.some(yield* sent([entry], state, false)) : Option.none();
                return Option.match(through, {
                    onNone: () => piece({ entries: [entry], outcomes: [Result.map(answered, (drawn) => ({ proof: 'answer' as const, ...(Schema.is(Wire.Route)(drawn) ? { route: drawn } : {}), tried: [], popUps: [] }))] }),
                    onSome: (fallen) =>
                        piece({
                            ...fallen,
                            outcomes: Array.map(
                                fallen.outcomes,
                                Result.mapBoth({
                                    onSuccess: (delivery) => ({ ...delivery, tried: [...refused, ...delivery.tried] }),
                                    onFailure: (cause) => new Cause.UnknownError(cause, [cause.message, ...refused.map(({ failure }) => `source refused, ${failure}`)].join(', ')),
                                }),
                            ),
                        }),
                });
            });
        const reaching = (at: string) => Address.reach(aimed, at);
        const pieces = yield* Stream.runCollect(
            Stream.paginate(
                { state: live.state, rest: Array.fromIterable(numbered) },
                Effect.fnUntraced(function* ({ state: before, rest }) {
                    const resolved = yield* Stream.fromIterable(rest).pipe(
                        Stream.mapEffect((each) => Effect.result(prepared(aimed, map, before, reaching)(each))),
                        Stream.takeUntil(Result.isFailure),
                        Stream.runCollect,
                    );
                    const [ready, refusals] = Array.partition(resolved, Function.identity);
                    const entries = Array.dedupeWith(ready.flat(), (left, right) => Option.isSome(left.opener) && Equal.equals(left.opener, right.opener));
                    const left = Array.drop(rest, ready.length);
                    if (!Array.isReadonlyArrayNonEmpty(entries)) {
                        const others = yield* Effect.forEach(Array.drop(left, 1), (each) => Effect.result(prepared(aimed, map, before, settled)(each)), { concurrency: 'unbounded' });
                        const alike = Array.zip(Array.drop(left, 1), others).flatMap(([each, other]) => (Result.isFailure(other) && refusals.some(sharing(other.failure)) ? [each] : []));
                        const stopped = [...Array.take(left, 1), ...alike];
                        const named = Format.list(stopped.map((each) => head(each).join(' ')));
                        const notes = refusals.map(({ message }) => new Line({ head: [named], value: 'not run', facts: [message] }));
                        return [[piece({ entries: [], outcomes: [], notes, refused: stopped.map(Struct.get('number')) })], Option.none()] as const;
                    }
                    const recording = record && entries.every(({ facet }) => Option.isNone(facet));
                    const ran = yield* Stream.fromIterable(Array.groupWith(entries, (each, next) => Option.isSome(each.facet) === Option.isSome(next.facet))).pipe(
                        Stream.flatMap((group) =>
                            Option.match(Option.all(Array.map(group, Struct.get('facet'))), {
                                onNone: () => Stream.fromEffect(sent(group, before, recording)),
                                onSome: (facets) => Stream.mapEffect(Stream.fromIterable(Array.zip(group, facets)), operate(before)),
                            }),
                        ),
                        Stream.takeUntil(({ outcomes }) => ending(outcomes)),
                        Stream.runCollect,
                    );
                    const after = Option.match(Array.last(Array.getSomes(ran.map(Struct.get('acted')))), { onNone: () => before, onSome: Struct.get('state') });
                    const more = Array.isReadonlyArrayEmpty(left) || ran.some(({ outcomes }) => ending(outcomes)) ? Option.none() : Option.some({ state: after, rest: left });
                    return [ran, more] as const;
                }),
            ),
        );
        const ran = pieces.flatMap(({ entries, outcomes }) => Array.zip(entries, outcomes));
        const acted = Array.getSomes(pieces.map(Struct.get('acted')));
        const state = Option.match(Array.last(acted), { onNone: () => live.state, onSome: Struct.get('state') });
        const delivered = Array.getSuccesses(ran.map(([, outcome]) => outcome));
        const answering = Instance.State.matchOrElse(state, { running: ({ status }) => status._tag === 'responding' }, Function.constFalse);
        const unanswered = delivered.some(({ proof }) => proof === 'unanswered');
        const touched = new Set([...ran.flatMap(([{ step }]) => Option.toArray(aimOf(step))), ...delivered.flatMap(({ resolved }) => Array.fromNullishOr(resolved?.ref))]);
        const after = yield* Effect.result(Effect.when(read(aimed), Effect.succeed(Array.isReadonlyArrayNonEmpty(delivered) && answering)));
        const changed = after.pipe(Result.map(Option.map(({ base }) => Outline.changes(begun, base, touched))));
        const proven = yield* Ref.get(proofs);
        const promised = ran.flatMap(([{ number, label }, outcome]) => Array.fromNullishOr(Result.getOrUndefined(outcome)?.followed).map((followed) => Tuple.make({ instance: live.instance, followed }, { call, step: number, head: label })));
        const helped = pieces.flatMap(({ entries, outcomes, acted: through }) => (Option.isSome(through) ? Array.zip(entries, outcomes) : []));
        yield* maps.prove(
            live,
            helped.flatMap(([{ at, start }, outcome]) => Option.toArray(at).map((routed) => ({ at: routed, from: start, outcome }))),
        );
        yield* handle.update(Proving, HashMap.union(HashMap.fromIterable(promised.filter(([{ followed }]) => !HashSet.has(proven, followed)))));
        yield* awaitRunning;
        const subject = yield* Outline.subject(row.name, { instance: live.instance, state }, { troubled: unanswered || ran.some(([, outcome]) => Result.isFailure(outcome)), keyed: ran.some(([{ step }]) => 'key' in step || ('to' in step && Predicate.isString(step.to))) });
        const pasted = Option.toArray(Array.last(acted.flatMap(({ pasteboard }) => Array.fromNullishOr(pasteboard))));
        const clipboard = yield* Effect.forEach(pasted, (taken) =>
            taken.pipe(
                Effect.fromResult,
                Effect.flatMap((pasteboard) => Outline.clipped(live.instance, pasteboard, false)),
                Effect.result,
            ),
        );
        const recorded = pieces.flatMap(({ entries, acted: through, source: from }) => Option.toArray(Option.all({ from, recording: Option.flatMapNullishOr(through, Struct.get('recording')) })).map((taken) => ({ ...taken, entries })));
        const frames = yield* attach(recorded.flatMap(({ from, recording, entries }) => Result.match(recording, { onFailure: Array.empty, onSuccess: (taken) => keyFramesOf({ instance: live.instance, state }, from, renumbered(entries, taken)) })));
        const written = acted.flatMap(({ written: paths }) => Array.fromNullishOr(paths));
        const stepped = rowOf(Result.match(after, { onFailure: () => begun, onSuccess: Option.match({ onNone: () => begun, onSome: Struct.get('base') }) }), proven, Result.isSuccess(changed) && Option.exists(changed.success, Array.isReadonlyArrayNonEmpty));
        const done = new Set([...ran.flatMap(([{ number, opener }]) => (Option.isNone(opener) ? [number] : [])), ...pieces.flatMap(Struct.get('refused'))]);
        return new Report({
            lines: [
                ...subject,
                ...ran.map(([entry, outcome]) => stepped(Option.isNone(entry.opener) ? head(entry) : entry.label, entry.step, outcome)),
                ...pieces.flatMap(Struct.get('notes')),
                ...Array.match(
                    numbered.filter(({ number }) => !done.has(number)),
                    { onEmpty: Array.empty, onNonEmpty: (rest) => [new Line({ head: ['not run'], value: Format.list(rest.map(({ number, label }) => ['step', `${number}`, ...label].join(' '))) })] },
                ),
                ...Result.match(changed, {
                    onFailure: ({ message }) => [new Line({ head: ['changes'], value: 'unread', facts: [message] })],
                    onSuccess: Option.match({ onNone: Array.empty<Line>, onSome: (lines) => (Array.isReadonlyArrayEmpty(lines) ? [new Line({ head: ['nothing shown changed'] })] : lines) }),
                }),
                ...(answering && !unanswered ? [] : Option.toArray(Array.last(acted)).flatMap(diagnose)),
                ...Array.dedupe(Array.getFailures(written).map(Struct.get('message'))).map((message) => new Line({ head: ['written'], value: 'unread', facts: [message] })),
                ...Array.match(Array.getSuccesses(written).flat(), { onEmpty: Array.empty, onNonEmpty: (urls) => [new Line({ head: ['written'], value: Format.list(urls.map((url) => Format.quoted(Format.path(url)))) })] }),
                ...clipboard.flatMap(Result.match({ onFailure: ({ message }) => [new Line({ head: ['clipboard'], value: 'unread', facts: [message] })], onSuccess: Function.identity })),
                ...Array.getFailures(recorded.map(Struct.get('recording'))).map(({ message }) => new Line({ head: ['recording'], value: 'failed', facts: [message] })),
                ...frames,
                ...(record && ran.some(([{ facet }]) => Option.isSome(facet)) ? [new Line({ head: ['not recorded'], value: 'a linked source ran a step' })] : []),
            ],
        });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { Act };
