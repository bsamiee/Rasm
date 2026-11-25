import { pathToFileURL } from 'node:url';
import { Content, Server } from '@rasm/mcp';
import { Output, setting } from '@rasm/mcp/cli';
import { Unmatched } from '@rasm/search';
import { Array, Cause, Context, Duration, Effect, FileSystem, HashSet, Match, Option, Path, Predicate, Record, Result, Schema, Struct, SubscriptionRef, UndefinedOr } from 'effect';
import { Tool } from 'effect/ai';
import { SecureRandom } from 'effect/crypto';
import { ApplicationServer } from '#/client';
import { Act, Acted, AppleEventValue, type Capture, Diagnostics, ElementID, Installation, Node, QuitMode, RunningInstance, State, Terms, WindowID } from '#/contract';
import { type Entry, Inventory } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Scripting<E, R> = (target: { readonly row: Entry; readonly live: RunningInstance }, code: string) => Effect.Effect<{ readonly result: Option.Option<Schema.Json> }, E, R>;

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;
const INSTRUCTIONS = [
    'Start with applications, which lists what is installed and running, and whose open makes one application the target. The first result names a handle: pass it on each later call, since it holds your target and your place in the events.',
    'Each result lists the events since your last call: a dialog asking for an answer, a window opening or closing, an application exiting, crashing, or turning unresponsive, and memory or thermal pressure. Answer a dialog before other work in its application.',
    'A call returns at once when a dialog needs an answer, one open before it included, or its application exits or stops responding; the call keeps running under the call number in early, and finished of a later result holds its result once it finishes, act answering the dialog waiting for it.',
    'Take element, window, and document ids from results and events, never compose them.',
] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Preview = setting('preview', 'preview', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), 1568, 'Longest side in pixels of the image a view returns inline, its file keeping full resolution');
const Stated = Schema.toCodecJson(Schema.Struct({ installation: Schema.OptionFromOptionalKey(Installation), open: Terms.fields.open }));
const Reply = Schema.toCodecJson(AppleEventValue);
const TOOLS = {
    applications: Tool.make('applications', {
        description:
            'Lists installed applications with the running instance of each, an application another MCP server drives with that server and its scope, and one named application with its installation and the open command of its dictionary; open launches one in the background, or makes its running instance the target of the handle; close quits one, or each the handle opened or targeted, each save dialog arriving as an event act answers',
        parameters: Schema.Struct({
            action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
            application: Schema.optionalKey(
                Schema.String.annotate({
                    description: 'Listed name, alias, or bundle URL: list with one states its installation and the open command of its dictionary, open with none takes the application the system opens the documents with, close with none quits each application the handle opened or targeted',
                }),
            ),
            documents: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Absolute file paths or URLs open hands to the application' })),
            arguments: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Process arguments of a new instance, open failing where they would reach a running one' })),
            environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment variables of a new instance, open failing as it does for arguments' })),
            new: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Open an instance beside the running ones' })),
            opening: Schema.optionalKey(
                Schema.Record(Schema.String, Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])).annotate({ description: 'Parameters of the open command by the names a listing of the application states, sent with the documents once the instance settles behind the frontmost application' }),
            ),
            mode: Schema.optionalKey(QuitMode),
            wait: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds: open follows the instance until it exits or they pass and states its last state, close returns after them at the latest' })),
        }),
        success: Schema.Union([
            Schema.Struct({ installed: Schema.Array(Schema.Record(Schema.String, Schema.Json)), current: Schema.optionalKey(Schema.String) }),
            Schema.Struct({ application: Schema.String, instance: RunningInstance.fields.instance, created: Schema.Boolean, state: Schema.OptionFromOptionalKey(State), opened: Schema.OptionFromOptionalKey(AppleEventValue) }),
            Schema.Struct({ closed: Schema.Array(Schema.Struct({ application: Schema.String, instance: RunningInstance.fields.instance, state: Schema.OptionFromOptionalKey(State) })) }),
        ]),
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true }))),
    view: Tool.make('view', {
        description:
            'Shows windows as images, each with its file at full resolution: the front window of the target application, every window of every running listed application, one element cropped from its window, or the windows showing one document; object shows one document object the target application renders with no window, where the server has a source rendering it',
        parameters: Schema.Struct({
            of: Schema.optionalKey(Schema.Literals(['front', 'all', 'element', 'document', 'object']).annotate({ description: 'front when absent' })),
            id: Schema.optionalKey(Schema.String.annotate({ description: 'Element id from inspect or an event for element, document id from a result for document, id a script result gives a document object for object' })),
        }),
        success: Schema.Array(
            Schema.Struct({
                application: Schema.String,
                window: Schema.OptionFromOptionalKey(WindowID),
                title: Schema.OptionFromOptionalKey(Schema.String),
                document: Schema.OptionFromOptionalKey(Schema.String),
                image: Schema.OptionFromOptionalKey(Content.preview),
                failure: Schema.OptionFromOptionalKey(Schema.String),
            }),
        ),
    }).annotate(Tool.Readonly, true),
    inspect: Tool.make('inspect', {
        description: 'Reads the Accessibility tree of the target application one tier at a time: the application holds its windows and menu bar, each element its role, label, value, settable, enabled, actions, frame, and child count, an id drilling into its children',
        parameters: Schema.Struct({
            id: Schema.optionalKey(ElementID.annotate({ description: 'Element to read, the application when absent' })),
            depth: Schema.optionalKey(Schema.Int.check(Schema.isGreaterThan(0)).annotate({ description: 'Child levels below the element, 1 when absent' })),
        }),
        success: Node,
    }).annotate(Tool.Readonly, true),
    act: Tool.make('act', {
        description:
            'Runs Accessibility actions on element ids of the target application in order, without focus: press, set a value, choose a menu item, or post a key with modifiers, then returns each outcome and the settled state, after each call that returned early finishes or its application asks again or stops responding',
        parameters: Act,
        success: Acted,
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false))),
    execute_script: Tool.make('execute_script', {
        description:
            'Runs source through the script command the scripting dictionary of the target application declares and returns its reply as JSON, refusing with the commands of the dictionary where it declares none; a dialog needing an answer, open before the call or raised by it, the application exiting, or it turning unresponsive returns the call at once, its result arriving in finished of a later result',
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Source the script runtime of the target application runs' }) }),
        success: Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json) }),
    }).annotate(Tool.OpenWorld, false),
    diagnostics: Tool.make('diagnostics', {
        description: 'Reads diagnostics newest first: without an application, the log lines of both processes of the server as JSON; with one, its crash, hang, and diagnostic reports from both DiagnosticReports folders and its unified log',
        parameters: Schema.Struct({
            application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, alias, or bundle URL from applications, the server itself when absent' })),
            since: Diagnostics.fields.since,
            limit: Diagnostics.fields.limit.annotate({ default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
            find: Schema.optionalKey(Schema.String.annotate({ description: 'Text a log line or unified log event holds in any case, kept from the newest limit of each' })),
        }),
        success: Diagnostics.Result,
    }).annotate(Tool.Readonly, true),
};

// --- [OPERATIONS] ----------------------------------------------------------------------

const presented = (state: RunningInstance['state']) => State.matchOrElse(state, { running: ({ running }) => running.windows.filter((window) => window.presented && window.kind !== 'menu') }, () => []);
const fronted = (state: RunningInstance['state']) => {
    const shown = presented(state);
    return Option.orElse(
        Array.findFirst(shown, (window) => window.asking || window.element?.subrole === 'AXStandardWindow'),
        () => Array.head(shown),
    );
};
const titled = (state: RunningInstance['state'], number: number) =>
    Option.flatMap(
        Array.findFirst(presented(state), (window) => window.number === number),
        (window) => Option.fromUndefinedOr(window.title ?? window.element?.title),
    );
const named = (entries: readonly Entry[]) => (each: RunningInstance) => ({
    each,
    name: Option.getOrElse(
        Option.map(
            Array.findFirst(entries, (row) => row.url.href === each.url.href),
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
const described = <const Name extends string, P extends Schema.Top, S extends Schema.Top>(tool: Tool.Tool<Name, { readonly parameters: P; readonly success: S; readonly failure: Schema.Never; readonly failureMode: 'error' }>, description = tool.description) =>
    Tool.make(tool.name, { description, parameters: tool.parametersSchema, success: tool.successSchema }).annotateMerge(tool.annotations);
const listed = (instances: readonly RunningInstance[]) => (row: Entry) => ({
    ...row.facts,
    name: row.name,
    ...Option.getOrUndefined(Option.map(row.claim, ({ server: owner, scope }) => ({ claimed: { server: owner, scope } }))),
    ...Option.getOrUndefined(
        Option.map(Array.findFirst(instances, Installation.runs(row)), ({ instance, state }) => ({
            running: {
                instance,
                status: State.match(state, { running: ({ running: live }) => live.status, failed: () => 'failed', exited: () => 'exited' }),
                windows: State.matchOrElse(state, { running: ({ running: live }) => live.windows.filter(Struct.get('presented')).length }, () => 0),
            },
        })),
    ),
});
const scripted = Effect.fnUntraced(function* ({ row, live }: Parameters<Scripting<never, never>>[0], code: string) {
    const server = yield* ApplicationServer;
    const { script, command } = yield* server.terms({ url: row.url });
    yield* Effect.fromOption(script, () => Unmatched.of({ subject: `script command in the dictionary of ${row.name}`, candidates: command.map(Struct.get('name')) }));
    const reply = yield* server.execute({ instance: live.instance, arguments: { direct: code } });
    return { result: Option.some(yield* Schema.encodeEffect(Reply)(reply)) };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const register = Effect.fnUntraced(function* <E1 extends Error = never, R1 = never, E2 extends Error = never, R2 = never, E3 extends Error = never, R3 = never>({
    descriptions = {},
    scripts,
    render,
    logs,
}: {
    readonly descriptions?: Readonly<Partial<Record<keyof typeof TOOLS, string>>>;
    readonly scripts?: Scripting<E1, R1>;
    readonly render?: (row: Entry, id: string) => Effect.Effect<{ readonly image: Uint8Array; readonly title: string }, E2, R2>;
    readonly logs?: (row: Entry) => Effect.Effect<Diagnostics['folders'], E3, R3>;
} = {}) {
    const [server, { catalog, running, target, aimed, found, located, current, ends }, side, output, files, path, random] = yield* Effect.all([ApplicationServer, Inventory, Preview, Output, FileSystem.FileSystem, Path.Path, SecureRandom.SecureRandom]);
    const stated = Effect.fnUntraced(function* (row: Entry, instances: readonly RunningInstance[]) {
        const [installations, terms] = yield* Effect.all([server.installed, Effect.option(Effect.tapError(server.terms({ url: row.url }), (cause) => Effect.logDebug('Scripting dictionary unread', cause)))]);
        const launch = yield* Schema.encodeEffect(Stated)({ installation: Array.findFirst(installations, (each) => each.url.href === row.url.href), open: Option.flatMap(terms, Struct.get('open')) });
        return { ...listed(instances)(row), launch };
    });
    const list = Effect.fnUntraced(function* (given: string | undefined) {
        const [instances, { held }] = yield* Effect.all([SubscriptionRef.get(running), target]);
        const installed = yield* UndefinedOr.match(given, {
            onUndefined: () => Effect.map(SubscriptionRef.get(catalog), Array.map(listed(instances))),
            onDefined: (name) =>
                Effect.map(
                    Effect.flatMap(found(name), (row) => stated(row, instances)),
                    Array.of,
                ),
        });
        return { installed, ...Option.getOrUndefined(Option.map(held, ({ row }) => ({ current: row.name }))) };
    });
    const open = Effect.fnUntraced(function* (given: string | undefined, { documents = [], wait, ...launch }: Omit<Tool.Parameters<typeof TOOLS.applications>, 'action' | 'application' | 'mode'>) {
        const urls = documents.map((document) => (URL.canParse(document) ? new URL(document) : pathToFileURL(document)));
        const handled = Effect.forEach(urls, (document) => server.handler({ document }), { concurrency: 'unbounded' }).pipe(
            Effect.map(Array.map((handler) => handler.url.href)),
            Effect.map(Array.dedupe),
            Effect.filterOrFail(
                Predicate.isTupleOf(1),
                (hrefs) => new Cause.IllegalArgumentError(Array.match(hrefs, { onEmpty: () => 'open names an application from the list, or documents the system has an application for', onNonEmpty: (several) => `${several.join(', ')} open these documents, one open takes the documents of one application` })),
            ),
            Effect.map(Array.headNonEmpty),
        );
        const [row, instances] = yield* Effect.all([Effect.flatMap(UndefinedOr.match(given, { onUndefined: () => handled, onDefined: Effect.succeed }), located), SubscriptionRef.get(running)]);
        const reused = Option.filter(Array.findFirst(instances, Installation.runs(row)), () => Array.isReadonlyArrayEmpty([...Struct.keys(launch), ...urls]) && wait === undefined);
        const { instance, created, state, opened } = yield* Option.match(reused, {
            onSome: ({ instance: targeted }) => Effect.succeed({ instance: targeted, created: false, state: Option.none(), opened: Option.none() }),
            onNone: () => server.launched({ url: row.url, documents: urls, ...launch }, Option.map(Option.fromUndefinedOr(wait), Duration.seconds)),
        });
        yield* aimed({ row, instance });
        return { application: row.name, instance, created, state: Option.filter(state, () => wait !== undefined), opened };
    });
    const close = Effect.fnUntraced(function* (given: string | undefined, { mode, wait }: Pick<Tool.Parameters<typeof TOOLS.applications>, 'mode' | 'wait'>) {
        const [instances, entries, { followed }, chosen] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog), target, Effect.transposeOption(Option.map(Option.fromUndefinedOr(given), located))]);
        const quit = instances.filter((each) => Option.match(chosen, { onNone: () => HashSet.has(followed, each.instance) && !State.isAnyOf(['exited'])(each.state), onSome: (row) => Installation.runs(row)(each) })).map(named(entries));
        const bound = UndefinedOr.match(wait, { onUndefined: () => Duration.infinity, onDefined: Duration.seconds });
        const states = yield* Effect.forEach(quit, ({ each }) => Effect.timeoutOption(server.quit({ instance: each.instance, ...(mode === undefined ? {} : { mode }) }), bound), { concurrency: 'unbounded' });
        return { closed: Array.zipWith(quit, states, ({ each, name }, state) => ({ application: name, instance: each.instance, state })) };
    });
    const shots = (application: string, each: RunningInstance, source: Capture['source']) =>
        Effect.map(
            server.capture({ instance: each.instance, source, longestSide: Option.some(side) }),
            Record.collect((number, outcome) => ({
                application,
                window: Option.some(Number(number)),
                title: titled(each.state, Number(number)),
                ...Result.match(outcome, {
                    onFailure: ({ message }) => ({ document: Option.none<string>(), image: Option.none(), failure: Option.some(message) }),
                    onSuccess: (shot) => ({ document: Option.fromUndefinedOr(shot.document), image: Option.map(shot.preview, (data) => ({ data, mimeType: 'image/png', file: new URL(shot.file), width: shot.width, height: shot.height })), failure: Option.none<string>() }),
                }),
            })),
        );
    const everyone = Effect.gen(function* () {
        const [rows, entries] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog)]);
        return rows.filter((each) => !State.isAnyOf(['exited'])(each.state)).map(named(entries));
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
        const { row } = yield* current;
        const rendering = yield* Effect.mapError(Effect.fromNullishOr(render), () => new Cause.NoSuchElementError(`No source renders objects of ${row.name}`));
        const [{ image, title }, name] = yield* Effect.all([rendering(row, id), random.uuidV7()]);
        const written = path.join(output.folder(row.name, 'captures'), `${name}.png`);
        yield* Effect.andThen(files.makeDirectory(path.dirname(written), { recursive: true }), files.writeFile(written, image));
        const file = yield* path.toFileUrl(written);
        const { width, height, preview } = yield* server.preview({ file, longestSide: side });
        return [{ application: row.name, window: Option.none(), title: Option.some(title), document: Option.none(), image: Option.some({ data: preview, mimeType: 'image/png', file, width, height }), failure: Option.none() }];
    });
    const subject = Effect.fnUntraced(function* (given: string) {
        const [row, instances] = yield* Effect.all([located(given), SubscriptionRef.get(running)]);
        const instance = Option.map(Array.findFirst(instances, Installation.runs(row)), Struct.get('instance'));
        const folders = yield* UndefinedOr.match(logs, { onUndefined: () => Effect.succeed([]), onDefined: (kept) => kept(row) });
        return { application: { url: row.url, ...Option.getOrUndefined(Option.map(instance, (live) => ({ instance: live }))) }, folders };
    });
    const run: Scripting<Error, R1 | ApplicationServer> = scripts ?? scripted;
    yield* Server.registerTool(described(TOOLS.applications, descriptions.applications), ({ action = 'list', application, ...given }) =>
        Match.value(action).pipe(
            Match.when('list', () => list(application)),
            Match.when('open', () => open(application, Struct.omit(given, ['mode']))),
            Match.when('close', () => close(application, given)),
            Match.exhaustive,
        ),
    );
    yield* Server.registerTool(
        described(TOOLS.view, descriptions.view),
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
        described(TOOLS.inspect, descriptions.inspect),
        Effect.fnUntraced(function* ({ id, depth = 1 }) {
            const { live } = yield* current;
            return yield* server.nodes({ instance: live.instance, ...(id === undefined ? {} : { root: id }), depth });
        }),
    );
    yield* Server.registerTool(
        described(TOOLS.act, descriptions.act),
        Effect.fnUntraced(function* ({ actions }) {
            const { live } = yield* current;
            const acted = yield* server.act({ instance: live.instance, actions });
            yield* SubscriptionRef.update(
                running,
                Array.map((each) => (each.instance === live.instance ? RunningInstance.make({ ...each, state: acted.state }) : each)),
            );
            yield* Server.ToolCall.use(Struct.get('settle'));
            return acted;
        }),
    );
    yield* Server.registerTool(described(TOOLS.execute_script, descriptions.execute_script).annotateMerge(ends), ({ code }) => Effect.flatMap(current, (aim) => run(aim, code)));
    yield* Server.registerTool(
        described(TOOLS.diagnostics, descriptions.diagnostics),
        Effect.fnUntraced(function* ({ application, since, limit, find }) {
            const read = yield* UndefinedOr.match(application, { onUndefined: () => Effect.succeed({ folders: [{ url: pathToFileURL(`${output.logs}/`), glob: '*.log' }] }), onDefined: subject });
            const diagnosed = yield* server.diagnostics({ ...read, limit, ...(since === undefined ? {} : { since }) });
            return UndefinedOr.match(find, {
                onUndefined: () => diagnosed,
                onDefined: (text) => {
                    const held = new RegExp(RegExp.escape(text), 'iu');
                    return {
                        ...diagnosed,
                        logs: diagnosed.logs.filter((line) => held.test(line.text)),
                        unified: Result.map(
                            diagnosed.unified,
                            Array.filter(({ eventMessage }) => eventMessage !== undefined && held.test(eventMessage)),
                        ),
                    };
                },
            });
        }),
    );
});
const Tools = { instructions: INSTRUCTIONS, settings: [Preview] as const, register };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tools };
