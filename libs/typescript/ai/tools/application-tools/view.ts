import { pathToFileURL } from 'node:url';
import type { Entry } from '@rasm/applications/catalog';
import { Capture } from '@rasm/applications/server/capture';
import { ApplicationServer } from '@rasm/applications/server/client';
import { Element } from '@rasm/applications/server/element';
import { ApplicationFailure } from '@rasm/applications/server/failure';
import { Instance } from '@rasm/applications/server/instance';
import { setting } from '@rasm/mcp/cli';
import { Call, Format, Line, Report } from '@rasm/mcp/report';
import { Handle } from '@rasm/mcp/server';
import { ToolCall } from '@rasm/mcp/toolkit';
import { Array, Cause, Context, DateTime, Duration, Effect, Match, Option, Predicate, Record, Result, Schema, Struct, SubscriptionRef, Tuple } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { Flag } from 'effect/cli';
import { Address } from '#/handle/address';
import { Outline } from '#/handle/outline';
import { Target } from '#/handle/target';

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 6;

// --- [MODELS] --------------------------------------------------------------------------

const Side = setting('longest-side', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), Flag.withDefault(1568), 'Longest side in pixels of each image a view returns');
const tool = Tool.make('view', {
    description:
        "Shows the target as images: its front window, every window, one container or control, a document, an object, or a pixel rectangle of a window image enlarged, and with seconds the key frames of a recording, each naming the step or event it follows. Each image line names the window it shows and the pixel size of the image. A point an act step takes counts pixels of one window's whole image, which a crop, a view of one element, a failed view, and a cropped key frame leave in place, and marks draws the references a pointer step takes in place of a point. A changed rectangle of a key frame counts pixels of the first key frame, and of takes it to show that part at screen resolution. Calls on one handle answer in order",
    parameters: Schema.Struct({
        of: Schema.optionalKey(
            Schema.Union([
                Schema.Literals(['front', 'all', 'document', 'object']),
                Address.At,
                Schema.Tuple([Schema.Finite, Schema.Finite, Schema.Finite, Schema.Finite]).check(Schema.makeFilter(([x, y, wide, high]: readonly [number, number, number, number]) => x >= 0 && y >= 0 && wide > 0 && high > 0, { expected: 'a rectangle [x, y, width, height] from 0 with a width and height above 0' })),
            ]).annotate({
                description:
                    'front when absent; all for every window, one off screen or covered shown from its backing store; document or object with id; a reference or path as inspect prints it, a status item shown unpressed and a menu item with an outline of its open menus; or a rectangle [x, y, width, height] in pixels of a window image, shown at screen resolution',
            }),
        ),
        in: Schema.optionalKey(Address.At.annotate({ description: 'Window whose image the rectangle counts pixels of, named where view returned images of more than one' })),
        id: Schema.optionalKey(Schema.String.annotate({ description: 'document: document a result names; object: id a script result gives the object' })),
        marks: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Draw the frame and reference of each control on the image' })),
        seconds: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Seconds to record, key frames returned in place of one image' })),
        frames: Capture.fields.mode.cases.recording.fields.keyFrameLimit.annotate({ description: 'Most key frames a recording returns: 1 the last state, 2 the first and last whole, more the states between cropped to their change', default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
        into: Schema.optionalKey(Schema.String.annotate({ description: 'Absolute path the recording is also written to as a movie, .mov or .mp4' })),
    }).check(
        Schema.makeFilter(({ of, id, marks = false, seconds, into }: { readonly of?: unknown; readonly id?: string; readonly marks?: boolean; readonly seconds?: number; readonly into?: string }) => [
            ...(seconds !== undefined && (marks || of === 'object') ? [{ path: ['seconds'], issue: 'not taken with marks or of:"object"' }] : []),
            ...(into !== undefined && seconds === undefined ? [{ path: ['into'], issue: 'takes seconds' }] : []),
            ...(id === undefined && (of === 'document' || of === 'object') ? [{ path: ['id'], issue: 'required with of:"document" and of:"object"' }] : []),
        ]),
    ),
    success: Report.schema,
    failure: Schema.ErrorInstance(),
    dependencies: [Handle],
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Strict, true), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true })));

// --- [SERVICES] ------------------------------------------------------------------------

const Viewed = Context.Reference<Record.ReadonlyRecord<string, Pick<typeof Capture.Frame.Type, 'frame' | 'windows' | 'width' | 'height'> & Pick<Instance, 'instance'> & { readonly named: string }>>('@rasm/application-tools/view/Viewed', { defaultValue: Record.empty });
const Render = Context.Reference<(row: Entry, id: string) => Effect.Effect<{ readonly image: Uint8Array; readonly title: string }, Error>>('@rasm/application-tools/view/Render', {
    defaultValue: () => (row, id) => ApplicationFailure.of('noRenderer', `object ${id}`, [row.name]),
});

// --- [OPERATIONS] ----------------------------------------------------------------------

const windows = (state: Instance['state']): (typeof Instance.State.cases.running.Type)['windows'] => Instance.State.matchOrElse(state, { running: Struct.get('windows') }, Array.empty);
const front = (state: Instance['state']) => {
    const shown = windows(state).filter((window) => window.presented && window.kind !== 'menu');
    return Option.map(Option.firstSomeOf([Array.findFirst(shown, (window) => window.asking || window.kind === 'window'), Array.head(shown)]), Struct.get('number'));
};
const refuse = (lines: readonly Line[]) => new Cause.NoSuchElementError(Report.text(new Report({ lines })));
const point = (viewed: (typeof Viewed)['Service'], { instance, state, window }: Pick<Instance, 'instance' | 'state'> & { readonly window: Option.Option<Parameters<typeof Address.open>[1]> }) => {
    const wanted = Option.map(window, (node) => (node.live ? Array.lastNonEmpty(node.containers).window : undefined));
    const held = windows(state).flatMap(({ number, onScreen }) => (onScreen && !Option.exists(wanted, (at) => at !== number) ? Array.fromNullishOr(viewed[`${number}`]).filter((image) => image.instance === instance) : []));
    const token = Option.flatMap(window, (node) => node.token);
    const placed: Result.Result<(typeof Viewed)['Service'][string], Error> = Array.match(held, {
        onEmpty: () => Result.fail(new Cause.NoSuchElementError(Report.text(new Report({ lines: [new Line({ head: [Option.getOrElse(token, () => 'pixels')], value: 'no image held of a shown window' })], next: new Call({ tool: tool.name, given: Record.getSomes({ of: token }) }) })))),
        onNonEmpty: (several) => (Predicate.isTupleOf(several, 1) ? Result.succeed(several[0]) : Result.fail(ApplicationFailure.of('namesOneOf', 'in', several.map(Struct.get('named'))))),
    });
    return ([x, y]: readonly [x: number, y: number]) =>
        Result.flatMap(placed, ({ named, frame: [[left, top], [across, down]], width, height, windows: shown }) =>
            x >= 0 && y >= 0 && x <= width && y <= height ? Result.succeed({ at: Tuple.make(left + (x * across) / width, top + (y * down) / height), windows: shown }) : Result.fail(ApplicationFailure.of('outsideImage', `${x},${y}`, [named, Address.print('region')({ _tag: 'size', value: Tuple.make(width, height) })])),
        );
};
const frames = ({ instance, state }: Pick<Instance, 'instance' | 'state'>, source: Capture['source'], taken: readonly (typeof Capture.Frame.Type)[], crop: readonly string[] = []) => {
    const captions = Record.fromIterableWith(windows(state), ({ number, container, kind = 'window', title }) => [`${number}`, [...(container === undefined ? [`${number}`] : []), ...Outline.caption(kind, container, title)]]);
    const first = Array.head(taken);
    const began = Option.flatMapNullishOr(first, (held) => held.at);
    return taken.map((frame, index) => {
        const [head = ['display'], ...beside] = frame.windows.map((number) => captions[`${number}`] ?? [`${number}`, ...Outline.caption('window')]);
        const [named = 'display'] = head;
        const whole = source._tag === 'windows' && (frame.at === undefined || (index === taken.length - 1 && Predicate.isTupleOf(source.numbers, 1)));
        const follows = Array.dedupe((taken[index - 1]?.markers ?? []).map((marker) => (marker._tag === 'step' ? `step ${marker.index + 1}` : marker.change._tag)));
        const elapsed = Option.zipWith(began, Option.fromUndefinedOr(frame.at), (from, at) => DateTime.distance(from, at));
        const changed = Option.zipWith(first, Option.fromUndefinedOr(frame.changed), ({ frame: [[x, y], [across, down]], width, height }, [[left, top], [wide, high]]) => [((left - x) * width) / across, ((top - y) * height) / down, (wide * width) / across, (high * height) / down].map(Math.round).join(','));
        const facts = [
            ...beside.map(Array.join(' ')),
            Address.print('region')({ _tag: 'size', value: Tuple.make(frame.width, frame.height) }),
            ...Option.toArray(elapsed).map((span) => (index === 0 ? 'start' : Format.duration(span))),
            ...(Array.isReadonlyArrayEmpty(follows) ? [] : [`after ${Format.list(follows)}`]),
            ...Option.toArray(changed).map((rectangle) => `changed [${rectangle}]`),
            ...(source._tag === 'windows' ? [] : crop),
            ...(frame.backingStore ? ['backing store'] : []),
            ...Array.fromNullishOr(frame.document).map((url) => `document: ${url}`),
        ];
        return { line: new Line({ head, facts }), frame, placement: whole ? Array.take(frame.windows, 1).map((number) => Tuple.make(`${number}`, { instance, named, ...Struct.pick(frame, ['frame', 'windows', 'width', 'height']) })) : [] };
    });
};
const attach = Effect.fnUntraced(function* (drawn: readonly { readonly line: Line; readonly frame?: Pick<typeof Capture.Frame.Type, 'png' | 'presentation'>; readonly placement?: readonly (readonly [window: string, (typeof Viewed)['Service'][string]])[] }[]) {
    const [{ add }, handle] = yield* Effect.all([ToolCall, Handle]);
    yield* handle.update(Viewed, (held) => ({ ...held, ...Record.fromEntries(drawn.flatMap(({ placement = [] }) => placement)) }));
    const images = drawn.flatMap(({ frame }) => Array.fromNullishOr(frame));
    yield* Effect.forEach(images, ({ png }) => add(McpSchema.ImageContent.make({ data: png, mimeType: 'image/png' })), { discard: true });
    return drawn.map(({ line }) => line);
});
const capture = Effect.fnUntraced(function* (snapshot: Pick<Instance, 'instance' | 'state' | 'name' | 'identifier'>, source: Capture['source'], mode: (instance: Instance['instance']) => Capture['mode'], crop: readonly string[] = []) {
    const [server, side] = yield* Effect.all([ApplicationServer, Side]);
    const { instance, name = snapshot.identifier } = snapshot;
    const asked = mode(instance);
    const read = yield* server.capture({ instance, source, longestSide: Option.some(side), mode: asked });
    const state = Option.match(Record.get(Instance.byInstance(yield* SubscriptionRef.get(server.running)), instance), { onNone: () => snapshot.state, onSome: Struct.get('state') });
    const numbers: readonly number[] = asked._tag === 'still' && source._tag === 'windows' ? source.numbers : [];
    const off = new Set(numbers.flatMap((number, at) => (windows(state).some((shown) => shown.number === number && shown.onScreen) ? [] : [at])));
    const [taken, unread] = Array.partition(read, (each, at) => Result.mapError(each, ({ message }) => (off.has(at) ? Option.none() : Option.some(message))));
    const reopen = Target.open(name);
    const lines = [
        ...frames({ instance, state }, source, taken, crop),
        ...Array.getSomes(unread).map((message) => ({ line: new Line({ head: [], value: message }) })),
        ...(unread.some(Option.isNone) ? [{ line: new Line({ head: ['off screen'], value: Format.count(Array.countBy(unread, Option.isNone), Outline.nouns.window), facts: ['no image'], call: new Call({ tool: reopen.tool, given: { ...reopen.given, activate: true } }) }) }] : []),
    ];
    const movie = asked._tag === 'recording' ? Array.fromNullishOr(asked.movie).map((url) => ({ line: new Line({ head: ['movie'], value: Format.path(url) }) })) : [];
    return Array.isReadonlyArrayEmpty(taken) ? yield* refuse(lines.map(({ line }) => line)) : [...movie, ...lines];
});
const element = Effect.fnUntraced(function* (target: Effect.Success<typeof Target.current>, read: Address, at: string, mode: Parameters<typeof capture>[2]) {
    const node = yield* Address.reach(target, at);
    const named = Option.getOrElse(node.token, () => at);
    const shown = node.live ? [{ held: node.element, within: Array.lastNonEmpty(node.containers) }] : [];
    const drawn = yield* Effect.forEach(shown, ({ held, within }) => {
        const frame = Element.match(held, { container: (each) => each.frame, control: (each) => each.frame ?? within.frame, option: () => undefined });
        const unseen = held._tag === 'control' && held.frame === undefined && within.frame !== undefined;
        const part = within.kind === 'statusItem' || Option.isSome(Address.route(read, held.ref)) ? Option.map(Option.fromUndefinedOr(frame), (region): Capture['source'] => ({ _tag: 'region', frame: region })) : Option.some<Capture['source']>({ _tag: 'element', ref: held.ref });
        const root = Array.findFirst(windows(target.live.state), (each) => (each.container === held.ref ? Option.some(each.number) : Option.none()));
        const whole = Option.firstSomeOf([root, ...(Option.isNone(part) ? [Option.fromUndefinedOr(within.window), front(target.live.state)] : [])]);
        const source: Option.Option<Capture['source']> = unseen ? Option.none() : Option.firstSomeOf([Option.map(whole, (number): Capture['source'] => ({ _tag: 'windows', numbers: [number] })), part]);
        const step = unseen ? { pointer: named, button: 'none' } : { at: named };
        return Effect.fromOption(source, () => refuse([new Line({ head: [named], value: 'not in view', call: new Call({ tool: 'act', given: { steps: [step] } }) })])).pipe(Effect.flatMap((taken) => capture(target.live, taken, mode, [`crop ${named}`])));
    });
    const openers = shown.filter(({ held, within }) => held._tag === 'control' && held.opens !== undefined && within.kind !== 'statusItem');
    const opened = yield* Effect.forEach(openers, ({ held }) => Address.read(target, { _tag: 'menu', ref: held.ref }));
    const levels: ReturnType<typeof Address.locate> = node.live ? node.containers.flatMap(({ kind, ref }) => (kind === 'menu' ? Option.toArray(Address.get(read, ref)) : [])) : node.containers.flatMap(({ locator }) => Array.take(Address.locate(read, locator), 1));
    const outline = yield* Effect.forEach([...levels.map((each) => Outline.render(read, { in: Option.some(each) })), ...opened.map((menu) => Outline.render(menu, { in: Option.flatMap(Array.head(menu.model.elements), ({ ref }) => Address.get(menu, ref)) }))], Effect.fromResult);
    return [...drawn.flat(), ...outline.flat().map((line) => ({ line }))];
});
const document = Effect.fnUntraced(function* (id: string, mode: Parameters<typeof capture>[2]) {
    const { running } = yield* ApplicationServer;
    const showing = Array.filterMap(yield* SubscriptionRef.get(running), (each) => {
        const numbers = windows(each.state).flatMap((shown) => (shown.document === id ? [shown.number] : []));
        return Array.isReadonlyArrayNonEmpty(numbers) ? Result.succeed({ each, numbers }) : Result.failVoid;
    });
    yield* Effect.when(ApplicationFailure.of('notShown', `document ${id}`), Effect.succeed(Array.isReadonlyArrayEmpty(showing)));
    yield* Target.hold(showing.map(({ each }) => Target.Hold.Using({ instance: each.instance })));
    const drawn = yield* Effect.forEach(showing, ({ each, numbers }) => Effect.map(capture(each, { _tag: 'windows', numbers }, mode), Array.prepend({ line: new Line({ head: [each.name ?? each.identifier] }) })), { concurrency: 'unbounded' });
    return drawn.flat();
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const View = {
    tool,
    run: Effect.fnUntraced(function* ({ of = 'front', id, in: within, marks = false, seconds, frames: limit, into }: Tool.Parameters<typeof tool>) {
        const target = yield* Effect.cached(Target.current);
        const read = yield* Effect.cached(Effect.flatMap(target, (aim) => Address.read(aim)));
        const marked = yield* Effect.when(read, Effect.succeed(marks));
        const labels = Option.toArray(marked)
            .flatMap(Address.list)
            .flatMap(({ element: held, token }) => (held._tag === 'control' ? Option.toArray(Option.all({ frame: Option.fromUndefinedOr(held.frame), label: token })) : []));
        const named = Effect.fromNullishOr(id);
        const mode = (instance: Instance['instance']): Capture['mode'] =>
            seconds === undefined ? { _tag: 'still', marks: Option.exists(marked, (held) => held.instance === instance) ? labels : [] } : { _tag: 'recording', keyFrameLimit: limit, duration: Duration.seconds(seconds), ...(into === undefined ? {} : { movie: pathToFileURL(into) }) };
        const drawn: Parameters<typeof attach>[0] = yield* Match.value(of).pipe(
            Match.when(
                Match.is('front', 'all'),
                Effect.fnUntraced(function* (scope: 'front' | 'all') {
                    const { row, live } = yield* target;
                    const numbers = scope === 'front' ? Option.toArray(front(live.state)) : windows(live.state).flatMap(({ number, kind }) => (kind === 'menu' ? [] : [number]));
                    if (Array.isReadonlyArrayNonEmpty(numbers)) {
                        return yield* capture(live, { _tag: 'windows', numbers }, mode);
                    }
                    const item = Array.findFirst(Address.list(yield* read), ({ element: held, containers, token }) => (held._tag === 'control' && Array.lastNonEmpty(containers).kind === 'statusItem' ? token : Option.none()));
                    return yield* refuse([new Line({ head: ['windows'], value: 'none shown', call: Option.match(item, { onNone: () => Target.open(row.name), onSome: (at) => new Call({ tool: 'act', given: { steps: [{ at }] } }) }) })]);
                }),
            ),
            Match.when('document', () => Effect.flatMap(named, (held) => document(held, mode))),
            Match.when(
                'object',
                Effect.fnUntraced(function* () {
                    const [{ row }, render, server, side, held] = yield* Effect.all([target, Render, ApplicationServer, Side, named]);
                    const { image, title } = yield* render(row, held);
                    const { png, width, height } = yield* server.scale({ image, longestSide: side });
                    return [{ line: new Line({ head: ['object', Format.quoted(title)], facts: [Address.print('region')({ _tag: 'size', value: Tuple.make(width, height) })] }), frame: { png } }];
                }),
            ),
            Match.when(Predicate.isString, (at) => Effect.flatMap(Effect.all([target, read]), ([aim, held]) => element(aim, held, at, mode))),
            Match.orElse(
                Effect.fnUntraced(function* ([x, y, wide, high]: readonly [number, number, number, number]) {
                    const aim = yield* target;
                    const [viewed, window] = yield* Effect.all([Handle.use((handle) => handle.get(Viewed)), Effect.transposeOption(Option.map(Option.fromUndefinedOr(within), (at) => Address.reach(aim, at)))]);
                    const place = point(viewed, { ...aim.live, window });
                    const [origin, corner] = yield* Effect.fromResult(Result.all([place([x, y]), place([x + wide, y + high])]));
                    return yield* capture(aim.live, { _tag: 'region', frame: [origin.at, [corner.at[0] - origin.at[0], corner.at[1] - origin.at[1]]] }, mode, [`crop [${[x, y, wide, high].join(',')}]`]);
                }),
            ),
        );
        const lines = yield* attach(drawn);
        const fronted = drawn.flatMap(({ frame }) => Array.fromNullishOr(frame?.presentation)).some((held) => held._tag === 'forward' || (held._tag === 'restored' && !held.back));
        const subject = of === 'document' ? [] : yield* Effect.flatMap(target, ({ row, live }) => Outline.subject(row.name, live, { facts: fronted ? [Outline.say.front] : [] }));
        return new Report({ lines: [...subject, ...lines] });
    }, Effect.scoped),
    Side,
    Viewed,
    Render,
    limit: LIMIT,
    attach,
    front,
    frames,
    point,
    windows,
} as const;

// --- [EXPORTS] -------------------------------------------------------------------------

export { View };
