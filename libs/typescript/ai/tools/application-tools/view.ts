import { pathToFileURL } from 'node:url';
import { ApplicationServer } from '@rasm/applications/client';
import { type Capture, type Frame, type InstanceSnapshot, InstanceState, Recording, type Running } from '@rasm/applications/contract';
import { type Entry, Workspace } from '@rasm/applications/workspace';
import { setting } from '@rasm/mcp/cli';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Array, Cause, Context, DateTime, Effect, Match, Option, Predicate, Record, Result, Schema, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { Flag } from 'effect/cli';
import { Address, type Index, type Item, read } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { current, Hold, hold, openCall } from '#/handle/target';

// --- [TYPES] ---------------------------------------------------------------------------

type Aimed = Effect.Success<typeof current>;
interface Placement {
    readonly instance: InstanceSnapshot['instance'];
    readonly named: string;
    readonly windows: readonly number[];
    readonly frame: typeof Frame.Type;
    readonly size: readonly [number, number];
}
interface Shot {
    readonly line: Line;
    readonly png: Option.Option<Uint8Array>;
    readonly placement: Option.Option<readonly [window: string, Placement]>;
    readonly front: readonly string[];
}
interface Taking {
    readonly marks: Record.ReadonlyRecord<string, ReturnType<typeof marksOf>>;
    readonly clip: Option.Option<Pick<Parameters<ApplicationServer['Service']['record']>[0], 'seconds' | 'frames' | 'movie'>>;
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const FRAMES = 6;

// --- [MODELS] --------------------------------------------------------------------------

const Side = setting('longest-side', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), Flag.withDefault(1568), 'Longest side in pixels of each image a view returns');
const Pixel = Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0));
const Pixels = Schema.Tuple([Pixel, Pixel, Schema.Finite.check(Schema.isGreaterThan(0)), Schema.Finite.check(Schema.isGreaterThan(0))]);
const tool = Tool.make('view', {
    description:
        "Shows the target as images: front window, every window, one container or control, a document, an object, or a pixel rectangle enlarged, and with seconds key frames of a recording. Each image line names its window. A point an act step takes counts pixels of one window's whole image, which crops and failed views leave in place, and marks draws references a pointer step takes in place of a point. Changed rectangles of key frames count pixels of that image, and of shows one again",
    parameters: Schema.Struct({
        of: Schema.optionalKey(
            Schema.Union([Schema.Literals(['front', 'all', 'document', 'object']), Address.At, Pixels]).annotate({
                description:
                    'front when absent; all for every window, hidden, minimized, and off-Space ones from their last drawn content; document or object with id; a reference or label path as inspect prints it, a status item shown unpressed and a menu item with its open menu levels; or a pixel rectangle [x, y, width, height] of a window image at screen resolution',
            }),
        ),
        in: Schema.optionalKey(Address.At.annotate({ description: 'Window whose image a pixel rectangle counts, where images of several windows are held' })),
        id: Schema.optionalKey(Schema.String.annotate({ description: 'document: document id from a result; object: id a script result gives the object' })),
        marks: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Draw the frame and reference of each control' })),
        seconds: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Seconds to record, key frames returned in place of one image' })),
        frames: Recording.fields.frames.annotate({ description: 'Most key frames a recording returns: 1 the last state, 2 the first and last whole, more the states between cropped to their change', default: FRAMES }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(FRAMES))),
        into: Schema.optionalKey(Schema.String.annotate({ description: 'Path the recording is also written to as a movie, .mov or .mp4' })),
    }),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Strict, true), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [SERVICES] ------------------------------------------------------------------------

const Viewed = Context.Reference<Record.ReadonlyRecord<string, Placement>>('@rasm/application-tools/view/Viewed', { defaultValue: Record.empty });
const Render = Context.Reference<(row: Entry, id: string) => Effect.Effect<{ readonly image: Uint8Array; readonly title: string }, Error>>('@rasm/application-tools/view/Render', {
    defaultValue: () => (row, id) => new Cause.NoSuchElementError(`object ${id} unrendered, ${row.name} links no source`),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const refused = (lines: readonly Line[]) => new Cause.NoSuchElementError(Report.text(new Report({ lines })));
const said = (line: Line): Shot => ({ line, png: Option.none(), placement: Option.none(), front: [] });
const windowsOf = (state: InstanceSnapshot['state']): Running['windows'] => InstanceState.matchOrElse(state, { running: Struct.get('windows') }, Array.empty);
const frontOf = (state: InstanceSnapshot['state']) => {
    const shown = windowsOf(state).filter((window) => window.presented && window.kind !== 'menu');
    return Option.map(Option.firstSomeOf([Array.findFirst(shown, (window) => window.asking || window.kind === 'window'), Array.head(shown)]), Struct.get('number'));
};
const numbersOf = {
    front: (state: InstanceSnapshot['state']) => Option.toArray(frontOf(state)),
    all: (state: InstanceSnapshot['state']) => Array.filterMap(windowsOf(state), ({ number, kind }) => (kind === 'menu' ? Result.failVoid : Result.succeed(number))),
} as const;
const presented = (presentation: (typeof Capture.Screenshot.Type)['presentation']) => Array.fromNullishOr(presentation).flatMap((held) => (held._tag === 'forward' || (held._tag === 'restored' && !held.back) ? [Outline.say.front] : []));
const marksOf = (index: Index) => index.shown.flatMap(({ controls }) => controls.flatMap(({ control, item }) => Option.toArray(Option.all({ frame: Option.fromUndefinedOr(control.frame), label: item.token }))));
const unshownOf = ({ name }: Pick<Entry, 'name'>, index: Index) => {
    const items = index.shown.flatMap(({ container, controls }) => (container.kind === 'statusItem' ? controls : []));
    const call = Option.match(
        Array.findFirst(items, ({ item }) => item.token),
        { onSome: (at) => new Call({ tool: 'act', given: { steps: [{ at }] } }), onNone: () => openCall(name) },
    );
    return new Line({ head: ['windows'], value: 'none shown', call });
};
const menuPath = (index: Index, item: Item): readonly Item[] => {
    const menuOf = (ref: number) => Option.filter(Record.get(index.live, `${ref}`), ({ target }) => target._tag === 'Container' && target.container.kind === 'menu');
    const start = item.target._tag === 'Container' || item.target._tag === 'Control' ? menuOf(item.target.container.ref) : Option.none<Item>();
    return Array.reverse(Array.unfold(start, (at) => Option.map(at, (held) => [held, held.target._tag === 'Container' ? Option.flatMap(Option.fromUndefinedOr(held.target.container.parent), menuOf) : Option.none<Item>()] as const)));
};
const screenOf = ({ named, frame: [[left, top], [width, height]], size: [columns, rows] }: Placement, at: readonly [x: number, y: number, wide?: number, high?: number]): Result.Result<typeof Frame.Type, Cause.IllegalArgumentError> => {
    const [x, y, wide = 0, high = 0] = at;
    return x + wide <= columns && y + high <= rows
        ? Result.succeed([
              [left + (x * width) / columns, top + (y * height) / rows],
              [(wide * width) / columns, (high * height) / rows],
          ])
        : Result.fail(new Cause.IllegalArgumentError(`${at.join(',')}: outside the ${columns} by ${rows} image of ${named}`));
};
const placedOf = (viewed: Record.ReadonlyRecord<string, Placement>, { instance, state, window }: Pick<InstanceSnapshot, 'instance' | 'state'> & { readonly window: Option.Option<Item> }): Result.Result<Placement, Cause.IllegalArgumentError> => {
    const wanted = Option.map(window, ({ target }) => (target._tag === 'Container' || target._tag === 'Control' ? target.container.window : undefined));
    const shown = windowsOf(state).flatMap(({ number, onScreen }) => (onScreen && Option.match(wanted, { onNone: () => true, onSome: (at) => at === number }) ? Array.fromNullishOr(viewed[`${number}`]) : []));
    const kept = shown.filter((held) => held.instance === instance);
    const token = Option.flatMap(window, Struct.get('token'));
    const lacking = new Report({ lines: [new Line({ head: [Option.getOrElse(token, () => 'pixels')], value: 'no image held of a shown window' })], next: new Call({ tool: tool.name, given: Record.getSomes({ of: token }) }) });
    return Predicate.isTupleOf(kept, 1) ? Result.succeed(kept[0]) : Result.fail(new Cause.IllegalArgumentError(Array.match(kept, { onEmpty: () => Report.text(lacking), onNonEmpty: (several) => `in: names one of ${Format.list(several.map(Struct.get('named')))}` })));
};
const pointOf = (viewed: Record.ReadonlyRecord<string, Placement>, aim: Parameters<typeof placedOf>[1]) => {
    const placement = placedOf(viewed, aim);
    return (point: readonly [x: number, y: number]) => Result.flatMap(placement, (held) => Result.map(screenOf(held, point), ([at]) => ({ at, windows: held.windows })));
};
const shot = ({ instance, state }: Pick<InstanceSnapshot, 'instance' | 'state'>) => {
    const heads = Record.fromIterableWith(windowsOf(state), ({ number, container, kind = 'window', title }) => [`${number}`, UndefinedOr.match(container, { onDefined: (ref) => Outline.caption(kind, ref, title), onUndefined: () => [`${number}`, ...Outline.caption(kind, undefined, title)] })]);
    return ({ png, width, height, frame, windows, backingStore, document, presentation }: typeof Capture.Screenshot.Type, whole: boolean, facts: readonly string[]): Shot => {
        const [head = ['display'], ...beside] = windows.map((number) => heads[`${number}`] ?? [`${number}`, ...Outline.caption('window')]);
        const [named = 'display'] = head;
        return {
            line: new Line({ head, facts: [...beside.map(Array.join(' ')), ...facts, ...(backingStore ? ['off screen'] : []), ...Array.fromNullishOr(document).map((url) => `document: ${url}`)] }),
            png: Option.some(png),
            placement: whole ? Option.map(Array.head(windows), (window) => [`${window}`, { instance, named, windows, frame, size: [width, height] }] as const) : Option.none(),
            front: presented(presentation),
        };
    };
};
const stillsOf = (snapshot: Pick<InstanceSnapshot, 'instance' | 'state' | 'name' | 'identifier'>, source: Capture['source'], results: typeof Capture.Result.Type, crop: readonly string[]): readonly Shot[] => {
    const draw = shot(snapshot);
    const numbers = source._tag === 'windows' ? source.numbers : [];
    const shown = new Set(Array.filterMap(windowsOf(snapshot.state), ({ number, onScreen }) => (onScreen ? Result.succeed(number) : Result.failVoid)));
    const [shots, bare] = Array.partition(results, (result, at) =>
        Result.match(result, {
            onSuccess: (screenshot) => Result.succeed(draw(screenshot, source._tag === 'windows', source._tag === 'windows' ? [] : crop)),
            onFailure: ({ message }) => (Option.exists(Array.get(numbers, at), (number) => !shown.has(number)) ? Result.fail(at) : new Line({ head: [], value: message }).pipe(said, Result.succeed)),
        }),
    );
    const reopen = openCall(snapshot.name ?? snapshot.identifier);
    const hidden = new Line({ head: ['off screen'], value: Format.count(bare.length, Outline.nouns.window), facts: ['no image'], call: new Call({ tool: reopen.tool, given: Record.set(reopen.given, 'activate', true) }) });
    return Array.isReadonlyArrayEmpty(bare) ? shots : [...shots, said(hidden)];
};
const keyFramesOf = (snapshot: Pick<InstanceSnapshot, 'instance' | 'state'>, source: Capture['source'], { keyFrames }: typeof Recording.Result.Type): readonly Shot[] =>
    Array.match(keyFrames, {
        onEmpty: Array.empty,
        onNonEmpty: (frames) => {
            const draw = shot(snapshot);
            const first = Array.headNonEmpty(frames);
            const [[x, y], [across, down]] = first.screenshot.frame;
            const scale = [first.screenshot.width / across, first.screenshot.height / down] as const;
            const begun = frames.map(({ markers }) => markers.flatMap((marker) => (marker._tag === 'step' ? [`step ${marker.index + 1}`] : [])));
            const single = source._tag === 'windows' && Predicate.isTupleOf(source.numbers, 1);
            return frames.map(({ at, changed, screenshot }, index) =>
                draw(screenshot, single && index === frames.length - 1, [
                    index === 0 ? 'start' : Format.duration(DateTime.distance(first.at, at)),
                    ...Array.match(begun[index - 1] ?? [], { onEmpty: Array.empty, onNonEmpty: (steps) => [`after ${Format.list(steps)}`] }),
                    ...Array.fromNullishOr(changed).map(([[left, top], [wide, high]]) => `changed [${[(left - x) * scale[0], (top - y) * scale[1], wide * scale[0], high * scale[1]].map(Math.round).join(',')}]`),
                ]),
            );
        },
    });
const attach = Effect.fnUntraced(function* (shots: readonly Shot[]) {
    const [{ add }, handle] = yield* Effect.all([ToolCall, Handle]);
    yield* handle.update(Viewed, (held) => ({ ...held, ...Record.fromEntries(Array.getSomes(shots.map(Struct.get('placement')))) }));
    yield* Effect.forEach(Array.getSomes(shots.map(Struct.get('png'))), (data) => add(McpSchema.ImageContent.make({ data, mimeType: 'image/png' })), { discard: true });
    return shots.map(Struct.get('line'));
});
const capture = Effect.fnUntraced(function* (snapshot: Pick<InstanceSnapshot, 'instance' | 'state' | 'name' | 'identifier'>, source: Capture['source'], { marks, clip }: Taking, crop: readonly string[] = []) {
    const [server, { running }, side] = yield* Effect.all([ApplicationServer, Workspace, Side]);
    const { instance } = snapshot;
    yield* hold([Hold.Using({ instance })]);
    const request = { instance, source, longestSide: Option.some(side) };
    const taken = yield* Option.match(clip, {
        onNone: () => Effect.map(server.capture({ ...request, marks: marks[instance] ?? [] }), (results) => (fresh: typeof snapshot) => stillsOf(fresh, source, results, crop)),
        onSome: (held) => Effect.map(server.record({ ...request, ...held }), (recording) => (fresh: typeof snapshot) => [...Array.fromNullishOr(held.movie).map((url) => said(new Line({ head: ['movie'], value: Format.path(url) }))), ...keyFramesOf(fresh, source, recording)]),
    });
    const now = Array.findFirst(yield* SubscriptionRef.get(running), (each) => each.instance === instance);
    const shots = taken({ ...snapshot, state: Option.match(now, { onNone: () => snapshot.state, onSome: Struct.get('state') }) });
    return shots.some(({ png }) => Option.isSome(png)) ? shots : yield* refused(shots.map(Struct.get('line')));
});
const windowed = Effect.fnUntraced(function* <E, R>({ row, live }: Aimed, numbers: readonly number[], base: Effect.Effect<Index, E, R>, taking: Taking) {
    return yield* Array.match(numbers, {
        onEmpty: () => Effect.flatMap(base, (index) => refused([unshownOf(row, index)])),
        onNonEmpty: (held) => capture(live, { _tag: 'windows', numbers: held }, taking),
    });
});
const zoom = (pixels: typeof Pixels.Type, within: string | undefined, taking: Taking) =>
    Effect.fnUntraced(function* (aim: Aimed) {
        const [viewed, found] = yield* Effect.all([Handle.use((handle) => handle.get(Viewed)), Effect.forEach(Array.fromNullishOr(within), (at) => Address.reach(aim, at))]);
        const placement = yield* Effect.fromResult(placedOf(viewed, { ...aim.live, window: Array.head(found) }));
        const frame = yield* Effect.fromResult(screenOf(placement, pixels));
        return yield* capture(aim.live, { _tag: 'region', frame }, taking, [`crop [${pixels.join(',')}] of ${placement.named}`]);
    });
const element = (reference: string, taking: Taking) =>
    Effect.fnUntraced(function* (aim: Aimed & Effect.Success<ReturnType<typeof read>>) {
        const { row, live, base, map, route, indexed, level } = aim;
        const item = yield* Address.reach(aim, reference);
        const recalled = Address.recall(base)(map);
        const named = Option.getOrElse(item.token, () => reference);
        const placed = Match.value(item.target).pipe(
            Match.tag('Container', ({ container }) => ({ ...Struct.pick(container, ['ref', 'kind', 'frame', 'window']), opens: false, unseen: false })),
            Match.tag('Control', ({ control, container }) => ({ ref: control.ref, kind: container.kind, frame: control.frame ?? container.frame, window: container.window, opens: control.opens !== undefined && container.kind !== 'statusItem', unseen: control.frame === undefined && container.frame !== undefined })),
            Match.tag('Option', ({ option }) => ({ ref: option.ref, kind: undefined, frame: undefined, window: undefined, opens: false, unseen: false })),
            Match.orElse(() => undefined),
        );
        const shown = Array.fromNullishOr(placed);
        yield* shown.some(Struct.get('unseen')) ? refused([new Line({ head: [named], value: 'not in view', call: new Call({ tool: 'act', given: { steps: [{ pointer: named, button: 'none' }] } }) })]) : Effect.void;
        const sources = yield* Effect.forEach(shown, ({ ref, kind, frame, window }) => {
            const still = kind === 'statusItem' || Option.isSome(route(ref));
            const root = Option.map(
                Array.findFirst(windowsOf(live.state), (each) => each.container === ref),
                Struct.get('number'),
            );
            const part: readonly Capture['source'][] = still ? Array.fromNullishOr(frame).map((held) => ({ _tag: 'region', frame: held })) : [{ _tag: 'element', ref }];
            const whole = Option.map(Option.firstSomeOf([root, ...(Array.isReadonlyArrayEmpty(part) ? [Option.fromUndefinedOr(window), frontOf(live.state)] : [])]), (number): Capture['source'] => ({ _tag: 'windows', numbers: [number] }));
            return Effect.fromOption(
                Option.orElse(whole, () => Array.head(part)),
                () => refused([unshownOf(row, base)]),
            );
        });
        const shots = yield* Effect.forEach(sources, (source) => capture(live, source, taking, [`crop ${named}`]));
        const opened = yield* Effect.forEach(shown.filter(Struct.get('opens')), ({ ref }) => level({ _tag: 'menu', ref }));
        const fresh = indexed(opened.flatMap(Struct.get('containers')));
        const holder = item.target._tag === 'RememberedControl' ? Option.map(Record.get(recalled.located, item.target.container), Array.headNonEmpty) : Option.liftPredicate(item, ({ live: open }) => !open);
        const levels = [...menuPath(base, item).map((each) => [base, each] as const), ...Option.toArray(holder).map((each) => [recalled, each] as const), ...fresh.shown.map(({ item: each }) => [fresh, each] as const)];
        const outline = yield* Effect.forEach(levels, ([index, each]) => Effect.fromResult(Outline.render(index, { in: Option.some(each) })));
        return [...shots.flat(), ...outline.flat().map(said)];
    });
const document = (taking: Taking) =>
    Effect.fnUntraced(function* (id: string) {
        const instances = yield* Effect.flatMap(Workspace, ({ running }) => SubscriptionRef.get(running));
        const shown = instances.flatMap((each) =>
            Array.match(
                windowsOf(each.state).flatMap((window) => (window.document === id ? [window.number] : [])),
                { onEmpty: Array.empty, onNonEmpty: (numbers) => [{ each, numbers }] },
            ),
        );
        yield* Array.isReadonlyArrayEmpty(shown) ? new Cause.NoSuchElementError(`document ${id}: no window shows it`) : Effect.void;
        const taken = yield* Effect.forEach(shown, ({ each, numbers }) => Effect.map(capture(each, { _tag: 'windows', numbers }, taking), Array.prepend(said(new Line({ head: [each.name ?? each.identifier] })))), { concurrency: 'unbounded' });
        return taken.flat();
    });
const object = (id: string) =>
    Effect.fnUntraced(function* ({ row }: Aimed) {
        const [render, server, side] = yield* Effect.all([Render, ApplicationServer, Side]);
        const { image, title } = yield* render(row, id);
        const { png } = yield* server.scale({ image, longestSide: side });
        return [{ ...said(new Line({ head: ['object', Format.quoted(title)] })), png: Option.some(png) }];
    });
const View = {
    tool,
    handle: Effect.fnUntraced(function* ({ of = 'front', id, in: within, marks = false, seconds, frames, into }: Tool.Parameters<typeof tool>) {
        const refusals = Record.keys(Record.filter({ 'seconds: not taken with marks or of:"object"': seconds !== undefined && (marks || of === 'object'), 'into: takes seconds': into !== undefined && seconds === undefined }, Boolean));
        yield* Array.match(refusals, { onEmpty: () => Effect.void, onNonEmpty: (lines) => new Cause.IllegalArgumentError(lines.join('\n')) });
        const named = Effect.fromOption(Option.fromUndefinedOr(id), () => new Cause.IllegalArgumentError('id: required with of:"document" and of:"object"'));
        const aimed = yield* Effect.cached(current);
        const reading = yield* Effect.cached(Effect.flatMap(aimed, (aim) => Effect.map(read(aim), (held) => ({ ...aim, ...held }))));
        const base = Effect.map(reading, Struct.get('base'));
        const taking: Taking = {
            marks: marks ? yield* Effect.map(reading, ({ live, base: index }) => ({ [live.instance]: marksOf(index) })) : {},
            clip: Option.map(Option.fromUndefinedOr(seconds), (span) => ({ seconds: span, frames, ...Option.getOrUndefined(Option.map(Option.fromUndefinedOr(into), (path) => ({ movie: pathToFileURL(path) }))) })),
        };
        const shots = yield* Match.value(of).pipe(
            Match.when(Match.is('front', 'all'), (scope) => Effect.flatMap(aimed, (aim) => windowed(aim, numbersOf[scope](aim.live.state), base, taking))),
            Match.when('document', () => Effect.flatMap(named, document(taking))),
            Match.when('object', () => Effect.flatMap(named, (held) => Effect.flatMap(aimed, object(held)))),
            Match.when(Predicate.isString, (reference) => Effect.flatMap(reading, element(reference, taking))),
            Match.orElse((pixels) => Effect.flatMap(aimed, zoom(pixels, within, taking))),
        );
        const lines = yield* attach(shots);
        const subject = of === 'document' ? [] : yield* Effect.flatMap(aimed, ({ row, live }) => Outline.subject(row.name, live, { facts: Array.dedupe(shots.flatMap(Struct.get('front'))) }));
        return new Report({ lines: [...subject, ...lines] });
    }, Effect.scoped),
};

// --- [EXPORTS] -------------------------------------------------------------------------

export { attach, FRAMES, frontOf, keyFramesOf, pointOf, Render, Side, View, Viewed, windowsOf };
