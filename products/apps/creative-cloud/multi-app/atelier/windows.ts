import { Act, Acted, type Capture, ElementID, Node, type RunningInstance, State, WindowID } from '@rasm/applications';
import { ApplicationServer } from '@rasm/applications/node';
import type { Application } from '@rasm/creative-cloud-desktop';
import { Content, Link, Server } from '@rasm/mcp';
import { Output, setting } from '@rasm/mcp/node';
import { Rendering, Specifier } from '@rasm/object-model';
import type { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Effect, FileSystem, Match, Option, Path, Record, Result, Schema, Struct, SubscriptionRef } from 'effect';
import { Tool } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { Inventory } from '#/applications';

// --- [MODELS] --------------------------------------------------------------------------

const Preview = setting('preview', 'preview', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), 1568, 'Longest side in pixels of the image a view returns inline, its file keeping full resolution');
const Shot = Schema.Struct({
    application: Schema.String,
    window: Schema.OptionFromOptionalKey(WindowID.annotate({ description: 'Absent for an object an extension rendered' })),
    title: Schema.OptionFromOptionalKey(Schema.String),
    document: Schema.OptionFromOptionalKey(Schema.String.annotate({ description: 'Document id, its file URL' })),
    image: Schema.OptionFromOptionalKey(Content.preview),
    failure: Schema.OptionFromOptionalKey(Schema.String),
});
const View = Tool.make('view', {
    description:
        'Shows windows as images, each with its file at full resolution: the front window of the target application, every window of every running Adobe application, one element cropped from its window, or the windows showing one document; object shows one document, page, spread, artboard, layer, composition, or sequence the linked extension of the target application renders, with no window needed',
    parameters: Schema.Struct({
        of: Schema.optionalKey(Schema.Literals(['front', 'all', 'element', 'document', 'object']).annotate({ description: 'front when absent' })),
        id: Schema.optionalKey(Schema.String.annotate({ description: 'Element id from inspect or an event for element, document id from a result for document, object specifier from a script result for object' })),
    }),
    success: Schema.Array(Shot),
}).annotate(Tool.Readonly, true);
const Inspect = Tool.make('inspect', {
    description: 'Reads the Accessibility tree of the target application one tier at a time: the application holds its windows and menu bar, each element its role, label, value, settable, enabled, actions, frame, and child count, an id drilling into its children',
    parameters: Schema.Struct({
        id: Schema.optionalKey(ElementID.annotate({ description: 'Element to read, the application when absent' })),
        depth: Schema.optionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Child levels below the element, 1 when absent' })),
    }),
    success: Node,
}).annotate(Tool.Readonly, true);
const Acting = Tool.make('act', {
    description: 'Runs Accessibility actions on element ids of the target application in order, without focus: press, set a value, choose a menu item, or post a key with modifiers, then returns each outcome and the settled state',
    parameters: Act,
    success: Acted,
}).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false)));

// --- [OPERATIONS] ----------------------------------------------------------------------

const presented = (state: typeof State.Type) => State.matchOrElse(state, { running: ({ running }) => running.windows.filter((window) => window.presented && window.kind !== 'menu') }, () => []);
const fronted = (state: typeof State.Type) => {
    const shown = presented(state);
    return Option.orElse(
        Array.findFirst(shown, (window) => window.asking || window.element?.subrole === 'AXStandardWindow'),
        () => Array.head(shown),
    );
};
const titled = (state: typeof State.Type, number: number) =>
    Option.flatMap(
        Array.findFirst(presented(state), (window) => window.number === number),
        (window) => Option.fromUndefinedOr(window.title ?? window.element?.title),
    );
const framed = (taken: Result.Result.Success<(typeof Capture.Result.Type)[string]>) => ({
    document: Option.fromUndefinedOr(taken.document),
    image: Option.map(taken.preview, (data) => ({ data, mimeType: 'image/png', file: new URL(taken.file), width: taken.width, height: taken.height })),
    failure: Option.none<string>(),
});
const shot = (application: string, state: typeof State.Type) => (number: string, outcome: (typeof Capture.Result.Type)[string]) => ({
    application,
    window: Option.some(Number(number)),
    title: titled(state, Number(number)),
    ...Result.match(outcome, { onFailure: ({ message }) => ({ document: Option.none<string>(), image: Option.none(), failure: Option.some(message) }), onSuccess: framed }),
});
const named = (listed: readonly Application[]) => (each: RunningInstance) => ({
    each,
    name: Option.getOrElse(
        Option.map(
            Array.findFirst(listed, (row) => row.url.href === each.url.href),
            Struct.get('name'),
        ),
        () => each.url.href,
    ),
});
const showing = (document: string) => (row: { readonly each: RunningInstance; readonly name: string }) =>
    Array.match(
        presented(row.each.state).filter((window) => window.element?.document === document),
        { onEmpty: () => [], onNonEmpty: (shown) => [{ ...row, numbers: Array.map(shown, Struct.get('number')) }] },
    );

// --- [COMPOSITION] ---------------------------------------------------------------------

const windows = Effect.fnUntraced(function* (links: { readonly call: (holds: (peer: Link) => boolean, subject: string) => Effect.Effect<RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof Rendering>, RpcClientError.RpcClientError>, Unmatched> }) {
    const [server, { current, running, catalog }, side, { folder }, files, path, random] = yield* Effect.all([ApplicationServer, Inventory, Preview, Output, FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom]);
    const shots = Effect.fnUntraced(function* (application: string, each: RunningInstance, source: Capture['source']) {
        const taken = yield* server.capture({ instance: each.instance, source, longestSide: Option.some(side) });
        return Record.collect(taken, shot(application, each.state));
    });
    const everyone = Effect.gen(function* () {
        const [rows, listed] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
        return rows.filter((each) => !State.isAnyOf(['exited'])(each.state)).map(named(listed));
    });
    const front = Effect.gen(function* () {
        const { row, live } = yield* current;
        const window = yield* Effect.fromOption(fronted(live.state), () => new Cause.NoSuchElementError(`${row.name} presents no window`));
        return yield* shots(row.name, live, Option.some({ windows: { numbers: [window.number] } }));
    });
    const all = Effect.gen(function* () {
        const rows = yield* everyone;
        const shown = Array.getSomes(rows.map(({ each, name }) => Option.all({ each: Option.some(each), name: Option.some(name), numbers: Array.match(presented(each.state).map(Struct.get('number')), { onEmpty: Option.none, onNonEmpty: Option.some }) })));
        const taken = yield* Effect.forEach(shown, ({ each, name, numbers }) => shots(name, each, Option.some({ windows: { numbers } })), { concurrency: 'unbounded' });
        return Array.flatten(taken);
    });
    const element = Effect.fnUntraced(function* (id: string) {
        const { row, live } = yield* current;
        return yield* shots(row.name, live, Option.some({ element: { id } }));
    });
    const document = Effect.fnUntraced(function* (id: string) {
        const rows = yield* everyone;
        const taken = yield* Effect.forEach(rows.flatMap(showing(id)), ({ each, name, numbers }) => shots(name, each, Option.some({ windows: { numbers } })), { concurrency: 'unbounded' });
        return Array.flatten(taken);
    });
    const object = Effect.fnUntraced(function* (id: string) {
        const [{ row }, target] = yield* Effect.all([current, Schema.decodeEffect(Specifier.Path)(id)]);
        const client = yield* links.call((peer) => Link.hosted(row)(peer) && Link.serves(Rendering.requests.keys())(peer), `linked extension of ${row.name} rendering objects`);
        const [{ image }, name] = yield* Effect.all([client('render', { target }), random.uuidV7()]);
        const written = path.join(folder(row.name, 'captures'), `${name}.png`);
        yield* Effect.andThen(files.makeDirectory(path.dirname(written), { recursive: true }), files.writeFile(written, image));
        const file = yield* path.toFileUrl(written);
        const { width, height, preview } = yield* server.preview({ file, longestSide: side });
        return [{ application: row.name, window: Option.none(), title: Option.some(target.leaf), document: Option.none(), image: Option.some({ data: preview, mimeType: 'image/png', file, width, height }), failure: Option.none() }];
    });
    yield* Server.registerTool(
        View,
        Effect.fnUntraced(function* ({ of = 'front', id }) {
            const asked = [of, ...(id === undefined ? [] : [id])].join(' ');
            const identified = Effect.mapError(Effect.fromNullishOr(id), () => new Cause.IllegalArgumentError(`view of ${asked} takes an id`));
            const taken = yield* Match.value(of).pipe(
                Match.when('front', () => front),
                Match.when('all', () => all),
                Match.when('element', () => Effect.flatMap(identified, element)),
                Match.when('document', () => Effect.flatMap(identified, document)),
                Match.when('object', () => Effect.flatMap(identified, object)),
                Match.exhaustive,
            );
            return yield* Effect.fromOption(Option.liftPredicate(taken, Array.isReadonlyArrayNonEmpty), () => new Cause.NoSuchElementError(`Nothing to view for ${asked}`));
        }),
    );
    yield* Server.registerTool(
        Inspect,
        Effect.fnUntraced(function* ({ id, depth = 1 }) {
            const { live } = yield* current;
            return yield* server.nodes({ instance: live.instance, ...(id === undefined ? {} : { root: id }), depth });
        }),
    );
    yield* Server.registerTool(
        Acting,
        Effect.fnUntraced(function* ({ actions }) {
            const { live } = yield* current;
            return yield* server.act({ instance: live.instance, actions });
        }),
    );
});

// --- [EXPORTS] -------------------------------------------------------------------------

export { Preview, windows };
