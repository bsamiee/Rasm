import { pathToFileURL } from 'node:url';
import { Output, setting } from '@rasm/mcp/cli';
import { Handle, Server } from '@rasm/mcp/server';
import { Unmatched } from '@rasm/search/unmatched';
import { Array, Cause, Context, DateTime, Duration, Effect, Equal, Function, Hash, HashMap, HashSet, Match, Option, Predicate, pipe, Record, Ref, Result, Schema, Stream, Struct, SubscriptionRef, Tuple, UndefinedOr } from 'effect';
import { McpSchema, Tool } from 'effect/ai';
import { Flag } from 'effect/cli';
import type { RpcClient, RpcClientError, RpcGroup } from 'effect/rpc';
import { Address, type Index, type Item, type Shown } from '#/address';
import { ApplicationServer } from '#/client';
import { Act, AppleEventValue, type Capture, Cursor, Diagnostics, Event, type Frame, Installation, InstanceSnapshot, InstanceState, Kind, type Model, QuitMode, type Running, Scope, ScriptingDictionary, Value, Written } from '#/contract';
import { type InterfaceMap, InterfaceMaps } from '#/interface-map';
import { Outline } from '#/outline';
import { type Declaration, Description, type SourceRpcs } from '#/source';
import { describeUsage, type Entry, Workspace } from '#/workspace';

// --- [TYPES] ---------------------------------------------------------------------------

type Scripting<E, R> = (target: Effect.Success<Workspace['Service']['current']>, code: string) => Effect.Effect<Tool.Success<typeof TOOLS.execute_script>, E, R>;
type Linked = RpcClient.RpcClient.Flat<RpcGroup.Rpcs<typeof SourceRpcs>, RpcClientError.RpcClientError>;
type Drawn = Option.Option<{ readonly instance: InstanceSnapshot['instance']; readonly marks: ReturnType<typeof marksOf> }>;
type Sample = Exclude<Extract<Tool.Parameters<typeof TOOLS.act>['steps'][number], { readonly pointer: unknown }>['pointer'], string>;
interface Placement {
    readonly instance: InstanceSnapshot['instance'];
    readonly windows: readonly number[];
    readonly frame: typeof Frame.Type;
    readonly size: readonly [number, number];
}

// --- [CONSTANTS] -----------------------------------------------------------------------

const LIMIT = 50;
const INSTRUCTIONS = ['Start with applications: it lists what is installed and running, and open makes one application the target of later calls. Each result says what changed since your last call and, where an answer or a continuation is due, the call that gives it.'] as const;

// --- [MODELS] --------------------------------------------------------------------------

const Side = setting('longest-side', Schema.FiniteFromString.pipe(Schema.decodeTo(Schema.Int.check(Schema.isGreaterThan(0)))), Flag.withDefault(1568), 'Longest side in pixels of each image a view returns');
const ApplicationListing = Schema.toCodecJson(Schema.Struct({ installation: Schema.OptionFromOptionalKey(Installation), open: ScriptingDictionary.fields.open }));
const Reply = Schema.toCodecJson(AppleEventValue);
const Title = Schema.fromJsonString(Schema.String);
const Pixel = Schema.Finite.check(Schema.isGreaterThanOrEqualTo(0));
const Pixels = Schema.Tuple([Pixel, Pixel, Schema.Finite.check(Schema.isGreaterThan(0)), Schema.Finite.check(Schema.isGreaterThan(0))]);
const LogRecord = Schema.fromJsonString(
    Schema.Struct({ timestamp: Schema.String, severity_text: Schema.String, scope: Schema.optionalKey(Schema.String), body: Schema.String, attributes: Schema.optionalKey(Schema.Record(Schema.String, Schema.Json)) }).annotate({ description: 'OpenTelemetry log record a server process writes as one JSON line' }),
);
const TOOLS = {
    applications: Tool.make('applications', {
        description:
            'Lists installed applications and their running instances with the status items each shows in the menu bar, each one another MCP server drives naming that server, or one application with its installation, its App Intents actions with whether each opens it, and open command; open launches an application in the background or targets its running instance for later calls; close quits it, or every application this handle opened, each save dialog arriving as an event',
        parameters: Schema.Struct({
            action: Schema.optionalKey(Schema.Literals(['list', 'open', 'close']).annotate({ description: 'list when absent' })),
            application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, alias, or bundle URL; open without it takes the application the system opens the documents with, close without it quits every application this handle opened' })),
            documents: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: "File paths or URLs the application opens; list without application lists every application opening them, the system's default first" })),
            arguments: Schema.optionalKey(Schema.Array(Schema.String).annotate({ description: 'Process arguments of a new instance, refused where a running instance would receive them' })),
            environment: Schema.optionalKey(Schema.Record(Schema.String, Schema.String).annotate({ description: 'Environment variables of a new instance, refused as arguments are' })),
            new: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Open an instance beside the running ones' })),
            opening: Schema.optionalKey(Schema.Record(Schema.String, Schema.Union([Schema.Boolean, Schema.Finite, Schema.String])).annotate({ description: 'Open command parameters by the names the listing of the application states, sent with the documents once the instance is idle' })),
            mode: Schema.optionalKey(QuitMode),
            wait: Schema.optionalKey(Schema.Finite.check(Schema.isGreaterThan(0)).annotate({ description: 'Seconds open follows the instance, returning its last state, or close waits at most' })),
        }),
        success: Schema.Union([
            Schema.Struct({ installed: Schema.Array(Schema.Record(Schema.String, Schema.Json)), current: Schema.optionalKey(Schema.String) }),
            Schema.Struct({ application: Schema.String, instance: InstanceSnapshot.fields.instance, created: Schema.Boolean, state: Schema.OptionFromOptionalKey(InstanceState), opened: Schema.OptionFromOptionalKey(AppleEventValue) }),
            Schema.Struct({ closed: Schema.Array(Schema.Struct({ application: Schema.String, instance: InstanceSnapshot.fields.instance, state: Schema.OptionFromOptionalKey(InstanceState) })) }),
        ]),
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.Idempotent, true), Context.add(Tool.OpenWorld, false), Context.add(Tool.Meta, { 'anthropic/alwaysLoad': true }))),
    view: Tool.make('view', {
        description:
            'Shows windows as images, a line naming each: the front window of the target, every window of every running application, the windows showing a document, a document object a linked source renders, or one reference cropped from its window with any menu or panel of the application above it; a menu or the item opening it shows one image per open level and the outline of those levels, a menu the application cannot open while behind drawn from what was read',
        parameters: Schema.Struct({
            of: Schema.optionalKey(
                Schema.Union([Schema.Literals(['front', 'all', 'document', 'object']), Address.At, Pixels.annotate({ description: 'Pixel rectangle [x, y, width, height] of the last image a view returned, shown again at the full resolution of the screen' })]).annotate({
                    description: 'front when absent, else a reference or label path as inspect prints it',
                }),
            ),
            id: Schema.optionalKey(Schema.String.annotate({ description: 'document: document id from a result; object: id a script result gives the object' })),
            marks: Schema.optionalKey(Schema.Boolean.annotate({ description: 'Draw each control of the target in the image with its frame and reference, as act takes it' })),
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
    inspect: Tool.make('inspect', {
        description:
            'Reads the interface of the target as an outline: a state line, then each container and its controls, each led by a reference such as @a12 with its label, value, kind, and what it takes, and under ▸ what was shown before and is gone, with its last value. A menu item leads with its mark (✓ checked), names after › the menu it opens, which in reads, its shortcut as the act key action pressing it, a separator above it, and the item an option or other modifier puts in its place. An option of a choice, a tool among them, names its shortcut the same way and its help, and the state line names the current tool with its shortcut, its help on the line below. A cut ends with the call that continues it, a dialog asking for an answer with the act call that gives it. A read of every container counts the containers remembered and not open, which find searches, and ends with the clipboard once it changed since this handle last read it: its change count, types, the kinds the system detects in it, and its text where the Paste from Other Apps setting of the owner allows the read, else why it is withheld',
        parameters: Schema.Struct({
            in: Schema.optionalKey(Address.At.annotate({ description: 'Container, choice, multi, text, menu item, or remembered container read alone, a menu item reading its menu, a multi its rows, and a text control its style runs with their fonts, colors, links, and frames; every container when absent' })),
            find: Schema.optionalKey(Schema.String.annotate({ description: 'Text searched in labels, values, and help of every container shown and remembered, best match and live hits first' })),
            kind: Schema.optionalKey(Kind.annotate({ description: 'Kind of control find keeps, or every control of the kind without find' })),
            after: Schema.optionalKey(Address.At.annotate({ description: 'Item after which the page starts, as a cut names it' })),
            wait: Schema.optionalKey(Schema.Finite.check(Schema.isBetween({ minimum: 0, maximum: 60, exclusiveMinimum: true })).annotate({ description: 'Seconds to wait for the next change of the target before reading, marking what changed' })),
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
    act: Tool.make('act', {
        description:
            'Runs steps on the target in order without bringing it forward, each aimed at a reference or label path as inspect prints it: a step with at presses it, with to sets the value its kind takes (an option label chooses, text on the clipboard reference copies), with range selects or replaces text; key posts a key with modifiers; pointer clicks, drags, strokes, hovers, or scrolls at a reference or point, holding a press across steps by edge. Each step climbs from an Accessibility call through background event routes and reads back what it changed, the system route last bringing the target forward and saying so; every malformed step is refused before any runs. Returns each outcome and the state once the target answers, idle, not responding, or quiet for a beat, with its usage saying why a busy target has not answered; inspect with wait follows it further',
        parameters: Schema.Struct({ steps: Act.steps(Address.At, {}, {}) }),
        success: Act.Result,
    }).annotateMerge(Context.make(Tool.Destructive, false).pipe(Context.add(Tool.OpenWorld, false))),
    execute_script: Tool.make('execute_script', {
        description: 'Runs code through the script command the scripting dictionary of the target declares and returns its reply as JSON, refused with the commands of the dictionary where it declares none; a dialog, an exit, or a hang returns the call at once, its result arriving under finished in a later result',
        parameters: Schema.Struct({ code: Schema.String.annotate({ description: 'Source the script runtime of the target runs' }) }),
        success: Schema.Struct({ result: Schema.OptionFromOptionalKey(Schema.Json), written: Schema.optionalKey(Written) }),
    }).annotate(Tool.OpenWorld, false),
    diagnostics: Tool.make('diagnostics', {
        description: 'Reads diagnostics newest first, one line each: without application, the log lines of the server under the file holding them; with one, its crash, hang, and diagnostic reports and its unified log',
        parameters: Schema.Struct({
            application: Schema.optionalKey(Schema.String.annotate({ description: 'Listed name, alias, or bundle URL, the server itself when absent' })),
            since: Diagnostics.fields.since,
            limit: Diagnostics.fields.limit.annotate({ default: LIMIT }).pipe(Schema.withDecodingDefaultKey(Effect.succeed(LIMIT))),
            find: Diagnostics.fields.find,
        }),
        success: Schema.String,
    }).annotate(Tool.Readonly, true),
};

// --- [SERVICES] ------------------------------------------------------------------------

const Clipboard = Context.Reference<Option.Option<number>>('@rasm/applications/tools/Clipboard', { defaultValue: Option.none });
const Viewed = Context.Reference<Option.Option<Placement>>('@rasm/applications/tools/Viewed', { defaultValue: Option.none });

// --- [OPERATIONS] ----------------------------------------------------------------------

const screenOf = ({ frame: [[left, top], [width, height]], size: [columns, rows] }: Placement, [x, y, wide, high]: readonly [number, number, number, number]) =>
    x + wide <= columns && y + high <= rows
        ? Result.succeed<typeof Frame.Type>([
              [left + (x * width) / columns, top + (y * height) / rows],
              [(wide * width) / columns, (high * height) / rows],
          ])
        : Result.fail(new Cause.IllegalArgumentError(`[${[x, y, wide, high].join(', ')}] leaves the last image a view returned, ${columns}×${rows} pixels`));
const pointOf = (viewed: Option.Option<Placement>, [x, y, ...rest]: Sample) =>
    Option.match(viewed, {
        onNone: () => Result.fail(new Cause.IllegalArgumentError(`[${x}, ${y}] names a pixel of the last image a view of the target returned, and this handle holds none`)),
        onSome: (placement) => Result.map(screenOf(placement, [x, y, 0, 0]), ([[left, top]]): Sample => [left, top, ...rest]),
    });
const marksOf = (index: Index) => index.shown.flatMap(({ controls }) => controls.flatMap(({ control, item }) => Option.toArray(Option.all({ frame: Option.fromUndefinedOr(control.frame), label: item.token }))));
const locate = (document: string) => (URL.canParse(document) ? new URL(document) : pathToFileURL(document));
const presentedWindows = (state: InstanceSnapshot['state']) => InstanceState.matchOrElse(state, { running: ({ windows }) => windows.filter((window) => window.presented && window.kind !== 'menu') }, () => []);
const describeTool = <const Name extends string, P extends Schema.Top, S extends Schema.Top>(tool: Tool.Tool<Name, { readonly parameters: P; readonly success: S; readonly failure: Schema.Never; readonly failureMode: 'error' }>, description = tool.description) =>
    Tool.make(tool.name, { description, parameters: tool.parametersSchema, success: tool.successSchema }).annotateMerge(tool.annotations);
const listEntry = (instances: readonly InstanceSnapshot[]) => (row: Entry) => ({
    ...row.facts,
    name: row.name,
    ...Option.getOrUndefined(Option.map(row.claim, (claim) => ({ claimed: Struct.pick(claim, ['server', 'scope']) }))),
    ...Option.getOrUndefined(
        Option.map(Array.findFirst(instances, Installation.runs(row)), ({ instance, state }) => ({
            running: {
                instance,
                ...InstanceState.matchOrElse(state, { running: ({ statusItems = 0, ...activity }) => ({ ...Struct.pick(activity, ['status', 'statusItemsFailure']), ...(statusItems > 0 ? { statusItems } : {}) }) }, ({ _tag }) => ({ status: _tag })),
                windows: presentedWindows(state).length,
            },
        })),
    ),
});
const answerOf = ({ container, item, controls }: Shown) =>
    Option.all({
        asking: item.token,
        ref: Option.orElse(Option.fromUndefinedOr(container.default), () =>
            Option.map(
                Array.findFirst(controls, ({ control }) => control.kind === 'action'),
                ({ control }) => control.ref,
            ),
        ),
    });
const scopeOf = (start: Option.Option<Item>) =>
    Match.type<Item>().pipe(
        Match.when({ target: { _tag: 'Container' } }, ({ target: { container } }) => Result.succeedSome<typeof Scope.Type>({ _tag: 'container', ref: container.ref })),
        Match.when({ target: { _tag: 'Control', control: { kind: 'multi' } } }, ({ target: { control } }) =>
            Result.succeedSome<typeof Scope.Type>({
                _tag: 'rows',
                ref: control.ref,
                from: pipe(
                    Option.flatMap(start, ({ target }) => (target._tag === 'Control' ? Option.fromUndefinedOr(target.control.index) : Option.none())),
                    Option.match({ onNone: () => 0, onSome: (index) => index + 1 }),
                ),
                count: Outline.rows,
            }),
        ),
        Match.when({ target: { _tag: 'Control', control: { kind: 'text' } } }, ({ target: { control } }) => Result.succeedSome<typeof Scope.Type>({ _tag: 'text', ref: control.ref })),
        Match.when({ target: { _tag: 'Control', control: { kind: 'choice' } } }, () => Result.succeedNone),
        Match.when({ target: { _tag: 'Control', control: { opens: Match.number } } }, ({ target: { control } }) => Result.succeedSome<typeof Scope.Type>({ _tag: 'menu', ref: control.ref })),
        Match.when({ target: { _tag: 'RememberedContainer' } }, () => Result.succeedNone),
        Match.orElse(({ token, name, kind, live }) =>
            Result.fail(
                new Cause.IllegalArgumentError(
                    `in reads a container, a choice, a multi, a text control, a menu item with a submenu, or a remembered container, and ${Option.getOrElse(
                        Option.orElse(token, () => name),
                        () => 'the item',
                    )} is ${Option.match(kind, { onNone: () => 'an option', onSome: (held) => `${live ? 'a' : 'a remembered'} control of kind ${held}` })}`,
                ),
            ),
        ),
    );
const menuPath = (index: Index, item: Item): readonly Item[] => {
    const menuOf = (ref: number) => Option.filter(Record.get(index.live, `${ref}`), ({ target }) => target._tag === 'Container' && target.container.kind === 'menu');
    const start = item.target._tag === 'Container' || item.target._tag === 'Control' ? menuOf(item.target.container.ref) : Option.none<Item>();
    return Array.reverse(Array.unfold(start, (at) => Option.map(at, (held) => [held, held.target._tag === 'Container' ? Option.flatMap(Option.fromUndefinedOr(held.target.container.parent), menuOf) : Option.none<Item>()] as const)));
};
const accepts = (item: Item, { to, range }: { readonly to?: typeof Value.Type | typeof Frame.Type; readonly range?: unknown }) => {
    const kind = Option.getOrUndefined(item.kind);
    const options = Match.value(item.target).pipe(
        Match.tag('Control', ({ control }) =>
            Option.map(
                Option.fromUndefinedOr(control.options),
                Array.map(({ label }) => label),
            ),
        ),
        Match.tag('RememberedControl', ({ control }) => Option.filter(control.options, (listed): listed is readonly string[] => globalThis.Array.isArray(listed))),
        Match.orElse(() => Option.none<readonly string[]>()),
    );
    const taken = Match.value({ kind, to }).pipe(
        Match.when({ to: Match.undefined }, () => Option.some(range === undefined || kind === 'text')),
        Match.when({ to: Predicate.isTupleOf(2) }, () => Option.some(item.target._tag === 'Container' || item.target._tag === 'RememberedContainer')),
        Match.when({ kind: Match.undefined }, () => Option.some(false)),
        Match.when({ kind: Match.is('toggle', 'row') }, ({ to: value }) => Option.some(Predicate.isBoolean(value))),
        Match.when({ kind: 'number' }, ({ to: value }) => Option.some(Predicate.isNumber(value))),
        Match.when({ kind: Match.is('measure', 'text') }, ({ to: value }) => Option.some(Predicate.isNumber(value) || Predicate.isString(value))),
        Match.when({ kind: 'date' }, ({ to: value }) => Option.some(Predicate.isString(value) && Option.isSome(DateTime.make(value)))),
        Match.when({ kind: Match.is('choice', 'multi') }, ({ to: value }) => Option.some(Predicate.isString(value) && Option.match(options, { onNone: Function.constTrue, onSome: (listed) => listed.includes(value) }))),
        Match.when({ kind: 'color' }, ({ to: value }) => Option.some(Predicate.isString(value))),
        Match.orElse(() => Option.some(false)),
    );
    const name = [...Option.toArray(item.token), ...Option.toArray(item.name)].join(' ');
    const takes = Option.match(item.kind, {
        onNone: () => 'a frame [[x, y], [width, height]] or a press',
        onSome: (held) => `${held === 'action' || held === 'item' || held === 'region' ? 'a press alone' : `what a ${held} takes${Option.match(options, { onNone: () => '', onSome: (listed) => ` (${listed.map((label) => JSON.stringify(label)).join(', ')})` })}`}${held === 'text' ? ' or a range' : ''}`,
    });
    return Option.filter(Option.some(`${name} takes ${takes}, not ${JSON.stringify({ to, range })}`), () => !Option.getOrElse(taken, Function.constFalse));
};
const itemOf = (base: Index, map: InterfaceMap) => (at: string) => Effect.fromResult(Result.orElse(Address.resolve(base, at), () => Address.resolve(Address.recall(base, map), at)));
const printEntry = <A extends Readonly<Record<string, unknown>>>(record: A, lead: readonly (keyof A & string)[]) => [...lead.map((key) => record[key]).filter(Predicate.isString), ...Record.collect(Struct.omit(record, lead), (key, value) => `${key}=${JSON.stringify(value)}`)].join(' ').replaceAll('\n', '\n    ');
const runScript = Effect.fnUntraced(function* ({ row, live }: Parameters<Scripting<never, never>>[0], code: string) {
    const server = yield* ApplicationServer;
    const { script, command } = yield* server.dictionary({ url: row.url });
    yield* Effect.fromOption(script, () => Unmatched.of({ subject: `script command in the dictionary of ${row.name}`, candidates: command.map(Struct.get('name')) }));
    const { reply, written } = yield* server.execute({ instance: live.instance, arguments: { direct: code } });
    return { result: Option.some(yield* Schema.encodeEffect(Reply)(reply)), written };
});

// --- [COMPOSITION] ---------------------------------------------------------------------

const register = Effect.fnUntraced(function* <E1 extends Error = never, R1 = never, E2 extends Error = never, R2 = never, E3 extends Error = never, R3 = never, E4 extends Error = never, R4 = never, E5 extends Error = never, R5 = never>({
    descriptions = {},
    scripts,
    render,
    logs,
    sourceOf,
    toolsOf = () => Effect.succeed({ tools: HashMap.empty(), toolbar: [] }),
}: {
    readonly descriptions?: Readonly<Partial<Record<keyof typeof TOOLS, string>>>;
    readonly scripts?: Scripting<E1, R1>;
    readonly render?: (row: Entry, id: string) => Effect.Effect<{ readonly image: Uint8Array; readonly title: string }, E2, R2>;
    readonly logs?: (row: Entry) => Effect.Effect<Diagnostics['folders'], E3, R3>;
    readonly sourceOf?: (row: Entry) => Effect.Effect<Linked, E4, R4>;
    readonly toolsOf?: (row: Entry) => Effect.Effect<Declaration, E5, R5>;
} = {}) {
    const [server, workspace, maps, side, output] = yield* Effect.all([ApplicationServer, Workspace, InterfaceMaps, Side, Output]);
    const { catalog, running, changes, session, pressure, target, setTarget, resolve, current, ends } = workspace;
    const routed = yield* Ref.make(Record.empty<string, Record.ReadonlyRecord<string, Description['containers'][number]['locator']>>());
    const linked = (row: Entry) => Effect.flatMap(Effect.fromNullishOr(sourceOf), (from) => from(row));
    const routeOf = (instance: InstanceSnapshot['instance']) => Effect.map(Ref.get(routed), (held) => (ref: number) => Option.flatMap(Record.get(held, instance), Record.get(`${ref}`)));
    const intern = (instance: InstanceSnapshot['instance'], locator: string) => server.intern({ instance, locators: [locator] }).pipe(Effect.map(Array.head), Effect.flatMap(Effect.fromOption));
    const refOf = (instance: InstanceSnapshot['instance']) => (item: Item) =>
        Address.Target.$match(item.target, {
            Container: ({ container }) => Effect.succeed(container.ref),
            Control: ({ control }) => Effect.succeed(control.ref),
            Option: ({ option }) => Effect.succeed(option.ref),
            RememberedContainer: ({ locator }) => intern(instance, locator),
            RememberedControl: ({ locator }) => intern(instance, locator),
        });
    const prepared = (instance: InstanceSnapshot['instance'], base: Index, map: InterfaceMap, route: Effect.Success<ReturnType<typeof routeOf>>) =>
        Effect.fnUntraced(function* (step: Tool.Parameters<typeof TOOLS.act>['steps'][number]) {
            const reach = (at: string) => Effect.flatMap(itemOf(base, map)(at), refOf(instance));
            const viewed = Effect.map(
                Effect.flatMap(Handle, (handle) => handle.get(Viewed)),
                Option.filter((placement: Placement) => placement.instance === instance),
            );
            const aim = (at: string | Sample) => (Predicate.isString(at) ? Effect.map(reach(at), (ref): number | Sample => ref) : Effect.flatMap(viewed, (held) => Effect.fromResult(pointOf(held, at))));
            const plain = { at: Option.none<Option.Option.Value<ReturnType<typeof Address.locate>>>(), start: 'accessibility' as typeof Act.Rung.Type, opener: Option.none<number>(), openers: [], facet: Option.none<{ readonly locator: Description['containers'][number]['locator']; readonly to?: typeof Value.Type }>() };
            if ('pointer' in step) {
                const [pointer, path, held] = yield* Effect.all([aim(step.pointer), Effect.forEach(step.path ?? [], (point) => aim(point)), viewed]);
                const pixels = [step.pointer, ...(step.path ?? [])].some((at) => !Predicate.isString(at));
                return { ...plain, step: { ...step, pointer, path, ...(pixels ? Option.getOrUndefined(Option.map(held, ({ windows }) => ({ windows }))) : {}) } };
            }
            if ('key' in step) {
                const held = yield* Effect.transposeOption(Option.map(Option.fromUndefinedOr(step.at), reach));
                return { ...plain, step: { ...Struct.omit(step, ['at']), ...Option.getOrUndefined(Option.map(held, (ref) => ({ at: ref }))) } };
            }
            const item = yield* itemOf(base, map)(step.at);
            const refusal = accepts(item, step);
            if (Option.isSome(refusal)) {
                return yield* new Cause.IllegalArgumentError(refusal.value);
            }
            const opening = item.live ? [] : Address.opening(Address.recall(base, map), item);
            const [ref, refs] = yield* Effect.all([refOf(instance)(item), Effect.forEach(opening, refOf(instance))]);
            const located = Address.locate(item);
            const from = Option.flatMap(located, map.route.bind(map));
            const to = Option.getOrUndefined(Option.map(Option.filter(Option.fromUndefinedOr(step.to), Schema.is(Value)), (value) => ({ to: value })));
            return {
                ...plain,
                at: located,
                start: Option.getOrElse(from, () => plain.start),
                openers: Array.zipWith(opening, refs, (opener, at) => ({ ...plain, opener: Option.some(at), at: Address.locate(opener), step: { at } })),
                facet: Option.map(route(ref), (locator) => ({ locator, ...to })),
                step: { ...step, at: ref, ...Option.getOrUndefined(Option.map(from, (rung) => ({ from: rung }))) },
            };
        });
    const naming = Effect.map(SubscriptionRef.get(catalog), (entries) => {
        const names = Record.fromIterableWith(entries, (row) => [row.url.href, row.name]);
        return (href: string) => Option.getOrElse(Record.get(names, href), () => href);
    });
    const handlersOf = (urls: readonly URL[]) => Effect.forEach(urls, (document) => Effect.map(server.handlers({ document }), ({ installations }) => installations.map(Struct.get('url')).map(Struct.get('href'))), { concurrency: 'unbounded' });
    const listed = Effect.fnUntraced(function* (name: string, listing: ReturnType<typeof listEntry>) {
        const row = yield* workspace.find(name);
        const [installations, dictionary] = yield* Effect.all([server.currentInstallations, Effect.option(Effect.tapError(server.dictionary({ url: row.url }), (cause) => Effect.logDebug('Scripting dictionary unread', cause)))]);
        const launch = yield* Schema.encodeEffect(ApplicationListing)({ installation: Array.findFirst(installations, (each) => each.url.href === row.url.href), open: Option.flatMap(dictionary, Struct.get('open')) });
        return [{ ...listing(row), launch }];
    });
    const list = Effect.fnUntraced(function* (given: string | undefined, documents: readonly string[] = []) {
        const [instances, entries, { held }, opening] = yield* Effect.all([SubscriptionRef.get(running), SubscriptionRef.get(catalog), target, handlersOf(documents.map(locate))]);
        const listing = listEntry(instances);
        const opens = Array.groupBy(Array.flatten(Array.zipWith(opening, documents, (hrefs, document) => Array.cartesian(hrefs, [document]))), Tuple.get(0));
        const installed = yield* UndefinedOr.match(given, {
            onDefined: (name) => listed(name, listing),
            onUndefined: () =>
                Effect.succeed(
                    Array.match(documents, {
                        onEmpty: () => entries.map(listing),
                        onNonEmpty: () =>
                            Record.values(
                                Record.intersection(
                                    opens,
                                    Record.fromIterableBy(entries, (row) => row.url.href),
                                    (pairs, row) => ({ ...listing(row), opens: pairs.map(Tuple.get(1)) }),
                                ),
                            ),
                    }),
                ),
        });
        return { installed, ...Option.getOrUndefined(Option.map(held, ({ row }) => ({ current: row.name }))) };
    });
    const open = Effect.fnUntraced(function* (given: string | undefined, { documents = [], wait, ...launch }: Omit<Tool.Parameters<typeof TOOLS.applications>, 'action' | 'application' | 'mode'>) {
        const urls = documents.map(locate);
        const handler = Effect.gen(function* () {
            const [opening, named] = yield* Effect.all([handlersOf(urls), naming]);
            const defaults = Array.dedupe(Array.getSomes(opening.map(Array.head)));
            const every = Array.reduce(Array.drop(opening, 1), Array.flatten(Array.take(opening, 1)), Array.intersection<string, string>);
            const shared = Array.match(every, { onEmpty: () => 'no application opens every one', onNonEmpty: (hrefs) => `${hrefs.map(named).join(', ')} open every one` });
            const refusal = Array.match(defaults, { onEmpty: () => 'open names an application from the list, or documents the system has an application for', onNonEmpty: (hrefs) => `${hrefs.map(named).join(', ')} open these documents by default and ${shared}, application names the one to open` });
            return Predicate.isTupleOf(defaults, 1) ? defaults[0] : yield* new Cause.IllegalArgumentError(refusal);
        });
        const [row, instances] = yield* Effect.all([Effect.flatMap(UndefinedOr.match(given, { onUndefined: () => handler, onDefined: Effect.succeed }), resolve), SubscriptionRef.get(running)]);
        const reused = Option.filter(Array.findFirst(instances, Installation.runs(row)), () => Array.isReadonlyArrayEmpty([...Struct.keys(launch), ...urls]) && wait === undefined);
        const { instance, created, state, opened } = yield* Option.match(reused, {
            onSome: ({ instance: targeted }) => Effect.succeed({ instance: targeted, created: false, state: Option.none(), opened: Option.none() }),
            onNone: () => server.runLaunch({ url: row.url, documents: urls, ...launch }, Option.map(Option.fromUndefinedOr(wait), Duration.seconds)),
        });
        yield* setTarget({ row, instance });
        return { application: row.name, instance, created, state: Option.filter(state, () => wait !== undefined), opened };
    });
    const close = Effect.fnUntraced(function* (given: string | undefined, { wait, ...quitting }: Pick<Tool.Parameters<typeof TOOLS.applications>, 'mode' | 'wait'>) {
        const [instances, named, { followed }, chosen] = yield* Effect.all([SubscriptionRef.get(running), naming, target, Effect.transposeOption(Option.map(Option.fromUndefinedOr(given), resolve))]);
        const quit = instances.filter((each) => Option.match(chosen, { onNone: () => HashSet.has(followed, each.instance) && !InstanceState.isAnyOf(['exited'])(each.state), onSome: (row) => Installation.runs(row)(each) }));
        const bound = UndefinedOr.match(wait, { onUndefined: () => Duration.infinity, onDefined: Duration.seconds });
        const states = yield* Effect.forEach(quit, (each) => Effect.timeoutOption(server.quit({ instance: each.instance, ...quitting }), bound), { concurrency: 'unbounded' });
        return { closed: Array.zipWith(quit, states, (each, state) => ({ application: named(each.url.href), instance: each.instance, state })) };
    });
    const facets = Effect.fnUntraced(function* ({ row, live, live: { instance } }: Effect.Success<typeof current>, from: Linked, model: Model, payload: { readonly within?: Description['containers'][number]['locator'] }) {
        const [described, tools] = yield* Effect.all([from('describe', payload), toolsOf(row)]);
        const { model: composed, routes } = yield* Description.compose(model, yield* maps.learn(live, Description.fold(described, tools)), (locators) => server.intern({ instance, locators }));
        yield* Ref.update(routed, (held) => Record.set(held, instance, { ...held[instance], ...routes }));
        return composed;
    });
    const closed = Effect.fnUntraced(function* (
        { live, base, read, sequence }: { readonly live: Effect.Success<typeof current>['live']; readonly base: Index; readonly read: (scope: typeof Scope.Type) => Effect.Effect<Model & { readonly recalled: boolean }, Effect.Error<ReturnType<typeof server.model>>>; readonly sequence: Model['sequence'] },
        ref: number,
    ) {
        const pressed = Option.getOrUndefined(Option.map(Record.get(base.live, `${ref}`), Struct.get('target')));
        const opener = pressed?._tag === 'Control' && Option.getOrUndefined(Record.get(base.live, `${pressed.control.opens}`))?.target._tag !== 'Container' ? Option.some(pressed.control.locator) : Option.none<string>();
        const recalled = Option.flatten(yield* Effect.transposeOption(Option.map(opener, (locator) => maps.level(live, locator, sequence))));
        return yield* Option.match(recalled, { onNone: () => read({ _tag: 'menu', ref }), onSome: (model) => Effect.succeed<Model & { readonly recalled: boolean }>({ ...model, recalled: true }) });
    });
    const modelled = Effect.fnUntraced(function* ({ row, live, live: { instance } }: Effect.Success<typeof current>) {
        const map = yield* maps.recall(live);
        const read = (scope: typeof Scope.Type) => Effect.map(server.model({ instance, scope }), (model): Model & { readonly recalled: boolean } => ({ ...model, recalled: false }));
        const [accessible, menus, client] = yield* Effect.all([server.model({ instance, scope: { _tag: 'application' }, ...Option.getOrUndefined(Option.map(map.known, (known) => ({ known }))) }), read({ _tag: 'menus' }), Effect.option(linked(row))], { concurrency: 'unbounded' });
        const application = yield* Option.match(client, { onNone: () => Effect.succeed(accessible), onSome: (from) => facets({ row, live }, from, accessible, {}) });
        const route = yield* routeOf(instance);
        const indexed = (containers: Model['containers']) => Address.index({ ...application, containers });
        const base = indexed(Array.unionWith(application.containers, menus.containers, (left, right) => left.ref === right.ref));
        const level = (scope: typeof Scope.Type) =>
            Option.match(Option.all({ from: client, within: Option.flatMap(Scope.matchOrElse(scope, { container: ({ ref }) => Option.some(ref), menu: ({ ref }) => Option.some(ref), rows: ({ ref }) => Option.some(ref) }, Option.none<number>), route) }), {
                onNone: () => Scope.matchOrElse(scope, { menu: ({ ref }) => closed({ live, base, read, sequence: application.sequence }, ref) }, read),
                onSome: ({ from, within }) => Effect.map(facets({ row, live }, from, { sequence: application.sequence, containers: [] }, { within }), (model): Model & { readonly recalled: boolean } => ({ ...model, recalled: false })),
            });
        return { row, live, application, map, indexed, level, base };
    });
    const clipboardLine = Effect.fnUntraced(function* (instance: InstanceSnapshot['instance']) {
        const [pasteboard, handle] = yield* Effect.all([server.pasteboard({}), Handle]);
        const ref = yield* intern(instance, pasteboard.locator);
        const line = Outline.clipboard(pasteboard, ref);
        const digest = Hash.string(line);
        return yield* Effect.when(
            Effect.as(handle.set(Clipboard, Option.some(digest)), line),
            Effect.map(handle.get(Clipboard), (shown) => !Option.contains(shown, digest)),
        );
    });
    const capture = (application: string, { instance, state }: InstanceSnapshot, source: Capture['source'], drawn: Drawn) => {
        const titles = Record.getSomes(Record.fromIterableWith(presentedWindows(state), (window) => [String(window.number), Option.fromUndefinedOr(window.title)]));
        const marks = Option.match(
            Option.filter(drawn, (held) => held.instance === instance),
            { onNone: Array.empty, onSome: Struct.get('marks') },
        );
        return Effect.map(
            server.capture({ instance, source, longestSide: Option.some(side), marks }),
            Array.map((outcome) =>
                Result.match(outcome, {
                    onFailure: ({ message }) => ({ line: `${application} · ${message}`, image: Option.none() }),
                    onSuccess: ({ png, width, height, frame, windows, backingStore, document }) => ({
                        line: [
                            application,
                            ...Array.match(windows, {
                                onEmpty: () => ['drawn from what was read, the application showing no window of it'],
                                onNonEmpty: ([number]) => [...Option.toArray(Option.map(Record.get(titles, String(number)), (title) => JSON.stringify(title))), `window ${number}`],
                            }),
                            ...(backingStore ? ['off screen, its last drawn content'] : []),
                            ...Array.fromNullishOr(document).map((url) => `document ${url}`),
                        ].join(' · '),
                        image: Option.some({ png, placement: Option.some<Placement>({ instance, windows, frame, size: [width, height] }) }),
                    }),
                }),
            ),
        );
    };
    const shown = Effect.fnUntraced(function* (keep: (window: Running['windows'][number]) => boolean, drawn: Drawn) {
        const [instances, named] = yield* Effect.all([SubscriptionRef.get(running), naming]);
        const windowed = instances.flatMap((each) => Array.match(presentedWindows(each.state).filter(keep).map(Struct.get('number')), { onEmpty: Array.empty, onNonEmpty: (numbers) => [{ each, numbers }] }));
        return Array.flatten(yield* Effect.forEach(windowed, ({ each, numbers }) => capture(named(each.url.href), each, { _tag: 'windows', numbers }, drawn), { concurrency: 'unbounded' }));
    });
    const front = Effect.fnUntraced(function* (drawn: Drawn) {
        const { row, live } = yield* current;
        const presented = presentedWindows(live.state);
        const { number } = yield* Effect.fromOption(
            Option.orElse(
                Array.findFirst(presented, (window) => window.asking || window.kind === 'window'),
                () => Array.head(presented),
            ),
            () => new Cause.NoSuchElementError(`${row.name} presents no window, applications open with documents shows one`),
        );
        return yield* capture(row.name, live, { _tag: 'windows', numbers: [number] }, drawn);
    });
    const zoom = Effect.fnUntraced(function* (pixels: typeof Pixels.Type, drawn: Drawn) {
        const [viewed, instances, named] = yield* Effect.all([Effect.flatMap(Handle, (handle) => handle.get(Viewed)), SubscriptionRef.get(running), naming]);
        const placement = yield* Effect.fromOption(viewed, () => new Cause.NoSuchElementError('view of a pixel rectangle reads the last image a view returned, and this handle has none placed on the screen'));
        const each = yield* Effect.fromOption(
            Array.findFirst(instances, (held) => held.instance === placement.instance),
            () => new Cause.NoSuchElementError(`instance ${placement.instance} of the last image a view returned runs no more`),
        );
        const frame = yield* Effect.fromResult(screenOf(placement, pixels));
        return yield* capture(named(each.url.href), each, { _tag: 'region', frame }, drawn);
    });
    const element = (reference: string, drawn: Drawn) =>
        Effect.fnUntraced(function* ({ row, live, map, indexed, level, base }: Effect.Success<ReturnType<typeof modelled>>) {
            const item = yield* itemOf(base, map)(reference);
            const ref = yield* refOf(live.instance)(item);
            const shots = yield* capture(row.name, live, { _tag: 'element', ref }, drawn);
            const opened = yield* Effect.when(level({ _tag: 'menu', ref }), Effect.succeed(item.target._tag === 'Control' && item.target.control.opens !== undefined));
            const containers = Option.match(opened, { onNone: Array.empty, onSome: Struct.get('containers') });
            const fresh = indexed(containers);
            const levels = [
                ...menuPath(base, item).map((each) => ({ index: base, item: each })),
                ...(item.target._tag === 'RememberedContainer' ? [{ index: Address.recall(base, map), item }] : []),
                ...Array.getSomes(containers.map((container) => Record.get(fresh.live, `${container.ref}`))).map((each) => ({ index: fresh, item: each })),
            ];
            const outline = yield* Effect.forEach(levels, ({ index, item: each }) => Effect.fromResult(Result.map(Result.all([Outline.hits([each], []), Outline.render(index, { in: Option.some(each) })]), Array.flatten)));
            return [...shots, ...outline.flat().map((line) => ({ line, image: Option.none() }))];
        });
    const document = (drawn: Drawn) => (id: string) => shown((window) => window.document === id, drawn);
    const object = Effect.fnUntraced(function* (id: string) {
        const { row } = yield* current;
        const renderer = yield* Effect.fromOption(Option.fromUndefinedOr(render), () => new Cause.NoSuchElementError(`No linked source renders objects of ${row.name}`));
        const { image, title } = yield* renderer(row, id);
        const { png } = yield* server.scale({ image, longestSide: side });
        return [{ line: [row.name, yield* Schema.encodeEffect(Title)(title)].join(' · '), image: Option.some({ png, placement: Option.none<Placement>() }) }];
    });
    const run: Scripting<Error, R1 | ApplicationServer> = scripts ?? runScript;
    yield* Server.registerTool(describeTool(TOOLS.applications, descriptions.applications), ({ action = 'list', application, ...given }) =>
        Match.value(action).pipe(
            Match.when('list', () => list(application, given.documents)),
            Match.when('open', () => open(application, Struct.omit(given, ['mode']))),
            Match.when('close', () => close(application, given)),
            Match.exhaustive,
        ),
    );
    yield* Server.registerTool(
        describeTool(TOOLS.view, descriptions.view),
        Effect.fnUntraced(function* ({ of = 'front', id, marks = false }) {
            const identified = (kind: 'document' | 'object') => Effect.fromOption(Option.fromUndefinedOr(id), () => new Cause.IllegalArgumentError(`view of ${kind} takes an id`));
            const reading = yield* Effect.cached(Effect.flatMap(current, modelled));
            const drawn: Drawn = marks ? Option.some(yield* Effect.map(reading, ({ live, base }) => ({ instance: live.instance, marks: marksOf(base) }))) : Option.none();
            const taken = yield* Match.value(of).pipe(
                Match.when('front', () => front(drawn)),
                Match.when('all', () => shown(Function.constTrue, drawn)),
                Match.when('document', () => Effect.flatMap(identified('document'), document(drawn))),
                Match.when('object', () => Effect.flatMap(identified('object'), object)),
                Match.when(Predicate.isString, (reference) => Effect.flatMap(reading, element(reference, drawn))),
                Match.orElse((pixels) => zoom(pixels, drawn)),
            );
            yield* Array.isReadonlyArrayEmpty(taken) ? new Cause.NoSuchElementError(`Nothing to view for ${[of, ...Array.fromNullishOr(id)].join(' ')}, view all shows every presented window`) : Effect.void;
            const [{ add }, handle] = yield* Effect.all([Server.ToolCall, Handle]);
            const images = Array.getSomes(taken.map(Struct.get('image')));
            yield* handle.set(Viewed, Option.flatMap(Array.last(images), Struct.get('placement')));
            yield* Effect.forEach(images, ({ png }) => add(McpSchema.ImageContent.make({ data: png, mimeType: 'image/png' })), { discard: true });
            return taken.map(Struct.get('line')).join('\n');
        }),
    );
    yield* Server.registerTool(
        describeTool(TOOLS.inspect, descriptions.inspect),
        Effect.fnUntraced(function* ({ in: within, find: query, kind, after, wait }) {
            const aimed = yield* current;
            const { row, live } = aimed;
            const searched = Record.collect(Record.getSomes({ find: Option.fromUndefinedOr(query), kind: Option.fromUndefinedOr(kind) }), Outline.quoted);
            const whole = within === undefined && Array.isReadonlyArrayEmpty(searched);
            const first = yield* UndefinedOr.match(wait, {
                onUndefined: () => Effect.succeedNone,
                onDefined: (seconds) =>
                    changes.pipe(
                        Stream.filter((event) => event.instance === live.instance),
                        Stream.runHead,
                        Effect.timeoutOption(Duration.seconds(seconds)),
                        Effect.map(Option.flatten),
                    ),
            });
            const changed = HashSet.fromIterable(Option.toArray(Option.flatMap(first, ({ change }) => (Event.fields.change.isAnyOf(['focused', 'value', 'selection'])(change) ? Option.some(change.control) : Option.none()))));
            const [{ application, map, indexed, level, base }, clipboard, stated, strained] = yield* Effect.all([modelled(aimed), whole ? clipboardLine(live.instance) : Effect.succeedNone, session, pressure], { concurrency: 'unbounded' });
            const recalled = () => Address.recall(base, map);
            const lookup = (at: string | undefined) =>
                Result.transposeMapOption(Option.fromUndefinedOr(at), (held) => {
                    const resolved = Address.resolve(base, held);
                    return Result.isSuccess(resolved) ? resolved : Address.resolve(recalled(), held);
                });
            const [scope, start] = yield* Effect.fromResult(Result.all([lookup(within), lookup(after)]));
            const reading = yield* Effect.fromResult(Option.match(scope, { onNone: () => Result.succeedNone, onSome: scopeOf(start) }));
            const fresh = yield* Effect.transposeOption(Option.map(reading, level));
            const remembered = [scope, start].some(Option.exists(Predicate.not(Struct.get('live'))));
            const index = Option.match(fresh, { onNone: () => (remembered ? recalled() : base), onSome: ({ containers }) => indexed(containers) });
            const rows = Option.exists(reading, Scope.isAnyOf(['rows']));
            const opened = pipe(
                Option.filter(fresh, () => Option.exists(reading, Scope.isAnyOf(['menu']))),
                Option.flatMap(({ containers }) => Array.head(containers)),
                Option.flatMap(({ ref }) => Record.get(index.live, `${ref}`)),
            );
            const unopened = Array.groupBy(
                Record.collect(
                    Record.filter(map.containers, (_, locator) => !Record.has(base.located, locator)),
                    (_, { container }): string => container.kind,
                ),
                Function.identity,
            );
            const lines = yield* Effect.fromResult(
                Array.match(searched, {
                    onEmpty: () => Outline.render(index, { in: Option.orElse(opened, () => scope), after: rows ? Option.none() : start, changed }),
                    onNonEmpty: (call) => {
                        const searching = recalled();
                        const hits = Address.find(searching, query ?? '', Option.fromUndefinedOr(kind));
                        return Result.map(Outline.hits(hits, call, { after: start, opening: Option.some(searching) }), Array.append(`searched: ${searching.shown.length} containers shown and ${searching.remembered.containers.length} remembered, ${hits.filter(Struct.get('live')).length} of ${hits.length} hits live`));
                    },
                }),
            );
            const listedRows = index.items.filter((item) => item.target._tag === 'Control' && item.target.control.kind === 'row');
            const more = Option.all({
                in: Option.flatMap(scope, Struct.get('token')),
                after: pipe(
                    Array.last(listedRows),
                    Option.filter(() => rows && listedRows.length >= Outline.rows),
                    Option.flatMap(Struct.get('token')),
                ),
            });
            const answer = Option.flatMap(
                Array.findFirst(index.shown, ({ container }) => container.asking),
                answerOf,
            );
            const status = InstanceState.matchOrElse(live.state, { running: (activity) => [`${InstanceState.Status.matchOrElse(activity.status, { unresponsive: ({ thread }) => `unresponsive, main thread ${thread}` }, ({ _tag }) => _tag)}${describeUsage(activity)}`, activity.active ? 'front' : 'behind'] }, Array.empty);
            const { tool, shortcut, help, holding = [], operations = [], cursor, draggingFrame } = application.pointer ?? {};
            const pointed = [
                ...Array.fromNullishOr(tool).map((named) => `tool ${named}`),
                ...Array.fromNullishOr(shortcut).map(Outline.shortcut),
                ...(Array.isReadonlyArrayNonEmpty(holding) ? [`holding ${holding.join(', ')}`] : []),
                ...(Array.isReadonlyArrayNonEmpty(operations) ? [`operations ${operations.join(' > ')}`] : []),
                ...Array.fromNullishOr(cursor).map((held) => Cursor.match(held, { system: ({ name }) => `cursor ${name}`, custom: ({ digest, size }) => `cursor custom ${digest.slice(0, 8)} ${size.join('×')}` })),
                ...(draggingFrame === undefined ? [] : ['dragging']),
            ];
            yield* Server.ToolCall.use((call) => call.state([[row.name, ...status, ...pointed, ...stated, ...strained].join(' · '), ...Array.fromNullishOr(help)].join('\n')));
            return [
                ...lines,
                ...(whole && !Record.isEmptyReadonlyRecord(unopened) ? [`▸ not open, known from the map: ${Record.collect(unopened, (each, all) => `${each} ×${all.length}`).join(', ')} · find searches them`] : []),
                ...(Option.exists(fresh, Struct.get('recalled')) ? [`▸ menu level from the map of this build, not read now; act opens it to reach an item`] : []),
                ...Option.toArray(clipboard),
                ...Option.toArray(Option.map(more, (next) => `more rows may follow · ${Outline.call([Outline.quoted('in', next.in), Outline.quoted('after', next.after)])}`)),
                ...Option.toArray(Option.map(answer, ({ asking, ref }) => `> act {steps:[{at:"${Address.token('action', ref)}"}]} answers ${asking}`)),
            ].join('\n');
        }),
    );
    yield* Server.registerTool(
        describeTool(TOOLS.act, descriptions.act),
        Effect.fnUntraced(function* ({ steps }) {
            const aimed = yield* current;
            const { row, live } = aimed;
            const [{ base, map }, route] = yield* Effect.all([modelled(aimed), routeOf(live.instance)], { concurrency: 'unbounded' });
            const resolved = yield* Effect.forEach(steps, (step) => Effect.result(prepared(live.instance, base, map, route)(step)));
            const refusals = Array.getFailures(resolved).map(({ message }) => message);
            yield* Array.match(refusals, { onEmpty: () => Effect.void, onNonEmpty: (refused) => new Cause.IllegalArgumentError(`act refused every step before running one: ${refused.join('; ')}`) });
            const prepares = yield* Effect.forEach(resolved, Effect.fromResult);
            const ready = Array.dedupeWith(
                Array.flatMap(prepares, (entry) => Array.append(entry.openers, entry)),
                (first, second) => Option.isSome(first.opener) && Equal.equals(first.opener, second.opener),
            );
            const operated = Array.getSomes(ready.map(Struct.get('facet')));
            yield* operated.length === 0 || operated.length === ready.length ? Effect.void : new Cause.IllegalArgumentError(`act takes facet and Accessibility references in calls of their own, and its ${ready.length} steps name ${operated.length} facet references`);
            const client = yield* Effect.transposeOption(Option.as(Array.head(operated), linked(row)));
            if (Option.isSome(client)) {
                const outcomes = yield* Stream.fromIterable(operated).pipe(
                    Stream.mapEffect((payload) => Effect.result(client.value('operate', payload))),
                    Stream.takeUntil(Result.isFailure),
                    Stream.map(Result.map(() => ({ tried: [] }))),
                    Stream.runCollect,
                );
                yield* Server.ToolCall.use(Struct.get('awaitRunning'));
                return { outcomes, state: live.state };
            }
            const acted = yield* server.act({ instance: live.instance, steps: Array.map(ready, Struct.get('step')) });
            const proofs = Array.getSomes(Array.zipWith(ready, acted.outcomes, ({ at, start }, outcome) => Option.all({ at, from: Option.some(start), outcome: Option.some(outcome) })));
            yield* Effect.all(
                [
                    maps.prove(live, proofs),
                    SubscriptionRef.update(
                        running,
                        Array.map((each) => (each.instance === live.instance ? InstanceSnapshot.make({ ...each, state: acted.state }) : each)),
                    ),
                ],
                { discard: true },
            );
            yield* Server.ToolCall.use(Struct.get('awaitRunning'));
            return acted;
        }),
    );
    yield* Server.registerTool(describeTool(TOOLS.execute_script, descriptions.execute_script).annotateMerge(ends), ({ code }) => Effect.flatMap(current, (aim) => run(aim, code)));
    yield* Server.registerTool(
        describeTool(TOOLS.diagnostics, descriptions.diagnostics),
        Effect.fnUntraced(function* ({ application, ...query }) {
            const read = yield* UndefinedOr.match(application, {
                onUndefined: () => Effect.succeed({ folders: [{ url: pathToFileURL(`${output.logs}/`), glob: '*.log' }] }),
                onDefined: Effect.fnUntraced(function* (given: string) {
                    const [row, instances] = yield* Effect.all([resolve(given), SubscriptionRef.get(running)]);
                    const folders = yield* UndefinedOr.match(logs, { onUndefined: () => Effect.succeed([]), onDefined: (kept) => kept(row) });
                    return { application: { url: row.url, ...Option.getOrUndefined(Option.map(Array.findFirst(instances, Installation.runs(row)), Struct.pick(['instance']))) }, folders };
                }),
            });
            const found = yield* server.diagnostics({ ...query, ...read });
            const [, logged] = Array.mapAccum(found.logs, Option.none<string>(), (last, { file, text }) => [
                Option.some(file),
                [...(Option.contains(last, file) ? [] : [`log ${file}`]), `  ${Option.match(Schema.decodeOption(LogRecord)(text), { onNone: () => text, onSome: ({ attributes, ...record }) => printEntry({ ...attributes, ...record }, ['timestamp', 'severity_text', 'scope', 'body']) })}`],
            ]);
            const lines = [
                ...found.reports.map((report) => printEntry(report, ['kind', 'date'])),
                ...logged.flat(),
                ...Result.match(found.unified, {
                    onFailure: ({ message }) => [`unified log unread: ${message}`],
                    onSuccess: Array.match({
                        onEmpty: Array.empty,
                        onNonEmpty: (events) => ['unified log', ...events.map((event) => `  ${printEntry(event, ['timestamp', 'messageType', 'subsystem', 'category', 'eventMessage'])}`)],
                    }),
                }),
                ...found.unreadable.map((cause) => `unreadable: ${cause}`),
            ];
            return Array.match(lines, { onEmpty: () => 'no report, log line, or unified log event matched', onNonEmpty: (held) => held.join('\n') });
        }),
    );
});
const Tools = { instructions: INSTRUCTIONS, settings: [Side] as const, register };

// --- [EXPORTS] -------------------------------------------------------------------------

export { Tools };
